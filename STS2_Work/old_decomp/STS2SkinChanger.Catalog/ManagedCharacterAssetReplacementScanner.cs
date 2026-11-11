using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text;

namespace STS2SkinChanger.Catalog;

internal static class ManagedCharacterAssetReplacementScanner
{
	private sealed record ScannerCacheEntry(long Length, DateTime LastWriteTimeUtc, ManagedCharacterAssetReplacement? Replacement);

	private static readonly IReadOnlyDictionary<string, string[]> FrameworkAssetSetParameters = new Dictionary<string, string[]>(StringComparer.Ordinal)
	{
		["CharacterSceneAssetSet"] = new string[4] { "VisualsPath", "EnergyCounterPath", "MerchantAnimPath", "RestSiteAnimPath" },
		["CharacterUiAssetSet"] = new string[8] { "IconTexturePath", "IconOutlineTexturePath", "IconPath", "CharacterSelectBgPath", "CharacterSelectIconPath", "CharacterSelectLockedIconPath", "CharacterSelectTransitionPath", "MapMarkerPath" },
		["CharacterSpineAssetSet"] = new string[1] { "CombatSkeletonDataPath" },
		["CharacterAudioAssetSet"] = new string[5] { "CharacterSelectSfx", "CharacterTransitionSfx", "AttackSfx", "CastSfx", "DeathSfx" },
		["CharacterMultiplayerAssetSet"] = new string[4] { "ArmPointingTexturePath", "ArmRockTexturePath", "ArmPaperTexturePath", "ArmScissorsTexturePath" }
	};

	private static readonly string[] CharacterRegistrationMethods = new string[2] { "CharacterAssetReplacement", "RegisterCharacterAssetReplacement" };

	private static readonly object CacheSync = new object();

	private static readonly Dictionary<string, ScannerCacheEntry> Cache = new Dictionary<string, ScannerCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

	public static IReadOnlyList<ManagedCharacterAssetReplacement> Scan(string? providerRoot, string providerId)
	{
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return Array.Empty<ManagedCharacterAssetReplacement>();
		}
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		object obj = ((!File.Exists(text)) ? ((object)Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((object)new string[1] { text }));
		List<ManagedCharacterAssetReplacement> list = new List<ManagedCharacterAssetReplacement>();
		string[] array = (string[])obj;
		foreach (string assemblyPath in array)
		{
			try
			{
				ManagedCharacterAssetReplacement managedCharacterAssetReplacement = ScanAssemblyCached(assemblyPath);
				if (managedCharacterAssetReplacement != null)
				{
					list.Add(managedCharacterAssetReplacement);
				}
			}
			catch (Exception)
			{
			}
		}
		return list;
	}

	private static ManagedCharacterAssetReplacement? ScanAssemblyCached(string assemblyPath)
	{
		FileInfo fileInfo = new FileInfo(assemblyPath);
		lock (CacheSync)
		{
			if (Cache.TryGetValue(assemblyPath, out ScannerCacheEntry value) && value.Length == fileInfo.Length && value.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
			{
				return value.Replacement;
			}
		}
		ManagedCharacterAssetReplacement managedCharacterAssetReplacement = ScanAssembly(assemblyPath);
		lock (CacheSync)
		{
			Cache[assemblyPath] = new ScannerCacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, managedCharacterAssetReplacement);
			return managedCharacterAssetReplacement;
		}
	}

	private static ManagedCharacterAssetReplacement? ScanAssembly(string assemblyPath)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return null;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (TypeDefinitionHandle typeDefinition in metadataReader.TypeDefinitions)
		{
			foreach (MethodDefinitionHandle method in metadataReader.GetTypeDefinition(typeDefinition).GetMethods())
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(method);
				if (methodDefinition.RelativeVirtualAddress != 0)
				{
					byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
					if (iLBytes != null)
					{
						ScanIl(metadataReader, iLBytes, hashSet, dictionary);
					}
				}
			}
		}
		if (hashSet.Count != 1 || dictionary.Count == 0)
		{
			return ScanButtonIcon(assemblyPath);
		}
		string targetGroupId = NormalizeToken(hashSet.Single());
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			string canonicalPath = GetCanonicalPath(item.Key, targetGroupId);
			if (canonicalPath != null)
			{
				dictionary2[NormalizeResourcePath(item.Value)] = canonicalPath;
			}
		}
		return (dictionary2.Count == 0) ? null : new ManagedCharacterAssetReplacement(targetGroupId, dictionary2);
	}

	internal static ManagedCharacterAssetReplacement? ScanButtonIcon(string assemblyPath)
	{
		using FileStream peStream = File.OpenRead(assemblyPath);
		PEReader pe = new PEReader(peStream);
		try
		{
			if (!pe.HasMetadata)
			{
				return null;
			}
			MetadataReader reader = pe.GetMetadataReader();
			Dictionary<int, string> fields = new Dictionary<int, string>();
			Dictionary<MethodDefinitionHandle, List<(OpCode, int, string)>> dictionary = (from handle in reader.TypeDefinitions.SelectMany((TypeDefinitionHandle type) => reader.GetTypeDefinition(type).GetMethods())
				where reader.GetMethodDefinition(handle).RelativeVirtualAddress != 0
				select handle).ToDictionary((MethodDefinitionHandle handle) => handle, (MethodDefinitionHandle handle) => ReadIconInstructions(reader, pe, handle));
			MethodDefinitionHandle key;
			List<(OpCode, int, string)> value;
			foreach (KeyValuePair<MethodDefinitionHandle, List<(OpCode, int, string)>> item5 in dictionary)
			{
				item5.Deconstruct(out key, out value);
				MethodDefinitionHandle handle2 = key;
				List<(OpCode, int, string)> list = value;
				if (reader.GetString(reader.GetMethodDefinition(handle2).Name) != ".cctor")
				{
					continue;
				}
				string text = null;
				bool flag = false;
				foreach (var item6 in list)
				{
					if (item6.Item1 == OpCodes.Ldstr)
					{
						text = (IsIconImage(item6.Item3) ? item6.Item3 : null);
						flag = false;
					}
					if (item6.Item1 == OpCodes.Call && item6.Item3 == "Load")
					{
						flag = text != null;
					}
					if (item6.Item1 == OpCodes.Stsfld)
					{
						if (flag && text != null)
						{
							fields[item6.Item2] = text;
						}
						text = null;
						flag = false;
					}
				}
			}
			List<(string, string)> list2 = new List<(string, string)>();
			foreach (KeyValuePair<MethodDefinitionHandle, List<(OpCode, int, string)>> item7 in dictionary)
			{
				item7.Deconstruct(out key, out value);
				MethodDefinitionHandle handle3 = key;
				List<(OpCode, int, string)> list3 = value;
				MethodDefinition methodDefinition = reader.GetMethodDefinition(handle3);
				if (reader.GetString(methodDefinition.Name) != "Postfix" || !reader.GetTypeDefinition(methodDefinition.GetDeclaringType()).GetCustomAttributes().Concat(methodDefinition.GetCustomAttributes())
					.Any(delegate(CustomAttributeHandle handle)
					{
						CustomAttribute customAttribute = reader.GetCustomAttribute(handle);
						string @string = Encoding.UTF8.GetString(reader.GetBlobBytes(customAttribute.Value));
						return @string.Contains("MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectButton,", StringComparison.Ordinal) && @string.Contains("\u0004Init", StringComparison.Ordinal);
					}) || !list3.Any<(OpCode, int, string)>(delegate((OpCode Op, int Token, string Text) i)
				{
					string item = i.Text;
					return (item == "_icon" || item == "%Icon") ? true : false;
				}) || !list3.Any<(OpCode, int, string)>(delegate((OpCode Op, int Token, string Text) i)
				{
					string item2 = i.Text;
					return (item2 == "set_Texture" || item2 == "SetValue") ? true : false;
				}))
				{
					continue;
				}
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				for (int j = 0; j < list3.Count; j++)
				{
					(OpCode, int, string) tuple = list3[j];
					if (tuple.Item1 == OpCodes.Ldstr && IsPlausibleCharacterEntry(tuple.Item3) && list3.Skip(j + 1).Take(3).Any<(OpCode, int, string)>(delegate((OpCode Op, int Token, string Text) next)
					{
						bool flag2 = next.Op == OpCodes.Call || next.Op == OpCodes.Callvirt;
						if (flag2)
						{
							string item3 = next.Text;
							bool flag3 = ((item3 == "Equals" || item3 == "op_Equality") ? true : false);
							flag2 = flag3;
						}
						return flag2;
					}))
					{
						hashSet.Add(NormalizeToken(tuple.Item3));
					}
				}
				string[] array = (from i in list3
					where i.Op == OpCodes.Ldsfld && fields.ContainsKey(i.Token)
					select fields[i.Token]).Concat(from i in list3
					where i.Op == OpCodes.Ldstr && IsIconImage(i.Text)
					select i.Text).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
				if (hashSet.Count == 1 && array.Length == 1)
				{
					list2.Add((hashSet.Single(), array[0]));
				}
			}
			if (list2.Count == 0 || list2.Distinct().Count() != 1)
			{
				return null;
			}
			(string, string) tuple2 = list2[0];
			return new ManagedCharacterAssetReplacement(tuple2.Item1, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [tuple2.Item2] = GetCanonicalPath("CharacterSelectIconPath", tuple2.Item1) });
		}
		finally
		{
			if (pe != null)
			{
				((IDisposable)pe).Dispose();
			}
		}
	}

	private static bool IsIconImage(string? path)
	{
		string? text = path;
		if (text != null && text.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return new string[4] { ".png", ".jpg", ".webp", ".ctex" }.Any((string ext) => path.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}

	private static List<(OpCode Op, int Token, string? Text)> ReadIconInstructions(MetadataReader reader, PEReader pe, MethodDefinitionHandle method)
	{
		byte[] iLBytes = pe.GetMethodBody(reader.GetMethodDefinition(method).RelativeVirtualAddress).GetILBytes();
		List<(OpCode, int, string)> list = new List<(OpCode, int, string)>();
		int operandSize;
		for (int i = 0; i < iLBytes.Length; i += operandSize)
		{
			byte b = iLBytes[i++];
			ushort key = ((b == 254 && i < iLBytes.Length) ? ((ushort)(0xFE00 | iLBytes[i++])) : b);
			if (!OpCodesByValue.TryGetValue(key, out var value))
			{
				break;
			}
			operandSize = GetOperandSize(value.OperandType, iLBytes, i);
			if (operandSize < 0 || i + operandSize > iLBytes.Length)
			{
				break;
			}
			int num = ((operandSize == 4) ? BinaryPrimitives.ReadInt32LittleEndian(iLBytes.AsSpan(i, 4)) : 0);
			string item = ((value.OperandType == OperandType.InlineString) ? reader.GetUserString(MetadataTokens.UserStringHandle(num & 0xFFFFFF)) : ((value.OperandType == OperandType.InlineMethod) ? ResolveMethodName(reader, MetadataTokens.EntityHandle(num)) : null));
			list.Add((value, num, item));
		}
		return list;
	}

	private static void ScanIl(MetadataReader reader, byte[] il, ISet<string> characterEntries, IDictionary<string, string> providerPathsByProperty)
	{
		string text = null;
		string value = null;
		List<string> list = new List<string>();
		int operandSize;
		for (int i = 0; i < il.Length; i += operandSize)
		{
			byte b = il[i++];
			ushort key = ((b == 254 && i < il.Length) ? ((ushort)(0xFE00 | il[i++])) : b);
			if (!OpCodesByValue.TryGetValue(key, out var value2))
			{
				break;
			}
			int num = i;
			operandSize = GetOperandSize(value2.OperandType, il, num);
			if (operandSize < 0 || num + operandSize > il.Length)
			{
				break;
			}
			if (value2.OperandType == OperandType.InlineString && operandSize == 4)
			{
				int num2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
				try
				{
					text = reader.GetUserString(MetadataTokens.UserStringHandle(num2 & 0xFFFFFF));
					value = (text.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ? NormalizeResourcePath(text) : null);
				}
				catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
				{
					text = null;
					value = null;
				}
				list.Add(text);
			}
			else if (value2.Equals(OpCodes.Ldnull))
			{
				list.Add(null);
			}
			else if (value2.Equals(OpCodes.Newobj) && value2.OperandType == OperandType.InlineMethod && operandSize == 4)
			{
				int token = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
				ReadFrameworkAssetSet(reader, MetadataTokens.EntityHandle(token), list, providerPathsByProperty);
				list.Clear();
			}
			else if ((value2.Equals(OpCodes.Call) || value2.Equals(OpCodes.Callvirt)) && value2.OperandType == OperandType.InlineMethod && operandSize == 4)
			{
				int token2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
				string text2 = ResolveMethodName(reader, MetadataTokens.EntityHandle(token2));
				if (text2 != null)
				{
					if (text2.Equals("GetScenePath", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(text))
					{
						value = BuildScenePath(text);
					}
					else if (text2.Equals("GetImagePath", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(text))
					{
						value = BuildImagePath(text);
					}
					else if (text2.StartsWith("set_", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(value))
					{
						string text3 = text2;
						providerPathsByProperty[text3.Substring(4, text3.Length - 4)] = value;
						value = null;
					}
					else if (CharacterRegistrationMethods.Contains<string>(text2, StringComparer.Ordinal) && IsPlausibleCharacterEntry(text))
					{
						characterEntries.Add(text);
					}
				}
				list.Clear();
			}
			else
			{
				list.Clear();
			}
		}
	}

	private static void ReadFrameworkAssetSet(MetadataReader reader, EntityHandle constructorHandle, IReadOnlyList<string?> pushedArguments, IDictionary<string, string> providerPathsByProperty)
	{
		if (pushedArguments.Count == 0)
		{
			return;
		}
		string text = ResolveDeclaringTypeName(reader, constructorHandle);
		if (text == null || !FrameworkAssetSetParameters.TryGetValue(text, out string[] value) || value.Length != pushedArguments.Count)
		{
			return;
		}
		for (int i = 0; i < value.Length; i++)
		{
			string text2 = pushedArguments[i];
			if (text2 != null && text2.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				providerPathsByProperty[value[i]] = NormalizeResourcePath(text2);
			}
		}
	}

	private static string BuildScenePath(string value)
	{
		if (value.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return NormalizeResourcePath(value);
		}
		string text = value.Trim().TrimStart('/');
		if (!text.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
		{
			text += ".tscn";
		}
		return NormalizeResourcePath("res://scenes/" + text);
	}

	private static string BuildImagePath(string value)
	{
		if (value.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return NormalizeResourcePath(value);
		}
		return NormalizeResourcePath("res://images/" + value.Trim().TrimStart('/'));
	}

	private static string? GetCanonicalPath(string propertyName, string targetGroupId)
	{
		return propertyName switch
		{
			"VisualsPath" => "res://scenes/creature_visuals/" + targetGroupId + ".tscn", 
			"EnergyCounterPath" => "res://scenes/combat/energy_counters/" + targetGroupId + "_energy_counter.tscn", 
			"MerchantAnimPath" => "res://scenes/merchant/characters/" + targetGroupId + "_merchant.tscn", 
			"RestSiteAnimPath" => "res://scenes/rest_site/characters/" + targetGroupId + "_rest_site.tscn", 
			"IconTexturePath" => "res://images/ui/top_panel/character_icon_" + targetGroupId + ".png", 
			"IconOutlineTexturePath" => "res://images/ui/top_panel/character_icon_" + targetGroupId + "_outline.png", 
			"IconPath" => "res://scenes/ui/character_icons/" + targetGroupId + "_icon.tscn", 
			"CharacterSelectBgPath" => "res://scenes/screens/char_select/char_select_bg_" + targetGroupId + ".tscn", 
			"CharacterSelectIconPath" => "res://images/packed/character_select/char_select_" + targetGroupId + ".png", 
			"CharacterSelectLockedIconPath" => "res://images/packed/character_select/char_select_" + targetGroupId + "_locked.png", 
			"CharacterSelectTransitionPath" => "res://materials/transitions/" + targetGroupId + "_transition_mat.tres", 
			"MapMarkerPath" => "res://images/packed/map/icons/map_marker_" + targetGroupId + ".png", 
			"TrailPath" => "res://scenes/vfx/card_trail_" + targetGroupId + ".tscn", 
			"CombatSkeletonDataPath" => $"res://animations/characters/{targetGroupId}/{targetGroupId}_skel_data.tres", 
			"ArmPointingTexturePath" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_point.png", 
			"ArmRockTexturePath" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_rock.png", 
			"ArmPaperTexturePath" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_paper.png", 
			"ArmScissorsTexturePath" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_scissors.png", 
			_ => null, 
		};
	}

	private static string? ResolveMethodName(MetadataReader reader, EntityHandle handle)
	{
		return handle.Kind switch
		{
			HandleKind.MethodDefinition => reader.GetString(reader.GetMethodDefinition((MethodDefinitionHandle)handle).Name), 
			HandleKind.MemberReference => reader.GetString(reader.GetMemberReference((MemberReferenceHandle)handle).Name), 
			HandleKind.MethodSpecification => ResolveMethodName(reader, reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method), 
			_ => null, 
		};
	}

	private static string? ResolveDeclaringTypeName(MetadataReader reader, EntityHandle handle)
	{
		if (handle.Kind == HandleKind.MethodSpecification)
		{
			handle = reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method;
		}
		EntityHandle entityHandle = handle.Kind switch
		{
			HandleKind.MethodDefinition => reader.GetMethodDefinition((MethodDefinitionHandle)handle).GetDeclaringType(), 
			HandleKind.MemberReference => reader.GetMemberReference((MemberReferenceHandle)handle).Parent, 
			_ => default(EntityHandle), 
		};
		return entityHandle.Kind switch
		{
			HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)entityHandle).Name), 
			HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)entityHandle).Name), 
			_ => null, 
		};
	}

	private static bool IsPlausibleCharacterEntry(string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			int length = value.Length;
			if (length >= 3 && length <= 64 && !value.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				return value.All(delegate(char character)
				{
					bool flag = char.IsLetterOrDigit(character);
					if (!flag)
					{
						bool flag2 = ((character == '-' || character == '.' || character == '_') ? true : false);
						flag = flag2;
					}
					return flag;
				});
			}
		}
		return false;
	}

	private static string NormalizeToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static string NormalizeResourcePath(string value)
	{
		return value.Replace('\\', '/');
	}

	private static int GetOperandSize(OperandType operandType, byte[] il, int offset)
	{
		switch (operandType)
		{
		case OperandType.InlineNone:
			return 0;
		case OperandType.ShortInlineBrTarget:
		case OperandType.ShortInlineI:
		case OperandType.ShortInlineVar:
			return 1;
		case OperandType.InlineVar:
			return 2;
		case OperandType.InlineBrTarget:
		case OperandType.InlineField:
		case OperandType.InlineI:
		case OperandType.InlineMethod:
		case OperandType.InlineSig:
		case OperandType.InlineString:
		case OperandType.InlineTok:
		case OperandType.InlineType:
		case OperandType.ShortInlineR:
			return 4;
		case OperandType.InlineI8:
		case OperandType.InlineR:
			return 8;
		case OperandType.InlineSwitch:
			if (offset + 4 <= il.Length)
			{
				return 4 + checked(BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(offset, 4)) * 4);
			}
			break;
		}
		return -1;
	}
}

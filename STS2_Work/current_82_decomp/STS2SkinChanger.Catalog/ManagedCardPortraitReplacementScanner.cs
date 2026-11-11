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

internal static class ManagedCardPortraitReplacementScanner
{
	private sealed record ScannerCacheEntry(long Length, DateTime LastWriteTimeUtc, IReadOnlyDictionary<string, string> Portraits);

	private static readonly object CacheSync = new object();

	private static readonly Dictionary<string, ScannerCacheEntry> Cache = new Dictionary<string, ScannerCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

	public static IReadOnlyDictionary<string, string> Scan(string? providerRoot, string providerId)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return dictionary;
		}
		foreach (string assemblyPath in GetAssemblyPaths(providerRoot, providerId))
		{
			try
			{
				foreach (KeyValuePair<string, string> item in ScanAssemblyCached(assemblyPath))
				{
					dictionary[item.Key] = item.Value;
				}
			}
			catch (Exception)
			{
			}
		}
		return dictionary;
	}

	internal static IReadOnlyList<string> GetAssemblyPaths(string providerRoot, string providerId)
	{
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		if (File.Exists(text))
		{
			return new _003C_003Ez__ReadOnlySingleElementList<string>(text);
		}
		string[] array = Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray();
		string[] array2 = Directory.EnumerateFiles(providerRoot, "*.pck", SearchOption.TopDirectoryOnly).ToArray();
		if (array.Length != 1 || array2.Length > 1)
		{
			return Array.Empty<string>();
		}
		return array;
	}

	private static IReadOnlyDictionary<string, string> ScanAssemblyCached(string assemblyPath)
	{
		FileInfo fileInfo = new FileInfo(assemblyPath);
		lock (CacheSync)
		{
			if (Cache.TryGetValue(assemblyPath, out ScannerCacheEntry value) && value.Length == fileInfo.Length && value.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
			{
				return value.Portraits;
			}
		}
		IReadOnlyDictionary<string, string> readOnlyDictionary = ScanAssembly(assemblyPath);
		lock (CacheSync)
		{
			Cache[assemblyPath] = new ScannerCacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, readOnlyDictionary);
			return readOnlyDictionary;
		}
	}

	private static IReadOnlyDictionary<string, string> ScanAssembly(string assemblyPath)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (!pEReader.HasMetadata)
		{
			return dictionary;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, string> dictionary3 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			bool flag = metadataReader.GetString(typeDefinition.Name).Contains("Portrait", StringComparison.OrdinalIgnoreCase) || HasPortraitPatchAttribute(metadataReader, typeDefinition.GetCustomAttributes());
			bool hasRuntimeTypeLookup = false;
			bool hasDictionaryLookup = false;
			bool hasTypeNameLookup = false;
			foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(method);
				flag |= metadataReader.GetString(methodDefinition.Name).Contains("Portrait", StringComparison.OrdinalIgnoreCase) || HasPortraitPatchAttribute(metadataReader, methodDefinition.GetCustomAttributes());
				if (methodDefinition.RelativeVirtualAddress != 0)
				{
					byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
					if (iLBytes != null)
					{
						ScanIl(metadataReader, iLBytes, dictionary2, dictionary3, ref hasRuntimeTypeLookup, ref hasDictionaryLookup, ref hasTypeNameLookup);
					}
				}
			}
			if (!flag || !hasRuntimeTypeLookup || !hasDictionaryLookup)
			{
				continue;
			}
			foreach (KeyValuePair<string, string> item in dictionary2)
			{
				dictionary[item.Key] = item.Value;
			}
			if (!hasTypeNameLookup)
			{
				continue;
			}
			foreach (KeyValuePair<string, string> item2 in dictionary3)
			{
				string key = item2.Key;
				int num = Math.Max(item2.Key.LastIndexOf('.'), item2.Key.LastIndexOf('+')) + 1;
				dictionary[key.Substring(num, key.Length - num)] = item2.Value;
			}
		}
		return dictionary;
	}

	private static void ScanIl(MetadataReader reader, byte[] il, IDictionary<string, string> portraits, IDictionary<string, string> namedPortraits, ref bool hasRuntimeTypeLookup, ref bool hasDictionaryLookup, ref bool hasTypeNameLookup)
	{
		string text = null;
		string text2 = null;
		int num = -1;
		int num2 = -1;
		string text3 = null;
		int num3 = -1;
		int num4 = -1;
		int num5 = 0;
		int num6 = 0;
		while (num6 < il.Length)
		{
			byte b = il[num6++];
			ushort key = ((b == 254 && num6 < il.Length) ? ((ushort)(0xFE00 | il[num6++])) : b);
			if (!OpCodesByValue.TryGetValue(key, out var value))
			{
				break;
			}
			int num7 = num6;
			int operandSize = GetOperandSize(value.OperandType, il, num7);
			if (operandSize < 0 || num7 + operandSize > il.Length)
			{
				break;
			}
			if ((value.Equals(OpCodes.Ldtoken) || value.OperandType == OperandType.InlineType) && operandSize == 4)
			{
				int token = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num7, 4));
				string text4 = ResolveTypeName(reader, MetadataTokens.EntityHandle(token));
				if (!string.IsNullOrWhiteSpace(text4))
				{
					text = text4;
					text2 = null;
					num = num5;
				}
			}
			else if (value.OperandType == OperandType.InlineString && operandSize == 4)
			{
				int num8 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num7, 4));
				try
				{
					string userString = reader.GetUserString(MetadataTokens.UserStringHandle(num8 & 0xFFFFFF));
					if (IsPortraitResource(userString))
					{
						text2 = NormalizeResourcePath(userString);
						num2 = num5;
					}
					else if (userString.Length > 0 && (char.IsLetter(userString[0]) || userString[0] == '_') && userString.All(delegate(char character)
					{
						bool flag = char.IsLetterOrDigit(character);
						if (!flag)
						{
							bool flag2 = ((character == '+' || character == '.' || character == '_') ? true : false);
							flag = flag2;
						}
						return flag;
					}))
					{
						text3 = userString;
						num3 = num5;
					}
					else
					{
						text3 = null;
					}
				}
				catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
				{
				}
			}
			else if ((value.Equals(OpCodes.Call) || value.Equals(OpCodes.Callvirt)) && value.OperandType == OperandType.InlineMethod && operandSize == 4)
			{
				int token2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num7, 4));
				string text5 = ResolveMethodName(reader, MetadataTokens.EntityHandle(token2));
				if (text5 != null)
				{
					hasRuntimeTypeLookup |= text5.Equals("GetType", StringComparison.Ordinal);
					if (text5 == "GetType")
					{
						num4 = num5;
					}
					bool flag3 = ((text5 == "get_Name" || text5 == "get_FullName") ? true : false);
					if (flag3 && num4 >= 0 && num5 - num4 <= 4 && IsTypeNameGetter(reader, MetadataTokens.EntityHandle(token2)))
					{
						hasTypeNameLookup = true;
					}
					hasDictionaryLookup |= text5.Equals("TryGetValue", StringComparison.Ordinal);
					switch (text5)
					{
					case "set_Item":
					case "Add":
					case "TryAdd":
						flag3 = true;
						break;
					default:
						flag3 = false;
						break;
					}
					if (flag3)
					{
						if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2) && num5 - num <= 24 && num5 - num2 <= 8)
						{
							portraits[text] = text2;
						}
						if (text3 != null && text2 != null && num2 == num3 + 1 && num5 == num2 + 1)
						{
							namedPortraits[text3] = text2;
						}
						text = null;
						text2 = null;
						text3 = null;
					}
				}
			}
			num6 += operandSize;
			num5++;
		}
	}

	private static bool IsTypeNameGetter(MetadataReader reader, EntityHandle handle)
	{
		if (handle.Kind != HandleKind.MemberReference)
		{
			return false;
		}
		EntityHandle parent = reader.GetMemberReference((MemberReferenceHandle)handle).Parent;
		if (parent.Kind != HandleKind.TypeReference)
		{
			return false;
		}
		TypeReference typeReference = reader.GetTypeReference((TypeReferenceHandle)parent);
		string @string = reader.GetString(typeReference.Namespace);
		string string2 = reader.GetString(typeReference.Name);
		if (!(@string == "System.Reflection"))
		{
			if (@string == "System" && string2 == "Type")
			{
				goto IL_0092;
			}
		}
		else if (string2 == "MemberInfo")
		{
			goto IL_0092;
		}
		return false;
		IL_0092:
		return true;
	}

	private static bool HasPortraitPatchAttribute(MetadataReader reader, CustomAttributeHandleCollection attributes)
	{
		foreach (CustomAttributeHandle item in attributes)
		{
			try
			{
				CustomAttribute customAttribute = reader.GetCustomAttribute(item);
				if (Encoding.UTF8.GetString(reader.GetBlobBytes(customAttribute.Value)).Contains("Portrait", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
			{
			}
		}
		return false;
	}

	private static string? ResolveTypeName(MetadataReader reader, EntityHandle handle)
	{
		return handle.Kind switch
		{
			HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name), 
			HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)handle).Name), 
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

	private static bool IsPortraitResource(string value)
	{
		if (!value.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string extension = Path.GetExtension(value);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".tres", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".res", StringComparison.OrdinalIgnoreCase);
		}
		return true;
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

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

namespace STS2SkinChanger.Catalog;

internal static class ManagedCardPresentationScanner
{
	private sealed record SurfaceInstruction(OpCode Op, int? Integer, float? Float, string? Member, int Offset, int? BranchTarget);

	private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

	private static readonly Dictionary<string, (long Length, DateTime Stamp, CardSurfaceDefinition? Surface)> SurfaceCache = new Dictionary<string, (long, DateTime, CardSurfaceDefinition)>(StringComparer.OrdinalIgnoreCase);

	public static IReadOnlyDictionary<string, CardPresentationDefinition> Scan(string? providerRoot, IReadOnlyCollection<string> knownCardStems, IReadOnlyCollection<string>? registryCardStems = null)
	{
		Dictionary<string, CardPresentationDefinition> dictionary = new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return dictionary;
		}
		Dictionary<string, string> dictionary2 = knownCardStems.Where((string stem) => !string.IsNullOrWhiteSpace(stem)).GroupBy<string, string>(NormalizeToken, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, string>, string, string>((IGrouping<string, string> group) => group.Key, (IGrouping<string, string> group) => group.First(), StringComparer.OrdinalIgnoreCase);
		if (dictionary2.Count == 0)
		{
			return dictionary;
		}
		Dictionary<string, string> registryCardStems2 = ((IEnumerable<string>)(((object)registryCardStems) ?? ((object)Array.Empty<string>()))).Where((string stem) => !string.IsNullOrWhiteSpace(stem)).GroupBy<string, string>(NormalizeToken, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, string>, string, string>((IGrouping<string, string> group) => group.Key, (IGrouping<string, string> group) => group.First(), StringComparer.OrdinalIgnoreCase);
		foreach (string item in Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.AllDirectories))
		{
			try
			{
				ScanAssembly(item, dictionary2, registryCardStems2, dictionary);
			}
			catch (Exception)
			{
			}
		}
		return dictionary;
	}

	private static void ScanAssembly(string dllPath, IReadOnlyDictionary<string, string> knownCardStems, IReadOnlyDictionary<string, string> registryCardStems, IDictionary<string, CardPresentationDefinition> presentations)
	{
		using FileStream peStream = new FileStream(dllPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
			string[] first = ReadBuiltInOverlayTargets(metadataReader, typeDefinition.GetCustomAttributes(), knownCardStems);
			HashSet<string> strings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(method);
				if (methodDefinition.RelativeVirtualAddress == 0)
				{
					continue;
				}
				byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
				if (iLBytes == null)
				{
					continue;
				}
				ScanIl(metadataReader, iLBytes, strings, hashSet, hashSet2);
				if (!IsConstantOverlayOffPostfix(metadataReader, methodDefinition, iLBytes))
				{
					continue;
				}
				foreach (string item in first.Concat(ReadBuiltInOverlayTargets(metadataReader, methodDefinition.GetCustomAttributes(), knownCardStems)))
				{
					CardPresentationDefinition value;
					CardPresentationDefinition cardPresentationDefinition = (presentations.TryGetValue(item, out value) ? value : new CardPresentationDefinition(UseAncientLayout: false, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, UseFullFrameArt: false, UseExpandedPortraitLayout: false, null, null, null, null, null, null, null, null, null));
					presentations[item] = cardPresentationDefinition with
					{
						BuiltInOverlayVisible = false
					};
				}
			}
			if (!LooksLikeAncientLayoutPatch(strings))
			{
				continue;
			}
			string[] array = (from typeName in hashSet
				select (TypeName: typeName, Token: NormalizeToken(typeName)) into candidate
				where MatchesKnownCardStem(candidate.Token, knownCardStems.Keys)
				select candidate.TypeName).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
			bool flag = false;
			if (array.Length == 0)
			{
				if (!ReferencesMember(hashSet2, "CardReplacementRegistry", "TryGetTexture"))
				{
					continue;
				}
				array = registryCardStems.Values.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
				if (array.Length == 0)
				{
					continue;
				}
				flag = true;
			}
			string frame = (flag ? null : FindResource(strings, "ancient_card_border"));
			string ancientTextBackground = (flag ? null : FindResource(strings, "ancient_card_text_bg"));
			string bannerTexture = (flag ? null : FindResource(strings, "ancient_banner.tres"));
			string bannerMaterial = (flag ? null : FindResource(strings, "card_banner_ancient_mat"));
			bool useAncientLayout = !flag;
			bool useExpandedPortraitLayout = flag;
			CardPresentationDefinition cardPresentationDefinition2 = new CardPresentationDefinition(useAncientLayout, frame, null, bannerTexture, bannerMaterial, null, null, ancientTextBackground, null, null, null, null, null, null, null, null, null, null, null, null, null, null, UseFullFrameArt: false, useExpandedPortraitLayout, null, null, null, null, null, null, null, null, null);
			string[] array2 = array;
			foreach (string key in array2)
			{
				if (presentations.TryGetValue(key, out CardPresentationDefinition value2))
				{
					if (!value2.UseAncientLayout && !value2.UseExpandedPortraitLayout)
					{
						presentations[key] = cardPresentationDefinition2 with
						{
							BuiltInOverlayVisible = value2.BuiltInOverlayVisible
						};
					}
				}
				else
				{
					presentations[key] = cardPresentationDefinition2;
				}
			}
		}
	}

	private static string[] ReadBuiltInOverlayTargets(MetadataReader reader, CustomAttributeHandleCollection attributes, IReadOnlyDictionary<string, string> knownCardStems)
	{
		List<string> list = new List<string>();
		foreach (CustomAttributeHandle item in attributes)
		{
			try
			{
				CustomAttribute customAttribute = reader.GetCustomAttribute(item);
				if (customAttribute.Constructor.Kind != HandleKind.MemberReference)
				{
					continue;
				}
				MemberReference memberReference = reader.GetMemberReference((MemberReferenceHandle)customAttribute.Constructor);
				if (memberReference.Parent.Kind != HandleKind.TypeReference)
				{
					continue;
				}
				TypeReference typeReference = reader.GetTypeReference((TypeReferenceHandle)memberReference.Parent);
				if (reader.GetString(typeReference.Namespace) != "HarmonyLib" || reader.GetString(typeReference.Name) != "HarmonyPatch")
				{
					continue;
				}
				BlobReader blobReader = reader.GetBlobReader(customAttribute.Value);
				if (blobReader.ReadUInt16() != 1)
				{
					continue;
				}
				string? text = blobReader.ReadSerializedString();
				string text2 = ((text != null) ? text.Split(',')[0] : null);
				string text3 = blobReader.ReadSerializedString();
				if (text2 != null && !(text3 != "HasBuiltInOverlay") && blobReader.ReadInt32() == 1 && blobReader.ReadUInt16() == 0 && blobReader.RemainingBytes == 0)
				{
					string text4 = text2;
					int num = text2.LastIndexOf('.') + 1;
					string value = text4.Substring(num, text4.Length - num);
					if (knownCardStems.TryGetValue(NormalizeToken(value), out string value2))
					{
						list.Add(value2);
					}
				}
			}
			catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
			{
			}
		}
		return list.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static bool IsConstantOverlayOffPostfix(MetadataReader reader, MethodDefinition method, byte[] il)
	{
		if (reader.GetString(method.Name) != "Postfix" || (method.Attributes & MethodAttributes.Static) == 0 || !reader.GetBlobBytes(method.Signature).AsSpan().SequenceEqual(new byte[5] { 0, 1, 1, 16, 2 }))
		{
			return false;
		}
		if (reader.GetString(method.GetParameters().Select(reader.GetParameter).SingleOrDefault((Parameter parameter) => parameter.SequenceNumber == 1)
			.Name) != "__result")
			{
				return false;
			}
			return il.Where((byte value) => value != 0).SequenceEqual(new byte[4] { 2, 22, 82, 42 });
		}

		private static bool LooksLikeAncientLayoutPatch(IReadOnlySet<string> strings)
		{
			string[] source = new string[5] { "AncientPortrait", "AncientBorder", "AncientTextBg", "AncientBanner", "AncientBorderGlassOverlay" };
			if (ContainsLayoutToken(strings, "AncientPortrait") && source.Count((string node) => ContainsLayoutToken(strings, node)) >= 3)
			{
				return new string[3] { "Portrait", "Frame", "TitleBanner" }.Any((string node) => ContainsLayoutToken(strings, node));
			}
			return false;
		}

		private static bool ContainsLayoutToken(IEnumerable<string> strings, string expected)
		{
			return strings.Any((string value) => NormalizeToken(value).Equals(NormalizeToken(expected), StringComparison.OrdinalIgnoreCase));
		}

		private static string? FindResource(IEnumerable<string> strings, string token)
		{
			return strings.FirstOrDefault((string value) => value.StartsWith("res://", StringComparison.OrdinalIgnoreCase) && value.Contains(token, StringComparison.OrdinalIgnoreCase));
		}

		private static void ScanIl(MetadataReader reader, byte[] il, ISet<string> strings, ISet<string> referencedTypes, ISet<string> referencedMembers)
		{
			int operandSize;
			for (int i = 0; i < il.Length; i += operandSize)
			{
				byte b = il[i++];
				ushort key = ((b == 254 && i < il.Length) ? ((ushort)(0xFE00 | il[i++])) : b);
				if (!OpCodesByValue.TryGetValue(key, out var value))
				{
					break;
				}
				int num = i;
				operandSize = GetOperandSize(value.OperandType, il, num);
				if (operandSize < 0 || num + operandSize > il.Length)
				{
					break;
				}
				if (value.OperandType == OperandType.InlineString && operandSize == 4)
				{
					int num2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
					try
					{
						strings.Add(reader.GetUserString(MetadataTokens.UserStringHandle(num2 & 0xFFFFFF)));
					}
					catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
					{
					}
					continue;
				}
				if (value.OperandType == OperandType.InlineType && operandSize == 4)
				{
					int token = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
					string text = ResolveTypeName(reader, MetadataTokens.EntityHandle(token));
					if (text != null)
					{
						referencedTypes.Add(text);
					}
					continue;
				}
				OperandType operandType = value.OperandType;
				bool flag = ((operandType == OperandType.InlineField || operandType == OperandType.InlineMethod) ? true : false);
				if (flag && operandSize == 4)
				{
					int token2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
					string text2 = ResolveMemberName(reader, MetadataTokens.EntityHandle(token2));
					if (text2 != null)
					{
						referencedMembers.Add(text2);
					}
				}
			}
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

		private static string? ResolveTypeName(MetadataReader reader, EntityHandle handle)
		{
			return handle.Kind switch
			{
				HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name), 
				HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)handle).Name), 
				_ => null, 
			};
		}

		private static string? ResolveMemberName(MetadataReader reader, EntityHandle handle)
		{
			switch (handle.Kind)
			{
			case HandleKind.MethodDefinition:
			{
				MethodDefinition methodDefinition = reader.GetMethodDefinition((MethodDefinitionHandle)handle);
				return reader.GetString(reader.GetTypeDefinition(methodDefinition.GetDeclaringType()).Name) + "." + reader.GetString(methodDefinition.Name);
			}
			case HandleKind.FieldDefinition:
			{
				FieldDefinition fieldDefinition = reader.GetFieldDefinition((FieldDefinitionHandle)handle);
				return reader.GetString(reader.GetTypeDefinition(fieldDefinition.GetDeclaringType()).Name) + "." + reader.GetString(fieldDefinition.Name);
			}
			case HandleKind.MemberReference:
			{
				MemberReference memberReference = reader.GetMemberReference((MemberReferenceHandle)handle);
				string text = ResolveTypeName(reader, memberReference.Parent);
				if (text != null)
				{
					return text + "." + reader.GetString(memberReference.Name);
				}
				return reader.GetString(memberReference.Name);
			}
			case HandleKind.MethodSpecification:
				return ResolveMemberName(reader, reader.GetMethodSpecification((MethodSpecificationHandle)handle).Method);
			default:
				return null;
			}
		}

		private static bool ReferencesMember(IEnumerable<string> members, string declaringType, string memberName)
		{
			string expected = NormalizeToken(declaringType + memberName);
			return members.Any((string member) => NormalizeToken(member).EndsWith(expected, StringComparison.OrdinalIgnoreCase));
		}

		private static string NormalizeToken(string value)
		{
			return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
		}

		private static bool MatchesKnownCardStem(string cardTypeToken, IEnumerable<string> knownCardStemTokens)
		{
			foreach (string knownCardStemToken in knownCardStemTokens)
			{
				if (knownCardStemToken.Equals(cardTypeToken, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (knownCardStemToken.StartsWith(cardTypeToken, StringComparison.OrdinalIgnoreCase) && knownCardStemToken.AsSpan(cardTypeToken.Length).Length > 0 && knownCardStemToken.AsSpan(cardTypeToken.Length).ToString().All(char.IsDigit))
				{
					return true;
				}
			}
			return false;
		}

		internal static CardSurfaceDefinition? ScanExportedSurface(string? root, string providerId)
		{
			if (root == null || !Directory.Exists(root))
			{
				return null;
			}
			IReadOnlyList<string> assemblyPaths = ManagedCardPortraitReplacementScanner.GetAssemblyPaths(root, providerId);
			if (assemblyPaths.Count != 1)
			{
				return null;
			}
			try
			{
				FileInfo fileInfo = new FileInfo(assemblyPaths[0]);
				lock (SurfaceCache)
				{
					if (SurfaceCache.TryGetValue(fileInfo.FullName, out (long, DateTime, CardSurfaceDefinition) value) && value.Item1 == fileInfo.Length && value.Item2 == fileInfo.LastWriteTimeUtc)
					{
						return value.Item3;
					}
					CardSurfaceDefinition cardSurfaceDefinition = ScanExportedSurfaceAssembly(fileInfo.FullName);
					SurfaceCache[fileInfo.FullName] = (fileInfo.Length, fileInfo.LastWriteTimeUtc, cardSurfaceDefinition);
					return cardSurfaceDefinition;
				}
			}
			catch (Exception)
			{
				return null;
			}
		}

		internal static CardSurfaceDefinition? ScanExportedSurfaceAssembly(string path)
		{
			using FileStream peStream = File.OpenRead(path);
			using PEReader pEReader = new PEReader(peStream);
			if (!pEReader.HasMetadata)
			{
				return null;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			HashSet<int> hashSet = new HashSet<int>();
			HashSet<(CardSurfaceColor, int?)> hashSet2 = new HashSet<(CardSurfaceColor, int?)>();
			foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
				HashSet<string> hashSet3 = new HashSet<string>();
				HashSet<string> hashSet4 = new HashSet<string>();
				List<(MethodDefinition, List<SurfaceInstruction>)> list = new List<(MethodDefinition, List<SurfaceInstruction>)>();
				foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
				{
					MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(method);
					if (methodDefinition.RelativeVirtualAddress != 0)
					{
						byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
						if (iLBytes != null)
						{
							ScanIl(metadataReader, iLBytes, hashSet3, new HashSet<string>(), hashSet4);
							list.Add((methodDefinition, ReadSurfaceInstructions(metadataReader, iLBytes)));
						}
					}
				}
				if (hashSet3.Contains("_portrait") && hashSet3.Contains("_ancientPortrait") && hashSet4.Contains("CardReplacementRegistry.TryGetTexture") && hashSet4.Contains("NCard.get_Model"))
				{
					foreach (var item3 in list)
					{
						List<SurfaceInstruction> item = item3.Item2;
						for (int j = 1; j < item.Count; j++)
						{
							SurfaceInstruction surfaceInstruction = ((item[j - 1].Op == OpCodes.Conv_I8 && j > 1) ? item[j - 2] : item[j - 1]);
							if (!(item[j].Member == "TextureRect.set_StretchMode"))
							{
								continue;
							}
							int? integer = surfaceInstruction.Integer;
							if (integer.HasValue)
							{
								int valueOrDefault = integer.GetValueOrDefault();
								if (valueOrDefault >= 0 && valueOrDefault <= 6)
								{
									hashSet.Add(surfaceInstruction.Integer.Value);
								}
							}
						}
					}
				}
				foreach (var (methodDefinition2, list2) in list)
				{
					if (!HasSurfaceAttribute(metadataReader, methodDefinition2.GetCustomAttributes(), "HarmonyPostfix") || (!HasTinyBannerTarget(metadataReader, typeDefinition.GetCustomAttributes()) && !HasTinyBannerTarget(metadataReader, methodDefinition2.GetCustomAttributes())))
					{
						continue;
					}
					Parameter[] array = (from p in methodDefinition2.GetParameters().Select(metadataReader.GetParameter)
						where p.SequenceNumber > 0
						select p).ToArray();
					int valueOrDefault = array.Length;
					bool flag = ((valueOrDefault < 1 || valueOrDefault > 2) ? true : false);
					if (flag || metadataReader.GetString(array[0].Name) != "__result" || list2.Any(delegate(SurfaceInstruction i)
					{
						bool flag2 = i.Op.FlowControl == FlowControl.Call;
						if (flag2)
						{
							string member = i.Member;
							bool flag3 = ((member == "Color..ctor" || member == "CardUiModeSpoofPatch.ShouldSpoofForUi") ? true : false);
							flag2 = !flag3;
						}
						return flag2;
					}))
					{
						continue;
					}
					int? item2 = null;
					for (int k = 2; k < list2.Count; k++)
					{
						if ((!(list2[k].Op == OpCodes.Beq_S) && !(list2[k].Op == OpCodes.Beq)) || !(list2[k - 2].Op == OpCodes.Ldarg_1))
						{
							continue;
						}
						int? integer = list2[k - 1].Integer;
						if (integer.HasValue)
						{
							int valueOrDefault2 = integer.GetValueOrDefault();
							if (array.Length == 2 && metadataReader.GetString(array[1].Name) == "rarity")
							{
								item2 = valueOrDefault2;
							}
						}
					}
					for (int l = 5; l + 1 < list2.Count; l++)
					{
						if (list2[l].Op != OpCodes.Newobj || list2[l].Member != "Color..ctor" || list2[l - 5].Op != OpCodes.Ldarg_0 || list2[l + 1].Op != OpCodes.Stobj || list2[l + 1].Member != "Color" || l + 3 != list2.Count)
						{
							continue;
						}
						if (list2[list2.Count - 1].Op != OpCodes.Ret)
						{
							continue;
						}
						if (KnownBannerPrefix(list2.Take(l - 5).ToArray(), list2[list2.Count - 1].Offset, array.Length))
						{
							float?[] array2 = (from x in list2.Skip(l - 4).Take(4)
								select x.Float).ToArray();
							if (!array2.Any(delegate(float? v)
							{
								bool flag4 = !v.HasValue || !float.IsFinite(v.Value);
								bool flag5;
								if (!flag4)
								{
									if (v.HasValue)
									{
										float valueOrDefault3 = v.GetValueOrDefault();
										if (valueOrDefault3 < 0f || valueOrDefault3 > 1f)
										{
											flag5 = true;
											goto IL_0046;
										}
									}
									flag5 = false;
									goto IL_0046;
								}
								goto IL_0048;
								IL_0046:
								flag4 = flag5;
								goto IL_0048;
								IL_0048:
								return flag4;
							}))
							{
								hashSet2.Add((new CardSurfaceColor(array2[0].Value, array2[1].Value, array2[2].Value, array2[3].Value), item2));
							}
						}
					}
				}
			}
			int? portraitStretchMode = ((hashSet.Count == 1) ? new int?(hashSet.Single()) : ((int?)null));
			(CardSurfaceColor, int?) tuple2 = ((hashSet2.Count == 1) ? hashSet2.Single() : default((CardSurfaceColor, int?)));
			return (!portraitStretchMode.HasValue && tuple2.Item1 == null) ? null : new CardSurfaceDefinition(portraitStretchMode, tuple2.Item1, tuple2.Item2);
		}

		private static bool HasSurfaceAttribute(MetadataReader reader, CustomAttributeHandleCollection attributes, string name)
		{
			return attributes.Any((CustomAttributeHandle h) => SurfaceAttributeName(reader, reader.GetCustomAttribute(h)) == "HarmonyLib." + name);
		}

		private static string? SurfaceAttributeName(MetadataReader reader, CustomAttribute attribute)
		{
			if (attribute.Constructor.Kind != HandleKind.MemberReference)
			{
				return null;
			}
			MemberReference memberReference = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
			if (memberReference.Parent.Kind != HandleKind.TypeReference)
			{
				return null;
			}
			TypeReference typeReference = reader.GetTypeReference((TypeReferenceHandle)memberReference.Parent);
			return reader.GetString(typeReference.Namespace) + "." + reader.GetString(typeReference.Name);
		}

		private static bool HasTinyBannerTarget(MetadataReader reader, CustomAttributeHandleCollection attributes)
		{
			foreach (CustomAttributeHandle item in attributes)
			{
				CustomAttribute customAttribute = reader.GetCustomAttribute(item);
				if (SurfaceAttributeName(reader, customAttribute) != "HarmonyLib.HarmonyPatch")
				{
					continue;
				}
				try
				{
					BlobReader blobReader = reader.GetBlobReader(customAttribute.Value);
					if (blobReader.ReadUInt16() == 1)
					{
						string? text = blobReader.ReadSerializedString();
						if (((text != null) ? text.Split(',')[0] : null) == "MegaCrit.Sts2.Core.Nodes.Cards.NTinyCard" && blobReader.ReadSerializedString() == "GetBannerColor" && blobReader.ReadUInt16() == 0 && blobReader.RemainingBytes == 0)
						{
							return true;
						}
					}
				}
				catch (BadImageFormatException)
				{
				}
			}
			return false;
		}

		private static bool KnownBannerPrefix(SurfaceInstruction[] prefix, int returnOffset, int parameterCount)
		{
			if (prefix.Length == 0)
			{
				return parameterCount == 1;
			}
			bool flag = parameterCount != 2;
			if (!flag)
			{
				int num = prefix.Length;
				bool flag2 = ((num == 5 || num == 9) ? true : false);
				flag = !flag2;
			}
			if (flag || prefix[0].Op != OpCodes.Ldarg_1 || !prefix[1].Integer.HasValue || (prefix[2].Op != OpCodes.Beq_S && prefix[2].Op != OpCodes.Beq) || prefix[3].Op != OpCodes.Call || prefix[3].Member != "CardUiModeSpoofPatch.ShouldSpoofForUi")
			{
				return false;
			}
			if (prefix.Length == 5)
			{
				if (prefix[2].BranchTarget == returnOffset)
				{
					return IsFalseBranch(prefix[4], returnOffset);
				}
				return false;
			}
			if (prefix[2].BranchTarget == prefix[5].Offset && (prefix[4].Op == OpCodes.Br || prefix[4].Op == OpCodes.Br_S) && prefix[4].BranchTarget == prefix[6].Offset && prefix[5].Integer == 0 && prefix[6].Op == OpCodes.Stloc_0 && prefix[7].Op == OpCodes.Ldloc_0)
			{
				return IsFalseBranch(prefix[8], returnOffset);
			}
			return false;
			static bool IsFalseBranch(SurfaceInstruction i, int target)
			{
				if (i.Op == OpCodes.Brfalse || i.Op == OpCodes.Brfalse_S)
				{
					return i.BranchTarget == target;
				}
				return false;
			}
		}

		private static List<SurfaceInstruction> ReadSurfaceInstructions(MetadataReader reader, byte[] il)
		{
			List<SurfaceInstruction> list = new List<SurfaceInstruction>();
			int operandSize;
			for (int i = 0; i < il.Length; i += operandSize)
			{
				int offset = i;
				byte b = il[i++];
				ushort key = ((b == 254) ? ((ushort)(0xFE00 | il[i++])) : b);
				if (!OpCodesByValue.TryGetValue(key, out var value))
				{
					break;
				}
				operandSize = GetOperandSize(value.OperandType, il, i);
				if (operandSize < 0 || i + operandSize > il.Length)
				{
					throw new BadImageFormatException();
				}
				int? integer = ((value.Value >= OpCodes.Ldc_I4_0.Value && value.Value <= OpCodes.Ldc_I4_8.Value) ? new int?(value.Value - OpCodes.Ldc_I4_0.Value) : ((value == OpCodes.Ldc_I4) ? new int?(BitConverter.ToInt32(il, i)) : ((value == OpCodes.Ldc_I4_S) ? new int?((sbyte)il[i]) : ((int?)null))));
				float? @float = ((value == OpCodes.Ldc_R4) ? new float?(BitConverter.ToSingle(il, i)) : ((float?)null));
				string member = ((value.OperandType == OperandType.InlineMethod) ? ResolveMemberName(reader, MetadataTokens.EntityHandle(BitConverter.ToInt32(il, i))) : ((value.OperandType == OperandType.InlineType) ? ResolveTypeName(reader, MetadataTokens.EntityHandle(BitConverter.ToInt32(il, i))) : null));
				int? branchTarget = ((value.OperandType == OperandType.ShortInlineBrTarget) ? new int?(i + operandSize + (sbyte)il[i]) : ((value.OperandType == OperandType.InlineBrTarget) ? new int?(i + operandSize + BitConverter.ToInt32(il, i)) : ((int?)null)));
				if (value != OpCodes.Nop)
				{
					list.Add(new SurfaceInstruction(value, integer, @float, member, offset, branchTarget));
				}
			}
			return list;
		}
	}

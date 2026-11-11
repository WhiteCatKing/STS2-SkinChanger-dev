using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace STS2SkinChanger.Catalog;

internal static class RuntimeMonsterVisualModeScanner
{
	private sealed class MetadataTypeNameProvider : ISignatureTypeProvider<string, MetadataReader>, IConstructedTypeProvider<string>, ISZArrayTypeProvider<string>, ISimpleTypeProvider<string>
	{
		public string GetArrayType(string elementType, ArrayShape shape)
		{
			return elementType + "[]";
		}

		public string GetByReferenceType(string elementType)
		{
			return elementType + "&";
		}

		public string GetFunctionPointerType(MethodSignature<string> signature)
		{
			return "fnptr";
		}

		public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
		{
			return genericType;
		}

		public string GetGenericMethodParameter(MetadataReader genericContext, int index)
		{
			return "!!" + index;
		}

		public string GetGenericTypeParameter(MetadataReader genericContext, int index)
		{
			return "!" + index;
		}

		public string GetModifiedType(string modifierType, string unmodifiedType, bool isRequired)
		{
			return unmodifiedType;
		}

		public string GetPinnedType(string elementType)
		{
			return elementType;
		}

		public string GetPointerType(string elementType)
		{
			return elementType + "*";
		}

		public string GetPrimitiveType(PrimitiveTypeCode typeCode)
		{
			return typeCode.ToString();
		}

		public string GetSZArrayType(string elementType)
		{
			return elementType + "[]";
		}

		public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
		{
			return GetTypeName(reader, reader.GetTypeDefinition(handle));
		}

		public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
		{
			return GetTypeName(reader, reader.GetTypeReference(handle));
		}

		public string GetTypeFromSpecification(MetadataReader reader, MetadataReader genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
		{
			return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
		}
	}

	private sealed record EnumCandidate(string TypeName, IReadOnlyList<string> Values);

	private static readonly string[] ModeTypeSuffixes = new string[3] { "VisualMode", "AppearanceMode", "SkinMode" };

	private static readonly HashSet<string> SetterNames = new HashSet<string> { "SetMode", "SetVisualMode", "SetAppearanceMode", "SetSkinMode" };

	public static IReadOnlyList<RuntimeMonsterVisualMode> Scan(PckResourceIndex index, string providerId, SkinOption option)
	{
		string rootPath = index.Mod.RootPath;
		if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath) || !LooksLikeMultiModeMonsterScene(option))
		{
			return Array.Empty<RuntimeMonsterVisualMode>();
		}
		foreach (string item in EnumerateCandidateAssemblies(rootPath))
		{
			try
			{
				IReadOnlyList<RuntimeMonsterVisualMode> readOnlyList = ScanAssembly(item, providerId);
				if (readOnlyList.Count >= 2)
				{
					return readOnlyList.Select((RuntimeMonsterVisualMode mode) => mode with
					{
						ResourcePaths = FindModeResourcePaths(mode, option, index)
					}).ToArray();
				}
			}
			catch (Exception)
			{
			}
		}
		return Array.Empty<RuntimeMonsterVisualMode>();
	}

	private static IReadOnlyList<string> FindModeResourcePaths(RuntimeMonsterVisualMode mode, SkinOption option, PckResourceIndex index)
	{
		string[] words = (from word in SplitModeWords(mode.ModeName)
			where word.Length > 2 && !(word == "animated") && !(word == "animation") && !(word == "mode")
			select word).ToArray();
		if (words.Length == 0)
		{
			return Array.Empty<string>();
		}
		string text = option.Assets.Keys.Append(option.ManagedMonsterScene?.SourcePath ?? string.Empty).FirstOrDefault((string path) => path.StartsWith("res://scenes/creature_visuals/", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase));
		string modelToken = ((text == null) ? string.Empty : Path.GetFileNameWithoutExtension(text));
		IEnumerable<string> second = index.Archive.Paths.Where((string path) => path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase)).Select(SkinCatalog.NormalizeTakeoverPath);
		return (from path in (from path in option.Assets.Keys.Concat(second).Distinct<string>(StringComparer.OrdinalIgnoreCase)
				where path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase)
				where modelToken.Length == 0 || path.Contains(modelToken, StringComparison.OrdinalIgnoreCase)
				select path).Where(delegate(string path)
			{
				string normalizedPath = path.Replace('-', '_').ToLowerInvariant();
				return words.Any((string word) => normalizedPath.Contains(word, StringComparison.Ordinal));
			})
			where path.Contains("spriteframe", StringComparison.OrdinalIgnoreCase) || path.Contains("animation", StringComparison.OrdinalIgnoreCase) || path.Contains("atlas", StringComparison.OrdinalIgnoreCase)
			select path).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static IReadOnlyList<string> SplitModeWords(string value)
	{
		List<string> list = new List<string>();
		int num = 0;
		for (int i = 1; i < value.Length; i++)
		{
			char c = value[i];
			bool flag = ((c == '-' || c == '_') ? true : false);
			if (flag || (char.IsUpper(value[i]) && !char.IsUpper(value[i - 1])))
			{
				if (i > num)
				{
					int num2 = num;
					list.Add(value.Substring(num2, i - num2).Trim('_', '-').ToLowerInvariant());
				}
				c = value[i];
				flag = ((c == '-' || c == '_') ? true : false);
				num = (flag ? (i + 1) : i);
			}
		}
		if (num < value.Length)
		{
			int num2 = num;
			list.Add(value.Substring(num2, value.Length - num2).Trim('_', '-').ToLowerInvariant());
		}
		return list.Where((string word) => word.Length > 0).ToArray();
	}

	private static bool LooksLikeMultiModeMonsterScene(SkinOption option)
	{
		string[] source = (from path in option.Assets.Keys.Append(option.ManagedMonsterScene?.SourcePath ?? string.Empty)
			where path.Length > 0
			select path).ToArray();
		if (source.Any((string path) => path.StartsWith("res://scenes/creature_visuals/", StringComparison.OrdinalIgnoreCase) && path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase)))
		{
			return source.Any((string path) => path.EndsWith("spriteframes.tres", StringComparison.OrdinalIgnoreCase) || path.Contains("/atlas/", StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}

	private static IEnumerable<string> EnumerateCandidateAssemblies(string providerRoot)
	{
		return (from path in Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.AllDirectories)
			orderby Path.GetRelativePath(providerRoot, path).Count((char c) => (c == '/' || c == '\\') ? true : false) descending
			select path).ThenByDescending<string, string>((string path) => Path.GetRelativePath(providerRoot, path), StringComparer.OrdinalIgnoreCase);
	}

	private static IReadOnlyList<RuntimeMonsterVisualMode> ScanAssembly(string assemblyPath, string providerId)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return Array.Empty<RuntimeMonsterVisualMode>();
		}
		MetadataReader reader = pEReader.GetMetadataReader();
		MetadataTypeNameProvider provider = new MetadataTypeNameProvider();
		Dictionary<string, EnumCandidate> dictionary = new Dictionary<string, EnumCandidate>(StringComparer.Ordinal);
		foreach (TypeDefinitionHandle typeDefinition3 in reader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = reader.GetTypeDefinition(typeDefinition3);
			string name = reader.GetString(typeDefinition.Name);
			if (ModeTypeSuffixes.Any((string suffix) => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) && IsEnum(reader, typeDefinition.BaseType))
			{
				string[] array = (from field in typeDefinition.GetFields().Select(reader.GetFieldDefinition)
					where field.Attributes.HasFlag(FieldAttributes.Literal)
					select reader.GetString(field.Name) into value
					where !value.Equals("value__", StringComparison.Ordinal)
					select value).ToArray();
				int num = array.Length;
				if ((num >= 2 && num <= 12) || 1 == 0)
				{
					string typeName = GetTypeName(reader, typeDefinition);
					dictionary[typeName] = new EnumCandidate(typeName, array);
				}
			}
		}
		if (dictionary.Count == 0)
		{
			return Array.Empty<RuntimeMonsterVisualMode>();
		}
		foreach (TypeDefinitionHandle typeDefinition4 in reader.TypeDefinitions)
		{
			TypeDefinition typeDefinition2 = reader.GetTypeDefinition(typeDefinition4);
			string serviceTypeName = GetTypeName(reader, typeDefinition2);
			foreach (MethodDefinitionHandle method in typeDefinition2.GetMethods())
			{
				MethodDefinition methodDefinition = reader.GetMethodDefinition(method);
				string methodName = reader.GetString(methodDefinition.Name);
				if (!methodDefinition.Attributes.HasFlag(MethodAttributes.Static) || !SetterNames.Contains(methodName))
				{
					continue;
				}
				string text = methodDefinition.DecodeSignature(provider, reader).ParameterTypes.FirstOrDefault(dictionary.ContainsKey);
				if (text != null)
				{
					EnumCandidate candidate = dictionary[text];
					return candidate.Values.Select((string value) => new RuntimeMonsterVisualMode(providerId, Path.GetFullPath(assemblyPath), serviceTypeName, candidate.TypeName, methodName, value, DisplayModeName(value))).ToArray();
				}
			}
		}
		return Array.Empty<RuntimeMonsterVisualMode>();
	}

	private static bool IsEnum(MetadataReader reader, EntityHandle baseType)
	{
		return (baseType.Kind switch
		{
			HandleKind.TypeDefinition => GetTypeName(reader, reader.GetTypeDefinition((TypeDefinitionHandle)baseType)), 
			HandleKind.TypeReference => GetTypeName(reader, reader.GetTypeReference((TypeReferenceHandle)baseType)), 
			_ => string.Empty, 
		}).Equals("System.Enum", StringComparison.Ordinal);
	}

	private static string GetTypeName(MetadataReader reader, TypeDefinition type)
	{
		string @string = reader.GetString(type.Name);
		string string2 = reader.GetString(type.Namespace);
		if (!string.IsNullOrEmpty(string2))
		{
			return string2 + "." + @string;
		}
		return @string;
	}

	private static string GetTypeName(MetadataReader reader, TypeReference type)
	{
		string @string = reader.GetString(type.Name);
		string string2 = reader.GetString(type.Namespace);
		if (!string.IsNullOrEmpty(string2))
		{
			return string2 + "." + @string;
		}
		return @string;
	}

	private static string DisplayModeName(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray()) switch
		{
			"staticultrahd" => "超高清静态", 
			"performanceanimated" => "性能优化动画", 
			"originalhdanimated" => "原始高清动画", 
			_ => string.Concat(value.Select((char character, int index) => (index <= 0 || !char.IsUpper(character) || char.IsUpper(value[index - 1])) ? character.ToString() : (" " + character))).Replace('_', ' '), 
		};
	}
}

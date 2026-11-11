using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;

namespace STS2SkinChanger.Catalog;

internal static class ManagedMonsterSceneScanner
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

	private sealed record SceneCandidate(string? ModelTypeName, string ScenePath);

	[CompilerGenerated]
	private sealed class _003CScanStrings_003Ed__8 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private byte[] il;

		public byte[] _003C_003E3__il;

		private MetadataReader reader;

		public MetadataReader _003C_003E3__reader;

		private int _003Coffset_003E5__2;

		private int _003CoperandSize_003E5__3;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CScanStrings_003Ed__8(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_0177;
			}
			_003C_003E1__state = -1;
			_003Coffset_003E5__2 = 0;
			goto IL_018a;
			IL_0177:
			_003Coffset_003E5__2 += _003CoperandSize_003E5__3;
			goto IL_018a;
			IL_018a:
			if (_003Coffset_003E5__2 < il.Length)
			{
				byte b = il[_003Coffset_003E5__2++];
				ushort key = ((b == 254 && _003Coffset_003E5__2 < il.Length) ? ((ushort)(0xFE00 | il[_003Coffset_003E5__2++])) : b);
				if (!OpCodesByValue.TryGetValue(key, out var value))
				{
					return false;
				}
				int num2 = _003Coffset_003E5__2;
				_003CoperandSize_003E5__3 = GetOperandSize(value.OperandType, il, num2);
				if (_003CoperandSize_003E5__3 < 0 || num2 + _003CoperandSize_003E5__3 > il.Length)
				{
					return false;
				}
				if (value.OperandType == OperandType.InlineString && _003CoperandSize_003E5__3 == 4)
				{
					int num3 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num2, 4));
					string value2 = null;
					try
					{
						value2 = reader.GetUserString(MetadataTokens.UserStringHandle(num3 & 0xFFFFFF));
					}
					catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
					{
					}
					if (!string.IsNullOrWhiteSpace(value2))
					{
						_003C_003E2__current = value2;
						_003C_003E1__state = 1;
						return true;
					}
				}
				goto IL_0177;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CScanStrings_003Ed__8 _003CScanStrings_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CScanStrings_003Ed__ = this;
			}
			else
			{
				_003CScanStrings_003Ed__ = new _003CScanStrings_003Ed__8(0);
			}
			_003CScanStrings_003Ed__.reader = _003C_003E3__reader;
			_003CScanStrings_003Ed__.il = _003C_003E3__il;
			return _003CScanStrings_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

	public static IReadOnlyList<ManagedMonsterSceneReplacement> Scan(string? providerRoot, string providerId)
	{
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return Array.Empty<ManagedMonsterSceneReplacement>();
		}
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		object obj = ((!File.Exists(text)) ? ((object)Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((object)new string[1] { text }));
		Dictionary<string, ManagedMonsterSceneReplacement> dictionary = new Dictionary<string, ManagedMonsterSceneReplacement>(StringComparer.OrdinalIgnoreCase);
		string[] array = (string[])obj;
		foreach (string assemblyPath in array)
		{
			try
			{
				ScanAssembly(assemblyPath, dictionary);
			}
			catch (Exception)
			{
			}
		}
		return dictionary.Values.ToArray();
	}

	public static IReadOnlyList<ManagedMonsterRuntimeProfile> ScanRuntimeProfiles(string? providerRoot, string providerId)
	{
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return Array.Empty<ManagedMonsterRuntimeProfile>();
		}
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		object obj = ((!File.Exists(text)) ? ((object)Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((object)new string[1] { text }));
		Dictionary<string, ManagedMonsterRuntimeProfile> dictionary = new Dictionary<string, ManagedMonsterRuntimeProfile>(StringComparer.OrdinalIgnoreCase);
		string[] array = (string[])obj;
		foreach (string assemblyPath in array)
		{
			try
			{
				ScanRuntimeProfilesAssembly(assemblyPath, dictionary);
			}
			catch (Exception)
			{
			}
		}
		return dictionary.Values.ToArray();
	}

	public static IReadOnlyList<ManagedMonsterAssetDeclaration> ScanDeclaredAssets(string? providerRoot, string providerId)
	{
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return Array.Empty<ManagedMonsterAssetDeclaration>();
		}
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		object obj = ((!File.Exists(text)) ? ((object)Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((object)new string[1] { text }));
		Dictionary<string, ManagedMonsterAssetDeclaration> dictionary = new Dictionary<string, ManagedMonsterAssetDeclaration>(StringComparer.OrdinalIgnoreCase);
		string[] array = (string[])obj;
		foreach (string assemblyPath in array)
		{
			try
			{
				ScanDeclaredAssetsAssembly(assemblyPath, dictionary);
			}
			catch (Exception)
			{
			}
		}
		return dictionary.Values.ToArray();
	}

	private static void ScanDeclaredAssetsAssembly(string assemblyPath, IDictionary<string, ManagedMonsterAssetDeclaration> declarations)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
			string text = ResolveTypeName(metadataReader, typeDefinition.BaseType);
			if (text == null || !IsMonsterModelType(text))
			{
				continue;
			}
			string @string = metadataReader.GetString(typeDefinition.Name);
			foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(method);
				if (methodDefinition.RelativeVirtualAddress == 0 || !metadataReader.GetString(methodDefinition.Name).Equals("get_AssetPaths", StringComparison.Ordinal))
				{
					continue;
				}
				byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
				if (iLBytes != null)
				{
					string[] array = (from path in ScanStrings(metadataReader, iLBytes).Select(TryCanonicalizeAssetPath)
						where path != null
						select path).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
					if (array.Length != 0)
					{
						declarations[NormalizeToken(@string)] = new ManagedMonsterAssetDeclaration(@string, array);
					}
				}
			}
		}
	}

	private static void ScanRuntimeProfilesAssembly(string assemblyPath, IDictionary<string, ManagedMonsterRuntimeProfile> profiles)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		foreach (TypeDefinitionHandle typeDefinition in metadataReader.TypeDefinitions)
		{
			foreach (MethodDefinitionHandle method in metadataReader.GetTypeDefinition(typeDefinition).GetMethods())
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
				string[] source = (from path in ScanStrings(metadataReader, iLBytes).Select(NormalizeResourcePath)
					where path != null
					select path).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
				string[] array = source.Where(IsCanonicalCreatureScenePath).ToArray();
				if (array.Length != 1)
				{
					continue;
				}
				string[] array2 = source.Where((string path) => !IsCanonicalCreatureScenePath(path) && IsPrivateMonsterVisualResourcePath(path)).ToArray();
				if (array2.Length != 0)
				{
					string text = array[0];
					if (profiles.TryGetValue(text, out ManagedMonsterRuntimeProfile value))
					{
						array2 = value.ProviderResourcePaths.Concat(array2).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
					}
					profiles[text] = new ManagedMonsterRuntimeProfile(text, array2);
				}
			}
		}
	}

	private static void ScanAssembly(string assemblyPath, IDictionary<string, ManagedMonsterSceneReplacement> replacements)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return;
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		MetadataTypeNameProvider provider = new MetadataTypeNameProvider();
		foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
			string @string = metadataReader.GetString(typeDefinition.Name);
			foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(method);
				if (methodDefinition.RelativeVirtualAddress == 0)
				{
					continue;
				}
				MethodSignature<string> methodSignature = methodDefinition.DecodeSignature(provider, metadataReader);
				if (methodSignature.ParameterTypes.Any(IsNonMonsterModelType))
				{
					continue;
				}
				byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
				if (iLBytes == null)
				{
					continue;
				}
				List<SceneCandidate> list = ScanIl(metadataReader, iLBytes);
				if (list.Count == 0)
				{
					continue;
				}
				bool flag = methodSignature.ParameterTypes.Any(IsStringByReferenceType);
				if (!@string.Contains("VisualsPath", StringComparison.OrdinalIgnoreCase) && !@string.Contains("Monster", StringComparison.OrdinalIgnoreCase) && (!flag || !methodSignature.ParameterTypes.Any(IsMonsterModelType)))
				{
					continue;
				}
				string patchTargetName = GetPatchTargetName(@string);
				foreach (SceneCandidate item in list)
				{
					string typeName = item.ModelTypeName ?? patchTargetName;
					if (IsPlausibleModelType(typeName))
					{
						string text = SimpleTypeName(typeName);
						replacements[NormalizeToken(text)] = new ManagedMonsterSceneReplacement(text, item.ScenePath);
					}
				}
			}
		}
	}

	private static List<SceneCandidate> ScanIl(MetadataReader reader, byte[] il)
	{
		List<SceneCandidate> list = new List<SceneCandidate>();
		string modelTypeName = null;
		int operandSize;
		for (int i = 0; i < il.Length; i += operandSize)
		{
			byte b = il[i++];
			ushort key = ((b == 254 && i < il.Length) ? ((ushort)(0xFE00 | il[i++])) : b);
			if (!OpCodesByValue.TryGetValue(key, out var value))
			{
				return list;
			}
			int num = i;
			operandSize = GetOperandSize(value.OperandType, il, num);
			if (operandSize < 0 || num + operandSize > il.Length)
			{
				return list;
			}
			if (value.OperandType == OperandType.InlineType && operandSize == 4 && (value.Equals(OpCodes.Isinst) || value.Equals(OpCodes.Castclass)))
			{
				int token = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
				modelTypeName = ResolveTypeName(reader, MetadataTokens.EntityHandle(token));
			}
			else
			{
				if (value.OperandType != OperandType.InlineString || operandSize != 4)
				{
					continue;
				}
				int num2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
				try
				{
					string userString = reader.GetUserString(MetadataTokens.UserStringHandle(num2 & 0xFFFFFF));
					if (IsScenePath(userString))
					{
						list.Add(new SceneCandidate(modelTypeName, userString));
					}
				}
				catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
				{
				}
			}
		}
		return list;
	}

	[IteratorStateMachine(typeof(_003CScanStrings_003Ed__8))]
	private static IEnumerable<string> ScanStrings(MetadataReader reader, byte[] il)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CScanStrings_003Ed__8(-2)
		{
			_003C_003E3__reader = reader,
			_003C_003E3__il = il
		};
	}

	private static string? TryCanonicalizeAssetPath(string value)
	{
		string text = value.Replace('\\', '/').Trim();
		if (text.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return text;
		}
		text = text.TrimStart('/');
		if (text.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
		{
			return "res://" + text;
		}
		if (!text.Contains('/') || (!text.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !text.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) && !text.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !text.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) && !text.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)))
		{
			return null;
		}
		return "res://images/" + text;
	}

	private static string? NormalizeResourcePath(string value)
	{
		string text = value.Replace('\\', '/').Trim();
		if (!text.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		return text;
	}

	private static bool IsCanonicalCreatureScenePath(string path)
	{
		if (path.StartsWith("res://scenes/creature_visuals/", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsPrivateMonsterVisualResourcePath(string path)
	{
		if (!path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return new string[6] { "/animations/", "/animation/", "/models/", "/model/", "/visuals/", "/visual/" }.Any((string segment) => path.Contains(segment, StringComparison.OrdinalIgnoreCase));
	}

	private static bool IsScenePath(string value)
	{
		if (value.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return value.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsMonsterModelType(string typeName)
	{
		return TypeNameEndsWith(typeName, "MonsterModel");
	}

	private static bool IsStringByReferenceType(string typeName)
	{
		if (!typeName.Equals("String&", StringComparison.OrdinalIgnoreCase))
		{
			return typeName.Equals("System.String&", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsNonMonsterModelType(string typeName)
	{
		if (!TypeNameEndsWith(typeName, "CharacterModel") && !TypeNameEndsWith(typeName, "CardModel"))
		{
			return TypeNameEndsWith(typeName, "EventModel");
		}
		return true;
	}

	private static bool TypeNameEndsWith(string typeName, string expected)
	{
		if (!typeName.Equals(expected, StringComparison.OrdinalIgnoreCase) && !typeName.EndsWith("." + expected, StringComparison.OrdinalIgnoreCase))
		{
			return typeName.EndsWith("+" + expected, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsPlausibleModelType(string? typeName)
	{
		if (string.IsNullOrWhiteSpace(typeName))
		{
			return false;
		}
		string text = NormalizeToken(typeName);
		if (text.Length > 2)
		{
			switch (text)
			{
			default:
				return !(text == "node2d");
			case "monster":
			case "monstermodel":
			case "ncreaturevisuals":
			case "packedscene":
			case "node":
				return false;
			}
		}
		return false;
	}

	private static string? GetPatchTargetName(string patchTypeName)
	{
		int num = patchTypeName.IndexOf("VisualsPath", StringComparison.OrdinalIgnoreCase);
		if (num <= 0)
		{
			return null;
		}
		string text = patchTypeName.Substring(0, num).TrimEnd('_', '.', '+');
		int num2 = Math.Max(text.LastIndexOf('.'), text.LastIndexOf('+'));
		if (num2 < 0)
		{
			return text;
		}
		string text2 = text;
		int num3 = num2 + 1;
		return text2.Substring(num3, text2.Length - num3).TrimEnd('_');
	}

	private static string? ResolveTypeName(MetadataReader reader, EntityHandle handle)
	{
		if (handle.IsNil)
		{
			return null;
		}
		return handle.Kind switch
		{
			HandleKind.TypeDefinition => GetTypeName(reader, reader.GetTypeDefinition((TypeDefinitionHandle)handle)), 
			HandleKind.TypeReference => GetTypeName(reader, reader.GetTypeReference((TypeReferenceHandle)handle)), 
			_ => null, 
		};
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

	private static string SimpleTypeName(string typeName)
	{
		int num = Math.Max(typeName.LastIndexOf('.'), typeName.LastIndexOf('+'));
		if (num < 0)
		{
			return typeName;
		}
		int num2 = num + 1;
		return typeName.Substring(num2, typeName.Length - num2);
	}

	private static string NormalizeToken(string value)
	{
		return new string(SimpleTypeName(value).Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
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

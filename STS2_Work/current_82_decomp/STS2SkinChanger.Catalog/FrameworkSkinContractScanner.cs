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
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Catalog;

internal static class FrameworkSkinContractScanner
{
	private sealed record RawDeclaration(string DescriptorTypeName, string FrameworkAssemblyName, string TargetGroupId, IReadOnlyDictionary<string, string> ScalarResources, IReadOnlyDictionary<string, IReadOnlyList<string>> ListResources, IReadOnlyDictionary<string, string> ScalarValues);

	private sealed record Registration(string DescriptorTypeName, string SkinId, string DisplayName);

	private sealed record IlInstruction(OpCode OpCode, EntityHandle Entity, string? UserString);

	private sealed record CacheEntry(long Length, DateTime LastWriteTimeUtc, string ProviderId, IReadOnlyList<FrameworkCharacterSkinContract> Contracts);

	private sealed record MetadataType(string FullName, string? AssemblyName, IReadOnlyList<MetadataType> GenericArguments)
	{
		public string SimpleName
		{
			get
			{
				int num = Math.Max(FullName.LastIndexOf('.'), FullName.LastIndexOf('+'));
				if (num >= 0)
				{
					string fullName = FullName;
					int num2 = num + 1;
					return fullName.Substring(num2, fullName.Length - num2);
				}
				return FullName;
			}
		}
	}

	private sealed class MetadataTypeProvider(string currentAssemblyName) : ISignatureTypeProvider<MetadataType, object?>, IConstructedTypeProvider<MetadataType>, ISZArrayTypeProvider<MetadataType>, ISimpleTypeProvider<MetadataType>
	{
		public MetadataType GetArrayType(MetadataType elementType, ArrayShape shape)
		{
			return new MetadataType(elementType.FullName + "[]", elementType.AssemblyName, new _003C_003Ez__ReadOnlySingleElementList<MetadataType>(elementType));
		}

		public MetadataType GetByReferenceType(MetadataType elementType)
		{
			return new MetadataType(elementType.FullName + "&", elementType.AssemblyName, new _003C_003Ez__ReadOnlySingleElementList<MetadataType>(elementType));
		}

		public MetadataType GetFunctionPointerType(MethodSignature<MetadataType> signature)
		{
			return new MetadataType("methodptr", null, Array.Empty<MetadataType>());
		}

		public MetadataType GetGenericInstantiation(MetadataType genericType, ImmutableArray<MetadataType> typeArguments)
		{
			return genericType with
			{
				GenericArguments = typeArguments
			};
		}

		public MetadataType GetGenericMethodParameter(object? genericContext, int index)
		{
			return new MetadataType("!!" + index, null, Array.Empty<MetadataType>());
		}

		public MetadataType GetGenericTypeParameter(object? genericContext, int index)
		{
			return new MetadataType("!" + index, null, Array.Empty<MetadataType>());
		}

		public MetadataType GetModifiedType(MetadataType modifier, MetadataType unmodifiedType, bool isRequired)
		{
			return unmodifiedType;
		}

		public MetadataType GetPinnedType(MetadataType elementType)
		{
			return elementType;
		}

		public MetadataType GetPointerType(MetadataType elementType)
		{
			return new MetadataType(elementType.FullName + "*", elementType.AssemblyName, new _003C_003Ez__ReadOnlySingleElementList<MetadataType>(elementType));
		}

		public MetadataType GetPrimitiveType(PrimitiveTypeCode typeCode)
		{
			return new MetadataType(typeCode.ToString(), "System.Private.CoreLib", Array.Empty<MetadataType>());
		}

		public MetadataType GetSZArrayType(MetadataType elementType)
		{
			return new MetadataType(elementType.FullName + "[]", elementType.AssemblyName, new _003C_003Ez__ReadOnlySingleElementList<MetadataType>(elementType));
		}

		public MetadataType GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
		{
			return new MetadataType(GetTypeDefinitionFullName(reader, handle), currentAssemblyName, Array.Empty<MetadataType>());
		}

		public MetadataType GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
		{
			TypeReference typeReference = reader.GetTypeReference(handle);
			string @string = reader.GetString(typeReference.Namespace);
			string string2 = reader.GetString(typeReference.Name);
			string fullName = (string.IsNullOrEmpty(@string) ? string2 : (@string + "." + string2));
			string assemblyName = ((typeReference.ResolutionScope.Kind == HandleKind.AssemblyReference) ? reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)typeReference.ResolutionScope).Name) : null);
			return new MetadataType(fullName, assemblyName, Array.Empty<MetadataType>());
		}

		public MetadataType GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
		{
			return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
		}
	}

	[CompilerGenerated]
	private sealed class _003CReadInstructions_003Ed__8 : IEnumerable<IlInstruction>, IEnumerable, IEnumerator<IlInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private IlInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private PEReader peReader;

		public PEReader _003C_003E3__peReader;

		private MethodDefinition method;

		public MethodDefinition _003C_003E3__method;

		private MetadataReader reader;

		public MetadataReader _003C_003E3__reader;

		private byte[] _003Cil_003E5__2;

		private int _003Coffset_003E5__3;

		private int _003CoperandSize_003E5__4;

		IlInstruction IEnumerator<IlInstruction>.Current
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
		public _003CReadInstructions_003Ed__8(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cil_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cil_003E5__2 = peReader.GetMethodBody(method.RelativeVirtualAddress).GetILBytes();
				if (_003Cil_003E5__2 == null)
				{
					return false;
				}
				_003Coffset_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Coffset_003E5__3 += _003CoperandSize_003E5__4;
				break;
			}
			if (_003Coffset_003E5__3 < _003Cil_003E5__2.Length)
			{
				byte b = _003Cil_003E5__2[_003Coffset_003E5__3++];
				ushort key = ((b == 254 && _003Coffset_003E5__3 < _003Cil_003E5__2.Length) ? ((ushort)(0xFE00 | _003Cil_003E5__2[_003Coffset_003E5__3++])) : b);
				if (!OpCodesByValue.TryGetValue(key, out var value))
				{
					return false;
				}
				int num = _003Coffset_003E5__3;
				_003CoperandSize_003E5__4 = GetOperandSize(value.OperandType, _003Cil_003E5__2, num);
				if (_003CoperandSize_003E5__4 < 0 || num + _003CoperandSize_003E5__4 > _003Cil_003E5__2.Length)
				{
					return false;
				}
				string userString = null;
				EntityHandle entity = default(EntityHandle);
				if (value.OperandType == OperandType.InlineString && _003CoperandSize_003E5__4 == 4)
				{
					int num2 = BinaryPrimitives.ReadInt32LittleEndian(_003Cil_003E5__2.AsSpan(num, 4));
					try
					{
						userString = reader.GetUserString(MetadataTokens.UserStringHandle(num2 & 0xFFFFFF));
					}
					catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
					{
					}
				}
				else
				{
					OperandType operandType = value.OperandType;
					bool flag = ((operandType == OperandType.InlineMethod || (uint)(operandType - 12) <= 1u) ? true : false);
					if (flag && _003CoperandSize_003E5__4 == 4)
					{
						int token = BinaryPrimitives.ReadInt32LittleEndian(_003Cil_003E5__2.AsSpan(num, 4));
						try
						{
							entity = MetadataTokens.EntityHandle(token);
						}
						catch (ArgumentException)
						{
							entity = default(EntityHandle);
						}
					}
				}
				_003C_003E2__current = new IlInstruction(value, entity, userString);
				_003C_003E1__state = 1;
				return true;
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
		IEnumerator<IlInstruction> IEnumerable<IlInstruction>.GetEnumerator()
		{
			_003CReadInstructions_003Ed__8 _003CReadInstructions_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CReadInstructions_003Ed__ = this;
			}
			else
			{
				_003CReadInstructions_003Ed__ = new _003CReadInstructions_003Ed__8(0);
			}
			_003CReadInstructions_003Ed__.peReader = _003C_003E3__peReader;
			_003CReadInstructions_003Ed__.reader = _003C_003E3__reader;
			_003CReadInstructions_003Ed__.method = _003C_003E3__method;
			return _003CReadInstructions_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<IlInstruction>)this).GetEnumerator();
		}
	}

	private static readonly object CacheSync = new object();

	private static readonly Dictionary<string, CacheEntry> Cache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

	public static IReadOnlyList<FrameworkCharacterSkinContract> Scan(string? providerRoot, string providerId)
	{
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return Array.Empty<FrameworkCharacterSkinContract>();
		}
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		return ((!File.Exists(text)) ? ((IEnumerable<string>)Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((IEnumerable<string>)new string[1] { text })).SelectMany((string path) => ScanAssemblyCached(path, providerId)).ToArray();
	}

	private static IReadOnlyList<FrameworkCharacterSkinContract> ScanAssemblyCached(string assemblyPath, string providerId)
	{
		FileInfo fileInfo = new FileInfo(assemblyPath);
		lock (CacheSync)
		{
			if (Cache.TryGetValue(assemblyPath, out CacheEntry value) && value.Length == fileInfo.Length && value.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc && value.ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase))
			{
				return value.Contracts;
			}
		}
		IReadOnlyList<FrameworkCharacterSkinContract> readOnlyList;
		try
		{
			readOnlyList = ScanAssembly(assemblyPath, providerId);
		}
		catch (Exception)
		{
			readOnlyList = Array.Empty<FrameworkCharacterSkinContract>();
		}
		lock (CacheSync)
		{
			Cache[assemblyPath] = new CacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, providerId, readOnlyList);
			return readOnlyList;
		}
	}

	private static IReadOnlyList<FrameworkCharacterSkinContract> ScanAssembly(string assemblyPath, string providerId)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return Array.Empty<FrameworkCharacterSkinContract>();
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		MetadataTypeProvider metadataTypeProvider = new MetadataTypeProvider(metadataReader.IsAssembly ? metadataReader.GetString(metadataReader.GetAssemblyDefinition().Name) : Path.GetFileNameWithoutExtension(assemblyPath));
		Dictionary<string, RawDeclaration> dictionary = new Dictionary<string, RawDeclaration>(StringComparer.Ordinal);
		List<FrameworkModelSkinContract> list = new List<FrameworkModelSkinContract>();
		List<FrameworkModelSkinContract> list2 = new List<FrameworkModelSkinContract>();
		foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
		{
			TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
			string typeDefinitionFullName = GetTypeDefinitionFullName(metadataReader, typeDefinition2);
			MetadataType metadataType = DecodeType(metadataReader, typeDefinition.BaseType, metadataTypeProvider);
			if (metadataType == null || metadataType.GenericArguments.Count != 1)
			{
				continue;
			}
			string text = TrimGenericArity(metadataType.SimpleName);
			string text2 = TrimGenericArity(metadataType.GenericArguments[0].SimpleName);
			Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.Ordinal);
			Dictionary<string, IReadOnlyList<string>> dictionary3 = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
			Dictionary<string, string> dictionary4 = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (PropertyDefinitionHandle property in typeDefinition.GetProperties())
			{
				PropertyDefinition propertyDefinition = metadataReader.GetPropertyDefinition(property);
				MethodDefinitionHandle getter = propertyDefinition.GetAccessors().Getter;
				if (getter.IsNil)
				{
					continue;
				}
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(getter);
				if (methodDefinition.RelativeVirtualAddress == 0)
				{
					continue;
				}
				string @string = metadataReader.GetString(propertyDefinition.Name);
				string[] source = ReadUserStrings(pEReader, metadataReader, methodDefinition).ToArray();
				string[] array = source.Where(IsResourcePath).Select(NormalizeResourcePath).Distinct<string>(StringComparer.OrdinalIgnoreCase)
					.ToArray();
				if (array.Length == 1)
				{
					dictionary2[@string] = array[0];
				}
				else if (array.Length > 1)
				{
					dictionary3[@string] = array;
				}
				else if (@string.EndsWith("Color", StringComparison.Ordinal))
				{
					string text3 = source.LastOrDefault(IsColorLiteral);
					if (text3 != null)
					{
						dictionary4[@string] = text3;
					}
				}
			}
			string frameworkAssemblyName = metadataType.AssemblyName ?? string.Empty;
			if (text.Equals("CharacterSkin", StringComparison.Ordinal) && dictionary2.Count + dictionary3.Count > 0)
			{
				dictionary[typeDefinitionFullName] = new RawDeclaration(typeDefinitionFullName, frameworkAssemblyName, NormalizeToken(text2), dictionary2, dictionary3, dictionary4);
			}
			else if (text.Equals("OrbSkin", StringComparison.Ordinal) && dictionary2.Count + dictionary3.Count > 0)
			{
				list.Add(new FrameworkModelSkinContract(text2, dictionary2, dictionary3, dictionary4));
			}
			else if (text.Equals("RelicSkin", StringComparison.Ordinal) && dictionary2.Count + dictionary3.Count > 0)
			{
				list2.Add(new FrameworkModelSkinContract(text2, dictionary2, dictionary3, dictionary4));
			}
		}
		if (dictionary.Count == 0)
		{
			return Array.Empty<FrameworkCharacterSkinContract>();
		}
		List<Registration> registrations = ReadRegistrations(pEReader, metadataReader, metadataTypeProvider, dictionary.Keys);
		int num = dictionary.Values.Select((RawDeclaration declaration) => declaration.TargetGroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count();
		List<FrameworkModelSkinContract> orbs = ((num == 1) ? list : new List<FrameworkModelSkinContract>());
		List<FrameworkModelSkinContract> relics = ((num == 1) ? list2 : new List<FrameworkModelSkinContract>());
		List<FrameworkCharacterSkinContract> list3 = new List<FrameworkCharacterSkinContract>();
		foreach (RawDeclaration declaration2 in dictionary.Values)
		{
			Registration? registration2 = registrations.FirstOrDefault((Registration candidate) => candidate.DescriptorTypeName.Equals(declaration2.DescriptorTypeName, StringComparison.Ordinal));
			string descriptorTypeName = declaration2.DescriptorTypeName;
			int num2 = declaration2.DescriptorTypeName.LastIndexOf('.') + 1;
			string text4 = descriptorTypeName.Substring(num2, descriptorTypeName.Length - num2);
			string text5 = registration2?.SkinId ?? text4;
			string displayName = registration2?.DisplayName ?? SplitPascalCase(text4);
			list3.Add(new FrameworkCharacterSkinContract(providerId, declaration2.FrameworkAssemblyName, declaration2.TargetGroupId, providerId + "::" + NormalizeOptionToken(text5), text5, displayName, declaration2.DescriptorTypeName, declaration2.ScalarResources, declaration2.ListResources, declaration2.ScalarValues, orbs, relics));
		}
		return list3.OrderBy((FrameworkCharacterSkinContract contract) => registrations.FindIndex((Registration registration) => registration.DescriptorTypeName.Equals(contract.DescriptorTypeName, StringComparison.Ordinal))).ThenBy<FrameworkCharacterSkinContract, string>((FrameworkCharacterSkinContract contract) => contract.DisplayName, StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static List<Registration> ReadRegistrations(PEReader peReader, MetadataReader reader, MetadataTypeProvider typeProvider, IEnumerable<string> descriptorTypeNames)
	{
		HashSet<string> hashSet = descriptorTypeNames.ToHashSet<string>(StringComparer.Ordinal);
		List<Registration> list = new List<Registration>();
		foreach (TypeDefinitionHandle typeDefinition in reader.TypeDefinitions)
		{
			foreach (MethodDefinitionHandle method in reader.GetTypeDefinition(typeDefinition).GetMethods())
			{
				MethodDefinition methodDefinition = reader.GetMethodDefinition(method);
				if (methodDefinition.RelativeVirtualAddress == 0)
				{
					continue;
				}
				List<string> list2 = new List<string>();
				Queue<(string, string)> queue = new Queue<(string, string)>();
				foreach (IlInstruction item in ReadInstructions(peReader, reader, methodDefinition))
				{
					if (item.UserString != null && !IsResourcePath(item.UserString))
					{
						list2.Add(item.UserString);
						if (list2.Count > 8)
						{
							list2.RemoveAt(0);
						}
					}
					if (!item.OpCode.Equals(OpCodes.Newobj) || item.Entity.IsNil)
					{
						continue;
					}
					MetadataType metadataType = ResolveMethodDeclaringType(reader, item.Entity, typeProvider);
					if (!(metadataType == null))
					{
						if (TrimGenericArity(metadataType.SimpleName).Equals("SkinData", StringComparison.Ordinal) && list2.Count >= 2)
						{
							queue.Enqueue((list2[list2.Count - 2], list2[list2.Count - 1]));
						}
						else if (hashSet.Contains(metadataType.FullName) && queue.Count > 0)
						{
							(string, string) tuple = queue.Dequeue();
							list.Add(new Registration(metadataType.FullName, tuple.Item1, tuple.Item2));
						}
					}
				}
			}
		}
		return list.DistinctBy<Registration, string>((Registration registration) => registration.DescriptorTypeName, StringComparer.Ordinal).ToList();
	}

	private static IEnumerable<string> ReadUserStrings(PEReader peReader, MetadataReader reader, MethodDefinition method)
	{
		return from instruction in ReadInstructions(peReader, reader, method)
			where instruction.UserString != null
			select instruction.UserString;
	}

	[IteratorStateMachine(typeof(_003CReadInstructions_003Ed__8))]
	private static IEnumerable<IlInstruction> ReadInstructions(PEReader peReader, MetadataReader reader, MethodDefinition method)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CReadInstructions_003Ed__8(-2)
		{
			_003C_003E3__peReader = peReader,
			_003C_003E3__reader = reader,
			_003C_003E3__method = method
		};
	}

	private static MetadataType? ResolveMethodDeclaringType(MetadataReader reader, EntityHandle methodHandle, MetadataTypeProvider typeProvider)
	{
		if (methodHandle.Kind == HandleKind.MethodSpecification)
		{
			methodHandle = reader.GetMethodSpecification((MethodSpecificationHandle)methodHandle).Method;
		}
		return methodHandle.Kind switch
		{
			HandleKind.MethodDefinition => DecodeType(reader, reader.GetMethodDefinition((MethodDefinitionHandle)methodHandle).GetDeclaringType(), typeProvider), 
			HandleKind.MemberReference => DecodeType(reader, reader.GetMemberReference((MemberReferenceHandle)methodHandle).Parent, typeProvider), 
			_ => null, 
		};
	}

	private static MetadataType? DecodeType(MetadataReader reader, EntityHandle handle, MetadataTypeProvider provider)
	{
		if (handle.IsNil)
		{
			return null;
		}
		return handle.Kind switch
		{
			HandleKind.TypeDefinition => provider.GetTypeFromDefinition(reader, (TypeDefinitionHandle)handle, 0), 
			HandleKind.TypeReference => provider.GetTypeFromReference(reader, (TypeReferenceHandle)handle, 0), 
			HandleKind.TypeSpecification => reader.GetTypeSpecification((TypeSpecificationHandle)handle).DecodeSignature(provider, null), 
			_ => null, 
		};
	}

	private static string GetTypeDefinitionFullName(MetadataReader reader, TypeDefinitionHandle handle)
	{
		TypeDefinition typeDefinition = reader.GetTypeDefinition(handle);
		string @string = reader.GetString(typeDefinition.Name);
		TypeDefinitionHandle declaringType = typeDefinition.GetDeclaringType();
		if (!declaringType.IsNil)
		{
			return GetTypeDefinitionFullName(reader, declaringType) + "+" + @string;
		}
		string string2 = reader.GetString(typeDefinition.Namespace);
		if (!string.IsNullOrEmpty(string2))
		{
			return string2 + "." + @string;
		}
		return @string;
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

	private static bool IsResourcePath(string value)
	{
		return value.StartsWith("res://", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsColorLiteral(string value)
	{
		int length = value.Length;
		if ((length == 6 || length == 8) ? true : false)
		{
			return value.All(Uri.IsHexDigit);
		}
		return false;
	}

	private static string NormalizeResourcePath(string value)
	{
		return value.Replace('\\', '/');
	}

	private static string NormalizeToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static string NormalizeOptionToken(string value)
	{
		string text = new string(value.Where(delegate(char character)
		{
			bool flag = char.IsLetterOrDigit(character);
			if (!flag)
			{
				bool flag2 = ((character == '-' || character == '.' || character == '_') ? true : false);
				flag = flag2;
			}
			return flag;
		}).ToArray());
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return "skin";
	}

	private static string TrimGenericArity(string value)
	{
		int num = value.IndexOf('`');
		if (num >= 0)
		{
			return value.Substring(0, num);
		}
		return value;
	}

	private static string SplitPascalCase(string value)
	{
		return Regex.Replace(value, "(?<!^)([A-Z])", " $1");
	}
}

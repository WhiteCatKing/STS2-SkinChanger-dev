using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Catalog;

internal static class ProviderSettingsVariantOptions
{
	private sealed record SettingsVariantBinding(SettingsVariantContract Contract, int Index)
	{
		private static MethodInfo? _refreshMethod;

		private static bool _refreshSearched;

		internal void Apply()
		{
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly candidate) => string.Equals(candidate.Location, Contract.AssemblyPath, StringComparison.OrdinalIgnoreCase) || string.Equals(candidate.GetName().Name, Contract.AssemblyName, StringComparison.Ordinal));
			if (assembly == null)
			{
				return;
			}
			Type type = assembly.GetType(Contract.RuntimeTypeName, throwOnError: false);
			Type type2 = assembly.GetType(Contract.SettingsTypeName, throwOnError: false);
			if (type == null || type2 == null)
			{
				return;
			}
			object current = ResolveInstance(type);
			if (current == null)
			{
				return;
			}
			PropertyInfo property = type2.GetProperty(Contract.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (property == null || !property.CanWrite)
			{
				return;
			}
			if (property.PropertyType.IsEnum)
			{
				ApplyEnum(current, property, assembly);
			}
			else
			{
				if (property.PropertyType != typeof(int))
				{
					return;
				}
				Action<int> action = ResolvePersister(type);
				Action<int> action2 = action ?? ((Action<int>)delegate(int value)
				{
					property.SetValue(current, value);
				});
				int num = ResolveBaseOffset(type2, property) + Index;
				if ((int)(property.GetValue(current) ?? ((object)0)) == num)
				{
					return;
				}
				int[] array = new int[3]
				{
					num,
					Index,
					Index + 1
				};
				foreach (int num2 in array)
				{
					if (num2 >= 0 && num2 <= Contract.Labels.Count + 1)
					{
						action2(num2);
						if ((int)(property.GetValue(current) ?? ((object)0)) == num2)
						{
							Refresh(assembly);
							LogOnce(num2, num);
							return;
						}
					}
				}
				ModLog.Warn($"无法让 {type2.Name}.{Contract.PropertyName} 切到变体「{Contract.Labels.ElementAtOrDefault(Index)}」；" + "该皮肤包的外观设置可能与预期不同。");
			}
		}

		private void ApplyEnum(object current, PropertyInfo property, Assembly assembly)
		{
			Type propertyType = property.PropertyType;
			object[] array = Enum.GetValues(propertyType).Cast<object>().ToArray();
			string[] names = Enum.GetNames(propertyType);
			object obj = null;
			for (int i = 0; i < names.Length; i++)
			{
				if (names[i].Equals(Contract.Labels[Index], StringComparison.OrdinalIgnoreCase))
				{
					obj = array[i];
					break;
				}
			}
			if (obj == null)
			{
				obj = ((Contract.EnumValues.Count == array.Length) ? Enum.ToObject(propertyType, Contract.EnumValues[Index]) : array.ElementAtOrDefault(Index));
			}
			if (obj != null && !object.Equals(property.GetValue(current), obj))
			{
				property.SetValue(current, obj);
				Refresh(assembly);
				LogOnce(Convert.ToInt32(obj, CultureInfo.InvariantCulture), Convert.ToInt32(obj, CultureInfo.InvariantCulture));
			}
		}

		private object? ResolveInstance(Type runtimeType)
		{
			try
			{
				if (Contract.InstanceMemberIsMethod)
				{
					return runtimeType.GetMethod(Contract.InstanceMemberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null)?.Invoke(null, null);
				}
				PropertyInfo property = runtimeType.GetProperty(Contract.InstanceMemberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				if (property != null)
				{
					return property.GetValue(null);
				}
				return runtimeType.GetField(Contract.InstanceMemberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
			}
			catch
			{
				return null;
			}
		}

		private Action<int>? ResolvePersister(Type runtimeType)
		{
			if (string.IsNullOrWhiteSpace(Contract.PersistMemberName))
			{
				return null;
			}
			try
			{
				if (runtimeType.GetProperty(Contract.PersistMemberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) is Action<int> result)
				{
					return result;
				}
				return runtimeType.GetField(Contract.PersistMemberName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null) as Action<int>;
			}
			catch
			{
				return null;
			}
		}

		private void LogOnce(int applied, int desired)
		{
			string item = Contract.SettingsTypeName + "\n" + applied;
			if (AppliedLogKeys.Add(item))
			{
				ModLog.Info($"已把 {Contract.SettingsTypeName} 的外观变体切到「{Contract.Labels.ElementAtOrDefault(Index)}」（值={applied}{((applied == desired) ? string.Empty : "，已按皮肤包自身的取值范围校正")}）。");
			}
		}

		private void Refresh(Assembly assembly)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null)
			{
				return;
			}
			if (_refreshMethod == null && !_refreshSearched)
			{
				_refreshSearched = true;
				Type[] types = assembly.GetTypes();
				for (int i = 0; i < types.Length; i++)
				{
					MethodInfo[] methods = types[i].GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
					foreach (MethodInfo methodInfo in methods)
					{
						if (!methodInfo.Name.StartsWith("Refresh", StringComparison.Ordinal) && !methodInfo.Name.StartsWith("Apply", StringComparison.Ordinal))
						{
							continue;
						}
						ParameterInfo[] parameters = methodInfo.GetParameters();
						if (parameters == null || parameters.Length != 1)
						{
							continue;
						}
						ParameterInfo parameterInfo = parameters[0];
						if (parameterInfo != null)
						{
							Type parameterType = parameterInfo.ParameterType;
							if (parameterType == typeof(SceneTree))
							{
								_refreshMethod = methodInfo;
								break;
							}
						}
					}
					if (_refreshMethod != null)
					{
						break;
					}
				}
			}
			try
			{
				_refreshMethod?.Invoke(null, new object[1] { val });
			}
			catch
			{
			}
		}

		private static int ResolveBaseOffset(Type settingsType, PropertyInfo property)
		{
			try
			{
				if (settingsType.GetConstructor(Type.EmptyTypes) != null)
				{
					object obj = Activator.CreateInstance(settingsType);
					if (obj != null && property.GetValue(obj) is int num)
					{
						return (num >= 1) ? 1 : 0;
					}
				}
			}
			catch
			{
			}
			return 1;
		}

		[CompilerGenerated]
		private SettingsVariantBinding(SettingsVariantBinding original)
		{
			Contract = original.Contract;
			Index = original.Index;
		}
	}

	internal sealed record SettingsVariantContract(string AssemblyPath, string AssemblyName, string RuntimeTypeName, string InstanceMemberName, bool InstanceMemberIsMethod, string SettingsTypeName, string PropertyName, bool PropertyIsEnum, IReadOnlyList<int> EnumValues, string? PersistMemberName, IReadOnlyList<string> Labels, int DefaultIndex);

	private static class SettingsVariantScanner
	{
		private sealed record TypeInfo(TypeDefinition Definition, string Name);

		private sealed record IntChannel(string SettingsTypeName, string PropertyName, bool IsEnum, string? EnumTypeName, int Score);

		private sealed record PersistChannel(string TypeName, string MemberName, string Suffix);

		private sealed record LabelsCandidate(int Score, string FieldName, MethodDefinitionHandle Cctor, FieldDefinitionHandle Field);

		private sealed class SignatureNames : ISignatureTypeProvider<string, MetadataReader?>, IConstructedTypeProvider<string>, ISZArrayTypeProvider<string>, ISimpleTypeProvider<string>
		{
			internal static readonly SignatureNames Instance = new SignatureNames();

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
				if (typeArguments.Length != 0)
				{
					return genericType + "<" + string.Join(",", typeArguments) + ">";
				}
				return genericType;
			}

			public string GetGenericMethodParameter(MetadataReader? genericContext, int index)
			{
				return "!!" + index;
			}

			public string GetGenericTypeParameter(MetadataReader? genericContext, int index)
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
				return typeCode switch
				{
					PrimitiveTypeCode.Int32 => "System.Int32", 
					PrimitiveTypeCode.String => "System.String", 
					PrimitiveTypeCode.Boolean => "System.Boolean", 
					PrimitiveTypeCode.Single => "System.Single", 
					PrimitiveTypeCode.Double => "System.Double", 
					PrimitiveTypeCode.Object => "System.Object", 
					PrimitiveTypeCode.Void => "System.Void", 
					_ => typeCode.ToString(), 
				};
			}

			public string GetSZArrayType(string elementType)
			{
				return elementType + "[]";
			}

			public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
			{
				return TypeName(reader, reader.GetTypeDefinition(handle));
			}

			public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
			{
				TypeReference typeReference = reader.GetTypeReference(handle);
				string @string = reader.GetString(typeReference.Name);
				string string2 = reader.GetString(typeReference.Namespace);
				if (!string.IsNullOrEmpty(string2))
				{
					return string2 + "." + @string;
				}
				return @string;
			}

			public string GetTypeFromSpecification(MetadataReader reader, MetadataReader? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
			{
				return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
			}
		}

		private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
			where field.FieldType == typeof(OpCode)
			select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

		private static readonly HashSet<string> GenericLabelWords = new HashSet<string>(StringComparer.Ordinal)
		{
			"names", "labels", "variants", "appearances", "palettes", "styles", "looks", "forms", "outfits", "costumes",
			"presets", "options", "modes", "colors", "colours", "themes"
		};

		private static readonly HashSet<string> AppearanceChannelWords = new HashSet<string>(StringComparer.Ordinal) { "appearance", "appearances", "variant", "palette", "outfit", "costume", "style", "look" };

		private static readonly HashSet<string> DenyChannelWords = new HashSet<string>(StringComparer.Ordinal)
		{
			"version", "revision", "rev", "count", "length", "size", "hash", "seed", "checksum", "enabled",
			"disabled", "flag", "flags", "max", "min", "limit", "timeout", "cache", "tick", "ticks",
			"ms", "fps", "scale", "offset", "alpha", "volume", "opacity", "speed", "delay", "duration",
			"width", "height", "depth", "level", "hp", "damage", "priority", "weight"
		};

		internal static SettingsVariantContract? Scan(string assemblyPath)
		{
			using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
			using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
			if (!pEReader.HasMetadata)
			{
				return null;
			}
			MetadataReader reader = pEReader.GetMetadataReader();
			TypeInfo[] types = (from handle in reader.TypeDefinitions
				select reader.GetTypeDefinition(handle) into type
				select new TypeInfo(type, TypeName(reader, type))).ToArray();
			List<IntChannel> list = CollectIntChannels(reader, types);
			if (list.Count == 0)
			{
				return null;
			}
			List<PersistChannel> source = CollectPersistChannels(reader, types);
			foreach (IntChannel channel2 in list.OrderByDescending((IntChannel channel) => channel.Score))
			{
				(string, string, bool)? tuple = FindInstanceProvider(reader, types, channel2);
				if (!tuple.HasValue)
				{
					continue;
				}
				IReadOnlyList<int> enumValues;
				IReadOnlyList<string> readOnlyList = ((channel2.IsEnum && channel2.EnumTypeName != null) ? ReadEnumLabels(reader, types, channel2.EnumTypeName, out enumValues) : FindStringLabels(pEReader, reader, types, channel2, source.Any((PersistChannel candidate) => candidate.Suffix.Equals(channel2.PropertyName, StringComparison.OrdinalIgnoreCase))));
				int count = readOnlyList.Count;
				if ((count >= 2 && count <= 16) || 1 == 0)
				{
					PersistChannel persistChannel = source.FirstOrDefault((PersistChannel candidate) => candidate.Suffix.Equals(channel2.PropertyName, StringComparison.OrdinalIgnoreCase));
					int enumDefaultIndex;
					int defaultIndex = ResolveDefaultIndex(pEReader, reader, types, channel2, readOnlyList.Count, out enumDefaultIndex);
					if (channel2.IsEnum)
					{
						defaultIndex = enumDefaultIndex;
					}
					string fullPath = Path.GetFullPath(assemblyPath);
					string? fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assemblyPath);
					string item = tuple.Value.Item1;
					string item2 = tuple.Value.Item2;
					bool item3 = tuple.Value.Item3;
					string settingsTypeName = channel2.SettingsTypeName;
					string propertyName = channel2.PropertyName;
					bool isEnum = channel2.IsEnum;
					IReadOnlyList<int> enumValues2;
					if (!channel2.IsEnum || channel2.EnumTypeName == null)
					{
						IReadOnlyList<int> readOnlyList2 = Array.Empty<int>();
						enumValues2 = readOnlyList2;
					}
					else
					{
						enumValues2 = ReadEnumValues(reader, types, channel2.EnumTypeName);
					}
					return new SettingsVariantContract(fullPath, fileNameWithoutExtension, item, item2, item3, settingsTypeName, propertyName, isEnum, enumValues2, persistChannel?.MemberName, readOnlyList, defaultIndex);
				}
			}
			return null;
		}

		private static List<IntChannel> CollectIntChannels(MetadataReader reader, TypeInfo[] types)
		{
			List<IntChannel> list = new List<IntChannel>();
			foreach (TypeInfo typeInfo in types)
			{
				foreach (PropertyDefinitionHandle property in typeInfo.Definition.GetProperties())
				{
					PropertyDefinition propertyDefinition = reader.GetPropertyDefinition(property);
					if (!HasSetter(reader, propertyDefinition, isStatic: false))
					{
						continue;
					}
					string @string = reader.GetString(propertyDefinition.Name);
					string returnType = Decode(reader, propertyDefinition).ReturnType;
					bool isEnum = false;
					string enumTypeName = null;
					if (!(returnType == "System.Int32"))
					{
						if (!types.Any((TypeInfo candidate) => candidate.Name.Equals(returnType, StringComparison.Ordinal) && candidate.Definition.BaseType.Kind == HandleKind.TypeReference && TypeReferenceName(reader, candidate.Definition.BaseType).Equals("System.Enum", StringComparison.Ordinal)))
						{
							continue;
						}
						isEnum = true;
						enumTypeName = returnType;
					}
					int num = ChannelHintScore(@string);
					if (num != 0 && !SplitWords(@string).Any((string word) => DenyChannelWords.Contains(word)))
					{
						list.Add(new IntChannel(typeInfo.Name, @string, isEnum, enumTypeName, num));
					}
				}
			}
			return list;
		}

		private static List<PersistChannel> CollectPersistChannels(MetadataReader reader, TypeInfo[] types)
		{
			List<PersistChannel> list = new List<PersistChannel>();
			foreach (TypeInfo typeInfo in types)
			{
				foreach (PropertyDefinitionHandle property in typeInfo.Definition.GetProperties())
				{
					PropertyDefinition propertyDefinition = reader.GetPropertyDefinition(property);
					if (HasSetter(reader, propertyDefinition, isStatic: true) && IsIntOrEnumDelegate(Decode(reader, propertyDefinition).ReturnType))
					{
						AddPersist(typeInfo, reader.GetString(propertyDefinition.Name), list);
					}
				}
				foreach (FieldDefinitionHandle field in typeInfo.Definition.GetFields())
				{
					FieldDefinition fieldDefinition = reader.GetFieldDefinition(field);
					if (fieldDefinition.Attributes.HasFlag(FieldAttributes.Static) && IsIntOrEnumDelegate(fieldDefinition.DecodeSignature(SignatureNames.Instance, null)))
					{
						AddPersist(typeInfo, reader.GetString(fieldDefinition.Name), list);
					}
				}
			}
			return list;
		}

		private static void AddPersist(TypeInfo type, string memberName, List<PersistChannel> channels)
		{
			string[] persistHints = PersistHints;
			foreach (string text in persistHints)
			{
				if (memberName.StartsWith(text, StringComparison.OrdinalIgnoreCase) && memberName.Length > text.Length)
				{
					string name = type.Name;
					int length = text.Length;
					channels.Add(new PersistChannel(name, memberName, memberName.Substring(length, memberName.Length - length)));
					break;
				}
			}
		}

		private static bool IsIntOrEnumDelegate(string typeName)
		{
			if (typeName.Contains("System.Action`1", StringComparison.Ordinal) || typeName.Contains("System.Func`1", StringComparison.Ordinal))
			{
				return typeName.Contains("System.Int32", StringComparison.Ordinal);
			}
			return false;
		}

		private static int ChannelHintScore(string propertyName)
		{
			IReadOnlyList<string> source = SplitWords(propertyName);
			for (int i = 0; i < ChannelHints.Length; i++)
			{
				string hint = ChannelHints[i];
				if (source.Any((string word) => MatchesWord(word, hint)))
				{
					return ChannelHints.Length - i;
				}
			}
			return 0;
		}

		private static bool MatchesWord(string word, string hint)
		{
			if (!word.Equals(hint, StringComparison.Ordinal) && !word.Equals(hint + "s", StringComparison.Ordinal))
			{
				return word + "s" == hint;
			}
			return true;
		}

		private static IReadOnlyList<string> SplitWords(string value)
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
			return list;
		}

		private static (string TypeName, string MemberName, bool IsMethod)? FindInstanceProvider(MetadataReader reader, TypeInfo[] types, IntChannel channel)
		{
			string[] instanceMemberHints = InstanceMemberHints;
			TypeInfo[] array;
			foreach (string text in instanceMemberHints)
			{
				array = types;
				foreach (TypeInfo typeInfo in array)
				{
					foreach (PropertyDefinitionHandle property in typeInfo.Definition.GetProperties())
					{
						PropertyDefinition propertyDefinition = reader.GetPropertyDefinition(property);
						if (HasGetter(reader, propertyDefinition, isStatic: true) && reader.GetString(propertyDefinition.Name).Equals(text, StringComparison.OrdinalIgnoreCase) && Decode(reader, propertyDefinition).ReturnType.Equals(channel.SettingsTypeName, StringComparison.Ordinal))
						{
							return (typeInfo.Name, text, false);
						}
					}
					foreach (FieldDefinitionHandle field in typeInfo.Definition.GetFields())
					{
						FieldDefinition fieldDefinition = reader.GetFieldDefinition(field);
						if (fieldDefinition.Attributes.HasFlag(FieldAttributes.Static) && reader.GetString(fieldDefinition.Name).Equals(text, StringComparison.OrdinalIgnoreCase) && fieldDefinition.DecodeSignature(SignatureNames.Instance, null).Equals(channel.SettingsTypeName, StringComparison.Ordinal))
						{
							return (typeInfo.Name, text, false);
						}
					}
					foreach (MethodDefinitionHandle method in typeInfo.Definition.GetMethods())
					{
						MethodDefinition methodDefinition = reader.GetMethodDefinition(method);
						if (methodDefinition.Attributes.HasFlag(MethodAttributes.Static) && reader.GetString(methodDefinition.Name).Equals(text, StringComparison.OrdinalIgnoreCase) && methodDefinition.GetParameters().Count == 0 && Decode(reader, methodDefinition).ReturnType.Equals(channel.SettingsTypeName, StringComparison.Ordinal))
						{
							return (typeInfo.Name, text, true);
						}
					}
				}
			}
			array = types;
			foreach (TypeInfo typeInfo2 in array)
			{
				foreach (PropertyDefinitionHandle property2 in typeInfo2.Definition.GetProperties())
				{
					PropertyDefinition propertyDefinition2 = reader.GetPropertyDefinition(property2);
					if (HasGetter(reader, propertyDefinition2, isStatic: true) && Decode(reader, propertyDefinition2).ReturnType.Equals(channel.SettingsTypeName, StringComparison.Ordinal))
					{
						return (typeInfo2.Name, reader.GetString(propertyDefinition2.Name), false);
					}
				}
			}
			return null;
		}

		private static IReadOnlyList<string> FindStringLabels(PEReader peReader, MetadataReader reader, TypeInfo[] types, IntChannel channel, bool hasPersist)
		{
			string propertyName = channel.PropertyName;
			HashSet<string> hashSet = SplitWords(propertyName).ToHashSet<string>(StringComparer.Ordinal);
			bool flag = hashSet.Any((string word) => AppearanceChannelWords.Contains(word));
			List<LabelsCandidate> list = new List<LabelsCandidate>();
			foreach (TypeInfo typeInfo in types)
			{
				MethodDefinitionHandle cctor = default(MethodDefinitionHandle);
				foreach (MethodDefinitionHandle method in typeInfo.Definition.GetMethods())
				{
					if (reader.GetString(reader.GetMethodDefinition(method).Name) == ".cctor")
					{
						cctor = method;
						break;
					}
				}
				if (cctor.IsNil)
				{
					continue;
				}
				foreach (FieldDefinitionHandle field in typeInfo.Definition.GetFields())
				{
					FieldDefinition fieldDefinition = reader.GetFieldDefinition(field);
					if (fieldDefinition.Attributes.HasFlag(FieldAttributes.Static) && IsStringSequence(fieldDefinition.DecodeSignature(SignatureNames.Instance, null)))
					{
						string @string = reader.GetString(fieldDefinition.Name);
						HashSet<string> hashSet2 = SplitWords(@string).ToHashSet<string>(StringComparer.Ordinal);
						bool flag2 = hashSet2.Overlaps(hashSet) && (flag || hasPersist);
						bool flag3 = hashSet2.Count == 1 && GenericLabelWords.Contains(hashSet2.First());
						if (flag2 || (flag3 && hasPersist && flag))
						{
							int score = LabelHintScore(@string) + (flag2 ? 40 : 0) + (@string.Contains(propertyName, StringComparison.OrdinalIgnoreCase) ? 20 : 0);
							list.Add(new LabelsCandidate(score, @string, cctor, field));
						}
					}
				}
			}
			foreach (LabelsCandidate item in list.OrderByDescending((LabelsCandidate candidate) => candidate.Score))
			{
				List<string> list2 = ReadStaticStringArray(peReader, reader, item.Cctor, item.Field);
				int i = list2.Count;
				if (i >= 2 && i <= 16)
				{
					return list2;
				}
			}
			return Array.Empty<string>();
		}

		private static bool IsStringSequence(string typeName)
		{
			switch (typeName)
			{
			case "System.String[]":
			case "System.Collections.Generic.List<System.String>":
			case "System.Collections.Generic.IReadOnlyList<System.String>":
			case "System.Collections.Generic.IEnumerable<System.String>":
				return true;
			default:
				return false;
			}
		}

		private static int LabelHintScore(string fieldName)
		{
			IReadOnlyList<string> source = SplitWords(fieldName);
			for (int i = 0; i < LabelFieldHints.Length; i++)
			{
				string hint = LabelFieldHints[i];
				if (source.Any((string word) => MatchesWord(word, hint)))
				{
					return LabelFieldHints.Length - i;
				}
			}
			return 0;
		}

		private static IReadOnlyList<string> ReadEnumLabels(MetadataReader reader, TypeInfo[] types, string enumTypeName, out IReadOnlyList<int> enumValues)
		{
			enumValues = Array.Empty<int>();
			TypeInfo typeInfo = types.FirstOrDefault((TypeInfo candidate) => candidate.Name.Equals(enumTypeName, StringComparison.Ordinal));
			if (typeInfo == null)
			{
				return Array.Empty<string>();
			}
			List<string> list = new List<string>();
			List<int> list2 = new List<int>();
			foreach (FieldDefinitionHandle field in typeInfo.Definition.GetFields())
			{
				FieldDefinition fieldDefinition = reader.GetFieldDefinition(field);
				if (fieldDefinition.Attributes.HasFlag(FieldAttributes.Literal))
				{
					string @string = reader.GetString(fieldDefinition.Name);
					if (!(@string == "value__"))
					{
						list.Add(@string);
						list2.Add(TryReadConstant(reader, fieldDefinition.GetDefaultValue()));
					}
				}
			}
			enumValues = list2;
			return list;
		}

		private static IReadOnlyList<int> ReadEnumValues(MetadataReader reader, TypeInfo[] types, string enumTypeName)
		{
			ReadEnumLabels(reader, types, enumTypeName, out IReadOnlyList<int> enumValues);
			return enumValues;
		}

		private static int TryReadConstant(MetadataReader reader, ConstantHandle handle)
		{
			try
			{
				if (handle.IsNil)
				{
					return 0;
				}
				Constant constant = reader.GetConstant(handle);
				BlobReader blobReader = reader.GetBlobReader(constant.Value);
				return constant.TypeCode switch
				{
					ConstantTypeCode.SByte => blobReader.ReadSByte(), 
					ConstantTypeCode.Byte => blobReader.ReadByte(), 
					ConstantTypeCode.Int16 => blobReader.ReadInt16(), 
					ConstantTypeCode.UInt16 => blobReader.ReadUInt16(), 
					ConstantTypeCode.Int32 => blobReader.ReadInt32(), 
					ConstantTypeCode.UInt32 => (int)blobReader.ReadUInt32(), 
					_ => 0, 
				};
			}
			catch
			{
				return 0;
			}
		}

		private static int ResolveDefaultIndex(PEReader peReader, MetadataReader reader, TypeInfo[] types, IntChannel channel, int labelCount, out int enumDefaultIndex)
		{
			enumDefaultIndex = -1;
			TypeInfo typeInfo = types.FirstOrDefault((TypeInfo candidate) => candidate.Name.Equals(channel.SettingsTypeName, StringComparison.Ordinal));
			if (typeInfo == null)
			{
				return -1;
			}
			List<int> list = new List<int>();
			foreach (MethodDefinitionHandle method in typeInfo.Definition.GetMethods())
			{
				MethodDefinition methodDefinition = reader.GetMethodDefinition(method);
				if (!(reader.GetString(methodDefinition.Name) != ".ctor") && !methodDefinition.Attributes.HasFlag(MethodAttributes.Static) && methodDefinition.GetParameters().Count == 0)
				{
					list.AddRange(ReadCtorAssignedInts(peReader, reader, methodDefinition, channel.PropertyName));
				}
			}
			if (list.Count == 0)
			{
				return -1;
			}
			int num = list[list.Count - 1];
			if (channel.IsEnum)
			{
				int num2 = ReadEnumValues(reader, types, channel.EnumTypeName).ToList().IndexOf(num);
				enumDefaultIndex = ((num2 >= 0 && num2 < 16) ? num2 : (-1));
				return enumDefaultIndex;
			}
			int num3 = ((num >= 1) ? 1 : 0);
			int num4 = num - num3;
			if (num4 < 0 || num4 >= labelCount)
			{
				return -1;
			}
			return num4;
		}

		private static IEnumerable<int> ReadCtorAssignedInts(PEReader peReader, MetadataReader reader, MethodDefinition method, string propertyName)
		{
			List<int> list = new List<int>();
			if (method.RelativeVirtualAddress == 0)
			{
				return list;
			}
			byte[] iLBytes = peReader.GetMethodBody(method.RelativeVirtualAddress).GetILBytes();
			if (iLBytes == null)
			{
				return list;
			}
			int num = int.MinValue;
			int num2 = 0;
			while (num2 < iLBytes.Length)
			{
				byte b = iLBytes[num2++];
				ushort key = ((b == 254 && num2 < iLBytes.Length) ? ((ushort)(0xFE00 | iLBytes[num2++])) : b);
				if (!OpCodesByValue.TryGetValue(key, out var value))
				{
					return list;
				}
				int num3 = num2;
				int operandSize = GetOperandSize(value.OperandType, iLBytes, num3);
				if (operandSize < 0 || num3 + operandSize > iLBytes.Length)
				{
					return list;
				}
				if (value.OperandType == OperandType.InlineField && operandSize == 4 && value.Equals(OpCodes.Stfld))
				{
					int rowNumber = BinaryPrimitives.ReadInt32LittleEndian(iLBytes.AsSpan(num3, 4));
					try
					{
						if (reader.GetString(reader.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(rowNumber)).Name).Contains(propertyName, StringComparison.OrdinalIgnoreCase) && num != int.MinValue)
						{
							list.Add(num);
						}
					}
					catch
					{
					}
				}
				else if (value.Equals(OpCodes.Ldc_I4_0))
				{
					num = 0;
				}
				else if (value.Equals(OpCodes.Ldc_I4_1))
				{
					num = 1;
				}
				else if (value.Equals(OpCodes.Ldc_I4_2))
				{
					num = 2;
				}
				else if (value.Equals(OpCodes.Ldc_I4_3))
				{
					num = 3;
				}
				else if (value.Equals(OpCodes.Ldc_I4_4))
				{
					num = 4;
				}
				else if (value.Equals(OpCodes.Ldc_I4_5))
				{
					num = 5;
				}
				else if (value.Equals(OpCodes.Ldc_I4_6))
				{
					num = 6;
				}
				else if (value.Equals(OpCodes.Ldc_I4_7))
				{
					num = 7;
				}
				else if (value.Equals(OpCodes.Ldc_I4_8))
				{
					num = 8;
				}
				else if ((value.Equals(OpCodes.Ldc_I4_S) || value.Equals(OpCodes.Ldc_I4)) && operandSize > 0)
				{
					num = (value.Equals(OpCodes.Ldc_I4_S) ? iLBytes[num3] : BinaryPrimitives.ReadInt32LittleEndian(iLBytes.AsSpan(num3, 4)));
				}
				num2 = num3 + operandSize;
			}
			return list;
		}

		private static List<string> ReadStaticStringArray(PEReader peReader, MetadataReader reader, MethodDefinitionHandle cctorHandle, FieldDefinitionHandle targetField)
		{
			List<string> result = new List<string>();
			MethodDefinition methodDefinition = reader.GetMethodDefinition(cctorHandle);
			if (methodDefinition.RelativeVirtualAddress == 0)
			{
				return result;
			}
			byte[] iLBytes = peReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
			if (iLBytes == null)
			{
				return result;
			}
			List<string> list = new List<string>();
			int num = -1;
			int num2 = -1;
			int num3 = 0;
			while (num3 < iLBytes.Length)
			{
				byte b = iLBytes[num3++];
				ushort key = ((b == 254 && num3 < iLBytes.Length) ? ((ushort)(0xFE00 | iLBytes[num3++])) : b);
				if (!OpCodesByValue.TryGetValue(key, out var value))
				{
					return new List<string>();
				}
				int num4 = num3;
				int operandSize = GetOperandSize(value.OperandType, iLBytes, num4);
				if (operandSize < 0 || num4 + operandSize > iLBytes.Length)
				{
					return new List<string>();
				}
				if (value.OperandType == OperandType.InlineString && operandSize == 4)
				{
					int num5 = BinaryPrimitives.ReadInt32LittleEndian(iLBytes.AsSpan(num4, 4));
					try
					{
						list.Add(reader.GetUserString(MetadataTokens.UserStringHandle(num5 & 0xFFFFFF)));
					}
					catch
					{
					}
				}
				else if (value.OperandType == OperandType.InlineField && operandSize == 4)
				{
					FieldDefinitionHandle fieldDefinitionHandle = MetadataTokens.FieldDefinitionHandle(BinaryPrimitives.ReadInt32LittleEndian(iLBytes.AsSpan(num4, 4)));
					if (value.Equals(OpCodes.Stsfld) && !fieldDefinitionHandle.IsNil && fieldDefinitionHandle == targetField)
					{
						int count = ((num > 0) ? Math.Min(num, list.Count) : list.Count);
						return (from text in list.Take(count)
							where !string.IsNullOrWhiteSpace(text)
							select text).ToList();
					}
					list.Clear();
				}
				else if (value.Equals(OpCodes.Newarr))
				{
					num = num2;
				}
				else if (value.Equals(OpCodes.Ldc_I4_0))
				{
					num2 = 0;
				}
				else if (value.Equals(OpCodes.Ldc_I4_1))
				{
					num2 = 1;
				}
				else if (value.Equals(OpCodes.Ldc_I4_2))
				{
					num2 = 2;
				}
				else if (value.Equals(OpCodes.Ldc_I4_3))
				{
					num2 = 3;
				}
				else if (value.Equals(OpCodes.Ldc_I4_4))
				{
					num2 = 4;
				}
				else if (value.Equals(OpCodes.Ldc_I4_5))
				{
					num2 = 5;
				}
				else if (value.Equals(OpCodes.Ldc_I4_6))
				{
					num2 = 6;
				}
				else if (value.Equals(OpCodes.Ldc_I4_7))
				{
					num2 = 7;
				}
				else if (value.Equals(OpCodes.Ldc_I4_8))
				{
					num2 = 8;
				}
				else if ((value.Equals(OpCodes.Ldc_I4_S) || value.Equals(OpCodes.Ldc_I4)) && operandSize > 0)
				{
					num2 = (value.Equals(OpCodes.Ldc_I4_S) ? iLBytes[num4] : BinaryPrimitives.ReadInt32LittleEndian(iLBytes.AsSpan(num4, 4)));
				}
				num3 = num4 + operandSize;
			}
			return new List<string>();
		}

		private static bool HasSetter(MetadataReader reader, PropertyDefinition property, bool isStatic)
		{
			return HasAccessor(reader, property.GetAccessors().Setter, isStatic);
		}

		private static bool HasGetter(MetadataReader reader, PropertyDefinition property, bool isStatic)
		{
			return HasAccessor(reader, property.GetAccessors().Getter, isStatic);
		}

		private static bool HasAccessor(MetadataReader reader, MethodDefinitionHandle handle, bool isStatic)
		{
			if (handle.IsNil)
			{
				return false;
			}
			return reader.GetMethodDefinition(handle).Attributes.HasFlag(MethodAttributes.Static) == isStatic;
		}

		private static MethodSignature<string> Decode(MetadataReader reader, PropertyDefinition property)
		{
			return property.DecodeSignature(SignatureNames.Instance, null);
		}

		private static MethodSignature<string> Decode(MetadataReader reader, MethodDefinition method)
		{
			return method.DecodeSignature(SignatureNames.Instance, null);
		}

		private static string TypeName(MetadataReader reader, TypeDefinition type)
		{
			string @string = reader.GetString(type.Name);
			string string2 = reader.GetString(type.Namespace);
			if (!string.IsNullOrEmpty(string2))
			{
				return string2 + "." + @string;
			}
			return @string;
		}

		private static string TypeReferenceName(MetadataReader reader, EntityHandle handle)
		{
			if (handle.Kind != HandleKind.TypeReference)
			{
				return string.Empty;
			}
			TypeReference typeReference = reader.GetTypeReference((TypeReferenceHandle)handle);
			string @string = reader.GetString(typeReference.Name);
			string string2 = reader.GetString(typeReference.Namespace);
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
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateCandidateAssemblies_003Ed__14 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string root;

		public string _003C_003E3__root;

		private string providerId;

		public string _003C_003E3__providerId;

		private IEnumerator<string> _003C_003E7__wrap1;

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
		public _003CEnumerateCandidateAssemblies_003Ed__14(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					string text;
					try
					{
						text = SkinPackagePaths.Resolve(root, providerId, ".dll");
					}
					catch
					{
						text = string.Empty;
					}
					if (!string.IsNullOrWhiteSpace(text) && File.Exists(text))
					{
						_003C_003E2__current = text;
						_003C_003E1__state = 1;
						return true;
					}
					if (!Directory.Exists(root))
					{
						return false;
					}
					_003C_003E7__wrap1 = Directory.EnumerateFiles(root, "*.dll", SearchOption.TopDirectoryOnly).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -1;
					return false;
				case 2:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					string current = _003C_003E7__wrap1.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CEnumerateCandidateAssemblies_003Ed__14 _003CEnumerateCandidateAssemblies_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateCandidateAssemblies_003Ed__ = this;
			}
			else
			{
				_003CEnumerateCandidateAssemblies_003Ed__ = new _003CEnumerateCandidateAssemblies_003Ed__14(0);
			}
			_003CEnumerateCandidateAssemblies_003Ed__.root = _003C_003E3__root;
			_003CEnumerateCandidateAssemblies_003Ed__.providerId = _003C_003E3__providerId;
			return _003CEnumerateCandidateAssemblies_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private const string VariantMarker = "::variant:";

	private const int MaximumVariants = 16;

	private static readonly string[] ChannelHints = new string[16]
	{
		"appearance", "appearances", "variant", "palette", "outfit", "costume", "style", "look", "form", "visual",
		"skin", "preset", "color", "colour", "theme", "model"
	};

	private static readonly string[] InstanceMemberHints = new string[9] { "Current", "Instance", "Settings", "Config", "Configuration", "State", "Data", "Runtime", "Defaults" };

	private static readonly string[] LabelFieldHints = new string[16]
	{
		"names", "labels", "variants", "appearances", "palettes", "styles", "looks", "forms", "outfits", "costumes",
		"presets", "colors", "colours", "themes", "options", "modes"
	};

	private static readonly string[] PersistHints = new string[6] { "Persist", "Set", "Save", "Apply", "Write", "Update" };

	private static readonly Dictionary<string, SettingsVariantBinding> Bindings = new Dictionary<string, SettingsVariantBinding>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, SettingsVariantContract?> Contracts = new Dictionary<string, SettingsVariantContract>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> AppliedLogKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	internal static bool IsSettingsVariant(string? optionId)
	{
		if (!string.IsNullOrWhiteSpace(optionId))
		{
			return Bindings.ContainsKey(optionId);
		}
		return false;
	}

	internal static void Expand(IReadOnlyCollection<PckResourceIndex> indexes, IDictionary<string, SkinGroup> groups)
	{
		foreach (PckResourceIndex index in indexes)
		{
			if (!index.Mod.HasDll || !Directory.Exists(index.Mod.RootPath))
			{
				continue;
			}
			(SkinGroup, SkinOption)[] array = (from candidate in groups.Values.SelectMany((SkinGroup @group) => from option in @group.Options
					where !option.Id.Contains("::variant:", StringComparison.OrdinalIgnoreCase) && !option.IsComposition && !option.IsCharacterIconOnly && option.EffectiveProviderId.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase)
					select (Group: @group, Option: option))
				where candidate.Option.Assets.Values.Any((ResourceAsset asset) => asset.SourcePath.StartsWith("res://scenes/creature_visuals/", StringComparison.OrdinalIgnoreCase))
				select candidate).ToArray();
			if (array.Length == 0)
			{
				continue;
			}
			SettingsVariantContract settingsVariantContract = ResolveContract(index.Mod.RootPath, index.Mod.ResourceNamespaceId);
			if (settingsVariantContract == null || settingsVariantContract.Labels.Count < 2)
			{
				continue;
			}
			(SkinGroup, SkinOption)[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				(SkinGroup, SkinOption) tuple = array2[i];
				SkinGroup item = tuple.Item1;
				SkinOption option2 = tuple.Item2;
				int num = item.Options.IndexOf(option2);
				if (num < 0 || item.Options.Any((SkinOption existing) => existing.Id.StartsWith(option2.Id + "::variant:", StringComparison.OrdinalIgnoreCase)))
				{
					continue;
				}
				int num2 = 0;
				int num3 = settingsVariantContract.DefaultIndex;
				if (num3 >= 0 && num3 < settingsVariantContract.Labels.Count)
				{
					item.Options[num] = option2 with
					{
						Name = option2.Name + "_" + settingsVariantContract.Labels[num3]
					};
					Bindings[option2.Id] = new SettingsVariantBinding(settingsVariantContract, num3);
				}
				else
				{
					num3 = -1;
				}
				for (int j = 0; j < settingsVariantContract.Labels.Count; j++)
				{
					if (j == num3)
					{
						continue;
					}
					string text = settingsVariantContract.Labels[j];
					if (!string.IsNullOrWhiteSpace(text))
					{
						string id = option2.Id + "::variant:" + j.ToString(CultureInfo.InvariantCulture);
						if (!item.Options.Any((SkinOption existing) => existing.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
						{
							item.Options.Insert(++num, option2 with
							{
								Id = id,
								Name = option2.Name + "_" + text,
								ProviderId = option2.EffectiveProviderId
							});
							Bindings[id] = new SettingsVariantBinding(settingsVariantContract, j);
							num2++;
						}
					}
				}
				if (num2 > 0 || num3 >= 0)
				{
					ModLog.Info($"已把 {option2.Name} 的 {settingsVariantContract.Labels.Count} 个外观变体注册为独立皮肤选项：名称={option2.Name}_{string.Join("、", settingsVariantContract.Labels)}" + ((num3 >= 0) ? ("（原选项即「" + settingsVariantContract.Labels[num3] + "」）") : string.Empty) + "；设置=" + settingsVariantContract.SettingsTypeName + "." + settingsVariantContract.PropertyName + "（" + settingsVariantContract.AssemblyPath + "）。");
				}
			}
		}
	}

	internal static void ApplyActiveSelections(IReadOnlyDictionary<string, string> selections)
	{
		if (Bindings.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> selection in selections)
		{
			if (Bindings.TryGetValue(selection.Value, out SettingsVariantBinding value))
			{
				try
				{
					value.Apply();
				}
				catch (Exception ex)
				{
					ModLog.Warn("切换 " + value.Contract.SettingsTypeName + " 的外观变体失败，保持游戏内当前外观：" + ex.GetBaseException().Message);
				}
			}
		}
	}

	internal static SettingsVariantContract? ScanAssemblyForDiagnostics(string assemblyPath)
	{
		if (!File.Exists(assemblyPath))
		{
			return null;
		}
		return SettingsVariantScanner.Scan(assemblyPath);
	}

	private static SettingsVariantContract? ResolveContract(string providerRoot, string providerId)
	{
		string key = providerRoot + "\n" + providerId;
		if (Contracts.TryGetValue(key, out SettingsVariantContract value))
		{
			return value;
		}
		SettingsVariantContract settingsVariantContract = null;
		try
		{
			foreach (string item in EnumerateCandidateAssemblies(providerRoot, providerId))
			{
				try
				{
					settingsVariantContract = SettingsVariantScanner.Scan(item);
				}
				catch (Exception ex)
				{
					ModLog.Warn("读取 " + Path.GetFileName(item) + " 的外观变体设置失败：" + ex.GetBaseException().Message);
				}
				if (settingsVariantContract != null)
				{
					break;
				}
			}
		}
		finally
		{
			Contracts[key] = settingsVariantContract;
		}
		return settingsVariantContract;
	}

	[IteratorStateMachine(typeof(_003CEnumerateCandidateAssemblies_003Ed__14))]
	private static IEnumerable<string> EnumerateCandidateAssemblies(string root, string providerId)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateCandidateAssemblies_003Ed__14(-2)
		{
			_003C_003E3__root = root,
			_003C_003E3__providerId = providerId
		};
	}
}

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
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Catalog;

internal static class DirectCharacterRuntimeTargetScanner
{
	private sealed record ScannerCacheEntry(long Length, DateTime LastWriteTimeUtc, IReadOnlyList<(string FieldName, string Value)> Declarations);

	private static readonly object CacheSync = new object();

	private static readonly Dictionary<string, ScannerCacheEntry> Cache = new Dictionary<string, ScannerCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode opCode) => (ushort)opCode.Value);

	public static IReadOnlySet<string> Scan(string? providerRoot, string providerId, IEnumerable<string> knownCharacterGroupIds)
	{
		if (string.IsNullOrWhiteSpace(providerRoot) || !Directory.Exists(providerRoot))
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		string text = SkinPackagePaths.Resolve(providerRoot, providerId, ".dll");
		object obj = ((!File.Exists(text)) ? ((object)Directory.EnumerateFiles(providerRoot, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((object)new string[1] { text }));
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string[] array = (string[])obj;
		foreach (string assemblyPath in array)
		{
			try
			{
				hashSet.UnionWith(ScanAssembly(assemblyPath, knownCharacterGroupIds));
			}
			catch (Exception)
			{
			}
		}
		return hashSet;
	}

	internal static IReadOnlySet<string> ScanAssembly(string assemblyPath, IEnumerable<string> knownCharacterGroupIds)
	{
		Dictionary<string, string> knownCharacterGroups = knownCharacterGroupIds.Where((string value) => !string.IsNullOrWhiteSpace(value)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToDictionary<string, string, string>((string value) => value, (string value) => value, StringComparer.OrdinalIgnoreCase);
		FileInfo fileInfo = new FileInfo(assemblyPath);
		IReadOnlyList<(string, string)> declarations;
		lock (CacheSync)
		{
			if (Cache.TryGetValue(assemblyPath, out ScannerCacheEntry value2) && value2.Length == fileInfo.Length && value2.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
			{
				declarations = value2.Declarations;
			}
			else
			{
				declarations = ReadDeclarations(assemblyPath, knownCharacterGroups);
				Cache[assemblyPath] = new ScannerCacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, declarations);
			}
		}
		return DirectCharacterRuntimeTargetPolicy.ResolveTargets(declarations, knownCharacterGroupIds);
	}

	private static IReadOnlyList<(string FieldName, string Value)> ReadDeclarations(string assemblyPath, IReadOnlyDictionary<string, string> knownCharacterGroups)
	{
		using FileStream peStream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
		using PEReader pEReader = new PEReader(peStream, PEStreamOptions.PrefetchEntireImage);
		if (!pEReader.HasMetadata)
		{
			return Array.Empty<(string, string)>();
		}
		MetadataReader metadataReader = pEReader.GetMetadataReader();
		Dictionary<FieldDefinitionHandle, string> dictionary = new Dictionary<FieldDefinitionHandle, string>();
		List<(string, string)> list = new List<(string, string)>();
		foreach (FieldDefinitionHandle fieldDefinition2 in metadataReader.FieldDefinitions)
		{
			FieldDefinition fieldDefinition = metadataReader.GetFieldDefinition(fieldDefinition2);
			string @string = metadataReader.GetString(fieldDefinition.Name);
			if (!DirectCharacterRuntimeTargetPolicy.IsTargetCharacterField(@string))
			{
				continue;
			}
			dictionary[fieldDefinition2] = @string;
			ConstantHandle defaultValue = fieldDefinition.GetDefaultValue();
			if (defaultValue.IsNil)
			{
				continue;
			}
			Constant constant = metadataReader.GetConstant(defaultValue);
			if (constant.TypeCode == ConstantTypeCode.String)
			{
				BlobReader blobReader = metadataReader.GetBlobReader(constant.Value);
				string text = blobReader.ReadUTF16(blobReader.Length).TrimEnd('\0');
				if (!string.IsNullOrWhiteSpace(text))
				{
					list.Add((@string, text));
				}
			}
		}
		if (dictionary.Count == 0)
		{
			foreach (MethodDefinitionHandle methodDefinition3 in metadataReader.MethodDefinitions)
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(methodDefinition3);
				if (methodDefinition.RelativeVirtualAddress != 0 && IsSkinPathDispatcher(metadataReader, methodDefinition))
				{
					byte[] iLBytes = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress).GetILBytes();
					if (iLBytes != null)
					{
						ReadCharacterStringLiterals(metadataReader, iLBytes, knownCharacterGroups, list);
					}
				}
			}
			return list.Distinct().ToArray();
		}
		foreach (MethodDefinitionHandle methodDefinition4 in metadataReader.MethodDefinitions)
		{
			MethodDefinition methodDefinition2 = metadataReader.GetMethodDefinition(methodDefinition4);
			if (methodDefinition2.RelativeVirtualAddress != 0)
			{
				byte[] iLBytes2 = pEReader.GetMethodBody(methodDefinition2.RelativeVirtualAddress).GetILBytes();
				if (iLBytes2 != null)
				{
					ReadStaticAssignments(metadataReader, iLBytes2, dictionary, list);
				}
			}
		}
		return list.Distinct().ToArray();
	}

	private static bool IsSkinPathDispatcher(MetadataReader reader, MethodDefinition method)
	{
		if (!method.Attributes.HasFlag(MethodAttributes.Static) || !reader.GetString(method.Name).Contains("SkinPath", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		BlobReader blobReader = reader.GetBlobReader(method.Signature);
		if (!blobReader.ReadSignatureHeader().IsGeneric && blobReader.ReadCompressedInteger() == 1 && blobReader.ReadSignatureTypeCode() == SignatureTypeCode.String)
		{
			return blobReader.ReadSignatureTypeCode() == SignatureTypeCode.String;
		}
		return false;
	}

	private static void ReadCharacterStringLiterals(MetadataReader reader, byte[] il, IReadOnlyDictionary<string, string> knownCharacterGroups, ICollection<(string FieldName, string Value)> declarations)
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
			if (!value.Equals(OpCodes.Ldstr) || operandSize != 4)
			{
				continue;
			}
			int num2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
			try
			{
				string userString = reader.GetUserString(MetadataTokens.UserStringHandle(num2 & 0xFFFFFF));
				if (knownCharacterGroups.TryGetValue(userString.Trim(), out string value2))
				{
					declarations.Add(("RuntimeSkinCharacterName", value2));
				}
			}
			catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
			{
			}
		}
	}

	private static void ReadStaticAssignments(MetadataReader reader, byte[] il, IReadOnlyDictionary<FieldDefinitionHandle, string> targetFields, ICollection<(string FieldName, string Value)> declarations)
	{
		string text = null;
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
			if (value.Equals(OpCodes.Ldstr) && operandSize == 4)
			{
				int num2 = BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4));
				try
				{
					text = reader.GetUserString(MetadataTokens.UserStringHandle(num2 & 0xFFFFFF));
				}
				catch (Exception ex) when (((ex is BadImageFormatException || ex is ArgumentException) ? 1 : 0) != 0)
				{
					text = null;
				}
			}
			else if (value.Equals(OpCodes.Stsfld) && operandSize == 4 && text != null)
			{
				EntityHandle entityHandle = MetadataTokens.EntityHandle(BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(num, 4)));
				if (entityHandle.Kind == HandleKind.FieldDefinition && targetFields.TryGetValue((FieldDefinitionHandle)entityHandle, out string value2))
				{
					declarations.Add((value2, text));
				}
				text = null;
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
}

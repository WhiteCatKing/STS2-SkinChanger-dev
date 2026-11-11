using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace STS2SkinChanger.Core;

internal static class WorkshopCodeInspection
{
	private static readonly Dictionary<short, OpCode> Codes = (from f in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where f.FieldType == typeof(OpCode)
		select (OpCode)f.GetValue(null)).ToDictionary((OpCode op) => op.Value);

	public static bool CanOmitExecution(string path)
	{
		try
		{
			using FileStream peStream = File.OpenRead(path);
			using PEReader pEReader = new PEReader(peStream);
			if (!pEReader.HasMetadata || pEReader.PEHeaders.CorHeader == null || (pEReader.PEHeaders.CorHeader.Flags & CorFlags.ILOnly) == 0)
			{
				return false;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			foreach (CustomAttributeHandle customAttribute in metadataReader.CustomAttributes)
			{
				if (MethodOwner(metadataReader, metadataReader.GetCustomAttribute(customAttribute).Constructor).StartsWith("HarmonyLib.", StringComparison.Ordinal))
				{
					return false;
				}
			}
			foreach (MethodDefinitionHandle methodDefinition2 in metadataReader.MethodDefinitions)
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(methodDefinition2);
				if ((methodDefinition.Attributes & (MethodAttributes.Virtual | MethodAttributes.Abstract | MethodAttributes.PinvokeImpl)) != 0)
				{
					return false;
				}
				if (methodDefinition.RelativeVirtualAddress == 0)
				{
					return false;
				}
				MethodBodyBlock methodBody = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress);
				if (methodBody.ExceptionRegions.Length != 0)
				{
					return false;
				}
				byte[] iLBytes = methodBody.GetILBytes();
				int num = 0;
				while (num < iLBytes.Length)
				{
					short num2 = iLBytes[num++];
					if (num2 == 254)
					{
						num2 = (short)(0xFE00 | iLBytes[num++]);
					}
					if (!Codes.TryGetValue(num2, out var value))
					{
						return false;
					}
					if (value == OpCodes.Call || value == OpCodes.Newobj)
					{
						EntityHandle handle = MetadataTokens.EntityHandle(BitConverter.ToInt32(iLBytes, num));
						num += 4;
						if (!OmittableCall(metadataReader, handle))
						{
							return false;
						}
					}
					else if (value == OpCodes.Ldstr)
					{
						num += 4;
					}
					else if (value != OpCodes.Nop && value != OpCodes.Ret && value != OpCodes.Pop && value != OpCodes.Dup && value != OpCodes.Ldarg_0 && value != OpCodes.Ldnull && value != OpCodes.Ldc_I4_0 && value != OpCodes.Ldc_I4_1)
					{
						return false;
					}
					if (num > iLBytes.Length)
					{
						return false;
					}
				}
			}
			return true;
		}
		catch (Exception ex) when (((ex is IOException || ex is BadImageFormatException || ex is ArgumentException || ex is IndexOutOfRangeException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	private static bool OmittableCall(MetadataReader reader, EntityHandle handle)
	{
		if (handle.Kind != HandleKind.MemberReference)
		{
			return false;
		}
		MemberReference memberReference = reader.GetMemberReference((MemberReferenceHandle)handle);
		string text = MethodOwner(reader, handle);
		string @string = reader.GetString(memberReference.Name);
		switch (text)
		{
		case "System.Object":
		case "System.Attribute":
			if (!(@string == ".ctor"))
			{
				break;
			}
			goto IL_00d6;
		case "System.Console":
			if (!(@string == "WriteLine"))
			{
				break;
			}
			goto IL_00d6;
		case "Godot.GD":
			if (!(@string == "Print"))
			{
				break;
			}
			goto IL_00d6;
		case "HarmonyLib.Harmony":
			if (!(@string == ".ctor") && !(@string == "PatchAll"))
			{
				break;
			}
			goto IL_00d6;
		case "System.Reflection.Assembly":
			{
				if (!(@string == "GetExecutingAssembly"))
				{
					break;
				}
				goto IL_00d6;
			}
			IL_00d6:
			return true;
		}
		return false;
	}

	private static string MethodOwner(MetadataReader reader, EntityHandle handle)
	{
		if (handle.Kind != HandleKind.MemberReference)
		{
			return "";
		}
		EntityHandle parent = reader.GetMemberReference((MemberReferenceHandle)handle).Parent;
		if (parent.Kind != HandleKind.TypeReference)
		{
			return "";
		}
		TypeReference typeReference = reader.GetTypeReference((TypeReferenceHandle)parent);
		return reader.GetString(typeReference.Namespace) + "." + reader.GetString(typeReference.Name);
	}
}

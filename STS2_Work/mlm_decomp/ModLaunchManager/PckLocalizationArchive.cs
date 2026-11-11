using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ModLaunchManager;

internal static class PckLocalizationArchive
{
	private sealed record Entry(string Path, long Offset, int Size, byte[] Hash);

	internal const int MaxFileBytes = 8388608;

	internal const int MaxTotalBytes = 67108864;

	internal const int MaxFiles = 2048;

	internal static readonly UTF8Encoding Utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	internal static Dictionary<string, byte[]> Read(string packPath, string modId)
	{
		ModTranslationFiles.ValidateModId(modId);
		using FileStream fileStream = File.Open(packPath, FileMode.Open, FileAccess.Read, FileShare.Read);
		using BinaryReader binaryReader = new BinaryReader(fileStream, Utf8, leaveOpen: true);
		if (binaryReader.ReadUInt32() != 1129333831)
		{
			throw new TranslationFileException("unsupported");
		}
		uint num = binaryReader.ReadUInt32();
		if (num - 2 > 1)
		{
			throw new TranslationFileException("unsupported");
		}
		binaryReader.ReadUInt32();
		binaryReader.ReadUInt32();
		binaryReader.ReadUInt32();
		if ((binaryReader.ReadUInt32() & 0xFFFFFFFDu) != 0)
		{
			throw new TranslationFileException("unsupported");
		}
		ulong num2 = binaryReader.ReadUInt64();
		if (num == 3)
		{
			ulong num3 = binaryReader.ReadUInt64();
			if (num3 < 40 || num3 > (ulong)(fileStream.Length - 4))
			{
				throw new TranslationFileException("invalidPack");
			}
			fileStream.Position = (long)num3;
		}
		else
		{
			fileStream.Position = 96L;
		}
		uint num4 = binaryReader.ReadUInt32();
		if (num4 > 1000000 || num4 > (ulong)(fileStream.Length - fileStream.Position) / 40uL)
		{
			throw new TranslationFileException("invalidPack");
		}
		string text = modId + "/localization/";
		List<Entry> list = new List<Entry>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		long num5 = 0L;
		for (uint num6 = 0u; num6 < num4; num6++)
		{
			uint num7 = binaryReader.ReadUInt32();
			if (num7 == 0 || num7 > 16384)
			{
				throw new TranslationFileException("invalidPack");
			}
			string text2 = Utf8.GetString(ReadExactly(binaryReader, (int)num7)).TrimEnd('\0');
			if (text2.StartsWith("res://", StringComparison.Ordinal))
			{
				text2 = text2.Substring(6);
			}
			ulong num8 = binaryReader.ReadUInt64();
			ulong num9 = binaryReader.ReadUInt64();
			byte[] hash = ReadExactly(binaryReader, 16);
			uint num10 = binaryReader.ReadUInt32();
			if (!text2.StartsWith(text, StringComparison.Ordinal) || !text2.EndsWith(".json", StringComparison.Ordinal))
			{
				continue;
			}
			switch (num10)
			{
			default:
				throw new TranslationFileException("unsupported");
			case 0u:
			{
				string text3 = ModTranslationFiles.ValidateRelativePath(text2.Substring(text.Length));
				if (!hashSet.Add(text3))
				{
					throw new TranslationFileException("invalidPack");
				}
				if (num9 > 8388608 || (num5 += (long)num9) > 67108864 || list.Count >= 2048)
				{
					throw new TranslationFileException("tooLarge");
				}
				ulong num11 = checked(num2 + num8);
				if (num11 > (ulong)fileStream.Length || num9 > (ulong)(fileStream.Length - (long)num11))
				{
					throw new TranslationFileException("invalidPack");
				}
				list.Add(new Entry(text3, (long)num11, (int)num9, hash));
				break;
			}
			case 2u:
				break;
			}
		}
		if (list.Count == 0)
		{
			throw new TranslationFileException("noFiles");
		}
		Dictionary<string, byte[]> dictionary = new Dictionary<string, byte[]>(StringComparer.Ordinal);
		foreach (Entry item in list)
		{
			fileStream.Position = item.Offset;
			byte[] array = ReadExactly(binaryReader, item.Size);
			if (item.Hash.Any((byte b) => b != 0) && !MD5.HashData(array).AsSpan().SequenceEqual(item.Hash))
			{
				throw new TranslationFileException("invalidPack");
			}
			Utf8.GetString(array);
			dictionary.Add(item.Path, array);
		}
		return dictionary;
	}

	private static byte[] ReadExactly(BinaryReader reader, int length)
	{
		byte[] array = reader.ReadBytes(length);
		if (array.Length != length)
		{
			throw new TranslationFileException("invalidPack");
		}
		return array;
	}
}

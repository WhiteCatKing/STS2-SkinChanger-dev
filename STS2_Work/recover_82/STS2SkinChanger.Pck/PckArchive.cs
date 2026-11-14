using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace STS2SkinChanger.Pck;

internal sealed class PckArchive : IDisposable
{
	private sealed record PckEntry(ulong Offset, ulong Size, byte[] Md5);

	private sealed class OutputEntry
	{
		private readonly byte[]? _bytes;

		private readonly PckArchive? _archive;

		private readonly string? _archivePath;

		public string Path { get; }

		public long Size { get; }

		public byte[] Md5 { get; }

		public long Offset { get; set; }

		private OutputEntry(string path, long size, byte[] md5, byte[]? bytes, PckArchive? archive, string? archivePath)
		{
			Path = path;
			Size = size;
			Md5 = md5;
			_bytes = bytes;
			_archive = archive;
			_archivePath = archivePath;
		}

		public static OutputEntry FromBytes(string path, byte[] bytes)
		{
			return new OutputEntry(path, bytes.LongLength, MD5.HashData(bytes), bytes, null, null);
		}

		public static OutputEntry FromArchive(string path, PckArchive archive, string archivePath)
		{
			return new OutputEntry(path, checked((long)archive.GetFileSize(archivePath)), archive.GetFileMd5(archivePath), null, archive, archivePath);
		}

		public void CopyTo(Stream destination)
		{
			if (_bytes != null)
			{
				destination.Write(_bytes);
			}
			else
			{
				_archive.CopyFileTo(_archivePath, destination);
			}
		}
	}

	private const uint HeaderMagic = 1129333831u;

	private const uint DirectoryEncrypted = 1u;

	private const uint RelativeFileBase = 2u;

	private const uint SparseBundle = 4u;

	private const uint FileEncrypted = 1u;

	private const uint FileRemoval = 2u;

	private readonly FileStream _stream;

	private readonly Dictionary<string, PckEntry> _entries;

	public string Path { get; }

	public IReadOnlyCollection<string> Paths => _entries.Keys;

	private PckArchive(string path, FileStream stream, Dictionary<string, PckEntry> entries)
	{
		Path = path;
		_stream = stream;
		_entries = entries;
	}

	public static PckArchive Open(string path, uint maxFileCount = 2000000u)
	{
		FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
		try
		{
			using BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8, leaveOpen: true);
			long position = fileStream.Position;
			if (binaryReader.ReadUInt32() != 1129333831)
			{
				throw new InvalidDataException(path + " 不是受支持的 Godot PCK 文件。");
			}
			uint num = binaryReader.ReadUInt32();
			if ((num < 2 || num > 4) ? true : false)
			{
				throw new InvalidDataException($"{path} 使用不支持的 PCK 格式版本 {num}。");
			}
			binaryReader.ReadUInt32();
			binaryReader.ReadUInt32();
			binaryReader.ReadUInt32();
			uint num2 = binaryReader.ReadUInt32();
			if ((num2 & 1) != 0)
			{
				throw new InvalidDataException(path + " 的 PCK 目录已加密，无法作为皮肤资源读取。");
			}
			if ((num2 & 4) != 0)
			{
				throw new InvalidDataException(path + " 使用了稀疏打包格式，暂不支持作为皮肤资源读取。");
			}
			ulong num3 = binaryReader.ReadUInt64();
			ulong fileBase = (((num2 & 2) != 0 || num >= 3) ? checked((ulong)position + num3) : num3);
			if (num >= 3)
			{
				long position2 = checked((long)((ulong)position + binaryReader.ReadUInt64()));
				fileStream.Position = position2;
			}
			else
			{
				fileStream.Position += 64L;
			}
			uint num4 = binaryReader.ReadUInt32();
			if (num4 > maxFileCount)
			{
				throw new InvalidDataException($"{path} 的 PCK 文件数量异常：{num4}。");
			}
			long position3 = fileStream.Position;
			Dictionary<string, PckEntry> entries;
			if (num == 2)
			{
				try
				{
					entries = ReadEntries(binaryReader, fileStream, path, fileBase, num4, includeFileFlags: true);
				}
				catch (Exception ex) when (((ex is InvalidDataException || ex is EndOfStreamException || ex is OverflowException) ? 1 : 0) != 0)
				{
					fileStream.Position = position3;
					entries = ReadEntries(binaryReader, fileStream, path, fileBase, num4, includeFileFlags: false);
				}
			}
			else
			{
				entries = ReadEntries(binaryReader, fileStream, path, fileBase, num4, includeFileFlags: true);
			}
			return new PckArchive(path, fileStream, entries);
		}
		catch
		{
			fileStream.Dispose();
			throw;
		}
	}

	private static Dictionary<string, PckEntry> ReadEntries(BinaryReader reader, FileStream stream, string path, ulong fileBase, uint fileCount, bool includeFileFlags)
	{
		Dictionary<string, PckEntry> dictionary = new Dictionary<string, PckEntry>(checked((int)fileCount), StringComparer.OrdinalIgnoreCase);
		for (uint num = 0u; num < fileCount; num++)
		{
			uint num2 = reader.ReadUInt32();
			if (num2 > 1048576)
			{
				throw new InvalidDataException(path + " 的 PCK 路径长度异常。");
			}
			byte[] array = reader.ReadBytes(checked((int)num2));
			if (array.Length != (int)num2)
			{
				throw new InvalidDataException(path + " 的 PCK 目录在读取路径时被截断。");
			}
			string text = Encoding.UTF8.GetString(array).TrimEnd('\0');
			if (string.IsNullOrWhiteSpace(text) || text.Any((char character) => char.IsControl(character)))
			{
				throw new InvalidDataException(path + " 的 PCK 目录包含无效资源路径。");
			}
			ulong num3 = reader.ReadUInt64();
			ulong num4 = reader.ReadUInt64();
			byte[] array2 = reader.ReadBytes(16);
			if (array2.Length != 16)
			{
				throw new InvalidDataException(path + " 的 PCK 目录在读取 " + text + " 校验和时被截断。");
			}
			if (includeFileFlags)
			{
				uint num5 = reader.ReadUInt32();
				if ((num5 & 2) != 0)
				{
					dictionary.Remove(NormalizePath(text));
					continue;
				}
				if ((num5 & 1) != 0)
				{
					throw new InvalidDataException(path + " 中的资源 " + text + " 已加密。");
				}
			}
			ulong num6 = checked(fileBase + num3);
			if (checked(num6 + num4) > (ulong)stream.Length)
			{
				throw new InvalidDataException(path + " 中的资源 " + text + " 超出文件范围。");
			}
			dictionary[NormalizePath(text)] = new PckEntry(num6, num4, array2);
		}
		return dictionary;
	}

	public bool Contains(string resourcePath)
	{
		return _entries.ContainsKey(NormalizePath(resourcePath));
	}

	public byte[] ReadFile(string resourcePath)
	{
		string text = NormalizePath(resourcePath);
		if (!_entries.TryGetValue(text, out PckEntry value))
		{
			throw new FileNotFoundException("PCK 中不存在资源 " + text + "。", text);
		}
		if (value.Size > int.MaxValue)
		{
			throw new InvalidDataException("资源 " + text + " 过大，无法载入内存。");
		}
		checked
		{
			byte[] array = new byte[(int)value.Size];
			lock (_stream)
			{
				_stream.Position = (long)value.Offset;
				_stream.ReadExactly(array);
				return array;
			}
		}
	}

	public ulong GetFileSize(string resourcePath)
	{
		return _entries[NormalizePath(resourcePath)].Size;
	}

	public byte[] GetFileMd5(string resourcePath)
	{
		return _entries[NormalizePath(resourcePath)].Md5;
	}

	public void CopyFileTo(string resourcePath, Stream destination)
	{
		PckEntry pckEntry = _entries[NormalizePath(resourcePath)];
		lock (_stream)
		{
			_stream.Position = checked((long)pckEntry.Offset);
			ulong num = pckEntry.Size;
			byte[] array = new byte[131072];
			while (num != 0)
			{
				int num2 = _stream.Read(array, 0, checked((int)Math.Min((ulong)array.Length, num)));
				if (num2 == 0)
				{
					throw new EndOfStreamException("读取 " + resourcePath + " 时意外到达 PCK 末尾。");
				}
				destination.Write(array, 0, num2);
				num -= checked((ulong)num2);
			}
		}
	}

	public void Dispose()
	{
		_stream.Dispose();
	}

	public static void Write(string outputPath, IReadOnlyDictionary<string, byte[]> files)
	{
		OutputEntry[] orderedFiles = files.Select<KeyValuePair<string, byte[]>, OutputEntry>((KeyValuePair<string, byte[]> pair) => OutputEntry.FromBytes(NormalizePath(pair.Key), pair.Value)).OrderBy<OutputEntry, string>((OutputEntry entry) => entry.Path, StringComparer.Ordinal).ToArray();
		WriteEntries(outputPath, orderedFiles);
	}

	public static void WriteFromArchives(string outputPath, IReadOnlyDictionary<string, (PckArchive Archive, string Path)> files)
	{
		OutputEntry[] orderedFiles = files.Select<KeyValuePair<string, (PckArchive, string)>, OutputEntry>((KeyValuePair<string, (PckArchive Archive, string Path)> pair) => OutputEntry.FromArchive(NormalizePath(pair.Key), pair.Value.Archive, pair.Value.Path)).OrderBy<OutputEntry, string>((OutputEntry entry) => entry.Path, StringComparer.Ordinal).ToArray();
		WriteEntries(outputPath, orderedFiles);
	}

	private static void WriteEntries(string outputPath, OutputEntry[] orderedFiles)
	{
		int num = orderedFiles.Sum((OutputEntry entry) => 4 + Align4(Encoding.UTF8.GetByteCount(entry.Path) + 1) + 8 + 8 + 16 + 4);
		long num2 = Align(100 + num, 16L);
		long num3 = num2;
		OutputEntry[] array = orderedFiles;
		foreach (OutputEntry outputEntry in array)
		{
			outputEntry.Offset = num3 - num2;
			num3 = Align(num3 + outputEntry.Size, 16L);
		}
		Directory.CreateDirectory(System.IO.Path.GetDirectoryName(outputPath));
		using FileStream output = File.Open(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
		using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.UTF8, leaveOpen: false);
		binaryWriter.Write(1129333831u);
		binaryWriter.Write(2u);
		binaryWriter.Write(4u);
		binaryWriter.Write(5u);
		binaryWriter.Write(1u);
		binaryWriter.Write(2u);
		binaryWriter.Write(checked((ulong)num2));
		for (int j = 0; j < 16; j++)
		{
			binaryWriter.Write(0u);
		}
		checked
		{
			binaryWriter.Write((uint)orderedFiles.Length);
			array = orderedFiles;
			foreach (OutputEntry outputEntry2 in array)
			{
				byte[] bytes = Encoding.UTF8.GetBytes(outputEntry2.Path);
				int num4 = Align4(unchecked(bytes.Length + 1));
				binaryWriter.Write((uint)num4);
				binaryWriter.Write(bytes);
				binaryWriter.Write(new byte[unchecked(num4 - bytes.Length)]);
				binaryWriter.Write((ulong)outputEntry2.Offset);
				binaryWriter.Write((ulong)outputEntry2.Size);
				binaryWriter.Write(outputEntry2.Md5);
				binaryWriter.Write(0u);
			}
			PadTo(binaryWriter, num2);
			array = orderedFiles;
		}
		foreach (OutputEntry outputEntry3 in array)
		{
			PadTo(binaryWriter, num2 + outputEntry3.Offset);
			outputEntry3.CopyTo(binaryWriter.BaseStream);
		}
	}

	private static string NormalizePath(string path)
	{
		string text = path.Replace('\\', '/').TrimStart('/');
		if (text.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			text = text2.Substring(6, text2.Length - 6);
		}
		while (text.Contains("//", StringComparison.Ordinal))
		{
			text = text.Replace("//", "/", StringComparison.Ordinal);
		}
		return "res://" + text.Trim('/');
	}

	private static int Align4(int value)
	{
		return (value + 3) & -4;
	}

	private static long Align(long value, long alignment)
	{
		return (value + alignment - 1) / alignment * alignment;
	}

	private static void PadTo(BinaryWriter writer, long position)
	{
		long num = position - writer.BaseStream.Position;
		if (num < 0)
		{
			throw new InvalidDataException("PCK 写入位置发生重叠。");
		}
		if (num > 0)
		{
			writer.Write(new byte[checked((int)num)]);
		}
	}
}

using System.IO.Compression;
using System.Text;
using Godot;

namespace STS2SkinChanger.Catalog;

// Godot's exported binary resources store dependencies and internal-resource offsets in
// separate tables. Rewrite those tables so a mounted card pack only needs private paths.
internal sealed class CardBinaryResource
{
    private readonly byte[] _bytes;
    private readonly long _metadataOffsetPosition;
    private readonly long _dependenciesPosition;
    private readonly long _internalTablePosition;
    private readonly bool _hasUids;
    private readonly string[] _dependencyTypes;
    private readonly string[] _dependencyPaths;

    internal IEnumerable<string> Dependencies(string sourcePath) =>
        _dependencyPaths.Select(path => ResolvePath(sourcePath, path));

    private CardBinaryResource(byte[] data, bool compressed)
    {
        _bytes = data;
        using var stream = new MemoryStream(data, writable: false);
        using var reader = new BinaryReader(stream);
        if (!compressed) stream.Position = 4;
        if (reader.ReadUInt32() != 0) throw new InvalidDataException("Unsupported binary card resource byte order.");
        _ = reader.ReadUInt32(); // Real number size
        var major = reader.ReadUInt32();
        _ = reader.ReadUInt32(); // Minor version
        var format = reader.ReadUInt32();
        if (major != 4 || format is < 4 or > 6)
            throw new InvalidDataException("Unsupported binary card resource format.");

        _ = ReadString(reader);
        _metadataOffsetPosition = stream.Position;
        _ = reader.ReadUInt64();
        var flags = reader.ReadUInt32();
        _hasUids = (flags & 2) != 0;
        _ = reader.ReadUInt64(); // Root UID
        if ((flags & 8) != 0) _ = ReadString(reader);
        stream.Position += 11 * sizeof(uint); // Reserved header fields
        var stringCount = ReadCount(reader);
        for (var i = 0; i < stringCount; i++) _ = ReadString(reader);

        var dependencyCount = ReadCount(reader);
        _dependenciesPosition = stream.Position;
        _dependencyTypes = new string[dependencyCount];
        _dependencyPaths = new string[dependencyCount];
        for (var i = 0; i < dependencyCount; i++)
        {
            _dependencyTypes[i] = ReadString(reader);
            _dependencyPaths[i] = ReadString(reader);
            if (_hasUids) _ = reader.ReadUInt64();
        }

        _internalTablePosition = stream.Position;
    }

    internal static CardBinaryResource? Parse(byte[] bytes)
    {
        if (bytes.AsSpan().StartsWith("RSRC"u8)) return new(bytes, compressed: false);
        if (bytes.AsSpan().StartsWith("RSCC"u8)) return new(Decompress(bytes), compressed: true);
        return null;
    }

    internal byte[] Rewrite(string sourcePath, IReadOnlyDictionary<string, string> aliases,
        IReadOnlyDictionary<string, string> payloadAliases)
    {
        var hadMagic = _bytes.AsSpan().StartsWith("RSRC"u8);
        using var input = new MemoryStream(_bytes, writable: false);
        using var output = new MemoryStream();
        using var reader = new BinaryReader(input);
        using var writer = new BinaryWriter(output);
        if (!hadMagic) writer.Write("RSRC"u8);
        writer.Write(_bytes.AsSpan(0, checked((int)_dependenciesPosition)));
        for (var i = 0; i < _dependencyPaths.Length; i++)
        {
            var path = ResolvePath(sourcePath, _dependencyPaths[i]);
            WriteString(writer, _dependencyTypes[i]);
            WriteString(writer, aliases.GetValueOrDefault(path) ?? payloadAliases.GetValueOrDefault(path) ?? path);
            if (_hasUids) writer.Write(-1L); // Force resolution through the private path.
        }

        input.Position = _internalTablePosition;
        var delta = output.Position - input.Position;
        var internalCount = ReadCount(reader);
        writer.Write(internalCount);
        for (var i = 0; i < internalCount; i++)
        {
            WriteString(writer, ReadString(reader));
            writer.Write(ShiftOffset(reader.ReadInt64(), delta));
        }
        input.CopyTo(output);

        input.Position = _metadataOffsetPosition;
        output.Position = _metadataOffsetPosition + (hadMagic ? 0 : 4);
        writer.Write(ShiftOffset(reader.ReadInt64(), delta));
        output.Position += sizeof(uint); // Flags
        writer.Write(-1L); // Do not publish the provider's root UID.
        return output.ToArray();
    }

    private long ShiftOffset(long offset, long delta) => offset == 0
        ? 0
        : offset < _internalTablePosition || offset >= _bytes.Length
            ? throw new InvalidDataException("Invalid binary card resource offset.")
            : offset + delta;

    private static string ResolvePath(string sourcePath, string dependency)
    {
        if (dependency.Contains("://", StringComparison.Ordinal)) return dependency;

        // Resolve a relative reference against the original logical resource before moving it.
        var relative = sourcePath[6..];
        var separator = relative.LastIndexOf('/');
        var parts = separator < 0 ? new List<string>() : relative[..separator].Split('/').ToList();
        foreach (var part in dependency.Split('/'))
        {
            if (part == ".." && parts.Count > 0) parts.RemoveAt(parts.Count - 1);
            else if (part is not ("" or "." or "..")) parts.Add(part);
        }
        return "res://" + string.Join('/', parts);
    }

    private static int ReadCount(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        if (count < 0 || count > (reader.BaseStream.Length - reader.BaseStream.Position) / sizeof(uint))
            throw new InvalidDataException("Invalid binary card resource table length.");
        return count;
    }

    private static string ReadString(BinaryReader reader)
    {
        var length = reader.ReadInt32();
        if (length < 0 || length > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new InvalidDataException("Invalid binary card resource string length.");
        return Encoding.UTF8.GetString(reader.ReadBytes(length)).TrimEnd('\0');
    }

    private static void WriteString(BinaryWriter writer, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        writer.Write(bytes.Length + 1);
        writer.Write(bytes);
        writer.Write((byte)0);
    }

    private static byte[] Decompress(byte[] bytes)
    {
        using var input = new MemoryStream(bytes, writable: false);
        using var reader = new BinaryReader(input);
        input.Position = 4;
        var mode = (Godot.FileAccess.CompressionMode)reader.ReadUInt32();
        var blockSize = reader.ReadInt32();
        var length = reader.ReadInt32();
        // Card resource containers hold metadata. Imported texture payloads go through the
        // normal Godot .ctex reader and never pass through this bounded decompressor.
        if (length <= 0 || length > 8 * 1024 * 1024 || blockSize <= 0 ||
            !Enum.IsDefined(mode))
            throw new InvalidDataException("Invalid compressed card resource header.");

        var blockCount = length / blockSize + 1;
        if (blockCount > (input.Length - input.Position) / sizeof(uint))
            throw new InvalidDataException("Invalid compressed card resource block table.");

        var blockSizes = new int[blockCount];
        for (var i = 0; i < blockCount; i++) blockSizes[i] = reader.ReadInt32();

        var result = new byte[length];
        var written = 0;
        foreach (var size in blockSizes)
        {
            if (size <= 0 || size > input.Length - input.Position)
                throw new InvalidDataException("Invalid compressed card resource block.");
            var bytesInBlock = reader.ReadBytes(size);
            var count = Math.Min(length - written, blockSize);
            if (count == 0) continue;
            if (mode == Godot.FileAccess.CompressionMode.Deflate)
            {
                using var block = new ZLibStream(new MemoryStream(bytesInBlock, writable: false), CompressionMode.Decompress);
                block.ReadExactly(result.AsSpan(written, count));
                if (block.ReadByte() != -1) throw new InvalidDataException("Invalid decompressed card resource block length.");
            }
            else
            {
                var block = bytesInBlock.Decompress(count, mode);
                if (block.Length != count) throw new InvalidDataException("Invalid decompressed card resource block length.");
                block.CopyTo(result, written);
            }
            written += count;
        }
        return result;
    }
}


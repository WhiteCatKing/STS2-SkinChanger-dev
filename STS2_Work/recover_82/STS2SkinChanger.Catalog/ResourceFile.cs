using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Catalog;

internal sealed record ResourceFile
{
	public string Path { get; }

	public byte[]? InlineBytes { get; }

	public PckArchive Archive => _archive ?? throw new InvalidOperationException("资源 " + Path + " 由卡图包内联提供，没有对应的 PCK 归档。");

	public bool IsInline => InlineBytes != null;

	public string ArchivePath
	{
		get
		{
			if (!IsInline)
			{
				return Archive.Path;
			}
			return "<inline-card-art-pack>";
		}
	}

	private readonly PckArchive? _archive;

	private const string InlineArchiveToken = "<inline-card-art-pack>";

	public ResourceFile(PckArchive archive, string path)
	{
		_archive = archive;
		Path = path;
	}

	public ResourceFile(string path, byte[] inlineBytes)
	{
		Path = path;
		InlineBytes = inlineBytes;
	}

	public byte[] Read()
	{
		return InlineBytes ?? Archive.ReadFile(Path);
	}

	public bool HasSameContentAs(ResourceFile other)
	{
		if ((object)this == other)
		{
			return true;
		}
		if (!SkinCatalog.NormalizeTakeoverPath(other.Path).Equals(SkinCatalog.NormalizeTakeoverPath(Path), StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (IsInline && other.IsInline)
		{
			return InlineBytes.AsSpan().SequenceEqual(other.InlineBytes);
		}
		byte[] array = Read();
		byte[] array2 = other.Read();
		if (array.Length == array2.Length)
		{
			return SHA256.HashData(array).AsSpan().SequenceEqual(SHA256.HashData(array2));
		}
		return false;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Path = ");
		builder.Append((object?)Path);
		builder.Append(", InlineBytes = ");
		builder.Append(InlineBytes);
		builder.Append(", Archive = ");
		builder.Append(Archive);
		builder.Append(", IsInline = ");
		builder.Append(IsInline.ToString());
		builder.Append(", ArchivePath = ");
		builder.Append((object?)ArchivePath);
		return true;
	}

	[CompilerGenerated]
	public override int GetHashCode()
	{
		return ((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<PckArchive>.Default.GetHashCode(_archive)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path)) * -1521134295 + EqualityComparer<byte[]>.Default.GetHashCode(InlineBytes);
	}

	[CompilerGenerated]
	public bool Equals(ResourceFile? other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<PckArchive>.Default.Equals(_archive, other._archive) && EqualityComparer<string>.Default.Equals(Path, other.Path))
			{
				return EqualityComparer<byte[]>.Default.Equals(InlineBytes, other.InlineBytes);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	private ResourceFile(ResourceFile original)
	{
		_archive = original._archive;
		Path = original.Path;
		InlineBytes = original.InlineBytes;
	}
}

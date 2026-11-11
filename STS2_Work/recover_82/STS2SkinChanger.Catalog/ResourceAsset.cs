using System;
using System.Collections.Generic;
using System.Linq;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Catalog;

internal sealed class ResourceAsset(string sourcePath)
{
	public string SourcePath { get; } = sourcePath;

	public List<ResourceFile> Files { get; } = new List<ResourceFile>();

	public void AddFile(PckArchive archive, string path)
	{
		if (Files.All((ResourceFile file) => !file.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
		{
			Files.Add(new ResourceFile(archive, path));
		}
	}

	public void AddInlineFile(string path, byte[] bytes)
	{
		if (Files.All((ResourceFile file) => !file.Path.Equals(path, StringComparison.OrdinalIgnoreCase)))
		{
			Files.Add(new ResourceFile(path, bytes));
		}
	}
}

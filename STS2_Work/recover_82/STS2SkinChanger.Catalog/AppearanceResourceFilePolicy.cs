using System;
using System.Collections.Generic;
using System.IO;

namespace STS2SkinChanger.Catalog;

internal static class AppearanceResourceFilePolicy
{
	private static readonly HashSet<string> Extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".png", ".jpg", ".jpeg", ".webp", ".bmp", ".tga", ".svg", ".dds", ".ktx", ".ctex",
		".tres", ".res", ".tscn", ".scn", ".escn", ".atlas", ".skel", ".spine", ".ogg", ".wav",
		".mp3", ".ttf", ".otf", ".fnt"
	};

	internal static bool IsAppearanceResourceFile(string path)
	{
		return Extensions.Contains(Path.GetExtension(path));
	}
}

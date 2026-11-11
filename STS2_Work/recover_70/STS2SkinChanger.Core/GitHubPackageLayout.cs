using System;
using System.Collections.Generic;
using System.IO;

namespace STS2SkinChanger.Core;

internal static class GitHubPackageLayout
{
	public static int SelectIndex(IReadOnlyList<string?> roots, string packageRoot, string repoName)
	{
		for (int i = 0; i < roots.Count; i++)
		{
			if (Same(roots[i], packageRoot))
			{
				return i;
			}
		}
		for (int j = 0; j < roots.Count; j++)
		{
			string text = roots[j];
			if (text != null && text.Length > 0 && Path.GetFileName(text.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).Equals(repoName, StringComparison.OrdinalIgnoreCase))
			{
				return j;
			}
		}
		if (roots.Count <= 0)
		{
			return -1;
		}
		return 0;
	}

	private static bool Same(string? left, string right)
	{
		if (left != null && left.Length > 0)
		{
			return string.Equals(left.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), right.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}
}

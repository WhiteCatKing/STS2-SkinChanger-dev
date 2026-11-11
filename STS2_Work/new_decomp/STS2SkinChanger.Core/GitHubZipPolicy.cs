using System;
using System.IO;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class GitHubZipPolicy
{
	public const int MaxEntries = 20000;

	public const long MaxEntryBytes = 268435456L;

	public const long MaxTotalBytes = 536870912L;

	public static GitHubZipEntry Classify(string entryName, long length, int index, long totalSoFar, out string relative)
	{
		relative = "";
		if (index >= 20000 || length > 268435456 || totalSoFar + length > 536870912)
		{
			return GitHubZipEntry.Reject;
		}
		string text = entryName.Replace('\\', '/');
		if (text.Length == 0 || text.EndsWith('/'))
		{
			return GitHubZipEntry.Skip;
		}
		if (text.StartsWith('/') || text.Contains(':'))
		{
			return GitHubZipEntry.Reject;
		}
		string[] array = text.Split('/');
		if (array.Any(delegate(string part)
		{
			bool flag = part.Length == 0;
			if (!flag)
			{
				bool flag2 = ((part == "." || part == "..") ? true : false);
				flag = flag2;
			}
			return flag;
		}) || array.Any((string part) => part.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) || text.Length > 240)
		{
			return GitHubZipEntry.Reject;
		}
		if (array[0].StartsWith("__MACOSX", StringComparison.OrdinalIgnoreCase) || array[0] == ".git" || array[^1].Equals(".DS_Store", StringComparison.OrdinalIgnoreCase))
		{
			return GitHubZipEntry.Skip;
		}
		relative = string.Join('/', array);
		return GitHubZipEntry.Accept;
	}
}

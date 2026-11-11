using System;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class GitHubModsPolicy
{
	public const string DisabledSuffix = ".disabled";

	public static bool IsModsFolderName(string? name)
	{
		if (name != null)
		{
			int length = name.Length;
			if (length > 0 && length <= 64 && !(name == ".") && !(name == ".."))
			{
				return name.All(delegate(char c)
				{
					bool flag = char.IsAsciiLetterOrDigit(c);
					if (!flag)
					{
						bool flag2 = ((c == '-' || c == '.' || c == '_') ? true : false);
						flag = flag2;
					}
					return flag;
				});
			}
		}
		return false;
	}

	public static bool IsLeftoverFolderName(string? name)
	{
		if (name != null && name.Length > 2 && name[0] == '.' && (name.EndsWith(".installing", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".previous", StringComparison.OrdinalIgnoreCase)))
		{
			return IsModsFolderName(name);
		}
		return false;
	}

	public static bool IsManifestName(string? fileName)
	{
		if (fileName != null && fileName.Length > 5)
		{
			return fileName.EndsWith(".json", StringComparison.Ordinal);
		}
		return false;
	}

	public static string DisabledManifestName(string packageId)
	{
		return packageId + ".json.disabled";
	}

	public static string DigestToken(string? digest)
	{
		string text = GitHubSkinPolicy.NormalizeDigest(digest);
		if (text.Length != 64)
		{
			return "";
		}
		return text.Substring(0, 8);
	}

	public static string FreshFolderName(string folder, string token)
	{
		if (!GitHubInstallPolicy.IsSafeToken(folder) || !GitHubInstallPolicy.IsSafeToken(token))
		{
			return "";
		}
		int num = 64 - token.Length - 1;
		if (num < 2)
		{
			return "";
		}
		string text = ((folder.Length > num) ? folder.Substring(0, num) : folder).TrimEnd('.') + "-" + token;
		if (!GitHubInstallPolicy.IsSafeToken(text))
		{
			return "";
		}
		return text;
	}

	public static bool CanDeleteFolder(string? folder, bool live, bool holdsManifest)
	{
		if (GitHubInstallPolicy.IsSafeToken(folder) && !live)
		{
			return !holdsManifest;
		}
		return false;
	}
}

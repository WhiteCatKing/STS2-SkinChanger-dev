using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class GitHubInstallPolicy
{
	public static string ModsDirectory(string executablePath)
	{
		string text = Path.GetDirectoryName(Path.GetFullPath(executablePath));
		if (Path.GetFileName(text).Equals("MacOS", StringComparison.OrdinalIgnoreCase))
		{
			text = Path.GetFullPath(Path.Combine(text, "..", "Resources"));
		}
		return Path.Combine(text, "mods");
	}

	public static string FolderName(string packageId, string repoName)
	{
		if (IsSafeToken(packageId))
		{
			return packageId;
		}
		string text = new string(repoName.Where(char.IsAsciiLetterOrDigit).ToArray());
		int length = text.Length;
		if (length <= 0 || length > 64)
		{
			return "Skin";
		}
		return text;
	}

	public static bool IsSafeToken(string? value)
	{
		if (value != null)
		{
			int length = value.Length;
			if (length > 0 && length <= 64 && value[0] != '.')
			{
				if (value[value.Length - 1] != '.')
				{
					return value.All(delegate(char c)
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
		}
		return false;
	}

	public static int CompareVersions(string? left, string? right)
	{
		int[] array = Segments(left);
		int[] array2 = Segments(right);
		if (array.Length == 0 || array2.Length == 0)
		{
			if (array.Length != array2.Length)
			{
				if (array.Length != 0)
				{
					return 1;
				}
				return -1;
			}
			return 0;
		}
		for (int i = 0; i < Math.Max(array.Length, array2.Length); i++)
		{
			int num = ((i < array.Length) ? array[i] : 0);
			int num2 = ((i < array2.Length) ? array2[i] : 0);
			if (num != num2)
			{
				return num.CompareTo(num2);
			}
		}
		return 0;
	}

	private static int[] Segments(string? version)
	{
		int result;
		return (from part in (version ?? "").Trim().TrimStart('v', 'V').Split('.', StringSplitOptions.RemoveEmptyEntries)
			select (!int.TryParse(new string(part.TakeWhile(char.IsAsciiDigit).ToArray()), out result)) ? (-1) : result).ToArray();
	}

	public static GitHubInstallState Classify(string? installedVersion, string? installedDigest, string? availableVersion, string? availableDigest)
	{
		if (string.IsNullOrWhiteSpace(installedVersion) && string.IsNullOrWhiteSpace(installedDigest))
		{
			return GitHubInstallState.NotInstalled;
		}
		string text = GitHubSkinPolicy.NormalizeDigest(installedDigest);
		string text2 = GitHubSkinPolicy.NormalizeDigest(availableDigest);
		if (text.Length > 0 && text2.Length > 0)
		{
			if (!text.Equals(text2, StringComparison.OrdinalIgnoreCase))
			{
				return GitHubInstallState.Updatable;
			}
			return GitHubInstallState.Installed;
		}
		if (string.IsNullOrWhiteSpace(availableVersion) || CompareVersions(installedVersion, availableVersion) >= 0)
		{
			return GitHubInstallState.Installed;
		}
		return GitHubInstallState.Updatable;
	}

	public static string BackfillDigest(string? installedVersion, string? installedDigest, string? availableVersion, string? availableDigest)
	{
		if (string.IsNullOrWhiteSpace(installedVersion) || GitHubSkinPolicy.NormalizeDigest(installedDigest).Length > 0)
		{
			return "";
		}
		string text = GitHubSkinPolicy.NormalizeDigest(availableDigest);
		if (text.Length <= 0 || CompareVersions(installedVersion, availableVersion) < 0)
		{
			return "";
		}
		return text;
	}

	public static bool CanReusePackage(string declaredDigest, long expectedSize, long cachedSize, string cachedDigest)
	{
		if (cachedSize < 0 || cachedSize != expectedSize)
		{
			return false;
		}
		string text = GitHubSkinPolicy.NormalizeDigest(declaredDigest);
		if (text.Length != 0)
		{
			return cachedDigest.Equals(text, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public static string DigestOfFile(string path)
	{
		try
		{
			using FileStream source = File.OpenRead(path);
			return Convert.ToHexString(SHA256.HashData(source)).ToLowerInvariant();
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return "";
		}
	}

	public static string? UninstallTarget(string modsDirectory, string folder, string packageId)
	{
		if (!IsSafeToken(folder))
		{
			return null;
		}
		string text = Path.Combine(modsDirectory, folder);
		if (!Directory.Exists(text))
		{
			return null;
		}
		if (packageId.Length != 0 && ReadManifestVersion(text, packageId) == null)
		{
			return null;
		}
		return text;
	}

	public static string? ReadManifestVersion(string modRoot, string packageId)
	{
		if (!IsSafeToken(packageId))
		{
			return null;
		}
		string text = Path.Combine(modRoot, packageId + ".json");
		try
		{
			if (!File.Exists(text) || new FileInfo(text).Length > 1048576)
			{
				return null;
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(text).TrimStart('\ufeff'));
			JsonElement value;
			return (jsonDocument.RootElement.ValueKind == JsonValueKind.Object && jsonDocument.RootElement.TryGetProperty("version", out value) && value.ValueKind == JsonValueKind.String) ? value.GetString() : null;
		}
		catch (Exception ex) when (((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return null;
		}
	}
}

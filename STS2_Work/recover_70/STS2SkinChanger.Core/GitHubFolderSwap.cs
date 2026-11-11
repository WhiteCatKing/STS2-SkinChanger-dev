using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class GitHubFolderSwap
{
	public static GitHubPlacement Place(string modsDirectory, string? recordedFolder, string canonicalFolder, string packageId, string digest, string sourceDirectory, Func<string, bool> isLoaded, Action<string> warn)
	{
		if (!GitHubInstallPolicy.IsSafeToken(canonicalFolder))
		{
			throw new InvalidOperationException("Invalid install folder name: " + canonicalFolder);
		}
		Directory.CreateDirectory(modsDirectory);
		string text = Owned(modsDirectory, recordedFolder, canonicalFolder, packageId);
		string text2 = text ?? canonicalFolder;
		string text3 = Path.Combine(modsDirectory, text2);
		if (text == null && !Owns(text3, packageId))
		{
			if (isLoaded(text3))
			{
				throw new InvalidOperationException("mods/" + text2 + " is in use and was not installed by this mod.");
			}
			if (HoldsManifest(text3))
			{
				throw new InvalidOperationException("mods/" + text2 + " belongs to another mod.");
			}
		}
		if (text == null || !isLoaded(text3))
		{
			try
			{
				Replace(modsDirectory, text3, sourceDirectory, warn);
				return new GitHubPlacement(text2, null);
			}
			catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
			{
				if (text == null)
				{
					throw;
				}
				warn("目录 mods/" + text2 + " 无法直接替换，改写进旁边的备用目录：" + ex.Message);
			}
		}
		if (!Disable(text3, warn))
		{
			throw new InvalidOperationException("Could not disable the loaded copy mods/" + text2 + ".");
		}
		string text4 = GitHubModsPolicy.FreshFolderName(canonicalFolder, GitHubModsPolicy.DigestToken(digest));
		if (text4.Length == 0)
		{
			throw new InvalidOperationException("Could not build a folder name for the new package.");
		}
		string text5 = Path.Combine(modsDirectory, text4);
		if (!isLoaded(text5))
		{
			Replace(modsDirectory, text5, sourceDirectory, warn);
		}
		return new GitHubPlacement(text4, text2);
	}

	public static bool Disable(string folder, Action<string> warn)
	{
		if (!Directory.Exists(folder))
		{
			return true;
		}
		string[] array = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).ToArray();
		foreach (string text in array)
		{
			if (ManifestId(text) != null)
			{
				try
				{
					File.Move(text, text + ".disabled", overwrite: true);
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
				{
					warn("无法停用清单 " + text + "：" + ex.Message);
				}
			}
		}
		return !HoldsManifest(folder);
	}

	public static bool HoldsManifest(string folder)
	{
		if (Directory.Exists(folder))
		{
			return Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Any((string file) => Path.GetFileName(file).EndsWith(".json", StringComparison.OrdinalIgnoreCase) && ManifestId(file) != null);
		}
		return false;
	}

	public static string[] Sweep(string modsDirectory, IEnumerable<(string FullName, string Folder)> pending, IReadOnlySet<string> liveFolders, Action<string> log)
	{
		List<string> list = new List<string>();
		foreach (string item in pending.Select<(string, string), string>(((string FullName, string Folder) entry) => entry.Folder).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (!GitHubInstallPolicy.IsSafeToken(item))
			{
				log("跳过删除 " + item + "：目录名不合法。");
				continue;
			}
			string text = Path.Combine(modsDirectory, item);
			if (!GitHubModsPolicy.CanDeleteFolder(item, liveFolders.Contains(item), HoldsManifest(text)))
			{
				log("跳过删除 mods/" + item + "：它不是在用目录就是已经重新装上了清单。");
			}
			else if (Directory.Exists(text))
			{
				try
				{
					Directory.Delete(text, recursive: true);
					log("已删除 GitHub 皮肤的旧目录：mods/" + item);
				}
				catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
				{
					list.Add(item);
					log("mods/" + item + " 仍被占用，留到下次启动再删：" + ex.Message);
				}
			}
		}
		return list.ToArray();
	}

	private static string? Owned(string modsDirectory, string? recordedFolder, string canonicalFolder, string packageId)
	{
		if (GitHubInstallPolicy.IsSafeToken(recordedFolder))
		{
			return recordedFolder;
		}
		if (packageId.Length == 0)
		{
			return null;
		}
		if (!Owns(Path.Combine(modsDirectory, canonicalFolder), packageId))
		{
			return null;
		}
		return canonicalFolder;
	}

	private static bool Owns(string folder, string packageId)
	{
		if (packageId.Length > 0 && Directory.Exists(folder))
		{
			if (!(from file in Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
				where Path.GetFileName(file).Equals(packageId + ".json", StringComparison.OrdinalIgnoreCase)
				select file).Any((string file) => string.Equals(ManifestId(file), packageId, StringComparison.OrdinalIgnoreCase)))
			{
				return File.Exists(Path.Combine(folder, GitHubModsPolicy.DisabledManifestName(packageId)));
			}
			return true;
		}
		return false;
	}

	private static string? ManifestId(string file)
	{
		try
		{
			if (new FileInfo(file).Length > 1048576)
			{
				return null;
			}
			using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(file).TrimStart('\ufeff'));
			if (jsonDocument.RootElement.ValueKind != JsonValueKind.Object || !jsonDocument.RootElement.TryGetProperty("id", out var value) || value.ValueKind != JsonValueKind.String)
			{
				return null;
			}
			string @string = value.GetString();
			return string.IsNullOrWhiteSpace(@string) ? null : @string;
		}
		catch (Exception ex) when (((ex is JsonException || ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return null;
		}
	}

	private static void Replace(string modsDirectory, string target, string source, Action<string> warn)
	{
		if (Directory.Exists(target))
		{
			string text = Path.Combine(modsDirectory, "." + Path.GetFileName(target) + ".previous");
			TryDelete(text);
			Directory.Move(target, text);
			Disable(text, warn);
			TryDelete(text);
		}
		CopyDirectory(source, target);
	}

	private static void CopyDirectory(string source, string destination)
	{
		Directory.CreateDirectory(destination);
		foreach (string item in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
		{
			string relativePath = Path.GetRelativePath(source, item);
			if (!relativePath.Split(Path.DirectorySeparatorChar).Any((string part) => part.StartsWith(".git", StringComparison.OrdinalIgnoreCase)))
			{
				string text = Path.Combine(destination, relativePath);
				Directory.CreateDirectory(Path.GetDirectoryName(text));
				File.Copy(item, text, overwrite: true);
			}
		}
	}

	private static void TryDelete(string directory)
	{
		try
		{
			if (Directory.Exists(directory))
			{
				Directory.Delete(directory, recursive: true);
			}
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
		}
	}
}

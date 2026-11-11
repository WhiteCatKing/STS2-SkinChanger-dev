using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class SkinChangerStorage : IDisposable
{
	private static readonly string[] LegacyConfigurationNames = new string[5] { "skin_changer.json", "skin_changer_theme.json", "skin_changer_theme_presets.json", "skin_changer_bundle_run_restore.json", "sts2_skin_switcher.json" };

	private readonly object _cacheLock = new object();

	private FileStream? _lease;

	private bool _closing;

	public string UserDirectory { get; }

	public string TempDirectory { get; }

	public string RootDirectory { get; }

	public string CacheDirectory { get; }

	public List<string> Warnings { get; } = new List<string>();

	public SkinChangerStorage(string userDirectory, string tempDirectory, string sessionId)
	{
		if (sessionId.Length == 0 || sessionId.Any((char c) => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_'))
		{
			throw new ArgumentException("Invalid cache session id.", "sessionId");
		}
		UserDirectory = Path.GetFullPath(userDirectory);
		TempDirectory = Path.GetFullPath(tempDirectory);
		RootDirectory = Path.Combine(UserDirectory, "Gurio.SkinChanger");
		CacheDirectory = Path.Combine(RootDirectory, "cache", sessionId);
	}

	public void MigrateConfiguration()
	{
		string[] legacyConfigurationNames = LegacyConfigurationNames;
		foreach (string text in legacyConfigurationNames)
		{
			if (MigrateFile(Path.Combine(UserDirectory, text), Path.Combine(RootDirectory, text)))
			{
				MigrateFile(Path.Combine(UserDirectory, text + ".bak"), Path.Combine(RootDirectory, text + ".bak"));
			}
		}
		string text2 = Path.Combine(UserDirectory, "skin_changer_bundle_runs");
		if (!SafePath(text2, UserDirectory) || !Directory.Exists(text2))
		{
			return;
		}
		foreach (string item in (from n in Directory.EnumerateFiles(text2).Select(Path.GetFileName)
			where n.EndsWith(".json", StringComparison.OrdinalIgnoreCase) || n.EndsWith(".json.bak", StringComparison.OrdinalIgnoreCase)
			select (!n.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) ? n : n.Substring(0, n.Length - 4)).Distinct())
		{
			string text3 = Path.Combine(RootDirectory, "skin_changer_bundle_runs", item);
			if (MigrateFile(Path.Combine(text2, item), text3))
			{
				MigrateFile(Path.Combine(text2, item + ".bak"), text3 + ".bak");
			}
		}
		try
		{
			if (!Directory.EnumerateFileSystemEntries(text2).Any())
			{
				Directory.Delete(text2);
			}
		}
		catch (Exception ex) when (IsIoError(ex))
		{
			Warnings.Add(ex.Message);
		}
	}

	private bool MigrateFile(string source, string destination)
	{
		if (!File.Exists(source))
		{
			return true;
		}
		try
		{
			if (!SafePath(source, UserDirectory) || !SafePath(destination, UserDirectory))
			{
				throw new IOException("Refusing linked configuration path: " + source);
			}
			if (File.Exists(destination))
			{
				destination = Path.Combine(RootDirectory, "migration-backup", Guid.NewGuid().ToString("N") + "_" + Path.GetFileName(source));
			}
			if (!SafePath(destination, UserDirectory))
			{
				throw new IOException("Refusing linked migration backup path: " + destination);
			}
			Directory.CreateDirectory(Path.GetDirectoryName(destination));
			File.Move(source, destination);
			return true;
		}
		catch (Exception ex) when (IsIoError(ex))
		{
			Warnings.Add("配置迁移保留旧文件：" + source + "；" + ex.Message);
			return false;
		}
	}

	public string ConfigurationFile(string name)
	{
		if (Path.GetFileName(name) != name || name.Length > 64 || !name.EndsWith(".json", StringComparison.Ordinal) || !name.All(delegate(char c)
		{
			bool flag = char.IsAsciiLetterOrDigit(c);
			if (!flag)
			{
				bool flag2 = ((c == '-' || c == '.' || c == '_') ? true : false);
				flag = flag2;
			}
			return flag;
		}))
		{
			throw new ArgumentException("Unsafe SC configuration name.", "name");
		}
		string text = Path.Combine(RootDirectory, name);
		string text2 = Path.Combine(UserDirectory, name);
		if (File.Exists(text) || File.Exists(text + ".bak") || (!File.Exists(text2) && !File.Exists(text2 + ".bak")))
		{
			return text;
		}
		return text2;
	}

	public string RunRecordFile(string name)
	{
		if (Path.GetFileName(name) != name || !name.EndsWith(".json", StringComparison.Ordinal))
		{
			throw new ArgumentException("Invalid run record.", "name");
		}
		string text = Path.Combine(RootDirectory, "skin_changer_bundle_runs", name);
		string text2 = Path.Combine(UserDirectory, "skin_changer_bundle_runs", name);
		if (File.Exists(text) || File.Exists(text + ".bak") || (!File.Exists(text2) && !File.Exists(text2 + ".bak")))
		{
			return text;
		}
		return text2;
	}

	public void StartCacheSession()
	{
		lock (_cacheLock)
		{
			if (!_closing && _lease == null)
			{
				if (!SafePath(CacheDirectory, UserDirectory))
				{
					throw new IOException("Unsafe SC cache directory.");
				}
				Directory.CreateDirectory(CacheDirectory);
				_lease = new FileStream(Path.Combine(CacheDirectory, ".active"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			}
		}
	}

	public bool WriteCache(string path, Action write)
	{
		lock (_cacheLock)
		{
			if (_closing)
			{
				return false;
			}
			if (!SafePath(path, CacheDirectory))
			{
				throw new IOException("Cache write outside the current SC session.");
			}
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			write();
			return true;
		}
	}

	public void StopCacheWrites()
	{
		lock (_cacheLock)
		{
			_closing = true;
			_lease?.Dispose();
			_lease = null;
		}
	}

	public CacheCleanupReport CleanCaches()
	{
		CacheCleanupReport report = new CacheCleanupReport(UserDirectory, TempDirectory);
		Attempt(delegate
		{
			CleanSessionRoot(Path.Combine(RootDirectory, "cache"), report);
		});
		Attempt(delegate
		{
			CleanSessionRoot(Path.Combine(TempDirectory, "Gurio.SkinChanger", "runtime"), report);
		});
		Attempt(delegate
		{
			CleanSessionRoot(Path.Combine(TempDirectory, "Gurio.SkinChanger", "online"), report);
		});
		Attempt(delegate
		{
			CleanDirectory(Path.Combine(UserDirectory, "skin_changer_workshop"), report);
		});
		Attempt(delegate
		{
			if (Directory.Exists(UserDirectory))
			{
				string[] array = new string[2] { "sts2_skin_overlay_*.pck", "sts2_skin_provider_namespace_*.pck" };
				foreach (string searchPattern in array)
				{
					foreach (string item in Directory.EnumerateFiles(UserDirectory, searchPattern))
					{
						CleanFile(item, report);
					}
				}
			}
		});
		return report;
		void Attempt(Action clean)
		{
			try
			{
				clean();
			}
			catch (Exception ex) when (IsIoError(ex))
			{
				report.Errors.Add(ex.Message);
			}
		}
	}

	private void CleanSessionRoot(string root, CacheCleanupReport report)
	{
		if (!SafePath(root, root.StartsWith(UserDirectory, StringComparison.Ordinal) ? UserDirectory : TempDirectory) || !Directory.Exists(root))
		{
			return;
		}
		foreach (string item in Directory.EnumerateDirectories(root))
		{
			if (!SafePath(item, root) || (item == CacheDirectory && _lease != null))
			{
				continue;
			}
			string path = Path.Combine(item, ".active");
			if (File.Exists(path))
			{
				try
				{
					using (new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
					{
					}
				}
				catch (IOException)
				{
					continue;
				}
				catch (UnauthorizedAccessException)
				{
					continue;
				}
			}
			CleanDirectory(item, report);
		}
	}

	private void CleanDirectory(string directory, CacheCleanupReport report)
	{
		if (!IsCacheTarget(directory, directory: true) || !Directory.Exists(directory))
		{
			return;
		}
		foreach (string item in Directory.EnumerateDirectories(directory))
		{
			if (SafePath(item, directory))
			{
				CleanDirectory(item, report);
			}
		}
		foreach (string item2 in Directory.EnumerateFiles(directory))
		{
			CleanFile(item2, report);
		}
		try
		{
			Directory.Delete(directory, recursive: false);
		}
		catch (Exception e) when (IsIoError(e))
		{
			report.Directories.Add(directory);
		}
	}

	private void CleanFile(string file, CacheCleanupReport report)
	{
		if (!IsCacheTarget(file, directory: false))
		{
			return;
		}
		try
		{
			File.Delete(file);
			report.RemovedFiles++;
		}
		catch (Exception e) when (IsIoError(e))
		{
			report.Files.Add(file);
		}
	}

	public bool IsCacheTarget(string path, bool directory)
	{
		path = Path.GetFullPath(path);
		string[] array = new string[4]
		{
			Path.Combine(RootDirectory, "cache"),
			Path.Combine(UserDirectory, "skin_changer_workshop"),
			Path.Combine(TempDirectory, "Gurio.SkinChanger", "runtime"),
			Path.Combine(TempDirectory, "Gurio.SkinChanger", "online")
		};
		foreach (string text in array)
		{
			if (path == text && text.EndsWith("skin_changer_workshop", StringComparison.Ordinal) && SafePath(path, UserDirectory))
			{
				return true;
			}
			if (SafePath(path, text) && SafePath(text, text.StartsWith(UserDirectory, StringComparison.Ordinal) ? UserDirectory : TempDirectory))
			{
				return true;
			}
		}
		string fileName = Path.GetFileName(path);
		if (!directory && Path.GetDirectoryName(path) == UserDirectory && SafePath(path, UserDirectory) && fileName.EndsWith(".pck", StringComparison.Ordinal))
		{
			if (!fileName.StartsWith("sts2_skin_overlay_", StringComparison.Ordinal))
			{
				return fileName.StartsWith("sts2_skin_provider_namespace_", StringComparison.Ordinal);
			}
			return true;
		}
		return false;
	}

	internal static bool SafePath(string path, string root)
	{
		path = Path.GetFullPath(path);
		root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
		StringComparison comparisonType = (OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		if (!path.StartsWith(root + Path.DirectorySeparatorChar, comparisonType))
		{
			return false;
		}
		for (string text = path; text != null; text = Path.GetDirectoryName(text))
		{
			if ((File.Exists(text) || Directory.Exists(text)) && (File.GetAttributes(text) & FileAttributes.ReparsePoint) != 0)
			{
				return false;
			}
			if (text.Equals(root, comparisonType))
			{
				break;
			}
		}
		return true;
	}

	private static bool IsIoError(Exception e)
	{
		if (e is IOException || e is UnauthorizedAccessException)
		{
			return true;
		}
		return false;
	}

	public void Dispose()
	{
		StopCacheWrites();
	}
}

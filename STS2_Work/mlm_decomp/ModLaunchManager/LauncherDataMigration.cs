using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModLaunchManager;

internal sealed class LauncherDataMigration
{
	internal const string FolderName = "ModLaunchManager";

	internal const string PlanFileName = "mod_launch_last_plan.json";

	internal const string WorkshopFolderName = "workshop_branches";

	internal const string BackupFolderName = "migration_backups";

	internal static readonly string[] FileNames = new string[6] { "mod_launch_manager_settings.json", "mod_launch_options.json", "mod_launch_organization.json", "mod_launch_profiles.json", "mod_friend_import_pending.json", "mod_launch_cloud_sync.json" };

	internal static readonly string[] DirectoryNames = new string[2] { "mod_launch_translations", "mod_list_exports" };

	private readonly string _userRoot;

	private readonly HashSet<string> _warnings = new HashSet<string>(StringComparer.Ordinal);

	private readonly Action<string>? _warn;

	internal string Root { get; }

	internal LauncherDataMigration(string userRoot, Action<string>? warn = null)
	{
		_userRoot = Path.GetFullPath(userRoot);
		Root = Path.Combine(_userRoot, "ModLaunchManager");
		_warn = warn;
	}

	internal void MigrateAll()
	{
		string[] fileNames = FileNames;
		foreach (string text in fileNames)
		{
			TryMove(text, text);
			MigrateCompanions(text, text);
		}
		fileNames = DirectoryNames;
		foreach (string text2 in fileNames)
		{
			TryMove(text2, text2);
		}
		fileNames = new string[2] { "steam", "default" };
		foreach (string text3 in fileNames)
		{
			string path = Path.Combine(_userRoot, text3);
			try
			{
				CheckParents(path);
				if (!Directory.Exists(path) || IsLink(path))
				{
					continue;
				}
				foreach (string item in Directory.EnumerateDirectories(path))
				{
					if (!IsLink(item))
					{
						MigrateAccount(Path.Combine(text3, Path.GetFileName(item)));
					}
				}
			}
			catch (Exception ex)
			{
				Warn(text3, ex);
			}
		}
	}

	internal string FilePath(string name, bool readOnly = false)
	{
		if (!FileNames.Contains<string>(name, StringComparer.Ordinal))
		{
			throw new ArgumentException("Unknown launcher file.", "name");
		}
		return Resolve(name, name, readOnly);
	}

	internal string DirectoryPath(string name, bool readOnly = false)
	{
		if (!DirectoryNames.Contains<string>(name, StringComparer.Ordinal))
		{
			throw new ArgumentException("Unknown launcher folder.", "name");
		}
		return Resolve(name, name, readOnly, directory: true);
	}

	internal string CloudRoot()
	{
		foreach (string item in FileNames.Where((string name) => name != "mod_friend_import_pending.json"))
		{
			Resolve(item, item, readOnly: false);
			Resolve(item + ".before-cloud.json", item + ".before-cloud.json", readOnly: false);
			Resolve(item + ".tmp", item + ".tmp", readOnly: false);
		}
		return Root;
	}

	internal string AccountPlanPath(string legacyPath, bool readOnly = false)
	{
		string text = Path.Combine(AccountFor(legacyPath), "mod_launch_last_plan.json");
		return Resolve(text, Path.Combine("accounts", text), readOnly);
	}

	internal string AccountWorkshopPath(string legacyPlanPath)
	{
		string text = Path.Combine(AccountFor(legacyPlanPath), "workshop_branches");
		return Resolve(text, Path.Combine("accounts", text), readOnly: false, directory: true);
	}

	private string AccountFor(string legacyPlanPath)
	{
		string relativePath = Path.GetRelativePath(_userRoot, Path.GetFullPath(legacyPlanPath));
		global::_003C_003Ey__InlineArray2<char> buffer = default(global::_003C_003Ey__InlineArray2<char>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<char>, char>(ref buffer, 0) = Path.DirectorySeparatorChar;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<char>, char>(ref buffer, 1) = Path.AltDirectorySeparatorChar;
		string[] array = relativePath.Split(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<char>, char>(in buffer, 2));
		bool flag = array.Length != 3;
		if (!flag)
		{
			string text = array[0];
			bool flag2 = ((text == "." || text == "..") ? true : false);
			flag = flag2;
		}
		bool flag3 = flag;
		if (!flag3)
		{
			string text = array[1];
			bool flag2 = ((text == "." || text == "..") ? true : false);
			flag3 = flag2;
		}
		if (flag3 || !string.Equals(array[2], "mod_launch_last_plan.json", StringComparison.Ordinal))
		{
			throw new ArgumentException("Launch-plan path is outside a game account.", "legacyPlanPath");
		}
		Child(_userRoot, relativePath);
		return Path.Combine(array[0], array[1]);
	}

	private void MigrateAccount(string account)
	{
		string[] array = new string[2] { "mod_launch_last_plan.json", "workshop_branches" };
		foreach (string path in array)
		{
			string text = Path.Combine(account, path);
			TryMove(text, Path.Combine("accounts", text));
		}
		string text2 = Path.Combine(account, "mod_launch_last_plan.json");
		MigrateCompanions(text2, Path.Combine("accounts", text2));
	}

	private void MigrateCompanions(string oldRelative, string newRelative)
	{
		string path = Child(_userRoot, oldRelative);
		string directoryName = Path.GetDirectoryName(path);
		try
		{
			CheckParents(path);
			if (!Directory.Exists(directoryName))
			{
				return;
			}
			string fileName = Path.GetFileName(path);
			string[] files = Directory.GetFiles(directoryName);
			for (int i = 0; i < files.Length; i++)
			{
				string fileName2 = Path.GetFileName(files[i]);
				bool num = fileName2.StartsWith(fileName + ".", StringComparison.Ordinal);
				int num3;
				if (fileName2.Length == fileName.Length + 38 && fileName2.StartsWith("." + fileName + ".", StringComparison.Ordinal) && fileName2.EndsWith(".tmp", StringComparison.Ordinal))
				{
					string text = fileName2;
					int num2 = fileName.Length + 2;
					num3 = (Guid.TryParseExact(text.Substring(num2, text.Length - 4 - num2), "N", out var _) ? 1 : 0);
				}
				else
				{
					num3 = 0;
				}
				bool flag = (byte)num3 != 0;
				if (num || flag)
				{
					TryMove(Path.Combine(Path.GetDirectoryName(oldRelative) ?? "", fileName2), Path.Combine(Path.GetDirectoryName(newRelative) ?? "", fileName2));
				}
			}
		}
		catch (Exception ex)
		{
			Warn(oldRelative, ex);
		}
	}

	private string Resolve(string oldRelative, string newRelative, bool readOnly, bool directory = false)
	{
		string text = Child(_userRoot, oldRelative);
		string text2 = Child(Root, newRelative);
		try
		{
			Move(text, text2, oldRelative);
			EnsureParent(text2);
			if (IsLink(text2))
			{
				throw new IOException("Launcher data target is a link: " + text2);
			}
			return text2;
		}
		catch (Exception ex)
		{
			Warn(oldRelative, ex);
			if (!readOnly)
			{
				throw new IOException("Could not move launcher data into its folder. The existing data has been kept.", ex);
			}
			return ReadableTarget(text2, directory) ? text2 : text;
		}
	}

	private bool ReadableTarget(string target, bool directory)
	{
		try
		{
			CheckParents(target);
			return !IsLink(target) && (directory ? Directory.Exists(target) : File.Exists(target));
		}
		catch
		{
			return false;
		}
	}

	private void TryMove(string oldRelative, string newRelative)
	{
		try
		{
			Move(Child(_userRoot, oldRelative), Child(Root, newRelative), oldRelative);
		}
		catch (Exception ex)
		{
			Warn(oldRelative, ex);
		}
	}

	private void Move(string source, string target, string oldRelative)
	{
		CheckParents(source);
		CheckParents(target);
		if (IsLink(target))
		{
			throw new IOException("Launcher data target is a link: " + target);
		}
		if (!File.Exists(source) && !Directory.Exists(source))
		{
			return;
		}
		bool flag = Directory.Exists(source);
		if (!File.Exists(target) && !Directory.Exists(target))
		{
			EnsureParent(target);
			if (flag)
			{
				Directory.Move(source, target);
			}
			else
			{
				File.Move(source, target);
			}
			return;
		}
		if (flag != Directory.Exists(target))
		{
			throw new IOException("Launcher data destination has the wrong file type: " + target);
		}
		if (flag && Directory.Exists(target) && !IsLink(source) && !IsLink(target))
		{
			string[] fileSystemEntries = Directory.GetFileSystemEntries(source);
			foreach (string text in fileSystemEntries)
			{
				string fileName = Path.GetFileName(text);
				Move(text, Path.Combine(target, fileName), Path.Combine(oldRelative, fileName));
			}
			Directory.Delete(source);
			return;
		}
		string text2 = Child(Path.Combine(Root, "migration_backups"), oldRelative);
		if (File.Exists(text2) || Directory.Exists(text2))
		{
			text2 = text2 + "." + Guid.NewGuid().ToString("N");
		}
		EnsureParent(text2);
		if (flag)
		{
			Directory.Move(source, text2);
		}
		else
		{
			File.Move(source, text2);
		}
	}

	private void EnsureParent(string path)
	{
		CheckParents(path);
		Directory.CreateDirectory(Path.GetDirectoryName(path));
	}

	private void CheckParents(string path)
	{
		string directoryName = Path.GetDirectoryName(path);
		while (directoryName != null && !SamePath(directoryName, _userRoot))
		{
			if (IsLink(directoryName))
			{
				throw new IOException("Launcher data contains a linked parent folder: " + directoryName);
			}
			directoryName = Path.GetDirectoryName(directoryName);
		}
		if (directoryName == null)
		{
			throw new IOException("Launcher data path is outside the game user folder.");
		}
	}

	private static string Child(string root, string relative)
	{
		string fullPath = Path.GetFullPath(Path.Combine(root, relative));
		string value = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root)) + Path.DirectorySeparatorChar;
		if (!fullPath.StartsWith(value, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
		{
			throw new ArgumentException("Launcher data path is outside its folder.", "relative");
		}
		return fullPath;
	}

	private static bool IsLink(string path)
	{
		try
		{
			return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
		}
		catch (FileNotFoundException)
		{
			return false;
		}
		catch (DirectoryNotFoundException)
		{
			return false;
		}
	}

	private static bool SamePath(string left, string right)
	{
		return string.Equals(Path.TrimEndingDirectorySeparator(left), Path.TrimEndingDirectorySeparator(right), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
	}

	private void Warn(string path, Exception ex)
	{
		string text = "launcher data migration deferred for " + path + ": " + ex.Message;
		if (_warnings.Add(text))
		{
			_warn?.Invoke(text);
		}
	}
}

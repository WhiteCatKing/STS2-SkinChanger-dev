using System;
using System.IO;
using Godot;

namespace ModLaunchManager;

internal static class LauncherDataPaths
{
	private static readonly object Gate = new object();

	private static string? _userRoot;

	private static LauncherDataMigration? _migration;

	private static LauncherDataMigration Migration
	{
		get
		{
			lock (Gate)
			{
				string fullPath = Path.GetFullPath(ProjectSettings.GlobalizePath("user://"));
				if (_migration == null || !string.Equals(_userRoot, fullPath, StringComparison.Ordinal))
				{
					_userRoot = fullPath;
					_migration = new LauncherDataMigration(fullPath, delegate(string message)
					{
						GD.PushWarning("[ModLaunchManager] " + message);
					});
					_migration.MigrateAll();
				}
				return _migration;
			}
		}
	}

	internal static string CloudRoot => Migration.CloudRoot();

	internal static string File(string name)
	{
		return Migration.FilePath(name);
	}

	internal static string ReadFile(string name)
	{
		return Migration.FilePath(name, readOnly: true);
	}

	internal static string Directory(string name)
	{
		return Migration.DirectoryPath(name);
	}

	internal static string ReadDirectory(string name)
	{
		return Migration.DirectoryPath(name, readOnly: true);
	}

	internal static string AccountPlan(string legacyPath)
	{
		return Migration.AccountPlanPath(legacyPath);
	}

	internal static string ReadAccountPlan(string legacyPath)
	{
		return Migration.AccountPlanPath(legacyPath, readOnly: true);
	}

	internal static string AccountWorkshop(string legacyPlanPath)
	{
		return Migration.AccountWorkshopPath(legacyPlanPath);
	}
}

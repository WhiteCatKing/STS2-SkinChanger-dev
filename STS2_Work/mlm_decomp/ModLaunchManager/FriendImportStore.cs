using System;
using System.IO;
using System.Text;
using System.Text.Json;
using Godot;

namespace ModLaunchManager;

internal static class FriendImportStore
{
	private const string FileName = "mod_friend_import_pending.json";

	private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	private static string Path => LauncherDataPaths.File("mod_friend_import_pending.json");

	internal static PendingFriendImport? Load()
	{
		try
		{
			string path = LauncherDataPaths.ReadFile("mod_friend_import_pending.json");
			if (!File.Exists(path))
			{
				return null;
			}
			PendingFriendImport pendingFriendImport = JsonSerializer.Deserialize<PendingFriendImport>(File.ReadAllText(path, Encoding.UTF8), Options);
			if (pendingFriendImport == null || pendingFriendImport.Version != 1 || pendingFriendImport.List == null || !FriendPresenceCodec.TryValidate(pendingFriendImport.List, out FriendSyncError _))
			{
				return null;
			}
			return pendingFriendImport;
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not read pending friend import: " + ex.Message);
			return null;
		}
	}

	internal static bool Save(PendingFriendImport pending, out string? error)
	{
		error = null;
		string text = null;
		try
		{
			string path = Path;
			text = path + ".tmp";
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
			File.WriteAllText(text, JsonSerializer.Serialize(pending, Options), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			File.Move(text, path, overwrite: true);
			return true;
		}
		catch (Exception ex)
		{
			error = ex.Message;
			try
			{
				if (File.Exists(text))
				{
					File.Delete(text);
				}
			}
			catch
			{
			}
			return false;
		}
	}

	internal static void Clear()
	{
		try
		{
			if (File.Exists(Path))
			{
				File.Delete(Path);
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not clear pending friend import: " + ex.Message);
		}
	}
}

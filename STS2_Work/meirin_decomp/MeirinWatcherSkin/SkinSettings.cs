using System;
using System.Collections.Generic;
using CznCore.Presentation;
using CznCore.Settings;
using Godot;

namespace MeirinWatcherSkin;

internal static class SkinSettings
{
	private const string ModId = "MeirinWatcherSkin";

	private const string CharacterId = "meirin";

	private const string ConfigDirectoryPath = "user://CznModConfig/MeirinWatcherSkin";

	private const string LiveDefaultMigrationMarkerPath = "user://CznModConfig/MeirinWatcherSkin/v0.0.3-live-default.migrated";

	private static readonly string ConfigPath = CharacterSkinSettingsStore.BuildConfigPath("MeirinWatcherSkin");

	internal static CharacterSkinSettingsStore Store { get; } = CreateStore();

	private static CharacterSkinSettingsStore CreateStore()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		bool num = !FileAccess.FileExists(ConfigPath) || !FileAccess.FileExists("user://CznModConfig/MeirinWatcherSkin/v0.0.3-live-default.migrated");
		CharacterSkinSettingsStore val = new CharacterSkinSettingsStore(ConfigPath, "meirin", (Action<string, int>)SkinLog.Warn, (Func<CharacterSkinSettingsStore, string, bool>)null, (Func<CharacterSkinSettingsStore, string, string, bool>)null, (Action<CharacterSkinSettingsStore>)null, 0.2f, (IReadOnlyList<string>)null);
		if (num)
		{
			val.SetCharacterSelectBackgroundVariant("meirin", (CharacterSelectBackgroundVariant)1, true);
			WriteLiveDefaultMigrationMarker();
		}
		return val;
	}

	private static void WriteLiveDefaultMigrationMarker()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		try
		{
			Error val = DirAccess.MakeDirRecursiveAbsolute("user://CznModConfig/MeirinWatcherSkin");
			if ((int)val != 0 && (long)val != 32)
			{
				throw new InvalidOperationException("Could not create settings directory: " + ((object)(Error)(ref val)/*cast due to .constrained prefix*/).ToString());
			}
			FileAccess val2 = FileAccess.Open("user://CznModConfig/MeirinWatcherSkin/v0.0.3-live-default.migrated", (ModeFlags)2);
			try
			{
				if (val2 == null)
				{
					throw new InvalidOperationException("Could not open migration marker for writing.");
				}
				val2.StoreString("v0.0.3\n");
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Meirin Live Lobby default migration marker failed: " + ex.Message);
		}
	}

	public static SkinCharacterSettings ForCharacter(string characterId)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return new SkinCharacterSettings(Store, characterId);
	}
}

using System.IO;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal static class CharacterSkinBundleRunStore
{
	internal static CharacterSkinBundleRunState? LoadMatching(string path, string identity)
	{
		if (!File.Exists(path))
		{
			return null;
		}
		CharacterSkinBundleRunState characterSkinBundleRunState;
		try
		{
			characterSkinBundleRunState = JsonSerializer.Deserialize<CharacterSkinBundleRunState>(File.ReadAllText(path));
		}
		catch (JsonException) when (File.Exists(path + ".bak"))
		{
			characterSkinBundleRunState = JsonSerializer.Deserialize<CharacterSkinBundleRunState>(File.ReadAllText(path + ".bak"));
		}
		if (characterSkinBundleRunState == null || characterSkinBundleRunState.Version != 1 || characterSkinBundleRunState.Cards == null || characterSkinBundleRunState.Monsters == null)
		{
			throw new InvalidDataException("皮肤包对局记录格式无效。");
		}
		if (!(characterSkinBundleRunState.RunIdentity == identity))
		{
			return null;
		}
		return characterSkinBundleRunState;
	}

	internal static void Save(string path, CharacterSkinBundleRunState state)
	{
		string text = JsonSerializer.Serialize(state);
		if (File.Exists(path) && File.ReadAllText(path) == text)
		{
			return;
		}
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		string text2 = path + ".tmp";
		using (FileStream fileStream = new FileStream(text2, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			JsonSerializer.Serialize(fileStream, state);
			fileStream.Flush(flushToDisk: true);
		}
		File.Move(text2, path, overwrite: true);
		try
		{
			File.Copy(path, path + ".bak", overwrite: true);
		}
		catch (IOException ex)
		{
			ModLog.Warn("皮肤包对局记录已保存，但备份失败：" + ex.Message);
		}
	}
}

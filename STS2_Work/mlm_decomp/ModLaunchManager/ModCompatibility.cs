using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModCompatibility
{
	internal static ModGameVersionCheck CheckGameVersion(Mod mod)
	{
		return CheckMinimumGameVersion(mod.manifest?.minGameVersion);
	}

	private static ModGameVersionCheck CheckMinimumGameVersion(string? requiredRaw)
	{
		string text = Normalize(requiredRaw);
		if (text == null)
		{
			return new ModGameVersionCheck(ModGameVersionCompatibility.NotDeclared, null, null);
		}
		SemanticVersion val = default(SemanticVersion);
		if (!SemanticVersion.TryFromString(text, ref val) || val == null)
		{
			return new ModGameVersionCheck(ModGameVersionCompatibility.InvalidMinimumVersion, text, null);
		}
		string text2 = ReadCurrentGameVersion();
		SemanticVersion val2 = default(SemanticVersion);
		if (text2 == null || !SemanticVersion.TryFromString(text2, ref val2) || val2 == null)
		{
			return new ModGameVersionCheck(ModGameVersionCompatibility.CurrentVersionUnavailable, text, text2);
		}
		return new ModGameVersionCheck((val2.CompareTo(val) >= 0) ? ModGameVersionCompatibility.Compatible : ModGameVersionCompatibility.Unsupported, ((object)val).ToString(), ((object)val2).ToString());
	}

	private static string? ReadCurrentGameVersion()
	{
		try
		{
			ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
			return Normalize((releaseInfo != null) ? releaseInfo.Version : null);
		}
		catch
		{
			return null;
		}
	}

	private static string? Normalize(string? raw)
	{
		if (string.IsNullOrWhiteSpace(raw))
		{
			return null;
		}
		string text = raw.Trim();
		if (!text.StartsWith('v'))
		{
			return text;
		}
		return text.Substring(1);
	}
}

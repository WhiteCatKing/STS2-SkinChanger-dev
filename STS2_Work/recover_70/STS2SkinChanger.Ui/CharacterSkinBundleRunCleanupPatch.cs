using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(RunManager), "CleanUp")]
internal static class CharacterSkinBundleRunCleanupPatch
{
	[HarmonyPriority(800)]
	private static void Prefix()
	{
		try
		{
			SkinService.RestoreCharacterSkinBundleAfterRun();
		}
		catch (Exception ex)
		{
			ModLog.Error("退出对局时恢复皮肤包预设失败，将在下次启动重试：" + ex);
		}
	}
}

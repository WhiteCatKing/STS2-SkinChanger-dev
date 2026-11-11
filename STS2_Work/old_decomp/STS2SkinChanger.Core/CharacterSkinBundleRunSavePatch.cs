using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Saves;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(SaveManager), "SaveRun")]
internal static class CharacterSkinBundleRunSavePatch
{
	private static void Prefix()
	{
		try
		{
			SkinService.SaveCharacterSkinBundleRunPresets();
		}
		catch (Exception ex)
		{
			ModLog.Error("保存本局皮肤包外观失败，不阻止游戏存档：" + ex);
		}
	}
}

using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCharacterSelectScreen), "OnSubmenuOpened")]
internal static class RandomCharacterLobbySkinPatch
{
	[HarmonyPriority(800)]
	private static void Prefix()
	{
		try
		{
			SkinService.RestoreRandomCharacterSkinsForLobby();
		}
		catch (Exception ex)
		{
			ModLog.Error("进入选角时恢复随机皮肤的原皮失败，不阻止打开界面：" + ex);
		}
	}
}

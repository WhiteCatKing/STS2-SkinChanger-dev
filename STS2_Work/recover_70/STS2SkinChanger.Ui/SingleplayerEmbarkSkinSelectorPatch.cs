using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCharacterSelectScreen), "StartNewSingleplayerRun")]
internal static class SingleplayerEmbarkSkinSelectorPatch
{
	private static void Prefix(NCharacterSelectScreen __instance)
	{
		try
		{
			string groupId = ((AbstractModel)ContextualSkinControls.GetLocalLobbyCharacter(__instance.Lobby)).Id.Entry.ToLowerInvariant();
			if (SkinService.IsRandomCharacterSkinEnabled(groupId))
			{
				ContextualSkinControls.HideCharacterSelector(__instance);
				return;
			}
			if (!SkinService.ApplySelectedCharacterSkinBundleForRun(groupId, out IReadOnlyList<string> warnings))
			{
				ModLog.Error("开始对局前应用皮肤包失败：" + SkinService.LastError);
			}
			foreach (string item in warnings)
			{
				ModLog.Warn("皮肤包：" + item);
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("开始单人对局前应用皮肤包失败，将继续使用当前外观：" + ex);
		}
		ContextualSkinControls.HideCharacterSelector(__instance);
	}
}

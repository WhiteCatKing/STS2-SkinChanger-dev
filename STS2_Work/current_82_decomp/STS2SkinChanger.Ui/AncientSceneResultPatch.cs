using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(EventModel), "CreateBackgroundScene")]
internal static class AncientSceneResultPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(EventModel __instance)
	{
		AncientEventModel val = (AncientEventModel)(object)((__instance is AncientEventModel) ? __instance : null);
		if (val != null)
		{
			SkinGroup skinGroup = AncientCompendiumEntry.FindGroup(((AbstractModel)val).Id.Entry);
			if (skinGroup != null)
			{
				CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(new _003C_003Ez__ReadOnlySingleElementList<string>(skinGroup.Id), "先古场景", null);
			}
		}
	}

	[HarmonyPriority(0)]
	private static void Postfix(EventModel __instance, ref PackedScene __result)
	{
		AncientEventModel val = (AncientEventModel)(object)((__instance is AncientEventModel) ? __instance : null);
		if (val == null)
		{
			return;
		}
		SkinGroup skinGroup = AncientCompendiumEntry.FindGroup(((AbstractModel)val).Id.Entry);
		if (skinGroup == null)
		{
			return;
		}
		try
		{
			if (SkinService.IsExternalRuntimeProviderSelected(skinGroup.Id))
			{
				__result = AncientCompendiumScreen.BuildSelectedRuntimeImageScene(skinGroup.Id);
				return;
			}
			string scenePath = AncientCompendiumEntry.GetScenePath(val);
			PackedScene orLoadRuntimeScene = SkinService.GetOrLoadRuntimeScene(skinGroup.Id, scenePath);
			__result = orLoadRuntimeScene;
		}
		catch (Exception value)
		{
			ModLog.Error($"最终应用 {((AbstractModel)val).Id.Entry} 的先古皮肤失败：{value}");
		}
	}
}

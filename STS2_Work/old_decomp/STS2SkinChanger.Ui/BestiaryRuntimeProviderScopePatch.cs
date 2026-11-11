using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NBestiary), "OnSubmenuClosed")]
internal static class BestiaryRuntimeProviderScopePatch
{
	[HarmonyPostfix]
	private static void Postfix()
	{
		if (NRun.Instance != null)
		{
			CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(null, "关闭局内怪物图鉴", null, null, refreshCurrentRoom: true);
		}
		else
		{
			SkinService.FocusRuntimeProviderBehaviorsOnGroups(Array.Empty<string>(), (IReadOnlyCollection<string>)(object)Array.Empty<string>(), "关闭怪物图鉴");
		}
	}
}

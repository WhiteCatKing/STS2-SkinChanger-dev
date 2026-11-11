using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NMainMenu), "_Ready")]
internal static class ModThemeEditorReadyPatch
{
	private static void Postfix(NMainMenu __instance)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)__instance) || !((Node)__instance).IsInsideTree())
			{
				return;
			}
			try
			{
				ModThemeEditor.Ensure((Node)(object)__instance);
			}
			catch (Exception ex)
			{
				ModLog.Warn("创建主题调节入口失败：" + ex.GetBaseException().Message);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}
}

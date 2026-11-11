using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Events;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NAncientEventLayout), "InitializeVisuals")]
internal static class ManagedAncientSceneAnimationPatch
{
	private static void Postfix(NAncientEventLayout __instance)
	{
		try
		{
			if (AncientRuntimeAppearance.TryGet(__instance, out AncientEventModel _, out SkinGroup group))
			{
				Node nodeOrNull = ((Node)__instance).GetNodeOrNull<Node>(NodePath.op_Implicit("%AncientBgContainer"));
				Node val = ((nodeOrNull != null && nodeOrNull.GetChildCount(false) > 0) ? nodeOrNull.GetChild(0, false) : null);
				if (val != null)
				{
					ManagedAncientStaticBackground.Fit(val);
					ManagedAncientLayeredImage.TryApply(group.Id, val);
					ManagedAncientSceneAnimation.TryStart(group.Id, val);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("在游戏内启动先古 Spine 动画失败：" + ex.Message);
		}
	}
}

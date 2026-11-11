using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;

namespace PengoTarot.Patch.relic;

[HarmonyPatch(typeof(NRelicInventory), "Remove")]
public static class NRelicInventory_RemoveAnimation_Patch
{
	[HarmonyPrefix]
	private static void Prefix(NRelicInventory __instance, RelicModel relic)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		NRelicInventoryHolder val = ((IEnumerable<NRelicInventoryHolder>)__instance.RelicNodes).FirstOrDefault((Func<NRelicInventoryHolder, bool>)((NRelicInventoryHolder node) => node.Relic.Model == relic));
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val.Relic))
		{
			Vector2 globalPosition = ((Control)val.Relic).GlobalPosition;
			Vector2 scale = ((Control)val.Relic).Scale;
			TaskHelper.RunSafely(AnimateRemovedRelic(relic, globalPosition, scale));
		}
	}

	private unsafe static async Task AnimateRemovedRelic(RelicModel relic, Vector2 startPosition, Vector2 startScale)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		NRun instance = NRun.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NGlobalUi globalUi = instance.GlobalUi;
			obj = ((globalUi != null) ? globalUi.AboveTopBarVfxContainer : null);
		}
		Control val = (Control)obj;
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			return;
		}
		NRelic relicImage = NRelic.Create(relic, (IconSize)0);
		if (relicImage != null)
		{
			((Control)relicImage).MouseFilter = (MouseFilterEnum)2;
			GodotTreeExtensions.AddChildSafely((Node)(object)val, (Node)(object)relicImage);
			await NodeUtil.AwaitProcessFrame((Node)(object)relicImage, default(CancellationToken));
			if (GodotObject.IsInstanceValid((GodotObject)(object)relicImage))
			{
				((Control)relicImage).GlobalPosition = startPosition;
				((Control)relicImage).Scale = startScale;
				((Control)relicImage).PivotOffset = ((Control)relicImage).Size * 0.5f;
				Tween obj2 = ((Node)relicImage).CreateTween().SetParallel(true);
				obj2.TweenProperty((GodotObject)(object)relicImage, NodePath.op_Implicit("global_position:y"), Variant.op_Implicit(startPosition.Y - 100f), 0.4000000059604645).SetEase((EaseType)0).SetTrans((TransitionType)10);
				obj2.TweenProperty((GodotObject)(object)relicImage, NodePath.op_Implicit("scale"), Variant.op_Implicit(startScale * 0.65f), 0.4000000059604645).SetEase((EaseType)0).SetTrans((TransitionType)4);
				obj2.TweenProperty((GodotObject)(object)relicImage, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 0.25).SetDelay(0.15000000596046448).SetEase((EaseType)0);
				obj2.TweenCallback(Callable.From(new Action(relicImage, (nint)(delegate*<Node, void>)(&GodotTreeExtensions.QueueFreeSafely)))).SetDelay(0.4000000059604645);
			}
		}
	}
}

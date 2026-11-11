using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Random;

namespace PengoTarot.BalatroEffect;

public static class EasePatches
{
	[HarmonyPatch(typeof(NCardHolder), "DoCardHoverEffects")]
	private static class NCardHolderEase
	{
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			CodeMatcher val = new CodeMatcher(instructions, (ILGenerator)null);
			val.Start().MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((Func<CodeInstruction, bool>)((CodeInstruction i) => CodeInstructionExtensions.Calls(i, createHoverTipsMethod)), (string)null)
			});
			if (val.IsValid)
			{
				val.InsertAfter((CodeInstruction[])(object)new CodeInstruction[4]
				{
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Callvirt, (object)getHoverScale),
					new CodeInstruction(OpCodes.Call, (object)animateEaseMethod)
				});
			}
			else
			{
				GD.PrintErr("PengoTarot: Failed to patch DoCardHoverEffects in NCardHolder");
			}
			return val.InstructionEnumeration();
		}
	}

	[HarmonyPatch(typeof(NCard), "OnFreedToPool")]
	private static class NCard_OnFreedToPool_Cleanup
	{
		private static void Postfix(NCard __instance)
		{
			ShaderController.CleanupCard(__instance);
		}
	}

	[HarmonyPatch(typeof(NCard), "OnReturnedFromPool")]
	private static class NCard_OnReturnedFromPool_Reapply
	{
		private static void Postfix(NCard __instance)
		{
			if (__instance != null && GodotObject.IsInstanceValid((GodotObject)(object)__instance))
			{
				ShaderController.ScheduleReapply(__instance);
			}
		}
	}

	[HarmonyPatch(typeof(NCardHolder), "ReassignToCard")]
	private static class NCardHolder_ReassignToCard_Patch
	{
		private static void Postfix(NCardHolder __instance)
		{
			NCard cardNode = __instance.CardNode;
			if (cardNode != null && cardNode.Model != null)
			{
				ShaderController.ApplyShader(cardNode);
			}
		}
	}

	[HarmonyPatch(typeof(NHandCardHolder), "DoCardHoverEffects")]
	private static class NHandCardHolderEase
	{
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Expected O, but got Unknown
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Expected O, but got Unknown
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Expected O, but got Unknown
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Expected O, but got Unknown
			CodeMatcher val = new CodeMatcher(instructions, (ILGenerator)null);
			val.Start().MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((Func<CodeInstruction, bool>)((CodeInstruction i) => CodeInstructionExtensions.Calls(i, createHoverTipsMethod)), (string)null)
			});
			if (val.IsValid)
			{
				val.InsertAfter((CodeInstruction[])(object)new CodeInstruction[4]
				{
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Ldarg_0, (object)null),
					new CodeInstruction(OpCodes.Callvirt, (object)getHoverScale),
					new CodeInstruction(OpCodes.Call, (object)animateEaseMethod)
				});
			}
			else
			{
				GD.PrintErr("PengoTarot: Failed to patch DoCardHoverEffects in NHandCardHolder");
			}
			return val.InstructionEnumeration();
		}
	}

	[HarmonyPatch(typeof(NCard), "UpdateVisuals")]
	public static class NCard_UpdateVisuals_ApplyShader_Patch
	{
		private static void Postfix(NCard __instance)
		{
			if (((__instance != null) ? __instance.Model : null) != null)
			{
				if (__instance.Body == null)
				{
					ShaderController.ScheduleReapply(__instance);
				}
				else
				{
					ShaderController.ApplyShader(__instance);
				}
			}
		}
	}

	[HarmonyPatch(typeof(NInspectCardScreen), "UpdateCardDisplay")]
	private static class NInspectCardScreen_UpdateCardDisplay_Reapply
	{
		private static void Postfix(NInspectCardScreen __instance)
		{
			object? obj = AccessTools.Field(typeof(NInspectCardScreen), "_card")?.GetValue(__instance);
			NCard val = (NCard)((obj is NCard) ? obj : null);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && val.Model != null && val.Body != null)
			{
				ShaderController.ApplyShader(val);
			}
		}
	}

	private static readonly MethodInfo getHoverScale = AccessTools.PropertyGetter(typeof(NCardHolder), "HoverScale");

	private static readonly MethodInfo createHoverTipsMethod = AccessTools.Method(typeof(NCardHolder), "CreateHoverTips", (Type[])null, (Type[])null);

	private static readonly MethodInfo animateEaseMethod = AccessTools.Method(typeof(EasePatches), "AnimateEase", (Type[])null, (Type[])null);

	public static void AnimateEase(Control instance, Vector2 targetScale)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (Config.GlobalDynamicEffect)
		{
			Traverse<Tween> obj = Traverse.Create((object)instance).Field<Tween>("_hoverTween");
			Tween value = obj.Value;
			if (value != null)
			{
				value.Kill();
			}
			instance.Scale *= 0.88f;
			instance.Rotation = 0.35f * (Rng.Chaotic.NextBool() ? 1f : (-1f));
			((Node)instance).CreateTween().TweenProperty((GodotObject)(object)instance, NodePath.op_Implicit("rotation"), Variant.op_Implicit(0f), 0.11999999731779099).SetEase((EaseType)1)
				.SetTrans((TransitionType)10);
			Tween val = ((Node)instance).CreateTween();
			val.TweenProperty((GodotObject)(object)instance, NodePath.op_Implicit("scale"), Variant.op_Implicit(targetScale), 0.11999999731779099).SetEase((EaseType)1).SetTrans((TransitionType)10);
			obj.Value = val;
		}
	}
}

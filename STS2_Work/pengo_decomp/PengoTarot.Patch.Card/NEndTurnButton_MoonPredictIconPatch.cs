using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

public static class NEndTurnButton_MoonPredictIconPatch
{
	[HarmonyPatch(typeof(NEndTurnButton), "_Ready")]
	public static class NEndTurnButton_Ready_MoonPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NEndTurnButton __instance)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Expected O, but got Unknown
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_010e: Unknown result type (might be due to invalid IL or missing references)
			if (((Node)__instance).FindChild("MoonPredictIcon", false, false) == null)
			{
				TextureRect val = new TextureRect
				{
					Name = StringName.op_Implicit("MoonPredictIcon"),
					MouseFilter = (MouseFilterEnum)2,
					AnchorLeft = 0.5f,
					AnchorTop = 0.5f,
					AnchorRight = 0.5f,
					AnchorBottom = 0.5f,
					OffsetLeft = -28f,
					OffsetTop = -88f,
					OffsetRight = 28f,
					OffsetBottom = -32f,
					GrowHorizontal = (GrowDirection)2,
					GrowVertical = (GrowDirection)2,
					StretchMode = (StretchModeEnum)5,
					Texture = GD.Load<Texture2D>("res://images/enchantments/tar_moon_reversed_enchantment.png"),
					Visible = false
				};
				((Node)__instance).AddChild((Node)(object)val, false, (InternalMode)0);
				Tween obj = ((Node)val).CreateTween().SetLoops(0);
				obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.2f), 0.6499999761581421).SetTrans((TransitionType)1).SetEase((EaseType)2);
				obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 0.6499999761581421).SetTrans((TransitionType)1).SetEase((EaseType)2);
			}
		}
	}

	[HarmonyPatch(typeof(NEndTurnButton), "OnCombatStateChanged")]
	public static class NEndTurnButton_OnCombatStateChanged_MoonPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NEndTurnButton __instance)
		{
			UpdateIcon(__instance);
		}
	}

	private const int MoonFlagIndex = 18;

	private const string MoonReversedIconPath = "res://images/enchantments/tar_moon_reversed_enchantment.png";

	private const string IconNodeName = "MoonPredictIcon";

	private const float IconSize = 56f;

	private const float IconCenterYAbove = -60f;

	private const float MinAlpha = 0.2f;

	private const float BlinkHalfPeriod = 0.65f;

	private static bool ShouldApply()
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(18) && RunManager.Instance.IsInProgress && CombatManager.Instance != null && CombatManager.Instance.IsInProgress)
		{
			return !CombatManager.Instance.IsOverOrEnding;
		}
		return false;
	}

	internal static void UpdateIcon(NEndTurnButton button)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Node obj = ((Node)button).FindChild("MoonPredictIcon", false, false);
		TextureRect val = (TextureRect)(object)((obj is TextureRect) ? obj : null);
		if (val == null)
		{
			return;
		}
		bool flag = ShouldShowMoonPredictIcon(button);
		if (((CanvasItem)val).Visible != flag)
		{
			((CanvasItem)val).Visible = flag;
			if (flag)
			{
				((CanvasItem)val).Modulate = new Color(1f, 1f, 1f, 1f);
			}
		}
	}

	private static bool ShouldShowMoonPredictIcon(NEndTurnButton button)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		if (!ShouldApply())
		{
			return false;
		}
		CombatState value = Traverse.Create((object)button).Field("_combatState").GetValue<CombatState>();
		if (value == null || (int)value.CurrentSide != 1)
		{
			return false;
		}
		Player me = LocalContext.GetMe((ICombatState)(object)value);
		if (me == null || !me.Creature.IsAlive)
		{
			return false;
		}
		CardPile pile = PileTypeExtensions.GetPile((PileType)2, me);
		CardPile pile2 = PileTypeExtensions.GetPile((PileType)1, me);
		CardPile pile3 = PileTypeExtensions.GetPile((PileType)3, me);
		bool flag = Hook.ShouldFlush((ICombatState)(object)value, me);
		int num = 0;
		foreach (CardModel card in pile.Cards)
		{
			if (!flag || card.ShouldRetainThisTurn)
			{
				num++;
			}
		}
		IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
		int num2 = (int)Math.Min(Hook.ModifyHandDraw((ICombatState)(object)value, me, 5m, ref enumerable), Math.Max(0, CardPile.MaxCardsInHand - num));
		if (num2 <= 0)
		{
			return false;
		}
		int num3 = pile3.Cards.Count + (pile.Cards.Count - num);
		if (pile2.Cards.Count < num2)
		{
			return num3 > 0;
		}
		return false;
	}
}

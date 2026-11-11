using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patch.Card;

public static class TarDevilDivinationPatch
{
	[HarmonyPatch(typeof(Creature), "SetCurrentHpInternal")]
	public static class Creature_SetCurrentHpInternal_DevilPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(Creature __instance, decimal amount)
		{
			if (!ShouldAffect(__instance))
			{
				return true;
			}
			SetPrivatePropertyValue(__instance, "CurrentHp", (int)Math.Max(amount, 0m));
			return false;
		}
	}

	[HarmonyPatch(typeof(Creature), "SetMaxHpInternal")]
	public static class Creature_SetMaxHpInternal_DevilPatch
	{
		[HarmonyPrefix]
		private static void Prefix(Creature __instance, out int __state)
		{
			__state = __instance.CurrentHp;
		}

		[HarmonyPostfix]
		private static void Postfix(Creature __instance, int __state)
		{
			if (ShouldAffect(__instance) && __instance.CurrentHp < __state)
			{
				SetPrivatePropertyValue(__instance, "CurrentHp", __state);
			}
		}
	}

	[HarmonyPatch(typeof(Player), "SyncWithSerializedPlayer")]
	public static class Player_SyncWithSerializedPlayer_DevilPatch
	{
		[HarmonyPostfix]
		private static void Postfix(Player __instance, SerializablePlayer player)
		{
			if (player.CurrentHp > player.MaxHp && __instance.Creature.CurrentHp != player.CurrentHp)
			{
				SetPrivatePropertyValue(__instance.Creature, "CurrentHp", player.CurrentHp);
			}
		}
	}

	[HarmonyPatch(typeof(CreatureCmd), "LoseMaxHp")]
	public static class CreatureCmd_LoseMaxHp_DevilPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(Creature creature, decimal amount, ref Task __result)
		{
			if (!ShouldAffect(creature))
			{
				return true;
			}
			__result = LoseMaxHpWithoutCurrentHpLoss(creature, amount);
			return false;
		}

		private static async Task LoseMaxHpWithoutCurrentHpLoss(Creature creature, decimal amount)
		{
			if (amount < 0m)
			{
				throw new ArgumentException("amount must be non-negative. Use GainMaxHp for max HP gain.");
			}
			decimal val = (decimal)creature.MaxHp - amount;
			Player player = creature.Player;
			MapPointHistoryEntry val2 = ((player != null) ? player.RunState.CurrentMapPointHistoryEntry : null);
			if (val2 != null)
			{
				PlayerMapPointHistoryEntry entry = val2.GetEntry(creature.Player.NetId);
				entry.MaxHpLost += (int)amount;
			}
			await CreatureCmd.SetMaxHp(creature, Math.Max(1.0m, val));
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterRestSiteHeal")]
	public static class Hook_AfterRestSiteHeal_DevilPatch
	{
		[HarmonyPostfix]
		private static async void Postfix(IRunState runState, Player player, bool isMimicked)
		{
			if (!isMimicked && ShouldApply())
			{
				await CreatureCmd.LoseMaxHp((PlayerChoiceContext)new ThrowingPlayerChoiceContext(), player.Creature, 6m, false);
			}
		}
	}

	[HarmonyPatch(typeof(NHealthBar), "GetFgWidth", new Type[]
	{
		typeof(int),
		typeof(float)
	})]
	public static class NHealthBar_GetFgWidth_DevilPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NHealthBar __instance, int amount, float maxFgWidth, ref float __result)
		{
			Creature value = Traverse.Create((object)__instance).Field("_creature").GetValue<Creature>();
			if (value == null || !value.IsPlayer || !ShouldApply())
			{
				return true;
			}
			if (value.MaxHp <= 0 || value.CurrentHp <= value.MaxHp)
			{
				return true;
			}
			float val = (float)amount / (float)value.CurrentHp * maxFgWidth;
			__result = Math.Max(val, (value.CurrentHp > 0) ? 12f : 0f);
			return false;
		}
	}

	[HarmonyPatch(typeof(NHealthBar), "RefreshText")]
	public static class NHealthBar_RefreshText_DevilPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NHealthBar __instance)
		{
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			Creature value = Traverse.Create((object)__instance).Field("_creature").GetValue<Creature>();
			if (value != null && value.IsPlayer && ShouldApply() && value.CurrentHp > 0 && value.CurrentHp > value.MaxHp)
			{
				Label nodeOrNull = ((Node)__instance).GetNodeOrNull<Label>(NodePath.op_Implicit("%HpLabel"));
				if (nodeOrNull != null)
				{
					((Control)nodeOrNull).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), DevilOverflowOutlineColor);
				}
			}
		}
	}

	[HarmonyPatch(typeof(NHealthBar), "RefreshForeground")]
	public static class NHealthBar_RefreshForeground_DevilPatch
	{
		private const float FgContainerDefaultOffsetRight = -5f;

		private const float BgDefaultOffsetRight = -1f;

		private static readonly Shader DevilOverflowShader = GD.Load<Shader>("res://shaders/devil_hp_overflow.gdshader");

		private static readonly ConditionalWeakTable<NHealthBar, ShaderMaterial> DevilOverflowMaterials = new ConditionalWeakTable<NHealthBar, ShaderMaterial>();

		private const float MintyRightPadding = 6f;

		[HarmonyPrefix]
		private static void Prefix(NHealthBar __instance, out float __state)
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			__state = 0f;
			Creature value = Traverse.Create((object)__instance).Field("_creature").GetValue<Creature>();
			if (value == null || !value.IsPlayer || !ShouldApply() || value.MaxHp <= 0)
			{
				return;
			}
			float x = __instance.HpBarContainer.Size.X;
			float num = x - 10f;
			Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpForegroundContainer"));
			Traverse val = Traverse.Create((object)__instance);
			if (value.CurrentHp <= value.MaxHp)
			{
				if (nodeOrNull != null)
				{
					nodeOrNull.OffsetRight = -5f;
				}
				val.Field("_expectedMaxFgWidth").SetValue((object)num);
				return;
			}
			float num2 = num * Math.Min(__state = (float)value.CurrentHp / (float)value.MaxHp, 2f);
			if (nodeOrNull != null)
			{
				nodeOrNull.OffsetRight = num2 - (x - 5f);
			}
			val.Field("_expectedMaxFgWidth").SetValue((object)num2);
		}

		[HarmonyPostfix]
		private static void Postfix(NHealthBar __instance, float __state)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			if (__state <= 0f)
			{
				RestoreOverflowShader(__instance);
				Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpBarContainer/HpBackground"));
				if (nodeOrNull != null)
				{
					nodeOrNull.OffsetRight = -1f;
				}
				Control nodeOrNull2 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%BlockOutline"));
				if (nodeOrNull2 != null)
				{
					nodeOrNull2.OffsetRight = -1f;
				}
				return;
			}
			float x = __instance.HpBarContainer.Size.X;
			float num = x - 10f;
			float num2 = num * Math.Min(1f, 2f / __state);
			float newFgW = num * Math.Min(__state, 2f);
			float offsetRight = 5f + num2 - x;
			Control nodeOrNull3 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpBarContainer/HpBackground"));
			if (nodeOrNull3 != null)
			{
				nodeOrNull3.OffsetRight = offsetRight;
			}
			Control nodeOrNull4 = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("%BlockOutline"));
			if (nodeOrNull4 != null)
			{
				nodeOrNull4.OffsetRight = offsetRight;
			}
			ApplyOverflowShader(__instance, num2);
			ShiftMintyIncomingDamageLabel(__instance, newFgW);
		}

		private static void ApplyOverflowShader(NHealthBar bar, float maxHpEdge)
		{
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			Control nodeOrNull = ((Node)bar).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpForeground"));
			if (nodeOrNull != null && DevilOverflowShader != null)
			{
				if (!DevilOverflowMaterials.TryGetValue(bar, out ShaderMaterial value))
				{
					value = new ShaderMaterial
					{
						Shader = DevilOverflowShader
					};
					DevilOverflowMaterials.Add(bar, value);
				}
				value.SetShaderParameter(StringName.op_Implicit("max_hp_edge"), Variant.op_Implicit(maxHpEdge));
				((CanvasItem)nodeOrNull).Material = (Material)(object)value;
			}
		}

		private static void RestoreOverflowShader(NHealthBar bar)
		{
			Control nodeOrNull = ((Node)bar).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpForeground"));
			if (nodeOrNull != null)
			{
				((CanvasItem)nodeOrNull).Material = null;
			}
			DevilOverflowMaterials.Remove(bar);
		}

		private static void ShiftMintyIncomingDamageLabel(NHealthBar bar, float newFgW)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			Label nodeOrNull = ((Node)bar).GetNodeOrNull<Label>(NodePath.op_Implicit("MintyIncomingDamageText"));
			if (nodeOrNull != null)
			{
				Control nodeOrNull2 = ((Node)bar).GetNodeOrNull<Control>(NodePath.op_Implicit("%HpForegroundContainer"));
				if (nodeOrNull2 != null)
				{
					((Control)nodeOrNull).GlobalPosition = new Vector2(nodeOrNull2.GlobalPosition.X + newFgW + 6f, ((Control)nodeOrNull).GlobalPosition.Y);
				}
			}
		}
	}

	[HarmonyPatch(typeof(Hook), "ModifyExtraRestSiteHealText")]
	public static class Hook_ModifyExtraRestSiteHealText_DevilPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ref IReadOnlyList<LocString> __result)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			if (ShouldApply())
			{
				LocString element = new LocString("gameplay_ui", "BAL_CFW_DEVIL_REST_HEAL_DESC");
				__result = __result.Append(element).ToArray();
			}
		}
	}

	[HarmonyPatch(typeof(NRestSiteButton), "_Ready")]
	public static class NRestSiteButton_Ready_DevilPatch
	{
		private const string DevilReversedIconPath = "res://images/enchantments/tar_devil_reversed_enchantment.png";

		private const float DevilIconSize = 32f;

		private const float DevilIconGap = 4f;

		private const float DevilIconRightOffset = 54f;

		private const float DevilIconBreathMinAlpha = 0.6f;

		private const float DevilIconBreathHalfPeriod = 1.4f;

		[HarmonyPostfix]
		private static void Postfix(NRestSiteButton __instance)
		{
			if (__instance.Option is HealRestSiteOption && ShouldApply())
			{
				AddDevilIconAboveIcon(__instance);
			}
		}

		private static void AddDevilIconAboveIcon(NRestSiteButton btn)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Expected O, but got Unknown
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			Control value = Traverse.Create((object)btn).Field("_visuals").GetValue<Control>();
			TextureRect value2 = Traverse.Create((object)btn).Field("_icon").GetValue<TextureRect>();
			if (value != null && value2 != null)
			{
				TextureRect val = new TextureRect
				{
					Texture = GD.Load<Texture2D>("res://images/enchantments/tar_devil_reversed_enchantment.png"),
					Size = new Vector2(32f, 32f),
					Position = new Vector2(((Control)value2).Position.X + ((Control)value2).Size.X / 2f - 16f + 54f, ((Control)value2).Position.Y - 32f - 4f),
					ExpandMode = (ExpandModeEnum)1,
					StretchMode = (StretchModeEnum)5,
					MouseFilter = (MouseFilterEnum)2
				};
				((Node)value).AddChild((Node)(object)val, false, (InternalMode)0);
				Tween obj = ((Node)val).CreateTween().SetLoops(0);
				obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0.6f), 1.399999976158142).SetTrans((TransitionType)1).SetEase((EaseType)2);
				obj.TweenProperty((GodotObject)(object)val, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 1.399999976158142).SetTrans((TransitionType)1).SetEase((EaseType)2);
			}
		}
	}

	private const int DevilFlagIndex = 15;

	private const decimal RestMaxHpLoss = 6m;

	private const string DevilOverflowColor = "#F59C2E";

	private static readonly Color DevilOverflowOutlineColor = new Color("7A3B00");

	internal static bool ShouldApply()
	{
		if (ConfigFloatingWindowRunData.GetTarFlag(15))
		{
			return RunManager.Instance.IsInProgress;
		}
		return false;
	}

	private static bool ShouldAffect(Creature creature)
	{
		if (creature.IsPlayer)
		{
			return ShouldApply();
		}
		return false;
	}

	private static void SetPrivatePropertyValue(Creature creature, string propertyName, object value)
	{
		typeof(Creature).GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(creature, value);
	}
}

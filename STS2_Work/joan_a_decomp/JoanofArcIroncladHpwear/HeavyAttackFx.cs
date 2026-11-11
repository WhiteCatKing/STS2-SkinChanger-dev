using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(AttackCommand), "Execute")]
internal static class HeavyAttackFx
{
	private sealed class DirectionalImpact
	{
		private readonly Creature? attacker;

		internal DirectionalImpact(Creature? attacker)
		{
			this.attacker = attacker;
		}

		internal Node2D? CreateHorizontalImpact(Creature target)
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(target) : null);
			if (val == null)
			{
				return null;
			}
			NCreature val2 = ((attacker == null) ? null : ((instance != null) ? instance.GetCreatureNode(attacker) : null));
			float num = ((val2 != null && ((Control)val).GlobalPosition.X < ((Control)val2).GlobalPosition.X) ? 180f : 0f);
			return (Node2D?)(object)NBigSlashImpactVfx.Create(val.VfxSpawnPosition, num, Color.FromHtml("#80dbff".AsSpan()));
		}
	}

	private static readonly FieldRef<AttackCommand, string?> Animation = AccessTools.FieldRefAccess<AttackCommand, string>("_attackerAnimName");

	private static readonly FieldRef<AttackCommand, bool> PlaysAnimation = AccessTools.FieldRefAccess<AttackCommand, bool>("_shouldPlayAnimation");

	private static readonly FieldRef<AttackCommand, Creature?> VisualAttacker = AccessTools.FieldRefAccess<AttackCommand, Creature>("_visualAttacker");

	private static readonly FieldRef<AttackCommand, string?> AttackerVfx = AccessTools.FieldRefAccess<AttackCommand, string>("_attackerVfx");

	private static readonly FieldRef<AttackCommand, List<Func<Node2D?>>> AttackerNodes = AccessTools.FieldRefAccess<AttackCommand, List<Func<Node2D>>>("_customAttackerVfxNodes");

	private static readonly FieldRef<AttackCommand, List<Func<Creature, Node2D?>>> HitNodes = AccessTools.FieldRefAccess<AttackCommand, List<Func<Creature, Node2D>>>("_customHitVfxNodes");

	private static void Prefix(AttackCommand __instance)
	{
		try
		{
			Creature attacker = __instance.Attacker;
			if (__instance.ModelSource is CardModel)
			{
				object obj;
				if (attacker == null)
				{
					obj = null;
				}
				else
				{
					Player player = attacker.Player;
					obj = ((player != null) ? player.Character : null);
				}
				if (obj is Ironclad && WantsReplacement(Animation.Invoke(__instance), PlaysAnimation.Invoke(__instance), VisualAttacker.Invoke(__instance) == null || VisualAttacker.Invoke(__instance) == attacker, __instance.ModelSource is PerfectedStrike) && IsUsingThisSkin(attacker))
				{
					ReplaceEffects(__instance);
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("heavy attack fx skipped: " + ex.Message, 1);
		}
	}

	internal static bool IsUsingThisSkin(Creature attacker)
	{
		if (!SkinSelectionGate.PlayerAllowed(attacker.Player))
		{
			return false;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NCreature creatureNode = instance.GetCreatureNode(attacker);
			if (creatureNode == null)
			{
				obj = null;
			}
			else
			{
				NCreatureVisuals visuals = creatureNode.Visuals;
				obj = ((visuals != null) ? visuals.GetCurrentBody() : null);
			}
		}
		Node2D val = (Node2D)obj;
		if (val == null || ((GodotObject)val).GetClass() != "SpineSprite")
		{
			return false;
		}
		return SkinCatalog.IsOurPath(SpineSwap.GetSkeletonPath((Node)(object)val));
	}

	internal static bool WantsReplacement(string? trigger, bool playsAnimation, bool ownsVisual, bool perfectedStrike)
	{
		if (ownsVisual)
		{
			if (!perfectedStrike)
			{
				if (playsAnimation)
				{
					return trigger == "heavyAttack";
				}
				return false;
			}
			return true;
		}
		return false;
	}

	internal static void ReplaceEffects(AttackCommand attack)
	{
		AttackerVfx.Invoke(attack) = null;
		AttackerNodes.Invoke(attack).Clear();
		attack.WithHitFx((string)null, attack.HitSfx, attack.TmpHitSfx);
		HitNodes.Invoke(attack).Clear();
		attack.WithHitVfxNode((Func<Creature, Node2D>)new DirectionalImpact(attack.Attacker).CreateHorizontalImpact);
	}
}

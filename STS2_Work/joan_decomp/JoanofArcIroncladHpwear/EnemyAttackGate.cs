using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace JoanofArcIroncladHpwear;

internal static class EnemyAttackGate
{
	private static readonly EnemyTurnGate<Creature> Gate = new EnemyTurnGate<Creature>();

	private static CombatState? snapshot;

	private static int round = -1;

	private static Creature? last;

	private static bool sawPerforming;

	internal static bool IsLow(Creature creature)
	{
		int num = Math.Clamp(WearSettingsRuntime.Current.LowHealthIdlePercent, 0, 100);
		if (creature.MaxHp > 0 && creature.CurrentHp > 0 && num > 0)
		{
			return (long)creature.CurrentHp * 100L <= (long)creature.MaxHp * (long)num;
		}
		return false;
	}

	internal static void Snapshot(CombatState? state)
	{
		if (state == null || (snapshot == state && round == state.RoundNumber))
		{
			return;
		}
		snapshot = state;
		round = state.RoundNumber;
		last = null;
		sawPerforming = false;
		List<Creature> list = new List<Creature>();
		foreach (Player player in state.Players)
		{
			if (((player != null) ? player.Character : null) is Ironclad && player.Creature != null && IsLow(player.Creature))
			{
				list.Add(player.Creature);
			}
		}
		foreach (Creature enemy in state.Enemies)
		{
			if (enemy != null && !enemy.IsDead)
			{
				MonsterModel monster = enemy.Monster;
				if (monster != null && monster.IntendsToAttack)
				{
					last = enemy;
				}
			}
		}
		Gate.Begin(last, list);
	}

	internal static void EnsureArmed()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		CombatManager instance = CombatManager.Instance;
		CombatState val = ((instance != null) ? instance.DebugOnlyGetState() : null);
		if (val == null)
		{
			Reset();
			return;
		}
		if ((int)val.CurrentSide == 2)
		{
			Snapshot(val);
		}
		bool flag = false;
		if (last != null)
		{
			foreach (Creature enemy in val.Enemies)
			{
				if (enemy == last)
				{
					flag = true;
					break;
				}
			}
		}
		Creature? obj = last;
		if (((obj != null) ? obj.Monster : null) != null && !last.IsDead && flag)
		{
			if (last.Monster.IsPerformingMove)
			{
				sawPerforming = true;
			}
			else if (sawPerforming)
			{
				Gate.MoveFinished(last);
			}
		}
		else if (last != null)
		{
			Gate.MoveFinished(last);
		}
	}

	internal static void MoveFinished(MonsterModel? monster)
	{
		if (((monster != null) ? monster.Creature : null) != null)
		{
			Gate.MoveFinished(monster.Creature);
		}
	}

	internal static void EndTurn()
	{
		Gate.EndTurn();
	}

	internal static void Reset()
	{
		Gate.Reset();
		snapshot = null;
		round = -1;
		last = null;
		sawPerforming = false;
	}

	internal static bool Hold(Creature creature, string? animation)
	{
		object obj;
		if (creature == null)
		{
			obj = null;
		}
		else
		{
			Player player = creature.Player;
			obj = ((player != null) ? player.Character : null);
		}
		if (!(obj is Ironclad) || !SkinSelectionGate.PlayerAllowed(creature.Player))
		{
			return false;
		}
		EnsureArmed();
		return Gate.ShouldHold(creature, IsLow(creature), creature.IsDead || creature.CurrentHp <= 0, animation);
	}

	internal static bool BlockAnimation(MegaAnimationState animationState)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		NCombatRoom instance = NCombatRoom.Instance;
		CombatManager instance2 = CombatManager.Instance;
		CombatState val = ((instance2 != null) ? instance2.DebugOnlyGetState() : null);
		if (instance == null || val == null || animationState == null)
		{
			return false;
		}
		foreach (Player player in val.Players)
		{
			if (!(((player != null) ? player.Character : null) is Ironclad) || !SkinSelectionGate.PlayerAllowed(player))
			{
				continue;
			}
			Creature creature = player.Creature;
			NCreature creatureNode = instance.GetCreatureNode(creature);
			object obj;
			if (creatureNode == null)
			{
				obj = null;
			}
			else
			{
				NCreatureVisuals visuals = creatureNode.Visuals;
				obj = ((visuals != null) ? visuals.GetCurrentBody() : null);
			}
			Node2D val2 = (Node2D)obj;
			if (val2 != null && SpineSwap.IsOurs((Node)(object)val2))
			{
				Variant val3 = ((GodotObject)val2).Call(StringName.op_Implicit("get_animation_state"), Array.Empty<Variant>());
				GodotObject val4 = ((Variant)(ref val3)).AsGodotObject();
				if (val4 != null && GodotObject.IsInstanceValid(val4) && val4.GetInstanceId() == ((MegaSpineBinding)animationState).BoundObject.GetInstanceId())
				{
					return Hold(creature, SpineSwap.CurrentAnimation((Node)(object)val2));
				}
			}
		}
		return false;
	}
}

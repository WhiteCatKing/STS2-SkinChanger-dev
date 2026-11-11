using System;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class TunnelerCompoundBodyAdapter
{
	private sealed class PresentationState
	{
		public int Token { get; set; }
	}

	private const string ProfileId = "sandworm_for_tunneler";

	private const string NestRole = "nest";

	private const float BurrowDurationSeconds = 1.6333f;

	private static readonly ConditionalWeakTable<NCreature, PresentationState> States = new ConditionalWeakTable<NCreature, PresentationState>();

	public static bool TryResolvePresentationHost(NCreature creature, EnemyReplacementProfile profile, EnemyAction action, out Node2D host)
	{
		host = null;
		if (profile.Id == "sandworm_for_tunneler" && action == EnemyAction.HeavyAttack)
		{
			return EnemyVisualHostAdapter.TryResolveSecondaryBody(creature, "nest", out host);
		}
		return false;
	}

	public static void ObserveTrigger(NCreature creature, string trigger)
	{
		if (!EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile) || profile.Id != "sandworm_for_tunneler")
		{
			return;
		}
		int token = ++States.GetOrCreateValue(creature).Token;
		switch (trigger)
		{
		case "Burrow":
			EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: true);
			EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: true);
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "nest", "normal_debuff_1", "normal_idle");
			Schedule(creature, token, 1.6333f, delegate
			{
				EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: false);
				ShowNestIdle(creature);
			});
			break;
		case "BurrowAttack":
			EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: false);
			EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: true);
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "nest", "normal_special_1", "normal_idle");
			break;
		case "WakeUp":
			EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: true);
			EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: true);
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "nest", "normal_enter", "normal_idle");
			Schedule(creature, token, 1f, delegate
			{
				EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: false);
			});
			break;
		case "Stun":
			EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: false);
			EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: true);
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "nest", "normal_groggy", "normal_groggy");
			break;
		case "Attack":
			ShowWorm(creature);
			break;
		case "Hit":
			if (EnemyVisualHostAdapter.IsSecondaryBodyVisible(creature, "nest"))
			{
				Creature entity = creature.Entity;
				MonsterModel obj = ((entity != null) ? entity.Monster : null);
				Tunneler val = (Tunneler)(object)((obj is Tunneler) ? obj : null);
				string nextLoopAnimation = ((val != null && val.IsStunned) ? "normal_groggy" : "normal_idle");
				EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "nest", "normal_hit", nextLoopAnimation);
			}
			break;
		}
	}

	private static void ShowWorm(NCreature creature)
	{
		EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: false);
		EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: true);
	}

	private static void ShowNestIdle(NCreature creature)
	{
		EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "nest", visible: true);
		EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "nest", "normal_idle", "normal_idle");
	}

	private static void Schedule(NCreature creature, int token, float seconds, Action action)
	{
		((Node)creature).GetTree().CreateTimer((double)seconds, false, false, false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creature) && States.TryGetValue(creature, out PresentationState value) && value.Token == token)
			{
				action();
			}
		};
	}
}

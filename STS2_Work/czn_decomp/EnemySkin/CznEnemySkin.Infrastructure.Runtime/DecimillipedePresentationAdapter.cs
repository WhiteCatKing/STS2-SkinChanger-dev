using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class DecimillipedePresentationAdapter
{
	private static readonly HashSet<string> LinkedProfileIds = new HashSet<string> { "abyss_insect_for_decimillipede_segment_front", "abyss_insect_for_decimillipede_segment_middle", "abyss_insect_for_decimillipede_segment_back" };

	private static WeakReference<NCreature>? _visibleActionOwner;

	public static async Task PlayCombatActor(NCreature creature, EnemyAction action)
	{
		try
		{
			StartOne(creature, action, includeAudio: true);
			await Cmd.Wait(0.5f, false);
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Decimillipede actor presentation failed: " + ex.Message, 1);
		}
	}

	public static bool ShouldPlayEntranceAudio(MonsterModel? monster)
	{
		if (monster is DecimillipedeSegment)
		{
			return monster is DecimillipedeSegmentFront;
		}
		return true;
	}

	public static bool OwnsCombatActionPresentation(MonsterModel? monster, EnemyReplacementProfile profile, string trigger)
	{
		if (monster is DecimillipedeSegment && string.Equals(trigger, "Attack", StringComparison.Ordinal))
		{
			return LinkedProfileIds.Contains(profile.Id);
		}
		return false;
	}

	public static bool TryCreateCombatPresentation(DecimillipedeSegment source, EnemyAction action, out Task presentation)
	{
		presentation = Task.CompletedTask;
		NCreature creatureNode = ((MonsterModel)source).Creature.GetCreatureNode();
		if (creatureNode == null || !EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile) || !profile.Target.MonsterId.StartsWith("DECIMILLIPEDE_SEGMENT_", StringComparison.Ordinal))
		{
			return false;
		}
		presentation = PlayCombatActor(creatureNode, action);
		return true;
	}

	public static async Task PlayBestiarySequentially(IEnumerable<NCreature> creatures, Task nativePresentation)
	{
		try
		{
			await PlaySequentially(creatures.OrderBy((NCreature creature) => SegmentOrder(creature.Entity.Monster)), EnemyAction.BasicAttack);
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Decimillipede Bestiary presentation failed: " + ex.Message, 1);
		}
		await nativePresentation;
	}

	private static async Task PlaySequentially(IEnumerable<NCreature> creatures, EnemyAction action)
	{
		bool includeAudio = true;
		foreach (NCreature creature in creatures)
		{
			float num = StartOne(creature, action, includeAudio);
			if (num > 0f)
			{
				await Cmd.Wait(num, false);
			}
			includeAudio = false;
		}
	}

	private static float StartOne(NCreature creature, EnemyAction action, bool includeAudio)
	{
		if (!EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile))
		{
			return 0f;
		}
		EnemyAnimationBinding enemyAnimationBinding = profile.Monster.AnimationBindings.First((EnemyAnimationBinding candidate) => candidate.Stance == EnemyPresentationStance.Normal && candidate.Action == action);
		MegaSprite spineBody = creature.Visuals.SpineBody;
		MegaAnimationState val = ((spineBody != null) ? spineBody.TryGetAnimationState() : null);
		if (val == null)
		{
			return 0f;
		}
		if (includeAudio)
		{
			Entry.ActionPresentation.StopNonPriorityAudioForProfiles(LinkedProfileIds);
		}
		HandoffVisibleActionLane(creature);
		Entry.ActionPresentation.PlayActorIsolated(creature, profile, action, includeAudio);
		val.SetAnimation(enemyAnimationBinding.CznAnimationName, false, 0);
		val.AddAnimation("normal_idle", 0f, true, 0);
		return creature.GetCurrentAnimationTimeRemaining();
	}

	private static void HandoffVisibleActionLane(NCreature creature)
	{
		if (_visibleActionOwner != null && _visibleActionOwner.TryGetTarget(out NCreature target) && target != creature && GodotObject.IsInstanceValid((GodotObject)(object)target))
		{
			Entry.ActionPresentation.CancelActorPresentation(target);
		}
		_visibleActionOwner = new WeakReference<NCreature>(creature);
	}

	private static int SegmentOrder(MonsterModel? monster)
	{
		if (!(monster is DecimillipedeSegmentFront))
		{
			if (!(monster is DecimillipedeSegmentMiddle))
			{
				if (monster is DecimillipedeSegmentBack)
				{
					return 2;
				}
				return 3;
			}
			return 1;
		}
		return 0;
	}
}

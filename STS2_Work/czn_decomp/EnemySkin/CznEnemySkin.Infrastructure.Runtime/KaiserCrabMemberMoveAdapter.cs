using System;
using System.Threading.Tasks;
using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class KaiserCrabMemberMoveAdapter
{
	public static void Play(MonsterModel monster, EnemyAction action, string bodyAnimation, float startDelaySeconds = 0f)
	{
		if (startDelaySeconds <= 0f)
		{
			PlayNow(monster, action, bodyAnimation);
		}
		else
		{
			PlayAfter(monster, action, bodyAnimation, startDelaySeconds);
		}
	}

	private static async Task PlayAfter(MonsterModel monster, EnemyAction action, string bodyAnimation, float delaySeconds)
	{
		try
		{
			await Cmd.Wait(delaySeconds, false);
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Kaiser Crab member presentation delay failed: " + ex.Message, 1);
			return;
		}
		if (!monster.Creature.IsDead)
		{
			PlayNow(monster, action, bodyAnimation);
		}
	}

	private static void PlayNow(MonsterModel monster, EnemyAction action, string bodyAnimation)
	{
		try
		{
			NCreature creatureNode = monster.Creature.GetCreatureNode();
			if (creatureNode != null && EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile))
			{
				MegaSprite spineBody = creatureNode.Visuals.SpineBody;
				MegaAnimationState val = ((spineBody != null) ? spineBody.TryGetAnimationState() : null);
				if (val != null)
				{
					val.SetAnimation(bodyAnimation, false, 0);
					val.AddAnimation("normal_idle", 0f, true, 0);
					Entry.ActionPresentation.Play(creatureNode, profile, action);
				}
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Kaiser Crab member presentation adapter failed: " + ex.Message, 1);
		}
	}
}

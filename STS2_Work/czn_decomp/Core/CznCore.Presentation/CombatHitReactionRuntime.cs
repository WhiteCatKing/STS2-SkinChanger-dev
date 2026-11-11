using System;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatHitReactionRuntime
{
	public static bool ShouldReactToHpChange(decimal delta, bool alive, bool inCombat)
	{
		return delta < 0m && alive && inCombat;
	}

	public static bool Play(NCreature creature, CombatPresentationState reaction, Func<int> begin, Func<int, bool> isCurrent, Action complete)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creature) || !creature.Entity.IsAlive || reaction.BodySteps.Count != 1 || string.IsNullOrEmpty(reaction.LoopAnimation))
		{
			return false;
		}
		string hit = reaction.BodySteps[0].BodyAnimation;
		string idle = reaction.LoopAnimation;
		SpineAnimationAccess spineAnimation = creature.SpineAnimation;
		MegaAnimationState animationState = ((SpineAnimationAccess)(ref spineAnimation)).GetAnimationState();
		if (animationState == null)
		{
			return false;
		}
		int token = begin();
		MegaTrackEntry val = null;
		MegaTrackEntry val2 = null;
		float animationEnd;
		try
		{
			val = SpineAnimationCompat.SetAnimationTracked(animationState, hit, loop: false);
			if (val == null)
			{
				return false;
			}
			if (reaction.AtomicPoseTransition)
			{
				val.SetMixDuration(0f);
			}
			animationEnd = val.GetAnimationEnd();
			val2 = SpineAnimationCompat.AddAnimationTracked(animationState, idle, animationEnd);
			if (val2 == null)
			{
				return false;
			}
			if (reaction.AtomicPoseTransition)
			{
				val2.SetMixDuration(0f);
			}
		}
		finally
		{
			SpineAnimationCompat.ReleaseTracked(val2);
			SpineAnimationCompat.ReleaseTracked(val);
		}
		GodotScheduler.Delay(animationEnd, Finish);
		return true;
		void Finish()
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)creature) && creature.Entity.IsAlive && isCurrent(token))
				{
					SpineAnimationAccess spineAnimation2 = creature.SpineAnimation;
					string currentAnimationName = ((SpineAnimationAccess)(ref spineAnimation2)).GetCurrentAnimationName(0);
					if (string.Equals(currentAnimationName, hit, StringComparison.Ordinal))
					{
						GodotScheduler.Delay(0f, Finish);
					}
					else if (string.Equals(currentAnimationName, idle, StringComparison.Ordinal))
					{
						complete();
					}
				}
			}
			catch (Exception ex)
			{
				SkinLog.Warn("Hit reaction completion failed: " + SkinLog.FormatException(ex));
			}
		}
	}
}

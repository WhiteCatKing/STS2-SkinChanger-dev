using System;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class CombatDeathSequenceRuntime(CharacterSkinProfile profile)
{
	public void Schedule(Creature creature, Action<NCreature, string, bool>? applyBodyEffectState, Action<string, int> info, Action<string, int> warn, CombatBodySurfaceCoordinator? bodySurface = null)
	{
		ArgumentNullException.ThrowIfNull(creature, "creature");
		GodotScheduler.Deferred(delegate
		{
			RunScheduled(creature, applyBodyEffectState, info, warn, bodySurface);
		});
	}

	private void RunScheduled(Creature creature, Action<NCreature, string, bool>? applyBodyEffectState, Action<string, int> info, Action<string, int> warn, CombatBodySurfaceCoordinator? bodySurface)
	{
		try
		{
			Play(creature, applyBodyEffectState, info, warn, bodySurface);
		}
		catch (Exception ex)
		{
			warn("Player death animation failed: " + FormatDeathSequence() + " " + SkinLog.FormatException(ex), 2);
		}
	}

	private void Play(Creature creature, Action<NCreature, string, bool>? applyBodyEffectState, Action<string, int> info, Action<string, int> warn, CombatBodySurfaceCoordinator? bodySurface)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		NCombatRoom instance = NCombatRoom.Instance;
		NCreature val = ((instance != null) ? instance.GetCreatureNode(creature) : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			warn("Player death animation skipped: combat creature node not found for " + FormatDeathSequence(), 2);
			return;
		}
		string[] deathSequence = profile.CombatBody.DeathSequence;
		if (deathSequence.Length == 0)
		{
			throw new InvalidOperationException("Death sequence is empty.");
		}
		if (bodySurface != null)
		{
			bodySurface.Ensure(val, "combat_death_sequence");
			string[] array = deathSequence;
			foreach (string text in array)
			{
				if (!bodySurface.HasAnimation(val, text, profile.ProfileId + " combat death"))
				{
					warn("Player death animation skipped missing body animation: " + text, 2);
					return;
				}
			}
		}
		applyBodyEffectState?.Invoke(val, deathSequence[0], arg3: false);
		SpineAnimationCompat.SetAnimation(val.SpineAnimation, deathSequence[0], loop: false);
		for (int j = 1; j < deathSequence.Length; j++)
		{
			SpineAnimationCompat.AddAnimation(val.SpineAnimation, deathSequence[j], 0f, loop: false);
		}
		info("Player death animation sequence started: " + FormatDeathSequence(), 2);
	}

	private string FormatDeathSequence()
	{
		return string.Join(" -> ", profile.CombatBody.DeathSequence);
	}
}

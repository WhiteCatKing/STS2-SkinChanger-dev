using System;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class ToughEggCompoundBodyAdapter
{
	private const string ProfileId = "fairy_changeling_for_tough_egg";

	private const string EggRole = "egg";

	private const float HatchDurationSeconds = 1.2f;

	public static void ObserveTrigger(NCreature creature, string trigger)
	{
		if (!EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile) || profile.Id != "fairy_changeling_for_tough_egg" || !EnemyVisualHostAdapter.IsSecondaryBodyVisible(creature, "egg"))
		{
			return;
		}
		switch (trigger)
		{
		case "Hatch":
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "egg", "normal_transform_1");
			Schedule(creature, 1.2f, delegate
			{
				EnemyVisualHostAdapter.TrySetSecondaryBodyVisible(creature, "egg", visible: false);
				EnemyVisualHostAdapter.TrySetPrimaryBodyVisible(creature, visible: true);
			});
			break;
		case "Hit":
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "egg", "normal_hit", "normal_idle");
			break;
		case "Dead":
			EnemyVisualHostAdapter.TryPlaySecondaryBody(creature, "egg", "normal_death");
			break;
		}
	}

	private static void Schedule(NCreature creature, float seconds, Action action)
	{
		((Node)creature).GetTree().CreateTimer((double)seconds, false, false, false).Timeout += delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creature))
			{
				action();
			}
		};
	}
}

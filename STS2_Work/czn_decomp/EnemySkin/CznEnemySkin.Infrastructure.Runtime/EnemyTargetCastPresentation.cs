using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyTargetCastPresentation
{
	internal static bool Supports(string profileId, EnemyAction action)
	{
		bool flag = profileId == "senectus_for_soul_fysh";
		if (flag)
		{
			bool flag2 = (((uint)(action - 1) <= 1u || action == EnemyAction.Special) ? true : false);
			flag = flag2;
		}
		bool flag3 = flag;
		if (!flag3)
		{
			bool flag2 = profileId == "debolessa_for_lagavulin_matriarch";
			if (flag2)
			{
				bool flag4 = (((uint)(action - 1) <= 1u || (uint)(action - 5) <= 1u) ? true : false);
				flag2 = flag4;
			}
			flag3 = flag2;
		}
		bool flag5 = flag3 || (profileId == "resentful_convict_for_the_insatiable" && action == EnemyAction.Special);
		if (!flag5)
		{
			bool flag2 = action == EnemyAction.Cast;
			if (flag2)
			{
				bool flag4;
				switch (profileId)
				{
				case "shadow_collector_for_queen":
				case "bomberplasm_for_the_forgotten":
				case "debolessa_for_lagavulin_matriarch":
				case "senectus_for_soul_fysh":
				case "bercula_for_vantom":
					flag4 = true;
					break;
				default:
					flag4 = false;
					break;
				}
				flag2 = flag4;
			}
			flag5 = flag2;
		}
		return flag5;
	}

	internal static void Play(NCreature actor, EnemyReplacementProfile profile, EnemyAction action)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!Supports(profile.Id, action) || !Entry.PresentationSettings.ActionEffectsEnabled || !Entry.Replacements.IsEnabled(profile))
		{
			return;
		}
		if (profile.Id == "bercula_for_vantom")
		{
			Creature entity = actor.Entity;
			Control val = ((entity != null) ? entity.GetVfxContainer() : null);
			if (val != null)
			{
				Entry.ActionPresentation.TryPlayTargetEffects((Node)(object)val, Vector2.Zero, profile, action, Entry.ActionPresentation.EffectsGeneration, (Node?)(object)actor.Entity.GetBackVfxContainer(), null, atEntry: true, centerOnly: false, null, screenSpace: true, null, EnemyPresentationStance.Normal, null);
			}
			return;
		}
		Creature entity2 = actor.Entity;
		object obj;
		if (entity2 == null)
		{
			obj = null;
		}
		else
		{
			ICombatState combatState = entity2.CombatState;
			obj = ((combatState != null) ? combatState.PlayerCreatures.Where((Creature c) => c.IsAlive).ToArray() : null);
		}
		Creature[] array = (Creature[])obj;
		if (array == null || array.Length == 0)
		{
			return;
		}
		Creature[] array2 = array.Where(delegate(Creature c)
		{
			NCreature creatureNode = c.GetCreatureNode();
			return creatureNode != null && GodotObject.IsInstanceValid((GodotObject)(object)creatureNode) && ((Node)creatureNode).IsInsideTree() && !((GodotObject)creatureNode).IsQueuedForDeletion();
		}).ToArray();
		if (array2.Length != 0)
		{
			NCreature[] targets = array2.Select((Creature c) => c.GetCreatureNode()).ToArray();
			Control vfxContainer = array2[0].GetVfxContainer();
			if (vfxContainer != null)
			{
				PlayOnTargets(Entry.ActionPresentation, profile, action, targets, (Node)(object)vfxContainer, (Node?)(object)array2[0].GetBackVfxContainer(), Entry.ActionPresentation.EffectsGeneration);
			}
		}
	}

	internal static bool PlayOnTargets(EnemyActionPresentationRuntime runtime, EnemyReplacementProfile profile, EnemyAction action, IReadOnlyList<NCreature> targets, Node front, Node? back, int generation)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		if (!Supports(profile.Id, action) || profile.Id == "bercula_for_vantom")
		{
			return false;
		}
		NCreature[] array = targets.Where((NCreature n) => GodotObject.IsInstanceValid((GodotObject)(object)n) && ((Node)n).IsInsideTree() && !((GodotObject)n).IsQueuedForDeletion()).Distinct().ToArray();
		if (array.Length == 0)
		{
			return false;
		}
		bool flag = false;
		NCreature[] array2 = array;
		foreach (NCreature target in array2)
		{
			bool num = flag;
			Vector2 globalPosition = EnemyTargetPosition.ResolveFloor(target);
			Vector2? bodyPosition = EnemyTargetPosition.Resolve(target, EnemyTargetOrigin.Body);
			flag = num | runtime.TryPlayTargetEffects(front, globalPosition, profile, action, generation, back, null, atEntry: true, centerOnly: false, null, screenSpace: false, null, EnemyPresentationStance.Normal, bodyPosition);
		}
		return flag | runtime.TryPlayTargetEffects(front, EnemyTargetPosition.ResolveSideFloor(array), profile, action, generation, back, null, atEntry: true, centerOnly: true, null, screenSpace: false, null, EnemyPresentationStance.Normal, null);
	}
}

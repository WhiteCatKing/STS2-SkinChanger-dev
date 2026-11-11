using System;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(SfxCmd), "PlayDeath", new Type[] { typeof(MonsterModel) })]
internal static class EnemyReplacementDeathSfxPatch
{
	private static bool Prefix(MonsterModel monster)
	{
		NCreature creatureNode;
		try
		{
			creatureNode = monster.Creature.GetCreatureNode();
		}
		catch
		{
			return true;
		}
		if (!EnemyActionPresentationPatch.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile))
		{
			return true;
		}
		TestSubject val = (TestSubject)(object)((monster is TestSubject) ? monster : null);
		if (val != null && ((MonsterModel)val).Creature.HasPower<AdaptablePower>())
		{
			return false;
		}
		CeremonialBeast val2 = (CeremonialBeast)(object)((monster is CeremonialBeast) ? monster : null);
		int choiceIndex = ((val2 != null && val2.IsInSecondPhase) ? 1 : 0);
		return !Entry.ActionPresentation.PlayAudioChoice(creatureNode, profile, EnemyAction.Death, choiceIndex);
	}
}

using System;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(BygoneEffigy), "WakeMove")]
internal static class BygoneEffigyWakePresentationPatch
{
	private static void Prefix(BygoneEffigy __instance)
	{
		try
		{
			NCreature creatureNode = ((MonsterModel)__instance).Creature.GetCreatureNode();
			if (creatureNode != null && !((GodotObject)creatureNode).IsQueuedForDeletion() && EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out EnemyReplacementProfile profile) && !(profile.Id != "cavalry_automata_for_bygone_effigy"))
			{
				creatureNode.SetAnimationTrigger("Cast");
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Bygone Effigy wake presentation failed: " + ex.Message, 1);
		}
	}
}

using CznEnemySkin.Domain;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NCreatureStateDisplay), "SetCreature")]
internal static class EnemyCombatOriginalNameCapturePatch
{
	internal const string OriginalNameMeta = "czn_enemy_skin_original_combat_name";

	private static void Prefix(NCreatureStateDisplay __instance, Creature creature)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		MonsterModel monster = creature.Monster;
		if (monster != null && Entry.Replacements.TryResolveMonster(((AbstractModel)monster).Id.Entry, out EnemyReplacementProfile _) && !string.IsNullOrWhiteSpace(creature.Name))
		{
			StringName obj = StringName.op_Implicit("czn_enemy_skin_original_combat_name");
			string name = creature.Name;
			((GodotObject)__instance).SetMeta(obj, Variant.From<string>(ref name));
		}
	}
}

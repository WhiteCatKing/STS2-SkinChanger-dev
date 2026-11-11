using System.Collections.Generic;
using System.Threading.Tasks;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBestiaryLayoutDecimillipede), "AnimAttack")]
internal static class DecimillipedeBestiaryAttackPatch
{
	private static void Postfix(List<NCreature> ____creatures, ref Task __result)
	{
		__result = DecimillipedePresentationAdapter.PlayBestiarySequentially(____creatures, __result);
	}
}

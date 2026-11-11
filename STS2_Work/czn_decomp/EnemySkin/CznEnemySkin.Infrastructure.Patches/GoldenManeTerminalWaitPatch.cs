using System.Threading.Tasks;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
internal static class GoldenManeTerminalWaitPatch
{
	private static void Postfix(Creature creature, string triggerName, ref Task __result)
	{
		if (!(triggerName != "Erupt") && creature.Monster is WaterfallGiant && EnemyVisualHostAdapter.TryResolveAppliedProfile(creature.GetCreatureNode(), out EnemyReplacementProfile profile) && !(profile.Id != "golden_mane_for_waterfall_giant"))
		{
			global::_003C_003Ey__InlineArray2<Task> buffer = default(global::_003C_003Ey__InlineArray2<Task>);
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 0) = __result;
			global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<global::_003C_003Ey__InlineArray2<Task>, Task>(ref buffer, 1) = Cmd.Wait(2.25f, false);
			__result = Task.WhenAll(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<global::_003C_003Ey__InlineArray2<Task>, Task>(in buffer, 2));
		}
	}
}

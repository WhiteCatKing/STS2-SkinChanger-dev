using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;

namespace CznCore.Presentation;

internal static class SharedDefenseTransport
{
	[HarmonyPatch(typeof(Hook), "AfterDamageReceived")]
	private static class FullyBlockedBreak
	{
		private static void Prefix(Creature target, DamageResult result)
		{
			if (SharedDefenseRules.NeedsFullyBlockedBreakSound(result))
			{
				SharedShieldBreakAudio.TryReplace(target);
			}
		}
	}

	[HarmonyPatch(typeof(Creature), "DamageBlockInternal")]
	private static class Damaged
	{
		private static void Prefix(Creature __instance, out int __state)
		{
			__state = __instance.Block;
		}

		private static void Postfix(Creature __instance, int __state)
		{
			SkinEventHub.Publish(new DefenseBlockDamaged(__instance, __state, __instance.Block));
		}
	}

	[HarmonyPatch(typeof(Hook), "AfterBlockBroken")]
	private static class Broken
	{
		private static void Prefix(Creature target)
		{
			SkinEventHub.Publish(new DefenseBlockBroken(target));
		}
	}

	public static IReadOnlyList<Type> PatchTypes { get; } = new _003C_003Ez__ReadOnlyArray<Type>(new Type[5]
	{
		SkinTransportPatches.BlockGainPatchType,
		typeof(Damaged),
		typeof(Broken),
		typeof(FullyBlockedBreak),
		typeof(SharedShieldBreakAudio)
	});
}

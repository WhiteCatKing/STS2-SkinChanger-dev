using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Combat;

namespace PengoTarot.Data.Divination;

internal static class EliteDivinationSharedState
{
	private static readonly ConditionalWeakTable<ICombatState, HashSet<ulong>> ChariotTriggered = new ConditionalWeakTable<ICombatState, HashSet<ulong>>();

	private static readonly ConditionalWeakTable<ICombatState, HashSet<ulong>> StrengthTriggered = new ConditionalWeakTable<ICombatState, HashSet<ulong>>();

	public static HashSet<ulong> Chariot(ICombatState combat)
	{
		return ChariotTriggered.GetValue(combat, (ICombatState _) => new HashSet<ulong>());
	}

	public static HashSet<ulong> Strength(ICombatState combat)
	{
		return StrengthTriggered.GetValue(combat, (ICombatState _) => new HashSet<ulong>());
	}
}

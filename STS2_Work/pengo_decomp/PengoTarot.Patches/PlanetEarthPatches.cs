using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using PengoTarot.Powers;

namespace PengoTarot.Patches;

public static class PlanetEarthPatches
{
	[HarmonyPatch(typeof(PlayerCombatState), "GainEnergy")]
	internal static class GainEnergy_Patch
	{
		private static void Postfix(PlayerCombatState __instance)
		{
			SyncEnergy(__instance);
		}
	}

	[HarmonyPatch(typeof(PlayerCombatState), "LoseEnergy")]
	internal static class LoseEnergy_Patch
	{
		private static void Postfix(PlayerCombatState __instance)
		{
			SyncEnergy(__instance);
		}
	}

	[HarmonyPatch(typeof(PlayerCombatState), "get_MaxEnergy")]
	internal static class MaxEnergy_Get_Patch
	{
		private static void Postfix(PlayerCombatState __instance, ref int __result)
		{
			if (_syncingMax)
			{
				return;
			}
			object? value = _playerField.GetValue(__instance);
			Player val = (Player)((value is Player) ? value : null);
			if (val == null)
			{
				return;
			}
			PlanetEarthPower power = val.Creature.GetPower<PlanetEarthPower>();
			if (power == null || power.PairedPlayers.Count == 0)
			{
				return;
			}
			_syncingMax = true;
			PlanetEarthPower.DisableEarthModifier = true;
			try
			{
				HashSet<Player> hashSet = new HashSet<Player>();
				int num = 0;
				hashSet.Add(val);
				num += val.PlayerCombatState.MaxEnergy;
				foreach (Player pairedPlayer in power.PairedPlayers)
				{
					if (pairedPlayer != null && pairedPlayer.PlayerCombatState != null && hashSet.Add(pairedPlayer))
					{
						num += pairedPlayer.PlayerCombatState.MaxEnergy;
					}
				}
				__result = num;
			}
			finally
			{
				PlanetEarthPower.DisableEarthModifier = false;
				_syncingMax = false;
			}
		}
	}

	private static readonly FieldInfo _playerField = AccessTools.Field(typeof(PlayerCombatState), "_player");

	private static readonly FieldInfo _maxEnergyBackingField = AccessTools.Field(typeof(Player), "<MaxEnergy>k__BackingField");

	private static bool _syncingEnergy;

	private static bool _syncingMax;

	private static void SyncEnergy(PlayerCombatState state)
	{
		if (_syncingEnergy)
		{
			return;
		}
		object? value = _playerField.GetValue(state);
		Player val = (Player)((value is Player) ? value : null);
		if (val == null)
		{
			return;
		}
		PlanetEarthPower power = val.Creature.GetPower<PlanetEarthPower>();
		if (power == null || power.PairedPlayers.Count == 0)
		{
			return;
		}
		int energy = state.Energy;
		_syncingEnergy = true;
		try
		{
			foreach (Player pairedPlayer in power.PairedPlayers)
			{
				PlayerCombatState val2 = ((pairedPlayer != null) ? pairedPlayer.PlayerCombatState : null);
				if (val2 != null && val2.Energy != energy)
				{
					val2.Energy = energy;
				}
			}
		}
		finally
		{
			_syncingEnergy = false;
		}
	}
}

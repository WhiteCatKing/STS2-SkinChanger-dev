using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.GameOverScreen;
using MegaCrit.Sts2.Core.Runs;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch(typeof(NGameOverScreen), "MoveCreaturesToDifferentLayerAndDisableUi")]
internal static class GameOverWearPatch
{
	[HarmonyPostfix]
	private static void Postfix(NGameOverScreen __instance)
	{
		try
		{
			if (NCombatRoom.Instance != null || !WearSettingsRuntime.Current.WearEnabled)
			{
				return;
			}
			object? obj = AccessTools.Field(typeof(NGameOverScreen), "_runState")?.GetValue(__instance);
			RunState val = (RunState)((obj is RunState) ? obj : null);
			object? obj2 = AccessTools.Field(typeof(NGameOverScreen), "_creatureContainer")?.GetValue(__instance);
			Node val2 = (Node)((obj2 is Node) ? obj2 : null);
			if (val == null || val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				return;
			}
			List<NCreatureVisuals> list = new List<NCreatureVisuals>();
			foreach (Node child in val2.GetChildren(false))
			{
				NCreatureVisuals val3 = (NCreatureVisuals)(object)((child is NCreatureVisuals) ? child : null);
				if (val3 != null)
				{
					list.Add(val3);
				}
			}
			for (int i = 0; i < val.Players.Count && i < list.Count; i++)
			{
				Player val4 = val.Players[i];
				if (!(((val4 != null) ? val4.Character : null) is Ironclad) || !SkinSelectionGate.PlayerAllowed(val4))
				{
					continue;
				}
				Node val5 = SpineSwap.FindSpine((Node?)(object)list[i]);
				if (val5 == null || SkinCatalog.StageFromPath("ironclad", SpineSwap.GetSkeletonPath(val5)) == 0)
				{
					continue;
				}
				int num = WearSettingsRuntime.Current.ResolveDeathStage(val4.Creature.CurrentHp, val4.Creature.MaxHp);
				if (num != 0)
				{
					Resource val6 = SpineSwap.LoadStageData("ironclad", num);
					if (val6 != null && SpineSwap.SetSkeletonData(val5, val6))
					{
						SpineSwap.Play(val5, "die", loop: false);
					}
				}
			}
		}
		catch (Exception ex)
		{
			ModEntry.Log.Info("game-over wear skipped: " + ex.Message, 1);
		}
	}
}

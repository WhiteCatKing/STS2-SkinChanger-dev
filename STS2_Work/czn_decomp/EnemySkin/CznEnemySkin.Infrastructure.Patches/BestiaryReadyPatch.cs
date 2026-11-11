using System;
using CznEnemySkin.Presentation.Bestiary;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBestiary), "_Ready")]
internal static class BestiaryReadyPatch
{
	private static void Postfix(NBestiary __instance)
	{
		try
		{
			if (((Node)__instance).GetNodeOrNull<BestiaryReplacementToggle>(NodePath.op_Implicit("CznEnemySkinReplacementToggle")) == null)
			{
				BestiaryReplacementToggle bestiaryReplacementToggle = new BestiaryReplacementToggle();
				bestiaryReplacementToggle.Initialize(Entry.Replacements, delegate(NBestiaryEntry entry)
				{
					BestiarySelectionAdapter.Refresh(__instance, entry);
				});
				((Node)__instance).AddChild((Node)(object)bestiaryReplacementToggle, false, (InternalMode)0);
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not attach enemy replacement Bestiary toggle: " + ex.Message, 1);
		}
	}
}

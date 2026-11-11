using System;
using CznEnemySkin.Domain;
using CznEnemySkin.Presentation.Bestiary;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBestiary), "SelectMonster")]
internal static class BestiarySelectMonsterPatch
{
	private static void Postfix(NBestiary __instance, NBestiaryEntry entry)
	{
		try
		{
			BestiaryReplacementToggle nodeOrNull = ((Node)__instance).GetNodeOrNull<BestiaryReplacementToggle>(NodePath.op_Implicit("CznEnemySkinReplacementToggle"));
			if (nodeOrNull != null)
			{
				if (!BestiaryProfileResolver.TryResolve(entry, out EnemyReplacementProfile profile))
				{
					nodeOrNull.HideProfile();
					return;
				}
				nodeOrNull.ShowProfile(profile, entry, BestiaryProfileResolver.ResolveLinkedProfiles(entry, profile));
				BestiaryProfileResolver.RefreshLabels(__instance, entry, profile);
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not update enemy replacement Bestiary UI: " + ex.Message, 1);
		}
	}
}

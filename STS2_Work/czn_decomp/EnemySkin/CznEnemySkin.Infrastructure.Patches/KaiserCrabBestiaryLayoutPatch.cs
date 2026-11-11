using System;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBestiaryLayoutKaiserCrab), "Setup")]
internal static class KaiserCrabBestiaryLayoutPatch
{
	private const float DefaultLayoutVisualCenterYOffset = 48f;

	private static void Postfix(NBestiaryLayoutKaiserCrab __instance)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			NCreature val = null;
			foreach (NCreature creature in ((NBestiaryLayout)__instance).GetCreatures())
			{
				if (EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile) && IsKaiserCrabMember(profile))
				{
					val = creature;
					break;
				}
			}
			if (val != null)
			{
				CanvasItem nodeOrNull = ((Node)__instance).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("%KaiserCrab"));
				if (nodeOrNull != null)
				{
					nodeOrNull.Visible = false;
				}
				((Control)val).Position = new Vector2(((Control)__instance).Size.X * 0.5f, ((Control)__instance).Size.Y * 0.5f + 48f + val.Hitbox.Size.Y * 0.5f);
			}
		}
		catch (Exception ex)
		{
			Entry.Log.Warn("Could not adapt Kaiser Crab Bestiary layout: " + ex.Message, 1);
		}
	}

	private static bool IsKaiserCrabMember(EnemyReplacementProfile profile)
	{
		string monsterId = profile.Target.MonsterId;
		if (monsterId == "CRUSHER" || monsterId == "ROCKET")
		{
			return true;
		}
		return false;
	}
}

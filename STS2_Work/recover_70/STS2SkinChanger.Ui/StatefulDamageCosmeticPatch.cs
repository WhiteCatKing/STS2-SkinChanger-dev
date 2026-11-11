using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Singleton;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(AbstractModel), "AfterDamageGiven")]
internal static class StatefulDamageCosmeticPatch
{
	private static void Postfix(AbstractModel __instance, Creature target, CardModel? cardSource)
	{
		if (!(__instance is MultiplayerScalingModel) || cardSource == null)
		{
			return;
		}
		StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(cardSource);
		if (runtime == null || !runtime.Enabled || runtime.Simple)
		{
			return;
		}
		try
		{
			if (cardSource is Clash && runtime.Setting("EnableClashAsGrandFinale", fallback: true))
			{
				NGrandFinaleImpactVfx val = NGrandFinaleImpactVfx.Create(target);
				if (val != null)
				{
					NCombatRoom instance = NCombatRoom.Instance;
					if (instance != null)
					{
						((Node)instance.CombatVfxContainer).AddChild((Node)(object)val, false, (InternalMode)0);
					}
				}
				return;
			}
			bool flag;
			switch (((object)cardSource).GetType().Name)
			{
			case "Squeeze":
			case "Flatten":
			case "Rattle":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				StatefulCombatEffects.Play(cardSource, target, runtime);
			}
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("damage cosmetic", exception);
		}
	}
}

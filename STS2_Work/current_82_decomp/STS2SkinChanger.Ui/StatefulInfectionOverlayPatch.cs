using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(CardModel), "CreateOverlay")]
internal static class StatefulInfectionOverlayPatch
{
	private static void Postfix(CardModel __instance, ref Control? __result)
	{
		if (((object)__instance).GetType().Name != "Infection")
		{
			return;
		}
		StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(__instance);
		if (runtime == null || !runtime.Enabled)
		{
			return;
		}
		try
		{
			PackedScene val = SkinService.LoadCardPresentationResource<PackedScene>(__instance, runtime.Contract.ResourceRoot + "/scenes/cards/overlays/red_infection.tscn");
			if (val != null)
			{
				Control val2 = val.Instantiate<Control>((GenEditState)0);
				Control? obj = __result;
				if (obj != null)
				{
					((Node)obj).QueueFree();
				}
				__result = val2;
			}
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("infection overlay", exception);
		}
	}
}

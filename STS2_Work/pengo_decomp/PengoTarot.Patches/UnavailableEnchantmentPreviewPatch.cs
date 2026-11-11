using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;

namespace PengoTarot.Patches;

[HarmonyPatch]
internal static class UnavailableEnchantmentPreviewPatch
{
	[HarmonyPatch(typeof(NCardHolder), "DoCardHoverEffects")]
	[HarmonyPostfix]
	private static void CardHoverPostfix(NCardHolder __instance, bool isHovered)
	{
		Node parent = ((Node)__instance).GetParent();
		if (((parent != null) ? parent.GetParent() : null) is NChooseACardSelectionScreen)
		{
			if (isHovered)
			{
				UnavailableEnchantmentPreview.NotifyHovered(__instance);
			}
			else
			{
				UnavailableEnchantmentPreview.NotifyUnhovered(__instance);
			}
		}
	}

	[HarmonyPatch(typeof(NChooseACardSelectionScreen), "SelectHolder")]
	[HarmonyPrefix]
	private static void CardSelectedPrefix()
	{
		UnavailableEnchantmentPreview.RetractImmediately();
	}

	[HarmonyPatch(typeof(NChooseACardSelectionScreen), "OnSkipButtonReleased")]
	[HarmonyPrefix]
	private static void ReturnedPrefix()
	{
		UnavailableEnchantmentPreview.RetractImmediately();
	}

	[HarmonyPatch(typeof(NChooseACardSelectionScreen), "AfterOverlayHidden")]
	[HarmonyPostfix]
	private static void OverlayHiddenPostfix()
	{
		UnavailableEnchantmentPreview.RetractImmediately();
	}

	[HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuOpened")]
	[HarmonyPrefix]
	private static void SettingsOpenedPrefix()
	{
		UnavailableEnchantmentPreview.RetractImmediately();
	}

	[HarmonyPatch(typeof(NSettingsScreen), "OnSubmenuShown")]
	[HarmonyPrefix]
	private static void SettingsShownPrefix()
	{
		UnavailableEnchantmentPreview.RetractImmediately();
	}

	[HarmonyPatch(typeof(NDeckEnchantSelectScreen), "ShowScreen")]
	[HarmonyPostfix]
	private static void EnchantScreenOpenedPostfix(EnchantmentModel enchantment)
	{
		UnavailableEnchantmentPreview.ShowForEnchantment(enchantment);
	}

	[HarmonyPatch(typeof(NCardGridSelectionScreen), "AfterOverlayHidden")]
	[HarmonyPostfix]
	private static void GridOverlayHiddenPostfix(NCardGridSelectionScreen __instance)
	{
		if (__instance is NDeckEnchantSelectScreen)
		{
			UnavailableEnchantmentPreview.RetractImmediately();
		}
	}
}

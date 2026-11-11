using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace PengoTarot.BalatroEffect;

public static class CardPlayPatch
{
	[HarmonyPatch(typeof(NPlayerHand), "StartCardPlay")]
	public static class StartCardPlay_Patch
	{
		public static void Prefix(NHandCardHolder holder)
		{
			NCard cardNode = ((NCardHolder)holder).CardNode;
			CardModel val = ((cardNode != null) ? cardNode.Model : null);
			if (val != null)
			{
				CardPlayTracker.MarkDragStarted(((object)((AbstractModel)val).Id).ToString());
			}
		}
	}

	[HarmonyPatch(typeof(CardModel), "OnPlayWrapper")]
	public static class OnPlayWrapper_Patch
	{
		public static void Postfix(CardModel __instance)
		{
			CardPlayTracker.MarkPlayFinished(((object)((AbstractModel)__instance).Id).ToString());
		}
	}

	[HarmonyPatch(typeof(NCardPlay), "Cleanup")]
	public static class Cleanup_Patch
	{
		public static void Postfix(NCardPlay __instance)
		{
			object? obj = CardProp?.GetValue(__instance);
			CardModel val = (CardModel)((obj is CardModel) ? obj : null);
			if (val != null)
			{
				CardPlayTracker.MarkPlayFinished(((object)((AbstractModel)val).Id).ToString());
			}
		}
	}

	private static readonly PropertyInfo? CardProp = AccessTools.Property(typeof(NCardPlay), "Card");
}

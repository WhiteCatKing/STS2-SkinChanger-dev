using System;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch]
internal static class CardPlayNoneCastPatch
{
	private static readonly FieldInfo? VisualAttackerField = AccessTools.Field(typeof(AttackCommand), "_visualAttacker");

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Hook), "BeforeCardPlayed")]
	private static void BeforeCardPlayedPrefix(CardPlay cardPlay)
	{
		if (PlayAllowed(cardPlay))
		{
			CardPlayAnimTracker.Begin(cardPlay);
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CreatureCmd), "TriggerAnim")]
	private static void TriggerAnimPrefix(Creature creature, ref string triggerName)
	{
		if (creature != null && creature.IsPlayer && PlayerSelected(creature.Player))
		{
			string text = CardAnimDefault.Resolve(triggerName);
			if (text != null && !string.Equals(text, triggerName, StringComparison.Ordinal))
			{
				triggerName = text;
			}
			CardPlayAnimTracker.NotePlayerAnim();
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(AttackCommand), "Execute")]
	private static void AttackCommandExecutePrefix(AttackCommand __instance)
	{
		try
		{
			Creature val = ((__instance != null) ? __instance.Attacker : null);
			if (((val != null) ? val.Player : null) != null && PlayerSelected(val.Player) && !((Creature)(((object)/*isinst with value type is only supported in some contexts*/) ?? ((object)val))).IsPlayer)
			{
				CardPlayAnimTracker.NotePlayerAnim();
			}
		}
		catch
		{
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CreatureCmd), "GainBlock", new Type[]
	{
		typeof(Creature),
		typeof(decimal),
		typeof(ValueProp),
		typeof(CardPlay),
		typeof(bool)
	})]
	private static void GainBlockPrefix(Creature creature, CardPlay cardPlay)
	{
		if (creature == null || !creature.IsPlayer || !PlayerSelected(creature.Player))
		{
			return;
		}
		try
		{
			CardModel val = ((cardPlay != null) ? cardPlay.Card : null);
			if (val != null && CardPlayAnimTracker.NeedsCast(cardPlay) && ShouldHandle(val))
			{
				Player owner = val.Owner;
				if (((owner != null) ? owner.Creature : null) != null && creature == owner.Creature)
				{
					InjectDefaultAnim(creature, val, "GainBlock");
					CardPlayAnimTracker.NotePlayerAnim();
				}
			}
		}
		catch
		{
		}
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(CombatHistory), "CardPlayFinished")]
	private static void CardPlayFinishedPrefix(CardPlay cardPlay)
	{
		TryInjectAfterPlay(cardPlay, "CardPlayFinished");
	}

	[HarmonyPrefix]
	[HarmonyPatch(typeof(Hook), "AfterCardPlayed")]
	private static void AfterCardPlayedPrefix(CardPlay cardPlay)
	{
		TryInjectAfterPlay(cardPlay, "AfterCardPlayed");
	}

	private static void TryInjectAfterPlay(CardPlay cardPlay, string reason)
	{
		if (cardPlay == null)
		{
			return;
		}
		try
		{
			CardModel card = cardPlay.Card;
			if (card == null || !ShouldHandle(card))
			{
				CardPlayAnimTracker.ShouldInjectCast(cardPlay);
			}
			else if (CardPlayAnimTracker.ShouldInjectCast(cardPlay))
			{
				Player owner = card.Owner;
				Creature val = ((owner != null) ? owner.Creature : null);
				if (val != null && !val.IsDead)
				{
					InjectDefaultAnim(val, card, reason);
				}
			}
		}
		catch
		{
		}
	}

	private static void InjectDefaultAnim(Creature creature, CardModel card, string reason)
	{
		string text = CardAnimDefault.ResolveMissingForCard(((object)card).GetType().Name);
		NCreature creatureNode = creature.GetCreatureNode();
		if (creatureNode == null)
		{
			return;
		}
		try
		{
			Player player = creature.Player;
			CharacterModel val = ((player != null) ? player.Character : null);
			if (creature.IsPlayer && val != null)
			{
				SfxCmd.Play((text == "Attack") ? val.AttackSfx : val.CastSfx, 1f);
			}
		}
		catch
		{
		}
		creatureNode.SetAnimationTrigger(text);
		ModEntry.TryInfo($"[card-anim] {((object)card).GetType().Name} -> {text} ({reason})");
	}

	private static bool ShouldHandle(CardModel card)
	{
		try
		{
			Player owner = card.Owner;
			if (((owner != null) ? owner.Creature : null) == null || !owner.Creature.IsPlayer)
			{
				return false;
			}
			return PlayerSelected(owner);
		}
		catch
		{
			return false;
		}
	}

	private static bool PlayAllowed(CardPlay? play)
	{
		if (play == null)
		{
			return false;
		}
		try
		{
			CardModel card = play.Card;
			Player val = ((card != null) ? card.Owner : null);
			return (val != null) ? PlayerSelected(val) : SkinSelectionGate.SceneAllowed();
		}
		catch
		{
			return false;
		}
	}

	private static bool PlayerSelected(Player? player)
	{
		try
		{
			if (player == null || !(player.Character is Ironclad))
			{
				return false;
			}
			return SkinSelectionGate.PlayerAllowed(player);
		}
		catch
		{
			return false;
		}
	}
}

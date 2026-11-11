using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class SkinEventOwnership
{
	private static readonly HashSet<Type> Broadcasts = new HashSet<Type>
	{
		typeof(RoomEnteringEvent),
		typeof(PresentationMainMenuEvent),
		typeof(CombatStartingEvent),
		typeof(RunEndedEvent),
		typeof(TreasureChestOpeningEvent),
		typeof(MerchantCharacterReadyEvent),
		typeof(FakeMerchantRoomLoadedEvent),
		typeof(MultiplayerLoadScreenStartedEvent),
		typeof(CharacterSelectedEvent),
		typeof(RandomCharacterResolvedEvent),
		typeof(CharacterSelectButtonSelectedEvent),
		typeof(PotionUsedEvent)
	};

	internal static bool IsClassified(Type eventType)
	{
		if (!Broadcasts.Contains(eventType))
		{
			return IsOwned(eventType);
		}
		return true;
	}

	internal static void RequireClassified(Type eventType)
	{
		if (!IsClassified(eventType))
		{
			throw new InvalidOperationException("Unclassified owned presentation event: " + eventType.Name);
		}
	}

	private static bool IsOwned(Type eventType)
	{
		if (!(eventType == typeof(NativeBlockGainSfxReplacementRequest)) && !(eventType == typeof(NativeShieldBreakSfxRequest)) && !(eventType == typeof(BlockActuallyIncreasedEvent)) && !(eventType == typeof(RestSiteCharacterReadyEvent)) && !(eventType == typeof(RestSiteCharacterFlippedEvent)) && !(eventType == typeof(RestSiteHealedEvent)) && !(eventType == typeof(RestSiteSmithedEvent)) && !(eventType == typeof(PlayerTurnStartedEvent)) && !(eventType == typeof(DamageReceivedEvent)) && !(eventType == typeof(PlayerDeathResolvedEvent)) && !(eventType == typeof(PlayerCombatVictoryEvent)) && !(eventType == typeof(RunFinishedPresentationEvent)) && !(eventType == typeof(CardPlayingEvent)) && !(eventType == typeof(CardPlayedEvent)) && !(eventType == typeof(CardPlayEnqueuingEvent)) && !(eventType == typeof(CardPlayCommittedEvent)) && !(eventType == typeof(LocalCardPlayedEvent)) && !(eventType == typeof(CardSelectionPresentationStartedEvent)) && !(eventType == typeof(HandCardFocusedEvent)) && !(eventType == typeof(HandCardUnfocusedEvent)) && !(eventType == typeof(HandCardHoverEffectsEvent)) && !(eventType == typeof(HandCardMousePressedEvent)) && !(eventType == typeof(MouseCardPlayStartedEvent)) && !(eventType == typeof(CardPlayCancelledEvent)))
		{
			return eventType == typeof(PlayerHandCardAddedEvent);
		}
		return true;
	}

	internal static bool Accepts<T>(CharacterSkinProfile profile, T value)
	{
		if (!(value is NativeBlockGainSfxReplacementRequest nativeBlockGainSfxReplacementRequest))
		{
			if (!(value is NativeShieldBreakSfxRequest nativeShieldBreakSfxRequest))
			{
				if (value is BlockActuallyIncreasedEvent)
				{
					object obj = value;
					return profile.AppliesTo(((BlockActuallyIncreasedEvent)((obj is BlockActuallyIncreasedEvent) ? obj : null)).Receiver.Player);
				}
				if (value is RestSiteCharacterReadyEvent)
				{
					object obj2 = value;
					return profile.AppliesTo(((RestSiteCharacterReadyEvent)((obj2 is RestSiteCharacterReadyEvent) ? obj2 : null)).Node.Player);
				}
				if (value is RestSiteCharacterFlippedEvent)
				{
					object obj3 = value;
					return profile.AppliesTo(((RestSiteCharacterFlippedEvent)((obj3 is RestSiteCharacterFlippedEvent) ? obj3 : null)).Node.Player);
				}
				if (value is RestSiteHealedEvent)
				{
					object obj4 = value;
					return profile.AppliesTo(((RestSiteHealedEvent)((obj4 is RestSiteHealedEvent) ? obj4 : null)).Player);
				}
				if (value is RestSiteSmithedEvent)
				{
					object obj5 = value;
					return profile.AppliesTo(((RestSiteSmithedEvent)((obj5 is RestSiteSmithedEvent) ? obj5 : null)).Player);
				}
				if (value is PlayerTurnStartedEvent)
				{
					object obj6 = value;
					return profile.AppliesTo(((PlayerTurnStartedEvent)((obj6 is PlayerTurnStartedEvent) ? obj6 : null)).Player);
				}
				if (value is DamageReceivedEvent)
				{
					object obj7 = value;
					return profile.AppliesTo(((DamageReceivedEvent)((obj7 is DamageReceivedEvent) ? obj7 : null)).Target.Player);
				}
				if (value is PlayerDeathResolvedEvent)
				{
					object obj8 = value;
					return profile.AppliesTo(((PlayerDeathResolvedEvent)((obj8 is PlayerDeathResolvedEvent) ? obj8 : null)).Player);
				}
				if (value is PlayerCombatVictoryEvent)
				{
					object obj9 = value;
					return profile.AppliesTo(((PlayerCombatVictoryEvent)((obj9 is PlayerCombatVictoryEvent) ? obj9 : null)).Player);
				}
				if (value is RunFinishedPresentationEvent)
				{
					object obj10 = value;
					return profile.AppliesTo(((RunFinishedPresentationEvent)((obj10 is RunFinishedPresentationEvent) ? obj10 : null)).Player);
				}
				if (value is CardPlayingEvent)
				{
					object obj11 = value;
					return profile.AppliesTo(((CardPlayingEvent)((obj11 is CardPlayingEvent) ? obj11 : null)).CardPlay.Player);
				}
				if (value is CardPlayedEvent)
				{
					object obj12 = value;
					return profile.AppliesTo(((CardPlayedEvent)((obj12 is CardPlayedEvent) ? obj12 : null)).CardPlay.Player);
				}
				if (value is CardPlayEnqueuingEvent)
				{
					object obj13 = value;
					return profile.AppliesTo(((CardPlayEnqueuingEvent)((obj13 is CardPlayEnqueuingEvent) ? obj13 : null)).Card.Owner);
				}
				if (value is CardPlayCommittedEvent)
				{
					object obj14 = value;
					return profile.AppliesTo(((CardPlayCommittedEvent)((obj14 is CardPlayCommittedEvent) ? obj14 : null)).Card.Owner);
				}
				if (value is LocalCardPlayedEvent)
				{
					object obj15 = value;
					return profile.AppliesTo(((LocalCardPlayedEvent)((obj15 is LocalCardPlayedEvent) ? obj15 : null)).Card.Owner);
				}
				if (value is CardSelectionPresentationStartedEvent)
				{
					object obj16 = value;
					return profile.AppliesTo(((CardSelectionPresentationStartedEvent)((obj16 is CardSelectionPresentationStartedEvent) ? obj16 : null)).Card.Owner);
				}
				if (value is HandCardFocusedEvent)
				{
					object obj17 = value;
					HandCardFocusedEvent handCardFocusedEvent = (HandCardFocusedEvent)((obj17 is HandCardFocusedEvent) ? obj17 : null);
					CardModel cardModel = ((NCardHolder)handCardFocusedEvent.Holder).CardModel;
					return profile.AppliesTo((cardModel != null) ? cardModel.Owner : null);
				}
				if (value is HandCardUnfocusedEvent)
				{
					object obj18 = value;
					HandCardUnfocusedEvent handCardUnfocusedEvent = (HandCardUnfocusedEvent)((obj18 is HandCardUnfocusedEvent) ? obj18 : null);
					CardModel cardModel2 = ((NCardHolder)handCardUnfocusedEvent.Holder).CardModel;
					return profile.AppliesTo((cardModel2 != null) ? cardModel2.Owner : null);
				}
				if (value is HandCardHoverEffectsEvent)
				{
					object obj19 = value;
					HandCardHoverEffectsEvent handCardHoverEffectsEvent = (HandCardHoverEffectsEvent)((obj19 is HandCardHoverEffectsEvent) ? obj19 : null);
					CardModel cardModel3 = ((NCardHolder)handCardHoverEffectsEvent.Holder).CardModel;
					return profile.AppliesTo((cardModel3 != null) ? cardModel3.Owner : null);
				}
				if (value is HandCardMousePressedEvent)
				{
					object obj20 = value;
					HandCardMousePressedEvent handCardMousePressedEvent = (HandCardMousePressedEvent)((obj20 is HandCardMousePressedEvent) ? obj20 : null);
					CardModel cardModel4 = ((NCardHolder)handCardMousePressedEvent.Holder).CardModel;
					return profile.AppliesTo((cardModel4 != null) ? cardModel4.Owner : null);
				}
				if (value is MouseCardPlayStartedEvent)
				{
					object obj21 = value;
					MouseCardPlayStartedEvent mouseCardPlayStartedEvent = (MouseCardPlayStartedEvent)((obj21 is MouseCardPlayStartedEvent) ? obj21 : null);
					NHandCardHolder holder = ((NCardPlay)mouseCardPlayStartedEvent.Node).Holder;
					object player;
					if (holder == null)
					{
						player = null;
					}
					else
					{
						CardModel cardModel5 = ((NCardHolder)holder).CardModel;
						player = ((cardModel5 != null) ? cardModel5.Owner : null);
					}
					return profile.AppliesTo((Player?)player);
				}
				if (value is CardPlayCancelledEvent)
				{
					object obj22 = value;
					CardPlayCancelledEvent cardPlayCancelledEvent = (CardPlayCancelledEvent)((obj22 is CardPlayCancelledEvent) ? obj22 : null);
					NHandCardHolder holder2 = cardPlayCancelledEvent.Node.Holder;
					object player2;
					if (holder2 == null)
					{
						player2 = null;
					}
					else
					{
						CardModel cardModel6 = ((NCardHolder)holder2).CardModel;
						player2 = ((cardModel6 != null) ? cardModel6.Owner : null);
					}
					return profile.AppliesTo((Player?)player2);
				}
				if (value is PlayerHandCardAddedEvent)
				{
					object obj23 = value;
					PlayerHandCardAddedEvent playerHandCardAddedEvent = (PlayerHandCardAddedEvent)((obj23 is PlayerHandCardAddedEvent) ? obj23 : null);
					NHandCardHolder? holder3 = playerHandCardAddedEvent.Holder;
					object obj24 = ((holder3 != null) ? ((NCardHolder)holder3).CardModel : null);
					if (obj24 == null)
					{
						NCard? card = playerHandCardAddedEvent.Card;
						obj24 = ((card != null) ? card.Model : null);
					}
					return profile.AppliesTo((obj24 != null) ? ((CardModel)obj24).Owner : null);
				}
				return Broadcasts.Contains(typeof(T));
			}
			return profile.AppliesTo(nativeShieldBreakSfxRequest.Target.Player);
		}
		return profile.AppliesTo(nativeBlockGainSfxReplacementRequest.Receiver.Player);
	}
}

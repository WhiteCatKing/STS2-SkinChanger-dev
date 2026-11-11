using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal static class CombatSupportFullScreenPresentationPolicy
{
	public static bool IsSupportCard(CardModel? card)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Invalid comparison between Unknown and I4
		CardType? val = ((card != null) ? new CardType?(card.Type) : ((CardType?)null));
		if (val.HasValue)
		{
			CardType valueOrDefault = val.GetValueOrDefault();
			if (valueOrDefault - 2 <= 1)
			{
				return true;
			}
		}
		return false;
	}

	public static bool Allows(CombatPresentationAudiencePolicy audiencePolicy, CardModel? card)
	{
		ArgumentNullException.ThrowIfNull(audiencePolicy, "audiencePolicy");
		if (IsSupportCard(card) && audiencePolicy.IsLocal((card != null) ? card.Owner : null))
		{
			return audiencePolicy.Allows(card, CombatPresentationChannel.CutIn);
		}
		return false;
	}
}

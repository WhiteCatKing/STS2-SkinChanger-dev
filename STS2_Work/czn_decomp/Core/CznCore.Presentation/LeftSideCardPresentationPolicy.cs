using System;

namespace CznCore.Presentation;

internal sealed class LeftSideCardPresentationPolicy<TCard>(Func<LeftSideCardPresentationMode> getMode, Func<TCard, bool> isEligibleCard, Func<TCard, bool> hasCutInRoute) where TCard : class
{
	public bool IsEnabled
	{
		get
		{
			LeftSideCardPresentationMode leftSideCardPresentationMode = _003CgetMode_003EP();
			if ((uint)(leftSideCardPresentationMode - 1) <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	public bool CanShow(TCard? card)
	{
		LeftSideCardPresentationMode leftSideCardPresentationMode = _003CgetMode_003EP();
		bool flag = (uint)(leftSideCardPresentationMode - 1) <= 1u;
		if (!flag || card == null || !_003CisEligibleCard_003EP(card))
		{
			return false;
		}
		if (leftSideCardPresentationMode != LeftSideCardPresentationMode.AllSupportedCards)
		{
			return _003ChasCutInRoute_003EP(card);
		}
		return true;
	}
}

using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal static class BattleReadyCardPlayEvents
{
	public static void AddSubscriptions(ICollection<IDisposable> subscriptions, string owner, Func<BattleReadyOverlayRuntime<CardModel>> getOverlay, Action<CardModel>? prepareCinematicOwnership = null)
	{
		subscriptions.Add(SkinEventHub.Subscribe(owner + " shared left-side card play", delegate(CardPlayingEvent e)
		{
			BattleReadyOverlayRuntime<CardModel> battleReadyOverlayRuntime = getOverlay();
			if (prepareCinematicOwnership != null)
			{
				if (!battleReadyOverlayRuntime.CanShow(e.CardPlay.Card))
				{
					return;
				}
				prepareCinematicOwnership(e.CardPlay.Card);
			}
			battleReadyOverlayRuntime.PlayForCard(e.CardPlay.Card);
		}));
		subscriptions.Add(SkinEventHub.Subscribe(owner + " shared left-side card complete", delegate(CardPlayedEvent e)
		{
			getOverlay().CompleteCardExecution(e.CardPlay.Card);
		}));
	}
}

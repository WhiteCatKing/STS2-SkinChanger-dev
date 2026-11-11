using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Data.Judgement;

public static class PortableTarotInventoryUi
{
	public static void RefreshLocal(ulong? changedPlayerId = null)
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		try
		{
			if (!RunManager.Instance.IsInProgress)
			{
				NConfigFloatingWindowEntryButton.SetPortableTarots(Array.Empty<CardModel>());
				return;
			}
			RunState val = RunManager.Instance.DebugOnlyGetState();
			if (val == null)
			{
				return;
			}
			Player me = LocalContext.GetMe((IPlayerCollection)(object)val);
			if (me == null || (changedPlayerId.HasValue && changedPlayerId.Value != me.NetId))
			{
				return;
			}
			List<CardModel> list = new List<CardModel>();
			foreach (string id in JudgementTarotInventoryState.GetTarotIds(me.NetId))
			{
				TarotDef tarotDef = TarotDeck.All.Find((TarotDef candidate) => candidate.Id == id);
				if (tarotDef != null)
				{
					CardModel item = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(tarotDef.CardType).Invoke(null, null);
					list.Add(item);
				}
			}
			NConfigFloatingWindowEntryButton.SetPortableTarots(list);
		}
		catch (Exception ex)
		{
			GD.PushWarning("[Judgement] 刷新携带栏失败：" + ex.Message);
		}
	}

	public static void PlayFullFeedback()
	{
		NConfigFloatingWindowEntryButton.PlayPortableTarotFullFeedback();
	}
}

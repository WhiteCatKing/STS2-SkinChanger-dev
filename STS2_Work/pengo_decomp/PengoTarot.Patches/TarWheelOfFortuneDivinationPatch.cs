using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;

namespace PengoTarot.Patches;

public static class TarWheelOfFortuneDivinationPatch
{
	private sealed class ShadowState
	{
		public NCard? Card;
	}

	private sealed class SelectionState
	{
		public readonly List<CardModel> SelectionOrder = new List<CardModel>();
	}

	[HarmonyPatch(typeof(Hook), "AfterCardChangedPiles")]
	private static class AfterCardChangedPilesPatch
	{
		[HarmonyPostfix]
		private static void Postfix(CardModel card, ref Task __result)
		{
			__result = ResolvePileChange(__result, card);
		}
	}

	[HarmonyPatch(typeof(NCardRewardSelectionScreen), "RefreshOptions")]
	private static class RewardScreenRefreshPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NCardRewardSelectionScreen __instance)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Callable val = Callable.From((Action)delegate
			{
				UpdateRewardScreen(__instance);
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	[HarmonyPatch(typeof(NMerchantCard), "FillSlot")]
	private static class MerchantCardFillSlotPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NMerchantCard __instance)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Callable val = Callable.From((Action)delegate
			{
				UpdateMerchantCard(__instance);
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	[HarmonyPatch(typeof(NGridCardHolder), "OnFreedToPool")]
	private static class GridCardHolderFreedToPoolPatch
	{
		[HarmonyPrefix]
		private static void Prefix(NGridCardHolder __instance)
		{
			ClearShadow((Node)(object)__instance);
		}
	}

	[HarmonyPatch(typeof(NSimpleCardSelectScreen), "OnCardClicked")]
	private static class SimpleSelectionClickedPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NSimpleCardSelectScreen __instance, CardModel card)
		{
			if (Traverse.Create((object)__instance).Field("_cardResults").GetValue() == null)
			{
				return;
			}
			HashSet<CardModel> selected = Traverse.Create((object)__instance).Field("_selectedCards").GetValue<HashSet<CardModel>>();
			SelectionState orCreateValue = SelectionStates.GetOrCreateValue(__instance);
			if (selected.Contains(card))
			{
				if (!orCreateValue.SelectionOrder.Contains(card))
				{
					orCreateValue.SelectionOrder.Add(card);
				}
			}
			else
			{
				orCreateValue.SelectionOrder.Remove(card);
			}
			orCreateValue.SelectionOrder.RemoveAll((CardModel candidate) => !selected.Contains(candidate));
			UpdateSimpleSelectionScreen(__instance, orCreateValue);
		}
	}

	[HarmonyPatch(typeof(NSimpleCardSelectScreen), "ConnectSignalsAndInitGrid")]
	private static class SimpleSelectionReadyPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NSimpleCardSelectScreen __instance)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Callable val = Callable.From((Action)delegate
			{
				UpdateSimpleSelectionScreen(__instance, SelectionStates.GetOrCreateValue(__instance));
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	[HarmonyPatch(typeof(NCardGridSelectionScreen), "CardsSelected")]
	private static class PreserveSimpleSelectionOrderPatch
	{
		[HarmonyPostfix]
		private static void Postfix(NCardGridSelectionScreen __instance, ref Task<IEnumerable<CardModel>> __result)
		{
			NSimpleCardSelectScreen val = (NSimpleCardSelectScreen)(object)((__instance is NSimpleCardSelectScreen) ? __instance : null);
			if (val != null && SelectionStates.TryGetValue(val, out SelectionState value))
			{
				__result = ReorderSelectedCards(__result, value);
			}
		}
	}

	private const int FlagIndex = 10;

	private const int CardInterval = 5;

	private const string ShadowName = "PengoTarotWheelCardShadow";

	private static readonly Vector2 ShadowOffset = new Vector2(33f, -44f);

	private static readonly ConditionalWeakTable<NSimpleCardSelectScreen, SelectionState> SelectionStates = new ConditionalWeakTable<NSimpleCardSelectScreen, SelectionState>();

	private static readonly ConditionalWeakTable<Node, ShadowState> ShadowStates = new ConditionalWeakTable<Node, ShadowState>();

	private static bool IsEnabled()
	{
		if (RunManager.Instance.IsInProgress)
		{
			return ConfigFloatingWindowRunData.GetTarFlag(10);
		}
		return false;
	}

	private static async Task ResolvePileChange(Task originalTask, CardModel card)
	{
		await originalTask;
		if (card.Owner.Creature.IsDead)
		{
			return;
		}
		CardPile pile = card.Pile;
		if (pile != null && (int)pile.Type == 6 && IsEnabled())
		{
			int num = ConfigFloatingWindowRunData.RecordWheelOfFortuneCard(card.Owner.NetId);
			Log.Info($"[PengoTarot] [WheelOfFortune] player={card.Owner.NetId} card={((AbstractModel)card).Id} count={num}", 2);
			if (num % 5 == 0)
			{
				CardPileAddResult val = await CardPileCmd.Add(((ICardScope)card.Owner.RunState).CloneCard(card), (PileType)6, (CardPilePosition)1, (AbstractModel)(object)card, false);
				CardCmd.PreviewCardPileAdd(val, 1.2f, (CardPreviewStyle)1);
				Log.Info($"[PengoTarot] [WheelOfFortune] copied={((AbstractModel)val.cardAdded).Id} success={val.success}", 2);
			}
			RefreshVisibleChoiceShadows();
		}
	}

	private static async Task<IEnumerable<CardModel>> ReorderSelectedCards(Task<IEnumerable<CardModel>> originalTask, SelectionState state)
	{
		HashSet<CardModel> hashSet = (await originalTask).ToHashSet();
		return state.SelectionOrder.Where(hashSet.Contains).ToList();
	}

	private static void UpdateRewardScreen(NCardRewardSelectionScreen screen)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)screen))
		{
			return;
		}
		Control value = Traverse.Create((object)screen).Field("_cardRow").GetValue<Control>();
		List<NGridCardHolder> list = ((value != null) ? ((IEnumerable)((Node)value).GetChildren(false)).OfType<NGridCardHolder>().ToList() : null);
		if (list == null || list.Count == 0)
		{
			return;
		}
		ulong netId = ((NCardHolder)list[0]).CardModel.Owner.NetId;
		bool visible = IsEnabled() && (ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(netId) + 1) % 5 == 0;
		foreach (NGridCardHolder item in list)
		{
			SetShadow(item, visible);
		}
	}

	private static void UpdateSimpleSelectionScreen(NSimpleCardSelectScreen screen, SelectionState state)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)screen) || Traverse.Create((object)screen).Field("_cardResults").GetValue() == null)
		{
			return;
		}
		NCardGrid value = Traverse.Create((object)screen).Field("_grid").GetValue<NCardGrid>();
		List<NGridCardHolder> list = ((value != null) ? value.CurrentlyDisplayedCardHolders.ToList() : null);
		if (list == null || list.Count == 0)
		{
			return;
		}
		int wheelOfFortuneCardCount = ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(((NCardHolder)list[0]).CardModel.Owner.NetId);
		HashSet<CardModel> hashSet = new HashSet<CardModel>();
		int num = wheelOfFortuneCardCount;
		foreach (CardModel item in state.SelectionOrder)
		{
			num++;
			if (num % 5 == 0)
			{
				hashSet.Add(item);
				num++;
			}
		}
		bool flag = (num + 1) % 5 == 0;
		foreach (NGridCardHolder item2 in list)
		{
			bool flag2 = state.SelectionOrder.Contains(((NCardHolder)item2).CardModel);
			bool visible = IsEnabled() && (hashSet.Contains(((NCardHolder)item2).CardModel) || (flag && !flag2));
			SetShadow(item2, visible);
		}
	}

	private static void SetShadow(NGridCardHolder holder, bool visible)
	{
		SetShadow((Node)(object)holder, ((NCardHolder)holder).CardNode, ((NCardHolder)holder).CardModel, visible);
	}

	private static void UpdateMerchantCard(NMerchantCard merchantCard)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)merchantCard))
		{
			Traverse obj = Traverse.Create((object)merchantCard);
			Control value = obj.Field("_cardHolder").GetValue<Control>();
			NCard value2 = obj.Field("_cardNode").GetValue<NCard>();
			if (value != null && ((value2 != null) ? value2.Model : null) != null)
			{
				bool visible = IsEnabled() && (ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(value2.Model.Owner.NetId) + 1) % 5 == 0;
				SetShadow((Node)(object)value, value2, value2.Model, visible);
			}
		}
	}

	private static void SetShadow(Node holder, NCard? cardNode, CardModel? cardModel, bool visible)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		ShadowState orCreateValue = ShadowStates.GetOrCreateValue(holder);
		NCard val = orCreateValue.Card;
		if (val != null && !GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			orCreateValue.Card = null;
			val = null;
		}
		NCard nodeOrNull = holder.GetNodeOrNull<NCard>(NodePath.op_Implicit("PengoTarotWheelCardShadow"));
		if (nodeOrNull != null && nodeOrNull != val)
		{
			ReleaseShadow(holder, nodeOrNull);
		}
		if (!visible)
		{
			ClearShadow(holder);
		}
		else
		{
			if (val != null && ((Node)val).GetParent() == holder && val.Model == cardModel)
			{
				return;
			}
			if (val != null)
			{
				ClearShadow(holder);
			}
			if (cardNode != null && cardModel != null)
			{
				NCard val2 = NCard.Create(cardModel, (ModelVisibility)1);
				if (val2 != null)
				{
					((CanvasItem)val2).Modulate = new Color(0.7f, 0.7f, 0.7f, 1f);
					((Control)val2).Position = ((Control)cardNode).Position + ShadowOffset;
					holder.AddChild((Node)(object)val2, false, (InternalMode)0);
					holder.MoveChild((Node)(object)val2, 0);
					val2.UpdateVisuals((PileType)0, (CardPreviewMode)1);
					orCreateValue.Card = val2;
				}
			}
		}
	}

	private static void ClearShadow(Node holder)
	{
		NCard val = null;
		if (ShadowStates.TryGetValue(holder, out ShadowState value))
		{
			val = value.Card;
			value.Card = null;
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				ReleaseShadow(holder, val);
			}
		}
		NCard nodeOrNull = holder.GetNodeOrNull<NCard>(NodePath.op_Implicit("PengoTarotWheelCardShadow"));
		if (nodeOrNull != null && nodeOrNull != val)
		{
			ReleaseShadow(holder, nodeOrNull);
		}
	}

	private static void ReleaseShadow(Node holder, NCard shadow)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)shadow).GetParent() == holder)
		{
			holder.RemoveChild((Node)(object)shadow);
		}
		((Node)shadow).Name = StringName.op_Implicit("Card");
		((Control)shadow).Position = Vector2.Zero;
		((CanvasItem)shadow).Modulate = Colors.White;
		GodotTreeExtensions.QueueFreeSafely((Node)(object)shadow);
	}

	private static void RefreshVisibleChoiceShadows()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (((val != null) ? val.Root : null) == null)
		{
			return;
		}
		foreach (Node item in ((Node)val.Root).FindChildren("NCardRewardSelectionScreen", "", true, false))
		{
			NCardRewardSelectionScreen val2 = (NCardRewardSelectionScreen)(object)((item is NCardRewardSelectionScreen) ? item : null);
			if (val2 != null)
			{
				UpdateRewardScreen(val2);
			}
		}
		foreach (Node item2 in ((Node)val.Root).FindChildren("*", "", true, false))
		{
			NMerchantCard val3 = (NMerchantCard)(object)((item2 is NMerchantCard) ? item2 : null);
			if (val3 != null)
			{
				UpdateMerchantCard(val3);
			}
		}
	}
}

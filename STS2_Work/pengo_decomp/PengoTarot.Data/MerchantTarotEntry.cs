using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.ConfigFW;
using PengoTarot.Data.Judgement;
using PengoTarot.Network;

namespace PengoTarot.Data;

public sealed class MerchantTarotEntry : MerchantEntry
{
	private bool _used;

	private List<CardModel>? _pendingCards;

	private Dictionary<CardModel, TarotDef>? _pendingMap;

	public override bool IsStocked => !_used;

	public bool HasPurchased => _used;

	public MerchantTarotEntry(Player player)
		: base(player)
	{
		Rng shops = player.PlayerRng.Shops;
		int num = Math.Max(0, ConfigFloatingWindowRunData.TarotPriceMin);
		int num2 = Math.Max(0, ConfigFloatingWindowRunData.TarotPriceMax);
		int tarotPriceOffset = ConfigFloatingWindowRunData.TarotPriceOffset;
		int num3 = (ConfigFloatingWindowRunData.GetTarFlag(5) ? (-100) : 0);
		base._cost = shops.NextInt(num, num2 + 1) + tarotPriceOffset + num3;
		InitPendingCards(player, null);
	}

	public override void CalcCost()
	{
	}

	protected override async Task<(bool, int)> OnTryPurchase(MerchantInventory? inventory, bool ignoreCost)
	{
		if (_used)
		{
			return (false, 0);
		}
		if (JudgementDivination.IsEnabled && !JudgementTarotInventoryState.HasOpenSlot(base._player.NetId))
		{
			if (LocalContext.IsMe(base._player))
			{
				PortableTarotInventoryUi.PlayFullFeedback();
			}
			return (false, 0);
		}
		int goldCost = ((!ignoreCost) ? ((MerchantEntry)this).Cost : 0);
		if (!RunManager.Instance.IsSingleplayerOrFakeMultiplayer && ModInitializer.TarotSync != null)
		{
			bool num = await DoMultiplayerPurchase(goldCost, ignoreCost);
			if (num)
			{
				_used = true;
				AdjustPriceAfterPurchase();
			}
			return (num, goldCost);
		}
		if (await DoPurchaseWithCache(base._player, goldCost, ignoreCost))
		{
			_used = true;
			AdjustPriceAfterPurchase();
			return (true, goldCost);
		}
		return (false, 0);
	}

	private static void AdjustPriceAfterPurchase()
	{
		ConfigFloatingWindowRunData.AdjustTarotPrice(50);
		if (ConfigFloatingWindowRunData.GetTarFlag(2))
		{
			ConfigFloatingWindowRunData.AdjustTarotPrice(-50);
		}
	}

	private async Task<bool> DoPurchaseWithCache(Player player, int goldCost, bool ignoreCost)
	{
		if (_pendingCards == null)
		{
			InitPendingCards(player, null);
		}
		CardModel val = await ShowTarotSelection(player, canSkip: true);
		if (val == null)
		{
			return false;
		}
		if (!(await TarotAcquisitionResolver.ResolveSelectedTarot(_pendingMap[val], player, isLocalBuyer: true, () => SpendGold(player, goldCost, ignoreCost))))
		{
			return false;
		}
		ClearPending();
		return true;
	}

	private async Task<bool> DoMultiplayerPurchase(int goldCost, bool ignoreCost)
	{
		List<string> cachedDefIds = null;
		if (_pendingCards != null && _pendingMap != null)
		{
			cachedDefIds = _pendingMap.Values.Select((TarotDef d) => d.Id).ToList();
		}
		TarotPurchaseRequestMessage tarotPurchaseRequestMessage = default(TarotPurchaseRequestMessage);
		tarotPurchaseRequestMessage.goldCost = goldCost;
		tarotPurchaseRequestMessage.cachedDefIds = cachedDefIds;
		tarotPurchaseRequestMessage.Location = default(RunLocation);
		TarotPurchaseRequestMessage msg = tarotPurchaseRequestMessage;
		ModInitializer.TarotSync.SendMessage(msg);
		return await ExecuteNetworkedPurchase(base._player, goldCost, ignoreCost, cachedDefIds, isLocalBuyer: true, canSkip: true);
	}

	public async Task<bool> ExecuteNetworkedPurchase(Player player, int goldCost, bool ignoreCostThis, List<string>? cachedDefIds, bool isLocalBuyer, bool canSkip)
	{
		if (_pendingCards == null)
		{
			if (cachedDefIds != null && cachedDefIds.Count > 0)
			{
				_pendingCards = new List<CardModel>();
				_pendingMap = new Dictionary<CardModel, TarotDef>();
				foreach (string defId in cachedDefIds)
				{
					TarotDef tarotDef = TarotDeck.All.First((TarotDef d) => d.Id == defId);
					CardModel val = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(tarotDef.CardType).Invoke(null, null);
					_pendingCards.Add(val);
					_pendingMap[val] = tarotDef;
				}
			}
			else
			{
				InitPendingCards(player, null);
			}
		}
		CardModel val2 = await ShowTarotSelection(player, canSkip);
		if (val2 == null)
		{
			return false;
		}
		if (!(await TarotAcquisitionResolver.ResolveSelectedTarot(_pendingMap[val2], player, isLocalBuyer, isLocalBuyer ? ((Func<Task>)(() => SpendGold(player, goldCost, ignoreCostThis))) : null)))
		{
			return false;
		}
		ClearPending();
		return true;
	}

	public static async Task<bool> HandleRemoteTarotPurchase(Player player, int goldCost, List<string>? cachedDefIds)
	{
		return await ExecuteNetworkedPurchaseStatic(player, goldCost, cachedDefIds, isLocalBuyer: false, canSkip: false);
	}

	private static async Task<bool> ExecuteNetworkedPurchaseStatic(Player player, int goldCost, List<string>? cachedDefIds, bool isLocalBuyer, bool canSkip)
	{
		if (JudgementDivination.IsEnabled && !JudgementTarotInventoryState.HasOpenSlot(player.NetId))
		{
			return false;
		}
		List<CardModel> list;
		Dictionary<CardModel, TarotDef> pendingMap;
		if (cachedDefIds != null && cachedDefIds.Count > 0)
		{
			list = new List<CardModel>();
			pendingMap = new Dictionary<CardModel, TarotDef>();
			foreach (string defId in cachedDefIds)
			{
				TarotDef tarotDef = TarotDeck.All.First((TarotDef d) => d.Id == defId);
				CardModel val = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(tarotDef.CardType).Invoke(null, null);
				list.Add(val);
				pendingMap[val] = tarotDef;
			}
		}
		else
		{
			Rng shops = player.PlayerRng.Shops;
			List<TarotDef> list2 = DrawTarotPackOptions(player, shops);
			list = new List<CardModel>();
			pendingMap = new Dictionary<CardModel, TarotDef>();
			foreach (TarotDef item in list2)
			{
				CardModel val2 = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(item.CardType).Invoke(null, null);
				list.Add(val2);
				pendingMap[val2] = item;
			}
		}
		CardModel val3 = await CardSelectCmd.FromChooseACardScreen((PlayerChoiceContext)(object)new SilentPlayerChoiceContext(), (IReadOnlyList<CardModel>)list, player, canSkip);
		if (val3 == null)
		{
			return false;
		}
		return await TarotAcquisitionResolver.ResolveSelectedTarot(pendingMap[val3], player, isLocalBuyer, isLocalBuyer ? ((Func<Task>)(() => SpendGold(player, goldCost, ignoreCost: false))) : null);
	}

	private async Task<CardModel?> ShowTarotSelection(Player player, bool canSkip)
	{
		return await CardSelectCmd.FromChooseACardScreen((PlayerChoiceContext)(object)new SilentPlayerChoiceContext(), (IReadOnlyList<CardModel>)_pendingCards, player, canSkip);
	}

	private static Task SpendGold(Player player, int goldCost, bool ignoreCost)
	{
		if (goldCost <= 0 || ignoreCost)
		{
			return Task.CompletedTask;
		}
		return PlayerCmd.LoseGold((decimal)goldCost, player, (GoldLossType)1);
	}

	private static List<TarotDef> DrawTarotPackOptions(Player player, Rng rng)
	{
		int count = 1 + (ConfigFloatingWindowRunData.GetTarFlag(1) ? 2 : 0);
		bool tarFlag = ConfigFloatingWindowRunData.GetTarFlag(3);
		bool tarFlag2 = ConfigFloatingWindowRunData.GetTarFlag(4);
		return TarotDeck.DrawUnique(player, rng, count, tarFlag, tarFlag2, JudgementDivination.IsEnabled ? new Func<TarotDef, bool>(TarotAcquisitionResolver.IsPortableCandidate) : null);
	}

	private void InitPendingCards(Player player, List<string>? defIds)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		Rng shops = player.PlayerRng.Shops;
		List<TarotDef> list = ((defIds == null) ? DrawTarotPackOptions(player, shops) : defIds.Select((string id) => TarotDeck.All.First((TarotDef d) => d.Id == id)).ToList());
		_pendingCards = new List<CardModel>();
		_pendingMap = new Dictionary<CardModel, TarotDef>();
		foreach (TarotDef item in list)
		{
			CardModel val = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(item.CardType).Invoke(null, null);
			_pendingCards.Add(val);
			_pendingMap[val] = item;
		}
	}

	private void ClearPending()
	{
		_pendingCards = null;
		_pendingMap = null;
	}

	protected override void ClearAfterPurchase()
	{
	}

	protected override void RestockAfterPurchase(MerchantInventory? inventory)
	{
	}
}

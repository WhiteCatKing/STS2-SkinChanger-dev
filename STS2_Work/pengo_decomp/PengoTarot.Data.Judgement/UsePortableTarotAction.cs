using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Data.Judgement;

public sealed class UsePortableTarotAction : GameAction
{
	private readonly Player _player;

	private readonly int _slotIndex;

	private readonly string _tarotId;

	private readonly uint _expectedRevision;

	private readonly PortableTarotUseContext _context;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => (GameActionType)3;

	public UsePortableTarotAction(Player player, int slotIndex, string tarotId, uint expectedRevision, PortableTarotUseContext context)
	{
		_player = player;
		_slotIndex = slotIndex;
		_tarotId = tarotId;
		_expectedRevision = expectedRevision;
		_context = context;
	}

	protected override async Task ExecuteAction()
	{
		TarotDef def = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.Id == _tarotId);
		IReadOnlyList<CardModel> readOnlyList = PortableTarotUseRouter.ResolveActionCards(_player, _context);
		if (def?.Enchantment == null || readOnlyList == null || !JudgementTarotInventoryState.TryGetSlot(_player.NetId, _slotIndex, out string tarotId, out uint revision) || revision != _expectedRevision || !string.Equals(tarotId, _tarotId, StringComparison.Ordinal))
		{
			((GameAction)this).Cancel();
			return;
		}
		CardModel[] array = ((IEnumerable<CardModel>)readOnlyList).Where((Func<CardModel, bool>)def.Enchantment.CanEnchant).ToArray();
		string id = def.Id;
		bool flag = ((id == "LOVERS_UPRIGHT" || id == "LOVERS_REVERSED") ? true : false);
		int num = ((!flag) ? 1 : def.CardsToEnchant);
		if (array.Length < num)
		{
			((GameAction)this).Cancel();
			return;
		}
		IReadOnlyList<CardModel> readOnlyList2;
		if (_context == PortableTarotUseContext.Deck)
		{
			CardSelectorPrefs val = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, num, def.CardsToEnchant);
			((CardSelectorPrefs)(ref val)).set_Cancelable(true);
			((CardSelectorPrefs)(ref val)).set_RequireManualConfirmation(true);
			CardSelectorPrefs prefs = val;
			GameActionPlayerChoiceContext choiceContext = new GameActionPlayerChoiceContext((GameAction)(object)this);
			readOnlyList2 = await PortableTarotUseRouter.SelectDeckTargetsInAction(_player, array, def.Enchantment, def.CardsToEnchant, prefs, (PlayerChoiceContext)(object)choiceContext);
		}
		else
		{
			readOnlyList2 = array;
		}
		if (readOnlyList2.Count == 0)
		{
			((GameAction)this).Cancel();
			return;
		}
		if (!JudgementTarotInventoryState.TryDiscard(_player.NetId, _slotIndex, _tarotId, _expectedRevision))
		{
			((GameAction)this).Cancel();
			return;
		}
		foreach (CardModel item in readOnlyList2)
		{
			CardCmd.Enchant(def.Enchantment.ToMutable(), item, 1m);
		}
		PortableTarotUseRouter.RefreshVisibleCards(readOnlyList2);
	}

	public override INetAction ToNetAction()
	{
		NetUsePortableTarotAction netUsePortableTarotAction = default(NetUsePortableTarotAction);
		netUsePortableTarotAction.SlotIndex = _slotIndex;
		netUsePortableTarotAction.TarotId = _tarotId;
		netUsePortableTarotAction.ExpectedRevision = _expectedRevision;
		netUsePortableTarotAction.Context = (int)_context;
		return (INetAction)(object)netUsePortableTarotAction;
	}
}

using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Cards;

namespace PengoTarot.Data.Judgement;

public sealed class UsePortableTarotCombatAction : GameAction
{
	private readonly Player _player;

	private readonly int _slotIndex;

	private readonly string _tarotId;

	private readonly uint _expectedRevision;

	public override ulong OwnerId => _player.NetId;

	public override GameActionType ActionType => (GameActionType)2;

	public UsePortableTarotCombatAction(Player player, int slotIndex, string tarotId, uint expectedRevision)
	{
		_player = player;
		_slotIndex = slotIndex;
		_tarotId = tarotId;
		_expectedRevision = expectedRevision;
	}

	protected override async Task ExecuteAction()
	{
		TarotDef tarotDef = TarotDeck.All.FirstOrDefault((TarotDef candidate) => candidate.Id == _tarotId);
		string tarotId;
		uint revision;
		bool flag = JudgementTarotInventoryState.TryGetSlot(_player.NetId, _slotIndex, out tarotId, out revision);
		ICombatState combatState = _player.Creature.CombatState;
		if (tarotDef == null || !TarotAcquisitionResolver.IsPortableCandidate(tarotDef) || tarotId != _tarotId || !flag || revision != _expectedRevision || combatState == null || CombatManager.Instance.IsOverOrEnding || _player.Creature.IsDead)
		{
			((GameAction)this).Cancel();
			return;
		}
		CardModel val = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(tarotDef.CardType).Invoke(null, null);
		TarCard tarot = (TarCard)(object)combatState.CreateCard(val, _player);
		tarot.MarkAsPortableCombatInstance();
		await CardCmd.AutoPlay((PlayerChoiceContext)new GameActionPlayerChoiceContext((GameAction)(object)this), (CardModel)(object)tarot, (Creature)null, (AutoPlayType)1, false, false);
		if (!tarot.PortableCombatEffectApplied || !JudgementTarotInventoryState.TryDiscard(_player.NetId, _slotIndex, _tarotId, _expectedRevision))
		{
			((GameAction)this).Cancel();
		}
	}

	public override INetAction ToNetAction()
	{
		NetUsePortableTarotCombatAction netUsePortableTarotCombatAction = default(NetUsePortableTarotCombatAction);
		netUsePortableTarotCombatAction.SlotIndex = _slotIndex;
		netUsePortableTarotCombatAction.TarotId = _tarotId;
		netUsePortableTarotCombatAction.ExpectedRevision = _expectedRevision;
		return (INetAction)(object)netUsePortableTarotCombatAction;
	}
}

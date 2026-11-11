using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data;

namespace PengoTarot.Cards;

public abstract class TarCard : CardModel
{
	private bool _isPortableCombatInstance;

	public bool PortableCombatEffectApplied { get; private set; }

	protected override int CanonicalEnergyCost => -1;

	public override IEnumerable<CardKeyword> CanonicalKeywords => Enumerable.Empty<CardKeyword>();

	public override bool HasBuiltInOverlay => true;

	public void MarkAsPortableCombatInstance()
	{
		_isPortableCombatInstance = true;
	}

	protected TarCard()
		: base(0, (CardType)0, (CardRarity)5, (TargetType)0, true)
	{
	}

	protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (_isPortableCombatInstance)
		{
			PortableCombatEffectApplied = await ExecutePortableCombatEffect(choiceContext, cardPlay);
		}
	}

	protected virtual Task<bool> ExecutePortableCombatEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return TarotCombatEffectExecutor.Execute(this, choiceContext, cardPlay);
	}

	protected override CardLocation GetResultLocationForCardPlay()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!_isPortableCombatInstance)
		{
			return ((CardModel)this).GetResultLocationForCardPlay();
		}
		return new CardLocation(((CardModel)this).Owner, (PileType)0, (CardPilePosition)1);
	}

	protected override void OnUpgrade()
	{
	}
}

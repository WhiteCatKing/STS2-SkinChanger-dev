using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Cards;

public abstract class PlanetCard : CardModel
{
	protected override int CanonicalEnergyCost => -1;

	public override IEnumerable<CardKeyword> CanonicalKeywords => Enumerable.Empty<CardKeyword>();

	public override bool HasBuiltInOverlay => false;

	protected PlanetCard(CardType type)
		: base(0, type, (CardRarity)5, (TargetType)0, true)
	{
	}//IL_0002: Unknown result type (might be due to invalid IL or missing references)


	protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		return Task.CompletedTask;
	}

	protected override void OnUpgrade()
	{
	}
}

using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Cards;

public sealed class TarWheelOfFortuneReversed : TarCard
{
	protected override void AddExtraArgsToDescription(LocString description)
	{
		((CardModel)this).AddExtraArgsToDescription(description);
		description.Add("JudgementDivination", JudgementDivination.IsEnabledForCardDisplay);
	}
}

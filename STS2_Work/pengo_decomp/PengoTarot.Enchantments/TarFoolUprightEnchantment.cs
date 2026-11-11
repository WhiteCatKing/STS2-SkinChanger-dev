using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarFoolUprightEnchantment : EnchantmentModel
{
	private bool _triggered;

	public override bool HasExtraCardText => true;

	public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I4
		if (card == ((EnchantmentModel)this).Card && !_triggered && ((int)card.Type == 1 || (int)card.Type == 2))
		{
			_triggered = true;
			((EnchantmentModel)this).Status = (EnchantmentStatus)1;
			cardLocation.pileType = (PileType)2;
			cardLocation.position = (CardPilePosition)1;
			return cardLocation;
		}
		return cardLocation;
	}

	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card != ((EnchantmentModel)this).Card || _triggered)
		{
			return;
		}
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) == null || (int)((EnchantmentModel)this).Card.Type != 3)
		{
			return;
		}
		_triggered = true;
		((EnchantmentModel)this).Status = (EnchantmentStatus)1;
		CardModel val = ((EnchantmentModel)this).Card.CreateClone();
		if (val != null)
		{
			CardCmd.ClearEnchantment(val);
			CardCmd.Enchant<TarFoolUprightEnchantment>(val, 1m);
			if (val.Enchantment is TarFoolUprightEnchantment tarFoolUprightEnchantment)
			{
				tarFoolUprightEnchantment._triggered = true;
				((EnchantmentModel)tarFoolUprightEnchantment).Status = (EnchantmentStatus)1;
			}
			await CardPileCmd.AddGeneratedCardToCombat(val, (PileType)2, ((EnchantmentModel)this).Card.Owner, (CardPilePosition)1);
		}
	}
}

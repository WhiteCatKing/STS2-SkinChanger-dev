using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using PengoTarot.Powers;

namespace PengoTarot.Enchantments;

public sealed class PlanetSaturnEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override bool ShouldGlowRed => true;

	public override bool CanEnchantCardType(CardType cardType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		return (int)cardType == 1;
	}

	public override bool CanEnchant(CardModel card)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I4
		if (!((EnchantmentModel)this).CanEnchant(card))
		{
			return false;
		}
		if ((int)card.MultiplayerConstraint == 1)
		{
			return false;
		}
		return true;
	}

	public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (cardSource != ((EnchantmentModel)this).Card || !ValuePropExtensions.IsPoweredAttack(props))
		{
			return;
		}
		int effectiveDamage = result.TotalDamage + result.OverkillDamage;
		if (effectiveDamage <= 0)
		{
			return;
		}
		PlanetSaturnPower power = target.GetPower<PlanetSaturnPower>();
		if (power == null || ((PowerModel)power).Amount < effectiveDamage)
		{
			if (power != null)
			{
				await PowerCmd.Remove((PowerModel)(object)power);
			}
			PlanetSaturnPower planetSaturnPower = (PlanetSaturnPower)(object)((PowerModel)ModelDb.Power<PlanetSaturnPower>()).ToMutable(0);
			await PowerCmd.Apply(choiceContext, (PowerModel)(object)planetSaturnPower, target, (decimal)effectiveDamage, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
		}
	}

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		await PowerCmd.Apply<TickTackPower>(choiceContext, ((EnchantmentModel)this).Card.Owner.Creature, 10m, ((EnchantmentModel)this).Card.Owner.Creature, ((EnchantmentModel)this).Card, false);
	}
}

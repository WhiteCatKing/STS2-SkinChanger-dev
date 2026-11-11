using System;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Data;

public class PlanetDef
{
	public string Id { get; }

	public int Cost { get; }

	public Type CardType { get; }

	public CardType PlanetCardType { get; }

	public EnchantmentModel? Enchantment { get; }

	public int CardsToEnchant { get; }

	public Func<Player, bool>? RequiredCharacter { get; }

	public Func<Player, bool>? AvailabilityCheck { get; }

	public int MinAct { get; }

	public double Weight { get; }

	public PlanetDef(string id, int cost, Type cardType, CardType planetCardType, EnchantmentModel enchantment, int cardsToEnchant = 1, Func<Player, bool>? requiredCharacter = null, Func<Player, bool>? availabilityCheck = null, int minAct = 1, double weight = 1.0)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Id = id;
		Cost = cost;
		CardType = cardType;
		PlanetCardType = planetCardType;
		Enchantment = enchantment;
		CardsToEnchant = cardsToEnchant;
		RequiredCharacter = requiredCharacter;
		AvailabilityCheck = availabilityCheck;
		MinAct = minAct;
		Weight = weight;
	}
}

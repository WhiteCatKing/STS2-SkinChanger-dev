using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using PengoTarot.Cards;
using PengoTarot.Enchantments;

namespace PengoTarot.Data;

public static class PlanetDeck
{
	public static readonly List<PlanetDef> All = new List<PlanetDef>
	{
		new PlanetDef("MERCURY", 0, typeof(PlanetMercury), (CardType)3, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetMercuryEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetMercuryEnchantment>()) >= 1),
		new PlanetDef("VENUS", 0, typeof(PlanetVenus), (CardType)3, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetVenusEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetVenusEnchantment>()) >= 1),
		new PlanetDef("EARTH", 0, typeof(PlanetEarth), (CardType)3, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetEarthEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetEarthEnchantment>()) >= 1),
		new PlanetDef("MARS", 0, typeof(PlanetMars), (CardType)3, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetMarsEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetMarsEnchantment>()) >= 1),
		new PlanetDef("JUPITER", 0, typeof(PlanetJupiter), (CardType)1, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetJupiterEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetJupiterEnchantment>()) >= 1),
		new PlanetDef("SATURN", 0, typeof(PlanetSaturn), (CardType)1, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetSaturnEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetSaturnEnchantment>()) >= 1),
		new PlanetDef("URANUS", 0, typeof(PlanetUranus), (CardType)1, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetUranusEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetUranusEnchantment>()) >= 1),
		new PlanetDef("NEPTUNE", 0, typeof(PlanetNeptune), (CardType)1, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetNeptuneEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetNeptuneEnchantment>()) >= 1),
		new PlanetDef("PLUTO", 0, typeof(PlanetPluto), (CardType)2, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetPlutoEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetPlutoEnchantment>()) >= 1),
		new PlanetDef("X", 0, typeof(PlanetX), (CardType)2, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetXEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetXEnchantment>()) >= 1),
		new PlanetDef("CERES", 0, typeof(PlanetCeres), (CardType)2, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetCeresEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetCeresEnchantment>()) >= 1),
		new PlanetDef("ERIS", 0, typeof(PlanetEris), (CardType)2, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetErisEnchantment>(), 1, null, (Player p) => CountEnchantableCards(p, (EnchantmentModel)(object)ModelDb.Enchantment<PlanetErisEnchantment>()) >= 1)
	};

	private static int CountEnchantableCards(Player player, EnchantmentModel enchantment)
	{
		return player.Piles.First((CardPile p) => (int)p.Type == 6).Cards.Count((CardModel card) => enchantment.CanEnchant(card));
	}

	public static List<PlanetDef> DrawThreeUnique(Player player, Rng rng)
	{
		int currentAct = player.RunState.CurrentActIndex + 1;
		List<PlanetDef> source = All.Where((PlanetDef def) => (def.RequiredCharacter == null || def.RequiredCharacter(player)) && (def.AvailabilityCheck == null || def.AvailabilityCheck(player)) && def.MinAct <= currentAct).ToList();
		List<PlanetDef> pool2 = source.Where((PlanetDef d) => (int)d.PlanetCardType == 3).ToList();
		List<PlanetDef> pool3 = source.Where((PlanetDef d) => (int)d.PlanetCardType == 1).ToList();
		List<PlanetDef> pool4 = source.Where((PlanetDef d) => (int)d.PlanetCardType == 2).ToList();
		List<PlanetDef> list = new List<PlanetDef>();
		PlanetDef planetDef = TryTake(pool2, list);
		if (planetDef != null)
		{
			list.Add(planetDef);
		}
		planetDef = TryTake(pool3, list);
		if (planetDef != null)
		{
			list.Add(planetDef);
		}
		planetDef = TryTake(pool4, list);
		if (planetDef != null)
		{
			list.Add(planetDef);
		}
		return list;
		PlanetDef? TryTake(List<PlanetDef> pool, List<PlanetDef> already)
		{
			List<PlanetDef> list2 = pool.Where((PlanetDef d) => !already.Contains(d)).ToList();
			if (list2.Count == 0)
			{
				return null;
			}
			return rng.NextItem<PlanetDef>((IEnumerable<PlanetDef>)list2);
		}
	}
}

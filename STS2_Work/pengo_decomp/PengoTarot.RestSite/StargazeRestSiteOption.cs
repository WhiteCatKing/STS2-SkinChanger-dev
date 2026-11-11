using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using PengoTarot.Data;
using PengoTarot.Relics;

namespace PengoTarot.RestSite;

public class StargazeRestSiteOption : RestSiteOption
{
	private List<PlanetDef>? _cachedDefs;

	public override string OptionId => "STARGAZE";

	public StargazeRestSiteOption(Player owner)
		: base(owner)
	{
	}

	public override async Task<bool> OnSelect()
	{
		Player player = ((RestSiteOption)this).Owner;
		Rng shops = player.PlayerRng.Shops;
		if (_cachedDefs == null)
		{
			_cachedDefs = PlanetDeck.DrawThreeUnique(player, shops);
		}
		if (_cachedDefs.Count == 0)
		{
			return false;
		}
		List<CardModel> list = new List<CardModel>();
		Dictionary<CardModel, PlanetDef> pendingMap = new Dictionary<CardModel, PlanetDef>();
		foreach (PlanetDef cachedDef in _cachedDefs)
		{
			CardModel val = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(cachedDef.CardType).Invoke(null, null);
			list.Add(val);
			pendingMap[val] = cachedDef;
		}
		CardModel val2 = await CardSelectCmd.FromChooseACardScreen((PlayerChoiceContext)(object)new SilentPlayerChoiceContext(), (IReadOnlyList<CardModel>)list, player, true);
		if (val2 == null)
		{
			return false;
		}
		PlanetDef planetDef = pendingMap[val2];
		EnchantmentModel enchantment = planetDef.Enchantment;
		int cardsToEnchant = planetDef.CardsToEnchant;
		int num = 1;
		CardSelectorPrefs val3 = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, num, cardsToEnchant);
		((CardSelectorPrefs)(ref val3)).set_Cancelable(true);
		((CardSelectorPrefs)(ref val3)).set_RequireManualConfirmation(true);
		CardSelectorPrefs val4 = val3;
		IEnumerable<CardModel> enumerable = await CardSelectCmd.FromDeckForEnchantment(player, enchantment, cardsToEnchant, val4);
		if (enumerable == null || !enumerable.Any())
		{
			return false;
		}
		player.GetRelic<StargazerKit>()?.UseCharge();
		foreach (CardModel item in enumerable)
		{
			CardCmd.Enchant(enchantment.ToMutable(), item, 1m);
			if (LocalContext.IsMe(player))
			{
				CardCmd.Preview(item, 1.2f, (CardPreviewStyle)1);
			}
		}
		_cachedDefs = null;
		return true;
	}

	public override Task DoLocalPostSelectVfx(CancellationToken ct = default(CancellationToken))
	{
		return Task.CompletedTask;
	}

	public override Task DoRemotePostSelectVfx()
	{
		return Task.CompletedTask;
	}
}

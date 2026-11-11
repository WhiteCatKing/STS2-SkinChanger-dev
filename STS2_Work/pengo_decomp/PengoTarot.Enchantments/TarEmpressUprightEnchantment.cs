using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Enchantments;

public sealed class TarEmpressUprightEnchantment : EnchantmentModel
{
	protected override IEnumerable<IHoverTip> ExtraHoverTips => (IEnumerable<IHoverTip>)(object)new IHoverTip[1] { HoverTipFactory.Static((StaticHoverTip)14, (DynamicVar[])(object)new DynamicVar[1]
	{
		new DynamicVar("Times", 1m)
	}) };

	public override bool HasExtraCardText => true;

	protected override IEnumerable<DynamicVar> CanonicalVars => (IEnumerable<DynamicVar>)(object)new DynamicVar[1]
	{
		new DynamicVar("PlayCount", 0m)
	};

	public override int EnchantPlayCount(int originalPlayCount)
	{
		if (((EnchantmentModel)this).DynamicVars["PlayCount"].IntValue >= 2)
		{
			return originalPlayCount + 1;
		}
		return originalPlayCount;
	}

	public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (cardPlay.Card != ((EnchantmentModel)this).Card)
		{
			return Task.CompletedTask;
		}
		int intValue = ((EnchantmentModel)this).DynamicVars["PlayCount"].IntValue;
		((EnchantmentModel)this).DynamicVars["PlayCount"].BaseValue = intValue + 1;
		((EnchantmentModel)this).RecalculateValues();
		return Task.CompletedTask;
	}
}

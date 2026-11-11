using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace PengoTarot.Enchantments;

public sealed class TarSunUprightEnchantment : EnchantmentModel
{
	public override bool HasExtraCardText => true;

	public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
	{
		CardModel card = ((EnchantmentModel)this).Card;
		if (((card != null) ? card.Owner : null) == null)
		{
			return;
		}
		List<PowerModel> powers = ((EnchantmentModel)this).Card.Owner.Creature.Powers.ToList();
		List<PowerModel> list = (from p in powers
			where (int)p.TypeForCurrentAmount == 2
			where (int)p.InstanceType == 0
			where p.Amount != 0
			where !IsBackedByTemporaryDebuff(p, powers)
			select p).ToList();
		if (list.Count == 0)
		{
			return;
		}
		PowerModel val = ((EnchantmentModel)this).Card.Owner.RunState.Rng.CombatTargets.NextItem<PowerModel>((IEnumerable<PowerModel>)list);
		if (val != null)
		{
			int amount = val.Amount;
			int num = amount / 2 - amount;
			if (num != 0)
			{
				await PowerCmd.ModifyAmount(choiceContext, val, (decimal)num, (Creature)null, ((EnchantmentModel)this).Card, false);
			}
		}
	}

	private static bool IsBackedByTemporaryDebuff(PowerModel power, IReadOnlyCollection<PowerModel> powers)
	{
		if (!(power is StrengthPower) && !(power is DexterityPower) && !(power is FocusPower))
		{
			return false;
		}
		return powers.Any(delegate(PowerModel candidate)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Invalid comparison between Unknown and I4
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			ITemporaryPower val = (ITemporaryPower)(object)((candidate is ITemporaryPower) ? candidate : null);
			return val != null && (int)candidate.TypeForCurrentAmount == 2 && (int)candidate.InstanceType == 0 && candidate.Amount > 0 && ((object)val.InternallyAppliedPower).GetType() == ((object)power).GetType();
		});
	}
}

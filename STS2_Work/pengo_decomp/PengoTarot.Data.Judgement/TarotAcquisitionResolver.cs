using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;

namespace PengoTarot.Data.Judgement;

public static class TarotAcquisitionResolver
{
	public static bool IsPortableCandidate(TarotDef def)
	{
		if (def.Enchantment == null && !IsTower(def))
		{
			return IsWheelOfFortune(def);
		}
		return true;
	}

	public static bool IsTower(TarotDef def)
	{
		return def.Id.StartsWith("TOWER_", StringComparison.Ordinal);
	}

	public static bool IsWheelOfFortune(TarotDef def)
	{
		return def.Id.StartsWith("WHEEL_OF_FORTUNE_", StringComparison.Ordinal);
	}

	public static async Task<bool> ResolveSelectedTarot(TarotDef def, Player player, bool isLocalBuyer, Func<Task>? onConfirmed = null)
	{
		if (!JudgementDivination.IsEnabled)
		{
			return await TarotEffectExecutor.ExecuteEffectAndEnchant(def, player, isLocalBuyer, onConfirmed);
		}
		if (!IsPortableCandidate(def))
		{
			return false;
		}
		if (!JudgementTarotInventoryState.TryProcure(player.NetId, def.Id))
		{
			if (isLocalBuyer)
			{
				PortableTarotInventoryUi.PlayFullFeedback();
			}
			return false;
		}
		try
		{
			if (onConfirmed != null)
			{
				await onConfirmed();
			}
			return true;
		}
		catch
		{
			JudgementTarotInventoryState.RollbackLast(player.NetId, def.Id);
			throw;
		}
	}
}

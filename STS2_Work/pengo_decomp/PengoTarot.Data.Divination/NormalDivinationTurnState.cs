using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace PengoTarot.Data.Divination;

public static class NormalDivinationTurnState
{
	public static int CountCardsPlayedThisTurn(Creature owner, CardType type)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (owner.Player == null)
		{
			return 0;
		}
		return CombatManager.Instance.History.CardPlaysStarted.Count((CardPlayStartedEntry entry) => ((CombatHistoryEntry)entry).HappenedThisTurn(owner.CombatState) && ((CombatHistoryEntry)entry).Actor == owner && entry.CardPlay.Card.Type == type);
	}

	public static bool HasPlayedCardThisTurn(Creature owner, CardType type)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CountCardsPlayedThisTurn(owner, type) > 0;
	}
}

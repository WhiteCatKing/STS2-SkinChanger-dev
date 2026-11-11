using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using CznEnemySkin.Localization;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

internal static class BestiaryMovesAdapter
{
	public static void AddEntrance(MonsterModel monster, List<BestiaryMonsterMove> moves)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!Entry.Replacements.TryResolveMonster(((AbstractModel)monster).Id.Entry, out EnemyReplacementProfile profile))
		{
			return;
		}
		EnemyDeathBodyPolicy deathBodyPolicy = profile.Monster.DeathBodyPolicy;
		if ((uint)(deathBodyPolicy - 1) <= 1u)
		{
			moves.RemoveAll((BestiaryMonsterMove move) => move.animId == "die");
		}
		string entranceAnimation = profile.Monster.EntranceAnimation;
		if (entranceAnimation != null && !moves.Any((BestiaryMonsterMove move) => move.animId == entranceAnimation))
		{
			BestiaryMonsterMove val = default(BestiaryMonsterMove);
			val.displayName = EnemyLocalization.GetPreviewAction(profile.Bestiary.PreviewAction);
			val.animId = entranceAnimation;
			BestiaryMonsterMove item = val;
			int num = moves.FindIndex(delegate(BestiaryMonsterMove move)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				string animId = move.animId;
				return (animId == "hurt" || animId == "die") ? true : false;
			});
			moves.Insert((num >= 0) ? num : moves.Count, item);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using PengoTarot.Data.Divination;
using PengoTarot.Data.Judgement;

namespace PengoTarot.Data;

public sealed class TarotReward : Reward
{
	private const int DynamicRewardTypeFloor = 1073741824;

	private static readonly Dictionary<int, string> TarotIdPrefixByFlag = new Dictionary<int, string>
	{
		{ 7, "CHARIOT" },
		{ 8, "STRENGTH" },
		{ 9, "HERMIT" },
		{ 11, "JUSTICE" },
		{ 12, "HANGED_MAN" },
		{ 13, "DEATH" }
	};

	private const string FallbackTarotId = "WHEEL_OF_FORTUNE_UPRIGHT";

	private readonly int _flagIndex;

	public int FlagIndex => _flagIndex;

	protected override RewardType RewardType => GetRewardTypeForFlag(_flagIndex);

	public override int RewardsSetIndex => 9;

	public override bool IsPopulated => true;

	public override LocString Description => new LocString("gameplay_ui", "BAL_CFW_TAROT_REWARD_DESC");

	protected override string? IconPath => TarotMarkerSystem.GetMarkerIconPath(_flagIndex);

	public TarotReward(Player player, int flagIndex)
		: base(player)
	{
		_flagIndex = flagIndex;
	}

	public static RewardType GetRewardTypeForFlag(int flagIndex)
	{
		return (RewardType)(1073741824 + flagIndex);
	}

	public static bool TryGetFlagFromRewardType(RewardType type, out int flagIndex)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected I4, but got Unknown
		int num = type - 1073741824;
		if (TarotIdPrefixByFlag.ContainsKey(num))
		{
			flagIndex = num;
			return true;
		}
		flagIndex = 0;
		return false;
	}

	public override void Populate()
	{
	}

	public override void MarkContentAsSeen()
	{
	}

	protected override async Task<bool> OnSelect()
	{
		Player player = ((Reward)this).Player;
		if (JudgementDivination.IsEnabled && !JudgementTarotInventoryState.HasOpenSlot(player.NetId))
		{
			if (LocalContext.IsMe(player))
			{
				PortableTarotInventoryUi.PlayFullFeedback();
			}
			return false;
		}
		List<TarotDef> defs = CollectDefs(player);
		if (defs.Count == 0)
		{
			return false;
		}
		List<CardModel> cards = new List<CardModel>();
		foreach (TarotDef item2 in defs)
		{
			CardModel item = (CardModel)typeof(ModelDb).GetMethod("Card", Type.EmptyTypes).MakeGenericMethod(item2.CardType).Invoke(null, null);
			cards.Add(item);
		}
		CardModel val = await CardSelectCmd.FromChooseACardScreen((PlayerChoiceContext)(object)new SilentPlayerChoiceContext(), (IReadOnlyList<CardModel>)cards, player, true);
		if (val == null)
		{
			return false;
		}
		return await TarotAcquisitionResolver.ResolveSelectedTarot(defs[cards.IndexOf(val)], player, LocalContext.IsMe(player));
	}

	private List<TarotDef> CollectDefs(Player player)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		List<TarotDef> list = new List<TarotDef>();
		IRunState runState = player.RunState;
		MapCoord? currentMapCoord = runState.CurrentMapCoord;
		Rng val = new Rng(runState.Rng.Seed, $"pengo_divination_reward_{((IPlayerCollection)runState).GetPlayerSlotIndex(player)}_{runState.CurrentActIndex}_{currentMapCoord?.col ?? (-1)}_{currentMapCoord?.row ?? (-1)}_{_flagIndex}");
		if (TarotIdPrefixByFlag.TryGetValue(_flagIndex, out string prefix))
		{
			List<TarotDef> list2 = new List<TarotDef>();
			AddIfAvailable(list2, player, prefix + "_UPRIGHT");
			AddIfAvailable(list2, player, prefix + "_REVERSED");
			TarotDef tarotDef = val.NextItem<TarotDef>((IEnumerable<TarotDef>)list2);
			if (tarotDef != null)
			{
				list.Add(tarotDef);
			}
			List<TarotDef> collection = TarotDeck.DrawUnique(player, val, 3 - list.Count, includeReversed: true, includeCharacterSpecific: true, (TarotDef def) => !def.Id.StartsWith(prefix + "_", StringComparison.Ordinal) && (!JudgementDivination.IsEnabled || TarotAcquisitionResolver.IsPortableCandidate(def)));
			list.AddRange(collection);
		}
		if (list.Count == 0)
		{
			TarotDef tarotDef2 = (JudgementDivination.IsEnabled ? TarotDeck.All.FirstOrDefault((TarotDef d) => TarotAcquisitionResolver.IsPortableCandidate(d) && (d.RequiredCharacter == null || d.RequiredCharacter(player))) : TarotDeck.All.FirstOrDefault((TarotDef d) => d.Id == "WHEEL_OF_FORTUNE_UPRIGHT"));
			if (tarotDef2 != null)
			{
				list.Add(tarotDef2);
			}
		}
		val.Shuffle<TarotDef>((IList<TarotDef>)list);
		return list;
	}

	private static void AddIfAvailable(List<TarotDef> list, Player player, string id)
	{
		TarotDef tarotDef = TarotDeck.All.FirstOrDefault((TarotDef d) => d.Id == id);
		if (tarotDef != null && (JudgementDivination.IsEnabled || tarotDef.AvailabilityCheck == null || tarotDef.AvailabilityCheck(player)))
		{
			list.Add(tarotDef);
		}
	}

	public override SerializableReward ToSerializable()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		return new SerializableReward
		{
			RewardType = GetRewardTypeForFlag(_flagIndex)
		};
	}
}

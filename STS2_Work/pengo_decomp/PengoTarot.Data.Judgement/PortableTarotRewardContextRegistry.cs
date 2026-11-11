using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace PengoTarot.Data.Judgement;

public static class PortableTarotRewardContextRegistry
{
	private static readonly Dictionary<ulong, IReadOnlyList<CardModel>> Contexts = new Dictionary<ulong, IReadOnlyList<CardModel>>();

	private static readonly Dictionary<(ulong PlayerId, PortableTarotUseContext Kind), IReadOnlyList<CardModel>> LogicalContexts = new Dictionary<(ulong, PortableTarotUseContext), IReadOnlyList<CardModel>>();

	private static readonly Dictionary<ulong, MerchantInventory> MerchantContexts = new Dictionary<ulong, MerchantInventory>();

	public static void Register(Node owner, IEnumerable<CardModel> cards)
	{
		Contexts[((GodotObject)owner).GetInstanceId()] = cards.ToArray();
	}

	public static void Unregister(Node owner)
	{
		Contexts.Remove(((GodotObject)owner).GetInstanceId());
	}

	public static IReadOnlyList<CardModel>? Get(Node owner)
	{
		if (!Contexts.TryGetValue(((GodotObject)owner).GetInstanceId(), out IReadOnlyList<CardModel> value))
		{
			return null;
		}
		return value;
	}

	public static void Register(Player player, PortableTarotUseContext kind, IEnumerable<CardModel> cards)
	{
		LogicalContexts[(player.NetId, kind)] = cards.ToArray();
	}

	public static void Unregister(Player player, PortableTarotUseContext kind)
	{
		LogicalContexts.Remove((player.NetId, kind));
	}

	public static IReadOnlyList<CardModel>? Get(Player player, PortableTarotUseContext kind)
	{
		if (kind == PortableTarotUseContext.Merchant && MerchantContexts.TryGetValue(player.NetId, out MerchantInventory value))
		{
			return (from entry in value.CardEntries
				where ((MerchantEntry)entry).IsStocked && entry.CreationResult != null
				select entry.CreationResult.Card).ToArray();
		}
		if (!LogicalContexts.TryGetValue((player.NetId, kind), out IReadOnlyList<CardModel> value2))
		{
			return null;
		}
		return value2;
	}

	public static void RegisterMerchant(Player player, MerchantInventory inventory)
	{
		MerchantContexts[player.NetId] = inventory;
	}

	public static MerchantInventory? GetMerchant(Player player)
	{
		return MerchantContexts.GetValueOrDefault(player.NetId);
	}
}

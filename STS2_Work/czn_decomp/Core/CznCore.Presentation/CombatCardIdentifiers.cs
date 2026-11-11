using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal static class CombatCardIdentifiers
{
	private static readonly ConditionalWeakTable<CardModel, string[]> Cache = new ConditionalWeakTable<CardModel, string[]>();

	public static IReadOnlyList<string> Collect(CardModel? card)
	{
		if (card == null)
		{
			return Array.Empty<string>();
		}
		return Cache.GetValue(card, Create);
	}

	private static string[] Create(CardModel card)
	{
		List<string> list = new List<string>(4);
		AddIfPresent(list, ((AbstractModel)card).Id.Entry);
		AddIfPresent(list, ((object)((AbstractModel)card).Id)?.ToString());
		Type type = ((object)card).GetType();
		AddIfPresent(list, type.Name);
		AddIfPresent(list, type.FullName);
		return list.ToArray();
	}

	private static void AddIfPresent(List<string> identifiers, string? identifier)
	{
		if (!string.IsNullOrWhiteSpace(identifier))
		{
			identifiers.Add(identifier);
		}
	}
}

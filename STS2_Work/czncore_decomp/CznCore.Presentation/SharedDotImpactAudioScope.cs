using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace CznCore.Presentation;

internal static class SharedDotImpactAudioScope
{
	private sealed class Request(Creature target)
	{
		internal readonly Creature Target = target;

		internal bool Claimed;

		internal int Suppressed;
	}

	[HarmonyPatch(typeof(CreatureCmd), "Damage", new Type[]
	{
		typeof(PlayerChoiceContext),
		typeof(IEnumerable<Creature>),
		typeof(decimal),
		typeof(ValueProp),
		typeof(Creature),
		typeof(CardModel),
		typeof(CardPlay)
	})]
	private static class DamageScopePatch
	{
		private static void Prefix(IEnumerable<Creature>? targets, out Creature? __state)
		{
			__state = Active.Value;
			Request value = Pending.Value;
			Active.Value = null;
			if (value != null && !value.Claimed)
			{
				value.Claimed = true;
				if (targets is IReadOnlyCollection<Creature> { Count: 1 } readOnlyCollection && readOnlyCollection.First() == value.target)
				{
					Active.Value = value.target;
				}
			}
		}

		private static void Finalizer(Creature? __state)
		{
			Active.Value = __state;
		}
	}

	[HarmonyPatch(typeof(SfxCmd), "PlayDamage")]
	private static class ImpactPatch
	{
		private static bool Prefix(MonsterModel? monster)
		{
			int num;
			if (monster != null && Active.Value != null && Active.Value.IsMonster)
			{
				num = ((Active.Value.Monster != monster) ? 1 : 0);
				if (num == 0)
				{
					Request value = Pending.Value;
					if (value != null)
					{
						value.Suppressed++;
					}
				}
			}
			else
			{
				num = 1;
			}
			return (byte)num != 0;
		}
	}

	private static readonly AsyncLocal<Request?> Pending = new AsyncLocal<Request>();

	private static readonly AsyncLocal<Creature?> Active = new AsyncLocal<Creature>();

	private static bool _patched;

	private static readonly object PatchLock = new object();

	internal static async Task Run(Creature target, bool replaced, Func<Task> damage)
	{
		if (!replaced)
		{
			await damage();
			return;
		}
		try
		{
			EnsurePatched();
		}
		catch (Exception ex)
		{
			SkinLog.Warn("DOT impact replacement unavailable; native sound retained: " + ex);
			await damage();
			return;
		}
		Request parent = Pending.Value;
		Request request = new Request(target);
		Pending.Value = request;
		try
		{
			await damage();
		}
		finally
		{
			Pending.Value = parent;
			SkinLog.Info($"CZN_DOT_AUDIO claimed={request.Claimed} suppressed={request.Suppressed}");
		}
	}

	private static void EnsurePatched()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		lock (PatchLock)
		{
			if (!_patched)
			{
				Harmony val = new Harmony("czn.core.dot-impact-audio");
				val.CreateClassProcessor(typeof(DamageScopePatch)).Patch();
				val.CreateClassProcessor(typeof(ImpactPatch)).Patch();
				_patched = true;
			}
		}
	}
}

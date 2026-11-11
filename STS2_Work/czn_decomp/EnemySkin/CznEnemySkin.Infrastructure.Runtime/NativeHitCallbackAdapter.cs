using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class NativeHitCallbackAdapter
{
	internal static bool IsReviewed(MonsterModel? monster, string? trigger, IReadOnlyList<Func<Creature, Node2D?>> callbacks)
	{
		if ((((object)monster)?.GetType() == typeof(Wriggler) || ((object)monster)?.GetType() == typeof(PhrogParasite)) && trigger == "Attack")
		{
			if (callbacks.Count == 1 && callbacks[0].GetInvocationList().Length == 1 && callbacks[0].Method.DeclaringType == typeof(NWormyImpactVfx))
			{
				return callbacks[0].Method.Name == "Create";
			}
			return false;
		}
		if (monster is KinPriest && trigger == "AttackGrenade")
		{
			if (callbacks.Count == 1 && callbacks[0].GetInvocationList().Length == 1 && callbacks[0].Method.DeclaringType?.DeclaringType == typeof(KinPriest))
			{
				return new string[2] { "OrbOfFrailtyMove", "OrbOfWeaknessMove" }.Any((string move) => callbacks[0].Method.Name.StartsWith("<" + move + ">b__", StringComparison.Ordinal));
			}
			return false;
		}
		string[] array = ((!(monster is CeremonialBeast) || !(trigger == "Attack")) ? ((((object)monster)?.GetType() == typeof(LivingFog) && trigger == "Attack") ? new string[2] { "BloatMove", "SuperGasBlastMove" } : ((((object)monster)?.GetType() == typeof(MechaKnight) && trigger == "charge") ? new string[1] { "ChargeMove" } : ((((object)monster)?.GetType() == typeof(MechaKnight) && trigger == "Attack") ? new string[1] { "HeavyCleaveMove" } : Array.Empty<string>()))) : new string[2] { "StompMove", "CrushMove" });
		bool flag = ((monster is LivingFog || monster is CeremonialBeast) ? true : false);
		int num = (flag ? 1 : 2);
		if (array.Length != 0 && callbacks.Count == num && callbacks.Select((Func<Creature, Node2D> c) => c.Method).Distinct().Count() == num)
		{
			return array.Any((string move) => callbacks.All((Func<Creature, Node2D> c) => c.GetInvocationList().Length == 1 && c.Method.DeclaringType == ((object)monster).GetType() && c.Target == monster && c.Method.Name.StartsWith("<" + move + ">b__", StringComparison.Ordinal)));
		}
		return false;
	}

	internal static Func<Creature, bool> Wrap(List<Func<Creature, Node2D?>> callbacks, Func<int> hitIndex, Func<Creature, bool> tryPlay)
	{
		int previous = -1;
		Dictionary<Creature, bool> results = new Dictionary<Creature, bool>(ReferenceEqualityComparer.Instance);
		for (int i = 0; i < callbacks.Count; i++)
		{
			Func<Creature, Node2D?> original = callbacks[i];
			callbacks[i] = (Creature target) => (!Play(target)) ? original(target) : null;
		}
		return Play;
		bool Play(Creature target)
		{
			int num = hitIndex();
			if (num != previous)
			{
				results.Clear();
				previous = num;
			}
			if (!results.TryGetValue(target, out var value))
			{
				value = (results[target] = tryPlay(target));
			}
			return value;
		}
	}
}

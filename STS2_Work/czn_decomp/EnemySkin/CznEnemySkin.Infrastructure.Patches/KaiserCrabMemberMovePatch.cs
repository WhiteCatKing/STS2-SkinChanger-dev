using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch]
internal static class KaiserCrabMemberMovePatch
{
	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private (Type, string)[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		MethodBase IEnumerator<MethodBase>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CTargetMethods_003Ed__0(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = new(Type, string)[10]
				{
					(typeof(Crusher), "ThrashMove"),
					(typeof(Crusher), "EnlargingStrikeMove"),
					(typeof(Crusher), "BugStingMove"),
					(typeof(Crusher), "AdaptMove"),
					(typeof(Crusher), "GuardedStrikeMove"),
					(typeof(Rocket), "TargetingReticleMove"),
					(typeof(Rocket), "PrecisionBeamMove"),
					(typeof(Rocket), "ChargeUpMove"),
					(typeof(Rocket), "LaserMove"),
					(typeof(Rocket), "RechargeMove")
				};
				_003C_003E7__wrap2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E7__wrap2++;
				break;
			}
			if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
			{
				(Type, string) tuple = _003C_003E7__wrap1[_003C_003E7__wrap2];
				Type item = tuple.Item1;
				string item2 = tuple.Item2;
				_003C_003E2__current = AccessTools.Method(item, item2, (Type[])null, (Type[])null);
				_003C_003E1__state = 1;
				return true;
			}
			_003C_003E7__wrap1 = null;
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<MethodBase> IEnumerable<MethodBase>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CTargetMethods_003Ed__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__0))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargetMethods_003Ed__0(-2);
	}

	private static void Prefix(MonsterModel __instance, MethodBase __originalMethod, out IDisposable? __state)
	{
		__state = Entry.NativePresentation.Capture(__instance);
		string name = __originalMethod.Name;
		(EnemyAction, string)? tuple;
		if (__instance is Crusher)
		{
			switch (name)
			{
			case "ThrashMove":
				break;
			case "EnlargingStrikeMove":
			case "GuardedStrikeMove":
				goto IL_00e7;
			case "BugStingMove":
				goto IL_00fd;
			case "AdaptMove":
				goto IL_0110;
			default:
				goto IL_0182;
			}
			tuple = (EnemyAction.BasicAttack, "normal_skill4");
		}
		else
		{
			if (!(__instance is Rocket))
			{
				goto IL_0182;
			}
			switch (name)
			{
			case "TargetingReticleMove":
				break;
			case "PrecisionBeamMove":
				goto IL_0136;
			case "ChargeUpMove":
				goto IL_0149;
			case "LaserMove":
				goto IL_015c;
			case "RechargeMove":
				goto IL_016f;
			default:
				goto IL_0182;
			}
			tuple = (EnemyAction.BasicAttack, "normal_skill4");
		}
		goto IL_018d;
		IL_00fd:
		tuple = (EnemyAction.Cast, "normal_skill1_3");
		goto IL_018d;
		IL_0182:
		tuple = null;
		goto IL_018d;
		IL_018d:
		(EnemyAction, string)? tuple2 = tuple;
		if (tuple2.HasValue)
		{
			(EnemyAction, string) valueOrDefault = tuple2.GetValueOrDefault();
			KaiserCrabMemberMoveAdapter.Play(__instance, valueOrDefault.Item1, valueOrDefault.Item2, ResolveStartDelay(__instance, __originalMethod.Name));
		}
		return;
		IL_0136:
		tuple = (EnemyAction.HeavyAttack, "normal_skill1_3");
		goto IL_018d;
		IL_0149:
		tuple = (EnemyAction.Buff, "normal_skill3");
		goto IL_018d;
		IL_015c:
		tuple = (EnemyAction.Cast, "normal_skill1_3");
		goto IL_018d;
		IL_016f:
		tuple = (EnemyAction.Special, "normal_skill2_3");
		goto IL_018d;
		IL_00e7:
		tuple = (EnemyAction.HeavyAttack, "normal_skill1_3");
		goto IL_018d;
		IL_0110:
		tuple = (EnemyAction.Buff, "normal_skill3");
		goto IL_018d;
	}

	private static float ResolveStartDelay(MonsterModel monster, string moveName)
	{
		if (monster is Crusher && moveName == "ThrashMove")
		{
			return 0.6f;
		}
		return 0f;
	}

	private static void Postfix(IDisposable? __state)
	{
		__state?.Dispose();
	}
}

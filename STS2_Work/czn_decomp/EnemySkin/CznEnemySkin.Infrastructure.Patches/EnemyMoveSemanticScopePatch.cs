using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch]
internal static class EnemyMoveSemanticScopePatch
{
	private sealed record MoveSemantic(Type MonsterType, string MethodName, string Trigger, EnemyAction Action);

	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__2 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private MoveSemantic[] _003C_003E7__wrap1;

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
		public _003CTargetMethods_003Ed__2(int _003C_003E1__state)
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
			int num = _003C_003E1__state;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_006d;
			}
			_003C_003E1__state = -1;
			_003C_003E7__wrap1 = Semantics;
			_003C_003E7__wrap2 = 0;
			goto IL_007b;
			IL_006d:
			_003C_003E7__wrap2++;
			goto IL_007b;
			IL_007b:
			if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
			{
				MoveSemantic moveSemantic = _003C_003E7__wrap1[_003C_003E7__wrap2];
				MethodInfo methodInfo = AccessTools.Method(moveSemantic.MonsterType, moveSemantic.MethodName, (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					_003C_003E2__current = methodInfo;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_006d;
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
			return new _003CTargetMethods_003Ed__2(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	private static readonly MoveSemantic[] Semantics = new MoveSemantic[21]
	{
		new MoveSemantic(typeof(CeremonialBeast), "StompMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(CeremonialBeast), "CrushMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(CeremonialBeast), "PlowMove", "Attack", EnemyAction.ChargeEnd),
		new MoveSemantic(typeof(Vantom), "DismemberMove", "Attack", EnemyAction.Cast),
		new MoveSemantic(typeof(Crusher), "EnlargingStrikeMove", "Attack", EnemyAction.HeavyAttack),
		new MoveSemantic(typeof(Crusher), "GuardedStrikeMove", "Attack", EnemyAction.HeavyAttack),
		new MoveSemantic(typeof(Crusher), "BugStingMove", "Attack", EnemyAction.Cast),
		new MoveSemantic(typeof(Rocket), "PrecisionBeamMove", "Attack", EnemyAction.HeavyAttack),
		new MoveSemantic(typeof(Crusher), "ThrashMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(Rocket), "TargetingReticleMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(Rocket), "LaserMove", "Attack", EnemyAction.Cast),
		new MoveSemantic(typeof(LivingShield), "ShieldSlamMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(LivingShield), "SmashMove", "Attack", EnemyAction.HeavyAttack),
		new MoveSemantic(typeof(SoulNexus), "MaelstromMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(SoulNexus), "SoulBurnMove", "Attack", EnemyAction.HeavyAttack),
		new MoveSemantic(typeof(Queen), "OffWithYourHeadMove", "Attack", EnemyAction.HeavyAttack),
		new MoveSemantic(typeof(Queen), "ExecutionMove", "Attack", EnemyAction.Special),
		new MoveSemantic(typeof(OwlMagistrate), "MagistrateScrutinyMove", "Attack", EnemyAction.Special),
		new MoveSemantic(typeof(OwlMagistrate), "PeckAssaultMove", "Attack", EnemyAction.BasicAttack),
		new MoveSemantic(typeof(OwlMagistrate), "JudicialFlightMove", "TakeOff", EnemyAction.Buff),
		new MoveSemantic(typeof(OwlMagistrate), "VerdictMove", "Attack", EnemyAction.HeavyAttack)
	};

	[IteratorStateMachine(typeof(_003CTargetMethods_003Ed__2))]
	private static IEnumerable<MethodBase> TargetMethods()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargetMethods_003Ed__2(-2);
	}

	private static void Prefix(MonsterModel __instance, MethodBase __originalMethod, out IDisposable? __state)
	{
		MoveSemantic moveSemantic = Semantics.FirstOrDefault((MoveSemantic candidate) => candidate.MonsterType == __originalMethod.DeclaringType && candidate.MethodName == __originalMethod.Name);
		__state = ((moveSemantic == null) ? null : Entry.MoveSemantics.Capture(__instance, moveSemantic.Trigger, moveSemantic.Action));
	}

	private static void Postfix(IDisposable? __state)
	{
		__state?.Dispose();
	}
}

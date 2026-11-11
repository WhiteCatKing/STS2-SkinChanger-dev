using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace JoanofArcIroncladHpwear;

[HarmonyPatch]
internal static class EnemyLowAnimationPatch
{
	[CompilerGenerated]
	private sealed class _003CTargets_003Ed__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string[] _003C_003E7__wrap1;

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
		public _003CTargets_003Ed__0(int _003C_003E1__state)
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
				goto IL_0085;
			}
			_003C_003E1__state = -1;
			_003C_003E7__wrap1 = new string[3] { "SetAnimation", "AddAnimation", "AddAnimationTracked" };
			_003C_003E7__wrap2 = 0;
			goto IL_0093;
			IL_0085:
			_003C_003E7__wrap2++;
			goto IL_0093;
			IL_0093:
			if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
			{
				string text = _003C_003E7__wrap1[_003C_003E7__wrap2];
				MethodInfo methodInfo = AccessTools.Method(typeof(MegaAnimationState), text, (Type[])null, (Type[])null);
				if (methodInfo != null)
				{
					_003C_003E2__current = methodInfo;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0085;
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
			return new _003CTargets_003Ed__0(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<MethodBase>)this).GetEnumerator();
		}
	}

	[IteratorStateMachine(typeof(_003CTargets_003Ed__0))]
	[HarmonyTargetMethods]
	private static IEnumerable<MethodBase> Targets()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTargets_003Ed__0(-2);
	}

	[HarmonyPrefix]
	private static void Prefix(MegaAnimationState __instance, ref string __0)
	{
		if (!string.Equals(__0, "low_health_loop", StringComparison.OrdinalIgnoreCase))
		{
			return;
		}
		try
		{
			if (EnemyAttackGate.BlockAnimation(__instance))
			{
				__0 = "idle_loop";
			}
		}
		catch (Exception ex)
		{
			ModEntry.TryInfo("enemy gate animation: " + ex.Message);
		}
	}
}

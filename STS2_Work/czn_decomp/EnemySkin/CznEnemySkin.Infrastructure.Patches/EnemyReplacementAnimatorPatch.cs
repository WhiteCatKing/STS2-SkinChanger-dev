using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Models;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch]
internal static class EnemyReplacementAnimatorPatch
{
	[CompilerGenerated]
	private sealed class _003CTargetMethods_003Ed__0 : IEnumerable<MethodBase>, IEnumerable, IEnumerator<MethodBase>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private MethodBase _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Type> _003C_003E7__wrap1;

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
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = EnemyAnimatorRoutes.PatchTargets.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					Type current = _003C_003E7__wrap1.Current;
					MethodBase methodBase = AccessTools.DeclaredMethod(current, "GenerateAnimator", new Type[1] { typeof(MegaSprite) }, (Type[])null);
					if (methodBase == null)
					{
						Entry.Log.Warn("STS2 no longer declares " + current.Name + ".GenerateAnimator(MegaSprite); its replacement animator routes stay inactive.", 1);
						continue;
					}
					_003C_003E2__current = methodBase;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
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

	private static bool Prefix(MonsterModel __instance, MegaSprite controller, MethodBase __originalMethod, ref CreatureAnimator __result)
	{
		foreach (EnemyAnimatorRoute item in EnemyAnimatorRoutes.For(__originalMethod.DeclaringType))
		{
			if (!item.MonsterType.IsInstanceOfType(__instance) || !Entry.Replacements.TryResolveMonster(item.MonsterIdOverride ?? ((AbstractModel)__instance).Id.Entry, out EnemyReplacementProfile profile) || (item.RequiredProfileId != null && profile.Id != item.RequiredProfileId))
			{
				continue;
			}
			try
			{
				CreatureAnimator val = item.Create(__instance, controller, profile);
				if (val == null)
				{
					continue;
				}
				__result = val;
				return false;
			}
			catch (Exception ex)
			{
				Entry.Log.Warn($"{item.FailureDescription} failed; using STS2 animator [profile={profile.Id}, error={ex.Message}].", 1);
				return true;
			}
		}
		return true;
	}
}

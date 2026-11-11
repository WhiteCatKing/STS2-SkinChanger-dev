using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed class CombatPresentationProfileTables
{
	[CompilerGenerated]
	private sealed class _003CEnumerateSfxCues_003Ed__23 : IEnumerable<CombatPresentationSfxCue>, IEnumerable, IEnumerator<CombatPresentationSfxCue>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CombatPresentationSfxCue _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private bool includeRoutePlanSfx;

		public bool _003C_003E3__includeRoutePlanSfx;

		public CombatPresentationProfileTables _003C_003E4__this;

		private IEnumerator<CombatPresentationRoutePlan> _003C_003E7__wrap1;

		private IEnumerator<CombatPresentationSfxCue> _003C_003E7__wrap2;

		private IEnumerator<KeyValuePair<string, IReadOnlyList<CombatPresentationSfxCue>>> _003C_003E7__wrap3;

		CombatPresentationSfxCue IEnumerator<CombatPresentationSfxCue>.Current
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
		public _003CEnumerateSfxCues_003Ed__23(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			switch (num)
			{
			case -4:
			case -3:
			case 1:
				try
				{
					if (num == -4 || num == 1)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -6:
			case -5:
			case 2:
				try
				{
					if (num == -6 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally4();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally3();
				}
				break;
			}
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				CombatPresentationProfileTables combatPresentationProfileTables = _003C_003E4__this;
				CombatPresentationRoutePlan current;
				string arg;
				IReadOnlyList<CombatPresentationSfxCue> readOnlyList2;
				switch (num)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (includeRoutePlanSfx)
					{
						_003C_003E7__wrap1 = combatPresentationProfileTables._routePlans.Values.GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_00cd;
					}
					goto IL_00ea;
				case 1:
					_003C_003E1__state = -4;
					goto IL_00b3;
				case 2:
					{
						_003C_003E1__state = -6;
						goto IL_017e;
					}
					IL_00ea:
					_003C_003E7__wrap3 = combatPresentationProfileTables._sfxCues.GetEnumerator();
					_003C_003E1__state = -5;
					goto IL_0198;
					IL_00cd:
					while (_003C_003E7__wrap1.MoveNext())
					{
						current = _003C_003E7__wrap1.Current;
						if (current.SfxCues == null)
						{
							continue;
						}
						goto IL_006d;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					goto IL_00ea;
					IL_0198:
					do
					{
						if (!_003C_003E7__wrap3.MoveNext())
						{
							_003C_003Em__Finally3();
							_003C_003E7__wrap3 = null;
							return false;
						}
						(arg, readOnlyList2) = (KeyValuePair<string, IReadOnlyList<CombatPresentationSfxCue>>)(ref _003C_003E7__wrap3.Current);
					}
					while (combatPresentationProfileTables._shouldExposeSfxKey != null && !combatPresentationProfileTables._shouldExposeSfxKey(arg));
					_003C_003E7__wrap2 = readOnlyList2.GetEnumerator();
					_003C_003E1__state = -6;
					goto IL_017e;
					IL_006d:
					_003C_003E7__wrap2 = current.SfxCues.GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00b3;
					IL_00b3:
					if (_003C_003E7__wrap2.MoveNext())
					{
						CombatPresentationSfxCue current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 1;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00cd;
					IL_017e:
					if (_003C_003E7__wrap2.MoveNext())
					{
						CombatPresentationSfxCue current3 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current3;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally4();
					_003C_003E7__wrap2 = null;
					goto IL_0198;
				}
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

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		private void _003C_003Em__Finally4()
		{
			_003C_003E1__state = -5;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<CombatPresentationSfxCue> IEnumerable<CombatPresentationSfxCue>.GetEnumerator()
		{
			_003CEnumerateSfxCues_003Ed__23 _003CEnumerateSfxCues_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateSfxCues_003Ed__ = this;
			}
			else
			{
				_003CEnumerateSfxCues_003Ed__ = new _003CEnumerateSfxCues_003Ed__23(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CEnumerateSfxCues_003Ed__.includeRoutePlanSfx = _003C_003E3__includeRoutePlanSfx;
			return _003CEnumerateSfxCues_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CombatPresentationSfxCue>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateTransientEffectCues_003Ed__22 : IEnumerable<CombatPresentationEffectCue>, IEnumerable, IEnumerator<CombatPresentationEffectCue>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CombatPresentationEffectCue _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public CombatPresentationProfileTables _003C_003E4__this;

		private IEnumerator<KeyValuePair<string, IReadOnlyList<CombatPresentationEffectCue>>> _003C_003E7__wrap1;

		private IEnumerator<CombatPresentationEffectCue> _003C_003E7__wrap2;

		CombatPresentationEffectCue IEnumerator<CombatPresentationEffectCue>.Current
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
		public _003CEnumerateTransientEffectCues_003Ed__22(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 1)
			{
				try
				{
					if (num == -4 || num == 1)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				CombatPresentationProfileTables combatPresentationProfileTables = _003C_003E4__this;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -4;
					goto IL_00b8;
				}
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = combatPresentationProfileTables._transientEffects.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_00d2;
				IL_00b8:
				if (_003C_003E7__wrap2.MoveNext())
				{
					CombatPresentationEffectCue current = _003C_003E7__wrap2.Current;
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally2();
				_003C_003E7__wrap2 = null;
				goto IL_00d2;
				IL_00d2:
				string arg;
				IReadOnlyList<CombatPresentationEffectCue> readOnlyList2;
				do
				{
					if (!_003C_003E7__wrap1.MoveNext())
					{
						_003C_003Em__Finally1();
						_003C_003E7__wrap1 = null;
						return false;
					}
					(arg, readOnlyList2) = (KeyValuePair<string, IReadOnlyList<CombatPresentationEffectCue>>)(ref _003C_003E7__wrap1.Current);
				}
				while (combatPresentationProfileTables._shouldExposeTransientEffectKey != null && !combatPresentationProfileTables._shouldExposeTransientEffectKey(arg));
				_003C_003E7__wrap2 = readOnlyList2.GetEnumerator();
				_003C_003E1__state = -4;
				goto IL_00b8;
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

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<CombatPresentationEffectCue> IEnumerable<CombatPresentationEffectCue>.GetEnumerator()
		{
			_003CEnumerateTransientEffectCues_003Ed__22 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CEnumerateTransientEffectCues_003Ed__22(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CombatPresentationEffectCue>)this).GetEnumerator();
		}
	}

	private readonly IReadOnlyDictionary<string, CombatPresentationState> _states;

	private readonly IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationRoutePlan> _routePlans;

	private readonly IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan> _attackPlans;

	private readonly IReadOnlyDictionary<string, CombatPresentationAnimationTiming> _bodyAnimationTimings;

	private readonly IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>> _persistentEffectBindings;

	private readonly IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationEffectCue>> _transientEffects;

	private readonly IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationSfxCue>> _sfxCues;

	private readonly Func<string, bool>? _shouldExposeTransientEffectKey;

	private readonly Func<string, bool>? _shouldExposeSfxKey;

	private static readonly IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan> EmptyAttackPlans = new Dictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan>();

	private static readonly IReadOnlyDictionary<string, CombatPresentationAnimationTiming> EmptyBodyAnimationTimings = new Dictionary<string, CombatPresentationAnimationTiming>(StringComparer.Ordinal);

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>> EmptyPersistentEffectBindings = new Dictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>>(StringComparer.Ordinal);

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationEffectCue>> EmptyTransientEffects = new Dictionary<string, IReadOnlyList<CombatPresentationEffectCue>>(StringComparer.Ordinal);

	private static readonly IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationSfxCue>> EmptySfxCues = new Dictionary<string, IReadOnlyList<CombatPresentationSfxCue>>(StringComparer.Ordinal);

	public CombatPresentationProfileTables(IReadOnlyDictionary<string, CombatPresentationState> states, IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationRoutePlan> routePlans, IReadOnlyDictionary<CombatPresentationCardRoute, CombatPresentationAttackPlan>? attackPlans = null, IReadOnlyDictionary<string, CombatPresentationAnimationTiming>? bodyAnimationTimings = null, IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationPersistentEffectBinding>>? persistentEffectBindings = null, IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationEffectCue>>? transientEffects = null, IReadOnlyDictionary<string, IReadOnlyList<CombatPresentationSfxCue>>? sfxCues = null, Func<string, bool>? shouldExposeTransientEffectKey = null, Func<string, bool>? shouldExposeSfxKey = null)
	{
		_states = states ?? throw new ArgumentNullException("states");
		_routePlans = routePlans ?? throw new ArgumentNullException("routePlans");
		_attackPlans = attackPlans ?? EmptyAttackPlans;
		_bodyAnimationTimings = bodyAnimationTimings ?? EmptyBodyAnimationTimings;
		_persistentEffectBindings = persistentEffectBindings ?? EmptyPersistentEffectBindings;
		_transientEffects = transientEffects ?? EmptyTransientEffects;
		_sfxCues = sfxCues ?? EmptySfxCues;
		_shouldExposeTransientEffectKey = shouldExposeTransientEffectKey;
		_shouldExposeSfxKey = shouldExposeSfxKey;
	}

	public bool TryGetState(string stateId, out CombatPresentationState state)
	{
		return _states.TryGetValue(stateId, out state);
	}

	public bool TryGetRoutePlan(CombatPresentationCardRoute route, out CombatPresentationRoutePlan plan)
	{
		return _routePlans.TryGetValue(route, out plan);
	}

	public bool TryGetAttackPlan(CombatPresentationCardRoute route, out CombatPresentationAttackPlan plan)
	{
		return _attackPlans.TryGetValue(route, out plan);
	}

	public bool TryGetBodyAnimationTiming(string animationName, out CombatPresentationAnimationTiming timing)
	{
		return _bodyAnimationTimings.TryGetValue(animationName, out timing);
	}

	public bool TryGetPersistentEffectBindings(string animationName, out IReadOnlyList<CombatPresentationPersistentEffectBinding> effects)
	{
		return _persistentEffectBindings.TryGetValue(animationName, out effects);
	}

	public bool TryGetTransientEffects(string animationName, out IReadOnlyList<CombatPresentationEffectCue> effects)
	{
		return _transientEffects.TryGetValue(animationName, out effects);
	}

	public bool TryGetSfxCues(string animationName, out IReadOnlyList<CombatPresentationSfxCue> sfxCues)
	{
		return _sfxCues.TryGetValue(animationName, out sfxCues);
	}

	[IteratorStateMachine(typeof(_003CEnumerateTransientEffectCues_003Ed__22))]
	public IEnumerable<CombatPresentationEffectCue> EnumerateTransientEffectCues()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateTransientEffectCues_003Ed__22(-2)
		{
			_003C_003E4__this = this
		};
	}

	[IteratorStateMachine(typeof(_003CEnumerateSfxCues_003Ed__23))]
	public IEnumerable<CombatPresentationSfxCue> EnumerateSfxCues(bool includeRoutePlanSfx = true)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateSfxCues_003Ed__23(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__includeRoutePlanSfx = includeRoutePlanSfx
		};
	}
}

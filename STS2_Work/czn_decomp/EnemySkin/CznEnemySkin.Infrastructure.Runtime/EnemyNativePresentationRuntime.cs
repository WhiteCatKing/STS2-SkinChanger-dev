using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class EnemyNativePresentationRuntime
{
	private sealed record ActivePresentation(MonsterModel Monster, EnemyReplacementProfile Profile);

	private sealed class CallerContextRestore(AsyncLocal<ActivePresentation?> slot, ActivePresentation? previous) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				slot.Value = previous;
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CLiveCreatureNodes_003Ed__16 : IEnumerable<NCreature>, IEnumerable, IEnumerator<NCreature>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private NCreature _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<NCreature> _003C_003E7__wrap1;

		NCreature IEnumerator<NCreature>.Current
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
		public _003CLiveCreatureNodes_003Ed__16(int _003C_003E1__state)
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
				{
					_003C_003E1__state = -1;
					NCombatRoom instance = NCombatRoom.Instance;
					if (instance == null || !GodotObject.IsInstanceValid((GodotObject)(object)instance))
					{
						return false;
					}
					_003C_003E7__wrap1 = instance.CreatureNodes.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					NCreature current = _003C_003E7__wrap1.Current;
					if (GodotObject.IsInstanceValid((GodotObject)(object)current))
					{
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
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
		IEnumerator<NCreature> IEnumerable<NCreature>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CLiveCreatureNodes_003Ed__16(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<NCreature>)this).GetEnumerator();
		}
	}

	private readonly AsyncLocal<ActivePresentation?> _activePresentation = new AsyncLocal<ActivePresentation>();

	private readonly HashSet<string> _suppressibleAudio;

	private readonly HashSet<string> _suppressibleVfxPaths;

	private readonly HashSet<EnemyNativeVfxFeature> _suppressibleVfxFeatures;

	public EnemyNativePresentationRuntime(IEnumerable<EnemyReplacementProfile> profiles)
	{
		EnemyNativePresentationPolicy[] source = profiles.Select((EnemyReplacementProfile profile) => profile.Monster.NativePresentation).ToArray();
		_suppressibleAudio = source.SelectMany((EnemyNativePresentationPolicy policy) => policy.Audio.Keys).ToHashSet<string>(StringComparer.Ordinal);
		_suppressibleVfxPaths = source.SelectMany((EnemyNativePresentationPolicy policy) => policy.VfxPaths.Keys).ToHashSet<string>(StringComparer.Ordinal);
		_suppressibleVfxFeatures = source.SelectMany((EnemyNativePresentationPolicy policy) => policy.VfxFeatures.Keys).ToHashSet();
	}

	public IDisposable? Capture(MonsterModel monster)
	{
		if (!TryResolveAppliedProfile(monster, out EnemyReplacementProfile profile))
		{
			return null;
		}
		ActivePresentation value = _activePresentation.Value;
		_activePresentation.Value = new ActivePresentation(monster, profile);
		return new CallerContextRestore(_activePresentation, value);
	}

	public bool ShouldSuppressAudio(string resourcePath)
	{
		if (!_suppressibleAudio.Contains(resourcePath))
		{
			return false;
		}
		if (ShouldSuppressActiveAudio(resourcePath))
		{
			return true;
		}
		if (TryFindAppliedProfile((EnemyNativePresentationPolicy policy) => policy.Audio.TryGetValue(resourcePath, out var _), out EnemyReplacementProfile profile) && profile.Monster.NativePresentation.Audio.TryGetValue(resourcePath, out var value2))
		{
			return Entry.ActionPresentation.CanPlayAudio(profile, value2);
		}
		return false;
	}

	public bool ShouldSuppressActiveAudio(string resourcePath)
	{
		if (_suppressibleAudio.Contains(resourcePath) && TryGetCurrentProfile(out EnemyReplacementProfile profile) && TryGetReplacementAction(profile.Monster.NativePresentation.Audio, resourcePath, out var action))
		{
			return Entry.ActionPresentation.CanPlayAudio(profile, action);
		}
		return false;
	}

	public bool ShouldSuppressDamageSfx(MonsterModel? monster)
	{
		if (monster != null && TryResolveAppliedProfile(monster, out EnemyReplacementProfile profile))
		{
			EnemyAction? damageSfxReplacementAction = profile.Monster.NativePresentation.DamageSfxReplacementAction;
			if (damageSfxReplacementAction.HasValue)
			{
				EnemyAction valueOrDefault = damageSfxReplacementAction.GetValueOrDefault();
				return Entry.ActionPresentation.CanPlayAudio(profile, valueOrDefault);
			}
		}
		return false;
	}

	public bool ShouldSuppressVfxPath(string resourcePath)
	{
		if (_suppressibleVfxPaths.Contains(resourcePath) && TryGetCurrentProfile(out EnemyReplacementProfile profile) && TryGetReplacementAction(profile.Monster.NativePresentation.VfxPaths, resourcePath, out var action))
		{
			return Entry.ActionPresentation.CanPlayEffects(profile, action);
		}
		return false;
	}

	public bool ShouldSuppressVfxFeature(EnemyNativeVfxFeature feature)
	{
		if (_suppressibleVfxFeatures.Contains(feature) && TryGetCurrentProfile(out EnemyReplacementProfile profile) && profile.Monster.NativePresentation.VfxFeatures.TryGetValue(feature, out var value))
		{
			return Entry.ActionPresentation.CanPlayEffects(profile, value);
		}
		return false;
	}

	private bool TryGetCurrentProfile(out EnemyReplacementProfile profile)
	{
		ActivePresentation value = _activePresentation.Value;
		if (value != null && TryResolveAppliedProfile(value.Monster, out profile) && profile.Id == value.Profile.Id)
		{
			return true;
		}
		foreach (NCreature item in LiveCreatureNodes())
		{
			Creature entity = item.Entity;
			MonsterModel obj = ((entity != null) ? entity.Monster : null);
			if (obj != null && obj.IsPerformingMove && EnemyVisualHostAdapter.TryResolveAppliedProfile(item, out profile))
			{
				return true;
			}
		}
		profile = null;
		return false;
	}

	private static bool TryResolveAppliedProfile(MonsterModel monster, out EnemyReplacementProfile profile)
	{
		profile = null;
		NCreature creatureNode;
		try
		{
			creatureNode = monster.Creature.GetCreatureNode();
		}
		catch
		{
			return false;
		}
		if (creatureNode != null)
		{
			return EnemyVisualHostAdapter.TryResolveAppliedProfile(creatureNode, out profile);
		}
		return false;
	}

	private static bool TryFindAppliedProfile(Func<EnemyNativePresentationPolicy, bool> matches, out EnemyReplacementProfile profile)
	{
		profile = null;
		foreach (NCreature item in LiveCreatureNodes())
		{
			if (EnemyVisualHostAdapter.TryResolveAppliedProfile(item, out profile) && matches(profile.Monster.NativePresentation))
			{
				return true;
			}
		}
		profile = null;
		return false;
	}

	[IteratorStateMachine(typeof(_003CLiveCreatureNodes_003Ed__16))]
	private static IEnumerable<NCreature> LiveCreatureNodes()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLiveCreatureNodes_003Ed__16(-2);
	}

	private static bool TryGetReplacementAction<TKey>(IReadOnlyDictionary<TKey, EnemyAction> values, TKey key, out EnemyAction action) where TKey : notnull
	{
		return values.TryGetValue(key, out action);
	}
}

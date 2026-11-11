using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyPresentationAssetPaths
{
	[CompilerGenerated]
	private sealed class _003CForProfile_003Ed__1 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private EnemyReplacementProfile profile;

		public EnemyReplacementProfile _003C_003E3__profile;

		private bool actionEffectsEnabled;

		public bool _003C_003E3__actionEffectsEnabled;

		private bool _003CneedsNeutralMultiply_003E5__2;

		private IEnumerator<EnemySecondaryBodyProfile> _003C_003E7__wrap2;

		private IEnumerator<EnemyActionPresentation> _003C_003E7__wrap3;

		private EnemyActionPresentation _003Cpresentation_003E5__5;

		private IEnumerator<EnemyEffectCue> _003C_003E7__wrap5;

		private EnemyEffectCue _003Ccue_003E5__7;

		private IEnumerator<EnemyAudioCue> _003C_003E7__wrap7;

		private IEnumerator<EnemyAudioLayerCue> _003C_003E7__wrap8;

		string IEnumerator<string>.Current
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
		public _003CForProfile_003Ed__1(int _003C_003E1__state)
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
			case -3:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -7:
			case -6:
			case -5:
			case -4:
			case 3:
			case 4:
			case 5:
				try
				{
					switch (num)
					{
					case -5:
					case 3:
						try
						{
						}
						finally
						{
							_003C_003Em__Finally3();
						}
						break;
					case -6:
					case 4:
						try
						{
						}
						finally
						{
							_003C_003Em__Finally4();
						}
						break;
					case -7:
					case 5:
						try
						{
						}
						finally
						{
							_003C_003Em__Finally5();
						}
						break;
					}
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			}
			_003C_003E7__wrap2 = null;
			_003C_003E7__wrap3 = null;
			_003Cpresentation_003E5__5 = null;
			_003C_003E7__wrap5 = null;
			_003Ccue_003E5__7 = null;
			_003C_003E7__wrap7 = null;
			_003C_003E7__wrap8 = null;
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
					_003C_003E2__current = profile.Monster.SkeletonDataPath;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap2 = profile.Monster.SecondaryBodies.GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00b5;
				case 2:
					_003C_003E1__state = -3;
					goto IL_00b5;
				case 3:
					_003C_003E1__state = -5;
					_003CneedsNeutralMultiply_003E5__2 |= _003Ccue_003E5__7.MaterialPolicy == EnemyEffectMaterialPolicy.StraightAlphaNeutralMultiply;
					_003Ccue_003E5__7 = null;
					goto IL_01af;
				case 4:
					_003C_003E1__state = -6;
					goto IL_0219;
				case 5:
					_003C_003E1__state = -7;
					goto IL_028c;
				case 6:
					{
						_003C_003E1__state = -1;
						break;
					}
					IL_00b5:
					if (_003C_003E7__wrap2.MoveNext())
					{
						EnemySecondaryBodyProfile current = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current.SkeletonDataPath;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap2 = null;
					_003CneedsNeutralMultiply_003E5__2 = false;
					_003C_003E7__wrap3 = profile.Monster.ActionPresentations.Values.GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_02ad;
					IL_02ad:
					if (_003C_003E7__wrap3.MoveNext())
					{
						_003Cpresentation_003E5__5 = _003C_003E7__wrap3.Current;
						IReadOnlyList<EnemyEffectCue> readOnlyList2;
						if (!EnemyActionEffectPolicy.ShouldPlay(_003Cpresentation_003E5__5.Action, actionEffectsEnabled))
						{
							IReadOnlyList<EnemyEffectCue> readOnlyList = Array.Empty<EnemyEffectCue>();
							readOnlyList2 = readOnlyList;
						}
						else
						{
							readOnlyList2 = _003Cpresentation_003E5__5.Effects;
						}
						_003C_003E7__wrap5 = readOnlyList2.GetEnumerator();
						_003C_003E1__state = -5;
						goto IL_01af;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					if (_003CneedsNeutralMultiply_003E5__2 || profile.Monster.ApplyNeutralMultiplyToBody)
					{
						_003C_003E2__current = "res://CznEnemySkin/materials/straight_alpha_neutral_multiply.tres";
						_003C_003E1__state = 6;
						return true;
					}
					break;
					IL_0219:
					if (_003C_003E7__wrap7.MoveNext())
					{
						EnemyAudioCue current2 = _003C_003E7__wrap7.Current;
						_003C_003E2__current = current2.ResourcePath;
						_003C_003E1__state = 4;
						return true;
					}
					_003C_003Em__Finally4();
					_003C_003E7__wrap7 = null;
					_003C_003E7__wrap8 = (_003Cpresentation_003E5__5.AudioLayers ?? Array.Empty<EnemyAudioLayerCue>()).GetEnumerator();
					_003C_003E1__state = -7;
					goto IL_028c;
					IL_028c:
					if (_003C_003E7__wrap8.MoveNext())
					{
						EnemyAudioLayerCue current3 = _003C_003E7__wrap8.Current;
						_003C_003E2__current = current3.ResourcePath;
						_003C_003E1__state = 5;
						return true;
					}
					_003C_003Em__Finally5();
					_003C_003E7__wrap8 = null;
					_003Cpresentation_003E5__5 = null;
					goto IL_02ad;
					IL_01af:
					if (_003C_003E7__wrap5.MoveNext())
					{
						_003Ccue_003E5__7 = _003C_003E7__wrap5.Current;
						_003C_003E2__current = _003Ccue_003E5__7.ScenePath;
						_003C_003E1__state = 3;
						return true;
					}
					_003C_003Em__Finally3();
					_003C_003E7__wrap5 = null;
					_003C_003E7__wrap7 = _003Cpresentation_003E5__5.AudioChoices.GetEnumerator();
					_003C_003E1__state = -6;
					goto IL_0219;
				}
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
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		private void _003C_003Em__Finally3()
		{
			_003C_003E1__state = -4;
			if (_003C_003E7__wrap5 != null)
			{
				_003C_003E7__wrap5.Dispose();
			}
		}

		private void _003C_003Em__Finally4()
		{
			_003C_003E1__state = -4;
			if (_003C_003E7__wrap7 != null)
			{
				_003C_003E7__wrap7.Dispose();
			}
		}

		private void _003C_003Em__Finally5()
		{
			_003C_003E1__state = -4;
			if (_003C_003E7__wrap8 != null)
			{
				_003C_003E7__wrap8.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CForProfile_003Ed__1 _003CForProfile_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CForProfile_003Ed__ = this;
			}
			else
			{
				_003CForProfile_003Ed__ = new _003CForProfile_003Ed__1(0);
			}
			_003CForProfile_003Ed__.profile = _003C_003E3__profile;
			_003CForProfile_003Ed__.actionEffectsEnabled = _003C_003E3__actionEffectsEnabled;
			return _003CForProfile_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	internal const string EffectScenePrefix = "res://CznEnemySkin/scenes/";

	[IteratorStateMachine(typeof(_003CForProfile_003Ed__1))]
	public static IEnumerable<string> ForProfile(EnemyReplacementProfile profile, bool actionEffectsEnabled = true)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CForProfile_003Ed__1(-2)
		{
			_003C_003E3__profile = profile,
			_003C_003E3__actionEffectsEnabled = actionEffectsEnabled
		};
	}

	public static bool IsEffectScene(string path)
	{
		if (path.EndsWith(".tscn", StringComparison.Ordinal))
		{
			return path.StartsWith("res://CznEnemySkin/scenes/", StringComparison.Ordinal);
		}
		return false;
	}
}

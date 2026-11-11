using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace STS2SkinChanger.Core;

internal static class PresentationNodeOwnership
{
	[CompilerGenerated]
	private sealed class _003CWalk_003Ed__1<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Func<T, string> nativeClass;

		public Func<T, string> _003C_003E3__nativeClass;

		private T root;

		public T _003C_003E3__root;

		private Func<T, IEnumerable<T>> children;

		public Func<T, IEnumerable<T>> _003C_003E3__children;

		private IEnumerator<T> _003C_003E7__wrap1;

		private IEnumerator<T> _003C_003E7__wrap2;

		T IEnumerator<T>.Current
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
		public _003CWalk_003Ed__1(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
			{
				try
				{
					if (num == -4 || num == 2)
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
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					if (IsRendererOwned(nativeClass(root)))
					{
						return false;
					}
					_003C_003E2__current = root;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = children(root).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_0100;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00e6;
					}
					IL_0100:
					if (_003C_003E7__wrap1.MoveNext())
					{
						T current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = Walk(current, nativeClass, children).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00e6;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_00e6:
					if (_003C_003E7__wrap2.MoveNext())
					{
						T current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_0100;
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			_003CWalk_003Ed__1<T> _003CWalk_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CWalk_003Ed__ = this;
			}
			else
			{
				_003CWalk_003Ed__ = new _003CWalk_003Ed__1<T>(0);
			}
			_003CWalk_003Ed__.root = _003C_003E3__root;
			_003CWalk_003Ed__.nativeClass = _003C_003E3__nativeClass;
			_003CWalk_003Ed__.children = _003C_003E3__children;
			return _003CWalk_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	internal static bool IsRendererOwned(string nativeClass)
	{
		return nativeClass == "SpineMesh2D";
	}

	[IteratorStateMachine(typeof(_003CWalk_003Ed__1<>))]
	internal static IEnumerable<T> Walk<T>(T root, Func<T, string> nativeClass, Func<T, IEnumerable<T>> children)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CWalk_003Ed__1<T>(-2)
		{
			_003C_003E3__root = root,
			_003C_003E3__nativeClass = nativeClass,
			_003C_003E3__children = children
		};
	}
}

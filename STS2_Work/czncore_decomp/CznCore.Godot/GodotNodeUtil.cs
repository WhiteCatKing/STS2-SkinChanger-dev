using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Godot;

namespace CznCore.Godot;

public static class GodotNodeUtil
{
	[CompilerGenerated]
	private sealed class _003CTraverseDepthFirst_003Ed__2 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private Stack<Node> _003Cstack_003E5__2;

		private Node _003Cnode_003E5__3;

		Node IEnumerator<Node>.Current
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
		public _003CTraverseDepthFirst_003Ed__2(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cstack_003E5__2 = null;
			_003Cnode_003E5__3 = null;
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
				_003Cstack_003E5__2 = new Stack<Node>();
				_003Cstack_003E5__2.Push(root);
				break;
			case 1:
				_003C_003E1__state = -1;
				foreach (Node child in _003Cnode_003E5__3.GetChildren(false))
				{
					_003Cstack_003E5__2.Push(child);
				}
				_003Cnode_003E5__3 = null;
				break;
			}
			if (_003Cstack_003E5__2.Count > 0)
			{
				_003Cnode_003E5__3 = _003Cstack_003E5__2.Pop();
				_003C_003E2__current = _003Cnode_003E5__3;
				_003C_003E1__state = 1;
				return true;
			}
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
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			_003CTraverseDepthFirst_003Ed__2 _003CTraverseDepthFirst_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CTraverseDepthFirst_003Ed__ = this;
			}
			else
			{
				_003CTraverseDepthFirst_003Ed__ = new _003CTraverseDepthFirst_003Ed__2(0);
			}
			_003CTraverseDepthFirst_003Ed__.root = _003C_003E3__root;
			return _003CTraverseDepthFirst_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	public static bool IsAlive([NotNullWhen(true)] GodotObject? obj)
	{
		if (obj != null)
		{
			return GodotObject.IsInstanceValid(obj);
		}
		return false;
	}

	public static bool IsClass([NotNullWhen(true)] GodotObject? obj, string className)
	{
		if (IsAlive(obj))
		{
			return string.Equals(obj.GetClass(), className, StringComparison.Ordinal);
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CTraverseDepthFirst_003Ed__2))]
	public static IEnumerable<Node> TraverseDepthFirst(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CTraverseDepthFirst_003Ed__2(-2)
		{
			_003C_003E3__root = root
		};
	}

	public static List<Node> FindNodesByClass(Node root, string className)
	{
		List<Node> list = new List<Node>();
		foreach (Node item in TraverseDepthFirst(root))
		{
			if (IsClass((GodotObject?)(object)item, className))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void SafeQueueFree(Node? node)
	{
		if (IsAlive((GodotObject?)(object)node))
		{
			node.QueueFree();
		}
	}

	public static void ClearChildren(Node node)
	{
		foreach (Node child in node.GetChildren(false))
		{
			node.RemoveChild(child);
			child.QueueFree();
		}
	}
}

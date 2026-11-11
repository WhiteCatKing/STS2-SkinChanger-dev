using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal static class CombatActorDrawOrder
{
	private sealed class CombatLease(IDisposable? actor, IDisposable? container) : IDisposable
	{
		public void Dispose()
		{
			actor?.Dispose();
			container?.Dispose();
		}
	}

	private sealed class OrderState
	{
		internal readonly List<Node> Original = new List<Node>(original);

		internal readonly Dictionary<Node, int> Active = new Dictionary<Node, int>();

		public OrderState(IEnumerable<Node> original)
		{
		}
	}

	private sealed class Lease(Node actor, Node parent, OrderState state) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			int num = state.Active[actor] - 1;
			if (num > 0)
			{
				state.Active[actor] = num;
				return;
			}
			state.Active.Remove(actor);
			if (GodotObject.IsInstanceValid((GodotObject)(object)actor) && GodotObject.IsInstanceValid((GodotObject)(object)parent) && actor.GetParent() == parent && !((GodotObject)parent).IsQueuedForDeletion())
			{
				int num2 = state.Original.IndexOf(actor);
				Node val = null;
				for (int i = num2 + 1; i < state.Original.Count; i++)
				{
					Node val2 = state.Original[i];
					if (GodotObject.IsInstanceValid((GodotObject)(object)val2) && val2.GetParent() == parent && !state.Active.ContainsKey(val2))
					{
						val = val2;
						break;
					}
				}
				int num3 = ((val == null) ? (parent.GetChildCount(false) - 1) : val.GetIndex(false));
				if (val != null && actor.GetIndex(false) < num3)
				{
					num3--;
				}
				parent.MoveChild(actor, num3);
			}
			if (state.Active.Count == 0)
			{
				Parents.Remove(parent);
			}
		}
	}

	private static readonly Dictionary<Node2D, (IDisposable Lease, HashSet<string> Owners)> Active = new Dictionary<Node2D, (IDisposable, HashSet<string>)>();

	private static readonly Dictionary<Node, OrderState> Parents = new Dictionary<Node, OrderState>();

	internal static void AcquireTracked(Node2D visual, string owner)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (!Active.TryGetValue(visual, out (IDisposable, HashSet<string>) value))
		{
			IDisposable disposable = Acquire(visual);
			if (disposable == null)
			{
				return;
			}
			value = (Active[visual] = (disposable, new HashSet<string>()));
			if (!((GodotObject)visual).HasMeta(StringName.op_Implicit("czn_core_melee_order_cleanup")))
			{
				((GodotObject)visual).SetMeta(StringName.op_Implicit("czn_core_melee_order_cleanup"), Variant.op_Implicit(true));
				((Node)visual).TreeExiting += delegate
				{
					if (Active.Remove(visual, out (IDisposable, HashSet<string>) value2))
					{
						value2.Item1.Dispose();
					}
				};
			}
		}
		value.Item2.Add(owner);
	}

	internal static bool ReleaseTracked(Node2D visual, string owner)
	{
		if (!Active.TryGetValue(visual, out (IDisposable, HashSet<string>) value) || !value.Item2.Remove(owner))
		{
			return false;
		}
		if (value.Item2.Count == 0)
		{
			Active.Remove(visual);
			value.Item1.Dispose();
		}
		return true;
	}

	public static IDisposable? Acquire(Node2D visual)
	{
		Node val = (Node)(object)visual;
		while (val != null && !(val is NCreature))
		{
			val = val.GetParent();
		}
		NCreature val2 = (NCreature)(object)((val is NCreature) ? val : null);
		if (val2 == null)
		{
			return null;
		}
		return AcquireCombatOrder((Node)(object)val2);
	}

	internal static IDisposable? AcquireCombatOrder(Node actor)
	{
		Node parent = actor.GetParent();
		IDisposable container = null;
		if (((parent != null) ? parent.Name : null) == StringName.op_Implicit("AllyContainer"))
		{
			Node parent2 = parent.GetParent();
			if (((parent2 != null) ? parent2.GetNodeOrNull(NodePath.op_Implicit("EnemyContainer")) : null) != null)
			{
				container = AcquireSiblingOrder(parent);
			}
		}
		return new CombatLease(AcquireSiblingOrder(actor), container);
	}

	internal static IDisposable? AcquireSiblingOrder(Node actor)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)actor))
		{
			Node parent = actor.GetParent();
			if (parent != null)
			{
				if (!Parents.TryGetValue(parent, out OrderState value))
				{
					value = (Parents[parent] = new OrderState((IEnumerable<Node>)parent.GetChildren(false)));
				}
				value.Active[actor] = value.Active.GetValueOrDefault(actor) + 1;
				parent.MoveChild(actor, parent.GetChildCount(false) - 1);
				return new Lease(actor, parent, value);
			}
		}
		return null;
	}
}

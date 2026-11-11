using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

internal sealed class CombatCinematicFeedback : IDisposable
{
	private sealed record Binding(CombatCinematicFeedback Owner, Node2D Proxy, Vector2 LocalPoint)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Owner = ");
			builder.Append(Owner);
			builder.Append(", Proxy = ");
			builder.Append(Proxy);
			builder.Append(", LocalPoint = ");
			Vector2 localPoint = LocalPoint;
			builder.Append(((object)(Vector2)(ref localPoint)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	private static readonly Dictionary<Node, Binding> Targets = new Dictionary<Node, Binding>();

	private static readonly Dictionary<Node, List<(Node2D Proxy, Vector2 Point)>> Mirrors = new Dictionary<Node, List<(Node2D, Vector2)>>();

	private static readonly Dictionary<CanvasLayer, CombatCinematicFeedback> Stages = new Dictionary<CanvasLayer, CombatCinematicFeedback>();

	private readonly List<Node> _targets = new List<Node>();

	private readonly CanvasLayer _layer;

	private bool _disposed;

	private int _popups;

	public CombatCinematicFeedback(CanvasLayer stage)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		_layer = new CanvasLayer
		{
			Name = StringName.op_Implicit("CznCinematicDamage"),
			Layer = stage.Layer + 1
		};
		((Node)((Node)stage).GetTree().Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
	}

	public void Bind(Node nativeTarget, Node2D proxy, Vector2 localPoint)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!_targets.Contains(nativeTarget))
		{
			_targets.Add(nativeTarget);
		}
		Targets[nativeTarget] = new Binding(this, proxy, localPoint);
	}

	internal static void RegisterMirror(Node2D source, Node2D proxy)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Node val = (Node)(object)source;
		while (val != null && !(val is NCreature))
		{
			val = val.GetParent();
		}
		NCreature val2 = (NCreature)(object)((val is NCreature) ? val : null);
		if (val2 != null)
		{
			Transform2D globalTransform = source.GlobalTransform;
			Vector2 point = ((Transform2D)(ref globalTransform)).AffineInverse() * val2.VfxSpawnPosition;
			RegisterMirror((Node)(object)val2, proxy, point);
		}
	}

	internal static void RegisterMirror(Node creature, Node2D proxy, Vector2 point)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((Node)proxy).TreeEntered += Enter;
		((Node)proxy).TreeExiting += Exit;
		if (((Node)proxy).IsInsideTree())
		{
			Enter();
		}
		void Enter()
		{
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			if (!Mirrors.TryGetValue(creature, out List<(Node2D, Vector2)> value))
			{
				value = (Mirrors[creature] = new List<(Node2D, Vector2)>());
			}
			if (!value.Exists(((Node2D Proxy, Vector2 Point) item) => item.Proxy == proxy))
			{
				value.Add((proxy, point));
			}
		}
		void Exit()
		{
			if (Mirrors.TryGetValue(creature, out List<(Node2D, Vector2)> value2))
			{
				value2.RemoveAll(((Node2D Proxy, Vector2 Point) item) => item.Proxy == proxy);
				if (value2.Count == 0)
				{
					Mirrors.Remove(creature);
				}
			}
		}
	}

	private static void ResolveVisibleMirror(Node target)
	{
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (!Mirrors.TryGetValue(target, out List<(Node2D, Vector2)> value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			(Node2D, Vector2) tuple = value[num];
			if (GodotObject.IsInstanceValid((GodotObject)(object)tuple.Item1) && ((CanvasItem)tuple.Item1).IsVisibleInTree())
			{
				Node parent = ((Node)tuple.Item1).GetParent();
				while (parent != null && !(parent is CanvasLayer))
				{
					parent = parent.GetParent();
				}
				CanvasLayer stage = (CanvasLayer)(object)((parent is CanvasLayer) ? parent : null);
				if (stage != null && stage.Layer > 0)
				{
					if (!Stages.TryGetValue(stage, out CombatCinematicFeedback scope))
					{
						Stages[stage] = (scope = new CombatCinematicFeedback(stage));
						((Node)stage).TreeExiting += delegate
						{
							scope.Dispose();
							Stages.Remove(stage);
						};
					}
					scope.Bind(target, tuple.Item1, tuple.Item2);
					break;
				}
			}
		}
	}

	public static bool TryMount(Node nativeTarget, Node2D popup, out Func<Vector2>? position)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		position = null;
		ResolveVisibleMirror(nativeTarget);
		if (!Targets.TryGetValue(nativeTarget, out Binding binding) || binding.Owner._disposed || !GodotObject.IsInstanceValid((GodotObject)(object)binding.Proxy) || !((Node)binding.Proxy).IsInsideTree() || !((CanvasItem)binding.Proxy).IsVisibleInTree())
		{
			return false;
		}
		CombatCinematicFeedback owner = binding.Owner;
		Vector2 last = ((CanvasItem)binding.Proxy).GetGlobalTransformWithCanvas() * binding.LocalPoint;
		((Node)owner._layer).AddChild((Node)(object)popup, false, (InternalMode)0);
		owner._popups++;
		((Node)popup).TreeExiting += delegate
		{
			owner._popups--;
			owner.TryFree();
		};
		position = delegate
		{
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			if (!owner._disposed && GodotObject.IsInstanceValid((GodotObject)(object)binding.Proxy) && ((Node)binding.Proxy).IsInsideTree() && ((CanvasItem)binding.Proxy).IsVisibleInTree())
			{
				last = ((CanvasItem)binding.Proxy).GetGlobalTransformWithCanvas() * binding.LocalPoint;
			}
			return last;
		};
		return true;
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		foreach (Node target in _targets)
		{
			if (Targets.TryGetValue(target, out Binding value) && value.Owner == this)
			{
				Targets.Remove(target);
			}
		}
		_targets.Clear();
		TryFree();
	}

	private void TryFree()
	{
		if (_disposed && _popups == 0 && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
	}
}

using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznCore.Presentation;

public static class CombatFollowAnchor
{
	private static bool Live(Node node)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)node) && node.IsInsideTree())
		{
			return !((GodotObject)node).IsQueuedForDeletion();
		}
		return false;
	}

	private static void Bind(Node2D anchor, Action update, bool immediate)
	{
		SceneTree tree = ((Node)anchor).GetTree();
		tree.ProcessFrame += update;
		((Node)anchor).TreeExiting += delegate
		{
			tree.ProcessFrame -= update;
		};
		if (immediate)
		{
			update();
		}
	}

	private static bool Usable(Node2D anchor)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)anchor))
		{
			return !((GodotObject)anchor).IsQueuedForDeletion();
		}
		return false;
	}

	private static void Retire(Node2D anchor)
	{
		((CanvasItem)anchor).Hide();
		((Node)anchor).QueueFree();
	}

	public static void FollowTranslation(Node2D anchor, CanvasItem target)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		Vector2 offset = anchor.GlobalPosition - target.GetGlobalTransform().Origin;
		Bind(anchor, delegate
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if (Usable(anchor))
			{
				if (!Live((Node)(object)target))
				{
					Retire(anchor);
				}
				else if (((Node)anchor).CanProcess())
				{
					anchor.GlobalPosition = target.GetGlobalTransform().Origin + offset;
				}
			}
		}, immediate: false);
	}

	public static void FollowPosition(Node2D anchor, Node target, Func<Vector2> resolvePosition)
	{
		Bind(anchor, delegate
		{
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (Usable(anchor))
			{
				if (!Live(target))
				{
					Retire(anchor);
				}
				else if (((Node)anchor).CanProcess())
				{
					anchor.GlobalPosition = resolvePosition();
				}
			}
		}, immediate: true);
	}

	public static Vector2 ResolveFloor(NCreature target)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(target.VfxSpawnPosition.X, target.GetBottomOfHitbox().Y);
	}

	public static bool TryResolveLiveSideFloor(IReadOnlyList<NCreature> targets, out Vector2 position)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		float num2 = float.NegativeInfinity;
		int num3 = 0;
		for (int i = 0; i < targets.Count; i++)
		{
			NCreature val = targets[i];
			if (Live((Node)(object)val))
			{
				Creature entity = val.Entity;
				if (entity == null || entity.IsAlive)
				{
					Vector2 val2 = ResolveFloor(val);
					num += val2.X;
					num2 = Math.Max(num2, val2.Y);
					num3++;
				}
			}
		}
		position = ((num3 == 0) ? default(Vector2) : new Vector2(num / (float)num3, num2));
		return num3 != 0;
	}

	public static void FollowSideFloor(Node2D anchor, IReadOnlyList<NCreature> targets)
	{
		Bind(anchor, delegate
		{
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			if (Usable(anchor))
			{
				Vector2 position;
				if (!((Node)anchor).CanProcess())
				{
					for (int i = 0; i < targets.Count; i++)
					{
						if (Live((Node)(object)targets[i]))
						{
							Creature entity = targets[i].Entity;
							if (entity == null || entity.IsAlive)
							{
								return;
							}
						}
					}
					Retire(anchor);
				}
				else if (!TryResolveLiveSideFloor(targets, out position))
				{
					Retire(anchor);
				}
				else
				{
					anchor.GlobalPosition = position;
				}
			}
		}, immediate: true);
	}

	public static void LockToScreen(Node2D anchor)
	{
		Bind(anchor, delegate
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			if (Usable(anchor) && ((Node)anchor).CanProcess())
			{
				Node2D obj = anchor;
				Transform2D canvasTransform = ((CanvasItem)anchor).GetCanvasTransform();
				Transform2D val = ((Transform2D)(ref canvasTransform)).AffineInverse();
				Rect2 viewportRect = ((CanvasItem)anchor).GetViewportRect();
				obj.GlobalTransform = val * new Transform2D(0f, ((Rect2)(ref viewportRect)).GetCenter());
			}
		}, immediate: true);
	}
}

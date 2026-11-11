using System.Collections.Generic;
using System.Linq;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyTargetAnchor
{
	internal static void Follow(Node2D anchor, CanvasItem target)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		SceneTree tree = ((Node)anchor).GetTree();
		Vector2 offset = anchor.GlobalPosition - target.GetGlobalTransform().Origin;
		tree.ProcessFrame += Update;
		((Node)anchor).TreeExiting += delegate
		{
			tree.ProcessFrame -= Update;
		};
		void Update()
		{
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor) && !((GodotObject)anchor).IsQueuedForDeletion())
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)target) || !((Node)target).IsInsideTree() || ((GodotObject)target).IsQueuedForDeletion())
				{
					((CanvasItem)anchor).Visible = false;
					((Node)anchor).QueueFree();
				}
				else if (((Node)anchor).CanProcess())
				{
					anchor.GlobalPosition = target.GetGlobalTransform().Origin + offset;
				}
			}
		}
	}

	internal static void FollowCreature(Node2D anchor, NCreature target, EnemyTargetOrigin origin)
	{
		SceneTree tree = ((Node)anchor).GetTree();
		tree.ProcessFrame += Update;
		((Node)anchor).TreeExiting += delegate
		{
			tree.ProcessFrame -= Update;
		};
		Update();
		void Update()
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor) && !((GodotObject)anchor).IsQueuedForDeletion())
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)target) || !((Node)target).IsInsideTree() || ((GodotObject)target).IsQueuedForDeletion())
				{
					((CanvasItem)anchor).Visible = false;
					((Node)anchor).QueueFree();
				}
				else if (((Node)anchor).CanProcess())
				{
					anchor.GlobalPosition = EnemyTargetPosition.Resolve(target, origin);
				}
			}
		}
	}

	internal static void FollowSide(Node2D anchor, IReadOnlyList<NCreature> targets)
	{
		SceneTree tree = ((Node)anchor).GetTree();
		tree.ProcessFrame += Update;
		((Node)anchor).TreeExiting += delegate
		{
			tree.ProcessFrame -= Update;
		};
		Update();
		void Update()
		{
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor) && !((GodotObject)anchor).IsQueuedForDeletion())
			{
				NCreature[] array = targets.Where(delegate(NCreature n)
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)n) && ((Node)n).IsInsideTree() && !((GodotObject)n).IsQueuedForDeletion())
					{
						Creature entity = n.Entity;
						if (entity == null)
						{
							return true;
						}
						return entity.IsAlive;
					}
					return false;
				}).ToArray();
				if (array.Length == 0)
				{
					((CanvasItem)anchor).Visible = false;
					((Node)anchor).QueueFree();
				}
				else if (((Node)anchor).CanProcess())
				{
					anchor.GlobalPosition = EnemyTargetPosition.ResolveSideFloor(array);
				}
			}
		}
	}

	internal static void LockToScreen(Node2D anchor)
	{
		SceneTree tree = ((Node)anchor).GetTree();
		tree.ProcessFrame += Update;
		((Node)anchor).TreeExiting += delegate
		{
			tree.ProcessFrame -= Update;
		};
		Update();
		void Update()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)anchor) && !((GodotObject)anchor).IsQueuedForDeletion() && ((Node)anchor).CanProcess())
			{
				Node2D obj = anchor;
				Transform2D canvasTransform = ((CanvasItem)anchor).GetCanvasTransform();
				Transform2D val = ((Transform2D)(ref canvasTransform)).AffineInverse();
				Rect2 viewportRect = ((CanvasItem)anchor).GetViewportRect();
				obj.GlobalTransform = val * new Transform2D(0f, ((Rect2)(ref viewportRect)).GetCenter());
			}
		}
	}
}

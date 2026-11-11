using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CreatureIntentDepth
{
	private static readonly ConditionalWeakTable<Control, CreatureIntentDepthState> States = new ConditionalWeakTable<Control, CreatureIntentDepthState>();

	internal static void Apply(NCreature creature)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Creature entity = creature.Entity;
			if (entity == null || !entity.IsMonster || !GodotObject.IsInstanceValid((GodotObject)(object)creature.Visuals) || !GodotObject.IsInstanceValid((GodotObject)(object)creature.IntentContainer))
			{
				return;
			}
			Control intentContainer = creature.IntentContainer;
			NCreatureVisuals visuals = creature.Visuals;
			if (!((Node)intentContainer).IsInsideTree() || !((Node)visuals).IsInsideTree() || ((CanvasItem)intentContainer).GetCanvas() != ((CanvasItem)visuals).GetCanvas())
			{
				return;
			}
			int num = EffectiveZ((CanvasItem?)(object)visuals);
			Node val = (Node)(object)visuals;
			bool flag = !IsAfterVisualBranch((Node)(object)creature, (Node)(object)intentContainer, (Node)(object)visuals) || ((CanvasItem)intentContainer).ShowBehindParent;
			Stack<Node> stack = new Stack<Node>();
			stack.Push((Node)(object)visuals);
			Node result;
			while (stack.TryPop(out result))
			{
				if ((result is CanvasLayer || result is Viewport) ? true : false)
				{
					continue;
				}
				CanvasItem val2 = (CanvasItem)(object)((result is CanvasItem) ? result : null);
				if (val2 != null)
				{
					int num2 = EffectiveZ(val2);
					if (num2 > num)
					{
						num = num2;
						val = (Node)(object)val2;
					}
					flag |= val2.TopLevel;
				}
				foreach (Node child in result.GetChildren(false))
				{
					stack.Push(child);
				}
			}
			int num3 = ((!((CanvasItem)intentContainer).TopLevel) ? EffectiveZ((CanvasItem?)/*isinst with value type is only supported in some contexts*/) : 0);
			(int, bool) tuple = (((CanvasItem)intentContainer).ZIndex, ((CanvasItem)intentContainer).ZAsRelative);
			(int, bool) tuple2 = States.GetValue(intentContainer, (Control _) => new CreatureIntentDepthState()).Resolve(tuple.Item1, tuple.Item2, num3, num, flag);
			(int, bool) tuple3 = tuple2;
			(int, bool) tuple4 = tuple;
			if (tuple3.Item1 != tuple4.Item1 || tuple3.Item2 != tuple4.Item2)
			{
				((CanvasItem)intentContainer).ZIndex = tuple2.Item1;
				((CanvasItem)intentContainer).ZAsRelative = tuple2.Item2;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(62, 7);
				defaultInterpolatedStringHandler.AppendLiteral("意图图层保护 monster=");
				MonsterModel monster = creature.Entity.Monster;
				defaultInterpolatedStringHandler.AppendFormatted((monster != null) ? ((AbstractModel)monster).Id.Entry : null);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendLiteral("visual=");
				defaultInterpolatedStringHandler.AppendFormatted<NodePath>(((Node)creature).GetPathTo(val, false));
				defaultInterpolatedStringHandler.AppendLiteral(" visualZ=");
				defaultInterpolatedStringHandler.AppendFormatted(num);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendLiteral("intent=");
				defaultInterpolatedStringHandler.AppendFormatted(tuple);
				defaultInterpolatedStringHandler.AppendLiteral("->");
				defaultInterpolatedStringHandler.AppendFormatted(tuple2);
				defaultInterpolatedStringHandler.AppendLiteral(" parentZ=");
				defaultInterpolatedStringHandler.AppendFormatted(num3);
				defaultInterpolatedStringHandler.AppendLiteral(" position=");
				defaultInterpolatedStringHandler.AppendFormatted<Vector2>(intentContainer.GlobalPosition);
				defaultInterpolatedStringHandler.AppendLiteral("。");
				ModLog.Info(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("意图图层保护未应用：" + ex.GetBaseException().Message);
		}
	}

	private static int EffectiveZ(CanvasItem? item)
	{
		if (item == null)
		{
			return 0;
		}
		CanvasItem item2 = (CanvasItem)((item.ZAsRelative && !item.TopLevel) ? /*isinst with value type is only supported in some contexts*/: null);
		return Math.Clamp(item.ZIndex + EffectiveZ(item2), -4096, 4096);
	}

	private static bool IsAfterVisualBranch(Node creature, Node intent, Node visuals)
	{
		Node val = Branch(creature, intent);
		Node val2 = Branch(creature, visuals);
		if (val != null && val2 != null)
		{
			return val.GetIndex(false) > val2.GetIndex(false);
		}
		return false;
		static Node? Branch(Node owner, Node node)
		{
			while (true)
			{
				Node parent = node.GetParent();
				if (parent == null)
				{
					break;
				}
				if (parent == owner)
				{
					return node;
				}
				node = parent;
			}
			return null;
		}
	}
}

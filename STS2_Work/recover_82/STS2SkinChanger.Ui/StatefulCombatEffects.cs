using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class StatefulCombatEffects : Node
{
	private sealed record Effect(Node Node, Node? Driver, NCreatureVisuals Visuals, CardModel Source, string Provider, StatefulCardArtRuntime.Runtime Runtime);

	private readonly List<Effect> _effects = new List<Effect>();

	private double _elapsed;

	private const string NodeName = "SCStatefulCombatEffects";

	internal static void Play(CardModel source, Creature target, StatefulCardArtRuntime.Runtime runtime)
	{
		NCombatRoom instance = NCombatRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NCreature creatureNode = instance.GetCreatureNode(target);
			obj = ((creatureNode != null) ? creatureNode.Visuals : null);
		}
		NCreatureVisuals visuals = (NCreatureVisuals)obj;
		if (instance == null || visuals == null)
		{
			return;
		}
		StatefulCombatEffects statefulCombatEffects = ((Node)instance).GetNodeOrNull<StatefulCombatEffects>(NodePath.op_Implicit("SCStatefulCombatEffects"));
		if (statefulCombatEffects == null)
		{
			StatefulCombatEffects statefulCombatEffects2 = new StatefulCombatEffects();
			((Node)statefulCombatEffects2).Name = StringName.op_Implicit("SCStatefulCombatEffects");
			statefulCombatEffects = statefulCombatEffects2;
			((Node)instance).AddChild((Node)(object)statefulCombatEffects, false, (InternalMode)0);
		}
		string kind = ((object)source).GetType().Name;
		NCard val = NCard.FindOnTable(source, (PileType?)null);
		if (val != null)
		{
			StatefulCardPulse.Play(val, runtime, kind);
		}
		if (kind != "Rattle" && statefulCombatEffects._effects.Any((Effect effect) => effect.Visuals == visuals && ((object)effect.Source).GetType() == ((object)source).GetType() && GodotObject.IsInstanceValid((GodotObject)(object)effect.Node)))
		{
			return;
		}
		Type type2 = runtime.Assembly.GetTypes().Single((Type type) => type.Name == "N" + kind + "Vfx");
		MethodInfo methodInfo = type2.GetMethods().Single((MethodInfo method) => method.Name == "Create" && !method.IsGenericMethod);
		object obj2 = Enum.Parse(methodInfo.GetParameters()[^1].ParameterType, (kind == "Rattle") ? "Timed" : "UntilRevert");
		object[] parameters = ((!(kind == "Flatten")) ? new object[3]
		{
			visuals,
			(kind == "Rattle") ? 0.5f : 0.4f,
			obj2
		} : new object[4] { visuals, null, 0.4f, obj2 });
		object? obj3 = methodInfo.Invoke(null, parameters);
		Node val2 = (Node)((obj3 is Node) ? obj3 : null);
		if (val2 != null)
		{
			object? obj4 = AccessTools.Field(type2, "_driver")?.GetValue(val2);
			Node driver = (Node)((obj4 is Node) ? obj4 : null);
			string text = SkinService.GetSelectedCardOption(source)?.ProviderId;
			if (text == null)
			{
				AccessTools.Method(type2, "ForceKill", (Type[])null, (Type[])null)?.Invoke(val2, null);
			}
			else
			{
				statefulCombatEffects._effects.Add(new Effect(val2, driver, visuals, source, text, runtime));
			}
		}
	}

	public override void _Process(double delta)
	{
		_elapsed += delta;
		if (_elapsed < 0.2)
		{
			return;
		}
		_elapsed = 0.0;
		Effect[] array = _effects.ToArray();
		foreach (Effect effect in array)
		{
			try
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)effect.Node) || !GodotObject.IsInstanceValid((GodotObject)(object)effect.Visuals) || !effect.Runtime.Enabled || effect.Runtime.Simple || SkinService.GetSelectedCardOption(effect.Source)?.ProviderId != effect.Provider)
				{
					Remove(effect);
				}
			}
			catch (Exception exception)
			{
				StatefulCardArtRuntime.Warn("effect cleanup", exception);
				Remove(effect);
			}
		}
	}

	private void Remove(Effect effect, bool exiting = false)
	{
		_effects.Remove(effect);
		if (GodotObject.IsInstanceValid((GodotObject)(object)effect.Node))
		{
			AccessTools.Method(((object)effect.Node).GetType(), "ForceKill", (Type[])null, (Type[])null)?.Invoke(effect.Node, null);
			AccessTools.Field(((object)effect.Node).GetType(), "_driver")?.SetValue(effect.Node, null);
		}
		Node driver = effect.Driver;
		if (driver == null || !GodotObject.IsInstanceValid((GodotObject)(object)driver) || !(AccessTools.Field(((object)driver).GetType(), "_activeModifiers")?.GetValue(driver) is ICollection { Count: 0 }))
		{
			return;
		}
		if (!exiting)
		{
			Node parent = driver.GetParent();
			if (parent != null)
			{
				parent.RemoveChild(driver);
			}
		}
		driver.QueueFree();
	}

	internal static void ClearCurrent()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		StatefulCombatEffects statefulCombatEffects = ((instance != null) ? ((Node)instance).GetNodeOrNull<StatefulCombatEffects>(NodePath.op_Implicit("SCStatefulCombatEffects")) : null);
		if (statefulCombatEffects == null)
		{
			return;
		}
		Effect[] array = statefulCombatEffects._effects.ToArray();
		foreach (Effect effect in array)
		{
			try
			{
				statefulCombatEffects.Remove(effect);
			}
			catch (Exception exception)
			{
				StatefulCardArtRuntime.Warn("turn effect cleanup", exception);
			}
		}
	}

	public override void _ExitTree()
	{
		Effect[] array = _effects.ToArray();
		foreach (Effect effect in array)
		{
			try
			{
				Remove(effect, exiting: true);
			}
			catch (Exception exception)
			{
				StatefulCardArtRuntime.Warn("combat exit cleanup", exception);
			}
		}
	}
}

using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class CombatCreatureNodeResolver
{
	public NCreature? ResolvePlayer(NCombatRoom? room, Player? player, string failureMessage, Action<string>? warn = null)
	{
		return Resolve(room, (player != null) ? player.Creature : null, failureMessage, warn);
	}

	public NCreature? Resolve(NCombatRoom? room, Creature? creature, string failureMessage, Action<string>? warn = null)
	{
		if (room == null || creature == null)
		{
			return null;
		}
		try
		{
			NCreature creatureNode = room.GetCreatureNode(creature);
			return GodotNodeUtil.IsAlive((GodotObject?)(object)creatureNode) ? creatureNode : null;
		}
		catch (Exception ex)
		{
			warn?.Invoke(failureMessage + ": " + SkinLog.FormatException(ex));
			return null;
		}
	}

	public IReadOnlyList<NCreature>? ResolveMany(NCombatRoom? room, IReadOnlyList<Creature>? creatures, string failureMessage, Action<string>? warn = null)
	{
		if (room == null || creatures == null || creatures.Count == 0)
		{
			return null;
		}
		if (creatures.Count == 1)
		{
			NCreature val = Resolve(room, creatures[0], failureMessage, warn);
			if (val != null)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<NCreature>(val);
			}
			return null;
		}
		List<NCreature> list = null;
		foreach (Creature creature in creatures)
		{
			NCreature val2 = Resolve(room, creature, failureMessage, warn);
			if (val2 != null)
			{
				if (list == null)
				{
					list = new List<NCreature>();
				}
				list.Add(val2);
			}
		}
		return list;
	}
}

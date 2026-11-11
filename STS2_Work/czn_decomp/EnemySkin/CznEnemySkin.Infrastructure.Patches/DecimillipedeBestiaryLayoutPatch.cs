using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(NBestiaryLayoutDecimillipede), "Setup")]
internal static class DecimillipedeBestiaryLayoutPatch
{
	private static void Prefix(List<NCreature> ____creatures)
	{
		foreach (NCreature ____creature in ____creatures)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)____creature))
			{
				Node parent = ((Node)____creature).GetParent();
				if (parent != null)
				{
					parent.RemoveChild((Node)(object)____creature);
				}
				((Node)____creature).QueueFree();
			}
		}
		____creatures.Clear();
	}
}

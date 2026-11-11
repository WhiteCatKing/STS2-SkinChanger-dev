using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal class FrameworkPreviewCreature : NCreature
{
	internal void Initialize(Player player, NCreatureVisuals visuals)
	{
		AccessTools.PropertySetter(typeof(NCreature), "Entity").Invoke(this, new object[1] { player.Creature });
		AccessTools.PropertySetter(typeof(NCreature), "Visuals").Invoke(this, new object[1] { visuals });
		((Node)this).AddChild((Node)(object)visuals, false, (InternalMode)0);
	}

	public override void _EnterTree()
	{
	}

	public override void _Ready()
	{
	}

	public override void _ExitTree()
	{
		((NCreature)this).DeathAnimCancelToken.Cancel();
	}
}

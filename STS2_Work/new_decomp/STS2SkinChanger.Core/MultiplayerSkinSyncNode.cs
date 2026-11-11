using Godot;

namespace STS2SkinChanger.Core;

internal class MultiplayerSkinSyncNode : Node
{
	public override void _Ready()
	{
		MultiplayerSkinSync.AttachToRun();
	}

	public override void _Process(double delta)
	{
		MultiplayerSkinSync.Tick(delta);
	}

	public override void _ExitTree()
	{
		MultiplayerSkinSync.DetachFromRun(clearCapabilities: false);
	}
}

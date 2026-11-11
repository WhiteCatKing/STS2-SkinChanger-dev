using Godot;

namespace STS2SkinChanger.Ui;

internal class CharacterAppearanceRuntimeNode : Node
{
	public override void _Ready()
	{
		((Node)this).SetProcess(false);
	}

	public void Wake()
	{
		((Node)this).SetProcess(true);
	}

	public override void _Process(double delta)
	{
		if (!CharacterAppearanceRuntime.ProcessPendingSelection())
		{
			((Node)this).SetProcess(false);
		}
	}

	public override void _ExitTree()
	{
		CharacterAppearanceRuntime.ClearPendingSelection();
	}
}

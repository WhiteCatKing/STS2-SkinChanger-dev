using Godot;

namespace STS2SkinChanger.Core;

internal partial class SkinLoadingInputObserver : Node
{
    internal static bool CountsAsAction(InputEvent input) => input switch
    {
        InputEventMouseButton mouse => mouse.Pressed,
        InputEventKey key => key.Pressed && !key.Echo,
        InputEventJoypadButton button => button.Pressed,
        InputEventAction action => action.Pressed,
        _ => false
    };
    public override void _Input(InputEvent input) { if (CountsAsAction(input)) CharacterPackPrefetch.NoteAction(); }
    internal static void Install()
    {
        if (Engine.GetMainLoop() is SceneTree tree && tree.Root.GetNodeOrNull<Node>("SCSkinIdleInput") == null)
            tree.Root.AddChild(new SkinLoadingInputObserver { Name = "SCSkinIdleInput", ProcessMode = ProcessModeEnum.Always });
    }
}

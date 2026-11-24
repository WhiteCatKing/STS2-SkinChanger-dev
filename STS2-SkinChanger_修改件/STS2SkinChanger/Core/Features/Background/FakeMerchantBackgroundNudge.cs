using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2SkinChanger.Core;

internal sealed partial class FakeMerchantBackgroundNudge : Node
{
    private static Vector2 _pan;
    private static int _zoomPercent = 100;

    private NCombatBackground _background = null!;
    private Vector2 _origin;
    private Vector2 _scale;

    internal static void Attach(NCombatBackground background)
    {
        if (background.GetNodeOrNull<FakeMerchantBackgroundNudge>("SkinChangerBackgroundNudge") != null) return;
        var nudge = new FakeMerchantBackgroundNudge
        {
            Name = "SkinChangerBackgroundNudge",
            _background = background,
            _origin = background.Position,
            _scale = background.Scale
        };
        background.AddChild(nudge);
        nudge.Apply();
    }

    public override void _Input(InputEvent input)
    {
        if (input is not InputEventKey { Pressed: true, Echo: false, CtrlPressed: false, AltPressed: false, MetaPressed: false, ShiftPressed: false } key ||
            !_background.IsVisibleInTree()) return;

        switch (key.Keycode)
        {
            case Key.U: _zoomPercent++; break;
            case Key.O: _zoomPercent = Math.Max(1, _zoomPercent - 1); break;
            case Key.I: _pan += Vector2.Up; break;
            case Key.K: _pan += Vector2.Down; break;
            case Key.J: _pan += Vector2.Left; break;
            case Key.L: _pan += Vector2.Right; break;
            default: return;
        }

        Apply();
        GetViewport().SetInputAsHandled();
        ModLog.Info($"假商人战斗背景微调：缩放={_zoomPercent}%，偏移={_pan} px。");
    }

    private void Apply()
    {
        _background.Position = _origin + _pan;
        _background.Scale = _scale * (_zoomPercent / 100f);
    }
}

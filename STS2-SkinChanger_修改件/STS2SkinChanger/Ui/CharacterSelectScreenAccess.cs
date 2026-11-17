using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Ui;

// All three native screens keep their own lobby/gameplay lifecycle. Share cosmetic controls,
// never reparent their native nodes or create a fake standard-mode screen.
internal static class CharacterSelectScreenAccess
{
    internal static readonly string[] ModeTypes = [
        "MegaCrit.Sts2.Core.Nodes.Screens.Blitz.NBlitzScreen",
        "MegaCrit.Sts2.Core.Nodes.Screens.Dread.NDreadScreen"];
    internal static bool IsScreenType(Type? type) => type != null &&
        (typeof(NCharacterSelectScreen).IsAssignableFrom(type) || ModeTypes.Contains(type.FullName, StringComparer.Ordinal));
    internal static bool IsSupported(Control screen) => IsScreenType(screen.GetType());
    internal static Control? Find(Node node)
    {
        for (Node? current = node; current != null; current = current.GetParent())
            if (current is Control control && IsSupported(control)) return control;
        return null;
    }
    internal static object? Read(FieldInfo? field, Control screen) => field == null ? null :
        (field.DeclaringType!.IsInstanceOfType(screen) ? field : AccessTools.Field(screen.GetType(), field.Name))?.GetValue(screen);
    internal static StartRunLobby Lobby(Control screen) =>
        (StartRunLobby)(AccessTools.Property(screen.GetType(), "Lobby")?.GetValue(screen) ??
        AccessTools.Field(screen.GetType(), "_lobby")?.GetValue(screen) ??
        throw new InvalidOperationException("选角大厅尚未初始化。"));
    internal static Control? Background(Control screen) =>
        AccessTools.Field(screen.GetType(), "_bgContainer")?.GetValue(screen) as Control ??
        screen.GetNodeOrNull<Control>("AnimatedBg") ?? screen.GetNodeOrNull<Control>("%AnimatedBg");
    internal static void EnsureHost(Control screen)
    {
        if (screen.GetNodeOrNull<Control>("InfoPanel") != null || !IsSupported(screen)) return;
        // Dread intentionally has no standard stats panel. This is only a cosmetic anchor.
        var host = new Control { Name = "InfoPanel", MouseFilter = Control.MouseFilterEnum.Ignore };
        screen.AddChild(host);
        host.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
        host.Position = new Vector2(280, 330);
        host.Size = new Vector2(680, 390);
    }
}

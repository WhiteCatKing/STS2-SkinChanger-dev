using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

internal static class ByrdineAttackTests
{
    internal static void Run(string game)
    {
        using var pack = PckArchive.Open(game);
        var scene = Encoding.UTF8.GetString(pack.ReadFile(ByrdineAttackPreview.ScenePath));
        foreach (var token in new[] { "NByrdineAttackVfx.cs", "byrdine_overlay_skel_data.tres",
            "attack_overlay", "fx_placeholder", "normal_material", "SpineSlotNode", "Vector2(0.25, 0.25)" })
            Require(scene.Contains(token), "Native full-screen effect contract missing: " + token);
        Require(ByrdineAttackPreview.TakeoffDelay == .6 && ByrdineAttackPreview.AttackSfx == "event:/sfx/byrdpip/byrd_2_attack",
            "Official timing/sound not preserved.");
        var nativePower = typeof(MonsterModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Powers.GodByrdPower");
        if (nativePower != null)
        {
            var method = AccessTools.Method(nativePower, "AfterPlayerTurnStart");
            var state = method.GetCustomAttribute<AsyncStateMachineAttribute>()!.StateMachineType;
            var native = PatchProcessor.GetOriginalInstructions(AccessTools.Method(state, "MoveNext"));
            Require(native.Any(i => i.operand is float value && value == .6f) &&
                native.Any(i => Equals(i.operand, ByrdineAttackPreview.AttackSfx)) &&
                native.Any(i => i.operand is MethodBase m && m.DeclaringType?.Name == "NByrdineAttackVfx" && m.Name == "Create"),
                "Preview timing must match actual native power, not a guessed phase duration.");
        }
        var screen = typeof(ContextualSkinControls).Assembly.GetType("STS2SkinChanger.Ui.AncientCompendiumScreen", true)!;
        var callbacks = screen.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            .Where(m => m.GetMethodBody() != null);
        Require(callbacks.Any(m => Calls(m, typeof(ByrdineAttackPreview), "Start")), "Actual attack button must dispatch the full sequence.");
        Require(Calls(AccessTools.Method(screen, "ClearPreview"), typeof(ByrdineAttackPreview), "Cancel"), "Preview switch must cancel full-screen effect.");
        var start = AccessTools.Method(typeof(ByrdineAttackPreview), "Start");
        Require(Calls(start, typeof(ByrdineAttackPreview), "Cancel") && Calls(start, typeof(Tween), "TweenInterval") &&
            Calls(start, typeof(ManagedAncientSceneAnimation), "TryPlay"), "Restart must cancel old playback while retaining takeoff/landing.");
        var playback = typeof(ByrdineAttackPreview).GetNestedType("Playback", BindingFlags.NonPublic)!;
        var show = PatchProcessor.GetOriginalInstructions(AccessTools.Method(playback, "Show"));
        var extract = show.FindIndex(i => i.operand is MethodBase m && m.Name == "ExtractVisual");
        var attach = show.FindIndex(i => i.operand is MethodBase m && m.Name == "AddChild");
        Require(extract >= 0 && attach > extract, "Native script root must not enter the preview tree with a null Creature.");
        Require(Calls(AccessTools.Method(playback, "Stop"), typeof(Tween), "Kill") &&
            Calls(AccessTools.Method(playback, "Stop"), typeof(Node), "remove_TreeExiting"), "Cleanup must kill timer and release exit signal.");
        Require(show.All(i => i.operand is not MethodBase m ||
            (m.DeclaringType?.Name != "CreatureCmd" && m.DeclaringType?.Name != "PowerCmd" &&
             m.DeclaringType?.Name != "NByrdineAttackVfx")), "Preview must not invoke combat commands/native gameplay factory.");
        Console.WriteLine("Byrdine attack preview passed: native overlay scene/shader tree, actual power delay/sound, attack button wiring, root extraction-before-entry and bounded cancellation.");
    }
    private static bool Calls(MethodBase method, Type type, string name) => PatchProcessor.GetOriginalInstructions(method)
        .Any(i => i.operand is MethodBase m && m.DeclaringType == type && m.Name == name);
    private static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
}

using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Screens;
using STS2SkinChanger.Ui;

// Inspect has a final BigPortrait write outside NCard.UpdateVisuals. Guard that real
// game boundary without starting Godot: the final inspect postfix must restore the
// selected portrait and then let an explicit external editor keep its ownership.
internal static class CardInspectPortraitBoundaryTests
{
    public static void Run()
    {
        var display = AccessTools.Method(typeof(NInspectCardScreen), "UpdateCardDisplay");
        var calls = Calls(display);
        var update = Array.FindLastIndex(calls, method => method.Name == nameof(NCard.UpdateVisuals));
        var portrait = Array.FindLastIndex(calls, method => method.Name == "SetPortraitTexture");
        Require(update >= 0 && portrait > update,
            "Inspect portrait lifecycle changed; review the final portrait boundary against the game.");

        var patch = typeof(CardSkinControls).Assembly.GetType("STS2SkinChanger.Ui.CardInspectSkinDisplayPatch")!;
        var target = patch.GetCustomAttribute<HarmonyPatch>()!.info;
        Require(target.declaringType == typeof(NInspectCardScreen) && target.methodName == "UpdateCardDisplay",
            "The repair must run after the inspect display, not only after NCard.UpdateVisuals.");
        var postfix = AccessTools.Method(patch, "Postfix");
        calls = Calls(postfix);
        var apply = Array.FindIndex(calls, method => method.Name == "ApplySelectedPortraitToNode");
        var editor = Array.FindIndex(calls, method => method.Name == "SynchronizeProvider");
        var selector = Array.FindIndex(calls, method => method.Name == "Sync");
        Require(apply >= 0 && editor > apply && selector > editor,
            "Inspect must restore the selected portrait, synchronize external ownership, then sync the selector.");
        Require(!calls.Any(method => method.Name is "RefreshCardSkin" or "UpdateVisuals"),
            "Inspect repair must preserve the upgrade preview and avoid recursive visual reloads.");
        Console.WriteLine("Card inspect portrait boundary passed: final BigPortrait overwrite repaired with external ownership preserved.");
    }

    private static MethodBase[] Calls(MethodBase method) => PatchProcessor.GetOriginalInstructions(method)
        .Select(instruction => instruction.operand).OfType<MethodBase>().ToArray();

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
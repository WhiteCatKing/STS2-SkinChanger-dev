using System.Reflection;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class JoanStageBindingTests
{
    internal static void Run()
    {
        var cycle = typeof(SkinService).Assembly.GetType("STS2SkinChanger.Ui.CharSelectStageCycle", true)!;
        var loadStage = AccessTools.Method(cycle, "LoadStageDataFile");
        var batch = AccessTools.Method(cycle, "StageBatchPaths");
        var roots = new HashSet<string>();
        for (var stage = 1; stage <= 3; stage++)
        {
            var paths = (string[])batch.Invoke(null, [$"res://joanofarcironcladhpwear/s{stage}_charselect_data.tres"])!;
            Require(paths.Length == 3 && paths.All(path => path.StartsWith("res://joanofarcironcladhpwear/")),
                "Stage graph must retain selected source and all three stages.");
            roots.Add(string.Join("\n", paths));
        }
        Require(roots.Count == 1, "Stage clicks must reuse one graph rather than decode three copies.");
        var loads = Calls(loadStage);
        Require(loads.Any(m => m.DeclaringType == typeof(SkinService) && m.Name == "WithRuntimeResources"),
            "Stage requests must enter the selected-package isolation boundary.");
        Require(!loads.Any(m => m.DeclaringType == typeof(ResourceLoader)),
            "Canonical cache-first stage loading can borrow the other Joan package.");
        var tick = Calls(AccessTools.Method(cycle, "Tick"));
        foreach (var name in new[] { "LoadStageData", "ReapplyPalette", "WearEnabled" })
            Require(tick.Any(m => m.Name == name), "Rebuilt background lost binding: " + name);
        var click = Calls(AccessTools.Method(cycle, "CycleWithLog"));
        foreach (var name in new[] { "LoadStageData", "ReapplyPalette", "RefreshCharacterButtons" })
            Require(click.Any(m => m.Name == name), "Stage click lost a presentation step: " + name);
        Require(cycle.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .SelectMany(Calls).All(m => m.Name != "LoadResourcePack"),
            "Select-screen stage changes must never mount the raw provider PCK.");
        var input = Calls(AccessTools.Method(cycle, "OnClick"));
        Require(input.Any(m => m.Name == "AcceptEvent") && input.Any(m => m.Name == "WearEnabled"),
            "One click must be consumed once and preserve the author's wear toggle.");
        var palette = Calls(AccessTools.Method(cycle, "ReapplyPalette"));
        Require(palette.Any(m => m.Name == "TryGetExecutorContract"), "Palette execution must respect same-contract package resolution.");
        Require(palette.Any(m => m.DeclaringType == typeof(StagePalettePages) && m.Name == "Ensure"),
            "Every stage swap must complete the selected stage's palette page before native ApplyNow.");
        Console.WriteLine("Joan stage boundary passed: selected-package isolation, rebuilt palette, exclusive click and wear toggle.");
    }
    private static MethodInfo[] Calls(MethodInfo method) => PatchProcessor.GetOriginalInstructions(method)
        .Select(i => i.operand).OfType<MethodInfo>().ToArray();
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

internal static class LegacyDefectPresentationTests
{
    private static Assembly pack = null!;
    private static PackedScene scene = null!;
    private static int preloads;
    internal static void Run(string root)
    {
        pack = Assembly.LoadFrom(Path.Combine(root, "DefectSkinMod.dll"));
        var globals = pack.GetType("DefectSkinMod.Scripts.DefectSkinGlobals", true)!;
        var preload = AccessTools.Method(globals, "Preload");
        var callback = LegacyDefectPresentationCompat.BackgroundCallback(pack) ?? throw new Exception("Real background callback missing.");
        var silence = AuditLogSilencer.Install();
        var harmony = new Harmony("sc.tests.legacy-defect");
        scene = (PackedScene)RuntimeHelpers.GetUninitializedObject(typeof(PackedScene));
        GC.SuppressFinalize(scene);
        harmony.Patch(preload, prefix: new HarmonyMethod(typeof(LegacyDefectPresentationTests), nameof(Preload)));
        try
        {
            LegacyDefectPresentationCompat.EnsureScenes(pack);
            LegacyDefectPresentationCompat.EnsureScenes(pack);
            Require(preloads == 1 && ReferenceEquals(globals.GetField("CharacterScene")!.GetValue(null), scene) &&
                ReferenceEquals(globals.GetField("SelectBgScene")!.GetValue(null), scene), "Both native scene fields must initialize exactly once.");
            Require(ReferenceEquals(LegacyDefectPresentationCompat.PreviewScene(pack), scene) &&
                LegacyDefectPresentationCompat.PreviewScene(typeof(CharacterModel).Assembly) == null,
                "Preview actor must use the selected legacy pack's actual CharacterScene, never another provider.");
            var previewCalls = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(FrameworkModelPreview), "TryRefresh"))
                .Select(i => i.operand).OfType<MethodBase>().ToArray();
            Require(previewCalls.Any(m => m.DeclaringType == typeof(LegacyDefectPresentationCompat) && m.Name == "ApplyPreviewBody"),
                "Shared preview creation must apply the legacy actor.");
            var actorCalls = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(LegacyDefectPresentationCompat), "ApplyPreviewBody"))
                .Select(i => i.operand).OfType<MethodBase>().ToArray();
            Require(actorCalls.Any(m => m.Name == "GetSelectedCreatureRuntimeProvider") &&
                actorCalls.Any(m => m.Name == "GetActiveProviderAssembly") &&
                actorCalls.All(m => m.Name is not "ReplaySelectedCreatureNodeReady" and not "UpdateBounds" and not "_Ready"),
                "Preview attachment must retain selection/activation gates and avoid combat lifecycle replay.");
            var screen = (NCharacterSelectScreen)RuntimeHelpers.GetUninitializedObject(typeof(NCharacterSelectScreen));
            var button = (NCharacterSelectButton)RuntimeHelpers.GetUninitializedObject(typeof(NCharacterSelectButton));
            GC.SuppressFinalize(screen); GC.SuppressFinalize(button);
            var character = (CharacterModel)RuntimeHelpers.GetUninitializedObject(typeof(CharacterModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Characters.Defect", true)!);
            var target = AccessTools.Method(typeof(NCharacterSelectScreen), "SelectCharacter");
            Require(target.GetParameters().All(p => p.Name != "characterModel"), "Reproduce the new game signature.");
            object?[] arguments = [callback, target, screen, button, character, null];
            Require((bool)AccessTools.Method(typeof(ManagedSkinModLoader), "TryBuildCharacterPresentationArguments").Invoke(null, arguments)!,
                "Legacy postfix must bind through the existing scoped replay argument adapter.");
            var bound = (object?[])arguments[5]!;
            Require(ReferenceEquals(bound[0], screen) && ReferenceEquals(bound[1], button) && ReferenceEquals(bound[2], character),
                "Replay must receive the selected screen/button/model, not fabricated state.");
            Require(LegacyDefectPresentationCompat.BackgroundCallback(typeof(CharacterModel).Assembly) == null,
                "Other providers must not enter legacy recovery.");
            Console.WriteLine("Legacy Defect recovery passed: actual DLL, two scene fields, single preload and removed-parameter replay binding.");
        }
        finally
        {
            globals.GetField("CharacterScene")!.SetValue(null, null);
            globals.GetField("SelectBgScene")!.SetValue(null, null);
            harmony.UnpatchAll(harmony.Id); silence.UnpatchAll(silence.Id);
        }
    }
    private static bool Preload()
    {
        preloads++;
        var globals = pack.GetType("DefectSkinMod.Scripts.DefectSkinGlobals", true)!;
        globals.GetField("CharacterScene")!.SetValue(null, scene);
        globals.GetField("SelectBgScene")!.SetValue(null, scene);
        return false;
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
}

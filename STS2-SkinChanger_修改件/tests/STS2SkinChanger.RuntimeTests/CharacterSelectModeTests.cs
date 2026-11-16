using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class CharacterSelectModeTests
{
    internal static void Run()
    {
        var harmony = new Harmony("sc.tests.select-modes");
        var silence = AuditLogSilencer.Install();
        try
        {
            CharacterSelectModePatches.Install(harmony);
            foreach (var type in new[] { typeof(NCharacterSelectScreen) }.Concat(CharacterSelectScreenAccess.ModeTypes
                         .Select(name => typeof(NCharacterSelectScreen).Assembly.GetType(name, false)).OfType<Type>()))
            {
                var screen = (Control)RuntimeHelpers.GetUninitializedObject(type); GC.SuppressFinalize(screen);
                if (type.TypeInitializer != null)
                    harmony.Patch(type.TypeInitializer, prefix: new HarmonyMethod(typeof(CharacterSelectModeTests), nameof(SkipNativeStatics)));
                Require(CharacterSelectScreenAccess.IsSupported(screen), "Unrecognized native selection screen: " + type.Name);
                var lobby = (StartRunLobby)RuntimeHelpers.GetUninitializedObject(typeof(StartRunLobby));
                AccessTools.Field(type, "_lobby").SetValue(screen, lobby);
                Require(ReferenceEquals(CharacterSelectScreenAccess.Lobby(screen), lobby), "Read another mode's lobby or initialized gameplay.");
                var background = (Control)RuntimeHelpers.GetUninitializedObject(typeof(Control)); GC.SuppressFinalize(background);
                AccessTools.Field(type, "_bgContainer").SetValue(screen, background);
                Require(ReferenceEquals(CharacterSelectScreenAccess.Background(screen), background),
                    "Nested Dread SubViewport background must be resolved through its real host, preserving the grayscale material.");
                Require(ReferenceEquals(CharacterSelectScreenAccess.Read(AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer"), screen), background),
                    "Protected stage switch must address the actual mode's background field.");
                Require(AccessTools.Method(typeof(ContextualSkinControls), "ShowCharacter").GetParameters()[0].ParameterType.IsInstanceOfType(screen) &&
                        AccessTools.Method(typeof(CharacterModelPreviewControls), "Refresh").GetParameters()[0].ParameterType.IsInstanceOfType(screen) &&
                        AccessTools.Method(typeof(OstyModelPreviewControls), "Refresh").GetParameters()[0].ParameterType.IsInstanceOfType(screen),
                    "A mode cannot use the same selector/model/companion pipeline.");
                if (type == typeof(NCharacterSelectScreen)) continue;
                var selection = Harmony.GetPatchInfo(AccessTools.Method(type, "SelectCharacter"))!;
                Require(selection.Prefixes.Any(patch => patch.PatchMethod.DeclaringType == typeof(CharacterSelectionSkinPatch)) &&
                        selection.Postfixes.Any(patch => patch.PatchMethod.DeclaringType == typeof(CharacterSelectionSkinPatch)),
                    "Native mode SelectCharacter is not attached to the shared pipeline: " + type.Name);
                Require(Harmony.GetPatchInfo(AccessTools.Method(type, "AfterInitialized"))!.Postfixes.Any(patch =>
                    patch.PatchMethod.DeclaringType == typeof(MultiplayerSkinLobbyAttachPatch)), "Mode lobby skin sync is missing.");
                foreach (var method in new[] { "StartNewSingleplayerRun", "StartNewMultiplayerRun" })
                    if (AccessTools.DeclaredMethod(type, method) is { } start)
                        Require(Harmony.GetPatchInfo(start)?.Prefixes.Any(patch => patch.owner == harmony.Id) == true,
                            "Mode embark bypasses selected skin bundle: " + type.Name + "." + method);
            }
            var ordinary = (Control)RuntimeHelpers.GetUninitializedObject(typeof(Control)); GC.SuppressFinalize(ordinary);
            Require(!CharacterSelectScreenAccess.IsSupported(ordinary), "Ordinary panels must not become selection screens.");
            Console.WriteLine("Selection modes passed: actual Standard/Blitz/Dread screen and lobby identity, shared selector/model/companion/stage APIs, native mode selection/embark/sync patches and unrelated-panel exclusion.");
        }
        finally { harmony.UnpatchAll(harmony.Id); silence.UnpatchAll(silence.Id); }
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
    private static bool SkipNativeStatics() => false;
}

using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

internal static class ModePresentationParityTests
{
    private static Control _screen = null!, _host = null!;
    private static CharacterModel _character = null!;
    private static Texture2D _portrait = null!;
    private static int _modeCalls, _portraits;

    internal static void Run(string workshop)
    {
        var waifu = Assembly.LoadFrom(Path.Combine(workshop, "3838977271/Fool_Waifu.dll"));
        var core = Assembly.LoadFrom(Path.Combine(workshop, "3826011578/CznCore.dll"));
        var single = Assembly.LoadFrom(Path.Combine(workshop, "3838976517/FoolSkinStatic.dll"));
        var postfix = AccessTools.Method(waifu.GetType("FoolWaifu.SelectionPatch"), "Postfix");
        var classify = AccessTools.Method(typeof(ManagedSkinModLoader), "IsCharacterPresentationTarget");
        var bind = AccessTools.Method(typeof(ManagedSkinModLoader), "TryBuildCharacterPresentationArguments");
        var nativeRuntimeType = core.GetType("CznCore.Presentation.CharacterSelectPresentationRuntime", true)!;
        var runtime = RuntimeHelpers.GetUninitializedObject(nativeRuntimeType);
        var modeApply = AccessTools.Method(nativeRuntimeType, "ApplyModeBackground");
        var build = IndependentFoolSkinCompat.SelectionBackgroundContract(single)!;
        var patches = build.DeclaringType!;
        var harmony = new Harmony("sc.tests.mode-presentation-parity");
        _host = Fake<Control>(); _portrait = Fake<CompressedTexture2D>();
        try
        {
            harmony.Patch(modeApply, prefix: new HarmonyMethod(typeof(ModePresentationParityTests), nameof(ModeApply)));
            harmony.Patch(AccessTools.Method(patches, "EnsurePortrait"), prefix: new HarmonyMethod(typeof(ModePresentationParityTests), nameof(EnsurePortrait)));
            harmony.Patch(build, prefix: new HarmonyMethod(typeof(ModePresentationParityTests), nameof(BuildPortrait)));
            AccessTools.Field(patches, "_portrait").SetValue(null, _portrait);
            var screens = new[] { typeof(NCharacterSelectScreen) }.Concat(CharacterSelectScreenAccess.ModeTypes
                .Select(n => typeof(CharacterModel).Assembly.GetType(n)).OfType<Type>());
            foreach (var screenType in screens)
            {
                _screen = (Control)RuntimeHelpers.GetUninitializedObject(screenType); GC.SuppressFinalize(_screen);
                var button = Fake<NCharacterSelectButton>();
                foreach (var name in new[] { "Ironclad", "Silent", "Regent", "Necrobinder", "Defect", "Fool" })
                {
                    var characterType = typeof(CharacterModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Characters." + name);
                    if (characterType == null) continue;
                    _character = (CharacterModel)RuntimeHelpers.GetUninitializedObject(characterType);
                    var target = AccessTools.Method(screenType, "SelectCharacter");
                    Require((bool)classify.Invoke(null, [target])!, "Presentation callback omitted: " + screenType.Name);
                    object?[] args = [postfix, target, _screen, button, _character, null];
                    Require((bool)bind.Invoke(null, args)!, "Real Waifu callback cannot bind actual mode screen.");
                    var bound = (object?[])args[5]!;
                    Require(ReferenceEquals(bound[0], _screen) && ReferenceEquals(bound[1], button), "Wrong callback screen/button.");
                    if (screenType != typeof(NCharacterSelectScreen))
                        Require(CznPresentationBridge.ReapplyModeRuntime(runtime, _screen, _host, _character), "Real CZN native mode event not dispatched.");
                }
                Require(IndependentFoolSkinCompat.BuildSelectionBackground(single, _host), "Embedded portrait not dispatched to real author builder.");
            }
            Require(_portraits > 0, "Portrait builder never called.");
            if (CharacterSelectScreenAccess.ModeTypes.Any(n => typeof(CharacterModel).Assembly.GetType(n) != null))
                Require(_modeCalls >= 10, "Other characters did not exercise mode dispatch.");
            Require(IndependentFoolSkinCompat.SelectionBackgroundContract(waifu) == null, "Static adapter claimed unrelated provider.");
            Require(SkinService.GetCharacterSelectIcon("unregistered-content-character", "unknown", false) == null,
                "A character without a skin group must retain its native getter rather than failing overlay lookup.");
            var iconCalls = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(SkinService), "GetCharacterSelectIcon"));
            Require(iconCalls.Any(i => i.operand is MethodBase m && m.Name == "ShouldDeferCharacterIconResourceToExternalRuntime"),
                "Embedded code-provider portraits must retain ownership.");
            var iconWrites = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(ContextualSkinControls), "SetCharacterButtonIcon"));
            Require(iconWrites.Any(i => Equals(i.operand, "%Icon")) && iconWrites.Any(i => Equals(i.operand, "%IconAdd")),
                "Normal and highlighted portrait layers must be synchronized.");
            // Validate the installed shared framework and other character profiles independently
            // of optional gameplay-character Workshop fixtures that may not be subscribed.
            foreach (var spec in new[] { ("3840027648", "HildeFoolSkin", "fool"),
                ("3826012249", "HeidemarieRegentSkin", "regent"), ("3826012071", "TressaSilentSkin", "silent"),
                ("3826011833", "ChizuruIroncladSkin", "ironclad"), ("3789216478", "MeirinWatcherSkin", "watcher") })
                Require(ManagedCharacterAssetReplacementScanner.Scan(Path.Combine(workshop, spec.Item1), spec.Item2)
                    .Any(r => r.TargetGroupId == spec.Item3), "Other character declaration missing: " + spec.Item2);
            if (_modeCalls > 0)
                AccessTools.Method(typeof(CznCompatibilityTests), "CheckCorePatchContract").Invoke(null,
                    [Path.Combine(workshop, "3826011578/CznCore.dll")]);
            Console.WriteLine($"Mode presentation parity passed: real Waifu callback binding, CZN native events for all available characters ({_modeCalls}), static portrait builder, unregistered-character fallback and both icon layers.");
        }
        finally
        {
            AccessTools.Field(patches, "_portrait").SetValue(null, null);
            harmony.UnpatchAll(harmony.Id);
        }
    }
    private static bool ModeApply(object __0)
    {
        var type = __0.GetType();
        Require(ReferenceEquals(AccessTools.Property(type, "Screen").GetValue(__0), _screen) &&
            ReferenceEquals(AccessTools.Property(type, "Host").GetValue(__0), _host) &&
            ReferenceEquals(AccessTools.Property(type, "Character").GetValue(__0), _character) &&
            Equals(AccessTools.Property(type, "ShowMenu").GetValue(__0), true), "Mode event lost its real host/character/menu.");
        _modeCalls++; return false;
    }
    private static bool EnsurePortrait() => false;
    private static bool BuildPortrait(Node __0) { Require(ReferenceEquals(__0, _host), "Wrong portrait host."); _portraits++; return false; }
    private static T Fake<T>() where T : class
    { var item = (T)RuntimeHelpers.GetUninitializedObject(typeof(T)); GC.SuppressFinalize(item); return item; }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

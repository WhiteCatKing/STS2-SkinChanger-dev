using System.Reflection;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Ui;

// 极速/恐惧模式的选人背景：这两套界面会在 SelectCharacter 之外自己重写背景容器
// （Dread 的连胜动画直接调 SetSelectedCharacterVisuals）。这里验证切换器确实给这个入口
// 挂上了「只重做背景」的 Postfix，而且方式对正式版/测试版都安全（类型不存在就跳过）。
internal static class CharacterSelectModeBackgroundTests
{
    internal static void Run()
    {
        VerifyRebuiltBackgroundHook();
        VerifyDreadClickPassthroughPolicy();

        var dread = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.Dread.NDreadScreen");
        if (dread == null)
        {
            Console.WriteLine("Dread 选人界面不存在，跳过（旧版本游戏）。");
            return;
        }

        var helper = dread.GetMethod(
            "SetSelectedCharacterVisuals",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        Require(helper != null, "Dread 选人界面应有自己的 SetSelectedCharacterVisuals 背景入口。");
        Require(helper!.GetParameters().Length == 1 &&
                helper.GetParameters()[0].ParameterType == typeof(MegaCrit.Sts2.Core.Models.CharacterModel),
            "SetSelectedCharacterVisuals 应只接收一个 CharacterModel。");

        var harmony = new Harmony("STS2SkinChanger.test.mode-background");
        try
        {
            CharacterSelectModePatches.Install(harmony);
            var info = Harmony.GetPatchInfo(helper);
            var patched = info?.Postfixes.Any(patch =>
                patch.PatchMethod.DeclaringType == typeof(CharacterSelectModeBackgroundPatch) &&
                patch.PatchMethod.Name == nameof(CharacterSelectModeBackgroundPatch.Postfix)) == true;
            Require(patched, "Dread 的背景入口必须挂上 CharacterSelectModeBackgroundPatch.Postfix。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }

        Console.WriteLine("Character select mode background patch passed.");
    }

    private static void VerifyRebuiltBackgroundHook()
    {
        Require(ProviderModeSelectOverlayBridge.IsSuccubusProvider("succubusIroncladHpWear") &&
                ProviderModeSelectOverlayBridge.IsSuccubusProvider("succubusIroncladHpWear::source:local"),
            "The rebuilt background should accept the base and source-scoped Succubus provider IDs.");
        Require(!ProviderModeSelectOverlayBridge.IsSuccubusProvider(null) &&
                !ProviderModeSelectOverlayBridge.IsSuccubusProvider("JoanOfArcIroncladHpwear") &&
                !ProviderModeSelectOverlayBridge.IsSuccubusProvider("succubusIroncladHpWearExtra"),
            "The rebuilt background hook must leave other selections alone.");

        var replace = AccessTools.DeclaredMethod(typeof(ContextualSkinControls), "ReplaceCharacterBackground");
        var instructions = PatchProcessor.GetOriginalInstructions(replace);
        var add = instructions.FindIndex(instruction => instruction.operand is MethodInfo method &&
            method.Name == "AddChildSafely");
        var attach = instructions.FindIndex(instruction => instruction.operand is MethodInfo method &&
            method.DeclaringType == typeof(ProviderModeSelectOverlayBridge) &&
            method.Name == nameof(ProviderModeSelectOverlayBridge.AttachRebuiltBackground));
        Require(add >= 0 && attach > add,
            "The provider must be attached after adding the rebuilt background to the tree.");
    }

    private static void VerifyDreadClickPassthroughPolicy()
    {
        const string dreadName = "MegaCrit.Sts2.Core.Nodes.Screens.Dread.NDreadScreen";
        const string blitzName = "MegaCrit.Sts2.Core.Nodes.Screens.Blitz.NBlitzScreen";
        const string standardName = "MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen";

        // 1. 仅限恐惧/极速模式界面，且背景位于 SubViewport 内且存在启用中的点击热区时才放通遮罩。
        Require(ProviderModeSelectOverlayBridge.ShouldAllowOverlayClickPassthrough(dreadName, true, true),
            "Dread screen with SubViewport background and active catcher must enable click passthrough.");
        Require(!ProviderModeSelectOverlayBridge.ShouldAllowOverlayClickPassthrough(dreadName, true, false),
            "Dread screen without an active catcher must restore original overlay MouseFilter.");
        Require(!ProviderModeSelectOverlayBridge.ShouldAllowOverlayClickPassthrough(blitzName, false, true),
            "Blitz screen (no SubViewport) must not touch screen-level overlays.");
        Require(!ProviderModeSelectOverlayBridge.ShouldAllowOverlayClickPassthrough(standardName, true, true),
            "Standard character select screen must never be touched by mode click passthrough.");

        // 2. 热区识别：覆盖魅魔、贞德、切换器统一战损热区，不误认原版节点。
        Require(ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("SuccubusSkinPreviewClick") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("JoanofArcIroncladHpwearClickCatcher") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("SkinChangerCharSelectStageCatcher"),
            "Known background click catchers must be recognized.");
        Require(!ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName(null) &&
                !ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("IRONCLAD_bg") &&
                !ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("GrayScaler") &&
                !ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("BigRotatingContainer"),
            "Vanilla background and overlay nodes must not be misidentified as click catchers.");

        // 3. 只穿透非交互视觉遮罩（GrayScaler/BigRotatingContainer/RotatingImage），绝不穿透按钮等交互控件。
        Require(ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(ColorRect)) &&
                ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(Control)) &&
                ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(TextureRect)),
            "Visual overlay controls (ColorRect/Control/TextureRect) must be eligible for passthrough.");
        Require(!ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(Button)) &&
                !ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(CheckBox)) &&
                !ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(LineEdit)) &&
                !ProviderModeSelectOverlayBridge.IsBlockingVisualOverlayType(typeof(Node2D)),
            "Interactive controls and non-Control nodes must never be modified.");

        // 4. 验证关键入口均会在完成后同步点击穿透状态（包括切走恢复）。
        RequireCallsMethod(typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.Attach),
            typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough));
        RequireCallsMethod(typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.AttachRebuiltBackground),
            typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough));
        RequireCallsMethod(typeof(CharacterSelectModeBackgroundPatch), nameof(CharacterSelectModeBackgroundPatch.Postfix),
            typeof(CharSelectStageCycle), nameof(CharSelectStageCycle.Attach));
        RequireCallsMethod(typeof(ContextualSkinControls), "RebuildCharacterDisplay",
            typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough));
    }

    private static void RequireCallsMethod(Type callerType, string callerName, Type calleeType, string calleeName)
    {
        var method = AccessTools.DeclaredMethod(callerType, callerName);
        Require(method != null, $"Missing method {callerType.Name}.{callerName}.");
        var instructions = PatchProcessor.GetOriginalInstructions(method);
        var called = instructions.Any(instruction =>
            instruction.operand is MethodInfo target &&
            target.DeclaringType == calleeType &&
            target.Name == calleeName);
        Require(called, $"{callerType.Name}.{callerName} must call {calleeType.Name}.{calleeName}.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}

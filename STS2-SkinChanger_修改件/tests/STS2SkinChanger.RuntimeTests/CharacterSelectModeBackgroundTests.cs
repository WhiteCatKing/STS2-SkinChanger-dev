using System.Reflection;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Ui;
using STS2SkinChanger.Core;

// 极速/恐惧模式的选人背景：这两套界面会在 SelectCharacter 之外自己重写背景容器
// （Dread 的连胜动画直接调 SetSelectedCharacterVisuals）。这里验证切换器确实给这个入口
// 挂上了「只重做背景」的 Postfix，而且方式对正式版/测试版都安全（类型不存在就跳过）。
internal static class CharacterSelectModeBackgroundTests
{
    internal static void Run()
    {
        VerifyRebuiltBackgroundHook();
        VerifyGenericProviderAttachDiscovery();
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

        // 2. 热区识别：覆盖战损框架 DBWCharSelectClickCatcher、魅魔、贞德、观者、弩手与切换器统一战损热区，不误认原版节点。
        Require(ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("DBWCharSelectClickCatcher") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("SuccubusSkinPreviewClick") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("JoanofArcIroncladHpwearClickCatcher") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("SkinChangerCharSelectStageCatcher") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("WatcherHpWearClick") &&
                ProviderModeSelectOverlayBridge.IsBackgroundClickCatcherName("CrossbowFoolClick"),
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

        var rebuildCalls = PatchProcessor.GetOriginalInstructions(
            AccessTools.DeclaredMethod(typeof(ContextualSkinControls), "RebuildCharacterDisplay"))
            .Select(instruction => (instruction.operand as MethodInfo)?.Name).ToList();
        var syncVariant = rebuildCalls.IndexOf(nameof(ManualCharacterVariantBridge.ApplyLocalChoice));
        var backgroundLoad = rebuildCalls.IndexOf("RebuildMountedFullRuntimeCharacterDisplay");
        Require(syncVariant >= 0 && backgroundLoad > syncVariant,
            "The saved character variant must apply before the background resource scope captures the provider flag.");
        Require(!PatchProcessor.GetOriginalInstructions(
            AccessTools.DeclaredMethod(typeof(ContextualSkinControls), "ApplyOstySelection"))
            .Any(instruction => instruction.operand is MethodInfo method && method.Name == "RefreshSelectorDisplay"),
            "An independent Osty choice must not rebuild the character backdrop.");


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

    private static void VerifyGenericProviderAttachDiscovery()
    {
        // 1. 覆盖双参数 BattleWear.CharSelectCore.Attach(Control, CsProfile) 形态：
        //    necrobinderHpWear（静态字段 + NotifySelectedCharacter）、ironcladHpWear（多角色字段按当前角色精确匹配）、
        //    regentHpWear（ProfileFor(string) + NotifySelectedCharacter + Dead 复位）。
        MockBattleWearCharSelectCore.Reset();
        MockBattleWearCharSelectCore.Necrobinder.Dead = true;
        var necroBindings = ProviderModeSelectOverlayBridge.DiscoverBackgroundAttachBindings(
            [typeof(MockBattleWearCharSelectCore)],
            "necrobinder");
        Require(necroBindings.Count == 1 && necroBindings[0].NotifySelectedCharacterMethod != null,
            "应通过通用反射发现双参数 Attach(Control, Profile) 及 NotifySelectedCharacter。");
        Require(ProviderModeSelectOverlayBridge.InvokeBinding(necroBindings[0], null, "necrobinder"),
            "调用双参数绑定应成功执行。");
        Require(MockBattleWearCharSelectCore.SelectedId == "necrobinder" &&
                ReferenceEquals(MockBattleWearCharSelectCore.LastAttachedProfile, MockBattleWearCharSelectCore.Necrobinder) &&
                !MockBattleWearCharSelectCore.Necrobinder.Dead,
            "调用前必须同步 NotifySelectedCharacter 并复位 Profile.Dead，再绑定对应角色的 Profile。");

        MockBattleWearCharSelectCore.Reset();
        var ironBindings = ProviderModeSelectOverlayBridge.DiscoverBackgroundAttachBindings(
            [typeof(MockBattleWearCharSelectCore)],
            "ironclad");
        Require(ironBindings.Count == 1 &&
                ProviderModeSelectOverlayBridge.InvokeBinding(ironBindings[0], null, "ironclad") &&
                ReferenceEquals(MockBattleWearCharSelectCore.LastAttachedProfile, MockBattleWearCharSelectCore.Ironclad),
            "多角色静态字段存在时必须精确匹配当前角色（ironclad），不得串到其它角色 Profile。");

        MockRegentCharSelectCore.Reset();
        MockRegentCharSelectCore.Regent.Dead = true;
        var regentBindings = ProviderModeSelectOverlayBridge.DiscoverBackgroundAttachBindings(
            [typeof(MockRegentCharSelectCore)],
            "regent");
        Require(regentBindings.Count == 1 &&
                ProviderModeSelectOverlayBridge.InvokeBinding(regentBindings[0], null, "regent") &&
                MockRegentCharSelectCore.SelectedId == "regent" &&
                ReferenceEquals(MockRegentCharSelectCore.LastAttachedProfile, MockRegentCharSelectCore.Regent) &&
                !MockRegentCharSelectCore.Regent.Dead,
            "带 ProfileFor(string) 的战损框架（如 regentHpWear）必须通过 ProfileFor 解析并同步选中角色与复位 Dead。");

        // 阴性对照：不拥有该角色的 CharSelectCore 不应产出绑定。
        var unmatched = ProviderModeSelectOverlayBridge.DiscoverBackgroundAttachBindings(
            [typeof(MockRegentCharSelectCore)],
            "silent");
        Require(unmatched.Count == 0,
            "当前角色与该包任何 Profile 都不匹配时，不应误挂 Attach。");

        // 2. 覆盖单参数 Attach(Control)（魅魔/贞德）与 AttachBackground(Control)（观者）。
        MockSingleArgOverlay.AttachCount = 0;
        MockSingleArgOverlay.AttachBackgroundCount = 0;
        var singleBindings = ProviderModeSelectOverlayBridge.DiscoverBackgroundAttachBindings(
            [typeof(MockSingleArgOverlay)],
            "ironclad");
        Require(singleBindings.Count == 2,
            "单参数 Attach(Control) 与 AttachBackground(Control) 均应被通用发现，非 void 工厂方法应被忽略。");
        foreach (var binding in singleBindings)
        {
            ProviderModeSelectOverlayBridge.InvokeBinding(binding, null, "ironclad");
        }

        Require(MockSingleArgOverlay.AttachCount == 1 && MockSingleArgOverlay.AttachBackgroundCount == 1,
            "单参数 Attach 与 AttachBackground 绑定应被各调用一次。");

        // 3. 标准模式保护：Attach 与 AttachRebuiltBackground 均校验 IsModeScreenTypeName。
        RequireCallsMethod(typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.Attach),
            typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.IsModeScreenTypeName));
        RequireCallsMethod(typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.AttachRebuiltBackground),
            typeof(ProviderModeSelectOverlayBridge), nameof(ProviderModeSelectOverlayBridge.IsModeScreenTypeName));

        // 4. #207 回归：标准模式狐猎重挂必须大小写无关匹配 SILENT_bg（Godot GetNodeOrNull 区分大小写）。
        Require(STS2SkinChanger.Core.CharSelectPackRebindCompat.IsCharacterBackgroundNodeName("SILENT_bg", "silent") &&
                STS2SkinChanger.Core.CharSelectPackRebindCompat.IsCharacterBackgroundNodeName("silent_bg", "silent") &&
                !STS2SkinChanger.Core.CharSelectPackRebindCompat.IsCharacterBackgroundNodeName("IRONCLAD_bg", "silent") &&
                !STS2SkinChanger.Core.CharSelectPackRebindCompat.IsCharacterBackgroundNodeName("SILENT", "silent") &&
                !STS2SkinChanger.Core.CharSelectPackRebindCompat.IsCharacterBackgroundNodeName(null, "silent"),
            "CharSelectPackRebindCompat 必须能大小写无关识别 SILENT_bg / silent_bg 节点名。");
        RequireCallsMethod(typeof(STS2SkinChanger.Core.CharSelectPackRebindCompat), "AfterRebuild",
            typeof(STS2SkinChanger.Core.CharSelectPackRebindCompat), "FindCharacterBackground");

        // 5. #211 回归：模式界面同帧多次重挂合并为最终背景一次、旧背景销毁前停掉输入轮询器、
        //    以及跨界面隐藏时复位 WasPressed/IgnoreClickUntilRelease 防止静态单例状态被窃取。
        Require(ProviderModeSelectOverlayBridge.ShouldRunDeferredAttach(5, 5, screenValid: true, screenVisible: true) &&
                !ProviderModeSelectOverlayBridge.ShouldRunDeferredAttach(4, 5, screenValid: true, screenVisible: true) &&
                !ProviderModeSelectOverlayBridge.ShouldRunDeferredAttach(5, 5, screenValid: true, screenVisible: false) &&
                !ProviderModeSelectOverlayBridge.ShouldRunDeferredAttach(5, 5, screenValid: false, screenVisible: true),
            "模式界面延后挂接必须仅在代次最新且界面有效可见时执行，避免旧背景重复挂接。");

        Require(ProviderModeSelectOverlayBridge.IsProviderInputPollerName("DBWInputPoller") &&
                ProviderModeSelectOverlayBridge.IsProviderInputPollerName("JoanofArcIroncladHpwearPoller") &&
                !ProviderModeSelectOverlayBridge.IsProviderInputPollerName("WearSafetyPoller") &&
                !ProviderModeSelectOverlayBridge.IsProviderInputPollerName("SkinChangerCrossbowSelect") &&
                !ProviderModeSelectOverlayBridge.IsProviderInputPollerName(null),
            "输入轮询器识别必须精准命中 DBWInputPoller 等提供者点击轮询器，且不误伤安全/其他 Timer。");

        var stuckProfile = new MockWearProfile("silent")
        {
            Dead = true,
            WasPressed = true,
            IgnoreClickUntilRelease = true
        };
        Require(ProviderModeSelectOverlayBridge.ResetProfileInputState(stuckProfile) &&
                !stuckProfile.Dead &&
                !stuckProfile.WasPressed &&
                !stuckProfile.IgnoreClickUntilRelease,
            "重挂前必须同时复位 Profile.Dead、WasPressed 与 IgnoreClickUntilRelease，防止跨界面轮询卡死。");

        var replaceBg = AccessTools.DeclaredMethod(typeof(ContextualSkinControls), "ReplaceCharacterBackground");
        var replaceIl = PatchProcessor.GetOriginalInstructions(replaceBg);
        var stopIdx = replaceIl.FindIndex(inst => inst.operand is MethodInfo m &&
            m.DeclaringType == typeof(ProviderModeSelectOverlayBridge) &&
            m.Name == nameof(ProviderModeSelectOverlayBridge.StopBackgroundPollers));
        var removeIdx = replaceIl.FindIndex(inst => inst.operand is MethodInfo m &&
            m.Name == "RemoveChildSafely");
        Require(stopIdx >= 0 && removeIdx > stopIdx,
            "ReplaceCharacterBackground 移除旧背景前必须先同步停用旧背景上的输入轮询器。");
    }

    private sealed class MockWearProfile(string name)
    {
        public string Name = name;
        public bool Dead;
        public bool WasPressed;
        public bool IgnoreClickUntilRelease;
    }

    private static class MockBattleWearCharSelectCore
    {
        public static readonly MockWearProfile Ironclad = new("ironclad");
        public static readonly MockWearProfile Necrobinder = new("necrobinder");
        public static string? SelectedId;
        public static MockWearProfile? LastAttachedProfile;

        public static void Reset()
        {
            SelectedId = null;
            LastAttachedProfile = null;
            Ironclad.Dead = false;
            Necrobinder.Dead = false;
        }

        private static void NotifySelectedCharacter(string id) => SelectedId = id;
        private static void Attach(Control? bg, MockWearProfile profile) => LastAttachedProfile = profile;
    }

    private static class MockRegentCharSelectCore
    {
        public static readonly MockWearProfile Regent = new("regent");
        public static string? SelectedId;
        public static MockWearProfile? LastAttachedProfile;

        public static void Reset()
        {
            SelectedId = null;
            LastAttachedProfile = null;
            Regent.Dead = false;
        }

        private static void NotifySelectedCharacter(string id) => SelectedId = id;
        private static MockWearProfile? ProfileFor(string id) =>
            id.Equals("regent", StringComparison.OrdinalIgnoreCase) ? Regent : null;
        private static void Attach(Control? bg, MockWearProfile profile) => LastAttachedProfile = profile;
    }

    private static class MockSingleArgOverlay
    {
        public static int AttachCount;
        public static int AttachBackgroundCount;

        private static void Attach(Control? bg) => AttachCount++;
        private static void AttachBackground(Control? bg) => AttachBackgroundCount++;
        private static Control? Attach(Control? bg, int ignored) => bg;
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}

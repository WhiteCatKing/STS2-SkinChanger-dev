using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class UiInjectionGuardTests
{
    private static readonly string[] LifecycleTargets =
    [
        "_Ready",
        "_EnterTree",
        "OnSubmenuOpened",
        "OnSubmenuClosed"
    ];

    private static readonly HashSet<string> AuditedScreenPatches = new(StringComparer.Ordinal)
    {
        "LoadOrderWarningModalClosedPatch",
        "RemoteLobbyPlayerVisualRefreshScopePatch",
        "CharacterSelectionSkinPatch",
        "BestiarySelectionSkinPatch",
        "CardLibraryPoolSkinPatch",
        "ContinueRunPortraitPatch"
    };

    private static readonly HashSet<string> ExcludedNonUiInjectionLifecyclePatches = new(StringComparer.Ordinal)
    {
        // Card pipeline exception finalizer itself (no Prefix/Postfix; guards NCard lifecycle).
        "CardPipelineExceptionBoundary",
        // Guarded by CardPipelineExceptionBoundary (third-party exceptions swallowed, own exceptions bubble).
        "CardTreeReentrySkinPatch",
        // Guarded by CombatAppearanceBoundary / in-run creature, HUD & map visuals.
        "CharacterAppearanceCreatureReadyPatch",
        "InRunCharacterAppearanceRuntimePatch",
        "BossMapPointSkinPresentationPatch",
        "FakeMerchantBattleFigureReadyPatch",
        "ManagedCharacterAnimationReadyPatch",
        "NecrobinderOptionalHeadReadyPatch",
        "FrameworkEnergyLayersPatch",
        // Multiplayer in-run node scope leases (Prefix/Finalizer pairs, non-UI injection).
        "MultiplayerCreatureReadyScopePatch",
        "MultiplayerRestSiteReadyScopePatch",
        "MultiplayerTreasureHandReadyScopePatch",
        // Merchant / rest site scene scope, frame scheduler & preview suppression patches (non-UI injection).
        "MerchantRoomCreateAppearancePatch",
        "MerchantRoomPlayerAppearancePatch",
        "MerchantRoomPreviewEnterTreePatch",
        "FakeMerchantAppearancePatch",
        "FakeMerchantEventFramePatch",
        "FakeMerchantPreviewEnterTreePatch",
        "RestSitePlayerAppearancePatch",
        "MerchantButtonProviderReadyBridgePatch",
        "MerchantInventoryProviderReadyBridgePatch",
        "MerchantHandProviderReadyBridgePatch",
        "MerchantPreviewProceedReadyPatch",
        // Startup diagnostics & self-check schedulers on NMainMenu._Ready (non-UI injection; internally guarded).
        "GlobalWriteSummaryPatch",
        "VanillaIntegrityMainMenuReadyPatch",
        // Non-UI random skin state restore; has own try/catch and RandomCharacterSkinTests inspects Prefix IL.
        "RandomCharacterLobbySkinPatch"
    };

    private static readonly (Type DeclaringType, string MethodName, string TargetMethodName)[] ProgrammaticUiLifecycleHooks =
    [
        // FrameworkRegistryCooperation.Bind patches thunninoiSkinManager's SkinSelector._Ready programmatically.
        (typeof(FrameworkRegistryCooperation), "TrackControl", "_Ready")
    ];

    private static bool _downstreamRan;

    internal static void Run()
    {
        VerifyExceptionIsolationAndCounting();
        VerifyHarmonyPostfixChainSurvivesFailure();
        VerifyRunWhenInTreeOfflineContract();
        var (classCount, hookCount, programmaticCount) = VerifyAllUiInjectionPatchesGuarded();
        Console.WriteLine(
            $"UI injection guard passed: {classCount} patch classes ({hookCount} hooks) + {programmaticCount} programmatic UI hooks verified, exception isolation & postfix chain preserved.");
    }

    private static void VerifyExceptionIsolationAndCounting()
    {
        var logs = new List<string>();
        try
        {
            UiInjectionGuard.ResetForTests();
            UiInjectionGuard.ErrorLoggerForTests = logs.Add;

            var executed = false;
            UiInjectionGuard.Run("test.ok", () => executed = true);
            Require(executed && logs.Count == 0 && UiInjectionGuard.GetFailureCount("test.ok") == 0,
                "正常执行的注入动作不应记录失败。");

            UiInjectionGuard.Run("test.ready", () => throw new InvalidOperationException("boom1"));
            Require(logs.Count == 1,
                $"同一 point 首次失败应记录 1 条日志，实际：{logs.Count}。");
            Require(logs[0].StartsWith("[界面注入] test.ready 失败: InvalidOperationException: boom1", StringComparison.Ordinal),
                $"首错日志格式不符合约定：{logs[0]}");
            Require(logs[0].Contains(nameof(VerifyExceptionIsolationAndCounting), StringComparison.Ordinal) &&
                    logs[0].Contains('\n'),
                $"首错日志必须包含完整异常堆栈：{logs[0]}");
            Require(UiInjectionGuard.GetFailureCount("test.ready") == 1,
                "首次失败后计数应为 1。");

            UiInjectionGuard.Run("test.ready", () => throw new NullReferenceException("boom2"));
            UiInjectionGuard.Run("test.ready", () => throw new NullReferenceException("boom3"));
            Require(logs.Count == 1,
                $"同一 point 后续失败只累计次数、不得重复输出堆栈，实际日志数：{logs.Count}。");
            Require(UiInjectionGuard.GetFailureCount("test.ready") == 3,
                $"test.ready 累计失败次数应为 3，实际：{UiInjectionGuard.GetFailureCount("test.ready")}。");

            UiInjectionGuard.Run("test.other", () => throw new ArgumentException("other"));
            Require(logs.Count == 2 &&
                    logs[1].StartsWith("[界面注入] test.other 失败: ArgumentException: other", StringComparison.Ordinal),
                "不同 point 的首次失败必须各自记录一次完整堆栈。");
            Require(UiInjectionGuard.GetFailureCount("test.other") == 1 &&
                    UiInjectionGuard.GetFailureCount("test.ready") == 3,
                "不同 point 的失败计数必须相互独立。");

            var firstBool = UiInjectionGuard.Run("test.bool", () => throw new FormatException("bad"), fallback: true);
            var secondBool = UiInjectionGuard.Run("test.bool", () => throw new FormatException("bad2"), fallback: true);
            Require(firstBool && secondBool && logs.Count == 3 && UiInjectionGuard.GetFailureCount("test.bool") == 2,
                "带返回值的 Prefix 失败时应回退默认值且同样按 point 去重记日志并累计次数。");
        }
        finally
        {
            UiInjectionGuard.ResetForTests();
        }
    }

    private static void VerifyHarmonyPostfixChainSurvivesFailure()
    {
        var harmony = new Harmony("Gurio.SkinChanger.Tests.UiInjectionGuardChain");
        var logs = new List<string>();
        try
        {
            UiInjectionGuard.ResetForTests();
            UiInjectionGuard.ErrorLoggerForTests = logs.Add;
            _downstreamRan = false;

            var target = AccessTools.Method(typeof(UiInjectionGuardTests), nameof(DummyLifecycleTarget)) ??
                         throw new InvalidOperationException("找不到 DummyLifecycleTarget。");
            harmony.Patch(
                target,
                postfix: new HarmonyMethod(typeof(UiInjectionGuardTests), nameof(ThrowingGuardedPostfix))
                {
                    priority = Priority.First
                });
            harmony.Patch(
                target,
                postfix: new HarmonyMethod(typeof(UiInjectionGuardTests), nameof(DownstreamThirdPartyPostfix))
                {
                    priority = Priority.Last
                });

            DummyLifecycleTarget();

            Require(_downstreamRan,
                "前一个 UI 注入 Postfix 抛错后，同链后续 Mod 的 Postfix 必须照常执行。");
            Require(logs.Count == 1 && UiInjectionGuard.GetFailureCount("test.chain") == 1,
                "Postfix 链异常应被隔离记录一次并累计计数。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            UiInjectionGuard.ResetForTests();
            _downstreamRan = false;
        }
    }

    private static void VerifyRunWhenInTreeOfflineContract()
    {
        UiInjectionGuard.ResetForTests();
        UiInjectionGuard.RunWhenInTree(null, "test.null", () => throw new InvalidOperationException("unreachable"));
        Require(UiInjectionGuard.GetFailureCount("test.null") == 0,
            "传入 null Node 时 RunWhenInTree 不应触发异常。");

        var guardMethods = new List<MethodBase>
        {
            AccessTools.Method(typeof(UiInjectionGuard), nameof(UiInjectionGuard.RunWhenInTree)) ??
            throw new InvalidOperationException("找不到 UiInjectionGuard.RunWhenInTree。")
        };
        foreach (var nested in typeof(UiInjectionGuard).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
        {
            guardMethods.AddRange(nested
                .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(m => m.GetMethodBody() != null));
        }

        var calledMethods = guardMethods
            .SelectMany(method => PatchProcessor.GetOriginalInstructions(method))
            .Select(instruction => instruction.operand)
            .OfType<MethodInfo>()
            .ToArray();

        Require(calledMethods.Any(m => m.DeclaringType == typeof(Node) && m.Name == nameof(Node.IsInsideTree)),
            "RunWhenInTree 必须检查 Node.IsInsideTree()。");
        Require(calledMethods.Any(m => m.DeclaringType == typeof(Node) && m.Name == "add_TreeEntered"),
            "节点未入树时 RunWhenInTree 必须订阅 Node.TreeEntered。");
        Require(calledMethods.Any(m => m.DeclaringType == typeof(Node) && m.Name == "remove_TreeEntered"),
            "RunWhenInTree 的 TreeEntered 回调必须是一次性订阅（触发后退订）。");
        Require(calledMethods.Any(m => m.DeclaringType == typeof(UiInjectionGuard) && m.Name == nameof(UiInjectionGuard.Run)),
            "RunWhenInTree 必须统一委托 UiInjectionGuard.Run 执行并隔离异常。");
    }

    private static (int ClassCount, int HookCount, int ProgrammaticHookCount) VerifyAllUiInjectionPatchesGuarded()
    {
        // Scan the ENTIRE assembly (all namespaces) so lifecycle and UI-callback patches
        // placed under Core / Core.Features.* cannot bypass the guard gate.
        var allPatchTypes = typeof(Entry).Assembly.GetTypes()
            .Where(type => type.IsClass &&
                           type.IsDefined(typeof(HarmonyPatch), inherit: false))
            .OrderBy(type => type.Name, StringComparer.Ordinal)
            .ToArray();

        var discoveredLifecycleClasses = new HashSet<string>(StringComparer.Ordinal);
        var guardedClasses = new List<Type>();

        foreach (var patchType in allPatchTypes)
        {
            var targets = ResolveTargetMethodNames(patchType);
            var isLifecyclePatch = targets.Any(name => LifecycleTargets.Contains(name, StringComparer.Ordinal));
            if (isLifecyclePatch)
            {
                discoveredLifecycleClasses.Add(patchType.Name);
            }

            if (ExcludedNonUiInjectionLifecyclePatches.Contains(patchType.Name))
            {
                continue;
            }

            if (isLifecyclePatch || AuditedScreenPatches.Contains(patchType.Name))
            {
                guardedClasses.Add(patchType);
            }
        }

        var staleExclusions = ExcludedNonUiInjectionLifecyclePatches
            .Where(name => !discoveredLifecycleClasses.Contains(name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Require(staleExclusions.Length == 0,
            $"ExcludedNonUiInjectionLifecyclePatches 存在未命中的陈旧项：{string.Join(", ", staleExclusions)}。");

        var missingAudited = AuditedScreenPatches
            .Where(name => guardedClasses.All(type => type.Name != name))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        Require(missingAudited.Length == 0,
            $"AuditedScreenPatches 中未找到补丁类：{string.Join(", ", missingAudited)}。");

        var hookCount = 0;
        var unguardedHooks = new List<string>();

        foreach (var patchType in guardedClasses)
        {
            var hooks = patchType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(IsHarmonyHookMethod)
                .ToArray();
            Require(hooks.Length > 0, $"UI 注入补丁类 {patchType.Name} 找不到 Prefix/Postfix 钩子方法。");

            foreach (var hook in hooks)
            {
                hookCount++;
                var callsGuard = PatchProcessor.GetOriginalInstructions(hook)
                    .Select(instruction => instruction.operand)
                    .OfType<MethodInfo>()
                    .Any(method => method.DeclaringType == typeof(UiInjectionGuard) &&
                                   (method.Name == nameof(UiInjectionGuard.Run) ||
                                    method.Name == nameof(UiInjectionGuard.RunWhenInTree)));
                if (!callsGuard)
                {
                    unguardedHooks.Add($"{patchType.Name}.{hook.Name}");
                }
            }
        }

        foreach (var (ownerType, methodName, targetName) in ProgrammaticUiLifecycleHooks)
        {
            var hook = ownerType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                       ?? throw new InvalidOperationException($"找不到程序化注册的 UI 生命周期钩子 {ownerType.Name}.{methodName}。");
            var callsGuard = PatchProcessor.GetOriginalInstructions(hook)
                .Select(instruction => instruction.operand)
                .OfType<MethodInfo>()
                .Any(method => method.DeclaringType == typeof(UiInjectionGuard) &&
                               (method.Name == nameof(UiInjectionGuard.Run) ||
                                method.Name == nameof(UiInjectionGuard.RunWhenInTree)));
            if (!callsGuard)
            {
                unguardedHooks.Add($"{ownerType.Name}.{methodName} ({targetName})");
            }
        }

        // Verify FrameworkRegistryCooperation.Bind still registers the documented programmatic _Ready hook.
        var bindMethod = AccessTools.Method(typeof(FrameworkRegistryCooperation), nameof(FrameworkRegistryCooperation.Bind))
                         ?? throw new InvalidOperationException("找不到 FrameworkRegistryCooperation.Bind。");
        var bindStrings = PatchProcessor.GetOriginalInstructions(bindMethod)
            .Select(instruction => instruction.operand)
            .OfType<string>()
            .ToArray();
        Require(bindStrings.Contains("_Ready", StringComparer.Ordinal) &&
                bindStrings.Contains("TrackControl", StringComparer.Ordinal),
            "FrameworkRegistryCooperation.Bind 应保留已登记的 SkinSelector._Ready -> TrackControl 程序化钩子。");

        Require(unguardedHooks.Count == 0,
            $"以下 UI 注入补丁未调用 UiInjectionGuard.Run/RunWhenInTree：{string.Join(", ", unguardedHooks)}。");
        Require(guardedClasses.Count == 28 && hookCount == 34 && ProgrammaticUiLifecycleHooks.Length == 1,
            $"已审计 UI 注入补丁预期为 28 类 / 34 钩子 + 1 程序化钩子，实际 {guardedClasses.Count} 类 / {hookCount} 钩子 + {ProgrammaticUiLifecycleHooks.Length} 程序化钩子（新增项须接入 guard 或显式说明原因）。");

        return (guardedClasses.Count, hookCount, ProgrammaticUiLifecycleHooks.Length);
    }

    private static HashSet<string> ResolveTargetMethodNames(Type patchType)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var attr in patchType.GetCustomAttributes<HarmonyPatch>(inherit: false))
        {
            if (!string.IsNullOrWhiteSpace(attr.info.methodName))
            {
                names.Add(attr.info.methodName);
            }
        }

        foreach (var helperName in new[] { "TargetMethod", "TargetMethods" })
        {
            var helper = patchType.GetMethod(helperName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            if (helper == null)
            {
                continue;
            }

            try
            {
                var result = helper.Invoke(null, null);
                if (result is MethodBase single)
                {
                    names.Add(single.Name);
                }
                else if (result is System.Collections.IEnumerable many)
                {
                    foreach (var item in many)
                    {
                        if (item is MethodBase m)
                        {
                            names.Add(m.Name);
                        }
                    }
                }
            }
            catch
            {
                // Optional target methods absent on one branch do not break scanning.
            }
        }

        return names;
    }

    private static bool IsHarmonyHookMethod(MethodInfo method) =>
        method.Name == "Prefix" ||
        method.Name == "Postfix" ||
        method.IsDefined(typeof(HarmonyPrefix), inherit: false) ||
        method.IsDefined(typeof(HarmonyPostfix), inherit: false);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void DummyLifecycleTarget()
    {
    }

    [HarmonyPriority(Priority.First)]
    private static void ThrowingGuardedPostfix() =>
        UiInjectionGuard.Run("test.chain", () => throw new NullReferenceException("simulated SetLabel NRE"));

    [HarmonyPriority(Priority.Last)]
    private static void DownstreamThirdPartyPostfix() =>
        _downstreamRan = true;

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

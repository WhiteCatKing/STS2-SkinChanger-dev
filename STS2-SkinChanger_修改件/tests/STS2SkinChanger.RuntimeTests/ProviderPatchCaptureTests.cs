using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;

internal static class ProviderPatchCaptureTests
{
    internal static void Run()
    {
        VerifyFourPathsEquivalenceAndOtherOwnerPreservation();
        VerifyInitializerMidwayFailureOwnershipAndRollback();
        VerifyDeclarativeTargetDiscoveryAndLeakVerification();
        BenchmarkFullVsTargetedWithDecoyMods();
        Console.WriteLine(
            "Provider patch capture passed: 4-path full-vs-targeted equivalence, duplicate counts, other-owner order, failure ownership rollback and leak verification.");
    }

    private static void VerifyFourPathsEquivalenceAndOtherOwnerPreservation()
    {
        var providerAsm = BuildDynamicCallbackAssembly("TestProviderAlpha", out var providerCbs);
        var otherAsm = BuildDynamicCallbackAssembly("TestOtherModBeta", out var otherCbs);
        var dynamicAsm = BuildDynamicCallbackAssembly("TestDynamicProviderGamma", out var dynamicCbs);

        var behaviorTarget = AccessTools.Method(typeof(DummyTargets), nameof(DummyTargets.BehaviorWork))!;
        var unseenBehaviorTarget = AccessTools.Method(typeof(DummyTargets), nameof(DummyTargets.DynamicLateWork))!;
        var managedTarget = AccessTools.Method(typeof(NCard), nameof(NCard._EnterTree))!;

        var hOther1 = new Harmony("sc.test.other.1");
        var hOther2 = new Harmony("sc.test.other.2");
        var hProvider1 = new Harmony("sc.test.provider.1");
        var hProvider2 = new Harmony("sc.test.provider.2");
        var hDynamic = new Harmony("sc.test.dynamic.1");

        try
        {
            // Other Mod installs 2 callbacks with distinct priorities on shared targets.
            hOther1.Patch(behaviorTarget, postfix: new HarmonyMethod(otherCbs.PostfixA, priority: Priority.High));
            hOther2.Patch(behaviorTarget, postfix: new HarmonyMethod(otherCbs.PostfixB, priority: Priority.Low));
            hOther1.Patch(managedTarget, postfix: new HarmonyMethod(otherCbs.PostfixA, priority: Priority.High));
            var expectedOtherOrderBefore = SnapshotOtherOwners(behaviorTarget, providerAsm);
            var expectedManagedOtherBefore = SnapshotOtherOwners(managedTarget, providerAsm);

            // Provider installs behavior + duplicate callback under second owner + managed patch.
            hProvider1.Patch(behaviorTarget, prefix: new HarmonyMethod(providerCbs.PrefixA));
            hProvider1.Patch(behaviorTarget, postfix: new HarmonyMethod(providerCbs.PostfixA));
            // Deliberately add the exact same (Target, Callback, Kind) under a second owner -> DuplicateCount == 1.
            hProvider2.Patch(behaviorTarget, postfix: new HarmonyMethod(providerCbs.PostfixA));
            hProvider1.Patch(managedTarget, postfix: new HarmonyMethod(providerCbs.PostfixB));

            // Path 1: First activation -> must use Full discovery.
            var first = ProviderPatchIndex.CaptureForActivation(
                providerAsm,
                skippedInitializer: false,
                hasProviderBlueprint: false,
                isAdoptedGameAssembly: false);
            var fullBaseline = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.FirstActivation);
            Require(first.Path == ProviderPatchCapturePath.FirstActivation &&
                    first.Mode == ProviderPatchCaptureMode.Full,
                $"首次激活必须走全量发现，实际：path={first.Path}, mode={first.Mode}");
            Require(first.DuplicateCount == 1 && first.RawEntries.Count == 4 && first.DistinctPatches.Count == 3,
                $"重复项计数应为 1（raw=4, distinct=3），实际：dup={first.DuplicateCount}, raw={first.RawEntries.Count}, distinct={first.DistinctPatches.Count}");
            AssertExactMatch(first, fullBaseline, "首次激活");

            // Record blueprint targets (all observed targets: behavior + managed).
            ProviderPatchIndex.RecordBlueprintTargets(
                providerAsm,
                first.DistinctPatches.Select(p => p.Target),
                markCoverageComplete: true);

            // Simulate unpatching managed patch (leaving behavior patches + other Mod's callbacks intact).
            var unpatcher = new Harmony("sc.test.unpatcher");
            unpatcher.Unpatch(managedTarget, providerCbs.PostfixB);
            Require(SnapshotOtherOwners(managedTarget, providerAsm).SequenceEqual(expectedManagedOtherBefore),
                "卸载受管补丁不得影响同方法上其他归属的回调与原有顺序。");

            // Path 2: Repeat activation with complete blueprint -> Targeted mode, exact match with Full.
            var repeat = ProviderPatchIndex.CaptureForActivation(
                providerAsm,
                skippedInitializer: true,
                hasProviderBlueprint: true,
                isAdoptedGameAssembly: false);
            var fullAfterUnpatch = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.RepeatActivation);
            Require(repeat.Path == ProviderPatchCapturePath.RepeatActivation &&
                    repeat.Mode == ProviderPatchCaptureMode.Targeted,
                $"命中完整蓝图的重复激活必须走精准发现，实际：path={repeat.Path}, mode={repeat.Mode}");
            AssertExactMatch(repeat, fullAfterUnpatch, "重复激活");

            // Even if provider lazily re-patches a previously unseen managed target (e.g. NCard.UpdateVisuals),
            // repeat activation's managed-target intersection MUST still detect it identically to Full.
            var lateManagedTarget = AccessTools.Method(typeof(NCard), nameof(NCard.UpdateVisuals))!;
            hProvider1.Patch(lateManagedTarget, postfix: new HarmonyMethod(providerCbs.PostfixB));
            var repeatWithLateManaged = ProviderPatchIndex.CaptureForActivation(
                providerAsm,
                skippedInitializer: true,
                hasProviderBlueprint: true,
                isAdoptedGameAssembly: false);
            var fullWithLateManaged = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.RepeatActivation);
            Require(repeatWithLateManaged.Mode == ProviderPatchCaptureMode.Targeted,
                "重复激活遇新受管目标时仍应保持精准模式。");
            AssertExactMatch(repeatWithLateManaged, fullWithLateManaged, "重复激活+新受管目标");

            // Unpatch all provider callbacks and verify other Mod's callbacks and exact priority order remain intact.
            foreach (var p in repeatWithLateManaged.DistinctPatches)
            {
                unpatcher.Unpatch(p.Target, p.Callback);
            }
            var otherOrderAfter = SnapshotOtherOwners(behaviorTarget, providerAsm);
            Require(otherOrderAfter.SequenceEqual(expectedOtherOrderBefore),
                "卸载提供者回调后，同目标上其他归属回调及其原有顺序必须保持不变。");

            // Path 3a: Dynamic provider (like FoolWaifu) dynamically adds a brand-new arbitrary non-managed target ->
            // must fall back to Full discovery even when skippedInitializer=true & blueprint exists.
            ProviderPatchIndex.MarkDynamicPatchAssembly(dynamicAsm);
            hDynamic.Patch(behaviorTarget, postfix: new HarmonyMethod(dynamicCbs.PostfixA));
            ProviderPatchIndex.RecordBlueprintTargets(dynamicAsm, [behaviorTarget], markCoverageComplete: true);
            hDynamic.Patch(unseenBehaviorTarget, prefix: new HarmonyMethod(dynamicCbs.PrefixA));
            var dynResult = ProviderPatchIndex.CaptureForActivation(
                dynamicAsm,
                skippedInitializer: true,
                hasProviderBlueprint: true,
                isAdoptedGameAssembly: false);
            var dynFull = ProviderPatchIndex.CaptureFull(dynamicAsm, ProviderPatchCapturePath.DynamicOrIncompleteFallback);
            Require(dynResult.Path == ProviderPatchCapturePath.DynamicOrIncompleteFallback &&
                    dynResult.Mode == ProviderPatchCaptureMode.Full,
                $"动态补丁提供者必须保留全量发现，实际：path={dynResult.Path}, mode={dynResult.Mode}");
            AssertExactMatch(dynResult, dynFull, "动态新增目标");

            // Path 3b: Incomplete blueprint coverage -> must also fall back to Full discovery.
            ProviderPatchIndex.GetOrCreateRegistry(providerAsm).InvalidateCoverage();
            hProvider1.Patch(unseenBehaviorTarget, postfix: new HarmonyMethod(providerCbs.PostfixA));
            var incompleteResult = ProviderPatchIndex.CaptureForActivation(
                providerAsm,
                skippedInitializer: true,
                hasProviderBlueprint: true,
                isAdoptedGameAssembly: false);
            var incompleteFull = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.DynamicOrIncompleteFallback);
            Require(incompleteResult.Path == ProviderPatchCapturePath.DynamicOrIncompleteFallback &&
                    incompleteResult.Mode == ProviderPatchCaptureMode.Full,
                "蓝图覆盖不完整时必须回退全量发现。");
            AssertExactMatch(incompleteResult, incompleteFull, "蓝图不完整兜底");
        }
        finally
        {
            hOther1.UnpatchAll(hOther1.Id);
            hOther2.UnpatchAll(hOther2.Id);
            hProvider1.UnpatchAll(hProvider1.Id);
            hProvider2.UnpatchAll(hProvider2.Id);
            hDynamic.UnpatchAll(hDynamic.Id);
            ProviderPatchIndex.UnmarkDynamicPatchAssembly(dynamicAsm);
            ProviderPatchIndex.ClearRegistry(providerAsm);
            ProviderPatchIndex.ClearRegistry(otherAsm);
            ProviderPatchIndex.ClearRegistry(dynamicAsm);
        }
    }

    /// <summary>
    /// Section 7 Failure Scenario #1:
    /// Pre-install an existing callback from the same assembly AND a callback from another owner/assembly;
    /// initializer installs a new callback and then throws. Verify entry-by-entry ownership determination
    /// and rollback against the legacy full-scan semantics (count-only equality is not sufficient).
    /// </summary>
    private static void VerifyInitializerMidwayFailureOwnershipAndRollback()
    {
        var providerAsm = BuildDynamicCallbackAssembly("TestFailProvider", out var providerCbs);
        var otherAsm = BuildDynamicCallbackAssembly("TestFailOtherMod", out var otherCbs);
        var sharedTarget = AccessTools.Method(typeof(DummyTargets), nameof(DummyTargets.FailureTarget))!;

        var hPreExisting = new Harmony("sc.test.fail.preexisting");
        var hOtherOwner = new Harmony("sc.test.fail.other");
        var hInitializer = new Harmony("sc.test.fail.initializer");

        try
        {
            // 1. Pre-existing callback from same assembly + callback from another owner/assembly on same target.
            hPreExisting.Patch(sharedTarget, postfix: new HarmonyMethod(providerCbs.PostfixA, priority: Priority.Normal));
            hOtherOwner.Patch(sharedTarget, prefix: new HarmonyMethod(otherCbs.PrefixA, priority: Priority.High));
            hOtherOwner.Patch(sharedTarget, postfix: new HarmonyMethod(otherCbs.PostfixB, priority: Priority.Low));
            var expectedOtherSnapshot = SnapshotOtherOwners(sharedTarget, providerAsm);

            // Capture pre-initializer snapshot (MUST NOT be skipped!).
            var preSnapshot = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.FirstActivation);
            var legacyPreCount = CaptureLegacyFull(providerAsm).Count;
            Require(preSnapshot.RawEntries.Count == 1 &&
                    preSnapshot.RawEntries[0].Owner == hPreExisting.Id &&
                    preSnapshot.RawEntries[0].Callback == providerCbs.PostfixA &&
                    legacyPreCount == 1,
                "初始化前预扫必须捕获预先存在的同程序集回调及精确归属，且不计入其他程序集回调。");

            // 2. Simulate initializer installing a new callback and then throwing midway.
            var threw = false;
            try
            {
                hInitializer.Patch(sharedTarget, postfix: new HarmonyMethod(providerCbs.PostfixB, priority: Priority.Normal));
                throw new InvalidOperationException("simulated midway initializer failure");
            }
            catch (InvalidOperationException)
            {
                threw = true;
            }
            Require(threw, "应命中模拟初始化异常。");

            var postSnapshot = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.InitializerFailure);
            var legacyPost = CaptureLegacyFull(providerAsm);
            var legacySawInstalled = legacyPost.Count != legacyPreCount;
            var newSawInstalled = ProviderPatchIndex.DidInitializerInstallPatches(preSnapshot.RawEntries, postSnapshot.RawEntries);
            Require(legacySawInstalled && newSawInstalled,
                "初始化半途装入新回调后抛出，新旧判定都必须识别为已装入补丁。");

            var added = ProviderPatchIndex.DiffAddedPatches(preSnapshot.RawEntries, postSnapshot.RawEntries);
            Require(added.Count == 1 &&
                    added[0].Target == sharedTarget &&
                    added[0].Callback == providerCbs.PostfixB &&
                    added[0].Owner == hInitializer.Id,
                $"新增补丁归属核对失败：count={added.Count}");

            // Entry-by-entry comparison against legacy full scan before rollback.
            var legacyOwners = CaptureLegacyFullWithOwners(providerAsm);
            var newOwners = postSnapshot.RawEntries
                .Select(e => (e.Target, e.Callback, e.Kind, e.Owner))
                .OrderBy(x => x.Owner, StringComparer.Ordinal)
                .ThenBy(x => x.Callback.Name, StringComparer.Ordinal)
                .ToArray();
            Require(newOwners.SequenceEqual(legacyOwners),
                "失败路径下逐条 (Target, Callback, Kind, Owner) 必须与旧全量扫描完全一致。");

            // 3. Also test same-count replacement (remove old + add new -> count stays 1==1,
            //    yet ownership/callback changed; entry-by-entry check must detect it!).
            var swappedEntries = new[]
            {
                new CapturedProviderPatch(sharedTarget, providerCbs.PostfixB, ProviderPatchIndex.PatchKindPostfix, hInitializer.Id, Priority.Normal, [], [], false)
            };
            Require(ProviderPatchIndex.DidInitializerInstallPatches(preSnapshot.RawEntries, swappedEntries),
                "总数恰好相等但回调/归属变化时，逐条核对必须识别出新补丁，不能只比总数。");

            // 4. Simulate rollback unpatch (exact production semantics) and verify other owner untouched in order.
            var rollbackUnpatcher = new Harmony("sc.test.fail.rollback");
            foreach (var p in postSnapshot.DistinctPatches)
            {
                rollbackUnpatcher.Unpatch(p.Target, p.Callback);
            }

            var remainingProvider = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.InitializerFailure);
            Require(remainingProvider.RawEntries.Count == 0,
                $"回滚后同程序集补丁应全部清零，实际剩余：{remainingProvider.RawEntries.Count}");
            Require(SnapshotOtherOwners(sharedTarget, providerAsm).SequenceEqual(expectedOtherSnapshot),
                "回滚后其他归属的回调内容与先后顺序必须完整保留。");
        }
        finally
        {
            hPreExisting.UnpatchAll(hPreExisting.Id);
            hOtherOwner.UnpatchAll(hOtherOwner.Id);
            hInitializer.UnpatchAll(hInitializer.Id);
            ProviderPatchIndex.ClearRegistry(providerAsm);
            ProviderPatchIndex.ClearRegistry(otherAsm);
        }
    }

    private static void VerifyDeclarativeTargetDiscoveryAndLeakVerification()
    {
        var declarativeTargets = ProviderPatchIndex.ResolveDeclarativePatchTargets(typeof(SampleDeclarativePatch).Assembly);
        var expected = AccessTools.Method(typeof(DummyTargets), nameof(DummyTargets.DeclarativeTarget))!;
        Require(declarativeTargets.Contains(expected),
            "ResolveDeclarativePatchTargets 必须能静态解析 [HarmonyPatch] 声明目标。");

        // Verify leak verification scans ONLY unpatched targets (not all Harmony methods in process).
        var providerAsm = BuildDynamicCallbackAssembly("TestLeakProvider", out var cbs);
        var managedTarget = AccessTools.Method(typeof(DummyTargets), nameof(DummyTargets.LeakCheckTarget))!;
        var h = new Harmony("sc.test.leak");
        try
        {
            h.Patch(managedTarget, postfix: new HarmonyMethod(cbs.PostfixA));
            var beforeUnpatch = ProviderPatchIndex.CaptureOnTargets(providerAsm, [managedTarget], ProviderPatchCapturePath.LeakVerification);
            Require(beforeUnpatch.ScannedTargetCount == 1 && beforeUnpatch.DistinctPatches.Count == 1,
                "泄漏校验在未卸载前应能精准检出残留补丁，且只扫 1 个目标。");

            h.Unpatch(managedTarget, cbs.PostfixA);
            var afterUnpatch = ProviderPatchIndex.CaptureOnTargets(providerAsm, [managedTarget], ProviderPatchCapturePath.LeakVerification);
            Require(afterUnpatch.ScannedTargetCount == 1 && afterUnpatch.DistinctPatches.Count == 0,
                "泄漏校验在卸载后应确认 0 残留且只扫本次卸载过的 1 个目标。");
        }
        finally
        {
            h.UnpatchAll(h.Id);
            ProviderPatchIndex.ClearRegistry(providerAsm);
        }
    }

    private static void BenchmarkFullVsTargetedWithDecoyMods()
    {
        const int decoyCount = 150;
        var providerAsm = BuildDynamicCallbackAssembly("TestBenchProvider", out var providerCbs);
        var decoyAsm = BuildDynamicCallbackAssembly("TestBenchDecoys", out var decoyCbs);
        var decoyTargets = BuildDynamicTargetMethods("TestBenchDecoyTargets", decoyCount);
        var behaviorTarget = AccessTools.Method(typeof(DummyTargets), nameof(DummyTargets.BehaviorWork))!;

        var hDecoy = new Harmony("sc.test.bench.decoy");
        var hProvider = new Harmony("sc.test.bench.provider");
        try
        {
            foreach (var target in decoyTargets)
            {
                hDecoy.Patch(target, postfix: new HarmonyMethod(decoyCbs.PostfixA));
            }
            hProvider.Patch(behaviorTarget, postfix: new HarmonyMethod(providerCbs.PostfixA));

            var first = ProviderPatchIndex.CaptureForActivation(
                providerAsm,
                skippedInitializer: false,
                hasProviderBlueprint: false,
                isAdoptedGameAssembly: false);
            ProviderPatchIndex.RecordBlueprintTargets(providerAsm, first.DistinctPatches.Select(p => p.Target), markCoverageComplete: true);

            // Warmup once
            _ = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.FirstActivation);
            _ = ProviderPatchIndex.CaptureForActivation(providerAsm, skippedInitializer: true, hasProviderBlueprint: true, isAdoptedGameAssembly: false);

            const int rounds = 10;
            var fullSw = Stopwatch.StartNew();
            ProviderPatchCaptureResult lastFull = null!;
            for (var i = 0; i < rounds; i++)
            {
                lastFull = ProviderPatchIndex.CaptureFull(providerAsm, ProviderPatchCapturePath.FirstActivation);
            }
            fullSw.Stop();

            var targetedSw = Stopwatch.StartNew();
            ProviderPatchCaptureResult lastTargeted = null!;
            for (var i = 0; i < rounds; i++)
            {
                lastTargeted = ProviderPatchIndex.CaptureForActivation(
                    providerAsm,
                    skippedInitializer: true,
                    hasProviderBlueprint: true,
                    isAdoptedGameAssembly: false);
            }
            targetedSw.Stop();

            AssertExactMatch(lastTargeted, lastFull, "基准对照");
            Require(lastTargeted.ScannedTargetCount < lastFull.ScannedTargetCount / 5,
                $"精准扫描目标数应远小于全量扫描：targeted={lastTargeted.ScannedTargetCount}, full={lastFull.ScannedTargetCount}");
            Console.WriteLine(FormattableString.Invariant(
                $"Provider patch capture benchmark ({decoyCount} decoy methods, {rounds}x avg): full={fullSw.Elapsed.TotalMilliseconds / rounds:F3} ms ({lastFull.ScannedTargetCount} targets), targeted={targetedSw.Elapsed.TotalMilliseconds / rounds:F3} ms ({lastTargeted.ScannedTargetCount} targets)."));
        }
        finally
        {
            hDecoy.UnpatchAll(hDecoy.Id);
            hProvider.UnpatchAll(hProvider.Id);
            ProviderPatchIndex.ClearRegistry(providerAsm);
            ProviderPatchIndex.ClearRegistry(decoyAsm);
        }
    }

    private static void AssertExactMatch(
        ProviderPatchCaptureResult actual,
        ProviderPatchCaptureResult expected,
        string label)
    {
        Require(actual.DuplicateCount == expected.DuplicateCount,
            $"[{label}] 重复项数量不一致：actual={actual.DuplicateCount}, expected={expected.DuplicateCount}");

        var actualDistinct = actual.DistinctPatches
            .Select(p => (p.Target, p.Callback, p.Kind, p.Owner))
            .OrderBy(x => x.Target.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Callback.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Kind)
            .ThenBy(x => x.Owner, StringComparer.Ordinal)
            .ToArray();
        var expectedDistinct = expected.DistinctPatches
            .Select(p => (p.Target, p.Callback, p.Kind, p.Owner))
            .OrderBy(x => x.Target.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Callback.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Kind)
            .ThenBy(x => x.Owner, StringComparer.Ordinal)
            .ToArray();
        Require(actualDistinct.SequenceEqual(expectedDistinct),
            $"[{label}] 去重补丁集合不一致：actual={actualDistinct.Length}, expected={expectedDistinct.Length}");

        var actualRaw = actual.RawEntries
            .Select(p => (p.Target, p.Callback, p.Kind, p.Owner))
            .OrderBy(x => x.Target.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Callback.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Kind)
            .ThenBy(x => x.Owner, StringComparer.Ordinal)
            .ToArray();
        var expectedRaw = expected.RawEntries
            .Select(p => (p.Target, p.Callback, p.Kind, p.Owner))
            .OrderBy(x => x.Target.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Callback.Name, StringComparer.Ordinal)
            .ThenBy(x => x.Kind)
            .ThenBy(x => x.Owner, StringComparer.Ordinal)
            .ToArray();
        Require(actualRaw.SequenceEqual(expectedRaw),
            $"[{label}] 原始归属清单不一致：actual={actualRaw.Length}, expected={expectedRaw.Length}");
    }

    private static (MethodInfo Callback, string Owner, int Priority, int Kind)[] SnapshotOtherOwners(
        MethodBase target,
        Assembly excludedAssembly)
    {
        var info = Harmony.GetPatchInfo(target);
        if (info == null) return [];
        return info.Prefixes.Select(p => (p, ProviderPatchIndex.PatchKindPrefix))
            .Concat(info.Postfixes.Select(p => (p, ProviderPatchIndex.PatchKindPostfix)))
            .Concat(info.Transpilers.Select(p => (p, ProviderPatchIndex.PatchKindTranspiler)))
            .Concat(info.Finalizers.Select(p => (p, ProviderPatchIndex.PatchKindFinalizer)))
            .Where(x => !ReferenceEquals(x.p.PatchMethod.Module.Assembly, excludedAssembly))
            .Select(x => (x.p.PatchMethod, x.p.owner ?? string.Empty, x.p.priority, x.Item2))
            .ToArray();
    }

    private static List<(MethodBase Target, MethodInfo Callback, int Kind)> CaptureLegacyFull(Assembly assembly) =>
        Harmony.GetAllPatchedMethods()
            .SelectMany(target =>
            {
                var info = Harmony.GetPatchInfo(target);
                if (info == null) return [];
                return info.Prefixes.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindPrefix))
                    .Concat(info.Postfixes.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindPostfix)))
                    .Concat(info.Transpilers.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindTranspiler)))
                    .Concat(info.Finalizers.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindFinalizer)));
            })
            .Where(x => ReferenceEquals(x.PatchMethod.Module.Assembly, assembly))
            .Distinct()
            .ToList();

    private static (MethodBase Target, MethodInfo Callback, int Kind, string Owner)[] CaptureLegacyFullWithOwners(Assembly assembly) =>
        Harmony.GetAllPatchedMethods()
            .SelectMany(target =>
            {
                var info = Harmony.GetPatchInfo(target);
                if (info == null) return [];
                return info.Prefixes.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindPrefix, p.owner ?? string.Empty))
                    .Concat(info.Postfixes.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindPostfix, p.owner ?? string.Empty)))
                    .Concat(info.Transpilers.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindTranspiler, p.owner ?? string.Empty)))
                    .Concat(info.Finalizers.Select(p => (target, p.PatchMethod, ProviderPatchIndex.PatchKindFinalizer, p.owner ?? string.Empty)));
            })
            .Where(x => ReferenceEquals(x.PatchMethod.Module.Assembly, assembly))
            .OrderBy(x => x.Item4, StringComparer.Ordinal)
            .ThenBy(x => x.PatchMethod.Name, StringComparer.Ordinal)
            .ToArray();

    private static Assembly BuildDynamicCallbackAssembly(string name, out DynamicCallbacks callbacks)
    {
        var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
        var mod = asm.DefineDynamicModule(name + ".dll");
        var type = mod.DefineType(name + ".Callbacks", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        foreach (var methodName in new[] { "PrefixA", "PostfixA", "PostfixB" })
        {
            var mb = type.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
            var il = mb.GetILGenerator();
            il.Emit(OpCodes.Ret);
        }

        var created = type.CreateType()!;
        callbacks = new DynamicCallbacks(
            created.GetMethod("PrefixA")!,
            created.GetMethod("PostfixA")!,
            created.GetMethod("PostfixB")!);
        return created.Assembly;
    }

    private static MethodInfo[] BuildDynamicTargetMethods(string name, int count)
    {
        var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), AssemblyBuilderAccess.Run);
        var mod = asm.DefineDynamicModule(name + ".dll");
        var type = mod.DefineType(name + ".Targets", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        for (var i = 0; i < count; i++)
        {
            var mb = type.DefineMethod("Decoy_" + i, MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes);
            var il = mb.GetILGenerator();
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ret);
        }

        var created = type.CreateType()!;
        return Enumerable.Range(0, count).Select(i => created.GetMethod("Decoy_" + i)!).ToArray();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private sealed record DynamicCallbacks(MethodInfo PrefixA, MethodInfo PostfixA, MethodInfo PostfixB);

    internal static class DummyTargets
    {
        public static int BehaviorWork() => 1;
        public static int DynamicLateWork() => 2;
        public static int FailureTarget() => 3;
        public static int DeclarativeTarget() => 4;
        public static int LeakCheckTarget() => 5;
    }

    [HarmonyPatch(typeof(DummyTargets), nameof(DummyTargets.DeclarativeTarget))]
    internal static class SampleDeclarativePatch
    {
        private static void Postfix() { }
    }
}

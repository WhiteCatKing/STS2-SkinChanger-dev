using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

public static class TwinAssemblyBehaviorDelegationTests
{
    private static readonly ConcurrentDictionary<string, int> InitCounts = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, Harmony> ActiveHarmonies = new(StringComparer.Ordinal);

    private static readonly string[] SlimmerTwinCoreTypes =
    [
        "SharedCore.Palette",
        "SharedCore.WearTimer",
        "SharedCore.Settings",
        "SharedCore.CharSelectBridge"
    ];

    private static readonly string[] FullerTwinCoreTypes =
    [
        .. SlimmerTwinCoreTypes,
        "SharedCore.CombatStageWear"
    ];

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static int ComputeTwinBehaviorValue() => 0;

    public static void OnTestProviderInitialized(string tag, Type providerType)
    {
        InitCounts.AddOrUpdate(tag, 1, static (_, count) => count + 1);
        var target = AccessTools.Method(typeof(TwinAssemblyBehaviorDelegationTests), nameof(ComputeTwinBehaviorValue))!;
        var postfix = providerType.GetMethod("Postfix", BindingFlags.Public | BindingFlags.Static)!;
        var harmony = ActiveHarmonies.GetOrAdd(tag, static key => new Harmony("sc.test.twin.behavior." + key));
        harmony.Patch(target, postfix: new HarmonyMethod(postfix));
    }

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-twin-delegation-");
        var infoLogs = new List<string>();
        var warnLogs = new List<string>();
        var logHook = CaptureModLog(infoLogs, warnLogs);
        try
        {
            VerifySlimmerFirstAlwaysLoadsFullerAndDelegates(root, infoLogs);
            VerifyFullerFirstDirectionPreserved(root, infoLogs);
            VerifyDirectFullerToSlimmerSwitchKeepsActiveExecutorWithoutTeardownOrReinstall(root);
            VerifyRegistrationConflictStillPrefersFullerBuild(root, infoLogs);
            VerifyReverseDelegationFullerToSlimmerForbidden(root, warnLogs);
            VerifyTemplateCloneDifferentTypesDoesNotDelegate(root, warnLogs);
            VerifySameSettingsContractLowOverlapSelectSmallerDoesNotRunUnselectedLarger(root);
            VerifyDelegatedCallbacksAndRunEnvironmentAuthorization(root);
            VerifyUnregisteredCollisionStillWarnsAndStaysResourceOnly(root, warnLogs);
            Console.WriteLine("Twin-assembly behavior delegation passed: slimmer-first loads fuller (including early inspection), A->vanilla->B, B->vanilla->A, A<->B, A->other->B, reverse delegation blocked, same-ID reuse, template-clone + same-contract low-overlap guard, delegated char/room replay + run-environment auth, and unregistered conflict guard.");
        }
        finally
        {
            logHook.UnpatchAll(logHook.Id);
            foreach (var harmony in ActiveHarmonies.Values)
            {
                harmony.UnpatchAll(harmony.Id);
            }
            ActiveHarmonies.Clear();
            InitCounts.Clear();
            try { root.Delete(recursive: true); } catch { }
        }
    }

    private static void VerifySlimmerFirstAlwaysLoadsFullerAndDelegates(DirectoryInfo root, List<string> infoLogs)
    {
        // A = slimmer build (5 authored types: Entry + 4 core), B = fuller build (6 authored types: Entry + 5 core).
        // Overlap = 5/6 = 0.833 >= 0.80. Requesting A FIRST (even via early inspection / script registration)
        // must load B's bytes, delegate A -> B, and run ONLY B across A->vanilla->B, B->vanilla->A, A<->B, A->other->B.
        var asmName = "TwinSlimmerFirst_" + Guid.NewGuid().ToString("N");
        var otherAsmName = "TwinOtherSkin_" + Guid.NewGuid().ToString("N");
        var sameIdAsmName = "TwinSameIdFixture_" + Guid.NewGuid().ToString("N");

        var idSlimA = "TwinSlimA_" + Guid.NewGuid().ToString("N");
        var idFullB = "TwinFullB_" + Guid.NewGuid().ToString("N");
        var idOther = "OtherCharSkin_" + Guid.NewGuid().ToString("N");
        var sameIdRoot = "SameIdSibling_" + Guid.NewGuid().ToString("N");
        var sameIdA = sameIdRoot + "::source:first";
        var sameIdB = sameIdRoot + "::source:second";

        var pathSlimA = EmitTwinProviderDll(root, "slim_a.dll", asmName, new Version(0, 3, 5, 0), tag: idSlimA, delta: 100, extraTypes: SlimmerTwinCoreTypes);
        var pathFullB = EmitTwinProviderDll(root, "full_b.dll", asmName, new Version(0, 3, 16, 0), tag: idFullB, delta: 200, extraTypes: FullerTwinCoreTypes);
        var pathOther = EmitTwinProviderDll(root, "pack_other.dll", otherAsmName, new Version(1, 0, 0, 0), tag: idOther, delta: 7);
        // Same-ID sibling intentionally has disjoint types to prove same-ID reuse remains unchanged.
        var pathSameA = EmitTwinProviderDll(root, "same_a.dll", sameIdAsmName, new Version(1, 0, 0, 0), tag: sameIdA, delta: 33, extraTypes: ["SameA.OnlyA1", "SameA.OnlyA2"]);
        var pathSameB = EmitTwinProviderDll(root, "same_b.dll", sameIdAsmName, new Version(1, 2, 0, 0), tag: sameIdB, delta: 44, extraTypes: ["SameB.OnlyB1", "SameB.OnlyB2"]);

        var registeredIds = new[] { idSlimA, idFullB, idOther, sameIdA, sameIdB };
        var registeredPaths = new[] { pathSlimA, pathFullB, pathOther, pathSameA, pathSameB };
        try
        {
            RegisterTestProvider(idSlimA, pathSlimA, canActivateBehavior: true);
            RegisterTestProvider(idFullB, pathFullB, canActivateBehavior: true);
            RegisterTestProvider(idOther, pathOther, canActivateBehavior: true);
            RegisterTestProvider(sameIdA, pathSameA, canActivateBehavior: true);
            RegisterTestProvider(sameIdB, pathSameB, canActivateBehavior: true);

            // Simulate an early loader path (like EnsureProviderGodotScripts / TryGetProviderAssemblyForInspection)
            // touching slimmer A before ActivateSelectedProviders ever runs: it MUST load fuller B, NOT A.
            Require(ManagedSkinModLoader.TryGetProviderAssemblyForInspection(idSlimA, out var preloadedAsm) &&
                    preloadedAsm.GetName().Version == new Version(0, 3, 16, 0),
                $"提前检查精简版 A 时也必须优先加载更完整的 B（0.3.16）：actual={preloadedAsm?.GetName().Version}");
            Require(ManagedSkinModLoader.TryGetLoadedProviderAssembly(pathFullB, out var loadedB) &&
                    ReferenceEquals(loadedB, preloadedAsm) &&
                    !ManagedSkinModLoader.TryGetLoadedProviderAssembly(pathSlimA, out _),
                "实际加载并登记的必须是完整版 B 的字节，精简版 A 的 DLL 不得抢先加载。");
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA) == idFullB,
                $"精简版 A 必须被委托给完整版 B：actual={ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA)}");

            // 1) First activation requests A (slimmer) -> B's behavior runs!
            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);
            Require(ComputeTwinBehaviorValue() == 200,
                $"先选精简版 A 时必须实际运行完整版 B 的行为补丁（200）：actual={ComputeTwinBehaviorValue()}");
            Require(InitCounts.GetValueOrDefault(idFullB) == 1 && InitCounts.GetValueOrDefault(idSlimA) == 0,
                $"只能初始化完整版 B 一次，精简版 A 不得运行：initB={InitCounts.GetValueOrDefault(idFullB)}, initA={InitCounts.GetValueOrDefault(idSlimA)}");
            Require(ManagedSkinModLoader.IsBehaviorActiveFor(idSlimA) && ManagedSkinModLoader.IsProviderActive(idFullB),
                "A 选中时代理执行者 B 必须处于激活态。");
            var activeAsm = ManagedSkinModLoader.GetActiveProviderAssembly(idSlimA);
            Require(activeAsm != null && ManagedSkinModLoader.IsProviderAssemblyFor(idSlimA, activeAsm),
                "IsProviderAssemblyFor 必须经别名放行 A 对应的 B 程序集（保障 ScopedSkinBehavior / 换装计时器放行）。");
            Require(infoLogs.Any(line =>
                    line.Contains(idSlimA, StringComparison.Ordinal) &&
                    line.Contains(idFullB, StringComparison.Ordinal) &&
                    line.Contains("类型重合 0.83", StringComparison.Ordinal)),
                "委托成立日志必须写明委托双方与类型重合度。");

            // 2) A -> Vanilla -> B
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            Require(ComputeTwinBehaviorValue() == 0 && !ManagedSkinModLoader.IsProviderActive(idFullB),
                "切回原版后 B 的行为补丁必须卸载。");

            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);
            Require(ComputeTwinBehaviorValue() == 200 &&
                    ManagedSkinModLoader.IsProviderActive(idFullB) &&
                    InitCounts.GetValueOrDefault(idFullB) == 1,
                $"A -> 原版 -> B 后必须经蓝图重挂 B 的补丁且只初始化一次：val={ComputeTwinBehaviorValue()}, initB={InitCounts.GetValueOrDefault(idFullB)}");

            // 3) B -> Vanilla -> A (still only B runs, initializer still once!)
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            Require(ComputeTwinBehaviorValue() == 0, "B 切原版后必须卸载。");

            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);
            Require(ComputeTwinBehaviorValue() == 200 &&
                    ManagedSkinModLoader.IsBehaviorActiveFor(idSlimA) &&
                    ManagedSkinModLoader.IsProviderActive(idFullB) &&
                    InitCounts.GetValueOrDefault(idFullB) == 1 &&
                    InitCounts.GetValueOrDefault(idSlimA) == 0,
                $"B -> 原版 -> A 后同样只有完整版 B 在跑且只初始化一次：val={ComputeTwinBehaviorValue()}, initB={InitCounts.GetValueOrDefault(idFullB)}");

            // 4) Direct swap A <-> B must keep B alive without unpatching
            ManagedSkinModLoader.DeactivateProvidersExcept([idSlimA]);
            Require(ComputeTwinBehaviorValue() == 200 && ManagedSkinModLoader.IsProviderActive(idFullB),
                "DeactivateProvidersExcept([A]) 必须保留委托目标 B。");
            ManagedSkinModLoader.DeactivateProvidersExcept([idFullB]);
            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);
            Require(ComputeTwinBehaviorValue() == 200 && InitCounts.GetValueOrDefault(idFullB) == 1,
                "A -> B 直接互切必须保持 B 在位且不重复初始化。");
            ManagedSkinModLoader.DeactivateProvidersExcept([idSlimA]);
            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);
            Require(ComputeTwinBehaviorValue() == 200 && InitCounts.GetValueOrDefault(idFullB) == 1,
                "B -> A 直接互切必须保持 B 在位且不重复初始化。");

            // 5) A -> OtherSkin C -> B
            ManagedSkinModLoader.DeactivateProvidersExcept([idOther]);
            ManagedSkinModLoader.ActivateSelectedProviders([idOther]);
            Require(ComputeTwinBehaviorValue() == 7 && !ManagedSkinModLoader.IsProviderActive(idFullB),
                $"切到别的角色皮肤 C 后应停用 B 并启用 C：actual={ComputeTwinBehaviorValue()}");

            ManagedSkinModLoader.DeactivateProvidersExcept([idFullB]);
            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);
            Require(ComputeTwinBehaviorValue() == 200 && !ManagedSkinModLoader.IsProviderActive(idOther) &&
                    InitCounts.GetValueOrDefault(idFullB) == 1,
                $"C -> B 后必须停用 C 并走蓝图重激活 B：val={ComputeTwinBehaviorValue()}, initB={InitCounts.GetValueOrDefault(idFullB)}");

            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            Require(ComputeTwinBehaviorValue() == 0, "清空选择后应无残留补丁。");

            // 6) Same-ID sibling (::source:*) non-regression
            ManagedSkinModLoader.ActivateSelectedProviders([sameIdA]);
            Require(ComputeTwinBehaviorValue() == 33 && InitCounts.GetValueOrDefault(sameIdA) == 1,
                "同 ID 姊妹包 A 首次激活必须正常。");
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            Require(ComputeTwinBehaviorValue() == 0, "同 ID 姊妹包切原版后必须卸载。");
            ManagedSkinModLoader.ActivateSelectedProviders([sameIdB]);
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(sameIdB) == sameIdA &&
                    ComputeTwinBehaviorValue() == 33 &&
                    InitCounts.GetValueOrDefault(sameIdA) == 1,
                "同 ID 姊妹包原有路径不得回退。");
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
        }
        finally
        {
            CleanupTestProviders(registeredIds, registeredPaths);
        }
    }

    private static void VerifyFullerFirstDirectionPreserved(DirectoryInfo root, List<string> infoLogs)
    {
        var asmName = "TwinFullerFirst_" + Guid.NewGuid().ToString("N");
        var idSlimA = "FFSlimA_" + Guid.NewGuid().ToString("N");
        var idFullB = "FFFullB_" + Guid.NewGuid().ToString("N");

        var pathSlimA = EmitTwinProviderDll(root, "ff_slim_a.dll", asmName, new Version(0, 3, 5, 0), tag: idSlimA, delta: 50, extraTypes: SlimmerTwinCoreTypes);
        var pathFullB = EmitTwinProviderDll(root, "ff_full_b.dll", asmName, new Version(0, 3, 16, 0), tag: idFullB, delta: 60, extraTypes: FullerTwinCoreTypes);

        try
        {
            RegisterTestProvider(idSlimA, pathSlimA, canActivateBehavior: true);
            RegisterTestProvider(idFullB, pathFullB, canActivateBehavior: true);

            // Request fuller B first -> Vanilla -> slimmer A
            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);
            Require(ComputeTwinBehaviorValue() == 60 && InitCounts.GetValueOrDefault(idFullB) == 1,
                "先选完整版 B 时 B 自身行为必须生效。");

            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            Require(ComputeTwinBehaviorValue() == 0, "B 切原版后必须卸载。");

            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA) == idFullB &&
                    ManagedSkinModLoader.IsBehaviorActiveFor(idSlimA) &&
                    ComputeTwinBehaviorValue() == 60 &&
                    InitCounts.GetValueOrDefault(idFullB) == 1 &&
                    InitCounts.GetValueOrDefault(idSlimA) == 0,
                $"先选 B 再切原版再选精简版 A 时，A 必须委托给 B 且只初始化一次：val={ComputeTwinBehaviorValue()}, alias={ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA)}");

            ManagedSkinModLoader.DeactivateProvidersExcept([idSlimA]);
            Require(ComputeTwinBehaviorValue() == 60, "DeactivateProvidersExcept([A]) 不得卸掉 B。");
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
        }
        finally
        {
            CleanupTestProviders([idSlimA, idFullB], [pathSlimA, pathFullB]);
        }
    }

    private static int _spyDeactivateProviderCalls;
    private static int _spyPatchProviderCallbackCalls;
    private static int _spySuspendNodeCalls;
    private static int _spyRestoreNodeCalls;

    private static void SpyCountDeactivateProvider() => _spyDeactivateProviderCalls++;
    private static void SpyCountPatchProviderCallbacks() => _spyPatchProviderCallbackCalls++;
    private static void SpyCountSuspendNodes() => _spySuspendNodeCalls++;
    private static void SpyCountRestoreNodes() => _spyRestoreNodeCalls++;

    private static void VerifyDirectFullerToSlimmerSwitchKeepsActiveExecutorWithoutTeardownOrReinstall(DirectoryInfo root)
    {
        // Fresh process selects fuller twin B first while slimmer twin A has NEVER been inspected or
        // registered as an alias. Direct switch B -> A (without visiting vanilla) must keep B active
        // across DeactivateProvidersExcept([A]) + ActivateSelectedProviders([A]) with ZERO redundant
        // teardown and ZERO redundant blueprint re-installation.
        var asmName = "TwinDirectBToA_" + Guid.NewGuid().ToString("N");
        var idSlimA = "DirectSlimA_" + Guid.NewGuid().ToString("N");
        var idFullB = "DirectFullB_" + Guid.NewGuid().ToString("N");
        var idCloneC = "DirectCloneC_" + Guid.NewGuid().ToString("N");

        var pathSlimA = EmitTwinProviderDll(root, "direct_slim_a.dll", asmName, new Version(0, 3, 5, 0), tag: idSlimA, delta: 100, extraTypes: SlimmerTwinCoreTypes);
        var pathFullB = EmitTwinProviderDll(root, "direct_full_b.dll", asmName, new Version(0, 3, 16, 0), tag: idFullB, delta: 240, extraTypes: FullerTwinCoreTypes);
        // Same assembly name but disjoint types (< 0.80 overlap): switching B -> C MUST still deactivate B!
        var pathCloneC = EmitTwinProviderDll(root, "direct_clone_c.dll", asmName, new Version(1, 0, 0, 0), tag: idCloneC, delta: 99,
            extraTypes: ["CloneC.One", "CloneC.Two", "CloneC.Three", "CloneC.Four", "CloneC.Five"]);

        var spy = new Harmony("sc.test.twin.direct-b-to-a." + Guid.NewGuid().ToString("N"));
        try
        {
            RegisterTestProvider(idSlimA, pathSlimA, canActivateBehavior: true);
            RegisterTestProvider(idFullB, pathFullB, canActivateBehavior: true);
            RegisterTestProvider(idCloneC, pathCloneC, canActivateBehavior: true);

            // Step 1: Activate fuller B first; A is completely untouched/unaliased.
            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);
            Require(ComputeTwinBehaviorValue() == 240 &&
                    ManagedSkinModLoader.IsProviderActive(idFullB) &&
                    InitCounts.GetValueOrDefault(idFullB) == 1 &&
                    ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA) == idSlimA,
                "直切前置条件：完整版 B 必须已激活，且精简版 A 尚未建立别名。");

            // Attach spies right before the direct B -> A switch.
            _spyDeactivateProviderCalls = 0;
            _spyPatchProviderCallbackCalls = 0;
            _spySuspendNodeCalls = 0;
            _spyRestoreNodeCalls = 0;
            spy.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "DeactivateProvider"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SpyCountDeactivateProvider)));
            spy.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "PatchProviderCallbacks"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SpyCountPatchProviderCallbacks)));
            spy.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "SuspendProviderNodes"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SpyCountSuspendNodes)));
            spy.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "RestoreProviderNodes"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SpyCountRestoreNodes)));

            // Step 2: Direct switch B -> A (exact MountOverlay order: DeactivateProvidersExcept then ActivateSelectedProviders).
            ManagedSkinModLoader.DeactivateProvidersExcept([idSlimA]);
            Require(_spyDeactivateProviderCalls == 0 && _spySuspendNodeCalls == 0,
                $"B -> A 首次直切在停用阶段不得误拆仍在用的执行者 B：deactivate={_spyDeactivateProviderCalls}, suspend={_spySuspendNodeCalls}");
            Require(ManagedSkinModLoader.IsProviderActive(idFullB) && ComputeTwinBehaviorValue() == 240,
                $"DeactivateProvidersExcept([A]) 后执行者 B 必须持续保持在位与补丁生效：active={ManagedSkinModLoader.IsProviderActive(idFullB)}, val={ComputeTwinBehaviorValue()}");
            // Read-only pre-check must NOT load A's DLL or eagerly initialize anything before activation.
            Require(!ManagedSkinModLoader.TryGetLoadedProviderAssembly(pathSlimA, out _),
                "停用筛选阶段不得提前加载精简版 A 的程序集。");

            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);
            Require(_spyDeactivateProviderCalls == 0 &&
                    _spyPatchProviderCallbackCalls == 0 &&
                    _spySuspendNodeCalls == 0 &&
                    _spyRestoreNodeCalls == 0,
                $"B -> A 首次直切全程不得出现多余停用、节点暂停/恢复或蓝图重挂：deactivate={_spyDeactivateProviderCalls}, patch={_spyPatchProviderCallbackCalls}, suspend={_spySuspendNodeCalls}, restore={_spyRestoreNodeCalls}");
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA) == idFullB &&
                    ManagedSkinModLoader.IsBehaviorActiveFor(idSlimA) &&
                    ManagedSkinModLoader.IsProviderActive(idFullB) &&
                    ComputeTwinBehaviorValue() == 240 &&
                    InitCounts.GetValueOrDefault(idFullB) == 1 &&
                    InitCounts.GetValueOrDefault(idSlimA) == 0,
                $"B -> A 直切后应建立 A -> B 委托且 B 仅初始化一次：alias={ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA)}, val={ComputeTwinBehaviorValue()}, initB={InitCounts.GetValueOrDefault(idFullB)}, initA={InitCounts.GetValueOrDefault(idSlimA)}");

            // Step 3: Negative control — switching from B/A to an unrelated low-overlap clone C MUST still deactivate B.
            ManagedSkinModLoader.DeactivateProvidersExcept([idCloneC]);
            Require(_spyDeactivateProviderCalls == 1 &&
                    !ManagedSkinModLoader.IsProviderActive(idFullB) &&
                    ComputeTwinBehaviorValue() == 0,
                $"切到同名低重合克隆包 C 时仍必须正常停用 B：deactivate={_spyDeactivateProviderCalls}, activeB={ManagedSkinModLoader.IsProviderActive(idFullB)}, val={ComputeTwinBehaviorValue()}");
        }
        finally
        {
            spy.UnpatchAll(spy.Id);
            CleanupTestProviders([idSlimA, idFullB, idCloneC], [pathSlimA, pathFullB, pathCloneC]);
        }
    }

    private static void VerifyRegistrationConflictStillPrefersFullerBuild(DirectoryInfo root, List<string> infoLogs)
    {
        // Both packs have identical FullName (same version) and different bytes: slimmer A was scanned
        // first at registration (CanActivateBehavior=true) and fuller B second (CanActivateBehavior=false).
        // Even when A is requested first, fuller B MUST be loaded instead of slimmer A.
        var asmName = "TwinIdenticalVerFuller_" + Guid.NewGuid().ToString("N");
        var idSlimA = "SameVerSlimA_" + Guid.NewGuid().ToString("N");
        var idFullB = "SameVerFullB_" + Guid.NewGuid().ToString("N");

        var pathSlimA = EmitTwinProviderDll(root, "samever_slim_a.dll", asmName, new Version(1, 0, 0, 0), tag: idSlimA, delta: 80, extraTypes: SlimmerTwinCoreTypes);
        var pathFullB = EmitTwinProviderDll(root, "samever_full_b.dll", asmName, new Version(1, 0, 0, 0), tag: idFullB, delta: 90, extraTypes: FullerTwinCoreTypes);
        var identity = AssemblyName.GetAssemblyName(pathSlimA).FullName;

        try
        {
            RegisterTestProvider(idSlimA, pathSlimA, canActivateBehavior: true, registerSourceIdentity: identity);
            RegisterTestProvider(idFullB, pathFullB, canActivateBehavior: false);

            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA) == idFullB &&
                    ComputeTwinBehaviorValue() == 90 &&
                    InitCounts.GetValueOrDefault(idFullB) == 1 &&
                    InitCounts.GetValueOrDefault(idSlimA) == 0,
                $"同 FullName 下精简版先登记时，选 A 也必须优先加载更完整的 B（90）：val={ComputeTwinBehaviorValue()}, alias={ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA)}");

            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);
            Require(ComputeTwinBehaviorValue() == 90 && InitCounts.GetValueOrDefault(idFullB) == 1,
                "切回 B 必须走蓝图重激活且只初始化一次。");
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
        }
        finally
        {
            CleanupTestProviders([idSlimA, idFullB], [pathSlimA, pathFullB], [identity]);
        }
    }

    private static void VerifyReverseDelegationFullerToSlimmerForbidden(DirectoryInfo root, List<string> warnLogs)
    {
        // Simulate a missed/external path that already loaded the slimmer DLL into the load context.
        // Requesting the fuller build MUST NOT reverse-delegate to the slimmer build and must log a clear warning.
        var asmName = "TwinReverseBlock_" + Guid.NewGuid().ToString("N");
        var idSlimA = "RevBlockSlimA_" + Guid.NewGuid().ToString("N");
        var idFullB = "RevBlockFullB_" + Guid.NewGuid().ToString("N");

        var pathSlimA = EmitTwinProviderDll(root, "revblock_slim_a.dll", asmName, new Version(0, 3, 5, 0), tag: idSlimA, delta: 111, extraTypes: SlimmerTwinCoreTypes);
        var pathFullB = EmitTwinProviderDll(root, "revblock_full_b.dll", asmName, new Version(0, 3, 16, 0), tag: idFullB, delta: 222, extraTypes: FullerTwinCoreTypes);

        try
        {
            RegisterTestProvider(idSlimA, pathSlimA, canActivateBehavior: true);
            RegisterTestProvider(idFullB, pathFullB, canActivateBehavior: true);

            // Force slimmer A to already occupy the load context before B is requested.
            var context = AssemblyLoadContext.GetLoadContext(typeof(ManagedSkinModLoader).Assembly)!;
            Assembly loadedSlim;
            using (var stream = File.OpenRead(pathSlimA))
                loadedSlim = context.LoadFromStream(stream);
            var registerLoaded = typeof(ManagedSkinModLoader).GetMethod(
                "CacheLoadedProviderAssembly",
                BindingFlags.Static | BindingFlags.NonPublic)!;
            registerLoaded.Invoke(null, [pathSlimA, "fp-forced-slim-" + asmName, loadedSlim.FullName, loadedSlim]);

            var warnsBefore = warnLogs.Count;
            ManagedSkinModLoader.ActivateSelectedProviders([idFullB]);

            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idFullB) == idFullB,
                $"禁止完整版反向委托给精简版：actual={ManagedSkinModLoader.ResolveBehaviorProviderId(idFullB)}");
            Require(!ManagedSkinModLoader.IsBehaviorActiveFor(idFullB) &&
                    !ManagedSkinModLoader.IsProviderActive(idSlimA) &&
                    ComputeTwinBehaviorValue() == 0,
                $"精简版已占坑时不得反向激活精简版冒充完整版：val={ComputeTwinBehaviorValue()}");
            Require(warnLogs.Skip(warnsBefore).Any(line =>
                    line.Contains("拒绝反向委托", StringComparison.Ordinal) &&
                    line.Contains("行为可能不完整", StringComparison.Ordinal)),
                "完整版无法加载且被精简版占坑时必须打出明确的反向委托警告。");
        }
        finally
        {
            CleanupTestProviders([idSlimA, idFullB], [pathSlimA, pathFullB]);
        }
    }

    private static void VerifyTemplateCloneDifferentTypesDoesNotDelegate(DirectoryInfo root, List<string> warnLogs)
    {
        // Two unrelated mods compiled from the same template: identical assembly simple name,
        // culture, public key token, and sharing only Entry (1/9 = 0.111 < 0.80).
        // Selecting B MUST NOT execute A's behavior patches and must keep the conflict warning.
        var asmName = "TemplateCloneSkinMod_" + Guid.NewGuid().ToString("N");
        var idTemplateA = "TemplateModA_" + Guid.NewGuid().ToString("N");
        var idTemplateB = "TemplateModB_" + Guid.NewGuid().ToString("N");

        var pathA = EmitTwinProviderDll(root, "template_a.dll", asmName, new Version(1, 0, 0, 0), tag: idTemplateA, delta: 77,
            extraTypes: ["ModA.Feature1", "ModA.Feature2", "ModA.Feature3", "ModA.Feature4"]);
        var pathB = EmitTwinProviderDll(root, "template_b.dll", asmName, new Version(1, 1, 0, 0), tag: idTemplateB, delta: 88,
            extraTypes: ["ModB.Feature1", "ModB.Feature2", "ModB.Feature3", "ModB.Feature4"]);

        try
        {
            RegisterTestProvider(idTemplateA, pathA, canActivateBehavior: true);
            RegisterTestProvider(idTemplateB, pathB, canActivateBehavior: true);

            ManagedSkinModLoader.ActivateSelectedProviders([idTemplateA]);
            Require(ComputeTwinBehaviorValue() == 77, "模板 Mod A 自身激活应正常生效。");

            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            Require(ComputeTwinBehaviorValue() == 0, "切原版后 A 应停用。");

            var warnsBefore = warnLogs.Count;
            ManagedSkinModLoader.ActivateSelectedProviders([idTemplateB]);

            var overlap = ManagedSkinModLoader.GetAssemblyTypeOverlapRatio(pathA, pathB);
            Require(overlap < ManagedSkinModLoader.TwinAssemblyTypeOverlapThreshold,
                $"模板克隆不同 Mod 的类型重合度必须低于阈值：actual={overlap:F4}");
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idTemplateB) == idTemplateB,
                $"同名但类型集合不同的 Mod 绝不能委托执行别人的代码：actual={ManagedSkinModLoader.ResolveBehaviorProviderId(idTemplateB)}");
            Require(!ManagedSkinModLoader.IsBehaviorActiveFor(idTemplateB) &&
                    !ManagedSkinModLoader.IsProviderActive(idTemplateA) &&
                    ComputeTwinBehaviorValue() == 0,
                $"选中 B 时不得偷偷运行 A 的行为补丁：val={ComputeTwinBehaviorValue()}");
            Require(warnLogs.Skip(warnsBefore).Any(line => line.Contains("同名但内容不同", StringComparison.Ordinal)),
                "类型重合不足拒绝委托时必须保留原有的冲突警告。");
        }
        finally
        {
            CleanupTestProviders([idTemplateA, idTemplateB], [pathA, pathB]);
        }
    }

    private static void VerifySameSettingsContractLowOverlapSelectSmallerDoesNotRunUnselectedLarger(DirectoryInfo root)
    {
        // Exact blocking repro from Astra review:
        // Two DIFFERENT logical IDs sharing assembly simple name + identical SettingsVariantContract
        // (same SettingsTypeName, PropertyName, Labels), where B has MORE types than A, but
        // authored-type Jaccard overlap is low (2/10 = 0.20 < 0.80).
        // Selecting A alone MUST load and run A only; unselected B must have 0 initializations and stay inactive.
        var asmName = "TemplateContractSkinMod_" + Guid.NewGuid().ToString("N");
        var idA = "ContractModA_" + Guid.NewGuid().ToString("N");
        var idB = "ContractModB_" + Guid.NewGuid().ToString("N");

        var pathA = EmitTwinProviderDll(root, "contract_mod_a.dll", asmName, new Version(1, 0, 0, 0), tag: idA, delta: 13,
            extraTypes: ["SharedTemplate.AppearanceSettings", "ModAOnly.Extra1"]);
        var pathB = EmitTwinProviderDll(root, "contract_mod_b.dll", asmName, new Version(2, 0, 0, 0), tag: idB, delta: 29,
            extraTypes:
            [
                "SharedTemplate.AppearanceSettings",
                "ModBOnly.Extra1", "ModBOnly.Extra2", "ModBOnly.Extra3",
                "ModBOnly.Extra4", "ModBOnly.Extra5", "ModBOnly.Extra6", "ModBOnly.Extra7"
            ]);

        // Also test same-logical-ID re-export WITH identical settings contract: fuller copy still wins.
        var sameAsmName = "SameIdContractReexport_" + Guid.NewGuid().ToString("N");
        var sameBase = "SameIdContract_" + Guid.NewGuid().ToString("N");
        var sameSlim = sameBase + "::source:first";
        var sameFull = sameBase + "::source:second";
        var pathSameSlim = EmitTwinProviderDll(root, "same_contract_slim.dll", sameAsmName, new Version(1, 0, 0, 0), tag: sameSlim, delta: 41,
            extraTypes: ["SharedTemplate.AppearanceSettings"]);
        var pathSameFull = EmitTwinProviderDll(root, "same_contract_full.dll", sameAsmName, new Version(1, 1, 0, 0), tag: sameFull, delta: 53,
            extraTypes: ["SharedTemplate.AppearanceSettings", "SameFull.WearStage1", "SameFull.WearStage2"]);

        var contractsField = (IDictionary)AccessTools.Field(typeof(ProviderSettingsVariantOptions), "Contracts")!.GetValue(null)!;
        var bindingsField = (IDictionary)AccessTools.Field(typeof(ProviderSettingsVariantOptions), "Bindings")!.GetValue(null)!;
        var contractType = typeof(ProviderSettingsVariantOptions).GetNestedType("SettingsVariantContract", BindingFlags.NonPublic)!;
        var bindingType = typeof(ProviderSettingsVariantOptions).GetNestedType("SettingsVariantBinding", BindingFlags.NonPublic)!;
        var addedContractKeys = new List<string>();
        var addedBindingKeys = new List<string>();

        void RegisterFakeContract(string providerId, string assemblyPath, string assemblySimpleName)
        {
            var contract = Activator.CreateInstance(
                contractType,
                assemblyPath,
                assemblySimpleName,
                providerId,
                "SharedTemplate.AppearanceSettings",
                "Current",
                false,
                "SharedTemplate.AppearanceSettings",
                "Appearance",
                false,
                (IReadOnlyList<int>)Array.Empty<int>(),
                null,
                (IReadOnlyList<string>)new[] { "Default", "Alt" },
                0)!;
            var contractKey = assemblyPath + "|" + providerId;
            contractsField[contractKey] = contract;
            addedContractKeys.Add(contractKey);
            bindingsField[providerId] = Activator.CreateInstance(bindingType, contract, 0)!;
            addedBindingKeys.Add(providerId);
        }

        try
        {
            RegisterTestProvider(idA, pathA, canActivateBehavior: true);
            RegisterTestProvider(idB, pathB, canActivateBehavior: true);
            RegisterFakeContract(idA, pathA, asmName);
            RegisterFakeContract(idB, pathB, asmName);

            RegisterTestProvider(sameSlim, pathSameSlim, canActivateBehavior: true);
            RegisterTestProvider(sameFull, pathSameFull, canActivateBehavior: true);
            RegisterFakeContract(sameSlim, pathSameSlim, sameAsmName);
            RegisterFakeContract(sameFull, pathSameFull, sameAsmName);

            // Confirm the fixture genuinely triggers the old unfiltered FindRicherSiblingAssemblyPath.
            var unfiltered = ProviderSettingsVariantOptions.FindRicherSiblingAssemblyPath(pathA);
            Require(string.Equals(unfiltered, pathB, StringComparison.OrdinalIgnoreCase),
                "夹具必须真实命中旧版无过滤 FindRicherSiblingAssemblyPath，以证明阻塞路径已被收拢。");
            var overlap = ManagedSkinModLoader.GetAssemblyTypeOverlapRatio(pathA, pathB);
            Require(overlap < ManagedSkinModLoader.TwinAssemblyTypeOverlapThreshold,
                $"低重合模板包重合度必须低于 0.80：actual={overlap:F3}");

            // Selecting smaller A alone MUST initialize & activate ONLY A, NEVER unselected larger B!
            ManagedSkinModLoader.ActivateSelectedProviders([idA]);
            Require(InitCounts.GetValueOrDefault(idB) == 0,
                $"只选 A 时，类型重合不足（{overlap:F2}）且设置合约相同的未选包 B 初始化次数必须为 0，实际={InitCounts.GetValueOrDefault(idB)}。");
            Require(InitCounts.GetValueOrDefault(idA) == 1,
                $"只选 A 时应初始化 A 一次，实际={InitCounts.GetValueOrDefault(idA)}。");
            Require(ManagedSkinModLoader.IsProviderActive(idA) && !ManagedSkinModLoader.IsProviderActive(idB),
                "只选 A 时必须 ACTIVE_A=True、ACTIVE_B=False。");
            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idA) == idA && ComputeTwinBehaviorValue() == 13,
                $"只选 A 时必须运行 A 自身行为（13），actual={ComputeTwinBehaviorValue()}。");
            ManagedSkinModLoader.DeactivateProvidersExcept([]);

            // Same-logical-ID re-export with shared settings contract still activates the fuller copy.
            ManagedSkinModLoader.ActivateSelectedProviders([sameSlim]);
            Require(InitCounts.GetValueOrDefault(sameFull) == 1 &&
                    InitCounts.GetValueOrDefault(sameSlim) == 0 &&
                    ManagedSkinModLoader.IsBehaviorActiveFor(sameSlim) &&
                    ManagedSkinModLoader.IsProviderActive(sameFull) &&
                    ComputeTwinBehaviorValue() == 53,
                $"同逻辑 ID 重导出（设置合约相同）仍应优先运行更完整的同 ID 副本（53）：val={ComputeTwinBehaviorValue()}, initFull={InitCounts.GetValueOrDefault(sameFull)}");
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
        }
        finally
        {
            foreach (var key in addedBindingKeys) bindingsField.Remove(key);
            foreach (var key in addedContractKeys) contractsField.Remove(key);
            CleanupTestProviders([idA, idB, sameSlim, sameFull], [pathA, pathB, pathSameSlim, pathSameFull]);
        }
    }

    private static int _delegatedCharReplayCount;
    private static int _delegatedRoomReplayCount;
    private static readonly Dictionary<string, int> DelegatedRoomReplayProviders = new(StringComparer.OrdinalIgnoreCase);

    private static void DelegatedCharPresentationCallback(Control __instance, NCharacterSelectButton button) =>
        _delegatedCharReplayCount++;

    private static void DelegatedRoomReadyCallback(Node __instance) =>
        _delegatedRoomReplayCount++;

    private static bool SkipGodotCharacterPresentationState() => false;
    private static bool StubEmptyCharacterState(ref object __result)
    {
        var method = AccessTools.Method(typeof(ManagedSkinModLoader), "CaptureCharacterPresentationState")!;
        __result = Activator.CreateInstance(method.ReturnType)!;
        return false;
    }
    private static bool StubIsInstanceValidTrue(ref bool __result)
    {
        __result = true;
        return false;
    }
    private static bool StubReplayNodeReadyBehavior(string providerId, Node node, bool includePrefixes, ref IReadOnlyList<Node> __result)
    {
        var resolved = ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);
        DelegatedRoomReplayProviders[resolved] = DelegatedRoomReplayProviders.GetValueOrDefault(resolved) + 1;
        _delegatedRoomReplayCount++;
        __result = [];
        return false;
    }

    private static void VerifyDelegatedCallbacksAndRunEnvironmentAuthorization(DirectoryInfo root)
    {
        var asmName = "TwinCallbackAuth_" + Guid.NewGuid().ToString("N");
        var idSlimA = "CbSlimA_" + Guid.NewGuid().ToString("N");
        var idFullB = "CbFullB_" + Guid.NewGuid().ToString("N");
        var pathSlimA = EmitTwinProviderDll(root, "cb_slim_a.dll", asmName, new Version(0, 3, 5, 0), tag: idSlimA, delta: 10, extraTypes: SlimmerTwinCoreTypes);
        var pathFullB = EmitTwinProviderDll(root, "cb_full_b.dll", asmName, new Version(0, 3, 16, 0), tag: idFullB, delta: 20, extraTypes: FullerTwinCoreTypes);

        var harmony = new Harmony("sc.test.twin.callbacks." + Guid.NewGuid().ToString("N"));
        _delegatedCharReplayCount = 0;
        _delegatedRoomReplayCount = 0;
        DelegatedRoomReplayProviders.Clear();
        try
        {
            RegisterTestProvider(idSlimA, pathSlimA, canActivateBehavior: true);
            RegisterTestProvider(idFullB, pathFullB, canActivateBehavior: true);

            // Prevent headless Godot calls inside ScheduleRunEnvironmentPresentationRefresh / character state capture.
            harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "ScheduleRunEnvironmentPresentationRefresh"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SkipGodotCharacterPresentationState)));
            harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "RestoreCharacterPresentation",
                    [typeof(ManagedSkinModLoader).GetNestedType("ActiveProviderRuntime", BindingFlags.NonPublic)!, typeof(Control)]),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SkipGodotCharacterPresentationState)));
            harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "CaptureCharacterPresentationState"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(StubEmptyCharacterState)));
            harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "TrackCharacterPresentationMutation"),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SkipGodotCharacterPresentationState)));

            // 1) Run-environment authorization configured for A BEFORE A->B alias even exists (exact SkinService.cs:1714-1735 order).
            ManagedSkinModLoader.ConfigureRunEnvironmentProviders([idSlimA]);
            ManagedSkinModLoader.ActivateSelectedProviders([idSlimA]);

            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idSlimA) == idFullB &&
                    ManagedSkinModLoader.IsProviderActive(idFullB),
                "A 选中后必须成功委托并激活执行者 B。");

            var activeRuntimes = (IDictionary)AccessTools.Field(typeof(ManagedSkinModLoader), "ActiveProviderRuntimes")!.GetValue(null)!;
            var runtimeB = activeRuntimes[idFullB]!;
            var runEnvEnabledProp = runtimeB.GetType().GetProperty("RunEnvironmentEnabled")!;
            Require((bool)runEnvEnabledProp.GetValue(runtimeB)! &&
                    ManagedSkinModLoader.IsRunEnvironmentAuthorizedForBehavior(idFullB),
                "先按 A 配置地区授权、再建立 A->B 委托时，执行者 B 必须获得地区行为授权。");

            var fullAsm = ManagedSkinModLoader.GetActiveProviderAssembly(idSlimA)!;
            var runEnvPrefix = AccessTools.Method(typeof(ManagedSkinModLoader), "RunEnvironmentCallbackPrefix")!;
            var entryMethodB = fullAsm.GetTypes().First().GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static)!;
            Require((bool)runEnvPrefix.Invoke(null, [entryMethodB])!,
                "A 持有地区授权时，RunEnvironmentCallbackPrefix 必须放行执行者 B 的程序集回调。");

            // Revoking back to vanilla must disable B's run-environment authorization.
            ManagedSkinModLoader.ConfigureRunEnvironmentProviders([]);
            Require(!(bool)runEnvEnabledProp.GetValue(runtimeB)! &&
                    !ManagedSkinModLoader.IsRunEnvironmentAuthorizedForBehavior(idFullB) &&
                    !(bool)runEnvPrefix.Invoke(null, [entryMethodB])!,
                "切回原版清空地区授权后，执行者 B 的地区行为授权必须撤销。");

            // Re-authorizing A after alias already exists must re-enable B.
            ManagedSkinModLoader.ConfigureRunEnvironmentProviders([idSlimA]);
            Require((bool)runEnvEnabledProp.GetValue(runtimeB)! &&
                    (bool)runEnvPrefix.Invoke(null, [entryMethodB])!,
                "重新为 A 授权后，执行者 B 的地区行为授权必须重新生效。");

            // 2) Character-select presentation replay invoked with A must hit B and run afterReplay.
            var providerPatchType = typeof(ManagedSkinModLoader).GetNestedType("ProviderPatch", BindingFlags.NonPublic)!;
            var patchKindEnum = typeof(ManagedSkinModLoader).GetNestedType("ProviderPatchKind", BindingFlags.NonPublic)!;
            var postfixKind = Enum.Parse(patchKindEnum, "Postfix");
            var selectTarget = AccessTools.Method(typeof(NCharacterSelectScreen), "SelectCharacter")
                               ?? typeof(NCharacterSelectScreen).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).First(m => m.DeclaringType == typeof(NCharacterSelectScreen));
            var charCallback = AccessTools.Method(typeof(TwinAssemblyBehaviorDelegationTests), nameof(DelegatedCharPresentationCallback))!;
            var syntheticCharPatch = Activator.CreateInstance(
                providerPatchType,
                selectTarget,
                charCallback,
                postfixKind,
                400,
                Array.Empty<string>(),
                Array.Empty<string>(),
                false)!;
            var charPatchList = Array.CreateInstance(providerPatchType, 1);
            charPatchList.SetValue(syntheticCharPatch, 0);
            var charPatchesField = runtimeB.GetType().GetField("<CharacterPresentationPatches>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                                   ?? throw new InvalidOperationException("找不到 CharacterPresentationPatches 后备字段。");
            charPatchesField.SetValue(runtimeB, charPatchList);

            var dummyScreen = FakeGodot<NCharacterSelectScreen>();
            var dummyButton = FakeGodot<NCharacterSelectButton>();
            var concreteCharType = typeof(CharacterModel).Assembly.GetTypes()
                .First(t => !t.IsAbstract && typeof(CharacterModel).IsAssignableFrom(t));
            var dummyChar = (CharacterModel)RuntimeHelpers.GetUninitializedObject(concreteCharType);
            var afterReplayRuns = 0;
            ManagedSkinModLoader.ReplaySelectedCharacterPresentation(idSlimA, dummyScreen, dummyButton, dummyChar, () => afterReplayRuns++);
            Require(_delegatedCharReplayCount == 1 && afterReplayRuns == 1,
                $"以精简包 A 触发选角重放时必须命中执行者 B 并执行 afterReplay：charReplay={_delegatedCharReplayCount}, afterReplay={afterReplayRuns}");

            // 3) Room-ready filtered replay invoked with onlyProviderId = A must match B.
            var roomTarget = AccessTools.Method(typeof(Node), nameof(Node._Ready))!;
            var roomCallback = AccessTools.Method(typeof(TwinAssemblyBehaviorDelegationTests), nameof(DelegatedRoomReadyCallback))!;
            var syntheticRoomPatch = Activator.CreateInstance(
                providerPatchType,
                roomTarget,
                roomCallback,
                postfixKind,
                400,
                Array.Empty<string>(),
                Array.Empty<string>(),
                false)!;
            var roomPatchList = Array.CreateInstance(providerPatchType, 1);
            roomPatchList.SetValue(syntheticRoomPatch, 0);
            var roomPatchesField = runtimeB.GetType().GetField("<NodeReadyPresentationPatches>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic)
                                   ?? throw new InvalidOperationException("找不到 NodeReadyPresentationPatches 后备字段。");
            roomPatchesField.SetValue(runtimeB, roomPatchList);

            harmony.Patch(AccessTools.Method(typeof(GodotObject), nameof(GodotObject.IsInstanceValid)),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(StubIsInstanceValidTrue)));
            harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), nameof(ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior)),
                prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(StubReplayNodeReadyBehavior)));

            var dummyRoom = FakeGodot<Node>();
            var roomResults = ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(dummyRoom, onlyProviderId: idSlimA);
            Require(roomResults.Count == 1 &&
                    roomResults[0].ProviderId.Equals(idFullB, StringComparison.OrdinalIgnoreCase) &&
                    _delegatedRoomReplayCount == 1 &&
                    DelegatedRoomReplayProviders.GetValueOrDefault(idFullB) == 1,
                $"以精简包 A 过滤房间热切换重放时必须命中执行者 B：count={roomResults.Count}, replays={_delegatedRoomReplayCount}");

            // Switching back to vanilla deactivates B so subsequent char/room replays do nothing.
            ManagedSkinModLoader.ConfigureRunEnvironmentProviders([]);
            ManagedSkinModLoader.DeactivateProvidersExcept([]);
            ManagedSkinModLoader.ReplaySelectedCharacterPresentation(idSlimA, dummyScreen, dummyButton, dummyChar, () => afterReplayRuns++);
            var afterVanillaRooms = ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(dummyRoom, onlyProviderId: idSlimA);
            Require(_delegatedCharReplayCount == 1 && afterReplayRuns == 1 && afterVanillaRooms.Count == 0,
                "切回原版停用 B 后，选角与房间重放必须停止生效。");
        }
        finally
        {
            try { ManagedSkinModLoader.ConfigureRunEnvironmentProviders([]); } catch { }
            CleanupTestProviders([idSlimA, idFullB], [pathSlimA, pathFullB]);
            harmony.UnpatchAll(harmony.Id);
            _delegatedCharReplayCount = 0;
            _delegatedRoomReplayCount = 0;
            DelegatedRoomReplayProviders.Clear();
        }
    }

    private static void VerifyUnregisteredCollisionStillWarnsAndStaysResourceOnly(DirectoryInfo root, List<string> warnLogs)
    {
        var asmName = "TwinUnregisteredHost_" + Guid.NewGuid().ToString("N");
        var idOrphan = "OrphanPack_" + Guid.NewGuid().ToString("N");

        var hostPath = EmitTwinProviderDll(root, "unregistered_host.dll", asmName, new Version(1, 0, 0, 0), tag: "host_" + asmName, delta: 500, extraTypes: FullerTwinCoreTypes);
        var orphanPath = EmitTwinProviderDll(root, "orphan.dll", asmName, new Version(2, 0, 0, 0), tag: idOrphan, delta: 600, extraTypes: SlimmerTwinCoreTypes);

        try
        {
            var context = AssemblyLoadContext.GetLoadContext(typeof(ManagedSkinModLoader).Assembly)!;
            using (var stream = File.OpenRead(hostPath))
                _ = context.LoadFromStream(stream);

            RegisterTestProvider(idOrphan, orphanPath, canActivateBehavior: true);
            var warnsBefore = warnLogs.Count;
            ManagedSkinModLoader.ActivateSelectedProviders([idOrphan]);

            Require(ManagedSkinModLoader.ResolveBehaviorProviderId(idOrphan) == idOrphan,
                "冲突程序集不属于任何已登记提供者时不得建立委托。");
            Require(!ManagedSkinModLoader.IsBehaviorActiveFor(idOrphan) && ComputeTwinBehaviorValue() == 0,
                "找不到可委托姊妹包时必须保持只接管资源、不执行冲突 DLL。");
            Require(warnLogs.Skip(warnsBefore).Any(line => line.Contains("同名但内容不同", StringComparison.Ordinal)),
                "找不到可委托姊妹包时必须保留原来的冲突警告日志。");
        }
        finally
        {
            CleanupTestProviders([idOrphan], [orphanPath]);
        }
    }

    private static string EmitTwinProviderDll(
        DirectoryInfo root,
        string fileName,
        string assemblySimpleName,
        Version version,
        string tag,
        int delta,
        string[]? extraTypes = null)
    {
        var path = Path.Combine(root.FullName, fileName);
        var asmName = new AssemblyName(assemblySimpleName) { Version = version };
        var builder = new PersistedAssemblyBuilder(asmName, typeof(object).Assembly);
        var module = builder.DefineDynamicModule(fileName);
        var type = module.DefineType(
            assemblySimpleName + ".Entry",
            TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);

        var attrCtor = typeof(ModInitializerAttribute).GetConstructor([typeof(string)])!;
        type.SetCustomAttribute(new CustomAttributeBuilder(attrCtor, ["Initialize"]));

        var postfix = type.DefineMethod(
            "Postfix",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            [typeof(int).MakeByRefType()]);
        postfix.DefineParameter(1, ParameterAttributes.None, "__result");
        var postIl = postfix.GetILGenerator();
        postIl.Emit(OpCodes.Ldarg_0);
        postIl.Emit(OpCodes.Ldarg_0);
        postIl.Emit(OpCodes.Ldind_I4);
        postIl.Emit(OpCodes.Ldc_I4, delta);
        postIl.Emit(OpCodes.Add);
        postIl.Emit(OpCodes.Stind_I4);
        postIl.Emit(OpCodes.Ret);

        var init = type.DefineMethod(
            "Initialize",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(void),
            Type.EmptyTypes);
        var initIl = init.GetILGenerator();
        initIl.Emit(OpCodes.Ldstr, tag);
        initIl.Emit(OpCodes.Ldtoken, type);
        initIl.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
        initIl.Emit(OpCodes.Call, typeof(TwinAssemblyBehaviorDelegationTests).GetMethod(
            nameof(OnTestProviderInitialized),
            BindingFlags.Public | BindingFlags.Static)!);
        initIl.Emit(OpCodes.Ret);

        type.CreateType();

        if (extraTypes != null)
        {
            foreach (var extraFullName in extraTypes)
            {
                var extra = module.DefineType(
                    extraFullName,
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
                extra.CreateType();
            }
        }

        builder.Save(path);
        return path;
    }

    private static void RegisterTestProvider(
        string providerId,
        string assemblyPath,
        bool canActivateBehavior,
        string? registerSourceIdentity = null)
    {
        var loader = typeof(ManagedSkinModLoader);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var providers = (IDictionary)loader.GetField("ProviderAssemblies", flags)!.GetValue(null)!;
        var providerType = loader.GetNestedType("ProviderAssembly", BindingFlags.NonPublic)!;
        providers[providerId] = Activator.CreateInstance(
            providerType,
            assemblyPath,
            providerId,
            false,
            false,
            canActivateBehavior,
            providerId)!;

        if (registerSourceIdentity != null)
        {
            var sources = (IDictionary)loader.GetField("ProviderAssemblySourcesByIdentity", flags)!.GetValue(null)!;
            var sourceType = loader.GetNestedType("ProviderAssemblySourceIdentity", BindingFlags.NonPublic)!;
            sources[registerSourceIdentity] = Activator.CreateInstance(
                sourceType,
                "fp-" + providerId,
                assemblyPath,
                providerId)!;
        }
    }

    private static void CleanupTestProviders(
        IEnumerable<string> providerIds,
        IEnumerable<string> assemblyPaths,
        IEnumerable<string>? identities = null)
    {
        try { ManagedSkinModLoader.DeactivateProvidersExcept([]); } catch { }
        var loader = typeof(ManagedSkinModLoader);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var providers = (IDictionary)loader.GetField("ProviderAssemblies", flags)!.GetValue(null)!;
        var aliases = (IDictionary)loader.GetField("BehaviorProviderAliases", flags)!.GetValue(null)!;
        var blueprints = (IDictionary)loader.GetField("ProviderRuntimeBlueprints", flags)!.GetValue(null)!;
        var loaded = (IDictionary)loader.GetField("LoadedProviderAssemblies", flags)!.GetValue(null)!;
        var sources = (IDictionary)loader.GetField("ProviderAssemblySourcesByIdentity", flags)!.GetValue(null)!;

        foreach (var id in providerIds)
        {
            providers.Remove(id);
            aliases.Remove(id);
            blueprints.Remove(id);
        }

        foreach (var path in assemblyPaths)
        {
            loaded.Remove(path);
        }

        if (identities != null)
        {
            foreach (var identity in identities)
            {
                sources.Remove(identity);
            }
        }
    }

    private static Harmony CaptureModLog(List<string> infos, List<string> warns)
    {
        _capturedInfos = infos;
        _capturedWarns = warns;
        var harmony = new Harmony("sc.test.twin.log." + Guid.NewGuid().ToString("N"));
        harmony.Patch(AccessTools.Method(typeof(ModLog), nameof(ModLog.Info)),
            prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(CaptureInfo)));
        harmony.Patch(AccessTools.Method(typeof(ModLog), nameof(ModLog.Warn)),
            prefix: new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(CaptureWarn)));
        // Headless RuntimeTests has no Godot native SceneTree; skip only the SceneTree node-monitor
        // helpers while keeping 100% of ManagedSkinModLoader activation/deactivation/blueprint logic real.
        var skip = new HarmonyMethod(typeof(TwinAssemblyBehaviorDelegationTests), nameof(SkipHeadlessGodotCall));
        harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "EnsureProviderNodeMonitor"), prefix: skip);
        harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "SuspendProviderNodes"), prefix: skip);
        harmony.Patch(AccessTools.Method(typeof(ManagedSkinModLoader), "RestoreProviderNodes"), prefix: skip);
        return harmony;
    }

    private static bool SkipHeadlessGodotCall() => false;

    private static List<string>? _capturedInfos;
    private static List<string>? _capturedWarns;

    private static bool CaptureInfo(string message)
    {
        _capturedInfos?.Add(message);
        return false;
    }

    private static bool CaptureWarn(string message)
    {
        _capturedWarns?.Add(message);
        return false;
    }

    private static T FakeGodot<T>() where T : class
    {
        var obj = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        GC.SuppressFinalize(obj);
        return obj;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}

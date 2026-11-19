using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger;
using STS2SkinChanger.Core;

internal static class CreatureVisualLifecycleTests
{
    private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static Node _owner = null!;
    private static Node _oldParent = null!;
    private static Node _oldVisuals = null!;
    private static NCreatureVisuals _newVisuals = null!;
    private static FieldInfo _visualsField = null!;
    private static bool _detached;
    private static bool _attached;
    private static int _finishingCalls;

    internal static void Run()
    {
        CheckCreaturePostfixDiscovery();
        CheckNativeReadyParent();
        CheckLocalCreatureFinishing();
        CheckCompanionRevealEligibility();
        CheckLowMemoryFallback();
        CheckCombatCleanupDeferred();
        Console.WriteLine("Creature visual lifecycle passed: provider finishing, native Ready parent, low-memory fallback and deferred cleanup.");
    }

    private static Creature _pressureCreature = null!;
    private static NCreature _nativeReadyFinished = null!;
    private static NCreatureVisuals _nativeVisual = null!;
    private static Exception? _nativeError;
    private static bool _failFinishing;
    private static int _readyReplays;
    private static int _pressureAttempts;

    private static void CheckLowMemoryFallback()
    {
        var create = AccessTools.Method(typeof(Creature), nameof(Creature.CreateVisuals));
        var ready = AccessTools.Method(typeof(NCreature), nameof(NCreature._Ready));
        var harmony = new Harmony("Gurio.SkinChanger.Tests.CombatAppearanceFallback");
        var log = AuditLogSilencer.Install();
        try
        {
            harmony.Patch(create, transpiler: TestPatch(nameof(UseNativeCreateFixture)));
            harmony.Patch(ready, transpiler: TestPatch(nameof(UseNativeReadyFixture)));
            harmony.CreateClassProcessor(typeof(MultiplayerCreatureVisualScopePatch)).Patch();
            harmony.CreateClassProcessor(typeof(MultiplayerCreatureReadyScopePatch)).Patch();
            harmony.Patch(AccessTools.Method(typeof(MultiplayerSkinSync), "BeginCreatureRuntimeScope"),
                prefix: TestPatch(nameof(OpenOptionalScope)));
            var controls = typeof(Entry).Assembly.GetType("STS2SkinChanger.Ui.ContextualSkinControls", true)!;
            harmony.Patch(AccessTools.Method(controls, "ApplySelectedCreatureVisualPostfix"),
                prefix: TestPatch(nameof(SimulateOptionalFinishing)));

            harmony.Patch(AccessTools.Method(typeof(STS2SkinChanger.Ui.CharacterAppearanceRuntime),
                    "ReplaySelectedCreatureNodeReady", [typeof(NCreature)]),
                prefix: TestPatch(nameof(SimulateReadyReplay)));

            _pressureCreature = (Creature)RuntimeHelpers.GetUninitializedObject(typeof(Creature));
            var nextCreature = (Creature)RuntimeHelpers.GetUninitializedObject(typeof(Creature));
            _pressureAttempts = _readyReplays = 0;
            _nativeVisual = CreateTestVisual();
            var visual = (NCreatureVisuals)create.Invoke(_pressureCreature, null)!;
            Require(ReferenceEquals(visual, _nativeVisual) && CombatAppearanceBoundary.IsFallback(visual),
                "创建资源范围被内存守卫拒绝时，CreateVisuals 必须返回游戏创建的模型。");

            var node = CreateTestCreatureNode(_pressureCreature, visual);
            ready.Invoke(node, null);
            Require(ReferenceEquals(_nativeReadyFinished, node) && _pressureAttempts == 1,
                "已经降级的模型必须继续完成原生 Ready，且不能再次申请被拒绝的范围。");

            node = CreateTestCreatureNode(_pressureCreature, CreateTestVisual());
            ready.Invoke(node, null);
            Require(ReferenceEquals(_nativeReadyFinished, node) && _pressureAttempts == 2,
                "首次在 Ready 申请范围被内存守卫拒绝，也必须继续完成原生 Ready。");

            _failFinishing = true;
            _nativeVisual = CreateTestVisual();
            visual = (NCreatureVisuals)create.Invoke(nextCreature, null)!;
            Require(ReferenceEquals(visual, _nativeVisual) && CombatAppearanceBoundary.IsFallback(visual),
                "生物级换肤后置失败时，必须保留游戏已有的模型。");

            _failFinishing = false;
            _nativeVisual = CreateTestVisual();
            visual = (NCreatureVisuals)create.Invoke(nextCreature, null)!;
            Require(ReferenceEquals(visual, _nativeVisual) && !CombatAppearanceBoundary.IsFallback(visual),
                "下一个单位必须独立创建模型，不能继承前一个单位的降级状态。");

            Require(_readyReplays == 0, "降级后必须跳过给模型重放的外观回调。");
            var healthyNode = CreateTestCreatureNode(nextCreature, visual);
            ready.Invoke(healthyNode, null);
            Require(ReferenceEquals(_nativeReadyFinished, healthyNode) && _readyReplays == 1,
                "自身的 Ready 回调失败时，原生 Ready 已完成，回调失败可独立降级。");

            _nativeError = new InvalidOperationException("native setup failed");
            CheckNativeError(create, _pressureCreature);
            CheckNativeError(ready, node);
            Require(!CombatAppearanceBoundary.SkipAppearance, "原版失败后也必须退出当前外观范围。");
        }
        finally
        {
            _nativeError = null;
            _failFinishing = false;
            harmony.UnpatchAll(harmony.Id);
            log.UnpatchAll(log.Id);
        }
    }

    private static void CheckNativeError(MethodInfo method, object owner)
    {
        try
        {
            method.Invoke(owner, null);
            throw new InvalidOperationException("游戏原生异常被换肤补丁吞掉。");
        }
        catch (TargetInvocationException error) when (ReferenceEquals(error.InnerException, _nativeError)) { }
    }

    private static NCreatureVisuals CreateTestVisual()
    {
        var visual = (NCreatureVisuals)RuntimeHelpers.GetUninitializedObject(typeof(NCreatureVisuals));
        GC.SuppressFinalize(visual);
        return visual;
    }

    private static NCreature CreateTestCreatureNode(Creature entity, NCreatureVisuals visual)
    {
        var node = (NCreature)RuntimeHelpers.GetUninitializedObject(typeof(NCreature));
        GC.SuppressFinalize(node);
        _visualsField.SetValue(node, visual);
        AccessTools.Field(typeof(NCreature), "<Entity>k__BackingField").SetValue(node, entity);
        return node;
    }

    private static HarmonyMethod TestPatch(string method) => new(typeof(CreatureVisualLifecycleTests), method);

    private static IEnumerable<CodeInstruction> UseNativeCreateFixture(IEnumerable<CodeInstruction> instructions) =>
        [new(OpCodes.Call, AccessTools.Method(typeof(CreatureVisualLifecycleTests), nameof(NativeCreate))), new(OpCodes.Ret)];

    private static IEnumerable<CodeInstruction> UseNativeReadyFixture(IEnumerable<CodeInstruction> instructions) =>
        [new(OpCodes.Ldarg_0),
            new(OpCodes.Call, AccessTools.Method(typeof(CreatureVisualLifecycleTests), nameof(NativeReady))),
            new(OpCodes.Ret)];

    private static NCreatureVisuals NativeCreate() => _nativeError == null ? _nativeVisual : throw _nativeError;

    private static void NativeReady(NCreature owner)
    {
        if (_nativeError != null) throw _nativeError;
        _nativeReadyFinished = owner;
    }

    private static bool OpenOptionalScope(Creature creature, ref IDisposable? __result)
    {
        if (ReferenceEquals(creature, _pressureCreature))
        {
            _pressureAttempts++;
            throw new AppearanceResourceUnavailableException("test: low appearance memory");
        }
        __result = null;
        return false;
    }

    private static bool SimulateOptionalFinishing() =>
        _failFinishing ? throw new AppearanceResourceUnavailableException("test: low memory in finishing") : false;

    private static bool SimulateReadyReplay()
    {
        _readyReplays++;
        throw new AppearanceResourceUnavailableException("test: low memory in Ready replay");
    }

    private static void CheckCombatCleanupDeferred()
    {
        var combat = AccessTools.Field(typeof(STS2SkinChanger.Ui.CharacterAppearanceRuntime), "_combatRuntimeScopeActive");
        var pending = AccessTools.Field(typeof(SkinService), "_runtimePressureCleanupPending");
        var previousCombat = combat.GetValue(null);
        var previousPending = pending.GetValue(null);
        try
        {
            combat.SetValue(null, true);
            pending.SetValue(null, false);
            AccessTools.Method(typeof(SkinService), "RequestRuntimePressureCleanup").Invoke(null, null);
            SkinService.TryRunDeferredRuntimePressureCleanup();
            Require((bool)pending.GetValue(null)!, "战斗中的资源压力只应登记清理请求，不能清空缓存。");
        }
        finally
        {
            combat.SetValue(null, previousCombat);
            pending.SetValue(null, previousPending);
        }
    }

    private static void CheckCompanionRevealEligibility()
    {
        var loader = typeof(Entry).Assembly.GetType("STS2SkinChanger.Core.ManagedSkinModLoader", true)!;
        var eligible = loader.GetMethod("IsCompanionRevealPresentationPatch", Static)
            ?? throw new InvalidOperationException("热替换活着的随从必须补齐被登场流程隐藏后的显示初始化。");
        var patchType = loader.GetNestedType("ProviderPatch", BindingFlags.NonPublic)!;
        var kindType = loader.GetNestedType("ProviderPatchKind", BindingFlags.NonPublic)!;
        var callback = typeof(CreatureFinishingPatch).GetMethod("Postfix", Static)!;
        foreach (var (method, kind, pet, dead, hidden, expected) in new[]
        {
            ("StartReviveAnim", "Prefix", true, false, true, true),
            ("StartReviveAnim", "Prefix", false, false, true, false),
            ("StartReviveAnim", "Prefix", true, true, true, false),
            ("StartReviveAnim", "Prefix", true, false, false, false),
            ("StartReviveAnim", "Postfix", true, false, true, false),
            ("_Ready", "Prefix", true, false, true, false)
        })
        {
            var patch = Activator.CreateInstance(patchType,
                AccessTools.Method(typeof(NCreature), method), callback, Enum.Parse(kindType, kind),
                Priority.Normal, Array.Empty<string>(), Array.Empty<string>(), false)!;
            Require((bool)eligible.Invoke(null, [patch, pet, dead, hidden])! == expected,
                "随从显示补全只能用于活着且主模型被隐藏的随从，不得复活死亡单位或重复执行正常登场。");
        }
    }

    private static void CheckLocalCreatureFinishing()
    {
        var assembly = typeof(Entry).Assembly;
        var controls = assembly.GetType("STS2SkinChanger.Ui.ContextualSkinControls", true)!;
        var finish = controls.GetMethod("ApplySelectedCreatureVisualPostfix", Static)!;
        var patch = assembly.GetType("STS2SkinChanger.Core.MultiplayerCreatureVisualScopePatch", true)!;
        var postfix = patch.GetMethod("Postfix", Static)!;
        var harmony = new Harmony("Gurio.SkinChanger.Tests.LocalCreatureFinishing");
        try
        {
            _finishingCalls = 0;
            harmony.Patch(finish, prefix: new HarmonyMethod(typeof(CreatureVisualLifecycleTests), nameof(CaptureFinishing)));
            var creature = RuntimeHelpers.GetUninitializedObject(typeof(Creature));
            postfix.Invoke(null, [creature, _newVisuals, null]);
            Require(_finishingCalls == 1, "没有多人资源范围的本机生物也必须且只能执行一次选中皮肤的生物级收尾。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static bool CaptureFinishing(Creature creature, ref NCreatureVisuals? visuals)
    {
        Require(ReferenceEquals(visuals, _newVisuals), "收尾必须操作最终模型，不能又从原版工厂创建一个。");
        _finishingCalls++;
        return false;
    }

    private static void CheckCreaturePostfixDiscovery()
    {
        var loader = typeof(Entry).Assembly.GetType("STS2SkinChanger.Core.ManagedSkinModLoader", true)!;
        var discover = loader.GetMethod("DiscoverVisualPostfixes", Static)!;
        var discovered = ((System.Collections.IEnumerable)discover.Invoke(null, [typeof(CreatureVisualLifecycleTests).Assembly])!)
            .Cast<object>().ToArray();
        var callback = typeof(CreatureFinishingPatch).GetMethod("Postfix", Static)!;
        Require(discovered.Any(item => Equals(item.GetType().GetProperty("Method")!.GetValue(item), callback)),
            "Creature.CreateVisuals 的收尾必须被发现，否则删除鬼火等操作永远不会重放。");
        var patchType = loader.GetNestedType("ProviderPatch", BindingFlags.NonPublic)!;
        var kindType = loader.GetNestedType("ProviderPatchKind", BindingFlags.NonPublic)!;
        var target = AccessTools.Method(typeof(Creature), nameof(Creature.CreateVisuals));
        var ownership = loader.GetMethod("IsManagedResourceOwnershipPatch", Static)!;
        foreach (var kind in new[] { "Postfix", "Prefix" })
        {
            var patch = Activator.CreateInstance(patchType,
                target, callback, Enum.Parse(kindType, kind), Priority.Normal,
                Array.Empty<string>(), Array.Empty<string>(), false)!;
            Require((bool)ownership.Invoke(null, [patch])! == (kind == "Postfix"),
                "可重放的 Creature 收尾必须停止全局执行，且不能误停不支持重放的前置行为。");
        }
        var stateful = Activator.CreateInstance(patchType, target,
            typeof(StatefulCreaturePatch).GetMethod("Postfix", Static)!, Enum.Parse(kindType, "Postfix"),
            Priority.Normal, Array.Empty<string>(), Array.Empty<string>(), false)!;
        Require(!(bool)ownership.Invoke(null, [stateful])!,
            "需要原生前置 __state 的回调不可脱离原调用重放，不能误停后丢失它的效果。");
    }

    private static void CheckNativeReadyParent()
    {
        var runtime = typeof(Entry).Assembly.GetType("STS2SkinChanger.Ui.CharacterAppearanceRuntime", true)!;
        var attach = runtime.GetMethod("AttachReplacementCreatureVisuals", Static)
            ?? throw new InvalidOperationException("热切换必须以原生生物节点为父级初始化新外观，不能在位移容器下触发 Ready。");
        _owner = (NCreature)RuntimeHelpers.GetUninitializedObject(typeof(NCreature));
        _oldParent = (Node2D)RuntimeHelpers.GetUninitializedObject(typeof(Node2D));
        _oldVisuals = (NCreatureVisuals)RuntimeHelpers.GetUninitializedObject(typeof(NCreatureVisuals));
        _newVisuals = (NCreatureVisuals)RuntimeHelpers.GetUninitializedObject(typeof(NCreatureVisuals));
        _visualsField = (FieldInfo)runtime.GetField("VisualsField", Static)!.GetValue(null)!;
        _visualsField.SetValue(_owner, _oldVisuals);
        _attached = _detached = false;

        // Only replace the two native engine calls. Run the actual attachment code, checking
        // what the provider's Ready callback observes at the AddChild boundary.
        var harmony = new Harmony("Gurio.SkinChanger.Tests.CreatureAttachment");
        try
        {
            harmony.Patch(AccessTools.Method(typeof(Node), nameof(Node.RemoveChild)),
                prefix: new HarmonyMethod(typeof(CreatureVisualLifecycleTests), nameof(RemoveChild)));
            harmony.Patch(AccessTools.Method(typeof(Node), nameof(Node.AddChild)),
                prefix: new HarmonyMethod(typeof(CreatureVisualLifecycleTests), nameof(AddChild)));
            attach.Invoke(null, [_owner, _oldParent, _oldVisuals, _newVisuals]);
            Require(_attached && _detached, "新模型必须替换旧模型并经历原生父级下的初始化。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static bool RemoveChild(Node __instance, Node node)
    {
        Require(ReferenceEquals(__instance, _oldParent) && ReferenceEquals(node, _oldVisuals),
            "只允许移出被替换的旧模型。");
        _detached = true;
        return false;
    }

    private static bool AddChild(Node __instance, Node node)
    {
        Require(_detached && ReferenceEquals(__instance, _owner) && ReferenceEquals(node, _newVisuals),
            "原作者 Ready 必须看到 NCreature 父级，且不能再按旧节点名找到旧模型。");
        Require(ReferenceEquals(_visualsField.GetValue(_owner), _newVisuals),
            "Ready 读取 NCreature.Visuals 时必须已经指向新模型。");
        _attached = true;
        return false;
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
    private static class CreatureFinishingPatch
    {
        private static void Postfix(Creature __instance, ref NCreatureVisuals __result) { }
    }

    [HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
    private static class StatefulCreaturePatch
    {
        private static void Postfix(Creature __instance, bool __state, ref NCreatureVisuals __result) { }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

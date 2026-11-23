using System.Collections.Concurrent;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Ancients;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace STS2SkinChanger.Core;

internal enum ProviderPatchCapturePath
{
    FirstActivation,
    RepeatActivation,
    DynamicOrIncompleteFallback,
    InitializerFailure,
    LeakVerification,
    DeactivationFallback
}

internal enum ProviderPatchCaptureMode
{
    Full,
    Targeted
}

internal sealed record CapturedProviderPatch(
    MethodBase Target,
    MethodInfo Callback,
    int Kind,
    string Owner,
    int Priority,
    string[] Before,
    string[] After,
    bool Debug);

internal sealed record ProviderPatchCaptureResult(
    IReadOnlyList<CapturedProviderPatch> DistinctPatches,
    IReadOnlyList<CapturedProviderPatch> RawEntries,
    int DuplicateCount,
    int ScannedTargetCount,
    ProviderPatchCapturePath Path,
    ProviderPatchCaptureMode Mode)
{
    internal string ModeLabel => Mode == ProviderPatchCaptureMode.Full ? "全量" : "精准";

    internal string PathLabel => Path switch
    {
        ProviderPatchCapturePath.FirstActivation => "首次激活",
        ProviderPatchCapturePath.RepeatActivation => "重复激活",
        ProviderPatchCapturePath.DynamicOrIncompleteFallback => "动态兜底",
        ProviderPatchCapturePath.InitializerFailure => "初始化失败",
        ProviderPatchCapturePath.LeakVerification => "泄漏校验",
        ProviderPatchCapturePath.DeactivationFallback => "停用兜底",
        _ => Path.ToString()
    };
}

internal sealed class AssemblyPatchTargetRegistry
{
    private readonly object _sync = new();
    private readonly HashSet<MethodBase> _observedTargets = [];
    private readonly HashSet<MethodBase> _declarativeTargets = [];
    private bool _declarativeScanned;
    private bool _coverageComplete;

    internal bool CoverageComplete
    {
        get
        {
            lock (_sync)
            {
                return _coverageComplete;
            }
        }
    }

    internal int KnownTargetCount
    {
        get
        {
            lock (_sync)
            {
                return _observedTargets.Count + _declarativeTargets.Count;
            }
        }
    }

    internal void RecordObservedTargets(IEnumerable<MethodBase> targets, bool markCoverageComplete)
    {
        lock (_sync)
        {
            foreach (var target in targets)
            {
                if (target != null)
                {
                    _observedTargets.Add(target);
                }
            }

            if (markCoverageComplete)
            {
                _coverageComplete = true;
            }
        }
    }

    internal void EnsureDeclarativeTargets(Assembly assembly)
    {
        lock (_sync)
        {
            if (_declarativeScanned)
            {
                return;
            }

            _declarativeScanned = true;
        }

        var resolved = ProviderPatchIndex.ResolveDeclarativePatchTargets(assembly);
        lock (_sync)
        {
            foreach (var target in resolved)
            {
                _declarativeTargets.Add(target);
            }
        }
    }

    internal void InvalidateCoverage()
    {
        lock (_sync)
        {
            _coverageComplete = false;
        }
    }

    internal MethodBase[] SnapshotAllKnownTargets()
    {
        lock (_sync)
        {
            var merged = new HashSet<MethodBase>(_observedTargets);
            merged.UnionWith(_declarativeTargets);
            return merged.ToArray();
        }
    }
}

/// <summary>
/// Per-assembly patch target registry and 4-path provider patch discovery engine.
/// Keeps full Harmony scans on first activation, dynamic providers (e.g. FoolWaifu),
/// incomplete blueprints and initializer-failure handling, while switching verified repeat
/// activations and post-unpatch leak checks to targeted <see cref="Harmony.GetPatchInfo"/> lookups.
/// </summary>
internal static class ProviderPatchIndex
{
    internal const int PatchKindPrefix = 0;
    internal const int PatchKindPostfix = 1;
    internal const int PatchKindTranspiler = 2;
    internal const int PatchKindFinalizer = 3;

    private static readonly ConcurrentDictionary<Assembly, AssemblyPatchTargetRegistry> Registries = new();
    private static readonly ConcurrentDictionary<Assembly, byte> DynamicPatchAssemblies = new();

    internal static AssemblyPatchTargetRegistry GetOrCreateRegistry(Assembly assembly) =>
        Registries.GetOrAdd(assembly, _ => new AssemblyPatchTargetRegistry());

    internal static bool TryGetRegistry(Assembly assembly, out AssemblyPatchTargetRegistry? registry) =>
        Registries.TryGetValue(assembly, out registry);

    internal static void MarkDynamicPatchAssembly(Assembly assembly) =>
        DynamicPatchAssemblies[assembly] = 1;

    internal static void UnmarkDynamicPatchAssembly(Assembly assembly) =>
        DynamicPatchAssemblies.TryRemove(assembly, out _);

    internal static void ClearRegistry(Assembly assembly) =>
        Registries.TryRemove(assembly, out _);

    internal static bool IsDynamicPatchProvider(Assembly assembly) =>
        FoolWaifuRuntimeCompat.IsProvider(assembly) || DynamicPatchAssemblies.ContainsKey(assembly);

    internal static ProviderPatchCapturePath SelectDiscoveryPath(
        Assembly assembly,
        bool skippedInitializer,
        bool hasProviderBlueprint)
    {
        if (IsDynamicPatchProvider(assembly))
        {
            return ProviderPatchCapturePath.DynamicOrIncompleteFallback;
        }

        if (!skippedInitializer)
        {
            return ProviderPatchCapturePath.FirstActivation;
        }

        if (!hasProviderBlueprint ||
            !Registries.TryGetValue(assembly, out var registry) ||
            !registry.CoverageComplete)
        {
            return ProviderPatchCapturePath.DynamicOrIncompleteFallback;
        }

        return ProviderPatchCapturePath.RepeatActivation;
    }

    internal static ProviderPatchCaptureResult CaptureForActivation(
        Assembly assembly,
        bool skippedInitializer,
        bool hasProviderBlueprint,
        bool isAdoptedGameAssembly,
        IEnumerable<MethodBase>? extraKnownTargets = null)
    {
        if (isAdoptedGameAssembly)
        {
            return new ProviderPatchCaptureResult(
                [],
                [],
                0,
                0,
                ProviderPatchCapturePath.RepeatActivation,
                ProviderPatchCaptureMode.Targeted);
        }

        var path = SelectDiscoveryPath(assembly, skippedInitializer, hasProviderBlueprint);
        if (path != ProviderPatchCapturePath.RepeatActivation)
        {
            return CaptureFull(assembly, path);
        }

        var registry = GetOrCreateRegistry(assembly);
        registry.EnsureDeclarativeTargets(assembly);

        var candidateTargets = new HashSet<MethodBase>(registry.SnapshotAllKnownTargets());
        if (extraKnownTargets != null)
        {
            foreach (var target in extraKnownTargets)
            {
                if (target != null)
                {
                    candidateTargets.Add(target);
                }
            }
        }

        // Harmony.GetAllPatchedMethods() only returns HarmonySharedState.state.Keys (no deserialization).
        // Intersect those live keys with the fixed set of managed-ownership engine target shapes so
        // GetPatchInfo deserialization runs only on blueprint targets + patched managed engine targets.
        foreach (var liveTarget in Harmony.GetAllPatchedMethods())
        {
            if (IsManagedCandidateTarget(liveTarget, assembly))
            {
                candidateTargets.Add(liveTarget);
            }
        }

        return CaptureOnTargets(assembly, candidateTargets, ProviderPatchCapturePath.RepeatActivation);
    }

    internal static ProviderPatchCaptureResult CaptureFull(
        Assembly assembly,
        ProviderPatchCapturePath path = ProviderPatchCapturePath.FirstActivation)
    {
        var allTargets = Harmony.GetAllPatchedMethods().ToArray();
        return CaptureOnTargetsCore(assembly, allTargets, path, ProviderPatchCaptureMode.Full);
    }

    internal static ProviderPatchCaptureResult CaptureOnTargets(
        Assembly assembly,
        IEnumerable<MethodBase> targets,
        ProviderPatchCapturePath path = ProviderPatchCapturePath.LeakVerification)
    {
        var distinctTargets = targets is ISet<MethodBase> set
            ? set.ToArray()
            : targets.Where(t => t != null).Distinct().ToArray();
        return CaptureOnTargetsCore(assembly, distinctTargets, path, ProviderPatchCaptureMode.Targeted);
    }

    internal static void RecordBlueprintTargets(
        Assembly assembly,
        IEnumerable<MethodBase> observedTargets,
        bool markCoverageComplete)
    {
        var registry = GetOrCreateRegistry(assembly);
        registry.EnsureDeclarativeTargets(assembly);
        registry.RecordObservedTargets(
            observedTargets,
            markCoverageComplete: markCoverageComplete && !IsDynamicPatchProvider(assembly));
    }

    /// <summary>
    /// Entry-by-entry comparison of (Target, Callback, Kind, Owner) multiplicities so initializer
    /// failure handling never confuses count equality with unchanged ownership/callbacks.
    /// </summary>
    internal static bool DidInitializerInstallPatches(
        IReadOnlyList<CapturedProviderPatch> beforeInitializer,
        IReadOnlyList<CapturedProviderPatch> afterInitializer)
    {
        var beforeCounts = CountPatchOwnershipEntries(beforeInitializer);
        var afterCounts = CountPatchOwnershipEntries(afterInitializer);

        foreach (var (key, afterCount) in afterCounts)
        {
            if (!beforeCounts.TryGetValue(key, out var beforeCount) || afterCount > beforeCount)
            {
                return true;
            }
        }

        return false;
    }

    internal static IReadOnlyList<CapturedProviderPatch> DiffAddedPatches(
        IReadOnlyList<CapturedProviderPatch> beforeEntries,
        IReadOnlyList<CapturedProviderPatch> afterEntries)
    {
        var remaining = CountPatchOwnershipEntries(beforeEntries);
        var added = new List<CapturedProviderPatch>();
        foreach (var entry in afterEntries)
        {
            var key = (entry.Target, entry.Callback, entry.Kind, entry.Owner);
            if (remaining.TryGetValue(key, out var count) && count > 0)
            {
                remaining[key] = count - 1;
                continue;
            }

            added.Add(entry);
        }

        return added;
    }

    internal static IReadOnlyList<MethodBase> ResolveDeclarativePatchTargets(Assembly assembly)
    {
        var resolved = new HashSet<MethodBase>();
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            types = exception.Types.OfType<Type>().ToArray();
        }
        catch
        {
            return [];
        }

        foreach (var type in types)
        {
            List<HarmonyMethod> classMethods;
            try
            {
                classMethods = HarmonyMethodExtensions.GetFromType(type);
            }
            catch
            {
                continue;
            }

            var classMerged = classMethods.Count > 0 ? HarmonyMethod.Merge(classMethods) : null;
            if (classMerged != null && TryResolveHarmonyMethodTarget(classMerged) is { } classTarget)
            {
                resolved.Add(classTarget);
            }

            MethodInfo[] declaredMethods;
            try
            {
                declaredMethods = type.GetMethods(
                    BindingFlags.Static | BindingFlags.Instance |
                    BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);
            }
            catch
            {
                continue;
            }

            foreach (var method in declaredMethods)
            {
                List<HarmonyMethod> methodAttrs;
                try
                {
                    methodAttrs = HarmonyMethodExtensions.GetFromMethod(method);
                }
                catch
                {
                    continue;
                }

                if (methodAttrs.Count == 0)
                {
                    continue;
                }

                var combined = classMerged != null
                    ? HarmonyMethod.Merge([classMerged, .. methodAttrs])
                    : HarmonyMethod.Merge(methodAttrs);
                if (TryResolveHarmonyMethodTarget(combined) is { } methodTarget)
                {
                    resolved.Add(methodTarget);
                }
            }
        }

        return resolved.ToArray();
    }

    /// <summary>
    /// Superset predicate over target metadata only (zero <see cref="Harmony.GetPatchInfo"/> calls)
    /// covering every target that <see cref="ManagedSkinModLoader"/>'s managed isolation rules can
    /// ever claim, plus targets declared in <paramref name="providerAssembly"/> itself.
    /// </summary>
    internal static bool IsManagedCandidateTarget(MethodBase target, Assembly providerAssembly)
    {
        var declaringType = target.DeclaringType;
        if (declaringType == null)
        {
            return false;
        }

        if (ReferenceEquals(declaringType.Assembly, providerAssembly))
        {
            return true;
        }

        var name = target.Name;

        // IsManagedCardTexturePatch
        if (declaringType == typeof(NCard) &&
            name is nameof(NCard._EnterTree) or nameof(NCard.UpdateVisuals))
        {
            return true;
        }

        // IsManagedResourceOwnershipPatch: Creature.CreateVisuals & NRelic._Ready
        if (declaringType == typeof(Creature) && name == nameof(Creature.CreateVisuals))
        {
            return true;
        }

        if (declaringType.Name.Equals("NRelic", StringComparison.Ordinal) &&
            name.Equals("_Ready", StringComparison.Ordinal))
        {
            return true;
        }

        // IsManagedResourceOwnershipPatch: global asset loaders/helpers
        if (declaringType == typeof(AssetCache) ||
            declaringType == typeof(ResourceLoader) ||
            declaringType == typeof(AtlasManager) ||
            declaringType == typeof(SceneHelper) ||
            declaringType == typeof(ImageHelper))
        {
            return name.StartsWith("Get", StringComparison.Ordinal) ||
                   name.StartsWith("Load", StringComparison.Ordinal) ||
                   name.StartsWith("Instantiate", StringComparison.Ordinal) ||
                   name.Contains("Path", StringComparison.OrdinalIgnoreCase);
        }

        // IsManagedResourceOwnershipPatch: CardModel.PortraitPath getter (including overrides)
        if (target is MethodInfo { Name: "get_" + nameof(CardModel.PortraitPath) } getter &&
            getter.GetBaseDefinition().DeclaringType == typeof(CardModel))
        {
            return true;
        }

        // IsManagedResourceOwnershipPatch: CharacterModel / MonsterModel CreateVisuals and visual getters
        if (typeof(CharacterModel).IsAssignableFrom(declaringType) ||
            typeof(MonsterModel).IsAssignableFrom(declaringType))
        {
            if (name.Equals(nameof(CharacterModel.CreateVisuals), StringComparison.Ordinal))
            {
                return true;
            }

            if (name.StartsWith("get_", StringComparison.Ordinal))
            {
                var propertyName = name[4..];
                if (propertyName.Contains("Visual", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("Scene", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("Portrait", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("Icon", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("Texture", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("Background", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("MapMarker", StringComparison.OrdinalIgnoreCase) ||
                    propertyName.Contains("Path", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        // IsManagedCharacterPresentationPatch
        if ((STS2SkinChanger.Ui.CharacterSelectScreenAccess.IsScreenType(declaringType) &&
             name.Equals(nameof(NCharacterSelectScreen.SelectCharacter), StringComparison.Ordinal)) ||
            (typeof(NCharacterSelectButton).IsAssignableFrom(declaringType) &&
             name.Equals("Init", StringComparison.Ordinal)))
        {
            return true;
        }

        // IsManagedNodeReadyPresentationPatch & IsOstyPositioningPatch
        if (typeof(NCreature).IsAssignableFrom(declaringType) &&
            name is "_Ready" or "OstyScaleToSize" or "ScaleAndPositionOsty")
        {
            return true;
        }

        if (name.Equals("_Ready", StringComparison.Ordinal) &&
            (typeof(NMerchantRoom).IsAssignableFrom(declaringType) ||
             typeof(NRestSiteRoom).IsAssignableFrom(declaringType) ||
             typeof(NBossMapPoint).IsAssignableFrom(declaringType) ||
             declaringType.Name is
                 "NMerchantButton" or
                 "NMerchantHand" or
                 "NMerchantInventory" or
                 "NMerchantSlot" or
                 "NMerchantDialogue" or
                 "NFakeMerchant"))
        {
            return true;
        }

        // HildeAncientDialogueCompat.OwnsPatch
        if (declaringType == typeof(AncientDialogueSet) &&
            name == nameof(AncientDialogueSet.PopulateLocKeys))
        {
            return true;
        }

        // ShouldStripPlayerMerchantCharacterPatch
        if (declaringType.Name.Equals("NMerchantCharacter", StringComparison.Ordinal))
        {
            return true;
        }

        return false;
    }

    private static ProviderPatchCaptureResult CaptureOnTargetsCore(
        Assembly assembly,
        MethodBase[] targets,
        ProviderPatchCapturePath path,
        ProviderPatchCaptureMode mode)
    {
        var rawEntries = new List<CapturedProviderPatch>();
        foreach (var target in targets)
        {
            Patches? patchInfo;
            try
            {
                patchInfo = Harmony.GetPatchInfo(target);
            }
            catch
            {
                continue;
            }

            if (patchInfo == null)
            {
                continue;
            }

            AppendMatchingPatches(target, patchInfo.Prefixes, PatchKindPrefix, assembly, rawEntries);
            AppendMatchingPatches(target, patchInfo.Postfixes, PatchKindPostfix, assembly, rawEntries);
            AppendMatchingPatches(target, patchInfo.Transpilers, PatchKindTranspiler, assembly, rawEntries);
            AppendMatchingPatches(target, patchInfo.Finalizers, PatchKindFinalizer, assembly, rawEntries);
        }

        var distinct = rawEntries
            .DistinctBy(patch => (patch.Target, patch.Callback, patch.Kind))
            .ToArray();
        var duplicateCount = rawEntries.Count - distinct.Length;
        return new ProviderPatchCaptureResult(
            distinct,
            rawEntries,
            duplicateCount,
            targets.Length,
            path,
            mode);
    }

    private static void AppendMatchingPatches(
        MethodBase target,
        IReadOnlyCollection<Patch> entries,
        int kind,
        Assembly assembly,
        List<CapturedProviderPatch> destination)
    {
        foreach (var entry in entries)
        {
            if (entry.PatchMethod != null &&
                ReferenceEquals(entry.PatchMethod.Module.Assembly, assembly))
            {
                destination.Add(new CapturedProviderPatch(
                    target,
                    entry.PatchMethod,
                    kind,
                    entry.owner ?? string.Empty,
                    entry.priority,
                    entry.before ?? [],
                    entry.after ?? [],
                    entry.debug));
            }
        }
    }

    private static Dictionary<(MethodBase Target, MethodInfo Callback, int Kind, string Owner), int>
        CountPatchOwnershipEntries(IReadOnlyList<CapturedProviderPatch> entries)
    {
        var counts = new Dictionary<(MethodBase Target, MethodInfo Callback, int Kind, string Owner), int>();
        foreach (var entry in entries)
        {
            var key = (entry.Target, entry.Callback, entry.Kind, entry.Owner);
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        return counts;
    }

    private static MethodBase? TryResolveHarmonyMethodTarget(HarmonyMethod info)
    {
        var declaringType = info.declaringType;
        if (declaringType == null)
        {
            return null;
        }

        var methodType = info.methodType ?? MethodType.Normal;
        try
        {
            return methodType switch
            {
                MethodType.Normal => string.IsNullOrWhiteSpace(info.methodName)
                    ? null
                    : AccessTools.Method(declaringType, info.methodName, info.argumentTypes),
                MethodType.Getter => string.IsNullOrWhiteSpace(info.methodName)
                    ? null
                    : AccessTools.PropertyGetter(declaringType, info.methodName),
                MethodType.Setter => string.IsNullOrWhiteSpace(info.methodName)
                    ? null
                    : AccessTools.PropertySetter(declaringType, info.methodName),
                MethodType.Constructor => AccessTools.Constructor(declaringType, info.argumentTypes, searchForStatic: false),
                MethodType.StaticConstructor => AccessTools.Constructor(declaringType, [], searchForStatic: true),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }
}

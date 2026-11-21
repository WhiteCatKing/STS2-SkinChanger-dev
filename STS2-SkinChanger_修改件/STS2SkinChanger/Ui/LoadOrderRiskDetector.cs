using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

[Flags]
internal enum LoadOrderRiskReasons
{
    None = 0,
    CardPatch = 1,
    SharedGameAsset = 2
}

internal sealed record DangerousLoadOrderProvider(
    Mod Mod,
    string DisplayName,
    LoadOrderRiskReasons Reasons);

internal enum LoadOrderPromptKind
{
    None,
    FirstInstallAutoMoved,
    FirstInstallRiskAutoMoved,
    OrderChangedChoice,
    RiskChoice,
    OrderWithExternalManager,
    RiskWithExternalManager
}

internal sealed record LoadOrderPromptPlan(
    LoadOrderPromptKind Kind,
    ModText? MessageKey,
    IReadOnlyList<Mod> ListedProviders,
    bool ShowRestartButton,
    bool ShouldAutoMove,
    bool SaveSnapshotOnSchedule,
    bool SaveSnapshotOnDecline);

internal static class LoadOrderRiskDetector
{
    internal const string ExternalLoadOrderManagerId = "LoadOrderManager";

    private static readonly FieldInfo? ModAssemblyField =
        AccessTools.Field(typeof(Mod), "assembly");

    internal static bool IsExternalLoadOrderManagerLoaded(IEnumerable<Mod> mods) =>
        mods.Any(mod =>
            mod.state == ModLoadState.Loaded &&
            string.Equals(mod.manifest?.id, ExternalLoadOrderManagerId, StringComparison.OrdinalIgnoreCase));

    internal static IReadOnlyList<DangerousLoadOrderProvider> DetectDangerousProviders(
        IEnumerable<Mod> providersBeforeSelf,
        SkinCatalog? catalog)
    {
        var providers = providersBeforeSelf.ToArray();
        if (providers.Length == 0)
        {
            return [];
        }

        var patchedMethods = CaptureAllPatchedMethods();
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var results = new List<DangerousLoadOrderProvider>();
        foreach (var mod in providers)
        {
            var reasons = LoadOrderRiskReasons.None;
            var modId = mod.manifest?.id;
            var assembly = ResolveLoadedModAssembly(mod, loadedAssemblies);
            if (HasCardPatchRisk(assembly, modId, patchedMethods))
            {
                reasons |= LoadOrderRiskReasons.CardPatch;
            }

            if (HasSharedGameAssetRisk(mod, catalog))
            {
                reasons |= LoadOrderRiskReasons.SharedGameAsset;
            }

            if (reasons != LoadOrderRiskReasons.None)
            {
                results.Add(new DangerousLoadOrderProvider(
                    mod,
                    GetDisplayName(mod),
                    reasons));
            }
        }

        return results;
    }

    internal static bool HasCardPatchRisk(
        Assembly? modAssembly,
        string? modId,
        IEnumerable<(MethodBase Target, Patches? PatchInfo)> patchedMethods)
    {
        if (modAssembly == null && string.IsNullOrWhiteSpace(modId))
        {
            return false;
        }

        foreach (var (target, patchInfo) in patchedMethods)
        {
            if (patchInfo == null || !IsRiskyCardPatchTarget(target))
            {
                continue;
            }

            foreach (var patch in EnumeratePatches(patchInfo))
            {
                if (PatchBelongsToMod(patch, modAssembly, modId))
                {
                    return true;
                }
            }
        }

        return false;
    }

    internal static bool IsRiskyCardPatchTarget(MethodBase target)
    {
        var declaringType = target.DeclaringType;
        if (declaringType == null)
        {
            return false;
        }

        if (typeof(NCard).IsAssignableFrom(declaringType))
        {
            return target.Name is "_EnterTree"
                or "_Ready"
                or "UpdateVisuals"
                or "Reload"
                or "UpdateVisualsFromModel";
        }

        if (typeof(CardModel).IsAssignableFrom(declaringType))
        {
            return target.Name is "get_PortraitPath"
                or "get_Portrait"
                or "get_BetaPortraitPath"
                or "get_PackedPortraitPath"
                or "get_HasPortrait"
                or "GetPortraitPath";
        }

        return false;
    }

    internal static bool HasSharedGameAssetRisk(IEnumerable<string> pckPaths, SkinCatalog? catalog) =>
        pckPaths.Any(path => catalog != null
            ? catalog.ProtectGlobalCanonicalPackFile(path)
            : SkinCatalog.IsSharedGameAtlasPath(path));

    internal static string FormatRiskLogLine(IReadOnlyList<DangerousLoadOrderProvider> dangerousProviders)
    {
        var items = dangerousProviders.Select(item => $"{item.DisplayName}({FormatReasonTag(item.Reasons)})");
        return "[加载顺序] 提前加载的危险提供者: " + string.Join("、", items);
    }

    internal static string FormatReasonTag(LoadOrderRiskReasons reasons) => reasons switch
    {
        LoadOrderRiskReasons.CardPatch | LoadOrderRiskReasons.SharedGameAsset => "原因a/b",
        LoadOrderRiskReasons.CardPatch => "原因a",
        LoadOrderRiskReasons.SharedGameAsset => "原因b",
        _ => "未知"
    };

    internal static LoadOrderPromptPlan ResolvePromptPlan(
        bool hasExternalLoadOrderManager,
        bool isFirstInstall,
        bool isUpgradeBaselineOrLegacySnapshot,
        bool orderChanged,
        bool shouldShowWarning,
        IReadOnlyList<Mod> providersAboveSelf,
        IReadOnlyList<DangerousLoadOrderProvider> dangerousProviders)
    {
        if (providersAboveSelf.Count == 0)
        {
            return new LoadOrderPromptPlan(
                LoadOrderPromptKind.None,
                MessageKey: null,
                ListedProviders: [],
                ShowRestartButton: false,
                ShouldAutoMove: false,
                SaveSnapshotOnSchedule: true,
                SaveSnapshotOnDecline: false);
        }

        var dangerousMods = dangerousProviders.Select(item => item.Mod).ToArray();
        if (dangerousMods.Length > 0)
        {
            if (hasExternalLoadOrderManager)
            {
                return new LoadOrderPromptPlan(
                    LoadOrderPromptKind.RiskWithExternalManager,
                    ModText.LoadOrderRiskWithManagerMessage,
                    dangerousMods,
                    ShowRestartButton: false,
                    ShouldAutoMove: false,
                    SaveSnapshotOnSchedule: false,
                    SaveSnapshotOnDecline: false);
            }

            if (isFirstInstall)
            {
                return new LoadOrderPromptPlan(
                    LoadOrderPromptKind.FirstInstallRiskAutoMoved,
                    ModText.LoadOrderRiskFirstInstallMessage,
                    dangerousMods,
                    ShowRestartButton: true,
                    ShouldAutoMove: true,
                    SaveSnapshotOnSchedule: false,
                    SaveSnapshotOnDecline: false);
            }

            return new LoadOrderPromptPlan(
                LoadOrderPromptKind.RiskChoice,
                ModText.LoadOrderRiskMessage,
                dangerousMods,
                ShowRestartButton: true,
                ShouldAutoMove: false,
                SaveSnapshotOnSchedule: false,
                SaveSnapshotOnDecline: false);
        }

        // No dangerous providers above self: preserve existing behavior, except when LoadOrderManager
        // is loaded (never auto-move or offer "Prioritize & Restart" against it).
        if (isFirstInstall)
        {
            if (!shouldShowWarning)
            {
                return new LoadOrderPromptPlan(
                    LoadOrderPromptKind.None,
                    MessageKey: null,
                    ListedProviders: [],
                    ShowRestartButton: false,
                    ShouldAutoMove: false,
                    SaveSnapshotOnSchedule: true,
                    SaveSnapshotOnDecline: false);
            }

            if (hasExternalLoadOrderManager)
            {
                return new LoadOrderPromptPlan(
                    LoadOrderPromptKind.OrderWithExternalManager,
                    ModText.LoadOrderWithManagerMessage,
                    providersAboveSelf,
                    ShowRestartButton: false,
                    ShouldAutoMove: false,
                    SaveSnapshotOnSchedule: true,
                    SaveSnapshotOnDecline: false);
            }

            return new LoadOrderPromptPlan(
                LoadOrderPromptKind.FirstInstallAutoMoved,
                ModText.LoadOrderMessage,
                providersAboveSelf,
                ShowRestartButton: true,
                ShouldAutoMove: true,
                SaveSnapshotOnSchedule: false,
                SaveSnapshotOnDecline: false);
        }

        if (isUpgradeBaselineOrLegacySnapshot)
        {
            return new LoadOrderPromptPlan(
                LoadOrderPromptKind.None,
                MessageKey: null,
                ListedProviders: [],
                ShowRestartButton: false,
                ShouldAutoMove: false,
                SaveSnapshotOnSchedule: true,
                SaveSnapshotOnDecline: false);
        }

        if (!LoadOrderWarningController.ShouldPromptForOrderChange(orderChanged, providersAboveSelf.Count) ||
            !shouldShowWarning)
        {
            return new LoadOrderPromptPlan(
                LoadOrderPromptKind.None,
                MessageKey: null,
                ListedProviders: [],
                ShowRestartButton: false,
                ShouldAutoMove: false,
                SaveSnapshotOnSchedule: orderChanged && !shouldShowWarning,
                SaveSnapshotOnDecline: false);
        }

        if (hasExternalLoadOrderManager)
        {
            return new LoadOrderPromptPlan(
                LoadOrderPromptKind.OrderWithExternalManager,
                ModText.LoadOrderWithManagerMessage,
                providersAboveSelf,
                ShowRestartButton: false,
                ShouldAutoMove: false,
                SaveSnapshotOnSchedule: true,
                SaveSnapshotOnDecline: false);
        }

        return new LoadOrderPromptPlan(
            LoadOrderPromptKind.OrderChangedChoice,
            ModText.LoadOrderChoiceMessage,
            providersAboveSelf,
            ShowRestartButton: true,
            ShouldAutoMove: false,
            SaveSnapshotOnSchedule: false,
            SaveSnapshotOnDecline: true);
    }

    internal static string GetDisplayName(Mod mod) =>
        !string.IsNullOrWhiteSpace(mod.manifest?.name)
            ? mod.manifest!.name!
            : !string.IsNullOrWhiteSpace(mod.manifest?.id)
                ? mod.manifest!.id!
                : mod.path;

    private static bool HasSharedGameAssetRisk(Mod mod, SkinCatalog? catalog)
    {
        var modId = mod.manifest?.id;
        if (string.IsNullOrWhiteSpace(mod.path) || string.IsNullOrWhiteSpace(modId))
        {
            return false;
        }

        var pckPath = SkinPackagePaths.Resolve(mod.path, modId, ".pck");
        if (!File.Exists(pckPath))
        {
            return false;
        }

        try
        {
            using var archive = PckArchive.Open(pckPath);
            return HasSharedGameAssetRisk(archive.Paths, catalog);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"读取提前加载提供者 {GetDisplayName(mod)} 的 PCK 失败，跳过共享资源风险检查：" +
                        exception.GetBaseException().Message);
            return false;
        }
    }

    private static (MethodBase Target, Patches? PatchInfo)[] CaptureAllPatchedMethods()
    {
        try
        {
            return Harmony.GetAllPatchedMethods()
                .Select(target => (target, (Patches?)Harmony.GetPatchInfo(target)))
                .ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static IEnumerable<Patch> EnumeratePatches(Patches patchInfo) =>
        patchInfo.Prefixes
            .Concat(patchInfo.Postfixes)
            .Concat(patchInfo.Transpilers)
            .Concat(patchInfo.Finalizers);

    private static bool PatchBelongsToMod(Patch patch, Assembly? modAssembly, string? modId)
    {
        if (modAssembly != null &&
            ReferenceEquals(patch.PatchMethod?.DeclaringType?.Assembly, modAssembly))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(modId) &&
               string.Equals(patch.owner, modId, StringComparison.OrdinalIgnoreCase);
    }

    private static Assembly? ResolveLoadedModAssembly(Mod mod, Assembly[] loadedAssemblies)
    {
        if (ModAssemblyField?.GetValue(mod) is Assembly direct)
        {
            return direct;
        }

        var modId = mod.manifest?.id;
        if (string.IsNullOrWhiteSpace(mod.path) || string.IsNullOrWhiteSpace(modId))
        {
            return null;
        }

        var dllPath = SkinPackagePaths.Resolve(mod.path, modId, ".dll");
        if (!File.Exists(dllPath))
        {
            return null;
        }

        var fullDllPath = Path.GetFullPath(dllPath);
        foreach (var candidate in loadedAssemblies)
        {
            if (candidate.IsDynamic)
            {
                continue;
            }

            try
            {
                var location = candidate.Location;
                if (!string.IsNullOrWhiteSpace(location) &&
                    string.Equals(Path.GetFullPath(location), fullDllPath, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }
            catch
            {
                // Ignore assemblies without a filesystem location.
            }
        }

        return null;
    }
}

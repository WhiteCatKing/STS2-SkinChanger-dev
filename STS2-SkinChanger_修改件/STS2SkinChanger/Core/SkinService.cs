using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Core;

internal static partial class SkinService
{
    public const float MinimumMonsterScale = 0.2f;
    public const float MaximumMonsterScale = 5f;
    public const float MonsterScaleStep = 0.05f;
    public const float MinimumCharacterScale = 0.2f;
    public const float MaximumCharacterScale = 5f;
    public const float CharacterScaleStep = 0.05f;
    public const float MinimumCharacterOffset = -1000f;
    public const float MaximumCharacterOffset = 1000f;
    public const float CharacterOffsetStep = 1f;
    public const string InheritCardSelectionId = "__inherit__";
    public const string InheritMonsterSelectionId = "__monster_category__";
    private const string LegacyFollowCharacterSkinIconSelectionId = "__follow_character_skin__";
    public const int CardSkinPresetNameMaxLength = 40;
    public const int MonsterSkinPresetNameMaxLength = 40;
    private const long DirectRuntimeProviderPackThresholdBytes = 64L * 1024L * 1024L;

    private static readonly object Sync = new();
    private static readonly Dictionary<string, Resource> RuntimeResourceCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, PreparedRuntimeOverlay> PreparedRuntimeOverlays =
        new(StringComparer.OrdinalIgnoreCase);
    // The most recent overlay of each group, kept so a caller can ask for the private (isolated)
    // copy of one canonical resource without rebuilding or re-mounting the pack.
    private static readonly Dictionary<string, PreparedRuntimeOverlay> LastPreparedRuntimeOverlays =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, RuntimeResourceBundleState> RuntimeResourceBundles =
        new(StringComparer.OrdinalIgnoreCase);
    private const int MaxCardPortraitCacheEntries = 64;
    private static readonly BoundedLruCache<string, Texture2D> CardPortraitCache =
        new(MaxCardPortraitCacheEntries, StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> ExternalCardProviderIdentityPaths =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Texture2D> BaselineRelicAtlasCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, Texture2D> BaselineRelicIconCache =
        new(StringComparer.OrdinalIgnoreCase);
    // A malformed or incomplete provider card must never throw through CardModel.Portrait.
    // Cache the failed request for this session so every redraw does not rebuild the same
    // overlay (and so the vanilla portrait remains usable in choice screens).
    private static readonly HashSet<string> FailedCardPortraitRequests =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, CardCoverageState> CardCoverageCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, IsolatedCardOverlayState> IsolatedCardOverlayCache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly CanonicalResourceOwnershipTracker CardCanonicalResourceOwners = new();
    private static readonly Dictionary<string, string> CardPreviewSelections =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> CharacterPreviewSelections =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, System.Reflection.MethodInfo> AncientStyleMethods =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> MissingAncientStyleMethods =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> FailedAncientStyleMethods =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, string> MountedOverlayCache =
        new(StringComparer.Ordinal);
    private static readonly HashSet<string> MountedScopedRuntimeProviderPacks =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, MountedProviderPackState> MountedLargeRuntimeProviderPacks =
        new(StringComparer.OrdinalIgnoreCase);
    /// <summary>
    /// 整包提权的顺序（后提权的在栈顶）。联机的 8 秒自愈定时器会反复重建外观覆盖，而
    /// `LoadResourcePack(replaceFiles: true)` 每次要 42–49 ms；只要这份包里没有再被别人提权过，
    /// 重复把同一个包压到栈顶对结果没有任何影响，可以直接跳过。
    /// </summary>
    private static readonly List<string> PromotedPackOrder = [];
    /// <summary>
    /// 上一次组装好的外观覆盖文件表，按"会影响产物的全部输入"的签名复用。
    /// 场景切换时选择没有变化，复用可以把每次 1-2 秒的组装压到 0；输入一变签名就变，
    /// 自然回到完整重建，所以不需要额外的失效逻辑。
    /// </summary>
    private static OverlayPlan? _cachedOverlayPlan;
    /// <summary>
    /// 我们已经挂上去的每一层覆盖包对"每个路径最终由谁提供"的记录（后挂的层赢）。换肤时只把和
    /// 这份记录不一致的路径写成新的一层：整份覆盖包约 630 MB，一份一份重写既慢又会耗尽内存。
    /// </summary>
    private static readonly Dictionary<string, OverlayOwner> MountedOverlayOwners =
        new(StringComparer.OrdinalIgnoreCase);
    private static long _mountedOverlayBytes;
    private static long _nextMountedOverlayWarningBytes = 2L * 1024 * 1024 * 1024;
    private static int _mountedOverlayLayers;
    /// <summary>单层覆盖包的上限；超过就保留当前外观，避免一次写盘+挂载把内存吃光。</summary>
    private const long MaxSingleOverlayBytes = 1536L * 1024 * 1024;
    private static readonly Dictionary<string, HashSet<string>> RuntimeCanonicalDependencyPaths =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> WarmedRuntimeProviderPacks =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> WarmingRuntimeProviderPacks =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly SemaphoreSlim RuntimePackWarmGate = new(1, 1);
    private static readonly ScopedMonsterSelectionSnapshot ScopedMonsterSelections = new();
    private static RuntimeProviderScope? _runtimeProviderBehaviorScope;
    private static readonly RuntimeProviderScopeLeaseTracker RuntimeProviderScopeLeases = new();
    private static readonly Dictionary<string, ResourceFile> MountedLocalizationFiles =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, LocalizationCacheState> LocalizationStateCache =
        new(StringComparer.Ordinal);
    private static readonly System.Reflection.FieldInfo? LocTablesField =
        AccessTools.Field(typeof(LocManager), "_tables");
    private static readonly System.Reflection.MethodInfo? SetLanguageInternalMethod =
        AccessTools.Method(typeof(LocManager), "SetLanguageInternal");
    private static readonly HashSet<string> SharedCardPoolIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "event",
        "token",
        "status",
        "curse",
        "quest",
        "deprived",
        "deprecated",
        "mock"
    };
    private static int _overlayGeneration;
    private static int _runtimePackWarmGeneration;
    private static bool _fakeMerchantEventArtMounted;
    private static string _sessionId =
        $"{DateTime.Now:yyyyMMdd-HHmmss}-{System.Environment.ProcessId}-{Guid.NewGuid():N}";
    private static bool _initialized;
    private static bool _configLoaded;
    private static bool _cardGroupsInitialized;
    private static bool _loadOrderAutoReorderLogged;
    private static string? _mountedLocalizationSignature;
    private static ConditionalWeakTable<CardModel, CardLookup> _cardLookupCache = new();
    private static readonly ConditionalWeakTable<SkinCatalog, Dictionary<Type, string>> CharacterBehaviorGroups = new();

    public static SkinCatalog? Catalog { get; private set; }
    public static SkinConfig Config { get; private set; } = new();

    private static volatile string? _lastError;

    // 用 volatile 保证异常路径下 UI 线程的可见性。
    public static string? LastError
    {
        get => _lastError;
        private set => _lastError = value;
    }

    // Set by ShouldShowLoadOrderWarning: true when the previous run ended in a safe order
    // (SkinChanger before all skin providers) and skin providers are now above it again. That
    // is evidence of a player's deliberate manual move, not a first-install ordering.
    public static bool LoadOrderMovedBehindSkinMods { get; private set; }

    private static string GetVisualSelection(string groupId) =>
        MultiplayerSkinSync.GetScopedSelection(groupId) ??
        CharacterPreviewSelections.GetValueOrDefault(groupId) ??
        Config.GetSelection(groupId);

    /// <summary>
    /// The selection the game is currently showing for a group, including multiplayer scope and
    /// previews. Used where a provider's own presentation has to follow the visible choice.
    /// </summary>
    internal static string GetDisplayedSelection(string groupId) => GetVisualSelection(groupId);

    private static IReadOnlyDictionary<string, string> GetVisualSelections()
    {
        var merged = VisualSelectionOverlayPolicy.Merge(
            Config.Selections,
            CharacterPreviewSelections,
            MultiplayerSkinSync.GetScopedSelections());

        // The Osty impostor mod owns that summon with its own SpineSprite. Mounting a skin's osty
        // resources on top of it made the model render dark and stay dark after an attack.
        if (merged.Count == 0 ||
            !merged.Keys.Any(key => OstyImpostorCompat.MustDefer(key)))
        {
            return merged;
        }

        var filtered = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in merged)
        {
            if (!OstyImpostorCompat.MustDefer(pair.Key))
            {
                filtered[pair.Key] = pair.Value;
            }
        }

        return filtered;
    }

    internal static bool TryGetSelectedFrameworkContract(
        string groupId,
        out FrameworkCharacterSkinContract contract)
    {
        lock (Sync)
        {
            contract = null!;
            var found = Catalog != null && Catalog.TryGetSelectedFrameworkContract(
                groupId,
                GetVisualSelection(groupId),
                out contract);
            if (found) contract = FrameworkRegistryCooperation.Filter(contract);
            return found;
        }
    }

    internal static IReadOnlyList<FrameworkCharacterSkinContract>
        GetSelectedFrameworkContracts()
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return [];
            }

            return Catalog.Groups
                .Select(group => Catalog.TryGetSelectedFrameworkContract(
                    group.Id,
                    GetVisualSelection(group.Id),
                    out var contract)
                    ? contract
                    : null)
                .Where(contract => contract != null)
                .Cast<FrameworkCharacterSkinContract>()
                .Select(FrameworkRegistryCooperation.Filter)
                .ToArray();
        }
    }

    public static bool TryBuildSessionCharacterSelection(
        string groupId,
        string optionId,
        out IReadOnlyDictionary<string, string> selectionOverrides)
    {
        lock (Sync)
        {
            selectionOverrides = null!;
            var catalog = Catalog;
            var group = catalog?.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (catalog == null || group == null || !catalog.IsCharacterAppearanceGroup(group.Id) ||
                (!optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                 group.Options.All(option => !option.Id.Equals(
                     optionId,
                     StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }

            var workingSelections = new Dictionary<string, string>(
                Config.Selections,
                StringComparer.OrdinalIgnoreCase);
            selectionOverrides = catalog.BuildVisualSelectionTransaction(
                group.Id,
                optionId,
                workingSelections);
            return true;
        }
    }

    public static bool TryBuildSessionCharacterComposition(
        string groupId,
        IReadOnlyList<string> sourceOptionIds,
        out IReadOnlyDictionary<string, string> selectionOverrides,
        out string resolvedOptionId)
    {
        lock (Sync)
        {
            selectionOverrides = null!;
            resolvedOptionId = SkinCatalog.BaseOptionId;
            var catalog = Catalog;
            if (catalog == null || !catalog.IsCharacterAppearanceGroup(groupId))
            {
                return false;
            }

            if (!catalog.TryCreateSessionCharacterComposition(
                    groupId,
                    sourceOptionIds,
                    out resolvedOptionId))
            {
                resolvedOptionId = SkinCatalog.BaseOptionId;
            }

            return TryBuildSessionCharacterSelection(
                groupId,
                resolvedOptionId,
                out selectionOverrides);
        }
    }

    internal static void ClearSessionCharacterCompositions()
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return;
            }

            foreach (var groupId in Catalog.ClearSessionCharacterCompositions())
            {
                ClearRuntimeResourceCache(groupId);
            }
        }
    }

    public static IReadOnlyList<SkinOption> GetCharacterSkinOptions(string groupId)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            var group = catalog?.Groups.FirstOrDefault(candidate => candidate.Id.Equals(
                groupId,
                StringComparison.OrdinalIgnoreCase));
            if (catalog == null || group == null || !catalog.IsCharacterAppearanceGroup(groupId))
            {
                return [];
            }

            var visibleRawIds = CharacterSkinCompositionPolicy.VisibleRawOptionIds(
                    groupId,
                    catalog.GetRawCharacterOptions(groupId).Select(option => option.Id),
                    Config.CharacterSkinCompositions)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var hiddenBundleSources = BundlePresetPolicy.HiddenSources(Config, groupId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return group.Options.Where(option =>
                    !hiddenBundleSources.Contains(option.Id) && (option.IsComposition
                        ? !option.IsSessionComposition
                        : visibleRawIds.Contains(option.Id)))
                .ToArray();
        }
    }

    public static IReadOnlyList<SkinOption> GetRawCharacterSkinOptions(string groupId)
    {
        lock (Sync)
        {
            return Catalog?.GetRawCharacterOptions(groupId) ?? [];
        }
    }

    public static IReadOnlyList<CharacterSkinComposition> GetCharacterSkinCompositions(
        string groupId)
    {
        lock (Sync)
        {
            return Config.CharacterSkinCompositions
                .Where(composition => composition.GroupId.Equals(
                    groupId,
                    StringComparison.OrdinalIgnoreCase))
                .Select(CloneCharacterSkinComposition)
                .ToArray();
        }
    }

    public static IReadOnlyList<string> GetCharacterSelectionSourceIds(
        string groupId,
        string optionId)
    {
        lock (Sync)
        {
            return Catalog?.GetMultiplayerSourceIds(groupId, optionId) ?? [];
        }
    }

    public static IReadOnlyList<string> GetAvailableCharacterSelectionSourceIds(
        string groupId,
        IReadOnlyList<string> sourceOptionIds)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null || !catalog.IsCharacterAppearanceGroup(groupId))
            {
                return [];
            }

            return catalog.ResolveMultiplayerSourceIds(groupId, sourceOptionIds);
        }
    }

    public static bool SaveCharacterSkinComposition(
        string groupId,
        string? compositionId,
        string? name,
        IReadOnlyList<string> sourceOptionIds,
        bool hideSources,
        out string savedId)
    {
        lock (Sync)
        {
            savedId = string.Empty;
            var catalog = Catalog;
            var rawOptions = catalog?.GetRawCharacterOptions(groupId) ?? [];
            if (catalog == null || rawOptions.Count == 0 ||
                !catalog.IsCharacterAppearanceGroup(groupId))
            {
                LastError = $"找不到角色皮肤分组：{groupId}";
                return false;
            }

            var existing = string.IsNullOrWhiteSpace(compositionId)
                ? null
                : Config.CharacterSkinCompositions.FirstOrDefault(composition =>
                    composition.Id.Equals(compositionId, StringComparison.OrdinalIgnoreCase) &&
                    composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(compositionId) && existing == null)
            {
                LastError = $"找不到合并皮肤：{compositionId}";
                return false;
            }

            var knownRawIds = rawOptions.Select(option => option.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var retainedMissingIds = existing?.SourceOptionIds
                .Where(optionId => !knownRawIds.Contains(optionId))
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ??
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var normalizedSources = sourceOptionIds
                .Where(optionId => !string.IsNullOrWhiteSpace(optionId))
                .Select(optionId => optionId.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (normalizedSources.Count == 0 || normalizedSources.Any(optionId =>
                    !knownRawIds.Contains(optionId) && !retainedMissingIds.Contains(optionId)))
            {
                LastError = "合并皮肤至少需要一个当前角色的原始皮肤来源。";
                return false;
            }

            var previousCompositions = Config.CharacterSkinCompositions
                .Select(CloneCharacterSkinComposition)
                .ToList();
            var previousSelections = new Dictionary<string, string>(
                Config.Selections,
                StringComparer.OrdinalIgnoreCase);
            var previousProviderPriority = Config.VisualProviderPriority.ToList();
            var id = existing?.Id ?? CharacterSkinCompositionPolicy.CreateId();
            var uniqueName = CharacterSkinCompositionPolicy.UniqueName(
                name,
                Config.CharacterSkinCompositions
                    .Where(composition =>
                        composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) &&
                        !composition.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                    .Select(composition => composition.Name),
                ModLocalization.Get(ModText.CombinedSkinDefaultName));
            var updated = new CharacterSkinComposition
            {
                Id = id,
                GroupId = groupId,
                Name = uniqueName,
                SourceOptionIds = normalizedSources,
                HideSources = hideSources
            };
            var wasSelected = existing != null && Config.GetSelection(groupId).Equals(
                existing.Id,
                StringComparison.OrdinalIgnoreCase);
            var shouldApply = CharacterSkinCompositionPolicy.ShouldApplyAfterSave(
                existing == null,
                wasSelected);
            var exitUpdates = wasSelected
                ? catalog.BuildVisualSelectionTransaction(
                    groupId,
                    SkinCatalog.BaseOptionId,
                    Config.Selections)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var affectedGroups = exitUpdates.Keys
                .Append(groupId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (existing == null)
                {
                    Config.CharacterSkinCompositions.Add(updated);
                }
                else
                {
                    var index = Config.CharacterSkinCompositions.IndexOf(existing);
                    Config.CharacterSkinCompositions[index] = updated;
                }

                catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
                if (catalog.Groups.First(group => group.Id.Equals(
                        groupId,
                        StringComparison.OrdinalIgnoreCase)).Options.All(option =>
                        !option.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("合并皮肤没有当前可用的来源，无法应用。");
                }

                if (!shouldApply)
                {
                    Config.Save(ConfigPath);
                    savedId = id;
                    LastError = null;
                    return true;
                }

                var transitionSelections = new Dictionary<string, string>(
                    Config.Selections,
                    StringComparer.OrdinalIgnoreCase);
                foreach (var exitUpdate in exitUpdates)
                {
                    transitionSelections[exitUpdate.Key] = exitUpdate.Value;
                }
                var enterUpdates = catalog.BuildVisualSelectionTransaction(
                    groupId,
                    id,
                    transitionSelections);
                var updates = CharacterSkinCompositionPolicy.MergeSelectionUpdates(
                    [exitUpdates, enterUpdates]);
                affectedGroups.UnionWith(updates.Keys);
                foreach (var update in updates)
                {
                    Config.Selections[update.Key] = update.Value;
                    ClearRuntimeResourceCache(update.Key);
                }

                UpdateVisualProviderPriority(groupId, id);
                MountOverlay(affectedGroups);
                Config.Save(ConfigPath);
                savedId = id;
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                Config.CharacterSkinCompositions = previousCompositions;
                Config.Selections = previousSelections;
                Config.VisualProviderPriority = previousProviderPriority;
                catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
                foreach (var affectedGroup in affectedGroups)
                {
                    ClearRuntimeResourceCache(affectedGroup);
                }
                TryRestoreOverlay(affectedGroups, cardOverlay: false);
                LastError = exception.GetBaseException().Message;
                ModLog.Error($"保存 {groupId} 的合并皮肤失败：{exception}");
                return false;
            }
        }
    }

    public static bool DeleteCharacterSkinComposition(string groupId, string compositionId)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            var existing = Config.CharacterSkinCompositions.FirstOrDefault(composition =>
                composition.Id.Equals(compositionId, StringComparison.OrdinalIgnoreCase) &&
                composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (catalog == null || existing == null)
            {
                LastError = $"找不到合并皮肤：{compositionId}";
                return false;
            }

            var previousCompositions = Config.CharacterSkinCompositions
                .Select(CloneCharacterSkinComposition)
                .ToList();
            var previousSelections = new Dictionary<string, string>(
                Config.Selections,
                StringComparer.OrdinalIgnoreCase);
            var previousProviderPriority = Config.VisualProviderPriority.ToList();
            var updates = Config.GetSelection(groupId).Equals(
                    compositionId,
                    StringComparison.OrdinalIgnoreCase)
                ? catalog.BuildVisualSelectionTransaction(
                    groupId,
                    SkinCatalog.BaseOptionId,
                    Config.Selections)
                : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var affectedGroups = updates.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var update in updates)
                {
                    Config.Selections[update.Key] = update.Value;
                    ClearRuntimeResourceCache(update.Key);
                }
                Config.CharacterSkinCompositions.Remove(existing);
                catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
                SanitizeVisualProviderPriority();
                if (affectedGroups.Count > 0)
                {
                    MountOverlay(affectedGroups);
                }
                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                Config.CharacterSkinCompositions = previousCompositions;
                Config.Selections = previousSelections;
                Config.VisualProviderPriority = previousProviderPriority;
                catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
                foreach (var affectedGroup in affectedGroups)
                {
                    ClearRuntimeResourceCache(affectedGroup);
                }
                TryRestoreOverlay(affectedGroups, cardOverlay: false);
                LastError = exception.GetBaseException().Message;
                ModLog.Error($"删除 {groupId}/{compositionId} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool ApplyCharacterPreviewSelection(string groupId, string optionId)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            var group = catalog?.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (catalog == null || group == null || !catalog.IsCharacterAppearanceGroup(group.Id) ||
                (!optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                 group.Options.All(option =>
                     !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
            {
                LastError = $"未知的角色皮肤预览：{groupId}/{optionId}";
                return false;
            }

            var nextPreview = catalog.BuildVisualSelectionTransaction(
                group.Id,
                optionId,
                Config.Selections);
            var previousPreview = new Dictionary<string, string>(
                CharacterPreviewSelections,
                StringComparer.OrdinalIgnoreCase);
            var affectedGroups = VisualSelectionOverlayPolicy.AffectedGroups(
                previousPreview.Keys,
                nextPreview.Keys);
            try
            {
                CharacterPreviewSelections.Clear();
                foreach (var pair in nextPreview)
                {
                    CharacterPreviewSelections[pair.Key] = pair.Value;
                }

                MountOverlay(affectedGroups);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                CharacterPreviewSelections.Clear();
                foreach (var pair in previousPreview)
                {
                    CharacterPreviewSelections[pair.Key] = pair.Value;
                }

                TryRestoreOverlay(affectedGroups, cardOverlay: false);
                LastError = exception.Message;
                ModLog.Error($"预览 {groupId}/{optionId} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool ClearCharacterPreviewSelection(bool restoreOverlay)
    {
        lock (Sync)
        {
            if (CharacterPreviewSelections.Count == 0)
            {
                return false;
            }

            var affectedGroups = CharacterPreviewSelections.Keys
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            CharacterPreviewSelections.Clear();
            if (restoreOverlay)
            {
                TryRestoreOverlay(affectedGroups, cardOverlay: false);
            }
            return true;
        }
    }

    internal static bool TryRegisterOnlineSessionProvider(
        string optionId,
        string optionName,
        string pckPath,
        string groupId,
        IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings,
        out string error)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                error = "皮肤目录尚未初始化。";
                return false;
            }

            return Catalog.TryAddSessionVisualProvider(
                optionId,
                optionName,
                pckPath,
                groupId,
                resourceBindings,
                out error);
        }
    }

    internal static bool TryReadBaseGameResource(string resourcePath, out byte[] bytes)
    {
        lock (Sync)
        {
            if (Catalog == null || !Catalog.IsBaseGameResource(resourcePath))
            {
                bytes = [];
                return false;
            }

            return Catalog.TryReadBaseGameResource(resourcePath, out bytes);
        }
    }

    internal static void RemoveOnlineSessionProvider(string optionId)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return;
            }

            var affectedGroups = Catalog.Groups
                .Where(group => group.Options.Any(option => option.Id.Equals(
                    optionId,
                    StringComparison.OrdinalIgnoreCase)))
                .Select(group => group.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (affectedGroups.Count > 0)
            {
                MountOverlay(affectedGroups);
            }

            Catalog.RemoveSessionVisualProvider(optionId);
            var cacheMarker = "\n" + optionId + "\n";
            foreach (var key in RuntimeResourceCache.Keys.Where(key =>
                         key.Contains(cacheMarker, StringComparison.OrdinalIgnoreCase)).ToArray())
            {
                RuntimeResourceCache.Remove(key);
            }
            foreach (var key in PreparedRuntimeOverlays.Keys.Where(key =>
                         key.Contains(cacheMarker, StringComparison.OrdinalIgnoreCase)).ToArray())
            {
                RuntimeResourceBundles.Remove(key);
                var overlayPath = PreparedRuntimeOverlays[key].OverlayPath;
                PreparedRuntimeOverlays.Remove(key);
                if (overlayPath != null)
                {
                    try
                    {
                        File.Delete(overlayPath);
                    }
                    catch (Exception exception)
                    {
                        ModLog.Info(
                            "联机皮肤运行缓存仍被 Godot 占用，将在下次启动时清理：" +
                            exception.GetBaseException().Message);
                    }
                }
            }
        }
    }

    internal static bool IsBaseGameResource(string resourcePath)
    {
        lock (Sync)
        {
            return Catalog?.IsBaseGameResource(resourcePath) == true;
        }
    }

    public static void RefreshSessionRuntimeProviders()
    {
        lock (Sync)
        {
            if (Catalog != null)
            {
                MountOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            }
        }
    }

    private static string ConfigPath => SkinChangerPaths.Configuration("skin_changer.json");
    private static string LegacyConfigPath =>
        SkinChangerPaths.Configuration("sts2_skin_switcher.json");

    public static void SuppressLoadOrderWarning()
    {
        lock (Sync)
        {
            // 目录可能尚未初始化：先读取现有配置，避免用默认实例覆盖用户已保存的选择。
            EnsureConfigLoaded();
            Config.SuppressLoadOrderWarning = true;
            Config.Save(ConfigPath);
            ModLog.Info("已保存旧版“不再提示”设置；当前版本会自动调整加载顺序，不再用该设置跳过提示。");
        }
    }

    public static bool ShouldReceiveMultiplayerSkinChanges() =>
        GetMultiplayerSkinMode() == MultiplayerSkinMode.Shared;

    public static bool ShouldSendMultiplayerSkinChanges() => true;

    public static MultiplayerSkinMode GetMultiplayerSkinMode()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return MultiplayerSkinModePolicy.Normalize(Config.MultiplayerSkinMode);
        }
    }

    public static void SetMultiplayerSkinMode(MultiplayerSkinMode mode)
    {
        bool changed;
        lock (Sync)
        {
            EnsureConfigLoaded();
            changed = MultiplayerSkinModePolicy.Normalize(Config.MultiplayerSkinMode) != mode;
            if (changed)
            {
                Config.MultiplayerSkinMode = (int)mode;
                Config.Save(ConfigPath);
            }
        }

        if (changed)
        {
            ModLog.Info("联机皮肤显示方式：" + mode);
            MultiplayerSkinSync.OnSkinModeChanged();
        }
    }

    public static void SetSendMultiplayerSkinChanges(bool enabled)
    {
        // Retained for older callers/tests: "not sending" is expressed as the vanilla mode.
        if (!enabled)
        {
            SetMultiplayerSkinMode(MultiplayerSkinMode.Vanilla);
        }
    }

    public static void SetReceiveMultiplayerSkinChanges(bool enabled)
    {
        if (enabled)
        {
            if (GetMultiplayerSkinMode() == MultiplayerSkinMode.Vanilla)
            {
                SetMultiplayerSkinMode(MultiplayerSkinMode.Shared);
            }

            return;
        }

        SetMultiplayerSkinMode(MultiplayerSkinMode.Vanilla);
    }

    public static bool ShouldShowInRunAppearanceEntry()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.ShowInRunAppearanceEntry;
        }
    }

    public static void SetShowInRunAppearanceEntry(bool enabled)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            if (Config.ShowInRunAppearanceEntry == enabled)
            {
                return;
            }

            Config.ShowInRunAppearanceEntry = enabled;
            Config.Save(ConfigPath);
        }
    }

    public static bool ShouldShowInRunBackgroundEntry()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.ShowInRunBackgroundEntry;
        }
    }

    public static void SetShowInRunBackgroundEntry(bool enabled)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            if (Config.ShowInRunBackgroundEntry == enabled)
            {
                return;
            }

            Config.ShowInRunBackgroundEntry = enabled;
            Config.Save(ConfigPath);
        }
    }

    public static bool ShouldPlaceCharacterSelectorTopRight()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.CharacterSelectorTopRight;
        }
    }

    public static (float X, float Y)? GetCharacterSkinControlPosition(bool mergeButton)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            var x = mergeButton ? Config.CharacterSkinMergeX : Config.CharacterSkinSelectorX;
            var y = mergeButton ? Config.CharacterSkinMergeY : Config.CharacterSkinSelectorY;
            return x is { } px && y is { } py && float.IsFinite(px) && float.IsFinite(py)
                ? (Math.Clamp(px, 0f, 1f), Math.Clamp(py, 0f, 1f))
                : null;
        }
    }

    public static void SetCharacterSkinControlPosition(bool mergeButton, float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y))
        {
            return;
        }

        lock (Sync)
        {
            EnsureConfigLoaded();
            if (mergeButton)
            {
                Config.CharacterSkinMergeX = Math.Clamp(x, 0f, 1f);
                Config.CharacterSkinMergeY = Math.Clamp(y, 0f, 1f);
            }
            else
            {
                Config.CharacterSkinSelectorX = Math.Clamp(x, 0f, 1f);
                Config.CharacterSkinSelectorY = Math.Clamp(y, 0f, 1f);
            }
            Config.Save(ConfigPath);
        }
    }

    public static void ResetCharacterSkinControlPosition(bool mergeButton)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            if (mergeButton)
            {
                Config.CharacterSkinMergeX = null;
                Config.CharacterSkinMergeY = null;
            }
            else
            {
                Config.CharacterSkinSelectorX = null;
                Config.CharacterSkinSelectorY = null;
                // Keep the old preference until the player explicitly resets the position.
                Config.CharacterSelectorTopRight = false;
            }
            Config.Save(ConfigPath);
        }
    }

    public static bool TryGetIndividualCardSkinSelectorPosition(out float x, out float y)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            if (Config.IndividualCardSkinSelectorX is { } storedX &&
                Config.IndividualCardSkinSelectorY is { } storedY &&
                float.IsFinite(storedX) &&
                float.IsFinite(storedY))
            {
                x = Math.Clamp(storedX, 0f, 1f);
                y = Math.Clamp(storedY, 0f, 1f);
                return true;
            }

            x = 0f;
            y = 0f;
            return false;
        }
    }

    public static void SetIndividualCardSkinSelectorPosition(float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y))
        {
            return;
        }

        lock (Sync)
        {
            EnsureConfigLoaded();
            Config.IndividualCardSkinSelectorX = Math.Clamp(x, 0f, 1f);
            Config.IndividualCardSkinSelectorY = Math.Clamp(y, 0f, 1f);
            Config.Save(ConfigPath);
        }
    }

    public static void ResetIndividualCardSkinSelectorPosition()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            if (Config.IndividualCardSkinSelectorX == null &&
                Config.IndividualCardSkinSelectorY == null)
            {
                return;
            }

            Config.IndividualCardSkinSelectorX = null;
            Config.IndividualCardSkinSelectorY = null;
            Config.Save(ConfigPath);
        }
    }

    public static IReadOnlyList<string> GetRespectedLoadOrderProviders()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.RespectedLoadOrderProviders;
        }
    }

    public static void SetRespectedLoadOrderProviders(IEnumerable<string> providerIds)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            var ids = providerIds.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            if (ids.Length == Config.RespectedLoadOrderProviders.Count &&
                ids.SequenceEqual(Config.RespectedLoadOrderProviders, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            Config.RespectedLoadOrderProviders = [.. ids];
            Config.Save(ConfigPath);
        }
    }

    public static bool HasLoadOrderHistory()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.LastKnownBeforeAllSkinMods != null ||
                   Config.LastKnownFirstInLoadOrder != null ||
                   Config.LastKnownSkinProviderOrder != null;
        }
    }

    public static IReadOnlyList<string>? GetLastKnownSkinProviderOrder()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.LastKnownSkinProviderOrder;
        }
    }

    public static void SetLastKnownSkinProviderOrder(IEnumerable<string> providerIds)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            var ids = providerIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (Config.LastKnownSkinProviderOrder != null &&
                ids.Length == Config.LastKnownSkinProviderOrder.Count &&
                ids.SequenceEqual(Config.LastKnownSkinProviderOrder, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }

            Config.LastKnownSkinProviderOrder = [.. ids];
            Config.Save(ConfigPath);
        }
    }

    public static bool ShouldShowLoadOrderWarning(bool isBeforeAllSkinMods)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            var previousSafeState = Config.LastKnownBeforeAllSkinMods == true ||
                                    Config.LastKnownFirstInLoadOrder == true;
            var movedBehindSkinMod = previousSafeState && !isBeforeAllSkinMods;
            LoadOrderMovedBehindSkinMods = movedBehindSkinMod;
            var stateChanged = Config.LastKnownBeforeAllSkinMods != isBeforeAllSkinMods;
            if (movedBehindSkinMod)
            {
                Config.SuppressLoadOrderWarning = false;
                ModLog.Info("检测到有皮肤 Mod 被移到本 Mod 之前，已恢复加载顺序提醒。");
            }

            Config.LastKnownBeforeAllSkinMods = isBeforeAllSkinMods;
            Config.LastKnownFirstInLoadOrder = isBeforeAllSkinMods;
            if (stateChanged || movedBehindSkinMod)
            {
                Config.Save(ConfigPath);
            }

            // 只有「排序确实变了」并且「确实有皮肤 Mod 排在本 Mod 之前」时才提醒，
            // 否则每次启动都会因为别的运行时差异误报。
            var shouldShow = !isBeforeAllSkinMods && (stateChanged || movedBehindSkinMod);
            if (isBeforeAllSkinMods)
            {
                _loadOrderAutoReorderLogged = false;
            }
            else if (!_loadOrderAutoReorderLogged)
            {
                var legacySuppression = Config.SuppressLoadOrderWarning;
                ModLog.Info(legacySuppression
                    ? "检测到皮肤 Mod 排在本 Mod 之前；旧版“不再提示”设置不再生效，正在核对玩家排序。"
                    : "检测到皮肤 Mod 排在本 Mod 之前，正在核对玩家排序。");
                _loadOrderAutoReorderLogged = true;
            }

            return shouldShow;
        }
    }

    public static void EnsureConfigLoaded()
    {
        lock (Sync)
        {
            if (!_configLoaded)
            {
                Config = LoadConfig();
                _configLoaded = true;
                // Configs written before the three-mode UI carry two independent switches instead
                // of a mode. Migrate once, then always persist a real mode.
                if (Config.MultiplayerSkinMode < 0)
                {
                    var mode = MultiplayerSkinModePolicy.FromLegacyToggles(
                        Config.MultiplayerSkinSyncEnabled,
                        Config.LoadOtherPlayersCustomSkins);
                    Config.MultiplayerSkinMode = (int)mode;
                    try
                    {
                        Config.Save(ConfigPath);
                    }
                    catch (Exception exception)
                    {
                        ModLog.Warn("保存联机皮肤显示方式失败：" + exception.GetBaseException().Message);
                    }
                }
            }
        }
    }

    public static void InitializeBeforeAssets()
    {
        lock (Sync)
        {
            if (_initialized)
            {
                return;
            }

            try
            {
                Catalog?.Dispose();
                Catalog = null;
                RuntimeResourceCache.Clear();
                PreparedRuntimeOverlays.Clear();
                RuntimeResourceBundles.Clear();
                CardPortraitCache.Clear();
                ExternalCardProviderIdentityPaths.Clear();
                BaselineRelicAtlasCache.Clear();
                BaselineRelicIconCache.Clear();
                FailedCardPortraitRequests.Clear();
                IsolatedCardOverlayCache.Clear();
                CardCanonicalResourceOwners.Reset();
                CharacterPreviewSelections.Clear();
                _cardLookupCache = new ConditionalWeakTable<CardModel, CardLookup>();
                AncientStyleMethods.Clear();
                MissingAncientStyleMethods.Clear();
                FailedAncientStyleMethods.Clear();
                MountedOverlayCache.Clear();
                _cachedOverlayPlan = null;
                MountedOverlayOwners.Clear();
                _mountedOverlayBytes = 0;
                _mountedOverlayLayers = 0;
                _nextMountedOverlayWarningBytes = 2L * 1024 * 1024 * 1024;
                // 第三方随从 Mod 的发现/适配器按每次初始化重扫一遍：Mod 列表可能在重载后变化。
                SummonModCompat.Reset();
                MountedScopedRuntimeProviderPacks.Clear();
                MountedLargeRuntimeProviderPacks.Clear();
                RuntimeCanonicalDependencyPaths.Clear();
                WarmedRuntimeProviderPacks.Clear();
                WarmingRuntimeProviderPacks.Clear();
                _runtimePackWarmGeneration++;
                ScopedMonsterSelections.Replace(
                    new Dictionary<string, IReadOnlyCollection<string>>(
                        StringComparer.OrdinalIgnoreCase));
                _runtimeProviderBehaviorScope = null;
                RuntimeProviderScopeLeases.Reset();
                // 启动阶段"延后初始化"的队列属于上一份目录，重载后必须清空。
                DeferredProviderActivator.Reset();
                MountedLocalizationFiles.Clear();
                LocalizationStateCache.Clear();
                _mountedLocalizationSignature = null;
                var cleaned = SkinChangerPaths.Storage.CleanCaches();
                if (cleaned.RemovedFiles > 0) ModLog.Info($"已清理 {cleaned.RemovedFiles} 个过期皮肤缓存文件。");
                foreach (var error in cleaned.Errors) ModLog.Warn("部分旧缓存清理失败：" + error);
                // 退出清理在崩溃/卡死/强退时会整份跳过，孤儿会话会堆到几十 GB；这里按总量再兜一次底。
                var trimmed = SkinChangerPaths.Storage.TrimCacheBudget(8L * 1024 * 1024 * 1024);
                if (trimmed.RemovedSessions > 0)
                    ModLog.Info($"皮肤缓存超出 8 GB 预算，已删除 {trimmed.RemovedSessions} 个旧会话（{trimmed.RemovedBytes / (1024 * 1024)} MB），剩余约 {trimmed.RemainingBytes / (1024 * 1024)} MB。");
                foreach (var error in trimmed.Errors) ModLog.Warn("皮肤缓存预算清理失败：" + error);
                var gamePckPath = GamePackLocator.Resolve(
                    OS.GetExecutablePath(),
                    GameCommandLine.Arguments(),
                    message => ModLog.Warn("定位游戏主资源包：" + message));
                ModLog.Info($"已定位游戏主资源包：{gamePckPath}");
                var loadedMods = ManagedSkinModLoader.GetCatalogMods()
                    .Where(mod => mod.manifest is { id: not null })
                    .Where(mod => !Entry.IsSelfModId(mod.manifest!.id))
                    .ToArray();
                var mods = loadedMods
                    .Select(mod => new SkinModDescriptor(
                        mod.manifest!.id!,
                        mod.manifest.name ?? mod.manifest.id!,
                        mod.manifest.hasPck
                            ? SkinPackagePaths.Resolve(mod.path, mod.manifest.id!, ".pck")
                            : null,
                        ManagedSkinModLoader.ShouldTreatAsGameplayBaseline(mod, loadedMods),
                        mod.path,
                        mod.manifest.hasDll))
                    .ToArray();

                Catalog = SkinCatalog.Build(gamePckPath, mods);
                // 外观覆盖包的跨会话缓存：材质戳 = 游戏主包 + 每个皮肤包的大小/修改时间。
                // 任何一方更新都会让它变化，从而整目录作废——这是"缓存不会挂到旧皮肤"的保证。
                OverlayCacheStore.BeginSession(OverlayCacheStore.ComputeStamp(
                    gamePckPath,
                    mods.Where(mod => !string.IsNullOrWhiteSpace(mod.PckPath))
                        .Select(mod => mod.PckPath!)));
                _workshopGamePack = gamePckPath;
                _workshopDescriptors = mods;
                _workshopSourceStamps = WorkshopSourceStamps(mods);
                ManagedCharacterAssetRegistrationGuard.Configure(Catalog);
                Config = LoadConfig();
                _configLoaded = true;
                // 玩家手动选过随从外观后就不再按主人皮肤自动推导（老行为保留给老存档）。
                Catalog.CompanionAutoFollow = !Config.CompanionSelectionDecoupled;
                OstyImpostorCompat.Decoupled = Config.CompanionSelectionDecoupled;
                // 接管独立奥斯提 Mod：禁用它在选人界面的接管 + 绑定它自己的换肤按钮。
                CharSelectTakeoverSuppressor.Install(OnImpostorSelectionChanged);
                SanitizeSelections();
                MountOverlay(Catalog.Groups.Select(group => group.Id).ToHashSet(StringComparer.OrdinalIgnoreCase));
                Config.Save(ConfigPath);
                // 仅在完整成功后才标记已初始化，失败时允许后续调用重试而不是整个会话失效。
                _initialized = true;
                ModLog.Info(
                    $"发现 {Catalog.Groups.Count} 个生物外观组和 {Catalog.CardGroups.Count} 个卡牌外观组。" +
                    "角色、怪物、先古之民与卡牌选项已接入对应界面。");
            }
            catch (Exception exception)
            {
                Catalog?.Dispose();
                Catalog = null;
                LastError = exception.ToString();
                ModLog.Error("初始化失败：" + exception);
            }
        }
    }

    public static void InitializeCardGroupsAfterModels()
    {
        lock (Sync)
        {
            if (Catalog == null || _cardGroupsInitialized)
            {
                return;
            }

            try
            {
                FrameworkCompatibilityLayer.SynchronizeSelections(
                    Catalog,
                    GetVisualSelections());
                InitializeMonsterSkinCategoriesAfterModels();
                InitializeEventSkinCategoriesAfterModels();
                var entries = BuildCardCatalogEntries(ModelDb.AllCards);
                RegisterCardArtPacks(entries);

                Catalog.FinalizeCardGroups(entries);
                _cardLookupCache = new ConditionalWeakTable<CardModel, CardLookup>();
                CardCoverageCache.Clear();
                MigrateStoredCardSelections();
                SanitizeCardSelections();
                MigrateLegacyCardSkinPresets();
                SanitizeCardSkinPresets();
                BundlePresetPolicy.Synchronize(Config, Catalog.CardGroups.Select(group => group.Id),
                    Config.MonsterSkinCategoryGroups.Keys);
                MountCardOverlay(Catalog.CardGroups
                    .Select(group => group.Id)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase));
                Config.Save(ConfigPath);
                _cardGroupsInitialized = true;
                LastError = null;
                ModLog.Info($"已按卡牌总览分类接入 {Catalog.CardGroups.Count} 个卡牌外观组。");
            }
            catch (Exception exception)
            {
                LastError = exception.ToString();
                ModLog.Error("按卡牌总览分类卡牌皮肤失败：" + exception);
            }
        }
    }

    /// <summary>
    /// Takes over pure asset card-art packs. Such a pack is a plain JSON bundle of card artwork
    /// with no code, no PCK and no manifest, so the game's mod loader never registers it and it
    /// would otherwise only appear through Card Art Editor's runtime manager. Reading the bundle
    /// directly makes the artwork usable without requiring that accompanying mod.
    ///
    /// A provider can also keep its card art as ordinary image files beside its code. That code is
    /// held back until the player selects it, which would leave the artwork permanently invisible,
    /// so those folders are read here as well and offered on the same rows as a bundle.
    /// </summary>
    private static void RegisterCardArtPacks(IEnumerable<CardCatalogEntry> cards)
    {
        var catalog = Catalog;
        if (catalog == null)
        {
            return;
        }

        try
        {
            var cardEntries = cards as IReadOnlyList<CardCatalogEntry> ?? cards.ToArray();
            var packs = new List<CardArtPack>();
            var roots = CardArtPackScanRoots();
            if (roots.Count > 0)
            {
                var scan = CardArtPackScanner.Scan(roots);
                foreach (var warning in scan.Warnings)
                {
                    ModLog.Warn(warning);
                }

                packs.AddRange(scan.Packs);
            }

            packs.AddRange(LooseCardPortraitPackScanner.Scan(LooseCardPortraitProviders(), cardEntries));
            if (packs.Count == 0)
            {
                return;
            }

            var (cardTypes, usedPacks) = catalog.AttachCardArtPacks(packs, cards);
            if (cardTypes.Count == 0)
            {
                return;
            }

            // Each card renders through the layout its artwork was composed for, so an
            // accompanying editor must yield those layers.
            ManagedSkinModLoader.RememberArtPackProviders(usedPacks.Select(pack => pack.Id));
            ExternalCardVisualBridge.RegisterArtPackCoverage(cardTypes);
            ModLog.Info(
                $"已接管 {usedPacks.Count} 个纯素材卡图包（{string.Join("、", usedPacks.Select(pack => pack.Name))}）；" +
                $"覆盖 {cardTypes.Count} 张卡牌，无需 Card Art Editor 即可选用。");
        }
        catch (Exception exception)
        {
            // An unreadable pack must never block the normal catalog from finishing.
            ModLog.Warn("接管纯素材卡图包失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// The Workshop content folder that holds installed mods. A card-art pack has no manifest,
    /// so it is not visible through the game's mod list and must be discovered on disk.
    /// </summary>
    private static IReadOnlyList<string> CardArtPackScanRoots()
    {
        var roots = new List<string>();
        foreach (var mod in ManagedSkinModLoader.GetCatalogMods())
        {
            var directory = Path.GetDirectoryName(mod.path);
            if (!string.IsNullOrWhiteSpace(directory) && !roots.Contains(directory, StringComparer.OrdinalIgnoreCase))
            {
                roots.Add(directory);
            }
        }

        return roots;
    }

    /// <summary>
    /// The installed providers that may keep card art beside their code. Each provider folder is
    /// offered on its own: the scanner skips the ones that mount a package or ship a bundle, and
    /// this Mod's own folder is never a source of card art.
    /// </summary>
    private static IReadOnlyList<LooseCardPortraitProvider> LooseCardPortraitProviders()
    {
        var providers = new List<LooseCardPortraitProvider>();
        foreach (var mod in ManagedSkinModLoader.GetCatalogMods())
        {
            var id = mod.manifest?.id;
            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(mod.path) ||
                Entry.IsSelfModId(id))
            {
                continue;
            }

            providers.Add(new LooseCardPortraitProvider(id, mod.manifest?.name ?? id, mod.path));
        }

        return providers;
    }

    private static CardCatalogEntry[] BuildCardCatalogEntries(IEnumerable<CardModel> cards)
    {
        var entries = new List<CardCatalogEntry>();
        foreach (var card in cards)
        {
            try
            {
                if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var path)) continue;
                entries.Add(new CardCatalogEntry(card.GetType().Name, path,
                    GetCardPoolGroupId(card), GetCardCatalogGroupId(card), GetCardFilterGroupId(card))
                { IsCharacterPool = IsCharacterCardPool(card) });
            }
            catch (Exception exception)
            {
                FrameworkCardVisualGuard.ReportCardReadFailure(card, exception);
            }
        }
        return entries.ToArray();
    }

    private static void InitializeMonsterSkinCategoriesAfterModels()
    {
        var catalog = Catalog!;
        var registered = 0;
        foreach (var act in ModelDb.Acts)
        {
            var groupIds = ResolveMonsterSkinGroupIds(catalog, act.AllMonsters);
            if (RegisterMonsterSkinCategory(
                    "act:" + act.Id.Entry.ToLowerInvariant(),
                    groupIds))
            {
                registered++;
            }
        }

        var eventEncounters = typeof(ModelDb).GetProperty("EventEncounters")?.GetValue(null)
            as IEnumerable<EncounterModel>;
        if (eventEncounters != null && RegisterMonsterSkinCategory(
                "events",
                ResolveMonsterSkinGroupIds(
                    catalog,
                    eventEncounters.SelectMany(encounter => encounter.AllPossibleMonsters))))
        {
            registered++;
        }

        if (registered > 0)
        {
            ModLog.Info($"已按怪物图鉴登记 {registered} 个地区的怪物皮肤优先级。");
        }
    }

    private static IReadOnlyList<string> ResolveMonsterSkinGroupIds(
        SkinCatalog catalog,
        IEnumerable<MonsterModel> monsters) =>
        monsters
            .Select(monster => catalog.ResolveManagedMonsterGroupId(monster.Id.Entry) ??
                               catalog.ResolveManagedMonsterGroupId(monster.GetType().Name))
            .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    /// <summary>
    /// Builds the selected character-preview packs while the game's own startup loading screen is
    /// still active. No Godot resources are decoded here; the expensive PCK graph walk and write
    /// are simply moved out of the first character click and the result is reused for the session.
    /// </summary>
    public static void PrepareSelectedCharacterPreviews()
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null)
            {
                return;
            }

            var started = Stopwatch.GetTimestamp();
            var prepared = 0;
            var preparedRelicBundles = 0;
            foreach (var character in ModelDb.AllCharacters)
            {
                var groupId = character.Id.Entry.ToLowerInvariant();
                var group = catalog.Groups.FirstOrDefault(candidate =>
                    candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
                if (group == null)
                {
                    continue;
                }

                var selection = Config.GetSelection(group.Id);
                if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) ||
                    (catalog.IsRuntimeProviderOption(group.Id, selection) &&
                     !catalog.IsResourceBackedOption(group.Id, selection) &&
                     catalog.GetRuntimeImagePath(group.Id, selection) != null))
                {
                    continue;
                }

                var paths = CharacterSelectResourcePaths(groupId);
                _ = GetOrPrepareRuntimeOverlay(
                    catalog,
                    group.Id,
                    selection,
                    paths,
                    includeProviderDependencies: true);
                prepared++;

                var selected = group.Options.FirstOrDefault(option => option.Id.Equals(
                    selection,
                    StringComparison.OrdinalIgnoreCase));
                if (selected != null)
                {
                    var relicPaths = catalog.GetProviderRelicSpritePaths(selected);
                    if (relicPaths.Count > 0)
                    {
                        _ = GetOrPrepareRuntimeOverlay(
                            catalog,
                            group.Id,
                            selection,
                            relicPaths,
                            includeProviderDependencies: false,
                            isolateRelicCanonicalPaths: true);
                        preparedRelicBundles++;
                    }
                }
            }

            if (prepared > 0)
            {
                ModLog.Info(
                    $"已在启动阶段准备 {prepared} 个角色的选角资源包，" +
                    $"其中 {preparedRelicBundles} 套包含完整遗物图集；" +
                    $"耗时={Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1} ms。");
            }
        }
    }

    private static string? _lastPrewarmedRuntimeSceneKey;

    /// <summary>
    /// Prepares the per-selection overlay pack of one live-room scene before the room is entered
    /// (F40: shop entry). Same subset of work <see cref="InstantiateRuntimeScene{T}"/> later runs;
    /// no selection, scope or mount changes. One attempt per selection; a failure also counts as
    /// attempted and only retries after the selection changes.
    /// </summary>
    public static void PrewarmRuntimeSceneOverlay(string groupId, string scenePath)
    {
        try
        {
            lock (Sync)
            {
                var catalog = Catalog;
                if (catalog == null)
                {
                    return;
                }

                var selection = GetVisualSelection(groupId);
                if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var key = groupId + "|" + selection + "|" + scenePath;
                if (key.Equals(_lastPrewarmedRuntimeSceneKey, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                _lastPrewarmedRuntimeSceneKey = key;
                var started = Stopwatch.GetTimestamp();
                _ = GetOrPrepareRuntimeOverlay(
                    catalog,
                    groupId,
                    selection,
                    RuntimeSceneResourcePaths(groupId, scenePath),
                    includeProviderDependencies: true);
                var elapsed = Stopwatch.GetElapsedTime(started);
                if (elapsed >= TimeSpan.FromMilliseconds(30))
                {
                    ModLog.Info(
                        $"已提前准备 {groupId} 的房间资源包；" +
                        $"耗时={elapsed.TotalMilliseconds:F1} ms。");
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Info(
                $"提前准备 {groupId} 房间资源未完成，进入房间时将继续按原路径加载：" +
                exception.GetBaseException().Message);
        }
    }

    public static void FocusRuntimeProviderBehaviorsOnCharacters(IEnumerable<string> groupIds)
        => FocusRuntimeProviderBehaviorsOnGroups(
            groupIds,
            runEnvironmentProviderIds: [],
            reason: "角色预览");

    public static long FocusRuntimeProviderBehaviorsOnGroups(
        IEnumerable<string> groupIds,
        IReadOnlyCollection<string> runEnvironmentProviderIds,
        string reason)
    {
        lock (Sync)
        {
            var nextScope = groupIds
                .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var nextRunEnvironmentProviders = runEnvironmentProviderIds
                .Where(providerId => !string.IsNullOrWhiteSpace(providerId))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (_runtimeProviderBehaviorScope is { } currentScope &&
                currentScope.RunEnvironmentProviderIds.ToHashSet(
                    StringComparer.OrdinalIgnoreCase).SetEquals(nextRunEnvironmentProviders) &&
                currentScope.VisibleGroupIds.ToHashSet(
                    StringComparer.OrdinalIgnoreCase).SetEquals(nextScope))
            {
                // Reuse resources/patches, not lifecycle ownership. A new room can request
                // the same providers as an outgoing room or its compendium preview.
                return RuntimeProviderScopeLeases.Claim();
            }

            _runtimeProviderBehaviorScope = new RuntimeProviderScope(
                nextScope,
                nextRunEnvironmentProviders);
            var scopeLease = RuntimeProviderScopeLeases.Claim();
            var catalog = Catalog;
            if (catalog == null)
            {
                return scopeLease;
            }

            var activeProviders = GetActiveRuntimeProviders(catalog);
            // 真正的场景出现了：从这一刻起由场景范围决定谁该激活。启动阶段的延后队列必须
            // 停掉，否则它会在后台把刚被收窄掉的行为补丁又装回去。
            DeferredProviderActivator.Cancel(reason);
            ManagedSkinModLoader.ConfigureRunEnvironmentProviders(nextRunEnvironmentProviders);
            ManagedSkinModLoader.DeactivateProvidersExcept(activeProviders);
            EnsureScopedRuntimeProviderResourcesMounted(catalog, activeProviders);
            foreach (var providerId in activeProviders.Where(
                         catalog.ProviderUsesManagedGodotScripts))
            {
                ManagedSkinModLoader.EnsureProviderGodotScripts(providerId);
            }
            foreach (var providerId in activeProviders.Where(
                         catalog.ProviderUsesScopedMonsterRuntime))
            {
                ManagedSkinModLoader.EnsureScopedMonsterSelectionRouter(providerId);
            }
            ManagedSkinModLoader.ActivateSelectedProviders(activeProviders);
            // Provider initializers can write character paths into a framework-owned registry.
            // Suppress and clear those writes after every activation so deselected providers
            // cannot keep controlling combat, merchant, rest-site or UI scenes indirectly.
            ManagedCharacterAssetRegistrationGuard.SuppressCurrentRegistrations();
            ScheduleRuntimeProviderPackWarm(catalog, activeProviders, reason);
            ModLog.Info(
                $"已按{reason}将皮肤代码行为收窄到 {nextScope.Count} 个可见外观组；" +
                $"保留 {activeProviders.Count} 个当前场景需要的 DLL 皮肤提供者。");
            return scopeLease;
        }
    }

    public static bool TryFocusRuntimeProviderBehaviorsOnGroups(
        long expectedScopeLease,
        IEnumerable<string> groupIds,
        IReadOnlyCollection<string> runEnvironmentProviderIds,
        string reason,
        out long scopeLease)
    {
        lock (Sync)
        {
            if (!RuntimeProviderScopeLeases.IsCurrent(expectedScopeLease))
            {
                scopeLease = RuntimeProviderScopeLeases.Current;
                return false;
            }

            scopeLease = FocusRuntimeProviderBehaviorsOnGroups(
                groupIds,
                runEnvironmentProviderIds,
                reason);
            return true;
        }
    }

    public static IReadOnlyList<string> GetSelectionResourcePackPaths(
        string groupId,
        string optionId)
    {
        lock (Sync)
        {
            var group = Catalog?.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            var option = group?.Options.FirstOrDefault(candidate =>
                candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
            if (option == null || !option.IsRuntimeProvider)
            {
                return [];
            }

            return Catalog!.GetProviderResourcePackPaths(option.EffectiveProviderId)
                .Select(System.IO.Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(File.Exists)
                .ToArray();
        }
    }

    public static async Task WarmResourcePackFilesAsync(
        IReadOnlyCollection<string> resourcePackPaths,
        Action<double>? reportProgress = null,
        CancellationToken cancellationToken = default)
    {
        var paths = resourcePackPaths
            .Where(File.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var totalBytes = paths.Sum(path => new FileInfo(path).Length);
        if (totalBytes <= 0)
        {
            reportProgress?.Invoke(1d);
            return;
        }

        var completedBytes = 0L;
        var buffer = GC.AllocateUninitializedArray<byte>(1024 * 1024);
        foreach (var path in paths)
        {
            await using var stream = new FileStream(
                path,
                FileMode.Open,
                System.IO.FileAccess.Read,
                FileShare.Read | FileShare.Delete,
                buffer.Length,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            while (true)
            {
                var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read == 0)
                {
                    break;
                }

                completedBytes += read;
                reportProgress?.Invoke(Math.Clamp((double)completedBytes / totalBytes, 0d, 1d));
            }
        }

        reportProgress?.Invoke(1d);
    }

    private static void ScheduleRuntimeProviderPackWarm(
        SkinCatalog catalog,
        IEnumerable<string> activeProviders,
        string reason)
    {
        var generation = ++_runtimePackWarmGeneration;
        var paths = new List<string>();
        foreach (var path in activeProviders
                     .SelectMany(catalog.GetProviderResourcePackPaths)
                     .Select(System.IO.Path.GetFullPath)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                var alreadyWarmed = WarmedRuntimeProviderPacks.Contains(path) ||
                                    WarmingRuntimeProviderPacks.Contains(path);
                if (!File.Exists(path) ||
                    !RuntimePackWarmPolicy.ShouldWarm(
                        new FileInfo(path).Length,
                        alreadyWarmed))
                {
                    continue;
                }

                WarmingRuntimeProviderPacks.Add(path);
                paths.Add(path);
            }
            catch (Exception exception)
            {
                ModLog.Info(
                    $"跳过无法检查的当前皮肤资源包 {System.IO.Path.GetFileName(path)}：" +
                    exception.GetBaseException().Message);
            }
        }

        if (paths.Count > 0)
        {
            _ = WarmRuntimeProviderPacksAsync(paths, reason, generation);
        }
    }

    private static async Task WarmRuntimeProviderPacksAsync(
        IReadOnlyCollection<string> paths,
        string reason,
        int generation)
    {
        var started = Stopwatch.GetTimestamp();
        await RuntimePackWarmGate.WaitAsync().ConfigureAwait(false);
        try
        {
            lock (Sync)
            {
                if (generation != _runtimePackWarmGeneration)
                {
                    foreach (var path in paths)
                    {
                        WarmingRuntimeProviderPacks.Remove(path);
                    }
                    return;
                }
            }

            await WarmResourcePackFilesAsync(paths).ConfigureAwait(false);
            lock (Sync)
            {
                foreach (var path in paths)
                {
                    WarmingRuntimeProviderPacks.Remove(path);
                    WarmedRuntimeProviderPacks.Add(path);
                }
            }

            var elapsed = Stopwatch.GetElapsedTime(started);
            if (elapsed >= TimeSpan.FromMilliseconds(50))
            {
                ModLog.Info(
                    $"已后台预读{reason}所需的 {paths.Count} 个当前皮肤资源包；" +
                    $"耗时={elapsed.TotalMilliseconds:F1} ms。");
            }
        }
        catch (Exception exception)
        {
            lock (Sync)
            {
                foreach (var path in paths)
                {
                    WarmingRuntimeProviderPacks.Remove(path);
                }
            }

            ModLog.Info(
                $"后台预读{reason}皮肤资源未完成，将在实际加载时继续：" +
                exception.GetBaseException().Message);
        }
        finally
        {
            RuntimePackWarmGate.Release();
        }
    }

    public static bool ApplySelection(string groupId, string optionId)
    {
        // Any appearance change invalidates the memoised per-variant portraits.
        VariantPortraits.ClearCache();

        if (optionId.Equals(InheritEventSelectionId, StringComparison.OrdinalIgnoreCase))
            return FollowEventCategoryPriority(groupId);
        // The category entry is an instruction, not a resource provider. Keep this in the
        // shared selection path so queued in-run choices retain their follow-category state.
        if (optionId.Equals(InheritMonsterSelectionId, StringComparison.OrdinalIgnoreCase))
        {
            return FollowMonsterCategoryPriority(groupId);
        }

        lock (Sync)
        {
            if (Catalog == null)
            {
                LastError = "皮肤目录尚未初始化。";
                return false;
            }

            // 随从分组走独立事务：保证它永远不会联动到主人分组，反之亦然。
            if (Catalog.IsCompanionGroup(groupId))
            {
                return ApplyCompanionGroupSelection(groupId, optionId);
            }

            var group = Catalog.Groups.FirstOrDefault(group =>
                group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group == null ||
                (!optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                 group.Options.All(option =>
                     !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
            {
                LastError = $"未知的皮肤选择：{groupId}/{optionId}";
                return false;
            }

            // F29 取证：选角界面切回基础选择时报告过整帧卡死闪退。这行入口日志让 godot.log 能分辨
            // 卡死发生在选择事务、挂载期间还是挂载后的延迟重建（对照「开始重建外观覆盖」与「已挂载」）。
            ModLog.Info($"开始应用皮肤选择：{groupId} → {optionId}。");

            var updates = Catalog.BuildVisualSelectionTransaction(
                groupId,
                optionId,
                Config.Selections);
            var previousSelections = updates.Keys.ToDictionary(
                key => key,
                key => Config.Selections.TryGetValue(key, out var previous)
                    ? (HadValue: true, Value: (string?)previous)
                    : (HadValue: false, Value: (string?)null),
                StringComparer.OrdinalIgnoreCase);
            var previousVisualProviderPriority = Config.VisualProviderPriority.ToList();
            var previousFollowingGroups = Config.MonsterGroupsFollowingCategory.ToList();
            var previousManualGroups = Config.MonsterGroupsWithManualSelection.ToList();
            var previousEventManualGroups = Config.EventSkinPriorities.ManualGroups.ToList();
            var previousAncientManualGroups = Config.AncientSkinPriorities.ManualGroups.ToList();
            var previousActiveMonsterPresets = new Dictionary<string, string>(
                Config.ActiveMonsterSkinPresets,
                StringComparer.OrdinalIgnoreCase);
            var affectedGroups = updates.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);
            // Variants of one package ship identical files, so the runtime resource cache stays valid:
            // reusing it keeps the (needed) display rebuild from re-packing tens of MiB every hotkey press.
            var sameFamilyVariant = ProviderSettingsVariantOptions.IsSamePackageVariant(
                Config.GetSelection(groupId),
                optionId);
            var stateOnly = IsSameManualVariantSource(groupId, optionId) && affectedGroups.SetEquals([groupId]);
            try
            {
                foreach (var id in affectedGroups.Where(EventSkinPolicy.IsEventGroup))
                    if (!Config.EventSkinPriorities.ManualGroups.Contains(id, StringComparer.OrdinalIgnoreCase))
                        Config.EventSkinPriorities.ManualGroups.Add(id);
                foreach (var id in affectedGroups.Where(HasAncientSkinCategory))
                    if (!Config.AncientSkinPriorities.ManualGroups.Contains(id, StringComparer.OrdinalIgnoreCase))
                        Config.AncientSkinPriorities.ManualGroups.Add(id);
                foreach (var categoryId in Config.MonsterSkinCategoryGroups
                             .Where(pair => pair.Value.Any(affectedGroups.Contains))
                             .Select(pair => pair.Key))
                {
                    Config.ActiveMonsterSkinPresets.Remove(categoryId);
                }
                Config.MonsterGroupsFollowingCategory.RemoveAll(candidate =>
                    affectedGroups.Contains(candidate));
                foreach (var affectedGroup in affectedGroups.Where(affectedGroup =>
                             Config.MonsterSkinCategoryGroups.Values.Any(groupIds =>
                                 groupIds.Contains(affectedGroup, StringComparer.OrdinalIgnoreCase)) &&
                             !Config.MonsterGroupsWithManualSelection.Contains(
                                 affectedGroup,
                                 StringComparer.OrdinalIgnoreCase)))
                {
                    Config.MonsterGroupsWithManualSelection.Add(affectedGroup);
                }

                foreach (var update in updates)
                {
                    Config.Selections[update.Key] = update.Value;
                    if (!stateOnly) ClearRuntimeResourceCache(update.Key);
                }

                UpdateVisualProviderPriority(groupId, optionId);
                if (!stateOnly)
                {
                    // A cross-group full package that is being replaced may still sit on top of
                    // Godot's pack stack; only an all-group rebuild re-asserts every canonical path.
                    foreach (var update in updates)
                    {
                        if (Config.GetSelection(update.Key).Equals(
                                update.Value,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var previousProvider = Catalog.ResolveVisualProviderId(
                            Config.GetSelection(update.Key));
                        if (Catalog.ProviderUsesFullRuntime(previousProvider) &&
                            Catalog.GetFullRuntimeProviderGroups(previousProvider).Count > 1)
                        {
                            RequestFullOverlayReassert(
                                $"多分组提供者 {previousProvider} 在 {update.Key} 上被替换");
                            break;
                        }
                    }

                    // Third-party packs cache the resources they swapped; drop those caches so they
                    // reload from the skin that is selected *now* (both sides of the switch).
                    ProviderRuntimeCacheReset.ResetForSelections(
                        updates.Keys
                            .Select(key => Config.GetSelection(key))
                            .Concat(updates.Values)
                            .ToArray());
                    MountOverlay(affectedGroups);
                }
                Config.Save(ConfigPath);
                ManualCharacterVariantBridge.ApplyLocalChoice(groupId);
                EventSkinRuntime.RefreshCurrent(groupId);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                foreach (var previous in previousSelections)
                {
                    RestoreSelection(previous.Key, previous.Value.Value, previous.Value.HadValue);
                    ClearRuntimeResourceCache(previous.Key);
                }
                Config.VisualProviderPriority = previousVisualProviderPriority;
                Config.MonsterGroupsFollowingCategory = previousFollowingGroups;
                Config.MonsterGroupsWithManualSelection = previousManualGroups;
                Config.EventSkinPriorities.ManualGroups = previousEventManualGroups;
                Config.AncientSkinPriorities.ManualGroups = previousAncientManualGroups;
                Config.ActiveMonsterSkinPresets = previousActiveMonsterPresets;

                TryRestoreOverlay(affectedGroups, cardOverlay: false);
                LastError = exception.Message;
                ModLog.Error($"切换 {groupId} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool ApplyCardSelection(string groupId, string optionId)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                LastError = "皮肤目录尚未初始化。";
                return false;
            }

            var group = Catalog.CardGroups.FirstOrDefault(group =>
                group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group == null ||
                (!optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                 group.Options.All(option =>
                     !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
            {
                LastError = $"未知的卡牌皮肤选择：{groupId}/{optionId}";
                return false;
            }

            var entries = GetCardPriorityEntriesInternal(group)
                .Select(entry => entry with
                {
                    Enabled = !optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                              entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)
                })
                .OrderByDescending(entry => entry.Enabled)
                .ToArray();
            return ApplyCardPriority(groupId, entries);
        }
    }

    // 以下读取不持锁：所有写操作都发生在 Godot 主线程，与 UI 读取同线程；
    // LastError 用 volatile 保证异常路径下的可见性。
    public static string GetCardSelection(string groupId) =>
        Config.GetSelection(CardSelectionKey(groupId));

    public static IReadOnlyList<CardPriorityOptionState> GetCardPriorityOptions(string groupId)
    {
        lock (Sync)
        {
            var group = Catalog?.CardGroups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                return [];
            }

            var entries = GetCardPriorityEntriesInternal(group);
            var coverage = GetCardCoverage(group);
            return entries.Select((entry, colorIndex) =>
                {
                    var option = group.Options.First(candidate =>
                        candidate.Id.Equals(entry.OptionId, StringComparison.OrdinalIgnoreCase));
                    return new CardPriorityOptionState(
                        option.Id,
                        option.Name,
                        entry.Enabled,
                        colorIndex,
                        coverage.ByOption.GetValueOrDefault(entry.OptionId),
                        coverage.TotalCards);
                })
                .ToArray();
        }
    }

    public static IReadOnlyList<CardSkinSourceState> GetCardSkinSources(CardModel card, string? displayGroupId = null)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            if (lookup.Group == null)
            {
                return [];
            }

            var current = GetEffectiveCardSelection(card, lookup);
            var displayGroup = lookup.PriorityGroups.FirstOrDefault(group => group.Id.Equals(
                displayGroupId, StringComparison.OrdinalIgnoreCase)) ?? lookup.Group;
            return GetCardPriorityEntriesInternal(displayGroup)
                .Select((entry, colorIndex) => (Entry: entry, ColorIndex: colorIndex))
                .Where(pair =>
                    lookup.OptionsById.ContainsKey(pair.Entry.OptionId))
                .Select(pair =>
                {
                    var option = lookup.OptionsById[pair.Entry.OptionId].Option;
                    return new CardSkinSourceState(
                        option.Id,
                        option.Name,
                        pair.Entry.Enabled,
                        pair.ColorIndex,
                        option.Id.Equals(current, StringComparison.OrdinalIgnoreCase));
                })
                .ToArray();
        }
    }

    public static int GetCardSkinSourceCount(CardModel card) =>
        GetCardSkinSources(card).Count;

    public static IReadOnlyList<CardSkinPresetState> GetCardSkinPresets(string groupId)
    {
        lock (Sync)
        {
            return Config.CardSkinPresets
                .Where(preset => preset.CategoryId?.Equals(
                    groupId,
                    StringComparison.OrdinalIgnoreCase) == true)
                .OrderByDescending(preset => BundlePresetPolicy.IsOwned(preset.Name))
                .Select(preset => new CardSkinPresetState(
                    preset.Name,
                    preset.Name.Equals(
                        GetActiveCardSkinPreset(groupId),
                        StringComparison.OrdinalIgnoreCase),
                    BundlePresetPolicy.IsOwned(preset.Name) ? GetPresetDisplayName(preset.Name) : null))
                .ToArray();
        }
    }

    public static bool CreateCardSkinPreset(string groupId, string name)
    {
        lock (Sync)
        {
            if (!IsKnownCardGroup(groupId))
            {
                LastError = $"未知的卡牌皮肤分类：{groupId}";
                return false;
            }

            var normalizedName = NormalizeCardSkinPresetName(name);
            if (normalizedName == null)
            {
                return false;
            }

            if (FindCardSkinPresetIndex(groupId, normalizedName) >= 0)
            {
                LastError = $"卡图预设已存在：{normalizedName}";
                return false;
            }

            var previousActivePreset = GetActiveCardSkinPreset(groupId);
            var preset = CaptureCurrentCardSkinPreset(groupId, normalizedName);
            try
            {
                Config.CardSkinPresets.Add(preset);
                SetActiveCardSkinPreset(groupId, preset.Name);
                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                Config.CardSkinPresets.Remove(preset);
                SetActiveCardSkinPreset(groupId, previousActivePreset);
                LastError = exception.Message;
                ModLog.Error($"保存卡图预设 {normalizedName} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool OverwriteCardSkinPreset(string groupId, string name)
    {
        lock (Sync)
        {
            var index = FindCardSkinPresetIndex(groupId, name);
            if (index < 0)
            {
                LastError = $"找不到卡图预设：{name}";
                return false;
            }

            var previous = CloneCardSkinPreset(Config.CardSkinPresets[index]);
            var previousActivePreset = GetActiveCardSkinPreset(groupId);
            var replacement = CaptureCurrentCardSkinPreset(groupId, previous.Name);
            try
            {
                Config.CardSkinPresets[index] = replacement;
                SetActiveCardSkinPreset(groupId, replacement.Name);
                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                Config.CardSkinPresets[index] = previous;
                SetActiveCardSkinPreset(groupId, previousActivePreset);
                LastError = exception.Message;
                ModLog.Error($"覆盖卡图预设 {previous.Name} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool RenameCardSkinPreset(string groupId, string currentName, string newName)
    {
        lock (Sync)
        {
            if (!CanRenameOrDeletePreset(currentName)) return false;
            var index = FindCardSkinPresetIndex(groupId, currentName);
            var normalizedName = NormalizeCardSkinPresetName(newName);
            if (index < 0 || normalizedName == null)
            {
                if (index < 0)
                {
                    LastError = $"找不到卡图预设：{currentName}";
                }

                return false;
            }

            var duplicateIndex = FindCardSkinPresetIndex(groupId, normalizedName);
            if (duplicateIndex >= 0 && duplicateIndex != index)
            {
                LastError = $"卡图预设已存在：{normalizedName}";
                return false;
            }

            var previousName = Config.CardSkinPresets[index].Name;
            var previousActivePreset = GetActiveCardSkinPreset(groupId);
            var previousBundles = UpdateBundlePresetReferences(false, groupId, previousName, normalizedName);
            try
            {
                Config.CardSkinPresets[index].Name = normalizedName;
                if (previousName.Equals(previousActivePreset, StringComparison.OrdinalIgnoreCase))
                {
                    SetActiveCardSkinPreset(groupId, normalizedName);
                }

                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                Config.CardSkinPresets[index].Name = previousName;
                Config.CharacterSkinBundles = previousBundles;
                SetActiveCardSkinPreset(groupId, previousActivePreset);
                LastError = exception.Message;
                ModLog.Error($"重命名卡图预设 {previousName} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool DeleteCardSkinPreset(string groupId, string name)
    {
        lock (Sync)
        {
            if (!CanRenameOrDeletePreset(name)) return false;
            var index = FindCardSkinPresetIndex(groupId, name);
            if (index < 0)
            {
                LastError = $"找不到卡图预设：{name}";
                return false;
            }

            var preset = Config.CardSkinPresets[index];
            var previousActivePreset = GetActiveCardSkinPreset(groupId);
            var previousBundles = UpdateBundlePresetReferences(false, groupId, preset.Name, null);
            try
            {
                Config.CardSkinPresets.RemoveAt(index);
                if (preset.Name.Equals(
                        previousActivePreset,
                        StringComparison.OrdinalIgnoreCase))
                {
                    SetActiveCardSkinPreset(groupId, null);
                }

                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                Config.CardSkinPresets.Insert(index, preset);
                Config.CharacterSkinBundles = previousBundles;
                SetActiveCardSkinPreset(groupId, previousActivePreset);
                LastError = exception.Message;
                ModLog.Error($"删除卡图预设 {preset.Name} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool ApplyCardSkinPreset(string groupId, string name)
    {
        lock (Sync)
        {
            var index = FindCardSkinPresetIndex(groupId, name);
            var catalog = Catalog;
            var group = catalog?.CardGroups.FirstOrDefault(candidate => candidate.Id.Equals(
                groupId,
                StringComparison.OrdinalIgnoreCase));
            if (index < 0 || catalog == null || group == null)
            {
                LastError = index < 0
                    ? $"找不到卡图预设：{name}"
                    : catalog == null ? "皮肤目录尚未初始化。" : $"未知的卡牌皮肤分类：{groupId}";
                return false;
            }

            var preset = Config.CardSkinPresets[index];
            var hadPreviousPriority = Config.CardSkinPriorities.TryGetValue(groupId, out var previousPriority);
            var previousSelections = CaptureCardSelectionsForGroup(groupId);
            var previousActivePreset = GetActiveCardSkinPreset(groupId);
            var previousDefaultsVersion = Config.CardPriorityDefaultsVersion;
            try
            {
                ApplyCardPresetSettings(group, preset);
                CardPreviewSelections.Clear();
                ClearCardPortraitCache(groupId);

                MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId });
                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                if (hadPreviousPriority)
                {
                    Config.CardSkinPriorities[groupId] = previousPriority!;
                }
                else
                {
                    Config.CardSkinPriorities.Remove(groupId);
                }

                ReplaceCardSelectionsForGroup(groupId, previousSelections);
                SetActiveCardSkinPreset(groupId, previousActivePreset);
                Config.CardPriorityDefaultsVersion = previousDefaultsVersion;
                CardPreviewSelections.Clear();
                ClearCardPortraitCache(groupId);

                try
                {
                    MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId });
                }
                catch (Exception restoreException)
                {
                    ModLog.Error("恢复卡图预设切换前的资源失败：" + restoreException);
                }

                LastError = exception.Message;
                ModLog.Error($"应用卡图预设 {preset.Name} 失败：{exception}");
                return false;
            }
        }
    }

    public static bool SetCardPriorityEnabled(string groupId, string optionId, bool enabled)
    {
        lock (Sync)
        {
            var group = Catalog?.CardGroups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                LastError = $"未知的卡牌皮肤分类：{groupId}";
                return false;
            }

            var entries = GetCardPriorityEntriesInternal(group)
                .Select(entry => entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)
                    ? entry with { Enabled = enabled }
                    : entry)
                .ToArray();
            return ApplyCardPriority(groupId, entries);
        }
    }

    public static bool MoveCardPriority(string groupId, string optionId, int offset)
    {
        lock (Sync)
        {
            var group = Catalog?.CardGroups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                LastError = $"未知的卡牌皮肤分类：{groupId}";
                return false;
            }

            var entries = GetCardPriorityEntriesInternal(group).ToList();
            var index = entries.FindIndex(entry =>
                entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
            var target = Math.Clamp(index + offset, 0, entries.Count - 1);
            if (index < 0 || target == index)
            {
                return index >= 0;
            }

            var moved = entries[index];
            entries.RemoveAt(index);
            entries.Insert(target, moved);
            return ApplyCardPriority(groupId, entries);
        }
    }

    private static bool ApplyCardPriority(
        string groupId,
        IReadOnlyList<CardSkinPriorityEntry> requestedEntries)
    {
        var group = Catalog?.CardGroups.FirstOrDefault(candidate =>
            candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
        if (group == null)
        {
            LastError = $"未知的卡牌皮肤分类：{groupId}";
            return false;
        }

        var knownIds = group.Options.Select(option => option.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var knownEntries = requestedEntries
            .Where(entry => knownIds.Contains(entry.OptionId))
            .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var option in group.Options.Where(option => knownEntries.All(entry =>
                     !entry.OptionId.Equals(option.Id, StringComparison.OrdinalIgnoreCase))))
        {
            knownEntries.Add(new CardSkinPriorityEntry(option.Id, Enabled: true));
        }

        var previousEntries = Config.CardSkinPriorities.TryGetValue(group.Id, out var configured)
            ? configured.ToList()
            : null;
        var storedEntries = MergeKnownCardPriorityEntries(
            previousEntries ?? [],
            knownEntries,
            knownIds);
        var selectionKey = CardSelectionKey(group.Id);
        var hadPreviousSelection = Config.Selections.TryGetValue(selectionKey, out var previousSelection);
        var previousActivePreset = GetActiveCardSkinPreset(group.Id);
        try
        {
            Config.CardSkinPriorities[group.Id] = storedEntries;
            Config.Selections[selectionKey] = knownEntries.FirstOrDefault(entry => entry.Enabled)?.OptionId ??
                                             SkinCatalog.BaseOptionId;
            SetActiveCardSkinPreset(group.Id, null);
            ClearCardPortraitCache(group.Id);
            MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { group.Id });
            Config.Save(ConfigPath);
            LastError = null;
            return true;
        }
        catch (Exception exception)
        {
            if (previousEntries == null)
            {
                Config.CardSkinPriorities.Remove(group.Id);
            }
            else
            {
                Config.CardSkinPriorities[group.Id] = previousEntries;
            }

            RestoreSelection(selectionKey, previousSelection, hadPreviousSelection);
            SetActiveCardSkinPreset(group.Id, previousActivePreset);
            ClearCardPortraitCache(group.Id);
            TryRestoreOverlay(group.Id, cardOverlay: true);
            LastError = exception.Message;
            ModLog.Error($"调整 {group.Id} 卡牌皮肤优先级失败：{exception}");
            return false;
        }
    }

    public static bool RegisterMonsterSkinCategory(
        string categoryId,
        IEnumerable<string> groupIds)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null || string.IsNullOrWhiteSpace(categoryId))
            {
                return false;
            }

            var knownGroupIds = groupIds
                .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
                .Where(groupId => catalog.Groups.Any(group =>
                    group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase) &&
                    group.Options.Count > 0))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (knownGroupIds.Count == 0)
            {
                return false;
            }

            if (Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out var current) &&
                current.ToHashSet(StringComparer.OrdinalIgnoreCase)
                    .SetEquals(knownGroupIds))
            {
                return true;
            }

            return ChangeMonsterPriorityConfiguration(
                categoryId,
                () => Config.MonsterSkinCategoryGroups[categoryId] = knownGroupIds,
                adoptUnconfiguredGroups: true);
        }
    }

    public static IReadOnlyList<MonsterPriorityOptionState> GetMonsterPriorityOptions(string categoryId)
    {
        lock (Sync)
        {
            var options = GetMonsterCategoryOptionsInternal(categoryId);
            if (options.Count == 0)
            {
                return [];
            }

            var entries = GetMonsterPriorityEntriesInternal(categoryId);
            return entries.Select((entry, colorIndex) =>
                {
                    var option = options.First(candidate => candidate.OptionId.Equals(
                        entry.OptionId,
                        StringComparison.OrdinalIgnoreCase));
                    return new MonsterPriorityOptionState(
                        option.OptionId,
                        option.Name,
                        entry.Enabled,
                        colorIndex,
                        option.Coverage,
                        option.TotalMonsters);
                })
                .ToArray();
        }
    }

    public static IReadOnlyList<MonsterSkinPresetState> GetMonsterSkinPresets(string categoryId)
    {
        lock (Sync)
        {
            var active = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
            return Config.MonsterSkinPresets
                .Where(preset => preset.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(preset => BundlePresetPolicy.IsOwned(preset.Name))
                .Select(preset => new MonsterSkinPresetState(
                    preset.Name,
                    preset.Name.Equals(active, StringComparison.OrdinalIgnoreCase),
                    BundlePresetPolicy.IsOwned(preset.Name) ? GetPresetDisplayName(preset.Name) : null))
                .ToArray();
        }
    }

    public static bool CreateMonsterSkinPreset(string categoryId, string name)
    {
        lock (Sync)
        {
            var normalized = NormalizeMonsterSkinPresetName(name);
            if (normalized == null)
            {
                return false;
            }
            if (!Config.MonsterSkinCategoryGroups.ContainsKey(categoryId))
            {
                LastError = $"未知的怪物图鉴分类：{categoryId}";
                return false;
            }
            if (FindMonsterSkinPresetIndex(categoryId, normalized) >= 0)
            {
                LastError = $"怪物皮肤预设已存在：{normalized}";
                return false;
            }

            var preset = CaptureCurrentMonsterSkinPreset(categoryId, normalized);
            var previousActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
            Config.MonsterSkinPresets.Add(preset);
            Config.ActiveMonsterSkinPresets[categoryId] = preset.Name;
            return SaveMonsterPresetConfiguration("保存", preset, () =>
            {
                Config.MonsterSkinPresets.Remove(preset);
                RestoreActiveMonsterPreset(categoryId, previousActive);
            });
        }
    }

    public static bool OverwriteMonsterSkinPreset(string categoryId, string name)
    {
        lock (Sync)
        {
            var index = FindMonsterSkinPresetIndex(categoryId, name);
            if (index < 0)
            {
                LastError = $"找不到怪物皮肤预设：{name}";
                return false;
            }
            var previous = CloneMonsterSkinPreset(Config.MonsterSkinPresets[index]);
            var previousActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
            Config.MonsterSkinPresets[index] = CaptureCurrentMonsterSkinPreset(categoryId, previous.Name);
            Config.ActiveMonsterSkinPresets[categoryId] = previous.Name;
            return SaveMonsterPresetConfiguration("覆盖", previous, () =>
            {
                Config.MonsterSkinPresets[index] = previous;
                RestoreActiveMonsterPreset(categoryId, previousActive);
            });
        }
    }

    public static bool RenameMonsterSkinPreset(string categoryId, string currentName, string newName)
    {
        lock (Sync)
        {
            if (!CanRenameOrDeletePreset(currentName)) return false;
            var index = FindMonsterSkinPresetIndex(categoryId, currentName);
            var normalized = NormalizeMonsterSkinPresetName(newName);
            if (normalized == null)
            {
                return false;
            }
            if (index < 0)
            {
                LastError = $"找不到怪物皮肤预设：{currentName}";
                return false;
            }
            var duplicate = FindMonsterSkinPresetIndex(categoryId, normalized);
            if (duplicate >= 0 && duplicate != index)
            {
                LastError = $"怪物皮肤预设已存在：{normalized}";
                return false;
            }

            var preset = Config.MonsterSkinPresets[index];
            var previousName = preset.Name;
            var previousBundles = UpdateBundlePresetReferences(true, categoryId, previousName, normalized);
            var wasActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId)
                ?.Equals(previousName, StringComparison.OrdinalIgnoreCase) == true;
            preset.Name = normalized;
            if (wasActive)
            {
                Config.ActiveMonsterSkinPresets[categoryId] = normalized;
            }
            return SaveMonsterPresetConfiguration("重命名", preset, () =>
            {
                preset.Name = previousName;
                Config.CharacterSkinBundles = previousBundles;
                if (wasActive)
                {
                    Config.ActiveMonsterSkinPresets[categoryId] = previousName;
                }
            });
        }
    }

    public static bool DeleteMonsterSkinPreset(string categoryId, string name)
    {
        lock (Sync)
        {
            if (!CanRenameOrDeletePreset(name)) return false;
            var index = FindMonsterSkinPresetIndex(categoryId, name);
            if (index < 0)
            {
                LastError = $"找不到怪物皮肤预设：{name}";
                return false;
            }
            var preset = Config.MonsterSkinPresets[index];
            var previousActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
            var previousBundles = UpdateBundlePresetReferences(true, categoryId, preset.Name, null);
            Config.MonsterSkinPresets.RemoveAt(index);
            if (preset.Name.Equals(previousActive, StringComparison.OrdinalIgnoreCase))
            {
                Config.ActiveMonsterSkinPresets.Remove(categoryId);
            }
            return SaveMonsterPresetConfiguration("删除", preset, () =>
            {
                Config.MonsterSkinPresets.Insert(index, preset);
                Config.CharacterSkinBundles = previousBundles;
                if (previousActive != null)
                {
                    Config.ActiveMonsterSkinPresets[categoryId] = previousActive;
                }
            });
        }
    }

    public static bool ApplyMonsterSkinPreset(string categoryId, string name)
    {
        lock (Sync)
        {
            var index = FindMonsterSkinPresetIndex(categoryId, name);
            if (index < 0)
            {
                LastError = $"找不到怪物皮肤预设：{name}";
                return false;
            }
            var preset = Config.MonsterSkinPresets[index];
            return ChangeMonsterPriorityConfiguration(categoryId,
                () => ApplyMonsterPresetSettings(preset), adoptUnconfiguredGroups: false);
        }
    }

    private static void ApplyMonsterPresetSettings(MonsterSkinPreset preset)
    {
        var categoryId = preset.CategoryId;
        var currentGroupIds = Config.MonsterSkinCategoryGroups[categoryId]
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var safeSelections = BundlePresetPolicy.MonsterSelections(preset, currentGroupIds)
            .Where(pair => currentGroupIds.Contains(pair.Key))
            .ToDictionary(
                pair => pair.Key,
                pair =>
                {
                    var group = Catalog?.Groups.FirstOrDefault(candidate =>
                        candidate.Id.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));
                    return pair.Value.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) ||
                           group?.Options.Any(option => option.Id.Equals(
                               pair.Value, StringComparison.OrdinalIgnoreCase)) == true
                        ? pair.Value
                        : SkinCatalog.BaseOptionId;
                },
                StringComparer.OrdinalIgnoreCase);
        Config.MonsterSkinPriorities[categoryId] = preset.AllOriginal
            ? GetMonsterPriorityEntriesInternal(categoryId).Select(entry => entry with { Enabled = false }).ToList()
            : preset.Priority.ToList();
        Config.Selections = MonsterSkinPresetPolicy.Apply(
            new MonsterSkinPresetSnapshot(
                preset.CategoryId,
                preset.Priority.Select(entry => new MonsterSkinPresetPriorityState(
                    entry.OptionId, entry.Enabled)).ToArray(),
                safeSelections),
            Config.Selections);
        foreach (var groupId in Config.MonsterSkinCategoryGroups[categoryId])
        {
            var follows = !preset.AllOriginal && preset.FollowingGroupIds.Contains(
                groupId, StringComparer.OrdinalIgnoreCase);
            Config.MonsterGroupsFollowingCategory.RemoveAll(candidate =>
                candidate.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            Config.MonsterGroupsWithManualSelection.RemoveAll(candidate =>
                candidate.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            (follows
                ? Config.MonsterGroupsFollowingCategory
                : Config.MonsterGroupsWithManualSelection).Add(groupId);
        }
        Config.ActiveMonsterSkinPresets[categoryId] = preset.Name;
    }

    public static IReadOnlySet<string> GetMonsterRunEnvironmentProviders(
        string categoryId,
        IEnumerable<string> groupIds,
        IReadOnlySet<string>? activeCombatGroupIds = null)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null)
            {
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            var groups = groupIds
                .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(groupId => catalog.Groups.FirstOrDefault(group =>
                    group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)))
                .Where(group => group != null)
                .Cast<SkinGroup>()
                .ToArray();
            var options = groups
                .SelectMany(group => group.Options)
                .DistinctBy(option => option.Id, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var knownOptionIds = options
                .Select(option => option.Id)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var priorities = Config.MonsterSkinPriorities.TryGetValue(categoryId, out var configured)
                ? configured
                    .Where(entry => knownOptionIds.Contains(entry.OptionId))
                    .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
                    .ToList()
                : [];
            foreach (var option in options.Where(option => priorities.All(entry =>
                         !entry.OptionId.Equals(option.Id, StringComparison.OrdinalIgnoreCase))))
            {
                priorities.Add(new MonsterSkinPriorityEntry(option.Id, Enabled: true));
            }

            var activeProviderIds = activeCombatGroupIds?
                .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
                .Select(groupId => catalog.ResolveVisualProviderId(GetVisualSelection(groupId)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var manuallySelectedActiveProviderIds = activeCombatGroupIds?
                .Where(groupId => Config.MonsterGroupsWithManualSelection.Contains(
                    groupId,
                    StringComparer.OrdinalIgnoreCase))
                .Select(groupId => catalog.ResolveVisualProviderId(GetVisualSelection(groupId)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase) ??
                new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var candidates = priorities.Select(entry =>
            {
                var providerId = catalog.ResolveVisualProviderId(entry.OptionId);
                return new RuntimeProviderPriorityCandidate(
                    providerId,
                    entry.Enabled || manuallySelectedActiveProviderIds.Contains(providerId),
                    catalog.ProviderUsesScopedMonsterRuntime(providerId),
                    activeProviderIds == null || activeProviderIds.Contains(providerId));
            });
            return RuntimeProviderScopePolicy.SelectRunEnvironmentProviders(candidates);
        }
    }

    public static bool SetMonsterPriorityOptionEnabled(
        string categoryId,
        string optionId,
        bool enabled)
    {
        lock (Sync)
        {
            var entries = GetMonsterPriorityEntriesInternal(categoryId)
                .Select(entry => entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)
                    ? entry with { Enabled = enabled }
                    : entry)
                .ToList();
            if (entries.All(entry => !entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)))
            {
                LastError = $"未知的怪物皮肤选项：{categoryId}/{optionId}";
                return false;
            }

            return ChangeMonsterPriorityConfiguration(
                categoryId,
                () => Config.MonsterSkinPriorities[categoryId] = entries,
                adoptUnconfiguredGroups: false);
        }
    }

    public static bool MoveMonsterPriority(string categoryId, string optionId, int offset)
    {
        lock (Sync)
        {
            var entries = GetMonsterPriorityEntriesInternal(categoryId).ToList();
            var index = entries.FindIndex(entry =>
                entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
            var target = Math.Clamp(index + offset, 0, entries.Count - 1);
            if (index < 0 || target == index)
            {
                return index >= 0;
            }

            var moved = entries[index];
            entries.RemoveAt(index);
            entries.Insert(target, moved);
            return ChangeMonsterPriorityConfiguration(
                categoryId,
                () => Config.MonsterSkinPriorities[categoryId] = entries,
                adoptUnconfiguredGroups: false);
        }
    }

    public static bool HasMonsterSkinCategory(string groupId) =>
        Config.MonsterSkinCategoryGroups.Values.Any(groupIds =>
            groupIds.Contains(groupId, StringComparer.OrdinalIgnoreCase));

    public static string GetMonsterOverrideSelection(string groupId) =>
        Config.MonsterGroupsFollowingCategory.Contains(groupId, StringComparer.OrdinalIgnoreCase)
            ? InheritMonsterSelectionId
            : Config.GetSelection(groupId);

    public static bool FollowMonsterCategoryPriority(string groupId)
    {
        lock (Sync)
        {
            var categoryId = Config.MonsterSkinCategoryGroups
                .FirstOrDefault(pair => pair.Value.Contains(groupId, StringComparer.OrdinalIgnoreCase))
                .Key;
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                LastError = $"怪物 {groupId} 不属于已登记的图鉴分类。";
                return false;
            }

            return ChangeMonsterPriorityConfiguration(
                categoryId,
                () =>
                {
                    Config.MonsterGroupsWithManualSelection.RemoveAll(candidate =>
                        candidate.Equals(groupId, StringComparison.OrdinalIgnoreCase));
                    if (!Config.MonsterGroupsFollowingCategory.Contains(
                            groupId,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        Config.MonsterGroupsFollowingCategory.Add(groupId);
                    }
                },
                adoptUnconfiguredGroups: false);
        }
    }

    private static bool ChangeMonsterPriorityConfiguration(
        string categoryId,
        Action mutation,
        bool adoptUnconfiguredGroups)
    {
        var previousSelections = new Dictionary<string, string>(
            Config.Selections,
            StringComparer.OrdinalIgnoreCase);
        var previousPriorities = Config.MonsterSkinPriorities.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);
        var previousCategories = Config.MonsterSkinCategoryGroups.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);
        var previousEnabledCategories = Config.EnabledMonsterSkinPriorityCategories.ToList();
        var previousFollowingGroups = Config.MonsterGroupsFollowingCategory.ToList();
        var previousManualGroups = Config.MonsterGroupsWithManualSelection.ToList();
        var previousActivePresets = new Dictionary<string, string>(
            Config.ActiveMonsterSkinPresets,
            StringComparer.OrdinalIgnoreCase);
        var previousVisualProviderPriority = Config.VisualProviderPriority.ToList();
        var affectedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            Config.ActiveMonsterSkinPresets.Remove(categoryId);
            mutation();
            if (!Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out var categoryGroups))
            {
                throw new InvalidOperationException($"未知的怪物图鉴分类：{categoryId}");
            }

            if (adoptUnconfiguredGroups)
            {
                foreach (var groupId in categoryGroups.Where(groupId =>
                             !Config.MonsterGroupsWithManualSelection.Contains(
                                 groupId,
                                 StringComparer.OrdinalIgnoreCase)))
                {
                    if (!Config.MonsterGroupsFollowingCategory.Contains(
                            groupId,
                            StringComparer.OrdinalIgnoreCase))
                    {
                        Config.MonsterGroupsFollowingCategory.Add(groupId);
                    }
                }
            }

            _ = GetMonsterPriorityEntriesInternal(categoryId);
            affectedGroups = ApplyMonsterCategoryPriorityToSelections(categoryId);
            affectedGroups.UnionWith(Config.Selections.Keys
                .Union(previousSelections.Keys, StringComparer.OrdinalIgnoreCase)
                .Where(groupId => !string.Equals(
                    Config.Selections.GetValueOrDefault(groupId),
                    previousSelections.GetValueOrDefault(groupId),
                    StringComparison.OrdinalIgnoreCase)));
            foreach (var groupId in affectedGroups)
            {
                ClearRuntimeResourceCache(groupId);
                UpdateVisualProviderPriority(groupId, Config.GetSelection(groupId));
            }

            if (affectedGroups.Count > 0)
            {
                MountOverlay(affectedGroups);
            }

            Config.Save(ConfigPath);
            LastError = null;
            return true;
        }
        catch (Exception exception)
        {
            var restoreGroups = Config.Selections.Keys
                .Union(previousSelections.Keys, StringComparer.OrdinalIgnoreCase)
                .Where(groupId => !string.Equals(
                    Config.Selections.GetValueOrDefault(groupId),
                    previousSelections.GetValueOrDefault(groupId),
                    StringComparison.OrdinalIgnoreCase))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            restoreGroups.UnionWith(affectedGroups);
            Config.Selections = previousSelections;
            Config.MonsterSkinPriorities = previousPriorities;
            Config.MonsterSkinCategoryGroups = previousCategories;
            Config.EnabledMonsterSkinPriorityCategories = previousEnabledCategories;
            Config.MonsterGroupsFollowingCategory = previousFollowingGroups;
            Config.MonsterGroupsWithManualSelection = previousManualGroups;
            Config.ActiveMonsterSkinPresets = previousActivePresets;
            Config.VisualProviderPriority = previousVisualProviderPriority;
            foreach (var groupId in restoreGroups)
            {
                ClearRuntimeResourceCache(groupId);
            }

            if (restoreGroups.Count > 0)
            {
                TryRestoreOverlay(restoreGroups, cardOverlay: false);
            }

            LastError = exception.Message;
            ModLog.Error($"调整 {categoryId} 怪物皮肤优先级失败：{exception}");
            return false;
        }
    }

    public static bool ShouldDriveManagedCharacterAnimations(string groupId)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null)
            {
                return false;
            }

            var selectedId = GetVisualSelection(groupId);
            return catalog.IsRuntimeProviderOption(groupId, selectedId) &&
                   catalog.ProviderUsesManagedCharacterScene(groupId, selectedId);
        }
    }

    public static IReadOnlyList<CardSkinOption> GetCardOptions(CardModel card)
    {
        lock (Sync)
        {
            return GetCardLookup(card).Options;
        }
    }

    /// <summary>
    /// The saved priority rows of the card group that supplies a card's skins, plus the same
    /// options in that saved order. Callers use them to render the list the priority screen shows,
    /// including which skins are ticked.
    /// </summary>
    public static (IReadOnlyList<CardSkinOption> Options, IReadOnlyList<SkinPriorityState> States) GetCardOptionsForDisplay(
        CardModel card)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            // Deliberately the single group that already supplies the options, never the Ancient
            // fallback groups: their order belongs to a different priority list.
            if (lookup.Group == null)
            {
                return (lookup.Options, []);
            }

            var states = GetCardPriorityStatesInternal(lookup.Group);
            return (SkinPriorityDisplayPolicy.Project(lookup.Options, option => option.Id, states), states);
        }
    }

    private static IReadOnlyList<SkinPriorityState> GetCardPriorityStatesInternal(CardSkinGroup group) =>
        SkinPriorityDisplayPolicy.States(
            GetCardPriorityEntriesInternal(group),
            entry => entry.OptionId,
            entry => entry.Enabled);

    /// <summary>
    /// The saved priority rows that govern a character, monster or event appearance group, or null
    /// when the group has no skin-priority screen at all. Characters, companions and merchants
    /// are chosen directly and were never part of a manual order; only bestiary regions,
    /// event regions and the ancient category have one, so the other lists must keep the
    /// catalogue order they are given.
    /// </summary>
    public static IReadOnlyList<SkinPriorityState>? GetVisualPriorityStates(string groupId)
    {
        lock (Sync)
        {
            var categoryId = Config.MonsterSkinCategoryGroups
                .FirstOrDefault(pair => pair.Value.Contains(
                    groupId,
                    StringComparer.OrdinalIgnoreCase))
                .Key;
            if (!string.IsNullOrWhiteSpace(categoryId))
            {
                return SkinPriorityDisplayPolicy.States(
                    GetMonsterPriorityEntriesInternal(categoryId),
                    entry => entry.OptionId,
                    entry => entry.Enabled);
            }

            var region = Config.EventSkinPriorities.RegionGroups
                .FirstOrDefault(pair => pair.Value.Contains(
                    groupId,
                    StringComparer.OrdinalIgnoreCase))
                .Key;
            if (!string.IsNullOrWhiteSpace(region))
            {
                return SkinPriorityDisplayPolicy.States(
                    GetEventPriorityEntries(region),
                    entry => entry.OptionId,
                    entry => entry.Enabled);
            }

            var ancient = Config.AncientSkinPriorities.RegionGroups
                .FirstOrDefault(pair => pair.Value.Contains(
                    groupId,
                    StringComparer.OrdinalIgnoreCase))
                .Key;
            return string.IsNullOrWhiteSpace(ancient)
                ? null
                : SkinPriorityDisplayPolicy.States(
                    GetAncientPriorityEntries(),
                    entry => entry.OptionId,
                    entry => entry.Enabled);
        }
    }

    /// <summary>
    /// A player-facing list of one group's skins, ready to draw: the same members in the order the
    /// group's skin-priority screen saved, together with the priority state each row carries, so a
    /// list cannot order itself by one snapshot and dim itself by another. A group without a
    /// priority screen keeps the order it was given and reports no states.
    /// </summary>
    public static (IReadOnlyList<SkinOption> Options, IReadOnlyList<SkinPriorityState>? States) GetSkinListForDisplay(
        string groupId,
        IReadOnlyList<SkinOption> options)
    {
        var states = GetVisualPriorityStates(groupId);
        return (states == null
            ? options
            : SkinPriorityDisplayPolicy.Project(options, option => option.Id, states), states);
    }

    public static string? GetCardOptionName(string optionId)
    {
        lock (Sync)
        {
            return Catalog?.CardGroups
                .SelectMany(group => group.Options)
                .FirstOrDefault(option => option.Id.Equals(
                    optionId,
                    StringComparison.OrdinalIgnoreCase))
                ?.Name;
        }
    }

    public static bool HasCardSkin(CardModel card)
    {
        lock (Sync)
        {
            return GetCardLookup(card).Options.Count > 0;
        }
    }

    public static string GetCardOverrideSelection(CardModel card)
    {
        lock (Sync)
        {
            return Config.Selections.GetValueOrDefault(
                IndividualCardSelectionKey(card),
                InheritCardSelectionId);
        }
    }

    public static string GetEffectiveCardSelection(CardModel card)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            return GetEffectiveCardSelection(card, lookup);
        }
    }

    public static void WithCardPreviewSelection(
        CardModel card,
        string selection,
        Action previewAction)
    {
        var key = IndividualCardSelectionKey(card);
        string? previous = null;
        bool hadPrevious;
        lock (Sync)
        {
            hadPrevious = CardPreviewSelections.TryGetValue(key, out previous);
            CardPreviewSelections[key] = selection;
        }

        try
        {
            previewAction();
        }
        finally
        {
            lock (Sync)
            {
                if (hadPrevious)
                {
                    CardPreviewSelections[key] = previous!;
                }
                else
                {
                    CardPreviewSelections.Remove(key);
                }
            }
        }
    }

    private static string GetEffectiveCardSelection(
        CardModel card,
        CardLookup lookup)
        => ResolveCardSelection(card, lookup).OptionId;

    public static string? GetCardInheritedGroupId(CardModel card)
    {
        lock (Sync)
        {
            return ResolveCardSelection(card, GetCardLookup(card)).GroupId;
        }
    }

    private static (string OptionId, string? GroupId) ResolveCardSelection(
        CardModel card,
        CardLookup lookup)
    {
        var cardSelectionKey = IndividualCardSelectionKey(card);
        var individual = CardPreviewSelections.TryGetValue(cardSelectionKey, out var previewSelection)
            ? previewSelection
            : Config.Selections.GetValueOrDefault(cardSelectionKey, InheritCardSelectionId);
        if (individual.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
        {
            return (individual, null);
        }

        if (lookup.OptionsById.ContainsKey(individual))
        {
            return (individual, null);
        }

        HashSet<string>? excluded = null;
        for (var index = 0; index < lookup.PriorityGroups.Count; index++)
        {
            var group = lookup.PriorityGroups[index];
            var entries = GetCardPriorityEntriesInternal(group);
            foreach (var entry in entries)
            {
                if (entry.Enabled && excluded?.Contains(entry.OptionId) != true &&
                    lookup.OptionsById.TryGetValue(entry.OptionId, out var option))
                {
                    return (option.Option.Id, group.Id);
                }
            }
            // All original is an explicit stop, not a missing-coverage fallback.
            if (entries.Count > 0 && entries.All(entry => !entry.Enabled))
                return (SkinCatalog.BaseOptionId, group.Id);
            if (index + 1 < lookup.PriorityGroups.Count)
            {
                excluded ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                excluded.UnionWith(entries.Where(entry => !entry.Enabled).Select(entry => entry.OptionId));
            }
        }
        return (SkinCatalog.BaseOptionId, null);
    }

    internal static CardSkinOption? GetSelectedCardOption(CardModel card)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            return lookup.OptionsById.GetValueOrDefault(GetEffectiveCardSelection(card, lookup))?.Option;
        }
    }

    internal static CardSurfaceDefinition? GetCardSurface(CardModel card)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            return lookup.OptionsById.GetValueOrDefault(GetEffectiveCardSelection(card, lookup))?
                .Option.CardSurfaces.GetValueOrDefault(lookup.CardType);
        }
    }

    public static CardPresentationDefinition? GetCardPresentation(CardModel card)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            var selection = GetEffectiveCardSelection(card, lookup);
            if (!lookup.OptionsById.TryGetValue(selection, out var option)) return null;
            if (option.Option.StatefulArt != null)
                return StatefulCardArtRuntime.For(option.Option)?.Presentation(card);
            return option.Option.CardPresentations.GetValueOrDefault(lookup.CardType);
        }
    }

    public static T? LoadCardPresentationResource<T>(CardModel card, string? resourcePath)
        where T : Resource
    {
        if (string.IsNullOrWhiteSpace(resourcePath))
        {
            return null;
        }

        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            var selection = GetEffectiveCardSelection(card, lookup);
            lookup.OptionsById.TryGetValue(selection, out var optionLookup);
            var option = optionLookup?.Option;
            var cacheKey =
                $"card-presentation:{lookup.GroupId}:{typeof(T).FullName}:{selection}:{resourcePath}";
            if (RuntimeResourceCache.TryGetValue(cacheKey, out var cached) &&
                cached is T typedCached &&
                GodotObject.IsInstanceValid(typedCached))
            {
                return typedCached;
            }

            T? resource = null;
            if (lookup.Group != null)
            {
                foreach (var useSelectedProvider in new[] { true, false })
                {
                    if (useSelectedProvider && option?.ProviderRootPath == null)
                    {
                        continue;
                    }

                    try
                    {
                        var providerPaths = useSelectedProvider && option != null
                            ? option.CardPresentations.Values
                                .SelectMany(presentation => presentation.ResourcePaths)
                                .Append(resourcePath)
                            : [resourcePath];
                        var overlay = EnsureIsolatedCardOverlay(
                            lookup.Group.Id,
                            selection,
                            useSelectedProvider,
                            providerPaths);
                        if (overlay.ResourcePaths.TryGetValue(resourcePath, out var isolatedPath))
                        {
                            resource = ResourceLoader.Load<T>(
                                isolatedPath,
                                null,
                                overlay.CanReuseExternalDependencies
                                    ? ResourceLoader.CacheMode.Ignore
                                    : ResourceLoader.CacheMode.IgnoreDeep);
                        }
                    }
                    catch
                    {
                        // Provider configs may intentionally point at a base-game resource. Try the
                        // isolated baseline next so another globally loaded skin cannot supply it.
                    }

                    if (resource != null)
                    {
                        break;
                    }
                }
            }

            resource ??= ResourceLoader.Load<T>(
                resourcePath,
                null,
                ResourceLoader.CacheMode.Reuse);
            if (resource != null)
            {
                RuntimeResourceCache[cacheKey] = resource;
            }
            return resource;
        }
    }

    public static bool ApplyCardSelection(CardModel card, string optionId)
    {
        lock (Sync)
        {
            var lookup = GetCardLookup(card);
            var group = lookup.Group;
            if (group == null)
            {
                LastError = $"没有找到卡牌 {card.Id} 的皮肤分类。";
                return false;
            }

            if (!optionId.Equals(InheritCardSelectionId, StringComparison.OrdinalIgnoreCase) &&
                !optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                !lookup.OptionsById.ContainsKey(optionId))
            {
                LastError = $"未知的单卡皮肤选择：{card.Id}/{optionId}";
                return false;
            }

            var key = IndividualCardSelectionKey(card);
            var hadPrevious = Config.Selections.TryGetValue(key, out var previous);
            var previousActivePreset = GetActiveCardSkinPreset(group.Id);
            try
            {
                if (optionId.Equals(InheritCardSelectionId, StringComparison.OrdinalIgnoreCase))
                {
                    Config.Selections.Remove(key);
                }
                else
                {
                    Config.Selections[key] = optionId;
                }

                SetActiveCardSkinPreset(group.Id, null);

                ClearCardPortraitCache(group.Id);
                MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { group.Id });
                Config.Save(ConfigPath);
                LastError = null;
                return true;
            }
            catch (Exception exception)
            {
                RestoreSelection(key, previous, hadPrevious);
                SetActiveCardSkinPreset(group.Id, previousActivePreset);
                ClearCardPortraitCache(group.Id);
                TryRestoreOverlay(group.Id, cardOverlay: true);
                LastError = exception.Message;
                ModLog.Error($"切换单卡 {card.Id} 皮肤失败：{exception}");
                return false;
            }
        }
    }

    /// <summary>
    /// Card portraits that reach the asset cache (the enlarged copy a card becomes while it is being
    /// dragged onto a target) are owned by the CARD group, not by the character group that happens to
    /// contain the same canonical path. Resolving them through the character group returned the
    /// character provider's own card art, which is why picking a card up flipped it to that skin and
    /// dropping it flipped back.
    /// </summary>
    public static bool TryReplaceCardPortraitTextureByPath(string resourcePath, ref Texture2D result)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null || string.IsNullOrWhiteSpace(resourcePath))
            {
                return false;
            }

            var groupId = catalog.FindCardPortraitGroupForPath(resourcePath);
            if (string.IsNullOrEmpty(groupId))
            {
                return false;
            }

            var selection = GetCardSelection(groupId);
            if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                // "Game default" for this card group: keep whatever the game resolved.
                return false;
            }

            var request = new CardPortraitRequest(
                groupId,
                selection,
                resourcePath,
                $"{groupId}\n{selection}\nby-path\n{resourcePath}",
                UseSelectedProvider: false,
                WrapAtlas: false);
            var portrait = GetOrLoadCardPortrait(request);
            if (portrait == null)
            {
                return false;
            }

            result = portrait;
            return true;
        }
    }

    public static void ReplaceCardPortrait(CardModel card, ref Texture2D result)
    {
        lock (Sync)
        {
            var request = ResolveCardPortraitRequest(card);
            if (request == null)
            {
                return;
            }

            var portrait = GetOrLoadCardPortrait(request);
            if (portrait != null)
            {
                result = portrait;
            }
        }
    }

    public static string? GetExternalCardProviderIdentity(
        CardModel card,
        Texture2D texture)
    {
        lock (Sync)
        {
            var request = ResolveCardPortraitRequest(card);
            if (request == null ||
                !CardPortraitCache.TryGetValue(request.CacheKey, out var managedTexture) ||
                !GodotObject.IsInstanceValid(managedTexture) ||
                managedTexture.GetInstanceId() != texture.GetInstanceId())
            {
                return null;
            }

            // Identity comes from the winning card request, not from guessing a filename.
            // Imported textures can be named wither1/2/3, or anything else entirely.
            // Do not rename the source resource: the bridge exports a lightweight view.
            if (!ExternalCardProviderIdentityPaths.TryGetValue(
                    request.CacheKey,
                    out var providerPath))
            {
                providerPath = ExternalCardProviderIdentityPolicy.BuildSyntheticPath(
                    card.Id.ToString(),
                    request.CacheKey);
                ExternalCardProviderIdentityPaths[request.CacheKey] = providerPath;
            }

            return providerPath;
        }
    }

    public static void PreloadCardPortraits(IEnumerable<CardModel> cards)
    {
        lock (Sync)
        {
            var started = Stopwatch.GetTimestamp();
            var requests = cards
                .Select(ResolveCardPortraitRequest)
                .Where(request => request != null)
                .Cast<CardPortraitRequest>()
                .DistinctBy(request => request.CacheKey, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            foreach (var batch in requests.GroupBy(
                         request => request.OverlayKey,
                         StringComparer.OrdinalIgnoreCase))
            {
                var sample = batch.First();
                try
                {
                    EnsureIsolatedCardOverlay(
                        sample.GroupId,
                        sample.Selection,
                        sample.UseSelectedProvider,
                        batch.Select(request => request.ResourcePath));
                }
                catch
                {
                    // Individual portrait loads keep their existing fallback behavior and error
                    // handling; preloading is only a latency optimization.
                }
            }

            foreach (var request in requests)
            {
                try
                {
                    GetOrLoadCardPortrait(request);
                }
                catch
                {
                    // Keep the grid usable when one optional provider resource is malformed.
                    // The normal portrait getter retains the established per-card fallback path.
                }
            }

            // F34 evidence: the compendium grid loads every portrait synchronously under one
            // lock; the total and per-load slow logs locate where the time actually goes.
            var elapsed = Stopwatch.GetElapsedTime(started);
            if (requests.Length > 0 && elapsed >= TimeSpan.FromMilliseconds(100))
            {
                ModLog.Info(
                    $"卡牌总览已同步准备 {requests.Length} 张卡图；" +
                    $"耗时={elapsed.TotalMilliseconds:F1} ms。");
            }
        }
    }

    private static CardPortraitRequest? ResolveCardPortraitRequest(CardModel card)
    {
        var lookup = GetCardLookup(card);
        if (lookup.Group == null)
        {
            return null;
        }

        var selection = GetEffectiveCardSelection(card, lookup);
        if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
        {
            if (lookup.Options.Count == 0) return null;
            if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var originalPath)) return null;
            return new CardPortraitRequest(
                lookup.GroupId,
                selection,
                originalPath,
                $"{lookup.GroupId}\n{selection}\npck\n{originalPath}",
                UseSelectedProvider: false,
                WrapAtlas: false);
        }

        if (!lookup.OptionsById.TryGetValue(selection, out var optionLookup))
        {
            return null;
        }

        var configuredPath = optionLookup.Option.GetPortraitPath(
            lookup.CardType,
            IsAncientStyleEnabled(optionLookup.Option, lookup.CardType, card));
        if (optionLookup.Option.StatefulArt != null && StatefulCardArtRuntime.For(optionLookup.Option) is {} runtime)
        {
            if (!runtime.Enabled && FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var originalPath))
                return new CardPortraitRequest(lookup.GroupId, SkinCatalog.BaseOptionId, originalPath,
                    $"{lookup.GroupId}\n{selection}\nstateful-disabled\n{originalPath}", false, false);
            configuredPath = runtime.Portrait(card, configuredPath) ?? configuredPath;
        }
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return new CardPortraitRequest(
                lookup.GroupId,
                selection,
                configuredPath,
                $"{lookup.GroupId}\n{selection}\nconfig\n{configuredPath}",
                UseSelectedProvider: true,
                WrapAtlas: true);
        }

        if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var baselinePath)) return null;
        var selectedProviderPath = SelectProviderCardPath(
            optionLookup.MatchedAssetPaths,
            card,
            baselinePath);
        return selectedProviderPath == null
            // The winning provider may own only the frame/layout. Missing layers belong to the
            // game baseline, never to the next provider in the priority list. Load an isolated
            // baseline portrait explicitly so a previously mounted lower-priority card atlas
            // cannot leak into this card.
            ? new CardPortraitRequest(
                lookup.GroupId,
                SkinCatalog.BaseOptionId,
                baselinePath,
                $"{lookup.GroupId}\n{selection}\nbase-fallback\n{baselinePath}",
                UseSelectedProvider: false,
                WrapAtlas: false)
            : new CardPortraitRequest(
                lookup.GroupId,
                selection,
                selectedProviderPath,
                $"{lookup.GroupId}\n{selection}\npck\n{selectedProviderPath}",
                UseSelectedProvider: true,
                WrapAtlas: false);
    }

    private static Texture2D? GetOrLoadCardPortrait(CardPortraitRequest request)
    {
        if (CardPortraitCache.TryGetValue(request.CacheKey, out var cached) &&
            GodotObject.IsInstanceValid(cached))
        {
            return cached;
        }

        if (FailedCardPortraitRequests.Contains(request.CacheKey))
        {
            return null;
        }

        try
        {
            var loadStarted = Stopwatch.GetTimestamp();
            var loaded = LoadIsolatedCardPortrait(
                request.GroupId,
                request.Selection,
                request.ResourcePath,
                request.UseSelectedProvider);
            var loadElapsed = Stopwatch.GetElapsedTime(loadStarted);
            if (loadElapsed >= TimeSpan.FromMilliseconds(30))
            {
                // F34 evidence: a slow first load (often an overlay remount) shows up as a hitch
                // when drawing or playing that card. Record which skin path paid for it.
                ModLog.Info(
                    $"单张卡图首次加载较慢：{request.GroupId}/{request.Selection}/" +
                    $"{request.ResourcePath}，耗时={loadElapsed.TotalMilliseconds:F1} ms。");
            }

            if (loaded == null)
            {
                throw new InvalidOperationException("资源包未返回可用卡牌贴图。");
            }

            // ResourceLoader normally returns a CompressedTexture2D for imported card images;
            // those need an AtlasTexture wrapper for the card model's atlas-based path.  The
            // raw-image fallback above creates a real ImageTexture, which is already a complete
            // canvas.  Wrapping it as an AtlasTexture changes the texture-size metadata used by
            // NCard and makes exported portraits appear horizontally compressed, so preserve
            // the original ImageTexture shape.
            var portrait = request.WrapAtlas &&
                           loaded is not AtlasTexture &&
                           loaded is not ImageTexture
                ? new AtlasTexture
                {
                    Atlas = loaded,
                    Region = new Rect2(0, 0, loaded.GetWidth(), loaded.GetHeight())
                }
                : loaded;
            if (CardPortraitCache.Set(request.CacheKey, portrait, out var evicted))
            {
                // Existing NCard nodes keep their own texture reference. Dropping only the cache
                // reference lets Godot release high-resolution portraits after those nodes leave
                // the tree, instead of retaining every card ever viewed for the whole session.
                ExternalCardProviderIdentityPaths.Remove(evicted.Key);
            }
            return portrait;
        }
        catch (Exception exception)
        {
            if (FailedCardPortraitRequests.Add(request.CacheKey))
            {
                ModLog.Warn(
                    $"卡牌皮肤资源不可用，已回退原版卡图：{request.GroupId}/{request.Selection}/" +
                    $"{request.ResourcePath}；原因={exception.GetBaseException().Message}");
            }

            // Returning null leaves the original CardModel.Portrait value untouched. This is
            // especially important for Skill Potion/card-choice overlays: an optional skin
            // failure must not abort NCard construction and leave the action queue waiting for a
            // choice that can no longer be shown.
            return null;
        }
    }

    private static int CardArtSelectionScore(
        string assetPath,
        CardModel card,
        string originalPath)
    {
        var score = HasSameResourceExtension(assetPath, originalPath) ? 20 : 0;
        var assetUsesBeta = assetPath.Contains("/beta/", StringComparison.OrdinalIgnoreCase);
        var originalUsesBeta = originalPath.Contains("/beta/", StringComparison.OrdinalIgnoreCase);
        if (assetUsesBeta == originalUsesBeta)
        {
            score += 100;
        }

        var asset = CardPortraitIdentity(assetPath);
        var original = CardPortraitIdentity(originalPath);
        var typeStem = NormalizeCardToken(card.GetType().Name);
        var expectedStem = original?.Stem ?? typeStem;
        if (asset?.Stem.Equals(expectedStem, StringComparison.OrdinalIgnoreCase) == true ||
            asset?.Stem.Equals(typeStem, StringComparison.OrdinalIgnoreCase) == true)
        {
            score += 30;
        }

        var level = GetCardPresentationLevel(card);
        foreach (var expected in new[] { expectedStem, typeStem }.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (asset == null ||
                !asset.Value.Stem.StartsWith(expected, StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(asset.Value.Stem[expected.Length..], out var variantNumber))
            {
                continue;
            }

            score += variantNumber == level + 1
                ? 40
                : Math.Max(0, 10 - Math.Abs(variantNumber - level - 1));
            break;
        }

        return score;
    }

    private static int GetCardPresentationLevel(CardModel card)
    {
        foreach (var propertyName in new[] { "FakeUpgradeLevel", "CurrentUpgradeLevel" })
        {
            try
            {
                var property = AccessTools.Property(card.GetType(), propertyName);
                if (property?.GetValue(card) is int level)
                {
                    return Math.Max(0, level);
                }
            }
            catch
            {
                // Provider-specific levels are optional; fall back to the base presentation.
            }
        }

        return 0;
    }

    public static bool CardBelongsToGroup(CardModel card, string groupId)
    {
        lock (Sync)
        {
            return GetCardLookup(card).PriorityGroups.Any(group => group.Id.Equals(
                groupId, StringComparison.OrdinalIgnoreCase));
        }
    }

    private static CardLookup GetCardLookup(CardModel card)
    {
        var lookup = _cardLookupCache.GetValue(card, ResolveCardLookup);
        if (lookup.BaselineUnavailable) _cardLookupCache.Remove(card);
        return lookup;
    }

    private static CardLookup ResolveCardLookup(CardModel card)
    {
        var poolGroupId = GetCardPoolGroupId(card);
        // Failure to read an intrinsic Mod-card profile is not evidence of skin ownership.
        // In particular, do not fall back to a vanilla path or suppress the framework for it.
        if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out _))
            return new CardLookup(poolGroupId, card.GetType().Name, null, [],
                new Dictionary<string, CardOptionLookup>(StringComparer.OrdinalIgnoreCase), [])
            { BaselineUnavailable = true };
        var filterGroupId = GetCardFilterGroupId(card);
        string groupId;
        if (!filterGroupId.Equals(poolGroupId, StringComparison.OrdinalIgnoreCase) &&
            ((filterGroupId.Equals("ancients", StringComparison.OrdinalIgnoreCase) &&
              Catalog?.CardGroups.Any(group => group.Id.Equals("ancients", StringComparison.OrdinalIgnoreCase)) == true) ||
             CardGroupAffectsCardUncached(filterGroupId, card)))
        {
            groupId = filterGroupId;
        }
        else
        {
            var cardType = card.GetType().Name;
            var configuredGroup = Catalog?.CardGroups.FirstOrDefault(group =>
                group.Options.Any(option =>
                    option.NormalPortraits.ContainsKey(cardType) ||
                    option.AncientPortraits.ContainsKey(cardType)));
            if (configuredGroup != null)
            {
                groupId = configuredGroup.Id;
            }
            else if (CardGroupAffectsCardUncached(poolGroupId, card))
            {
                groupId = poolGroupId;
            }
            else
            {
                var catalogGroupId = GetCardCatalogGroupId(card);
                groupId = CardGroupAffectsCardUncached(catalogGroupId, card)
                    ? catalogGroupId
                    : poolGroupId;
            }
        }

        var group = Catalog?.CardGroups.FirstOrDefault(candidate =>
            candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
        var options = group?.Options
            .Select(option => BuildCardOptionLookup(option, card))
            .Where(option => option != null)
            .Cast<CardOptionLookup>()
            .ToArray() ?? [];
        var priorityGroups = new List<CardSkinGroup>();
        if (card.Rarity == CardRarity.Ancient && IsCharacterCardPool(card))
        {
            var characterGroup = Catalog?.CardGroups.FirstOrDefault(candidate =>
                candidate.Id.Equals(poolGroupId, StringComparison.OrdinalIgnoreCase));
            if (characterGroup != null) priorityGroups.Add(characterGroup);
        }
        if (group != null && !priorityGroups.Contains(group)) priorityGroups.Add(group);
        return new CardLookup(
            groupId,
            card.GetType().Name,
            group,
            options.Select(option => option.Option).ToArray(),
            options.ToDictionary(
                option => option.Option.Id,
                StringComparer.OrdinalIgnoreCase),
            priorityGroups);
    }

    private static bool CardGroupAffectsCardUncached(string groupId, CardModel card)
    {
        var group = Catalog?.CardGroups.FirstOrDefault(group =>
            group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
        if (group == null)
        {
            return false;
        }

        return group.Options.Any(option => CardOptionAffectsCardUncached(option, card));
    }

    private static CardSkinGroup? GetCardGroup(CardModel card)
        => GetCardLookup(card).Group;

    private static CardOptionLookup? BuildCardOptionLookup(
        CardSkinOption option,
        CardModel card)
    {
        var cardType = card.GetType().Name;
        var name = option.GetNameForCard(cardType);
        if (!name.Equals(option.Name, StringComparison.Ordinal))
        {
            option = option with { Name = name };
        }
        if (option.NormalPortraits.ContainsKey(cardType) ||
            option.AncientPortraits.ContainsKey(cardType))
        {
            return new CardOptionLookup(option, []);
        }

        var matchedAssetPaths = option.Assets.Keys
            .Where(assetPath => CardArtMatches(assetPath, card))
            .ToArray();
        return option.CardPresentations.ContainsKey(cardType) || matchedAssetPaths.Length > 0
            ? new CardOptionLookup(option, matchedAssetPaths)
            : null;
    }

    private static bool CardOptionAffectsCardUncached(CardSkinOption option, CardModel card) =>
        BuildCardOptionLookup(option, card) != null;

    private static CardCoverageState GetCardCoverage(CardSkinGroup group)
    {
        if (CardCoverageCache.TryGetValue(group.Id, out var cached))
        {
            return cached;
        }

        var cards = ModelDb.AllCards
            .Select(card => GetCardLookup(card))
            .Where(lookup => lookup.PriorityGroups.Any(member => member.Id.Equals(
                group.Id, StringComparison.OrdinalIgnoreCase)))
            .ToArray();
        var byOption = group.Options.ToDictionary(
            option => option.Id,
            option => cards.Count(lookup =>
                lookup.OptionsById.ContainsKey(option.Id)),
            StringComparer.OrdinalIgnoreCase);
        cached = new CardCoverageState(cards.Length, byOption);
        CardCoverageCache[group.Id] = cached;
        return cached;
    }

    private static string GetCardPoolGroupId(CardModel card) =>
        (card.Pool?.Title ?? string.Empty).ToLowerInvariant();

    private static bool IsCharacterCardPool(CardModel card) =>
        card.Pool is { IsColorless: false } && !SharedCardPoolIds.Contains(GetCardPoolGroupId(card));

    private static string GetCardCatalogGroupId(CardModel card)
    {
        var poolGroupId = GetCardPoolGroupId(card);
        if (!SharedCardPoolIds.Contains(poolGroupId))
        {
            return poolGroupId;
        }

        return GetCardFilterGroupId(card);
    }

    private static readonly HashSet<CardRarity> MiscCardRarities =
    [
        CardRarity.Event,
        CardRarity.Token,
        CardRarity.Status,
        CardRarity.Curse,
        CardRarity.Quest
    ];

    private static string GetCardFilterGroupId(CardModel card)
    {
        if (card.Rarity == CardRarity.Ancient)
        {
            return "ancients";
        }

        return MiscCardRarities.Contains(card.Rarity)
            ? "misc"
            : GetCardPoolGroupId(card);
    }

    private static Texture2D? LoadIsolatedCardPortrait(
        string groupId,
        string selection,
        string resourcePath,
        bool useSelectedProvider)
    {
        // Raw raster entries are not Godot resources. Calling ResourceLoader for them first emits
        // two engine errors per card ("No loader found" + "Error loading resource") before the
        // direct-image fallback gets a chance to run. Large card-art packs can therefore flood
        // the log and stall the card library even though their bytes are perfectly valid. Resolve
        // direct provider images first; imported .ctex/.tres resources still take the overlay path
        // below because TryReadCardImageBytes deliberately accepts only raster source files.
        var catalog = Catalog;
        if (catalog != null && catalog.TryReadCardImageBytes(
                groupId,
                selection,
                resourcePath,
                useSelectedProvider,
                out var directBytes))
        {
            using var image = LoadRasterImageFromBuffer(directBytes, resourcePath);
            return ImageTexture.CreateFromImage(image);
        }

        var overlay = EnsureIsolatedCardOverlay(
            groupId,
            selection,
            useSelectedProvider,
            [resourcePath]);
        if (!overlay.ResourcePaths.TryGetValue(resourcePath, out var isolatedPath))
        {
            return null;
        }

        var loaded = ResourceLoader.Load<Texture2D>(
            isolatedPath,
            null,
            overlay.CanReuseExternalDependencies
                ? ResourceLoader.CacheMode.Ignore
                : ResourceLoader.CacheMode.IgnoreDeep);
        if (loaded != null)
        {
            return loaded;
        }

        // Some exported card providers ship raw raster files without Godot's imported
        // .import/.ctex pair.  The generated PCK is still useful for the normal resource path,
        // but ResourceLoader cannot decode that raw entry.  Read the selected provider's source
        // bytes directly and create an in-memory texture instead of falling back to the game
        // portrait.  This is deliberately provider-agnostic.
        return null;
    }

    private static Image LoadRasterImageFromBuffer(byte[] bytes, string imagePath)
    {
        if (bytes.Length == 0)
        {
            throw new InvalidOperationException($"独立卡牌图片为空：{imagePath}");
        }

        var image = new Image();
        Error error;
        if (HasPrefix(bytes, [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]))
        {
            error = image.LoadPngFromBuffer(bytes);
        }
        else if (HasPrefix(bytes, [0xff, 0xd8, 0xff]))
        {
            error = image.LoadJpgFromBuffer(bytes);
        }
        else if (bytes.Length >= 12 &&
                 HasPrefix(bytes, [(byte)'R', (byte)'I', (byte)'F', (byte)'F']) &&
                 bytes.AsSpan(8, 4).SequenceEqual([(byte)'W', (byte)'E', (byte)'B', (byte)'P']))
        {
            error = image.LoadWebpFromBuffer(bytes);
        }
        else
        {
            image.Dispose();
            throw new InvalidOperationException($"无法识别独立卡牌图片格式：{imagePath}");
        }

        if (error != Error.Ok || image.IsEmpty())
        {
            image.Dispose();
            throw new InvalidOperationException($"无法解码独立卡牌图片（{error}）：{imagePath}");
        }

        return image;

        static bool HasPrefix(byte[] value, ReadOnlySpan<byte> prefix) =>
            value.Length >= prefix.Length && value.AsSpan(0, prefix.Length).SequenceEqual(prefix);
    }

    private static IsolatedCardOverlayState EnsureIsolatedCardOverlay(
        string groupId,
        string selection,
        bool useSelectedProvider,
        IEnumerable<string> resourcePaths)
    {
        var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
        var sourceName = useSelectedProvider ? "provider" : "base";
        // Every unskinned card comes from the same game resource set. Share one baseline
        // namespace across card groups so switching categories does not upload another copy of
        // the game's giant card atlas to the GPU.
        var cacheKey = useSelectedProvider
            ? $"{groupId}\n{selection}\n{sourceName}"
            : $"{SkinCatalog.BaseOptionId}\n{sourceName}";
        if (!IsolatedCardOverlayCache.TryGetValue(cacheKey, out var state))
        {
            state = new IsolatedCardOverlayState(cacheKey);
            IsolatedCardOverlayCache.Add(cacheKey, state);
        }

        ReactivateIsolatedCardOverlayIfNeeded(state);

        var missingPaths = resourcePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(path => !state.ResourcePaths.ContainsKey(path) &&
                           !state.UnavailablePaths.Contains(path))
            .ToArray();
        if (missingPaths.Length == 0)
        {
            return state;
        }

        RuntimeResourceOverlay overlay;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var generation = ++_overlayGeneration;
            state.AliasToken ??=
                $"{_sessionId}/{generation:D3}_card_{sourceName}";
            overlay = catalog.BuildIsolatedCardResources(
                groupId,
                selection,
                missingPaths,
                useSelectedProvider,
                state.AliasToken,
                state.ResourcePaths);
            var newFiles = overlay.Files
                .Where(pair => !state.MountedFilePaths.Contains(pair.Key))
                .ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value,
                    StringComparer.OrdinalIgnoreCase);
            if (newFiles.Count == 0)
            {
                throw new InvalidOperationException("独立卡牌资源包没有产生新的可挂载文件。");
            }
            var overlayPath = System.IO.Path.Combine(
                SkinChangerPaths.CacheDirectory,
                $"sts2_skin_overlay_{_sessionId}_{generation:D3}_card_{sourceName}.pck");
            SkinChangerPaths.WriteCache(overlayPath, () => PckArchive.Write(overlayPath, newFiles));
            if (!ProjectSettings.LoadResourcePack(overlayPath, replaceFiles: true))
            {
                throw new InvalidOperationException("Godot 拒绝加载批量独立卡牌资源包。");
            }
            state.OverlayPaths.Add(overlayPath);
            state.MountedFilePaths.UnionWith(newFiles.Keys);
            state.CanonicalRedirectPaths.UnionWith(overlay.CanonicalDependencyPaths);
            state.CanReuseExternalDependencies &= overlay.CanReuseExternalDependencies;
            CardCanonicalResourceOwners.MarkActivated(
                state.OwnerId,
                overlay.CanonicalDependencyPaths);
            stopwatch.Stop();
            if (missingPaths.Length > 1 || stopwatch.ElapsedMilliseconds >= 100)
            {
                ModLog.Info(
                    $"已批量隔离 {groupId}/{selection} 的 {overlay.ResourcePaths.Count} 个卡牌资源" +
                    $"（{sourceName}，请求 {missingPaths.Length} 个），耗时={stopwatch.Elapsed.TotalMilliseconds:F1} ms。");
            }
        }
        catch
        {
            // A batch can fail because one provider resource is malformed. Do not poison every
            // other path in the batch; a later single-card request can still isolate the good ones.
            throw;
        }

        foreach (var pair in overlay.ResourcePaths)
        {
            state.ResourcePaths[pair.Key] = pair.Value;
        }
        foreach (var path in missingPaths)
        {
            if (!overlay.ResourcePaths.ContainsKey(path))
            {
                state.UnavailablePaths.Add(path);
            }
        }

        return state;
    }

    private static void ReactivateIsolatedCardOverlayIfNeeded(IsolatedCardOverlayState state)
    {
        if (state.OverlayPaths.Count == 0 ||
            !CardCanonicalResourceOwners.RequiresActivation(
                state.OwnerId,
                state.CanonicalRedirectPaths))
        {
            return;
        }

        foreach (var overlayPath in state.OverlayPaths)
        {
            if (!File.Exists(overlayPath) ||
                !ProjectSettings.LoadResourcePack(overlayPath, replaceFiles: true))
            {
                throw new InvalidOperationException(
                    "Godot 拒绝重新激活已缓存的独立卡牌资源包。");
            }
        }

        CardCanonicalResourceOwners.MarkActivated(
            state.OwnerId,
            state.CanonicalRedirectPaths);
    }

    private static string? SelectProviderCardPath(
        IReadOnlyList<string> paths,
        CardModel card,
        string originalPath)
    {
        string? selected = null;
        var selectedScore = int.MinValue;
        foreach (var path in paths)
        {
            var score = CardArtSelectionScore(path, card, originalPath);
            if (score > selectedScore ||
                (score == selectedScore &&
                 string.Compare(path, selected, StringComparison.OrdinalIgnoreCase) < 0))
            {
                selected = path;
                selectedScore = score;
            }
        }

        return selected;
    }

    private static bool CardArtMatches(string assetPath, CardModel card)
    {
        if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out var path)) return false;
        var portraitIdentity = CardPortraitIdentity(path);
        var poolGroupId = GetCardPoolGroupId(card);
        var assetIdentity = CardPortraitIdentity(
            assetPath,
            poolGroupId,
            portraitIdentity?.Category);
        if (assetIdentity == null || portraitIdentity == null)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(assetIdentity.Value.Category) &&
            !assetIdentity.Value.Category.Equals(
                poolGroupId, StringComparison.OrdinalIgnoreCase) &&
            !assetIdentity.Value.Category.Equals(
                portraitIdentity.Value.Category, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var typeStem = NormalizeCardToken(card.GetType().Name);
        return CardStemsMatch(assetIdentity.Value.Stem, portraitIdentity.Value.Stem) ||
               CardStemsMatch(assetIdentity.Value.Stem, typeStem);
    }

    private static (string Category, string Stem)? CardPortraitIdentity(
        string? path,
        string? expectedCategory = null,
        string? alternateCategory = null)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        var lowerPath = path.ToLowerInvariant();
        var markerIndex = -1;
        var markerLength = 0;
        foreach (var marker in new[]
                 {
                     "/card_portraits/",
                     "/card_atlas.sprites/",
                     "/cards/",
                     "/card/",
                     "/card_art/",
                     "/cardart/"
                 })
        {
            markerIndex = lowerPath.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex >= 0)
            {
                markerLength = marker.Length;
                break;
            }
        }

        var category = string.Empty;
        if (markerIndex >= 0)
        {
            var directoryStart = markerIndex + markerLength;
            var fileSeparator = lowerPath.LastIndexOf('/');
            if (fileSeparator >= directoryStart)
            {
                var directories = lowerPath[directoryStart..fileSeparator]
                    .Split('/', StringSplitOptions.RemoveEmptyEntries);
                var categoryIndex = Array.FindIndex(directories, candidate =>
                    (!string.IsNullOrWhiteSpace(expectedCategory) &&
                     candidate.Equals(expectedCategory, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(alternateCategory) &&
                     candidate.Equals(alternateCategory, StringComparison.OrdinalIgnoreCase)));
                if (categoryIndex < 0 && directories.Length > 0)
                {
                    categoryIndex = 0;
                }

                if (categoryIndex >= 0)
                {
                    category = directories[categoryIndex];
                }
            }
        }

        var fileName = lowerPath[(lowerPath.LastIndexOf('/') + 1)..];
        var extensionIndex = fileName.LastIndexOf('.');
        var rawStem = extensionIndex >= 0 ? fileName[..extensionIndex] : fileName;
        var typeSeparator = rawStem.LastIndexOf('.');
        if (typeSeparator >= 0)
        {
            rawStem = rawStem[(typeSeparator + 1)..];
        }

        foreach (var suffix in new[]
                 {
                     "_card_art", "-card-art", " card art", "card_art", "cardart"
                 })
        {
            if (rawStem.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                rawStem = rawStem[..^suffix.Length];
                break;
            }
        }

        var stem = NormalizeCardToken(rawStem);
        return (category, stem);
    }

    private static bool CardStemsMatch(string candidate, string expected) =>
        candidate.Equals(expected, StringComparison.OrdinalIgnoreCase) ||
        candidate.Equals(expected + "ancient", StringComparison.OrdinalIgnoreCase) ||
        candidate.Equals(expected + "normal", StringComparison.OrdinalIgnoreCase) ||
        candidate.Equals(expected + "portrait", StringComparison.OrdinalIgnoreCase) ||
        candidate.Equals(expected + "art", StringComparison.OrdinalIgnoreCase) ||
        IsNumberedCardVariant(candidate, expected);

    private static bool IsNumberedCardVariant(string candidate, string expected) =>
        candidate.StartsWith(expected, StringComparison.OrdinalIgnoreCase) &&
        candidate.Length > expected.Length &&
        candidate[expected.Length..].All(char.IsDigit);

    private static string NormalizeCardToken(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static bool HasSameResourceExtension(string left, string right)
    {
        var leftExtension = System.IO.Path.GetExtension(left);
        var rightExtension = System.IO.Path.GetExtension(right);
        return leftExtension.Equals(rightExtension, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAncientStyleEnabled(
        CardSkinOption option,
        string cardType,
        CardModel card)
    {
        if (!option.AncientPortraits.ContainsKey(cardType))
        {
            return false;
        }

        if (!AncientStyleMethods.TryGetValue(option.Id, out var method) &&
            !MissingAncientStyleMethods.Contains(option.Id))
        {
            method = AppDomain.CurrentDomain.GetAssemblies()
                .Where(assembly => assembly.GetName().Name?.Equals(
                    option.ProviderId ?? option.Id, StringComparison.OrdinalIgnoreCase) == true)
                .Select(AncientStyleMethodPolicy.Find)
                .FirstOrDefault(candidate => candidate != null);
            if (method == null)
            {
                MissingAncientStyleMethods.Add(option.Id);
            }
            else
            {
                AncientStyleMethods[option.Id] = method;
            }
        }

        if (method == null)
        {
            // Exported card projects commonly declare both image and ancientImage for every
            // card.  Without their optional ConfigHelper, ancientImage belongs to the game's
            // Ancient layout only; using it for every normal card makes a 375x527 portrait get
            // fitted into the normal 375x285 slot and visibly narrows the artwork. A provider
            // presentation that explicitly requests the Ancient layout is different: choosing
            // the normal/base image there would produce an Ancient frame with vanilla art.
            var requestsAncientLayout = option.CardPresentations
                .GetValueOrDefault(cardType)?.UseAncientLayout == true;
            return AncientStyleMethodPolicy.ResolveWithoutProviderMethod(
                card.Rarity == CardRarity.Ancient,
                requestsAncientLayout);
        }

        try
        {
            return method.Invoke(null, [cardType]) as bool? ?? true;
        }
        catch (Exception exception)
        {
            // 反射调用失败时每次渲染都会重复触发，只警告一次避免刷日志。
            if (FailedAncientStyleMethods.Add(option.Id))
            {
                ModLog.Warn($"读取 {option.Id} 的先古卡图样式设置失败：{exception.Message}");
            }

            return true;
        }
    }

    public static PackedScene LoadRuntimeScene(string groupId, string scenePath)
    {
        var resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
        return LoadRuntimeResources(
                   groupId,
                   resourcePaths,
                   includeProviderDependencies: true)
               .GetValueOrDefault(scenePath) as PackedScene ??
               throw new InvalidOperationException($"独立皮肤资源不是场景：{scenePath}");
    }

    public static PackedScene GetOrLoadRuntimeScene(string groupId, string scenePath)
    {
        var resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
        return LoadRuntimeResources(
                   groupId,
                   resourcePaths,
                   includeProviderDependencies: true)
               .GetValueOrDefault(scenePath) as PackedScene ??
               throw new InvalidOperationException($"独立皮肤资源不是场景：{scenePath}");
    }

    /// <summary>
    /// Loads and instantiates a scene while its per-selection overlay is still mounted.
    /// PackedScene keeps some binary external-resource references lazy; returning the scene and
    /// instantiating it after <see cref="WithRuntimeResources{T}"/> restores the canonical packs
    /// can therefore bind a remote player's model to the other player's currently mounted skin.
    /// </summary>
    public static T InstantiateRuntimeScene<T>(
        string groupId,
        string scenePath,
        Action? beforeInstantiate = null)
        where T : Node
    {
        var resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
        if (AppearanceDiagnostics.Enabled)
        {
            var effective = GetVisualSelection(groupId);
            var option = FindVisualOption(groupId, effective);
            AppearanceDiagnostics.Log(
                "场景实例化",
                $"分组={groupId} 生效选择={effective} " +
                $"提供者={Catalog?.ResolveVisualProviderId(effective) ?? "?"} " +
                $"资源数={option?.Assets.Count ?? 0} 场景={scenePath} 依赖={resourcePaths.Count}",
                120);
        }

        return WithRuntimeResources(
            groupId,
            resourcePaths,
            resources =>
            {
                beforeInstantiate?.Invoke();
                var scene = resources.GetValueOrDefault(scenePath) as PackedScene ??
                            throw new InvalidOperationException($"独立皮肤资源不是场景：{scenePath}");
                return scene.Instantiate<T>(PackedScene.GenEditState.Disabled);
            },
            includeProviderDependencies: true);
    }

    /// <summary>
    /// True when the effective selection for a group (multiplayer scope, preview or configuration)
    /// serves the character's canonical combat scene path itself. The game's own creation then
    /// instantiates the skin's scene instead of the game's, so creation has to be owned up front
    /// and converted by the scene factory when the served scene is a framework scene.
    /// </summary>
    public static bool SelectionServesCombatScene(string groupId, string scenePath)
    {
        lock (Sync)
        {
            var option = FindVisualOption(groupId, GetVisualSelection(groupId));
            return CharacterCombatSceneInstantiationPolicy.ShouldOwnCanonicalSceneInstantiation(
                option == null || option.Id.Equals(
                    SkinCatalog.BaseOptionId,
                    StringComparison.OrdinalIgnoreCase),
                option?.Assets.Keys.Any(path =>
                    path.Equals(scenePath, StringComparison.OrdinalIgnoreCase)) == true);
        }
    }

    private static SkinOption? FindVisualOption(string groupId, string selection) =>
        Catalog?.Groups.FirstOrDefault(group =>
                group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?
            .Options.FirstOrDefault(option =>
                option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));

    public static bool TryInstantiateSelectedCharacterCreatureVisuals(
        string groupId,
        string scenePath,
        Func<NCreatureVisuals, NCreatureVisuals>? configureVisuals,
        out NCreatureVisuals visuals)
    {
        lock (Sync)
        {
            var option = FindVisualOption(groupId, GetVisualSelection(groupId));
            var hasManagedCombatScene = option?.FrameworkContract?
                                            .CharacterResources.ContainsKey("CombatVisual") == true ||
                                        option?.Assets.Keys.Any(path =>
                                            path.Equals(scenePath, StringComparison.OrdinalIgnoreCase)) == true;
            var hasManagedCombatDependencies = option != null &&
                                               CharacterCombatSceneInstantiationPolicy
                                                   .HasManagedCombatDependencies(
                                                       scenePath,
                                                       option.Assets.Keys);
            var isBaseSelection = option == null || option.Id.Equals(
                SkinCatalog.BaseOptionId,
                StringComparison.OrdinalIgnoreCase);
            if (!CharacterCombatSceneInstantiationPolicy.ShouldUseManagedFactory(
                    isBaseSelection,
                    hasManagedCombatScene,
                    hasManagedCombatDependencies))
            {
                visuals = null!;
                return false;
            }
        }

        visuals = InstantiateManagedCharacterCreatureVisuals(
            groupId,
            scenePath,
            configureVisuals);
        return true;
    }

    public static NCreatureVisuals InstantiateManagedCharacterCreatureVisuals(
        string groupId,
        string scenePath,
        Func<NCreatureVisuals, NCreatureVisuals>? configureVisuals = null)
    {
        var resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
        return WithRuntimeResources(
            groupId,
            resourcePaths,
            resources =>
            {
                var scene = resources.GetValueOrDefault(scenePath) as PackedScene ??
                            throw new InvalidOperationException(
                                $"角色皮肤资源不是场景：{scenePath}");
                var visuals = FrameworkCreatureSceneFactory.Create(scene);
                return configureVisuals?.Invoke(visuals) ?? visuals;
            },
            includeProviderDependencies: true,
            takeOverCanonicalPaths: true);
    }

    /// <summary>
    /// Mounts a selected visual's private resource overlay for a short-lived Godot lifecycle
    /// callback (for example NCreature._Ready).  Unlike <see cref="WithRuntimeResources{T}"/>,
    /// this does not load a root resource; it keeps canonical external references visible while
    /// the already-instantiated scene is being attached to the tree, then restores the local
    /// selection when the callback finishes.
    /// </summary>
    public static IDisposable BeginRuntimeResourceScope(string groupId, string scenePath)
    {
        var resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
        lock (Sync)
        {
            var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
            var selection = GetVisualSelection(groupId);
            var prepared = GetOrPrepareRuntimeOverlay(
                catalog,
                groupId,
                selection,
                resourcePaths,
                includeProviderDependencies: true);
            MountContextualFullRuntimeProviderPacks(catalog, selection);
            if (prepared.OverlayPath != null &&
                !ProjectSettings.LoadResourcePack(prepared.OverlayPath, replaceFiles: true))
            {
                throw new InvalidOperationException("Godot 拒绝加载已准备的独立皮肤场景资源包。");
            }

            if (prepared.CanonicalDependencyPaths.Count > 0)
            {
                if (!RuntimeCanonicalDependencyPaths.TryGetValue(groupId, out var trackedPaths))
                {
                    trackedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    RuntimeCanonicalDependencyPaths[groupId] = trackedPaths;
                }

                trackedPaths.UnionWith(prepared.CanonicalDependencyPaths);
            }

            // A remote provider is not necessarily selected in the local config, so its Godot
            // scripts may not have been registered by the normal global mount yet.  Register
            // them after the private files are visible, before any _Ready callback can resolve
            // script-backed nodes.
            if (catalog.IsRuntimeProviderOption(groupId, selection) &&
                catalog.ProviderUsesManagedGodotScripts(selection))
            {
                ManagedSkinModLoader.EnsureProviderGodotScripts(
                    catalog.ResolveVisualProviderId(selection));
            }

            var restoreGroups = prepared.RestoreGroups
                .Append(groupId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            return new MountedRuntimeResourceScope(restoreGroups, ManualCharacterVariantBridge.BeginScope(groupId));
        }
    }

    private static IReadOnlyList<string> RuntimeSceneResourcePaths(string groupId, string scenePath)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            var selection = GetVisualSelection(groupId);
            var mode = catalog?.GetRuntimeMonsterVisualMode(groupId, selection);
            IEnumerable<string> modeResourcePaths = catalog != null &&
                                                    CanReuseMountedLargeProviderPack(
                                                        catalog,
                                                        groupId,
                                                        selection)
                ? []
                : mode?.ResourcePaths ?? [];
            return new[] { scenePath }
                .Concat(modeResourcePaths)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private static bool CanReuseMountedLargeProviderPack(
        SkinCatalog catalog,
        string groupId,
        string selection)
    {
        if (!Config.GetSelection(groupId).Equals(
                selection,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var option = catalog.Groups.FirstOrDefault(group =>
                group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?
            .Options.FirstOrDefault(option =>
                option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
        return option != null && catalog.GetProviderResourcePackPaths(option.EffectiveProviderId)
            .Select(System.IO.Path.GetFullPath)
            .Any(MountedLargeRuntimeProviderPacks.ContainsKey);
    }

    public static Resource GetOrLoadRuntimeResource(
        string groupId,
        string resourcePath,
        bool includeProviderDependencies = false)
    {
        lock (Sync)
        {
            var cacheKey = RuntimeResourceKey(groupId, resourcePath);
            if (RuntimeResourceCache.TryGetValue(cacheKey, out var cached) &&
                GodotObject.IsInstanceValid(cached))
            {
                return cached;
            }

            RuntimeResourceCache.Remove(cacheKey);
            return LoadRuntimeResources(
                groupId,
                [resourcePath],
                includeProviderDependencies)[resourcePath];
        }
    }

    public static Resource GetOrLoadCharacterIconResource(
        string groupId,
        string resourcePath,
        bool includeProviderDependencies = false)
    {
        lock (Sync)
        {
            var selection = GetVisualSelection(groupId);
            var cacheKey = RuntimeResourceKey(groupId, selection, resourcePath);
            if (RuntimeResourceCache.TryGetValue(cacheKey, out var cached) &&
                GodotObject.IsInstanceValid(cached))
            {
                return cached;
            }

            RuntimeResourceCache.Remove(cacheKey);
            return LoadRuntimeResourcesForSelection(
                groupId,
                selection,
                [resourcePath],
                includeProviderDependencies)[resourcePath];
        }
    }

    public static T WithCharacterIconResource<T>(
        string groupId,
        string resourcePath,
        Func<Resource, T> callback,
        bool includeProviderDependencies = false)
    {
        ArgumentNullException.ThrowIfNull(callback);
        lock (Sync)
        {
            var selection = GetVisualSelection(groupId);
            return WithRuntimeResourcesForSelection(
                groupId,
                selection,
                [resourcePath],
                resources => callback(resources[resourcePath]),
                includeProviderDependencies,
                takeOverCanonicalPaths: false);
        }
    }

    internal static bool IsSelectedCharacterStandaloneTexture(string resourcePath)
    {
        lock (Sync)
            return Catalog?.IsSelectedCharacterStandaloneTexture(resourcePath, GetVisualSelections()) == true;
    }

    public static Texture2D? GetRelicIconOverride(string resourcePath)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            var groupId = catalog?.FindSelectedRelicIconGroup(
                resourcePath,
                Config.Selections,
                Config.VisualProviderPriority);
            if (catalog == null)
            {
                return null;
            }

            if (groupId == null)
            {
                return GetBaselineRelicIcon(catalog, resourcePath);
            }

            var cacheKey = RuntimeResourceKey(groupId, resourcePath);
            if (!RuntimeResourceCache.TryGetValue(cacheKey, out var cached) ||
                !GodotObject.IsInstanceValid(cached))
            {
                LoadSelectedRelicBundle(catalog, groupId);
                cached = RuntimeResourceCache.GetValueOrDefault(cacheKey);
            }

            return cached as Texture2D ?? throw new InvalidOperationException(
                $"隔离的遗物图标不是贴图：{resourcePath}");
        }
    }

    private static Texture2D? GetBaselineRelicIcon(SkinCatalog catalog, string resourcePath)
    {
        if (BaselineRelicIconCache.TryGetValue(resourcePath, out var cached) &&
            GodotObject.IsInstanceValid(cached))
        {
            return cached;
        }

        BaselineRelicIconCache.Remove(resourcePath);
        var ownerGroupId = catalog.FindRelicIconOwnerGroup(resourcePath);
        if (ownerGroupId == null ||
            !catalog.TryGetBaselineRelicTextureDefinition(resourcePath, out var definition))
        {
            return null;
        }

        if (!BaselineRelicAtlasCache.TryGetValue(definition.AtlasPath, out var atlas) ||
            !GodotObject.IsInstanceValid(atlas))
        {
            BaselineRelicAtlasCache.Remove(definition.AtlasPath);
            var prepared = GetOrPrepareRuntimeOverlay(
                catalog,
                ownerGroupId,
                SkinCatalog.BaseOptionId,
                [definition.AtlasPath],
                includeProviderDependencies: false,
                isolateRelicCanonicalPaths: true);
            if (prepared.OverlayPath != null &&
                !ProjectSettings.LoadResourcePack(prepared.OverlayPath, replaceFiles: true))
            {
                throw new InvalidOperationException("Godot 拒绝加载原版遗物私有图集。");
            }

            if (!prepared.ResourcePaths.TryGetValue(definition.AtlasPath, out var atlasAlias))
            {
                throw new InvalidOperationException($"无法隔离原版遗物图集：{definition.AtlasPath}");
            }

            atlas = ResourceLoader.Load<Texture2D>(
                        atlasAlias,
                        null,
                        ResourceLoader.CacheMode.IgnoreDeep) ??
                    throw new InvalidOperationException($"无法加载原版遗物私有图集：{atlasAlias}");
            BaselineRelicAtlasCache[definition.AtlasPath] = atlas;
        }

        var result = new AtlasTexture
        {
            Atlas = atlas,
            Region = new Rect2(
                definition.Region.X,
                definition.Region.Y,
                definition.Region.Width,
                definition.Region.Height),
            Margin = new Rect2(
                definition.Margin.X,
                definition.Margin.Y,
                definition.Margin.Width,
                definition.Margin.Height),
            FilterClip = definition.FilterClip
        };
        BaselineRelicIconCache[resourcePath] = result;
        return result;
    }

    private static void LoadSelectedRelicBundle(SkinCatalog catalog, string groupId)
    {
        var group = catalog.Groups.First(group => group.Id.Equals(
            groupId,
            StringComparison.OrdinalIgnoreCase));
        var selection = Config.GetSelection(groupId);
        var selected = group.Options.First(option => option.Id.Equals(
            selection,
            StringComparison.OrdinalIgnoreCase));
        var relicPaths = catalog.GetProviderRelicSpritePaths(selected);
        if (relicPaths.Count == 0)
        {
            return;
        }

        var started = Stopwatch.GetTimestamp();
        var prepared = GetOrPrepareRuntimeOverlay(
            catalog,
            groupId,
            selection,
            relicPaths,
            includeProviderDependencies: false,
            isolateRelicCanonicalPaths: true);

        if (prepared.OverlayPath != null &&
            !ProjectSettings.LoadResourcePack(prepared.OverlayPath, replaceFiles: true))
        {
            throw new InvalidOperationException("Godot 拒绝加载遗物皮肤资源包。");
        }

        var loaded = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var providerAtlases = prepared.ResourcePaths
                .Where(pair => SkinCatalog.IsRelicAtlasTexturePath(pair.Key))
                .ToDictionary(
                    pair => pair.Key,
                    pair => ResourceLoader.Load<Texture2D>(
                                pair.Value,
                                null,
                                ResourceLoader.CacheMode.IgnoreDeep) ??
                            throw new InvalidOperationException(
                                $"无法加载遗物皮肤私有图集：{pair.Value}"),
                    StringComparer.OrdinalIgnoreCase);
            var normalAtlas = providerAtlases.FirstOrDefault(pair =>
                !pair.Key.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase)).Value;
            var outlineAtlas = providerAtlases.FirstOrDefault(pair =>
                pair.Key.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase)).Value;
            var needsNormalAtlas = relicPaths.Any(path => !path.Contains(
                "relic_outline_atlas",
                StringComparison.OrdinalIgnoreCase));
            var needsOutlineAtlas = relicPaths.Any(path => path.Contains(
                "relic_outline_atlas",
                StringComparison.OrdinalIgnoreCase));
            if ((needsNormalAtlas && normalAtlas == null) ||
                (needsOutlineAtlas && outlineAtlas == null))
            {
                throw new InvalidOperationException("遗物皮肤缺少其切片引用的图集。");
            }

            foreach (var relicPath in relicPaths)
            {
                if (!prepared.ResourcePaths.TryGetValue(relicPath, out var alias))
                {
                    continue;
                }

                var texture = ResourceLoader.Load<AtlasTexture>(
                                  alias,
                                  null,
                                  ResourceLoader.CacheMode.Ignore) ??
                              throw new InvalidOperationException(
                                  $"无法加载遗物皮肤切片：{alias}");
                texture.Atlas = relicPath.Contains(
                    "relic_outline_atlas",
                    StringComparison.OrdinalIgnoreCase)
                    ? outlineAtlas!
                    : normalAtlas!;
                loaded[relicPath] = texture;
            }
        }
        finally
        {
            var restoreGroups = prepared.RestoreGroups
                .Append(groupId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            MountOverlay(restoreGroups);
        }

        foreach (var resource in loaded)
        {
            RuntimeResourceCache[RuntimeResourceKey(groupId, resource.Key)] = resource.Value;
        }

        ModLog.Info(
            $"已一次性加载 {groupId} 的 {loaded.Count} 个遗物皮肤切片；" +
            $"运行包={prepared.FileCount} 个文件/{prepared.FileSize / 1024d:F1} KiB，" +
            $"耗时={Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1} ms。");
    }

    public static bool IsRuntimeProviderSelected(string groupId)
    {
        lock (Sync)
        {
            return Catalog?.IsRuntimeProviderOption(groupId, GetVisualSelection(groupId)) == true;
        }
    }

    internal static bool IsDirectCharacterRuntimeProviderSelected(
        string providerId,
        string characterTypeName)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null || string.IsNullOrWhiteSpace(characterTypeName))
            {
                return true;
            }

            var group = catalog.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(characterTypeName, StringComparison.OrdinalIgnoreCase) ||
                candidate.DisplayName.Equals(characterTypeName, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                // Unknown names belong to a future character or a provider-specific alias. Do
                // not suppress those paths; the route is only a safety gate for known groups.
                return true;
            }

            return catalog.ProviderUsesDirectCharacterRuntime(providerId) &&
                   catalog.SelectionUsesVisualProvider(
                       group.Id,
                       GetVisualSelection(group.Id),
                       providerId);
        }
    }

    /// <summary>
    /// Re-mounts the fake merchant event art right before the event scene is instantiated. The
    /// scene binds its Spine skeleton (and that skeleton's texture) while it loads, so the pack that
    /// redrew the tent has to be on top at exactly this moment; a later overlay rebuild cannot
    /// change a skeleton that is already bound.
    /// </summary>
    public static void MountFakeMerchantEventArt()
    {
        try
        {
            var catalog = Catalog;
            if (catalog == null)
            {
                return;
            }

            var files = catalog.FakeMerchantEventPresentation()
                .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
            if (files.Count == 0)
            {
                return;
            }

            lock (Sync)
            {
                MountOverlayDelta(files, "fake_merchant_event", "Godot 拒绝加载假商人事件背景资源包。");
                _fakeMerchantEventArtMounted = true;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("假商人事件背景重挂失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Mounts the fake merchant event art only if this session has not done so yet. Previews and
    /// the background list use this so they never re-load the (large) event pack per option while
    /// still not depending on the event screen having run first.
    /// </summary>
    public static void EnsureFakeMerchantEventArtMounted()
    {
        lock (Sync)
        {
            if (_fakeMerchantEventArtMounted)
            {
                return;
            }
        }

        MountFakeMerchantEventArt();
    }

    public static string? GetSelectedFullRuntimeProvider(string groupId)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return null;
            }

            var selection = GetVisualSelection(groupId);
            if (!Catalog.TryGetVisualProviderId(groupId, selection, out var providerId))
            {
                return null;
            }

            return Catalog.IsRuntimeProviderOption(groupId, selection) &&
                   Catalog.ProviderUsesFullRuntime(providerId) &&
                   Catalog.IsFullRuntimeProviderFullySelected(providerId, GetVisualSelections())
                ? providerId
                : null;
        }
    }

    internal static bool? IsCharacterBehaviorSelected(
        System.Reflection.Assembly assembly,
        MegaCrit.Sts2.Core.Models.CharacterModel character,
        MegaCrit.Sts2.Core.Entities.Players.Player? player)
    {
        // Player -> CharacterModel predicates inherit their caller's identity. Use the same
        // selection overlay as avatar/creature hot reload, not this computer's saved selection
        // for every instance of that character. This does not mount or load any resources.
        using var scope = player == null ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId);
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null) return null;
            // Predicates may run frequently during effects. Cache only stable type -> group
            // identity, never a selection result; no scene scan, disk access or reload here.
            var groups = CharacterBehaviorGroups.GetValue(catalog, static _ => new());
            var characterType = character.GetType();
            if (!groups.TryGetValue(characterType, out var groupId))
            {
                var group = Ui.ContextualSkinControls.FindGroup(character.Id.Entry, characterType.Name);
                if (group == null) return null;
                groups[characterType] = groupId = group.Id;
            }
            var providerId = GetSelectedRuntimeProvider(groupId);
            return providerId != null && ManagedSkinModLoader.IsProviderAssemblyFor(providerId, assembly);
        }
    }

    internal static string? GetSelectedCreatureRuntimeProvider(string groupId)
    {
        lock (Sync)
        {
            var fullProvider = GetSelectedFullRuntimeProvider(groupId);
            if (fullProvider != null)
            {
                return fullProvider;
            }

            // Independently selectable character runtimes still own isolated NCreature._Ready
            // callbacks. Full-package linkage controls selections, not whether the selected
            // creature needs its author's skeleton/actor initialization replayed.
            var providerId = GetSelectedRuntimeProvider(groupId);
            return providerId != null &&
                   Catalog?.ProviderUsesDirectCharacterRuntime(providerId) == true
                ? providerId
                : null;
        }
    }

    private static string _lastRuntimeProviderProbe = string.Empty;

    public static string? GetSelectedRuntimeProvider(string groupId)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null)
            {
                return null;
            }

            var selection = GetVisualSelection(groupId);
            var isRuntime = catalog.IsRuntimeProviderOption(groupId, selection);
            var resolved = isRuntime &&
                           catalog.TryGetVisualProviderId(groupId, selection, out var providerId)
                ? providerId
                : null;

            // 皮肤包（血量换装那一族）靠这个方法拿到「当前选中的是不是它自己」。
            // 日志只在结果变化时打一次，避免每帧刷屏。
            var probe = groupId + "\n" + selection + "\n" + (resolved ?? "<null>");
            if (!string.Equals(probe, _lastRuntimeProviderProbe, StringComparison.Ordinal))
            {
                _lastRuntimeProviderProbe = probe;
                ModLog.Info(
                    $"[选角闸门] 分组={groupId} 选择={selection} 是运行包={isRuntime} 返回={resolved ?? "<null>"}");
            }

            return resolved;
        }
    }

    /// <summary>
    /// The selection for this group is a provider-owned per-monster runtime profile: the provider
    /// constructs the replacement body from its own skeleton at runtime, so Skin Changer only
    /// mounts those resources and must not instantiate the canonical host scene over the result.
    /// </summary>
    public static bool SelectionOwnsMonsterRuntimeProfile(string groupId)
    {
        lock (Sync)
        {
            return FindVisualOption(groupId, GetVisualSelection(groupId))
                ?.IsManagedMonsterRuntimeProfile == true;
        }
    }

    public static void ApplySelectedVisualPostfix(
        string groupId,
        object model,
        ref MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals)
    {
        string? providerId;
        lock (Sync)
        {
            var selection = GetVisualSelection(groupId);
            var catalog = Catalog;
            // A summon group is decided by its own selection alone: a pack chosen for the summon
            // still has to finish that summon's visuals (Orchis removes the vanilla Osty flame and
            // offsets the body). Requiring the pack to be fully selected also on the owner character
            // dropped exactly that work, so the Orchis summon kept vanilla effects.
            // The same holds for a character group: the provider picked for this group is the one
            // whose pack was promoted for this scene, so its CreateVisuals postfix has to run here
            // even when another group (the summon) picks a different pack. Otherwise Orchis kept the
            // vanilla head flame on the player model.
            var allowed = catalog?.IsRuntimeProviderOption(groupId, selection) == true;
            providerId = allowed ? catalog!.ResolveVisualProviderId(selection) : null;
        }

        if (providerId != null)
        {
            ManagedSkinModLoader.ApplySelectedVisualPostfix(providerId, model, ref visuals);
        }
    }

    /// <summary>
    /// Re-applies the independent Osty mod's own skin after the summon's visual node was rebuilt.
    /// The rebuild replaces the sprite that mod just installed, so applying it before the rebuild
    /// made the in-run "撑在地上的鸡煲" entry look like it did nothing.
    /// </summary>
    internal static void ReapplyCompanionImpostorSkin()
    {
        var impostorSkinId = Config.CompanionImpostorSkinId;
        if (string.IsNullOrWhiteSpace(impostorSkinId))
        {
            // The summon goes back to a skin pack's own Osty resources, so the independent mod has to
            // let go of the body it currently owns as well.
            OstyImpostorBridge.ApplyQuiet(null);
            OstyImpostorBridge.RefreshLiveSummons();
            return;
        }

        OstyImpostorBridge.ApplyQuiet(impostorSkinId!);
        OstyImpostorBridge.RefreshLiveSummons();
        ModLog.Info($"[奥斯提] 重建后重新套用独立 Mod 皮肤：{impostorSkinId}");
    }

    public static bool IsScopedMonsterRuntimeProviderSelected(
        string providerId,
        string monsterId) => ScopedMonsterSelections.IsSelected(providerId, monsterId);

    internal static void ReleaseTransientRuntimeResources(IEnumerable<string> groupIds)
    {
        lock (Sync)
        {
            var releasedResources = 0;
            var releasedBundles = 0;
            foreach (var groupId in groupIds
                         .Where(groupId => !string.IsNullOrWhiteSpace(groupId))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var prefix = groupId + "\n";
                foreach (var key in RuntimeResourceCache.Keys
                             .Where(key => key.StartsWith(
                                 prefix,
                                 StringComparison.OrdinalIgnoreCase))
                             .ToArray())
                {
                    if (RuntimeResourceCache.Remove(key))
                    {
                        releasedResources++;
                    }
                }

                foreach (var key in RuntimeResourceBundles.Keys
                             .Where(key => key.StartsWith(
                                 prefix,
                                 StringComparison.OrdinalIgnoreCase))
                             .ToArray())
                {
                    if (RuntimeResourceBundles.Remove(key))
                    {
                        releasedBundles++;
                    }
                }
            }

            if (releasedResources > 0 || releasedBundles > 0)
            {
                ModLog.Info(
                    $"战斗房间退出后已释放 {releasedResources} 个怪物资源引用/" +
                    $"{releasedBundles} 个资源包缓存；已生成的运行包仍保留供下次快速复用。");
            }
        }
    }

    public static void ApplyScopedMonsterRuntimeProviderSelection(
        string providerId,
        string monsterId,
        bool enabled)
    {
        string? groupId;
        string currentSelection;
        lock (Sync)
        {
            var catalog = Catalog;
            if (catalog == null || !catalog.ProviderUsesScopedMonsterRuntime(providerId))
            {
                return;
            }

            groupId = catalog.ResolveManagedMonsterGroupId(monsterId);
            if (groupId == null)
            {
                return;
            }

            currentSelection = Config.GetSelection(groupId);
        }

        if (enabled)
        {
            if (!currentSelection.Equals(providerId, StringComparison.OrdinalIgnoreCase))
            {
                ApplySelection(groupId, providerId);
            }
        }
        else if (currentSelection.Equals(providerId, StringComparison.OrdinalIgnoreCase))
        {
            ApplySelection(groupId, SkinCatalog.BaseOptionId);
        }
    }

    public static bool IsExternalRuntimeProviderSelected(string groupId)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return false;
            }

            var selection = GetVisualSelection(groupId);
            return Catalog.IsRuntimeProviderOption(groupId, selection) &&
                   !Catalog.IsResourceBackedOption(groupId, selection) &&
                   Catalog.GetRuntimeImagePath(groupId, selection) != null;
        }
    }

    /// <summary>
    /// True when the skin selected for this group really ships a file for this canonical resource.
    /// The Skin Changer may only take a path over in that case; anything else used to be "loaded"
    /// through the canonical fallback, which handed back the game's vanilla art and flattened every
    /// code-driven provider (CZN/Meirin keep their avatars in their DLL and register no image).
    /// </summary>
    public static bool SelectedSkinProvidesResource(string groupId, string resourcePath)
    {
        lock (Sync)
        {
            return Catalog?.SelectedOptionProvidesResource(
                groupId,
                GetVisualSelection(groupId),
                resourcePath) == true;
        }
    }

    public static bool ShouldDeferCharacterIconResourceToExternalRuntime(
        string groupId,
        string resourcePath)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return false;
            }

            // A DLL/framework provider draws its own select-portrait, top-bar avatar and map marker
            // by patching the CharacterModel getters after us. Pure resource skins keep the old
            // unconditional takeover.
            var visualSelection = GetVisualSelection(groupId);

            // A settings variant is painted by the Skin Changer even when that package's own code cannot
            // run (its player gate is off for a same-identity sibling's selection). Handing the icons
            // back to the runtime provider in that state left the top-left run avatar on the vanilla art.
            if (ProviderSettingsVariantOptions.ContractOf(visualSelection) != null)
            {
                return false;
            }

            if (!Catalog.IsRuntimeProviderOption(groupId, visualSelection))
            {
                return false;
            }

            if (!Catalog.IsResourceBackedOption(groupId, visualSelection))
            {
                return true;
            }

            // A runtime provider may still ship some files itself. Defer for the paths it does not
            // ship: CZN packs keep their avatars inside their own DLL and register no standalone
            // image at all (the provider audit reports "groups=[], 独立图片=0"), so loading the
            // canonical path only fetched the game's vanilla texture and overwrote whatever the
            // provider had just set.
            return !Catalog.SelectedOptionProvidesResource(groupId, visualSelection, resourcePath);
        }
    }

    public static bool IsInteractiveRuntimeProviderSelected(string groupId)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                return false;
            }

            var selection = GetVisualSelection(groupId);
            return Catalog.IsRuntimeProviderOption(groupId, selection) &&
                   Catalog.ProviderUsesInteractiveRuntime(selection);
        }
    }

    public static bool IsManagedResourceOptionSelected(string groupId)
    {
        lock (Sync)
        {
            var catalog = Catalog;
            return catalog != null &&
                   catalog.IsResourceBackedOption(groupId, GetVisualSelection(groupId));
        }
    }

    public static float GetSelectedMonsterScale(string groupId)
    {
        lock (Sync)
        {
            var optionId = Config.GetSelection(groupId);
            return Config.MonsterScales.TryGetValue(groupId, out var options) &&
                   options.TryGetValue(optionId, out var scale)
                ? Mathf.Clamp(scale, MinimumMonsterScale, MaximumMonsterScale)
                : 1f;
        }
    }

    public static float SetSelectedMonsterScale(
        string groupId,
        float scale,
        bool save = true)
    {
        lock (Sync)
        {
            var normalized = Mathf.Clamp(
                Mathf.Round(scale / MonsterScaleStep) * MonsterScaleStep,
                MinimumMonsterScale,
                MaximumMonsterScale);
            StoreMonsterScale(groupId, Config.GetSelection(groupId), normalized);

            // Shared-animation families are split into placeholder sibling groups in the game;
            // the slider belongs to the canonical group, so copy the same value to each sibling
            // that actually exists in the current catalog.
            if (Catalog != null &&
                SkinCatalog.SharedAnimationSiblingGroups.TryGetValue(groupId, out var siblingGroupIds))
            {
                foreach (var siblingGroupId in siblingGroupIds)
                {
                    if (siblingGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) ||
                        !Catalog.Groups.Any(group => group.Id.Equals(
                            siblingGroupId, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    StoreMonsterScale(siblingGroupId, Config.GetSelection(siblingGroupId), normalized);
                }
            }

            if (save)
            {
                Config.Save(ConfigPath);
            }

            return normalized;
        }
    }

    private static void StoreMonsterScale(string groupId, string optionId, float normalized)
    {
        if (Mathf.IsEqualApprox(normalized, 1f))
        {
            if (Config.MonsterScales.TryGetValue(groupId, out var existing))
            {
                existing.Remove(optionId);
                if (existing.Count == 0)
                {
                    Config.MonsterScales.Remove(groupId);
                }
            }
            return;
        }

        if (!Config.MonsterScales.TryGetValue(groupId, out var options))
        {
            options = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
            Config.MonsterScales[groupId] = options;
        }

        options[optionId] = normalized;
    }

    /// <summary>
    /// Reserved entry that remembers a group's last model/health-bar size and position. Per-skin
    /// entries still win when present; this one is what survives a skin switch and a reload.
    /// </summary>
    private const string GroupTransformMemoryKey = "__group_default__";

    public static CharacterCombatTransform GetCharacterCombatTransform(
        string groupId,
        string? optionId = null)
    {
        lock (Sync)
        {
            var selectedOptionId = optionId ?? Config.GetSelection(groupId);
            if (!Config.CharacterCombatTransforms.TryGetValue(groupId, out var options))
            {
                return new CharacterCombatTransform();
            }

            // Per-skin entry first, then the group-level memory. Without the fallback, switching to a
            // skin that was never adjusted snapped the model back to the default size/position.
            if (options.TryGetValue(selectedOptionId, out var value))
            {
                return NormalizeCharacterCombatTransform(value);
            }

            return options.TryGetValue(GroupTransformMemoryKey, out var remembered)
                ? NormalizeCharacterCombatTransform(remembered)
                : new CharacterCombatTransform();
        }
    }

    internal static IReadOnlyDictionary<string, CharacterCombatTransform>
        GetSessionCharacterCombatTransforms(string groupId, string optionId)
    {
        lock (Sync)
        {
            var result = new Dictionary<string, CharacterCombatTransform>(
                StringComparer.OrdinalIgnoreCase)
            {
                [groupId] = GetCharacterCombatTransform(groupId, optionId)
            };
            var companionPrefix = groupId + "::companion::";
            foreach (var pair in Config.CharacterCombatTransforms.Where(pair =>
                         pair.Key.StartsWith(companionPrefix, StringComparison.OrdinalIgnoreCase)))
            {
                if (pair.Value.TryGetValue(optionId, out var value))
                {
                    result[pair.Key] = NormalizeCharacterCombatTransform(value);
                }
            }
            return result;
        }
    }

    public static CharacterCombatTransform SetCharacterCombatTransform(
        string groupId,
        string optionId,
        CharacterCombatTransform value,
        bool save = true)
    {
        lock (Sync)
        {
            var normalized = NormalizeCharacterCombatTransform(value);
            if (IsDefaultCharacterCombatTransform(normalized))
            {
                if (Config.CharacterCombatTransforms.TryGetValue(groupId, out var existing))
                {
                    existing.Remove(optionId);
                    // A full reset must forget the group-level memory as well. Keeping it made the
                    // read fall back to the last numbers, so the sliders jumped to 100%/%0 while the
                    // model kept its old size and position — the "重置按钮没反应" report.
                    existing.Remove(GroupTransformMemoryKey);
                    if (existing.Count == 0)
                    {
                        Config.CharacterCombatTransforms.Remove(groupId);
                    }
                }
            }
            else
            {
                if (!Config.CharacterCombatTransforms.TryGetValue(groupId, out var options))
                {
                    options = new Dictionary<string, CharacterCombatTransform>(StringComparer.OrdinalIgnoreCase);
                    Config.CharacterCombatTransforms[groupId] = options;
                }

                options[optionId] = normalized;
                // Remember the last used numbers for the whole group as well, so switching to another
                // skin, reloading the save and starting the next combat all keep the same size and
                // position instead of falling back to the default.
                options[GroupTransformMemoryKey] = normalized;
            }

            if (save)
            {
                Config.Save(ConfigPath);
            }

            return normalized;
        }
    }

    internal static CharacterCombatTransform NormalizeCharacterCombatTransform(
        CharacterCombatTransform value) =>
        new CharacterCombatTransform(
            Mathf.Clamp(
                Mathf.Round(value.Scale / CharacterScaleStep) * CharacterScaleStep,
                MinimumCharacterScale,
                MaximumCharacterScale),
            Mathf.Clamp(
                Mathf.Round(value.OffsetX / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            Mathf.Clamp(
                Mathf.Round(value.OffsetY / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset))
        {
            HealthBarScale = Mathf.Clamp(
                Mathf.Round(value.HealthBarScale / CharacterScaleStep) * CharacterScaleStep,
                MinimumCharacterScale,
                MaximumCharacterScale),
            HealthBarOffsetX = Mathf.Clamp(
                Mathf.Round(value.HealthBarOffsetX / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            HealthBarOffsetY = Mathf.Clamp(
                Mathf.Round(value.HealthBarOffsetY / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            HealthBarFollowsModelScale = value.HealthBarFollowsModelScale,
            HealthBarFollowsModelMovement = value.HealthBarFollowsModelMovement,
            IntentScale = Mathf.Clamp(
                Mathf.Round(value.IntentScale / CharacterScaleStep) * CharacterScaleStep,
                MinimumCharacterScale,
                MaximumCharacterScale),
            IntentOffsetX = Mathf.Clamp(
                Mathf.Round(value.IntentOffsetX / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            IntentOffsetY = Mathf.Clamp(
                Mathf.Round(value.IntentOffsetY / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            IntentFollowsModelScale = value.IntentFollowsModelScale,
            IntentFollowsModelMovement = value.IntentFollowsModelMovement,
            SelectionReticleScale = Mathf.Clamp(
                Mathf.Round(value.SelectionReticleScale / CharacterScaleStep) * CharacterScaleStep,
                MinimumCharacterScale,
                MaximumCharacterScale),
            SelectionReticleOffsetX = Mathf.Clamp(
                Mathf.Round(value.SelectionReticleOffsetX / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            SelectionReticleOffsetY = Mathf.Clamp(
                Mathf.Round(value.SelectionReticleOffsetY / CharacterOffsetStep) * CharacterOffsetStep,
                MinimumCharacterOffset,
                MaximumCharacterOffset),
            SelectionReticleFollowsModelScale = value.SelectionReticleFollowsModelScale,
            SelectionReticleFollowsModelMovement = value.SelectionReticleFollowsModelMovement
        };

    private static bool IsDefaultCharacterCombatTransform(CharacterCombatTransform value) =>
        Mathf.IsEqualApprox(value.Scale, 1f) &&
        Mathf.IsZeroApprox(value.OffsetX) &&
        Mathf.IsZeroApprox(value.OffsetY) &&
        Mathf.IsEqualApprox(value.HealthBarScale, 1f) &&
        Mathf.IsZeroApprox(value.HealthBarOffsetX) &&
        Mathf.IsZeroApprox(value.HealthBarOffsetY) &&
        !value.HealthBarFollowsModelScale &&
        value.HealthBarFollowsModelMovement &&
        Mathf.IsEqualApprox(value.IntentScale, 1f) &&
        Mathf.IsZeroApprox(value.IntentOffsetX) &&
        Mathf.IsZeroApprox(value.IntentOffsetY) &&
        !value.IntentFollowsModelScale &&
        value.IntentFollowsModelMovement &&
        Mathf.IsEqualApprox(value.SelectionReticleScale, 1f) &&
        Mathf.IsZeroApprox(value.SelectionReticleOffsetX) &&
        Mathf.IsZeroApprox(value.SelectionReticleOffsetY) &&
        value.SelectionReticleFollowsModelScale &&
        value.SelectionReticleFollowsModelMovement;

    public static Texture2D GetSelectedRuntimeImageTexture(string groupId)
    {
        lock (Sync)
        {
            var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
            var selection = GetVisualSelection(groupId);
            var imagePath = catalog.GetRuntimeImagePath(groupId, selection) ??
                            throw new InvalidOperationException($"{groupId}/{selection} 没有独立图片资源。");
            var cacheKey = RuntimeResourceKey(groupId, "external-image:" + imagePath);
            if (RuntimeResourceCache.TryGetValue(cacheKey, out var cached) &&
                GodotObject.IsInstanceValid(cached) && cached is Texture2D cachedTexture)
            {
                return cachedTexture;
            }

            var image = LoadRuntimeImage(imagePath);
            var texture = ImageTexture.CreateFromImage(image);
            RuntimeResourceCache[cacheKey] = texture;
            return texture;
        }
    }

    private static Image LoadRuntimeImage(string imagePath)
    {
        var bytes = File.ReadAllBytes(imagePath);
        if (bytes.Length == 0)
        {
            throw new InvalidOperationException($"独立皮肤图片为空：{imagePath}");
        }

        var image = new Image();
        Error error;
        if (HasPrefix(bytes, [0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]))
        {
            error = image.LoadPngFromBuffer(bytes);
        }
        else if (HasPrefix(bytes, [0xff, 0xd8, 0xff]))
        {
            error = image.LoadJpgFromBuffer(bytes);
        }
        else if (bytes.Length >= 12 &&
                 HasPrefix(bytes, [(byte)'R', (byte)'I', (byte)'F', (byte)'F']) &&
                 bytes.AsSpan(8, 4).SequenceEqual([(byte)'W', (byte)'E', (byte)'B', (byte)'P']))
        {
            error = image.LoadWebpFromBuffer(bytes);
        }
        else
        {
            image.Dispose();
            image = Image.LoadFromFile(imagePath);
            if (image == null || image.IsEmpty())
            {
                throw new InvalidOperationException($"无法识别独立皮肤图片格式：{imagePath}");
            }

            return image;
        }

        if (error != Error.Ok || image.IsEmpty())
        {
            image.Dispose();
            throw new InvalidOperationException(
                $"无法解码独立皮肤图片（{error}）：{imagePath}");
        }

        return image;

        static bool HasPrefix(byte[] value, ReadOnlySpan<byte> prefix) =>
            value.Length >= prefix.Length && value.AsSpan(0, prefix.Length).SequenceEqual(prefix);
    }

    public static RuntimeMonsterVisualMode? GetSelectedRuntimeMonsterVisualMode(string groupId)
    {
        lock (Sync)
        {
            return Catalog?.GetRuntimeMonsterVisualMode(groupId, GetVisualSelection(groupId));
        }
    }

    public static AncientLayeredImageTextures? GetSelectedAncientLayeredImageTextures(string groupId)
    {
        lock (Sync)
        {
            var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
            var paths = catalog.GetAncientLayeredImagePaths(
                groupId,
                GetVisualSelection(groupId));
            if (paths == null)
            {
                return null;
            }

            var requestedPaths = new[]
                {
                    paths.Character,
                    paths.BackgroundCover,
                    paths.Mask,
                    paths.SleepingCharacter
                }
                .Where(path => path != null)
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            var resources = LoadRuntimeResources(groupId, requestedPaths);

            Texture2D Required(string path) =>
                resources.GetValueOrDefault(path) as Texture2D ??
                throw new InvalidOperationException($"先古图层资源不是贴图：{path}");
            Texture2D? Optional(string? path) =>
                path == null ? null : Required(path);

            return new AncientLayeredImageTextures(
                Required(paths.Character),
                Optional(paths.BackgroundCover),
                Optional(paths.Mask),
                Optional(paths.SleepingCharacter));
        }
    }

    public static IReadOnlyDictionary<string, PackedScene> LoadRuntimeScenes(
        string groupId,
        IReadOnlyCollection<string> scenePaths)
    {
        var resources = LoadRuntimeResources(
            groupId,
            scenePaths,
            includeProviderDependencies: true);
        // Runtime resource loading deliberately returns the requested roots together with every
        // discovered dependency so native resources (for example Spine skeleton data) can be
        // rebound after a hot swap. Only the caller-requested roots are scenes; attempting to cast
        // the dependency textures/materials as PackedScene makes every resource-backed preview
        // fail while image-only providers appear to work.
        return scenePaths
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                scenePath => scenePath,
                scenePath => resources.GetValueOrDefault(scenePath) as PackedScene ??
                             throw new InvalidOperationException(
                                 $"独立皮肤资源不是场景：{scenePath}"),
                StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, Resource> LoadRuntimeResources(
        string groupId,
        IReadOnlyCollection<string> resourcePaths,
        bool includeProviderDependencies = false)
    {
        lock (Sync)
        {
            return LoadRuntimeResourcesForSelection(
                groupId,
                GetVisualSelection(groupId),
                resourcePaths,
                includeProviderDependencies);
        }
    }

    private static IReadOnlyDictionary<string, Resource> LoadRuntimeResourcesForSelection(
        string groupId,
        string selection,
        IReadOnlyCollection<string> resourcePaths,
        bool includeProviderDependencies)
    {
        _ = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
        if (TryGetCachedRuntimeResources(
                groupId,
                selection,
                resourcePaths,
                out var cached))
        {
            return cached;
        }

        // Keep the fast cache path above, but do the first load through the callback form so
        // callers that need to instantiate a PackedScene can run that instantiation while
        // the temporary dependency pack is still mounted.
        return WithRuntimeResourcesForSelection(
            groupId,
            selection,
            resourcePaths,
            resources => resources,
            includeProviderDependencies,
            takeOverCanonicalPaths: false);
    }

    /// <summary>
    /// Loads a selected skin's resources and executes <paramref name="callback">callback</paramref>
    /// before the temporary resource overlay is restored.  A PackedScene is not fully resolved
    /// when ResourceLoader.Load returns: its external resources are often looked up when the
    /// scene is instantiated.  Keeping this operation inside the mount scope prevents a hot
    /// character-select switch from binding a skeleton or layout resource from the previously
    /// selected skin.
    /// </summary>
    public static T WithRuntimeResources<T>(
        string groupId,
        IReadOnlyCollection<string> resourcePaths,
        Func<IReadOnlyDictionary<string, Resource>, T> callback,
        bool includeProviderDependencies = false,
        bool takeOverCanonicalPaths = false)
    {
        ArgumentNullException.ThrowIfNull(callback);

        lock (Sync)
        {
            var selection = GetVisualSelection(groupId);
            return WithRuntimeResourcesForSelection(
                groupId,
                selection,
                resourcePaths,
                callback,
                includeProviderDependencies,
                takeOverCanonicalPaths);
        }
    }

    private static T WithRuntimeResourcesForSelection<T>(
        string groupId,
        string selection,
        IReadOnlyCollection<string> resourcePaths,
        Func<IReadOnlyDictionary<string, Resource>, T> callback,
        bool includeProviderDependencies,
        bool takeOverCanonicalPaths)
    {
        var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
        var loadStarted = Stopwatch.GetTimestamp();
        var prepared = GetOrPrepareRuntimeOverlay(
            catalog,
            groupId,
            selection,
            resourcePaths,
            includeProviderDependencies);
        if (prepared.OverlayPath != null &&
            !ProjectSettings.LoadResourcePack(prepared.OverlayPath, replaceFiles: true))
        {
            throw new InvalidOperationException("Godot 拒绝加载已准备的独立皮肤场景资源包。");
        }

        if (prepared.CanonicalDependencyPaths.Count > 0)
        {
            if (!RuntimeCanonicalDependencyPaths.TryGetValue(groupId, out var trackedPaths))
            {
                trackedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                RuntimeCanonicalDependencyPaths[groupId] = trackedPaths;
            }

            trackedPaths.UnionWith(prepared.CanonicalDependencyPaths);
        }

        var reused = TryGetRuntimeResourceBundle(prepared.Key, out var resources);
        if (!reused)
        {
            resources = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in prepared.ResourcePaths)
            {
                var resource = ResourceLoader.Load<Resource>(
                    pair.Value,
                    null,
                    ResourceLoader.CacheMode.IgnoreDeep);
                if (resource == null)
                {
                    throw new InvalidOperationException($"无法加载独立皮肤资源：{pair.Value}");
                }

                resources[pair.Key] = resource;
                RuntimeResourceCache[
                    RuntimeResourceKey(groupId, selection, pair.Key)] = resource;
            }

            RuntimeResourceBundles[prepared.Key] = new RuntimeResourceBundleState(resources);
        }

        T callbackResult;
        IDisposable? canonicalOwnership = null;
        using var manualModeScope = ManualCharacterVariantBridge.BeginScope(groupId);
        try
        {
            if (takeOverCanonicalPaths)
            {
                canonicalOwnership = BeginCanonicalRuntimeResourceOwnership(
                    groupId,
                    prepared.ResourcePaths,
                    resources);
            }

            callbackResult = callback(resources);
        }
        finally
        {
            canonicalOwnership?.Dispose();
            if (prepared.RestoreGroups.Count > 0)
            {
                // Binary resources cannot rewrite all of their internal paths. Restore only
                // the other catalog groups that the temporary dependency pack actually
                // touched; rebuilding every skin group here caused a full localization reload
                // on the first character click.
                MountOverlay(prepared.RestoreGroups);
            }
        }

        var elapsedMs = Stopwatch.GetElapsedTime(loadStarted).TotalMilliseconds;
        ModLog.Info(
            $"已{(reused ? "复用" : "加载")} {groupId} 的 {resources.Count} 个独立资源；" +
            $"运行包={prepared.FileCount} 个文件/{prepared.FileSize / 1024d:F1} KiB，" +
            $"耗时={elapsedMs:F1} ms：{prepared.AliasToken}");
        return callbackResult;
    }

    private static IDisposable BeginCanonicalRuntimeResourceOwnership(
        string groupId,
        IReadOnlyDictionary<string, string> resourcePaths,
        IReadOnlyDictionary<string, Resource> resources)
    {
        var restoreOwnership =
            CharacterCombatSceneInstantiationPolicy.ShouldRestoreCanonicalOwnership(
                MultiplayerSkinSync.GetScopedSelection(groupId),
                Config.GetSelection(groupId));
        var ownedResources = new List<CanonicalRuntimeResourceOwnershipEntry>(resources.Count);
        foreach (var pair in resources)
        {
            var canonicalPath = pair.Key;
            var selectedResource = pair.Value;
            Resource? previousResource = null;
            if (ResourceLoader.HasCached(canonicalPath))
            {
                var cached = ResourceLoader.Load<Resource>(
                    canonicalPath,
                    null,
                    ResourceLoader.CacheMode.Reuse);
                if (cached != null && !ReferenceEquals(cached, selectedResource))
                {
                    previousResource = cached;
                }
            }

            var aliasPath = resourcePaths.GetValueOrDefault(canonicalPath);
            if (string.IsNullOrWhiteSpace(aliasPath))
            {
                aliasPath = selectedResource.ResourcePath;
            }

            selectedResource.TakeOverPath(canonicalPath);
            ownedResources.Add(new CanonicalRuntimeResourceOwnershipEntry(
                canonicalPath,
                aliasPath,
                selectedResource,
                previousResource));
        }

        return restoreOwnership
            ? new CanonicalRuntimeResourceOwnershipScope(ownedResources)
            : NoopDisposable.Instance;
    }

    private static PreparedRuntimeOverlay GetOrPrepareRuntimeOverlay(
        SkinCatalog catalog,
        string groupId,
        string selection,
        IReadOnlyCollection<string> resourcePaths,
        bool includeProviderDependencies,
        bool isolateRelicCanonicalPaths = false)
    {
        var normalizedPaths = resourcePaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // Appearance variants of one package ship exactly the same files; only the pack's own settings
        // value differs. Keying the overlay by the variant id rebuilt a 60+ file / 20+ MiB runtime pack
        // on every hotkey press (~600 ms hitch), so the overlay is always prepared for the family base.
        if (ProviderSettingsVariantOptions.ContractOf(selection) != null)
        {
            selection = ProviderSettingsVariantOptions.FamilyBaseId(selection);
        }
        var reuseMountedPrivateDependencies =
            !isolateRelicCanonicalPaths &&
            catalog.IsRuntimeProviderOption(groupId, selection) &&
            catalog.ProviderRequiresCoherentRuntimePackage(selection) &&
            Config.GetSelection(groupId).Equals(
                selection,
                StringComparison.OrdinalIgnoreCase);
        var key = RuntimeOverlayKey(
            groupId,
            selection,
            normalizedPaths,
            includeProviderDependencies,
            reuseMountedPrivateDependencies,
            isolateRelicCanonicalPaths);
        if (PreparedRuntimeOverlays.TryGetValue(key, out var cached) &&
            (cached.OverlayPath == null || File.Exists(cached.OverlayPath)))
        {
            LastPreparedRuntimeOverlays[groupId] = cached;
            return cached;
        }

        RuntimeResourceBundles.Remove(key);

        var generation = ++_overlayGeneration;
        var aliasToken = $"{_sessionId}/{generation:D3}";
        var overlay = isolateRelicCanonicalPaths
            ? catalog.BuildIsolatedRelicResourceOverlay(
                groupId,
                selection,
                normalizedPaths,
                aliasToken)
            : catalog.BuildRuntimeResourceOverlay(
                groupId,
                selection,
                normalizedPaths,
                aliasToken,
                includeProviderDependencies,
                reuseMountedPrivateDependencies);
        var restoreGroups = catalog.GetRuntimeDependencyRestoreGroups(
            groupId,
            overlay.CanonicalDependencyPaths);
        string? overlayPath = null;
        long overlaySize = 0;
        if (overlay.Files.Count > 0)
        {
            var directory = PreparedRuntimeOverlayDirectory();
            overlayPath = System.IO.Path.Combine(directory, $"{generation:D3}.pck");
            SkinChangerPaths.WriteCache(overlayPath, () => PckArchive.Write(overlayPath, overlay.Files));
            overlaySize = new FileInfo(overlayPath).Length;
        }

        var prepared = new PreparedRuntimeOverlay(
            key,
            aliasToken,
            overlayPath,
            new Dictionary<string, string>(
                overlay.ResourcePaths,
                StringComparer.OrdinalIgnoreCase),
            overlay.CanonicalDependencyPaths.ToHashSet(StringComparer.OrdinalIgnoreCase),
            restoreGroups.ToHashSet(StringComparer.OrdinalIgnoreCase),
            overlay.Files.Count,
            overlaySize);
        PreparedRuntimeOverlays[key] = prepared;
        LastPreparedRuntimeOverlays[groupId] = prepared;
        return prepared;
    }

    /// <summary>
    /// The private alias this group's most recent runtime overlay already prepared for a canonical
    /// resource, or null when the current selection does not isolate that resource. Callers use it
    /// to build the game's own copy of a scene while an event pack's art is mounted on top of the
    /// canonical path (the fake merchant fight's "游戏默认" background).
    /// </summary>
    public static string? TryGetIsolatedRuntimeResourcePath(string groupId, string canonicalPath)
    {
        if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(canonicalPath))
        {
            return null;
        }

        lock (Sync)
        {
            if (!LastPreparedRuntimeOverlays.TryGetValue(groupId, out var prepared) ||
                !prepared.ResourcePaths.TryGetValue(canonicalPath, out var alias) ||
                string.IsNullOrWhiteSpace(alias) ||
                alias.Equals(canonicalPath, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return ResourceLoader.Exists(alias) ? alias : null;
        }
    }

    private static bool TryGetRuntimeResourceBundle(
        string key,
        out Dictionary<string, Resource> resources)
    {
        if (RuntimeResourceBundles.TryGetValue(key, out var bundle) &&
            bundle.Resources.Values.All(resource =>
                GodotObject.IsInstanceValid(resource)))
        {
            resources = bundle.Resources;
            return true;
        }

        RuntimeResourceBundles.Remove(key);
        resources = null!;
        return false;
    }

    private static bool TryGetCachedRuntimeResources(
        string groupId,
        string selection,
        IReadOnlyCollection<string> resourcePaths,
        out IReadOnlyDictionary<string, Resource> resources)
    {
        var loaded = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
        foreach (var resourcePath in resourcePaths)
        {
            var cacheKey = RuntimeResourceKey(groupId, selection, resourcePath);
            if (!RuntimeResourceCache.TryGetValue(cacheKey, out var cached) ||
                !GodotObject.IsInstanceValid(cached))
            {
                resources = null!;
                return false;
            }

            loaded[resourcePath] = cached;
        }

        resources = loaded;
        return true;
    }

    private static void RestoreSelection(string key, string? previous, bool hadPrevious)
    {
        if (hadPrevious && previous != null)
        {
            Config.Selections[key] = previous;
        }
        else
        {
            Config.Selections.Remove(key);
        }
    }

    private static void TryRestoreOverlay(string groupId, bool cardOverlay)
    {
        TryRestoreOverlay(
            new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId },
            cardOverlay);
    }

    private static void TryRestoreOverlay(IReadOnlySet<string> groups, bool cardOverlay)
    {
        try
        {
            if (cardOverlay)
            {
                MountCardOverlay(groups);
            }
            else
            {
                MountOverlay(groups);
            }
        }
        catch (Exception restoreException)
        {
            ModLog.Error($"回滚 {string.Join(", ", groups)} 的皮肤覆盖失败：{restoreException}");
        }
    }

    [ThreadStatic]
    private static int _mountOverlayDepth;

    [ThreadStatic]
    private static HashSet<string>? _pendingRemountGroups;

    /// <summary>
    /// Set when a previously promoted full package can no longer be trusted to stay out of the
    /// way: Godot cannot unload a mounted PCK, so after a cross-group provider is deselected the
    /// only reliable way to make every canonical path point at the current selection again is a
    /// rebuild of **all** groups.
    /// </summary>
    private static bool _needsFullOverlayReassert;

    internal static void RequestFullOverlayReassert(string reason)
    {
        if (_needsFullOverlayReassert)
        {
            return;
        }

        _needsFullOverlayReassert = true;
        ModLog.Info($"外观一致性保护：下一次重建将覆盖全部分组（原因：{reason}）。");
    }

    // F29：挂载过程中框架/资源回调可能同线程再次触发重建。递归重建会逐层同步写 overlay 包
    //（IO 风暴，表现为选人界面长时间卡死）直至栈溢出闪退。重入请求改为合并到最外层，
    // 外层返回后用最终状态循环补一轮，既断开递归也不丢任何分组的更新。
    private static void MountOverlay(IReadOnlySet<string> groups)
    {
        var depth = ++_mountOverlayDepth;
        try
        {
            if (depth > 1)
            {
                var pending = _pendingRemountGroups ??=
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var group in groups)
                {
                    pending.Add(group);
                }

                ModLog.Warn(
                    $"外观覆盖重建被同线程重入（深度={depth}），已合并到最外层稍后统一重建：分组={string.Join("、", groups)}。");
                return;
            }

            var current = new HashSet<string>(groups, StringComparer.OrdinalIgnoreCase);
            if (_needsFullOverlayReassert && Catalog != null)
            {
                _needsFullOverlayReassert = false;
                foreach (var group in Catalog.Groups)
                {
                    current.Add(group.Id);
                }
            }

            for (var round = 0; round < 8; round++)
            {
                MountOverlayCore(current);
                var queued = _pendingRemountGroups;
                if (queued == null || queued.Count == 0)
                {
                    return;
                }

                current = new HashSet<string>(queued, StringComparer.OrdinalIgnoreCase);
                queued.Clear();
                ModLog.Info($"合并重入请求后补一轮外观覆盖重建：{current.Count} 个分组。");
            }

            var leftover = _pendingRemountGroups;
            if (leftover is { Count: > 0 })
            {
                // This is a real failure, not a throttling detail: the selection is already saved
                // in the config while the mounted overlay keeps the previous skin until a later
                // mount. Log it as an error so the "selected skin does not show up" reports leave
                // a trace instead of looking like a successful switch.
                ModLog.Error(
                    "重入重建连续超过 8 轮，已放弃余下请求；这些分组的新选择要等下一次外观重建才生效：" +
                    $"分组={string.Join("、", leftover)}。");
                leftover.Clear();
            }
        }
        finally
        {
            _mountOverlayDepth--;
        }
    }

    private static void MountOverlayCore(IReadOnlySet<string> groups)
    {
        var totalStarted = Stopwatch.GetTimestamp();
        var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
        // F29 取证：与结尾的「已挂载」成对，卡死时可分辨停在目录构建、写包还是挂载阶段。
        ModLog.Info($"开始重建外观覆盖：{groups.Count} 个分组。");
        // 临时性能探针：把这次重建拆成阶段，找到"到底是哪一步在卡"。
        // 玩家反馈切角色/切皮肤要等很久，日志里只有「目录/资源包/本地化」三个大块不够用。
        var marks = new List<(string Name, double Ms)>();
        var markWatch = Stopwatch.StartNew();
        void Mark(string name)
        {
            if (!PerfTrace.Enabled)
            {
                return;
            }

            marks.Add((name, markWatch.Elapsed.TotalMilliseconds));
            markWatch.Restart();
        }

        RefreshScopedMonsterSelectionSnapshot(catalog);
        var effectiveSelections = GetVisualSelections();
        Mark("选择快照");
        if (AppearanceDiagnostics.Enabled)
        {
            var overridden = effectiveSelections
                .Where(pair =>
                {
                    var stored = Config.GetSelection(pair.Key);
                    return !stored.Equals(pair.Value, StringComparison.OrdinalIgnoreCase);
                })
                .Select(pair =>
                    $"{pair.Key}: 配置={Config.GetSelection(pair.Key)} 生效={pair.Value}")
                .ToArray();
            AppearanceDiagnostics.Log(
                "外观重建",
                $"分组={groups.Count} 生效覆盖={effectiveSelections.Count} " +
                (overridden.Length == 0
                    ? "生效=配置"
                    : "与配置不同: " + string.Join("；", overridden)),
                150);
        }

        FrameworkCompatibilityLayer.SynchronizeSelections(catalog, effectiveSelections);
        var activeRuntimeProviders = GetActiveRuntimeProviders(catalog);
        Mark("行为提供者筛选");
        ManagedSkinModLoader.ConfigureRunEnvironmentProviders(
            _runtimeProviderBehaviorScope?.RunEnvironmentProviderIds ?? []);
        // Provider callbacks must be gone before a baseline replacement pack is mounted. Otherwise
        // a stale AssetCache/TakeOverPath callback can immediately reclaim the path being restored.
        ManagedSkinModLoader.DeactivateProvidersExcept(activeRuntimeProviders);
        EnsureScopedRuntimeProviderResourcesMounted(catalog, activeRuntimeProviders);
        Mark("停用未选中补丁");
        // Native framework initializers register author settings (including persisted values).
        // Read them before building the correction overlay, not one selection too late.
        ManagedSkinModLoader.ActivateSelectedProviders(
            activeRuntimeProviders.Where(FrameworkRegistryCooperation.IsNativeProvider));
        Mark("激活原生提供者");
        var largeProviderMountPlan = MountSelectedLargeRuntimeProviderPacks(catalog, groups);
        Mark("提权完整包");
        var promotedPackResourcePaths = largeProviderMountPlan.PromotedPackPaths
            .Where(MountedLargeRuntimeProviderPacks.ContainsKey)
            .SelectMany(path => MountedLargeRuntimeProviderPacks[path].ResourcePaths)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var promotedPackRestoreGroups = catalog.GetRuntimeDependencyRestoreGroups(
            string.Empty,
            promotedPackResourcePaths);
        var overlayGroups = groups
            .Union(promotedPackRestoreGroups)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var buildStarted = Stopwatch.GetTimestamp();
        var staleCanonicalPaths = groups
            .Where(RuntimeCanonicalDependencyPaths.ContainsKey)
            .SelectMany(group => RuntimeCanonicalDependencyPaths[group])
            .Concat(promotedPackResourcePaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        // 进入对局、进战斗、火堆、回到主菜单都会用同一组选择再挂一次覆盖包。以前每次都从头
        // 组装一遍（本机实测「目录=1.9 s」× 每次场景切换），但输入完全没变，产物也一样。
        // 这里按"会影响产物的全部输入"做签名复用：签名不变就直接用上次组装好的文件表，
        // 只重做挂载（挂载命中缓存时是毫秒级）。任何一项输入变化都会自然落到重建分支。
        var overlayPlanSignature = BuildOverlayPlanSignature(
            catalog,
            overlayGroups,
            effectiveSelections,
            staleCanonicalPaths,
            largeProviderMountPlan.PromotedPackPaths);
        Dictionary<string, ResourceFile> files;
        var overlayPlanRebuilt = false;
        if (_cachedOverlayPlan is { } cached && cached.Signature.Equals(overlayPlanSignature))
        {
            files = cached.Files;
            Mark("覆盖计划复用（0 组装）");
        }
        else
        {
            overlayPlanRebuilt = true;
            files = catalog.BuildBaselineDependencyOverlay(staleCanonicalPaths);
            // 提权包的路径必须按**游戏原包**还原。BuildBaselineDependencyOverlay 走的是通用基线
            // 索引（提供者包也在里面），会给 midori_black 的 osty.tscn 壳「恢复」成它自己那一份，
            // 结果没选这个提供者的分组（奥斯提选「撑在地上的鸡煲」）拿到的仍然是猫。
            foreach (var pair in catalog.BuildGameBaselineOverlay(promotedPackResourcePaths))
            {
                files[pair.Key] = pair.Value;
            }

            Mark($"基线依赖覆盖（{staleCanonicalPaths.Length} 个待恢复路径）");
            var selectedOverlay = catalog.BuildOverlay(effectiveSelections, overlayGroups);
            Mark($"选中项覆盖（{overlayGroups.Count} 个分组 → {selectedOverlay.Count} 个文件）");
            // The selected complete PCK is already mounted directly below this correction overlay.
            // A vanilla canonical resource in the correction overlay would hide a selected provider's
            // <canonical path>.remap and produce a mixed skin (for example, a selected merchant body
            // with the vanilla merchant hand). Remove only canonical paths whose provider remap is in
            // the selected overlay. Other files from a multi-group PCK must stay on the baseline.
            var selectedProviderOverlayPaths = selectedOverlay
                .Where(pair =>
                    largeProviderMountPlan.PromotedPackPaths.Contains(pair.Value.ArchivePath))
                .Select(pair => pair.Key);
            foreach (var ownedPath in
                     PromotedPackOverlayPolicy.FindBaselinePathsShadowingSelectedRemaps(
                         files.Keys,
                         selectedProviderOverlayPaths))
            {
                files.Remove(ownedPath);
            }

            foreach (var selectedFile in selectedOverlay)
            {
                if (largeProviderMountPlan.PromotedPackPaths.Contains(selectedFile.Value.ArchivePath) &&
                    selectedFile.Key.Equals(
                        selectedFile.Value.Path,
                        StringComparison.OrdinalIgnoreCase) &&
                    IsDirectProviderPayloadPath(selectedFile.Key))
                {
                    // Imported payloads make up almost all of a large animated PCK. The selected pack
                    // was just remounted at highest priority, so copying the identical payload into the
                    // correction overlay only duplicates it. Keep scenes, imports, localization and
                    // mapped/takeover paths in the small overlay so normal refresh and ownership logic
                    // still observes them.
                    files.Remove(selectedFile.Key);
                    continue;
                }

                // Baseline dependency restoration must happen before the current selection is applied.
                // The current provider therefore remains authoritative for paths it still owns.
                files[selectedFile.Key] = selectedFile.Value;
            }

            // 提权包是整包置顶的，它的资源表里往往还带着**别的分组**的规范路径
            // （midori_black.pck 里的 res://scenes/creature_visuals/osty.tscn 就是那只猫）。
            // 上面的选中项覆盖会把这些路径重新写回文件表，于是没选这个提供者的分组
            // （例如 奥斯提=「撑在地上的鸡煲」）在局内重建身体时又拿回了别人的皮肤。
            // 这里在选中项之后按"该路径归属分组当前选中的提供者"再判一次：不归它，就按
            // 游戏原包还原。归属分组自己仍选着这个提供者时不动（它的选中项仍然权威）。
            var foreignPromotedPaths = new List<string>();
            foreach (var (promotedPath, promotedFile) in files)
            {
                if (!largeProviderMountPlan.PromotedPackPaths.Contains(promotedFile.ArchivePath) ||
                    !catalog.IsBaseGameResource(promotedPath))
                {
                    continue;
                }

                var ownerGroupId = catalog.FindGroupIdForResourcePath(promotedPath);
                if (ownerGroupId == null)
                {
                    continue;
                }

                var ownerSelection =
                    effectiveSelections.GetValueOrDefault(ownerGroupId) ?? "__base__";
                var ownerProviderId = catalog.ResolveVisualProviderId(ownerSelection);
                if (largeProviderMountPlan.PromotedProviderIds.Contains(ownerProviderId))
                {
                    continue;
                }

                foreignPromotedPaths.Add(promotedPath);
            }

            if (foreignPromotedPaths.Count > 0)
            {
                foreach (var pair in catalog.BuildGameBaselineOverlay(foreignPromotedPaths))
                {
                    files[pair.Key] = pair.Value;
                }

                ModLog.Info(
                    $"[提权] 已按游戏原包还原 {foreignPromotedPaths.Count} 条其它分组的规范路径：" +
                    string.Join("、", foreignPromotedPaths.Take(6)));
                Mark($"提权包外来路径还原（{foreignPromotedPaths.Count} 个路径）");
            }

            _cachedOverlayPlan = new OverlayPlan(overlayPlanSignature, files);
            Mark("覆盖合并");
        }

        var buildElapsed = Stopwatch.GetElapsedTime(buildStarted);
        var mountStarted = Stopwatch.GetTimestamp();
        var mountedDeltaFiles = 0;
        try
        {
            // 只挂"和当前已经挂在栈顶的那一层不一样"的文件。整份覆盖包平均 630 MB，
            // 每换一次皮肤就整份重写、再整份挂上去，既慢（500 ms 写盘）又会把内存和
            // 文件句柄耗光（实测一个会话连切 20 次写了 11.8 GB 后闪退）。
            // 玩家绝大多数切换只改动几十个路径，剩下的直接复用先前已经挂好的层。
            mountedDeltaFiles = MountOverlayDelta(
                files,
                "visual",
                "Godot 拒绝加载生成的皮肤资源包。");
        }        catch (Exception exception)
        {
            // 覆盖包生成/挂载失败时保留上一次的外观继续玩，不要让一次写盘失败把整个
            // 初始化（或当前场景构建）带崩。下一次重建会重新尝试。
            ModLog.Error("本次外观覆盖没有挂上，保留上一次的外观：" +
                         exception.GetBaseException().Message);
        }
        foreach (var group in groups)
        {
            RuntimeCanonicalDependencyPaths.Remove(group);
        }
        var mountElapsed = Stopwatch.GetElapsedTime(mountStarted);
        Mark($"写包+挂载（{files.Count} 个文件，增量 {mountedDeltaFiles}）");
        if (overlayPlanRebuilt || mountedDeltaFiles > 0)
        {
            // 新层已经挂在栈顶，但游戏缓存里还是旧皮肤的场景（见方法注释）。计划重建时
            // 即使增量层没有新文件也要刷一次：这条路径可能由更早的一层一直提供着旧皮肤。
            RefreshCachedCreatureVisualScenes(files.Keys);
        }

        var localizationStarted = Stopwatch.GetTimestamp();
        RefreshLocalizationIfNeeded(files);
        var localizationElapsed = Stopwatch.GetElapsedTime(localizationStarted);
        Mark("本地化");

        // Register scripts and run third-party initializers only after every private scene, atlas,
        // imported payload and frame directory is visible at its original res:// path. Static
        // resource fields in provider assemblies are often initialized on their first type access.
        foreach (var group in catalog.Groups.Where(group => groups.Contains(group.Id)))
        {
            var selectedId = effectiveSelections.GetValueOrDefault(
                group.Id,
                SkinCatalog.BaseOptionId);
            if (catalog.IsRuntimeProviderOption(group.Id, selectedId) &&
                catalog.ProviderUsesManagedGodotScripts(selectedId))
            {
                // Register before a private PackedScene is instantiated. The compatibility patch
                // on Godot's path map makes this operation idempotent when the provider initializer
                // also calls LookupScriptsInAssembly (a common pattern in complete character packs).
                ManagedSkinModLoader.EnsureProviderGodotScripts(
                    catalog.ResolveVisualProviderId(selectedId));
            }
        }

        // A remote player may use a managed character provider that is not selected in our
        // persistent config. Its scripts still need registering before that player's isolated
        // scene is instantiated; registering is idempotent and does not choose the provider for
        // any local character.
        foreach (var providerId in activeRuntimeProviders.Where(
                     catalog.ProviderUsesManagedGodotScripts))
        {
            ManagedSkinModLoader.EnsureProviderGodotScripts(providerId);
        }

        foreach (var providerId in activeRuntimeProviders.Where(
                     catalog.ProviderUsesScopedMonsterRuntime))
        {
            ManagedSkinModLoader.EnsureScopedMonsterSelectionRouter(providerId);
        }
        // Install the per-monster selection router before invoking a provider initializer. Some
        // providers evaluate IsEnabled(profile) while constructing their runtime services; doing
        // this afterwards can leave a disabled profile cached as active (or vice versa) until the
        // next game launch.
        //
        // 启动时（还没有任何场景范围）屏幕上没有任何角色或怪物，因此只有"玩家下一步就要看到"
        // 的角色/随从外观需要立刻初始化；怪物、先古、事件、卡牌的提供者等主菜单出来以后在后台
        // 逐个初始化，进入它们所属的场景时场景范围也会再把它们同步激活一次。这一批工作以前
        // 全部压在这次重建上（本机 22 个提供者约 3.9 s），是"窗口出来了但主菜单很久不响应"
        // 的主要来源。有了真实场景范围之后不再拆分：可见提供者必须同步就位。
        // STS2SC_NO_DEFER=1 关掉拆分（复活老行为，用于排查"是不是延后初始化引起的"）。
        var deferNonFirstScreen = _runtimeProviderBehaviorScope == null &&
                                  !string.Equals(
                                      System.Environment.GetEnvironmentVariable("STS2SC_NO_DEFER"),
                                      "1",
                                      StringComparison.Ordinal);
        var firstScreenProviders = deferNonFirstScreen
            ? catalog.GetFirstScreenRuntimeProviders()
            : null;
        var activationSchedule = ProviderActivationSchedulePolicy.Plan(
            activeRuntimeProviders,
            providerId => firstScreenProviders?.Contains(providerId) ?? true,
            deferNonFirstScreen);
        ManagedSkinModLoader.ActivateSelectedProviders(activationSchedule.Immediate);
        DeferredProviderActivator.Schedule(activationSchedule.Deferred);
        Mark(activationSchedule.HasDeferred
            ? $"激活行为补丁（同步 {activationSchedule.Immediate.Count}，" +
              $"延后 {activationSchedule.Deferred.Count}）"
            : "激活行为补丁");
        // The fake merchant event picture belongs to an event-only pack that no selection mounts.
        // Mount it after every provider pack so a merchant skin's full PCK cannot shadow the tent
        // the event pack drew (that shadowing is what made the event snap back to vanilla).
        try
        {
            var eventArt = catalog.FakeMerchantEventPresentation();
            if (eventArt.Count > 0)
            {
                MountOverlayDelta(
                    eventArt.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
                    "fake_merchant_event",
                    "Godot 拒绝加载假商人事件背景资源包。");
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("挂载假商人事件背景失败：" + exception.GetBaseException().Message);
        }
        Mark("假商人事件背景");

        ScheduleRuntimeProviderPackWarm(catalog, activeRuntimeProviders, "当前场景");
        // Providers that expose their palette variants through their own settings only show the
        // selected variant while their setting says so. Every rebuild (startup, run start, skin
        // change) re-asserts the variant behind the current selection.
        try
        {
            STS2SkinChanger.Catalog.ProviderSettingsVariantOptions.ApplyActiveSelections(effectiveSelections);
        }
        catch (Exception exception)
        {
            ModLog.Warn("应用皮肤包的外观变体设置失败：" + exception.GetBaseException().Message);
        }
        Mark("预热/变体");
        // 把本次选中皮肤的场景排进后台预加载：进战斗/火堆时这些资源已经在缓存里，
        // 不会再出现"切场景先黑一下"的同步读盘尖峰。
        SkinPreloader.Schedule(catalog, effectiveSelections, groups);
        ModLog.Info(
            $"已挂载 {groups.Count} 个外观分组/{files.Count} 个文件；" +
            $"目录={buildElapsed.TotalMilliseconds:F1} ms，" +
            $"资源包={mountElapsed.TotalMilliseconds:F1} ms，" +
            $"本地化={localizationElapsed.TotalMilliseconds:F1} ms，" +
            $"总计={Stopwatch.GetElapsedTime(totalStarted).TotalMilliseconds:F1} ms。");
        if (PerfTrace.Enabled && marks.Count > 0)
        {
            // 一段式汇总，方便直接看出"这一次卡在谁身上"；单项超过阈值的会另外打 WARN。
            var total = marks.Sum(mark => mark.Ms);
            var detail = string.Join("，", marks
                .OrderByDescending(mark => mark.Ms)
                .Select(mark => $"{mark.Name}={mark.Ms:F0}ms"));
            var line = $"[性能] 外观重建合计 {total:F0} ms：{detail}";
            if (total >= PerfTrace.WarnMilliseconds * 2)
            {
                ModLog.Warn(line);
            }
            else
            {
                ModLog.Info(line);
            }

            foreach (var mark in marks.Where(mark => mark.Ms >= PerfTrace.WarnMilliseconds))
            {
                ModLog.Warn($"[性能] 慢阶段 {mark.Name} = {mark.Ms:F0} ms");
            }
        }
    }

    private static void RefreshScopedMonsterSelectionSnapshot(SkinCatalog catalog)
    {
        var selectedMonsterIdsByProvider = new Dictionary<string, IReadOnlyCollection<string>>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var providerId in catalog.GetSelectedScopedMonsterRuntimeProviders(
                     Config.Selections))
        {
            selectedMonsterIdsByProvider[providerId] = catalog
                .GetRuntimeProviderGroups(providerId)
                .Where(groupId => catalog.ResolveVisualProviderId(Config.GetSelection(groupId))
                    .Equals(providerId, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }

        ScopedMonsterSelections.Replace(selectedMonsterIdsByProvider);
    }

    private sealed class MountedRuntimeResourceScope(IReadOnlySet<string> restoreGroups, IDisposable? manualModeScope) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            lock (Sync)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                manualModeScope?.Dispose();
                try
                {
                    MountOverlay(restoreGroups);
                }
                catch (Exception exception)
                {
                    ModLog.Error($"恢复临时运行资源覆盖失败：{exception}");
                }
            }
        }
    }

    private sealed record CanonicalRuntimeResourceOwnershipEntry(
        string CanonicalPath,
        string AliasPath,
        Resource SelectedResource,
        Resource? PreviousResource);

    private sealed class CanonicalRuntimeResourceOwnershipScope(
        IReadOnlyList<CanonicalRuntimeResourceOwnershipEntry> entries) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            for (var index = entries.Count - 1; index >= 0; index--)
            {
                var entry = entries[index];
                try
                {
                    if (GodotObject.IsInstanceValid(entry.SelectedResource))
                    {
                        entry.SelectedResource.TakeOverPath(entry.AliasPath);
                    }

                    if (entry.PreviousResource != null &&
                        GodotObject.IsInstanceValid(entry.PreviousResource))
                    {
                        entry.PreviousResource.TakeOverPath(entry.CanonicalPath);
                    }
                }
                catch (Exception exception)
                {
                    ModLog.Warn(
                        $"恢复角色规范资源 {entry.CanonicalPath} 失败：{exception.Message}");
                }
            }
        }
    }

    private sealed class NoopDisposable : IDisposable
    {
        public static NoopDisposable Instance { get; } = new();

        public void Dispose()
        {
        }
    }

    private static void MountContextualFullRuntimeProviderPacks(
        SkinCatalog catalog,
        string selection)
    {
        if (!catalog.ProviderUsesFullRuntime(selection) ||
            catalog.IsFullRuntimeProviderFullySelected(selection, Config.Selections))
        {
            return;
        }

        // A multiplayer selection can differ from this client's persistent character selection.
        // Full-runtime character scenes often reference hundreds of imported frames through
        // prefix-compressed binary paths. The small alias overlay cannot discover or reproduce
        // that import map, so mount the already-installed provider PCK at full priority for the
        // lifetime of this creature-creation scope. The generated alias is mounted immediately
        // afterwards and remains the authoritative entry scene; MountedRuntimeResourceScope then
        // restores this client's configured provider/baseline resources.
        foreach (var resourcePackPath in catalog.GetProviderResourcePackPaths(selection))
        {
            var normalizedPath = System.IO.Path.GetFullPath(resourcePackPath);
            if (!ProjectSettings.LoadResourcePack(normalizedPath, replaceFiles: true))
            {
                throw new InvalidOperationException(
                    $"无法为联机玩家挂载 {selection} 的完整运行时资源包。");
            }

            ModLog.Info($"已为联机玩家临时挂载 {selection} 的完整运行时资源。");
        }
    }

    private static void EnsureScopedRuntimeProviderResourcesMounted(
        SkinCatalog catalog,
        IEnumerable<string> activeRuntimeProviders)
    {
        foreach (var providerId in activeRuntimeProviders
                     .Where(provider => catalog.ProviderUsesScopedMonsterRuntime(provider) ||
                                        catalog.ProviderUsesDirectCharacterRuntime(provider))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            foreach (var resourcePackPath in catalog.GetProviderResourcePackPaths(providerId))
            {
                var normalizedPath = System.IO.Path.GetFullPath(resourcePackPath);
                if (!MountedScopedRuntimeProviderPacks.Add(normalizedPath))
                {
                    continue;
                }

                // A scoped DLL can resolve effects, sounds or encounter backgrounds dynamically;
                // those paths cannot be discovered by walking references from the selected
                // skeleton alone. Mount its original PCK as a low-priority dependency namespace.
                // Canonical game paths remain owned by the game/current generated overlay because
                // replaceFiles is false, while provider-private res:// paths become available.
                if (!ProjectSettings.LoadResourcePack(normalizedPath, replaceFiles: false))
                {
                    MountedScopedRuntimeProviderPacks.Remove(normalizedPath);
                    throw new InvalidOperationException(
                        $"无法挂载 {providerId} 的运行时依赖资源包。");
                }

                ModLog.Info($"已低优先级挂载 {providerId} 的完整运行时资源。");
            }
        }
    }

    private static LargeProviderMountPlan MountSelectedLargeRuntimeProviderPacks(
        SkinCatalog catalog,
        IReadOnlySet<string> affectedGroups)
    {
        var selectedOptions = catalog.Groups
            .Select(group =>
            {
                var selection = GetVisualSelection(group.Id);
                var option = group.Options.FirstOrDefault(option =>
                    option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase) &&
                    option.IsRuntimeProvider);
                return (GroupId: group.Id, Option: option);
            })
            .Where(selection => selection.Option != null)
            .ToArray();
        var selectionSnapshot = selectedOptions.ToDictionary(
            selection => selection.GroupId,
            selection => selection.Option!.Id,
            StringComparer.OrdinalIgnoreCase);
        foreach (var group in catalog.Groups.Where(group => !selectionSnapshot.ContainsKey(group.Id)))
        {
            selectionSnapshot[group.Id] = GetVisualSelection(group.Id);
        }

        var directProviderPacks = selectedOptions
            .SelectMany(selection =>
            {
                var providerId = selection.Option!.EffectiveProviderId;
                var requiresCoherentPackage =
                    catalog.ProviderRequiresCoherentRuntimePackage(providerId);
                var companionOnly = catalog.IsCompanionOnlyProviderSelection(
                    providerId,
                    selectionSnapshot);
                return catalog.GetProviderResourcePackPaths(providerId)
                    .Select(System.IO.Path.GetFullPath)
                    .Where(path =>
                        requiresCoherentPackage ||
                        new FileInfo(path).Length >= DirectRuntimeProviderPackThresholdBytes)
                    .Select(path => (selection.GroupId, Path: path, CompanionOnly: companionOnly, ProviderId: providerId));
            })
            .ToArray();
        var selectedPackPaths = directProviderPacks
            .Select(pack => pack.Path)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        // A pack shared by a summon group and a real group (same provider) must stay promoted for
        // the real group; only packs used exclusively by summon selections are demoted.
        var companionOnlyPackPaths = directProviderPacks
            .Where(pack => pack.CompanionOnly)
            .Select(pack => pack.Path)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        companionOnlyPackPaths.ExceptWith(directProviderPacks
            .Where(pack => !pack.CompanionOnly)
            .Select(pack => pack.Path));
        var promotedPackPaths = directProviderPacks
            .Where(pack => affectedGroups.Contains(pack.GroupId))
            .Select(pack => pack.Path)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var packPath in selectedPackPaths)
        {
            if (companionOnlyPackPaths.Contains(packPath))
            {
                // A pack that was promoted while the owner character still used it stays on top of
                // Godot's pack stack forever (packs cannot be unloaded). Demoting it is only half
                // the fix: force one all-group rebuild so every canonical path is re-asserted by
                // the correction overlay.
                if (MountedLargeRuntimeProviderPacks.ContainsKey(packPath))
                {
                    RequestFullOverlayReassert(
                        $"{System.IO.Path.GetFileName(packPath)} 已不再用于角色本体，只用于随从");
                }

                // Private summon assets must be resolvable, but nothing in this pack may shadow a
                // canonical game/character path. `replaceFiles: false` registers the private
                // namespace without taking over existing resources (see the method's doc comment).
                if (!MountedCompanionOnlyProviderPacks.Add(packPath))
                {
                    continue;
                }

                if (!ProjectSettings.LoadResourcePack(packPath, replaceFiles: false))
                {
                    MountedCompanionOnlyProviderPacks.Remove(packPath);
                    ModLog.Warn(
                        "无法低优先级挂载随从皮肤包的完整资源：" +
                        System.IO.Path.GetFileName(packPath) + "；随从将使用资源校正包里的文件。");
                    continue;
                }

                ModLog.Info(
                    $"已低优先级挂载随从皮肤包 {System.IO.Path.GetFileName(packPath)}：" +
                    "该包只用于随从自己的私有资源，不会覆盖角色已选的皮肤。");
                continue;
            }

            var firstMount = !MountedLargeRuntimeProviderPacks.ContainsKey(packPath);
            if (!MountedLargeRuntimeProviderPacks.ContainsKey(packPath))
            {
                using var archive = PckArchive.Open(packPath);
                MountedLargeRuntimeProviderPacks[packPath] = new MountedProviderPackState(
                    archive.Paths.ToArray());
                promotedPackPaths.Add(packPath);
            }

            // The correction overlay already restores every selected canonical path. Re-loading
            // every unrelated 50-200 MiB character pack on each dropdown change was therefore
            // pure I/O and could freeze the menu for tens of seconds. Promote only a newly seen
            // pack or the provider belonging to the group that actually changed.
            if (!firstMount && !promotedPackPaths.Contains(packPath))
            {
                continue;
            }

            // Move the affected pack to the top of Godot's pack stack. A generated correction
            // overlay is mounted immediately afterwards to restore every card/game path that this
            // visual selection does not own.
            //
            // 注意：这里**不能**因为「本次会话已提权过」就跳过。校正覆盖层每次都挂在整包上面，
            // 它会把整包里的规范路径再写一遍；如果不再把整包压回栈顶，像 Orchis 的
            // 原版特效替换这类「靠整包压栈顶生效」的资源就会被游戏原版盖掉（1.0.30.195 的教训）。
            if (!ProjectSettings.LoadResourcePack(packPath, replaceFiles: true))
            {
                throw new InvalidOperationException(
                    $"无法直接挂载完整外观资源包：{System.IO.Path.GetFileName(packPath)}");
            }

            PromotedPackOrder.RemoveAll(path =>
                path.Equals(packPath, StringComparison.OrdinalIgnoreCase));
            PromotedPackOrder.Add(packPath);

            // 这个整包现在压在**所有**已挂载的覆盖层上面，它带的每条路径都重新由它提供。
            // 把归属记录清掉，接下来的校正覆盖包才会把这些路径重新写一遍（否则增量层会误以为
            // "栈顶还是原来那一份"，切走以后皮肤看起来没变）。
            foreach (var ownedPath in MountedLargeRuntimeProviderPacks[packPath].ResourcePaths)
            {
                MountedOverlayOwners.Remove(ownedPath);
            }

            ModLog.Info(
                $"已直接复用完整外观资源包 {System.IO.Path.GetFileName(packPath)}，" +
                "后续仅生成资源校正包。");
        }

        // 记住是哪些提供者的整包被提到了栈顶：覆盖包装配阶段要据此判断某条路径
        // 是不是"别人的提权包顺带带来的"，好把这部分按游戏原包还原回去。
        var promotedProviderIds = directProviderPacks
            .Where(pack => promotedPackPaths.Contains(pack.Path))
            .Select(pack => pack.ProviderId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        return new LargeProviderMountPlan(promotedPackPaths, promotedProviderIds);
    }

    private static readonly HashSet<string> MountedCompanionOnlyProviderPacks =
        new(StringComparer.OrdinalIgnoreCase);

    private static bool IsDirectProviderPayloadPath(string path) =>
        path.StartsWith("res://.godot/imported/", StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether a same-code-base re-export is activated alongside the selected pack.</summary>
    private const bool EnableSiblingVariantActivation = false;

    private static HashSet<string> GetActiveRuntimeProviders(SkinCatalog catalog)
    {
        var localSelections = GetVisualSelections();
        var selectionSets = new List<IReadOnlyDictionary<string, string>> { localSelections };
        selectionSets.AddRange(MultiplayerSkinSync.GetAvailableSelectionMaps());
        var selectedProviders = catalog.GetFullySelectedFullRuntimeProviders(localSelections)
            // 只在"是否激活运行时行为"这里放宽：骨妹皮肤是 Orchis 就激活它（背景动画），
            // 即使奥斯提选了别的皮肤。选项过滤/选择事务仍走严格判定。
            .Union(
                catalog.GetFullySelectedFullRuntimeProvidersIgnoringCompanions(localSelections),
                StringComparer.OrdinalIgnoreCase)
            .Union(
                catalog.GetSelectedInteractiveRuntimeProviders(localSelections),
                StringComparer.OrdinalIgnoreCase)
            .Union(
                catalog.GetSelectedDirectCharacterRuntimeProviders(localSelections),
                StringComparer.OrdinalIgnoreCase)
            .Union(
                catalog.GetSelectedScopedMonsterRuntimeProviders(localSelections),
                StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var selections in selectionSets.Skip(1))
        {
            selectedProviders.UnionWith(catalog.GetFullySelectedFullRuntimeProviders(selections));
            selectedProviders.UnionWith(catalog.GetFullySelectedFullRuntimeProvidersIgnoringCompanions(selections));
            selectedProviders.UnionWith(catalog.GetSelectedInteractiveRuntimeProviders(selections));
            selectedProviders.UnionWith(catalog.GetSelectedDirectCharacterRuntimeProviders(selections));
            selectedProviders.UnionWith(catalog.GetSelectedScopedMonsterRuntimeProviders(selections));
        }
        if (_runtimeProviderBehaviorScope is { } scope)
        {
            selectedProviders.UnionWith(scope.RunEnvironmentProviderIds);
        }

        // A pack chosen only through a summon group must not run its DLL. Skin packs are authored for
        // the character: Orchis Necrobinder Skin Mod's `Creature.CreateVisuals` postfix rewrites the
        // *Necrobinder player's* skeleton whenever it sees one, and its character-select / merchant /
        // rest-site patches do the same. Keeping that DLL active because the summon uses its Osty art
        // turned every Necrobinder skin into Orchis in every new battle, merchant and rest site.
        var companionOnlyProviders = new List<string>();
        foreach (var providerId in selectedProviders.ToArray())
        {
            if (!catalog.IsCompanionOnlyProviderSelection(providerId, localSelections))
            {
                continue;
            }

            selectedProviders.Remove(providerId);
            companionOnlyProviders.Add(providerId);
            ModLog.Info(
                $"提供者 {providerId} 只在随从分组上被选中：不激活它的行为补丁，避免它的角色级补丁" +
                "把主人本体改成同款皮肤（随从的外观仍由本 Mod 挂载的资源提供）。");
        }

        if (AppearanceDiagnostics.Enabled)
        {
            AppearanceDiagnostics.Log(
                "行为提供者",
                $"候选={string.Join(",", selectedProviders)} " +
                $"随从专用已排除={string.Join(",", companionOnlyProviders)}",
                200);
        }
        var candidates = selectedProviders.Select(providerId => new RuntimeProviderCandidate(
            providerId,
            catalog.GetRuntimeProviderGroups(providerId),
            catalog.ProviderUsesScopedMonsterRuntime(providerId)));
        var active = RuntimeProviderScopePolicy.SelectActiveProviders(
            candidates,
            _runtimeProviderBehaviorScope).ToHashSet(StringComparer.OrdinalIgnoreCase);
        // A same-ID variation cannot execute its own DLL, so the copy that owns the loaded assembly
        // has to keep running while the variation is on screen: the variation supplies the art, the
        // executable copy supplies the code (its merchant transform, for example). Without this the
        // variation is drawn by a skeleton the code never positioned.
        foreach (var providerId in active.ToArray())
        {
            var behaviorProviderId = ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);
            if (!behaviorProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase) &&
                ProviderAssembliesContain(behaviorProviderId))
            {
                active.Add(behaviorProviderId);
            }
            // The richer of two same-contract packages is activated by
            // ManagedSkinModLoader.ActivateSelectedProviders; adding the other one here was tried and
            // reverted (both cannot own the same assembly identity) — see the dual-pack issue doc.
        }

        return active;
    }

    private static bool ProviderAssembliesContain(string providerId) =>
        ManagedSkinModLoader.HasProviderAssembly(providerId);

    /// <summary>
    /// 后台延后初始化队列（<see cref="DeferredProviderActivator"/>）调用：替一个提供者补上
    /// "资源挂载 → 场景脚本注册 → 作者初始化器/行为补丁"这一串，等价于它本来在启动重建里
    /// 会拿到的那一份。返回 false 表示这一项已经不需要做（没有 DLL、已经激活、或目录已重载）。
    /// </summary>
    internal static bool TryActivateDeferredProvider(string providerId)
    {
        lock (Sync)
        {
            if (Catalog is not { } catalog ||
                string.IsNullOrWhiteSpace(providerId) ||
                !ManagedSkinModLoader.HasProviderAssembly(providerId))
            {
                return false;
            }

            if (ManagedSkinModLoader.IsProviderActive(
                    ManagedSkinModLoader.ResolveBehaviorProviderId(providerId)))
            {
                return false;
            }

            // 提供者的运行时资源包必须先可见：作者初始化器和它的静态字段会在第一次访问时
            // 直接解析 res:// 私有路径。与重建路径使用同一套幂等挂载。
            EnsureScopedRuntimeProviderResourcesMounted(catalog, [providerId]);
            if (catalog.ProviderUsesManagedGodotScripts(providerId))
            {
                ManagedSkinModLoader.EnsureProviderGodotScripts(providerId);
            }

            if (catalog.ProviderUsesScopedMonsterRuntime(providerId))
            {
                ManagedSkinModLoader.EnsureScopedMonsterSelectionRouter(providerId);
            }

            ManagedSkinModLoader.ActivateSelectedProviders([providerId]);
            // 初始化器可能把角色资源写进框架注册表；延后激活同样要拦一次，否则它会在此刻
            // 悄悄接管别的角色的外观。
            ManagedCharacterAssetRegistrationGuard.SuppressCurrentRegistrations();
            return ManagedSkinModLoader.IsProviderActive(providerId);
        }
    }

    private static void RefreshLocalizationIfNeeded(
        IReadOnlyDictionary<string, ResourceFile> mountedFiles)
    {
        foreach (var file in mountedFiles.Where(pair =>
                     pair.Key.Contains("/localization/", StringComparison.OrdinalIgnoreCase)))
        {
            MountedLocalizationFiles[file.Key] = file.Value;
        }

        // A deselection can mount no language files at all. It still changes which provider
        // tables the game is allowed to merge, so never skip this check for an empty file delta.
        var signature = BuildActiveLocalizationSignature();

        try
        {
            var manager = LocManager.Instance;
            if (manager == null || string.IsNullOrWhiteSpace(manager.Language))
            {
                // During boot LocManager initializes after Mod PCK mounting and reads the
                // selected files itself. Only an in-session switch needs an explicit reload.
                _mountedLocalizationSignature = signature;
                return;
            }

            if (string.Equals(_mountedLocalizationSignature, signature, StringComparison.Ordinal))
            {
                return;
            }

            var language = manager.Language;
            if (_mountedLocalizationSignature != null)
            {
                CaptureLocalizationState(manager, _mountedLocalizationSignature, language);
            }

            var cacheKey = LocalizationCacheKey(signature, language);
            if (LocalizationStateCache.TryGetValue(cacheKey, out var cached) &&
                SetLanguageInternalMethod != null)
            {
                SetLanguageInternalMethod.Invoke(
                    manager,
                    [language, cached.Tables, cached.OverridesActive, cached.ValidationErrors.ToList()]);
                _mountedLocalizationSignature = signature;
                ModLog.Info($"已复用 {language} 本地化状态缓存。");
                return;
            }

            manager.SetLanguage(language);
            _mountedLocalizationSignature = signature;
            CaptureLocalizationState(manager, signature, language);
            ModLog.Info($"已刷新 {language} 本地化缓存。");
        }
        catch (Exception exception)
        {
            // A broken optional translation must not make an otherwise valid visual switch fail.
            ModLog.Warn("刷新皮肤本地化缓存失败：" + exception.GetBaseException().Message);
        }
    }

    private static string BuildActiveLocalizationSignature()
    {
        var catalog = Catalog;
        if (catalog == null)
        {
            return string.Empty;
        }

        var visualSelections = GetVisualSelections();
        var activePaths = catalog.FilterModdedLocalizationTables(
                MountedLocalizationFiles.Keys,
                visualSelections)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var activeFiles = MountedLocalizationFiles
            .Where(pair => activePaths.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);
        return BuildOverlaySignature(activeFiles, "localization") + "\n" + string.Join(
            "\n",
            catalog.GetSelectedLocalizationProviderIds(visualSelections)
                .Order(StringComparer.OrdinalIgnoreCase));
    }

    public static IEnumerable<string> FilterModdedLocalizationTables(
        IEnumerable<string> localizationPaths)
    {
        lock (Sync)
        {
            return Catalog?.FilterModdedLocalizationTables(localizationPaths, GetVisualSelections()) ??
                   localizationPaths.ToArray();
        }
    }

    private static void CaptureLocalizationState(
        LocManager manager,
        string signature,
        string language)
    {
        if (LocTablesField?.GetValue(manager) is not Dictionary<string, LocTable> tables)
        {
            return;
        }

        LocalizationStateCache[LocalizationCacheKey(signature, language)] = new LocalizationCacheState(
            tables,
            manager.OverridesActive,
            manager.ValidationErrors.ToList());
    }

    private static string LocalizationCacheKey(string signature, string language) =>
        language + "\n" + signature;

    private static void MountCardOverlay(IReadOnlySet<string> groups)
    {
        var catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
        var priorityStacks = catalog.CardGroups.Where(group => groups.Contains(group.Id)).ToDictionary(
            group => group.Id,
            group => (IReadOnlyList<string>)GetCardPriorityEntriesInternal(group)
                .Where(entry => entry.Enabled)
                .Select(entry => entry.OptionId)
                .ToArray(),
            StringComparer.OrdinalIgnoreCase);
        var files = catalog.BuildCardOverlay(Config.Selections, priorityStacks, groups);
        if (files.Count == 0)
        {
            return;
        }

        // Resolve ownership first, then package the already-disjoint target paths by source
        // archive. Toggling a tiny provider must not create a new combined copy of every other
        // enabled pack. Unchanged slices reuse MountArchiveOverlay's deterministic cache.
        foreach (var slice in files.GroupBy(pair => pair.Value.Archive))
            MountArchiveOverlay(slice.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase),
                "cards", "Godot 拒绝加载生成的卡牌皮肤资源包。");
        // The baseline card pack intentionally reclaims canonical source/import/remap paths.
        // Cached binary AtlasTexture overlays must therefore reacquire only the bridge paths
        // they actually depend on when the corresponding skin is requested again.
        CardCanonicalResourceOwners.Reset();
    }

    /// <summary>
    /// 只把"栈顶提供的来源和现在要的不一样"的路径写成新的一层覆盖包挂上去。Godot 的
    /// LoadResourcePack 只能置顶、不能卸载，所以先前挂过的层会继续为没变化的路径服务：
    /// 换一次皮肤通常只动几十条路径，没必要把整份 600 MB 重写一遍。
    /// 返回这次真正挂上去的文件数（0 表示什么都没变、也没有写盘）。
    /// </summary>
    private static int MountOverlayDelta(
        IReadOnlyDictionary<string, ResourceFile> files,
        string category,
        string failureMessage)
    {
        if (files.Count == 0)
        {
            return 0;
        }

        var delta = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in files)
        {
            if (MountedOverlayOwners.TryGetValue(pair.Key, out var owner) &&
                owner.ArchivePath.Equals(pair.Value.ArchivePath, StringComparison.OrdinalIgnoreCase) &&
                owner.Path.Equals(pair.Value.Path, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            delta[pair.Key] = pair.Value;
        }

        if (delta.Count == 0)
        {
            PerfTrace.Report($"覆盖包[{category}] 无变化（跳过写盘+挂载）", 0, files.Count);
            return 0;
        }

        var estimatedBytes = EstimateOverlayBytes(delta);
        if (estimatedBytes > MaxSingleOverlayBytes)
        {
            // 单层就超过上限：与其把磁盘/内存一次性吃光然后闪退，不如保留当前外观。
            ModLog.Error(
                $"本次外观覆盖要写 {estimatedBytes / 1024.0 / 1024.0:F0} MB，" +
                $"超过单层上限 {MaxSingleOverlayBytes / 1024 / 1024} MB；已跳过这次挂载并保留当前外观。" +
                "如果反复出现，请减少同时启用的皮肤包。");
            return 0;
        }

        var written = MountArchiveOverlay(delta, category, failureMessage);
        foreach (var pair in delta)
        {
            MountedOverlayOwners[pair.Key] = new OverlayOwner(pair.Value.ArchivePath, pair.Value.Path);
        }

        _mountedOverlayLayers++;
        _mountedOverlayBytes += written;
        if (_mountedOverlayBytes >= _nextMountedOverlayWarningBytes)
        {
            ModLog.Warn(
                $"[性能] 本次会话已挂载覆盖层累计 {_mountedOverlayBytes / 1024.0 / 1024.0:F0} MB、" +
                $"{_mountedOverlayLayers} 层。Godot 不能卸载已挂载的包，继续大量换肤会耗尽内存。");
            _nextMountedOverlayWarningBytes *= 2;
        }

        PerfTrace.Report($"覆盖包[{category}] 增量层", 0, delta.Count);
        return delta.Count;
    }

    /// <summary>按 PCK 索引里记录的文件大小估算这一层要写多少字节，只用于单层上限保护。</summary>
    private static long EstimateOverlayBytes(
        IReadOnlyDictionary<string, ResourceFile> files)
    {
        long total = 0;
        foreach (var pair in files)
        {
            try
            {
                if (pair.Value.Archive.Contains(pair.Value.Path))
                {
                    total += (long)pair.Value.Archive.GetFileSize(pair.Value.Path);
                }
            }
            catch
            {
                // 估不出来就不计入；这个数字只用来做上限保护。
            }
        }

        return total;
    }

    private static long MountArchiveOverlay(
        IReadOnlyDictionary<string, ResourceFile> files,
        string category,
        string failureMessage)
    {
        if (files.Count == 0)
        {
            return 0;
        }

        var signature = BuildOverlaySignature(files, category);
        long writtenBytes = 0;
        if (!MountedOverlayCache.TryGetValue(signature, out var overlayPath) || !File.Exists(overlayPath))
        {
            overlayPath = TryGetCompleteSourceArchive(files);
            if (overlayPath != null)
            {
                PerfTrace.Report($"覆盖包[{category}] 直接复用原整包", 0, files.Count);
            }

            if (overlayPath == null)
            {
                // 先看跨会话缓存：同样的材质戳 + 同样的覆盖包签名 = 同一份字节，不必重写几百 MB。
                // 缓存由 OverlayCacheStore 负责上限与材质戳失效，这里只消费。
                var cacheWatch = Stopwatch.StartNew();
                overlayPath = OverlayCacheStore.TryGet(signature);
                if (overlayPath != null)
                {
                    PerfTrace.Report($"覆盖包[{category}] 命中磁盘缓存",
                        cacheWatch.Elapsed.TotalMilliseconds, files.Count);
                }
            }

            if (overlayPath == null)
            {
                overlayPath = OverlayCacheStore.ReservePath(signature);
                var sources = files.ToDictionary(
                    pair => pair.Key,
                    pair => (pair.Value.Archive, pair.Value.Path),
                    StringComparer.OrdinalIgnoreCase);
                var writeWatch = Stopwatch.StartNew();
                try
                {
                    EnsureCacheSpace(overlayPath, files.Count);
                    System.IO.Directory.CreateDirectory(
                        System.IO.Path.GetDirectoryName(overlayPath)!);
                    PckArchive.WriteFromArchives(overlayPath, sources);
                    var written = File.Exists(overlayPath) ? new FileInfo(overlayPath).Length : 0;
                    writtenBytes = written;
                    PerfTrace.Report(
                        $"覆盖包[{category}] 写盘 {written / 1024.0 / 1024.0:F1} MB",
                        writeWatch.Elapsed.TotalMilliseconds,
                        files.Count);
                    OverlayCacheStore.AfterWrite();
                }
                catch (Exception exception)
                {
                    // 写盘失败（磁盘满、被占用、文件系统只读）以前会把异常一路抛到初始化，
                    // 整个皮肤功能被禁用甚至崩在场景构建里。现在只作废这一份覆盖包并保留
                    // 已有的外观，下一次重建还能再试。
                    TryDelete(overlayPath);
                    MountedOverlayCache.Remove(signature);
                    ModLog.Error(
                        $"生成外观覆盖包失败（{category}，{files.Count} 个文件）：" +
                        exception.GetBaseException().Message);
                    throw new InvalidOperationException(failureMessage, exception);
                }
            }

            MountedOverlayCache[signature] = overlayPath;
        }

        // Loading the same pack again is intentional. Godot gives the most recently loaded pack
        // priority, so an existing deterministic pack can restore paths shadowed by a temporary
        // runtime pack without writing another multi-hundred-megabyte copy to disk.
        var mountWatch = Stopwatch.StartNew();
        if (!ProjectSettings.LoadResourcePack(overlayPath, replaceFiles: true))
        {
            MountedOverlayCache.Remove(signature);
            throw new InvalidOperationException(failureMessage);
        }

        PerfTrace.Report($"覆盖包[{category}] LoadResourcePack",
            mountWatch.Elapsed.TotalMilliseconds, files.Count);
        return writtenBytes;
    }

    private static string? TryGetCompleteSourceArchive(
        IReadOnlyDictionary<string, ResourceFile> files)
    {
        var archive = files.Values.FirstOrDefault()?.Archive;
        if (archive == null ||
            files.Values.Any(file => !ReferenceEquals(file.Archive, archive)) ||
            !File.Exists(archive.Path))
        {
            return null;
        }

        var archivePaths = archive.Paths.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (files.Count != archivePaths.Count ||
            files.Any(pair =>
                !ReferenceEquals(pair.Value.Archive, archive) ||
                !pair.Key.Equals(pair.Value.Path, StringComparison.OrdinalIgnoreCase)) ||
            !archivePaths.SetEquals(files.Keys))
        {
            return null;
        }

        // Mounting the original archive is equivalent to mounting an exact byte-for-byte copy.
        // Baseline overlays still restore any canonical paths after deselection; private provider
        // namespaces are inert once their DLL callbacks are deactivated.
        return archive.Path;
    }

    private static string BuildOverlaySignature(
        IReadOnlyDictionary<string, ResourceFile> files,
        string category)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendSignaturePart(hash, category);
        foreach (var pair in files.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            AppendSignaturePart(hash, pair.Key);
            AppendSignaturePart(hash, pair.Value.ArchivePath);
            AppendSignaturePart(hash, pair.Value.Path);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>
    /// 外观覆盖文件表的复用签名。必须包含**所有**能改变产物的输入：
    /// 目录实例（换了目录就换了一套选项/归档）、涉及的分组、生效选择、要恢复的基线路径、
    /// 以及当前被提到栈顶的完整包。少写一项就会复用出过期覆盖包（皮肤看起来"切了没变"）。
    /// </summary>
    private static string BuildOverlayPlanSignature(
        SkinCatalog catalog,
        IEnumerable<string> overlayGroups,
        IReadOnlyDictionary<string, string> selections,
        IEnumerable<string> staleCanonicalPaths,
        IEnumerable<string> promotedPackPaths)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendSignaturePart(hash, "overlay-plan-v1");
        AppendSignaturePart(hash, RuntimeHelpers.GetHashCode(catalog).ToString());
        foreach (var groupId in overlayGroups.OrderBy(id => id, StringComparer.OrdinalIgnoreCase))
        {
            AppendSignaturePart(hash, groupId);
        }

        foreach (var pair in selections.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            AppendSignaturePart(hash, pair.Key);
            AppendSignaturePart(hash, pair.Value);
        }

        foreach (var path in staleCanonicalPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            AppendSignaturePart(hash, path);
        }

        foreach (var path in promotedPackPaths.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
        {
            AppendSignaturePart(hash, path);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private sealed record OverlayPlan(string Signature, Dictionary<string, ResourceFile> Files);

    /// <summary>
    /// 写覆盖包之前的空间闸门。一个完整外观覆盖包在这些大型皮肤包上实测 100-400 MB，
    /// 而缓存在 C: 盘（`%APPDATA%\SlayTheSpire2\Gurio.SkinChanger\cache`）。磁盘写满时
    /// Godot 会在场景构建中途失败，表现为黑屏或整局崩掉；提前失败至少能保留当前外观。
    /// </summary>
    private static void EnsureCacheSpace(string overlayPath, int fileCount)
    {
        try
        {
            var root = Path.GetPathRoot(Path.GetFullPath(overlayPath));
            if (string.IsNullOrEmpty(root) || new DriveInfo(root) is not { IsReady: true } drive)
            {
                return;
            }

            // 阈值按文件数给一个保守估计：导入载荷平均 0.5 MB/文件，留 128 MB 余量。
            var estimatedBytes = Math.Min((long)fileCount * 512 * 1024, 2L * 1024 * 1024 * 1024);
            var required = estimatedBytes / 4 + 128L * 1024 * 1024;
            if (drive.AvailableFreeSpace >= required)
            {
                return;
            }

            throw new IOException(
                $"缓存盘 {drive.Name} 只剩 {drive.AvailableFreeSpace / 1024 / 1024} MB，" +
                $"本次覆盖包（{fileCount} 个文件）需要约 {required / 1024 / 1024} MB。" +
                "请清理磁盘后重试：缓存目录 " + SkinChangerPaths.CacheDirectory);
        }
        catch (IOException)
        {
            throw;
        }
        catch
        {
            // 拿不到磁盘信息时不要因为诊断代码本身阻断换肤。
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // 留着坏文件也比在这里抛异常安全：下次启动的缓存清理会把它带走。
        }
    }

    private sealed record CardLookup(
        string GroupId,
        string CardType,
        CardSkinGroup? Group,
        IReadOnlyList<CardSkinOption> Options,
        IReadOnlyDictionary<string, CardOptionLookup> OptionsById,
        IReadOnlyList<CardSkinGroup> PriorityGroups)
    {
        public bool BaselineUnavailable { get; init; }
    }

    private sealed record CardOptionLookup(
        CardSkinOption Option,
        IReadOnlyList<string> MatchedAssetPaths);

    private sealed record CardCoverageState(
        int TotalCards,
        IReadOnlyDictionary<string, int> ByOption);

    private sealed record CardPortraitRequest(
        string GroupId,
        string Selection,
        string ResourcePath,
        string CacheKey,
        bool UseSelectedProvider,
        bool WrapAtlas)
    {
        public string OverlayKey =>
            $"{GroupId}\n{Selection}\n{(UseSelectedProvider ? "provider" : "base")}";
    }

    private sealed record PreparedRuntimeOverlay(
        string Key,
        string AliasToken,
        string? OverlayPath,
        IReadOnlyDictionary<string, string> ResourcePaths,
        IReadOnlySet<string> CanonicalDependencyPaths,
        IReadOnlySet<string> RestoreGroups,
        int FileCount,
        long FileSize);

    private sealed record RuntimeResourceBundleState(
        Dictionary<string, Resource> Resources);

    private sealed class IsolatedCardOverlayState(string ownerId)
    {
        public string OwnerId { get; } = ownerId;
        public string? AliasToken { get; set; }
        public Dictionary<string, string> ResourcePaths { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> UnavailablePaths { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public List<string> OverlayPaths { get; } = [];
        public HashSet<string> MountedFilePaths { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> CanonicalRedirectPaths { get; } =
            new(StringComparer.OrdinalIgnoreCase);
        public bool CanReuseExternalDependencies { get; set; } = true;
    }

    private sealed record LocalizationCacheState(
        Dictionary<string, LocTable> Tables,
        bool OverridesActive,
        IReadOnlyList<LocValidationError> ValidationErrors);

    private static void AppendSignaturePart(IncrementalHash hash, string value)
    {
        hash.AppendData(Encoding.UTF8.GetBytes(value));
        hash.AppendData([0]);
    }

    private static void ClearRuntimeResourceCache(string groupId)
    {
        var prefix = groupId + "\n";
        RestoreCachedRelicTexturesToBaseline(groupId, prefix);
        foreach (var key in RuntimeResourceCache.Keys
                     .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                     .ToArray())
        {
            RuntimeResourceCache.Remove(key);
        }
        foreach (var key in PreparedRuntimeOverlays.Keys
                     .Where(key => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                     .ToArray())
        {
            PreparedRuntimeOverlays.Remove(key);
            RuntimeResourceBundles.Remove(key);
        }
    }

    private static void RestoreCachedRelicTexturesToBaseline(string groupId, string cachePrefix)
    {
        if (Catalog == null)
        {
            return;
        }

        var restored = 0;
        foreach (var pair in RuntimeResourceCache.Where(pair =>
                     pair.Key.StartsWith(cachePrefix, StringComparison.OrdinalIgnoreCase) &&
                     pair.Value is AtlasTexture))
        {
            var pathStart = pair.Key.IndexOf('\n', cachePrefix.Length);
            if (pathStart < 0 || pathStart + 1 >= pair.Key.Length)
            {
                continue;
            }

            var resourcePath = pair.Key[(pathStart + 1)..];
            if (!SkinCatalog.IsRelicAtlasSpritePath(resourcePath) ||
                pair.Value is not AtlasTexture existing)
            {
                continue;
            }

            try
            {
                if (GetBaselineRelicIcon(Catalog, resourcePath) is not AtlasTexture baseline)
                {
                    continue;
                }

                // TextureRect nodes can keep the old AtlasTexture object after the character
                // selection changes. Mutate that still-referenced object back to the game's
                // atlas and coordinates before dropping our cache entry; merely remounting the
                // original PCK cannot update an already constructed Godot Resource.
                existing.Atlas = baseline.Atlas;
                existing.Region = baseline.Region;
                existing.Margin = baseline.Margin;
                existing.FilterClip = baseline.FilterClip;
                existing.EmitChanged();
                restored++;
            }
            catch (Exception exception)
            {
                ModLog.Warn(
                    $"回退 {resourcePath} 的遗物图集缓存失败，将在下次创建图标时恢复：" +
                    exception.GetBaseException().Message);
            }
        }

        if (restored > 0)
        {
            ModLog.Info($"已将 {groupId} 的 {restored} 个存量遗物图标恢复到游戏图集。");
        }
    }

    private static void ClearCardPortraitCache(string groupId)
    {
        // Character priorities can change a portrait cached under its canonical Ancient
        // category. Clear dependent keys as well, without throwing away GPU resources for
        // unrelated providers or changing their ownership.
        var affectedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId };
        foreach (var pair in _cardLookupCache)
            if (pair.Value.PriorityGroups.Any(group => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)))
                affectedGroups.Add(pair.Value.GroupId);
        bool Affected(string key) => affectedGroups.Any(id => key.StartsWith(id + "\n", StringComparison.OrdinalIgnoreCase));
        foreach (var key in CardPortraitCache.Keys
                     .Where(Affected)
                     .ToArray())
        {
            CardPortraitCache.Remove(key);
            ExternalCardProviderIdentityPaths.Remove(key);
        }

        FailedCardPortraitRequests.RemoveWhere(Affected);
    }

    private static string CardSelectionKey(string groupId) => "cards:" + groupId;

    private static string IndividualCardSelectionKey(CardModel card) =>
        "cards:item:" + card.Id.ToString().ToLowerInvariant();

    private static string? NormalizeCardSkinPresetName(string name)
    {
        var normalized = name.Trim();
        if (!CanRenameOrDeletePreset(normalized)) return null;
        if (normalized.Length == 0 || normalized.Any(char.IsControl))
        {
            LastError = "卡图预设名称不能为空。";
            return null;
        }

        if (normalized.Length > CardSkinPresetNameMaxLength)
        {
            LastError = $"卡图预设名称不能超过 {CardSkinPresetNameMaxLength} 个字符。";
            return null;
        }

        return normalized;
    }

    private static string? NormalizeMonsterSkinPresetName(string name)
    {
        var normalized = name.Trim();
        if (!CanRenameOrDeletePreset(normalized)) return null;
        if (normalized.Length == 0 || normalized.Any(char.IsControl))
        {
            LastError = "怪物皮肤预设名称不能为空。";
            return null;
        }
        if (normalized.Length > MonsterSkinPresetNameMaxLength)
        {
            LastError = $"怪物皮肤预设名称不能超过 {MonsterSkinPresetNameMaxLength} 个字符。";
            return null;
        }
        return normalized;
    }

    private static int FindMonsterSkinPresetIndex(string categoryId, string name) =>
        Config.MonsterSkinPresets.FindIndex(preset =>
            preset.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase) &&
            preset.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

    private static MonsterSkinPreset CaptureCurrentMonsterSkinPreset(string categoryId, string name)
    {
        if (!Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out var groupIds))
        {
            throw new InvalidOperationException($"未知的怪物图鉴分类：{categoryId}");
        }
        var priority = GetMonsterPriorityEntriesInternal(categoryId).ToList();
        var scopedSelections = groupIds.ToDictionary(
            groupId => groupId,
            Config.GetSelection,
            StringComparer.OrdinalIgnoreCase);
        var snapshot = MonsterSkinPresetPolicy.Capture(
            categoryId,
            groupIds,
            priority.Select(entry => new MonsterSkinPresetPriorityState(entry.OptionId, entry.Enabled)).ToArray(),
            scopedSelections);
        return new MonsterSkinPreset
        {
            Name = name,
            CategoryId = snapshot.CategoryId,
            Priority = snapshot.Priority
                .Select(entry => new MonsterSkinPriorityEntry(entry.OptionId, entry.Enabled))
                .ToList(),
            Selections = new Dictionary<string, string>(snapshot.Selections, StringComparer.OrdinalIgnoreCase),
            FollowingGroupIds = groupIds.Where(groupId => Config.MonsterGroupsFollowingCategory.Contains(
                groupId, StringComparer.OrdinalIgnoreCase)).ToList()
        };
    }

    private static MonsterSkinPreset CloneMonsterSkinPreset(MonsterSkinPreset preset) => preset.Clone();

    private static bool SaveMonsterPresetConfiguration(
        string operation,
        MonsterSkinPreset preset,
        Action rollback)
    {
        try
        {
            Config.Save(ConfigPath);
            LastError = null;
            return true;
        }
        catch (Exception exception)
        {
            rollback();
            LastError = exception.Message;
            ModLog.Error($"{operation}怪物皮肤预设 {preset.Name} 失败：{exception}");
            return false;
        }
    }

    private static void RestoreActiveMonsterPreset(string categoryId, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Config.ActiveMonsterSkinPresets.Remove(categoryId);
        }
        else
        {
            Config.ActiveMonsterSkinPresets[categoryId] = name;
        }
    }

    private static int FindCardSkinPresetIndex(string groupId, string name) =>
        Config.CardSkinPresets.FindIndex(preset =>
            preset.CategoryId?.Equals(groupId, StringComparison.OrdinalIgnoreCase) == true &&
            preset.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));

    private static bool IsKnownCardGroup(string groupId) =>
        !string.IsNullOrWhiteSpace(groupId) &&
        Catalog?.CardGroups.Any(group => group.Id.Equals(
            groupId,
            StringComparison.OrdinalIgnoreCase)) == true;

    private static string? GetActiveCardSkinPreset(string groupId) =>
        Config.ActiveCardSkinPresets.GetValueOrDefault(groupId);

    private static void SetActiveCardSkinPreset(string groupId, string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            Config.ActiveCardSkinPresets.Remove(groupId);
            return;
        }

        Config.ActiveCardSkinPresets[groupId] = name;
    }

    private static CardSkinPreset CaptureCurrentCardSkinPreset(string groupId, string name)
    {
        var group = Catalog?.CardGroups.FirstOrDefault(candidate => candidate.Id.Equals(
            groupId,
            StringComparison.OrdinalIgnoreCase));
        if (group == null)
        {
            throw new InvalidOperationException($"未知的卡牌皮肤分类：{groupId}");
        }

        GetCardPriorityEntriesInternal(group);

        return new CardSkinPreset
        {
            Name = name,
            CategoryId = group.Id,
            CardSkinPriorities = new Dictionary<string, List<CardSkinPriorityEntry>>(
                StringComparer.OrdinalIgnoreCase)
            {
                [group.Id] = Config.CardSkinPriorities.TryGetValue(group.Id, out var entries)
                    ? entries.ToList()
                    : []
            },
            Selections = CaptureCardSelectionsForGroup(group.Id)
        };
    }

    private static CardSkinPreset CloneCardSkinPreset(CardSkinPreset preset) => preset.Clone();

    private static Dictionary<string, List<CardSkinPriorityEntry>> CloneCardSkinPriorities(
        IReadOnlyDictionary<string, List<CardSkinPriorityEntry>> priorities) =>
        priorities.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToList(),
            StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, string> CaptureCardSelections() =>
        Config.Selections
            .Where(pair => pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);

    private static Dictionary<string, string> CaptureCardSelectionsForGroup(string groupId) =>
        CaptureCardSelections()
            .Where(pair => CardSelectionBelongsToGroup(pair.Key, groupId))
            .ToDictionary(
                pair => pair.Key,
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);

    private static void ReplaceCardSelectionsForGroup(
        string groupId,
        IReadOnlyDictionary<string, string> selections)
    {
        foreach (var key in Config.Selections.Keys
                     .Where(key => CardSelectionBelongsToGroup(key, groupId))
                     .ToArray())
        {
            Config.Selections.Remove(key);
        }

        foreach (var pair in selections.Where(pair =>
                     pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase) &&
                     CardSelectionBelongsToGroup(pair.Key, groupId)))
        {
            Config.Selections[pair.Key] = pair.Value;
        }
    }

    private static bool CardSelectionBelongsToGroup(string key, string groupId)
    {
        if (key.Equals(CardSelectionKey(groupId), StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        const string prefix = "cards:item:";
        if (!key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ModelDb.AllCards.Any(card =>
            IndividualCardSelectionKey(card).Equals(key, StringComparison.OrdinalIgnoreCase) &&
            CardBelongsToGroup(card, groupId));
    }

    private static void MigrateLegacyCardSkinPresets()
    {
        if (Catalog == null || Catalog.CardGroups.Count == 0) return;
        var cardGroups = ModelDb.AllCards.DistinctBy(IndividualCardSelectionKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(IndividualCardSelectionKey, card => GetCardLookup(card).GroupId, StringComparer.OrdinalIgnoreCase);
        var result = CardPresetMigrationPolicy.Run(Config,
            Catalog.CardGroups.ToDictionary(group => group.Id, group => group.DisplayName, StringComparer.OrdinalIgnoreCase), cardGroups);
        if (result.Split > 0 || result.Archived > 0)
            ModLog.Info($"卡牌预设兼容修复：按实际旧数据拆分 {result.Split} 项，归档重复回填副本 {result.Archived} 项；" +
                        "归档保存在 ArchivedLegacyCardSkinPresets，未重置当前卡面选择。");
    }

    private static string RuntimeResourceKey(string groupId, string resourcePath) =>
        RuntimeResourceKey(groupId, GetVisualSelection(groupId), resourcePath);

    private static string RuntimeResourceKey(
        string groupId,
        string selection,
        string resourcePath) =>
        groupId + "\n" + ManualVariantResourceSelection(groupId, selection) + "\n" + resourcePath +
        FrameworkRegistryCooperation.CacheSuffix(groupId, selection);

    private static string RuntimeOverlayKey(
        string groupId,
        string selection,
        IReadOnlyCollection<string> resourcePaths,
        bool includeProviderDependencies,
        bool reuseMountedPrivateDependencies,
        bool isolateRelicCanonicalPaths = false) =>
        groupId + "\n" + ManualVariantResourceSelection(groupId, selection) + "\n" + includeProviderDependencies + "\n" +
        reuseMountedPrivateDependencies + "\n" + isolateRelicCanonicalPaths + "\n" +
        string.Join("\n", resourcePaths) + FrameworkRegistryCooperation.CacheSuffix(groupId, selection);

    private static string[] CharacterSelectResourcePaths(string characterId) =>
    [
        $"res://scenes/screens/char_select/char_select_bg_{characterId}.tscn",
        $"res://images/packed/character_select/char_select_{characterId}.png",
        $"res://images/packed/character_select/char_select_{characterId}_locked.png"
    ];

    private static string PreparedRuntimeOverlayDirectory() => SkinChangerPaths.RuntimeCacheDirectory;

    private static SkinConfig LoadConfig()
    {
        if (File.Exists(ConfigPath) || File.Exists(ConfigPath + ".bak"))
        {
            return RecoverInterruptedCharacterSkinBundleSession(SkinConfig.Load(ConfigPath));
        }

        var config = SkinConfig.Load(LegacyConfigPath);
        if (File.Exists(LegacyConfigPath))
        {
            ModLog.Info("已将旧版 STS2SkinChanger 设置迁移到皮肤切换器-Skin Changer。");
        }
        return RecoverInterruptedCharacterSkinBundleSession(config);
    }

    private static CharacterSkinComposition CloneCharacterSkinComposition(
        CharacterSkinComposition source) =>
        new()
        {
            Id = source.Id,
            GroupId = source.GroupId,
            Name = source.Name,
            SourceOptionIds = source.SourceOptionIds.ToList(),
            HideSources = source.HideSources
        };

    private static void MigrateLegacyCharacterIconSelections()
    {
        if (Catalog == null || Config.CharacterIconSelections.Count == 0)
        {
            return;
        }

        foreach (var pair in Config.CharacterIconSelections.ToArray())
        {
            Config.CharacterIconSelections.Remove(pair.Key);
            if (pair.Value.Equals(
                    LegacyFollowCharacterSkinIconSelectionId,
                    StringComparison.OrdinalIgnoreCase) ||
                pair.Value.Equals(
                    SkinCatalog.BaseOptionId,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var iconOptionId = Catalog.ResolveStoredVisualSelectionId(pair.Key, pair.Value);
            var rawOptions = Catalog.GetRawCharacterOptions(pair.Key);
            if (rawOptions.All(option => !option.Id.Equals(
                    iconOptionId,
                    StringComparison.OrdinalIgnoreCase)))
            {
                ModLog.Info($"旧头像来源 {pair.Key}/{pair.Value} 已不存在，已忽略。 ");
                continue;
            }

            var sources = new List<string> { iconOptionId };
            var selected = Config.GetSelection(pair.Key);
            if (!selected.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                sources.AddRange(Catalog.GetCompositionSourceOptionIds(pair.Key, selected));
            }
            sources = sources.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var sourceNames = sources.Select(sourceId =>
                    rawOptions.FirstOrDefault(option => option.Id.Equals(
                        sourceId,
                        StringComparison.OrdinalIgnoreCase))?.Name)
                .Where(sourceName => !string.IsNullOrWhiteSpace(sourceName))
                .Cast<string>();
            var name = CharacterSkinCompositionPolicy.UniqueName(
                string.Join(" + ", sourceNames),
                Config.CharacterSkinCompositions
                    .Where(composition => composition.GroupId.Equals(
                        pair.Key,
                        StringComparison.OrdinalIgnoreCase))
                    .Select(composition => composition.Name),
                ModLocalization.Get(ModText.CombinedSkinDefaultName));
            var composition = new CharacterSkinComposition
            {
                Id = CharacterSkinCompositionPolicy.CreateId(),
                GroupId = pair.Key,
                Name = name,
                SourceOptionIds = sources
            };
            Config.CharacterSkinCompositions.Add(composition);
            Config.Selections[pair.Key] = composition.Id;
            ModLog.Info($"已将 {pair.Key} 的旧头像来源迁移为合并皮肤“{name}”。");
        }
    }

    private static void SanitizeSelections()
    {
        Config.CharacterSkinCompositions = CharacterSkinCompositionPolicy.Normalize(
            Config.CharacterSkinCompositions);
        Catalog!.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
        MigrateLegacyCharacterIconSelections();
        Catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);

        var migratedVisualSelectionCount = 0;
        foreach (var group in Catalog!.Groups)
        {
            if (!Config.Selections.TryGetValue(group.Id, out var storedSelection))
            {
                continue;
            }

            var resolvedSelection = Catalog.ResolveStoredVisualSelectionId(
                group.Id,
                storedSelection);
            if (resolvedSelection.Equals(storedSelection, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Config.Selections[group.Id] = resolvedSelection;
            migratedVisualSelectionCount++;
        }

        var migratedManualSlots = MigrateLegacyManualSlots(Config, Catalog);
        if (migratedManualSlots > 0) ModLog.Info($"已将 {migratedManualSlots} 项旧部件显隐设置迁移为编号皮肤差分。");

        if (migratedVisualSelectionCount > 0)
        {
            ModLog.Info(
                $"已把 {migratedVisualSelectionCount} 项旧外观设置迁移到具体的同 ID 差分包。");
        }

        if (Config.VisualSelectionDefaultsVersion < 1)
        {
            // The old default picked the first discovered Mod option for every new group. A
            // multi-monster DLL could therefore be saved as a misleading partial bundle: the UI
            // said that provider was selected, but its runtime was intentionally not started.
            // Only clear those legacy partial selections. Complete and independently saved
            // selections remain untouched.
            var legacyScopedProviders = Catalog!.Groups
                .Select(group => Config.GetSelection(group.Id))
                .Where(Catalog.ProviderUsesScopedMonsterRuntime)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            foreach (var providerId in legacyScopedProviders)
            {
                var ownedGroups = Catalog.GetScopedMonsterRuntimeProviderGroups(providerId);
                var selectedGroups = ownedGroups.Where(groupId =>
                    Config.GetSelection(groupId).Equals(providerId, StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (selectedGroups.Length == 0 || selectedGroups.Length == ownedGroups.Count)
                {
                    continue;
                }

                foreach (var groupId in selectedGroups)
                {
                    Config.Selections[groupId] = SkinCatalog.BaseOptionId;
                }

                ModLog.Info(
                    $"已清理 {providerId} 的 {selectedGroups.Length} 个旧版自动外观选择；" +
                    "这些组现在默认使用游戏原版，玩家重新选择后可按怪物独立生效。");
            }

            Config.VisualSelectionDefaultsVersion = 1;
        }

        // A recognized part takeover is already drawn by the author's own mounted package, so it
        // becomes the group's default. Recognising it must never switch off what the player is
        // looking at; turning it off stays an explicit choice in the skin list.
        var adoptPartTakeovers = Config.BaselinePartTakeoverVersion < 1;
        foreach (var group in Catalog!.Groups)
        {
            var partDefault = SkinCatalog.ResolvePartTakeoverDefault(group.Options);
            if (partDefault == null)
            {
                if (!Config.Selections.ContainsKey(group.Id))
                {
                    Config.Selections[group.Id] = SkinCatalog.BaseOptionId;
                }

                continue;
            }

            if (!Config.Selections.TryGetValue(group.Id, out var storedSelection))
            {
                Config.Selections[group.Id] = partDefault;
                continue;
            }

            // Earlier versions wrote the game original for every untouched group, which is
            // indistinguishable from a deliberate choice. Adopt the part once, then respect it.
            if (adoptPartTakeovers &&
                storedSelection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                Config.Selections[group.Id] = partDefault;
                ModLog.Info(
                    $"已将 {group.Id} 的默认外观保留给部件接管 {partDefault}；" +
                    "该部件原本就由作者自己的 Mod 生效，如需关闭请在皮肤列表中选择游戏原版。");
            }
        }

        Config.BaselinePartTakeoverVersion = 1;

        SanitizeMonsterSkinPriorities();

        // Old versions saved Osty independently, and only linked DLL-backed suites. Repair
        // that stale choice before deciding whether a full-runtime suite is incomplete.
        SynchronizeCompanionSelections();

        // A full-runtime provider is safe only as one coherent selection transaction. Never leave
        // a partial provider displayed as selected while its callbacks are deliberately inactive,
        // and never complete it by silently overwriting another explicit group choice.
        var incompleteProviders = Catalog.Groups
            .Select(group => Config.GetSelection(group.Id))
            .Select(Catalog.ResolveVisualProviderId)
            .Where(Catalog.ProviderUsesFullRuntime)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            // Companion groups mount their own skin through their own transaction, so a suite that
            // every non-companion group it owns still selects is complete. Counting the companion
            // group here reset the character skin to vanilla on the next launch as soon as the pet
            // was given a different provider than its owner. Option filtering keeps the strict form.
            .Where(providerId =>
                !Catalog.IsFullRuntimeProviderSelectedIgnoringCompanions(providerId, Config.Selections))
            .ToArray();
        foreach (var providerId in incompleteProviders)
        {
            var ownedGroups = Catalog.GetFullRuntimeProviderGroups(providerId);
            var resetCount = 0;
            foreach (var ownedGroupId in ownedGroups.Where(ownedGroupId =>
                         !Catalog.IsCompanionGroup(ownedGroupId) &&
                         Catalog.SelectionUsesVisualProvider(
                             ownedGroupId,
                             Config.GetSelection(ownedGroupId),
                             providerId)))
            {
                Config.Selections[ownedGroupId] = SkinCatalog.BaseOptionId;
                resetCount++;
            }

            if (resetCount > 0)
            {
                ModLog.Info(
                    $"已将 {providerId} 的 {resetCount} 个不完整联动选择恢复为游戏原版，" +
                    "避免界面显示已选但实际运行时未启用。");
            }
        }

        SynchronizeCompanionSelections();
        SanitizeCardSelections();
        SanitizeVisualProviderPriority();
    }

    /// <summary>
    /// 读取某个角色当前的选人头像。必须绕过资源缓存：切皮肤后同一路径的文件已被新的
    /// 覆盖层替换，但 Godot/游戏的 PreloadManager 里仍留着上一张皮肤的头像，
    /// 直接用 <c>character.CharacterSelectIcon</c> 就会拿到旧图（甚至别的皮肤的头像）。
    /// </summary>
    public static Texture2D? LoadCharacterSelectIcon(string characterId, bool locked)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        var id = characterId.ToLowerInvariant();
        var path = $"res://images/packed/character_select/char_select_{id}{(locked ? "_locked" : "")}.png";
        return ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.IgnoreDeep);
    }

    /// <summary>
    /// 覆盖包挂载完成后，把游戏 <c>PreloadManager.Cache</c> 里仍是上一张皮肤的场景换掉。
    /// 游戏的随从/怪物身体都走 <c>PreloadManager.Cache.GetScene(VisualsPath)</c>，
    /// 而 <c>AssetCache.GetAsset</c> 命中缓存就直接返回、不重新判定包栈；于是「奥斯提」
    /// 从 midori_black 切回游戏原版（或切到独立 Mod）时，重建出来的身体仍然是缓存里那张
    /// 帧动画的猫，独立 Mod 找不到原生 <c>%Visuals</c> SpineSprite 只能回滚并保留猫。
    /// 这里只处理 <c>creature_visuals</c> 下的场景：它们数量少、都在真正实例化时才需要。
    /// </summary>
    private static void RefreshCachedCreatureVisualScenes(IEnumerable<string> mountedPaths)
    {
        var cache = PreloadManager.Cache;
        var refreshed = new List<string>();
        foreach (var path in mountedPaths)
        {
            if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) ||
                !path.Contains("/creature_visuals/", StringComparison.OrdinalIgnoreCase) ||
                !cache.ContainsKey(path))
            {
                continue;
            }

            try
            {
                // Ignore：只绕开这个场景自身的缓存条目，不重读它引用的骨骼/贴图依赖。
                var scene = ResourceLoader.Load<PackedScene>(path, null, ResourceLoader.CacheMode.Ignore);
                if (scene == null)
                {
                    continue;
                }

                cache.SetAsset(path, scene);
                refreshed.Add(path);
            }
            catch (Exception exception)
            {
                ModLog.Warn($"刷新外观场景缓存失败 {path}：{exception.GetBaseException().Message}");
            }
        }

        if (refreshed.Count > 0)
        {
            ModLog.Info(
                $"[缓存] 已按当前包栈刷新 {refreshed.Count} 个外观场景缓存：" +
                string.Join("、", refreshed.Take(6)));
        }
    }

    /// <summary>
    /// 把游戏原包里的某个场景（例如 <c>res://scenes/creature_visuals/osty.tscn</c>）用最小覆盖包
    /// 重新压到包栈顶，并同步刷新游戏的场景缓存。独立召唤物 Mod（如 OstyDefectImpostor）要求
    /// 身体是**原生 Spine** 场景才会替换；只要规范路径当前还被别的提供者占着（整包提权、
    /// 更早的覆盖层、或 <c>PreloadManager.Cache</c> 里的旧条目），重建出来的就不是原生身体，
    /// Mod 校验失败后只能回滚、把旧皮肤继续留在屏幕上。
    /// 内容与游戏原包逐字节相同，所以即使已经是最上层，重复压一次也不会改变任何表现。
    /// </summary>
    internal static void EnsureGameSceneOnTop(string scenePath)
    {
        var catalog = Catalog;
        if (catalog == null || string.IsNullOrWhiteSpace(scenePath) ||
            !catalog.IsBaseGameResource(scenePath))
        {
            return;
        }

        try
        {
            if (!catalog.TryReadBaseGameResource(scenePath, out var bytes))
            {
                return;
            }

            var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                [scenePath] = bytes,
            };
            var remapPath = scenePath + ".remap";
            if (catalog.TryReadBaseGameResource(remapPath, out var remapBytes))
            {
                files[remapPath] = remapBytes;
            }

            var hash = Convert.ToHexString(SHA256.HashData(bytes))[..16];
            // 包名必须每次都不一样：LoadResourcePack 对同一个包重复调用并不会把它重新压到栈顶，
            // 于是中途被提权的完整皮肤包会一直盖在它上面，Replace 重读拿到的还是旧皮肤。
            var sequence = Interlocked.Increment(ref _baseScenePackSequence);
            var packPath = System.IO.Path.Combine(
                SkinChangerPaths.CacheDirectory,
                $"base_scene_{hash}_{sequence:D3}.pck");
            SkinChangerPaths.WriteCache(packPath, () => PckArchive.Write(packPath, files));
            if (!ProjectSettings.LoadResourcePack(packPath, replaceFiles: true))
            {
                ModLog.Warn($"恢复原版场景失败（Godot 拒绝加载）：{scenePath}");
                return;
            }

            // 只压包栈是不够的：Godot 的资源缓存按路径长期持有旧皮肤，CreateVisuals 走的是
            // CacheMode.Reuse。Replace 会强制重新从（新的）栈顶读取并覆盖缓存条目，再把结果
            // 写进游戏的 PreloadManager，后续按规范路径取场景才是真正的原版。
            var forcedScene = ResourceLoader.Load<PackedScene>(
                scenePath,
                null,
                ResourceLoader.CacheMode.Replace);
            if (forcedScene != null)
            {
                PreloadManager.Cache.SetAsset(scenePath, forcedScene);
                ModLog.Info($"[原版] 已用 Replace 重读并写回缓存：{scenePath}");
            }

            RefreshCachedCreatureVisualScenes(new[] { scenePath });
            ModLog.Info($"[原版] 已把游戏原版场景压到栈顶：{scenePath}（{bytes.Length} B）");
        }
        catch (Exception exception)
        {
            ModLog.Warn($"恢复原版场景失败 {scenePath}：{exception.GetBaseException().Message}");
        }
    }

    /// <summary>
    /// 探针：按规范路径取一次场景，看实例出来的身体是不是原生 Spine。
    /// 只有 false 时才需要走「私有别名路径」兜底。
    /// </summary>
    internal static bool ProbeSceneHasSpine(string scenePath)
    {
        try
        {
            var scene = ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Ignore);
            if (scene == null)
            {
                return false;
            }

            var probe = scene.Instantiate<NCreatureVisuals>(PackedScene.GenEditState.Disabled);
            if (probe == null)
            {
                return false;
            }

            var hasSpine = probe.HasSpineAnimation;
            probe.Free();
            return hasSpine;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// 用「私有别名路径」从游戏原包实例化一份场景。别名只存在于我们自己挂的覆盖包里，
    /// 所以 Godot 按路径的资源缓存里没有它的历史条目，也不受提权包/旧皮肤的栈顺序影响。
    /// 规范路径（如 res://scenes/creature_visuals/osty.tscn）一旦被别的皮肤缓存污染，
    /// 无论怎么压栈顶、刷缓存都只能拿回旧资源，这里绕开整条链路。
    /// </summary>
    private static int _baseScenePackSequence;

    /// <summary>
    /// 把「私有别名路径」里的原版场景登记到**规范路径**的 AssetCache 条目上。
    /// 这样游戏自己的 <c>MonsterModel.CreateVisuals()</c> 拿到的就是原版身体，
    /// 独立 Mod 的 CreateVisuals 后置补丁也才有机会接管（我们自己实例化会绕过它）。
    /// </summary>
    internal static bool ForceCanonicalSceneFromAlias(string scenePath)
    {
        var scene = LoadGameSceneViaAlias(scenePath, out var error);
        if (scene == null)
        {
            ModLog.Warn($"[原版] 别名场景准备失败：{error}");
            return false;
        }

        try
        {
            PreloadManager.Cache.SetAsset(scenePath, scene);
            ModLog.Info($"[原版] 已把别名场景登记为规范路径缓存：{scenePath}");
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[原版] 登记规范路径缓存失败：{exception.GetType().Name}: {exception.Message}");
            return false;
        }
    }

    internal static PackedScene? LoadGameSceneViaAlias(string scenePath, out string? error)
    {
        error = null;
        var catalog = Catalog;
        if (catalog == null || string.IsNullOrWhiteSpace(scenePath) ||
            !catalog.IsBaseGameResource(scenePath))
        {
            error = "不是游戏原包资源：" + scenePath;
            return null;
        }

        try
        {
            if (!catalog.TryReadBaseGameResource(scenePath, out var bytes))
            {
                error = "读不到游戏原包字节：" + scenePath;
                return null;
            }

            var hash = Convert.ToHexString(SHA256.HashData(bytes))[..16];
            var fileName = scenePath[(scenePath.LastIndexOf('/') + 1)..];
            var aliasPath = $"res://__skin_changer_base/{hash}/{fileName}";
            var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                [aliasPath] = bytes,
            };
            var packPath = System.IO.Path.Combine(
                SkinChangerPaths.CacheDirectory,
                $"base_alias_{hash}.pck");
            SkinChangerPaths.WriteCache(packPath, () => PckArchive.Write(packPath, files));
            if (!ProjectSettings.LoadResourcePack(packPath, replaceFiles: true))
            {
                ModLog.Warn($"[原版] 别名场景包可能已挂载过，继续尝试读取：{packPath}");
            }

            var aliasScene = ResourceLoader.Load<PackedScene>(aliasPath, null, ResourceLoader.CacheMode.Ignore);
            if (aliasScene == null)
            {
                error = "别名场景加载失败：" + aliasPath;
                return null;
            }

            ModLog.Info($"[原版] 别名场景已就绪：{aliasPath} ← {scenePath}（{bytes.Length} B）");
            return aliasScene;
        }
        catch (Exception exception)
        {
            error = $"{exception.GetType().Name}: {exception.GetBaseException().Message}";
            return null;
        }
    }

    internal static NCreatureVisuals? InstantiateGameCreatureVisuals(string scenePath, out string? error)
    {
        error = null;
        var catalog = Catalog;
        if (catalog == null || string.IsNullOrWhiteSpace(scenePath) ||
            !catalog.IsBaseGameResource(scenePath))
        {
            error = "不是游戏原包资源：" + scenePath;
            return null;
        }

        try
        {
            if (!catalog.TryReadBaseGameResource(scenePath, out var bytes))
            {
                error = "读不到游戏原包字节：" + scenePath;
                return null;
            }

            var hash = Convert.ToHexString(SHA256.HashData(bytes))[..16];
            var fileName = scenePath[(scenePath.LastIndexOf('/') + 1)..];
            var aliasPath = $"res://__skin_changer_base/{hash}/{fileName}";
            var files = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase)
            {
                [aliasPath] = bytes,
            };
            var packPath = System.IO.Path.Combine(
                SkinChangerPaths.CacheDirectory,
                $"base_alias_{hash}.pck");
            SkinChangerPaths.WriteCache(packPath, () => PckArchive.Write(packPath, files));
            if (!ProjectSettings.LoadResourcePack(packPath, replaceFiles: true))
            {
                ModLog.Warn($"[原版] 别名场景包可能已挂载过，继续尝试读取：{packPath}");
            }

            var scene = ResourceLoader.Load<PackedScene>(aliasPath, null, ResourceLoader.CacheMode.Ignore);
            if (scene == null)
            {
                error = "别名场景加载失败：" + aliasPath;
                return null;
            }

            var visuals = scene.Instantiate<NCreatureVisuals>(PackedScene.GenEditState.Disabled);
            if (visuals == null)
            {
                error = "别名场景实例化失败：" + aliasPath;
                return null;
            }

            ModLog.Info(
                $"[原版] 别名场景实例化成功：{aliasPath} ← {scenePath}（{bytes.Length} B，Spine={visuals.HasSpineAnimation}）");
            return visuals;
        }
        catch (Exception exception)
        {
            error = exception.GetBaseException().Message;
            return null;
        }
    }

    // 选人头像按"分组 + 当前选择"缓存：切奥斯提/the 别的分组时不会把别的角色的头像
    // 弄回原皮（覆盖层被重建时那种路径上的文件可能暂时不可用）。
    private static readonly Dictionary<string, Texture2D> CharacterIconCache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 选人头像永远绑定"该角色分组当前选中的皮肤"：按当前选择把该分组的资源包临时挂上，
    /// 读出这张皮肤自己的 <c>char_select_&lt;id&gt;.png</c>，并缓存（键含选择，切换即失效）。
    /// </summary>
    public static Texture2D? GetCharacterSelectIcon(string groupId, string characterId, bool locked)
    {
        if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(characterId))
        {
            return LoadCharacterSelectIcon(characterId, locked);
        }

        var selection = Config.GetSelection(groupId);
        var key = $"{groupId}|{selection}|{locked}";
        if (CharacterIconCache.TryGetValue(key, out var cached) && GodotObject.IsInstanceValid(cached))
        {
            return cached;
        }

        var id = characterId.ToLowerInvariant();
        var path = $"res://images/packed/character_select/char_select_{id}{(locked ? "_locked" : "")}.png";
        Texture2D? icon = null;
        var target = path;
        try
        {
            icon = WithRuntimeResources(groupId, [target],
                resources => resources.TryGetValue(target, out var resource) ? resource as Texture2D : null);
        }
        catch
        {
        }

        icon ??= LoadCharacterSelectIcon(characterId, locked);
        if (icon != null)
        {
            CharacterIconCache[key] = icon;
        }

        return icon;
    }

    /// <summary>某个分组的选择变了就丢掉它的头像缓存。</summary>
    internal static void InvalidateCharacterIconCache(string groupId)
    {
        foreach (var key in CharacterIconCache.Keys
                     .Where(key => key.StartsWith(groupId + "|", StringComparison.OrdinalIgnoreCase))
                     .ToArray())
        {
            CharacterIconCache.Remove(key);
        }
    }

    private static void SynchronizeCompanionSelections()
    {
        foreach (var update in Catalog!.BuildCompanionSelectionUpdates(Config.Selections))
        {
            var previous = Config.GetSelection(update.Key);
            if (previous.Equals(update.Value, StringComparison.OrdinalIgnoreCase)) continue;
            Config.Selections[update.Key] = update.Value;
            ModLog.Info($"已按主人皮肤同步随从外观：{update.Key}，{previous} -> {update.Value}。");
        }
    }

    /// <summary>
    /// Whether the in-run Appearance panel may offer its own skin list for this companion group.
    /// Only once the player decoupled the companion from the owner suite (the companion group owns
    /// an independent selection, e.g. the Osty dropdown in character select) — otherwise the
    /// owner-suite decides, as documented.
    /// </summary>
    internal static bool CanSelectCompanionSkin(string groupId) =>
        // The list must always be reachable for a known summon group. Whether the summon currently
        // follows its owner is a *default selection* question (CompanionAutoFollow), not a reason to
        // hide the whole skin row: with the row hidden the player could not switch away from the
        // independent Osty mod's body in-run.
        Catalog?.IsCompanionGroup(groupId) == true ||
        CompanionGroupIds.Any(id => id.Equals(groupId, StringComparison.OrdinalIgnoreCase)) ||
        groupId.Equals("osty", StringComparison.OrdinalIgnoreCase);
    /// <summary>随从组（目前只有奥斯提）的 id 列表；"随从下拉"在所有角色界面共用这一份。</summary>
    internal static IReadOnlyList<string> CompanionGroupIds =>
        Catalog?.CompanionGroupIds ?? [];

    /// <summary>随从外观当前选择。</summary>
    internal static string GetCompanionSelection(string groupId) => Config.GetSelection(groupId);

    /// <summary>是否还按主人皮肤自动推导随从外观（false = 已解耦）。</summary>
    internal static bool CompanionAutoFollow => Catalog?.CompanionAutoFollow ?? false;

    /// <summary>随从选择发生变化时通知界面（下拉与预览需要跟着刷新）。</summary>
    internal static event Action? CompanionSelectionChanged;

    /// <summary>随从是否处于“随机皮肤”状态。</summary>
    internal static bool GetCompanionRandomEnabled(string groupId)
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            return Config.CompanionRandomEnabled;
        }
    }

    /// <summary>清除“随机皮肤”状态（玩家手动选了别的皮肤时）。</summary>
    internal static void ClearCompanionRandom()
    {
        lock (Sync)
        {
            EnsureConfigLoaded();
            if (!Config.CompanionRandomEnabled)
            {
                return;
            }

            Config.CompanionRandomEnabled = false;
            Config.Save(ConfigPath);
        }
    }

    /// <summary>
    /// 与角色下拉的“随机皮肤”语义一致：进入随机状态后先把随从切回游戏原版显示，
    /// 真正的随机抽取发生在每次选人界面展示时（<see cref="DrawCompanionRandomSkin"/>）。
    /// </summary>
    internal static bool SetCompanionRandomEnabled(string groupId, bool enabled)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                LastError = "皮肤目录尚未初始化。";
                return false;
            }

            Config.CompanionSelectionDecoupled = true;
            Catalog.CompanionAutoFollow = false;
            OstyImpostorCompat.Decoupled = true;
            Config.CompanionRandomEnabled = enabled;
            if (enabled)
            {
                Config.CompanionImpostorSkinId = null;
                OstyImpostorBridge.ApplyQuiet(null);
            }
        }

        var applied = ApplyCompanionGroupSelection(groupId, SkinCatalog.BaseOptionId);
        CompanionSelectionChanged?.Invoke();
        return applied;
    }

    /// <summary>处于随机状态时抽一个随从皮肤并应用。</summary>
    internal static bool DrawCompanionRandomSkin(string groupId)
    {
        if (!GetCompanionRandomEnabled(groupId) || Catalog == null)
        {
            return false;
        }

        var group = Catalog.Groups.FirstOrDefault(candidate =>
            candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
        var candidates = group?.Options.Where(option => !option.IsComposition)
            .Select(option => option.Id).ToArray();
        if (candidates == null || candidates.Length == 0)
        {
            return false;
        }

        var pick = candidates[Random.Shared.Next(candidates.Length)];
        ModLog.Info($"[奥斯提] 随机皮肤抽到 {pick}。");
        return ApplyCompanionSelection(groupId, pick);
    }

    /// <summary>随从组当前选中的皮肤选项（可能为 null = 游戏原版）。</summary>
    internal static SkinOption? GetCompanionSelectedOption(string groupId)
    {
        var group = Catalog?.Groups.FirstOrDefault(candidate =>
            candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
        if (group == null)
        {
            return null;
        }

        var selection = Config.GetSelection(groupId);
        return group.Options.FirstOrDefault(option =>
            !option.IsComposition && option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 独立奥斯提 Mod 在它自己的界面里改了皮肤：同步到本 Mod 的下拉与配置。
    /// 它自带的「原版奥斯提」对应本下拉的「游戏默认」，「撑在地上的鸡煲」对应它那个独立选项。
    /// </summary>
    private static void OnImpostorSelectionChanged(string skinId)
    {
        if (CharSelectTakeoverSuppressor.EchoSuppressed)
        {
            return;
        }

        var isVanilla = string.IsNullOrEmpty(skinId) ||
                        skinId.Equals(OstyImpostorBridge.VanillaSkinId, StringComparison.OrdinalIgnoreCase);
        var groupId = CompanionGroupIds.FirstOrDefault();
        if (groupId == null)
        {
            return;
        }

        lock (Sync)
        {
            if (Catalog == null)
            {
                return;
            }

            Config.CompanionSelectionDecoupled = true;
            Catalog.CompanionAutoFollow = false;
            OstyImpostorCompat.Decoupled = true;
            Config.CompanionImpostorSkinId = isVanilla ? null : skinId;
        }

        ApplyCompanionGroupSelection(groupId, SkinCatalog.BaseOptionId);
        ModLog.Info($"[奥斯提] 已在它自己的界面切到 {skinId}，本 Mod 下拉同步为{(isVanilla ? "游戏默认" : "对应独立选项")}。");
        CompanionSelectionChanged?.Invoke();
    }

    /// <summary>
    /// 手动选择随从外观：写入选择、标记为已解耦（此后不再跟随主人皮肤），并立即应用。
    /// 与角色皮肤走同一套 ApplySelection，因此资源挂载/卸载、预览刷新都复用既有逻辑。
    /// 选到「独立奥斯提 Mod」的皮肤时，osty 组回到游戏原版，由那个 Mod 自己渲染；
    /// 选到皮肤自带的 osty 时，先把那个 Mod 切回它的原版，把位子让出来。
    /// </summary>
    internal static bool ApplyCompanionSelection(string groupId, string optionId)
    {
        if (Catalog == null)
        {
            LastError = "皮肤目录尚未初始化。";
            return false;
        }

        Config.CompanionSelectionDecoupled = true;
        Catalog.CompanionAutoFollow = false;
        OstyImpostorCompat.Decoupled = true;

        string targetOptionId;
        if (OstyImpostorBridge.TryParseOption(optionId, out var impostorSkinId))
        {
            // 独立奥斯提 Mod 接管：osty 组回到原版，避免两边同时挂资源。
            Config.CompanionImpostorSkinId = impostorSkinId;
            OstyImpostorBridge.ApplyQuiet(impostorSkinId);
            targetOptionId = SkinCatalog.BaseOptionId;
        }
        else
        {
            Config.CompanionImpostorSkinId = null;
            OstyImpostorBridge.ApplyQuiet(null);
            targetOptionId = optionId;
        }

        var applied = ApplyCompanionGroupSelection(groupId, targetOptionId);
        if (applied)
        {
            Config.Save(ConfigPath);
            CompanionSelectionChanged?.Invoke();
        }

        return applied;
    }

    /// <summary>
    /// 随从分组的"最小事务"：只写这一个分组、只挂这一个分组的资源，不经过
    /// <see cref="SkinCatalog.BuildVisualSelectionTransaction"/>，因此**不可能**联动到主人分组
    /// （全量运行时提供者、共享骨骼、随从跟随等逻辑全部绕开）。这是"双向彻底解耦"的硬约束。
    /// </summary>
    private static bool ApplyCompanionGroupSelection(string groupId, string optionId)
    {
        lock (Sync)
        {
            if (Catalog == null)
            {
                LastError = "皮肤目录尚未初始化。";
                return false;
            }

            var group = Catalog.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                // No enabled pack ships companion resources right now. The player's choice (for
                // example an independent Osty mod's own skin) is still valid, there is simply nothing
                // for this Mod to mount, so record it as the base selection and succeed.
                Config.Selections[groupId] = SkinCatalog.BaseOptionId;
                Config.Save(ConfigPath);
                LastError = null;
                ModLog.Info(
                    $"随从分组 {groupId} 当前没有可挂载的皮肤资源（没有启用带该随从资源的皮肤包）；" +
                    "本次选择只记录，随从外观由它自己的 Mod 负责。");
                return true;
            }

            if (!optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) &&
                group.Options.All(option =>
                    !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase)))
            {
                LastError = $"未知的随从皮肤选择：{groupId}/{optionId}";
                return false;
            }

            try
            {
                Config.Selections[groupId] = optionId;
                ClearRuntimeResourceCache(groupId);
                MountOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId });
                Config.Save(ConfigPath);
                LastError = null;
                ModLog.Info($"随从分组 {groupId} 独立应用：{optionId}（不影响任何其它分组）。");
                return true;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                ModLog.Error($"切换随从 {groupId} 失败：{exception}");
                return false;
            }
        }
    }

    private static void UpdateVisualProviderPriority(string groupId, string optionId)
    {
        var requestedProviderIds = Catalog!.GetSelectionProviderIds(groupId, optionId);
        SanitizeVisualProviderPriority();
        if (requestedProviderIds.Count == 0)
        {
            return;
        }

        Config.VisualProviderPriority.RemoveAll(providerId =>
            requestedProviderIds.Contains(providerId, StringComparer.OrdinalIgnoreCase));
        Config.VisualProviderPriority.AddRange(
            CharacterSkinCompositionPolicy.BuildProviderPriority(requestedProviderIds));
    }

    private static void SanitizeVisualProviderPriority()
    {
        var selectedProviderIds = Catalog!.Groups
            .SelectMany(group => Catalog.GetSelectionProviderIds(
                group.Id,
                Config.GetSelection(group.Id)))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Config.VisualProviderPriority = Config.VisualProviderPriority
            .Select(providerId => Catalog.ResolveStoredProviderId(
                providerId,
                selectedProviderIds))
            .Where(selectedProviderIds.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var providerId in selectedProviderIds
                     .OrderBy(providerId => providerId, StringComparer.OrdinalIgnoreCase))
        {
            if (!Config.VisualProviderPriority.Contains(
                    providerId,
                    StringComparer.OrdinalIgnoreCase))
            {
                Config.VisualProviderPriority.Insert(0, providerId);
            }
        }
    }

    private static void SanitizeCardSelections()
    {
        var enableAllByDefault = Config.CardPriorityDefaultsVersion < 1;
        foreach (var group in Catalog!.CardGroups)
        {
            GetCardPriorityEntriesInternal(group, enableAllByDefault);
        }

        Config.CardPriorityDefaultsVersion = 1;
    }

    private static void MigrateStoredCardSelections()
    {
        var catalog = Catalog;
        if (catalog == null)
        {
            return;
        }

        var groupsById = catalog.CardGroups.ToDictionary(
            group => group.Id,
            StringComparer.OrdinalIgnoreCase);
        var individualGroupIds = ModelDb.AllCards
            .GroupBy(IndividualCardSelectionKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => GetCardLookup(group.First()).GroupId,
                StringComparer.OrdinalIgnoreCase);
        var migratedCount = 0;
        foreach (var pair in Config.Selections
                     .Where(pair => pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase))
                     .ToArray())
        {
            string? groupId = null;
            if (individualGroupIds.TryGetValue(pair.Key, out var individualGroupId))
            {
                groupId = individualGroupId;
            }
            else if (!pair.Key.StartsWith("cards:item:", StringComparison.OrdinalIgnoreCase))
            {
                var categoryId = pair.Key["cards:".Length..];
                if (groupsById.ContainsKey(categoryId))
                {
                    groupId = categoryId;
                }
            }

            if (groupId == null ||
                pair.Value.Equals(InheritCardSelectionId, StringComparison.OrdinalIgnoreCase) ||
                pair.Value.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var resolvedSelection = catalog.ResolveStoredCardSelectionId(groupId, pair.Value);
            if (resolvedSelection.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Config.Selections[pair.Key] = resolvedSelection;
            migratedCount++;
        }

        if (migratedCount > 0)
        {
            ModLog.Info($"已把 {migratedCount} 项旧单卡设置迁移到具体的同 ID 差分包。");
        }
    }

    private static void SanitizeCardSkinPresets()
    {
        var catalog = Catalog;
        if (catalog == null)
        {
            return;
        }

        var selectionKeysByGroup = catalog.CardGroups.ToDictionary(
            group => group.Id,
            group => new HashSet<string>(
                [CardSelectionKey(group.Id)],
                StringComparer.OrdinalIgnoreCase),
            StringComparer.OrdinalIgnoreCase);
        foreach (var card in ModelDb.AllCards)
        {
            var lookup = GetCardLookup(card);
            foreach (var member in lookup.PriorityGroups)
            {
                if (selectionKeysByGroup.TryGetValue(member.Id, out var keys))
                    keys.Add(IndividualCardSelectionKey(card));
            }
        }

        foreach (var preset in Config.CardSkinPresets.Where(preset =>
                     !string.IsNullOrWhiteSpace(preset.CategoryId)))
        {
            var group = catalog.CardGroups.FirstOrDefault(candidate => candidate.Id.Equals(
                preset.CategoryId,
                StringComparison.OrdinalIgnoreCase));
            if (group == null)
            {
                continue;
            }

            if (preset.CardSkinPriorities.TryGetValue(group.Id, out var priorityEntries))
            {
                preset.CardSkinPriorities[group.Id] = priorityEntries
                    .Select(entry => entry with
                    {
                        OptionId = catalog.ResolveStoredCardSelectionId(
                            group.Id,
                            entry.OptionId)
                    })
                    .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            var selectionKeys = selectionKeysByGroup[group.Id];
            foreach (var pair in preset.Selections
                         .Where(pair => selectionKeys.Contains(pair.Key))
                         .ToArray())
            {
                if (pair.Value.Equals(InheritCardSelectionId, StringComparison.OrdinalIgnoreCase) ||
                    pair.Value.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                preset.Selections[pair.Key] = catalog.ResolveStoredCardSelectionId(
                    group.Id,
                    pair.Value);
            }
        }
    }

    private static void SanitizeMonsterSkinPriorities()
    {
        var knownGroupIds = Catalog!.Groups
            .Select(group => group.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Config.MonsterSkinCategoryGroups = Config.MonsterSkinCategoryGroups
            .ToDictionary(
                pair => pair.Key,
                pair => pair.Value
                    .Where(knownGroupIds.Contains)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);
        var knownCategoryIds = Config.MonsterSkinCategoryGroups.Keys
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var categorizedGroupIds = Config.MonsterSkinCategoryGroups.Values
            .SelectMany(groupIds => groupIds)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Config.MonsterGroupsWithManualSelection = Config.MonsterGroupsWithManualSelection
            .Where(knownGroupIds.Contains)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        Config.MonsterGroupsFollowingCategory = Config.MonsterGroupsFollowingCategory
            .Where(categorizedGroupIds.Contains)
            .Where(groupId => !Config.MonsterGroupsWithManualSelection.Contains(
                groupId,
                StringComparer.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (Config.MonsterPriorityDefaultsVersion < 1)
        {
            foreach (var groupId in categorizedGroupIds.Where(groupId =>
                         !Config.MonsterGroupsFollowingCategory.Contains(
                             groupId,
                             StringComparer.OrdinalIgnoreCase) &&
                         !Config.MonsterGroupsWithManualSelection.Contains(
                             groupId,
                             StringComparer.OrdinalIgnoreCase) &&
                         !Config.GetSelection(groupId).Equals(
                             SkinCatalog.BaseOptionId,
                             StringComparison.OrdinalIgnoreCase)))
            {
                Config.MonsterGroupsWithManualSelection.Add(groupId);
            }

            Config.MonsterPriorityDefaultsVersion = 1;
        }

        if (Config.MonsterPriorityDefaultsVersion < 2)
        {
            foreach (var categoryId in knownCategoryIds)
            {
                var entries = GetMonsterPriorityEntriesInternal(categoryId).ToList();
                if (entries.Count > 0 && entries.All(entry => !entry.Enabled))
                {
                    Config.MonsterSkinPriorities[categoryId] = entries
                        .Select(entry => entry with { Enabled = true })
                        .ToList();
                }
            }

            Config.MonsterPriorityDefaultsVersion = 2;
        }

        Config.EnabledMonsterSkinPriorityCategories.Clear();
        Config.MonsterGroupsFollowingCategory = categorizedGroupIds
            .Where(groupId => !Config.MonsterGroupsWithManualSelection.Contains(
                groupId,
                StringComparer.OrdinalIgnoreCase))
            .ToList();

        foreach (var categoryId in knownCategoryIds)
        {
            _ = GetMonsterPriorityEntriesInternal(categoryId);
            _ = ApplyMonsterCategoryPriorityToSelections(categoryId);
        }
    }

    private static IReadOnlyList<MonsterSkinPriorityEntry> GetMonsterPriorityEntriesInternal(
        string categoryId)
    {
        var options = GetMonsterCategoryOptionsInternal(categoryId);
        var knownIds = options
            .Select(option => option.OptionId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var configuredEntries = Config.MonsterSkinPriorities.TryGetValue(categoryId, out var configured)
            ? configured
            : [];
        configuredEntries = configuredEntries
            .Select(entry => entry with
            {
                OptionId = ResolveStoredMonsterPriorityOptionId(categoryId, entry.OptionId)
            })
            .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var entries = configuredEntries
            .Where(entry => knownIds.Contains(entry.OptionId))
            .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        foreach (var option in options.Where(option => entries.All(entry =>
                     !entry.OptionId.Equals(option.OptionId, StringComparison.OrdinalIgnoreCase))))
        {
            entries.Add(new MonsterSkinPriorityEntry(option.OptionId, Enabled: true));
        }

        Config.MonsterSkinPriorities[categoryId] = MergeKnownMonsterPriorityEntries(
            configuredEntries,
            entries,
            knownIds);
        return entries;
    }

    private static string ResolveStoredMonsterPriorityOptionId(
        string categoryId,
        string optionId)
    {
        var catalog = Catalog;
        if (catalog == null ||
            !Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out var groupIds))
        {
            return optionId;
        }

        foreach (var groupId in groupIds)
        {
            var resolved = catalog.ResolveStoredVisualSelectionId(groupId, optionId);
            if (!resolved.Equals(optionId, StringComparison.OrdinalIgnoreCase))
            {
                return resolved;
            }
        }

        return optionId;
    }

    private static IReadOnlyList<MonsterCategoryOptionState> GetMonsterCategoryOptionsInternal(
        string categoryId)
    {
        var catalog = Catalog;
        if (catalog == null ||
            !Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out var categoryGroupIds))
        {
            return [];
        }

        var groups = categoryGroupIds
            .Select(groupId => catalog.Groups.FirstOrDefault(group =>
                group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)))
            .Where(group => group != null)
            .Cast<SkinGroup>()
            .ToArray();
        var optionOrder = groups
            .SelectMany(group => group.Options)
            .DistinctBy(option => option.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return optionOrder.Select(option => new MonsterCategoryOptionState(
                option.Id,
                option.Name,
                groups.Count(group => group.Options.Any(candidate =>
                    candidate.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase))),
                groups.Length))
            .ToArray();
    }

    private static HashSet<string> ApplyMonsterCategoryPriorityToSelections(string categoryId)
    {
        var catalog = Catalog!;
        if (!Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out var categoryGroupIds))
        {
            return [];
        }

        var previousSelections = new Dictionary<string, string>(
            Config.Selections,
            StringComparer.OrdinalIgnoreCase);
        var workingSelections = new Dictionary<string, string>(
            Config.Selections,
            StringComparer.OrdinalIgnoreCase);
        var entries = GetMonsterPriorityEntriesInternal(categoryId);
        var managedGroupIds = categoryGroupIds
            .Where(groupId => Config.MonsterGroupsFollowingCategory.Contains(
                groupId,
                StringComparer.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var excludedFullProviders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> desiredSelections;
        while (true)
        {
            desiredSelections = managedGroupIds.ToDictionary(
                groupId => groupId,
                groupId =>
                {
                    var group = catalog.Groups.First(candidate =>
                        candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
                    return entries.FirstOrDefault(entry =>
                        entry.Enabled &&
                        !excludedFullProviders.Contains(entry.OptionId) &&
                        group.Options.Any(option => option.Id.Equals(
                            entry.OptionId,
                            StringComparison.OrdinalIgnoreCase)))?.OptionId ??
                        SkinCatalog.BaseOptionId;
                },
                StringComparer.OrdinalIgnoreCase);
            var invalidFullProviders = desiredSelections.Values
                .Where(catalog.ProviderUsesFullRuntime)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(providerId => catalog.GetFullRuntimeProviderGroups(providerId).Any(groupId =>
                    !managedGroupIds.Contains(groupId) ||
                    !desiredSelections.GetValueOrDefault(groupId, SkinCatalog.BaseOptionId)
                        .Equals(providerId, StringComparison.OrdinalIgnoreCase)))
                .Where(excludedFullProviders.Add)
                .ToArray();
            if (invalidFullProviders.Length == 0)
            {
                break;
            }
        }

        var appliedFullProviders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var groupId in managedGroupIds)
        {
            var desiredOptionId = desiredSelections[groupId];
            if (catalog.ProviderUsesFullRuntime(desiredOptionId) &&
                !appliedFullProviders.Add(desiredOptionId))
            {
                continue;
            }

            foreach (var update in catalog.BuildVisualSelectionTransaction(
                         groupId,
                         desiredOptionId,
                         workingSelections))
            {
                workingSelections[update.Key] = update.Value;
            }
        }

        var affectedGroups = workingSelections.Keys
            .Union(previousSelections.Keys, StringComparer.OrdinalIgnoreCase)
            .Where(groupId => !string.Equals(
                workingSelections.GetValueOrDefault(groupId),
                previousSelections.GetValueOrDefault(groupId),
                StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        Config.Selections = workingSelections;
        return affectedGroups;
    }

    private static List<MonsterSkinPriorityEntry> MergeKnownMonsterPriorityEntries(
        IReadOnlyList<MonsterSkinPriorityEntry> existingEntries,
        IReadOnlyList<MonsterSkinPriorityEntry> knownEntries,
        IReadOnlySet<string> knownIds)
    {
        var pendingKnown = new Queue<MonsterSkinPriorityEntry>(knownEntries);
        var result = new List<MonsterSkinPriorityEntry>(
            Math.Max(existingEntries.Count, knownEntries.Count));
        var preservedUnknownIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var existing in existingEntries)
        {
            if (knownIds.Contains(existing.OptionId))
            {
                if (pendingKnown.Count > 0)
                {
                    result.Add(pendingKnown.Dequeue());
                }

                continue;
            }

            if (preservedUnknownIds.Add(existing.OptionId))
            {
                result.Add(existing);
            }
        }

        while (pendingKnown.Count > 0)
        {
            result.Add(pendingKnown.Dequeue());
        }

        return result;
    }

    private static IReadOnlyList<CardSkinPriorityEntry> GetCardPriorityEntriesInternal(
        CardSkinGroup group,
        bool enableAllByDefault = false)
    {
        var knownIds = group.Options
            .Select(option => option.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        List<CardSkinPriorityEntry> entries;
        var configuredEntries = Config.CardSkinPriorities.TryGetValue(group.Id, out var configured)
            ? configured
            : [];
        configuredEntries = configuredEntries
            .Select(entry => entry with
            {
                OptionId = Catalog!.ResolveStoredCardSelectionId(group.Id, entry.OptionId)
            })
            .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (configuredEntries.Count > 0)
        {
            if (enableAllByDefault)
            {
                configuredEntries = configuredEntries
                    .Select(entry => entry with { Enabled = true })
                    .ToList();
            }

            entries = configuredEntries
                .Where(entry => knownIds.Contains(entry.OptionId))
                .DistinctBy(entry => entry.OptionId, StringComparer.OrdinalIgnoreCase)
                .ToList();
            foreach (var option in group.Options.Where(option => entries.All(entry =>
                         !entry.OptionId.Equals(option.Id, StringComparison.OrdinalIgnoreCase))))
            {
                entries.Add(new CardSkinPriorityEntry(option.Id,
                    Enabled: configuredEntries.Any(entry => entry.Enabled)));
            }
        }
        else
        {
            var selectionKey = CardSelectionKey(group.Id);
            var hasLegacySelection = Config.Selections.TryGetValue(selectionKey, out var legacySelection);
            var selectedId = hasLegacySelection
                ? Catalog!.ResolveStoredCardSelectionId(group.Id, legacySelection!)
                : group.Options.FirstOrDefault()?.Id ?? SkinCatalog.BaseOptionId;
            entries = group.Options
                .OrderByDescending(option => option.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase))
                .Select(option => new CardSkinPriorityEntry(
                    option.Id,
                    Enabled: enableAllByDefault || !hasLegacySelection ||
                             !selectedId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        Config.CardSkinPriorities[group.Id] = MergeKnownCardPriorityEntries(
            configuredEntries,
            entries,
            knownIds);
        Config.Selections[CardSelectionKey(group.Id)] =
            entries.FirstOrDefault(entry => entry.Enabled)?.OptionId ?? SkinCatalog.BaseOptionId;
        return entries;
    }

    private static List<CardSkinPriorityEntry> MergeKnownCardPriorityEntries(
        IReadOnlyList<CardSkinPriorityEntry> existingEntries,
        IReadOnlyList<CardSkinPriorityEntry> knownEntries,
        IReadOnlySet<string> knownIds)
    {
        var pendingKnown = new Queue<CardSkinPriorityEntry>(knownEntries);
        var result = new List<CardSkinPriorityEntry>(
            Math.Max(existingEntries.Count, knownEntries.Count));
        var preservedUnknownIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var existing in existingEntries)
        {
            if (knownIds.Contains(existing.OptionId))
            {
                if (pendingKnown.Count > 0)
                {
                    result.Add(pendingKnown.Dequeue());
                }

                continue;
            }

            if (preservedUnknownIds.Add(existing.OptionId))
            {
                result.Add(existing);
            }
        }

        while (pendingKnown.Count > 0)
        {
            result.Add(pendingKnown.Dequeue());
        }

        return result;
    }

}

internal sealed record MountedProviderPackState(
    IReadOnlyList<string> ResourcePaths);

/// <summary>某一层覆盖包给某个路径提供的具体来源（归档 + 包内路径）。</summary>
internal readonly record struct OverlayOwner(string ArchivePath, string Path);

internal sealed record LargeProviderMountPlan(
    IReadOnlySet<string> PromotedPackPaths,
    IReadOnlySet<string> PromotedProviderIds);

internal sealed record AncientLayeredImageTextures(
    Texture2D Character,
    Texture2D? BackgroundCover,
    Texture2D? Mask,
    Texture2D? SleepingCharacter);

internal sealed record CardPriorityOptionState(
    string OptionId,
    string Name,
    bool Enabled,
    int ColorIndex,
    int Coverage,
    int TotalCards);

internal sealed record CardSkinPresetState(
    string Name,
    bool Active,
    string? BundleName = null)
{
    public string DisplayName => BundleName ?? Name;
    public bool IsBundlePreset => BundleName != null;
}

internal sealed record MonsterSkinPresetState(
    string Name,
    bool Active,
    string? BundleName = null)
{
    public string DisplayName => BundleName ?? Name;
    public bool IsBundlePreset => BundleName != null;
}

internal sealed record MonsterPriorityOptionState(
    string OptionId,
    string Name,
    bool Enabled,
    int ColorIndex,
    int Coverage,
    int TotalMonsters);

internal sealed record MonsterCategoryOptionState(
    string OptionId,
    string Name,
    int Coverage,
    int TotalMonsters);

internal sealed record CardSkinSourceState(
    string OptionId,
    string Name,
    bool Enabled,
    int ColorIndex,
    bool IsCurrent);

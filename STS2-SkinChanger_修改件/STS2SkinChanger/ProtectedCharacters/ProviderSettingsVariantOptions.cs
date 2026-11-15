// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using System.Collections.Immutable;
using System.Globalization;
using Godot;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Catalog;

/// <summary>
/// Settings-driven appearance variants. Some skin packs keep a single character scene and expose
/// their palette/style variants through their own settings (JoanofArcIroncladHpwear: 原色 /
/// 黑贞德 / Saber 王庭蓝 / 霓虹审判官). Players rarely read the mod page, so those variants stay
/// invisible and the author's extra work goes unnoticed.
///
/// This turns every variant into a first-class dropdown option named
/// <c>&lt;skin&gt;_&lt;variant label&gt;</c>, the same way <see cref="ProviderVariantOptions"/>
/// treats packs that ship one scene per variant. The plain option is not kept as an extra entry
/// when the pack's own default is one of the labelled variants: that default variant simply takes
/// the plain option's id over, so a saved selection keeps working and the list has exactly one
/// entry per variant.
///
/// Detection is capability/name based and never provider specific; every step is optional except
/// the settings object itself and its label list:
/// <list type="bullet">
///   <item>a static <c>Current</c>/<c>Settings</c>/<c>Instance</c>/... member (property, field or
///         parameterless getter) that returns the settings object,</item>
///   <item>a settable <c>int</c> or enum property on it whose name reads like an appearance
///         channel (Appearance / Palette / Variant / Style / Look / Form / Outfit / ...),</item>
///   <item>labels for that channel: a static <c>string[]</c>/<c>List&lt;string&gt;</c> named like
///         the channel and initialized in the type's static constructor, or the enum's members,</item>
///   <item>optionally the pack's own write path (a static <c>Persist&lt;Channel&gt;</c>-style
///         delegate) and a static <c>Refresh*(SceneTree)</c> hook.</item>
/// </list>
/// A pack that does not match is left completely untouched.
///
/// Two re-exports of one pack share an assembly identity, so only one copy of the code can execute.
/// The rules that keep such a pair working are generic and live here / in the loader:
/// <list type="bullet">
///   <item><b>richest wins</b> — <see cref="FindRicherSiblingAssemblyPath"/> compares the number of
///         metadata type definitions and lets the fuller build run, whatever was selected. The slimmer
///         re-export keeps supplying its own art; its palette writes go through the executor below.</item>
///   <item><b>one owner per behaviour</b> — the player/damage-stage gate only answers for the package
///         that owns the selection, so one pack's clothing stages never land on the other's model.</item>
///   <item><b>executor for the other one</b> — when the selected package cannot run, the same-contract
///         loaded package writes that package's settings on its behalf (<see cref="Apply"/>).</item>
///   <item><b>shared resources</b> — variants of one package ship identical files, so the runtime
///         overlay is keyed by the family base id (<see cref="IsSamePackageVariant"/>), not per variant.</item>
/// </list>
/// </summary>
internal static class ProviderSettingsVariantOptions
{
    private const string VariantMarker = "::variant:";
    private const int MaximumVariants = 16;

    /// <summary>Names of the integer channel the pack switches its look with, best first.</summary>
    private static readonly string[] ChannelHints =
    [
        "appearance", "appearances", "variant", "palette", "outfit", "costume", "style",
        "look", "form", "visual", "skin", "preset", "color", "colour", "theme", "model"
    ];

    /// <summary>Static members that hand out the live settings object.</summary>
    private static readonly string[] InstanceMemberHints =
        ["Current", "Instance", "Settings", "Config", "Configuration", "State", "Data", "Runtime", "Defaults"];

    /// <summary>Label container names, best first.</summary>
    private static readonly string[] LabelFieldHints =
    [
        "names", "labels", "variants", "appearances", "palettes", "styles", "looks", "forms",
        "outfits", "costumes", "presets", "colors", "colours", "themes", "options", "modes"
    ];

    /// <summary>Prefixes of the pack's own write delegate, e.g. PersistAppearance / SetVariant.</summary>
    private static readonly string[] PersistHints = ["Persist", "Set", "Save", "Apply", "Write", "Update"];

    // Options added for a provider's variants, keyed by the synthetic option id.
    private static readonly Dictionary<string, SettingsVariantBinding> Bindings =
        new(StringComparer.OrdinalIgnoreCase);

    // One scan per provider package.
    private static readonly Dictionary<string, SettingsVariantContract?> Contracts =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly HashSet<string> AppliedLogKeys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether this option id is a palette/style variant produced by this scanner. Those variants
    /// reuse the provider's single scene and only change the pack's own settings, so they must
    /// never be treated as "<c>&lt;group&gt;_&lt;variant&gt;.tscn</c>" scene siblings.
    /// </summary>
    internal static bool IsSettingsVariant(string? optionId) =>
        !string.IsNullOrWhiteSpace(optionId) &&
        Bindings.ContainsKey(optionId);

    /// <summary>
    /// One dropdown entry produced from a pack's own appearance setting, together with the setting
    /// value it maps to. Ordered so a caller can translate an observed setting value back into the
    /// option the Skin Changer should be showing (that is how the pack's own variant hotkey keeps
    /// the dropdown in sync).
    /// </summary>
    internal readonly record struct VariantSlot(
        string OptionId,
        SettingsVariantContract Contract,
        int Index);

    internal static IReadOnlyList<VariantSlot> Slots() =>
        Bindings
            .Select(pair => new VariantSlot(pair.Key, pair.Value.Contract, pair.Value.Index))
            .ToArray();

    /// <summary>The package that owns this variant option, or null when it is not one of ours.</summary>
    internal static SettingsVariantContract? ContractOf(string? optionId) =>
        !string.IsNullOrWhiteSpace(optionId) && Bindings.TryGetValue(optionId, out var binding)
            ? binding.Contract
            : null;

    /// <summary>
    /// 0-based variant index of this option inside its own package (the pack's 1-based appearance value
    /// minus one). Used for the per-variant portrait folders.
    /// </summary>
    internal static int? VariantIndex(string? optionId) =>
        !string.IsNullOrWhiteSpace(optionId) && Bindings.TryGetValue(optionId, out var binding)
            ? binding.Index
            : null;

    /// <summary>
    /// True when both options are appearance variants of the same package: only the pack's settings value
    /// changes, the shipped resources are identical, so the runtime overlay does not need a rebuild.
    /// </summary>
    internal static bool IsSamePackageVariant(string? previous, string? next)
    {
        if (string.IsNullOrWhiteSpace(previous) || string.IsNullOrWhiteSpace(next))
        {
            return false;
        }

        if (ContractOf(previous) is not { } before || ContractOf(next) is not { } after)
        {
            return false;
        }

        return before.AssemblyPath.Equals(after.AssemblyPath, StringComparison.OrdinalIgnoreCase) &&
               FamilyBaseId(previous).Equals(FamilyBaseId(next), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Whether the package's own assembly is actually loaded. A second package that ships the same
    /// assembly identity (Joan 0.3.5-S next to 0.3.6) can never be loaded, so its palette code does
    /// not run: its variants may only be switched through this Mod, never by writing the value into
    /// the other package's settings.
    /// </summary>
    internal static bool IsPackageLoaded(SettingsVariantContract contract) =>
        ResolvePackageAssembly(contract) != null;

    /// <summary>Base option id of a variant family: the option without the <c>::variant:</c> tail.</summary>
    internal static string FamilyBaseId(string optionId)
    {
        var marker = optionId.LastIndexOf(VariantMarker, StringComparison.OrdinalIgnoreCase);
        return marker < 0 ? optionId : optionId[..marker];
    }

    /// <summary>
    /// The variant options that belong to <paramref name="optionId"/>'s own package and family,
    /// ordered by their setting value. This is what a quick-switch may cycle: another package's
    /// variants never appear here even when both packs are installed and both are Ironclad skins.
    /// </summary>
    internal static IReadOnlyList<string> Family(string? optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return [];
        }

        var baseId = FamilyBaseId(optionId);
        return Bindings
            .Where(pair => FamilyBaseId(pair.Key).Equals(baseId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(pair => pair.Value.Index)
            .Select(pair => pair.Key)
            .ToArray();
    }

    /// <summary>Next option inside the same family, or null when the family has nothing to cycle.</summary>
    internal static string? NextFamilyOption(string? current, int direction)
    {
        var family = Family(current);
        return family.Count < 2 ? null : SkinOptionCycle.NextOption(family, current, direction);
    }

    /// <summary>
    /// The package whose settings hold the live values for this variant channel: the package itself
    /// when its assembly is loaded, otherwise a same-contract package that is (the two Joan packs
    /// share one code base, so one of them is always loaded and owns the global value).
    /// </summary>
    internal static bool TryGetExecutorContract(
        SettingsVariantContract contract,
        out SettingsVariantContract executor)
    {
        if (ResolvePackageAssembly(contract) != null)
        {
            executor = contract;
            return true;
        }

        return TryResolveExecutor(contract, out executor, out _);
    }

    /// <summary>
    /// The hotkey the package's own settings hold for its appearance channel (Joan:
    /// <c>AppearanceHotkey</c>, default <c>=</c>). Read from the live settings object, so a player
    /// who rebinds it in the RitsuLib page gets the new binding. Null when the package defines none.
    /// </summary>
    internal static string? ReadHotkeyBinding(SettingsVariantContract contract)
    {
        var key = contract.AssemblyPath + "\n" + contract.ProviderId;
        if (HotkeyBindings.TryGetValue(key, out var cached))
        {
            return cached;
        }

        string? binding = null;
        try
        {
            var executor = TryGetExecutorContract(contract, out var resolved) ? resolved : contract;
            if (ResolvePackageAssembly(executor) is { } assembly &&
                assembly.GetType(executor.RuntimeTypeName, throwOnError: false) is { } runtimeType &&
                assembly.GetType(executor.SettingsTypeName, throwOnError: false) is { } settingsType &&
                ResolveSettingsInstance(executor, runtimeType) is { } instance)
            {
                binding = settingsType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(property => property.PropertyType == typeof(string) && property.CanRead &&
                                       property.Name.Contains("Hotkey", StringComparison.OrdinalIgnoreCase))
                    // Prefer the appearance channel over any other hotkey the pack may expose.
                    .OrderByDescending(property =>
                        property.Name.Contains("Appearance", StringComparison.OrdinalIgnoreCase) ||
                        property.Name.Contains("Variant", StringComparison.OrdinalIgnoreCase))
                    .Select(property =>
                    {
                        try
                        {
                            return property.GetValue(instance) as string;
                        }
                        catch
                        {
                            return null;
                        }
                    })
                    .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            }
        }
        catch
        {
            binding = null;
        }

        // Do not cache a negative: the package may be loaded later in the session.
        if (!string.IsNullOrWhiteSpace(binding))
        {
            HotkeyBindings[key] = binding;
        }

        return binding;
    }

    private static readonly Dictionary<string, string?> HotkeyBindings =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The variant contract detected for one package assembly path, or null.</summary>
    internal static SettingsVariantContract? ContractForAssembly(string? assemblyPath) =>
        string.IsNullOrWhiteSpace(assemblyPath)
            ? null
            : Contracts.Values.FirstOrDefault(contract =>
                contract != null &&
                contract.AssemblyPath.Equals(assemblyPath, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Two contracts describe the same palette channel when every identifying part matches: assembly
    /// name, settings type, property and the full label list. That is the definition of "the same code
    /// base re-exported as another package" used by the executor fallback and by the pack gate
    /// compatibility patch.
    /// </summary>
    internal static bool SharesContractShape(
        SettingsVariantContract left,
        SettingsVariantContract right) =>
        left.AssemblyName.Equals(right.AssemblyName, StringComparison.Ordinal) &&
        left.SettingsTypeName.Equals(right.SettingsTypeName, StringComparison.Ordinal) &&
        left.PropertyName.Equals(right.PropertyName, StringComparison.Ordinal) &&
        left.Labels.Count == right.Labels.Count &&
        left.Labels.Zip(right.Labels)
            .All(pair => pair.First.Equals(pair.Second, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// When several re-exports of the same pack are installed, only one assembly can execute (.NET keeps
    /// one copy per identity). Whatever the player selected, the <b>fullest</b> build should be the one
    /// running: the slimmer re-export is usually a stripped re-export that lacks the damaged-stage
    /// (clothing) behaviour, and letting it win made that behaviour disappear from its sibling.
    /// Returns the assembly path of the fuller sibling, or null when this one already is the fullest.
    /// </summary>
    internal static string? FindRicherSiblingAssemblyPath(string assemblyPath)
    {
        try
        {
            var mine = ContractForAssembly(assemblyPath);
            if (mine == null)
            {
                return null;
            }

            var mineTypes = CountTypeDefinitions(assemblyPath);
            string? best = null;
            var bestTypes = mineTypes;
            foreach (var slot in Slots())
            {
                var candidatePath = slot.Contract.AssemblyPath;
                if (candidatePath.Equals(assemblyPath, StringComparison.OrdinalIgnoreCase) ||
                    !File.Exists(candidatePath) ||
                    !SharesContractShape(mine, slot.Contract))
                {
                    continue;
                }

                var types = CountTypeDefinitions(candidatePath);
                if (types > bestTypes)
                {
                    bestTypes = types;
                    best = candidatePath;
                }
            }

            return best;
        }
        catch
        {
            return null;
        }
    }

    private static readonly Dictionary<string, int> TypeCountCache =
        new(StringComparer.OrdinalIgnoreCase);

    private static int CountTypeDefinitions(string assemblyPath)
    {
        lock (TypeCountCache)
        {
            if (TypeCountCache.TryGetValue(assemblyPath, out var cached))
            {
                return cached;
            }
        }

        var count = 0;
        try
        {
            using var stream = File.OpenRead(assemblyPath);
            using var peReader = new System.Reflection.PortableExecutable.PEReader(stream);
            if (peReader.HasMetadata)
            {
                count = peReader.GetMetadataReader().TypeDefinitions.Count;
            }
        }
        catch
        {
            count = 0;
        }

        lock (TypeCountCache)
        {
            TypeCountCache[assemblyPath] = count;
        }

        return count;
    }

    private static object? ResolveSettingsInstance(SettingsVariantContract contract, Type runtimeType)
    {
        try
        {
            if (contract.InstanceMemberIsMethod)
            {
                return runtimeType
                    .GetMethod(
                        contract.InstanceMemberName,
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        Type.EmptyTypes,
                        null)
                    ?.Invoke(null, null);
            }

            var property = runtimeType.GetProperty(
                contract.InstanceMemberName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                return property.GetValue(null);
            }

            return runtimeType.GetField(
                contract.InstanceMemberName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>The setting value the provider currently holds for this contract, or null.</summary>
    internal static int? ReadCurrentIndex(SettingsVariantContract contract)
    {
        // Reference identity only: the previous value comparison by type/property/assembly name
        // let one package's reading answer for the other one, which is how the two Joan packs ended
        // up driving each other's palette.
        var binding = Bindings.Values.FirstOrDefault(candidate =>
            ReferenceEquals(candidate.Contract, contract));
        return binding != null && binding.TryReadCurrentIndex(out var index) ? index : null;
    }

    // A name-only assembly lookup is the one thing this file must never do: it silently returns the
    // other package when two packs ship the same assembly identity.
    private static readonly Dictionary<string, Assembly> ResolvedPackages =
        new(StringComparer.OrdinalIgnoreCase);

    private static Assembly? ResolvePackageAssembly(SettingsVariantContract contract)
    {
        if (ResolvedPackages.TryGetValue(contract.AssemblyPath, out var cached))
        {
            return cached;
        }

        // 1) The exact package the Skin Changer executed. Provider DLLs are often loaded from a
        //    memory stream (empty Location), so this cache is the only reliable identity.
        if (ManagedSkinModLoader.TryGetLoadedProviderAssembly(contract.AssemblyPath, out var loaded) &&
            loaded != null)
        {
            ResolvedPackages[contract.AssemblyPath] = loaded;
            return loaded;
        }

        // 2) A provider the game's own mod loader executed from disk.
        var wanted = NormalizeAssemblyPath(contract.AssemblyPath);
        foreach (var candidate in AppDomain.CurrentDomain.GetAssemblies())
        {
            string location;
            try
            {
                location = candidate.IsDynamic ? string.Empty : candidate.Location ?? string.Empty;
            }
            catch
            {
                continue;
            }

            if (location.Length > 0 &&
                NormalizeAssemblyPath(location).Equals(wanted, StringComparison.OrdinalIgnoreCase))
            {
                ResolvedPackages[contract.AssemblyPath] = candidate;
                return candidate;
            }
        }

        // A different package may hold the same simple name (the Skin Changer deliberately keeps
        // the second DLL from executing). That assembly belongs to the other package, so it is not
        // a substitute: report the package as not loaded instead of reading or writing its values.
        return null;
    }

    private static string NormalizeAssemblyPath(string path) =>
        path.Replace('\\', '/').TrimEnd('/');

    internal static void Expand(
        IReadOnlyCollection<PckResourceIndex> indexes,
        IDictionary<string, SkinGroup> groups)
    {
        foreach (var index in indexes)
        {
            if (!index.Mod.HasDll || !Directory.Exists(index.Mod.RootPath))
            {
                continue;
            }

            var candidates = groups.Values
                .SelectMany(group => group.Options
                    .Where(option =>
                        !option.Id.Contains(VariantMarker, StringComparison.OrdinalIgnoreCase) &&
                        !option.IsComposition &&
                        !option.IsCharacterIconOnly &&
                        option.EffectiveProviderId.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase))
                    .Select(option => (Group: group, Option: option)))
                .Where(candidate =>
                    // Character packs only: the variants below swap the creature's palette.
                    candidate.Option.Assets.Values.Any(asset => asset.SourcePath.StartsWith(
                        "res://scenes/creature_visuals/",
                        StringComparison.OrdinalIgnoreCase)))
                .ToArray();
            if (candidates.Length == 0)
            {
                continue;
            }

            // ResourceNamespaceId is the manifest id when the package has one, which is what
            // identifies the package: two re-exports of the same pack share the folder/PCK namespace
            // but must stay separate variant channels.
            var contract = ResolveContract(index.Mod.RootPath, index.Mod.ResourceNamespaceId);
            if (contract == null || contract.Labels.Count < 2)
            {
                continue;
            }

            foreach (var (group, option) in candidates)
            {
                var at = group.Options.IndexOf(option);
                if (at < 0)
                {
                    continue;
                }

                // A pack that already exposes sibling scene variants was expanded by
                // ProviderVariantOptions; do not stack a second set of palette options on top.
                if (group.Options.Any(existing => existing.Id.StartsWith(
                        option.Id + VariantMarker,
                        StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var inserted = 0;
                var baseIndex = contract.DefaultIndex;
                if (baseIndex >= 0 && baseIndex < contract.Labels.Count)
                {
                    // The pack's own default is one of the labelled variants: let the plain option
                    // become that variant instead of keeping a duplicate unsuffixed entry.
                    group.Options[at] = option with
                    {
                        Name = option.Name + "_" + contract.Labels[baseIndex]
                    };
                    Bindings[option.Id] = new SettingsVariantBinding(contract, baseIndex);
                }
                else
                {
                    baseIndex = -1;
                }

                for (var variant = 0; variant < contract.Labels.Count; variant++)
                {
                    if (variant == baseIndex)
                    {
                        continue;
                    }

                    var label = contract.Labels[variant];
                    if (string.IsNullOrWhiteSpace(label))
                    {
                        continue;
                    }

                    var id = option.Id + VariantMarker + variant.ToString(CultureInfo.InvariantCulture);
                    if (group.Options.Any(existing => existing.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    group.Options.Insert(++at, option with
                    {
                        Id = id,
                        Name = option.Name + "_" + label,
                        ProviderId = option.EffectiveProviderId
                    });
                    Bindings[id] = new SettingsVariantBinding(contract, variant);
                    inserted++;
                }

                if (inserted > 0 || baseIndex >= 0)
                {
                    ModLog.Info(
                        $"已把 {option.Name} 的 {contract.Labels.Count} 个外观变体注册为独立皮肤选项：" +
                        $"名称={option.Name}_{string.Join("、", contract.Labels)}" +
                        (baseIndex >= 0 ? $"（原选项即「{contract.Labels[baseIndex]}」）" : string.Empty) +
                        $"；设置={contract.SettingsTypeName}.{contract.PropertyName}" +
                        $"（{contract.AssemblyPath}）。");
                }
            }
        }
    }

    /// <summary>
    /// Asks every provider whose variant option is currently selected to switch to that variant.
    /// Called on every overlay rebuild so a restart, a run start or a later scene load all end up
    /// on the saved variant without persisting anything into the provider ourselves.
    /// </summary>
    internal static void ApplyActiveSelections(IReadOnlyDictionary<string, string> selections)
    {
        if (Bindings.Count == 0)
        {
            return;
        }

        foreach (var pair in selections)
        {
            if (!Bindings.TryGetValue(pair.Value, out var binding))
            {
                continue;
            }

            // The palette of these packs is one global value, so it must follow ONLY the local
            // player's own choice. A rebuild that runs inside another player's scope (a teammate, a
            // remote portrait) used to write that player's variant here, which is what made both
            // clients converge on the same variant after a save-and-load.
            if (MultiplayerSkinSync.GetScopedSelection(pair.Key) != null)
            {
                continue;
            }

            if (!SkinService.Config.GetSelection(pair.Key)
                    .Equals(pair.Value, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                if (!binding.Apply())
                {
                    WarnUnloadablePackage(binding.Contract, pair.Value);
                }
            }
            catch (Exception exception)
            {
                ModLog.Warn(
                    $"切换 {binding.Contract.SettingsTypeName} 的外观变体失败，保持游戏内当前外观：" +
                    exception.GetBaseException().Message);
            }
        }
    }

    private static readonly HashSet<string> UnloadablePackageWarnings =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Finds a loaded package that can execute a variant for a package whose own assembly lost the
    /// identity race. The shapes must match exactly (assembly name, settings type, property and the
    /// full label list), which is only true for re-exports of the same code base — the two Joan
    /// packs. The dropdown and the hotkey sync stay per package; only the palette value is shared,
    /// exactly like inside the pack where one global value drives every drawn target.
    /// </summary>
    private static bool TryResolveExecutor(
        SettingsVariantContract contract,
        out SettingsVariantContract executor,
        out Assembly executorAssembly)
    {
        executor = null!;
        executorAssembly = null!;
        foreach (var candidate in Bindings.Values.Select(binding => binding.Contract).Distinct())
        {
            if (ReferenceEquals(candidate, contract) ||
                !candidate.AssemblyName.Equals(contract.AssemblyName, StringComparison.Ordinal) ||
                !candidate.SettingsTypeName.Equals(contract.SettingsTypeName, StringComparison.Ordinal) ||
                !candidate.PropertyName.Equals(contract.PropertyName, StringComparison.Ordinal) ||
                candidate.Labels.Count != contract.Labels.Count ||
                !candidate.Labels
                    .Zip(contract.Labels)
                    .All(pair => pair.First.Equals(pair.Second, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (ResolvePackageAssembly(candidate) is not { } assembly)
            {
                continue;
            }

            executor = candidate;
            executorAssembly = assembly;
            return true;
        }

        return false;
    }

    private static readonly HashSet<string> DelegatedPackageLog =
        new(StringComparer.OrdinalIgnoreCase);

    private static void LogDelegatedOnce(SettingsVariantContract executor)
    {
        var pack = Path.GetFileName(executor.AssemblyPath);
        if (!DelegatedPackageLog.Add(pack))
        {
            return;
        }

        ModLog.Info(
            $"另一个贞德皮肤包（{pack}）占用了同名程序集，未加载的那个包的外观变体现在由它代为执行；" +
            "两包共用同一套调色板，所以外观仍会正常切换。");
    }

    private static void WarnUnloadablePackage(SettingsVariantContract contract, string optionId)
    {
        var label = Path.GetFileName(contract.AssemblyPath);
        if (!UnloadablePackageWarnings.Add(label + "\n" + optionId))
        {
            return;
        }

        ModLog.Warn(
            $"选了 {optionId}，但 {label} 的程序集没有加载，" +
            "也找不到能代它执行外观的同契约皮肤包；本次只改了下拉显示，外观不会变化。");
    }

    /// <summary>
    /// Diagnostics hook (RuntimeTests `--audit-settings-variants`): what this scanner detects in one
    /// provider assembly, without touching the catalog or the game.
    /// </summary>
    internal static SettingsVariantContract? ScanAssemblyForDiagnostics(string assemblyPath) =>
        File.Exists(assemblyPath)
            ? SettingsVariantScanner.Scan(assemblyPath, Path.GetFileNameWithoutExtension(assemblyPath))
            : null;

    private static SettingsVariantContract? ResolveContract(string providerRoot, string providerId)
    {
        var key = providerRoot + "\n" + providerId;
        if (Contracts.TryGetValue(key, out var cached))
        {
            return cached;
        }

        SettingsVariantContract? contract = null;
        try
        {
            foreach (var assemblyPath in EnumerateCandidateAssemblies(providerRoot, providerId))
            {
                try
                {
                    contract = SettingsVariantScanner.Scan(assemblyPath, providerId);
                }
                catch (Exception exception)
                {
                    ModLog.Warn(
                        $"读取 {Path.GetFileName(assemblyPath)} 的外观变体设置失败：" +
                        exception.GetBaseException().Message);
                }

                if (contract != null)
                {
                    break;
                }
            }
        }
        finally
        {
            Contracts[key] = contract;
        }

        return contract;
    }

    private static IEnumerable<string> EnumerateCandidateAssemblies(string root, string providerId)
    {
        string primary;
        try
        {
            primary = SkinPackagePaths.Resolve(root, providerId, ".dll");
        }
        catch
        {
            primary = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(primary) && File.Exists(primary))
        {
            yield return primary;
            yield break;
        }

        if (!Directory.Exists(root))
        {
            yield break;
        }

        foreach (var path in Directory.EnumerateFiles(root, "*.dll", SearchOption.TopDirectoryOnly))
        {
            yield return path;
        }
    }

    private sealed record SettingsVariantBinding(SettingsVariantContract Contract, int Index)
    {
        /// <summary>
        /// Reads the provider's current appearance value and maps it back to a variant index. Used by
        /// the hotkey bridge: the pack changes its own setting when its hotkey fires, and the Skin
        /// Changer has to show the matching dropdown entry.
        /// </summary>
        internal bool TryReadCurrentIndex(out int index)
        {
            index = -1;
            var assembly = ResolvePackageAssembly(Contract);
            if (assembly == null)
            {
                return false;
            }

            var runtimeType = assembly.GetType(Contract.RuntimeTypeName, throwOnError: false);
            var settingsType = assembly.GetType(Contract.SettingsTypeName, throwOnError: false);
            if (runtimeType == null || settingsType == null)
            {
                return false;
            }

            var current = ResolveInstance(Contract, runtimeType);
            if (current == null)
            {
                return false;
            }

            var property = settingsType.GetProperty(
                Contract.PropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanRead)
            {
                return false;
            }

            object? raw;
            try
            {
                raw = property.GetValue(current);
            }
            catch
            {
                return false;
            }

            if (raw == null)
            {
                return false;
            }

            if (property.PropertyType.IsEnum)
            {
                // Enum labels are matched by name first; fall back to the declared value table.
                var name = Enum.GetName(property.PropertyType, raw);
                for (var i = 0; i < Contract.Labels.Count; i++)
                {
                    if (string.Equals(Contract.Labels[i], name, StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        return true;
                    }
                }

                var numeric = Convert.ToInt32(raw, CultureInfo.InvariantCulture);
                var at = -1;
                for (var i = 0; i < Contract.EnumValues.Count; i++)
                {
                    if (Contract.EnumValues[i] == numeric)
                    {
                        at = i;
                        break;
                    }
                }
                if (at >= 0)
                {
                    index = at;
                    return true;
                }

                return false;
            }

            if (property.PropertyType != typeof(int))
            {
                return false;
            }

            var value = Convert.ToInt32(raw, CultureInfo.InvariantCulture);
            var baseOffset = ResolveBaseOffset(settingsType, property);
            var offset = value - baseOffset;
            if (offset < 0 || offset >= Contract.Labels.Count)
            {
                offset = value;
            }

            if (offset < 0 || offset >= Contract.Labels.Count)
            {
                return false;
            }

            index = offset;
            return true;
        }

        internal bool Apply()
        {
            if (ResolvePackageAssembly(Contract) is { } own)
            {
                return ApplyTo(Contract, own, Index);
            }

            // Two packs can ship the same assembly identity (Joan 0.3.5-S next to 0.3.6). .NET keeps
            // only one of them, so the other pack's palette code never runs and its variants would
            // never change anything. Both packs use the same palette channel and the same label
            // list, so the loaded one can execute the value on their behalf: the dropdown stays per
            // pack (see ContractKey / VariantHotkeyBridge), only the palette value is shared, which
            // it already is inside the game.
            if (!TryResolveExecutor(Contract, out var executor, out var executorAssembly))
            {
                return false;
            }

            var index = IndexInExecutor(executor);
            LogDelegatedOnce(executor);
            return ApplyTo(executor, executorAssembly, index);
        }

        private bool ApplyTo(SettingsVariantContract contract, Assembly assembly, int index)
        {
            var runtimeType = assembly.GetType(contract.RuntimeTypeName, throwOnError: false);
            var settingsType = assembly.GetType(contract.SettingsTypeName, throwOnError: false);
            if (runtimeType == null || settingsType == null)
            {
                return false;
            }

            var current = ResolveInstance(contract, runtimeType);
            if (current == null)
            {
                return false;
            }

            var property = settingsType.GetProperty(
                contract.PropertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null || !property.CanWrite)
            {
                return false;
            }

            if (property.PropertyType.IsEnum)
            {
                return ApplyEnum(contract, index, current, property, assembly);
            }

            if (property.PropertyType != typeof(int))
            {
                return false;
            }

            var persist = ResolvePersister(contract, runtimeType);
            var writer = persist ?? (value => property.SetValue(current, value));

            var baseOffset = ResolveBaseOffset(settingsType, property);
            var desired = baseOffset + index;
            if ((int)(property.GetValue(current) ?? 0) == desired)
            {
                return true;
            }

            // The provider clamps its own range; try the plausible offsets and keep the one it
            // actually accepted so a 0-based or 1-based pack both land on the selected label.
            foreach (var candidate in new[] { desired, index, index + 1 })
            {
                if (candidate < 0 || candidate > contract.Labels.Count + 1)
                {
                    continue;
                }

                writer(candidate);
                if ((int)(property.GetValue(current) ?? 0) == candidate)
                {
                    Refresh(assembly);
                    LogOnce(contract, index, candidate, desired);
                    return true;
                }
            }

            ModLog.Warn(
                $"无法让 {settingsType.Name}.{contract.PropertyName} 切到变体「{contract.Labels.ElementAtOrDefault(index)}」；" +
                "该皮肤包的外观设置可能与预期不同。");
            return false;
        }

        /// <summary>Variant index of this binding's label inside a same-contract executor.</summary>
        private int IndexInExecutor(SettingsVariantContract executor)
        {
            var label = Contract.Labels.ElementAtOrDefault(Index);
            for (var i = 0; i < executor.Labels.Count; i++)
            {
                if (string.Equals(executor.Labels[i], label, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return Math.Clamp(Index, 0, Math.Max(0, executor.Labels.Count - 1));
        }

        private bool ApplyEnum(
            SettingsVariantContract contract,
            int index,
            object current,
            PropertyInfo property,
            Assembly assembly)
        {
            var enumType = property.PropertyType;
            var values = Enum.GetValues(enumType).Cast<object>().ToArray();
            var names = Enum.GetNames(enumType);
            object? desired = null;
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i].Equals(contract.Labels[index], StringComparison.OrdinalIgnoreCase))
                {
                    desired = values[i];
                    break;
                }
            }

            desired ??= contract.EnumValues.Count == values.Length
                ? Enum.ToObject(enumType, contract.EnumValues[index])
                : values.ElementAtOrDefault(index);
            if (desired == null || Equals(property.GetValue(current), desired))
            {
                return true;
            }

            property.SetValue(current, desired);
            Refresh(assembly);
            LogOnce(contract, index, Convert.ToInt32(desired, CultureInfo.InvariantCulture), Convert.ToInt32(desired, CultureInfo.InvariantCulture));
            return true;
        }

        private object? ResolveInstance(SettingsVariantContract contract, Type runtimeType)
        {
            try
            {
                if (contract.InstanceMemberIsMethod)
                {
                    return runtimeType
                        .GetMethod(
                            contract.InstanceMemberName,
                            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                            null,
                            Type.EmptyTypes,
                            null)
                        ?.Invoke(null, null);
                }

                var property = runtimeType.GetProperty(
                    contract.InstanceMemberName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (property != null)
                {
                    return property.GetValue(null);
                }

                return runtimeType.GetField(
                    contract.InstanceMemberName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);
            }
            catch
            {
                return null;
            }
        }

        private Action<int>? ResolvePersister(SettingsVariantContract contract, Type runtimeType)
        {
            if (string.IsNullOrWhiteSpace(contract.PersistMemberName))
            {
                return null;
            }

            try
            {
                var property = runtimeType.GetProperty(
                    contract.PersistMemberName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                if (property?.GetValue(null) is Action<int> fromProperty)
                {
                    return fromProperty;
                }

                return runtimeType.GetField(
                    contract.PersistMemberName,
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null)
                    as Action<int>;
            }
            catch
            {
                return null;
            }
        }

        private void LogOnce(SettingsVariantContract contract, int index, int applied, int desired)
        {
            var key = contract.SettingsTypeName + "\n" + applied;
            if (!AppliedLogKeys.Add(key))
            {
                return;
            }

            ModLog.Info(
                $"已把 {contract.SettingsTypeName} 的外观变体切到「{contract.Labels.ElementAtOrDefault(index)}」" +
                $"（值={applied}{(applied == desired ? string.Empty : "，已按皮肤包自身的取值范围校正")}）。");
        }

        private void Refresh(Assembly assembly)
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null)
            {
                return;
            }

            // The pack's own re-apply hook is usually a static helper next to the palette code
            // (Palette.RefreshAppearance(SceneTree) in JoanofArcIroncladHpwear), not on the
            // settings holder. Search the assembly once and remember it.
            if (_refreshMethod == null && !_refreshSearched)
            {
                _refreshSearched = true;
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(
                                 BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        if ((method.Name.StartsWith("Refresh", StringComparison.Ordinal) ||
                             method.Name.StartsWith("Apply", StringComparison.Ordinal)) &&
                            method.GetParameters() is [{ ParameterType: var parameterType }] &&
                            parameterType == typeof(SceneTree))
                        {
                            _refreshMethod = method;
                            break;
                        }
                    }

                    if (_refreshMethod != null)
                    {
                        break;
                    }
                }
            }

            try
            {
                _refreshMethod?.Invoke(null, [tree]);
            }
            catch
            {
                // The pack also re-reads its settings on its own; ignore refresh failures.
            }
        }

        private MethodInfo? _refreshMethod;
        private bool _refreshSearched;

        private static int ResolveBaseOffset(Type settingsType, PropertyInfo property)
        {
            try
            {
                if (settingsType.GetConstructor(Type.EmptyTypes) != null &&
                    Activator.CreateInstance(settingsType) is { } defaults &&
                    property.GetValue(defaults) is int value)
                {
                    return value >= 1 ? 1 : 0;
                }
            }
            catch
            {
                // Fall through to the common 1-based layout.
            }

            return 1;
        }
    }

    /// <summary>
    /// Stable identity of one package's variant channel. Two packages can ship the same assembly
    /// identity and the same settings type (<c>JoanofArcIroncladHpwear</c> 0.3.5-S and 0.3.6, or any
    /// re-exported pack), so the package's own assembly path is part of the key: a variant observed
    /// through one package must never be adopted by, or written into, the other one.
    /// </summary>
    internal static string ContractKey(SettingsVariantContract contract) =>
        contract.AssemblyPath + "\n" + contract.ProviderId + "\n" +
        contract.SettingsTypeName + "\n" + contract.PropertyName;

    /// <summary>The shape detected in one provider assembly.</summary>
    internal sealed record SettingsVariantContract(
        string AssemblyPath,
        string AssemblyName,
        string ProviderId,
        string RuntimeTypeName,
        string InstanceMemberName,
        bool InstanceMemberIsMethod,
        string SettingsTypeName,
        string PropertyName,
        bool PropertyIsEnum,
        IReadOnlyList<int> EnumValues,
        string? PersistMemberName,
        IReadOnlyList<string> Labels,
        int DefaultIndex);

    /// <summary>Metadata-only reader: provider assemblies are never loaded just to inspect them.</summary>
    private static class SettingsVariantScanner
    {
        private static readonly IReadOnlyDictionary<ushort, OpCode> OpCodesByValue =
            typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.FieldType == typeof(OpCode))
                .Select(field => (OpCode)field.GetValue(null)!)
                .ToDictionary(opCode => unchecked((ushort)opCode.Value));

        private sealed record TypeInfo(TypeDefinition Definition, string Name);

        private sealed record IntChannel(
            string SettingsTypeName,
            string PropertyName,
            bool IsEnum,
            string? EnumTypeName,
            int Score);

        private sealed record PersistChannel(string TypeName, string MemberName, string Suffix);

        private sealed record LabelsCandidate(
            int Score,
            string FieldName,
            MethodDefinitionHandle Cctor,
            FieldDefinitionHandle Field);

        internal static SettingsVariantContract? Scan(string assemblyPath, string providerId)
        {
            using var stream = new System.IO.FileStream(
                assemblyPath,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read,
                System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete);
            using var peReader = new PEReader(stream, PEStreamOptions.PrefetchEntireImage);
            if (!peReader.HasMetadata)
            {
                return null;
            }

            var reader = peReader.GetMetadataReader();
            var types = reader.TypeDefinitions
                .Select(handle => reader.GetTypeDefinition(handle))
                .Select(type => new TypeInfo(type, TypeName(reader, type)))
                .ToArray();

            var channels = CollectIntChannels(reader, types);
            if (channels.Count == 0)
            {
                return null;
            }

            var persists = CollectPersistChannels(reader, types);

            foreach (var channel in channels.OrderByDescending(channel => channel.Score))
            {
                var instance = FindInstanceProvider(reader, types, channel);
                if (instance == null)
                {
                    continue;
                }

                var labels = channel.IsEnum && channel.EnumTypeName != null
                    ? ReadEnumLabels(reader, types, channel.EnumTypeName, out var enumValues)
                    : FindStringLabels(peReader, reader, types, channel, hasPersist: persists.Any(candidate => candidate.Suffix.Equals(channel.PropertyName, StringComparison.OrdinalIgnoreCase)));
                if (labels.Count is < 2 or > MaximumVariants)
                {
                    continue;
                }

                var persist = persists.FirstOrDefault(candidate =>
                    candidate.Suffix.Equals(channel.PropertyName, StringComparison.OrdinalIgnoreCase));
                var defaultIndex = ResolveDefaultIndex(
                    peReader,
                    reader,
                    types,
                    channel,
                    labels.Count,
                    out var defaultEnumIndex);
                if (channel.IsEnum)
                {
                    defaultIndex = defaultEnumIndex;
                }

                return new SettingsVariantContract(
                    Path.GetFullPath(assemblyPath),
                    // The real assembly name from metadata, not the file name: a re-export can ship a
                    // differently named DLL (JoanofArcIroncladHpwear111.dll) whose assembly identity is
                    // still JoanofArcIroncladHpwear. Contract shape comparisons must use the identity.
                    ReadAssemblyName(reader, assemblyPath),
                    providerId,
                    instance.Value.TypeName,
                    instance.Value.MemberName,
                    instance.Value.IsMethod,
                    channel.SettingsTypeName,
                    channel.PropertyName,
                    channel.IsEnum,
                    channel.IsEnum && channel.EnumTypeName != null
                        ? ReadEnumValues(reader, types, channel.EnumTypeName)
                        : [],
                    persist?.MemberName,
                    labels,
                    defaultIndex);
            }

            return null;
        }

        private static string ReadAssemblyName(MetadataReader reader, string assemblyPath)
        {
            try
            {
                return reader.GetString(reader.GetAssemblyDefinition().Name);
            }
            catch
            {
                // Netmodules and malformed images have no assembly row; the file name is the best
                // available identity there.
                return Path.GetFileNameWithoutExtension(assemblyPath);
            }
        }

        private static List<IntChannel> CollectIntChannels(MetadataReader reader, TypeInfo[] types)
        {
            var channels = new List<IntChannel>();
            foreach (var type in types)
            {
                foreach (var propertyHandle in type.Definition.GetProperties())
                {
                    var property = reader.GetPropertyDefinition(propertyHandle);
                    if (!HasSetter(reader, property, isStatic: false))
                    {
                        continue;
                    }

                    var propertyName = reader.GetString(property.Name);
                    var returnType = Decode(reader, property).ReturnType;
                    var isEnum = false;
                    string? enumTypeName = null;
                    if (returnType == "System.Int32")
                    {
                        // Plain integer channel.
                    }
                    else if (types.Any(candidate => candidate.Name.Equals(returnType, StringComparison.Ordinal) &&
                                                    candidate.Definition.BaseType.Kind == HandleKind.TypeReference &&
                                                    TypeReferenceName(reader, candidate.Definition.BaseType)
                                                        .Equals("System.Enum", StringComparison.Ordinal)))
                    {
                        isEnum = true;
                        enumTypeName = returnType;
                    }
                    else
                    {
                        continue;
                    }

                    var score = ChannelHintScore(propertyName);
                    if (score == 0)
                    {
                        continue;
                    }

                    // "VisualSelectionDefaultsVersion" is a version counter, not a look channel.
                    if (SplitWords(propertyName).Any(word => DenyChannelWords.Contains(word)))
                    {
                        continue;
                    }

                    channels.Add(new IntChannel(type.Name, propertyName, isEnum, enumTypeName, score));
                }
            }

            return channels;
        }

        private static List<PersistChannel> CollectPersistChannels(MetadataReader reader, TypeInfo[] types)
        {
            var channels = new List<PersistChannel>();
            foreach (var type in types)
            {
                foreach (var propertyHandle in type.Definition.GetProperties())
                {
                    var property = reader.GetPropertyDefinition(propertyHandle);
                    if (!HasSetter(reader, property, isStatic: true))
                    {
                        continue;
                    }

                    if (!IsIntOrEnumDelegate(Decode(reader, property).ReturnType))
                    {
                        continue;
                    }

                    AddPersist(type, reader.GetString(property.Name), channels);
                }

                foreach (var fieldHandle in type.Definition.GetFields())
                {
                    var field = reader.GetFieldDefinition(fieldHandle);
                    if (!field.Attributes.HasFlag(FieldAttributes.Static))
                    {
                        continue;
                    }

                    if (!IsIntOrEnumDelegate(field.DecodeSignature(SignatureNames.Instance, null)))
                    {
                        continue;
                    }

                    AddPersist(type, reader.GetString(field.Name), channels);
                }
            }

            return channels;
        }

        private static void AddPersist(TypeInfo type, string memberName, List<PersistChannel> channels)
        {
            foreach (var hint in PersistHints)
            {
                if (!memberName.StartsWith(hint, StringComparison.OrdinalIgnoreCase) ||
                    memberName.Length <= hint.Length)
                {
                    continue;
                }

                channels.Add(new PersistChannel(type.Name, memberName, memberName[hint.Length..]));
                return;
            }
        }

        private static bool IsIntOrEnumDelegate(string typeName) =>
            (typeName.Contains("System.Action`1", StringComparison.Ordinal) ||
             typeName.Contains("System.Func`1", StringComparison.Ordinal)) &&
            typeName.Contains("System.Int32", StringComparison.Ordinal);

        private static int ChannelHintScore(string propertyName)
        {
            var words = SplitWords(propertyName);
            for (var i = 0; i < ChannelHints.Length; i++)
            {
                var hint = ChannelHints[i];
                if (words.Any(word => MatchesWord(word, hint)))
                {
                    return ChannelHints.Length - i;
                }
            }

            return 0;
        }

        /// <summary>
        /// PascalCase/underscore aware matching: <c>Appearance</c>/<c>AppearanceIndex</c> are an
        /// appearance channel, <c>Format</c> is not (a plain substring test treated it as "form").
        /// </summary>
        private static bool MatchesWord(string word, string hint) =>
            word.Equals(hint, StringComparison.Ordinal) ||
            word.Equals(hint + "s", StringComparison.Ordinal) ||
            word + "s" == hint;

        private static IReadOnlyList<string> SplitWords(string value)
        {
            var words = new List<string>();
            var start = 0;
            for (var index = 1; index < value.Length; index++)
            {
                if (value[index] is '_' or '-' ||
                    char.IsUpper(value[index]) && !char.IsUpper(value[index - 1]))
                {
                    if (index > start)
                    {
                        words.Add(value[start..index].Trim('_', '-').ToLowerInvariant());
                    }

                    start = value[index] is '_' or '-' ? index + 1 : index;
                }
            }

            if (start < value.Length)
            {
                words.Add(value[start..].Trim('_', '-').ToLowerInvariant());
            }

            return words;
        }

        private static (string TypeName, string MemberName, bool IsMethod)? FindInstanceProvider(
            MetadataReader reader,
            TypeInfo[] types,
            IntChannel channel)
        {
            foreach (var hint in InstanceMemberHints)
            {
                foreach (var type in types)
                {
                    foreach (var propertyHandle in type.Definition.GetProperties())
                    {
                        var property = reader.GetPropertyDefinition(propertyHandle);
                        if (!HasGetter(reader, property, isStatic: true) ||
                            !reader.GetString(property.Name).Equals(hint, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (Decode(reader, property).ReturnType.Equals(channel.SettingsTypeName, StringComparison.Ordinal))
                        {
                            return (type.Name, hint, false);
                        }
                    }

                    foreach (var fieldHandle in type.Definition.GetFields())
                    {
                        var field = reader.GetFieldDefinition(fieldHandle);
                        if (!field.Attributes.HasFlag(FieldAttributes.Static) ||
                            !reader.GetString(field.Name).Equals(hint, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (field.DecodeSignature(SignatureNames.Instance, null)
                            .Equals(channel.SettingsTypeName, StringComparison.Ordinal))
                        {
                            return (type.Name, hint, false);
                        }
                    }

                    foreach (var methodHandle in type.Definition.GetMethods())
                    {
                        var method = reader.GetMethodDefinition(methodHandle);
                        if (!method.Attributes.HasFlag(MethodAttributes.Static) ||
                            !reader.GetString(method.Name).Equals(hint, StringComparison.OrdinalIgnoreCase) ||
                            method.GetParameters().Count != 0)
                        {
                            continue;
                        }

                        if (Decode(reader, method).ReturnType.Equals(channel.SettingsTypeName, StringComparison.Ordinal))
                        {
                            return (type.Name, hint, true);
                        }
                    }
                }
            }

            // Last resort: any static member that hands out the settings object.
            foreach (var type in types)
            {
                foreach (var propertyHandle in type.Definition.GetProperties())
                {
                    var property = reader.GetPropertyDefinition(propertyHandle);
                    if (!HasGetter(reader, property, isStatic: true))
                    {
                        continue;
                    }

                    if (Decode(reader, property).ReturnType.Equals(channel.SettingsTypeName, StringComparison.Ordinal))
                    {
                        return (type.Name, reader.GetString(property.Name), false);
                    }
                }
            }

            return null;
        }

        private static IReadOnlyList<string> FindStringLabels(
            PEReader peReader,
            MetadataReader reader,
            TypeInfo[] types,
            IntChannel channel,
            bool hasPersist)
        {
            var propertyName = channel.PropertyName;
            var channelWords = SplitWords(propertyName).ToHashSet(StringComparer.Ordinal);
            var appearanceChannel = channelWords.Any(word =>
                AppearanceChannelWords.Contains(word));
            var candidates = new List<LabelsCandidate>();
            foreach (var type in types)
            {
                var cctor = default(MethodDefinitionHandle);
                foreach (var methodHandle in type.Definition.GetMethods())
                {
                    if (reader.GetString(reader.GetMethodDefinition(methodHandle).Name) == ".cctor")
                    {
                        cctor = methodHandle;
                        break;
                    }
                }

                if (cctor.IsNil)
                {
                    continue;
                }

                foreach (var fieldHandle in type.Definition.GetFields())
                {
                    var field = reader.GetFieldDefinition(fieldHandle);
                    if (!field.Attributes.HasFlag(FieldAttributes.Static))
                    {
                        continue;
                    }

                    var fieldType = field.DecodeSignature(SignatureNames.Instance, null);
                    if (!IsStringSequence(fieldType))
                    {
                        continue;
                    }

                    var fieldName = reader.GetString(field.Name);
                    var fieldWords = SplitWords(fieldName).ToHashSet(StringComparer.Ordinal);

                    // Two ways an array can be this channel's label list:
                    //  - its name carries the channel word (AppearanceNames / VariantLabels), or
                    //  - it is a bare generic list (Names / Labels) *and* the pack exposes its own
                    //    write path for that channel (Persist<Channel>). The second rule keeps
                    //    unrelated arrays such as an "InputActionNames" from being picked up.
                    var nameRelated = fieldWords.Overlaps(channelWords) &&
                                      (appearanceChannel || hasPersist);
                    var bareGeneric = fieldWords.Count == 1 && GenericLabelWords.Contains(fieldWords.First());
                    if (!nameRelated && !(bareGeneric && hasPersist && appearanceChannel))
                    {
                        continue;
                    }

                    var score = LabelHintScore(fieldName) +
                                (nameRelated ? 40 : 0) +
                                (fieldName.Contains(propertyName, StringComparison.OrdinalIgnoreCase) ? 20 : 0);
                    candidates.Add(new LabelsCandidate(score, fieldName, cctor, fieldHandle));
                }
            }

            foreach (var candidate in candidates.OrderByDescending(candidate => candidate.Score))
            {
                var labels = ReadStaticStringArray(peReader, reader, candidate.Cctor, candidate.Field);
                if (labels.Count is >= 2 and <= MaximumVariants)
                {
                    return labels;
                }
            }

            return [];
        }

        /// <summary>Words that make a plain "Names"/"Labels" array a credible variant list.</summary>
        private static readonly HashSet<string> GenericLabelWords = new(StringComparer.Ordinal)
        {
            "names", "labels", "variants", "appearances", "palettes", "styles", "looks",
            "forms", "outfits", "costumes", "presets", "options", "modes", "colors", "colours", "themes"
        };

        private static readonly HashSet<string> AppearanceChannelWords = new(StringComparer.Ordinal)
        {
            "appearance", "appearances", "variant", "palette", "outfit", "costume", "style", "look"
        };

        /// <summary>Words that mark a numeric setting as something other than a look channel.</summary>
        private static readonly HashSet<string> DenyChannelWords = new(StringComparer.Ordinal)
        {
            "version", "revision", "rev", "count", "length", "size", "hash", "seed", "checksum",
            "enabled", "disabled", "flag", "flags", "max", "min", "limit", "timeout", "cache",
            "tick", "ticks", "ms", "fps", "scale", "offset", "alpha", "volume", "opacity", "speed",
            "delay", "duration", "width", "height", "depth", "level", "hp", "damage", "priority", "weight"
        };

        private static bool IsStringSequence(string typeName) =>
            typeName is "System.String[]" or
                "System.Collections.Generic.List<System.String>" or
                "System.Collections.Generic.IReadOnlyList<System.String>" or
                "System.Collections.Generic.IEnumerable<System.String>";

        private static int LabelHintScore(string fieldName)
        {
            var words = SplitWords(fieldName);
            for (var i = 0; i < LabelFieldHints.Length; i++)
            {
                var hint = LabelFieldHints[i];
                if (words.Any(word => MatchesWord(word, hint)))
                {
                    return LabelFieldHints.Length - i;
                }
            }

            return 0;
        }

        private static IReadOnlyList<string> ReadEnumLabels(
            MetadataReader reader,
            TypeInfo[] types,
            string enumTypeName,
            out IReadOnlyList<int> enumValues)
        {
            enumValues = [];
            var type = types.FirstOrDefault(candidate =>
                candidate.Name.Equals(enumTypeName, StringComparison.Ordinal));
            if (type == null)
            {
                return [];
            }

            var names = new List<string>();
            var values = new List<int>();
            foreach (var fieldHandle in type.Definition.GetFields())
            {
                var field = reader.GetFieldDefinition(fieldHandle);
                if (!field.Attributes.HasFlag(FieldAttributes.Literal))
                {
                    continue;
                }

                var name = reader.GetString(field.Name);
                if (name == "value__")
                {
                    continue;
                }

                names.Add(name);
                values.Add(TryReadConstant(reader, field.GetDefaultValue()));
            }

            enumValues = values;
            return names;
        }

        private static IReadOnlyList<int> ReadEnumValues(
            MetadataReader reader,
            TypeInfo[] types,
            string enumTypeName)
        {
            _ = ReadEnumLabels(reader, types, enumTypeName, out var values);
            return values;
        }

        private static int TryReadConstant(MetadataReader reader, ConstantHandle handle)
        {
            try
            {
                if (handle.IsNil)
                {
                    return 0;
                }

                var constant = reader.GetConstant(handle);
                var blob = reader.GetBlobReader(constant.Value);
                return constant.TypeCode switch
                {
                    ConstantTypeCode.SByte => blob.ReadSByte(),
                    ConstantTypeCode.Byte => blob.ReadByte(),
                    ConstantTypeCode.Int16 => blob.ReadInt16(),
                    ConstantTypeCode.UInt16 => blob.ReadUInt16(),
                    ConstantTypeCode.Int32 => blob.ReadInt32(),
                    ConstantTypeCode.UInt32 => unchecked((int)blob.ReadUInt32()),
                    _ => 0
                };
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// Index of the variant the pack starts with, read from the settings constructor. Keeping
        /// the plain option alive as that variant is only correct when the default really is one of
        /// the labelled values.
        /// </summary>
        private static int ResolveDefaultIndex(
            PEReader peReader,
            MetadataReader reader,
            TypeInfo[] types,
            IntChannel channel,
            int labelCount,
            out int enumDefaultIndex)
        {
            enumDefaultIndex = -1;
            var type = types.FirstOrDefault(candidate =>
                candidate.Name.Equals(channel.SettingsTypeName, StringComparison.Ordinal));
            if (type == null)
            {
                return -1;
            }

            var defaultValues = new List<int>();
            foreach (var methodHandle in type.Definition.GetMethods())
            {
                var method = reader.GetMethodDefinition(methodHandle);
                if (reader.GetString(method.Name) != ".ctor" ||
                    method.Attributes.HasFlag(MethodAttributes.Static))
                {
                    continue;
                }

                // Prefer the parameterless constructor; a ctor with arguments often assigns from
                // them and would not describe the pack's defaults.
                if (method.GetParameters().Count != 0)
                {
                    continue;
                }

                defaultValues.AddRange(ReadCtorAssignedInts(peReader, reader, method, channel.PropertyName));
            }

            if (defaultValues.Count == 0)
            {
                return -1;
            }

            var value = defaultValues[^1];
            if (channel.IsEnum)
            {
                var values = ReadEnumValues(reader, types, channel.EnumTypeName!);
                var position = values.ToList().IndexOf(value);
                enumDefaultIndex = position is >= 0 and < MaximumVariants ? position : -1;
                return enumDefaultIndex;
            }

            // 0-based packs default to 0; 1-based packs default to 1 (or the first variant).
            var offset = value >= 1 ? 1 : 0;
            var index = value - offset;
            return index >= 0 && index < labelCount ? index : -1;
        }

        private static IEnumerable<int> ReadCtorAssignedInts(
            PEReader peReader,
            MetadataReader reader,
            MethodDefinition method,
            string propertyName)
        {
            var result = new List<int>();
            if (method.RelativeVirtualAddress == 0)
            {
                return result;
            }

            var il = peReader.GetMethodBody(method.RelativeVirtualAddress).GetILBytes();
            if (il == null)
            {
                return result;
            }

            var lastInt = int.MinValue;
            var offset = 0;
            while (offset < il.Length)
            {
                var first = il[offset++];
                var value = first == 0xfe && offset < il.Length
                    ? (ushort)(0xfe00 | il[offset++])
                    : first;
                if (!OpCodesByValue.TryGetValue(value, out var opCode))
                {
                    return result;
                }

                var operandOffset = offset;
                var operandSize = GetOperandSize(opCode.OperandType, il, operandOffset);
                if (operandSize < 0 || operandOffset + operandSize > il.Length)
                {
                    return result;
                }

                if (opCode.OperandType == OperandType.InlineField && operandSize == 4 &&
                    opCode.Equals(OpCodes.Stfld))
                {
                    var token = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
                        il.AsSpan(operandOffset, 4));
                    try
                    {
                        var field = reader.GetFieldDefinition(MetadataTokens.FieldDefinitionHandle(token));
                        var fieldName = reader.GetString(field.Name);
                        if (fieldName.Contains(propertyName, StringComparison.OrdinalIgnoreCase) &&
                            lastInt != int.MinValue)
                        {
                            result.Add(lastInt);
                        }
                    }
                    catch
                    {
                        // Ignore malformed tokens.
                    }
                }
                else if (opCode.Equals(OpCodes.Ldc_I4_0)) lastInt = 0;
                else if (opCode.Equals(OpCodes.Ldc_I4_1)) lastInt = 1;
                else if (opCode.Equals(OpCodes.Ldc_I4_2)) lastInt = 2;
                else if (opCode.Equals(OpCodes.Ldc_I4_3)) lastInt = 3;
                else if (opCode.Equals(OpCodes.Ldc_I4_4)) lastInt = 4;
                else if (opCode.Equals(OpCodes.Ldc_I4_5)) lastInt = 5;
                else if (opCode.Equals(OpCodes.Ldc_I4_6)) lastInt = 6;
                else if (opCode.Equals(OpCodes.Ldc_I4_7)) lastInt = 7;
                else if (opCode.Equals(OpCodes.Ldc_I4_8)) lastInt = 8;
                else if ((opCode.Equals(OpCodes.Ldc_I4_S) || opCode.Equals(OpCodes.Ldc_I4)) && operandSize > 0)
                {
                    lastInt = opCode.Equals(OpCodes.Ldc_I4_S)
                        ? il[operandOffset]
                        : System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
                            il.AsSpan(operandOffset, 4));
                }

                offset = operandOffset + operandSize;
            }

            return result;
        }

        private static List<string> ReadStaticStringArray(
            PEReader peReader,
            MetadataReader reader,
            MethodDefinitionHandle cctorHandle,
            FieldDefinitionHandle targetField)
        {
            var labels = new List<string>();
            var cctor = reader.GetMethodDefinition(cctorHandle);
            if (cctor.RelativeVirtualAddress == 0)
            {
                return labels;
            }

            var il = peReader.GetMethodBody(cctor.RelativeVirtualAddress).GetILBytes();
            if (il == null)
            {
                return labels;
            }

            var pending = new List<string>();
            var arrayLength = -1;
            var lastInt = -1;
            var offset = 0;
            while (offset < il.Length)
            {
                var first = il[offset++];
                var value = first == 0xfe && offset < il.Length
                    ? (ushort)(0xfe00 | il[offset++])
                    : first;
                if (!OpCodesByValue.TryGetValue(value, out var opCode))
                {
                    return [];
                }

                var operandOffset = offset;
                var operandSize = GetOperandSize(opCode.OperandType, il, operandOffset);
                if (operandSize < 0 || operandOffset + operandSize > il.Length)
                {
                    return [];
                }

                if (opCode.OperandType == OperandType.InlineString && operandSize == 4)
                {
                    var token = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
                        il.AsSpan(operandOffset, 4));
                    try
                    {
                        pending.Add(reader.GetUserString(MetadataTokens.UserStringHandle(token & 0x00FFFFFF)));
                    }
                    catch
                    {
                        // Ignore malformed string tokens.
                    }
                }
                else if (opCode.OperandType == OperandType.InlineField && operandSize == 4)
                {
                    var token = System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
                        il.AsSpan(operandOffset, 4));
                    var fieldHandle = MetadataTokens.FieldDefinitionHandle(token);
                    if (opCode.Equals(OpCodes.Stsfld) &&
                        !fieldHandle.IsNil &&
                        fieldHandle == targetField)
                    {
                        var take = arrayLength > 0 ? Math.Min(arrayLength, pending.Count) : pending.Count;
                        return pending.Take(take).Where(text => !string.IsNullOrWhiteSpace(text)).ToList();
                    }

                    pending.Clear();
                }
                else if (opCode.Equals(OpCodes.Newarr))
                {
                    arrayLength = lastInt;
                }
                else if (opCode.Equals(OpCodes.Ldc_I4_0)) lastInt = 0;
                else if (opCode.Equals(OpCodes.Ldc_I4_1)) lastInt = 1;
                else if (opCode.Equals(OpCodes.Ldc_I4_2)) lastInt = 2;
                else if (opCode.Equals(OpCodes.Ldc_I4_3)) lastInt = 3;
                else if (opCode.Equals(OpCodes.Ldc_I4_4)) lastInt = 4;
                else if (opCode.Equals(OpCodes.Ldc_I4_5)) lastInt = 5;
                else if (opCode.Equals(OpCodes.Ldc_I4_6)) lastInt = 6;
                else if (opCode.Equals(OpCodes.Ldc_I4_7)) lastInt = 7;
                else if (opCode.Equals(OpCodes.Ldc_I4_8)) lastInt = 8;
                else if ((opCode.Equals(OpCodes.Ldc_I4_S) || opCode.Equals(OpCodes.Ldc_I4)) && operandSize > 0)
                {
                    lastInt = opCode.Equals(OpCodes.Ldc_I4_S)
                        ? il[operandOffset]
                        : System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(
                            il.AsSpan(operandOffset, 4));
                }

                offset = operandOffset + operandSize;
            }

            return [];
        }

        private static bool HasSetter(MetadataReader reader, PropertyDefinition property, bool isStatic) =>
            HasAccessor(reader, property.GetAccessors().Setter, isStatic);

        private static bool HasGetter(MetadataReader reader, PropertyDefinition property, bool isStatic) =>
            HasAccessor(reader, property.GetAccessors().Getter, isStatic);

        private static bool HasAccessor(MetadataReader reader, MethodDefinitionHandle handle, bool isStatic)
        {
            if (handle.IsNil)
            {
                return false;
            }

            var method = reader.GetMethodDefinition(handle);
            return method.Attributes.HasFlag(MethodAttributes.Static) == isStatic;
        }

        private static MethodSignature<string> Decode(MetadataReader reader, PropertyDefinition property) =>
            property.DecodeSignature(SignatureNames.Instance, null);

        private static MethodSignature<string> Decode(MetadataReader reader, MethodDefinition method) =>
            method.DecodeSignature(SignatureNames.Instance, null);

        private static string TypeName(MetadataReader reader, TypeDefinition type)
        {
            var name = reader.GetString(type.Name);
            var ns = reader.GetString(type.Namespace);
            return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        }

        private static string TypeReferenceName(MetadataReader reader, EntityHandle handle)
        {
            if (handle.Kind != HandleKind.TypeReference)
            {
                return string.Empty;
            }

            var type = reader.GetTypeReference((TypeReferenceHandle)handle);
            var name = reader.GetString(type.Name);
            var ns = reader.GetString(type.Namespace);
            return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
        }

        private static int GetOperandSize(OperandType operandType, byte[] il, int offset) => operandType switch
        {
            OperandType.InlineNone => 0,
            OperandType.ShortInlineBrTarget or
            OperandType.ShortInlineI or
            OperandType.ShortInlineVar => 1,
            OperandType.InlineVar => 2,
            OperandType.InlineBrTarget or
            OperandType.InlineField or
            OperandType.InlineI or
            OperandType.InlineMethod or
            OperandType.InlineSig or
            OperandType.InlineString or
            OperandType.InlineTok or
            OperandType.InlineType or
            OperandType.ShortInlineR => 4,
            OperandType.InlineI8 or
            OperandType.InlineR => 8,
            OperandType.InlineSwitch when offset + 4 <= il.Length =>
                4 + checked(System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(offset, 4)) * 4),
            _ => -1
        };

        private sealed class SignatureNames : ISignatureTypeProvider<string, MetadataReader?>
        {
            internal static readonly SignatureNames Instance = new();

            public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[]";

            public string GetByReferenceType(string elementType) => elementType + "&";

            public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";

            public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) =>
                typeArguments.Length == 0
                    ? genericType
                    : genericType + "<" + string.Join(",", typeArguments) + ">";

            public string GetGenericMethodParameter(MetadataReader? genericContext, int index) => "!!" + index;

            public string GetGenericTypeParameter(MetadataReader? genericContext, int index) => "!" + index;

            public string GetModifiedType(string modifierType, string unmodifiedType, bool isRequired) => unmodifiedType;

            public string GetPinnedType(string elementType) => elementType;

            public string GetPointerType(string elementType) => elementType + "*";

            public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode switch
            {
                PrimitiveTypeCode.Int32 => "System.Int32",
                PrimitiveTypeCode.String => "System.String",
                PrimitiveTypeCode.Boolean => "System.Boolean",
                PrimitiveTypeCode.Single => "System.Single",
                PrimitiveTypeCode.Double => "System.Double",
                PrimitiveTypeCode.Object => "System.Object",
                PrimitiveTypeCode.Void => "System.Void",
                _ => typeCode.ToString()
            };

            public string GetSZArrayType(string elementType) => elementType + "[]";

            public string GetTypeFromDefinition(
                MetadataReader reader,
                TypeDefinitionHandle handle,
                byte rawTypeKind) => TypeName(reader, reader.GetTypeDefinition(handle));

            public string GetTypeFromReference(
                MetadataReader reader,
                TypeReferenceHandle handle,
                byte rawTypeKind)
            {
                var type = reader.GetTypeReference(handle);
                var name = reader.GetString(type.Name);
                var ns = reader.GetString(type.Namespace);
                return string.IsNullOrEmpty(ns) ? name : ns + "." + name;
            }

            public string GetTypeFromSpecification(
                MetadataReader reader,
                MetadataReader? genericContext,
                TypeSpecificationHandle handle,
                byte rawTypeKind) => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
        }
    }
}

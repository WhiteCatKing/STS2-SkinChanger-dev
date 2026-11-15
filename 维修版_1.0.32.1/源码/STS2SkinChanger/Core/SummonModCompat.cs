using System.Collections;
using System.Reflection;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// 随从（目前是奥斯提）皮肤的两种来源，以及第二种的通用兼容层。
///
/// **① 角色皮肤包顺带做了随从**（Orchis、midori_black、Yuzuki 这类）：
///   随从组只是皮肤目录里的一个普通分组，覆盖包负责把它选择的场景/帧图挂到规范路径上；
///   提供者的 DLL 在"只在随从分组被选中"时保持停用（见 <c>SkinService.GetActiveRuntimeProviders</c>），
///   所以它不会把主人角色也画成同款。这条路径完全由皮肤目录驱动，加新包不用写代码。
///
/// **② 专门做随从的 Mod**（「让鸡煲撑在地上假装自己是奥斯提」= OstyDefectImpostor）：
///   它自己在 <c>NCreature._Ready</c> 里换掉整条身体（自建 SpineSprite），皮肤不是资源覆盖能表达的。
///   本 Mod 对这类 Mod 只做三件事：不重建它装好的节点、默认把随从外观让给它、
///   在玩家选到它自己的皮肤时通过它的公开 API 切换。
///
/// 为了让**下一个同类 Mod 不用改本 Mod 的代码**，这一层做成了"发现 + 适配器"：
///   * <see cref="SummonBodyModRegistry"/> 自动发现"自己接管随从身体"的第三方程序集：
///     它的 DLL 里出现随从的模型 id（如 <c>MONSTER.OSTY</c>），并且它补丁了 NCreature 的外观入口。
///   * 如果这个程序集还提供了一套约定形状的换肤 API（见 <see cref="ReflectedSummonSkinAdapter"/>），
///     自动包装成适配器，于是它的皮肤会出现在奥斯提下拉里、能被立即切换、能在选人界面预览。
///   * 没有适配器也不影响安全：本 Mod 仍然不会重建它的身体、也不会和它抢资源。
/// </summary>
internal static class SummonModCompat
{
    private const string CompanionGroupId = "osty";

    private static readonly HashSet<string> DecoupledGroups = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReportedGroups = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// 玩家是否已经在「奥斯提下拉」里选过。false 表示保留旧行为：
    /// 装了接管随从的 Mod 就把随从让给它（避免两边争同一只骨骼变黑）；
    /// true 表示随从外观完全由本 Mod 的选择决定。
    /// 现在按分组记录，将来增加别的随从时互不影响。
    /// </summary>
    internal static bool IsDecoupled(string groupId) =>
        DecoupledGroups.Contains(groupId);

    internal static void SetDecoupled(string groupId, bool decoupled)
    {
        if (decoupled)
        {
            DecoupledGroups.Add(groupId);
        }
        else
        {
            DecoupledGroups.Remove(groupId);
        }
    }

    /// <summary>该随从是否有"自己接管身体"的第三方 Mod（自动发现的结果）。</summary>
    internal static bool HasBodyOwner(string groupId) =>
        SummonBodyModRegistry.OwnersFor(groupId).Count > 0;

    /// <summary>该随从是否有能列出/切换自己皮肤的适配器。</summary>
    internal static bool HasSkinAdapter(string groupId) =>
        SummonBodyModRegistry.AdaptersFor(groupId).Count > 0;

    /// <summary>
    /// 随从的最终外观是否由第三方 Mod 负责：玩家选了它的皮肤，或者分组停在"游戏默认"
    /// 而它装着（此时由它自己的偏好决定）。两种情况下都不能重建节点、也不能挂本 Mod 的随从资源。
    /// </summary>
    internal static bool OwnsBody(string groupId)
    {
        if (!HasBodyOwner(groupId))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SkinService.Config.CompanionImpostorSkinId) &&
            SummonBodyModRegistry.AdaptersFor(groupId).Any(adapter =>
                adapter.TryParseOption(SkinService.Config.CompanionImpostorSkinId!, out _)))
        {
            return true;
        }

        return SkinService.Config.GetSelection(groupId)
            .Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>本 Mod 是否必须把该随从分组让出去（不挂资源、不回放后处理）。</summary>
    internal static bool MustDefer(string groupId)
    {
        if (!HasBodyOwner(groupId))
        {
            return false;
        }

        // 已经解耦：由「奥斯提下拉」的选择决定，选到它的皮肤时随从组回到游戏原版，
        // 本身就没有资源要挂，不需要再让位。
        if (IsDecoupled(groupId))
        {
            return false;
        }

        if (ReportedGroups.Add(groupId))
        {
            var owners = string.Join("、", SummonBodyModRegistry.OwnersFor(groupId));
            ModLog.Info($"{groupId} 的外观交回 {owners}（该召唤由它的自建骨骼负责）。");
        }

        return true;
    }

    /// <summary>可供下拉展示的第三方随从皮肤（不含"原版"，原版由本 Mod 自己的条目表示）。</summary>
    internal static IReadOnlyList<SummonSkinEntry> SkinsFor(string groupId) =>
        SummonBodyModRegistry.AdaptersFor(groupId)
            .SelectMany(adapter => adapter.Skins)
            .DistinctBy(skin => skin.OptionId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    internal static bool TryParseOptionEntry(string optionId, out SummonSkinEntry entry)
    {
        foreach (var adapter in SummonBodyModRegistry.AllAdapters)
        {
            if (adapter.TryParseOption(optionId, out entry))
            {
                return true;
            }
        }

        entry = default;
        return false;
    }

    internal static bool TryParseOption(string optionId, out string skinId)
    {
        if (TryParseOptionEntry(optionId, out var entry))
        {
            skinId = entry.SkinId;
            return true;
        }

        skinId = string.Empty;
        return false;
    }

    internal static Node2D? TryCreateSelectDisplay(string groupId)
    {
        foreach (var adapter in SummonBodyModRegistry.AdaptersFor(groupId))
        {
            if (adapter.TryCreateSelectDisplay() is { } display)
            {
                return display;
            }
        }

        return null;
    }

    internal static string SelectDisplayAnimation(string groupId) =>
        SummonBodyModRegistry.AdaptersFor(groupId).FirstOrDefault()?.SelectDisplayAnimation ?? "animation";

    /// <summary>
    /// 我们主动切换第三方皮肤时用它：不把回灌当成"玩家在它的界面里改了"。
    /// </summary>
    internal static bool ApplyQuiet(string groupId, string? skinId)
    {
        var applied = false;
        foreach (var adapter in SummonBodyModRegistry.AdaptersFor(groupId))
        {
            applied |= adapter.ApplyQuiet(skinId);
        }

        return applied;
    }

    /// <summary>
    /// 让第三方 Mod 重新登记场上已有的召唤物，按当前皮肤立刻重建。
    /// 它们普遍是"下一次出现时生效"的排队语义，不重登记的话局内换肤看起来没反应。
    /// </summary>
    internal static int RefreshLiveSummons(string groupId)
    {
        var refreshed = 0;
        foreach (var adapter in SummonBodyModRegistry.AdaptersFor(groupId))
        {
            refreshed += adapter.RefreshLiveSummons();
        }

        return refreshed;
    }

    internal static void Reset()
    {
        ReportedGroups.Clear();
        SummonBodyModRegistry.Reset();
    }
}

/// <summary>第三方随从皮肤在下拉里的一个条目。</summary>
internal readonly record struct SummonSkinEntry(
    string OptionId,
    string SkinId,
    string Name,
    string ProviderId);

/// <summary>第三方随从 Mod 的适配器契约。没有适配器时只有"不重建、不抢资源"的安全行为。</summary>
internal interface ISummonSkinAdapter
{
    string ProviderId { get; }
    IReadOnlyList<SummonSkinEntry> Skins { get; }
    string OptionPrefix { get; }
    string VanillaSkinId { get; }
    string SelectDisplayAnimation { get; }
    bool TryParseOption(string optionId, out SummonSkinEntry entry);
    bool ApplyQuiet(string? skinId);
    int RefreshLiveSummons();
    Node2D? TryCreateSelectDisplay();
}

/// <summary>
/// 自动发现"自己接管随从身体"的第三方 Mod，并为它们建立适配器。
///
/// 判定不依赖 Mod 名字：DLL 里出现随从的模型 id 字符串（.NET 的 <c>ldstr</c> 是 UTF-16），
/// 并且该程序集确实补丁了 NCreature 的外观入口（Harmony 补丁表可查）。
/// 这样作者换名字、换包、出重制版都能被认出来；误判的代价也只是"我们不碰它的随从"。
/// </summary>
internal static class SummonBodyModRegistry
{
    private static readonly Dictionary<string, List<string>> Owners = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, List<ISummonSkinAdapter>> Adapters = new(StringComparer.OrdinalIgnoreCase);
    private static bool _scanned;
    private static int _scannedAssemblyCount;

    internal static IReadOnlyList<ISummonSkinAdapter> AllAdapters =>
        Adapters.Values.SelectMany(list => list).ToArray();

    internal static IReadOnlyList<string> OwnersFor(string companionGroupId)
    {
        EnsureScanned();
        return Owners.TryGetValue(companionGroupId, out var owners)
            ? owners
            : (IReadOnlyList<string>)[];
    }

    internal static IReadOnlyList<ISummonSkinAdapter> AdaptersFor(string companionGroupId)
    {
        EnsureScanned();
        return Adapters.TryGetValue(companionGroupId, out var adapters)
            ? adapters
            : (IReadOnlyList<ISummonSkinAdapter>)[];
    }

    internal static void Reset()
    {
        Owners.Clear();
        Adapters.Clear();
        _scanned = false;
    }

    private static void EnsureScanned()
    {
        // 扫描可能发生在第三方 Mod 还没全部加载完的时候（初始化钩子顺序不保证）。
        // 程序集数量变了就重扫一次，这样"第一眼没发现"不会变成永久失效。
        var assemblyCount = AppDomain.CurrentDomain.GetAssemblies().Length;
        if (_scanned && assemblyCount == _scannedAssemblyCount)
        {
            return;
        }

        _scanned = true;
        _scannedAssemblyCount = assemblyCount;
        Owners.Clear();
        Adapters.Clear();
        try
        {
            var visualPatchedAssemblies = CollectVisualPatchedAssemblies();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string? name;
                string? location;
                try
                {
                    name = assembly.GetName().Name;
                    location = assembly.Location;
                }
                catch
                {
                    continue;
                }

                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(location) ||
                    SummonModCompatIds.IsIgnoredAssembly(name!))
                {
                    continue;
                }

                if (!visualPatchedAssemblies.Contains(location!))
                {
                    continue;
                }

                var companion = MatchCompanionGroup(location!);
                if (companion == null)
                {
                    continue;
                }

                if (!Owners.TryGetValue(companion, out var owners))
                {
                    Owners[companion] = owners = [];
                }

                owners.Add(name!);
                if (ReflectedSummonSkinAdapter.TryCreate(assembly, name!, companion) is { } adapter)
                {
                    if (!Adapters.TryGetValue(companion, out var adapters))
                    {
                        Adapters[companion] = adapters = [];
                    }

                    adapters.Add(adapter);
                    ModLog.Info(
                        $"已接入独立随从 Mod {name}（{companion}）：{adapter.Skins.Count} 个皮肤、" +
                        "会按它自己的 API 切换与刷新。");
                }
                else
                {
                    ModLog.Info(
                        $"检测到独立随从 Mod {name}（{companion}）：它自己接管随从身体，" +
                        "本 Mod 不再重建该随从、也不与它抢资源（它没有提供可选的皮肤 API）。");
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("扫描独立随从 Mod 失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>Harmony 补丁表里出现过的、以 NCreature 外观入口为目标的程序集位置。</summary>
    private static HashSet<string> CollectVisualPatchedAssemblies()
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var target in HarmonyLib.Harmony.GetAllPatchedMethods())
            {
                var declaring = target.DeclaringType;
                if (declaring == null ||
                    (declaring != typeof(NCreature) &&
                     !typeof(NCreature).IsAssignableFrom(declaring) &&
                     !typeof(NCreatureVisuals).IsAssignableFrom(declaring)))
                {
                    continue;
                }

                var info = HarmonyLib.Harmony.GetPatchInfo(target);
                if (info == null)
                {
                    continue;
                }

                foreach (var patch in info.Prefixes.Concat(info.Postfixes)
                             .Concat(info.Transpilers))
                {
                    var location = patch.PatchMethod.Module.Assembly.Location;
                    if (!string.IsNullOrEmpty(location))
                    {
                        result.Add(location);
                    }
                }
            }
        }
        catch
        {
            // 补丁表不可用时退化为"不发现"，不影响原有行为。
        }

        return result;
    }

    /// <summary>DLL 的字符串堆里出现哪个随从的模型 id，就归到哪个随从分组。</summary>
    private static string? MatchCompanionGroup(string assemblyPath)
    {
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(assemblyPath);
        }
        catch
        {
            return null;
        }

        foreach (var pair in SummonModCompatIds.CompanionModelIds)
        {
            foreach (var modelId in pair.Value)
            {
                if (ContainsUtf16(bytes, modelId))
                {
                    return pair.Key;
                }
            }
        }

        return null;
    }

    private static bool ContainsUtf16(byte[] haystack, string needle)
    {
        var pattern = Encoding.Unicode.GetBytes(needle);
        return haystack.AsSpan().IndexOf(pattern) >= 0;
    }
}

internal static class SummonModCompatIds
{
    /// <summary>本 Mod、游戏本体、Harmony 以及运行时框架永远不算"第三方随从 Mod"。</summary>
    private static readonly HashSet<string> Ignored =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Gurio.SkinChanger", "thunninoiSkinManager", "sts2", "0Harmony", "HarmonyLib",
            "GodotSharp", "Sentry", "Steamworks.NET", "STS2SkinChanger.RuntimeTests"
        };

    /// <summary>
    /// 判定"这个程序集在管哪个随从"的字符串。作者的写法不统一（有的比 <c>MONSTER.OSTY</c>，
    /// 有的只比 <c>OSTY</c>），所以按"最长优先、短串兜底"来认。新增随从时在这里补一行。
    /// </summary>
    internal static IReadOnlyDictionary<string, string[]> CompanionModelIds { get; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["osty"] = ["MONSTER.OSTY", "Monster.Osty", "OSTY"]
        };

    internal static bool IsIgnoredAssembly(string name) =>
        Ignored.Contains(name) ||
        name.StartsWith("System.", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase) ||
        name.StartsWith("MegaCrit.", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// 约定形状的换肤 API 的反射适配器。
///
/// 作者不用知道本 Mod 的存在，只要他的程序集里有这些**形状**（名称可以带自己的命名空间）：
///   * 某个静态方法 <c>bool Apply(string skinId)</c>；
///   * 某个静态属性 <c>All</c>，元素带 <c>Id</c>/<c>Name</c>；
///   * 可选：静态 <c>Register(NCreature)</c> / <c>Unregister(NCreature)</c> 用于局内立即生效；
///   * 可选：静态 <c>Create(bool)</c> 返回 Node2D，用于选人界面预览。
/// 「让鸡煲撑在地上假装自己是奥斯提」正好符合，所以它成了第一个零改动的接入样例。
/// </summary>
internal sealed class ReflectedSummonSkinAdapter : ISummonSkinAdapter
{
    private readonly MethodInfo _apply;
    private readonly MethodInfo? _register;
    private readonly MethodInfo? _unregister;
    private readonly MethodInfo? _createDisplay;
    private readonly List<SummonSkinEntry> _skins = [];
    private int _echoDepth;

    private ReflectedSummonSkinAdapter(
        string providerId,
        string optionPrefix,
        string vanillaSkinId,
        MethodInfo apply,
        MethodInfo? register,
        MethodInfo? unregister,
        MethodInfo? createDisplay)
    {
        ProviderId = providerId;
        OptionPrefix = optionPrefix;
        VanillaSkinId = vanillaSkinId;
        _apply = apply;
        _register = register;
        _unregister = unregister;
        _createDisplay = createDisplay;
    }

    public string ProviderId { get; }
    public IReadOnlyList<SummonSkinEntry> Skins => _skins;
    public string OptionPrefix { get; }
    public string VanillaSkinId { get; }
    public string SelectDisplayAnimation => "animation";

    internal static ReflectedSummonSkinAdapter? TryCreate(
        Assembly assembly,
        string providerId,
        string companionGroupId)
    {
        try
        {
            var types = assembly.GetTypes();
            var apply = types
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .FirstOrDefault(method =>
                    method.Name == "Apply" &&
                    method.ReturnType == typeof(bool) &&
                    method.GetParameters() is [{ ParameterType: var parameterType }] &&
                    parameterType == typeof(string));
            if (apply == null)
            {
                return null;
            }

            var all = types
                .Select(type => type.GetProperty(
                                    "All",
                                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                ?.GetValue(null) ??
                            type.GetField("All", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                                ?.GetValue(null))
                .FirstOrDefault(value => value is IEnumerable and not string);
            if (all is not IEnumerable items)
            {
                return null;
            }

            var companionToken = companionGroupId;
            var adapter = new ReflectedSummonSkinAdapter(
                providerId,
                companionToken + ":",
                "vanilla." + companionToken,
                apply,
                FindCreatureMethod(types, "Register"),
                FindCreatureMethod(types, "Unregister"),
                FindDisplayFactory(types));
            foreach (var item in items)
            {
                if (item == null)
                {
                    continue;
                }

                var type = item.GetType();
                var id = type.GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetValue(item) as string;
                var name = type.GetProperty("Name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    ?.GetValue(item) as string;
                if (string.IsNullOrEmpty(id) ||
                    id.Equals(adapter.VanillaSkinId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                adapter._skins.Add(new SummonSkinEntry(
                    adapter.OptionPrefix + id,
                    id,
                    string.IsNullOrEmpty(name) ? id : name,
                    providerId));
            }

            return adapter._skins.Count > 0 ? adapter : null;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"读取 {providerId} 的随从皮肤 API 失败：{exception.GetBaseException().Message}");
            return null;
        }
    }

    private static MethodInfo? FindCreatureMethod(IEnumerable<Type> types, string name) =>
        types.SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .FirstOrDefault(method =>
                method.Name == name &&
                method.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType.IsAssignableFrom(typeof(NCreature)));

    private static MethodInfo? FindDisplayFactory(IEnumerable<Type> types) =>
        types.SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            .FirstOrDefault(method =>
                method.Name == "Create" &&
                typeof(Node).IsAssignableFrom(method.ReturnType) &&
                method.GetParameters() is [{ ParameterType: var parameterType }] &&
                parameterType == typeof(bool));

    public bool TryParseOption(string optionId, out SummonSkinEntry entry)
    {
        if (optionId.StartsWith(OptionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var skinId = optionId[OptionPrefix.Length..];
            entry = _skins.FirstOrDefault(skin =>
                skin.SkinId.Equals(skinId, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(entry.SkinId))
            {
                return true;
            }
        }

        // 兼容旧配置里 `impostor:<id>` 这种历史前缀：只要皮肤 id 相同就认。
        entry = _skins.FirstOrDefault(skin =>
            skin.SkinId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
        return !string.IsNullOrEmpty(entry.SkinId);
    }

    public bool ApplyQuiet(string? skinId)
    {
        _echoDepth++;
        CharSelectTakeoverSuppressor.EchoSuppressed = true;
        try
        {
            var target = string.IsNullOrEmpty(skinId) ? VanillaSkinId : skinId;
            return _apply.Invoke(null, [target]) is true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"切换 {ProviderId} 的随从皮肤失败：{exception.GetBaseException().Message}");
            return false;
        }
        finally
        {
            _echoDepth--;
            CharSelectTakeoverSuppressor.EchoSuppressed = _echoDepth > 0;
        }
    }

    public int RefreshLiveSummons()
    {
        if (_register == null || _unregister == null)
        {
            return 0;
        }

        var refreshed = 0;
        foreach (var creature in MegaCrit.Sts2.Core.Nodes.Rooms.NCombatRoom.Instance?.CreatureNodes ?? [])
        {
            if (creature == null || !GodotObject.IsInstanceValid(creature) ||
                creature.Entity?.PetOwner == null)
            {
                continue;
            }

            try
            {
                _unregister.Invoke(null, [creature]);
                _register.Invoke(null, [creature]);
                refreshed++;
            }
            catch (Exception exception)
            {
                ModLog.Warn($"让 {ProviderId} 重新登记召唤物失败：" + exception.GetBaseException().Message);
            }
        }

        return refreshed;
    }

    public Node2D? TryCreateSelectDisplay()
    {
        if (_createDisplay == null)
        {
            return null;
        }

        try
        {
            return _createDisplay.Invoke(null, [true]) as Node2D;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"用 {ProviderId} 自己的展示工厂创建预览失败：" + exception.GetBaseException().Message);
            return null;
        }
    }
}

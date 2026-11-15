using System.Reflection;
using Godot;

namespace STS2SkinChanger.Core;

/// <summary>
/// 「独立随从 Mod」的皮肤桥门面。实现已经通用化（<see cref="SummonModCompat"/>）：
/// 任何"自己接管随从身体、并且提供了约定形状换肤 API"的 Mod 都会自动出现在奥斯提下拉里，
/// 不需要改本文件。历史上这个类只服务「让鸡煲撑在地上假装自己是奥斯提」，保留名字是为了
/// 不动既有调用点。
///
/// 约定形状（作者的 DLL 里只要有这些"形状"，命名空间随意）：
///   * <c>static bool Apply(string skinId)</c>
///   * <c>static IEnumerable All</c>，元素带 <c>Id</c>/<c>Name</c>
///   * 可选 <c>static void Register/Unregister(NCreature)</c>：局内立即生效
///   * 可选 <c>static Node Create(bool)</c>：选人界面预览
/// </summary>
internal static class OstyImpostorBridge
{
    private const string GroupId = "osty";

    /// <summary>历史下拉前缀；新代码用适配器自己的前缀（<c>osty:</c>），这里仅用于兼容旧配置。</summary>
    internal const string LegacyOptionPrefix = "impostor:";

    internal const string VanillaSkinId = "vanilla.osty";

    internal static bool IsImpostorOption(string optionId) =>
        TryParseOption(optionId, out _);

    /// <summary>它提供的随从皮肤（不含"原版"，原版由本 Mod 自己的"游戏默认"条目表示）。</summary>
    internal static IReadOnlyList<(string Id, string Name)> Options =>
        SummonModCompat.SkinsFor(GroupId)
            .Select(entry => (entry.SkinId, entry.Name))
            .ToArray();

    /// <summary>下拉里这一项对应的 metadata（选项 id）。</summary>
    internal static string CreateOptionId(string skinId) =>
        SummonModCompat.SkinsFor(GroupId)
            .FirstOrDefault(entry => entry.SkinId.Equals(skinId, StringComparison.OrdinalIgnoreCase))
            .OptionId ?? LegacyOptionPrefix + skinId;

    internal static string GetOptionName(string optionId) =>
        TryParseOptionEntry(optionId, out var entry) ? entry.Name : optionId;

    internal static bool TryParseOption(string optionId, out string skinId)
    {
        // 历史前缀先剥掉再交给通用解析，老存档里的 `impostor:<id>` 仍然能认出来。
        var normalized = optionId.StartsWith(LegacyOptionPrefix, StringComparison.OrdinalIgnoreCase)
            ? optionId[LegacyOptionPrefix.Length..]
            : optionId;
        if (SummonModCompat.TryParseOptionEntry(normalized, out var entry))
        {
            skinId = entry.SkinId;
            return true;
        }

        skinId = string.Empty;
        return false;
    }

    internal static bool TryParseOptionEntry(string optionId, out SummonSkinEntry entry) =>
        SummonModCompat.TryParseOptionEntry(
            optionId.StartsWith(LegacyOptionPrefix, StringComparison.OrdinalIgnoreCase)
                ? optionId[LegacyOptionPrefix.Length..]
                : optionId,
            out entry);

    /// <summary>我们主动切换它的皮肤时用这个：不把自己的回灌当成"玩家在它的界面里改了"。</summary>
    internal static bool ApplyQuiet(string? skinId) =>
        SummonModCompat.ApplyQuiet(GroupId, skinId);

    /// <summary>把它自己的奥斯提外观切到指定皮肤；skinId 为空/未知时切回原版。</summary>
    internal static bool Apply(string? skinId) => ApplyQuiet(skinId);

    /// <summary>
    /// 用它自己的展示工厂创建一只「选人界面用」的奥斯提精灵（Node2D）。失败返回 null。
    /// </summary>
    internal static Node2D? CreateSelectDisplay() =>
        SummonModCompat.TryCreateSelectDisplay(GroupId);

    /// <summary>它自己的展示精灵使用的待机动画名。</summary>
    internal static string SelectDisplayAnimation =>
        SummonModCompat.SelectDisplayAnimation(GroupId);

    /// <summary>
    /// 让第三方 Mod 重新登记场上已有的召唤物，按当前皮肤立刻重建（它们普遍是"下一次出现时生效"）。
    /// </summary>
    internal static int RefreshLiveSummons() =>
        SummonModCompat.RefreshLiveSummons(GroupId);

    internal static void Reset() => SummonModCompat.Reset();
}

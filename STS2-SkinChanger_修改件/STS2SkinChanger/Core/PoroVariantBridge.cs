using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

/// <summary>
/// 提供者自己画的随从（魄罗/奥斯提）外观。
///
/// 有一类整包外观 Mod 会把随从一起画好，但随从形态是**绑在角色变体上**的（作者的
/// <c>CharacterId(pet)</c> 直接读同一个 bool），玩家没法单独给随从挑形态。这里补上这条：
/// 随从单独记一个选择，在随从视觉创建之后把它的骨骼数据换成另一套。
///
/// 判定不看 Mod 名字，只看能力形状：提供者程序集里同时有
///   * <c>static Resource Data(string)</c>：按 id 取骨骼数据；
///   * <c>static bool IsCompanion(Creature)</c>：自己声明谁是它的随从。
/// 这样作者换名字、出重制版仍然能被认出来；认不出来时只是不出现这两个选项，不影响别的功能。
/// </summary>
internal static class PoroVariantBridge
{
    internal const string OptionPrefix = "poro:";

    private const string CompanionGroupId = "osty";
    private const string OwnerGroupId = "necrobinder";

    internal readonly record struct PoroOption(string OptionId, string SkinId, string Name);

    /// <summary>下拉里的两个条目。名字按玩家叫法（魄罗），与作者菜单里的"小红帽 · 原皮/女仆"区分开。</summary>
    internal static IReadOnlyList<PoroOption> Options { get; } =
    [
        new(OptionPrefix + "base", "redhood_bot01", "魄罗 · 原皮"),
        new(OptionPrefix + "maid", "redhood_maid_bot01", "魄罗 · 女仆")
    ];

    internal static bool IsPoroOption(string? optionId) =>
        !string.IsNullOrEmpty(optionId) &&
        optionId.StartsWith(OptionPrefix, StringComparison.OrdinalIgnoreCase);

    internal static bool TryParseOption(string? optionId, out PoroOption option)
    {
        option = default;
        if (!IsPoroOption(optionId))
        {
            return false;
        }

        foreach (var candidate in Options)
        {
            if (candidate.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase))
            {
                option = candidate;
                return true;
            }
        }

        return false;
    }

    /// <summary>下拉里选中项对应的选项 id；没选过返回空串（下拉回到"游戏默认"）。</summary>
    internal static string CurrentOptionId
    {
        get
        {
            var skin = SkinService.Config.CompanionProviderVariantId;
            if (string.IsNullOrEmpty(skin))
            {
                return string.Empty;
            }

            foreach (var candidate in Options)
            {
                if (candidate.SkinId.Equals(skin, StringComparison.OrdinalIgnoreCase))
                {
                    return candidate.OptionId;
                }
            }

            return string.Empty;
        }
    }

    /// <summary>当前挂着的提供者是不是"自己画随从"的那一类；不是的话这两个选项不该出现。</summary>
    internal static bool Available => ResolveCapability() != null;

    /// <summary>
    /// 随从分组下拉/局内面板用：只有在主人分组选中的提供者确实自己画随从时才给选项。
    /// </summary>
    internal static bool AvailableFor(string companionGroupId) =>
        companionGroupId.Equals(CompanionGroupId, StringComparison.OrdinalIgnoreCase) && Available;

    /// <summary>玩家点了"魄罗 · xxx"：写盘并立刻换掉场上已有的魄罗。</summary>
    internal static bool ApplyQuiet(string? skinId)
    {
        SkinService.Config.CompanionProviderVariantId = string.IsNullOrWhiteSpace(skinId) ? null : skinId;
        var refreshed = RefreshLiveSkins();
        return refreshed >= 0;
    }

    /// <summary>把场上已经存在的魄罗换成当前选择；返回换成功的数量。</summary>
    internal static int RefreshLiveSkins()
    {
        if (ResolveCapability() is not { } capability)
        {
            return 0;
        }

        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree?.Root == null)
        {
            return 0;
        }

        var skinId = SkinService.Config.CompanionProviderVariantId;
        var changed = 0;
        foreach (var node in tree.Root.FindChildren("*", nameof(Node2D), recursive: true, owned: false))
        {
            if (node is not Node2D visuals || visuals.GetNodeOrNull<Node2D>("Visuals") is not { } spine)
            {
                continue;
            }

            // 只认"自己画随从"的那份视觉：作者把自制视觉挂在自己的骨骼数据上，别的随从没有这层结构。
            if (!LooksLikeProviderPet(spine))
            {
                continue;
            }

            if (ApplyTo(spine, capability, skinId))
            {
                changed++;
            }
        }

        return changed;
    }

    /// <summary>随从视觉刚创建完：按当前选择覆盖一次。</summary>
    internal static void ApplyOnCreated(Creature creature, Node? visuals)
    {
        if (visuals == null || !IsCompanion(creature) || ResolveCapability() is not { } capability)
        {
            return;
        }

        if (visuals.GetNodeOrNull<Node2D>("Visuals") is not { } spine || !LooksLikeProviderPet(spine))
        {
            return;
        }

        ApplyTo(spine, capability, SkinService.Config.CompanionProviderVariantId);
    }

    private static bool IsCompanion(Creature creature) =>
        ResolveCapability()?.IsCompanion?.Invoke(creature) is true;

    /// <summary>
    /// "这是提供者自己画的随从"的判据：Visuals 是 SpineSprite，且作者在它身上留了标记（meta 值等于提供者 id）。
    /// 作者换标记名也认得出；没有标记（别的随从、游戏原生奥斯提）就完全不碰。
    /// </summary>
    private static bool LooksLikeProviderPet(Node2D spine)
    {
        var providerId = SkinService.GetSelectedRuntimeProvider(OwnerGroupId);
        if (string.IsNullOrEmpty(providerId))
        {
            return false;
        }

        if (spine.HasMethod("set_skeleton_data_res") == false)
        {
            return false;
        }

        foreach (var key in spine.GetMetaList())
        {
            if (spine.GetMeta(key).AsString().Equals(providerId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ApplyTo(Node2D spine, Capability capability, string? skinId)
    {
        if (string.IsNullOrEmpty(skinId))
        {
            return false;
        }

        try
        {
            if (capability.Data?.Invoke(null, [skinId]) is not GodotObject data)
            {
                return false;
            }

            spine.Call("set_skeleton_data_res", data);
            // 换骨骼后从头播待机，否则会停在旧骨架的最后一帧。
            if (spine.Call("get_animation_state").AsGodotObject() is { } state)
            {
                state.Call("set_animation", "idle_loop", true, 0);
            }

            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"魄罗外观切换失败：{exception.GetBaseException().Message}");
            return false;
        }
    }

    private sealed record Capability(MethodInfo? Data, Func<Creature, bool>? IsCompanion);

    private static Capability? _capability;
    private static string? _capabilityProvider;

    private static Capability? ResolveCapability()
    {
        var providerId = SkinService.GetSelectedRuntimeProvider(OwnerGroupId);
        if (string.IsNullOrEmpty(providerId))
        {
            return null;
        }

        if (_capability != null && string.Equals(_capabilityProvider, providerId, StringComparison.Ordinal))
        {
            return _capability;
        }

        _capability = null;
        _capabilityProvider = providerId;

        var assembly = ManagedSkinModLoader.GetActiveProviderAssembly(providerId);
        if (assembly == null)
        {
            return null;
        }

        try
        {
            MethodInfo? data = null;
            MethodInfo? companion = null;
            foreach (var type in assembly.GetTypes())
            {
                data ??= type.GetMethod("Data",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    [typeof(string)],
                    null) is { } candidate && candidate.ReturnType.FullName == "Godot.Resource"
                    ? candidate
                    : null;

                companion ??= type.GetMethod("IsCompanion",
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    [typeof(Creature)],
                    null) is { } probe && probe.ReturnType == typeof(bool)
                    ? probe
                    : null;
            }

            if (data == null || companion == null)
            {
                return null;
            }

            _capability = new Capability(data, creature => companion.Invoke(null, [creature]) is true);
            return _capability;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"识别魄罗提供者失败：{exception.GetBaseException().Message}");
            return null;
        }
    }
}

/// <summary>
/// 随从视觉创建完之后再覆盖一次骨骼：提供者总是按角色变体造随从，
/// 玩家单独选的魄罗形态要盖在它后面。
/// </summary>
[HarmonyLib.HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class PoroVariantVisualsPatch
{
    private static void Postfix(Creature __instance, ref NCreatureVisuals __result) =>
        PoroVariantBridge.ApplyOnCreated(__instance, __result);
}

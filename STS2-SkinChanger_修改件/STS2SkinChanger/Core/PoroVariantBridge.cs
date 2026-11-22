using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

/// <summary>
/// 提供者自己画好的随从（魄罗/奥斯提）外观，单独做成一档可选项。
///
/// 有两件事必须守住：
///   * **只碰随从**——本体和它的预览小人绝不能被换成随从骨架（曾经把骨妹换成了魄罗）。
///   * **不跟角色皮肤绑定**——骨妹选任何皮肤，魄罗这两档都该在列表里，照样能选。
///
/// 所以这里完全不依赖提供者的程序集有没有被激活：直接在已挂载的包资源里找
/// <c>scripts/spine_factory.gd</c> 与 <c>spine42/&lt;随从&gt;/</c>，用作者自己的工厂取骨骼，
/// 再把随从视觉的骨骼数据换过去。判定随从也不看 Mod 名字，只看游戏自己的
/// 「有主人 + 怪物是 Osty」这条结构。
/// </summary>
internal static class PoroVariantBridge
{
    internal const string OptionPrefix = "poro:";

    private const string CompanionGroupId = "osty";
    private const string CompanionMonsterName = "Osty";
    private const string FactoryRelativePath = "scripts/spine_factory.gd";
    private const string SpineFolder = "spine42";

    internal readonly record struct PoroOption(string OptionId, string SkinId, string Name);

    private static Discovery? _discovery;
    private static bool _discovered;

    private sealed record Discovery(GDScript Factory, IReadOnlyList<PoroOption> Options);

    internal static IReadOnlyList<PoroOption> Options => Resolve()?.Options ?? [];

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

    /// <summary>装着的包里有没有"自己带随从"的那一类；跟角色当前选了什么皮肤无关。</summary>
    internal static bool Available => Resolve() != null;

    internal static bool AvailableFor(string companionGroupId) =>
        companionGroupId.Equals(CompanionGroupId, StringComparison.OrdinalIgnoreCase) && Available;

    /// <summary>玩家点了"魄罗 · xxx"：写盘并立刻换掉场上已有的魄罗。</summary>
    internal static bool ApplyQuiet(string? skinId)
    {
        SkinService.Config.CompanionProviderVariantId = string.IsNullOrWhiteSpace(skinId) ? null : skinId;
        RefreshLiveSkins();
        return true;
    }

    /// <summary>把场上已经存在的魄罗换成当前选择；返回换成功的数量。只认随从本体。</summary>
    internal static int RefreshLiveSkins()
    {
        var discovery = Resolve();
        if (discovery == null)
        {
            return 0;
        }

        var skinId = SkinService.Config.CompanionProviderVariantId;
        if (string.IsNullOrEmpty(skinId))
        {
            return 0;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            return 0;
        }

        var changed = 0;
        foreach (var node in tree.Root.FindChildren("*", nameof(NCreature), recursive: true, owned: false))
        {
            if (node is not NCreature owner || !IsCompanion(owner.Entity))
            {
                continue;
            }

            if (FindSpine(owner) is not { } spine)
            {
                continue;
            }

            if (ApplyTo(spine, discovery, skinId))
            {
                changed++;
            }
        }

        return changed;
    }

    /// <summary>随从视觉刚创建完：按当前选择覆盖一次。</summary>
    internal static void ApplyOnCreated(Creature creature, Node? visuals)
    {
        if (visuals == null || !IsCompanion(creature))
        {
            return;
        }

        var discovery = Resolve();
        var skinId = SkinService.Config.CompanionProviderVariantId;
        if (discovery == null || string.IsNullOrEmpty(skinId))
        {
            return;
        }

        if (FindSpine(visuals) is { } spine)
        {
            ApplyTo(spine, discovery, skinId);
        }
    }

    /// <summary>游戏自己的随从结构：有主人，且怪物就是奥斯提。跟提供者是谁无关。</summary>
    private static bool IsCompanion(Creature? creature) =>
        creature?.PetOwner != null &&
        string.Equals(creature.Monster?.GetType().Name, CompanionMonsterName, StringComparison.OrdinalIgnoreCase);

    /// <summary>随从的骨骼节点：先按作者用的名字找，再按能力找，最后放弃。</summary>
    private static Node2D? FindSpine(Node owner)
    {
        if (owner.GetNodeOrNull<Node2D>("Visuals") is { } named && named.HasMethod("set_skeleton_data_res"))
        {
            return named;
        }

        foreach (var node in owner.FindChildren("*", nameof(Node2D), recursive: true, owned: false))
        {
            if (node is Node2D candidate && candidate.HasMethod("set_skeleton_data_res"))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool ApplyTo(Node2D spine, Discovery discovery, string skinId)
    {
        try
        {
            using var result = discovery.Factory.Call("data_for", skinId);
            if (result.AsGodotObject() is not Resource data)
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

    /// <summary>预览面板用：按皮肤 id 取提供者自己那套随从骨架。</summary>
    internal static bool TryLoadSkeleton(string? skinId, out Resource? skeleton, out string path)
    {
        skeleton = null;
        path = string.Empty;
        if (string.IsNullOrWhiteSpace(skinId) || Resolve() is not { } discovery)
        {
            return false;
        }

        try
        {
            using var result = discovery.Factory.Call("data_for", skinId);
            if (result.AsGodotObject() is not Resource data)
            {
                return false;
            }

            skeleton = data;
            path = "poro://" + skinId;
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"魄罗外观：取骨架失败：{exception.GetBaseException().Message}");
            return false;
        }
    }

    private static Discovery? Resolve()
    {
        if (_discovered)
        {
            return _discovery;
        }

        _discovered = true;
        _discovery = Discover();
        return _discovery;
    }

    /// <summary>忘掉探测结果；换皮/换包后重新找一次。</summary>
    internal static void Reset()
    {
        _discovered = false;
        _discovery = null;
    }

    private static Discovery? Discover()
    {
        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return null;
        }

        try
        {
            foreach (var group in catalog.Groups)
            {
                foreach (var option in group.Options)
                {
                    if (option.IsComposition)
                    {
                        continue;
                    }

                    var root = option.ProviderRootPath;
                    if (string.IsNullOrWhiteSpace(root) || !File.Exists(Path.Combine(root, "scripts", "spine_factory.gd")))
                    {
                        continue;
                    }

                    var spineRoot = Path.Combine(root, SpineFolder);
                    if (!Directory.Exists(spineRoot))
                    {
                        continue;
                    }

                    var names = Directory.GetDirectories(spineRoot)
                        .Select(Path.GetFileName)
                        .Where(name => !string.IsNullOrEmpty(name))
                        .Select(name => name!)
                        .Where(LooksLikeCompanion)
                        .OrderBy(name => name.Length)
                        .ThenBy(name => name, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var base_ = names.FirstOrDefault(name => !name.Contains("maid", StringComparison.OrdinalIgnoreCase));
                    var maid = names.FirstOrDefault(name => name.Contains("maid", StringComparison.OrdinalIgnoreCase));
                    if (base_ == null)
                    {
                        continue;
                    }

                    var prefix = Path.GetFileName(root);
                    var script = ResourceLoader.Load<GDScript>(
                        "res://" + prefix + "/" + FactoryRelativePath,
                        null,
                        ResourceLoader.CacheMode.IgnoreDeep);
                    if (script == null)
                    {
                        ModLog.Warn($"魄罗外观：{prefix} 的骨骼工厂脚本加载失败，跳过随从选项。");
                        continue;
                    }

                    var options = new List<PoroOption> { new(OptionPrefix + "base", base_, "魄罗 · 原皮") };
                    if (maid != null)
                    {
                        options.Add(new PoroOption(OptionPrefix + "maid", maid, "魄罗 · 女仆"));
                    }

                    return new Discovery(script, options);
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"魄罗外观：读取包资源失败：{exception.GetBaseException().Message}");
        }

        return null;
    }

    /// <summary>目录名像不像"包里的随从"：含 bot/pet/poro，或直接叫 osty。</summary>
    private static bool LooksLikeCompanion(string name) =>
        name.Contains("bot", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("pet", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("poro", StringComparison.OrdinalIgnoreCase) ||
        name.Equals(CompanionGroupId, StringComparison.OrdinalIgnoreCase) ||
        name.Contains(CompanionGroupId, StringComparison.OrdinalIgnoreCase);
}

/// <summary>随从视觉刚创建时按选择覆盖骨骼；只对随从生效，本体不碰。</summary>
[HarmonyLib.HarmonyPatch(typeof(Creature), nameof(Creature.CreateVisuals))]
internal static class PoroVariantVisualsPatch
{
    private static void Postfix(Creature __instance, ref NCreatureVisuals __result) =>
        PoroVariantBridge.ApplyOnCreated(__instance, __result);
}

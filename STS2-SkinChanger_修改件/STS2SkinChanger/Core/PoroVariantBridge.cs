using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using System.IO;
using Godot;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models.Monsters;

namespace STS2SkinChanger.Core;

/// <summary>
/// 提供者自己画好的随从（魄罗/奥斯提）外观，单独做成一档可选项。
///
/// 有两件事必须守住：
///   * **只碰随从**——本体和它的预览小人绝不能被换成随从骨架（曾经把骨妹换成了魄罗）。
///   * **不跟角色皮肤绑定**——骨妹选任何皮肤，魄罗这两档都该在列表里，照样能选。
///
/// 从提供者的 PCK 找随从资源，按需挂载随从专用资源包，再实例化作者的骨骼工厂。
/// 随从判定使用游戏的「有主人 + 怪物是 Osty」结构。
/// </summary>
internal static class PoroVariantBridge
{
    internal const string OptionPrefix = "poro:";
    internal const string CombatBodyMeta = "sc_poro_combat_body";

    private const string CompanionGroupId = "osty";
    private const string FactoryRelativePath = "scripts/spine_factory.gd";
    private const string SpineFolder = "spine42";

    internal readonly record struct PoroOption(string OptionId, string SkinId, string Name);

    private static Discovery? _discovery;
    private static SkinCatalog? _catalog;

    private sealed record Discovery(string Pack, string ScriptPath, IReadOnlyList<PoroOption> Options)
    {
        internal GodotObject? Factory { get; set; }
    }

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
        creature?.Monster is Osty && creature.PetOwner is { } owner && LocalContext.IsMe(owner);

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
            using var result = GetFactory(discovery).Call("data_for", skinId);
            if (result.AsGodotObject() is not Resource data)
            {
                return false;
            }

            spine.Call("set_skeleton_data_res", data);
            // Match NativeVisuals.Create(pet: true) in the author pack. The parent still
            // handles the game's health-based scaling and the player's size adjustments.
            spine.Scale = Vector2.One * 0.45f;
            NativeCreatureEffects.HideOstyFlame(spine);
            spine.SetMeta(CombatBodyMeta, true);

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
            using var result = GetFactory(discovery).Call("data_for", skinId);
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
        var catalog = SkinService.Catalog;
        if (catalog == null) return null;
        if (!ReferenceEquals(catalog, _catalog))
        {
            _discovery = Discover(catalog);
            _catalog = catalog;
        }
        return _discovery;
    }

    private static Discovery? Discover(SkinCatalog catalog)
    {
        var providers = catalog.Groups.SelectMany(group => group.Options)
            .Where(option => !option.IsComposition)
            .SelectMany(CandidateProviders).Distinct(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in providers)
        foreach (var pack in catalog.GetProviderResourcePackPaths(provider))
        {
            using var archive = PckArchive.Open(pack);
            var script = $"res://{provider}/{FactoryRelativePath}";
            if (!archive.Contains(script)) continue;

            var prefix = $"res://{provider}/{SpineFolder}/";
            var names = archive.Paths.Where(path => path.StartsWith(prefix, StringComparison.Ordinal) &&
                                                  path.EndsWith(".spine-json", StringComparison.Ordinal))
                .Select(path => path[prefix.Length..].Split('/')[0])
                .Where(LooksLikeCompanion).Distinct(StringComparer.Ordinal)
                .OrderBy(name => name.Length).ThenBy(name => name, StringComparer.Ordinal).ToArray();
            var original = names.FirstOrDefault(name => !name.Contains("maid", StringComparison.OrdinalIgnoreCase));
            if (original == null) continue;

            var options = new List<PoroOption> { new(OptionPrefix + "base", original, "魄罗 · 原皮") };
            var maid = names.FirstOrDefault(name => name.Contains("maid", StringComparison.OrdinalIgnoreCase));
            if (maid != null) options.Add(new(OptionPrefix + "maid", maid, "魄罗 · 女仆"));
            ModLog.Info($"魄罗外观：{provider} 提供 {options.Count} 档随从外观（{string.Join("/", options.Select(o => o.SkinId))}）。");
            return new Discovery(pack, script, options);
        }
        return null;
    }

    internal static IReadOnlyDictionary<string, (PckArchive Archive, string Path)> CompanionFiles(
        PckArchive archive, string script, IEnumerable<string> skins)
    {
        var root = script[..^FactoryRelativePath.Length];
        var folders = skins.Select(skin => $"{root}{SpineFolder}/{skin}/").ToArray();
        var index = PckResourceIndex.Build(
            new SkinModDescriptor("poro", "poro", archive.Path, false, null, false),
            archive, new(StringComparer.OrdinalIgnoreCase), _ => false);
        var files = new Dictionary<string, (PckArchive, string)>(StringComparer.OrdinalIgnoreCase)
        {
            [script] = (archive, script)
        };
        foreach (var path in archive.Paths.Where(path => folders.Any(folder => path.StartsWith(folder, StringComparison.Ordinal))))
        {
            files[path] = (archive, path);
            if (!path.EndsWith(".import", StringComparison.Ordinal)) continue;
            foreach (var file in index.TryBuildAsset(path[..^7])!.Files)
                files[file.Path] = (archive, file.Path);
        }
        return files;
    }

    private static GodotObject GetFactory(Discovery discovery)
    {
        if (discovery.Factory != null) return discovery.Factory;

        // The workshop pack is intentionally absent from res:// until the owner skin is selected.
        // Mount only the companion folders, their imported textures, and the factory script.
        using var archive = PckArchive.Open(discovery.Pack);
        var files = CompanionFiles(archive, discovery.ScriptPath, discovery.Options.Select(o => o.SkinId));
        var info = new FileInfo(discovery.Pack);
        var stamp = $"{info.FullName}|{info.Length}|{info.LastWriteTimeUtc.Ticks}";
        var key = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(stamp)));
        var output = Path.Combine(SkinChangerPaths.CacheDirectory, $"poro_{key}.pck");
        if (!File.Exists(output))
            SkinChangerPaths.WriteCache(output, () => PckArchive.WriteFromArchives(output, files));
        if (!SkinChangerPaths.LoadResourcePack(GlobalWriteSite.A32, output, replaceFiles: false))
            throw new IOException("无法挂载魄罗随从资源：" + output);

        var script = ResourceLoader.Load<GDScript>(discovery.ScriptPath);
        return discovery.Factory = script.New().AsGodotObject();
    }

    /// <summary>一个选项可能挂在几种 id 下面：提供者 id、有效提供者 id、选项 id 的前缀。</summary>
    private static IEnumerable<string> CandidateProviders(SkinOption option)
    {
        foreach (var candidate in new[]
                 {
                     option.ProviderId,
                     option.EffectiveProviderId,
                     option.Id.Split(':', 2)[0]
                 })
        {
            if (!string.IsNullOrWhiteSpace(candidate))
            {
                yield return candidate;
            }
        }
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

[HarmonyPatch(typeof(Osty), nameof(Osty.GenerateAnimator))]
internal static class PoroDeathAnimationPatch
{
    private static readonly System.Reflection.FieldInfo AnyState =
        AccessTools.Field(typeof(CreatureAnimator), "_anyState");

    private static void Postfix(MegaSprite controller, CreatureAnimator __result)
    {
        if (controller.BoundObject.HasMeta(PoroVariantBridge.CombatBodyMeta))
            StopDeathLoop((AnimState)AnyState.GetValue(__result)!);
    }

    internal static void StopDeathLoop(AnimState anyState)
    {
        // Both authored bot skeletons alias dead_loop to die. Keep the real death action,
        // and leave its final pose in place until the native Revive trigger arrives.
        if (anyState.CallTrigger(CreatureAnimator.deathTrigger) is { } death)
            death.NextState = null;
    }
}

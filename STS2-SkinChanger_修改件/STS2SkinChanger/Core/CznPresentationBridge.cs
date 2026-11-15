using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

// CznCore owns shared event subscriptions and shared Harmony callbacks. Removing a
// skin DLL's patches does not remove either. Route the profile boundary instead.
internal static class CznPresentationBridge
{
    private sealed record Binding(string Provider, object Profile, Type Target);
    private static readonly Dictionary<Assembly, string> Owners = new();
    private static readonly Dictionary<string, Binding> Profiles = new(StringComparer.Ordinal);
    private static readonly List<object> SelectionRuntimes = [];
    private static readonly Harmony Harmony = new(Entry.ModId + ".czn-presentation");
    private static bool _installed;
    private const BindingFlags Members = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    internal static void RegisterAssembly(string provider, Assembly assembly)
    {
        Owners[assembly] = provider;
        EnsureInstalled();
    }

    internal static void EnsureInstalled()
    {
        if (_installed) return;
        var core = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CznCore");
        var bootstrap = core?.GetType("CznCore.Presentation.CharacterSkinModBootstrap");
        var catalog = core?.GetType("CznCore.Presentation.CharacterAppearanceCatalog");
        var runtime = core?.GetType("CznCore.Presentation.CharacterSelectPresentationRuntime");
        if (bootstrap == null || catalog == null || runtime == null) return;
        try
        {
            Harmony.Patch(AccessTools.Method(bootstrap, "Init"), prefix: Patch(nameof(CaptureProfile)));
            foreach (var method in catalog.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                         .Where(m => m.Name is "IsSelected" or "IsActive"))
                Harmony.Patch(method, prefix: Patch(nameof(ProfileSelected)));
            foreach (var constructor in runtime.GetConstructors(Members))
                Harmony.Patch(constructor, postfix: Patch(nameof(CaptureSelectionRuntime)));
            var art = core!.GetType("CznCore.Presentation.BundledSkinCardArt");
            if (art != null) Harmony.Patch(AccessTools.PropertyGetter(art, "Enabled"), prefix: Patch(nameof(CardArtEnabled)));
            _installed = true;
            ModLog.Info("[CZN兼容] 已接入共享外观选择、事件所有权、选角展示与卡图边界。");
        }
        catch (Exception exception)
        {
            Harmony.UnpatchAll(Harmony.Id);
            ModLog.Warn("[CZN兼容] 共享呈现接口接入失败：" + exception.GetBaseException().Message);
        }
    }

    private static HarmonyMethod Patch(string name) => new(typeof(CznPresentationBridge), name);

    private static void CaptureProfile(Type __0, object __1)
    {
        // Gameplay characters (Tiphera) keep their native framework selection and events.
        if (!Owners.TryGetValue(__0.Assembly, out var provider)) return;
        var id = Get(__1, "ProfileId") as string;
        var target = AccessTools.Method(__1.GetType(), "ResolveTargetCharacterType").Invoke(__1, null) as Type;
        if (id != null && target != null) Profiles[id] = new(provider, __1, target);
    }

    private static Binding? Find(object profile) => Get(profile, "ProfileId") is string id
        ? Profiles.GetValueOrDefault(id) : null;

    private static object? Get(object value, string property) => value.GetType().GetProperty(property, Members)?.GetValue(value);

    private static bool ProfileSelected(object __0, object[] __args, ref bool __result)
    {
        var binding = Find(__0);
        if (binding == null || SkinService.Catalog == null) return true;
        var player = __args.OfType<Player>().FirstOrDefault();
        using var scope = player == null || FrameworkModelPreview.IsPreviewPlayer(player)
            ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId);
        var character = player?.Character ?? __args.OfType<CharacterModel>().FirstOrDefault();
        var group = character == null
            ? SkinService.Catalog.Groups.FirstOrDefault(g => g.Options.Any(o =>
                string.Equals(o.EffectiveProviderId, binding.Provider, StringComparison.OrdinalIgnoreCase)))
            : ContextualSkinControls.FindGroup(character.Id.Entry, character.GetType().Name);
        var selected = group == null ? null : SkinService.GetDisplayedSelection(group.Id);
        var provider = group?.Options.FirstOrDefault(o => o.Id.Equals(selected, StringComparison.OrdinalIgnoreCase))?.EffectiveProviderId;
        __result = provider != null && provider.Equals(binding.Provider, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    private static bool CardArtEnabled(object __instance, ref bool __result)
    {
        var profile = AccessTools.Field(__instance.GetType(), "_profile")?.GetValue(__instance);
        if (profile == null || Find(profile) == null) return true;
        // Their declarative card_replacements.json is already in SC's card catalog.
        // Shared Enter/Inspect patches must not impose the character's bundled card art.
        __result = false;
        return false;
    }

    private static void CaptureSelectionRuntime(object __instance)
    {
        if (!SelectionRuntimes.Any(value => ReferenceEquals(value, __instance))) SelectionRuntimes.Add(__instance);
    }

    internal static void ReapplySelection(NCharacterSelectScreen screen, CharacterModel character, NCharacterSelectButton button)
    {
        foreach (var runtime in SelectionRuntimes.ToArray())
        {
            try { AccessTools.Method(runtime.GetType(), "ApplyScreenSelection").Invoke(runtime, [screen, button, character]); }
            catch (Exception exception) { ModLog.Warn("[CZN兼容] 选角展示恢复失败：" + exception.GetBaseException().Message); }
        }
    }

    internal static bool ApplyPreviewBody(NCreatureVisuals visuals, CharacterModel character, string? groupId)
    {
        if (groupId == null) return false;
        var provider = SkinService.GetSelectedCreatureRuntimeProvider(groupId);
        var binding = Profiles.Values.FirstOrDefault(b => b.Provider.Equals(provider, StringComparison.OrdinalIgnoreCase) &&
            b.Target.IsInstanceOfType(character));
        if (binding == null || Get(binding.Profile, "BodySkeletonDataPath") is not string path) return false;
        // A fresh model only, before it enters the tree. Never replace a running Spine skeleton.
        if (visuals.IsInsideTree()) throw new InvalidOperationException("CZN 预览骨架必须在入树前绑定。");
        // NCreatureVisuals caches its current body in Ready, which has deliberately not
        // run yet. Resolve the fresh scene's sole Spine node instead of reading that cache.
        var sprites = Descendants(visuals).Where(n => n.GetClass().ToString() == "SpineSprite").ToArray();
        if (sprites.Length != 1) return false;
        var sprite = sprites[0];
        var resource = SkinService.GetOrLoadRuntimeResource(groupId, path, includeProviderDependencies: true);
        if (resource.GetClass().ToString() != "SpineSkeletonDataResource")
            throw new InvalidOperationException("CZN 预览骨架资源类型不正确：" + path);
        sprite.Set("skeleton_data_res", resource);
        return true;
    }

    private static IEnumerable<Node> Descendants(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            yield return child;
            foreach (var descendant in Descendants(child)) yield return descendant;
        }
    }
}

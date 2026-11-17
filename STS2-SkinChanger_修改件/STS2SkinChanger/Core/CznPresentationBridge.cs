using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
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
    [ThreadStatic] private static (Player Player, NMerchantCharacter Visual)? _nonCombatOwner;
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
            var identityPatch = Patch(nameof(RestoreNonCombatIdentity));
            identityPatch.priority = Priority.First;
            Harmony.Patch(AccessTools.Method(typeof(NMerchantCharacter), "_Ready"), prefix: identityPatch);
            Harmony.Patch(AccessTools.Method(typeof(NRestSiteCharacter), "_Ready"), prefix: identityPatch);
            var merchant = core!.GetType("CznCore.Presentation.MerchantPresentationRuntime", true)!;
            Harmony.Patch(AccessTools.Method(merchant, "ResolveMerchantCharacterPlayer"), prefix: Patch(nameof(ResolveMerchantOwner)));
            Harmony.Patch(AccessTools.Method(merchant, "Apply", [typeof(NMerchantCharacter)]), prefix: Patch(nameof(AllowMerchantPresentation)));
            var ownership = core.GetType("CznCore.Presentation.SkinEventOwnership", true)!;
            var merchantEvent = core.GetType("CznCore.Presentation.MerchantCharacterReadyEvent", true)!;
            var accepts = ownership.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(m => m.Name == "Accepts" && m.IsGenericMethodDefinition).MakeGenericMethod(merchantEvent);
            Harmony.Patch(accepts, prefix: Patch(nameof(AllowMerchantEvent)));
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

    internal static IDisposable BeginNonCombatOwnerScope(Player owner, NMerchantCharacter visual)
    {
        var previous = _nonCombatOwner;
        _nonCombatOwner = (owner, visual);
        return new OwnerScope(previous);
    }

    private sealed class OwnerScope((Player Player, NMerchantCharacter Visual)? previous) : IDisposable
    {
        private bool _disposed;
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _nonCombatOwner = previous;
        }
    }

    private static Player? MerchantOwner(NMerchantCharacter visual)
    {
        if (_nonCombatOwner is { } context && ReferenceEquals(context.Visual, visual)) return context.Player;
        var room = NMerchantRoom.Instance;
        if (room == null) return null;
        var players = AccessTools.Field(typeof(NMerchantRoom), "_players")?.GetValue(room) as List<Player>;
        if (players == null) return null;
        // PlayerVisuals identifies ownership; GetIndex counts unrelated/retiring siblings.
        var index = room.PlayerVisuals.ToList().FindIndex(node => ReferenceEquals(node, visual));
        return index >= 0 && index < players.Count ? players[index] : players.Count == 1 ? players[0] : null;
    }

    private static bool ResolveMerchantOwner(NMerchantCharacter __0, ref Player? __result)
    {
        __result = MerchantOwner(__0);
        return false;
    }

    private static bool AllowMerchantPresentation(object __instance, NMerchantCharacter __0)
    {
        var owner = MerchantOwner(__0);
        if (owner == null) return true; // native catalogue contexts keep their existing behavior
        var predicate = AccessTools.Field(__instance.GetType(), "_appliesTo")?.GetValue(__instance) as Func<Player?, bool>;
        return predicate?.Invoke(owner) ?? true;
    }

    private static bool AllowMerchantEvent(object[] __args, ref bool __result)
    {
        var profile = __args[0];
        var visual = Get(__args[1], "Node") as NMerchantCharacter;
        var owner = visual == null ? null : MerchantOwner(visual);
        if (owner == null)
        {
            if (Find(profile) == null) return true;
            __result = false;
            return false;
        }
        __result = (bool)AccessTools.Method(profile.GetType(), "AppliesTo", [typeof(Player)]).Invoke(profile, [owner])!;
        return false;
    }

    private static void RestoreNonCombatIdentity(Node __instance)
    {
        var canonical = NonCombatSceneIdentity.Resolve(__instance.SceneFilePath);
        if (canonical != __instance.SceneFilePath) __instance.SceneFilePath = canonical;
    }

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

    internal static void ReapplySelection(Control screen, CharacterModel character, NCharacterSelectButton button)
    {
        foreach (var runtime in SelectionRuntimes.ToArray())
        {
            try
            {
                var method = AccessTools.Method(runtime.GetType(), "ApplyScreenSelection");
                if (method?.GetParameters()[0].ParameterType.IsInstanceOfType(screen) == true)
                    method.Invoke(runtime, [screen, button, character]);
                else if (CharacterSelectScreenAccess.IsSupported(screen) && !button.IsLocked &&
                    CharacterSelectScreenAccess.Background(screen) is { } host)
                {
                    // The shared framework already exposes its native mode event. Invoke only
                    // the selected profile: several skins can target the same character type.
                    var profile = AccessTools.Field(runtime.GetType(), "_profile")?.GetValue(runtime);
                    var binding = profile == null ? null : Find(profile);
                    var group = ContextualSkinControls.FindGroup(character.Id.Entry, character.GetType().Name);
                    if (binding != null && group != null && binding.Target.IsInstanceOfType(character) &&
                        string.Equals(binding.Provider, SkinService.GetSelectedCreatureRuntimeProvider(group.Id), StringComparison.OrdinalIgnoreCase))
                        ReapplyModeRuntime(runtime, screen, host, character);
                }
            }
            catch (Exception exception) { ModLog.Warn("[CZN兼容] 选角展示恢复失败：" + exception.GetBaseException().Message); }
        }
    }

    internal static bool ReapplyModeRuntime(object runtime, Control screen, Control host, CharacterModel character)
    {
        var eventType = runtime.GetType().Assembly.GetType("CznCore.Presentation.ModeSelectionBackground");
        var constructor = eventType?.GetConstructor(Members, null,
            [typeof(Control), typeof(Control), typeof(CharacterModel), typeof(bool)], null);
        var apply = eventType == null ? null : AccessTools.Method(runtime.GetType(), "ApplyModeBackground", [eventType]);
        if (constructor == null || apply == null) return false;
        var selection = constructor.Invoke([screen, host, character, true]);
        apply.Invoke(runtime, [selection]);
        return true;
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

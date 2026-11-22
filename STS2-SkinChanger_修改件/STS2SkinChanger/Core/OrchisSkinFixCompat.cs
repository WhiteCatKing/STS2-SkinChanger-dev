using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>Scopes Orchis 0.1.8's independent room and text callbacks to the player's selected appearance.</summary>
internal static class OrchisSkinFixCompat
{
    private const string FixAssembly = "STS2-OrchisNecrobinderSkinFix";
    internal const string ProviderId = "OrchisNecrobinderSkinMod";
    private const string FixNamespace = "STS2OrchisNecrobinderSkinFix.Patches.";
    private const string ProviderEntry = "OrchisNecrobinderSkinMod.Scripts.Entry";
    private const string TextReplacePatchType = "OrchisNecrobinderSkinMod.Scripts.TextReplacePatch";
    private const string FixOstyScalingPatch = FixNamespace + "OrchisOstyScalingPatch";
    private const string Body = "necrobinder";
    private const string OstyGroup = "osty";
    private const string RestBodyPath = "res://OrchisNecrobinderSkinMod/animations/rest_site/necrobinder/rest_site_necrobinder_skel_data.tres";
    private const string RestOstyPath = "res://OrchisNecrobinderSkinMod/animations/rest_site/necrobinder/lloyd_relax_skel_data.tres";
    private static readonly Harmony Patcher = new(Entry.ModId + ".orchis-fix-selection");
    private static readonly object Sync = new();
    private static readonly HashSet<MethodBase> Patched = [];
    private static bool _watching;
    [ThreadStatic] private static Player? _restSitePlayer;

    internal static void Install()
    {
        lock (Sync)
        {
            if (!_watching)
            {
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
                _watching = true;
            }
            InstallLoaded();
        }
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        if (!IsNamed(args.LoadedAssembly, FixAssembly) && !IsNamed(args.LoadedAssembly, ProviderId)) return;
        lock (Sync) InstallLoaded();
    }

    private static void InstallLoaded()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var hasFix = assemblies.Any(assembly => IsNamed(assembly, FixAssembly));
        foreach (var assembly in assemblies)
        {
            if (IsNamed(assembly, FixAssembly)) InstallFixCallbacks(assembly);
            else if (IsNamed(assembly, ProviderId))
            {
                InstallProviderTextCallbacks(assembly);
                InstallProviderOstyCallbacks(assembly);
                if (hasFix) InstallProviderCallbacks(assembly);
            }
        }
    }

    private static bool IsNamed(Assembly assembly, string name) =>
        !assembly.IsDynamic && string.Equals(assembly.GetName().Name, name, StringComparison.OrdinalIgnoreCase);

    private static void InstallFixCallbacks(Assembly assembly)
    {
        Patch(assembly, FixNamespace + "NecrobinderLowHealthAnimatorPatch", "Prefix",
            [typeof(CharacterModel), typeof(object[]), typeof(CreatureAnimator).MakeByRefType()], nameof(AllowAnimator));
        Patch(assembly, FixNamespace + "NMerchantRoomAfterRoomIsLoadedPatch", "ApplyNecrobinderVisuals",
            [typeof(IReadOnlyList<NMerchantCharacter>), typeof(IReadOnlyList<Player>)], nameof(FilterMerchantPlayers));
        Patch(assembly, FixNamespace + "NRestSiteCharacterReadyPatch", "GetOrLoadSkeleton",
            [typeof(string), typeof(Resource).MakeByRefType()], nameof(FilterRestSiteSkeleton));
        Patch(assembly, FixNamespace + "NRestSiteCharacterReadyPatch", "Postfix",
            [typeof(NRestSiteCharacter)], nameof(EnterRestSitePlayer), nameof(ExitRestSitePlayer));
        Patch(assembly, FixOstyScalingPatch, "ResolveScaleFactor",
            [typeof(float), typeof(NCreature), typeof(float)], nameof(AllowFixOstyScale));
    }

    private static void InstallProviderTextCallbacks(Assembly assembly)
    {
        Patch(assembly, TextReplacePatchType, "ShouldReplace",
            [typeof(LocString)], nameof(AllowTextReplace));
        Patch(assembly, TextReplacePatchType, "TryGetCharacterText",
            [typeof(LocString), typeof(string).MakeByRefType()], nameof(AllowCharacterTextReplace));
    }

    private static void InstallProviderOstyCallbacks(Assembly assembly)
    {
        Patch(assembly, ProviderEntry + "+CreatureCreateVisualsPatch", "Postfix",
            [typeof(Creature), typeof(NCreatureVisuals).MakeByRefType()], nameof(AllowCreatureVisuals));
        Patch(assembly, ProviderEntry + "+ScaleAndPositionOstyPatch", "Prefix",
            [typeof(NCreature), typeof(float), typeof(double)], nameof(AllowOstyScale));
        Patch(assembly, ProviderEntry, "SnapLocalOstyToOwner",
            [typeof(NCreature)], nameof(AllowOstyNode));
        // The author's ordering callback can be deferred past a selection change, so gate
        // the method that actually moves the node as well as the original scale prefix.
        Patch(assembly, ProviderEntry, "PutCombatOstyBelowCharacters",
            [typeof(NCreature)], nameof(AllowOstyNode));
    }

    private static void InstallProviderCallbacks(Assembly assembly)
    {
        Patch(assembly, ProviderEntry, "ApplyNecrobinderCombatScale", [typeof(NCreature)], nameof(AllowCreatureNodeBody));
        Patch(assembly, ProviderEntry + "+CharacterModelCreateVisualsPatch", "Postfix",
            [typeof(CharacterModel), typeof(NCreatureVisuals).MakeByRefType()], nameof(AllowCharacterBody));
        Patch(assembly, ProviderEntry + "+CharacterSelectBgPatch", "Postfix",
            [typeof(CharacterModel), typeof(string).MakeByRefType()], nameof(AllowCharacterBody));
        Patch(assembly, ProviderEntry + "+CharacterSelectScreenPatch", "Postfix",
            [typeof(NCharacterSelectScreen), typeof(object[])], nameof(AllowSelectBackground));
    }

    private static void Patch(Assembly assembly, string typeName, string methodName, Type[] signature,
        string prefix, string? finalizer = null)
    {
        try
        {
            var type = assembly.GetType(typeName, throwOnError: false);
            var target = type == null ? null : AccessTools.DeclaredMethod(type, methodName, signature);
            if (target == null || !Patched.Add(target)) return;
            try
            {
                Patcher.Patch(target,
                    prefix: new HarmonyMethod(typeof(OrchisSkinFixCompat), prefix) { priority = Priority.First },
                    finalizer: finalizer == null ? null : new HarmonyMethod(typeof(OrchisSkinFixCompat), finalizer));
            }
            catch
            {
                Patched.Remove(target);
                throw;
            }
        }
        catch (Exception error)
        {
            ModLog.Warn($"[兼容] 奥契丝补丁 {typeName}.{methodName} 安装失败：{error.GetBaseException().Message}");
        }
    }

    private static bool AllowTextReplace(ref bool __result)
    {
        if (AllowsText()) return true;
        __result = false;
        return false;
    }

    private static bool AllowCharacterTextReplace(ref string __1, ref bool __result)
    {
        if (AllowsText()) return true;
        __1 = string.Empty;
        __result = false;
        return false;
    }

    internal static bool AllowsText() => Allows(Body);

    private static bool AllowCreatureVisuals(Creature __0) =>
        __0.Monster is Osty ? Allows(OstyGroup, __0.PetOwner) : AllowCreatureBody(__0);

    private static bool AllowOstyNode(NCreature __0) =>
        __0.Entity.Monster is not Osty || Allows(OstyGroup, __0.Entity.PetOwner);

    private static bool AllowOstyScale(NCreature __0, ref bool __result)
    {
        if (AllowOstyNode(__0)) return true;
        __result = true; // The upstream prefix was skipped, so the game's scaling still runs.
        return false;
    }

    private static bool AllowFixOstyScale(float __0, NCreature __1, ref float __result)
    {
        if (AllowOstyNode(__1)) return true;
        __result = __0;
        return false;
    }

    private static bool AllowCreatureBody(Creature __0) =>
        __0.Player?.Character is not Necrobinder || Allows(Body, __0.Player);

    private static bool AllowCreatureNodeBody(NCreature __0) => AllowCreatureBody(__0.Entity);

    private static bool AllowCharacterBody(CharacterModel __0) =>
        __0 is not Necrobinder || Allows(Body);

    private static bool AllowAnimator(CharacterModel __0, ref bool __result)
    {
        if (AllowCharacterBody(__0)) return true;
        // We are patching a bool-returning Harmony prefix. Pass through to the game's animator.
        __result = true;
        return false;
    }

    private static bool AllowSelectBackground(object[] __1)
    {
        var character = __1.Length > 1 ? __1[1] as CharacterModel : (__1.FirstOrDefault() as NCharacterSelectButton)?.Character;
        return character is not Necrobinder || Allows(Body);
    }

    private static bool FilterMerchantPlayers(
        ref IReadOnlyList<NMerchantCharacter> __0, ref IReadOnlyList<Player> __1)
    {
        var count = Math.Min(__0.Count, __1.Count);
        var players = __1;
        var kept = Enumerable.Range(0, count).Where(index => players[index].Character is Necrobinder && Allows(Body, players[index])).ToArray();
        if (kept.Length == 0) return false;
        var visuals = __0;
        __0 = kept.Select(index => visuals[index]).ToArray();
        __1 = kept.Select(index => players[index]).ToArray();
        return true;
    }

    private static void EnterRestSitePlayer(NRestSiteCharacter __0, out Player? __state)
    {
        __state = _restSitePlayer;
        _restSitePlayer = __0.Player;
    }

    private static void ExitRestSitePlayer(Player? __state) => _restSitePlayer = __state;

    private static bool FilterRestSiteSkeleton(string __0, ref Resource? __result)
    {
        var group = RestSkeletonGroup(__0);
        if (_restSitePlayer == null || group == null || Allows(group, _restSitePlayer)) return true;
        __result = null;
        return false;
    }

    internal static string? RestSkeletonGroup(string path) => path switch
    {
        RestBodyPath => Body,
        // The companion is part of the authored room composition, just like the merchant actor.
        RestOstyPath => Body,
        _ => null
    };

    private static bool Allows(string group, Player? player = null)
    {
        var catalog = SkinService.Catalog;
        if (catalog == null) return true;
        using var scope = player == null || FrameworkModelPreview.IsPreviewPlayer(player)
            ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId);
        return OwnsAppearance(catalog.ResolveVisualProviderId(SkinService.GetDisplayedSelection(group)),
            OstyImpostorCompat.MustDefer(group));
    }

    internal static bool OwnsAppearance(string providerId, bool deferredToImpostor) =>
        !deferredToImpostor && ProviderInstanceIdentityPolicy.IsOptionSelectionAlias(ProviderId, ProviderId, ProviderId, providerId);
}

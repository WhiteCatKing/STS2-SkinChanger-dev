using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using MerchantPatch = STS2OrchisNecrobinderSkinFix.Patches.NMerchantRoomAfterRoomIsLoadedPatch;
using RestPatch = STS2OrchisNecrobinderSkinFix.Patches.NRestSiteCharacterReadyPatch;

internal static class OrchisRoomCallbackTests
{
    private static Player _player = null!;
    private static string _body = SkinCatalog.BaseOptionId;
    private static string _osty = SkinCatalog.BaseOptionId;

    internal static void Run()
    {
        var temp = Directory.CreateTempSubdirectory("sc-orchis-room-");
        var oldWriter = PckArchive.WriteOutput;
        var previousCatalog = SkinService.Catalog;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var harness = new Harmony("sc.tests.orchis-callback");
        try
        {
            PckArchive.WriteOutput = null;
            var baseline = Path.Combine(temp.FullName, "base.pck");
            PckArchive.Write(baseline, new Dictionary<string, byte[]> { ["res://fixture.bin"] = [1] });
            using var catalog = SkinCatalog.Build(baseline, []);
            catalogSetter.Invoke(null, [catalog]);
            _player = Uninitialized<Player>();
            AccessTools.Field(typeof(Player), "<Character>k__BackingField").SetValue(_player, Uninitialized<Necrobinder>());
            var other = Uninitialized<Player>();
            AccessTools.Field(typeof(Player), "<Character>k__BackingField").SetValue(other, Uninitialized<Ironclad>());
            harness.Patch(AccessTools.Method(typeof(SkinService), "GetDisplayedSelection"),
                prefix: new HarmonyMethod(typeof(OrchisRoomCallbackTests), nameof(GetSelection)));
            harness.Patch(AccessTools.PropertyGetter(typeof(NRestSiteCharacter), "Player"),
                prefix: new HarmonyMethod(typeof(OrchisRoomCallbackTests), nameof(GetRestPlayer)));
            AccessTools.Method(typeof(OrchisSkinFixCompat), "InstallFixCallbacks").Invoke(null, [typeof(OrchisRoomCallbackTests).Assembly]);

            var merchant = AccessTools.Method(typeof(MerchantPatch), "ApplyNecrobinderVisuals");
            var rest = AccessTools.Method(typeof(RestPatch), "Postfix");
            NMerchantCharacter[] visuals = [Uninitialized<NMerchantCharacter>(), Uninitialized<NMerchantCharacter>()];
            Player[] players = [other, _player];
            var site = Uninitialized<NRestSiteCharacter>();
            _osty = OrchisSkinFixCompat.ProviderId;
            merchant.Invoke(null, [visuals, players]);
            rest.Invoke(null, [site]);
            Require(MerchantPatch.LastPlayer == null && !RestPatch.AppliedBody && RestPatch.AppliedOsty,
                "Osty-only selection must keep the original body in merchant and rest site.");

            _body = OrchisSkinFixCompat.ProviderId;
            _osty = SkinCatalog.BaseOptionId;
            merchant.Invoke(null, [visuals, players]);
            rest.Invoke(null, [site]);
            Require(ReferenceEquals(MerchantPatch.LastPlayer, _player) && ReferenceEquals(MerchantPatch.LastVisual, visuals[1]) &&
                RestPatch.AppliedBody && !RestPatch.AppliedOsty,
                "Orchis body selection must retain paired merchant visuals and body-only campfire repair.");
            Console.WriteLine("Orchis room callbacks passed: real detours on synthetic merchant and mixed rest-site methods.");
        }
        finally
        {
            harness.UnpatchAll(harness.Id);
            catalogSetter.Invoke(null, [previousCatalog]);
            PckArchive.WriteOutput = oldWriter;
            temp.Delete(recursive: true);
        }
    }

    internal static T Uninitialized<T>() where T : class
    {
        var value = (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
        GC.SuppressFinalize(value);
        return value;
    }

    private static bool GetSelection(string groupId, ref string __result)
    {
        __result = groupId == "osty" ? _osty : _body;
        return false;
    }

    private static bool GetRestPlayer(ref Player __result)
    {
        __result = _player;
        return false;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

namespace STS2OrchisNecrobinderSkinFix.Patches
{
    internal static class NMerchantRoomAfterRoomIsLoadedPatch
    {
        internal static Player? LastPlayer;
        internal static NMerchantCharacter? LastVisual;

        private static void ApplyNecrobinderVisuals(IReadOnlyList<NMerchantCharacter> visuals, IReadOnlyList<Player> players)
        {
            LastPlayer = players.Single();
            LastVisual = visuals.Single();
        }
    }

    internal static class NRestSiteCharacterReadyPatch
    {
        private const string Prefix = "res://OrchisNecrobinderSkinMod/animations/rest_site/necrobinder/";
        private static Resource _skeleton = OrchisRoomCallbackTests.Uninitialized<Resource>();
        internal static bool AppliedBody;
        internal static bool AppliedOsty;

        private static void Postfix(NRestSiteCharacter host)
        {
            AppliedBody = GetOrLoadSkeleton(Prefix + "rest_site_necrobinder_skel_data.tres", ref _skeleton) != null;
            AppliedOsty = GetOrLoadSkeleton(Prefix + "lloyd_relax_skel_data.tres", ref _skeleton) != null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Resource GetOrLoadSkeleton(string path, ref Resource skeleton) => skeleton;
    }
}

using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Ui;
using ProviderEntry = OrchisNecrobinderSkinMod.Scripts.Entry;
using ScalePatch = STS2OrchisNecrobinderSkinFix.Patches.OrchisOstyScalingPatch;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;
using MerchantPatch = STS2OrchisNecrobinderSkinFix.Patches.NMerchantRoomAfterRoomIsLoadedPatch;
using RestPatch = STS2OrchisNecrobinderSkinFix.Patches.NRestSiteCharacterReadyPatch;
using TextPatch = OrchisNecrobinderSkinMod.Scripts.TextReplacePatch;

internal static class OrchisRoomCallbackTests
{
    private static Player _player = null!;
    private static string _body = SkinCatalog.BaseOptionId;
    private static string _osty = SkinCatalog.BaseOptionId;
    private static bool _fixFeatureToggle = true;
    private static bool _deferOsty;
    private static bool _localOsty;
    private static bool _includedOstyInLayout;
    private static Node _allyContainer = null!;
    private static NCreature _ostyNode = null!;
    private static Tween _oldScaleTween = null!;
    private static bool _killedScaleTween;
    private static (float Health, double Duration)? _nativeScaleRequest;
    private static readonly Dictionary<Control, Vector2> Positions = new(ReferenceEqualityComparer.Instance);

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
            AccessTools.Method(typeof(OrchisSkinFixCompat), "InstallProviderTextCallbacks").Invoke(null, [typeof(OrchisRoomCallbackTests).Assembly]);
            harness.Patch(AccessTools.Method(typeof(TextPatch), "ShouldReplace", [typeof(LocString)]),
                postfix: new HarmonyMethod(typeof(OrchisRoomCallbackTests), nameof(SimulateFixTextShouldReplacePostfix)));
            harness.Patch(AccessTools.Method(typeof(TextPatch), "TryGetCharacterText", [typeof(LocString), typeof(string).MakeByRefType()]),
                prefix: new HarmonyMethod(typeof(OrchisRoomCallbackTests), nameof(SimulateFixCharacterTextPrefix)));

            var merchant = AccessTools.Method(typeof(MerchantPatch), "ApplyNecrobinderVisuals");
            var rest = AccessTools.Method(typeof(RestPatch), "Postfix");
            var locString = Uninitialized<LocString>();
            NMerchantCharacter[] visuals = [Uninitialized<NMerchantCharacter>(), Uninitialized<NMerchantCharacter>()];
            Player[] players = [other, _player];
            var site = Uninitialized<NRestSiteCharacter>();
            _fixFeatureToggle = true;
            _body = SkinCatalog.BaseOptionId;
            _osty = SkinCatalog.BaseOptionId;
            Require(!TextPatch.ShouldReplace(locString) &&
                !TextPatch.TryGetCharacterText(locString, out var vanillaText) &&
                vanillaText == string.Empty,
                "Vanilla Necrobinder selection must block Orchis text replacement.");

            _body = "necrobinderSkin_NSFW.Ver.2.0";
            _osty = SkinCatalog.BaseOptionId;
            Require(!TextPatch.ShouldReplace(locString) &&
                !TextPatch.TryGetCharacterText(locString, out var otherSkinText) &&
                otherSkinText == string.Empty,
                "Other Necrobinder skins must block Orchis text replacement.");

            _body = SkinCatalog.BaseOptionId;
            _osty = OrchisSkinFixCompat.ProviderId;
            merchant.Invoke(null, [visuals, players]);
            rest.Invoke(null, [site]);
            Require(MerchantPatch.LastPlayer == null && !RestPatch.AppliedBody && !RestPatch.AppliedOsty,
                "Osty-only selection must leave the merchant and both campfire actors at their original visuals.");
            Require(!TextPatch.ShouldReplace(locString) &&
                !TextPatch.TryGetCharacterText(locString, out var ostyOnlyText) &&
                ostyOnlyText == string.Empty,
                "Osty-only Orchis selection must not leak Orchis text onto non-Orchis Necrobinder.");

            _body = OrchisSkinFixCompat.ProviderId;
            _osty = SkinCatalog.BaseOptionId;
            merchant.Invoke(null, [visuals, players]);
            rest.Invoke(null, [site]);
            Require(ReferenceEquals(MerchantPatch.LastPlayer, _player) && ReferenceEquals(MerchantPatch.LastVisual, visuals[1]) &&
                RestPatch.AppliedBody && RestPatch.AppliedOsty,
                "Orchis body selection must retain paired merchant and campfire visuals regardless of the independent combat companion.");
            Require(TextPatch.ShouldReplace(locString) &&
                TextPatch.TryGetCharacterText(locString, out var orchisText) &&
                orchisText == "奥契丝",
                "Orchis body selection must allow Orchis character and name text replacement.");

            _fixFeatureToggle = false;
            Require(!TextPatch.ShouldReplace(locString) &&
                !TextPatch.TryGetCharacterText(locString, out var fixDisabledText) &&
                fixDisabledText == string.Empty,
                "When Orchis is selected, OrchisSkinFix's own feature toggle must still be respected.");

            _fixFeatureToggle = true;
            _body = "necrobinderSkin_NSFW.Ver.2.0";
            Require(!TextPatch.ShouldReplace(locString) &&
                !TextPatch.TryGetCharacterText(locString, out var switchedBackText) &&
                switchedBackText == string.Empty,
                "Switching Necrobinder away from Orchis must immediately restore non-Orchis text.");
            Console.WriteLine("Orchis room & text callbacks passed: real detours on synthetic merchant, rest-site, and text-replace methods.");
            VerifyOstyCallbacksAndPlacement(harness);
        }
        finally
        {
            harness.UnpatchAll(harness.Id);
            catalogSetter.Invoke(null, [previousCatalog]);
            PckArchive.WriteOutput = oldWriter;
            temp.Delete(recursive: true);
        }
    }

    private static void VerifyOstyCallbacksAndPlacement(Harmony harness)
    {
        var fixture = typeof(OrchisRoomCallbackTests).Assembly;
        AccessTools.DeclaredMethod(typeof(OrchisSkinFixCompat), "InstallProviderOstyCallbacks").Invoke(null, [fixture]);
        harness.Patch(AccessTools.DeclaredMethod(typeof(OstyImpostorCompat), nameof(OstyImpostorCompat.MustDefer)),
            prefix: Patch(nameof(DeferOsty)));
        var osty = Uninitialized<Creature>();
        AccessTools.Field(typeof(Creature), "<Monster>k__BackingField").SetValue(osty, Uninitialized<Osty>());
        osty.PetOwner = _player;
        _ostyNode = MakeNode(osty);
        NCreatureVisuals? visuals = null;

        foreach (var (body, summon, defer, allowed) in new[]
                 {
                     ("necrobinderSkin_NSFW.Ver.2.0", SkinCatalog.BaseOptionId, false, false),
                     (OrchisSkinFixCompat.ProviderId, SkinCatalog.BaseOptionId, true, false),
                     (OrchisSkinFixCompat.ProviderId, SkinCatalog.BaseOptionId, false, false),
                     (SkinCatalog.BaseOptionId, OrchisSkinFixCompat.ProviderId, false, true),
                     (OrchisSkinFixCompat.ProviderId, OrchisSkinFixCompat.ProviderId, true, false),
                     (OrchisSkinFixCompat.ProviderId, OrchisSkinFixCompat.ProviderId, false, true),
                     ("necrobinderSkin_NSFW.Ver.2.0", SkinCatalog.BaseOptionId, false, false)
                 })
        {
            _body = body;
            _osty = summon;
            _deferOsty = defer;
            ProviderEntry.Calls = 0;
            ProviderEntry.CreatureCreateVisualsPatch.Postfix(osty, ref visuals);
            var vanilla = ProviderEntry.ScaleAndPositionOstyPatch.Prefix(_ostyNode, 50f, 0d);
            ProviderEntry.SnapLocalOstyToOwner(_ostyNode);
            ProviderEntry.PutCombatOstyBelowCharacters(_ostyNode);

            Require(ProviderEntry.Calls == (allowed ? 4 : 0) && vanilla != allowed,
                "The author must only change a selected, independently owned Orchis summon.");
            Require(ScalePatch.ResolveScaleFactor(0.8f, _ostyNode, 50f) == (allowed ? 1f : 0.8f),
                "The optional fix must return the native scale when Orchis does not own Osty.");
        }

        var runtime = typeof(CharacterAppearanceRuntime);
        var scale = (System.Reflection.MethodInfo?)AccessTools.Field(runtime, "OstyScaleAndPositionMethod").GetValue(null);
        Require(scale != null, "Both game references must resolve the public native Osty placement method.");
        var classify = AccessTools.DeclaredMethod(typeof(ManagedSkinModLoader), "ClassifyMonsterVisualCallback");
        Require(Equals(classify.Invoke(null, [scale]), MonsterVisualCallback.OstyPositioning),
            "The legacy and renamed native methods must both be classified as Osty positioning.");

        // Run the actual hot layout method with a stand-in for Godot's tree and controls.
        var playerCreature = Uninitialized<Creature>();
        AccessTools.Field(typeof(Creature), "<Player>k__BackingField").SetValue(playerCreature, _player);
        AccessTools.Field(typeof(Player), "<Creature>k__BackingField").SetValue(_player, playerCreature);
        var owner = MakeNode(playerCreature);
        var room = Uninitialized<NCombatRoom>();
        AccessTools.Field(typeof(NCombatRoom), "_creatureNodes").SetValue(room, new List<NCreature> { owner, _ostyNode });
        _allyContainer = Uninitialized<Node>();
        harness.Patch(AccessTools.DeclaredMethod(typeof(LocalContext), nameof(LocalContext.IsMe), [typeof(Player)]),
            prefix: Patch(nameof(IsLocalOsty)));
        harness.Patch(AccessTools.DeclaredMethod(typeof(NCombatRoom), nameof(NCombatRoom.PositionPlayersAndPets)),
            prefix: Patch(nameof(PositionPlayers)));
        harness.Patch(AccessTools.DeclaredMethod(typeof(NCreature), nameof(NCreature.GetOstyOffsetFromPlayer)),
            prefix: Patch(nameof(GetOstyOffset)));
        harness.Patch(AccessTools.DeclaredMethod(runtime, "RefreshCreatureAnchors"), prefix: Patch(nameof(SkipNative)));
        harness.Patch(typeof(Node).GetMethod(nameof(Node.GetParent), 0, Type.EmptyTypes)!, prefix: Patch(nameof(GetAllyContainer)));
        harness.Patch(AccessTools.DeclaredMethod(typeof(Node), nameof(Node.GetIndex), [typeof(bool)]),
            prefix: Patch(nameof(SkipNative)));
        harness.Patch(AccessTools.PropertyGetter(typeof(Control), nameof(Control.Position)), prefix: Patch(nameof(ReadPosition)));
        harness.Patch(AccessTools.PropertySetter(typeof(Control), nameof(Control.Position)), prefix: Patch(nameof(WritePosition)));

        _oldScaleTween = Uninitialized<Tween>();
        AccessTools.Field(typeof(Creature), "_maxHp").SetValue(osty, 80);
        var scaleTween = AccessTools.Field(typeof(NCreature), "_scaleTween");
        harness.Patch(scale!, prefix: Patch(nameof(RecordNativeScale)));
        harness.Patch(AccessTools.DeclaredMethod(typeof(Tween), nameof(Tween.IsRunning)), prefix: Patch(nameof(IsRunning)));
        harness.Patch(AccessTools.DeclaredMethod(typeof(Tween), nameof(Tween.Kill)), prefix: Patch(nameof(KillScaleTween)));
        harness.Patch(AccessTools.DeclaredMethod(typeof(GodotObject), nameof(GodotObject.IsInstanceValid), [typeof(GodotObject)]),
            prefix: Patch(nameof(RecognizeTestTween)));

        var captured = AccessTools.Field(runtime, "_playerLayoutRoom");
        var previous = captured.GetValue(null);
        captured.SetValue(null, new WeakReference<NCombatRoom>(room));
        try
        {
            var refresh = AccessTools.DeclaredMethod(runtime, "RefreshPlayerAndPetLayout");
            for (var round = 0; round < 24; round++)
            {
                _localOsty = round % 3 != 2;
                _includedOstyInLayout = true;
                Positions[owner] = new Vector2(15f + round, 20f);
                Positions[_ostyNode] = Vector2.Zero;
                var hasPendingScale = round % 4 == 0;
                scaleTween.SetValue(_ostyNode, hasPendingScale ? _oldScaleTween : null);
                _killedScaleTween = false;
                _nativeScaleRequest = null;
                refresh.Invoke(null, [room]);

                Require(_includedOstyInLayout != _localOsty,
                    "The generic formation should keep remote pets and exclude local Osty.");
                Require(Positions[_ostyNode] == (_localOsty ? Positions[owner] + new Vector2(130f, -12f) : Vector2.Zero),
                    "After the owner moves, local Osty must be placed at the game's summon offset.");
                var retargetsScale = _localOsty && hasPendingScale;
                (float Health, double Duration)? expectedScale = retargetsScale ? (80f, 0d) : null;
                Require(_killedScaleTween == retargetsScale &&
                        _nativeScaleRequest == expectedScale,
                    "Only a pending local health tween should be replaced at the new position.");
            }
        }
        finally
        {
            captured.SetValue(null, previous);
            Positions.Clear();
            _deferOsty = false;
        }

        Console.WriteLine("Orchis Osty passed: callback selection, native scale compatibility and 24 hot layouts.");

        static NCreature MakeNode(Creature entity)
        {
            var node = Uninitialized<NCreature>();
            AccessTools.Field(typeof(NCreature), "<Entity>k__BackingField").SetValue(node, entity);
            return node;
        }
    }

    private static HarmonyMethod Patch(string name) => new(typeof(OrchisRoomCallbackTests), name);

    private static bool DeferOsty(string groupId, ref bool __result)
    {
        __result = groupId == "osty" && _deferOsty;
        return false;
    }

    private static bool IsLocalOsty(ref bool __result) { __result = _localOsty; return false; }

    private static bool PositionPlayers(List<NCreature> creatureNodes)
    {
        _includedOstyInLayout = creatureNodes.Contains(_ostyNode);
        var owner = creatureNodes.Single(node => node.Entity.IsPlayer);
        Positions[owner] += new Vector2(-200f, 50f);
        return false;
    }

    private static bool GetOstyOffset(ref Vector2 __result) { __result = new Vector2(130f, -12f); return false; }
    private static bool GetAllyContainer(ref Node __result) { __result = _allyContainer; return false; }
    private static bool ReadPosition(Control __instance, ref Vector2 __result) { __result = Positions[__instance]; return false; }
    private static bool WritePosition(Control __instance, Vector2 value) { Positions[__instance] = value; return false; }
    private static bool SkipNative() => false;

    private static bool RecognizeTestTween(GodotObject __0, ref bool __result)
    {
        if (!ReferenceEquals(__0, _oldScaleTween)) return true;
        __result = true;
        return false;
    }

    private static bool IsRunning(ref bool __result) { __result = true; return false; }
    private static bool KillScaleTween() { _killedScaleTween = true; return false; }

    private static bool RecordNativeScale(float __0, double __1)
    {
        _nativeScaleRequest = (__0, __1);
        return false;
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

    private static void SimulateFixTextShouldReplacePostfix(ref bool __result)
    {
        if (__result && !_fixFeatureToggle) __result = false;
    }

    private static bool SimulateFixCharacterTextPrefix(ref string __1, ref bool __result)
    {
        if (_fixFeatureToggle) return true;
        __1 = string.Empty;
        __result = false;
        return false;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

namespace STS2OrchisNecrobinderSkinFix.Patches
{
    internal static class OrchisOstyScalingPatch
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static float ResolveScaleFactor(float nativeFactor, NCreature creature, float health) => 1f;
    }

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

namespace OrchisNecrobinderSkinMod.Scripts
{
    internal static class Entry
    {
        internal static int Calls;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void SnapLocalOstyToOwner(NCreature osty) => Calls++;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void PutCombatOstyBelowCharacters(NCreature osty) => Calls++;

        internal static class CreatureCreateVisualsPatch
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static void Postfix(Creature creature, ref NCreatureVisuals? visuals) => Calls++;
        }

        internal static class ScaleAndPositionOstyPatch
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            internal static bool Prefix(NCreature osty, float health, double duration)
            {
                Calls++;
                return false;
            }
        }
    }

    internal static class TextReplacePatch
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool ShouldReplace(LocString locString) => locString != null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static bool TryGetCharacterText(LocString locString, out string text)
        {
            text = locString != null ? "奥契丝" : string.Empty;
            return locString != null;
        }
    }
}

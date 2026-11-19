using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class FoolWaifuIdentityTests
{
    private static string _selection = SkinCatalog.BaseOptionId;
    internal static readonly Texture2D NativeTexture = MakeTexture();
    internal static readonly Texture2D WaifuTexture = MakeTexture();

    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-fool-waifu-");
        var writer = PckArchive.WriteOutput;
        var previousCatalog = SkinService.Catalog;
        var catalogSetter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var harness = new Harmony("sc.tests.fool-waifu-selection");
        var author = new Harmony(FoolWaifu.ResumeSkinCompatibility.Owner);
        var original = AccessTools.Method(typeof(FoolWaifu.ResumeSkinCompatibility), "BeforeRun");
        var authorForwarder = original.CreateDelegate<Action<RunState?>>();
        try
        {
            PckArchive.WriteOutput = null;
            var pack = Path.Combine(root.FullName, "base.pck");
            PckArchive.Write(pack, new Dictionary<string, byte[]> { ["res://test.bin"] = [1] });
            using var catalog = SkinCatalog.Build(pack, []);
            catalogSetter.Invoke(null, [catalog]);
            harness.Patch(AccessTools.Method(typeof(SkinService), "GetDisplayedSelection"),
                prefix: new HarmonyMethod(typeof(FoolWaifuIdentityTests), nameof(Select)));
            var fixture = typeof(FoolWaifuIdentityTests).Assembly;
            FoolWaifuRuntimeCompat.Install(fixture);
            FoolWaifuRuntimeCompat.Install(fixture);

            _selection = "FoolSkinStatic";
            authorForwarder(null);
            Require(FoolWaifu.ResumeSkinCompatibility.RestoreCalls == 0 &&
                ReferenceEquals(ReadIcon(nameof(ReadRunIcon)), NativeTexture) &&
                ReferenceEquals(ReadIcon(nameof(ReadSelectIcon)), NativeTexture),
                "Another skin must prevent the pre-existing start/load delegate from reinstalling callbacks.");

            _selection = "Fool_Waifu::source:fixture";
            authorForwarder(null);
            Require(FoolWaifu.ResumeSkinCompatibility.RestoreCalls == 1 &&
                ReferenceEquals(ReadIcon(nameof(ReadRunIcon)), WaifuTexture) &&
                ReferenceEquals(ReadIcon(nameof(ReadSelectIcon)), WaifuTexture),
                "The selected provider and its source aliases must keep both native callback paths.");

            FoolWaifu.SkinVisuals.MerchantReady(null!);
            FoolWaifu.SkinVisuals.MerchantTrigger(null!, "relaxed_loop");
            Require(FoolWaifu.SkinVisuals.MerchantAttachCalls == 2,
                "Waifu must still be able to attach its own merchant rig when it is selected.");

            // Simulate callbacks retained after a prior chosen run. The provider-private cache
            // stays live; it must not replace the game's already chosen icon for another skin.
            _selection = "Hilde";
            authorForwarder(null);
            Require(FoolWaifu.ResumeSkinCompatibility.RestoreCalls == 1 &&
                ReferenceEquals(ReadIcon(nameof(ReadRunIcon)), NativeTexture) &&
                ReferenceEquals(ReadIcon(nameof(ReadSelectIcon)), NativeTexture),
                "A restored callback must stop returning cached Waifu portraits after a selection change.");
            FoolWaifu.SkinVisuals.MerchantTrigger(null!, "relaxed_loop");
            FoolWaifu.SkinVisuals.MerchantReady(null!);
            Require(FoolWaifu.SkinVisuals.MerchantAttachCalls == 2,
                "Stale merchant callbacks must not insert a rig before Hilde binds the native first child.");
            Require(ReferenceEquals(FoolWaifu.SkinAssets.UiTexture("hand_rock.png"), WaifuTexture),
                "The identity guard must leave unrelated author artwork alone.");
            var settings = AccessTools.Method(typeof(FoolWaifuIdentityTests), nameof(SettingsProbe));
            author.Patch(settings, postfix: new HarmonyMethod(typeof(FoolWaifuIdentityTests), nameof(KeepSettings)));
            harness.Patch(AccessTools.Method(typeof(FoolWaifuRuntimeCompat), nameof(FoolWaifuRuntimeCompat.IsProvider)),
                prefix: new HarmonyMethod(typeof(FoolWaifuIdentityTests), nameof(IsFixtureProvider)));
            var cleanup = AccessTools.Method(typeof(ManagedSkinModLoader), "UnpatchDeactivatedProviderCallbacks");
            var patchType = cleanup.GetParameters()[1].ParameterType.GenericTypeArguments[0];
            cleanup.Invoke(null, [fixture, Array.CreateInstance(patchType, 0)]);
            foreach (var name in new[] { nameof(ReadRunIcon), nameof(ReadSelectIcon) })
                Require(!HasAuthorPostfix(AccessTools.Method(typeof(FoolWaifuIdentityTests), name)),
                    "Deselection must detach the restored icon callback even when it was absent from the activation snapshot.");
            Require(HasAuthorPostfix(settings), "Cleanup must leave independent settings installed.");

            _selection = "Fool_Waifu";
            authorForwarder(null);
            Require(FoolWaifu.ResumeSkinCompatibility.RestoreCalls == 2 &&
                ReferenceEquals(ReadIcon(nameof(ReadRunIcon)), WaifuTexture),
                "Switching back must let the author's existing forwarder restore its icon behavior.");
            Console.WriteLine("Fool Waifu identity passed: start/load forwarder, selected restoration, cached icon isolation, exact cleanup, settings retention and merchant attachment ownership.");
        }
        finally
        {
            author.UnpatchAll(author.Id);
            harness.UnpatchAll(harness.Id);
            catalogSetter.Invoke(null, [previousCatalog]);
            PckArchive.WriteOutput = writer;
            root.Delete(recursive: true);
        }
    }

    private static bool HasAuthorPostfix(MethodBase target) => Harmony.GetPatchInfo(target)?
        .Postfixes.Any(patch => patch.owner == FoolWaifu.ResumeSkinCompatibility.Owner) == true;

    private static bool IsFixtureProvider(Assembly assembly, ref bool __result)
    {
        __result = assembly == typeof(FoolWaifuIdentityTests).Assembly;
        return false;
    }

    private static void SettingsProbe() { }
    private static void KeepSettings() { }
    private static bool Select(ref string __result) { __result = _selection; return false; }
    internal static Texture2D ReadRunIcon() => NativeTexture;
    internal static Texture2D ReadSelectIcon() => NativeTexture;

    private static Texture2D? ReadIcon(string method) =>
        (Texture2D?)AccessTools.Method(typeof(FoolWaifuIdentityTests), method).Invoke(null, null);

    private static Texture2D MakeTexture()
    {
        var texture = (ImageTexture)RuntimeHelpers.GetUninitializedObject(typeof(ImageTexture));
        GC.SuppressFinalize(texture);
        return texture;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

namespace FoolWaifu
{
    internal static class ResumeSkinCompatibility
    {
        internal const string Owner = "sc.tests.fool-waifu-author";
        internal static int RestoreCalls { get; private set; }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void BeforeRun(RunState? state)
        {
            RestoreCalls++;
            var patcher = new Harmony(Owner);
            foreach (var icon in new[] { nameof(FoolWaifuIdentityTests.ReadRunIcon), nameof(FoolWaifuIdentityTests.ReadSelectIcon) })
                patcher.Patch(AccessTools.Method(typeof(FoolWaifuIdentityTests), icon),
                    postfix: new HarmonyMethod(typeof(PortraitPatch), "Postfix"));
        }

    }

    internal static class PortraitPatch
    {
        private static void Postfix(MethodBase __originalMethod, ref Texture2D __result)
        {
            var art = __originalMethod.Name == nameof(FoolWaifuIdentityTests.ReadRunIcon) ? "portrait.png" : "select_icon.png";
            if (SkinAssets.UiTexture(art) is { } privateTexture) __result = privateTexture;
        }
    }

    internal static class SkinVisuals
    {
        internal static int MerchantAttachCalls { get; private set; }

        internal static void MerchantReady(NMerchantCharacter merchant) => AttachMerchant(merchant);
        internal static void MerchantTrigger(NMerchantCharacter merchant, string animation) => AttachMerchant(merchant);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void AttachMerchant(NMerchantCharacter merchant) => MerchantAttachCalls++;
    }

    internal static class SkinAssets
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static Texture2D? UiTexture(string filename) => FoolWaifuIdentityTests.WaifuTexture;
    }
}

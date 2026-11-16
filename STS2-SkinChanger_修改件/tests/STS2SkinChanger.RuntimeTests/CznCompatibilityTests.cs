using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

internal static class CznCompatibilityTests
{
    internal static void Run(string gamePack, string workshop)
    {
        var specs = new[] { ("3840027648", "fool"), ("3826012249", "regent"),
            ("3826012071", "silent"), ("3826011833", "ironclad"), ("3789216478", "watcher") };
        var descriptors = new List<SkinModDescriptor>();
        foreach (var (id, target) in specs)
        {
            var root = Path.Combine(workshop, id);
            var manifest = Directory.GetFiles(root, "*.json").Single();
            using var json = JsonDocument.Parse(File.ReadAllText(manifest));
            var name = json.RootElement.GetProperty("id").GetString()!;
            var replacements = ManagedCharacterAssetReplacementScanner.Scan(root, name);
            Require(replacements.Any(r => r.TargetGroupId == target && r.CanonicalPathsByProviderPath.Values.Any(p =>
                    p == $"res://animations/characters/{target}/{target}_skel_data.tres")),
                $"Missing declared combat body: {name}/{target}");
            descriptors.Add(new(name, name, Path.Combine(root, name + ".pck"), false, root, true));
            Console.WriteLine($"CZN declaration: {name} -> {target}; paths={replacements.Sum(r => r.CanonicalPathsByProviderPath.Count)}");
        }
        var watcherRoot = Path.Combine(workshop, "3747526116");
        descriptors.Add(new("Watcher", "Watcher", Path.Combine(watcherRoot, "Watcher.pck"), true, watcherRoot, true));
        var tiphera = Path.Combine(workshop, "3799519206");
        Require(ProviderGameplayContentPolicy.DefinesNewContent(tiphera, "CznTipheraCharacter"), "Tiphera must retain gameplay ownership.");
        descriptors.Add(new("CznTipheraCharacter", "Tiphera", Path.Combine(tiphera, "CznTipheraCharacter.pck"), true, tiphera, true));
        CheckStyleUi(gamePack, workshop);
        var probes = SkinCatalog.ProbeSkinProviders(descriptors, gamePack);
        foreach (var descriptor in descriptors.Where(d => !d.AffectsGameplay))
            Require(probes.Any(p => p.Id == descriptor.Id && p.VisualGroupCount > 0), "Provider not recognized: " + descriptor.Id);
        Require(probes.All(p => p.Id != "CznTipheraCharacter"), "Gameplay character must not be isolated as a skin.");
        using (var built = SkinCatalog.Build(gamePack, descriptors))
        {
            foreach (var (id, target) in specs)
            {
                var name = descriptors.Single(d => d.RootPath == Path.Combine(workshop, id)).Id;
                Require(built.Groups.Any(g => g.Id == target && g.Options.Any(o => o.EffectiveProviderId == name)),
                    "Declared skin did not reach its character dropdown: " + name);
            }
        }
        CheckSelectionGate();
        CheckCorePatchContract(Path.Combine(workshop, "3826011578", "CznCore.dll"));
        Require(!SkinPreloader.IsImmutableResourcePath("res://animations/characters/defect/defect_skel_data.tres"), "Mutable canonical path must not be loaded on a background worker.");
        Require(SkinPreloader.IsImmutableResourcePath("res://sts2_skin_runtime/session/001/body.tres"), "Private immutable paths remain eligible.");
        Require(!SkinPreloader.IsImmutableResourcePath("res://sts2_skin_runtime/../body.tres"), "Private path escape rejected.");
        Console.WriteLine("CZN compatibility passed: five real skin declarations/probes, gameplay exclusion, shared-event ownership and immutable preloading.");
    }

    internal static void CheckStyleUi(string gamePack, string workshop)
    {
        var uiRoot = Path.Combine(workshop, "3771060891");
        var ui = new SkinModDescriptor("CznStyleUI::copy", "CZN Style UI", Path.Combine(uiRoot, "CznStyleUI.pck"),
            false, uiRoot, true, "CznStyleUI");
        Require(!SkinCatalog.ProbeSkinProviders([ui], gamePack).Any(),
            "The real UI overhaul must retain its native initializer and complete PCK, including duplicate installations.");
        using (var uiCatalog = SkinCatalog.Build(gamePack, [ui]))
            Require(uiCatalog.PckCardOptions.Count == 0 && uiCatalog.CardGroups.Count == 0 &&
                !uiCatalog.Groups.Any(g => g.Options.Any(o => o.EffectiveProviderId == ui.Id)),
                "UI overhaul must not appear in appearance dropdowns.");
        Console.WriteLine("CznStyleUI native loader ownership passed: real PCK, duplicate manifest identity, no card or appearance options.");
    }

    private static void CheckCorePatchContract(string dll)
    {
        // Game logging initializes Godot.OS; this test process has no native engine.
        var logging = new Harmony("sc.tests.czn.logging");
        foreach (var name in new[] { "Info", "Warn" })
            logging.Patch(AccessTools.Method(typeof(ModLog), name), prefix: new HarmonyMethod(typeof(CznCompatibilityTests), nameof(SkipLog)));
        try
        {
            var assembly = Assembly.LoadFrom(dll);
            CznPresentationBridge.EnsureInstalled();
            var catalog = assembly.GetType("CznCore.Presentation.CharacterAppearanceCatalog", true)!;
            foreach (var method in catalog.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                         .Where(m => m.Name is "IsSelected" or "IsActive"))
                Require(Harmony.GetPatchInfo(method)?.Prefixes.Any(p => p.owner.EndsWith(".czn-presentation", StringComparison.Ordinal)) == true,
                    "CznCore profile ownership hook did not install: " + method);
            var bootstrap = assembly.GetType("CznCore.Presentation.CharacterSkinModBootstrap", true)!;
            Require(Harmony.GetPatchInfo(AccessTools.Method(bootstrap, "Init"))?.Prefixes.Count > 0, "CznCore registration hook missing.");
            foreach (var type in new[] { typeof(MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantCharacter),
                         typeof(MegaCrit.Sts2.Core.Nodes.RestSite.NRestSiteCharacter) })
                Require(Harmony.GetPatchInfo(AccessTools.Method(type, "_Ready"))?.Prefixes.Any(p =>
                    p.PatchMethod.Name == "RestoreNonCombatIdentity") == true,
                    "Noncombat scene identity must be restored before author callbacks: " + type.Name);
            CheckSelectionGate();
            CheckMerchantOwnership(assembly);
            Console.WriteLine("Real CznCore Harmony contract installed without running provider initializers.");
        }
        finally { logging.UnpatchAll(logging.Id); }
    }

    private static bool SkipLog() => false;

    private static void CheckMerchantOwnership(Assembly core)
    {
        var player = (MegaCrit.Sts2.Core.Entities.Players.Player)RuntimeHelpers.GetUninitializedObject(
            typeof(MegaCrit.Sts2.Core.Entities.Players.Player));
        var other = (MegaCrit.Sts2.Core.Entities.Players.Player)RuntimeHelpers.GetUninitializedObject(player.GetType());
        var visual = (MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantCharacter)RuntimeHelpers.GetUninitializedObject(
            typeof(MegaCrit.Sts2.Core.Nodes.Screens.Shops.NMerchantCharacter));
        GC.SuppressFinalize(visual);
        var merchant = core.GetType("CznCore.Presentation.MerchantPresentationRuntime", true)!;
        var resolve = AccessTools.Method(merchant, "ResolveMerchantCharacterPlayer");
        var instance = RuntimeHelpers.GetUninitializedObject(merchant);
        var apply = AccessTools.Method(merchant, "Apply", [visual.GetType()]);
        Require(Harmony.GetPatchInfo(apply)?.Prefixes.Any(p => p.PatchMethod.Name == "AllowMerchantPresentation") == true,
            "Shared presentation must check ownership before touching Spine.");
        using (CznPresentationBridge.BeginNonCombatOwnerScope(player, visual))
        {
            for (var i = 0; i < 200; i++)
            {
                using (CznPresentationBridge.BeginNonCombatOwnerScope(other, visual))
                    Require(ReferenceEquals(resolve.Invoke(instance, [visual]), other), "Nested owner lost.");
                Require(ReferenceEquals(resolve.Invoke(instance, [visual]), player), "Owner not restored after replacement.");
            }
            var predicate = AccessTools.Field(merchant, "_appliesTo");
            var gate = AccessTools.Method(typeof(CznPresentationBridge), "AllowMerchantPresentation");
            foreach (var expected in new[] { false, true, false })
            {
                predicate.SetValue(instance, new Func<MegaCrit.Sts2.Core.Entities.Players.Player?, bool>(p =>
                    ReferenceEquals(p, player) && expected));
                Require((bool)gate.Invoke(null, [instance, visual])! == expected, "Wrong skin reached merchant body.");
            }
            var profileType = core.GetType("CznCore.Presentation.CharacterSkinProfile", true)!;
            var profile = RuntimeHelpers.GetUninitializedObject(profileType);
            AccessTools.Field(profileType, "_targetCharacterTypeResolved").SetValue(profile, true);
            AccessTools.Field(profileType, "_resolvedTargetCharacterType").SetValue(profile, typeof(string));
            var eventType = core.GetType("CznCore.Presentation.MerchantCharacterReadyEvent", true)!;
            var evt = Activator.CreateInstance(eventType, [visual]);
            var accepts = core.GetType("CznCore.Presentation.SkinEventOwnership", true)!
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(m => m.Name == "Accepts" && m.IsGenericMethodDefinition).MakeGenericMethod(eventType);
            Require(!(bool)accepts.Invoke(null, [profile, evt])!, "Foreign owned callback could bind its skeleton before the shared gate.");
            var rest = (MegaCrit.Sts2.Core.Nodes.RestSite.NRestSiteCharacter)RuntimeHelpers.GetUninitializedObject(
                typeof(MegaCrit.Sts2.Core.Nodes.RestSite.NRestSiteCharacter));
            GC.SuppressFinalize(rest);
            foreach (var (name, arguments) in new (string, object?[])[] {
                ("RestSiteCharacterReadyEvent", [rest]),
                ("PlayerTurnStartedEvent", [null, null, player]) })
            {
                var type = core.GetType("CznCore.Presentation." + name, true)!;
                var value = Activator.CreateInstance(type, arguments);
                var boundary = accepts.GetGenericMethodDefinition().MakeGenericMethod(type);
                Require(!(bool)boundary.Invoke(null, [profile, value])!, "Foreign profile passed native owner gate: " + name);
            }
        }
        Require(AccessTools.Field(typeof(CznPresentationBridge), "_nonCombatOwner").GetValue(null) == null,
            "Owner scope leaked after refresh.");
        Console.WriteLine("Character ownership: real CznCore merchant/rest/combat event gates, shared predicate and 200 nested replacements passed.");
    }

    private sealed class Profile
    {
        public string ProfileId => "czn-test";
        public Type ResolveTargetCharacterType() => typeof(MegaCrit.Sts2.Core.Models.Characters.Ironclad);
    }

    private static void CheckSelectionGate()
    {
        var previous = SkinService.Catalog;
        var config = SkinService.Config;
        var catalog = (SkinCatalog)RuntimeHelpers.GetUninitializedObject(typeof(SkinCatalog));
        var group = new SkinGroup("ironclad", "Ironclad");
        group.Options.Add(new("czn-source", "CZN", new Dictionary<string, ResourceAsset>(), ProviderId: "czn-provider"));
        AccessTools.Field(typeof(SkinCatalog), "_groups").SetValue(catalog, new List<SkinGroup> { group });
        var setter = AccessTools.PropertySetter(typeof(SkinService), "Catalog");
        var profile = new Profile();
        try
        {
            setter.Invoke(null, [catalog]);
            AccessTools.PropertySetter(typeof(SkinService), "Config").Invoke(null, [new SkinConfig()]);
            CznPresentationBridge.RegisterAssembly("czn-provider", typeof(CznCompatibilityTests).Assembly);
            AccessTools.Method(typeof(CznPresentationBridge), "CaptureProfile").Invoke(null, [typeof(CznCompatibilityTests), profile]);
            foreach (var (selection, expected) in new[] { ("__base__", false), ("czn-source", true), ("other-skin", false) })
            {
                SkinService.Config.Selections["ironclad"] = selection;
                object[] arguments = [profile, new object[] { profile }, true];
                var runNative = (bool)AccessTools.Method(typeof(CznPresentationBridge), "ProfileSelected").Invoke(null, arguments)!;
                Require(!runNative && (bool)arguments[2] == expected, "Stale CZN event gate for selection " + selection);
                var core = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "CznCore");
                if (core != null)
                {
                    var profileType = core.GetType("CznCore.Presentation.CharacterSkinProfile", true)!;
                    var native = RuntimeHelpers.GetUninitializedObject(profileType);
                    AccessTools.Field(profileType, "<ProfileId>k__BackingField").SetValue(native, profile.ProfileId);
                    var nativeCatalog = core.GetType("CznCore.Presentation.CharacterAppearanceCatalog", true)!;
                    Require((bool)AccessTools.Method(nativeCatalog, "IsActive").Invoke(null, [native])! == expected,
                        "Real shared CznCore gate did not follow selection " + selection);
                    var artType = core.GetType("CznCore.Presentation.BundledSkinCardArt", true)!;
                    var art = RuntimeHelpers.GetUninitializedObject(artType);
                    AccessTools.Field(artType, "_profile").SetValue(art, native);
                    AccessTools.Field(artType, "_enabled").SetValue(art, true);
                    Require((bool)AccessTools.PropertyGetter(artType, "Enabled").Invoke(art, null)! == false,
                        "Shared card art must not force the character bundle over SC's independent card selection.");
                }
            }
        }
        finally
        {
            setter.Invoke(null, [previous]);
            AccessTools.PropertySetter(typeof(SkinService), "Config").Invoke(null, [config]);
            // Uninitialized catalog owns no PCK resources; suppress its finalizer.
            GC.SuppressFinalize(catalog);
        }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

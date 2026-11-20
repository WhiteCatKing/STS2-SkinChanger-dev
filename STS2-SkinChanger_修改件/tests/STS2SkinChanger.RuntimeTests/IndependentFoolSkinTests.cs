using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

internal static class IndependentFoolSkinTests
{
    internal static void Run(string game, string workshop)
    {
        var silence = AuditLogSilencer.Install();
        var specs = new[] { ("3838977271", "Fool_Waifu"), ("3838744026", "NailongFool"), ("3838976517", "FoolSkinStatic") }.ToList();
        // NonoFoolSkin (workshop 3841262975) is optional on machines that have not subscribed to it.
        if (File.Exists(Path.Combine(workshop, "3841262975", "NonoFoolSkin.dll"))) specs.Add(("3841262975", "NonoFoolSkin"));
        var mods = specs.Select(s => new SkinModDescriptor(s.Item2, s.Item2,
            s.Item2 is "Fool_Waifu" or "NonoFoolSkin" ? Path.Combine(workshop, s.Item1, s.Item2 + ".pck") : null,
            false, Path.Combine(workshop, s.Item1), true)).ToArray();
        foreach (var mod in mods)
        {
            var normalized = IndependentFoolSkinContract.Normalize(mod);
            Require(IndependentFoolSkinContract.Matches(normalized), "Actual standalone contract missing: " + mod.Id);
            Require(!IndependentFoolSkinContract.Matches(normalized with { AffectsGameplay = true }) &&
                !IndependentFoolSkinContract.Matches(normalized with { HasDll = false }) &&
                !IndependentFoolSkinContract.Matches(normalized with { Id = "Unknown", ManifestId = "Unknown" }),
                "Unrelated, gameplay and missing-code negatives must remain excluded.");
        }
        using (var baseline = STS2SkinChanger.Pck.PckArchive.Open(game))
        {
            if (!baseline.Contains("res://scenes/creature_visuals/fool.tscn") &&
                !baseline.Contains("res://scenes/creature_visuals/fool.tscn.remap"))
            {
                Require(!SkinCatalog.ProbeSkinProviders(mods, game).Any(), "Older game without Fool must not advertise beta-only providers.");
                using var older = SkinCatalog.Build(game, mods);
                Require(!older.Groups.Any(g => g.Options.Any(o => mods.Any(m => m.Id == o.Id))),
                    "Older branch must not add unsupported Fool selections.");
                Console.WriteLine("Older game capability passed: all three beta-only Fool skins excluded because the game has no Fool.");
                return;
            }
        }
        var probes = SkinCatalog.ProbeSkinProviders(mods, game);
        Require(mods.All(m => probes.Any(p => p.Id == m.Id && p.VisualGroupCount > 0 && p.HasResourceBackedCosmetics)),
            "All three real packages must be detected before native initialization.");
        using var catalog = SkinCatalog.Build(game, mods);
        var fool = catalog.Groups.Single(g => g.Id == "fool");
        foreach (var mod in mods)
        {
            var option = fool.Options.Single(o => o.Id == mod.Id);
            Require(option.IsRuntimeProvider && option.IsDirectCharacterRuntimeProvider && !option.IsCharacterIconOnly,
                "Real option must own full appearance: " + mod.Id);
            var active = catalog.GetSelectedDirectCharacterRuntimeProviders(new Dictionary<string, string> { ["fool"] = option.Id });
            Require(active.Contains(mod.Id) && mods.Where(m => m.Id != mod.Id).All(m => !active.Contains(m.Id)),
                "Selecting one runtime must not activate the other two: " + mod.Id);
        }
        Require(!catalog.GetSelectedDirectCharacterRuntimeProviders(
            new Dictionary<string, string> { ["fool"] = SkinCatalog.BaseOptionId }).Any(), "Default must activate none.");
        var body = catalog.BuildRuntimeResourceOverlay("fool", "NailongFool",
            ["res://scenes/creature_visuals/fool.tscn"], "naiwa-body", true);
        using (var pack = STS2SkinChanger.Pck.PckArchive.Open(Path.Combine(workshop, "3838744026", "NailongFool.assets")))
        {
            var texture = pack.Paths.Single(p => p.Contains("/_imported/fool.png-"));
            var bytes = pack.ReadFile(texture);
            Require(body.Files.Values.Any(b => b.AsSpan().SequenceEqual(bytes)), "Real Naiwa body texture must reach the isolated scene graph.");
            Require(body.SourceAliases.ContainsKey(texture) &&
                !body.SourceAliases.Keys.Any(p => p.StartsWith("res://.godot/imported/fool.png-", StringComparison.Ordinal)),
                "Inherited atlas must bind to the selected texture payload, not an additional vanilla page.");
            var menu = catalog.BuildRuntimeResourceOverlay("fool", "NailongFool",
                ["res://scenes/screens/char_select/char_select_bg_fool.tscn"], "naiwa-menu", true);
            var menuTexture = pack.Paths.Single(p => p.Contains("/_imported/character_select_fool_bg.png-"));
            var menuBytes = pack.ReadFile(menuTexture);
            Require(menu.Files.Values.Any(b => b.AsSpan().SequenceEqual(menuBytes)),
                "Real Naiwa character-select atlas must retain the selected texture too.");
            var vanilla = catalog.BuildRuntimeResourceOverlay("fool", SkinCatalog.BaseOptionId,
                ["res://scenes/creature_visuals/fool.tscn"], "naiwa-default", true);
            Require(!vanilla.Files.Values.Any(b => b.AsSpan().SequenceEqual(bytes)), "Default must restore vanilla texture ownership.");
        }
        var previewCalls = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(FrameworkModelPreview), "TryRefresh"))
            .Select(i => i.operand).OfType<MethodBase>().ToList();
        var replacement = previewCalls.FindIndex(m => m.DeclaringType == typeof(IndependentFoolSkinCompat) && m.Name == "ApplyPreview");
        Require(replacement > previewCalls.FindIndex(m => m.Name == "ApplyRuntimeSpine") &&
            replacement > previewCalls.FindIndex(m => m.Name == "ReplaySelectedCreatureNodeReady") &&
            replacement < previewCalls.FindIndex(m => m.Name == "BeginCapture"),
            "Static replacement/hiding must occur after Ready finishing, before capture; Body is unavailable before Ready.");
        var assembly = Assembly.LoadFrom(Path.Combine(workshop, "3838744026", "NailongFool.dll"));
        IndependentFoolSkinCompat.Prepare(assembly);
        var mount = AccessTools.Method(assembly.GetType("NailongFool.NailongFoolMod"), "MountAssets");
        Require(Harmony.GetPatchInfo(mount)?.Prefixes.Any(p => p.PatchMethod.Name == "SkipAuthorMount") == true,
            "Author initializer must not promote its .assets globally.");
        var sprite = Assembly.LoadFrom(Path.Combine(workshop, "3838976517", "FoolSkinStatic.dll")).GetType("FoolSkinStatic.Patches", true)!;
        foreach (var method in new[] { "EnsureCharacter", "MakeSprite", "Play", "BlinkLoop" })
            Require(sprite.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Any(m => m.Name == method), "Real preview method missing: " + method);
        var nonoDir = Path.Combine(workshop, "3841262975");
        if (File.Exists(Path.Combine(nonoDir, "NonoFoolSkin.dll")))
            NonoFoolPreviewTests.VerifyRealPackageContract(nonoDir);
        Console.WriteLine($"{mods.Length} actual Fool skin packages passed: PCK/assets/embedded images, full dropdown ownership, mutually exclusive selection, default off and native mount suppression.");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
}

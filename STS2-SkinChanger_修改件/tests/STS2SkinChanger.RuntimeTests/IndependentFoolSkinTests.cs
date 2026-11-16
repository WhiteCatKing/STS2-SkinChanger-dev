using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

internal static class IndependentFoolSkinTests
{
    internal static void Run(string game, string workshop)
    {
        var silence = AuditLogSilencer.Install();
        var specs = new[] { ("3838977271", "Fool_Waifu"), ("3838744026", "NailongFool"), ("3838976517", "FoolSkinStatic") };
        var mods = specs.Select(s => new SkinModDescriptor(s.Item2, s.Item2,
            s.Item2 == "Fool_Waifu" ? Path.Combine(workshop, s.Item1, s.Item2 + ".pck") : null,
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
        var assembly = Assembly.LoadFrom(Path.Combine(workshop, "3838744026", "NailongFool.dll"));
        IndependentFoolSkinCompat.Prepare(assembly);
        var mount = AccessTools.Method(assembly.GetType("NailongFool.NailongFoolMod"), "MountAssets");
        Require(Harmony.GetPatchInfo(mount)?.Prefixes.Any(p => p.PatchMethod.Name == "SkipAuthorMount") == true,
            "Author initializer must not promote its .assets globally.");
        var sprite = Assembly.LoadFrom(Path.Combine(workshop, "3838976517", "FoolSkinStatic.dll")).GetType("FoolSkinStatic.Patches", true)!;
        foreach (var method in new[] { "EnsureCharacter", "MakeSprite", "Play", "BlinkLoop" })
            Require(sprite.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public).Any(m => m.Name == method), "Real preview method missing: " + method);
        Console.WriteLine("Three actual Fool skin packages passed: PCK/assets/embedded images, full dropdown ownership, mutually exclusive selection, default off and native mount suppression.");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
}

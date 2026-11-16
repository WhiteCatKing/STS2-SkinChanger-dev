using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class JoanStagePaletteTests
{
    internal static void Run(string dll)
    {
        var assembly = Assembly.LoadFrom(dll); // Metadata and pure lookup methods only; no mod initializer.
        var palette = assembly.GetType("JoanofArcIroncladHpwear.Palette", true)!;
        var pageType = palette.GetNestedType("Page", BindingFlags.NonPublic)!;
        var pages = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(typeof(string), pageType))!;
        var material = (ShaderMaterial)RuntimeHelpers.GetUninitializedObject(typeof(ShaderMaterial));
        GC.SuppressFinalize(material);
        var created = 0;
        ShaderMaterial Create() { created++; return material; }
        foreach (var key in new[] { "s1_ironclad", "s2_ironclad", "s1_restsite", "s2_restsite",
                     "s1_charselect", "hand_paper", "hand_point", "hand_rock", "hand_scissors" })
            StagePalettePages.RegisterMissing(pages, pageType, key, Create);
        Require(pages.Count == 9, "Reproduce startup with only the first select stage exposed.");
        var weights = new HashSet<Vector4>();
        for (var variant = 0; variant < 4; variant++)
        {
            weights.Add((Vector4)AccessTools.Method(palette, "PresetWeights").Invoke(null, [variant])!);
            foreach (var stage in new[] { 1, 2, 3, 1, 2, 3 })
            {
                var key = $"s{stage}_charselect";
                Require((string)AccessTools.Method(palette, "MapPage").Invoke(null, [key])! == key, "Native stage lookup lost its page.");
                StagePalettePages.RegisterMissing(pages, pageType, key, Create);
                Require(ReferenceEquals(AccessTools.Field(pageType, "Material").GetValue(pages[key]), material), "Native material registration failed.");
            }
        }
        Require(weights.Count == 4 && pages.Count == 11 && created == 11,
            "All variants need all three pages, with only two additional allocations.");
        var before = pages.Count;
        try { StagePalettePages.RegisterMissing(pages, pageType, "failed", () => throw new InvalidDataException()); }
        catch (InvalidDataException) { }
        Require(pages.Count == before && !pages.Contains("failed"), "Failed material creation published a partial page.");
        Console.WriteLine("Joan palette pages passed: real 9-page startup, four native variant weights, 24 stage switches, two lazy pages and no partial publication.");
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

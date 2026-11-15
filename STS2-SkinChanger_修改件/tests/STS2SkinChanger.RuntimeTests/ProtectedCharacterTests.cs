using System.Text.Json;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

internal static class ProtectedCharacterTests
{
    internal static void Run(string gamePack, string[] roots)
    {
        var silencer = AuditLogSilencer.Install();
        try
        {
            var descriptors = roots.Select(root =>
            {
                var manifest = Directory.GetFiles(root, "*.json").First(path =>
                {
                    using var json = JsonDocument.Parse(File.ReadAllText(path));
                    return json.RootElement.TryGetProperty("id", out _);
                });
                using var doc = JsonDocument.Parse(File.ReadAllText(manifest));
                var id = doc.RootElement.GetProperty("id").GetString()!;
                return new SkinModDescriptor(id, id, Path.Combine(root, id + ".pck"), false, root, true);
            }).ToArray();
            using var catalog = SkinCatalog.Build(gamePack, descriptors);
            var group = catalog.Groups.Single(group => group.Id == "ironclad");
            foreach (var provider in descriptors)
            {
                var options = group.Options.Where(option => option.EffectiveProviderId == provider.Id).ToArray();
                var minimum = provider.Id == "CrusaderSkin" ? 5 : 4;
                if (options.Length < minimum) throw new Exception("Protected variants missing: " + provider.Id);
                var scenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var selection in options)
                {
                    if (provider.Id.StartsWith("JoanofArc", StringComparison.OrdinalIgnoreCase))
                    {
                        var wear = catalog.ReadProviderFile("ironclad", selection.Id, "s2_charselect_data.tres") is { Length: > 0 };
                        if (wear != (provider.Id == "JoanofArcIroncladHpwear"))
                            throw new Exception("Joan stage capability crossed package boundary: " + selection.Id);
                        if (wear)
                        {
                            var stageRoots = Enumerable.Range(1, 3).Select(stage =>
                                $"res://joanofarcironcladhpwear/s{stage}_charselect_data.tres").ToArray();
                            var stages = catalog.BuildRuntimeResourceOverlay("ironclad", selection.Id, stageRoots, "protected-stages/001", true);
                            if (!stages.CanBindDependencies || stageRoots.Any(path => !stages.ResourcePaths.ContainsKey(path)) ||
                                RuntimeLoadSafety.EstimateTextureBytes(stages.Files) > RuntimeLoadSafety.MaximumSingleLoadBytes)
                                throw new Exception("Joan stage graph incomplete or outside budget: " + selection.Id +
                                    "; bind=" + stages.CanBindDependencies + "; bytes=" + RuntimeLoadSafety.EstimateTextureBytes(stages.Files) +
                                    "; missing=" + string.Join(",", stageRoots.Where(path => !stages.ResourcePaths.ContainsKey(path))) +
                                    "; unbound=" + string.Join(",", stages.UnboundDependencies));
                        }
                    }
                    const string canonical = "res://scenes/creature_visuals/ironclad.tscn";
                    var scenePath = selection.Assets.TryGetValue(canonical, out var sceneAsset) ? sceneAsset.SourcePath : canonical;
                    scenes.Add(scenePath);
                    var overlay = catalog.BuildRuntimeResourceOverlay("ironclad", selection.Id,
                        [scenePath], "protected-test/001", true);
                    var estimated = RuntimeLoadSafety.EstimateTextureBytes(overlay.Files);
                    if (estimated > RuntimeLoadSafety.MaximumSingleLoadBytes) throw new Exception("Protected model exceeds new budget: " + provider.Id);
                    Console.WriteLine($"Protected real package: {provider.Id}; variant={selection.Id}; estimated={estimated / 1048576d:F1} MiB; bound graph={overlay.CanBindDependencies}; order={overlay.LoadOrder.Count}");
                    if (!overlay.CanBindDependencies || overlay.LoadOrder.Count != overlay.ResourcePaths.Count)
                        throw new Exception("Incomplete isolated dependency graph: " + provider.Id + "; " + string.Join("; ", overlay.UnboundDependencies.Take(6)));
                }
                if (provider.Id == "CrusaderSkin" && scenes.Count != 5)
                    throw new Exception("Crusader variants collapsed to the same combat scene.");
            }
            Console.WriteLine("Protected character package/variant and texture budget contracts passed.");
        }
        finally { silencer.UnpatchAll(silencer.Id); }
    }
}

using System.Text.Json;
using Godot;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class DecimillipedeSkinIsolationTests
{
    private const string Group = "decimillipede";

    internal static void Run(string gamePack, string dynamicPack, string staticRoot)
    {
        AuditLogSilencer.Install();
        dynamicPack = Path.GetFullPath(dynamicPack);
        using var manifest = JsonDocument.Parse(File.ReadAllText(Path.ChangeExtension(dynamicPack, ".json")));
        var dynamicId = manifest.RootElement.GetProperty("id").GetString()!;
        var dynamicName = manifest.RootElement.GetProperty("name").GetString()!;
        var motion = new SkinModDescriptor(dynamicId, dynamicName, dynamicPack, false,
            Path.GetDirectoryName(dynamicPack), HasDll: manifest.RootElement.GetProperty("has_dll").GetBoolean());
        var staticSkins = Directory.EnumerateFiles(staticRoot, "*.pck", SearchOption.AllDirectories)
            .Where(path => !Path.GetFullPath(path).Equals(dynamicPack, StringComparison.OrdinalIgnoreCase))
            .Select(path => (Id: Path.GetFileNameWithoutExtension(path), Path: path))
            .Where(skin => skin.Id.StartsWith("Kiteseven ", StringComparison.OrdinalIgnoreCase))
            .Select(skin => new SkinModDescriptor(
                skin.Id, skin.Id, skin.Path, false, Path.GetDirectoryName(skin.Path)))
            .ToArray();
        Require(staticSkins.Length == 3, "Expected three original Kiteseven variants.");
        using var catalog = SkinCatalog.Build(gamePack, [motion, .. staticSkins]);
        Require(catalog.ProviderUsesInstanceAnimation(dynamicId) &&
                !catalog.ProviderRequiresCoherentRuntimePackage(dynamicId),
            $"A skin that only animates its own sprites must stay isolated after repackaging ({dynamicId}).");
        Require(staticSkins.All(skin => !catalog.ProviderUsesInstanceAnimation(skin.Id)),
            "The three static variants must keep their independent resource-only routes.");
        var segments = new[] { "front", "middle", "back" };
        var selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var shared = segments.Select(segment => Group + "_segment_" + segment).Append(Group).ToArray();
        var center = new Vector2(300, 150);
        var front = new Vector2(200, 130);
        Require(DecimillipedeBestiarySetupCleanupPatch.ScalePosition(front, center, 0.5f) == new Vector2(250, 140) &&
                DecimillipedeBestiarySetupCleanupPatch.ScalePosition(front, center, 2f) == new Vector2(100, 110) &&
                DecimillipedeBestiarySetupCleanupPatch.ScalePosition(front, center, 1f) == front,
            "Changing the static formation size must keep the same center and restore the original slots.");
        Require(ManagedSceneCompatibility.RequiresSpineParent("SpineSlotNode", false) &&
                ManagedSceneCompatibility.RequiresSpineParent("Node", true) &&
                !ManagedSceneCompatibility.RequiresSpineParent("Marker2D", false) &&
                !ManagedSceneCompatibility.RequiresSpineParent("Control", false),
            "Scene repair must distinguish native Spine effects from required bounds and marker nodes.");

        foreach (var provider in staticSkins)
        {
            foreach (var choice in new[] { dynamicId, provider.Id, SkinCatalog.BaseOptionId, dynamicId })
            {
                var transaction = catalog.BuildVisualSelectionTransaction(Group, choice, selections);
                Require(transaction.Count == shared.Length &&
                        shared.All(group => transaction.GetValueOrDefault(group) == choice),
                    $"One click on {choice} must select all three segments.");
                foreach (var update in transaction)
                    selections[update.Key] = update.Value;

                var active = catalog.GetSelectedInteractiveRuntimeProviders(selections);
                Require(active.Contains(dynamicId) == (choice == dynamicId),
                    $"The motion callbacks must follow the dynamic choice only ({choice}).");
                var canonical = catalog.BuildOverlay(selections, shared.ToHashSet(StringComparer.OrdinalIgnoreCase));
                Require(canonical.Values.All(file => !Path.GetFullPath(file.ArchivePath)
                        .Equals(Path.GetFullPath(dynamicPack), StringComparison.OrdinalIgnoreCase)),
                    "The dynamic pack must not write into the game's global resource layer.");

                foreach (var segment in segments)
                {
                    var group = Group + "_segment_" + segment;
                    var path = $"res://scenes/creature_visuals/{group}.tscn";
                    var overlay = catalog.BuildRuntimeResourceOverlay(
                        group, choice, [path], "decimillipede-test/001", includeProviderDependencies: true);
                    Require(overlay.ResourcePaths.ContainsKey(path), $"No isolated scene for {group}/{choice}.");
                    var texture = $"/dmotion/private/decimillipede_segment_{segment}_motion.png";
                    Require(overlay.Files.Keys.Any(key => key.EndsWith(texture + ".import",
                                StringComparison.OrdinalIgnoreCase)) == (choice == dynamicId),
                        $"Incorrect private texture ownership for {group}/{choice}.");
                    if (choice == dynamicId)
                    {
                        Require(overlay.Files.Keys.All(key => key.StartsWith(
                                "res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase)),
                            "The dynamic resource graph must publish only private aliases.");
                        var scene = overlay.Files.Single(file => file.Key.EndsWith(
                            $"-{group}.scn", StringComparison.OrdinalIgnoreCase));
                        var privateTexture = overlay.ResourcePaths["res:/" + texture];
                        Require(CardBinaryResource.Parse(scene.Value)?.Dependencies(path).Contains(privateTexture) == true,
                            "The exported scene must reference the selected texture's private alias.");
                    }
                    if (choice == provider.Id)
                        Require(canonical.Values.Any(file => file.ArchivePath == provider.PckPath),
                            "Static choice fell back to the game instead of using its own archive.");
                }
            }
        }

        Console.WriteLine($"Decimillipede isolation passed: {dynamicId}, all three static PCKs and game default.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

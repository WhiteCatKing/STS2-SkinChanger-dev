using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    // Some merchant packs create their Spine overlay from a room callback instead of replacing
    // canonical files. Require all three pieces of evidence: an actual room patch, the vanilla
    // merchant skeleton target, and a private merchant skeleton in this PCK. Room hooks alone
    // must never claim UI/content/additive-effect packages as selectable skins.
    private static void AddPrivateMerchantRuntimeOptions(
        IEnumerable<PckResourceIndex> indexes, IDictionary<string, SkinGroup> groups)
    {
        const string target = "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres";
        foreach (var index in indexes)
        {
            if (!index.Mod.HasDll || string.IsNullOrWhiteSpace(index.Mod.RootPath)) continue;
            if (groups.TryGetValue("merchant", out var existing) &&
                existing.Options.Any(option => option.EffectiveProviderId == index.Mod.Id)) continue;
            try
            {
                // Filter the PCK first; inspecting every cosmetic DLL during catalogue rebuilds
                // would add startup cost even for packages that contain no private merchant.
                var roots = index.Archive.Paths.Where(path =>
                        path.Contains("/merchant/", StringComparison.OrdinalIgnoreCase) &&
                        !path.StartsWith("res://animations/", StringComparison.OrdinalIgnoreCase) &&
                        path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) &&
                        index.Archive.GetFileSize(path) <= 256 * 1024 &&
                        Encoding.UTF8.GetString(index.Archive.ReadFile(path))
                            .Contains("type=\"SpineSkeletonDataResource\"", StringComparison.Ordinal))
                    .Select(path => path[..path.IndexOf("/merchant/", StringComparison.OrdinalIgnoreCase)] + "/")
                    .Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                if (roots.Length == 0) continue;
                var assemblyPath = SkinPackagePaths.Resolve(index.Mod.RootPath, index.Mod.ResourceNamespaceId, ".dll");
                var bytes = File.ReadAllBytes(assemblyPath);
                if (bytes.AsSpan().IndexOf(Encoding.Unicode.GetBytes(target)) < 0) continue;
                using var stream = new MemoryStream(bytes, writable: false);
                using var pe = new PEReader(stream);
                if (!pe.HasMetadata) continue;
                var reader = pe.GetMetadataReader();
                var roomPatch = reader.TypeDefinitions.Any(handle =>
                {
                    var type = reader.GetTypeDefinition(handle);
                    var metadata = new StringBuilder();
                    AppendHarmonyPatchMetadata(reader, type.GetCustomAttributes(), metadata);
                    foreach (var method in type.GetMethods())
                        AppendHarmonyPatchMetadata(reader, reader.GetMethodDefinition(method).GetCustomAttributes(), metadata);
                    var text = metadata.ToString();
                    return text.Contains("NMerchantRoom", StringComparison.Ordinal) &&
                           text.Contains("_Ready", StringComparison.Ordinal);
                });
                if (!roomPatch) continue;
                // The DLL must actually address the same namespace; do not infer ownership merely
                // from an unrelated file named merchant. Keep private paths intact: the author's
                // callback creates its own overlay and interaction viewport, not a canonical Spine.
                roots = roots.Where(root => bytes.AsSpan().IndexOf(Encoding.Unicode.GetBytes(root)) >= 0).ToArray();
                if (roots.Length == 0) continue;
                var assets = index.Archive.Paths.Where(path => roots.Any(root => path.StartsWith(root, StringComparison.OrdinalIgnoreCase)))
                    .Select(index.TryBuildAsset).OfType<ResourceAsset>()
                    .DistinctBy(asset => asset.SourcePath, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(asset => asset.SourcePath, StringComparer.OrdinalIgnoreCase);
                if (!groups.TryGetValue("merchant", out var group))
                    groups["merchant"] = group = new SkinGroup("merchant", DisplayName("merchant"));
                group.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, assets, IsRuntimeProvider: true)
                { ProviderRootPath = index.Mod.RootPath, UsesPrivateMerchantRoomCallback = true });
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or BadImageFormatException or ArgumentException or InvalidOperationException)
            {
                System.Diagnostics.Debug.WriteLine($"私有商人外观探测失败 {index.Mod.Id}: {error.Message}");
            }
        }
    }
}

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace STS2SkinChanger.Catalog;

// Explicit contracts for standalone runtime skins; never infer ownership from a workshop title.
internal static class IndependentFoolSkinContract
{
    internal static SkinModDescriptor Normalize(SkinModDescriptor mod)
    {
        if (mod.ResourceNamespaceId == "NailongFool" && mod.HasDll && mod.RootPath != null)
        {
            var assets = Path.Combine(mod.RootPath, "NailongFool.assets");
            if (File.Exists(assets))
            {
                using var stream = File.OpenRead(assets);
                using var reader = new BinaryReader(stream);
                if (stream.Length >= 4 && reader.ReadUInt32() == 0x43504447) mod = mod with { PckPath = assets };
            }
        }
        return mod;
    }

    internal static bool Matches(SkinModDescriptor mod)
    {
        if (!mod.HasDll || mod.AffectsGameplay || mod.RootPath == null) return false;
        var requiredType = mod.ResourceNamespaceId switch
        {
            "Fool_Waifu" => "FoolWaifu.OwnRigSkin",
            "NailongFool" => "NailongFool.NailongFoolMod",
            "FoolSkinStatic" => "FoolSkinStatic.SkinSpec",
            // NonoFoolSkin draws The Fool with its own SpriteFrames and reads its own PCK
            // through a private GDPC reader; the manifest id and initializer type are the
            // only stable contract (contributed by the package author, 2026-11-20).
            "NonoFoolSkin" => "NonoFoolSkin.Entry",
            _ => null
        };
        if (requiredType == null) return false;
        var dll = SkinPackagePaths.Resolve(mod.RootPath, mod.ResourceNamespaceId, ".dll");
        if (!File.Exists(dll)) return false;
        try
        {
            using var stream = File.OpenRead(dll);
            using var pe = new PEReader(stream);
            var reader = pe.GetMetadataReader();
            if (reader.GetString(reader.GetAssemblyDefinition().Name) != mod.ResourceNamespaceId ||
                !reader.TypeDefinitions.Any(h =>
                {
                    var t = reader.GetTypeDefinition(h);
                    return reader.GetString(t.Namespace) + "." + reader.GetString(t.Name) == requiredType;
                })) return false;
            if (mod.ResourceNamespaceId == "FoolSkinStatic")
                return reader.ManifestResources.Any(h => reader.GetString(reader.GetManifestResource(h).Name)
                    .StartsWith("FoolSkinStatic.Art.", StringComparison.Ordinal));
            return mod.PckPath != null && File.Exists(mod.PckPath);
        }
        catch (Exception e) when (e is IOException or BadImageFormatException) { return false; }
    }
}

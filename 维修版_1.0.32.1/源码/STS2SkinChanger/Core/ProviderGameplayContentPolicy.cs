using System.Reflection;
using System.Reflection.Metadata;
using STS2SkinChanger.Catalog;
using System.Reflection.PortableExecutable;

namespace STS2SkinChanger.Core;

/// <summary>
/// Some Workshop packages are gameplay content, not cosmetics: they add a new card pool, character,
/// monster, relic pool and so on, and only ship their own art for that new content. Their manifest
/// often still says <c>affects_gameplay: false</c> because the author only lists it as an "add-on",
/// so the cosmetic scanner happily sees their card portraits and treats the whole pack as a skin
/// provider — and then never mounts the PCK, which makes the pack look "hidden" (its cards lose
/// their art, or custom scenes never load).
///
/// A pack that defines a subclass of one of the game's model types is adding content, never
/// replacing the appearance of existing content, so it must keep loading through the game's own
/// loader with its PCK mounted globally. The check is metadata-only: no assembly is loaded.
/// </summary>
internal static class ProviderGameplayContentPolicy
{
    private static readonly HashSet<string> ContentModelBaseNames = new(StringComparer.Ordinal)
    {
        "CardPoolModel",
        "CardModel",
        "CharacterModel",
        "MonsterModel",
        "RelicModel",
        "RelicPoolModel",
        "PotionModel",
        "PotionPoolModel",
        "EventModel",
        "ActModel",
        "EncounterModel",
        "OrbModel",
        "EnchantmentModel",
        "AfflictionModel",
        "ModifierModel"
    };

    private static readonly Dictionary<string, bool> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Whether this provider's DLL declares one of the game's content models itself.</summary>
    public static bool DefinesNewContent(string? rootPath, string providerId)
    {
        if (string.IsNullOrWhiteSpace(rootPath) || string.IsNullOrWhiteSpace(providerId))
        {
            return false;
        }

        var key = rootPath + "\n" + providerId;
        lock (Cache)
        {
            if (Cache.TryGetValue(key, out var cached))
            {
                return cached;
            }
        }

        var result = false;
        try
        {
            foreach (var assemblyPath in EnumerateAssemblies(rootPath, providerId))
            {
                if (ScanAssembly(assemblyPath))
                {
                    result = true;
                    break;
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn(
                "检查皮肤包是否新增游戏内容失败，本次按外观包处理：" +
                exception.GetBaseException().Message);
        }

        lock (Cache)
        {
            Cache[key] = result;
        }

        return result;
    }

    private static IEnumerable<string> EnumerateAssemblies(string rootPath, string providerId)
    {
        string primary;
        try
        {
            primary = SkinPackagePaths.Resolve(rootPath, providerId, ".dll");
        }
        catch
        {
            primary = string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(primary) && File.Exists(primary))
        {
            yield return primary;
            yield break;
        }

        if (!Directory.Exists(rootPath))
        {
            yield break;
        }

        foreach (var path in Directory.EnumerateFiles(rootPath, "*.dll", SearchOption.TopDirectoryOnly))
        {
            yield return path;
        }
    }

    private static bool ScanAssembly(string assemblyPath)
    {
        using var stream = new System.IO.FileStream(
            assemblyPath,
            System.IO.FileMode.Open,
            System.IO.FileAccess.Read,
            System.IO.FileShare.ReadWrite | System.IO.FileShare.Delete);
        using var peReader = new PEReader(stream, PEStreamOptions.PrefetchEntireImage);
        if (!peReader.HasMetadata)
        {
            return false;
        }

        var reader = peReader.GetMetadataReader();
        foreach (var typeHandle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(typeHandle);
            var baseName = BaseTypeName(reader, type.BaseType);
            if (baseName != null && ContentModelBaseNames.Contains(baseName))
            {
                return true;
            }
        }

        return false;
    }

    private static string? BaseTypeName(MetadataReader reader, EntityHandle handle)
    {
        try
        {
            switch (handle.Kind)
            {
                case HandleKind.TypeDefinition:
                    return reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)handle).Name);
                case HandleKind.TypeReference:
                    return reader.GetString(reader.GetTypeReference((TypeReferenceHandle)handle).Name);
                case HandleKind.TypeSpecification:
                    var specification = reader.GetTypeSpecification((TypeSpecificationHandle)handle);
                    var blob = reader.GetBlobReader(specification.Signature);
                    // A generic instantiation (e.g. some Model<T>) is not one of the plain model bases.
                    return blob.Length > 0 ? string.Empty : null;
                default:
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }
}

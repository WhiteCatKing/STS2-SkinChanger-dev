using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

namespace STS2SkinChanger.Catalog;

/// <summary>
/// Whether a skin package advances its own look when the player clicks the character-select
/// background.
/// <para>
/// Spine packs that support more than one look install a click catcher on
/// <c>&lt;character&gt;_bg</c> from a <c>NCharacterSelectScreen.SelectCharacter</c> postfix (the Joan
/// 0.3.6 pack does exactly that: <c>CharSelectPatch</c> → <c>CharSelectCycle.Attach</c>). The older
/// 0.3.5-S build of the same pack ships the same palette but no such patch, so clicking does
/// nothing there.
/// </para>
/// <para>
/// The Skin Changer may only add its own click cycling when the package does not have one: two
/// catchers on the same background would advance two different things on a single click, which is
/// exactly what "quick switch must not jump between packs" forbids.
/// </para>
/// Detection reads Harmony attribute metadata only (never the pack's types), so it stays valid for
/// a package that cannot even be loaded.
/// </summary>
internal static class ProviderCharSelectCyclingPolicy
{
    private static readonly Dictionary<string, bool> Cache = new(StringComparer.OrdinalIgnoreCase);

    internal static bool ProvidesOwnCycling(string? assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
        {
            return false;
        }

        string key;
        try
        {
            key = Path.GetFullPath(assemblyPath);
        }
        catch
        {
            return false;
        }

        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var result = false;
        try
        {
            result = File.Exists(key) && Scan(key);
        }
        catch
        {
            // A pack that cannot be inspected is treated as "no cycling": the Skin Changer then
            // offers its own catcher, which is the visible, recoverable failure.
            result = false;
        }

        Cache[key] = result;
        return result;
    }

    private static bool Scan(string assemblyPath)
    {
        using var stream = new FileStream(
            assemblyPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);
        using var peReader = new PEReader(stream, PEStreamOptions.PrefetchEntireImage);
        if (!peReader.HasMetadata)
        {
            return false;
        }

        var reader = peReader.GetMetadataReader();
        foreach (var typeHandle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(typeHandle);
            if (MentionsCharacterSelectClick(reader, type.GetCustomAttributes()))
            {
                return true;
            }

            foreach (var methodHandle in type.GetMethods())
            {
                if (MentionsCharacterSelectClick(
                        reader,
                        reader.GetMethodDefinition(methodHandle).GetCustomAttributes()))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool MentionsCharacterSelectClick(
        MetadataReader reader,
        CustomAttributeHandleCollection attributes)
    {
        foreach (var attributeHandle in attributes)
        {
            var attribute = reader.GetCustomAttribute(attributeHandle);
            if (!AttributeTypeName(reader, attribute.Constructor)
                    .Equals("HarmonyPatch", StringComparison.Ordinal))
            {
                continue;
            }

            var metadata = Encoding.UTF8.GetString(reader.GetBlobBytes(attribute.Value));
            // The target arrives either as the typeof argument alone or with the method name; both
            // carry the type and member names as UTF-8 in the attribute blob.
            if (metadata.Contains("NCharacterSelectScreen", StringComparison.Ordinal) &&
                metadata.Contains("SelectCharacter", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static string AttributeTypeName(MetadataReader reader, EntityHandle constructor)
    {
        EntityHandle typeHandle;
        switch (constructor.Kind)
        {
            case HandleKind.MemberReference:
                typeHandle = reader.GetMemberReference((MemberReferenceHandle)constructor).Parent;
                break;
            case HandleKind.MethodDefinition:
                typeHandle = reader.GetMethodDefinition((MethodDefinitionHandle)constructor).GetDeclaringType();
                break;
            default:
                return string.Empty;
        }

        return typeHandle.Kind switch
        {
            HandleKind.TypeDefinition => reader.GetString(
                reader.GetTypeDefinition((TypeDefinitionHandle)typeHandle).Name),
            HandleKind.TypeReference => reader.GetString(
                reader.GetTypeReference((TypeReferenceHandle)typeHandle).Name),
            _ => string.Empty
        };
    }
}

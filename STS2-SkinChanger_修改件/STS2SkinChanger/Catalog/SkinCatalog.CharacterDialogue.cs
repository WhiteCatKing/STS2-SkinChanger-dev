using System.Text.Json;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    private sealed record CharacterDialogueLocalization(
        string ProviderId,
        ResourceFile File,
        IReadOnlySet<string> CharacterGroups);

    private static CharacterDialogueLocalization? ClassifyCharacterDialogueLocalization(
        PckResourceIndex index,
        string path,
        IEnumerable<string> characterGroups)
    {
        var ownedGroups = characterGroups.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var subjects = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            using var document = JsonDocument.Parse(StripUtf8Bom(index.Archive.ReadFile(path)));
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                var parts = property.Name.Split('.', 4);
                if (property.Value.ValueKind != JsonValueKind.String ||
                    parts.Length != 4 || parts[0].Length == 0 || parts[1] != "talk" ||
                    parts[3].Length == 0 || !ownedGroups.TryGetValue(parts[2], out var groupId))
                {
                    // Mixed dialogue and gameplay tables retain the native global lifetime.
                    return null;
                }

                subjects.Add(groupId);
            }

            return subjects.Count == 0 ? null :
                new CharacterDialogueLocalization(index.Mod.Id, new ResourceFile(index.Archive, path), subjects);
        }
        catch (Exception exception) when (exception is JsonException or IOException)
        {
            return null;
        }
    }

    private CharacterDialogueLocalization? SelectedCharacterDialogueLocalization(
        string path,
        IReadOnlyDictionary<string, string> selections) =>
        _characterDialogueLocalization[path].LastOrDefault(table => table.CharacterGroups.All(groupId =>
            selections.TryGetValue(groupId, out var selection) &&
            TryGetVisualProviderId(groupId, selection, out var providerId) &&
            providerId.Equals(table.ProviderId, StringComparison.OrdinalIgnoreCase)));

    private bool LocalizationOwnerMatches(
        string path,
        string providerId,
        IReadOnlyDictionary<string, string> selections) =>
        _characterDialogueLocalization.Contains(path)
            ? SelectedCharacterDialogueLocalization(path, selections)?.ProviderId.Equals(
                providerId, StringComparison.OrdinalIgnoreCase) == true
            : TryGetLocalizationProviderId(path, out var pathProviderId) &&
              pathProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase);

    private void ApplyCharacterDialogueLocalization(
        IDictionary<string, ResourceFile> files,
        IReadOnlyDictionary<string, string> selections)
    {
        foreach (var tables in _characterDialogueLocalization)
        {
            if (SelectedCharacterDialogueLocalization(tables.Key, selections) is { } selected)
            {
                files[tables.Key] = selected.File;
            }
            else
            {
                files.Remove(tables.Key);
            }
        }
    }
}

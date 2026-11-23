using System.Text.RegularExpressions;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Catalog;

internal sealed partial class SkinCatalog
{
    // Use the selected package's own indexed visual resources, not the package ID or a list of mods.
    // s1/s2 directories, s1_foo, foo_stage2, etc. all normalize to a common visual family.
    private static readonly Regex RuntimeStageSegment = new(
        @"(?i)(?<=^|[/_\-.])(?:(?:stage|phase|damage|wear|hp|s)[_-]?(?<stage>[0-9]+)|(?<stage>healthy|wounded|damaged|critical|fullhp|lowhp))(?=$|[/_\-.])",
        RegexOptions.CultureInvariant);

    private readonly Dictionary<(string Group, string Selection), CardFileStamp[]> _runtimeStageSources = [];
    private readonly Dictionary<PckArchive, bool> _stagedRuntimeArchives = [];

    internal CardFileStamp[] RuntimeStageSources(string groupId, string selection)
    {
        if (selection == BaseOptionId) return [];
        var key = (groupId, selection);
        if (_runtimeStageSources.TryGetValue(key, out var sources)) return sources;

        var option = Groups.FirstOrDefault(group => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?
            .Options.FirstOrDefault(option => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
        if (option == null || option.IsCharacterIconOnly || !IsCharacterAppearanceGroup(groupId))
            return _runtimeStageSources[key] = [];

        var indexes = GetSelectionProviderIndexes(option);
        if (!indexes.Any(index => HasStagedRuntimeVisuals(index.Archive)))
            return _runtimeStageSources[key] = [];

        // Composed appearances can take roots from more than one source. Keep their source
        // identities together while keeping each selection's existing private alias key.
        return _runtimeStageSources[key] = indexes.Select(index => index.Archive.Stamp)
            .Concat(option.Assets.Values.SelectMany(asset => asset.Files)
                .Select(file => file.SourceStamp).OfType<CardFileStamp>())
            .Distinct().OrderBy(stamp => stamp.PhysicalPath, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private bool HasStagedRuntimeVisuals(PckArchive archive)
    {
        if (_stagedRuntimeArchives.TryGetValue(archive, out var staged)) return staged;
        return _stagedRuntimeArchives[archive] = HasStagedRuntimeVisuals(archive.Paths);
    }

    internal static bool HasStagedRuntimeVisuals(IEnumerable<string> paths)
    {
        var families = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in paths)
        {
            var path = StripResourceRedirectSuffix(raw.Replace('\\', '/'));
            var ext = Path.GetExtension(path);
            if (!ext.Equals(".tres", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".res", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".skel", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".spskel", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".spatlas", StringComparison.OrdinalIgnoreCase) &&
                !ext.Equals(".tscn", StringComparison.OrdinalIgnoreCase)) continue;
            var stages = RuntimeStageSegment.Matches(path);
            if (stages.Count == 0) continue;
            var family = RuntimeStageSegment.Replace(path, "#");
            var numbers = string.Join("/", stages.Select(stage => stage.Groups["stage"].Value.TrimStart('0')));
            if (!families.TryAdd(family, numbers) && families[family] != numbers) return true;
        }
        return false;
    }
}

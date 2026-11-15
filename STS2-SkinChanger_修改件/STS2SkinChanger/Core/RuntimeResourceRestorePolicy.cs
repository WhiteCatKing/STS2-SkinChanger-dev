namespace STS2SkinChanger.Core;

internal static class RuntimeResourceRestorePolicy
{
    internal static IReadOnlySet<string> GetGroups(string groupId, IEnumerable<string> touchedForeignGroups,
        string? scopedSelection, string localSelection)
    {
        var groups = touchedForeignGroups.ToHashSet(StringComparer.OrdinalIgnoreCase);
        // A local preview uses the committed selection. Restoring it again remounts a
        // whole provider after its live Spine model has just entered the render tree.
        // A remote override, in contrast, must restore the local group's resources.
        if (CharacterCombatSceneInstantiationPolicy.ShouldRestoreCanonicalOwnership(scopedSelection, localSelection))
            groups.Add(groupId);
        return groups;
    }
}

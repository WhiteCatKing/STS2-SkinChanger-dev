using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

/// <summary>
/// Game-provided act enumeration. ModelDb.Acts is compiled with exactly the four base acts, so
/// content mods that register additional ActModel subclasses never show up there. This helper
/// enumerates every registered ActModel instance and falls back to ModelDb.Acts when reflection
/// is unavailable. Everything that needs "all acts" must use this instead of ModelDb.Acts.
/// </summary>
internal static class ActCatalog
{
    private static List<ActModel>? _acts;

    public static IReadOnlyList<ActModel> All
    {
        get
        {
            lock (typeof(ActCatalog))
            {
                return _acts ??= Enumerate();
            }
        }
    }

    public static void Invalidate()
    {
        lock (typeof(ActCatalog))
        {
            _acts = null;
        }
    }

    private static bool HasContent(ActModel act)
    {
        try
        {
            if (act.AllEncounters.Any())
            {
                return true;
            }
        }
        catch
        {
            // Fall through to the other content probes.
        }

        try
        {
            if (act.AllEvents.Any() || act.AllAncients.Any())
            {
                return true;
            }
        }
        catch
        {
            // Treat an unreadable act as contentless.
        }

        return false;
    }

    private static List<ActModel> Enumerate()
    {
        var acts = new List<ActModel>();
        try
        {
            foreach (var type in ModelDb.AllAbstractModelSubtypes)
            {
                if (type.IsAbstract ||
                    type.IsGenericTypeDefinition ||
                    !typeof(ActModel).IsAssignableFrom(type))
                {
                    continue;
                }

                try
                {
                    var id = ModelDb.GetId(type);
                    if (ModelDb.GetByIdOrNull<ActModel>(id) is not { } act ||
                        type.Name.Contains("Deprecated", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Content mods may register a real act with Index = -1 (temporary/story
                    // stages such as Wuthering Heights). Only drop true placeholders: no
                    // encounters, events or ancients at all.
                    if (act.Index >= 0 || HasContent(act))
                    {
                        acts.Add(act);
                    }
                }
                catch
                {
                    // A content type that cannot be materialised is skipped; the built-in list
                    // still provides the four base acts.
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("枚举地区失败：" + exception.GetBaseException().Message);
        }

        if (acts.Count == 0)
        {
            try
            {
                acts.AddRange(ModelDb.Acts);
            }
            catch
            {
                // Nothing else to add.
            }
        }

        return acts
            .GroupBy(act => act.Id.Entry, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(act => act.Index)
            .ThenByDescending(act => act.IsDefault)
            .ToList();
    }
}

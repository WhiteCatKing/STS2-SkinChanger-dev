using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Random;

namespace STS2SkinChanger.Core.GameApi;

internal static class CombatBackgroundApi
{
    // NRun stores the mutable run state in "_state" (older builds used "_runState").
    internal static readonly FieldInfo? RunStateField =
        AccessTools.Field(typeof(NRun), "_state") ?? AccessTools.Field(typeof(NRun), "_runState");
    internal static readonly PropertyInfo? BackgroundProperty =
        AccessTools.Property(typeof(NCombatRoom), "Background");
    internal static readonly PropertyInfo? HasCustomBackgroundProperty =
        AccessTools.Property(typeof(EncounterModel), "HasCustomBackground");
    // NCombatRoom keeps the combat visuals (and therefore the encounter) in "_visuals".
    internal static readonly FieldInfo? CombatRoomVisualsField =
        AccessTools.Field(typeof(NCombatRoom), "_visuals");

    internal static IEnumerable<MethodBase> PauseMenuTargetMethods() =>
    [
        AccessTools.Method(typeof(NPauseMenu), nameof(NPauseMenu._Ready)),
        AccessTools.Method(typeof(NPauseMenu), nameof(NPauseMenu.OnSubmenuOpened))
    ];

    internal static Rng? CreatePreviewRng()
    {
        var rngType = typeof(Rng);
        foreach (var constructor in rngType.GetConstructors(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != 1)
            {
                continue;
            }

            var type = parameters[0].ParameterType;
            object? seed = type == typeof(ulong) ? 20260814UL
                : type == typeof(uint) ? 20260814u
                : type == typeof(long) ? 20260814L
                : type == typeof(int) ? 20260814
                : null;
            if (seed == null)
            {
                continue;
            }

            try
            {
                return constructor.Invoke([seed]) as Rng;
            }
            catch
            {
                // Try the next shape.
            }
        }

        return rngType.GetProperty("Chaotic", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null) as Rng;
    }
}

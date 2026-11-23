using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using STS2SkinChanger.Core;

using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Ui;

// NBestiaryLayoutDecimillipede.Setup appends new segment creatures to _creatures and to
// _creatureContainer without clearing the previous set. Rebuilding the same entry after a skin
// change therefore stacks another full set of segments on top of the old skin. Clear both the
// way the game's Cleanup would if it owned these nodes; the layout rebuild then starts clean.
[FeaturePatch(FeatureIds.Ancient)]
[HarmonyPatch(typeof(NBestiaryLayoutDecimillipede), "Setup")]
internal static class DecimillipedeBestiarySetupCleanupPatch
{
    internal const string GroupId = "decimillipede";

    private sealed record StaticFormation(NCreature[] Segments, Vector2[] Positions, Vector2 Center);

    private static readonly ConditionalWeakTable<NBestiaryLayoutDecimillipede, StaticFormation> StaticFormations = new();

    [HarmonyPrefix]
    private static void Prefix(NBestiaryLayoutDecimillipede __instance)
    {
        try
        {
            StaticFormations.Remove(__instance);
            if (AccessTools.Field(typeof(NBestiaryLayoutDecimillipede), "_creatures")
                    ?.GetValue(__instance) is IList creatures)
            {
                foreach (var item in creatures)
                {
                    if (item is Node node &&
                        GodotObject.IsInstanceValid(node) &&
                        !node.IsQueuedForDeletion())
                    {
                        node.QueueFree();
                    }
                }

                creatures.Clear();
            }

            if (AccessTools.Field(typeof(NBestiaryLayoutDecimillipede), "_creatureContainer")
                    ?.GetValue(__instance) is Node container &&
                GodotObject.IsInstanceValid(container))
            {
                foreach (var child in container.GetChildren())
                {
                    if (child is Node node &&
                        GodotObject.IsInstanceValid(node) &&
                        !node.IsQueuedForDeletion())
                    {
                        node.QueueFree();
                    }
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("清理残杀千足虫图鉴旧段落失败：" + exception.GetBaseException().Message);
        }
    }

    [HarmonyPostfix]
    private static void Postfix(NBestiaryLayoutDecimillipede __instance)
    {
        try
        {
            var selection = SkinService.GetDisplayedSelection(GroupId);
            if (!string.Equals(SkinService.Catalog?.ResolveVisualProviderId(selection),
                    "Kiteseven sfw csqzc", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var segments = __instance.GetCreatures().ToArray();
            if (segments.Length == 3)
            {
                var positions = segments.Select(segment => segment.Position).ToArray();
                StaticFormations.Add(__instance, new StaticFormation(segments, positions, positions[1]));
                ApplyStaticScale(__instance, SkinService.GetSelectedMonsterScale(GroupId));
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"设置残杀千足虫图鉴站位失败：{exception.GetBaseException().Message}");
        }
    }

    internal static void ApplyStaticScale(NBestiaryLayoutDecimillipede layout, float scale)
    {
        if (StaticFormations.TryGetValue(layout, out var formation))
        {
            for (var index = 0; index < formation.Segments.Length; index++)
            {
                formation.Segments[index].Position = ScalePosition(
                    formation.Positions[index], formation.Center, scale);
            }
        }
    }

    internal static Vector2 ScalePosition(Vector2 position, Vector2 center, float scale) =>
        center + (position - center) * scale;
}

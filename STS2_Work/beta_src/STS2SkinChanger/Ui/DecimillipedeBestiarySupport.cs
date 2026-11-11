using System;
using System.Collections;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// NBestiaryLayoutDecimillipede.Setup appends new segment creatures to _creatures and to
// _creatureContainer without clearing the previous set. Rebuilding the same entry after a skin
// change therefore stacks another full set of segments on top of the old skin. Clear both the
// way the game's Cleanup would if it owned these nodes; the layout rebuild then starts clean.
[HarmonyPatch(typeof(NBestiaryLayoutDecimillipede), "Setup")]
internal static class DecimillipedeBestiarySetupCleanupPatch
{
    [HarmonyPrefix]
    private static void Prefix(NBestiaryLayoutDecimillipede __instance)
    {
        try
        {
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
}

using System.Reflection;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.RestSite;

namespace STS2SkinChanger.Core;

internal static class RedHoodCompanionCompat
{
    private const string Provider = "RedHoodNecrobinder";
    private static readonly Harmony Patcher = new(Entry.ModId + ".redhood-companion");

    internal static void ApplyInitialLocalAppearance(string providerId)
    {
        const string groupId = "necrobinder";
        if (providerId.Equals(Provider, StringComparison.OrdinalIgnoreCase) &&
            SkinService.GetSelectedRuntimeProvider(groupId) == providerId &&
            SkinService.GetDisplayedSelection(groupId) == SkinService.Config.GetSelection(groupId))
        {
            ManualCharacterVariantBridge.ApplyLocalChoice(groupId);
        }
    }

    internal static void Prepare(Assembly assembly)
    {
        if (assembly.GetName().Name != Provider) return;

        Patch(assembly, Provider + ".NativeCreatureVisuals", "Prefix",
            [typeof(Creature), typeof(NCreatureVisuals).MakeByRefType()], nameof(AllowVisualFactory));
        Patch(assembly, Provider + ".Mod", "AttachCombat", [typeof(NCreature)], nameof(AllowCombatActor));
        Patch(assembly, Provider + ".Mod", "Decorate", [typeof(Node), typeof(string)], nameof(AllowRestSiteActor));
    }

    private static void Patch(Assembly assembly, string type, string name, Type[] arguments, string prefix)
    {
        var owner = assembly.GetType(type);
        var method = owner == null ? null : AccessTools.DeclaredMethod(owner, name, arguments);
        if (method == null || Harmony.GetPatchInfo(method)?.Owners.Contains(Patcher.Id) == true) return;
        Patcher.Patch(method, prefix: new HarmonyMethod(typeof(RedHoodCompanionCompat), prefix));
    }

    internal static bool UsesIndependentOsty(Creature creature) =>
        SkinService.Config.CompanionSelectionDecoupled && creature.Monster is Osty &&
        creature.PetOwner is { } owner && LocalContext.IsMe(owner);

    private static bool AllowVisualFactory(Creature __0, ref bool __result)
    {
        if (!UsesIndependentOsty(__0)) return true;

        // This is the author's prefix, not Creature.CreateVisuals itself. Returning true from
        // the author callback leaves the game and the selected Osty provider's factory in charge.
        __result = true;
        return false;
    }

    // The author keeps its SceneTree.NodeAdded subscription after its Harmony hooks are removed.
    // A rebuilt local camp character can still reach the deferred decorator after switching away.
    private static bool AllowRestSiteActor(Node __0) =>
        __0 is not NRestSiteCharacter { Player: { } player } ||
        !LocalContext.IsMe(player) ||
        SkinService.GetSelectedRuntimeProvider("necrobinder") == Provider;

    private static bool AllowCombatActor(NCreature __0) => !UsesIndependentOsty(__0.Entity);
}

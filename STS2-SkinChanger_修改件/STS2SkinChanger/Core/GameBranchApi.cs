using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace STS2SkinChanger.Core;

internal static class GameBranchApi
{
    internal static CanvasItem? CurrentBody(MegaCrit.Sts2.Core.Nodes.Combat.NCreatureVisuals visuals) =>
        AccessTools.Property(visuals.GetType(), "Body")?.GetValue(visuals) as CanvasItem ??
        AccessTools.Method(visuals.GetType(), "GetCurrentBody", Type.EmptyTypes)?.Invoke(visuals, null) as CanvasItem;

    internal static object?[] OptionalArguments(MethodInfo method, object first, string? seed = null) =>
        method.GetParameters().Select((p, i) => i == 0 ? first :
            p.Name == "seed" ? seed : p.HasDefaultValue ? p.DefaultValue :
            throw new MissingMethodException("Unsupported required preview argument: " + p.Name)).ToArray();

    internal static RunState CreatePreviewRun(IReadOnlyList<Player> players, string seed)
    {
        var method = typeof(RunState).GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == "CreateForTest" && m.GetParameters()[0].ParameterType.IsInstanceOfType(players));
        return (RunState)method.Invoke(null, OptionalArguments(method, players, seed))!;
    }

    internal static void AddCharacterDetails(CharacterModel character, LocString text)
    {
        var method = character.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "AddDetailsTo" && m.GetParameters() is { Length: > 0 } p &&
                p[0].ParameterType == typeof(LocString) && p.Skip(1).All(a => a.HasDefaultValue));
        method?.Invoke(character, OptionalArguments(method, text));
    }

    internal static void StartLoop(object state, string animation) =>
        AccessTools.Method(state.GetType(), "SetAnimation", [typeof(string), typeof(bool), typeof(int)])!
            .Invoke(state, [animation, true, 0]);
}

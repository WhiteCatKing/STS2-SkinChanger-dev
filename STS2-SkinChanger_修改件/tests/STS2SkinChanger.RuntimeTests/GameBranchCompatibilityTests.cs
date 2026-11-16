using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger;
using STS2SkinChanger.Core;

internal static class GameBranchCompatibilityTests
{
    internal static void Run()
    {
        var path = typeof(Entry).Assembly.Location;
        var prepared = ProviderAssemblyCompatibility.PrepareForCurrentGame(path);
        using var image = prepared.Assembly;
        Require(prepared.Report.Failure == null && prepared.Report.Changes.Count == 0 &&
                prepared.Report.UnresolvedReferences.Count == 0,
            "Shipped DLL must need no rewriting on this branch: " + prepared.Report);
        var body = AccessTools.Property(typeof(NCreatureVisuals), "Body");
        var bodyType = body?.PropertyType ?? AccessTools.Method(typeof(NCreatureVisuals), "GetCurrentBody", Type.EmptyTypes)?.ReturnType;
        Require(bodyType != null && typeof(CanvasItem).IsAssignableFrom(bodyType), "Native body contract unavailable.");
        foreach (var type in new[] { typeof(FrameworkModelPreview), typeof(IndependentFoolSkinCompat), typeof(GameBranchApi) })
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                         .Where(m => !m.ContainsGenericParameters && m.GetMethodBody() != null))
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
        var create = typeof(RunState).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(m => m.Name == "CreateForTest");
        IReadOnlyList<Player> players = Array.Empty<Player>();
        var arguments = GameBranchApi.OptionalArguments(create, players, "branch-preview");
        Require(ReferenceEquals(arguments[0], players) && arguments.Length == create.GetParameters().Length &&
            Equals(arguments[Array.FindIndex(create.GetParameters(), p => p.Name == "seed")], "branch-preview"),
            "Preview run adapter must retain players/seed and all native optional defaults.");
        var target = AccessTools.Method(typeof(Vakuu), "ConfigureBackgroundScene");
        var methods = (IEnumerable<MethodBase>)AccessTools.Method(typeof(VakuuVanillaArtPatch), "TargetMethods").Invoke(null, null)!;
        Require(methods.SequenceEqual(target == null ? [] : new MethodBase[] { target }), "Optional native callbacks must follow game capabilities.");
        Console.WriteLine($"Branch ABI passed: {typeof(NCreatureVisuals).Assembly.ManifestModule.ModuleVersionId}; zero rewritten/unresolved references; preview JIT, Body={bodyType!.Name}, native preview defaults and optional Vakuu callback.");
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

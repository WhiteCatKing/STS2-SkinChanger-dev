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
        var invalidCalls = new List<string>();
        var screen = typeof(STS2SkinChanger.Ui.CharacterAppearanceScreen);
        foreach (var type in typeof(Entry).Assembly.GetTypes().Where(t => t == screen || t.FullName!.StartsWith(screen.FullName + "+", StringComparison.Ordinal)))
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(m => m.GetMethodBody() != null && !m.ContainsGenericParameters))
                foreach (var instruction in PatchProcessor.GetOriginalInstructions(method))
                    if (instruction.opcode == System.Reflection.Emit.OpCodes.Call && instruction.operand is MethodInfo { IsAbstract: true } native)
                        invalidCalls.Add(method.Name + " -> " + native.DeclaringType + "." + native.Name);
        Require(invalidCalls.Count == 0, "Nonvirtual calls to native abstract methods are invalid on this branch: " + string.Join("; ", invalidCalls));
        var merchantGetter = AccessTools.PropertyGetter(typeof(MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom), "MerchantButton");
        Require(PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(GameBranchApi), "MerchantButton"))
            .Any(i => i.opcode == System.Reflection.Emit.OpCodes.Callvirt && Equals(i.operand, merchantGetter)),
            "Merchant accessor must keep virtual dispatch when the concrete stable property becomes abstract.");
        var blitzType = typeof(RunState).Assembly.GetType("MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom");
        if (blitzType != null)
        {
            var instance = RuntimeHelpers.GetUninitializedObject(blitzType); GC.SuppressFinalize(instance);
            Require(STS2SkinChanger.Ui.MerchantRuntimeAppearance.ResolveMerchantButtonField(instance)?.FieldType.Name == "NBlitzMerchantButton" &&
                STS2SkinChanger.Ui.MerchantRuntimeAppearance.GetMerchantScenePath(roomType: blitzType) == "res://scenes/rooms/blitz_merchant_room.tscn" &&
                STS2SkinChanger.Ui.MerchantRuntimeAppearance.GetMerchantScenePath("fake_merchant_monster", blitzType).Contains("fake_merchant"),
                "Live Blitz swaps must use its concrete scene/button while fake merchant keeps its own scene.");
        }
        CheckMerchantDispatch(blitzType ?? typeof(MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom));
        var body = AccessTools.Property(typeof(NCreatureVisuals), "Body");
        var bodyType = body?.PropertyType ?? AccessTools.Method(typeof(NCreatureVisuals), "GetCurrentBody", Type.EmptyTypes)?.ReturnType;
        Require(bodyType != null && typeof(CanvasItem).IsAssignableFrom(bodyType), "Native body contract unavailable.");
        foreach (var type in new[] { typeof(FrameworkModelPreview), typeof(IndependentFoolSkinCompat), typeof(GameBranchApi) })
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                         .Where(m => !m.ContainsGenericParameters && m.GetMethodBody() != null))
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
        var iterator = typeof(STS2SkinChanger.Ui.CharacterAppearanceScreen).GetMethod("EnumerateSelectableTargets", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetCustomAttribute<IteratorStateMachineAttribute>()!.StateMachineType;
        RuntimeHelpers.PrepareMethod(iterator.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic)!.MethodHandle);
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
    private static MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantButton? _expectedMerchant;
    private static void CheckMerchantDispatch(Type concreteRoomType)
    {
        var getter = AccessTools.PropertyGetter(concreteRoomType, "MerchantButton");
        var harmony = new Harmony("sc.test.branch-merchant-dispatch");
        var room = (MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantRoom)RuntimeHelpers.GetUninitializedObject(concreteRoomType);
        _expectedMerchant = (MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantButton)RuntimeHelpers.GetUninitializedObject(typeof(MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantButton));
        GC.SuppressFinalize(room); GC.SuppressFinalize(_expectedMerchant);
        try
        {
            harmony.Patch(getter, prefix: new HarmonyMethod(typeof(GameBranchCompatibilityTests), nameof(CaptureMerchant)));
            Require(ReferenceEquals(GameBranchApi.MerchantButton(room), _expectedMerchant) && GameBranchApi.MerchantButton(null) == null,
                "Runtime merchant dispatch must use the concrete override and handle no room.");
        }
        finally { harmony.UnpatchAll(harmony.Id); _expectedMerchant = null; }
    }
    private static bool CaptureMerchant(ref MegaCrit.Sts2.Core.Nodes.Rooms.NMerchantButton __result)
    { __result = _expectedMerchant!; return false; }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

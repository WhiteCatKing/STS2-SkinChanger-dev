using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;
using STS2SkinChanger.Core.Features.Card;
using STS2SkinChanger.Pck;

internal static class CardPipelineExceptionTests
{
    private static readonly List<string> Warnings = [];
    private static Exception? _nativeFailure;
    private static Exception? _localFailure;
    private static int _completedOriginalBodies;

    internal static void Run()
    {
        var enter = AccessTools.DeclaredMethod(typeof(NCard), nameof(NCard._EnterTree), Type.EmptyTypes);
        var ready = AccessTools.DeclaredMethod(typeof(NCard), nameof(NCard._Ready), Type.EmptyTypes);
        var update = AccessTools.DeclaredMethod(typeof(NCard), nameof(NCard.UpdateVisuals),
            [typeof(PileType), typeof(CardPreviewMode)]);
        var reload = AccessTools.DeclaredMethod(typeof(NCard), "Reload", Type.EmptyTypes);
        var harmony = new Harmony("Gurio.SkinChanger.Tests.CardPipelineBoundary");
        var fixture = new Harmony("Tests.ThirdPartyCardPatch");
        try
        {
            harmony.Patch(AccessTools.Method(typeof(ModLog), nameof(ModLog.Warn)),
                prefix: Callback(nameof(CaptureWarning)));
            foreach (var target in new[] { enter, ready, update, reload })
                harmony.Patch(target, transpiler: Callback(nameof(ReplaceGameBody)));
            harmony.CreateClassProcessor(typeof(CardPipelineExceptionBoundary)).Patch();
            foreach (var target in new[] { enter, ready, update, reload })
                Require(Harmony.GetPatchInfo(target).Finalizers.Any(patch =>
                        patch.PatchMethod.DeclaringType == typeof(CardPipelineExceptionBoundary)),
                    $"Missing exception boundary on NCard.{target.Name}.");

            var card = (NCard)RuntimeHelpers.GetUninitializedObject(typeof(NCard));
            GC.SuppressFinalize(card);
            var args = new object[] { default(PileType), default(CardPreviewMode) };

            // Each patched original completes, then its simulated third-party postfix fails.
            fixture.Patch(enter, postfix: Callback(nameof(ForeignEnterTreePostfix)));
            fixture.Patch(update, postfix: Callback(nameof(ForeignBclPostfix)));
            for (var count = 0; count < 3; count++)
            {
                enter.Invoke(card, null);
                update.Invoke(card, args);
            }
            Require(_completedOriginalBodies == 6, "A failing provider must leave both card callers able to continue.");
            Require(Warnings.Count == 2 && Warnings.Count(message =>
                    message.StartsWith("[卡牌管线] 已隔离第三方异常: ", StringComparison.Ordinal) &&
                    message.Contains("ObjectDisposedException", StringComparison.Ordinal)) == 2 &&
                    Warnings.Any(message => message.Contains("NCard._EnterTree", StringComparison.Ordinal)) &&
                    Warnings.Any(message => message.Contains("NCard.UpdateVisuals", StringComparison.Ordinal)),
                "Repeated failures must log once per assembly and original card method.");

            fixture.UnpatchAll(fixture.Id);
            _nativeFailure = new InvalidOperationException("Game card update fixture.");
            Require(ReferenceEquals(Capture(() => update.Invoke(card, args)).InnerException, _nativeFailure),
                "An exception thrown by the original card body must propagate unchanged.");
            _nativeFailure = null;

            fixture.Patch(update, postfix: Callback(nameof(ForeignCallsLocalPatch)));
            Require(ReferenceEquals(Capture(() => update.Invoke(card, args)).InnerException, _localFailure),
                "A Skin Changer exception must still propagate when a third-party caller is also on its stack.");
            Require(Warnings.Count == 2, "Game and Skin Changer failures must not be logged as third-party faults.");

            var godotFailure = Capture(ForeignCallsGodotMath);
            Require(CardPipelineExceptionBoundary.FindThirdPartyOrigin(godotFailure) == typeof(CardPipelineExceptionTests).Assembly,
                "Godot framework and reflection frames must lead back to the fixture that called them.");
            Require(CardPipelineExceptionBoundary.FindThirdPartyOrigin(new ObjectDisposedException("no stack")) == null,
                "Exceptions without an attributable throw site must be preserved.");
            var ambiguous = new AggregateException(new InvalidOperationException("Second cause."), _localFailure!);
            Require(CardPipelineExceptionBoundary.FindThirdPartyOrigin(ambiguous) == null,
                "Multiple independent causes must not be classified as one third-party fault.");
        }
        finally
        {
            fixture.UnpatchAll(fixture.Id);
            harmony.UnpatchAll(harmony.Id);
            _nativeFailure = null;
            _localFailure = null;
            _completedOriginalBodies = 0;
            Warnings.Clear();
        }
        Console.WriteLine("Card pipeline exception isolation passed: Godot/BCL provider stacks, native and local failures, deduplication.");
    }

    private static HarmonyMethod Callback(string name) => new(typeof(CardPipelineExceptionTests), name);

    private static IEnumerable<CodeInstruction> ReplaceGameBody(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var notThrowing = generator.DefineLabel();
        var nativeFailure = AccessTools.Field(typeof(CardPipelineExceptionTests), nameof(_nativeFailure));
        var executions = AccessTools.Field(typeof(CardPipelineExceptionTests), nameof(_completedOriginalBodies));
        // Inline the throw in the generated original body. Calling a fixture method to throw
        // would put this test assembly on the stack and accidentally make it a provider fault.
        yield return new CodeInstruction(OpCodes.Ldsfld, nativeFailure);
        yield return new CodeInstruction(OpCodes.Brfalse, notThrowing);
        yield return new CodeInstruction(OpCodes.Ldsfld, nativeFailure);
        yield return new CodeInstruction(OpCodes.Throw);
        yield return new CodeInstruction(OpCodes.Ldsfld, executions).WithLabels(notThrowing);
        yield return new CodeInstruction(OpCodes.Ldc_I4_1);
        yield return new CodeInstruction(OpCodes.Add);
        yield return new CodeInstruction(OpCodes.Stsfld, executions);
        yield return new CodeInstruction(OpCodes.Ret);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForeignEnterTreePostfix() => throw new ObjectDisposedException("Godot.AtlasTexture");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForeignBclPostfix() => ObjectDisposedException.ThrowIf(true, "cached card texture");

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForeignCallsGodotMath() =>
        AccessTools.PropertyGetter(typeof(Vector2), "Item").Invoke(Vector2.Zero, [9]);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ForeignCallsLocalPatch()
    {
        try { PckArchive.Open(string.Empty); }
        catch (Exception error)
        {
            _localFailure = error;
            throw;
        }
    }

    private static bool CaptureWarning(string message)
    {
        Warnings.Add(message);
        return false;
    }

    private static TargetInvocationException Capture(Action action)
    {
        try { action(); }
        catch (TargetInvocationException error) { return error; }
        throw new InvalidOperationException("Expected the game caller to receive the original exception.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

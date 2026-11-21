using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace STS2SkinChanger.Core.Features.Card;

[HarmonyPatch]
[FeaturePatch(FeatureIds.Card)]
internal static class CardPipelineExceptionBoundary
{
    private static readonly ConcurrentDictionary<(Assembly Source, MethodBase Target), byte> Reported = new();
    private static readonly FieldInfo? ModAssemblyField = AccessTools.Field(typeof(Mod), "assembly");

    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.DeclaredMethod(typeof(NCard), nameof(NCard._EnterTree), Type.EmptyTypes);
        yield return AccessTools.DeclaredMethod(typeof(NCard), nameof(NCard._Ready), Type.EmptyTypes);
        yield return AccessTools.DeclaredMethod(typeof(NCard), nameof(NCard.UpdateVisuals),
            [typeof(PileType), typeof(CardPreviewMode)]);
        // Model/visibility setters can call Reload directly, before an UpdateVisuals boundary.
        yield return AccessTools.DeclaredMethod(typeof(NCard), "Reload", Type.EmptyTypes);
    }

    [HarmonyFinalizer]
    [HarmonyPriority(Priority.Last)]
    private static Exception? Finalizer(Exception? __exception, MethodBase __originalMethod)
    {
        if (__exception == null) return null;

        Assembly? source;
        try { source = FindThirdPartyOrigin(__exception); }
        catch { return __exception; }
        if (source == null) return __exception;

        if (Reported.TryAdd((source, __originalMethod), 0))
        {
            try
            {
                ModLog.Warn($"[卡牌管线] 已隔离第三方异常: {ResolveDisplayName(source)} " +
                            $"{__originalMethod.DeclaringType?.Name}.{__originalMethod.Name} " +
                            $"{__exception.GetBaseException().GetType().Name}");
            }
            catch
            {
                // A diagnostic failure must not interrupt the card caller either.
            }
        }
        return null;
    }

    internal static Assembly? FindThirdPartyOrigin(Exception exception)
    {
        var chain = new Stack<Exception>();
        for (Exception? cause = exception; cause != null; cause = cause.InnerException)
        {
            if (cause is AggregateException { InnerExceptions.Count: not 1 }) return null;
            chain.Push(cause);
        }

        // A reflection exception may have only framework frames in its inner exception.
        // Continue with the enclosing exception's frames only after those are exhausted.
        foreach (var cause in chain)
        foreach (var frame in new StackTrace(cause, fNeedFileInfo: false).GetFrames())
        {
            var owner = frame.GetMethod()?.DeclaringType?.Assembly;
            // A generated Harmony wrapper can contain original game IL. Without a declaring
            // type we cannot distinguish that code from another mod's generated code.
            if (owner == null || owner.IsDynamic) return null;
            if (owner == typeof(NCard).Assembly || owner == typeof(CardPipelineExceptionBoundary).Assembly)
                return null;
            if (!IsFrameworkAssembly(owner)) return owner;
        }
        return null;
    }

    private static bool IsFrameworkAssembly(Assembly assembly)
    {
        if (assembly == typeof(object).Assembly || assembly == typeof(GodotObject).Assembly ||
            assembly == typeof(Harmony).Assembly)
        {
            return true;
        }

        var identity = assembly.GetName();
        var name = identity.Name ?? string.Empty;
        if (name is not ("System" or "mscorlib" or "netstandard" or "Microsoft.CSharp" or
                         "Microsoft.VisualBasic" or "Microsoft.VisualBasic.Core") &&
            !name.StartsWith("System.", StringComparison.Ordinal))
        {
            return false;
        }

        // Match signed platform assemblies. A mod's assembly can also start with "System.".
        var token = identity.GetPublicKeyToken();
        return token != null && Convert.ToHexString(token) is
            "7CEC85D7BEA7798E" or "B03F5F7F11D50A3A" or "CC7B13FFCD2DDD51" or
            "31BF3856AD364E35" or "B77A5C561934E089";
    }

    private static string ResolveDisplayName(Assembly assembly)
    {
        var fallback = assembly.GetName().Name ?? assembly.FullName ?? "<程序集未知>";
        try
        {
            var manifest = ModManager.Mods.FirstOrDefault(mod =>
                ReferenceEquals(ModAssemblyField?.GetValue(mod), assembly))?.manifest;
            return !string.IsNullOrWhiteSpace(manifest?.name) ? manifest.name :
                   !string.IsNullOrWhiteSpace(manifest?.id) ? manifest.id : fallback;
        }
        catch
        {
            return fallback;
        }
    }
}

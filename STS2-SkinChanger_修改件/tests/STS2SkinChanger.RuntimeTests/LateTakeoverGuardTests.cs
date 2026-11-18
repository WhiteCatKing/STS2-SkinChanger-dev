using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class LateTakeoverGuardTests
{
    internal static void Run()
    {
        const string assemblyName = "STS2SkinChanger.ScriptPathFixture";
        const string typeName = "ScriptPathFixture.LateTakeoverFixture";
        var target = new CharSelectTakeoverTarget(assemblyName, typeName, "Takeover", typeName, "Apply", 1);
        var messages = new List<string>();
        var logging = AuditLogSilencer.Install();
        try
        {
            Require(!AppDomain.CurrentDomain.GetAssemblies().Any(assembly => assembly.GetName().Name == assemblyName),
                "The fixture must really arrive after guard installation.");
            CharSelectTakeoverSuppressor.Install(messages.Add, [target]);
            var fixture = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, assemblyName + ".dll"));
            var type = fixture.GetType(typeName, throwOnError: true)!;
            var takeover = AccessTools.Method(type, "Takeover");
            var apply = AccessTools.Method(type, "Apply");
            takeover.Invoke(null, null);
            apply.Invoke(null, ["late"]);
            Require((int)AccessTools.Property(type, "TakeoverCalls").GetValue(null)! == 0 &&
                messages.SequenceEqual(new[] { "late" }), "A late assembly must have both hooks installed immediately.");

            CharSelectTakeoverSuppressor.Install(messages.Add, [target]);
            CharSelectTakeoverSuppressor.EchoSuppressed = true;
            apply.Invoke(null, ["internal"]);
            CharSelectTakeoverSuppressor.EchoSuppressed = false;
            apply.Invoke(null, ["manual"]);
            Require(messages.SequenceEqual(new[] { "late", "manual" }),
                "Repeating installation must keep one notification and preserve echo suppression.");
        }
        finally
        {
            CharSelectTakeoverSuppressor.EchoSuppressed = false;
            CharSelectTakeoverSuppressor.Install();
            logging.UnpatchAll(logging.Id);
        }

        var orchis = OrchisSkinFixCompat.ProviderId;
        Require(OrchisSkinFixCompat.OwnsAppearance(orchis, false) &&
            OrchisSkinFixCompat.OwnsAppearance(orchis + "::source:fixture", false) &&
            !OrchisSkinFixCompat.OwnsAppearance("__base__", false) &&
            !OrchisSkinFixCompat.OwnsAppearance("different-skin", false) &&
            !OrchisSkinFixCompat.OwnsAppearance(orchis, true),
            "Orchis must only apply to the active provider family and yield to the standalone summon.");
        const string rest = "res://OrchisNecrobinderSkinMod/animations/rest_site/necrobinder/";
        Require(OrchisSkinFixCompat.RestSkeletonGroup(rest + "rest_site_necrobinder_skel_data.tres") == "necrobinder" &&
            OrchisSkinFixCompat.RestSkeletonGroup(rest + "lloyd_relax_skel_data.tres") == "osty" &&
            OrchisSkinFixCompat.RestSkeletonGroup(rest + "other.tres") == null,
            "The body and summon must be gated independently and unrelated resources stay untouched.");
        Console.WriteLine("Orchis ownership and late select guard passed: independent room groups, late install, idempotence and echo suppression.");
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

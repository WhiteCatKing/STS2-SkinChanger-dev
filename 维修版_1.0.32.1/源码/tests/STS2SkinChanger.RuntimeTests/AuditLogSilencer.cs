using System.Reflection;
using HarmonyLib;

internal static class AuditLogSilencer
{
    internal static bool Suppress() => false;

    internal static Harmony Install()
    {
        var harmony = new Harmony("tests.audit-log-silencer");
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        foreach (var method in typeof(STS2SkinChanger.Core.ModLog).GetMethods(flags)
                     .Where(method => method.Name is "Info" or "Warn" or "Error"))
        {
            harmony.Patch(
                method,
                prefix: new HarmonyMethod(typeof(AuditLogSilencer), nameof(Suppress)));
        }

        return harmony;
    }
}

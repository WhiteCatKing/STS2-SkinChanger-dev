using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

/// <summary>
/// Temporary, cheap evidence log for the "selected skin ≠ rendered skin" and
/// "scale slider ≠ actual size" reports. Every line is prefixed with [外观诊断] so a single
/// grep of godot.log is enough to see which layer picked which skin/transform.
///
/// Rate limiting is per scope: a per-frame caller must not flood the log.
/// </summary>
internal static class AppearanceDiagnostics
{
    private static readonly Dictionary<string, ulong> LastLogged = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Set to false to silence everything at once.</summary>
    internal static bool Enabled { get; set; } = true;

    internal static void Log(string scope, string message, int minIntervalMsec = 400)
    {
        if (!Enabled)
        {
            return;
        }

        ulong now;
        try
        {
            now = Time.GetTicksMsec();
        }
        catch
        {
            now = 0;
        }

        if (now != 0 &&
            LastLogged.TryGetValue(scope, out var last) &&
            now - last < (ulong)Math.Max(0, minIntervalMsec))
        {
            return;
        }

        LastLogged[scope] = now;
        ModLog.Info($"[外观诊断] {scope}: {message}");
    }

    internal static string Describe(CharacterCombatTransform value) =>
        $"scale={value.Scale:F2} off=({value.OffsetX:F0},{value.OffsetY:F0})" +
        $" hb={value.HealthBarScale:F2}({value.HealthBarOffsetX:F0},{value.HealthBarOffsetY:F0})" +
        $" follow={value.HealthBarFollowsModelScale}/{value.HealthBarFollowsModelMovement}" +
        $" intent={value.IntentScale:F2}({value.IntentOffsetX:F0},{value.IntentOffsetY:F0})" +
        $" reticle={value.SelectionReticleScale:F2}({value.SelectionReticleOffsetX:F0}," +
        $"{value.SelectionReticleOffsetY:F0})" +
        $" reticleFollow={value.SelectionReticleFollowsModelScale}/" +
        $"{value.SelectionReticleFollowsModelMovement}";

    internal static string Describe(NCreature creature)
    {
        try
        {
            var kind = creature.Entity.IsPlayer
                ? "player"
                : creature.Entity.PetOwner != null ? "companion" : "monster";
            return $"{kind}:{creature.Entity.Name}({creature.Entity.ModelId.Entry})";
        }
        catch
        {
            return "<creature?>";
        }
    }
}

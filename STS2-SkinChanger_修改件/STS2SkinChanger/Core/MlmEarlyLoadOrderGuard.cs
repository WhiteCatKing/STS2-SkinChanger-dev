using STS2SkinChanger.Catalog;
using MegaCrit.Sts2.Core.Modding;
using System.Collections;
using HarmonyLib;

namespace STS2SkinChanger.Core;

// This guard intentionally throws from the mod initializer. MLM catches initializer failures
// in ResumeNext/ContinueAfterStep, shows its own load-error screen and keeps the startup gate
// closed. Do not defer it to the main menu: game type scanning has already failed by then.
internal static class MlmEarlyLoadOrderGuard
{
    internal static string? LastRejection { get; private set; }
    internal static void Check()
    {
        var violations = FindViolations(ModManager.Mods);
        if (violations.Count == 0) { LastRejection = null; return; }
        RestoreOwnDeclaredPriority();
        LastRejection = Message(violations);
        throw new InvalidOperationException(LastRejection);
    }

    // A user can explicitly ignore a manifest priority when dragging in MLM. Re-enable only
    // our own declaration after an unsafe order is rejected, so its native Organize button
    // can repair that order too. Persist through MLM now: its error-screen restart does not
    // save pending organization changes, which previously restored ignorePriority=true.
    internal static void RestoreOwnDeclaredPriority(Action<object>? persist = null)
    {
        try
        {
            var manager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly =>
                assembly.GetType("ModLaunchManager.ManagerOverlay", false) != null);
            var overlay = manager?.GetType("ModLaunchManager.ManagerOverlay", false);
            if (overlay == null) return;
            if (AccessTools.Field(overlay, "_organization")?.GetValue(null) is { } organization)
            {
                var ignored = AccessTools.Property(organization.GetType(), "Mods")?.GetValue(organization) is IDictionary rows &&
                    rows[Entry.ModId] is { } row && AccessTools.Property(row.GetType(), "IgnorePriority")?.GetValue(row) is true;
                AccessTools.Method(organization.GetType(), "SetIgnorePriority")?.Invoke(organization, [Entry.ModId, false]);
                if (ignored)
                {
                    if (persist != null) persist(organization);
                    else if (manager?.GetType("ModLaunchManager.ModOrganizationStore", false) is { } store)
                        AccessTools.Method(store, "Save")?.Invoke(null, [organization]);
                }
            }
            if (AccessTools.Field(overlay, "_plan")?.GetValue(null) is { } plan &&
                AccessTools.Property(plan.GetType(), "Entries")?.GetValue(plan) is IEnumerable entries)
                foreach (var entry in entries)
                    if (entry != null && Entry.IsSelfModId(AccessTools.Property(entry.GetType(), "Id")?.GetValue(entry) as string))
                        AccessTools.Property(entry.GetType(), "IgnorePriority")?.SetValue(entry, false);
        }
        catch (Exception error)
        {
            // A changed optional UI API must never hide the actionable load-order error.
            System.Diagnostics.Debug.WriteLine("MLM 优先级恢复未完成：" + error.GetBaseException().Message);
        }
    }

    internal static IReadOnlyList<string> FindViolations(IReadOnlyList<Mod> mods, string? gamePack = null)
    {
        if (!mods.Any(mod => mod.manifest?.id == "ModLaunchManager" && mod.state == ModLoadState.Loaded)) return [];
        var self = mods.ToList().FindIndex(mod => Entry.IsSelfModId(mod.manifest?.id) &&
            mod.state is ModLoadState.None or ModLoadState.Loaded);
        if (self < 0) return [];
        var preceding = mods.Take(self).Where(mod => mod.state == ModLoadState.Loaded &&
            !Entry.IsSelfModId(mod.manifest?.id) && mod.manifest?.id != "ModLaunchManager").ToArray();
        if (preceding.Length == 0) return [];
        var failures = preceding.Where(mod => MlmAssemblyContextCompat.RequiresGameContext(mod, mods) &&
                mod.assemblies.Any(assembly => assembly.IsCollectible))
            .Select(mod => mod.manifest?.name ?? mod.manifest?.id ?? mod.path).ToList();
        // Use the same metadata-only detector as normal takeover. Do not call initializers,
        // mount PCKs, or treat every DLL/UI/content mod as a skin merely because it precedes us.
        var descriptors = ManagedSkinModLoader.GetProviderProbeDescriptors(preceding);
        foreach (var provider in SkinCatalog.ProbeSkinProviders(descriptors, gamePack))
            if (preceding.FirstOrDefault(mod => mod.path.Equals(provider.RootPath, StringComparison.OrdinalIgnoreCase)) is { } mod)
                failures.Add(mod.manifest?.name ?? mod.manifest?.id ?? mod.path);
        return failures.Distinct(StringComparer.OrdinalIgnoreCase).Take(8).ToArray();
    }

    internal static string Message(IReadOnlyList<string> violations) =>
        "【必须调整加载顺序】想正常使用皮肤切换器，必须把它紧挨着放在 Mod Launch Manager（MLM）正下方，排在第 2 位。\n" +
        "正确顺序：第 1 位 Mod Launch Manager → 第 2 位 皮肤切换器-Skin Changer → 其他框架和皮肤 Mod。\n" +
        "请点「重启」返回 MLM，点击「一键整理」，确认上述顺序后保存并重新启动游戏。" +
        "本次已经加载过其他模组，不要点「忽略并继续」；继续运行无法正常使用皮肤切换器。\n" +
        "REQUIRED: place Skin Changer directly below MLM (position 2). Restart to MLM, use Organize, verify the order, save and restart. Do not ignore this error and continue.\n" +
        "已提前加载 / Already loaded: " + string.Join("、", violations);
}

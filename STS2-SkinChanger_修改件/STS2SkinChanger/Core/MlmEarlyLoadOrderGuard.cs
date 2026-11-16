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
    internal static void Check()
    {
        var violations = FindViolations(ModManager.Mods);
        if (violations.Count == 0) return;
        RestoreOwnDeclaredPriority();
        throw new InvalidOperationException(Message(violations));
    }

    // A user can explicitly ignore a manifest priority when dragging in MLM. Re-enable only
    // our own declaration after an unsafe order is rejected, so its native Organize button
    // can repair that order too. MLM owns saving this organization on the next user action.
    internal static void RestoreOwnDeclaredPriority()
    {
        try
        {
            var manager = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly =>
                assembly.GetType("ModLaunchManager.ManagerOverlay", false) != null);
            var overlay = manager?.GetType("ModLaunchManager.ManagerOverlay", false);
            if (overlay == null) return;
            if (AccessTools.Field(overlay, "_organization")?.GetValue(null) is { } organization)
                AccessTools.Method(organization.GetType(), "SetIgnorePriority")?.Invoke(organization, [Entry.ModId, false]);
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
        "皮肤切换器加载顺序错误：必须位于所有皮肤 Mod 之前，也必须先于被 MLM 隔离加载的共享框架。" +
        "请返回 Mod Launch Manager，点击「一键整理」，或将「皮肤切换器-Skin Changer」移到 MLM 本体下方（第 2 位）。" +
        "如果本次已经加载过其他模组，请保存正确顺序后重启游戏，不要忽略错误继续启动。\n" +
        "Skin Changer must load before skin mods and isolated shared frameworks. Return to MLM and use Organize, " +
        "or move Skin Changer directly below Mod Launch Manager. Save the order and restart before continuing.\n" +
        "已提前加载 / Already loaded: " + string.Join("、", violations);
}

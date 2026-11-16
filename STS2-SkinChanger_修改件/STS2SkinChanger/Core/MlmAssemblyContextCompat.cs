using System.Collections;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

// MLM's newer collectible contexts cannot be referenced by the game's non-collectible skin
// providers. Shared framework contracts must live beside the game. Let MLM perform the load
// itself so its restart-to-remove bookkeeping remains correct; do not duplicate framework DLLs.
internal static class MlmAssemblyContextCompat
{
    private static readonly Harmony Patches = new(Entry.ModId + ".mlm_shared_contracts");
    private static readonly HashSet<Type> Installed = [];
    private static readonly HashSet<string> FrameworkIds = new(StringComparer.OrdinalIgnoreCase)
        { "STS2-RitsuLib", "BaseLib", "JmcModLib", "thunninoiSkinManager" };

    internal static bool RequiresGameContext(Mod mod, IEnumerable<Mod> mods)
    {
        var all = mods.Where(item => item.manifest?.id != null).ToArray();
        var required = new HashSet<string>(FrameworkIds, StringComparer.OrdinalIgnoreCase);
        var pending = new Queue<Mod>(all.Where(item => required.Contains(item.manifest!.id!)));
        while (pending.TryDequeue(out var current))
            foreach (var dependency in current.manifest!.dependencies ?? [])
                if (dependency.id != null && required.Add(dependency.id))
                    foreach (var target in all.Where(item => item.manifest!.id!.Equals(dependency.id, StringComparison.OrdinalIgnoreCase)))
                        pending.Enqueue(target);
        return mod.manifest?.id is { } id && required.Contains(id);
    }

    internal static bool Install(Assembly? manager = null)
    {
        manager ??= AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly =>
            assembly.GetType("ModLaunchManager.CollectibleModAssemblyLoader", false) != null);
        var policy = manager?.GetType("ModLaunchManager.ModAssemblyLoadingPolicy", false);
        var loader = manager?.GetType("ModLaunchManager.CollectibleModAssemblyLoader", false);
        if (policy == null || loader == null || Installed.Contains(loader)) return false;
        var read = AccessTools.Method(policy, "ReadRequirement", [typeof(Mod)]);
        var load = AccessTools.Method(loader, "LoadFromCollectibleContext");
        if (read?.ReturnType.GetConstructor([typeof(bool), typeof(string)]) == null || load == null ||
            AccessTools.Field(loader, "_loadDecisions") == null) return false;
        try
        {
            Patches.Patch(load, prefix: new HarmonyMethod(typeof(MlmAssemblyContextCompat), nameof(BeforeLoad)) { priority = Priority.First });
            Installed.Add(loader);
            ModLog.Info("已接入 MLM 共享框架加载上下文兼容：RitsuLib/BaseLib 等由游戏上下文加载，其他模组仍遵循 MLM 的隔离策略。");
            return true;
        }
        catch (Exception error)
        {
            try { Patches.Unpatch(load, HarmonyPatchType.All, Patches.Id); } catch { /* Preserve the original compatibility failure. */ }
            ModLog.Warn("MLM 共享框架兼容安装未完成：" + error.GetBaseException().Message);
            return false;
        }
    }

    private static void BeforeLoad(MethodBase __originalMethod, string assemblyPath)
    {
        // The launch plan may have been cached before Skin Changer initialized. Correct this
        // entry immediately before the native MLM loader consumes it, without changing order,
        // enable state, other decisions or MLM's GameContextLoads registration.
        var loader = __originalMethod.DeclaringType!;
        var mod = AccessTools.Method(loader, "FindOwningMod")?.Invoke(null, [Path.GetFullPath(assemblyPath)]) as Mod;
        if (mod == null || !RequiresGameContext(mod, ModManager.Mods)) return;
        var field = AccessTools.Field(loader, "_loadDecisions");
        if (field?.GetValue(null) is not IDictionary decisions)
        {
            var policy = loader.Assembly.GetType("ModLaunchManager.ModAssemblyLoadingPolicy", true)!;
            decisions = (IDictionary)AccessTools.Method(policy, "Build").Invoke(null, [ModManager.Mods])!;
            field!.SetValue(null, decisions);
        }
        var decisionType = decisions.GetType().GetGenericArguments()[1];
        decisions[mod] = Activator.CreateInstance(decisionType,
            [true, "Skin Changer shared framework contract; runtime removal requires restart"]);
    }
}

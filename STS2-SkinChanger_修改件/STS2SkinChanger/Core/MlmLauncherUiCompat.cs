using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

internal static class MlmLauncherUiCompat
{
    private static readonly Harmony Patches = new(Entry.ModId + ".mlm_launcher_ui");
    private static readonly HashSet<Type> Installed = [];
    internal static void Install(Assembly? assembly = null)
    {
        assembly ??= AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(value => value.GetType("ModLaunchManager.ManagerOverlay", false) != null);
        var overlay = assembly?.GetType("ModLaunchManager.ManagerOverlay", false);
        if (overlay == null || Installed.Contains(overlay)) return;
        try
        {
            Patches.Patch(AccessTools.Method(overlay, "AutoSortPlan"), transpiler: new HarmonyMethod(typeof(MlmLauncherUiCompat), nameof(RewriteSort)));
            Patches.Patch(AccessTools.Method(overlay, "BuildLoadErrorDetails"), prefix: new HarmonyMethod(typeof(MlmLauncherUiCompat), nameof(ErrorDetails)));
            Installed.Add(overlay);
        }
        catch (Exception error) { System.Diagnostics.Debug.WriteLine("MLM 排序/错误提示兼容未完成：" + error.GetBaseException().Message); }
    }

    private static bool ErrorDetails(object __0, ref string __result)
    {
        var mod = AccessTools.Property(__0.GetType(), "Mod")?.GetValue(__0) as Mod;
        if (!Entry.IsSelfModId(mod?.manifest?.id) || MlmEarlyLoadOrderGuard.LastRejection is not { } message) return true;
        __result = message;
        return false;
    }

    private static IEnumerable<CodeInstruction> RewriteSort(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.operand is MethodInfo method && method.Name == "AutoSortDependencies")
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = AccessTools.Method(typeof(MlmLauncherUiCompat), nameof(Organize)).MakeGenericMethod(method.ReturnType);
            }
            yield return instruction;
        }
    }

    internal static T Organize<T>(object plan, bool includeDisabledRelations, IReadOnlyCollection<string>? preexisting)
    {
        var result = AccessTools.Method(plan.GetType(), "AutoSortDependencies").Invoke(plan, [includeDisabledRelations, preexisting])!;
        if (AccessTools.Property(result.GetType(), "Success").GetValue(result) is not true) return (T)result;
        if (AccessTools.Property(plan.GetType(), "Entries").GetValue(plan) is not IList entries) return (T)result;
        var self = entries.Cast<object>().FirstOrDefault(entry => Entry.IsSelfModId(Id(entry)) &&
            AccessTools.Property(entry.GetType(), "Enabled").GetValue(entry) is true);
        var manager = entries.Cast<object>().FirstOrDefault(entry => Id(entry) == "ModLaunchManager");
        if (self == null || manager == null || entries.IndexOf(manager) != 0 || entries.IndexOf(self) == 1) return (T)result;
        var oldIndex = entries.IndexOf(self);
        entries.Remove(self); entries.Insert(1, self);
        var validation = AccessTools.Method(plan.GetType(), "Validate").Invoke(plan, null)!;
        if (AccessTools.Property(validation.GetType(), "Success").GetValue(validation) is not true)
        {
            entries.Remove(self); entries.Insert(oldIndex, self);
            return (T)validation; // Never violate a real dependency just to force a visual order.
        }
        var oldMessage = AccessTools.Property(result.GetType(), "Message").GetValue(result) as string;
        var caution = AccessTools.Property(result.GetType(), "Caution")?.GetValue(result) as bool? ?? false;
        return (T)Activator.CreateInstance(typeof(T), [true,
            "皮肤切换器已移至 MLM 正下方（第 2 位）。Skin Changer moved directly below MLM.\n" + oldMessage, caution])!;
    }

    private static string? Id(object entry) => AccessTools.Property(entry.GetType(), "Id")?.GetValue(entry) as string;
}

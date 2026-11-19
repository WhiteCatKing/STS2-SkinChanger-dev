using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class FoolWaifuRuntimeCompat
{
    internal const string ProviderId = "Fool_Waifu";
    private static readonly Harmony Patcher = new(Entry.ModId + ".fool-waifu-selection");
    private static readonly HashSet<MethodInfo> Installed = [];

    internal static void InstallLoaded()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(IsProvider))
            Install(assembly);
    }

    internal static bool IsProvider(Assembly assembly) =>
        !assembly.IsDynamic && string.Equals(assembly.GetName().Name, ProviderId, StringComparison.OrdinalIgnoreCase);

    // Cover already loaded providers and the manager's lazy activation path. The author's
    // NGame prefix lives in a different dynamic assembly, so ordinary provider ownership
    // cannot capture or remove it. Guard the actual callback it forwards to instead.
    internal static void Install(Assembly assembly)
    {
        TryPatch(assembly, "FoolWaifu.ResumeSkinCompatibility", "BeforeRun", [typeof(RunState)], nameof(OwnsFoolSelection));
        // Both _Ready and PlayAnimation call this before the author inserts a sibling rig.
        TryPatch(assembly, "FoolWaifu.SkinVisuals", "AttachMerchant", [typeof(NMerchantCharacter)], nameof(OwnsFoolSelection));
        TryPatch(assembly, "FoolWaifu.SkinAssets", "UiTexture", [typeof(string)], nameof(AllowUiTexture));
    }

    private static void TryPatch(Assembly assembly, string typeName, string methodName, Type[] parameters, string prefix)
    {
        try
        {
            var type = assembly.GetType(typeName, throwOnError: false);
            var target = type == null ? null : AccessTools.DeclaredMethod(type, methodName, parameters);
            if (target == null || !target.IsStatic) return;
            lock (Installed)
            {
                if (Installed.Contains(target)) return;
                Patcher.Patch(target, prefix: new HarmonyMethod(typeof(FoolWaifuRuntimeCompat), prefix));
                Installed.Add(target);
            }
        }
        catch (Exception error)
        {
            ModLog.Warn($"Fool_Waifu {methodName} 兼容拦截安装失败：{error.GetBaseException().Message}");
        }
    }

    internal static bool IsIdentityCallback(MethodInfo callback) =>
        callback.Name == "Postfix" && callback.DeclaringType?.FullName is
            "FoolWaifu.PortraitPatch" or "FoolWaifu.PortraitOutlinePatch" or "FoolWaifu.CharacterIconScenePatch" or
            "FoolWaifu.PackedIconPatches" or "FoolWaifu.CharacterSelectButtonTexturePatch";

    // This is the common return path for the portrait, outline, packed icons, Icon scene and
    // direct button patches, including their private cache. Leave all other author art alone.
    private static bool AllowUiTexture(string __0, ref Texture2D? __result)
    {
        if (__0 is not ("portrait.png" or "portrait_outline.png" or "select_icon.png" or "select_icon_locked.png" or "map_marker.png") ||
            OwnsFoolSelection())
            return true;
        __result = null;
        return false;
    }

    private static bool OwnsFoolSelection()
    {
        var catalog = SkinService.Catalog;
        if (catalog == null) return true;
        var provider = catalog.ResolveVisualProviderId(SkinService.GetDisplayedSelection("fool"));
        return ProviderInstanceIdentityPolicy.IsOptionSelectionAlias(ProviderId, ProviderId, ProviderId, provider);
    }
}

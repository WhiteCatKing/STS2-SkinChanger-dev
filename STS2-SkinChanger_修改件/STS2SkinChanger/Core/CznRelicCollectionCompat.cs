using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

/// <summary>
/// Compatibility bridge for <c>CznStyleUI</c>'s custom Relic Collection archive overlay.
///
/// <c>CznStyleUI</c> replaces <c>NRelicCollection</c> with its own archive screen and resolves
/// relic textures through <c>RelicCatalogProvider.ResolveCatalogIcon</c> and
/// <c>ResolveDetailIcon</c>, which call <c>ResourceLoader.Load</c> directly on the canonical
/// <c>res://images/atlases/relic_atlas.sprites/*.tres</c> and <c>res://images/relics/*.png</c>
/// paths before falling back to <see cref="RelicModel.Icon"/> / <see cref="RelicModel.BigIcon"/>.
/// Because Skin Changer keeps vanilla relic paths mounted in the global VFS to prevent unselected
/// packs from leaking, those direct loads bypass <see cref="RelicIconSkinPatch"/> and
/// <see cref="RelicBigIconSkinPatch"/> and display vanilla icons in the compendium.
///
/// This bridge attaches postfixes to those two methods when <c>CznStyleUI</c> is present so both
/// grid tiles and detail views honor the active Skin Changer relic selection without touching
/// <c>CznStyleUI</c> files or introducing a hard assembly reference.
/// </summary>
internal static class CznRelicCollectionCompat
{
    private const string ProviderTypeName =
        "CznStyleUI.GameModeSelection.RelicCollection.RelicCatalogProvider";

    private static readonly object Sync = new();
    private static Harmony? _harmony;
    private static bool _installed;
    private static bool _assemblyLoadHooked;

    public static void Install(Harmony harmony)
    {
        lock (Sync)
        {
            _harmony = harmony;
            if (!_installed)
            {
                TryInstallFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
            }

            if (!_installed && !_assemblyLoadHooked)
            {
                _assemblyLoadHooked = true;
                AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
            }
        }
    }

    private static void OnAssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        if (_installed)
        {
            return;
        }

        lock (Sync)
        {
            if (_installed || _harmony == null)
            {
                return;
            }

            TryInstallFromAssemblies([args.LoadedAssembly]);
        }
    }

    private static void TryInstallFromAssemblies(IEnumerable<Assembly> assemblies)
    {
        if (_harmony == null || _installed)
        {
            return;
        }

        Type? providerType = null;
        foreach (var assembly in assemblies)
        {
            providerType = assembly.GetType(ProviderTypeName, throwOnError: false);
            if (providerType != null)
            {
                break;
            }
        }

        if (providerType == null)
        {
            return;
        }

        var (resolveCatalog, resolveDetail) = FindTargetMethods(providerType);
        if (resolveCatalog == null && resolveDetail == null)
        {
            _installed = true;
            return;
        }

        try
        {
            var patchedCount = 0;
            if (resolveCatalog != null)
            {
                _harmony.Patch(
                    resolveCatalog,
                    postfix: new HarmonyMethod(
                        typeof(CznRelicCollectionCompat),
                        nameof(ResolveCatalogIconPostfix))
                    {
                        priority = Priority.Last
                    });
                patchedCount++;
            }

            if (resolveDetail != null)
            {
                _harmony.Patch(
                    resolveDetail,
                    postfix: new HarmonyMethod(
                        typeof(CznRelicCollectionCompat),
                        nameof(ResolveDetailIconPostfix))
                    {
                        priority = Priority.Last
                    });
                patchedCount++;
            }

            _installed = true;
            if (patchedCount > 0)
            {
                ModLog.Info($"[兼容] 已接入 CznStyleUI 遗物图鉴图标替换（{patchedCount} 个入口）。");
            }
        }
        catch (Exception exception)
        {
            _installed = true;
            ModLog.Warn("[兼容] 接入 CznStyleUI 遗物图鉴图标替换失败：" + exception.GetBaseException().Message);
        }
    }

    internal static (MethodInfo? ResolveCatalogIcon, MethodInfo? ResolveDetailIcon) FindTargetMethods(
        Type providerType)
    {
        const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var paramTypes = new[] { typeof(RelicModel) };
        var resolveCatalog = providerType.GetMethod("ResolveCatalogIcon", flags, null, paramTypes, null);
        var resolveDetail = providerType.GetMethod("ResolveDetailIcon", flags, null, paramTypes, null);
        return (resolveCatalog, resolveDetail);
    }

    private static void ResolveCatalogIconPostfix(RelicModel? model, ref Texture2D? __result) =>
        RelicSkinResources.ReplaceCatalogIcon(model, ref __result);

    private static void ResolveDetailIconPostfix(RelicModel? model, ref Texture2D? __result) =>
        RelicSkinResources.ReplaceDetailIcon(model, ref __result);
}

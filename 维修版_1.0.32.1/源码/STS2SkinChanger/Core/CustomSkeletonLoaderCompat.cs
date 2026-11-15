using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// Compatibility bridge for the "Custom Skeleton Loader" Workshop/Nexus mod (id
/// <c>CustomSkeletonLoader</c>, author Khiral). That mod drops external Spine assets into
/// <c>mods/CustomSkeletonLoader/ReplaceResources</c> and swaps the game's whole
/// <c>SpineSkeletonDataResource</c> (skeleton + atlas + texture) for a matching skeleton name,
/// from <c>AssetCache.LoadAsset</c>, <c>AssetLoadingSession.AddToCache</c> and several node
/// <c>_Ready</c> hooks.
///
/// A Skin Changer skin is also a complete appearance for that character, so both mods want to own
/// the same creature: run together, the loader replaces the skeleton the selected skin just bound
/// (or hijacks the skin's <c>_skel_data.tres</c> load), and the model comes out broken. Every loader
/// hook funnels through <c>SkeletonLoader.TryBuildMegaSkeletonDataResource(name)</c>, so deferring
/// there is enough: when this client currently shows a skin for that group, the loader returns "no
/// replacement" and the game/skin resource stays untouched. With the base appearance selected the
/// loader works exactly as before.
///
/// Nothing in the other mod is modified; the bridge is a Harmony prefix installed at runtime and
/// silently skipped when that mod is not installed.
/// </summary>
internal static class CustomSkeletonLoaderCompat
{
    private const string LoaderTypeName = "CustomSkeletonLoader.SkeletonLoader";
    private const string LoaderMethodName = "TryBuildMegaSkeletonDataResource";

    private static readonly HashSet<string> ReportedTokens = new(StringComparer.OrdinalIgnoreCase);
    private static Harmony? _harmony;
    private static bool _installed;
    private static bool _watching;

    public static void Install(Harmony harmony)
    {
        _harmony = harmony;
        TryInstall();
    }

    private static void TryInstall()
    {
        if (_installed)
        {
            return;
        }

        var loaderType = AppDomain.CurrentDomain.GetAssemblies()
            .Select(assembly => assembly.GetType(LoaderTypeName, throwOnError: false))
            .FirstOrDefault(type => type != null);
        if (loaderType == null)
        {
            if (!_watching)
            {
                _watching = true;
                AppDomain.CurrentDomain.AssemblyLoad += (_, _) => TryInstall();
            }

            return;
        }

        _installed = true;
        try
        {
            var target = loaderType.GetMethod(
                LoaderMethodName,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                [typeof(string)],
                null);
            if (target == null)
            {
                ModLog.Warn(
                    "Custom Skeleton Loader 兼容：未找到 " + LoaderTypeName + "." + LoaderMethodName +
                    "，跳过兼容处理（该 Mod 的版本可能不同）。");
                return;
            }

            _harmony!.Patch(
                target,
                prefix: new HarmonyMethod(
                    typeof(CustomSkeletonLoaderCompat),
                    nameof(DeferToSelectedSkin)));
            ModLog.Info(
                "Custom Skeleton Loader 兼容已启用：当前选中的皮肤拥有该角色时，" +
                "它的骨骼替换会让位给皮肤（原皮外观下仍照常生效）。");
        }
        catch (Exception exception)
        {
            _installed = false;
            ModLog.Warn("Custom Skeleton Loader 兼容安装失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Returns false (and a null replacement) when a Skin Changer skin currently owns this
    /// skeleton name, which makes every loader hook fall back to the untouched game resource.
    /// </summary>
    private static bool DeferToSelectedSkin(string name, ref MegaSkeletonDataResource __result)
    {
        try
        {
            if (!SkinOwnsSkeleton(name))
            {
                return true;
            }

            __result = null!;
            return false;
        }
        catch
        {
            // A compatibility guard must never break the other mod's own path.
            return true;
        }
    }

    private static bool SkinOwnsSkeleton(string name)
    {
        var token = NormalizeSkeletonName(name);
        if (token.Length == 0)
        {
            return false;
        }

        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return false;
        }

        var group = catalog.Groups.FirstOrDefault(candidate =>
            candidate.Id.Equals(token, StringComparison.OrdinalIgnoreCase));
        if (group == null)
        {
            // Names such as "ironclad_shop", "merchant_hand" or "characterselect_regent" carry the
            // group as a prefix; prefer the longest group id so "fake_merchant_monster" wins over
            // a shorter accidental match.
            group = catalog.Groups
                .Where(candidate => token.StartsWith(candidate.Id, StringComparison.OrdinalIgnoreCase) &&
                                    (token.Length == candidate.Id.Length ||
                                     token[candidate.Id.Length] is '_' or '-'))
                .OrderByDescending(candidate => candidate.Id.Length)
                .FirstOrDefault();
        }

        if (group == null)
        {
            return false;
        }

        if (HasNonBaseSelection(group.Id))
        {
            if (ReportedTokens.Add(group.Id))
            {
                ModLog.Info(
                    $"Custom Skeleton Loader 让位给皮肤：{group.Id} 当前使用皮肤选择，" +
                    "不再用 ReplaceResources 里的骨骼替换该角色。");
            }

            return true;
        }

        return false;
    }

    private static bool HasNonBaseSelection(string groupId)
    {
        if (!IsBase(SkinService.Config.GetSelection(groupId)))
        {
            return true;
        }

        // A teammate's shared skin also owns that model in multiplayer; the loader is global, so
        // replacing it would break the remote player's appearance too.
        foreach (var selections in MultiplayerSkinSync.GetAvailableSelectionMaps())
        {
            if (selections.TryGetValue(groupId, out var selection) && !IsBase(selection))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsBase(string? selection) =>
        string.IsNullOrWhiteSpace(selection) ||
        selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeSkeletonName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var token = name.Trim().Replace('\\', '/');
        var slash = token.LastIndexOf('/');
        if (slash >= 0)
        {
            token = token[(slash + 1)..];
        }

        foreach (var suffix in new[] { "_skel_data", "_skeleton_data", "_skel", "_data" })
        {
            if (token.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                token = token[..^suffix.Length];
            }
        }

        foreach (var prefix in new[] { "characterselect_", "charselect_", "character_select_" })
        {
            if (token.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                token = token[prefix.Length..];
            }
        }

        return token.Trim('_', '-');
    }
}

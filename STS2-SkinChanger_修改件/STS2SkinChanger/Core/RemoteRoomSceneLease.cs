using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace STS2SkinChanger.Core;

/// <summary>
/// Makes a remote player's room scene (rest site, merchant) really use that player's skin.
///
/// <para>
/// The game preloads room scenes by reading the character model's own getters
/// (<c>RestSiteAnimPath</c>/<c>MerchantAnimPath</c>) once for the whole room, before any player
/// scope exists. That call populates <see cref="PreloadManager"/>.Cache with the vanilla scene
/// under the canonical path. Creating a remote player's node later inside our per-player scope
/// still asks the cache for the same canonical path, so the player gets the preloaded vanilla
/// scene and the skin only shows up in combat.
/// </para>
///
/// <para>
/// The fix primes the cache with the skin's scene for the duration of that one instantiation and
/// restores the previous entry afterwards: the node keeps the scene it instantiated, while every
/// later local load sees the original resource again.
/// </para>
/// </summary>
internal sealed class RemoteRoomSceneLease : IDisposable
{
    private static readonly HashSet<string> Reported = new(StringComparer.OrdinalIgnoreCase);

    private readonly string _path;
    private readonly PackedScene _previous;
    private readonly bool _applied;

    private RemoteRoomSceneLease(string path, PackedScene previous, bool applied)
    {
        _path = path;
        _previous = previous;
        _applied = applied;
    }

    public static RemoteRoomSceneLease? Begin(Player? player, string? scenePath, string kind)
    {
        if (player == null || string.IsNullOrWhiteSpace(scenePath))
        {
            return null;
        }

        try
        {
            var groupId = Ui.ContextualSkinControls.FindGroup(
                player.Character.Id.Entry,
                player.Character.GetType().Name)?.Id;
            if (groupId == null)
            {
                return null;
            }

            var selection = SkinService.GetDisplayedSelection(groupId);
            if (string.IsNullOrWhiteSpace(selection) ||
                selection.Equals(Catalog.SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            PackedScene previous;
            try
            {
                previous = PreloadManager.Cache.GetScene(scenePath);
            }
            catch
            {
                previous = null!;
            }

            var resolved = SkinService.GetOrLoadRuntimeScene(groupId, scenePath);
            if (resolved == null || ReferenceEquals(resolved, previous))
            {
                return null;
            }

            GlobalResourceWrites.SetPreloadAsset(GlobalWriteSite.A25, PreloadManager.Cache, scenePath, resolved);
            if (Reported.Add(groupId + "\n" + scenePath))
            {
                ModLog.Info(
                    $"联机远端{kind}场景已切换：“{groupId}”的玩家使用 {selection}，" +
                    $"房间场景取皮肤资源而不是预加载的原版（{scenePath}）。");
            }

            return new RemoteRoomSceneLease(scenePath, previous, applied: true);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"联机远端{kind}场景切换失败：" + exception.GetBaseException().Message);
            return null;
        }
    }

    public void Dispose()
    {
        if (!_applied)
        {
            return;
        }

        try
        {
            if (_previous != null && GodotObject.IsInstanceValid(_previous))
            {
                GlobalResourceWrites.SetPreloadAsset(GlobalWriteSite.A26, PreloadManager.Cache, _path, _previous, restoring: true);
            }
        }
        catch
        {
            // Restoring the world's cache is best effort; the node already has its own scene.
        }
    }

    /// <summary>
    /// Same trick for a canonical <b>texture</b> the provider overrides (orb icons, card frames,
    /// UI art): serve the owner's skin texture for this one lookup, then restore the shared entry.
    /// </summary>
    internal static IDisposable? BeginTexture(Player? player, string? texturePath, string kind)
    {
        if (player == null || string.IsNullOrWhiteSpace(texturePath))
        {
            return null;
        }

        try
        {
            var groupId = Ui.ContextualSkinControls.FindGroup(
                player.Character.Id.Entry,
                player.Character.GetType().Name)?.Id;
            if (groupId == null)
            {
                return null;
            }

            var selection = SkinService.GetDisplayedSelection(groupId);
            if (string.IsNullOrWhiteSpace(selection) ||
                selection.Equals(Catalog.SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Resource? previous = null;
            try
            {
                previous = PreloadManager.Cache.GetAsset<Resource>(texturePath);
            }
            catch
            {
                // Not cached yet is fine.
            }

            var resolved = SkinService.GetOrLoadRuntimeResource(groupId, texturePath);
            if (resolved == null || ReferenceEquals(resolved, previous))
            {
                return null;
            }

            GlobalResourceWrites.SetPreloadAsset(GlobalWriteSite.A27, PreloadManager.Cache, texturePath, resolved);
            if (Reported.Add(groupId + "\n" + texturePath))
            {
                ModLog.Info(
                    $"联机远端{kind}已切换：“{groupId}”的玩家使用 {selection}，" +
                    $"该资源取皮肤包而不是原版（{texturePath}）。");
            }

            return new RemoteTextureLease(texturePath, previous as Texture2D);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"联机远端{kind}切换失败：" + exception.GetBaseException().Message);
            return null;
        }
    }

    private sealed class RemoteTextureLease(string path, Texture2D? previous) : IDisposable
    {
        public void Dispose()
        {
            try
            {
                if (previous != null && GodotObject.IsInstanceValid(previous))
                {
                    GlobalResourceWrites.SetPreloadAsset(GlobalWriteSite.A28, PreloadManager.Cache, path, previous, restoring: true);
                }
            }
            catch
            {
                // Best effort.
            }
        }
    }
}

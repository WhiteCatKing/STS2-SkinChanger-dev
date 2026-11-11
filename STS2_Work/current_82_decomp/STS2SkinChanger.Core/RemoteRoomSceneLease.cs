using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal sealed class RemoteRoomSceneLease : IDisposable
{
	private sealed class RemoteTextureLease(string path, Texture2D? previous) : IDisposable
	{
		public void Dispose()
		{
			try
			{
				if (previous != null && GodotObject.IsInstanceValid((GodotObject)(object)previous))
				{
					PreloadManager.Cache.SetAsset(path, (Resource)(object)previous);
				}
			}
			catch
			{
			}
		}
	}

	private static readonly HashSet<string> Reported = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
			string text = ContextualSkinControls.FindGroup(((AbstractModel)player.Character).Id.Entry, ((object)player.Character).GetType().Name)?.Id;
			if (text == null)
			{
				return null;
			}
			string displayedSelection = SkinService.GetDisplayedSelection(text);
			if (string.IsNullOrWhiteSpace(displayedSelection) || displayedSelection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			PackedScene val;
			try
			{
				val = PreloadManager.Cache.GetScene(scenePath);
			}
			catch
			{
				val = null;
			}
			PackedScene orLoadRuntimeScene = SkinService.GetOrLoadRuntimeScene(text, scenePath);
			if (orLoadRuntimeScene == null || orLoadRuntimeScene == val)
			{
				return null;
			}
			PreloadManager.Cache.SetAsset(scenePath, (Resource)(object)orLoadRuntimeScene);
			if (Reported.Add(text + "\n" + scenePath))
			{
				ModLog.Info($"联机远端{kind}场景已切换：“{text}”的玩家使用 {displayedSelection}，房间场景取皮肤资源而不是预加载的原版（{scenePath}）。");
			}
			return new RemoteRoomSceneLease(scenePath, val, applied: true);
		}
		catch (Exception ex)
		{
			ModLog.Warn("联机远端" + kind + "场景切换失败：" + ex.GetBaseException().Message);
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
			if (_previous != null && GodotObject.IsInstanceValid((GodotObject)(object)_previous))
			{
				PreloadManager.Cache.SetAsset(_path, (Resource)(object)_previous);
			}
		}
		catch
		{
		}
	}

	internal static IDisposable? BeginTexture(Player? player, string? texturePath, string kind)
	{
		if (player == null || string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}
		try
		{
			string text = ContextualSkinControls.FindGroup(((AbstractModel)player.Character).Id.Entry, ((object)player.Character).GetType().Name)?.Id;
			if (text == null)
			{
				return null;
			}
			string displayedSelection = SkinService.GetDisplayedSelection(text);
			if (string.IsNullOrWhiteSpace(displayedSelection) || displayedSelection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			Resource val = null;
			try
			{
				val = PreloadManager.Cache.GetAsset<Resource>(texturePath);
			}
			catch
			{
			}
			Resource orLoadRuntimeResource = SkinService.GetOrLoadRuntimeResource(text, texturePath);
			if (orLoadRuntimeResource == null || orLoadRuntimeResource == val)
			{
				return null;
			}
			PreloadManager.Cache.SetAsset(texturePath, orLoadRuntimeResource);
			if (Reported.Add(text + "\n" + texturePath))
			{
				ModLog.Info($"联机远端{kind}已切换：“{text}”的玩家使用 {displayedSelection}，该资源取皮肤包而不是原版（{texturePath}）。");
			}
			return new RemoteTextureLease(texturePath, (Texture2D?)(object)((val is Texture2D) ? val : null));
		}
		catch (Exception ex)
		{
			ModLog.Warn("联机远端" + kind + "切换失败：" + ex.GetBaseException().Message);
			return null;
		}
	}
}

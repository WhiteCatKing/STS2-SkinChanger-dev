using System;
using Godot;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace ModLaunchManager;

internal static class SteamOverlay
{
	private const ulong CheckIntervalMsec = 30uL;

	private const ulong SettleMsec = 3000uL;

	private static SceneTree? _tree;

	private static Rid _frameRequest;

	private static ulong _drawUntil;

	private static ulong _nextCheck;

	internal static Func<string, Error> OpenInBrowser { get; set; } = OS.ShellOpen;

	internal static PageOpenResult OpenPage(string url)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (!WorkshopUpdates.IsAvailable)
		{
			if ((int)OpenInBrowser(url) != 0)
			{
				return PageOpenResult.Failed;
			}
			return PageOpenResult.InBrowser;
		}
		bool num = SteamUtils.IsOverlayEnabled();
		SteamFriends.ActivateGameOverlayToWebPage(url, (EActivateGameOverlayToWebPageMode)0);
		_drawUntil = Time.GetTicksMsec() + 3000;
		if (!num)
		{
			return PageOpenResult.InSteam;
		}
		return PageOpenResult.InGame;
	}

	internal static void KeepDrawn()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (_tree != null || !SteamInitializer.Initialized)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		try
		{
			_frameRequest = RenderingServer.CanvasItemCreate();
			_nextCheck = 0uL;
			_tree = val;
			val.ProcessFrame += RequestFrames;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not keep Steam's overlay drawn: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void StopKeepingDrawn()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		if (_tree != null)
		{
			_tree.ProcessFrame -= RequestFrames;
			_tree = null;
			if (((Rid)(ref _frameRequest)).IsValid)
			{
				RenderingServer.FreeRid(_frameRequest);
				_frameRequest = default(Rid);
			}
		}
	}

	private static void RequestFrames()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		if (!SteamInitializer.Initialized)
		{
			return;
		}
		try
		{
			ulong ticksMsec = Time.GetTicksMsec();
			if (PlatformUtil.IsPlatformOverlayOpen())
			{
				_drawUntil = ticksMsec + 3000;
			}
			bool flag = ticksMsec < _drawUntil;
			if (!flag && ticksMsec >= _nextCheck)
			{
				_nextCheck = ticksMsec + 30;
				flag = SteamUtils.BOverlayNeedsPresent();
			}
			if (flag)
			{
				RenderingServer.CanvasItemClear(_frameRequest);
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} stopped keeping Steam's overlay drawn: {ex.GetType().Name} {ex.Message}");
			StopKeepingDrawn();
		}
	}
}

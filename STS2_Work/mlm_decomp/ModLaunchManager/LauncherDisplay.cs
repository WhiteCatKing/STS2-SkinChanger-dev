using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace ModLaunchManager;

internal static class LauncherDisplay
{
	private static bool _oversamplingChanged;

	private static bool _previousOversampling;

	private static bool _awaitingReady;

	private static bool _redrawChanged;

	private static bool _previousLowProcessor;

	private static int _previousSleepUsec;

	private static readonly Vector2 DesignSize = new Vector2(1920f, 1080f);

	private static readonly Vector2 MinimumLayout = new Vector2(1280f, 720f);

	private const float ReadableShare = 0.9f;

	private static Window? _fitWindow;

	private static Control? _fitLayout;

	private static Vector2I _fittedWindowSize;

	private static bool _refitQueued;

	private static (ContentScaleAspectEnum Aspect, Vector2I Size, float Factor) _gameScale;

	private static (ContentScaleAspectEnum Aspect, Vector2I Size, float Factor) _launcherScale;

	private static bool _gameVertexSnap;

	private static bool _launcherVertexSnap;

	private static bool _steamNoticesMoved;

	internal static float? DpiOverride { get; set; }

	internal static void ApplySavedSettings()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		NGame instance = NGame.Instance;
		if (instance == null)
		{
			GD.PushWarning("[ModLaunchManager] no NGame instance; the launcher keeps the engine's boot resolution.");
		}
		else if (((Node)instance).IsNodeReady())
		{
			Apply(instance);
		}
		else if (!_awaitingReady)
		{
			_awaitingReady = true;
			((GodotObject)instance).Connect(SignalName.Ready, Callable.From((Action)OnGameReady), 4u);
		}
	}

	private static void OnGameReady()
	{
		_awaitingReady = false;
		NGame instance = NGame.Instance;
		if (instance != null && GodotObject.IsInstanceValid((GodotObject)(object)instance))
		{
			Apply(instance);
		}
	}

	private static void Apply(NGame game)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			game.ApplyDisplaySettings();
			GD.Print($"{"[ModLaunchManager]"} applied the saved display settings before the launcher; window is {DisplayServer.WindowGetSize(0)}.");
			if (_fitWindow != null && GodotObject.IsInstanceValid((GodotObject)(object)_fitWindow))
			{
				FitLauncher(_fitWindow);
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not apply display settings early: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void FitLauncher(Window root, Control? layout = null)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I8
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if ((long)root.ContentScaleMode != 1)
			{
				return;
			}
			Vector2 val = Vector2I.op_Implicit(root.Size);
			if (val.X < 1f || val.Y < 1f)
			{
				return;
			}
			_fittedWindowSize = root.Size;
			if (_fitWindow == null)
			{
				_fitWindow = root;
				_gameScale = (Aspect: root.ContentScaleAspect, Size: root.ContentScaleSize, Factor: root.ContentScaleFactor);
				_gameVertexSnap = ((Viewport)root).Snap2DVerticesToPixel;
				_launcherVertexSnap = _gameVertexSnap;
				((Viewport)root).SizeChanged += OnRootSizeChanged;
			}
			else
			{
				if (root.ContentScaleAspect != _launcherScale.Aspect)
				{
					_gameScale.Aspect = root.ContentScaleAspect;
				}
				if (root.ContentScaleSize != _launcherScale.Size)
				{
					_gameScale.Size = root.ContentScaleSize;
				}
				if (!Mathf.IsEqualApprox(root.ContentScaleFactor, _launcherScale.Factor))
				{
					_gameScale.Factor = root.ContentScaleFactor;
				}
				if (((Viewport)root).Snap2DVerticesToPixel != _launcherVertexSnap)
				{
					_gameVertexSnap = ((Viewport)root).Snap2DVerticesToPixel;
				}
			}
			if (layout != null && layout != _fitLayout)
			{
				WatchLayout(_fitLayout, watch: false);
				_fitLayout = layout;
				WatchLayout(layout, watch: true);
			}
			Vector2 layoutMinimum = LayoutMinimum(_fitLayout);
			float num = LauncherScale(val, ScreenScaling(root), layoutMinimum);
			_launcherScale = (Aspect: (ContentScaleAspectEnum)4, Size: root.Size, Factor: num);
			if (root.ContentScaleAspect != _launcherScale.Aspect)
			{
				root.ContentScaleAspect = _launcherScale.Aspect;
			}
			if (root.ContentScaleSize != _launcherScale.Size)
			{
				root.ContentScaleSize = _launcherScale.Size;
			}
			if (!Mathf.IsEqualApprox(root.ContentScaleFactor, _launcherScale.Factor))
			{
				root.ContentScaleFactor = _launcherScale.Factor;
			}
			_launcherVertexSnap = Math.Abs(num - MathF.Round(num)) > 0.001f || _gameVertexSnap;
			if (((Viewport)root).Snap2DVerticesToPixel != _launcherVertexSnap)
			{
				((Viewport)root).Snap2DVerticesToPixel = _launcherVertexSnap;
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not fit the launcher to the window: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static float LauncherScale(Vector2 window, float screenScaling, Vector2 layoutMinimum = default(Vector2))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Min(window.X / DesignSize.X, window.Y / DesignSize.Y);
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(Math.Max(MinimumLayout.X, layoutMinimum.X), Math.Max(MinimumLayout.Y, layoutMinimum.Y));
		float num2 = Math.Min(window.X / val.X, window.Y / val.Y);
		float num3 = Math.Max(num, Math.Min(0.9f * screenScaling, num2));
		float num4 = MathF.Round(num3);
		if (num3 < 1f && num3 >= 0.85f && num2 >= 1f)
		{
			num3 = 1f;
		}
		else if (num4 >= 1f && Math.Abs(num3 - num4) <= 0.06f && num4 <= Math.Max(num2, num))
		{
			num3 = num4;
		}
		return num3;
	}

	private static float ScreenScaling(Window root)
	{
		float? dpiOverride = DpiOverride;
		if (dpiOverride.HasValue)
		{
			float valueOrDefault = dpiOverride.GetValueOrDefault();
			return Math.Clamp(valueOrDefault / 96f, 1f, 3f);
		}
		int currentScreen = root.CurrentScreen;
		if (OS.GetName() == "macOS")
		{
			return Math.Clamp(DisplayServer.ScreenGetScale(currentScreen), 1f, 3f);
		}
		int num = DisplayServer.ScreenGetDpi(currentScreen);
		if (num <= 0)
		{
			return 1f;
		}
		return Math.Clamp((float)num / 96f, 1f, 3f);
	}

	private static Vector2 LayoutMinimum(Control? layout)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		Vector2 result = Vector2.Zero;
		if (layout == null || !GodotObject.IsInstanceValid((GodotObject)(object)layout))
		{
			return result;
		}
		Vector2 val = default(Vector2);
		foreach (Control item in LayoutPages(layout))
		{
			((Vector2)(ref val))._002Ector(Math.Clamp(item.AnchorRight - item.AnchorLeft, 0.05f, 1f), Math.Clamp(item.AnchorBottom - item.AnchorTop, 0.05f, 1f));
			Vector2 val2 = item.GetCombinedMinimumSize() / val;
			if (val2.X > DesignSize.X)
			{
				val2.X = 0f;
			}
			if (val2.Y > DesignSize.Y)
			{
				val2.Y = 0f;
			}
			result = ((Vector2)(ref result)).Max(val2);
		}
		return result;
	}

	private static IEnumerable<Control> LayoutPages(Control layout)
	{
		if (!(layout is Container))
		{
			return ((IEnumerable)((Node)layout).GetChildren(false)).OfType<Control>().SelectMany((Control child) => (IEnumerable<Control>)((!(child is Container)) ? ((IEnumerable)((IEnumerable)((Node)child).GetChildren(false)).OfType<Container>()) : ((IEnumerable)new _003C_003Ez__ReadOnlySingleElementList<Control>(child))));
		}
		return new _003C_003Ez__ReadOnlySingleElementList<Control>(layout);
	}

	private static void WatchLayout(Control? layout, bool watch)
	{
		if (layout == null || !GodotObject.IsInstanceValid((GodotObject)(object)layout))
		{
			return;
		}
		foreach (Control item in LayoutPages(layout))
		{
			if (watch)
			{
				item.MinimumSizeChanged += RefitIfOverflowing;
			}
			else
			{
				item.MinimumSizeChanged -= RefitIfOverflowing;
			}
		}
	}

	private static void RefitIfOverflowing()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (_fitWindow != null && GodotObject.IsInstanceValid((GodotObject)(object)_fitWindow))
		{
			Rect2 visibleRect = ((Viewport)_fitWindow).GetVisibleRect();
			Vector2 size = ((Rect2)(ref visibleRect)).Size;
			Vector2 val = LayoutMinimum(_fitLayout);
			if (val.X > size.X + 0.5f || val.Y > size.Y + 0.5f)
			{
				QueueRefit();
			}
		}
	}

	private static void OnRootSizeChanged()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (_fitWindow != null && GodotObject.IsInstanceValid((GodotObject)(object)_fitWindow) && _fitWindow.Size != _fittedWindowSize)
		{
			QueueRefit();
		}
	}

	private static void QueueRefit()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (_refitQueued)
		{
			return;
		}
		_refitQueued = true;
		Callable val = Callable.From((Action)delegate
		{
			_refitQueued = false;
			if (_fitWindow != null && GodotObject.IsInstanceValid((GodotObject)(object)_fitWindow))
			{
				FitLauncher(_fitWindow);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	internal static void RestoreFit()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Window fitWindow = _fitWindow;
		Control? fitLayout = _fitLayout;
		_fitWindow = null;
		_fitLayout = null;
		WatchLayout(fitLayout, watch: false);
		if (fitWindow == null || !GodotObject.IsInstanceValid((GodotObject)(object)fitWindow))
		{
			return;
		}
		try
		{
			((Viewport)fitWindow).SizeChanged -= OnRootSizeChanged;
			if (fitWindow.ContentScaleAspect == _launcherScale.Aspect)
			{
				fitWindow.ContentScaleAspect = _gameScale.Aspect;
			}
			if (fitWindow.ContentScaleSize == _launcherScale.Size)
			{
				fitWindow.ContentScaleSize = _gameScale.Size;
			}
			if (Mathf.IsEqualApprox(fitWindow.ContentScaleFactor, _launcherScale.Factor))
			{
				fitWindow.ContentScaleFactor = _gameScale.Factor;
			}
			if (((Viewport)fitWindow).Snap2DVerticesToPixel == _launcherVertexSnap)
			{
				((Viewport)fitWindow).Snap2DVerticesToPixel = _gameVertexSnap;
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not restore the game's content scale: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void SharpenFonts(SceneTree tree)
	{
		if (_oversamplingChanged)
		{
			return;
		}
		try
		{
			Window root = tree.Root;
			_previousOversampling = ((Viewport)root).Oversampling;
			if (!_previousOversampling)
			{
				((Viewport)root).Oversampling = true;
				_oversamplingChanged = true;
				GD.Print($"{"[ModLaunchManager]"} launcher font oversampling enabled (factor {((Viewport)root).GetOversampling():F2}).");
			}
		}
		catch (Exception ex)
		{
			_oversamplingChanged = false;
			GD.PushWarning($"{"[ModLaunchManager]"} could not enable font oversampling: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void MoveSteamNotices()
	{
		if (_steamNoticesMoved || !SteamInitializer.Initialized)
		{
			return;
		}
		try
		{
			SetSteamNoticePosition(topLeft: true);
			_steamNoticesMoved = true;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not move Steam notifications: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void RestoreSteamNotices()
	{
		if (!_steamNoticesMoved)
		{
			return;
		}
		_steamNoticesMoved = false;
		try
		{
			SetSteamNoticePosition(topLeft: false);
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not restore Steam notifications: {ex.GetType().Name} {ex.Message}");
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void SetSteamNoticePosition(bool topLeft)
	{
		SteamUtils.SetOverlayNotificationPosition((ENotificationPosition)((!topLeft) ? 3 : 0));
		int num = (topLeft ? (-10000) : 0);
		SteamUtils.SetOverlayNotificationInset(num, num);
	}

	internal static void RestoreFonts(SceneTree? tree)
	{
		if (!_oversamplingChanged)
		{
			return;
		}
		_oversamplingChanged = false;
		try
		{
			if (tree != null)
			{
				((Viewport)tree.Root).Oversampling = _previousOversampling;
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not restore font oversampling: {ex.GetType().Name} {ex.Message}");
		}
	}

	internal static void RedrawOnlyChanges()
	{
		if (_redrawChanged)
		{
			return;
		}
		try
		{
			_previousLowProcessor = OS.LowProcessorUsageMode;
			_previousSleepUsec = OS.LowProcessorUsageModeSleepUsec;
			_redrawChanged = true;
			float num = DisplayServer.ScreenGetRefreshRate(DisplayServer.WindowGetCurrentScreen(0));
			if (num >= 30f)
			{
				OS.LowProcessorUsageModeSleepUsec = (int)(1000000f / Math.Min(num, 240f));
			}
			OS.LowProcessorUsageMode = true;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not limit launcher redraws: {ex.GetType().Name} {ex.Message}");
		}
		SteamOverlay.KeepDrawn();
	}

	internal static void RestoreRedraw()
	{
		if (!_redrawChanged)
		{
			return;
		}
		_redrawChanged = false;
		SteamOverlay.StopKeepingDrawn();
		try
		{
			OS.LowProcessorUsageMode = _previousLowProcessor;
			OS.LowProcessorUsageModeSleepUsec = _previousSleepUsec;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not restore the game's redraw mode: {ex.GetType().Name} {ex.Message}");
		}
	}
}

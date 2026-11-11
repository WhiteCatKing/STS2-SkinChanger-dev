using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace CznCore.Presentation;

internal sealed class CombatVideoCutInOverlayRuntime<TActor> where TActor : class
{
	private readonly IReadOnlyDictionary<string, CombatVideoCutInProfile> _profiles;

	private readonly Func<bool> _isEnabled;

	private readonly Func<TActor?, bool> _isEnabledFor;

	private readonly Func<TActor?, int, bool> _isTokenCurrent;

	private readonly Func<string, string> _runtimeNodeName;

	private readonly Action<string, int> _warn;

	private readonly Action<string, int>? _info;

	private readonly Action<TActor?, int, float, Action> _scheduleDelay;

	private readonly Func<Viewport?> _getViewport;

	private readonly string _logPrefix;

	private readonly Dictionary<string, VideoStreamTheora> _videoCache = new Dictionary<string, VideoStreamTheora>(StringComparer.OrdinalIgnoreCase);

	private CanvasLayer? _layer;

	private ColorRect? _dim;

	private VideoStreamPlayer? _video;

	private Tween? _introTween;

	private Tween? _outroTween;

	private int _generation;

	public CombatVideoCutInOverlayRuntime(IReadOnlyDictionary<string, CombatVideoCutInProfile> profiles, ICombatVideoCutInOverlayHost<TActor> host, Action<TActor?, int, float, Action>? scheduleDelay = null, Func<Viewport?>? getViewport = null, string logPrefix = "Combat video cut-in")
	{
		_profiles = profiles ?? throw new ArgumentNullException("profiles");
		ArgumentNullException.ThrowIfNull(host, "host");
		_isEnabled = host.IsEnabled;
		_isEnabledFor = host.IsEnabledFor;
		_isTokenCurrent = host.IsTokenCurrent;
		_runtimeNodeName = host.RuntimeNodeName;
		_warn = host.Warn;
		_info = host.Info;
		_scheduleDelay = scheduleDelay ?? new Action<TActor, int, float, Action>(ScheduleDefaultDelay);
		_getViewport = getViewport ?? new Func<Viewport>(ResolveDefaultOverlayViewport);
		_logPrefix = (string.IsNullOrWhiteSpace(logPrefix) ? "Combat video cut-in" : logPrefix);
	}

	public bool HasProfile(string? stateId)
	{
		CombatVideoCutInProfile profile;
		return TryGetProfile(stateId, out profile);
	}

	public void Preload(string? stateId)
	{
		try
		{
			if (_isEnabled() && TryGetProfile(stateId, out CombatVideoCutInProfile profile))
			{
				TryGetVideoStream(profile.VideoPath, out VideoStreamTheora _);
			}
		}
		catch (Exception ex)
		{
			_warn(_logPrefix + " preload failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	public void TryPlay(TActor? actor, string? stateId, int token)
	{
		try
		{
			if (!_isEnabled() || !_isEnabledFor(actor) || !TryGetProfile(stateId, out CombatVideoCutInProfile profile) || !_isTokenCurrent(actor, token))
			{
				return;
			}
			if (profile.StartDelaySeconds > 0f)
			{
				_scheduleDelay(actor, token, profile.StartDelaySeconds, delegate
				{
					TryPlayNow(actor, profile, token);
				});
				_info?.Invoke(_logPrefix + " scheduled state=" + profile.StateId + " delay=" + profile.StartDelaySeconds.ToString("0.###"), 2);
			}
			else
			{
				TryPlayNow(actor, profile, token);
			}
		}
		catch (Exception ex)
		{
			_warn(_logPrefix + " failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	public void Cleanup(bool releaseNodes)
	{
		try
		{
			if (_layer == null || !GodotObject.IsInstanceValid((GodotObject)(object)_layer))
			{
				ResetNodeCache();
			}
			else if (releaseNodes)
			{
				_generation++;
				KillTween(ref _introTween);
				KillTween(ref _outroTween);
				if (GodotObject.IsInstanceValid((GodotObject)(object)_video))
				{
					_video.Stop();
				}
				((Node)_layer).QueueFree();
				ResetNodeCache();
			}
			else
			{
				Release(_layer, null);
			}
		}
		catch (Exception ex)
		{
			_warn(_logPrefix + " cleanup failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	public void ClearVideoCache()
	{
		_videoCache.Clear();
	}

	private static void ScheduleDefaultDelay(TActor? actor, int token, float seconds, Action action)
	{
		GodotScheduler.Delay(Math.Max(0.05f, seconds), action);
	}

	private static Viewport? ResolveDefaultOverlayViewport()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val.Root))
		{
			return (Viewport?)(object)val.Root;
		}
		NCombatRoom instance = NCombatRoom.Instance;
		Viewport val2 = ((instance != null) ? ((Node)instance).GetViewport() : null);
		if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
		{
			return null;
		}
		return val2;
	}

	private bool TryGetProfile(string? stateId, out CombatVideoCutInProfile profile)
	{
		if (!string.IsNullOrWhiteSpace(stateId) && _profiles.TryGetValue(stateId, out profile))
		{
			return true;
		}
		profile = null;
		return false;
	}

	private void TryPlayNow(TActor? actor, CombatVideoCutInProfile profile, int token)
	{
		try
		{
			if (_isEnabled() && _isEnabledFor(actor) && _isTokenCurrent(actor, token))
			{
				Viewport val = _getViewport();
				VideoStreamTheora stream;
				if (val == null)
				{
					_warn(_logPrefix + " skipped: no viewport.", 2);
				}
				else if (TryGetVideoStream(profile.VideoPath, out stream))
				{
					PlayVideoOverlay(actor, token, val, stream, profile);
					_info?.Invoke(_logPrefix + " queued state=" + profile.StateId + " video=" + profile.VideoPath, 2);
				}
			}
		}
		catch (Exception ex)
		{
			_warn(_logPrefix + " playback failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	private bool TryGetVideoStream(string videoPath, [NotNullWhen(true)] out VideoStreamTheora? stream)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		stream = null;
		try
		{
			if (_videoCache.TryGetValue(videoPath, out VideoStreamTheora value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				stream = value;
				return true;
			}
			if (!FileAccess.FileExists(videoPath) && !ResourceLoader.Exists(videoPath, ""))
			{
				_warn(_logPrefix + " video missing: " + videoPath, 2);
				return false;
			}
			stream = new VideoStreamTheora
			{
				File = videoPath
			};
			_videoCache[videoPath] = stream;
			return true;
		}
		catch (Exception ex)
		{
			_warn(_logPrefix + " video load failed: " + SkinLog.FormatException(ex), 2);
			return false;
		}
	}

	private void PlayVideoOverlay(TActor? actor, int token, Viewport viewport, VideoStreamTheora stream, CombatVideoCutInProfile profile)
	{
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		if (_isTokenCurrent(actor, token) && TryAcquire(viewport, stream, profile, out CanvasLayer layer, out ColorRect dim, out VideoStreamPlayer video))
		{
			int overlayGeneration = ++_generation;
			if (profile.FadeInSeconds > 0f)
			{
				_introTween = ((Node)layer).CreateTween();
				_introTween.SetParallel(true);
				_introTween.SetEase((EaseType)1);
				_introTween.SetTrans((TransitionType)7);
				_introTween.TweenProperty((GodotObject)(object)dim, NodePath.op_Implicit("color"), Variant.op_Implicit(new Color(0f, 0f, 0f, profile.DimAlpha)), (double)profile.FadeInSeconds);
				_introTween.TweenProperty((GodotObject)(object)video, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), (double)profile.FadeInSeconds);
			}
			else
			{
				dim.Color = new Color(0f, 0f, 0f, profile.DimAlpha);
				((CanvasItem)video).Modulate = Colors.White;
			}
			video.Play();
			GodotScheduler.OneShot((Node)(object)layer, Math.Max(0.001f, profile.FadeOutDelaySeconds), delegate
			{
				FadeOut(actor, token, overlayGeneration, layer, dim, video, profile);
			});
			GodotScheduler.OneShot((Node)(object)layer, Math.Max(0.05f, profile.LifetimeSeconds + profile.FadeOutSeconds + 0.05f), delegate
			{
				Release(layer, overlayGeneration);
			});
		}
	}

	private bool TryAcquire(Viewport viewport, VideoStreamTheora stream, CombatVideoCutInProfile profile, out CanvasLayer layer, out ColorRect dim, out VideoStreamPlayer video)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Expected O, but got Unknown
		if (!TryGetNodes(out layer, out dim, out video))
		{
			layer = new CanvasLayer
			{
				Name = StringName.op_Implicit(_runtimeNodeName("CombatVideoCutInLayer")),
				Layer = profile.CanvasLayer
			};
			dim = new ColorRect
			{
				Name = StringName.op_Implicit(_runtimeNodeName("CombatVideoCutInDim"))
			};
			video = new VideoStreamPlayer
			{
				Name = StringName.op_Implicit(_runtimeNodeName("CombatVideoCutIn"))
			};
			((Node)layer).AddChild((Node)(object)dim, false, (InternalMode)0);
			((Node)layer).AddChild((Node)(object)video, false, (InternalMode)0);
			_layer = layer;
			_dim = dim;
			_video = video;
		}
		KillTween(ref _introTween);
		KillTween(ref _outroTween);
		Configure(viewport, stream, profile, layer, dim, video);
		AttachLayer(viewport, layer);
		return true;
	}

	private bool TryGetNodes(out CanvasLayer layer, out ColorRect dim, out VideoStreamPlayer video)
	{
		layer = _layer;
		dim = _dim;
		video = _video;
		if (_layer == null || _dim == null || _video == null)
		{
			return false;
		}
		layer = _layer;
		dim = _dim;
		video = _video;
		if (GodotObject.IsInstanceValid((GodotObject)(object)layer) && GodotObject.IsInstanceValid((GodotObject)(object)dim))
		{
			return GodotObject.IsInstanceValid((GodotObject)(object)video);
		}
		return false;
	}

	private void Configure(Viewport viewport, VideoStreamTheora stream, CombatVideoCutInProfile profile, CanvasLayer layer, ColorRect dim, VideoStreamPlayer video)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		Rect2 visibleRect = viewport.GetVisibleRect();
		Vector2 size = ((Rect2)(ref visibleRect)).Size;
		Vector2 val = size / 2f;
		Vector2 sourceSize = profile.SourceSize;
		Vector2 val2 = ComputeCoverSize(size, sourceSize);
		((Node)layer).Name = StringName.op_Implicit(_runtimeNodeName("CombatVideoCutInLayer"));
		layer.Layer = profile.CanvasLayer;
		layer.Visible = true;
		((Node)dim).Name = StringName.op_Implicit(_runtimeNodeName("CombatVideoCutInDim"));
		dim.Color = new Color(0f, 0f, 0f, 0f);
		((Control)dim).Size = size;
		((Control)dim).MouseFilter = (MouseFilterEnum)2;
		((CanvasItem)dim).ZIndex = 0;
		((CanvasItem)dim).ZAsRelative = false;
		((CanvasItem)dim).Visible = true;
		video.Stop();
		((Node)video).Name = StringName.op_Implicit(_runtimeNodeName("CombatVideoCutIn"));
		video.Stream = (VideoStream)(object)stream;
		video.Expand = true;
		video.Loop = false;
		video.Autoplay = false;
		((Control)video).MouseFilter = (MouseFilterEnum)2;
		((Control)video).Position = val - val2 / 2f;
		((Control)video).Size = val2;
		((CanvasItem)video).Modulate = new Color(1f, 1f, 1f, 0f);
		((CanvasItem)video).ZIndex = 10;
		((CanvasItem)video).ZAsRelative = false;
		video.VolumeDb = -80f;
		((CanvasItem)video).Visible = true;
	}

	private static Vector2 ComputeCoverSize(Vector2 containerSize, Vector2 contentSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (contentSize.X <= 0f || contentSize.Y <= 0f)
		{
			return containerSize;
		}
		float num = MathF.Max(containerSize.X / contentSize.X, containerSize.Y / contentSize.Y);
		return contentSize * num;
	}

	private void FadeOut(TActor? actor, int token, int overlayGeneration, CanvasLayer layer, ColorRect dim, VideoStreamPlayer video, CombatVideoCutInProfile profile)
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)layer))
		{
			return;
		}
		if (!_isTokenCurrent(actor, token))
		{
			Release(layer, overlayGeneration);
		}
		else if (overlayGeneration == _generation)
		{
			KillTween(ref _introTween);
			KillTween(ref _outroTween);
			if (profile.FadeOutSeconds > 0f)
			{
				_outroTween = ((Node)layer).CreateTween();
				_outroTween.SetParallel(true);
				_outroTween.SetEase((EaseType)0);
				_outroTween.SetTrans((TransitionType)1);
				_outroTween.TweenProperty((GodotObject)(object)dim, NodePath.op_Implicit("color"), Variant.op_Implicit(new Color(0f, 0f, 0f, 0f)), (double)profile.FadeOutSeconds);
				_outroTween.TweenProperty((GodotObject)(object)video, NodePath.op_Implicit("modulate"), Variant.op_Implicit(new Color(1f, 1f, 1f, 0f)), (double)profile.FadeOutSeconds);
			}
			else
			{
				dim.Color = new Color(0f, 0f, 0f, 0f);
				((CanvasItem)video).Modulate = new Color(1f, 1f, 1f, 0f);
			}
		}
	}

	private static void AttachLayer(Viewport viewport, CanvasLayer layer)
	{
		Node parent = ((Node)layer).GetParent();
		if (parent != null)
		{
			parent.RemoveChild((Node)(object)layer);
		}
		((Node)viewport).AddChild((Node)(object)layer, false, (InternalMode)0);
	}

	private static void KillTween(ref Tween? tween)
	{
		try
		{
			if (tween != null && GodotObject.IsInstanceValid((GodotObject)(object)tween))
			{
				tween.Kill();
			}
		}
		catch
		{
		}
		tween = null;
	}

	private void Release(CanvasLayer layer, int? expectedGeneration = null)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)layer) && (!expectedGeneration.HasValue || expectedGeneration.Value == _generation))
		{
			_generation++;
			KillTween(ref _introTween);
			KillTween(ref _outroTween);
			if (GodotObject.IsInstanceValid((GodotObject)(object)_video))
			{
				_video.Stop();
				_video.Stream = null;
				((CanvasItem)_video).Modulate = new Color(1f, 1f, 1f, 0f);
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)_dim))
			{
				_dim.Color = new Color(0f, 0f, 0f, 0f);
			}
			layer.Visible = false;
			Node parent = ((Node)layer).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)layer);
			}
		}
	}

	private void ResetNodeCache()
	{
		_layer = null;
		_dim = null;
		_video = null;
		_introTween = null;
		_outroTween = null;
	}
}

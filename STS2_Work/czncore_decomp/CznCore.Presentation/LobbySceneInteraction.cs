using System;
using Godot;

namespace CznCore.Presentation;

public sealed class LobbySceneInteraction : IDisposable
{
	private readonly Control _scene;

	private readonly Action _stop;

	private AudioStreamPlayerRuntime? _audio;

	private Func<float>? _volume;

	private bool _disposed;

	private LobbySceneInteraction(Control scene, Func<string, float> play, Action stop)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		_scene = scene;
		_stop = stop;
		((GodotObject)scene).Set(StringName.op_Implicit("play_voice"), Variant.op_Implicit(Callable.From<string, float>(play)));
		((GodotObject)scene).Set(StringName.op_Implicit("stop_voice"), Variant.op_Implicit(Callable.From(stop)));
		scene.MouseFilter = (MouseFilterEnum)1;
		((CanvasItem)scene).VisibilityChanged += OnVisibilityChanged;
		((Node)scene).TreeExiting += Dispose;
	}

	public static bool Supports(Node scene)
	{
		return ((GodotObject)scene).HasMethod(StringName.op_Implicit("try_interact"));
	}

	public static LobbySceneInteraction? Bind(Control scene, Func<string, float> play, Action stop)
	{
		if (!Supports((Node)(object)scene))
		{
			return null;
		}
		return new LobbySceneInteraction(scene, play, stop);
	}

	public static LobbySceneInteraction? Attach(Control scene, Func<float> volume)
	{
		if (!Supports((Node)(object)scene))
		{
			return null;
		}
		Action<string, int> warn = SkinLog.Warn;
		AudioStreamPlayerRuntime audio = new AudioStreamPlayerRuntime("LobbyTouchVoice", "Missing lobby voice: ", "Lobby voice load failed: ", null, warn, 1, (Node?)(object)scene);
		return new LobbySceneInteraction(scene, delegate(string path)
		{
			audio.Stop();
			float num = Math.Max(0f, volume());
			return (!(num > 0f)) ? 0f : audio.PlayWithResult(path, num).DurationSeconds;
		}, audio.Stop)
		{
			_audio = audio,
			_volume = volume
		};
	}

	public void RefreshVolume()
	{
		if (!_disposed && _volume != null)
		{
			_audio?.RefreshVolume(Math.Max(0f, _volume()));
		}
	}

	private void OnVisibilityChanged()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (!((CanvasItem)_scene).IsVisibleInTree())
		{
			_stop();
			((GodotObject)_scene).Call(StringName.op_Implicit("cancel_interaction"), Array.Empty<Variant>());
		}
	}

	public void Dispose()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (!_disposed)
		{
			_disposed = true;
			_stop();
			_audio?.TrimCache(stopPlayback: true);
			_audio = null;
			_volume = null;
			if (GodotObject.IsInstanceValid((GodotObject)(object)_scene))
			{
				((CanvasItem)_scene).VisibilityChanged -= OnVisibilityChanged;
				((Node)_scene).TreeExiting -= Dispose;
				((GodotObject)_scene).Set(StringName.op_Implicit("play_voice"), Variant.op_Implicit(default(Callable)));
				((GodotObject)_scene).Set(StringName.op_Implicit("stop_voice"), Variant.op_Implicit(default(Callable)));
				((GodotObject)_scene).Call(StringName.op_Implicit("cancel_interaction"), Array.Empty<Variant>());
			}
		}
	}
}

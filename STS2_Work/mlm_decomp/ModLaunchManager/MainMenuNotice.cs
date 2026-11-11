using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace ModLaunchManager;

internal static class MainMenuNotice
{
	private sealed record Notice(string Title, string Body);

	private const int MaxFrames = 1800;

	private const int MaxPending = 3;

	private static readonly List<Notice> Pending = new List<Notice>();

	private static SceneTree? _tree;

	private static int _frames;

	private static string? _lastError;

	internal static void Queue(string title, string body)
	{
		if (Pending.Count >= 3)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} dropping an extra player notice: {title} - {body}");
		}
		else
		{
			Pending.Add(new Notice(title, body));
			GD.PushWarning($"{"[ModLaunchManager]"} {title}: {body}");
			Start();
		}
	}

	private static void Start()
	{
		if (_tree == null)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null)
			{
				GD.PushWarning("[ModLaunchManager] could not show a player notice: the Godot SceneTree is unavailable.");
				return;
			}
			_frames = 0;
			_lastError = null;
			_tree = val;
			val.ProcessFrame += TryShow;
		}
	}

	private static void TryShow()
	{
		if (Pending.Count == 0)
		{
			Stop();
		}
		else
		{
			if (LocManager.Instance == null)
			{
				return;
			}
			_frames++;
			try
			{
				NModalContainer instance = NModalContainer.Instance;
				if (instance != null && GodotObject.IsInstanceValid((GodotObject)(object)instance) && instance.OpenModal == null)
				{
					Notice notice = Pending[0];
					NErrorPopup val = NErrorPopup.Create(notice.Title, notice.Body, false);
					if (val != null)
					{
						instance.Add((Node)(object)val, true);
						Pending.RemoveAt(0);
						_frames = 0;
						GD.Print("[ModLaunchManager] showed a player notice: " + notice.Title);
						if (Pending.Count == 0)
						{
							Stop();
						}
						return;
					}
				}
			}
			catch (Exception ex)
			{
				_lastError = ex.GetType().Name + " " + ex.Message;
			}
			if (_frames >= 1800)
			{
				GD.PushWarning($"{"[ModLaunchManager]"} could not show {Pending.Count} player notice(s) within {1800} frames" + ((_lastError == null) ? "." : (": " + _lastError)));
				Pending.Clear();
				Stop();
			}
		}
	}

	private static void Stop()
	{
		if (_tree != null)
		{
			_tree.ProcessFrame -= TryShow;
			_tree = null;
		}
	}

	internal static void Clear()
	{
		Pending.Clear();
		Stop();
	}
}

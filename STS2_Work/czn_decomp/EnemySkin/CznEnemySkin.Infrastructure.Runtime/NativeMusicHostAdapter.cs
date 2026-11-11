using System;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Runs;

namespace CznEnemySkin.Infrastructure.Runtime;

internal sealed class NativeMusicHostAdapter
{
	private const string StopMusicMethod = "stop_music";

	private static readonly FieldInfo? ProxyField = AccessTools.Field(typeof(NRunMusicController), "_proxy");

	private static readonly FieldInfo? RunStateField = AccessTools.Field(typeof(NRunMusicController), "_runState");

	public bool TryGetRunState(NRunMusicController controller, out IRunState runState, out string reason)
	{
		ArgumentNullException.ThrowIfNull(controller, "controller");
		if (RunStateField == null)
		{
			runState = null;
			reason = "NRunMusicController._runState contract was not found";
			return false;
		}
		try
		{
			object? value = RunStateField.GetValue(controller);
			IRunState val = (IRunState)((value is IRunState) ? value : null);
			if (val == null)
			{
				runState = null;
				reason = "run-music controller has no active run state";
				return false;
			}
			runState = val;
			reason = string.Empty;
			return true;
		}
		catch (Exception ex)
		{
			runState = null;
			reason = ex.GetBaseException().Message;
			return false;
		}
	}

	public bool TrySuppress(out string reason)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		NRunMusicController instance = NRunMusicController.Instance;
		if (instance == null)
		{
			reason = "NRunMusicController is unavailable";
			return false;
		}
		if (ProxyField == null)
		{
			reason = "NRunMusicController._proxy contract was not found";
			return false;
		}
		try
		{
			object? value = ProxyField.GetValue(instance);
			Node val = (Node)((value is Node) ? value : null);
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val) || !((GodotObject)val).HasMethod(StringName.op_Implicit("stop_music")))
			{
				reason = "run-music proxy has no stop_music contract";
				return false;
			}
			((GodotObject)val).Call(StringName.op_Implicit("stop_music"), Array.Empty<Variant>());
			reason = string.Empty;
			return true;
		}
		catch (Exception ex)
		{
			reason = ex.GetBaseException().Message;
			return false;
		}
	}

	public bool TryRestore(out string reason)
	{
		NRunMusicController instance = NRunMusicController.Instance;
		if (instance == null)
		{
			reason = "NRunMusicController is unavailable";
			return false;
		}
		try
		{
			instance.StopCustomMusic();
			reason = string.Empty;
			return true;
		}
		catch (Exception ex)
		{
			reason = ex.GetBaseException().Message;
			return false;
		}
	}
}

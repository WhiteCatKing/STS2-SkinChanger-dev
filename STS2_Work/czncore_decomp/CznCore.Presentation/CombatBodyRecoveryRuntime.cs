using System;
using CznCore.Godot;

namespace CznCore.Presentation;

internal static class CombatBodyRecoveryRuntime
{
	public static void ScheduleExit(float delaySeconds, Func<bool> isCurrent, Func<float> playExit, Action complete, Action<string> warn)
	{
		GodotScheduler.Delay(Math.Max(0.05f, delaySeconds), delegate
		{
			Run(delegate
			{
				float num = playExit();
				if (num <= 0f)
				{
					Run(complete);
				}
				else
				{
					GodotScheduler.Delay(num, delegate
					{
						Run(complete);
					});
				}
			});
		});
		void Run(Action action)
		{
			try
			{
				if (isCurrent())
				{
					action();
				}
			}
			catch (Exception ex)
			{
				warn("Body recovery failed: " + ex.Message);
			}
		}
	}
}

using System;

namespace ModLaunchManager;

internal static class ConfigCloudChanges
{
	internal static Action? Changed { get; set; }

	internal static void Notify()
	{
		try
		{
			Changed?.Invoke();
		}
		catch
		{
		}
	}
}

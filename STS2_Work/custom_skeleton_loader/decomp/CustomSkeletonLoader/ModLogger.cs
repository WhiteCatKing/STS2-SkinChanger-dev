using MegaCrit.Sts2.Core.Logging;

namespace CustomSkeletonLoader;

internal static class ModLogger
{
	internal static void Debug(string message)
	{
		if (ModConfigIntegration.IsDebugLogEnabled())
		{
			Log.Info(message, 2);
		}
	}

	internal static void Warn(string message)
	{
		Log.Warn(message, 2);
	}

	internal static void Error(string message)
	{
		Log.Error(message, 2);
	}
}

using MegaCrit.Sts2.Core.Logging;

namespace STS2SkinChanger.Core;

internal static class ModLog
{
	private const string Prefix = "[SkinChanger] ";

	public static void Info(string message)
	{
		Log.Info("[SkinChanger] " + message, 2);
	}

	public static void Warn(string message)
	{
		Log.Warn("[SkinChanger] " + message, 2);
	}

	public static void Error(string message)
	{
		Log.Error("[SkinChanger] " + message, 2);
	}
}

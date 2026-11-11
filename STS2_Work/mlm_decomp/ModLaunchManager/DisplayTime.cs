using System;
using System.Globalization;

namespace ModLaunchManager;

internal static class DisplayTime
{
	internal static string Format(DateTimeOffset time)
	{
		return time.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
	}
}

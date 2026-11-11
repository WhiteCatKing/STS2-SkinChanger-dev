using System;

namespace STS2SkinChanger.Ui;

internal static class ManagedModListNamePolicy
{
	private const string Prefix = "[SC] ";

	public static string Format(string name, bool isManagedProvider)
	{
		if (!isManagedProvider || name.StartsWith("[SC] ", StringComparison.OrdinalIgnoreCase))
		{
			return name;
		}
		return "[SC] " + name;
	}
}

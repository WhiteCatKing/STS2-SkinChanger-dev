using System.Linq;

namespace STS2SkinChanger.Core;

internal static class GitHubSkinFilter
{
	public const string All = "";

	public const string Missing = "missing";

	public const string Installed = "installed";

	public const string Updatable = "updatable";

	public const string Unrecognized = "unrecognized";

	public static readonly string[] Options = new string[4] { "missing", "installed", "updatable", "unrecognized" };

	public const string DefaultFilter = "missing";

	public static bool Matches(string filter, bool recognized, GitHubInstallState state)
	{
		return filter switch
		{
			"missing" => recognized && state == GitHubInstallState.NotInstalled, 
			"installed" => recognized && state != GitHubInstallState.NotInstalled, 
			"updatable" => recognized && state == GitHubInstallState.Updatable, 
			"unrecognized" => !recognized, 
			_ => true, 
		};
	}

	public static bool IgnoresKindFilter(string filter)
	{
		return filter == "unrecognized";
	}

	public static bool IsOption(string? filter)
	{
		if (filter != null)
		{
			return Options.Contains<string>(filter);
		}
		return false;
	}
}

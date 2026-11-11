using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal readonly record struct ModGameVersionCheck(ModGameVersionCompatibility State, string? RequiredVersion, string? CurrentVersion)
{
	internal bool NeedsAttention
	{
		get
		{
			ModGameVersionCompatibility state = State;
			if ((uint)(state - 2) <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append("State = ");
		builder.Append(State.ToString());
		builder.Append(", RequiredVersion = ");
		builder.Append((object?)RequiredVersion);
		builder.Append(", CurrentVersion = ");
		builder.Append((object?)CurrentVersion);
		return true;
	}
}

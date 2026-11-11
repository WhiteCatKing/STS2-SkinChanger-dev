using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record ModConflict(LaunchEntry Declarer, LaunchEntry Target, ModIncompatibility Rule)
{
	internal string PairKey => ModIncompatibilities.PairKey(Declarer.Id, Target.Id);

	internal bool Involves(LaunchEntry entry)
	{
		if (entry != Declarer)
		{
			return entry == Target;
		}
		return true;
	}

	internal LaunchEntry OtherThan(LaunchEntry entry)
	{
		if (entry != Declarer)
		{
			return Declarer;
		}
		return Target;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Declarer = ");
		builder.Append(Declarer);
		builder.Append(", Target = ");
		builder.Append(Target);
		builder.Append(", Rule = ");
		builder.Append(Rule);
		return true;
	}
}

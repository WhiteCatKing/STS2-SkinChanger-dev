using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record ModIncompatibility(string Id, string? MinVersion, string? MaxVersion, string? Reason)
{
	internal bool HasUnreadableBound
	{
		get
		{
			if (MinVersion == null || ModIncompatibilities.IsReadableVersion(MinVersion))
			{
				if (MaxVersion != null)
				{
					return !ModIncompatibilities.IsReadableVersion(MaxVersion);
				}
				return false;
			}
			return true;
		}
	}

	internal string RangeText
	{
		get
		{
			string minVersion = MinVersion;
			string maxVersion = MaxVersion;
			if (minVersion != null)
			{
				if (maxVersion != null)
				{
					return minVersion + " – " + maxVersion;
				}
				string text = minVersion;
				return "≥ " + text;
			}
			if (maxVersion != null)
			{
				string text2 = maxVersion;
				return "≤ " + text2;
			}
			return string.Empty;
		}
	}

	internal bool AppliesTo(string? version)
	{
		if (!(ModIncompatibilities.CompareVersions(version, MinVersion) < 0))
		{
			return !(ModIncompatibilities.CompareVersions(version, MaxVersion) > 0);
		}
		return false;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Id = ");
		builder.Append((object?)Id);
		builder.Append(", MinVersion = ");
		builder.Append((object?)MinVersion);
		builder.Append(", MaxVersion = ");
		builder.Append((object?)MaxVersion);
		builder.Append(", Reason = ");
		builder.Append((object?)Reason);
		return true;
	}
}

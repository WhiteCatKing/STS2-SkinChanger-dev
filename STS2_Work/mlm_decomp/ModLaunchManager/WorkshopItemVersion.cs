using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record WorkshopItemVersion(ulong ManifestId, uint Updated, string MinBranch, string MaxBranch)
{
	internal bool IsUnrestricted
	{
		get
		{
			if (MinBranch.Length == 0)
			{
				return MaxBranch.Length == 0;
			}
			return false;
		}
	}

	internal bool HasKnownBounds
	{
		get
		{
			if ((MinBranch.Length == 0 || ((ReadOnlySpan<string>)BranchOrder).Contains(MinBranch)) && (MaxBranch.Length == 0 || ((ReadOnlySpan<string>)BranchOrder).Contains(MaxBranch)))
			{
				if (MinBranch.Length != 0 && MaxBranch.Length != 0)
				{
					return Array.IndexOf(BranchOrder, MinBranch) <= Array.IndexOf(BranchOrder, MaxBranch);
				}
				return true;
			}
			return false;
		}
	}

	private static readonly string[] BranchOrder = new string[4] { "public", "public-beta", "private-beta", "dev-test" };

	internal bool Supports(string branch)
	{
		int num = Array.IndexOf(BranchOrder, branch);
		int num2 = Array.IndexOf(BranchOrder, MinBranch);
		int num3 = Array.IndexOf(BranchOrder, MaxBranch);
		if (num >= 0 && (MinBranch.Length == 0 || (num2 >= 0 && num >= num2)))
		{
			if (MaxBranch.Length != 0)
			{
				if (num3 >= 0)
				{
					return num <= num3;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("ManifestId = ");
		builder.Append(ManifestId.ToString());
		builder.Append(", Updated = ");
		builder.Append(Updated.ToString());
		builder.Append(", MinBranch = ");
		builder.Append((object?)MinBranch);
		builder.Append(", MaxBranch = ");
		builder.Append((object?)MaxBranch);
		return true;
	}

	[CompilerGenerated]
	private WorkshopItemVersion(WorkshopItemVersion original)
	{
		ManifestId = original.ManifestId;
		Updated = original.Updated;
		MinBranch = original.MinBranch;
		MaxBranch = original.MaxBranch;
	}
}

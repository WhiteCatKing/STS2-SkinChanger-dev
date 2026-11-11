using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ModLaunchManager;

internal sealed record WorkshopChangeNote(uint Time, string Text, IReadOnlyList<string> Branches)
{
	private static readonly Regex BranchToken = new Regex("^[a-z0-9][a-z0-9_.-]*$", RegexOptions.CultureInvariant);

	private static readonly string[] KnownBranches = new string[4] { "public", "public-beta", "private-beta", "dev-test" };

	internal bool ExcludesBranch(string branch)
	{
		if (Branches.Count > 0 && ((ReadOnlySpan<string>)KnownBranches).Contains(branch) && Branches.All((string name) => BranchToken.IsMatch(name)))
		{
			return !Branches.Contains<string>(branch, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}

	[CompilerGenerated]
	private WorkshopChangeNote(WorkshopChangeNote original)
	{
		Time = original.Time;
		Text = original.Text;
		Branches = original.Branches;
	}
}

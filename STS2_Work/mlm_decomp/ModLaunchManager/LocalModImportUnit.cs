using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record LocalModImportUnit(string Folder, string TargetName, IReadOnlyList<LocalModManifest> Mods, LocalModInstalled? Replaces = null)
{
	internal string DisplayName => string.Join("、", Mods.Select((LocalModManifest mod) => mod.DisplayName));

	internal string TargetFolder(string modsFolder)
	{
		return Replaces?.LocalFolder ?? Path.Combine(modsFolder, TargetName);
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Folder = ");
		builder.Append((object?)Folder);
		builder.Append(", TargetName = ");
		builder.Append((object?)TargetName);
		builder.Append(", Mods = ");
		builder.Append(Mods);
		builder.Append(", Replaces = ");
		builder.Append(Replaces);
		return true;
	}
}

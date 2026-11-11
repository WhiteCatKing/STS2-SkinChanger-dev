using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record LocalModManifest(string Id, string? Name, string? Version, string ManifestFile, bool HasDll, bool HasPck)
{
	internal string DisplayName
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(Name))
			{
				return TextEncodingRepair.Repair(Name);
			}
			return Id;
		}
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Id = ");
		builder.Append((object?)Id);
		builder.Append(", Name = ");
		builder.Append((object?)Name);
		builder.Append(", Version = ");
		builder.Append((object?)Version);
		builder.Append(", ManifestFile = ");
		builder.Append((object?)ManifestFile);
		builder.Append(", HasDll = ");
		builder.Append(HasDll.ToString());
		builder.Append(", HasPck = ");
		builder.Append(HasPck.ToString());
		return true;
	}
}

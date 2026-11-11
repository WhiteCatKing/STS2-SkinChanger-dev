using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace ModLaunchManager;

internal sealed record CloudSyncReport(IReadOnlyList<CloudSyncFileResult> Files)
{
	internal bool ChangedAnyLocal => Files.Any(delegate(CloudSyncFileResult file)
	{
		CloudSyncDirection direction = file.Direction;
		return (uint)(direction - 2) <= 1u;
	});

	internal bool HasPending { get; init; }

	internal bool HasProblems => Files.Any((CloudSyncFileResult file) => file.Problem != null);

	internal bool ChangedLocal(string localName)
	{
		return Files.Any(delegate(CloudSyncFileResult file)
		{
			bool flag = file.LocalName == localName;
			if (flag)
			{
				CloudSyncDirection direction = file.Direction;
				bool flag2 = (uint)(direction - 2) <= 1u;
				flag = flag2;
			}
			return flag;
		});
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Files = ");
		builder.Append(Files);
		return true;
	}
}

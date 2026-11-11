using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed class ModOptionApplyReport
{
	internal int ModsChanged { get; set; }

	internal int KeysWritten { get; set; }

	internal bool StoreChanged { get; set; }

	internal List<string> Failures { get; } = new List<string>();
}

using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal sealed class WorkshopBranchStatus
{
	internal required ulong ItemId { get; init; }

	internal required string CacheFile { get; init; }

	internal required string ItemRoot { get; init; }

	internal List<Mod> Mods { get; } = new List<Mod>();

	internal WorkshopItemVersions? Catalog { get; set; }

	internal WorkshopBranchSelection? Active { get; set; }

	internal bool Busy { get; set; }

	internal bool RestoreFailed { get; set; }

	internal string? TargetBranch { get; set; }

	internal string? RequestFolder { get; set; }

	internal string? ErrorKey { get; set; }

	internal double Elapsed { get; set; }

	internal bool Locked => Mods.Any((Mod mod) => (int)mod.state == 1 || mod.manifest?.id == "ModLaunchManager" || CollectibleModAssemblyLoader.GetRecordedAssemblies(mod).Length != 0);

	internal IEnumerable<string> AvailableBranches => new string[2] { "public", "public-beta" }.Where((string branch) => Catalog?.ForBranch(branch) != null);
}

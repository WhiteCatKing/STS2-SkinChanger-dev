using System.Collections.Generic;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal sealed record LocalModScan(IReadOnlyList<Mod> Added, IReadOnlyList<string> VanishedFiles);

using System;
using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record LocalModImportHost(Func<IReadOnlyDictionary<string, LocalModInstalled>> Installed, Func<string> PrepareModsFolder, Action<IReadOnlyList<string>> Replaced, Action OpenModsFolder);

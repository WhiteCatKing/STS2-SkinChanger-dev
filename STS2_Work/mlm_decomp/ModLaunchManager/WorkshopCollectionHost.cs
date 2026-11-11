using System;
using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record WorkshopCollectionHost(Func<IReadOnlyList<LaunchProfile>> Profiles, Func<LaunchProfile, bool, PlanResult> Save, Action OpenDownloads, Action<ulong, string, string>? RemoveItem = null, Func<ulong, bool>? CanRemoveItem = null);

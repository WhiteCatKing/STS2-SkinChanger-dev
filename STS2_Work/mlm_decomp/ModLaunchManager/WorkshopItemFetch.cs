using System;
using System.Collections.Generic;

namespace ModLaunchManager;

internal delegate void WorkshopItemFetch(IReadOnlyList<ulong> ids, Action<IReadOnlyList<WorkshopItemFacts>?> done);

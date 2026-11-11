using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record VanillaTable(string File, IReadOnlyDictionary<string, string> Source, IReadOnlyDictionary<string, string> Target);

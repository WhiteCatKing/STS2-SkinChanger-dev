using System;

namespace ModLaunchManager;

internal sealed record AiTranslationRetryNotice(AiTranslationRetryKind Kind, int Attempt, int Entries, TimeSpan Delay, string Issues = "");

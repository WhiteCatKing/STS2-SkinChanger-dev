using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed record AiTranslationBatchResult(Dictionary<string, string> Accepted, Dictionary<string, TranslationFileException> Failed);

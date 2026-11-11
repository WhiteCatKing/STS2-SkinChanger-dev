using System.Collections.Generic;

namespace STS2SkinChanger.Ui;

internal sealed record OtherCreatureDefinition(string Id, string LocalizationTable, string LocalizationKey, string FallbackTitle, string ScenePath, IReadOnlyList<string> IdleAliases, IReadOnlyList<OtherCreatureActionDefinition> Actions);

using System.Collections.Generic;

namespace STS2SkinChanger.Ui;

internal sealed record OtherCreatureActionDefinition(OtherCreatureActionKind Kind, IReadOnlyList<string> AnimationAliases, bool Loop, IReadOnlyList<string> FollowUpAliases, bool FollowUpLoop, string? SfxPath = null);

using System.Collections.Generic;

namespace CznEnemySkin.Domain;

internal sealed record EnemyActionPresentation(EnemyAction Action, IReadOnlyList<EnemyAudioCue> AudioChoices, IReadOnlyList<EnemyEffectCue> Effects, IReadOnlyList<EnemyAudioLayerCue>? AudioLayers = null);

using CznEnemySkin.Domain;

namespace CznEnemySkin.Application;

internal sealed record CuratedBossMusicSelection(CuratedBossMusicProfile Profile, int PhaseIndex, MusicTrackProfile Track);

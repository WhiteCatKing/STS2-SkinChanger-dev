namespace CznEnemySkin.Domain;

internal sealed record EnemySecondaryBodyProfile(string Role, string SkeletonDataPath, float VisualScale, bool InitiallyVisible, bool HidePrimaryInitially, string? EntranceAnimation, string IdleAnimation, int ZIndexOffset = 0);

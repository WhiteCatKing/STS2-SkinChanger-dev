using System;
using CznEnemySkin.Domain;

namespace CznEnemySkin.Profiles;

internal sealed record SharedHitRoute(Type MonsterType, string ProfileId, string Trigger, EnemyAction Action, string? NativeVfx, bool FollowTarget, float MinimumWait, bool NativeMultiHit = false, string? MoveId = null, bool StateSelected = false, string? AlternateNativeVfx = null, bool EntryTarget = false)
{
	internal bool AcceptsNativeVfx(string? path)
	{
		if (!(path == NativeVfx))
		{
			if (AlternateNativeVfx != null)
			{
				return path == AlternateNativeVfx;
			}
			return false;
		}
		return true;
	}
}

using CznEnemySkin.Domain;
using MegaCrit.Sts2.Core.Rooms;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class ActRoomScopeClassifier
{
	public static bool TryClassify(RoomType roomType, bool isWeak, out ActRoomScope scope)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Invalid comparison between Unknown and I4
		if ((int)roomType != 1)
		{
			if ((int)roomType == 2)
			{
				scope = ActRoomScope.Elite;
				return true;
			}
			scope = ActRoomScope.StandardCombat;
			return false;
		}
		if (isWeak)
		{
			scope = ActRoomScope.WeakMonster;
			return true;
		}
		scope = ActRoomScope.RegularMonster;
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class MultiplayerMerchantPlayerVisualIsolationPatch
{
	private static MethodBase TargetMethod()
	{
		return AccessTools.Method(typeof(NMerchantRoom), "AfterRoomIsLoaded", (Type[])null, (Type[])null) ?? throw new MissingMethodException(typeof(NMerchantRoom).FullName, "AfterRoomIsLoaded");
	}

	[HarmonyPrefix]
	[HarmonyPriority(800)]
	private static bool Prefix(List<Player> ____players, Control ____characterContainer, List<NMerchantCharacter> ____playerVisuals)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (____players.Count <= 1)
		{
			return true;
		}
		Player item = LocalContext.GetMe((IEnumerable<Player>)____players) ?? ____players[0];
		____players.Remove(item);
		____players.Insert(0, item);
		int num = Mathf.CeilToInt(Mathf.Sqrt((float)____players.Count));
		for (int i = 0; i < num; i++)
		{
			float num2 = -140f * (float)i;
			for (int j = 0; j < num; j++)
			{
				int num3 = i * num + j;
				if (num3 >= ____players.Count)
				{
					break;
				}
				Player val = ____players[num3];
				using (MultiplayerSkinSync.BeginPlayerRuntimeScope(val, val.Character.MerchantAnimPath))
				{
					NMerchantCharacter val2 = PreloadManager.Cache.GetScene(val.Character.MerchantAnimPath).Instantiate<NMerchantCharacter>((GenEditState)0);
					GodotTreeExtensions.AddChildSafely((Node)(object)____characterContainer, (Node)(object)val2);
					GodotTreeExtensions.MoveChildSafely((Node)(object)____characterContainer, (Node)(object)val2, 0);
					((Node2D)val2).Position = new Vector2(num2, -50f * (float)i);
					if (i > 0)
					{
						((CanvasItem)val2).Modulate = new Color(0.5f, 0.5f, 0.5f, 1f);
					}
					num2 -= 275f;
					____playerVisuals.Add(val2);
				}
			}
		}
		return false;
	}
}

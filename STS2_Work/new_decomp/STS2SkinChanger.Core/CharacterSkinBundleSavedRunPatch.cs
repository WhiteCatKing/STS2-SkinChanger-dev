using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class CharacterSkinBundleSavedRunPatch
{
	private static IEnumerable<MethodBase> TargetMethods()
	{
		return new MethodInfo[2]
		{
			AccessTools.Method(typeof(RunManager), "SetUpSavedSingleplayer", (Type[])null, (Type[])null),
			AccessTools.Method(typeof(RunManager), "SetUpSavedMultiplayer", (Type[])null, (Type[])null)
		};
	}

	[HarmonyPriority(800)]
	private static void Prefix(RunState state, object[] __args)
	{
		try
		{
			SerializableRun val = __args.OfType<SerializableRun>().FirstOrDefault();
			if (val != null)
			{
				SkinService.LoadCharacterSkinBundleRun(state, val, 1uL, multiplayer: false);
				return;
			}
			LoadRunLobby val2 = __args.OfType<LoadRunLobby>().FirstOrDefault();
			if (val2 != null)
			{
				SkinService.LoadCharacterSkinBundleRun(state, val2.Run, val2.NetService.NetId, multiplayer: true);
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("继续对局恢复皮肤包记录失败，不阻止游戏读档：" + ex);
		}
	}
}

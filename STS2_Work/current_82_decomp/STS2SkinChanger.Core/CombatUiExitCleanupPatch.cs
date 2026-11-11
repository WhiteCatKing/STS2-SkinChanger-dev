using System;
using System.Threading;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NCombatUi), "_ExitTree")]
internal static class CombatUiExitCleanupPatch
{
	[HarmonyFinalizer]
	private static Exception? Finalizer(NCombatUi __instance, Exception? __exception)
	{
		if (__exception == null)
		{
			return null;
		}
		try
		{
			(AccessTools.Field(typeof(NCombatUi), "_cts")?.GetValue(__instance) as CancellationTokenSource)?.Cancel();
		}
		catch (Exception ex)
		{
			ModLog.Warn("战斗界面退出异常后的任务取消失败：" + ex.GetBaseException().Message);
		}
		try
		{
			AccessTools.Method(typeof(NCombatUi), "DisconnectSignals", (Type[])null, (Type[])null).Invoke(__instance, null);
		}
		catch (Exception ex2)
		{
			ModLog.Warn("战斗界面退出异常后的回调清理失败：" + ex2.GetBaseException().Message);
		}
		ModLog.Warn("战斗界面扩展退出异常，已尝试补做原版任务取消和战斗回调清理，原始异常仍保留。");
		return __exception;
	}
}

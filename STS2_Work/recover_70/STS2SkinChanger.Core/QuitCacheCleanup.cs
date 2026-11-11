using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NGame), "Quit")]
internal static class QuitCacheCleanup
{
	private static readonly QuitCleanupCoordinator Coordinator = new QuitCleanupCoordinator();

	internal static bool AcceptsQuitAsExit => Coordinator.InitializationCompleted;

	internal static void MarkInitializationCompleted()
	{
		Coordinator.MarkInitializationCompleted();
	}

	private static bool Prefix(NGame __instance)
	{
		return Coordinator.HandleQuit(() => Task.Run(() => QuitExitCleanup.Clean(SkinChangerPaths.Storage)), delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)__instance))
			{
				__instance.Quit();
			}
		}, ModLog.Info);
	}
}

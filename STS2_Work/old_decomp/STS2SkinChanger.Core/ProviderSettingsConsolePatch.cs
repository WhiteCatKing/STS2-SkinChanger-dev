using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole;

namespace STS2SkinChanger.Core;

[HarmonyPatch(/*Could not decode attribute arguments.*/)]
internal static class ProviderSettingsConsolePatch
{
	private static void Postfix(DevConsole __instance, bool __0)
	{
		ProviderSettingsControls.Attach(__instance, __0);
	}
}

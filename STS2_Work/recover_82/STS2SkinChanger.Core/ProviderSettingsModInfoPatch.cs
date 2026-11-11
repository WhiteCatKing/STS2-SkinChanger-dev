using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NModInfoContainer), "Fill")]
internal static class ProviderSettingsModInfoPatch
{
	private static void Postfix(NModInfoContainer __instance, Mod mod)
	{
		ManagedSkinModLoader.EnsureProviderSettings(mod);
		ManagedMerchantSettingsBridge.FillSettings(__instance, mod);
	}
}

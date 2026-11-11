using CznEnemySkin.Infrastructure.Runtime;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace CznEnemySkin.Infrastructure.Patches;

[HarmonyPatch(typeof(AssetLoadingSession), "IsVfxScene")]
internal static class EffectSceneSerialLoadPatch
{
	private static void Postfix(string path, ref bool __result)
	{
		if (!__result && EnemyPresentationAssetPaths.IsEffectScene(path))
		{
			__result = true;
		}
	}
}

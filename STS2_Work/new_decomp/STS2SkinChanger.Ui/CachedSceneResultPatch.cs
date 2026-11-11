using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(AssetCache), "GetScene")]
internal static class CachedSceneResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(string path, ref PackedScene __result)
	{
		ContextualSkinControls.ReplaceCachedScene(path, ref __result);
	}
}

using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(AssetCache), "GetTexture2D")]
internal static class CachedTextureResultPatch
{
	[HarmonyPriority(0)]
	private static void Postfix(string path, ref Texture2D __result)
	{
		ContextualSkinControls.ReplaceCachedTexture(path, ref __result);
	}
}

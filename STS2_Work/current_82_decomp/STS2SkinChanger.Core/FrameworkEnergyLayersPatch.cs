using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

[HarmonyPatch(typeof(NEnergyCounter), "_Ready")]
internal static class FrameworkEnergyLayersPatch
{
	private static readonly FieldInfo? PlayerField = AccessTools.Field(typeof(NEnergyCounter), "_player");

	private static readonly string[] LayerPaths = new string[5] { "Layers/Layer1", "Layers/RotationLayers/Layer2", "Layers/RotationLayers/Layer3", "Layers/Layer4", "Layers/Layer5" };

	[HarmonyPostfix]
	private static void Postfix(NEnergyCounter __instance)
	{
		object? obj = PlayerField?.GetValue(__instance);
		Player val = (Player)((obj is Player) ? obj : null);
		if (val == null || !FrameworkSkinRuntime.TryGetEnergyLayers(val.Character, out IReadOnlyList<string> paths))
		{
			return;
		}
		for (int i = 0; i < Math.Min(paths.Count, LayerPaths.Length); i++)
		{
			TextureRect nodeOrNull = ((Node)__instance).GetNodeOrNull<TextureRect>(NodePath.op_Implicit(LayerPaths[i]));
			if (nodeOrNull != null)
			{
				nodeOrNull.Texture = ResourceLoader.Load<Texture2D>(paths[i], (string)null, (CacheMode)1);
			}
		}
	}
}

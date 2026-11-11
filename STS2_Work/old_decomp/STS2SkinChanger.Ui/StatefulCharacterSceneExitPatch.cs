using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCharacterSelectScreen), "BeginRun")]
internal static class StatefulCharacterSceneExitPatch
{
	private static void Prefix(NCharacterSelectScreen __instance)
	{
		try
		{
			Visit((Node)(object)__instance);
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("character scene exit", exception);
		}
		static void Visit(Node root)
		{
			if (StatefulCardArtRuntime.Owns(((object)root).GetType().Assembly))
			{
				AccessTools.Method(((object)root).GetType(), "ResetFingersPosition", (Type[])null, (Type[])null)?.Invoke(root, null);
			}
			foreach (Node child in root.GetChildren(false))
			{
				Visit(child);
			}
		}
	}
}

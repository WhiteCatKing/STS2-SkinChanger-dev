using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using St2bs.St2bsCode.Debug;

namespace St2bs.St2bsCode.Patches;

[HarmonyPatch(typeof(ActModel), "GenerateBackgroundAssets")]
public static class CustomActBackgroundPatch
{
	private static readonly string[] LayersFieldNameCandidates = new string[4] { "<BgLayers>k__BackingField", "_bgLayers", "BgLayers", "m_bgLayers" };

	private static readonly string[] BgSceneFieldNameCandidates = new string[4] { "<BackgroundScenePath>k__BackingField", "_backgroundScenePath", "BackgroundScenePath", "m_backgroundScenePath" };

	private static readonly string[] FgFieldNameCandidates = new string[4] { "<FgLayer>k__BackingField", "_fgLayer", "FgLayer", "m_fgLayer" };

	private static void SetFieldValue(object target, string[] candidates, object value)
	{
		BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		Type type = target.GetType();
		foreach (string text in candidates)
		{
			FieldInfo field = type.GetField(text, bindingAttr);
			if (field != null)
			{
				field.SetValue(target, value);
				St2bsLogger.Info("CustomActBackgroundPatch: set field '" + text + "'", "SetFieldValue", 47);
				return;
			}
		}
		St2bsLogger.Warn("CustomActBackgroundPatch: could not find any field matching candidates", "SetFieldValue", 51);
	}

	[HarmonyPrefix]
	public static bool BeforeGenerateBackgroundAssets(ActModel __instance, Rng rng, ref BackgroundAssets __result)
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Expected O, but got Unknown
		bool flag;
		switch (((AbstractModel)__instance).Id.Entry)
		{
		case "WINTER_BELL":
		case "DARK_STAGE":
		case "CRIMEA_CARETAKER_CEMETERY":
		case "KURANINDE":
		case "RED_CITY_FRIEDELL":
		case "SMOKE_FOREST":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag)
		{
			return true;
		}
		__result = new BackgroundAssets("glory", rng);
		FieldInfo field = typeof(BackgroundAssets).GetField(LayersFieldNameCandidates[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			string[] layersFieldNameCandidates = LayersFieldNameCandidates;
			foreach (string name in layersFieldNameCandidates)
			{
				field = typeof(BackgroundAssets).GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (field != null)
				{
					break;
				}
			}
		}
		if (field?.GetValue(__result) is IList list)
		{
			list.Clear();
		}
		SetFieldValue(__result, FgFieldNameCandidates, null);
		switch (((AbstractModel)__instance).Id.Entry)
		{
		case "WINTER_BELL":
			SetFieldValue(__result, BgSceneFieldNameCandidates, "res://St2bs/scenes/backgrounds/winter_bell/winter_bell_background.tscn");
			break;
		case "DARK_STAGE":
			SetFieldValue(__result, BgSceneFieldNameCandidates, "res://St2bs/scenes/backgrounds/gulan/gulan_bg.tscn");
			break;
		case "CRIMEA_CARETAKER_CEMETERY":
			SetFieldValue(__result, BgSceneFieldNameCandidates, "res://St2bs/scenes/Crimea.tscn");
			break;
		case "KURANINDE":
			SetFieldValue(__result, BgSceneFieldNameCandidates, "res://St2bs/scenes/Kuraninde.tscn");
			break;
		case "RED_CITY_FRIEDELL":
			SetFieldValue(__result, BgSceneFieldNameCandidates, "res://St2bs/scenes/redcity.tscn");
			break;
		case "SMOKE_FOREST":
			SetFieldValue(__result, BgSceneFieldNameCandidates, "res://St2bs/scenes/SmokeForest.tscn");
			break;
		}
		St2bsLogger.Info("CustomActBackgroundPatch: applied background for " + ((AbstractModel)__instance).Id.Entry, "BeforeGenerateBackgroundAssets", 106);
		return false;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.0.0.7889')

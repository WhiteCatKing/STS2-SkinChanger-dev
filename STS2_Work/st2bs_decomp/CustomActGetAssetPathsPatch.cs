using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using St2bs.St2bsCode.Debug;

namespace St2bs.St2bsCode.Patches;

[HarmonyPatch(typeof(EncounterModel), "GetAssetPaths")]
public static class CustomActGetAssetPathsPatch
{
	private static readonly HashSet<string> CustomActs = new HashSet<string> { "KURANINDE", "PSYCHIATRIC_WARD", "RED_CITY_FRIEDELL", "SMOKE_FOREST", "DARK_STAGE", "CRIMEA_CARETAKER_CEMETERY", "OXFORD_ACADEMY", "CLOCK_TOWER", "CARROLL_RIVER", "GIRLS_NIGHTMARE" };

	[HarmonyPrefix]
	public static bool BeforeGetAssetPaths(EncounterModel __instance, ref IEnumerable<string> __result)
	{
		try
		{
			RunManager instance = RunManager.Instance;
			if (instance == null)
			{
				return true;
			}
			RunState value = Traverse.Create((object)instance).Property("State", (object[])null).GetValue<RunState>();
			if (((value != null) ? value.Acts : null) == null || value.CurrentActIndex < 0 || value.CurrentActIndex >= value.Acts.Count)
			{
				return true;
			}
			ActModel obj = value.Acts[value.CurrentActIndex];
			string text = ((obj != null) ? ((AbstractModel)obj).Id.Entry : null);
			if (text != null && CustomActs.Contains(text))
			{
				St2bsLogger.Info("CustomActGetAssetPathsPatch: returning empty assets for encounter " + ((AbstractModel)__instance).Id.Entry + " on act " + text, "BeforeGetAssetPaths", 170);
				__result = Enumerable.Empty<string>();
				return false;
			}
		}
		catch
		{
		}
		return true;
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '9.0.0.7889')

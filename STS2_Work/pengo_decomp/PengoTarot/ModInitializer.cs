using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.BalatroEffect;
using PengoTarot.ConfigFW;
using PengoTarot.Network;
using PengoTarot.Patch.Card;
using PengoTarot.Relics;

namespace PengoTarot;

[ModInitializer("Initialize")]
public static class ModInitializer
{
	[HarmonyPatch(typeof(RunManager), "Launch")]
	public static class InitTarotSyncPatch
	{
		private static void Postfix(RunManager __instance)
		{
			if (!__instance.IsSingleplayerOrFakeMultiplayer)
			{
				TarotSync?.Dispose();
				TarotSync = null;
				RunState val = __instance.DebugOnlyGetState();
				if (val != null)
				{
					TarotSync = new TarotSynchronizer(__instance.RunLocationTargetedBuffer, __instance.NetService, (IPlayerCollection)(object)val, __instance.NetService.NetId);
				}
			}
		}
	}

	private const string CompatVersion = "0.111.0";

	public static TarotSynchronizer? TarotSync { get; private set; }

	public static void Initialize()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Log.Info("[PengoTarot] Initializing (compat target: STS2 v0.111.0)", 2);
		new Harmony("PengoTarot.Pengo").PatchAll();
		ModHelper.AddModelToPool(typeof(EventRelicPool), typeof(StargazerKit));
		Config.Load();
		EnchantmentConfig.Load();
		EffectRegistry.Initialize();
		GetNodeCallRewriter.ScheduleAfterAllModsLoaded();
		MintyRestHPRenderRewriter.ScheduleAfterModsLoaded();
		NConfigFloatingWindow.ScheduleHintFontsWarmUp();
		Log.Info("[PengoTarot] Initialization complete. Loaded for STS2 v0.111.0.", 2);
	}
}

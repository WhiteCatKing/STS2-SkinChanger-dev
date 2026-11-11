using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using thunninoiSkinManager.thunninoiSkinManagerCode;
using thunninoiSkinManager.thunninoiSkinManagerCode.Patches;

namespace ChenIronclad.ChenIroncladCode;

[HarmonyPatch(typeof(SkinRegistry), "SkinDbSetup")]
public class SkinRegister
{
	[HarmonyPostfix]
	private static void RegisterSkin()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		ChenIroncladInit.Logger.Info("Registering skin", 1);
		SkinData val = new SkinData(((AbstractModel)ModelDb.Character<Ironclad>()).Id, "chen", "Dawnstreak");
		val.RegisterCharacter((CharacterSkin)(object)new Chen_Skin());
		val.RegisterConfig("UseCardFrame", (Func<bool>)(() => ChenConfig.UseChenCardFrame));
		val.RegisterConfig("UseEnergy", (Func<bool>)(() => ChenConfig.UseChenEnergy));
		val.RegisterConfig("UseHands", (Func<bool>)(() => ChenConfig.UseChenMultArm));
		SkinRegistry.Register(val);
	}
}

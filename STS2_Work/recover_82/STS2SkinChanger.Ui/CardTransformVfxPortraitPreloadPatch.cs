using System;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardTransformVfx), "Create")]
internal static class CardTransformVfxPortraitPreloadPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(CardModel startCard, CardModel endCard)
	{
		try
		{
			ModLog.Info("变化特效已创建：" + (((startCard != null) ? ((object)((AbstractModel)startCard).Id).ToString() : null) ?? "<null>") + " → " + (((endCard != null) ? ((object)((AbstractModel)endCard).Id).ToString() : null) ?? "<null>"));
			SkinService.PreloadCardPortraits(new global::_003C_003Ez__ReadOnlyArray<CardModel>((CardModel[])(object)new CardModel[2] { startCard, endCard }));
		}
		catch (Exception ex)
		{
			ModLog.Warn("预先加载变化特效卡图失败：" + ex.GetBaseException().Message);
		}
	}
}

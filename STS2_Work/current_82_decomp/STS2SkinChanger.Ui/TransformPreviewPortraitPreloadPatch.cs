using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NTransformPreview), "Initialize")]
internal static class TransformPreviewPortraitPreloadPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(IEnumerable<CardTransformation> cardTransformations)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			List<CardModel> list = new List<CardModel>();
			foreach (CardTransformation cardTransformation in cardTransformations)
			{
				CardTransformation current = cardTransformation;
				if (((CardTransformation)(ref current)).Original != null)
				{
					list.Add(((CardTransformation)(ref current)).Original);
				}
				if (((CardTransformation)(ref current)).Replacement != null)
				{
					list.Add(((CardTransformation)(ref current)).Replacement);
				}
				if (((CardTransformation)(ref current)).ReplacementOptions != null)
				{
					list.AddRange(((CardTransformation)(ref current)).ReplacementOptions);
				}
			}
			if (list.Count > 0)
			{
				SkinService.PreloadCardPortraits(list);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("预先加载变化预览卡图失败：" + ex.GetBaseException().Message);
		}
	}
}

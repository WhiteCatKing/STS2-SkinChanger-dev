using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

[HarmonyPatch(typeof(NCardTransformShineVfx), "Create")]
internal static class CardTransformShineVfxPortraitPreloadPatch
{
	[HarmonyPriority(800)]
	private static void Prefix(NCard cardNode, CardModel endCard)
	{
		try
		{
			List<CardModel> list = new List<CardModel>();
			if (((cardNode != null) ? cardNode.Model : null) != null)
			{
				list.Add(cardNode.Model);
			}
			if (endCard != null)
			{
				list.Add(endCard);
			}
			if (list.Count > 0)
			{
				SkinService.PreloadCardPortraits(list);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("预先加载变化光效卡图失败：" + ex.GetBaseException().Message);
		}
	}

	[HarmonyPriority(0)]
	private static void Postfix(NCardTransformShineVfx? __result)
	{
		try
		{
			if (__result != null && GodotObject.IsInstanceValid((GodotObject)(object)__result))
			{
				((CanvasItem)__result).ZIndex = 10;
			}
		}
		catch
		{
		}
	}
}

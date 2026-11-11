using System;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Ui.EmperorCrabCompat;

internal static class EmperorCrabCompat
{
	private static bool _installed;

	internal static void Install(Harmony harmony)
	{
		if (_installed)
		{
			return;
		}
		_installed = true;
		try
		{
			KaiserCrabSkinRefreshPatch.EnsureInstalled(harmony);
		}
		catch (Exception ex)
		{
			GD.PrintErr("[SkinChanger] 安装帝皇蟹图鉴刷新补丁失败：" + ex);
		}
		try
		{
			CombatCrabSkinPatch.Install(harmony);
		}
		catch (Exception ex2)
		{
			GD.PrintErr("[SkinChanger] 安装帝皇蟹局内背景补丁失败：" + ex2);
		}
		try
		{
			HalfHpCrabSkinPatch.Install(harmony);
		}
		catch (Exception ex3)
		{
			GD.PrintErr("[SkinChanger] 安装帝皇蟹半血爆衣补丁失败：" + ex3);
		}
		try
		{
			RitsuLibCardGuardPatch.Install(harmony);
		}
		catch (Exception ex4)
		{
			GD.PrintErr("[SkinChanger] 安装 RitsuLib 卡牌异步保护失败：" + ex4);
		}
	}
}

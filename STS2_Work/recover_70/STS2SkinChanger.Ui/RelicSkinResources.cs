using System;
using System.Collections.Generic;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class RelicSkinResources
{
	private static readonly HashSet<string> ReportedFailures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ReportedStandaloneOverrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public static void Replace(string resourcePath, ref Texture2D result)
	{
		try
		{
			if (SkinService.IsSelectedCharacterStandaloneTexture(((Resource)result).ResourcePath))
			{
				if (ReportedStandaloneOverrides.Add(resourcePath + "\n" + ((Resource)result).ResourcePath))
				{
					ModLog.Info("已保留选中角色皮肤的独立遗物图标：" + resourcePath + " -> " + ((Resource)result).ResourcePath);
				}
			}
			else
			{
				result = SkinService.GetRelicIconOverride(resourcePath) ?? result;
			}
		}
		catch (Exception ex)
		{
			if (ReportedFailures.Add(resourcePath))
			{
				ModLog.Warn("隔离加载遗物图标失败，已保留当前图标 " + resourcePath + "：" + ex.GetBaseException().Message);
			}
		}
	}
}

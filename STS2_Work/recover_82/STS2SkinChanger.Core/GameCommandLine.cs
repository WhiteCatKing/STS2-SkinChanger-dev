using System;
using System.Collections.Generic;
using Godot;

namespace STS2SkinChanger.Core;

internal static class GameCommandLine
{
	public static IReadOnlyList<string> Arguments()
	{
		try
		{
			return OS.GetCmdlineArgs();
		}
		catch (Exception ex)
		{
			ModLog.Warn("无法读取启动参数，跳过由启动器注入的游戏资源包：" + ex.GetBaseException().Message);
			return Array.Empty<string>();
		}
	}
}

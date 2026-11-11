using System;
using System.Reflection;

namespace STS2SkinChanger.Core;

internal static class ManagedSkinBehaviorBridge
{
	private static readonly ScopedSkinBehavior Router = new ScopedSkinBehavior("Gurio.SkinChanger.skin-behavior-ownership", SkinService.IsCharacterBehaviorSelected);

	public static void Install(Assembly assembly)
	{
		try
		{
			int num = Router.Install(assembly);
			if (num > 0)
			{
				ModLog.Info($"已为 {assembly.GetName().Name} 的 {num} 个皮肤适用入口接入对象归属检查；保留原作者设置与原判断。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("接入原皮肤行为范围失败，未修改原设置：" + ex.GetBaseException().Message);
		}
	}
}

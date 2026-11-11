using System;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Ui.EmperorCrabCompat;

internal static class RitsuLibCardGuardPatch
{
	private static bool _installed;

	public static void Install(Harmony harmony)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		if (_installed)
		{
			return;
		}
		try
		{
			Assembly assembly = FindAssembly("STS2-RitsuLib.Runtime");
			if (assembly == null)
			{
				GD.Print("[EmperorCrabSkinFix] 未检测到 RitsuLib，跳过卡牌异步兼容补丁");
				return;
			}
			MethodInfo methodInfo = assembly.GetType("STS2RitsuLib.Utils.HarmonyIl.HarmonyAsyncTaskBridge", throwOnError: false, ignoreCase: false)?.GetMethod("InvokeContinuation", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[1] { typeof(Action) }, null);
			if (methodInfo == null)
			{
				GD.PrintErr("[EmperorCrabSkinFix] 找不到 RitsuLib HarmonyAsyncTaskBridge.InvokeContinuation(Action)");
				return;
			}
			harmony.Patch((MethodBase)methodInfo, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(RitsuLibCardGuardPatch).GetMethod("Finalizer", BindingFlags.Static | BindingFlags.NonPublic)));
			_installed = true;
			GD.Print("[EmperorCrabSkinFix] 已挂上 RitsuLib 卡牌异步兼容补丁");
		}
		catch (Exception ex)
		{
			GD.PrintErr("[EmperorCrabSkinFix] 安装 RitsuLib 兼容补丁失败：" + ex);
		}
	}

	private static Assembly FindAssembly(string name)
	{
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetName().Name == name)
			{
				return assembly;
			}
		}
		return null;
	}

	private static Exception Finalizer(Exception __exception)
	{
		if (__exception is ObjectDisposedException)
		{
			GD.Print("[EmperorCrabSkinFix] 已忽略 RitsuLib 卡牌异步 continuation 的 ObjectDisposedException");
			return null;
		}
		return __exception;
	}
}

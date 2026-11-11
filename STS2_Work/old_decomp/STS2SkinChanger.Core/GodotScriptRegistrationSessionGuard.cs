using System;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Core;

[HarmonyPatch]
internal static class GodotScriptRegistrationSessionGuard
{
	private static bool Prepare()
	{
		return FindTarget() != null;
	}

	private static MethodBase TargetMethod()
	{
		return FindTarget() ?? throw new MissingMethodException("Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly");
	}

	private static Exception? Finalizer(Assembly __0, Exception __exception)
	{
		if (!GodotScriptPathRegistrationPolicy.IsHarmlessDuplicate(__exception))
		{
			return __exception;
		}
		string text = GodotScriptPathRegistrationPolicy.DuplicatePath(__exception);
		ModLog.Warn("注册 " + __0.GetName().Name + " 的 Godot 场景脚本时遇到重复路径" + ((text == null) ? string.Empty : (" " + text)) + "：已保留先注册的映射并继续本次视觉会话；若该皮肤缺模型或缺交互，请把这条日志连同路径反馈给皮肤作者。");
		return null;
	}

	private static MethodBase? FindTarget()
	{
		return typeof(GodotObject).Assembly.GetType("Godot.Bridge.ScriptManagerBridge")?.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
		{
			if (method.Name.Equals("LookupScriptsInAssembly", StringComparison.Ordinal))
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 1)
				{
					ParameterInfo parameterInfo = parameters[0];
					if (parameterInfo != null)
					{
						Type parameterType = parameterInfo.ParameterType;
						return parameterType == typeof(Assembly);
					}
				}
			}
			return false;
		});
	}
}

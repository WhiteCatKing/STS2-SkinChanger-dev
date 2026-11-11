using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class CombatHudResourceLifecycle
{
	private sealed class HudBinding(MethodInfo attach, NCombatUi ui, CombatState state) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			try
			{
				attach.Invoke(null, new object[2] { ui, state });
			}
			catch (Exception ex)
			{
				ModLog.Warn("重新绑定 CZN 能量界面失败：" + ex.GetBaseException().Message);
			}
		}
	}

	private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string RuntimeName = "CznStyleUI.CombatHud.CombatResourceHudRuntime";

	internal static IDisposable? BeforeEnergyCounterReplacement(NCombatUi ui)
	{
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly candidate) => candidate.GetName().Name == "CznStyleUI");
		if (assembly == null)
		{
			return null;
		}
		Type type = assembly.GetType("CznStyleUI.CombatHud.CombatResourceHudRuntime") ?? throw new MissingMemberException("CZN HUD 接口已变化，已保留原能量控件。");
		FieldInfo? obj = type.GetField("States", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new MissingFieldException("CznStyleUI.CombatHud.CombatResourceHudRuntime", "States");
		MethodInfo methodInfo = obj.FieldType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SingleOrDefault(delegate(MethodInfo method)
		{
			if (method.Name == "TryGetValue" && method.ReturnType == typeof(bool))
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 2)
				{
					ParameterInfo parameterInfo = parameters[0];
					if (parameterInfo != null)
					{
						Type parameterType = parameterInfo.ParameterType;
						ParameterInfo parameterInfo2 = parameters[1];
						if (parameterInfo2 != null && parameterInfo2.IsOut)
						{
							return parameterType == typeof(NCombatUi);
						}
					}
				}
			}
			return false;
		}) ?? throw new MissingMethodException("CznStyleUI.CombatHud.CombatResourceHudRuntime", "States.TryGetValue");
		object value = obj.GetValue(null);
		object[] array = new object[2] { ui, null };
		if (value == null || !(bool)methodInfo.Invoke(value, array) || array[1] == null)
		{
			return null;
		}
		object obj2 = array[1];
		MethodInfo methodInfo2 = RequiredMethod(obj2.GetType(), "RestoreNativeResourceLayout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Array.Empty<Type>());
		MethodInfo methodInfo3 = RequiredMethod(obj2.GetType(), "Unsubscribe", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Array.Empty<Type>());
		MethodInfo attach = RequiredMethod(type, "Attach", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, new Type[2]
		{
			typeof(NCombatUi),
			typeof(CombatState)
		});
		object? obj3 = AccessTools.Field(typeof(NCombatUi), "_state")?.GetValue(ui);
		CombatState state = (CombatState)(((obj3 is CombatState) ? obj3 : null) ?? throw new InvalidOperationException("当前能量界面没有可重新绑定的战斗状态，已保留原控件。"));
		HudBinding hudBinding = new HudBinding(attach, ui, state);
		try
		{
			methodInfo2.Invoke(obj2, null);
			methodInfo3.Invoke(obj2, null);
			return hudBinding;
		}
		catch
		{
			hudBinding.Dispose();
			throw;
		}
	}

	private static MethodInfo RequiredMethod(Type type, string name, BindingFlags flags, Type[] arguments)
	{
		MethodInfo method = type.GetMethod(name, flags, null, arguments, null);
		if (!(method?.ReturnType == typeof(void)))
		{
			throw new MissingMethodException(type.FullName, name);
		}
		return method;
	}
}

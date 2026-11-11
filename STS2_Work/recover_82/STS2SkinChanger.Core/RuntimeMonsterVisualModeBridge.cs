using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class RuntimeMonsterVisualModeBridge
{
	private sealed record ModeBinding(Type ServiceType, Type EnumType, MethodInfo Setter, PropertyInfo? CurrentMode);

	private static readonly object Sync = new object();

	private static readonly Dictionary<string, ModeBinding> Bindings = new Dictionary<string, ModeBinding>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> FailedBindings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> RegisteredAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public static void ApplySelected(string groupId)
	{
		RuntimeMonsterVisualMode selectedRuntimeMonsterVisualMode = SkinService.GetSelectedRuntimeMonsterVisualMode(groupId);
		if (selectedRuntimeMonsterVisualMode == null)
		{
			return;
		}
		lock (Sync)
		{
			string text = selectedRuntimeMonsterVisualMode.AssemblyPath + "\n" + selectedRuntimeMonsterVisualMode.ServiceTypeName + "\n" + selectedRuntimeMonsterVisualMode.EnumTypeName;
			if (FailedBindings.Contains(text))
			{
				return;
			}
			try
			{
				if (!Bindings.TryGetValue(text, out ModeBinding value))
				{
					value = CreateBinding(selectedRuntimeMonsterVisualMode);
					Bindings[text] = value;
				}
				EnsureServiceNode(value);
				object obj = Enum.Parse(value.EnumType, selectedRuntimeMonsterVisualMode.ModeName, ignoreCase: false);
				if (!object.Equals(value.CurrentMode?.GetValue(null), obj))
				{
					value.Setter.Invoke(null, new object[1] { obj });
				}
			}
			catch (Exception value2)
			{
				FailedBindings.Add(text);
				ModLog.Error($"应用 {selectedRuntimeMonsterVisualMode.ProviderId} 的运行时外观模式 {selectedRuntimeMonsterVisualMode.ModeName} 失败：{value2}");
			}
		}
	}

	private static ModeBinding CreateBinding(RuntimeMonsterVisualMode mode)
	{
		Assembly orLoadAssembly = GetOrLoadAssembly(mode.AssemblyPath);
		RegisterGodotScripts(orLoadAssembly, mode.AssemblyPath);
		Type type = orLoadAssembly.GetType(mode.ServiceTypeName, throwOnError: true);
		Type type2 = orLoadAssembly.GetType(mode.EnumTypeName, throwOnError: true);
		if (!type2.IsEnum || !typeof(Node).IsAssignableFrom(type))
		{
			throw new InvalidOperationException(mode.ServiceTypeName + "/" + mode.EnumTypeName + " 不是可用的 Godot 外观模式服务。");
		}
		MethodInfo setter = type.GetMethod(mode.SetterName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { type2 }, null) ?? throw new MissingMethodException(type.FullName, mode.SetterName);
		PropertyInfo property = type.GetProperty("CurrentMode", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		return new ModeBinding(type, type2, setter, property);
	}

	private static Assembly GetOrLoadAssembly(string assemblyPath)
	{
		string fullPath = Path.GetFullPath(assemblyPath);
		Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(delegate(Assembly candidate)
		{
			try
			{
				return !candidate.IsDynamic && Path.GetFullPath(candidate.Location).Equals(fullPath, StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		});
		if (assembly != null)
		{
			return assembly;
		}
		return AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly())?.LoadFromAssemblyPath(fullPath) ?? Assembly.LoadFrom(fullPath);
	}

	private static void RegisterGodotScripts(Assembly assembly, string assemblyPath)
	{
		if (!RegisteredAssemblies.Add(assemblyPath))
		{
			return;
		}
		try
		{
			(typeof(GodotObject).Assembly.GetType("Godot.Bridge.ScriptManagerBridge")?.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault(delegate(MethodInfo method)
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
			}))?.Invoke(null, new object[1] { assembly });
		}
		catch
		{
			RegisteredAssemblies.Remove(assemblyPath);
			throw;
		}
	}

	private static void EnsureServiceNode(ModeBinding binding)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
		Window val = ((obj != null) ? ((SceneTree)obj).Root : null) ?? throw new InvalidOperationException("Godot 场景树尚未创建。");
		if (!((IEnumerable<Node>)((Node)val).GetChildren(false)).Any(binding.ServiceType.IsInstanceOfType))
		{
			object? obj2 = Activator.CreateInstance(binding.ServiceType);
			Node val2 = (Node)(((obj2 is Node) ? obj2 : null) ?? throw new InvalidOperationException("无法创建外观模式服务 " + binding.ServiceType.FullName + "。"));
			((Node)val).AddChild(val2, false, (InternalMode)0);
		}
	}
}

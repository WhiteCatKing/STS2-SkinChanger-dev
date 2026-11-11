using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;

namespace STS2SkinChanger.Core;

internal static class ProviderModelDiscovery
{
	private static readonly object Sync = new object();

	private static readonly HashSet<Assembly> Published = new HashSet<Assembly>();

	private static readonly List<object> SingletonKeepAlive = new List<object>();

	private static readonly FieldInfo? ModTypesField = typeof(ReflectionHelper).GetField("_modTypes", BindingFlags.Static | BindingFlags.NonPublic);

	public static void Publish(Assembly? assembly, string providerId)
	{
		if (assembly == null)
		{
			return;
		}
		lock (Sync)
		{
			if (!Published.Add(assembly))
			{
				return;
			}
			Mod val = FindOwningMod(assembly, providerId);
			if (val != null)
			{
				try
				{
					PublishAssemblyToMod(val, assembly);
					ModTypesField?.SetValue(null, null);
				}
				catch (Exception ex)
				{
					ModLog.Warn("把皮肤提供者 " + providerId + " 的程序集并入模型类型发现失败：" + ex.GetBaseException().Message);
				}
			}
			InstantiateMissingSingletons(assembly, providerId);
		}
	}

	public static void InstantiatePublishedAfterInit()
	{
		lock (Sync)
		{
			foreach (Assembly item in Published)
			{
				InstantiateMissingSingletons(item, item.GetName().Name ?? "provider");
			}
		}
	}

	private static void PublishAssemblyToMod(Mod mod, Assembly assembly)
	{
		Type type = ((object)mod).GetType();
		if (type.GetField("assemblies", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(mod) is IList list)
		{
			if (!list.Contains(assembly))
			{
				list.Add(assembly);
				ModLog.Info("已把皮肤提供者 " + mod.manifest?.id + " 的程序集纳入模型类型发现，其单例/战斗钩子可按原版注册。");
			}
			return;
		}
		FieldInfo field = type.GetField("assembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (field == null)
		{
			return;
		}
		if (field.GetValue(mod) is Assembly assembly2)
		{
			if (assembly2 != assembly)
			{
				ModLog.Warn($"皮肤提供者 {mod.manifest?.id} 已有程序集 {assembly2.GetName().Name}，无法再挂入 {assembly.GetName().Name}；请检查是否重复加载。");
			}
		}
		else
		{
			field.SetValue(mod, assembly);
			ModLog.Info("已把皮肤提供者 " + mod.manifest?.id + " 的程序集纳入模型类型发现，其单例/战斗钩子可按原版注册。");
		}
	}

	private static Mod? FindOwningMod(Assembly assembly, string providerId)
	{
		string text = (string.IsNullOrWhiteSpace(assembly.Location) ? null : Path.GetFullPath(assembly.Location));
		foreach (Mod loadedMod in ModManager.GetLoadedMods())
		{
			if (text != null && !string.IsNullOrWhiteSpace(loadedMod.path))
			{
				string text2 = Path.GetFullPath(loadedMod.path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				if (text.StartsWith(text2 + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					return loadedMod;
				}
			}
			if (string.Equals(loadedMod.manifest?.id, providerId, StringComparison.OrdinalIgnoreCase))
			{
				return loadedMod;
			}
		}
		return null;
	}

	private static void InstantiateMissingSingletons(Assembly assembly, string providerId)
	{
		if (!ModelDb.Contains(typeof(Ironclad)))
		{
			return;
		}
		foreach (Type item in SafeTypes(assembly))
		{
			if (item == null || !item.IsClass || item.IsAbstract || !typeof(SingletonModel).IsAssignableFrom(item) || ModelDb.Contains(item))
			{
				continue;
			}
			try
			{
				object obj = Activator.CreateInstance(item);
				if (obj != null)
				{
					SingletonKeepAlive.Add(obj);
					ModLog.Info($"已补注册皮肤提供者 {providerId} 的单例模型 {item.Name}，其战斗/房间钩子已启用。");
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn($"补注册皮肤提供者 {providerId} 的单例模型 {item.Name} 失败：" + ex.GetBaseException().Message);
			}
		}
	}

	private static IEnumerable<Type?> SafeTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types;
		}
		catch
		{
			return Array.Empty<Type>();
		}
	}
}

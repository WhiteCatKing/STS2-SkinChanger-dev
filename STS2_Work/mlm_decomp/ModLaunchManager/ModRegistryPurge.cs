using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModRegistryPurge
{
	private const int NodeScanLimit = 40000;

	private const int SampleTypeCount = 3;

	private static readonly FieldInfo? PoolContentField = AccessTools.Field(typeof(ModHelper), "_moddedContentForPools");

	private static readonly FieldInfo? RunHookSubscribersField = AccessTools.Field(typeof(ModHelper), "_runHookSubscribers");

	private static readonly FieldInfo? CombatHookSubscribersField = AccessTools.Field(typeof(ModHelper), "_combatHookSubscribers");

	private static readonly FieldInfo? ModTypesCacheField = AccessTools.Field(typeof(ReflectionHelper), "_modTypes");

	private static readonly Type? AssemblyInfoType = typeof(ModManager).Assembly.GetType("MegaCrit.Sts2.Core.Modding.AssemblyInfo", throwOnError: false);

	private static readonly PropertyInfo? AssemblyModMapProperty = AssemblyInfoType?.GetProperty("ModMap", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo? AssemblyModMapField = ((AssemblyInfoType == null) ? null : (AccessTools.Field(AssemblyInfoType, "<ModMap>k__BackingField") ?? AccessTools.Field(AssemblyInfoType, "ModMap")));

	internal static RegistryPurgeReport DropReferences(IReadOnlyCollection<Assembly> assemblies)
	{
		HashSet<Assembly> owned = new HashSet<Assembly>(assemblies, ReferenceEqualityComparer.Instance);
		List<string> failures = new List<string>();
		return new RegistryPurgeReport(0 + Guarded(() => PurgePools(owned), "ModHelper._moddedContentForPools", failures) + Guarded(() => PurgeSubscribers(RunHookSubscribersField, owned), "ModHelper._runHookSubscribers", failures) + Guarded(() => PurgeSubscribers(CombatHookSubscribersField, owned), "ModHelper._combatHookSubscribers", failures) + Guarded(() => PurgeAssemblyMap(owned), "AssemblyInfo.ModMap", failures) + Guarded(() => PurgeModTypeCache(owned), "ReflectionHelper._modTypes", failures) + PurgeDelegateFields(typeof(ModManager), null, owned, failures) + Guarded(() => PurgeDelegateFields(typeof(CombatManager), CombatManager.Instance, owned, failures), "CombatManager events", failures) + PurgeDelegateFields(typeof(AppDomain), AppDomain.CurrentDomain, owned, failures) + PurgeDelegateFields(typeof(AssemblyLoadContext), AssemblyLoadContext.Default, owned, failures) + Guarded(() => JsonRuntimeCachePurge.DropReferences(owned), "System.Text.Json reflection accessor cache", failures), failures);
	}

	internal static IReadOnlyList<string> DescribeRetainers(IReadOnlyCollection<WeakReference> contextReferences)
	{
		HashSet<AssemblyLoadContext> hashSet = ModRootScanner.LiveContexts(contextReferences);
		if (hashSet.Count == 0)
		{
			return Array.Empty<string>();
		}
		List<string> list = new List<string>();
		try
		{
			var (num, item) = CountLiveNodes(hashSet);
			if (num > 0)
			{
				list.Add(MlmLocManager.Get("loading.error.unload.retainerNodes", ("count", num), ("types", item)));
			}
		}
		catch
		{
		}
		try
		{
			int num2 = CountRemainingPatches(hashSet);
			if (num2 > 0)
			{
				list.Add(MlmLocManager.Get("loading.error.unload.retainerHarmony", ("count", num2)));
			}
		}
		catch
		{
		}
		return list;
	}

	private static int Guarded(Func<int> step, string description, ICollection<string> failures)
	{
		try
		{
			return step();
		}
		catch (Exception ex)
		{
			failures.Add(description + ": " + ex.GetType().Name);
			return 0;
		}
	}

	private static int PurgePools(HashSet<Assembly> owned)
	{
		if (!(PoolContentField?.GetValue(null) is IDictionary dictionary))
		{
			return 0;
		}
		int num = 0;
		List<object> list = new List<object>();
		foreach (DictionaryEntry item in dictionary)
		{
			if (item.Key is Type type && owned.Contains(type.Assembly))
			{
				list.Add(item.Key);
				num++;
			}
			else
			{
				if (item.Value == null || !(AccessTools.Field(item.Value.GetType(), "modelsToAdd")?.GetValue(item.Value) is IList list2))
				{
					continue;
				}
				for (int num2 = list2.Count - 1; num2 >= 0; num2--)
				{
					if (list2[num2] is Type type2 && owned.Contains(type2.Assembly))
					{
						list2.RemoveAt(num2);
						num++;
					}
				}
			}
		}
		foreach (object item2 in list)
		{
			dictionary.Remove(item2);
		}
		return num;
	}

	private static int PurgeSubscribers(FieldInfo? field, HashSet<Assembly> owned)
	{
		if (!(field?.GetValue(null) is IList list))
		{
			return 0;
		}
		int num = 0;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			object obj = list[num2];
			if (obj != null && AccessTools.Field(obj.GetType(), "del")?.GetValue(obj) is Delegate handler && IsOwned(handler, owned))
			{
				list.RemoveAt(num2);
				num++;
			}
		}
		return num;
	}

	private static int PurgeAssemblyMap(HashSet<Assembly> owned)
	{
		if (!((AssemblyModMapProperty?.GetValue(null) ?? AssemblyModMapField?.GetValue(null)) is IDictionary dictionary))
		{
			return 0;
		}
		int num = 0;
		foreach (Assembly item in owned)
		{
			if (dictionary.Contains(item))
			{
				dictionary.Remove(item);
				num++;
			}
		}
		return num;
	}

	private static int PurgeModTypeCache(HashSet<Assembly> owned)
	{
		if (!(ModTypesCacheField?.GetValue(null) is Type[] source) || !source.Any((Type type) => owned.Contains(type.Assembly)))
		{
			return 0;
		}
		ModTypesCacheField.SetValue(null, null);
		return 1;
	}

	private static int PurgeDelegateFields(Type type, object? instance, HashSet<Assembly> owned, ICollection<string> failures)
	{
		int num = 0;
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo.IsLiteral || fieldInfo.IsInitOnly || !typeof(MulticastDelegate).IsAssignableFrom(fieldInfo.FieldType) || (!fieldInfo.IsStatic && instance == null))
			{
				continue;
			}
			object obj = (fieldInfo.IsStatic ? null : instance);
			try
			{
				if (!(fieldInfo.GetValue(obj) is Delegate @delegate))
				{
					continue;
				}
				Delegate delegate2 = null;
				int num2 = 0;
				Delegate[] invocationList = @delegate.GetInvocationList();
				foreach (Delegate delegate3 in invocationList)
				{
					if (IsOwned(delegate3, owned))
					{
						num2++;
					}
					else
					{
						delegate2 = (((object)delegate2 == null) ? delegate3 : Delegate.Combine(delegate2, delegate3));
					}
				}
				if (num2 != 0)
				{
					fieldInfo.SetValue(obj, delegate2);
					num += num2;
				}
			}
			catch (Exception ex)
			{
				failures.Add($"{type.Name}.{fieldInfo.Name}: {ex.GetType().Name}");
			}
		}
		return num;
	}

	private static bool IsOwned(Delegate handler, HashSet<Assembly> owned)
	{
		return handler.GetInvocationList().Any((Delegate entry) => owned.Contains(entry.Method.Module.Assembly) || (entry.Target != null && owned.Contains(entry.Target.GetType().Assembly)));
	}

	private static (int Count, string Sample) CountLiveNodes(HashSet<AssemblyLoadContext> owned)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val.Root))
		{
			return (Count: 0, Sample: string.Empty);
		}
		int num = 0;
		int num2 = 0;
		List<string> list = new List<string>();
		Queue<Node> queue = new Queue<Node>();
		queue.Enqueue((Node)(object)val.Root);
		while (queue.Count > 0 && num2 < 40000)
		{
			Node obj = queue.Dequeue();
			num2++;
			Type type = ((object)obj).GetType();
			if (ModRootScanner.IsOwned(type.Assembly, owned))
			{
				num++;
				if (list.Count < 3 && !list.Contains(type.Name))
				{
					list.Add(type.Name);
				}
			}
			foreach (Node child in obj.GetChildren(false))
			{
				queue.Enqueue(child);
			}
		}
		return (Count: num, Sample: string.Join(", ", list));
	}

	private static int CountRemainingPatches(HashSet<AssemblyLoadContext> owned)
	{
		int num = 0;
		MethodBase[] array = Harmony.GetAllPatchedMethods().ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Patches patchInfo = Harmony.GetPatchInfo(array[i]);
			if (patchInfo != null)
			{
				num += patchInfo.Prefixes.Concat(patchInfo.Postfixes).Concat(patchInfo.Transpilers).Concat(patchInfo.Finalizers)
					.Concat(patchInfo.InnerPrefixes)
					.Concat(patchInfo.InnerPostfixes)
					.Count((Patch patch) => ModRootScanner.IsOwned(patch.PatchMethod.Module.Assembly, owned));
			}
		}
		return num;
	}
}

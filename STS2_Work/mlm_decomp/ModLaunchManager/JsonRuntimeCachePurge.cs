using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ModLaunchManager;

internal static class JsonRuntimeCachePurge
{
	internal static int DropReferences(IReadOnlyCollection<Assembly> owned)
	{
		int num = 0;
		Func<object, bool> func = ModRootScanner.OwnedReferencePredicate(owned);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (assembly.GetName().Name != "System.Text.Json" || owned.Contains(assembly))
			{
				continue;
			}
			num += PurgeOptions(assembly, func);
			Type type = assembly.GetType("System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver");
			if (type == null || type.TypeInitializer != null)
			{
				continue;
			}
			object obj = type.GetField("s_memberAccessor", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null);
			if (obj?.GetType().FullName != "System.Text.Json.Serialization.Metadata.ReflectionEmitCachingMemberAccessor")
			{
				continue;
			}
			object obj2 = ReadCache(obj);
			if (obj2 == null || !(ReadCache(obj2) is IDictionary dictionary))
			{
				continue;
			}
			object[] array = dictionary.Keys.Cast<object>().ToArray();
			foreach (object obj3 in array)
			{
				if (func(obj3))
				{
					dictionary.Remove(obj3);
					num++;
				}
			}
		}
		return num;
	}

	private static int PurgeOptions(Assembly library, Func<object, bool> isOwned)
	{
		if (!(library.GetType("System.Text.Json.JsonSerializerOptions+TrackedOptionsInstances")?.GetField("<All>k__BackingField", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) is IEnumerable enumerable))
		{
			return 0;
		}
		int num = 0;
		foreach (object item in enumerable)
		{
			object obj = item.GetType().GetProperty("Key")?.GetValue(item);
			if (obj == null)
			{
				continue;
			}
			Type type = obj.GetType();
			object obj2 = type.GetField("_cachingContext", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(obj);
			if (obj2 != null && ReadCache(obj2) is IDictionary dictionary)
			{
				object[] array = dictionary.Keys.Cast<object>().ToArray();
				foreach (object obj3 in array)
				{
					if (isOwned(obj3))
					{
						dictionary.Remove(obj3);
						num++;
					}
				}
			}
			string[] array2 = new string[2] { "_lastTypeInfo", "_objectTypeInfo" };
			foreach (string name in array2)
			{
				FieldInfo field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
				object obj4 = field?.GetValue(obj);
				if (obj4?.GetType().GetProperty("Type")?.GetValue(obj4) is Type arg && isOwned(arg))
				{
					field.SetValue(obj, null);
					num++;
				}
			}
		}
		return num;
	}

	private static object? ReadCache(object owner)
	{
		return owner.GetType().GetField("_cache", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner);
	}
}

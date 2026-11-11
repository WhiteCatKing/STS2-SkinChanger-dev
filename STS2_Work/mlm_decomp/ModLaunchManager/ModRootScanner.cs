using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class ModRootScanner
{
	private readonly record struct GraphInspection(int References, int Removed, int Remaining)
	{
		internal static GraphInspection Empty => default(GraphInspection);

		internal GraphInspection Add(GraphInspection other)
		{
			return new GraphInspection(References + other.References, Removed + other.Removed, Remaining + other.Remaining);
		}
	}

	private sealed class GraphContext(HashSet<AssemblyLoadContext> owned, HashSet<Assembly> traversableAssemblies, bool prune)
	{
		internal HashSet<AssemblyLoadContext> Owned { get; } = owned;

		internal HashSet<Assembly> TraversableAssemblies { get; } = traversableAssemblies;

		internal bool Prune { get; } = prune;

		internal int NodesLeft { get; set; } = 16384;

		internal bool Truncated { get; set; }

		internal HashSet<object> Path { get; } = new HashSet<object>(ReferenceEqualityComparer.Instance);
	}

	private const int MaxGraphDepth = 14;

	private const int MaxGraphNodesPerField = 16384;

	internal static Func<object, bool> OwnedReferencePredicate(IReadOnlyCollection<Assembly> assemblies)
	{
		HashSet<AssemblyLoadContext> owned = new HashSet<AssemblyLoadContext>(ReferenceEqualityComparer.Instance);
		foreach (Assembly assembly in assemblies)
		{
			AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(assembly);
			if (loadContext != null)
			{
				owned.Add(loadContext);
			}
		}
		HashSet<Assembly> traversable = new HashSet<Assembly>(ScanTargets(owned), ReferenceEqualityComparer.Instance);
		return delegate(object value)
		{
			traversable.Add(value.GetType().Assembly);
			GraphContext context = new GraphContext(owned, traversable, prune: false);
			return Inspect(value, context, 0, allowPrune: false).References > 0;
		};
	}

	internal static IReadOnlyList<ModRootHit> Sweep(IReadOnlyCollection<WeakReference> ownedContextReferences, bool prune)
	{
		HashSet<AssemblyLoadContext> hashSet = LiveContexts(ownedContextReferences);
		if (hashSet.Count == 0)
		{
			return Array.Empty<ModRootHit>();
		}
		Assembly[] array = ScanTargets(hashSet);
		HashSet<Assembly> traversableAssemblies = new HashSet<Assembly>(array, ReferenceEqualityComparer.Instance);
		List<ModRootHit> list = new List<ModRootHit>();
		Assembly[] array2 = array;
		foreach (Assembly obj in array2)
		{
			string holderAssembly = obj.GetName().Name ?? "<unnamed>";
			foreach (Type item in TypesOf(obj))
			{
				if (item.IsGenericTypeDefinition || item.ContainsGenericParameters || !RuntimeTypeInitialization.CanReadStatics(item))
				{
					continue;
				}
				FieldInfo[] fields = item.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (FieldInfo fieldInfo in fields)
				{
					if (fieldInfo.IsLiteral || IsUninteresting(fieldInfo.FieldType))
					{
						continue;
					}
					try
					{
						object value = fieldInfo.GetValue(null);
						if (value != null)
						{
							GraphContext context = new GraphContext(hashSet, traversableAssemblies, prune);
							GraphInspection graphInspection = InspectField(fieldInfo, null, value, context, 0, allowPrune: true);
							if (graphInspection.References > 0)
							{
								list.Add(new ModRootHit(holderAssembly, item.Name + "." + fieldInfo.Name, graphInspection.References, graphInspection.Removed));
							}
						}
					}
					catch
					{
					}
				}
			}
		}
		return list;
	}

	private static Assembly[] ScanTargets(HashSet<AssemblyLoadContext> owned)
	{
		List<Assembly> targets = new List<Assembly>();
		HashSet<Assembly> seen = new HashSet<Assembly>(ReferenceEqualityComparer.Instance);
		Add(typeof(ModRootScanner).Assembly);
		foreach (Mod mod in ModManager.Mods)
		{
			Assembly[] recordedAssemblies = CollectibleModAssemblyLoader.GetRecordedAssemblies(mod);
			foreach (Assembly assembly2 in recordedAssemblies)
			{
				Add(assembly2);
				try
				{
					AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(assembly2);
					if (loadContext == null || loadContext == AssemblyLoadContext.Default)
					{
						continue;
					}
					foreach (Assembly assembly3 in loadContext.Assemblies)
					{
						Add(assembly3);
					}
				}
				catch
				{
				}
			}
		}
		return targets.ToArray();
		void Add(Assembly? assembly)
		{
			if (!(assembly == null) && !assembly.IsDynamic && !IsOwned(assembly, owned) && !IsHostAssembly(assembly) && seen.Add(assembly))
			{
				targets.Add(assembly);
			}
		}
	}

	private static bool IsHostAssembly(Assembly assembly)
	{
		string text = assembly.GetName().Name ?? string.Empty;
		bool flag;
		switch (text)
		{
		case "0Harmony":
		case "mscorlib":
		case "sts2":
		case "GodotSharp":
		case "GodotSharpEditor":
		case "Steamworks.NET":
		case "netstandard":
		case "System":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (!flag && !text.StartsWith("System.", StringComparison.Ordinal))
		{
			return text.StartsWith("Microsoft.", StringComparison.Ordinal);
		}
		return true;
	}

	private static IEnumerable<Type> TypesOf(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			return ex.Types.Where((Type type) => type != null);
		}
		catch
		{
			return Array.Empty<Type>();
		}
	}

	private static bool IsUninteresting(Type fieldType)
	{
		if (!fieldType.IsPrimitive && !fieldType.IsEnum && !(fieldType == typeof(string)))
		{
			return fieldType == typeof(decimal);
		}
		return true;
	}

	private static GraphInspection Inspect(object value, GraphContext context, int depth, bool allowPrune)
	{
		if (!(value is Type type))
		{
			if (!(value is Assembly assembly))
			{
				if (value is MemberInfo member)
				{
					return OwnedResult(IsOwned(member, context.Owned));
				}
				Type type2 = value.GetType();
				int num = (IsOwned(type2, context.Owned) ? 1 : 0);
				if (!TryEnter(value, context, depth, out var tracked))
				{
					if (num != 0)
					{
						return new GraphInspection(num, 0, num);
					}
					return GraphInspection.Empty;
				}
				try
				{
					if (value is WeakReference || (type2.IsGenericType && type2.GetGenericTypeDefinition() == typeof(WeakReference<>)))
					{
						return new GraphInspection(num, 0, num);
					}
					if (value is Delegate handler)
					{
						return InspectDelegate(handler, context, depth, allowPrune, num);
					}
					if (value is IDictionary dictionary)
					{
						return InspectDictionary(dictionary, context, depth, allowPrune, num);
					}
					if (value is IList list)
					{
						return InspectList(list, context, depth, allowPrune, num);
					}
					Type type3 = FindGenericInterface(type2, typeof(IDictionary<, >));
					if (type3 != null)
					{
						return InspectGenericDictionary(value, type3, context, depth, allowPrune, num);
					}
					Type type4 = FindGenericInterface(type2, typeof(ICollection<>));
					if (type4 != null)
					{
						return InspectGenericCollection(value, type4, context, depth, allowPrune, num);
					}
					if (value is IEnumerable sequence && IsMaterializedCollection(sequence))
					{
						return InspectReadOnlySequence(sequence, context, depth, allowPrune, num);
					}
					if (num > 0)
					{
						return new GraphInspection(num, 0, num);
					}
					if (!CanTraverseObject(type2, context))
					{
						return GraphInspection.Empty;
					}
					return InspectObjectFields(value, type2, context, depth, allowPrune);
				}
				finally
				{
					if (tracked)
					{
						context.Path.Remove(value);
					}
				}
			}
			return OwnedResult(IsOwned(assembly, context.Owned));
		}
		return OwnedResult(IsOwned(type, context.Owned));
	}

	private static GraphInspection InspectDictionary(IDictionary dictionary, GraphContext context, int depth, bool allowPrune, int selfReferences)
	{
		List<(object, object)> list = new List<(object, object)>();
		foreach (DictionaryEntry item3 in dictionary)
		{
			list.Add((item3.Key, item3.Value));
		}
		bool flag = context.Prune && allowPrune && !dictionary.IsReadOnly && !dictionary.IsFixedSize;
		int num = selfReferences;
		int num2 = 0;
		int num3 = selfReferences;
		foreach (var item4 in list)
		{
			object item = item4.Item1;
			object item2 = item4.Item2;
			GraphInspection graphInspection = ((item == null) ? GraphInspection.Empty : Inspect(item, context, depth + 1, allowPrune: false));
			GraphInspection graphInspection2 = ((item2 == null) ? GraphInspection.Empty : Inspect(item2, context, depth + 1, allowPrune));
			num += graphInspection.References + graphInspection2.References;
			num2 += graphInspection.Removed + graphInspection2.Removed;
			int num4 = graphInspection.Remaining + graphInspection2.Remaining;
			if (num4 == 0)
			{
				continue;
			}
			if (flag && (graphInspection.Remaining > 0 || CanRemoveEntry(item2, context)))
			{
				try
				{
					dictionary.Remove(item);
					num2++;
				}
				catch
				{
					goto IL_0136;
				}
				continue;
			}
			goto IL_0136;
			IL_0136:
			num3 += num4;
		}
		return new GraphInspection(num, num2, num3);
	}

	private static GraphInspection InspectList(IList list, GraphContext context, int depth, bool allowPrune, int selfReferences)
	{
		bool flag = context.Prune && allowPrune && !list.IsReadOnly && !list.IsFixedSize;
		int num = selfReferences;
		int num2 = 0;
		int num3 = selfReferences;
		for (int num4 = list.Count - 1; num4 >= 0; num4--)
		{
			object obj = list[num4];
			GraphInspection graphInspection = ((obj == null) ? GraphInspection.Empty : Inspect(obj, context, depth + 1, allowPrune));
			num += graphInspection.References;
			num2 += graphInspection.Removed;
			if (graphInspection.Remaining == 0)
			{
				continue;
			}
			if (flag && CanRemoveEntry(obj, context))
			{
				try
				{
					list.RemoveAt(num4);
					num2++;
				}
				catch
				{
					goto IL_0091;
				}
				continue;
			}
			goto IL_0091;
			IL_0091:
			num3 += graphInspection.Remaining;
		}
		return new GraphInspection(num, num2, num3);
	}

	private static GraphInspection InspectGenericDictionary(object dictionary, Type dictionaryInterface, GraphContext context, int depth, bool allowPrune, int selfReferences)
	{
		Type[] genericArguments = dictionaryInterface.GetGenericArguments();
		Type type = typeof(KeyValuePair<, >).MakeGenericType(genericArguments);
		Type type2 = typeof(ICollection<>).MakeGenericType(type);
		bool flag = true;
		try
		{
			flag = ((bool?)type2.GetProperty("IsReadOnly")?.GetValue(dictionary)) ?? true;
		}
		catch
		{
		}
		MethodInfo method = dictionaryInterface.GetMethod("Remove", new Type[1] { genericArguments[0] });
		bool flag2 = context.Prune && allowPrune && !flag && method != null;
		List<object> list = ((IEnumerable)dictionary).Cast<object>().ToList();
		int num = selfReferences;
		int num2 = 0;
		int num3 = selfReferences;
		foreach (object item in list)
		{
			Type type3 = item.GetType();
			object obj2 = type3.GetProperty("Key")?.GetValue(item);
			object obj3 = type3.GetProperty("Value")?.GetValue(item);
			GraphInspection graphInspection = ((obj2 == null) ? GraphInspection.Empty : Inspect(obj2, context, depth + 1, allowPrune: false));
			GraphInspection graphInspection2 = ((obj3 == null) ? GraphInspection.Empty : Inspect(obj3, context, depth + 1, allowPrune));
			num += graphInspection.References + graphInspection2.References;
			num2 += graphInspection.Removed + graphInspection2.Removed;
			int num4 = graphInspection.Remaining + graphInspection2.Remaining;
			if (num4 != 0)
			{
				if (flag2 && (graphInspection.Remaining > 0 || CanRemoveEntry(obj3, context)) && TryInvokeRemove(method, dictionary, obj2))
				{
					num2++;
				}
				else
				{
					num3 += num4;
				}
			}
		}
		return new GraphInspection(num, num2, num3);
	}

	private static GraphInspection InspectGenericCollection(object collection, Type collectionInterface, GraphContext context, int depth, bool allowPrune, int selfReferences)
	{
		Type type = collectionInterface.GetGenericArguments()[0];
		bool flag = true;
		try
		{
			flag = ((bool?)collectionInterface.GetProperty("IsReadOnly")?.GetValue(collection)) ?? true;
		}
		catch
		{
		}
		MethodInfo method = collectionInterface.GetMethod("Remove", new Type[1] { type });
		bool flag2 = context.Prune && allowPrune && !flag && method != null;
		List<object> list = ((IEnumerable)collection).Cast<object>().ToList();
		int num = selfReferences;
		int num2 = 0;
		int num3 = selfReferences;
		foreach (object item in list)
		{
			GraphInspection graphInspection = ((item == null) ? GraphInspection.Empty : Inspect(item, context, depth + 1, allowPrune));
			num += graphInspection.References;
			num2 += graphInspection.Removed;
			if (graphInspection.Remaining != 0)
			{
				if (flag2 && CanRemoveEntry(item, context) && TryInvokeRemove(method, collection, item))
				{
					num2++;
				}
				else
				{
					num3 += graphInspection.Remaining;
				}
			}
		}
		return new GraphInspection(num, num2, num3);
	}

	private static GraphInspection InspectReadOnlySequence(IEnumerable sequence, GraphContext context, int depth, bool allowPrune, int selfReferences)
	{
		GraphInspection result = new GraphInspection(selfReferences, 0, selfReferences);
		foreach (object item in sequence)
		{
			if (item != null)
			{
				result = result.Add(Inspect(item, context, depth + 1, allowPrune));
			}
		}
		return result;
	}

	private static GraphInspection InspectObjectFields(object value, Type runtimeType, GraphContext context, int depth, bool allowPrune)
	{
		GraphInspection result = GraphInspection.Empty;
		bool allowPrune2 = allowPrune && !IsRegistrationRecord(runtimeType) && !IsOwnedRegistrationWrapper(value, context);
		for (Type type = runtimeType; type != null && type != typeof(object); type = type.BaseType)
		{
			if (!CanTraverseDeclaredFields(type, context))
			{
				continue;
			}
			FieldInfo[] fields;
			try
			{
				fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			}
			catch
			{
				continue;
			}
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (fieldInfo.IsStatic || fieldInfo.IsLiteral || IsUninteresting(fieldInfo.FieldType))
				{
					continue;
				}
				try
				{
					object value2 = fieldInfo.GetValue(value);
					if (value2 != null)
					{
						result = result.Add(InspectField(fieldInfo, value, value2, context, depth + 1, allowPrune2));
					}
				}
				catch
				{
				}
			}
		}
		return result;
	}

	private static GraphInspection InspectField(FieldInfo field, object? owner, object value, GraphContext context, int depth, bool allowPrune)
	{
		if (context.Prune && allowPrune && !field.IsInitOnly)
		{
			if (value is Delegate handler)
			{
				return PruneDelegateField(field, owner, handler, context);
			}
			if (value is Array { Rank: 1 } array && array.GetLowerBound(0) == 0 && !IsOwned(array.GetType(), context.Owned) && array.Cast<object>().All((object item) => item == null || IsRegistrationValue(item)))
			{
				return PruneRegistrationArray(field, owner, array, context, depth);
			}
		}
		return Inspect(value, context, depth, allowPrune);
	}

	private static GraphInspection PruneRegistrationArray(FieldInfo field, object? owner, Array array, GraphContext context, int depth)
	{
		List<object> list = new List<object>();
		int num = 0;
		int num2 = 0;
		foreach (object item in array)
		{
			if (item is Delegate @delegate)
			{
				Delegate delegate2 = null;
				Delegate[] invocationList = @delegate.GetInvocationList();
				foreach (Delegate delegate3 in invocationList)
				{
					GraphInspection graphInspection = Inspect(delegate3, context, depth + 1, allowPrune: false);
					num += graphInspection.References;
					if (graphInspection.Remaining > 0)
					{
						num2++;
					}
					else
					{
						delegate2 = Delegate.Combine(delegate2, delegate3);
					}
				}
				if ((object)delegate2 != null)
				{
					list.Add(delegate2);
				}
			}
			else
			{
				GraphInspection graphInspection2 = ((item == null) ? GraphInspection.Empty : Inspect(item, context, depth + 1, allowPrune: false));
				num += graphInspection2.References;
				if (graphInspection2.Remaining == 0)
				{
					list.Add(item);
				}
				else
				{
					num2++;
				}
			}
		}
		if (num2 == 0)
		{
			return new GraphInspection(num, 0, 0);
		}
		Array array2 = Array.CreateInstance(array.GetType().GetElementType(), list.Count);
		for (int j = 0; j < list.Count; j++)
		{
			array2.SetValue(list[j], j);
		}
		if (!ReferenceFieldUpdate.TryReplace(field, owner, array, array2))
		{
			return new GraphInspection(num, 0, num);
		}
		return new GraphInspection(num, num2, 0);
	}

	private static bool IsRegistrationValue(object value)
	{
		if ((!(value is Delegate) && !(value is MemberInfo) && !(value is Assembly)) || 1 == 0)
		{
			return IsRegistrationRecord(value.GetType());
		}
		return true;
	}

	private static bool IsRegistrationRecord(Type type, int depth = 0)
	{
		if (depth > 3 || type.IsPrimitive || type.IsEnum || type == typeof(string) || typeof(IEnumerable).IsAssignableFrom(type) || IsFrameworkCarrier(type) || type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length != 0)
		{
			return false;
		}
		bool result = false;
		Type type2 = type;
		while (type2 != null && type2 != typeof(object))
		{
			FieldInfo[] fields = type2.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			for (int i = 0; i < fields.Length; i++)
			{
				Type fieldType = fields[i].FieldType;
				if (IsUninteresting(fieldType))
				{
					continue;
				}
				if (typeof(IEnumerable).IsAssignableFrom(fieldType))
				{
					return false;
				}
				if (typeof(Delegate).IsAssignableFrom(fieldType) || typeof(MemberInfo).IsAssignableFrom(fieldType) || typeof(Assembly).IsAssignableFrom(fieldType))
				{
					result = true;
					continue;
				}
				if (IsRegistrationRecord(fieldType, depth + 1))
				{
					result = true;
					continue;
				}
				return false;
			}
			type2 = type2.BaseType;
		}
		return result;
	}

	private static bool CanRemoveEntry(object? value, GraphContext context)
	{
		if (value != null)
		{
			if (!IsOwned(value.GetType(), context.Owned) && !IsRegistrationValue(value))
			{
				return IsOwnedRegistrationWrapper(value, context);
			}
			return true;
		}
		return false;
	}

	private static bool IsOwnedRegistrationWrapper(object value, GraphContext context)
	{
		int budget = 64;
		bool ownsPayload;
		return InspectRegistrationPayload(value, context, 0, ref budget, out ownsPayload) && ownsPayload;
	}

	private static bool InspectRegistrationPayload(object? value, GraphContext context, int depth, ref int budget, out bool ownsPayload)
	{
		ownsPayload = false;
		if (depth > 4 || --budget < 0)
		{
			return false;
		}
		if (value == null)
		{
			return true;
		}
		Type type = value.GetType();
		if (IsUninteresting(type))
		{
			return true;
		}
		if (value is MemberInfo member)
		{
			ownsPayload = IsOwned(member, context.Owned);
			return true;
		}
		if (value is Assembly assembly)
		{
			ownsPayload = IsOwned(assembly, context.Owned);
			return true;
		}
		if (IsOwned(type, context.Owned))
		{
			ownsPayload = true;
			return true;
		}
		if (value is Delegate @delegate)
		{
			Delegate[] invocationList = @delegate.GetInvocationList();
			foreach (Delegate delegate2 in invocationList)
			{
				if (!IsOwned(delegate2.Method, context.Owned) && (delegate2.Target == null || !InspectRegistrationPayload(delegate2.Target, context, depth + 1, ref budget, out var ownsPayload2) || !ownsPayload2))
				{
					return false;
				}
			}
			ownsPayload = true;
			return true;
		}
		if (value is IEnumerable || IsFrameworkCarrier(type) || (!type.IsValueType && !CanTraverseObject(type, context)))
		{
			return false;
		}
		try
		{
			if (type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length != 0)
			{
				return false;
			}
			Type type2 = type;
			while (type2 != null && type2 != typeof(object) && type2 != typeof(ValueType))
			{
				if (!type2.IsValueType && !CanTraverseDeclaredFields(type2, context))
				{
					return false;
				}
				FieldInfo[] fields = type2.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				for (int i = 0; i < fields.Length; i++)
				{
					if (!InspectRegistrationPayload(fields[i].GetValue(value), context, depth + 1, ref budget, out var ownsPayload3))
					{
						return false;
					}
					ownsPayload |= ownsPayload3;
				}
				type2 = type2.BaseType;
			}
			return ownsPayload || type.IsValueType;
		}
		catch
		{
			return false;
		}
	}

	private static GraphInspection PruneDelegateField(FieldInfo field, object? owner, Delegate handler, GraphContext context)
	{
		List<Delegate> list = new List<Delegate>();
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		Delegate[] invocationList = handler.GetInvocationList();
		foreach (Delegate @delegate in invocationList)
		{
			GraphInspection graphInspection = Inspect(@delegate, context, 0, allowPrune: false);
			num += graphInspection.References;
			num2 += graphInspection.Removed;
			num3 += graphInspection.Remaining;
			if (graphInspection.Remaining > 0)
			{
				num4++;
			}
			else
			{
				list.Add(@delegate);
			}
		}
		if (num4 == 0)
		{
			return new GraphInspection(num, num2, 0);
		}
		try
		{
			Delegate delegate2 = null;
			foreach (Delegate item in list)
			{
				delegate2 = Delegate.Combine(delegate2, item);
			}
			if (ReferenceFieldUpdate.TryReplace(field, owner, handler, delegate2))
			{
				return new GraphInspection(num, num2 + num4, 0);
			}
			return new GraphInspection(num, num2, num3);
		}
		catch
		{
			return new GraphInspection(num, num2, num3);
		}
	}

	private static GraphInspection InspectDelegate(Delegate handler, GraphContext context, int depth, bool allowPrune, int selfReferences)
	{
		int num = selfReferences;
		int num2 = 0;
		int num3 = selfReferences;
		Delegate[] invocationList = handler.GetInvocationList();
		foreach (Delegate @delegate in invocationList)
		{
			if (IsOwned(@delegate.Method, context.Owned) || (@delegate.Target != null && IsOwned(@delegate.Target.GetType(), context.Owned)))
			{
				num++;
				num3++;
			}
			else if (@delegate.Target != null)
			{
				GraphInspection graphInspection = Inspect(@delegate.Target, context, depth + 1, allowPrune: false);
				num += graphInspection.References;
				num2 += graphInspection.Removed;
				num3 += graphInspection.Remaining;
			}
		}
		return new GraphInspection(num, num2, num3);
	}

	private static bool TryInvokeRemove(MethodInfo removeMethod, object collection, object? item)
	{
		try
		{
			object obj = removeMethod.Invoke(collection, new object[1] { item });
			bool flag = default(bool);
			int num;
			if (obj is bool)
			{
				flag = (bool)obj;
				num = 1;
			}
			else
			{
				num = 0;
			}
			return num == 0 || flag;
		}
		catch
		{
			return false;
		}
	}

	private static bool TryEnter(object value, GraphContext context, int depth, out bool tracked)
	{
		tracked = false;
		if (depth > 14 || context.NodesLeft <= 0)
		{
			context.Truncated = true;
			return false;
		}
		context.NodesLeft--;
		if (value.GetType().IsValueType)
		{
			return true;
		}
		if (!context.Path.Add(value))
		{
			return false;
		}
		tracked = true;
		return true;
	}

	private static GraphInspection OwnedResult(bool owned)
	{
		if (!owned)
		{
			return GraphInspection.Empty;
		}
		return new GraphInspection(1, 0, 1);
	}

	private static Type? FindGenericInterface(Type type, Type genericDefinition)
	{
		return type.GetInterfaces().FirstOrDefault((Type candidate) => candidate.IsGenericType && candidate.GetGenericTypeDefinition() == genericDefinition);
	}

	private static bool CanTraverseObject(Type type, GraphContext context)
	{
		if (!context.TraversableAssemblies.Contains(type.Assembly))
		{
			return IsFrameworkCarrier(type);
		}
		return true;
	}

	private static bool CanTraverseDeclaredFields(Type type, GraphContext context)
	{
		if (!context.TraversableAssemblies.Contains(type.Assembly))
		{
			return IsFrameworkCarrier(type);
		}
		return true;
	}

	private static bool IsFrameworkCarrier(Type type)
	{
		string text = type.Namespace ?? string.Empty;
		if (!typeof(ITuple).IsAssignableFrom(type) && !text.StartsWith("System.Collections", StringComparison.Ordinal) && !text.StartsWith("System.Threading.Tasks", StringComparison.Ordinal))
		{
			if (type.IsGenericType)
			{
				return type.GetGenericTypeDefinition() == typeof(Lazy<>);
			}
			return false;
		}
		return true;
	}

	private static bool IsMaterializedCollection(IEnumerable sequence)
	{
		bool flag = !(sequence is string);
		if (flag)
		{
			bool flag2;
			switch (sequence.GetType().Namespace)
			{
			case "System.Collections":
			case "System.Collections.Generic":
			case "System.Collections.Concurrent":
			case "System.Collections.ObjectModel":
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		return flag;
	}

	private static bool IsOwned(Type type, HashSet<AssemblyLoadContext> owned)
	{
		try
		{
			if (IsOwned(type.Assembly, owned))
			{
				return true;
			}
			if (type.HasElementType)
			{
				Type elementType = type.GetElementType();
				if ((object)elementType != null && IsOwned(elementType, owned))
				{
					return true;
				}
			}
			return type.IsGenericType && type.GetGenericArguments().Any((Type argument) => IsOwned(argument, owned));
		}
		catch
		{
			return false;
		}
	}

	private static bool IsOwned(MemberInfo member, HashSet<AssemblyLoadContext> owned)
	{
		try
		{
			if (!IsOwned(member.Module.Assembly, owned))
			{
				Type declaringType = member.DeclaringType;
				if ((object)declaringType == null || !IsOwned(declaringType, owned))
				{
					Type reflectedType = member.ReflectedType;
					if ((object)reflectedType == null || !IsOwned(reflectedType, owned))
					{
						if (!(member is MethodInfo methodInfo))
						{
							if (!(member is ConstructorInfo constructorInfo))
							{
								if (!(member is FieldInfo fieldInfo))
								{
									if (!(member is PropertyInfo propertyInfo))
									{
										if (member is EventInfo eventInfo)
										{
											Type eventHandlerType = eventInfo.EventHandlerType;
											return (object)eventHandlerType != null && IsOwned(eventHandlerType, owned);
										}
										return false;
									}
									return IsOwned(propertyInfo.PropertyType, owned) || propertyInfo.GetIndexParameters().Any((ParameterInfo parameter) => IsOwned(parameter.ParameterType, owned));
								}
								return IsOwned(fieldInfo.FieldType, owned);
							}
							return constructorInfo.GetParameters().Any((ParameterInfo parameter) => IsOwned(parameter.ParameterType, owned));
						}
						if (IsOwned(methodInfo.ReturnType, owned) || (methodInfo.IsGenericMethod && methodInfo.GetGenericArguments().Any((Type argument) => IsOwned(argument, owned))))
						{
							return true;
						}
						return methodInfo.GetParameters().Any((ParameterInfo parameter) => IsOwned(parameter.ParameterType, owned));
					}
				}
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	internal static bool IsOwned(Assembly assembly, HashSet<AssemblyLoadContext> owned)
	{
		try
		{
			AssemblyLoadContext loadContext = AssemblyLoadContext.GetLoadContext(assembly);
			return loadContext != null && owned.Contains(loadContext);
		}
		catch
		{
			return false;
		}
	}

	internal static HashSet<AssemblyLoadContext> LiveContexts(IEnumerable<WeakReference> contextReferences)
	{
		HashSet<AssemblyLoadContext> hashSet = new HashSet<AssemblyLoadContext>(ReferenceEqualityComparer.Instance);
		foreach (WeakReference contextReference in contextReferences)
		{
			if (contextReference.Target is AssemblyLoadContext item)
			{
				hashSet.Add(item);
			}
		}
		return hashSet;
	}
}

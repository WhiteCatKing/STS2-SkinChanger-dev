using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

internal static class ModelTypeCompatibility
{
	private static readonly Lazy<IReadOnlySet<ModelId>> CanonicalModelIds = new Lazy<IReadOnlySet<ModelId>>(BuildCanonicalModelIds);

	private static readonly HashSet<ModelId> ReportedCollisions = new HashSet<ModelId>();

	private static readonly PropertyInfo? RegisteredModelsProperty = typeof(ModelDb).GetProperty("All", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

	private static readonly FieldInfo? RegisteredModelsField = typeof(ModelDb).GetField("_contentById", BindingFlags.Static | BindingFlags.NonPublic);

	private static IReadOnlySet<ModelId> BuildCanonicalModelIds()
	{
		HashSet<ModelId> hashSet = new HashSet<ModelId>();
		Type[] types = typeof(AbstractModel).Assembly.GetTypes();
		foreach (Type type in types)
		{
			if (!type.IsAbstract && type.IsSubclassOf(typeof(AbstractModel)))
			{
				try
				{
					hashSet.Add(ModelDb.GetId(type));
				}
				catch
				{
				}
			}
		}
		return hashSet;
	}

	internal static Type[] Filter(IEnumerable<Type> types, bool includeAlreadyRegisteredModels = false)
	{
		Assembly assembly = typeof(AbstractModel).Assembly;
		HashSet<ModelId> hashSet = (includeAlreadyRegisteredModels ? (from model in GetRegisteredModels()
			select model.Id).ToHashSet() : new HashSet<ModelId>());
		List<Type> list = new List<Type>();
		foreach (Type type in types)
		{
			if (type.Assembly == assembly || !type.IsSubclassOf(typeof(AbstractModel)))
			{
				list.Add(type);
				continue;
			}
			ModelId id;
			try
			{
				id = ModelDb.GetId(type);
			}
			catch
			{
				list.Add(type);
				continue;
			}
			if (CanonicalModelIds.Value.Contains(id) || !hashSet.Add(id))
			{
				ReportCollision(id);
			}
			else
			{
				list.Add(type);
			}
		}
		return list.ToArray();
	}

	private static void ReportCollision(ModelId id)
	{
		lock (ReportedCollisions)
		{
			if (ReportedCollisions.Add(id))
			{
				ModLog.Warn($"检测到重复 Mod 模型 ID {id}，已保留先注册的模型以避免启动失败。");
			}
		}
	}

	internal static int RemoveExistingCanonicalConflicts()
	{
		Assembly assembly = typeof(AbstractModel).Assembly;
		int num = 0;
		AbstractModel[] array = GetRegisteredModels().ToArray();
		foreach (AbstractModel val in array)
		{
			Type type = ((object)val).GetType();
			if (type.Assembly == assembly || !CanonicalModelIds.Value.Contains(val.Id))
			{
				continue;
			}
			ModelDb.Remove(type);
			num++;
			lock (ReportedCollisions)
			{
				if (ReportedCollisions.Add(val.Id))
				{
					ModLog.Warn($"检测到已注入的 Mod 模型 {val.Id} 与当前游戏原版重复，已移除 Mod 实例并保留原版。");
				}
			}
		}
		return num;
	}

	private static IEnumerable<AbstractModel> GetRegisteredModels()
	{
		if (RegisteredModelsProperty?.GetValue(null) is IEnumerable<AbstractModel> result)
		{
			return result;
		}
		if (RegisteredModelsField?.GetValue(null) is IDictionary<ModelId, AbstractModel> dictionary)
		{
			return dictionary.Values;
		}
		return Array.Empty<AbstractModel>();
	}
}

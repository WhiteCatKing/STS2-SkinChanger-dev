using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

internal static class ActCatalog
{
	private static List<ActModel>? _acts;

	public static IReadOnlyList<ActModel> All
	{
		get
		{
			lock (typeof(ActCatalog))
			{
				return _acts ?? (_acts = Enumerate());
			}
		}
	}

	public static void Invalidate()
	{
		lock (typeof(ActCatalog))
		{
			_acts = null;
		}
	}

	private static bool HasContent(ActModel act)
	{
		try
		{
			if (act.AllEncounters.Any())
			{
				return true;
			}
		}
		catch
		{
		}
		try
		{
			if (act.AllEvents.Any() || act.AllAncients.Any())
			{
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private static List<ActModel> Enumerate()
	{
		List<ActModel> list = new List<ActModel>();
		try
		{
			Type[] allAbstractModelSubtypes = ModelDb.AllAbstractModelSubtypes;
			foreach (Type type in allAbstractModelSubtypes)
			{
				if (type.IsAbstract || type.IsGenericTypeDefinition || !typeof(ActModel).IsAssignableFrom(type))
				{
					continue;
				}
				try
				{
					ActModel byIdOrNull = ModelDb.GetByIdOrNull<ActModel>(ModelDb.GetId(type));
					if (byIdOrNull != null && !type.Name.Contains("Deprecated", StringComparison.OrdinalIgnoreCase) && (byIdOrNull.Index >= 0 || HasContent(byIdOrNull)))
					{
						list.Add(byIdOrNull);
					}
				}
				catch
				{
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("枚举地区失败：" + ex.GetBaseException().Message);
		}
		if (list.Count == 0)
		{
			try
			{
				list.AddRange(ModelDb.Acts);
			}
			catch
			{
			}
		}
		return (from @group in list.GroupBy<ActModel, string>((ActModel act) => ((AbstractModel)act).Id.Entry, StringComparer.OrdinalIgnoreCase)
			select @group.First() into act
			orderby act.Index, act.IsDefault descending
			select act).ToList();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class WorkshopSubscriptionFilter
{
	public static readonly string[] Options = new string[3] { "subscribed", "unsubscribed", "code_errors" };

	private readonly Dictionary<ulong, bool> _known = new Dictionary<ulong, bool>();

	private readonly HashSet<ulong> _pending = new HashSet<ulong>();

	public string Value { get; private set; } = "unsubscribed";

	public bool Unavailable { get; private set; }

	public void Select(string value)
	{
		if (value.Length != 0 && !Options.Contains(value))
		{
			throw new ArgumentOutOfRangeException("value");
		}
		Value = value;
		_pending.Clear();
	}

	public void Refresh(IEnumerable<ulong> ids, Func<ulong, bool> read)
	{
		Unavailable = false;
		foreach (ulong id in ids)
		{
			try
			{
				_known[id] = read(id);
			}
			catch
			{
				Unavailable = true;
			}
		}
	}

	public void BeginAction(ulong id)
	{
		_pending.Add(id);
	}

	public void EndAction(ulong id)
	{
		_pending.Remove(id);
	}

	public bool Matches(ulong id)
	{
		if (Value != "code_errors")
		{
			if (Value.Length != 0 && !_pending.Contains(id))
			{
				if (_known.TryGetValue(id, out var value))
				{
					if (!(Value == "subscribed"))
					{
						return !value;
					}
					return value;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public IEnumerable<WorkshopCatalogItem> Filter(IEnumerable<WorkshopCatalogItem> items)
	{
		return items.Where((WorkshopCatalogItem item) => Matches(item.Id));
	}

	public static string Name(string value)
	{
		return value switch
		{
			"subscribed" => WorkshopCommunityText.Get(WorkshopCommunityTextKey.Subscribed), 
			"unsubscribed" => WorkshopCommunityText.Get(WorkshopCommunityTextKey.NotSubscribed), 
			"code_errors" => WorkshopCodeErrorText.Get(WorkshopCodeUi.Filter), 
			_ => WorkshopText.Get(WorkshopTextKey.All), 
		};
	}
}

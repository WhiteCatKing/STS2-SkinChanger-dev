using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class EventSkinPolicy
{
	public const string Prefix = "event:";

	private static readonly string[] ResourceRoots = new string[7] { "res://images/events/", "res://images/packed/vfx/event/", "res://scenes/vfx/events/", "res://scenes/events/background_scenes/", "res://scenes/events/custom/", "res://animations/events/", "res://audio/events/" };

	public static string GroupId(string eventId)
	{
		if (!eventId.Equals("fake_merchant", StringComparison.OrdinalIgnoreCase))
		{
			return "event:" + eventId.ToLowerInvariant();
		}
		return "fake_merchant_monster";
	}

	public static bool IsEventGroup(string groupId)
	{
		return groupId.StartsWith("event:", StringComparison.Ordinal);
	}

	public static bool CouldOwnResource(string path)
	{
		return ResourceRoots.Any((string root) => path.StartsWith(root, StringComparison.OrdinalIgnoreCase));
	}

	public static string? EventIdFromKey(string key)
	{
		int num = key.IndexOf('.');
		if (num <= 0)
		{
			return null;
		}
		return key.Substring(0, num).ToLowerInvariant();
	}

	public static string? FindResourceOwner(string path, IEnumerable<string> eventIds)
	{
		string text = path.ToLowerInvariant();
		string text2 = ResourceRoots.FirstOrDefault(text.StartsWith);
		if (text2 == null)
		{
			return null;
		}
		string text3 = text;
		int length = text2.Length;
		string text4 = text3.Substring(length, text3.Length - length);
		string text5 = null;
		foreach (string eventId in eventIds)
		{
			bool flag = (text5 == null || eventId.Length > text5.Length) && text4.StartsWith(eventId, StringComparison.OrdinalIgnoreCase) && text4.Length > eventId.Length;
			if (flag)
			{
				char c = text4[eventId.Length];
				bool flag2 = ((c == '.' || c == '/' || c == '_') ? true : false);
				flag = flag2;
			}
			if (flag)
			{
				text5 = eventId;
			}
		}
		return text5;
	}
}

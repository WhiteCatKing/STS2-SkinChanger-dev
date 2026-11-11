using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace STS2SkinChanger.Core;

internal static class EventPreviewPolicy
{
	public static Rect2 TextBounds(Vector2 viewportSize, Rect2 nativeColumn, float contentHeight)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		float num = Math.Max(800f, ((Rect2)(ref nativeColumn)).Size.X) + 20f;
		float num2 = Math.Max(24f, Math.Min(((Rect2)(ref nativeColumn)).Position.X, viewportSize.X - 24f - num));
		float num3 = viewportSize.Y - 24f;
		float num4 = Math.Min(96f, Math.Max(0f, num3 - 1f));
		float num5 = Math.Clamp(max: Math.Clamp(((Rect2)(ref nativeColumn)).Position.Y, num4, Math.Max(num4, num3 - 1f)), value: num3 - Math.Max(0f, contentHeight), min: num4);
		return new Rect2(num2, num5, num, Math.Max(1f, num3 - num5));
	}

	public static string[] Pages(string eventId, IEnumerable<string> keys)
	{
		string prefix = eventId + ".pages.";
		return (from page in (from page in keys.Where((string key) => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && key.EndsWith(".description", StringComparison.Ordinal)).Select(delegate(string key)
				{
					int length = prefix.Length;
					int length2 = ".description".Length;
					return key.Substring(length, key.Length - length2 - length);
				})
				where page.Length > 0 && !page.Contains('.')
				select page).Distinct<string>(StringComparer.Ordinal)
			orderby (!(page == "INITIAL")) ? 1 : 0
			select page).ThenBy<string, string>((string page) => page, StringComparer.Ordinal).ToArray();
	}

	public static string[] Options(string eventId, string page, IEnumerable<string> keys)
	{
		string prefix = eventId + ".pages." + page + ".options.";
		return (from key in keys
			where key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) && (key.EndsWith(".title", StringComparison.Ordinal) || key.EndsWith(".description", StringComparison.Ordinal))
			select key.Substring(0, key.LastIndexOf('.'))).Distinct<string>(StringComparer.Ordinal).Order<string>(StringComparer.Ordinal).ToArray();
	}
}

using System;
using Godot;

namespace STS2SkinChanger.Ui;

internal static class WorkshopHoverPolicy
{
	public static bool CanDisplay(ulong hovered, ulong active, bool blocked)
	{
		if (!blocked && hovered != 0L)
		{
			return hovered == active;
		}
		return false;
	}

	public static Rect2 PlaceIntroduction(Rect2 anchor, Vector2 size, Rect2 viewport)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		Rect2 val = ((Rect2)(ref viewport)).Grow(-12f);
		size = ((Vector2)(ref size)).Min(((Rect2)(ref val)).Size);
		float num = ((((Rect2)(ref val)).End.X - ((Rect2)(ref anchor)).End.X >= size.X + 12f) ? (((Rect2)(ref anchor)).End.X + 12f) : ((!(((Rect2)(ref anchor)).Position.X - ((Rect2)(ref val)).Position.X >= size.X + 12f)) ? ((Rect2)(ref val)).Position.X : (((Rect2)(ref anchor)).Position.X - size.X - 12f)));
		float num2 = Math.Clamp(((Rect2)(ref anchor)).Position.Y, ((Rect2)(ref val)).Position.Y, Math.Max(((Rect2)(ref val)).Position.Y, ((Rect2)(ref val)).End.Y - size.Y));
		Rect2 val2 = new Rect2(new Vector2(num, num2), size);
		if (((Rect2)(ref val2)).Intersects(anchor, false))
		{
			if (((Rect2)(ref val)).End.Y - ((Rect2)(ref anchor)).End.Y >= size.Y + 12f)
			{
				num2 = ((Rect2)(ref anchor)).End.Y + 12f;
			}
			else if (((Rect2)(ref anchor)).Position.Y - ((Rect2)(ref val)).Position.Y >= size.Y + 12f)
			{
				num2 = ((Rect2)(ref anchor)).Position.Y - size.Y - 12f;
			}
		}
		return new Rect2(new Vector2(Math.Clamp(num, ((Rect2)(ref val)).Position.X, Math.Max(((Rect2)(ref val)).Position.X, ((Rect2)(ref val)).End.X - size.X)), num2), size);
	}
}

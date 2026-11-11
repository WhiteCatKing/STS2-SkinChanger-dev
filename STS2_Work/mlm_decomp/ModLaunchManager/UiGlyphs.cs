using System;
using Godot;

namespace ModLaunchManager;

internal static class UiGlyphs
{
	internal enum Mark
	{
		None,
		Check,
		Dash
	}

	private static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

	internal static ImageTexture CheckBox(int size, Color fill, Color border, float borderWidth, Color mark, Mark shape)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		Vector2 centre = new Vector2((float)size * 0.5f, (float)size * 0.5f);
		Vector2 half = new Vector2((float)size * 0.5f - 1f, (float)size * 0.5f - 1f);
		float radius = Mathf.Max(2f, (float)size * 0.16f);
		float stroke = (float)size * 0.05f;
		Vector2 checkStart = new Vector2((float)size * 0.28f, (float)size * 0.52f);
		Vector2 checkCorner = new Vector2((float)size * 0.43f, (float)size * 0.68f);
		Vector2 checkEnd = new Vector2((float)size * 0.74f, (float)size * 0.32f);
		Vector2 dashStart = new Vector2((float)size * 0.3f, (float)size * 0.5f);
		Vector2 dashEnd = new Vector2((float)size * 0.7f, (float)size * 0.5f);
		return Rasterize(size, delegate(Vector2 point)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_008f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			float distance = RoundedBox(point - centre, half, radius);
			Color val = Fade(fill, Coverage(distance));
			if (borderWidth > 0f)
			{
				val = Over(val, Fade(border, Coverage(Ring(distance, borderWidth))));
			}
			if (shape == Mark.None)
			{
				return val;
			}
			float distance2 = ((shape == Mark.Check) ? Mathf.Min(Segment(point, checkStart, checkCorner, stroke), Segment(point, checkCorner, checkEnd, stroke)) : Segment(point, dashStart, dashEnd, stroke));
			return Over(val, Fade(mark, Coverage(distance2)));
		});
	}

	internal static ImageTexture Radio(int size, Color fill, Color border, float borderWidth, Color dot, bool selected)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Vector2 centre = new Vector2((float)size * 0.5f, (float)size * 0.5f);
		float outer = (float)size * 0.5f - 1f;
		float dotRadius = (float)size * 0.21f;
		return Rasterize(size, delegate(Vector2 point)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			float distance = ((Vector2)(ref point)).DistanceTo(centre) - outer;
			Color val = Fade(fill, Coverage(distance));
			if (borderWidth > 0f)
			{
				val = Over(val, Fade(border, Coverage(Ring(distance, borderWidth))));
			}
			return (!selected) ? val : Over(val, Fade(dot, Coverage(((Vector2)(ref point)).DistanceTo(centre) - dotRadius)));
		});
	}

	internal static ImageTexture Chevron(int size, Color color, float thickness, bool up = false, int indent = 0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		Vector2 left = new Vector2((float)indent + (float)size * 0.24f, (float)size * (up ? 0.59f : 0.41f));
		Vector2 tip = new Vector2((float)indent + (float)size * 0.5f, (float)size * (up ? 0.34f : 0.66f));
		Vector2 right = new Vector2((float)indent + (float)size * 0.76f, (float)size * (up ? 0.59f : 0.41f));
		float stroke = thickness * 0.5f;
		return Rasterize(size + indent, size, (Vector2 point) => Fade(color, Coverage(Mathf.Min(Segment(point, left, tip, stroke), Segment(point, tip, right, stroke)))));
	}

	internal static ImageTexture ChevronLeft(int size, Color color, float thickness)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		Vector2 top = new Vector2((float)size * 0.59f, (float)size * 0.24f);
		Vector2 tip = new Vector2((float)size * 0.34f, (float)size * 0.5f);
		Vector2 bottom = new Vector2((float)size * 0.59f, (float)size * 0.76f);
		float stroke = thickness * 0.5f;
		return Rasterize(size, (Vector2 point) => Fade(color, Coverage(Mathf.Min(Segment(point, top, tip, stroke), Segment(point, tip, bottom, stroke)))));
	}

	internal static ImageTexture ChevronRight(int size, Color color, float thickness, int indent = 0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Vector2 top = new Vector2((float)indent + (float)size * 0.41f, (float)size * 0.24f);
		Vector2 tip = new Vector2((float)indent + (float)size * 0.66f, (float)size * 0.5f);
		Vector2 bottom = new Vector2((float)indent + (float)size * 0.41f, (float)size * 0.76f);
		float stroke = thickness * 0.5f;
		return Rasterize(size + indent, size, (Vector2 point) => Fade(color, Coverage(Mathf.Min(Segment(point, top, tip, stroke), Segment(point, tip, bottom, stroke)))));
	}

	internal static ImageTexture Gear(int size, Color color)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Vector2 centre = new Vector2((float)size * 0.5f, (float)size * 0.5f);
		float bodyRadius = (float)size * 0.31f;
		float holeRadius = (float)size * 0.115f;
		float toothCentre = (float)size * 0.35f;
		Vector2 toothHalf = new Vector2((float)size * 0.075f, (float)size * 0.12f);
		float toothCorner = Mathf.Max(0.75f, (float)size * 0.025f);
		return Rasterize(size, delegate(Vector2 point)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = point - centre;
			float num = ((Vector2)(ref val)).Length() - bodyRadius;
			Vector2 val2 = default(Vector2);
			Vector2 val3 = default(Vector2);
			Vector2 point2 = default(Vector2);
			for (int i = 0; i < 8; i++)
			{
				float num2 = (float)i * (float)Math.PI / 4f;
				((Vector2)(ref val2))._002Ector(Mathf.Cos(num2), Mathf.Sin(num2));
				((Vector2)(ref val3))._002Ector(0f - val2.Y, val2.X);
				((Vector2)(ref point2))._002Ector(((Vector2)(ref val)).Dot(val3), ((Vector2)(ref val)).Dot(val2) - toothCentre);
				num = Mathf.Min(num, RoundedBox(point2, toothHalf, toothCorner));
			}
			num = Mathf.Max(num, holeRadius - ((Vector2)(ref val)).Length());
			return Fade(color, Coverage(num));
		});
	}

	internal static ImageTexture PadSymbolIcon(int size, Color color, PadSymbol symbol)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		Vector2 centre = new Vector2((float)size * 0.5f, (float)size * 0.5f);
		float stroke = Mathf.Max(0.75f, (float)size * 0.045f);
		float span = (float)size * 0.22f;
		Vector2 top = centre + new Vector2(0f, (0f - span) * 1.05f);
		Vector2 left = centre + new Vector2((0f - span) * 1.1f, span * 0.8f);
		Vector2 right = centre + new Vector2(span * 1.1f, span * 0.8f);
		return Rasterize(size, delegate(Vector2 point)
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_012a: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015a: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0162: Unknown result type (might be due to invalid IL or missing references)
			//IL_0182: Unknown result type (might be due to invalid IL or missing references)
			//IL_0184: Unknown result type (might be due to invalid IL or missing references)
			//IL_019d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
			//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0203: Unknown result type (might be due to invalid IL or missing references)
			//IL_0208: Unknown result type (might be due to invalid IL or missing references)
			//IL_0218: Unknown result type (might be due to invalid IL or missing references)
			//IL_021a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0232: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0254: Unknown result type (might be due to invalid IL or missing references)
			//IL_0259: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_028a: Unknown result type (might be due to invalid IL or missing references)
			float distance = symbol switch
			{
				PadSymbol.Cross => Mathf.Min(Segment(point, centre - new Vector2(span, span), centre + new Vector2(span, span), stroke), Segment(point, centre + new Vector2(0f - span, span), centre + new Vector2(span, 0f - span), stroke)), 
				PadSymbol.Circle => Mathf.Abs(((Vector2)(ref point)).DistanceTo(centre) - span * 1.05f) - stroke, 
				PadSymbol.Square => Mathf.Abs(RoundedBox(point - centre, new Vector2(span, span), (float)size * 0.03f)) - stroke, 
				PadSymbol.Triangle => Mathf.Min(Segment(point, top, left, stroke), Mathf.Min(Segment(point, left, right, stroke), Segment(point, right, top, stroke))), 
				PadSymbol.Menu => Mathf.Min(Segment(point, centre + new Vector2(0f - span, (0f - span) * 0.7f), centre + new Vector2(span, (0f - span) * 0.7f), stroke), Mathf.Min(Segment(point, centre + new Vector2(0f - span, 0f), centre + new Vector2(span, 0f), stroke), Segment(point, centre + new Vector2(0f - span, span * 0.7f), centre + new Vector2(span, span * 0.7f), stroke))), 
				_ => float.MaxValue, 
			};
			return Fade(color, Coverage(distance));
		});
	}

	internal static ImageTexture Folder(int size, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		float stroke = Mathf.Max(0.75f, (float)size * 0.035f);
		Vector2 body = new Vector2((float)size * 0.5f, (float)size * 0.57f);
		Vector2 bodyHalf = new Vector2((float)size * 0.34f, (float)size * 0.23f);
		Vector2 tab = new Vector2((float)size * 0.31f, (float)size * 0.31f);
		Vector2 tabHalf = new Vector2((float)size * 0.15f, (float)size * 0.08f);
		return Rasterize(size, delegate(Vector2 point)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			float distance = Mathf.Min(Mathf.Abs(Mathf.Min(RoundedBox(point - body, bodyHalf, (float)size * 0.06f), RoundedBox(point - tab, tabHalf, (float)size * 0.04f))) - stroke, Segment(point, new Vector2((float)size * 0.16f, (float)size * 0.43f), new Vector2((float)size * 0.84f, (float)size * 0.43f), stroke));
			return Fade(color, Coverage(distance));
		});
	}

	internal static ImageTexture Archive(int size, Color color)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		float stroke = Mathf.Max(0.75f, (float)size * 0.035f);
		Vector2 centre = new Vector2((float)size * 0.5f, (float)size * 0.5f);
		Vector2 half = new Vector2((float)size * 0.28f, (float)size * 0.36f);
		Vector2 pull = new Vector2((float)size * 0.5f, (float)size * 0.62f);
		return Rasterize(size, delegate(Vector2 point)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0101: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			float num = Mathf.Abs(RoundedBox(point - centre, half, (float)size * 0.06f)) - stroke;
			for (int i = 0; i < 4; i++)
			{
				float num2 = (float)size * (0.2f + (float)i * 0.075f);
				float num3 = ((i % 2 == 0) ? ((float)size * 0.43f) : ((float)size * 0.5f));
				num = Mathf.Min(num, Segment(point, new Vector2(num3, num2), new Vector2(num3 + (float)size * 0.07f, num2), stroke));
			}
			num = Mathf.Min(num, Mathf.Abs(RoundedBox(point - pull, new Vector2((float)size * 0.07f, (float)size * 0.08f), (float)size * 0.025f)) - stroke);
			return Fade(color, Coverage(num));
		});
	}

	private static ImageTexture Rasterize(int size, Func<Vector2, Color> shade)
	{
		return Rasterize(size, size, shade);
	}

	private static ImageTexture Rasterize(int width, int height, Func<Vector2, Color> shade)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Image val = Image.CreateEmpty(width, height, false, (Format)5);
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < width; j++)
			{
				val.SetPixel(j, i, shade(new Vector2((float)j + 0.5f, (float)i + 0.5f)));
			}
		}
		return ImageTexture.CreateFromImage(val);
	}

	private static float Coverage(float distance)
	{
		return Mathf.Clamp(0.5f - distance, 0f, 1f);
	}

	private static float Ring(float distance, float width)
	{
		return Mathf.Max(distance, 0f - (distance + width));
	}

	private static float RoundedBox(Vector2 point, Vector2 half, float radius)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((Vector2)(ref point)).Abs() - half + new Vector2(radius, radius);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(Mathf.Max(val.X, 0f), Mathf.Max(val.Y, 0f));
		return ((Vector2)(ref val2)).Length() + Mathf.Min(Mathf.Max(val.X, val.Y), 0f) - radius;
	}

	private static float Segment(Vector2 point, Vector2 from, Vector2 to, float halfThickness)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = point - from;
		Vector2 val2 = to - from;
		float num = Mathf.Clamp(((Vector2)(ref val)).Dot(val2) / ((Vector2)(ref val2)).Dot(val2), 0f, 1f);
		Vector2 val3 = val - val2 * num;
		return ((Vector2)(ref val3)).Length() - halfThickness;
	}

	private static Color Fade(Color color, float coverage)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		return new Color(color.R, color.G, color.B, color.A * coverage);
	}

	private static Color Over(Color under, Color over)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		float num = under.A * (1f - over.A);
		float num2 = over.A + num;
		if (num2 <= 0.0001f)
		{
			return Transparent;
		}
		return new Color((over.R * over.A + under.R * num) / num2, (over.G * over.A + under.G * num) / num2, (over.B * over.A + under.B * num) / num2, num2);
	}
}

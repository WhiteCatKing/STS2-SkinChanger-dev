using Godot;

namespace CznCore.Presentation;

public static class UiCrispness
{
	public const float MinimumFontOversampling = 2f;

	public static float PixelsPerUnit(CanvasItem item)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Transform2D val = ((Node)item).GetViewport().GetFinalTransform();
		float num = ((Vector2)(ref val.X)).Length();
		val = item.GetGlobalTransformWithCanvas();
		return num * ((Vector2)(ref val.X)).Length();
	}

	public static float FitViewport(SubViewportContainer container, SubViewport viewport, Vector2 layoutSize, float pageScale)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float num = PixelsPerUnit((CanvasItem)(((object)((Node)container).GetParent<CanvasItem>()) ?? ((object)container)));
		Vector2I val = default(Vector2I);
		((Vector2I)(ref val))._002Ector(Mathf.RoundToInt(layoutSize.X * num), Mathf.RoundToInt(layoutSize.Y * num));
		viewport.Size = val;
		((Viewport)viewport).CanvasTransform = new Transform2D(0f, Vector2.One * pageScale * num, 0f, Vector2.Zero);
		container.Stretch = false;
		((Control)container).Size = Vector2I.op_Implicit(val);
		((Control)container).Scale = Vector2.One / num;
		return pageScale * num;
	}

	public static void SampleFont(Font font, float effectiveScale)
	{
		FontFile val = (FontFile)(object)((font is FontFile) ? font : null);
		FontFile val3;
		if (val == null)
		{
			FontVariation val2 = (FontVariation)(object)((font is FontVariation) ? font : null);
			val3 = (FontFile)((val2 == null) ? null : /*isinst with value type is only supported in some contexts*/);
		}
		else
		{
			val3 = val;
		}
		FontFile val4 = val3;
		if (val4 == null || val4.MultichannelSignedDistanceField)
		{
			return;
		}
		float num = Mathf.Max(2f, Mathf.Ceil(effectiveScale * 2f) / 2f);
		if (val4.Oversampling < num)
		{
			val4.Oversampling = num;
		}
		foreach (Font fallback in font.Fallbacks)
		{
			SampleFont(fallback, effectiveScale);
		}
	}
}

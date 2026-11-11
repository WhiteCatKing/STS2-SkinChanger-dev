using System;
using Godot;

namespace STS2SkinChanger.Core;

internal sealed class RenderedPreviewFraming
{
	private readonly Rect2 _canvas;

	private int _validSamples;

	public Transform2D CanvasTransform { get; private set; }

	public bool Complete { get; private set; }

	public bool HasContent => VisibleBounds.HasValue;

	public int Samples { get; private set; }

	public Rect2? VisibleBounds { get; private set; }

	public RenderedPreviewFraming(Rect2 initialBounds, Vector2I resolution)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		_canvas = new Rect2(Vector2.Zero, new Vector2((float)Math.Max(32, resolution.X), (float)Math.Max(32, resolution.Y)));
		if (!Valid(initialBounds))
		{
			((Rect2)(ref initialBounds))._002Ector(-200f, -400f, 400f, 400f);
		}
		SetCamera(((Rect2)(ref initialBounds)).Grow(Math.Max(((Rect2)(ref initialBounds)).Size.X, ((Rect2)(ref initialBounds)).Size.Y) / 8f), _canvas);
	}

	public bool Observe(Rect2? renderedPixels)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		if (Complete)
		{
			return false;
		}
		Samples++;
		if (renderedPixels.HasValue)
		{
			Rect2 valueOrDefault = renderedPixels.GetValueOrDefault();
			if (Valid(valueOrDefault))
			{
				Transform2D canvasTransform;
				if (((Rect2)(ref valueOrDefault)).Position.X <= 1f || ((Rect2)(ref valueOrDefault)).Position.Y <= 1f || ((Rect2)(ref valueOrDefault)).End.X >= ((Rect2)(ref _canvas)).End.X - 1f || ((Rect2)(ref valueOrDefault)).End.Y >= ((Rect2)(ref _canvas)).End.Y - 1f)
				{
					canvasTransform = CanvasTransform;
					Rect2 val = ((Transform2D)(ref canvasTransform)).AffineInverse() * _canvas;
					SetCamera(((Rect2)(ref val)).Grow(Math.Max(((Rect2)(ref val)).Size.X, ((Rect2)(ref val)).Size.Y) / 2f), _canvas);
					_validSamples = 0;
				}
				else
				{
					canvasTransform = CanvasTransform;
					VisibleBounds = ((Transform2D)(ref canvasTransform)).AffineInverse() * valueOrDefault;
					SetCamera(VisibleBounds.Value, ((Rect2)(ref _canvas)).Grow(-8f));
					_validSamples++;
				}
			}
		}
		Complete = _validSamples >= 2 || Samples >= 6;
		return !Complete;
	}

	public void Cancel()
	{
		Complete = true;
	}

	private void SetCamera(Rect2 bounds, Rect2 area)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		(float, Vector2)? tuple = FrameworkModelPreview.FitBounds(bounds, area);
		if (tuple.HasValue)
		{
			(float, Vector2) valueOrDefault = tuple.GetValueOrDefault();
			CanvasTransform = new Transform2D(new Vector2(valueOrDefault.Item1, 0f), new Vector2(0f, valueOrDefault.Item1), valueOrDefault.Item2);
		}
	}

	private static bool Valid(Rect2 rect)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((Rect2)(ref rect)).Position;
		if (((Vector2)(ref val)).IsFinite())
		{
			val = ((Rect2)(ref rect)).Size;
			if (((Vector2)(ref val)).IsFinite())
			{
				return ((Rect2)(ref rect)).HasArea();
			}
		}
		return false;
	}
}

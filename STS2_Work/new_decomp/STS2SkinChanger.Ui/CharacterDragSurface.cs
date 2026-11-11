using System;
using System.Collections;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace STS2SkinChanger.Ui;

internal class CharacterDragSurface : Control
{
	private NCreature? _creature;

	private NMerchantCharacter? _shopPlayerVisual;

	public bool DragEnabled { get; private set; }

	public bool SelectionMode { get; private set; }

	public void SetCreature(NCreature? creature)
	{
		_creature = creature;
	}

	public void SetShopPlayerVisual(NMerchantCharacter? visual)
	{
		_shopPlayerVisual = visual;
	}

	public void SetDragEnabled(bool enabled)
	{
		DragEnabled = enabled;
		RefreshCursor();
	}

	public void SetSelectionMode(bool enabled)
	{
		SelectionMode = enabled;
		RefreshCursor();
	}

	private void RefreshCursor()
	{
		((Control)this).MouseDefaultCursorShape = (CursorShape)(SelectionMode ? 2 : (DragEnabled ? 13 : 0));
	}

	public CharacterAppearanceDragTarget HitTest(Vector2 localPosition)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		if (!DragEnabled)
		{
			return CharacterAppearanceDragTarget.None;
		}
		Control healthBarBounds = CharacterAppearanceRuntime.GetHealthBarBounds(_creature);
		if (healthBarBounds != null && TryGetCanvasRect(healthBarBounds, 12f, out var rect) && ((Rect2)(ref rect)).HasPoint(localPosition))
		{
			return CharacterAppearanceDragTarget.HealthBar;
		}
		if (CharacterAppearanceRuntime.SupportsIntentAppearance(_creature) && TryGetIntentTargetRect(out var rect2) && ((Rect2)(ref rect2)).HasPoint(localPosition))
		{
			return CharacterAppearanceDragTarget.Intent;
		}
		NSelectionReticle selectionReticle = CharacterAppearanceRuntime.GetSelectionReticle(_creature);
		if (selectionReticle != null && ((CanvasItem)selectionReticle).IsVisibleInTree() && TryGetCanvasRect((Control)(object)selectionReticle, 0f, out var rect3) && IsNearRectBorder(rect3, localPosition, 20f))
		{
			return CharacterAppearanceDragTarget.SelectionReticle;
		}
		Control modelBounds = CharacterAppearanceRuntime.GetModelBounds(_creature);
		if (modelBounds != null && TryGetCanvasRect(modelBounds, 8f, out var rect4) && ((Rect2)(ref rect4)).HasPoint(localPosition))
		{
			return CharacterAppearanceDragTarget.Model;
		}
		if (_shopPlayerVisual != null && TryGetNode2DTargetRect((Node2D)(object)_shopPlayerVisual, new Rect2(-190f, -450f, 380f, 520f), out var rect5) && ((Rect2)(ref rect5)).HasPoint(localPosition))
		{
			return CharacterAppearanceDragTarget.Model;
		}
		return CharacterAppearanceDragTarget.None;
	}

	private static bool IsNearRectBorder(Rect2 rect, Vector2 point, float width)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		Rect2 val = ((Rect2)(ref rect)).Grow(width);
		if (!((Rect2)(ref val)).HasPoint(point))
		{
			return false;
		}
		float num = Math.Max(0f, ((Rect2)(ref rect)).Size.X - width * 2f);
		float num2 = Math.Max(0f, ((Rect2)(ref rect)).Size.Y - width * 2f);
		Rect2 val2 = default(Rect2);
		((Rect2)(ref val2))._002Ector(((Rect2)(ref rect)).Position + Vector2.One * width, new Vector2(num, num2));
		if (!(num <= 0f) && !(num2 <= 0f))
		{
			return !((Rect2)(ref val2)).HasPoint(point);
		}
		return true;
	}

	private bool TryGetIntentTargetRect(out Rect2 rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		rect = default(Rect2);
		if (_creature == null || !GodotObject.IsInstanceValid((GodotObject)(object)_creature) || !GodotObject.IsInstanceValid((GodotObject)(object)_creature.IntentContainer))
		{
			return false;
		}
		bool flag = false;
		Vector2 val = Vector2.Zero;
		Vector2 val2 = Vector2.Zero;
		foreach (Control item in from control in ((IEnumerable)((Node)_creature.IntentContainer).GetChildren(false)).OfType<Control>()
			where ((CanvasItem)control).IsVisibleInTree()
			select control)
		{
			if (TryGetCanvasRect(item, 10f, out var rect2))
			{
				if (!flag)
				{
					val = ((Rect2)(ref rect2)).Position;
					val2 = ((Rect2)(ref rect2)).End;
					flag = true;
				}
				else
				{
					val = new Vector2(Math.Min(val.X, ((Rect2)(ref rect2)).Position.X), Math.Min(val.Y, ((Rect2)(ref rect2)).Position.Y));
					val2 = new Vector2(Math.Max(val2.X, ((Rect2)(ref rect2)).End.X), Math.Max(val2.Y, ((Rect2)(ref rect2)).End.Y));
				}
			}
		}
		if (!flag)
		{
			return false;
		}
		rect = new Rect2(val, val2 - val);
		return true;
	}

	public Vector2 GetTargetLocalDelta(Vector2 from, Vector2 to)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		Transform2D globalTransformWithCanvas = ((CanvasItem)this).GetGlobalTransformWithCanvas();
		Transform2D globalTransformWithCanvas2;
		if (_creature != null && GodotObject.IsInstanceValid((GodotObject)(object)_creature))
		{
			globalTransformWithCanvas2 = ((CanvasItem)_creature).GetGlobalTransformWithCanvas();
			Transform2D val = ((Transform2D)(ref globalTransformWithCanvas2)).AffineInverse();
			return val * (globalTransformWithCanvas * to) - val * (globalTransformWithCanvas * from);
		}
		if (_shopPlayerVisual != null && GodotObject.IsInstanceValid((GodotObject)(object)_shopPlayerVisual))
		{
			Node parent = ((Node)_shopPlayerVisual).GetParent();
			CanvasItem val2 = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			if (val2 != null)
			{
				globalTransformWithCanvas2 = val2.GetGlobalTransformWithCanvas();
				Transform2D val3 = ((Transform2D)(ref globalTransformWithCanvas2)).AffineInverse();
				return val3 * (globalTransformWithCanvas * to) - val3 * (globalTransformWithCanvas * from);
			}
		}
		return Vector2.Zero;
	}

	public bool TryGetCreatureTargetRect(NCreature creature, out Rect2 rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		rect = default(Rect2);
		if (GodotObject.IsInstanceValid((GodotObject)(object)creature.Hitbox))
		{
			return TryGetCanvasRect(creature.Hitbox, 8f, out rect);
		}
		return false;
	}

	public bool TryGetCanvasRect(Control control, float padding, out Rect2 rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		rect = default(Rect2);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)control))
		{
			return false;
		}
		Transform2D globalTransformWithCanvas = ((CanvasItem)this).GetGlobalTransformWithCanvas();
		Transform2D val = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse();
		Transform2D globalTransformWithCanvas2 = ((CanvasItem)control).GetGlobalTransformWithCanvas();
		Vector2 val2 = val * (globalTransformWithCanvas2 * Vector2.Zero);
		Vector2 val3 = val * (globalTransformWithCanvas2 * new Vector2(control.Size.X, 0f));
		Vector2 val4 = val * (globalTransformWithCanvas2 * control.Size);
		Vector2 val5 = val * (globalTransformWithCanvas2 * new Vector2(0f, control.Size.Y));
		Vector2 val6 = default(Vector2);
		((Vector2)(ref val6))._002Ector(Math.Min(Math.Min(val2.X, val3.X), Math.Min(val4.X, val5.X)), Math.Min(Math.Min(val2.Y, val3.Y), Math.Min(val4.Y, val5.Y)));
		Vector2 val7 = default(Vector2);
		((Vector2)(ref val7))._002Ector(Math.Max(Math.Max(val2.X, val3.X), Math.Max(val4.X, val5.X)), Math.Max(Math.Max(val2.Y, val3.Y), Math.Max(val4.Y, val5.Y)));
		Vector2 val8 = Vector2.One * padding;
		rect = new Rect2(val6 - val8, val7 - val6 + val8 * 2f);
		if (((Rect2)(ref rect)).Size.X > 0f)
		{
			return ((Rect2)(ref rect)).Size.Y > 0f;
		}
		return false;
	}

	public bool TryGetNode2DTargetRect(Node2D node, Rect2 localRect, out Rect2 rect)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		rect = default(Rect2);
		if (!GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			return false;
		}
		Transform2D globalTransformWithCanvas = ((CanvasItem)this).GetGlobalTransformWithCanvas();
		Transform2D val = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse();
		Transform2D globalTransformWithCanvas2 = ((CanvasItem)node).GetGlobalTransformWithCanvas();
		Vector2 val2 = val * (globalTransformWithCanvas2 * ((Rect2)(ref localRect)).Position);
		Vector2 val3 = val * (globalTransformWithCanvas2 * new Vector2(((Rect2)(ref localRect)).End.X, ((Rect2)(ref localRect)).Position.Y));
		Vector2 val4 = val * (globalTransformWithCanvas2 * ((Rect2)(ref localRect)).End);
		Vector2 val5 = val * (globalTransformWithCanvas2 * new Vector2(((Rect2)(ref localRect)).Position.X, ((Rect2)(ref localRect)).End.Y));
		Vector2 val6 = default(Vector2);
		((Vector2)(ref val6))._002Ector(Math.Min(Math.Min(val2.X, val3.X), Math.Min(val4.X, val5.X)), Math.Min(Math.Min(val2.Y, val3.Y), Math.Min(val4.Y, val5.Y)));
		Vector2 val7 = default(Vector2);
		((Vector2)(ref val7))._002Ector(Math.Max(Math.Max(val2.X, val3.X), Math.Max(val4.X, val5.X)), Math.Max(Math.Max(val2.Y, val3.Y), Math.Max(val4.Y, val5.Y)));
		rect = new Rect2(val6, val7 - val6);
		if (((Rect2)(ref rect)).Size.X > 0f)
		{
			return ((Rect2)(ref rect)).Size.Y > 0f;
		}
		return false;
	}
}

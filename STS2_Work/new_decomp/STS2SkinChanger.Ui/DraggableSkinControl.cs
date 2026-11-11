using System;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class DraggableSkinControl : Node
{
	private const string BindingName = "SkinChangerDragBinding";

	private Control _screen;

	private Control _target;

	private Button _handle;

	private Action _defaultPlacement;

	private Func<(float X, float Y)?> _loadPosition;

	private Action<float, float> _savePosition;

	private Action _resetPosition;

	private Action? _dragMoved;

	private Action<bool>? _dragStateChanged;

	private bool _dragging;

	private bool _placing;

	private Vector2 _dragOffset;

	internal static void Attach(Control screen, HBoxContainer target, bool mergeButton, Action defaultPlacement)
	{
		Attach(screen, target, () => SkinService.GetCharacterSkinControlPosition(mergeButton), delegate(float x, float y)
		{
			SkinService.SetCharacterSkinControlPosition(mergeButton, x, y);
		}, delegate
		{
			SkinService.ResetCharacterSkinControlPosition(mergeButton);
		}, defaultPlacement);
	}

	internal static void Attach(Control screen, HBoxContainer target, Func<(float X, float Y)?> loadPosition, Action<float, float> savePosition, Action resetPosition, Action defaultPlacement)
	{
		AttachCore(screen, (Control)(object)target, null, loadPosition, savePosition, resetPosition, defaultPlacement);
	}

	internal static void AttachWithHandle(Control screen, Control target, Button handle, Func<(float X, float Y)?> loadPosition, Action<float, float> savePosition, Action resetPosition, Action defaultPlacement, Action? dragMoved = null, Action<bool>? dragStateChanged = null)
	{
		AttachCore(screen, target, handle, loadPosition, savePosition, resetPosition, defaultPlacement, dragMoved, dragStateChanged);
	}

	private static void AttachCore(Control screen, Control target, Button? handle, Func<(float X, float Y)?> loadPosition, Action<float, float> savePosition, Action resetPosition, Action defaultPlacement, Action? dragMoved = null, Action<bool>? dragStateChanged = null)
	{
		DraggableSkinControl nodeOrNull = ((Node)target).GetNodeOrNull<DraggableSkinControl>(NodePath.op_Implicit("SkinChangerDragBinding"));
		if (nodeOrNull != null)
		{
			nodeOrNull.RestorePosition();
			return;
		}
		DraggableSkinControl draggableSkinControl = new DraggableSkinControl();
		((Node)draggableSkinControl).Name = StringName.op_Implicit("SkinChangerDragBinding");
		draggableSkinControl._screen = screen;
		draggableSkinControl._target = target;
		draggableSkinControl._handle = handle;
		draggableSkinControl._loadPosition = loadPosition;
		draggableSkinControl._savePosition = savePosition;
		draggableSkinControl._resetPosition = resetPosition;
		draggableSkinControl._dragMoved = dragMoved;
		draggableSkinControl._dragStateChanged = dragStateChanged;
		draggableSkinControl._defaultPlacement = defaultPlacement;
		nodeOrNull = draggableSkinControl;
		((Node)target).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
		nodeOrNull.Initialize();
	}

	private void Initialize()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Expected O, but got Unknown
		if (_handle == null)
		{
			_handle = new Button
			{
				Name = StringName.op_Implicit("SkinChangerDragHandle"),
				Text = "⋮",
				CustomMinimumSize = new Vector2(24f, 44f),
				FocusMode = (FocusModeEnum)0,
				MouseFilter = (MouseFilterEnum)0,
				MouseDefaultCursorShape = (CursorShape)13
			};
			ContextualSkinControls.ApplyGameTheme(_handle);
			((Control)_handle).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 22);
			_target.AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
			((Node)_target).AddChild((Node)(object)_handle, false, (InternalMode)0);
			((Node)_target).MoveChild((Node)(object)_handle, 0);
		}
		((Control)_handle).GuiInput += new GuiInputEventHandler(HandleInput);
		DragHandleHoverVisibility.Attach(_target, _handle, () => _dragging);
		((CanvasItem)_target).VisibilityChanged += CancelHiddenDrag;
		_target.Resized += ClampAfterLayout;
		_screen.Resized += RestorePosition;
		ModLocalization.Bind((Node)(object)_handle, delegate
		{
			((Control)_handle).TooltipText = ModLocalization.Get(ModText.CardSkinSelectorDragHint);
		});
		RestorePosition();
		Callable val = Callable.From((Action)RestorePosition);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void HandleInput(InputEvent input)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I8
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Invalid comparison between Unknown and I8
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
		if (val != null)
		{
			if ((long)val.ButtonIndex == 2)
			{
				if (val.Pressed)
				{
					SetDragging(dragging: false);
					_resetPosition();
					RestorePosition();
				}
				((Control)_handle).AcceptEvent();
			}
			else if ((long)val.ButtonIndex == 1)
			{
				if (val.Pressed)
				{
					Transform2D globalTransformWithCanvas = ((CanvasItem)_screen).GetGlobalTransformWithCanvas();
					Vector2 val2 = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * (((CanvasItem)_target).GetGlobalTransformWithCanvas() * (_target.Size / 2f));
					ApplyPosition(val2.X / Math.Max(1f, _screen.Size.X), val2.Y / Math.Max(1f, _screen.Size.Y));
					SetDragging(dragging: true);
					_dragOffset = ((CanvasItem)_screen).GetLocalMousePosition() - val2;
					((Control)_handle).GrabClickFocus();
				}
				else if (_dragging)
				{
					SetDragging(dragging: false);
					NormalizedControlPosition normalizedControlPosition = MoveToMouse();
					_savePosition(normalizedControlPosition.X, normalizedControlPosition.Y);
				}
				((Control)_handle).AcceptEvent();
			}
		}
		else if (input is InputEventMouseMotion && _dragging)
		{
			if (!Input.IsMouseButtonPressed((MouseButton)1) || !((Node)_screen).GetWindow().HasFocus())
			{
				SetDragging(dragging: false);
				RestorePosition();
			}
			else
			{
				MoveToMouse();
				((Control)_handle).AcceptEvent();
			}
		}
	}

	private NormalizedControlPosition MoveToMouse()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((CanvasItem)_screen).GetLocalMousePosition() - _dragOffset;
		NormalizedControlPosition result = ApplyPosition(val.X / Math.Max(1f, _screen.Size.X), val.Y / Math.Max(1f, _screen.Size.Y));
		Action? dragMoved = _dragMoved;
		if (dragMoved != null)
		{
			dragMoved();
			return result;
		}
		return result;
	}

	private void SetDragging(bool dragging)
	{
		if (_dragging != dragging)
		{
			_dragging = dragging;
			_dragStateChanged?.Invoke(dragging);
		}
	}

	internal static void CancelDrag(Control target)
	{
		DraggableSkinControl nodeOrNull = ((Node)target).GetNodeOrNull<DraggableSkinControl>(NodePath.op_Implicit("SkinChangerDragBinding"));
		if (nodeOrNull != null && nodeOrNull._dragging)
		{
			nodeOrNull.RestorePosition();
		}
	}

	private NormalizedControlPosition ApplyPosition(float x, float y)
	{
		return ApplyPosition(_screen, _target, x, y, ref _placing);
	}

	internal static void ApplyDefaultPosition(Control screen, HBoxContainer target, NormalizedControlPosition position)
	{
		bool placing = false;
		ApplyPosition(screen, (Control)(object)target, position.X, position.Y, ref placing);
	}

	private static NormalizedControlPosition ApplyPosition(Control screen, Control target, float x, float y, ref bool placing)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		Vector2 size = target.Size;
		Vector2 val = ((Vector2)(ref size)).Max(target.GetCombinedMinimumSize());
		NormalizedControlPosition result = DraggableControlPlacementPolicy.ClampNormalized(x, y, screen.Size.X, screen.Size.Y, val.X, val.Y);
		placing = true;
		try
		{
			Node parent = ((Node)target).GetParent();
			CanvasItem val2 = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			float anchorLeft;
			if (val2 != null && val2 != screen)
			{
				Transform2D globalTransformWithCanvas = ((CanvasItem)screen).GetGlobalTransformWithCanvas();
				if (!((Transform2D)(ref globalTransformWithCanvas)).IsFinite() || Mathf.IsZeroApprox(((Transform2D)(ref globalTransformWithCanvas)).Determinant()))
				{
					return result;
				}
				Transform2D parentToScreen = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * val2.GetGlobalTransformWithCanvas();
				(Vector2, Vector2)? tuple = ResolveNestedPlacement(val, screen.Size, parentToScreen, new Vector2(x, y));
				if (tuple.HasValue)
				{
					(Vector2, Vector2) valueOrDefault = tuple.GetValueOrDefault();
					(Vector2, Vector2) tuple2 = valueOrDefault;
					Vector2 item = tuple2.Item1;
					Vector2 item2 = tuple2.Item2;
					float num2 = (target.AnchorBottom = 0f);
					float num4 = (target.AnchorTop = num2);
					anchorLeft = (target.AnchorRight = num4);
					target.AnchorLeft = anchorLeft;
					target.OffsetLeft = item.X;
					target.OffsetTop = item.Y;
					target.OffsetRight = item.X + val.X;
					target.OffsetBottom = item.Y + val.Y;
					return new NormalizedControlPosition(item2.X, item2.Y);
				}
				return result;
			}
			anchorLeft = (target.AnchorRight = result.X);
			target.AnchorLeft = anchorLeft;
			anchorLeft = (target.AnchorBottom = result.Y);
			target.AnchorTop = anchorLeft;
			target.OffsetLeft = (0f - val.X) / 2f;
			target.OffsetTop = (0f - val.Y) / 2f;
			target.OffsetRight = val.X / 2f;
			target.OffsetBottom = val.Y / 2f;
			return result;
		}
		finally
		{
			placing = false;
		}
	}

	internal static (Vector2 TopLeft, Vector2 Normalized)? ResolveNestedPlacement(Vector2 size, Vector2 screenSize, Transform2D parentToScreen, Vector2 desired)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		if (!((Transform2D)(ref parentToScreen)).IsFinite() || Mathf.IsZeroApprox(((Transform2D)(ref parentToScreen)).Determinant()) || !((Vector2)(ref size)).IsFinite() || !((Vector2)(ref screenSize)).IsFinite() || screenSize.X <= 0f || screenSize.Y <= 0f)
		{
			return null;
		}
		Rect2 val = parentToScreen * new Rect2(Vector2.Zero, size);
		NormalizedControlPosition normalizedControlPosition = DraggableControlPlacementPolicy.ClampNormalized(desired.X, desired.Y, screenSize.X, screenSize.Y, ((Rect2)(ref val)).Size.X, ((Rect2)(ref val)).Size.Y);
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(normalizedControlPosition.X, normalizedControlPosition.Y);
		return (((Transform2D)(ref parentToScreen)).AffineInverse() * (val2 * screenSize) - size / 2f, val2);
	}

	internal static void RefreshPlacement(Control target)
	{
		DraggableSkinControl nodeOrNull = ((Node)target).GetNodeOrNull<DraggableSkinControl>(NodePath.op_Implicit("SkinChangerDragBinding"));
		if (nodeOrNull != null && !nodeOrNull._dragging)
		{
			nodeOrNull.RestorePosition();
		}
	}

	private void RestorePosition()
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_target))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_screen))
			{
				_screen.Resized -= RestorePosition;
			}
			return;
		}
		SetDragging(dragging: false);
		(float, float)? tuple = _loadPosition();
		if (tuple.HasValue)
		{
			(float, float) valueOrDefault = tuple.GetValueOrDefault();
			ApplyPosition(valueOrDefault.Item1, valueOrDefault.Item2);
		}
		else
		{
			_defaultPlacement();
		}
	}

	private void CancelHiddenDrag()
	{
		if (!((CanvasItem)_target).IsVisibleInTree())
		{
			SetDragging(dragging: false);
		}
	}

	private void ClampAfterLayout()
	{
		if (!_placing && !_dragging)
		{
			(float, float)? tuple = _loadPosition();
			if (tuple.HasValue)
			{
				(float, float) valueOrDefault = tuple.GetValueOrDefault();
				ApplyPosition(valueOrDefault.Item1, valueOrDefault.Item2);
			}
		}
	}

	public override void _ExitTree()
	{
		SetDragging(dragging: false);
		if (GodotObject.IsInstanceValid((GodotObject)(object)_screen))
		{
			_screen.Resized -= RestorePosition;
		}
	}
}

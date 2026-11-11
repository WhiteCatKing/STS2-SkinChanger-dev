using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ModLaunchManager;

internal sealed class ModReorderTree : Tree
{
	private const float DragThreshold = 5f;

	private const float TopEdgeZone = 64f;

	private const float BottomEdgeZone = 36f;

	private const float EdgeScrollSpeed = 900f;

	private const double DwellSeconds = 0.7;

	private string _pendingKey = string.Empty;

	private string _targetKey = string.Empty;

	private TreeDropPlacement _targetPlacement;

	private Vector2 _pressPosition;

	private Vector2 _lastPointer;

	private TreeItem? _highlightedTarget;

	private readonly List<Color> _highlightedColors = new List<Color>();

	private bool _dragging;

	private bool _pendingFromDoubleClick;

	private bool _dragEnabled = true;

	private ColorRect? _dropLine;

	private SceneTree? _frameSource;

	private string _dwellKey = string.Empty;

	private double _dwellTime;

	internal Func<string, bool>? CanStartDrag { get; set; }

	internal Func<string, string, bool>? CanDropEntry { get; set; }

	internal Func<string, string, float, TreeDropPlacement>? PlacementFor { get; set; }

	internal Action<string, string>? DropEntry { get; set; }

	internal Action<string, string, TreeDropPlacement>? DropEntryAt { get; set; }

	internal Action<string, string, TreeDropPlacement>? DropTargetChanged { get; set; }

	internal Action<string>? HeadingDwell { get; set; }

	internal Action<int>? MoveRequested { get; set; }

	internal Action? ActivateRequested { get; set; }

	internal Action<int>? SidewaysRequested { get; set; }

	internal Action<bool>? DragActiveChanged { get; set; }

	internal Action<string, Vector2>? ContextMenuRequested { get; set; }

	internal Action<string>? ToggleRequested { get; set; }

	internal Func<string, bool>? IsHeaderKey { get; set; }

	internal Action<string>? HeaderActivated { get; set; }

	internal int ToggleColumn { get; set; } = -1;

	internal bool DragEnabled
	{
		get
		{
			return _dragEnabled;
		}
		set
		{
			_dragEnabled = value;
			if (!value)
			{
				ClearDragState();
			}
		}
	}

	internal ModReorderTree()
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		((Control)this).GuiInput += new GuiInputEventHandler(HandleGuiInput);
		((Node)this).TreeExiting += delegate
		{
			FollowFrames(follow: false);
		};
	}

	private void HandleGuiInput(InputEvent inputEvent)
	{
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Invalid comparison between Unknown and I8
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Invalid comparison between Unknown and I8
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Invalid comparison between Unknown and I8
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Invalid comparison between Unknown and I8
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I8
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Invalid comparison between Unknown and I8
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Invalid comparison between Unknown and I8
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I8
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Invalid comparison between Unknown and I8
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Invalid comparison between Unknown and I8
		if (!_dragEnabled)
		{
			return;
		}
		InputEventKey val = (InputEventKey)(object)((inputEvent is InputEventKey) ? inputEvent : null);
		bool flag;
		if (val != null && val.Pressed && !((InputEventWithModifiers)val).AltPressed && !((InputEventWithModifiers)val).CtrlPressed && !((InputEventWithModifiers)val).ShiftPressed && !((InputEventWithModifiers)val).MetaPressed)
		{
			Key keycode = val.Keycode;
			flag = (((long)keycode == 32 || (long)(keycode - 4194309) <= 1L) ? true : false);
			if (flag && ActivateRequested != null)
			{
				((Control)this).AcceptEvent();
				if (!val.Echo)
				{
					ActivateRequested();
				}
				return;
			}
			keycode = val.Keycode;
			flag = (((long)keycode == 4194319 || (long)keycode == 4194321) ? true : false);
			if (flag && SidewaysRequested != null)
			{
				((Control)this).AcceptEvent();
				SidewaysRequested(((long)val.Keycode != 4194319) ? 1 : (-1));
				return;
			}
		}
		InputEventKey val2 = (InputEventKey)(object)((inputEvent is InputEventKey) ? inputEvent : null);
		flag = val2 != null && val2.Pressed && ((InputEventWithModifiers)val2).AltPressed;
		if (flag)
		{
			Key keycode = val2.Keycode;
			bool flag2 = (((long)keycode == 4194320 || (long)keycode == 4194322) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			((Control)this).AcceptEvent();
			MoveRequested?.Invoke(((long)val2.Keycode != 4194320) ? 1 : (-1));
			return;
		}
		InputEventMouseButton val3 = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val3 != null && (long)val3.ButtonIndex == 2 && val3.Pressed)
		{
			string text = EntryKey(((Tree)this).GetItemAtPosition(((InputEventMouse)val3).Position));
			if (!string.IsNullOrEmpty(text))
			{
				ClearDragState();
				((Control)this).AcceptEvent();
				ContextMenuRequested?.Invoke(text, ((InputEventMouse)val3).Position);
			}
			return;
		}
		InputEventMouseButton val4 = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val4 != null && (long)val4.ButtonIndex == 1)
		{
			if (val4.Pressed)
			{
				RequestToggle(((InputEventMouse)val4).Position);
				BeginPotentialDrag(((InputEventMouse)val4).Position, val4.DoubleClick);
				if (string.IsNullOrEmpty(_pendingKey) && !val4.DoubleClick)
				{
					ActivateHeaderAt(((InputEventMouse)val4).Position);
				}
			}
			else
			{
				FinishDrag(released: true);
			}
			return;
		}
		InputEventMouseMotion val5 = (InputEventMouseMotion)(object)((inputEvent is InputEventMouseMotion) ? inputEvent : null);
		if (val5 != null && !string.IsNullOrEmpty(_pendingKey))
		{
			if (Input.IsMouseButtonPressed((MouseButton)1))
			{
				UpdateDrag(((InputEventMouse)val5).Position);
			}
			else
			{
				FinishDrag(released: false);
			}
		}
	}

	private void RequestToggle(Vector2 position)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (ToggleColumn >= 0 && ((Tree)this).GetColumnAtPosition(position) == ToggleColumn)
		{
			string text = EntryKey(((Tree)this).GetItemAtPosition(position));
			if (!string.IsNullOrEmpty(text))
			{
				ToggleRequested?.Invoke(text);
			}
		}
	}

	private void ActivateHeaderAt(Vector2 position)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (IsHeaderKey != null && ((Tree)this).GetColumnAtPosition(position) != ToggleColumn)
		{
			string text = EntryKey(((Tree)this).GetItemAtPosition(position));
			if (!string.IsNullOrEmpty(text) && IsHeaderKey(text))
			{
				((Control)this).AcceptEvent();
				HeaderActivated?.Invoke(text);
			}
		}
	}

	private void BeginPotentialDrag(Vector2 position, bool doubleClick)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (((Tree)this).GetColumnAtPosition(position) != 0)
		{
			ClearDragState();
			return;
		}
		TreeItem itemAtPosition = ((Tree)this).GetItemAtPosition(position);
		string text = EntryKey(itemAtPosition);
		if (itemAtPosition != null && !string.IsNullOrEmpty(text))
		{
			Func<string, bool>? canStartDrag = CanStartDrag;
			if (canStartDrag != null && canStartDrag(text))
			{
				_pendingKey = text;
				_pendingFromDoubleClick = doubleClick;
				_pressPosition = position;
				_lastPointer = position;
				SetDragging(dragging: false);
				Func<string, bool>? isHeaderKey = IsHeaderKey;
				if (isHeaderKey == null || !isHeaderKey(text))
				{
					itemAtPosition.Select(2);
				}
				((Control)this).AcceptEvent();
				return;
			}
		}
		ClearDragState();
	}

	private void UpdateDrag(Vector2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_lastPointer = position;
		if (_dragging || !(((Vector2)(ref position)).DistanceTo(_pressPosition) < 5f))
		{
			SetDragging(dragging: true);
			UpdateTarget(position);
			((Control)this).AcceptEvent();
		}
	}

	private void UpdateTarget(Vector2 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		TreeItem val = ((Tree)this).GetItemAtPosition(position);
		string text = EntryKey(val);
		TreeDropPlacement treeDropPlacement = TreeDropPlacement.None;
		if (val != null && !string.IsNullOrEmpty(text) && !string.Equals(_pendingKey, text, StringComparison.Ordinal))
		{
			Func<string, string, bool>? canDropEntry = CanDropEntry;
			if (canDropEntry != null && canDropEntry(_pendingKey, text))
			{
				Rect2 itemAreaRect = ((Tree)this).GetItemAreaRect(val, -1, -1);
				float arg = ((((Rect2)(ref itemAreaRect)).Size.Y > 0f) ? Math.Clamp((position.Y - ((Rect2)(ref itemAreaRect)).Position.Y) / ((Rect2)(ref itemAreaRect)).Size.Y, 0f, 1f) : 0.5f);
				treeDropPlacement = PlacementFor?.Invoke(_pendingKey, text, arg) ?? TreeDropPlacement.Into;
			}
		}
		if (treeDropPlacement == TreeDropPlacement.None)
		{
			val = null;
			text = string.Empty;
		}
		bool num = !string.Equals(_targetKey, text, StringComparison.Ordinal) || _targetPlacement != treeDropPlacement;
		_targetKey = text;
		_targetPlacement = treeDropPlacement;
		if (val == null)
		{
			ClearDropHighlight();
		}
		else if (treeDropPlacement == TreeDropPlacement.Into)
		{
			HideDropLine();
			HighlightTarget(val);
		}
		else
		{
			ClearHighlightedRow();
			ShowDropLine(val, treeDropPlacement);
		}
		if (num)
		{
			DropTargetChanged?.Invoke(_pendingKey, text, treeDropPlacement);
		}
	}

	private void FinishDrag(bool released)
	{
		if (string.IsNullOrEmpty(_pendingKey))
		{
			return;
		}
		string pendingKey = _pendingKey;
		string targetKey = _targetKey;
		TreeDropPlacement targetPlacement = _targetPlacement;
		bool num = _dragging && !string.IsNullOrEmpty(targetKey) && targetPlacement != TreeDropPlacement.None;
		bool flag = released && !_dragging && !_pendingFromDoubleClick && (IsHeaderKey?.Invoke(pendingKey) ?? false);
		ClearDragState();
		((Control)this).AcceptEvent();
		if (num)
		{
			if (DropEntryAt != null)
			{
				DropEntryAt(pendingKey, targetKey, targetPlacement);
			}
			else
			{
				DropEntry?.Invoke(pendingKey, targetKey);
			}
		}
		else if (flag)
		{
			HeaderActivated?.Invoke(pendingKey);
		}
	}

	private void OnDragFrame()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		if (!_dragging || !((Node)this).IsInsideTree())
		{
			FollowFrames(follow: false);
		}
		else
		{
			if (!Input.IsMouseButtonPressed((MouseButton)1))
			{
				return;
			}
			double processDeltaTime = ((Node)this).GetProcessDeltaTime();
			Vector2 lastPointer = _lastPointer;
			float num = 0f;
			if (lastPointer.X >= -40f && lastPointer.X <= ((Control)this).Size.X + 40f)
			{
				if (lastPointer.Y < 64f)
				{
					num = 0f - Math.Min(2f, (64f - lastPointer.Y) / 64f);
				}
				else if (lastPointer.Y > ((Control)this).Size.Y - 36f)
				{
					num = Math.Min(2f, (lastPointer.Y - (((Control)this).Size.Y - 36f)) / 36f);
				}
			}
			if (num != 0f)
			{
				VScrollBar val = ((IEnumerable)((Node)this).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
				if (val != null && ((CanvasItem)val).Visible)
				{
					((Range)val).Value = ((Range)val).Value + (double)(num * 900f) * processDeltaTime;
				}
			}
			UpdateTarget(lastPointer);
			TreeItem itemAtPosition = ((Tree)this).GetItemAtPosition(lastPointer);
			string text = EntryKey(itemAtPosition);
			if (itemAtPosition != null && itemAtPosition.Collapsed && itemAtPosition.GetFirstChild() != null)
			{
				Func<string, bool>? isHeaderKey = IsHeaderKey;
				if (isHeaderKey != null && isHeaderKey(text))
				{
					if (!string.Equals(_dwellKey, text, StringComparison.Ordinal))
					{
						_dwellKey = text;
						_dwellTime = 0.0;
					}
					_dwellTime += processDeltaTime;
					if (_dwellTime >= 0.7)
					{
						_dwellTime = double.NegativeInfinity;
						HeadingDwell?.Invoke(text);
					}
					return;
				}
			}
			_dwellKey = string.Empty;
			_dwellTime = 0.0;
		}
	}

	private void FollowFrames(bool follow)
	{
		if (follow == (_frameSource != null))
		{
			return;
		}
		if (follow)
		{
			if (((Node)this).IsInsideTree())
			{
				_frameSource = ((Node)this).GetTree();
				_frameSource.ProcessFrame += OnDragFrame;
			}
		}
		else
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)_frameSource))
			{
				_frameSource.ProcessFrame -= OnDragFrame;
			}
			_frameSource = null;
		}
	}

	private void HighlightTarget(TreeItem target)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (_highlightedTarget != target)
		{
			ClearHighlightedRow();
			_highlightedTarget = target;
			for (int i = 0; i < ((Tree)this).Columns; i++)
			{
				_highlightedColors.Add(target.GetCustomBgColor(i));
				target.SetCustomBgColor(i, new Color(UiKit.Accent.R, UiKit.Accent.G, UiKit.Accent.B, 0.34f), false);
			}
		}
	}

	private void ShowDropLine(TreeItem target, TreeDropPlacement placement)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (_dropLine == null || !GodotObject.IsInstanceValid((GodotObject)(object)_dropLine))
		{
			_dropLine = new ColorRect
			{
				MouseFilter = (MouseFilterEnum)2,
				Visible = false
			};
			((Node)this).AddChild((Node)(object)_dropLine, false, (InternalMode)0);
		}
		Rect2 itemAreaRect = ((Tree)this).GetItemAreaRect(target, -1, -1);
		float num = (float)((Control)this).GetThemeConstant(StringName.op_Implicit("v_separation"), (StringName)null) / 2f;
		float num2 = ((placement == TreeDropPlacement.Before) ? (((Rect2)(ref itemAreaRect)).Position.Y - num) : (((Rect2)(ref itemAreaRect)).End.Y + num));
		_dropLine.Color = UiKit.Accent;
		((Control)_dropLine).Position = new Vector2(((Rect2)(ref itemAreaRect)).Position.X, num2 - 1.5f);
		float num3 = ((Control)this).Size.X - ((Control)this).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)2);
		VScrollBar val = ((IEnumerable)((Node)this).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
		float val2 = num3 - ((val != null && ((CanvasItem)val).Visible) ? ((Control)val).Size.X : 0f);
		((Control)_dropLine).Size = new Vector2(Math.Max(0f, Math.Min(((Rect2)(ref itemAreaRect)).End.X, val2) - ((Rect2)(ref itemAreaRect)).Position.X), 3f);
		((CanvasItem)_dropLine).Visible = true;
	}

	private void HideDropLine()
	{
		if (_dropLine != null && GodotObject.IsInstanceValid((GodotObject)(object)_dropLine))
		{
			((CanvasItem)_dropLine).Visible = false;
		}
	}

	private void ClearDragState()
	{
		ClearDropHighlight();
		_pendingKey = string.Empty;
		_targetKey = string.Empty;
		_targetPlacement = TreeDropPlacement.None;
		_pendingFromDoubleClick = false;
		_dwellKey = string.Empty;
		_dwellTime = 0.0;
		SetDragging(dragging: false);
	}

	private void SetDragging(bool dragging)
	{
		if (_dragging != dragging)
		{
			_dragging = dragging;
			FollowFrames(dragging);
			DragActiveChanged?.Invoke(dragging);
		}
	}

	private void ClearDropHighlight()
	{
		ClearHighlightedRow();
		HideDropLine();
	}

	private void ClearHighlightedRow()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (_highlightedTarget != null && GodotObject.IsInstanceValid((GodotObject)(object)_highlightedTarget))
		{
			for (int i = 0; i < ((Tree)this).Columns; i++)
			{
				Color val = (Color)((i < _highlightedColors.Count) ? _highlightedColors[i] : default(Color));
				if (val.A > 0f)
				{
					_highlightedTarget.SetCustomBgColor(i, val, false);
				}
				else
				{
					_highlightedTarget.ClearCustomBgColor(i);
				}
			}
		}
		_highlightedTarget = null;
		_highlightedColors.Clear();
	}

	private static string EntryKey(TreeItem? item)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		object obj;
		if (item == null)
		{
			obj = null;
		}
		else
		{
			Variant metadata = item.GetMetadata(0);
			obj = ((Variant)(ref metadata)).AsString();
		}
		if (obj == null)
		{
			obj = string.Empty;
		}
		return (string)obj;
	}
}

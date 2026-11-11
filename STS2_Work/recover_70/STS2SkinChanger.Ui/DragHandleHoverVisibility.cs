using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class DragHandleHoverVisibility : Node
{
	private const string BindingName = "SkinChangerDragHandleHover";

	private Control _target;

	private Button _handle;

	private Func<bool> _isDragging;

	private Color _shownColor;

	private Window? _window;

	private bool _refreshQueued;

	private bool _connected;

	private readonly List<Control> _hoverTargets = new List<Control>();

	private bool _pointerSeen;

	private int _diagnosticsLeft;

	private string? _lastDiagnostic;

	internal static void Attach(Control target, Button handle, Func<bool> isDragging)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)target).GetNodeOrNull<DragHandleHoverVisibility>(NodePath.op_Implicit("SkinChangerDragHandleHover")) == null)
		{
			DragHandleHoverVisibility dragHandleHoverVisibility = new DragHandleHoverVisibility();
			((Node)dragHandleHoverVisibility).Name = StringName.op_Implicit("SkinChangerDragHandleHover");
			dragHandleHoverVisibility._target = target;
			dragHandleHoverVisibility._handle = handle;
			dragHandleHoverVisibility._isDragging = isDragging;
			dragHandleHoverVisibility._shownColor = ((CanvasItem)handle).Modulate;
			DragHandleHoverVisibility dragHandleHoverVisibility2 = dragHandleHoverVisibility;
			dragHandleHoverVisibility2.ApplyShown(shown: false);
			((Node)dragHandleHoverVisibility2).TreeEntered += dragHandleHoverVisibility2.Initialize;
			((Node)dragHandleHoverVisibility2).TreeExiting += dragHandleHoverVisibility2.Disconnect;
			((Node)target).AddChild((Node)(object)dragHandleHoverVisibility2, false, (InternalMode)0);
			dragHandleHoverVisibility2.Initialize();
		}
	}

	private void Initialize()
	{
		if (_connected || !((Node)this).IsInsideTree())
		{
			return;
		}
		_connected = true;
		_pointerSeen = false;
		_diagnosticsLeft = 6;
		_lastDiagnostic = null;
		_window = ((Node)_target).GetWindow();
		WatchHoverTarget(_target);
		foreach (Control item in ((IEnumerable)((Node)_target).GetChildren(false)).OfType<Control>())
		{
			WatchHoverTarget(item);
		}
		((CanvasItem)_target).ItemRectChanged += QueueRefresh;
		((CanvasItem)_target).VisibilityChanged += OnVisibilityChanged;
		_window.FocusEntered += QueueRefresh;
		_window.FocusExited += OnWindowUnavailable;
		_window.MouseEntered += QueueRefresh;
		_window.MouseExited += OnWindowUnavailable;
		OnVisibilityChanged();
		ModLog.Info("拖拽柄悬停事件已接入：" + StringName.op_Implicit(((Node)_target).Name));
	}

	private void WatchHoverTarget(Control target)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		_hoverTargets.Add(target);
		target.MouseEntered += OnPointerActivity;
		target.MouseExited += OnPointerActivity;
		target.GuiInput += new GuiInputEventHandler(OnPointerInput);
	}

	private void OnPointerActivity()
	{
		_pointerSeen = true;
		QueueRefresh();
	}

	private void OnPointerInput(InputEvent input)
	{
		if (input is InputEventMouse)
		{
			OnPointerActivity();
		}
	}

	private void QueueRefresh()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_refreshQueued || !((Node)this).IsInsideTree() || ((GodotObject)this).IsQueuedForDeletion())
		{
			return;
		}
		_refreshQueued = true;
		Callable val = Callable.From((Action)delegate
		{
			_refreshQueued = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)this) && _connected && ((Node)this).IsInsideTree() && !((GodotObject)this).IsQueuedForDeletion())
			{
				Refresh();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void Refresh()
	{
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_target) || !GodotObject.IsInstanceValid((GodotObject)(object)_handle))
		{
			return;
		}
		Control val = ((Node)_target).GetViewport().GuiGetHoveredControl();
		bool flag = val != null && (val == _target || ((Node)_target).IsAncestorOf((Node)(object)val));
		bool flag2 = ((CanvasItem)_target).IsVisibleInTree();
		bool flag3 = GodotObject.IsInstanceValid((GodotObject)(object)_window) && _window.HasFocus();
		bool flag4 = _isDragging();
		ApplyShown(ShouldReveal(flag2, flag3, flag4, Input.IsMouseButtonPressed((MouseButton)1), flag));
		if (_pointerSeen && _diagnosticsLeft > 0)
		{
			string text = $"hover={((val != null) ? ((Node)val).Name : null)}, visible={flag2}, focus={flag3}, over={flag}, drag={flag4}, alpha={((CanvasItem)_handle).Modulate.A * ((CanvasItem)_handle).SelfModulate.A}";
			if (text != _lastDiagnostic)
			{
				_lastDiagnostic = text;
				_diagnosticsLeft--;
				ModLog.Info($"拖拽柄悬停状态 {((Node)_target).Name}：{text}");
			}
		}
	}

	internal static bool ShouldReveal(bool targetVisible, bool windowFocused, bool dragging, bool leftPressed, bool pointerOverTarget)
	{
		if (targetVisible && windowFocused)
		{
			if (!pointerOverTarget)
			{
				return dragging && leftPressed;
			}
			return true;
		}
		return false;
	}

	private void ApplyShown(bool shown)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)_handle))
		{
			Color shownColor = _shownColor;
			if (!shown)
			{
				shownColor.A = 0f;
			}
			if (((CanvasItem)_handle).Modulate != shownColor)
			{
				((CanvasItem)_handle).Modulate = shownColor;
			}
		}
	}

	private void OnVisibilityChanged()
	{
		if (((CanvasItem)_target).IsVisibleInTree())
		{
			QueueRefresh();
		}
		else
		{
			ApplyShown(shown: false);
		}
	}

	private void OnWindowUnavailable()
	{
		ApplyShown(shown: false);
	}

	private void Disconnect()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		if (!_connected)
		{
			return;
		}
		_connected = false;
		ApplyShown(shown: false);
		_refreshQueued = false;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_target))
		{
			((CanvasItem)_target).ItemRectChanged -= QueueRefresh;
			((CanvasItem)_target).VisibilityChanged -= OnVisibilityChanged;
		}
		foreach (Control hoverTarget in _hoverTargets)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)hoverTarget))
			{
				hoverTarget.MouseEntered -= OnPointerActivity;
				hoverTarget.MouseExited -= OnPointerActivity;
				hoverTarget.GuiInput -= new GuiInputEventHandler(OnPointerInput);
			}
		}
		_hoverTargets.Clear();
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			_window.FocusEntered -= QueueRefresh;
			_window.FocusExited -= OnWindowUnavailable;
			_window.MouseEntered -= QueueRefresh;
			_window.MouseExited -= OnWindowUnavailable;
		}
		_window = null;
	}
}

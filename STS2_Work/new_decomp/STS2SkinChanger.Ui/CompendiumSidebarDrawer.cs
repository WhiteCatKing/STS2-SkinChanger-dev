using System;
using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;

namespace STS2SkinChanger.Ui;

internal sealed class CompendiumSidebarDrawer
{
	private readonly Control _panel;

	private readonly Control _parent;

	private readonly Func<bool> _canInteract;

	private readonly HashSet<PopupMenu> _openPopups = new HashSet<PopupMenu>();

	private Window? _window;

	private Tween? _tween;

	private bool _expanded;

	private bool _keyboard;

	private bool _idleAllowed;

	private int _animationVersion;

	private static bool Instant
	{
		get
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Invalid comparison between Unknown and I4
			SaveManager instance = SaveManager.Instance;
			if (instance == null)
			{
				return false;
			}
			PrefsSave prefsSave = instance.PrefsSave;
			return (int)((prefsSave != null) ? new FastModeType?(prefsSave.FastMode) : ((FastModeType?)null)).GetValueOrDefault() == 3;
		}
	}

	public CompendiumSidebarDrawer(Control panel, Func<bool> canInteract)
	{
		CompendiumSidebarDrawer compendiumSidebarDrawer = this;
		_panel = panel;
		_parent = ((Node)panel).GetParent<Control>();
		_canInteract = canInteract;
		((Node)panel).TreeEntered += Connect;
		((Node)panel).TreeExiting += Disconnect;
		((CanvasItem)panel).VisibilityChanged += delegate
		{
			if (((CanvasItem)panel).IsVisibleInTree())
			{
				compendiumSidebarDrawer.Connect();
			}
			else
			{
				compendiumSidebarDrawer.Disconnect();
			}
		};
		_parent.Resized += Reposition;
		panel.Resized += Reposition;
		Connect();
	}

	public void KeepOpenFor(OptionButton selector)
	{
		PopupMenu popup = selector.GetPopup();
		((Window)popup).AboutToPopup += delegate
		{
			_openPopups.Add(popup);
			Refresh();
		};
		((Popup)popup).PopupHide += delegate
		{
			_openPopups.Remove(popup);
			Refresh();
		};
	}

	private void Connect()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		if (_window == null && ((Node)_panel).IsInsideTree() && ((CanvasItem)_panel).IsVisibleInTree())
		{
			_window = ((Node)_panel).GetWindow();
			_window.WindowInput += new WindowInputEventHandler(OnInput);
			_expanded = false;
			_keyboard = false;
			Reposition();
		}
	}

	private void Disconnect()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			_window.WindowInput -= new WindowInputEventHandler(OnInput);
		}
		_window = null;
		StopAnimation();
		_openPopups.Clear();
		_expanded = false;
		_keyboard = false;
	}

	private void OnInput(InputEvent input)
	{
		if (input is InputEventMouse)
		{
			_keyboard = false;
			goto IL_0081;
		}
		InputEventKey val = (InputEventKey)(object)((input is InputEventKey) ? input : null);
		if (val != null)
		{
			if (val.Pressed)
			{
				goto IL_0037;
			}
		}
		else
		{
			InputEventJoypadButton val2 = (InputEventJoypadButton)(object)((input is InputEventJoypadButton) ? input : null);
			if (val2 != null && val2.Pressed)
			{
				goto IL_0037;
			}
		}
		bool flag = false;
		goto IL_003d;
		IL_003d:
		bool flag2 = flag;
		bool flag3;
		if (!flag2)
		{
			InputEventJoypadMotion val3 = (InputEventJoypadMotion)(object)((input is InputEventJoypadMotion) ? input : null);
			if (val3 != null)
			{
				float axisValue = val3.AxisValue;
				if (axisValue > 0.35f || axisValue < -0.35f)
				{
					flag3 = true;
					goto IL_0071;
				}
			}
			flag3 = false;
			goto IL_0071;
		}
		goto IL_0074;
		IL_0081:
		Refresh();
		return;
		IL_0071:
		flag2 = flag3;
		goto IL_0074;
		IL_0074:
		if (flag2)
		{
			_keyboard = true;
			goto IL_0081;
		}
		return;
		IL_0037:
		flag = true;
		goto IL_003d;
	}

	public void Refresh()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		if (_window == null || !((CanvasItem)_panel).IsVisibleInTree())
		{
			return;
		}
		Control val = ((Node)_panel).GetViewport().GuiGetFocusOwner();
		Rect2 globalRect = _panel.GetGlobalRect();
		bool flag = CompendiumSidebarPolicy.ShouldExpand(((Rect2)(ref globalRect)).HasPoint(((CanvasItem)_panel).GetGlobalMousePosition()), _openPopups.Count > 0, _keyboard && val != null && (_panel == val || ((Node)_panel).IsAncestorOf((Node)(object)val)), _canInteract());
		bool flag2 = _canInteract() && !Instant;
		if (flag == _expanded && flag2 == _idleAllowed)
		{
			return;
		}
		_expanded = flag;
		_idleAllowed = flag2;
		StopAnimation();
		Vector2 val2 = TargetPosition();
		if (Instant)
		{
			_panel.Position = val2;
			return;
		}
		_tween = ((Node)_panel).CreateTween().SetIgnoreTimeScale(true).SetEase((EaseType)1)
			.SetTrans((TransitionType)7);
		_tween.TweenProperty((GodotObject)(object)_panel, NodePath.op_Implicit("position"), Variant.op_Implicit(val2), flag ? 0.2 : 0.15);
		int version = _animationVersion;
		if (flag)
		{
			return;
		}
		_tween.TweenCallback(Callable.From((Action)delegate
		{
			if (version == _animationVersion)
			{
				StartIdle();
			}
		}));
	}

	private void StopAnimation()
	{
		_animationVersion++;
		Tween? tween = _tween;
		if (tween != null)
		{
			tween.Kill();
		}
		_tween = null;
	}

	private void StartIdle()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (_window != null && !_expanded && ((CanvasItem)_panel).IsVisibleInTree() && _canInteract() && !Instant)
		{
			StopAnimation();
			_tween = ((Node)_panel).CreateTween().SetIgnoreTimeScale(true).SetLoops(0)
				.SetEase((EaseType)2)
				.SetTrans((TransitionType)1);
			_tween.TweenProperty((GodotObject)(object)_panel, NodePath.op_Implicit("position:x"), Variant.op_Implicit(CompendiumSidebarPolicy.IdleX(_parent.Size.X, _panel.Size.X, outer: false)), 1.8);
			_tween.TweenProperty((GodotObject)(object)_panel, NodePath.op_Implicit("position:x"), Variant.op_Implicit(CompendiumSidebarPolicy.IdleX(_parent.Size.X, _panel.Size.X, outer: true)), 1.8);
		}
	}

	private Vector2 TargetPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(CompendiumSidebarPolicy.X(_parent.Size.X, _panel.Size.X, _expanded), 0f);
	}

	private void Reposition()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)_panel) && ((Node)_panel).IsInsideTree())
		{
			StopAnimation();
			_panel.Position = TargetPosition();
			_idleAllowed = _canInteract() && !Instant;
			StartIdle();
		}
	}
}

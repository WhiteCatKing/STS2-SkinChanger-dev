using System;
using Godot;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace CznCore.Settings;

internal class CznSettingsNativeInputOwner : NButton
{
	private Panel _focusRing;

	private BaseButton? _visualControl;

	private Action? _onReleased;

	private Action<int>? _onDirection;

	private bool _lastDisabled;

	internal Control VisualControl
	{
		get
		{
			if (_visualControl == null)
			{
				return (Control)(object)this;
			}
			return (Control)(object)_visualControl;
		}
	}

	internal void Bind(BaseButton visualControl, Action onReleased, Action<int>? onDirection)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		_visualControl = visualControl;
		_onReleased = onReleased;
		_onDirection = onDirection;
		_lastDisabled = visualControl.Disabled;
		_focusRing = new Panel
		{
			Name = StringName.op_Implicit("ControllerFocusRing"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)_focusRing).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateFocusRingStyle());
		((Node)this).AddChild((Node)(object)_focusRing, false, (InternalMode)0);
		((Control)_focusRing).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
	}

	public override void _Ready()
	{
		((NClickableControl)this).ConnectSignals();
		SyncEnabledState(force: true);
	}

	public override void _Process(double delta)
	{
		SyncEnabledState(force: false);
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (_visualControl == null || _visualControl.Disabled)
		{
			return;
		}
		if (_onDirection != null)
		{
			int num = 0;
			if (inputEvent.IsActionPressed(MegaInput.left, false, false))
			{
				num = -1;
			}
			else if (inputEvent.IsActionPressed(MegaInput.right, false, false))
			{
				num = 1;
			}
			if (num != 0)
			{
				_onDirection(num);
				((Control)this).AcceptEvent();
				return;
			}
		}
		((NClickableControl)this)._GuiInput(inputEvent);
	}

	protected override void OnFocus()
	{
		((NButton)this).OnFocus();
		((CanvasItem)_focusRing).Visible = true;
	}

	protected override void OnUnfocus()
	{
		((NClickableControl)this).OnUnfocus();
		((CanvasItem)_focusRing).Visible = false;
	}

	protected override void OnRelease()
	{
		((NClickableControl)this).OnRelease();
		_onReleased?.Invoke();
	}

	private void SyncEnabledState(bool force)
	{
		if (_visualControl == null)
		{
			return;
		}
		bool disabled = _visualControl.Disabled;
		if (force || disabled != _lastDisabled)
		{
			_lastDisabled = disabled;
			((NClickableControl)this).SetEnabled(!disabled);
			if (disabled)
			{
				((CanvasItem)_focusRing).Visible = false;
			}
		}
	}

	private static StyleBoxFlat CreateFocusRingStyle()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = Colors.Transparent,
			BorderColor = new Color(0.95f, 0.83f, 0.52f, 0.95f),
			BorderWidthLeft = 2,
			BorderWidthTop = 2,
			BorderWidthRight = 2,
			BorderWidthBottom = 2,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4
		};
	}
}

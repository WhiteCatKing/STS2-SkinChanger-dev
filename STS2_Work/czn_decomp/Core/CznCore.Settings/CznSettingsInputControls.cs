using System;
using Godot;

namespace CznCore.Settings;

internal static class CznSettingsInputControls
{
	private const string NativeOwnerName = "CznSettingsNativeInputOwner";

	internal static Button AttachNativeOwner(Button button)
	{
		AttachNativeOwner((BaseButton)(object)button, delegate
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (!((BaseButton)button).Disabled)
			{
				if (((BaseButton)button).ToggleMode)
				{
					((BaseButton)button).ButtonPressed = !((BaseButton)button).ButtonPressed;
				}
				((GodotObject)button).EmitSignal(SignalName.Pressed, Array.Empty<Variant>());
			}
		});
		return button;
	}

	internal static CheckButton AttachNativeOwner(CheckButton button)
	{
		AttachNativeOwner((BaseButton)(object)button, delegate
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (!((BaseButton)button).Disabled)
			{
				((BaseButton)button).ButtonPressed = !((BaseButton)button).ButtonPressed;
				((GodotObject)button).EmitSignal(SignalName.Pressed, Array.Empty<Variant>());
			}
		});
		return button;
	}

	internal static OptionButton AttachNativeOwner(OptionButton dropdown)
	{
		AttachNativeOwner((BaseButton)(object)dropdown, delegate
		{
			MoveSelection(dropdown, 1);
		}, delegate(int direction)
		{
			MoveSelection(dropdown, direction);
		});
		return dropdown;
	}

	internal static Control ResolveFocusOwner(Control visualControl)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		return (Control)(((object)((Node)visualControl).GetNodeOrNull<CznSettingsNativeInputOwner>(new NodePath("CznSettingsNativeInputOwner"))) ?? ((object)visualControl));
	}

	internal static Control ResolveVisualControl(Control focusControl)
	{
		if (!(focusControl is CznSettingsNativeInputOwner cznSettingsNativeInputOwner))
		{
			return focusControl;
		}
		return cznSettingsNativeInputOwner.VisualControl;
	}

	private static void AttachNativeOwner(BaseButton visualControl, Action onReleased, Action<int>? onDirection = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)visualControl).GetNodeOrNull<CznSettingsNativeInputOwner>(new NodePath("CznSettingsNativeInputOwner")) == null)
		{
			FocusModeEnum focusMode = ((Control)visualControl).FocusMode;
			((Control)visualControl).FocusMode = (FocusModeEnum)0;
			CznSettingsNativeInputOwner cznSettingsNativeInputOwner = new CznSettingsNativeInputOwner();
			((Node)cznSettingsNativeInputOwner).Name = StringName.op_Implicit("CznSettingsNativeInputOwner");
			((Control)cznSettingsNativeInputOwner).FocusMode = focusMode;
			((Control)cznSettingsNativeInputOwner).MouseFilter = (MouseFilterEnum)2;
			CznSettingsNativeInputOwner cznSettingsNativeInputOwner2 = cznSettingsNativeInputOwner;
			cznSettingsNativeInputOwner2.Bind(visualControl, onReleased, onDirection);
			((Node)visualControl).AddChild((Node)(object)cznSettingsNativeInputOwner2, false, (InternalMode)0);
			((Control)cznSettingsNativeInputOwner2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		}
	}

	private static void MoveSelection(OptionButton dropdown, int direction)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseButton)dropdown).Disabled || dropdown.ItemCount <= 0)
		{
			return;
		}
		int num = ((dropdown.Selected >= 0) ? dropdown.Selected : 0);
		for (int i = 1; i <= dropdown.ItemCount; i++)
		{
			int num2 = (num + direction * i) % dropdown.ItemCount;
			if (num2 < 0)
			{
				num2 += dropdown.ItemCount;
			}
			if (!dropdown.IsItemDisabled(num2))
			{
				if (num2 != dropdown.Selected)
				{
					dropdown.Select(num2);
					((GodotObject)dropdown).EmitSignal(SignalName.ItemSelected, (Variant[])(object)new Variant[1] { Variant.op_Implicit(num2) });
				}
				break;
			}
		}
	}
}

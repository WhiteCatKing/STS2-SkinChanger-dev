using System;
using CznCore.Settings;
using Godot;

namespace CznCore.Presentation;

internal sealed class CharacterSelectMenuToggleView : CharacterSelectMenuItemView
{
	private readonly CharacterSelectMenuToggleDescriptor _descriptor;

	private readonly Label _state;

	private readonly Panel _box = new Panel
	{
		Name = StringName.op_Implicit("SwitchTrack"),
		CustomMinimumSize = new Vector2(28f, 28f),
		MouseFilter = (MouseFilterEnum)2,
		SizeFlagsVertical = (SizeFlags)4
	};

	private readonly Label _check = CharacterSelectMenuStyle.Label("✓", 20, Colors.White);

	public CharacterSelectMenuToggleView(CharacterSelectMenuToggleDescriptor descriptor, Action changed)
		: base(descriptor, "Toggle_", 52, 6f, toggle: true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		_descriptor = descriptor;
		_state = CharacterSelectMenuStyle.Label(string.Empty, 16, descriptor.Accent);
		((Control)_state).CustomMinimumSize = new Vector2(48f, 0f);
		_state.HorizontalAlignment = (HorizontalAlignment)2;
		_state.VerticalAlignment = (VerticalAlignment)1;
		((Node)base.Row).AddChild((Node)(object)_state, false, (InternalMode)0);
		_check.HorizontalAlignment = (HorizontalAlignment)1;
		_check.VerticalAlignment = (VerticalAlignment)1;
		((Control)_check).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)_box).AddChild((Node)(object)_check, false, (InternalMode)0);
		((Node)base.Row).AddChild((Node)(object)_box, false, (InternalMode)0);
		((BaseButton)base.Button).Toggled += (ToggledEventHandler)delegate(bool value)
		{
			descriptor.SetValue(value);
			changed();
		};
	}

	public override void Refresh()
	{
		base.Refresh();
		bool flag = _descriptor.Value();
		((BaseButton)base.Button).SetPressedNoSignal(flag);
		_state.Text = (flag ? SharedSettingsText.Enabled : SharedSettingsText.Disabled).Current;
		((Control)_box).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)(flag ? CharacterSelectMenuStyle.ToggleOn : CharacterSelectMenuStyle.ToggleOff));
		((CanvasItem)_check).Visible = flag;
	}
}

using System;
using Godot;

namespace CznCore.Presentation;

internal sealed class CharacterSelectMenuActionView : CharacterSelectMenuItemView
{
	private readonly CharacterSelectMenuActionDescriptor _descriptor;

	private readonly Label _state = CharacterSelectMenuStyle.Label(string.Empty, 16, CharacterSelectMenuStyle.Text);

	public CharacterSelectMenuActionView(CharacterSelectMenuActionDescriptor descriptor, Action changed)
		: base(descriptor, "Action_", 60, 8f, toggle: false)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		_descriptor = descriptor;
		((Control)_state).CustomMinimumSize = new Vector2(170f, 0f);
		_state.HorizontalAlignment = (HorizontalAlignment)1;
		_state.VerticalAlignment = (VerticalAlignment)1;
		Label val = CharacterSelectMenuStyle.Label("›", 24, CharacterSelectMenuStyle.Text);
		val.VerticalAlignment = (VerticalAlignment)1;
		((Control)val).CustomMinimumSize = new Vector2(24f, 0f);
		PanelContainer val2 = new PanelContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CharacterSelectMenuStyle.Raised);
		HBoxContainer val3 = new HBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val3).AddChild((Node)(object)_state, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)base.Row).AddChild((Node)(object)val2, false, (InternalMode)0);
		((BaseButton)base.Button).Pressed += delegate
		{
			descriptor.Activate();
			changed();
		};
	}

	public override void Refresh()
	{
		base.Refresh();
		_state.Text = _descriptor.State();
	}
}

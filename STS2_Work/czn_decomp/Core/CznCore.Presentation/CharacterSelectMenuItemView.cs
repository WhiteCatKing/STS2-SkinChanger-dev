using Godot;

namespace CznCore.Presentation;

internal abstract class CharacterSelectMenuItemView
{
	private readonly CharacterSelectMenuItemDescriptor _descriptor;

	private readonly Label _title = CharacterSelectMenuStyle.Label(string.Empty, 16, CharacterSelectMenuStyle.Text);

	public Button Button { get; }

	protected HBoxContainer Row { get; }

	protected CharacterSelectMenuItemView(CharacterSelectMenuItemDescriptor descriptor, string prefix, int height, float padding, bool toggle)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		_descriptor = descriptor;
		Button = new Button
		{
			Name = StringName.op_Implicit(prefix + descriptor.Id),
			ToggleMode = toggle,
			CustomMinimumSize = new Vector2(0f, (float)height),
			SizeFlagsHorizontal = (SizeFlags)3,
			FocusMode = (FocusModeEnum)2
		};
		CharacterSelectMenuStyle.ApplyEntry((BaseButton)(object)Button, descriptor.Accent, toggle);
		Row = new HBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)Row).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)Row).OffsetLeft = 14f;
		((Control)Row).OffsetRight = -14f;
		((Control)Row).OffsetTop = padding;
		((Control)Row).OffsetBottom = 0f - padding;
		((Control)Row).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)Row).AddChild((Node)(object)CharacterSelectMenuStyle.Icon(descriptor.IconPath, descriptor.Accent), false, (InternalMode)0);
		((Control)_title).SizeFlagsHorizontal = (SizeFlags)3;
		_title.VerticalAlignment = (VerticalAlignment)1;
		_title.AutowrapMode = (AutowrapMode)3;
		((Node)Row).AddChild((Node)(object)_title, false, (InternalMode)0);
		((Node)Button).AddChild((Node)(object)Row, false, (InternalMode)0);
	}

	public virtual void Refresh()
	{
		_title.Text = _descriptor.Title();
		((Control)Button).TooltipText = _descriptor.Tooltip();
	}
}

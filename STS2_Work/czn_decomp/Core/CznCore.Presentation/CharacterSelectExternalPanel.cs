using System;
using CznCore.Settings;
using Godot;

namespace CznCore.Presentation;

internal sealed class CharacterSelectExternalPanel : PanelContainer
{
	private const int Layer = 125;

	private const float MaxHeight = 480f;

	private readonly Control _screen;

	private readonly Control _anchor;

	private readonly Button _back;

	public CharacterSelectExternalPanel(Control screen, Control anchor, CharacterSelectMenuSectionDescriptor section, Control content, Action closed)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		CharacterSelectExternalPanel characterSelectExternalPanel = this;
		_screen = screen;
		_anchor = anchor;
		((Node)this).Name = StringName.op_Implicit("CznGameplaySettings");
		((CanvasItem)this).Visible = false;
		((CanvasItem)this).ZIndex = 125;
		((Control)this).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CharacterSelectMenuStyle.Panel);
		MarginContainer val = new MarginContainer();
		string[] array = new string[4] { "left", "top", "right", "bottom" };
		foreach (string text in array)
		{
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_" + text), 14);
		}
		((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
		VBoxContainer val2 = new VBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer val3 = new HBoxContainer();
		((Node)val3).AddChild((Node)(object)CharacterSelectMenuStyle.Icon(section.ExternalIconPath, CharacterSelectMenuStyle.Muted), false, (InternalMode)0);
		Label val4 = CharacterSelectMenuStyle.Label(section.Title(), 20, CharacterSelectMenuStyle.Text);
		((Control)val4).SizeFlagsHorizontal = (SizeFlags)3;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		_back = CharacterSelectMenuStyle.TextButton("Back", "‹ " + SharedSettingsText.Back.Current, new Vector2(80f, 40f));
		((BaseButton)_back).Pressed += delegate
		{
			Close();
		};
		((Node)val3).AddChild((Node)(object)_back, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		ScrollContainer val5 = new ScrollContainer
		{
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0,
			FollowFocus = true
		};
		((Node)val5).AddChild((Node)(object)content, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val5, false, (InternalMode)0);
		CharacterSelectMenuStyle.CloseOnCancel((Node)(object)this, Close);
		Action unbindModal = CharacterSelectMenuStyle.BindModalLayer((Control)(object)this, 125);
		screen.Resized += Layout;
		((CanvasItem)screen).VisibilityChanged += HideWithScreen;
		((Node)this).TreeExiting += delegate
		{
			unbindModal();
			screen.Resized -= characterSelectExternalPanel.Layout;
			((CanvasItem)screen).VisibilityChanged -= HideWithScreen;
		};
		bool Close()
		{
			if (!((CanvasItem)characterSelectExternalPanel).Visible || CharacterSelectMenuStyle.NativeModalOpen)
			{
				return false;
			}
			((CanvasItem)characterSelectExternalPanel).Hide();
			closed();
			return true;
		}
		void HideWithScreen()
		{
			if (!((CanvasItem)screen).IsVisibleInTree())
			{
				((CanvasItem)characterSelectExternalPanel).Hide();
			}
		}
	}

	public void Toggle()
	{
		((CanvasItem)this).Visible = !((CanvasItem)this).Visible;
		if (((CanvasItem)this).Visible)
		{
			Layout();
			((Control)_back).GrabFocus();
		}
	}

	private void Layout()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		((Control)this).Size = new Vector2(_anchor.Size.X, Math.Min(480f, _screen.Size.Y - 48f));
		Transform2D globalTransform = ((CanvasItem)_screen).GetGlobalTransform();
		Vector2 val = ((Transform2D)(ref globalTransform)).AffineInverse() * _anchor.GlobalPosition;
		((Control)this).Position = new Vector2(Math.Clamp(val.X, 0f, Math.Max(0f, _screen.Size.X - ((Control)this).Size.X)), Math.Clamp(val.Y, 0f, Math.Max(0f, _screen.Size.Y - ((Control)this).Size.Y)));
	}
}

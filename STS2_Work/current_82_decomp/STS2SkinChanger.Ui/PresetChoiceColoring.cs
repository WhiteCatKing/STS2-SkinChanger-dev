using System;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class PresetChoiceColoring
{
	private const string ListName = "SCColoredChoices";

	internal static void Attach(OptionButton picker, Func<string, bool>? isAccented = null, bool colorSelection = true)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Expected O, but got Unknown
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Expected O, but got Unknown
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Expected O, but got Unknown
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Expected O, but got Unknown
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Expected O, but got Unknown
		if (isAccented == null)
		{
			isAccented = BundlePresetPolicy.IsOwned;
		}
		PopupMenu popup = picker.GetPopup();
		ItemList nodeOrNull = ((Node)popup).GetNodeOrNull<ItemList>(NodePath.op_Implicit("SCColoredChoices"));
		if (nodeOrNull != null)
		{
			RefreshItems(picker, nodeOrNull, isAccented);
			return;
		}
		ItemList list = new ItemList
		{
			Name = StringName.op_Implicit("SCColoredChoices"),
			MouseFilter = (MouseFilterEnum)0,
			AllowReselect = true,
			MaxColumns = 1,
			ZIndex = 100
		};
		((Node)popup).AddChild((Node)(object)list, false, (InternalMode)0);
		((Control)list).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)list).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), colorSelection ? 19 : ((Control)picker).GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null));
		((Control)list).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		Font gameFont = ContextualSkinControls.GameFont;
		if (gameFont != null)
		{
			((Control)list).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("45104e"), new Color("79547e"), 2));
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("hovered"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("2c586f"), new Color("afcdde")));
		((Control)list).AddThemeStyleboxOverride(StringName.op_Implicit("selected"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("58205f"), new Color("efc850"), 2));
		ModThemeRuntime.ItemList(list);
		list.ItemClicked += (ItemClickedEventHandler)delegate(long index, Vector2 _, long button)
		{
			if (button == 1)
			{
				Choose(index);
			}
		};
		list.ItemActivated += new ItemActivatedEventHandler(Choose);
		int focused = -1;
		if (!colorSelection)
		{
			((Control)list).GuiInput += (GuiInputEventHandler)delegate(InputEvent ev)
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				if (ev is InputEventMouseMotion)
				{
					Focus(list.GetItemAtPosition(((CanvasItem)list).GetLocalMousePosition(), true));
				}
			};
			list.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
			{
				Focus((int)index);
			};
		}
		picker.ItemSelected += (ItemSelectedEventHandler)delegate
		{
			StyleSelection();
		};
		((Window)popup).AboutToPopup += delegate
		{
			focused = -1;
			RefreshItems(picker, list, isAccented);
		};
		ModThemeRuntime.Bind((Node)(object)picker, "preset_colors", delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			StyleSelection();
			for (int i = 0; i < Math.Min(list.ItemCount, picker.ItemCount); i++)
			{
				list.SetItemCustomFgColor(i, RowColor(picker, Owned(i), i));
			}
			CharacterSkinRandomExclusionUi.Refresh(picker);
		});
		RefreshItems(picker, list, isAccented);
		void Choose(long index)
		{
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			if (index >= 0 && index < picker.ItemCount && !picker.IsItemDisabled((int)index))
			{
				picker.Select((int)index);
				((Window)popup).Hide();
				((GodotObject)picker).EmitSignal(SignalName.ItemSelected, (Variant[])(object)new Variant[1] { Variant.op_Implicit(index) });
			}
		}
		void Focus(int index)
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			if (index >= 0 && index < picker.ItemCount && focused != index)
			{
				focused = index;
				popup.SetFocusedItem(index);
				((GodotObject)popup).EmitSignal(SignalName.IdFocused, (Variant[])(object)new Variant[1] { Variant.op_Implicit(picker.GetItemId(index)) });
			}
		}
		bool Owned(int index)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			if (index >= 0 && index < picker.ItemCount)
			{
				Func<string, bool>? func = isAccented;
				Variant itemMetadata = picker.GetItemMetadata(index);
				return func(((Variant)(ref itemMetadata)).AsString());
			}
			return false;
		}
		void StyleSelection()
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			if (colorSelection)
			{
				Color val = (Owned(picker.Selected) ? ModThemeRuntime.Accent : ModThemeRuntime.Text);
				string[] array = new string[4] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" };
				foreach (string text in array)
				{
					((Control)picker).AddThemeColorOverride(StringName.op_Implicit(text), val);
				}
			}
		}
	}

	private static Color RowColor(OptionButton picker, bool owned, int index)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Color color = (owned ? ModThemeRuntime.Accent : ModThemeRuntime.Text);
		Variant itemMetadata = picker.GetItemMetadata(index);
		return SkinChoiceDimming.Row(color, picker, ((Variant)(ref itemMetadata)).AsString());
	}

	private static void RefreshItems(OptionButton picker, ItemList list, Func<string, bool> isAccented)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		list.Clear();
		for (int i = 0; i < picker.ItemCount; i++)
		{
			list.AddItem(picker.GetItemText(i), (Texture2D)null, true);
			list.SetItemDisabled(i, picker.IsItemDisabled(i));
			ItemList obj = list;
			int num = i;
			OptionButton picker2 = picker;
			Variant itemMetadata = picker.GetItemMetadata(i);
			obj.SetItemCustomFgColor(num, RowColor(picker2, isAccented(((Variant)(ref itemMetadata)).AsString()), i));
		}
		if (picker.Selected >= 0 && picker.Selected < list.ItemCount)
		{
			list.Select(picker.Selected, true);
		}
		CharacterSkinRandomExclusionUi.Refresh(picker);
		Callable val = Callable.From((Action)delegate
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)list) && GodotObject.IsInstanceValid((GodotObject)(object)picker))
			{
				PopupMenu popup = picker.GetPopup();
				if (((Window)popup).Visible)
				{
					((Control)list).Position = Vector2.Zero;
					((Control)list).Size = Vector2I.op_Implicit(((Window)popup).Size);
					((Control)list).GrabFocus();
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}
}

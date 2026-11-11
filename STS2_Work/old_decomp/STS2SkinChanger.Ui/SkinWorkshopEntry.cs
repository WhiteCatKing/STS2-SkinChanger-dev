using System;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class SkinWorkshopEntry
{
	internal static void Append(OptionButton dropdown, bool characterPopup = false)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		int itemCount = dropdown.ItemCount;
		dropdown.AddItem(WorkshopText.EntryLabel, -1);
		dropdown.SetItemMetadata(itemCount, Variant.op_Implicit("__workshop__"));
		for (int i = 0; i < dropdown.ItemCount; i++)
		{
			Variant itemMetadata = dropdown.GetItemMetadata(i);
			if (((Variant)(ref itemMetadata)).AsString() == "__base__")
			{
				dropdown.SetItemText(i, ModLocalization.Get(ModText.GameDefault));
			}
		}
		if (!characterPopup)
		{
			PresetChoiceColoring.Attach(dropdown, SkinOptionStylePolicy.IsAccented, colorSelection: false);
			RefreshSelectionColor(dropdown);
		}
	}

	internal static void RefreshSelectionColor(OptionButton dropdown)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		if (!((GodotObject)dropdown).HasMeta(StringName.op_Implicit("sc_skin_choice_color")))
		{
			((GodotObject)dropdown).SetMeta(StringName.op_Implicit("sc_skin_choice_color"), Variant.op_Implicit(true));
			dropdown.ItemSelected += (ItemSelectedEventHandler)delegate
			{
				RefreshSelectionColor(dropdown);
			};
		}
		ModThemeRuntime.Bind((Node)(object)dropdown, "skin_choice_color", delegate(ModThemeSettings theme)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			int num;
			if (dropdown.Selected >= 0 && dropdown.Selected < dropdown.ItemCount)
			{
				Variant itemMetadata = dropdown.GetItemMetadata(dropdown.Selected);
				num = (SkinOptionStylePolicy.IsAccented(((Variant)(ref itemMetadata)).AsString()) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool flag = (byte)num != 0;
			Color val = default(Color);
			((Color)(ref val))._002Ector(flag ? theme.AccentColor : theme.TextColor);
			string[] array = new string[5] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" };
			foreach (string text in array)
			{
				((Control)dropdown).AddThemeColorOverride(StringName.op_Implicit(text), val);
			}
		});
	}

	internal static Button CreatePriorityButton(Node origin, string kind, string target, Action refresh, string region = "")
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		Button val = new Button
		{
			Text = WorkshopText.EntryLabel,
			CustomMinimumSize = new Vector2(180f, 42f),
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2
		};
		ContextualSkinControls.ApplyGameTheme(val);
		ModThemeRuntime.AccentText((Control)val);
		((BaseButton)val).Pressed += delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			Callable val2 = Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)origin) && origin.IsInsideTree())
				{
					SkinWorkshopPanel.Show(origin, kind, target, refresh, region);
				}
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		};
		return val;
	}

	internal static bool Open(string optionId, Node origin, string groupId, Action refresh, bool cards = false, string? kind = null)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (WorkshopCatalogPolicy.IsSkinChoice(optionId))
		{
			return false;
		}
		refresh();
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)origin) && origin.IsInsideTree())
			{
				SkinWorkshopPanel.Show(origin, kind ?? (cards ? "cards" : (SkinService.Catalog?.WorkshopKind(groupId) ?? "character")), groupId, refresh);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		return true;
	}
}

using System;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CharacterSkinRandomExclusionUi
{
	private sealed record Binding(ItemList List, Func<string?> Group)
	{
		public int Hovered { get; set; } = -1;
	}

	private static readonly ConditionalWeakTable<OptionButton, Binding> Bindings = new ConditionalWeakTable<OptionButton, Binding>();

	internal static void Attach(OptionButton picker, ItemList list, Func<string?> group)
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		if (Bindings.TryGetValue(picker, out Binding _))
		{
			Refresh(picker);
			return;
		}
		Binding binding = new Binding(list, group);
		Bindings.Add(picker, binding);
		((Control)list).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Invalid comparison between Unknown and I8
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			InputEventMouseMotion val = (InputEventMouseMotion)(object)((input is InputEventMouseMotion) ? input : null);
			if (val != null)
			{
				int itemAtPosition = list.GetItemAtPosition(((InputEventMouse)val).Position, true);
				if (binding.Hovered != itemAtPosition)
				{
					binding.Hovered = itemAtPosition;
					RefreshInteractionColors(picker, binding);
				}
			}
			InputEventMouseButton val2 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val2 != null && (long)val2.ButtonIndex == 2 && val2.Pressed)
			{
				int itemAtPosition2 = list.GetItemAtPosition(((InputEventMouse)val2).Position, true);
				string text = group();
				if (text != null && itemAtPosition2 >= 0 && itemAtPosition2 < picker.ItemCount && !picker.IsItemDisabled(itemAtPosition2))
				{
					Variant itemMetadata = picker.GetItemMetadata(itemAtPosition2);
					string text2 = ((Variant)(ref itemMetadata)).AsString();
					if (RandomCharacterSkinPolicy.IsCandidate(text2))
					{
						((Control)list).AcceptEvent();
						if (SkinService.ToggleCharacterSkinRandomExclusion(text, text2))
						{
							Refresh(picker);
						}
					}
				}
			}
		};
		((Control)list).MouseExited += delegate
		{
			binding.Hovered = -1;
			RefreshInteractionColors(picker, binding);
		};
		list.ItemSelected += (ItemSelectedEventHandler)delegate
		{
			RefreshInteractionColors(picker, binding);
		};
		((Window)picker.GetPopup()).AboutToPopup += delegate
		{
			Refresh(picker);
		};
		ModThemeRuntime.Bind((Node)(object)list, "random_exclusions", delegate
		{
			Refresh(picker);
		});
	}

	internal static Color ChoiceColor(Color normal, bool excluded)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		if (!excluded)
		{
			return normal;
		}
		return new Color(normal, normal.A * 0.5f);
	}

	internal static void ApplyInteractionColors(string? group, string? hoveredOption, string? selectedOption, Color normal, Color accent, Action<string, Color> apply)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Color arg = For(hoveredOption);
		apply("font_hovered_color", arg);
		apply("font_selected_color", For(selectedOption));
		apply("font_hovered_selected_color", arg);
		Color For(string? option)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			return ChoiceColor((option != null && SkinOptionStylePolicy.IsAccented(option)) ? accent : normal, group != null && option != null && SkinService.IsCharacterSkinExcludedFromRandom(group, option));
		}
	}

	private static void RefreshInteractionColors(OptionButton picker, Binding binding)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		ItemList list = binding.List;
		int[] selectedItems = list.GetSelectedItems();
		string hovered = Option(binding.Hovered);
		string current = Option((selectedItems.Length == 0) ? (-1) : selectedItems[0]);
		ApplyInteractionColors(binding.Group(), hovered, current, ModThemeRuntime.Text, ModThemeRuntime.Accent, delegate(string name, Color color)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			((Control)list).AddThemeColorOverride(StringName.op_Implicit(name), SkinChoiceDimming.InteractionRow(name, color, picker, hovered, current));
		});
		string? Option(int index)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (index < 0 || index >= Math.Min(picker.ItemCount, list.ItemCount))
			{
				return null;
			}
			Variant itemMetadata = picker.GetItemMetadata(index);
			return ((Variant)(ref itemMetadata)).AsString();
		}
	}

	internal static void Refresh(OptionButton picker)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (Bindings.TryGetValue(picker, out Binding value))
		{
			string text = value.Group();
			ItemList list = value.List;
			for (int i = 0; i < Math.Min(list.ItemCount, picker.ItemCount); i++)
			{
				Variant itemMetadata = picker.GetItemMetadata(i);
				string optionId = ((Variant)(ref itemMetadata)).AsString();
				Color normal = (SkinOptionStylePolicy.IsAccented(optionId) ? ModThemeRuntime.Accent : ModThemeRuntime.Text);
				list.SetItemCustomFgColor(i, SkinChoiceDimming.Row(ChoiceColor(normal, text != null && SkinService.IsCharacterSkinExcludedFromRandom(text, optionId)), picker, optionId));
			}
			RefreshInteractionColors(picker, value);
		}
	}
}

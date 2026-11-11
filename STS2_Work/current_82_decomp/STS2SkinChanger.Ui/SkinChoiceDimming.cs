using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class SkinChoiceDimming
{
	private static readonly Func<string, bool> Never = (string _) => false;

	private static readonly ConditionalWeakTable<OptionButton, Func<string, bool>> Bindings = new ConditionalWeakTable<OptionButton, Func<string, bool>>();

	internal static void Bind(OptionButton picker, IReadOnlyList<SkinPriorityState>? states)
	{
		Func<string, bool> value = ((states == null) ? Never : SkinPriorityDisplayPolicy.UncheckedLookup(states));
		if (Bindings.TryGetValue(picker, out Func<string, bool> _))
		{
			Bindings.Remove(picker);
		}
		Bindings.Add(picker, value);
	}

	internal static bool IsUnchecked(OptionButton? picker, string? optionId)
	{
		if (picker != null && optionId != null && Bindings.TryGetValue(picker, out Func<string, bool> value))
		{
			return value(optionId);
		}
		return false;
	}

	internal static Color Row(Color color, OptionButton? picker, string? optionId)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!IsUnchecked(picker, optionId))
		{
			return color;
		}
		return new Color(color, color.A * 0.55f);
	}

	internal static Color InteractionRow(string stateKey, Color color, OptionButton picker, string? hoveredOption, string? selectedOption)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return Row(color, picker, (stateKey == "font_selected_color") ? selectedOption : hoveredOption);
	}
}

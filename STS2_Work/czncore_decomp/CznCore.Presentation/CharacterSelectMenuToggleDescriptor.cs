using System;
using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed record CharacterSelectMenuToggleDescriptor : CharacterSelectMenuItemDescriptor
{
	public Func<bool> Value { get; init; }

	public Action<bool> SetValue { get; init; }

	public CharacterSelectMenuToggleDescriptor(string Id, string IconPath, Func<string> Title, Func<string> Tooltip, Func<bool> Value, Action<bool> SetValue)
	{
		this.Value = Value;
		this.SetValue = SetValue;
		base._002Ector(Id, IconPath, Title, Tooltip);
	}

	internal override CharacterSelectMenuItemView CreateView(Action changed)
	{
		return new CharacterSelectMenuToggleView(this, changed);
	}

	[CompilerGenerated]
	public void Deconstruct(out string Id, out string IconPath, out Func<string> Title, out Func<string> Tooltip, out Func<bool> Value, out Action<bool> SetValue)
	{
		Id = base.Id;
		IconPath = base.IconPath;
		Title = base.Title;
		Tooltip = base.Tooltip;
		Value = this.Value;
		SetValue = this.SetValue;
	}
}

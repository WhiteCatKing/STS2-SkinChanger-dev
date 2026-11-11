using System;
using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed record CharacterSelectMenuActionDescriptor : CharacterSelectMenuItemDescriptor
{
	public Func<string> State { get; init; }

	public Action Activate { get; init; }

	public CharacterSelectMenuActionDescriptor(string Id, string IconPath, Func<string> Title, Func<string> State, Func<string> Tooltip, Action Activate)
	{
		this.State = State;
		this.Activate = Activate;
		base._002Ector(Id, IconPath, Title, Tooltip);
	}

	internal override CharacterSelectMenuItemView CreateView(Action changed)
	{
		return new CharacterSelectMenuActionView(this, changed);
	}

	[CompilerGenerated]
	public void Deconstruct(out string Id, out string IconPath, out Func<string> Title, out Func<string> State, out Func<string> Tooltip, out Action Activate)
	{
		Id = base.Id;
		IconPath = base.IconPath;
		Title = base.Title;
		State = this.State;
		Tooltip = base.Tooltip;
		Activate = this.Activate;
	}
}

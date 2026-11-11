using System;
using System.Runtime.CompilerServices;

namespace CznCore.Settings;

internal sealed record SettingsUiExtensionToggle : SettingsUiExtensionControl
{
	public Func<bool> Read { get; init; }

	public Action<bool> Write { get; init; }

	public SettingsUiExtensionToggle(string Id, int Order, Func<string> Label, Func<string> Description, Func<bool> Read, Action<bool> Write, Func<bool> Enabled)
	{
		this.Read = Read;
		this.Write = Write;
		base._002Ector(Id, Order, Label, Description, Enabled);
	}

	[CompilerGenerated]
	public void Deconstruct(out string Id, out int Order, out Func<string> Label, out Func<string> Description, out Func<bool> Read, out Action<bool> Write, out Func<bool> Enabled)
	{
		Id = base.Id;
		Order = base.Order;
		Label = base.Label;
		Description = base.Description;
		Read = this.Read;
		Write = this.Write;
		Enabled = base.Enabled;
	}
}

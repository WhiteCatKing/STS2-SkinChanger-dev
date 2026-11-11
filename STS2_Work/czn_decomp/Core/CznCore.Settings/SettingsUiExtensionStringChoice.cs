using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CznCore.Settings;

internal sealed record SettingsUiExtensionStringChoice : SettingsUiExtensionControl
{
	public Func<string> Read { get; init; }

	public Action<string> Write { get; init; }

	public IReadOnlyList<SettingsUiExtensionStringChoiceOption> Options { get; init; }

	public SettingsUiExtensionStringChoice(string Id, int Order, Func<string> Label, Func<string> Description, Func<string> Read, Action<string> Write, Func<bool> Enabled, IReadOnlyList<SettingsUiExtensionStringChoiceOption> Options)
	{
		this.Read = Read;
		this.Write = Write;
		this.Options = Options;
		base._002Ector(Id, Order, Label, Description, Enabled);
	}

	[CompilerGenerated]
	public void Deconstruct(out string Id, out int Order, out Func<string> Label, out Func<string> Description, out Func<string> Read, out Action<string> Write, out Func<bool> Enabled, out IReadOnlyList<SettingsUiExtensionStringChoiceOption> Options)
	{
		Id = base.Id;
		Order = base.Order;
		Label = base.Label;
		Description = base.Description;
		Read = this.Read;
		Write = this.Write;
		Enabled = base.Enabled;
		Options = this.Options;
	}
}

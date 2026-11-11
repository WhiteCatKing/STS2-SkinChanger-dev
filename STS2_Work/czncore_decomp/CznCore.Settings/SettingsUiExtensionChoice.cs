using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CznCore.Settings;

internal sealed record SettingsUiExtensionChoice : SettingsUiExtensionControl
{
	public Func<int> Read { get; init; }

	public Action<int> Write { get; init; }

	public IReadOnlyList<SettingsUiExtensionChoiceOption> Options { get; init; }

	public SettingsUiExtensionChoice(string Id, int Order, Func<string> Label, Func<string> Description, Func<int> Read, Action<int> Write, Func<bool> Enabled, IReadOnlyList<SettingsUiExtensionChoiceOption> Options)
	{
		this.Read = Read;
		this.Write = Write;
		this.Options = Options;
		base._002Ector(Id, Order, Label, Description, Enabled);
	}

	[CompilerGenerated]
	public void Deconstruct(out string Id, out int Order, out Func<string> Label, out Func<string> Description, out Func<int> Read, out Action<int> Write, out Func<bool> Enabled, out IReadOnlyList<SettingsUiExtensionChoiceOption> Options)
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

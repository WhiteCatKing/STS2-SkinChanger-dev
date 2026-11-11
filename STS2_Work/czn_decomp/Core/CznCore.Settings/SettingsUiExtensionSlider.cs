using System;
using System.Runtime.CompilerServices;

namespace CznCore.Settings;

internal sealed record SettingsUiExtensionSlider : SettingsUiExtensionControl
{
	public Func<double> Read { get; init; }

	public Action<double> Write { get; init; }

	public double Min { get; init; }

	public double Max { get; init; }

	public double Step { get; init; }

	public Func<string> Suffix { get; init; }

	public double DisplayOffset { get; init; }

	public SettingsUiExtensionSlider(string Id, int Order, Func<string> Label, Func<string> Description, Func<double> Read, Action<double> Write, Func<bool> Enabled, double Min, double Max, double Step, Func<string> Suffix, double DisplayOffset)
	{
		this.Read = Read;
		this.Write = Write;
		this.Min = Min;
		this.Max = Max;
		this.Step = Step;
		this.Suffix = Suffix;
		this.DisplayOffset = DisplayOffset;
		base._002Ector(Id, Order, Label, Description, Enabled);
	}

	[CompilerGenerated]
	public void Deconstruct(out string Id, out int Order, out Func<string> Label, out Func<string> Description, out Func<double> Read, out Action<double> Write, out Func<bool> Enabled, out double Min, out double Max, out double Step, out Func<string> Suffix, out double DisplayOffset)
	{
		Id = base.Id;
		Order = base.Order;
		Label = base.Label;
		Description = base.Description;
		Read = this.Read;
		Write = this.Write;
		Enabled = base.Enabled;
		Min = this.Min;
		Max = this.Max;
		Step = this.Step;
		Suffix = this.Suffix;
		DisplayOffset = this.DisplayOffset;
	}
}

using System;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal abstract record CharacterSelectMenuItemDescriptor(string Id, string IconPath, Func<string> Title, Func<string> Tooltip)
{
	public Color Accent { get; init; } = CharacterSelectMenuStyle.Muted;

	internal abstract CharacterSelectMenuItemView CreateView(Action changed);

	[CompilerGenerated]
	protected virtual bool PrintMembers(StringBuilder builder)
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Id = ");
		builder.Append((object?)Id);
		builder.Append(", IconPath = ");
		builder.Append((object?)IconPath);
		builder.Append(", Title = ");
		builder.Append(Title);
		builder.Append(", Tooltip = ");
		builder.Append(Tooltip);
		builder.Append(", Accent = ");
		Color accent = Accent;
		builder.Append(((object)(Color)(ref accent)/*cast due to .constrained prefix*/).ToString());
		return true;
	}
}

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record CharacterSelectMenuSectionDescriptor(string Id, Func<string> Title, IReadOnlyList<CharacterSelectMenuItemDescriptor> Items)
{
	public Func<Action, Control>? CreateExtraContent { get; init; }

	public string? ExternalIconPath { get; init; }

	public Color ExternalAccent { get; init; } = new Color("8860ad");

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Id = ");
		builder.Append((object?)Id);
		builder.Append(", Title = ");
		builder.Append(Title);
		builder.Append(", Items = ");
		builder.Append(Items);
		builder.Append(", CreateExtraContent = ");
		builder.Append(CreateExtraContent);
		builder.Append(", ExternalIconPath = ");
		builder.Append((object?)ExternalIconPath);
		builder.Append(", ExternalAccent = ");
		Color externalAccent = ExternalAccent;
		builder.Append(((object)(Color)(ref externalAccent)/*cast due to .constrained prefix*/).ToString());
		return true;
	}
}

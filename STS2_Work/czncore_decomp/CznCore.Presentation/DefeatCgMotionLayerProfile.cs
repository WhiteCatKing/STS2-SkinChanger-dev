using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record DefeatCgMotionLayerProfile(string Name, int ZIndex, Color Tint, bool Additive, IReadOnlyList<DefeatCgMotionKeyProfile> Keyframes)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("Name = ");
		builder.Append((object?)Name);
		builder.Append(", ZIndex = ");
		builder.Append(ZIndex.ToString());
		builder.Append(", Tint = ");
		Color tint = Tint;
		builder.Append(((object)(Color)(ref tint)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", Additive = ");
		builder.Append(Additive.ToString());
		builder.Append(", Keyframes = ");
		builder.Append(Keyframes);
		return true;
	}
}

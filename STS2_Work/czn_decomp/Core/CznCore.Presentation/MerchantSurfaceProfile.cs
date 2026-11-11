using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record MerchantSurfaceProfile(string LoopAnimationName, Vector2 SpineScale)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("LoopAnimationName = ");
		builder.Append((object?)LoopAnimationName);
		builder.Append(", SpineScale = ");
		Vector2 spineScale = SpineScale;
		builder.Append(((object)(Vector2)(ref spineScale)/*cast due to .constrained prefix*/).ToString());
		return true;
	}
}

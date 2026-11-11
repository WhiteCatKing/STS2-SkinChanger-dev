using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record FakeMerchantPresentationResult(int AppliedCount, string AnimationName, Vector2 SpineScale)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("AppliedCount = ");
		builder.Append(AppliedCount.ToString());
		builder.Append(", AnimationName = ");
		builder.Append((object?)AnimationName);
		builder.Append(", SpineScale = ");
		Vector2 spineScale = SpineScale;
		builder.Append(((object)(Vector2)(ref spineScale)/*cast due to .constrained prefix*/).ToString());
		return true;
	}
}

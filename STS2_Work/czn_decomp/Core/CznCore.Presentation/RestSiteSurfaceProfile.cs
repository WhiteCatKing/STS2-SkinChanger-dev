using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record RestSiteSurfaceProfile(bool ApplySceneLayout, Vector2 SpinePosition, Vector2 SpineScale, RestSiteControlRectProfile? ControlRoot, RestSiteControlRectProfile? SelectionReticle, RestSiteControlRectProfile? Hitbox, RestSiteControlRectProfile? ThoughtBubbleRight, RestSiteControlRectProfile? ThoughtBubbleLeft)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("ApplySceneLayout = ");
		builder.Append(ApplySceneLayout.ToString());
		builder.Append(", SpinePosition = ");
		Vector2 val = SpinePosition;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", SpineScale = ");
		val = SpineScale;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", ControlRoot = ");
		builder.Append(ControlRoot);
		builder.Append(", SelectionReticle = ");
		builder.Append(SelectionReticle);
		builder.Append(", Hitbox = ");
		builder.Append(Hitbox);
		builder.Append(", ThoughtBubbleRight = ");
		builder.Append(ThoughtBubbleRight);
		builder.Append(", ThoughtBubbleLeft = ");
		builder.Append(ThoughtBubbleLeft);
		return true;
	}
}

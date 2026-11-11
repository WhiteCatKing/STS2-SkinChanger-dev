using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record BattleReadyOverlayCoordinateCalibration(Vector2 LocalOriginCorrection, float ScaleMultiplier = 1f)
{
	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("LocalOriginCorrection = ");
		Vector2 localOriginCorrection = LocalOriginCorrection;
		builder.Append(((object)(Vector2)(ref localOriginCorrection)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", ScaleMultiplier = ");
		builder.Append(ScaleMultiplier.ToString());
		return true;
	}
}

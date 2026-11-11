using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record BattleReadyOverlayOfficialEffectPlacement(Vector2 StandbyPosition, Vector2 Offset, float Scale, Vector2 PrimitiveOffset, float PrimitiveOffsetScale = 0f, float VisualScaleMultiplier = 1f)
{
	public BattleReadyOverlayIdleEffectPlan ToScene(string scenePath, BattleReadyOverlayCoordinateCalibration calibration)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = default(Vector2);
		((Vector2)(ref val))._002Ector(0f - PrimitiveOffset.X, PrimitiveOffset.Y);
		Vector2 val2 = Offset + StandbyPosition + calibration.LocalOriginCorrection + val * PrimitiveOffsetScale;
		return BattleReadyOverlayIdleEffectPlan.Scene(scenePath, val2.X, val2.Y, Scale * VisualScaleMultiplier * calibration.ScaleMultiplier);
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("StandbyPosition = ");
		Vector2 val = StandbyPosition;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", Offset = ");
		val = Offset;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", Scale = ");
		builder.Append(Scale.ToString());
		builder.Append(", PrimitiveOffset = ");
		val = PrimitiveOffset;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", PrimitiveOffsetScale = ");
		builder.Append(PrimitiveOffsetScale.ToString());
		builder.Append(", VisualScaleMultiplier = ");
		builder.Append(VisualScaleMultiplier.ToString());
		return true;
	}
}

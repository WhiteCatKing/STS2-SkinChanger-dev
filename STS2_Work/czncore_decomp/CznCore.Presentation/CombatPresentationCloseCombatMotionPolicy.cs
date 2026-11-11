using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed record CombatPresentationCloseCombatMotionPolicy(bool SnapApproach, float ApproachSnapDelaySeconds, bool SnapReturn, float ReturnSnapDelaySeconds, bool LiftOwnerLayer, int OwnerLayerZIndex, int StepEffectLayerZIndex)
{
	public static CombatPresentationCloseCombatMotionPolicy SharedStepEffectTeleport { get; } = new CombatPresentationCloseCombatMotionPolicy(SnapApproach: true, 0.04f, SnapReturn: true, 0.06f, LiftOwnerLayer: true, -6, -3);

	[CompilerGenerated]
	private CombatPresentationCloseCombatMotionPolicy(CombatPresentationCloseCombatMotionPolicy original)
	{
		SnapApproach = original.SnapApproach;
		ApproachSnapDelaySeconds = original.ApproachSnapDelaySeconds;
		SnapReturn = original.SnapReturn;
		ReturnSnapDelaySeconds = original.ReturnSnapDelaySeconds;
		LiftOwnerLayer = original.LiftOwnerLayer;
		OwnerLayerZIndex = original.OwnerLayerZIndex;
		StepEffectLayerZIndex = original.StepEffectLayerZIndex;
	}
}

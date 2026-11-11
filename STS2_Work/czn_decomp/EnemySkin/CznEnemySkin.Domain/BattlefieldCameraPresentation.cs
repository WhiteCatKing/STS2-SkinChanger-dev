using System.Runtime.CompilerServices;

namespace CznEnemySkin.Domain;

internal sealed record BattlefieldCameraPresentation(PresentationMode Mode, float Scale, float OffsetX, float OffsetY)
{
	public static BattlefieldCameraPresentation Inherit { get; } = new BattlefieldCameraPresentation(PresentationMode.Inherit, 1f, 0f, 0f);

	[CompilerGenerated]
	private BattlefieldCameraPresentation(BattlefieldCameraPresentation original)
	{
		Mode = original.Mode;
		Scale = original.Scale;
		OffsetX = original.OffsetX;
		OffsetY = original.OffsetY;
	}
}

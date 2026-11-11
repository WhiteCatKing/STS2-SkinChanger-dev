using System.Runtime.CompilerServices;

namespace CznEnemySkin.Domain;

internal sealed record BattlefieldBgmPresentation(PresentationMode Mode, string? TrackId)
{
	public static BattlefieldBgmPresentation Inherit { get; } = new BattlefieldBgmPresentation(PresentationMode.Inherit, null);

	[CompilerGenerated]
	private BattlefieldBgmPresentation(BattlefieldBgmPresentation original)
	{
		Mode = original.Mode;
		TrackId = original.TrackId;
	}
}

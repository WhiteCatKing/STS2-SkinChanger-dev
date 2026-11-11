using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal readonly record struct RunFinishedPresentationEvent(Player Player)
{
	public bool IsFailure { get; init; } = true;
}

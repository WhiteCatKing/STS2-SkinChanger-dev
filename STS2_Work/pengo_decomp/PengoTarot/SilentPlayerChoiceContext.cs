using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace PengoTarot;

internal sealed class SilentPlayerChoiceContext : PlayerChoiceContext
{
	public override ulong? OwnerId => null;

	public override Task SignalPlayerChoiceBegun(Player chooser, PlayerChoiceOptions options)
	{
		return Task.CompletedTask;
	}

	public override Task SignalPlayerChoiceEnded()
	{
		return Task.CompletedTask;
	}
}

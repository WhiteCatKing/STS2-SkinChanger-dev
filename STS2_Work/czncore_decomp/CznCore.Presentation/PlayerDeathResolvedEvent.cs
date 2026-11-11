using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Players;

namespace CznCore.Presentation;

internal readonly record struct PlayerDeathResolvedEvent
{
	public Player Player { get; init; }

	public bool IsGameOver { get; init; }

	public float DeathAnimationLength { get; init; }

	public bool IsAbandoned { get; init; }

	public PlayerDeathResolvedEvent(Player Player, bool IsGameOver, float DeathAnimationLength)
	{
		IsAbandoned = false;
		this.Player = Player;
		this.IsGameOver = IsGameOver;
		this.DeathAnimationLength = DeathAnimationLength;
	}

	[CompilerGenerated]
	public void Deconstruct(out Player Player, out bool IsGameOver, out float DeathAnimationLength)
	{
		Player = this.Player;
		IsGameOver = this.IsGameOver;
		DeathAnimationLength = this.DeathAnimationLength;
	}
}

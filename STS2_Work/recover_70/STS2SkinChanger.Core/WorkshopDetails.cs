namespace STS2SkinChanger.Core;

internal sealed record WorkshopDetails(string Title, string PreviewUrl)
{
	public ulong? Subscriptions { get; init; }

	public ulong? LifetimeSubscriptions { get; init; }

	public ulong? Favorites { get; init; }

	public ulong? Comments { get; init; }

	public float? Score { get; init; }

	public uint? VotesUp { get; init; }

	public uint? VotesDown { get; init; }

	public uint? Created { get; init; }

	public uint? Updated { get; init; }
}

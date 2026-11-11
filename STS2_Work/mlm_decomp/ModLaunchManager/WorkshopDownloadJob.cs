namespace ModLaunchManager;

internal sealed class WorkshopDownloadJob
{
	internal required ulong ItemId { get; init; }

	internal required string DisplayName { get; init; }

	internal WorkshopDownloadState State { get; set; }

	internal float Progress { get; set; }

	internal double Elapsed { get; set; }

	internal double IdleElapsed { get; set; }

	internal ulong DownloadedBytes { get; set; }

	internal double RetryElapsed { get; set; }

	internal WorkshopDownloadFailure? Failure { get; set; }
}

namespace ModLaunchManager;

internal sealed class WorkshopDownloadRecord
{
	internal required ulong ItemId { get; init; }

	internal required string Name { get; set; }

	internal required WorkshopDownloadSource Source { get; set; }

	internal string? Origin { get; set; }

	internal WorkshopDownloadState State { get; set; }

	internal float Progress { get; set; }

	internal ulong DownloadedBytes { get; set; }

	internal ulong TotalBytes { get; set; }

	internal double IdleElapsed { get; set; }

	internal long Order { get; set; }
}

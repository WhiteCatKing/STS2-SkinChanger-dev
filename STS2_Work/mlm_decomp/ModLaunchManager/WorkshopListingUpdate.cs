namespace ModLaunchManager;

internal static class WorkshopListingUpdate
{
	internal const double DownloadStartGraceSeconds = 30.0;

	internal static bool ShowsNewerFiles(uint localUpdated, ulong localSize, uint listingUpdated, ulong listingSize, bool sharedAcrossBranches)
	{
		if (sharedAcrossBranches && localUpdated != 0 && localSize != 0L && listingSize != 0L && listingUpdated > localUpdated)
		{
			return listingSize != localSize;
		}
		return false;
	}
}

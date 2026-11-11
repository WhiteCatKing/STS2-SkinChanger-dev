namespace ModLaunchManager;

internal enum LocalModImportProblem
{
	NoMods,
	UnsupportedArchive,
	ArchiveBroken,
	TooLarge,
	InsideModsFolder,
	ManifestBroken,
	MissingFile,
	NothingToLoad,
	DuplicateInSource,
	AlreadyInstalled,
	InstalledFromWorkshop,
	InstalledInUse,
	CopyFailed
}

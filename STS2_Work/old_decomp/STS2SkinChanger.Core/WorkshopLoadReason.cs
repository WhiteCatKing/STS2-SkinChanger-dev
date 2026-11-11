namespace STS2SkinChanger.Core;

internal enum WorkshopLoadReason
{
	None,
	StartupCode,
	Dependency,
	Version,
	VersionRule,
	IncompleteResources,
	ChangedFiles,
	DuplicateId,
	InvalidPackage,
	Gameplay
}

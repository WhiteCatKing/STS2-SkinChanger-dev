namespace STS2SkinChanger.Core;

internal enum WorkshopCodeError
{
	Format,
	MissingName,
	NameMismatch,
	MissingParts,
	Conflict,
	Checksum,
	Version,
	TooLarge,
	InvalidData,
	WrongGame,
	Unavailable,
	Legacy
}

namespace ModLaunchManager;

internal enum AiTranslationStage
{
	ReadSource,
	ReadReferences,
	PrepareEntries,
	ProtectFormat,
	SendRequest,
	ReadResponse,
	ParseResponse,
	RestoreFormat,
	ValidateGameFormat,
	CheckSource,
	Save,
	Complete,
	SaveProgress,
	WaitToRetry
}

namespace ModLaunchManager;

internal enum TranslationFormatIssue
{
	None,
	SourceBraces,
	MissingToken,
	DuplicateToken,
	UnknownToken,
	AddedSyntax,
	LineBreaks,
	GameFormat,
	AddedNumber,
	ChoiceBlock,
	Untranslated,
	Highlight,
	ForeignWord,
	MeasureWord
}

using System;
using System.Collections.Generic;
using System.IO;

namespace ModLaunchManager;

internal sealed class TranslationFileException : IOException
{
	internal string Reason { get; }

	internal TranslationFormatIssue FormatIssue { get; }

	internal AiTranslationRetryKind RetryKind { get; init; }

	internal TimeSpan? RetryAfter { get; init; }

	internal string? ReplyIssue { get; init; }

	internal string? Fallback { get; init; }

	internal string? Fix { get; init; }

	internal IReadOnlyList<string> MissingKinds { get; init; }

	public TranslationFileException(string reason, string? detail = null, TranslationFormatIssue formatIssue = TranslationFormatIssue.None)
	{
		Reason = reason;
		FormatIssue = formatIssue;
		MissingKinds = Array.Empty<string>();
		base._002Ector(detail ?? reason);
	}
}

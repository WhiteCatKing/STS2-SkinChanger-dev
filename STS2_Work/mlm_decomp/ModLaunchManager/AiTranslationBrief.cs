using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ModLaunchManager;

internal sealed record AiTranslationBrief(string Language, string Title, string Description, IReadOnlyList<string> Excerpts, IReadOnlyList<TranslationReference> References, IReadOnlyList<TranslationReference>? Examples = null, string SourceWork = "")
{
	internal string Json(string notes)
	{
		return JsonSerializer.Serialize(new
		{
			targetLanguage = Language,
			translatorNotes = notes,
			title = Title,
			description = Description,
			sourceWork = SourceWork,
			storyExcerpts = Excerpts,
			referencePairs = References.Select((TranslationReference r) => new
			{
				source = r.Source,
				target = r.Target,
				from = r.Kind
			}),
			styleExamples = (Examples ?? Array.Empty<TranslationReference>()).Select((TranslationReference r) => new
			{
				source = r.Source,
				target = r.Target
			})
		}, AiTranslationClient.PayloadJson);
	}
}

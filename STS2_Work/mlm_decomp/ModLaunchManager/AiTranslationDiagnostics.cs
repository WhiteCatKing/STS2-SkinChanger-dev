using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace ModLaunchManager;

internal sealed class AiTranslationDiagnostics
{
	private int _batch;

	private long _inputTokens;

	private long _cachedInputTokens;

	private long _outputTokens;

	private long _ignoredKeys;

	private long _keptStyleMisses;

	private int _thinkingSwitchRejected;

	private string _finish = "none";

	internal AiTranslationStage Stage { get; set; }

	internal int Batch => _batch;

	internal int Entry { get; set; } = -1;

	internal int? HttpStatus { get; set; }

	internal int CompletedEntries { get; set; }

	internal int TotalEntries { get; set; }

	internal long KeptStyleMisses => Interlocked.Read(in _keptStyleMisses);

	internal long IgnoredKeys => Interlocked.Read(in _ignoredKeys);

	internal long InputTokens => Interlocked.Read(in _inputTokens);

	internal long CachedInputTokens => Interlocked.Read(in _cachedInputTokens);

	internal long OutputTokens => Interlocked.Read(in _outputTokens);

	internal int CanonNames { get; set; }

	internal bool ThinkingSwitchRejected => Volatile.Read(in _thinkingSwitchRejected) != 0;

	internal void KeepStyleMisses(int count)
	{
		Interlocked.Add(ref _keptStyleMisses, count);
	}

	internal void IgnoreKeys(int count)
	{
		Interlocked.Add(ref _ignoredKeys, count);
	}

	internal void RejectThinkingSwitch()
	{
		Volatile.Write(ref _thinkingSwitchRejected, 1);
	}

	internal void BeginBatch()
	{
		Interlocked.Increment(ref _batch);
		Entry = -1;
		HttpStatus = null;
		_finish = "none";
		Stage = AiTranslationStage.ProtectFormat;
	}

	internal void AddUsage(long input, long cached, long output)
	{
		Interlocked.Add(ref _inputTokens, input);
		Interlocked.Add(ref _cachedInputTokens, Math.Min(cached, input));
		Interlocked.Add(ref _outputTokens, output);
	}

	internal void Add(AiTranslationDiagnostics batch)
	{
		Interlocked.Add(ref _batch, batch.Batch);
		AddUsage(batch.InputTokens, batch.CachedInputTokens, batch.OutputTokens);
		IgnoreKeys((int)batch.IgnoredKeys);
		KeepStyleMisses((int)batch.KeptStyleMisses);
		if (batch.ThinkingSwitchRejected)
		{
			RejectThinkingSwitch();
		}
	}

	internal void AdoptFailure(AiTranslationDiagnostics batch)
	{
		Stage = batch.Stage;
		Entry = batch.Entry;
		HttpStatus = batch.HttpStatus;
		_finish = batch._finish;
	}

	internal static string Skipped(IEnumerable<TranslationFileException> errors)
	{
		return string.Join(", ", from e in errors
			group e by e.FormatIssue into g
			orderby g.Key
			select $"{g.Key}={g.Count()}");
	}

	internal static string Issues(IEnumerable<TranslationFileException> errors)
	{
		List<TranslationFileException> source = errors.ToList();
		return string.Join(", ", Counts(source.Select((TranslationFileException e) => (e.FormatIssue == TranslationFormatIssue.None) ? ((e.ReplyIssue == null) ? e.Reason : ("reply:" + e.ReplyIssue)) : e.FormatIssue.ToString())).Concat(Counts(from kind in source.SelectMany((TranslationFileException e) => e.MissingKinds)
			select "missing:" + kind.Replace(",", "").Replace(' ', '-'))));
		static IEnumerable<string> Counts(IEnumerable<string> labels)
		{
			return from g in labels.GroupBy<string, string>((string label) => label, StringComparer.Ordinal).OrderBy<IGrouping<string, string>, string>((IGrouping<string, string> g) => g.Key, StringComparer.Ordinal)
				select $"{g.Key}={g.Count()}";
		}
	}

	internal string Usage()
	{
		return $"requests={Batch}, inputTokens={InputTokens}, cachedInputTokens={CachedInputTokens}, outputTokens={OutputTokens}" + ((IgnoredKeys > 0) ? $", ignoredReplyKeys={IgnoredKeys}" : "") + ((KeptStyleMisses > 0) ? $", keptStyleMisses={KeptStyleMisses}" : "") + (ThinkingSwitchRejected ? ", thinkingSwitch=rejected" : "") + ((CanonNames > 0) ? $", canonNames={CanonNames}" : ((CanonNames < 0) ? ", canonNames=failed" : ""));
	}

	internal void SetFinish(string? finish)
	{
		string finish2;
		switch (finish)
		{
		case "stop":
		case "length":
		case "content_filter":
		case "tool_calls":
		case "function_call":
			finish2 = finish;
			break;
		default:
			finish2 = "unknown";
			break;
		}
		_finish = finish2;
	}

	internal static string Reason(Exception error)
	{
		if (!(error is TranslationFileException ex))
		{
			if (error is OperationCanceledException)
			{
				return "cancelled";
			}
			return "io";
		}
		string result;
		switch (ex.Reason)
		{
		case "invalidJson":
		case "invalidPath":
		case "invalidPack":
		case "unsupported":
		case "tooLarge":
		case "endpoint":
		case "settings":
		case "response":
		case "timeout":
		case "service":
		case "network":
		case "noFiles":
		case "changed":
		case "auth":
		case "rate":
		case "model":
		case "quota":
		case "invalidFormat":
			result = ex.Reason;
			break;
		default:
			result = "io";
			break;
		}
		return result;
	}

	internal string Failure(Exception error)
	{
		TranslationFormatIssue value = ((error is TranslationFileException ex) ? ex.FormatIssue : TranslationFormatIssue.None);
		return $"AI translation failed: stage={Stage}, batch={Batch}, entry={Entry}, http={HttpStatus?.ToString(CultureInfo.InvariantCulture) ?? "none"}, finish={_finish}, reason={Reason(error)}, format={value}, reply={(error as TranslationFileException)?.ReplyIssue ?? "none"}, completed={CompletedEntries}/{TotalEntries}.";
	}

	internal string FeedbackKey(Exception error)
	{
		if (Reason(error) != "invalidFormat")
		{
			return "translations.error." + Reason(error);
		}
		switch ((error is TranslationFileException ex) ? ex.FormatIssue : TranslationFormatIssue.None)
		{
		case TranslationFormatIssue.SourceBraces:
			return "ai.error.sourceFormat";
		case TranslationFormatIssue.LineBreaks:
			return "ai.error.lineBreaks";
		case TranslationFormatIssue.MissingToken:
		case TranslationFormatIssue.DuplicateToken:
		case TranslationFormatIssue.UnknownToken:
		case TranslationFormatIssue.ChoiceBlock:
			return "ai.error.tokens";
		default:
			return "ai.error.format";
		}
	}
}

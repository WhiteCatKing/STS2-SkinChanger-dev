using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace ModLaunchManager;

internal static class AiModTranslator
{
	private const int MaxBatchEntries = 24;

	private const int MaxBatchCharacters = 6000;

	private const int MaxGlossary = 60;

	private const int CanonPart = 80;

	private const int ModRank = 0;

	private const int TermRank = 1;

	private const int NameRank = 2;

	private const int BodyRank = 3;

	internal static async Task<AiTranslationDocument> GenerateAsync(AiTranslationClient client, AiTranslationSettings settings, byte[] key, string language, string name, string description, IReadOnlyDictionary<string, byte[]> original, IReadOnlyList<TranslationReference> official, Action<int, int> progress, CancellationToken cancellation, Action<Dictionary<string, string>>? validate = null, AiTranslationCheckpoint? checkpoint = null, Action<AiTranslationRetryNotice>? retry = null, Func<TimeSpan, CancellationToken, Task>? retryDelay = null, int concurrency = 1, IReadOnlyList<TranslationReference>? examples = null)
	{
		AiTranslationDiagnostics diagnostics = client.Diagnostics;
		if (diagnostics != null)
		{
			diagnostics.Stage = AiTranslationStage.PrepareEntries;
		}
		AiTranslationDocument result = new AiTranslationDocument
		{
			SourceName = name,
			SourceDescription = description
		};
		List<(string Table, string Key, string Text)> entries = new List<(string, string, string)>(1) { ("", "name", name) };
		if (!string.IsNullOrWhiteSpace(description))
		{
			entries.Add(("", "description", description));
		}
		List<TranslationReference> references = official.Take(180).ToList();
		List<string> list = new List<string>();
		foreach (IGrouping<string, KeyValuePair<string, byte[]>> group in (from p in original
			group p by p.Key.Substring(p.Key.IndexOf('/') + 1)).OrderBy<IGrouping<string, KeyValuePair<string, byte[]>>, string>((IGrouping<string, KeyValuePair<string, byte[]>> g) => g.Key, StringComparer.Ordinal))
		{
			KeyValuePair<string, byte[]> keyValuePair = group.FirstOrDefault((KeyValuePair<string, byte[]> p) => p.Key.StartsWith("eng/", StringComparison.Ordinal));
			if (keyValuePair.Key == null)
			{
				keyValuePair = group.OrderBy<KeyValuePair<string, byte[]>, string>((KeyValuePair<string, byte[]> p) => p.Key, StringComparer.Ordinal).First();
			}
			Dictionary<string, string> dictionary = ModTranslationFiles.Parse(keyValuePair.Value);
			KeyValuePair<string, byte[]> keyValuePair2 = group.FirstOrDefault((KeyValuePair<string, byte[]> p) => p.Key == language + "/" + group.Key);
			Dictionary<string, string> dictionary2 = ((keyValuePair2.Key == null) ? new Dictionary<string, string>() : ModTranslationFiles.Parse(keyValuePair2.Value));
			foreach (var (text4, text5) in dictionary)
			{
				if (text4.StartsWith("EXTENSION.", StringComparison.Ordinal) || string.IsNullOrWhiteSpace(text5))
				{
					continue;
				}
				if (list.Sum((string s) => s.Length) < 10000 && (text4.Contains("NAME", StringComparison.OrdinalIgnoreCase) || group.Key.Contains("character", StringComparison.OrdinalIgnoreCase) || group.Key.Contains("event", StringComparison.OrdinalIgnoreCase)))
				{
					list.Add(group.Key + "/" + text4 + ": " + text5.Substring(0, Math.Min(600, text5.Length)));
				}
				if (dictionary2.TryGetValue(text4, out var value) && !string.IsNullOrWhiteSpace(value) && (keyValuePair2.Key == keyValuePair.Key || value != text5))
				{
					if (references.Count < 220 && text5.Length < 300)
					{
						references.Add(new TranslationReference(text5, value, "mod"));
					}
					continue;
				}
				if (text5.Length > 12000)
				{
					throw new TranslationFileException("tooLarge");
				}
				entries.Add((group.Key, text4, text5));
			}
		}
		if (entries.Count > 20000 || name.Length > 4000 || description.Length > 12000)
		{
			throw new TranslationFileException("tooLarge");
		}
		foreach (string item3 in AiTranslationReferences.RecurringTerms(entries.Select<(string, string, string), string>(((string Table, string Key, string Text) e) => e.Text), from e in entries
			where Rank(e) == 2
			select e.Text, references.Select((TranslationReference r) => r.Source)))
		{
			entries.Add(("", "term", item3));
		}
		entries = entries.OrderBy(Rank).ToList();
		string item = ModTranslationFiles.Languages.First(((string Code, string Name) l) => l.Code == language).Name;
		AiTranslationBrief brief = new AiTranslationBrief(item, name, description.Substring(0, Math.Min(6000, description.Length)), list, references, examples);
		Dictionary<string, (string Table, string Key, string Text)> indexed = entries.Select<(string, string, string), ((string, string, string), string)>(((string Table, string Key, string Text) entry, int index) => (entry: entry, id: index.ToString(CultureInfo.InvariantCulture))).ToDictionary<((string, string, string), string), string, (string, string, string)>((((string Table, string Key, string Text) entry, string id) p) => p.id, (((string Table, string Key, string Text) entry, string id) p) => p.entry, StringComparer.Ordinal);
		string fingerprint = AiTranslationCheckpoint.Fingerprint(settings, language, name, description, original, official, examples);
		Dictionary<string, string> completed = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (KeyValuePair<string, string> item4 in checkpoint?.Read(fingerprint) ?? new Dictionary<string, string>(StringComparer.Ordinal))
		{
			cancellation.ThrowIfCancellationRequested();
			if (indexed.TryGetValue(item4.Key, out (string, string, string) value2) && (string.IsNullOrWhiteSpace(value2.Item3) || !string.IsNullOrWhiteSpace(item4.Value)))
			{
				try
				{
					new ProtectedTranslationText(value2.Item3).VerifyEquivalent(new ProtectedTranslationText(item4.Value));
					ValidateEntry(item4.Key, item4.Value);
					completed.Add(item4.Key, item4.Value);
				}
				catch (TranslationFileException)
				{
				}
			}
		}
		List<TranslationReference> drafts = new List<TranslationReference>();
		foreach (KeyValuePair<string, (string, string, string)> item5 in indexed)
		{
			if (completed.TryGetValue(item5.Key, out string value3))
			{
				AddDraft(item5.Key, value3);
			}
		}
		cancellation.ThrowIfCancellationRequested();
		checkpoint?.Save(fingerprint, completed);
		if (diagnostics != null)
		{
			diagnostics.CompletedEntries = completed.Count;
			diagnostics.TotalEntries = entries.Count;
		}
		progress(completed.Count, entries.Count);
		AiCanon aiCanon = checkpoint?.ReadCanon(fingerprint);
		List<string[]> parts;
		AiCanon?[] answers;
		if (aiCanon == null)
		{
			List<string> source2 = AiTranslationReferences.CanonCandidates(from e in entries
				where e.Table.Length > 0 || e.Key == "name"
				select e.Text, references.Select((TranslationReference r) => r.Source));
			parts = source2.Chunk(80).ToList();
			answers = new AiCanon[parts.Count];
			if (concurrency > 1)
			{
				await Task.WhenAll(Enumerable.Range(0, parts.Count).Select(Ask)).ConfigureAwait(continueOnCapturedContext: false);
			}
			else
			{
				for (int i = 0; i < parts.Count; i++)
				{
					await Ask(i).ConfigureAwait(continueOnCapturedContext: false);
				}
			}
			aiCanon = AiCanon.Merge(answers.OfType<AiCanon>());
			if (diagnostics != null && parts.Count > 0 && answers.All((AiCanon answer) => answer == null))
			{
				diagnostics.CanonNames = -1;
			}
			if (checkpoint != null)
			{
				checkpoint.Canon = aiCanon;
				checkpoint.Save(fingerprint, completed);
			}
		}
		else if (checkpoint != null)
		{
			checkpoint.Canon = aiCanon;
		}
		if (diagnostics != null && aiCanon.Names.Count > 0)
		{
			diagnostics.CanonNames = aiCanon.Names.Count;
		}
		if (aiCanon.Work.Length > 0)
		{
			brief = brief with
			{
				SourceWork = aiCanon.Work
			};
		}
		drafts.InsertRange(0, aiCanon.Names.Select<KeyValuePair<string, string>, TranslationReference>((KeyValuePair<string, string> p) => new TranslationReference(p.Key, p.Value, "canon")));
		object gate = new object();
		bool failureLogged = false;
		Dictionary<string, TranslationFileException> skipped = new Dictionary<string, TranslationFileException>(StringComparer.Ordinal);
		int[] array = new int[4] { 0, 1, 2, 3 };
		foreach (int rank in array)
		{
			List<Dictionary<string, string>> batches = Batches(indexed.Where<KeyValuePair<string, (string, string, string)>>((KeyValuePair<string, (string Table, string Key, string Text)> p) => !completed.ContainsKey(p.Key) && Rank(p.Value) == rank));
			int num = rank;
			bool flag = (uint)(num - 2) <= 1u;
			int alone = ((flag && concurrency > 1 && batches.Count > 1 && !completed.Any<KeyValuePair<string, string>>((KeyValuePair<string, string> p) => Rank(indexed[p.Key]) == rank)) ? 1 : 0);
			await RunConcurrentAsync(batches.Take(alone).ToList(), 1, cancellation, TranslateBatch).ConfigureAwait(continueOnCapturedContext: false);
			await RunConcurrentAsync(batches.Skip(alone).ToList(), concurrency, cancellation, TranslateBatch).ConfigureAwait(continueOnCapturedContext: false);
		}
		cancellation.ThrowIfCancellationRequested();
		result.SkippedEntries = skipped.Count;
		result.SkippedSummary = AiTranslationDiagnostics.Skipped(skipped.Values);
		foreach (KeyValuePair<string, (string, string, string)> item6 in indexed)
		{
			(string, string, string) value4 = item6.Value;
			if (!completed.TryGetValue(item6.Key, out string value5))
			{
				continue;
			}
			if (value4.Item1.Length == 0)
			{
				if (value4.Item2 == "name")
				{
					result.Name = value5;
				}
				else if (value4.Item2 == "description")
				{
					result.Description = value5;
				}
				continue;
			}
			if (!result.Tables.TryGetValue(value4.Item1, out Dictionary<string, string> value6))
			{
				value6 = (result.Tables[value4.Item1] = new Dictionary<string, string>(StringComparer.Ordinal));
			}
			value6[value4.Item2] = value5;
			if (!result.Sources.TryGetValue(value4.Item1, out Dictionary<string, string> value7))
			{
				value7 = (result.Sources[value4.Item1] = new Dictionary<string, string>(StringComparer.Ordinal));
			}
			value7[value4.Item2] = value4.Item3;
		}
		return result;
		void AddDraft(string id, string text)
		{
			string item2 = indexed[id].Text;
			if (item2.Length < 100 && drafts.Count < 2000)
			{
				try
				{
					if (new ProtectedTranslationText(item2).Tokens.Values.Any(delegate(string kind)
					{
						bool flag2 = ((kind == "number" || kind == "code") ? true : false);
						return !flag2;
					}))
					{
						return;
					}
				}
				catch (TranslationFileException)
				{
					return;
				}
				drafts.Add(new TranslationReference(item2, text, "draft"));
			}
		}
		async Task Ask(int part)
		{
			AiTranslationDiagnostics partDiagnostics = ((diagnostics == null) ? null : new AiTranslationDiagnostics());
			try
			{
				AiCanon?[] array2 = answers;
				array2[part] = await ((partDiagnostics == null) ? client : client.WithDiagnostics(partDiagnostics)).RecallCanonAsync(settings, key, brief, parts[part], cancellation).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (TranslationFileException ex3) when (((Func<bool>)delegate
			{
				// Could not convert BlockContainer to single expression
				bool flag3 = default(bool);
				switch (ex3.Reason)
				{
				case "auth":
				case "quota":
				case "model":
				case "endpoint":
				case "settings":
					flag3 = true;
					break;
				default:
					flag3 = false;
					break;
				}
				return !flag3;
			}).Invoke())
			{
			}
			finally
			{
				if (partDiagnostics != null)
				{
					diagnostics.Add(partDiagnostics);
				}
			}
		}
		async Task TranslateBatch(Dictionary<string, string> source)
		{
			List<TranslationReference> list2;
			lock (gate)
			{
				list2 = Glossary(drafts, source.Values);
				int stage = Rank(indexed[source.Keys.First()]);
				int num2 = stage;
				if ((uint)(num2 - 2) <= 1u)
				{
					list2.AddRange(AiTranslationReferences.Earlier(from p in completed
						where Rank(indexed[p.Key]) == stage
						select (Text: indexed[p.Key].Text, Value: p.Value), source.Values));
				}
			}
			List<TranslationReference> list3 = new List<TranslationReference>();
			list3.AddRange(references);
			list3.AddRange(list2.Where(delegate(TranslationReference r)
			{
				string kind2 = r.Kind;
				return (kind2 == "draft" || kind2 == "canon") ? true : false;
			}));
			List<TranslationReference> terms = list3;
			Dictionary<string, string> dictionary5 = source.Keys.ToDictionary<string, string, string>((string id) => id, (string id) => Role(indexed[id]), StringComparer.Ordinal);
			AiTranslationDiagnostics batchDiagnostics = ((diagnostics == null) ? null : new AiTranslationDiagnostics());
			AiTranslationClient client2 = ((batchDiagnostics == null) ? client : client.WithDiagnostics(batchDiagnostics));
			try
			{
				AiTranslationSettings settings2 = settings;
				byte[] key3 = key;
				AiTranslationBrief brief2 = brief;
				Action<string, string> validate2 = Validate;
				Action<Dictionary<string, string>> accept = delegate(Dictionary<string, string> accepted)
				{
					lock (gate)
					{
						cancellation.ThrowIfCancellationRequested();
						AiTranslationStage? aiTranslationStage = batchDiagnostics?.Stage;
						if (batchDiagnostics != null)
						{
							batchDiagnostics.Stage = AiTranslationStage.SaveProgress;
						}
						foreach (KeyValuePair<string, string> item7 in accepted)
						{
							completed.Add(item7.Key, ProtectedTranslationText.ForGamePlurals(language, AiTranslationReferences.TidySpacing(language, item7.Value)));
						}
						checkpoint?.Save(fingerprint, completed);
						foreach (KeyValuePair<string, string> item8 in accepted)
						{
							AddDraft(item8.Key, completed[item8.Key]);
						}
						if (diagnostics != null)
						{
							diagnostics.CompletedEntries = completed.Count;
						}
						if (batchDiagnostics != null)
						{
							batchDiagnostics.Stage = aiTranslationStage.Value;
						}
						progress(completed.Count, entries.Count);
					}
				};
				CancellationToken cancellation2 = cancellation;
				Action<AiTranslationRetryNotice>? notify = retry;
				Func<TimeSpan, CancellationToken, Task>? delay = retryDelay;
				IReadOnlyList<TranslationReference> glossary = list2;
				IReadOnlyDictionary<string, string> roles = dictionary5;
				await AiTranslationRetry.TranslateAsync(client2, settings2, key3, brief2, source, validate2, accept, cancellation2, notify, delay, null, glossary, roles, delegate(Dictionary<string, TranslationFileException> unrepaired)
				{
					lock (gate)
					{
						foreach (KeyValuePair<string, TranslationFileException> item9 in unrepaired)
						{
							skipped[item9.Key] = item9.Value;
						}
					}
				}).ConfigureAwait(continueOnCapturedContext: false);
			}
			catch (Exception) when (batchDiagnostics != null && !cancellation.IsCancellationRequested)
			{
				lock (gate)
				{
					if (!failureLogged)
					{
						diagnostics.AdoptFailure(batchDiagnostics);
					}
					failureLogged = true;
				}
				throw;
			}
			finally
			{
				if (batchDiagnostics != null)
				{
					diagnostics.Add(batchDiagnostics);
				}
			}
			void Validate(string id, string text)
			{
				ValidateEntry(id, text);
				AiTranslationReferences.CheckHighlights(language, indexed[id].Text, text, terms);
				AiTranslationReferences.CheckForeignWords(language, indexed[id].Text, text);
				AiTranslationReferences.CheckMeasureWords(language, indexed[id].Text, text);
			}
		}
		void ValidateEntry(string id, string text)
		{
			if (indexed[id].Table.Length > 0)
			{
				validate?.Invoke(new Dictionary<string, string> { [id] = text });
			}
			if (AiTranslationReferences.LeavesChoiceUntranslated(language, indexed[id].Text, text))
			{
				throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.Untranslated);
			}
		}
	}

	private static int Rank((string Table, string Key, string Text) entry)
	{
		if (entry.Table.Length != 0)
		{
			if (!entry.Key.EndsWith("NAME", StringComparison.OrdinalIgnoreCase) && !entry.Key.EndsWith("TITLE", StringComparison.OrdinalIgnoreCase))
			{
				return 3;
			}
			return 2;
		}
		return (entry.Key == "term") ? 1 : 0;
	}

	private static string Role((string Table, string Key, string Text) entry)
	{
		if (entry.Table.Length != 0)
		{
			return entry.Table + "/" + entry.Key;
		}
		return "mod " + entry.Key;
	}

	private static List<Dictionary<string, string>> Batches(IEnumerable<KeyValuePair<string, (string Table, string Key, string Text)>> pending)
	{
		List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		int num = 0;
		foreach (KeyValuePair<string, (string, string, string)> item in pending)
		{
			if (dictionary.Count > 0 && (dictionary.Count >= 24 || num + item.Value.Item3.Length > 6000))
			{
				list.Add(dictionary);
				dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
				num = 0;
			}
			dictionary.Add(item.Key, item.Value.Item3);
			num += item.Value.Item3.Length;
		}
		if (dictionary.Count > 0)
		{
			list.Add(dictionary);
		}
		return list;
	}

	private static List<TranslationReference> Glossary(List<TranslationReference> drafts, IEnumerable<string> texts)
	{
		string joined = string.Join("\n", texts);
		return drafts.Where((TranslationReference d) => d.Source.Length > 1 && Mentions(d.Source)).DistinctBy<TranslationReference, string>((TranslationReference d) => d.Source, StringComparer.Ordinal).Take(60)
			.ToList();
		bool Mentions(string source)
		{
			for (int num = joined.IndexOf(source, StringComparison.OrdinalIgnoreCase); num >= 0; num = joined.IndexOf(source, num + 1, StringComparison.OrdinalIgnoreCase))
			{
				int num2 = num + source.Length;
				if (num == 0 || !Word(joined[num - 1]) || !Word(source[0]))
				{
					if (num2 != joined.Length && Word(joined[num2]))
					{
						if (Word(source[source.Length - 1]))
						{
							continue;
						}
					}
					return true;
				}
			}
			return false;
		}
		static bool Word(char c)
		{
			return char.IsLetterOrDigit(c);
		}
	}

	private static async Task RunConcurrentAsync(IReadOnlyList<Dictionary<string, string>> batches, int concurrency, CancellationToken cancellation, Func<Dictionary<string, string>, Task> translate)
	{
		Exception failure = null;
		int next = -1;
		if (batches.Count == 0)
		{
			return;
		}
		try
		{
			await Task.WhenAll(from _ in Enumerable.Range(0, Math.Clamp(concurrency, 1, batches.Count))
				select Worker()).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
		{
		}
		cancellation.ThrowIfCancellationRequested();
		if (failure != null)
		{
			ExceptionDispatchInfo.Throw(failure);
		}
		async Task Worker()
		{
			while (Volatile.Read(in failure) == null)
			{
				cancellation.ThrowIfCancellationRequested();
				int num = Interlocked.Increment(ref next);
				if (num >= batches.Count)
				{
					break;
				}
				try
				{
					await translate(batches[num]).ConfigureAwait(continueOnCapturedContext: false);
				}
				catch (Exception ex2) when (!(ex2 is OperationCanceledException) || !cancellation.IsCancellationRequested)
				{
					Interlocked.CompareExchange(ref failure, ex2, null);
					break;
				}
			}
		}
	}
}

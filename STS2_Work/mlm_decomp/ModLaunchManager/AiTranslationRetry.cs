using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ModLaunchManager;

internal static class AiTranslationRetry
{
	internal const int MaxAdditionalRequests = 8;

	internal const int MaxFormatAttempts = 3;

	internal const int MaxTransientRetries = 2;

	internal static readonly TimeSpan MaxRetryDelay = TimeSpan.FromSeconds(30L);

	internal static readonly TimeSpan MaxBatchDuration = TimeSpan.FromMinutes(6L);

	internal static async Task TranslateAsync(AiTranslationClient client, AiTranslationSettings settings, byte[] key, AiTranslationBrief brief, Dictionary<string, string> source, Action<string, string>? validate, Action<Dictionary<string, string>> accept, CancellationToken cancellation, Action<AiTranslationRetryNotice>? notify = null, Func<TimeSpan, CancellationToken, Task>? delay = null, TimeSpan? batchDuration = null, IReadOnlyList<TranslationReference>? glossary = null, IReadOnlyDictionary<string, string>? roles = null, Action<Dictionary<string, TranslationFileException>>? giveUp = null)
	{
		using CancellationTokenSource deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
		deadline.CancelAfter(batchDuration ?? MaxBatchDuration);
		try
		{
			await TranslateWithinDeadlineAsync(client, settings, key, brief, source, validate, accept, deadline.Token, notify, delay, glossary, roles, giveUp).ConfigureAwait(continueOnCapturedContext: false);
		}
		catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
		{
			throw new TranslationFileException("timeout");
		}
	}

	private static async Task TranslateWithinDeadlineAsync(AiTranslationClient client, AiTranslationSettings settings, byte[] key, AiTranslationBrief brief, Dictionary<string, string> source, Action<string, string>? validate, Action<Dictionary<string, string>> accept, CancellationToken cancellation, Action<AiTranslationRetryNotice>? notify, Func<TimeSpan, CancellationToken, Task>? delay, IReadOnlyList<TranslationReference>? glossary, IReadOnlyDictionary<string, string>? roles, Action<Dictionary<string, TranslationFileException>>? giveUp)
	{
		if (delay == null)
		{
			delay = Task.Delay;
		}
		Dictionary<string, string> pending = new Dictionary<string, string>(source, StringComparer.Ordinal);
		Dictionary<string, TranslationFileException> failures = new Dictionary<string, TranslationFileException>(StringComparer.Ordinal);
		Dictionary<string, string> fallbacks = new Dictionary<string, string>(StringComparer.Ordinal);
		TranslationFileException last = new TranslationFileException("response");
		int requests = 0;
		for (int round = 0; round < 3; round++)
		{
			if (pending.Count <= 0)
			{
				break;
			}
			int val = round switch
			{
				1 => 4, 
				0 => source.Count, 
				_ => 1, 
			};
			foreach (KeyValuePair<string, string>[] item in pending.ToArray().Chunk(Math.Max(1, val)))
			{
				Dictionary<string, string> request = item.ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value, StringComparer.Ordinal);
				AiTranslationBatchResult result = null;
				int transient = 0;
				while (true)
				{
					cancellation.ThrowIfCancellationRequested();
					if (requests >= 9)
					{
						break;
					}
					if (round > 0 && transient == 0)
					{
						notify?.Invoke(new AiTranslationRetryNotice(AiTranslationRetryKind.Format, round, request.Count, TimeSpan.Zero, AiTranslationDiagnostics.Issues(from id in request.Keys.Where(failures.ContainsKey)
							select failures[id])));
					}
					requests++;
					try
					{
						result = await client.TranslateBatchAsync(settings, key, brief, request, cancellation, validate, (round > 0) ? failures : null, glossary, roles).ConfigureAwait(continueOnCapturedContext: false);
					}
					catch (TranslationFileException ex)
					{
						TranslationFileException ex2 = (last = ex);
						if (ex2.RetryKind != AiTranslationRetryKind.Format)
						{
							if (ex2.RetryKind != AiTranslationRetryKind.Transient || transient >= 2 || requests >= 9)
							{
								throw;
							}
							if (ex2.RetryAfter > MaxRetryDelay)
							{
								throw;
							}
							double num = Math.Max(Math.Pow(2.0, transient), ex2.RetryAfter?.TotalSeconds ?? 0.0);
							TimeSpan timeSpan = TimeSpan.FromSeconds(Math.Min(MaxRetryDelay.TotalSeconds, num + Random.Shared.NextDouble() * 0.25));
							if (client.Diagnostics != null)
							{
								client.Diagnostics.Stage = AiTranslationStage.WaitToRetry;
							}
							notify?.Invoke(new AiTranslationRetryNotice(AiTranslationRetryKind.Transient, transient + 1, request.Count, timeSpan));
							await delay(timeSpan, cancellation).ConfigureAwait(continueOnCapturedContext: false);
							goto IL_057b;
						}
						foreach (string key2 in request.Keys)
						{
							failures[key2] = ex2;
						}
					}
					if (!(result == null))
					{
						cancellation.ThrowIfCancellationRequested();
						if (result.Accepted.Count > 0)
						{
							accept(result.Accepted);
							foreach (string key3 in result.Accepted.Keys)
							{
								pending.Remove(key3);
								failures.Remove(key3);
							}
						}
						foreach (KeyValuePair<string, TranslationFileException> item2 in result.Failed)
						{
							failures[item2.Key] = item2.Value;
							last = item2.Value;
							if (item2.Value.Fallback != null)
							{
								fallbacks[item2.Key] = item2.Value.Fallback;
							}
						}
						if (round > 0)
						{
							Keep(from f in result.Failed
								where f.Value.Fallback != null
								select f.Key);
						}
					}
					goto IL_075a;
					IL_057b:
					transient++;
				}
				goto end_IL_079d;
				IL_075a:;
			}
			continue;
			end_IL_079d:
			break;
		}
		Keep(pending.Keys.ToArray());
		if (pending.Count != 0)
		{
			TranslationFileException value;
			Dictionary<string, TranslationFileException> dictionary = pending.Keys.ToDictionary<string, string, TranslationFileException>((string id) => id, (string id) => (!failures.TryGetValue(id, out value)) ? last : value, StringComparer.Ordinal);
			if (giveUp == null || dictionary.Values.Any((TranslationFileException error) => error.Reason != "invalidFormat"))
			{
				throw last;
			}
			giveUp(dictionary);
		}
		void Keep(IEnumerable<string> ids)
		{
			Dictionary<string, string> dictionary2 = ids.Where((string id) => pending.ContainsKey(id) && fallbacks.ContainsKey(id)).ToDictionary<string, string, string>((string id) => id, (string id) => fallbacks[id], StringComparer.Ordinal);
			if (dictionary2.Count != 0)
			{
				accept(dictionary2);
				foreach (string key4 in dictionary2.Keys)
				{
					pending.Remove(key4);
					failures.Remove(key4);
				}
				client.Diagnostics?.KeepStyleMisses(dictionary2.Count);
			}
		}
	}
}

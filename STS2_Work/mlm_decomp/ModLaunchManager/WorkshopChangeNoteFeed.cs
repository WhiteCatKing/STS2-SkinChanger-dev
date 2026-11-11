using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace ModLaunchManager;

internal static class WorkshopChangeNoteFeed
{
	private enum OutcomeKind
	{
		Done,
		Private,
		Failed,
		Stop
	}

	private readonly record struct Outcome(OutcomeKind Kind, IReadOnlyList<WorkshopChangeNote> Notes, bool Truncated, string? Reason)
	{
		internal static Outcome Done(IReadOnlyList<WorkshopChangeNote> notes, bool truncated)
		{
			return new Outcome(OutcomeKind.Done, notes, truncated, null);
		}

		internal static Outcome Fail(string reason)
		{
			return new Outcome(OutcomeKind.Failed, Array.Empty<WorkshopChangeNote>(), Truncated: false, reason);
		}

		internal static Outcome Stop(string reason)
		{
			return new Outcome(OutcomeKind.Stop, Array.Empty<WorkshopChangeNote>(), Truncated: false, reason);
		}
	}

	internal const string FileName = "workshop_change_notes.json";

	private const int MaxPages = 3;

	private static readonly TimeSpan Spacing = TimeSpan.FromSeconds(1L);

	private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(15L);

	private static readonly Dictionary<ulong, uint> Reconciled = new Dictionary<ulong, uint>();

	private static readonly List<WorkshopChangeNoteItem> Items = new List<WorkshopChangeNoteItem>();

	private static readonly Queue<WorkshopChangeNoteItem> Pending = new Queue<WorkshopChangeNoteItem>();

	private static readonly Dictionary<(ulong ItemId, uint Time, string Language), string> Translations = new Dictionary<(ulong, uint, string), string>();

	private static WorkshopChangeNoteHistory? _history;

	private static HttpClient? _http;

	private static CancellationTokenSource? _cancellation;

	private static Action? _onChanged;

	private static int _generation;

	private static bool _reading;

	private static bool _pruned;

	internal static IReadOnlyList<WorkshopChangeNoteItem> Updates => Items.Where((WorkshopChangeNoteItem item) => !item.NothingNew).ToList();

	internal static int ReadingCount => Items.Count((WorkshopChangeNoteItem item) => item.IsReading);

	internal static int FailedCount => Items.Count((WorkshopChangeNoteItem item) => item.State == WorkshopChangeNoteState.Failed);

	private static HttpClient Http => _http ?? (_http = CreateHttpClient());

	internal static WorkshopChangeNoteItem? For(ulong itemId)
	{
		return Items.FirstOrDefault((WorkshopChangeNoteItem item) => item.ItemId == itemId && !item.NothingNew);
	}

	internal static IReadOnlyList<WorkshopChangeNote> RecentNotes(ulong itemId)
	{
		return _history?.RecentNotes(itemId) ?? Array.Empty<WorkshopChangeNote>();
	}

	internal static IReadOnlyList<WorkshopChangeNote> ShownNotes(ulong itemId)
	{
		return For(itemId)?.Notes ?? RecentNotes(itemId);
	}

	internal static string? Translation(ulong itemId, WorkshopChangeNote note, string language)
	{
		string? text = Translations.GetValueOrDefault((itemId, note.Time, language));
		if (text == null)
		{
			WorkshopChangeNoteHistory? history = _history;
			if (history == null)
			{
				return null;
			}
			text = history.Translation(itemId, note.Time, language);
		}
		return text;
	}

	internal static void SaveTranslation(ulong itemId, WorkshopChangeNote note, string language, string text)
	{
		Translations[(itemId, note.Time, language)] = text;
		if (_history != null && _history.StoreTranslation(itemId, note.Time, language, text))
		{
			Save(_history);
		}
	}

	internal static string ChangeNotesUrl(ulong itemId)
	{
		return $"https://steamcommunity.com/sharedfiles/filedetails/changelog/{itemId}";
	}

	internal static void Start(Action onChanged)
	{
		Stop();
		_onChanged = onChanged;
		_cancellation = new CancellationTokenSource();
		try
		{
			_history = WorkshopChangeNoteHistory.Load(Path.Combine(Path.GetDirectoryName(CanonicalLaunchStore.Path), "workshop_change_notes.json"));
			if (_history.LoadError != null)
			{
				GD.PushWarning("[ModLaunchManager] Workshop change-note history was unreadable and starts over: " + _history.LoadError);
			}
		}
		catch (Exception ex)
		{
			_history = null;
			GD.PushWarning("[ModLaunchManager] Workshop change notes are unavailable: " + ex.Message);
		}
	}

	internal static void Refresh()
	{
		if (_history == null || WorkshopUpdates.Checking || WorkshopUpdates.HasPendingScan)
		{
			return;
		}
		try
		{
			RefreshCore(_history);
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} Workshop change-note check failed: {value}");
		}
	}

	private static void RefreshCore(WorkshopChangeNoteHistory history)
	{
		bool flag = false;
		bool flag2 = false;
		IReadOnlyCollection<WorkshopStatus> allStatuses = WorkshopUpdates.AllStatuses;
		if (!_pruned)
		{
			_pruned = true;
			if (!WorkshopUpdates.CheckFailed)
			{
				flag |= history.Prune(allStatuses.Select((WorkshopStatus status) => status.ItemId).ToHashSet());
			}
		}
		foreach (WorkshopStatus status2 in allStatuses)
		{
			if (WorkshopBranches.IsPinned(status2.ItemId) || (Reconciled.TryGetValue(status2.ItemId, out var value) && value == status2.LocalUpdated) || Items.Any((WorkshopChangeNoteItem item) => item.ItemId == status2.ItemId && item.IsReading))
			{
				continue;
			}
			Reconciled[status2.ItemId] = status2.LocalUpdated;
			WorkshopChangeNoteCheck workshopChangeNoteCheck = history.Check(status2.ItemId, status2.LocalUpdated);
			switch (workshopChangeNoteCheck.Kind)
			{
			case WorkshopChangeNoteCheckKind.Baseline:
			case WorkshopChangeNoteCheckKind.Older:
				flag = true;
				break;
			case WorkshopChangeNoteCheckKind.Fetch:
			{
				Items.RemoveAll((WorkshopChangeNoteItem item) => item.ItemId == status2.ItemId);
				WorkshopChangeNoteItem item2 = new WorkshopChangeNoteItem
				{
					ItemId = status2.ItemId,
					LocalTime = status2.LocalUpdated,
					After = workshopChangeNoteCheck.After,
					Through = workshopChangeNoteCheck.Through
				};
				Items.Add(item2);
				Pending.Enqueue(item2);
				flag2 = true;
				break;
			}
			}
		}
		if (flag)
		{
			Save(history);
		}
		if (flag2)
		{
			GD.Print($"{"[ModLaunchManager]"} {Pending.Count} Workshop item(s) changed since the " + "last launch; reading their change notes.");
			Notify();
			ReadPending();
		}
	}

	internal static void Stop()
	{
		_generation++;
		_cancellation?.Cancel();
		_cancellation?.Dispose();
		_cancellation = null;
		Items.Clear();
		Pending.Clear();
		Reconciled.Clear();
		Translations.Clear();
		_history = null;
		_onChanged = null;
		_reading = false;
		_pruned = false;
	}

	private static async void ReadPending()
	{
		if (_reading || _cancellation == null)
		{
			return;
		}
		_reading = true;
		int generation = _generation;
		CancellationToken token = _cancellation.Token;
		try
		{
			bool first = true;
			while (generation == _generation && Pending.Count > 0)
			{
				if (!first)
				{
					await Task.Delay(Spacing, token);
				}
				first = false;
				WorkshopChangeNoteItem item2 = Pending.Dequeue();
				item2.State = WorkshopChangeNoteState.Loading;
				Notify();
				Outcome outcome = await ReadAsync(item2, token);
				if (generation != _generation)
				{
					break;
				}
				Apply(item2, outcome);
				if (outcome.Kind == OutcomeKind.Stop)
				{
					FailPending();
				}
				Notify();
			}
		}
		catch (OperationCanceledException) when (token.IsCancellationRequested)
		{
		}
		catch (Exception value)
		{
			if (generation != _generation)
			{
				return;
			}
			GD.PushWarning($"{"[ModLaunchManager]"} reading Workshop change notes failed: {value}");
			foreach (WorkshopChangeNoteItem item3 in Items.Where((WorkshopChangeNoteItem item) => item.IsReading))
			{
				item3.State = WorkshopChangeNoteState.Failed;
			}
			Pending.Clear();
			Notify();
		}
		finally
		{
			if (generation == _generation)
			{
				_reading = false;
			}
		}
	}

	private static async Task<Outcome> ReadAsync(WorkshopChangeNoteItem item, CancellationToken token)
	{
		List<WorkshopChangeNote> found = new List<WorkshopChangeNote>();
		string language = WorkshopUpdates.SteamApiLanguage(MlmLocManager.EffectiveLocale);
		for (int page = 1; page <= 3; page++)
		{
			if (page > 1)
			{
				await Task.Delay(Spacing, token);
			}
			string requestUri = ChangeNotesUrl(item.ItemId) + "?l=" + language + ((page > 1) ? $"&p={page}" : string.Empty);
			string html;
			try
			{
				using HttpResponseMessage response = await Http.GetAsync(requestUri, token);
				if (response.StatusCode == HttpStatusCode.TooManyRequests)
				{
					return Outcome.Stop("Steam is rate-limiting requests (HTTP 429)");
				}
				if (!response.IsSuccessStatusCode)
				{
					return Outcome.Fail($"HTTP {response.StatusCode}");
				}
				html = await response.Content.ReadAsStringAsync(token);
			}
			catch (HttpRequestException ex)
			{
				return Outcome.Stop(ex.Message);
			}
			catch (TaskCanceledException) when (!token.IsCancellationRequested)
			{
				return Outcome.Stop($"no answer within {RequestTimeout.TotalSeconds:F0}s");
			}
			WorkshopChangeNotePage workshopChangeNotePage = WorkshopChangeNotePage.Parse(html, page);
			if (workshopChangeNotePage.Kind == WorkshopChangeNotePageKind.Unavailable)
			{
				return (page == 1) ? new Outcome(OutcomeKind.Private, Array.Empty<WorkshopChangeNote>(), Truncated: false, null) : Outcome.Fail($"page {page} became unavailable");
			}
			if (workshopChangeNotePage.Kind == WorkshopChangeNotePageKind.Unrecognized)
			{
				return Outcome.Fail($"page {page} did not look like a change-notes page");
			}
			var (collection, flag) = WorkshopChangeNoteHistory.Window(workshopChangeNotePage, item.After, item.Through);
			found.AddRange(collection);
			if (!flag)
			{
				return Outcome.Done(found, truncated: false);
			}
		}
		return Outcome.Done(found, truncated: true);
	}

	private static void Apply(WorkshopChangeNoteItem item, Outcome outcome)
	{
		switch (outcome.Kind)
		{
		case OutcomeKind.Done:
			item.Notes = outcome.Notes.OrderByDescending((WorkshopChangeNote note) => note.Time).ToArray();
			item.Truncated = outcome.Truncated;
			item.State = WorkshopChangeNoteState.Ready;
			_history?.Complete(item.ItemId, item.LocalTime, item.After, item.Notes);
			GD.Print($"{"[ModLaunchManager]"} [change notes] {item.ItemId}: {item.Notes.Count} new note(s){(item.Truncated ? ", more on the Workshop page" : string.Empty)}.");
			break;
		case OutcomeKind.Private:
			item.State = WorkshopChangeNoteState.Private;
			_history?.Complete(item.ItemId, item.LocalTime, item.After, Array.Empty<WorkshopChangeNote>());
			GD.Print($"{"[ModLaunchManager]"} [change notes] {item.ItemId}: notes are not public.");
			break;
		default:
			item.State = WorkshopChangeNoteState.Failed;
			GD.PushWarning($"{"[ModLaunchManager]"} [change notes] {item.ItemId}: could not read change notes ({outcome.Reason}); will try again next launch.");
			return;
		}
		if (_history != null)
		{
			Save(_history);
		}
	}

	private static void FailPending()
	{
		while (Pending.Count > 0)
		{
			Pending.Dequeue().State = WorkshopChangeNoteState.Failed;
		}
	}

	private static void Save(WorkshopChangeNoteHistory history)
	{
		try
		{
			history.Save();
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not save Workshop change-note history: " + ex.Message);
		}
	}

	private static HttpClient CreateHttpClient()
	{
		HttpClient httpClient = new HttpClient(new HttpClientHandler
		{
			AutomaticDecompression = DecompressionMethods.All,
			UseCookies = false,
			MaxAutomaticRedirections = 3
		});
		httpClient.Timeout = RequestTimeout;
		httpClient.MaxResponseContentBufferSize = 4194304L;
		httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ModLaunchManager");
		return httpClient;
	}

	private static void Notify()
	{
		try
		{
			_onChanged?.Invoke();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} change-note UI refresh failed: {value}");
		}
	}
}

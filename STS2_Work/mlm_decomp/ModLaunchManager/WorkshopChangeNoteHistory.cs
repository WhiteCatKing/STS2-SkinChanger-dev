using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ModLaunchManager;

internal sealed class WorkshopChangeNoteHistory
{
	internal const int FormatVersion = 1;

	internal const uint Grace = 900u;

	internal const int MaxStoredNotes = 5;

	internal const int MaxStoredLength = 1000;

	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true
	};

	private readonly string _path;

	private WorkshopChangeNoteDocument _document;

	internal string? LoadError { get; private set; }

	internal int Count => _document.Items.Count;

	private WorkshopChangeNoteHistory(string path, WorkshopChangeNoteDocument document)
	{
		_path = path;
		_document = document;
	}

	internal static WorkshopChangeNoteHistory Load(string path)
	{
		try
		{
			if (!File.Exists(path))
			{
				return new WorkshopChangeNoteHistory(path, new WorkshopChangeNoteDocument());
			}
			WorkshopChangeNoteDocument workshopChangeNoteDocument = JsonSerializer.Deserialize<WorkshopChangeNoteDocument>(File.ReadAllText(path, Encoding.UTF8)) ?? throw new InvalidDataException("empty document");
			if (workshopChangeNoteDocument.FormatVersion != 1)
			{
				throw new InvalidDataException($"unsupported format {workshopChangeNoteDocument.FormatVersion}");
			}
			WorkshopChangeNoteDocument workshopChangeNoteDocument2 = workshopChangeNoteDocument;
			if (workshopChangeNoteDocument2.Items == null)
			{
				Dictionary<ulong, WorkshopChangeNoteRecord> dictionary2 = (workshopChangeNoteDocument2.Items = new Dictionary<ulong, WorkshopChangeNoteRecord>());
			}
			foreach (WorkshopChangeNoteRecord value in workshopChangeNoteDocument.Items.Values)
			{
				WorkshopChangeNoteRecord current;
				WorkshopChangeNoteRecord workshopChangeNoteRecord = (current = value);
				if (current.Notes == null)
				{
					List<WorkshopChangeNoteRecordEntry> list2 = (current.Notes = new List<WorkshopChangeNoteRecordEntry>());
				}
				foreach (WorkshopChangeNoteRecordEntry note in workshopChangeNoteRecord.Notes)
				{
					if (note.Translations == null)
					{
						Dictionary<string, string> dictionary4 = (note.Translations = new Dictionary<string, string>(StringComparer.Ordinal));
					}
				}
			}
			return new WorkshopChangeNoteHistory(path, workshopChangeNoteDocument);
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is JsonException || ex is InvalidDataException || ex is NotSupportedException) ? 1 : 0) != 0)
		{
			return new WorkshopChangeNoteHistory(path, new WorkshopChangeNoteDocument())
			{
				LoadError = ex.Message
			};
		}
	}

	internal WorkshopChangeNoteCheck Check(ulong itemId, uint localTime)
	{
		if (itemId == 0L || localTime == 0)
		{
			return new WorkshopChangeNoteCheck(WorkshopChangeNoteCheckKind.Skip, 0u, 0u);
		}
		if (!_document.Items.TryGetValue(itemId, out WorkshopChangeNoteRecord value))
		{
			_document.Items[itemId] = new WorkshopChangeNoteRecord
			{
				LocalTime = localTime,
				SeenThrough = Add(localTime, 900u)
			};
			return new WorkshopChangeNoteCheck(WorkshopChangeNoteCheckKind.Baseline, 0u, 0u);
		}
		if (value.LocalTime == localTime)
		{
			return new WorkshopChangeNoteCheck(WorkshopChangeNoteCheckKind.Unchanged, 0u, 0u);
		}
		uint num = Add(localTime, 900u);
		if (num <= value.SeenThrough)
		{
			value.LocalTime = localTime;
			return new WorkshopChangeNoteCheck(WorkshopChangeNoteCheckKind.Older, 0u, 0u);
		}
		return new WorkshopChangeNoteCheck(WorkshopChangeNoteCheckKind.Fetch, value.SeenThrough, num);
	}

	internal void Complete(ulong itemId, uint localTime, uint after, IReadOnlyList<WorkshopChangeNote> notes)
	{
		if (!_document.Items.TryGetValue(itemId, out WorkshopChangeNoteRecord value))
		{
			value = new WorkshopChangeNoteRecord();
			_document.Items[itemId] = value;
		}
		uint val = ((notes.Count != 0) ? notes.Max((WorkshopChangeNote note) => note.Time) : 0u);
		value.LocalTime = localTime;
		value.SeenThrough = Math.Max(Math.Max(value.SeenThrough, after), Math.Max(localTime, val));
		if (notes.Count != 0)
		{
			value.Notes = (from note in notes.OrderByDescending((WorkshopChangeNote note) => note.Time).Take(5)
				select new WorkshopChangeNoteRecordEntry
				{
					Time = note.Time,
					Text = ((note.Text.Length <= 1000) ? note.Text : (note.Text.Substring(0, 1000) + "…")),
					Branches = note.Branches.ToList()
				}).ToList();
		}
	}

	internal IReadOnlyList<WorkshopChangeNote> RecentNotes(ulong itemId)
	{
		if (!_document.Items.TryGetValue(itemId, out WorkshopChangeNoteRecord value))
		{
			return Array.Empty<WorkshopChangeNote>();
		}
		return value.Notes.Select(delegate(WorkshopChangeNoteRecordEntry note)
		{
			uint time = note.Time;
			string text = note.Text ?? string.Empty;
			IReadOnlyList<string> branches = note.Branches;
			return new WorkshopChangeNote(time, text, branches ?? Array.Empty<string>());
		}).ToArray();
	}

	internal string? Translation(ulong itemId, uint time, string language)
	{
		return Stored(itemId, time)?.Translations.GetValueOrDefault(language);
	}

	internal bool StoreTranslation(ulong itemId, uint time, string language, string text)
	{
		WorkshopChangeNoteRecordEntry workshopChangeNoteRecordEntry = Stored(itemId, time);
		if (workshopChangeNoteRecordEntry == null)
		{
			return false;
		}
		workshopChangeNoteRecordEntry.Translations[language] = ((text.Length <= 2000) ? text : (text.Substring(0, 2000) + "…"));
		return true;
	}

	private WorkshopChangeNoteRecordEntry? Stored(ulong itemId, uint time)
	{
		if (!_document.Items.TryGetValue(itemId, out WorkshopChangeNoteRecord value))
		{
			return null;
		}
		return value.Notes.FirstOrDefault((WorkshopChangeNoteRecordEntry note) => note.Time == time);
	}

	internal bool Prune(IReadOnlySet<ulong> present)
	{
		List<ulong> list = _document.Items.Keys.Where((ulong id) => !present.Contains(id)).ToList();
		foreach (ulong item in list)
		{
			_document.Items.Remove(item);
		}
		return list.Count > 0;
	}

	internal void Save()
	{
		string directoryName = Path.GetDirectoryName(_path);
		if (!string.IsNullOrEmpty(directoryName))
		{
			Directory.CreateDirectory(directoryName);
		}
		string text = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
		try
		{
			File.WriteAllText(text, JsonSerializer.Serialize(_document, JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			File.Move(text, _path, overwrite: true);
		}
		finally
		{
			if (File.Exists(text))
			{
				File.Delete(text);
			}
		}
	}

	internal static (IReadOnlyList<WorkshopChangeNote> InWindow, bool ReadOlder) Window(WorkshopChangeNotePage page, uint after, uint through)
	{
		WorkshopChangeNote[] item = page.Notes.Where((WorkshopChangeNote note) => note.Time > after && note.Time <= through).ToArray();
		bool item2 = page.HasNextPage && page.Notes.Count > 0 && page.Notes.Min((WorkshopChangeNote note) => note.Time) > after;
		return (InWindow: item, ReadOlder: item2);
	}

	private static uint Add(uint time, uint seconds)
	{
		if (time <= (uint)(-1 - (int)seconds))
		{
			return time + seconds;
		}
		return uint.MaxValue;
	}
}

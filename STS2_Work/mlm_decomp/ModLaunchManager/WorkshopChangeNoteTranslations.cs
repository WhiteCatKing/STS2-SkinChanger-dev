using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Saves;

namespace ModLaunchManager;

internal static class WorkshopChangeNoteTranslations
{
	private static readonly HashSet<ulong> Running = new HashSet<ulong>();

	private static readonly HashSet<ulong> ShowingOriginal = new HashSet<ulong>();

	internal static Action<IReadOnlyList<ulong>>? Runner { get; set; }

	internal static Action? Changed { get; set; }

	internal static bool Busy => Running.Count > 0;

	internal static string Language
	{
		get
		{
			string game = null;
			try
			{
				SaveManager instance = SaveManager.Instance;
				object obj;
				if (instance == null)
				{
					obj = null;
				}
				else
				{
					SettingsSave settingsSave = instance.SettingsSave;
					obj = ((settingsSave != null) ? settingsSave.Language : null);
				}
				game = (string)obj;
			}
			catch
			{
			}
			if (game != null && ModTranslationFiles.Languages.Any(((string Code, string Name) language) => language.Code == game))
			{
				return game;
			}
			string launcher = MlmLocManager.AvailableLocales.FirstOrDefault((MlmLocaleInfo locale) => locale.Id == MlmLocManager.EffectiveLocale)?.GameLanguage;
			if (launcher == null || !ModTranslationFiles.Languages.Any(((string Code, string Name) language) => language.Code == launcher))
			{
				return "eng";
			}
			return launcher;
		}
	}

	internal static string LanguageName(string language)
	{
		return ModTranslationFiles.Languages.FirstOrDefault(((string Code, string Name) entry) => entry.Code == language).Name ?? language;
	}

	internal static WorkshopChangeNoteTranslationView ViewFor(ulong itemId)
	{
		List<WorkshopChangeNote> list = (from note in WorkshopChangeNoteFeed.ShownNotes(itemId)
			where note.Text.Length > 0
			select note).ToList();
		if (list.Count == 0)
		{
			return WorkshopChangeNoteTranslationView.None;
		}
		if (Running.Contains(itemId))
		{
			return WorkshopChangeNoteTranslationView.Translating;
		}
		string language = Language;
		if (list.Any((WorkshopChangeNote note) => WorkshopChangeNoteFeed.Translation(itemId, note, language) == null))
		{
			return WorkshopChangeNoteTranslationView.Untranslated;
		}
		if (!ShowingOriginal.Contains(itemId))
		{
			return WorkshopChangeNoteTranslationView.Translated;
		}
		return WorkshopChangeNoteTranslationView.Original;
	}

	internal static (string Text, bool Translated) Display(ulong itemId, WorkshopChangeNote note)
	{
		if (note.Text.Length > 0 && !ShowingOriginal.Contains(itemId))
		{
			string text = WorkshopChangeNoteFeed.Translation(itemId, note, Language);
			if (text != null)
			{
				return (Text: text, Translated: true);
			}
		}
		return (Text: note.Text, Translated: false);
	}

	internal static void Activate(ulong itemId)
	{
		switch (ViewFor(itemId))
		{
		case WorkshopChangeNoteTranslationView.Untranslated:
			if (!Busy)
			{
				ShowingOriginal.Remove(itemId);
				Runner?.Invoke(new _003C_003Ez__ReadOnlySingleElementList<ulong>(itemId));
			}
			break;
		case WorkshopChangeNoteTranslationView.Translated:
			ShowingOriginal.Add(itemId);
			Notify();
			break;
		case WorkshopChangeNoteTranslationView.Original:
			ShowingOriginal.Remove(itemId);
			Notify();
			break;
		case WorkshopChangeNoteTranslationView.Translating:
			break;
		}
	}

	internal static bool CanTranslateAny(IEnumerable<ulong> itemIds)
	{
		return itemIds.Any(delegate(ulong id)
		{
			WorkshopChangeNoteTranslationView workshopChangeNoteTranslationView = ViewFor(id);
			return (workshopChangeNoteTranslationView == WorkshopChangeNoteTranslationView.Untranslated || workshopChangeNoteTranslationView == WorkshopChangeNoteTranslationView.Original) ? true : false;
		});
	}

	internal static void TranslateAll(IReadOnlyList<ulong> itemIds)
	{
		if (!Busy)
		{
			List<ulong> list = itemIds.Where((ulong id) => ViewFor(id) == WorkshopChangeNoteTranslationView.Untranslated).ToList();
			ShowingOriginal.ExceptWith(itemIds);
			if (list.Count > 0)
			{
				Runner?.Invoke(list);
			}
			else
			{
				Notify();
			}
		}
	}

	internal static void Begin(IEnumerable<ulong> itemIds)
	{
		Running.UnionWith(itemIds);
		Notify();
	}

	internal static void End()
	{
		Running.Clear();
		Notify();
	}

	internal static void Reset()
	{
		Running.Clear();
		ShowingOriginal.Clear();
	}

	private static void Notify()
	{
		try
		{
			Changed?.Invoke();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} change-note translation UI refresh failed: {value}");
		}
	}
}

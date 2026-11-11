using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ModLaunchManager;

internal static class WorkshopChangeNoteText
{
	internal static string Summary(IReadOnlyList<WorkshopChangeNoteItem> updates)
	{
		int num = updates.Count((WorkshopChangeNoteItem item) => item.IsReading);
		int num2 = updates.Count((WorkshopChangeNoteItem item) => item.State == WorkshopChangeNoteState.Failed);
		if (num <= 0)
		{
			if (num2 <= 0)
			{
				return MlmLocManager.Get("changeNotes.summary.ready", ("count", updates.Count));
			}
			return MlmLocManager.Get("changeNotes.summary.partial", ("count", updates.Count), ("failed", num2));
		}
		return MlmLocManager.Get("changeNotes.summary.reading", ("count", updates.Count));
	}

	internal static string? StateMessage(WorkshopChangeNoteItem item)
	{
		switch (item.State)
		{
		case WorkshopChangeNoteState.Waiting:
		case WorkshopChangeNoteState.Loading:
			return MlmLocManager.Get("changeNotes.state.reading");
		case WorkshopChangeNoteState.Failed:
			return MlmLocManager.Get("changeNotes.state.failed");
		case WorkshopChangeNoteState.Private:
			return MlmLocManager.Get("changeNotes.state.private");
		default:
			return null;
		}
	}

	internal static void ConfigureTranslateButton(Button button, WorkshopChangeNoteTranslationView view, bool locked)
	{
		string item = WorkshopChangeNoteTranslations.LanguageName(WorkshopChangeNoteTranslations.Language);
		((CanvasItem)button).Visible = view != WorkshopChangeNoteTranslationView.None;
		var (key, key2) = view switch
		{
			WorkshopChangeNoteTranslationView.Translating => ("changeNotes.ai.translating", "changeNotes.ai.translate.tooltip"), 
			WorkshopChangeNoteTranslationView.Translated => ("changeNotes.ai.showOriginal", "changeNotes.ai.showOriginal.tooltip"), 
			WorkshopChangeNoteTranslationView.Original => ("changeNotes.ai.showTranslation", "changeNotes.ai.showTranslation.tooltip"), 
			_ => ("changeNotes.ai.translate", "changeNotes.ai.translate.tooltip"), 
		};
		button.Text = MlmLocManager.Get(key);
		((Control)button).TooltipText = MlmLocManager.Get(key2, ("language", item));
		((BaseButton)button).Disabled = locked || view == WorkshopChangeNoteTranslationView.Translating || (view == WorkshopChangeNoteTranslationView.Untranslated && WorkshopChangeNoteTranslations.Busy);
	}

	internal static Control NoteView(ulong itemId, WorkshopChangeNote note, int maxLength)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 3);
		string text = WorkshopStatus.FormatTime(note.Time);
		string text2 = ModErrorText.CurrentBranchName();
		if (note.ExcludesBranch(text2))
		{
			text = text + "  ·  " + MlmLocManager.Get("changeNotes.note.otherBranch", ("branches", string.Join(" / ", note.Branches)), ("current", text2));
		}
		(string Text, bool Translated) tuple = WorkshopChangeNoteTranslations.Display(itemId, note);
		var (text3, _) = tuple;
		if (tuple.Translated)
		{
			text = text + "  ·  " + MlmLocManager.Get("changeNotes.ai.marker");
		}
		((Node)val).AddChild((Node)(object)UiKit.Label(text, "CaptionLabel"), false, (InternalMode)0);
		Label val2 = UiKit.Label((text3.Length == 0) ? MlmLocManager.Get("changeNotes.note.empty") : ((text3.Length <= maxLength) ? text3 : (text3.Substring(0, maxLength).TrimEnd() + "…")), (note.Text.Length == 0) ? "DimLabel" : "MutedLabel");
		val2.AutowrapMode = (AutowrapMode)3;
		((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		return (Control)(object)val;
	}
}

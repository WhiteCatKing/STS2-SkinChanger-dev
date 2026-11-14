using Steamworks;
using System.Text.Json;
using System.Threading;

const uint AppId = 2868840;
const ulong PublishedFileId = 3787302680;
const string WorkshopRoot = @"C:\Users\Administrator\Desktop\工作2\STS2-SkinChanger_修改件\workshop";
const string ContentFolder = @"C:\Users\Administrator\Desktop\工作2\STS2_Work\workshop_content_1.0.20";
const string ChangeNote = "1.0.31.29";

// "english-only": refresh files, gallery and the default (English) text only. Used to isolate
// whether localized text is what Steam is rejecting; a second run without the flag applies all.
var englishOnly = args.Contains("english-only");
// "clean-previews": only drop the gallery entries, so the next run can add exactly the new set.
var cleanPreviewsOnly = args.Contains("clean-previews");

Environment.SetEnvironmentVariable("SteamAppId", AppId.ToString());
Environment.SetEnvironmentVariable("SteamGameId", AppId.ToString());

if (!SteamAPI.Init())
{
    Console.Error.WriteLine("SteamAPI.Init failed. Make sure Steam is running and the account owns the app.");
    return 3;
}

try
{
    if (!SteamUser.BLoggedOn())
    {
        Console.Error.WriteLine("Steam is not logged on.");
        return 4;
    }

    var workshopJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(WorkshopRoot, "workshop.json")));
    var localizations = JsonDocument.Parse(File.ReadAllText(Path.Combine(WorkshopRoot, "localizations.json")));
    var defaultTitle = workshopJson.RootElement.GetProperty("title").GetString() ?? "Skin Changer";
    var defaultDescription = workshopJson.RootElement.GetProperty("description").GetString() ?? string.Empty;
    var previewFiles = workshopJson.RootElement.GetProperty("previewFiles").EnumerateArray()
        .Select(entry => entry.GetString())
        .Where(path => !string.IsNullOrWhiteSpace(path))
        .Select(path => Path.Combine(WorkshopRoot, path!.Replace('/', Path.DirectorySeparatorChar)))
        .ToArray();

    var app = new AppId_t(AppId);
    var item = new PublishedFileId_t(PublishedFileId);

    // "content-only": replace the item's files and nothing else — no title, no description, no
    // gallery, no change note. This is the documented release path for a normal skin update.
    if (args.Contains("content-only"))
    {
        var contentUpdate = SteamUGC.StartItemUpdate(app, item);
        if (!SteamUGC.SetItemContent(contentUpdate, ContentFolder))
        {
            Console.Error.WriteLine("SetItemContent failed.");
            return 5;
        }

        Console.WriteLine($"CONTENT_FOLDER={ContentFolder}");
        return Submit(contentUpdate, "content-only");
    }

    // "note-only": publish the change note from workshop.json without touching content, text or
    // images. Used when the release notes are ready but the item itself is already up to date.
    if (args.Contains("note-only"))
    {
        var note = workshopJson.RootElement.TryGetProperty("changeNote", out var changeNoteElement)
            ? changeNoteElement.GetString() ?? string.Empty
            : string.Empty;
        if (string.IsNullOrWhiteSpace(note))
        {
            Console.Error.WriteLine("workshop.json has an empty changeNote.");
            return 5;
        }

        var noteUpdate = SteamUGC.StartItemUpdate(app, item);
        Console.WriteLine($"SUBMITTING_NOTE_LENGTH={note.Length}");
        return Submit(noteUpdate, "note-only", note);
    }

    // Steam keeps only the language that is active when SubmitItemUpdate is called, so localized
    // text has to go out one submission per language instead of one update with many switches.
    if (args.Contains("langs"))
    {
        foreach (var localization in localizations.RootElement.EnumerateArray())
        {
            var language = localization.GetProperty("language").GetString();
            var title = localization.GetProperty("title").GetString();
            var description = localization.GetProperty("description").GetString();
            if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description))
            {
                continue;
            }

            var perLanguage = SteamUGC.StartItemUpdate(app, item);
            if (!SteamUGC.SetItemUpdateLanguage(perLanguage, language) ||
                !SteamUGC.SetItemTitle(perLanguage, title) ||
                !SteamUGC.SetItemDescription(perLanguage, description))
            {
                Console.Error.WriteLine($"Set failed for {language}.");
                return 5;
            }

            var code = Submit(perLanguage, language);
            Console.WriteLine($"LANG_SUBMIT={language} code={code}");
            if (code != 0)
            {
                return code;
            }
        }

        return 0;
    }

    var update = SteamUGC.StartItemUpdate(app, item);

    if (!SteamUGC.SetItemContent(update, ContentFolder))
    {
        Console.Error.WriteLine("SetItemContent failed.");
        return 5;
    }

    // Gallery first: drop every preview (the main one is restored right after), then append the
    // new set in order so the page shows exactly these 12 images.
    // Steam's RemoveItemPreview reports success even for an out-of-range index, so the removal is
    // verified later by querying the item instead of trusting the return value.
    var removed = 0;
    if (cleanPreviewsOnly || !englishOnly)
    {
        // Index 0 is a real gallery entry on this item (the main image is separate), so the sweep
        // has to start at 0 or one stale image always survives.
        for (var index = 0; index <= 64; index++)
        {
            SteamUGC.RemoveItemPreview(update, (uint)index);
            removed++;
        }
    }

    Console.WriteLine($"REMOVED_PREVIEW_CALLS={removed}");

    if (cleanPreviewsOnly)
    {
        return Submit(update, "clean-previews");
    }

    var preview = Path.Combine(WorkshopRoot, "SkinChanger.jpg");
    Console.WriteLine($"MAIN_PREVIEW={preview} exists={File.Exists(preview)}");
    if (!SteamUGC.SetItemPreview(update, preview))
    {
        Console.Error.WriteLine("SetItemPreview failed.");
        return 5;
    }

    var added = 0;
    foreach (var file in previewFiles)
    {
        if (!File.Exists(file))
        {
            Console.Error.WriteLine($"Missing preview file: {file}");
            return 5;
        }

        if (!SteamUGC.AddItemPreviewFile(update, file, EItemPreviewType.k_EItemPreviewType_Image))
        {
            Console.Error.WriteLine($"AddItemPreviewFile failed: {file}");
            return 5;
        }

        added++;
    }

    Console.WriteLine($"ADDED_PREVIEWS={added}");

    // Default language slot: set title/description without selecting a language.
    if (!SteamUGC.SetItemTitle(update, defaultTitle))
    {
        Console.Error.WriteLine("SetItemTitle(default) failed.");
        return 5;
    }

    if (!SteamUGC.SetItemDescription(update, defaultDescription))
    {
        Console.Error.WriteLine("SetItemDescription(default) failed.");
        return 5;
    }

    var applied = new List<string> { "english(default)" };
    if (!englishOnly)
    {
        foreach (var localization in localizations.RootElement.EnumerateArray())
        {
            var language = localization.GetProperty("language").GetString();
            var title = localization.GetProperty("title").GetString();
            var description = localization.GetProperty("description").GetString();
            if (string.IsNullOrWhiteSpace(language) || string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(description))
            {
                continue;
            }

            SteamUGC.SetItemUpdateLanguage(update, language);
            if (!SteamUGC.SetItemTitle(update, title) ||
                !SteamUGC.SetItemDescription(update, description))
            {
                Console.Error.WriteLine($"Localized text failed for {language}.");
                return 5;
            }

            applied.Add(language);
        }
    }

    Console.WriteLine($"LANGUAGES={string.Join(",", applied)}");


    return Submit(update, englishOnly ? "english-only" : "full");
}
finally
{
    SteamAPI.Shutdown();
}

int Submit(UGCUpdateHandle_t update, string label, string? changeNote = null)
{
    var done = false;
    var result = EResult.k_EResultFail;
    var legalAgreementRequired = false;
    var callResult = CallResult<SubmitItemUpdateResult_t>.Create((callback, ioFailure) =>
    {
        result = callback.m_eResult;
        legalAgreementRequired = callback.m_bUserNeedsToAcceptWorkshopLegalAgreement;
        done = true;
        Console.WriteLine($"CALLBACK[{label}]: result={result} ioFailure={ioFailure} legal={legalAgreementRequired}");
    });

    var call = SteamUGC.SubmitItemUpdate(update, changeNote ?? ChangeNote);
    if (call == SteamAPICall_t.Invalid)
    {
        Console.Error.WriteLine("SubmitItemUpdate returned an invalid call handle.");
        return 6;
    }

    callResult.Set(call);
    var deadline = DateTime.UtcNow.AddMinutes(3);
    while (!done && DateTime.UtcNow < deadline)
    {
        SteamAPI.RunCallbacks();
        Thread.Sleep(50);
    }

    if (!done)
    {
        Console.Error.WriteLine("Timed out waiting for SubmitItemUpdate.");
        return 7;
    }

    Console.WriteLine($"PUBLISHED_FILE_ID={PublishedFileId}");
    Console.WriteLine($"RESULT={result}");
    Console.WriteLine($"LEGAL_AGREEMENT_REQUIRED={legalAgreementRequired}");
    return result == EResult.k_EResultOK ? 0 : 8;
}


using Steamworks;
using System.Threading;

const uint AppId = 2868840;
const ulong PublishedFileId = 3787302680;

Environment.SetEnvironmentVariable("SteamAppId", AppId.ToString());
Environment.SetEnvironmentVariable("SteamGameId", AppId.ToString());

if (!SteamAPI.Init())
{
    Console.Error.WriteLine("SteamAPI.Init failed");
    return 3;
}

try
{
    var language = args.Length > 0 ? args[0] : null;
    var query = SteamUGC.CreateQueryUGCDetailsRequest(
        [new PublishedFileId_t(PublishedFileId)], 1);
    if (!string.IsNullOrWhiteSpace(language))
    {
        SteamUGC.SetLanguage(query, language);
    }

    SteamUGC.SetReturnLongDescription(query, true);
    SteamUGC.SetReturnAdditionalPreviews(query, true);

    var done = false;
    var callResult = CallResult<SteamUGCQueryCompleted_t>.Create((callback, failure) =>
    {
        done = true;
        Console.WriteLine($"QUERY_CALLBACK result={callback.m_eResult} failure={failure}");
    });
    var call = SteamUGC.SendQueryUGCRequest(query);
    callResult.Set(call);
    var deadline = DateTime.UtcNow.AddMinutes(1);
    while (!done && DateTime.UtcNow < deadline)
    {
        SteamAPI.RunCallbacks();
        Thread.Sleep(50);
    }

    if (!done)
    {
        Console.Error.WriteLine("timeout");
        return 4;
    }

    if (!SteamUGC.GetQueryUGCResult(query, 0, out var details))
    {
        Console.Error.WriteLine("GetQueryUGCResult failed");
        return 5;
    }

    var description = details.m_rgchDescription ?? string.Empty;
    Console.WriteLine($"LANG={language ?? "client"}");
    Console.WriteLine($"TITLE={details.m_rgchTitle}");
    Console.WriteLine($"FILE_SIZE={details.m_nFileSize}");
    Console.WriteLine($"TIME_UPDATED={details.m_rtimeUpdated} ({DateTimeOffset.FromUnixTimeSeconds(details.m_rtimeUpdated):yyyy-MM-dd HH:mm:ss zzz})");
    Console.WriteLine($"VISIBILITY={details.m_eVisibility}");
    Console.WriteLine($"DESCRIPTION_LENGTH={description.Length}");
    Console.WriteLine($"DESCRIPTION_HEAD={description.Substring(0, Math.Min(160, description.Length)).Replace('\n', ' ')}");
    Console.WriteLine($"DESCRIPTION_TAIL={description.Substring(Math.Max(0, description.Length - 80)).Replace('\n', ' ')}");
    if (SteamUGC.GetQueryUGCPreviewURL(query, 0, out var previewUrl, 1024))
    {
        Console.WriteLine($"MAIN_PREVIEW={previewUrl}");
    }

    var additional = SteamUGC.GetQueryUGCNumAdditionalPreviews(query, 0);
    Console.WriteLine($"ADDITIONAL_PREVIEWS={additional}");
    for (uint index = 0; index < additional; index++)
    {
        if (SteamUGC.GetQueryUGCAdditionalPreview(query, 0, index, out var url, 1024, out var original, 1024, out var type))
        {
            Console.WriteLine($"PREVIEW[{index}] type={type} url={url}");
        }
    }

    SteamUGC.ReleaseQueryUGCRequest(query);
    return 0;
}
finally
{
    SteamAPI.Shutdown();
}

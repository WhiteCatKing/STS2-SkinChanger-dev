using Steamworks;
using System.Threading;

// Subscribes to one Workshop item through the running Steam client and waits until the client has
// downloaded it. The Steam client stores it under steamapps/workshop/content/<appid>/<itemId>, which
// is where this tool prints the path.
const uint AppId = 2868840;
const ulong DefaultItem = 3747679239;

var itemId = args.Length > 0 && ulong.TryParse(args[0], out var parsed) ? parsed : DefaultItem;
var timeoutMinutes = args.Length > 1 && int.TryParse(args[1], out var minutes) ? minutes : 8;

Environment.SetEnvironmentVariable("SteamAppId", AppId.ToString());
Environment.SetEnvironmentVariable("SteamGameId", AppId.ToString());

if (!SteamAPI.Init())
{
    Console.Error.WriteLine("SteamAPI.Init failed. Is Steam running and the account logged in?");
    return 3;
}

try
{
    if (!SteamUser.BLoggedOn())
    {
        Console.Error.WriteLine("Steam is not logged on.");
        return 4;
    }

    var app = new AppId_t(AppId);
    var item = new PublishedFileId_t(itemId);
    if (args.Length > 1 && args[0] == "--search")
    {
        var searchText = string.Join(' ', args.Skip(1));
        var search = SteamUGC.CreateQueryAllUGCRequest(
            EUGCQuery.k_EUGCQuery_RankedByTextSearch,
            EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items,
            app,
            app,
            1);
        SteamUGC.SetSearchText(search, searchText);
        SteamUGC.SetReturnLongDescription(search, false);
        var searchDone = false;
        var searchResult = EResult.k_EResultFail;
        var searchResultHandle = CallResult<SteamUGCQueryCompleted_t>.Create((callback, failure) =>
        {
            searchResult = callback.m_eResult;
            Console.WriteLine($"SEARCH_RESULT={callback.m_eResult} total={callback.m_unTotalMatchingResults}");
            for (uint index = 0; index < callback.m_unNumResultsReturned; index++)
            {
                if (SteamUGC.GetQueryUGCResult(search, index, out var details))
                {
                    Console.WriteLine($"ITEM {details.m_nPublishedFileId.m_PublishedFileId} :: {details.m_rgchTitle}");
                }
            }

            searchDone = true;
        });
        searchResultHandle.Set(SteamUGC.SendQueryUGCRequest(search));
        var searchDeadline = DateTime.UtcNow.AddSeconds(60);
        while (!searchDone && DateTime.UtcNow < searchDeadline)
        {
            SteamAPI.RunCallbacks();
            Thread.Sleep(50);
        }

        SteamUGC.ReleaseQueryUGCRequest(search);
        return searchDone && searchResult == EResult.k_EResultOK ? 0 : 6;
    }

    if (args.Contains("--unsubscribe"))
    {
        Console.WriteLine($"UNSUBSCRIBE item={itemId}");
        SteamUGC.UnsubscribeItem(item);
        var until = DateTime.UtcNow.AddSeconds(20);
        while (DateTime.UtcNow < until)
        {
            SteamAPI.RunCallbacks();
            Thread.Sleep(200);
        }

        Console.WriteLine($"STATE={SteamUGC.GetItemState(item)}");
        return 0;
    }

    Console.WriteLine($"SUBSCRIBE app={AppId} item={itemId}");
    SteamUGC.SubscribeItem(item);

    var deadline = DateTime.UtcNow.AddMinutes(timeoutMinutes);
    var lastState = uint.MaxValue;
    var installed = false;
    while (DateTime.UtcNow < deadline)
    {
        SteamAPI.RunCallbacks();
        var state = SteamUGC.GetItemState(item);
        if (state != lastState)
        {
            Console.WriteLine($"STATE={state}");
            lastState = state;
        }

        var isInstalled = (state & (uint)EItemState.k_EItemStateInstalled) != 0;
        var needsUpdate = (state & (uint)EItemState.k_EItemStateNeedsUpdate) != 0;
        if (isInstalled && !needsUpdate)
        {
            installed = true;
            break;
        }

        Thread.Sleep(1000);
    }

    if (!installed)
    {
        Console.Error.WriteLine($"Item {itemId} was not installed within {timeoutMinutes} minutes (state={lastState}).");
        return 5;
    }

    if (SteamUGC.GetItemInstallInfo(item, out var sizeOnDisk, out var folder, 1024, out var timeStamp))
    {
        Console.WriteLine($"INSTALL_FOLDER={folder}");
        Console.WriteLine($"SIZE_ON_DISK={sizeOnDisk}");
        Console.WriteLine($"TIME_STAMP={DateTimeOffset.FromUnixTimeSeconds(timeStamp):yyyy-MM-dd HH:mm:ss zzz}");
    }

    if (SteamUGC.GetItemDownloadInfo(item, out var downloaded, out var total) && total > 0)
    {
        Console.WriteLine($"DOWNLOADED={downloaded}/{total}");
    }

    Console.WriteLine("DONE");
    return 0;
}
finally
{
    SteamAPI.Shutdown();
}

using System.IO;
using Godot;
using STS2SkinChanger.Core;
using Steamworks;

namespace STS2SkinChanger.Ui;

internal static class WorkshopCommunityLinks
{
	public static void OpenItem(ulong id)
	{
		Open(WorkshopItemActions.ItemUrl(id, client: false), WorkshopItemActions.ItemUrl(id, client: true));
	}

	public static void OpenDiscussion(bool presets)
	{
		Open(WorkshopCommunityPolicy.DiscussionUrl(presets, client: false), WorkshopCommunityPolicy.DiscussionUrl(presets, client: true));
	}

	public static void OpenSubmissionSource(string url)
	{
		if (!WorkshopCodeDiagnostics.SafeSource(url))
		{
			throw new InvalidDataException("Invalid submission source.");
		}
		Open(url, "steam://openurl/" + url);
	}

	private static void Open(string webUrl, string clientUrl)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (SteamUtils.IsOverlayEnabled())
		{
			SteamFriends.ActivateGameOverlayToWebPage(webUrl, (EActivateGameOverlayToWebPageMode)0);
		}
		else if ((int)OS.ShellOpen(clientUrl) != 0)
		{
			throw new IOException(WorkshopCommunityText.Get(WorkshopCommunityTextKey.OpenFailed));
		}
	}
}

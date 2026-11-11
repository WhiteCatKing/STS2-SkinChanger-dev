namespace STS2SkinChanger.Core;

internal static class WorkshopCommunityPolicy
{
	public static string DiscussionUrl(bool presets, bool client)
	{
		string text = "https://steamcommunity.com/workshop/filedetails/discussion/3787302680/" + (presets ? "592940620292746467" : "592940620292752301");
		if (!client)
		{
			return text;
		}
		return "steam://openurl/" + text;
	}
}

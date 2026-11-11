namespace STS2SkinChanger.Ui;

internal static class CardPresentationLayoutPolicy
{
	public static CardPresentationLayout Resolve(bool isNativeAncient, bool requestsAncientLayout, bool requestsExpandedPortrait)
	{
		if (isNativeAncient || requestsAncientLayout)
		{
			return CardPresentationLayout.Ancient;
		}
		if (!requestsExpandedPortrait)
		{
			return CardPresentationLayout.Normal;
		}
		return CardPresentationLayout.ExpandedPortrait;
	}
}

using System.Linq;

namespace STS2SkinChanger.Catalog;

internal static class LooseCardPortraitPackPolicy
{
	public const int MinimumCards = 20;

	public const int MinimumMatchPercent = 80;

	public static bool IsPack(int imageCount, int matchedCards)
	{
		if (imageCount > 0 && matchedCards >= 20)
		{
			return matchedCards * 100 >= imageCount * 80;
		}
		return false;
	}

	public static string SanitizePathToken(string id)
	{
		string text = new string(id.Select((char character) => (!char.IsLetterOrDigit(character) && character != '_') ? '_' : character).ToArray());
		if (text.Length != 0 && !char.IsDigit(text[0]))
		{
			return text;
		}
		return "_" + text;
	}
}

using System;
using System.Security.Cryptography;
using System.Text;

namespace STS2SkinChanger.Core;

internal static class ExternalCardProviderIdentityPolicy
{
	public static ExternalCardVisualOwnershipState ResolveEditorOwnership(bool hasOverride, bool fullArt, bool ancientTextOutside)
	{
		if (!hasOverride)
		{
			return default(ExternalCardVisualOwnershipState);
		}
		return new ExternalCardVisualOwnershipState(Portrait: true, fullArt, fullArt || ancientTextOutside);
	}

	public static string BuildSyntheticPath(string cardId, string stableKey)
	{
		string value = NormalizeCardId(cardId);
		byte[] array = SHA256.HashData(Encoding.UTF8.GetBytes(stableKey));
		return $"user://skin_changer/card_provider/{value}_{Convert.ToHexString(array.AsSpan(0, 8)).ToLowerInvariant()}.png";
	}

	private static string NormalizeCardId(string cardId)
	{
		StringBuilder stringBuilder = new StringBuilder(cardId.Length);
		bool flag = false;
		string text = cardId.ToLowerInvariant();
		foreach (char c in text)
		{
			bool flag2;
			switch (c)
			{
			case '0':
			case '1':
			case '2':
			case '3':
			case '4':
			case '5':
			case '6':
			case '7':
			case '8':
			case '9':
			case 'a':
			case 'b':
			case 'c':
			case 'd':
			case 'e':
			case 'f':
			case 'g':
			case 'h':
			case 'i':
			case 'j':
			case 'k':
			case 'l':
			case 'm':
			case 'n':
			case 'o':
			case 'p':
			case 'q':
			case 'r':
			case 's':
			case 't':
			case 'u':
			case 'v':
			case 'w':
			case 'x':
			case 'y':
			case 'z':
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			if (flag2)
			{
				stringBuilder.Append(c);
				flag = false;
			}
			else if (!flag && stringBuilder.Length > 0)
			{
				stringBuilder.Append('_');
				flag = true;
			}
		}
		string text2 = stringBuilder.ToString().Trim('_');
		if (text2.Length != 0)
		{
			return text2;
		}
		return "card";
	}
}

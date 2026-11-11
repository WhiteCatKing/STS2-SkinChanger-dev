using System;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class PartTakeoverFeedbackScopePolicy
{
	private static readonly string[] AudioTokens = new string[6] { "Sfx", "Sound", "Audio", "Voice", "Music", "Bus" };

	private static readonly string[] ShopTokens = new string[2] { "Merchant", "Shop" };

	private static readonly string[] ReactionNames = new string[10] { "Open", "Close", "Release", "Press", "Click", "Interact", "Interaction", "Dialogue", "Speak", "Talk" };

	private static readonly string[] ReactionEndings = new string[14]
	{
		"Thrown", "Opened", "Closed", "Pressed", "Released", "Clicked", "Purchased", "Selected", "Used", "Failed",
		"Completed", "Finished", "Started", "Ended"
	};

	internal static bool IsMerchantSelectionFeedback(string? targetTypeName, string? targetMethodName, string? patchTypeName)
	{
		if (!ContainsAny(targetTypeName, ShopTokens) && !ContainsAny(patchTypeName, ShopTokens))
		{
			return false;
		}
		if (ContainsAny(targetTypeName, AudioTokens) || ContainsAny(targetMethodName, AudioTokens))
		{
			return true;
		}
		if (string.IsNullOrWhiteSpace(targetMethodName))
		{
			return false;
		}
		if (!targetMethodName.StartsWith("On", StringComparison.Ordinal) && !ReactionNames.Contains<string>(targetMethodName, StringComparer.Ordinal))
		{
			return ReactionEndings.Any((string ending) => targetMethodName.EndsWith(ending, StringComparison.Ordinal));
		}
		return true;
	}

	private static bool ContainsAny(string? value, string[] tokens)
	{
		if (value != null)
		{
			return tokens.Any((string token) => value.Contains(token, StringComparison.OrdinalIgnoreCase));
		}
		return false;
	}
}

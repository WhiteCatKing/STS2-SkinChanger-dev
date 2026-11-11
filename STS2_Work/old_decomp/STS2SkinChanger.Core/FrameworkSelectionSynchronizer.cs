using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class FrameworkSelectionSynchronizer
{
	public static int Synchronize<TCharacter>(IEnumerable<TCharacter> registeredCharacters, Func<TCharacter, string> resolveGroupId, Func<string, string?> resolveSkinId, Action<TCharacter, string> setActiveSkin)
	{
		int num = 0;
		foreach (TCharacter registeredCharacter in registeredCharacters)
		{
			string arg = resolveGroupId(registeredCharacter);
			string text = resolveSkinId(arg);
			setActiveSkin(registeredCharacter, string.IsNullOrWhiteSpace(text) ? "default" : text);
			num++;
		}
		return num;
	}
}

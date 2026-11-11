using System.Collections.Generic;
using CznCore.Presentation;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinCharacterIdentityProfile
{
	internal const string LocalizationPrefix = "MEIRIN";

	internal static readonly CharacterSelectIdentityProfile CharacterSelect = new CharacterSelectIdentityProfile("MEIRIN.title", "MEIRIN.description", (IReadOnlyDictionary<CharacterSelectBackgroundVariant, string>)new Dictionary<CharacterSelectBackgroundVariant, string>
	{
		[(CharacterSelectBackgroundVariant)0] = "res://MeirinWatcherSkin/audio/voice/vo_1027_gacha_01.ogg",
		[(CharacterSelectBackgroundVariant)2] = "res://MeirinWatcherSkin/audio/voice/vo_1027_team_join_01.ogg",
		[(CharacterSelectBackgroundVariant)1] = "res://MeirinWatcherSkin/audio/voice/vo_1027_lobby_enter_01.ogg"
	});
}

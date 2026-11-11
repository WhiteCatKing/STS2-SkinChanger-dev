using System;
using System.Collections.Generic;
using CznCore.Presentation;

namespace CznCore.Settings;

internal sealed class CharacterSettingsUiRuntime : SkinSettingsUiRuntimeBase
{
	private static readonly IReadOnlyList<CharacterSelectBackgroundVariant> BackgroundVariants = Array.AsReadOnly(new CharacterSelectBackgroundVariant[3]
	{
		CharacterSelectBackgroundVariant.StaticPortrait,
		CharacterSelectBackgroundVariant.TeamJoinPortrait,
		CharacterSelectBackgroundVariant.LiveLobby
	});

	public override string CharacterSettingsKey => profile.Key;

	public override string CharacterSettingsHeader => profile.Header.Current;

	public override string CharacterSkinTarget => profile.Target.Current;

	public override string CharacterSettingsAvatarPath => profile.AvatarPath;

	public override string CharacterSettingsAccentColor => profile.AccentColor;

	public override string GlobalSettingsIconPath => profile.GlobalIconPath;

	public override string CharacterEffectsLabel => SharedSettingsText.EffectsEnabled.Current;

	public override string CharacterVoiceLabel => SharedSettingsText.VoiceEnabled.Current;

	public override bool SupportsMeleeApproachMovement => profile.SupportsMeleeApproachMovement;

	public override bool SupportsOfficialAttackTiming => profile.SupportsOfficialAttackTiming;

	public override IReadOnlyList<CharacterSelectBackgroundVariant> SupportedCharacterSelectBackgroundVariants => BackgroundVariants;

	public CharacterSettingsUiRuntime(CharacterSettingsUiProfile profile, SkinCharacterSettings settings, SkinSettingsUiRuntimeHooks hooks)
		: base(settings, hooks)
	{
	}

	protected override CharacterSelectBackgroundVariant NormalizeBackgroundVariant(CharacterSelectBackgroundVariant variant)
	{
		if ((uint)(variant - 1) > 1u)
		{
			return CharacterSelectBackgroundVariant.StaticPortrait;
		}
		return variant;
	}
}

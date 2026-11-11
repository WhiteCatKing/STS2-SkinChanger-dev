using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CznCore.Settings;
using Godot;

namespace CznCore.Presentation;

internal static class CharacterSelectCommonOptions
{
	public const string CombatIcon = "res://CznCore/images/ui/icon_large_battle_ui_48.png";

	public const string BackgroundIcon = "res://CznCore/images/ui/icon_contents_lobby_mode_live_event_ui_48.png";

	public const string EffectsIcon = "res://CznCore/images/ui/icon_comm_buff_eff.png";

	public const string VoiceIcon = "res://CznCore/images/ui/icon_voice.png";

	public static List<CharacterSelectMenuItemDescriptor> CreateItems(SkinCharacterSettings settings, IReadOnlyList<CharacterSelectBackgroundVariant> backgrounds, Action<CharacterSelectBackgroundVariant> applyBackground, Action<bool>? applyVoice = null)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		SkinSettingsUiTextProfile text = SharedSettingsText.Ui;
		int num = 4;
		List<CharacterSelectMenuItemDescriptor> list = new List<CharacterSelectMenuItemDescriptor>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<CharacterSelectMenuItemDescriptor> span = CollectionsMarshal.AsSpan(list);
		span[0] = new CharacterSelectMenuActionDescriptor("character_background", "res://CznCore/images/ui/icon_contents_lobby_mode_live_event_ui_48.png", () => text.CharacterSelectBackgroundToolbarTitle.Current, () => settings.CharacterSelectBackgroundVariant switch
		{
			CharacterSelectBackgroundVariant.TeamJoinPortrait => text.CharacterSelectBackgroundTeamJoinPortrait.Current, 
			CharacterSelectBackgroundVariant.LiveLobby => text.CharacterSelectBackgroundLiveLobby.Current, 
			_ => text.CharacterSelectBackgroundStaticPortrait.Current, 
		}, () => SharedSettingsText.SwitchBackground.Current, delegate
		{
			int num2 = IndexOf(backgrounds, settings.CharacterSelectBackgroundVariant);
			CharacterSelectBackgroundVariant characterSelectBackgroundVariant = backgrounds[(num2 + 1) % backgrounds.Count];
			settings.SetCharacterSelectBackgroundVariant(characterSelectBackgroundVariant, persist: true);
			applyBackground(characterSelectBackgroundVariant);
		})
		{
			Accent = new Color("397fba")
		};
		span[1] = new CharacterSelectMenuActionDescriptor("combat_presentation", "res://CznCore/images/ui/icon_large_battle_ui_48.png", () => text.CharacterSelectCombatToolbarTitle.Current, () => settings.CombatPresentationMode switch
		{
			CombatPresentationMode.Fast => text.CombatPresentationFast.Current, 
			CombatPresentationMode.Full => text.CombatPresentationFull.Current, 
			_ => text.CombatPresentationCustom.Current, 
		}, () => text.CombatPresentationModeDescription.Current, delegate
		{
			SkinCharacterSettings skinCharacterSettings = settings;
			skinCharacterSettings.SetCombatPresentationMode(settings.CombatPresentationMode switch
			{
				CombatPresentationMode.Fast => CombatPresentationMode.Full, 
				CombatPresentationMode.Full => CombatPresentationMode.Custom, 
				_ => CombatPresentationMode.Fast, 
			}, persist: true);
		})
		{
			Accent = new Color("c67b12")
		};
		span[2] = new CharacterSelectMenuToggleDescriptor("combat_effects", "res://CznCore/images/ui/icon_comm_buff_eff.png", () => SharedSettingsText.EffectsEnabled.Current, () => text.EffectsEnabledDescription.Current, () => settings.EffectsEnabled, delegate(bool value)
		{
			settings.SetEffectsEnabled(value, persist: true);
		})
		{
			Accent = new Color("338c80")
		};
		span[3] = new CharacterSelectMenuToggleDescriptor("character_voice", "res://CznCore/images/ui/icon_voice.png", () => SharedSettingsText.VoiceEnabled.Current, () => SharedSettingsText.VoiceDescription.Current, () => settings.VoiceEnabled, delegate(bool value)
		{
			settings.SetVoiceEnabled(value, persist: true);
			applyVoice?.Invoke(value);
		})
		{
			Accent = new Color("8270a3")
		};
		return list;
	}

	private static int IndexOf(IReadOnlyList<CharacterSelectBackgroundVariant> variants, CharacterSelectBackgroundVariant value)
	{
		for (int i = 0; i < variants.Count; i++)
		{
			if (variants[i] == value)
			{
				return i;
			}
		}
		return -1;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record CharacterSelectPresentationProfile(string LiveNodeName, string OriginalBgNodeName, string CustomBgNodeName, string ToggleButtonNodeName, string StaticScenePath, string LiveScenePath, string LoopAnimationName, Vector2 ToggleButtonMinimumSize, Vector2 ToggleButtonPosition, IReadOnlyList<CharacterSelectBackgroundPresentationProfile>? BackgroundOptions = null, string CombatPresentationIconPath = "", string CharacterBackgroundIconPath = "", string SettingsAvatarPath = "", string MapMarkerIconPath = "", string MainMenuThumbnailPath = "", string LobbyMusicEventPath = "")
{
	public string CharacterSelectIconPath { get; init; } = "";

	public string CharacterSelectIconBackgroundPath { get; init; } = "";

	public string CharacterIconPath { get; init; } = "";

	public string OutfitPreviewAnimation { get; init; } = "";

	public string DescriptionLocKey { get; init; } = "";

	public string EffectiveMapMarkerIconPath
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(MapMarkerIconPath))
			{
				return MapMarkerIconPath;
			}
			return SettingsAvatarPath;
		}
	}

	public string LiveLobbyScenePath
	{
		get
		{
			IReadOnlyList<CharacterSelectBackgroundPresentationProfile> supportedBackgroundOptions = SupportedBackgroundOptions;
			for (int i = 0; i < supportedBackgroundOptions.Count; i++)
			{
				if (supportedBackgroundOptions[i].Variant == CharacterSelectBackgroundVariant.LiveLobby && !string.IsNullOrWhiteSpace(supportedBackgroundOptions[i].ScenePath))
				{
					return supportedBackgroundOptions[i].ScenePath;
				}
			}
			return LiveScenePath;
		}
	}

	public IReadOnlyList<CharacterSelectBackgroundPresentationProfile> SupportedBackgroundOptions
	{
		get
		{
			IReadOnlyList<CharacterSelectBackgroundPresentationProfile> backgroundOptions = BackgroundOptions;
			if (backgroundOptions != null && backgroundOptions.Count > 0)
			{
				return BackgroundOptions;
			}
			return new _003C_003Ez__ReadOnlyArray<CharacterSelectBackgroundPresentationProfile>(new CharacterSelectBackgroundPresentationProfile[2]
			{
				new CharacterSelectBackgroundPresentationProfile(CharacterSelectBackgroundVariant.StaticPortrait, StaticScenePath),
				new CharacterSelectBackgroundPresentationProfile(CharacterSelectBackgroundVariant.LiveLobby, LiveScenePath, ForceLoopAnimation: true)
			});
		}
	}

	public IReadOnlyList<CharacterSelectBackgroundVariant> SupportedBackgroundVariants
	{
		get
		{
			IReadOnlyList<CharacterSelectBackgroundPresentationProfile> supportedBackgroundOptions = SupportedBackgroundOptions;
			CharacterSelectBackgroundVariant[] array = new CharacterSelectBackgroundVariant[supportedBackgroundOptions.Count];
			for (int i = 0; i < supportedBackgroundOptions.Count; i++)
			{
				array[i] = supportedBackgroundOptions[i].Variant;
			}
			return array;
		}
	}

	public CharacterSelectPresentationProfile WithBackground(CharacterSelectBackgroundVariant variant, string scenePath)
	{
		IReadOnlyList<CharacterSelectBackgroundPresentationProfile> supportedBackgroundOptions = SupportedBackgroundOptions;
		if (!supportedBackgroundOptions.Any((CharacterSelectBackgroundPresentationProfile o) => o.Variant == variant))
		{
			throw new ArgumentException("No background option to replace: " + variant);
		}
		return this with
		{
			BackgroundOptions = supportedBackgroundOptions.Select((CharacterSelectBackgroundPresentationProfile o) => (o.Variant != variant) ? o : o with
			{
				ScenePath = scenePath
			}).ToArray()
		};
	}

	public CharacterSelectBackgroundPresentationProfile GetBackgroundOptionOrDefault(CharacterSelectBackgroundVariant variant)
	{
		IReadOnlyList<CharacterSelectBackgroundPresentationProfile> supportedBackgroundOptions = SupportedBackgroundOptions;
		for (int i = 0; i < supportedBackgroundOptions.Count; i++)
		{
			if (supportedBackgroundOptions[i].Variant == variant)
			{
				return supportedBackgroundOptions[i];
			}
		}
		if (supportedBackgroundOptions.Count > 0)
		{
			return supportedBackgroundOptions[0];
		}
		throw new InvalidOperationException("Character select background profile has no supported options.");
	}

	public CharacterSelectBackgroundVariant NormalizeVariant(CharacterSelectBackgroundVariant variant)
	{
		return GetBackgroundOptionOrDefault(variant).Variant;
	}

	public CharacterSelectBackgroundVariant NextVariant(CharacterSelectBackgroundVariant current)
	{
		IReadOnlyList<CharacterSelectBackgroundPresentationProfile> supportedBackgroundOptions = SupportedBackgroundOptions;
		if (supportedBackgroundOptions.Count == 0)
		{
			return CharacterSelectBackgroundVariant.StaticPortrait;
		}
		for (int i = 0; i < supportedBackgroundOptions.Count; i++)
		{
			if (supportedBackgroundOptions[i].Variant == current)
			{
				return supportedBackgroundOptions[(i + 1) % supportedBackgroundOptions.Count].Variant;
			}
		}
		return supportedBackgroundOptions[0].Variant;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("LiveNodeName = ");
		builder.Append((object?)LiveNodeName);
		builder.Append(", OriginalBgNodeName = ");
		builder.Append((object?)OriginalBgNodeName);
		builder.Append(", CustomBgNodeName = ");
		builder.Append((object?)CustomBgNodeName);
		builder.Append(", ToggleButtonNodeName = ");
		builder.Append((object?)ToggleButtonNodeName);
		builder.Append(", StaticScenePath = ");
		builder.Append((object?)StaticScenePath);
		builder.Append(", LiveScenePath = ");
		builder.Append((object?)LiveScenePath);
		builder.Append(", LoopAnimationName = ");
		builder.Append((object?)LoopAnimationName);
		builder.Append(", ToggleButtonMinimumSize = ");
		Vector2 val = ToggleButtonMinimumSize;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", ToggleButtonPosition = ");
		val = ToggleButtonPosition;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", BackgroundOptions = ");
		builder.Append(BackgroundOptions);
		builder.Append(", CombatPresentationIconPath = ");
		builder.Append((object?)CombatPresentationIconPath);
		builder.Append(", CharacterBackgroundIconPath = ");
		builder.Append((object?)CharacterBackgroundIconPath);
		builder.Append(", SettingsAvatarPath = ");
		builder.Append((object?)SettingsAvatarPath);
		builder.Append(", MapMarkerIconPath = ");
		builder.Append((object?)MapMarkerIconPath);
		builder.Append(", MainMenuThumbnailPath = ");
		builder.Append((object?)MainMenuThumbnailPath);
		builder.Append(", LobbyMusicEventPath = ");
		builder.Append((object?)LobbyMusicEventPath);
		builder.Append(", CharacterSelectIconPath = ");
		builder.Append((object?)CharacterSelectIconPath);
		builder.Append(", CharacterSelectIconBackgroundPath = ");
		builder.Append((object?)CharacterSelectIconBackgroundPath);
		builder.Append(", CharacterIconPath = ");
		builder.Append((object?)CharacterIconPath);
		builder.Append(", OutfitPreviewAnimation = ");
		builder.Append((object?)OutfitPreviewAnimation);
		builder.Append(", DescriptionLocKey = ");
		builder.Append((object?)DescriptionLocKey);
		builder.Append(", EffectiveMapMarkerIconPath = ");
		builder.Append((object?)EffectiveMapMarkerIconPath);
		builder.Append(", LiveLobbyScenePath = ");
		builder.Append((object?)LiveLobbyScenePath);
		builder.Append(", SupportedBackgroundOptions = ");
		builder.Append(SupportedBackgroundOptions);
		builder.Append(", SupportedBackgroundVariants = ");
		builder.Append(SupportedBackgroundVariants);
		return true;
	}
}

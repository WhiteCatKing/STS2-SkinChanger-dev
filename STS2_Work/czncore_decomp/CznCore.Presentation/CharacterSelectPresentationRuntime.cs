using System;
using System.Collections.Generic;
using System.Linq;
using CznCore.Godot;
using CznCore.Settings;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace CznCore.Presentation;

internal sealed class CharacterSelectPresentationRuntime
{
	private const string LoadedSceneMeta = "czn_character_select_loaded_scene";

	private const string LoadedSceneInstanceMeta = "czn_character_select_loaded_scene_instance";

	private const string NativeHostOwnerMeta = "czn_character_select_native_host_owner";

	private const string NativeHostPositionMeta = "czn_character_select_native_host_position";

	private const string NativeHostSizeMeta = "czn_character_select_native_host_size";

	private const string NativeHostScaleMeta = "czn_character_select_native_host_scale";

	private const string NativeHostPivotMeta = "czn_character_select_native_host_pivot";

	private const string ToolbarPanelNodeNameSuffix = "CharacterSelectOptionsPanel";

	private const string ToolbarButtonsRootNodeName = "CharSelectButtons";

	private readonly CharacterSkinProfile _profile;

	private NCharacterSelectButton? _lastSelectionButton;

	private readonly SkinCharacterSettings _settings;

	private readonly CharacterSkinVoicePlayerRuntime? _voicePlayer;

	private readonly ILocalizedText? _switchBackgroundText;

	private readonly SkinSettingsUiTextProfile? _settingsText;

	private readonly Func<Node, string, bool>? _tryPrepareLoopSpine;

	private readonly Func<Player?>? _voiceOwner;

	private readonly Func<IReadOnlyList<CharacterSelectMenuSectionDescriptor>>? _additionalMenuSections;

	private CharacterSelectBackgroundVariant _backgroundVariant;

	private CharacterSelectPresentationProfile Presentation => _profile.For(null).CharacterSelectPresentation;

	private string ToolbarPanelNodeName => _profile.RuntimeNodeName("CharacterSelectOptionsPanel");

	private static string OutfitLabel(Type slot)
	{
		return CharacterAppearanceCatalog.Label(slot);
	}

	public CharacterSelectPresentationRuntime(CharacterSkinProfile profile, SkinCharacterSettings settings, CharacterSkinVoicePlayerRuntime? voicePlayer = null, CharacterSelectPresentationRuntimeOptions? options = null)
	{
		_profile = profile ?? throw new ArgumentNullException("profile");
		_settings = settings ?? throw new ArgumentNullException("settings");
		_voicePlayer = voicePlayer;
		_switchBackgroundText = options?.SwitchBackgroundText;
		_settingsText = options?.SettingsText;
		_tryPrepareLoopSpine = options?.TryPrepareLoopSpine;
		_voiceOwner = options?.VoiceOwner;
		_additionalMenuSections = options?.AdditionalMenuSections;
	}

	public void ApplyScreenSelection(NCharacterSelectScreen screen, NCharacterSelectButton button, CharacterModel characterModel)
	{
		_lastSelectionButton = button;
		SkinPatchGuard.Run("Character select background patch", delegate
		{
			CharacterSelectPresentationProfile presentation = Presentation;
			RemoveLegacyCharacterSelectControls(screen);
			Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(presentation.OriginalBgNodeName));
			Control val = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(presentation.CustomBgNodeName));
			if (button.IsLocked || !_profile.AppliesTo(characterModel))
			{
				RemoveOutfitSelector((Node)(object)screen);
				CharacterSelectSettingsMenu.RemoveOwned((Control)(object)screen, _profile.ProfileId);
				if (val != null)
				{
					SetVisible((CanvasItem?)(object)val, visible: false);
					GodotNodeUtil.ClearChildren((Node)(object)val);
					RemoveLoadedSceneMeta((Node)(object)val);
				}
				if (nodeOrNull != null && !AnyOtherCustomBgVisible(screen, presentation.CustomBgNodeName))
				{
					RestoreNativeCharacterSelectHost(nodeOrNull);
					SetVisible((CanvasItem?)(object)nodeOrNull, visible: true);
				}
			}
			else
			{
				SetCharacterSelectBackgroundVariant(presentation.NormalizeVariant(_settings.CharacterSelectBackgroundVariant), persist: false);
				TextureRect nodeOrNull2 = ((Node)button).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Icon"));
				if (nodeOrNull2 != null)
				{
					nodeOrNull2.Texture = (Texture2D)(object)characterModel.CharacterSelectIcon;
					TextureRect nodeOrNull3 = ((Node)button).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%IconAdd"));
					if (nodeOrNull3 != null)
					{
						nodeOrNull3.Texture = nodeOrNull2.Texture;
					}
				}
				if (val == null)
				{
					val = CreateCustomBgContainer(screen, nodeOrNull, presentation.CustomBgNodeName);
				}
				CharacterSelectIconBackground.Apply(button, presentation);
				ApplyCharacterSelectBackgroundMode(val, nodeOrNull, characterModel, "character_select");
				EnsureCharacterSelectMenu(screen, val, nodeOrNull, characterModel);
			}
		});
	}

	public void ApplyMultiplayerLoadBackground(NMultiplayerLoadGameScreen screen)
	{
		SkinPatchGuard.Run("Multiplayer load character select background patch", delegate
		{
			if (TryGetMultiplayerLoadLocalCharacter(screen, out CharacterModel characterModel) && _profile.AppliesTo(characterModel))
			{
				object? obj = AccessTools.Field(typeof(NMultiplayerLoadGameScreen), "_bgContainer")?.GetValue(screen);
				Control val = (Control)((obj is Control) ? obj : null);
				if (val == null)
				{
					SkinLog.Warn("Multiplayer load background container missing.");
				}
				else
				{
					CharacterSelectPresentationProfile presentation = Presentation;
					SetCharacterSelectBackgroundVariant(presentation.NormalizeVariant(_settings.CharacterSelectBackgroundVariant), persist: false);
					CharacterSelectBackgroundPresentationProfile backgroundOptionOrDefault = presentation.GetBackgroundOptionOrDefault(_backgroundVariant);
					string scenePathForVariant = GetScenePathForVariant(characterModel, backgroundOptionOrDefault);
					PrepareNativeCharacterSelectHost(val);
					Node val2 = LoadSceneIntoContainer(val, scenePathForVariant, "multiplayer_load");
					if (val2 != null && backgroundOptionOrDefault.ForceLoopAnimation)
					{
						ForceLoopAnimationRecursive(val2, "multiplayer_load");
					}
				}
			}
		});
	}

	public void ApplyRandomCharacterResolved(NCharacterSelectScreen screen, CharacterModel characterModel)
	{
		SkinPatchGuard.Run("Random character select background resolution patch", delegate
		{
			if (_profile.AppliesTo(characterModel))
			{
				CharacterSelectPresentationProfile presentation = Presentation;
				Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(presentation.OriginalBgNodeName));
				Control val = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(presentation.CustomBgNodeName));
				if (val == null)
				{
					val = CreateCustomBgContainer(screen, nodeOrNull, presentation.CustomBgNodeName);
				}
				SetCharacterSelectBackgroundVariant(presentation.NormalizeVariant(_settings.CharacterSelectBackgroundVariant), persist: false);
				ApplyCharacterSelectBackgroundMode(val, nodeOrNull, characterModel, "character_select_random_resolution");
				EnsureCharacterSelectMenu(screen, val, nodeOrNull, characterModel);
			}
		});
	}

	public void PlaySelectedVoice(NCharacterSelectButton button)
	{
		SkinPatchGuard.Run("Character select voice", delegate
		{
			if (_profile.AppliesTo(button.Character))
			{
				SetCharacterSelectBackgroundVariant(Presentation.NormalizeVariant(_settings.CharacterSelectBackgroundVariant), persist: false);
				TryPlayCharacterSelectVoice(_backgroundVariant, "character_selected");
			}
			else
			{
				_voicePlayer?.Stop();
			}
		});
	}

	public void ApplyCombatPresentationModeDropdown(NCharacterSelectScreen screen, NCharacterSelectButton button, CharacterModel characterModel)
	{
		SkinPatchGuard.Run("Character select combat presentation mode", delegate
		{
			RemoveLegacyCharacterSelectControls(screen);
			if (button.IsLocked || !_profile.AppliesTo(characterModel))
			{
				CharacterSelectSettingsMenu.RemoveOwned((Control)(object)screen, _profile.ProfileId);
			}
			else
			{
				CharacterSelectPresentationProfile presentation = Presentation;
				Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(presentation.OriginalBgNodeName));
				Control val = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(presentation.CustomBgNodeName));
				if (val == null)
				{
					val = CreateCustomBgContainer(screen, nodeOrNull, presentation.CustomBgNodeName);
				}
				EnsureCharacterSelectMenu(screen, val, nodeOrNull, characterModel);
			}
		});
	}

	private Control CreateCustomBgContainer(NCharacterSelectScreen screen, Control? originalBg, string nodeName)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		Control val = new Control
		{
			Name = StringName.op_Implicit(nodeName)
		};
		val.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		if (originalBg != null)
		{
			((Node)originalBg).GetParent().AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)originalBg).GetParent().MoveChild((Node)(object)val, ((Node)originalBg).GetIndex(false));
		}
		else
		{
			((Node)screen).AddChild((Node)(object)val, false, (InternalMode)0);
			((Node)screen).MoveChild((Node)(object)val, 0);
		}
		return val;
	}

	private void EnsureCharacterSelectMenu(NCharacterSelectScreen screen, Control customBg, Control? originalBg, CharacterModel characterModel)
	{
		CharacterSelectPresentationProfile presentation = Presentation;
		List<CharacterSelectMenuItemDescriptor> list = CharacterSelectCommonOptions.CreateItems(_settings, presentation.SupportedBackgroundVariants, delegate(CharacterSelectBackgroundVariant variant)
		{
			SetCharacterSelectBackgroundVariant(variant, persist: false);
			ApplyCharacterSelectBackgroundMode(customBg, originalBg, characterModel, "background_toggle");
			TryPlayCharacterSelectVoice(variant, "background_toggle");
		}, delegate(bool enabled)
		{
			if (!enabled)
			{
				_voicePlayer?.Stop();
			}
		});
		Type slot = ((object)characterModel).GetType();
		IReadOnlyList<string> characters = CharacterAppearanceCatalog.CharacterIds(slot);
		CharacterSelectSettingsMenu menu = null;
		if (characters.Count > 1)
		{
			list.Insert(1, new CharacterSelectMenuActionDescriptor("appearance_character", "res://CznCore/images/ui/icon_contents_lobby_mode_live_event_ui_48.png", () => CharacterAppearanceCatalog.UiText("appearance_character", "Character"), () => CharacterAppearanceCatalog.CharacterName(slot), () => CharacterAppearanceCatalog.UiText("appearance_character_tooltip", "Select a presentation character; gameplay stays unchanged."), delegate
			{
				int num = characters.ToList().IndexOf(CharacterAppearanceCatalog.Current(slot).ProfileId);
				CharacterAppearanceCatalog.Select(slot, characters[(num + 1) % characters.Count], "default");
				RefreshAppearance();
			}));
		}
		IReadOnlyList<string> variants = CharacterAppearanceCatalog.VariantIds(_profile.ProfileId);
		EnsureOutfitSelector(screen, variants.Count, CycleOutfit);
		if (variants.Count > 1 && string.IsNullOrEmpty(presentation.OutfitPreviewAnimation))
		{
			list.Insert((characters.Count <= 1) ? 1 : 2, new CharacterSelectMenuActionDescriptor("appearance_variant", "res://CznCore/images/ui/icon_contents_lobby_mode_live_event_ui_48.png", () => CharacterAppearanceCatalog.UiText("appearance_outfit", "Outfit"), () => OutfitLabel(slot), () => CharacterAppearanceCatalog.UiText("appearance_outfit_tooltip", "Select before starting a run. Unchanged presentation surfaces inherit the original outfit."), delegate
			{
				CycleOutfit(1);
			}));
		}
		List<CharacterSelectMenuSectionDescriptor> list2 = new List<CharacterSelectMenuSectionDescriptor>
		{
			new CharacterSelectMenuSectionDescriptor("character_screen", () => _settingsText?.VisualSettingsHeader.Current ?? "Character Screen", list)
		};
		IReadOnlyList<CharacterSelectMenuSectionDescriptor> readOnlyList = _additionalMenuSections?.Invoke();
		if (readOnlyList != null && readOnlyList.Count > 0)
		{
			list2.AddRange(readOnlyList);
		}
		menu = CharacterSelectSettingsMenu.Attach((Control)(object)screen, _profile.ProfileId, _profile.SettingsText.SettingsHeader.Current, list2);
		void CycleOutfit(int direction)
		{
			int num2 = variants.ToList().IndexOf(CharacterAppearanceCatalog.Current(slot).VariantId);
			CharacterAppearanceCatalog.Select(slot, _profile.ProfileId, variants[(num2 + direction + variants.Count) % variants.Count]);
			RefreshAppearance();
		}
		void RefreshAppearance()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Callable val = Callable.From((Action)delegate
			{
				//IL_0063: Unknown result type (might be due to invalid IL or missing references)
				if (menu != null && GodotObject.IsInstanceValid((GodotObject)(object)menu) && menu.IsCurrent)
				{
					CharacterSelectSettingsMenu.RemoveOwned((Control)(object)screen, _profile.ProfileId);
					((Node)screen).GetNode<RichTextLabel>(NodePath.op_Implicit("InfoPanel/VBoxContainer/DescriptionLabel")).Text = new LocString("characters", characterModel.CharacterSelectDesc).GetFormattedText();
					NCharacterSelectButton lastSelectionButton = _lastSelectionButton;
					if (lastSelectionButton != null && GodotObject.IsInstanceValid((GodotObject)(object)lastSelectionButton))
					{
						SkinEventHub.Publish(new CharacterSelectedEvent(screen, lastSelectionButton, characterModel));
					}
					else
					{
						ApplyRandomCharacterResolved(screen, characterModel);
					}
					BundledSkinCardArtService.Refresh();
				}
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private void RemoveOutfitSelector(Node screen)
	{
		Control nodeOrNull = screen.GetNodeOrNull<Control>(NodePath.op_Implicit(_profile.RuntimeNodeName("OutfitSelector")));
		if (nodeOrNull != null)
		{
			((Node)nodeOrNull).GetParent().RemoveChild((Node)(object)nodeOrNull);
			((Node)nodeOrNull).QueueFree();
		}
	}

	private void EnsureOutfitSelector(NCharacterSelectScreen screen, int count, Action<int> cycle)
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		CharacterSkinProfile characterSkinProfile = _profile.For(null);
		CharacterSelectPresentationProfile characterSelectPresentation = characterSkinProfile.CharacterSelectPresentation;
		string text = _profile.RuntimeNodeName("OutfitSelector");
		if (count < 2 || string.IsNullOrEmpty(characterSelectPresentation.OutfitPreviewAnimation))
		{
			RemoveOutfitSelector((Node)(object)screen);
			return;
		}
		string text2 = OutfitLabel(_profile.ResolveTargetCharacterType());
		string text3 = CharacterAppearanceCatalog.Current(_profile.ResolveTargetCharacterType())?.VariantId;
		string text4 = characterSkinProfile.BodySkeletonDataPath + "|" + text3 + "|" + text2;
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit(text));
		if (nodeOrNull != null)
		{
			StringName obj = StringName.op_Implicit("appearance");
			Variant val = default(Variant);
			val = ((GodotObject)nodeOrNull).GetMeta(obj, val);
			if (((Variant)(ref val)).AsString() == text4)
			{
				return;
			}
		}
		RemoveOutfitSelector((Node)(object)screen);
		Script val2 = ResourceLoader.Load<Script>("res://CznCore/scripts/outfit_selector.gd", (string)null, (CacheMode)1);
		if (val2 != null)
		{
			Control val3 = new Control
			{
				Name = StringName.op_Implicit(text),
				MouseFilter = (MouseFilterEnum)2
			};
			((GodotObject)val3).SetScript(Variant.op_Implicit((GodotObject)(object)val2));
			((GodotObject)val3).Set(StringName.op_Implicit("skeleton_path"), Variant.op_Implicit(characterSkinProfile.BodySkeletonDataPath));
			((GodotObject)val3).Set(StringName.op_Implicit("animation_name"), Variant.op_Implicit(characterSelectPresentation.OutfitPreviewAnimation));
			((GodotObject)val3).Set(StringName.op_Implicit("outfit_name"), Variant.op_Implicit(text2));
			((GodotObject)val3).Set(StringName.op_Implicit("previous_outfit"), Variant.op_Implicit(Callable.From((Action)delegate
			{
				cycle(-1);
			})));
			((GodotObject)val3).Set(StringName.op_Implicit("next_outfit"), Variant.op_Implicit(Callable.From((Action)delegate
			{
				cycle(1);
			})));
			((GodotObject)val3).SetMeta(StringName.op_Implicit("appearance"), Variant.op_Implicit(text4));
			((Node)screen).AddChild((Node)(object)val3, false, (InternalMode)0);
			val3.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		}
	}

	private void RemoveLegacyCharacterSelectControls(NCharacterSelectScreen screen)
	{
		string text = _profile.RuntimeNodeName("CardPlayPresentationBtn");
		string toggleButtonNodeName = Presentation.ToggleButtonNodeName;
		string[] array = new string[6]
		{
			ToolbarPanelNodeName,
			"CharSelectButtons/" + ToolbarPanelNodeName,
			text,
			toggleButtonNodeName,
			"CharSelectButtons/" + text,
			"CharSelectButtons/" + toggleButtonNodeName
		};
		foreach (string text2 in array)
		{
			Node nodeOrNull = ((Node)screen).GetNodeOrNull<Node>(NodePath.op_Implicit(text2));
			if (nodeOrNull != null)
			{
				Node parent = nodeOrNull.GetParent();
				if (parent != null)
				{
					parent.RemoveChild(nodeOrNull);
				}
				nodeOrNull.QueueFree();
			}
		}
	}

	private void ApplyCharacterSelectBackgroundMode(Control customBg, Control? originalBg, CharacterModel characterModel, string source)
	{
		CharacterSelectBackgroundPresentationProfile backgroundOptionOrDefault = Presentation.GetBackgroundOptionOrDefault(_backgroundVariant);
		SetCharacterSelectBackgroundVariant(backgroundOptionOrDefault.Variant, persist: false);
		if (!string.IsNullOrWhiteSpace(backgroundOptionOrDefault.ScenePath))
		{
			Control val = originalBg ?? customBg;
			bool flag = val == originalBg;
			if (flag)
			{
				PrepareNativeCharacterSelectHost(val);
			}
			SetVisible((CanvasItem?)(object)originalBg, visible: true);
			SetVisible((CanvasItem?)(object)customBg, !flag);
			Node val2 = LoadSceneIntoContainer(val, backgroundOptionOrDefault.ScenePath, source);
			if (val2 != null && backgroundOptionOrDefault.ForceLoopAnimation)
			{
				ForceLoopAnimationRecursive(val2, source);
			}
			return;
		}
		SetVisible((CanvasItem?)(object)customBg, visible: false);
		SetVisible((CanvasItem?)(object)originalBg, visible: true);
		if (originalBg != null)
		{
			RestoreNativeCharacterSelectHost(originalBg);
			if (backgroundOptionOrDefault.ForceLoopAnimation)
			{
				ForceLoopAnimationRecursive((Node)(object)originalBg, source);
			}
			return;
		}
		SetVisible((CanvasItem?)(object)customBg, visible: true);
		Node val3 = LoadSceneIntoContainer(customBg, characterModel.CharacterSelectBg, source);
		if (val3 != null && backgroundOptionOrDefault.ForceLoopAnimation)
		{
			ForceLoopAnimationRecursive(val3, source);
		}
	}

	private Node? LoadSceneIntoContainer(Control bgContainer, string scenePath, string source)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(scenePath))
		{
			SkinLog.Warn("Character select background scene path is empty: source=" + source);
			return null;
		}
		if (((Node)bgContainer).GetChildCount(false) > 0 && ((GodotObject)bgContainer).HasMeta(StringName.op_Implicit("czn_character_select_loaded_scene")))
		{
			StringName obj = StringName.op_Implicit("czn_character_select_loaded_scene");
			Variant val = default(Variant);
			val = ((GodotObject)bgContainer).GetMeta(obj, val);
			if (((Variant)(ref val)).AsString() == scenePath && ((GodotObject)((Node)bgContainer).GetChild(0, false)).HasMeta(StringName.op_Implicit("czn_character_select_loaded_scene_instance")))
			{
				Node child = ((Node)bgContainer).GetChild(0, false);
				StringName obj2 = StringName.op_Implicit("czn_character_select_loaded_scene_instance");
				val = default(Variant);
				val = ((GodotObject)child).GetMeta(obj2, val);
				if (((Variant)(ref val)).AsString() == scenePath)
				{
					return ((Node)bgContainer).GetChild(0, false);
				}
			}
		}
		GodotNodeUtil.ClearChildren((Node)(object)bgContainer);
		RemoveLoadedSceneMeta((Node)(object)bgContainer);
		PackedScene val2 = ResourceLoader.Load<PackedScene>(scenePath, (string)null, (CacheMode)1);
		if (val2 == null)
		{
			SkinLog.Warn("Character select background scene missing: source=" + source + " scene=" + scenePath);
			return null;
		}
		Node val3 = val2.Instantiate((GenEditState)0);
		Control val4 = (Control)(object)((val3 is Control) ? val3 : null);
		if (val4 != null)
		{
			CharacterSkinVoicePlayerRuntime voice = _voicePlayer;
			if (voice != null)
			{
				LobbySceneInteraction.Bind(val4, delegate(string path)
				{
					voice.Stop();
					return (!_settings.VoiceEnabled || string.IsNullOrWhiteSpace(path)) ? 0f : voice.PlayRequiredPresentationForWithResult(_voiceOwner?.Invoke(), path).DurationSeconds;
				}, voice.Stop);
			}
		}
		((GodotObject)val3).SetMeta(StringName.op_Implicit("czn_character_select_loaded_scene_instance"), Variant.From<string>(ref scenePath));
		((Node)bgContainer).AddChild(val3, false, (InternalMode)0);
		((GodotObject)bgContainer).SetMeta(StringName.op_Implicit("czn_character_select_loaded_scene"), Variant.From<string>(ref scenePath));
		SkinLog.Info("Character select background loaded: source=" + source + " scene=" + scenePath + " variant=" + _backgroundVariant);
		return val3;
	}

	private void PrepareNativeCharacterSelectHost(Control nativeHost)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		StoreNativeCharacterSelectHost(nativeHost);
		StringName obj = StringName.op_Implicit("czn_character_select_native_host_owner");
		string profileId = _profile.ProfileId;
		((GodotObject)nativeHost).SetMeta(obj, Variant.From<string>(ref profileId));
		nativeHost.Position = Vector2.Zero;
		Rect2 viewportRect = ((CanvasItem)nativeHost).GetViewportRect();
		nativeHost.Size = ((Rect2)(ref viewportRect)).Size;
		nativeHost.Scale = Vector2.One;
		nativeHost.PivotOffset = Vector2.Zero;
	}

	private static void StoreNativeCharacterSelectHost(Control nativeHost)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_position")))
		{
			StringName obj = StringName.op_Implicit("czn_character_select_native_host_position");
			Vector2 position = nativeHost.Position;
			((GodotObject)nativeHost).SetMeta(obj, Variant.From<Vector2>(ref position));
			StringName obj2 = StringName.op_Implicit("czn_character_select_native_host_size");
			position = nativeHost.Size;
			((GodotObject)nativeHost).SetMeta(obj2, Variant.From<Vector2>(ref position));
			StringName obj3 = StringName.op_Implicit("czn_character_select_native_host_scale");
			position = nativeHost.Scale;
			((GodotObject)nativeHost).SetMeta(obj3, Variant.From<Vector2>(ref position));
			StringName obj4 = StringName.op_Implicit("czn_character_select_native_host_pivot");
			position = nativeHost.PivotOffset;
			((GodotObject)nativeHost).SetMeta(obj4, Variant.From<Vector2>(ref position));
		}
	}

	private void RestoreNativeCharacterSelectHost(Control nativeHost)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_owner")))
		{
			return;
		}
		StringName obj = StringName.op_Implicit("czn_character_select_native_host_owner");
		Variant val = default(Variant);
		val = ((GodotObject)nativeHost).GetMeta(obj, val);
		string text = ((Variant)(ref val)).AsString();
		if (string.IsNullOrEmpty(text) || string.Equals(text, _profile.ProfileId, StringComparison.Ordinal))
		{
			if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_position")))
			{
				StringName obj2 = StringName.op_Implicit("czn_character_select_native_host_position");
				val = default(Variant);
				val = ((GodotObject)nativeHost).GetMeta(obj2, val);
				nativeHost.Position = ((Variant)(ref val)).AsVector2();
			}
			if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_size")))
			{
				StringName obj3 = StringName.op_Implicit("czn_character_select_native_host_size");
				val = default(Variant);
				val = ((GodotObject)nativeHost).GetMeta(obj3, val);
				nativeHost.Size = ((Variant)(ref val)).AsVector2();
			}
			if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_scale")))
			{
				StringName obj4 = StringName.op_Implicit("czn_character_select_native_host_scale");
				val = default(Variant);
				val = ((GodotObject)nativeHost).GetMeta(obj4, val);
				nativeHost.Scale = ((Variant)(ref val)).AsVector2();
			}
			if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_pivot")))
			{
				StringName obj5 = StringName.op_Implicit("czn_character_select_native_host_pivot");
				val = default(Variant);
				val = ((GodotObject)nativeHost).GetMeta(obj5, val);
				nativeHost.PivotOffset = ((Variant)(ref val)).AsVector2();
			}
			((GodotObject)nativeHost).RemoveMeta(StringName.op_Implicit("czn_character_select_native_host_owner"));
			RemoveNativeHostTransformMeta(nativeHost);
			RemoveLoadedSceneMeta((Node)(object)nativeHost);
		}
	}

	private static void RemoveNativeHostTransformMeta(Control nativeHost)
	{
		if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_position")))
		{
			((GodotObject)nativeHost).RemoveMeta(StringName.op_Implicit("czn_character_select_native_host_position"));
		}
		if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_size")))
		{
			((GodotObject)nativeHost).RemoveMeta(StringName.op_Implicit("czn_character_select_native_host_size"));
		}
		if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_scale")))
		{
			((GodotObject)nativeHost).RemoveMeta(StringName.op_Implicit("czn_character_select_native_host_scale"));
		}
		if (((GodotObject)nativeHost).HasMeta(StringName.op_Implicit("czn_character_select_native_host_pivot")))
		{
			((GodotObject)nativeHost).RemoveMeta(StringName.op_Implicit("czn_character_select_native_host_pivot"));
		}
	}

	private void ForceLoopAnimationRecursive(Node root, string source)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		CharacterSelectPresentationProfile presentation = Presentation;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			CanvasItem val2 = (CanvasItem)(object)((val is CanvasItem) ? val : null);
			if (val2 != null && !val2.Visible)
			{
				continue;
			}
			foreach (Node child in val.GetChildren(false))
			{
				stack.Push(child);
			}
			if (((GodotObject)val).GetClass() != "SpineSprite")
			{
				continue;
			}
			try
			{
				MegaSprite val3 = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val));
				num++;
				Func<Node, string, bool>? tryPrepareLoopSpine = _tryPrepareLoopSpine;
				if (tryPrepareLoopSpine != null && tryPrepareLoopSpine(val, source))
				{
					num5++;
				}
				if (val3.HasAnimation(presentation.LoopAnimationName))
				{
					SpineAnimationCompat.SetAnimation(val3.GetAnimationState(), presentation.LoopAnimationName);
					num2++;
					Func<Node, string, bool>? tryPrepareLoopSpine2 = _tryPrepareLoopSpine;
					if (tryPrepareLoopSpine2 != null && tryPrepareLoopSpine2(val, source))
					{
						num5++;
					}
				}
				else
				{
					num3++;
				}
			}
			catch (Exception ex)
			{
				num4++;
				SkinLog.Warn("Character select SpineSprite loop failed: source=" + source + " " + SkinLog.FormatException(ex));
			}
		}
		SkinLog.Info("Character select bg loop forced: source=" + source + " found=" + num + " started=" + num2 + " missing=" + num3 + " failed=" + num4 + " prepared=" + num5);
	}

	private void TryPlayCharacterSelectVoice(CharacterSelectBackgroundVariant backgroundVariant, string source)
	{
		CharacterSkinVoicePlayerRuntime voicePlayer = _voicePlayer;
		if (voicePlayer == null)
		{
			return;
		}
		voicePlayer.Stop();
		if (_profile.CharacterSelect.TryGetVoicePath(backgroundVariant, out string voicePath))
		{
			if (voicePlayer.PlayFor(_voiceOwner?.Invoke(), voicePath))
			{
				SkinLog.Info("Played character select voice: source=" + source + " background=" + backgroundVariant.ToString() + " path=" + voicePath);
			}
			voicePlayer.Prewarm(voicePath);
		}
	}

	private void SetCharacterSelectBackgroundVariant(CharacterSelectBackgroundVariant variant, bool persist)
	{
		_backgroundVariant = Presentation.NormalizeVariant(variant);
		_settings.SetCharacterSelectBackgroundVariant(_backgroundVariant, persist);
	}

	private string GetScenePathForVariant(CharacterModel characterModel, CharacterSelectBackgroundVariant variant)
	{
		return GetScenePathForVariant(characterModel, Presentation.GetBackgroundOptionOrDefault(variant));
	}

	private static string GetScenePathForVariant(CharacterModel characterModel, CharacterSelectBackgroundPresentationProfile option)
	{
		if (!string.IsNullOrWhiteSpace(option.ScenePath))
		{
			return option.ScenePath;
		}
		return characterModel.CharacterSelectBg;
	}

	private static void SetVisible(CanvasItem? item, bool visible)
	{
		if (item != null)
		{
			item.Visible = visible;
		}
	}

	private static bool AnyOtherCustomBgVisible(NCharacterSelectScreen screen, string activeCustomBgNodeName)
	{
		foreach (Node child in ((Node)screen).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((object)child.Name).ToString().EndsWith("CustomBg", StringComparison.Ordinal) && child.Name != StringName.op_Implicit(activeCustomBgNodeName) && ((CanvasItem)val).Visible)
			{
				return true;
			}
		}
		return false;
	}

	private static void RemoveLoadedSceneMeta(Node node)
	{
		if (((GodotObject)node).HasMeta(StringName.op_Implicit("czn_character_select_loaded_scene")))
		{
			((GodotObject)node).RemoveMeta(StringName.op_Implicit("czn_character_select_loaded_scene"));
		}
	}

	private static bool TryGetMultiplayerLoadLocalCharacter(NMultiplayerLoadGameScreen screen, out CharacterModel characterModel)
	{
		characterModel = null;
		LoadRunLobby lobby = default(LoadRunLobby);
		ref LoadRunLobby reference = ref lobby;
		object? obj = AccessTools.Field(typeof(NMultiplayerLoadGameScreen), "_runLobby")?.GetValue(screen);
		reference = (LoadRunLobby)((obj is LoadRunLobby) ? obj : null);
		if (lobby == null)
		{
			return false;
		}
		SerializablePlayer val = ((IEnumerable<SerializablePlayer>)lobby.Run.Players).FirstOrDefault((Func<SerializablePlayer, bool>)((SerializablePlayer player) => player.NetId == lobby.NetService.NetId));
		if (((val != null) ? val.CharacterId : null) == (ModelId)null)
		{
			return false;
		}
		characterModel = ModelDb.GetById<CharacterModel>(val.CharacterId);
		return true;
	}
}

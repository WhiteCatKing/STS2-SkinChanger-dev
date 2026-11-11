using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CznCore.Godot;
using CznCore.Presentation;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace CznCore.Settings;

internal static class SkinSettingsUi
{
	private enum RebuildScope
	{
		Global,
		Multiplayer,
		Extensions,
		Shared,
		Character
	}

	private sealed record SettingsUiShell(VBoxContainer PanelVBox, HBoxContainer Shell, VBoxContainer Sidebar, VBoxContainer ContentHost);

	private sealed record SettingsUiBuildContext(NSettingsScreen Screen, NSettingsPanel Panel, VBoxContainer VBox, RichTextLabel TemplateLabel, PackedScene SliderScene, PackedScene? TickboxVisualScene, ISkinSettingsUiRuntime Runtime, ISkinSettingsGlobalUiRuntime Global, ISkinSettingsUiText Text)
	{
		internal SettingsUiExtensionContext Extensions => new SettingsUiExtensionContext(Screen, Panel, VBox, TemplateLabel, SliderScene, TickboxVisualScene, Text);

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Screen = ");
			builder.Append(Screen);
			builder.Append(", Panel = ");
			builder.Append(Panel);
			builder.Append(", VBox = ");
			builder.Append(VBox);
			builder.Append(", TemplateLabel = ");
			builder.Append(TemplateLabel);
			builder.Append(", SliderScene = ");
			builder.Append(SliderScene);
			builder.Append(", TickboxVisualScene = ");
			builder.Append(TickboxVisualScene);
			builder.Append(", Runtime = ");
			builder.Append(Runtime);
			builder.Append(", Global = ");
			builder.Append(Global);
			builder.Append(", Text = ");
			builder.Append(Text);
			return true;
		}
	}

	private sealed record SettingsUiExtensionContext(NSettingsScreen Screen, NSettingsPanel Panel, VBoxContainer VBox, RichTextLabel TemplateLabel, PackedScene SliderScene, PackedScene? TickboxVisualScene, ISkinSettingsUiText Text);

	private enum SettingsControlRowRole
	{
		Standalone,
		Parent,
		Child
	}

	internal const string PortraitNavigationMeta = "czn_mod_settings_portrait_navigation";

	private const string ClipperPath = "ScrollContainer/Mask/Clipper";

	private const string SoundSettingsPanelPath = "ScrollContainer/Mask/Clipper/SoundSettings";

	private const string TemplateLabelPath = "ScrollContainer/Mask/Clipper/SoundSettings/VBoxContainer/SfxVolume/Label";

	private const string ModTabScenePath = "res://scenes/screens/settings_tab.tscn";

	private const string SettingsSliderScenePath = "res://scenes/screens/settings_slider.tscn";

	private const string TickboxVisualScenePath = "res://scenes/ui/tickbox.tscn";

	private const string TabBindingMeta = "czn_mod_settings_tab_bound";

	private const string SliderBindingMeta = "czn_mod_slider_bound";

	private const string ShellVersionMeta = "czn_mod_settings_shell_version";

	private const int CurrentShellVersion = 8;

	private const string SelectedPageMeta = "czn_mod_settings_selected_page";

	private const string PageIdMeta = "czn_mod_settings_page_id";

	private const string SelectedGlobalCategoryMeta = "czn_mod_settings_selected_global_category";

	private const string GlobalCategoryMeta = "czn_mod_settings_global_category";

	private const string GlobalCategoryIdMeta = "czn_mod_settings_global_category_id";

	private const string GlobalCategorySignatureMeta = "czn_mod_settings_global_category_signature";

	private const string SelectedCharacterCategoryMeta = "czn_mod_settings_selected_character_category";

	private const string CharacterCategoryMeta = "czn_mod_settings_character_category";

	private const string CategoryIdMeta = "czn_mod_settings_category_id";

	private const string FocusPreferredMeta = "czn_mod_settings_focus_preferred";

	private const string GlobalPageId = "global";

	private const string GlobalCategoryGeneral = "general";

	private const string GlobalCategoryMultiplayer = "multiplayer";

	private const string CharacterCategoryVisual = "visual";

	private const string CharacterCategoryCombat = "combat";

	private const string CharacterCategoryEffects = "effects";

	private const string CharacterCategoryAudio = "audio";

	private const string ShellName = "CznModSettingsShell";

	private const string SidebarName = "CznModSettingsSidebar";

	private const string ContentHostName = "CznModSettingsContentHost";

	private const string GlobalPageName = "CznModSettingsPage_Global";

	private const string CharacterPageNamePrefix = "CznModSettingsPage_";

	private const string GlobalNavName = "CznModSettingsNav_Global";

	private const string CharacterNavNamePrefix = "CznModSettingsNav_";

	private const float ShellWidth = 1012f;

	private const float SidebarWidth = 118f;

	private const float ShellGap = 12f;

	private const float NavButtonWidth = 96f;

	private const float NavButtonHeight = 96f;

	private const float NavIconSize = 76f;

	private const float ContentWidth = 882f;

	private const float LabelWidth = 330f;

	private const float SliderWidth = 324f;

	private const float DropdownWidth = 320f;

	private const float CategoryButtonWidth = 206f;

	private const float CombatModeButtonWidth = 106f;

	private const float CombatModeSegmentWidth = 334f;

	private const float ToggleWidth = 64f;

	private const float RowHeight = 56f;

	private const float DescribedRowHeight = 74f;

	private const int MinSettingsFontSize = 14;

	private const int MaxSettingsFontSize = 28;

	private const int RowFontDelta = 2;

	private const int DescriptionFontDelta = -2;

	private static readonly Color CreamGold = new Color(0.831f, 0.784f, 0.557f, 1f);

	private static readonly Color DimText = new Color(0.541f, 0.494f, 0.361f, 1f);

	private static readonly Color TextColor = new Color(0.9f, 0.85f, 0.75f, 1f);

	private static readonly Color DisabledRowModulate = new Color(1f, 1f, 1f, 0.46f);

	private static readonly Color ModHeaderBg = new Color(0.173f, 0.263f, 0.31f, 1f);

	private static readonly Color SubHeaderBg = new Color(0.105f, 0.147f, 0.165f, 0.82f);

	private static readonly Color StatusHeaderBg = new Color(0.098f, 0.137f, 0.153f, 0.92f);

	private static readonly Color SubHeaderLineColor = new Color(0.831f, 0.784f, 0.557f, 0.46f);

	private static readonly Color RowSeparatorColor = new Color(0.52f, 0.49f, 0.42f, 0.55f);

	private static readonly Color NavIdleBg = new Color(0.118f, 0.157f, 0.173f, 0.84f);

	private static readonly Color NavHoverBg = new Color(0.18f, 0.25f, 0.29f, 0.92f);

	private static readonly Color NavSelectedBg = new Color(0.2f, 0.31f, 0.36f, 0.96f);

	private const string CznTabName = "CznModSettingsTab";

	private const string CznPanelName = "CznModSettingsPanel";

	private const string RootHeaderSectionName = "CznModSettingsRootHeader";

	private const string GlobalCategoryNavSectionName = "CznModGlobalCategoryNav";

	private const string GlobalHeaderSectionName = "CznModGlobalHeader";

	private const string MultiplayerHeaderSectionName = "CznModMultiplayerHeader";

	private const string ExtensionSectionNamePrefix = "CznModExtension_";

	private const string MultiplayerIntroSectionName = "CznModMultiplayerIntro";

	private const string MultiplayerSummarySectionName = "CznModMultiplayerSummary";

	private const string TeammateVisualHeaderSectionName = "CznModTeammateVisualHeader";

	private const string TeammateAudioHeaderSectionName = "CznModTeammateAudioHeader";

	private const string DebugLogSectionName = "CznModDebugLogEnabled";

	private const string GlobalEffectsSectionName = "CznModGlobalEffectsEnabled";

	private const string GlobalParticlesSectionName = "CznModGlobalParticlesEnabled";

	private const string GlobalCombatSfxSectionName = "CznModGlobalCombatSfxEnabled";

	private const string GlobalVoiceSectionName = "CznModGlobalVoiceEnabled";

	private const string TeammatePresentationSectionName = "CznModTeammatePresentationEnabled";

	private const string TeammateEffectsSectionName = "CznModTeammateEffectsEnabled";

	private const string TeammateCombatSfxSectionName = "CznModTeammateCombatSfxEnabled";

	private const string TeammateVoiceSectionName = "CznModTeammateVoiceEnabled";

	private const string TeammateVoiceCooldownSectionName = "CznModTeammateVoiceCooldown";

	private const string TeammateCutInSectionName = "CznModTeammateCutInEnabled";

	private const string TeammateAttackFullScreenPresentationModeSectionName = "CznModTeammateAttackFullScreenPresentationMode";

	private const string GlobalCombatPresentationApplySectionName = "CznModGlobalCombatPresentationApply";

	private const string CombatPresentationModeSectionName = "CznModCombatPresentationMode";

	private const string CombatPresentationSummarySectionName = "CznModCombatPresentationSummary";

	private const string CharacterStatusSectionName = "CznModCharacterStatus";

	private const string CharacterCategoryNavSectionName = "CznModCharacterCategoryNav";

	private const string CharacterHeaderSectionName = "CznModCharacterHeader";

	private const string CharacterExtensionSectionNamePrefix = "CznModCharacterExtension_";

	private const string CharacterEffectsSectionName = "CznModCharacterEffectsEnabled";

	private const string ParticlesSectionName = "CznModParticlesEnabled";

	private const string CombatSfxSectionName = "CznModCombatSfxEnabled";

	private const string CharacterVoiceSectionName = "CznModCharacterVoiceEnabled";

	private const string VisualHeaderSectionName = "CznModVisualHeader";

	private const string CombatEffectsHeaderSectionName = "CznModCombatEffectsHeader";

	private const string AudioHeaderSectionName = "CznModAudioHeader";

	private const string VoiceSectionName = "CznModVoiceVolume";

	private const string VoiceCooldownSectionName = "CznModVoiceCooldown";

	private const string EffectVolumeSectionName = "CznModEffectVolume";

	private const string EffectBrightnessSectionName = "CznModEffectBrightness";

	private const string EffectOpacitySectionName = "CznModEffectOpacity";

	private const string CharacterSelectBackgroundSectionName = "CznModCharacterSelectBackground";

	private const string CardAnimationHeaderSectionName = "CznModCardAnimationHeader";

	private const string CombatPresentationCustomHeaderSectionName = "CznModCombatPresentationCustomHeader";

	private const string LeftSideCardPresentationHeaderSectionName = "CznModLeftSideCardPresentationHeader";

	private const string BattleReadyEnabledSectionName = "CznModBattleReadyEnabled";

	private const string CardPlayPresentationEnabledSectionName = "CznModCardPlayPresentationEnabled";

	private const string AttackFullScreenPresentationModeSectionName = "CznModAttackFullScreenPresentationMode";

	private const string CardSelectionPresentationTriggerSectionName = "CznModCardSelectionPresentationTrigger";

	private const string MeleeApproachMovementEnabledSectionName = "CznModMeleeApproachMovementEnabled";

	private const string OfficialAttackTimingEnabledSectionName = "CznModOfficialAttackTimingEnabled";

	private const string BattleReadyScaleSectionName = "CznModBattleReadyScale";

	private const string BattleReadyOpacitySectionName = "CznModBattleReadyOpacity";

	private const string BattleReadyOffsetXSectionName = "CznModBattleReadyOffsetX";

	private const string BattleReadyOffsetYSectionName = "CznModBattleReadyOffsetY";

	private const string BattleReadyResetPositionSectionName = "CznModBattleReadyResetPosition";

	private const string DefeatCgEnabledSectionName = "CznModDefeatCgEnabled";

	public static void TryInject(NSettingsScreen screen, ISkinSettingsUiRuntime runtime, ISkinSettingsGlobalUiRuntime global, ISkinSettingsUiText text)
	{
		TryInjectCharacters(screen, new _003C_003Ez__ReadOnlySingleElementList<CharacterSettingsUiRegistration>(new CharacterSettingsUiRegistration(runtime, global)), text);
	}

	public static void TryInjectCharacters(NSettingsScreen screen, IReadOnlyList<CharacterSettingsUiRegistration> characters, ISkinSettingsUiText text)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		if (characters.Count == 0)
		{
			return;
		}
		ISkinSettingsUiRuntime runtime = characters[0].Runtime;
		ISkinSettingsGlobalUiRuntime global = characters[0].Global;
		try
		{
			if (!EnsureModSettingsTabAndPanel(screen, text, out NSettingsPanel panel) || panel == null)
			{
				return;
			}
			VBoxContainer nodeOrNull = ((Node)panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
			if (nodeOrNull == null)
			{
				SkinLog.Warn("Settings UI inject skipped: mod panel missing VBoxContainer.");
				return;
			}
			RichTextLabel nodeOrNull2 = ((Node)screen).GetNodeOrNull<RichTextLabel>(new NodePath("ScrollContainer/Mask/Clipper/SoundSettings/VBoxContainer/SfxVolume/Label"));
			PackedScene val = ResourceLoader.Load<PackedScene>("res://scenes/screens/settings_slider.tscn", (string)null, (CacheMode)1);
			PackedScene tickboxVisualScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/tickbox.tscn", (string)null, (CacheMode)1);
			if (nodeOrNull2 == null || val == null)
			{
				SkinLog.Warn("Settings UI inject skipped: missing settings template.");
				return;
			}
			SettingsUiShell settingsUiShell = EnsureSettingsShell(panel, nodeOrNull);
			bool flag = false;
			VBoxContainer val2 = EnsureSettingsPage(settingsUiShell.ContentHost, "CznModSettingsPage_Global", "global");
			flag |= EnsureGlobalNavigationButton(panel, settingsUiShell.Sidebar, nodeOrNull2, runtime.GlobalSettingsIconPath, text);
			SettingsUiBuildContext settingsUiBuildContext = new SettingsUiBuildContext(screen, panel, val2, nodeOrNull2, val, tickboxVisualScene, runtime, global, text);
			flag |= TryCreateTopSpacerSection(val2, "CznModSettingsRootHeader");
			flag |= EnsureGlobalCategoryNavSection(settingsUiBuildContext.Extensions, includeCharacterCategories: true);
			flag |= TryCreateGlobalSettingsSections(settingsUiBuildContext);
			flag |= TryCreateMultiplayerSettingsSections(settingsUiBuildContext);
			flag |= TryCreateExtensionSettingsSections(settingsUiBuildContext.Extensions);
			foreach (CharacterSettingsUiRegistration character in characters)
			{
				string pageId = CharacterPageId(character.Runtime);
				VBoxContainer val3 = EnsureSettingsPage(settingsUiShell.ContentHost, CharacterPageName(character.Runtime), pageId);
				SettingsUiBuildContext ctx = new SettingsUiBuildContext(screen, panel, val3, nodeOrNull2, val, tickboxVisualScene, character.Runtime, character.Global, text);
				flag |= EnsureCharacterNavigationButton(settingsUiShell.Sidebar, ctx);
				flag |= TryCreateTopSpacerSection(val3, "CznModSettingsRootHeader");
				flag |= TryCreateCharacterSettingsSections(ctx);
			}
			RefreshPanelLayout(panel, nodeOrNull);
			RefreshSettingsShellSelection(panel);
			RefreshGlobalCategorySelection(panel);
			RefreshCharacterCategorySelection(panel);
			RefreshFocusNeighbors(panel);
			if (flag)
			{
				runtime.DebugLog("Injected CZN settings tab controls.");
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Settings UI inject failed: " + ex);
		}
	}

	public static void TryInjectShared(NSettingsScreen screen, ISkinSettingsUiText text)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		try
		{
			if (!EnsureModSettingsTabAndPanel(screen, text, out NSettingsPanel panel) || panel == null)
			{
				return;
			}
			VBoxContainer nodeOrNull = ((Node)panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
			if (nodeOrNull == null)
			{
				SkinLog.Warn("Shared settings UI inject skipped: mod panel missing VBoxContainer.");
				return;
			}
			RichTextLabel nodeOrNull2 = ((Node)screen).GetNodeOrNull<RichTextLabel>(new NodePath("ScrollContainer/Mask/Clipper/SoundSettings/VBoxContainer/SfxVolume/Label"));
			PackedScene val = ResourceLoader.Load<PackedScene>("res://scenes/screens/settings_slider.tscn", (string)null, (CacheMode)1);
			PackedScene tickboxVisualScene = ResourceLoader.Load<PackedScene>("res://scenes/ui/tickbox.tscn", (string)null, (CacheMode)1);
			if (nodeOrNull2 == null || val == null)
			{
				SkinLog.Warn("Shared settings UI inject skipped: missing settings template.");
				return;
			}
			SettingsUiShell settingsUiShell = EnsureSettingsShell(panel, nodeOrNull);
			VBoxContainer val2 = EnsureSettingsPage(settingsUiShell.ContentHost, "CznModSettingsPage_Global", "global");
			SettingsUiExtensionContext ctx = new SettingsUiExtensionContext(screen, panel, val2, nodeOrNull2, val, tickboxVisualScene, text);
			bool flag = EnsureGlobalNavigationButton(panel, settingsUiShell.Sidebar, nodeOrNull2, string.Empty, text);
			flag |= TryCreateTopSpacerSection(val2, "CznModSettingsRootHeader");
			flag |= EnsureGlobalCategoryNavSection(ctx, includeCharacterCategories: false);
			flag |= TryCreateExtensionSettingsSections(ctx);
			RefreshPanelLayout(panel, nodeOrNull);
			RefreshSettingsShellSelection(panel);
			RefreshGlobalCategorySelection(panel);
			RefreshCharacterCategorySelection(panel);
			RefreshFocusNeighbors(panel);
			SkinLog.InfoOnce("czn-mod-config-shared-host", "CZN Mod Config shared host: built=" + flag + ", categories=" + CountGlobalCategories(settingsUiShell) + ", character pages=" + CountCharacterPages(settingsUiShell) + ".");
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Shared settings UI inject failed: " + ex);
		}
	}

	private static int CountGlobalCategories(SettingsUiShell shell)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		List<string> list = new List<string>();
		foreach (Node child in ((Node)shell.ContentHost).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				Variant val2 = default(Variant);
				val2 = ((GodotObject)val).GetMeta(obj, val2);
				if (string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), "global", StringComparison.Ordinal))
				{
					CollectGlobalCategoryIds((Node)(object)val, list);
				}
			}
		}
		return list.Count;
	}

	private static int CountCharacterPages(SettingsUiShell shell)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Node child in ((Node)shell.ContentHost).GetChildren(false))
		{
			if (((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				Variant val = default(Variant);
				val = ((GodotObject)child).GetMeta(obj, val);
				if (((object)(Variant)(ref val)/*cast due to .constrained prefix*/).ToString().StartsWith("character:", StringComparison.Ordinal))
				{
					num++;
				}
			}
		}
		return num;
	}

	private static string CharacterPageId(ISkinSettingsUiRuntime runtime)
	{
		return "character:" + BuildCharacterSectionSuffix(runtime);
	}

	private static string CharacterPageName(ISkinSettingsUiRuntime runtime)
	{
		return "CznModSettingsPage_" + BuildCharacterSectionSuffix(runtime);
	}

	private static string CharacterNavName(ISkinSettingsUiRuntime runtime)
	{
		return "CznModSettingsNav_" + BuildCharacterSectionSuffix(runtime);
	}

	private static bool EnsureGlobalCategoryNavSection(SettingsUiExtensionContext ctx, bool includeCharacterCategories)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Expected O, but got Unknown
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		includeCharacterCategories |= ((Node)ctx.VBox).GetNodeOrNull(new NodePath("CznModGlobalHeader")) != null;
		List<(string, string)> list = new List<(string, string)>();
		if (includeCharacterCategories)
		{
			list.Add(("general", ctx.Text.GlobalSettingsHeader));
			list.Add(("multiplayer", ctx.Text.MultiplayerSettingsHeader));
		}
		foreach (SharedSettingsExtensionCategory category in SharedSettingsExtensionRegistry.GetCategories())
		{
			list.Add((category.Id, category.Label()));
		}
		string text = BuildGlobalCategorySignature(list);
		Node nodeOrNull = ((Node)ctx.VBox).GetNodeOrNull(new NodePath("CznModGlobalCategoryNav"));
		if (nodeOrNull != null)
		{
			if (((GodotObject)nodeOrNull).HasMeta(StringName.op_Implicit("czn_mod_settings_global_category_signature")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_global_category_signature");
				Variant val = default(Variant);
				val = ((GodotObject)nodeOrNull).GetMeta(obj, val);
				if (string.Equals(((object)(Variant)(ref val)/*cast due to .constrained prefix*/).ToString(), text, StringComparison.Ordinal))
				{
					return false;
				}
			}
			((Node)ctx.VBox).RemoveChild(nodeOrNull);
			nodeOrNull.QueueFree();
		}
		MarginContainer val2 = CreateSection("CznModGlobalCategoryNav", 0, 12, 46f);
		((GodotObject)val2).SetMeta(StringName.op_Implicit("czn_mod_settings_global_category_signature"), Variant.op_Implicit(text));
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(858f, 38f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		foreach (var (categoryId, label) in list)
		{
			((Node)val3).AddChild((Node)(object)CreateGlobalCategoryButton(ctx, categoryId, label), false, (InternalMode)0);
		}
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)ctx.VBox).AddChild((Node)(object)val2, false, (InternalMode)0);
		if (nodeOrNull != null)
		{
			((Node)ctx.VBox).MoveChild((Node)(object)val2, 1);
		}
		return true;
	}

	private static string BuildGlobalCategorySignature(IReadOnlyList<(string Id, string Label)> categories)
	{
		List<string> list = new List<string>();
		foreach (var category in categories)
		{
			string item = category.Id;
			list.Add(item);
		}
		return string.Join("|", list);
	}

	private static Button CreateGlobalCategoryButton(SettingsUiExtensionContext ctx, string categoryId, string label)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		CznSettingsButton cznSettingsButton = new CznSettingsButton();
		((Button)cznSettingsButton).Text = label;
		((BaseButton)cznSettingsButton).ToggleMode = false;
		((Control)cznSettingsButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsButton).CustomMinimumSize = new Vector2(206f, 36f);
		((Control)cznSettingsButton).SizeFlagsHorizontal = (SizeFlags)4;
		((GodotObject)cznSettingsButton).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((GodotObject)cznSettingsButton).SetMeta(StringName.op_Implicit("czn_mod_settings_global_category_id"), Variant.op_Implicit(categoryId));
		((Control)cznSettingsButton).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)ctx.TemplateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)cznSettingsButton).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(ctx.TemplateLabel, -2));
		((Control)cznSettingsButton).AddThemeColorOverride(StringName.op_Implicit("font_color"), CreamGold);
		((BaseButton)cznSettingsButton).Pressed += delegate
		{
			SetSelectedGlobalCategory(ctx.Panel, categoryId);
		};
		ApplyCategoryButtonStyle((Button)(object)cznSettingsButton, string.Equals(GetSelectedGlobalCategory(ctx.Panel), categoryId, StringComparison.Ordinal));
		return CznSettingsInputControls.AttachNativeOwner((Button)(object)cznSettingsButton);
	}

	private static void AssignGlobalCategory(SettingsUiBuildContext ctx, string categoryId, params string[] sectionNames)
	{
		AssignGlobalCategory(ctx.VBox, categoryId, sectionNames);
	}

	private static void AssignGlobalCategory(VBoxContainer vbox, string categoryId, params string[] sectionNames)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		foreach (string text in sectionNames)
		{
			Node nodeOrNull = ((Node)vbox).GetNodeOrNull(new NodePath(text));
			if (nodeOrNull != null)
			{
				((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("czn_mod_settings_global_category"), Variant.op_Implicit(categoryId));
			}
		}
	}

	private static bool TryCreateCharacterSettingsSections(SettingsUiBuildContext ctx)
	{
		return TryCreateCharacterStatusSection(ctx) | TryCreateCharacterCategoryNavSection(ctx) | TryCreateVisualSettingsSections(ctx) | TryCreateBattleReadySections(ctx) | TryCreateCombatEffectSections(ctx) | TryCreateAudioSettingsSections(ctx) | TryCreateCharacterExtensionSettingsSections(ctx);
	}

	private static bool TryCreateVisualSettingsSections(SettingsUiBuildContext ctx)
	{
		bool flag = Supports(ctx, SkinSettingsUiCapabilities.CharacterSelectBackground);
		bool flag2 = Supports(ctx, SkinSettingsUiCapabilities.DefeatCg);
		if (!flag && !flag2)
		{
			return false;
		}
		bool flag3 = TryCreateSubHeaderSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModVisualHeader"), ctx.Text.VisualSettingsHeader);
		AssignCharacterCategory(ctx, "CznModVisualHeader", "visual");
		if (flag)
		{
			flag3 |= TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModCharacterSelectBackground"), ctx.Text.CharacterSelectBackground, ctx.Text.CharacterSelectBackgroundDescription, BuildCharacterSelectBackgroundOptions(ctx), (int)ctx.Runtime.CharacterSelectBackgroundVariant, delegate(int id)
			{
				ctx.Runtime.SetCharacterSelectBackgroundVariant((CharacterSelectBackgroundVariant)id, persist: true);
			});
			AssignCharacterCategory(ctx, "CznModCharacterSelectBackground", "visual");
		}
		if (flag2)
		{
			flag3 |= TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModDefeatCgEnabled"), ctx.Text.ShowDefeatCgOverlay, ctx.Text.ShowDefeatCgOverlayDescription, BuildDefeatCgOverlayOptions(ctx), (int)ctx.Runtime.DefeatCgOverlayMode, delegate(int id)
			{
				ctx.Runtime.SetDefeatCgOverlayMode((DefeatCgOverlayMode)id, persist: true);
			});
			AssignCharacterCategory(ctx, "CznModDefeatCgEnabled", "visual");
		}
		return flag3;
	}

	private static IReadOnlyList<(int Id, string Label)> BuildDefeatCgOverlayOptions(SettingsUiBuildContext ctx)
	{
		return new _003C_003Ez__ReadOnlyArray<(int, string)>(new(int, string)[4]
		{
			(0, ctx.Text.DefeatCgOverlayOff),
			(1, ctx.Text.DefeatCgOverlayType1),
			(2, ctx.Text.DefeatCgOverlayType2),
			(3, ctx.Text.DefeatCgOverlayRandom)
		});
	}

	private static IReadOnlyList<(int Id, string Label)> BuildCharacterSelectBackgroundOptions(SettingsUiBuildContext ctx)
	{
		List<(int, string)> list = new List<(int, string)>();
		foreach (CharacterSelectBackgroundVariant supportedCharacterSelectBackgroundVariant in ctx.Runtime.SupportedCharacterSelectBackgroundVariants)
		{
			list.Add(((int)supportedCharacterSelectBackgroundVariant, GetCharacterSelectBackgroundLabel(ctx.Text, supportedCharacterSelectBackgroundVariant)));
		}
		if (list.Count == 0)
		{
			list.Add((0, ctx.Text.CharacterSelectBackgroundStaticPortrait));
		}
		return list;
	}

	private static string GetCharacterSelectBackgroundLabel(ISkinSettingsUiText text, CharacterSelectBackgroundVariant variant)
	{
		return variant switch
		{
			CharacterSelectBackgroundVariant.TeamJoinPortrait => text.CharacterSelectBackgroundTeamJoinPortrait, 
			CharacterSelectBackgroundVariant.LiveLobby => text.CharacterSelectBackgroundLiveLobby, 
			_ => text.CharacterSelectBackgroundStaticPortrait, 
		};
	}

	private static bool TryCreateBattleReadySections(SettingsUiBuildContext ctx)
	{
		bool flag = Supports(ctx, SkinSettingsUiCapabilities.CombatPresentationMode);
		bool flag2 = Supports(ctx, SkinSettingsUiCapabilities.CardSelectionPresentationTrigger);
		bool flag3 = Supports(ctx, SkinSettingsUiCapabilities.CustomCombatPresentation);
		bool flag4 = Supports(ctx, SkinSettingsUiCapabilities.CardPlayPresentation);
		bool flag5 = Supports(ctx, SkinSettingsUiCapabilities.BattleReadyPresentation);
		if (!flag && !flag2 && !flag3 && !flag5)
		{
			return false;
		}
		bool flag6 = TryCreateSubHeaderSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModCardAnimationHeader"), ctx.Text.CardAnimationHeader);
		AssignCharacterCategory(ctx, "CznModCardAnimationHeader", "combat");
		if (flag)
		{
			flag6 |= TryCreateCombatPresentationModeSection(ctx);
			flag6 |= TryCreateCombatPresentationSummarySection(ctx);
			AssignCharacterCategory(ctx, "CznModCombatPresentationSummary", "combat");
		}
		if (flag2)
		{
			flag6 |= TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModCardSelectionPresentationTrigger"), ctx.Text.CardSelectionPresentationTrigger, ctx.Text.CardSelectionPresentationTriggerDescription, new _003C_003Ez__ReadOnlyArray<(int, string)>(new(int, string)[2]
			{
				(0, ctx.Text.CardSelectionPresentationTriggerHover),
				(1, ctx.Text.CardSelectionPresentationTriggerSelection)
			}), (int)ctx.Runtime.CardSelectionPresentationTrigger, delegate(int id)
			{
				ctx.Runtime.SetCardSelectionPresentationTrigger((CardSelectionPresentationTrigger)id, persist: true);
			});
			AssignCharacterCategory(ctx, "CznModCardSelectionPresentationTrigger", "combat");
		}
		if (flag3 && ctx.Runtime.CombatPresentationMode == CombatPresentationMode.Custom)
		{
			flag6 |= TryCreateMiniHeaderSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModCombatPresentationCustomHeader"), ctx.Text.CombatPresentationCustom + " - " + ctx.Text.CardAnimationHeader);
			AssignCharacterCategory(ctx, "CznModCombatPresentationCustomHeader", "combat");
			if (flag4)
			{
				flag6 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModCardPlayPresentationEnabled"), ctx.Text.ShowCardPlayPresentation, ctx.Text.ShowCardPlayPresentationDescription, ctx.Runtime.ShowCardPlayPresentation, delegate(bool value)
				{
					ctx.Runtime.SetCombatPresentationMode(CombatPresentationMode.Custom, persist: true);
					ctx.Runtime.SetCardPlayPresentationEnabled(value, persist: true);
					RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
				});
				AssignCharacterCategory(ctx, "CznModCardPlayPresentationEnabled", "combat");
				flag6 |= TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModAttackFullScreenPresentationMode"), ctx.Text.AttackFullScreenPresentationMode, ctx.Text.AttackFullScreenPresentationModeDescription, new _003C_003Ez__ReadOnlyArray<(int, string)>(new(int, string)[2]
				{
					(0, ctx.Text.AttackFullScreenPresentationAlways),
					(1, ctx.Text.AttackFullScreenPresentationLethalOnly)
				}), (int)ctx.Runtime.AttackFullScreenPresentationMode, delegate(int id)
				{
					ctx.Runtime.SetAttackFullScreenPresentationMode((AttackFullScreenPresentationMode)id, persist: true);
				}, ctx.Runtime.ShowCardPlayPresentation);
				AssignCharacterCategory(ctx, "CznModAttackFullScreenPresentationMode", "combat");
			}
			if (ctx.Runtime.SupportsMeleeApproachMovement)
			{
				flag6 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModMeleeApproachMovementEnabled"), ctx.Text.MeleeApproachMovement, ctx.Text.MeleeApproachMovementDescription, ctx.Runtime.MeleeApproachMovementEnabled, delegate(bool value)
				{
					ctx.Runtime.SetCombatPresentationMode(CombatPresentationMode.Custom, persist: true);
					ctx.Runtime.SetMeleeApproachMovementEnabled(value, persist: true);
				}, !flag4 || ctx.Runtime.ShowCardPlayPresentation);
				AssignCharacterCategory(ctx, "CznModMeleeApproachMovementEnabled", "combat");
			}
			if (ctx.Runtime.SupportsOfficialAttackTiming)
			{
				flag6 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModOfficialAttackTimingEnabled"), ctx.Text.OfficialAttackTiming, ctx.Text.OfficialAttackTimingDescription, ctx.Runtime.OfficialAttackTimingEnabled, delegate(bool value)
				{
					ctx.Runtime.SetCombatPresentationMode(CombatPresentationMode.Custom, persist: true);
					ctx.Runtime.SetOfficialAttackTimingEnabled(value, persist: true);
				}, !flag4 || ctx.Runtime.ShowCardPlayPresentation);
				AssignCharacterCategory(ctx, "CznModOfficialAttackTimingEnabled", "combat");
			}
		}
		if (flag5)
		{
			flag6 |= TryCreateSubHeaderSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModLeftSideCardPresentationHeader"), ctx.Text.LeftSideCardPresentationHeader);
			AssignCharacterCategory(ctx, "CznModLeftSideCardPresentationHeader", "combat");
			flag6 |= TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModBattleReadyEnabled"), ctx.Text.ShowBattleReadyOverlay, ctx.Text.ShowBattleReadyOverlayDescription, new _003C_003Ez__ReadOnlyArray<(int, string)>(new(int, string)[3]
			{
				(0, ctx.Text.BattleReadyOverlayOff),
				(1, ctx.Text.BattleReadyOverlayAllSupportedCards),
				(2, ctx.Text.BattleReadyOverlayCutInCardsOnly)
			}), (int)ctx.Runtime.BattleReadyOverlayMode, delegate(int id)
			{
				ctx.Runtime.SetBattleReadyOverlayMode((LeftSideCardPresentationMode)id, persist: true);
				RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
			});
			AssignCharacterCategory(ctx, "CznModBattleReadyEnabled", "combat");
			bool enabled = ctx.Runtime.BattleReadyOverlayMode != LeftSideCardPresentationMode.Off;
			flag6 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModBattleReadyScale"), ctx.Text.BattleReadyScale, string.Empty, "CznModBattleReadyScaleSlider", 50.0, 200.0, 5.0, Math.Round((double)ctx.Runtime.BattleReadyScale * 100.0), "%", 0.0, delegate(double value)
			{
				ctx.Runtime.SetBattleReadyScale((float)(value / 100.0), persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModBattleReadyScale", "combat");
			flag6 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModBattleReadyOpacity"), ctx.Text.BattleReadyOpacity, string.Empty, "CznModBattleReadyOpacitySlider", 50.0, 100.0, 5.0, Math.Round((double)ctx.Runtime.BattleReadyOpacity * 100.0), "%", 0.0, delegate(double value)
			{
				ctx.Runtime.SetBattleReadyOpacity((float)(value / 100.0), persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModBattleReadyOpacity", "combat");
			flag6 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModBattleReadyOffsetX"), ctx.Text.BattleReadyOffsetX, string.Empty, "CznModBattleReadyOffsetXSlider", 0.0, 800.0, 10.0, Math.Round((double)ctx.Runtime.BattleReadyOffsetX + 400.0), "px", -400.0, delegate(double value)
			{
				ctx.Runtime.SetBattleReadyOffsetX((float)value, persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModBattleReadyOffsetX", "combat");
			flag6 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModBattleReadyOffsetY"), ctx.Text.BattleReadyOffsetY, string.Empty, "CznModBattleReadyOffsetYSlider", 0.0, 800.0, 10.0, Math.Round((double)ctx.Runtime.BattleReadyOffsetY + 400.0), "px", -400.0, delegate(double value)
			{
				ctx.Runtime.SetBattleReadyOffsetY((float)value, persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModBattleReadyOffsetY", "combat");
			flag6 |= TryCreateActionSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModBattleReadyResetPosition"), ctx.Text.ResetBattleReadyPosition, ctx.Text.ResetBattleReadyPositionDescription, ctx.Text.ResetDefaults, delegate
			{
				ctx.Runtime.SetBattleReadyOffsetX(0f, persist: true);
				ctx.Runtime.SetBattleReadyOffsetY(0f, persist: true);
				RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModBattleReadyResetPosition", "combat");
		}
		return flag6;
	}

	private static bool TryCreateCombatEffectSections(SettingsUiBuildContext ctx)
	{
		bool flag = Supports(ctx, SkinSettingsUiCapabilities.Effects);
		bool flag2 = Supports(ctx, SkinSettingsUiCapabilities.Particles);
		bool flag3 = Supports(ctx, SkinSettingsUiCapabilities.CombatSfx);
		bool flag4 = Supports(ctx, SkinSettingsUiCapabilities.EffectVolume);
		bool flag5 = Supports(ctx, SkinSettingsUiCapabilities.EffectBrightness);
		bool flag6 = Supports(ctx, SkinSettingsUiCapabilities.EffectOpacity);
		if (!flag && !flag2 && !flag3 && !flag4 && !flag5 && !flag6)
		{
			return false;
		}
		bool flag7 = TryCreateSubHeaderSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModCombatEffectsHeader"), ctx.Text.CombatEffectsHeader);
		AssignCharacterCategory(ctx, "CznModCombatEffectsHeader", "effects");
		if (flag)
		{
			flag7 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModCharacterEffectsEnabled"), ctx.Runtime.CharacterEffectsLabel, ctx.Text.EffectsEnabledDescription, ctx.Runtime.EffectsEnabled, delegate(bool value)
			{
				ctx.Runtime.SetEffectsEnabled(value, persist: true);
				RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
			}, ctx.Global.GlobalEffectsEnabled);
			AssignCharacterCategory(ctx, "CznModCharacterEffectsEnabled", "effects");
		}
		bool flag8 = ctx.Global.GlobalEffectsEnabled && (!flag || ctx.Runtime.EffectsEnabled);
		if (flag2)
		{
			flag7 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModParticlesEnabled"), ctx.Text.ParticlesEnabled, ctx.Text.ParticlesEnabledDescription, ctx.Runtime.ParticlesEnabled, delegate(bool value)
			{
				ctx.Runtime.SetParticlesEnabled(value, persist: true);
			}, flag8 && ctx.Global.GlobalParticlesEnabled);
			AssignCharacterCategory(ctx, "CznModParticlesEnabled", "effects");
		}
		if (flag3)
		{
			flag7 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModCombatSfxEnabled"), ctx.Text.CombatSfxEnabled, string.Empty, ctx.Runtime.CombatSfxEnabled, delegate(bool value)
			{
				ctx.Runtime.SetCombatSfxEnabled(value, persist: true);
				RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
			}, ctx.Global.GlobalCombatSfxEnabled);
			AssignCharacterCategory(ctx, "CznModCombatSfxEnabled", "effects");
		}
		bool enabled = ctx.Global.GlobalCombatSfxEnabled && (!flag3 || ctx.Runtime.CombatSfxEnabled);
		if (flag4)
		{
			flag7 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModEffectVolume"), ctx.Text.EffectVolume, string.Empty, "CznModEffectVolumeSlider", 0.0, 200.0, 5.0, Math.Round((double)ctx.Runtime.EffectVolume * 100.0), "%", 0.0, delegate(double value)
			{
				ctx.Runtime.SetEffectVolume((float)(value / 100.0), persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModEffectVolume", "effects");
		}
		if (flag5)
		{
			flag7 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModEffectBrightness"), ctx.Text.EffectBrightness, string.Empty, "CznModEffectBrightnessSlider", 25.0, 200.0, 5.0, Math.Round((double)ctx.Runtime.EffectBrightness * 100.0), "%", 0.0, delegate(double value)
			{
				ctx.Runtime.SetEffectBrightness((float)(value / 100.0), persist: true);
			}, flag8);
			AssignCharacterCategory(ctx, "CznModEffectBrightness", "effects");
		}
		if (flag6)
		{
			flag7 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModEffectOpacity"), ctx.Text.EffectOpacity, string.Empty, "CznModEffectOpacitySlider", 25.0, 100.0, 5.0, Math.Round((double)ctx.Runtime.EffectOpacity * 100.0), "%", 0.0, delegate(double value)
			{
				ctx.Runtime.SetEffectOpacity((float)(value / 100.0), persist: true);
			}, flag8);
			AssignCharacterCategory(ctx, "CznModEffectOpacity", "effects");
		}
		return flag7;
	}

	private static bool TryCreateAudioSettingsSections(SettingsUiBuildContext ctx)
	{
		bool flag = Supports(ctx, SkinSettingsUiCapabilities.Voice);
		bool flag2 = Supports(ctx, SkinSettingsUiCapabilities.VoiceVolume);
		bool flag3 = Supports(ctx, SkinSettingsUiCapabilities.VoiceCooldown);
		if (!flag && !flag2 && !flag3)
		{
			return false;
		}
		bool flag4 = TryCreateSubHeaderSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModAudioHeader"), ctx.Text.AudioSettingsHeader);
		AssignCharacterCategory(ctx, "CznModAudioHeader", "audio");
		if (flag)
		{
			flag4 |= TryCreateToggleSection(ctx, CharacterSectionName(ctx, "CznModCharacterVoiceEnabled"), ctx.Runtime.CharacterVoiceLabel, string.Empty, ctx.Runtime.VoiceEnabled, delegate(bool value)
			{
				ctx.Runtime.SetVoiceEnabled(value, persist: true);
				RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
			}, ctx.Global.GlobalVoiceEnabled);
			AssignCharacterCategory(ctx, "CznModCharacterVoiceEnabled", "audio");
		}
		bool enabled = ctx.Global.GlobalVoiceEnabled && (!flag || ctx.Runtime.VoiceEnabled);
		if (flag2)
		{
			flag4 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModVoiceVolume"), ctx.Text.VoiceVolume, string.Empty, "CznModVoiceSlider", 0.0, 100.0, 5.0, Math.Round((double)ctx.Runtime.VoiceVolume * 100.0), "%", 0.0, delegate(double value)
			{
				ctx.Runtime.SetVoiceVolume((float)(value / 100.0), persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModVoiceVolume", "audio");
		}
		if (flag3)
		{
			flag4 |= TryCreateSliderSection(ctx, CharacterSectionName(ctx, "CznModVoiceCooldown"), ctx.Text.VoiceCooldown, ctx.Text.VoiceCooldownDescription, "CznModVoiceCooldownSlider", 0.0, 10.0, 0.5, ctx.Runtime.VoiceCooldownSeconds, "s", 0.0, delegate(double value)
			{
				ctx.Runtime.SetVoiceCooldownSeconds((float)value, persist: true);
			}, enabled);
			AssignCharacterCategory(ctx, "CznModVoiceCooldown", "audio");
		}
		return flag4;
	}

	private static void AssignCharacterCategory(SettingsUiBuildContext ctx, string baseName, string categoryId)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Node nodeOrNull = ((Node)ctx.VBox).GetNodeOrNull(new NodePath(CharacterSectionName(ctx, baseName)));
		if (nodeOrNull != null)
		{
			((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("czn_mod_settings_character_category"), Variant.op_Implicit(categoryId));
		}
	}

	private static bool Supports(SettingsUiBuildContext ctx, SkinSettingsUiCapabilities capability)
	{
		return (ctx.Runtime.Capabilities & capability) == capability;
	}

	private static string CharacterSectionName(SettingsUiBuildContext ctx, string baseName)
	{
		return baseName + "_" + BuildCharacterSectionSuffix(ctx);
	}

	private static string BuildCharacterSectionSuffix(SettingsUiBuildContext ctx)
	{
		return BuildCharacterSectionSuffix(ctx.Runtime);
	}

	private static string BuildCharacterSectionSuffix(ISkinSettingsUiRuntime runtime)
	{
		string text = runtime.CharacterSettingsKey;
		if (string.IsNullOrWhiteSpace(text))
		{
			text = runtime.CharacterSettingsHeader + "_" + runtime.CharacterSkinTarget;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		string text2 = text;
		foreach (char c in text2)
		{
			if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
			{
				stringBuilder.Append(c);
			}
			else if (stringBuilder.Length > 0 && stringBuilder[stringBuilder.Length - 1] != '_')
			{
				stringBuilder.Append('_');
			}
		}
		if (stringBuilder.Length == 0)
		{
			return "Character";
		}
		return stringBuilder.ToString().TrimEnd('_');
	}

	private static bool EnsureModSettingsTabAndPanel(NSettingsScreen screen, ISkinSettingsUiText text, out NSettingsPanel? panel)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Expected O, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Expected O, but got Unknown
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		panel = null;
		NSettingsTabManager val = ((Node)screen).GetNodeOrNull<NSettingsTabManager>(new NodePath("SettingsTabManager")) ?? ((Node)screen).GetNodeOrNull<NSettingsTabManager>(new NodePath("%SettingsTabManager"));
		if (val == null)
		{
			SkinLog.Warn("Settings UI inject skipped: missing SettingsTabManager.");
			return false;
		}
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(new NodePath("ScrollContainer/Mask/Clipper"));
		if (nodeOrNull == null)
		{
			SkinLog.Warn("Settings UI inject skipped: missing settings clipper.");
			return false;
		}
		panel = ((Node)nodeOrNull).GetNodeOrNull<NSettingsPanel>(new NodePath("CznModSettingsPanel"));
		if (panel == null)
		{
			NSettingsPanel val2 = ((Node)screen).GetNodeOrNull<NSettingsPanel>(new NodePath("%SoundSettings")) ?? ((Node)screen).GetNodeOrNull<NSettingsPanel>(new NodePath("ScrollContainer/Mask/Clipper/SoundSettings"));
			if (val2 == null)
			{
				SkinLog.Warn("Settings UI inject skipped: missing SoundSettings template panel.");
				return false;
			}
			Node obj = ((Node)val2).Duplicate(15);
			panel = (NSettingsPanel?)(object)((obj is NSettingsPanel) ? obj : null);
			if (panel == null)
			{
				SkinLog.Warn("Settings UI inject skipped: duplicate settings panel failed.");
				return false;
			}
			((Node)panel).Name = StringName.op_Implicit("CznModSettingsPanel");
			((Node)panel).UniqueNameInOwner = true;
			((CanvasItem)panel).Visible = false;
			VBoxContainer nodeOrNull2 = ((Node)panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
			if (nodeOrNull2 == null)
			{
				SkinLog.Warn("Settings UI inject skipped: duplicated panel missing VBoxContainer.");
				return false;
			}
			((Control)nodeOrNull2).CustomMinimumSize = new Vector2(1012f, 0f);
			((Control)nodeOrNull2).SizeFlagsHorizontal = (SizeFlags)4;
			((Control)nodeOrNull2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
			NSettingsPanel createdPanel = panel;
			VBoxContainer createdVBox = nodeOrNull2;
			((Control)nodeOrNull2).Resized += delegate
			{
				RefreshPanelLayout(createdPanel, createdVBox);
			};
			((Node)nodeOrNull).AddChild((Node)(object)panel, false, (InternalMode)0);
			GodotNodeUtil.ClearChildren((Node)(object)nodeOrNull2);
		}
		NSettingsTab val3 = ((Node)val).GetNodeOrNull<NSettingsTab>(new NodePath("CznModSettingsTab"));
		if (val3 == null)
		{
			PackedScene val4 = ResourceLoader.Load<PackedScene>("res://scenes/screens/settings_tab.tscn", (string)null, (CacheMode)1);
			if (val4 == null)
			{
				SkinLog.Warn("Settings UI inject skipped: missing settings tab scene.");
				return false;
			}
			val3 = val4.Instantiate<NSettingsTab>((GenEditState)0);
			((Node)val3).Name = StringName.op_Implicit("CznModSettingsTab");
			((Node)val3).UniqueNameInOwner = true;
			Node val5 = ((Node)val).GetNodeOrNull(new NodePath("RightTabIcon")) ?? ((Node)val).GetNodeOrNull(new NodePath("RightTriggerIcon"));
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			if (val5 != null)
			{
				((Node)val).MoveChild((Node)(object)val3, val5.GetIndex(false));
			}
		}
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		SetTabLabelDeferred(val3, text);
		EnsureTabBinding(val, val3, panel);
		return true;
	}

	private static void EnsureTabBinding(NSettingsTabManager tabManager, NSettingsTab tab, NSettingsPanel panel)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (AccessTools.Field(typeof(NSettingsTabManager), "_tabs")?.GetValue(tabManager) is Dictionary<NSettingsTab, NSettingsPanel> dictionary)
			{
				dictionary[tab] = panel;
			}
			if (!((GodotObject)tab).HasMeta(StringName.op_Implicit("czn_mod_settings_tab_bound")))
			{
				((GodotObject)tab).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
				{
					SwitchTabTo(tabManager, tab);
				}), 0u);
				((GodotObject)tab).SetMeta(StringName.op_Implicit("czn_mod_settings_tab_bound"), Variant.op_Implicit(true));
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Settings tab binding failed: " + SkinLog.FormatException(ex));
		}
	}

	private static void SwitchTabTo(NSettingsTabManager tabManager, NSettingsTab tab)
	{
		try
		{
			AccessTools.Method(typeof(NSettingsTabManager), "SwitchTabTo", (Type[])null, (Type[])null)?.Invoke(tabManager, new object[1] { tab });
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Settings tab switch failed: " + SkinLog.FormatException(ex));
		}
	}

	private static void SetTabLabelDeferred(NSettingsTab tab, ISkinSettingsUiText text)
	{
		GodotScheduler.Deferred(delegate
		{
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)tab) && ((Node)tab).IsNodeReady())
			{
				tab.SetLabel(text.CznSettingsTab);
			}
		});
	}

	private static void RebuildSettingsPanelDeferred(SettingsUiBuildContext ctx, RebuildScope scope)
	{
		GodotScheduler.Deferred(delegate
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)ctx.Screen) && GodotNodeUtil.IsAlive((GodotObject?)(object)ctx.Panel))
			{
				VBoxContainer nodeOrNull = ((Node)ctx.Panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
				if (nodeOrNull != null)
				{
					if (scope == RebuildScope.Character)
					{
						int insertionIndex = RemoveCharacterSections(ctx.VBox, ctx.Runtime);
						TryCreateCharacterSettingsSections(ctx);
						MoveCharacterSectionsToIndex(ctx.VBox, ctx.Runtime, insertionIndex);
					}
					else
					{
						if (scope == RebuildScope.Global || scope == RebuildScope.Shared)
						{
							int insertionIndex2 = RemoveGlobalSections(ctx.VBox);
							TryCreateGlobalSettingsSections(ctx);
							MoveGlobalSectionsToIndex(ctx.VBox, insertionIndex2);
						}
						if (scope == RebuildScope.Multiplayer || scope == RebuildScope.Shared)
						{
							int insertionIndex3 = RemoveMultiplayerSections(ctx.VBox);
							TryCreateMultiplayerSettingsSections(ctx);
							MoveMultiplayerSectionsToIndex(ctx.VBox, insertionIndex3);
						}
						if (scope == RebuildScope.Extensions || scope == RebuildScope.Shared)
						{
							int insertionIndex4 = RemoveExtensionSections(ctx.VBox);
							TryCreateExtensionSettingsSections(ctx.Extensions);
							MoveExtensionSectionsToIndex(ctx.VBox, insertionIndex4);
						}
					}
					RefreshPanelLayout(ctx.Panel, nodeOrNull);
					RefreshGlobalCategorySelection(ctx.Panel);
					RefreshCharacterCategorySelection(ctx.Panel);
					RefreshFocusNeighbors(ctx.Panel);
				}
			}
		});
	}

	private static void RebuildExtensionSectionsDeferred(SettingsUiExtensionContext ctx)
	{
		GodotScheduler.Deferred(delegate
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)ctx.Screen) && GodotNodeUtil.IsAlive((GodotObject?)(object)ctx.Panel))
			{
				VBoxContainer nodeOrNull = ((Node)ctx.Panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
				if (nodeOrNull != null)
				{
					int insertionIndex = RemoveExtensionSections(ctx.VBox);
					TryCreateExtensionSettingsSections(ctx);
					MoveExtensionSectionsToIndex(ctx.VBox, insertionIndex);
					RefreshPanelLayout(ctx.Panel, nodeOrNull);
					RefreshGlobalCategorySelection(ctx.Panel);
					RefreshCharacterCategorySelection(ctx.Panel);
					RefreshFocusNeighbors(ctx.Panel);
				}
			}
		});
	}

	private static void RebuildCurrentCharacterPageDeferred(SettingsUiBuildContext ctx)
	{
		GodotScheduler.Deferred(delegate
		{
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Expected O, but got Unknown
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Expected O, but got Unknown
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)ctx.Screen) && GodotNodeUtil.IsAlive((GodotObject?)(object)ctx.Panel))
			{
				VBoxContainer nodeOrNull = ((Node)ctx.Panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
				if (nodeOrNull != null && TryGetSettingsShell(ctx.Panel, out SettingsUiShell shell))
				{
					VBoxContainer nodeOrNull2 = ((Node)shell.ContentHost).GetNodeOrNull<VBoxContainer>(new NodePath(CharacterPageName(ctx.Runtime)));
					if (nodeOrNull2 != null)
					{
						SettingsUiBuildContext ctx2 = new SettingsUiBuildContext(ctx.Screen, ctx.Panel, nodeOrNull2, ctx.TemplateLabel, ctx.SliderScene, ctx.TickboxVisualScene, ctx.Runtime, ctx.Global, ctx.Text);
						int insertionIndex = RemoveCharacterSections(nodeOrNull2, ctx.Runtime);
						TryCreateCharacterSettingsSections(ctx2);
						MoveCharacterSectionsToIndex(nodeOrNull2, ctx.Runtime, insertionIndex);
						RefreshPanelLayout(ctx.Panel, nodeOrNull);
						RefreshGlobalCategorySelection(ctx.Panel);
						RefreshCharacterCategorySelection(ctx.Panel);
						RefreshFocusNeighbors(ctx.Panel);
					}
				}
			}
		});
	}

	private static int RemoveGlobalSections(VBoxContainer vbox)
	{
		HashSet<string> globalSectionNames = new HashSet<string>(StringComparer.Ordinal) { "CznModGlobalHeader", "CznModGlobalParticlesEnabled", "CznModGlobalCombatSfxEnabled", "CznModGlobalEffectsEnabled", "CznModGlobalVoiceEnabled", "CznModDebugLogEnabled", "CznModGlobalCombatPresentationApply" };
		return RemoveSections(vbox, (Node child) => globalSectionNames.Contains(((object)child.Name).ToString()));
	}

	private static int RemoveCharacterSections(VBoxContainer vbox, ISkinSettingsUiRuntime runtime)
	{
		string currentSuffix = "_" + BuildCharacterSectionSuffix(runtime);
		return RemoveSections(vbox, (Node child) => ((object)child.Name).ToString().EndsWith(currentSuffix, StringComparison.Ordinal));
	}

	private static int RemoveMultiplayerSections(VBoxContainer vbox)
	{
		HashSet<string> multiplayerSectionNames = MultiplayerSectionNames();
		return RemoveSections(vbox, (Node child) => multiplayerSectionNames.Contains(((object)child.Name).ToString()));
	}

	private static int RemoveExtensionSections(VBoxContainer vbox)
	{
		return RemoveSections(vbox, (Node child) => ((object)child.Name).ToString().StartsWith("CznModExtension_", StringComparison.Ordinal));
	}

	private static int RemoveSections(VBoxContainer vbox, Func<Node, bool> shouldRemove)
	{
		int num = -1;
		foreach (Node child in ((Node)vbox).GetChildren(false))
		{
			if (shouldRemove(child))
			{
				num = ((num < 0) ? child.GetIndex(false) : Math.Min(num, child.GetIndex(false)));
				((Node)vbox).RemoveChild(child);
				child.QueueFree();
			}
		}
		return num;
	}

	private static void MoveGlobalSectionsToIndex(VBoxContainer vbox, int insertionIndex)
	{
		HashSet<string> globalSectionNames = new HashSet<string>(StringComparer.Ordinal) { "CznModGlobalHeader", "CznModGlobalParticlesEnabled", "CznModGlobalCombatSfxEnabled", "CznModGlobalEffectsEnabled", "CznModGlobalVoiceEnabled", "CznModDebugLogEnabled", "CznModGlobalCombatPresentationApply" };
		MoveSectionsToIndex(vbox, (Node child) => globalSectionNames.Contains(((object)child.Name).ToString()), insertionIndex);
	}

	private static void MoveCharacterSectionsToIndex(VBoxContainer vbox, ISkinSettingsUiRuntime runtime, int insertionIndex)
	{
		string currentSuffix = "_" + BuildCharacterSectionSuffix(runtime);
		MoveSectionsToIndex(vbox, (Node child) => ((object)child.Name).ToString().EndsWith(currentSuffix, StringComparison.Ordinal), insertionIndex);
	}

	private static void MoveMultiplayerSectionsToIndex(VBoxContainer vbox, int insertionIndex)
	{
		HashSet<string> multiplayerSectionNames = MultiplayerSectionNames();
		MoveSectionsToIndex(vbox, (Node child) => multiplayerSectionNames.Contains(((object)child.Name).ToString()), insertionIndex);
	}

	private static void MoveExtensionSectionsToIndex(VBoxContainer vbox, int insertionIndex)
	{
		MoveSectionsToIndex(vbox, (Node child) => ((object)child.Name).ToString().StartsWith("CznModExtension_", StringComparison.Ordinal), insertionIndex);
	}

	private static HashSet<string> MultiplayerSectionNames()
	{
		return new HashSet<string>(StringComparer.Ordinal)
		{
			"CznModMultiplayerHeader", "CznModMultiplayerIntro", "CznModTeammatePresentationEnabled", "CznModMultiplayerSummary", "CznModTeammateVisualHeader", "CznModTeammateAudioHeader", "CznModTeammateEffectsEnabled", "CznModTeammateCombatSfxEnabled", "CznModTeammateVoiceEnabled", "CznModTeammateVoiceCooldown",
			"CznModTeammateCutInEnabled", "CznModTeammateAttackFullScreenPresentationMode"
		};
	}

	private static void MoveSectionsToIndex(VBoxContainer vbox, Func<Node, bool> shouldMove, int insertionIndex)
	{
		if (insertionIndex < 0)
		{
			return;
		}
		List<Node> list = new List<Node>();
		foreach (Node child in ((Node)vbox).GetChildren(false))
		{
			if (shouldMove(child))
			{
				list.Add(child);
			}
		}
		int num = Math.Min(insertionIndex, Math.Max(0, ((Node)vbox).GetChildCount(false) - list.Count));
		foreach (Node item in list)
		{
			((Node)vbox).MoveChild(item, Math.Min(num, ((Node)vbox).GetChildCount(false) - 1));
			num++;
		}
	}

	private static string FormatCharacterHeader(string characterName, string skinTarget)
	{
		if (!string.IsNullOrWhiteSpace(skinTarget))
		{
			return characterName + " - " + skinTarget;
		}
		return characterName;
	}

	private static bool TryCreateCharacterStatusSection(SettingsUiBuildContext ctx)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Expected O, but got Unknown
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		string text = CharacterSectionName(ctx, "CznModCharacterStatus");
		if (((Node)ctx.VBox).GetNodeOrNull(new NodePath(text)) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection(text, 0, 12, 82f);
		PanelContainer val2 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(858f, 74f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateStatusHeaderStyle());
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(834f, 62f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		ColorRect val4 = new ColorRect
		{
			Color = CreamGold,
			CustomMinimumSize = new Vector2(5f, 54f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val4).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		Control val5 = CreateCharacterStatusAvatar(ctx.TemplateLabel, ctx.Runtime);
		VBoxContainer val6 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(562f, 58f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val6).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val6).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		Label val7 = CreateTextLabel(ctx.TemplateLabel, ctx.Runtime.CharacterSettingsHeader, 4, CreamGold);
		((Control)val7).CustomMinimumSize = new Vector2(552f, 32f);
		val7.VerticalAlignment = (VerticalAlignment)2;
		((Node)val6).AddChild((Node)(object)val7, false, (InternalMode)0);
		if (!string.IsNullOrWhiteSpace(ctx.Runtime.CharacterSkinTarget))
		{
			Label val8 = CreateTextLabel(ctx.TemplateLabel, ctx.Runtime.CharacterSkinTarget, -2, TextColor);
			((Control)val8).CustomMinimumSize = new Vector2(552f, 24f);
			val8.VerticalAlignment = (VerticalAlignment)0;
			((Node)val6).AddChild((Node)(object)val8, false, (InternalMode)0);
		}
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)CreateResetButton(ctx.TemplateLabel, delegate
		{
			ctx.Runtime.ResetCharacterToDefaults(persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
		}, ctx.Text), false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)ctx.VBox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static Control CreateCharacterStatusAvatar(RichTextLabel templateLabel, ISkinSettingsUiRuntime runtime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		CenterContainer val = new CenterContainer
		{
			CustomMinimumSize = new Vector2(58f, 58f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		Texture2D val2 = (string.IsNullOrWhiteSpace(runtime.CharacterSettingsAvatarPath) ? null : ResourceLoader.Load<Texture2D>(runtime.CharacterSettingsAvatarPath, (string)null, (CacheMode)1));
		if (val2 != null)
		{
			TextureRect val3 = new TextureRect
			{
				Texture = val2,
				CustomMinimumSize = new Vector2(50f, 50f),
				MouseFilter = (MouseFilterEnum)2,
				ExpandMode = (ExpandModeEnum)1,
				StretchMode = (StretchModeEnum)5
			};
			((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			return (Control)(object)val;
		}
		Label val4 = CreateTextLabel(templateLabel, BuildNavInitials(runtime.CharacterSettingsHeader), 2, CreamGold);
		((Control)val4).CustomMinimumSize = new Vector2(50f, 50f);
		val4.HorizontalAlignment = (HorizontalAlignment)1;
		val4.VerticalAlignment = (VerticalAlignment)1;
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		return (Control)(object)val;
	}

	private static bool TryCreateCharacterCategoryNavSection(SettingsUiBuildContext ctx)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		string text = CharacterSectionName(ctx, "CznModCharacterCategoryNav");
		if (((Node)ctx.VBox).GetNodeOrNull(new NodePath(text)) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection(text, 0, 12, 46f);
		HBoxContainer val2 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(858f, 38f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)val2).AddChild((Node)(object)CreateCategoryButton(ctx, "visual", ctx.Text.VisualSettingsHeader), false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)CreateCategoryButton(ctx, "combat", ctx.Text.CardAnimationHeader), false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)CreateCategoryButton(ctx, "effects", ctx.Text.CombatEffectsHeader), false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)CreateCategoryButton(ctx, "audio", ctx.Text.AudioSettingsHeader), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)ctx.VBox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static Button CreateCategoryButton(SettingsUiBuildContext ctx, string categoryId, string label)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		CznSettingsButton cznSettingsButton = new CznSettingsButton();
		((Button)cznSettingsButton).Text = label;
		((BaseButton)cznSettingsButton).ToggleMode = false;
		((Control)cznSettingsButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsButton).CustomMinimumSize = new Vector2(206f, 36f);
		((Control)cznSettingsButton).SizeFlagsHorizontal = (SizeFlags)4;
		((GodotObject)cznSettingsButton).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((GodotObject)cznSettingsButton).SetMeta(StringName.op_Implicit("czn_mod_settings_category_id"), Variant.op_Implicit(categoryId));
		((Control)cznSettingsButton).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)ctx.TemplateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)cznSettingsButton).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(ctx.TemplateLabel, -2));
		((Control)cznSettingsButton).AddThemeColorOverride(StringName.op_Implicit("font_color"), CreamGold);
		((BaseButton)cznSettingsButton).Pressed += delegate
		{
			SetSelectedCharacterCategory(ctx.Panel, categoryId);
		};
		ApplyCategoryButtonStyle((Button)(object)cznSettingsButton, string.Equals(GetSelectedCharacterCategory(ctx.Panel), categoryId, StringComparison.Ordinal));
		return CznSettingsInputControls.AttachNativeOwner((Button)(object)cznSettingsButton);
	}

	private static bool TryCreateCombatPresentationModeSection(SettingsUiBuildContext ctx)
	{
		bool result = TryCreateControlSection(ctx.VBox, ctx.TemplateLabel, CharacterSectionName(ctx, "CznModCombatPresentationMode"), ctx.Text.CombatPresentationMode, ctx.Text.CombatPresentationModeDescription, delegate(float rowHeight)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			HBoxContainer val = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(334f, rowHeight),
				MouseFilter = (MouseFilterEnum)2
			};
			((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)val).AddChild((Node)(object)CreateCombatModeButton(ctx, CombatPresentationMode.Full, ctx.Text.CombatPresentationFull), false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)CreateCombatModeButton(ctx, CombatPresentationMode.Fast, ctx.Text.CombatPresentationFast), false, (InternalMode)0);
			if (Supports(ctx, SkinSettingsUiCapabilities.CustomCombatPresentation))
			{
				((Node)val).AddChild((Node)(object)CreateCombatModeButton(ctx, CombatPresentationMode.Custom, ctx.Text.CombatPresentationCustom), false, (InternalMode)0);
			}
			return (Control)(object)WrapControlCenter((Control)(object)val, 334f, rowHeight);
		});
		AssignCharacterCategory(ctx, "CznModCombatPresentationMode", "combat");
		return result;
	}

	private static bool TryCreateCombatPresentationSummarySection(SettingsUiBuildContext ctx)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (ctx.Runtime.CombatPresentationMode == CombatPresentationMode.Custom)
		{
			return false;
		}
		bool flag = Supports(ctx, SkinSettingsUiCapabilities.CardPlayPresentation);
		if (!flag && !ctx.Runtime.SupportsMeleeApproachMovement && !ctx.Runtime.SupportsOfficialAttackTiming)
		{
			return false;
		}
		string text = CharacterSectionName(ctx, "CznModCombatPresentationSummary");
		if (((Node)ctx.VBox).GetNodeOrNull(new NodePath(text)) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection(text, 8, 12, 30f);
		PanelContainer val2 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(858f, 28f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateCombatSummaryStatusStyle());
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(842f, 26f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		bool flag2 = false;
		if (flag)
		{
			AddCombatSummaryStatusItem(val3, ctx.TemplateLabel, ctx.Text.ShowCardPlayPresentation, ctx.Runtime.ShowCardPlayPresentation);
			flag2 = true;
		}
		if (ctx.Runtime.SupportsMeleeApproachMovement)
		{
			if (flag2)
			{
				AddCombatSummarySeparator(val3, ctx.TemplateLabel);
			}
			AddCombatSummaryStatusItem(val3, ctx.TemplateLabel, ctx.Text.MeleeApproachMovement, ctx.Runtime.MeleeApproachMovementEnabled);
			flag2 = true;
		}
		if (ctx.Runtime.SupportsOfficialAttackTiming)
		{
			if (flag2)
			{
				AddCombatSummarySeparator(val3, ctx.TemplateLabel);
			}
			AddCombatSummaryStatusItem(val3, ctx.TemplateLabel, ctx.Text.OfficialAttackTiming, ctx.Runtime.OfficialAttackTimingEnabled);
		}
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)ctx.VBox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static void AddCombatSummaryStatusItem(HBoxContainer row, RichTextLabel templateLabel, string labelText, bool active)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Label val = CreateTextLabel(templateLabel, labelText, -6, active ? CreamGold : DimText);
		((Control)val).CustomMinimumSize = new Vector2(0f, 26f);
		val.HorizontalAlignment = (HorizontalAlignment)0;
		val.VerticalAlignment = (VerticalAlignment)1;
		((Node)row).AddChild((Node)(object)val, false, (InternalMode)0);
	}

	private static void AddCombatSummarySeparator(HBoxContainer row, RichTextLabel templateLabel)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Label val = CreateTextLabel(templateLabel, "/", -6, new Color(DimText.R, DimText.G, DimText.B, 0.5f));
		((Control)val).CustomMinimumSize = new Vector2(10f, 26f);
		val.HorizontalAlignment = (HorizontalAlignment)1;
		val.VerticalAlignment = (VerticalAlignment)1;
		((Node)row).AddChild((Node)(object)val, false, (InternalMode)0);
	}

	private static Button CreateCombatModeButton(SettingsUiBuildContext ctx, CombatPresentationMode mode, string label)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ctx.Runtime.CombatPresentationMode == mode;
		CznSettingsButton cznSettingsButton = new CznSettingsButton();
		((Button)cznSettingsButton).Text = label;
		((BaseButton)cznSettingsButton).ToggleMode = false;
		((Control)cznSettingsButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsButton).CustomMinimumSize = new Vector2(106f, 36f);
		((Control)cznSettingsButton).SizeFlagsHorizontal = (SizeFlags)4;
		((GodotObject)cznSettingsButton).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)cznSettingsButton).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)ctx.TemplateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)cznSettingsButton).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(ctx.TemplateLabel, -2));
		((BaseButton)cznSettingsButton).Pressed += delegate
		{
			if (mode == CombatPresentationMode.Custom)
			{
				ctx.Runtime.SetCombatPresentationMode(mode, persist: true);
			}
			else
			{
				ApplyCombatPresentationModePreset(ctx, mode);
			}
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
		};
		((GodotObject)cznSettingsButton).SetMeta(StringName.op_Implicit("czn_mod_settings_focus_preferred"), Variant.op_Implicit(flag));
		ApplyCombatModeButtonStyle((Button)(object)cznSettingsButton, flag);
		return CznSettingsInputControls.AttachNativeOwner((Button)(object)cznSettingsButton);
	}

	private static void ApplyCombatPresentationModePreset(SettingsUiBuildContext ctx, CombatPresentationMode mode)
	{
		ctx.Runtime.SetCombatPresentationMode(mode, persist: true);
	}

	private static void UpdateTickboxVisualState(Control visual, bool ticked)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)visual))
		{
			CanvasItem nodeOrNull = ((Node)visual).GetNodeOrNull<CanvasItem>(new NodePath("Ticked"));
			CanvasItem nodeOrNull2 = ((Node)visual).GetNodeOrNull<CanvasItem>(new NodePath("NotTicked"));
			if (nodeOrNull != null)
			{
				nodeOrNull.Visible = ticked;
			}
			if (nodeOrNull2 != null)
			{
				nodeOrNull2.Visible = !ticked;
			}
		}
	}

	private static void AddTransparentButtonStyles(Button button)
	{
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)CreateTransparentStyle());
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)CreateTransparentStyle());
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)CreateTransparentStyle());
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)CreateTransparentStyle());
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("disabled"), (StyleBox)(object)CreateTransparentStyle());
	}

	private static void RefreshPanelLayout(NSettingsPanel panel, VBoxContainer vbox)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)panel) && GodotNodeUtil.IsAlive((GodotObject?)(object)vbox))
		{
			((Control)panel).CustomMinimumSize = new Vector2(((Control)panel).CustomMinimumSize.X, Math.Max(640f, ((Control)vbox).Size.Y + 160f));
		}
	}

	private static void RefreshPanelLayout(NSettingsPanel panel)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		VBoxContainer nodeOrNull = ((Node)panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
		if (nodeOrNull != null)
		{
			RefreshPanelLayout(panel, nodeOrNull);
		}
	}

	private static void RefreshFocusNeighbors(NSettingsPanel panel)
	{
		try
		{
			if (TryGetSettingsShell(panel, out SettingsUiShell shell))
			{
				List<Control> list = CollectSidebarFocusables(shell.Sidebar);
				Control val = ResolveSelectedNavigation(panel, list);
				Control val2 = ResolveSelectedPage(panel, shell.ContentHost);
				List<List<Control>> rows = new List<List<Control>>();
				if (val2 != null)
				{
					CollectFocusRows((Node)(object)val2, rows);
				}
				Control val3 = ResolveContentEntry(rows);
				ApplySidebarNavigation(list, val, val3);
				ApplyContentNavigation(rows, val);
				Control value = val ?? ((list.Count > 0) ? list[0] : null) ?? val3;
				AccessTools.Field(typeof(NSettingsPanel), "_firstControl")?.SetValue(panel, value);
			}
		}
		catch (Exception ex)
		{
			SkinLog.Warn("Settings focus graph refresh failed: " + SkinLog.FormatException(ex));
		}
	}

	private static List<Control> CollectSidebarFocusables(VBoxContainer sidebar)
	{
		List<Control> list = new List<Control>();
		foreach (Node child in ((Node)sidebar).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null)
			{
				Control val2 = CznSettingsInputControls.ResolveFocusOwner(val);
				if (IsFocusCandidate(val2))
				{
					list.Add(val2);
				}
			}
		}
		return list;
	}

	private static Control? ResolveSelectedNavigation(NSettingsPanel panel, IReadOnlyList<Control> sidebar)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		string selectedPage = GetSelectedPage(panel);
		foreach (Control item in sidebar)
		{
			Control val = CznSettingsInputControls.ResolveVisualControl(item);
			if (((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				Variant val2 = default(Variant);
				val2 = ((GodotObject)val).GetMeta(obj, val2);
				if (string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), selectedPage, StringComparison.Ordinal))
				{
					return item;
				}
			}
		}
		return null;
	}

	private static Control? ResolveSelectedPage(NSettingsPanel panel, VBoxContainer contentHost)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		string selectedPage = GetSelectedPage(panel);
		foreach (Node child in ((Node)contentHost).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((CanvasItem)val).Visible && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				Variant val2 = default(Variant);
				val2 = ((GodotObject)child).GetMeta(obj, val2);
				if (string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), selectedPage, StringComparison.Ordinal))
				{
					return val;
				}
			}
		}
		return null;
	}

	private static void CollectFocusRows(Node node, List<List<Control>> rows)
	{
		foreach (Node child in node.GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && !((CanvasItem)val).Visible)
			{
				continue;
			}
			HBoxContainer val2 = (HBoxContainer)(object)((child is HBoxContainer) ? child : null);
			if (val2 != null)
			{
				List<Control> list = CollectHorizontalFocusGroup(val2);
				if (list.Count > 1)
				{
					rows.Add(list);
					continue;
				}
			}
			Control val3 = (Control)(object)((child is Control) ? child : null);
			if (val3 != null && IsFocusCandidate(val3))
			{
				int num = 1;
				List<Control> list2 = new List<Control>(num);
				CollectionsMarshal.SetCount(list2, num);
				CollectionsMarshal.AsSpan(list2)[0] = val3;
				rows.Add(list2);
			}
			else
			{
				CollectFocusRows(child, rows);
			}
		}
	}

	private static List<Control> CollectHorizontalFocusGroup(HBoxContainer row)
	{
		List<Control> list = new List<Control>();
		foreach (Node child in ((Node)row).GetChildren(false))
		{
			Control val = FindFirstFocusable(child);
			if (val != null)
			{
				list.Add(val);
			}
		}
		return list;
	}

	private static Control? FindFirstFocusable(Node node)
	{
		Control val = (Control)(object)((node is Control) ? node : null);
		if (val != null && !((CanvasItem)val).Visible)
		{
			return null;
		}
		Control val2 = (Control)(object)((node is Control) ? node : null);
		if (val2 != null && IsFocusCandidate(val2))
		{
			return val2;
		}
		foreach (Node child in node.GetChildren(false))
		{
			Control val3 = FindFirstFocusable(child);
			if (val3 != null)
			{
				return val3;
			}
		}
		return null;
	}

	private static bool IsFocusCandidate(Control control)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I8
		if (((CanvasItem)control).Visible && (long)control.FocusMode == 2)
		{
			BaseButton val = (BaseButton)(object)((control is BaseButton) ? control : null);
			if (val == null || !val.Disabled)
			{
				NButton val2 = (NButton)(object)((control is NButton) ? control : null);
				if (val2 != null)
				{
					return ((NClickableControl)val2).IsEnabled;
				}
				return true;
			}
		}
		return false;
	}

	private static Control? ResolveContentEntry(IReadOnlyList<List<Control>> rows)
	{
		foreach (List<Control> row in rows)
		{
			foreach (Control item in row)
			{
				Control val = CznSettingsInputControls.ResolveVisualControl(item);
				if (((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_global_category_id")) || ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_category_id")))
				{
					return ResolvePreferredControl(row, null);
				}
			}
		}
		if (rows.Count != 0)
		{
			return ResolvePreferredControl(rows[0], null);
		}
		return null;
	}

	private static void ApplySidebarNavigation(IReadOnlyList<Control> sidebar, Control? selectedNav, Control? firstContent)
	{
		for (int i = 0; i < sidebar.Count; i++)
		{
			Control val = sidebar[i];
			val.FocusNeighborLeft = ((Node)val).GetPath();
			val.FocusNeighborTop = ((Node)sidebar[Math.Max(0, i - 1)]).GetPath();
			val.FocusNeighborBottom = ((Node)sidebar[Math.Min(sidebar.Count - 1, i + 1)]).GetPath();
			val.FocusNeighborRight = ((val == selectedNav && firstContent != null) ? ((Node)firstContent).GetPath() : ((Node)val).GetPath());
		}
	}

	private static void ApplyContentNavigation(IReadOnlyList<List<Control>> rows, Control? selectedNav)
	{
		for (int i = 0; i < rows.Count; i++)
		{
			List<Control> list = rows[i];
			for (int j = 0; j < list.Count; j++)
			{
				Control val = list[j];
				Control val2 = list[Math.Max(0, j - 1)];
				Control val3 = list[Math.Min(list.Count - 1, j + 1)];
				if (list.Count > 1 && j == 0 && selectedNav != null)
				{
					val.FocusNeighborLeft = ((Node)selectedNav).GetPath();
				}
				else
				{
					val.FocusNeighborLeft = ((Node)val2).GetPath();
				}
				val.FocusNeighborRight = ((Node)val3).GetPath();
				Control val4 = (Control)((i > 0) ? ((object)ResolvePreferredControl(rows[i - 1], val)) : ((object)val));
				Control val5 = (Control)((i < rows.Count - 1) ? ((object)ResolvePreferredControl(rows[i + 1], val)) : ((object)val));
				val.FocusNeighborTop = ((Node)val4).GetPath();
				val.FocusNeighborBottom = ((Node)val5).GetPath();
			}
		}
	}

	private static Control ResolvePreferredControl(IReadOnlyList<Control> row, Control? source)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		foreach (Control item in row)
		{
			Control val = CznSettingsInputControls.ResolveVisualControl(item);
			if (((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_focus_preferred")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_focus_preferred");
				Variant val2 = default(Variant);
				val2 = ((GodotObject)val).GetMeta(obj, val2);
				if (((Variant)(ref val2)).AsBool())
				{
					return item;
				}
			}
			BaseButton val3 = (BaseButton)(object)((val is BaseButton) ? val : null);
			if (val3 != null && val3.ButtonPressed)
			{
				return item;
			}
		}
		if (source == null || row.Count == 1)
		{
			return row[0];
		}
		float num = source.GlobalPosition.X + source.Size.X * 0.5f;
		Control result = row[0];
		float num2 = float.PositiveInfinity;
		foreach (Control item2 in row)
		{
			float num3 = Math.Abs(item2.GlobalPosition.X + item2.Size.X * 0.5f - num);
			if (num3 < num2)
			{
				num2 = num3;
				result = item2;
			}
		}
		return result;
	}

	private static string FormatSliderValue(double value, string suffix)
	{
		if (string.IsNullOrEmpty(suffix))
		{
			return $"{value:0}";
		}
		return $"{value:0}{suffix}";
	}

	private static bool TryCreateGlobalSettingsSections(SettingsUiBuildContext ctx)
	{
		bool flag = TryCreateGroupHeaderSection(ctx.VBox, ctx.TemplateLabel, "CznModGlobalHeader", ctx.Text.GlobalSettingsHeader, first: true, delegate
		{
			ctx.Global.ResetGlobalToDefaults(persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Shared);
		}, ctx.Text);
		flag |= TryCreateToggleSection(ctx, "CznModDebugLogEnabled", ctx.Text.DebugLogEnabled, ctx.Text.DebugLogEnabledDescription, ctx.Global.DebugLogEnabled, delegate(bool value)
		{
			ctx.Global.SetDebugLogEnabled(value, persist: true);
		});
		flag |= TryCreateToggleSection(ctx, "CznModGlobalEffectsEnabled", ctx.Text.GlobalEffectsEnabled, ctx.Text.EffectsEnabledDescription, ctx.Global.GlobalEffectsEnabled, delegate(bool value)
		{
			ctx.Global.SetGlobalEffectsEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Shared);
			RebuildCurrentCharacterPageDeferred(ctx);
		});
		flag |= TryCreateToggleSection(ctx, "CznModGlobalParticlesEnabled", ctx.Text.GlobalParticlesEnabled, ctx.Text.GlobalParticlesEnabledDescription, ctx.Global.GlobalParticlesEnabled, delegate(bool value)
		{
			ctx.Global.SetGlobalParticlesEnabled(value, persist: true);
			RebuildCurrentCharacterPageDeferred(ctx);
		}, ctx.Global.GlobalEffectsEnabled);
		flag |= TryCreateToggleSection(ctx, "CznModGlobalCombatSfxEnabled", ctx.Text.GlobalCombatSfxEnabled, string.Empty, ctx.Global.GlobalCombatSfxEnabled, delegate(bool value)
		{
			ctx.Global.SetGlobalCombatSfxEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
			RebuildCurrentCharacterPageDeferred(ctx);
		});
		flag |= TryCreateToggleSection(ctx, "CznModGlobalVoiceEnabled", ctx.Text.GlobalVoiceEnabled, string.Empty, ctx.Global.GlobalVoiceEnabled, delegate(bool value)
		{
			ctx.Global.SetGlobalVoiceEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
			RebuildCurrentCharacterPageDeferred(ctx);
		});
		flag |= TryCreateGlobalCombatPresentationApplySection(ctx);
		AssignGlobalCategory(ctx, "general", "CznModGlobalHeader", "CznModDebugLogEnabled", "CznModGlobalEffectsEnabled", "CznModGlobalParticlesEnabled", "CznModGlobalCombatSfxEnabled", "CznModGlobalVoiceEnabled", "CznModGlobalCombatPresentationApply");
		return flag;
	}

	private static bool TryCreateMultiplayerSettingsSections(SettingsUiBuildContext ctx)
	{
		bool flag = TryCreateGroupHeaderSection(ctx.VBox, ctx.TemplateLabel, "CznModMultiplayerHeader", ctx.Text.MultiplayerSettingsHeader, first: true, delegate
		{
			ctx.Global.EnableAllTeammatePresentationChannels(persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
		}, ctx.Text, ctx.Text.TeammatePresentationEnableAll);
		flag |= TryCreateMultiplayerIntroSection(ctx);
		flag |= TryCreateToggleSection(ctx, "CznModTeammatePresentationEnabled", ctx.Text.TeammatePresentationEnabled, ctx.Text.TeammatePresentationEnabledDescription, ctx.Global.TeammatePresentationEnabled, delegate(bool value)
		{
			ctx.Global.SetTeammatePresentationEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
		});
		flag |= TryCreateMultiplayerSummarySection(ctx);
		flag |= TryCreateMiniHeaderSection(ctx.VBox, ctx.TemplateLabel, "CznModTeammateVisualHeader", ctx.Text.TeammateVisualSettingsHeader);
		bool teammatePresentationEnabled = ctx.Global.TeammatePresentationEnabled;
		bool enabled = teammatePresentationEnabled && ctx.Global.GlobalEffectsEnabled;
		bool enabled2 = teammatePresentationEnabled && ctx.Global.GlobalCombatSfxEnabled;
		bool flag2 = teammatePresentationEnabled && ctx.Global.GlobalVoiceEnabled;
		flag |= TryCreateToggleSection(ctx, "CznModTeammateCutInEnabled", ctx.Text.TeammateCutInEnabled, ctx.Text.TeammateCutInEnabledDescription, ctx.Global.TeammateCutInEnabled, delegate(bool value)
		{
			ctx.Global.SetTeammateCutInEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
		}, teammatePresentationEnabled);
		flag |= TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, "CznModTeammateAttackFullScreenPresentationMode", ctx.Text.TeammateAttackFullScreenPresentationMode, ctx.Text.TeammateAttackFullScreenPresentationModeDescription, new _003C_003Ez__ReadOnlyArray<(int, string)>(new(int, string)[2]
		{
			(0, ctx.Text.AttackFullScreenPresentationAlways),
			(1, ctx.Text.AttackFullScreenPresentationLethalOnly)
		}), (int)ctx.Global.TeammateAttackFullScreenPresentationMode, delegate(int id)
		{
			ctx.Global.SetTeammateAttackFullScreenPresentationMode((AttackFullScreenPresentationMode)id, persist: true);
		}, teammatePresentationEnabled && ctx.Global.TeammateCutInEnabled);
		flag |= TryCreateToggleSection(ctx, "CznModTeammateEffectsEnabled", ctx.Text.TeammateEffectsEnabled, ctx.Text.TeammateEffectsEnabledDescription, ctx.Global.TeammateEffectsEnabled, delegate(bool value)
		{
			ctx.Global.SetTeammateEffectsEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
		}, enabled);
		flag |= TryCreateMiniHeaderSection(ctx.VBox, ctx.TemplateLabel, "CznModTeammateAudioHeader", ctx.Text.TeammateAudioSettingsHeader);
		flag |= TryCreateToggleSection(ctx, "CznModTeammateCombatSfxEnabled", ctx.Text.TeammateCombatSfxEnabled, ctx.Text.TeammateCombatSfxEnabledDescription, ctx.Global.TeammateCombatSfxEnabled, delegate(bool value)
		{
			ctx.Global.SetTeammateCombatSfxEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
		}, enabled2);
		flag |= TryCreateToggleSection(ctx, "CznModTeammateVoiceEnabled", ctx.Text.TeammateVoiceEnabled, ctx.Text.TeammateVoiceEnabledDescription, ctx.Global.TeammateVoiceEnabled, delegate(bool value)
		{
			ctx.Global.SetTeammateVoiceEnabled(value, persist: true);
			RebuildSettingsPanelDeferred(ctx, RebuildScope.Multiplayer);
		}, flag2);
		flag |= TryCreateSliderSection(ctx, "CznModTeammateVoiceCooldown", ctx.Text.TeammateVoiceCooldown, ctx.Text.TeammateVoiceCooldownDescription, "CznModTeammateVoiceCooldownSlider", 0.0, 10.0, 0.5, ctx.Global.TeammateVoiceCooldownSeconds, "s", 0.0, delegate(double value)
		{
			ctx.Global.SetTeammateVoiceCooldownSeconds((float)value, persist: true);
		}, flag2 && ctx.Global.TeammateVoiceEnabled);
		AssignGlobalCategory(ctx, "multiplayer", "CznModMultiplayerHeader", "CznModMultiplayerIntro", "CznModTeammatePresentationEnabled", "CznModMultiplayerSummary", "CznModTeammateVisualHeader", "CznModTeammateCutInEnabled", "CznModTeammateAttackFullScreenPresentationMode", "CznModTeammateEffectsEnabled", "CznModTeammateAudioHeader", "CznModTeammateCombatSfxEnabled", "CznModTeammateVoiceEnabled", "CznModTeammateVoiceCooldown");
		return flag;
	}

	private static bool TryCreateMultiplayerIntroSection(SettingsUiBuildContext ctx)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)ctx.VBox).GetNodeOrNull(new NodePath("CznModMultiplayerIntro")) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection("CznModMultiplayerIntro", 0, 10, 108f);
		PanelContainer val2 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(858f, 98f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateStatusHeaderStyle());
		VBoxContainer val3 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(830f, 86f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
		Label val4 = CreateTextLabel(ctx.TemplateLabel, ctx.Text.TeammatePresentationIntro, -2, TextColor);
		((Control)val4).CustomMinimumSize = new Vector2(826f, 48f);
		val4.VerticalAlignment = (VerticalAlignment)0;
		val4.AutowrapMode = (AutowrapMode)3;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		Label val5 = CreateTextLabel(ctx.TemplateLabel, ctx.Text.TeammatePresentationPerformanceWarning, -3, CreamGold);
		((Control)val5).CustomMinimumSize = new Vector2(826f, 34f);
		val5.VerticalAlignment = (VerticalAlignment)0;
		val5.AutowrapMode = (AutowrapMode)3;
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)ctx.VBox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static bool TryCreateMultiplayerSummarySection(SettingsUiBuildContext ctx)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Expected O, but got Unknown
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)ctx.VBox).GetNodeOrNull(new NodePath("CznModMultiplayerSummary")) != null)
		{
			return false;
		}
		List<string> teammatePresentationGlobalLimits = GetTeammatePresentationGlobalLimits(ctx);
		bool flag = ctx.Global.TeammateCutInEnabled && ctx.Global.TeammateEffectsEnabled && ctx.Global.TeammateCombatSfxEnabled && ctx.Global.TeammateVoiceEnabled && teammatePresentationGlobalLimits.Count == 0;
		Color color = ((!ctx.Global.TeammatePresentationEnabled) ? DimText : (flag ? CreamGold : TextColor));
		float num = ((teammatePresentationGlobalLimits.Count == 0) ? 42f : 68f);
		MarginContainer val = CreateSection("CznModMultiplayerSummary", 8, 10, num + 8f);
		PanelContainer val2 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(858f, num),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateCombatSummaryStatusStyle());
		Label val3 = CreateTextLabel(ctx.TemplateLabel, BuildTeammatePresentationStatus(ctx, teammatePresentationGlobalLimits), -3, color);
		((Control)val3).CustomMinimumSize = new Vector2(834f, num - 2f);
		val3.VerticalAlignment = (VerticalAlignment)1;
		val3.AutowrapMode = (AutowrapMode)3;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)ctx.VBox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static string BuildTeammatePresentationStatus(SettingsUiBuildContext ctx, IReadOnlyList<string> globalLimits)
	{
		if (!ctx.Global.TeammatePresentationEnabled)
		{
			return ctx.Text.TeammatePresentationStatusOriginal;
		}
		List<string> list = new List<string>();
		if (!ctx.Global.TeammateCutInEnabled)
		{
			list.Add(ctx.Text.TeammateCutInEnabled);
		}
		if (!ctx.Global.TeammateEffectsEnabled)
		{
			list.Add(ctx.Text.TeammateEffectsEnabled);
		}
		if (!ctx.Global.TeammateCombatSfxEnabled)
		{
			list.Add(ctx.Text.TeammateCombatSfxEnabled);
		}
		if (!ctx.Global.TeammateVoiceEnabled)
		{
			list.Add(ctx.Text.TeammateVoiceEnabled);
		}
		string text = ((list.Count == 0 && globalLimits.Count == 0) ? ctx.Text.TeammatePresentationStatusAll : ctx.Text.TeammatePresentationStatusPartial);
		if (list.Count > 0)
		{
			text = text + " — " + string.Join(" / ", list);
		}
		if (globalLimits.Count > 0)
		{
			text = text + "\n" + ctx.Text.TeammatePresentationStatusGlobalLimits + " — " + string.Join(" / ", globalLimits);
		}
		return text;
	}

	private static List<string> GetTeammatePresentationGlobalLimits(SettingsUiBuildContext ctx)
	{
		List<string> list = new List<string>();
		if (!ctx.Global.TeammatePresentationEnabled)
		{
			return list;
		}
		if (ctx.Global.TeammateEffectsEnabled && !ctx.Global.GlobalEffectsEnabled)
		{
			list.Add(ctx.Text.TeammateEffectsEnabled);
		}
		if (ctx.Global.TeammateCombatSfxEnabled && !ctx.Global.GlobalCombatSfxEnabled)
		{
			list.Add(ctx.Text.TeammateCombatSfxEnabled);
		}
		if (ctx.Global.TeammateVoiceEnabled && !ctx.Global.GlobalVoiceEnabled)
		{
			list.Add(ctx.Text.TeammateVoiceEnabled);
		}
		return list;
	}

	private static bool TryCreateGlobalCombatPresentationApplySection(SettingsUiBuildContext ctx)
	{
		return TryCreateControlSection(ctx.VBox, ctx.TemplateLabel, "CznModGlobalCombatPresentationApply", ctx.Text.GlobalCombatPresentationApply, ctx.Text.GlobalCombatPresentationApplyDescription, delegate(float rowHeight)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Expected O, but got Unknown
			HBoxContainer val = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(360f, rowHeight),
				MouseFilter = (MouseFilterEnum)2
			};
			((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
			Button val2 = CreateHeaderActionButton(ctx.TemplateLabel, ctx.Text.GlobalCombatPresentationApplyFast, 170f);
			((BaseButton)val2).Pressed += delegate
			{
				ApplyGlobalCombatPresentationMode(ctx, CombatPresentationMode.Fast);
			};
			Button val3 = CreateHeaderActionButton(ctx.TemplateLabel, ctx.Text.GlobalCombatPresentationApplyFull, 150f);
			((BaseButton)val3).Pressed += delegate
			{
				ApplyGlobalCombatPresentationMode(ctx, CombatPresentationMode.Full);
			};
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			return (Control)(object)WrapControlCenter((Control)val, 360f, rowHeight);
		});
	}

	private static void ApplyGlobalCombatPresentationMode(SettingsUiBuildContext ctx, CombatPresentationMode mode)
	{
		ctx.Global.ApplyCombatPresentationModeToLoadedCharacters(mode, persist: true);
		ApplyCombatPresentationModePreset(ctx, mode);
		RebuildSettingsPanelDeferred(ctx, RebuildScope.Global);
		RebuildCurrentCharacterPageDeferred(ctx);
	}

	private static bool TryCreateExtensionSettingsSections(SettingsUiExtensionContext ctx)
	{
		bool flag = false;
		foreach (SharedSettingsExtensionCategory category in SharedSettingsExtensionRegistry.GetCategories())
		{
			for (int i = 0; i < category.Groups.Count; i++)
			{
				SettingsUiExtensionGroup settingsUiExtensionGroup = category.Groups[i];
				string groupPrefix = "CznModExtension_" + BuildExtensionNodeSegment(settingsUiExtensionGroup.ProviderId) + "_";
				flag |= TryCreateSettingsExtensionGroup(ctx, settingsUiExtensionGroup, i == 0, (string segment) => groupPrefix + segment, delegate
				{
					RebuildExtensionSectionsDeferred(ctx);
				}, out string[] sectionNames);
				AssignGlobalCategory(ctx.VBox, category.Id, sectionNames);
			}
		}
		return flag;
	}

	private static bool TryCreateCharacterExtensionSettingsSections(SettingsUiBuildContext ctx)
	{
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		List<SettingsUiExtensionGroup> list = new List<SettingsUiExtensionGroup>(ctx.Runtime.CharacterSettingsExtensions);
		list.Sort(delegate(SettingsUiExtensionGroup left, SettingsUiExtensionGroup right)
		{
			int num = string.Compare(left.CategoryId, right.CategoryId, StringComparison.Ordinal);
			if (num != 0)
			{
				return num;
			}
			int num2 = left.Order.CompareTo(right.Order);
			return (num2 == 0) ? string.Compare(left.ProviderId, right.ProviderId, StringComparison.Ordinal) : num2;
		});
		bool flag = false;
		foreach (SettingsUiExtensionGroup item in list)
		{
			if (!IsCharacterCategory(item.CategoryId))
			{
				continue;
			}
			string groupPrefix = "CznModCharacterExtension_" + BuildExtensionNodeSegment(item.ProviderId) + "_";
			flag |= TryCreateSettingsExtensionGroup(ctx.Extensions, item, first: false, (string segment) => CharacterSectionName(ctx, groupPrefix + segment), delegate
			{
				RebuildSettingsPanelDeferred(ctx, RebuildScope.Character);
			}, out string[] sectionNames);
			string[] array = sectionNames;
			foreach (string text in array)
			{
				Node nodeOrNull = ((Node)ctx.VBox).GetNodeOrNull(new NodePath(text));
				if (nodeOrNull != null)
				{
					((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("czn_mod_settings_character_category"), Variant.op_Implicit(item.CategoryId));
				}
			}
		}
		return flag;
	}

	private static bool TryCreateSettingsExtensionGroup(SettingsUiExtensionContext ctx, SettingsUiExtensionGroup group, bool first, Func<string, string> buildSectionName, Action rebuild, out string[] sectionNames)
	{
		string text = buildSectionName("Header");
		List<string> list = new List<string>(group.Controls.Count + 1) { text };
		Action resetAction = ((group.Reset == null) ? null : ((Action)delegate
		{
			group.Reset();
			rebuild();
		}));
		bool flag = TryCreateGroupHeaderSection(ctx.VBox, ctx.TemplateLabel, text, group.Label(), first, resetAction, ctx.Text);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		foreach (SettingsUiExtensionControl control in group.Controls)
		{
			if (!string.IsNullOrWhiteSpace(control.ParentId))
			{
				hashSet.Add(control.ParentId);
			}
		}
		foreach (SettingsUiExtensionControl control2 in group.Controls)
		{
			SettingsControlRowRole rowRole = ((!string.IsNullOrWhiteSpace(control2.ParentId)) ? SettingsControlRowRole.Child : (hashSet.Contains(control2.Id) ? SettingsControlRowRole.Parent : SettingsControlRowRole.Standalone));
			string text2 = buildSectionName(BuildExtensionNodeSegment(control2.Id));
			list.Add(text2);
			flag |= TryCreateSettingsExtensionControl(ctx, control2, text2, rebuild, rowRole);
		}
		sectionNames = list.ToArray();
		return flag;
	}

	private static bool TryCreateSettingsExtensionControl(SettingsUiExtensionContext ctx, SettingsUiExtensionControl control, string sectionName, Action rebuild, SettingsControlRowRole rowRole)
	{
		SettingsUiExtensionToggle settingsUiExtensionToggle = control as SettingsUiExtensionToggle;
		if ((object)settingsUiExtensionToggle == null)
		{
			if (!(control is SettingsUiExtensionSlider settingsUiExtensionSlider))
			{
				SettingsUiExtensionChoice settingsUiExtensionChoice = control as SettingsUiExtensionChoice;
				if ((object)settingsUiExtensionChoice == null)
				{
					if (control is SettingsUiExtensionStringChoice choice)
					{
						return TryCreateStringChoiceSection(ctx, choice, sectionName, rebuild, rowRole);
					}
					return false;
				}
				List<(int, string)> list = new List<(int, string)>(settingsUiExtensionChoice.Options.Count);
				foreach (SettingsUiExtensionChoiceOption option in settingsUiExtensionChoice.Options)
				{
					list.Add((option.Id, option.Label()));
				}
				return TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, sectionName, settingsUiExtensionChoice.Label(), settingsUiExtensionChoice.Description(), list, settingsUiExtensionChoice.Read(), delegate(int value)
				{
					settingsUiExtensionChoice.Write(value);
					rebuild();
				}, settingsUiExtensionChoice.Enabled(), rowRole);
			}
			return TryCreateSliderSection(ctx, sectionName, settingsUiExtensionSlider.Label(), settingsUiExtensionSlider.Description(), BuildExtensionNodeSegment(settingsUiExtensionSlider.Id + "_Slider"), settingsUiExtensionSlider.Min, settingsUiExtensionSlider.Max, settingsUiExtensionSlider.Step, settingsUiExtensionSlider.Read(), settingsUiExtensionSlider.Suffix(), settingsUiExtensionSlider.DisplayOffset, settingsUiExtensionSlider.Write, settingsUiExtensionSlider.Enabled(), rowRole);
		}
		return TryCreateToggleSection(ctx, sectionName, settingsUiExtensionToggle.Label(), settingsUiExtensionToggle.Description(), settingsUiExtensionToggle.Read(), delegate(bool value)
		{
			settingsUiExtensionToggle.Write(value);
			rebuild();
		}, settingsUiExtensionToggle.Enabled(), rowRole);
	}

	private static bool TryCreateStringChoiceSection(SettingsUiExtensionContext ctx, SettingsUiExtensionStringChoice choice, string sectionName, Action rebuild, SettingsControlRowRole rowRole)
	{
		if (choice.Options.Count == 0)
		{
			return false;
		}
		string b = choice.Read();
		int currentId = 0;
		List<(int, string)> list = new List<(int, string)>(choice.Options.Count);
		for (int i = 0; i < choice.Options.Count; i++)
		{
			SettingsUiExtensionStringChoiceOption settingsUiExtensionStringChoiceOption = choice.Options[i];
			list.Add((i, settingsUiExtensionStringChoiceOption.Label()));
			if (string.Equals(settingsUiExtensionStringChoiceOption.Id, b, StringComparison.Ordinal))
			{
				currentId = i;
			}
		}
		return TryCreateDropdownSection(ctx.VBox, ctx.TemplateLabel, sectionName, choice.Label(), choice.Description(), list, currentId, delegate(int index)
		{
			choice.Write(choice.Options[index].Id);
			rebuild();
		}, choice.Enabled(), rowRole);
	}

	private static bool IsCharacterCategory(string categoryId)
	{
		switch (categoryId)
		{
		case "visual":
		case "combat":
		case "effects":
		case "audio":
			return true;
		default:
			return false;
		}
	}

	private static string BuildExtensionNodeSegment(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return "Unnamed";
		}
		char[] array = new char[value.Length];
		for (int i = 0; i < value.Length; i++)
		{
			char c = value[i];
			array[i] = (char.IsLetterOrDigit(c) ? c : '_');
		}
		return new string(array);
	}

	private static bool TryCreateToggleSection(SettingsUiBuildContext ctx, string sectionName, string labelText, string descriptionText, bool current, Action<bool> onToggled, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		return TryCreateToggleSection(ctx.VBox, ctx.TemplateLabel, ctx.TickboxVisualScene, sectionName, labelText, descriptionText, current, onToggled, enabled, rowRole);
	}

	private static bool TryCreateToggleSection(SettingsUiExtensionContext ctx, string sectionName, string labelText, string descriptionText, bool current, Action<bool> onToggled, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		return TryCreateToggleSection(ctx.VBox, ctx.TemplateLabel, ctx.TickboxVisualScene, sectionName, labelText, descriptionText, current, onToggled, enabled, rowRole);
	}

	private static bool TryCreateSliderSection(SettingsUiBuildContext ctx, string sectionName, string labelText, string descriptionText, string sliderRootName, double min, double max, double step, double current, string suffix, double displayValueOffset, Action<double> onChanged, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		return TryCreateSliderSection(ctx.VBox, ctx.TemplateLabel, ctx.SliderScene, sectionName, labelText, descriptionText, sliderRootName, min, max, step, current, suffix, displayValueOffset, onChanged, enabled, rowRole);
	}

	private static bool TryCreateSliderSection(SettingsUiExtensionContext ctx, string sectionName, string labelText, string descriptionText, string sliderRootName, double min, double max, double step, double current, string suffix, double displayValueOffset, Action<double> onChanged, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		return TryCreateSliderSection(ctx.VBox, ctx.TemplateLabel, ctx.SliderScene, sectionName, labelText, descriptionText, sliderRootName, min, max, step, current, suffix, displayValueOffset, onChanged, enabled, rowRole);
	}

	private static bool TryCreateTopSpacerSection(VBoxContainer vbox, string sectionName)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)vbox).GetNodeOrNull(new NodePath(sectionName)) != null)
		{
			return false;
		}
		Control val = new Control
		{
			Name = StringName.op_Implicit(sectionName),
			CustomMinimumSize = new Vector2(882f, 18f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		val.SizeFlagsHorizontal = (SizeFlags)4;
		((Node)vbox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static bool TryCreateGroupHeaderSection(VBoxContainer vbox, RichTextLabel templateLabel, string sectionName, string labelText, bool first, Action? resetAction, ISkinSettingsUiText text, string? actionText = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Expected O, but got Unknown
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Expected O, but got Unknown
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)vbox).GetNodeOrNull(new NodePath(sectionName)) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection(sectionName, (!first) ? 28 : 0, 10, 58f, 0);
		PanelContainer val2 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(882f, 46f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		StyleBoxFlat val3 = new StyleBoxFlat
		{
			BgColor = ModHeaderBg,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4,
			ContentMarginLeft = 12f,
			ContentMarginRight = 12f,
			ContentMarginTop = 5f,
			ContentMarginBottom = 5f
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val3);
		HBoxContainer val4 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(858f, 36f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val4).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		ColorRect val5 = new ColorRect
		{
			Color = CreamGold,
			CustomMinimumSize = new Vector2(4f, 28f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val5).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		Label val6 = CreateTextLabel(templateLabel, labelText, 2, CreamGold);
		((Control)val6).CustomMinimumSize = new Vector2(522f, 36f);
		((Control)val6).SizeFlagsHorizontal = (SizeFlags)3;
		val6.VerticalAlignment = (VerticalAlignment)1;
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
		if (resetAction != null)
		{
			((Node)val4).AddChild((Node)(object)CreateResetButton(templateLabel, resetAction, text, actionText), false, (InternalMode)0);
		}
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)vbox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static bool TryCreateMiniHeaderSection(VBoxContainer vbox, RichTextLabel templateLabel, string sectionName, string labelText)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Expected O, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Expected O, but got Unknown
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)vbox).GetNodeOrNull(new NodePath(sectionName)) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection(sectionName, 10, 2, 38f);
		PanelContainer val2 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(858f, 32f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateSettingStripStyle(new Color(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.6f), accented: false));
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(838f, 28f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		ColorRect val4 = new ColorRect
		{
			Color = new Color(CreamGold.R, CreamGold.G, CreamGold.B, 0.42f),
			CustomMinimumSize = new Vector2(3f, 20f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val4).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		Label val5 = CreateTextLabel(templateLabel, labelText, 0, CreamGold);
		((Control)val5).CustomMinimumSize = new Vector2(812f, 28f);
		val5.VerticalAlignment = (VerticalAlignment)1;
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)vbox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static bool TryCreateSubHeaderSection(VBoxContainer vbox, RichTextLabel templateLabel, string sectionName, string labelText)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)vbox).GetNodeOrNull(new NodePath(sectionName)) != null)
		{
			return false;
		}
		MarginContainer val = CreateSection(sectionName, 18, 8, 62f);
		VBoxContainer val2 = CreateStack();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)val2).AddChild((Node)(object)CreateSubHeaderDivider(sectionName + "Divider"), false, (InternalMode)0);
		PanelContainer val3 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(858f, 38f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val3).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val3).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateSettingStripStyle(SubHeaderBg, accented: true));
		HBoxContainer val4 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(838f, 30f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val4).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		ColorRect val5 = new ColorRect
		{
			Color = CreamGold,
			CustomMinimumSize = new Vector2(4f, 24f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val5).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		Label val6 = CreateTextLabel(templateLabel, labelText, 2, CreamGold);
		((Control)val6).CustomMinimumSize = new Vector2(812f, 30f);
		val6.VerticalAlignment = (VerticalAlignment)1;
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)vbox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static Button CreateResetButton(RichTextLabel templateLabel, Action resetAction, ISkinSettingsUiText text, string? buttonText = null)
	{
		Button obj = CreateHeaderActionButton(templateLabel, buttonText ?? text.ResetDefaults, 124f);
		((BaseButton)obj).Pressed += resetAction;
		return obj;
	}

	private static Button CreateHeaderActionButton(RichTextLabel templateLabel, string text, float width)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		CznSettingsButton cznSettingsButton = new CznSettingsButton();
		((Button)cznSettingsButton).Text = text;
		((Control)cznSettingsButton).CustomMinimumSize = new Vector2(width, 34f);
		((Control)cznSettingsButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsButton).SizeFlagsHorizontal = (SizeFlags)8;
		((Control)cznSettingsButton).SizeFlagsVertical = (SizeFlags)4;
		((GodotObject)cznSettingsButton).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)cznSettingsButton).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)templateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)cznSettingsButton).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(templateLabel, -1));
		((Control)cznSettingsButton).AddThemeColorOverride(StringName.op_Implicit("font_color"), CreamGold);
		((Control)cznSettingsButton).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), TextColor);
		((Control)cznSettingsButton).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), TextColor);
		((Control)cznSettingsButton).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)CreateHeaderButtonStyle(new Color(0.12f, 0.19f, 0.23f, 0.92f)));
		((Control)cznSettingsButton).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)CreateHeaderButtonStyle(new Color(0.18f, 0.29f, 0.34f, 0.96f)));
		((Control)cznSettingsButton).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)CreateHeaderButtonStyle(new Color(0.09f, 0.15f, 0.18f, 1f)));
		((Control)cznSettingsButton).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)CreateHeaderButtonStyle(new Color(0.18f, 0.29f, 0.34f, 0.96f)));
		return CznSettingsInputControls.AttachNativeOwner((Button)(object)cznSettingsButton);
	}

	private static bool TryCreateSliderSection(VBoxContainer vbox, RichTextLabel templateLabel, PackedScene sliderScene, string sectionName, string labelText, string descriptionText, string sliderRootName, double min, double max, double step, double current, string suffix, double displayValueOffset, Action<double> onChanged, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		Control sliderRoot = null;
		bool num = TryCreateControlSection(vbox, templateLabel, sectionName, labelText, descriptionText, delegate(float rowHeight)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			sliderRoot = sliderScene.Instantiate<Control>((GenEditState)0);
			((Node)sliderRoot).Name = StringName.op_Implicit(sliderRootName);
			((GodotObject)sliderRoot).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			sliderRoot.CustomMinimumSize = new Vector2(324f, rowHeight);
			sliderRoot.SizeFlagsHorizontal = (SizeFlags)8;
			return sliderRoot;
		}, enabled, rowRole);
		if (num && sliderRoot != null)
		{
			GodotScheduler.Deferred(delegate
			{
				WireSliderWhenReady(sliderRoot, min, max, step, current, suffix, displayValueOffset, onChanged, enabled, 8);
			});
		}
		return num;
	}

	private static bool TryCreateToggleSection(VBoxContainer vbox, RichTextLabel templateLabel, PackedScene? tickboxScene, string sectionName, string labelText, string descriptionText, bool current, Action<bool> onToggled, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		return TryCreateControlSection(vbox, templateLabel, sectionName, labelText, descriptionText, delegate(float rowHeight)
		{
			Control val = CreateToggleControl(templateLabel, tickboxScene, sectionName + "Toggle", current, onToggled);
			if (!enabled)
			{
				BaseButton val2 = (BaseButton)(object)((val is BaseButton) ? val : null);
				if (val2 != null)
				{
					val2.Disabled = true;
				}
			}
			val.TooltipText = descriptionText;
			return (Control)(object)WrapControlCenter(val, 64f, rowHeight);
		}, enabled, rowRole);
	}

	private static bool TryCreateActionSection(VBoxContainer vbox, RichTextLabel templateLabel, string sectionName, string labelText, string descriptionText, string buttonText, Action onPressed, bool enabled = true)
	{
		return TryCreateControlSection(vbox, templateLabel, sectionName, labelText, descriptionText, delegate(float rowHeight)
		{
			Button obj = CreateHeaderActionButton(templateLabel, buttonText, 164f);
			((BaseButton)obj).Disabled = !enabled;
			((BaseButton)obj).Pressed += onPressed;
			((Control)obj).TooltipText = descriptionText;
			return (Control)(object)WrapControlCenter((Control)(object)obj, 184f, rowHeight);
		}, enabled);
	}

	private static bool TryCreateDropdownSection(VBoxContainer vbox, RichTextLabel templateLabel, string sectionName, string labelText, string descriptionText, IReadOnlyList<(int Id, string Label)> options, int currentId, Action<int> onSelected, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		return TryCreateControlSection(vbox, templateLabel, sectionName, labelText, descriptionText, delegate(float rowHeight)
		{
			OptionButton obj = CreateDropdownControl(templateLabel, sectionName + "Dropdown", options, currentId, onSelected);
			((BaseButton)obj).Disabled = !enabled;
			((Control)obj).TooltipText = descriptionText;
			return (Control)(object)WrapControlCenter((Control)(object)obj, 320f, rowHeight);
		}, enabled, rowRole);
	}

	private static bool TryCreateControlSection(VBoxContainer vbox, RichTextLabel templateLabel, string sectionName, string labelText, string descriptionText, Func<float, Control> createControl, bool enabled = true, SettingsControlRowRole rowRole = SettingsControlRowRole.Standalone)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)vbox).GetNodeOrNull(new NodePath(sectionName)) != null)
		{
			return false;
		}
		bool num = !string.IsNullOrWhiteSpace(descriptionText);
		float num2 = (num ? 74f : 56f);
		MarginContainer val = CreateSection(sectionName, 0, 0, num2 + 1f);
		float num3 = ((rowRole == SettingsControlRowRole.Standalone) ? 858f : 834f);
		VBoxContainer val2 = CreateStack((rowRole == SettingsControlRowRole.Standalone) ? 882f : num3);
		((Node)val2).AddChild((Node)(object)CreateRowSeparator(sectionName + "Divider", num3), false, (InternalMode)0);
		HBoxContainer val3 = CreateRow(num2, num3);
		VBoxContainer val4 = CreateTextStack(templateLabel, labelText, descriptionText, num2);
		Control val5 = createControl(num2);
		if (!enabled)
		{
			((CanvasItem)val3).Modulate = DisabledRowModulate;
		}
		if (num)
		{
			((Control)val3).TooltipText = descriptionText;
			val5.TooltipText = descriptionText;
		}
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)CreateSpacer(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)WrapControlHierarchy(val2, rowRole, num2), false, (InternalMode)0);
		((Node)vbox).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static Control WrapControlHierarchy(VBoxContainer stack, SettingsControlRowRole rowRole, float rowHeight)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		if (rowRole == SettingsControlRowRole.Standalone)
		{
			return (Control)(object)stack;
		}
		((Control)stack).SizeFlagsHorizontal = (SizeFlags)3;
		if (rowRole == SettingsControlRowRole.Parent)
		{
			PanelContainer val = new PanelContainer
			{
				CustomMinimumSize = new Vector2(858f, rowHeight + 1f),
				MouseFilter = (MouseFilterEnum)2
			};
			((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateSettingStripStyle(new Color(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.46f), accented: true));
			((Node)val).AddChild((Node)(object)stack, false, (InternalMode)0);
			return (Control)val;
		}
		HBoxContainer val2 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(858f, rowHeight + 1f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		((Node)val2).AddChild((Node)new ColorRect
		{
			Name = StringName.op_Implicit("CznModSettingsChildGuide"),
			Color = new Color(CreamGold.R, CreamGold.G, CreamGold.B, 0.38f),
			CustomMinimumSize = new Vector2(3f, rowHeight + 1f),
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)stack, false, (InternalMode)0);
		return (Control)val2;
	}

	private static OptionButton CreateDropdownControl(RichTextLabel templateLabel, string name, IReadOnlyList<(int Id, string Label)> options, int currentId, Action<int> onSelected)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		CznSettingsOptionButton cznSettingsOptionButton = new CznSettingsOptionButton();
		((Node)cznSettingsOptionButton).Name = StringName.op_Implicit(name);
		((Control)cznSettingsOptionButton).CustomMinimumSize = new Vector2(320f, 44f);
		((Control)cznSettingsOptionButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsOptionButton).SizeFlagsHorizontal = (SizeFlags)8;
		OptionButton dropdown = (OptionButton)(object)cznSettingsOptionButton;
		((GodotObject)dropdown).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)dropdown).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)templateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)dropdown).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(templateLabel, -1));
		int num = 0;
		for (int i = 0; i < options.Count; i++)
		{
			var (num2, text) = options[i];
			dropdown.AddItem(text, num2);
			if (num2 == currentId)
			{
				num = i;
			}
		}
		dropdown.Select(num);
		dropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			int itemId = dropdown.GetItemId((int)index);
			onSelected(itemId);
		};
		return CznSettingsInputControls.AttachNativeOwner(dropdown);
	}

	private static MarginContainer CreateSection(string name, int top, int bottom, float minHeight, int horizontalMargin = 12)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		MarginContainer val = new MarginContainer
		{
			Name = StringName.op_Implicit(name),
			CustomMinimumSize = new Vector2(882f, minHeight),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).SizeFlagsHorizontal = (SizeFlags)4;
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), horizontalMargin);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), top);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), horizontalMargin);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), bottom);
		return val;
	}

	private static VBoxContainer CreateStack(float width = 882f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(width, 0f)
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
		return val;
	}

	private static HBoxContainer CreateRow(float height, float width = 858f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(width, height)
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 18);
		return val;
	}

	private static VBoxContainer CreateTextStack(RichTextLabel templateLabel, string labelText, string descriptionText, float rowHeight)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		VBoxContainer val = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(330f, rowHeight),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).SizeFlagsHorizontal = (SizeFlags)1;
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 1);
		if (string.IsNullOrWhiteSpace(descriptionText))
		{
			Label val2 = CreateTextLabel(templateLabel, labelText, 2, TextColor);
			((Control)val2).CustomMinimumSize = new Vector2(330f, rowHeight);
			val2.VerticalAlignment = (VerticalAlignment)1;
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			return val;
		}
		Label val3 = CreateTextLabel(templateLabel, labelText, 2, TextColor);
		((Control)val3).CustomMinimumSize = new Vector2(330f, 32f);
		val3.VerticalAlignment = (VerticalAlignment)2;
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		Label val4 = CreateTextLabel(templateLabel, descriptionText, -2, DimText);
		((Control)val4).CustomMinimumSize = new Vector2(330f, Math.Max(30f, rowHeight - 34f));
		val4.VerticalAlignment = (VerticalAlignment)0;
		val4.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		return val;
	}

	private static Control CreateToggleControl(RichTextLabel templateLabel, PackedScene? tickboxScene, string name, bool current, Action<bool> onToggled)
	{
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected O, but got Unknown
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		if (tickboxScene != null)
		{
			try
			{
				CznSettingsButton cznSettingsButton = new CznSettingsButton();
				((Node)cznSettingsButton).Name = StringName.op_Implicit(name);
				((BaseButton)cznSettingsButton).ToggleMode = true;
				((BaseButton)cznSettingsButton).ButtonPressed = current;
				((Button)cznSettingsButton).Text = string.Empty;
				((Button)cznSettingsButton).Flat = true;
				((Control)cznSettingsButton).FocusMode = (FocusModeEnum)2;
				((Control)cznSettingsButton).CustomMinimumSize = new Vector2(64f, 64f);
				((Control)cznSettingsButton).SizeFlagsHorizontal = (SizeFlags)4;
				((GodotObject)cznSettingsButton).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
				AddTransparentButtonStyles((Button)(object)cznSettingsButton);
				Control visual = tickboxScene.Instantiate<Control>((GenEditState)0);
				((Node)visual).Name = StringName.op_Implicit("TickboxVisual");
				visual.CustomMinimumSize = new Vector2(64f, 64f);
				visual.MouseFilter = (MouseFilterEnum)2;
				UpdateTickboxVisualState(visual, current);
				CenterContainer val = new CenterContainer
				{
					CustomMinimumSize = new Vector2(64f, 64f),
					MouseFilter = (MouseFilterEnum)2
				};
				((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
				((Node)val).AddChild((Node)(object)visual, false, (InternalMode)0);
				((Node)cznSettingsButton).AddChild((Node)(object)val, false, (InternalMode)0);
				((BaseButton)cznSettingsButton).Toggled += (ToggledEventHandler)delegate(bool pressed)
				{
					UpdateTickboxVisualState(visual, pressed);
					onToggled(pressed);
				};
				return (Control)(object)CznSettingsInputControls.AttachNativeOwner((Button)(object)cznSettingsButton);
			}
			catch (Exception ex)
			{
				SkinLog.Warn("Settings tickbox visual unavailable: " + SkinLog.FormatException(ex));
			}
		}
		CznSettingsCheckButton cznSettingsCheckButton = new CznSettingsCheckButton();
		((Node)cznSettingsCheckButton).Name = StringName.op_Implicit(name);
		((BaseButton)cznSettingsCheckButton).ButtonPressed = current;
		((Button)cznSettingsCheckButton).Text = string.Empty;
		((Control)cznSettingsCheckButton).CustomMinimumSize = new Vector2(58f, 44f);
		((Control)cznSettingsCheckButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsCheckButton).SizeFlagsHorizontal = (SizeFlags)4;
		((Control)cznSettingsCheckButton).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)templateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)cznSettingsCheckButton).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(templateLabel, 0));
		((BaseButton)cznSettingsCheckButton).Toggled += (ToggledEventHandler)delegate(bool pressed)
		{
			onToggled(pressed);
		};
		return (Control)(object)CznSettingsInputControls.AttachNativeOwner((CheckButton)(object)cznSettingsCheckButton);
	}

	private static CenterContainer WrapControlCenter(Control control, float width, float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		CenterContainer val = new CenterContainer
		{
			CustomMinimumSize = new Vector2(width, height),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).SizeFlagsHorizontal = (SizeFlags)8;
		((Node)val).AddChild((Node)(object)control, false, (InternalMode)0);
		return val;
	}

	private static Control CreateSpacer()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		Control val = new Control
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		val.SizeFlagsHorizontal = (SizeFlags)3;
		return val;
	}

	private static HSeparator CreateRowSeparator(string name, float width = 858f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Expected O, but got Unknown
		HSeparator val = new HSeparator
		{
			Name = StringName.op_Implicit(name),
			CustomMinimumSize = new Vector2(width, 1f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 1);
		StyleBoxFlat val2 = new StyleBoxFlat
		{
			BgColor = RowSeparatorColor,
			ContentMarginTop = 0f,
			ContentMarginBottom = 0f
		};
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("separator"), (StyleBox)(object)val2);
		return val;
	}

	private static ColorRect CreateSubHeaderDivider(string name)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		ColorRect val = new ColorRect
		{
			Name = StringName.op_Implicit(name),
			Color = SubHeaderLineColor,
			CustomMinimumSize = new Vector2(858f, 2f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		return val;
	}

	private static Label CreateTextLabel(RichTextLabel templateLabel, string text, int fontDelta, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		Label val = new Label
		{
			Text = text,
			MouseFilter = (MouseFilterEnum)2,
			Theme = ((Control)templateLabel).Theme
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), ((Control)templateLabel).GetThemeFont(StringName.op_Implicit("normal_font"), (StringName)null));
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), SettingsFontSize(templateLabel, fontDelta));
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), color);
		return val;
	}

	private static int SettingsFontSize(RichTextLabel templateLabel, int fontDelta)
	{
		return Math.Clamp(((Control)templateLabel).GetThemeFontSize(StringName.op_Implicit("normal_font_size"), (StringName)null) + fontDelta, 14, 28);
	}

	private static void WireSliderWhenReady(Control sliderRoot, double min, double max, double step, double current, string suffix, double displayValueOffset, Action<double> onChanged, bool enabled, int retries)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotNodeUtil.IsAlive((GodotObject?)(object)sliderRoot))
		{
			return;
		}
		NSlider nodeOrNull = ((Node)sliderRoot).GetNodeOrNull<NSlider>(new NodePath("Slider"));
		MegaLabel valueLabel = ((Node)sliderRoot).GetNodeOrNull<MegaLabel>(new NodePath("SliderValue"));
		if (nodeOrNull == null || valueLabel == null)
		{
			if (retries > 0)
			{
				GodotScheduler.Deferred(delegate
				{
					WireSliderWhenReady(sliderRoot, min, max, step, current, suffix, displayValueOffset, onChanged, enabled, retries - 1);
				});
			}
			return;
		}
		((Control)nodeOrNull).MouseFilter = (MouseFilterEnum)(enabled ? 0 : 2);
		((Control)nodeOrNull).FocusMode = (FocusModeEnum)(enabled ? 2 : 0);
		((Range)nodeOrNull).MinValue = min;
		((Range)nodeOrNull).MaxValue = max;
		((Range)nodeOrNull).Step = step;
		nodeOrNull.SetValueWithoutAnimation(current);
		valueLabel.SetTextAutoSize(FormatSliderValue(current + displayValueOffset, suffix));
		if (!((GodotObject)nodeOrNull).HasMeta(StringName.op_Implicit("czn_mod_slider_bound")))
		{
			((GodotObject)nodeOrNull).Connect(SignalName.ValueChanged, Callable.From<double>((Action<double>)delegate(double value)
			{
				double num = Math.Round(value / step) * step + displayValueOffset;
				valueLabel.SetTextAutoSize(FormatSliderValue(num, suffix));
				onChanged(num);
			}), 0u);
			((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("czn_mod_slider_bound"), Variant.op_Implicit(true));
		}
	}

	private static SettingsUiShell EnsureSettingsShell(NSettingsPanel panel, VBoxContainer panelVBox)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Expected O, but got Unknown
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer val = ((Node)panelVBox).GetNodeOrNull<HBoxContainer>(new NodePath("CznModSettingsShell"));
		if (val != null && !IsCurrentSettingsShell(val))
		{
			((Node)panelVBox).RemoveChild((Node)(object)val);
			((Node)val).QueueFree();
			val = null;
		}
		if (val == null)
		{
			GodotNodeUtil.ClearChildren((Node)(object)panelVBox);
			((Control)panelVBox).CustomMinimumSize = new Vector2(1012f, 0f);
			((Control)panelVBox).SizeFlagsHorizontal = (SizeFlags)4;
			((Control)panelVBox).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
			val = new HBoxContainer
			{
				Name = StringName.op_Implicit("CznModSettingsShell"),
				CustomMinimumSize = new Vector2(1012f, 0f),
				MouseFilter = (MouseFilterEnum)2
			};
			((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			((Control)val).SizeFlagsHorizontal = (SizeFlags)4;
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			VBoxContainer val2 = CreateSettingsSidebar();
			VBoxContainer val3 = CreateSettingsContentHost();
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)panelVBox).AddChild((Node)(object)val, false, (InternalMode)0);
		}
		((GodotObject)val).SetMeta(StringName.op_Implicit("czn_mod_settings_shell_version"), Variant.op_Implicit(8));
		VBoxContainer val4 = ((Node)val).GetNodeOrNull<VBoxContainer>(new NodePath("CznModSettingsSidebar"));
		VBoxContainer val5 = ((Node)val).GetNodeOrNull<VBoxContainer>(new NodePath("CznModSettingsContentHost"));
		if (val4 == null)
		{
			val4 = CreateSettingsSidebar();
			((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val).MoveChild((Node)(object)val4, 0);
		}
		if (val5 == null)
		{
			val5 = CreateSettingsContentHost();
			((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		}
		if (!((GodotObject)panel).HasMeta(StringName.op_Implicit("czn_mod_settings_selected_page")))
		{
			((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_page"), Variant.op_Implicit("global"));
		}
		if (!((GodotObject)panel).HasMeta(StringName.op_Implicit("czn_mod_settings_selected_global_category")))
		{
			((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_global_category"), Variant.op_Implicit("general"));
		}
		return new SettingsUiShell(panelVBox, val, val4, val5);
	}

	private static bool IsCurrentSettingsShell(HBoxContainer shell)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I8
		if (!((GodotObject)shell).HasMeta(StringName.op_Implicit("czn_mod_settings_shell_version")))
		{
			return false;
		}
		Variant meta = ((GodotObject)shell).GetMeta(StringName.op_Implicit("czn_mod_settings_shell_version"), default(Variant));
		if ((long)((Variant)(ref meta)).VariantType == 2)
		{
			return ((Variant)(ref meta)).AsInt32() >= 8;
		}
		return false;
	}

	private static VBoxContainer CreateSettingsSidebar()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			Name = StringName.op_Implicit("CznModSettingsSidebar"),
			CustomMinimumSize = new Vector2(118f, 0f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).SizeFlagsHorizontal = (SizeFlags)0;
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		return val;
	}

	private static VBoxContainer CreateSettingsContentHost()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			Name = StringName.op_Implicit("CznModSettingsContentHost"),
			CustomMinimumSize = new Vector2(882f, 0f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)val).SizeFlagsHorizontal = (SizeFlags)4;
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
		return val;
	}

	private static VBoxContainer EnsureSettingsPage(VBoxContainer contentHost, string pageName, string pageId)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		VBoxContainer nodeOrNull = ((Node)contentHost).GetNodeOrNull<VBoxContainer>(new NodePath(pageName));
		if (nodeOrNull != null)
		{
			return nodeOrNull;
		}
		nodeOrNull = new VBoxContainer
		{
			Name = StringName.op_Implicit(pageName),
			CustomMinimumSize = new Vector2(882f, 0f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)nodeOrNull).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((Control)nodeOrNull).SizeFlagsHorizontal = (SizeFlags)4;
		((Control)nodeOrNull).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
		((GodotObject)nodeOrNull).SetMeta(StringName.op_Implicit("czn_mod_settings_page_id"), Variant.op_Implicit(pageId));
		((Node)contentHost).AddChild((Node)(object)nodeOrNull, false, (InternalMode)0);
		return nodeOrNull;
	}

	private static bool EnsureGlobalNavigationButton(NSettingsPanel panel, VBoxContainer sidebar, RichTextLabel templateLabel, string globalSettingsIconPath, ISkinSettingsUiText text)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected O, but got Unknown
		if (((Node)sidebar).GetNodeOrNull(new NodePath("CznModSettingsNav_Global")) != null)
		{
			return false;
		}
		Button val = CreateSettingsNavButton(templateLabel, "CznModSettingsNav_Global", "global", BuildNavigationTooltip(text.GlobalSettingsHeader, text.GlobalSettingsNavDescription), "ALL", globalSettingsIconPath, preferRawImage: true);
		((BaseButton)val).Pressed += delegate
		{
			SetSelectedPage(panel, "global");
		};
		((Node)sidebar).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)sidebar).MoveChild((Node)(object)val, 0);
		return true;
	}

	private static bool EnsureCharacterNavigationButton(VBoxContainer sidebar, SettingsUiBuildContext ctx)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		string pageId = CharacterPageId(ctx.Runtime);
		string text = CharacterNavName(ctx.Runtime);
		if (((Node)sidebar).GetNodeOrNull(new NodePath(text)) != null)
		{
			return false;
		}
		string tooltip = FormatCharacterHeader(ctx.Runtime.CharacterSettingsHeader, ctx.Runtime.CharacterSkinTarget);
		Button val = CreateSettingsNavButton(ctx.TemplateLabel, text, pageId, tooltip, BuildNavInitials(ctx.Runtime.CharacterSettingsHeader), ctx.Runtime.CharacterSettingsAvatarPath, preferRawImage: false, ctx.Runtime.CharacterSettingsAccentColor);
		((BaseButton)val).Pressed += delegate
		{
			RebuildCurrentCharacterPageDeferred(ctx);
			SetSelectedPage(ctx.Panel, pageId);
		};
		((Node)sidebar).AddChild((Node)(object)val, false, (InternalMode)0);
		return true;
	}

	private static Button CreateSettingsNavButton(RichTextLabel templateLabel, string name, string pageId, string tooltip, string fallbackText, string avatarPath, bool preferRawImage = false, string accentColor = "")
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		CznSettingsButton cznSettingsButton = new CznSettingsButton();
		((Node)cznSettingsButton).Name = StringName.op_Implicit(name);
		((BaseButton)cznSettingsButton).ToggleMode = false;
		((Button)cznSettingsButton).Text = string.Empty;
		((Control)cznSettingsButton).TooltipText = tooltip;
		((Control)cznSettingsButton).FocusMode = (FocusModeEnum)2;
		((Control)cznSettingsButton).CustomMinimumSize = new Vector2(96f, 96f);
		((Control)cznSettingsButton).SizeFlagsHorizontal = (SizeFlags)4;
		Button val = (Button)(object)cznSettingsButton;
		((GodotObject)val).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		((GodotObject)val).SetMeta(StringName.op_Implicit("czn_mod_settings_page_id"), Variant.op_Implicit(pageId));
		((GodotObject)val).SetMeta(StringName.op_Implicit("czn_mod_settings_portrait_navigation"), Variant.op_Implicit(true));
		if (!string.IsNullOrWhiteSpace(accentColor))
		{
			((GodotObject)val).SetMeta(StringName.op_Implicit("czn_nav_accent"), Variant.op_Implicit(Color.FromString(accentColor, CreamGold)));
		}
		ApplyNavButtonStyle(val, selected: false);
		CenterContainer val2 = new CenterContainer
		{
			CustomMinimumSize = new Vector2(96f, 96f),
			MouseFilter = (MouseFilterEnum)2
		};
		((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
		Control val3 = CreateNavIcon(templateLabel, fallbackText, avatarPath, preferRawImage);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		return CznSettingsInputControls.AttachNativeOwner(val);
	}

	private static Control CreateNavIcon(RichTextLabel templateLabel, string fallbackText, string avatarPath, bool preferRawImage)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Expected O, but got Unknown
		Texture2D val = LoadNavTexture(avatarPath, preferRawImage);
		if (val != null)
		{
			TextureRect val2 = new TextureRect
			{
				Texture = val,
				CustomMinimumSize = new Vector2(76f, 76f),
				MouseFilter = (MouseFilterEnum)2,
				ExpandMode = (ExpandModeEnum)1,
				StretchMode = (StretchModeEnum)5
			};
			((GodotObject)val2).Set(StringName.op_Implicit("layout_mode"), Variant.op_Implicit(2));
			return (Control)val2;
		}
		Label obj = CreateTextLabel(templateLabel, fallbackText, 2, CreamGold);
		((Control)obj).CustomMinimumSize = new Vector2(76f, 76f);
		obj.HorizontalAlignment = (HorizontalAlignment)1;
		obj.VerticalAlignment = (VerticalAlignment)1;
		return (Control)(object)obj;
	}

	private static Texture2D? LoadNavTexture(string texturePath, bool preferRawImage)
	{
		if (string.IsNullOrWhiteSpace(texturePath))
		{
			return null;
		}
		if (preferRawImage)
		{
			Texture2D val = LoadRawNavTexture(texturePath);
			if (val != null)
			{
				return val;
			}
		}
		try
		{
			Texture2D val2 = ResourceLoader.Load<Texture2D>(texturePath, (string)null, (CacheMode)1);
			if (val2 != null)
			{
				return val2;
			}
		}
		catch
		{
		}
		if (!preferRawImage)
		{
			return LoadRawNavTexture(texturePath);
		}
		return null;
	}

	private static Texture2D? LoadRawNavTexture(string texturePath)
	{
		try
		{
			if (!FileAccess.FileExists(texturePath))
			{
				return null;
			}
			Image val = Image.LoadFromFile(texturePath);
			return (Texture2D?)(object)((val == null || val.IsEmpty()) ? null : ImageTexture.CreateFromImage(val));
		}
		catch
		{
			return null;
		}
	}

	private static string BuildNavigationTooltip(string title, string description)
	{
		if (!string.IsNullOrWhiteSpace(description))
		{
			return title + "\n" + description;
		}
		return title;
	}

	private static string BuildNavInitials(string text)
	{
		string text2 = text.Trim();
		if (!string.IsNullOrEmpty(text2))
		{
			return text2.Substring(0, 1).ToUpperInvariant();
		}
		return "?";
	}

	private static void SetSelectedPage(NSettingsPanel panel, string pageId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_page"), Variant.op_Implicit(pageId));
		RefreshSettingsShellSelection(panel);
		RefreshGlobalCategorySelection(panel);
		RefreshCharacterCategorySelection(panel);
		RefreshPanelLayout(panel);
		RefreshFocusNeighbors(panel);
	}

	private static void SetSelectedGlobalCategory(NSettingsPanel panel, string categoryId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_global_category"), Variant.op_Implicit(categoryId));
		RefreshGlobalCategorySelection(panel);
		RefreshPanelLayout(panel);
		RefreshFocusNeighbors(panel);
	}

	private static void SetSelectedCharacterCategory(NSettingsPanel panel, string categoryId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_character_category"), Variant.op_Implicit(categoryId));
		RefreshCharacterCategorySelection(panel);
		RefreshPanelLayout(panel);
		RefreshFocusNeighbors(panel);
	}

	private static string GetSelectedPage(NSettingsPanel panel)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)panel).HasMeta(StringName.op_Implicit("czn_mod_settings_selected_page")))
		{
			return "global";
		}
		StringName obj = StringName.op_Implicit("czn_mod_settings_selected_page");
		Variant val = default(Variant);
		val = ((GodotObject)panel).GetMeta(obj, val);
		string text = ((object)(Variant)(ref val)/*cast due to .constrained prefix*/).ToString();
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return "global";
	}

	private static void RefreshSettingsShellSelection(NSettingsPanel panel)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetSettingsShell(panel, out SettingsUiShell shell))
		{
			return;
		}
		string text = GetSelectedPage(panel);
		bool flag = false;
		Variant val2;
		foreach (Node child in ((Node)shell.ContentHost).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				val2 = default(Variant);
				val2 = ((GodotObject)child).GetMeta(obj, val2);
				bool flag3 = (((CanvasItem)val).Visible = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), text, StringComparison.Ordinal));
				flag = flag || flag3;
			}
		}
		if (!flag)
		{
			text = "global";
			((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_page"), Variant.op_Implicit(text));
			foreach (Node child2 in ((Node)shell.ContentHost).GetChildren(false))
			{
				Control val3 = (Control)(object)((child2 is Control) ? child2 : null);
				if (val3 != null && ((GodotObject)child2).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
				{
					StringName obj2 = StringName.op_Implicit("czn_mod_settings_page_id");
					val2 = default(Variant);
					val2 = ((GodotObject)child2).GetMeta(obj2, val2);
					((CanvasItem)val3).Visible = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), text, StringComparison.Ordinal);
				}
			}
		}
		foreach (Node child3 in ((Node)shell.Sidebar).GetChildren(false))
		{
			Button val4 = (Button)(object)((child3 is Button) ? child3 : null);
			if (val4 != null && ((GodotObject)child3).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj3 = StringName.op_Implicit("czn_mod_settings_page_id");
				val2 = default(Variant);
				val2 = ((GodotObject)child3).GetMeta(obj3, val2);
				bool selected = (((BaseButton)val4).ButtonPressed = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), text, StringComparison.Ordinal));
				ApplyNavButtonStyle(val4, selected);
			}
		}
	}

	private static void RefreshGlobalCategorySelection(NSettingsPanel panel)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetSettingsShell(panel, out SettingsUiShell shell))
		{
			return;
		}
		string selectedCategory = ResolveSelectedGlobalCategory(panel, shell);
		foreach (Node child in ((Node)shell.ContentHost).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				Variant val2 = default(Variant);
				val2 = ((GodotObject)val).GetMeta(obj, val2);
				if (string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), "global", StringComparison.Ordinal))
				{
					RefreshGlobalCategoryNode((Node)(object)val, selectedCategory);
				}
			}
		}
	}

	private static void RefreshGlobalCategoryNode(Node node, string selectedCategory)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in node.GetChildren(false))
		{
			Button val = (Button)(object)((child is Button) ? child : null);
			Variant val2;
			if (val != null && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_global_category_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_global_category_id");
				val2 = default(Variant);
				val2 = ((GodotObject)child).GetMeta(obj, val2);
				bool flag2 = (((BaseButton)val).ButtonPressed = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), selectedCategory, StringComparison.Ordinal));
				((GodotObject)val).SetMeta(StringName.op_Implicit("czn_mod_settings_focus_preferred"), Variant.op_Implicit(flag2));
				ApplyCategoryButtonStyle(val, flag2);
			}
			Control val3 = (Control)(object)((child is Control) ? child : null);
			if (val3 != null && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_global_category")))
			{
				StringName obj2 = StringName.op_Implicit("czn_mod_settings_global_category");
				val2 = default(Variant);
				val2 = ((GodotObject)child).GetMeta(obj2, val2);
				((CanvasItem)val3).Visible = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), selectedCategory, StringComparison.Ordinal);
			}
			RefreshGlobalCategoryNode(child, selectedCategory);
		}
	}

	private static string ResolveSelectedGlobalCategory(NSettingsPanel panel, SettingsUiShell shell)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		string selectedGlobalCategory = GetSelectedGlobalCategory(panel);
		List<string> list = new List<string>();
		foreach (Node child in ((Node)shell.ContentHost).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((GodotObject)val).HasMeta(StringName.op_Implicit("czn_mod_settings_page_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_page_id");
				Variant val2 = default(Variant);
				val2 = ((GodotObject)val).GetMeta(obj, val2);
				if (string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), "global", StringComparison.Ordinal))
				{
					CollectGlobalCategoryIds((Node)(object)val, list);
				}
			}
		}
		if (list.Count == 0 || list.Contains(selectedGlobalCategory))
		{
			return selectedGlobalCategory;
		}
		((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_global_category"), Variant.op_Implicit(list[0]));
		return list[0];
	}

	private static void CollectGlobalCategoryIds(Node node, List<string> ids)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in node.GetChildren(false))
		{
			if (child is Button && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_global_category_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_global_category_id");
				Variant val = default(Variant);
				val = ((GodotObject)child).GetMeta(obj, val);
				string item = ((object)(Variant)(ref val)/*cast due to .constrained prefix*/).ToString();
				if (!ids.Contains(item))
				{
					ids.Add(item);
				}
			}
			CollectGlobalCategoryIds(child, ids);
		}
	}

	private static string GetSelectedGlobalCategory(NSettingsPanel panel)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)panel).HasMeta(StringName.op_Implicit("czn_mod_settings_selected_global_category")))
		{
			((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_global_category"), Variant.op_Implicit("general"));
			return "general";
		}
		StringName obj = StringName.op_Implicit("czn_mod_settings_selected_global_category");
		Variant val = default(Variant);
		val = ((GodotObject)panel).GetMeta(obj, val);
		string text = ((object)(Variant)(ref val)/*cast due to .constrained prefix*/).ToString();
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return "general";
	}

	private static void RefreshCharacterCategorySelection(NSettingsPanel panel)
	{
		if (!TryGetSettingsShell(panel, out SettingsUiShell shell))
		{
			return;
		}
		string selectedCharacterCategory = GetSelectedCharacterCategory(panel);
		foreach (Node child in ((Node)shell.ContentHost).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null)
			{
				RefreshCharacterCategoryNode((Node)(object)val, selectedCharacterCategory);
			}
		}
	}

	private static void RefreshCharacterCategoryNode(Node node, string selectedCategory)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		foreach (Node child in node.GetChildren(false))
		{
			Button val = (Button)(object)((child is Button) ? child : null);
			Variant val2;
			if (val != null && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_category_id")))
			{
				StringName obj = StringName.op_Implicit("czn_mod_settings_category_id");
				val2 = default(Variant);
				val2 = ((GodotObject)child).GetMeta(obj, val2);
				bool flag2 = (((BaseButton)val).ButtonPressed = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), selectedCategory, StringComparison.Ordinal));
				((GodotObject)val).SetMeta(StringName.op_Implicit("czn_mod_settings_focus_preferred"), Variant.op_Implicit(flag2));
				ApplyCategoryButtonStyle(val, flag2);
			}
			Control val3 = (Control)(object)((child is Control) ? child : null);
			if (val3 != null && ((GodotObject)child).HasMeta(StringName.op_Implicit("czn_mod_settings_character_category")))
			{
				StringName obj2 = StringName.op_Implicit("czn_mod_settings_character_category");
				val2 = default(Variant);
				val2 = ((GodotObject)child).GetMeta(obj2, val2);
				((CanvasItem)val3).Visible = string.Equals(((object)(Variant)(ref val2)/*cast due to .constrained prefix*/).ToString(), selectedCategory, StringComparison.Ordinal);
			}
			RefreshCharacterCategoryNode(child, selectedCategory);
		}
	}

	private static string GetSelectedCharacterCategory(NSettingsPanel panel)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)panel).HasMeta(StringName.op_Implicit("czn_mod_settings_selected_character_category")))
		{
			((GodotObject)panel).SetMeta(StringName.op_Implicit("czn_mod_settings_selected_character_category"), Variant.op_Implicit("visual"));
			return "visual";
		}
		StringName obj = StringName.op_Implicit("czn_mod_settings_selected_character_category");
		Variant val = default(Variant);
		val = ((GodotObject)panel).GetMeta(obj, val);
		string text = ((object)(Variant)(ref val)/*cast due to .constrained prefix*/).ToString();
		if (!string.IsNullOrWhiteSpace(text))
		{
			return text;
		}
		return "visual";
	}

	private static bool TryGetSettingsShell(NSettingsPanel panel, out SettingsUiShell shell)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		shell = null;
		VBoxContainer nodeOrNull = ((Node)panel).GetNodeOrNull<VBoxContainer>(new NodePath("VBoxContainer"));
		HBoxContainer val = ((nodeOrNull == null) ? null : ((Node)nodeOrNull).GetNodeOrNull<HBoxContainer>(new NodePath("CznModSettingsShell")));
		VBoxContainer val2 = ((val == null) ? null : ((Node)val).GetNodeOrNull<VBoxContainer>(new NodePath("CznModSettingsSidebar")));
		VBoxContainer val3 = ((val == null) ? null : ((Node)val).GetNodeOrNull<VBoxContainer>(new NodePath("CznModSettingsContentHost")));
		if (nodeOrNull == null || val == null || val2 == null || val3 == null)
		{
			return false;
		}
		shell = new SettingsUiShell(nodeOrNull, val, val2, val3);
		return true;
	}

	private static StyleBoxFlat CreateTransparentStyle()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = new Color(0f, 0f, 0f, 0f),
			ContentMarginLeft = 0f,
			ContentMarginRight = 0f,
			ContentMarginTop = 0f,
			ContentMarginBottom = 0f
		};
	}

	private static void ApplyNavButtonStyle(Button button, bool selected)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		Color val;
		if (!((GodotObject)button).HasMeta(StringName.op_Implicit("czn_nav_accent")))
		{
			val = CreamGold;
		}
		else
		{
			StringName obj = StringName.op_Implicit("czn_nav_accent");
			Variant val2 = default(Variant);
			val2 = ((GodotObject)button).GetMeta(obj, val2);
			val = ((Variant)(ref val2)).AsColor();
		}
		Color val3 = val;
		string[] array = new string[5] { "normal", "hover", "pressed", "focus", "disabled" };
		foreach (string text in array)
		{
			bool flag = selected;
			if (!flag)
			{
				bool flag2;
				switch (text)
				{
				case "hover":
				case "pressed":
				case "focus":
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = flag2;
			}
			bool flag3 = flag;
			StyleBoxFlat val4 = CreateNavButtonStyle(new Color(val3.R, val3.G, val3.B, 0.22f), flag3);
			val4.BorderColor = val3;
			val4.BorderWidthBottom = 6;
			int cornerRadiusTopLeft = (val4.CornerRadiusTopRight = 14);
			val4.CornerRadiusTopLeft = cornerRadiusTopLeft;
			cornerRadiusTopLeft = (val4.CornerRadiusBottomRight = 14);
			val4.CornerRadiusBottomLeft = cornerRadiusTopLeft;
			if (flag3)
			{
				val4.ShadowColor = new Color(1f, 1f, 1f, 0.6f);
				val4.ShadowSize = 3;
			}
			((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit(text), (StyleBox)(object)val4);
		}
	}

	private static void ApplyCategoryButtonStyle(Button button, bool selected)
	{
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		Color bgColor = default(Color);
		((Color)(ref bgColor))._002Ector(ModHeaderBg.R, ModHeaderBg.G, ModHeaderBg.B, 0.92f);
		Color bgColor2 = default(Color);
		((Color)(ref bgColor2))._002Ector(ModHeaderBg.R, ModHeaderBg.G, ModHeaderBg.B, 0.96f);
		Color bgColor3 = default(Color);
		((Color)(ref bgColor3))._002Ector(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.22f);
		Color bgColor4 = default(Color);
		((Color)(ref bgColor4))._002Ector(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.34f);
		Color bgColor5 = default(Color);
		((Color)(ref bgColor5))._002Ector(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.3f);
		Color borderColor = default(Color);
		((Color)(ref borderColor))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.92f);
		Color borderColor2 = default(Color);
		((Color)(ref borderColor2))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.04f);
		Color borderColor3 = default(Color);
		((Color)(ref borderColor3))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.16f);
		Color borderColor4 = default(Color);
		((Color)(ref borderColor4))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.18f);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_color"), selected ? CreamGold : DimText);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), selected ? CreamGold : TextColor);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), CreamGold);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_focus_color"), selected ? CreamGold : TextColor);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_disabled_color"), DimText);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor3, borderColor2, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor2, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor4, borderColor3, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor5, borderColor4, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("disabled"), (StyleBox)(object)CreateCategoryButtonStyle(new Color(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.5f), borderColor2, 0, 0, 0, 1));
	}

	private static void ApplyCombatModeButtonStyle(Button button, bool selected)
	{
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		Color bgColor = default(Color);
		((Color)(ref bgColor))._002Ector(ModHeaderBg.R, ModHeaderBg.G, ModHeaderBg.B, 0.94f);
		Color bgColor2 = default(Color);
		((Color)(ref bgColor2))._002Ector(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.22f);
		Color bgColor3 = default(Color);
		((Color)(ref bgColor3))._002Ector(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.34f);
		Color borderColor = default(Color);
		((Color)(ref borderColor))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.9f);
		Color borderColor2 = default(Color);
		((Color)(ref borderColor2))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.04f);
		Color borderColor3 = default(Color);
		((Color)(ref borderColor3))._002Ector(CreamGold.R, CreamGold.G, CreamGold.B, 0.16f);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_color"), selected ? CreamGold : DimText);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), selected ? CreamGold : TextColor);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), CreamGold);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_focus_color"), selected ? CreamGold : TextColor);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor2, borderColor2, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor3, borderColor3, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor3, borderColor3, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)(selected ? CreateCategoryButtonStyle(bgColor, borderColor, 4, 1, 1, 3) : CreateCategoryButtonStyle(bgColor3, borderColor3, 0, 0, 0, 1)));
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("disabled"), (StyleBox)(object)CreateCategoryButtonStyle(bgColor2, borderColor2, 0, 0, 0, 1));
	}

	private static StyleBoxFlat CreateNavButtonStyle(Color bgColor, bool accented)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = bgColor,
			BorderColor = (Color)(accented ? CreamGold : new Color(CreamGold.R, CreamGold.G, CreamGold.B, 0.24f)),
			BorderWidthLeft = ((!accented) ? 1 : 2),
			BorderWidthTop = ((!accented) ? 1 : 2),
			BorderWidthRight = ((!accented) ? 1 : 2),
			BorderWidthBottom = ((!accented) ? 1 : 2),
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomLeft = 6,
			CornerRadiusBottomRight = 6,
			ContentMarginLeft = 8f,
			ContentMarginRight = 8f,
			ContentMarginTop = 8f,
			ContentMarginBottom = 8f
		};
	}

	private static StyleBoxFlat CreateCategoryButtonStyle(Color bgColor, Color borderColor, int borderWidthLeft, int borderWidthTop, int borderWidthRight, int borderWidthBottom)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = bgColor,
			BorderColor = borderColor,
			BorderWidthLeft = borderWidthLeft,
			BorderWidthTop = borderWidthTop,
			BorderWidthRight = borderWidthRight,
			BorderWidthBottom = borderWidthBottom,
			CornerRadiusTopLeft = 3,
			CornerRadiusTopRight = 3,
			CornerRadiusBottomLeft = 3,
			CornerRadiusBottomRight = 3,
			ContentMarginLeft = 8f,
			ContentMarginRight = 8f,
			ContentMarginTop = 2f,
			ContentMarginBottom = 2f
		};
	}

	private static StyleBoxFlat CreateStatusHeaderStyle()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = StatusHeaderBg,
			BorderColor = new Color(CreamGold.R, CreamGold.G, CreamGold.B, 0.36f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 4,
			CornerRadiusTopRight = 4,
			CornerRadiusBottomLeft = 4,
			CornerRadiusBottomRight = 4,
			ContentMarginLeft = 12f,
			ContentMarginRight = 12f,
			ContentMarginTop = 6f,
			ContentMarginBottom = 6f
		};
	}

	private static StyleBoxFlat CreateSettingStripStyle(Color bgColor, bool accented)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = bgColor,
			BorderColor = new Color(CreamGold.R, CreamGold.G, CreamGold.B, accented ? 0.28f : 0.14f),
			BorderWidthLeft = (accented ? 1 : 0),
			BorderWidthTop = 0,
			BorderWidthRight = 0,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 2,
			CornerRadiusTopRight = 2,
			CornerRadiusBottomLeft = 2,
			CornerRadiusBottomRight = 2,
			ContentMarginLeft = 10f,
			ContentMarginRight = 10f,
			ContentMarginTop = (accented ? 3f : 2f),
			ContentMarginBottom = (accented ? 3f : 2f)
		};
	}

	private static StyleBoxFlat CreateCombatSummaryStatusStyle()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = new Color(SubHeaderBg.R, SubHeaderBg.G, SubHeaderBg.B, 0.16f),
			BorderColor = new Color(CreamGold.R, CreamGold.G, CreamGold.B, 0.24f),
			BorderWidthLeft = 3,
			BorderWidthTop = 0,
			BorderWidthRight = 0,
			BorderWidthBottom = 0,
			CornerRadiusTopLeft = 1,
			CornerRadiusTopRight = 1,
			CornerRadiusBottomLeft = 1,
			CornerRadiusBottomRight = 1,
			ContentMarginLeft = 12f,
			ContentMarginRight = 10f,
			ContentMarginTop = 1f,
			ContentMarginBottom = 1f
		};
	}

	private static StyleBoxFlat CreateHeaderButtonStyle(Color bgColor)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = bgColor,
			BorderColor = new Color(CreamGold.R, CreamGold.G, CreamGold.B, 0.55f),
			BorderWidthLeft = 1,
			BorderWidthTop = 1,
			BorderWidthRight = 1,
			BorderWidthBottom = 1,
			CornerRadiusTopLeft = 3,
			CornerRadiusTopRight = 3,
			CornerRadiusBottomLeft = 3,
			CornerRadiusBottomRight = 3,
			ContentMarginLeft = 10f,
			ContentMarginRight = 10f,
			ContentMarginTop = 2f,
			ContentMarginBottom = 2f
		};
	}
}

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static partial class ContextualSkinControls
{
    private const string SelectorName = "STS2SkinSelector";
    private const string DropdownName = "SkinDropdown";
    /// <summary>随从（奥斯提）专属下拉：所有角色界面共用一份，放在主皮肤下拉的左边。</summary>
    private const string OstyDropdownName = "OstyDropdown";
    private const string OstyGroupMeta = "sts2_skin_companion_group";
    internal const string CompanionHiddenMeta = "sts2_skin_companion_hidden";
    private const string OstyPopupListName = "STS2OstyChoicePopupList";
    private const string MonsterScaleSliderName = "MonsterScaleSlider";
    private const string MonsterScaleValueName = "MonsterScaleValue";
    private const string MonsterScaleLabelName = "MonsterScaleLabel";
    private const string MonsterScaleResetName = "MonsterScaleReset";
    private const string CharacterRefreshGenerationMeta = "sts2_skin_character_refresh_generation";
    private const string CharacterLoadingGenerationMeta = "sts2_skin_character_loading_generation";
    private const string CharacterLoadingOverlayName = "STS2CharacterSkinLoadingOverlay";
    private const string CharacterBundlePopupListName = "STS2CharacterBundlePopupList";
    private const string GroupMeta = "sts2_skin_group";
    private const string VariantLabelRetryMeta = "sts2_skin_variant_label_retry";
    private const string VariantLabelCachePath = "user://skin_changer_variant_labels.txt";
    private static readonly Dictionary<string, string[]> ProviderVariantLabels = new(StringComparer.Ordinal);
    private static bool variantLabelsLoaded;
    private const string UpdatingMeta = "sts2_skin_updating";
    private const string MonsterScaleGroupMeta = "sts2_skin_monster_scale_group";
    private const string MonsterBaseScaleMeta = "sts2_skin_monster_base_scale";
    private const string MonsterBaseDefaultScaleMeta = "sts2_skin_monster_base_default_scale";
    private const string MonsterAppliedScaleMeta = "sts2_skin_monster_applied_scale";
    internal const string ManagedCombatSceneMeta = "sts2_skin_managed_combat_scene";
    private static readonly Dictionary<ulong, Action> RefreshActions = [];
    private static readonly ConditionalWeakTable<Control, CharacterBackgroundHostLayout>
        CharacterBackgroundHostLayouts = new();
    private static readonly HashSet<string> LoggedMissingMultiplayerIcons =
        new(StringComparer.OrdinalIgnoreCase);
    private static bool _refreshingMonsterDisplay;
    private static readonly ReloadingReferenceCache<Font> GameFontCache = new();

    internal static bool IsRefreshingMonsterDisplay => _refreshingMonsterDisplay;

    internal static Font? GameFont => GameFontCache.Get(
        static () => ResourceLoader.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres"),
        static font => GodotObject.IsInstanceValid(font));

    private static readonly System.Reflection.FieldInfo BestiarySelectedEntryField =
        AccessTools.Field(typeof(NBestiary), "_selectedEntry");
    private static readonly System.Reflection.MethodInfo BestiarySelectMonsterMethod =
        AccessTools.Method(typeof(NBestiary), "SelectMonster", [typeof(NBestiaryEntry)]);
    private static readonly System.Reflection.MethodInfo MonsterVisualsPathGetter =
        AccessTools.PropertyGetter(typeof(MonsterModel), "VisualsPath");
    private static readonly System.Reflection.FieldInfo CharacterNameField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_name");
    private static readonly System.Reflection.FieldInfo CharacterDescriptionField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_description");
    private static readonly System.Reflection.FieldInfo CharacterRelicTitleField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_relicTitle");
    private static readonly System.Reflection.FieldInfo CharacterRelicDescriptionField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_relicDescription");
    private static readonly System.Reflection.FieldInfo CharacterRelicIconField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_relicIcon");
    private static readonly System.Reflection.FieldInfo CharacterRelicIconOutlineField =
        AccessTools.Field(typeof(NCharacterSelectScreen), "_relicIconOutline");
    private static readonly System.Reflection.MethodInfo? CharacterBackgroundWindowChangeMethod =
        AccessTools.Method(typeof(NCharacterSelectScreenBg), "OnWindowChange");

    // These paths are inputs to our isolated overlay and must not pass through another Mod's Harmony redirect.
    internal static string CanonicalScenePath(string innerPath) =>
        "res://scenes/" + innerPath.TrimStart('/') + ".tscn";

    internal static string? ProviderVisualsPath(CharacterModel model)
    {
        try
        {
            return model.GetType()
                .GetProperty("VisualsPath",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic)
                ?.GetValue(model) as string;
        }
        catch
        {
            return null;
        }
    }

    internal static bool HasCustomVisualsPath(CharacterModel model, string canonicalPath)
    {
        var providerPath = ProviderVisualsPath(model);
        return !string.IsNullOrWhiteSpace(providerPath) &&
               !string.Equals(providerPath, canonicalPath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolve the scene of a synthetic provider variant (option id ends in
    /// <c>::variant:&lt;suffix&gt;</c>). The optional compatibility pattern is described in
    /// <see cref="STS2SkinChanger.Catalog.ProviderVariantOptions"/>; providers that resolve their
    /// own variants through a getter are left to that getter when no synthetic option is chosen.
    /// </summary>
    internal static string? ResolveVariantCombatPath(CharacterModel model)
    {
        var groupId = FindGroup(model.Id.Entry)?.Id ?? model.Id.Entry.ToLowerInvariant();
        var suffix = SelectedVariantSuffix(groupId);
        if (suffix == null || suffix == "default") return null;
        var path = $"res://scenes/creature_visuals/{groupId}_{suffix}.tscn";
        return VariantSceneAvailable(groupId, suffix, path) ? path : null;
    }

    internal static string? ResolveVariantScenePath(CharacterModel model, string kind)
    {
        var groupId = FindGroup(model.Id.Entry)?.Id ?? model.Id.Entry.ToLowerInvariant();
        var suffix = SelectedVariantSuffix(groupId);
        if (suffix == null) return null;
        var sceneSuffix = suffix == "default" ? string.Empty : "_" + suffix;
        var path = kind switch
        {
            "merchant" => $"res://scenes/merchant/characters/{groupId}{sceneSuffix}_merchant.tscn",
            "rest" => $"res://scenes/rest_site/characters/{groupId}{sceneSuffix}_rest_site.tscn",
            _ => null
        };
        if (path == null) return null;
        if (suffix == "default") return path;
        return VariantSceneAvailable(groupId, suffix, path) ? path : null;
    }

    /// <summary>
    /// A synthetic option may be a <b>settings</b> variant (palette/style switch inside the pack's
    /// own settings, same scene) or a <b>scene</b> variant (its own sibling scene shipped in the
    /// PCK). Only the latter has per-variant merchant/rest scenes, and even then the pack may not
    /// ship them. Requesting a missing scene throws inside <c>NRestSiteRoom._Ready</c> and leaves
    /// the rest site unusable, so a variant scene is used only when it is really loadable;
    /// otherwise the caller keeps the game/provider default.
    /// </summary>
    private static bool VariantSceneAvailable(string groupId, string suffix, string path)
    {
        if (STS2SkinChanger.Catalog.ProviderSettingsVariantOptions.IsSettingsVariant(
                SkinService.Config.GetSelection(groupId)))
        {
            return false;
        }

        try
        {
            if (ResourceLoader.Exists(path))
            {
                return true;
            }
        }
        catch
        {
            return false;
        }

        if (ReportedMissingVariantScenes.Add(path))
        {
            ModLog.Info(
                $"变体场景不存在，已回退到默认外观：{path}" +
                $"（分组={groupId}，变体={suffix}）。该皮肤包没有为这个变体提供火堆/商人场景。");
        }

        return false;
    }

    private static readonly HashSet<string> ReportedMissingVariantScenes =
        new(StringComparer.OrdinalIgnoreCase);

    private static string? SelectedVariantSuffix(string groupId)
    {
        var selection = SkinService.GetDisplayedSelection(groupId);
        const string marker = "::variant:";
        var at = selection.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return at < 0 ? null : selection[(at + marker.Length)..].ToLowerInvariant();
    }

    internal static string CanonicalImagePath(string innerPath) =>
        "res://images/" + innerPath.TrimStart('/');

    public static void ShowCharacter(Control screen, CharacterModel character)
    {
        CharacterSelectScreenAccess.EnsureHost(screen);
        OstyModelPreviewControls.BindCharacter(screen, character.Id.Entry);
        if (!OstyModelPreviewControls.IsAllowed(screen))
        {
            OstyModelPreviewControls.Hide(screen);
            screen.GetNodeOrNull<Control>($"InfoPanel/{OstyModelPreviewControls.PanelName}")?.Set("visible", false);
        }
        CharacterModelPreviewControls.Refresh(screen, character);
        CaptureCharacterBackgroundHostLayout(screen);
        CancelCharacterDropdownSelection(screen);
        // Restore while the old provider is still active. FocusRuntimeProviderBehaviorsOnCharacters
        // deactivates it immediately afterwards, so waiting for the deferred rebuild can leave a
        // provider-owned toolbar visible over the newly selected character for one or more frames.
        ManagedSkinModLoader.RestoreCharacterPresentation(screen);
        RemoveStaleProviderCharacterSelectControls(screen, character);
        var selector = EnsureCharacterSelector(screen);
        selector.SetMeta("sc_workshop_target", character.Id.Entry.ToLowerInvariant());
        ProviderModeSelectOverlayBridge.RegisterScreenLifecycle(screen);
        ProviderModeSelectOverlayBridge.SyncExclusiveScreenPollers(screen);
        var group = FindGroup(character.Id.Entry);
        if (group != null)
        {
            SkinService.FocusRuntimeProviderBehaviorsOnGroups(
                [group.Id],
                runEnvironmentProviderIds: [],
                reason: "选角预览");
        }
        RegisterRefresh(selector, () => RebuildCharacterDisplay(screen, character, FindGroup(character.Id.Entry)?.Id ?? character.Id.Entry.ToLowerInvariant()));
        Populate(selector, group);
        Action refreshSelection = () =>
            {
                Populate(selector, group);
                if (group == null)
                {
                    return;
                }

                ScheduleCharacterRefresh(screen, character, group.Id);
                if (IsMultiplayerCharacterSelect(screen))
                {
                    MultiplayerSkinSync.OnLocalCharacterSelectionChanged(group.Id);
                }
            };
        CharacterSkinCompositionControls.Show(screen, group, refreshSelection);
        CharacterSkinBundleControls.ShowForCharacter(screen,
            group?.Id ?? character.Id.Entry.ToLowerInvariant(),
            group?.DisplayName ?? character.Title.GetFormattedText(),
            () => Populate(selector, group));
        RefreshMultiplayerSkinModeButtons(screen);
        if (group != null)
        {
            // 游戏的 SelectCharacter 每次点击都会清空 AnimatedBg 并重新实例化原版背景，
            // 所以这里必须每次重建；资源已缓存在 SkinService，重建不会再次写盘或加载。
            ScheduleCharacterRefresh(screen, character, group.Id);
            if (IsMultiplayerCharacterSelect(screen))
            {
                // SelectCharacter is also the point where the lobby changes the local player's
                // character. Publish after that state settles so peers update this row immediately
                // instead of waiting for ready or the next periodic snapshot.
                Callable.From(() =>
                    MultiplayerSkinSync.OnLocalCharacterSelectionChanged(group.Id)).CallDeferred();
            }
        }
        else
        {
            CharacterModelPreviewControls.Refresh(screen, character);
            RefreshAllCharacterButtonIcons(screen);
        }
        // 随从处于“随机皮肤”状态时，每次进入选人界面抽一次（与角色随机一致的语义）。
        var companionGroupId = SkinService.CompanionGroupIds.FirstOrDefault();
        if (companionGroupId != null && SkinService.GetCompanionRandomEnabled(companionGroupId))
        {
            SkinService.DrawCompanionRandomSkin(companionGroupId);
        }

        // Companion preview belongs only to Necrobinder; the character preview always exists.
        OstyModelPreviewControls.Refresh(screen);
        // 记下当前选人界面正在看哪个角色分组：皮肤包自己的外观快捷键要知道该切谁
        // （见 VariantHotkeyPoller）。
        VariantHotkeyPoller.NoteCharacterSelectScreen(
            screen,
            group?.Id ?? character.Id.Entry.ToLowerInvariant());
        // 皮肤包自己的代码没在执行时（两个包同名程序集只能活一个），由本 Mod 按它自己的
        // s1/s2/s3 资源接上“点角色切战损”。
        CharSelectStageCycle.Attach(screen, character);
        // Blitz has no separate SetSelectedCharacterVisuals helper: its background is written
        // inside SelectCharacter, so the overlay can only be attached from here.
        if (screen.GetType() != typeof(MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen))
        {
            ProviderModeSelectOverlayBridge.Attach(screen, character);
        }
    }

    private static bool IsMultiplayerCharacterSelect(Control screen)
    {
        try
        {
            return CharacterSelectScreenAccess.Lobby(screen)?.NetService.Type.IsMultiplayer() == true;
        }
        catch
        {
            return false;
        }
    }

    private static void ScheduleCharacterRefresh(
        Control screen,
        CharacterModel character,
        string groupId)
    {
        SkinService.PrefetchCharacterChoices(groupId);
        var generation = screen.GetMeta(CharacterRefreshGenerationMeta, 0L).AsInt64() + 1L;
        screen.SetMeta(CharacterRefreshGenerationMeta, generation);
        Callable.From(() =>
        {
            if (!GodotObject.IsInstanceValid(screen) ||
                !screen.IsInsideTree() ||
                screen.GetMeta(CharacterRefreshGenerationMeta, 0L).AsInt64() != generation)
            {
                return;
            }

            RunRefresh(() => RebuildCharacterDisplay(screen, character, groupId));
        }).CallDeferred();
    }

    public static void ShowMonster(NBestiary screen, NBestiaryEntry entry)
    {
        var selector = EnsureMonsterSelector(screen);
        SetMonsterPriorityContext(selector, ResolveMonsterSkinCategory(entry));
        // Encounter entries (the decimillipede family is registered as one encounter) have no
        // singular monsterModel. The layout still builds every segment, so use the front segment
        // as the representative for the shared group/selector instead of leaving it null.
        var monster = entry.IsDiscovered ? ResolveBestiaryMonster(entry.Entry) : null;
        selector.SetMeta("sc_workshop_target", monster?.Id.Entry.ToLowerInvariant() ?? string.Empty);
        var group = monster == null
            ? null
            : FindGroup(monster.Id.Entry, monster.GetType().Name);
        if (NRun.Instance != null)
        {
            CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(
                group == null ? [] : [group.Id],
                reason: "局内怪物图鉴", refreshCurrentRoom: true);
        }
        else
        {
            SkinService.FocusRuntimeProviderBehaviorsOnGroups(
                group == null ? [] : [group.Id],
                runEnvironmentProviderIds: [],
                reason: "怪物图鉴");
        }
        RegisterRefresh(
            selector,
            monster == null || group == null
                ? null
                : () => RebuildMonsterDisplay(screen, entry, monster, group.Id));
        Populate(selector, group);
    }

    private static MonsterModel? ResolveBestiaryMonster(BestiaryEntry entry)
    {
        if (entry.monsterModel != null)
        {
            return entry.monsterModel;
        }

        var candidates = entry.encounterModel.AllPossibleMonsters.ToArray();
        if (candidates.Length == 0)
        {
            return null;
        }

        // The multi-segment encounter has three monster models; the front segment owns the
        // canonical shared group, so it is the stable representative for selector/refresh.
        var front = candidates.FirstOrDefault(candidate =>
            candidate.Id.Entry.Equals("decimillipede_segment_front", StringComparison.OrdinalIgnoreCase));
        if (front != null)
        {
            return front;
        }

        return candidates.FirstOrDefault(candidate =>
                   FindGroup(candidate.Id.Entry, candidate.GetType().Name) != null) ??
               candidates[0];
    }

    private static HBoxContainer EnsureCharacterSelector(Control screen)
    {
        var infoPanel = screen.GetNodeOrNull<Control>("InfoPanel");
        if (infoPanel == null)
        {
            ModLog.Error("选角界面缺少 InfoPanel 节点，无法挂载皮肤选择器。");
            return new HBoxContainer();
        }

        var existing = FindCharacterSelector(screen);
        if (existing != null)
        {
            AttachCharacterSelectorDragging(screen, infoPanel, existing);
            EnsureMultiplayerSkinModePanel(screen, infoPanel);
            return existing;
        }

        var selector = BuildSelector();
        screen.AddChild(selector);
        AttachCharacterSelectorDragging(screen, infoPanel, selector);
        EnsureMultiplayerSkinModePanel(screen, infoPanel);
        ModLocalization.Bind(selector, () => RefreshLocalizedText(selector));
        return selector;
    }

    private static HBoxContainer? FindCharacterSelector(Control screen) =>
        screen.GetNodeOrNull<HBoxContainer>(SelectorName) ??
        screen.GetNodeOrNull<HBoxContainer>($"InfoPanel/{SelectorName}");

    private static void AttachCharacterSelectorDragging(
        Control screen, Control infoPanel, HBoxContainer selector) =>
        DraggableSkinControl.Attach(screen, selector, mergeButton: false,
            () => ApplyCharacterSelectorPlacement(screen, infoPanel, selector));

    private static void ApplyCharacterSelectorPlacement(
        Control screen,
        Control infoPanel,
        HBoxContainer selector)
    {
        var placement = CharacterSelectorPlacementPolicy.Resolve(
            SkinService.ShouldPlaceCharacterSelectorTopRight());
        if (placement.Host == CharacterSelectorHost.InfoPanel)
        {
            // Resolve the old default against InfoPanel, but keep one screen parent for the
            // control's entire lifetime. Reparenting during a drag interrupts input capture
            // and removes TreeExited-bound localization/refresh callbacks.
            var transform = screen.GetGlobalTransformWithCanvas().AffineInverse() *
                            infoPanel.GetGlobalTransformWithCanvas();
            var topLeft = transform * new Vector2(
                infoPanel.Size.X * placement.AnchorLeft + placement.OffsetLeft,
                infoPanel.Size.Y * placement.AnchorTop + placement.OffsetTop);
            var bottomRight = transform * new Vector2(
                infoPanel.Size.X * placement.AnchorRight + placement.OffsetRight,
                infoPanel.Size.Y * placement.AnchorBottom + placement.OffsetBottom);
            selector.AnchorLeft = selector.AnchorTop = selector.AnchorRight = selector.AnchorBottom = 0f;
            selector.OffsetLeft = topLeft.X;
            selector.OffsetTop = topLeft.Y;
            selector.OffsetRight = bottomRight.X;
            selector.OffsetBottom = bottomRight.Y;
            selector.ZIndex = 20;
            return;
        }

        selector.AnchorLeft = placement.AnchorLeft;
        selector.AnchorTop = placement.AnchorTop;
        selector.AnchorRight = placement.AnchorRight;
        selector.AnchorBottom = placement.AnchorBottom;
        selector.OffsetLeft = placement.OffsetLeft;
        selector.OffsetTop = placement.OffsetTop;
        selector.OffsetRight = placement.OffsetRight;
        selector.OffsetBottom = placement.OffsetBottom;
        selector.ZIndex = placement.Host == CharacterSelectorHost.Screen ? 20 : 0;
    }

    private const string MultiplayerSkinModePanelName = "MultiplayerSkinModePanel";
    private const string MultiplayerSkinModeMeta = "sts2_skin_mode";

    // Three mutually exclusive choices replaced the old send/receive checkboxes. Exactly one is
    // always active, so a player can never end up with a configuration where nothing works and
    // does not have to reason about two independent directions.
    private static void EnsureMultiplayerSkinModePanel(
        Control screen,
        Control infoPanel)
    {
        var panel = infoPanel.GetNodeOrNull<VBoxContainer>(MultiplayerSkinModePanelName);
        if (!IsMultiplayerCharacterSelect(screen))
        {
            if (panel != null)
            {
                panel.Visible = false;
            }
            return;
        }

        if (panel == null)
        {
            panel = new VBoxContainer
            {
                Name = MultiplayerSkinModePanelName,
                AnchorLeft = 0.5f,
                AnchorTop = 0f,
                AnchorRight = 0.5f,
                AnchorBottom = 0f,
                OffsetLeft = -260f,
                OffsetTop = -212f,
                OffsetRight = 260f,
                OffsetBottom = -92f,
                Alignment = BoxContainer.AlignmentMode.Begin
            };
            panel.AddThemeConstantOverride("separation", 2);
            AddMultiplayerSkinModeRow(panel, ModText.MultiplayerSkinModeVanilla, MultiplayerSkinMode.Vanilla);
            AddMultiplayerSkinModeRow(panel, ModText.MultiplayerSkinModeShared, MultiplayerSkinMode.Shared);
            AddMultiplayerSkinModeRow(panel, ModText.MultiplayerSkinModeMine, MultiplayerSkinMode.Mine);
            infoPanel.AddChild(panel);
        }

        panel.Visible = true;
        SyncMultiplayerSkinModeButtons(panel);
    }

    private static void AddMultiplayerSkinModeRow(
        VBoxContainer panel,
        ModText text,
        MultiplayerSkinMode mode)
    {
        var toggle = new CheckButton
        {
            Name = "MultiplayerSkinMode" + mode,
            CustomMinimumSize = new Vector2(0f, 36f),
            Alignment = HorizontalAlignment.Center,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand,
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis
        };
        toggle.SetMeta(MultiplayerSkinModeMeta, (int)mode);
        toggle.AddThemeColorOverride("font_color", new Color("fff6e2"));
        toggle.AddThemeColorOverride("font_hover_color", Colors.White);
        toggle.AddThemeColorOverride("font_pressed_color", new Color("efc850"));
        toggle.AddThemeColorOverride("font_outline_color", new Color("332f27"));
        toggle.AddThemeConstantOverride("outline_size", 3);
        toggle.AddThemeFontSizeOverride("font_size", 18);
        // The game theme draws a big white box for a toggled button; these rows are a radio group,
        // so only the little round indicator should change when one of them is active.
        foreach (var state in new[] { "normal", "hover", "pressed", "hover_pressed", "focus", "disabled" })
        {
            toggle.AddThemeStyleboxOverride(state, new StyleBoxEmpty());
        }
        toggle.AddThemeConstantOverride("icon_max_width", 26);
        if (GameFont != null)
        {
            toggle.AddThemeFontOverride("font", GameFont);
        }

        toggle.Toggled += pressed => OnMultiplayerSkinModeToggled(toggle, mode, pressed);
        panel.AddChild(toggle);
        ModLocalization.Bind(toggle, () =>
        {
            toggle.Text = ModLocalization.Get(text);
            toggle.TooltipText = ModLocalization.Get(DescribeMode(text));
        });
    }

    private static ModText DescribeMode(ModText text) => text switch
    {
        ModText.MultiplayerSkinModeVanilla => ModText.MultiplayerSkinModeVanillaHint,
        ModText.MultiplayerSkinModeShared => ModText.MultiplayerSkinModeSharedHint,
        _ => ModText.MultiplayerSkinModeMineHint
    };

    private static void OnMultiplayerSkinModeToggled(
        CheckButton toggle,
        MultiplayerSkinMode mode,
        bool pressed)
    {
        if (toggle.GetParent() is not VBoxContainer panel)
        {
            return;
        }

        if (!pressed)
        {
            // The active choice can not be switched off, otherwise the player would end up with
            // no mode selected at all.
            toggle.SetPressedNoSignal(SkinService.GetMultiplayerSkinMode() == mode);
            return;
        }

        SkinService.SetMultiplayerSkinMode(mode);
        SyncMultiplayerSkinModeButtons(panel);
    }

    private static void SyncMultiplayerSkinModeButtons(VBoxContainer panel)
    {
        if (!GodotObject.IsInstanceValid(panel))
        {
            return;
        }

        var active = (int)SkinService.GetMultiplayerSkinMode();
        foreach (var toggle in panel.GetChildren().OfType<CheckButton>())
        {
            toggle.SetPressedNoSignal(toggle.GetMeta(MultiplayerSkinModeMeta, active).AsInt32() == active);
        }
    }

    private static void RefreshMultiplayerSkinModeButtons(Control screen)
    {
        var panel = screen.GetNodeOrNull<VBoxContainer>("InfoPanel/" + MultiplayerSkinModePanelName);
        if (panel == null)
        {
            return;
        }

        panel.Visible = IsMultiplayerCharacterSelect(screen);
        if (panel.Visible)
        {
            SyncMultiplayerSkinModeButtons(panel);
        }
    }

    private static HBoxContainer EnsureMonsterSelector(NBestiary screen)
    {
        var existing = screen.GetNodeOrNull<HBoxContainer>(SelectorName);
        if (existing != null)
        {
            return existing;
        }

        var selector = BuildSelector();
        AttachMonsterPriorityControls(screen, selector);
        AddMonsterScaleControls(screen, selector);
        selector.AnchorLeft = 0.5f;
        selector.AnchorRight = 0.5f;
        selector.OffsetLeft = -350;
        selector.OffsetTop = 168;
        selector.OffsetRight = 350;
        selector.OffsetBottom = 212;
        screen.AddChild(selector);
        ModLocalization.Bind(selector, () => RefreshLocalizedText(selector));
        return selector;
    }

    private static void AddMonsterScaleControls(NBestiary screen, HBoxContainer selector)
    {
        var label = BuildCompactLabel(ModLocalization.Get(ModText.MonsterSize), 60);
        label.Name = MonsterScaleLabelName;
        var slider = new HSlider
        {
            Name = MonsterScaleSliderName,
            MinValue = SkinService.MinimumMonsterScale,
            MaxValue = SkinService.MaximumMonsterScale,
            Step = SkinService.MonsterScaleStep,
            Value = 1d,
            CustomMinimumSize = new Vector2(160, 36),
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
        };
        var valueLabel = BuildCompactLabel("100%", 62);
        valueLabel.Name = MonsterScaleValueName;
        var reset = new Button
        {
            Name = MonsterScaleResetName,
            Text = ModLocalization.Get(ModText.Reset),
            CustomMinimumSize = new Vector2(108, 38),
            FocusMode = Control.FocusModeEnum.None,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand
        };
        ApplyCompactButtonTheme(reset);
        slider.ValueChanged += value =>
            OnMonsterScaleChanged(screen, selector, (float)value);
        reset.Pressed += () => slider.Value = 1d;
        selector.AddChild(label);
        selector.AddChild(slider);
        selector.AddChild(valueLabel);
        selector.AddChild(reset);
    }

    private static void RefreshLocalizedText(HBoxContainer selector)
    {
        var groupId = selector.GetMeta(GroupMeta, string.Empty).AsString();
        var group = string.IsNullOrWhiteSpace(groupId) ? null : FindGroup(groupId);
        if (group != null)
        {
            Populate(selector, group);
        }

        var scaleLabel = selector.GetNodeOrNull<Label>(MonsterScaleLabelName);
        if (scaleLabel != null)
        {
            scaleLabel.Text = ModLocalization.Get(ModText.MonsterSize);
        }

        var reset = selector.GetNodeOrNull<Button>(MonsterScaleResetName);
        if (reset != null)
        {
            reset.Text = ModLocalization.Get(ModText.Reset);
        }

        RefreshMonsterPriorityButton(selector);
    }

    private static Label BuildCompactLabel(string text, float width)
    {
        var label = new Label
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, 38),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", new Color("fff6e2"));
        label.AddThemeColorOverride("font_outline_color", new Color("332f27"));
        label.AddThemeConstantOverride("outline_size", 4);
        label.AddThemeFontSizeOverride("font_size", 20);
        if (GameFont != null)
        {
            label.AddThemeFontOverride("font", GameFont);
        }

        return label;
    }

    private static void ApplyCompactButtonTheme(Button button)
    {
        if (GameFont != null)
        {
            button.AddThemeFontOverride("font", GameFont);
        }
        ModThemeRuntime.Button(button, 19);
    }

    private static HBoxContainer BuildSelector()
    {
        var selector = new HBoxContainer
        {
            Name = SelectorName,
            Visible = false,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        var ostyDropdown = new OptionButton
        {
            Name = OstyDropdownName,
            CustomMinimumSize = new Vector2(180, 44),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            FitToLongestItem = false,
            ClipText = true,
            Alignment = HorizontalAlignment.Center,
            Visible = false,
            TooltipText = "奥斯提皮肤（所有角色通用）"
        };
        ApplyGameTheme(ostyDropdown);
        ostyDropdown.ItemSelected += index => ApplyOstySelection(selector, ostyDropdown, checked((int)index));
        // 放在主皮肤下拉左边："角色皮肤 | 奥斯提皮肤"。
        selector.AddChild(ostyDropdown);

        var dropdown = new OptionButton
        {
            Name = DropdownName,
            CustomMinimumSize = new Vector2(244, 44),
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            FitToLongestItem = false,
            ClipText = true,
            Alignment = HorizontalAlignment.Center
        };
        ApplyGameTheme(dropdown);
        dropdown.ItemSelected += index => ApplyDropdownSelection(selector, dropdown, checked((int)index));
        selector.AddChild(dropdown);

        selector.TreeExited += () =>
        {
            var id = selector.GetInstanceId();
            RefreshActions.Remove(id);
        };
        return selector;
    }

    internal static void ApplyGameTheme(OptionButton dropdown)
    {
        if (GameFont is { } font)
        {
            dropdown.AddThemeFontOverride("font", font);
            dropdown.GetPopup().AddThemeFontOverride("font", font);
        }
        ModThemeRuntime.Button(dropdown, 23);
        ModThemeRuntime.Popup(dropdown.GetPopup());
    }

    internal static void ApplyGameTheme(Button button)
    {
        if (GameFont is { } font) button.AddThemeFontOverride("font", font);
        ModThemeRuntime.Button(button, 21);
    }

    internal static void HideCharacterSelector(Control screen)
    {
        CharacterModelPreviewControls.Hide(screen);
        OstyModelPreviewControls.Hide(screen);
        CharacterSkinCompositionControls.Hide(screen);
        CharacterSkinBundleControls.Hide(screen);
        var selector = FindCharacterSelector(screen);
        if (selector != null)
        {
            selector.Visible = false;
        }

        var modePanel = screen.GetNodeOrNull<Control>(
            $"InfoPanel/{MultiplayerSkinModePanelName}");
        if (modePanel != null)
        {
            modePanel.Visible = false;
        }

    }

    internal static bool ShouldIgnoreBackgroundMute(NMuteInBackgroundHandler handler, int notification)
    {
        const int ApplicationFocusOut = 1005;
        return notification == ApplicationFocusOut && handler.GetWindow().HasFocus();
    }

    internal static StyleBoxFlat CreateStyleBox(Color background, Color border, int borderWidth = 1)
    {
        return new StyleBoxFlat
        {
            BgColor = background,
            BorderColor = border,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth,
            CornerRadiusTopLeft = 12,
            CornerRadiusTopRight = 12,
            CornerRadiusBottomRight = 12,
            CornerRadiusBottomLeft = 12,
            ContentMarginLeft = 12,
            ContentMarginRight = 12
        };
    }

    /// <summary>
    /// Rebuilds the character-select skin dropdown for one group. A skin pack's own variant hotkey
    /// changes the pack behind the Skin Changer's back, and the bridge adopts the matching option in
    /// the config; without this the panel kept showing the old entry until the character was
    /// re-selected.
    /// </summary>
    internal static void RefreshCharacterSelectDropdown(string groupId)
    {
        try
        {
            var root = (Engine.GetMainLoop() as SceneTree)?.Root;
            if (root == null)
            {
                return;
            }

            var group = FindGroup(groupId);
            foreach (var screen in EnumerateNodes(root).OfType<Control>().Where(CharacterSelectScreenAccess.IsSupported))
            {
                if (!GodotObject.IsInstanceValid(screen))
                {
                    continue;
                }

                var selector = screen.GetNodeOrNull<HBoxContainer>(SelectorName);
                if (selector != null &&
                    GodotObject.IsInstanceValid(selector))
                {
                    Populate(selector, group);
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"刷新选角皮肤下拉失败：{exception.GetBaseException().Message}");
        }
    }

    // A provider that switches appearance through its own argument-taking entry point lists the two
    // choices in a menu of its own; reuse those words so both menus read the same. The provider menu
    // only exists while its own skin is active, so the words are remembered per language.
    private static void ApplyProviderVariantLabels(OptionButton dropdown, SkinGroup group, Control? screen)
    {
        if (screen == null || !GodotObject.IsInstanceValid(dropdown)) return;
        var variants = group.Options
            .Where(option => option.ManualCharacterVariant is { State.AppliesArgument: true })
            .ToArray();
        if (variants.Length != 2) return;
        var providerId = variants[0].EffectiveProviderId;
        var cacheKey = providerId + "|" + ModLocalization.CurrentLanguage;
        LoadProviderVariantLabels();
        var panel = providerId.Length == 0 ? null : screen.FindChild(providerId + "Options", true, false) as Control;
        var source = panel?.FindChildren("*", nameof(OptionButton), true, false)
            .OfType<OptionButton>()
            .FirstOrDefault(candidate => candidate.ItemCount == variants.Length);
        string[] labels;
        if (source != null)
        {
            labels = Enumerable.Range(0, variants.Length).Select(source.GetItemText).ToArray();
            if (!ProviderVariantLabels.TryGetValue(cacheKey, out var known) || !known.SequenceEqual(labels))
            {
                ProviderVariantLabels[cacheKey] = labels;
                SaveProviderVariantLabels();
            }
        }
        else if (ProviderVariantLabels.TryGetValue(cacheKey, out var remembered))
        {
            labels = remembered;
        }
        else
        {
            // The provider builds its menu from its own character-select hook, which may run after this
            // rebuild; retry once at the end of the frame instead of keeping the ordinal labels.
            var pending = dropdown.HasMeta(VariantLabelRetryMeta);
            dropdown.SetMeta(VariantLabelRetryMeta, true);
            if (pending) return;
            Callable.From(() =>
            {
                if (!GodotObject.IsInstanceValid(dropdown) || !GodotObject.IsInstanceValid(screen)) return;
                dropdown.RemoveMeta(VariantLabelRetryMeta);
                ApplyProviderVariantLabels(dropdown, group, screen);
            }).CallDeferred();
            return;
        }
        for (var index = 0; index < dropdown.ItemCount; index++)
        {
            var option = variants.FirstOrDefault(variant => variant.Id == dropdown.GetItemMetadata(index).AsString());
            if (option?.ManualCharacterVariant is not { } manual) continue;
            var label = VariantWord(labels[manual.Value ? 1 : 0]);
            if (string.IsNullOrWhiteSpace(label)) continue;
            dropdown.SetItemText(index, StripVariantOrdinal(option.Name) + " · " + label);
        }
    }

    // The provider menu only exists while its own skin is active, so what it said is remembered on disk;
    // otherwise the dropdown falls back to ordinals as soon as another skin is selected.
    private static void LoadProviderVariantLabels()
    {
        if (variantLabelsLoaded) return;
        variantLabelsLoaded = true;
        try
        {
            if (!Godot.FileAccess.FileExists(VariantLabelCachePath)) return;
            using var file = Godot.FileAccess.Open(VariantLabelCachePath, Godot.FileAccess.ModeFlags.Read);
            if (file == null) return;
            foreach (var line in file.GetAsText().Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.TrimEnd('\r').Split('\t');
                if (parts.Length >= 3 && parts[0].Length > 0) ProviderVariantLabels[parts[0]] = [parts[1], parts[2]];
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("读取变体标签缓存失败：" + exception.GetBaseException().Message);
        }
    }

    private static void SaveProviderVariantLabels()
    {
        try
        {
            using var file = Godot.FileAccess.Open(VariantLabelCachePath, Godot.FileAccess.ModeFlags.Write);
            if (file == null) return;
            file.StoreString(string.Join('\n', ProviderVariantLabels.Select(pair => pair.Key + "\t" + string.Join('\t', pair.Value))));
        }
        catch (Exception exception)
        {
            ModLog.Warn("写入变体标签缓存失败：" + exception.GetBaseException().Message);
        }
    }

    // The provider menu spells the whole skin name in each item; only its last word is the variant.
    private static string VariantWord(string label)
    {
        var separator = label.LastIndexOf(" · ", StringComparison.Ordinal);
        return separator >= 0 ? label[(separator + 3)..] : label;
    }

    private static string StripVariantOrdinal(string name)
    {
        var separator = name.LastIndexOf(" · ", StringComparison.Ordinal);
        return separator > 0 && name[(separator + 3)..].All(char.IsDigit) ? name[..separator] : name;
    }

    private static void Populate(HBoxContainer selector, SkinGroup? group)
    {
        var dropdown = selector.GetNode<OptionButton>(DropdownName);
        if (group != null) group = SkinService.Catalog?.Groups.FirstOrDefault(g => g.Id == group.Id) ?? group;
        dropdown.GetPopup().Hide();
        var characterScreen = CharacterSelectScreenAccess.Find(selector);
        var visualOptions = group == null
            ? []
            : SkinService.Catalog?.IsCharacterAppearanceGroup(group.Id) == true
                ? SkinService.GetCharacterSkinOptions(group.Id).ToArray()
                : group.Options.ToArray();
        var bundles = group != null && characterScreen != null && !HasMonsterPriorityContext(selector)
            ? SkinService.GetCharacterSkinBundles(group.Id)
            : [];
        var allowRandom = group != null && characterScreen != null && !HasMonsterPriorityContext(selector);
        // An author's own switch can select a variant hidden as a pack/merge ingredient.
        // Show that current choice without changing the user's hidden-source preferences.
        if (group != null && characterScreen != null &&
            group.Options.FirstOrDefault(option => option.ManualCharacterVariant != null && !option.IsComposition &&
                option.Id.Equals(SkinService.GetCharacterSelectionOptionId(group.Id), StringComparison.OrdinalIgnoreCase)) is { } currentVariant &&
            visualOptions.All(option => option.Id != currentVariant.Id)) visualOptions = [.. visualOptions, currentVariant];
        // Bestiary categories order their skins by the manual skin priority; character groups have
        // no priority screen, so this returns the list unchanged for them.
        IReadOnlyList<SkinPriorityState>? priority = null;
        if (group != null)
        {
            var display = SkinService.GetSkinListForDisplay(group.Id, visualOptions);
            visualOptions = [.. display.Options];
            priority = display.States;
        }
        if (group == null)
        {
            dropdown.Clear();
            // The previous character's dimming must not survive into the empty selector.
            SkinChoiceDimming.Bind(dropdown, (IReadOnlyList<SkinPriorityState>?)null);
            var target = selector.GetMeta("sc_workshop_target", string.Empty).AsString();
            selector.SetMeta(GroupMeta, target);
            dropdown.AddItem(ModLocalization.Get(ModText.GameDefault));
            dropdown.SetItemMetadata(0, SkinCatalog.BaseOptionId);
            SkinWorkshopEntry.Append(dropdown, characterPopup: true);
            dropdown.Select(0);
            selector.Visible = target.Length > 0;
            ConfigureCharacterBundlePopupList(selector, dropdown, 0);
            PopulateOsty(selector);
            return;
        }

        selector.SetMeta(UpdatingMeta, true);
        selector.SetMeta(GroupMeta, group.Id);
        dropdown.Clear();
        // Skins the category's priority screen switched off are shown dimmed, in the same order.
        SkinChoiceDimming.Bind(dropdown, priority);
        var hasMonsterPriorityContext = HasMonsterPriorityContext(selector);
        if (hasMonsterPriorityContext)
        {
            dropdown.AddItem(ModLocalization.Get(ModText.FollowCategory));
            dropdown.SetItemMetadata(0, SkinService.InheritMonsterSelectionId);
        }

        foreach (var bundle in bundles)
        {
            var index = dropdown.ItemCount;
            dropdown.AddItem(CharacterSkinBundlePolicy.CreateSelectionDisplayName(bundle.Name));
            dropdown.SetItemMetadata(index, CharacterSkinBundlePolicy.CreateSelectionOptionId(bundle.Name));
        }

        var defaultIndex = dropdown.ItemCount;
        dropdown.AddItem(ModLocalization.Get(ModText.GameDefault));
        dropdown.SetItemMetadata(defaultIndex, SkinCatalog.BaseOptionId);
        foreach (var option in visualOptions)
        {
            if (SkinService.Catalog?.IsCompanionGroup(group.Id) != true &&
                SkinService.Catalog?.IsCompanionOnlyProvider(option.EffectiveProviderId) == true)
            {
                continue;
            }

            var index = dropdown.ItemCount;
            dropdown.AddItem(CharacterSkinCompositionPolicy.ResolveDisplayName(
                option.Name,
                option.IsComposition,
                ModLocalization.DisplayOptionName));
            dropdown.SetItemMetadata(index, option.Id);
        }
        ApplyProviderVariantLabels(dropdown, group, characterScreen);
        if (allowRandom)
        {
            var index = dropdown.ItemCount;
            dropdown.AddItem(ModLocalization.Get(ModText.RandomCharacterSkin));
            dropdown.SetItemMetadata(index, RandomCharacterSkinPolicy.OptionId);
        }
        SkinWorkshopEntry.Append(dropdown, characterPopup: true);
        ConfigureCharacterBundlePopupList(selector, dropdown, bundles.Count + (allowRandom ? 1 : 0));

        var selected = hasMonsterPriorityContext
            ? SkinService.GetMonsterOverrideSelection(group.Id)
            : characterScreen != null ? SkinService.GetCharacterSelectionOptionId(group.Id)
            : SkinService.Config.GetSelection(group.Id);
        if (dropdown.ItemCount > 0)
        {
            var selectedIndex = Enumerable.Range(0, dropdown.ItemCount)
                .FirstOrDefault(index => dropdown.GetItemMetadata(index).AsString()
                    .Equals(selected, StringComparison.OrdinalIgnoreCase));
            dropdown.Select(selectedIndex);
        }
        ApplyCharacterBundleSelectionTheme(
            dropdown,
            IsAccentedCharacterOption(selected));
        dropdown.Visible = true;
        PopulateMonsterScale(selector, group.Id);
        selector.SetMeta(UpdatingMeta, false);
        selector.Visible = true;
        RefreshMonsterPriorityButton(selector);
        PopulateOsty(selector);
    }

    /// <summary>
    /// 随从（奥斯提）下拉：所有角色界面都显示同一份选项、同一个选择。
    /// 选项 = 游戏原版 + 各皮肤里能提供随从外观的条目；切换后随从与主人彻底解耦。
    /// </summary>
    private static void PopulateOsty(HBoxContainer selector)
    {
        EnsureCompanionHook();
        var osty = selector.GetNodeOrNull<OptionButton>(OstyDropdownName);
        if (osty == null)
        {
            return;
        }

        // 只在"角色选择"界面显示；图鉴/怪物分类等选择器不挂随从下拉。
        var screen = CharacterSelectScreenAccess.Find(selector);
        if (screen == null)
        {
            osty.Visible = false;
            return;
        }
        // 随从下拉只在它主人（骨妹）的页面显示。作者要求：除骨妹外所有角色都不显示。
        var ownerGroup = selector.GetMeta(GroupMeta, string.Empty).AsString();
        var companionOwner = SkinService.Catalog?.CompanionOwnerGroup("osty");
        screen.SetMeta(CompanionHiddenMeta, false);
        if (companionOwner != null &&
            !string.Equals(ownerGroup, companionOwner, StringComparison.OrdinalIgnoreCase))
        {
            osty.Visible = false;
            selector.SetMeta(OstyGroupMeta, string.Empty);
            // 作者要求：非主人页面连右侧的随从预览框一起不显示。
            // 面板挂在 InfoPanel 下，而且 Refresh 会无条件 ShowOsty，所以这里既收一次，
            // 也在 screen 上留标记让后续 Refresh 别再把它显示出来。
            screen.SetMeta(CompanionHiddenMeta, true);
            OstyModelPreviewControls.Hide(screen);
            var panel = screen.GetNodeOrNull<Control>($"InfoPanel/{OstyModelPreviewControls.PanelName}");
            if (panel != null)
            {
                panel.Visible = false;
            }

            return;
        }

        var groupIds = SkinService.CompanionGroupIds;
        var groupId = groupIds.FirstOrDefault();
        var group = groupId == null
            ? null
            : SkinService.Catalog?.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));

        // 没有启用任何"带 osty 资源"的皮肤包时，目录里就没有 osty 分组；但独立奥斯提 Mod
        // 自己的皮肤仍然可以在这里选，所以下拉要在这种情况下继续显示，不能整个消失。
        var impostorOptions = OstyImpostorBridge.Options;
        if (group == null &&
            impostorOptions.Count == 0 &&
            string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId))
        {
            osty.Visible = false;
            selector.SetMeta(OstyGroupMeta, string.Empty);
            return;
        }

        groupId ??= "osty";
        selector.SetMeta(OstyGroupMeta, groupId);
        OstySkinChoices.Populate(osty, groupId);
        ConfigureOstyPopupList(selector, osty);
        ApplyOstyButtonColor(osty, OstySkinChoices.Current(groupId));
        osty.Visible = true;
    }

    /// <summary>随从下拉里"非皮肤"的条目（游戏默认 / 随机 / 创意工坊）用强调色，皮肤用正文色。</summary>
    private static bool IsAccentedOstyOption(string optionId) =>
        optionId.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase) ||
        RandomCharacterSkinPolicy.IsRandom(optionId) ||
        optionId.Equals(WorkshopCatalogPolicy.CommandId, StringComparison.OrdinalIgnoreCase);

    private static void ApplyOstyButtonColor(OptionButton dropdown, string optionId)
    {
        var color = IsAccentedOstyOption(optionId) ? ModThemeRuntime.Accent : ModThemeRuntime.Text;
        foreach (var state in new[]
                 {
                     "font_color", "font_hover_color", "font_pressed_color",
                     "font_hover_pressed_color", "font_focus_color"
                 })
        {
            dropdown.AddThemeColorOverride(state, color);
        }
    }

    /// <summary>
    /// 给随从下拉装一套和角色下拉**同款外观**的彩色列表（同样的底板/高亮/字号 22/
    /// 强调色行 vs 正文色行），但点击回调走 <see cref="ApplyOstySelection"/>，
    /// 不会像上次那样误用角色下拉的回调。
    /// </summary>
    private static void ConfigureOstyPopupList(HBoxContainer selector, OptionButton dropdown)
    {
        var popup = dropdown.GetPopup();
        var list = popup.GetNodeOrNull<ItemList>(OstyPopupListName);
        if (list == null)
        {
            list = CreateChoiceItemList(OstyPopupListName);
            popup.AddChild(list);
            var popupList = list;
            popupList.ItemClicked += (clickedIndex, _, button) =>
            {
                if (button != (long)MouseButton.Left || clickedIndex < 0 ||
                    clickedIndex >= dropdown.ItemCount)
                {
                    return;
                }

                dropdown.Select((int)clickedIndex);
                popup.Hide();
                ApplyOstySelection(selector, dropdown, (int)clickedIndex);
            };
            popupList.ItemActivated += activatedIndex =>
            {
                if (activatedIndex < 0 || activatedIndex >= dropdown.ItemCount)
                {
                    return;
                }

                dropdown.Select((int)activatedIndex);
                popup.Hide();
                ApplyOstySelection(selector, dropdown, (int)activatedIndex);
            };
            popup.AboutToPopup += () =>
            {
                if (!RefreshOstyPopupList(dropdown, popupList))
                {
                    return;
                }

                Callable.From(() =>
                {
                    if (GodotObject.IsInstanceValid(popupList) && popupList.Visible && popup.Visible)
                    {
                        popupList.Position = Vector2.Zero;
                        popupList.Size = popup.Size;
                        popupList.GrabFocus();
                    }
                }).CallDeferred();
            };
        }

        RefreshOstyPopupList(dropdown, list);
    }

    private static bool RefreshOstyPopupList(OptionButton dropdown, ItemList list)
    {
        list.Clear();
        list.Visible = false;
        if (dropdown.ItemCount == 0)
        {
            return false;
        }

        for (var index = 0; index < dropdown.ItemCount; index++)
        {
            var optionId = dropdown.GetItemMetadata(index).AsString();
            var accented = IsAccentedOstyOption(optionId);
            list.AddItem(dropdown.GetItemText(index));
            list.SetItemCustomFgColor(index, SkinChoiceDimming.Row(
                accented ? ModThemeRuntime.Accent : ModThemeRuntime.Text, dropdown, optionId));
        }

        list.Visible = true;
        return true;
    }

    /// <summary>角色下拉那套彩色列表的外观，抽出来给两边共用，避免样式分叉。</summary>
    private static ItemList CreateChoiceItemList(string name)
    {
        var list = new ItemList
        {
            Name = name,
            MouseFilter = Control.MouseFilterEnum.Stop,
            SelectMode = ItemList.SelectModeEnum.Single,
            AllowReselect = true,
            SameColumnWidth = true,
            MaxColumns = 1,
            ZIndex = 100
        };
        list.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        list.AddThemeColorOverride("font_color", new Color("fff6e2"));
        list.AddThemeColorOverride("font_hovered_color", Colors.White);
        list.AddThemeColorOverride("font_selected_color", Colors.White);
        list.AddThemeFontSizeOverride("font_size", 22);
        list.AddThemeStyleboxOverride(
            "panel", CreateStyleBox(new Color("45104e"), new Color("79547e"), 2));
        list.AddThemeStyleboxOverride(
            "hovered", CreateStyleBox(new Color("2c586f"), new Color("afcdde")));
        list.AddThemeStyleboxOverride(
            "selected", CreateStyleBox(new Color("58205f"), new Color("efc850"), 2));
        ModThemeRuntime.ItemList(list);
        if (GameFont != null)
        {
            list.AddThemeFontOverride("font", GameFont);
        }

        return list;
    }

    private static void ApplyOstySelection(HBoxContainer selector, OptionButton dropdown, int index)
    {
        var groupId = selector.GetMeta(OstyGroupMeta, string.Empty).AsString();
        if (string.IsNullOrEmpty(groupId) || index < 0 || index >= dropdown.ItemCount)
        {
            return;
        }

        var optionId = dropdown.GetItemMetadata(index).AsString();

        // 与角色下拉一致：创意工坊入口只负责打开面板，不写选择。
        if (SkinWorkshopEntry.Open(optionId, selector, groupId, () => PopulateOsty(selector),
                kind: "character"))
        {
            return;
        }

        // 随机皮肤：进入随机状态（先显示游戏原版），抽取发生在每次选人界面展示时。
        if (RandomCharacterSkinPolicy.IsRandom(optionId))
        {
            if (!OstySkinChoices.Apply(groupId, optionId))
            {
                ModLog.Error($"切换奥斯提随机皮肤失败：{SkinService.LastError}");
            }

            PopulateOsty(selector);
            if (CharacterSelectScreenAccess.Find(selector) is { } randomScreen)
            {
                OstyModelPreviewControls.Refresh(randomScreen);
            }

            return;
        }

        if (!OstySkinChoices.Apply(groupId, optionId))
        {
            ModLog.Error($"切换奥斯提皮肤失败：{SkinService.LastError}");
            PopulateOsty(selector);
            return;
        }

        ModLog.Info($"奥斯提皮肤已切换到 {optionId}；此后与角色皮肤互不影响。");
        PopulateOsty(selector);
        // 骨妹选人图里奥斯提与本体是同一条骨骼，换完要按当前角色重刷一次预览。
        RefreshSelectorDisplay(selector);
        if (CharacterSelectScreenAccess.Find(selector) is { } ostyScreen)
        {
            OstyModelPreviewControls.Refresh(ostyScreen);
        }
    }

    private static bool _companionHookInstalled;

    /// <summary>订阅"随从选择在别处被改了"（独立奥斯提 Mod 自带的按钮），同步所有下拉与预览。</summary>
    private static void EnsureCompanionHook()
    {
        if (_companionHookInstalled)
        {
            return;
        }

        _companionHookInstalled = true;
        SkinService.CompanionSelectionChanged += RefreshAllOstyDropdowns;
    }

    internal static void RefreshAllOstyDropdowns()
    {
        try
        {
            var root = (Engine.GetMainLoop() as SceneTree)?.Root;
            if (root == null)
            {
                return;
            }

            foreach (var screen in EnumerateNodes(root).OfType<Control>().Where(CharacterSelectScreenAccess.IsSupported))
            {
                if (!GodotObject.IsInstanceValid(screen))
                {
                    continue;
                }

                var selector = screen.GetNodeOrNull<HBoxContainer>(SelectorName);
                if (selector != null && GodotObject.IsInstanceValid(selector))
                {
                    PopulateOsty(selector);
                }

                OstyModelPreviewControls.Refresh(screen);
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"同步奥斯提下拉失败：{exception.GetBaseException().Message}");
        }
    }

    private static void RefreshSelectorDisplay(HBoxContainer selector)
    {
        if (!RefreshActions.TryGetValue(selector.GetInstanceId(), out var refresh))
        {
            return;
        }

        Callable.From(() => RunRefresh(refresh)).CallDeferred();
    }

    private static bool IsAccentedCharacterOption(string optionId) =>
        SkinOptionStylePolicy.IsAccented(optionId);

    private static void ApplyCharacterBundleSelectionTheme(OptionButton dropdown, bool selectedBundle)
    {
        ApplyGameTheme(dropdown);
        if (!selectedBundle)
        {
            return;
        }
        ModThemeRuntime.AccentText(dropdown);
    }

    private static void ConfigureCharacterBundlePopupList(
        HBoxContainer selector,
        OptionButton dropdown,
        int bundleCount)
    {
        var popup = dropdown.GetPopup();
        var list = popup.GetNodeOrNull<ItemList>(CharacterBundlePopupListName);
        if (bundleCount <= 0 && list == null && !Enumerable.Range(0, dropdown.ItemCount)
                .Any(index => IsAccentedCharacterOption(dropdown.GetItemMetadata(index).AsString())))
        {
            return;
        }

        if (list == null)
        {
            list = new ItemList
            {
                Name = CharacterBundlePopupListName,
                MouseFilter = Control.MouseFilterEnum.Stop,
                SelectMode = ItemList.SelectModeEnum.Single,
                AllowReselect = true,
                SameColumnWidth = true,
                MaxColumns = 1,
                ZIndex = 100
            };
            list.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            list.AddThemeColorOverride("font_color", new Color("fff6e2"));
            list.AddThemeColorOverride("font_hovered_color", Colors.White);
            list.AddThemeColorOverride("font_selected_color", Colors.White);
            list.AddThemeFontSizeOverride("font_size", 22);
            list.AddThemeStyleboxOverride(
                "panel", CreateStyleBox(new Color("45104e"), new Color("79547e"), 2));
            list.AddThemeStyleboxOverride(
                "hovered", CreateStyleBox(new Color("2c586f"), new Color("afcdde")));
            list.AddThemeStyleboxOverride(
                "selected", CreateStyleBox(new Color("58205f"), new Color("efc850"), 2));
            ModThemeRuntime.ItemList(list);
            var themedList = list;
            ModThemeRuntime.Bind(list, "bundle_colors", theme =>
            {
                ApplyCharacterBundleRowColors(dropdown, themedList, ModThemeRuntime.Accent);
                CharacterSkinRandomExclusionUi.Refresh(dropdown);
            });
            if (GameFont != null)
            {
                list.AddThemeFontOverride("font", GameFont);
            }
            popup.AddChild(list);

            var popupList = list;
            popupList.ItemClicked += (clickedIndex, _, button) =>
            {
                if (button != (long)MouseButton.Left || clickedIndex < 0 || clickedIndex >= dropdown.ItemCount)
                {
                    return;
                }
                dropdown.Select((int)clickedIndex);
                popup.Hide();
                ApplyDropdownSelection(selector, dropdown, (int)clickedIndex);
            };
            popupList.ItemActivated += activatedIndex =>
            {
                if (activatedIndex < 0 || activatedIndex >= dropdown.ItemCount)
                {
                    return;
                }
                dropdown.Select((int)activatedIndex);
                popup.Hide();
                ApplyDropdownSelection(selector, dropdown, (int)activatedIndex);
            };
            popup.AboutToPopup += () =>
            {
                // The same selector survives character changes. Always read the current native
                // entries; an earlier character's colored bundle list must never reappear.
                if (!RefreshCharacterBundlePopupList(dropdown, popupList))
                {
                    return;
                }
                Callable.From(() =>
                {
                    if (GodotObject.IsInstanceValid(popupList) && popupList.Visible && popup.Visible)
                    {
                        popupList.Position = Vector2.Zero;
                        popupList.Size = popup.Size;
                        popupList.GrabFocus();
                    }
                }).CallDeferred();
            };
        }

        CharacterSkinRandomExclusionUi.Attach(dropdown, list, () =>
        {
            var id = selector.GetMeta(GroupMeta, string.Empty).AsString();
            return CharacterSelectScreenAccess.Find(selector) != null &&
                   SkinService.Catalog?.IsCharacterAppearanceGroup(id) == true ? id : null;
        });
        RefreshCharacterBundlePopupList(dropdown, list);
    }

    private static bool RefreshCharacterBundlePopupList(OptionButton dropdown, ItemList list)
    {
        list.Clear();
        list.Visible = false;
        var hasBundles = Enumerable.Range(0, dropdown.ItemCount).Any(index =>
            IsAccentedCharacterOption(dropdown.GetItemMetadata(index).AsString()));
        if (!hasBundles)
        {
            return false;
        }

        // Read names and skin ids from the current dropdown every time; an earlier character's
        // colored overlay must never reappear. Every row is written, not only the accented ones,
        // so a skin the category switched off fades without losing its accent color.
        for (var index = 0; index < dropdown.ItemCount; index++)
        {
            var optionId = dropdown.GetItemMetadata(index).AsString();
            list.AddItem(dropdown.GetItemText(index));
            list.SetItemCustomFgColor(index, SkinChoiceDimming.Row(
                IsAccentedCharacterOption(optionId) ? ModThemeRuntime.Accent : ModThemeRuntime.Text,
                dropdown,
                optionId));
        }
        if (dropdown.Selected >= 0 && dropdown.Selected < list.ItemCount)
        {
            list.Select(dropdown.Selected);
        }
        CharacterSkinRandomExclusionUi.Refresh(dropdown);
        list.Visible = true;
        return true;
    }

    private static void ApplyCharacterBundleRowColors(OptionButton dropdown, ItemList list, Color accent)
    {
        for (var index = 0; index < Math.Min(list.ItemCount, dropdown.ItemCount); index++)
        {
            var optionId = dropdown.GetItemMetadata(index).AsString();
            list.SetItemCustomFgColor(index, SkinChoiceDimming.Row(
                IsAccentedCharacterOption(optionId) ? accent : ModThemeRuntime.Text, dropdown, optionId));
        }
    }

    private static void PopulateMonsterScale(HBoxContainer selector, string groupId)
    {
        var slider = selector.GetNodeOrNull<HSlider>(MonsterScaleSliderName);
        var valueLabel = selector.GetNodeOrNull<Label>(MonsterScaleValueName);
        if (slider == null || valueLabel == null)
        {
            return;
        }

        var scale = SkinService.GetSelectedMonsterScale(groupId);
        slider.Value = scale;
        valueLabel.Text = $"{Mathf.RoundToInt(scale * 100f)}%";
    }

    private static void OnMonsterScaleChanged(
        NBestiary screen,
        HBoxContainer selector,
        float scale)
    {
        var valueLabel = selector.GetNodeOrNull<Label>(MonsterScaleValueName);
        if (valueLabel != null)
        {
            valueLabel.Text = $"{Mathf.RoundToInt(scale * 100f)}%";
        }

        if (selector.GetMeta(UpdatingMeta, false).AsBool())
        {
            return;
        }

        var groupId = selector.GetMeta(GroupMeta, string.Empty).AsString();
        if (string.IsNullOrWhiteSpace(groupId))
        {
            return;
        }

        try
        {
            SkinService.SetSelectedMonsterScale(groupId, scale);
            ApplySharedAnimationScaleToDescendants(screen, groupId);
        }
        catch (Exception exception)
        {
            ModLog.Error("保存怪物缩放失败：" + exception.Message);
        }
    }

    private static OptionButton? FindFrameworkDropdown(Control screen, string groupId)
    {
        var selector = FindCharacterSelector(screen);
        if (selector == null || !selector.GetMeta(GroupMeta, string.Empty).AsString()
                .Equals(groupId, StringComparison.OrdinalIgnoreCase)) return null;
        return selector.GetNodeOrNull<OptionButton>(DropdownName);
    }

    internal static string? GetFrameworkCycleOption(
        Control screen, string groupId, string? pendingOptionId, int direction)
    {
        var dropdown = FindFrameworkDropdown(screen, groupId);
        if (dropdown == null) return null;
        // Read the same items the player sees; packs, merges and hidden ingredients have
        // already been handled by Populate. Never expand the native SkinData registry.
        var options = Enumerable.Range(0, dropdown.ItemCount)
            .Select(index => dropdown.GetItemMetadata(index).AsString()).Where(WorkshopCatalogPolicy.IsSkinChoice).ToArray();
        var current = pendingOptionId != null && options.Contains(pendingOptionId, StringComparer.OrdinalIgnoreCase)
            ? pendingOptionId
            : SkinService.GetCharacterSelectionOptionId(groupId);
        return SkinOptionCycle.NextOption(options, current, direction);
    }

    internal static string? GetFrameworkSelectionName(
        Control screen, string groupId, string? pendingOptionId)
    {
        var dropdown = FindFrameworkDropdown(screen, groupId);
        if (dropdown == null) return null;
        var current = pendingOptionId ?? SkinService.GetCharacterSelectionOptionId(groupId);
        for (var index = 0; index < dropdown.ItemCount; index++)
            if (dropdown.GetItemMetadata(index).AsString().Equals(current, StringComparison.OrdinalIgnoreCase))
                return dropdown.GetItemText(index);
        return null;
    }

    internal static bool RequestFrameworkSelection(Control screen, string groupId, string optionId)
    {
        var dropdown = FindFrameworkDropdown(screen, groupId);
        if (dropdown == null) return false;
        var selector = (HBoxContainer)dropdown.GetParent();
        for (var index = 0; index < dropdown.ItemCount; index++)
        {
            if (!dropdown.GetItemMetadata(index).AsString().Equals(optionId, StringComparison.OrdinalIgnoreCase)) continue;
            // A newer explicit native-manager choice supersedes the pending SC warm-up.
            CancelCharacterDropdownSelection(screen);
            dropdown.Select(index);
            ApplyDropdownSelection(selector, dropdown, index);
            return true;
        }
        return false;
    }

    internal static bool RequestManualVariantSelection(Control screen, string groupId, string optionId)
    {
        if (RequestFrameworkSelection(screen, groupId, optionId)) return true;
        var dropdown = FindFrameworkDropdown(screen, groupId);
        var option = FindGroup(groupId)?.Options.FirstOrDefault(option => !option.IsComposition &&
            option.Id == optionId && option.ManualCharacterVariant != null);
        if (dropdown == null || option == null) return false;
        var index = dropdown.ItemCount;
        dropdown.AddItem(ModLocalization.DisplayOptionName(option.Name));
        dropdown.SetItemMetadata(index, option.Id);
        return RequestFrameworkSelection(screen, groupId, option.Id);
    }

    internal static bool IsCharacterSelectionLoading(Control screen) =>
        FindCharacterSelector(screen)?.GetMeta(UpdatingMeta, false).AsBool() == true;

    private static void ApplyDropdownSelection(HBoxContainer selector, OptionButton dropdown, int index)
    {
        if (selector.GetMeta(UpdatingMeta, false).AsBool())
        {
            return;
        }

        var groupId = selector.GetMeta(GroupMeta, string.Empty).AsString();
        var optionId = dropdown.GetItemMetadata(index).AsString();
        var characterScreen = CharacterSelectScreenAccess.Find(selector);
        if (SkinWorkshopEntry.Open(optionId, selector, groupId, () => Populate(selector, FindGroup(groupId)),
                kind: characterScreen != null ? "character" : "monster")) return;
        if (characterScreen != null && !HasMonsterPriorityContext(selector) && RandomCharacterSkinPolicy.IsRandom(optionId))
        {
            // Show vanilla through the same resource, preview and avatar refresh transaction
            // as an explicit vanilla choice. The random draw itself still waits for a new run.
            BeginCharacterDropdownSelection(characterScreen, selector, dropdown, index, groupId,
                SkinCatalog.BaseOptionId, preserveCharacterSkinBundle: false, selectRandomCharacterSkin: true);
            return;
        }
        if (characterScreen != null &&
            !HasMonsterPriorityContext(selector) &&
            CharacterSkinBundlePolicy.TryGetSelectionBundleName(optionId, out var bundleName))
        {
            var characterOptionId = SkinService.GetCharacterSkinBundleCharacterOption(groupId, bundleName);
            if (characterOptionId == null ||
                !SkinService.SelectCharacterSkinBundle(groupId, bundleName))
            {
                ModLog.Error($"选择皮肤包失败：{SkinService.LastError}");
                Populate(selector, FindGroup(groupId));
                return;
            }
            ApplyCharacterBundleSelectionTheme(dropdown, selectedBundle: true);
            BeginCharacterDropdownSelection(
                characterScreen,
                selector,
                dropdown,
                index,
                groupId,
                characterOptionId,
                preserveCharacterSkinBundle: true);
            return;
        }
        if (characterScreen != null && !HasMonsterPriorityContext(selector))
        {
            BeginCharacterDropdownSelection(
                characterScreen,
                selector,
                dropdown,
                index,
                groupId,
                optionId);
            return;
        }

        ApplyDropdownSelectionNow(selector, dropdown, index, groupId, optionId);
    }

    private static bool ApplyDropdownSelectionNow(
        HBoxContainer selector,
        OptionButton dropdown,
        int index,
        string groupId,
        string optionId,
        bool preserveCharacterSkinBundle = false,
        bool selectRandomCharacterSkin = false)
    {
        var previousSelections = new Dictionary<string, string>(
            SkinService.Config.Selections,
            StringComparer.OrdinalIgnoreCase);
        // Default choices may be implicit and therefore absent from the persisted dictionary.
        previousSelections[groupId] = SkinService.Config.GetSelection(groupId);
        var applied = optionId.Equals(
            SkinService.InheritMonsterSelectionId,
            StringComparison.OrdinalIgnoreCase)
            ? SkinService.FollowMonsterCategoryPriority(groupId)
            : SkinService.ApplySelection(groupId, optionId);
        if (!applied)
        {
            ModLog.Error($"界面切换失败：{SkinService.LastError}");
            CacheWriteNotice.ShowInteractiveFailure(SkinService.LastError);
            var current = HasMonsterPriorityContext(selector)
                ? SkinService.GetMonsterOverrideSelection(groupId)
                : SkinService.Config.GetSelection(groupId);
            var currentIndex = Enumerable.Range(0, dropdown.ItemCount)
                .FirstOrDefault(item => dropdown.GetItemMetadata(item).AsString()
                    .Equals(current, StringComparison.OrdinalIgnoreCase));
            dropdown.Select(currentIndex);
            return false;
        }

        var characterScreen = CharacterSelectScreenAccess.Find(selector);
        var refreshedCharacter = false;
        if (characterScreen != null && !HasMonsterPriorityContext(selector) &&
            RefreshActions.TryGetValue(selector.GetInstanceId(), out var characterRefresh))
        {
            try
            {
                characterRefresh();
                refreshedCharacter = true;
            }
            catch (Exception error)
            {
                // A selection is a presentation transaction. Never publish a new dropdown/map/
                // combat identity while the background or required preview still shows the old one.
                foreach (var key in previousSelections.Keys.Concat(SkinService.Config.Selections.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToArray())
                {
                    var previous = previousSelections.GetValueOrDefault(key, SkinCatalog.BaseOptionId);
                    if (SkinService.Config.GetSelection(key) != previous && !SkinService.ApplySelection(key, previous))
                        ModLog.Warn("恢复角色外观选择失败：" + key + "；" + SkinService.LastError);
                }
                Populate(selector, FindGroup(groupId));
                RunRefresh(characterRefresh);
                ModLog.Warn("角色外观未完整就绪，已回滚本次选择：" + error.GetBaseException().Message);
                CacheWriteNotice.ShowInteractiveFailure(error.GetBaseException().Message);
                return false;
            }
        }

        if (CharacterSelectScreenAccess.Find(selector) != null &&
            !HasMonsterPriorityContext(selector) &&
            !SkinService.SetRandomCharacterSkinEnabled(groupId, selectRandomCharacterSkin))
        {
            ModLog.Warn($"保存选角随机皮肤状态失败：{SkinService.LastError}");
        }

        if (CharacterSelectScreenAccess.Find(selector) != null &&
            !HasMonsterPriorityContext(selector) &&
            !preserveCharacterSkinBundle &&
            !SkinService.ClearSelectedCharacterSkinBundle(groupId))
        {
            ModLog.Warn($"清除选角皮肤包选择失败：{SkinService.LastError}");
        }

        selector.SetMeta(UpdatingMeta, true);
        PopulateMonsterScale(selector, groupId);
        selector.SetMeta(UpdatingMeta, false);

        // This method is shared by several selectors. The multiplayer sync method verifies that
        // groupId belongs to the current local character, so non-character selectors are ignored.
        MultiplayerSkinSync.OnLocalCharacterSelectionChanged(groupId);
        var affectedGroups = previousSelections.Keys
            .Concat(SkinService.Config.Selections.Keys)
            .Where(key => !string.Equals(
                previousSelections.GetValueOrDefault(key),
                SkinService.Config.Selections.GetValueOrDefault(key),
                StringComparison.OrdinalIgnoreCase))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        affectedGroups.Add(groupId);
        CharacterAppearanceRuntime.RefreshRunMonsterSelection(
            affectedGroups,
            "局内怪物图鉴");

        if (selector.GetParent() is NBestiary bestiary)
        {
            RefreshBestiaryMonsterNames(bestiary);
        }

        if (!refreshedCharacter && RefreshActions.TryGetValue(selector.GetInstanceId(), out var refresh))
        {
            Callable.From(() => RunRefresh(refresh)).CallDeferred();
        }
        return true;
    }

    private static void BeginCharacterDropdownSelection(
        Control screen,
        HBoxContainer selector,
        OptionButton dropdown,
        int index,
        string groupId,
        string optionId,
        bool preserveCharacterSkinBundle = false,
        bool selectRandomCharacterSkin = false)
    {
        CancelCharacterDropdownSelection(screen);
        CharacterPackPrefetch.NoteAction();
        // Keep the committed option selected while the requested replacement is being prepared.
        var previousOption = SkinService.Config.GetSelection(groupId);
        var previousIndex = Enumerable.Range(0, dropdown.ItemCount)
            .FirstOrDefault(item => dropdown.GetItemMetadata(item).AsString() == previousOption);
        dropdown.Select(previousIndex);
        // Keep the visible bundle ID while warming its underlying character skin, so a
        // second arrow click advances from the bundle rather than from its ingredient.
        FrameworkRegistryCooperation.SelectionStarting(groupId, dropdown.GetItemMetadata(index).AsString());
        var generation = screen.GetMeta(CharacterLoadingGenerationMeta, 0L).AsInt64() + 1L;
        screen.SetMeta(CharacterLoadingGenerationMeta, generation);
        selector.SetMeta(UpdatingMeta, false);
        dropdown.Disabled = false;

        var optionName = dropdown.GetItemText(index);
        var overlay = EnsureCharacterLoadingOverlay(screen, optionName);
        var progressValue = 0;
        var packPaths = SkinService.IsSameManualVariantSource(groupId, optionId)
            ? [] : SkinService.GetSelectionResourcePackPaths(groupId, optionId);
        var warmCancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var warmStarted = System.Diagnostics.Stopwatch.GetTimestamp();
        var warmTask = SkinService.WarmResourcePackFilesAsync(
            packPaths,
            progress => Interlocked.Exchange(
                ref progressValue,
                (int)Math.Round(progress * 78d)), warmCancellation.Token);

        Action poll = null!;
        poll = () =>
        {
            if (!GodotObject.IsInstanceValid(screen) ||
                !screen.IsInsideTree() ||
                screen.GetMeta(CharacterLoadingGenerationMeta, 0L).AsInt64() != generation)
            {
                warmCancellation.Cancel();
                warmCancellation.Dispose();
                return;
            }

            UpdateCharacterLoadingOverlay(overlay, optionName, Volatile.Read(ref progressValue));
            if (!warmTask.IsCompleted && System.Diagnostics.Stopwatch.GetElapsedTime(warmStarted).TotalSeconds < 10)
            {
                ScheduleNextFrame(screen, poll);
                return;
            }
            warmCancellation.Cancel();
            warmCancellation.Dispose();

            if (warmTask.IsFaulted)
            {
                ModLog.Warn(
                    $"后台预读 {optionName} 资源包失败，将直接继续挂载：" +
                    warmTask.Exception?.GetBaseException().Message);
            }

            UpdateCharacterLoadingOverlay(overlay, optionName, 82);
            ScheduleNextFrame(screen, () =>
            {
                if (!GodotObject.IsInstanceValid(screen) ||
                    screen.GetMeta(CharacterLoadingGenerationMeta, 0L).AsInt64() != generation)
                {
                    return;
                }

                selector.SetMeta(UpdatingMeta, false);
                var applied = ApplyDropdownSelectionNow(
                    selector,
                    dropdown,
                    index,
                    groupId,
                    optionId,
                    preserveCharacterSkinBundle,
                    selectRandomCharacterSkin);
                if (preserveCharacterSkinBundle && !applied)
                {
                    SkinService.ClearSelectedCharacterSkinBundle(groupId);
                }
                // Mounting can repopulate the selector before the old bundle is cleared.
                // Finish both bundle and ordinary selections from the committed state.
                Populate(selector, FindGroup(groupId));
                UpdateCharacterLoadingOverlay(overlay, optionName, 94);
                ScheduleNextFrame(screen, () =>
                {
                    if (!GodotObject.IsInstanceValid(screen) ||
                        screen.GetMeta(CharacterLoadingGenerationMeta, 0L).AsInt64() != generation)
                    {
                        return;
                    }

                    UpdateCharacterLoadingOverlay(overlay, optionName, 100);
                    dropdown.Disabled = false;
                    selector.SetMeta(UpdatingMeta, false);
                    FrameworkRegistryCooperation.QueueRefreshControls();
                    ScheduleNextFrame(screen, () => HideCharacterLoadingOverlay(screen, overlay));
                });
            });
        };

        ScheduleNextFrame(screen, poll);
    }

    private static void CancelCharacterDropdownSelection(Control screen)
    {
        var overlay = screen.GetNodeOrNull<PanelContainer>(CharacterLoadingOverlayName);
        if (overlay?.Visible != true)
        {
            return;
        }

        screen.SetMeta(
            CharacterLoadingGenerationMeta,
            screen.GetMeta(CharacterLoadingGenerationMeta, 0L).AsInt64() + 1L);
        overlay.Visible = false;
        var selector = FindCharacterSelector(screen);
        if (selector != null)
        {
            selector.SetMeta(UpdatingMeta, false);
            var dropdown = selector.GetNodeOrNull<OptionButton>(DropdownName);
            if (dropdown != null)
            {
                dropdown.Disabled = false;
            }
        }
    }

    private static PanelContainer EnsureCharacterLoadingOverlay(
        Control screen,
        string optionName)
    {
        var existing = screen.GetNodeOrNull<PanelContainer>(CharacterLoadingOverlayName);
        if (existing != null)
        {
            UpdateCharacterLoadingOverlay(existing, optionName, 0);
            existing.Visible = true;
            return existing;
        }

        var overlay = new PanelContainer
        {
            Name = CharacterLoadingOverlayName,
            MouseFilter = Control.MouseFilterEnum.Stop,
            ZAsRelative = false,
            ZIndex = 500,
            AnchorLeft = 0.5f,
            AnchorTop = 0,
            AnchorRight = 0.5f,
            AnchorBottom = 0,
            OffsetLeft = -210,
            OffsetTop = 34,
            OffsetRight = 210,
            OffsetBottom = 112
        };
        overlay.AddThemeStyleboxOverride(
            "panel",
            CreateStyleBox(new Color("2a4058e8"), new Color("efc850"), 2));
        ModThemeRuntime.Panel(overlay);
        var content = new VBoxContainer
        {
            Name = "Content",
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        overlay.AddChild(content);
        var label = new Label
        {
            Name = "Label",
            HorizontalAlignment = HorizontalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeColorOverride("font_color", new Color("fff6e2"));
        label.AddThemeFontSizeOverride("font_size", 20);
        if (GameFont != null)
        {
            label.AddThemeFontOverride("font", GameFont);
        }
        content.AddChild(label);
        var progress = new ProgressBar
        {
            Name = "Progress",
            MinValue = 0,
            MaxValue = 100,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(380, 16),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        progress.AddThemeStyleboxOverride(
            "background",
            CreateStyleBox(new Color("172433"), new Color("50606b")));
        progress.AddThemeStyleboxOverride(
            "fill",
            CreateStyleBox(new Color("efc850"), new Color("fff1a8")));
        var progressFill = (StyleBoxFlat)progress.GetThemeStylebox("fill");
        ModThemeRuntime.Bind(progress, "progress", theme =>
        {
            progressFill.BgColor = new Color(theme.AccentColor);
            progressFill.BorderColor = new Color(theme.AccentColor);
        });
        content.AddChild(progress);
        screen.AddChild(overlay);
        UpdateCharacterLoadingOverlay(overlay, optionName, 0);
        return overlay;
    }

    private static void UpdateCharacterLoadingOverlay(
        PanelContainer overlay,
        string optionName,
        int progress)
    {
        if (!GodotObject.IsInstanceValid(overlay))
        {
            return;
        }

        var value = Math.Clamp(progress, 0, 100);
        overlay.GetNode<Label>("Content/Label").Text = $"{optionName}  {value}%";
        overlay.GetNode<ProgressBar>("Content/Progress").Value = value;
    }

    private static void HideCharacterLoadingOverlay(
        Control screen,
        PanelContainer overlay)
    {
        if (GodotObject.IsInstanceValid(screen) && GodotObject.IsInstanceValid(overlay))
        {
            overlay.Visible = false;
        }
    }

    private static T? FindAncestor<T>(Node node) where T : Node
    {
        for (var parent = node.GetParent(); parent != null; parent = parent.GetParent())
        {
            if (parent is T match)
            {
                return match;
            }
        }

        return null;
    }

    private static void ScheduleNextFrame(Node owner, Action action)
    {
        if (!GodotObject.IsInstanceValid(owner) || !owner.IsInsideTree())
        {
            return;
        }

        owner.GetTree().CreateTimer(0.01d).Timeout += action;
    }

    private static void RunRefresh(Action refresh)
    {
        // 重建期间要同步读资源（还会反射触发皮肤包的同步加载），后台预加载必须让路，
        // 否则两条加载路径在 Godot 的资源加载器里互等，表现为切皮肤瞬间死锁。
        var preloadWasPaused = SkinPreloader.Paused;
        SkinPreloader.Paused = true;
        try
        {
            refresh();
        }
        catch (Exception exception)
        {
            ModLog.Error("重建皮肤展示失败：" + exception);
        }
        finally
        {
            SkinPreloader.Paused = preloadWasPaused;
        }
    }

    private static void RegisterRefresh(HBoxContainer selector, Action? refresh)
    {
        var id = selector.GetInstanceId();
        if (refresh == null)
        {
            RefreshActions.Remove(id);
        }
        else
        {
            RefreshActions[id] = refresh;
        }
    }

    private static void RebuildCharacterDisplay(Control screen, CharacterModel character, string groupId)
    {
        // F29 取证：卡死复现停在选角预览重建时，godot.log 以此行与挂载日志分界。
        ModLog.Info($"开始重建选角预览：{groupId}。");
        CharacterModelPreviewControls.Refresh(screen, character);
        // A behavior-driven provider can replace the title/description and hide the original
        // background without changing a canonical scene. Always restore the game presentation
        // first so switching *away* from such a provider is just as complete as switching to it.
        ManagedSkinModLoader.RestoreCharacterPresentation(screen);
        RestoreCharacterBackgroundHostLayout(screen);
        RestoreCharacterInfoText(screen, character);

        if (ShouldSkipExternalRuntimeRedirect(groupId))
        {
            RebuildRuntimeProviderCharacterDisplay(screen, character);
            ReplaySelectedCharacterPresentation(screen, character, groupId);
            RefreshLocalLobbyAvatar(screen);
            if (!CharacterModelPreviewControls.BindNow(screen, character))
                throw new AppearanceResourceUnavailableException("角色预览尚未就绪，停止提交不完整外观。");
            CharSelectStageCycle.Attach(screen, character);
            ProviderModeSelectOverlayBridge.Attach(screen, character);
            ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough(screen, character);
            return;
        }

        var characterId = character.Id.Entry.ToLowerInvariant();
        var characterSelectPath = CanonicalScenePath("screens/char_select/char_select_bg_" + characterId);
        var characterSelectTextures = new[]
        {
            CanonicalImagePath("packed/character_select/char_select_" + characterId + ".png"),
            CanonicalImagePath("packed/character_select/char_select_" + characterId + "_locked.png")
        };
        // Only the background and button textures are needed on this screen. Combat, rest-site
        // and merchant scenes have their own lazy replacement hooks and caches; eagerly pulling
        // all of them here made the first click decode entire animation sets for some skins.
        var resourcePaths = new[] { characterSelectPath }
            .Concat(characterSelectTextures)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        // A complete DLL skin is mounted at its original game-facing paths while it is selected,
        // but that is not enough for a hot switch: Godot may already have the canonical scene,
        // skeleton or atlas cached from the skin that was active at startup. Instantiate the
        // display through the same isolated alias overlay used by resource skins so the whole
        // dependency graph is loaded from the selected provider's files as one coherent set.
        // 随从分组（osty 等）的骨架只认 1 条动画，不能占主人选人展示的整包运行时槽位；
        // 这条分支一旦抛异常（实测 6 条动画 → NSpineAutoPlayer 报错）会卡死整局，因此：
        // ① 随从分组不走整包重建；② 任何异常都退回普通资源挂载，而不是 return。
        if (SkinService.GetSelectedFullRuntimeProvider(groupId) != null &&
            SkinService.Catalog?.IsCompanionGroup(groupId) != true)
        {
            try
            {
                RebuildMountedFullRuntimeCharacterDisplay(
                    screen,
                    character,
                    groupId,
                    characterSelectPath,
                    characterSelectTextures);
                RefreshLocalLobbyAvatar(screen);
                if (!CharacterModelPreviewControls.BindNow(screen, character))
                    throw new AppearanceResourceUnavailableException("角色预览尚未就绪，停止提交不完整外观。");
                CharSelectStageCycle.Attach(screen, character);
                ProviderModeSelectOverlayBridge.Attach(screen, character);
                ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough(screen, character);
                return;
            }
            catch (AppearanceResourceUnavailableException exception)
            {
                // A failed native dependency must not be retried through the generic fallback.
                ModLog.Warn("[资源保护] 已停止本次选角重建，保留可用展示：" + exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                ModLog.Warn(
                    $"[骨骼防护] 完整运行时皮肤重建失败，已退回资源挂载（分组={groupId}）：" +
                    $"{exception.GetBaseException().Message}");
            }
        }

        // 需要包含提供者依赖：DLL 提供者的场景可能引用其 PCK 里的编译脚本(.gdc)、导出场景(.scn)等。
        SkinService.WithRuntimeResources(
            groupId,
            resourcePaths,
            resources =>
            {
                if (TryResolveProviderDeclaredBackground(
                        character,
                        characterSelectPath,
                        out var declaredPath,
                        out var declaredScene))
                {
                    // 提供者不占用规范路径，而是给 CharacterModel.CharacterSelectBg 打补丁返回自己命名
                    // 空间下的场景（RedHood 系列 = res://RedHood*/portraits/selection.tscn）。这种包里
                    // resources[规范路径] 照样解析出游戏原版场景，「scene != null」不能说明包提供了背景，
                    // 所以必须先按 getter 的当前值判断，再考虑规范路径替换。
                    TakeOverCharacterSelectSceneCache(declaredPath, declaredScene);
                    // 提供者场景的贴图、骨骼都在它自己的命名空间下，不能按规范路径的隔离资源去重绑。
                    ReplaceCharacterBackground(screen, character, declaredScene);
                    ModLog.Info($"已按提供者声明的背景场景替换选角背景：{declaredPath}");
                }
                else
                {
                    var scene = resources[characterSelectPath] as PackedScene;
                    if (scene == null && ProviderShipsCanonicalBackground(groupId, characterId))
                    {
                        throw new InvalidOperationException($"角色选角资源不是场景：{characterSelectPath}");
                    }

                    if (scene != null)
                    {
                        // 包没有自己的规范背景（DefectSkinMod 用 DefectAnimatedBg、魅魔战士只有
                        // char_select_ironclad.png）时资源解析会退回游戏原版场景，拿它替换只会冲掉
                        // 包自己画的那套，所以只在真的取到场景时才替换。
                        TakeOverCharacterSelectSceneCache(characterSelectPath, scene);
                        // The scene must be instantiated before WithRuntimeResources restores canonical
                        // dependency paths; otherwise a skeleton/animation resource can come from the
                        // previous character skin even though the PackedScene itself loaded correctly.
                        ReplaceCharacterBackground(screen, character, scene, resources);
                    }
                }
                RefreshCharacterButtonIcon(screen, character);
                RefreshAllCharacterButtonIcons(screen);
                // CZN and similar complete packs create their own background variant and toolbar
                // from this callback. Keep the provider overlay mounted while replaying it so
                // those dynamically loaded scenes/textures resolve to the selected pack rather
                // than a stale canonical or previously selected skin.
                ReplaySelectedCharacterPresentation(screen, character, groupId);
                return true;
            },
            includeProviderDependencies: true);
        RefreshLocalLobbyAvatar(screen);
        if (!CharacterModelPreviewControls.BindNow(screen, character))
            throw new AppearanceResourceUnavailableException("角色预览尚未就绪，停止提交不完整外观。");
        CharSelectStageCycle.Attach(screen, character);
        ProviderModeSelectOverlayBridge.Attach(screen, character);
        ProviderModeSelectOverlayBridge.SyncModeBackgroundClickPassthrough(screen, character);
        ModLog.Info($"已完整重建 {character.Id.Entry} 的选角展示。");
        DescribeCharacterSelectBackground(screen, groupId, character);
        CharSelectPackRebindCompat.AfterRebuild(screen, groupId, SkinService.GetSelectedRuntimeProvider(groupId));
        CrossbowFoolSelectCompat.AfterRebuild(screen, groupId);
    }

    /// <summary>
    /// 这个皮肤包自己是否带规范选人背景场景。没有的话，资源解析会退回游戏原版场景，
    /// 此时替换背景只会冲掉包自己画的那一套，所以要跳过替换。
    /// </summary>
    private static bool ProviderShipsCanonicalBackground(string groupId, string characterId)
    {
        try
        {
            var catalog = SkinService.Catalog;
            var selection = SkinService.GetSelectedRuntimeProvider(groupId);
            if (catalog == null || string.IsNullOrEmpty(selection))
            {
                return true;
            }

            return catalog.ProviderShipsFile(
                groupId,
                selection!,
                $"scenes/screens/char_select/char_select_bg_{characterId}.tscn");
        }
        catch (Exception)
        {
            return true;
        }
    }

    /// <summary>
    /// 提供者用 <see cref="CharacterModel.CharacterSelectBg"/> 的 getter 补丁声明自己的选人背景场景时，
    /// 按 getter 当前返回值补做一次背景替换。游戏只在进入选人界面时按该 getter 实例化一次背景，
    /// 换皮肤不会重跑，所以不做这一步就会一直显示上一个皮肤或原版的背景。
    /// </summary>
    private static void ReplaceProviderDeclaredBackground(
        Control screen,
        CharacterModel character,
        string canonicalScenePath)
    {
        if (!TryResolveProviderDeclaredBackground(character, canonicalScenePath, out var scenePath, out var scene))
        {
            return;
        }

        TakeOverCharacterSelectSceneCache(scenePath, scene);
        // 提供者场景的贴图、骨骼都在它自己的命名空间下，不能按规范路径的隔离资源去重绑。
        ReplaceCharacterBackground(screen, character, scene);
        ModLog.Info($"已按提供者声明的背景场景替换选角背景：{scenePath}");
    }

    /// <summary>
    /// 读出提供者当前声明的选角背景场景。只有当 <c>CharacterModel.CharacterSelectBg</c> 给出的路径
    /// 不是游戏规范路径、并且确实能加载成场景时才算命中；其余情况一律返回 false，调用方保持原行为。
    /// </summary>
    private static bool TryResolveProviderDeclaredBackground(
        CharacterModel character,
        string canonicalScenePath,
        out string scenePath,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out PackedScene? scene)
    {
        scenePath = string.Empty;
        scene = null;
        try
        {
            var declared = character.CharacterSelectBg;
            ModLog.Info(
                $"[DEBUG-rh1] 选角背景来源：角色={character.Id.Entry} 声明={declared} " +
                $"规范={canonicalScenePath} 相同={string.Equals(declared, canonicalScenePath, StringComparison.OrdinalIgnoreCase)}");
            if (string.IsNullOrEmpty(declared) ||
                string.Equals(declared, canonicalScenePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // 提供者 PCK 不在规范路径上，游戏的 AssetCache 可能留着加载失败条目，绕过它再写回缓存。
            scene = ResourceLoader.Load<PackedScene>(
                        declared,
                        null,
                        ResourceLoader.CacheMode.IgnoreDeep) ??
                    PreloadManager.Cache.GetScene(declared);
            if (scene == null)
            {
                ModLog.Warn($"提供者声明的选角背景场景加载失败：{declared}");
                return false;
            }

            scenePath = declared;
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"替换提供者选角背景失败：{exception.GetBaseException().Message}");
            return false;
        }
    }

    /// <summary>
    /// The game only calls NRemoteLobbyPlayer.RefreshVisuals when the character changes.  A
    /// Skin Changer selection does not change the CharacterModel, so the local row keeps the
    /// texture that was assigned when the lobby was created.  Refresh the row explicitly after
    /// the deferred character rebuild; this is a direct texture assignment and never re-enters
    /// the game's full visual refresh (which can recurse while a lobby is being constructed).
    /// </summary>
    private static void RefreshLocalLobbyAvatar(Control screen)
    {
        try
        {
            if (!IsMultiplayerCharacterSelect(screen))
            {
                return;
            }

            // Do not call StartRunLobby.LocalPlayer here: its return type changed between the
            // formal and public-beta game builds (LobbyPlayer vs StartRunLobbyPlayer), which
            // makes an otherwise valid dual-version DLL fail with MissingMethodException.
            var playerId = CharacterSelectScreenAccess.Lobby(screen).NetService.NetId;
            RefreshMultiplayerPlayerIcons(playerId);
        }
        catch (Exception exception)
        {
            ModLog.Warn("刷新本机选角头像失败：" + exception.GetBaseException().Message);
        }
    }

    internal static CharacterModel GetLocalLobbyCharacter(StartRunLobby lobby)
    {
        // LocalPlayer returns LobbyPlayer on formal and StartRunLobbyPlayer on beta.
        // Resolve both the getter and player field at runtime, without embedding either ABI.
        var player = AccessTools.Property(typeof(StartRunLobby), "LocalPlayer")?.GetValue(lobby);
        return player != null &&
               AccessTools.Field(player.GetType(), "character")?.GetValue(player) is CharacterModel character
            ? character
            : throw new InvalidOperationException("选角大厅中没有可读取的本机角色。");
    }

    private static void RebuildMountedFullRuntimeCharacterDisplay(
        Control screen,
        CharacterModel character,
        string groupId,
        string scenePath,
        IReadOnlyCollection<string> texturePaths)
    {
        var resourcePaths = new[] { scenePath }
            .Concat(texturePaths)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        SkinService.WithRuntimeResources(
            groupId,
            resourcePaths,
            resources =>
            {
                var scene = resources[scenePath] as PackedScene ??
                            throw new InvalidOperationException($"完整运行时皮肤的选角资源不是场景：{scenePath}");
                // CZN's lobby variant switch loads this canonical path again with
                // ResourceLoader.CacheMode.Reuse.  Merely mounting a corrected .remap cannot
                // replace a scene that another skin already placed in Godot's global cache.
                // Make the coherently isolated scene own the canonical path so both this rebuild
                // and later provider-owned variant switches resolve to the selected skin.
                TakeOverCharacterSelectSceneCache(scenePath, scene);
                // 提供者可能根本不占用规范路径，而是给 CharacterModel.CharacterSelectBg 打补丁返回自己
                // 命名空间下的背景场景（RedHood 系列 = res://RedHood*/portraits/selection.tscn）。那种
                // 情况下 resources[规范路径] 解析出来的是游戏原版场景，拿它替换等于把背景换回原版。
                if (TryResolveProviderDeclaredBackground(character, scenePath, out var declaredPath, out var declaredScene))
                {
                    TakeOverCharacterSelectSceneCache(declaredPath, declaredScene);
                    ReplaceCharacterBackground(screen, character, declaredScene);
                    ModLog.Info($"已按提供者声明的背景场景替换选角背景：{declaredPath}");
                }
                else
                {
                    // Instantiate while the alias pack is mounted. PackedScene external resources
                    // are often resolved at Instantiate(), not at Load(), so loading only the scene
                    // object is insufficient to prevent a previous skin's skeleton/atlas binding.
                    ReplaceCharacterBackground(screen, character, scene, resources);
                }

                RefreshCharacterButtonIcon(screen, character);
                RefreshAllCharacterButtonIcons(screen);
                // Replay provider-owned character-select presentation before WithRuntimeResources
                // restores canonical paths. This is required for CZN's dynamic preview/background
                // scenes and keeps their two toolbar controls tied to the selected package.
                ReplaySelectedCharacterPresentation(screen, character, groupId);
                return true;
            },
            includeProviderDependencies: true);

        ModLog.Info($"已从隔离依赖路径重建完整 DLL 皮肤 {character.Id.Entry} 的选角展示。");
        CharSelectPackRebindCompat.AfterRebuild(screen, groupId, SkinService.GetSelectedRuntimeProvider(groupId));
        CrossbowFoolSelectCompat.AfterRebuild(screen, groupId);
        DescribeCharacterSelectBackground(screen, groupId, character);
    }

    /// <summary>
    /// 诊断用：把选人界面背景容器的子节点现状打出来。皮肤包自己的背景（例如 DefectSkinMod 的
    /// <c>DefectAnimatedBg</c>）如果被我们的重建整段换掉，这里会看不到它；如果还在但不可见，
    /// 就会显示 <c>可见=False</c>。只有这两种情况之一，才会出现「背景退回原版」。
    /// </summary>
    /// <summary>
    /// Re-applies only the selected skin's character-select background. Used by the Blitz/Dread
    /// screens after the game rewrites their background container outside SelectCharacter (the
    /// Dread win-streak animation calls its own SetSelectedCharacterVisuals directly).
    /// </summary>
    internal static void ReapplySelectedBackground(Control screen, CharacterModel character)
    {
        var group = FindGroup(character.Id.Entry);
        if (group == null || !GodotObject.IsInstanceValid(screen))
        {
            return;
        }

        var characterId = character.Id.Entry.ToLowerInvariant();
        var scenePath = CanonicalScenePath("screens/char_select/char_select_bg_" + characterId);
        SkinService.WithRuntimeResources(
            group.Id,
            [scenePath],
            resources =>
            {
                var scene = resources[scenePath] as PackedScene;
                if (scene == null)
                {
                    // 这个包没有规范背景场景（它自己画一层），但可能通过 CharacterSelectBg 声明了自己的场景。
                    if (TryResolveProviderDeclaredBackground(
                            character,
                            scenePath,
                            out var declaredPath,
                            out var declaredScene))
                    {
                        TakeOverCharacterSelectSceneCache(declaredPath, declaredScene);
                        ReplaceCharacterBackground(screen, character, declaredScene);
                        ModLog.Info($"已按提供者声明的背景场景替换选角背景：{declaredPath}");
                        return true;
                    }

                    return false;
                }

                TakeOverCharacterSelectSceneCache(scenePath, scene);
                ReplaceCharacterBackground(screen, character, scene, resources);
                return true;
            });
    }

    private static void DescribeCharacterSelectBackground(
        Control screen,
        string groupId,
        CharacterModel character)
    {
        try
        {
            var animated = CharacterSelectScreenAccess.Background(screen);
            var parts = new List<string>();
            if (animated != null)
            {
                parts.Add($"AnimatedBg 子节点={animated.GetChildCount()}");
                foreach (var child in animated.GetChildren())
                {
                    var item = child as CanvasItem;
                    parts.Add(
                        $"{child.Name}[{child.GetType().Name}] 可见={item?.Visible} " +
                        $"在树可见={item?.IsVisibleInTree()} 子={child.GetChildCount()}");
                }
            }

            var container = screen.GetNodeOrNull<Node>("BgContainer");
            if (container != null)
            {
                parts.Add($"BgContainer 子节点={container.GetChildCount()}");
                foreach (var child in container.GetChildren())
                {
                    var item = child as CanvasItem;
                    parts.Add(
                        $"{child.Name}[{child.GetType().Name}] 可见={item?.Visible} " +
                        $"在树可见={item?.IsVisibleInTree()} 子={child.GetChildCount()}");
                }
            }

            ModLog.Info($"[选角背景] {groupId}/{character.Id.Entry}：{string.Join("；", parts)}");
        }
        catch (Exception)
        {
        }
    }

    private static void RestoreCharacterInfoText(
        Control screen,
        CharacterModel character)
    {
        try
        {
            var button = FindCharacterButton(screen, character);
            var title = button?.IsLocked == true
                ? new LocString("main_menu_ui", "CHARACTER_SELECT.locked.title").GetFormattedText()
                : new LocString("characters", character.CharacterSelectTitle).GetFormattedText();
            var nameLabel = CharacterSelectScreenAccess.Read(CharacterNameField, screen);
            AccessTools.Method(nameLabel?.GetType(), "SetTextAutoSize", [typeof(string)])?
                .Invoke(nameLabel, [title]);

            if (CharacterSelectScreenAccess.Read(CharacterDescriptionField, screen) is RichTextLabel description)
            {
                description.Text = button?.IsLocked == true
                    ? character.GetUnlockText().GetFormattedText()
                    : new LocString(
                        "characters",
                        character.CharacterSelectDesc).GetFormattedText();
            }

            var relic = character.StartingRelics.FirstOrDefault();
            if (relic == null)
            {
                return;
            }

            var isLocked = button?.IsLocked == true;
            if (CharacterSelectScreenAccess.Read(CharacterRelicTitleField, screen) is RichTextLabel relicTitle)
            {
                relicTitle.Text = isLocked
                    ? new LocString(
                        "main_menu_ui",
                        "CHARACTER_SELECT.lockedRelic.title").GetFormattedText()
                    : relic.Title.GetFormattedText();
            }

            if (CharacterSelectScreenAccess.Read(CharacterRelicDescriptionField, screen) is RichTextLabel relicDescription)
            {
                relicDescription.Text = isLocked
                    ? new LocString(
                        "main_menu_ui",
                        "CHARACTER_SELECT.lockedRelic.description").GetFormattedText()
                    : relic.DynamicDescription.GetFormattedText();
            }

            if (CharacterSelectScreenAccess.Read(CharacterRelicIconField, screen) is TextureRect relicIcon)
            {
                relicIcon.Texture = relic.Icon;
                relicIcon.SelfModulate = isLocked ? StsColors.ninetyPercentBlack : Colors.White;
            }

            if (CharacterSelectScreenAccess.Read(CharacterRelicIconOutlineField, screen) is TextureRect relicIconOutline)
            {
                relicIconOutline.Texture = relic.IconOutline;
                relicIconOutline.SelfModulate = isLocked
                    ? StsColors.halfTransparentWhite
                    : StsColors.halfTransparentBlack;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"恢复 {character.Id.Entry} 的选角文字失败：{exception.GetBaseException().Message}");
        }
    }

    private static void ReplaySelectedCharacterPresentation(
        Control screen,
        CharacterModel character,
        string groupId)
    {
        var providerId = SkinService.GetSelectedCreatureRuntimeProvider(groupId);
        if (providerId == null)
        {
            return;
        }

        var existingNodes = EnumerateNodes(screen)
            .Select(node => node.GetInstanceId())
            .ToHashSet();
        var button = FindCharacterButton(screen, character);
        if (button != null)
        {
            ManualCharacterVariantBridge.ApplyLocalChoice(groupId);
            ManagedSkinModLoader.ReplaySelectedCharacterPresentation(
                providerId,
                screen,
                button,
                character,
                () =>
                {
                    // Include framework/static art in the same reversible presentation snapshot.
                    FrameworkSelectPresentationBridge.Reapply(screen, character, button);
                    IndependentFoolSkinCompat.ApplySelectionBackground(screen, groupId);
                    LegacyDefectPresentationCompat.ApplyModeBackground(screen, groupId);
                    StabilizeProviderCharacterSelectControls(screen, existingNodes, providerId);
                });
            ManualCharacterVariantBridge.BindScreen(screen, groupId, providerId);
        }

        var animatedBackground = CharacterSelectScreenAccess.Background(screen);
        var sceneRoot = animatedBackground?.GetChildCount() > 0
            ? animatedBackground.GetChild(0)
            : null;
        if (sceneRoot != null)
        {
            ManagedCharacterAnimationBridge.TryStartCharacterSelectLoops(sceneRoot, providerId);
            ManagedSlotVisibilityBridge.BindPreview(sceneRoot, groupId, providerId);
        }

    }

    private static void RemoveStaleProviderCharacterSelectControls(
        Control screen,
        CharacterModel? character)
    {
        // A runtime (DLL/framework) provider attaches its own character-select toolbar and names
        // its root "...CharacterSelectOptionsPanel" (CZN packs put the background / portrait
        // switch there). The sweep below deletes every node with that suffix, which included the
        // toolbar the provider had just attached, so the player was left with only the default
        // background and no way to switch. Let the provider manage its own toolbar lifecycle.
        var sweepGroup = character == null ? null : FindGroup(character.Id.Entry);
        if (sweepGroup != null &&
            SkinService.Catalog?.IsRuntimeProviderOption(
                sweepGroup.Id,
                SkinService.GetDisplayedSelection(sweepGroup.Id)) == true)
        {
            return;
        }

        var staleRoots = EnumerateNodes(screen)
            .OfType<Control>()
            .Where(control => control.Name.ToString().EndsWith(
                "CharacterSelectOptionsPanel",
                StringComparison.OrdinalIgnoreCase))
            .ToArray();
        foreach (var staleRoot in staleRoots)
        {
            var parent = staleRoot.GetParent();
            if (parent != null && GodotObject.IsInstanceValid(parent))
            {
                parent.RemoveChildSafely(staleRoot);
            }

            staleRoot.QueueFreeSafely();
        }

        if (staleRoots.Length > 0)
        {
            ModLog.Info($"已清理 {staleRoots.Length} 个上一角色遗留的选角交互面板。");
        }
    }

    /// <summary>
    /// Some complete DLL skins create their own compact character-select toolbar from the
    /// SelectCharacter callback (background variant, presentation mode, and similar controls).
    /// UI replacements may clip that toolbar or draw a later full-screen layer above it. Keep
    /// newly replayed interactive roots visible and above presentation art without recognizing a
    /// particular Mod, node name, or button label.
    /// </summary>
    private static void StabilizeProviderCharacterSelectControls(
        Control screen,
        IReadOnlySet<ulong> existingNodes,
        string providerId)
    {
        var interactiveRoots = EnumerateNodes(screen)
            .Where(node => !existingNodes.Contains(node.GetInstanceId()))
            .OfType<Control>()
            .Where(root =>
                root.Name.ToString().Contains("CharacterSelectOptionsPanel", StringComparison.OrdinalIgnoreCase) ||
                root.GetNodeOrNull<Node>("Options") != null ||
                root.GetNodeOrNull<Node>("ButtonContainer") != null)
            .ToArray();
        if (interactiveRoots.Length == 0)
        {
            return;
        }

        foreach (var root in interactiveRoots)
        {
            StabilizeProviderCharacterSelectControl(screen, root);
        }

        foreach (var root in interactiveRoots)
        {
            var parent = root.GetParent();
            var parentVisible = parent is CanvasItem parentCanvas ? parentCanvas.Visible : true;
            var ancestry = new List<string>();
            for (Node? node = root; node != null && node != screen; node = node.GetParent())
            {
                if (node is Control ancestor)
                {
                    ancestry.Add(
                        $"{ancestor.Name}:v={ancestor.Visible},clip={ancestor.ClipContents}," +
                        $"global={ancestor.GlobalPosition},size={ancestor.Size},a={ancestor.Modulate.A:0.###}");
                }
            }

            var childStates = root.GetChildren()
                .OfType<CanvasItem>()
                .Select(child => $"{child.Name}:v={child.Visible},a={child.Modulate.A:0.###}")
                .ToArray();
            ModLog.Info(
                $"选角交互面板诊断 provider={providerId} 节点={root.GetPath()} " +
                $"visible={root.Visible} parentVisible={parentVisible} " +
                $"position={root.Position} global={root.GlobalPosition} size={root.Size} z={root.ZIndex} " +
                $"alpha={root.Modulate.A:0.###} " +
                $"ancestors=[{string.Join(" | ", ancestry)}] " +
                $"children=[{string.Join(" | ", childStates)}]");
        }

        // Container layouts and third-party UI patches can run after the provider callback. Apply
        // the same normalization once more after the tree has settled; this changes no authored
        // size or position unless the whole control ended up outside the visible viewport.
        Callable.From(() =>
        {
            if (!GodotObject.IsInstanceValid(screen))
            {
                return;
            }

            foreach (var root in interactiveRoots)
            {
                if (GodotObject.IsInstanceValid(root) && root.IsInsideTree())
                {
                    StabilizeProviderCharacterSelectControl(screen, root);
                }
            }
        }).CallDeferred();

        ModLog.Info($"已保留 {providerId} 的 {interactiveRoots.Length} 个选角交互面板。");
    }

    private static void StabilizeProviderCharacterSelectControl(
        Control screen,
        Control control)
    {
        control.Visible = true;
        control.ZAsRelative = false;
        control.ZIndex = Math.Max(control.ZIndex, 100);
        control.MoveToFront();

        // The character strip owns its clipping (including arrows/pagination). Only the
        // provider toolbar may escape that boundary, without changing its authored node path.
        control.ClipContents = false;
        ProviderToolbarCanvas.Bind(control);
    }

    private static void RebuildRuntimeProviderCharacterDisplay(
        Control screen,
        CharacterModel character)
    {
        var scenePath = character.CharacterSelectBg;
        // 提供者 PCK 未被全局挂载，游戏 AssetCache 可能已缓存过加载失败（"Asset previously
        // failed to load"）。绕过它直接加载，成功后把结果写回缓存以治愈失效条目。
        var scene = ResourceLoader.Load<PackedScene>(
            scenePath,
            null,
            ResourceLoader.CacheMode.IgnoreDeep);
        if (scene != null)
        {
            TakeOverCharacterSelectSceneCache(scenePath, scene);
            PreloadManager.Cache.SetAsset(scenePath, scene);
        }
        else
        {
            scene = PreloadManager.Cache.GetScene(scenePath);
        }

        ReplaceCharacterBackground(screen, character, scene);

        var button = FindCharacterButton(screen, character);
        if (button != null)
        {
            var fallback = button.IsLocked
                ? character.CharacterSelectLockedIcon
                : character.CharacterSelectIcon;
            button.GetNode<TextureRect>("%Icon").Texture =
                SkinService.GetCharacterSelectIcon(character.Id.Entry.ToLowerInvariant(),
                    character.Id.Entry, button.IsLocked) ?? fallback;
        }

        // 切皮肤会替换选人头像的贴图文件，其它角色的按钮也可能受影响，一起按当前皮肤重刷。
        RefreshAllCharacterButtonIcons(screen);

        ModLog.Info($"已由 DLL 皮肤提供器重建 {character.Id.Entry} 的选角展示。");
    }

    /// <summary>把所有角色按钮的头像按当前选择重刷一遍（绕开资源缓存）。</summary>
    private static void RefreshAllCharacterButtonIcons(Control screen)
    {
        foreach (var candidate in EnumerateNodes(screen).OfType<NCharacterSelectButton>())
        {
            var character = candidate.Character;
            if (character == null || candidate.IsRandom || !GodotObject.IsInstanceValid(candidate) ||
                FindGroup(character.Id.Entry) == null) continue;
            try
            {
                CharacterStripPortraits.Refresh(candidate, character, () =>
                {
                    var icon = SkinService.GetCharacterSelectIcon(character.Id.Entry.ToLowerInvariant(),
                        character.Id.Entry, candidate.IsLocked) ?? (candidate.IsLocked
                            ? character.CharacterSelectLockedIcon : character.CharacterSelectIcon);
                    SetCharacterButtonIcon(candidate, icon);
                });
            }
            catch (Exception exception)
            {
                // One third-party character cannot prevent later buttons from refreshing.
                ModLog.Warn($"刷新 {character.Id.Entry} 角色头像失败：" + exception.GetBaseException().Message);
            }
        }
    }

    internal static void SetCharacterButtonIcon(NCharacterSelectButton button, Texture2D icon)
    {
        button.GetNodeOrNull<TextureRect>("%Icon")?.SetTexture(icon);
        button.GetNodeOrNull<TextureRect>("%IconAdd")?.SetTexture(icon);
    }

    private static void TakeOverCharacterSelectSceneCache(string scenePath, PackedScene scene)
    {
        // Resource.take_over_path replaces the ResourceLoader cache entry itself.  The game's
        // PreloadManager cache is separate and is updated as well because base-game and skin
        // providers use both loading routes.  The next skin rebuild repeats this operation with
        // its own isolated scene, so the ownership follows the active selection instead of a
        // provider that happened to load first at startup.
        scene.TakeOverPath(scenePath);
        PreloadManager.Cache.SetAsset(scenePath, scene);
    }

    private static void ReplaceCharacterBackground(
        Control screen,
        CharacterModel character,
        PackedScene scene,
        IReadOnlyDictionary<string, Resource>? isolatedResources = null)
    {
        var container = CharacterSelectScreenAccess.Background(screen);
        if (container == null)
        {
            ModLog.Error("选角界面缺少 AnimatedBg 节点，无法替换角色背景。");
            return;
        }

        // Provider presentation callbacks are allowed to temporarily hide this host while their
        // own full-screen layer is selected. A normal Skin Changer rebuild always owns this host.
        container.Visible = true;

        var group = FindGroup(character.Id.Entry, character.GetType().Name);
        var identity = group == null ? (CharacterAppearanceIdentity?)null : SkinService.CaptureCharacterAppearance(group.Id);
        var background = scene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
        try
        {
            if (isolatedResources != null) RebindCharacterSceneResources(background, isolatedResources);
            identity?.Bind(background);
        }
        catch { background.Free(); throw; }

        // Keep the current background until its replacement was instantiated and bound.
        foreach (var child in container.GetChildren())
        {
            ProviderModeSelectOverlayBridge.StopBackgroundPollers(child, queueFree: true);
            container.RemoveChildSafely(child);
            child.QueueFreeSafely();
        }

        background.Name = character.Id.Entry + "_bg";
        container.AddChildSafely(background);

        if (background.IsInsideTree())
        {
            RefreshCharacterBackgroundLayout(container, background);
            // A provider can have attached to the old background before this refresh replaced it.
            ProviderModeSelectOverlayBridge.AttachRebuiltBackground(screen, character, background);
        }

        // NCharacterSelectScreenBg only subscribes to SizeChanged in _Ready; it does not run
        // that layout calculation for a scene added after the viewport was already sized. The
        // game therefore leaves hot-swapped backgrounds at their default scale. Replay the
        // game's own private layout method after _Ready, without imposing any skin-authored
        // position or scale on the child scene.
        Callable.From(() =>
        {
            if (GodotObject.IsInstanceValid(container) &&
                GodotObject.IsInstanceValid(background))
            {
                RefreshCharacterBackgroundLayout(container, background);
            }
        }).CallDeferred();
    }

    internal static void CaptureCharacterBackgroundHostLayout(Control screen)
    {
        if (!GodotObject.IsInstanceValid(screen) ||
            CharacterBackgroundHostLayouts.TryGetValue(screen, out _))
        {
            return;
        }

        var container = CharacterSelectScreenAccess.Background(screen);
        if (container != null)
        {
            CharacterBackgroundHostLayouts.Add(
                screen,
                CharacterBackgroundHostLayout.Capture(container));
        }
    }

    private static void RestoreCharacterBackgroundHostLayout(Control screen)
    {
        var container = CharacterSelectScreenAccess.Background(screen);
        if (container == null)
        {
            return;
        }

        if (!CharacterBackgroundHostLayouts.TryGetValue(screen, out var layout))
        {
            CaptureCharacterBackgroundHostLayout(screen);
            return;
        }

        if (!layout.Matches(container))
        {
            ModLog.Info(
                $"已清除选角背景容器遗留变换：" +
                $"位置={container.Position}，缩放={container.Scale}，旋转={container.Rotation:F3}。" );
        }

        layout.Apply(container);
        container.Visible = true;
    }

    private static void RebindCharacterSceneResources(
        Node background,
        IReadOnlyDictionary<string, Resource> isolatedResources)
    {
        var rebound = 0;
        foreach (var node in EnumerateNodes(background))
        {
            // SpineSprite is a native extension class, so use its stable Godot method instead of
            // depending on a concrete managed node type. The property currently contains the
            // canonical resource chosen while PackedScene was decoded; replace it before _Ready
            // initializes the skeleton.
            if (!node.HasMethod("set_skeleton_data_res"))
            {
                continue;
            }

            var current = node.Get("skeleton_data_res").AsGodotObject() as Resource;
            if (current == null ||
                string.IsNullOrWhiteSpace(current.ResourcePath) ||
                !isolatedResources.TryGetValue(current.ResourcePath, out var replacement) ||
                ReferenceEquals(current, replacement))
            {
                continue;
            }

            node.Call("set_skeleton_data_res", replacement);
            rebound++;
        }

        if (rebound > 0)
        {
            ModLog.Info($"选角场景已在进树前重新绑定 {rebound} 个隔离骨骼资源。");
        }
    }

    private static void RefreshCharacterBackgroundLayout(Control container, Control background)
    {
        if (CharacterBackgroundWindowChangeMethod == null ||
            container is not NCharacterSelectScreenBg gameBackground ||
            !GodotObject.IsInstanceValid(gameBackground))
        {
            return;
        }

        try
        {
            // The game owns aspect-ratio fitting on the shared AnimatedBg host. A provider may
            // also use NCharacterSelectScreenBg inside its scene, but the original game merely
            // adds that child and never invokes its private window callback. Calling it here a
            // second time scales some custom previews around their own pivot and moves them down
            // and right.
            CharacterBackgroundWindowChangeMethod.Invoke(gameBackground, null);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"刷新选角背景的宽高比布局失败：{exception.GetBaseException().Message}");
        }

        if (GodotObject.IsInstanceValid(background))
        {
            background.QueueRedraw();
        }
    }

    private static IEnumerable<Node> EnumerateNodes(Node root)
    {
        return BoundedTraversal.Walk(root, node => GodotObject.IsInstanceValid(node)
            ? node.GetChildren() : Enumerable.Empty<Node>()).Where(GodotObject.IsInstanceValid);
    }

    private static void RefreshCharacterButtonIcon(
        Control screen,
        CharacterModel character)
    {
        var button = FindCharacterButton(screen, character);
        if (button == null)
        {
            return;
        }

        try
        {
            // Use the same CharacterModel property as the game. The final-result Harmony
            // patch below applies an explicit icon pack, while a code-driven full skin can
            // still provide its own icon when the setting is "follow character skin".
            var fallback = button.IsLocked
                ? character.CharacterSelectLockedIcon
                : character.CharacterSelectIcon;
            // 头像写死绑定"这个角色分组当前选中的皮肤"，与奥斯提/其它分组的选择无关。
            SetCharacterButtonIcon(button,
                SkinService.GetCharacterSelectIcon(
                    character.Id.Entry.ToLowerInvariant(), character.Id.Entry, button.IsLocked)
                ?? fallback);
            ModLog.Info($"已刷新 {character.Id.Entry} 的角色列表头像。");
        }
        catch (AppearanceResourceUnavailableException) { throw; }
        catch (Exception exception)
        {
            ModLog.Warn(
                $"刷新 {character.Id.Entry} 的角色列表头像失败：" +
                exception.GetBaseException().Message);
        }
    }

    private static NCharacterSelectButton? FindCharacterButton(
        Control screen,
        CharacterModel character) =>
        FindDescendant<NCharacterSelectButton>(screen, candidate =>
            candidate.Character?.Id.Entry.Equals(character.Id.Entry, StringComparison.OrdinalIgnoreCase) == true);

    private static T? FindDescendant<T>(Node root, Func<T, bool> predicate) where T : Node
    {
        foreach (var child in root.GetChildren())
        {
            if (child is T match && predicate(match))
            {
                return match;
            }

            var descendant = FindDescendant(child, predicate);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static void RebuildMonsterDisplay(
        NBestiary screen,
        NBestiaryEntry entry,
        MonsterModel monster,
        string groupId)
    {
        var visualsPath = GetMonsterVisualsPath(monster);
        // The canonical shared group (decimillipede/kaiser_crab) owns the real artwork, while
        // the scene path belongs to a per-model sibling group. Load the scene through the group
        // that actually indexes this path so the overlay contains it; both groups are selected
        // together by the shared-animation transaction.
        var sceneGroupId = SkinService.Catalog?.FindGroupIdForResourcePath(visualsPath) ?? groupId;
        var scene = SkinService.LoadRuntimeScene(sceneGroupId, visualsPath);
        PreloadManager.Cache.SetAsset(visualsPath, scene);

        // 置空选中项绕过 SelectMonster 的同项短路；SelectMonster 的 Setup 阶段会重建生物，
        // 走 MonsterModel.CreateVisuals 的接管补丁从而应用新皮肤。
        try
        {
            _refreshingMonsterDisplay = true;
            BestiarySelectedEntryField.SetValue(screen, null);
            BestiarySelectMonsterMethod.Invoke(screen, [entry]);
        }
        finally
        {
            _refreshingMonsterDisplay = false;
        }

        ModLog.Info($"已完整重建 {monster.Id.Entry} 的图鉴展示。");
    }

    internal static string GetMonsterVisualsPath(MonsterModel monster)
    {
        try
        {
            if (MonsterVisualsPathGetter.Invoke(monster, null) is string path &&
                !string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"无法读取 {monster.Id.Entry} 的实际 VisualsPath，将使用默认场景路径：{exception.Message}");
        }

        return CanonicalScenePath("creature_visuals/" + monster.Id.Entry.ToLowerInvariant());
    }

    internal static SkinGroup? FindGroup(string modelId, string? modelTypeName = null)
    {
        var tokens = BuildModelIdentityTokens(modelId, modelTypeName);
        var exact = SkinService.Catalog?.Groups.FirstOrDefault(group =>
            tokens.Contains(NormalizeToken(group.Id)));
        if (exact != null && !SkinCatalog.SharedAnimationPlaceholderGroups.Contains(exact.Id))
        {
            return exact;
        }

        // Placeholder per-model groups (rocket/crusher/decimillipede_segment_*) must yield to
        // the shared-directory group that actually owns the artwork. Non-placeholder exact
        // groups, such as a real damp_cultist directory, keep winning as before.
        var shared = FindSharedAnimationGroup(modelId, modelTypeName);
        return shared ?? exact;
    }

    internal static bool MatchesGroupIdentity(
        string groupId,
        string modelId,
        string? modelTypeName = null)
    {
        var shared = FindSharedAnimationGroup(modelId, modelTypeName);
        if (shared != null && shared.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return BuildModelIdentityTokens(modelId, modelTypeName).Contains(NormalizeToken(groupId)) ||
               GroupMatchesSharedAnimationLayout(groupId, modelId, modelTypeName);
    }

    private static SkinGroup? FindSharedAnimationGroup(string modelId, string? modelTypeName)
    {
        var tokens = BuildModelIdentityTokens(modelId, modelTypeName);
        foreach (var (groupId, variantModels) in SkinCatalog.SharedAnimationModelIds)
        {
            if (!variantModels.Any(id => tokens.Contains(NormalizeToken(id))))
            {
                continue;
            }

            var group = SkinService.Catalog?.Groups.FirstOrDefault(candidate =>
                candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
            if (group != null)
            {
                return group;
            }
        }

        return null;
    }

    // Variant monster models (damp_cultist, decimillipede_elite, kaiser_crab_boss, ...) load
    // their visuals from the base creature's shared animation directory, so a skin that
    // follows the game's real layout can only live in that directory's group.  See
    // SkinCatalog.SharedAnimationModelIds for the directory -> variant-model table.
    private static bool GroupMatchesSharedAnimationLayout(
        string groupId,
        string modelId,
        string? modelTypeName) =>
        SkinCatalog.SharedAnimationModelIds.TryGetValue(groupId, out var variantModels) &&
        (variantModels.Contains(modelId, StringComparer.OrdinalIgnoreCase) ||
         (!string.IsNullOrWhiteSpace(modelTypeName) &&
          variantModels.Any(id => NormalizeToken(id).Equals(
              NormalizeToken(modelTypeName!), StringComparison.OrdinalIgnoreCase))));

    private static HashSet<string> BuildModelIdentityTokens(
        string modelId,
        string? modelTypeName) =>
        new[] { modelId, modelTypeName }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => NormalizeToken(value!))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

    private static string NormalizeToken(string value) => NonAlphanumericRegex().Replace(value, string.Empty).ToLowerInvariant();

    [GeneratedRegex("[^a-zA-Z0-9]")]
    private static partial Regex NonAlphanumericRegex();

    internal static void ReplaceCreatedVisuals(
        string modelId,
        string? modelTypeName,
        string visualsPath,
        ref NCreatureVisuals result, NCreatureVisuals? retainedVisuals)
    {
        // Creation for this node was already owned by the combat scene entry point, which
        // converted the served scene through the scene factory before the game could cast it.
        // Instantiating the same scene again here would fail for a framework scene (plain Node2D)
        // and only duplicate work for a native one.
        if (CombatAppearanceBoundary.SkipAppearance || result.HasMeta(ManagedCombatSceneMeta))
        {
            return;
        }

        var group = FindGroup(modelId, modelTypeName);
        if (group == null || ShouldSkipExternalRuntimeRedirect(group.Id))
        {
            return;
        }

        // The independent Osty mod builds the summon's whole body in NCreature._Ready from the node
        // that is already there. Replacing it here deletes that sprite, which is why the summon fell
        // back to the vanilla body whenever its own skin was selected.
        if (OstyImpostorCompat.OwnsSummonBody(group.Id))
        {
            return;
        }
        // The selected provider builds this monster's replacement body from its own skeleton
        // (per-monster runtime profile). Its visual callback already produced the result, so
        // instantiating the canonical host scene here would put the vanilla monster back.
        if (MonsterRuntimePresentationPolicy.DefersVisualReplacementToProvider(
                SkinService.SelectionOwnsMonsterRuntimeProfile(group.Id)))
        {
            return;
        }

        // Framework CombatVisual scenes are already converted by FrameworkCombatVisualPatch
        // through the same BaseLib NodeFactory contract as their original manager. Replacing the
        // result a second time here would instantiate a permitted plain Node2D as NCreatureVisuals.
        if (SkinService.TryGetSelectedFrameworkContract(group.Id, out _))
        {
            return;
        }

        NCreatureVisuals? replacement = null;
        try
        {
            // Instantiate while the selected player's temporary overlay is mounted. Binary
            // scenes can defer external-resource resolution until Instantiate; loading a
            // PackedScene first and instantiating it after the overlay is restored lets another
            // player's skin occupy the same canonical path in between the two operations.
            // The shared canonical group may not index this exact scene path; use the group that
            // owns the path so the overlay contains it, while runtime-provider gating stays on
            // the canonical family group.
            var sceneGroupId = SkinService.Catalog?.FindGroupIdForResourcePath(visualsPath) ?? group.Id;
            replacement = SkinService.InstantiateRuntimeScene<NCreatureVisuals>(
                sceneGroupId,
                visualsPath,
                () => RuntimeMonsterVisualModeBridge.ApplySelected(group.Id));
            var copied = ManagedSceneCompatibility.CopyMissingUniqueNodes(result, replacement);
            if (copied > 0)
            {
                ModLog.Info($"已为 {modelId} 的替换场景补齐 {copied} 个游戏必需节点。");
            }

            if (AppearanceDiagnostics.Enabled)
            {
                AppearanceDiagnostics.Log(
                    "外观替换",
                    $"模型={modelId}({modelTypeName ?? "?"}) 分组={group.Id} 取资源分组={sceneGroupId} " +
                    $"新场景={replacement.Name} 原场景={(result != null ? result.Name.ToString() : "<null>")}");
            }

            if (!ReferenceEquals(result, retainedVisuals)) CombatAppearanceBoundary.ReleaseNode(result);
            result = replacement;
        }
        catch (Exception exception)
        {
            CombatAppearanceBoundary.ReleaseNode(replacement);
            CombatAppearanceBoundary.SkipAfterFailure(exception);
        }
    }

    /// <summary>
    /// Applies the final visual selected for one concrete creature.  Multiplayer character
    /// selections are owned by Player.NetId, not CharacterModel, so the owner-aware Creature
    /// patch must be the last writer when a per-player selection scope is active.
    /// </summary>
    internal static void ReplaceCreatedCreatureVisuals(
        Creature creature,
        ref NCreatureVisuals? result, NCreatureVisuals retainedVisuals)
    {
        if (result == null || MultiplayerSkinSync.GetScopedSelections() == null)
        {
            return;
        }

        var visuals = result;
        try
        {
            if (creature.Player != null)
            {
                var character = creature.Player.Character;
                ReplaceCreatedVisuals(
                    character.Id.Entry,
                    character.GetType().Name,
                    CanonicalScenePath("creature_visuals/" + character.Id.Entry.ToLowerInvariant()),
                    ref visuals, retainedVisuals);
                ApplySelectedProviderVisualPostfix(
                    character.Id.Entry,
                    character.GetType().Name,
                    character,
                    ref visuals, retainedVisuals);
            }
            else if (creature.Monster != null)
            {
                var monster = creature.Monster;
                ReplaceCreatedVisuals(
                    monster.Id.Entry,
                    monster.GetType().Name,
                    GetMonsterVisualsPath(monster),
                    ref visuals, retainedVisuals);
                ApplySelectedProviderVisualPostfix(
                    monster.Id.Entry,
                    monster.GetType().Name,
                    monster,
                    ref visuals, retainedVisuals);
                if (!CombatAppearanceBoundary.SkipAppearance)
                    MarkAndApplyMonsterScale(monster.Id.Entry, monster.GetType().Name, visuals);
            }
        }
        finally { result = visuals; }
    }

    internal static void ApplySelectedCreatureVisualPostfix(
        Creature creature,
        ref NCreatureVisuals? visuals)
    {
        if (visuals == null) return;
        var result = visuals;
        ApplySelectedProviderVisualPostfix(
            creature.ModelId.Entry,
            creature.Player?.Character.GetType().Name ?? creature.Monster?.GetType().Name,
            creature,
            ref result);
        visuals = result;
    }

    internal static void ApplySelectedProviderVisualPostfix(
        string modelId,
        string? modelTypeName,
        object model,
        ref NCreatureVisuals visuals,
        NCreatureVisuals? retainedVisuals = null)
    {
        if (CombatAppearanceBoundary.SkipAppearance) return;
        var group = FindGroup(modelId, modelTypeName);
        if (group != null)
        {
            // 变体选项：提供者后置照常运行（它负责填充模型/动画），最后再把场景写回变体，
            // 否则会出现 EmptyNCreatureVisuals（预览小人空白）。
            var variantSuffix = SelectedVariantSuffix(group.Id);

            // A provider's replayed CreateVisuals postfix may instantiate an auxiliary scene or
            // resolve a private attachment after the main visual has been created.  For a remote
            // player that work must see the same canonical overlay as the main scene; otherwise
            // only the root model is isolated and the provider can still bind the local player's
            // skin.  Local creations keep the existing fast path.
            IDisposable? resourceScope = null;
            if (MultiplayerSkinSync.GetScopedSelection(group.Id) != null)
            {
                var monster = model as MonsterModel ?? (model as Creature)?.Monster;
                var scenePath = monster != null
                    ? GetMonsterVisualsPath(monster)
                    : CanonicalScenePath("creature_visuals/" + modelId.ToLowerInvariant());
                try
                {
                    resourceScope = SkinService.BeginRuntimeResourceScope(group.Id, scenePath);
                }
                catch (Exception exception)
                {
                    CombatAppearanceBoundary.SkipAfterFailure(exception);
                    return;
                }
            }

            try
            {
                SkinService.ApplySelectedVisualPostfix(group.Id, model, ref visuals);
            }
            finally
            {
                resourceScope?.Dispose();
            }

            // 变体选项最后写入：提供者已经填充过模型，这一步只把场景换回选项自带的变体场景。
            if (variantSuffix != null &&
                !string.Equals(variantSuffix, "default", StringComparison.OrdinalIgnoreCase) &&
                model is CharacterModel character)
            {
                var variantPath = ResolveVariantCombatPath(character);
                if (variantPath != null)
                {
                    ReplaceCreatedVisuals(
                        group.Id,
                        modelTypeName ?? character.GetType().Name,
                        variantPath,
                        ref visuals,
                        retainedVisuals);
                    ModLog.Info($"[十字军变体] 已把场景写回变体：{variantPath}（分组={group.Id}）。");
                }
            }
        }
    }

    internal static void MarkAndApplyMonsterScale(
        string modelId,
        string modelTypeName,
        NCreatureVisuals visuals)
    {
        var group = FindGroup(modelId, modelTypeName);
        if (group == null)
        {
            return;
        }

        visuals.SetMeta(MonsterScaleGroupMeta, group.Id);
        CaptureMonsterBaseScale(visuals);
        ApplyStoredMonsterScale(visuals, group.Id);
    }

    internal static void ReapplyMonsterScaleAfterGameScale(NCreatureVisuals visuals)
    {
        if (!visuals.HasMeta(MonsterScaleGroupMeta))
        {
            return;
        }

        var groupId = visuals.GetMeta(MonsterScaleGroupMeta).AsString();
        CaptureMonsterBaseScale(visuals);
        ApplyStoredMonsterScale(visuals, groupId);
    }

    internal static float GetAppliedMonsterScaleFactor(NCreatureVisuals visuals)
    {
        if (!visuals.HasMeta(MonsterScaleGroupMeta))
        {
            return 1f;
        }

        return visuals.GetMeta(MonsterAppliedScaleMeta, 1f).AsSingle();
    }

    internal static bool IsMonsterScaleManaged(NCreatureVisuals visuals) =>
        visuals.HasMeta(MonsterScaleGroupMeta);

    internal static void ApplyMonsterScalePreview(NCreatureVisuals visuals, float factor)
    {
        if (!visuals.HasMeta(MonsterScaleGroupMeta))
        {
            return;
        }

        var normalized = Mathf.Clamp(
            Mathf.Round(factor / SkinService.MonsterScaleStep) * SkinService.MonsterScaleStep,
            SkinService.MinimumMonsterScale,
            SkinService.MaximumMonsterScale);
        ApplyMonsterScaleFactor(visuals, normalized);
    }

    private static void CaptureMonsterBaseScale(NCreatureVisuals visuals)
    {
        visuals.SetMeta(MonsterBaseScaleMeta, visuals.Scale);
        visuals.SetMeta(MonsterBaseDefaultScaleMeta, visuals.DefaultScale);
    }

    private static void ApplyStoredMonsterScale(NCreatureVisuals visuals, string groupId)
    {
        ApplyMonsterScaleFactor(visuals, SkinService.GetSelectedMonsterScale(groupId));
    }

    private static void ApplyMonsterScaleFactor(NCreatureVisuals visuals, float factor)
    {
        var baseScale = visuals.GetMeta(MonsterBaseScaleMeta, visuals.Scale).AsVector2();
        var baseDefaultScale = visuals
            .GetMeta(MonsterBaseDefaultScaleMeta, visuals.DefaultScale)
            .AsSingle();
        visuals.Scale = baseScale * factor;
        visuals.DefaultScale = baseDefaultScale * factor;
        visuals.SetMeta(MonsterAppliedScaleMeta, factor);
    }

    private static void ApplyMonsterScaleToDescendants(Node root, string groupId)
    {
        if (root is NCreatureVisuals visuals &&
            visuals.GetMeta(MonsterScaleGroupMeta, string.Empty).AsString()
                .Equals(groupId, StringComparison.OrdinalIgnoreCase))
        {
            ApplyStoredMonsterScale(visuals, groupId);
        }

        foreach (var child in root.GetChildren())
        {
            ApplyMonsterScaleToDescendants(child, groupId);
        }
    }

    private static void ApplySharedAnimationScaleToDescendants(Node root, string groupId)
    {
        ApplyMonsterScaleToDescendants(root, groupId);
        if (!SkinCatalog.SharedAnimationSiblingGroups.TryGetValue(groupId, out var siblingGroupIds))
        {
            return;
        }

        foreach (var siblingGroupId in siblingGroupIds)
        {
            if (!siblingGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
            {
                ApplyMonsterScaleToDescendants(root, siblingGroupId);
            }
        }
    }

    internal static void ReplaceCachedScene(string resourcePath, ref PackedScene result)
    {
        Core.DebugSnapshot.EnsureInstalled();
        var groupId = SkinService.Catalog?.FindGroupIdForResourcePath(resourcePath);
        // Character-select scenes are instantiated by the game (and by a few compatibility
        // libraries) immediately after AssetCache.GetScene returns. Returning a temporary alias
        // here makes their PackedScene resolve external skeleton/layout resources after the alias
        // pack has already been restored. The deferred character-screen rebuild owns this path,
        // so leave the cache result alone and let it replace the child under the correct mount.
        if (groupId == null ||
            IsCreatureVisualScenePath(resourcePath) ||
            IsCharacterSelectBackgroundPath(resourcePath) ||
            ShouldSkipExternalRuntimeRedirect(groupId))
        {
            return;
        }

        try
        {
            result = SkinService.GetOrLoadRuntimeScene(groupId, resourcePath);
        }
        catch (Exception exception)
        {
            ModLog.Error($"最终接管场景 {resourcePath} 失败：{exception}");
        }
    }

    private static bool IsCharacterSelectBackgroundPath(string resourcePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(resourcePath);
        return fileName.StartsWith("char_select_bg_", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsCreatureVisualScenePath(string resourcePath) =>
        resourcePath.Contains("/creature_visuals/", StringComparison.OrdinalIgnoreCase);

    internal static void ReplaceCachedTexture(string resourcePath, ref Texture2D result)
    {
        if (CombatAppearanceBoundary.SkipAppearance) return;
        try
        {
            var groupId = SkinService.Catalog?.FindGroupIdForResourcePath(resourcePath);
            if (groupId == null || ShouldSkipExternalRuntimeRedirect(groupId)) return;

            var replacement = SkinService.GetOrLoadRuntimeResource(groupId, resourcePath) as Texture2D ??
                              throw new InvalidOperationException($"独立皮肤资源不是贴图：{resourcePath}");
            if (EventSkinPolicy.IsEventGroup(groupId))
                replacement.SetMeta(EventSkinRuntime.TextureSourceMeta, resourcePath);
            result = replacement;
        }
        catch (Exception exception)
        {
            CombatAppearanceBoundary.Warn("贴图", exception);
        }
    }

    internal static void ReplaceCharacterSelectTexture(
        CharacterModel character,
        bool locked,
        ref CompressedTexture2D result)
    {
        var group = FindGroup(character.Id.Entry);
        if (group == null)
        {
            return;
        }

        var characterId = character.Id.Entry.ToLowerInvariant();
        if (!locked && VariantPortraits.Resolve(group.Id, "select", 1) is { } variantSelect)
        {
            if (variantSelect as CompressedTexture2D is { } variantTexture)
            {
                result = variantTexture;
                return;
            }
        }

        var resourcePath = CanonicalImagePath(
            "packed/character_select/char_select_" + characterId + (locked ? "_locked.png" : ".png"));
        if (!SkinService.SelectedSkinProvidesResource(group.Id, resourcePath) ||
            SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(
                group.Id,
                resourcePath))
        {
            return;
        }

        try
        {
            result = SkinService.GetOrLoadCharacterIconResource(group.Id, resourcePath) as CompressedTexture2D ??
                     throw new InvalidOperationException($"角色头像资源不是压缩贴图：{resourcePath}");
        }
        catch (Exception exception)
        {
            ModLog.Error($"最终接管角色列表贴图 {resourcePath} 失败：{exception}");
        }
    }

    internal static void ReplaceCharacterIcon(CharacterModel character, ref Control result)
    {
        if (CombatAppearanceBoundary.SkipAppearance) return;
        Control? replacement = null;
        try
        {
            var group = FindGroup(character.Id.Entry);
            if (group == null)
            {
                return;
            }

            // The in-run top-left avatar is this Control, and it is handed back to us as the *existing*
            // instance whenever the HUD refreshes it. Repainting that instance is what makes a variant
            // change visible without rebuilding the whole top bar.
            var variantAvatar = VariantPortraits.Resolve(group.Id, "run", 1);

            var characterId = character.Id.Entry.ToLowerInvariant();
            var resourcePath = CanonicalScenePath("ui/character_icons/" + characterId + "_icon");
            if (!SkinService.SelectedSkinProvidesResource(group.Id, resourcePath) ||
                SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(
                    group.Id,
                    resourcePath))
            {
                if (variantAvatar != null && result != null && GodotObject.IsInstanceValid(result))
                    ApplyVariantTexture(result, variantAvatar);
                return;
            }

            replacement = SkinService.WithCharacterIconResource(
                group.Id,
                resourcePath,
                resource => (resource as PackedScene ??
                             throw new InvalidOperationException(
                                 $"角色头像资源不是场景：{resourcePath}"))
                    .Instantiate<Control>(PackedScene.GenEditState.Disabled),
                includeProviderDependencies: true);
            ManagedSceneCompatibility.CopyMissingUniqueNodes(result, replacement);
            if (variantAvatar != null)
            {
                // The avatar scene carries its own TextureRect; swap in the variant portrait so the
                // top-left run avatar follows the appearance change like the pack's own code did.
                foreach (var rect in EnumerateTextureRects(replacement))
                {
                    rect.Texture = variantAvatar;
                }
            }

            CombatAppearanceBoundary.ReleaseNode(result);
            result = replacement;
        }
        catch (Exception exception)
        {
            CombatAppearanceBoundary.ReleaseNode(replacement);
            CombatAppearanceBoundary.Warn("头像", exception);
        }
    }

    internal static void ReplaceCharacterTexture(
        CharacterModel character,
        string resourcePath,
        ref Texture2D result)
    {
        if (CombatAppearanceBoundary.SkipAppearance) return;
        try
        {
            var group = FindGroup(character.Id.Entry);

            // A settings-driven variant ships its own per-variant portrait; the pack's own code would have
            // swapped it, and that code does not run for a package that lost the assembly identity.
            if (group != null && !resourcePath.Contains("map_marker", StringComparison.OrdinalIgnoreCase))
            {
                var kind = resourcePath.Contains("outline", StringComparison.OrdinalIgnoreCase)
                    ? "run_outline"
                    : "run";
                if (VariantPortraits.Resolve(group.Id, kind, 1) is { } variantPortrait)
                {
                    result = variantPortrait;
                    return;
                }
            }

            if (group == null ||
                !SkinService.SelectedSkinProvidesResource(group.Id, resourcePath) ||
                SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(
                    group.Id,
                    resourcePath))
            {
                return;
            }

            result = SkinService.GetOrLoadCharacterIconResource(group.Id, resourcePath) as Texture2D ??
                     throw new InvalidOperationException($"角色头像资源不是贴图：{resourcePath}");
        }
        catch (Exception exception)
        {
            CombatAppearanceBoundary.Warn("头像", exception);
        }
    }

    /// <summary>Paints a variant portrait onto an existing avatar control (and any TextureRect it holds).</summary>
    private static void ApplyVariantTexture(Node root, Texture2D texture)
    {
        try
        {
            foreach (var rect in EnumerateTextureRects(root))
            {
                if (GodotObject.IsInstanceValid(rect))
                {
                    rect.Texture = texture;
                }
            }

            if (root is TextureRect self)
            {
                self.Texture = texture;
            }
        }
        catch
        {
            // Never break the HUD because a variant portrait could not be applied.
        }
    }

    private static IEnumerable<TextureRect> EnumerateTextureRects(Node root)
    {
        if (root is TextureRect rect)
        {
            yield return rect;
        }

        foreach (var child in root.GetChildren())
        {
            foreach (var nested in EnumerateTextureRects(child))
            {
                yield return nested;
            }
        }
    }

    private static bool ShouldSkipExternalRuntimeRedirect(string groupId) =>
        MultiplayerSkinSync.GetScopedSelection(groupId) == null &&
        SkinService.IsExternalRuntimeProviderSelected(groupId);

    internal static bool RefreshMultiplayerPlayerIcons(ulong playerNetId)
    {
        try
        {
            // The lobby and the in-run HUD are not always children of the same NGame node
            // during the hand-off into a run.  Resolve the actual SceneTree root first and only
            // use NGame as a fallback; otherwise a selection packet that arrives during that
            // transition silently misses both avatar nodes.
            var root = (Engine.GetMainLoop() as SceneTree)?.Root ??
                       NGame.Instance?.GetTree().Root;
            if (root == null)
            {
                return false;
            }

            var stateCount = 0;
            var lobbyCount = 0;
            var voteCount = 0;
            var pending = new Stack<Node>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var node = pending.Pop();
                if (node is NMultiplayerPlayerState state &&
                    state.Player?.NetId == playerNetId)
                {
                    if (RefreshMultiplayerPlayerStateIcon(state))
                    {
                        stateCount++;
                    }
                }
                else if (node is NRemoteLobbyPlayer lobbyPlayer &&
                         lobbyPlayer.PlayerId == playerNetId)
                {
                    if (RefreshRemoteLobbyPlayerIcon(lobbyPlayer))
                    {
                        lobbyCount++;
                    }
                }
                else if (node is NMultiplayerVoteContainer voteContainer)
                {
                    voteCount += RefreshMultiplayerVoteIcons(voteContainer, playerNetId);
                }

                foreach (var child in node.GetChildren())
                {
                    pending.Push(child);
                }
            }

            if (stateCount == 0 && lobbyCount == 0 && voteCount == 0)
            {
                // This is expected while the run scene is being rebuilt.  The _Ready patches
                // below apply the same scope when the nodes are created, so no global polling is
                // needed here.
                return false;
            }

            ModLog.Info(
                $"已刷新联机玩家 {playerNetId} 的头像：战斗栏 {stateCount}、" +
                $"选角栏 {lobbyCount}、投票栏 {voteCount}。 ");
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn($"刷新联机玩家 {playerNetId} 的头像失败：{exception.GetBaseException().Message}");
            return false;
        }
    }

    private static bool RefreshMultiplayerPlayerStateIcon(NMultiplayerPlayerState state)
    {
        // Prefer the game's already-resolved private field.  During a scene hand-off the
        // unique-name lookup can briefly resolve against the old owner (or return null) even
        // though NMultiplayerPlayerState has a live _characterIcon reference.
        var icon = AccessTools.Field(typeof(NMultiplayerPlayerState), "_characterIcon")
                       ?.GetValue(state) as TextureRect ??
                   state.GetNodeOrNull<TextureRect>("%CharacterIcon");
        // NMultiplayerPlayerState already owns the authoritative Player instance.  Looking it up
        // through NRun reflection here races run creation and was the reason remote HUD avatars
        // stayed on the base texture even though the selection packet had been received.
        var player = state.Player;
        var character = player?.Character;
        if (icon == null || !GodotObject.IsInstanceValid(icon) || player == null || character == null)
        {
            return false;
        }

        using var scope = MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId);
        icon.Texture = TryLoadManagedCharacterIconTexture(character) ?? character.IconTexture;
        return true;
    }

    private static Texture2D? TryLoadManagedCharacterIconTexture(CharacterModel character)
    {
        var group = FindGroup(character.Id.Entry, character.GetType().Name) ??
                    FindGroup(character.Id.Entry);
        var path = CanonicalImagePath(
            "ui/top_panel/character_icon_" + character.Id.Entry.ToLowerInvariant() + ".png");
        if (group == null ||
            !SkinService.SelectedSkinProvidesResource(group.Id, path) ||
            SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(group.Id, path))
        {
            return null;
        }

        try
        {
            var resource = SkinService.GetOrLoadCharacterIconResource(group.Id, path);
            if (resource is Texture2D texture)
            {
                return texture;
            }

            LogMissingMultiplayerIcon(group.Id, path, $"资源类型={resource.GetType().Name}");
            return null;
        }
        catch (Exception exception)
        {
            // Some skins intentionally omit a separate top-panel icon.  In that case the
            // game's current IconTexture (and any provider-specific presentation) is the
            // correct fallback; do not turn a missing optional icon into a lobby error.
            LogMissingMultiplayerIcon(group.Id, path, exception.GetBaseException().Message);
            return null;
        }
    }

    private static void LogMissingMultiplayerIcon(string groupId, string path, string detail)
    {
        var selection = MultiplayerSkinSync.GetScopedSelection(groupId);
        if (selection == null || !selection.StartsWith("__online_", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var key = groupId + "\n" + selection + "\n" + path;
        if (LoggedMissingMultiplayerIcons.Add(key))
        {
            ModLog.Warn(
                $"联机皮肤头像未能从安全资源包加载：分组={groupId}，选项={selection}，" +
                $"路径={path}，原因={detail}。 ");
        }
    }

    private static bool RefreshRemoteLobbyPlayerIcon(NRemoteLobbyPlayer node)
    {
        var character = AccessTools.Field(typeof(NRemoteLobbyPlayer), "_character")
            ?.GetValue(node) as CharacterModel;
        if (character == null)
        {
            return false;
        }

        using var scope = MultiplayerSkinSync.BeginPlayerSelectionScope(node.PlayerId);
        var icon = AccessTools.Field(typeof(NRemoteLobbyPlayer), "_characterIcon")
                       ?.GetValue(node) as TextureRect ??
                   node.GetNodeOrNull<TextureRect>("%CharacterIcon");
        if (icon == null || !GodotObject.IsInstanceValid(icon))
        {
            return false;
        }

        // Keep an explicit assignment instead of invoking RefreshVisuals.  RefreshVisuals is
        // also called by the game's lobby callbacks; invoking it here caused a synchronous
        // re-entry loop while the room was being created.
        icon.Texture = TryLoadManagedCharacterIconTexture(character) ?? character.IconTexture;
        return true;
    }

    internal static void RefreshMultiplayerVoteIcons(NMultiplayerVoteContainer container) =>
        RefreshMultiplayerVoteIcons(container, playerNetId: null);

    private static int RefreshMultiplayerVoteIcons(
        NMultiplayerVoteContainer container,
        ulong? playerNetId)
    {
        var refreshed = 0;
        try
        {
            var votes = AccessTools.Field(typeof(NMultiplayerVoteContainer), "_votes")
                ?.GetValue(container) as System.Collections.IEnumerable;
            if (votes == null)
            {
                return 0;
            }

            foreach (var vote in votes)
            {
                if (vote == null)
                {
                    continue;
                }

                var voteType = vote.GetType();
                var player = AccessTools.Field(voteType, "player")?.GetValue(vote) as Player;
                var node = AccessTools.Field(voteType, "node")?.GetValue(vote) as TextureRect;
                if (player == null || node == null || !GodotObject.IsInstanceValid(node))
                {
                    continue;
                }
                if (playerNetId.HasValue && player.NetId != playerNetId.Value)
                {
                    continue;
                }

                using var scope = MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId);
                node.Texture = TryLoadManagedCharacterIconTexture(player.Character) ??
                               player.Character.IconTexture;
                var outline = node.GetNodeOrNull<TextureRect>("Outline");
                if (outline != null && GodotObject.IsInstanceValid(outline))
                {
                    outline.Texture = player.Character.IconOutlineTexture;
                }
                refreshed++;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("刷新联机投票头像失败：" + exception.GetBaseException().Message);
        }

        return refreshed;
    }

    internal static void ReplaceCharacterMapMarker(
        CharacterModel character,
        ref CompressedTexture2D result)
    {
        var characterId = character.Id.Entry.ToLowerInvariant();
        var resourcePath = CanonicalImagePath("packed/map/icons/map_marker_" + characterId + ".png");
        Texture2D texture = result;
        ReplaceCharacterTexture(character, resourcePath, ref texture);
        if (texture is CompressedTexture2D compressedTexture)
        {
            result = compressedTexture;
        }
    }

    private sealed record CharacterBackgroundHostLayout(
        float AnchorLeft,
        float AnchorTop,
        float AnchorRight,
        float AnchorBottom,
        float OffsetLeft,
        float OffsetTop,
        float OffsetRight,
        float OffsetBottom,
        Control.GrowDirection GrowHorizontal,
        Control.GrowDirection GrowVertical,
        Vector2 PivotOffset,
        Vector2 Scale,
        float Rotation)
    {
        internal static CharacterBackgroundHostLayout Capture(Control control) => new(
            control.AnchorLeft,
            control.AnchorTop,
            control.AnchorRight,
            control.AnchorBottom,
            control.OffsetLeft,
            control.OffsetTop,
            control.OffsetRight,
            control.OffsetBottom,
            control.GrowHorizontal,
            control.GrowVertical,
            control.PivotOffset,
            control.Scale,
            control.Rotation);

        internal bool Matches(Control control) =>
            Mathf.IsEqualApprox(control.AnchorLeft, AnchorLeft) &&
            Mathf.IsEqualApprox(control.AnchorTop, AnchorTop) &&
            Mathf.IsEqualApprox(control.AnchorRight, AnchorRight) &&
            Mathf.IsEqualApprox(control.AnchorBottom, AnchorBottom) &&
            Mathf.IsEqualApprox(control.OffsetLeft, OffsetLeft) &&
            Mathf.IsEqualApprox(control.OffsetTop, OffsetTop) &&
            Mathf.IsEqualApprox(control.OffsetRight, OffsetRight) &&
            Mathf.IsEqualApprox(control.OffsetBottom, OffsetBottom) &&
            control.GrowHorizontal == GrowHorizontal &&
            control.GrowVertical == GrowVertical &&
            control.PivotOffset.IsEqualApprox(PivotOffset) &&
            control.Scale.IsEqualApprox(Scale) &&
            Mathf.IsEqualApprox(control.Rotation, Rotation);

        internal void Apply(Control control)
        {
            control.AnchorLeft = AnchorLeft;
            control.AnchorTop = AnchorTop;
            control.AnchorRight = AnchorRight;
            control.AnchorBottom = AnchorBottom;
            control.OffsetLeft = OffsetLeft;
            control.OffsetTop = OffsetTop;
            control.OffsetRight = OffsetRight;
            control.OffsetBottom = OffsetBottom;
            control.GrowHorizontal = GrowHorizontal;
            control.GrowVertical = GrowVertical;
            control.PivotOffset = PivotOffset;
            control.Scale = Scale;
            control.Rotation = Rotation;
        }
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.OnSubmenuOpened))]
internal static class RandomCharacterLobbySkinPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix()
    {
        try { SkinService.RestoreRandomCharacterSkinsForLobby(); }
        catch (Exception error)
        {
            ModLog.Error("进入选角时恢复随机皮肤的原皮失败，不阻止打开界面：" + error);
        }
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.SelectCharacter))]
internal static class CharacterSelectionSkinPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(Control __instance) =>
        ContextualSkinControls.CaptureCharacterBackgroundHostLayout(__instance);

    [HarmonyPriority(Priority.First)]
    private static void Postfix(Control __instance, NCharacterSelectButton charSelectButton) =>
        ContextualSkinControls.ShowCharacter(__instance, charSelectButton.Character);
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "StartNewSingleplayerRun")]
internal static class SingleplayerEmbarkSkinSelectorPatch
{
    private static void Prefix(Control __instance)
    {
        try
        {
            var groupId = ContextualSkinControls.GetLocalLobbyCharacter(CharacterSelectScreenAccess.Lobby(__instance)).Id.Entry.ToLowerInvariant();
            if (SkinService.IsRandomCharacterSkinEnabled(groupId))
            {
                ContextualSkinControls.HideCharacterSelector(__instance);
                return; // Resolve after the fade, once RunManager has the final local character.
            }
            if (!SkinService.ApplySelectedCharacterSkinBundleForRun(groupId, out var warnings))
            {
                ModLog.Error($"开始对局前应用皮肤包失败：{SkinService.LastError}");
            }
            foreach (var warning in warnings)
            {
                ModLog.Warn("皮肤包：" + warning);
            }
        }
        catch (Exception error)
        {
            ModLog.Error("开始单人对局前应用皮肤包失败，将继续使用当前外观：" + error);
        }
        ContextualSkinControls.HideCharacterSelector(__instance);
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "StartNewMultiplayerRun")]
internal static class MultiplayerEmbarkSkinSelectorPatch
{
    private static void Prefix(Control __instance)
    {
        try
        {
            var groupId = ContextualSkinControls.GetLocalLobbyCharacter(CharacterSelectScreenAccess.Lobby(__instance)).Id.Entry.ToLowerInvariant();
            if (SkinService.IsRandomCharacterSkinEnabled(groupId))
            {
                ContextualSkinControls.HideCharacterSelector(__instance);
                return;
            }
            if (!SkinService.ApplySelectedCharacterSkinBundleForRun(groupId, out var warnings))
            {
                ModLog.Error($"开始多人对局前应用皮肤包失败：{SkinService.LastError}");
            }
            foreach (var warning in warnings)
            {
                ModLog.Warn("皮肤包：" + warning);
            }
        }
        catch (Exception error)
        {
            ModLog.Error("开始多人对局前应用皮肤包失败，将继续使用当前外观：" + error);
        }
        ContextualSkinControls.HideCharacterSelector(__instance);
    }
}

[HarmonyPatch(typeof(RunManager), nameof(RunManager.CleanUp))]
internal static class CharacterSkinBundleRunCleanupPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix()
    {
        try
        {
            SkinService.RestoreCharacterSkinBundleAfterRun();
        }
        catch (Exception exception)
        {
            // A cosmetic restore must never prevent the game's own run cleanup. The sidecar is
            // deliberately retained so startup recovery can finish the restore on the next boot.
            ModLog.Error("退出对局时恢复皮肤包预设失败，将在下次启动重试：" + exception);
        }
    }
}

[HarmonyPatch(typeof(NBestiary), "SelectMonster")]
internal static class BestiarySelectionSkinPatch
{
    private static void Postfix(NBestiary __instance, NBestiaryEntry entry)
    {
        // 皮肤切换触发的重建会再次进入 SelectMonster，跳过重入以避免重复刷新下拉框。
        if (!ContextualSkinControls.IsRefreshingMonsterDisplay)
        {
            ContextualSkinControls.ShowMonster(__instance, entry);
        }
    }
}

[HarmonyPatch(typeof(NBestiary), nameof(NBestiary.OnSubmenuClosed))]
internal static class BestiaryRuntimeProviderScopePatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        if (NRun.Instance != null)
        {
            CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(
                reason: "关闭局内怪物图鉴", refreshCurrentRoom: true);
            return;
        }

        SkinService.FocusRuntimeProviderBehaviorsOnGroups(
            [],
            runEnvironmentProviderIds: [],
            reason: "关闭怪物图鉴");
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.CreateVisuals))]
internal static class CharacterVisualResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref NCreatureVisuals __result)
    {
        if (__result == null || CombatAppearanceBoundary.SkipAppearance) return;
        using var boundary = CombatAppearanceBoundary.BeginIfNeeded();
        var original = __result;
        var candidate = original;
        try
        {
            var group = ContextualSkinControls.FindGroup(
                __instance.Id.Entry,
                __instance.GetType().Name);
            if (group != null && MultiplayerSkinSync.GetScopedSelection(group.Id) != null)
            {
                // The owner-aware Creature.CreateVisuals postfix is the sole final writer for a
                // remote player.  Applying here would reduce the selection back to one value shared
                // by every instance of this CharacterModel.
                return;
            }

            var variantPath = ContextualSkinControls.ResolveVariantCombatPath(__instance);
            if (variantPath != null)
            {
                var packed = GD.Load<PackedScene>(variantPath);
                if (packed != null)
                {
                    candidate = packed.Instantiate<NCreatureVisuals>();
                    ModLog.Info($"已应用十字军变体场景：{variantPath}");
                    return;
                }
            }

            var canonicalPath = ContextualSkinControls.CanonicalScenePath(
                "creature_visuals/" + __instance.Id.Entry.ToLowerInvariant());
            // A provider can resolve its own sub-variant by patching CharacterModel.VisualsPath.
            // The original CreateVisuals has already built that variant; replacing it here would
            // force the provider's default scene. Keep a non-canonical provider path; SkinChanger's
            // explicit synthetic option still wins through ResolveVariantCombatPath above.
            if (ContextualSkinControls.HasCustomVisualsPath(__instance, canonicalPath))
            {
                return;
            }

            ContextualSkinControls.ReplaceCreatedVisuals(
                __instance.Id.Entry,
                null,
                canonicalPath,
                ref candidate, original);
            ContextualSkinControls.ApplySelectedProviderVisualPostfix(
                __instance.Id.Entry,
                __instance.GetType().Name,
                __instance,
                ref candidate, original);
        }
        catch (Exception error)
        {
            CombatAppearanceBoundary.SkipAfterFailure(error);
        }
        finally
        {
            __result = CombatAppearanceBoundary.FinishVisual(original, candidate);
        }
    }
}

[HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.CreateVisuals))]
internal static class MonsterVisualResultPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(MonsterModel __instance)
    {
        if (CombatAppearanceBoundary.SkipAppearance) return;
        try
        {
            var group = ContextualSkinControls.FindGroup(
                __instance.Id.Entry,
                __instance.GetType().Name);
            if (group != null)
            {
                CharacterAppearanceRuntime.AddVisibleCombatRuntimeGroup(group.Id);
            }
        }
        catch (Exception error)
        {
            CombatAppearanceBoundary.SkipAfterFailure(error);
        }
    }

    [HarmonyPriority(Priority.Last)]
    private static void Postfix(MonsterModel __instance, ref NCreatureVisuals __result)
    {
        if (__result == null || CombatAppearanceBoundary.SkipAppearance) return;
        using var boundary = CombatAppearanceBoundary.BeginIfNeeded();
        var original = __result;
        var candidate = original;
        try
        {
            var group = ContextualSkinControls.FindGroup(
                __instance.Id.Entry,
                __instance.GetType().Name);
            if (group != null && MultiplayerSkinSync.GetScopedSelection(group.Id) != null)
            {
                // Pets can also be owned by a remote player.  Their companion group is resolved from
                // the owning player's complete selection transaction by the Creature-level patch.
                return;
            }

            ContextualSkinControls.ReplaceCreatedVisuals(
                __instance.Id.Entry,
                __instance.GetType().Name,
                ContextualSkinControls.GetMonsterVisualsPath(__instance),
                ref candidate, original);
            ContextualSkinControls.ApplySelectedProviderVisualPostfix(
                __instance.Id.Entry,
                __instance.GetType().Name,
                __instance,
                ref candidate, original);
            if (!CombatAppearanceBoundary.SkipAppearance)
                ContextualSkinControls.MarkAndApplyMonsterScale(
                    __instance.Id.Entry,
                    __instance.GetType().Name,
                    candidate);
        }
        catch (Exception error)
        {
            CombatAppearanceBoundary.SkipAfterFailure(error);
        }
        finally
        {
            __result = CombatAppearanceBoundary.FinishVisual(original, candidate);
        }
    }
}

[HarmonyPatch(typeof(NCreatureVisuals), nameof(NCreatureVisuals.SetScaleAndHue))]
internal static class MonsterVisualScalePatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NCreatureVisuals __instance) =>
        ContextualSkinControls.ReapplyMonsterScaleAfterGameScale(__instance);
}

[HarmonyPatch(typeof(AssetCache), nameof(AssetCache.GetScene))]
internal static class CachedSceneResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(string path, ref PackedScene __result) =>
        ContextualSkinControls.ReplaceCachedScene(path, ref __result);
}

[HarmonyPatch(typeof(AssetCache), nameof(AssetCache.GetTexture2D))]
internal static class CachedTextureResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(string path, ref Texture2D __result) =>
        ContextualSkinControls.ReplaceCachedTexture(path, ref __result);
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.CharacterSelectIcon), MethodType.Getter)]
internal static class CharacterSelectIconResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result) =>
        ContextualSkinControls.ReplaceCharacterSelectTexture(__instance, locked: false, ref __result);
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.CharacterSelectLockedIcon), MethodType.Getter)]
internal static class CharacterSelectLockedIconResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result) =>
        ContextualSkinControls.ReplaceCharacterSelectTexture(__instance, locked: true, ref __result);
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.Icon), MethodType.Getter)]
internal static class CharacterIconResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref Control __result) =>
        ContextualSkinControls.ReplaceCharacterIcon(__instance, ref __result);
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.IconTexture), MethodType.Getter)]
internal static class CharacterIconTextureResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref Texture2D __result)
    {
        var characterId = __instance.Id.Entry.ToLowerInvariant();
        var path = ContextualSkinControls.CanonicalImagePath("ui/top_panel/character_icon_" + characterId + ".png");
        ContextualSkinControls.ReplaceCharacterTexture(__instance, path, ref __result);
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.IconOutlineTexture), MethodType.Getter)]
internal static class CharacterIconOutlineTextureResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref Texture2D __result)
    {
        var characterId = __instance.Id.Entry.ToLowerInvariant();
        var path = ContextualSkinControls.CanonicalImagePath("ui/top_panel/character_icon_" + characterId + "_outline.png");
        ContextualSkinControls.ReplaceCharacterTexture(__instance, path, ref __result);
    }
}

[HarmonyPatch(typeof(CharacterModel), nameof(CharacterModel.MapMarker), MethodType.Getter)]
internal static class CharacterMapMarkerResultPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(CharacterModel __instance, ref CompressedTexture2D __result) =>
        ContextualSkinControls.ReplaceCharacterMapMarker(__instance, ref __result);
}

[HarmonyPatch(typeof(NMultiplayerPlayerState), nameof(NMultiplayerPlayerState._Ready))]
internal static class MultiplayerPlayerStateIconScopePatch
{
    private static void Prefix(NMultiplayerPlayerState __instance, out IDisposable? __state) =>
        __state = __instance.Player == null
            ? null
            : MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.Player.NetId);

    private static void Postfix(
        NMultiplayerPlayerState __instance,
        IDisposable? __state)
    {
        __state?.Dispose();
        if (__instance.Player != null)
        {
            // Queue one pass after the game's own icon assignment.  The queue is drained from
            // the normal process tick, avoiding scene-tree re-entry during _Ready.
            MultiplayerSkinSync.RequestIconRefresh(__instance.Player.NetId);
        }
    }
}

[HarmonyPatch(typeof(NRemoteLobbyPlayer), nameof(NRemoteLobbyPlayer._Ready))]
internal static class RemoteLobbyPlayerIconScopePatch
{
    private static void Prefix(NRemoteLobbyPlayer __instance, out IDisposable? __state) =>
        __state = MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.PlayerId);

    private static void Postfix(
        NRemoteLobbyPlayer __instance,
        IDisposable? __state)
    {
        __state?.Dispose();
        // The deferred queue is the single refresh path.  Calling back into the scene tree from
        // _Ready can re-enter NRemoteLobbyPlayer while the lobby is still adding its children.
        MultiplayerSkinSync.RequestIconRefresh(__instance.PlayerId);
    }
}

// NRemoteLobbyPlayer.RefreshVisuals is also called when the lobby receives a character update,
// after its _Ready callback has already run.  Keep the per-player selection scope around that
// call as well; otherwise the game immediately overwrites a refreshed lobby avatar with the
// unscoped character texture.
[HarmonyPatch(typeof(NRemoteLobbyPlayer), "RefreshVisuals")]
internal static class RemoteLobbyPlayerVisualRefreshScopePatch
{
    private static void Prefix(NRemoteLobbyPlayer __instance, out IDisposable? __state) =>
        __state = MultiplayerSkinSync.BeginPlayerSelectionScope(__instance.PlayerId);

    private static void Postfix(
        NRemoteLobbyPlayer __instance,
        IDisposable? __state)
    {
        __state?.Dispose();
        MultiplayerSkinSync.RequestIconRefresh(__instance.PlayerId);
    }
}

[HarmonyPatch(typeof(NMultiplayerVoteContainer), nameof(NMultiplayerVoteContainer.RefreshPlayerVotes))]
internal static class MultiplayerVoteIconRefreshPatch
{
    private static void Postfix(NMultiplayerVoteContainer __instance) =>
        ContextualSkinControls.RefreshMultiplayerVoteIcons(__instance);
}

[HarmonyPatch(typeof(NMuteInBackgroundHandler), nameof(NMuteInBackgroundHandler._Notification))]
internal static class SkinPopupBackgroundMutePatch
{
    private static bool Prefix(NMuteInBackgroundHandler __instance, int what) =>
        !ContextualSkinControls.ShouldIgnoreBackgroundMute(__instance, what);
}











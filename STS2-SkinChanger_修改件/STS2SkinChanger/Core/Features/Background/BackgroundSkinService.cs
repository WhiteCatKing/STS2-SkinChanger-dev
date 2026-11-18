using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;

using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Core;

internal sealed record BackgroundOption(
    string Id,
    string DisplayName,
    string Source,
    string Title,
    BackgroundPackShell? Pack);

/// <summary>
/// Switches the battle background of a whole act. Every detected background (the game's own act
/// and encounter scenes plus each pack's layered battlefields) is offered for every act, so one
/// act can borrow any other act's look. "跟随分类" and "模组默认" keep the providers' own
/// per-encounter behaviour; "游戏默认" restores the game's own background; an explicit choice
/// replaces the background of every battle in that act.
/// </summary>
internal static class BackgroundSkinService
{
    private const string GameSourceId = "game";
    // Child node holding the chosen battlefield behind an encounter-owned background.
    private const string EncounterBackdropName = "SkinChangerEncounterBackdrop";
    // The option id currently grafted behind that background ("游戏默认" clears it).
    private const string EncounterBackdropOptionMeta = "sc_encounter_backdrop_option";
    private static readonly object Sync = new();
    private static readonly string[] BaseActIds = ["overgrowth", "underdocks", "hive", "glory"];
    // NRun stores the mutable run state in "_state" (older builds used "_runState").
    private static readonly FieldInfo? RunStateField =
        AccessTools.Field(typeof(NRun), "_state") ?? AccessTools.Field(typeof(NRun), "_runState");
    private static readonly PropertyInfo? BackgroundProperty =
        AccessTools.Property(typeof(NCombatRoom), "Background");
    private static readonly PropertyInfo? HasCustomBackgroundProperty =
        AccessTools.Property(typeof(EncounterModel), "HasCustomBackground");
    // NCombatRoom keeps the combat visuals (and therefore the encounter) in "_visuals".
    private static readonly FieldInfo? CombatRoomVisualsField =
        AccessTools.Field(typeof(NCombatRoom), "_visuals");
    private static readonly HashSet<string> ReportedFailures = new(StringComparer.OrdinalIgnoreCase);
    private static List<BackgroundActEntry>? _acts;
    private static List<BackgroundOption>? _backgrounds;
    // The event pack's picture, read once per session and reused by previews and fights.
    private static Dictionary<string, string> _providerActDefaults =
        new(StringComparer.OrdinalIgnoreCase);

    internal sealed class BackgroundActEntry
    {
        public BackgroundActEntry(string actId, string title, List<BackgroundOption> options)
        {
            ActId = actId;
            Title = title;
            Options = options;
        }

        public string ActId { get; }
        public string Title { get; }
        public List<BackgroundOption> Options { get; private set; }

        internal void ReplaceOptions(List<BackgroundOption> options)
        {
            Options = options;
        }
    }

    public static IReadOnlyList<BackgroundActEntry> Acts
    {
        get
        {
            lock (Sync)
            {
                return _acts ??= SafeDiscoverActs();
            }
        }
    }

    public static string? CurrentActId
    {
        get
        {
            if (TryGetCurrentRunState(out var state))
            {
                try
                {
                    return NormalizeActId(state.Act.Id.Entry);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// The act the player is in. An act the catalogue has never seen (a new act added by a mod,
    /// or one whose id changed this session) is synthesised on the spot instead of returning
    /// null, so both the pause menu and the compendium keep working.
    /// </summary>
    public static BackgroundActEntry? CurrentAct
    {
        get
        {
            if (!TryGetCurrentRunState(out var state))
            {
                return null;
            }

            string actId;
            string title;
            try
            {
                actId = NormalizeActId(state.Act.Id.Entry);
                title = ReadActTitle(state.Act);
            }
            catch
            {
                return null;
            }

            return FindAct(actId) ?? new BackgroundActEntry(
                actId,
                title,
                CreateOptions(Backgrounds));
        }
    }

    /// <summary>All catalogue acts plus the current run's act when the catalogue does not know it.</summary>
    public static IReadOnlyList<BackgroundActEntry> AllActs
    {
        get
        {
            var acts = Acts.ToList();
            var current = CurrentAct;
            if (current != null && acts.All(entry =>
                    !entry.ActId.Equals(current.ActId, StringComparison.OrdinalIgnoreCase)))
            {
                acts.Add(current);
            }

            return acts;
        }
    }

    public static BackgroundActEntry? FindAct(string? actId)
    {
        if (string.IsNullOrWhiteSpace(actId))
        {
            return null;
        }

        var key = NormalizeActId(actId);
        return Acts.FirstOrDefault(entry =>
            entry.ActId.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public static string GetSelection(string actId)
    {
        var key = NormalizeActId(actId);
        return SkinService.Config.BackgroundSelections.TryGetValue(key, out var selection) &&
               !string.IsNullOrWhiteSpace(selection)
            ? selection
            : BackgroundSkinPolicy.FollowCategoryId;
    }

    public static bool SetSelection(string actId, string optionId)
    {
        var key = NormalizeActId(actId);
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var selections = SkinService.Config.BackgroundSelections;
        if (string.IsNullOrWhiteSpace(optionId) ||
            optionId.Equals(BackgroundSkinPolicy.FollowCategoryId, StringComparison.OrdinalIgnoreCase) ||
            optionId.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
        {
            if (optionId.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
            {
                selections[key] = optionId.Trim();
            }
            else
            {
                selections.Remove(key);
            }
        }
        else
        {
            selections[key] = optionId.Trim();
        }

        try
        {
            SkinService.Config.Save(SkinChangerPaths.Configuration("skin_changer.json"));
        }
        catch (Exception exception)
        {
            ModLog.Warn("保存背景选择失败：" + exception.GetBaseException().Message);
        }

        RefreshCurrentActIfNeeded(key);
        return true;
    }

    /// <summary>
    /// Final writer for <see cref="EncounterModel.CreateBackground"/>. A postfix is used so every
    /// other background provider can finish first and the explicit choice always wins; the
    /// replaced node was never added to the scene tree, so it is freed immediately.
    /// </summary>
    public static void ApplyToResult(
        EncounterModel encounter,
        ActModel act,
        Rng rng,
        ref NCombatBackground result)
    {
        try
        {
            var actId = NormalizeActId(act.Id.Entry);
            var selection = GetSelection(actId);
            if (IsFakeMerchantEncounter(encounter))
            {
                // The fake merchant fight happens inside the tent the event was held in, so it must
                // show the event pack's picture whatever the act-level setting says - except for
                // "游戏默认", which asks for the game's own art. Only an explicit choice of another
                // background replaces it with that background (handled below).
                if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryUseFakeMerchantVanillaBackground(ref result))
                    {
                        return;
                    }

                    if (TryUseFakeMerchantEventImageBackground(ref result))
                    {
                        return;
                    }

                    ModLog.Info("假商人战斗背景来源=遭遇自带。");
                    return;
                }

                if (BackgroundSkinPolicy.IsFollowCategory(selection) ||
                    selection.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
                {
                    if (TryUseFakeMerchantEventImageBackground(ref result))
                    {
                        return;
                    }

                    ModLog.Info("假商人战斗背景来源=遭遇自带。");
                    return;
                }
            }

            // A boss arena that the encounter itself declares is not ours to replace (see
            // EncounterOwnsBackground), but its scenery *layers* can be re-skinned: the encounter
            // node - and everything the boss cached inside it - stays where it is.
            if (!IsFakeMerchantEncounter(encounter) && !CanApplyActBackground(result, encounter))
            {
                if (!TryApplyEncounterBackdrop(result, encounter, actId, selection, act.Id.Entry, out _))
                {
                    ReportOnce(
                        "encounter-background:" + encounter.Id.Entry,
                        $"{encounter.Id.Entry} 自带专属战斗背景，已原样保留（所选背景无法垫在它之后）。");
                }

                return;
            }

            // "跟随分类" and "模组默认" prefer the selected provider that owns this act's
            // presentation (CZN battlefields for acts 1-3, St2bs custom acts). Only when no
            // provider claims the act does the vanilla/encounter background stay untouched.
            BackgroundOption? option = null;
            if (BackgroundSkinPolicy.IsFollowCategory(selection) ||
                selection.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
            {
                if (_providerActDefaults.TryGetValue(actId, out var providerId) &&
                    FindBackground(providerId) is { } providerOption &&
                    !providerOption.Source.Equals(GameSourceId, StringComparison.OrdinalIgnoreCase))
                {
                    option = providerOption;
                }
                else
                {
                    return;
                }
            }
            else if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                option = ResolveGameDefault(encounter, actId);
            }
            else
            {
                option = FindBackground(selection);
            }

            if (option == null)
            {
                ReportOnce(
                    "resolve:" + actId,
                    $"{act.Id.Entry} 背景选择 {selection} 已不存在或对应资源缺失，已保留原背景。");
                return;
            }

            var replacement = CreateOptionBackground(option, rng);
            if (replacement == null || !GodotObject.IsInstanceValid(replacement))
            {
                return;
            }

            if (result != null &&
                GodotObject.IsInstanceValid(result) &&
                !ReferenceEquals(result, replacement))
            {
                result.Free();
            }

            replacement.SetMeta("sc_background_applied", true);
            result = replacement;
            ModLog.Info($"已按背景选择创建 {act.Id.Entry} 战斗背景：{option.DisplayName}。");
        }
        catch (Exception exception)
        {
            ReportOnce(
                "create:" + encounter.Id.Entry,
                "创建战斗背景失败，已保留原背景：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// The background a dropdown entry should preview. "跟随分类" and "模组默认" show the act's
    /// own default look (the game's act background, or the pack's own default battlefield for an
    /// act the game does not ship); "游戏默认" does the same for an unknown act.
    /// </summary>
    /// <summary>
    /// Fallback for rooms whose background is set directly in a provider's
    /// NCombatRoom.SetUpBackground prefix (that path never goes through
    /// EncounterModel.CreateBackground). Applied only when the player made an explicit choice.
    /// </summary>
    public static void ApplyToRoom(NCombatRoom room, IRunState state)
    {
        try
        {
            var act = state.Act;
            var actId = NormalizeActId(act.Id.Entry);
            var selection = GetSelection(actId);
            if (IsFakeMerchantCombat(room))
            {
                // The fake merchant's fight happens inside the tent the event was held in. That
                // encounter ships the tent as its own background; follow/mod-default and
                // "游戏默认" both have to leave it alone instead of swapping in the act's
                // battlefield, which is what made the fight look unrelated to the event.
                return;
            }

            BackgroundOption? providerDefault = null;
            var roomEncounter = CurrentEncounter(room);
            if (!IsFakeMerchantEncounter(roomEncounter) &&
                !CanApplyActBackground(room.Background, roomEncounter))
            {
                // Same rule as ApplyToResult: never free an encounter-owned background (the Emperor
                // Crab keeps its whole body in NKaiserCrabBossBackground and both claws cache the
                // node; freeing it made AfterAddedToRoom hit a disposed object and aborted
                // StartCombatInternal). Re-skin its layers instead of replacing the node.
                if (!TryApplyEncounterBackdrop(room.Background, roomEncounter, actId, selection, act.Id.Entry, out _))
                {
                    ReportOnce(
                        "encounter-room-background:" + actId,
                        $"{act.Id.Entry} 的战斗背景由遭遇自带，已原样保留（所选背景无法垫在它之后）。");
                }

                return;
            }

            if (BackgroundSkinPolicy.IsFollowCategory(selection) ||
                selection.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
            {
                if (_providerActDefaults.TryGetValue(actId, out var providerId) &&
                    FindBackground(providerId) is { } providerOption &&
                    !providerOption.Source.Equals(GameSourceId, StringComparison.OrdinalIgnoreCase))
                {
                    providerDefault = providerOption;
                }
                else
                {
                    return;
                }
            }

            if (room.Background != null &&
                GodotObject.IsInstanceValid(room.Background) &&
                room.Background.HasMeta("sc_background_applied"))
            {
                return;
            }

            var option = providerDefault ??
                         (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase)
                             ? (GameBackgroundExists(actId) ? GameOption(actId) : null)
                             : FindBackground(selection));
            if (option == null)
            {
                return;
            }

            var rng = CreatePreviewRng();
            if (rng == null)
            {
                return;
            }

            var replacement = CreateOptionBackground(option, rng);
            if (replacement == null || !GodotObject.IsInstanceValid(replacement))
            {
                return;
            }

            var container = room.GetNodeOrNull<Control>("%BgContainer");
            var old = room.Background;
            try
            {
                BackgroundProperty?.SetValue(room, null);
            }
            catch
            {
                // Set the property below regardless.
            }

            if (old != null && GodotObject.IsInstanceValid(old))
            {
                container?.RemoveChild(old);
                old.QueueFree();
            }

            replacement.SetMeta("sc_background_applied", true);
            try
            {
                BackgroundProperty?.SetValue(room, replacement);
            }
            catch
            {
                // The node is still added below if the property is not writable.
            }

            container?.AddChild(replacement);
            ModLog.Info($"已按背景选择替换 {act.Id.Entry} 的房间背景：{option.DisplayName}。");
        }
        catch (Exception exception)
        {
            ReportOnce(
                "room:" + state.Act.Id.Entry,
                "替换房间背景失败，已保留原背景：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Makes sure the background an act uses as its default is also a selectable option. This is
    /// what keeps "跟随分类" and the free-switch list in sync: a background that can be shown as
    /// the default must be listed, whatever private path layout the provider used.
    /// </summary>
    public static void EnsureActDefaultOption(string actId)
    {
        try
        {
            var token = NormalizeToken(actId);
            if (token.Length == 0)
            {
                return;
            }

            var existing = Backgrounds.FirstOrDefault(option =>
            {
                var candidate = NormalizeToken(option.Title);
                return candidate.Length > 0 &&
                       (candidate.StartsWith(token, StringComparison.Ordinal) ||
                        token.StartsWith(candidate, StringComparison.Ordinal));
            });
            if (existing != null)
            {
                EnsuredDefaults[actId] = existing.Id;
                return;
            }

            var act = EnumerateActs().FirstOrDefault(candidate =>
                NormalizeActId(candidate.Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase));
            var rng = CreatePreviewRng();
            if (act == null || rng == null)
            {
                return;
            }

            BackgroundAssets assets;
            try
            {
                assets = act.GenerateBackgroundAssets(rng);
            }
            catch
            {
                return;
            }

            var path = assets.BackgroundScenePath;
            if (string.IsNullOrWhiteSpace(path) || !path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var title = System.IO.Path.GetFileNameWithoutExtension(path);
            if (title.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
            {
                title = title[..^"_background".Length];
            }

            var directory = path[..path.LastIndexOf('/')];
            var layers = ListLayersInDirectory(directory);
            var source = act.GetType().Assembly.GetName().Name ?? "act";
            var optionId = BackgroundSkinPolicy.ProviderOptionId(source, title);
            if (FindBackground(optionId) != null)
            {
                return;
            }

            var display = source + " · " + BackgroundSkinPolicy.DisplayName(title);
            var option = new BackgroundOption(
                optionId,
                display,
                source,
                layers.Count > 0 ? title : path,
                layers.Count > 0 ? new BackgroundPackShell(title, path, layers) : null);
            lock (Sync)
            {
                _backgrounds?.Add(option);
            }

            EnsuredDefaults[actId] = option.Id;
            RefreshActOptions();
            ModLog.Info($"已将 {act.Id.Entry} 的默认背景加入可切换选项：{display}。");
        }
        catch (Exception exception)
        {
            ModLog.Warn(
                $"同步 {actId} 的默认背景选项失败，已跳过：" +
                exception.GetBaseException().Message);
        }
    }

    private static List<string> ListLayersInDirectory(string directory)
    {
        var layers = new List<string>();
        try
        {
            using var handle = DirAccess.Open(directory + "/layers");
            if (handle == null)
            {
                return layers;
            }

            handle.ListDirBegin();
            while (true)
            {
                var name = handle.GetNext();
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                if (name.StartsWith(".", StringComparison.Ordinal) || handle.CurrentIsDir() ||
                    !SkinCatalog.IsScenePath(name, ".tscn"))
                {
                    continue;
                }

                layers.Add(SkinCatalog.StripRemap($"{directory}/layers/{name}"));
            }

            handle.ListDirEnd();
        }
        catch
        {
            // No layers means the shell is treated as a plain scene option.
        }

        return layers;
    }

    public static BackgroundOption? ResolvePreviewOption(string actId, string selection)
    {
        if (BackgroundSkinPolicy.IsFollowCategory(selection) ||
            selection.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
        {
            return ActDefaultOption(actId);
        }

        if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
        {
            return ActDefaultOption(actId);
        }

        var explicitOption = FindBackground(selection);
        if (explicitOption == null)
        {
            // A saved selection can point at a resource that no longer exists (e.g. an empty
            // *_bg.tscn shell replaced by its paired image). Fall back to the act's real default
            // instead of leaving a blank screen.
            var fallback = ActDefaultOption(actId);
            if (fallback != null)
            {
                ModLog.Info(
                    $"背景选择 {selection} 已失效，{actId} 改用默认背景：{fallback.DisplayName}。");
                return fallback;
            }

            return null;
        }

        // "Game default" is only meaningful for the four official acts. On a custom act the
        // generic game background would show the wrong (or vanilla) art, so use the mod default.
        if (explicitOption.Source.Equals(GameSourceId, StringComparison.OrdinalIgnoreCase) &&
            !BaseActIds.Contains(actId, StringComparer.OrdinalIgnoreCase))
        {
            var fallback = ActDefaultOption(actId);
            if (fallback != null)
            {
                return fallback;
            }
        }

        return explicitOption;
    }

    public static Control? CreatePreview(BackgroundOption option)
    {
        var rng = CreatePreviewRng();
        if (rng == null)
        {
            ReportOnce("preview-rng", "无法创建背景预览所需的随机数生成器。");
            return null;
        }

        return CreateOptionBackground(option, rng);
    }

    /// <summary>
    /// Whether an option can really render. Used before an entry is added to the list so it never
    /// offers a background that would leave the screen empty when selected (that is exactly how the
    /// fake merchant entry turned into a black fight background). The probe also mounts the fake
    /// merchant event pack: no selection would otherwise mount it while the list is built.
    /// </summary>
    private static bool TryProbeBackground(BackgroundOption option)
    {
        try
        {
            var rng = CreatePreviewRng();
            if (rng == null)
            {
                return false;
            }

            var preview = CreateOptionBackground(option, rng);
            var ok = preview != null &&
                     GodotObject.IsInstanceValid(preview) &&
                     HasLayerContent(preview);
            if (preview != null && GodotObject.IsInstanceValid(preview))
            {
                preview.Free();
            }

            return ok;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "probe:" + option.Id,
                $"{option.DisplayName} 背景自检失败，已从列表移除：" +
                exception.GetBaseException().Message);
            return false;
        }
    }

    private static NCombatBackground? CreateOptionBackground(BackgroundOption option, Rng rng)
    {
        // An option whose title is the act's own empty placeholder should show the act's real
        // boss artwork instead of a transparent screen.
        if (IsActStubTitle(option.Title))
        {
            var sibling = FindBossSibling(option);
            if (sibling != null)
            {
                option = sibling;
            }
        }

        if (option.Source.Equals(GameSourceId, StringComparison.OrdinalIgnoreCase))
        {
            return CreateGameBackground(option.Title, rng);
        }

        if (option.Pack != null)
        {
            return CreatePackBackground(option, rng);
        }

        if (option.Title == FakeMerchantEventArtPolicy.EncounterBackgroundScene && option.Source != GameSourceId)
        {
            var layers = ListLayerPaths(FakeMerchantEncounterId);
            return SkinService.WithBackgroundEventResources(option.Source, layers.Append(option.Title).ToArray(), resources =>
            {
                var root = ((PackedScene)resources[option.Title]).Instantiate<NCombatBackground>();
                FillLayers(root, root, layers, rng,
                    path => ((PackedScene)resources[path]).Instantiate<Control>(PackedScene.GenEditState.Disabled));
                return root;
            });
        }

        return option.Title.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase)
            ? CreateSceneBackground(option)
            : CreateImageBackground(option);
    }

    private static NCombatBackground? CreateImageBackground(BackgroundOption option)
    {
        try
        {
            var isFakeMerchantEventImage = FakeMerchantEventArtPolicy.EventImage.Equals(
                option.Title,
                StringComparison.OrdinalIgnoreCase);
            var texture = isFakeMerchantEventImage
                ? SkinService.WithBackgroundEventResource(option.Source, option.Title, resource => resource as Texture2D)
                : PreloadManager.Cache.GetTexture2D(option.Title);
            if (texture == null)
            {
                ReportOnce("image:" + option.Id, $"{option.DisplayName} 的图片尚未挂载。");
                return null;
            }

            var background = new NCombatBackground
            {
                Name = "SkinChangerImageBackground",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            var rect = new TextureRect
            {
                Name = "BackgroundImage",
                Texture = texture,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Position = new Vector2(-960f, -540f),
                Size = new Vector2(1920f, 1080f)
            };
            background.AddChild(rect);
            return background;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "image:" + option.Id,
                $"{option.DisplayName} 背景图片创建失败：" +
                exception.GetBaseException().Message);
            return null;
        }
    }

    /// <summary>
    /// The default background of a custom act (one the game itself does not ship). The act's own
    /// GenerateBackgroundAssets is already patched by content mods to point at their scene, so
    /// asking the act is the only provider-neutral way to preview it.
    /// </summary>
    /// <summary>
    /// Last-resort default: a content mod may register an act whose own GenerateBackgroundAssets
    /// cannot produce a scene in the compendium context (its patch only answers for the current
    /// act). Match the act id against the background scene names the same package declared, so a
    /// sensible default still appears instead of a blank screen.
    /// </summary>
    public static BackgroundOption? ResolveActDefaultFallback(string actId)
    {
        EnsureActDefaultOption(actId);
        var target = NormalizeToken(actId);
        if (target.Length == 0)
        {
            return null;
        }

        var targetBase = BaseTokenOf(target);
        var prefixMatch = Backgrounds.FirstOrDefault(option =>
        {
            var candidate = NormalizeToken(
                System.IO.Path.GetFileNameWithoutExtension(option.Title));
            return candidate.Length >= 6 &&
                   (candidate.StartsWith(targetBase, StringComparison.Ordinal) ||
                    targetBase.StartsWith(candidate, StringComparison.Ordinal));
        });
        if (prefixMatch != null)
        {
            return prefixMatch;
        }

        BackgroundOption? best = null;
        var bestScore = 0d;
        foreach (var option in Backgrounds)
        {
            if (option.Source.Equals(GameSourceId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var name = NormalizeToken(
                System.IO.Path.GetFileNameWithoutExtension(option.Title));
            if (name.Length == 0)
            {
                continue;
            }

            var score = Similarity(target, name);
            if (score > bestScore)
            {
                bestScore = score;
                best = option;
            }
        }

        // A loose match is only used when the act itself could not produce a background, so a
        // plausible artwork beats a blank screen ("CARROLL_RIVER" → Carol.tscn scores 0.42).
        return bestScore >= 0.4 ? best : null;
    }

    private static BackgroundOption? ResolveEncounterImageDefault(ActModel act)
    {
        string[] tokens;
        try
        {
            tokens = act.AllEncounters
                .Take(16)
                .Select(encounter => NormalizeToken(encounter.Id.Entry))
                .Where(token => token.Length >= 6)
                .ToArray();
        }
        catch
        {
            return null;
        }

        if (tokens.Length == 0)
        {
            return null;
        }

        BackgroundOption? best = null;
        var bestLength = 0;
        foreach (var option in Backgrounds)
        {
            if (option.Pack != null ||
                !IsBackgroundImagePath(option.Title))
            {
                continue;
            }

            var pathToken = NormalizeToken(option.Title);
            foreach (var token in tokens)
            {
                if (token.Length > bestLength &&
                    pathToken.Contains(token, StringComparison.Ordinal))
                {
                    best = option;
                    bestLength = token.Length;
                }
            }
        }

        return best;
    }

    private static bool IsVanillaActBackground(string? scenePath, string actId)
    {
        if (string.IsNullOrWhiteSpace(scenePath))
        {
            return false;
        }

        var path = scenePath.Replace('\\', '/');
        foreach (var baseId in BaseActIds)
        {
            if (actId.Equals(baseId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (path.Contains(
                    $"/scenes/backgrounds/{baseId}/{baseId}_background",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A template-act content mod inherits a vanilla act's default background, but its own battle
    /// look comes from its encounters. Building a representative encounter's background shows the
    /// artwork the player actually sees in that map instead of the borrowed vanilla scene.
    /// </summary>
    private static NCombatBackground? CreateRepresentativeEncounterBackground(ActModel act, Rng rng)
    {
        try
        {
            foreach (var canonical in act.AllEncounters.Take(10))
            {
                if (canonical == null)
                {
                    continue;
                }

                EncounterModel encounter;
                try
                {
                    encounter = canonical.IsMutable ? canonical : canonical.ToMutable();
                }
                catch
                {
                    encounter = canonical;
                }

                try
                {
                    var background = encounter.CreateBackground(act, rng);
                    if (background != null && HasLayerContent(background))
                    {
                        return background;
                    }

                    if (background != null && GodotObject.IsInstanceValid(background))
                    {
                        background.QueueFree();
                    }
                }
                catch
                {
                    // Try the next encounter; one unsupported encounter must not blank the preview.
                }
            }
        }
        catch (Exception exception)
        {
            ReportOnce(
                "representative:" + act.Id.Entry,
                $"无法获取 {act.Id.Entry} 的代表性战斗背景：" +
                exception.GetBaseException().Message);
        }

        return null;
    }

    private static bool HasVisibleLayerContent(Node background)
    {
        var sawLayer = false;
        foreach (var child in background.GetChildren())
        {
            var name = child.Name.ToString();
            if (name.StartsWith("Layer_", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Foreground", StringComparison.OrdinalIgnoreCase))
            {
                sawLayer = true;
                if (child.GetChildCount() > 0)
                {
                    return true;
                }
            }
        }

        return !sawLayer && background.GetChildCount() > 0;
    }

    private static bool IsActStubTitle(string title)
    {
        var token = NormalizeToken(System.IO.Path.GetFileNameWithoutExtension(title));
        return token.EndsWith("act", StringComparison.Ordinal) ||
               token.EndsWith("stage", StringComparison.Ordinal);
    }

    private static BackgroundOption? FindBossSibling(BackgroundOption option)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(option.Title);
        var token = NormalizeToken(name);
        var baseToken = BaseTokenOf(token);
        if (baseToken.Length < 6)
        {
            return null;
        }

        return Backgrounds.FirstOrDefault(candidate =>
            !candidate.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase) &&
            candidate.Source.Equals(option.Source, StringComparison.OrdinalIgnoreCase) &&
            NormalizeToken(System.IO.Path.GetFileNameWithoutExtension(candidate.Title))
                .StartsWith(baseToken, StringComparison.Ordinal) &&
            (NormalizeToken(candidate.Title).Contains("boss", StringComparison.Ordinal) ||
             NormalizeToken(candidate.Title).Contains("encounter", StringComparison.Ordinal)));
    }

    private static bool HasLayerContent(Node background)
    {
        foreach (var child in background.GetChildren())
        {
            var name = child.Name.ToString();
            if (name.StartsWith("Layer_", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("Foreground", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("BattlefieldCanvas", StringComparison.OrdinalIgnoreCase))
            {
                if (child.GetChildCount() > 0)
                {
                    return true;
                }

                continue;
            }

            // A scene-based custom background puts its own artwork directly under the node.
            return true;
        }

        return false;
    }

    private static string? ReadCustomBackgroundPath(object encounter)
    {
        const BindingFlags flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        foreach (var name in new[]
                 {
                     "CustomBackgroundScenePath",
                     "CustomBackgroundPath",
                     "BackgroundScenePath"
                 })
        {
            var property = encounter.GetType().GetProperty(name, flags);
            if (property == null ||
                property.PropertyType != typeof(string) ||
                property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            try
            {
                if (property.GetValue(encounter) is string { Length: > 0 } value)
                {
                    return value;
                }
            }
            catch
            {
                // Try the next property name.
            }
        }

        return null;
    }

    private static BackgroundAssets? PrepCustomBackgroundAssets(EncounterModel encounter, ActModel act)
    {
        try
        {
            var rng = CreatePreviewRng();
            if (rng == null)
            {
                return null;
            }

            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var mutable = encounter.IsMutable ? encounter : encounter.ToMutable();
            var method = mutable.GetType().GetMethod("PrepCustomBackground", flags);
            if (method == null)
            {
                return null;
            }

            method.Invoke(mutable, [act, rng]);
            for (var type = mutable.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField("_customBackgroundAssets", flags);
                if (field?.GetValue(mutable) is BackgroundAssets assets &&
                    !string.IsNullOrWhiteSpace(assets.BackgroundScenePath))
                {
                    return assets;
                }
            }
        }
        catch
        {
            // The encounter does not support BaseLib custom backgrounds.
        }

        return null;
    }

    private static BackgroundOption? RegisterPreparedOption(
        string actId,
        ActModel act,
        BackgroundAssets assets)
    {
        try
        {
            var shellPath = assets.BackgroundScenePath.Replace('\\', '/');
            var layers = new List<string>(
                assets.BgLayers.Select(path => path.Replace('\\', '/')));
            if (!string.IsNullOrWhiteSpace(assets.FgLayer))
            {
                layers.Add(assets.FgLayer.Replace('\\', '/'));
            }

            layers = layers
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (layers.Count == 0)
            {
                return null;
            }

            var title = System.IO.Path.GetFileNameWithoutExtension(shellPath);
            if (title.EndsWith("_bg", StringComparison.OrdinalIgnoreCase))
            {
                title = title[..^"_bg".Length];
            }

            var source = act.GetType().Assembly.GetName().Name ?? "act-default";
            var optionId = BackgroundSkinPolicy.ProviderOptionId(source, "prepared:" + title);
            var existing = FindBackground(optionId);
            if (existing != null)
            {
                return existing;
            }

            var option = new BackgroundOption(
                optionId,
                source + " · " + BackgroundSkinPolicy.DisplayName(title),
                source,
                title,
                new BackgroundPackShell(title, shellPath, layers));
            lock (Sync)
            {
                _backgrounds?.Add(option);
            }

            EnsuredDefaults[actId] = option.Id;
            RefreshActOptions();
            ModLog.Info(
                $"已按 BaseLib 自定义遭遇注册 {act.Id.Entry} 的默认背景：" +
                $"{option.DisplayName}（图层 {layers.Count}：" +
                string.Join(" | ", layers.Take(4)) + "）。");
            return option;
        }
        catch
        {
            return null;
        }
    }

    private static BackgroundOption? RegisterScenePathOption(
        string actId,
        ActModel act,
        string scenePath)
    {
        try
        {
            scenePath = scenePath.Replace('\\', '/');
            var source = act.GetType().Assembly.GetName().Name ?? "act-default";
            var optionId = BackgroundSkinPolicy.ProviderOptionId(source, scenePath);
            var existing = FindBackground(optionId);
            if (existing != null)
            {
                return existing;
            }

            var title = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (title.EndsWith("_bg", StringComparison.OrdinalIgnoreCase))
            {
                title = title[..^"_bg".Length];
            }

            var option = new BackgroundOption(
                optionId,
                source + " · " + BackgroundSkinPolicy.DisplayName(title),
                source,
                scenePath,
                null);
            lock (Sync)
            {
                _backgrounds?.Add(option);
            }

            EnsuredDefaults[actId] = option.Id;
            RefreshActOptions();
            ModLog.Info(
                $"已按 BaseLib 遭遇场景登记 {act.Id.Entry} 的背景：{option.DisplayName}（{scenePath}）。");
            return option;
        }
        catch
        {
            return null;
        }
    }

    private static BackgroundOption? RegisterImageOption(
        string actId,
        ActModel act,
        string imagePath)
    {
        try
        {
            imagePath = imagePath.Replace('\\', '/');
            var source = act.GetType().Assembly.GetName().Name ?? "act-default";
            var title = System.IO.Path.GetFileNameWithoutExtension(imagePath);
            var optionId = BackgroundSkinPolicy.ProviderOptionId(source, imagePath);
            var existing = FindBackground(optionId);
            if (existing != null)
            {
                return existing;
            }

            var option = new BackgroundOption(
                optionId,
                source + " · " + BackgroundSkinPolicy.DisplayName(title),
                source,
                imagePath,
                null);
            lock (Sync)
            {
                _backgrounds?.Add(option);
            }

            EnsuredDefaults[actId] = option.Id;
            RefreshActOptions();
            ModLog.Info(
                $"已按 BaseLib 遭遇图层登记 {act.Id.Entry} 的背景：{option.DisplayName}。");
            return option;
        }
        catch
        {
            return null;
        }
    }

    private static string DescribeNode(Node node, int depth)
    {
        if (depth > 4)
        {
            return "...";
        }

        var builder = new System.Text.StringBuilder();
        builder.Append(node.Name).Append('(').Append(node.GetType().Name).Append(')');
        foreach (var child in node.GetChildren())
        {
            builder.Append(" [").Append(DescribeNode(child, depth + 1)).Append(']');
        }

        return builder.ToString();
    }

    private static string SceneStem(string path)
    {
        var token = NormalizeToken(
            System.IO.Path.GetFileNameWithoutExtension(path));
        if (token.EndsWith("bg", StringComparison.Ordinal) && token.Length > 2)
        {
            token = token[..^2];
        }

        return token;
    }

    private static BackgroundOption? FindImageOptionForScene(string scenePath)
    {
        var stem = SceneStem(scenePath);
        if (stem.Length == 0)
        {
            return null;
        }

        return Backgrounds.FirstOrDefault(option =>
            IsBackgroundImagePath(option.Title) &&
            SceneStem(option.Title).Equals(stem, StringComparison.OrdinalIgnoreCase));
    }

    private static string BaseTokenOf(string token)
    {
        foreach (var suffix in new[] { "act", "stage" })
        {
            if (token.Length > suffix.Length + 4 &&
                token.EndsWith(suffix, StringComparison.Ordinal))
            {
                return token[..^suffix.Length];
            }
        }

        return token;
    }

    private static string NormalizeToken(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());

    private static double Similarity(string left, string right)
    {
        if (left.Length == 0 || right.Length == 0)
        {
            return 0;
        }

        if (left.Contains(right, StringComparison.Ordinal) ||
            right.Contains(left, StringComparison.Ordinal))
        {
            return 1;
        }

        var distance = Levenshtein(left, right);
        return 1d - distance / (double)Math.Max(left.Length, right.Length);
    }

    private static int Levenshtein(string left, string right)
    {
        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];
        for (var index = 0; index <= right.Length; index++)
        {
            previous[index] = index;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= right.Length; j++)
            {
                var cost = left[i - 1] == right[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }

    private static string? _lastDefaultScenePath;
    private static string? _lastDefaultSource;
    private static string? _lastDefaultActId;
    private static readonly Dictionary<string, string> EnsuredDefaults =
        new(StringComparer.OrdinalIgnoreCase);

    private static void RefreshActOptions()
    {
        lock (Sync)
        {
            if (_acts == null)
            {
                return;
            }

            foreach (var entry in _acts)
            {
                entry.ReplaceOptions(CreateOptions(Backgrounds));
            }
        }
    }

    /// <summary>
    /// After rendering the "follow category" preview, register the exact resource it used as a
    /// selectable option. This closes the gap where a provider exposes a default through a private
    /// path that the discovery scan never sees.
    /// </summary>
    public static bool RegisterLastDefaultAsOption()
    {
        var path = _lastDefaultScenePath;
        var source = _lastDefaultSource;
        var isScene = path?.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) == true;
        var isImage = path != null && IsBackgroundImagePath(path);
        if (string.IsNullOrWhiteSpace(path) || (!isScene && !isImage))
        {
            return false;
        }

        try
        {
            var title = System.IO.Path.GetFileNameWithoutExtension(path);
            if (isScene && title.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
            {
                title = title[..^"_background".Length];
            }

            if (Backgrounds.Any(option =>
                    option.Title.Equals(path, StringComparison.OrdinalIgnoreCase) ||
                    (NormalizeToken(option.Title) == NormalizeToken(title) &&
                     option.Source.Equals(source, StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }

            var directory = path[..path.LastIndexOf('/')];
            var layers = ListLayersInDirectory(directory);
            var provider = string.IsNullOrWhiteSpace(source) ? "act-default" : source;
            var option = new BackgroundOption(
                BackgroundSkinPolicy.ProviderOptionId(provider, title),
                provider + " · " + BackgroundSkinPolicy.DisplayName(title),
                provider,
                layers.Count > 0 ? title : path,
                layers.Count > 0 ? new BackgroundPackShell(title, path, layers) : null);
            lock (Sync)
            {
                _backgrounds?.Add(option);
            }

            if (!string.IsNullOrWhiteSpace(_lastDefaultActId))
            {
                EnsuredDefaults[_lastDefaultActId] = option.Id;
            }

            RefreshActOptions();
            ModLog.Info($"已将默认背景加入可切换选项：{option.DisplayName}（{path}）。");
            return true;
        }
        catch (Exception exception)
        {
            ModLog.Warn("登记默认背景选项失败：" + exception.GetBaseException().Message);
            return false;
        }
    }

    public static Control? CreateActDefaultPreview(string actId)
    {
        _lastDefaultScenePath = null;
        _lastDefaultSource = null;
        _lastDefaultActId = actId;
        try
        {
            var act = EnumerateActs().FirstOrDefault(candidate =>
                NormalizeActId(candidate.Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase));
            if (act == null)
            {
                return null;
            }

            var rng = CreatePreviewRng();
            if (rng == null)
            {
                return null;
            }

            var assets = act.GenerateBackgroundAssets(rng);
            _lastDefaultScenePath = assets.BackgroundScenePath;
            _lastDefaultSource = act.GetType().Assembly.GetName().Name;
            var background = NCombatBackground.Create(assets);
            // Template-act content mods derive from a vanilla act and inherit its background.
            // For a modded act that is visually not that vanilla act, a representative encounter
            // background is much closer to what the player sees in battle.
            var inheritsVanillaAct = IsVanillaActBackground(assets.BackgroundScenePath, actId);
            if (background == null ||
                !HasLayerContent(background) ||
                inheritsVanillaAct)
            {
                // Content mods that paint battle backgrounds as images usually name the folder
                // after the encounter; match one of this act's own encounters and use that image.
                var imageDefault = ResolveEncounterImageDefault(act);
                if (imageDefault != null)
                {
                    var imageBackground = CreateImageBackground(imageDefault);
                    if (imageBackground != null)
                    {
                        _lastDefaultScenePath = imageDefault.Title;
                        _lastDefaultSource = imageDefault.Source;
                        if (background != null && GodotObject.IsInstanceValid(background))
                        {
                            background.QueueFree();
                        }

                        return imageBackground;
                    }
                }

                var representative = CreateRepresentativeEncounterBackground(act, rng);
                if (representative != null)
                {
                    if (background != null && GodotObject.IsInstanceValid(background))
                    {
                        background.QueueFree();
                    }

                    return representative;
                }
            }

            if (background != null &&
                (!HasLayerContent(background) || inheritsVanillaAct))
            {
                // Either an empty asset set or a borrowed vanilla act background (Briah and
                // similar template acts). Returning null lets the caller use this act's own
                // encounter/mod background instead of the old game art.
                background.QueueFree();
                return null;
            }

            return background;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "act-default:" + actId,
                $"无法创建 {actId} 的默认背景预览：" + exception.GetBaseException().Message);
            return null;
        }
    }

    private static NCombatBackground? CreateSceneBackground(
        BackgroundOption option,
        bool allowSiblingFallback = true)
    {
        try
        {
            var scene = PreloadManager.Cache.GetScene(option.Title.Replace('\\', '/'));
            if (scene == null)
            {
                ReportOnce(
                    "scene:" + option.Id,
                    $"{option.DisplayName} 的背景场景尚未挂载，已保留原背景。");
                return null;
            }

            var instance = scene.Instantiate(PackedScene.GenEditState.Disabled);
            if (instance is not Control root)
            {
                // Ruina2 and friends author battle scenes as Node2D trees. Wrap them in a
                // screen-sized Control instead of failing the cast and leaving a blank preview.
                if (instance is Node2D node2d)
                {
                    var host = new Control
                    {
                        Name = "SkinChangerSceneHost",
                        MouseFilter = Control.MouseFilterEnum.Ignore,
                        Position = new Vector2(-960f, -540f),
                        Size = new Vector2(1920f, 1080f)
                    };
                    host.AddChild(node2d);
                    var nodeWrapper = new NCombatBackground
                    {
                        Name = "SkinChangerSceneBackground",
                        MouseFilter = Control.MouseFilterEnum.Ignore
                    };
                    nodeWrapper.AddChild(host);
                    return nodeWrapper;
                }

                if (GodotObject.IsInstanceValid(instance))
                {
                    instance.QueueFree();
                }

                return null;
            }

            if (root is NCombatBackground background)
            {
                return background;
            }

            var wrapper = new NCombatBackground
            {
                Name = "SkinChangerSceneBackground",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            while (root.GetChildCount() > 0)
            {
                var child = root.GetChild(0);
                root.RemoveChild(child);
                wrapper.AddChild(child);
            }

            root.QueueFree();

            if (allowSiblingFallback && !HasVisibleLayerContent(wrapper))
            {
                var sibling = FindBossSibling(option);
                if (sibling != null)
                {
                    wrapper.QueueFree();
                    return CreateSceneBackground(sibling, allowSiblingFallback: false);
                }
            }

            return wrapper;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "scene:" + option.Id,
                $"{option.DisplayName} 背景创建失败，已保留原背景：" +
                exception.GetBaseException().Message);
            return null;
        }
    }

    /// <summary>
    /// The game changed Rng's constructor shape between branches (0.107 uses a two-argument
    /// ctor, 0.111 a one-argument one). Reflection picks whatever the running game offers instead
    /// of binding to one branch at build time.
    /// </summary>
    private static Rng? CreatePreviewRng()
    {
        var rngType = typeof(Rng);
        foreach (var constructor in rngType.GetConstructors(
                     BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length != 1)
            {
                continue;
            }

            var type = parameters[0].ParameterType;
            object? seed = type == typeof(ulong) ? 20260814UL
                : type == typeof(uint) ? 20260814u
                : type == typeof(long) ? 20260814L
                : type == typeof(int) ? 20260814
                : null;
            if (seed == null)
            {
                continue;
            }

            try
            {
                return constructor.Invoke([seed]) as Rng;
            }
            catch
            {
                // Try the next shape.
            }
        }

        return rngType.GetProperty("Chaotic", BindingFlags.Public | BindingFlags.Static)
            ?.GetValue(null) as Rng;
    }

    public static BackgroundOption? DefaultOptionFor(string actId) => ActDefaultOption(actId);

    /// <summary>
    /// Startup self-check: resolves and renders every non-vanilla act's default background and
    /// logs a single PASS/FAIL line, so regressions are visible without opening the compendium.
    /// </summary>
    public static void RunSelfCheck()
    {
        try
        {
            foreach (var act in Acts)
            {
                if (BaseActIds.Contains(act.ActId, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                var option = DefaultOptionFor(act.ActId);
                if (option == null)
                {
                    ModLog.Warn($"背景自检失败 {act.ActId}：没有解析到默认背景。");
                    continue;
                }

                var preview = CreatePreview(option);
                var ok = preview != null &&
                         GodotObject.IsInstanceValid(preview) &&
                         HasLayerContent(preview);
                ModLog.Info(
                    $"背景自检 {act.ActId}：{(ok ? "通过" : "空白")} → " +
                    $"{option.Source} · {option.Title}");
                if (!ok && preview != null && GodotObject.IsInstanceValid(preview))
                {
                    ModLog.Warn(
                        $"背景自检节点 {act.ActId}：" +
                        DescribeNode(preview, 0));
                }
                if (preview != null && GodotObject.IsInstanceValid(preview))
                {
                    preview.QueueFree();
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("背景自检异常：" + exception.GetBaseException().Message);
        }
    }

    private static BackgroundOption? ActDefaultOption(string actId)
    {
        // A selected provider that owns this act's presentation (CZN battlefields, St2bs acts)
        // must win over the vanilla default, otherwise "follow category" shows the old art.
        if (_providerActDefaults.TryGetValue(actId, out var providerOptionId) &&
            FindBackground(providerOptionId) is { } providerOption)
        {
            return providerOption;
        }

        if (EnsuredDefaults.TryGetValue(actId, out var ensuredId) &&
            FindBackground(ensuredId) is { } ensured)
        {
            return ensured;
        }

        // Only the four official acts use their id as the background title. A modded act can
        // have an empty stub folder with the same name, so it must go through the matching
        // below (encounters → boss → prefix).
        if (BaseActIds.Contains(actId, StringComparer.OrdinalIgnoreCase) &&
            GameBackgroundExists(actId))
        {
            return FindBackground(
                BackgroundSkinPolicy.ProviderOptionId(GameSourceId, actId));
        }

        var actToken = NormalizeToken(actId);
        var actBaseToken = BaseTokenOf(actToken);

        // BaseLib custom encounters declare their battle scene through
        // CustomBackgroundScenePath (Ruina2 images/background/*_bg.tscn). Use the act's own
        // encounter art instead of the vanilla act default.
        try
        {
            var act = EnumerateActs().FirstOrDefault(candidate =>
                NormalizeActId(candidate.Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase));
            if (act != null)
            {
                foreach (var encounter in act.AllEncounters
                             .Concat(act.BossDiscoveryOrder)
                             .Take(24))
                {
                    var prepared = PrepCustomBackgroundAssets(encounter, act);
                    if (prepared != null)
                    {
                        var layerScene = new[] { prepared.FgLayer }
                            .Concat(prepared.BgLayers)
                            .FirstOrDefault(candidate =>
                                !string.IsNullOrWhiteSpace(candidate) &&
                                candidate.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase));
                        if (layerScene != null)
                        {
                            var pairedImage = FindImageOptionForScene(layerScene);
                            if (pairedImage != null)
                            {
                                return pairedImage;
                            }
                        }

                        var layerImage = prepared.BgLayers.FirstOrDefault(IsBackgroundImagePath);
                        if (layerImage != null)
                        {
                            var layerOption = RegisterImageOption(actId, act, layerImage);
                            if (layerOption != null)
                            {
                                return layerOption;
                            }
                        }

                        var imageOption = FindImageOptionForScene(prepared.BackgroundScenePath);
                        if (imageOption != null)
                        {
                            return imageOption;
                        }

                        var preparedOption = RegisterPreparedOption(actId, act, prepared);
                        if (preparedOption != null)
                        {
                            return preparedOption;
                        }
                    }

                    var path = ReadCustomBackgroundPath(encounter);
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        var imageOption = FindImageOptionForScene(path);
                        if (imageOption != null)
                        {
                            return imageOption;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(path) ||
                        path.Contains("/images/background/", StringComparison.OrdinalIgnoreCase))
                    {
                        // A bare Ruina2-style scene is only a shell; its layers come from
                        // PrepCustomBackgroundAssets. Building it alone produces a blank screen.
                        continue;
                    }

                    var match = Backgrounds.FirstOrDefault(option =>
                        option.Title.Equals(path, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                    {
                        return match;
                    }
                }
            }
        }
        catch
        {
            // Fall through to the other matching strategies.
        }

        // A boss/story act's real battle art is often the background of one of its encounters
        // (DISCORDANT_HEART_ACT → discordant_heart_boss_encounter). Only consider encounters
        // that belong to this act's own name prefix; a shared act-4 boss must not make the
        // Wuthering Heights map display the Discordant Heart background.
        try
        {
            var act = EnumerateActs().FirstOrDefault(candidate =>
                NormalizeActId(candidate.Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase));
            if (act != null)
            {
                var encounterTokens = act.AllEncounters
                    .Concat(act.BossDiscoveryOrder)
                    .Take(32)
                    .Select(encounter => NormalizeToken(encounter.Id.Entry))
                    .Where(candidate => candidate.Length >= 6 &&
                        (candidate.StartsWith(actBaseToken, StringComparison.Ordinal) ||
                         actBaseToken.StartsWith(candidate, StringComparison.Ordinal)))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                BackgroundOption? encounterBest = null;
                var encounterBestLength = 0;
                foreach (var option in Backgrounds)
                {
                    var candidate = NormalizeToken(option.Title);
                    foreach (var encounterToken in encounterTokens)
                    {
                        if (encounterToken.Length > encounterBestLength &&
                            candidate.Contains(encounterToken, StringComparison.Ordinal))
                        {
                            encounterBest = option;
                            encounterBestLength = encounterToken.Length;
                        }
                    }
                }

                if (encounterBest != null)
                {
                    return encounterBest;
                }
            }
        }
        catch
        {
            // Fall through to the act-name prefix match.
        }

        // Content mods often name the background folder after the act with a suffix
        // (WUTHERING_HEIGHTS → wuthering_heights_act). Match by normalized prefix so the
        // act's own default can be found even though the ids are not identical.
        var token = actToken;
        if (token.Length < 6)
        {
            return null;
        }

        BackgroundOption? best = null;
        var bestLength = 0;
        foreach (var option in Backgrounds)
        {
            var candidate = NormalizeToken(option.Title);
            if (candidate.Length < 6)
            {
                continue;
            }

            if (!candidate.StartsWith(token, StringComparison.Ordinal) &&
                !token.StartsWith(candidate, StringComparison.Ordinal))
            {
                continue;
            }

            if (candidate.Length > bestLength)
            {
                best = option;
                bestLength = candidate.Length;
            }
        }

        // The act's own folder is often an empty placeholder while the real art is its boss
        // encounter's background (DISCORDANT_HEART_ACT → discordant_heart_boss_encounter).
        var baseToken = actBaseToken;
        var bestToken = best == null ? "" : NormalizeToken(best.Title);
        var isPlaceholderStub = best == null ||
                                (bestToken.StartsWith(baseToken, StringComparison.Ordinal) &&
                                 (bestToken.EndsWith("act", StringComparison.Ordinal) ||
                                  bestToken.EndsWith("stage", StringComparison.Ordinal)));
        if (baseToken.Length >= 6 && isPlaceholderStub)
        {
            BackgroundOption? bossOption = null;
            var bossLength = 0;
            foreach (var option in Backgrounds)
            {
                var candidate = NormalizeToken(option.Title);
                if (candidate.Length <= bossLength ||
                    !candidate.StartsWith(baseToken, StringComparison.Ordinal) ||
                    (!candidate.Contains("boss", StringComparison.Ordinal) &&
                     !candidate.Contains("encounter", StringComparison.Ordinal)))
                {
                    continue;
                }

                bossOption = option;
                bossLength = candidate.Length;
            }

            if (bossOption != null)
            {
                best = bossOption;
            }
            else
            {
                // Last resort: any background declared by the same mod as the act itself.
                string? assemblyName = null;
                try
                {
                    assemblyName = EnumerateActs()
                        .FirstOrDefault(candidate => NormalizeActId(candidate.Id.Entry)
                            .Equals(actId, StringComparison.OrdinalIgnoreCase))
                        ?.GetType().Assembly.GetName().Name;
                }
                catch
                {
                    // Ignore; keep the placeholder.
                }

                if (!string.IsNullOrWhiteSpace(assemblyName))
                {
                    BackgroundOption? prefixBest = null;
                    var prefixLength = 0;
                    BackgroundOption? anyBest = null;
                    var anyLength = 0;
                    foreach (var option in Backgrounds)
                    {
                        if (!option.Source.Equals(assemblyName, StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var candidate = NormalizeToken(option.Title);
                        if (candidate.StartsWith(baseToken, StringComparison.Ordinal))
                        {
                            if (candidate.Length > prefixLength)
                            {
                                prefixBest = option;
                                prefixLength = candidate.Length;
                            }
                        }

                        if (candidate.Length > anyLength)
                        {
                            anyBest = option;
                            anyLength = candidate.Length;
                        }
                    }

                    // Never borrow another act's artwork when one with the same prefix exists.
                    best = prefixBest ?? anyBest;
                }
            }
        }

        if (best == null)
        {
            // Last resort: a background from the act's own package still beats showing the
            // vanilla act art when the provider names its battle scenes by encounter/floor.
            try
            {
                var assembly = EnumerateActs()
                    .FirstOrDefault(candidate => NormalizeActId(candidate.Id.Entry)
                        .Equals(actId, StringComparison.OrdinalIgnoreCase))
                    ?.GetType().Assembly.GetName().Name;
                if (!string.IsNullOrWhiteSpace(assembly))
                {
                    best = Backgrounds.FirstOrDefault(option =>
                        option.Source.Equals(assembly, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch
            {
                // Keep null.
            }
        }

        ModLog.Info(
            $"地区默认背景 {actId} → " +
            (best == null ? "未找到" : $"{best.Source} · {best.Title}"));
        return best;
    }

    /// <summary>
    /// "游戏默认" is the game's own background for this encounter. Acts the game does not know
    /// (a new act, or a mod that renamed an existing one) simply keep the providers' presentation
    /// instead of failing, exactly like "跟随分类".
    /// </summary>
    private static BackgroundOption? ResolveGameDefault(EncounterModel encounter, string actId)
    {
        try
        {
            if (HasCustomBackgroundProperty?.GetValue(encounter) is true)
            {
                var encounterTitle = encounter.Id.Entry.ToLowerInvariant();
                if (GameBackgroundExists(encounterTitle))
                {
                    return GameOption(encounterTitle);
                }
            }
        }
        catch
        {
            // Fall through to the act background.
        }

        return GameBackgroundExists(actId) ? GameOption(actId) : null;
    }

    private static BackgroundOption GameOption(string title) =>
        FindBackground(BackgroundSkinPolicy.ProviderOptionId(GameSourceId, title)) ??
        new BackgroundOption(
            BackgroundSkinPolicy.ProviderOptionId(GameSourceId, title),
            BackgroundSkinPolicy.DisplayName(title),
            GameSourceId,
            title,
            null);

    /// <summary>
    /// Builds a game-format background ourselves instead of calling NCombatBackground.Create:
    /// content mods (for example St2bs) prefix that static method and redirect every request to
    /// their current act's custom scene, which made vanilla backgrounds impossible to select on
    /// their acts. Loading the chosen shell and adding its layers bypasses those prefixes.
    /// </summary>
    private static NCombatBackground? CreateGameBackground(string title, Rng rng)
    {
        try
        {
            var shellPath = $"res://scenes/backgrounds/{title}/{title}_background.tscn";
            if (ResourceLoader.Exists(shellPath))
            {
                var scene = PreloadManager.Cache.GetScene(shellPath);
                if (scene != null)
                {
                    var root = scene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
                    var layers = ListLayerPaths(title);
                    if (root is NCombatBackground background)
                    {
                        FillLayers(root, root, layers, rng);
                        return background;
                    }

                    // Some mods ship a plain Control shell instead of an NCombatBackground
                    // script; wrapping it keeps its authored children instead of falling back to
                    // the game's Create (which would cast the root and throw).
                    FillLayers(root, root.GetNodeOrNull<Node>("BattlefieldCanvas") ?? root, layers, rng);
                    if (root.Size == Vector2.Zero)
                    {
                        root.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                    }

                    var wrapper = new NCombatBackground
                    {
                        Name = "SkinChangerGameBackground",
                        MouseFilter = Control.MouseFilterEnum.Ignore
                    };
                    wrapper.AddChild(root);
                    return wrapper;
                }
            }

            return NCombatBackground.Create(new BackgroundAssets(title, rng));
        }
        catch (Exception exception)
        {
            ReportOnce(
                "game:" + title,
                $"游戏背景 {title} 无法创建，已保留原背景：" + exception.GetBaseException().Message);
            return null;
        }
    }

    private static List<string> ListLayerPaths(string title)
    {
        var paths = new List<string>();
        try
        {
            using var directory = DirAccess.Open($"res://scenes/backgrounds/{title}/layers");
            if (directory == null)
            {
                return paths;
            }

            directory.ListDirBegin();
            while (true)
            {
                var name = directory.GetNext();
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                if (name.StartsWith(".", StringComparison.Ordinal) ||
                    directory.CurrentIsDir() ||
                    !SkinCatalog.IsScenePath(name, ".tscn"))
                {
                    continue;
                }

                paths.Add(SkinCatalog.StripRemap(
                    $"res://scenes/backgrounds/{title}/layers/{name}"));
            }

            directory.ListDirEnd();
        }
        catch (Exception exception)
        {
            ReportOnce("layers:" + title, "列出背景层失败：" + exception.GetBaseException().Message);
        }

        return paths;
    }

    private static int FillLayers(
        Control shell,
        Node canvas,
        IEnumerable<string> layerPaths,
        Rng rng,
        Func<string, Control>? instantiate = null)
    {
        var paths = layerPaths.ToArray();
        var added = 0;
        var index = 0;
        foreach (var group in paths
                     .Where(path => path.Contains("_bg_", StringComparison.OrdinalIgnoreCase))
                     .GroupBy(path => LayerGroupKey(path, "_bg_"))
                     .OrderBy(group => group.Key, StringComparer.Ordinal))
        {
            var chosen = rng.NextItem(group.ToArray());
            var host = canvas.GetNodeOrNull<Node>($"Layer_{index:D2}") ??
                       canvas.GetNodeOrNull<Node>("Layer_00");
            index++;
            if (string.IsNullOrWhiteSpace(chosen) || host == null)
            {
                continue;
            }

            host.AddChild(instantiate?.Invoke(chosen) ?? InstantiateLayer(chosen));
            added++;
        }

        var foreground = paths.FirstOrDefault(path =>
            path.Contains("_fg_", StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(foreground))
        {
            var host = canvas.GetNodeOrNull<Node>("Foreground") ??
                       canvas.GetNodeOrNull<Node>("ForegroundLayer");
            if (host != null)
            {
                host.AddChild(instantiate?.Invoke(foreground!) ?? InstantiateLayer(foreground!));
                added++;
            }
        }

        return added;
    }

    /// <summary>
    /// Builds a pack's layered background generically: instantiate its shell, pick one variant per
    /// background layer group, add it to the shell's canvas layer slots and call the pack's own
    /// configure contract when the layer declares one. CZN uses a private BattlefieldCanvas; packs
    /// that follow the game's canonical layout use the root as the canvas and Layer_00.. nodes.
    /// </summary>
    private static NCombatBackground? CreatePackBackground(
        BackgroundOption option,
        Rng rng,
        bool allowSiblingFallback = true)
    {
        var shell = option.Pack;
        if (shell == null)
        {
            return null;
        }

        try
        {
            ManagedSkinModLoader.EnsureProviderGodotScripts(option.Source);
            if (!ResourceLoader.Exists(shell.ShellPath))
            {
                // The provider may not be selected as a skin right now. Mounting its package at
                // low priority only makes its private resources visible; the game's own files
                // still win for canonical paths.
                foreach (var packPath in SkinService.Catalog?.GetProviderResourcePackPaths(option.Source) ?? [])
                {
                    if (File.Exists(packPath))
                    {
                        ProjectSettings.LoadResourcePack(packPath, replaceFiles: false);
                    }
                }
            }

            var shellScene = PreloadManager.Cache.GetScene(shell.ShellPath);
            if (shellScene == null)
            {
                ReportOnce(
                    "pack:" + option.Id,
                    $"{option.DisplayName} 的背景资源尚未挂载，已保留原背景。");
                return null;
            }

            var root = shellScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
            var canvas = root.GetNodeOrNull<Node>("BattlefieldCanvas") ?? root;
            FillLayers(root, canvas, shell.LayerPaths, rng);

            var hasLayers = canvas.GetChildren().Any(child =>
                child.Name.ToString().StartsWith("Layer_", StringComparison.OrdinalIgnoreCase) &&
                child.GetChildCount() > 0);
            if (!hasLayers && allowSiblingFallback)
            {
                // An act folder can be an empty placeholder; its real art is then the boss
                // encounter's background. Use that instead of a transparent option.
                var baseToken = BaseTokenOf(NormalizeToken(option.Title));
                var sibling = Backgrounds.FirstOrDefault(candidate =>
                    !candidate.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase) &&
                    candidate.Source.Equals(option.Source, StringComparison.OrdinalIgnoreCase) &&
                    candidate.Pack != null &&
                    NormalizeToken(candidate.Title).StartsWith(baseToken, StringComparison.Ordinal) &&
                    (NormalizeToken(candidate.Title).Contains("boss", StringComparison.Ordinal) ||
                     NormalizeToken(candidate.Title).Contains("encounter", StringComparison.Ordinal)));
                if (sibling != null)
                {
                    if (GodotObject.IsInstanceValid(root))
                    {
                        root.QueueFree();
                    }

                    return CreatePackBackground(sibling, rng, allowSiblingFallback: false);
                }
            }

            var background = new NCombatBackground
            {
                Name = "SkinChangerLayeredBackground",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            background.AddChild(root);
            return background;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "pack:" + option.Id,
                $"{option.DisplayName} 背景创建失败，已保留原背景：" +
                exception.GetBaseException().Message);
            return null;
        }
    }

    private static Control InstantiateLayer(string path)
    {
        var scene = PreloadManager.Cache.GetScene(path);
        var layer = scene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
        if (layer.HasMethod("configure_for_sts2"))
        {
            var accepted = layer.Call("configure_for_sts2");
            if (accepted.VariantType == Variant.Type.Bool && !accepted.AsBool())
            {
                throw new InvalidOperationException("背景层拒绝了运行契约：" + path);
            }
        }

        layer.Visible = true;
        return layer;
    }

    private static string LayerGroupKey(string path, string marker)
    {
        var name = System.IO.Path.GetFileNameWithoutExtension(path);
        var index = name.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return name;
        }

        var rest = name[(index + marker.Length)..];
        var separator = rest.IndexOf('_');
        return separator > 0 ? rest[..separator] : rest;
    }

    private static void RefreshCurrentActIfNeeded(string actId)
    {
        try
        {
            if (!TryGetCurrentRunState(out var state))
            {
                return;
            }

            if (!NormalizeActId(state.Act.Id.Entry).Equals(actId, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (CombatManager.Instance is not { IsInProgress: true })
            {
                return;
            }

            var room = NCombatRoom.Instance;
            if (room == null || room.Background == null)
            {
                return;
            }

            var refreshEncounter = CurrentEncounter(room);
            if (!IsFakeMerchantEncounter(refreshEncounter) &&
                !CanApplyActBackground(room.Background, refreshEncounter))
            {
                // Changing the act background mid-fight must not free an encounter-owned
                // background, but swapping the backdrop behind it is safe and gives a live switch.
                if (!TryApplyEncounterBackdrop(
                        room.Background,
                        refreshEncounter,
                        actId,
                        GetSelection(actId),
                        actId,
                        out _))
                {
                    ReportOnce(
                        "encounter-refresh:" + actId,
                        $"{actId} 当前战斗背景由遭遇自带，无法把所选背景垫在它之后。");
                }

                return;
            }

            var container = room.GetNodeOrNull<Control>("%BgContainer");
            var old = room.Background;
            try
            {
                BackgroundProperty?.SetValue(room, null);
            }
            catch
            {
                // SetUpBackground overwrites the field even if clearing it is unavailable.
            }

            if (GodotObject.IsInstanceValid(old))
            {
                container?.RemoveChild(old);
                old.QueueFree();
            }

            room.SetUpBackground(state);
            ModLog.Info("已按新选择刷新当前战斗背景。");
        }
        catch (Exception exception)
        {
            ModLog.Warn("刷新当前战斗背景失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Discovery touches third-party PCKs and assemblies. Any failure here degrades to "no
    /// background switching" for that piece instead of taking the mod (or the game) down.
    /// </summary>
    private static List<BackgroundActEntry> SafeDiscoverActs()
    {
        try
        {
            return DiscoverActs();
        }
        catch (Exception exception)
        {
            ModLog.Warn("枚举背景地区失败，已跳过背景功能：" + exception.GetBaseException().Message);
            return [];
        }
    }

    private static List<BackgroundOption> SafeDiscoverBackgrounds()
    {
        try
        {
            return DiscoverBackgrounds();
        }
        catch (Exception exception)
        {
            ModLog.Warn("扫描背景失败，已跳过背景功能：" + exception.GetBaseException().Message);
            return [];
        }
    }

    private static List<BackgroundActEntry> DiscoverActs()
    {
        var backgrounds = Backgrounds;
        var entries = new Dictionary<string, BackgroundActEntry>(StringComparer.OrdinalIgnoreCase);
        var order = new List<string>();
        foreach (var act in EnumerateActs())
        {
            var id = NormalizeActId(act.Id.Entry);
            if (!entries.ContainsKey(id))
            {
                order.Add(id);
            }

            entries[id] = new BackgroundActEntry(id, ReadActTitle(act), CreateOptions(backgrounds));
        }

        foreach (var id in BaseActIds)
        {
            if (entries.ContainsKey(id))
            {
                continue;
            }

            order.Add(id);
            entries[id] = new BackgroundActEntry(id, id, CreateOptions(backgrounds));
        }

        return order.Select(id => entries[id]).ToList();
    }

    /// <summary>
    /// ModelDb.Acts only returns the four built-in acts. Content mods register additional
    /// ActModel subclasses in ModelDb, so enumerate those instead; fall back to ModelDb.Acts if
    /// the reflection path is unavailable.
    /// </summary>
    private static IEnumerable<ActModel> EnumerateActs() => ActCatalog.All;

    private static List<BackgroundOption> CreateOptions(IReadOnlyList<BackgroundOption> backgrounds)
    {
        var options = new List<BackgroundOption>
        {
            new(
                BackgroundSkinPolicy.FollowCategoryId,
                ModLocalization.Get(ModText.FollowCategory),
                string.Empty,
                string.Empty,
                null),
            new(
                SkinCatalog.BaseOptionId,
                ModLocalization.Get(ModText.GameDefault),
                GameSourceId,
                string.Empty,
                null),
            new(
                BackgroundSkinPolicy.ModDefaultId,
                ModLocalization.Get(ModText.BackgroundModDefault),
                string.Empty,
                string.Empty,
                null)
        };
        options.AddRange(backgrounds);
        return options;
    }

    private static IReadOnlyList<BackgroundOption> Backgrounds
    {
        get
        {
            lock (Sync)
            {
                return _backgrounds ??= SafeDiscoverBackgrounds();
            }
        }
    }

    public static void Invalidate()
    {
        lock (Sync)
        {
            _acts = null;
            _backgrounds = null;
        }
    }

    private static List<BackgroundOption> DiscoverBackgrounds()
    {
        var result = new List<BackgroundOption>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var actTitles = ReadActTitles();
        var actOrder = actTitles.Keys.ToList();

        // The fake merchant event picture, sourced from the pack that redrew it. Listed first so it
        // is easy to find and clearly not the game's own copy. The game's own tent scene stays in
        // the list as its own entry; this one is the event pack's full-frame illustration, which is
        // also what the event screen shows.
        void AddFakeMerchantEventFromPack()
        {
            foreach (var provider in SkinService.Catalog?.FindFakeMerchantEventArtProviders() ?? [])
            {
                if (!seen.Add(provider.Id + "::" + FakeMerchantEventOptionToken)) continue;
                var option = new BackgroundOption(
                    BackgroundSkinPolicy.ProviderOptionId(provider.Id, FakeMerchantEventOptionToken),
                    FakeMerchantEventBackgroundName + " · " + provider.Name,
                    provider.Id, SkinService.Catalog!.HasFakeMerchantEventImage(provider.Id)
                        ? FakeMerchantEventArtPolicy.EventImage : FakeMerchantEventArtPolicy.EncounterBackgroundScene, null);
                // A package resource is isolated on demand: global ResourceLoader.Exists cannot
                // determine availability before its private overlay has been mounted.
                if (TryProbeBackground(option)) result.Add(option);
            }
        }

        void AddGame(string title)
        {
            if (!GameBackgroundExists(title) ||
                !seen.Add(GameSourceId + "::" + title))
            {
                return;
            }

            result.Add(new BackgroundOption(
                BackgroundSkinPolicy.ProviderOptionId(GameSourceId, title),
                actTitles.TryGetValue(title, out var localized)
                    ? localized
                    : BackgroundSkinPolicy.DisplayName(title),
                GameSourceId,
                title,
                null));
        }

        AddFakeMerchantEventFromPack();

        // The game's own act backgrounds first, in act order, then its special encounter scenes.
        var gameTitles = ListGameBackgroundTitles();
        foreach (var actId in actOrder.Concat(gameTitles))
        {
            AddGame(actId);
        }

        foreach (var title in gameTitles.OrderBy(title => title, StringComparer.OrdinalIgnoreCase))
        {
            AddGame(title);
        }

        // Content mods can add canonical scene folders after the engine's directory listing was
        // built, and many ship battle backgrounds as plain images. Scan every loaded mod package
        // for both formats.
        var modResources = DiscoverModBackgroundResources();
        foreach (var title in modResources.Titles)
        {
            var titleToken = NormalizeToken(title);
            var titleBase = BaseTokenOf(titleToken);
            if ((titleToken.EndsWith("act", StringComparison.Ordinal) ||
                 titleToken.EndsWith("stage", StringComparison.Ordinal)) &&
                modResources.Shells.Any(shell =>
                    NormalizeToken(shell.Shell.Title).StartsWith(titleBase, StringComparison.Ordinal) &&
                    (NormalizeToken(shell.Shell.Title).Contains("boss", StringComparison.Ordinal) ||
                     NormalizeToken(shell.Shell.Title).Contains("encounter", StringComparison.Ordinal))))
            {
                continue;
            }

            AddGame(title);
        }

        foreach (var image in modResources.Images
                     .OrderBy(image => image.ModName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(image => image.Path, StringComparer.OrdinalIgnoreCase))
        {
            if (!seen.Add("image::" + image.Path))
            {
                continue;
            }

            result.Add(new BackgroundOption(
                BackgroundSkinPolicy.ProviderOptionId(image.ModId, image.Path),
                image.ModName + " · " +
                BackgroundSkinPolicy.DisplayName(
                    System.IO.Path.GetFileNameWithoutExtension(image.Path)),
                image.ModId,
                image.Path,
                null));
        }

        // Layered battlefields shipped by gameplay mods that are not skin providers (Library,
        // Arknights, Heathcliff Wild Hunt, …). Their scenes may live under <mod>/scenes/backgrounds
        // or be exported as .tscn.remap, both already normalised by the scanner.
        foreach (var entry in modResources.Shells
                     .OrderBy(shell => shell.ModName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(shell => shell.Shell.Title, StringComparer.OrdinalIgnoreCase))
        {
            if (!seen.Add(entry.ModId + "::" + entry.Shell.Title))
            {
                continue;
            }

            // Skip an empty "<act>_act" stub when the same package also ships a populated
            // boss/encounter background for it; listing both only produces a transparent option.
            var shellToken = NormalizeToken(entry.Shell.Title);
            var baseToken = BaseTokenOf(shellToken);
            if ((shellToken.EndsWith("act", StringComparison.Ordinal) ||
                 shellToken.EndsWith("stage", StringComparison.Ordinal)) &&
                modResources.Shells.Any(other =>
                    other.ModId.Equals(entry.ModId, StringComparison.OrdinalIgnoreCase) &&
                    !other.Shell.Title.Equals(entry.Shell.Title, StringComparison.OrdinalIgnoreCase) &&
                    NormalizeToken(other.Shell.Title).StartsWith(baseToken, StringComparison.Ordinal) &&
                    (NormalizeToken(other.Shell.Title).Contains("boss", StringComparison.Ordinal) ||
                     NormalizeToken(other.Shell.Title).Contains("encounter", StringComparison.Ordinal))))
            {
                continue;
            }

            result.Add(new BackgroundOption(
                BackgroundSkinPolicy.ProviderOptionId(entry.ModId, entry.Shell.Title),
                entry.ModName + " · " +
                BackgroundSkinPolicy.DisplayName(entry.Shell.Title),
                entry.ModId,
                entry.Shell.Title,
                entry.Shell));
        }

        foreach (var scene in modResources.Scenes
                     .OrderBy(scene => scene.ModName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(scene => scene.Path, StringComparer.OrdinalIgnoreCase))
        {
            var stem = SceneStem(scene.Path);
            var image = modResources.Images.FirstOrDefault(candidate =>
                SceneStem(candidate.Path).Equals(stem, StringComparison.OrdinalIgnoreCase));
            if (image == null &&
                scene.Path.Contains("/images/background/", StringComparison.OrdinalIgnoreCase))
            {
                // Ruina2-style *_bg.tscn files are empty shells. Without a paired image there is
                // nothing to render, so do not expose a guaranteed-blank option.
                continue;
            }

            var optionPath = image?.Path ?? scene.Path;
            if (!seen.Add("scene::" + optionPath))
            {
                continue;
            }

            result.Add(new BackgroundOption(
                BackgroundSkinPolicy.ProviderOptionId(scene.ModId, optionPath),
                scene.ModName + " · " +
                BackgroundSkinPolicy.DisplayName(
                    System.IO.Path.GetFileNameWithoutExtension(optionPath)),
                scene.ModId,
                optionPath,
                null));
        }

        var declaredScenes = DiscoverDeclaredSceneBackgrounds().ToArray();
        foreach (var scene in declaredScenes)
        {
            var optionId = BackgroundSkinPolicy.ProviderOptionId(scene.Source, scene.ScenePath);
            if (!seen.Add("scene::" + scene.ScenePath))
            {
                continue;
            }

            var sceneToken = NormalizeToken(
                System.IO.Path.GetFileNameWithoutExtension(scene.ScenePath));
            var sceneBase = BaseTokenOf(sceneToken);
            if ((sceneToken.EndsWith("act", StringComparison.Ordinal) ||
                 sceneToken.EndsWith("stage", StringComparison.Ordinal)) &&
                declaredScenes.Any(other =>
                    other.Source.Equals(scene.Source, StringComparison.OrdinalIgnoreCase) &&
                    !other.ScenePath.Equals(scene.ScenePath, StringComparison.OrdinalIgnoreCase) &&
                    NormalizeToken(System.IO.Path.GetFileNameWithoutExtension(other.ScenePath))
                        .StartsWith(sceneBase, StringComparison.Ordinal) &&
                    (NormalizeToken(other.ScenePath).Contains("boss", StringComparison.Ordinal) ||
                     NormalizeToken(other.ScenePath).Contains("encounter", StringComparison.Ordinal))))
            {
                continue;
            }

            result.Add(new BackgroundOption(
                optionId,
                scene.SourceDisplay + " · " +
                BackgroundSkinPolicy.DisplayName(
                    System.IO.Path.GetFileNameWithoutExtension(scene.ScenePath)),
                scene.Source,
                scene.ScenePath,
                null));
        }

        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return result;
        }

        foreach (var candidate in catalog.FindBackgroundPackCandidates()
                     .OrderBy(candidate => candidate.DisplayName, StringComparer.OrdinalIgnoreCase))
        {
            foreach (var shell in candidate.Shells
                         .OrderBy(shell => shell.Title, StringComparer.OrdinalIgnoreCase))
            {
                if (!seen.Add(candidate.ProviderId + "::" + shell.Title))
                {
                    continue;
                }

                result.Add(new BackgroundOption(
                    BackgroundSkinPolicy.ProviderOptionId(candidate.ProviderId, shell.Title),
                    candidate.DisplayName + " · " +
                    BackgroundSkinPolicy.DisplayName(shell.Title),
                    candidate.ProviderId,
                    shell.Title,
                    shell));
            }
        }

        _providerActDefaults = DiscoverProviderActDefaults(result);
        ModLog.Info(
            $"背景资源扫描完成：规范场景={modResources.Titles.Count}，" +
            $"背景图={modResources.Images.Count}，可选背景总数={result.Count}。");
        return result;
    }

    private sealed record ModBackgroundImage(string ModId, string ModName, string Path);

    private sealed record ModBackgroundShell(
        string ModId,
        string ModName,
        BackgroundPackShell Shell);

    private sealed record ModBackgroundScene(
        string ModId,
        string ModName,
        string Path);

    private sealed record ModBackgroundResources(
        IReadOnlyList<string> Titles,
        IReadOnlyList<ModBackgroundImage> Images,
        IReadOnlyList<ModBackgroundShell> Shells,
        IReadOnlyList<ModBackgroundScene> Scenes);

    private static ModBackgroundResources DiscoverModBackgroundResources()
    {
        var titles = new List<string>();
        var images = new List<ModBackgroundImage>();
        var shells = new List<ModBackgroundShell>();
        var scenes = new List<ModBackgroundScene>();
        foreach (var mod in MegaCrit.Sts2.Core.Modding.ModManager.Mods)
        {
            string? pckPath;
            try
            {
                if (mod.manifest is not { id: not null } manifest ||
                    string.IsNullOrWhiteSpace(mod.path))
                {
                    continue;
                }

                pckPath = SkinPackagePaths.Resolve(mod.path, manifest.id, ".pck");
            }
            catch
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(pckPath) || !File.Exists(pckPath))
            {
                continue;
            }

            try
            {
                var modImages = new List<ModBackgroundImage>();
                var hasLayeredScenes = false;
                using var archive = PckArchive.Open(pckPath);
                var paths = archive.Paths.ToArray();
                foreach (var path in paths)
                {
                    if (SkinCatalog.IsScenePath(path, "_background.tscn") &&
                        SkinCatalog.IsBattleBackgroundLocation(path))
                    {
                        hasLayeredScenes = true;
                        var sourcePath = SkinCatalog.StripRemap(path);
                        var title = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
                        if (title.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
                        {
                            title = title[..^"_background".Length];
                        }

                        if (sourcePath.StartsWith("res://scenes/backgrounds/", StringComparison.OrdinalIgnoreCase))
                        {
                            titles.Add(title);
                        }

                        var directory = sourcePath[..sourcePath.LastIndexOf('/')];
                        var layers = paths
                            .Where(candidate => candidate.StartsWith(
                                    directory + "/layers/",
                                    StringComparison.OrdinalIgnoreCase) &&
                                SkinCatalog.IsScenePath(candidate, ".tscn"))
                            .Select(SkinCatalog.StripRemap)
                            .ToArray();
                        if (layers.Length > 0)
                        {
                            shells.Add(new ModBackgroundShell(
                                mod.manifest!.id!,
                                mod.manifest.name ?? mod.manifest.id!,
                                new BackgroundPackShell(title, sourcePath, layers)));
                        }

                        continue;
                    }

                    // Standalone battle scenes such as Ruina2/images/background/*_bg.tscn.
                    if (SkinCatalog.IsScenePath(path, ".tscn") &&
                        path.Contains("/images/background/", StringComparison.OrdinalIgnoreCase) &&
                        !path.Contains("/rest/", StringComparison.OrdinalIgnoreCase) &&
                        !path.Contains("rest_site", StringComparison.OrdinalIgnoreCase))
                    {
                        scenes.Add(new ModBackgroundScene(
                            mod.manifest!.id!,
                            mod.manifest.name ?? mod.manifest.id!,
                            SkinCatalog.StripRemap(path)));
                        continue;
                    }

                    var imagePath = ResolveBackgroundImageSource(path);
                    if (imagePath != null)
                    {
                        modImages.Add(new ModBackgroundImage(
                            mod.manifest!.id!,
                            mod.manifest.name ?? mod.manifest.id!,
                            imagePath));
                    }
                }

                // A package that also ships complete layered scenes uses loose images only as
                // layer pieces (ground/foreground/body). Listing them separately produces empty
                // looking options, so the complete scenes are authoritative.
                if (!hasLayeredScenes)
                {
                    images.AddRange(modImages);
                }
                else
                {
                    // Keep singular images/background assets: BaseLib encounter layers live
                    // there and pair with images/background/*_bg.tscn shells. Only the plural
                    // images/backgrounds layer fragments are redundant with layered scenes.
                    images.AddRange(modImages.Where(image =>
                        !image.Path.Contains("/images/backgrounds/", StringComparison.OrdinalIgnoreCase)));
                }
            }
            catch
            {
                // A package that cannot be read contributes nothing.
            }
        }

        return new ModBackgroundResources(
            titles.Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
            images
                .DistinctBy(image => image.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            shells
                .DistinctBy(entry => entry.Shell.ShellPath, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            scenes
                .DistinctBy(scene => scene.Path, StringComparer.OrdinalIgnoreCase)
                .ToArray());
    }

    /// <summary>
    /// Exported packages normally keep only the import record (<c>foo.png.import</c>) plus the
    /// compiled <c>.ctex</c>; the source <c>foo.png</c> path exists logically and ResourceLoader
    /// resolves it through the import record. Strip the suffix so the texture can be loaded.
    /// </summary>
    private static string? ResolveBackgroundImageSource(string path)
    {
        var candidate = path.EndsWith(".import", StringComparison.OrdinalIgnoreCase)
            ? path[..^".import".Length]
            : path;
        return IsBackgroundImagePath(candidate) ? candidate : null;
    }

    private static bool IsBackgroundImagePath(string path)
    {
        var extension = System.IO.Path.GetExtension(path);
        if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) &&
            !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var lower = path.ToLowerInvariant();
        // Only battle-background artwork. Ancient/merchant/event art lives under
        // animations/backgrounds, scenes/events or images/events and must stay out of the list.
        if (!lower.Contains("/images/backgrounds/", StringComparison.Ordinal) &&
            !lower.Contains("/images/background/", StringComparison.Ordinal))
        {
            return false;
        }

        foreach (var excluded in BackgroundImageExcludedSegments)
        {
            if (lower.Contains(excluded, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static readonly string[] BackgroundImageExcludedSegments =
    [
        "/animations/",
        "/events/",
        "/event/",
        "merchant",
        "ancient",
        "neow",
        "/ui/",
        "map_bgs",
        "/map/",
        "icon",
        "portrait",
        "card",
        "/rest/",
        "rest_site"
    ];

    private static IEnumerable<(string Source, string ScenePath, string SourceDisplay)>
        DiscoverDeclaredSceneBackgrounds()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string location;
            try
            {
                if (assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
                {
                    continue;
                }

                location = assembly.Location;
            }
            catch
            {
                continue;
            }

            var directory = System.IO.Path.GetDirectoryName(location) ?? string.Empty;
            if (ReferenceEquals(assembly, typeof(BackgroundSkinService).Assembly) ||
                !(directory.Contains("\\mods\\", StringComparison.OrdinalIgnoreCase) ||
                  directory.Contains("\\workshop\\content\\", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            var source = assembly.GetName().Name ?? "mod";
            var sourceDisplay = System.IO.Path.GetFileNameWithoutExtension(location);
            foreach (var type in types.Where(type =>
                         type.Name.Contains("Background", StringComparison.OrdinalIgnoreCase)))
            {
                foreach (var field in type.GetFields(
                             BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    if (field.FieldType != typeof(string[]) ||
                        field.GetValue(null) is not string[] values)
                    {
                        continue;
                    }

                    foreach (var value in values)
                    {
                        if (string.IsNullOrWhiteSpace(value) ||
                            !value.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                            !value.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) ||
                            !seen.Add(value) ||
                            !ResourceLoader.Exists(value))
                        {
                            continue;
                        }

                        yield return (source, value, sourceDisplay);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Maps a pack's acts to its own default battlefield (the standard/regular battle look) so
    /// acts that only exist in that pack can preview "follow category / mod default / game
    /// default" instead of showing an empty screen.
    /// </summary>
    private static Dictionary<string, string> DiscoverProviderActDefaults(
        IReadOnlyList<BackgroundOption> backgrounds)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var standard = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var catalog = SkinService.Catalog;
        if (catalog == null)
        {
            return result;
        }

        const BindingFlags flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        foreach (var candidate in catalog.FindBackgroundPackCandidates())
        {
            if (!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(
                    candidate.ProviderId,
                    out var assembly))
            {
                continue;
            }

            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                types = exception.Types.Where(type => type != null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types.Where(type =>
                         type.Name.Contains("Profile", StringComparison.OrdinalIgnoreCase)))
            {
                var property = type.GetProperty("Profiles", flags);
                if (property == null || property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                System.Collections.IEnumerable elements;
                try
                {
                    if (property.GetValue(null) is not System.Collections.IEnumerable value)
                    {
                        continue;
                    }

                    elements = value;
                }
                catch
                {
                    continue;
                }

                foreach (var element in elements)
                {
                    if (element == null)
                    {
                        continue;
                    }

                    var elementType = element.GetType();
                    var target = elementType.GetProperty("Target", flags)?.GetValue(element);
                    var actId = target?.GetType().GetProperty("ActId", flags)?.GetValue(target) as string;
                    if (string.IsNullOrWhiteSpace(actId))
                    {
                        continue;
                    }

                    var battlefield = elementType.GetProperty("Battlefield", flags)?.GetValue(element);
                    var shell = battlefield == null ? null : ReadBackgroundShellName(battlefield, flags);
                    if (string.IsNullOrWhiteSpace(shell))
                    {
                        continue;
                    }

                    var optionId = BackgroundSkinPolicy.ProviderOptionId(
                        candidate.ProviderId,
                        shell!);
                    if (!backgrounds.Any(option => option.Id.Equals(
                            optionId,
                            StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var normalizedAct = NormalizeActId(actId!);
                    var isStandard = IsStandardCombatScope(target, flags);
                    if (result.ContainsKey(normalizedAct) &&
                        (standard[normalizedAct] || !isStandard))
                    {
                        continue;
                    }

                    result[normalizedAct] = optionId;
                    standard[normalizedAct] = isStandard;
                }
            }
        }

        return result;
    }

    private static bool IsStandardCombatScope(object? target, BindingFlags flags)
    {
        try
        {
            var scope = target?.GetType().GetProperty("RoomScope", flags)?.GetValue(target);
            return scope != null &&
                   scope.ToString()?.Equals("StandardCombat", StringComparison.OrdinalIgnoreCase) == true;
        }
        catch
        {
            return false;
        }
    }

    private static string? ReadBackgroundShellName(object battlefield, BindingFlags flags)
    {
        try
        {
            var background = battlefield.GetType().GetProperty("Background", flags)
                ?.GetValue(battlefield);
            var shellPath = background?.GetType().GetProperty("ShellScenePath", flags)
                ?.GetValue(background) as string;
            if (string.IsNullOrWhiteSpace(shellPath))
            {
                return null;
            }

            var name = System.IO.Path.GetFileNameWithoutExtension(shellPath);
            const string suffix = "_background";
            return name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                ? name[..^suffix.Length]
                : name;
        }
        catch
        {
            return null;
        }
    }

    private static BackgroundOption? FindBackground(string id)
    {
        var exact = Backgrounds.FirstOrDefault(option =>
            option.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (exact != null)
        {
            return exact;
        }

        // A saved selection can carry a slightly different provider prefix (the same artwork is
        // declared by more than one package). Match the resource part as a fallback.
        if (!BackgroundSkinPolicy.TryParseProviderOptionId(id, out _, out var title))
        {
            return null;
        }

        return Backgrounds.FirstOrDefault(option =>
            option.Title.Equals(title, StringComparison.OrdinalIgnoreCase) ||
            option.Title.EndsWith(title, StringComparison.OrdinalIgnoreCase));
    }

    private const string FakeMerchantEncounterId = "fake_merchant_event_encounter";

    /// <summary>Player-facing name of the fake merchant event picture in the background list.</summary>
    internal const string FakeMerchantEventBackgroundName = "假商人事件背景 / Fake Merchant Event";

    private const string FakeMerchantEventOptionToken = "fake_merchant_event";

    private static bool IsFakeMerchantEncounter(EncounterModel? encounter)
    {
        try
        {
            return encounter != null && encounter.Id.Entry.Equals(
                FakeMerchantEncounterId,
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// The game marks encounters that bring their own battlefield with
    /// <c>EncounterModel.HasCustomBackground</c>, and such a node is part of the encounter rather
    /// than scenery the act owns. The Emperor Crab is the extreme case: its whole body is an
    /// <c>NKaiserCrabBossBackground</c> node <b>inside</b> that background, and both claws
    /// (Rocket/Crusher) cache the node in their own <c>_background</c> field, so freeing it deletes
    /// the boss and leaves the claws holding a disposed object - which made
    /// <c>Crusher.AfterAddedToRoom</c> throw inside <c>StartCombatInternal</c> and the fight never
    /// started (no body, no cards, frozen game).
    /// </summary>
    private static bool EncounterOwnsBackground(EncounterModel? encounter)
    {
        try
        {
            return encounter != null && HasCustomBackgroundProperty?.GetValue(encounter) is true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// A background may only be replaced when it is plain scenery: not a specialised background
    /// class, and not the battlefield its own encounter declared. The fake merchant's tent is the
    /// one deliberate exception and is handled by its own branches before this check.
    /// </summary>
    private static bool CanApplyActBackground(NCombatBackground? background, EncounterModel? encounter) =>
        (background == null ||
         !GodotObject.IsInstanceValid(background) ||
         background.GetType() == typeof(NCombatBackground)) &&
        !EncounterOwnsBackground(encounter);

    /// <summary>
    /// The encounter the room is showing. <see cref="NCombatRoom"/> keeps it in the private
    /// <c>_visuals</c> field; <see cref="NCombatRoom.Background"/> alone cannot tell us whether the
    /// node belongs to the encounter or to the act.
    /// </summary>
    private static EncounterModel? CurrentEncounter(NCombatRoom room)
    {
        // Public path first: the combat state owns the authoritative encounter in a fight.
        try
        {
            if (CombatManager.Instance?.DebugOnlyGetState()?.Encounter is { } combatEncounter)
            {
                return combatEncounter;
            }
        }
        catch
        {
            // Fall through to the room visuals below.
        }

        try
        {
            if (CombatRoomVisualsField == null)
            {
                ReportOnce(
                    "combat-visuals-field",
                    "找不到 NCombatRoom._visuals，无法判断战斗背景归属；自带背景的遭遇可能被地区背景替换。");
                return null;
            }

            return (CombatRoomVisualsField.GetValue(room) as ICombatRoomVisuals)?.Encounter;
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Re-skins an encounter-owned background by putting the chosen battlefield *behind* it instead
    /// of replacing or gutting it. Pack battlefields are authored to live in their own negative
    /// CanvasLayer (CZN uses layer -1 "so it can never compete with combat actors") and order their
    /// own parts with large positive/negative z_index values; lifting their layer scenes into the
    /// encounter's canvas-0 Layer_XX slots made the sky (z_index 2400+) draw over the boss body.
    /// Keeping the whole node intact preserves that contract: the battlefield stays below everything,
    /// the boss stays above it, and only the encounter's own scenery layers are hidden so they do
    /// not cover the new arena.
    /// </summary>
    private static bool TryApplyEncounterBackdrop(
        NCombatBackground? background,
        EncounterModel? encounter,
        string actId,
        string selection,
        string actLabel,
        out bool handled)
    {
        handled = false;
        if (background == null || !GodotObject.IsInstanceValid(background))
        {
            return false;
        }

        var option = ResolveActBattlefieldOption(actId, selection);
        if (option == null)
        {
            // "游戏默认" (or nothing the act has to offer) means: show the encounter's own arena.
            if (ClearEncounterBackdrop(background))
            {
                ModLog.Info($"{actLabel} 战斗背景已恢复为遭遇自带。");
            }

            handled = true;
            return true;
        }

        var current = background.HasMeta(EncounterBackdropOptionMeta)
            ? background.GetMeta(EncounterBackdropOptionMeta).AsString()
            : null;
        if (current != null && current.Equals(option.Id, StringComparison.OrdinalIgnoreCase))
        {
            handled = true;
            return true;
        }

        var rng = CreatePreviewRng();
        if (rng == null || !AttachEncounterBackdrop(background, option, rng, encounter))
        {
            return false;
        }

        handled = true;
        ModLog.Info($"{actLabel} 战斗背景由遭遇自带，已把所选战场垫在本体之后：{option.DisplayName}。");
        return true;
    }

    private static bool AttachEncounterBackdrop(
        NCombatBackground background,
        BackgroundOption option,
        Rng rng,
        EncounterModel? encounter)
    {
        try
        {
            ClearEncounterBackdrop(background);

            var chosen = CreateOptionBackground(option, rng);
            if (chosen == null || !GodotObject.IsInstanceValid(chosen))
            {
                return false;
            }

            chosen.Name = EncounterBackdropName;
            chosen.MouseFilter = Control.MouseFilterEnum.Ignore;
            background.AddChild(chosen);
            // First child: draws below the encounter's own nodes (its CanvasLayer decides when the
            // pack uses one, so this is only a fallback ordering for plain game backgrounds).
            background.MoveChild(chosen, 0);
            chosen.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);

            // A battlefield that lives on canvas 0 is drawn through the encounter's Camera2D, so it
            // shrinks by GetCameraScaling() (the Emperor Crab uses 0.75, most other bosses 0.85-0.9):
            // a 1920x1080 picture then only covers three quarters of the screen. Compensate so it
            // still fills the view. CanvasLayer-based packs (CZN puts its battlefield on layer -1)
            // ignore the camera entirely and must not be scaled.
            var scaling = 1f;
            try
            {
                if (encounter != null)
                {
                    scaling = encounter.GetCameraScaling();
                }
            }
            catch
            {
                scaling = 1f;
            }

            if (float.IsFinite(scaling) && scaling > 0.01f &&
                Math.Abs(scaling - 1f) > 0.001f &&
                !ContainsCanvasLayer(chosen))
            {
                chosen.Scale = Vector2.One / scaling;
            }

            // Hide the arena art the encounter shipped with; the boss body and its foreground stay.
            foreach (var slot in background.GetChildren().OfType<Node>().Where(IsLayerSlot))
            {
                if (slot is CanvasItem canvasItem)
                {
                    canvasItem.Visible = false;
                }
            }

            background.SetMeta(EncounterBackdropOptionMeta, option.Id);
            return true;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "encounter-backdrop:" + option.Id,
                $"叠加 {option.DisplayName} 战场失败：" + exception.GetBaseException().Message);
            return false;
        }
    }

    /// <summary>
    /// Drops the grafted battlefield and makes the encounter's own scenery visible again.
    /// </summary>
    private static bool ClearEncounterBackdrop(NCombatBackground background)
    {
        var changed = false;

        var previous = background.GetNodeOrNull<Node>(EncounterBackdropName);
        if (previous != null)
        {
            background.RemoveChild(previous);
            previous.QueueFree();
            changed = true;
        }

        if (background.HasMeta(EncounterBackdropOptionMeta))
        {
            background.RemoveMeta(EncounterBackdropOptionMeta);
            changed = true;
        }

        foreach (var slot in background.GetChildren().OfType<Node>().Where(IsLayerSlot))
        {
            if (slot is CanvasItem canvasItem && !canvasItem.Visible)
            {
                canvasItem.Visible = true;
                changed = true;
            }
        }

        return changed;
    }

    private static bool IsLayerSlot(Node node) =>
        node.Name.ToString().StartsWith("Layer_", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsCanvasLayer(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is CanvasLayer || ContainsCanvasLayer(child))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// The battlefield the player asked for, resolved the way <see cref="ApplyToResult"/> reads it.
    /// "游戏默认" means the encounter's own background, so it deliberately returns null (nothing to
    /// graft) instead of the act's art.
    /// </summary>
    private static BackgroundOption? ResolveActBattlefieldOption(string actId, string selection)
    {
        try
        {
            if (BackgroundSkinPolicy.IsFollowCategory(selection) ||
                selection.Equals(BackgroundSkinPolicy.ModDefaultId, StringComparison.OrdinalIgnoreCase))
            {
                return _providerActDefaults.TryGetValue(actId, out var providerId) &&
                       FindBackground(providerId) is { } providerOption &&
                       !providerOption.Source.Equals(GameSourceId, StringComparison.OrdinalIgnoreCase)
                    ? providerOption
                    : null;
            }

            if (selection.Equals(SkinCatalog.BaseOptionId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return FindBackground(selection);
        }
        catch (Exception exception)
        {
            ReportOnce(
                "graft-option:" + actId,
                "解析可叠加的背景失败：" + exception.GetBaseException().Message);
            return null;
        }
    }

    private static bool IsFakeMerchantCombat(NCombatRoom room)
    {
        try
        {
            var background = room.Background;
            return background != null &&
                   GodotObject.IsInstanceValid(background) &&
                   background.SceneFilePath.Contains(
                       "fake_merchant",
                       StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// The fight's background for "跟随分类"/"模组默认": the event pack's own picture, built exactly
    /// like the "假商人事件背景" entry in the background list so the fight shows the same artwork the
    /// event shows. Returns false (leaving <paramref name="result"/> untouched) when no event pack
    /// supplies that picture.
    /// </summary>
    private static bool TryUseFakeMerchantEventImageBackground(ref NCombatBackground result)
    {
        try
        {
            var provider = SkinService.Catalog?.FindFakeMerchantEventArtProvider();
            if (string.IsNullOrWhiteSpace(provider))
            {
                return false;
            }

            var id = BackgroundSkinPolicy.ProviderOptionId(provider, FakeMerchantEventOptionToken);
            var option = FindBackground(id) ??
                         new BackgroundOption(
                             id,
                             FakeMerchantEventBackgroundName,
                             provider,
                             SkinService.Catalog!.HasFakeMerchantEventImage(provider)
                                 ? FakeMerchantEventArtPolicy.EventImage : FakeMerchantEventArtPolicy.EncounterBackgroundScene,
                             null);
            var replacement = CreateOptionBackground(option, CreatePreviewRng()!);
            if (replacement == null || !GodotObject.IsInstanceValid(replacement))
            {
                return false;
            }

            if (result != null &&
                GodotObject.IsInstanceValid(result) &&
                !ReferenceEquals(result, replacement))
            {
                result.Free();
            }

            replacement.SetMeta("sc_background_applied", true);
            result = replacement;
            ModLog.Info($"假商人战斗背景来源=事件图（{option.DisplayName}）。");
            return true;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "fake-merchant-event-image",
                "构建假商人事件背景失败，已保留遭遇自带背景：" +
                exception.GetBaseException().Message);
            return false;
        }
    }

    /// <summary>
    /// Builds the fight's background from the game's own copy of the encounter scene (the private
    /// baseline alias) so "游戏默认" keeps the vanilla tent even while the event pack's redrawn
    /// tent is mounted on the canonical path. Returns false — leaving <paramref name="result"/>
    /// untouched — when no isolated copy exists for the current selection.
    /// </summary>
    private static bool TryUseFakeMerchantVanillaBackground(ref NCombatBackground result)
    {
        try
        {
            var alias = SkinService.TryGetIsolatedRuntimeResourcePath(
                FakeMerchantEventArtPolicy.MonsterGroupId,
                FakeMerchantEventArtPolicy.EncounterBackgroundScene);
            if (string.IsNullOrWhiteSpace(alias))
            {
                return false;
            }

            var replacement = CreateSceneBackground(
                new BackgroundOption(
                    "sc_fake_merchant_vanilla",
                    "game default",
                    GameSourceId,
                    alias,
                    null),
                allowSiblingFallback: false);
            if (replacement == null || !GodotObject.IsInstanceValid(replacement))
            {
                return false;
            }

            if (result != null &&
                GodotObject.IsInstanceValid(result) &&
                !ReferenceEquals(result, replacement))
            {
                result.Free();
            }

            replacement.SetMeta("sc_background_applied", true);
            result = replacement;
            ModLog.Info("假商人战斗背景来源=游戏默认（原版帐篷副本）。");
            return true;
        }
        catch (Exception exception)
        {
            ReportOnce(
                "fake-merchant-vanilla",
                "构建假商人原版战斗背景失败，已保留遭遇自带背景：" +
                exception.GetBaseException().Message);
            return false;
        }
    }

    private static bool GameBackgroundExists(string title) =>
        !string.IsNullOrWhiteSpace(title) &&
        ResourceLoader.Exists($"res://scenes/backgrounds/{title}/{title}_background.tscn");

    private static List<string> ListGameBackgroundTitles()
    {
        var titles = new List<string>();
        try
        {
            using var directory = DirAccess.Open("res://scenes/backgrounds");
            if (directory == null)
            {
                return titles;
            }

            directory.ListDirBegin();
            while (true)
            {
                var name = directory.GetNext();
                if (string.IsNullOrEmpty(name))
                {
                    break;
                }

                if (name.StartsWith(".", StringComparison.Ordinal) || !directory.CurrentIsDir())
                {
                    continue;
                }

                if (GameBackgroundExists(name))
                {
                    titles.Add(name);
                }
            }

            directory.ListDirEnd();
        }
        catch (Exception exception)
        {
            ReportOnce("list-backgrounds", "列出游戏背景失败：" + exception.GetBaseException().Message);
        }

        return titles;
    }

    private static Dictionary<string, string> ReadActTitles()
    {
        var titles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var act in EnumerateActs())
            {
                var id = NormalizeActId(act.Id.Entry);
                if (!titles.ContainsKey(id))
                {
                    titles[id] = ReadActTitle(act);
                }
            }
        }
        catch (Exception exception)
        {
            ReportOnce("act-titles", "读取地区名称失败：" + exception.GetBaseException().Message);
        }

        return titles;
    }

    private static string ReadActTitle(ActModel act)
    {
        try
        {
            var title = act.Title.GetFormattedText();
            return string.IsNullOrWhiteSpace(title) ? act.Id.Entry : title;
        }
        catch
        {
            return act.Id.Entry;
        }
    }

    /// <summary>
    /// In combat the combat state owns the authoritative run state; on the map (or before the
    /// first room) fall back to the run node's own state field.
    /// </summary>
    private static bool TryGetCurrentRunState(out IRunState runState)
    {
        try
        {
            var run = NRun.Instance;
            if (run != null && RunStateField?.GetValue(run) is IRunState state)
            {
                runState = state;
                return true;
            }
        }
        catch
        {
            // Fall through to the combat state below.
        }

        try
        {
            if (CombatManager.Instance?.DebugOnlyGetState() is { RunState: { } combatRun })
            {
                runState = combatRun;
                return true;
            }
        }
        catch
        {
            // No run is active.
        }

        runState = null!;
        return false;
    }

    private static string NormalizeActId(string actId) => actId.Trim().ToLowerInvariant();

    private static void ReportOnce(string key, string message)
    {
        if (ReportedFailures.Add(key))
        {
            ModLog.Warn(message);
        }
    }
}

[FeaturePatch(FeatureIds.Background)]
[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom.SetUpBackground))]
internal static class BackgroundSkinRoomPatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NCombatRoom __instance, IRunState state) =>
        BackgroundSkinService.ApplyToRoom(__instance, state);
}

/// <summary>
/// The fake merchant fight instantiates the tent scene from scratch, exactly like the event does.
/// The pack that redrew the tent must therefore be on top before that scene binds its skeleton,
/// otherwise the fight keeps drawing the vanilla tent even though the event shows the new one.
/// </summary>
[FeaturePatch(FeatureIds.Background)]
[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.CreateBackground))]
internal static class FakeMerchantCombatArtPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix(EncounterModel __instance)
    {
        if (!__instance.Id.Entry.Equals(
                "fake_merchant_event_encounter",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SkinService.MountFakeMerchantEventArt();
    }
}

[FeaturePatch(FeatureIds.Background)]
[HarmonyPatch(typeof(EncounterModel), nameof(EncounterModel.CreateBackground))]
internal static class BackgroundSkinCreatePatch
{
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(
        EncounterModel __instance,
        ActModel parentAct,
        Rng rng,
        ref NCombatBackground __result) =>
        BackgroundSkinService.ApplyToResult(__instance, parentAct, rng, ref __result);
}


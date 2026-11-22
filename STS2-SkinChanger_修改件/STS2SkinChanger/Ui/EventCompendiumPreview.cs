using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Events;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// The native layout and native option visuals are reused, but no EventModel.BeginEvent,
// OnRoomEnter, option callback, room singleton or player save is involved in this viewer.
internal static class EventCompendiumPreview
{
    private static readonly ConditionalWeakTable<Node, object> PreviewNodes = new();
    public static bool IsPreview(Node node) => PreviewNodes.TryGetValue(node, out _);

    public static EventModel[] Events()
    {
        var events = new List<EventModel>();
        try
        {
            foreach (var type in ModelDb.AllAbstractModelSubtypes)
            {
                if (type.IsAbstract ||
                    type.IsGenericTypeDefinition ||
                    !typeof(EventModel).IsAssignableFrom(type))
                {
                    continue;
                }

                try
                {
                    var id = ModelDb.GetId(type);
                    if (ModelDb.GetByIdOrNull<EventModel>(id) is { } model)
                    {
                        events.Add(model);
                    }
                }
                catch
                {
                    // Skip types that cannot be materialised.
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("枚举事件失败：" + exception.GetBaseException().Message);
        }

        if (events.Count == 0)
        {
            events.AddRange(ModelDb.AllEvents);
        }

        return events
            .Where(model => model is not AncientEventModel && model.Id.Entry != "FAKE_MERCHANT")
            .DistinctBy(model => model.Id)
            .OrderBy(model => Title(model), StringComparer.CurrentCulture)
            .ToArray();
    }

    private static string? ReadCustomString(EventModel model, string propertyName)
    {
        try
        {
            return model.GetType().GetProperty(propertyName)?.GetValue(model) as string;
        }
        catch
        {
            return null;
        }
    }

    public static string Title(EventModel model)
    {
        var title = Format(model, model.Id.Entry + ".title");
        // Debug/test events sometimes ship without a localization title. Showing the id is more
        // useful than an empty row.
        return string.IsNullOrWhiteSpace(title) ? model.Id.Entry : title;
    }

    public static Control Create(EventModel model, string? selectedPage = null)
    {
        var groupId = EventSkinPolicy.GroupId(model.Id.Entry);
        var groupExists = EventSkinRuntime.IsManaged(model);
        var host = new Control { Name = "EventSkinPreview", Size = new Vector2(1920, 1080) };
        var layout = PreloadManager.Cache.GetScene(NEventLayout.defaultScenePath).Instantiate<NEventLayout>();
        PreviewNodes.Add(layout, new object());
        // Preserve the native full-screen anchors, portrait/VFX alignment and scale. Shrinking
        // the whole layout to fit the sidebar also shrank the event's background to 78%.
        host.AddChild(layout);
        var keys = LocManager.Instance.GetTable(model.LocTable).Keys.ToArray();
        var pages = EventPreviewPolicy.Pages(model.Id.Entry, keys);
        if (pages.Length == 0) pages = ["INITIAL"];
        var page = selectedPage != null && pages.Contains(selectedPage) ? selectedPage : pages[0];
        var pageSelector = new OptionButton
        {
            Position = new Vector2(70, 80), CustomMinimumSize = new Vector2(150, 44),
            FitToLongestItem = false
        };
        ContextualSkinControls.ApplyGameTheme(pageSelector);
        for (var i = 0; i < pages.Length; i++) pageSelector.AddItem($"{i + 1}/{pages.Length}");
        pageSelector.Select(Array.IndexOf(pages, page));
        pageSelector.Visible = pages.Length > 1;
        host.AddChild(pageSelector);
        ScrollContainer? pageScroll = null;
        Action? resizeText = null;
        pageSelector.ItemSelected += index => Populate(pages[checked((int)index)]);
        // Ready occurs after the host is attached by the compendium, not while building it.
        host.Ready += () =>
        {
            // Keep the game's typography and use the available viewport height. A long page
            // first moves into unused space above, and scrolls only when it still cannot fit.
            var column = layout.GetNode<VBoxContainer>("VBoxContainer");
            var nativeBounds = column.GetRect();
            var bounds = EventPreviewPolicy.TextBounds(host.Size, nativeBounds, 0);
            var scroll = new ScrollContainer
            {
                Name = "EventPageScroll", Position = bounds.Position, Size = bounds.Size,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled
            };
            layout.AddChild(scroll);
            column.Reparent(scroll, keepGlobalTransform: false);
            column.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopLeft);
            column.CustomMinimumSize = new Vector2(800, 0);
            column.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            pageScroll = scroll;
            var resizeQueued = false;
            resizeText = () =>
            {
                if (resizeQueued) return;
                resizeQueued = true;
                Callable.From(() =>
                {
                    resizeQueued = false;
                    if (!GodotObject.IsInstanceValid(host) || !host.IsInsideTree() || host.IsQueuedForDeletion()) return;
                    var next = EventPreviewPolicy.TextBounds(host.Size, nativeBounds, column.GetCombinedMinimumSize().Y);
                    scroll.Position = next.Position;
                    scroll.Size = next.Size;
                }).CallDeferred();
            };
            column.MinimumSizeChanged += () => resizeText();
            host.Resized += () => resizeText();
            try
            {
                var canonical = $"res://images/events/{model.Id.Entry.ToLowerInvariant()}.png";
                Texture2D? portrait = null;
                // The game's own method goes through provider patches (The Shame and other mods
                // redirect it to their private resource folder), so it must be tried first.
                try
                {
                    portrait = EventSkinRuntime.TryLoadInitialPortrait(model, false, out var selectedPortrait)
                        ? selectedPortrait : model.CreateInitialPortrait();
                }
                catch
                {
                    // Fall back to the canonical/custom paths below.
                }

                if (portrait == null)
                {
                    var customPortrait = ReadCustomString(model, "CustomInitialPortraitPath");
                    var path = !string.IsNullOrWhiteSpace(customPortrait) &&
                               ResourceLoader.Exists(customPortrait)
                        ? customPortrait!
                        : canonical;
                    if (ResourceLoader.Exists(path))
                    {
                        portrait = groupExists && path.Equals(canonical, StringComparison.OrdinalIgnoreCase)
                            ? EventSkinRuntime.LoadTexture(groupId, path)
                            : PreloadManager.Cache.GetTexture2D(path);
                    }
                }

                if (portrait != null)
                {
                    layout.SetPortrait(portrait);
                }

                // BaseLib-based custom events often put their artwork in a full background scene
                // instead of the canonical portrait image. Show it behind the layout exactly as
                // the event room does.
                PackedScene? backgroundScene = null;
                try
                {
                    backgroundScene = model.CreateBackgroundScene();
                }
                catch
                {
                    // Fall through to the BaseLib path below.
                }

                if (backgroundScene == null)
                {
                    var customBackground = ReadCustomString(model, "CustomBackgroundScenePath");
                    if (!string.IsNullOrWhiteSpace(customBackground) &&
                        ResourceLoader.Exists(customBackground))
                    {
                        backgroundScene = PreloadManager.Cache.GetScene(customBackground);
                    }
                }

                if (backgroundScene != null)
                {
                    var background = backgroundScene
                        .Instantiate<Control>(PackedScene.GenEditState.Disabled);
                    background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
                    background.MouseFilter = Control.MouseFilterEnum.Ignore;
                    layout.AddChild(background);
                    layout.MoveChild(background, 0);
                }
                if (model.HasVfx)
                {
                    var vfx = model.CreateVfx();
                    layout.AddVfxAnchoredToPortrait(vfx);
                    vfx.Position = EventModel.VfxOffset;
                }
            }
            catch (Exception exception)
            {
                ModLog.Error($"事件图鉴预览 {model.Id.Entry} 失败：{exception}");
            }
            Populate(page);
        };
        return host;

        void Populate(string pageId)
        {
            var title = layout.GetNodeOrNull<MegaLabel>("%Title");
            if (title != null) { title.Text = Title(model); title.Modulate = Colors.White; }
            var description = layout.GetNodeOrNull<MegaRichTextLabel>("%EventDescription");
            if (description != null)
            {
                description.SetTextAutoSize(Format(model, $"{model.Id.Entry}.pages.{pageId}.description"));
                description.Modulate = Colors.White;
                description.VisibleRatio = 1;
            }
            var shared = layout.GetNodeOrNull<Control>("%SharedEventLabel");
            if (shared != null) shared.Visible = false;
            var container = layout.GetNode<VBoxContainer>("%OptionsContainer");
            foreach (var old in container.GetChildren()) { container.RemoveChild(old); old.QueueFree(); }
            foreach (var key in EventPreviewPolicy.Options(model.Id.Entry, pageId, keys))
            {
                var option = PreloadManager.Cache.GetScene("res://scenes/events/event_option_button.tscn")
                    .Instantiate<NEventOptionButton>();
                PreviewNodes.Add(option, new object());
                option.MouseFilter = Control.MouseFilterEnum.Ignore;
                option.FocusMode = Control.FocusModeEnum.None;
                option.ProcessMode = Node.ProcessModeEnum.Disabled;
                container.AddChild(option);
                var optionTitle = Format(model, key + ".title");
                var optionDescription = Format(model, key + ".description");
                option.GetNode<MegaRichTextLabel>("%Text").SetTextAutoSize(
                    $"[gold][b]{optionTitle}[/b][/gold]\n{optionDescription}");
            }
            if (pageScroll != null) pageScroll.ScrollVertical = 0;
            resizeText?.Invoke();
        }
    }

    private static string Format(EventModel model, string key)
    {
        if (!LocString.Exists(model.LocTable, key)) return "";
        var text = new LocString(model.LocTable, key);
        model.DynamicVars.AddTo(text);
        GameBranchApi.AddCharacterDetails(ModelDb.Character<Ironclad>(), text);
        text.Add("IsMultiplayer", false);
        var raw = text.GetRawText();
        // Some later pages require a rolled card/relic or prior choice. Don't simulate those
        // actions or spam the formatter with absent variables; keep authored placeholders.
        if (Regex.Matches(raw, @"(?<!\{)\{([A-Za-z_]\w*)")
            .Any(match => !text.Variables.ContainsKey(match.Groups[1].Value))) return raw;
        return text.GetFormattedText();
    }
}

[HarmonyPatch]
internal static class EventPreviewLifecyclePatch
{
    private static IEnumerable<MethodBase> TargetMethods() =>
    [
        AccessTools.Method(typeof(NEventLayout), nameof(NEventLayout._EnterTree)),
        AccessTools.Method(typeof(NEventLayout), nameof(NEventLayout._ExitTree)),
        AccessTools.Method(typeof(NEventOptionButton), nameof(NEventOptionButton._Ready)),
        AccessTools.Method(typeof(NEventOptionButton), "OnRelease")
    ];
    private static bool Prefix(Node __instance) =>
        UiInjectionGuard.Run("EventPreviewLifecyclePatch.Prefix",
            () => !EventCompendiumPreview.IsPreview(__instance),
            fallback: true);
}

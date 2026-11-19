using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Some character skins draw their character-select image themselves: their Harmony postfix only
/// targets <c>NCharacterSelectScreen.SelectCharacter</c> and attaches an overlay control to the
/// <c>&lt;character&gt;_bg</c> child of <c>AnimatedBg</c> (魅魔战士/贞德这类 BattleWear 包就是如此).
/// Blitz/Dread screens never call that method, so the overlay is missing there and the game's own
/// background shows. Replaying the postfix is impossible — its first parameter is the standard
/// screen type — but those packs expose a screen-agnostic <c>Attach(Control background)</c> helper,
/// which is exactly what the postfix calls. Invoke that on the mode screen's background instead,
/// only for the currently selected provider.
/// </summary>
internal static class ProviderModeSelectOverlayBridge
{
    private const string SuccubusProvider = "succubusIroncladHpWear";
    private const string SuccubusAttachType = "BattleWear.SuccubusCharacterSelect";
    private const string SuccubusPreviewClick = "SuccubusSkinPreviewClick";
    private const string JoanClickCatcher = "JoanofArcIroncladHpwearClickCatcher";
    private const string StageCycleCatcher = "SkinChangerCharSelectStageCatcher";
    private const string OriginalMouseFilterMeta = "sc_mode_bg_orig_mouse_filter";

    internal static bool IsSuccubusProvider(string? providerId) =>
        providerId != null && ProviderInstanceIdentityPolicy.IsOptionSelectionAlias(
            SuccubusProvider, SuccubusProvider, SuccubusProvider, providerId);

    internal static bool IsModeScreenTypeName(string? fullName) =>
        fullName != null && CharacterSelectScreenAccess.ModeTypes.Contains(fullName, StringComparer.Ordinal);

    internal static bool IsBackgroundClickCatcherName(string? nodeName) =>
        !string.IsNullOrWhiteSpace(nodeName) && (
            nodeName.Equals(SuccubusPreviewClick, StringComparison.OrdinalIgnoreCase) ||
            nodeName.Equals(JoanClickCatcher, StringComparison.OrdinalIgnoreCase) ||
            nodeName.Equals(StageCycleCatcher, StringComparison.OrdinalIgnoreCase) ||
            nodeName.Contains("ClickCatcher", StringComparison.OrdinalIgnoreCase) ||
            nodeName.Contains("PreviewClick", StringComparison.OrdinalIgnoreCase));

    internal static bool IsBlockingVisualOverlayType(Type? controlType) =>
        controlType != null &&
        typeof(Control).IsAssignableFrom(controlType) &&
        !typeof(BaseButton).IsAssignableFrom(controlType) &&
        !typeof(Godot.Range).IsAssignableFrom(controlType) &&
        !typeof(LineEdit).IsAssignableFrom(controlType) &&
        !typeof(TextEdit).IsAssignableFrom(controlType) &&
        !typeof(ItemList).IsAssignableFrom(controlType) &&
        !typeof(Tree).IsAssignableFrom(controlType) &&
        !typeof(TabBar).IsAssignableFrom(controlType);

    internal static bool ShouldAllowOverlayClickPassthrough(
        string? screenTypeFullName,
        bool backgroundInSubViewport,
        bool hasActiveCatcher) =>
        IsModeScreenTypeName(screenTypeFullName) && backgroundInSubViewport && hasActiveCatcher;

    internal static void AttachRebuiltBackground(Control screen, CharacterModel character, Control background)
    {
        if (!IsModeScreenTypeName(screen.GetType().FullName))
        {
            return;
        }

        try
        {
            if (!character.Id.Entry.Equals("ironclad", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var providerId = SkinService.GetSelectedCreatureRuntimeProvider("ironclad");
            if (providerId == null || !IsSuccubusProvider(providerId))
            {
                return;
            }

            var assembly = ManagedSkinModLoader.GetActiveProviderAssembly(providerId) ??
                AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                    candidate => ManagedSkinModLoader.IsProviderAssemblyFor(providerId, candidate));
            var attach = assembly?.GetType(SuccubusAttachType)?.GetMethod(
                "Attach",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                null,
                [typeof(Control)],
                null);
            attach?.Invoke(null, [background]);
            if (attach != null && background.GetNodeOrNull<Control>(SuccubusPreviewClick) != null)
            {
                ModLog.Info($"[选角背景] {providerId} 已挂到新背景 {background.Name}（实例 {background.GetInstanceId()}）");
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"恢复模式选人背景失败：{exception.GetBaseException().Message}");
        }
        finally
        {
            SyncModeBackgroundClickPassthrough(screen, character);
        }
    }

    internal static void Attach(Control screen, CharacterModel character)
    {
        if (!GodotObject.IsInstanceValid(screen) || character == null)
        {
            return;
        }

        try
        {
            var groupId = character.Id.Entry.ToLowerInvariant();
            var providerId = SkinService.GetSelectedCreatureRuntimeProvider(groupId);
            if (string.IsNullOrWhiteSpace(providerId))
            {
                return;
            }

            var container = CharacterSelectScreenAccess.Background(screen);
            var background = container?.GetNodeOrNull<Control>(character.Id.Entry + "_bg");
            if (background == null || !GodotObject.IsInstanceValid(background) || !background.IsInsideTree())
            {
                return;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!ManagedSkinModLoader.IsProviderAssemblyFor(providerId, assembly))
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

                foreach (var type in types)
                {
                    // The pack keeps this helper private; match it by shape (single Control argument)
                    // instead of relying on any name of ours.
                    var method = type.GetMethod(
                        "Attach",
                        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
                        null,
                        [typeof(Control)],
                        null);
                    if (method == null || type == typeof(ProviderModeSelectOverlayBridge))
                    {
                        continue;
                    }

                    try
                    {
                        method.Invoke(null, [background]);
                        ModLog.Info($"[选角背景] 已为 {providerId} 在模式界面重放提供者的选人叠加层（{type.Name}.Attach）。");
                    }
                    catch (Exception exception)
                    {
                        ModLog.Warn(
                            $"模式界面重放 {providerId} 的选人叠加层失败（{type.Name}）：" +
                            exception.GetBaseException().Message);
                    }
                }
            }
        }
        finally
        {
            SyncModeBackgroundClickPassthrough(screen, character);
        }
    }

    /// <summary>
    /// Dread wraps <c>AnimatedBg</c> inside a <c>SubViewport</c> and places full-screen visual
    /// siblings (<c>GrayScaler</c>, <c>BigRotatingContainer</c>) after it with default
    /// <c>MouseFilter = Stop</c>, swallowing every click meant for background stage/preview
    /// catchers. Pass mouse input through those visual-only siblings only while the currently
    /// selected skin actually has an active background click catcher, and restore their original
    /// <c>MouseFilter</c> as soon as another skin or character is selected.
    /// </summary>
    internal static void SyncModeBackgroundClickPassthrough(Control screen, CharacterModel? character)
    {
        if (!GodotObject.IsInstanceValid(screen) || !IsModeScreenTypeName(screen.GetType().FullName))
        {
            return;
        }

        try
        {
            var container = CharacterSelectScreenAccess.Background(screen);
            if (container == null || !GodotObject.IsInstanceValid(container) ||
                container.GetParent() is not SubViewport subViewport)
            {
                return;
            }

            var overlays = CollectBlockingSubViewportOverlays(subViewport, container);
            if (overlays.Count == 0)
            {
                return;
            }

            var hasActiveCatcher = HasActiveBackgroundClickCatcher(container, character);
            if (ShouldAllowOverlayClickPassthrough(screen.GetType().FullName, backgroundInSubViewport: true, hasActiveCatcher))
            {
                var changed = new List<string>();
                foreach (var overlay in overlays)
                {
                    if (!overlay.HasMeta(OriginalMouseFilterMeta))
                    {
                        overlay.SetMeta(OriginalMouseFilterMeta, (int)overlay.MouseFilter);
                    }

                    if (overlay.MouseFilter != Control.MouseFilterEnum.Ignore)
                    {
                        overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
                        changed.Add(overlay.Name.ToString());
                    }
                }

                if (changed.Count > 0)
                {
                    ModLog.Info(
                        $"[选角背景] 已在模式界面放通 {character?.Id.Entry ?? "当前角色"} 背景点击" +
                        $"（穿透 {string.Join("、", changed)}）。");
                }
            }
            else
            {
                var restored = new List<string>();
                foreach (var overlay in overlays)
                {
                    if (!overlay.HasMeta(OriginalMouseFilterMeta))
                    {
                        continue;
                    }

                    var original = (Control.MouseFilterEnum)overlay.GetMeta(OriginalMouseFilterMeta).AsInt32();
                    overlay.MouseFilter = original;
                    overlay.RemoveMeta(OriginalMouseFilterMeta);
                    restored.Add(overlay.Name.ToString());
                }

                if (restored.Count > 0)
                {
                    ModLog.Info(
                        $"[选角背景] 已在模式界面恢复 {character?.Id.Entry ?? "当前角色"} 遮罩点击拦截" +
                        $"（{string.Join("、", restored)}）。");
                }
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("同步模式界面选角背景点击穿透失败：" + exception.GetBaseException().Message);
        }
    }

    private static List<Control> CollectBlockingSubViewportOverlays(SubViewport subViewport, Control animatedBg)
    {
        var overlays = new List<Control>();
        var bgIndex = animatedBg.GetIndex();
        foreach (var sibling in subViewport.GetChildren())
        {
            if (sibling.GetIndex() <= bgIndex || !GodotObject.IsInstanceValid(sibling))
            {
                continue;
            }

            CollectVisualControls(sibling, overlays);
        }

        return overlays;
    }

    private static void CollectVisualControls(Node node, List<Control> target)
    {
        if (node is Control control && IsBlockingVisualOverlayType(control.GetType()))
        {
            target.Add(control);
        }

        foreach (var child in node.GetChildren())
        {
            if (GodotObject.IsInstanceValid(child))
            {
                CollectVisualControls(child, target);
            }
        }
    }

    private static bool HasActiveBackgroundClickCatcher(Control container, CharacterModel? character)
    {
        if (container.GetNodeOrNull<Control>(StageCycleCatcher) is { } stageCatcher &&
            IsActiveCatcherControl(stageCatcher))
        {
            return true;
        }

        var background = character != null
            ? container.GetNodeOrNull<Control>(character.Id.Entry + "_bg")
            : null;
        if (background != null && GodotObject.IsInstanceValid(background) &&
            ContainsActiveCatcher(background))
        {
            return true;
        }

        foreach (var child in container.GetChildren())
        {
            if (child is Control control &&
                !ReferenceEquals(control, background) &&
                !control.Name.ToString().Equals(StageCycleCatcher, StringComparison.OrdinalIgnoreCase) &&
                ContainsActiveCatcher(control))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsActiveCatcher(Node root)
    {
        foreach (var child in root.GetChildren())
        {
            if (!GodotObject.IsInstanceValid(child))
            {
                continue;
            }

            if (child is Control control &&
                IsBackgroundClickCatcherName(control.Name.ToString()) &&
                IsActiveCatcherControl(control))
            {
                return true;
            }

            if (ContainsActiveCatcher(child))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsActiveCatcherControl(Control control) =>
        GodotObject.IsInstanceValid(control) &&
        control.Visible &&
        control.MouseFilter != Control.MouseFilterEnum.Ignore;
}

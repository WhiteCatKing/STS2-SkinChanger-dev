using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Some character skins attach their character-select preview / battle-wear click cycling only
/// from Harmony postfixes on <c>NCharacterSelectScreen.SelectCharacter</c> (for example
/// <c>BattleWear.CharSelectCore.Attach(Control, CsProfile)</c> used by the HpWear family,
/// <c>BattleWear.SuccubusCharacterSelect.Attach(Control)</c>,
/// <c>JoanofArcIroncladHpwear.CharSelectCycle.Attach(Control)</c>, or
/// <c>WatcherHpWear.SelectionCycle.AttachBackground(Control)</c>).
/// Blitz/Dread mode screens never invoke <c>NCharacterSelectScreen.SelectCharacter</c>, and
/// Skin Changer's background rebuilds replace the <c>&lt;character&gt;_bg</c> node with a fresh
/// instance. This bridge generically discovers and replays the currently selected provider's
/// screen-agnostic background attach entry on Blitz/Dread screens only (coalesced per frame so
/// each selection attaches once to the final surviving background), pauses hidden screens'
/// background input pollers so static singleton <c>CsProfile.WasPressed</c> state is never
/// stolen across cached submenus, and coordinates Dread's SubViewport visual overlay click
/// passthrough while a background click catcher is active.
/// </summary>
internal static class ProviderModeSelectOverlayBridge
{
    private const string SuccubusProvider = "succubusIroncladHpWear";
    private const string SuccubusPreviewClick = "SuccubusSkinPreviewClick";
    private const string JoanClickCatcher = "JoanofArcIroncladHpwearClickCatcher";
    private const string StageCycleCatcher = "SkinChangerCharSelectStageCatcher";
    private const string CrossbowFoolClick = "CrossbowFoolClick";
    private const string OriginalMouseFilterMeta = "sc_mode_bg_orig_mouse_filter";
    private const string AttachedProviderMeta = "sc_mode_bg_attached_provider";
    private const string PendingAttachGenerationMeta = "sc_mode_bg_attach_generation";
    private const string LifecycleHookedMeta = "sc_char_select_poller_lifecycle";
    private const string PausedPollerMeta = "sc_paused_input_poller";

    private const BindingFlags DeclaredStaticFlags =
        BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    private const BindingFlags InstanceMemberFlags =
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    private static readonly List<Control> TrackedScreens = [];

    internal sealed record BackgroundAttachBinding(
        Type DeclaringType,
        MethodInfo AttachMethod,
        MethodInfo? NotifySelectedCharacterMethod,
        Func<string, object?>? ResolveProfile);

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
            nodeName.Equals(CrossbowFoolClick, StringComparison.OrdinalIgnoreCase) ||
            nodeName.Contains("ClickCatcher", StringComparison.OrdinalIgnoreCase) ||
            nodeName.Contains("PreviewClick", StringComparison.OrdinalIgnoreCase) ||
            nodeName.Contains("WearClick", StringComparison.OrdinalIgnoreCase));

    internal static bool IsProviderInputPollerName(string? nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName) ||
            nodeName.Contains("Safety", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return nodeName.Equals("DBWInputPoller", StringComparison.OrdinalIgnoreCase) ||
               nodeName.Contains("InputPoller", StringComparison.OrdinalIgnoreCase) ||
               nodeName.Contains("ClickPoller", StringComparison.OrdinalIgnoreCase) ||
               nodeName.Contains("HpwearPoller", StringComparison.OrdinalIgnoreCase);
    }

    internal static bool ShouldRunDeferredAttach(
        long scheduledGeneration,
        long currentGeneration,
        bool screenValid,
        bool screenVisible) =>
        screenValid && screenVisible && scheduledGeneration == currentGeneration;

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

    internal static IReadOnlyList<BackgroundAttachBinding> DiscoverBackgroundAttachBindings(
        IEnumerable<Type> types,
        string characterId)
    {
        var bindings = new List<BackgroundAttachBinding>();
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return bindings;
        }

        foreach (var type in types)
        {
            if (type == null ||
                type == typeof(ProviderModeSelectOverlayBridge) ||
                type.ContainsGenericParameters)
            {
                continue;
            }

            MethodInfo[] methods;
            try
            {
                methods = type.GetMethods(DeclaredStaticFlags);
            }
            catch
            {
                continue;
            }

            var notifyMethod = FindNotifySelectedCharacterMethod(methods);
            foreach (var method in methods)
            {
                if (method.ContainsGenericParameters ||
                    (method.ReturnType != typeof(void) && method.ReturnType != typeof(bool)))
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 1 &&
                    parameters[0].ParameterType == typeof(Control) &&
                    (method.Name.Equals("Attach", StringComparison.Ordinal) ||
                     method.Name.Equals("AttachBackground", StringComparison.Ordinal)))
                {
                    bindings.Add(new BackgroundAttachBinding(
                        type,
                        method,
                        notifyMethod,
                        ResolveProfile: null));
                    continue;
                }

                if (parameters.Length == 2 &&
                    method.Name.Equals("Attach", StringComparison.Ordinal) &&
                    parameters[0].ParameterType == typeof(Control) &&
                    IsProfileParameterType(parameters[1].ParameterType))
                {
                    var profileType = parameters[1].ParameterType;
                    if (TryResolveProfile(type, profileType, characterId) == null)
                    {
                        continue;
                    }

                    bindings.Add(new BackgroundAttachBinding(
                        type,
                        method,
                        notifyMethod,
                        id => TryResolveProfile(type, profileType, id)));
                }
            }
        }

        return bindings;
    }

    internal static object? TryResolveProfile(Type declaringType, Type profileType, string characterId)
    {
        if (string.IsNullOrWhiteSpace(characterId))
        {
            return null;
        }

        try
        {
            var profileFor = declaringType.GetMethod(
                "ProfileFor",
                DeclaredStaticFlags,
                null,
                [typeof(string)],
                null);
            if (profileFor != null && profileType.IsAssignableFrom(profileFor.ReturnType))
            {
                var resolved = profileFor.Invoke(null, [characterId]);
                if (resolved != null && profileType.IsInstanceOfType(resolved))
                {
                    return resolved;
                }
            }
        }
        catch
        {
            // Fall back to static profile fields when ProfileFor is unavailable or throws.
        }

        FieldInfo[] fields;
        try
        {
            fields = declaringType.GetFields(DeclaredStaticFlags);
        }
        catch
        {
            return null;
        }

        object? nameFallback = null;
        foreach (var field in fields)
        {
            if (!profileType.IsAssignableFrom(field.FieldType))
            {
                continue;
            }

            object? value;
            try
            {
                value = field.GetValue(null);
            }
            catch
            {
                continue;
            }

            if (value == null)
            {
                continue;
            }

            if (field.Name.Equals(characterId, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }

            if (nameFallback == null && ProfileMatchesCharacterId(value, characterId))
            {
                nameFallback = value;
            }
        }

        return nameFallback;
    }

    internal static bool ResetProfileDeadFlag(object? profile) =>
        ResetBoolField(profile, "Dead");

    internal static bool ResetProfileInputState(object? profile)
    {
        var changed = false;
        changed |= ResetBoolField(profile, "Dead");
        changed |= ResetBoolField(profile, "WasPressed");
        changed |= ResetBoolField(profile, "IgnoreClickUntilRelease");
        return changed;
    }

    private static bool ResetBoolField(object? instance, string fieldName)
    {
        if (instance == null)
        {
            return false;
        }

        try
        {
            var field = instance.GetType().GetField(fieldName, InstanceMemberFlags);
            if (field != null &&
                field.FieldType == typeof(bool) &&
                field.GetValue(instance) is true)
            {
                field.SetValue(instance, false);
                return true;
            }
        }
        catch
        {
            // Ignore read-only or incompatible profile shapes.
        }

        return false;
    }

    internal static bool InvokeBinding(
        BackgroundAttachBinding binding,
        Control? background,
        string characterId)
    {
        binding.NotifySelectedCharacterMethod?.Invoke(null, [characterId]);

        if (binding.ResolveProfile != null)
        {
            var profile = binding.ResolveProfile(characterId);
            if (profile == null)
            {
                return false;
            }

            ResetProfileInputState(profile);
            binding.AttachMethod.Invoke(null, [background, profile]);
            return true;
        }

        binding.AttachMethod.Invoke(null, [background]);
        return true;
    }

    /// <summary>
    /// Tracks every character-select screen (Standard, Blitz, Dread) and pauses its background
    /// input pollers whenever the screen is hidden in <c>NSubmenuStack</c>. Packs such as
    /// <c>silentKaguyaHpWear</c>, <c>ironcladHpWear</c>, and <c>defectHpWear</c> do not check
    /// <c>bg.IsVisibleInTree()</c> inside <c>DBWInputPoller</c> and mutate a static singleton
    /// <c>CsProfile.WasPressed</c>; leaving a hidden screen's poller running steals rising-edge
    /// clicks from whichever screen is currently visible.
    /// </summary>
    internal static void RegisterScreenLifecycle(Control? screen)
    {
        if (screen == null || !GodotObject.IsInstanceValid(screen) || !CharacterSelectScreenAccess.IsSupported(screen))
        {
            return;
        }

        PruneTrackedScreens();
        if (!TrackedScreens.Any(existing => ReferenceEquals(existing, screen)))
        {
            TrackedScreens.Add(screen);
        }

        if (screen.HasMeta(LifecycleHookedMeta))
        {
            return;
        }

        screen.SetMeta(LifecycleHookedMeta, true);
        screen.Connect(
            CanvasItem.SignalName.VisibilityChanged,
            Callable.From(() => OnTrackedScreenVisibilityChanged(screen)));
    }

    internal static void SyncExclusiveScreenPollers(Control? activeScreen)
    {
        PruneTrackedScreens();
        foreach (var screen in TrackedScreens)
        {
            if (ReferenceEquals(screen, activeScreen) && screen.IsVisibleInTree())
            {
                ResumeScreenBackgroundPollers(screen);
            }
            else
            {
                PauseScreenBackgroundPollers(screen);
            }
        }
    }

    internal static void StopBackgroundPollers(Node? root, bool queueFree = false)
    {
        if (root == null || !GodotObject.IsInstanceValid(root))
        {
            return;
        }

        var timers = new List<Godot.Timer>();
        CollectInputPollerTimers(root, timers);
        foreach (var timer in timers)
        {
            if (!GodotObject.IsInstanceValid(timer))
            {
                continue;
            }

            timer.Stop();
            if (queueFree)
            {
                timer.QueueFree();
            }
            else
            {
                timer.SetMeta(PausedPollerMeta, true);
            }
        }
    }

    internal static void AttachRebuiltBackground(Control screen, CharacterModel character, Control background)
    {
        if (!GodotObject.IsInstanceValid(screen) ||
            character == null ||
            !IsModeScreenTypeName(screen.GetType().FullName))
        {
            return;
        }

        try
        {
            RegisterScreenLifecycle(screen);
            SyncExclusiveScreenPollers(screen);
            ScheduleDeferredAttach(screen, character);
        }
        finally
        {
            SyncModeBackgroundClickPassthrough(screen, character);
        }
    }

    internal static void Attach(Control screen, CharacterModel character)
    {
        if (!GodotObject.IsInstanceValid(screen) ||
            character == null ||
            !IsModeScreenTypeName(screen.GetType().FullName))
        {
            return;
        }

        try
        {
            RegisterScreenLifecycle(screen);
            SyncExclusiveScreenPollers(screen);
            ScheduleDeferredAttach(screen, character);
        }
        finally
        {
            SyncModeBackgroundClickPassthrough(screen, character);
        }
    }

    private static void ScheduleDeferredAttach(Control screen, CharacterModel character)
    {
        var generation = screen.GetMeta(PendingAttachGenerationMeta, 0L).AsInt64() + 1L;
        screen.SetMeta(PendingAttachGenerationMeta, generation);
        Callable.From(() =>
        {
            var valid = GodotObject.IsInstanceValid(screen) && screen.IsInsideTree();
            var visible = valid && screen.IsVisibleInTree();
            var current = valid ? screen.GetMeta(PendingAttachGenerationMeta, 0L).AsInt64() : -1L;
            if (!ShouldRunDeferredAttach(generation, current, valid, visible))
            {
                return;
            }

            try
            {
                SyncExclusiveScreenPollers(screen);
                var container = CharacterSelectScreenAccess.Background(screen);
                var background = FindCharacterBackground(container, character.Id.Entry);
                AttachCore(character, background);
            }
            finally
            {
                SyncModeBackgroundClickPassthrough(screen, character);
            }
        }).CallDeferred();
    }

    private static void OnTrackedScreenVisibilityChanged(Control screen)
    {
        if (!GodotObject.IsInstanceValid(screen))
        {
            PruneTrackedScreens();
            return;
        }

        if (screen.IsVisibleInTree())
        {
            SyncExclusiveScreenPollers(screen);
        }
        else
        {
            PauseScreenBackgroundPollers(screen);
        }
    }

    private static void PauseScreenBackgroundPollers(Control screen)
    {
        var container = CharacterSelectScreenAccess.Background(screen);
        if (container != null && GodotObject.IsInstanceValid(container))
        {
            StopBackgroundPollers(container, queueFree: false);
        }
    }

    private static void ResumeScreenBackgroundPollers(Control screen)
    {
        var container = CharacterSelectScreenAccess.Background(screen);
        if (container == null || !GodotObject.IsInstanceValid(container))
        {
            return;
        }

        var timers = new List<Godot.Timer>();
        CollectInputPollerTimers(container, timers);
        foreach (var timer in timers)
        {
            if (!GodotObject.IsInstanceValid(timer) || !timer.HasMeta(PausedPollerMeta))
            {
                continue;
            }

            if (timer.GetParent() is CanvasItem parentCanvas && !parentCanvas.IsVisibleInTree())
            {
                continue;
            }

            timer.RemoveMeta(PausedPollerMeta);
            if (timer.IsStopped())
            {
                timer.Start();
            }
        }
    }

    private static void CollectInputPollerTimers(Node root, List<Godot.Timer> target)
    {
        if (root is Godot.Timer timer && IsProviderInputPollerName(timer.Name.ToString()))
        {
            target.Add(timer);
        }

        foreach (var child in root.GetChildren())
        {
            if (GodotObject.IsInstanceValid(child))
            {
                CollectInputPollerTimers(child, target);
            }
        }
    }

    private static void PruneTrackedScreens()
    {
        for (var i = TrackedScreens.Count - 1; i >= 0; i--)
        {
            if (!GodotObject.IsInstanceValid(TrackedScreens[i]))
            {
                TrackedScreens.RemoveAt(i);
            }
        }
    }

    private static void AttachCore(CharacterModel character, Control? background)
    {
        try
        {
            if (background == null || !GodotObject.IsInstanceValid(background) || !background.IsInsideTree())
            {
                return;
            }

            var groupId = character.Id.Entry.ToLowerInvariant();
            var providerId = SkinService.GetSelectedCreatureRuntimeProvider(groupId) ??
                             SkinService.GetSelectedRuntimeProvider(groupId);
            if (string.IsNullOrWhiteSpace(providerId))
            {
                return;
            }

            var assemblies = ResolveProviderAssemblies(providerId);
            if (assemblies.Count == 0)
            {
                return;
            }

            var alreadyAttached =
                background.HasMeta(AttachedProviderMeta) &&
                string.Equals(
                    background.GetMeta(AttachedProviderMeta).AsString(),
                    providerId,
                    StringComparison.OrdinalIgnoreCase) &&
                ContainsActiveCatcher(background);

            var attachedAny = false;
            foreach (var assembly in assemblies)
            {
                var bindings = DiscoverBackgroundAttachBindings(GetLoadableTypes(assembly), groupId);
                foreach (var binding in bindings)
                {
                    if (alreadyAttached)
                    {
                        try
                        {
                            binding.NotifySelectedCharacterMethod?.Invoke(null, [groupId]);
                        }
                        catch (Exception exception)
                        {
                            ModLog.Warn(
                                $"模式界面同步 {providerId} 选中角色失败（{binding.DeclaringType.Name}）：" +
                                exception.GetBaseException().Message);
                        }

                        continue;
                    }

                    try
                    {
                        if (InvokeBinding(binding, background, groupId))
                        {
                            attachedAny = true;
                            ModLog.Info(
                                $"[选角背景] 已为 {providerId} 在模式界面挂接选人背景交互" +
                                $"（{binding.DeclaringType.Name}.{binding.AttachMethod.Name}，" +
                                $"背景={background.Name}，实例={background.GetInstanceId()}）。");
                        }
                    }
                    catch (Exception exception)
                    {
                        ModLog.Warn(
                            $"模式界面重放 {providerId} 的选人叠加层失败（{binding.DeclaringType.Name}）：" +
                            exception.GetBaseException().Message);
                    }
                }
            }

            if (attachedAny)
            {
                background.SetMeta(AttachedProviderMeta, providerId);
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"恢复模式选人背景失败：{exception.GetBaseException().Message}");
        }
    }

    private static List<Assembly> ResolveProviderAssemblies(string providerId)
    {
        var assemblies = new List<Assembly>();
        var active = ManagedSkinModLoader.GetActiveProviderAssembly(providerId);
        if (active != null)
        {
            assemblies.Add(active);
        }

        foreach (var candidate in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assemblies.Contains(candidate))
            {
                continue;
            }

            if (ManagedSkinModLoader.IsProviderAssemblyFor(providerId, candidate))
            {
                assemblies.Add(candidate);
            }
        }

        return assemblies;
    }

    private static Type[] GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type != null).Cast<Type>().ToArray();
        }
        catch
        {
            return [];
        }
    }

    private static MethodInfo? FindNotifySelectedCharacterMethod(MethodInfo[] methods)
    {
        foreach (var method in methods)
        {
            if (!method.Name.Equals("NotifySelectedCharacter", StringComparison.Ordinal) ||
                method.ContainsGenericParameters)
            {
                continue;
            }

            var parameters = method.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
            {
                return method;
            }
        }

        return null;
    }

    private static bool IsProfileParameterType(Type type) =>
        !type.IsPrimitive &&
        !type.IsEnum &&
        type != typeof(string) &&
        type != typeof(object) &&
        !typeof(Node).IsAssignableFrom(type);

    private static bool ProfileMatchesCharacterId(object profile, string characterId)
    {
        try
        {
            var type = profile.GetType();
            var nameField = type.GetField("Name", InstanceMemberFlags);
            if (nameField?.FieldType == typeof(string) &&
                nameField.GetValue(profile) is string fieldName &&
                fieldName.Equals(characterId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var nameProperty = type.GetProperty("Name", InstanceMemberFlags);
            if (nameProperty?.PropertyType == typeof(string) &&
                nameProperty.GetValue(profile) is string propName &&
                propName.Equals(characterId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch
        {
            // Ignore unexpected profile members.
        }

        return false;
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
            ? FindCharacterBackground(container, character.Id.Entry)
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

    private static Control? FindCharacterBackground(Control? container, string? characterEntry)
    {
        if (container == null || !GodotObject.IsInstanceValid(container) || string.IsNullOrWhiteSpace(characterEntry))
        {
            return null;
        }

        var direct = container.GetNodeOrNull<Control>(characterEntry + "_bg") ??
                     container.GetNodeOrNull<Control>(characterEntry.ToUpperInvariant() + "_bg") ??
                     container.GetNodeOrNull<Control>(characterEntry.ToLowerInvariant() + "_bg");
        if (direct != null)
        {
            return direct;
        }

        var expected = characterEntry + "_bg";
        foreach (var child in container.GetChildren())
        {
            if (child is Control control &&
                GodotObject.IsInstanceValid(control) &&
                control.Name.ToString().Equals(expected, StringComparison.OrdinalIgnoreCase))
            {
                return control;
            }
        }

        return null;
    }

    private static bool IsActiveCatcherControl(Control control) =>
        GodotObject.IsInstanceValid(control) &&
        control.Visible &&
        control.MouseFilter != Control.MouseFilterEnum.Ignore;
}

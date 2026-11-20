// 受保护兼容模块：十字军/贞德及同包变体。修改前阅读本目录 AGENTS.md 和 README.md。
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Click-to-cycle for the <b>damage stages</b> proved present in the selected package.
///
/// The Joan packs ship <c>s1/s2/s3_&lt;group&gt;_data.tres</c> and, when their DLL runs, a click on the
/// background swaps the live <c>SpineSprite</c>'s skeleton data between those stages. Two packs can
/// ship the same assembly identity, so only one of them can execute — the other one kept its art but
/// lost the click. The persistent SC catcher performs one isolated swap and consumes the event;
/// it does not depend on the author's stale static stage cache after a background replacement.
/// </summary>
internal static class CharSelectStageCycle
{
    private const string HostName = "SkinChangerCharSelectStageHost";
    private const string CatcherName = "SkinChangerCharSelectStageCatcher";
    private const string GroupMeta = "skin_changer_stage_group";
    private const string EntryMeta = "skin_changer_stage_entry";
    private const string UsableMeta = "skin_changer_stage_usable";
    private const string StageMeta = "skin_changer_stage_current";
    private const int MaxStage = 5;

    private static FieldInfo? _bgContainerField;

    private static WeakReference<CharacterModel>? _character;


    internal static void Attach(Control screen, CharacterModel? characterModel)
    {
        try
        {
            var entry = characterModel?.Id.Entry?.ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(entry))
            {
                return;
            }

            var group = ContextualSkinControls.FindGroup(entry);
            if (group == null)
            {
                return;
            }


            var host = screen.GetNodeOrNull<Godot.Timer>(new NodePath(HostName));
            if (host == null)
            {
                host = new Godot.Timer
                {
                    Name = HostName,
                    WaitTime = 0.2,
                    OneShot = false,
                    Autostart = true,
                    ProcessCallback = Godot.Timer.TimerProcessCallback.Idle,
                    ProcessMode = Node.ProcessModeEnum.Always
                };
                screen.AddChild(host);
                var capturedHost = host;
                host.Timeout += () => Tick(screen, capturedHost);
                host.Start(-1);
            }

            host.SetMeta(GroupMeta, group.Id);
            host.SetMeta(EntryMeta, entry);
            _character = new WeakReference<CharacterModel>(characterModel!);
            Tick(screen, host);
        }
        catch (Exception exception)
        {
            ModLog.Warn("选人界面战损档准备失败：" + exception.GetBaseException().Message);
        }
    }

    private static void Tick(Control screen, Godot.Timer host)
    {
        try
        {
            if (!GodotObject.IsInstanceValid(screen) || !GodotObject.IsInstanceValid(host))
            {
                return;
            }

            var groupId = host.GetMeta(GroupMeta, string.Empty).AsString();
            var entry = host.GetMeta(EntryMeta, string.Empty).AsString();
            if (string.IsNullOrWhiteSpace(groupId) || string.IsNullOrWhiteSpace(entry))
            {
                return;
            }

            if (FindBackground(screen, entry, groupId) is not { } background)
            {
                return;
            }

            var spine = FindSpine(background);
            if (spine == null)
            {
                return;
            }

            var plan = ResolvePlan(groupId, spine);
            var usable = plan != null && WearEnabled(groupId);
            // A provider catcher can remain visible/Stop while its private stage cache or
            // ownership check no longer matches the rebuilt model. For packages whose own
            // PCK proves stage support, one persistent SC catcher owns the click exclusively.
            if (plan is { } currentPlan)
            {
                var selection = SkinService.GetCharacterSelectionOptionId(groupId);
                var binding = selection + "|" + background.GetInstanceId();
                if (host.GetMeta("sc_stage_binding", string.Empty).AsString() != binding)
                {
                    // One attempt per new selection/background; failed resources already have
                    // their own circuit breaker. A 5 Hz observer must never become a retry loop.
                    host.SetMeta("sc_stage_binding", binding);
                    var data = LoadStageData(groupId, currentPlan.DataDirectory ?? currentPlan.Directory,
                        currentPlan.Group, currentPlan.CurrentStage);
                    if (data != null)
                    {
                        spine.Set("skeleton_data_res", data);
                        PlayIdle(spine);
                        if (ProviderSettingsVariantOptions.ContractOf(selection) is { } contract)
                            ReapplyPalette(contract, spine);
                    }
                }
            }

            // Parent to the container that holds the background art (AnimatedBg), not to the background
            // node itself and not to the screen: the Skin Changer rebuilds the per-character background
            // node on every skin switch (a catcher parented to the old node stops receiving input),
            // while the screen itself is not a sized Control (a full-rect child there has a zero rect).
            var catcherHost = ResolveCatcherHost(screen, background);
            var catcher = catcherHost.GetNodeOrNull<Control>(new NodePath(CatcherName));
            if (catcher == null)
            {
                catcher = new Control
                {
                    Name = CatcherName,
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    FocusMode = Control.FocusModeEnum.None
                };
                catcher.SetAnchorsPreset(Control.LayoutPreset.FullRect, false);
                catcher.SetOffsetsPreset(Control.LayoutPreset.FullRect);
                var capturedCatcher = catcher;
                catcher.GuiInput += input => OnClick(screen, capturedCatcher, input);
                catcherHost.AddChild(catcher);
            }

            catcher.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            catcher.SetMeta(GroupMeta, groupId);
            catcher.SetMeta(EntryMeta, entry);
            catcher.MouseFilter = usable ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;

            if (usable && catcherHost.GetChildCount(false) > 0 &&
                catcherHost.GetChild(catcherHost.GetChildCount(false) - 1) != catcher)
            {
                catcherHost.MoveChild(catcher, catcherHost.GetChildCount(false) - 1);
            }

            if (usable && !host.GetMeta(UsableMeta, false).AsBool())
            {
                host.SetMeta(UsableMeta, true);
                ModLog.Info(
                    $"已在选人界面为 {groupId} 接上“点角色切战损”：" +
                    "由本 Mod 按当前包隔离的 s1/s2/s3 资源切换。");
            }
        }
        catch
        {
            // Never break the character-select screen from a poller.
        }
    }



    /// <summary>Performs one stage advance. Returns true when the click was consumed.</summary>
    private static bool CycleWithLog(Control screen, string groupId, string entry)
    {
        var background = FindBackground(screen, entry, groupId);
        var spine = background == null ? null : FindSpine(background);
        if (spine == null || ResolvePlan(groupId, spine) is not { } plan)
        {
            return false;
        }

        var next = plan.NextStage;
        var data = LoadStageData(groupId, plan.DataDirectory ?? plan.Directory, plan.Group, next);
        if (data == null)
        {
            next = 1;
            data = LoadStageData(groupId, plan.DataDirectory ?? plan.Directory, plan.Group, 1);
        }

        if (data == null)
        {
            return false;
        }

        spine.Set("skeleton_data_res", data);
        PlayIdle(spine);
        if (ProviderSettingsVariantOptions.ContractOf(
                SkinService.GetCharacterSelectionOptionId(groupId)) is { } selectedContract)
        {
            ReapplyPalette(selectedContract, spine);
        }

        // Preview first, portraits last: the preview rebuild refreshes the character buttons from the
        // skin service, which would overwrite a stage portrait applied before it.
        if (_character is { } characterRef && characterRef.TryGetTarget(out var characterModel))
        {
            CharacterModelPreviewControls.Refresh(screen, characterModel);
        }

        RefreshCharacterButtons(screen, groupId, entry, next);
        ModLog.Info(
            $"选人界面点角色切战损：{plan.Group} 第 {plan.CurrentStage} 档 → 第 {next} 档" +
            $"（{plan.Directory}；变体不变）。");
        return true;
    }

    /// <summary>
    /// The node the click catcher is parented to. It has to be a Control with a real rect that survives the
    /// Skin Changer rebuilding the per-character background: prefer the background's own container (the
    /// pack does the same), then the screen's <c>AnimatedBg</c>, and only then the background itself.
    /// </summary>
    private static Control ResolveCatcherHost(Control screen, Node background)
    {
        if (background.GetParent() is Control { } parent && parent != screen)
        {
            return parent;
        }

        if (screen.GetNodeOrNull<Control>(new NodePath("AnimatedBg")) is { } animated)
        {
            return animated;
        }

        foreach (var candidate in EnumerateNodes(screen).OfType<Control>())
        {
            if (candidate.Name.ToString().Contains("AnimatedBg", StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        return background as Control ?? screen;
    }

    private static void OnClick(Control screen, Control catcher, InputEvent input)
    {
        if (input is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
        {
            return;
        }

        try
        {
            var groupId = catcher.GetMeta(GroupMeta, string.Empty).AsString();
            var entry = catcher.GetMeta(EntryMeta, string.Empty).AsString();
            if (string.IsNullOrWhiteSpace(groupId))
            {
                return;
            }

            if (WearEnabled(groupId) && CycleWithLog(screen, groupId, string.IsNullOrWhiteSpace(entry) ? groupId : entry))
                catcher.AcceptEvent();
        }
        catch (Exception exception)
        {
            ModLog.Warn("选人界面点角色切战损失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Whether this click is ours and what the next stage is. A package without further stages
    /// (the 0.3.5-S build only ships s1) never installs an active catcher.
    /// </summary>
    private static StagePlan? ResolvePlan(string groupId, Node spine)
    {
        var optionId = SkinService.GetCharacterSelectionOptionId(groupId);
        var contract = ProviderSettingsVariantOptions.ContractOf(optionId);
        if (contract == null)
        {
            return null;
        }

        var skeletonPath = ReadSkeletonPath(spine);
        if (string.IsNullOrWhiteSpace(skeletonPath))
        {
            return null;
        }


        var slash = skeletonPath.LastIndexOf('/');
        if (slash <= 0)
        {
            return null;
        }

        var directory = skeletonPath[..slash];
        var file = skeletonPath[(slash + 1)..];
        var dot = file.LastIndexOf('.');
        if (dot > 0)
        {
            file = file[..dot];
        }

        // s<stage>_<group>
        if (file.Length < 4 || file[0] != 's' || !char.IsAsciiDigit(file[1]) || file[2] != '_')
        {
            return null;
        }

        var stage = file[1] - '0';
        var group = file[3..];

        // The live spine can point into the runtime remount namespace, where only the files that were
        // needed for the current stage were materialised. Fall back to the package's own resource root
        // (what the pack itself would have loaded) before deciding the pack has no damaged stages.
        if (!PackageShipsStage(groupId, optionId, group, 2))
        {
            // No damaged stages ship with this package: keep the click free for the game/pack.
            return null;
        }

        var next = stage + 1;
        if (next > MaxStage || !PackageShipsStage(groupId, optionId, group, next))
        {
            next = 1;
        }

        // The live spine can point into a remount namespace that only materialised this one stage,
        // so stage 2/3 files are unreachable from that folder. Load them from the package's own
        // root instead (res://<root>/s<stage>_<group>_data.tres).
        var packageRoot = SkinService.Catalog?.GetAssetRootDirectory(groupId, optionId);
        return new StagePlan(directory, group, stage, next, packageRoot);
    }


    /// <summary>
    /// File names (relative to the skeleton's folder) that may hold one damage stage, in priority order.
    /// The Joan packs use <c>s&lt;stage&gt;_&lt;group&gt;_data.tres</c>; the alternatives cover the obvious
    /// variants of the same idea so a similar pack works without touching this file.
    /// </summary>
    private static IEnumerable<string> StageFileCandidates(string group, int stage)
    {
        yield return $"s{stage}_{group}_data.tres";
        yield return $"s{stage}_{group}.tres";
        yield return $"s{stage}_data.tres";
        yield return $"{group}_s{stage}_data.tres";
    }

    private static Resource? LoadStageData(string groupId, string directory, string group, int stage)
    {
        foreach (var candidate in StageFileCandidates(group, stage))
        {
            var loaded = LoadStageDataFile(groupId, $"{directory}/{candidate}");
            if (loaded != null)
            {
                return loaded;
            }
        }

        return null;
    }

    private static Resource? LoadStageDataFile(string groupId, string path)
    {
        try
        {
            // Both Joan packages export identical canonical paths. Load only the selected
            // stage graph through its aliases; mounting the whole PCK publishes its shared
            // character select portrait and reading the canonical cache can borrow another pack.
            return SkinService.WithRuntimeResources(
                groupId,
                StageBatchPaths(path),
                loaded => loaded.TryGetValue(path, out var resource) ? resource as Resource : null,
                includeProviderDependencies: true);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"读取 {path} 失败：{exception.GetBaseException().Message}");
            return null;
        }
    }

    internal static string[] StageBatchPaths(string path)
    {
        var slash = path.LastIndexOf('/');
        var file = slash < 0 ? path : path[(slash + 1)..];
        // The wear-enabled Joan package proves all three stages exist. Request a stable
        // graph once, rather than creating three alias packs and decoding the same atlas
        // again on each click. Other resource layouts keep their original single request.
        if (slash >= 0 && file.Length == "s1_charselect_data.tres".Length &&
            file[0] == 's' && file[1] is >= '1' and <= '3' && file[2..] == "_charselect_data.tres")
            return Enumerable.Range(1, 3).Select(stage => path[..(slash + 1)] + $"s{stage}_charselect_data.tres").ToArray();
        return [path];
    }

    private static readonly Dictionary<string, bool> StageSupportCache =
        new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the selected package ships this stage at all — read straight from its PCK, so it is also
    /// correct for the package whose DLL is not the one executing.
    /// </summary>
    private static bool PackageShipsStage(string groupId, string? optionId, string group, int stage)
    {
        if (string.IsNullOrWhiteSpace(optionId))
        {
            return false;
        }

        var key = groupId + "|" + optionId + "|" + group + "|" + stage;
        if (StageSupportCache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var support = SkinService.Catalog?.ReadProviderFile(
            groupId,
            optionId,
            $"s{stage}_{group}_data.tres") is { Length: > 0 };
        StageSupportCache[key] = support;
        return support;
    }

    private static string? ReadSkeletonPath(Node spine)
    {
        try
        {
            // Every Variant holds a native reference; leaking them is what eventually crashes the game
            // on the GC finalizer thread ("Godot.GodotObject.Dispose at Finalize").
            using var data = spine.Get("skeleton_data_res");
            if (data.AsGodotObject() is not Resource skeletonData)
            {
                return null;
            }

            using var file = skeletonData.Get("skeleton_file_res");
            return file.AsGodotObject() is Resource fileResource ? fileResource.ResourcePath : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// The pack re-applies its palette right after its own skeleton swap; doing the swap from here has
    /// to do the same or the new stage art shows untinted. Complete any missing stage page before
    /// applying the current native weights; startup may only have exposed the SFW first stage.
    /// </summary>
    private static void ReapplyPalette(
        ProviderSettingsVariantOptions.SettingsVariantContract contract,
        Node spine)
    {
        try
        {
            if (!ProviderSettingsVariantOptions.TryGetExecutorContract(contract, out var executor) ||
                !ManagedSkinModLoader.TryGetLoadedProviderAssembly(executor.AssemblyPath, out var assembly) || assembly == null)
            {
                return;
            }

            foreach (var type in assembly.GetTypes())
            {
                if (type.Name != "Palette")
                {
                    continue;
                }

                StagePalettePages.Ensure(contract, type, ReadSkeletonPath(spine));

                var method = type.GetMethod(
                    "ApplyNow",
                    System.Reflection.BindingFlags.Static |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic,
                    null,
                    [typeof(Node)],
                    null);
                method?.Invoke(null, [spine]);
                return;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("战损档调色未完成：" + exception.GetBaseException().Message);
        }
    }

    private static bool WearEnabled(string groupId)
    {
        try
        {
            var contract = ProviderSettingsVariantOptions.ContractOf(SkinService.GetCharacterSelectionOptionId(groupId));
            if (contract == null || !ManagedSkinModLoader.TryGetLoadedProviderAssembly(contract.AssemblyPath, out var assembly) || assembly == null)
                return true;
            var runtime = assembly.GetType(contract.RuntimeTypeName);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var settings = contract.InstanceMemberIsMethod
                ? runtime?.GetMethod(contract.InstanceMemberName, flags)?.Invoke(null, null)
                : runtime?.GetProperty(contract.InstanceMemberName, flags)?.GetValue(null);
            return settings?.GetType().GetProperty("WearEnabled")?.GetValue(settings) is not false;
        }
        catch { return false; }
    }

    private static void PlayIdle(Node spine)
    {
        try
        {
            using var state = spine.Call("get_animation_state");
            if (state.AsGodotObject() is { } animationState && GodotObject.IsInstanceValid(animationState))
            {
                animationState.Call("set_animation", "animation", true, 0);
            }
        }
        catch
        {
            // Replaying the idle pose is cosmetic; the skeleton already changed.
        }
    }

    /// <summary>
    /// Keeps the character-select button portrait on the same stage as the background, using the
    /// package's own per-variant, per-stage portrait images when it ships them.
    /// </summary>
    private static void RefreshCharacterButtons(Control screen, string groupId, string entry, int stage)
    {
        try
        {
            var icon = LoadStageIcon(groupId, screen, entry, stage);
            if (icon == null)
            {
                return;
            }

            foreach (var button in EnumerateNodes(screen).OfType<NCharacterSelectButton>())
            {
                if (!GodotObject.IsInstanceValid(button) ||
                    button.Character?.Id.Entry?.ToLowerInvariant() != entry)
                {
                    continue;
                }

                foreach (var child in EnumerateNodes(button).OfType<TextureRect>())
                {
                    if (child.Name.ToString().StartsWith("Icon", StringComparison.OrdinalIgnoreCase))
                    {
                        child.Texture = icon;
                    }
                }
            }
        }
        catch
        {
            // The background swap is the feature; the button portrait is best effort.
        }
    }

    private static Texture2D? LoadStageIcon(string groupId, Control screen, string entry, int stage)
    {
        try
        {
            var optionId = SkinService.GetCharacterSelectionOptionId(groupId);
            var contract = ProviderSettingsVariantOptions.ContractOf(optionId);
            if (contract == null)
            {
                return null;
            }

            var index = ProviderSettingsVariantOptions.VariantIndex(optionId);
            if (index == null)
            {
                return null;
            }

            // The packs ship portrait_palettes/<appearance-1>/s<stage>_select_icon.png next to the
            // skeleton; the appearance value is 1-based.
            var directory = string.Empty;
            var spine = FindSpine(FindBackground(screen, entry, groupId) ?? screen);
            if (spine != null)
            {
                var skeletonPath = ReadSkeletonPath(spine);
                if (!string.IsNullOrWhiteSpace(skeletonPath))
                {
                    var slash = skeletonPath.LastIndexOf('/');
                    if (slash > 0)
                    {
                        directory = skeletonPath[..slash];
                    }
                }
            }

            if (directory.Length == 0)
            {
                return null;
            }

            // The packs ship portrait_palettes/<appearance-1>/s<stage>_select_icon.png as a raw PNG
            // inside their PCK (no .import next to it), so the pack itself loads it with
            // FileAccess + Image.LoadPngFromBuffer. Do the same.
            var path = $"{directory}/portrait_palettes/{index.Value}/s{stage}_select_icon.png";
            var bytes = TryReadBytes(path);
            if (bytes == null)
            {
                return null;
            }

            var image = new Image();
            if (image.LoadPngFromBuffer(bytes) != Error.Ok)
            {
                return null;
            }

            return ImageTexture.CreateFromImage(image);
        }
        catch
        {
            return null;
        }
    }

    private static byte[]? TryReadBytes(string path)
    {
        try
        {
            return Godot.FileAccess.FileExists(path) ? Godot.FileAccess.GetFileAsBytes(path) : null;
        }
        catch
        {
            return null;
        }
    }

    private static Node? FindSpine(Node? root, int depth = 0)
    {
        if (root == null || depth > 8 || !GodotObject.IsInstanceValid(root))
        {
            return null;
        }

        if (root is Node2D && root.GetClass() == "SpineSprite")
        {
            return root;
        }

        foreach (var child in root.GetChildren())
        {
            if (FindSpine(child, depth + 1) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static Control? FindBackground(Control screen, string entry, string groupId)
    {
        var container = screen.GetNodeOrNull<Control>(new NodePath("AnimatedBg"));
        if (container == null)
        {
            _bgContainerField ??= AccessTools.Field(typeof(NCharacterSelectScreen), "_bgContainer");
            container = CharacterSelectScreenAccess.Read(_bgContainerField, screen) as Control;
        }

        container ??= screen;

        foreach (var name in new[] { entry + "_bg", groupId + "_bg" })
        {
            if (container.GetNodeOrNull<Control>(new NodePath(name)) is { } direct)
            {
                return direct;
            }
        }

        var wanted = entry.Replace("_", string.Empty) + "bg";
        foreach (var child in container.GetChildren())
        {
            if (child is Control control &&
                control.Name.ToString().Replace("_", string.Empty)
                    .Equals(wanted, StringComparison.OrdinalIgnoreCase))
            {
                return control;
            }
        }

        return null;
    }

    private static IEnumerable<Node> EnumerateNodes(Node root)
    {
        yield return root;
        foreach (var child in root.GetChildren())
        {
            foreach (var node in EnumerateNodes(child))
            {
                yield return node;
            }
        }
    }

    private readonly record struct StagePlan(
        string Directory,
        string Group,
        int CurrentStage,
        int NextStage,
        string? DataDirectory = null);
}

[FeaturePatch(FeatureIds.Character)]
[HarmonyPatch(typeof(NCharacterSelectScreen), "SelectCharacter")]
internal static class CharSelectStageCyclePatch
{
    // v0.113.0 起 SelectCharacter 只收按钮参数，角色模型要自己从按钮上取。
    [HarmonyPostfix]
    private static void Postfix(Control __instance, NCharacterSelectButton charSelectButton) =>
        CharSelectStageCycle.Attach(__instance, charSelectButton?.Character);
}

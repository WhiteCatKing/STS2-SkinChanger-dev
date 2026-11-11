using System;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// 角色选择界面右侧的「奥斯提预览框」。
///
/// 与角色皮肤预览小人并列，专门显示当前选中的奥斯提外观；所有角色（含以后新增）都显示同一个框、
/// 同一份选项。内容来源按优先级：
///   1. 玩家选了独立奥斯提 Mod 的皮肤 → 直接加载它的 display 资源（例如
///      <c>res://osty_defect/display_select.tres</c>）；
///   2. 否则加载本体 Mod 的标准奥斯提骨架 <c>res://animations/monsters/osty/osty_skel_data.tres</c>，
///      皮肤切换器的资源覆盖层会把这个路径换成当前选中皮肤自带的奥斯提。
/// 框本身可以像角色预览小人一样拖动，拖到信息面板左侧的停靠区即隐藏（停靠）。
/// </summary>
internal static class OstyModelPreviewControls
{
    internal const string PanelName = "STS2OstyModelPreview";
    private const string OstySkeletonPath = "res://animations/monsters/osty/osty_skel_data.tres";
    private const string OstyScenePath = "res://scenes/creature_visuals/osty.tscn";
    private const string ImpostorDisplayPath = "res://osty_defect/display_select.tres";

    internal static bool IsEnabled => CharacterModelPreviewControls.IsEnabled;

    public static void Refresh(NCharacterSelectScreen screen)
    {
        if (!IsEnabled)
        {
            Hide(screen);
            return;
        }

        var info = screen.GetNodeOrNull<Control>("InfoPanel");
        if (info == null)
        {
            return;
        }

        var panel = info.GetNodeOrNull<OstyModelPreviewPanel>(PanelName);
        if (panel == null)
        {
            panel = new OstyModelPreviewPanel { Name = PanelName, Visible = false };
            panel.Initialize(screen, info);
            info.AddChild(panel);
        }

        panel.ShowOsty();
    }

    public static void Hide(NCharacterSelectScreen screen) =>
        screen.GetNodeOrNull<OstyModelPreviewPanel>($"InfoPanel/{PanelName}")?.Suspend();


    /// <summary>
    /// 按当前选择加载奥斯提骨架：
    ///   1. 独立 Mod 皮肤 → 它自己的选人展示骨架；
    ///   2. 皮肤自带的 osty → 优先用**该皮肤自己提供的** <c>*skel_data*.tres</c>，
    ///      并用皮肤切换器的独立资源包加载，确保拿到的是这张皮肤的那一份（不是缓存的游戏原版）；
    ///   3. 都没有 → 标准 osty 骨架。
    /// </summary>
    internal static Resource? LoadSkeletonForCurrentSelection(string groupId, out string path)
    {
        path = OstySkeletonPath;
        if (!string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId))
        {
            path = ImpostorDisplayPath;
            // 按它自己代码的方式加载：显式类型 + CacheMode.Ignore。
            // 用泛型 Load<Resource>(path) 时子资源可能走缓存/延迟解析，导致
            // SpineSprite 一直拿不到骨架（预览框空白）。
            var typed = ResourceLoader.Load<Resource>(
                path, "SpineSkeletonDataResource", ResourceLoader.CacheMode.Ignore);
            if (typed != null && !IsSkeletonDataLoaded(typed))
            {
                ModLog.Warn($"奥斯提预览：{path} 首次加载后骨架仍未解析完成，重试一次。");
                typed = ResourceLoader.Load<Resource>(
                    path, "SpineSkeletonDataResource", ResourceLoader.CacheMode.Ignore) ?? typed;
            }

            return typed;
        }

        var option = string.IsNullOrEmpty(groupId) ? null : SkinService.GetCompanionSelectedOption(groupId);
        var own = option?.Assets.Keys.FirstOrDefault(key =>
            key.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) &&
            key.Contains("skel_data", StringComparison.OrdinalIgnoreCase));
        if (own != null)
        {
            path = own;
        }

        if (!string.IsNullOrEmpty(groupId))
        {
            var target = path;
            try
            {
                var loaded = SkinService.WithRuntimeResources(groupId, [target],
                    resources => resources.TryGetValue(target, out var resource) ? resource : null,
                    includeProviderDependencies: true);
                if (loaded != null)
                {
                    return loaded;
                }
            }
            catch (Exception exception)
            {
                ModLog.Warn($"奥斯提预览：按独立资源包加载 {target} 失败：{exception.GetBaseException().Message}");
            }
        }

        return ResourceLoader.Load<Resource>(path, null, ResourceLoader.CacheMode.IgnoreDeep);
    }

    /// <summary>
    /// 按当前选择加载「场景型」奥斯提外观：帧动画皮肤（例如 midori_black 的猫）只提供
    /// <c>res://scenes/creature_visuals/osty.tscn</c> 这样的场景，没有 Spine 骨架，
    /// 走骨架分支只能画回原版奥斯提。这里优先取该选项自己声明的规范场景。
    /// </summary>
    internal static bool TryLoadSceneForCurrentSelection(
        string groupId,
        out PackedScene? scene,
        out string path)
    {
        scene = null;
        path = string.Empty;
        if (string.IsNullOrEmpty(groupId) || !string.IsNullOrEmpty(SkinService.Config.CompanionImpostorSkinId))
        {
            return false;
        }

        var option = SkinService.GetCompanionSelectedOption(groupId);
        if (option == null)
        {
            return false;
        }

        var scenePath = option.Assets.Keys.FirstOrDefault(key =>
                             key.Equals(OstyScenePath, StringComparison.OrdinalIgnoreCase)) ??
                         option.Assets.Keys
                             .Where(key => key.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) &&
                                           key.Contains("/creature_visuals/", StringComparison.OrdinalIgnoreCase))
                             .OrderBy(key => key.Length)
                             .FirstOrDefault();
        if (scenePath == null)
        {
            return false;
        }

        path = scenePath;
        scene = LoadSceneForPreview(groupId, scenePath);
        if (scene != null)
        {
            return true;
        }

        path = string.Empty;
        return false;
    }

    private static PackedScene? LoadSceneForPreview(string groupId, string scenePath)
    {
        try
        {
            return SkinService.GetOrLoadRuntimeScene(groupId, scenePath);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"奥斯提预览：按独立资源包加载场景 {scenePath} 失败：{exception.GetBaseException().Message}");
        }

        // 运行时资源包对「完整运行时提供者」只带场景本身，帧图由已挂载的分组覆盖层解析；
        // 这里再按已挂载的包栈直接取一次，避免选人界面还没重建覆盖层时拿不到场景。
        try
        {
            return ResourceLoader.Load<PackedScene>(scenePath, null, ResourceLoader.CacheMode.Ignore);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"奥斯提预览：直接加载场景 {scenePath} 失败：{exception.GetBaseException().Message}");
            return null;
        }
    }

    /// <summary>骨架数据是否已经解析完成（SpineSprite 只有在这一步之后才会建骨架）。</summary>
    private static bool IsSkeletonDataLoaded(Resource data)
    {
        try
        {
            var obj = (GodotObject)data;
            if (!obj.HasMethod("is_skeleton_data_loaded"))
            {
                return true;
            }

            using var variant = obj.Call("is_skeleton_data_loaded");
            return variant.AsBool();
        }
        catch
        {
            return true;
        }
    }
}

internal partial class OstyModelPreviewPanel : Control
{
    private const float Gap = 1f;
    private const float GripWidth = CharacterModelPreviewControls.GripWidth;
    private const float ModelInset = CharacterModelPreviewControls.ModelInset;

    private NCharacterSelectScreen _screen = null!;
    private Control _info = null!;
    private Control _frame = null!;
    private NinePatchRect _background = null!;
    private Node2D _visualContainer = null!;
    private Button _dragHandle = null!;
    private Panel _dockHint = null!;
    private StyleBoxFlat _dockHintStyle = null!;
    private Node2D? _spine;
    private string? _loadedKey;
    private bool _sceneMode;
    private bool _docked;
    private bool _hintDragging;
    private bool _layoutQueued;
    private bool _refreshQueued;
    private Window _window = null!;

    internal void Initialize(NCharacterSelectScreen screen, Control info)
    {
        _screen = screen;
        _info = info;
        _frame = info.GetNodeOrNull<Control>("NinePatchRect") ?? info;
        MouseFilter = MouseFilterEnum.Pass;
        ClipContents = true;
        BuildInterface();
        BuildDockHint();
        DraggableSkinControl.AttachWithHandle(screen, this, _dragHandle,
            LoadPlacement, SavePlacement, ResetPlacement, ApplyDefaultPlacement, UpdateDockedFromDrag,
            OnDragStateChanged);
        _frame.ItemRectChanged += QueueLayout;
        _info.ItemRectChanged += QueueLayout;
        _screen.Resized += QueueLayout;
        VisibilityChanged += OnVisibilityChanged;
        _window = screen.GetWindow();
        _window.FocusExited += CancelPreviewDrag;
        TreeExiting += ReleaseDockHint;
    }

    private void BuildDockHint()
    {
        _dockHint = new Panel
        {
            Name = "OstyPreviewDockHint", Visible = false,
            MouseFilter = MouseFilterEnum.Ignore, FocusMode = FocusModeEnum.None
        };
        _dockHintStyle = new StyleBoxFlat
        {
            ContentMarginLeft = 0, ContentMarginRight = 0, ContentMarginTop = 0, ContentMarginBottom = 0,
            BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2
        };
        _dockHint.AddThemeStyleboxOverride("panel", _dockHintStyle);
        _info.AddChild(_dockHint);
        ModThemeRuntime.Bind(_dockHint, "preview_dock", ApplyDockHintStyle);
    }

    private void ApplyDockHintStyle(ModThemeSettings theme)
    {
        _dockHintStyle.BgColor = ModThemeRuntime.Tint(theme.AccentColor, _docked ? .32f : .14f);
        _dockHintStyle.BorderColor = ModThemeRuntime.Tint(theme.AccentColor, .95f);
        _dockHintStyle.CornerRadiusTopLeft = _dockHintStyle.CornerRadiusTopRight =
            _dockHintStyle.CornerRadiusBottomLeft = _dockHintStyle.CornerRadiusBottomRight =
                Math.Min(theme.CornerRadius, 6);
    }

    private void OnDragStateChanged(bool dragging)
    {
        _hintDragging = dragging;
        RefreshDockHint();
    }

    private void RefreshDockHint()
    {
        if (!GodotObject.IsInstanceValid(_dockHint))
        {
            return;
        }

        var area = Alive() && TryGetLayoutBounds(out var infoRect, out _)
            ? CharacterModelPreviewControls.ResolveDockHint(infoRect, _hintDragging, IsVisibleInTree()) : null;
        _dockHint.Visible = area != null;
        if (area is not { } rect)
        {
            return;
        }

        _dockHint.Position = rect.Position;
        _dockHint.Size = rect.Size;
        ApplyDockHintStyle(ModThemeRuntime.Current);
    }

    private void CancelPreviewDrag() => DraggableSkinControl.CancelDrag(this);

    private void ReleaseDockHint()
    {
        _hintDragging = false;
        if (GodotObject.IsInstanceValid(_window))
        {
            _window.FocusExited -= CancelPreviewDrag;
        }

        if (GodotObject.IsInstanceValid(_dockHint))
        {
            _dockHint.Hide();
            _dockHint.QueueFree();
        }
    }

    private void BuildInterface()
    {
        _background = new NinePatchRect { Name = "NinePatchRect", MouseFilter = MouseFilterEnum.Ignore };
        if (_frame is NinePatchRect source)
        {
            _background.Texture = source.Texture;
            _background.Modulate = source.Modulate * source.SelfModulate;
            _background.PatchMarginLeft = source.PatchMarginLeft;
            _background.PatchMarginRight = source.PatchMarginRight;
            _background.PatchMarginTop = source.PatchMarginTop;
            _background.PatchMarginBottom = source.PatchMarginBottom;
        }

        AddChild(_background);
        _background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _background.OffsetLeft = ModelInset;
        _visualContainer = new Node2D { Name = "VisualContainer" };
        AddChild(_visualContainer);
        _dragHandle = new Button
        {
            Name = "OstyPreviewDragHandle",
            FocusMode = FocusModeEnum.None,
            MouseFilter = MouseFilterEnum.Stop,
            MouseDefaultCursorShape = CursorShape.Move,
            AnchorTop = .1f,
            AnchorBottom = .9f,
            SelfModulate = new Color(1, 1, 1, _background.Modulate.A * _background.SelfModulate.A),
            OffsetRight = GripWidth
        };
        foreach (var (state, color) in new[] { ("normal", "000000"), ("hover", "161616"),
                     ("pressed", "303030"), ("focus", "161616"), ("disabled", "000000") })
        {
            _dragHandle.AddThemeStyleboxOverride(state, new StyleBoxFlat
            {
                BgColor = new Color(color),
                ContentMarginLeft = 0, ContentMarginRight = 0, ContentMarginTop = 0, ContentMarginBottom = 0,
                CornerRadiusTopLeft = 4, CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4, CornerRadiusBottomRight = 4
            });
        }

        AddChild(_dragHandle);
    }

    internal void ShowOsty()
    {
        QueueLayout();
        QueueRefresh();
    }

    internal void Suspend()
    {
        Visible = false;
        ClearModel();
    }

    private void OnVisibilityChanged()
    {
        RefreshDockHint();
        if (!IsVisibleInTree())
        {
            ClearModel();
        }
        else
        {
            QueueRefresh();
        }
    }

    private void SetDocked(bool docked)
    {
        if (_docked == docked)
        {
            return;
        }

        _docked = docked;
        _background.Visible = _visualContainer.Visible = !docked;
        MouseFilter = docked ? MouseFilterEnum.Ignore : MouseFilterEnum.Pass;
        if (docked)
        {
            ClearModel();
        }
        else
        {
            QueueRefresh();
        }
    }

    private void UpdateDockedFromDrag()
    {
        if (!TryGetLayoutBounds(out var infoRect, out _))
        {
            return;
        }

        var gripCenter = _info.GetGlobalTransformWithCanvas().AffineInverse() *
                         (_dragHandle.GetGlobalTransformWithCanvas() * (_dragHandle.Size / 2));
        SetDocked(CharacterModelPreviewControls.IsGripDocked(infoRect, gripCenter));
        RefreshDockHint();
    }

    private (float X, float Y)? LoadPlacement()
    {
        SetDocked(SkinService.IsOstyModelPreviewDocked());
        return _docked ? null : SkinService.GetOstyModelPreviewPosition();
    }

    private void SavePlacement(float x, float y)
    {
        SkinService.SetOstyModelPreviewPlacement(x, y, _docked);
        DraggableSkinControl.RefreshPlacement(this);
    }

    private void ResetPlacement()
    {
        SkinService.ResetOstyModelPreviewPosition();
        SetDocked(false);
    }

    private void QueueLayout()
    {
        if (_layoutQueued || !Alive())
        {
            return;
        }

        _layoutQueued = true;
        Callable.From(() =>
        {
            _layoutQueued = false;
            if (Alive())
            {
                ApplyLayout();
            }
        }).CallDeferred();
    }

    private void QueueRefresh()
    {
        if (_refreshQueued || !Alive())
        {
            return;
        }

        _refreshQueued = true;
        Callable.From(() =>
        {
            if (!Alive())
            {
                return;
            }

            try
            {
                RefreshModel();
            }
            catch (Exception exception)
            {
                ModLog.Warn("奥斯提预览刷新失败：" + exception.GetBaseException().Message);
            }
            finally
            {
                _refreshQueued = false;
            }
        }).CallDeferred();
    }

    /// <summary>
    /// 紧跟角色预览框的右边：X/宽度取角色预览框的真实矩形（同一父节点坐标系），
    /// Y 保持不变（与角色预览框对齐）。取不到时退回按 info 面板推算。
    /// </summary>
    private Rect2? ResolveLayout()
    {
        if (!TryGetLayoutBounds(out var infoRect, out var screenRect))
        {
            return null;
        }

        var sibling = _info.GetNodeOrNull<Control>("STS2CharacterModelPreview");
        if (sibling != null && GodotObject.IsInstanceValid(sibling) && sibling.Size.X > 8 && sibling.Size.Y > 8)
        {
            var x = sibling.Position.X + sibling.Size.X + Gap;
            return new Rect2(x, infoRect.Position.Y, sibling.Size.X, sibling.Size.Y);
        }

        var main = CharacterModelPreviewControls.ResolveLayout(infoRect, screenRect);
        var width = main?.Size.X ?? infoRect.Size.Y * (308f / 429f) + ModelInset;
        var height = main?.Size.Y ?? infoRect.Size.Y;
        var fallbackX = (main?.End.X ?? infoRect.End.X) + Gap;
        if (fallbackX + width > screenRect.End.X - 12)
        {
            fallbackX = Math.Max(infoRect.End.X + Gap, screenRect.End.X - 12 - width);
        }

        return new Rect2(fallbackX, infoRect.Position.Y, width, height);
    }

    private Rect2? GetDefaultLayout()
    {
        if (!TryGetLayoutBounds(out var infoRect, out _))
        {
            return null;
        }

        var expanded = ResolveLayout();
        if (!_docked)
        {
            return expanded;
        }

        if (CharacterModelPreviewControls.ResolveDockLayout(infoRect) is not { } dock)
        {
            return null;
        }

        return new Rect2(dock.Position, expanded?.Size ?? dock.Size);
    }

    private void ApplyLayout()
    {
        if (!GodotObject.IsInstanceValid(_frame))
        {
            return;
        }

        var area = GetDefaultLayout();
        Visible = area != null && OstyModelPreviewControls.IsEnabled;
        RefreshDockHint();
        if (area is not { } rect || !Visible)
        {
            return;
        }

        var resized = !Size.IsEqualApprox(rect.Size);
        Size = rect.Size;
        DraggableSkinControl.RefreshPlacement(this);
        if (resized)
        {
            FitModel();
        }
    }

    private void ApplyDefaultPlacement()
    {
        if (!Alive() || !GodotObject.IsInstanceValid(_frame) || GetDefaultLayout() is not { } rect)
        {
            return;
        }

        AnchorLeft = AnchorRight = AnchorTop = AnchorBottom = 0;
        Position = rect.Position;
    }

    private bool TryGetLayoutBounds(out Rect2 infoRect, out Rect2 screenRect)
    {
        infoRect = screenRect = default;
        var infoTransform = _info.GetGlobalTransformWithCanvas();
        if (!infoTransform.IsFinite() || Mathf.IsZeroApprox(infoTransform.Determinant()))
        {
            return false;
        }

        var inverse = infoTransform.AffineInverse();
        infoRect = (inverse * _frame.GetGlobalTransformWithCanvas()) * new Rect2(Vector2.Zero, _frame.Size);
        screenRect = (inverse * _screen.GetGlobalTransformWithCanvas()) * new Rect2(Vector2.Zero, _screen.Size);
        return true;
    }

    private void RefreshModel()
    {
        if (!OstyModelPreviewControls.IsEnabled || !IsVisibleInTree() || _docked)
        {
            return;
        }

        var groupId = SkinService.CompanionGroupIds.FirstOrDefault() ?? string.Empty;
        var impostorSkinId = SkinService.Config.CompanionImpostorSkinId;
        var key = (impostorSkinId ?? SkinService.GetCompanionSelection(groupId)) + "|" + groupId;
        if (key == _loadedKey && _spine != null && GodotObject.IsInstanceValid(_spine))
        {
            return;
        }

        ClearModel();

        // 独立奥斯提 Mod 的皮肤：优先用它自己的展示工厂，稳（它会校验骨架与动画）。
        if (!string.IsNullOrEmpty(impostorSkinId))
        {
            var own = OstyImpostorBridge.CreateSelectDisplay();
            if (own != null)
            {
                own.Name = "OstyPreviewSpine";
                _visualContainer.AddChild(own);
                // 进树后再补一次 rebuild：它的展示骨架是在树内才真正建好的。
                TryCall(own, "set_time_scale", 1.0);
                TryCall(own, "set_update_mode", 0);
                TryCall(own, "update_skeleton", 0f);
                PlayNamedAnimation(own, OstyImpostorBridge.SelectDisplayAnimation);
                _spine = own;
                _loadedKey = key;
                _fitAttempts = 0;
                ScheduleFit();
                ModLog.Info("奥斯提预览：用 OstyDefectImpostor 自己的展示工厂创建成功。");
                return;
            }
        }

        // 帧动画皮肤（只提供 creature_visuals 场景、没有 Spine 骨架）：
        // 必须按场景实例化，否则骨架分支只会画出原版奥斯提。
        if (OstyModelPreviewControls.TryLoadSceneForCurrentSelection(groupId, out var scene, out var scenePath))
        {
            if (TryShowScene(scene!, key))
            {
                ModLog.Info($"奥斯提预览：用场景型外观 {scenePath}。");
                return;
            }
        }

        var data = OstyModelPreviewControls.LoadSkeletonForCurrentSelection(groupId, out var path);
        if (data == null)
        {
            ModLog.Warn($"奥斯提预览：加载骨架失败（{path}）。");
            return;
        }

        ModLog.Info($"奥斯提预览：加载 {path}，类型={data.GetClass()}。");

        using var created = ClassDB.Instantiate("SpineSprite");
        if (created.AsGodotObject() is not Node2D spine)
        {
            ModLog.Warn("奥斯提预览：无法实例化 SpineSprite。");
            return;
        }

        spine.Name = "OstyPreviewSpine";
        ((GodotObject)spine).Set("skeleton_data_res", data);
        _visualContainer.AddChild(spine);
        // 让骨架真正每帧推进（不设这两项时 SpineSprite 可能停在绑定位姿，看起来就是"静态图"）。
        TryCall(spine, "set_time_scale", 1.0);
        TryCall(spine, "set_update_mode", 0);
        if (((GodotObject)spine).HasMethod("update_skeleton"))
        {
            using var ignored = ((GodotObject)spine).Call("update_skeleton", 0f);
        }

        PlayFirstAnimation(spine, data);
        _spine = spine;
        _loadedKey = key;
        // 骨架是异步建好的：多等几帧，直到拿到有效包围盒再摆正/缩放。
        _fitAttempts = 0;
        ScheduleFit();
    }

    private int _fitAttempts;
    private bool _loggedFit;
    private string? _pendingAnimation;
    private bool _animationApplied;

    private void ScheduleFit()
    {
        _fitAttempts++;
        Callable.From(() =>
        {
            if (!Alive() || _spine == null || !GodotObject.IsInstanceValid(_spine))
            {
                return;
            }

            TryApplyPendingAnimation();
            // 骨架迟迟不建：每隔几帧主动 rebuild 一次（部分展示骨架只在树内、被显式
            // update_skeleton 之后才会生成）。场景型外观没有骨架，跳过。
            if (!_sceneMode && _fitAttempts % 6 == 1)
            {
                TryCall(_spine, "update_skeleton", 0f);
            }

            if (FitModel())
            {
                _fitAttempts = 0;
                return;
            }

            if (_fitAttempts < 30)
            {
                ScheduleFit();
                return;
            }

            // 30 帧还拿不到包围盒：给一个保守的默认缩放，至少让模型出现在框里。
            try
            {
                _spine.Scale = Vector2.One * 0.4f;
                _spine.Position = Size / 2f;
                ModLog.Warn("奥斯提预览：30 帧内没拿到骨架包围盒，改用默认缩放。");
            }
            catch
            {
            }
        }).CallDeferred();
    }

    private static void TryCall(Node2D spine, string method, params Variant[] args)
    {
        try
        {
            var obj = (GodotObject)spine;
            if (obj.HasMethod(method))
            {
                using var ignored = obj.Call(method, args);
            }
        }
        catch
        {
        }
    }


    /// <summary>骨架就绪后再播动画（骨架是异步建好的，建好前拿不到动画状态）。</summary>
    private void TryApplyPendingAnimation()
    {
        if (_animationApplied || string.IsNullOrEmpty(_pendingAnimation) || _spine == null)
        {
            return;
        }

        try
        {
            using var stateVariant = ((GodotObject)_spine).Call("get_animation_state");
            var state = stateVariant.AsGodotObject();
            if (state == null || !GodotObject.IsInstanceValid(state))
            {
                return;
            }

            using var ignored = state.Call("set_animation", _pendingAnimation, true, 0);
            _animationApplied = true;
        }
        catch
        {
        }
    }

    private void PlayFirstAnimation(Node2D spine, Resource data)
    {
        _pendingAnimation = ResolveAnimationName(data);
        TryApplyPendingAnimation();
    }

    /// <summary>从骨架数据里挑一个待机动画名（get_animations 返回的是 SpineAnimation 对象）。</summary>
    private static string? ResolveAnimationName(Resource data)
    {
        try
        {
            using var animationsVariant = data.Call("get_animations");
            var animations = animationsVariant.AsGodotArray();
            string? chosen = null;
            for (var i = 0; i < animations.Count; i++)
            {
                var animation = animations[i].AsGodotObject();
                if (animation == null || !GodotObject.IsInstanceValid(animation))
                {
                    continue;
                }

                using var nameVariant = animation.Call("get_name");
                var name = nameVariant.AsString();
                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                if (chosen == null)
                {
                    chosen = name;
                }

                if (name.Contains("idle", StringComparison.OrdinalIgnoreCase) ||
                    name.Contains("animation", StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }

            return chosen;
        }
        catch
        {
            return null;
        }
    }

    private void PlayNamedAnimation(Node2D spine, string name)
    {
        _pendingAnimation = name;
        TryApplyPendingAnimation();
    }

    /// <summary>把场景型外观实例化进预览框；场景里没有任何可绘制的精灵时返回 false。</summary>
    private bool TryShowScene(PackedScene scene, string key)
    {
        Node2D? instance;
        try
        {
            instance = scene.Instantiate<Node2D>(PackedScene.GenEditState.Disabled);
        }
        catch (Exception exception)
        {
            ModLog.Warn($"奥斯提预览：实例化场景型外观失败：{exception.GetBaseException().Message}");
            return false;
        }

        if (instance == null)
        {
            return false;
        }

        if (!HasDrawableNode(instance))
        {
            ModLog.Warn($"奥斯提预览：场景 {scene.ResourcePath} 里没有精灵节点，改用骨架分支。");
            instance.QueueFree();
            return false;
        }

        instance.Name = "OstyPreviewScene";
        _visualContainer.AddChild(instance);
        _sceneMode = true;
        _spine = instance;
        _loadedKey = key;
        _fitAttempts = 0;
        PlaySceneIdleAnimation(instance);
        ScheduleFit();
        return true;
    }

    private static bool HasDrawableNode(Node root) =>
        root.FindChildren("*", "AnimatedSprite2D", true, false).Count > 0 ||
        root.FindChildren("*", "Sprite2D", true, false).Count > 0;

    /// <summary>场景型外观的待机动画：挑一个含 idle/待机 的动画，否则用它的当前/第一个动画。</summary>
    private static void PlaySceneIdleAnimation(Node root)
    {
        try
        {
            foreach (var node in root.FindChildren("*", "AnimatedSprite2D", true, false))
            {
                if (node is not AnimatedSprite2D sprite || sprite.SpriteFrames == null)
                {
                    continue;
                }

                var frames = sprite.SpriteFrames;
                var names = frames.GetAnimationNames();
                if (names.Length == 0)
                {
                    continue;
                }

                var current = sprite.Animation.ToString();
                var chosen = names.FirstOrDefault(name =>
                                 name.Contains("idle", StringComparison.OrdinalIgnoreCase) ||
                                 name.Contains("待机", StringComparison.OrdinalIgnoreCase)) ??
                             names.FirstOrDefault(name => name == current);
                sprite.Play(string.IsNullOrEmpty(chosen) ? names[0] : chosen);
                return;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn($"奥斯提预览：播放场景型外观的待机动画失败：{exception.GetBaseException().Message}");
        }
    }

    /// <summary>
    /// 场景型外观的包围盒：优先用 creature_visuals 约定的 <c>Bounds</c> 控件（与局内一致），
    /// 没有就按所有精灵的帧贴图算出局部矩形。
    /// </summary>
    private static Rect2 MeasureSceneBounds(Node2D root)
    {
        var boundsNode = root.GetNodeOrNull<Control>("Bounds");
        if (boundsNode != null && boundsNode.Size.X > 1f && boundsNode.Size.Y > 1f)
        {
            return new Rect2(boundsNode.Position, boundsNode.Size);
        }

        Transform2D inverse;
        try
        {
            inverse = root.GetGlobalTransform().AffineInverse();
        }
        catch
        {
            return default;
        }

        var result = new Rect2();
        var any = false;
        void Accumulate(Node2D node, Rect2 local)
        {
            if (local.Size.X <= 1f || local.Size.Y <= 1f)
            {
                return;
            }

            var rect = (inverse * node.GetGlobalTransform()) * local;
            result = any ? result.Merge(rect) : rect;
            any = true;
        }

        foreach (var node in root.FindChildren("*", "Sprite2D", true, false))
        {
            if (node is Sprite2D sprite && sprite.Texture != null)
            {
                Accumulate(sprite, sprite.GetRect());
            }
        }

        foreach (var node in root.FindChildren("*", "AnimatedSprite2D", true, false))
        {
            if (node is not AnimatedSprite2D sprite || sprite.SpriteFrames == null)
            {
                continue;
            }

            var frames = sprite.SpriteFrames;
            var animation = sprite.Animation;
            if (!frames.HasAnimation(animation))
            {
                var names = frames.GetAnimationNames();
                if (names.Length == 0)
                {
                    continue;
                }

                animation = names[0];
            }

            var texture = frames.GetFrameTexture(animation, 0);
            if (texture == null)
            {
                continue;
            }

            var size = texture.GetSize();
            Accumulate(sprite, new Rect2(
                sprite.Offset - (sprite.Centered ? size / 2f : Vector2.Zero),
                size));
        }

        return any ? result : default;
    }

    private bool FitScene()
    {
        if (_spine is not { } root || !GodotObject.IsInstanceValid(root) || !Alive())
        {
            return false;
        }

        var bounds = MeasureSceneBounds(root);
        var size = bounds.Size;
        if (size.X <= 1f || size.Y <= 1f)
        {
            return false;
        }

        var box = Size;
        var availableW = Math.Max(32f, box.X - ModelInset * 2);
        var availableH = Math.Max(32f, box.Y);
        var scale = Math.Min(availableW / size.X, availableH / size.Y) * 0.85f;
        root.Scale = Vector2.One * scale;
        root.Position = new Vector2(
            box.X / 2f - bounds.GetCenter().X * scale,
            box.Y / 2f - bounds.GetCenter().Y * scale);
        if (!_loggedFit)
        {
            _loggedFit = true;
            ModLog.Info($"奥斯提预览：场景包围盒={bounds.Size}，缩放={scale:F4}，位置={root.Position}。");
        }

        return true;
    }

    /// <summary>按骨架包围盒把奥斯提缩放到框内并居中。返回 false 表示骨架还没就绪。</summary>
    private bool FitModel()
    {
        if (_sceneMode)
        {
            return FitScene();
        }
        if (_spine == null || !GodotObject.IsInstanceValid(_spine) || !Alive())
        {
            return false;
        }

        try
        {
            using var skeletonVariant = ((GodotObject)_spine).Call("get_skeleton");
            var skeleton = skeletonVariant.AsGodotObject();
            if (skeleton == null || !GodotObject.IsInstanceValid(skeleton))
            {
                return false;
            }

            using var boundsVariant = skeleton.Call("get_bounds");
            var bounds = boundsVariant.AsRect2();
            var size = bounds.Size;
            if (size.X <= 1f || size.Y <= 1f)
            {
                return false;
            }

            var box = Size;
            var availableW = Math.Max(32f, box.X - ModelInset * 2);
            var availableH = Math.Max(32f, box.Y);
            var scale = Math.Min(availableW / size.X, availableH / size.Y) * 0.85f;
            _spine.Scale = Vector2.One * scale;
            _spine.Position = new Vector2(
                box.X / 2f - bounds.GetCenter().X * scale,
                box.Y / 2f - bounds.GetCenter().Y * scale);
            if (!_loggedFit)
            {
                _loggedFit = true;
                ModLog.Info($"奥斯提预览：骨架包围盒={bounds.Size}，缩放={scale:F4}，位置={_spine.Position}。");
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ClearModel()
    {
        if (GodotObject.IsInstanceValid(_visualContainer))
        {
            foreach (var child in _visualContainer.GetChildren())
            {
                _visualContainer.RemoveChild(child);
                child.QueueFree();
            }
        }

        _spine = null;
        _loadedKey = null;
        _sceneMode = false;
        _loggedFit = false;
        _pendingAnimation = null;
        _animationApplied = false;
    }

    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(_frame))
        {
            _frame.ItemRectChanged -= QueueLayout;
        }

        if (GodotObject.IsInstanceValid(_info))
        {
            _info.ItemRectChanged -= QueueLayout;
        }

        if (GodotObject.IsInstanceValid(_screen))
        {
            _screen.Resized -= QueueLayout;
        }

        VisibilityChanged -= OnVisibilityChanged;
    }

    private bool Alive() => GodotObject.IsInstanceValid(this) && IsInsideTree() && !IsQueuedForDeletion();
}

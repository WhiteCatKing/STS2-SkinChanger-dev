using System.Globalization;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Drives metadata-authored Spine touch/drag feedback (<c>Touch_Box_*</c> controls with
/// <c>ClickAnim</c>/<c>TargetBone</c>/<c>MaxDragRadius</c>/<c>Drag_Limit</c> metadata, as used by
/// AncientWaifus' Orobas and Tezcatara scenes) directly on Skin Changer's managed Ancient scenes.
///
/// Attachment is invoked explicitly from <c>ManagedAncientSceneAnimation.TryStart</c> rather than
/// subscribing to global <see cref="SceneTree.NodeAdded"/>, and <see cref="SceneTree.ProcessFrame"/>
/// is subscribed lazily only while at least one interactive touch box is active. Each registered
/// <c>TouchBoxEntry</c> caches its owning <c>SpineSprite</c> state, so Spine bone callbacks fast-skip
/// <c>GetSkeleton()</c> when idle and restore applied bone offsets directly from recorded bone
/// references without a second <c>FindBone</c> or scene-tree lookup.
/// </summary>
internal static class InteractiveTouchBoxBridge
{
    internal const float DefaultIntroDurationSeconds = 2.0f;
    internal const float BoneIdleEpsilonSq = 0.01f;
    private const int OverlayTrackIndex = 1;

    private readonly record struct AppliedBoneOffset(GodotObject BoundBone, float OffsetX, float OffsetY);

    private sealed class SpriteTouchState(Node sprite, ulong spriteId)
    {
        public Node Sprite { get; } = sprite;
        public ulong SpriteId { get; } = spriteId;
        public List<TouchBoxEntry> Entries { get; } = [];
        public List<AppliedBoneOffset> AppliedBones { get; } = [];
        public bool IsHooked { get; set; }
        public bool? HasDummy { get; set; }
        public ulong BlockedUntilMsec { get; set; }
    }

    private sealed class TouchBoxEntry
    {
        public required Control Box { get; init; }
        public required Node Root { get; init; }
        public required ulong RootId { get; init; }
        public required SpriteTouchState SpriteState { get; init; }
        public required string ClickAnim { get; init; }
        public required string TargetBoneName { get; init; }
        public bool HasTargetBone => !string.IsNullOrEmpty(TargetBoneName);
        public bool IsSingleAnim { get; init; }
        public bool IsOneShot { get; init; }
        public bool IsAbsoluteDrag { get; init; }
        public float MaxDragRadius { get; init; }
        public float DragSpeed { get; init; }
        public required string DragAxisMode { get; init; }
        public float? MixIn { get; init; }
        public float? MixStartToLoop { get; init; }
        public float? MixLoopToEnd { get; init; }
        public float MixOut { get; init; }
        public bool HasDragLimitBox { get; init; }
        public Vector2 DragLimitMin { get; init; }
        public Vector2 DragLimitMax { get; init; }
        public int Priority { get; init; }
        public bool IsHolding { get; set; }
        public Vector2 StartMousePos { get; set; }
        public Vector2 DragOffset { get; set; }
        public Vector2 TargetDragOffset { get; set; }
    }

    private static SceneTree? _subscribedTree;
    private static readonly List<TouchBoxEntry> ActiveTouches = [];
    private static readonly Dictionary<ulong, SpriteTouchState> SpriteStates = [];
    private static readonly HashSet<ulong> AttachedRootIds = [];
    private static readonly Dictionary<ulong, ulong> BlockedRootUntilMsec = [];
    private static bool _wasLeftMouseDown;

    internal static void AttachToScene(string? groupId, Node sceneRoot, Node spineNode)
    {
        if (!GodotObject.IsInstanceValid(sceneRoot) || !GodotObject.IsInstanceValid(spineNode))
        {
            return;
        }

        try
        {
            var touchBoxes = CollectTouchBoxes(sceneRoot);
            if (touchBoxes.Count == 0)
            {
                return;
            }

            var rootId = sceneRoot.GetInstanceId();
            var spriteId = spineNode.GetInstanceId();
            if (!SpriteStates.TryGetValue(spriteId, out var spriteState))
            {
                spriteState = new SpriteTouchState(spineNode, spriteId);
                SpriteStates[spriteId] = spriteState;
            }

            var attachedCount = 0;
            foreach (var box in touchBoxes)
            {
                if (RegisterTouchBox(box, sceneRoot, rootId, spriteState))
                {
                    attachedCount++;
                }
            }

            if (attachedCount == 0)
            {
                if (spriteState.Entries.Count == 0)
                {
                    SpriteStates.Remove(spriteId);
                }

                return;
            }

            if (AttachedRootIds.Add(rootId))
            {
                sceneRoot.Connect(
                    Node.SignalName.TreeExited,
                    Callable.From(() => DetachScene(rootId)),
                    (uint)GodotObject.ConnectFlags.OneShot);
            }

            EnsureProcessFrameSubscribed(sceneRoot);
            ModLog.Info(
                $"已为 {groupId ?? sceneRoot.Name.ToString()} 挂载 {attachedCount} 个先古交互触摸区域。");
        }
        catch (Exception exception)
        {
            ModLog.Warn("交互触摸区域挂载失败：" + exception.GetBaseException().Message);
        }
    }

    internal static void NotifyIntroStarted(Node sceneRoot)
    {
        if (!GodotObject.IsInstanceValid(sceneRoot))
        {
            return;
        }

        var rootId = sceneRoot.GetInstanceId();
        if (!AttachedRootIds.Contains(rootId))
        {
            return;
        }

        var duration = ReadIntroDurationSeconds(sceneRoot);
        if (duration > 0f)
        {
            BlockedRootUntilMsec[rootId] = Time.GetTicksMsec() + (ulong)(duration * 1000f);
        }
        else
        {
            BlockedRootUntilMsec.Remove(rootId);
        }
    }

    internal static bool IsManagedTouchScenePath(string? sceneFilePath) =>
        !string.IsNullOrWhiteSpace(sceneFilePath) &&
        sceneFilePath.StartsWith("res://sts2_skin_runtime/", StringComparison.OrdinalIgnoreCase);

    internal static bool IsTouchBoxName(string? nodeName) =>
        !string.IsNullOrEmpty(nodeName) &&
        nodeName.StartsWith("Touch_Box_", StringComparison.Ordinal);

    internal static bool NeedsBoneUpdate(bool hasTargetBone, bool isHolding, Vector2 dragOffset) =>
        hasTargetBone && (isHolding || dragOffset.LengthSquared() >= BoneIdleEpsilonSq);

    internal static (Vector2 NextOffset, bool ShouldApply) ComputeNextDragOffset(
        Vector2 currentOffset,
        Vector2 targetOffset,
        float dragSpeed,
        bool isHolding)
    {
        var next = isHolding
            ? currentOffset.Lerp(targetOffset, dragSpeed)
            : currentOffset.Lerp(Vector2.Zero, dragSpeed * 0.5f);
        if (next.LengthSquared() < BoneIdleEpsilonSq)
        {
            return (isHolding ? next : Vector2.Zero, false);
        }

        return (next, true);
    }

    internal static float ResolveIntroDurationSeconds(string? rawValue) =>
        !string.IsNullOrWhiteSpace(rawValue) &&
        float.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) &&
        parsed >= 0f
            ? parsed
            : DefaultIntroDurationSeconds;

    internal static (float X, float Y) MapDragOffsetToBoneAxes(string? dragAxisMode, float dragX, float dragY)
    {
        if (string.Equals(dragAxisMode, "Direct", StringComparison.Ordinal))
        {
            return (dragX, dragY);
        }

        if (string.Equals(dragAxisMode, "DirectInverted", StringComparison.Ordinal))
        {
            return (-dragX, -dragY);
        }

        if (string.Equals(dragAxisMode, "Rotated90Inverted", StringComparison.Ordinal))
        {
            return (dragY, dragX);
        }

        return (-dragY, -dragX);
    }

    internal static Vector2 ClampDragOffset(
        Vector2 rawOffset,
        bool hasDragLimitBox,
        Vector2 dragLimitMin,
        Vector2 dragLimitMax,
        float maxDragRadius)
    {
        if (hasDragLimitBox)
        {
            return new Vector2(
                Mathf.Clamp(rawOffset.X, dragLimitMin.X, dragLimitMax.X),
                Mathf.Clamp(rawOffset.Y, dragLimitMin.Y, dragLimitMax.Y));
        }

        return maxDragRadius > 0f && rawOffset.Length() > maxDragRadius
            ? rawOffset.Normalized() * maxDragRadius
            : rawOffset;
    }

    internal static bool ShouldPreferCandidate(
        int candidatePriority,
        int candidateZIndex,
        int bestPriority,
        int bestZIndex) =>
        candidatePriority > bestPriority ||
        (candidatePriority == bestPriority && candidateZIndex >= bestZIndex);

    private static List<Control> CollectTouchBoxes(Node root)
    {
        var boxes = new List<Control>();
        var stack = new Stack<Node>();
        foreach (var child in root.GetChildren())
        {
            stack.Push(child);
        }

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (current is Control control &&
                IsTouchBoxName(control.Name.ToString()) &&
                HasInteractionMetadata(control))
            {
                boxes.Add(control);
            }

            var childCount = current.GetChildCount();
            for (var i = 0; i < childCount; i++)
            {
                stack.Push(current.GetChild(i));
            }
        }

        return boxes;
    }

    private static bool RegisterTouchBox(
        Control box,
        Node root,
        ulong rootId,
        SpriteTouchState spriteState)
    {
        if (ActiveTouches.Any(existing => ReferenceEquals(existing.Box, box)))
        {
            return false;
        }

        var clickAnim = ReadStringMeta(box, "ClickAnim", "Idle");
        var targetBone = ReadStringMeta(box, "TargetBone", string.Empty);
        var isSingleAnim = ReadBoolMeta(box, "IsSingleAnim", false);
        var isOneShot = ReadBoolMeta(box, "IsOneShot", false);
        var isAbsoluteDrag = ReadBoolMeta(box, "IsAbsoluteDrag", false);
        var maxDragRadius = ReadFloatMeta(box, "MaxDragRadius", 200f);
        var dragSpeed = ReadFloatMeta(box, "DragSpeed", 0.2f);
        var dragAxisMode = ReadStringMeta(box, "DragAxisMode", "Rotated90");
        var mixIn = ReadOptionalFloatMeta(box, "MixIn");
        var mixStartToLoop = ReadOptionalFloatMeta(box, "MixStartToLoop");
        var mixLoopToEnd = ReadOptionalFloatMeta(box, "MixLoopToEnd");
        var mixOut = ReadFloatMeta(box, "MixOut", 0.3f);
        var priority = ReadIntMeta(box, "Priority", 0);

        var hasDragLimitBox = false;
        var dragLimitMin = Vector2.Zero;
        var dragLimitMax = Vector2.Zero;
        if (box.GetNodeOrNull<Control>("Drag_Limit") is { } dragLimit)
        {
            hasDragLimitBox = true;
            dragLimitMin = -dragLimit.PivotOffset;
            dragLimitMax = dragLimit.Size - dragLimit.PivotOffset;
        }

        var entry = new TouchBoxEntry
        {
            Box = box,
            Root = root,
            RootId = rootId,
            SpriteState = spriteState,
            ClickAnim = clickAnim,
            TargetBoneName = targetBone,
            IsSingleAnim = isSingleAnim,
            IsOneShot = isOneShot,
            IsAbsoluteDrag = isAbsoluteDrag,
            MaxDragRadius = maxDragRadius,
            DragSpeed = dragSpeed,
            DragAxisMode = dragAxisMode,
            MixIn = mixIn,
            MixStartToLoop = mixStartToLoop,
            MixLoopToEnd = mixLoopToEnd,
            MixOut = mixOut,
            HasDragLimitBox = hasDragLimitBox,
            DragLimitMin = dragLimitMin,
            DragLimitMax = dragLimitMax,
            Priority = priority,
        };

        ActiveTouches.Add(entry);
        spriteState.Entries.Add(entry);
        return true;
    }

    private static void DetachScene(ulong rootId)
    {
        AttachedRootIds.Remove(rootId);
        BlockedRootUntilMsec.Remove(rootId);

        for (var i = ActiveTouches.Count - 1; i >= 0; i--)
        {
            var entry = ActiveTouches[i];
            if (entry.RootId != rootId)
            {
                continue;
            }

            RemoveTouchEntryAt(i);
        }

        if (ActiveTouches.Count == 0)
        {
            UnsubscribeProcessFrame();
        }
    }

    private static void RemoveTouchEntryAt(int index)
    {
        var entry = ActiveTouches[index];
        ActiveTouches.RemoveAt(index);

        var spriteState = entry.SpriteState;
        spriteState.Entries.Remove(entry);
        if (spriteState.Entries.Count == 0)
        {
            spriteState.AppliedBones.Clear();
            SpriteStates.Remove(spriteState.SpriteId);
        }
    }

    private static void EnsureProcessFrameSubscribed(Node sceneRoot)
    {
        var tree = sceneRoot.GetTree() ?? Engine.GetMainLoop() as SceneTree;
        if (tree == null || !GodotObject.IsInstanceValid(tree))
        {
            return;
        }

        if (ReferenceEquals(_subscribedTree, tree))
        {
            return;
        }

        UnsubscribeProcessFrame();
        _subscribedTree = tree;
        tree.ProcessFrame += OnFrame;
    }

    private static void UnsubscribeProcessFrame()
    {
        if (_subscribedTree != null && GodotObject.IsInstanceValid(_subscribedTree))
        {
            _subscribedTree.ProcessFrame -= OnFrame;
        }

        _subscribedTree = null;
        _wasLeftMouseDown = false;
    }

    private static bool HasInteractionMetadata(GodotObject obj) =>
        obj.HasMeta("ClickAnim") ||
        obj.HasMeta("TargetBone") ||
        obj.HasMeta("MaxDragRadius") ||
        obj.HasMeta("DragSpeed") ||
        obj.HasMeta("IsAbsoluteDrag");

    private static void OnFrame()
    {
        try
        {
            ProcessTouchInput();
        }
        catch (Exception exception)
        {
            ModLog.Warn("交互触摸更新失败：" + exception.GetBaseException().Message);
        }
    }

    private static void ProcessTouchInput()
    {
        PruneInvalidEntries();
        if (ActiveTouches.Count == 0)
        {
            UnsubscribeProcessFrame();
            return;
        }

        var isLeftDown = Input.IsMouseButtonPressed(MouseButton.Left);
        if (isLeftDown && !_wasLeftMouseDown)
        {
            HandlePress();
        }
        else if (isLeftDown && _wasLeftMouseDown)
        {
            HandleDrag();
        }
        else if (!isLeftDown && _wasLeftMouseDown)
        {
            HandleRelease();
        }

        _wasLeftMouseDown = isLeftDown;
    }

    private static void PruneInvalidEntries()
    {
        for (var i = ActiveTouches.Count - 1; i >= 0; i--)
        {
            var entry = ActiveTouches[i];
            if (!GodotObject.IsInstanceValid(entry.Box) ||
                !GodotObject.IsInstanceValid(entry.Root) ||
                !GodotObject.IsInstanceValid(entry.SpriteState.Sprite))
            {
                var rootId = entry.RootId;
                RemoveTouchEntryAt(i);
                if (!ActiveTouches.Any(other => other.RootId == rootId))
                {
                    AttachedRootIds.Remove(rootId);
                    BlockedRootUntilMsec.Remove(rootId);
                }
            }
        }
    }

    private static void HandlePress()
    {
        var nowMsec = Time.GetTicksMsec();
        TouchBoxEntry? best = null;
        MegaAnimationState? bestAnimState = null;
        var bestPriority = int.MinValue;
        var bestZIndex = int.MinValue;

        foreach (var entry in ActiveTouches)
        {
            var box = entry.Box;
            if (!box.IsInsideTree() || !box.IsVisibleInTree() || entry.IsHolding)
            {
                continue;
            }

            if (BlockedRootUntilMsec.TryGetValue(entry.RootId, out var blockedUntil) &&
                nowMsec < blockedUntil)
            {
                continue;
            }

            var spriteState = entry.SpriteState;
            if (nowMsec < spriteState.BlockedUntilMsec)
            {
                continue;
            }

            var localMouse = box.GetLocalMousePosition();
            if (!new Rect2(Vector2.Zero, box.Size).HasPoint(localMouse))
            {
                continue;
            }

            if (!ShouldPreferCandidate(entry.Priority, box.ZIndex, bestPriority, bestZIndex))
            {
                continue;
            }

            var animState = new MegaSprite(spriteState.Sprite).TryGetAnimationState();
            if (animState == null)
            {
                continue;
            }

            best = entry;
            bestAnimState = animState;
            bestPriority = entry.Priority;
            bestZIndex = box.ZIndex;
        }

        if (best == null || bestAnimState == null)
        {
            return;
        }

        var bestSpriteState = best.SpriteState;
        if (best.HasTargetBone)
        {
            EnsureSpriteBoneHook(bestSpriteState);
        }

        best.IsHolding = true;
        best.StartMousePos = best.Box.GetGlobalMousePosition();
        if (best.IsAbsoluteDrag)
        {
            var center = best.Box.GlobalPosition + (best.Box.Size / 2f);
            var delta = best.StartMousePos - center;
            best.TargetDragOffset = ClampDragOffset(
                delta,
                best.HasDragLimitBox,
                best.DragLimitMin,
                best.DragLimitMax,
                best.MaxDragRadius);
        }
        else
        {
            best.TargetDragOffset = Vector2.Zero;
        }

        best.DragOffset = Vector2.Zero;
        StartOverlayTrack(bestSpriteState, bestAnimState);

        if (best.IsOneShot)
        {
            var track = AddAnimation(bestAnimState, best.ClickAnim, delay: 0f, loop: false, OverlayTrackIndex);
            var animEnd = 0f;
            if (track != null)
            {
                ApplyMix(track, best.MixIn);
                animEnd = track.GetAnimationEnd();
                DisposeIfNeeded(track);
            }

            bestAnimState.BoundObject.Call("add_empty_animation", OverlayTrackIndex, best.MixOut, 0f);
            bestSpriteState.BlockedUntilMsec =
                Time.GetTicksMsec() + (ulong)((animEnd + best.MixOut) * 1000f);
            return;
        }

        if (best.IsSingleAnim)
        {
            var track = AddAnimation(bestAnimState, best.ClickAnim, delay: 0f, loop: true, OverlayTrackIndex);
            if (track != null)
            {
                ApplyMix(track, best.MixIn);
                DisposeIfNeeded(track);
            }

            return;
        }

        var startTrack = AddAnimation(bestAnimState, best.ClickAnim + "_Start", delay: 0f, loop: false, OverlayTrackIndex);
        if (startTrack != null)
        {
            ApplyMix(startTrack, best.MixIn);
            DisposeIfNeeded(startTrack);
        }

        var loopTrack = AddAnimation(bestAnimState, best.ClickAnim + "_Loop", delay: 0f, loop: true, OverlayTrackIndex);
        if (loopTrack != null)
        {
            ApplyMix(loopTrack, best.MixStartToLoop);
            DisposeIfNeeded(loopTrack);
        }
    }

    private static void HandleDrag()
    {
        foreach (var entry in ActiveTouches)
        {
            if (!entry.IsHolding || !entry.Box.IsInsideTree())
            {
                continue;
            }

            var mousePos = entry.Box.GetGlobalMousePosition();
            var rawDelta = entry.IsAbsoluteDrag
                ? mousePos - (entry.Box.GlobalPosition + (entry.Box.Size / 2f))
                : mousePos - entry.StartMousePos;
            entry.TargetDragOffset = ClampDragOffset(
                rawDelta,
                entry.HasDragLimitBox,
                entry.DragLimitMin,
                entry.DragLimitMax,
                entry.MaxDragRadius);
        }
    }

    private static void HandleRelease()
    {
        foreach (var entry in ActiveTouches)
        {
            if (!entry.IsHolding)
            {
                continue;
            }

            entry.IsHolding = false;
            entry.TargetDragOffset = Vector2.Zero;
            if (entry.IsOneShot)
            {
                continue;
            }

            var spriteState = entry.SpriteState;
            var animState = new MegaSprite(spriteState.Sprite).TryGetAnimationState();
            if (animState == null)
            {
                continue;
            }

            var endDuration = 0f;
            if (!entry.IsSingleAnim)
            {
                var endTrack = SetAnimation(animState, entry.ClickAnim + "_End", loop: false, OverlayTrackIndex);
                if (endTrack != null)
                {
                    ApplyMix(endTrack, entry.MixLoopToEnd);
                    endDuration = endTrack.GetAnimationEnd();
                    DisposeIfNeeded(endTrack);
                }
            }

            animState.BoundObject.Call("add_empty_animation", OverlayTrackIndex, entry.MixOut, 0f);
            spriteState.BlockedUntilMsec =
                Time.GetTicksMsec() + (ulong)((endDuration + entry.MixOut) * 1000f);
        }
    }

    private static void StartOverlayTrack(SpriteTouchState spriteState, MegaAnimationState animState)
    {
        var hasDummy = spriteState.HasDummy ??=
            new MegaSprite(spriteState.Sprite).HasAnimation("Dummy");
        if (hasDummy)
        {
            PlayAnimation(animState, "Dummy", loop: false, OverlayTrackIndex);
        }
        else
        {
            animState.BoundObject.Call("set_empty_animation", OverlayTrackIndex, 0f);
        }
    }

    private static void EnsureSpriteBoneHook(SpriteTouchState spriteState)
    {
        if (spriteState.IsHooked)
        {
            return;
        }

        spriteState.IsHooked = true;
        var megaSprite = new MegaSprite(spriteState.Sprite);
        megaSprite.ConnectBeforeWorldTransformsChange(
            Callable.From<GodotObject>(OnBeforeWorldTransformsChange));
        megaSprite.ConnectWorldTransformsChanged(
            Callable.From<GodotObject>(OnWorldTransformsChanged));
    }

    private static bool HasAnyPendingBoneUpdate(List<TouchBoxEntry> entries)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (NeedsBoneUpdate(entry.HasTargetBone, entry.IsHolding, entry.DragOffset))
            {
                return true;
            }
        }

        return false;
    }

    private static void OnBeforeWorldTransformsChange(GodotObject spriteObj)
    {
        if (!GodotObject.IsInstanceValid(spriteObj) ||
            !SpriteStates.TryGetValue(spriteObj.GetInstanceId(), out var state))
        {
            return;
        }

        state.AppliedBones.Clear();
        if (!HasAnyPendingBoneUpdate(state.Entries))
        {
            return;
        }

        var skeleton = new MegaSprite(spriteObj).GetSkeleton();
        if (skeleton == null)
        {
            return;
        }

        for (var i = 0; i < state.Entries.Count; i++)
        {
            var entry = state.Entries[i];
            if (!NeedsBoneUpdate(entry.HasTargetBone, entry.IsHolding, entry.DragOffset) ||
                !GodotObject.IsInstanceValid(entry.Box) ||
                !entry.Box.IsInsideTree())
            {
                continue;
            }

            var (nextOffset, shouldApply) = ComputeNextDragOffset(
                entry.DragOffset,
                entry.TargetDragOffset,
                entry.DragSpeed,
                entry.IsHolding);
            entry.DragOffset = nextOffset;
            if (!shouldApply)
            {
                continue;
            }

            var bone = skeleton.FindBone(entry.TargetBoneName);
            var boundBone = bone?.BoundObject;
            if (boundBone == null || !GodotObject.IsInstanceValid(boundBone))
            {
                continue;
            }

            var x = boundBone.Call("get_x").AsSingle();
            var y = boundBone.Call("get_y").AsSingle();
            var (offsetX, offsetY) = MapDragOffsetToBoneAxes(
                entry.DragAxisMode,
                entry.DragOffset.X,
                entry.DragOffset.Y);
            boundBone.Call("set_x", x + offsetX);
            boundBone.Call("set_y", y + offsetY);
            state.AppliedBones.Add(new AppliedBoneOffset(boundBone, offsetX, offsetY));
        }
    }

    private static void OnWorldTransformsChanged(GodotObject spriteObj)
    {
        if (!GodotObject.IsInstanceValid(spriteObj) ||
            !SpriteStates.TryGetValue(spriteObj.GetInstanceId(), out var state) ||
            state.AppliedBones.Count == 0)
        {
            return;
        }

        for (var i = state.AppliedBones.Count - 1; i >= 0; i--)
        {
            var applied = state.AppliedBones[i];
            if (!GodotObject.IsInstanceValid(applied.BoundBone))
            {
                continue;
            }

            var x = applied.BoundBone.Call("get_x").AsSingle();
            var y = applied.BoundBone.Call("get_y").AsSingle();
            applied.BoundBone.Call("set_x", x - applied.OffsetX);
            applied.BoundBone.Call("set_y", y - applied.OffsetY);
        }

        state.AppliedBones.Clear();
    }

    private static MegaTrackEntry? SetAnimation(
        MegaAnimationState animState,
        string animName,
        bool loop,
        int trackIndex) =>
        WrapTrackEntry(animState.BoundObject.Call("set_animation", animName, loop, trackIndex));

    private static MegaTrackEntry? AddAnimation(
        MegaAnimationState animState,
        string animName,
        float delay,
        bool loop,
        int trackIndex) =>
        WrapTrackEntry(animState.BoundObject.Call("add_animation", animName, delay, loop, trackIndex));

    private static void PlayAnimation(
        MegaAnimationState animState,
        string animName,
        bool loop,
        int trackIndex) =>
        DisposeIfNeeded(SetAnimation(animState, animName, loop, trackIndex));

    private static MegaTrackEntry? WrapTrackEntry(Variant value) =>
        value.VariantType == Variant.Type.Object && value.AsGodotObject() is { }
            ? new MegaTrackEntry(value)
            : null;

    private static void ApplyMix(MegaTrackEntry track, float? mix)
    {
        if (mix.HasValue)
        {
            track.SetMixDuration(mix.Value);
        }
    }

    private static void DisposeIfNeeded(object? obj)
    {
        if (obj is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static float ReadIntroDurationSeconds(GodotObject obj)
    {
        if (!obj.HasMeta("IntroDuration"))
        {
            return DefaultIntroDurationSeconds;
        }

        var value = obj.GetMeta("IntroDuration");
        return value.VariantType switch
        {
            Variant.Type.Float or Variant.Type.Int => Math.Max(0f, value.AsSingle()),
            _ => ResolveIntroDurationSeconds(value.AsString())
        };
    }

    private static string ReadStringMeta(GodotObject obj, string key, string fallback) =>
        obj.HasMeta(key) ? obj.GetMeta(key).AsString() : fallback;

    private static bool ReadBoolMeta(GodotObject obj, string key, bool fallback) =>
        obj.HasMeta(key) ? obj.GetMeta(key).AsBool() : fallback;

    private static float ReadFloatMeta(GodotObject obj, string key, float fallback)
    {
        if (!obj.HasMeta(key))
        {
            return fallback;
        }

        var value = obj.GetMeta(key);
        return value.VariantType switch
        {
            Variant.Type.Float or Variant.Type.Int => value.AsSingle(),
            _ => float.TryParse(value.AsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : fallback
        };
    }

    private static float? ReadOptionalFloatMeta(GodotObject obj, string key)
    {
        if (!obj.HasMeta(key))
        {
            return null;
        }

        var value = obj.GetMeta(key);
        return value.VariantType switch
        {
            Variant.Type.Float or Variant.Type.Int => value.AsSingle(),
            _ => float.TryParse(value.AsString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : null
        };
    }

    private static int ReadIntMeta(GodotObject obj, string key, int fallback) =>
        obj.HasMeta(key) ? obj.GetMeta(key).AsInt32() : fallback;
}

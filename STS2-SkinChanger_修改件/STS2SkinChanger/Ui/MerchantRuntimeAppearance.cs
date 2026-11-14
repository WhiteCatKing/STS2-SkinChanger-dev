using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Rebuilds the two shop-only visual families after a hot selection: playable-character shop
/// poses and the merchant's body/hand presentation. The inventory model and purchased items stay
/// untouched.
/// </summary>
internal static class MerchantRuntimeAppearance
{
    internal const string GroupId = "merchant";
    internal const string FakeMerchantGroupId = "fake_merchant_monster";
    // v0.113.0 split the shop room into two concrete scenes and dropped the shared
    // scenes/rooms/merchant_room.tscn: loading the old path now yields a non-PackedScene
    // resource ("独立皮肤资源不是场景"), which is why the merchant preview refused to refresh.
    internal const string StandardMerchantRoomScenePath = "res://scenes/rooms/standard_merchant_room.tscn";
    internal const string BlitzMerchantRoomScenePath = "res://scenes/rooms/blitz_merchant_room.tscn";

    /// <summary>Scene used when the live room type is unknown (preview prewarm, base fallback).</summary>
    internal const string MerchantRoomScenePath = StandardMerchantRoomScenePath;

    internal static string ResolveMerchantRoomScenePath(NMerchantRoom? room) =>
        room != null && string.Equals(room.GetType().Name, "NBlitzMerchantRoom", StringComparison.Ordinal)
            ? BlitzMerchantRoomScenePath
            : StandardMerchantRoomScenePath;
    private const string FakeMerchantScenePath = "res://scenes/events/custom/fake_merchant.tscn";
    private const string MerchantInventoryScenePath = "res://scenes/merchant/merchant_inventory.tscn";
    private const string MerchantPreviewRootMeta = "skin_changer_merchant_preview_root";
    private const string MerchantPreviewInputProxyName = "SkinChangerMerchantInputProxy";
    private const string MerchantPreviewFocusedMeta = "skin_changer_merchant_preview_focused";
    private const string PlayerBasePositionMeta = "skin_changer_shop_player_base_position";
    private const string PlayerBaseScaleMeta = "skin_changer_shop_player_base_scale";
    private const string ProviderRootBasePositionMeta = "skin_changer_shop_provider_root_position";
    private const string ProviderRootBaseScaleMeta = "skin_changer_shop_provider_root_scale";

    // v0.113.0 made NMerchantRoom.MerchantButton abstract: the backing field no longer lives on
    // the base type but on the concrete room (NStandardMerchantRoom / NBlitzMerchantRoom), so it
    // has to be resolved from the live instance's own type instead of the abstract base.
    private static readonly Dictionary<Type, FieldInfo?> MerchantButtonFields = [];

    internal static FieldInfo? ResolveMerchantButtonField(object? instance)
    {
        if (instance == null)
        {
            return null;
        }

        var type = instance.GetType();
        lock (MerchantButtonFields)
        {
            if (MerchantButtonFields.TryGetValue(type, out var cached))
            {
                return cached;
            }

            FieldInfo? field = null;
            for (var current = type; current != null && field == null; current = current.BaseType)
            {
                field = AccessTools.Field(current, "<MerchantButton>k__BackingField") ??
                        AccessTools.Field(current, "_merchantButton");
            }

            MerchantButtonFields[type] = field;
            return field;
        }
    }
    private static readonly FieldInfo? FakeMerchantButtonField =
        AccessTools.Field(typeof(NFakeMerchant), "<MerchantButton>k__BackingField");
    private static readonly MethodInfo? FakeMerchantOpenedMethod =
        AccessTools.Method(typeof(NFakeMerchant), "OnMerchantOpened");
    private static readonly FieldInfo? MerchantHandField =
        AccessTools.Field(typeof(NMerchantInventory), "<MerchantHand>k__BackingField");
    private static readonly FieldInfo? MerchantRoomInventoryField =
        AccessTools.Field(typeof(NMerchantRoom), "<Inventory>k__BackingField");
    private static readonly FieldInfo? MerchantRoomProceedButtonField =
        AccessTools.Field(typeof(NMerchantRoom), "_proceedButton");
    private static readonly FieldInfo? FakeMerchantInventoryField =
        AccessTools.Field(typeof(NFakeMerchant), "<Inventory>k__BackingField");
    private static readonly FieldInfo? FakeMerchantProceedButtonField =
        AccessTools.Field(typeof(NFakeMerchant), "_proceedButton");
    private static readonly MethodInfo? FakeMerchantOpenInventoryMethod =
        AccessTools.Method(typeof(NFakeMerchant), "OpenInventory");
    private static readonly FieldInfo? MerchantRoomModelField =
        AccessTools.Field(typeof(NMerchantRoom), "<Room>k__BackingField");
    private static readonly FieldInfo? MerchantRoomPlayersField =
        AccessTools.Field(typeof(NMerchantRoom), "_players");
    private static readonly FieldInfo? MerchantRoomDialogueField =
        AccessTools.Field(typeof(NMerchantRoom), "_dialogue");
    private static readonly FieldInfo? MerchantSkeletonField =
        AccessTools.Field(typeof(NMerchantButton), "_merchantSkeleton");
    private static readonly MethodInfo? MegaSkeletonSetSkinByNameMethod =
        AccessTools.Method(typeof(MegaSkeleton), nameof(MegaSkeleton.SetSkinByName));
    private static readonly List<WeakReference<Node>> ReplayedInventoryAdditions = [];
    private static readonly Dictionary<ulong, List<WeakReference<Node2D>>> ShopProviderRoots = [];

    internal static NMerchantCharacter? GetLocalPlayerVisual()
    {
        var room = NMerchantRoom.Instance;
        return room is { PlayerVisuals.Count: > 0 } ? room.PlayerVisuals[0] : null;
    }

    internal static string? GetSelectedLocalCharacterProvider()
    {
        var player = CharacterAppearanceRuntime.GetLocalPlayer();
        if (player == null)
        {
            return null;
        }

        var group = ContextualSkinControls.FindGroup(
            player.Character.Id.Entry,
            player.Character.GetType().Name);
        return group == null
            ? null
            : SkinService.GetSelectedFullRuntimeProvider(group.Id);
    }

    internal static string? BeginSelectedMerchantNodeReady(Node node)
    {
        var groupId = ResolveMerchantGroupId(node);
        var providerId = groupId == null
            ? null
            : SkinService.GetSelectedFullRuntimeProvider(groupId);
        if (providerId == null || groupId == null)
        {
            return null;
        }

        EnsureSelectedMerchantProviderActive(groupId, providerId);
        TrackInventoryProviderAdditions(
            node,
            ManagedSkinModLoader.ReplaySelectedNodeReadyPrefixes(
                ManagedSkinModLoader.ResolveBehaviorProviderId(providerId),
                node));
        return providerId;
    }

    /// <summary>
    /// A merchant skin positions and scales its figure from the native node's own <c>_Ready</c>
    /// prefix. Skin Changer removes that callback from Harmony and replays it, but the replay only
    /// works while the provider's behaviour is installed: the run narrows behaviour down to the
    /// characters on screen, a shop is not one of them, and a merchant node created later then
    /// replayed nothing at all. The skin's skeleton stayed on the vanilla transform, which is about
    /// two and a half times larger and sits far up and to the left — the reported "merchant grew and
    /// jumped to the top-left corner". Bring the selected provider back before the node binds to it.
    /// </summary>
    private static void EnsureSelectedMerchantProviderActive(string groupId, string providerId)
    {
        try
        {
            if (ManagedSkinModLoader.IsBehaviorActiveFor(providerId))
            {
                return;
            }

            if (NRun.Instance == null)
            {
                // Before a run exists no scope has narrowed provider behaviour yet: every selected
                // provider is active, so a missing one here is a load failure rather than the room
                // problem this method repairs, and re-focusing would only shrink the title screen's
                // scope (character-select and catalogue previews share it).
                return;
            }

            ModLog.Info(
                $"商人外观提供者 {providerId} 不在当前场景范围内，先重新启用以套用其骨骼变换。");
            CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(
                [groupId],
                "商人外观");
        }
        catch (Exception exception)
        {
            ModLog.Warn("重新启用商人外观提供者失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Remembers that this merchant figure's transform belongs to the selected skin, and logs the
    /// values the game actually applied so a future report shows whether the skin's offset ran.
    /// </summary>
    internal static void RememberMerchantFigureProvider(
        NMerchantButton button,
        string? providerId)
    {
        if (providerId == null || !GodotObject.IsInstanceValid(button))
        {
            return;
        }

        try
        {
            var groupId = ResolveMerchantGroupId(button);
            var visual = button.GetNodeOrNull<Node>("%MerchantVisual");
            if (groupId == null || visual == null || !GodotObject.IsInstanceValid(visual))
            {
                return;
            }

            var behaviorProviderId = ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);
            ManagedSkinModLoader.MarkMerchantFigureProvider(visual, behaviorProviderId, groupId);
            if (visual is Node2D node2D)
            {
                // A same-ID variation runs the other copy's code; say so once, otherwise the id in
                // the line does not match the option the player picked and looks like a mystery.
                var behaviorNote = behaviorProviderId.Equals(
                    providerId,
                    StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : $"，行为程序集来自同 ID 主来源 {behaviorProviderId}";
                ModLog.Info(
                    $"商人骨骼变换已应用：{providerId}（{groupId}{behaviorNote}）" +
                    $"位置=({node2D.Position.X:F1}, {node2D.Position.Y:F1})，" +
                    $"缩放=({node2D.Scale.X:F4}, {node2D.Scale.Y:F4})。");
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("记录商人骨骼变换失败：" + exception.GetBaseException().Message);
        }
    }

    internal static void CompleteSelectedMerchantNodeReady(Node node, string? providerId)
    {
        if (providerId == null || !GodotObject.IsInstanceValid(node))
        {
            return;
        }

        var target = node switch
        {
            NMerchantButton => MerchantProviderReadyTarget.Button,
            NMerchantHand => MerchantProviderReadyTarget.Hand,
            _ => MerchantProviderReadyTarget.Inventory
        };
        if (MerchantProviderReadyPolicy.ResolvePostfixTiming(target) ==
            MerchantProviderPostfixTiming.NextFrameThenSpineReady)
        {
            // A provider prefix can replace skeleton_data_res while the outgoing skeleton still
            // reports a ready animation state. Defer one frame before resolving MegaSprite, then
            // use the game's own readiness gate so the provider registers the final skeleton.
            Callable.From(() => CompleteSelectedMerchantSpineNodeReady(node, providerId))
                .CallDeferred();
            return;
        }

        CompleteSelectedMerchantNodeReadyCore(node, providerId);
    }

    private static void CompleteSelectedMerchantSpineNodeReady(Node node, string providerId)
    {
        if (!IsSelectedProviderForNode(node, providerId))
        {
            return;
        }

        try
        {
            var spineNode = node switch
            {
                NMerchantButton button => button.GetNodeOrNull<Node>("%MerchantVisual"),
                NMerchantHand hand => hand.GetParent<Node2D>(),
                _ => null
            };
            if (spineNode == null || !GodotObject.IsInstanceValid(spineNode))
            {
                CompleteSelectedMerchantNodeReadyCore(node, providerId);
                return;
            }

            var sprite = new MegaSprite(spineNode);
            node.RunWhenSpineReady(sprite, _ =>
            {
                if (IsSelectedProviderForNode(node, providerId))
                {
                    CompleteSelectedMerchantNodeReadyCore(node, providerId);
                }
            });
        }
        catch (Exception exception)
        {
            ModLog.Warn(
                $"等待商人外观骨骼就绪失败，改为完成原作者初始化：" +
                exception.GetBaseException().Message);
            if (IsSelectedProviderForNode(node, providerId))
            {
                CompleteSelectedMerchantNodeReadyCore(node, providerId);
            }
        }
    }

    private static bool IsSelectedProviderForNode(Node node, string providerId)
    {
        if (!GodotObject.IsInstanceValid(node))
        {
            return false;
        }

        var groupId = ResolveMerchantGroupId(node);
        return groupId != null &&
               string.Equals(
                   SkinService.GetSelectedFullRuntimeProvider(groupId),
                   providerId,
                   StringComparison.OrdinalIgnoreCase);
    }

    private static void CompleteSelectedMerchantNodeReadyCore(Node node, string providerId)
    {
        var behaviorProviderId = ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);
        TrackInventoryProviderAdditions(
            node,
            ManagedSkinModLoader.ReplaySelectedNodeReadyPostfixes(behaviorProviderId, node));
        var groupId = ResolveMerchantGroupId(node);
        if (groupId != null)
        {
            ManagedMerchantSettingsBridge.Bind(node, groupId, behaviorProviderId);
        }
        if (node is NMerchantInventory)
        {
            MakeProviderInventoryVisualsPassThrough();
        }
    }

    internal static string? ResolveMerchantGroupId(Node node)
    {
        for (Node? current = node; current != null; current = current.GetParent())
        {
            if (current is NFakeMerchant or NFakeMerchantInventory)
            {
                return FakeMerchantGroupId;
            }

            if (current is NMerchantRoom)
            {
                return GroupId;
            }
        }

        return null;
    }

    private static void TrackInventoryProviderAdditions(
        Node owner,
        IEnumerable<Node> addedRoots)
    {
        if (owner is not NMerchantInventory)
        {
            return;
        }

        var existingIds = ReplayedInventoryAdditions
            .Select(reference => reference.TryGetTarget(out var node) &&
                                 GodotObject.IsInstanceValid(node)
                ? node.GetInstanceId()
                : 0UL)
            .ToHashSet();
        foreach (var addedRoot in addedRoots.Where(GodotObject.IsInstanceValid))
        {
            if (existingIds.Add(addedRoot.GetInstanceId()))
            {
                ReplayedInventoryAdditions.Add(new WeakReference<Node>(addedRoot));
            }
        }
    }

    internal static CharacterCombatTransform GetLocalPlayerTransform(string groupId)
    {
        return SkinService.GetCharacterCombatTransform(
            GetLocalPlayerTransformKey(groupId),
            SkinService.Config.GetSelection(groupId));
    }

    internal static CharacterCombatTransform SetLocalPlayerTransform(
        string groupId,
        CharacterCombatTransform value,
        bool save)
    {
        var optionId = SkinService.Config.GetSelection(groupId);
        var normalized = SkinService.SetCharacterCombatTransform(
            GetLocalPlayerTransformKey(groupId),
            optionId,
            value,
            save);
        MultiplayerSkinSync.OnLocalTransformChanged(groupId);
        return normalized;
    }

    internal static void ApplyLocalPlayerTransform(
        NMerchantCharacter visual,
        string groupId,
        CharacterCombatTransform? value = null)
    {
        CaptureLocalPlayerBaseline(visual);
        var basePosition = visual.GetMeta(PlayerBasePositionMeta, visual.Position).AsVector2();
        var baseScale = visual.GetMeta(PlayerBaseScaleMeta, visual.Scale).AsVector2();
        var transform = value ?? GetLocalPlayerTransform(groupId);
        visual.Position = basePosition + new Vector2(transform.OffsetX, transform.OffsetY);
        visual.Scale = baseScale * transform.Scale;
        ApplyProviderRootsTransform(visual, transform);
        ManagedSlotVisibilityBridge.BindPlayerScene(visual, CharacterAppearanceRuntime.GetLocalPlayer());
    }

    internal static void TrackProviderRoots(
        NMerchantCharacter visual,
        IEnumerable<Node> addedRoots)
    {
        if (!GodotObject.IsInstanceValid(visual))
        {
            return;
        }

        var candidates = addedRoots
            .OfType<Node2D>()
            .Where(node => GodotObject.IsInstanceValid(node) && !ReferenceEquals(node, visual))
            .ToArray();
        var sameParentCandidates = candidates
            .Where(node => ReferenceEquals(node.GetParent(), visual.GetParent()))
            .ToArray();
        var candidatePool = sameParentCandidates.Length > 0
            ? sameParentCandidates
            : candidates;
        var nearestDistance = candidatePool.Length == 0
            ? float.PositiveInfinity
            : candidatePool.Min(node => node.GlobalPosition.DistanceSquaredTo(visual.GlobalPosition));
        var selected = candidatePool
            .Where(node => nearestDistance < float.PositiveInfinity &&
                          (candidatePool.Length == 1 ||
                           node.GlobalPosition.DistanceSquaredTo(visual.GlobalPosition) <=
                           nearestDistance + 16f))
            .Select(node => new WeakReference<Node2D>(node))
            .ToList();
        var visualId = visual.GetInstanceId();
        if (selected.Count == 0)
        {
            ShopProviderRoots.Remove(visualId);
        }
        else
        {
            ShopProviderRoots[visualId] = selected;
        }
    }

    /// <summary>
    /// A style/UI package that overrides a part of this group draws the merchant's figure and hand
    /// from its own images. Record them exactly as the game built them — before any package writes —
    /// so the merchant selection can hand the game's own value back and restore the author's art.
    /// </summary>
    internal static void ScopePartTakeover(Node room) =>
        PartTakeoverVisualScope.Capture(room, GroupId);

    internal static void PrepareMerchantSelectionChange()
    {
        var room = NMerchantRoom.Instance;
        var providerId = SkinService.GetSelectedFullRuntimeProvider(GroupId);
        if (room?.Inventory != null && providerId != null)
        {
            // A provider can remain active because its fake-merchant skin is still selected.
            // Restore the real inventory explicitly before changing only the real merchant so
            // a prefix-added leg or other inventory visual cannot survive into the next skin.
            ManagedSkinModLoader.RestoreSelectedNodeReadyBehavior(providerId, room.Inventory);
        }

        foreach (var reference in ReplayedInventoryAdditions.ToArray())
        {
            if (!reference.TryGetTarget(out var node) || !GodotObject.IsInstanceValid(node))
            {
                continue;
            }

            node.GetParent()?.RemoveChild(node);
            node.QueueFree();
        }

        ReplayedInventoryAdditions.Clear();
    }

    internal static bool TryCreateCurrentMerchantRoom(
        NMerchantRoom preloadCreatedRoom,
        out NMerchantRoom? currentRoom,
        out string? error)
    {
        currentRoom = null;
        error = null;
        if (MerchantRoomModelField == null ||
            MerchantRoomPlayersField == null ||
            MerchantRoomDialogueField == null)
        {
            error = "merchant room state fields unavailable";
            return false;
        }

        try
        {
            // NMerchantRoom.Create normally instantiates PreloadManager's startup-cached scene.
            // A merchant selection changed later in the same game process cannot invalidate that
            // cache, so save/reload would recreate the startup skin while replaying the current
            // provider's code. Build from SkinChanger's selection-keyed runtime scene instead.
            // InstantiateRuntimeScene keeps the selected provider pack mounted while Godot resolves
            // lazy external resources (especially ATA/Merchant2Cute Spine data). If a provider
            // does not ship a replacement room scene, retain the base-game fallback.
            NMerchantRoom replacement;
            var replacementTransferred = false;
            var roomScenePath = ResolveMerchantRoomScenePath(preloadCreatedRoom);
            try
            {
                replacement = SkinService.InstantiateRuntimeScene<NMerchantRoom>(
                    GroupId,
                    roomScenePath);
            }
            catch (Exception runtimeException)
            {
                ModLog.Warn("当前商人没有独立房间场景，使用游戏场景兜底：" +
                            runtimeException.GetBaseException().Message);
                replacement = LoadRuntimeOrBaseScene(roomScenePath)
                    .Instantiate<NMerchantRoom>(PackedScene.GenEditState.Disabled);
            }
            try
            {
                MerchantRoomModelField.SetValue(
                    replacement,
                    MerchantRoomModelField.GetValue(preloadCreatedRoom));
                MerchantRoomDialogueField.SetValue(
                    replacement,
                    MerchantRoomDialogueField.GetValue(preloadCreatedRoom));

                var sourcePlayers = MerchantRoomPlayersField.GetValue(preloadCreatedRoom)
                                        as IEnumerable<Player> ??
                                    throw new InvalidOperationException(
                                        "预加载商店房间缺少玩家列表");
                var targetPlayers = MerchantRoomPlayersField.GetValue(replacement)
                                        as List<Player> ??
                                    throw new InvalidOperationException(
                                        "当前商店房间缺少玩家列表");
                targetPlayers.AddRange(sourcePlayers);

                currentRoom = replacement;
                replacementTransferred = true;
                return true;
            }
            finally
            {
                if (!replacementTransferred && GodotObject.IsInstanceValid(replacement))
                {
                    replacement.Free();
                }
            }
        }
        catch (Exception exception)
        {
            error = exception.GetBaseException().Message;
            ModLog.Error("按当前皮肤创建商店房间失败：" + exception);
            return false;
        }
    }

    internal static bool IsMerchantPreviewRoot(Node node) =>
        GodotObject.IsInstanceValid(node) &&
        node.HasMeta(MerchantPreviewRootMeta) &&
        node.GetMeta(MerchantPreviewRootMeta).AsBool();

    private static void InstallMerchantPreviewInputProxy(NMerchantButton button)
    {
        if (button.GetNodeOrNull<Control>(MerchantPreviewInputProxyName) != null)
        {
            return;
        }

        // Do not Harmony-patch NMerchantButton.RefreshFocus for the catalogue. Patching that live
        // method during SkinChanger startup makes the JIT bake the then-unpatched SetSkinByName
        // body into the shop's focus path; providers activated later can no longer remap
        // "outline" there. A transparent child owns catalogue input instead, leaving every live
        // merchant method untouched and letting the active provider receive skin changes through
        // reflection (which cannot inline past Harmony's current entry point).
        button.MouseFilter = Control.MouseFilterEnum.Ignore;
        button.FocusMode = Control.FocusModeEnum.None;
        button.SetMeta(MerchantPreviewFocusedMeta, false);

        var proxy = new Control
        {
            Name = MerchantPreviewInputProxyName,
            MouseFilter = Control.MouseFilterEnum.Stop,
            FocusMode = Control.FocusModeEnum.All,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand,
            ZIndex = 100
        };
        button.AddChild(proxy);
        proxy.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        proxy.MoveToFront();

        var focusState = MerchantPreviewFocusState.None;
        var pressed = false;
        void Transition(MerchantPreviewFocusEvent focusEvent)
        {
            var wasFocused = focusState.IsFocused;
            focusState = MerchantPreviewFocusPolicy.Resolve(focusState, focusEvent);
            if (wasFocused == focusState.IsFocused)
            {
                return;
            }

            button.SetMeta(MerchantPreviewFocusedMeta, focusState.IsFocused);
            ApplyMerchantPreviewFocus(button);
        }

        proxy.MouseEntered += () => Transition(MerchantPreviewFocusEvent.MouseEntered);
        proxy.MouseExited += () =>
        {
            pressed = false;
            Transition(MerchantPreviewFocusEvent.MouseExited);
        };
        proxy.FocusEntered += () => Transition(MerchantPreviewFocusEvent.ControllerFocused);
        proxy.FocusExited += () => Transition(MerchantPreviewFocusEvent.ControllerUnfocused);
        proxy.GuiInput += inputEvent =>
        {
            if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Left } mouseButton)
            {
                if (mouseButton.Pressed)
                {
                    pressed = true;
                }
                else if (pressed)
                {
                    pressed = false;
                    button.ForceClick();
                }

                proxy.AcceptEvent();
                return;
            }

            if (inputEvent.IsActionPressed(MegaInput.select))
            {
                pressed = true;
                proxy.AcceptEvent();
            }
            else if (inputEvent.IsActionReleased(MegaInput.select) && pressed)
            {
                pressed = false;
                button.ForceClick();
                proxy.AcceptEvent();
            }
        };
    }

    private static void ApplyMerchantPreviewFocus(NMerchantButton button)
    {
        if (!GodotObject.IsInstanceValid(button))
        {
            return;
        }

        var skeleton = MerchantSkeletonField?.GetValue(button) as MegaSkeleton;
        if (skeleton == null)
        {
            var visualNode = button.GetNodeOrNull<Node>("%MerchantVisual");
            if (visualNode != null)
            {
                var sprite = new MegaSprite(visualNode);
                button.RunWhenSpineReady(sprite, _ => ApplyMerchantPreviewFocus(button));
            }

            return;
        }

        var isFocused = button.HasMeta(MerchantPreviewFocusedMeta) &&
                        button.GetMeta(MerchantPreviewFocusedMeta).AsBool();
        try
        {
            if (MegaSkeletonSetSkinByNameMethod == null)
            {
                throw new MissingMethodException(
                    typeof(MegaSkeleton).FullName,
                    nameof(MegaSkeleton.SetSkinByName));
            }

            MegaSkeletonSetSkinByNameMethod.Invoke(
                skeleton,
                [isFocused ? "outline" : "default"]);
            skeleton.SetSlotsToSetupPose();
        }
        catch (Exception exception)
        {
            ModLog.Warn("应用图鉴商人悬浮外观失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Initializes the local visual part of the game's native merchant room for a catalogue
    /// preview. The real room's _Ready also touches the map and run singletons, which do not exist
    /// while the compendium is open, so those two global calls are intentionally omitted. The
    /// button, inventory and open/close signal path are otherwise the same native nodes and methods
    /// used by a live shop.
    /// </summary>
    internal static NBackButton? PrepareMerchantPreviewInteraction(
        NMerchantRoom preview,
        Action<bool>? inventoryVisibilityChanged = null)
    {
        try
        {
            var previewButtonField = ResolveMerchantButtonField(preview);
            if (previewButtonField == null ||
                MerchantRoomInventoryField == null ||
                MerchantRoomProceedButtonField == null)
            {
                ModLog.Warn("商人预览缺少原生交互字段，保留按钮视觉但不连接打开逻辑。");
                return null;
            }

            var button = preview.GetNodeOrNull<NMerchantButton>("%MerchantButton") ??
                         preview.FindChild("MerchantButton", recursive: true, owned: false) as NMerchantButton;
            var inventory = preview.GetNodeOrNull<NMerchantInventory>("%Inventory") ??
                            preview.FindChild("Inventory", recursive: true, owned: false) as NMerchantInventory;
            var proceed = preview.GetNodeOrNull<NProceedButton>("%ProceedButton") ??
                          preview.FindChild("ProceedButton", recursive: true, owned: false) as NProceedButton;
            if (button == null || inventory == null || proceed == null)
            {
                ModLog.Warn("商人预览场景缺少 MerchantButton/Inventory/ProceedButton，跳过原生交互连接。");
                return null;
            }

            var previewPlayer = CreatePreviewPlayer("SkinChangerMerchantPreview");
            previewPlayer.Gold = 9999;
            var model = MerchantInventory.CreateForNormalMerchant(previewPlayer);

            MerchantRoomInventoryField.SetValue(preview, inventory);
            MerchantRoomProceedButtonField.SetValue(preview, proceed);
            previewButtonField.SetValue(preview, button);
            button.IsLocalPlayerDead = false;
            button.PlayerDeadLines = MerchantRoom.Dialogue.PlayerDeadLines;
            InstallMerchantPreviewInputProxy(button);
            inventory.MouseFilter = Control.MouseFilterEnum.Ignore;
            inventory.Initialize(model, MerchantRoom.Dialogue);
            button.Connect(
                NMerchantButton.SignalName.MerchantOpened,
                Callable.From<NMerchantButton>(_ =>
                {
                    preview.OpenInventory();
                    inventoryVisibilityChanged?.Invoke(inventory.IsOpen);
                }));
            inventory.Connect(
                NMerchantInventory.SignalName.InventoryClosed,
                Callable.From(() => inventoryVisibilityChanged?.Invoke(false)));
            // A catalogue is not a room and cannot travel to the map. Keep the native node in the
            // hierarchy (so the scene layout remains identical), but hide the native proceed
            // control instead of exposing its uninitialized scene placeholder ("Tally-ho").
            proceed.Visible = false;
            ModLog.Info("商人预览已连接游戏原生 MerchantOpened → OpenInventory 交互路径。");
            return inventory.GetNodeOrNull<NBackButton>("%BackButton");
        }
        catch (Exception exception)
        {
            ModLog.Warn("连接商人预览原生交互失败：" + exception.GetBaseException().Message);
            return null;
        }
    }

    internal static NBackButton? PrepareFakeMerchantPreviewInteraction(
        NFakeMerchant preview,
        Action<bool>? inventoryVisibilityChanged = null)
    {
        try
        {
            if (FakeMerchantInventoryField == null ||
                FakeMerchantProceedButtonField == null ||
                FakeMerchantOpenInventoryMethod == null)
            {
                ModLog.Warn("假商人预览缺少原生交互字段，保留按钮视觉但不连接打开逻辑。");
                return null;
            }

            var button = preview.GetNodeOrNull<NMerchantButton>("%MerchantButton") ??
                         preview.FindChild("MerchantButton", recursive: true, owned: false) as NMerchantButton;
            var inventory = preview.GetNodeOrNull<NMerchantInventory>("%Inventory") ??
                            preview.FindChild("Inventory", recursive: true, owned: false) as NMerchantInventory;
            var proceed = preview.GetNodeOrNull<NProceedButton>("%ProceedButton") ??
                          preview.FindChild("ProceedButton", recursive: true, owned: false) as NProceedButton;
            if (button == null || inventory == null || proceed == null)
            {
                ModLog.Warn("假商人预览场景缺少 MerchantButton/Inventory/ProceedButton，跳过原生交互连接。");
                return null;
            }

            var previewPlayer = CreatePreviewPlayer("SkinChangerFakeMerchantPreview");
            previewPlayer.Gold = 9999;
            // v0.113.0 removed the public per-entry builders (AddRelicEntry and friends); the
            // inventory now has to come from the game's own factory, which fills every column.
            var model = MerchantInventory.CreateForNormalMerchant(previewPlayer);

            FakeMerchantInventoryField.SetValue(preview, inventory);
            FakeMerchantProceedButtonField.SetValue(preview, proceed);
            AccessTools.Field(typeof(NFakeMerchant), "<MerchantButton>k__BackingField")?.SetValue(
                preview,
                button);
            InstallMerchantPreviewInputProxy(button);
            inventory.MouseFilter = Control.MouseFilterEnum.Ignore;
            inventory.Initialize(model, FakeMerchant.Dialogue);
            button.Connect(
                NMerchantButton.SignalName.MerchantOpened,
                Callable.From<NMerchantButton>(_ =>
                {
                    FakeMerchantOpenInventoryMethod.Invoke(preview, null);
                    inventoryVisibilityChanged?.Invoke(inventory.IsOpen);
                }));
            inventory.Connect(
                NMerchantInventory.SignalName.InventoryClosed,
                Callable.From(() => inventoryVisibilityChanged?.Invoke(false)));
            proceed.Visible = false;
            ModLog.Info("假商人预览已连接游戏原生 MerchantOpened → OpenInventory 交互路径。");
            return inventory.GetNodeOrNull<NBackButton>("%BackButton");
        }
        catch (Exception exception)
        {
            ModLog.Warn("连接假商人预览原生交互失败：" + exception.GetBaseException().Message);
            return null;
        }
    }

    private static Player CreatePreviewPlayer(string seed)
    {
        // Use the local net id when one is available so any native ownership checks see the same
        // player as the rest of the UI.  The fallback is only for the title screen before Steam
        // has assigned a local id.
        var netId = LocalContext.NetId ?? 1UL;
        var player = Player.CreateForNewRun(
            ModelDb.Character<Ironclad>(),
            UnlockState.all,
            netId);
        _ = RunState.CreateForTest([player], seed: seed);
        return player;
    }

    // A save reload rebuilds the shop room long after the selection was stored, and the room's
    // own _Ready may run before the pack's resources are mounted. Watch the live room from the
    // always-running tick instead of trusting one event: refresh each new room instance once,
    // retrying for a few seconds because the mount lands a frame or two later.
    private static ulong _watchedRoomId;
    private static int _watchedRoomAttempts;
    private static bool _watchedRoomDone;
    private static string? _watchedRoomSelection;

    /// <summary>
    /// 临时总开关：关闭商人的全部增强刷新（房间就绪后的重试看门狗、热切换重放、位姿记忆）。
    /// 只保留最基础的一条：房间 _Ready 时按当前选择套用资源（MerchantRoomConcreteReadyPatch）。
    /// </summary>
    internal const bool EnhancedMerchantRefreshEnabled = false;

    // a/b 的缩放是乘性叠加（0.18 × 0.18 = 0.0324）：进门那次若包自己的 _Ready 修饰还在位，
    // 我们的重放会把位姿套两遍，表现为模型缩成小点并跑到右下角。检测到这个数量级就还原成单次。
    internal static void CorrectDoubleAppliedPose(Node? room)
    {
        try
        {
            var visual = room?.FindChild("MerchantVisual", recursive: true, owned: false) as Node2D
                         ?? room?.GetNodeOrNull<Node2D>("%MerchantVisual");
            if (visual == null)
            {
                return;
            }

            var scale = visual.Scale.X;
            var providerId = SkinService.GetSelectedRuntimeProvider(GroupId) ?? string.Empty;
            ModLog.Info($"[商人] 本次选择={providerId} 缩放={scale:0.0000} 位置=({visual.Position.X:0.0}, {visual.Position.Y:0.0}) 节点={visual.Name}");
            if (scale <= 0f || scale >= 0.1f)
            {
                return;
            }

            var isAta = providerId.Contains("ATA", StringComparison.OrdinalIgnoreCase);
            visual.Scale = isAta ? new Vector2(0.1500f, 0.1500f) : new Vector2(0.1800f, 0.1800f);
            visual.Position = isAta ? new Vector2(125.0f, 280.0f) : new Vector2(137.3f, 223.3f);
            ModLog.Warn($"[商人] 检测到位姿被重复套用（缩放={scale:0.0000}），已还原为单次：{providerId}");
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[商人] 位姿还原失败：{exception.GetBaseException().Message}");
        }
    }
    internal static void TickRefreshAfterLoad()
    {
        if (!EnhancedMerchantRefreshEnabled)
        {
            return;
        }

        var room = NMerchantRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
        {
            _watchedRoomId = 0;
            _watchedRoomAttempts = 0;
            _watchedRoomDone = false;
            _watchedRoomSelection = null;
            return;
        }

        var roomId = room.GetInstanceId();
        var selection = SkinService.GetSelectedRuntimeProvider("merchant");
        if (roomId != _watchedRoomId || !string.Equals(selection, _watchedRoomSelection, StringComparison.Ordinal))
        {
            _watchedRoomId = roomId;
            _watchedRoomAttempts = 0;
            _watchedRoomDone = false;
            _watchedRoomSelection = selection;
        }

        if (_watchedRoomDone || _watchedRoomAttempts > 240)
        {
            return;
        }

        _watchedRoomAttempts++;
        try
        {
            ScopePartTakeover(room);
            if (TryRefreshMerchant(out var error))
            {
                _watchedRoomDone = true;
                ModLog.Info($"[商人] 房间就绪后已套用当前选择：{selection}。");
            }
            else if (_watchedRoomAttempts == 240)
            {
                _watchedRoomDone = true;
                ModLog.Warn($"[商人] 房间就绪后重试商人外观失败：{error}");
            }
        }
        catch (Exception exception)
        {
            _watchedRoomDone = true;
            ModLog.Warn("[商人] 房间就绪后刷新商人外观异常：" + exception.GetBaseException().Message);
        }
    }
    internal static bool TryRefreshMerchant(out string? error)
    {
        error = null;
        var room = NMerchantRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
        {
            error = "merchant room unavailable";
            return false;
        }

        var merchantButtonField = ResolveMerchantButtonField(room);
        if (merchantButtonField == null || MerchantHandField == null)
        {
            error = "merchant visual fields unavailable";
            return false;
        }

        NMerchantButton? newButton = null;
        NMerchantInventory? inventoryTemplate = null;
        Node2D? newHandContainer = null;
        NMerchantButton? previousButton = null;
        Node2D? previousHandContainer = null;
        NMerchantHand? previousHand = null;
        var buttonSwapped = false;
        var handSwapped = false;
        try
        {
            newButton = InstantiateMerchantButton();
            try
            {
                // Inventory providers can replace the hand scene or its child Spine resources.
                // Instantiate while the selected merchant overlay is mounted so a hot swap never
                // binds the previous merchant's cached hand/skeleton.
                inventoryTemplate = SkinService.InstantiateRuntimeScene<NMerchantInventory>(
                    GroupId,
                    MerchantInventoryScenePath);
            }
            catch (Exception runtimeException)
            {
                ModLog.Warn("当前商人没有独立库存场景，使用游戏场景兜底：" +
                            runtimeException.GetBaseException().Message);
                inventoryTemplate = LoadRuntimeOrBaseScene(MerchantInventoryScenePath)
                    .Instantiate<NMerchantInventory>(PackedScene.GenEditState.Disabled);
            }
            newHandContainer = inventoryTemplate.GetNodeOrNull<Node2D>("MerchantHandContainer") ??
                               FindMerchantHand(inventoryTemplate)?.GetParent() as Node2D ??
                               throw new InvalidOperationException("商店库存场景缺少 MerchantHandContainer 节点");
            inventoryTemplate.RemoveChild(newHandContainer);

            ReplaceMerchantButton(room, newButton, out previousButton);
            buttonSwapped = true;
            ReplaceMerchantHand(room.Inventory, newHandContainer, out previousHandContainer, out previousHand);
            handSwapped = true;

            var providerId = SkinService.GetSelectedFullRuntimeProvider(GroupId);
            if (providerId != null)
            {
                // The replacement button and hand have already passed through the split native
                // _Ready bridge while being attached. The inventory itself is retained so bought
                // items and open state survive; replay only its selected provider callbacks here.
                var behaviorProviderId = ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);
                foreach (var addedNode in ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(
                             behaviorProviderId,
                             room.Inventory,
                             includePrefixes: true))
                {
                    ReplayedInventoryAdditions.Add(new WeakReference<Node>(addedNode));
                }

                ManagedMerchantSettingsBridge.Bind(room.Inventory, GroupId, behaviorProviderId);

                // A merchant skin is visual-only. Some providers add a full-size Control to the
                // inventory root; leaving its default MouseFilter=Stop makes it sit above the
                // game's BackButton and swallow the click after a hot swap. Keep those provider
                // visuals transparent to input while preserving the game's own controls.
                MakeProviderInventoryVisualsPassThrough();
            }

            // Commit only after the complete replacement and provider replay succeeded. Keeping
            // the old nodes alive until this point makes a malformed skin recoverable instead of
            // leaving NMerchantRoom/NMerchantInventory pointing at half-initialized nodes.
            previousButton.GetParent()?.RemoveChild(previousButton);
            previousButton.QueueFree();
            previousHandContainer.GetParent()?.RemoveChild(previousHandContainer);
            previousHandContainer.QueueFree();
            newButton = null;
            newHandContainer = null;
            buttonSwapped = false;
            handSwapped = false;

            // A hot selection change only swaps the part resources above; the merchant's figure and
            // hand are brought in line with the new selection right here. A style/UI package keeps
            // drawing its own shop, so its room and its screens are never touched.
            PartTakeoverVisualScope.Reconcile(room, GroupId);

            return true;
        }
        catch (Exception exception)
        {
            try
            {
                // Remove the failed replacements before restoring the original names. Godot node
                // names must be unique among siblings; restoring while both copies are still
                // parented can silently rename the original and break later % lookups.
                if (handSwapped && newHandContainer != null && GodotObject.IsInstanceValid(newHandContainer))
                {
                    newHandContainer.GetParent()?.RemoveChild(newHandContainer);
                    newHandContainer.Free();
                    newHandContainer = null;
                }

                if (buttonSwapped && newButton != null && GodotObject.IsInstanceValid(newButton))
                {
                    newButton.GetParent()?.RemoveChild(newButton);
                    newButton.Free();
                    newButton = null;
                }

                if (buttonSwapped && previousButton != null && GodotObject.IsInstanceValid(previousButton))
                {
                    merchantButtonField.SetValue(room, previousButton);
                    previousButton.Name = previousButton.Name.ToString().Replace(
                        "__SkinChangerPrevious",
                        string.Empty,
                        StringComparison.Ordinal);
                }

                if (handSwapped && previousHand != null && GodotObject.IsInstanceValid(previousHand))
                {
                    MerchantHandField.SetValue(room.Inventory, previousHand);
                    if (previousHandContainer != null && GodotObject.IsInstanceValid(previousHandContainer))
                    {
                        previousHandContainer.Name = previousHandContainer.Name.ToString().Replace(
                            "__SkinChangerPrevious",
                            string.Empty,
                            StringComparison.Ordinal);
                    }
                }
            }
            catch (Exception rollbackException)
            {
                ModLog.Error("回滚商人外观失败：" + rollbackException);
            }

            error = exception.GetBaseException().Message;
            ModLog.Error("刷新商人外观失败：" + exception);
            return false;
        }
        finally
        {
            if (newButton != null && GodotObject.IsInstanceValid(newButton))
            {
                newButton.Free();
            }

            if (newHandContainer != null && GodotObject.IsInstanceValid(newHandContainer))
            {
                newHandContainer.Free();
            }

            if (inventoryTemplate != null && GodotObject.IsInstanceValid(inventoryTemplate))
            {
                inventoryTemplate.Free();
            }
        }
    }

    internal static bool TryRefreshFakeMerchant(
        NFakeMerchant fakeMerchant,
        out string? error)
    {
        error = null;
        if (!GodotObject.IsInstanceValid(fakeMerchant) ||
            fakeMerchant.MerchantButton == null ||
            FakeMerchantButtonField == null ||
            FakeMerchantOpenedMethod == null)
        {
            error = "fake merchant visual fields unavailable";
            return false;
        }

        NMerchantButton? replacement = null;
        NMerchantButton? previous = null;
        var swapped = false;
        var previousName = string.Empty;
        try
        {
            previous = fakeMerchant.MerchantButton;
            previousName = previous.Name;
            replacement = InstantiateMerchantButton(FakeMerchantGroupId);
            ReplaceMerchantButtonCore(
                previous,
                replacement,
                fakeMerchant,
                FakeMerchantButtonField,
                button => FakeMerchantOpenedMethod.Invoke(fakeMerchant, [button]));
            swapped = true;

            var providerId = SkinService.GetSelectedFullRuntimeProvider(FakeMerchantGroupId);
            ModLog.Info($"[商人] 假商人诊断 选择={providerId ?? "<null>"} 手节点={(FindMerchantHand(fakeMerchant) != null ? "有" : "无")} 视觉节点={fakeMerchant.GetNodeOrNull<Node>("%MerchantVisual")?.Name ?? "<无>"}");
            var behaviorProviderId = providerId == null
                ? null
                : ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);

            // The fake merchant event keeps its inventory and dialogue objects; only the
            // merchant button/visual is replaced. This avoids re-running event initialization
            // and preserves the event's own opened/closed callbacks.
            previous.GetParent()?.RemoveChild(previous);
            previous.QueueFree();

            ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors(
                fakeMerchant,
                behaviorProviderId == null ? [] : [behaviorProviderId]);
            ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors(
                fakeMerchant.Inventory,
                behaviorProviderId == null ? [] : [behaviorProviderId]);
            ManagedSkinModLoader.RestoreUnselectedNodeReadyBehaviors(
                fakeMerchant.Inventory.MerchantHand,
                behaviorProviderId == null ? [] : [behaviorProviderId]);
            if (behaviorProviderId != null)
            {
                ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(fakeMerchant, behaviorProviderId);
                _ = ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(
                    behaviorProviderId,
                    fakeMerchant.Inventory,
                    includePrefixes: true);
                _ = ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(
                    behaviorProviderId,
                    fakeMerchant.Inventory.MerchantHand);
                ManagedMerchantSettingsBridge.Bind(fakeMerchant.Inventory, FakeMerchantGroupId, behaviorProviderId);
                ManagedMerchantSettingsBridge.Bind(fakeMerchant.Inventory.MerchantHand, FakeMerchantGroupId, behaviorProviderId);
            }

            // The hot swap rebuilt the button the author's presentation was attached to. Re-run the
            // same figure/layout hand-back the live event performs on _Ready, otherwise the
            // package's own figure or the layout it moved stayed applied to the new node.
            PartTakeoverVisualScope.Reconcile(fakeMerchant, FakeMerchantGroupId);

            replacement = null;
            return true;
        }
        catch (Exception exception)
        {
            if (replacement != null && GodotObject.IsInstanceValid(replacement))
            {
                replacement.GetParent()?.RemoveChild(replacement);
            }

            if (swapped && previous != null && GodotObject.IsInstanceValid(previous))
            {
                FakeMerchantButtonField.SetValue(fakeMerchant, previous);
                previous.Name = previousName;
            }

            error = exception.GetBaseException().Message;
            ModLog.Error("刷新假商人外观失败：" + exception);
            return false;
        }
        finally
        {
            if (replacement != null && GodotObject.IsInstanceValid(replacement))
            {
                replacement.GetParent()?.RemoveChild(replacement);
                replacement.Free();
            }
        }
    }

    internal static bool TryRefreshLocalPlayer(
        Player player,
        string groupId,
        out NMerchantCharacter? refreshedVisual,
        out string? error)
    {
        refreshedVisual = null;
        error = null;
        var room = NMerchantRoom.Instance;
        if (room == null || !GodotObject.IsInstanceValid(room))
        {
            error = "merchant room unavailable";
            return false;
        }

        if (room.PlayerVisuals is not List<NMerchantCharacter> visuals || visuals.Count == 0)
        {
            error = "merchant player visual unavailable";
            return false;
        }

        NMerchantCharacter? replacement = null;
        NMerchantCharacter? previous = null;
        Node? parent = null;
        var previousName = string.Empty;
        var previousIndex = -1;
        var listSwapped = false;
        var replacementAdded = false;
        var replayedRoots = new List<Node>();
        try
        {
            previous = visuals[0];
            parent = previous.GetParent() ??
                     throw new InvalidOperationException("merchant player visual has no parent");
            previousIndex = previous.GetIndex();
            previousName = previous.Name.ToString();
            replacement = SkinService
                .GetOrLoadRuntimeScene(groupId, player.Character.MerchantAnimPath)
                .Instantiate<NMerchantCharacter>(PackedScene.GenEditState.Disabled);

            // NMerchantRoom.AfterRoomIsLoaded always assigns the local player's slot to (0, 0).
            // Do not inherit a previous skin's already-adjusted root position: that turns a
            // provider-specific offset into the next skin's permanent anchor.
            replacement.Position = Vector2.Zero;
            replacement.Visible = previous.Visible;
            replacement.Modulate = previous.Modulate;
            replacement.SelfModulate = previous.SelfModulate;
            replacement.ZIndex = previous.ZIndex;
            replacement.ZAsRelative = previous.ZAsRelative;

            // Make the replacement visible through NMerchantRoom.PlayerVisuals while AddChild
            // invokes its _Ready callbacks. Character providers commonly resolve their creature
            // from that list during _Ready.
            previous.Name = previousName + "__SkinChangerPrevious";
            replacement.Name = previousName;
            visuals[0] = replacement;
            listSwapped = true;
            parent.AddChild(replacement);
            replacementAdded = true;
            parent.MoveChild(replacement, previousIndex);

            // Complete character skins can attach a separate shop presentation from an
            // NMerchantRoom._Ready postfix. That callback already ran when the room was first
            // created, so replay it after a hot swap as well; the loader tracks its added nodes
            // and visibility changes and restores them when the provider is left.
            var providerId = SkinService.GetSelectedFullRuntimeProvider(groupId);
            if (providerId != null)
            {
                foreach (var replay in ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(room, providerId))
                {
                    replayedRoots.AddRange(replay.AddedRoots);
                    TrackProviderRoots(replacement, replay.AddedRoots);
                }
            }

            // Match initial room creation: first let the scene and provider finish _Ready, then
            // capture their final authored pose as the baseline for the player's custom transform.
            CaptureLocalPlayerBaseline(replacement);
            ApplyLocalPlayerTransform(replacement, groupId);

            previous.GetParent()?.RemoveChild(previous);
            ShopProviderRoots.Remove(previous.GetInstanceId());
            previous.QueueFree();
            refreshedVisual = replacement;
            replacement = null;
            listSwapped = false;
            replacementAdded = false;

            return true;
        }
        catch (Exception exception)
        {
            // A failed character scene or provider callback must leave the live shop exactly as
            // it was. Previously the old node had already been queued for deletion, so the caller
            // received an error while the room kept a half-created replacement.
            foreach (var replayedRoot in replayedRoots.Distinct().ToArray())
            {
                if (!GodotObject.IsInstanceValid(replayedRoot))
                {
                    continue;
                }

                replayedRoot.GetParent()?.RemoveChild(replayedRoot);
                replayedRoot.Free();
            }

            if (replacement != null && GodotObject.IsInstanceValid(replacement))
            {
                ShopProviderRoots.Remove(replacement.GetInstanceId());
                if (replacementAdded)
                {
                    replacement.GetParent()?.RemoveChild(replacement);
                }
                replacement.Free();
                replacement = null;
            }

            if (listSwapped && previous != null && GodotObject.IsInstanceValid(previous))
            {
                visuals[0] = previous;
                previous.Name = previousName;
                if (parent != null && previous.GetParent() == parent && previousIndex >= 0)
                {
                    parent.MoveChild(previous, previousIndex);
                }
            }

            error = exception.GetBaseException().Message;
            ModLog.Error("刷新商店角色外观失败：" + exception);
            return false;
        }
        finally
        {
            if (replacement != null && GodotObject.IsInstanceValid(replacement))
            {
                replacement.Free();
            }
        }
    }

    private static string GetLocalPlayerTransformKey(string groupId) =>
        $"{groupId}::merchant_pose";

    private static void CaptureLocalPlayerBaseline(NMerchantCharacter visual)
    {
        if (!visual.HasMeta(PlayerBasePositionMeta))
        {
            visual.SetMeta(PlayerBasePositionMeta, visual.Position);
        }

        if (!visual.HasMeta(PlayerBaseScaleMeta))
        {
            visual.SetMeta(PlayerBaseScaleMeta, visual.Scale);
        }
    }

    private static void ApplyProviderRootsTransform(
        NMerchantCharacter visual,
        CharacterCombatTransform transform)
    {
        if (!ShopProviderRoots.TryGetValue(visual.GetInstanceId(), out var roots))
        {
            return;
        }

        foreach (var reference in roots.ToArray())
        {
            if (!reference.TryGetTarget(out var root) || !GodotObject.IsInstanceValid(root))
            {
                roots.Remove(reference);
                continue;
            }

            if (!root.HasMeta(ProviderRootBasePositionMeta))
            {
                root.SetMeta(ProviderRootBasePositionMeta, root.Position);
            }

            if (!root.HasMeta(ProviderRootBaseScaleMeta))
            {
                root.SetMeta(ProviderRootBaseScaleMeta, root.Scale);
            }

            var basePosition = root
                .GetMeta(ProviderRootBasePositionMeta, root.Position)
                .AsVector2();
            var baseScale = root
                .GetMeta(ProviderRootBaseScaleMeta, root.Scale)
                .AsVector2();
            root.Position = basePosition + new Vector2(transform.OffsetX, transform.OffsetY);
            root.Scale = baseScale * transform.Scale;
        }

        if (roots.Count == 0)
        {
            ShopProviderRoots.Remove(visual.GetInstanceId());
        }
    }

    private static void ReplaceMerchantButton(
        NMerchantRoom room,
        NMerchantButton replacement,
        out NMerchantButton previous)
    {
        previous = room.MerchantButton ??
                   throw new InvalidOperationException("merchant button is unavailable");
        ReplaceMerchantButtonCore(
            previous,
            replacement,
            room,
            ResolveMerchantButtonField(room)!,
            button => room.OpenInventory());
    }

    private static void ReplaceMerchantButtonCore(
        NMerchantButton previous,
        NMerchantButton replacement,
        object targetOwner,
        FieldInfo targetField,
        Action<NMerchantButton> openedCallback)
    {
        var parent = previous.GetParent() ??
                     throw new InvalidOperationException("merchant button has no parent");
        var index = previous.GetIndex();
        var originalName = previous.Name;
        var wasFocused = previous.HasFocus();
        CopyControlRuntimeState(previous, replacement);
        CopyRequiredUniqueNode(previous, replacement, "MerchantSelectionReticle");
        SetOwnerRecursive(replacement, replacement);
        if (replacement.GetNodeOrNull<Node>("%MerchantVisual") == null ||
            replacement.GetNodeOrNull<Node>("%MerchantSelectionReticle") == null)
        {
            throw new InvalidOperationException("商人场景缺少 MerchantVisual 或 MerchantSelectionReticle");
        }

        previous.Name = originalName + "__SkinChangerPrevious";
        replacement.Name = originalName;
        replacement.IsLocalPlayerDead = previous.IsLocalPlayerDead;
        replacement.PlayerDeadLines = previous.PlayerDeadLines;
        parent.AddChild(replacement);
        parent.MoveChild(replacement, index);
        CopyControlRuntimeState(previous, replacement);
        if (replacement.GetNodeOrNull<Node>("%MerchantVisual") == null ||
            replacement.GetNodeOrNull<Node>("%MerchantSelectionReticle") == null)
        {
            throw new InvalidOperationException("商人场景初始化后丢失必要的视觉节点");
        }

        replacement.SetEnabled(previous.IsEnabled);
        replacement.Connect(
            NMerchantButton.SignalName.MerchantOpened,
            Callable.From<NMerchantButton>(openedCallback));
        targetField.SetValue(targetOwner, replacement);
        if (wasFocused)
        {
            replacement.GrabFocus();
        }
    }

    internal static NMerchantButton InstantiateMerchantButton(
        string groupId = GroupId)
    {
        var scenePath = GetMerchantScenePath(groupId);
        // Build the same complete scene that the game uses for this entity. In particular, the
        // fake merchant has its own MerchantButton offsets and skeleton inside fake_merchant.tscn;
        // loading the normal standalone button made its default preview indistinguishable from
        // the real merchant. Keep load+instantiate inside the selected provider overlay so lazy
        // Spine dependencies also come from the selected skin.
        var sceneTemplate = SkinService.InstantiateRuntimeScene<Node>(groupId, scenePath);

        try
        {
            var button = sceneTemplate.GetNodeOrNull<NMerchantButton>("SceneContainer/MerchantButton") ??
                         sceneTemplate.FindChild(
                             "MerchantButton",
                             recursive: true,
                             owned: false) as NMerchantButton ??
                         throw new InvalidOperationException(
                             $"原生商人场景缺少 SceneContainer/MerchantButton：{scenePath}");
            button.GetParent()?.RemoveChild(button);
            // Give the extracted tree its own scene owner before freeing the temporary full
            // scene; otherwise %MerchantVisual
            // and %MerchantSelectionReticle resolve to the freed template and the button's
            // _Ready path receives Nil.
            SetOwnerRecursive(button, button);
            // SL 能修好，是因为它会在节点进入场景树、原生 _Ready 之前先把该皮肤的外观初始化步骤写上去。
            // 局内切换必须用同样的顺序，否则包的补丁只能写到一个已经被初始化过的节点上，
            // 结果就是 a/b 位姿丢失、c 退回原版商人。
            ReplayNodeReadyBeforeTree(button, groupId);
            return button;
        }

        finally
        {
            if (GodotObject.IsInstanceValid(sceneTemplate))
            {
                sceneTemplate.Free();
            }
        }
    }

    /// <summary>
    /// 在节点加入场景树之前重放选中皮肤的外观初始化步骤（等价于 SL 后的重建顺序）。
    /// </summary>
    private static void ReplayNodeReadyBeforeTree(Node node, string groupId)
    {
        try
        {
            var providerId = SkinService.GetSelectedFullRuntimeProvider(groupId);
            if (string.IsNullOrWhiteSpace(providerId) || !GodotObject.IsInstanceValid(node))
            {
                return;
            }

            EnsureSelectedMerchantProviderActive(groupId, providerId);
            // 第一次切到某个商人皮肤时，它的补丁可能还没装上（激活发生在重放之后），
            // 此时重放拿不到任何步骤 → 位姿退回基础值。先确保补丁在位再重放。
            ManagedSkinModLoader.EnsureProviderPatchesInstalled(providerId);
            var behaviorProviderId = ManagedSkinModLoader.ResolveBehaviorProviderId(providerId);
            // 只按一个键重放一次：ReplaySelectedNodeReadyPrefixes 开头会 RestoreNodeReadyMutation，
            // 同一节点重放两次会把自己刚建出来的节点回收掉，表现为“模型消失”。
            // 必须先全等：Merchant2CuteIIB 也 Contains("Merchant2CuteII")，混用会让 a 命中 b 的运行时，
            // 重放拿到 b 的步骤 → a 每次切换都错。
            var runtimeIds = ManagedSkinModLoader.ActiveProviderRuntimeIds();
            var replayKey = runtimeIds.FirstOrDefault(key => string.Equals(key, providerId, StringComparison.OrdinalIgnoreCase))
                            ?? runtimeIds.FirstOrDefault(key => string.Equals(key, behaviorProviderId, StringComparison.OrdinalIgnoreCase));
            var replayed = replayKey == null
                ? 0
                : ManagedSkinModLoader.ReplaySelectedNodeReadyPrefixes(replayKey, node).Count;

            ModLog.Info($"[商人] 建节点前已重放外观初始化步骤：{providerId}（分组={groupId}，重放={replayed}，运行时={string.Join("|", ManagedSkinModLoader.ActiveProviderRuntimeIds())}）。");
        }
        catch (Exception exception)
        {
            ModLog.Warn($"[商人] 建节点前重放失败（分组={groupId}）：{exception.GetBaseException().Message}");
        }
    }
    internal static (Node Scene, string ScenePath) InstantiateMerchantPreviewScene(
        string groupId = GroupId)
    {
        var scenePath = GetMerchantScenePath(groupId);
        // Keep the native root type while previewing. Provider code uses the ancestor type to
        // distinguish a real merchant from the fake merchant (Merchant2Cute, for example,
        // chooses a different scale/offset for NMerchantRoom versus NFakeMerchant). The root
        // _Ready method is skipped by the preview Harmony prefix below, so no run/map/inventory
        // state is touched; every visual child and authored hierarchy remains intact.
        Node scene = groupId.Equals(FakeMerchantGroupId, StringComparison.OrdinalIgnoreCase)
            ? SkinService.InstantiateRuntimeScene<NFakeMerchant>(groupId, scenePath)
            : SkinService.InstantiateRuntimeScene<NMerchantRoom>(groupId, scenePath);
        scene.SetMeta(MerchantPreviewRootMeta, true);
        PreparePreviewNativeReferences(scene);
        // Record the room's own merchant figure and button layout before the scene enters the tree:
        // a package's room callback runs during _Ready and draws its own merchant over them.
        PartTakeoverVisualScope.Capture(scene, groupId);
        return (scene, scenePath);
    }

    /// <summary>
    /// Fills in the native references a live room sets inside its own gameplay _Ready. The catalogue
    /// keeps the complete native scene and skips only that one method, so a package's own room
    /// callback would otherwise look at an empty merchant button and inventory: CZN Style UI cannot
    /// draw its merchant without the button and throws on the missing inventory. Every reference is
    /// the same node the game itself would cache, so the preview matches the live shop.
    /// </summary>
    private static void PreparePreviewNativeReferences(Node scene)
    {
        try
        {
            switch (scene)
            {
                case NMerchantRoom room:
                    SetPreviewReference(
                        ResolveMerchantButtonField(room),
                        room,
                        ResolveUniqueNode<NMerchantButton>(room, "MerchantButton"));
                    SetPreviewReference(
                        MerchantRoomInventoryField,
                        room,
                        ResolveUniqueNode<NMerchantInventory>(room, "Inventory"));
                    break;
                case NFakeMerchant fakeMerchant:
                    SetPreviewReference(
                        FakeMerchantButtonField,
                        fakeMerchant,
                        ResolveUniqueNode<NMerchantButton>(fakeMerchant, "MerchantButton"));
                    SetPreviewReference(
                        FakeMerchantInventoryField,
                        fakeMerchant,
                        ResolveUniqueNode<NMerchantInventory>(fakeMerchant, "Inventory"));
                    break;
            }
        }
        catch (Exception exception)
        {
            ModLog.Warn("准备商人预览的原生引用失败：" + exception.GetBaseException().Message);
        }
    }

    private static T? ResolveUniqueNode<T>(Node root, string uniqueName)
        where T : Node =>
        root.GetNodeOrNull<T>('%' + uniqueName) ??
        root.FindChild(uniqueName, recursive: true, owned: false) as T;

    private static void SetPreviewReference(FieldInfo? field, object target, Node? node)
    {
        if (field == null || node == null)
        {
            return;
        }

        field.SetValue(target, node);
    }

    internal static string GetMerchantScenePath(string groupId = GroupId) =>
        groupId.Equals(FakeMerchantGroupId, StringComparison.OrdinalIgnoreCase)
            ? FakeMerchantScenePath
            : MerchantRoomScenePath;

    private static PackedScene LoadRuntimeOrBaseScene(
        string scenePath,
        string groupId = GroupId) =>
        TryLoadRuntimeOrBaseScene(scenePath, groupId) ??
        throw new InvalidOperationException($"无法加载商店场景：{scenePath}");

    private static PackedScene? TryLoadRuntimeOrBaseScene(
        string scenePath,
        string groupId = GroupId)
    {
        try
        {
            var runtime = SkinService.GetOrLoadRuntimeScene(groupId, scenePath);
            if (runtime != null)
            {
                return runtime;
            }
        }
        catch (Exception exception)
        {
            // The selected provider may only supply code patches, or this path may not exist in
            // the current game version. Fall through to the base-game scene rather than leaving
            // the live shop half-replaced.
            ModLog.Warn($"加载商人场景 {scenePath} 失败，使用游戏场景兜底：" +
                        exception.GetBaseException().Message);
        }

        return ResourceLoader.Load<PackedScene>(
            scenePath,
            null,
            ResourceLoader.CacheMode.Reuse);
    }

    private static void MakeProviderInventoryVisualsPassThrough()
    {
        foreach (var reference in ReplayedInventoryAdditions.ToArray())
        {
            if (!reference.TryGetTarget(out var node) ||
                !GodotObject.IsInstanceValid(node))
            {
                continue;
            }

            foreach (var descendant in EnumerateNodeTree(node))
            {
                if (descendant is Control control)
                {
                    control.MouseFilter = Control.MouseFilterEnum.Ignore;
                }
            }
        }
    }

    private static void ReplaceMerchantHand(
        NMerchantInventory inventory,
        Node2D replacementContainer,
        out Node2D previousContainer,
        out NMerchantHand previousHand)
    {
        previousHand = inventory.MerchantHand;
        if (!GodotObject.IsInstanceValid(previousHand))
        {
            previousHand = FindMerchantHand(inventory) ??
                           throw new InvalidOperationException("当前商店库存缺少 MerchantHand 节点");
            MerchantHandField!.SetValue(inventory, previousHand);
        }

        previousContainer = previousHand.GetParent() as Node2D ??
                            throw new InvalidOperationException("当前 MerchantHand 没有容器节点");
        var parent = previousContainer.GetParent() ??
                     throw new InvalidOperationException("merchant hand has no parent");
        var index = previousContainer.GetIndex();
        var originalName = previousContainer.Name;
        CopyCanvasItemVisualState(previousContainer, replacementContainer);

        SetOwnerRecursive(replacementContainer, replacementContainer);
        var replacementHand = FindMerchantHand(replacementContainer) ??
                              throw new InvalidOperationException("替换后的商人手部缺少 MerchantHand 节点");
        previousContainer.Name = originalName + "__SkinChangerPrevious";
        replacementContainer.Name = originalName;
        parent.AddChild(replacementContainer);
        parent.MoveChild(replacementContainer, index);
        if (!GodotObject.IsInstanceValid(replacementHand) ||
            !replacementHand.IsInsideTree())
        {
            throw new InvalidOperationException("MerchantHand 在场景初始化后失效");
        }

        MerchantHandField!.SetValue(inventory, replacementHand);
    }

    private static NMerchantHand? FindMerchantHand(Node root) =>
        EnumerateNodeTree(root).OfType<NMerchantHand>().FirstOrDefault();

    private static void CopyCanvasItemVisualState(CanvasItem source, CanvasItem target)
    {
        target.Visible = source.Visible;
        target.Modulate = source.Modulate;
        target.SelfModulate = source.SelfModulate;
        target.ZIndex = source.ZIndex;
        target.ZAsRelative = source.ZAsRelative;
    }

    private static void CopyControlRuntimeState(Control source, Control target)
    {
        // Position, size, scale, rotation and pivot are authored by the newly selected scene.
        // Copying them from the live previous skin leaks provider-specific transforms and makes
        // repeated hot swaps accumulate offsets. Only preserve transient UI state.
        CopyCanvasItemVisualState(source, target);
        target.MouseFilter = source.MouseFilter;
        target.FocusMode = source.FocusMode;
    }

    private static void CopyRequiredUniqueNode(Node baseline, Node replacement, string uniqueName)
    {
        if (replacement.GetNodeOrNull<Node>('%' + uniqueName) != null)
        {
            return;
        }

        var source = baseline.GetNodeOrNull<Node>('%' + uniqueName);
        var clone = source?.Duplicate();
        if (clone == null)
        {
            return;
        }

        clone.Name = uniqueName;
        clone.UniqueNameInOwner = true;
        replacement.AddChild(clone);
    }

    private static void SetOwnerRecursive(Node node, Node owner)
    {
        if (!ReferenceEquals(node, owner))
        {
            node.Owner = owner;
        }

        foreach (var child in node.GetChildren())
        {
            SetOwnerRecursive(child, owner);
        }
    }

    private static IEnumerable<Node> EnumerateNodeTree(Node root)
    {
        yield return root;
        foreach (var child in root.GetChildren())
        {
            foreach (var descendant in EnumerateNodeTree(child))
            {
                yield return descendant;
            }
        }
    }
}

[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.Open))]
internal static class MerchantRoomPrewarmPatch
{
    // F40: the map opens before the player picks the shop node. Prepare the selected merchant
    // room's overlay pack here so shop entry only remounts and instantiates. Cheap no-op after
    // the first attempt for a selection, including base selections.
    [HarmonyPostfix]
    private static void Postfix() =>
        SkinService.PrewarmRuntimeSceneOverlay(
            MerchantRuntimeAppearance.GroupId,
            MerchantRuntimeAppearance.MerchantRoomScenePath);
}

// v0.113.0 removed NMerchantRoom.Create(): the shop room is no longer rebuilt through a static
// factory, so the old "throw away the preloaded room and create a fresh one" path has no entry
// point. Only the behaviour focus and the runtime-scope bookkeeping survive; player visuals are
// applied by MerchantRoomPlayerAppearancePatch below, which already runs on _Ready.
[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._Ready))]
internal static class MerchantRoomCreateAppearancePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix(out long __state) =>
        __state = CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(
            [MerchantRuntimeAppearance.GroupId],
            "商店");

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NMerchantRoom __instance, long __state)
    {
        if (!GodotObject.IsInstanceValid(__instance))
        {
            return;
        }

        RoomRuntimeScopeOwnership.Record(__instance, __state);
        MerchantRuntimeAppearance.ScopePartTakeover(__instance);
    }
}

// v0.113.0 splits the shop into NStandardMerchantRoom / NBlitzMerchantRoom, and both override
// _Ready() and call base._Ready() as their very first statement. A patch on the abstract base
// therefore runs before the concrete room has built its own %MerchantButton / %Inventory, so the
// part takeover is repeated once the concrete room finished its own initialisation.
// v0.113.0 splits the shop into NStandardMerchantRoom / NBlitzMerchantRoom, and both
// override _Ready() calling base first, so the base patch runs before the concrete room
// built its own %MerchantButton / %Inventory. Repeat part takeover once they finish.
//
// 注意：这两个类型只存在于 v0.113.0。正式版 v0.107.x 里没有，写死类型会让我方程序集
// 在旧版上加载失败（玩家表现为「所有皮肤都检测不到」）。所以不挂 [HarmonyPatch] 特性，
// 改为运行时按名字探测：找不到就跳过，旧版仍由基类 NMerchantRoom._Ready 的补丁兜着。
internal static class MerchantRoomConcreteReadyPatch
{
    public const string StandardTypeName = "MegaCrit.Sts2.Core.Nodes.Rooms.NStandardMerchantRoom";
    public const string BlitzTypeName = "MegaCrit.Sts2.Core.Nodes.Rooms.NBlitzMerchantRoom";

    public static void Install(Harmony harmony)
    {
        var patched = 0;
        foreach (var typeName in new[] { StandardTypeName, BlitzTypeName })
        {
            var type = AccessTools.TypeByName(typeName);
            var ready = type == null ? null : AccessTools.Method(type, "_Ready");
            if (ready == null)
            {
                continue;
            }

            harmony.Patch(ready, postfix: new HarmonyMethod(typeof(MerchantRoomConcreteReadyPatch)
                .GetMethod(nameof(Postfix), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)));
            patched++;
        }

        ModLog.Info(patched == 0
            ? "[商人] 当前游戏版本没有拆分后的商店房间类型，沿用基类 _Ready 补丁。"
            : $"[商人] 已为 {patched} 个具体商店房间挂上 _Ready 补丁。");
    }

    private static void Postfix(NMerchantRoom __instance)
    {
        if (!GodotObject.IsInstanceValid(__instance))
        {
            return;
        }

            MerchantRuntimeAppearance.ScopePartTakeover(__instance);
            MerchantRuntimeAppearance.CorrectDoubleAppliedPose(__instance);
    }
}

[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._Ready))]
internal static class MerchantRoomPlayerAppearancePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(NMerchantRoom __instance) =>
        // The compendium keeps the real NMerchantRoom type so provider patches can distinguish
        // it from NFakeMerchant. Its gameplay _Ready would otherwise touch map/run state, so only
        // the preview instance is skipped; live rooms keep the game's original lifecycle.
        !MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NMerchantRoom __instance)
    {
        if (MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance))
        {
            // The catalogue preview runs the same scene with the same packages, so the merchant's
            // figure and the button's layout follow the selection here as well.
            PartTakeoverVisualScope.Reconcile(__instance, MerchantRuntimeAppearance.GroupId);
            return;
        }

        var player = CharacterAppearanceRuntime.GetLocalPlayer();
        if (player == null || __instance.PlayerVisuals.Count == 0)
        {
            return;
        }

        var group = ContextualSkinControls.FindGroup(
            player.Character.Id.Entry,
            player.Character.GetType().Name);
        if (group != null)
        {
            MerchantRuntimeAppearance.ApplyLocalPlayerTransform(
                __instance.PlayerVisuals[0],
                group.Id);

            var providerId = MerchantRuntimeAppearance.GetSelectedLocalCharacterProvider();
            foreach (var replay in ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(__instance, providerId))
            {
                MerchantRuntimeAppearance.TrackProviderRoots(
                    __instance.PlayerVisuals[0],
                    replay.AddedRoots);
            }

            MerchantRuntimeAppearance.ApplyLocalPlayerTransform(
                __instance.PlayerVisuals[0],
                group.Id);
        }

        // Runs after other packages' own room callbacks: a style/UI package that overrides a part
        // of the merchant group draws the merchant from its own images. Only the merchant's figure
        // and hand follow this group's selection; the room and the shop interface stay as the
        // author drew them.
        PartTakeoverVisualScope.Reconcile(__instance, MerchantRuntimeAppearance.GroupId);
    }
}

[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._EnterTree))]
internal static class MerchantRoomPreviewEnterTreePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(NMerchantRoom __instance) =>
        !MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance);
}

[HarmonyPatch(typeof(NMerchantRoom), nameof(NMerchantRoom._ExitTree))]
internal static class MerchantRoomPreviewExitTreePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(NMerchantRoom __instance) =>
        !MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NMerchantRoom __instance)
    {
        if (!MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance))
        {
            RoomRuntimeScopeOwnership.Release(__instance);
        }
    }
}

[HarmonyPatch(typeof(NFakeMerchant), nameof(NFakeMerchant._Ready))]
internal static class FakeMerchantAppearancePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(NFakeMerchant __instance)
    {
        if (MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance))
        {
            return false;
        }

        // The event scene binds its tent's Spine skeleton while it loads; make sure the pack that
        // redrew that tent is on top before that happens.
        SkinService.MountFakeMerchantEventArt();

        var scopeLease = CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(
            [MerchantRuntimeAppearance.FakeMerchantGroupId],
            "假商人场景");
        RoomRuntimeScopeOwnership.Record(__instance, scopeLease);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NFakeMerchant __instance)
    {
        if (MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance))
        {
            PartTakeoverVisualScope.Reconcile(
                __instance,
                MerchantRuntimeAppearance.FakeMerchantGroupId);
            return;
        }

        try
        {
            var providerId = SkinService.GetSelectedFullRuntimeProvider("fake_merchant_monster");
            ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(__instance, providerId);
        }
        catch (Exception exception)
        {
            ModLog.Warn("重放假商人外观初始化失败：" + exception.GetBaseException().Message);
        }
    }
}

[HarmonyPatch(typeof(NFakeMerchant), nameof(NFakeMerchant._EnterTree))]
internal static class FakeMerchantPreviewEnterTreePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(NFakeMerchant __instance) =>
        !MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance);
}

[HarmonyPatch(typeof(NFakeMerchant), nameof(NFakeMerchant._ExitTree))]
internal static class FakeMerchantPreviewExitTreePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(NFakeMerchant __instance) =>
        !MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NFakeMerchant __instance)
    {
        if (!MerchantRuntimeAppearance.IsMerchantPreviewRoot(__instance))
        {
            RoomRuntimeScopeOwnership.Release(__instance);
        }
    }
}

[HarmonyPatch(typeof(NRestSiteRoom), nameof(NRestSiteRoom._Ready))]
internal static class RestSitePlayerAppearancePatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NRestSiteRoom __instance)
    {
        var providerId = MerchantRuntimeAppearance.GetSelectedLocalCharacterProvider();
        ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(__instance, providerId);
    }
}

[HarmonyPatch(typeof(NMerchantButton), nameof(NMerchantButton._Ready))]
internal static class MerchantButtonProviderReadyBridgePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix(NMerchantButton __instance, out string? __state) =>
        __state = MerchantRuntimeAppearance.BeginSelectedMerchantNodeReady(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NMerchantButton __instance, string? __state)
    {
        // The game has just built the merchant's button: record the figure it draws and the layout
        // it gave the button before any package's room callback can replace or move them.
        PartTakeoverVisualScope.RememberMerchantFigure(__instance);
        MerchantRuntimeAppearance.CompleteSelectedMerchantNodeReady(__instance, __state);
        MerchantRuntimeAppearance.RememberMerchantFigureProvider(__instance, __state);
    }
}

[HarmonyPatch(typeof(NMerchantInventory), nameof(NMerchantInventory._Ready))]
internal static class MerchantInventoryProviderReadyBridgePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix(NMerchantInventory __instance, out string? __state) =>
        __state = MerchantRuntimeAppearance.BeginSelectedMerchantNodeReady(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NMerchantInventory __instance, string? __state) =>
        MerchantRuntimeAppearance.CompleteSelectedMerchantNodeReady(__instance, __state);
}

[HarmonyPatch(typeof(NMerchantHand), nameof(NMerchantHand._Ready))]
internal static class MerchantHandProviderReadyBridgePatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static void Prefix(NMerchantHand __instance, out string? __state) =>
        __state = MerchantRuntimeAppearance.BeginSelectedMerchantNodeReady(__instance);

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NMerchantHand __instance, string? __state) =>
        MerchantRuntimeAppearance.CompleteSelectedMerchantNodeReady(__instance, __state);
}

// The catalogue uses the real NMerchantInventory scene so hover tips, hand pointing and the
// open/close animation remain native. It has no Player/RunManager transaction context, however,
// so allowing a slot's purchase task to run would invoke MerchantEntry.OnTryPurchase and produce
// null-reference errors. Return a completed task only for preview slots; live shop purchases are
// untouched.
[HarmonyPatch(typeof(NMerchantSlot), "OnSelected")]
internal static class MerchantPreviewPurchaseBlockPatch
{
    [HarmonyPrefix]
    private static bool Prefix(NMerchantSlot __instance, ref Task __result)
    {
        if (!IsPreviewDescendant(__instance))
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }

    private static bool IsPreviewDescendant(Node node)
    {
        for (Node? current = node; current != null; current = current.GetParent())
        {
            if (MerchantRuntimeAppearance.IsMerchantPreviewRoot(current))
            {
                return true;
            }
        }

        return false;
    }
}

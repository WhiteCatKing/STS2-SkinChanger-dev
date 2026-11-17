using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

// Presentation only: the native power's takeoff + 0.6s delay + fullscreen attack overlay.
// Never call GodByrdPower or NByrdineAttackVfx.Create: those require a real combat/pet owner.
internal static class ByrdineAttackPreview
{
    internal const string ScenePath = "res://scenes/vfx/monsters/byrdine_overlay_vfx.tscn";
    internal const string Animation = "attack_overlay";
    internal const string AttackSfx = "event:/sfx/byrdpip/byrd_2_attack";
    internal const double TakeoffDelay = .6;
    private static readonly ConditionalWeakTable<Node, Playback> Active = new();

    internal static void Start(Node bird, string? groupId, OtherCreatureActionDefinition action)
    {
        Cancel(bird);
        ManagedAncientSceneAnimation.TryPlay(bird, groupId, action.AnimationAliases, action.Loop,
            action.FollowUpAliases, action.FollowUpLoop);
        var stage = new Node2D { Name = "ByrdineAttackOverlayPreview", ZIndex = 100, ProcessMode = Node.ProcessModeEnum.Always };
        bird.GetParent().AddChild(stage);
        var playback = new Playback(bird, stage, groupId);
        Active.Add(bird, playback);
        bird.TreeExiting += playback.Stop;
        playback.Timer = stage.CreateTween();
        playback.Timer.TweenInterval(TakeoffDelay);
        playback.Timer.TweenCallback(Callable.From(playback.Show));
        // Bounded cleanup even if a replacement skeleton never emits completion.
        playback.Timer.TweenInterval(8);
        playback.Timer.TweenCallback(Callable.From(playback.Stop));
    }

    internal static void Cancel(Node? bird)
    {
        if (bird != null && Active.TryGetValue(bird, out var playback)) playback.Stop();
    }

    internal static Node ExtractVisual(Node scene)
    {
        // Keep the authored shader/slot subtree. The native root _Ready dereferences _creature;
        // detach its SpineSprite before entering the tree instead of manufacturing a run.
        var visual = scene.GetNodeOrNull<Node>("SpineSprite")
            ?? throw new InvalidOperationException("异鸟全屏攻击场景缺少 SpineSprite。");
        scene.RemoveChild(visual);
        visual.Owner = null;
        scene.Free();
        return visual;
    }

    private sealed class Playback(Node bird, Node2D stage, string? groupId)
    {
        internal Tween? Timer;
        private bool _stopped;

        internal void Show()
        {
            if (_stopped || !GodotObject.IsInstanceValid(bird) || !bird.IsInsideTree()) { Stop(); return; }
            Node? scene = null;
            try
            {
                using var resources = groupId == null ? null : SkinService.BeginRuntimeResourceScope(groupId, ScenePath);
                scene = groupId == null
                    ? ResourceLoader.Load<PackedScene>(ScenePath, null, ResourceLoader.CacheMode.IgnoreDeep)
                        ?.Instantiate(PackedScene.GenEditState.Disabled)
                    : SkinService.InstantiateRuntimeScene<Node>(groupId, ScenePath);
                if (scene == null) throw new InvalidOperationException("无法加载异鸟全屏攻击场景。");
                var visual = ExtractVisual(scene);
                scene = null;
                stage.Position = stage.GetViewportRect().Size * .5f;
                stage.AddChild(visual);
                var definition = OtherCreatureCatalog.Find("byrdine_monster")!;
                OtherCreaturePreview.Prepare(definition, visual);
                var sprite = new MegaSprite(visual);
                visual.RunWhenSpineReady(sprite, state =>
                {
                    if (_stopped) return;
                    sprite.ConnectAnimationCompleted(Callable.From<GodotObject, GodotObject, GodotObject>((_, _, _) => Stop()));
                    // Bound Spine API stays stable across the supported game branches.
                    using var entry = state.BoundObject.Call("set_animation", Animation, false, 0);
                    SfxCmd.Play(AttackSfx);
                });
            }
            catch (Exception error)
            {
                if (scene != null && GodotObject.IsInstanceValid(scene)) scene.Free();
                ModLog.Warn("异鸟全屏攻击预览失败：" + error.GetBaseException().Message);
                Stop();
            }
        }

        internal void Stop()
        {
            if (_stopped) return;
            _stopped = true;
            Timer?.Kill();
            if (GodotObject.IsInstanceValid(bird)) bird.TreeExiting -= Stop;
            if (Active.TryGetValue(bird, out var current) && ReferenceEquals(current, this)) Active.Remove(bird);
            if (!GodotObject.IsInstanceValid(stage)) return;
            stage.Hide();
            stage.ProcessMode = Node.ProcessModeEnum.Disabled;
            stage.QueueFree();
        }
    }
}

using System.Runtime.CompilerServices;
using Godot;

namespace STS2SkinChanger.Ui;

internal static class OtherCreatureFrameAnimation
{
    private sealed class Playback { internal Action? Finished; }
    private static readonly ConditionalWeakTable<AnimatedSprite2D, Playback> Playbacks = new();

    internal static AnimatedSprite2D? Find(Node root) => root as AnimatedSprite2D ??
        root.FindChildren("*", "AnimatedSprite2D", true, false).OfType<AnimatedSprite2D>().FirstOrDefault();

    internal static string? Resolve(IEnumerable<string> names, IReadOnlyList<string> aliases) =>
        aliases.Select(alias => names.FirstOrDefault(n => n.Equals(alias, StringComparison.OrdinalIgnoreCase)))
            .FirstOrDefault(n => n != null);

    internal static bool Play(Node root, IReadOnlyList<string> aliases, bool loop,
        IReadOnlyList<string>? followUp, bool followUpLoop)
    {
        var sprite = Find(root);
        if (sprite?.SpriteFrames is not { } source) return false;
        var name = Resolve(source.GetAnimationNames(), aliases);
        if (name == null) return false;
        var playback = Playbacks.GetOrCreateValue(sprite);
        if (playback.Finished != null) sprite.AnimationFinished -= playback.Finished;
        playback.Finished = null;
        // Loop settings are a preview choice. Never modify a shared provider SpriteFrames.
        var frames = (SpriteFrames)source.Duplicate();
        frames.SetAnimationLoop(name, loop);
        sprite.SpriteFrames = frames;
        var next = followUp == null ? null : Resolve(frames.GetAnimationNames(), followUp);
        if (!loop && next != null)
        {
            frames.SetAnimationLoop(next, followUpLoop);
            Action? finished = null;
            finished = () =>
            {
                sprite.AnimationFinished -= finished;
                playback.Finished = null;
                if (GodotObject.IsInstanceValid(sprite) && sprite.IsInsideTree()) sprite.Play(next);
            };
            playback.Finished = finished;
            sprite.AnimationFinished += finished;
        }
        sprite.Play(name);
        return true;
    }
}

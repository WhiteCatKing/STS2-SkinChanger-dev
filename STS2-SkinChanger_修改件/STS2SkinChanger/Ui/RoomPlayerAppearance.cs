using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class RoomPlayerAppearance
{
    internal static Node2D? GetVisual(Player player) =>
        NRestSiteRoom.Instance?.GetCharacterForPlayer(player) is { } restCharacter
            ? restCharacter
            : GetFakeMerchantVisual(NEventRoom.Instance?.CustomEventNode as NFakeMerchant);

    // The native loop inserts the local player first, then moves each visual to index zero.
    internal static NCreatureVisuals? GetFakeMerchantVisual(NFakeMerchant? room) =>
        room?.GetNodeOrNull<Control>("%CharacterContainer")?.GetChildren()
            .OfType<NCreatureVisuals>().LastOrDefault();

    internal static Control GetBounds(Node2D visual) => visual is NRestSiteCharacter rest
        ? rest.Hitbox
        : ((NCreatureVisuals)visual).Bounds;

    internal static Node2D Refresh(Player player, Node2D previous) =>
        previous is NRestSiteCharacter rest
            ? RefreshRestSite(player, rest)
            : RefreshFakeMerchant(player, (NCreatureVisuals)previous);

    internal static void DecorateFakeMerchantPlayer(Player player, NCreatureVisuals visual)
    {
        var group = ContextualSkinControls.FindGroup(player.Character.Id.Entry, player.Character.GetType().Name);
        if (group == null) return;
        var provider = SkinService.GetSelectedRuntimeProvider(group.Id);
        if (provider is not ("RedHoodIronclad" or "RedHoodNecrobinder")) return;
        var assembly = ManagedSkinModLoader.GetActiveProviderAssembly(provider);
        // Red Hood supplies a merchant pose from its scene decorator. The fake event uses
        // the battle presentation type, so hand that same decorator the authored shop path.
        var mod = assembly?.GetType("RedHoodNecrobinder.Mod");
        if (mod != null)
            AccessTools.DeclaredMethod(mod, "Decorate", [typeof(Node), typeof(string)])
                ?.Invoke(null, [visual, player.Character.MerchantAnimPath]);
    }

    internal static NCreatureVisuals RefreshFakeMerchant(Player player, NCreatureVisuals previous)
    {
        var replacement = player.Character.CreateVisuals() ??
            throw new InvalidOperationException("The current character did not create room visuals.");
        Replace(previous, replacement);
        if (replacement.SpineBody is { } spine)
        {
            var animation = spine.HasAnimation("relaxed_loop") ? "relaxed_loop" : "idle_loop";
            if (spine.HasAnimation(animation))
            {
                replacement.SpineAnimation.SetAnimation(animation);
                using var track = replacement.SpineAnimation.GetCurrentTrack();
                track?.SetLoop(true);
            }
        }
        DecorateFakeMerchantPlayer(player, replacement);
        return replacement;
    }

    private static NRestSiteCharacter RefreshRestSite(Player player, NRestSiteCharacter previous)
    {
        var room = NRestSiteRoom.Instance!;
        var index = room.Characters.IndexOf(previous);
        var animationIndex = room.characterAnims.IndexOf(previous);
        var replacement = NRestSiteCharacter.Create(player, index);
        room.Characters[index] = replacement;
        room.characterAnims[animationIndex] = replacement;
        Replace(previous, replacement);
        if (index % 2 == 1) replacement.FlipX();
        ManagedSkinModLoader.ReplaySelectedRoomReadyBehaviors(
            room, MerchantRuntimeAppearance.GetSelectedLocalCharacterProvider());
        return replacement;
    }

    private static void Replace(Node2D previous, Node2D replacement)
    {
        var parent = previous.GetParent();
        var index = previous.GetIndex();
        replacement.Position = previous.Position;
        replacement.Visible = previous.Visible;
        replacement.Modulate = previous.Modulate;
        parent.AddChild(replacement);
        parent.MoveChild(replacement, index);
        parent.RemoveChild(previous);
        previous.QueueFree();
    }
}

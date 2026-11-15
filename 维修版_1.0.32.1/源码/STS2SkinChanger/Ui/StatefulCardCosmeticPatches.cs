using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2SkinChanger.Core;
using STS2SkinChanger.Core.Safety;

namespace STS2SkinChanger.Ui;

[HarmonyPatch]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulCardTooltipPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.PropertyGetter(typeof(CardModel), "HoverTips");
        yield return AccessTools.PropertyGetter(typeof(CardModel), "ExtraHoverTips");
    }
    private static void Postfix(CardModel __instance, MethodBase __originalMethod, ref IEnumerable<IHoverTip> __result)
    {
        var runtime = StatefulCardArtRuntime.For(__instance);
        if (runtime is not { Enabled: true }) return;
        try
        {
            var tips = runtime.Setting("HideTooltips", true) ? [] : __result;
            if (__originalMethod.Name == "get_HoverTips" && runtime.Setting("ShowCreditsTooltip", true))
                tips = runtime.Credits(__instance).Concat(tips);
            __result = tips.ToArray();
        }
        catch (Exception exception) { StatefulCardArtRuntime.Warn("tooltips", exception); }
    }
}

[HarmonyPatch(typeof(NCard), "ActivateRewardScreenGlow")]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulRewardGlowPatch
{
    private static bool Prefix(NCard __instance)
    {
        return Safe.Get("patch:stateful-card", () =>
        {
            var runtime = StatefulCardArtRuntime.For(__instance.Model);
            return runtime is not { Enabled: true } || !runtime.Setting("HideCardRewardRarityGlow", true);
        }, true);
    }
}

[HarmonyPatch(typeof(CardModel), nameof(CardModel.CreateOverlay))]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulInfectionOverlayPatch
{
    private static void Postfix(CardModel __instance, ref Control? __result)
    {
        if (__instance.GetType().Name != "Infection" || StatefulCardArtRuntime.For(__instance) is not { Enabled: true } runtime) return;
        try
        {
            var scene = SkinService.LoadCardPresentationResource<PackedScene>(__instance,
                runtime.Contract.ResourceRoot + "/scenes/cards/overlays/red_infection.tscn");
            if (scene == null) return;
            var replacement = scene.Instantiate<Control>();
            __result?.QueueFree();
            __result = replacement;
        }
        catch (Exception exception) { StatefulCardArtRuntime.Warn("infection overlay", exception); }
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), "BeginRun")]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulCharacterSceneExitPatch
{
    private static void Prefix(NCharacterSelectScreen __instance)
    {
        void Visit(Node root)
        {
            if (StatefulCardArtRuntime.Owns(root.GetType().Assembly))
                AccessTools.Method(root.GetType(), "ResetFingersPosition")?.Invoke(root, null);
            foreach (var child in root.GetChildren()) Visit(child);
        }
        try { Visit(__instance); } catch (Exception exception) { StatefulCardArtRuntime.Warn("character scene exit", exception); }
    }
}

[HarmonyPatch]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulCardPlayCosmeticPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(Neurosurge), "OnPlay");
        yield return AccessTools.Method(typeof(Clash), "OnPlay");
    }
    // Void observer: cannot suppress/replace the game's play Task or execute DamageCmd again.
    private static void Prefix(CardModel __instance, CardPlay cardPlay)
    {
        Safe.Run("patch:stateful-card", () =>
        {
            var runtime = StatefulCardArtRuntime.For(__instance);
            if (runtime is not { Enabled: true, Simple: false }) return;
            try
            {
                if (__instance is Neurosurge && runtime.Setting("EnableNeurosurgeYippee", true))
                {
                    var type = runtime.Assembly.GetType(runtime.Contract.SettingsType[..runtime.Contract.SettingsType.LastIndexOf('.')] + ".Patches.NeurosurgeYippe");
                    AccessTools.Method(type, "Prefix")?.Invoke(null, [cardPlay]);
                }
                else if (__instance is Clash && runtime.Setting("EnableClashAsGrandFinale", true) && NCombatRoom.Instance is {} room)
                {
                    var vfx = NGrandFinaleVfx.Create(__instance.Owner.Creature);
                    if (vfx != null) room.CombatVfxContainer.AddChild(vfx);
                }
            }
            catch (Exception exception) { StatefulCardArtRuntime.Warn("play cosmetic", exception); }
        });
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterDamageGiven))]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulDamageCosmeticPatch
{
    private static void Postfix(AbstractModel __instance, Creature target, CardModel? cardSource)
    {
        Safe.Run("patch:stateful-card", () =>
        {
            if (__instance.GetType().Name != "MultiplayerScalingModel" || cardSource == null) return;
            var runtime = StatefulCardArtRuntime.For(cardSource);
            if (runtime is not { Enabled: true, Simple: false }) return;
            try
            {
                if (cardSource is Clash && runtime.Setting("EnableClashAsGrandFinale", true))
                {
                    var vfx = NGrandFinaleImpactVfx.Create(target);
                    if (vfx != null) NCombatRoom.Instance?.CombatVfxContainer.AddChild(vfx);
                }
                else if (cardSource.GetType().Name is "Squeeze" or "Flatten" or "Rattle")
                    StatefulCombatEffects.Play(cardSource, target, runtime);
            }
            catch (Exception exception) { StatefulCardArtRuntime.Warn("damage cosmetic", exception); }
        });
    }
}

[HarmonyPatch(typeof(AbstractModel), nameof(AbstractModel.AfterSideTurnStart))]
[STS2SkinChanger.Core.Features.FeaturePatch("stateful-card")]
internal static class StatefulSideTurnCosmeticPatch
{
    private static void Postfix(AbstractModel __instance)
    {
        if (__instance.GetType().Name == "MultiplayerScalingModel") StatefulCombatEffects.ClearCurrent();
    }
}

using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using STS2SkinChanger.Core.Features;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Warms card-skin portraits before a transform animation starts.
///
/// The deck-transform flow creates its cards on the spot: the select screen builds a preview card
/// per possible result, and <see cref="NCardTransformVfx"/> / <see cref="NCardTransformShineVfx"/>
/// swap the card model mid-animation. Card-skin portraits are read through <c>CardModel.Portrait</c>,
/// which loads its texture synchronously the first time a card is drawn, so every first-time load
/// lands in the middle of that animation: the card snaps to the new art without the shine/reveal
/// easing ("特效没了，很生硬"), which is exactly what a player reports.
///
/// Preloading the same requests through the existing cache removes the load from the animation
/// path; the animation then only reads already-loaded textures. Everything is best-effort: a
/// failure here never blocks the game's transform flow.
/// </summary>
[HarmonyPatch(typeof(NTransformPreview), nameof(NTransformPreview.Initialize))]
[FeaturePatch("card")]
internal static class TransformPreviewPortraitPreloadPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(IEnumerable<CardTransformation> cardTransformations)
    {
        try
        {
            var cards = new List<CardModel>();
            foreach (var transformation in cardTransformations)
            {
                if (transformation.Original != null)
                {
                    cards.Add(transformation.Original);
                }

                if (transformation.Replacement != null)
                {
                    cards.Add(transformation.Replacement);
                }

                if (transformation.ReplacementOptions != null)
                {
                    cards.AddRange(transformation.ReplacementOptions);
                }
            }

            if (cards.Count > 0)
            {
                Core.SkinService.PreloadCardPortraits(cards);
            }
        }
        catch (Exception exception)
        {
            Core.ModLog.Warn("预先加载变化预览卡图失败：" + exception.GetBaseException().Message);
        }
    }
}

[HarmonyPatch(typeof(NCardTransformVfx), nameof(NCardTransformVfx.Create))]
[FeaturePatch("card")]
internal static class CardTransformVfxPortraitPreloadPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(CardModel startCard, CardModel endCard)
    {
        try
        {
            // Diagnoses the "transform effect is gone" report from either side: if this line is
            // missing the game never created the effect (preview style/container), and if it is
            // present while nothing is drawn the effect node is being hidden or stalling.
            Core.ModLog.Info(
                $"变化特效已创建：{startCard?.Id.ToString() ?? "<null>"} → {endCard?.Id.ToString() ?? "<null>"}");
            Core.SkinService.PreloadCardPortraits([startCard, endCard]);
        }
        catch (Exception exception)
        {
            Core.ModLog.Warn("预先加载变化特效卡图失败：" + exception.GetBaseException().Message);
        }
    }
}

[HarmonyPatch(typeof(NCardTransformShineVfx), nameof(NCardTransformShineVfx.Create))]
[FeaturePatch("card")]
internal static class CardTransformShineVfxPortraitPreloadPatch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(NCard cardNode, CardModel endCard)
    {
        try
        {
            var cards = new List<CardModel>();
            if (cardNode?.Model != null)
            {
                cards.Add(cardNode.Model);
            }

            if (endCard != null)
            {
                cards.Add(endCard);
            }

            if (cards.Count > 0)
            {
                Core.SkinService.PreloadCardPortraits(cards);
            }
        }
        catch (Exception exception)
        {
            Core.ModLog.Warn("预先加载变化光效卡图失败：" + exception.GetBaseException().Message);
        }
    }

    /// <summary>
    /// Belt and braces with <c>CardSkinControls.SendPresentationBehindCardVfx</c>: a skin card face
    /// used to be added above <c>%CardVfxContainer</c>, which hid the shine entirely. Raising the
    /// effect above the card's own layers makes it visible even when a provider adds nodes at a
    /// positive z-index (2000 is reserved for our full-screen editors).
    /// </summary>
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(NCardTransformShineVfx? __result)
    {
        try
        {
            if (__result != null && GodotObject.IsInstanceValid(__result))
            {
                __result.ZIndex = 10;
            }
        }
        catch
        {
            // Cosmetic only; never break the game's transform animation over a z-index.
        }
    }
}

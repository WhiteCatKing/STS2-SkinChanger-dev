using System;
using System.Collections.Generic;
using CznCore.Presentation;
using Godot;

namespace MeirinWatcherSkin.Characters.Meirin;

internal static class MeirinDefeatPresentationProfile
{
	internal const string OverlayScenePath = "res://MeirinWatcherSkin/scenes/defeat_vacant_overlay.tscn";

	internal const string CollapseType1ImagePath = "res://MeirinWatcherSkin/images/collapse_1027_02.png";

	internal const string CollapseType2ImagePath = "res://MeirinWatcherSkin/images/collapse_1027_01.png";

	internal const string CollapseSfxPath = "";

	internal const float CollapseType1OuterScale = 1.1f;

	internal const float CollapseType2OuterScale = 1.5f;

	internal static readonly Vector2 CollapseType1Offset = new Vector2(-70f, 0f);

	internal static readonly Vector2 CollapseType2Offset = Vector2.Zero;

	public static readonly DefeatPresentationProfile Profile = new DefeatPresentationProfile("res://MeirinWatcherSkin/images/collapse_1027_02.png", "", 0f, 0f, 0f, 0f, 0f, 0f, 1.9667f, 1f, 1f, Vector2.Zero, Vector2.Zero, 0f, 0f, 2000uL, (IReadOnlyList<DefeatEffectLayerProfile>)Array.Empty<DefeatEffectLayerProfile>(), (IReadOnlyList<DefeatSfxCue>)Array.Empty<DefeatSfxCue>(), (IReadOnlyList<DefeatCgMotionLayerProfile>)null, 1f, "res://MeirinWatcherSkin/images/collapse_1027_01.png", 1.1f, 1.5f, (IReadOnlyList<DefeatFrameSequenceLayerProfile>)null, 1080f, (IReadOnlyList<DefeatCgMotionLayerProfile>)null);
}

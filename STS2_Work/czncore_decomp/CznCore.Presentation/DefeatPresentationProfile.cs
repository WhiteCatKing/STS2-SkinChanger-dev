using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace CznCore.Presentation;

internal sealed record DefeatPresentationProfile(string CgTexturePath, string CollapseSfxPath, float CgStartOverlapSeconds, float DimAlpha, float DimFadeSeconds, float EffectIntroDelaySeconds, float CgIntroDelaySeconds, float CgFadeInSeconds, float CgMotionSeconds, float CgStartScaleMultiplier, float CgEndScaleMultiplier, Vector2 CgStartOffset, Vector2 CgEndOffset, float CollapseSfxDelaySeconds, float CollapseSfxVolumeDb, ulong DuplicateSuppressMs, IReadOnlyList<DefeatEffectLayerProfile> EffectLayers, IReadOnlyList<DefeatSfxCue>? SfxCues = null, IReadOnlyList<DefeatCgMotionLayerProfile>? CgMotionLayers = null, float RuntimeCgScaleAdapter = 1f, string? CgType2TexturePath = null, float CgType1ScaleAdapter = 1f, float CgType2ScaleAdapter = 1f, IReadOnlyList<DefeatFrameSequenceLayerProfile>? FrameSequenceLayers = null, float EffectReferenceHeight = 0f, IReadOnlyList<DefeatCgMotionLayerProfile>? CgType2MotionLayers = null)
{
	public string? OfficialScenePath { get; init; }

	public IReadOnlyList<DefeatSfxCue> EffectiveSfxCues => SfxCues ?? new _003C_003Ez__ReadOnlySingleElementList<DefeatSfxCue>(new DefeatSfxCue("collapse", CollapseSfxPath, CollapseSfxDelaySeconds, CollapseSfxVolumeDb));

	public IReadOnlyList<DefeatCgMotionLayerProfile> EffectiveCgMotionLayers => CgMotionLayers ?? Array.Empty<DefeatCgMotionLayerProfile>();

	public IReadOnlyList<DefeatCgMotionLayerProfile> EffectiveCgType2MotionLayers => CgType2MotionLayers ?? EffectiveCgMotionLayers;

	public IReadOnlyList<DefeatFrameSequenceLayerProfile> EffectiveFrameSequenceLayers => FrameSequenceLayers ?? Array.Empty<DefeatFrameSequenceLayerProfile>();

	public float OverlayLifetimeSeconds
	{
		get
		{
			float num = 0f;
			foreach (DefeatEffectLayerProfile effectLayer in EffectLayers)
			{
				num = MathF.Max(num, effectLayer.VisibleSeconds);
			}
			float x = num + EffectIntroDelaySeconds;
			float x2 = 0f;
			foreach (DefeatFrameSequenceLayerProfile effectiveFrameSequenceLayer in EffectiveFrameSequenceLayers)
			{
				x2 = MathF.Max(x2, effectiveFrameSequenceLayer.StartDelaySeconds + effectiveFrameSequenceLayer.VisibleSeconds);
			}
			float x3 = CgIntroDelaySeconds + CgMotionSeconds;
			float dimFadeSeconds = DimFadeSeconds;
			return MathF.Max(0.25f, MathF.Max(x2, MathF.Max(x, MathF.Max(x3, dimFadeSeconds))));
		}
	}

	public static DefeatPresentationProfile ForOfficialScene(string scenePath, string type1Image, string type2Image, float startOverlapSeconds, ulong duplicateSuppressMs = 2000uL)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		return new DefeatPresentationProfile(type1Image, string.Empty, startOverlapSeconds, 0f, 0f, 0f, 0f, 0f, 0f, 1f, 1f, Vector2.Zero, Vector2.Zero, 0f, 0f, duplicateSuppressMs, Array.Empty<DefeatEffectLayerProfile>(), Array.Empty<DefeatSfxCue>(), null, 1f, type2Image)
		{
			OfficialScenePath = scenePath
		};
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("CgTexturePath = ");
		builder.Append((object?)CgTexturePath);
		builder.Append(", CollapseSfxPath = ");
		builder.Append((object?)CollapseSfxPath);
		builder.Append(", CgStartOverlapSeconds = ");
		builder.Append(CgStartOverlapSeconds.ToString());
		builder.Append(", DimAlpha = ");
		builder.Append(DimAlpha.ToString());
		builder.Append(", DimFadeSeconds = ");
		builder.Append(DimFadeSeconds.ToString());
		builder.Append(", EffectIntroDelaySeconds = ");
		builder.Append(EffectIntroDelaySeconds.ToString());
		builder.Append(", CgIntroDelaySeconds = ");
		builder.Append(CgIntroDelaySeconds.ToString());
		builder.Append(", CgFadeInSeconds = ");
		builder.Append(CgFadeInSeconds.ToString());
		builder.Append(", CgMotionSeconds = ");
		builder.Append(CgMotionSeconds.ToString());
		builder.Append(", CgStartScaleMultiplier = ");
		builder.Append(CgStartScaleMultiplier.ToString());
		builder.Append(", CgEndScaleMultiplier = ");
		builder.Append(CgEndScaleMultiplier.ToString());
		builder.Append(", CgStartOffset = ");
		Vector2 val = CgStartOffset;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", CgEndOffset = ");
		val = CgEndOffset;
		builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", CollapseSfxDelaySeconds = ");
		builder.Append(CollapseSfxDelaySeconds.ToString());
		builder.Append(", CollapseSfxVolumeDb = ");
		builder.Append(CollapseSfxVolumeDb.ToString());
		builder.Append(", DuplicateSuppressMs = ");
		builder.Append(DuplicateSuppressMs.ToString());
		builder.Append(", EffectLayers = ");
		builder.Append(EffectLayers);
		builder.Append(", SfxCues = ");
		builder.Append(SfxCues);
		builder.Append(", CgMotionLayers = ");
		builder.Append(CgMotionLayers);
		builder.Append(", RuntimeCgScaleAdapter = ");
		builder.Append(RuntimeCgScaleAdapter.ToString());
		builder.Append(", CgType2TexturePath = ");
		builder.Append((object?)CgType2TexturePath);
		builder.Append(", CgType1ScaleAdapter = ");
		builder.Append(CgType1ScaleAdapter.ToString());
		builder.Append(", CgType2ScaleAdapter = ");
		builder.Append(CgType2ScaleAdapter.ToString());
		builder.Append(", FrameSequenceLayers = ");
		builder.Append(FrameSequenceLayers);
		builder.Append(", EffectReferenceHeight = ");
		builder.Append(EffectReferenceHeight.ToString());
		builder.Append(", CgType2MotionLayers = ");
		builder.Append(CgType2MotionLayers);
		builder.Append(", OfficialScenePath = ");
		builder.Append((object?)OfficialScenePath);
		builder.Append(", EffectiveSfxCues = ");
		builder.Append(EffectiveSfxCues);
		builder.Append(", EffectiveCgMotionLayers = ");
		builder.Append(EffectiveCgMotionLayers);
		builder.Append(", EffectiveCgType2MotionLayers = ");
		builder.Append(EffectiveCgType2MotionLayers);
		builder.Append(", EffectiveFrameSequenceLayers = ");
		builder.Append(EffectiveFrameSequenceLayers);
		builder.Append(", OverlayLifetimeSeconds = ");
		builder.Append(OverlayLifetimeSeconds.ToString());
		return true;
	}
}

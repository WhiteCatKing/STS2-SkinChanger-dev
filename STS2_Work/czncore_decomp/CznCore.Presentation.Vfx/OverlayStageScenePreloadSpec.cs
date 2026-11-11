using System;
using System.Collections.Generic;
using Godot;

namespace CznCore.Presentation.Vfx;

internal sealed class OverlayStageScenePreloadSpec
{
	public IReadOnlyList<string> ScenePaths { get; }

	public float StaggerSeconds { get; init; } = 0.15f;

	public string TypeHint { get; init; } = string.Empty;

	public bool UseSubThreads { get; init; } = CznRuntimeCapabilities.UseSubThreadsForWarmupResourceLoads;

	public CacheMode CacheMode { get; init; } = (CacheMode)1;

	public OverlayStageScenePreloadSpec(IReadOnlyList<string> scenePaths)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ScenePaths = scenePaths ?? Array.Empty<string>();
	}
}

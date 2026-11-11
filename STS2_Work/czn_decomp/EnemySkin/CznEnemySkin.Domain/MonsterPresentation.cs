using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CznEnemySkin.Domain;

internal sealed class MonsterPresentation
{
	public PresentationMode Mode { get; }

	public string HostVisualsPath { get; }

	public string SkeletonDataPath { get; }

	public float VisualScale { get; }

	public EnemySelectionBounds SelectionBounds { get; }

	public IReadOnlyList<EnemyAnimationBinding> AnimationBindings { get; }

	public string? EntranceAnimation { get; }

	public IReadOnlyDictionary<EnemyAction, EnemyActionPresentation> ActionPresentations { get; }

	public IReadOnlyList<string> IncompatibleNativeVisualChildPaths { get; }

	public IReadOnlyList<string> IncompatibleNativeHostChildPaths { get; }

	public IReadOnlyList<string> NativeParticlePathsToDisable { get; }

	public EnemyNativePresentationPolicy NativePresentation { get; }

	public EnemyDeathBodyPolicy DeathBodyPolicy { get; }

	public IReadOnlyList<EnemySecondaryBodyProfile> SecondaryBodies { get; }

	public IReadOnlyList<EnemyAction> PrewarmEffectActions { get; }

	public EnemyInteractionBoundsAdapter? InteractionBounds { get; }

	public bool UseNativeHostBounds { get; }

	public EnemyBodyOriginPolicy BodyOriginPolicy { get; }

	public EnemyHostTransformPolicy HostTransformPolicy { get; }

	public bool MirrorBodyHorizontally { get; }

	public bool CenterAnchorOnSelectionBounds { get; }

	public bool ApplyNeutralMultiplyToBody { get; }

	public MonsterPresentation(PresentationMode mode, string hostVisualsPath, string skeletonDataPath, float visualScale, EnemySelectionBounds selectionBounds, IEnumerable<EnemyAnimationBinding> animationBindings, string? entranceAnimation, IEnumerable<EnemyActionPresentation>? actionPresentations = null, IEnumerable<string>? incompatibleNativeVisualChildPaths = null, IEnumerable<string>? incompatibleNativeHostChildPaths = null, IEnumerable<string>? nativeParticlePathsToDisable = null, EnemyNativePresentationPolicy? nativePresentation = null, EnemyDeathBodyPolicy deathBodyPolicy = EnemyDeathBodyPolicy.PlayAnimation, IEnumerable<EnemySecondaryBodyProfile>? secondaryBodies = null, IEnumerable<EnemyAction>? prewarmEffectActions = null, EnemyInteractionBoundsAdapter? interactionBounds = null, bool useNativeHostBounds = false, EnemyBodyOriginPolicy bodyOriginPolicy = EnemyBodyOriginPolicy.InheritNativeOffset, EnemyHostTransformPolicy hostTransformPolicy = EnemyHostTransformPolicy.Inherit, bool mirrorBodyHorizontally = false, bool centerAnchorOnSelectionBounds = false, bool applyNeutralMultiplyToBody = false)
	{
		Mode = mode;
		HostVisualsPath = RequireResourcePath(hostVisualsPath, "hostVisualsPath", requireModNamespace: false);
		SkeletonDataPath = RequireResourcePath(skeletonDataPath, "skeletonDataPath", mode != PresentationMode.Inherit);
		if (!float.IsFinite(visualScale) || visualScale <= 0f)
		{
			throw new ArgumentOutOfRangeException("visualScale", "Visual scale must be finite and greater than zero.");
		}
		VisualScale = visualScale;
		SelectionBounds = selectionBounds ?? throw new ArgumentNullException("selectionBounds");
		if (!float.IsFinite(selectionBounds.Left) || !float.IsFinite(selectionBounds.Top) || !float.IsFinite(selectionBounds.Width) || !float.IsFinite(selectionBounds.Height) || selectionBounds.Width <= 0f || selectionBounds.Height <= 0f)
		{
			throw new ArgumentOutOfRangeException("selectionBounds", "Selection bounds must be finite with positive size.");
		}
		EntranceAnimation = (string.IsNullOrWhiteSpace(entranceAnimation) ? null : entranceAnimation.Trim());
		EnemyAnimationBinding[] array = animationBindings?.ToArray() ?? throw new ArgumentNullException("animationBindings");
		if (array.Length == 0)
		{
			throw new ArgumentException("At least one animation binding is required.", "animationBindings");
		}
		HashSet<(EnemyPresentationStance, EnemyAction)> hashSet = new HashSet<(EnemyPresentationStance, EnemyAction)>();
		EnemyAnimationBinding[] array2 = array;
		foreach (EnemyAnimationBinding enemyAnimationBinding in array2)
		{
			if (!hashSet.Add((enemyAnimationBinding.Stance, enemyAnimationBinding.Action)))
			{
				throw new ArgumentException("Duplicate animation binding for " + $"'{enemyAnimationBinding.Stance}/{enemyAnimationBinding.Action}'.", "animationBindings");
			}
			RequireText(enemyAnimationBinding.Sts2AnimationName, "Sts2AnimationName");
			RequireText(enemyAnimationBinding.CznAnimationName, "CznAnimationName");
		}
		AnimationBindings = new ReadOnlyCollection<EnemyAnimationBinding>(array);
		ActionPresentations = BuildActionPresentations(actionPresentations);
		IncompatibleNativeVisualChildPaths = BuildNodePathList(incompatibleNativeVisualChildPaths, "incompatibleNativeVisualChildPaths");
		IncompatibleNativeHostChildPaths = BuildNodePathList(incompatibleNativeHostChildPaths, "incompatibleNativeHostChildPaths");
		NativeParticlePathsToDisable = BuildNodePathList(nativeParticlePathsToDisable, "nativeParticlePathsToDisable");
		NativePresentation = nativePresentation ?? EnemyNativePresentationPolicy.None;
		if (!Enum.IsDefined(deathBodyPolicy))
		{
			throw new ArgumentOutOfRangeException("deathBodyPolicy");
		}
		DeathBodyPolicy = deathBodyPolicy;
		SecondaryBodies = BuildSecondaryBodies(secondaryBodies);
		PrewarmEffectActions = BuildPrewarmEffectActions(prewarmEffectActions, ActionPresentations);
		InteractionBounds = ValidateInteractionBounds(interactionBounds);
		if (useNativeHostBounds && InteractionBounds != null)
		{
			throw new ArgumentException("Native host bounds cannot also use an interaction width cap.", "useNativeHostBounds");
		}
		UseNativeHostBounds = useNativeHostBounds;
		if (!Enum.IsDefined(bodyOriginPolicy))
		{
			throw new ArgumentOutOfRangeException("bodyOriginPolicy");
		}
		BodyOriginPolicy = bodyOriginPolicy;
		if (!Enum.IsDefined(hostTransformPolicy))
		{
			throw new ArgumentOutOfRangeException("hostTransformPolicy");
		}
		HostTransformPolicy = hostTransformPolicy;
		MirrorBodyHorizontally = mirrorBodyHorizontally;
		if (centerAnchorOnSelectionBounds && useNativeHostBounds)
		{
			throw new ArgumentException("A centre anchor derived from the replacement selection bounds cannot also keep the native host bounds.", "centerAnchorOnSelectionBounds");
		}
		CenterAnchorOnSelectionBounds = centerAnchorOnSelectionBounds;
		ApplyNeutralMultiplyToBody = applyNeutralMultiplyToBody;
	}

	private static EnemyInteractionBoundsAdapter? ValidateInteractionBounds(EnemyInteractionBoundsAdapter? value)
	{
		if (value == null)
		{
			return null;
		}
		if (!float.IsFinite(value.MaximumWidth) || value.MaximumWidth <= 0f)
		{
			throw new ArgumentOutOfRangeException("value", "Interaction-bounds maximum width must be finite and positive.");
		}
		return value;
	}

	private static IReadOnlyList<EnemyAction> BuildPrewarmEffectActions(IEnumerable<EnemyAction>? values, IReadOnlyDictionary<EnemyAction, EnemyActionPresentation> presentations)
	{
		EnemyAction[] array = values?.Distinct().ToArray() ?? Array.Empty<EnemyAction>();
		EnemyAction[] array2 = array;
		foreach (EnemyAction enemyAction in array2)
		{
			if (!presentations.TryGetValue(enemyAction, out EnemyActionPresentation value) || value.Effects.Count == 0)
			{
				throw new ArgumentException($"Effect prewarm action '{enemyAction}' has no effect cues.", "values");
			}
		}
		return new ReadOnlyCollection<EnemyAction>(array);
	}

	private static IReadOnlyList<EnemySecondaryBodyProfile> BuildSecondaryBodies(IEnumerable<EnemySecondaryBodyProfile>? values)
	{
		EnemySecondaryBodyProfile[] array = values?.ToArray() ?? Array.Empty<EnemySecondaryBodyProfile>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		EnemySecondaryBodyProfile[] array2 = array;
		foreach (EnemySecondaryBodyProfile enemySecondaryBodyProfile in array2)
		{
			RequireText(enemySecondaryBodyProfile.Role, "Role");
			if (!hashSet.Add(enemySecondaryBodyProfile.Role))
			{
				throw new ArgumentException("Duplicate secondary body role '" + enemySecondaryBodyProfile.Role + "'.", "values");
			}
			RequireResourcePath(enemySecondaryBodyProfile.SkeletonDataPath, "SkeletonDataPath", requireModNamespace: true);
			if (!float.IsFinite(enemySecondaryBodyProfile.VisualScale) || enemySecondaryBodyProfile.VisualScale <= 0f)
			{
				throw new ArgumentOutOfRangeException("values", "Secondary body scale must be finite and positive.");
			}
			RequireText(enemySecondaryBodyProfile.IdleAnimation, "IdleAnimation");
			if (enemySecondaryBodyProfile.EntranceAnimation != null)
			{
				RequireText(enemySecondaryBodyProfile.EntranceAnimation, "EntranceAnimation");
			}
		}
		return new ReadOnlyCollection<EnemySecondaryBodyProfile>(array);
	}

	private static IReadOnlyDictionary<EnemyAction, EnemyActionPresentation> BuildActionPresentations(IEnumerable<EnemyActionPresentation>? values)
	{
		Dictionary<EnemyAction, EnemyActionPresentation> dictionary = new Dictionary<EnemyAction, EnemyActionPresentation>();
		foreach (EnemyActionPresentation item in values ?? Array.Empty<EnemyActionPresentation>())
		{
			if (dictionary.ContainsKey(item.Action))
			{
				throw new ArgumentException($"Duplicate action presentation for '{item.Action}'.", "values");
			}
			EnemyAudioCue[] array = item.AudioChoices?.ToArray() ?? throw new ArgumentException("Action audio choices cannot be null.", "values");
			EnemyAudioCue[] array2 = array;
			foreach (EnemyAudioCue enemyAudioCue in array2)
			{
				RequireResourcePath(enemyAudioCue.ResourcePath, "ResourcePath", requireModNamespace: true);
				if (!float.IsFinite(enemyAudioCue.VolumeLinear) || enemyAudioCue.VolumeLinear <= 0f)
				{
					throw new ArgumentOutOfRangeException("VolumeLinear", "Audio volume must be finite and greater than zero.");
				}
			}
			EnemyAudioLayerCue[] array3 = item.AudioLayers?.ToArray() ?? Array.Empty<EnemyAudioLayerCue>();
			EnemyAudioLayerCue[] array4 = array3;
			foreach (EnemyAudioLayerCue enemyAudioLayerCue in array4)
			{
				RequireResourcePath(enemyAudioLayerCue.ResourcePath, "ResourcePath", requireModNamespace: true);
				if (!float.IsFinite(enemyAudioLayerCue.DelaySeconds) || enemyAudioLayerCue.DelaySeconds < 0f || !float.IsFinite(enemyAudioLayerCue.VolumeLinear) || enemyAudioLayerCue.VolumeLinear <= 0f)
				{
					throw new ArgumentOutOfRangeException("values", "Layered audio delay must be finite and non-negative; volume must be finite and greater than zero.");
				}
			}
			EnemyEffectCue[] array5 = item.Effects?.ToArray() ?? throw new ArgumentException("Action effects cannot be null.", "values");
			EnemyEffectCue[] array6 = array5;
			foreach (EnemyEffectCue enemyEffectCue in array6)
			{
				RequireResourcePath(enemyEffectCue.ScenePath, "ScenePath", requireModNamespace: true);
				if (enemyEffectCue.SpineAnimationName != null)
				{
					RequireText(enemyEffectCue.SpineAnimationName, "SpineAnimationName");
				}
				if (!Enum.IsDefined(typeof(EnemyEffectMaterialPolicy), enemyEffectCue.MaterialPolicy))
				{
					throw new ArgumentOutOfRangeException("values", $"Unsupported effect material policy '{enemyEffectCue.MaterialPolicy}'.");
				}
				if (!float.IsFinite(enemyEffectCue.OfficialPositionX) || !float.IsFinite(enemyEffectCue.OfficialPositionY) || !float.IsFinite(enemyEffectCue.ScaleX) || !float.IsFinite(enemyEffectCue.ScaleY) || !float.IsFinite(enemyEffectCue.OfficialRotationDegrees) || !float.IsFinite(enemyEffectCue.Opacity) || !float.IsFinite(enemyEffectCue.DelaySeconds) || !float.IsFinite(enemyEffectCue.LifetimeSeconds) || enemyEffectCue.ScaleX == 0f || enemyEffectCue.ScaleY == 0f || enemyEffectCue.Opacity <= 0f || enemyEffectCue.Opacity > 1f || enemyEffectCue.DelaySeconds < 0f || enemyEffectCue.LifetimeSeconds <= 0f)
				{
					throw new ArgumentOutOfRangeException("values", "Effect transforms and timing must be finite; scale/lifetime/opacity must be non-zero, opacity no greater than one, and delay non-negative.");
				}
			}
			if (array.Length == 0 && array3.Length == 0 && array5.Length == 0)
			{
				throw new ArgumentException($"Action presentation '{item.Action}' is empty.", "values");
			}
			dictionary.Add(item.Action, new EnemyActionPresentation(item.Action, new ReadOnlyCollection<EnemyAudioCue>(array), new ReadOnlyCollection<EnemyEffectCue>(array5), new ReadOnlyCollection<EnemyAudioLayerCue>(array3)));
		}
		return new ReadOnlyDictionary<EnemyAction, EnemyActionPresentation>(dictionary);
	}

	private static IReadOnlyList<string> BuildNodePathList(IEnumerable<string>? values, string parameterName)
	{
		if (values == null)
		{
			return Array.Empty<string>();
		}
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		foreach (string value in values)
		{
			string text = RequireText(value, parameterName);
			if (text.StartsWith("res://", StringComparison.Ordinal))
			{
				throw new ArgumentException("Godot node path must be relative to its host: " + text, parameterName);
			}
			if (hashSet.Add(text))
			{
				list.Add(text);
			}
		}
		return new ReadOnlyCollection<string>(list);
	}

	private static string RequireResourcePath(string value, string parameterName, bool requireModNamespace)
	{
		string text = RequireText(value, parameterName);
		if (!text.StartsWith("res://", StringComparison.Ordinal))
		{
			throw new ArgumentException("Resource path must start with 'res://': " + text, parameterName);
		}
		if (requireModNamespace && !text.StartsWith("res://CznEnemySkin/", StringComparison.Ordinal))
		{
			throw new ArgumentException("Replacement resource must stay under the res://CznEnemySkin/ namespace (" + text + ").", parameterName);
		}
		return text;
	}

	private static string RequireText(string value, string parameterName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new ArgumentException("Value cannot be empty.", parameterName);
		}
		return value.Trim();
	}
}

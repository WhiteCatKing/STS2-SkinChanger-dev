using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CznEnemySkin.Domain;

internal sealed class EnemyNativePresentationPolicy
{
	public static EnemyNativePresentationPolicy None { get; } = new EnemyNativePresentationPolicy(null, null, null, null);

	public IReadOnlyDictionary<string, EnemyAction> Audio { get; }

	public IReadOnlyDictionary<string, EnemyAction> VfxPaths { get; }

	public IReadOnlyDictionary<EnemyNativeVfxFeature, EnemyAction> VfxFeatures { get; }

	public EnemyAction? DamageSfxReplacementAction { get; }

	public EnemyNativePresentationPolicy(IEnumerable<EnemyNativeAudioSuppression>? audio = null, IEnumerable<EnemyNativeVfxPathSuppression>? vfxPaths = null, IEnumerable<EnemyNativeVfxFeatureSuppression>? vfxFeatures = null, EnemyAction? damageSfxReplacementAction = null)
	{
		Audio = BuildMap(audio, (EnemyNativeAudioSuppression value) => value.ResourcePath, (EnemyNativeAudioSuppression value) => value.ReplacementAction, "native audio");
		VfxPaths = BuildMap(vfxPaths, (EnemyNativeVfxPathSuppression value) => value.ResourcePath, (EnemyNativeVfxPathSuppression value) => value.ReplacementAction, "native VFX path");
		VfxFeatures = BuildMap(vfxFeatures, (EnemyNativeVfxFeatureSuppression value) => value.Feature, (EnemyNativeVfxFeatureSuppression value) => value.ReplacementAction, "native VFX feature");
		DamageSfxReplacementAction = damageSfxReplacementAction;
	}

	private static IReadOnlyDictionary<TKey, EnemyAction> BuildMap<TValue, TKey>(IEnumerable<TValue>? values, Func<TValue, TKey> getKey, Func<TValue, EnemyAction> getAction, string label) where TKey : notnull
	{
		Dictionary<TKey, EnemyAction> dictionary = new Dictionary<TKey, EnemyAction>();
		foreach (TValue item in values ?? Array.Empty<TValue>())
		{
			TKey val = getKey(item);
			if (val is string value && string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException(label + " cannot be empty.", "values");
			}
			if (!dictionary.TryAdd(val, getAction(item)))
			{
				throw new ArgumentException($"Duplicate {label} suppression '{val}'.", "values");
			}
		}
		return new ReadOnlyDictionary<TKey, EnemyAction>(dictionary);
	}
}

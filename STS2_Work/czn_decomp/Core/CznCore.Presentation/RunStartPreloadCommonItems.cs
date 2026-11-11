using System;
using System.Threading.Tasks;
using Godot;

namespace CznCore.Presentation;

internal static class RunStartPreloadCommonItems
{
	public static void Add(RunStartPreloadItemBuilder items, RunStartPreloadCommonItemSources sources)
	{
		ArgumentNullException.ThrowIfNull(items, "items");
		ArgumentNullException.ThrowIfNull(sources, "sources");
		CharacterSkinProfile characterSkinProfile = sources.SkinProfile.For(null);
		RunStartPreloadCommonItemPolicy policy = sources.Policy;
		if (policy.IncludeBodyResources)
		{
			Func<Node, Task> preloadBodyTexture = sources.PreloadBodyTexture;
			if (preloadBodyTexture != null)
			{
				items.AddCustom(characterSkinProfile.BodyTexturePath, characterSkinProfile.BodyTexturePath, "Body texture", preloadBodyTexture);
				goto IL_006f;
			}
		}
		if (policy.IncludeBodyResources)
		{
			items.AddResource(characterSkinProfile.BodyTexturePath, "Body texture");
		}
		goto IL_006f;
		IL_006f:
		if (policy.IncludeBodyResources)
		{
			items.AddResource(characterSkinProfile.BodySkeletonDataPath, "Body skeleton");
			items.AddResource(characterSkinProfile.PremultipliedAlphaMaterialPath, "Body material");
		}
		if (policy.IncludeIdleEffects && (sources.IdleEffectsEnabled ?? sources.EffectsEnabled)())
		{
			Func<Node, Task> preloadIdleEffectsScene = sources.PreloadIdleEffectsScene;
			if (preloadIdleEffectsScene != null)
			{
				items.AddCustom(characterSkinProfile.CombatIdleEffectsScenePath, characterSkinProfile.CombatIdleEffectsScenePath, "Idle effects", preloadIdleEffectsScene);
			}
			else
			{
				items.AddResource(characterSkinProfile.CombatIdleEffectsScenePath, "Idle effects");
			}
		}
		if (!sources.EffectsEnabled())
		{
			return;
		}
		sources.AddBeforeEffectCues?.Invoke(items);
		if (!policy.IncludeTransientEffectCues)
		{
			return;
		}
		foreach (CombatPresentationEffectCue item in characterSkinProfile.CombatPresentation.EnumerateTransientEffectCues())
		{
			CombatPresentationEffectCue combatPresentationEffectCue = CombatAppearanceResources.ResolveEffect(characterSkinProfile, item);
			Action<RunStartPreloadItemBuilder, CombatPresentationEffectCue> addEffectCue = sources.AddEffectCue;
			if (addEffectCue != null)
			{
				addEffectCue(items, combatPresentationEffectCue);
			}
			else
			{
				items.AddEffectSource(combatPresentationEffectCue.Source, "Combat effect");
			}
		}
	}
}

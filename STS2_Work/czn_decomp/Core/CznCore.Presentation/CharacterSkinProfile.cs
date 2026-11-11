using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal sealed record CharacterSkinProfile(string ProfileId, string LogPrefix, string RuntimeNodeNamePrefix, Type? TargetCharacterType, string BodyTexturePath, string BodySkeletonDataPath, string CombatSpriteNodeName, float BodySpineScaleMultiplier, Vector2 CombatPositionOffset, CharacterSkinCombatBodyProfile CombatBody, CharacterSkinFormVfxProfile FormVfx, string PremultipliedAlphaMaterialPath, string CombatIdleEffectsScenePath, CharacterSkinCombatEffectsProfile CombatEffects, CharacterSkinFeatureSupportProfile Features, CharacterSkinSettingsTextProfile SettingsText, CombatCardPresentationProfile CardPresentation, ICombatPresentationProfile CombatPresentation, BattleReadyOverlayProfile BattleReady, CharacterSelectIdentityProfile CharacterSelect, CharacterSelectPresentationProfile CharacterSelectPresentation, RestSitePresentationProfile RestSite, DefeatPresentationProfile Defeat, CharacterVoiceProfile Voice, string GlobalSettingsIconPath = "", Func<Type?>? TargetCharacterTypeResolver = null)
{
	public CharacterAppearanceDefinition? Appearance { get; init; }

	public IReadOnlyList<string> NonCombatBodySkeletonPaths { get; init; } = Array.Empty<string>();

	public IReadOnlyDictionary<CombatAppearanceEffectKey, string> CombatEffectOverrides { get; init; } = new Dictionary<CombatAppearanceEffectKey, string>();

	public string RuntimeMetaKeyPrefix => "character_skin_" + ProfileId;

	private readonly object _targetCharacterTypeSync = new object();

	private Type? _resolvedTargetCharacterType;

	private volatile bool _targetCharacterTypeResolved;

	public Type? ResolveTargetCharacterType()
	{
		if (TargetCharacterType != null)
		{
			return TargetCharacterType;
		}
		if (_targetCharacterTypeResolved)
		{
			return _resolvedTargetCharacterType;
		}
		lock (_targetCharacterTypeSync)
		{
			if (!_targetCharacterTypeResolved)
			{
				_resolvedTargetCharacterType = TargetCharacterTypeResolver?.Invoke();
				_targetCharacterTypeResolved = true;
			}
			return _resolvedTargetCharacterType;
		}
	}

	public bool AppliesTo(CharacterModel? characterModel)
	{
		Type type = ResolveTargetCharacterType();
		if (characterModel != null && type != null && type.IsInstanceOfType(characterModel))
		{
			return CharacterAppearanceCatalog.IsSelected(this, characterModel);
		}
		return false;
	}

	public bool AppliesTo(Player? player)
	{
		if (player != null)
		{
			Type? type = ResolveTargetCharacterType();
			if ((object)type != null && type.IsInstanceOfType(player.Character))
			{
				return CharacterAppearanceCatalog.IsSelected(this, player);
			}
		}
		return false;
	}

	public CharacterSkinProfile For(Player? player)
	{
		return CharacterAppearanceCatalog.Resolve(this, player);
	}

	public string RuntimeNodeName(string suffix)
	{
		return RuntimeNodeNamePrefix + suffix;
	}

	public string RuntimeMetaKey(string suffix)
	{
		return RuntimeMetaKeyPrefix + "_" + suffix;
	}

	[CompilerGenerated]
	private bool PrintMembers(StringBuilder builder)
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		RuntimeHelpers.EnsureSufficientExecutionStack();
		builder.Append("ProfileId = ");
		builder.Append((object?)ProfileId);
		builder.Append(", LogPrefix = ");
		builder.Append((object?)LogPrefix);
		builder.Append(", RuntimeNodeNamePrefix = ");
		builder.Append((object?)RuntimeNodeNamePrefix);
		builder.Append(", TargetCharacterType = ");
		builder.Append(TargetCharacterType);
		builder.Append(", BodyTexturePath = ");
		builder.Append((object?)BodyTexturePath);
		builder.Append(", BodySkeletonDataPath = ");
		builder.Append((object?)BodySkeletonDataPath);
		builder.Append(", CombatSpriteNodeName = ");
		builder.Append((object?)CombatSpriteNodeName);
		builder.Append(", BodySpineScaleMultiplier = ");
		builder.Append(BodySpineScaleMultiplier.ToString());
		builder.Append(", CombatPositionOffset = ");
		Vector2 combatPositionOffset = CombatPositionOffset;
		builder.Append(((object)(Vector2)(ref combatPositionOffset)/*cast due to .constrained prefix*/).ToString());
		builder.Append(", CombatBody = ");
		builder.Append(CombatBody);
		builder.Append(", FormVfx = ");
		builder.Append(FormVfx);
		builder.Append(", PremultipliedAlphaMaterialPath = ");
		builder.Append((object?)PremultipliedAlphaMaterialPath);
		builder.Append(", CombatIdleEffectsScenePath = ");
		builder.Append((object?)CombatIdleEffectsScenePath);
		builder.Append(", CombatEffects = ");
		builder.Append(CombatEffects);
		builder.Append(", Features = ");
		builder.Append(Features);
		builder.Append(", SettingsText = ");
		builder.Append(SettingsText);
		builder.Append(", CardPresentation = ");
		builder.Append(CardPresentation);
		builder.Append(", CombatPresentation = ");
		builder.Append(CombatPresentation);
		builder.Append(", BattleReady = ");
		builder.Append(BattleReady);
		builder.Append(", CharacterSelect = ");
		builder.Append(CharacterSelect);
		builder.Append(", CharacterSelectPresentation = ");
		builder.Append(CharacterSelectPresentation);
		builder.Append(", RestSite = ");
		builder.Append(RestSite);
		builder.Append(", Defeat = ");
		builder.Append(Defeat);
		builder.Append(", Voice = ");
		builder.Append(Voice);
		builder.Append(", GlobalSettingsIconPath = ");
		builder.Append((object?)GlobalSettingsIconPath);
		builder.Append(", TargetCharacterTypeResolver = ");
		builder.Append(TargetCharacterTypeResolver);
		builder.Append(", Appearance = ");
		builder.Append(Appearance);
		builder.Append(", NonCombatBodySkeletonPaths = ");
		builder.Append(NonCombatBodySkeletonPaths);
		builder.Append(", CombatEffectOverrides = ");
		builder.Append(CombatEffectOverrides);
		builder.Append(", RuntimeMetaKeyPrefix = ");
		builder.Append((object?)RuntimeMetaKeyPrefix);
		return true;
	}

	[CompilerGenerated]
	public override int GetHashCode()
	{
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		return (((((((((((((((((((((((((((((((EqualityComparer<Type>.Default.GetHashCode(EqualityContract) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ProfileId)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(LogPrefix)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(RuntimeNodeNamePrefix)) * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(TargetCharacterType)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BodyTexturePath)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(BodySkeletonDataPath)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CombatSpriteNodeName)) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(BodySpineScaleMultiplier)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(CombatPositionOffset)) * -1521134295 + EqualityComparer<CharacterSkinCombatBodyProfile>.Default.GetHashCode(CombatBody)) * -1521134295 + EqualityComparer<CharacterSkinFormVfxProfile>.Default.GetHashCode(FormVfx)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PremultipliedAlphaMaterialPath)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CombatIdleEffectsScenePath)) * -1521134295 + EqualityComparer<CharacterSkinCombatEffectsProfile>.Default.GetHashCode(CombatEffects)) * -1521134295 + EqualityComparer<CharacterSkinFeatureSupportProfile>.Default.GetHashCode(Features)) * -1521134295 + EqualityComparer<CharacterSkinSettingsTextProfile>.Default.GetHashCode(SettingsText)) * -1521134295 + EqualityComparer<CombatCardPresentationProfile>.Default.GetHashCode(CardPresentation)) * -1521134295 + EqualityComparer<ICombatPresentationProfile>.Default.GetHashCode(CombatPresentation)) * -1521134295 + EqualityComparer<BattleReadyOverlayProfile>.Default.GetHashCode(BattleReady)) * -1521134295 + EqualityComparer<CharacterSelectIdentityProfile>.Default.GetHashCode(CharacterSelect)) * -1521134295 + EqualityComparer<CharacterSelectPresentationProfile>.Default.GetHashCode(CharacterSelectPresentation)) * -1521134295 + EqualityComparer<RestSitePresentationProfile>.Default.GetHashCode(RestSite)) * -1521134295 + EqualityComparer<DefeatPresentationProfile>.Default.GetHashCode(Defeat)) * -1521134295 + EqualityComparer<CharacterVoiceProfile>.Default.GetHashCode(Voice)) * -1521134295 + EqualityComparer<string>.Default.GetHashCode(GlobalSettingsIconPath)) * -1521134295 + EqualityComparer<Func<Type>>.Default.GetHashCode(TargetCharacterTypeResolver)) * -1521134295 + EqualityComparer<CharacterAppearanceDefinition>.Default.GetHashCode(Appearance)) * -1521134295 + EqualityComparer<IReadOnlyList<string>>.Default.GetHashCode(NonCombatBodySkeletonPaths)) * -1521134295 + EqualityComparer<IReadOnlyDictionary<CombatAppearanceEffectKey, string>>.Default.GetHashCode(CombatEffectOverrides)) * -1521134295 + EqualityComparer<object>.Default.GetHashCode(_targetCharacterTypeSync)) * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(_resolvedTargetCharacterType)) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(_targetCharacterTypeResolved);
	}

	[CompilerGenerated]
	public bool Equals(CharacterSkinProfile? other)
	{
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		if ((object)this != other)
		{
			if ((object)other != null && EqualityContract == other.EqualityContract && EqualityComparer<string>.Default.Equals(ProfileId, other.ProfileId) && EqualityComparer<string>.Default.Equals(LogPrefix, other.LogPrefix) && EqualityComparer<string>.Default.Equals(RuntimeNodeNamePrefix, other.RuntimeNodeNamePrefix) && EqualityComparer<Type>.Default.Equals(TargetCharacterType, other.TargetCharacterType) && EqualityComparer<string>.Default.Equals(BodyTexturePath, other.BodyTexturePath) && EqualityComparer<string>.Default.Equals(BodySkeletonDataPath, other.BodySkeletonDataPath) && EqualityComparer<string>.Default.Equals(CombatSpriteNodeName, other.CombatSpriteNodeName) && EqualityComparer<float>.Default.Equals(BodySpineScaleMultiplier, other.BodySpineScaleMultiplier) && EqualityComparer<Vector2>.Default.Equals(CombatPositionOffset, other.CombatPositionOffset) && EqualityComparer<CharacterSkinCombatBodyProfile>.Default.Equals(CombatBody, other.CombatBody) && EqualityComparer<CharacterSkinFormVfxProfile>.Default.Equals(FormVfx, other.FormVfx) && EqualityComparer<string>.Default.Equals(PremultipliedAlphaMaterialPath, other.PremultipliedAlphaMaterialPath) && EqualityComparer<string>.Default.Equals(CombatIdleEffectsScenePath, other.CombatIdleEffectsScenePath) && EqualityComparer<CharacterSkinCombatEffectsProfile>.Default.Equals(CombatEffects, other.CombatEffects) && EqualityComparer<CharacterSkinFeatureSupportProfile>.Default.Equals(Features, other.Features) && EqualityComparer<CharacterSkinSettingsTextProfile>.Default.Equals(SettingsText, other.SettingsText) && EqualityComparer<CombatCardPresentationProfile>.Default.Equals(CardPresentation, other.CardPresentation) && EqualityComparer<ICombatPresentationProfile>.Default.Equals(CombatPresentation, other.CombatPresentation) && EqualityComparer<BattleReadyOverlayProfile>.Default.Equals(BattleReady, other.BattleReady) && EqualityComparer<CharacterSelectIdentityProfile>.Default.Equals(CharacterSelect, other.CharacterSelect) && EqualityComparer<CharacterSelectPresentationProfile>.Default.Equals(CharacterSelectPresentation, other.CharacterSelectPresentation) && EqualityComparer<RestSitePresentationProfile>.Default.Equals(RestSite, other.RestSite) && EqualityComparer<DefeatPresentationProfile>.Default.Equals(Defeat, other.Defeat) && EqualityComparer<CharacterVoiceProfile>.Default.Equals(Voice, other.Voice) && EqualityComparer<string>.Default.Equals(GlobalSettingsIconPath, other.GlobalSettingsIconPath) && EqualityComparer<Func<Type>>.Default.Equals(TargetCharacterTypeResolver, other.TargetCharacterTypeResolver) && EqualityComparer<CharacterAppearanceDefinition>.Default.Equals(Appearance, other.Appearance) && EqualityComparer<IReadOnlyList<string>>.Default.Equals(NonCombatBodySkeletonPaths, other.NonCombatBodySkeletonPaths) && EqualityComparer<IReadOnlyDictionary<CombatAppearanceEffectKey, string>>.Default.Equals(CombatEffectOverrides, other.CombatEffectOverrides) && EqualityComparer<object>.Default.Equals(_targetCharacterTypeSync, other._targetCharacterTypeSync) && EqualityComparer<Type>.Default.Equals(_resolvedTargetCharacterType, other._resolvedTargetCharacterType))
			{
				return EqualityComparer<bool>.Default.Equals(_targetCharacterTypeResolved, other._targetCharacterTypeResolved);
			}
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	private CharacterSkinProfile(CharacterSkinProfile original)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		ProfileId = original.ProfileId;
		LogPrefix = original.LogPrefix;
		RuntimeNodeNamePrefix = original.RuntimeNodeNamePrefix;
		TargetCharacterType = original.TargetCharacterType;
		BodyTexturePath = original.BodyTexturePath;
		BodySkeletonDataPath = original.BodySkeletonDataPath;
		CombatSpriteNodeName = original.CombatSpriteNodeName;
		BodySpineScaleMultiplier = original.BodySpineScaleMultiplier;
		CombatPositionOffset = original.CombatPositionOffset;
		CombatBody = original.CombatBody;
		FormVfx = original.FormVfx;
		PremultipliedAlphaMaterialPath = original.PremultipliedAlphaMaterialPath;
		CombatIdleEffectsScenePath = original.CombatIdleEffectsScenePath;
		CombatEffects = original.CombatEffects;
		Features = original.Features;
		SettingsText = original.SettingsText;
		CardPresentation = original.CardPresentation;
		CombatPresentation = original.CombatPresentation;
		BattleReady = original.BattleReady;
		CharacterSelect = original.CharacterSelect;
		CharacterSelectPresentation = original.CharacterSelectPresentation;
		RestSite = original.RestSite;
		Defeat = original.Defeat;
		Voice = original.Voice;
		GlobalSettingsIconPath = original.GlobalSettingsIconPath;
		TargetCharacterTypeResolver = original.TargetCharacterTypeResolver;
		Appearance = original.Appearance;
		NonCombatBodySkeletonPaths = original.NonCombatBodySkeletonPaths;
		CombatEffectOverrides = original.CombatEffectOverrides;
		_targetCharacterTypeSync = original._targetCharacterTypeSync;
		_resolvedTargetCharacterType = original._resolvedTargetCharacterType;
		_targetCharacterTypeResolved = original._targetCharacterTypeResolved;
	}
}

namespace MeirinWatcherSkin.Characters.Meirin;

internal readonly record struct MeirinCardCapabilities(bool ChangesStance, bool EntersDivinity, bool MantraRelated, bool AppliesPower, bool UsesRandomOpponentTargeting)
{
	public static MeirinCardCapabilities None { get; } = new MeirinCardCapabilities(ChangesStance: false, EntersDivinity: false, MantraRelated: false, AppliesPower: false, UsesRandomOpponentTargeting: false);
}

using System.Collections.Generic;
using System.Reflection;
using CznCore.Standalone;
using CznEnemySkin.Application;
using CznEnemySkin.Configuration;
using CznEnemySkin.Domain;
using CznEnemySkin.Infrastructure.Persistence;
using CznEnemySkin.Infrastructure.Runtime;
using CznEnemySkin.Profiles;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace CznEnemySkin;

[ModInitializer("Init")]
public static class Entry
{
	private const string HarmonyId = "local.cznenemyskin";

	internal static Logger Log { get; } = new Logger("CznEnemySkin", (LogType)0);

	internal static EnemyReplacementService Replacements { get; private set; } = null;

	internal static ConfigFileEnemyPresentationSettings PresentationSettings { get; private set; } = null;

	internal static ActPresentationService ActPresentations { get; private set; } = null;

	internal static EncounterBattlefieldService EncounterBattlefields { get; private set; } = null;

	internal static LayeredBackgroundRuntime Backgrounds { get; private set; } = null;

	internal static KaiserCrabBattlefieldAdapter KaiserCrabBattlefield { get; private set; } = null;

	internal static RunMusicService Music { get; private set; } = null;

	internal static RunMusicRuntime RunMusic { get; private set; } = null;

	internal static EnemyVisualHostAdapter VisualHostAdapter { get; private set; } = null;

	internal static EnemyActionPresentationRuntime ActionPresentation { get; private set; } = null;

	internal static EnemyNativePresentationRuntime NativePresentation { get; private set; } = null;

	internal static EnemyMoveSemanticRuntime MoveSemantics { get; private set; } = null;

	internal static CeremonialBeastPlowMotionAdapter PlowMotion { get; private set; } = null;

	internal static BossIconRuntime BossIcons { get; private set; } = null;

	internal static EnemyEffectMaterialRuntime EffectMaterials { get; private set; } = null;

	public static void Init()
	{
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		StandaloneEntry.RequireApi(1);
		EnemyReplacementProfile profile = MountainGodCeremonialBeastProfile.Profile;
		IReadOnlyList<EnemyReplacementProfile> profiles = GeneratedOrdinaryEnemyProfiles.Profiles;
		int num = 0;
		EnemyReplacementProfile[] array = new EnemyReplacementProfile[1 + profiles.Count];
		array[num] = profile;
		num++;
		foreach (EnemyReplacementProfile item in profiles)
		{
			array[num] = item;
			num++;
		}
		EnemyReplacementCatalog enemyReplacementCatalog = new EnemyReplacementCatalog(array);
		PresentationSettings = new ConfigFileEnemyPresentationSettings(delegate(string message)
		{
			Log.Warn(message, 1);
		});
		ConfigFileEnemySkinSettingsRepository settings = new ConfigFileEnemySkinSettingsRepository(enemyReplacementCatalog.Profiles, delegate(string message)
		{
			Log.Warn(message, 1);
		});
		Replacements = new EnemyReplacementService(enemyReplacementCatalog, settings);
		List<ActPresentationProfile> list = new List<ActPresentationProfile>();
		list.AddRange(OvergrowthLainoOutdoor02Profile.Profiles);
		list.AddRange(HiveSpaitzOutdoor12Profile.Profiles);
		list.AddRange(GloryEllinadOutdoor22Profile.Profiles);
		list.AddRange(UnderdocksEarthOutdoor32Profile.Profiles);
		ActPresentationCatalog actPresentationCatalog = new ActPresentationCatalog(new _003C_003Ez__ReadOnlyList<ActPresentationProfile>(list));
		ConfigFileActPresentationSettingsRepository settings2 = new ConfigFileActPresentationSettingsRepository(actPresentationCatalog.Profiles, delegate(string message)
		{
			Log.Warn(message, 1);
		});
		ActPresentations = new ActPresentationService(actPresentationCatalog, settings2);
		EncounterBattlefieldCatalog encounterBattlefieldCatalog = new EncounterBattlefieldCatalog(SelectedEncounterBattlefieldProfiles.Profiles);
		ConfigFileEncounterBattlefieldSettingsRepository settings3 = new ConfigFileEncounterBattlefieldSettingsRepository(encounterBattlefieldCatalog.Profiles, delegate(string message)
		{
			Log.Warn(message, 1);
		});
		EncounterBattlefields = new EncounterBattlefieldService(encounterBattlefieldCatalog, settings3);
		Backgrounds = new LayeredBackgroundRuntime();
		KaiserCrabBattlefield = new KaiserCrabBattlefieldAdapter(Backgrounds);
		RunMusicCatalog runMusicCatalog = new RunMusicCatalog(MusicTrackProfiles.All, ActMapMusicProfiles.All, CuratedBossMusicProfiles.All);
		ConfigFileRunMusicSettingsRepository settings4 = new ConfigFileRunMusicSettingsRepository(runMusicCatalog.ActMapProfiles, delegate(string message)
		{
			Log.Warn(message, 1);
		});
		Music = new RunMusicService(runMusicCatalog, settings4, delegate(string message)
		{
			Log.Warn(message, 1);
		});
		RunMusic = new RunMusicRuntime(Music, new NativeMusicHostAdapter());
		CznModConfigRegistration.Register();
		EnemyEffectMaterialRuntime effectMaterials = new EnemyEffectMaterialRuntime(() => PresentationSettings.EffectBrightness);
		EffectMaterials = effectMaterials;
		PresentationSettings.EffectBrightnessChanged += delegate
		{
			effectMaterials.RefreshIntensity();
		};
		VisualHostAdapter = new EnemyVisualHostAdapter(effectMaterials);
		ActionPresentation = new EnemyActionPresentationRuntime(effectMaterials, PresentationSettings.ActionEffectsEnabled);
		PresentationSettings.ActionEffectsEnabledChanged += ActionPresentation.SetEffectsEnabled;
		NativePresentation = new EnemyNativePresentationRuntime(enemyReplacementCatalog.Profiles);
		MoveSemantics = new EnemyMoveSemanticRuntime();
		PlowMotion = new CeremonialBeastPlowMotionAdapter();
		BossIcons = new BossIconRuntime();
		EnemyDefensePresentation.Initialize();
		new Harmony("local.cznenemyskin").PatchAll(Assembly.GetExecutingAssembly());
		Log.Info("Initialized CZN Enemy Skin catalog " + $"[profiles={enemyReplacementCatalog.Profiles.Count}, " + $"actProfiles={actPresentationCatalog.Profiles.Count}, " + "encounterBattlefieldProfiles=" + $"{encounterBattlefieldCatalog.Profiles.Count}, " + $"musicTracks={runMusicCatalog.Tracks.Count}, " + $"actMapMusicProfiles={runMusicCatalog.ActMapProfiles.Count}, " + "curatedBossMusicProfiles=" + $"{runMusicCatalog.CuratedBossProfiles.Count}, " + "mountainGodMode=" + $"{MountainGodCeremonialBeastProfile.Profile.Monster.Mode}, " + "ceremonialBeastBattlefield=" + BattlefieldPresentationProfiles.LainoBoss1.Id + ", overgrowthBackgroundMode=" + $"{OvergrowthLainoOutdoor02Profile.Profile.Battlefield.Background.Mode}, " + "overgrowthBgmMode=" + $"{OvergrowthLainoOutdoor02Profile.Profile.Battlefield.Bgm.Mode}, " + "showOriginalEnemyName=" + $"{PresentationSettings.ShowOriginalEnemyName}, " + $"actionEffectsEnabled={PresentationSettings.ActionEffectsEnabled}, " + "effectBrightness=" + $"{PresentationSettings.EffectBrightness:F2}, " + $"bgmSource={Music.BgmSource}, " + $"bossBgmSource={Music.BossBgmSource}].", 1);
	}
}

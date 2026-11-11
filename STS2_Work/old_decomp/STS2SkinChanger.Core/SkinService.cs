using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Pck;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class SkinService
{
	private sealed class MountedRuntimeResourceScope(IReadOnlySet<string> restoreGroups, IDisposable? manualModeScope) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			lock (Sync)
			{
				if (_disposed)
				{
					return;
				}
				_disposed = true;
				manualModeScope?.Dispose();
				try
				{
					MountOverlay(restoreGroups);
				}
				catch (Exception value)
				{
					ModLog.Error($"恢复临时运行资源覆盖失败：{value}");
				}
			}
		}
	}

	private sealed record CanonicalRuntimeResourceOwnershipEntry(string CanonicalPath, string AliasPath, Resource SelectedResource, Resource? PreviousResource);

	private sealed class CanonicalRuntimeResourceOwnershipScope(IReadOnlyList<CanonicalRuntimeResourceOwnershipEntry> entries) : IDisposable
	{
		private bool _disposed;

		public void Dispose()
		{
			if (_disposed)
			{
				return;
			}
			_disposed = true;
			for (int num = entries.Count - 1; num >= 0; num--)
			{
				CanonicalRuntimeResourceOwnershipEntry canonicalRuntimeResourceOwnershipEntry = entries[num];
				try
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)canonicalRuntimeResourceOwnershipEntry.SelectedResource))
					{
						canonicalRuntimeResourceOwnershipEntry.SelectedResource.TakeOverPath(canonicalRuntimeResourceOwnershipEntry.AliasPath);
					}
					if (canonicalRuntimeResourceOwnershipEntry.PreviousResource != null && GodotObject.IsInstanceValid((GodotObject)(object)canonicalRuntimeResourceOwnershipEntry.PreviousResource))
					{
						canonicalRuntimeResourceOwnershipEntry.PreviousResource.TakeOverPath(canonicalRuntimeResourceOwnershipEntry.CanonicalPath);
					}
				}
				catch (Exception ex)
				{
					ModLog.Warn("恢复角色规范资源 " + canonicalRuntimeResourceOwnershipEntry.CanonicalPath + " 失败：" + ex.Message);
				}
			}
		}
	}

	private sealed class NoopDisposable : IDisposable
	{
		public static NoopDisposable Instance { get; } = new NoopDisposable();

		public void Dispose()
		{
		}
	}

	private sealed record CardLookup(string GroupId, string CardType, CardSkinGroup? Group, IReadOnlyList<CardSkinOption> Options, IReadOnlyDictionary<string, CardOptionLookup> OptionsById, IReadOnlyList<CardSkinGroup> PriorityGroups)
	{
		public bool BaselineUnavailable { get; init; }
	}

	private sealed record CardOptionLookup(CardSkinOption Option, IReadOnlyList<string> MatchedAssetPaths);

	private sealed record CardCoverageState(int TotalCards, IReadOnlyDictionary<string, int> ByOption);

	private sealed record CardPortraitRequest(string GroupId, string Selection, string ResourcePath, string CacheKey, bool UseSelectedProvider, bool WrapAtlas)
	{
		public string OverlayKey => $"{GroupId}\n{Selection}\n{(UseSelectedProvider ? "provider" : "base")}";
	}

	private sealed record PreparedRuntimeOverlay(string Key, string AliasToken, string? OverlayPath, IReadOnlyDictionary<string, string> ResourcePaths, IReadOnlySet<string> CanonicalDependencyPaths, IReadOnlySet<string> RestoreGroups, int FileCount, long FileSize);

	private sealed record RuntimeResourceBundleState(Dictionary<string, Resource> Resources);

	private sealed class IsolatedCardOverlayState(string ownerId)
	{
		public string OwnerId { get; } = ownerId;

		public string? AliasToken { get; set; }

		public Dictionary<string, string> ResourcePaths { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public HashSet<string> UnavailablePaths { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public List<string> OverlayPaths { get; } = new List<string>();

		public HashSet<string> MountedFilePaths { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public HashSet<string> CanonicalRedirectPaths { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public bool CanReuseExternalDependencies { get; set; } = true;
	}

	private sealed record LocalizationCacheState(Dictionary<string, LocTable> Tables, bool OverridesActive, IReadOnlyList<LocValidationError> ValidationErrors);

	internal sealed record EventPriorityOptionState(string OptionId, string Name, bool Enabled, int Coverage, int TotalEvents);

	private static CharacterSkinBundleRunState? _characterSkinBundleRunState;

	private static string? _characterSkinBundleRunSavePath;

	private static SkinConfig? _characterSkinBundleRunSnapshot;

	private static HashSet<string> _characterSkinBundleRunVisualGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static HashSet<string> _characterSkinBundleRunCardGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public const float MinimumMonsterScale = 0.2f;

	public const float MaximumMonsterScale = 5f;

	public const float MonsterScaleStep = 0.05f;

	public const float MinimumCharacterScale = 0.2f;

	public const float MaximumCharacterScale = 5f;

	public const float CharacterScaleStep = 0.05f;

	public const float MinimumCharacterOffset = -1000f;

	public const float MaximumCharacterOffset = 1000f;

	public const float CharacterOffsetStep = 1f;

	public const string InheritCardSelectionId = "__inherit__";

	public const string InheritMonsterSelectionId = "__monster_category__";

	private const string LegacyFollowCharacterSkinIconSelectionId = "__follow_character_skin__";

	public const int CardSkinPresetNameMaxLength = 40;

	public const int MonsterSkinPresetNameMaxLength = 40;

	private const long DirectRuntimeProviderPackThresholdBytes = 67108864L;

	private static readonly object Sync = new object();

	private static readonly Dictionary<string, Resource> RuntimeResourceCache = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, PreparedRuntimeOverlay> PreparedRuntimeOverlays = new Dictionary<string, PreparedRuntimeOverlay>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, RuntimeResourceBundleState> RuntimeResourceBundles = new Dictionary<string, RuntimeResourceBundleState>(StringComparer.OrdinalIgnoreCase);

	private const int MaxCardPortraitCacheEntries = 64;

	private static readonly BoundedLruCache<string, Texture2D> CardPortraitCache = new BoundedLruCache<string, Texture2D>(64, StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> ExternalCardProviderIdentityPaths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, Texture2D> BaselineRelicAtlasCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, Texture2D> BaselineRelicIconCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> FailedCardPortraitRequests = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, CardCoverageState> CardCoverageCache = new Dictionary<string, CardCoverageState>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, IsolatedCardOverlayState> IsolatedCardOverlayCache = new Dictionary<string, IsolatedCardOverlayState>(StringComparer.OrdinalIgnoreCase);

	private static readonly CanonicalResourceOwnershipTracker CardCanonicalResourceOwners = new CanonicalResourceOwnershipTracker();

	private static readonly Dictionary<string, string> CardPreviewSelections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> CharacterPreviewSelections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, MethodInfo> AncientStyleMethods = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> MissingAncientStyleMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> FailedAncientStyleMethods = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, string> MountedOverlayCache = new Dictionary<string, string>(StringComparer.Ordinal);

	private static readonly HashSet<string> MountedScopedRuntimeProviderPacks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, MountedProviderPackState> MountedLargeRuntimeProviderPacks = new Dictionary<string, MountedProviderPackState>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, HashSet<string>> RuntimeCanonicalDependencyPaths = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> WarmedRuntimeProviderPacks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> WarmingRuntimeProviderPacks = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly SemaphoreSlim RuntimePackWarmGate = new SemaphoreSlim(1, 1);

	private static readonly ScopedMonsterSelectionSnapshot ScopedMonsterSelections = new ScopedMonsterSelectionSnapshot();

	private static RuntimeProviderScope? _runtimeProviderBehaviorScope;

	private static readonly RuntimeProviderScopeLeaseTracker RuntimeProviderScopeLeases = new RuntimeProviderScopeLeaseTracker();

	private static readonly Dictionary<string, ResourceFile> MountedLocalizationFiles = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, LocalizationCacheState> LocalizationStateCache = new Dictionary<string, LocalizationCacheState>(StringComparer.Ordinal);

	private static readonly FieldInfo? LocTablesField = AccessTools.Field(typeof(LocManager), "_tables");

	private static readonly MethodInfo? SetLanguageInternalMethod = AccessTools.Method(typeof(LocManager), "SetLanguageInternal", (Type[])null, (Type[])null);

	private static readonly HashSet<string> SharedCardPoolIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "event", "token", "status", "curse", "quest", "deprived", "deprecated", "mock" };

	private static int _overlayGeneration;

	private static int _runtimePackWarmGeneration;

	private static string _sessionId = $"{DateTime.Now:yyyyMMdd-HHmmss}-{Environment.ProcessId}-{Guid.NewGuid():N}";

	private static bool _initialized;

	private static bool _configLoaded;

	private static bool _cardGroupsInitialized;

	private static bool _loadOrderAutoReorderLogged;

	private static string? _mountedLocalizationSignature;

	private static ConditionalWeakTable<CardModel, CardLookup> _cardLookupCache = new ConditionalWeakTable<CardModel, CardLookup>();

	private static readonly ConditionalWeakTable<SkinCatalog, Dictionary<Type, string>> CharacterBehaviorGroups = new ConditionalWeakTable<SkinCatalog, Dictionary<Type, string>>();

	private static volatile string? _lastError;

	private static readonly HashSet<CardRarity> MiscCardRarities = new HashSet<CardRarity>
	{
		(CardRarity)6,
		(CardRarity)7,
		(CardRarity)8,
		(CardRarity)9,
		(CardRarity)10
	};

	public const string InheritEventSelectionId = "__event_category__";

	private static string? _workshopGamePack;

	private static SkinModDescriptor[] _workshopDescriptors = Array.Empty<SkinModDescriptor>();

	private static Dictionary<string, (long Length, DateTime Modified)> _workshopSourceStamps = new Dictionary<string, (long, DateTime)>();

	private static readonly SemaphoreSlim WorkshopRegistrationGate = new SemaphoreSlim(1);

	private static string CharacterSkinBundleRunSnapshotPath => SkinChangerPaths.Configuration("skin_changer_bundle_run_restore.json");

	public static SkinCatalog? Catalog { get; private set; }

	public static SkinConfig Config { get; private set; } = new SkinConfig();

	public static string? LastError
	{
		get
		{
			return _lastError;
		}
		private set
		{
			_lastError = value;
		}
	}

	private static string ConfigPath => SkinChangerPaths.Configuration("skin_changer.json");

	private static string LegacyConfigPath => SkinChangerPaths.Configuration("sts2_skin_switcher.json");

	internal static IReadOnlyList<BundleModSource> GetBundleModSources(bool monsters)
	{
		lock (Sync)
		{
			return (monsters ? (from option in GetBundleMonsterCategoryIds().SelectMany(GetMonsterCategoryOptionsInternal)
				select new BundleModSource(option.OptionId, option.Name)) : (from option in GetBundleCardGroups().SelectMany((CardSkinGroup @group) => @group.Options)
				select new BundleModSource(option.Id, option.Name))).DistinctBy<BundleModSource, string>((BundleModSource source) => source.OptionId, StringComparer.OrdinalIgnoreCase).OrderBy<BundleModSource, string>((BundleModSource source) => source.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
		}
	}

	internal static List<BundleModPriorityEntry> GetBundleModPriority(CharacterSkinBundle bundle, bool monsters)
	{
		lock (Sync)
		{
			return CharacterSkinBundlePolicy.CompletePriority(from entry in CharacterSkinBundlePolicy.NormalizePriority(monsters ? bundle.MonsterModPriority : bundle.CardModPriority)
				select entry with
				{
					OptionId = Resolve(entry.OptionId)
				}, from source in GetBundleModSources(monsters)
				select source.OptionId);
		}
		string Resolve(string id)
		{
			if (monsters)
			{
				foreach (string bundleMonsterCategoryId in GetBundleMonsterCategoryIds())
				{
					string text = ResolveStoredMonsterPriorityOptionId(bundleMonsterCategoryId, id);
					if (!text.Equals(id, StringComparison.OrdinalIgnoreCase))
					{
						return text;
					}
				}
			}
			else
			{
				SkinCatalog catalog = Catalog;
				if (catalog != null)
				{
					foreach (CardSkinGroup bundleCardGroup in GetBundleCardGroups())
					{
						string text2 = catalog.ResolveStoredCardSelectionId(bundleCardGroup.Id, id);
						if (!text2.Equals(id, StringComparison.OrdinalIgnoreCase))
						{
							return text2;
						}
					}
				}
			}
			return id;
		}
	}

	private static List<CardSkinPreset> ResolveBundleCardPresets(CharacterSkinBundle bundle)
	{
		if (bundle.CardMode != BundleContentMode.ModPriority)
		{
			HashSet<string> categories = (from @group in GetBundleCardGroups()
				select @group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			return (from pair in bundle.CardPresetNames
				where categories.Contains(pair.Key)
				select FindCardSkinPresetIndex(pair.Key, pair.Value) into index
				where index >= 0
				select Config.CardSkinPresets[index].Clone()).ToList();
		}
		List<BundleModPriorityEntry> priority = GetBundleModPriority(bundle, monsters: false);
		return GetBundleCardGroups().Select(delegate(CardSkinGroup group)
		{
			List<BundleModPriorityEntry> source = CharacterSkinBundlePolicy.ProjectPriority(priority, group.Options.Select((CardSkinOption option) => option.Id));
			return new CardSkinPreset
			{
				CategoryId = group.Id,
				CardSkinPriorities = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase) { [group.Id] = source.Select((BundleModPriorityEntry entry) => new CardSkinPriorityEntry(entry.OptionId, entry.Enabled)).ToList() },
				Selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [CardSelectionKey(group.Id)] = source.FirstOrDefault((BundleModPriorityEntry entry) => entry.Enabled)?.OptionId ?? "__base__" }
			};
		}).ToList();
	}

	private static List<MonsterSkinPreset> ResolveBundleMonsterPresets(CharacterSkinBundle bundle)
	{
		if (bundle.MonsterMode != BundleContentMode.ModPriority)
		{
			HashSet<string> categories = GetBundleMonsterCategoryIds().ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			return (from pair in bundle.MonsterPresetNames
				where categories.Contains(pair.Key)
				select FindMonsterSkinPresetIndex(pair.Key, pair.Value) into index
				where index >= 0
				select Config.MonsterSkinPresets[index].Clone()).ToList();
		}
		List<BundleModPriorityEntry> priority = GetBundleModPriority(bundle, monsters: true);
		return GetBundleMonsterCategoryIds().Select(delegate(string category)
		{
			string[] source = Config.MonsterSkinCategoryGroups[category].Where((string id) => Catalog.Groups.Any((SkinGroup group) => group.Id.Equals(id, StringComparison.OrdinalIgnoreCase))).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
			List<BundleModPriorityEntry> source2 = CharacterSkinBundlePolicy.ProjectPriority(priority, from option in GetMonsterCategoryOptionsInternal(category)
				select option.OptionId);
			return new MonsterSkinPreset
			{
				CategoryId = category,
				Priority = source2.Select((BundleModPriorityEntry entry) => new MonsterSkinPriorityEntry(entry.OptionId, entry.Enabled)).ToList(),
				Selections = source.ToDictionary<string, string, string>((string id) => id, (string _) => "__base__", StringComparer.OrdinalIgnoreCase),
				FollowingGroupIds = source.ToList()
			};
		}).ToList();
	}

	internal static (float X, float Y)? GetCharacterModelPreviewPosition()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			float? characterModelPreviewX = Config.CharacterModelPreviewX;
			(float, float)? result;
			if (characterModelPreviewX.HasValue)
			{
				float valueOrDefault = characterModelPreviewX.GetValueOrDefault();
				characterModelPreviewX = Config.CharacterModelPreviewY;
				if (characterModelPreviewX.HasValue)
				{
					float valueOrDefault2 = characterModelPreviewX.GetValueOrDefault();
					if (float.IsFinite(valueOrDefault) && float.IsFinite(valueOrDefault2))
					{
						result = (Math.Clamp(valueOrDefault, 0f, 1f), Math.Clamp(valueOrDefault2, 0f, 1f));
						goto IL_0095;
					}
				}
			}
			result = null;
			goto IL_0095;
			IL_0095:
			return result;
		}
	}

	internal static void SetCharacterModelPreviewPosition(float x, float y)
	{
		SetCharacterModelPreviewPlacement(x, y, docked: false);
	}

	internal static bool IsCharacterModelPreviewDocked()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			return Config.CharacterModelPreviewDocked;
		}
	}

	internal static void SetCharacterModelPreviewPlacement(float x, float y, bool docked)
	{
		if (!float.IsFinite(x) || !float.IsFinite(y))
		{
			return;
		}
		lock (Sync)
		{
			EnsureConfigLoaded();
			Config.CharacterModelPreviewX = Math.Clamp(x, 0f, 1f);
			Config.CharacterModelPreviewY = Math.Clamp(y, 0f, 1f);
			Config.CharacterModelPreviewDocked = docked;
			Config.Save(ConfigPath);
		}
	}

	internal static void ResetCharacterModelPreviewPosition()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			SkinConfig config = Config;
			float? characterModelPreviewX = (Config.CharacterModelPreviewY = null);
			config.CharacterModelPreviewX = characterModelPreviewX;
			Config.CharacterModelPreviewDocked = false;
			Config.Save(ConfigPath);
		}
	}

	private static string GetCharacterSkinBundleRunSavePath(bool multiplayer)
	{
		string s = SaveManager.Instance.GetProfileScopedPath("skin-changer") + (multiplayer ? ":mp" : ":sp");
		string text = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s)));
		return SkinChangerPaths.Storage.RunRecordFile(text + ".json");
	}

	private static void CaptureCharacterSkinBundleRunPresets()
	{
		CharacterSkinBundleRunState characterSkinBundleRunState = _characterSkinBundleRunState;
		if (characterSkinBundleRunState == null || _characterSkinBundleRunSnapshot == null)
		{
			return;
		}
		characterSkinBundleRunState.Cards = characterSkinBundleRunState.Cards.Select(delegate(CardSkinPreset preset)
		{
			SkinCatalog? catalog = Catalog;
			return (catalog == null || !catalog.CardGroups.Any((CardSkinGroup group) => group.Id.Equals(preset.CategoryId, StringComparison.OrdinalIgnoreCase))) ? preset.Clone() : CaptureCurrentCardSkinPreset(preset.CategoryId, Config.ActiveCardSkinPresets.GetValueOrDefault(preset.CategoryId, string.Empty));
		}).ToList();
		characterSkinBundleRunState.Monsters = characterSkinBundleRunState.Monsters.Select((MonsterSkinPreset preset) => (!Config.MonsterSkinCategoryGroups.ContainsKey(preset.CategoryId)) ? preset.Clone() : CaptureCurrentMonsterSkinPreset(preset.CategoryId, Config.ActiveMonsterSkinPresets.GetValueOrDefault(preset.CategoryId, string.Empty))).ToList();
	}

	internal static void SaveCharacterSkinBundleRunPresets()
	{
		lock (Sync)
		{
			if (_characterSkinBundleRunState != null && _characterSkinBundleRunSavePath != null)
			{
				CaptureCharacterSkinBundleRunPresets();
				if (_characterSkinBundleRunState.RandomCharacterOptionId != null)
				{
					_characterSkinBundleRunState.RandomCharacterOptionId = Config.GetSelection(_characterSkinBundleRunState.CharacterGroupId);
				}
				CharacterSkinBundleRunStore.Save(_characterSkinBundleRunSavePath, _characterSkinBundleRunState);
			}
		}
	}

	internal static void BindNewCharacterSkinBundleRun(RunManager manager, RunState state, bool multiplayer, bool shouldSave = true)
	{
		lock (Sync)
		{
			Player val = ((IEnumerable<Player>)state.Players).FirstOrDefault((Func<Player, bool>)((Player player) => player.NetId == manager.NetService.NetId));
			if (val == null)
			{
				return;
			}
			long num = (long)AccessTools.Field(typeof(RunManager), "_startTime").GetValue(manager);
			string text = CharacterSkinBundleRunState.Identity(num, state.Rng.StringSeed, val.NetId, state.Players.Select((Player player) => player.NetId));
			if (!(_characterSkinBundleRunState?.RunIdentity == text))
			{
				string groupId = ((AbstractModel)val.Character).Id.Entry.ToLowerInvariant();
				bool flag = IsRandomCharacterSkinEnabled(groupId);
				if (flag)
				{
					ApplyRandomCharacterSkinForNewRun(groupId);
				}
				CharacterSkinBundleRunState characterSkinBundleRunState = ((_characterSkinBundleRunSnapshot != null) ? _characterSkinBundleRunState : null);
				if (characterSkinBundleRunState == null)
				{
					characterSkinBundleRunState = new CharacterSkinBundleRunState
					{
						CharacterGroupId = ((AbstractModel)val.Character).Id.Entry.ToLowerInvariant()
					};
				}
				characterSkinBundleRunState.RunIdentity = text;
				characterSkinBundleRunState.RandomCharacterOptionId = (flag ? Config.GetSelection(groupId) : null);
				string text2 = (shouldSave ? GetCharacterSkinBundleRunSavePath(multiplayer) : null);
				if (text2 != null)
				{
					CharacterSkinBundleRunStore.Save(text2, characterSkinBundleRunState);
				}
				if (_characterSkinBundleRunSnapshot != null || flag)
				{
					_characterSkinBundleRunState = characterSkinBundleRunState;
					_characterSkinBundleRunSavePath = text2;
					ModLog.Info($"已绑定本局皮肤包“{characterSkinBundleRunState.BundleName}”到对局记录：开始时间={num}，多人={multiplayer}。");
				}
			}
		}
	}

	internal static void LoadCharacterSkinBundleRun(RunState state, SerializableRun save, ulong localId, bool multiplayer)
	{
		lock (Sync)
		{
			Player val = ((IEnumerable<Player>)state.Players).FirstOrDefault((Func<Player, bool>)((Player player) => player.NetId == localId));
			if (val == null)
			{
				return;
			}
			string characterSkinBundleRunSavePath = GetCharacterSkinBundleRunSavePath(multiplayer);
			string identity = CharacterSkinBundleRunState.Identity(save.StartTime, state.Rng.StringSeed, localId, state.Players.Select((Player player) => player.NetId));
			CharacterSkinBundleRunState characterSkinBundleRunState = CharacterSkinBundleRunStore.LoadMatching(characterSkinBundleRunSavePath, identity);
			if (characterSkinBundleRunState == null)
			{
				ModLog.Info("继续对局没有找到匹配的皮肤包记录；不根据选角界面的当前包猜测旧局选择。");
				return;
			}
			if (!characterSkinBundleRunState.CharacterGroupId.Equals(((AbstractModel)val.Character).Id.Entry, StringComparison.OrdinalIgnoreCase))
			{
				ModLog.Warn("皮肤包对局记录的角色与本机玩家不匹配，跳过恢复。");
				return;
			}
			ResumeRandomCharacterSkin(characterSkinBundleRunState);
			if (string.IsNullOrWhiteSpace(characterSkinBundleRunState.BundleName))
			{
				if (characterSkinBundleRunState.RandomCharacterOptionId != null)
				{
					_characterSkinBundleRunState = characterSkinBundleRunState;
					_characterSkinBundleRunSavePath = characterSkinBundleRunSavePath;
				}
			}
			else if (ResumeCharacterSkinBundleForRun(characterSkinBundleRunState))
			{
				_characterSkinBundleRunSavePath = characterSkinBundleRunSavePath;
			}
		}
	}

	internal static bool ResumeCharacterSkinBundleForRun(CharacterSkinBundleRunState state)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null)
			{
				return false;
			}
			if (_characterSkinBundleRunSnapshot != null)
			{
				RestoreCharacterSkinBundleAfterRun();
			}
			SkinConfig original = Config;
			SkinConfig staged = original.CloneForBundleTransaction();
			List<CardSkinPreset> cards = (from p in state.Cards
				where !string.IsNullOrWhiteSpace(p.CategoryId) && catalog.CardGroups.Any((CardSkinGroup g) => g.Id.Equals(p.CategoryId, StringComparison.OrdinalIgnoreCase))
				select p.Clone()).ToList();
			List<MonsterSkinPreset> monsters = (from p in state.Monsters
				where Config.MonsterSkinCategoryGroups.ContainsKey(p.CategoryId)
				select p.Clone()).ToList();
			HashSet<string> cardGroups = cards.Select((CardSkinPreset p) => p.CategoryId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> visualGroups = monsters.SelectMany((MonsterSkinPreset p) => Config.MonsterSkinCategoryGroups[p.CategoryId]).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			original.Save(CharacterSkinBundleRunSnapshotPath);
			Exception ex = StagedConfigurationTransaction.Run(original, staged, delegate(SkinConfig value)
			{
				Config = value;
			}, Prepare, Refresh, delegate
			{
				original.Save(ConfigPath);
			}, Refresh);
			LastError = ex?.Message;
			if (ex != null)
			{
				DeleteCharacterSkinBundleRunSnapshot();
				ModLog.Error("继续对局恢复皮肤包失败，已保留全局预设和原对局记录：" + ex);
				return false;
			}
			_characterSkinBundleRunSnapshot = original;
			_characterSkinBundleRunCardGroups = cardGroups;
			_characterSkinBundleRunVisualGroups = visualGroups;
			_characterSkinBundleRunState = new CharacterSkinBundleRunState
			{
				RunIdentity = state.RunIdentity,
				CharacterGroupId = state.CharacterGroupId,
				BundleName = state.BundleName,
				Cards = state.Cards.Select((CardSkinPreset p) => p.Clone()).ToList(),
				RandomCharacterOptionId = state.RandomCharacterOptionId,
				Monsters = state.Monsters.Select((MonsterSkinPreset p) => p.Clone()).ToList()
			};
			ModLog.Info($"继续对局已恢复皮肤包“{state.BundleName}”：卡牌分类={cards.Count}，怪物地区={monsters.Count}；离开后恢复当前全局预设。");
			return true;
			void Prepare()
			{
				foreach (CardSkinPreset preset in cards)
				{
					ApplyCardPresetSettings(catalog.CardGroups.First((CardSkinGroup g) => g.Id.Equals(preset.CategoryId, StringComparison.OrdinalIgnoreCase)), preset);
				}
				foreach (MonsterSkinPreset item in monsters)
				{
					ApplyMonsterPresetSettings(item);
				}
				foreach (string item2 in visualGroups)
				{
					UpdateVisualProviderPriority(item2, Config.GetSelection(item2));
				}
			}
			void Refresh()
			{
				CardPreviewSelections.Clear();
				foreach (string item3 in cardGroups)
				{
					ClearCardPortraitCache(item3);
				}
				foreach (string item4 in visualGroups)
				{
					ClearRuntimeResourceCache(item4);
				}
				IReadOnlyList<ActionFailure> readOnlyList = FailureIsolatedActionRunner.Run(new global::_003C_003Ez__ReadOnlyArray<(string, Action)>(new(string, Action)[2]
				{
					("visuals", delegate
					{
						if (visualGroups.Count > 0)
						{
							MountOverlay(visualGroups);
						}
					}),
					("cards", delegate
					{
						if (cardGroups.Count > 0)
						{
							MountCardOverlay(cardGroups);
						}
					})
				}));
				if (readOnlyList.Count > 0)
				{
					throw new AggregateException(readOnlyList.Select((ActionFailure f) => f.Exception));
				}
			}
		}
	}

	internal static string GetPresetDisplayName(string key)
	{
		return BundlePresetPolicy.DisplayName(Config, key, GetBundleCharacterName);
	}

	internal static string GetBundlePresetDisplayName(CharacterSkinBundle bundle)
	{
		return GetBundleCharacterName(bundle.CharacterGroupId) + "-" + (string.IsNullOrWhiteSpace(bundle.Name) ? ModLocalization.Get(ModText.NewBundle) : bundle.Name.Trim());
	}

	private static string GetBundleCharacterName(string groupId)
	{
		try
		{
			CharacterModel val = ModelDb.AllCharacters.FirstOrDefault((Func<CharacterModel, bool>)((CharacterModel model) => ((AbstractModel)model).Id.Entry.Equals(groupId, StringComparison.OrdinalIgnoreCase)));
			if (val != null)
			{
				return val.Title.GetFormattedText();
			}
		}
		catch
		{
		}
		return Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? groupId;
	}

	private static IEnumerable<CardSkinGroup> GetBundleCardGroups()
	{
		return Catalog?.CardGroups.Where((CardSkinGroup group) => group.Options.Count > 0) ?? Array.Empty<CardSkinGroup>();
	}

	private static IEnumerable<string> GetBundleMonsterCategoryIds()
	{
		return Config.MonsterSkinCategoryGroups.Keys.Where((string id) => GetMonsterCategoryOptionsInternal(id).Count > 0);
	}

	private static bool CanRenameOrDeletePreset(string key)
	{
		if (!BundlePresetPolicy.IsOwned(key))
		{
			return true;
		}
		LastError = ModLocalization.BundlePresetLocked;
		return false;
	}

	internal static IReadOnlyList<SkinOption> GetCharacterSkinBundleSourceOptions(string groupId)
	{
		return Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.Where((SkinOption option) => !option.IsSessionComposition).ToArray() ?? Array.Empty<SkinOption>();
	}

	public static IReadOnlyList<CharacterSkinBundle> GetCharacterSkinBundles(string groupId)
	{
		lock (Sync)
		{
			return Config.CharacterSkinBundles.Where((CharacterSkinBundle bundle) => bundle.CharacterGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Select(CharacterSkinBundlePolicy.Clone).ToArray();
		}
	}

	internal static string GetCharacterSelectionOptionId(string groupId)
	{
		lock (Sync)
		{
			if (IsRandomCharacterSkinEnabled(groupId))
			{
				return "__random_character_skin__";
			}
			string valueOrDefault = Config.ActiveCharacterSkinBundles.GetValueOrDefault(groupId);
			return (!string.IsNullOrWhiteSpace(valueOrDefault) && FindCharacterSkinBundleIndex(groupId, valueOrDefault) >= 0) ? CharacterSkinBundlePolicy.CreateSelectionOptionId(valueOrDefault) : Config.GetSelection(groupId);
		}
	}

	public static IReadOnlyList<SkinPresetCategory> GetCardPresetCategories()
	{
		lock (Sync)
		{
			return (from @group in GetBundleCardGroups()
				select new SkinPresetCategory(@group.Id, @group.DisplayName, (from preset in GetCardSkinPresets(@group.Id)
					select preset.Name).ToArray())).ToArray();
		}
	}

	public static IReadOnlyList<SkinPresetCategory> GetMonsterPresetCategories()
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		lock (Sync)
		{
			Dictionary<string, string> titles = ModelDb.Acts.ToDictionary<ActModel, string, string>((ActModel act) => "act:" + ((AbstractModel)act).Id.Entry.ToLowerInvariant(), (ActModel act) => act.Title.GetFormattedText(), StringComparer.OrdinalIgnoreCase);
			titles["events"] = new LocString("bestiary", "EVENTS.title").GetFormattedText();
			return (from id in GetBundleMonsterCategoryIds()
				select new SkinPresetCategory(id, titles.GetValueOrDefault(id, id), (from preset in GetMonsterSkinPresets(id)
					select preset.Name).ToArray())).ToArray();
		}
	}

	public static bool CreateCharacterSkinBundle(CharacterSkinBundle draft)
	{
		return SaveCharacterSkinBundle(null, draft);
	}

	public static bool OverwriteCharacterSkinBundle(string currentName, CharacterSkinBundle draft)
	{
		return SaveCharacterSkinBundle(currentName, draft);
	}

	public static bool RenameCharacterSkinBundle(string groupId, string currentName, string newName)
	{
		lock (Sync)
		{
			CharacterSkinBundle characterSkinBundle = GetCharacterSkinBundles(groupId).FirstOrDefault((CharacterSkinBundle candidate) => candidate.Name.Equals(currentName, StringComparison.OrdinalIgnoreCase));
			if (characterSkinBundle == null)
			{
				LastError = ModLocalization.Get(ModText.BundleUnavailable);
				return false;
			}
			characterSkinBundle.Name = newName;
			return SaveCharacterSkinBundle(currentName, characterSkinBundle);
		}
	}

	private static int FindCharacterSkinBundleIndex(string groupId, string name)
	{
		return Config.CharacterSkinBundles.FindIndex((CharacterSkinBundle bundle) => bundle.CharacterGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) && bundle.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	private static bool SaveCharacterSkinBundle(string? currentName, CharacterSkinBundle draft)
	{
		lock (Sync)
		{
			CharacterSkinBundle characterSkinBundle = CharacterSkinBundlePolicy.Normalize(new _003C_003Ez__ReadOnlySingleElementList<CharacterSkinBundle>(draft)).SingleOrDefault();
			if (characterSkinBundle == null || characterSkinBundle.Name.Length > 40 || characterSkinBundle.Name.Any(char.IsControl) || !CharacterSkinBundlePolicy.IsValidCharacterOptionReference(characterSkinBundle.CharacterOptionId))
			{
				LastError = ModLocalization.Get(ModText.BundleInvalidName);
				return false;
			}
			int num = ((currentName == null) ? (-1) : FindCharacterSkinBundleIndex(characterSkinBundle.CharacterGroupId, currentName));
			int num2 = FindCharacterSkinBundleIndex(characterSkinBundle.CharacterGroupId, characterSkinBundle.Name);
			if ((currentName != null && num < 0) || (num2 >= 0 && num2 != num))
			{
				LastError = ModLocalization.Get((num2 >= 0) ? ModText.BundleDuplicateName : ModText.BundleUnavailable);
				return false;
			}
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			if (num >= 0)
			{
				characterSkinBundle.Id = skinConfig.CharacterSkinBundles[num].Id;
			}
			if (num < 0)
			{
				skinConfig.CharacterSkinBundles.Add(characterSkinBundle);
			}
			else
			{
				skinConfig.CharacterSkinBundles[num] = characterSkinBundle;
				if (string.Equals(skinConfig.ActiveCharacterSkinBundles.GetValueOrDefault(characterSkinBundle.CharacterGroupId), currentName, StringComparison.OrdinalIgnoreCase))
				{
					skinConfig.ActiveCharacterSkinBundles[characterSkinBundle.CharacterGroupId] = characterSkinBundle.Name;
				}
			}
			BundlePresetPolicy.Synchronize(skinConfig, Catalog?.CardGroups.Select((CardSkinGroup group) => group.Id) ?? Array.Empty<string>(), skinConfig.MonsterSkinCategoryGroups.Keys);
			return CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	public static bool DeleteCharacterSkinBundle(string groupId, string name)
	{
		lock (Sync)
		{
			int num = FindCharacterSkinBundleIndex(groupId, name);
			if (num < 0)
			{
				LastError = ModLocalization.Get(ModText.BundleUnavailable);
				return false;
			}
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			BundlePresetPolicy.RemoveOwnedPresets(skinConfig, skinConfig.CharacterSkinBundles[num]);
			skinConfig.CharacterSkinBundles.RemoveAt(num);
			if (string.Equals(skinConfig.ActiveCharacterSkinBundles.GetValueOrDefault(groupId), name, StringComparison.OrdinalIgnoreCase))
			{
				skinConfig.ActiveCharacterSkinBundles.Remove(groupId);
			}
			return CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	public static bool SelectCharacterSkinBundle(string groupId, string name)
	{
		lock (Sync)
		{
			int num = FindCharacterSkinBundleIndex(groupId, name);
			if (num < 0)
			{
				LastError = ModLocalization.Get(ModText.BundleUnavailable);
				return false;
			}
			if (string.Equals(Config.ActiveCharacterSkinBundles.GetValueOrDefault(groupId), Config.CharacterSkinBundles[num].Name, StringComparison.OrdinalIgnoreCase))
			{
				LastError = null;
				return true;
			}
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			skinConfig.ActiveCharacterSkinBundles[groupId] = Config.CharacterSkinBundles[num].Name;
			return CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	public static string? GetCharacterSkinBundleCharacterOption(string groupId, string name)
	{
		lock (Sync)
		{
			int num = FindCharacterSkinBundleIndex(groupId, name);
			SkinCatalog catalog = Catalog;
			if (num < 0 || catalog == null)
			{
				LastError = ModLocalization.Get(ModText.BundleUnavailable);
				return null;
			}
			string optionId = catalog.ResolveStoredVisualSelectionId(groupId, Config.CharacterSkinBundles[num].CharacterOptionId);
			SkinGroup skinGroup = catalog.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (!optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && (skinGroup == null || !skinGroup.Options.Any((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
			{
				LastError = ModLocalization.Get(ModText.BundleMissingSkin);
				return null;
			}
			LastError = null;
			return optionId;
		}
	}

	public static bool ClearSelectedCharacterSkinBundle(string groupId)
	{
		lock (Sync)
		{
			if (!Config.ActiveCharacterSkinBundles.ContainsKey(groupId))
			{
				LastError = null;
				return true;
			}
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			skinConfig.ActiveCharacterSkinBundles.Remove(groupId);
			return CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	public static bool ApplySelectedCharacterSkinBundleForRun(string groupId, out IReadOnlyList<string> warnings)
	{
		lock (Sync)
		{
			warnings = Array.Empty<string>();
			string valueOrDefault = Config.ActiveCharacterSkinBundles.GetValueOrDefault(groupId);
			if (string.IsNullOrWhiteSpace(valueOrDefault))
			{
				LastError = null;
				return true;
			}
			if (!CharacterSkinBundlePolicy.TryEnterApplication(groupId, valueOrDefault, new HashSet<string>(StringComparer.OrdinalIgnoreCase), out HashSet<string> nextApplications))
			{
				LastError = ModLocalization.Get(ModText.BundleScopeConflict);
				return false;
			}
			return BeginCharacterSkinBundleRunSession(groupId, valueOrDefault, nextApplications, out warnings);
		}
	}

	private static bool BeginCharacterSkinBundleRunSession(string groupId, string name, IReadOnlySet<string> activeApplications, out IReadOnlyList<string> warnings)
	{
		SkinCatalog catalog;
		int index;
		CharacterSkinBundle bundle;
		SkinConfig original;
		HashSet<string> visualGroups;
		HashSet<string> cardGroups;
		HashSet<string> monsterCategories;
		lock (Sync)
		{
			List<string> list = new List<string>();
			warnings = list;
			catalog = Catalog;
			index = FindCharacterSkinBundleIndex(groupId, name);
			if (catalog == null || index < 0)
			{
				LastError = ModLocalization.Get(ModText.BundleUnavailable);
				return false;
			}
			bundle = CharacterSkinBundlePolicy.Clone(Config.CharacterSkinBundles[index]);
			if (!activeApplications.Contains(groupId.Trim() + "\n" + bundle.Name.Trim()) || !CharacterSkinBundlePolicy.IsValidCharacterOptionReference(bundle.CharacterOptionId))
			{
				LastError = ModLocalization.Get(ModText.BundleScopeConflict);
				return false;
			}
			if (_characterSkinBundleRunSnapshot != null)
			{
				RestoreCharacterSkinBundleAfterRun();
			}
			original = Config;
			SkinConfig staged = original.CloneForBundleTransaction();
			visualGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			cardGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			monsterCategories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			original.Save(CharacterSkinBundleRunSnapshotPath);
			Exception ex = StagedConfigurationTransaction.Run(original, staged, delegate(SkinConfig value)
			{
				Config = value;
			}, Prepare, delegate
			{
				RefreshVisuals();
				RefreshCards();
			}, delegate
			{
				original.Save(ConfigPath);
			}, Restore);
			LastError = ex?.Message;
			if (ex != null)
			{
				DeleteCharacterSkinBundleRunSnapshot();
				ModLog.Error("开始对局前应用皮肤包预设失败，已恢复原配置：" + ex);
				return false;
			}
			_characterSkinBundleRunSnapshot = original;
			_characterSkinBundleRunVisualGroups = visualGroups;
			_characterSkinBundleRunCardGroups = cardGroups;
			_characterSkinBundleRunState = new CharacterSkinBundleRunState
			{
				CharacterGroupId = groupId,
				BundleName = bundle.Name,
				Cards = cardGroups.Select((string id) => CaptureCurrentCardSkinPreset(id, Config.ActiveCardSkinPresets.GetValueOrDefault(id, string.Empty))).ToList(),
				Monsters = monsterCategories.Select((string id) => CaptureCurrentMonsterSkinPreset(id, Config.ActiveMonsterSkinPresets.GetValueOrDefault(id, string.Empty))).ToList()
			};
			_characterSkinBundleRunSavePath = null;
			ModLog.Info($"已为本局临时应用皮肤包“{bundle.Name}”：角色皮肤保持当前热切换结果，卡牌分类={cardGroups.Count}，怪物分组={visualGroups.Count}；离开本局时恢复原预设。");
			return true;
		}
		void Prepare()
		{
			BundlePresetPolicy.Synchronize(Config, catalog.CardGroups.Select((CardSkinGroup group) => group.Id), Config.MonsterSkinCategoryGroups.Keys);
			bundle = CharacterSkinBundlePolicy.Clone(Config.CharacterSkinBundles[index]);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (CardSkinPreset preset in ResolveBundleCardPresets(bundle))
			{
				CardSkinGroup cardSkinGroup = catalog.CardGroups.First((CardSkinGroup candidate) => candidate.Id.Equals(preset.CategoryId, StringComparison.OrdinalIgnoreCase));
				ApplyCardPresetSettings(cardSkinGroup, preset);
				cardGroups.Add(cardSkinGroup.Id);
			}
			foreach (MonsterSkinPreset item in ResolveBundleMonsterPresets(bundle))
			{
				ApplyMonsterPresetSettings(item);
				monsterCategories.Add(item.CategoryId);
				hashSet.UnionWith(Config.MonsterSkinCategoryGroups[item.CategoryId]);
				visualGroups.UnionWith(ApplyMonsterCategoryPriorityToSelections(item.CategoryId));
			}
			IEnumerable<string> protectedGroups = Config.MonsterSkinCategoryGroups.Values.SelectMany((List<string> ids) => ids).Concat(ModelDb.AllCharacters.Select((CharacterModel character) => ((AbstractModel)character).Id.Entry.ToLowerInvariant()));
			if (CharacterSkinBundlePolicy.ChangesOutsideRequestedGroups(original.Selections, Config.Selections, protectedGroups, hashSet) || !Config.GetSelection(groupId).Equals(original.GetSelection(groupId), StringComparison.OrdinalIgnoreCase))
			{
				throw new InvalidOperationException(ModLocalization.Get(ModText.BundleScopeConflict));
			}
			visualGroups.UnionWith(from id in Config.Selections.Keys.Union<string>(original.Selections.Keys, StringComparer.OrdinalIgnoreCase)
				where !id.StartsWith("cards:", StringComparison.OrdinalIgnoreCase) && !string.Equals(Config.Selections.GetValueOrDefault(id), original.Selections.GetValueOrDefault(id), StringComparison.OrdinalIgnoreCase)
				select id);
			foreach (string item2 in visualGroups)
			{
				UpdateVisualProviderPriority(item2, Config.GetSelection(item2));
			}
			Config.ActiveCharacterSkinBundles[groupId] = bundle.Name;
		}
		void RefreshCards()
		{
			CardPreviewSelections.Clear();
			foreach (string item3 in cardGroups)
			{
				ClearCardPortraitCache(item3);
			}
			if (cardGroups.Count > 0)
			{
				MountCardOverlay(cardGroups);
			}
		}
		void RefreshVisuals()
		{
			CharacterPreviewSelections.Clear();
			foreach (string item4 in visualGroups)
			{
				ClearRuntimeResourceCache(item4);
			}
			if (visualGroups.Count > 0)
			{
				MountOverlay(visualGroups);
			}
		}
		void Restore()
		{
			IReadOnlyList<ActionFailure> readOnlyList = FailureIsolatedActionRunner.Run(new global::_003C_003Ez__ReadOnlyArray<(string, Action)>(new(string, Action)[2]
			{
				("visuals", RefreshVisuals),
				("cards", RefreshCards)
			}));
			if (readOnlyList.Count > 0)
			{
				throw new AggregateException(readOnlyList.Select((ActionFailure failure) => failure.Exception));
			}
		}
	}

	public static void RestoreCharacterSkinBundleAfterRun()
	{
		lock (Sync)
		{
			SkinConfig characterSkinBundleRunSnapshot = _characterSkinBundleRunSnapshot;
			if (characterSkinBundleRunSnapshot == null)
			{
				SaveCharacterSkinBundleRunPresets();
				_characterSkinBundleRunState = null;
				_characterSkinBundleRunSavePath = null;
				return;
			}
			HashSet<string> visualGroups = _characterSkinBundleRunVisualGroups;
			HashSet<string> cardGroups = _characterSkinBundleRunCardGroups;
			try
			{
				SaveCharacterSkinBundleRunPresets();
			}
			catch (Exception ex)
			{
				ModLog.Error("保存离开对局前的皮肤包记录失败：" + ex);
			}
			characterSkinBundleRunSnapshot.CopyRandomCharacterSkinExclusionsFrom(Config);
			Config = characterSkinBundleRunSnapshot;
			CharacterPreviewSelections.Clear();
			CardPreviewSelections.Clear();
			foreach (string item in visualGroups)
			{
				ClearRuntimeResourceCache(item);
			}
			foreach (string item2 in cardGroups)
			{
				ClearCardPortraitCache(item2);
			}
			IReadOnlyList<ActionFailure> readOnlyList = FailureIsolatedActionRunner.Run(new global::_003C_003Ez__ReadOnlyArray<(string, Action)>(new(string, Action)[2]
			{
				("visuals", delegate
				{
					if (visualGroups.Count > 0)
					{
						MountOverlay(visualGroups);
					}
				}),
				("cards", delegate
				{
					if (cardGroups.Count > 0)
					{
						MountCardOverlay(cardGroups);
					}
				})
			}));
			Config.Save(ConfigPath);
			DeleteCharacterSkinBundleRunSnapshot();
			_characterSkinBundleRunSnapshot = null;
			_characterSkinBundleRunState = null;
			_characterSkinBundleRunSavePath = null;
			_characterSkinBundleRunVisualGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			_characterSkinBundleRunCardGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (readOnlyList.Count == 0)
			{
				ModLog.Info("已在离开本局时恢复皮肤包应用前的卡牌与怪物预设。");
				return;
			}
			ModLog.Error("恢复皮肤包应用前预设时有资源刷新失败：" + new AggregateException(readOnlyList.Select((ActionFailure failure) => failure.Exception)));
		}
	}

	private static void DeleteCharacterSkinBundleRunSnapshot()
	{
		string[] array = new string[2]
		{
			CharacterSkinBundleRunSnapshotPath,
			CharacterSkinBundleRunSnapshotPath + ".bak"
		};
		foreach (string path in array)
		{
			try
			{
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn("清理皮肤包本局恢复点失败，将在下次启动重试：" + ex.Message);
			}
		}
	}

	internal static SkinConfig RecoverInterruptedCharacterSkinBundleSession(SkinConfig current)
	{
		if (!File.Exists(CharacterSkinBundleRunSnapshotPath))
		{
			return current;
		}
		try
		{
			SkinConfig skinConfig = SkinConfig.Load(CharacterSkinBundleRunSnapshotPath);
			skinConfig.CopyRandomCharacterSkinExclusionsFrom(current);
			skinConfig.Save(ConfigPath);
			DeleteCharacterSkinBundleRunSnapshot();
			ModLog.Info("检测到上次游戏在皮肤包生效期间退出，已恢复进入该局前的预设。");
			return skinConfig;
		}
		catch (Exception ex)
		{
			ModLog.Warn("恢复上次皮肤包本局预设失败，将保留现有配置：" + ex.Message);
			return current;
		}
	}

	private static bool CommitBundleConfiguration(SkinConfig next, Action prepare, Action refresh, Action restore)
	{
		Exception ex = StagedConfigurationTransaction.Run(Config, next, delegate(SkinConfig value)
		{
			Config = value;
		}, prepare, refresh, delegate(SkinConfig value)
		{
			value.Save(ConfigPath);
		}, restore);
		LastError = ex?.Message;
		if (ex != null)
		{
			ModLog.Error("皮肤包操作失败，已恢复原配置：" + ex);
		}
		return ex == null;
	}

	private static void ApplyCardPresetSettings(CardSkinGroup group, CardSkinPreset preset)
	{
		List<CardSkinPriorityEntry> value;
		if (preset.AllOriginal)
		{
			Config.CardSkinPriorities[group.Id] = (from entry in GetCardPriorityEntriesInternal(@group)
				select entry with
				{
					Enabled = false
				}).ToList();
		}
		else if (preset.CardSkinPriorities.TryGetValue(group.Id, out value))
		{
			Config.CardSkinPriorities[group.Id] = value.ToList();
		}
		else
		{
			Config.CardSkinPriorities.Remove(group.Id);
		}
		ReplaceCardSelectionsForGroup(group.Id, BundlePresetPolicy.CardSelections(preset, group.Id));
		Config.CardPriorityDefaultsVersion = 1;
		SetActiveCardSkinPreset(group.Id, preset.Name);
		GetCardPriorityEntriesInternal(group);
	}

	private static List<CharacterSkinBundle> UpdateBundlePresetReferences(bool monsterPreset, string categoryId, string currentName, string? newName)
	{
		List<CharacterSkinBundle> characterSkinBundles = Config.CharacterSkinBundles;
		Config.CharacterSkinBundles = characterSkinBundles.Select(CharacterSkinBundlePolicy.Clone).ToList();
		foreach (CharacterSkinBundle characterSkinBundle in Config.CharacterSkinBundles)
		{
			if (newName == null)
			{
				CharacterSkinBundlePolicy.RemovePresetReference(characterSkinBundle, monsterPreset, categoryId, currentName);
			}
			else
			{
				CharacterSkinBundlePolicy.RenamePresetReference(characterSkinBundle, monsterPreset, categoryId, currentName, newName);
			}
		}
		return characterSkinBundles;
	}

	public static (float X, float Y)? GetCharacterSkinBundlePosition()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			float? characterSkinBundleX = Config.CharacterSkinBundleX;
			(float, float)? result;
			if (characterSkinBundleX.HasValue)
			{
				float valueOrDefault = characterSkinBundleX.GetValueOrDefault();
				characterSkinBundleX = Config.CharacterSkinBundleY;
				if (characterSkinBundleX.HasValue)
				{
					float valueOrDefault2 = characterSkinBundleX.GetValueOrDefault();
					if (float.IsFinite(valueOrDefault) && float.IsFinite(valueOrDefault2))
					{
						result = (Math.Clamp(valueOrDefault, 0f, 1f), Math.Clamp(valueOrDefault2, 0f, 1f));
						goto IL_0095;
					}
				}
			}
			result = null;
			goto IL_0095;
			IL_0095:
			return result;
		}
	}

	public static void SetCharacterSkinBundlePosition(float x, float y)
	{
		SaveCharacterSkinBundlePosition(x, y);
	}

	public static void ResetCharacterSkinBundlePosition()
	{
		SaveCharacterSkinBundlePosition(null, null);
	}

	private static void SaveCharacterSkinBundlePosition(float? x, float? y)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			float? characterSkinBundleX;
			if (x.HasValue)
			{
				float valueOrDefault = x.GetValueOrDefault();
				if (float.IsFinite(valueOrDefault))
				{
					characterSkinBundleX = Math.Clamp(valueOrDefault, 0f, 1f);
					goto IL_005a;
				}
			}
			characterSkinBundleX = null;
			goto IL_005a;
			IL_005a:
			skinConfig.CharacterSkinBundleX = characterSkinBundleX;
			float? characterSkinBundleY;
			if (y.HasValue)
			{
				float valueOrDefault2 = y.GetValueOrDefault();
				if (float.IsFinite(valueOrDefault2))
				{
					characterSkinBundleY = Math.Clamp(valueOrDefault2, 0f, 1f);
					goto IL_009a;
				}
			}
			characterSkinBundleY = null;
			goto IL_009a;
			IL_009a:
			skinConfig.CharacterSkinBundleY = characterSkinBundleY;
			CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	private static string GetVisualSelection(string groupId)
	{
		return MultiplayerSkinSync.GetScopedSelection(groupId) ?? CharacterPreviewSelections.GetValueOrDefault(groupId) ?? Config.GetSelection(groupId);
	}

	internal static string GetDisplayedSelection(string groupId)
	{
		return GetVisualSelection(groupId);
	}

	private static IReadOnlyDictionary<string, string> GetVisualSelections()
	{
		return VisualSelectionOverlayPolicy.Merge(Config.Selections, CharacterPreviewSelections, MultiplayerSkinSync.GetScopedSelections());
	}

	internal static bool TryGetSelectedFrameworkContract(string groupId, out FrameworkCharacterSkinContract contract)
	{
		lock (Sync)
		{
			contract = null;
			int num;
			if (Catalog != null)
			{
				num = (Catalog.TryGetSelectedFrameworkContract(groupId, GetVisualSelection(groupId), out contract) ? 1 : 0);
				if (num != 0)
				{
					contract = FrameworkRegistryCooperation.Filter(contract);
				}
			}
			else
			{
				num = 0;
			}
			return (byte)num != 0;
		}
	}

	internal static IReadOnlyList<FrameworkCharacterSkinContract> GetSelectedFrameworkContracts()
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return Array.Empty<FrameworkCharacterSkinContract>();
			}
			FrameworkCharacterSkinContract contract2;
			return (from @group in Catalog.Groups
				select (!Catalog.TryGetSelectedFrameworkContract(@group.Id, GetVisualSelection(@group.Id), out contract2)) ? null : contract2 into contract
				where contract != null
				select contract).Cast<FrameworkCharacterSkinContract>().Select(FrameworkRegistryCooperation.Filter).ToArray();
		}
	}

	public static bool TryBuildSessionCharacterSelection(string groupId, string optionId, out IReadOnlyDictionary<string, string> selectionOverrides)
	{
		lock (Sync)
		{
			selectionOverrides = null;
			SkinCatalog catalog = Catalog;
			SkinGroup skinGroup = catalog?.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (catalog == null || skinGroup == null || !catalog.IsCharacterAppearanceGroup(skinGroup.Id) || (!optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && skinGroup.Options.All((SkinOption option) => !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
			{
				return false;
			}
			Dictionary<string, string> selections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
			selectionOverrides = catalog.BuildVisualSelectionTransaction(skinGroup.Id, optionId, selections);
			return true;
		}
	}

	public static bool TryBuildSessionCharacterComposition(string groupId, IReadOnlyList<string> sourceOptionIds, out IReadOnlyDictionary<string, string> selectionOverrides, out string resolvedOptionId)
	{
		lock (Sync)
		{
			selectionOverrides = null;
			resolvedOptionId = "__base__";
			SkinCatalog catalog = Catalog;
			if (catalog == null || !catalog.IsCharacterAppearanceGroup(groupId))
			{
				return false;
			}
			if (!catalog.TryCreateSessionCharacterComposition(groupId, sourceOptionIds, out resolvedOptionId))
			{
				resolvedOptionId = "__base__";
			}
			return TryBuildSessionCharacterSelection(groupId, resolvedOptionId, out selectionOverrides);
		}
	}

	internal static void ClearSessionCharacterCompositions()
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return;
			}
			foreach (string item in Catalog.ClearSessionCharacterCompositions())
			{
				ClearRuntimeResourceCache(item);
			}
		}
	}

	public static IReadOnlyList<SkinOption> GetCharacterSkinOptions(string groupId)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			SkinGroup skinGroup = catalog?.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (catalog == null || skinGroup == null || !catalog.IsCharacterAppearanceGroup(groupId))
			{
				return Array.Empty<SkinOption>();
			}
			HashSet<string> visibleRawIds = CharacterSkinCompositionPolicy.VisibleRawOptionIds(groupId, from option in catalog.GetRawCharacterOptions(groupId)
				select option.Id, Config.CharacterSkinCompositions).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hiddenBundleSources = BundlePresetPolicy.HiddenSources(Config, groupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			return skinGroup.Options.Where((SkinOption option) => !hiddenBundleSources.Contains(option.Id) && ((!option.IsComposition) ? visibleRawIds.Contains(option.Id) : (!option.IsSessionComposition))).ToArray();
		}
	}

	public static IReadOnlyList<SkinOption> GetRawCharacterSkinOptions(string groupId)
	{
		lock (Sync)
		{
			return Catalog?.GetRawCharacterOptions(groupId) ?? Array.Empty<SkinOption>();
		}
	}

	public static IReadOnlyList<CharacterSkinComposition> GetCharacterSkinCompositions(string groupId)
	{
		lock (Sync)
		{
			return Config.CharacterSkinCompositions.Where((CharacterSkinComposition composition) => composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Select(CloneCharacterSkinComposition).ToArray();
		}
	}

	public static IReadOnlyList<string> GetCharacterSelectionSourceIds(string groupId, string optionId)
	{
		lock (Sync)
		{
			return Catalog?.GetMultiplayerSourceIds(groupId, optionId) ?? Array.Empty<string>();
		}
	}

	public static IReadOnlyList<string> GetAvailableCharacterSelectionSourceIds(string groupId, IReadOnlyList<string> sourceOptionIds)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null || !catalog.IsCharacterAppearanceGroup(groupId))
			{
				return Array.Empty<string>();
			}
			return catalog.ResolveMultiplayerSourceIds(groupId, sourceOptionIds);
		}
	}

	public static bool SaveCharacterSkinComposition(string groupId, string? compositionId, string? name, IReadOnlyList<string> sourceOptionIds, bool hideSources, out string savedId)
	{
		lock (Sync)
		{
			savedId = string.Empty;
			SkinCatalog catalog = Catalog;
			IReadOnlyList<SkinOption> readOnlyList = catalog?.GetRawCharacterOptions(groupId) ?? Array.Empty<SkinOption>();
			if (catalog == null || readOnlyList.Count == 0 || !catalog.IsCharacterAppearanceGroup(groupId))
			{
				LastError = "找不到角色皮肤分组：" + groupId;
				return false;
			}
			CharacterSkinComposition characterSkinComposition = (string.IsNullOrWhiteSpace(compositionId) ? null : Config.CharacterSkinCompositions.FirstOrDefault((CharacterSkinComposition composition) => composition.Id.Equals(compositionId, StringComparison.OrdinalIgnoreCase) && composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase)));
			if (!string.IsNullOrWhiteSpace(compositionId) && characterSkinComposition == null)
			{
				LastError = "找不到合并皮肤：" + compositionId;
				return false;
			}
			HashSet<string> knownRawIds = readOnlyList.Select((SkinOption option) => option.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> retainedMissingIds = characterSkinComposition?.SourceOptionIds.Where((string optionId) => !knownRawIds.Contains(optionId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<string> list = (from optionId in sourceOptionIds
				where !string.IsNullOrWhiteSpace(optionId)
				select optionId.Trim()).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
			if (list.Count == 0 || list.Any((string optionId) => !knownRawIds.Contains(optionId) && !retainedMissingIds.Contains(optionId)))
			{
				LastError = "合并皮肤至少需要一个当前角色的原始皮肤来源。";
				return false;
			}
			List<CharacterSkinComposition> characterSkinCompositions = Config.CharacterSkinCompositions.Select(CloneCharacterSkinComposition).ToList();
			Dictionary<string, string> selections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
			List<string> visualProviderPriority = Config.VisualProviderPriority.ToList();
			string id = characterSkinComposition?.Id ?? CharacterSkinCompositionPolicy.CreateId();
			string name2 = CharacterSkinCompositionPolicy.UniqueName(name, from composition in Config.CharacterSkinCompositions
				where composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) && !composition.Id.Equals(id, StringComparison.OrdinalIgnoreCase)
				select composition.Name, ModLocalization.Get(ModText.CombinedSkinDefaultName));
			CharacterSkinComposition characterSkinComposition2 = new CharacterSkinComposition
			{
				Id = id,
				GroupId = groupId,
				Name = name2,
				SourceOptionIds = list,
				HideSources = hideSources
			};
			bool flag = characterSkinComposition != null && Config.GetSelection(groupId).Equals(characterSkinComposition.Id, StringComparison.OrdinalIgnoreCase);
			bool flag2 = CharacterSkinCompositionPolicy.ShouldApplyAfterSave(characterSkinComposition == null, flag);
			IReadOnlyDictionary<string, string> readOnlyDictionary2;
			if (!flag)
			{
				IReadOnlyDictionary<string, string> readOnlyDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				readOnlyDictionary2 = readOnlyDictionary;
			}
			else
			{
				readOnlyDictionary2 = catalog.BuildVisualSelectionTransaction(groupId, "__base__", Config.Selections);
			}
			IReadOnlyDictionary<string, string> readOnlyDictionary3 = readOnlyDictionary2;
			HashSet<string> hashSet = readOnlyDictionary3.Keys.Append(groupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				if (characterSkinComposition == null)
				{
					Config.CharacterSkinCompositions.Add(characterSkinComposition2);
				}
				else
				{
					int index = Config.CharacterSkinCompositions.IndexOf(characterSkinComposition);
					Config.CharacterSkinCompositions[index] = characterSkinComposition2;
				}
				catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
				if (catalog.Groups.First((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Options.All((SkinOption option) => !option.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
				{
					throw new InvalidOperationException("合并皮肤没有当前可用的来源，无法应用。");
				}
				if (!flag2)
				{
					Config.Save(ConfigPath);
					savedId = id;
					LastError = null;
					return true;
				}
				Dictionary<string, string> dictionary = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, string> item in readOnlyDictionary3)
				{
					dictionary[item.Key] = item.Value;
				}
				IReadOnlyDictionary<string, string> readOnlyDictionary4 = catalog.BuildVisualSelectionTransaction(groupId, id, dictionary);
				IReadOnlyDictionary<string, string> readOnlyDictionary5 = CharacterSkinCompositionPolicy.MergeSelectionUpdates(new global::_003C_003Ez__ReadOnlyArray<IReadOnlyDictionary<string, string>>(new IReadOnlyDictionary<string, string>[2] { readOnlyDictionary3, readOnlyDictionary4 }));
				hashSet.UnionWith(readOnlyDictionary5.Keys);
				foreach (KeyValuePair<string, string> item2 in readOnlyDictionary5)
				{
					Config.Selections[item2.Key] = item2.Value;
					ClearRuntimeResourceCache(item2.Key);
				}
				UpdateVisualProviderPriority(groupId, id);
				MountOverlay(hashSet);
				Config.Save(ConfigPath);
				savedId = id;
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				Config.CharacterSkinCompositions = characterSkinCompositions;
				Config.Selections = selections;
				Config.VisualProviderPriority = visualProviderPriority;
				catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
				foreach (string item3 in hashSet)
				{
					ClearRuntimeResourceCache(item3);
				}
				TryRestoreOverlay(hashSet, cardOverlay: false);
				LastError = ex.GetBaseException().Message;
				ModLog.Error($"保存 {groupId} 的合并皮肤失败：{ex}");
				return false;
			}
		}
	}

	public static bool DeleteCharacterSkinComposition(string groupId, string compositionId)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			CharacterSkinComposition characterSkinComposition = Config.CharacterSkinCompositions.FirstOrDefault((CharacterSkinComposition composition) => composition.Id.Equals(compositionId, StringComparison.OrdinalIgnoreCase) && composition.GroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (catalog == null || characterSkinComposition == null)
			{
				LastError = "找不到合并皮肤：" + compositionId;
				return false;
			}
			List<CharacterSkinComposition> characterSkinCompositions = Config.CharacterSkinCompositions.Select(CloneCharacterSkinComposition).ToList();
			Dictionary<string, string> selections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
			List<string> visualProviderPriority = Config.VisualProviderPriority.ToList();
			IReadOnlyDictionary<string, string> readOnlyDictionary2;
			if (!Config.GetSelection(groupId).Equals(compositionId, StringComparison.OrdinalIgnoreCase))
			{
				IReadOnlyDictionary<string, string> readOnlyDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				readOnlyDictionary2 = readOnlyDictionary;
			}
			else
			{
				readOnlyDictionary2 = catalog.BuildVisualSelectionTransaction(groupId, "__base__", Config.Selections);
			}
			IReadOnlyDictionary<string, string> readOnlyDictionary3 = readOnlyDictionary2;
			HashSet<string> hashSet = readOnlyDictionary3.Keys.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			try
			{
				foreach (KeyValuePair<string, string> item in readOnlyDictionary3)
				{
					Config.Selections[item.Key] = item.Value;
					ClearRuntimeResourceCache(item.Key);
				}
				Config.CharacterSkinCompositions.Remove(characterSkinComposition);
				catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
				SanitizeVisualProviderPriority();
				if (hashSet.Count > 0)
				{
					MountOverlay(hashSet);
				}
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				Config.CharacterSkinCompositions = characterSkinCompositions;
				Config.Selections = selections;
				Config.VisualProviderPriority = visualProviderPriority;
				catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
				foreach (string item2 in hashSet)
				{
					ClearRuntimeResourceCache(item2);
				}
				TryRestoreOverlay(hashSet, cardOverlay: false);
				LastError = ex.GetBaseException().Message;
				ModLog.Error($"删除 {groupId}/{compositionId} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool ApplyCharacterPreviewSelection(string groupId, string optionId)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			SkinGroup skinGroup = catalog?.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (catalog == null || skinGroup == null || !catalog.IsCharacterAppearanceGroup(skinGroup.Id) || (!optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && skinGroup.Options.All((SkinOption option) => !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
			{
				LastError = "未知的角色皮肤预览：" + groupId + "/" + optionId;
				return false;
			}
			IReadOnlyDictionary<string, string> readOnlyDictionary = catalog.BuildVisualSelectionTransaction(skinGroup.Id, optionId, Config.Selections);
			Dictionary<string, string> dictionary = new Dictionary<string, string>(CharacterPreviewSelections, StringComparer.OrdinalIgnoreCase);
			IReadOnlySet<string> groups = VisualSelectionOverlayPolicy.AffectedGroups(dictionary.Keys, readOnlyDictionary.Keys);
			try
			{
				CharacterPreviewSelections.Clear();
				foreach (KeyValuePair<string, string> item in readOnlyDictionary)
				{
					CharacterPreviewSelections[item.Key] = item.Value;
				}
				MountOverlay(groups);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				CharacterPreviewSelections.Clear();
				foreach (KeyValuePair<string, string> item2 in dictionary)
				{
					CharacterPreviewSelections[item2.Key] = item2.Value;
				}
				TryRestoreOverlay(groups, cardOverlay: false);
				LastError = ex.Message;
				ModLog.Error($"预览 {groupId}/{optionId} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool ClearCharacterPreviewSelection(bool restoreOverlay)
	{
		lock (Sync)
		{
			if (CharacterPreviewSelections.Count == 0)
			{
				return false;
			}
			HashSet<string> groups = CharacterPreviewSelections.Keys.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			CharacterPreviewSelections.Clear();
			if (restoreOverlay)
			{
				TryRestoreOverlay(groups, cardOverlay: false);
			}
			return true;
		}
	}

	internal static bool TryRegisterOnlineSessionProvider(string optionId, string optionName, string pckPath, string groupId, IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings, out string error)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				error = "皮肤目录尚未初始化。";
				return false;
			}
			return Catalog.TryAddSessionVisualProvider(optionId, optionName, pckPath, groupId, resourceBindings, out error);
		}
	}

	internal static bool TryReadBaseGameResource(string resourcePath, out byte[] bytes)
	{
		lock (Sync)
		{
			if (Catalog == null || !Catalog.IsBaseGameResource(resourcePath))
			{
				bytes = Array.Empty<byte>();
				return false;
			}
			return Catalog.TryReadBaseGameResource(resourcePath, out bytes);
		}
	}

	internal static void RemoveOnlineSessionProvider(string optionId)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return;
			}
			HashSet<string> hashSet = (from @group in Catalog.Groups
				where @group.Options.Any((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))
				select @group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (hashSet.Count > 0)
			{
				MountOverlay(hashSet);
			}
			Catalog.RemoveSessionVisualProvider(optionId);
			string cacheMarker = "\n" + optionId + "\n";
			string[] array = RuntimeResourceCache.Keys.Where((string key) => key.Contains(cacheMarker, StringComparison.OrdinalIgnoreCase)).ToArray();
			foreach (string key2 in array)
			{
				RuntimeResourceCache.Remove(key2);
			}
			array = PreparedRuntimeOverlays.Keys.Where((string key) => key.Contains(cacheMarker, StringComparison.OrdinalIgnoreCase)).ToArray();
			foreach (string key3 in array)
			{
				RuntimeResourceBundles.Remove(key3);
				string overlayPath = PreparedRuntimeOverlays[key3].OverlayPath;
				PreparedRuntimeOverlays.Remove(key3);
				if (overlayPath != null)
				{
					try
					{
						File.Delete(overlayPath);
					}
					catch (Exception ex)
					{
						ModLog.Info("联机皮肤运行缓存仍被 Godot 占用，将在下次启动时清理：" + ex.GetBaseException().Message);
					}
				}
			}
		}
	}

	internal static bool IsBaseGameResource(string resourcePath)
	{
		lock (Sync)
		{
			return Catalog?.IsBaseGameResource(resourcePath) ?? false;
		}
	}

	public static void RefreshSessionRuntimeProviders()
	{
		lock (Sync)
		{
			if (Catalog != null)
			{
				MountOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase));
			}
		}
	}

	public static void SuppressLoadOrderWarning()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			Config.SuppressLoadOrderWarning = true;
			Config.Save(ConfigPath);
			ModLog.Info("已保存旧版“不再提示”设置；当前版本会自动调整加载顺序，不再用该设置跳过提示。");
		}
	}

	public static bool ShouldReceiveMultiplayerSkinChanges()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			return Config.LoadOtherPlayersCustomSkins;
		}
	}

	public static bool ShouldSendMultiplayerSkinChanges()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			return Config.MultiplayerSkinSyncEnabled;
		}
	}

	public static void SetSendMultiplayerSkinChanges(bool enabled)
	{
		if (!enabled && ShouldSendMultiplayerSkinChanges())
		{
			MultiplayerSkinSync.BroadcastLocalFallbackBeforeDisablingSend();
		}
		bool flag;
		lock (Sync)
		{
			EnsureConfigLoaded();
			flag = Config.MultiplayerSkinSyncEnabled != enabled;
			if (flag)
			{
				Config.MultiplayerSkinSyncEnabled = enabled;
				Config.Save(ConfigPath);
			}
		}
		if (flag)
		{
			MultiplayerSkinSync.OnDirectionPreferenceChanged();
		}
	}

	public static void SetReceiveMultiplayerSkinChanges(bool enabled)
	{
		bool flag;
		lock (Sync)
		{
			EnsureConfigLoaded();
			flag = Config.LoadOtherPlayersCustomSkins != enabled;
			if (flag)
			{
				Config.LoadOtherPlayersCustomSkins = enabled;
				Config.Save(ConfigPath);
			}
		}
		if (flag)
		{
			MultiplayerSkinSync.OnReceivePreferenceChanged(enabled);
			MultiplayerSkinSync.OnDirectionPreferenceChanged();
		}
	}

	public static bool ShouldShowInRunAppearanceEntry()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			return Config.ShowInRunAppearanceEntry;
		}
	}

	public static void SetShowInRunAppearanceEntry(bool enabled)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			if (Config.ShowInRunAppearanceEntry != enabled)
			{
				Config.ShowInRunAppearanceEntry = enabled;
				Config.Save(ConfigPath);
			}
		}
	}

	public static bool ShouldPlaceCharacterSelectorTopRight()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			return Config.CharacterSelectorTopRight;
		}
	}

	public static (float X, float Y)? GetCharacterSkinControlPosition(bool mergeButton)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			float? num = (mergeButton ? Config.CharacterSkinMergeX : Config.CharacterSkinSelectorX);
			float? num2 = (mergeButton ? Config.CharacterSkinMergeY : Config.CharacterSkinSelectorY);
			(float, float)? result;
			if (num.HasValue)
			{
				float valueOrDefault = num.GetValueOrDefault();
				if (num2.HasValue)
				{
					float valueOrDefault2 = num2.GetValueOrDefault();
					if (float.IsFinite(valueOrDefault) && float.IsFinite(valueOrDefault2))
					{
						result = (Math.Clamp(valueOrDefault, 0f, 1f), Math.Clamp(valueOrDefault2, 0f, 1f));
						goto IL_00b7;
					}
				}
			}
			result = null;
			goto IL_00b7;
			IL_00b7:
			return result;
		}
	}

	public static void SetCharacterSkinControlPosition(bool mergeButton, float x, float y)
	{
		if (!float.IsFinite(x) || !float.IsFinite(y))
		{
			return;
		}
		lock (Sync)
		{
			EnsureConfigLoaded();
			if (mergeButton)
			{
				Config.CharacterSkinMergeX = Math.Clamp(x, 0f, 1f);
				Config.CharacterSkinMergeY = Math.Clamp(y, 0f, 1f);
			}
			else
			{
				Config.CharacterSkinSelectorX = Math.Clamp(x, 0f, 1f);
				Config.CharacterSkinSelectorY = Math.Clamp(y, 0f, 1f);
			}
			Config.Save(ConfigPath);
		}
	}

	public static void ResetCharacterSkinControlPosition(bool mergeButton)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			if (mergeButton)
			{
				Config.CharacterSkinMergeX = null;
				Config.CharacterSkinMergeY = null;
			}
			else
			{
				Config.CharacterSkinSelectorX = null;
				Config.CharacterSkinSelectorY = null;
				Config.CharacterSelectorTopRight = false;
			}
			Config.Save(ConfigPath);
		}
	}

	public static bool TryGetIndividualCardSkinSelectorPosition(out float x, out float y)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			float? individualCardSkinSelectorX = Config.IndividualCardSkinSelectorX;
			if (individualCardSkinSelectorX.HasValue)
			{
				float valueOrDefault = individualCardSkinSelectorX.GetValueOrDefault();
				individualCardSkinSelectorX = Config.IndividualCardSkinSelectorY;
				if (individualCardSkinSelectorX.HasValue)
				{
					float valueOrDefault2 = individualCardSkinSelectorX.GetValueOrDefault();
					if (float.IsFinite(valueOrDefault) && float.IsFinite(valueOrDefault2))
					{
						x = Math.Clamp(valueOrDefault, 0f, 1f);
						y = Math.Clamp(valueOrDefault2, 0f, 1f);
						return true;
					}
				}
			}
			x = 0f;
			y = 0f;
			return false;
		}
	}

	public static void SetIndividualCardSkinSelectorPosition(float x, float y)
	{
		if (!float.IsFinite(x) || !float.IsFinite(y))
		{
			return;
		}
		lock (Sync)
		{
			EnsureConfigLoaded();
			Config.IndividualCardSkinSelectorX = Math.Clamp(x, 0f, 1f);
			Config.IndividualCardSkinSelectorY = Math.Clamp(y, 0f, 1f);
			Config.Save(ConfigPath);
		}
	}

	public static void ResetIndividualCardSkinSelectorPosition()
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			if (Config.IndividualCardSkinSelectorX.HasValue || Config.IndividualCardSkinSelectorY.HasValue)
			{
				Config.IndividualCardSkinSelectorX = null;
				Config.IndividualCardSkinSelectorY = null;
				Config.Save(ConfigPath);
			}
		}
	}

	public static bool ShouldShowLoadOrderWarning(bool isBeforeAllSkinMods)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			bool flag = (Config.LastKnownBeforeAllSkinMods ?? Config.LastKnownFirstInLoadOrder) == true && !isBeforeAllSkinMods;
			bool num = Config.LastKnownBeforeAllSkinMods != isBeforeAllSkinMods;
			if (flag)
			{
				Config.SuppressLoadOrderWarning = false;
				ModLog.Info("检测到有皮肤 Mod 被移到本 Mod 之前，已恢复加载顺序提醒。");
			}
			Config.LastKnownBeforeAllSkinMods = isBeforeAllSkinMods;
			if (num || flag)
			{
				Config.Save(ConfigPath);
			}
			bool result = !isBeforeAllSkinMods;
			if (isBeforeAllSkinMods)
			{
				_loadOrderAutoReorderLogged = false;
			}
			else if (!_loadOrderAutoReorderLogged)
			{
				ModLog.Info(Config.SuppressLoadOrderWarning ? "检测到皮肤 Mod 排在本 Mod 之前；旧版“不再提示”设置不再生效，将自动调整本 Mod 到所有皮肤 Mod 之前。" : "检测到皮肤 Mod 排在本 Mod 之前，将自动调整本 Mod 到所有皮肤 Mod 之前。");
				_loadOrderAutoReorderLogged = true;
			}
			return result;
		}
	}

	public static void EnsureConfigLoaded()
	{
		lock (Sync)
		{
			if (!_configLoaded)
			{
				Config = LoadConfig();
				_configLoaded = true;
			}
		}
	}

	public static void InitializeBeforeAssets()
	{
		lock (Sync)
		{
			if (_initialized)
			{
				return;
			}
			try
			{
				Catalog?.Dispose();
				Catalog = null;
				RuntimeResourceCache.Clear();
				PreparedRuntimeOverlays.Clear();
				RuntimeResourceBundles.Clear();
				CardPortraitCache.Clear();
				ExternalCardProviderIdentityPaths.Clear();
				BaselineRelicAtlasCache.Clear();
				BaselineRelicIconCache.Clear();
				FailedCardPortraitRequests.Clear();
				IsolatedCardOverlayCache.Clear();
				CardCanonicalResourceOwners.Reset();
				CharacterPreviewSelections.Clear();
				_cardLookupCache = new ConditionalWeakTable<CardModel, CardLookup>();
				AncientStyleMethods.Clear();
				MissingAncientStyleMethods.Clear();
				FailedAncientStyleMethods.Clear();
				MountedOverlayCache.Clear();
				MountedScopedRuntimeProviderPacks.Clear();
				MountedLargeRuntimeProviderPacks.Clear();
				RuntimeCanonicalDependencyPaths.Clear();
				WarmedRuntimeProviderPacks.Clear();
				WarmingRuntimeProviderPacks.Clear();
				_runtimePackWarmGeneration++;
				ScopedMonsterSelections.Replace(new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase));
				_runtimeProviderBehaviorScope = null;
				RuntimeProviderScopeLeases.Reset();
				MountedLocalizationFiles.Clear();
				LocalizationStateCache.Clear();
				_mountedLocalizationSignature = null;
				CacheCleanupReport cacheCleanupReport = SkinChangerPaths.Storage.CleanCaches();
				if (cacheCleanupReport.RemovedFiles > 0)
				{
					ModLog.Info($"已清理 {cacheCleanupReport.RemovedFiles} 个过期皮肤缓存文件。");
				}
				foreach (string error in cacheCleanupReport.Errors)
				{
					ModLog.Warn("部分旧缓存清理失败：" + error);
				}
				string text = GamePackLocator.Resolve(OS.GetExecutablePath(), GameCommandLine.Arguments(), delegate(string message)
				{
					ModLog.Warn("定位游戏主资源包：" + message);
				});
				ModLog.Info("已定位游戏主资源包：" + text);
				Mod[] loadedMods = (from mod in ManagedSkinModLoader.GetCatalogMods().Where(delegate(Mod mod)
					{
						ModManifest manifest = mod.manifest;
						return manifest != null && manifest.id != null;
					})
					where !Entry.IsSelfModId(mod.manifest.id)
					select mod).ToArray();
				SkinModDescriptor[] array = loadedMods.Select((Mod mod) => new SkinModDescriptor(mod.manifest.id, mod.manifest.name ?? mod.manifest.id, mod.manifest.hasPck ? SkinPackagePaths.Resolve(mod.path, mod.manifest.id, ".pck") : null, ManagedSkinModLoader.ShouldTreatAsGameplayBaseline(mod, loadedMods), mod.path, mod.manifest.hasDll)).ToArray();
				Catalog = SkinCatalog.Build(text, array);
				_workshopGamePack = text;
				_workshopDescriptors = array;
				_workshopSourceStamps = WorkshopSourceStamps(array);
				ManagedCharacterAssetRegistrationGuard.Configure(Catalog);
				Config = LoadConfig();
				_configLoaded = true;
				SanitizeSelections();
				MountOverlay(Catalog.Groups.Select((SkinGroup group) => group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase));
				Config.Save(ConfigPath);
				_initialized = true;
				ModLog.Info($"发现 {Catalog.Groups.Count} 个生物外观组和 {Catalog.CardGroups.Count} 个卡牌外观组。" + "角色、怪物、先古之民与卡牌选项已接入对应界面。");
			}
			catch (Exception ex)
			{
				Catalog?.Dispose();
				Catalog = null;
				LastError = ex.ToString();
				ModLog.Error("初始化失败：" + ex);
			}
		}
	}

	public static void InitializeCardGroupsAfterModels()
	{
		lock (Sync)
		{
			if (Catalog == null || _cardGroupsInitialized)
			{
				return;
			}
			try
			{
				FrameworkCompatibilityLayer.SynchronizeSelections(Catalog, GetVisualSelections());
				InitializeMonsterSkinCategoriesAfterModels();
				InitializeEventSkinCategoriesAfterModels();
				CardCatalogEntry[] cards = BuildCardCatalogEntries(ModelDb.AllCards);
				RegisterCardArtPacks(cards);
				Catalog.FinalizeCardGroups(cards);
				_cardLookupCache = new ConditionalWeakTable<CardModel, CardLookup>();
				CardCoverageCache.Clear();
				MigrateStoredCardSelections();
				SanitizeCardSelections();
				MigrateLegacyCardSkinPresets();
				SanitizeCardSkinPresets();
				BundlePresetPolicy.Synchronize(Config, Catalog.CardGroups.Select((CardSkinGroup group) => group.Id), Config.MonsterSkinCategoryGroups.Keys);
				MountCardOverlay(Catalog.CardGroups.Select((CardSkinGroup group) => group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase));
				Config.Save(ConfigPath);
				_cardGroupsInitialized = true;
				LastError = null;
				ModLog.Info($"已按卡牌总览分类接入 {Catalog.CardGroups.Count} 个卡牌外观组。");
			}
			catch (Exception ex)
			{
				LastError = ex.ToString();
				ModLog.Error("按卡牌总览分类卡牌皮肤失败：" + ex);
			}
		}
	}

	private static void RegisterCardArtPacks(IEnumerable<CardCatalogEntry> cards)
	{
		SkinCatalog catalog = Catalog;
		if (catalog == null)
		{
			return;
		}
		try
		{
			IReadOnlyList<CardCatalogEntry> cards2 = (cards as IReadOnlyList<CardCatalogEntry>) ?? cards.ToArray();
			List<CardArtPack> list = new List<CardArtPack>();
			IReadOnlyList<string> readOnlyList = CardArtPackScanRoots();
			if (readOnlyList.Count > 0)
			{
				CardArtPackScanResult cardArtPackScanResult = CardArtPackScanner.Scan(readOnlyList);
				foreach (string warning in cardArtPackScanResult.Warnings)
				{
					ModLog.Warn(warning);
				}
				list.AddRange(cardArtPackScanResult.Packs);
			}
			list.AddRange(LooseCardPortraitPackScanner.Scan(LooseCardPortraitProviders(), cards2));
			if (list.Count == 0)
			{
				return;
			}
			var (readOnlySet, readOnlyList2) = catalog.AttachCardArtPacks(list, cards);
			if (readOnlySet.Count != 0)
			{
				ManagedSkinModLoader.RememberArtPackProviders(readOnlyList2.Select((CardArtPack pack) => pack.Id));
				ExternalCardVisualBridge.RegisterArtPackCoverage(readOnlySet);
				ModLog.Info($"已接管 {readOnlyList2.Count} 个纯素材卡图包（{string.Join("、", readOnlyList2.Select((CardArtPack pack) => pack.Name))}）；覆盖 {readOnlySet.Count} 张卡牌，无需 Card Art Editor 即可选用。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("接管纯素材卡图包失败：" + ex.GetBaseException().Message);
		}
	}

	private static IReadOnlyList<string> CardArtPackScanRoots()
	{
		List<string> list = new List<string>();
		Mod[] catalogMods = ManagedSkinModLoader.GetCatalogMods();
		for (int i = 0; i < catalogMods.Length; i++)
		{
			string directoryName = Path.GetDirectoryName(catalogMods[i].path);
			if (!string.IsNullOrWhiteSpace(directoryName) && !list.Contains<string>(directoryName, StringComparer.OrdinalIgnoreCase))
			{
				list.Add(directoryName);
			}
		}
		return list;
	}

	private static IReadOnlyList<LooseCardPortraitProvider> LooseCardPortraitProviders()
	{
		List<LooseCardPortraitProvider> list = new List<LooseCardPortraitProvider>();
		Mod[] catalogMods = ManagedSkinModLoader.GetCatalogMods();
		foreach (Mod val in catalogMods)
		{
			string text = val.manifest?.id;
			if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(val.path) && !Entry.IsSelfModId(text))
			{
				list.Add(new LooseCardPortraitProvider(text, val.manifest?.name ?? text, val.path));
			}
		}
		return list;
	}

	private static CardCatalogEntry[] BuildCardCatalogEntries(IEnumerable<CardModel> cards)
	{
		List<CardCatalogEntry> list = new List<CardCatalogEntry>();
		foreach (CardModel card in cards)
		{
			try
			{
				if (FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out string path))
				{
					list.Add(new CardCatalogEntry(((object)card).GetType().Name, path, GetCardPoolGroupId(card), GetCardCatalogGroupId(card), GetCardFilterGroupId(card))
					{
						IsCharacterPool = IsCharacterCardPool(card)
					});
				}
			}
			catch (Exception exception)
			{
				FrameworkCardVisualGuard.ReportCardReadFailure(card, exception);
			}
		}
		return list.ToArray();
	}

	private static void InitializeMonsterSkinCategoriesAfterModels()
	{
		SkinCatalog catalog = Catalog;
		int num = 0;
		foreach (ActModel act in ModelDb.Acts)
		{
			IReadOnlyList<string> groupIds = ResolveMonsterSkinGroupIds(catalog, act.AllMonsters);
			if (RegisterMonsterSkinCategory("act:" + ((AbstractModel)act).Id.Entry.ToLowerInvariant(), groupIds))
			{
				num++;
			}
		}
		if (typeof(ModelDb).GetProperty("EventEncounters")?.GetValue(null) is IEnumerable<EncounterModel> source && RegisterMonsterSkinCategory("events", ResolveMonsterSkinGroupIds(catalog, source.SelectMany((EncounterModel encounter) => encounter.AllPossibleMonsters))))
		{
			num++;
		}
		if (num > 0)
		{
			ModLog.Info($"已按怪物图鉴登记 {num} 个地区的怪物皮肤优先级。");
		}
	}

	private static IReadOnlyList<string> ResolveMonsterSkinGroupIds(SkinCatalog catalog, IEnumerable<MonsterModel> monsters)
	{
		return (from monster in monsters
			select catalog.ResolveManagedMonsterGroupId(((AbstractModel)monster).Id.Entry) ?? catalog.ResolveManagedMonsterGroupId(((object)monster).GetType().Name) into groupId
			where !string.IsNullOrWhiteSpace(groupId)
			select groupId).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public static void PrepareSelectedCharacterPreviews()
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null)
			{
				return;
			}
			long timestamp = Stopwatch.GetTimestamp();
			int num = 0;
			int num2 = 0;
			foreach (CharacterModel allCharacter in ModelDb.AllCharacters)
			{
				string groupId = ((AbstractModel)allCharacter).Id.Entry.ToLowerInvariant();
				SkinGroup skinGroup = catalog.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
				if (skinGroup == null)
				{
					continue;
				}
				string selection = Config.GetSelection(skinGroup.Id);
				if (selection.Equals("__base__", StringComparison.OrdinalIgnoreCase) || (catalog.IsRuntimeProviderOption(skinGroup.Id, selection) && !catalog.IsResourceBackedOption(skinGroup.Id, selection) && catalog.GetRuntimeImagePath(skinGroup.Id, selection) != null))
				{
					continue;
				}
				string[] resourcePaths = CharacterSelectResourcePaths(groupId);
				GetOrPrepareRuntimeOverlay(catalog, skinGroup.Id, selection, (IReadOnlyCollection<string>)(object)resourcePaths, includeProviderDependencies: true);
				num++;
				SkinOption skinOption = skinGroup.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
				if (skinOption != null)
				{
					IReadOnlyList<string> providerRelicSpritePaths = catalog.GetProviderRelicSpritePaths(skinOption);
					if (providerRelicSpritePaths.Count > 0)
					{
						GetOrPrepareRuntimeOverlay(catalog, skinGroup.Id, selection, providerRelicSpritePaths, includeProviderDependencies: false, isolateRelicCanonicalPaths: true);
						num2++;
					}
				}
			}
			if (num > 0)
			{
				ModLog.Info($"已在启动阶段准备 {num} 个角色的选角资源包，其中 {num2} 套包含完整遗物图集；耗时={Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds:F1} ms。");
			}
		}
	}

	public static void FocusRuntimeProviderBehaviorsOnCharacters(IEnumerable<string> groupIds)
	{
		FocusRuntimeProviderBehaviorsOnGroups(groupIds, (IReadOnlyCollection<string>)(object)Array.Empty<string>(), "角色预览");
	}

	public static long FocusRuntimeProviderBehaviorsOnGroups(IEnumerable<string> groupIds, IReadOnlyCollection<string> runEnvironmentProviderIds, string reason)
	{
		lock (Sync)
		{
			HashSet<string> hashSet = groupIds.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet2 = runEnvironmentProviderIds.Where((string providerId) => !string.IsNullOrWhiteSpace(providerId)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			RuntimeProviderScope runtimeProviderBehaviorScope = _runtimeProviderBehaviorScope;
			if ((object)runtimeProviderBehaviorScope != null && runtimeProviderBehaviorScope.RunEnvironmentProviderIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase).SetEquals(hashSet2) && runtimeProviderBehaviorScope.VisibleGroupIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase).SetEquals(hashSet))
			{
				return RuntimeProviderScopeLeases.Claim();
			}
			_runtimeProviderBehaviorScope = new RuntimeProviderScope(hashSet, hashSet2);
			long result = RuntimeProviderScopeLeases.Claim();
			SkinCatalog catalog = Catalog;
			if (catalog == null)
			{
				return result;
			}
			HashSet<string> activeRuntimeProviders = GetActiveRuntimeProviders(catalog);
			ManagedSkinModLoader.ConfigureRunEnvironmentProviders(hashSet2);
			ManagedSkinModLoader.DeactivateProvidersExcept(activeRuntimeProviders);
			EnsureScopedRuntimeProviderResourcesMounted(catalog, activeRuntimeProviders);
			foreach (string item in activeRuntimeProviders.Where(catalog.ProviderUsesManagedGodotScripts))
			{
				ManagedSkinModLoader.EnsureProviderGodotScripts(item);
			}
			foreach (string item2 in activeRuntimeProviders.Where(catalog.ProviderUsesScopedMonsterRuntime))
			{
				ManagedSkinModLoader.EnsureScopedMonsterSelectionRouter(item2);
			}
			ManagedSkinModLoader.ActivateSelectedProviders(activeRuntimeProviders);
			ManagedCharacterAssetRegistrationGuard.SuppressCurrentRegistrations();
			ScheduleRuntimeProviderPackWarm(catalog, activeRuntimeProviders, reason);
			ModLog.Info($"已按{reason}将皮肤代码行为收窄到 {hashSet.Count} 个可见外观组；保留 {activeRuntimeProviders.Count} 个当前场景需要的 DLL 皮肤提供者。");
			return result;
		}
	}

	public static bool TryFocusRuntimeProviderBehaviorsOnGroups(long expectedScopeLease, IEnumerable<string> groupIds, IReadOnlyCollection<string> runEnvironmentProviderIds, string reason, out long scopeLease)
	{
		lock (Sync)
		{
			if (!RuntimeProviderScopeLeases.IsCurrent(expectedScopeLease))
			{
				scopeLease = RuntimeProviderScopeLeases.Current;
				return false;
			}
			scopeLease = FocusRuntimeProviderBehaviorsOnGroups(groupIds, runEnvironmentProviderIds, reason);
			return true;
		}
	}

	public static IReadOnlyList<string> GetSelectionResourcePackPaths(string groupId, string optionId)
	{
		lock (Sync)
		{
			SkinOption skinOption = (Catalog?.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			if (skinOption == null || !skinOption.IsRuntimeProvider)
			{
				return Array.Empty<string>();
			}
			return Catalog.GetProviderResourcePackPaths(skinOption.EffectiveProviderId).Select(Path.GetFullPath).Distinct<string>(StringComparer.OrdinalIgnoreCase)
				.Where(File.Exists)
				.ToArray();
		}
	}

	public static async Task WarmResourcePackFilesAsync(IReadOnlyCollection<string> resourcePackPaths, Action<double>? reportProgress = null, CancellationToken cancellationToken = default(CancellationToken))
	{
		string[] array = resourcePackPaths.Where(File.Exists).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		long totalBytes = array.Sum((string path) => new FileInfo(path).Length);
		if (totalBytes <= 0)
		{
			reportProgress?.Invoke(1.0);
			return;
		}
		long completedBytes = 0L;
		byte[] buffer = GC.AllocateUninitializedArray<byte>(1048576);
		string[] array2 = array;
		foreach (string path2 in array2)
		{
			await using FileStream stream = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, buffer.Length, FileOptions.Asynchronous | FileOptions.SequentialScan);
			while (true)
			{
				int num = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (num != 0)
				{
					completedBytes += num;
					reportProgress?.Invoke(Math.Clamp((double)completedBytes / (double)totalBytes, 0.0, 1.0));
					continue;
				}
				break;
			}
		}
		reportProgress?.Invoke(1.0);
	}

	private static void ScheduleRuntimeProviderPackWarm(SkinCatalog catalog, IEnumerable<string> activeProviders, string reason)
	{
		int generation = ++_runtimePackWarmGeneration;
		List<string> list = new List<string>();
		foreach (string item in activeProviders.SelectMany(catalog.GetProviderResourcePackPaths).Select(Path.GetFullPath).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			try
			{
				bool alreadyWarmed = WarmedRuntimeProviderPacks.Contains(item) || WarmingRuntimeProviderPacks.Contains(item);
				if (File.Exists(item) && RuntimePackWarmPolicy.ShouldWarm(new FileInfo(item).Length, alreadyWarmed))
				{
					WarmingRuntimeProviderPacks.Add(item);
					list.Add(item);
				}
			}
			catch (Exception ex)
			{
				ModLog.Info("跳过无法检查的当前皮肤资源包 " + Path.GetFileName(item) + "：" + ex.GetBaseException().Message);
			}
		}
		if (list.Count > 0)
		{
			WarmRuntimeProviderPacksAsync(list, reason, generation);
		}
	}

	private static async Task WarmRuntimeProviderPacksAsync(IReadOnlyCollection<string> paths, string reason, int generation)
	{
		long started = Stopwatch.GetTimestamp();
		await RuntimePackWarmGate.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);
		try
		{
			lock (Sync)
			{
				if (generation != _runtimePackWarmGeneration)
				{
					foreach (string path in paths)
					{
						WarmingRuntimeProviderPacks.Remove(path);
					}
					return;
				}
			}
			await WarmResourcePackFilesAsync(paths).ConfigureAwait(continueOnCapturedContext: false);
			lock (Sync)
			{
				foreach (string path2 in paths)
				{
					WarmingRuntimeProviderPacks.Remove(path2);
					WarmedRuntimeProviderPacks.Add(path2);
				}
			}
			TimeSpan elapsedTime = Stopwatch.GetElapsedTime(started);
			if (elapsedTime >= TimeSpan.FromMilliseconds(50L, 0L))
			{
				ModLog.Info($"已后台预读{reason}所需的 {paths.Count} 个当前皮肤资源包；耗时={elapsedTime.TotalMilliseconds:F1} ms。");
			}
		}
		catch (Exception ex)
		{
			lock (Sync)
			{
				foreach (string path3 in paths)
				{
					WarmingRuntimeProviderPacks.Remove(path3);
				}
			}
			ModLog.Info("后台预读" + reason + "皮肤资源未完成，将在实际加载时继续：" + ex.GetBaseException().Message);
		}
		finally
		{
			RuntimePackWarmGate.Release();
		}
	}

	public static bool ApplySelection(string groupId, string optionId)
	{
		if (optionId.Equals("__event_category__", StringComparison.OrdinalIgnoreCase))
		{
			return FollowEventCategoryPriority(groupId);
		}
		if (optionId.Equals("__monster_category__", StringComparison.OrdinalIgnoreCase))
		{
			return FollowMonsterCategoryPriority(groupId);
		}
		lock (Sync)
		{
			if (Catalog == null)
			{
				LastError = "皮肤目录尚未初始化。";
				return false;
			}
			SkinGroup skinGroup = Catalog.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (skinGroup == null || (!optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && skinGroup.Options.All((SkinOption option) => !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
			{
				LastError = "未知的皮肤选择：" + groupId + "/" + optionId;
				return false;
			}
			IReadOnlyDictionary<string, string> readOnlyDictionary = Catalog.BuildVisualSelectionTransaction(groupId, optionId, Config.Selections);
			string value;
			Dictionary<string, (bool, string)> dictionary = readOnlyDictionary.Keys.ToDictionary<string, string, (bool, string)>((string key) => key, (string key) => (!Config.Selections.TryGetValue(key, out value)) ? (HadValue: false, Value: null) : (HadValue: true, Value: value), StringComparer.OrdinalIgnoreCase);
			List<string> visualProviderPriority = Config.VisualProviderPriority.ToList();
			List<string> monsterGroupsFollowingCategory = Config.MonsterGroupsFollowingCategory.ToList();
			List<string> monsterGroupsWithManualSelection = Config.MonsterGroupsWithManualSelection.ToList();
			List<string> manualGroups = Config.EventSkinPriorities.ManualGroups.ToList();
			Dictionary<string, string> activeMonsterSkinPresets = new Dictionary<string, string>(Config.ActiveMonsterSkinPresets, StringComparer.OrdinalIgnoreCase);
			HashSet<string> affectedGroups = readOnlyDictionary.Keys.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			bool flag = IsSameManualVariantSource(groupId, optionId) && affectedGroups.SetEquals(new _003C_003Ez__ReadOnlySingleElementList<string>(groupId));
			try
			{
				foreach (string item in affectedGroups.Where(EventSkinPolicy.IsEventGroup))
				{
					if (!Config.EventSkinPriorities.ManualGroups.Contains<string>(item, StringComparer.OrdinalIgnoreCase))
					{
						Config.EventSkinPriorities.ManualGroups.Add(item);
					}
				}
				foreach (string item2 in from pair in Config.MonsterSkinCategoryGroups
					where pair.Value.Any(affectedGroups.Contains)
					select pair.Key)
				{
					Config.ActiveMonsterSkinPresets.Remove(item2);
				}
				Config.MonsterGroupsFollowingCategory.RemoveAll((string candidate) => affectedGroups.Contains(candidate));
				foreach (string item3 in affectedGroups.Where((string affectedGroup) => Config.MonsterSkinCategoryGroups.Values.Any((List<string> groupIds) => groupIds.Contains<string>(affectedGroup, StringComparer.OrdinalIgnoreCase)) && !Config.MonsterGroupsWithManualSelection.Contains<string>(affectedGroup, StringComparer.OrdinalIgnoreCase)))
				{
					Config.MonsterGroupsWithManualSelection.Add(item3);
				}
				foreach (KeyValuePair<string, string> item4 in readOnlyDictionary)
				{
					Config.Selections[item4.Key] = item4.Value;
					if (!flag)
					{
						ClearRuntimeResourceCache(item4.Key);
					}
				}
				UpdateVisualProviderPriority(groupId, optionId);
				if (!flag)
				{
					MountOverlay(affectedGroups);
				}
				Config.Save(ConfigPath);
				ManualCharacterVariantBridge.ApplyLocalChoice(groupId);
				EventSkinRuntime.RefreshCurrent(groupId);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				foreach (KeyValuePair<string, (bool, string)> item5 in dictionary)
				{
					RestoreSelection(item5.Key, item5.Value.Item2, item5.Value.Item1);
					ClearRuntimeResourceCache(item5.Key);
				}
				Config.VisualProviderPriority = visualProviderPriority;
				Config.MonsterGroupsFollowingCategory = monsterGroupsFollowingCategory;
				Config.MonsterGroupsWithManualSelection = monsterGroupsWithManualSelection;
				Config.EventSkinPriorities.ManualGroups = manualGroups;
				Config.ActiveMonsterSkinPresets = activeMonsterSkinPresets;
				TryRestoreOverlay(affectedGroups, cardOverlay: false);
				LastError = ex.Message;
				ModLog.Error($"切换 {groupId} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool ApplyCardSelection(string groupId, string optionId)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				LastError = "皮肤目录尚未初始化。";
				return false;
			}
			CardSkinGroup cardSkinGroup = Catalog.CardGroups.FirstOrDefault((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (cardSkinGroup == null || (!optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && cardSkinGroup.Options.All((CardSkinOption option) => !option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))))
			{
				LastError = "未知的卡牌皮肤选择：" + groupId + "/" + optionId;
				return false;
			}
			CardSkinPriorityEntry[] requestedEntries = (from entry in GetCardPriorityEntriesInternal(cardSkinGroup)
				select entry with
				{
					Enabled = (!optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase))
				} into entry
				orderby entry.Enabled descending
				select entry).ToArray();
			return ApplyCardPriority(groupId, requestedEntries);
		}
	}

	public static string GetCardSelection(string groupId)
	{
		return Config.GetSelection(CardSelectionKey(groupId));
	}

	public static IReadOnlyList<CardPriorityOptionState> GetCardPriorityOptions(string groupId)
	{
		lock (Sync)
		{
			CardSkinGroup group = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (group == null)
			{
				return Array.Empty<CardPriorityOptionState>();
			}
			IReadOnlyList<CardSkinPriorityEntry> cardPriorityEntriesInternal = GetCardPriorityEntriesInternal(group);
			CardCoverageState coverage = GetCardCoverage(group);
			return cardPriorityEntriesInternal.Select(delegate(CardSkinPriorityEntry entry, int colorIndex)
			{
				CardSkinOption cardSkinOption = group.Options.First((CardSkinOption candidate) => candidate.Id.Equals(entry.OptionId, StringComparison.OrdinalIgnoreCase));
				return new CardPriorityOptionState(cardSkinOption.Id, cardSkinOption.Name, entry.Enabled, colorIndex, coverage.ByOption.GetValueOrDefault(entry.OptionId), coverage.TotalCards);
			}).ToArray();
		}
	}

	public static IReadOnlyList<CardSkinSourceState> GetCardSkinSources(CardModel card, string? displayGroupId = null)
	{
		lock (Sync)
		{
			CardLookup lookup = GetCardLookup(card);
			if (lookup.Group == null)
			{
				return Array.Empty<CardSkinSourceState>();
			}
			string current = GetEffectiveCardSelection(card, lookup);
			return (from pair in GetCardPriorityEntriesInternal(lookup.PriorityGroups.FirstOrDefault((CardSkinGroup @group) => @group.Id.Equals(displayGroupId, StringComparison.OrdinalIgnoreCase)) ?? lookup.Group).Select((CardSkinPriorityEntry entry, int colorIndex) => (Entry: entry, ColorIndex: colorIndex))
				where lookup.OptionsById.ContainsKey(pair.Entry.OptionId)
				select pair).Select(delegate((CardSkinPriorityEntry Entry, int ColorIndex) pair)
			{
				CardSkinOption option = lookup.OptionsById[pair.Entry.OptionId].Option;
				return new CardSkinSourceState(option.Id, option.Name, pair.Entry.Enabled, pair.ColorIndex, option.Id.Equals(current, StringComparison.OrdinalIgnoreCase));
			}).ToArray();
		}
	}

	public static int GetCardSkinSourceCount(CardModel card)
	{
		return GetCardSkinSources(card).Count;
	}

	public static IReadOnlyList<CardSkinPresetState> GetCardSkinPresets(string groupId)
	{
		lock (Sync)
		{
			return (from preset in Config.CardSkinPresets
				where preset.CategoryId?.Equals(groupId, StringComparison.OrdinalIgnoreCase) ?? false
				orderby BundlePresetPolicy.IsOwned(preset.Name) descending
				select new CardSkinPresetState(preset.Name, preset.Name.Equals(GetActiveCardSkinPreset(groupId), StringComparison.OrdinalIgnoreCase), BundlePresetPolicy.IsOwned(preset.Name) ? GetPresetDisplayName(preset.Name) : null)).ToArray();
		}
	}

	public static bool CreateCardSkinPreset(string groupId, string name)
	{
		lock (Sync)
		{
			if (!IsKnownCardGroup(groupId))
			{
				LastError = "未知的卡牌皮肤分类：" + groupId;
				return false;
			}
			string text = NormalizeCardSkinPresetName(name);
			if (text == null)
			{
				return false;
			}
			if (FindCardSkinPresetIndex(groupId, text) >= 0)
			{
				LastError = "卡图预设已存在：" + text;
				return false;
			}
			string activeCardSkinPreset = GetActiveCardSkinPreset(groupId);
			CardSkinPreset cardSkinPreset = CaptureCurrentCardSkinPreset(groupId, text);
			try
			{
				Config.CardSkinPresets.Add(cardSkinPreset);
				SetActiveCardSkinPreset(groupId, cardSkinPreset.Name);
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				Config.CardSkinPresets.Remove(cardSkinPreset);
				SetActiveCardSkinPreset(groupId, activeCardSkinPreset);
				LastError = ex.Message;
				ModLog.Error($"保存卡图预设 {text} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool OverwriteCardSkinPreset(string groupId, string name)
	{
		lock (Sync)
		{
			int num = FindCardSkinPresetIndex(groupId, name);
			if (num < 0)
			{
				LastError = "找不到卡图预设：" + name;
				return false;
			}
			CardSkinPreset cardSkinPreset = CloneCardSkinPreset(Config.CardSkinPresets[num]);
			string activeCardSkinPreset = GetActiveCardSkinPreset(groupId);
			CardSkinPreset cardSkinPreset2 = CaptureCurrentCardSkinPreset(groupId, cardSkinPreset.Name);
			try
			{
				Config.CardSkinPresets[num] = cardSkinPreset2;
				SetActiveCardSkinPreset(groupId, cardSkinPreset2.Name);
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				Config.CardSkinPresets[num] = cardSkinPreset;
				SetActiveCardSkinPreset(groupId, activeCardSkinPreset);
				LastError = ex.Message;
				ModLog.Error($"覆盖卡图预设 {cardSkinPreset.Name} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool RenameCardSkinPreset(string groupId, string currentName, string newName)
	{
		lock (Sync)
		{
			if (!CanRenameOrDeletePreset(currentName))
			{
				return false;
			}
			int num = FindCardSkinPresetIndex(groupId, currentName);
			string text = NormalizeCardSkinPresetName(newName);
			if (num < 0 || text == null)
			{
				if (num < 0)
				{
					LastError = "找不到卡图预设：" + currentName;
				}
				return false;
			}
			int num2 = FindCardSkinPresetIndex(groupId, text);
			if (num2 >= 0 && num2 != num)
			{
				LastError = "卡图预设已存在：" + text;
				return false;
			}
			string name = Config.CardSkinPresets[num].Name;
			string activeCardSkinPreset = GetActiveCardSkinPreset(groupId);
			List<CharacterSkinBundle> characterSkinBundles = UpdateBundlePresetReferences(monsterPreset: false, groupId, name, text);
			try
			{
				Config.CardSkinPresets[num].Name = text;
				if (name.Equals(activeCardSkinPreset, StringComparison.OrdinalIgnoreCase))
				{
					SetActiveCardSkinPreset(groupId, text);
				}
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				Config.CardSkinPresets[num].Name = name;
				Config.CharacterSkinBundles = characterSkinBundles;
				SetActiveCardSkinPreset(groupId, activeCardSkinPreset);
				LastError = ex.Message;
				ModLog.Error($"重命名卡图预设 {name} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool DeleteCardSkinPreset(string groupId, string name)
	{
		lock (Sync)
		{
			if (!CanRenameOrDeletePreset(name))
			{
				return false;
			}
			int num = FindCardSkinPresetIndex(groupId, name);
			if (num < 0)
			{
				LastError = "找不到卡图预设：" + name;
				return false;
			}
			CardSkinPreset cardSkinPreset = Config.CardSkinPresets[num];
			string activeCardSkinPreset = GetActiveCardSkinPreset(groupId);
			List<CharacterSkinBundle> characterSkinBundles = UpdateBundlePresetReferences(monsterPreset: false, groupId, cardSkinPreset.Name, null);
			try
			{
				Config.CardSkinPresets.RemoveAt(num);
				if (cardSkinPreset.Name.Equals(activeCardSkinPreset, StringComparison.OrdinalIgnoreCase))
				{
					SetActiveCardSkinPreset(groupId, null);
				}
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				Config.CardSkinPresets.Insert(num, cardSkinPreset);
				Config.CharacterSkinBundles = characterSkinBundles;
				SetActiveCardSkinPreset(groupId, activeCardSkinPreset);
				LastError = ex.Message;
				ModLog.Error($"删除卡图预设 {cardSkinPreset.Name} 失败：{ex}");
				return false;
			}
		}
	}

	public static bool ApplyCardSkinPreset(string groupId, string name)
	{
		lock (Sync)
		{
			int num = FindCardSkinPresetIndex(groupId, name);
			SkinCatalog catalog = Catalog;
			CardSkinGroup cardSkinGroup = catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (num < 0 || catalog == null || cardSkinGroup == null)
			{
				LastError = ((num < 0) ? ("找不到卡图预设：" + name) : ((catalog == null) ? "皮肤目录尚未初始化。" : ("未知的卡牌皮肤分类：" + groupId)));
				return false;
			}
			CardSkinPreset cardSkinPreset = Config.CardSkinPresets[num];
			List<CardSkinPriorityEntry> value;
			bool flag = Config.CardSkinPriorities.TryGetValue(groupId, out value);
			Dictionary<string, string> selections = CaptureCardSelectionsForGroup(groupId);
			string activeCardSkinPreset = GetActiveCardSkinPreset(groupId);
			int cardPriorityDefaultsVersion = Config.CardPriorityDefaultsVersion;
			try
			{
				ApplyCardPresetSettings(cardSkinGroup, cardSkinPreset);
				CardPreviewSelections.Clear();
				ClearCardPortraitCache(groupId);
				MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId });
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex2)
			{
				if (flag)
				{
					Config.CardSkinPriorities[groupId] = value;
				}
				else
				{
					Config.CardSkinPriorities.Remove(groupId);
				}
				ReplaceCardSelectionsForGroup(groupId, selections);
				SetActiveCardSkinPreset(groupId, activeCardSkinPreset);
				Config.CardPriorityDefaultsVersion = cardPriorityDefaultsVersion;
				CardPreviewSelections.Clear();
				ClearCardPortraitCache(groupId);
				try
				{
					MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId });
				}
				catch (Exception ex)
				{
					ModLog.Error("恢复卡图预设切换前的资源失败：" + ex);
				}
				LastError = ex2.Message;
				ModLog.Error($"应用卡图预设 {cardSkinPreset.Name} 失败：{ex2}");
				return false;
			}
		}
	}

	public static bool SetCardPriorityEnabled(string groupId, string optionId, bool enabled)
	{
		lock (Sync)
		{
			CardSkinGroup cardSkinGroup = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (cardSkinGroup == null)
			{
				LastError = "未知的卡牌皮肤分类：" + groupId;
				return false;
			}
			CardSkinPriorityEntry[] requestedEntries = (from entry in GetCardPriorityEntriesInternal(cardSkinGroup)
				select (!entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)) ? entry : entry with
				{
					Enabled = enabled
				}).ToArray();
			return ApplyCardPriority(groupId, requestedEntries);
		}
	}

	public static bool MoveCardPriority(string groupId, string optionId, int offset)
	{
		lock (Sync)
		{
			CardSkinGroup cardSkinGroup = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (cardSkinGroup == null)
			{
				LastError = "未知的卡牌皮肤分类：" + groupId;
				return false;
			}
			List<CardSkinPriorityEntry> list = GetCardPriorityEntriesInternal(cardSkinGroup).ToList();
			int num = list.FindIndex((CardSkinPriorityEntry entry) => entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			int num2 = Math.Clamp(num + offset, 0, list.Count - 1);
			if (num < 0 || num2 == num)
			{
				return num >= 0;
			}
			CardSkinPriorityEntry item = list[num];
			list.RemoveAt(num);
			list.Insert(num2, item);
			return ApplyCardPriority(groupId, list);
		}
	}

	private static bool ApplyCardPriority(string groupId, IReadOnlyList<CardSkinPriorityEntry> requestedEntries)
	{
		CardSkinGroup cardSkinGroup = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (cardSkinGroup == null)
		{
			LastError = "未知的卡牌皮肤分类：" + groupId;
			return false;
		}
		HashSet<string> knownIds = cardSkinGroup.Options.Select((CardSkinOption option) => option.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<CardSkinPriorityEntry> knownEntries = requestedEntries.Where((CardSkinPriorityEntry entry) => knownIds.Contains(entry.OptionId)).DistinctBy<CardSkinPriorityEntry, string>((CardSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
		foreach (CardSkinOption item in cardSkinGroup.Options.Where((CardSkinOption option) => knownEntries.All((CardSkinPriorityEntry entry) => !entry.OptionId.Equals(option.Id, StringComparison.OrdinalIgnoreCase))))
		{
			knownEntries.Add(new CardSkinPriorityEntry(item.Id, Enabled: true));
		}
		List<CardSkinPriorityEntry> value;
		List<CardSkinPriorityEntry> list = (Config.CardSkinPriorities.TryGetValue(cardSkinGroup.Id, out value) ? value.ToList() : null);
		List<CardSkinPriorityEntry> value2 = MergeKnownCardPriorityEntries(list ?? new List<CardSkinPriorityEntry>(), knownEntries, knownIds);
		string key = CardSelectionKey(cardSkinGroup.Id);
		string value3;
		bool hadPrevious = Config.Selections.TryGetValue(key, out value3);
		string activeCardSkinPreset = GetActiveCardSkinPreset(cardSkinGroup.Id);
		try
		{
			Config.CardSkinPriorities[cardSkinGroup.Id] = value2;
			Config.Selections[key] = knownEntries.FirstOrDefault((CardSkinPriorityEntry entry) => entry.Enabled)?.OptionId ?? "__base__";
			SetActiveCardSkinPreset(cardSkinGroup.Id, null);
			ClearCardPortraitCache(cardSkinGroup.Id);
			MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { cardSkinGroup.Id });
			Config.Save(ConfigPath);
			LastError = null;
			return true;
		}
		catch (Exception ex)
		{
			if (list == null)
			{
				Config.CardSkinPriorities.Remove(cardSkinGroup.Id);
			}
			else
			{
				Config.CardSkinPriorities[cardSkinGroup.Id] = list;
			}
			RestoreSelection(key, value3, hadPrevious);
			SetActiveCardSkinPreset(cardSkinGroup.Id, activeCardSkinPreset);
			ClearCardPortraitCache(cardSkinGroup.Id);
			TryRestoreOverlay(cardSkinGroup.Id, cardOverlay: true);
			LastError = ex.Message;
			ModLog.Error($"调整 {cardSkinGroup.Id} 卡牌皮肤优先级失败：{ex}");
			return false;
		}
	}

	public static bool RegisterMonsterSkinCategory(string categoryId, IEnumerable<string> groupIds)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null || string.IsNullOrWhiteSpace(categoryId))
			{
				return false;
			}
			List<string> knownGroupIds = (from groupId in groupIds
				where !string.IsNullOrWhiteSpace(groupId)
				where catalog.Groups.Any((SkinGroup @group) => @group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase) && @group.Options.Count > 0)
				select groupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
			if (knownGroupIds.Count == 0)
			{
				return false;
			}
			if (Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out List<string> value) && value.ToHashSet<string>(StringComparer.OrdinalIgnoreCase).SetEquals(knownGroupIds))
			{
				return true;
			}
			return ChangeMonsterPriorityConfiguration(categoryId, delegate
			{
				Config.MonsterSkinCategoryGroups[categoryId] = knownGroupIds;
			}, adoptUnconfiguredGroups: true);
		}
	}

	public static IReadOnlyList<MonsterPriorityOptionState> GetMonsterPriorityOptions(string categoryId)
	{
		lock (Sync)
		{
			IReadOnlyList<MonsterCategoryOptionState> options = GetMonsterCategoryOptionsInternal(categoryId);
			if (options.Count == 0)
			{
				return Array.Empty<MonsterPriorityOptionState>();
			}
			return GetMonsterPriorityEntriesInternal(categoryId).Select(delegate(MonsterSkinPriorityEntry entry, int colorIndex)
			{
				MonsterCategoryOptionState monsterCategoryOptionState = options.First((MonsterCategoryOptionState candidate) => candidate.OptionId.Equals(entry.OptionId, StringComparison.OrdinalIgnoreCase));
				return new MonsterPriorityOptionState(monsterCategoryOptionState.OptionId, monsterCategoryOptionState.Name, entry.Enabled, colorIndex, monsterCategoryOptionState.Coverage, monsterCategoryOptionState.TotalMonsters);
			}).ToArray();
		}
	}

	public static IReadOnlyList<MonsterSkinPresetState> GetMonsterSkinPresets(string categoryId)
	{
		lock (Sync)
		{
			string active = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
			return (from preset in Config.MonsterSkinPresets
				where preset.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase)
				orderby BundlePresetPolicy.IsOwned(preset.Name) descending
				select new MonsterSkinPresetState(preset.Name, preset.Name.Equals(active, StringComparison.OrdinalIgnoreCase), BundlePresetPolicy.IsOwned(preset.Name) ? GetPresetDisplayName(preset.Name) : null)).ToArray();
		}
	}

	public static bool CreateMonsterSkinPreset(string categoryId, string name)
	{
		lock (Sync)
		{
			string text = NormalizeMonsterSkinPresetName(name);
			if (text == null)
			{
				return false;
			}
			if (!Config.MonsterSkinCategoryGroups.ContainsKey(categoryId))
			{
				LastError = "未知的怪物图鉴分类：" + categoryId;
				return false;
			}
			if (FindMonsterSkinPresetIndex(categoryId, text) >= 0)
			{
				LastError = "怪物皮肤预设已存在：" + text;
				return false;
			}
			MonsterSkinPreset preset = CaptureCurrentMonsterSkinPreset(categoryId, text);
			string previousActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
			Config.MonsterSkinPresets.Add(preset);
			Config.ActiveMonsterSkinPresets[categoryId] = preset.Name;
			return SaveMonsterPresetConfiguration("保存", preset, delegate
			{
				Config.MonsterSkinPresets.Remove(preset);
				RestoreActiveMonsterPreset(categoryId, previousActive);
			});
		}
	}

	public static bool OverwriteMonsterSkinPreset(string categoryId, string name)
	{
		lock (Sync)
		{
			int index = FindMonsterSkinPresetIndex(categoryId, name);
			if (index < 0)
			{
				LastError = "找不到怪物皮肤预设：" + name;
				return false;
			}
			MonsterSkinPreset previous = CloneMonsterSkinPreset(Config.MonsterSkinPresets[index]);
			string previousActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
			Config.MonsterSkinPresets[index] = CaptureCurrentMonsterSkinPreset(categoryId, previous.Name);
			Config.ActiveMonsterSkinPresets[categoryId] = previous.Name;
			return SaveMonsterPresetConfiguration("覆盖", previous, delegate
			{
				Config.MonsterSkinPresets[index] = previous;
				RestoreActiveMonsterPreset(categoryId, previousActive);
			});
		}
	}

	public static bool RenameMonsterSkinPreset(string categoryId, string currentName, string newName)
	{
		lock (Sync)
		{
			if (!CanRenameOrDeletePreset(currentName))
			{
				return false;
			}
			int num = FindMonsterSkinPresetIndex(categoryId, currentName);
			string text = NormalizeMonsterSkinPresetName(newName);
			if (text == null)
			{
				return false;
			}
			if (num < 0)
			{
				LastError = "找不到怪物皮肤预设：" + currentName;
				return false;
			}
			int num2 = FindMonsterSkinPresetIndex(categoryId, text);
			if (num2 >= 0 && num2 != num)
			{
				LastError = "怪物皮肤预设已存在：" + text;
				return false;
			}
			MonsterSkinPreset preset = Config.MonsterSkinPresets[num];
			string previousName = preset.Name;
			List<CharacterSkinBundle> previousBundles = UpdateBundlePresetReferences(monsterPreset: true, categoryId, previousName, text);
			bool wasActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId)?.Equals(previousName, StringComparison.OrdinalIgnoreCase) ?? false;
			preset.Name = text;
			if (wasActive)
			{
				Config.ActiveMonsterSkinPresets[categoryId] = text;
			}
			return SaveMonsterPresetConfiguration("重命名", preset, delegate
			{
				preset.Name = previousName;
				Config.CharacterSkinBundles = previousBundles;
				if (wasActive)
				{
					Config.ActiveMonsterSkinPresets[categoryId] = previousName;
				}
			});
		}
	}

	public static bool DeleteMonsterSkinPreset(string categoryId, string name)
	{
		lock (Sync)
		{
			if (!CanRenameOrDeletePreset(name))
			{
				return false;
			}
			int index = FindMonsterSkinPresetIndex(categoryId, name);
			if (index < 0)
			{
				LastError = "找不到怪物皮肤预设：" + name;
				return false;
			}
			MonsterSkinPreset preset = Config.MonsterSkinPresets[index];
			string previousActive = Config.ActiveMonsterSkinPresets.GetValueOrDefault(categoryId);
			List<CharacterSkinBundle> previousBundles = UpdateBundlePresetReferences(monsterPreset: true, categoryId, preset.Name, null);
			Config.MonsterSkinPresets.RemoveAt(index);
			if (preset.Name.Equals(previousActive, StringComparison.OrdinalIgnoreCase))
			{
				Config.ActiveMonsterSkinPresets.Remove(categoryId);
			}
			return SaveMonsterPresetConfiguration("删除", preset, delegate
			{
				Config.MonsterSkinPresets.Insert(index, preset);
				Config.CharacterSkinBundles = previousBundles;
				if (previousActive != null)
				{
					Config.ActiveMonsterSkinPresets[categoryId] = previousActive;
				}
			});
		}
	}

	public static bool ApplyMonsterSkinPreset(string categoryId, string name)
	{
		lock (Sync)
		{
			int num = FindMonsterSkinPresetIndex(categoryId, name);
			if (num < 0)
			{
				LastError = "找不到怪物皮肤预设：" + name;
				return false;
			}
			MonsterSkinPreset preset = Config.MonsterSkinPresets[num];
			return ChangeMonsterPriorityConfiguration(categoryId, delegate
			{
				ApplyMonsterPresetSettings(preset);
			}, adoptUnconfiguredGroups: false);
		}
	}

	private static void ApplyMonsterPresetSettings(MonsterSkinPreset preset)
	{
		string categoryId = preset.CategoryId;
		HashSet<string> currentGroupIds = Config.MonsterSkinCategoryGroups[categoryId].ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> selections = (from pair in BundlePresetPolicy.MonsterSelections(preset, currentGroupIds)
			where currentGroupIds.Contains(pair.Key)
			select pair).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, delegate(KeyValuePair<string, string> pair)
		{
			SkinGroup skinGroup = Catalog?.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(pair.Key, StringComparison.OrdinalIgnoreCase));
			return (!pair.Value.Equals("__base__", StringComparison.OrdinalIgnoreCase) && (skinGroup == null || !skinGroup.Options.Any((SkinOption option) => option.Id.Equals(pair.Value, StringComparison.OrdinalIgnoreCase)))) ? "__base__" : pair.Value;
		}, StringComparer.OrdinalIgnoreCase);
		Config.MonsterSkinPriorities[categoryId] = (preset.AllOriginal ? (from entry in GetMonsterPriorityEntriesInternal(categoryId)
			select entry with
			{
				Enabled = false
			}).ToList() : preset.Priority.ToList());
		Config.Selections = MonsterSkinPresetPolicy.Apply(new MonsterSkinPresetSnapshot(preset.CategoryId, preset.Priority.Select((MonsterSkinPriorityEntry entry) => new MonsterSkinPresetPriorityState(entry.OptionId, entry.Enabled)).ToArray(), selections), Config.Selections);
		foreach (string groupId in Config.MonsterSkinCategoryGroups[categoryId])
		{
			bool num = !preset.AllOriginal && preset.FollowingGroupIds.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase);
			Config.MonsterGroupsFollowingCategory.RemoveAll((string candidate) => candidate.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			Config.MonsterGroupsWithManualSelection.RemoveAll((string candidate) => candidate.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			(num ? Config.MonsterGroupsFollowingCategory : Config.MonsterGroupsWithManualSelection).Add(groupId);
		}
		Config.ActiveMonsterSkinPresets[categoryId] = preset.Name;
	}

	public static IReadOnlySet<string> GetMonsterRunEnvironmentProviders(string categoryId, IEnumerable<string> groupIds, IReadOnlySet<string>? activeCombatGroupIds = null)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null)
			{
				return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			SkinOption[] source = (from groupId in groupIds.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase)
				select catalog.Groups.FirstOrDefault((SkinGroup @group) => @group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)) into @group
				where @group != null
				select @group).Cast<SkinGroup>().ToArray().SelectMany((SkinGroup group) => group.Options)
				.DistinctBy<SkinOption, string>((SkinOption option) => option.Id, StringComparer.OrdinalIgnoreCase)
				.ToArray();
			HashSet<string> knownOptionIds = source.Select((SkinOption option) => option.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<MonsterSkinPriorityEntry> value;
			List<MonsterSkinPriorityEntry> priorities = (Config.MonsterSkinPriorities.TryGetValue(categoryId, out value) ? value.Where((MonsterSkinPriorityEntry entry) => knownOptionIds.Contains(entry.OptionId)).DistinctBy<MonsterSkinPriorityEntry, string>((MonsterSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList() : new List<MonsterSkinPriorityEntry>());
			foreach (SkinOption item in source.Where((SkinOption option) => priorities.All((MonsterSkinPriorityEntry entry) => !entry.OptionId.Equals(option.Id, StringComparison.OrdinalIgnoreCase))))
			{
				priorities.Add(new MonsterSkinPriorityEntry(item.Id, Enabled: true));
			}
			HashSet<string> activeProviderIds = (from groupId in activeCombatGroupIds?.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId))
				select catalog.ResolveVisualProviderId(GetVisualSelection(groupId))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> manuallySelectedActiveProviderIds = (from groupId in activeCombatGroupIds?.Where((string groupId) => Config.MonsterGroupsWithManualSelection.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase))
				select catalog.ResolveVisualProviderId(GetVisualSelection(groupId))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			return RuntimeProviderScopePolicy.SelectRunEnvironmentProviders(priorities.Select(delegate(MonsterSkinPriorityEntry entry)
			{
				string text = catalog.ResolveVisualProviderId(entry.OptionId);
				return new RuntimeProviderPriorityCandidate(text, entry.Enabled || manuallySelectedActiveProviderIds.Contains(text), catalog.ProviderUsesScopedMonsterRuntime(text), activeProviderIds == null || activeProviderIds.Contains(text));
			}));
		}
	}

	public static bool SetMonsterPriorityOptionEnabled(string categoryId, string optionId, bool enabled)
	{
		lock (Sync)
		{
			List<MonsterSkinPriorityEntry> entries = (from entry in GetMonsterPriorityEntriesInternal(categoryId)
				select (!entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)) ? entry : entry with
				{
					Enabled = enabled
				}).ToList();
			if (entries.All((MonsterSkinPriorityEntry entry) => !entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)))
			{
				LastError = "未知的怪物皮肤选项：" + categoryId + "/" + optionId;
				return false;
			}
			return ChangeMonsterPriorityConfiguration(categoryId, delegate
			{
				Config.MonsterSkinPriorities[categoryId] = entries;
			}, adoptUnconfiguredGroups: false);
		}
	}

	public static bool MoveMonsterPriority(string categoryId, string optionId, int offset)
	{
		lock (Sync)
		{
			List<MonsterSkinPriorityEntry> entries = GetMonsterPriorityEntriesInternal(categoryId).ToList();
			int num = entries.FindIndex((MonsterSkinPriorityEntry entry) => entry.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			int num2 = Math.Clamp(num + offset, 0, entries.Count - 1);
			if (num < 0 || num2 == num)
			{
				return num >= 0;
			}
			MonsterSkinPriorityEntry item = entries[num];
			entries.RemoveAt(num);
			entries.Insert(num2, item);
			return ChangeMonsterPriorityConfiguration(categoryId, delegate
			{
				Config.MonsterSkinPriorities[categoryId] = entries;
			}, adoptUnconfiguredGroups: false);
		}
	}

	public static bool HasMonsterSkinCategory(string groupId)
	{
		return Config.MonsterSkinCategoryGroups.Values.Any((List<string> groupIds) => groupIds.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase));
	}

	public static string GetMonsterOverrideSelection(string groupId)
	{
		if (!Config.MonsterGroupsFollowingCategory.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase))
		{
			return Config.GetSelection(groupId);
		}
		return "__monster_category__";
	}

	public static bool FollowMonsterCategoryPriority(string groupId)
	{
		lock (Sync)
		{
			string key = Config.MonsterSkinCategoryGroups.FirstOrDefault<KeyValuePair<string, List<string>>>((KeyValuePair<string, List<string>> pair) => pair.Value.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)).Key;
			if (string.IsNullOrWhiteSpace(key))
			{
				LastError = "怪物 " + groupId + " 不属于已登记的图鉴分类。";
				return false;
			}
			return ChangeMonsterPriorityConfiguration(key, delegate
			{
				Config.MonsterGroupsWithManualSelection.RemoveAll((string candidate) => candidate.Equals(groupId, StringComparison.OrdinalIgnoreCase));
				if (!Config.MonsterGroupsFollowingCategory.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase))
				{
					Config.MonsterGroupsFollowingCategory.Add(groupId);
				}
			}, adoptUnconfiguredGroups: false);
		}
	}

	private static bool ChangeMonsterPriorityConfiguration(string categoryId, Action mutation, bool adoptUnconfiguredGroups)
	{
		Dictionary<string, string> previousSelections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, List<MonsterSkinPriorityEntry>> monsterSkinPriorities = Config.MonsterSkinPriorities.ToDictionary<KeyValuePair<string, List<MonsterSkinPriorityEntry>>, string, List<MonsterSkinPriorityEntry>>((KeyValuePair<string, List<MonsterSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<MonsterSkinPriorityEntry>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
		Dictionary<string, List<string>> monsterSkinCategoryGroups = Config.MonsterSkinCategoryGroups.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> pair) => pair.Key, (KeyValuePair<string, List<string>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
		List<string> enabledMonsterSkinPriorityCategories = Config.EnabledMonsterSkinPriorityCategories.ToList();
		List<string> monsterGroupsFollowingCategory = Config.MonsterGroupsFollowingCategory.ToList();
		List<string> monsterGroupsWithManualSelection = Config.MonsterGroupsWithManualSelection.ToList();
		Dictionary<string, string> activeMonsterSkinPresets = new Dictionary<string, string>(Config.ActiveMonsterSkinPresets, StringComparer.OrdinalIgnoreCase);
		List<string> visualProviderPriority = Config.VisualProviderPriority.ToList();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			Config.ActiveMonsterSkinPresets.Remove(categoryId);
			mutation();
			if (!Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out List<string> value))
			{
				throw new InvalidOperationException("未知的怪物图鉴分类：" + categoryId);
			}
			if (adoptUnconfiguredGroups)
			{
				foreach (string item in value.Where((string groupId) => !Config.MonsterGroupsWithManualSelection.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)))
				{
					if (!Config.MonsterGroupsFollowingCategory.Contains<string>(item, StringComparer.OrdinalIgnoreCase))
					{
						Config.MonsterGroupsFollowingCategory.Add(item);
					}
				}
			}
			GetMonsterPriorityEntriesInternal(categoryId);
			hashSet = ApplyMonsterCategoryPriorityToSelections(categoryId);
			hashSet.UnionWith(from groupId in Config.Selections.Keys.Union<string>(previousSelections.Keys, StringComparer.OrdinalIgnoreCase)
				where !string.Equals(Config.Selections.GetValueOrDefault(groupId), previousSelections.GetValueOrDefault(groupId), StringComparison.OrdinalIgnoreCase)
				select groupId);
			foreach (string item2 in hashSet)
			{
				ClearRuntimeResourceCache(item2);
				UpdateVisualProviderPriority(item2, Config.GetSelection(item2));
			}
			if (hashSet.Count > 0)
			{
				MountOverlay(hashSet);
			}
			Config.Save(ConfigPath);
			LastError = null;
			return true;
		}
		catch (Exception ex)
		{
			HashSet<string> hashSet2 = (from groupId in Config.Selections.Keys.Union<string>(previousSelections.Keys, StringComparer.OrdinalIgnoreCase)
				where !string.Equals(Config.Selections.GetValueOrDefault(groupId), previousSelections.GetValueOrDefault(groupId), StringComparison.OrdinalIgnoreCase)
				select groupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			hashSet2.UnionWith(hashSet);
			Config.Selections = previousSelections;
			Config.MonsterSkinPriorities = monsterSkinPriorities;
			Config.MonsterSkinCategoryGroups = monsterSkinCategoryGroups;
			Config.EnabledMonsterSkinPriorityCategories = enabledMonsterSkinPriorityCategories;
			Config.MonsterGroupsFollowingCategory = monsterGroupsFollowingCategory;
			Config.MonsterGroupsWithManualSelection = monsterGroupsWithManualSelection;
			Config.ActiveMonsterSkinPresets = activeMonsterSkinPresets;
			Config.VisualProviderPriority = visualProviderPriority;
			foreach (string item3 in hashSet2)
			{
				ClearRuntimeResourceCache(item3);
			}
			if (hashSet2.Count > 0)
			{
				TryRestoreOverlay(hashSet2, cardOverlay: false);
			}
			LastError = ex.Message;
			ModLog.Error($"调整 {categoryId} 怪物皮肤优先级失败：{ex}");
			return false;
		}
	}

	public static bool ShouldDriveManagedCharacterAnimations(string groupId)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null)
			{
				return false;
			}
			string visualSelection = GetVisualSelection(groupId);
			return catalog.IsRuntimeProviderOption(groupId, visualSelection) && catalog.ProviderUsesManagedCharacterScene(groupId, visualSelection);
		}
	}

	public static IReadOnlyList<CardSkinOption> GetCardOptions(CardModel card)
	{
		lock (Sync)
		{
			return GetCardLookup(card).Options;
		}
	}

	public static (IReadOnlyList<CardSkinOption> Options, IReadOnlyList<SkinPriorityState> States) GetCardOptionsForDisplay(CardModel card)
	{
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			if (cardLookup.Group == null)
			{
				return (Options: cardLookup.Options, States: Array.Empty<SkinPriorityState>());
			}
			IReadOnlyList<SkinPriorityState> cardPriorityStatesInternal = GetCardPriorityStatesInternal(cardLookup.Group);
			return (Options: SkinPriorityDisplayPolicy.Project(cardLookup.Options, (CardSkinOption option) => option.Id, cardPriorityStatesInternal), States: cardPriorityStatesInternal);
		}
	}

	private static IReadOnlyList<SkinPriorityState> GetCardPriorityStatesInternal(CardSkinGroup group)
	{
		return SkinPriorityDisplayPolicy.States(GetCardPriorityEntriesInternal(group), (CardSkinPriorityEntry entry) => entry.OptionId, (CardSkinPriorityEntry entry) => entry.Enabled);
	}

	public static IReadOnlyList<SkinPriorityState>? GetVisualPriorityStates(string groupId)
	{
		lock (Sync)
		{
			string key = Config.MonsterSkinCategoryGroups.FirstOrDefault<KeyValuePair<string, List<string>>>((KeyValuePair<string, List<string>> pair) => pair.Value.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)).Key;
			if (!string.IsNullOrWhiteSpace(key))
			{
				return SkinPriorityDisplayPolicy.States(GetMonsterPriorityEntriesInternal(key), (MonsterSkinPriorityEntry entry) => entry.OptionId, (MonsterSkinPriorityEntry entry) => entry.Enabled);
			}
			string key2 = Config.EventSkinPriorities.RegionGroups.FirstOrDefault<KeyValuePair<string, List<string>>>((KeyValuePair<string, List<string>> pair) => pair.Value.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)).Key;
			return string.IsNullOrWhiteSpace(key2) ? null : SkinPriorityDisplayPolicy.States(GetEventPriorityEntries(key2), (EventSkinPriorityEntry entry) => entry.OptionId, (EventSkinPriorityEntry entry) => entry.Enabled);
		}
	}

	public static (IReadOnlyList<SkinOption> Options, IReadOnlyList<SkinPriorityState>? States) GetSkinListForDisplay(string groupId, IReadOnlyList<SkinOption> options)
	{
		IReadOnlyList<SkinPriorityState> visualPriorityStates = GetVisualPriorityStates(groupId);
		IReadOnlyList<SkinOption> item;
		if (visualPriorityStates != null)
		{
			item = SkinPriorityDisplayPolicy.Project(options, (SkinOption option) => option.Id, visualPriorityStates);
		}
		else
		{
			item = options;
		}
		return (Options: item, States: visualPriorityStates);
	}

	public static string? GetCardOptionName(string optionId)
	{
		lock (Sync)
		{
			return Catalog?.CardGroups.SelectMany((CardSkinGroup group) => group.Options).FirstOrDefault((CardSkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))?.Name;
		}
	}

	public static bool HasCardSkin(CardModel card)
	{
		lock (Sync)
		{
			return GetCardLookup(card).Options.Count > 0;
		}
	}

	public static string GetCardOverrideSelection(CardModel card)
	{
		lock (Sync)
		{
			return Config.Selections.GetValueOrDefault(IndividualCardSelectionKey(card), "__inherit__");
		}
	}

	public static string GetEffectiveCardSelection(CardModel card)
	{
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			return GetEffectiveCardSelection(card, cardLookup);
		}
	}

	public static void WithCardPreviewSelection(CardModel card, string selection, Action previewAction)
	{
		string key = IndividualCardSelectionKey(card);
		string value = null;
		bool flag;
		lock (Sync)
		{
			flag = CardPreviewSelections.TryGetValue(key, out value);
			CardPreviewSelections[key] = selection;
		}
		try
		{
			previewAction();
		}
		finally
		{
			lock (Sync)
			{
				if (flag)
				{
					CardPreviewSelections[key] = value;
				}
				else
				{
					CardPreviewSelections.Remove(key);
				}
			}
		}
	}

	private static string GetEffectiveCardSelection(CardModel card, CardLookup lookup)
	{
		return ResolveCardSelection(card, lookup).OptionId;
	}

	public static string? GetCardInheritedGroupId(CardModel card)
	{
		lock (Sync)
		{
			return ResolveCardSelection(card, GetCardLookup(card)).GroupId;
		}
	}

	private static (string OptionId, string? GroupId) ResolveCardSelection(CardModel card, CardLookup lookup)
	{
		string key = IndividualCardSelectionKey(card);
		string value;
		string text = (CardPreviewSelections.TryGetValue(key, out value) ? value : Config.Selections.GetValueOrDefault(key, "__inherit__"));
		if (text.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return (OptionId: text, GroupId: null);
		}
		if (lookup.OptionsById.ContainsKey(text))
		{
			return (OptionId: text, GroupId: null);
		}
		HashSet<string> hashSet = null;
		for (int i = 0; i < lookup.PriorityGroups.Count; i++)
		{
			CardSkinGroup cardSkinGroup = lookup.PriorityGroups[i];
			IReadOnlyList<CardSkinPriorityEntry> cardPriorityEntriesInternal = GetCardPriorityEntriesInternal(cardSkinGroup);
			foreach (CardSkinPriorityEntry item in cardPriorityEntriesInternal)
			{
				if (item.Enabled && (hashSet == null || !hashSet.Contains(item.OptionId)) && lookup.OptionsById.TryGetValue(item.OptionId, out CardOptionLookup value2))
				{
					return (OptionId: value2.Option.Id, GroupId: cardSkinGroup.Id);
				}
			}
			if (cardPriorityEntriesInternal.Count > 0 && cardPriorityEntriesInternal.All((CardSkinPriorityEntry entry) => !entry.Enabled))
			{
				return (OptionId: "__base__", GroupId: cardSkinGroup.Id);
			}
			if (i + 1 < lookup.PriorityGroups.Count)
			{
				if (hashSet == null)
				{
					hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				}
				hashSet.UnionWith(from entry in cardPriorityEntriesInternal
					where !entry.Enabled
					select entry.OptionId);
			}
		}
		return (OptionId: "__base__", GroupId: null);
	}

	internal static CardSkinOption? GetSelectedCardOption(CardModel card)
	{
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			return cardLookup.OptionsById.GetValueOrDefault(GetEffectiveCardSelection(card, cardLookup))?.Option;
		}
	}

	internal static CardSurfaceDefinition? GetCardSurface(CardModel card)
	{
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			return cardLookup.OptionsById.GetValueOrDefault(GetEffectiveCardSelection(card, cardLookup))?.Option.CardSurfaces.GetValueOrDefault(cardLookup.CardType);
		}
	}

	public static CardPresentationDefinition? GetCardPresentation(CardModel card)
	{
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			string effectiveCardSelection = GetEffectiveCardSelection(card, cardLookup);
			if (!cardLookup.OptionsById.TryGetValue(effectiveCardSelection, out CardOptionLookup value))
			{
				return null;
			}
			if (value.Option.StatefulArt != null)
			{
				return StatefulCardArtRuntime.For(value.Option)?.Presentation(card);
			}
			return value.Option.CardPresentations.GetValueOrDefault(cardLookup.CardType);
		}
	}

	public static T? LoadCardPresentationResource<T>(CardModel card, string? resourcePath) where T : Resource
	{
		if (string.IsNullOrWhiteSpace(resourcePath))
		{
			return default(T);
		}
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			string effectiveCardSelection = GetEffectiveCardSelection(card, cardLookup);
			cardLookup.OptionsById.TryGetValue(effectiveCardSelection, out CardOptionLookup value);
			CardSkinOption cardSkinOption = value?.Option;
			string key = $"card-presentation:{cardLookup.GroupId}:{typeof(T).FullName}:{effectiveCardSelection}:{resourcePath}";
			if (RuntimeResourceCache.TryGetValue(key, out Resource value2))
			{
				T val = (T)(object)((value2 is T) ? value2 : null);
				if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
				{
					return val;
				}
			}
			T val2 = default(T);
			if (cardLookup.Group != null)
			{
				bool[] array = new bool[2] { true, false };
				foreach (bool flag in array)
				{
					if (flag && ((object)cardSkinOption == null || cardSkinOption.ProviderRootPath == null))
					{
						continue;
					}
					try
					{
						IEnumerable<string> enumerable2;
						if (!flag || !(cardSkinOption != null))
						{
							IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(resourcePath);
							enumerable2 = enumerable;
						}
						else
						{
							enumerable2 = cardSkinOption.CardPresentations.Values.SelectMany((CardPresentationDefinition presentation) => presentation.ResourcePaths).Append<string>(resourcePath);
						}
						IEnumerable<string> resourcePaths = enumerable2;
						IsolatedCardOverlayState isolatedCardOverlayState = EnsureIsolatedCardOverlay(cardLookup.Group.Id, effectiveCardSelection, flag, resourcePaths);
						if (isolatedCardOverlayState.ResourcePaths.TryGetValue(resourcePath, out string value3))
						{
							val2 = ResourceLoader.Load<T>(value3, (string)null, (CacheMode)(isolatedCardOverlayState.CanReuseExternalDependencies ? 0 : 3));
						}
					}
					catch
					{
					}
					if (val2 != null)
					{
						break;
					}
				}
			}
			if (val2 == null)
			{
				val2 = ResourceLoader.Load<T>(resourcePath, (string)null, (CacheMode)1);
			}
			if (val2 != null)
			{
				RuntimeResourceCache[key] = (Resource)(object)val2;
			}
			return val2;
		}
	}

	public static bool ApplyCardSelection(CardModel card, string optionId)
	{
		lock (Sync)
		{
			CardLookup cardLookup = GetCardLookup(card);
			CardSkinGroup group = cardLookup.Group;
			if (group == null)
			{
				LastError = $"没有找到卡牌 {((AbstractModel)card).Id} 的皮肤分类。";
				return false;
			}
			if (!optionId.Equals("__inherit__", StringComparison.OrdinalIgnoreCase) && !optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase) && !cardLookup.OptionsById.ContainsKey(optionId))
			{
				LastError = $"未知的单卡皮肤选择：{((AbstractModel)card).Id}/{optionId}";
				return false;
			}
			string key = IndividualCardSelectionKey(card);
			string value;
			bool hadPrevious = Config.Selections.TryGetValue(key, out value);
			string activeCardSkinPreset = GetActiveCardSkinPreset(group.Id);
			try
			{
				if (optionId.Equals("__inherit__", StringComparison.OrdinalIgnoreCase))
				{
					Config.Selections.Remove(key);
				}
				else
				{
					Config.Selections[key] = optionId;
				}
				SetActiveCardSkinPreset(group.Id, null);
				ClearCardPortraitCache(group.Id);
				MountCardOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { group.Id });
				Config.Save(ConfigPath);
				LastError = null;
				return true;
			}
			catch (Exception ex)
			{
				RestoreSelection(key, value, hadPrevious);
				SetActiveCardSkinPreset(group.Id, activeCardSkinPreset);
				ClearCardPortraitCache(group.Id);
				TryRestoreOverlay(group.Id, cardOverlay: true);
				LastError = ex.Message;
				ModLog.Error($"切换单卡 {((AbstractModel)card).Id} 皮肤失败：{ex}");
				return false;
			}
		}
	}

	public static void ReplaceCardPortrait(CardModel card, ref Texture2D result)
	{
		lock (Sync)
		{
			CardPortraitRequest cardPortraitRequest = ResolveCardPortraitRequest(card);
			if (!(cardPortraitRequest == null))
			{
				Texture2D orLoadCardPortrait = GetOrLoadCardPortrait(cardPortraitRequest);
				if (orLoadCardPortrait != null)
				{
					result = orLoadCardPortrait;
				}
			}
		}
	}

	public static string? GetExternalCardProviderIdentity(CardModel card, Texture2D texture)
	{
		lock (Sync)
		{
			CardPortraitRequest cardPortraitRequest = ResolveCardPortraitRequest(card);
			if (cardPortraitRequest == null || !CardPortraitCache.TryGetValue(cardPortraitRequest.CacheKey, out Texture2D value) || !GodotObject.IsInstanceValid((GodotObject)(object)value) || ((GodotObject)value).GetInstanceId() != ((GodotObject)texture).GetInstanceId())
			{
				return null;
			}
			if (!ExternalCardProviderIdentityPaths.TryGetValue(cardPortraitRequest.CacheKey, out string value2))
			{
				value2 = ExternalCardProviderIdentityPolicy.BuildSyntheticPath(((object)((AbstractModel)card).Id).ToString(), cardPortraitRequest.CacheKey);
				ExternalCardProviderIdentityPaths[cardPortraitRequest.CacheKey] = value2;
			}
			return value2;
		}
	}

	public static void PreloadCardPortraits(IEnumerable<CardModel> cards)
	{
		lock (Sync)
		{
			CardPortraitRequest[] array = (from request in cards.Select(ResolveCardPortraitRequest)
				where request != null
				select request).Cast<CardPortraitRequest>().DistinctBy<CardPortraitRequest, string>((CardPortraitRequest request) => request.CacheKey, StringComparer.OrdinalIgnoreCase).ToArray();
			foreach (IGrouping<string, CardPortraitRequest> item in array.GroupBy<CardPortraitRequest, string>((CardPortraitRequest request) => request.OverlayKey, StringComparer.OrdinalIgnoreCase))
			{
				CardPortraitRequest cardPortraitRequest = item.First();
				try
				{
					EnsureIsolatedCardOverlay(cardPortraitRequest.GroupId, cardPortraitRequest.Selection, cardPortraitRequest.UseSelectedProvider, item.Select((CardPortraitRequest request) => request.ResourcePath));
				}
				catch
				{
				}
			}
			CardPortraitRequest[] array2 = array;
			foreach (CardPortraitRequest request2 in array2)
			{
				try
				{
					GetOrLoadCardPortrait(request2);
				}
				catch
				{
				}
			}
		}
	}

	private static CardPortraitRequest? ResolveCardPortraitRequest(CardModel card)
	{
		CardLookup cardLookup = GetCardLookup(card);
		if (cardLookup.Group == null)
		{
			return null;
		}
		string effectiveCardSelection = GetEffectiveCardSelection(card, cardLookup);
		if (effectiveCardSelection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			if (cardLookup.Options.Count == 0)
			{
				return null;
			}
			if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out string path))
			{
				return null;
			}
			return new CardPortraitRequest(cardLookup.GroupId, effectiveCardSelection, path, $"{cardLookup.GroupId}\n{effectiveCardSelection}\npck\n{path}", UseSelectedProvider: false, WrapAtlas: false);
		}
		if (!cardLookup.OptionsById.TryGetValue(effectiveCardSelection, out CardOptionLookup value))
		{
			return null;
		}
		string text = value.Option.GetPortraitPath(cardLookup.CardType, IsAncientStyleEnabled(value.Option, cardLookup.CardType, card));
		if (value.Option.StatefulArt != null)
		{
			StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(value.Option);
			if (runtime != null)
			{
				if (!runtime.Enabled && FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out string path2))
				{
					return new CardPortraitRequest(cardLookup.GroupId, "__base__", path2, $"{cardLookup.GroupId}\n{effectiveCardSelection}\nstateful-disabled\n{path2}", UseSelectedProvider: false, WrapAtlas: false);
				}
				text = runtime.Portrait(card, text) ?? text;
			}
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			return new CardPortraitRequest(cardLookup.GroupId, effectiveCardSelection, text, $"{cardLookup.GroupId}\n{effectiveCardSelection}\nconfig\n{text}", UseSelectedProvider: true, WrapAtlas: true);
		}
		if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out string path3))
		{
			return null;
		}
		string text2 = SelectProviderCardPath(value.MatchedAssetPaths, card, path3);
		if (text2 != null)
		{
			return new CardPortraitRequest(cardLookup.GroupId, effectiveCardSelection, text2, $"{cardLookup.GroupId}\n{effectiveCardSelection}\npck\n{text2}", UseSelectedProvider: true, WrapAtlas: false);
		}
		return new CardPortraitRequest(cardLookup.GroupId, "__base__", path3, $"{cardLookup.GroupId}\n{effectiveCardSelection}\nbase-fallback\n{path3}", UseSelectedProvider: false, WrapAtlas: false);
	}

	private static Texture2D? GetOrLoadCardPortrait(CardPortraitRequest request)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (CardPortraitCache.TryGetValue(request.CacheKey, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		if (FailedCardPortraitRequests.Contains(request.CacheKey))
		{
			return null;
		}
		try
		{
			Texture2D val = LoadIsolatedCardPortrait(request.GroupId, request.Selection, request.ResourcePath, request.UseSelectedProvider);
			if (val == null)
			{
				throw new InvalidOperationException("资源包未返回可用卡牌贴图。");
			}
			Texture2D val2 = (Texture2D)((!request.WrapAtlas || val is AtlasTexture || val is ImageTexture) ? ((object)val) : ((object)new AtlasTexture
			{
				Atlas = val,
				Region = new Rect2(0f, 0f, (float)val.GetWidth(), (float)val.GetHeight())
			}));
			if (CardPortraitCache.Set(request.CacheKey, val2, out KeyValuePair<string, Texture2D> evicted))
			{
				ExternalCardProviderIdentityPaths.Remove(evicted.Key);
			}
			return val2;
		}
		catch (Exception ex)
		{
			if (FailedCardPortraitRequests.Add(request.CacheKey))
			{
				ModLog.Warn($"卡牌皮肤资源不可用，已回退原版卡图：{request.GroupId}/{request.Selection}/{request.ResourcePath}；原因={ex.GetBaseException().Message}");
			}
			return null;
		}
	}

	private static int CardArtSelectionScore(string assetPath, CardModel card, string originalPath)
	{
		int num = (HasSameResourceExtension(assetPath, originalPath) ? 20 : 0);
		bool num2 = assetPath.Contains("/beta/", StringComparison.OrdinalIgnoreCase);
		bool flag = originalPath.Contains("/beta/", StringComparison.OrdinalIgnoreCase);
		if (num2 == flag)
		{
			num += 100;
		}
		(string, string)? tuple = CardPortraitIdentity(assetPath);
		(string, string)? tuple2 = CardPortraitIdentity(originalPath);
		string text = NormalizeCardToken(((object)card).GetType().Name);
		string text2 = tuple2?.Item2 ?? text;
		if ((tuple.HasValue && tuple.GetValueOrDefault().Item2.Equals(text2, StringComparison.OrdinalIgnoreCase)) || (tuple.HasValue && tuple.GetValueOrDefault().Item2.Equals(text, StringComparison.OrdinalIgnoreCase)))
		{
			num += 30;
		}
		int cardPresentationLevel = GetCardPresentationLevel(card);
		foreach (string item2 in new string[2] { text2, text }.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (tuple.HasValue && tuple.Value.Item2.StartsWith(item2, StringComparison.OrdinalIgnoreCase))
			{
				string item = tuple.Value.Item2;
				int length = item2.Length;
				if (int.TryParse(item.Substring(length, item.Length - length), out var result))
				{
					num += ((result == cardPresentationLevel + 1) ? 40 : Math.Max(0, 10 - Math.Abs(result - cardPresentationLevel - 1)));
					break;
				}
			}
		}
		return num;
	}

	private static int GetCardPresentationLevel(CardModel card)
	{
		string[] array = new string[2] { "FakeUpgradeLevel", "CurrentUpgradeLevel" };
		foreach (string text in array)
		{
			try
			{
				if (!(AccessTools.Property(((object)card).GetType(), text)?.GetValue(card) is int val))
				{
					continue;
				}
				return Math.Max(0, val);
			}
			catch
			{
			}
		}
		return 0;
	}

	public static bool CardBelongsToGroup(CardModel card, string groupId)
	{
		lock (Sync)
		{
			return GetCardLookup(card).PriorityGroups.Any((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		}
	}

	private static CardLookup GetCardLookup(CardModel card)
	{
		CardLookup value = _cardLookupCache.GetValue(card, ResolveCardLookup);
		if (value.BaselineUnavailable)
		{
			_cardLookupCache.Remove(card);
		}
		return value;
	}

	private static CardLookup ResolveCardLookup(CardModel card)
	{
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Invalid comparison between Unknown and I4
		string poolGroupId = GetCardPoolGroupId(card);
		if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out string _))
		{
			return new CardLookup(poolGroupId, ((object)card).GetType().Name, null, Array.Empty<CardSkinOption>(), new Dictionary<string, CardOptionLookup>(StringComparer.OrdinalIgnoreCase), Array.Empty<CardSkinGroup>())
			{
				BaselineUnavailable = true
			};
		}
		string cardFilterGroupId = GetCardFilterGroupId(card);
		if (!cardFilterGroupId.Equals(poolGroupId, StringComparison.OrdinalIgnoreCase))
		{
			if (cardFilterGroupId.Equals("ancients", StringComparison.OrdinalIgnoreCase))
			{
				SkinCatalog? catalog = Catalog;
				if (catalog != null && catalog.CardGroups.Any((CardSkinGroup group) => group.Id.Equals("ancients", StringComparison.OrdinalIgnoreCase)))
				{
					goto IL_00d3;
				}
			}
			if (CardGroupAffectsCardUncached(cardFilterGroupId, card))
			{
				goto IL_00d3;
			}
		}
		string cardType = ((object)card).GetType().Name;
		CardSkinGroup cardSkinGroup = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup group) => group.Options.Any((CardSkinOption option) => option.NormalPortraits.ContainsKey(cardType) || option.AncientPortraits.ContainsKey(cardType)));
		string groupId;
		if (cardSkinGroup != null)
		{
			groupId = cardSkinGroup.Id;
		}
		else if (CardGroupAffectsCardUncached(poolGroupId, card))
		{
			groupId = poolGroupId;
		}
		else
		{
			string cardCatalogGroupId = GetCardCatalogGroupId(card);
			groupId = (CardGroupAffectsCardUncached(cardCatalogGroupId, card) ? cardCatalogGroupId : poolGroupId);
		}
		goto IL_0182;
		IL_00d3:
		groupId = cardFilterGroupId;
		goto IL_0182;
		IL_0182:
		CardSkinGroup cardSkinGroup2 = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		CardOptionLookup[] source = (from option in cardSkinGroup2?.Options
			select BuildCardOptionLookup(option, card) into option
			where option != null
			select option).Cast<CardOptionLookup>().ToArray() ?? Array.Empty<CardOptionLookup>();
		List<CardSkinGroup> list = new List<CardSkinGroup>();
		if ((int)card.Rarity == 5 && IsCharacterCardPool(card))
		{
			CardSkinGroup cardSkinGroup3 = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(poolGroupId, StringComparison.OrdinalIgnoreCase));
			if (cardSkinGroup3 != null)
			{
				list.Add(cardSkinGroup3);
			}
		}
		if (cardSkinGroup2 != null && !list.Contains(cardSkinGroup2))
		{
			list.Add(cardSkinGroup2);
		}
		return new CardLookup(groupId, ((object)card).GetType().Name, cardSkinGroup2, source.Select((CardOptionLookup option) => option.Option).ToArray(), source.ToDictionary<CardOptionLookup, string>((CardOptionLookup option) => option.Option.Id, StringComparer.OrdinalIgnoreCase), list);
	}

	private static bool CardGroupAffectsCardUncached(string groupId, CardModel card)
	{
		return (Catalog?.CardGroups.FirstOrDefault((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)))?.Options.Any((CardSkinOption option) => CardOptionAffectsCardUncached(option, card)) ?? false;
	}

	private static CardSkinGroup? GetCardGroup(CardModel card)
	{
		return GetCardLookup(card).Group;
	}

	private static CardOptionLookup? BuildCardOptionLookup(CardSkinOption option, CardModel card)
	{
		string name = ((object)card).GetType().Name;
		string nameForCard = option.GetNameForCard(name);
		if (!nameForCard.Equals(option.Name, StringComparison.Ordinal))
		{
			option = option with
			{
				Name = nameForCard
			};
		}
		if (option.NormalPortraits.ContainsKey(name) || option.AncientPortraits.ContainsKey(name))
		{
			return new CardOptionLookup(option, Array.Empty<string>());
		}
		string[] array = option.Assets.Keys.Where((string assetPath) => CardArtMatches(assetPath, card)).ToArray();
		if (!option.CardPresentations.ContainsKey(name) && array.Length == 0)
		{
			return null;
		}
		return new CardOptionLookup(option, array);
	}

	private static bool CardOptionAffectsCardUncached(CardSkinOption option, CardModel card)
	{
		return BuildCardOptionLookup(option, card) != null;
	}

	private static CardCoverageState GetCardCoverage(CardSkinGroup group)
	{
		if (CardCoverageCache.TryGetValue(group.Id, out CardCoverageState value))
		{
			return value;
		}
		CardLookup[] cards = (from card in ModelDb.AllCards
			select GetCardLookup(card) into lookup
			where lookup.PriorityGroups.Any((CardSkinGroup member) => member.Id.Equals(@group.Id, StringComparison.OrdinalIgnoreCase))
			select lookup).ToArray();
		Dictionary<string, int> byOption = group.Options.ToDictionary<CardSkinOption, string, int>((CardSkinOption option) => option.Id, (CardSkinOption option) => cards.Count((CardLookup lookup) => lookup.OptionsById.ContainsKey(option.Id)), StringComparer.OrdinalIgnoreCase);
		value = new CardCoverageState(cards.Length, byOption);
		CardCoverageCache[group.Id] = value;
		return value;
	}

	private static string GetCardPoolGroupId(CardModel card)
	{
		CardPoolModel pool = card.Pool;
		return (((pool != null) ? pool.Title : null) ?? string.Empty).ToLowerInvariant();
	}

	private static bool IsCharacterCardPool(CardModel card)
	{
		CardPoolModel pool = card.Pool;
		if (pool != null && !pool.IsColorless)
		{
			return !SharedCardPoolIds.Contains(GetCardPoolGroupId(card));
		}
		return false;
	}

	private static string GetCardCatalogGroupId(CardModel card)
	{
		string cardPoolGroupId = GetCardPoolGroupId(card);
		if (!SharedCardPoolIds.Contains(cardPoolGroupId))
		{
			return cardPoolGroupId;
		}
		return GetCardFilterGroupId(card);
	}

	private static string GetCardFilterGroupId(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if ((int)card.Rarity == 5)
		{
			return "ancients";
		}
		if (!MiscCardRarities.Contains(card.Rarity))
		{
			return GetCardPoolGroupId(card);
		}
		return "misc";
	}

	private static Texture2D? LoadIsolatedCardPortrait(string groupId, string selection, string resourcePath, bool useSelectedProvider)
	{
		SkinCatalog catalog = Catalog;
		if (catalog != null && catalog.TryReadCardImageBytes(groupId, selection, resourcePath, useSelectedProvider, out byte[] bytes))
		{
			Image val = LoadRasterImageFromBuffer(bytes, resourcePath);
			try
			{
				return (Texture2D?)(object)ImageTexture.CreateFromImage(val);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		IsolatedCardOverlayState isolatedCardOverlayState = EnsureIsolatedCardOverlay(groupId, selection, useSelectedProvider, new _003C_003Ez__ReadOnlySingleElementList<string>(resourcePath));
		if (!isolatedCardOverlayState.ResourcePaths.TryGetValue(resourcePath, out string value))
		{
			return null;
		}
		Texture2D val2 = ResourceLoader.Load<Texture2D>(value, (string)null, (CacheMode)(isolatedCardOverlayState.CanReuseExternalDependencies ? 0 : 3));
		if (val2 != null)
		{
			return val2;
		}
		return null;
	}

	private static Image LoadRasterImageFromBuffer(byte[] bytes, string imagePath)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		if (bytes.Length == 0)
		{
			throw new InvalidOperationException("独立卡牌图片为空：" + imagePath);
		}
		Image val = new Image();
		Error val2;
		if (HasPrefix(bytes, new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 }))
		{
			val2 = val.LoadPngFromBuffer(bytes);
		}
		else if (HasPrefix(bytes, new byte[3] { 255, 216, 255 }))
		{
			val2 = val.LoadJpgFromBuffer(bytes);
		}
		else
		{
			if (bytes.Length < 12 || !HasPrefix(bytes, "RIFF"u8) || !bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
			{
				((GodotObject)val).Dispose();
				throw new InvalidOperationException("无法识别独立卡牌图片格式：" + imagePath);
			}
			val2 = val.LoadWebpFromBuffer(bytes);
		}
		if ((int)val2 != 0 || val.IsEmpty())
		{
			((GodotObject)val).Dispose();
			throw new InvalidOperationException($"无法解码独立卡牌图片（{val2}）：{imagePath}");
		}
		return val;
		static bool HasPrefix(byte[] value, ReadOnlySpan<byte> prefix)
		{
			if (value.Length >= prefix.Length)
			{
				return value.AsSpan(0, prefix.Length).SequenceEqual(prefix);
			}
			return false;
		}
	}

	private static IsolatedCardOverlayState EnsureIsolatedCardOverlay(string groupId, string selection, bool useSelectedProvider, IEnumerable<string> resourcePaths)
	{
		SkinCatalog skinCatalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
		string text = (useSelectedProvider ? "provider" : "base");
		string text2 = (useSelectedProvider ? $"{groupId}\n{selection}\n{text}" : ("__base__\n" + text));
		if (!IsolatedCardOverlayCache.TryGetValue(text2, out IsolatedCardOverlayState state))
		{
			state = new IsolatedCardOverlayState(text2);
			IsolatedCardOverlayCache.Add(text2, state);
		}
		ReactivateIsolatedCardOverlayIfNeeded(state);
		string[] array = (from path in resourcePaths.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			where !state.ResourcePaths.ContainsKey(path) && !state.UnavailablePaths.Contains(path)
			select path).ToArray();
		if (array.Length == 0)
		{
			return state;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		RuntimeResourceOverlay runtimeResourceOverlay;
		try
		{
			int value = ++_overlayGeneration;
			IsolatedCardOverlayState isolatedCardOverlayState = state;
			if (isolatedCardOverlayState.AliasToken == null)
			{
				string text4 = (isolatedCardOverlayState.AliasToken = $"{_sessionId}/{value:D3}_card_{text}");
			}
			runtimeResourceOverlay = skinCatalog.BuildIsolatedCardResources(groupId, selection, array, useSelectedProvider, state.AliasToken, state.ResourcePaths);
			Dictionary<string, byte[]> newFiles = runtimeResourceOverlay.Files.Where<KeyValuePair<string, byte[]>>((KeyValuePair<string, byte[]> pair) => !state.MountedFilePaths.Contains(pair.Key)).ToDictionary<KeyValuePair<string, byte[]>, string, byte[]>((KeyValuePair<string, byte[]> pair) => pair.Key, (KeyValuePair<string, byte[]> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
			if (newFiles.Count == 0)
			{
				throw new InvalidOperationException("独立卡牌资源包没有产生新的可挂载文件。");
			}
			string overlayPath = Path.Combine(SkinChangerPaths.CacheDirectory, $"sts2_skin_overlay_{_sessionId}_{value:D3}_card_{text}.pck");
			SkinChangerPaths.WriteCache(overlayPath, delegate
			{
				PckArchive.Write(overlayPath, newFiles);
			});
			if (!ProjectSettings.LoadResourcePack(overlayPath, true, 0))
			{
				throw new InvalidOperationException("Godot 拒绝加载批量独立卡牌资源包。");
			}
			state.OverlayPaths.Add(overlayPath);
			state.MountedFilePaths.UnionWith(newFiles.Keys);
			state.CanonicalRedirectPaths.UnionWith(runtimeResourceOverlay.CanonicalDependencyPaths);
			state.CanReuseExternalDependencies &= runtimeResourceOverlay.CanReuseExternalDependencies;
			CardCanonicalResourceOwners.MarkActivated(state.OwnerId, runtimeResourceOverlay.CanonicalDependencyPaths);
			stopwatch.Stop();
			if (array.Length > 1 || stopwatch.ElapsedMilliseconds >= 100)
			{
				ModLog.Info($"已批量隔离 {groupId}/{selection} 的 {runtimeResourceOverlay.ResourcePaths.Count} 个卡牌资源（{text}，请求 {array.Length} 个），耗时={stopwatch.Elapsed.TotalMilliseconds:F1} ms。");
			}
		}
		catch
		{
			throw;
		}
		foreach (KeyValuePair<string, string> resourcePath in runtimeResourceOverlay.ResourcePaths)
		{
			state.ResourcePaths[resourcePath.Key] = resourcePath.Value;
		}
		string[] array2 = array;
		foreach (string text5 in array2)
		{
			if (!runtimeResourceOverlay.ResourcePaths.ContainsKey(text5))
			{
				state.UnavailablePaths.Add(text5);
			}
		}
		return state;
	}

	private static void ReactivateIsolatedCardOverlayIfNeeded(IsolatedCardOverlayState state)
	{
		if (state.OverlayPaths.Count == 0 || !CardCanonicalResourceOwners.RequiresActivation(state.OwnerId, state.CanonicalRedirectPaths))
		{
			return;
		}
		foreach (string overlayPath in state.OverlayPaths)
		{
			if (!File.Exists(overlayPath) || !ProjectSettings.LoadResourcePack(overlayPath, true, 0))
			{
				throw new InvalidOperationException("Godot 拒绝重新激活已缓存的独立卡牌资源包。");
			}
		}
		CardCanonicalResourceOwners.MarkActivated(state.OwnerId, state.CanonicalRedirectPaths);
	}

	private static string? SelectProviderCardPath(IReadOnlyList<string> paths, CardModel card, string originalPath)
	{
		string text = null;
		int num = int.MinValue;
		foreach (string path in paths)
		{
			int num2 = CardArtSelectionScore(path, card, originalPath);
			if (num2 > num || (num2 == num && string.Compare(path, text, StringComparison.OrdinalIgnoreCase) < 0))
			{
				text = path;
				num = num2;
			}
		}
		return text;
	}

	private static bool CardArtMatches(string assetPath, CardModel card)
	{
		if (!FrameworkCardVisualGuard.TryGetBaselinePortraitPath(card, out string path))
		{
			return false;
		}
		(string, string)? tuple = CardPortraitIdentity(path);
		string cardPoolGroupId = GetCardPoolGroupId(card);
		(string, string)? tuple2 = CardPortraitIdentity(assetPath, cardPoolGroupId, tuple?.Item1);
		if (!tuple2.HasValue || !tuple.HasValue)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(tuple2.Value.Item1) && !tuple2.Value.Item1.Equals(cardPoolGroupId, StringComparison.OrdinalIgnoreCase) && !tuple2.Value.Item1.Equals(tuple.Value.Item1, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string expected = NormalizeCardToken(((object)card).GetType().Name);
		if (!CardStemsMatch(tuple2.Value.Item2, tuple.Value.Item2))
		{
			return CardStemsMatch(tuple2.Value.Item2, expected);
		}
		return true;
	}

	private static (string Category, string Stem)? CardPortraitIdentity(string? path, string? expectedCategory = null, string? alternateCategory = null)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}
		string text = path.ToLowerInvariant();
		int num = -1;
		int num2 = 0;
		string[] array = new string[6] { "/card_portraits/", "/card_atlas.sprites/", "/cards/", "/card/", "/card_art/", "/cardart/" };
		foreach (string text2 in array)
		{
			num = text.IndexOf(text2, StringComparison.Ordinal);
			if (num >= 0)
			{
				num2 = text2.Length;
				break;
			}
		}
		string item = string.Empty;
		int i;
		if (num >= 0)
		{
			int num3 = num + num2;
			int num4 = text.LastIndexOf('/');
			if (num4 >= num3)
			{
				i = num3;
				string[] array2 = text.Substring(i, num4 - i).Split('/', StringSplitOptions.RemoveEmptyEntries);
				int num5 = Array.FindIndex(array2, (string candidate) => (!string.IsNullOrWhiteSpace(expectedCategory) && candidate.Equals(expectedCategory, StringComparison.OrdinalIgnoreCase)) || (!string.IsNullOrWhiteSpace(alternateCategory) && candidate.Equals(alternateCategory, StringComparison.OrdinalIgnoreCase)));
				if (num5 < 0 && array2.Length != 0)
				{
					num5 = 0;
				}
				if (num5 >= 0)
				{
					item = array2[num5];
				}
			}
		}
		string text3 = text;
		i = text.LastIndexOf('/') + 1;
		string text4 = text3.Substring(i, text3.Length - i);
		int num6 = text4.LastIndexOf('.');
		string text5 = ((num6 >= 0) ? text4.Substring(0, num6) : text4);
		int num7 = text5.LastIndexOf('.');
		if (num7 >= 0)
		{
			text3 = text5;
			i = num7 + 1;
			text5 = text3.Substring(i, text3.Length - i);
		}
		array = new string[5] { "_card_art", "-card-art", " card art", "card_art", "cardart" };
		foreach (string text6 in array)
		{
			if (text5.EndsWith(text6, StringComparison.OrdinalIgnoreCase))
			{
				text3 = text5;
				int length = text6.Length;
				text5 = text3.Substring(0, text3.Length - length);
				break;
			}
		}
		string item2 = NormalizeCardToken(text5);
		return (item, item2);
	}

	private static bool CardStemsMatch(string candidate, string expected)
	{
		if (!candidate.Equals(expected, StringComparison.OrdinalIgnoreCase) && !candidate.Equals(expected + "ancient", StringComparison.OrdinalIgnoreCase) && !candidate.Equals(expected + "normal", StringComparison.OrdinalIgnoreCase) && !candidate.Equals(expected + "portrait", StringComparison.OrdinalIgnoreCase) && !candidate.Equals(expected + "art", StringComparison.OrdinalIgnoreCase))
		{
			return IsNumberedCardVariant(candidate, expected);
		}
		return true;
	}

	private static bool IsNumberedCardVariant(string candidate, string expected)
	{
		if (candidate.StartsWith(expected, StringComparison.OrdinalIgnoreCase) && candidate.Length > expected.Length)
		{
			int length = expected.Length;
			return candidate.Substring(length, candidate.Length - length).All(char.IsDigit);
		}
		return false;
	}

	private static string NormalizeCardToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	private static bool HasSameResourceExtension(string left, string right)
	{
		string? extension = Path.GetExtension(left);
		string extension2 = Path.GetExtension(right);
		return extension.Equals(extension2, StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsAncientStyleEnabled(CardSkinOption option, string cardType, CardModel card)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Invalid comparison between Unknown and I4
		if (!option.AncientPortraits.ContainsKey(cardType))
		{
			return false;
		}
		if (!AncientStyleMethods.TryGetValue(option.Id, out MethodInfo value) && !MissingAncientStyleMethods.Contains(option.Id))
		{
			value = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
				where assembly.GetName().Name?.Equals(option.ProviderId ?? option.Id, StringComparison.OrdinalIgnoreCase) ?? false
				select assembly).Select(AncientStyleMethodPolicy.Find).FirstOrDefault((MethodInfo candidate) => candidate != null);
			if (value == null)
			{
				MissingAncientStyleMethods.Add(option.Id);
			}
			else
			{
				AncientStyleMethods[option.Id] = value;
			}
		}
		if (value == null)
		{
			bool requestsAncientLayout = option.CardPresentations.GetValueOrDefault(cardType)?.UseAncientLayout ?? false;
			return AncientStyleMethodPolicy.ResolveWithoutProviderMethod((int)card.Rarity == 5, requestsAncientLayout);
		}
		try
		{
			return (value.Invoke(null, new object[1] { cardType }) as bool?) ?? true;
		}
		catch (Exception ex)
		{
			if (FailedAncientStyleMethods.Add(option.Id))
			{
				ModLog.Warn("读取 " + option.Id + " 的先古卡图样式设置失败：" + ex.Message);
			}
			return true;
		}
	}

	public static PackedScene LoadRuntimeScene(string groupId, string scenePath)
	{
		IReadOnlyList<string> resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
		Resource? valueOrDefault = LoadRuntimeResources(groupId, resourcePaths, includeProviderDependencies: true).GetValueOrDefault(scenePath);
		return (PackedScene)(object)(((valueOrDefault is PackedScene) ? valueOrDefault : null) ?? throw new InvalidOperationException("独立皮肤资源不是场景：" + scenePath));
	}

	public static PackedScene GetOrLoadRuntimeScene(string groupId, string scenePath)
	{
		IReadOnlyList<string> resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
		Resource? valueOrDefault = LoadRuntimeResources(groupId, resourcePaths, includeProviderDependencies: true).GetValueOrDefault(scenePath);
		return (PackedScene)(object)(((valueOrDefault is PackedScene) ? valueOrDefault : null) ?? throw new InvalidOperationException("独立皮肤资源不是场景：" + scenePath));
	}

	public static T InstantiateRuntimeScene<T>(string groupId, string scenePath, Action? beforeInstantiate = null) where T : Node
	{
		IReadOnlyList<string> resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
		return WithRuntimeResources(groupId, resourcePaths, delegate(IReadOnlyDictionary<string, Resource> resources)
		{
			beforeInstantiate?.Invoke();
			Resource? valueOrDefault = resources.GetValueOrDefault(scenePath);
			return ((PackedScene)(((valueOrDefault is PackedScene) ? valueOrDefault : null) ?? throw new InvalidOperationException("独立皮肤资源不是场景：" + scenePath))).Instantiate<T>((GenEditState)0);
		}, includeProviderDependencies: true);
	}

	public static bool SelectionServesCombatScene(string groupId, string scenePath)
	{
		lock (Sync)
		{
			SkinOption skinOption = FindVisualOption(groupId, GetVisualSelection(groupId));
			return CharacterCombatSceneInstantiationPolicy.ShouldOwnCanonicalSceneInstantiation(skinOption == null || skinOption.Id.Equals("__base__", StringComparison.OrdinalIgnoreCase), skinOption?.Assets.Keys.Any((string path) => path.Equals(scenePath, StringComparison.OrdinalIgnoreCase)) ?? false);
		}
	}

	private static SkinOption? FindVisualOption(string groupId, string selection)
	{
		return Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
	}

	public static bool TryInstantiateSelectedCharacterCreatureVisuals(string groupId, string scenePath, Func<NCreatureVisuals, NCreatureVisuals>? configureVisuals, out NCreatureVisuals visuals)
	{
		lock (Sync)
		{
			SkinOption skinOption = FindVisualOption(groupId, GetVisualSelection(groupId));
			bool hasManagedCombatScene = ((object)skinOption != null && skinOption.FrameworkContract?.CharacterResources.ContainsKey("CombatVisual") == true) || (skinOption?.Assets.Keys.Any((string path) => path.Equals(scenePath, StringComparison.OrdinalIgnoreCase)) ?? false);
			bool hasManagedCombatDependencies = skinOption != null && CharacterCombatSceneInstantiationPolicy.HasManagedCombatDependencies(scenePath, skinOption.Assets.Keys);
			if (!CharacterCombatSceneInstantiationPolicy.ShouldUseManagedFactory(skinOption == null || skinOption.Id.Equals("__base__", StringComparison.OrdinalIgnoreCase), hasManagedCombatScene, hasManagedCombatDependencies))
			{
				visuals = null;
				return false;
			}
		}
		visuals = InstantiateManagedCharacterCreatureVisuals(groupId, scenePath, configureVisuals);
		return true;
	}

	public static NCreatureVisuals InstantiateManagedCharacterCreatureVisuals(string groupId, string scenePath, Func<NCreatureVisuals, NCreatureVisuals>? configureVisuals = null)
	{
		IReadOnlyList<string> resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
		return WithRuntimeResources(groupId, resourcePaths, delegate(IReadOnlyDictionary<string, Resource> resources)
		{
			Resource? valueOrDefault = resources.GetValueOrDefault(scenePath);
			NCreatureVisuals val = FrameworkCreatureSceneFactory.Create((PackedScene)(object)(((valueOrDefault is PackedScene) ? valueOrDefault : null) ?? throw new InvalidOperationException("角色皮肤资源不是场景：" + scenePath)));
			return configureVisuals?.Invoke(val) ?? val;
		}, includeProviderDependencies: true, takeOverCanonicalPaths: true);
	}

	public static IDisposable BeginRuntimeResourceScope(string groupId, string scenePath)
	{
		IReadOnlyList<string> resourcePaths = RuntimeSceneResourcePaths(groupId, scenePath);
		lock (Sync)
		{
			SkinCatalog skinCatalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
			string visualSelection = GetVisualSelection(groupId);
			PreparedRuntimeOverlay orPrepareRuntimeOverlay = GetOrPrepareRuntimeOverlay(skinCatalog, groupId, visualSelection, resourcePaths, includeProviderDependencies: true);
			MountContextualFullRuntimeProviderPacks(skinCatalog, visualSelection);
			if (orPrepareRuntimeOverlay.OverlayPath != null && !ProjectSettings.LoadResourcePack(orPrepareRuntimeOverlay.OverlayPath, true, 0))
			{
				throw new InvalidOperationException("Godot 拒绝加载已准备的独立皮肤场景资源包。");
			}
			if (orPrepareRuntimeOverlay.CanonicalDependencyPaths.Count > 0)
			{
				if (!RuntimeCanonicalDependencyPaths.TryGetValue(groupId, out HashSet<string> value))
				{
					value = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
					RuntimeCanonicalDependencyPaths[groupId] = value;
				}
				value.UnionWith(orPrepareRuntimeOverlay.CanonicalDependencyPaths);
			}
			if (skinCatalog.IsRuntimeProviderOption(groupId, visualSelection) && skinCatalog.ProviderUsesManagedGodotScripts(visualSelection))
			{
				ManagedSkinModLoader.EnsureProviderGodotScripts(skinCatalog.ResolveVisualProviderId(visualSelection));
			}
			return new MountedRuntimeResourceScope(orPrepareRuntimeOverlay.RestoreGroups.Append(groupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase), ManualCharacterVariantBridge.BeginScope(groupId));
		}
	}

	private static IReadOnlyList<string> RuntimeSceneResourcePaths(string groupId, string scenePath)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			string visualSelection = GetVisualSelection(groupId);
			RuntimeMonsterVisualMode runtimeMonsterVisualMode = catalog?.GetRuntimeMonsterVisualMode(groupId, visualSelection);
			object obj;
			if (catalog == null || !CanReuseMountedLargeProviderPack(catalog, groupId, visualSelection))
			{
				obj = runtimeMonsterVisualMode?.ResourcePaths ?? Array.Empty<string>();
			}
			else
			{
				IReadOnlyList<string> readOnlyList = Array.Empty<string>();
				obj = readOnlyList;
			}
			IEnumerable<string> second = (IEnumerable<string>)obj;
			return new string[1] { scenePath }.Concat(second).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		}
	}

	private static bool CanReuseMountedLargeProviderPack(SkinCatalog catalog, string groupId, string selection)
	{
		if (!Config.GetSelection(groupId).Equals(selection, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		SkinOption skinOption = catalog.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
		if (skinOption != null)
		{
			return catalog.GetProviderResourcePackPaths(skinOption.EffectiveProviderId).Select(Path.GetFullPath).Any(MountedLargeRuntimeProviderPacks.ContainsKey);
		}
		return false;
	}

	public static Resource GetOrLoadRuntimeResource(string groupId, string resourcePath, bool includeProviderDependencies = false)
	{
		lock (Sync)
		{
			string key = RuntimeResourceKey(groupId, resourcePath);
			if (RuntimeResourceCache.TryGetValue(key, out Resource value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				return value;
			}
			RuntimeResourceCache.Remove(key);
			return LoadRuntimeResources(groupId, new _003C_003Ez__ReadOnlySingleElementList<string>(resourcePath), includeProviderDependencies)[resourcePath];
		}
	}

	public static Resource GetOrLoadCharacterIconResource(string groupId, string resourcePath, bool includeProviderDependencies = false)
	{
		lock (Sync)
		{
			string visualSelection = GetVisualSelection(groupId);
			string key = RuntimeResourceKey(groupId, visualSelection, resourcePath);
			if (RuntimeResourceCache.TryGetValue(key, out Resource value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				return value;
			}
			RuntimeResourceCache.Remove(key);
			return LoadRuntimeResourcesForSelection(groupId, visualSelection, new _003C_003Ez__ReadOnlySingleElementList<string>(resourcePath), includeProviderDependencies)[resourcePath];
		}
	}

	public static T WithCharacterIconResource<T>(string groupId, string resourcePath, Func<Resource, T> callback, bool includeProviderDependencies = false)
	{
		ArgumentNullException.ThrowIfNull(callback, "callback");
		lock (Sync)
		{
			string visualSelection = GetVisualSelection(groupId);
			return WithRuntimeResourcesForSelection(groupId, visualSelection, new _003C_003Ez__ReadOnlySingleElementList<string>(resourcePath), (IReadOnlyDictionary<string, Resource> resources) => callback(resources[resourcePath]), includeProviderDependencies, takeOverCanonicalPaths: false);
		}
	}

	internal static bool IsSelectedCharacterStandaloneTexture(string resourcePath)
	{
		lock (Sync)
		{
			return Catalog?.IsSelectedCharacterStandaloneTexture(resourcePath, GetVisualSelections()) ?? false;
		}
	}

	public static Texture2D? GetRelicIconOverride(string resourcePath)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			string text = catalog?.FindSelectedRelicIconGroup(resourcePath, Config.Selections, Config.VisualProviderPriority);
			if (catalog == null)
			{
				return null;
			}
			if (text == null)
			{
				return GetBaselineRelicIcon(catalog, resourcePath);
			}
			string key = RuntimeResourceKey(text, resourcePath);
			if (!RuntimeResourceCache.TryGetValue(key, out Resource value) || !GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				LoadSelectedRelicBundle(catalog, text);
				value = RuntimeResourceCache.GetValueOrDefault(key);
			}
			return (Texture2D?)(object)(((value is Texture2D) ? value : null) ?? throw new InvalidOperationException("隔离的遗物图标不是贴图：" + resourcePath));
		}
	}

	private static Texture2D? GetBaselineRelicIcon(SkinCatalog catalog, string resourcePath)
	{
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		if (BaselineRelicIconCache.TryGetValue(resourcePath, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		BaselineRelicIconCache.Remove(resourcePath);
		string text = catalog.FindRelicIconOwnerGroup(resourcePath);
		if (text == null || !catalog.TryGetBaselineRelicTextureDefinition(resourcePath, out BaselineRelicTextureDefinition definition))
		{
			return null;
		}
		if (!BaselineRelicAtlasCache.TryGetValue(definition.AtlasPath, out Texture2D value2) || !GodotObject.IsInstanceValid((GodotObject)(object)value2))
		{
			BaselineRelicAtlasCache.Remove(definition.AtlasPath);
			PreparedRuntimeOverlay orPrepareRuntimeOverlay = GetOrPrepareRuntimeOverlay(catalog, text, "__base__", new _003C_003Ez__ReadOnlySingleElementList<string>(definition.AtlasPath), includeProviderDependencies: false, isolateRelicCanonicalPaths: true);
			if (orPrepareRuntimeOverlay.OverlayPath != null && !ProjectSettings.LoadResourcePack(orPrepareRuntimeOverlay.OverlayPath, true, 0))
			{
				throw new InvalidOperationException("Godot 拒绝加载原版遗物私有图集。");
			}
			if (!orPrepareRuntimeOverlay.ResourcePaths.TryGetValue(definition.AtlasPath, out string value3))
			{
				throw new InvalidOperationException("无法隔离原版遗物图集：" + definition.AtlasPath);
			}
			value2 = ResourceLoader.Load<Texture2D>(value3, (string)null, (CacheMode)3) ?? throw new InvalidOperationException("无法加载原版遗物私有图集：" + value3);
			BaselineRelicAtlasCache[definition.AtlasPath] = value2;
		}
		AtlasTexture val = new AtlasTexture
		{
			Atlas = value2,
			Region = new Rect2(definition.Region.X, definition.Region.Y, definition.Region.Width, definition.Region.Height),
			Margin = new Rect2(definition.Margin.X, definition.Margin.Y, definition.Margin.Width, definition.Margin.Height),
			FilterClip = definition.FilterClip
		};
		BaselineRelicIconCache[resourcePath] = (Texture2D)(object)val;
		return (Texture2D?)(object)val;
	}

	private static void LoadSelectedRelicBundle(SkinCatalog catalog, string groupId)
	{
		SkinGroup skinGroup = catalog.Groups.First((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		string selection = Config.GetSelection(groupId);
		SkinOption selected = skinGroup.Options.First((SkinOption option) => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
		IReadOnlyList<string> providerRelicSpritePaths = catalog.GetProviderRelicSpritePaths(selected);
		if (providerRelicSpritePaths.Count == 0)
		{
			return;
		}
		long timestamp = Stopwatch.GetTimestamp();
		PreparedRuntimeOverlay orPrepareRuntimeOverlay = GetOrPrepareRuntimeOverlay(catalog, groupId, selection, providerRelicSpritePaths, includeProviderDependencies: false, isolateRelicCanonicalPaths: true);
		if (orPrepareRuntimeOverlay.OverlayPath != null && !ProjectSettings.LoadResourcePack(orPrepareRuntimeOverlay.OverlayPath, true, 0))
		{
			throw new InvalidOperationException("Godot 拒绝加载遗物皮肤资源包。");
		}
		Dictionary<string, Resource> dictionary = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
		try
		{
			Dictionary<string, Texture2D> source = orPrepareRuntimeOverlay.ResourcePaths.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => SkinCatalog.IsRelicAtlasTexturePath(pair.Key)).ToDictionary<KeyValuePair<string, string>, string, Texture2D>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => ResourceLoader.Load<Texture2D>(pair.Value, (string)null, (CacheMode)3) ?? throw new InvalidOperationException("无法加载遗物皮肤私有图集：" + pair.Value), StringComparer.OrdinalIgnoreCase);
			Texture2D value = source.FirstOrDefault((KeyValuePair<string, Texture2D> pair) => !pair.Key.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase)).Value;
			Texture2D value2 = source.FirstOrDefault((KeyValuePair<string, Texture2D> pair) => pair.Key.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase)).Value;
			bool num = providerRelicSpritePaths.Any((string path) => !path.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase));
			bool flag = providerRelicSpritePaths.Any((string path) => path.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase));
			if ((num && value == null) || (flag && value2 == null))
			{
				throw new InvalidOperationException("遗物皮肤缺少其切片引用的图集。");
			}
			foreach (string item in providerRelicSpritePaths)
			{
				if (orPrepareRuntimeOverlay.ResourcePaths.TryGetValue(item, out string value3))
				{
					AtlasTexture val = ResourceLoader.Load<AtlasTexture>(value3, (string)null, (CacheMode)0) ?? throw new InvalidOperationException("无法加载遗物皮肤切片：" + value3);
					val.Atlas = (item.Contains("relic_outline_atlas", StringComparison.OrdinalIgnoreCase) ? value2 : value);
					dictionary[item] = (Resource)(object)val;
				}
			}
		}
		finally
		{
			MountOverlay(orPrepareRuntimeOverlay.RestoreGroups.Append(groupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase));
		}
		foreach (KeyValuePair<string, Resource> item2 in dictionary)
		{
			RuntimeResourceCache[RuntimeResourceKey(groupId, item2.Key)] = item2.Value;
		}
		ModLog.Info($"已一次性加载 {groupId} 的 {dictionary.Count} 个遗物皮肤切片；运行包={orPrepareRuntimeOverlay.FileCount} 个文件/{(double)orPrepareRuntimeOverlay.FileSize / 1024.0:F1} KiB，耗时={Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds:F1} ms。");
	}

	public static bool IsRuntimeProviderSelected(string groupId)
	{
		lock (Sync)
		{
			return Catalog?.IsRuntimeProviderOption(groupId, GetVisualSelection(groupId)) ?? false;
		}
	}

	internal static bool IsDirectCharacterRuntimeProviderSelected(string providerId, string characterTypeName)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null || string.IsNullOrWhiteSpace(characterTypeName))
			{
				return true;
			}
			SkinGroup skinGroup = catalog.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(characterTypeName, StringComparison.OrdinalIgnoreCase) || candidate.DisplayName.Equals(characterTypeName, StringComparison.OrdinalIgnoreCase));
			if (skinGroup == null)
			{
				return true;
			}
			return catalog.ProviderUsesDirectCharacterRuntime(providerId) && catalog.SelectionUsesVisualProvider(skinGroup.Id, GetVisualSelection(skinGroup.Id), providerId);
		}
	}

	public static string? GetSelectedFullRuntimeProvider(string groupId)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return null;
			}
			string visualSelection = GetVisualSelection(groupId);
			if (!Catalog.TryGetVisualProviderId(groupId, visualSelection, out string providerId))
			{
				return null;
			}
			return (Catalog.IsRuntimeProviderOption(groupId, visualSelection) && Catalog.ProviderUsesFullRuntime(providerId) && Catalog.IsFullRuntimeProviderFullySelected(providerId, GetVisualSelections())) ? providerId : null;
		}
	}

	internal static bool? IsCharacterBehaviorSelected(Assembly assembly, CharacterModel character, Player? player)
	{
		using ((player == null) ? null : MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId))
		{
			lock (Sync)
			{
				SkinCatalog catalog = Catalog;
				if (catalog == null)
				{
					return null;
				}
				Dictionary<Type, string> value = CharacterBehaviorGroups.GetValue(catalog, (SkinCatalog _) => new Dictionary<Type, string>());
				Type type = ((object)character).GetType();
				if (!value.TryGetValue(type, out var value2))
				{
					SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, type.Name);
					if (skinGroup == null)
					{
						return null;
					}
					value2 = (value[type] = skinGroup.Id);
				}
				string selectedRuntimeProvider = GetSelectedRuntimeProvider(value2);
				return selectedRuntimeProvider != null && ManagedSkinModLoader.IsProviderAssemblyFor(selectedRuntimeProvider, assembly);
			}
		}
	}

	internal static string? GetSelectedCreatureRuntimeProvider(string groupId)
	{
		lock (Sync)
		{
			string selectedFullRuntimeProvider = GetSelectedFullRuntimeProvider(groupId);
			if (selectedFullRuntimeProvider != null)
			{
				return selectedFullRuntimeProvider;
			}
			string selectedRuntimeProvider = GetSelectedRuntimeProvider(groupId);
			object result;
			if (selectedRuntimeProvider != null)
			{
				SkinCatalog? catalog = Catalog;
				if (catalog != null && catalog.ProviderUsesDirectCharacterRuntime(selectedRuntimeProvider))
				{
					result = selectedRuntimeProvider;
					goto IL_0041;
				}
			}
			result = null;
			goto IL_0041;
			IL_0041:
			return (string?)result;
		}
	}

	public static string? GetSelectedRuntimeProvider(string groupId)
	{
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null)
			{
				return null;
			}
			string visualSelection = GetVisualSelection(groupId);
			string providerId;
			return (catalog.IsRuntimeProviderOption(groupId, visualSelection) && catalog.TryGetVisualProviderId(groupId, visualSelection, out providerId)) ? providerId : null;
		}
	}

	public static void ApplySelectedVisualPostfix(string groupId, object model, ref NCreatureVisuals visuals)
	{
		string text;
		lock (Sync)
		{
			string visualSelection = GetVisualSelection(groupId);
			SkinCatalog catalog = Catalog;
			text = ((catalog != null && catalog.IsRuntimeProviderOption(groupId, visualSelection) && (!catalog.ProviderUsesFullRuntime(visualSelection) || catalog.IsFullRuntimeProviderFullySelected(visualSelection, GetVisualSelections()))) ? catalog.ResolveVisualProviderId(visualSelection) : null);
		}
		if (text != null)
		{
			ManagedSkinModLoader.ApplySelectedVisualPostfix(text, model, ref visuals);
		}
	}

	public static bool IsScopedMonsterRuntimeProviderSelected(string providerId, string monsterId)
	{
		return ScopedMonsterSelections.IsSelected(providerId, monsterId);
	}

	internal static void ReleaseTransientRuntimeResources(IEnumerable<string> groupIds)
	{
		lock (Sync)
		{
			int num = 0;
			int num2 = 0;
			foreach (string item in groupIds.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				string prefix = item + "\n";
				string[] array = RuntimeResourceCache.Keys.Where((string key) => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
				foreach (string key2 in array)
				{
					if (RuntimeResourceCache.Remove(key2))
					{
						num++;
					}
				}
				array = RuntimeResourceBundles.Keys.Where((string key) => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
				foreach (string key3 in array)
				{
					if (RuntimeResourceBundles.Remove(key3))
					{
						num2++;
					}
				}
			}
			if (num > 0 || num2 > 0)
			{
				ModLog.Info($"战斗房间退出后已释放 {num} 个怪物资源引用/{num2} 个资源包缓存；已生成的运行包仍保留供下次快速复用。");
			}
		}
	}

	public static void ApplyScopedMonsterRuntimeProviderSelection(string providerId, string monsterId, bool enabled)
	{
		string text;
		string selection;
		lock (Sync)
		{
			SkinCatalog catalog = Catalog;
			if (catalog == null || !catalog.ProviderUsesScopedMonsterRuntime(providerId))
			{
				return;
			}
			text = catalog.ResolveManagedMonsterGroupId(monsterId);
			if (text == null)
			{
				return;
			}
			selection = Config.GetSelection(text);
		}
		if (enabled)
		{
			if (!selection.Equals(providerId, StringComparison.OrdinalIgnoreCase))
			{
				ApplySelection(text, providerId);
			}
		}
		else if (selection.Equals(providerId, StringComparison.OrdinalIgnoreCase))
		{
			ApplySelection(text, "__base__");
		}
	}

	public static bool IsExternalRuntimeProviderSelected(string groupId)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return false;
			}
			string visualSelection = GetVisualSelection(groupId);
			return Catalog.IsRuntimeProviderOption(groupId, visualSelection) && !Catalog.IsResourceBackedOption(groupId, visualSelection) && Catalog.GetRuntimeImagePath(groupId, visualSelection) != null;
		}
	}

	public static bool ShouldDeferCharacterIconResourceToExternalRuntime(string groupId, string resourcePath)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return false;
			}
			string visualSelection = GetVisualSelection(groupId);
			return Catalog.IsRuntimeProviderOption(groupId, visualSelection) && !Catalog.IsResourceBackedOption(groupId, visualSelection) && Catalog.GetRuntimeImagePath(groupId, visualSelection) != null;
		}
	}

	public static bool IsInteractiveRuntimeProviderSelected(string groupId)
	{
		lock (Sync)
		{
			if (Catalog == null)
			{
				return false;
			}
			string visualSelection = GetVisualSelection(groupId);
			return Catalog.IsRuntimeProviderOption(groupId, visualSelection) && Catalog.ProviderUsesInteractiveRuntime(visualSelection);
		}
	}

	public static bool IsManagedResourceOptionSelected(string groupId)
	{
		lock (Sync)
		{
			return Catalog?.IsResourceBackedOption(groupId, GetVisualSelection(groupId)) ?? false;
		}
	}

	public static float GetSelectedMonsterScale(string groupId)
	{
		lock (Sync)
		{
			string selection = Config.GetSelection(groupId);
			Dictionary<string, float> value;
			float value2;
			return (Config.MonsterScales.TryGetValue(groupId, out value) && value.TryGetValue(selection, out value2)) ? Mathf.Clamp(value2, 0.2f, 5f) : 1f;
		}
	}

	public static float SetSelectedMonsterScale(string groupId, float scale, bool save = true)
	{
		lock (Sync)
		{
			float num = Mathf.Clamp(Mathf.Round(scale / 0.05f) * 0.05f, 0.2f, 5f);
			string selection = Config.GetSelection(groupId);
			if (Mathf.IsEqualApprox(num, 1f))
			{
				if (Config.MonsterScales.TryGetValue(groupId, out Dictionary<string, float> value))
				{
					value.Remove(selection);
					if (value.Count == 0)
					{
						Config.MonsterScales.Remove(groupId);
					}
				}
			}
			else
			{
				if (!Config.MonsterScales.TryGetValue(groupId, out Dictionary<string, float> value2))
				{
					value2 = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
					Config.MonsterScales[groupId] = value2;
				}
				value2[selection] = num;
			}
			if (save)
			{
				Config.Save(ConfigPath);
			}
			return num;
		}
	}

	public static CharacterCombatTransform GetCharacterCombatTransform(string groupId, string? optionId = null)
	{
		lock (Sync)
		{
			string key = optionId ?? Config.GetSelection(groupId);
			if (!Config.CharacterCombatTransforms.TryGetValue(groupId, out Dictionary<string, CharacterCombatTransform> value) || !value.TryGetValue(key, out var value2))
			{
				return new CharacterCombatTransform();
			}
			return NormalizeCharacterCombatTransform(value2);
		}
	}

	internal static IReadOnlyDictionary<string, CharacterCombatTransform> GetSessionCharacterCombatTransforms(string groupId, string optionId)
	{
		lock (Sync)
		{
			Dictionary<string, CharacterCombatTransform> dictionary = new Dictionary<string, CharacterCombatTransform>(StringComparer.OrdinalIgnoreCase) { [groupId] = GetCharacterCombatTransform(groupId, optionId) };
			string companionPrefix = groupId + "::companion::";
			foreach (KeyValuePair<string, Dictionary<string, CharacterCombatTransform>> item in Config.CharacterCombatTransforms.Where<KeyValuePair<string, Dictionary<string, CharacterCombatTransform>>>((KeyValuePair<string, Dictionary<string, CharacterCombatTransform>> pair) => pair.Key.StartsWith(companionPrefix, StringComparison.OrdinalIgnoreCase)))
			{
				if (item.Value.TryGetValue(optionId, out var value))
				{
					dictionary[item.Key] = NormalizeCharacterCombatTransform(value);
				}
			}
			return dictionary;
		}
	}

	public static CharacterCombatTransform SetCharacterCombatTransform(string groupId, string optionId, CharacterCombatTransform value, bool save = true)
	{
		lock (Sync)
		{
			CharacterCombatTransform characterCombatTransform = NormalizeCharacterCombatTransform(value);
			if (IsDefaultCharacterCombatTransform(characterCombatTransform))
			{
				if (Config.CharacterCombatTransforms.TryGetValue(groupId, out Dictionary<string, CharacterCombatTransform> value2))
				{
					value2.Remove(optionId);
					if (value2.Count == 0)
					{
						Config.CharacterCombatTransforms.Remove(groupId);
					}
				}
			}
			else
			{
				if (!Config.CharacterCombatTransforms.TryGetValue(groupId, out Dictionary<string, CharacterCombatTransform> value3))
				{
					value3 = new Dictionary<string, CharacterCombatTransform>(StringComparer.OrdinalIgnoreCase);
					Config.CharacterCombatTransforms[groupId] = value3;
				}
				value3[optionId] = characterCombatTransform;
			}
			if (save)
			{
				Config.Save(ConfigPath);
			}
			return characterCombatTransform;
		}
	}

	internal static CharacterCombatTransform NormalizeCharacterCombatTransform(CharacterCombatTransform value)
	{
		return new CharacterCombatTransform(Mathf.Clamp(Mathf.Round(value.Scale / 0.05f) * 0.05f, 0.2f, 5f), Mathf.Clamp(Mathf.Round(value.OffsetX / 1f) * 1f, -1000f, 1000f), Mathf.Clamp(Mathf.Round(value.OffsetY / 1f) * 1f, -1000f, 1000f))
		{
			HealthBarScale = Mathf.Clamp(Mathf.Round(value.HealthBarScale / 0.05f) * 0.05f, 0.2f, 5f),
			HealthBarOffsetX = Mathf.Clamp(Mathf.Round(value.HealthBarOffsetX / 1f) * 1f, -1000f, 1000f),
			HealthBarOffsetY = Mathf.Clamp(Mathf.Round(value.HealthBarOffsetY / 1f) * 1f, -1000f, 1000f),
			HealthBarFollowsModelScale = value.HealthBarFollowsModelScale,
			HealthBarFollowsModelMovement = value.HealthBarFollowsModelMovement,
			IntentScale = Mathf.Clamp(Mathf.Round(value.IntentScale / 0.05f) * 0.05f, 0.2f, 5f),
			IntentOffsetX = Mathf.Clamp(Mathf.Round(value.IntentOffsetX / 1f) * 1f, -1000f, 1000f),
			IntentOffsetY = Mathf.Clamp(Mathf.Round(value.IntentOffsetY / 1f) * 1f, -1000f, 1000f),
			IntentFollowsModelScale = value.IntentFollowsModelScale,
			IntentFollowsModelMovement = value.IntentFollowsModelMovement,
			SelectionReticleScale = Mathf.Clamp(Mathf.Round(value.SelectionReticleScale / 0.05f) * 0.05f, 0.2f, 5f),
			SelectionReticleOffsetX = Mathf.Clamp(Mathf.Round(value.SelectionReticleOffsetX / 1f) * 1f, -1000f, 1000f),
			SelectionReticleOffsetY = Mathf.Clamp(Mathf.Round(value.SelectionReticleOffsetY / 1f) * 1f, -1000f, 1000f),
			SelectionReticleFollowsModelScale = value.SelectionReticleFollowsModelScale,
			SelectionReticleFollowsModelMovement = value.SelectionReticleFollowsModelMovement
		};
	}

	private static bool IsDefaultCharacterCombatTransform(CharacterCombatTransform value)
	{
		if (Mathf.IsEqualApprox(value.Scale, 1f) && Mathf.IsZeroApprox(value.OffsetX) && Mathf.IsZeroApprox(value.OffsetY) && Mathf.IsEqualApprox(value.HealthBarScale, 1f) && Mathf.IsZeroApprox(value.HealthBarOffsetX) && Mathf.IsZeroApprox(value.HealthBarOffsetY) && !value.HealthBarFollowsModelScale && value.HealthBarFollowsModelMovement && Mathf.IsEqualApprox(value.IntentScale, 1f) && Mathf.IsZeroApprox(value.IntentOffsetX) && Mathf.IsZeroApprox(value.IntentOffsetY) && !value.IntentFollowsModelScale && value.IntentFollowsModelMovement && Mathf.IsEqualApprox(value.SelectionReticleScale, 1f) && Mathf.IsZeroApprox(value.SelectionReticleOffsetX) && Mathf.IsZeroApprox(value.SelectionReticleOffsetY) && value.SelectionReticleFollowsModelScale)
		{
			return value.SelectionReticleFollowsModelMovement;
		}
		return false;
	}

	public static Texture2D GetSelectedRuntimeImageTexture(string groupId)
	{
		lock (Sync)
		{
			SkinCatalog? obj = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
			string visualSelection = GetVisualSelection(groupId);
			string text = obj.GetRuntimeImagePath(groupId, visualSelection) ?? throw new InvalidOperationException(groupId + "/" + visualSelection + " 没有独立图片资源。");
			string key = RuntimeResourceKey(groupId, "external-image:" + text);
			if (RuntimeResourceCache.TryGetValue(key, out Resource value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				Texture2D val = (Texture2D)(object)((value is Texture2D) ? value : null);
				if (val != null)
				{
					return val;
				}
			}
			ImageTexture val2 = ImageTexture.CreateFromImage(LoadRuntimeImage(text));
			RuntimeResourceCache[key] = (Resource)(object)val2;
			return (Texture2D)(object)val2;
		}
	}

	private static Image LoadRuntimeImage(string imagePath)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = File.ReadAllBytes(imagePath);
		if (array.Length == 0)
		{
			throw new InvalidOperationException("独立皮肤图片为空：" + imagePath);
		}
		Image val = new Image();
		Error val2;
		if (HasPrefix(array, new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 }))
		{
			val2 = val.LoadPngFromBuffer(array);
		}
		else if (HasPrefix(array, new byte[3] { 255, 216, 255 }))
		{
			val2 = val.LoadJpgFromBuffer(array);
		}
		else
		{
			if (array.Length < 12 || !HasPrefix(array, "RIFF"u8) || !array.AsSpan(8, 4).SequenceEqual("WEBP"u8))
			{
				((GodotObject)val).Dispose();
				val = Image.LoadFromFile(imagePath);
				if (val == null || val.IsEmpty())
				{
					throw new InvalidOperationException("无法识别独立皮肤图片格式：" + imagePath);
				}
				return val;
			}
			val2 = val.LoadWebpFromBuffer(array);
		}
		if ((int)val2 != 0 || val.IsEmpty())
		{
			((GodotObject)val).Dispose();
			throw new InvalidOperationException($"无法解码独立皮肤图片（{val2}）：{imagePath}");
		}
		return val;
		static bool HasPrefix(byte[] value, ReadOnlySpan<byte> prefix)
		{
			if (value.Length >= prefix.Length)
			{
				return value.AsSpan(0, prefix.Length).SequenceEqual(prefix);
			}
			return false;
		}
	}

	public static RuntimeMonsterVisualMode? GetSelectedRuntimeMonsterVisualMode(string groupId)
	{
		lock (Sync)
		{
			return Catalog?.GetRuntimeMonsterVisualMode(groupId, GetVisualSelection(groupId));
		}
	}

	public static AncientLayeredImageTextures? GetSelectedAncientLayeredImageTextures(string groupId)
	{
		lock (Sync)
		{
			AncientLayeredImagePaths ancientLayeredImagePaths = (Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。")).GetAncientLayeredImagePaths(groupId, GetVisualSelection(groupId));
			if (ancientLayeredImagePaths == null)
			{
				return null;
			}
			string[] resourcePaths = new string[4] { ancientLayeredImagePaths.Character, ancientLayeredImagePaths.BackgroundCover, ancientLayeredImagePaths.Mask, ancientLayeredImagePaths.SleepingCharacter }.Where((string path) => path != null).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase)
				.ToArray();
			IReadOnlyDictionary<string, Resource> resources = LoadRuntimeResources(groupId, (IReadOnlyCollection<string>)(object)resourcePaths);
			return new AncientLayeredImageTextures(Required(ancientLayeredImagePaths.Character), Optional(ancientLayeredImagePaths.BackgroundCover), Optional(ancientLayeredImagePaths.Mask), Optional(ancientLayeredImagePaths.SleepingCharacter));
			Texture2D? Optional(string? path)
			{
				if (path != null)
				{
					return Required(path);
				}
				return null;
			}
			Texture2D Required(string path)
			{
				Resource? valueOrDefault = resources.GetValueOrDefault(path);
				return (Texture2D)(object)(((valueOrDefault is Texture2D) ? valueOrDefault : null) ?? throw new InvalidOperationException("先古图层资源不是贴图：" + path));
			}
		}
	}

	public static IReadOnlyDictionary<string, PackedScene> LoadRuntimeScenes(string groupId, IReadOnlyCollection<string> scenePaths)
	{
		IReadOnlyDictionary<string, Resource> resources = LoadRuntimeResources(groupId, scenePaths, includeProviderDependencies: true);
		return scenePaths.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToDictionary<string, string, PackedScene>((string scenePath) => scenePath, delegate(string scenePath)
		{
			Resource? valueOrDefault = resources.GetValueOrDefault(scenePath);
			return (PackedScene)(object)(((valueOrDefault is PackedScene) ? valueOrDefault : null) ?? throw new InvalidOperationException("独立皮肤资源不是场景：" + scenePath));
		}, StringComparer.OrdinalIgnoreCase);
	}

	public static IReadOnlyDictionary<string, Resource> LoadRuntimeResources(string groupId, IReadOnlyCollection<string> resourcePaths, bool includeProviderDependencies = false)
	{
		lock (Sync)
		{
			return LoadRuntimeResourcesForSelection(groupId, GetVisualSelection(groupId), resourcePaths, includeProviderDependencies);
		}
	}

	private static IReadOnlyDictionary<string, Resource> LoadRuntimeResourcesForSelection(string groupId, string selection, IReadOnlyCollection<string> resourcePaths, bool includeProviderDependencies)
	{
		if (Catalog == null)
		{
			throw new InvalidOperationException("皮肤目录尚未初始化。");
		}
		if (TryGetCachedRuntimeResources(groupId, selection, resourcePaths, out IReadOnlyDictionary<string, Resource> resources2))
		{
			return resources2;
		}
		return WithRuntimeResourcesForSelection(groupId, selection, resourcePaths, (IReadOnlyDictionary<string, Resource> resources) => resources, includeProviderDependencies, takeOverCanonicalPaths: false);
	}

	public static T WithRuntimeResources<T>(string groupId, IReadOnlyCollection<string> resourcePaths, Func<IReadOnlyDictionary<string, Resource>, T> callback, bool includeProviderDependencies = false, bool takeOverCanonicalPaths = false)
	{
		ArgumentNullException.ThrowIfNull(callback, "callback");
		lock (Sync)
		{
			string visualSelection = GetVisualSelection(groupId);
			return WithRuntimeResourcesForSelection(groupId, visualSelection, resourcePaths, callback, includeProviderDependencies, takeOverCanonicalPaths);
		}
	}

	private static T WithRuntimeResourcesForSelection<T>(string groupId, string selection, IReadOnlyCollection<string> resourcePaths, Func<IReadOnlyDictionary<string, Resource>, T> callback, bool includeProviderDependencies, bool takeOverCanonicalPaths)
	{
		SkinCatalog? catalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
		long timestamp = Stopwatch.GetTimestamp();
		PreparedRuntimeOverlay orPrepareRuntimeOverlay = GetOrPrepareRuntimeOverlay(catalog, groupId, selection, resourcePaths, includeProviderDependencies);
		if (orPrepareRuntimeOverlay.OverlayPath != null && !ProjectSettings.LoadResourcePack(orPrepareRuntimeOverlay.OverlayPath, true, 0))
		{
			throw new InvalidOperationException("Godot 拒绝加载已准备的独立皮肤场景资源包。");
		}
		if (orPrepareRuntimeOverlay.CanonicalDependencyPaths.Count > 0)
		{
			if (!RuntimeCanonicalDependencyPaths.TryGetValue(groupId, out HashSet<string> value))
			{
				value = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				RuntimeCanonicalDependencyPaths[groupId] = value;
			}
			value.UnionWith(orPrepareRuntimeOverlay.CanonicalDependencyPaths);
		}
		Dictionary<string, Resource> resources;
		bool flag = TryGetRuntimeResourceBundle(orPrepareRuntimeOverlay.Key, out resources);
		if (!flag)
		{
			resources = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, string> resourcePath in orPrepareRuntimeOverlay.ResourcePaths)
			{
				Resource val = ResourceLoader.Load<Resource>(resourcePath.Value, (string)null, (CacheMode)3);
				if (val == null)
				{
					throw new InvalidOperationException("无法加载独立皮肤资源：" + resourcePath.Value);
				}
				resources[resourcePath.Key] = val;
				RuntimeResourceCache[RuntimeResourceKey(groupId, selection, resourcePath.Key)] = val;
			}
			RuntimeResourceBundles[orPrepareRuntimeOverlay.Key] = new RuntimeResourceBundleState(resources);
		}
		IDisposable disposable = null;
		using (ManualCharacterVariantBridge.BeginScope(groupId))
		{
			T result;
			try
			{
				if (takeOverCanonicalPaths)
				{
					disposable = BeginCanonicalRuntimeResourceOwnership(groupId, orPrepareRuntimeOverlay.ResourcePaths, resources);
				}
				result = callback(resources);
			}
			finally
			{
				disposable?.Dispose();
				if (orPrepareRuntimeOverlay.RestoreGroups.Count > 0)
				{
					MountOverlay(orPrepareRuntimeOverlay.RestoreGroups);
				}
			}
			double totalMilliseconds = Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds;
			ModLog.Info($"已{(flag ? "复用" : "加载")} {groupId} 的 {resources.Count} 个独立资源；运行包={orPrepareRuntimeOverlay.FileCount} 个文件/{(double)orPrepareRuntimeOverlay.FileSize / 1024.0:F1} KiB，耗时={totalMilliseconds:F1} ms：{orPrepareRuntimeOverlay.AliasToken}");
			return result;
		}
	}

	private static IDisposable BeginCanonicalRuntimeResourceOwnership(string groupId, IReadOnlyDictionary<string, string> resourcePaths, IReadOnlyDictionary<string, Resource> resources)
	{
		bool flag = CharacterCombatSceneInstantiationPolicy.ShouldRestoreCanonicalOwnership(MultiplayerSkinSync.GetScopedSelection(groupId), Config.GetSelection(groupId));
		List<CanonicalRuntimeResourceOwnershipEntry> list = new List<CanonicalRuntimeResourceOwnershipEntry>(resources.Count);
		foreach (KeyValuePair<string, Resource> resource in resources)
		{
			string key = resource.Key;
			Resource value = resource.Value;
			Resource previousResource = null;
			if (ResourceLoader.HasCached(key))
			{
				Resource val = ResourceLoader.Load<Resource>(key, (string)null, (CacheMode)1);
				if (val != null && val != value)
				{
					previousResource = val;
				}
			}
			string text = resourcePaths.GetValueOrDefault(key);
			if (string.IsNullOrWhiteSpace(text))
			{
				text = value.ResourcePath;
			}
			value.TakeOverPath(key);
			list.Add(new CanonicalRuntimeResourceOwnershipEntry(key, text, value, previousResource));
		}
		if (!flag)
		{
			return NoopDisposable.Instance;
		}
		return new CanonicalRuntimeResourceOwnershipScope(list);
	}

	private static PreparedRuntimeOverlay GetOrPrepareRuntimeOverlay(SkinCatalog catalog, string groupId, string selection, IReadOnlyCollection<string> resourcePaths, bool includeProviderDependencies, bool isolateRelicCanonicalPaths = false)
	{
		string[] resourcePaths2 = resourcePaths.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase).Order<string>(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		bool reuseMountedPrivateDependencies = !isolateRelicCanonicalPaths && catalog.IsRuntimeProviderOption(groupId, selection) && catalog.ProviderRequiresCoherentRuntimePackage(selection) && Config.GetSelection(groupId).Equals(selection, StringComparison.OrdinalIgnoreCase);
		string key = RuntimeOverlayKey(groupId, selection, (IReadOnlyCollection<string>)(object)resourcePaths2, includeProviderDependencies, reuseMountedPrivateDependencies, isolateRelicCanonicalPaths);
		if (PreparedRuntimeOverlays.TryGetValue(key, out PreparedRuntimeOverlay value) && (value.OverlayPath == null || File.Exists(value.OverlayPath)))
		{
			return value;
		}
		RuntimeResourceBundles.Remove(key);
		int value2 = ++_overlayGeneration;
		string aliasToken = $"{_sessionId}/{value2:D3}";
		RuntimeResourceOverlay overlay = (isolateRelicCanonicalPaths ? catalog.BuildIsolatedRelicResourceOverlay(groupId, selection, (IReadOnlyCollection<string>)(object)resourcePaths2, aliasToken) : catalog.BuildRuntimeResourceOverlay(groupId, selection, (IReadOnlyCollection<string>)(object)resourcePaths2, aliasToken, includeProviderDependencies, reuseMountedPrivateDependencies));
		IReadOnlySet<string> runtimeDependencyRestoreGroups = catalog.GetRuntimeDependencyRestoreGroups(groupId, overlay.CanonicalDependencyPaths);
		string overlayPath = null;
		long fileSize = 0L;
		if (overlay.Files.Count > 0)
		{
			string path2 = PreparedRuntimeOverlayDirectory();
			overlayPath = Path.Combine(path2, $"{value2:D3}.pck");
			SkinChangerPaths.WriteCache(overlayPath, delegate
			{
				PckArchive.Write(overlayPath, overlay.Files);
			});
			fileSize = new FileInfo(overlayPath).Length;
		}
		PreparedRuntimeOverlay preparedRuntimeOverlay = new PreparedRuntimeOverlay(key, aliasToken, overlayPath, new Dictionary<string, string>(overlay.ResourcePaths, StringComparer.OrdinalIgnoreCase), overlay.CanonicalDependencyPaths.ToHashSet<string>(StringComparer.OrdinalIgnoreCase), runtimeDependencyRestoreGroups.ToHashSet<string>(StringComparer.OrdinalIgnoreCase), overlay.Files.Count, fileSize);
		PreparedRuntimeOverlays[key] = preparedRuntimeOverlay;
		return preparedRuntimeOverlay;
	}

	private static bool TryGetRuntimeResourceBundle(string key, out Dictionary<string, Resource> resources)
	{
		if (RuntimeResourceBundles.TryGetValue(key, out RuntimeResourceBundleState value) && value.Resources.Values.All((Resource resource) => GodotObject.IsInstanceValid((GodotObject)(object)resource)))
		{
			resources = value.Resources;
			return true;
		}
		RuntimeResourceBundles.Remove(key);
		resources = null;
		return false;
	}

	private static bool TryGetCachedRuntimeResources(string groupId, string selection, IReadOnlyCollection<string> resourcePaths, out IReadOnlyDictionary<string, Resource> resources)
	{
		Dictionary<string, Resource> dictionary = new Dictionary<string, Resource>(StringComparer.OrdinalIgnoreCase);
		foreach (string resourcePath in resourcePaths)
		{
			string key = RuntimeResourceKey(groupId, selection, resourcePath);
			if (!RuntimeResourceCache.TryGetValue(key, out Resource value) || !GodotObject.IsInstanceValid((GodotObject)(object)value))
			{
				resources = null;
				return false;
			}
			dictionary[resourcePath] = value;
		}
		resources = dictionary;
		return true;
	}

	private static void RestoreSelection(string key, string? previous, bool hadPrevious)
	{
		if (hadPrevious && previous != null)
		{
			Config.Selections[key] = previous;
		}
		else
		{
			Config.Selections.Remove(key);
		}
	}

	private static void TryRestoreOverlay(string groupId, bool cardOverlay)
	{
		TryRestoreOverlay(new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId }, cardOverlay);
	}

	private static void TryRestoreOverlay(IReadOnlySet<string> groups, bool cardOverlay)
	{
		try
		{
			if (cardOverlay)
			{
				MountCardOverlay(groups);
			}
			else
			{
				MountOverlay(groups);
			}
		}
		catch (Exception value)
		{
			ModLog.Error($"回滚 {string.Join(", ", groups)} 的皮肤覆盖失败：{value}");
		}
	}

	private static void MountOverlay(IReadOnlySet<string> groups)
	{
		long timestamp = Stopwatch.GetTimestamp();
		SkinCatalog skinCatalog = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
		RefreshScopedMonsterSelectionSnapshot(skinCatalog);
		IReadOnlyDictionary<string, string> visualSelections = GetVisualSelections();
		FrameworkCompatibilityLayer.SynchronizeSelections(skinCatalog, visualSelections);
		HashSet<string> activeRuntimeProviders = GetActiveRuntimeProviders(skinCatalog);
		ManagedSkinModLoader.ConfigureRunEnvironmentProviders((IEnumerable<string>)(((object)_runtimeProviderBehaviorScope?.RunEnvironmentProviderIds) ?? ((object)Array.Empty<string>())));
		ManagedSkinModLoader.DeactivateProvidersExcept(activeRuntimeProviders);
		EnsureScopedRuntimeProviderResourcesMounted(skinCatalog, activeRuntimeProviders);
		ManagedSkinModLoader.ActivateSelectedProviders(activeRuntimeProviders.Where(FrameworkRegistryCooperation.IsNativeProvider));
		LargeProviderMountPlan largeProviderMountPlan = MountSelectedLargeRuntimeProviderPacks(skinCatalog, groups);
		HashSet<string> hashSet = largeProviderMountPlan.PromotedPackPaths.Where(MountedLargeRuntimeProviderPacks.ContainsKey).SelectMany((string path) => MountedLargeRuntimeProviderPacks[path].ResourcePaths).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		IReadOnlySet<string> runtimeDependencyRestoreGroups = skinCatalog.GetRuntimeDependencyRestoreGroups(string.Empty, hashSet);
		HashSet<string> onlyGroups = groups.Union(runtimeDependencyRestoreGroups).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		long timestamp2 = Stopwatch.GetTimestamp();
		string[] dependencyPaths = groups.Where(RuntimeCanonicalDependencyPaths.ContainsKey).SelectMany((string group) => RuntimeCanonicalDependencyPaths[group]).Concat(hashSet)
			.Distinct<string>(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		Dictionary<string, ResourceFile> dictionary = skinCatalog.BuildBaselineDependencyOverlay(dependencyPaths);
		Dictionary<string, ResourceFile> dictionary2 = skinCatalog.BuildOverlay(visualSelections, onlyGroups);
		IEnumerable<string> selectedProviderOverlayPaths = from pair in dictionary2
			where largeProviderMountPlan.PromotedPackPaths.Contains(pair.Value.ArchivePath)
			select pair.Key;
		foreach (string item in PromotedPackOverlayPolicy.FindBaselinePathsShadowingSelectedRemaps(dictionary.Keys, selectedProviderOverlayPaths))
		{
			dictionary.Remove(item);
		}
		foreach (KeyValuePair<string, ResourceFile> item2 in dictionary2)
		{
			if (largeProviderMountPlan.PromotedPackPaths.Contains(item2.Value.ArchivePath) && item2.Key.Equals(item2.Value.Path, StringComparison.OrdinalIgnoreCase) && IsDirectProviderPayloadPath(item2.Key))
			{
				dictionary.Remove(item2.Key);
			}
			else
			{
				dictionary[item2.Key] = item2.Value;
			}
		}
		TimeSpan elapsedTime = Stopwatch.GetElapsedTime(timestamp2);
		long timestamp3 = Stopwatch.GetTimestamp();
		MountArchiveOverlay(dictionary, "visual", "Godot 拒绝加载生成的皮肤资源包。");
		foreach (string group in groups)
		{
			RuntimeCanonicalDependencyPaths.Remove(group);
		}
		TimeSpan elapsedTime2 = Stopwatch.GetElapsedTime(timestamp3);
		long timestamp4 = Stopwatch.GetTimestamp();
		RefreshLocalizationIfNeeded(dictionary);
		TimeSpan elapsedTime3 = Stopwatch.GetElapsedTime(timestamp4);
		foreach (SkinGroup item3 in skinCatalog.Groups.Where((SkinGroup group) => groups.Contains(group.Id)))
		{
			string valueOrDefault = visualSelections.GetValueOrDefault(item3.Id, "__base__");
			if (skinCatalog.IsRuntimeProviderOption(item3.Id, valueOrDefault) && skinCatalog.ProviderUsesManagedGodotScripts(valueOrDefault))
			{
				ManagedSkinModLoader.EnsureProviderGodotScripts(skinCatalog.ResolveVisualProviderId(valueOrDefault));
			}
		}
		foreach (string item4 in activeRuntimeProviders.Where(skinCatalog.ProviderUsesManagedGodotScripts))
		{
			ManagedSkinModLoader.EnsureProviderGodotScripts(item4);
		}
		foreach (string item5 in activeRuntimeProviders.Where(skinCatalog.ProviderUsesScopedMonsterRuntime))
		{
			ManagedSkinModLoader.EnsureScopedMonsterSelectionRouter(item5);
		}
		ManagedSkinModLoader.ActivateSelectedProviders(activeRuntimeProviders);
		ScheduleRuntimeProviderPackWarm(skinCatalog, activeRuntimeProviders, "当前场景");
		ModLog.Info($"已挂载 {groups.Count} 个外观分组/{dictionary.Count} 个文件；目录={elapsedTime.TotalMilliseconds:F1} ms，资源包={elapsedTime2.TotalMilliseconds:F1} ms，本地化={elapsedTime3.TotalMilliseconds:F1} ms，总计={Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds:F1} ms。");
	}

	private static void RefreshScopedMonsterSelectionSnapshot(SkinCatalog catalog)
	{
		Dictionary<string, IReadOnlyCollection<string>> dictionary = new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);
		foreach (string providerId in catalog.GetSelectedScopedMonsterRuntimeProviders(Config.Selections))
		{
			dictionary[providerId] = (IReadOnlyCollection<string>)(object)(from groupId in catalog.GetRuntimeProviderGroups(providerId)
				where catalog.ResolveVisualProviderId(Config.GetSelection(groupId)).Equals(providerId, StringComparison.OrdinalIgnoreCase)
				select groupId).ToArray();
		}
		ScopedMonsterSelections.Replace(dictionary);
	}

	private static void MountContextualFullRuntimeProviderPacks(SkinCatalog catalog, string selection)
	{
		if (!catalog.ProviderUsesFullRuntime(selection) || catalog.IsFullRuntimeProviderFullySelected(selection, Config.Selections))
		{
			return;
		}
		foreach (string providerResourcePackPath in catalog.GetProviderResourcePackPaths(selection))
		{
			if (!ProjectSettings.LoadResourcePack(Path.GetFullPath(providerResourcePackPath), true, 0))
			{
				throw new InvalidOperationException("无法为联机玩家挂载 " + selection + " 的完整运行时资源包。");
			}
			ModLog.Info("已为联机玩家临时挂载 " + selection + " 的完整运行时资源。");
		}
	}

	private static void EnsureScopedRuntimeProviderResourcesMounted(SkinCatalog catalog, IEnumerable<string> activeRuntimeProviders)
	{
		foreach (string item in activeRuntimeProviders.Where((string provider) => catalog.ProviderUsesScopedMonsterRuntime(provider) || catalog.ProviderUsesDirectCharacterRuntime(provider)).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			foreach (string providerResourcePackPath in catalog.GetProviderResourcePackPaths(item))
			{
				string fullPath = Path.GetFullPath(providerResourcePackPath);
				if (MountedScopedRuntimeProviderPacks.Add(fullPath))
				{
					if (!ProjectSettings.LoadResourcePack(fullPath, false, 0))
					{
						MountedScopedRuntimeProviderPacks.Remove(fullPath);
						throw new InvalidOperationException("无法挂载 " + item + " 的运行时依赖资源包。");
					}
					ModLog.Info("已低优先级挂载 " + item + " 的完整运行时资源。");
				}
			}
		}
	}

	private static LargeProviderMountPlan MountSelectedLargeRuntimeProviderPacks(SkinCatalog catalog, IReadOnlySet<string> affectedGroups)
	{
		(string GroupId, string Path)[] source = (from selection in catalog.Groups.Select(delegate(SkinGroup @group)
			{
				string selection2 = GetVisualSelection(@group.Id);
				SkinOption item = @group.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selection2, StringComparison.OrdinalIgnoreCase) && option.IsRuntimeProvider);
				return (GroupId: @group.Id, Option: item);
			})
			where selection.Option != null
			select selection).ToArray().SelectMany(delegate((string GroupId, SkinOption Option) selection)
		{
			string effectiveProviderId = selection.Option.EffectiveProviderId;
			bool requiresCoherentPackage = catalog.ProviderRequiresCoherentRuntimePackage(effectiveProviderId);
			return from path in catalog.GetProviderResourcePackPaths(effectiveProviderId).Select(Path.GetFullPath)
				where requiresCoherentPackage || new FileInfo(path).Length >= 67108864
				select (GroupId: selection.GroupId, Path: path);
		}).ToArray();
		string[] array = source.Select(((string GroupId, string Path) pack) => pack.Path).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		HashSet<string> hashSet = (from pack in source
			where affectedGroups.Contains(pack.GroupId)
			select pack.Path).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		string[] array2 = array;
		foreach (string text in array2)
		{
			bool flag = !MountedLargeRuntimeProviderPacks.ContainsKey(text);
			if (!MountedLargeRuntimeProviderPacks.ContainsKey(text))
			{
				using PckArchive pckArchive = PckArchive.Open(text);
				MountedLargeRuntimeProviderPacks[text] = new MountedProviderPackState(pckArchive.Paths.ToArray());
				hashSet.Add(text);
			}
			if (flag || hashSet.Contains(text))
			{
				if (!ProjectSettings.LoadResourcePack(text, true, 0))
				{
					throw new InvalidOperationException("无法直接挂载完整外观资源包：" + Path.GetFileName(text));
				}
				ModLog.Info("已直接复用完整外观资源包 " + Path.GetFileName(text) + "，后续仅生成资源校正包。");
			}
		}
		return new LargeProviderMountPlan(hashSet);
	}

	private static bool IsDirectProviderPayloadPath(string path)
	{
		return path.StartsWith("res://.godot/imported/", StringComparison.OrdinalIgnoreCase);
	}

	private static HashSet<string> GetActiveRuntimeProviders(SkinCatalog catalog)
	{
		IReadOnlyDictionary<string, string> visualSelections = GetVisualSelections();
		List<IReadOnlyDictionary<string, string>> list = new List<IReadOnlyDictionary<string, string>>();
		list.Add(visualSelections);
		list.AddRange(MultiplayerSkinSync.GetAvailableSelectionMaps());
		HashSet<string> hashSet = catalog.GetFullySelectedFullRuntimeProviders(visualSelections).Union<string>(catalog.GetSelectedInteractiveRuntimeProviders(visualSelections), StringComparer.OrdinalIgnoreCase).Union<string>(catalog.GetSelectedDirectCharacterRuntimeProviders(visualSelections), StringComparer.OrdinalIgnoreCase)
			.Union<string>(catalog.GetSelectedScopedMonsterRuntimeProviders(visualSelections), StringComparer.OrdinalIgnoreCase)
			.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (IReadOnlyDictionary<string, string> item in list.Skip(1))
		{
			hashSet.UnionWith(catalog.GetFullySelectedFullRuntimeProviders(item));
			hashSet.UnionWith(catalog.GetSelectedInteractiveRuntimeProviders(item));
			hashSet.UnionWith(catalog.GetSelectedDirectCharacterRuntimeProviders(item));
			hashSet.UnionWith(catalog.GetSelectedScopedMonsterRuntimeProviders(item));
		}
		RuntimeProviderScope runtimeProviderBehaviorScope = _runtimeProviderBehaviorScope;
		if ((object)runtimeProviderBehaviorScope != null)
		{
			hashSet.UnionWith(runtimeProviderBehaviorScope.RunEnvironmentProviderIds);
		}
		return RuntimeProviderScopePolicy.SelectActiveProviders(hashSet.Select((string providerId) => new RuntimeProviderCandidate(providerId, catalog.GetRuntimeProviderGroups(providerId), catalog.ProviderUsesScopedMonsterRuntime(providerId))), _runtimeProviderBehaviorScope).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static void RefreshLocalizationIfNeeded(IReadOnlyDictionary<string, ResourceFile> mountedFiles)
	{
		foreach (KeyValuePair<string, ResourceFile> item in mountedFiles.Where<KeyValuePair<string, ResourceFile>>((KeyValuePair<string, ResourceFile> pair) => pair.Key.Contains("/localization/", StringComparison.OrdinalIgnoreCase)))
		{
			MountedLocalizationFiles[item.Key] = item.Value;
		}
		string text = BuildActiveLocalizationSignature();
		try
		{
			LocManager instance = LocManager.Instance;
			if (instance == null || string.IsNullOrWhiteSpace(instance.Language))
			{
				_mountedLocalizationSignature = text;
			}
			else if (!string.Equals(_mountedLocalizationSignature, text, StringComparison.Ordinal))
			{
				string language = instance.Language;
				if (_mountedLocalizationSignature != null)
				{
					CaptureLocalizationState(instance, _mountedLocalizationSignature, language);
				}
				string key = LocalizationCacheKey(text, language);
				if (LocalizationStateCache.TryGetValue(key, out LocalizationCacheState value) && SetLanguageInternalMethod != null)
				{
					SetLanguageInternalMethod.Invoke(instance, new object[4]
					{
						language,
						value.Tables,
						value.OverridesActive,
						value.ValidationErrors.ToList()
					});
					_mountedLocalizationSignature = text;
					ModLog.Info("已复用 " + language + " 本地化状态缓存。");
				}
				else
				{
					instance.SetLanguage(language);
					_mountedLocalizationSignature = text;
					CaptureLocalizationState(instance, text, language);
					ModLog.Info("已刷新 " + language + " 本地化缓存。");
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新皮肤本地化缓存失败：" + ex.GetBaseException().Message);
		}
	}

	private static string BuildActiveLocalizationSignature()
	{
		SkinCatalog catalog = Catalog;
		if (catalog == null)
		{
			return string.Empty;
		}
		IReadOnlyDictionary<string, string> visualSelections = GetVisualSelections();
		HashSet<string> activePaths = catalog.FilterModdedLocalizationTables(MountedLocalizationFiles.Keys, visualSelections).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return BuildOverlaySignature(MountedLocalizationFiles.Where<KeyValuePair<string, ResourceFile>>((KeyValuePair<string, ResourceFile> pair) => activePaths.Contains(pair.Key)).ToDictionary<KeyValuePair<string, ResourceFile>, string, ResourceFile>((KeyValuePair<string, ResourceFile> pair) => pair.Key, (KeyValuePair<string, ResourceFile> pair) => pair.Value, StringComparer.OrdinalIgnoreCase), "localization") + "\n" + string.Join("\n", catalog.GetSelectedLocalizationProviderIds(visualSelections).Order<string>(StringComparer.OrdinalIgnoreCase));
	}

	public static IEnumerable<string> FilterModdedLocalizationTables(IEnumerable<string> localizationPaths)
	{
		lock (Sync)
		{
			return Catalog?.FilterModdedLocalizationTables(localizationPaths, GetVisualSelections()) ?? localizationPaths.ToArray();
		}
	}

	private static void CaptureLocalizationState(LocManager manager, string signature, string language)
	{
		if (LocTablesField?.GetValue(manager) is Dictionary<string, LocTable> tables)
		{
			LocalizationStateCache[LocalizationCacheKey(signature, language)] = new LocalizationCacheState(tables, manager.OverridesActive, manager.ValidationErrors.ToList());
		}
	}

	private static string LocalizationCacheKey(string signature, string language)
	{
		return language + "\n" + signature;
	}

	private static void MountCardOverlay(IReadOnlySet<string> groups)
	{
		SkinCatalog? obj = Catalog ?? throw new InvalidOperationException("皮肤目录尚未初始化。");
		Dictionary<string, ResourceFile> dictionary = obj.BuildCardOverlay(priorityStacks: obj.CardGroups.Where((CardSkinGroup group) => groups.Contains(group.Id)).ToDictionary<CardSkinGroup, string, IReadOnlyList<string>>((CardSkinGroup group) => group.Id, (CardSkinGroup group) => (from entry in GetCardPriorityEntriesInternal(@group)
			where entry.Enabled
			select entry.OptionId).ToArray(), StringComparer.OrdinalIgnoreCase), selections: Config.Selections, onlyGroups: groups);
		if (dictionary.Count == 0)
		{
			return;
		}
		foreach (IGrouping<PckArchive, KeyValuePair<string, ResourceFile>> item in from pair in dictionary
			group pair by pair.Value.Archive)
		{
			MountArchiveOverlay(item.ToDictionary<KeyValuePair<string, ResourceFile>, string, ResourceFile>((KeyValuePair<string, ResourceFile> pair) => pair.Key, (KeyValuePair<string, ResourceFile> pair) => pair.Value, StringComparer.OrdinalIgnoreCase), "cards", "Godot 拒绝加载生成的卡牌皮肤资源包。");
		}
		CardCanonicalResourceOwners.Reset();
	}

	private static void MountArchiveOverlay(IReadOnlyDictionary<string, ResourceFile> files, string category, string failureMessage)
	{
		if (files.Count == 0)
		{
			return;
		}
		string key = BuildOverlaySignature(files, category);
		if (!MountedOverlayCache.TryGetValue(key, out string overlayPath) || !File.Exists(overlayPath))
		{
			overlayPath = TryGetCompleteSourceArchive(files);
			if (overlayPath == null)
			{
				overlayPath = Path.Combine(SkinChangerPaths.CacheDirectory, $"sts2_skin_overlay_{_sessionId}_{++_overlayGeneration:D3}_{category}.pck");
				Dictionary<string, (PckArchive Archive, string Path)> sources = files.ToDictionary<KeyValuePair<string, ResourceFile>, string, (PckArchive, string)>((KeyValuePair<string, ResourceFile> pair) => pair.Key, (KeyValuePair<string, ResourceFile> pair) => (Archive: pair.Value.Archive, Path: pair.Value.Path), StringComparer.OrdinalIgnoreCase);
				SkinChangerPaths.WriteCache(overlayPath, delegate
				{
					PckArchive.WriteFromArchives(overlayPath, sources);
				});
			}
			MountedOverlayCache[key] = overlayPath;
		}
		if (ProjectSettings.LoadResourcePack(overlayPath, true, 0))
		{
			return;
		}
		MountedOverlayCache.Remove(key);
		throw new InvalidOperationException(failureMessage);
	}

	private static string? TryGetCompleteSourceArchive(IReadOnlyDictionary<string, ResourceFile> files)
	{
		PckArchive archive = files.Values.FirstOrDefault()?.Archive;
		if (archive == null || files.Values.Any((ResourceFile file) => file.Archive != archive) || !File.Exists(archive.Path))
		{
			return null;
		}
		HashSet<string> hashSet = archive.Paths.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (files.Count != hashSet.Count || files.Any<KeyValuePair<string, ResourceFile>>((KeyValuePair<string, ResourceFile> pair) => pair.Value.Archive != archive || !pair.Key.Equals(pair.Value.Path, StringComparison.OrdinalIgnoreCase)) || !hashSet.SetEquals(files.Keys))
		{
			return null;
		}
		return archive.Path;
	}

	private static string BuildOverlaySignature(IReadOnlyDictionary<string, ResourceFile> files, string category)
	{
		using IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		AppendSignaturePart(incrementalHash, category);
		foreach (KeyValuePair<string, ResourceFile> item in files.OrderBy<KeyValuePair<string, ResourceFile>, string>((KeyValuePair<string, ResourceFile> pair) => pair.Key, StringComparer.OrdinalIgnoreCase))
		{
			AppendSignaturePart(incrementalHash, item.Key);
			AppendSignaturePart(incrementalHash, item.Value.ArchivePath);
			AppendSignaturePart(incrementalHash, item.Value.Path);
		}
		return Convert.ToHexString(incrementalHash.GetHashAndReset());
	}

	private static void AppendSignaturePart(IncrementalHash hash, string value)
	{
		hash.AppendData(Encoding.UTF8.GetBytes(value));
		hash.AppendData((ReadOnlySpan<byte>)new byte[1] { 0 });
	}

	private static void ClearRuntimeResourceCache(string groupId)
	{
		string prefix = groupId + "\n";
		RestoreCachedRelicTexturesToBaseline(groupId, prefix);
		string[] array = RuntimeResourceCache.Keys.Where((string key) => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (string key2 in array)
		{
			RuntimeResourceCache.Remove(key2);
		}
		array = PreparedRuntimeOverlays.Keys.Where((string key) => key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (string key3 in array)
		{
			PreparedRuntimeOverlays.Remove(key3);
			RuntimeResourceBundles.Remove(key3);
		}
	}

	private static void RestoreCachedRelicTexturesToBaseline(string groupId, string cachePrefix)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		if (Catalog == null)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<string, Resource> item in RuntimeResourceCache.Where<KeyValuePair<string, Resource>>((KeyValuePair<string, Resource> pair) => pair.Key.StartsWith(cachePrefix, StringComparison.OrdinalIgnoreCase) && pair.Value is AtlasTexture))
		{
			int num2 = item.Key.IndexOf('\n', cachePrefix.Length);
			if (num2 < 0 || num2 + 1 >= item.Key.Length)
			{
				continue;
			}
			string key = item.Key;
			int num3 = num2 + 1;
			string text = key.Substring(num3, key.Length - num3);
			if (!SkinCatalog.IsRelicAtlasSpritePath(text))
			{
				continue;
			}
			Resource value = item.Value;
			AtlasTexture val = (AtlasTexture)(object)((value is AtlasTexture) ? value : null);
			if (val == null)
			{
				continue;
			}
			try
			{
				Texture2D? baselineRelicIcon = GetBaselineRelicIcon(Catalog, text);
				AtlasTexture val2 = (AtlasTexture)(object)((baselineRelicIcon is AtlasTexture) ? baselineRelicIcon : null);
				if (val2 != null)
				{
					val.Atlas = val2.Atlas;
					val.Region = val2.Region;
					val.Margin = val2.Margin;
					val.FilterClip = val2.FilterClip;
					((Resource)val).EmitChanged();
					num++;
				}
			}
			catch (Exception ex)
			{
				ModLog.Warn("回退 " + text + " 的遗物图集缓存失败，将在下次创建图标时恢复：" + ex.GetBaseException().Message);
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已将 {groupId} 的 {num} 个存量遗物图标恢复到游戏图集。");
		}
	}

	private static void ClearCardPortraitCache(string groupId)
	{
		HashSet<string> affectedGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { groupId };
		foreach (KeyValuePair<CardModel, CardLookup> item in (IEnumerable<KeyValuePair<CardModel, CardLookup>>)_cardLookupCache)
		{
			if (item.Value.PriorityGroups.Any((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)))
			{
				affectedGroups.Add(item.Value.GroupId);
			}
		}
		string[] array = CardPortraitCache.Keys.Where(Affected).ToArray();
		foreach (string key2 in array)
		{
			CardPortraitCache.Remove(key2);
			ExternalCardProviderIdentityPaths.Remove(key2);
		}
		FailedCardPortraitRequests.RemoveWhere(Affected);
		bool Affected(string key)
		{
			return affectedGroups.Any((string id) => key.StartsWith(id + "\n", StringComparison.OrdinalIgnoreCase));
		}
	}

	private static string CardSelectionKey(string groupId)
	{
		return "cards:" + groupId;
	}

	private static string IndividualCardSelectionKey(CardModel card)
	{
		return "cards:item:" + ((object)((AbstractModel)card).Id).ToString().ToLowerInvariant();
	}

	private static string? NormalizeCardSkinPresetName(string name)
	{
		string text = name.Trim();
		if (!CanRenameOrDeletePreset(text))
		{
			return null;
		}
		if (text.Length == 0 || text.Any(char.IsControl))
		{
			LastError = "卡图预设名称不能为空。";
			return null;
		}
		if (text.Length > 40)
		{
			LastError = $"卡图预设名称不能超过 {40} 个字符。";
			return null;
		}
		return text;
	}

	private static string? NormalizeMonsterSkinPresetName(string name)
	{
		string text = name.Trim();
		if (!CanRenameOrDeletePreset(text))
		{
			return null;
		}
		if (text.Length == 0 || text.Any(char.IsControl))
		{
			LastError = "怪物皮肤预设名称不能为空。";
			return null;
		}
		if (text.Length > 40)
		{
			LastError = $"怪物皮肤预设名称不能超过 {40} 个字符。";
			return null;
		}
		return text;
	}

	private static int FindMonsterSkinPresetIndex(string categoryId, string name)
	{
		return Config.MonsterSkinPresets.FindIndex((MonsterSkinPreset preset) => preset.CategoryId.Equals(categoryId, StringComparison.OrdinalIgnoreCase) && preset.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
	}

	private static MonsterSkinPreset CaptureCurrentMonsterSkinPreset(string categoryId, string name)
	{
		if (!Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out List<string> value))
		{
			throw new InvalidOperationException("未知的怪物图鉴分类：" + categoryId);
		}
		List<MonsterSkinPriorityEntry> source = GetMonsterPriorityEntriesInternal(categoryId).ToList();
		Dictionary<string, string> selections = ((IEnumerable<string>)value).ToDictionary((Func<string, string>)((string groupId) => groupId), (Func<string, string>)Config.GetSelection, (IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase);
		MonsterSkinPresetSnapshot monsterSkinPresetSnapshot = MonsterSkinPresetPolicy.Capture(categoryId, value, source.Select((MonsterSkinPriorityEntry entry) => new MonsterSkinPresetPriorityState(entry.OptionId, entry.Enabled)).ToArray(), selections);
		return new MonsterSkinPreset
		{
			Name = name,
			CategoryId = monsterSkinPresetSnapshot.CategoryId,
			Priority = monsterSkinPresetSnapshot.Priority.Select((MonsterSkinPresetPriorityState entry) => new MonsterSkinPriorityEntry(entry.OptionId, entry.Enabled)).ToList(),
			Selections = new Dictionary<string, string>(monsterSkinPresetSnapshot.Selections, StringComparer.OrdinalIgnoreCase),
			FollowingGroupIds = value.Where((string groupId) => Config.MonsterGroupsFollowingCategory.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)).ToList()
		};
	}

	private static MonsterSkinPreset CloneMonsterSkinPreset(MonsterSkinPreset preset)
	{
		return preset.Clone();
	}

	private static bool SaveMonsterPresetConfiguration(string operation, MonsterSkinPreset preset, Action rollback)
	{
		try
		{
			Config.Save(ConfigPath);
			LastError = null;
			return true;
		}
		catch (Exception ex)
		{
			rollback();
			LastError = ex.Message;
			ModLog.Error($"{operation}怪物皮肤预设 {preset.Name} 失败：{ex}");
			return false;
		}
	}

	private static void RestoreActiveMonsterPreset(string categoryId, string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			Config.ActiveMonsterSkinPresets.Remove(categoryId);
		}
		else
		{
			Config.ActiveMonsterSkinPresets[categoryId] = name;
		}
	}

	private static int FindCardSkinPresetIndex(string groupId, string name)
	{
		return Config.CardSkinPresets.FindIndex(delegate(CardSkinPreset preset)
		{
			string? categoryId = preset.CategoryId;
			return categoryId != null && categoryId.Equals(groupId, StringComparison.OrdinalIgnoreCase) && preset.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase);
		});
	}

	private static bool IsKnownCardGroup(string groupId)
	{
		if (!string.IsNullOrWhiteSpace(groupId))
		{
			return Catalog?.CardGroups.Any((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)) ?? false;
		}
		return false;
	}

	private static string? GetActiveCardSkinPreset(string groupId)
	{
		return Config.ActiveCardSkinPresets.GetValueOrDefault(groupId);
	}

	private static void SetActiveCardSkinPreset(string groupId, string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			Config.ActiveCardSkinPresets.Remove(groupId);
		}
		else
		{
			Config.ActiveCardSkinPresets[groupId] = name;
		}
	}

	private static CardSkinPreset CaptureCurrentCardSkinPreset(string groupId, string name)
	{
		CardSkinGroup cardSkinGroup = Catalog?.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (cardSkinGroup == null)
		{
			throw new InvalidOperationException("未知的卡牌皮肤分类：" + groupId);
		}
		GetCardPriorityEntriesInternal(cardSkinGroup);
		List<CardSkinPriorityEntry> value;
		return new CardSkinPreset
		{
			Name = name,
			CategoryId = cardSkinGroup.Id,
			CardSkinPriorities = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase) { [cardSkinGroup.Id] = (Config.CardSkinPriorities.TryGetValue(cardSkinGroup.Id, out value) ? value.ToList() : new List<CardSkinPriorityEntry>()) },
			Selections = CaptureCardSelectionsForGroup(cardSkinGroup.Id)
		};
	}

	private static CardSkinPreset CloneCardSkinPreset(CardSkinPreset preset)
	{
		return preset.Clone();
	}

	private static Dictionary<string, List<CardSkinPriorityEntry>> CloneCardSkinPriorities(IReadOnlyDictionary<string, List<CardSkinPriorityEntry>> priorities)
	{
		return priorities.ToDictionary<KeyValuePair<string, List<CardSkinPriorityEntry>>, string, List<CardSkinPriorityEntry>>((KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
	}

	private static Dictionary<string, string> CaptureCardSelections()
	{
		return Config.Selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
	}

	private static Dictionary<string, string> CaptureCardSelectionsForGroup(string groupId)
	{
		return (from pair in CaptureCardSelections()
			where CardSelectionBelongsToGroup(pair.Key, groupId)
			select pair).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
	}

	private static void ReplaceCardSelectionsForGroup(string groupId, IReadOnlyDictionary<string, string> selections)
	{
		string[] array = Config.Selections.Keys.Where((string key) => CardSelectionBelongsToGroup(key, groupId)).ToArray();
		foreach (string key2 in array)
		{
			Config.Selections.Remove(key2);
		}
		foreach (KeyValuePair<string, string> item in selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase) && CardSelectionBelongsToGroup(pair.Key, groupId)))
		{
			Config.Selections[item.Key] = item.Value;
		}
	}

	private static bool CardSelectionBelongsToGroup(string key, string groupId)
	{
		if (key.Equals(CardSelectionKey(groupId), StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (!key.StartsWith("cards:item:", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return ModelDb.AllCards.Any((CardModel card) => IndividualCardSelectionKey(card).Equals(key, StringComparison.OrdinalIgnoreCase) && CardBelongsToGroup(card, groupId));
	}

	private static void MigrateLegacyCardSkinPresets()
	{
		if (Catalog != null && Catalog.CardGroups.Count != 0)
		{
			Dictionary<string, string> cardGroups = ModelDb.AllCards.DistinctBy<CardModel, string>(IndividualCardSelectionKey, StringComparer.OrdinalIgnoreCase).ToDictionary<CardModel, string, string>(IndividualCardSelectionKey, (CardModel card) => GetCardLookup(card).GroupId, StringComparer.OrdinalIgnoreCase);
			CardPresetMigrationResult cardPresetMigrationResult = CardPresetMigrationPolicy.Run(Config, Catalog.CardGroups.ToDictionary<CardSkinGroup, string, string>((CardSkinGroup group) => group.Id, (CardSkinGroup group) => group.DisplayName, StringComparer.OrdinalIgnoreCase), cardGroups);
			if (cardPresetMigrationResult.Split > 0 || cardPresetMigrationResult.Archived > 0)
			{
				ModLog.Info($"卡牌预设兼容修复：按实际旧数据拆分 {cardPresetMigrationResult.Split} 项，归档重复回填副本 {cardPresetMigrationResult.Archived} 项；" + "归档保存在 ArchivedLegacyCardSkinPresets，未重置当前卡面选择。");
			}
		}
	}

	private static string RuntimeResourceKey(string groupId, string resourcePath)
	{
		return RuntimeResourceKey(groupId, GetVisualSelection(groupId), resourcePath);
	}

	private static string RuntimeResourceKey(string groupId, string selection, string resourcePath)
	{
		return groupId + "\n" + ManualVariantResourceSelection(groupId, selection) + "\n" + resourcePath + FrameworkRegistryCooperation.CacheSuffix(groupId, selection);
	}

	private static string RuntimeOverlayKey(string groupId, string selection, IReadOnlyCollection<string> resourcePaths, bool includeProviderDependencies, bool reuseMountedPrivateDependencies, bool isolateRelicCanonicalPaths = false)
	{
		return groupId + "\n" + ManualVariantResourceSelection(groupId, selection) + "\n" + includeProviderDependencies + "\n" + reuseMountedPrivateDependencies + "\n" + isolateRelicCanonicalPaths + "\n" + string.Join("\n", resourcePaths) + FrameworkRegistryCooperation.CacheSuffix(groupId, selection);
	}

	private static string[] CharacterSelectResourcePaths(string characterId)
	{
		return new string[3]
		{
			"res://scenes/screens/char_select/char_select_bg_" + characterId + ".tscn",
			"res://images/packed/character_select/char_select_" + characterId + ".png",
			"res://images/packed/character_select/char_select_" + characterId + "_locked.png"
		};
	}

	private static string PreparedRuntimeOverlayDirectory()
	{
		return SkinChangerPaths.RuntimeCacheDirectory;
	}

	private static SkinConfig LoadConfig()
	{
		if (File.Exists(ConfigPath) || File.Exists(ConfigPath + ".bak"))
		{
			return RecoverInterruptedCharacterSkinBundleSession(SkinConfig.Load(ConfigPath));
		}
		SkinConfig current = SkinConfig.Load(LegacyConfigPath);
		if (File.Exists(LegacyConfigPath))
		{
			ModLog.Info("已将旧版 STS2SkinChanger 设置迁移到皮肤切换器-Skin Changer。");
		}
		return RecoverInterruptedCharacterSkinBundleSession(current);
	}

	private static CharacterSkinComposition CloneCharacterSkinComposition(CharacterSkinComposition source)
	{
		return new CharacterSkinComposition
		{
			Id = source.Id,
			GroupId = source.GroupId,
			Name = source.Name,
			SourceOptionIds = source.SourceOptionIds.ToList(),
			HideSources = source.HideSources
		};
	}

	private static void MigrateLegacyCharacterIconSelections()
	{
		if (Catalog == null || Config.CharacterIconSelections.Count == 0)
		{
			return;
		}
		KeyValuePair<string, string>[] array = Config.CharacterIconSelections.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, string> pair = array[i];
			Config.CharacterIconSelections.Remove(pair.Key);
			if (pair.Value.Equals("__follow_character_skin__", StringComparison.OrdinalIgnoreCase) || pair.Value.Equals("__base__", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string iconOptionId = Catalog.ResolveStoredVisualSelectionId(pair.Key, pair.Value);
			IReadOnlyList<SkinOption> rawOptions = Catalog.GetRawCharacterOptions(pair.Key);
			if (rawOptions.All((SkinOption option) => !option.Id.Equals(iconOptionId, StringComparison.OrdinalIgnoreCase)))
			{
				ModLog.Info($"旧头像来源 {pair.Key}/{pair.Value} 已不存在，已忽略。 ");
				continue;
			}
			List<string> list = new List<string> { iconOptionId };
			string selection = Config.GetSelection(pair.Key);
			if (!selection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
			{
				list.AddRange(Catalog.GetCompositionSourceOptionIds(pair.Key, selection));
			}
			list = list.Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
			IEnumerable<string> values = (from sourceId in list
				select rawOptions.FirstOrDefault((SkinOption option) => option.Id.Equals(sourceId, StringComparison.OrdinalIgnoreCase))?.Name into sourceName
				where !string.IsNullOrWhiteSpace(sourceName)
				select sourceName).Cast<string>();
			string text = CharacterSkinCompositionPolicy.UniqueName(string.Join(" + ", values), from composition in Config.CharacterSkinCompositions
				where composition.GroupId.Equals(pair.Key, StringComparison.OrdinalIgnoreCase)
				select composition.Name, ModLocalization.Get(ModText.CombinedSkinDefaultName));
			CharacterSkinComposition characterSkinComposition = new CharacterSkinComposition
			{
				Id = CharacterSkinCompositionPolicy.CreateId(),
				GroupId = pair.Key,
				Name = text,
				SourceOptionIds = list
			};
			Config.CharacterSkinCompositions.Add(characterSkinComposition);
			Config.Selections[pair.Key] = characterSkinComposition.Id;
			ModLog.Info($"已将 {pair.Key} 的旧头像来源迁移为合并皮肤“{text}”。");
		}
	}

	private static void SanitizeSelections()
	{
		Config.CharacterSkinCompositions = CharacterSkinCompositionPolicy.Normalize(Config.CharacterSkinCompositions);
		Catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
		MigrateLegacyCharacterIconSelections();
		Catalog.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
		int num = 0;
		foreach (SkinGroup group in Catalog.Groups)
		{
			if (Config.Selections.TryGetValue(group.Id, out string value))
			{
				string text = Catalog.ResolveStoredVisualSelectionId(group.Id, value);
				if (!text.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					Config.Selections[group.Id] = text;
					num++;
				}
			}
		}
		int num2 = MigrateLegacyManualSlots(Config, Catalog);
		if (num2 > 0)
		{
			ModLog.Info($"已将 {num2} 项旧部件显隐设置迁移为编号皮肤差分。");
		}
		if (num > 0)
		{
			ModLog.Info($"已把 {num} 项旧外观设置迁移到具体的同 ID 差分包。");
		}
		string[] array;
		if (Config.VisualSelectionDefaultsVersion < 1)
		{
			array = Catalog.Groups.Select((SkinGroup group) => Config.GetSelection(group.Id)).Where(Catalog.ProviderUsesScopedMonsterRuntime).Distinct<string>(StringComparer.OrdinalIgnoreCase)
				.ToArray();
			foreach (string providerId2 in array)
			{
				IReadOnlyList<string> scopedMonsterRuntimeProviderGroups = Catalog.GetScopedMonsterRuntimeProviderGroups(providerId2);
				string[] array2 = scopedMonsterRuntimeProviderGroups.Where((string groupId) => Config.GetSelection(groupId).Equals(providerId2, StringComparison.OrdinalIgnoreCase)).ToArray();
				if (array2.Length != 0 && array2.Length != scopedMonsterRuntimeProviderGroups.Count)
				{
					string[] array3 = array2;
					foreach (string key in array3)
					{
						Config.Selections[key] = "__base__";
					}
					ModLog.Info($"已清理 {providerId2} 的 {array2.Length} 个旧版自动外观选择；" + "这些组现在默认使用游戏原版，玩家重新选择后可按怪物独立生效。");
				}
			}
			Config.VisualSelectionDefaultsVersion = 1;
		}
		bool flag = Config.BaselinePartTakeoverVersion < 1;
		foreach (SkinGroup group2 in Catalog.Groups)
		{
			string text2 = SkinCatalog.ResolvePartTakeoverDefault(group2.Options);
			string value2;
			if (text2 == null)
			{
				if (!Config.Selections.ContainsKey(group2.Id))
				{
					Config.Selections[group2.Id] = "__base__";
				}
			}
			else if (!Config.Selections.TryGetValue(group2.Id, out value2))
			{
				Config.Selections[group2.Id] = text2;
			}
			else if (flag && value2.Equals("__base__", StringComparison.OrdinalIgnoreCase))
			{
				Config.Selections[group2.Id] = text2;
				ModLog.Info($"已将 {group2.Id} 的默认外观保留给部件接管 {text2}；" + "该部件原本就由作者自己的 Mod 生效，如需关闭请在皮肤列表中选择游戏原版。");
			}
		}
		Config.BaselinePartTakeoverVersion = 1;
		SanitizeMonsterSkinPriorities();
		SynchronizeCompanionSelections();
		array = (from providerId in Catalog.Groups.Select((SkinGroup @group) => Config.GetSelection(@group.Id)).Select(Catalog.ResolveVisualProviderId).Where(Catalog.ProviderUsesFullRuntime)
				.Distinct<string>(StringComparer.OrdinalIgnoreCase)
			where !Catalog.IsFullRuntimeProviderFullySelected(providerId, Config.Selections)
			select providerId).ToArray();
		foreach (string providerId3 in array)
		{
			IReadOnlyList<string> fullRuntimeProviderGroups = Catalog.GetFullRuntimeProviderGroups(providerId3);
			int num3 = 0;
			foreach (string item in fullRuntimeProviderGroups.Where((string ownedGroupId) => Catalog.SelectionUsesVisualProvider(ownedGroupId, Config.GetSelection(ownedGroupId), providerId3)))
			{
				Config.Selections[item] = "__base__";
				num3++;
			}
			if (num3 > 0)
			{
				ModLog.Info($"已将 {providerId3} 的 {num3} 个不完整联动选择恢复为游戏原版，" + "避免界面显示已选但实际运行时未启用。");
			}
		}
		SynchronizeCompanionSelections();
		SanitizeCardSelections();
		SanitizeVisualProviderPriority();
	}

	private static void SynchronizeCompanionSelections()
	{
		foreach (KeyValuePair<string, string> item in Catalog.BuildCompanionSelectionUpdates(Config.Selections))
		{
			string selection = Config.GetSelection(item.Key);
			if (!selection.Equals(item.Value, StringComparison.OrdinalIgnoreCase))
			{
				Config.Selections[item.Key] = item.Value;
				ModLog.Info($"已按主人皮肤同步随从外观：{item.Key}，{selection} -> {item.Value}。");
			}
		}
	}

	private static void UpdateVisualProviderPriority(string groupId, string optionId)
	{
		IReadOnlyList<string> requestedProviderIds = Catalog.GetSelectionProviderIds(groupId, optionId);
		SanitizeVisualProviderPriority();
		if (requestedProviderIds.Count != 0)
		{
			Config.VisualProviderPriority.RemoveAll((string providerId) => requestedProviderIds.Contains<string>(providerId, StringComparer.OrdinalIgnoreCase));
			Config.VisualProviderPriority.AddRange(CharacterSkinCompositionPolicy.BuildProviderPriority(requestedProviderIds));
		}
	}

	private static void SanitizeVisualProviderPriority()
	{
		HashSet<string> selectedProviderIds = Catalog.Groups.SelectMany((SkinGroup group) => Catalog.GetSelectionProviderIds(group.Id, Config.GetSelection(group.Id))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Config.VisualProviderPriority = Config.VisualProviderPriority.Select((string providerId) => Catalog.ResolveStoredProviderId(providerId, selectedProviderIds)).Where(selectedProviderIds.Contains).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			.ToList();
		foreach (string item in selectedProviderIds.OrderBy<string, string>((string providerId) => providerId, StringComparer.OrdinalIgnoreCase))
		{
			if (!Config.VisualProviderPriority.Contains<string>(item, StringComparer.OrdinalIgnoreCase))
			{
				Config.VisualProviderPriority.Insert(0, item);
			}
		}
	}

	private static void SanitizeCardSelections()
	{
		bool enableAllByDefault = Config.CardPriorityDefaultsVersion < 1;
		foreach (CardSkinGroup cardGroup in Catalog.CardGroups)
		{
			GetCardPriorityEntriesInternal(cardGroup, enableAllByDefault);
		}
		Config.CardPriorityDefaultsVersion = 1;
	}

	private static void MigrateStoredCardSelections()
	{
		SkinCatalog catalog = Catalog;
		if (catalog == null)
		{
			return;
		}
		Dictionary<string, CardSkinGroup> dictionary = catalog.CardGroups.ToDictionary<CardSkinGroup, string>((CardSkinGroup group) => group.Id, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary2 = ModelDb.AllCards.GroupBy<CardModel, string>(IndividualCardSelectionKey, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, CardModel>, string, string>((IGrouping<string, CardModel> group) => group.Key, (IGrouping<string, CardModel> group) => GetCardLookup(group.First()).GroupId, StringComparer.OrdinalIgnoreCase);
		int num = 0;
		KeyValuePair<string, string>[] array = Config.Selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase)).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, string> keyValuePair = array[i];
			string text = null;
			if (dictionary2.TryGetValue(keyValuePair.Key, out var value))
			{
				text = value;
			}
			else if (!keyValuePair.Key.StartsWith("cards:item:", StringComparison.OrdinalIgnoreCase))
			{
				string key = keyValuePair.Key;
				int length = "cards:".Length;
				string text2 = key.Substring(length, key.Length - length);
				if (dictionary.ContainsKey(text2))
				{
					text = text2;
				}
			}
			if (text != null && !keyValuePair.Value.Equals("__inherit__", StringComparison.OrdinalIgnoreCase) && !keyValuePair.Value.Equals("__base__", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = catalog.ResolveStoredCardSelectionId(text, keyValuePair.Value);
				if (!text3.Equals(keyValuePair.Value, StringComparison.OrdinalIgnoreCase))
				{
					Config.Selections[keyValuePair.Key] = text3;
					num++;
				}
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已把 {num} 项旧单卡设置迁移到具体的同 ID 差分包。");
		}
	}

	private static void SanitizeCardSkinPresets()
	{
		SkinCatalog catalog = Catalog;
		if (catalog == null)
		{
			return;
		}
		Dictionary<string, HashSet<string>> dictionary = catalog.CardGroups.ToDictionary<CardSkinGroup, string, HashSet<string>>((CardSkinGroup group) => group.Id, (CardSkinGroup group) => new HashSet<string>(new _003C_003Ez__ReadOnlySingleElementList<string>(CardSelectionKey(group.Id)), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
		foreach (CardModel allCard in ModelDb.AllCards)
		{
			foreach (CardSkinGroup priorityGroup in GetCardLookup(allCard).PriorityGroups)
			{
				if (dictionary.TryGetValue(priorityGroup.Id, out var value))
				{
					value.Add(IndividualCardSelectionKey(allCard));
				}
			}
		}
		foreach (CardSkinPreset preset2 in Config.CardSkinPresets.Where((CardSkinPreset preset) => !string.IsNullOrWhiteSpace(preset.CategoryId)))
		{
			CardSkinGroup group2 = catalog.CardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(preset2.CategoryId, StringComparison.OrdinalIgnoreCase));
			if (group2 == null)
			{
				continue;
			}
			if (preset2.CardSkinPriorities.TryGetValue(group2.Id, out List<CardSkinPriorityEntry> value2))
			{
				preset2.CardSkinPriorities[group2.Id] = value2.Select((CardSkinPriorityEntry entry) => entry with
				{
					OptionId = catalog.ResolveStoredCardSelectionId(group2.Id, entry.OptionId)
				}).DistinctBy<CardSkinPriorityEntry, string>((CardSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
			}
			HashSet<string> selectionKeys = dictionary[group2.Id];
			KeyValuePair<string, string>[] array = preset2.Selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => selectionKeys.Contains(pair.Key)).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<string, string> keyValuePair = array[i];
				if (!keyValuePair.Value.Equals("__inherit__", StringComparison.OrdinalIgnoreCase) && !keyValuePair.Value.Equals("__base__", StringComparison.OrdinalIgnoreCase))
				{
					preset2.Selections[keyValuePair.Key] = catalog.ResolveStoredCardSelectionId(group2.Id, keyValuePair.Value);
				}
			}
		}
	}

	private static void SanitizeMonsterSkinPriorities()
	{
		HashSet<string> knownGroupIds = Catalog.Groups.Select((SkinGroup group) => group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Config.MonsterSkinCategoryGroups = Config.MonsterSkinCategoryGroups.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> pair) => pair.Key, (KeyValuePair<string, List<string>> pair) => pair.Value.Where(knownGroupIds.Contains).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = Config.MonsterSkinCategoryGroups.Keys.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet2 = Config.MonsterSkinCategoryGroups.Values.SelectMany((List<string> groupIds) => groupIds).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Config.MonsterGroupsWithManualSelection = Config.MonsterGroupsWithManualSelection.Where(knownGroupIds.Contains).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		Config.MonsterGroupsFollowingCategory = (from groupId in Config.MonsterGroupsFollowingCategory.Where(hashSet2.Contains)
			where !Config.MonsterGroupsWithManualSelection.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)
			select groupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		if (Config.MonsterPriorityDefaultsVersion < 1)
		{
			foreach (string item in hashSet2.Where((string groupId) => !Config.MonsterGroupsFollowingCategory.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase) && !Config.MonsterGroupsWithManualSelection.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase) && !Config.GetSelection(groupId).Equals("__base__", StringComparison.OrdinalIgnoreCase)))
			{
				Config.MonsterGroupsWithManualSelection.Add(item);
			}
			Config.MonsterPriorityDefaultsVersion = 1;
		}
		if (Config.MonsterPriorityDefaultsVersion < 2)
		{
			foreach (string item2 in hashSet)
			{
				List<MonsterSkinPriorityEntry> list = GetMonsterPriorityEntriesInternal(item2).ToList();
				if (list.Count > 0 && list.All((MonsterSkinPriorityEntry entry) => !entry.Enabled))
				{
					Config.MonsterSkinPriorities[item2] = list.Select((MonsterSkinPriorityEntry entry) => entry with
					{
						Enabled = true
					}).ToList();
				}
			}
			Config.MonsterPriorityDefaultsVersion = 2;
		}
		Config.EnabledMonsterSkinPriorityCategories.Clear();
		Config.MonsterGroupsFollowingCategory = hashSet2.Where((string groupId) => !Config.MonsterGroupsWithManualSelection.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)).ToList();
		foreach (string item3 in hashSet)
		{
			GetMonsterPriorityEntriesInternal(item3);
			ApplyMonsterCategoryPriorityToSelections(item3);
		}
	}

	private static IReadOnlyList<MonsterSkinPriorityEntry> GetMonsterPriorityEntriesInternal(string categoryId)
	{
		IReadOnlyList<MonsterCategoryOptionState> monsterCategoryOptionsInternal = GetMonsterCategoryOptionsInternal(categoryId);
		HashSet<string> knownIds = monsterCategoryOptionsInternal.Select((MonsterCategoryOptionState option) => option.OptionId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<MonsterSkinPriorityEntry> source = (Config.MonsterSkinPriorities.TryGetValue(categoryId, out List<MonsterSkinPriorityEntry> value) ? value : new List<MonsterSkinPriorityEntry>());
		source = source.Select((MonsterSkinPriorityEntry entry) => entry with
		{
			OptionId = ResolveStoredMonsterPriorityOptionId(categoryId, entry.OptionId)
		}).DistinctBy<MonsterSkinPriorityEntry, string>((MonsterSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
		List<MonsterSkinPriorityEntry> entries = source.Where((MonsterSkinPriorityEntry entry) => knownIds.Contains(entry.OptionId)).DistinctBy<MonsterSkinPriorityEntry, string>((MonsterSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
		foreach (MonsterCategoryOptionState item in monsterCategoryOptionsInternal.Where((MonsterCategoryOptionState option) => entries.All((MonsterSkinPriorityEntry entry) => !entry.OptionId.Equals(option.OptionId, StringComparison.OrdinalIgnoreCase))))
		{
			entries.Add(new MonsterSkinPriorityEntry(item.OptionId, Enabled: true));
		}
		Config.MonsterSkinPriorities[categoryId] = MergeKnownMonsterPriorityEntries(source, entries, knownIds);
		return entries;
	}

	private static string ResolveStoredMonsterPriorityOptionId(string categoryId, string optionId)
	{
		SkinCatalog catalog = Catalog;
		if (catalog == null || !Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out List<string> value))
		{
			return optionId;
		}
		foreach (string item in value)
		{
			string text = catalog.ResolveStoredVisualSelectionId(item, optionId);
			if (!text.Equals(optionId, StringComparison.OrdinalIgnoreCase))
			{
				return text;
			}
		}
		return optionId;
	}

	private static IReadOnlyList<MonsterCategoryOptionState> GetMonsterCategoryOptionsInternal(string categoryId)
	{
		SkinCatalog catalog = Catalog;
		if (catalog == null || !Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out List<string> value))
		{
			return Array.Empty<MonsterCategoryOptionState>();
		}
		SkinGroup[] groups = (from groupId in value
			select catalog.Groups.FirstOrDefault((SkinGroup @group) => @group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)) into @group
			where @group != null
			select @group).Cast<SkinGroup>().ToArray();
		return (from option in groups.SelectMany((SkinGroup @group) => @group.Options).DistinctBy<SkinOption, string>((SkinOption option) => option.Id, StringComparer.OrdinalIgnoreCase).ToArray()
			select new MonsterCategoryOptionState(option.Id, option.Name, groups.Count((SkinGroup @group) => @group.Options.Any((SkinOption candidate) => candidate.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase))), groups.Length)).ToArray();
	}

	private static HashSet<string> ApplyMonsterCategoryPriorityToSelections(string categoryId)
	{
		SkinCatalog catalog = Catalog;
		if (!Config.MonsterSkinCategoryGroups.TryGetValue(categoryId, out List<string> value))
		{
			return new HashSet<string>();
		}
		Dictionary<string, string> previousSelections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> workingSelections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
		IReadOnlyList<MonsterSkinPriorityEntry> entries = GetMonsterPriorityEntriesInternal(categoryId);
		HashSet<string> managedGroupIds = value.Where((string groupId) => Config.MonsterGroupsFollowingCategory.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> excludedFullProviders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> desiredSelections;
		do
		{
			desiredSelections = managedGroupIds.ToDictionary<string, string, string>((string groupId) => groupId, delegate(string groupId)
			{
				SkinGroup group = catalog.Groups.First((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
				return entries.FirstOrDefault((MonsterSkinPriorityEntry entry) => entry.Enabled && !excludedFullProviders.Contains(entry.OptionId) && group.Options.Any((SkinOption option) => option.Id.Equals(entry.OptionId, StringComparison.OrdinalIgnoreCase)))?.OptionId ?? "__base__";
			}, StringComparer.OrdinalIgnoreCase);
		}
		while ((from providerId in desiredSelections.Values.Where(catalog.ProviderUsesFullRuntime).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			where catalog.GetFullRuntimeProviderGroups(providerId).Any((string groupId) => !managedGroupIds.Contains(groupId) || !desiredSelections.GetValueOrDefault(groupId, "__base__").Equals(providerId, StringComparison.OrdinalIgnoreCase))
			select providerId).Where(excludedFullProviders.Add).ToArray().Length != 0);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in managedGroupIds)
		{
			string text = desiredSelections[item];
			if (catalog.ProviderUsesFullRuntime(text) && !hashSet.Add(text))
			{
				continue;
			}
			foreach (KeyValuePair<string, string> item2 in catalog.BuildVisualSelectionTransaction(item, text, workingSelections))
			{
				workingSelections[item2.Key] = item2.Value;
			}
		}
		HashSet<string> result = (from groupId in workingSelections.Keys.Union<string>(previousSelections.Keys, StringComparer.OrdinalIgnoreCase)
			where !string.Equals(workingSelections.GetValueOrDefault(groupId), previousSelections.GetValueOrDefault(groupId), StringComparison.OrdinalIgnoreCase)
			select groupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Config.Selections = workingSelections;
		return result;
	}

	private static List<MonsterSkinPriorityEntry> MergeKnownMonsterPriorityEntries(IReadOnlyList<MonsterSkinPriorityEntry> existingEntries, IReadOnlyList<MonsterSkinPriorityEntry> knownEntries, IReadOnlySet<string> knownIds)
	{
		Queue<MonsterSkinPriorityEntry> queue = new Queue<MonsterSkinPriorityEntry>(knownEntries);
		List<MonsterSkinPriorityEntry> list = new List<MonsterSkinPriorityEntry>(Math.Max(existingEntries.Count, knownEntries.Count));
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (MonsterSkinPriorityEntry existingEntry in existingEntries)
		{
			if (knownIds.Contains(existingEntry.OptionId))
			{
				if (queue.Count > 0)
				{
					list.Add(queue.Dequeue());
				}
			}
			else if (hashSet.Add(existingEntry.OptionId))
			{
				list.Add(existingEntry);
			}
		}
		while (queue.Count > 0)
		{
			list.Add(queue.Dequeue());
		}
		return list;
	}

	private static IReadOnlyList<CardSkinPriorityEntry> GetCardPriorityEntriesInternal(CardSkinGroup group, bool enableAllByDefault = false)
	{
		HashSet<string> knownIds = group.Options.Select((CardSkinOption option) => option.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<CardSkinPriorityEntry> source = (Config.CardSkinPriorities.TryGetValue(group.Id, out List<CardSkinPriorityEntry> value) ? value : new List<CardSkinPriorityEntry>());
		source = source.Select((CardSkinPriorityEntry entry) => entry with
		{
			OptionId = Catalog.ResolveStoredCardSelectionId(group.Id, entry.OptionId)
		}).DistinctBy<CardSkinPriorityEntry, string>((CardSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
		List<CardSkinPriorityEntry> entries;
		if (source.Count > 0)
		{
			if (enableAllByDefault)
			{
				source = source.Select((CardSkinPriorityEntry entry) => entry with
				{
					Enabled = true
				}).ToList();
			}
			entries = source.Where((CardSkinPriorityEntry entry) => knownIds.Contains(entry.OptionId)).DistinctBy<CardSkinPriorityEntry, string>((CardSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
			foreach (CardSkinOption item in group.Options.Where((CardSkinOption option) => entries.All((CardSkinPriorityEntry entry) => !entry.OptionId.Equals(option.Id, StringComparison.OrdinalIgnoreCase))))
			{
				entries.Add(new CardSkinPriorityEntry(item.Id, source.Any((CardSkinPriorityEntry entry) => entry.Enabled)));
			}
		}
		else
		{
			string key = CardSelectionKey(group.Id);
			string value2;
			bool hasLegacySelection = Config.Selections.TryGetValue(key, out value2);
			string selectedId = (hasLegacySelection ? Catalog.ResolveStoredCardSelectionId(group.Id, value2) : (group.Options.FirstOrDefault()?.Id ?? "__base__"));
			entries = (from option in @group.Options
				orderby option.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase) descending
				select new CardSkinPriorityEntry(option.Id, enableAllByDefault || !hasLegacySelection || !selectedId.Equals("__base__", StringComparison.OrdinalIgnoreCase))).ToList();
		}
		Config.CardSkinPriorities[group.Id] = MergeKnownCardPriorityEntries(source, entries, knownIds);
		Config.Selections[CardSelectionKey(group.Id)] = entries.FirstOrDefault((CardSkinPriorityEntry entry) => entry.Enabled)?.OptionId ?? "__base__";
		return entries;
	}

	private static List<CardSkinPriorityEntry> MergeKnownCardPriorityEntries(IReadOnlyList<CardSkinPriorityEntry> existingEntries, IReadOnlyList<CardSkinPriorityEntry> knownEntries, IReadOnlySet<string> knownIds)
	{
		Queue<CardSkinPriorityEntry> queue = new Queue<CardSkinPriorityEntry>(knownEntries);
		List<CardSkinPriorityEntry> list = new List<CardSkinPriorityEntry>(Math.Max(existingEntries.Count, knownEntries.Count));
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (CardSkinPriorityEntry existingEntry in existingEntries)
		{
			if (knownIds.Contains(existingEntry.OptionId))
			{
				if (queue.Count > 0)
				{
					list.Add(queue.Dequeue());
				}
			}
			else if (hashSet.Add(existingEntry.OptionId))
			{
				list.Add(existingEntry);
			}
		}
		while (queue.Count > 0)
		{
			list.Add(queue.Dequeue());
		}
		return list;
	}

	private static void InitializeEventSkinCategoriesAfterModels()
	{
		if (Catalog == null)
		{
			return;
		}
		try
		{
			HashSet<string> managed = (from g in Catalog.Groups
				where EventSkinPolicy.IsEventGroup(g.Id) && g.Options.Count > 0
				select g.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			IReadOnlyDictionary<string, string[]> source = EventRegionPolicy.Group(ModelDb.AllEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry), ModelDb.Acts.ToDictionary((ActModel a) => ((AbstractModel)a).Id.Entry, (ActModel a) => a.AllEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry)), ModelDb.AllSharedEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry));
			Dictionary<string, List<string>> registered = (from p in source.ToDictionary<KeyValuePair<string, string[]>, string, List<string>>((KeyValuePair<string, string[]> p) => p.Key, (KeyValuePair<string, string[]> p) => p.Value.Select(EventSkinPolicy.GroupId).Where(managed.Contains).ToList(), StringComparer.OrdinalIgnoreCase)
				where p.Value.Count > 0
				select p).ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> p) => p.Key, (KeyValuePair<string, List<string>> p) => p.Value, StringComparer.OrdinalIgnoreCase);
			if (!ChangeEventPriorityConfiguration(delegate
			{
				Config.EventSkinPriorities.Register(registered, Config.Selections);
			}))
			{
				ModLog.Warn("初始化事件地区优先级失败：" + LastError);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("无法登记事件地区优先级，保留当前事件皮肤选择：" + ex.GetBaseException().Message);
		}
	}

	private static SkinGroup[] GetEventRegionGroups(string region)
	{
		if (!Config.EventSkinPriorities.RegionGroups.TryGetValue(region, out List<string> ids) || Catalog == null)
		{
			return Array.Empty<SkinGroup>();
		}
		return Catalog.Groups.Where((SkinGroup g) => EventSkinPolicy.IsEventGroup(g.Id) && ids.Contains<string>(g.Id, StringComparer.OrdinalIgnoreCase)).ToArray();
	}

	private static List<EventSkinPriorityEntry> GetEventPriorityEntries(string region)
	{
		return EventSkinPriorityPolicy.Entries(Config.EventSkinPriorities.Priorities.GetValueOrDefault(region) ?? new List<EventSkinPriorityEntry>(), from o in GetEventRegionGroups(region).SelectMany((SkinGroup g) => g.Options)
			select o.Id);
	}

	public static IReadOnlyList<EventPriorityOptionState> GetEventPriorityOptions(string region)
	{
		lock (Sync)
		{
			SkinGroup[] groups = GetEventRegionGroups(region);
			Dictionary<string, SkinOption> options = groups.SelectMany((SkinGroup g) => g.Options).DistinctBy<SkinOption, string>((SkinOption o) => o.Id, StringComparer.OrdinalIgnoreCase).ToDictionary<SkinOption, string>((SkinOption o) => o.Id, StringComparer.OrdinalIgnoreCase);
			return (from e in GetEventPriorityEntries(region)
				where options.ContainsKey(e.OptionId)
				select new EventPriorityOptionState(e.OptionId, options[e.OptionId].Name, e.Enabled, groups.Count((SkinGroup g) => g.Options.Any((SkinOption o) => o.Id.Equals(e.OptionId, StringComparison.OrdinalIgnoreCase))), groups.Length)).ToArray();
		}
	}

	public static bool SetEventPriorityOptionEnabled(string region, string optionId, bool enabled)
	{
		return EditEventPriority(region, optionId, (List<EventSkinPriorityEntry> entries) => entries.Select((EventSkinPriorityEntry e) => (!e.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)) ? e : e with
		{
			Enabled = enabled
		}).ToList());
	}

	public static bool MoveEventPriority(string region, string optionId, int offset)
	{
		return EditEventPriority(region, optionId, delegate(List<EventSkinPriorityEntry> entries)
		{
			List<string> visible = (from o in GetEventPriorityOptions(region)
				select o.OptionId).ToList();
			int num = visible.FindIndex((string id) => id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			int target = Math.Clamp(num + offset, 0, visible.Count - 1);
			if (target == num)
			{
				return entries;
			}
			int index = entries.FindIndex((EventSkinPriorityEntry e) => e.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			int index2 = entries.FindIndex((EventSkinPriorityEntry e) => e.OptionId.Equals(visible[target], StringComparison.OrdinalIgnoreCase));
			EventSkinPriorityEntry item = entries[index];
			entries.RemoveAt(index);
			entries.Insert(index2, item);
			return entries;
		});
	}

	private static bool EditEventPriority(string region, string optionId, Func<List<EventSkinPriorityEntry>, List<EventSkinPriorityEntry>> edit)
	{
		lock (Sync)
		{
			if (!GetEventPriorityOptions(region).Any((EventPriorityOptionState o) => o.OptionId.Equals(optionId, StringComparison.OrdinalIgnoreCase)))
			{
				LastError = "未知的事件皮肤选项：" + region + "/" + optionId;
				return false;
			}
			return ChangeEventPriorityConfiguration(delegate
			{
				Config.EventSkinPriorities.Priorities[region] = edit(GetEventPriorityEntries(region));
			});
		}
	}

	public static bool HasEventSkinCategory(string groupId)
	{
		return Config.EventSkinPriorities.RegionGroups.Values.Any((List<string> ids) => ids.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase));
	}

	public static string GetEventOverrideSelection(string groupId)
	{
		if (!Config.EventSkinPriorities.IsFollowing(groupId))
		{
			return Config.GetSelection(groupId);
		}
		return "__event_category__";
	}

	public static bool FollowEventCategoryPriority(string groupId)
	{
		lock (Sync)
		{
			if (!HasEventSkinCategory(groupId))
			{
				LastError = "事件 " + groupId + " 尚未登记地区。";
				return false;
			}
			return ChangeEventPriorityConfiguration(delegate
			{
				Config.EventSkinPriorities.ManualGroups.RemoveAll((string id) => id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			});
		}
	}

	private static bool ChangeEventPriorityConfiguration(Action mutation)
	{
		if (Catalog == null)
		{
			LastError = "皮肤目录尚未初始化。";
			return false;
		}
		EventSkinPrioritySettings eventSkinPriorities = Config.EventSkinPriorities.Clone();
		Dictionary<string, string> selections = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
		List<string> visualProviderPriority = Config.VisualProviderPriority.ToList();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		try
		{
			mutation();
			foreach (KeyValuePair<string, string> item in BuildEventPrioritySelectionUpdates())
			{
				Config.Selections[item.Key] = item.Value;
				hashSet.Add(item.Key);
			}
			foreach (string item2 in hashSet)
			{
				ClearRuntimeResourceCache(item2);
				UpdateVisualProviderPriority(item2, Config.GetSelection(item2));
			}
			if (hashSet.Count > 0)
			{
				MountOverlay(hashSet);
			}
			Config.Save(ConfigPath);
			foreach (string item3 in hashSet)
			{
				EventSkinRuntime.RefreshCurrent(item3);
			}
			LastError = null;
			return true;
		}
		catch (Exception ex)
		{
			Config.EventSkinPriorities = eventSkinPriorities;
			Config.Selections = selections;
			Config.VisualProviderPriority = visualProviderPriority;
			foreach (string item4 in hashSet)
			{
				ClearRuntimeResourceCache(item4);
			}
			if (hashSet.Count > 0)
			{
				TryRestoreOverlay(hashSet, cardOverlay: false);
			}
			LastError = ex.Message;
			ModLog.Error("调整事件地区优先级失败：" + ex);
			return false;
		}
	}

	private static IReadOnlyDictionary<string, string> BuildEventPrioritySelectionUpdates()
	{
		SkinCatalog catalog = Catalog;
		Dictionary<string, (SkinGroup, List<EventSkinPriorityEntry>)> dictionary = new Dictionary<string, (SkinGroup, List<EventSkinPriorityEntry>)>(StringComparer.OrdinalIgnoreCase);
		foreach (string key in Config.EventSkinPriorities.RegionGroups.Keys)
		{
			List<EventSkinPriorityEntry> eventPriorityEntries = GetEventPriorityEntries(key);
			Config.EventSkinPriorities.Priorities[key] = eventPriorityEntries;
			foreach (SkinGroup item in from g in GetEventRegionGroups(key)
				where Config.EventSkinPriorities.IsFollowing(g.Id)
				select g)
			{
				dictionary.TryAdd(item.Id, (item, eventPriorityEntries));
			}
		}
		HashSet<string> excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> desired;
		string value;
		do
		{
			desired = dictionary.ToDictionary<KeyValuePair<string, (SkinGroup, List<EventSkinPriorityEntry>)>, string, string>((KeyValuePair<string, (SkinGroup Group, List<EventSkinPriorityEntry> Entries)> p) => p.Key, (KeyValuePair<string, (SkinGroup Group, List<EventSkinPriorityEntry> Entries)> p) => EventSkinPriorityPolicy.Resolve(p.Value.Entries, from o in p.Value.Group.Options
				where !excluded.Contains(catalog.ResolveVisualProviderId(o.Id))
				select o.Id), StringComparer.OrdinalIgnoreCase);
		}
		while ((from provider in desired.Values.Select(catalog.ResolveVisualProviderId).Where(catalog.ProviderUsesFullRuntime).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			where catalog.GetFullRuntimeProviderGroups(provider).Any((string id) => !desired.TryGetValue(id, out value) || !catalog.ResolveVisualProviderId(value).Equals(provider, StringComparison.OrdinalIgnoreCase))
			select provider).Where(excluded.Add).ToArray().Length != 0);
		Dictionary<string, string> dictionary2 = new Dictionary<string, string>(Config.Selections, StringComparer.OrdinalIgnoreCase);
		foreach (var (text3, text4) in desired)
		{
			if (dictionary2.GetValueOrDefault(text3, "__base__").Equals(text4, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			IReadOnlyDictionary<string, string> readOnlyDictionary = catalog.BuildVisualSelectionTransaction(text3, text4, dictionary2);
			if (readOnlyDictionary.Keys.Any((string key) => !EventSkinPolicy.IsEventGroup(key)))
			{
				throw new InvalidOperationException("该事件皮肤要求同时修改其它外观组，不能按事件地区批量应用。");
			}
			foreach (KeyValuePair<string, string> item2 in readOnlyDictionary)
			{
				if (item2.Key != text3 && (!desired.TryGetValue(item2.Key, out string value2) || value2 != item2.Value))
				{
					throw new InvalidOperationException("该事件皮肤无法独立于其它事件切换。");
				}
				dictionary2[item2.Key] = item2.Value;
			}
		}
		return dictionary2.Where((KeyValuePair<string, string> p) => !Config.GetSelection(p.Key).Equals(p.Value, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> p) => p.Key, (KeyValuePair<string, string> p) => p.Value, StringComparer.OrdinalIgnoreCase);
	}

	internal static int MigrateLegacyManualSlots(SkinConfig config, SkinCatalog catalog)
	{
		int num = 0;
		SlotVisibilitySelection[] array = config.SlotVisibilitySelections.ToArray();
		foreach (SlotVisibilitySelection legacy in array)
		{
			SkinOption[] source = catalog.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(legacy.GroupId, StringComparison.OrdinalIgnoreCase))?.Options.Where(delegate(SkinOption option)
			{
				if (!option.IsComposition && option.EffectiveProviderId.Equals(legacy.ProviderId, StringComparison.OrdinalIgnoreCase))
				{
					ManualCharacterVariant manualCharacterVariant = option.ManualCharacterVariant;
					if ((object)manualCharacterVariant != null)
					{
						ManualCharacterState state = manualCharacterVariant.State;
						if ((object)state != null && !state.IsStatic && manualCharacterVariant.State.Id == legacy.ToggleId)
						{
							return manualCharacterVariant.State.SourceSlots.ToHashSet<string>(StringComparer.Ordinal).SetEquals(legacy.SourceSlots);
						}
					}
				}
				return false;
			}).ToArray() ?? Array.Empty<SkinOption>();
			SkinOption skinOption = source.FirstOrDefault((SkinOption option) => !option.ManualCharacterVariant.Value);
			SkinOption skinOption2 = source.FirstOrDefault((SkinOption option) => option.ManualCharacterVariant.Value == legacy.Hidden);
			if (!(skinOption == null) && !(skinOption2 == null))
			{
				if (config.GetSelection(legacy.GroupId).Equals(skinOption.Id, StringComparison.OrdinalIgnoreCase))
				{
					config.Selections[legacy.GroupId] = skinOption2.Id;
				}
				config.SlotVisibilitySelections.Remove(legacy);
				num++;
			}
		}
		return num;
	}

	internal static ManualCharacterVariant? GetSelectedManualCharacterVariant(string groupId)
	{
		lock (Sync)
		{
			return Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(GetVisualSelection(groupId), StringComparison.OrdinalIgnoreCase))?.ManualCharacterVariant;
		}
	}

	internal static bool IsSameManualVariantSource(string groupId, string optionId)
	{
		lock (Sync)
		{
			SkinGroup? obj = Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			SkinOption skinOption = obj?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(Config.GetSelection(groupId), StringComparison.OrdinalIgnoreCase));
			SkinOption skinOption2 = obj?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			ManualCharacterVariant manualCharacterVariant = skinOption?.ManualCharacterVariant;
			int result;
			if ((object)manualCharacterVariant != null)
			{
				ManualCharacterVariant manualCharacterVariant2 = skinOption2?.ManualCharacterVariant;
				if ((object)manualCharacterVariant2 != null && !skinOption.IsComposition && !skinOption2.IsComposition && skinOption.EffectiveProviderId == skinOption2.EffectiveProviderId && manualCharacterVariant.State.Id == manualCharacterVariant2.State.Id)
				{
					result = ((skinOption.Assets == skinOption2.Assets) ? 1 : 0);
					goto IL_0100;
				}
			}
			result = 0;
			goto IL_0100;
			IL_0100:
			return (byte)result != 0;
		}
	}

	private static string ManualVariantResourceSelection(string groupId, string selection)
	{
		SkinGroup skinGroup = Catalog?.Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		SkinOption option2 = skinGroup?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
		ManualCharacterVariant variant = option2?.ManualCharacterVariant;
		if ((object)variant == null || option2.IsComposition)
		{
			return selection;
		}
		return skinGroup.Options.FirstOrDefault(delegate(SkinOption candidate)
		{
			if (!candidate.IsComposition && candidate.EffectiveProviderId == option2.EffectiveProviderId && candidate.Assets == option2.Assets)
			{
				ManualCharacterVariant manualCharacterVariant = candidate.ManualCharacterVariant;
				if ((object)manualCharacterVariant != null && !manualCharacterVariant.Value)
				{
					return manualCharacterVariant.State.Id == variant.State.Id;
				}
			}
			return false;
		})?.Id ?? selection;
	}

	internal static bool IsCharacterSkinExcludedFromRandom(string groupId, string optionId)
	{
		if (Config.RandomCharacterSkinExclusions.TryGetValue(groupId, out List<string> value))
		{
			return value.Contains<string>(optionId, StringComparer.OrdinalIgnoreCase);
		}
		return false;
	}

	internal static bool ToggleCharacterSkinRandomExclusion(string groupId, string optionId)
	{
		if (string.IsNullOrWhiteSpace(groupId) || !RandomCharacterSkinPolicy.IsCandidate(optionId))
		{
			return false;
		}
		lock (Sync)
		{
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			if (!skinConfig.RandomCharacterSkinExclusions.TryGetValue(groupId, out List<string> value))
			{
				value = (skinConfig.RandomCharacterSkinExclusions[groupId] = new List<string>());
			}
			if (value.RemoveAll((string id) => id.Equals(optionId, StringComparison.OrdinalIgnoreCase)) == 0)
			{
				value.Add(optionId);
			}
			if (value.Count == 0)
			{
				skinConfig.RandomCharacterSkinExclusions.Remove(groupId);
			}
			return CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	internal static IEnumerable<string> FilterRandomCharacterSkinCandidates(string groupId, IEnumerable<string> options)
	{
		return options.Where((string id) => !IsCharacterSkinExcludedFromRandom(groupId, id));
	}

	internal static bool IsRandomCharacterSkinEnabled(string groupId)
	{
		return Config.RandomCharacterSkinGroups.Contains<string>(groupId, StringComparer.OrdinalIgnoreCase);
	}

	internal static bool SetRandomCharacterSkinEnabled(string groupId, bool enabled)
	{
		lock (Sync)
		{
			if (IsRandomCharacterSkinEnabled(groupId) == enabled)
			{
				return true;
			}
			SkinConfig skinConfig = Config.CloneForBundleTransaction();
			skinConfig.RandomCharacterSkinGroups.RemoveAll((string id) => id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
			if (enabled)
			{
				skinConfig.RandomCharacterSkinGroups.Add(groupId);
				skinConfig.ActiveCharacterSkinBundles.Remove(groupId);
			}
			return CommitBundleConfiguration(skinConfig, delegate
			{
			}, delegate
			{
			}, delegate
			{
			});
		}
	}

	internal static void RestoreRandomCharacterSkinsForLobby()
	{
		lock (Sync)
		{
			if (_characterSkinBundleRunState != null || _characterSkinBundleRunSnapshot != null)
			{
				return;
			}
			string[] array = Config.RandomCharacterSkinGroups.ToArray();
			foreach (string text in array)
			{
				SkinCatalog? catalog = Catalog;
				if (catalog == null || !catalog.IsCharacterAppearanceGroup(text))
				{
					continue;
				}
				string selection = Config.GetSelection(text);
				if (!selection.Equals("__base__", StringComparison.OrdinalIgnoreCase))
				{
					if (!ApplySelection(text, "__base__"))
					{
						ModLog.Warn("选角恢复随机皮肤的原皮失败，保留现状：" + text + "；" + LastError);
						continue;
					}
					ModLog.Info($"选角随机皮肤已恢复原皮：{text}，上一局来源={selection}；独立对局记录保持不变。");
				}
				if (!ClearSelectedCharacterSkinBundle(text))
				{
					ModLog.Warn("清除上局随机皮肤包标记失败：" + text + "；" + LastError);
				}
			}
		}
	}

	private static void ApplyRandomCharacterSkinForNewRun(string groupId)
	{
		IEnumerable<string> options = new string[1] { "__base__" }.Concat(from option in GetCharacterSkinOptions(groupId)
			select option.Id).Concat(from bundle in GetCharacterSkinBundles(groupId)
			where GetCharacterSkinBundleCharacterOption(groupId, bundle.Name) != null
			select CharacterSkinBundlePolicy.CreateSelectionOptionId(bundle.Name));
		string text = RandomCharacterSkinPolicy.Draw(FilterRandomCharacterSkinCandidates(groupId, options), Random.Shared.Next);
		string name;
		bool flag = CharacterSkinBundlePolicy.TryGetSelectionBundleName(text, out name);
		string text2 = (flag ? GetCharacterSkinBundleCharacterOption(groupId, name) : text);
		if (!ClearSelectedCharacterSkinBundle(groupId))
		{
			return;
		}
		if (text2 == null || !ApplySelection(groupId, text2))
		{
			ModLog.Warn($"随机皮肤 {groupId}/{text} 加载失败，回退原皮：{LastError}");
			ApplySelection(groupId, "__base__");
			return;
		}
		if (flag && SelectCharacterSkinBundle(groupId, name))
		{
			if (!ApplySelectedCharacterSkinBundleForRun(groupId, out IReadOnlyList<string> warnings))
			{
				ModLog.Warn("随机皮肤包预设应用失败：" + LastError);
			}
			foreach (string item in warnings)
			{
				ModLog.Warn("随机皮肤包：" + item);
			}
		}
		ModLog.Info($"本局随机皮肤：{groupId} -> {text}，实际角色来源={Config.GetSelection(groupId)}。");
	}

	private static void ResumeRandomCharacterSkin(CharacterSkinBundleRunState record)
	{
		string randomCharacterOptionId = record.RandomCharacterOptionId;
		if (randomCharacterOptionId != null)
		{
			string optionId = Catalog?.ResolveStoredVisualSelectionId(record.CharacterGroupId, randomCharacterOptionId) ?? "__base__";
			if (!ApplySelection(record.CharacterGroupId, optionId))
			{
				ModLog.Warn("本局保存的随机皮肤 " + randomCharacterOptionId + " 已不可用，回退原皮，不重新抽取。");
				ApplySelection(record.CharacterGroupId, "__base__");
			}
		}
	}

	internal static SlotVisibilitySelection[] GetSlotVisibilitySelections(string groupId, string providerId)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			ManualCharacterVariant selectedManualCharacterVariant = GetSelectedManualCharacterVariant(groupId);
			if ((object)selectedManualCharacterVariant != null)
			{
				ManualCharacterState state = selectedManualCharacterVariant.State;
				if ((object)state != null && !state.IsStatic)
				{
					string? selectedRuntimeProvider = GetSelectedRuntimeProvider(groupId);
					if (selectedRuntimeProvider != null && selectedRuntimeProvider.Equals(providerId, StringComparison.OrdinalIgnoreCase))
					{
						return new SlotVisibilitySelection[1]
						{
							new SlotVisibilitySelection(groupId, providerId, selectedManualCharacterVariant.State.Id, selectedManualCharacterVariant.Value, selectedManualCharacterVariant.State.SourceSlots)
						};
					}
				}
			}
			return SlotVisibilityPolicy.GetSelections(Config, groupId, providerId);
		}
	}

	internal static void SaveSlotVisibilitySelection(SlotVisibilitySelection selection)
	{
		lock (Sync)
		{
			EnsureConfigLoaded();
			Config.SlotVisibilitySelections.RemoveAll((SlotVisibilitySelection state) => state.GroupId.Equals(selection.GroupId, StringComparison.OrdinalIgnoreCase) && state.ProviderId.Equals(selection.ProviderId, StringComparison.OrdinalIgnoreCase) && state.ToggleId == selection.ToggleId);
			Config.SlotVisibilitySelections.Add(selection);
			Config.Save(ConfigPath);
		}
	}

	internal static async Task<T> RunWorkshopScan<T>(Func<T> scan, CancellationToken token)
	{
		await WorkshopRegistrationGate.WaitAsync(token);
		try
		{
			return await Task.Run(scan, token);
		}
		finally
		{
			WorkshopRegistrationGate.Release();
		}
	}

	internal static (string GamePack, CardCatalogEntry[] Cards, SkinModDescriptor[] Baselines) CaptureWorkshopScanContext()
	{
		lock (Sync)
		{
			if (Catalog == null || _workshopGamePack == null)
			{
				throw new InvalidOperationException("Skin catalog is not ready.");
			}
			return (GamePack: _workshopGamePack, Cards: BuildCardCatalogEntries(ModelDb.AllCards), Baselines: _workshopDescriptors.Where((SkinModDescriptor m) => m.AffectsGameplay).ToArray());
		}
	}

	internal static async Task<WorkshopLoadReason> TryRegisterWorkshopResources(string directory)
	{
		await WorkshopRegistrationGate.WaitAsync();
		SkinCatalog staged = null;
		try
		{
			SkinCatalog baseline;
			SkinModDescriptor[] descriptors;
			string gamePack;
			CardCatalogEntry[] cards;
			lock (Sync)
			{
				baseline = Catalog;
				descriptors = _workshopDescriptors;
				gamePack = _workshopGamePack;
				if (baseline == null || gamePack == null)
				{
					return WorkshopLoadReason.ChangedFiles;
				}
				if (!WorkshopSourcesUnchanged())
				{
					return WorkshopLoadReason.ChangedFiles;
				}
				cards = BuildCardCatalogEntries(ModelDb.AllCards);
			}
			string gameVersion = (AccessTools.Field(typeof(ModManager), "_gameVersion")?.GetValue(null))?.ToString();
			Dictionary<string, string> loaded = ModManager.Mods.Where((Mod mod) => (int)mod.state == 1 && mod.manifest?.id != null).GroupBy<Mod, string>((Mod mod) => mod.manifest.id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, Mod>, string, string>((IGrouping<string, Mod> group) => group.Key, (IGrouping<string, Mod> group) => group.First().manifest.version ?? "", StringComparer.OrdinalIgnoreCase);
			Dictionary<string, (long Length, DateTime Modified)> before = await Task.Run(() => WorkshopPackagePolicy.Snapshot(directory));
			WorkshopPackageAssessment workshopPackageAssessment = await Task.Run(() => WorkshopPackagePolicy.Assess(directory, gameVersion, loaded));
			if (!workshopPackageAssessment.CanInspectResources)
			{
				return workshopPackageAssessment.Reason;
			}
			SkinModDescriptor[] additions = workshopPackageAssessment.Mods;
			if (additions.Any((SkinModDescriptor mod) => descriptors.Any((SkinModDescriptor old) => old.Id.Equals(mod.Id, StringComparison.OrdinalIgnoreCase))))
			{
				return WorkshopLoadReason.DuplicateId;
			}
			SkinModDescriptor[] combined = descriptors.Concat(additions).ToArray();
			staged = await Task.Run(delegate
			{
				SkinCatalog candidate = SkinCatalog.Build(gamePack, combined);
				try
				{
					candidate.FinalizeCardGroups(cards);
					if (additions.All((SkinModDescriptor mod) => candidate.HasCompleteWorkshopResourceCoverage(mod.Id)))
					{
						return candidate;
					}
					candidate.Dispose();
					return (SkinCatalog?)null;
				}
				catch
				{
					candidate.Dispose();
					throw;
				}
			});
			if (staged == null)
			{
				return WorkshopLoadReason.IncompleteResources;
			}
			if (!(await Task.Run(() => WorkshopPackagePolicy.Unchanged(directory, before))))
			{
				return WorkshopLoadReason.ChangedFiles;
			}
			lock (Sync)
			{
				if (Catalog != baseline || !WorkshopSourcesUnchanged())
				{
					return WorkshopLoadReason.ChangedFiles;
				}
				staged.SynchronizeCharacterSkinCompositions(Config.CharacterSkinCompositions);
				SkinConfig config = Config;
				SkinConfig skinConfig = Config.CloneForBundleTransaction();
				Dictionary<string, (long, DateTime)> workshopSourceStamps = WorkshopSourceStamps(combined);
				Config = skinConfig;
				try
				{
					HashSet<string> providerIds = additions.Select((SkinModDescriptor mod) => mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
					foreach (CardSkinGroup group2 in staged.CardGroups)
					{
						string[] array = (from o in group2.Options
							where providerIds.Contains(o.ProviderId ?? o.Id)
							select o.Id).Distinct().ToArray();
						if (array.Length == 0)
						{
							continue;
						}
						object obj2 = skinConfig.CardSkinPriorities.GetValueOrDefault(group2.Id);
						if (obj2 == null)
						{
							CardSkinGroup cardSkinGroup = baseline.CardGroups.FirstOrDefault((CardSkinGroup g) => g.Id == group2.Id);
							obj2 = ((cardSkinGroup != null) ? GetCardPriorityEntriesInternal(cardSkinGroup).ToList() : new List<CardSkinPriorityEntry>());
						}
						List<CardSkinPriorityEntry> entries = (List<CardSkinPriorityEntry>)obj2;
						skinConfig.CardSkinPriorities[group2.Id] = entries.Concat(from id in array
							where entries.All((CardSkinPriorityEntry e) => e.OptionId != id)
							select new CardSkinPriorityEntry(id, Enabled: false)).ToList();
					}
					foreach (ActModel act in ModelDb.Acts)
					{
						string text = "act:" + ((AbstractModel)act).Id.Entry.ToLowerInvariant();
						IReadOnlyList<string> ids = ResolveMonsterSkinGroupIds(staged, act.AllMonsters);
						if (ids.Count == 0)
						{
							continue;
						}
						List<MonsterSkinPriorityEntry> entries2 = GetMonsterPriorityEntriesInternal(text).ToList();
						IEnumerable<string> source = (from o in staged.Groups.Where((SkinGroup g) => ids.Contains(g.Id)).SelectMany((SkinGroup g) => g.Options)
							where providerIds.Contains(o.EffectiveProviderId)
							select o.Id).Distinct();
						skinConfig.MonsterSkinPriorities[text] = entries2.Concat(from id in source
							where entries2.All((MonsterSkinPriorityEntry e) => e.OptionId != id)
							select new MonsterSkinPriorityEntry(id, Enabled: false)).ToList();
						skinConfig.MonsterSkinCategoryGroups[text] = ids.ToList();
					}
					IReadOnlyDictionary<string, string[]> source2 = EventRegionPolicy.Group(ModelDb.AllEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry), ModelDb.Acts.ToDictionary((ActModel a) => ((AbstractModel)a).Id.Entry, (ActModel a) => a.AllEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry)), ModelDb.AllSharedEvents.Select((EventModel e) => ((AbstractModel)e).Id.Entry));
					HashSet<string> managedEvents = (from g in staged.Groups
						where EventSkinPolicy.IsEventGroup(g.Id) && g.Options.Count > 0
						select g.Id).ToHashSet();
					Dictionary<string, List<string>> dictionary = source2.ToDictionary<KeyValuePair<string, string[]>, string, List<string>>((KeyValuePair<string, string[]> p) => p.Key, (KeyValuePair<string, string[]> p) => p.Value.Select(EventSkinPolicy.GroupId).Where(managedEvents.Contains).ToList());
					foreach (KeyValuePair<string, List<string>> item in dictionary)
					{
						item.Deconstruct(out var key, out var value);
						string text2 = key;
						List<string> ids2 = value;
						List<EventSkinPriorityEntry> entries3 = GetEventPriorityEntries(text2);
						IEnumerable<string> source3 = (from o in staged.Groups.Where((SkinGroup g) => ids2.Contains(g.Id)).SelectMany((SkinGroup g) => g.Options)
							where providerIds.Contains(o.EffectiveProviderId)
							select o.Id).Distinct();
						skinConfig.EventSkinPriorities.Priorities[text2] = entries3.Concat(from id in source3
							where entries3.All((EventSkinPriorityEntry e) => e.OptionId != id)
							select new EventSkinPriorityEntry(id, Enabled: false)).ToList();
					}
					skinConfig.EventSkinPriorities.Register(dictionary, skinConfig.Selections);
					skinConfig.Save(ConfigPath);
				}
				catch
				{
					Config = config;
					throw;
				}
				Catalog = staged;
				Config = skinConfig;
				staged = null;
				_workshopDescriptors = combined;
				_workshopSourceStamps = workshopSourceStamps;
				_cardLookupCache = new ConditionalWeakTable<CardModel, CardLookup>();
				CardCoverageCache.Clear();
			}
			return WorkshopLoadReason.None;
		}
		finally
		{
			staged?.Dispose();
			WorkshopRegistrationGate.Release();
		}
	}

	private static Dictionary<string, (long Length, DateTime Modified)> WorkshopSourceStamps(IEnumerable<SkinModDescriptor> mods)
	{
		return (from mod in mods
			where mod.PckPath != null && File.Exists(mod.PckPath)
			select mod.PckPath).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToDictionary<string, string, (long, DateTime)>((string path) => path, delegate(string path)
		{
			FileInfo fileInfo = new FileInfo(path);
			return (Length: fileInfo.Length, LastWriteTimeUtc: fileInfo.LastWriteTimeUtc);
		}, StringComparer.OrdinalIgnoreCase);
	}

	private static bool WorkshopSourcesUnchanged()
	{
		return _workshopSourceStamps.All<KeyValuePair<string, (long, DateTime)>>(delegate(KeyValuePair<string, (long Length, DateTime Modified)> pair)
		{
			if (File.Exists(pair.Key))
			{
				FileInfo fileInfo = new FileInfo(pair.Key);
				if (fileInfo != null)
				{
					long length = fileInfo.Length;
					DateTime lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
					(long, DateTime) value = pair.Value;
					if (length == value.Item1)
					{
						return lastWriteTimeUtc == value.Item2;
					}
					return false;
				}
			}
			return false;
		});
	}
}

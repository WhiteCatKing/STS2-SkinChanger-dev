using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

namespace STS2SkinChanger.Catalog;

internal sealed class SkinCatalog : IDisposable
{
	private sealed record GroupIdentity(string Id, string DisplayName);

	private sealed record RuntimeProviderAsset(GroupIdentity Identity, string CanonicalPath);

	private sealed record CardArtPathLayout(string Category, string Variant);

	private sealed record CardArtIdentity(string Category, string Stem);

	private sealed record CardArtVariant(string Key, ResourceAsset[] Assets, HashSet<string> Stems);

	private sealed record RuntimeResource(string SourcePath, ResourceFile? DirectFile, ResourceFile? RemapFile, IReadOnlyList<ResourceFile> PayloadFiles);

	private sealed class EventTables
	{
		public Dictionary<string, Dictionary<string, string>> Languages { get; } = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
	}

	[CompilerGenerated]
	private sealed class _003CGetSiblingAtlasTextureAssets_003Ed__152 : IEnumerable<ResourceAsset>, IEnumerable, IEnumerator<ResourceAsset>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private ResourceAsset _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private string atlasSourcePath;

		public string _003C_003E3__atlasSourcePath;

		private PckResourceIndex index;

		public PckResourceIndex _003C_003E3__index;

		private string _003Cdirectory_003E5__2;

		private IEnumerator<string> _003C_003E7__wrap2;

		ResourceAsset IEnumerator<ResourceAsset>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetSiblingAtlasTextureAssets_003Ed__152(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Cdirectory_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					if (!atlasSourcePath.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase) && !atlasSourcePath.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					int num = atlasSourcePath.LastIndexOf('/');
					if (num < 0)
					{
						return false;
					}
					_003Cdirectory_003E5__2 = atlasSourcePath.Substring(0, num + 1);
					IEnumerable<string> enumerable = (from path in index.Assets.Keys.Concat<string>(index.Archive.Paths.Select(NormalizeAtlasTextureCandidatePath))
						where path != null
						select path).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase);
					_003C_003E7__wrap2 = enumerable.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				}
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap2.MoveNext())
				{
					string current = _003C_003E7__wrap2.Current;
					if (!IsAtlasTextureSourcePath(current) || !current.StartsWith(_003Cdirectory_003E5__2, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					string text = current;
					int length = _003Cdirectory_003E5__2.Length;
					if (!text.Substring(length, text.Length - length).Contains('/'))
					{
						ResourceAsset resourceAsset = index.Assets.GetValueOrDefault(current) ?? index.TryBuildAsset(current);
						if (resourceAsset != null)
						{
							_003C_003E2__current = resourceAsset;
							_003C_003E1__state = 1;
							return true;
						}
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap2 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<ResourceAsset> IEnumerable<ResourceAsset>.GetEnumerator()
		{
			_003CGetSiblingAtlasTextureAssets_003Ed__152 _003CGetSiblingAtlasTextureAssets_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetSiblingAtlasTextureAssets_003Ed__ = this;
			}
			else
			{
				_003CGetSiblingAtlasTextureAssets_003Ed__ = new _003CGetSiblingAtlasTextureAssets_003Ed__152(0);
			}
			_003CGetSiblingAtlasTextureAssets_003Ed__.index = _003C_003E3__index;
			_003CGetSiblingAtlasTextureAssets_003Ed__.atlasSourcePath = _003C_003E3__atlasSourcePath;
			return _003CGetSiblingAtlasTextureAssets_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<ResourceAsset>)this).GetEnumerator();
		}
	}

	public const string BaseOptionId = "__base__";

	private static readonly (string Owner, string Companion)[] CompanionGroups = new(string, string)[1] { ("necrobinder", "osty") };

	private static readonly JsonSerializerOptions CardReplacementJsonOptions = new JsonSerializerOptions
	{
		PropertyNameCaseInsensitive = true
	};

	private static readonly object RuntimeAncientImageCacheSync = new object();

	private static readonly ConditionalWeakTable<PckResourceIndex, Dictionary<string, IReadOnlyCollection<ResourceFile>>> ProviderNamespaceFiles = new ConditionalWeakTable<PckResourceIndex, Dictionary<string, IReadOnlyCollection<ResourceFile>>>();

	private static readonly Dictionary<string, RuntimeAncientImageCacheEntry> RuntimeAncientImageCache = new Dictionary<string, RuntimeAncientImageCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly object ManagedScriptCountCacheSync = new object();

	private static readonly Dictionary<string, ManagedScriptCountCacheEntry> ManagedScriptCountCache = new Dictionary<string, ManagedScriptCountCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private static readonly object CharacterVisualPatchCacheSync = new object();

	private static readonly Dictionary<string, CharacterVisualPatchCacheEntry> CharacterVisualPatchCache = new Dictionary<string, CharacterVisualPatchCacheEntry>(StringComparer.OrdinalIgnoreCase);

	private readonly PckArchive _gameArchive;

	private readonly List<PckResourceIndex> _baselineIndexes;

	private readonly List<PckResourceIndex> _cosmeticIndexes;

	private readonly List<SkinGroup> _groups;

	private readonly IReadOnlyList<CardSkinGroup> _configuredCardGroups;

	private readonly IReadOnlyList<CardSkinOption> _pckCardOptions;

	private readonly List<CardSkinGroup> _cardGroups;

	private readonly Dictionary<string, CardSkinGroup> _artPackCardGroups = new Dictionary<string, CardSkinGroup>(StringComparer.OrdinalIgnoreCase);

	private readonly IReadOnlySet<string> _managedGodotScriptProviders;

	private readonly IReadOnlySet<string> _cosmeticLocalizationProviders;

	private readonly IReadOnlySet<string> _cosmeticLocalizationPaths;

	private readonly IReadOnlyDictionary<string, ResourceFile> _passthroughLocalizationFiles;

	private readonly IReadOnlySet<string> _fullRuntimeProviders;

	private readonly IReadOnlySet<string> _directCharacterRuntimeProviders;

	private readonly IReadOnlySet<string> _scopedMonsterRuntimeProviders;

	private readonly IReadOnlySet<string> _interactiveRuntimeProviders;

	private readonly IReadOnlySet<string> _characterAppearanceGroupIds;

	private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _fullRuntimeProviderGroups;

	private readonly IReadOnlyDictionary<string, IReadOnlyList<string>> _scopedMonsterRuntimeProviderGroups;

	private readonly IReadOnlyDictionary<string, string> _resourceGroupIds;

	private readonly IReadOnlyList<ProviderInstanceIdentity> _providerInstanceIdentities;

	private readonly Dictionary<string, IReadOnlyDictionary<string, ResourceFile>> _fullRuntimeProviderBaselineOverlays = new Dictionary<string, IReadOnlyDictionary<string, ResourceFile>>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, IReadOnlyDictionary<string, ResourceAsset>> _providerRelicAssets = new Dictionary<string, IReadOnlyDictionary<string, ResourceAsset>>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, IReadOnlySet<string>> _isolatedRelicProviderPaths = new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, string?> _relicOwnerGroups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	private readonly Dictionary<string, BaselineRelicTextureDefinition?> _baselineRelicTextureDefinitions = new Dictionary<string, BaselineRelicTextureDefinition>(StringComparer.OrdinalIgnoreCase);

	private const int SubstantialPackResourceCount = 24;

	private static readonly string[] NonCombatBackgroundSegments = new string[16]
	{
		"/animations/", "/events/", "/event/", "merchant", "ancient", "neow", "/ui/", "map_bgs", "/map/", "icon",
		"portrait", "/cards/", "treasure", "chest", "/rest/", "rest_site"
	};

	internal static readonly HashSet<string> KnownAncientIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "darv", "neow", "nonupeipe", "orobas", "pael", "tanx", "tezcatara", "vakuu" };

	internal static readonly IReadOnlyDictionary<string, string[]> SharedAnimationModelIds = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
	{
		["cultists"] = new string[2] { "calcified_cultist", "damp_cultist" },
		["decimillipede"] = new string[4] { "decimillipede_elite", "decimillipede_segment_back", "decimillipede_segment_front", "decimillipede_segment_middle" },
		["kaiser_crab"] = new string[3] { "kaiser_crab_boss", "rocket", "crusher" },
		["bowlbug"] = new string[4] { "bowlbug_egg", "bowlbug_nectar", "bowlbug_rock", "bowlbug_silk" }
	};

	internal static readonly IReadOnlyDictionary<string, string[]> SharedAnimationSiblingGroups = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
	{
		["decimillipede"] = new string[3] { "decimillipede_segment_front", "decimillipede_segment_middle", "decimillipede_segment_back" },
		["kaiser_crab"] = new string[4] { "kaiser_crab_boss", "kaiser_crab_boss_setup", "rocket", "crusher" },
		["bowlbug"] = new string[4] { "bowlbug_egg", "bowlbug_nectar", "bowlbug_rock", "bowlbug_silk" }
	};

	internal static readonly HashSet<string> SharedAnimationPlaceholderGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		"decimillipede_segment_front", "decimillipede_segment_middle", "decimillipede_segment_back", "kaiser_crab_boss", "kaiser_crab_boss_setup", "rocket", "crusher", "bowlbug_egg", "bowlbug_nectar", "bowlbug_rock",
		"bowlbug_silk"
	};

	private static readonly ConditionalWeakTable<PckArchive, EventTables> EventTableCache = new ConditionalWeakTable<PckArchive, EventTables>();

	private Dictionary<string, ResourceFile>? _fakeMerchantEventPresentation;

	private const string FakeMerchantTentRoot = "res://animations/backgrounds/fake_merchant_room/bottom/";

	private const string FakeMerchantEventArtPrefix = "res://images/events/fake_merchant";

	private readonly Dictionary<string, ulong> _workshopSourceIds;

	public IReadOnlyList<SkinGroup> Groups => _groups;

	public IReadOnlyList<CardSkinGroup> CardGroups => _cardGroups;

	public IReadOnlyList<CardSkinOption> PckCardOptions => _pckCardOptions;

	public IReadOnlySet<string> CardProviderRoots => (from option in _cardGroups.SelectMany((CardSkinGroup @group) => @group.Options)
		select option.ProviderRootPath into path
		where !string.IsNullOrWhiteSpace(path)
		select path).Cast<string>().ToHashSet<string>(StringComparer.OrdinalIgnoreCase);

	private SkinCatalog(PckArchive gameArchive, List<PckResourceIndex> baselineIndexes, List<PckResourceIndex> cosmeticIndexes, IReadOnlyList<SkinGroup> groups, IReadOnlyList<CardSkinGroup> cardGroups, IReadOnlyList<CardSkinOption> pckCardOptions, IReadOnlyList<SkinModDescriptor> mods)
	{
		_gameArchive = gameArchive;
		_workshopSourceIds = mods.Where((SkinModDescriptor mod) => WorkshopSourceId(mod.RootPath) != 0).ToDictionary<SkinModDescriptor, string, ulong>((SkinModDescriptor mod) => mod.Id, (SkinModDescriptor mod) => WorkshopSourceId(mod.RootPath), StringComparer.OrdinalIgnoreCase);
		_baselineIndexes = baselineIndexes;
		_cosmeticIndexes = cosmeticIndexes;
		_groups = groups.ToList();
		_characterAppearanceGroupIds = (from identity in baselineIndexes.SelectMany((PckResourceIndex index) => index.Assets.Keys).Select(TryGetUnambiguousCharacterGroup)
			where identity != null
			select identity.Id).Concat(from @group in _groups
			where @group.Options.Any(IsCharacterAppearanceOption)
			select @group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_resourceGroupIds = (from @group in _groups.SelectMany((SkinGroup @group) => from path in @group.Options.SelectMany((SkinOption option) => option.Assets.Keys)
				select (Path: NormalizeTakeoverPath(path), GroupId: @group.Id)).GroupBy<(string, string), string>(((string Path, string GroupId) pair) => pair.Path, StringComparer.OrdinalIgnoreCase)
			select new
			{
				Path = @group.Key,
				GroupIds = @group.Select(((string Path, string GroupId) pair) => pair.GroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray()
			} into entry
			where entry.GroupIds.Length == 1
			select entry).ToDictionary(entry => entry.Path, entry => entry.GroupIds[0], StringComparer.OrdinalIgnoreCase);
		_configuredCardGroups = cardGroups;
		_pckCardOptions = pckCardOptions;
		_cardGroups = cardGroups.ToList();
		_providerInstanceIdentities = mods.Select((SkinModDescriptor mod) => new ProviderInstanceIdentity(mod.ResourceNamespaceId, mod.Id, mod.Name)).ToArray();
		AddImageRuntimeProviderOptions(mods);
		HashSet<string> characterVisualProviderIds = (from option in _groups.SelectMany((SkinGroup @group) => @group.Options)
			where !option.IsCharacterIconOnly && IsCharacterAppearanceOption(option)
			select option.EffectiveProviderId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		(PckResourceIndex, string)[] source = cosmeticIndexes.SelectMany((PckResourceIndex index) => from path in index.Archive.Paths
			where IsProviderLocalizationFile(path, index.Mod.ResourceNamespaceId)
			select (Index: index, Path: path)).ToArray();
		_cosmeticLocalizationPaths = (from file in source
			where characterVisualProviderIds.Contains(file.Index.Mod.Id) && IsCharacterLocalizationFile(file.Path)
			select file.Path).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_cosmeticLocalizationProviders = (from file in source
			where _cosmeticLocalizationPaths.Contains(file.Path)
			select file.Index.Mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_passthroughLocalizationFiles = source.Where(((PckResourceIndex Index, string Path) file) => !_cosmeticLocalizationPaths.Contains(file.Path)).GroupBy<(PckResourceIndex, string), string>(((PckResourceIndex Index, string Path) file) => file.Path, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, (PckResourceIndex, string)>, string, ResourceFile>((IGrouping<string, (PckResourceIndex Index, string Path)> group) => group.Key, (IGrouping<string, (PckResourceIndex Index, string Path)> group) => new ResourceFile(group.Last().Index.Archive, group.Last().Path), StringComparer.OrdinalIgnoreCase);
		_managedGodotScriptProviders = (from index in cosmeticIndexes
			where index.Mod.HasDll && CountManagedGodotScripts(index.Archive) > 0
			select index.Mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_interactiveRuntimeProviders = (from index in cosmeticIndexes
			where index.Mod.HasDll && _managedGodotScriptProviders.Contains(index.Mod.Id) && ContainsInteractiveScene(index.Archive)
			select index.Mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string[]> visualGroupsByProvider = _groups.Where((SkinGroup group) => !EventSkinPolicy.IsEventGroup(group.Id)).SelectMany((SkinGroup group) => from option in @group.Options
			where !option.IsCharacterIconOnly
			select (GroupId: @group.Id, ProviderId: option.EffectiveProviderId)).GroupBy<(string, string), string>(((string GroupId, string ProviderId) pair) => pair.ProviderId, StringComparer.OrdinalIgnoreCase)
			.ToDictionary<IGrouping<string, (string, string)>, string, string[]>((IGrouping<string, (string GroupId, string ProviderId)> group) => group.Key, (IGrouping<string, (string GroupId, string ProviderId)> group) => group.Select(((string GroupId, string ProviderId) pair) => pair.GroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray(), StringComparer.OrdinalIgnoreCase);
		HashSet<string> cardProviderIds = (from option in _pckCardOptions
			where option.Assets.Count > 0 || option.CardPresentations.Count > 0
			select option.ProviderId ?? option.Id).Concat(from option in _configuredCardGroups.SelectMany((CardSkinGroup @group) => @group.Options)
			where option.Assets.Count > 0 || option.CardPresentations.Count > 0
			select option.ProviderId ?? option.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string[]> source2 = _groups.SelectMany<SkinGroup, (string, string)>(delegate(SkinGroup group)
		{
			return from option in @group.Options.Where(OptionOwnsCharacterRuntimeAssets)
				select (GroupId: @group.Id, ProviderId: option.EffectiveProviderId);
			static bool OptionOwnsCharacterRuntimeAssets(SkinOption option)
			{
				if (!option.IsCharacterIconOnly)
				{
					return option.Assets.Keys.Any(delegate(string path)
					{
						string input = NormalizeTakeoverPath(path);
						return CharacterPathRegex().IsMatch(input) || CharacterSelectSceneRegex().IsMatch(input) || MerchantCharacterSceneRegex().IsMatch(input) || RestSiteCharacterSceneRegex().IsMatch(input) || CharacterSelectIconRegex().IsMatch(input) || CharacterUiTextureRegex().IsMatch(input) || CharacterIconSceneRegex().IsMatch(input) || CharacterMapMarkerRegex().IsMatch(input);
					});
				}
				return false;
			}
		}).GroupBy<(string, string), string>(((string GroupId, string ProviderId) pair) => pair.ProviderId, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, (string, string)>, string, string[]>((IGrouping<string, (string GroupId, string ProviderId)> group) => group.Key, (IGrouping<string, (string GroupId, string ProviderId)> group) => group.Select(((string GroupId, string ProviderId) pair) => pair.GroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray(), StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string[]> singleCharacterBundleGroupsByProvider = source2.Where(delegate(KeyValuePair<string, string[]> pair)
		{
			return pair.Value.Select(CharacterRuntimeFamilyId).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count() == 1;
			static string CharacterRuntimeFamilyId(string groupId)
			{
				if (!groupId.EndsWith("_b", StringComparison.OrdinalIgnoreCase))
				{
					return groupId;
				}
				return groupId.Substring(0, groupId.Length - 2);
			}
		}).ToDictionary<KeyValuePair<string, string[]>, string, string[]>((KeyValuePair<string, string[]> pair) => pair.Key, (KeyValuePair<string, string[]> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
		HashSet<string> independentManagedAncientProviders = (from pair in visualGroupsByProvider
			where pair.Value.All(KnownAncientIds.Contains)
			where pair.Value.All((string groupId) => _groups.First((SkinGroup @group) => @group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Options.Any(delegate(SkinOption option)
			{
				return option.EffectiveProviderId.Equals(pair.Key, StringComparison.OrdinalIgnoreCase) && (option.RuntimeImagePath != null || OptionUsesManagedAncientLayers(groupId, option));
				static bool OptionUsesManagedAncientLayers(string groupId, SkinOption option)
				{
					return option.Assets.Keys.Any(delegate(string path)
					{
						Match match = AncientLayerImageRegex().Match(NormalizeTakeoverPath(path));
						return match.Success && match.Groups["id"].Value.Equals(groupId, StringComparison.OrdinalIgnoreCase) && match.Groups["kind"].Value.Equals("character", StringComparison.OrdinalIgnoreCase);
					});
				}
			}))
			select pair.Key).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_scopedMonsterRuntimeProviderGroups = visualGroupsByProvider.Where<KeyValuePair<string, string[]>>((KeyValuePair<string, string[]> pair) => pair.Value.All((string groupId) => _groups.First((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Options.Any((SkinOption option) => option.EffectiveProviderId.Equals(pair.Key, StringComparison.OrdinalIgnoreCase) && option.IsManagedMonsterRuntimeProfile))).ToDictionary<KeyValuePair<string, string[]>, string, IReadOnlyList<string>>((KeyValuePair<string, string[]> pair) => pair.Key, (KeyValuePair<string, string[]> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
		_scopedMonsterRuntimeProviders = _scopedMonsterRuntimeProviderGroups.Keys.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_directCharacterRuntimeProviders = (from option in _groups.SelectMany((SkinGroup @group) => @group.Options)
			where option.IsDirectCharacterRuntimeProvider
			select option.EffectiveProviderId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_fullRuntimeProviders = (from index in cosmeticIndexes
			where index.Mod.HasDll
			select index.Mod.Id into providerId
			where visualGroupsByProvider.ContainsKey(providerId) && !_directCharacterRuntimeProviders.Contains(providerId) && !independentManagedAncientProviders.Contains(providerId) && !_scopedMonsterRuntimeProviders.Contains(providerId) && (!cardProviderIds.Contains(providerId) || singleCharacterBundleGroupsByProvider.ContainsKey(providerId))
			select providerId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_fullRuntimeProviderGroups = visualGroupsByProvider.Where<KeyValuePair<string, string[]>>((KeyValuePair<string, string[]> pair) => _fullRuntimeProviders.Contains(pair.Key)).ToDictionary<KeyValuePair<string, string[]>, string, IReadOnlyList<string>>((KeyValuePair<string, string[]> pair) => pair.Key, (KeyValuePair<string, string[]> pair) => (!cardProviderIds.Contains(pair.Key)) ? pair.Value : singleCharacterBundleGroupsByProvider[pair.Key], StringComparer.OrdinalIgnoreCase);
	}

	public IReadOnlyList<SkinOption> GetRawCharacterOptions(string groupId)
	{
		SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup != null && IsCharacterAppearanceGroup(groupId))
		{
			return skinGroup.Options.Where((SkinOption option) => !option.IsComposition).ToArray();
		}
		return Array.Empty<SkinOption>();
	}

	public IReadOnlyList<string> GetCompositionSourceOptionIds(string groupId, string optionId)
	{
		if (optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return Array.Empty<string>();
		}
		SkinOption skinOption = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		if (!(skinOption == null))
		{
			if (!skinOption.IsComposition)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<string>(skinOption.Id);
			}
			return skinOption.CompositionSourceOptionIds;
		}
		return Array.Empty<string>();
	}

	public IReadOnlyList<string> GetSelectionProviderIds(string groupId, string optionId)
	{
		if (optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return Array.Empty<string>();
		}
		SkinOption skinOption = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		if (!(skinOption == null))
		{
			if (!skinOption.IsComposition)
			{
				return new _003C_003Ez__ReadOnlySingleElementList<string>(skinOption.EffectiveProviderId);
			}
			return skinOption.CompositionSourceProviderIds;
		}
		return Array.Empty<string>();
	}

	public void SynchronizeCharacterSkinCompositions(IReadOnlyList<CharacterSkinComposition> compositions)
	{
		foreach (SkinGroup group in _groups)
		{
			group.Options.RemoveAll((SkinOption option) => option.IsComposition && !option.IsSessionComposition);
		}
		foreach (CharacterSkinComposition composition in compositions)
		{
			SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(composition.GroupId, StringComparison.OrdinalIgnoreCase));
			if (skinGroup != null && IsCharacterAppearanceGroup(skinGroup.Id) && TryBuildCompositionOption(skinGroup, composition, session: false, out SkinOption option2))
			{
				skinGroup.Options.Add(option2);
			}
		}
		(string, string)[] companionGroups = CompanionGroups;
		for (int i = 0; i < companionGroups.Length; i++)
		{
			(string, string) tuple = companionGroups[i];
			string ownerId = tuple.Item1;
			string companionId = tuple.Item2;
			SkinGroup skinGroup2 = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(ownerId, StringComparison.OrdinalIgnoreCase));
			SkinGroup skinGroup3 = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(companionId, StringComparison.OrdinalIgnoreCase));
			if (skinGroup2 == null || skinGroup3 == null)
			{
				continue;
			}
			foreach (SkinOption item in skinGroup2.Options.Where((SkinOption option) => option.IsComposition))
			{
				ResolveCompanionSelection(skinGroup2, item.Id, skinGroup3);
			}
		}
		SortGroupsAndOptions();
	}

	public bool TryCreateSessionCharacterComposition(string groupId, IReadOnlyList<string> sourceOptionIds, out string optionId)
	{
		optionId = "__base__";
		SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null || !IsCharacterAppearanceGroup(skinGroup.Id))
		{
			return false;
		}
		IReadOnlyList<string> readOnlyList = CharacterSkinCompositionPolicy.ResolveAvailableSourceIds(sourceOptionIds, from option in GetRawCharacterOptions(groupId)
			select option.Id);
		if (readOnlyList.Count == 0)
		{
			return false;
		}
		if (readOnlyList.Count == 1)
		{
			optionId = readOnlyList[0];
			return true;
		}
		string sessionOptionId = CharacterSkinCompositionPolicy.CreateSessionId(groupId, readOnlyList);
		optionId = sessionOptionId;
		if (skinGroup.Options.Any((SkinOption option) => option.Id.Equals(sessionOptionId, StringComparison.OrdinalIgnoreCase)))
		{
			return true;
		}
		CharacterSkinComposition composition = new CharacterSkinComposition
		{
			Id = optionId,
			GroupId = skinGroup.Id,
			Name = "Session composition",
			SourceOptionIds = readOnlyList.ToList()
		};
		if (!TryBuildCompositionOption(skinGroup, composition, session: true, out SkinOption option2))
		{
			optionId = "__base__";
			return false;
		}
		skinGroup.Options.Add(option2);
		return true;
	}

	public IReadOnlyList<string> ClearSessionCharacterCompositions()
	{
		string[] array = (from @group in _groups
			where @group.Options.RemoveAll((SkinOption option) => option.IsSessionComposition) > 0
			select @group.Id).ToArray();
		if (array.Length != 0)
		{
			SortGroupsAndOptions();
		}
		return array;
	}

	public bool TryGetVisualProviderId(string groupId, string optionId, out string providerId)
	{
		SkinOption skinOption = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		providerId = skinOption?.EffectiveProviderId ?? string.Empty;
		if (skinOption != null)
		{
			return !optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public bool TryGetVisualProviderSource(string groupId, string optionId, out string providerId, out string pckPath, out IReadOnlyList<string> safeResourceRoots, out IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings)
	{
		providerId = string.Empty;
		pckPath = string.Empty;
		safeResourceRoots = Array.Empty<string>();
		resourceBindings = new Dictionary<string, VisualResourceBinding>(StringComparer.OrdinalIgnoreCase);
		SkinOption option = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		if (option == null || optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		PckResourceIndex[] source = _cosmeticIndexes.Concat(_baselineIndexes.Skip(1)).ToArray();
		PckResourceIndex index = source.FirstOrDefault((PckResourceIndex candidate) => candidate.Mod.Id.Equals(option.EffectiveProviderId, StringComparison.OrdinalIgnoreCase)) ?? source.FirstOrDefault((PckResourceIndex candidate) => option.Assets.Values.SelectMany((ResourceAsset asset) => asset.Files).Any((ResourceFile file) => file.Archive == candidate.Archive));
		if (index?.Mod.PckPath == null || !File.Exists(index.Mod.PckPath))
		{
			return false;
		}
		providerId = index.Mod.Id;
		pckPath = index.Mod.PckPath;
		resourceBindings = (from binding in Enumerable.Select(option.Assets, (KeyValuePair<string, ResourceAsset> pair) => new
			{
				TargetPath = pair.Key,
				SourcePath = pair.Value.SourcePath,
				Paths = (IReadOnlyList<string>)(from file in pair.Value.Files
					where file.Archive == index.Archive
					select file.Path).Distinct<string>(StringComparer.OrdinalIgnoreCase).Order<string>(StringComparer.OrdinalIgnoreCase).ToArray()
			})
			where binding.Paths.Count > 0
			select binding).ToDictionary(binding => binding.TargetPath, binding => new VisualResourceBinding(binding.SourcePath, binding.Paths), StringComparer.OrdinalIgnoreCase);
		safeResourceRoots = resourceBindings.Values.SelectMany((VisualResourceBinding binding) => binding.Files).Concat((from file in option.ManagedMonsterScene?.Files
			where file.Archive == index.Archive
			select file.Path) ?? Array.Empty<string>()).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			.Order<string>(StringComparer.OrdinalIgnoreCase)
			.ToArray();
		return true;
	}

	internal static bool IsSafeOnlineResourceRootForGroup(string resourcePath, string groupId)
	{
		string text = resourcePath;
		if (text.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			text = text2.Substring(0, text2.Length - 6);
		}
		else if (text.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			string text2 = text;
			text = text2.Substring(0, text2.Length - 7);
		}
		GroupIdentity groupIdentity = TryGetPrimaryGroup(NormalizeTakeoverPath(text));
		if (groupIdentity != null)
		{
			return groupIdentity.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	internal static bool IsCharacterImageResourceForProvider(string providerId, string resourcePath)
	{
		if (!(TryGetPrimaryGroup(NormalizeTakeoverPath(resourcePath)) != null))
		{
			return TryGetRuntimeProviderAsset(providerId, NormalizeTakeoverPath(resourcePath)) != null;
		}
		return true;
	}

	public bool IsBaseGameResource(string resourcePath)
	{
		return _gameArchive.Contains(resourcePath);
	}

	public bool TryReadBaseGameResource(string resourcePath, out byte[] bytes)
	{
		if (!_gameArchive.Contains(resourcePath))
		{
			bytes = Array.Empty<byte>();
			return false;
		}
		bytes = _gameArchive.ReadFile(resourcePath);
		return true;
	}

	public bool TryAddSessionVisualProvider(string optionId, string optionName, string pckPath, string expectedGroupId, IReadOnlyDictionary<string, VisualResourceBinding> resourceBindings, out string error)
	{
		error = string.Empty;
		if (_groups.SelectMany((SkinGroup group) => group.Options).Any((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase)))
		{
			error = "外观选项已存在。";
			return false;
		}
		PckArchive pckArchive = null;
		PckResourceIndex pckResourceIndex = null;
		try
		{
			pckArchive = PckArchive.Open(pckPath);
			pckResourceIndex = PckResourceIndex.Build(new SkinModDescriptor(optionId, optionName, pckPath, AffectsGameplay: false), pckArchive, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), null);
			pckArchive = null;
			SkinGroup skinGroup;
			if (resourceBindings.Count > 0)
			{
				Dictionary<string, ResourceAsset> dictionary = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, VisualResourceBinding> resourceBinding in resourceBindings)
				{
					if (!IsSafeOnlineResourceRootForGroup(resourceBinding.Key, expectedGroupId))
					{
						error = $"联机资源映射不属于角色 {expectedGroupId}：{resourceBinding.Key}。";
						return false;
					}
					ResourceAsset resourceAsset = new ResourceAsset(resourceBinding.Value.SourcePath);
					foreach (string file in resourceBinding.Value.Files)
					{
						if (!pckResourceIndex.Archive.Contains(file))
						{
							error = "安全资源包缺少映射文件：" + file + "。";
							return false;
						}
						resourceAsset.AddFile(pckResourceIndex.Archive, file);
					}
					if (resourceAsset.Files.Count > 0)
					{
						dictionary[resourceBinding.Key] = resourceAsset;
					}
				}
				skinGroup = new SkinGroup(expectedGroupId, TryGetPrimaryGroup(resourceBindings.Keys.FirstOrDefault() ?? string.Empty)?.DisplayName ?? expectedGroupId);
				skinGroup.Options.Add(new SkinOption(optionId, optionName, dictionary, IsRuntimeProvider: false, IsDirectCharacterRuntimeProvider: false, null, null, null, optionId));
			}
			else
			{
				SkinGroup[] array = (from @group in BuildGroups(new _003C_003Ez__ReadOnlySingleElementList<PckResourceIndex>(pckResourceIndex), _baselineIndexes)
					where @group.Id.Equals(expectedGroupId, StringComparison.OrdinalIgnoreCase)
					select @group).ToArray();
				if (array.Length != 1 || array[0].Options.Count != 1)
				{
					error = "安全资源包未形成匹配的角色外观。";
					return false;
				}
				skinGroup = array[0];
			}
			SkinGroup skinGroup2 = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(expectedGroupId, StringComparison.OrdinalIgnoreCase));
			if (!IsCharacterAppearanceOption(skinGroup.Options[0]))
			{
				error = "找不到对应的角色外观分组。";
				return false;
			}
			if (skinGroup2 == null)
			{
				skinGroup2 = new SkinGroup(skinGroup.Id, skinGroup.DisplayName);
				_groups.Add(skinGroup2);
			}
			skinGroup2.Options.Add(skinGroup.Options[0]);
			skinGroup2.Options.Sort((SkinOption left, SkinOption right) => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase));
			_cosmeticIndexes.Add(pckResourceIndex);
			pckResourceIndex = null;
			return true;
		}
		catch (Exception ex)
		{
			error = ex.GetBaseException().Message;
			return false;
		}
		finally
		{
			pckResourceIndex?.Dispose();
			pckArchive?.Dispose();
		}
	}

	public IReadOnlyList<string> RemoveSessionVisualProvider(string optionId)
	{
		string[] result = (from @group in _groups
			where @group.Options.RemoveAll((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase)) > 0
			select @group.Id).ToArray();
		PckResourceIndex[] array = _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase)).ToArray();
		foreach (PckResourceIndex pckResourceIndex in array)
		{
			_cosmeticIndexes.Remove(pckResourceIndex);
			pckResourceIndex.Dispose();
		}
		_groups.RemoveAll((SkinGroup group) => group.Options.Count == 0);
		return result;
	}

	public IReadOnlySet<string> GetSelectedLocalizationProviderIds(IReadOnlyDictionary<string, string> selections)
	{
		return GetSelectedLocalizationProviderPriority(selections).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public IReadOnlyList<string> GetSelectedLocalizationProviderPriority(IReadOnlyDictionary<string, string> selections)
	{
		List<string> list = new List<string>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (SkinGroup group in Groups)
		{
			selections.TryGetValue(group.Id, out string selectedId);
			SkinOption skinOption = group.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
			if (skinOption == null)
			{
				continue;
			}
			IEnumerable<SkinOption> enumerable2;
			if (!skinOption.IsComposition)
			{
				IEnumerable<SkinOption> enumerable = new _003C_003Ez__ReadOnlySingleElementList<SkinOption>(skinOption);
				enumerable2 = enumerable;
			}
			else
			{
				enumerable2 = (from sourceId in skinOption.CompositionSourceOptionIds
					select @group.Options.FirstOrDefault((SkinOption option) => !option.IsComposition && option.Id.Equals(sourceId, StringComparison.OrdinalIgnoreCase)) into option
					where option != null
					select option).Cast<SkinOption>();
			}
			foreach (SkinOption item in enumerable2)
			{
				if (_cosmeticLocalizationProviders.Contains(item.EffectiveProviderId) && hashSet.Add(item.EffectiveProviderId))
				{
					list.Add(item.EffectiveProviderId);
				}
			}
		}
		return list;
	}

	public IReadOnlyList<string> FilterModdedLocalizationTables(IEnumerable<string> localizationPaths, IReadOnlyDictionary<string, string> selections)
	{
		string[] paths = localizationPaths.ToArray();
		if (_cosmeticLocalizationProviders.Count == 0)
		{
			return paths;
		}
		IReadOnlyList<string> selectedLocalizationProviderPriority = GetSelectedLocalizationProviderPriority(selections);
		HashSet<string> selectedProviders = selectedLocalizationProviderPriority.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		string providerId2;
		IEnumerable<string> first = paths.Where((string path) => !_cosmeticLocalizationPaths.Contains(path) || !TryGetLocalizationProviderId(path, out providerId2));
		string providerId3;
		IEnumerable<string> second = selectedLocalizationProviderPriority.Reverse().SelectMany((string providerId) => paths.Where((string path) => _cosmeticLocalizationPaths.Contains(path) && TryGetLocalizationProviderId(path, out providerId3) && providerId3.Equals(providerId, StringComparison.OrdinalIgnoreCase)));
		string providerId4;
		return (from path in first.Concat(second)
			where !_cosmeticLocalizationPaths.Contains(path) || !TryGetLocalizationProviderId(path, out providerId4) || selectedProviders.Contains(providerId4)
			select path).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public bool IsManagedCosmeticLocalizationPath(string path)
	{
		return _cosmeticLocalizationPaths.Contains(path);
	}

	private static bool TryGetLocalizationProviderId(string path, out string providerId)
	{
		providerId = string.Empty;
		if (!path.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string text = path;
		string text2 = text.Substring(6, text.Length - 6);
		int num = text2.IndexOf('/');
		if (num > 0)
		{
			text = text2;
			int num2 = num + 1;
			if (text.Substring(num2, text.Length - num2).StartsWith("localization/", StringComparison.OrdinalIgnoreCase))
			{
				providerId = text2.Substring(0, num);
				return true;
			}
		}
		return false;
	}

	public static SkinCatalog Build(string gamePckPath, IEnumerable<SkinModDescriptor> mods)
	{
		SkinModDescriptor[] array = AssignProviderInstanceIdentities(mods);
		PckArchive pckArchive = PckArchive.Open(gamePckPath);
		List<PckResourceIndex> list = new List<PckResourceIndex>();
		List<PckResourceIndex> list2 = new List<PckResourceIndex>();
		try
		{
			Dictionary<string, string> importedToSource = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			list.Add(PckResourceIndex.Build(new SkinModDescriptor("game", "游戏原版", gamePckPath, AffectsGameplay: true), pckArchive, importedToSource, IsAnimationRemap));
			SkinModDescriptor[] array2 = array;
			foreach (SkinModDescriptor skinModDescriptor in array2)
			{
				if (skinModDescriptor.PckPath == null || !File.Exists(skinModDescriptor.PckPath))
				{
					continue;
				}
				PckArchive pckArchive2 = PckArchive.Open(skinModDescriptor.PckPath);
				try
				{
					PckResourceIndex item = PckResourceIndex.Build(skinModDescriptor, pckArchive2, importedToSource, null);
					if (skinModDescriptor.AffectsGameplay)
					{
						list.Add(item);
					}
					else
					{
						list2.Add(item);
					}
				}
				catch
				{
					pckArchive2.Dispose();
					throw;
				}
			}
			IReadOnlyList<SkinGroup> groups = BuildGroups(list2, list);
			IReadOnlyList<CardSkinGroup> cardGroups = BuildCardGroups(list2);
			IReadOnlyList<CardSkinOption> pckCardOptions = BuildPckCardOptions(list2, list);
			SkinCatalog skinCatalog = new SkinCatalog(pckArchive, list, list2, groups, cardGroups, pckCardOptions, array);
			skinCatalog.SortGroupsAndOptions();
			return skinCatalog;
		}
		catch
		{
			foreach (PckResourceIndex item2 in list.Skip(1).Concat(list2))
			{
				item2.Dispose();
			}
			pckArchive.Dispose();
			throw;
		}
	}

	public static IReadOnlyList<SkinProviderProbe> ProbeSkinProviders(IEnumerable<SkinModDescriptor> mods, string? gamePckPath = null)
	{
		SkinModDescriptor[] source = AssignProviderInstanceIdentities(mods);
		List<SkinProviderProbe> list = new List<SkinProviderProbe>();
		Dictionary<string, string> importedToSource = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		List<PckResourceIndex> baselineIndexes = new List<PckResourceIndex>();
		try
		{
			if (gamePckPath != null && File.Exists(gamePckPath))
			{
				TryAddProbeBaselineIndex(new SkinModDescriptor("game", "游戏原版", gamePckPath, AffectsGameplay: true), importedToSource, baselineIndexes, IsAnimationRemap);
			}
			foreach (SkinModDescriptor item in source.Where((SkinModDescriptor mod) => mod.AffectsGameplay && mod.PckPath != null && File.Exists(mod.PckPath) && !string.Equals(mod.PckPath, gamePckPath, StringComparison.OrdinalIgnoreCase)))
			{
				TryAddProbeBaselineIndex(item, importedToSource, baselineIndexes, null);
			}
			foreach (SkinModDescriptor mod2 in source.Where((SkinModDescriptor mod) => !mod.AffectsGameplay))
			{
				if (mod2.HasDll && !FrameworkCompatibilityLayer.IsKnownFrameworkHost(mod2.Id) && ProviderGameplayContentPolicy.DefinesNewContent(mod2.RootPath, mod2.ResourceNamespaceId))
				{
					ModLog.Info("未按皮肤接管 " + (mod2.Name ?? mod2.Id) + "：其 DLL 定义了新的游戏内容模型（卡池/角色/怪物等），属于内容 Mod 而非外观包；已交回游戏原加载器，DLL 与 PCK 按作者原意全局加载。");
					continue;
				}
				int num = 0;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				bool flag = false;
				int num5 = 0;
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				List<string> list2 = new List<string>();
				List<string> list3 = new List<string>();
				if (mod2.PckPath != null && File.Exists(mod2.PckPath))
				{
					PckArchive pckArchive = null;
					PckResourceIndex index = null;
					try
					{
						pckArchive = PckArchive.Open(mod2.PckPath);
						index = PckResourceIndex.Build(mod2, pckArchive, importedToSource, null);
						num4 = (mod2.HasDll ? CountManagedGodotScripts(pckArchive) : 0);
						flag = num4 > 0 && ContainsInteractiveScene(pckArchive);
						num5 = index.Assets.Count;
						(int, IReadOnlyList<string>, int, IReadOnlyList<string>, HashSet<string>) tuple = CountProbeVisualGroups(index, baselineIndexes);
						(num, _, _, _, _) = tuple;
						list3.AddRange(tuple.Item4);
						hashSet.UnionWith(tuple.Item5);
						list2.AddRange(tuple.Item2);
						list2.AddRange(tuple.Item4);
						FrameworkCharacterSkinContract[] array = (from contract in FrameworkSkinContractScanner.Scan(mod2.RootPath, mod2.ResourceNamespaceId)
							where FrameworkContractClosureOrLog(index, baselineIndexes, contract, mod2)
							select contract).ToArray();
						FrameworkCharacterSkinContract[] array2 = array;
						foreach (FrameworkCharacterSkinContract frameworkCharacterSkinContract in array2)
						{
							list2.Add(frameworkCharacterSkinContract.TargetGroupId);
							foreach (string resourcePath in frameworkCharacterSkinContract.ResourcePaths)
							{
								hashSet.Add(NormalizeTakeoverPath(resourcePath));
							}
						}
						num = Math.Max(num, array.Select((FrameworkCharacterSkinContract contract) => contract.TargetGroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count());
						IReadOnlyList<CardSkinGroup> source2 = BuildCardGroups(new _003C_003Ez__ReadOnlySingleElementList<PckResourceIndex>(index));
						num2 = source2.Sum((CardSkinGroup group) => group.Options.Sum((CardSkinOption option) => option.NormalPortraits.Count + option.AncientPortraits.Count));
						num3 = source2.Sum((CardSkinGroup group) => group.Options.Sum((CardSkinOption option) => option.CardPresentations.Count));
						IReadOnlyList<CardSkinOption> source3 = BuildPckCardOptions(new _003C_003Ez__ReadOnlySingleElementList<PckResourceIndex>(index));
						num2 += source3.Sum((CardSkinOption option) => option.Assets.Count + option.NormalPortraits.Count + option.AncientPortraits.Count);
						num3 += source3.Sum((CardSkinOption option) => option.CardPresentations.Count);
					}
					catch (Exception)
					{
					}
					finally
					{
						if (index != null)
						{
							index.Dispose();
						}
						else
						{
							pckArchive?.Dispose();
						}
					}
				}
				if (mod2.HasDll && mod2.RootPath != null)
				{
					ManagedCharacterAssetReplacement[] array3 = (from replacement in ManagedCharacterAssetReplacementScanner.Scan(mod2.RootPath, mod2.ResourceNamespaceId)
						where IsGameCharacterGroup(replacement.TargetGroupId, baselineIndexes)
						select replacement).ToArray();
					ManagedCharacterAssetReplacement[] array4 = array3;
					foreach (ManagedCharacterAssetReplacement managedCharacterAssetReplacement in array4)
					{
						list2.Add(managedCharacterAssetReplacement.TargetGroupId);
						hashSet.UnionWith(managedCharacterAssetReplacement.CanonicalPathsByProviderPath.Values);
					}
					num = Math.Max(num, array3.Select((ManagedCharacterAssetReplacement replacement) => replacement.TargetGroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase).Count());
				}
				int num6 = 0;
				bool flag2 = num > 0 || num2 > 0 || num3 > 0 || list3.Count > 0;
				bool flag3 = false;
				if (mod2.RootPath != null)
				{
					num6 = DiscoverRuntimeAncientImages(mod2).Count;
					flag2 = flag2 || num6 > 0;
					if (mod2.HasDll && num == 0 && num2 == 0 && num3 == 0 && LooksLikeDllSkinProvider(mod2))
					{
						bool flag4 = SkinProviderPatchEvidencePolicy.BringsOwnAppearance(num5, num6, ReplacesCharacterVisualsAtRuntime(mod2), HasLooseAppearanceResource(mod2.RootPath));
						if (LooksLikeAssetPipelineTool(mod2))
						{
							flag3 = true;
							ModLog.Info("未按皮肤接管 " + (mod2.Name ?? mod2.Id) + "：它的补丁接管的是资源加载管线（AssetCache/AssetLoadingSession），属于资源替换工具而不是外观包；已交回游戏原加载器，其 DLL 与 PCK 按作者原意完整加载。");
						}
						else if (num5 >= 24)
						{
							flag3 = true;
							ModLog.Info($"未按皮肤接管 {mod2.Name ?? mod2.Id}：其 {num5} 项资源中没有可呈现的皮肤资源，" + "但 DLL 会补丁外观入口；按皮肤隔离会让整个包失效。已交回游戏原加载器，其 DLL 与 PCK 按作者原意完整加载。");
						}
						else if (!flag4)
						{
							flag3 = true;
							ModLog.Info($"未按皮肤接管 {mod2.Name ?? mod2.Id}：它不带任何自己的外观资源（包资源 {num5} 项、运行时图片 {num6} 张、目录内无外观文件），" + "也没有替换角色自己的外观创建入口；它只能添加由本体或别的 Mod 资源拼出来的东西，按皮肤隔离只会让它停止工作。已交回游戏原加载器，其 DLL 按作者原意完整加载。");
						}
						else
						{
							num = 1;
						}
					}
				}
				int servedResourceCount = hashSet.Count + num2 + num3;
				if (num > 0 || num2 > 0 || num3 > 0 || num6 > 0 || list3.Count > 0)
				{
					string id = mod2.Id;
					string? rootPath = mod2.RootPath;
					int visualGroupCount = num;
					int cardAssetCount = num2;
					int cardPresentationCount = num3;
					int runtimeImageCount = num6;
					int managedScriptCount = num4;
					bool hasInteractiveScenes = flag;
					string resourceNamespaceId = mod2.ResourceNamespaceId;
					bool hasResourceBackedCosmetics = flag2;
					int packResourceCount = num5;
					List<string> list4 = new List<string>();
					list4.AddRange(list2.Distinct<string>(StringComparer.OrdinalIgnoreCase));
					_003C_003Ez__ReadOnlyList<string> servedGroupIds = new _003C_003Ez__ReadOnlyList<string>(list4);
					List<string> list5 = new List<string>();
					list5.AddRange(hashSet.Order<string>(StringComparer.OrdinalIgnoreCase).Take(64));
					_003C_003Ez__ReadOnlyList<string> servedSourcePaths = new _003C_003Ez__ReadOnlyList<string>(list5);
					int baselinePartGroupCount = list3.Distinct<string>(StringComparer.OrdinalIgnoreCase).Count();
					List<string> list6 = new List<string>();
					list6.AddRange(list3.Distinct<string>(StringComparer.OrdinalIgnoreCase));
					list.Add(new SkinProviderProbe(id, rootPath, visualGroupCount, cardAssetCount, cardPresentationCount, runtimeImageCount, managedScriptCount, hasInteractiveScenes, resourceNamespaceId, hasResourceBackedCosmetics, packResourceCount, servedResourceCount, servedGroupIds, servedSourcePaths, baselinePartGroupCount, new _003C_003Ez__ReadOnlyList<string>(list6)));
				}
			}
			return list;
		}
		finally
		{
			foreach (PckResourceIndex item2 in baselineIndexes)
			{
				item2.Dispose();
			}
		}
	}

	private static SkinModDescriptor[] AssignProviderInstanceIdentities(IEnumerable<SkinModDescriptor> mods)
	{
		SkinModDescriptor[] source = mods.ToArray();
		IReadOnlyList<ProviderInstanceIdentity> identities = ProviderInstanceIdentityPolicy.Resolve(source.Select((SkinModDescriptor mod) => new ProviderInstanceCandidate(mod.ResourceNamespaceId, mod.Name, mod.RootPath)).ToArray());
		return source.Select((SkinModDescriptor mod, int index) => mod with
		{
			Id = identities[index].InstanceId,
			Name = identities[index].DisplayName,
			ManifestId = identities[index].ManifestId
		}).ToArray();
	}

	private static void TryAddProbeBaselineIndex(SkinModDescriptor mod, Dictionary<string, string> importedToSource, ICollection<PckResourceIndex> baselineIndexes, Func<string, bool>? remapFilter)
	{
		if (mod.PckPath == null)
		{
			return;
		}
		PckArchive pckArchive = null;
		try
		{
			pckArchive = PckArchive.Open(mod.PckPath);
			PckResourceIndex item = PckResourceIndex.Build(mod, pckArchive, importedToSource, remapFilter);
			baselineIndexes.Add(item);
			pckArchive = null;
		}
		catch (Exception)
		{
		}
		finally
		{
			pckArchive?.Dispose();
		}
	}

	private static (int AnchoredGroupCount, IReadOnlyList<string> AnchoredGroupIds, int PartGroupCount, IReadOnlyList<string> PartGroupIds, HashSet<string> ServedSources) CountProbeVisualGroups(PckResourceIndex index, IReadOnlyCollection<PckResourceIndex> baselineIndexes)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (baselineIndexes.Count > 0)
		{
			SkinGroup[] array = (from @group in BuildGroups(new _003C_003Ez__ReadOnlySingleElementList<PckResourceIndex>(index), baselineIndexes)
				where @group.Options.Count > 0
				select @group).ToArray();
			AddGroupSources(array, hashSet);
			SkinGroup[] array2 = array.Where((SkinGroup group) => group.Options.Any((SkinOption option) => !option.IsBaselinePartOverride)).ToArray();
			SkinGroup[] array3 = array.Where((SkinGroup group) => group.Options.All((SkinOption option) => option.IsBaselinePartOverride)).ToArray();
			int item = array2.Length;
			List<string> list = new List<string>();
			list.AddRange(array2.Select((SkinGroup group) => group.Id));
			_003C_003Ez__ReadOnlyList<string> item2 = new _003C_003Ez__ReadOnlyList<string>(list);
			int item3 = array3.Length;
			List<string> list2 = new List<string>();
			list2.AddRange(array3.Select((SkinGroup group) => group.Id));
			return (AnchoredGroupCount: item, AnchoredGroupIds: item2, PartGroupCount: item3, PartGroupIds: new _003C_003Ez__ReadOnlyList<string>(list2), ServedSources: hashSet);
		}
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string key in index.Assets.Keys)
		{
			GroupIdentity groupIdentity = TryGetPrimaryGroup(key);
			if (!(groupIdentity == null) && IsGroupAnchorResource(key, groupIdentity))
			{
				hashSet2.Add(groupIdentity.Id);
				hashSet.Add(key);
			}
		}
		SkinGroup[] array4 = (from @group in BuildGroups(new _003C_003Ez__ReadOnlySingleElementList<PckResourceIndex>(index))
			where @group.Options.Count > 0
			select @group).ToArray();
		AddGroupSources(array4, hashSet);
		SkinGroup[] array5 = array4;
		foreach (SkinGroup skinGroup in array5)
		{
			hashSet2.Add(skinGroup.Id);
		}
		return (AnchoredGroupCount: hashSet2.Count, AnchoredGroupIds: new global::_003C_003Ez__ReadOnlyArray<string>(hashSet2.ToArray()), PartGroupCount: 0, PartGroupIds: Array.Empty<string>(), ServedSources: hashSet);
		static void AddGroupSources(IEnumerable<SkinGroup> groups, HashSet<string> sources)
		{
			foreach (SkinOption item4 in groups.SelectMany((SkinGroup group) => group.Options))
			{
				foreach (string key2 in item4.Assets.Keys)
				{
					sources.Add(key2);
				}
			}
		}
	}

	private static bool IsGroupAnchorResource(string sourcePath, GroupIdentity group)
	{
		if (!group.Id.Equals("merchant", StringComparison.OrdinalIgnoreCase) && !group.Id.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return !MerchantHandPartPathRegex().IsMatch(NormalizeTakeoverPath(sourcePath));
	}

	private static int CountManagedGodotScripts(PckArchive archive)
	{
		FileInfo fileInfo = new FileInfo(archive.Path);
		lock (ManagedScriptCountCacheSync)
		{
			if (ManagedScriptCountCache.TryGetValue(archive.Path, out ManagedScriptCountCacheEntry value) && value.Length == fileInfo.Length && value.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
			{
				return value.Count;
			}
		}
		HashSet<string> hashSet = archive.Paths.Where((string path) => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in archive.Paths.Where((string path) => path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase)))
		{
			string @string = Encoding.UTF8.GetString(archive.ReadFile(item));
			foreach (Match item2 in EmbeddedResourcePathRegex().Matches(@string))
			{
				if (item2.Value.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
				{
					hashSet.Add(item2.Value);
				}
			}
		}
		int count = hashSet.Count;
		lock (ManagedScriptCountCacheSync)
		{
			ManagedScriptCountCache[archive.Path] = new ManagedScriptCountCacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, count);
			return count;
		}
	}

	private static bool ContainsInteractiveScene(PckArchive archive)
	{
		foreach (string item in archive.Paths.Where((string path) => path.EndsWith(".scn", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase)))
		{
			string @string;
			try
			{
				@string = Encoding.UTF8.GetString(archive.ReadFile(item));
			}
			catch
			{
				continue;
			}
			bool num = @string.Contains("Touch_Box_", StringComparison.OrdinalIgnoreCase) || @string.Contains("InputEventMouse", StringComparison.OrdinalIgnoreCase) || @string.Contains("GuiInput", StringComparison.OrdinalIgnoreCase);
			bool flag = @string.Contains("ClickAnim", StringComparison.OrdinalIgnoreCase) || @string.Contains("DragSpeed", StringComparison.OrdinalIgnoreCase) || @string.Contains("MaxDragRadius", StringComparison.OrdinalIgnoreCase) || @string.Contains("IsAbsoluteDrag", StringComparison.OrdinalIgnoreCase);
			if (num && flag)
			{
				return true;
			}
		}
		return false;
	}

	private static bool HasLooseAppearanceResource(string? rootPath)
	{
		if (string.IsNullOrWhiteSpace(rootPath) || !Directory.Exists(rootPath))
		{
			return false;
		}
		try
		{
			foreach (string item in Directory.EnumerateFiles(rootPath, "*", SearchOption.AllDirectories))
			{
				if (AppearanceResourceFilePolicy.IsAppearanceResourceFile(item))
				{
					return true;
				}
			}
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException) ? 1 : 0) != 0)
		{
			return false;
		}
		return false;
	}

	private static bool LooksLikeAssetPipelineTool(SkinModDescriptor mod)
	{
		if (mod.RootPath == null)
		{
			return false;
		}
		string path = SkinPackagePaths.Resolve(mod.RootPath, mod.ResourceNamespaceId, ".dll");
		try
		{
			if (!File.Exists(path))
			{
				return false;
			}
			using FileStream peStream = File.OpenRead(path);
			using PEReader pEReader = new PEReader(peStream, PEStreamOptions.LeaveOpen);
			if (!pEReader.HasMetadata)
			{
				return false;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
				StringBuilder stringBuilder = new StringBuilder();
				AppendHarmonyPatchMetadata(metadataReader, typeDefinition.GetCustomAttributes(), stringBuilder);
				foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
				{
					AppendHarmonyPatchMetadata(metadataReader, metadataReader.GetMethodDefinition(method).GetCustomAttributes(), stringBuilder);
				}
				if (stringBuilder.Length != 0)
				{
					string text = stringBuilder.ToString();
					if (text.Contains("AssetLoadingSession", StringComparison.Ordinal) || (text.Contains("AssetCache", StringComparison.Ordinal) && text.Contains("LoadAsset", StringComparison.Ordinal)))
					{
						return true;
					}
				}
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	private static bool LooksLikeDllSkinProvider(SkinModDescriptor mod)
	{
		if (mod.RootPath == null)
		{
			return false;
		}
		string text = SkinPackagePaths.Resolve(mod.RootPath, mod.ResourceNamespaceId, ".dll");
		try
		{
			FileInfo fileInfo = new FileInfo(text);
			if (!fileInfo.Exists || fileInfo.Length <= 0 || fileInfo.Length > 33554432)
			{
				return false;
			}
			byte[] bytes = File.ReadAllBytes(text);
			string metadata = Encoding.Latin1.GetString(bytes);
			string unicodeMetadata = Encoding.Unicode.GetString(bytes);
			if (!ContainsMetadata("HarmonyLib") && !ContainsMetadata("HarmonyPatch") && !ContainsMetadata("PatchAll"))
			{
				return false;
			}
			bool num = new string[9] { "scenes/creature_visuals", "spineskins/", "screens/char_select", "packed/character_select", "events/background_scenes", "card_portraits", "card_atlas.sprites", "map_marker_", "ui/run_history" }.Any((string path) => ContainsMetadata(path, StringComparison.OrdinalIgnoreCase));
			bool flag = HasDirectVisualHarmonyPatch(text);
			if (!num && !flag)
			{
				return false;
			}
			return flag || HasTarget("CharacterModel", new string[1] { "CreateVisuals" }) || HasTarget("MonsterModel", new string[1] { "CreateVisuals" }) || HasTarget("EventModel", new string[1] { "CreateBackgroundScene" });
			bool ContainsMetadata(string value, StringComparison comparison = StringComparison.Ordinal)
			{
				if (!metadata.Contains(value, comparison))
				{
					return unicodeMetadata.Contains(value, comparison);
				}
				return true;
			}
			bool HasTarget(string typeName, string[] methodNames)
			{
				if (ContainsMetadata(typeName))
				{
					return methodNames.Any((string methodName) => ContainsMetadata(methodName));
				}
				return false;
			}
		}
		catch
		{
			return false;
		}
	}

	internal static bool ReplacesCharacterVisualsAtRuntime(SkinModDescriptor mod)
	{
		if (mod.RootPath == null || !mod.HasDll)
		{
			return false;
		}
		string text;
		try
		{
			text = SkinPackagePaths.Resolve(mod.RootPath, mod.ResourceNamespaceId, ".dll");
		}
		catch (ArgumentException)
		{
			return false;
		}
		FileInfo fileInfo;
		try
		{
			fileInfo = new FileInfo(text);
		}
		catch (Exception ex2) when (((ex2 is IOException || ex2 is UnauthorizedAccessException || ex2 is NotSupportedException) ? 1 : 0) != 0)
		{
			return false;
		}
		if (!fileInfo.Exists || fileInfo.Length <= 0)
		{
			return false;
		}
		lock (CharacterVisualPatchCacheSync)
		{
			if (CharacterVisualPatchCache.TryGetValue(text, out CharacterVisualPatchCacheEntry value) && value.Length == fileInfo.Length && value.LastWriteTimeUtc == fileInfo.LastWriteTimeUtc)
			{
				return value.ReplacesCharacterVisuals;
			}
		}
		bool flag = HasCharacterVisualCreationPatch(text);
		lock (CharacterVisualPatchCacheSync)
		{
			CharacterVisualPatchCache[text] = new CharacterVisualPatchCacheEntry(fileInfo.Length, fileInfo.LastWriteTimeUtc, flag);
			return flag;
		}
	}

	private static bool HasCharacterVisualCreationPatch(string assemblyPath)
	{
		try
		{
			using FileStream peStream = File.OpenRead(assemblyPath);
			using PEReader pEReader = new PEReader(peStream, PEStreamOptions.LeaveOpen);
			if (!pEReader.HasMetadata)
			{
				return false;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
				StringBuilder stringBuilder = new StringBuilder();
				AppendHarmonyPatchMetadata(metadataReader, typeDefinition.GetCustomAttributes(), stringBuilder);
				foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
				{
					AppendHarmonyPatchMetadata(metadataReader, metadataReader.GetMethodDefinition(method).GetCustomAttributes(), stringBuilder);
				}
				if (stringBuilder.Length != 0)
				{
					string text = stringBuilder.ToString();
					if (text.Contains("CharacterModel", StringComparison.Ordinal) && text.Contains("CreateVisuals", StringComparison.Ordinal))
					{
						return true;
					}
				}
			}
			return false;
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is BadImageFormatException) ? 1 : 0) != 0)
		{
			return false;
		}
	}

	private static bool HasDirectVisualHarmonyPatch(string assemblyPath)
	{
		using (FileStream peStream = File.OpenRead(assemblyPath))
		{
			using PEReader pEReader = new PEReader(peStream, PEStreamOptions.LeaveOpen);
			if (!pEReader.HasMetadata)
			{
				return false;
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			foreach (TypeDefinitionHandle typeDefinition2 in metadataReader.TypeDefinitions)
			{
				TypeDefinition typeDefinition = metadataReader.GetTypeDefinition(typeDefinition2);
				StringBuilder stringBuilder = new StringBuilder();
				AppendHarmonyPatchMetadata(metadataReader, typeDefinition.GetCustomAttributes(), stringBuilder);
				foreach (MethodDefinitionHandle method in typeDefinition.GetMethods())
				{
					AppendHarmonyPatchMetadata(metadataReader, metadataReader.GetMethodDefinition(method).GetCustomAttributes(), stringBuilder);
				}
				if (stringBuilder.Length != 0)
				{
					string text = stringBuilder.ToString();
					flag |= HasDirectVisualPatchTargetMetadata(text);
					flag2 |= HasPatchTarget(text, "NCreature", new string[2] { "_Ready", "SetAnimationTrigger" });
					flag3 |= HasPatchTarget(text, "NCharacterSelectScreen", new string[1] { "SelectCharacter" }) || HasPatchTarget(text, "NCharacterSelectButton", new string[1] { "Init" });
				}
			}
			if (SkinProviderPatchEvidencePolicy.IsSkinEvidence(flag, flag2, flag3))
			{
				return true;
			}
			return HasRuntimeSpineSkinPatchMetadata(assemblyPath);
		}
		static bool HasPatchTarget(string metadata, string typeName, string[] members)
		{
			if (metadata.Contains(typeName, StringComparison.Ordinal))
			{
				return members.Any((string member) => metadata.Contains(member, StringComparison.Ordinal));
			}
			return false;
		}
	}

	private static bool HasRuntimeSpineSkinPatchMetadata(string assemblyPath)
	{
		try
		{
			byte[] bytes2 = File.ReadAllBytes(assemblyPath);
			if (!ContainsBinaryString(bytes2, "Harmony", StringComparison.OrdinalIgnoreCase) || !ContainsBinaryString(bytes2, "spineskins/", StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			int num = 0;
			if (ContainsBinaryString(bytes2, "NCreature", StringComparison.Ordinal) && ContainsBinaryString(bytes2, "_Ready", StringComparison.Ordinal))
			{
				num++;
			}
			if (ContainsBinaryString(bytes2, "NRestSiteCharacter", StringComparison.Ordinal) && ContainsBinaryString(bytes2, "_Ready", StringComparison.Ordinal))
			{
				num++;
			}
			if (ContainsBinaryString(bytes2, "NMerchantRoom", StringComparison.Ordinal) && ContainsBinaryString(bytes2, "AfterRoomIsLoaded", StringComparison.Ordinal))
			{
				num++;
			}
			return num >= 2;
		}
		catch
		{
			return false;
		}
		static bool ContainsBinaryString(byte[] bytes, string value, StringComparison comparison)
		{
			if (Encoding.Latin1.GetString(bytes).Contains(value, comparison))
			{
				return true;
			}
			byte[] bytes3 = Encoding.Unicode.GetBytes(value);
			return bytes.AsSpan().IndexOf(bytes3) >= 0;
		}
	}

	internal static bool HasDirectVisualPatchTargetMetadata(string value)
	{
		if (!HasPatchTarget(value, "CharacterModel", new string[3] { "CreateVisuals", "CharacterSelectIcon", "IconTexture" }) && !HasPatchTarget(value, "MonsterModel", new string[1] { "CreateVisuals" }) && !HasPatchTarget(value, "EventModel", new string[3] { "CreateBackgroundScene", "MapIcon", "RunHistoryIcon" }) && !HasPatchTarget(value, "NMerchantButton", new string[3] { "_Ready", "MerchantVisual", "SetSkin" }) && !HasPatchTarget(value, "NMerchantHand", new string[2] { "_Ready", "skeleton" }) && !HasPatchTarget(value, "CardModel", new string[2] { "Portrait", "PortraitPath" }) && !HasPatchTarget(value, "AssetCache", new string[3] { "GetScene", "GetTexture2D", "GetAsset" }))
		{
			return HasPatchTarget(value, "AtlasManager", new string[2] { "GetSprite", "LoadAtlas" });
		}
		return true;
		static bool HasPatchTarget(string metadata, string typeName, string[] members)
		{
			if (metadata.Contains(typeName, StringComparison.Ordinal))
			{
				return members.Any((string member) => metadata.Contains(member, StringComparison.Ordinal));
			}
			return false;
		}
	}

	private static void AppendHarmonyPatchMetadata(MetadataReader reader, CustomAttributeHandleCollection attributes, StringBuilder destination)
	{
		foreach (CustomAttributeHandle item in attributes)
		{
			CustomAttribute customAttribute = reader.GetCustomAttribute(item);
			if (GetAttributeTypeName(reader, customAttribute.Constructor).Equals("HarmonyPatch", StringComparison.Ordinal))
			{
				byte[] blobBytes = reader.GetBlobBytes(customAttribute.Value);
				destination.Append(Encoding.UTF8.GetString(blobBytes));
				destination.Append('\n');
			}
		}
	}

	private static string GetAttributeTypeName(MetadataReader reader, EntityHandle constructor)
	{
		EntityHandle entityHandle;
		switch (constructor.Kind)
		{
		case HandleKind.MemberReference:
			entityHandle = reader.GetMemberReference((MemberReferenceHandle)constructor).Parent;
			break;
		case HandleKind.MethodDefinition:
			entityHandle = reader.GetMethodDefinition((MethodDefinitionHandle)constructor).GetDeclaringType();
			break;
		default:
			return string.Empty;
		}
		return entityHandle.Kind switch
		{
			HandleKind.TypeDefinition => reader.GetString(reader.GetTypeDefinition((TypeDefinitionHandle)entityHandle).Name), 
			HandleKind.TypeReference => reader.GetString(reader.GetTypeReference((TypeReferenceHandle)entityHandle).Name), 
			_ => string.Empty, 
		};
	}

	public bool IsRuntimeProviderOption(string groupId, string optionId)
	{
		SkinGroup? skinGroup = Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null)
		{
			return false;
		}
		return skinGroup.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))?.IsRuntimeProvider == true;
	}

	public bool TryGetSelectedFrameworkContract(string groupId, string? optionId, out FrameworkCharacterSkinContract contract)
	{
		contract = Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))?.FrameworkContract;
		return contract != null;
	}

	public bool ProviderUsesManagedCharacterScene(string groupId, string optionId)
	{
		string item = ResolveVisualProviderId(optionId);
		if (!_managedGodotScriptProviders.Contains(item))
		{
			return false;
		}
		return (Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase)))?.Assets.Keys.Any(delegate(string path)
		{
			Match match = CreatureVisualSceneRegex().Match(path);
			return match.Success && match.Groups[1].Value.Equals(groupId, StringComparison.OrdinalIgnoreCase);
		}) ?? false;
	}

	public bool ProviderUsesManagedGodotScripts(string optionId)
	{
		return _managedGodotScriptProviders.Contains(ResolveVisualProviderId(optionId));
	}

	public bool ProviderUsesInteractiveRuntime(string optionId)
	{
		return _interactiveRuntimeProviders.Contains(ResolveVisualProviderId(optionId));
	}

	public bool ProviderUsesDirectCharacterRuntime(string optionId)
	{
		return _directCharacterRuntimeProviders.Contains(ResolveVisualProviderId(optionId));
	}

	public IReadOnlySet<string> GetSelectedDirectCharacterRuntimeProviders(IReadOnlyDictionary<string, string> selections)
	{
		string value;
		return _directCharacterRuntimeProviders.Where((string providerId) => _groups.Any((SkinGroup group) => selections.TryGetValue(group.Id, out value) && SelectionUsesProvider(group.Id, value, providerId))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public bool ProviderRequiresCoherentRuntimePackage(string providerId)
	{
		if (!ProviderUsesFullRuntime(providerId) && !ProviderUsesInteractiveRuntime(providerId) && !ProviderUsesManagedGodotScripts(providerId))
		{
			return ProviderUsesDirectCharacterRuntime(providerId);
		}
		return true;
	}

	public IReadOnlySet<string> GetSelectedInteractiveRuntimeProviders(IReadOnlyDictionary<string, string> selections)
	{
		string value;
		return (from @group in _groups
			where selections.TryGetValue(@group.Id, out value)
			select ResolveVisualProviderId(selections[@group.Id])).Where(ProviderUsesInteractiveRuntime).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public bool ProviderUsesScopedMonsterRuntime(string optionId)
	{
		return _scopedMonsterRuntimeProviders.Contains(ResolveVisualProviderId(optionId));
	}

	public IReadOnlyList<BackgroundPackCandidate> FindBackgroundPackCandidates()
	{
		List<BackgroundPackCandidate> list = new List<BackgroundPackCandidate>();
		foreach (PckResourceIndex cosmeticIndex in _cosmeticIndexes)
		{
			if (!cosmeticIndex.Mod.HasDll && cosmeticIndex.Mod.PckPath == null)
			{
				continue;
			}
			IReadOnlyCollection<string> paths = cosmeticIndex.Archive.Paths;
			List<BackgroundPackShell> list2 = new List<BackgroundPackShell>();
			foreach (string item in paths.Where((string path) => IsScenePath(path, "_background.tscn")))
			{
				if (!IsBattleBackgroundLocation(item))
				{
					continue;
				}
				string text = StripRemap(item);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
				if (fileNameWithoutExtension.EndsWith("_background", StringComparison.OrdinalIgnoreCase))
				{
					string text2 = fileNameWithoutExtension;
					int length = "_background".Length;
					fileNameWithoutExtension = text2.Substring(0, text2.Length - length);
					string directory = text.Substring(0, text.LastIndexOf('/'));
					string[] array = paths.Where((string path) => path.StartsWith(directory + "/layers/", StringComparison.OrdinalIgnoreCase) && IsScenePath(path, ".tscn")).Select(StripRemap).ToArray();
					if (array.Length != 0)
					{
						list2.Add(new BackgroundPackShell(fileNameWithoutExtension, text, array));
					}
				}
			}
			if (list2.Count != 0)
			{
				list.Add(new BackgroundPackCandidate(cosmeticIndex.Mod.Id, cosmeticIndex.Mod.Name, list2));
			}
		}
		return list;
	}

	internal static bool IsBattleBackgroundLocation(string path)
	{
		string text = path.ToLowerInvariant();
		string[] nonCombatBackgroundSegments = NonCombatBackgroundSegments;
		foreach (string value in nonCombatBackgroundSegments)
		{
			if (text.Contains(value, StringComparison.Ordinal))
			{
				return false;
			}
		}
		if (!text.Contains("/backgrounds/", StringComparison.Ordinal))
		{
			return text.Contains("/battlefields/", StringComparison.Ordinal);
		}
		return true;
	}

	internal static bool IsScenePath(string path, string suffix)
	{
		if (!path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(suffix + ".remap", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	internal static string StripRemap(string path)
	{
		if (!path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		int length = ".remap".Length;
		return path.Substring(0, path.Length - length);
	}

	public IReadOnlyList<string> GetProviderResourcePackPaths(string optionId)
	{
		return (from index in _cosmeticIndexes
			where index.Mod.Id.Equals(ResolveVisualProviderId(optionId), StringComparison.OrdinalIgnoreCase)
			select index.Archive.Path).Where(File.Exists).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public IReadOnlyList<string> GetRuntimeProviderGroups(string optionId)
	{
		string providerId = ResolveVisualProviderId(optionId);
		return (from @group in _groups
			where @group.Options.Any((SkinOption option) => option.EffectiveProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase))
			select @group.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public IReadOnlyList<string> GetScopedMonsterRuntimeProviderGroups(string optionId)
	{
		return _scopedMonsterRuntimeProviderGroups.GetValueOrDefault(optionId) ?? Array.Empty<string>();
	}

	public IReadOnlySet<string> GetSelectedScopedMonsterRuntimeProviders(IReadOnlyDictionary<string, string> selections)
	{
		string value;
		return _scopedMonsterRuntimeProviders.Where((string providerId) => _scopedMonsterRuntimeProviderGroups[providerId].Any((string groupId) => selections.TryGetValue(groupId, out value) && SelectionUsesProvider(groupId, value, providerId))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public string? ResolveManagedMonsterGroupId(string monsterId)
	{
		string monsterToken = NormalizeResourceToken(monsterId);
		return _groups.FirstOrDefault((SkinGroup group) => NormalizeResourceToken(group.Id).Equals(monsterToken, StringComparison.OrdinalIgnoreCase))?.Id;
	}

	public bool ProviderUsesFullRuntime(string optionId)
	{
		return _fullRuntimeProviders.Contains(ResolveVisualProviderId(optionId));
	}

	public IReadOnlyList<string> GetFullRuntimeProviderGroups(string optionId)
	{
		return _fullRuntimeProviderGroups.GetValueOrDefault(ResolveVisualProviderId(optionId)) ?? Array.Empty<string>();
	}

	public bool IsCharacterAppearanceGroup(string groupId)
	{
		SkinGroup skinGroup = Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup != null)
		{
			return CharacterSkinCompositionPolicy.CanComposeCharacterGroup(_characterAppearanceGroupIds.Contains(skinGroup.Id), skinGroup.Options.Any(IsCharacterAppearanceOption), skinGroup.Options.Any((SkinOption option) => option.IsRuntimeProvider));
		}
		return false;
	}

	public bool IsFullRuntimeProviderFullySelected(string optionId, IReadOnlyDictionary<string, string> selections)
	{
		string providerId = ResolveVisualProviderId(optionId);
		if (!_fullRuntimeProviderGroups.TryGetValue(providerId, out IReadOnlyList<string> value) || value.Count == 0)
		{
			return false;
		}
		string value2;
		return value.All((string groupId) => selections.TryGetValue(groupId, out value2) && SelectionUsesProvider(groupId, value2, providerId));
	}

	public IReadOnlySet<string> GetFullySelectedFullRuntimeProviders(IReadOnlyDictionary<string, string> selections)
	{
		return _fullRuntimeProviders.Where((string providerId) => IsFullRuntimeProviderFullySelected(providerId, selections)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	public IReadOnlyDictionary<string, string> BuildVisualSelectionTransaction(string groupId, string optionId, IReadOnlyDictionary<string, string> selections)
	{
		Dictionary<string, string> updates = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		string requestedProviderId = ResolveVisualProviderId(optionId);
		string value;
		foreach (string item in (from targetGroupId in RuntimeProviderScopePolicy.ResolveCharacterSelectionTargets(ProviderUsesFullRuntime(requestedProviderId), GetFullRuntimeProviderGroups(requestedProviderId), groupId)
			select (!selections.TryGetValue(targetGroupId, out value)) ? null : ResolveVisualProviderId(value) into providerId
			where providerId != null && ProviderUsesFullRuntime(providerId) && !providerId.Equals(requestedProviderId, StringComparison.OrdinalIgnoreCase)
			select providerId).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			foreach (string fullRuntimeProviderGroup in GetFullRuntimeProviderGroups(item))
			{
				if (selections.TryGetValue(fullRuntimeProviderGroup, out string value2) && SelectionUsesProvider(fullRuntimeProviderGroup, value2, item))
				{
					ModLog.Info($"分组 {fullRuntimeProviderGroup} 的皮肤 {item} 被 {requestedProviderId} 接管，退回原版。");
					updates[fullRuntimeProviderGroup] = "__base__";
				}
			}
		}
		updates[groupId] = optionId;
		if (ProviderUsesFullRuntime(requestedProviderId))
		{
			foreach (string ownedGroupId in GetFullRuntimeProviderGroups(requestedProviderId))
			{
				if (!ownedGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase))
				{
					SkinOption skinOption = _groups.First((SkinGroup group) => group.Id.Equals(ownedGroupId, StringComparison.OrdinalIgnoreCase)).Options.FirstOrDefault((SkinOption candidate) => !candidate.IsComposition && candidate.EffectiveProviderId.Equals(requestedProviderId, StringComparison.OrdinalIgnoreCase));
					if (skinOption != null)
					{
						ModLog.Info($"全量运行时提供者 {requestedProviderId} 同步应用到分组 {ownedGroupId}（选择 {groupId} 时）。");
						updates[ownedGroupId] = skinOption.Id;
					}
				}
			}
		}
		if (SharedAnimationSiblingGroups.TryGetValue(groupId, out string[] value3))
		{
			SkinOption selectedOption = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
			string[] array = value3;
			foreach (string siblingGroupId in array)
			{
				if (siblingGroupId.Equals(groupId, StringComparison.OrdinalIgnoreCase) || updates.ContainsKey(siblingGroupId))
				{
					continue;
				}
				SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(siblingGroupId, StringComparison.OrdinalIgnoreCase));
				if (skinGroup == null)
				{
					continue;
				}
				if (optionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
				{
					updates[skinGroup.Id] = "__base__";
					continue;
				}
				SkinOption skinOption2 = skinGroup.Options.FirstOrDefault((SkinOption option) => !option.IsComposition && (option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase) || (selectedOption != null && option.EffectiveProviderId.Equals(selectedOption.EffectiveProviderId, StringComparison.OrdinalIgnoreCase) && option.Name.Equals(selectedOption.Name, StringComparison.Ordinal))));
				if (skinOption2 != null)
				{
					updates[skinGroup.Id] = skinOption2.Id;
					ModLog.Info($"共享骨骼分组 {groupId} 的 {optionId} 同步应用到 {skinGroup.Id}。");
				}
			}
		}
		if (!CompanionGroups.Any(((string Owner, string Companion) pair) => updates.ContainsKey(pair.Owner) || updates.ContainsKey(pair.Companion)))
		{
			return updates;
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>(selections, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, string> item2 in updates)
		{
			dictionary[item2.Key] = item2.Value;
		}
		foreach (KeyValuePair<string, string> item3 in BuildCompanionSelectionUpdates(dictionary, updates.Keys))
		{
			updates[item3.Key] = item3.Value;
		}
		return updates;
	}

	public IReadOnlyDictionary<string, string> BuildCompanionSelectionUpdates(IReadOnlyDictionary<string, string> selections, IEnumerable<string>? affectedGroups = null)
	{
		HashSet<string> hashSet = affectedGroups?.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		(string, string)[] companionGroups = CompanionGroups;
		for (int i = 0; i < companionGroups.Length; i++)
		{
			var (ownerId, companionId) = companionGroups[i];
			if (hashSet == null || hashSet.Contains(ownerId) || hashSet.Contains(companionId))
			{
				SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(ownerId, StringComparison.OrdinalIgnoreCase));
				SkinGroup skinGroup2 = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(companionId, StringComparison.OrdinalIgnoreCase));
				if (skinGroup != null && skinGroup2 != null)
				{
					dictionary[skinGroup2.Id] = ResolveCompanionSelection(skinGroup, selections.GetValueOrDefault(skinGroup.Id) ?? "__base__", skinGroup2);
				}
			}
		}
		return dictionary;
	}

	private string ResolveCompanionSelection(SkinGroup owner, string ownerOptionId, SkinGroup companion)
	{
		SkinOption skinOption = owner.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(ownerOptionId, StringComparison.OrdinalIgnoreCase));
		if (skinOption == null || ownerOptionId.Equals("__base__", StringComparison.OrdinalIgnoreCase))
		{
			return "__base__";
		}
		List<string> list = (from sourceId in GetCompositionSourceOptionIds(owner.Id, ownerOptionId)
			select owner.Options.FirstOrDefault((SkinOption option) => !option.IsComposition && option.Id.Equals(sourceId, StringComparison.OrdinalIgnoreCase)) into option
			where option != null
			select option into source
			select companion.Options.FirstOrDefault((SkinOption option) => !option.IsComposition && option.Id.Equals(source.Id, StringComparison.OrdinalIgnoreCase)) ?? companion.Options.FirstOrDefault((SkinOption option) => !option.IsComposition && option.EffectiveProviderId.Equals(source.EffectiveProviderId, StringComparison.OrdinalIgnoreCase)) into option
			where option != null
			select option.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		if (list.Count == 0)
		{
			return "__base__";
		}
		if (list.Count == 1)
		{
			return list[0];
		}
		string id = (skinOption.IsSessionComposition ? "companion:session:" : "companion:saved:") + CharacterSkinCompositionPolicy.CreateSessionId(companion.Id, list);
		if (companion.Options.Any((SkinOption option) => option.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
		{
			return id;
		}
		CharacterSkinComposition composition = new CharacterSkinComposition
		{
			Id = id,
			GroupId = companion.Id,
			Name = skinOption.Name,
			SourceOptionIds = list
		};
		if (!TryBuildCompositionOption(companion, composition, skinOption.IsSessionComposition, out SkinOption option2))
		{
			return "__base__";
		}
		companion.Options.Add(option2);
		return id;
	}

	public string ResolveVisualProviderId(string optionOrProviderId)
	{
		if (string.IsNullOrWhiteSpace(optionOrProviderId))
		{
			return optionOrProviderId;
		}
		return _groups.SelectMany((SkinGroup group) => group.Options).FirstOrDefault((SkinOption option) => option.Id.Equals(optionOrProviderId, StringComparison.OrdinalIgnoreCase))?.EffectiveProviderId ?? optionOrProviderId;
	}

	public string ResolveStoredVisualSelectionId(string groupId, string selectionId)
	{
		SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null || skinGroup.Options.Any((SkinOption option) => option.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase)))
		{
			return selectionId;
		}
		foreach (SkinOption option in skinGroup.Options)
		{
			ProviderInstanceIdentity providerInstanceIdentity = FindProviderIdentity(option.EffectiveProviderId);
			if (providerInstanceIdentity != null && ProviderInstanceIdentityPolicy.IsOptionSelectionAlias(providerInstanceIdentity.ManifestId, providerInstanceIdentity.InstanceId, option.Id, selectionId))
			{
				return option.Id;
			}
		}
		return selectionId;
	}

	public string ResolveStoredCardSelectionId(string groupId, string selectionId)
	{
		CardSkinGroup cardSkinGroup = _cardGroups.FirstOrDefault((CardSkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (cardSkinGroup == null || cardSkinGroup.Options.Any((CardSkinOption option) => option.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase)))
		{
			return selectionId;
		}
		foreach (CardSkinOption option in cardSkinGroup.Options)
		{
			ProviderInstanceIdentity providerInstanceIdentity = FindProviderIdentity(option.ProviderId ?? option.Id);
			if (providerInstanceIdentity != null && ProviderInstanceIdentityPolicy.IsOptionSelectionAlias(providerInstanceIdentity.ManifestId, providerInstanceIdentity.InstanceId, option.Id, selectionId))
			{
				return option.Id;
			}
		}
		return selectionId;
	}

	public string ResolveStoredProviderId(string providerId, IReadOnlySet<string>? allowedProviderIds = null)
	{
		if (_providerInstanceIdentities.Any((ProviderInstanceIdentity identity) => identity.InstanceId.Equals(providerId, StringComparison.OrdinalIgnoreCase) && (allowedProviderIds == null || allowedProviderIds.Contains(identity.InstanceId))))
		{
			return providerId;
		}
		foreach (ProviderInstanceIdentity providerInstanceIdentity in _providerInstanceIdentities)
		{
			if ((allowedProviderIds == null || allowedProviderIds.Contains(providerInstanceIdentity.InstanceId)) && ProviderInstanceIdentityPolicy.IsOptionSelectionAlias(providerInstanceIdentity.ManifestId, providerInstanceIdentity.InstanceId, providerInstanceIdentity.InstanceId, providerId))
			{
				return providerInstanceIdentity.InstanceId;
			}
		}
		return providerId;
	}

	private ProviderInstanceIdentity? FindProviderIdentity(string providerId)
	{
		return _providerInstanceIdentities.FirstOrDefault((ProviderInstanceIdentity identity) => identity.InstanceId.Equals(providerId, StringComparison.OrdinalIgnoreCase));
	}

	private bool SelectionUsesProvider(string groupId, string selectionId, string providerId)
	{
		SkinGroup? skinGroup = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null)
		{
			return false;
		}
		return skinGroup.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase))?.EffectiveProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase) == true;
	}

	public bool SelectionUsesVisualProvider(string groupId, string selectionId, string providerId)
	{
		return SelectionUsesProvider(groupId, selectionId, ResolveVisualProviderId(providerId));
	}

	public bool IsResourceBackedOption(string groupId, string optionId)
	{
		SkinOption skinOption = Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		if ((object)skinOption != null)
		{
			IReadOnlyDictionary<string, ResourceAsset> assets = skinOption.Assets;
			if (assets != null && assets.Count > 0)
			{
				return true;
			}
		}
		return skinOption?.ManagedMonsterScene != null;
	}

	public string? GetRuntimeImagePath(string groupId, string optionId)
	{
		return Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))?.RuntimeImagePath;
	}

	public RuntimeMonsterVisualMode? GetRuntimeMonsterVisualMode(string groupId, string optionId)
	{
		return Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase))?.RuntimeMonsterVisualMode;
	}

	public AncientLayeredImagePaths? GetAncientLayeredImagePaths(string groupId, string optionId)
	{
		SkinOption skinOption = Groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((SkinOption candidate) => candidate.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		if (skinOption == null)
		{
			return null;
		}
		string text = null;
		string backgroundCover = null;
		string mask = null;
		string sleepingCharacter = null;
		foreach (string key in skinOption.Assets.Keys)
		{
			Match match = AncientLayerImageRegex().Match(NormalizeTakeoverPath(key));
			if (match.Success && match.Groups["id"].Value.Equals(groupId, StringComparison.OrdinalIgnoreCase))
			{
				switch (match.Groups["kind"].Value.ToLowerInvariant())
				{
				case "character":
					text = key;
					break;
				case "background_cover":
					backgroundCover = key;
					break;
				case "character_mask":
					mask = key;
					break;
				case "character_sleeping":
					sleepingCharacter = key;
					break;
				}
			}
		}
		if (text != null)
		{
			return new AncientLayeredImagePaths(text, backgroundCover, mask, sleepingCharacter);
		}
		return null;
	}

	public string? FindGroupIdForResourcePath(string resourcePath)
	{
		if (_resourceGroupIds.TryGetValue(NormalizeTakeoverPath(resourcePath), out string value))
		{
			return value;
		}
		string text = ((!EventSkinPolicy.CouldOwnResource(resourcePath)) ? null : EventSkinPolicy.FindResourceOwner(resourcePath, Groups.Where((SkinGroup group) => EventSkinPolicy.IsEventGroup(group.Id)).Select(delegate(SkinGroup group)
		{
			string id = group.Id;
			int length = "event:".Length;
			return id.Substring(length, id.Length - length);
		})));
		if (text != null)
		{
			return EventSkinPolicy.GroupId(text);
		}
		GroupIdentity identity = TryGetPrimaryGroup(resourcePath) ?? TryGetCharacterSupplementGroup(resourcePath) ?? TryGetCharacterSelectIconGroup(resourcePath) ?? TryGetCharacterUiTextureGroup(resourcePath) ?? TryGetCharacterMapMarkerGroup(resourcePath) ?? TryGetCharacterIconSceneGroup(resourcePath) ?? TryGetAncientIconGroup(resourcePath);
		if (!(identity != null) || !Groups.Any((SkinGroup group) => group.Id.Equals(identity.Id, StringComparison.OrdinalIgnoreCase)))
		{
			return null;
		}
		return identity.Id;
	}

	public ResourceAsset? ResolveBaseline(string sourcePath)
	{
		for (int num = _baselineIndexes.Count - 1; num >= 0; num--)
		{
			PckResourceIndex pckResourceIndex = _baselineIndexes[num];
			if (pckResourceIndex.Assets.TryGetValue(sourcePath, out ResourceAsset value))
			{
				return value;
			}
			ResourceAsset resourceAsset = pckResourceIndex.TryBuildAsset(sourcePath);
			if (resourceAsset != null)
			{
				return resourceAsset;
			}
		}
		return null;
	}

	public Dictionary<string, ResourceFile> BuildOverlay(IReadOnlyDictionary<string, string> selections, IReadOnlySet<string>? onlyGroups = null)
	{
		Dictionary<string, ResourceFile> dictionary = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, ResourceFile> passthroughLocalizationFile in _passthroughLocalizationFiles)
		{
			dictionary[passthroughLocalizationFile.Key] = passthroughLocalizationFile.Value;
		}
		SkinGroup[] array = Groups.Where((SkinGroup group) => onlyGroups == null || onlyGroups.Contains(group.Id)).ToArray();
		SkinOption[] source = (from option in array.Select(delegate(SkinGroup @group)
			{
				selections.TryGetValue(@group.Id, out string selectedId);
				return @group.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
			})
			where option?.IsRuntimeProvider ?? false
			select option).Cast<SkinOption>().GroupBy<SkinOption, string>((SkinOption option) => option.Id, StringComparer.OrdinalIgnoreCase).Select(delegate(IGrouping<string, SkinOption> options)
		{
			SkinOption skinOption = options.First();
			Dictionary<string, ResourceAsset> assets = options.SelectMany((SkinOption option) => option.Assets).GroupBy<KeyValuePair<string, ResourceAsset>, string>((KeyValuePair<string, ResourceAsset> pair) => pair.Key, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, KeyValuePair<string, ResourceAsset>>, string, ResourceAsset>((IGrouping<string, KeyValuePair<string, ResourceAsset>> pairs) => pairs.Key, (IGrouping<string, KeyValuePair<string, ResourceAsset>> pairs) => pairs.Last().Value, StringComparer.OrdinalIgnoreCase);
			return skinOption with
			{
				Assets = assets
			};
		})
			.ToArray();
		HashSet<string> selectableProviderFiles = (from file in (from option in Groups.SelectMany((SkinGroup @group) => @group.Options)
				where option.IsRuntimeProvider
				select option).SelectMany((SkinOption option) => option.Assets.Values).SelectMany((ResourceAsset asset) => asset.Files).Concat(_pckCardOptions.SelectMany((CardSkinOption option) => option.Assets.Values).SelectMany((ResourceAsset asset) => asset.Files))
				.Concat(_configuredCardGroups.SelectMany((CardSkinGroup @group) => @group.Options).SelectMany((CardSkinOption option) => option.Assets.Values).SelectMany((ResourceAsset asset) => asset.Files))
			select NormalizeTakeoverPath(file.Path)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, IReadOnlySet<string>> dictionary2 = ((IEnumerable<SkinOption>)source).ToDictionary((Func<SkinOption, string>)((SkinOption option) => option.Id), (Func<SkinOption, IReadOnlySet<string>>)GetIsolatedRelicProviderPaths, (IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase);
		foreach (string item in (from option in array.SelectMany((SkinGroup @group) => @group.Options)
			select option.EffectiveProviderId).Where(ProviderUsesFullRuntime).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			foreach (KeyValuePair<string, ResourceFile> item2 in CollectFullRuntimeProviderBaselineOverlay(item))
			{
				dictionary[item2.Key] = item2.Value;
			}
		}
		foreach (SkinOption item3 in source.Where((SkinOption option) => ProviderUsesFullRuntime(option.Id) && IsFullRuntimeProviderFullySelected(option.Id, selections)))
		{
			foreach (KeyValuePair<string, ResourceFile> item4 in CollectSelectedProviderOverlayDependencies(item3))
			{
				if (!dictionary2[item3.Id].Contains(NormalizeTakeoverPath(item4.Key)) && ShouldMountProviderDependency(item3, item4.Key, selectableProviderFiles))
				{
					dictionary[item4.Key] = item4.Value;
				}
			}
		}
		SkinGroup[] array2 = array;
		foreach (SkinGroup skinGroup in array2)
		{
			selections.TryGetValue(skinGroup.Id, out string selectedId2);
			SkinOption skinOption2 = FrameworkRegistryCooperation.FilterAssets(skinGroup.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selectedId2, StringComparison.OrdinalIgnoreCase)));
			foreach (string item5 in skinGroup.Options.SelectMany((SkinOption option) => option.Assets.Keys).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				string text = NormalizeTakeoverPath(item5);
				if (AncientBackgroundSceneRegex().IsMatch(text))
				{
					continue;
				}
				ResourceAsset value;
				ResourceAsset resourceAsset = ((!IsRelicAtlasSpritePath(text) && skinOption2 != null && skinOption2.Assets.TryGetValue(item5, out value)) ? value : ResolveBaseline(item5));
				if (resourceAsset == null)
				{
					continue;
				}
				foreach (ResourceFile file in resourceAsset.Files)
				{
					string text2 = MapAssetFilePath(item5, resourceAsset.SourcePath, file.Path);
					dictionary[text2] = file;
					string text3 = NormalizeTakeoverPath(text2);
					if (!text3.Equals(text2, StringComparison.OrdinalIgnoreCase))
					{
						dictionary[text3] = file;
					}
				}
			}
		}
		foreach (string item6 in array.SelectMany((SkinGroup group) => group.Options).SelectMany((SkinOption option) => option.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => IsRelicAtlasSpritePath(pair.Key)).SelectMany((KeyValuePair<string, ResourceAsset> pair) => EnumerateDependencyPaths(pair.Value))).Where(IsRelicAtlasTexturePath)
			.Concat(array.SelectMany((SkinGroup group) => group.Options).SelectMany(GetIsolatedRelicProviderPaths).Where(IsRelicAtlasTexturePath))
			.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			ResourceAsset resourceAsset2 = ResolveBaseline(item6);
			if (resourceAsset2 == null)
			{
				continue;
			}
			foreach (ResourceFile file2 in resourceAsset2.Files)
			{
				dictionary[file2.Path] = file2;
			}
		}
		foreach (SkinOption item7 in source.Where((SkinOption option) => !ProviderUsesFullRuntime(option.Id)))
		{
			foreach (KeyValuePair<string, ResourceFile> item8 in CollectSelectedProviderOverlayDependencies(item7))
			{
				if (!dictionary2[item7.Id].Contains(NormalizeTakeoverPath(item8.Key)) && ShouldMountProviderDependency(item7, item8.Key, selectableProviderFiles))
				{
					dictionary[item8.Key] = item8.Value;
				}
			}
		}
		AddFakeMerchantEventPresentation(dictionary);
		return dictionary;
	}

	internal IReadOnlySet<string> GetIsolatedRelicProviderPaths(SkinOption selected)
	{
		if (!selected.IsComposition && _isolatedRelicProviderPaths.TryGetValue(selected.EffectiveProviderId, out IReadOnlySet<string> value))
		{
			return value;
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex selectionProviderIndex in GetSelectionProviderIndexes(selected))
		{
			foreach (KeyValuePair<string, ResourceAsset> item in selectionProviderIndex.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => IsRelicAtlasSpritePath(pair.Key) || IsRelicAtlasTexturePath(pair.Key)))
			{
				hashSet.Add(NormalizeTakeoverPath(item.Key));
				foreach (ResourceFile file in item.Value.Files)
				{
					hashSet.Add(NormalizeTakeoverPath(file.Path));
				}
			}
		}
		if (!selected.IsComposition)
		{
			_isolatedRelicProviderPaths[selected.EffectiveProviderId] = hashSet;
		}
		return hashSet;
	}

	internal IReadOnlyList<string> GetProviderRelicSpritePaths(SkinOption selected)
	{
		return selected.Assets.Keys.Where(IsRelicAtlasSpritePath).Concat(GetOptionProviderIds(selected).SelectMany((string providerId) => GetProviderRelicAssets(providerId).Keys)).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			.Order<string>(StringComparer.OrdinalIgnoreCase)
			.ToArray();
	}

	internal bool IsSelectedCharacterStandaloneTexture(string resourcePath, IReadOnlyDictionary<string, string> selections)
	{
		if (string.IsNullOrWhiteSpace(resourcePath) || IsRelicAtlasTexturePath(resourcePath) || !new string[4] { ".png", ".jpg", ".webp", ".ctex" }.Any((string ext) => resourcePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
		{
			return false;
		}
		foreach (SkinGroup group in Groups)
		{
			if (IsCharacterAppearanceGroup(group.Id) && selections.TryGetValue(group.Id, out string selection))
			{
				SkinOption skinOption = group.Options.FirstOrDefault((SkinOption o) => o.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
				if (!(skinOption == null) && !skinOption.Id.Equals("__base__", StringComparison.OrdinalIgnoreCase) && GetSelectionProviderIndexes(skinOption).Any((PckResourceIndex index) => index.Archive.Contains(resourcePath) || index.Archive.Contains(resourcePath + ".import")))
				{
					return true;
				}
			}
		}
		return false;
	}

	public string? FindSelectedRelicIconGroup(string resourcePath, IReadOnlyDictionary<string, string> selections, IReadOnlyList<string> providerPriority)
	{
		string text = NormalizeTakeoverPath(resourcePath);
		if (!IsRelicAtlasSpritePath(text))
		{
			return null;
		}
		List<(SkinGroup, SkinOption, string)> list = new List<(SkinGroup, SkinOption, string)>();
		foreach (SkinGroup group in Groups)
		{
			if (!selections.TryGetValue(group.Id, out string selectedId))
			{
				continue;
			}
			SkinOption skinOption = group.Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
			if (!(skinOption != null) || !IsCharacterAppearanceOption(skinOption) || !TryResolveProviderAsset(skinOption, text, out ResourceAsset asset))
			{
				continue;
			}
			string item = asset.Files.Select((ResourceFile file) => _cosmeticIndexes.FirstOrDefault((PckResourceIndex index) => index.Archive == file.Archive)?.Mod.Id).FirstOrDefault((string providerId) => providerId != null) ?? skinOption.EffectiveProviderId;
			list.Add((group, skinOption, item));
		}
		if (list.Count == 0)
		{
			return null;
		}
		for (int num = providerPriority.Count - 1; num >= 0; num--)
		{
			string providerId2 = providerPriority[num];
			(SkinGroup, SkinOption, string) tuple = list.FirstOrDefault<(SkinGroup, SkinOption, string)>(((SkinGroup Group, SkinOption Option, string ProviderId) candidate) => candidate.ProviderId.Equals(providerId2, StringComparison.OrdinalIgnoreCase));
			if (tuple.Item1 != null)
			{
				return tuple.Item1.Id;
			}
		}
		return list[list.Count - 1].Item1.Id;
	}

	public string? FindRelicIconOwnerGroup(string resourcePath)
	{
		string normalizedPath = NormalizeTakeoverPath(resourcePath);
		if (!IsRelicAtlasSpritePath(normalizedPath))
		{
			return null;
		}
		if (_relicOwnerGroups.TryGetValue(normalizedPath, out string value))
		{
			return value;
		}
		ResourceAsset asset;
		string text = Groups.FirstOrDefault((SkinGroup group) => group.Options.Any((SkinOption option) => IsCharacterAppearanceOption(option) && TryResolveProviderAsset(option, normalizedPath, out asset)))?.Id;
		_relicOwnerGroups[normalizedPath] = text;
		return text;
	}

	public bool TryGetBaselineRelicTextureDefinition(string resourcePath, out BaselineRelicTextureDefinition definition)
	{
		string text = NormalizeTakeoverPath(resourcePath);
		if (_baselineRelicTextureDefinitions.TryGetValue(text, out BaselineRelicTextureDefinition value))
		{
			definition = value;
			return value != null;
		}
		BaselineRelicTextureDefinition baselineRelicTextureDefinition = null;
		ResourceAsset resourceAsset = ResolveBaseline(text);
		ResourceFile resourceFile = ((resourceAsset == null) ? null : FindDirectFile(resourceAsset, text));
		if (resourceFile != null)
		{
			string @string = Encoding.UTF8.GetString(resourceFile.Read());
			string text2 = (from match in EmbeddedResourcePathRegex().Matches(@string)
				select match.Value).FirstOrDefault(IsRelicAtlasTexturePath);
			RelicTextureRect? relicTextureRect = ParseAtlasTextureRect(@string, "region");
			if (text2 != null && relicTextureRect.HasValue)
			{
				baselineRelicTextureDefinition = new BaselineRelicTextureDefinition(text2, relicTextureRect.Value, ParseAtlasTextureRect(@string, "margin").GetValueOrDefault(), AtlasTextureFilterClipRegex().IsMatch(@string));
			}
		}
		_baselineRelicTextureDefinitions[text] = baselineRelicTextureDefinition;
		definition = baselineRelicTextureDefinition;
		return baselineRelicTextureDefinition != null;
	}

	private static RelicTextureRect? ParseAtlasTextureRect(string text, string property)
	{
		Match match = AtlasTextureRectRegex().Match(text);
		while (match.Success)
		{
			if (match.Groups["property"].Value.Equals(property, StringComparison.OrdinalIgnoreCase) && float.TryParse(match.Groups["x"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) && float.TryParse(match.Groups["y"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result2) && float.TryParse(match.Groups["width"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3) && float.TryParse(match.Groups["height"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result4))
			{
				return new RelicTextureRect(result, result2, result3, result4);
			}
			match = match.NextMatch();
		}
		return null;
	}

	internal bool TryResolveProviderAsset(SkinOption selected, string sourcePath, out ResourceAsset asset)
	{
		string key = NormalizeTakeoverPath(sourcePath);
		if (selected.Assets.TryGetValue(key, out asset) || selected.Assets.TryGetValue(sourcePath, out asset))
		{
			return true;
		}
		foreach (string optionProviderId in GetOptionProviderIds(selected))
		{
			if (GetProviderRelicAssets(optionProviderId).TryGetValue(key, out asset))
			{
				return true;
			}
		}
		asset = null;
		return false;
	}

	private static IReadOnlyList<string> GetOptionProviderIds(SkinOption selected)
	{
		if (!selected.IsComposition)
		{
			return new _003C_003Ez__ReadOnlySingleElementList<string>(selected.EffectiveProviderId);
		}
		return selected.CompositionSourceProviderIds;
	}

	private IReadOnlyList<PckResourceIndex> GetSelectionProviderIndexes(SkinOption selected)
	{
		return GetOptionProviderIds(selected).SelectMany((string providerId) => _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase))).ToArray();
	}

	private IReadOnlyDictionary<string, ResourceAsset> GetProviderRelicAssets(string providerId)
	{
		if (_providerRelicAssets.TryGetValue(providerId, out IReadOnlyDictionary<string, ResourceAsset> value))
		{
			return value;
		}
		Dictionary<string, ResourceAsset> dictionary = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex item in _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase)))
		{
			foreach (KeyValuePair<string, ResourceAsset> item2 in item.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => IsRelicAtlasSpritePath(pair.Key)))
			{
				dictionary[NormalizeTakeoverPath(item2.Key)] = item2.Value;
			}
		}
		_providerRelicAssets[providerId] = dictionary;
		return dictionary;
	}

	private static bool IsCharacterAppearanceOption(SkinOption option)
	{
		return option.Assets.Keys.Any((string path) => CharacterPathRegex().IsMatch(path) || CharacterSelectSceneRegex().IsMatch(path) || AnyCharacterSelectSceneRegex().IsMatch(path) || MerchantCharacterSceneRegex().IsMatch(path) || RestSiteCharacterSceneRegex().IsMatch(path) || CharacterSelectIconRegex().IsMatch(path) || CharacterUiTextureRegex().IsMatch(path) || CharacterIconSceneRegex().IsMatch(path) || CharacterMapMarkerRegex().IsMatch(path));
	}

	private static bool IsCharacterIconSourcePath(string sourcePath)
	{
		return TryGetCharacterIconGroup(NormalizeTakeoverPath(sourcePath)) != null;
	}

	private static GroupIdentity? TryGetCharacterIconGroup(string sourcePath)
	{
		return TryGetCharacterSelectIconGroup(sourcePath) ?? TryGetCharacterUiTextureGroup(sourcePath) ?? TryGetCharacterIconSceneGroup(sourcePath) ?? TryGetCharacterMapMarkerGroup(sourcePath);
	}

	internal static bool IsRelicAtlasSpritePath(string path)
	{
		return RelicAtlasSpriteRegex().IsMatch(NormalizeTakeoverPath(path));
	}

	internal static bool IsRelicAtlasTexturePath(string path)
	{
		return RelicAtlasTextureRegex().IsMatch(NormalizeTakeoverPath(path));
	}

	private IReadOnlyDictionary<string, ResourceFile> CollectFullRuntimeProviderBaselineOverlay(string providerId)
	{
		providerId = ResolveVisualProviderId(providerId);
		if (_fullRuntimeProviderBaselineOverlays.TryGetValue(providerId, out IReadOnlyDictionary<string, ResourceFile> value))
		{
			return value;
		}
		Dictionary<string, ResourceFile> dictionary = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase)).SelectMany(delegate(PckResourceIndex index)
		{
			string namespaceToken = NormalizeResourceToken(index.Mod.ResourceNamespaceId);
			return from path in index.Archive.Paths.Where((string path) => !IsProviderProjectControlFile(path)).Select(NormalizeTakeoverPath).Concat(index.Assets.Keys)
				where !IsProviderNamespacePath(path, namespaceToken)
				select path;
		}).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			ResourceAsset resourceAsset = ResolveBaseline(item);
			if (resourceAsset == null)
			{
				continue;
			}
			foreach (ResourceFile file in resourceAsset.Files)
			{
				string text = MapAssetFilePath(item, resourceAsset.SourcePath, file.Path);
				dictionary[text] = file;
				string text2 = NormalizeTakeoverPath(text);
				if (!text2.Equals(text, StringComparison.OrdinalIgnoreCase))
				{
					dictionary[text2] = file;
				}
			}
		}
		_fullRuntimeProviderBaselineOverlays[providerId] = dictionary;
		return dictionary;
	}

	private static bool ShouldMountProviderDependency(SkinOption selected, string dependencyPath, IReadOnlySet<string> selectableProviderFiles)
	{
		string text = NormalizeTakeoverPath(dependencyPath);
		if (AncientBackgroundSceneRegex().IsMatch(StripResourceRedirectSuffix(text)))
		{
			return false;
		}
		if (!selectableProviderFiles.Contains(text))
		{
			return true;
		}
		return (from file in selected.Assets.Values.SelectMany((ResourceAsset asset) => asset.Files)
			select NormalizeTakeoverPath(file.Path)).Contains<string>(text, StringComparer.OrdinalIgnoreCase);
	}

	public Dictionary<string, ResourceFile> BuildCardOverlay(IReadOnlyDictionary<string, string> selections, IReadOnlyDictionary<string, IReadOnlyList<string>> priorityStacks, IReadOnlySet<string>? onlyGroups = null)
	{
		Dictionary<string, ResourceFile> dictionary = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (CardSkinGroup group2 in CardGroups)
		{
			if (onlyGroups != null && !onlyGroups.Contains(group2.Id))
			{
				continue;
			}
			IReadOnlyList<string> value;
			CardSkinOption[] array = (priorityStacks.TryGetValue(group2.Id, out value) ? (from id in value
				select group2.Options.FirstOrDefault((CardSkinOption option) => option.Id.Equals(id, StringComparison.OrdinalIgnoreCase)) into option
				where option != null
				select option).Cast<CardSkinOption>().ToArray() : Array.Empty<CardSkinOption>());
			if (array.Length == 0 && selections.TryGetValue("cards:" + group2.Id, out string selectedId))
			{
				CardSkinOption cardSkinOption = group2.Options.FirstOrDefault((CardSkinOption option) => option.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
				if (cardSkinOption != null)
				{
					array = new CardSkinOption[1] { cardSkinOption };
				}
			}
			CardSkinOption[] array2 = array;
			foreach (CardSkinOption cardSkinOption2 in array2)
			{
				hashSet.Add(cardSkinOption2.ProviderId ?? cardSkinOption2.Id);
			}
			foreach (string item in group2.Options.SelectMany((CardSkinOption option) => option.Assets.Keys).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				ResourceAsset resourceAsset = ResolveBaseline(item);
				if (resourceAsset == null)
				{
					continue;
				}
				foreach (ResourceFile file in resourceAsset.Files)
				{
					string text = MapAssetFilePath(item, resourceAsset.SourcePath, file.Path);
					dictionary[text] = file;
					string text2 = NormalizeTakeoverPath(text);
					if (!text2.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						dictionary[text2] = file;
					}
				}
			}
		}
		foreach (string selection in (from pair in selections
			where pair.Key.StartsWith("cards:item:", StringComparison.OrdinalIgnoreCase)
			select pair.Value).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			CardSkinOption cardSkinOption3 = CardGroups.SelectMany((CardSkinGroup group) => group.Options).FirstOrDefault((CardSkinOption candidate) => candidate.Id.Equals(selection, StringComparison.OrdinalIgnoreCase));
			if (cardSkinOption3 != null)
			{
				hashSet.Add(cardSkinOption3.ProviderId ?? cardSkinOption3.Id);
			}
		}
		foreach (KeyValuePair<string, ResourceFile> item2 in BuildCardProviderNamespaceOverlay(hashSet))
		{
			dictionary[item2.Key] = item2.Value;
		}
		return dictionary;
	}

	public Dictionary<string, ResourceFile> BuildCardProviderNamespaceOverlay(IEnumerable<string> providerIds)
	{
		return BuildProviderNamespaceOverlay(providerIds);
	}

	private Dictionary<string, ResourceFile> BuildProviderNamespaceOverlay(IEnumerable<string> providerIds)
	{
		Dictionary<string, ResourceFile> dictionary = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		foreach (string providerId in providerIds.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			foreach (PckResourceIndex item in _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(providerId, StringComparison.OrdinalIgnoreCase)))
			{
				foreach (ResourceFile item2 in CollectProviderNamespaceFiles(item, item.Mod.ResourceNamespaceId))
				{
					dictionary[item2.Path] = item2;
				}
			}
		}
		return dictionary;
	}

	private static IReadOnlyCollection<ResourceFile> CollectProviderNamespaceFiles(PckResourceIndex index, string providerId)
	{
		string text = NormalizeResourceToken(providerId);
		Dictionary<string, IReadOnlyCollection<ResourceFile>> value = ProviderNamespaceFiles.GetValue(index, (PckResourceIndex _) => new Dictionary<string, IReadOnlyCollection<ResourceFile>>(StringComparer.OrdinalIgnoreCase));
		lock (value)
		{
			if (value.TryGetValue(text, out var value2))
			{
				return value2;
			}
			IReadOnlyCollection<ResourceFile> readOnlyCollection = ScanProviderNamespaceFiles(index, text);
			value.Add(text, readOnlyCollection);
			return readOnlyCollection;
		}
	}

	private static IReadOnlyCollection<ResourceFile> ScanProviderNamespaceFiles(PckResourceIndex index, string idToken)
	{
		HashSet<string> hashSet = index.Archive.Paths.Where((string path) => IsProviderNamespacePath(path, idToken)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Queue<string> queue = new Queue<string>(hashSet);
		string result;
		while (queue.TryDequeue(out result))
		{
			if (!MayContainResourceReferences(result))
			{
				continue;
			}
			string @string = Encoding.UTF8.GetString(index.Archive.ReadFile(result));
			foreach (Match item in ResourcePathRegex().Matches(@string))
			{
				string value = item.Groups[1].Value;
				if (index.Archive.Contains(value) && hashSet.Add(value))
				{
					queue.Enqueue(value);
				}
			}
		}
		return (IReadOnlyCollection<ResourceFile>)(object)hashSet.Select((string path) => new ResourceFile(index.Archive, path)).ToArray();
	}

	private IReadOnlyDictionary<string, ResourceFile> CollectSelectedProviderOverlayDependencies(SkinOption selected)
	{
		PckResourceIndex[] array = _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(selected.EffectiveProviderId, StringComparison.OrdinalIgnoreCase)).ToArray();
		IEnumerable<PckResourceIndex> second = (from file in selected.Assets.Values.SelectMany((ResourceAsset asset) => asset.Files)
			select _cosmeticIndexes.FirstOrDefault((PckResourceIndex index) => index.Archive == file.Archive) into index
			where index != null
			select index).Cast<PckResourceIndex>();
		PckResourceIndex[] array2 = array.Concat(second).Distinct().ToArray();
		Dictionary<string, ResourceFile> files = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		if (ProviderUsesFullRuntime(selected.Id))
		{
			foreach (ResourceFile item2 in from @group in array.SelectMany((PckResourceIndex index) => from path in index.Archive.Paths
					where !IsProviderProjectControlFile(path)
					select new ResourceFile(index.Archive, path)).GroupBy<ResourceFile, string>((ResourceFile file) => file.Path, StringComparer.OrdinalIgnoreCase)
				select @group.Last())
			{
				files[item2.Path] = item2;
			}
		}
		Queue<(PckResourceIndex Index, ResourceFile File)> queue = new Queue<(PckResourceIndex, ResourceFile)>();
		HashSet<string> queued = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		PckResourceIndex[] array3 = array2;
		foreach (PckResourceIndex pckResourceIndex in array3)
		{
			foreach (string item3 in pckResourceIndex.Archive.Paths.Where((string path) => _cosmeticLocalizationPaths.Contains(path)))
			{
				files[item3] = new ResourceFile(pckResourceIndex.Archive, item3);
			}
		}
		foreach (ResourceFile assetFile in selected.Assets.Values.SelectMany((ResourceAsset asset) => asset.Files))
		{
			PckResourceIndex pckResourceIndex2 = array2.FirstOrDefault((PckResourceIndex candidate) => candidate.Archive == assetFile.Archive);
			if (pckResourceIndex2 != null)
			{
				Enqueue(pckResourceIndex2, assetFile, includeInOverlay: true);
			}
		}
		while (true)
		{
			if (!queue.TryDequeue(out var pending))
			{
				break;
			}
			if (!MayContainResourceReferences(pending.File.Path))
			{
				continue;
			}
			string @string = Encoding.UTF8.GetString(pending.File.Read());
			foreach (Match item4 in EmbeddedResourcePathRegex().Matches(@string))
			{
				string value = item4.Value;
				IEnumerable<PckResourceIndex> enumerable = new PckResourceIndex[1] { pending.Index }.Concat(array2.Where((PckResourceIndex index) => index != pending.Index && index.Mod.Id.Equals(pending.Index.Mod.Id, StringComparison.OrdinalIgnoreCase)));
				ResourceAsset resourceAsset = null;
				PckResourceIndex pckResourceIndex3 = null;
				foreach (PckResourceIndex item5 in enumerable)
				{
					resourceAsset = item5.Assets.GetValueOrDefault(value) ?? item5.TryBuildAsset(value);
					if (resourceAsset != null)
					{
						pckResourceIndex3 = item5;
						break;
					}
				}
				if (resourceAsset == null || pckResourceIndex3 == null)
				{
					continue;
				}
				foreach (ResourceFile file in resourceAsset.Files)
				{
					Enqueue(pckResourceIndex3, file, includeInOverlay: true);
				}
				foreach (ResourceAsset siblingAtlasTextureAsset in GetSiblingAtlasTextureAssets(pckResourceIndex3, value))
				{
					foreach (ResourceFile file2 in siblingAtlasTextureAsset.Files)
					{
						Enqueue(pckResourceIndex3, file2, includeInOverlay: true);
					}
				}
			}
		}
		return files;
		void Enqueue(PckResourceIndex index, ResourceFile file, bool includeInOverlay)
		{
			if (includeInOverlay)
			{
				files[file.Path] = file;
			}
			string item = index.Mod.Id + "\n" + file.Path;
			if (queued.Add(item))
			{
				queue.Enqueue((index, file));
			}
		}
	}

	private static bool IsProviderLocalizationFile(string path, string providerId)
	{
		if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase) && TryGetLocalizationProviderId(path, out string providerId2))
		{
			return providerId2.Equals(providerId, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private static bool IsCharacterLocalizationFile(string path)
	{
		return path.EndsWith("/characters.json", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsProviderProjectControlFile(string path)
	{
		if (path.Equals("res://project.binary", StringComparison.OrdinalIgnoreCase) || path.Equals("res://project.godot", StringComparison.OrdinalIgnoreCase) || path.Equals("res://export_presets.cfg", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".gdextension", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (path.StartsWith("res://.godot/", StringComparison.OrdinalIgnoreCase) && !path.StartsWith("res://.godot/imported/", StringComparison.OrdinalIgnoreCase))
		{
			return !path.StartsWith("res://.godot/exported/", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CGetSiblingAtlasTextureAssets_003Ed__152))]
	private static IEnumerable<ResourceAsset> GetSiblingAtlasTextureAssets(PckResourceIndex index, string atlasSourcePath)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetSiblingAtlasTextureAssets_003Ed__152(-2)
		{
			_003C_003E3__index = index,
			_003C_003E3__atlasSourcePath = atlasSourcePath
		};
	}

	private static string? NormalizeAtlasTextureCandidatePath(string path)
	{
		if (path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			string text = path;
			return text.Substring(0, text.Length - 7);
		}
		if (path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
		{
			string text = path;
			return text.Substring(0, text.Length - 6);
		}
		if (!IsAtlasTextureSourcePath(path))
		{
			return null;
		}
		return path;
	}

	private static bool IsAtlasTextureSourcePath(string path)
	{
		if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsProviderNamespacePath(string path, string idToken)
	{
		if (idToken.Length == 0 || !path.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string text = path.Substring(6, path.Length - 6);
		int num = text.IndexOf('/');
		string text2 = NormalizeResourceToken((num < 0) ? text : text.Substring(0, num));
		if (!text2.Equals(idToken, StringComparison.OrdinalIgnoreCase))
		{
			return text2.StartsWith(idToken, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string NormalizeResourceToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	public (IReadOnlySet<string> CardTypes, IReadOnlyList<CardArtPack> UsedPacks) AttachCardArtPacks(IEnumerable<CardArtPack> packs, IEnumerable<CardCatalogEntry> cards)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<CardArtPack> list = new List<CardArtPack>();
		if (_artPackCardGroups == null)
		{
			return (CardTypes: hashSet, UsedPacks: list);
		}
		CardCatalogEntry[] cards2 = cards.Where((CardCatalogEntry card) => !string.IsNullOrWhiteSpace(card.PortraitPath)).ToArray();
		foreach (CardArtPack pack in packs)
		{
			Dictionary<string, (Dictionary<string, ResourceAsset>, Dictionary<string, string>, Dictionary<string, CardPresentationDefinition>)> dictionary = new Dictionary<string, (Dictionary<string, ResourceAsset>, Dictionary<string, string>, Dictionary<string, CardPresentationDefinition>)>(StringComparer.OrdinalIgnoreCase);
			foreach (CardArtPackCard card in pack.Cards)
			{
				CardCatalogEntry cardCatalogEntry = FindCardForPackEntry(card, cards2);
				if (cardCatalogEntry == null || !card.SourcePath.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}
				string catalogGroupId = cardCatalogEntry.CatalogGroupId;
				if (!string.IsNullOrWhiteSpace(catalogGroupId))
				{
					if (!dictionary.TryGetValue(catalogGroupId, out var value))
					{
						value = (new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase), new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase));
						dictionary.Add(catalogGroupId, value);
					}
					ResourceAsset resourceAsset = new ResourceAsset(card.SourcePath);
					resourceAsset.AddInlineFile(card.SourcePath, card.Bytes);
					value.Item1[card.SourcePath] = resourceAsset;
					value.Item2[cardCatalogEntry.TypeName] = card.SourcePath;
					CardPresentationDefinition cardPresentationDefinition = CardArtPackScanPolicy.BuildCardPresentation(card);
					if ((object)cardPresentationDefinition != null)
					{
						value.Item3[cardCatalogEntry.TypeName] = cardPresentationDefinition;
					}
					hashSet.Add(cardCatalogEntry.TypeName);
				}
			}
			if (dictionary.Count == 0)
			{
				continue;
			}
			list.Add(pack);
			BindArtPackWorkshopSource(pack);
			foreach (var (groupId, tuple2) in dictionary)
			{
				AddCardOption(_artPackCardGroups, groupId, new CardSkinOption(pack.Id, pack.Name, tuple2.Item2, new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase), tuple2.Item1, pack.Root, pack.Id, tuple2.Item3));
			}
		}
		return (CardTypes: hashSet, UsedPacks: list);
	}

	private void BindArtPackWorkshopSource(CardArtPack pack)
	{
		ulong num = WorkshopSourceId(pack.Root);
		if (num != 0L && _workshopSourceIds != null)
		{
			_workshopSourceIds[pack.Id] = num;
		}
	}

	public IReadOnlySet<string> GetArtPackCardTypes()
	{
		IEnumerable<CardSkinGroup> enumerable = _artPackCardGroups?.Values;
		return (enumerable ?? Enumerable.Empty<CardSkinGroup>()).SelectMany((CardSkinGroup group) => group.Options).SelectMany((CardSkinOption option) => option.NormalPortraits.Keys).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static CardCatalogEntry? FindCardForPackEntry(CardArtPackCard packCard, IReadOnlyList<CardCatalogEntry> cards)
	{
		return ResolveCardForPortraitStem(packCard.CardId, cards);
	}

	internal static CardCatalogEntry? ResolveCardForPortraitStem(string stem, IReadOnlyList<CardCatalogEntry> cards)
	{
		string text = NormalizeCardToken(stem);
		if (text.Length == 0)
		{
			return null;
		}
		CardCatalogEntry cardCatalogEntry = null;
		foreach (CardCatalogEntry card in cards)
		{
			CardArtIdentity cardArtIdentity = TryGetCardArtIdentity(card.PortraitPath);
			if ((cardArtIdentity != null && CardStemsMatch(text, cardArtIdentity.Stem)) || CardStemsMatch(text, NormalizeCardToken(card.TypeName)))
			{
				if (cardCatalogEntry != null)
				{
					return null;
				}
				cardCatalogEntry = card;
			}
		}
		return cardCatalogEntry;
	}

	public void FinalizeCardGroups(IEnumerable<CardCatalogEntry> cards)
	{
		CardCatalogEntry[] array = cards.Where((CardCatalogEntry card) => !string.IsNullOrWhiteSpace(card.PortraitPath) && !string.IsNullOrWhiteSpace(card.CatalogGroupId) && !string.IsNullOrWhiteSpace(card.FilterGroupId)).ToArray();
		Dictionary<string, CardCatalogEntry> cardsByType = array.GroupBy<CardCatalogEntry, string>((CardCatalogEntry card) => card.TypeName, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, CardCatalogEntry>, string, CardCatalogEntry>((IGrouping<string, CardCatalogEntry> entries) => entries.Key, (IGrouping<string, CardCatalogEntry> entries) => entries.First(), StringComparer.OrdinalIgnoreCase);
		HashSet<string> knownCardGroups = (from @group in array.SelectMany((CardCatalogEntry card) => new string[4]
			{
				card.CatalogGroupId,
				card.FilterGroupId,
				card.PoolGroupId,
				TryGetCardPortraitGroup(card.PortraitPath)
			})
			where !string.IsNullOrWhiteSpace(@group)
			select @group).Cast<string>().ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		IReadOnlyDictionary<string, string> uniqueCardTypesByStem = BuildUniqueCardTypesByStem(array, knownCardGroups);
		Dictionary<string, CardSkinGroup> dictionary = new Dictionary<string, CardSkinGroup>(StringComparer.OrdinalIgnoreCase);
		foreach (CardSkinGroup configuredGroup in _configuredCardGroups)
		{
			foreach (CardSkinOption option4 in configuredGroup.Options)
			{
				AddCardOption(dictionary, configuredGroup.Id, option4);
				foreach (string specialGroupId in (from cardType in option4.NormalPortraits.Keys.Concat(option4.AncientPortraits.Keys).Concat(option4.CardPresentations.Keys)
					select cardsByType.GetValueOrDefault(cardType) into card
					where card != null && !card.FilterGroupId.Equals(configuredGroup.Id, StringComparison.OrdinalIgnoreCase)
					select card.FilterGroupId).Distinct<string>(StringComparer.OrdinalIgnoreCase))
				{
					CardCatalogEntry value;
					Dictionary<string, string> normalPortraits = option4.NormalPortraits.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => cardsByType.TryGetValue(pair.Key, out value) && value.FilterGroupId.Equals(specialGroupId, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
					CardCatalogEntry value2;
					Dictionary<string, AncientCardPortrait> ancientPortraits = option4.AncientPortraits.Where<KeyValuePair<string, AncientCardPortrait>>((KeyValuePair<string, AncientCardPortrait> pair) => cardsByType.TryGetValue(pair.Key, out value2) && value2.FilterGroupId.Equals(specialGroupId, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, AncientCardPortrait>, string, AncientCardPortrait>((KeyValuePair<string, AncientCardPortrait> pair) => pair.Key, (KeyValuePair<string, AncientCardPortrait> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
					CardCatalogEntry value3;
					Dictionary<string, CardPresentationDefinition> cardPresentations = option4.CardPresentations.Where<KeyValuePair<string, CardPresentationDefinition>>((KeyValuePair<string, CardPresentationDefinition> pair) => cardsByType.TryGetValue(pair.Key, out value3) && value3.FilterGroupId.Equals(specialGroupId, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, CardPresentationDefinition>, string, CardPresentationDefinition>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key, (KeyValuePair<string, CardPresentationDefinition> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
					AddCardOption(dictionary, specialGroupId, option4 with
					{
						NormalPortraits = normalPortraits,
						AncientPortraits = ancientPortraits,
						Assets = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase),
						CardPresentations = cardPresentations
					});
				}
			}
		}
		foreach (CardSkinOption pckCardOption in _pckCardOptions)
		{
			Dictionary<string, Dictionary<string, ResourceAsset>> dictionary2 = new Dictionary<string, Dictionary<string, ResourceAsset>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, Dictionary<string, string>> dictionary3 = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, Dictionary<string, AncientCardPortrait>> dictionary4 = new Dictionary<string, Dictionary<string, AncientCardPortrait>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, Dictionary<string, CardPresentationDefinition>> dictionary5 = new Dictionary<string, Dictionary<string, CardPresentationDefinition>>(StringComparer.OrdinalIgnoreCase);
			CardCatalogEntry[] array2 = array;
			foreach (CardCatalogEntry card2 in array2)
			{
				KeyValuePair<string, ResourceAsset>[] array3 = pckCardOption.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => CardArtMatches(pair.Key, card2, knownCardGroups)).ToArray();
				CardPresentationDefinition value4;
				bool flag = pckCardOption.CardPresentations.TryGetValue(card2.TypeName, out value4);
				string value5;
				bool flag2 = pckCardOption.NormalPortraits.TryGetValue(card2.TypeName, out value5);
				if (pckCardOption.StatefulArt != null)
				{
					value5 = pckCardOption.StatefulArt.DefaultPortrait(card2, pckCardOption.Assets);
					flag2 = value5 != null;
					array3 = ((!flag2) ? Array.Empty<KeyValuePair<string, ResourceAsset>>() : new KeyValuePair<string, ResourceAsset>[1]
					{
						new KeyValuePair<string, ResourceAsset>(value5, pckCardOption.Assets[value5])
					});
				}
				AncientCardPortrait value6;
				bool flag3 = pckCardOption.AncientPortraits.TryGetValue(card2.TypeName, out value6);
				if (array3.Length == 0 && !flag2 && !flag3 && TryGetUniqueSharedPoolPortrait(pckCardOption.Assets.Keys, card2, knownCardGroups, uniqueCardTypesByStem, out string portraitPath))
				{
					flag2 = true;
					value5 = portraitPath;
				}
				if (array3.Length == 0 && !flag2 && !flag3 && !flag)
				{
					continue;
				}
				string key = (card2.FilterGroupId.Equals(card2.CatalogGroupId, StringComparison.OrdinalIgnoreCase) ? card2.CatalogGroupId : card2.FilterGroupId);
				if (!dictionary2.TryGetValue(key, out var value7))
				{
					value7 = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
					dictionary2.Add(key, value7);
				}
				KeyValuePair<string, ResourceAsset>[] array4 = array3;
				for (int j = 0; j < array4.Length; j++)
				{
					KeyValuePair<string, ResourceAsset> keyValuePair = array4[j];
					value7[keyValuePair.Key] = keyValuePair.Value;
				}
				if (flag2 && value5 != null)
				{
					if (!dictionary3.TryGetValue(key, out var value8))
					{
						value8 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
						dictionary3.Add(key, value8);
					}
					value8[card2.TypeName] = value5;
				}
				if (flag3 && value6 != null)
				{
					if (!dictionary4.TryGetValue(key, out var value9))
					{
						value9 = new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase);
						dictionary4.Add(key, value9);
					}
					value9[card2.TypeName] = value6;
				}
				if (flag && value4 != null)
				{
					if (!dictionary5.TryGetValue(key, out var value10))
					{
						value10 = new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase);
						dictionary5.Add(key, value10);
					}
					value10[card2.TypeName] = value4;
				}
			}
			foreach (string item in dictionary2.Keys.Union<string>(dictionary3.Keys, StringComparer.OrdinalIgnoreCase).Union<string>(dictionary4.Keys, StringComparer.OrdinalIgnoreCase).Union<string>(dictionary5.Keys, StringComparer.OrdinalIgnoreCase))
			{
				AddCardOption(dictionary, item, pckCardOption with
				{
					NormalPortraits = (dictionary3.GetValueOrDefault(item) ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)),
					AncientPortraits = (dictionary4.GetValueOrDefault(item) ?? new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase)),
					Assets = (dictionary2.GetValueOrDefault(item) ?? new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase)),
					CardPresentations = (dictionary5.GetValueOrDefault(item) ?? new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase))
				});
			}
		}
		foreach (CardCatalogEntry card3 in array.Where((CardCatalogEntry card) => card.IsCharacterPool && card.FilterGroupId.Equals("ancients", StringComparison.OrdinalIgnoreCase)))
		{
			CardSkinOption[] array5 = (from options in dictionary.Values.Where((CardSkinGroup @group) => @group.Id.Equals(card3.PoolGroupId, StringComparison.OrdinalIgnoreCase) || @group.Id.Equals("ancients", StringComparison.OrdinalIgnoreCase)).SelectMany((CardSkinGroup @group) => @group.Options).GroupBy<CardSkinOption, string>((CardSkinOption option) => option.Id, StringComparer.OrdinalIgnoreCase)
				select options.Aggregate((CardSkinOption left, CardSkinOption right) => left.Merge(right))).ToArray();
			foreach (CardSkinOption cardSkinOption in array5)
			{
				Dictionary<string, string> dictionary6 = cardSkinOption.NormalPortraits.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.Equals(card3.TypeName, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
				Dictionary<string, AncientCardPortrait> dictionary7 = cardSkinOption.AncientPortraits.Where<KeyValuePair<string, AncientCardPortrait>>((KeyValuePair<string, AncientCardPortrait> pair) => pair.Key.Equals(card3.TypeName, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, AncientCardPortrait>, string, AncientCardPortrait>((KeyValuePair<string, AncientCardPortrait> pair) => pair.Key, (KeyValuePair<string, AncientCardPortrait> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
				Dictionary<string, CardPresentationDefinition> dictionary8 = cardSkinOption.CardPresentations.Where<KeyValuePair<string, CardPresentationDefinition>>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key.Equals(card3.TypeName, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, CardPresentationDefinition>, string, CardPresentationDefinition>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key, (KeyValuePair<string, CardPresentationDefinition> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
				Dictionary<string, ResourceAsset> dictionary9 = cardSkinOption.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => CardArtMatches(pair.Key, card3, knownCardGroups)).ToDictionary<KeyValuePair<string, ResourceAsset>, string, ResourceAsset>((KeyValuePair<string, ResourceAsset> pair) => pair.Key, (KeyValuePair<string, ResourceAsset> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
				if (dictionary6.Count + dictionary7.Count + dictionary8.Count + dictionary9.Count != 0)
				{
					CardSkinOption option2 = cardSkinOption with
					{
						NormalPortraits = dictionary6,
						AncientPortraits = dictionary7,
						CardPresentations = dictionary8,
						Assets = dictionary9,
						CardNames = cardSkinOption.CardNames.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.Equals(card3.TypeName, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase)
					};
					AddCardOption(dictionary, card3.PoolGroupId, option2);
					AddCardOption(dictionary, "ancients", option2);
				}
			}
		}
		IEnumerable<CardSkinGroup> enumerable = _artPackCardGroups?.Values;
		foreach (CardSkinGroup item2 in enumerable ?? Enumerable.Empty<CardSkinGroup>())
		{
			foreach (CardSkinOption option5 in item2.Options)
			{
				AddCardOption(dictionary, item2.Id, option5);
			}
		}
		foreach (string coveredType in GetArtPackCardTypes())
		{
			if (!cardsByType.TryGetValue(coveredType, out var value11))
			{
				continue;
			}
			foreach (string item3 in new string[2] { value11.CatalogGroupId, value11.FilterGroupId }.Where((string id) => !string.IsNullOrWhiteSpace(id)).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				if (!dictionary.TryGetValue(item3, out var value12))
				{
					continue;
				}
				CardSkinOption[] array5 = value12.Options.ToArray();
				foreach (CardSkinOption option3 in array5)
				{
					if (option3.NormalPortraits.ContainsKey(coveredType) || option3.CardPresentations.ContainsKey(coveredType))
					{
						AddCardOption(dictionary, item3, option3 with
						{
							NormalPortraits = option3.NormalPortraits.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.Equals(coveredType, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase),
							AncientPortraits = new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase),
							CardPresentations = option3.CardPresentations.Where<KeyValuePair<string, CardPresentationDefinition>>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key.Equals(coveredType, StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, CardPresentationDefinition>, string, CardPresentationDefinition>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key, (KeyValuePair<string, CardPresentationDefinition> pair) => pair.Value, StringComparer.OrdinalIgnoreCase),
							Assets = option3.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => pair.Key.Equals(option3.NormalPortraits.GetValueOrDefault(coveredType), StringComparison.OrdinalIgnoreCase)).ToDictionary<KeyValuePair<string, ResourceAsset>, string, ResourceAsset>((KeyValuePair<string, ResourceAsset> pair) => pair.Key, (KeyValuePair<string, ResourceAsset> pair) => pair.Value, StringComparer.OrdinalIgnoreCase)
						});
					}
				}
			}
		}
		_cardGroups.Clear();
		_cardGroups.AddRange((from @group in dictionary.Values
			where @group.Options.Count > 0
			orderby GroupSortOrder(@group.Id)
			select @group).ThenBy<CardSkinGroup, string>((CardSkinGroup group) => group.DisplayName, StringComparer.CurrentCultureIgnoreCase));
		foreach (CardSkinGroup cardGroup in _cardGroups)
		{
			cardGroup.Options.Sort((CardSkinOption left, CardSkinOption right) => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase));
		}
	}

	private static IReadOnlyDictionary<string, string> BuildUniqueCardTypesByStem(IReadOnlyList<CardCatalogEntry> cards, IReadOnlySet<string> knownCardGroups)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (CardCatalogEntry card in cards)
		{
			foreach (string item in new string[2]
			{
				NormalizeCardToken(card.TypeName),
				TryGetCardArtIdentity(card.PortraitPath, knownCardGroups)?.Stem
			}.Where((string stem) => !string.IsNullOrWhiteSpace(stem)).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				if (!hashSet.Contains(item))
				{
					if (dictionary.TryGetValue(item, out var value) && !value.Equals(card.TypeName, StringComparison.OrdinalIgnoreCase))
					{
						dictionary.Remove(item);
						hashSet.Add(item);
					}
					else
					{
						dictionary[item] = card.TypeName;
					}
				}
			}
		}
		return dictionary;
	}

	private static bool TryGetUniqueSharedPoolPortrait(IEnumerable<string> assetPaths, CardCatalogEntry card, IReadOnlySet<string> knownCardGroups, IReadOnlyDictionary<string, string> uniqueCardTypesByStem, out string portraitPath)
	{
		portraitPath = string.Empty;
		if (card.PoolGroupId.Equals(card.FilterGroupId, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		CardArtIdentity portrait = TryGetCardArtIdentity(card.PortraitPath, knownCardGroups);
		string typeStem = NormalizeCardToken(card.TypeName);
		if (portrait == null)
		{
			return false;
		}
		string value;
		string[] array = (from path in (from path in assetPaths
				select (Path: path, Identity: TryGetCardArtIdentity(path, knownCardGroups)) into candidate
				where candidate.Identity != null && !string.IsNullOrWhiteSpace(candidate.Identity.Category) && !candidate.Identity.Category.Equals(card.PoolGroupId, StringComparison.OrdinalIgnoreCase) && !candidate.Identity.Category.Equals(portrait.Category, StringComparison.OrdinalIgnoreCase) && (candidate.Identity.Stem.Equals(typeStem, StringComparison.OrdinalIgnoreCase) || candidate.Identity.Stem.Equals(portrait.Stem, StringComparison.OrdinalIgnoreCase)) && uniqueCardTypesByStem.TryGetValue(candidate.Identity.Stem, out value) && value.Equals(card.TypeName, StringComparison.OrdinalIgnoreCase)
				select candidate.Path).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			orderby SharedPoolPortraitScore(path)
			select path).ThenBy<string, string>((string path) => path, StringComparer.OrdinalIgnoreCase).ToArray();
		if (array.Length == 0)
		{
			return false;
		}
		portraitPath = array[0];
		return true;
	}

	private static int SharedPoolPortraitScore(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".tres", StringComparison.OrdinalIgnoreCase))
		{
			if (!extension.Equals(".res", StringComparison.OrdinalIgnoreCase))
			{
				if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
				{
					return 3;
				}
				return 2;
			}
			return 1;
		}
		return 0;
	}

	private static void AddCardOption(IDictionary<string, CardSkinGroup> groups, string groupId, CardSkinOption option)
	{
		if (!groups.TryGetValue(groupId, out CardSkinGroup value))
		{
			value = new CardSkinGroup(groupId, DisplayName(groupId));
			groups.Add(groupId, value);
		}
		int num = value.Options.FindIndex((CardSkinOption existing) => existing.Id.Equals(option.Id, StringComparison.OrdinalIgnoreCase));
		if (num >= 0)
		{
			value.Options[num] = value.Options[num].Merge(option);
		}
		else
		{
			value.Options.Add(option);
		}
	}

	public IReadOnlySet<string> GetAffectedSourcePaths(string groupId)
	{
		HashSet<string> hashSet = Groups.First((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Options.SelectMany((SkinOption option) => option.Assets.Keys).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex baselineIndex in _baselineIndexes)
		{
			foreach (string key in baselineIndex.Assets.Keys)
			{
				GroupIdentity? groupIdentity = TryGetPrimaryGroup(key);
				if ((object)groupIdentity != null && groupIdentity.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase) && key.EndsWith(".tres", StringComparison.OrdinalIgnoreCase))
				{
					hashSet.Add(key);
				}
			}
		}
		return hashSet;
	}

	public IReadOnlySet<string> GetRuntimeDependencyRestoreGroups(string loadedGroupId, IEnumerable<string> dependencyPaths)
	{
		HashSet<string> mountedPaths = dependencyPaths.Select(NormalizeTakeoverPath).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (mountedPaths.Count == 0)
		{
			return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		}
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (SkinGroup item in Groups.Where((SkinGroup group) => !group.Id.Equals(loadedGroupId, StringComparison.OrdinalIgnoreCase)))
		{
			if (item.Options.SelectMany((SkinOption option) => option.Assets).Any<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => mountedPaths.Contains(NormalizeTakeoverPath(pair.Key)) || pair.Value.Files.Any((ResourceFile file) => mountedPaths.Contains(NormalizeTakeoverPath(file.Path)))))
			{
				hashSet.Add(item.Id);
			}
		}
		return hashSet;
	}

	public Dictionary<string, ResourceFile> BuildBaselineDependencyOverlay(IEnumerable<string> dependencyPaths)
	{
		Dictionary<string, ResourceFile> dictionary = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in dependencyPaths.Select(NormalizeTakeoverPath).Select(StripResourceRedirectSuffix).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			ResourceAsset resourceAsset = ResolveBaseline(item);
			if (resourceAsset == null)
			{
				continue;
			}
			foreach (ResourceFile file in resourceAsset.Files)
			{
				string text = MapAssetFilePath(item, resourceAsset.SourcePath, file.Path);
				dictionary[text] = file;
				string text2 = NormalizeTakeoverPath(text);
				if (!text2.Equals(text, StringComparison.OrdinalIgnoreCase))
				{
					dictionary[text2] = file;
				}
			}
		}
		return dictionary;
	}

	public RuntimeResourceOverlay BuildRuntimeResourceOverlay(string groupId, string selectionId, IReadOnlyCollection<string> resourcePaths, string aliasToken, bool includeProviderDependencies = false, bool reuseMountedPrivateDependencies = false)
	{
		SkinOption skinOption = FrameworkRegistryCooperation.FilterAssets(Groups.First((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase)).Options.FirstOrDefault((SkinOption option) => option.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase)));
		HashSet<string> hashSet = resourcePaths.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		IncludeAtlasTexturePages(skinOption, hashSet);
		List<RuntimeResource> list = new List<RuntimeResource>();
		foreach (string sourcePath in hashSet)
		{
			ResourceAsset resourceAsset = ResolveBaseline(sourcePath);
			ResourceAsset asset2;
			object obj = ((skinOption != null && IsRelicAtlasSpritePath(sourcePath) && TryResolveProviderAsset(skinOption, sourcePath, out asset2)) ? asset2 : null);
			ResourceAsset resourceAsset2 = ((skinOption == null || IsRelicAtlasSpritePath(sourcePath)) ? null : (from index in GetSelectionProviderIndexes(skinOption)
				select index.Assets.GetValueOrDefault(sourcePath)).FirstOrDefault((ResourceAsset asset) => asset != null));
			if (obj == null)
			{
				obj = resourceAsset2 ?? ((skinOption != null && skinOption.Assets.TryGetValue(sourcePath, out ResourceAsset value)) ? value : ((skinOption?.ManagedMonsterScene != null && sourcePath.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase)) ? skinOption.ManagedMonsterScene : resourceAsset));
			}
			ResourceAsset resourceAsset3 = (ResourceAsset)obj;
			if (resourceAsset3 == null && (object)skinOption != null && skinOption.IsDirectCharacterRuntimeProvider)
			{
				resourceAsset3 = (from index in GetSelectionProviderIndexes(skinOption)
					select index.Assets.GetValueOrDefault(sourcePath)).FirstOrDefault((ResourceAsset asset) => asset != null);
			}
			if (resourceAsset3 != null)
			{
				list.Add(CreateRuntimeResource(sourcePath, resourceAsset3, resourceAsset));
			}
		}
		IncludeAliasedDependencyChain(skinOption, list, reuseMountedPrivateDependencies);
		RuntimeResourceOverlay runtimeResourceOverlay = BuildAliasedResourceOverlay(list, resourcePaths, aliasToken, redirectDirectScenesAtCanonicalPath: true);
		if (skinOption == null || !includeProviderDependencies || reuseMountedPrivateDependencies || ProviderUsesFullRuntime(skinOption.Id))
		{
			return runtimeResourceOverlay;
		}
		Dictionary<string, byte[]> dictionary = CollectSelectedProviderDependencies(skinOption, list, runtimeResourceOverlay.SourceAliases, runtimeResourceOverlay.PayloadAliases);
		if (dictionary.Count == 0)
		{
			return runtimeResourceOverlay;
		}
		Dictionary<string, byte[]> dictionary2 = runtimeResourceOverlay.Files.ToDictionary<KeyValuePair<string, byte[]>, string, byte[]>((KeyValuePair<string, byte[]> pair) => pair.Key, (KeyValuePair<string, byte[]> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, byte[]> item in dictionary)
		{
			dictionary2.TryAdd(item.Key, item.Value);
		}
		HashSet<string> canonicalDependencyPaths = runtimeResourceOverlay.CanonicalDependencyPaths.Concat(dictionary.Keys).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return new RuntimeResourceOverlay(runtimeResourceOverlay.ResourcePaths, dictionary2, runtimeResourceOverlay.SourceAliases, runtimeResourceOverlay.PayloadAliases, canonicalDependencyPaths);
	}

	internal RuntimeResourceOverlay BuildIsolatedRelicResourceOverlay(string groupId, string selectionId, IReadOnlyCollection<string> resourcePaths, string aliasToken)
	{
		if (resourcePaths.Any((string path) => !IsRelicAtlasSpritePath(path) && !IsRelicAtlasTexturePath(path)))
		{
			throw new ArgumentException("遗物私有资源包只能包含遗物图集或其切片。", "resourcePaths");
		}
		RuntimeResourceOverlay overlay = BuildRuntimeResourceOverlay(groupId, selectionId, resourcePaths, aliasToken);
		Dictionary<string, byte[]> files = overlay.Files.Where<KeyValuePair<string, byte[]>>((KeyValuePair<string, byte[]> pair) => !overlay.CanonicalDependencyPaths.Contains(pair.Key)).ToDictionary<KeyValuePair<string, byte[]>, string, byte[]>((KeyValuePair<string, byte[]> pair) => pair.Key, (KeyValuePair<string, byte[]> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
		return new RuntimeResourceOverlay(overlay.ResourcePaths, files, overlay.SourceAliases, overlay.PayloadAliases, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
	}

	private RuntimeResource CreateRuntimeResource(string sourcePath, ResourceAsset primary, ResourceAsset? baseline = null)
	{
		if (baseline == null)
		{
			baseline = ResolveBaseline(sourcePath);
		}
		ResourceFile resourceFile = FindDirectFile(primary, sourcePath);
		ResourceFile resourceFile2 = FindRemapFile(primary, sourcePath);
		ResourceFile[] importedPayloadFiles = GetImportedPayloadFiles(primary, sourcePath);
		if (resourceFile == null && resourceFile2 == null && baseline != null && primary != baseline)
		{
			resourceFile = FindDirectFile(baseline, sourcePath);
			resourceFile2 = FindRemapFile(baseline, sourcePath);
			if (importedPayloadFiles.Length == 0)
			{
				importedPayloadFiles = GetImportedPayloadFiles(baseline, sourcePath);
			}
		}
		return new RuntimeResource(sourcePath, resourceFile, resourceFile2, importedPayloadFiles);
	}

	private void IncludeAliasedDependencyChain(SkinOption? selected, List<RuntimeResource> resources, bool reuseMountedPrivateDependencies = false, IReadOnlySet<string>? availableSourcePaths = null)
	{
		IReadOnlyList<PckResourceIndex> readOnlyList;
		if (!(selected == null))
		{
			readOnlyList = GetSelectionProviderIndexes(selected);
		}
		else
		{
			IReadOnlyList<PckResourceIndex> readOnlyList2 = Array.Empty<PckResourceIndex>();
			readOnlyList = readOnlyList2;
		}
		IReadOnlyList<PckResourceIndex> selectedIndexes = readOnlyList;
		HashSet<string> selectedProviderIds = ((selected == null) ? new HashSet<string>(StringComparer.OrdinalIgnoreCase) : GetOptionProviderIds(selected).ToHashSet<string>(StringComparer.OrdinalIgnoreCase));
		HashSet<string> discoveredResourcePaths = resources.Select((RuntimeResource resource) => resource.SourcePath).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (availableSourcePaths != null)
		{
			discoveredResourcePaths.UnionWith(availableSourcePaths);
		}
		Queue<RuntimeResource> queue = new Queue<RuntimeResource>(resources);
		HashSet<string> selectableProviderFiles = (reuseMountedPrivateDependencies ? (from file in (from option in Groups.SelectMany((SkinGroup @group) => @group.Options)
				where option.IsRuntimeProvider
				select option).SelectMany((SkinOption option) => option.Assets.Values).SelectMany((ResourceAsset asset) => asset.Files).Concat(_pckCardOptions.SelectMany((CardSkinOption option) => option.Assets.Values).SelectMany((ResourceAsset asset) => asset.Files))
				.Concat(_configuredCardGroups.SelectMany((CardSkinGroup @group) => @group.Options).SelectMany((CardSkinOption option) => option.Assets.Values).SelectMany((ResourceAsset asset) => asset.Files))
			select NormalizeTakeoverPath(file.Path)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase) : new HashSet<string>());
		IReadOnlySet<string> readOnlySet;
		if (!(selected == null))
		{
			readOnlySet = GetIsolatedRelicProviderPaths(selected);
		}
		else
		{
			IReadOnlySet<string> readOnlySet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			readOnlySet = readOnlySet2;
		}
		IReadOnlySet<string> isolatedRelicProviderPaths = readOnlySet;
		RuntimeResource result;
		while (queue.TryDequeue(out result))
		{
			foreach (ResourceFile dependencyFile in new ResourceFile[2] { result.DirectFile, result.RemapFile }.Where((ResourceFile file) => file != null).Cast<ResourceFile>().Concat(result.PayloadFiles))
			{
				bool allowImportedPayloadAlias = IsRewritableTextResource(dependencyFile.Path);
				foreach (string item in EnumerateDependencyPaths(dependencyFile))
				{
					if (discoveredResourcePaths.Contains(item) || !CanAliasDependency(item, allowImportedPayloadAlias))
					{
						continue;
					}
					PckResourceIndex preferredIndex2 = selectedIndexes.FirstOrDefault((PckResourceIndex candidate) => candidate.Archive == dependencyFile.Archive);
					if (TryResolveSelected(item, preferredIndex2, out var asset2, out var index2))
					{
						IncludeResource(item, asset2, index2);
						continue;
					}
					ResourceAsset resourceAsset = ResolveBaseline(item);
					if (resourceAsset != null)
					{
						IncludeResource(item, resourceAsset, FindOwningIndex(resourceAsset));
					}
				}
			}
		}
		bool CanReuseMountedPrivateDependency(string sourcePath, ResourceAsset asset, PckResourceIndex? index, bool requiresAliasedLocation)
		{
			bool belongsToSelectedProvider = selected != null && index != null && selectedProviderIds.Contains(index.Mod.Id) && asset.Files.Any((ResourceFile file) => file.Archive == index.Archive);
			ResourceFile[] array = asset.Files.Where((ResourceFile file) => index != null && file.Archive == index.Archive).ToArray();
			bool isMountedBySelectedOverlay = selected != null && array.Length != 0 && array.All((ResourceFile file) => !isolatedRelicProviderPaths.Contains(NormalizeTakeoverPath(file.Path)) && ShouldMountProviderDependency(selected, file.Path, selectableProviderFiles));
			string normalizedSourcePath = NormalizeTakeoverPath(sourcePath);
			bool isProviderExclusivePath = ResolveBaseline(sourcePath) == null && !_cosmeticIndexes.Any((PckResourceIndex candidate) => (index == null || !candidate.Mod.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase)) && (candidate.Assets.ContainsKey(sourcePath) || candidate.Assets.ContainsKey(normalizedSourcePath) || candidate.Archive.Contains(sourcePath) || candidate.Archive.Contains(normalizedSourcePath)));
			return RuntimeDependencyIsolationPolicy.CanReuseMountedProviderDependency(belongsToSelectedProvider, isProviderExclusivePath, isMountedBySelectedOverlay, requiresAliasedLocation);
		}
		PckResourceIndex? FindOwningIndex(ResourceAsset asset)
		{
			return _cosmeticIndexes.Concat(_baselineIndexes).FirstOrDefault((PckResourceIndex candidate) => asset.Files.Any((ResourceFile file) => candidate.Archive == file.Archive));
		}
		void IncludeResource(string sourcePath, ResourceAsset asset, PckResourceIndex? index, bool requiresAliasedLocation = false)
		{
			if (!discoveredResourcePaths.Contains(sourcePath))
			{
				RuntimeResource runtimeResource = CreateRuntimeResource(sourcePath, asset);
				if (!(runtimeResource.DirectFile == null) || !(runtimeResource.RemapFile == null))
				{
					discoveredResourcePaths.Add(sourcePath);
					queue.Enqueue(runtimeResource);
					if (!reuseMountedPrivateDependencies || !CanReuseMountedPrivateDependency(sourcePath, asset, index, requiresAliasedLocation))
					{
						resources.Add(runtimeResource);
						if (index != null)
						{
							foreach (ResourceAsset siblingAtlasTextureAsset in GetSiblingAtlasTextureAssets(index, sourcePath))
							{
								IncludeResource(siblingAtlasTextureAsset.SourcePath, siblingAtlasTextureAsset, index, requiresAliasedLocation: true);
							}
						}
					}
				}
			}
		}
		bool TryResolveSelected(string sourcePath, PckResourceIndex? preferredIndex, out ResourceAsset asset, out PckResourceIndex? index)
		{
			if (selected != null && selected.Assets.TryGetValue(sourcePath, out ResourceAsset configured))
			{
				asset = configured;
				index = selectedIndexes.FirstOrDefault((PckResourceIndex candidate) => configured.Files.Any((ResourceFile file) => candidate.Archive == file.Archive));
				return true;
			}
			IEnumerable<PckResourceIndex> enumerable;
			if (preferredIndex != null)
			{
				enumerable = selectedIndexes.Where((PckResourceIndex candidate) => candidate.Mod.Id.Equals(preferredIndex.Mod.Id, StringComparison.OrdinalIgnoreCase));
			}
			else
			{
				IEnumerable<PckResourceIndex> enumerable2 = selectedIndexes;
				enumerable = enumerable2;
			}
			foreach (PckResourceIndex item2 in enumerable)
			{
				ResourceAsset resourceAsset2 = item2.Assets.GetValueOrDefault(sourcePath) ?? item2.TryBuildAsset(sourcePath);
				if (resourceAsset2 != null)
				{
					asset = resourceAsset2;
					index = item2;
					return true;
				}
			}
			asset = null;
			index = null;
			return false;
		}
	}

	private static IEnumerable<string> EnumerateDependencyPaths(ResourceAsset asset)
	{
		return asset.Files.Where((ResourceFile file) => IsDependencyGraphTextResource(file.Path)).SelectMany(EnumerateDependencyPaths).Distinct<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static IEnumerable<string> EnumerateDependencyPaths(ResourceFile? file)
	{
		if (file == null || !IsDependencyGraphTextResource(file.Path))
		{
			return Array.Empty<string>();
		}
		string @string = Encoding.UTF8.GetString(file.Read());
		return (from match in EmbeddedResourcePathRegex().Matches(@string)
			select match.Value).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	private static bool IsDependencyGraphTextResource(string path)
	{
		if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".scn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".res", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".import", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".gdshader", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool CanAliasDependency(string path, bool allowImportedPayloadAlias)
	{
		if ((allowImportedPayloadAlias || !path.StartsWith("res://.godot/imported/", StringComparison.OrdinalIgnoreCase)) && !path.StartsWith("res://.godot/exported/", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".gd", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".gdc", StringComparison.OrdinalIgnoreCase))
		{
			return !path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	private Dictionary<string, byte[]> CollectSelectedProviderDependencies(SkinOption selected, IReadOnlyCollection<RuntimeResource> resources, IReadOnlyDictionary<string, string> sourceAliases, IReadOnlyDictionary<string, string> payloadAliases)
	{
		IReadOnlyList<PckResourceIndex> selectionProviderIndexes = GetSelectionProviderIndexes(selected);
		if (selectionProviderIndexes.Count == 0)
		{
			return new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
		}
		Dictionary<string, byte[]> result = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
		Queue<(PckResourceIndex? Index, ResourceFile File)> queue = new Queue<(PckResourceIndex, ResourceFile)>();
		HashSet<string> queued = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (ResourceFile file2 in resources.SelectMany((RuntimeResource resource) => new ResourceFile[2] { resource.DirectFile, resource.RemapFile }.Where((ResourceFile file) => file != null).Cast<ResourceFile>().Concat(resource.PayloadFiles)))
		{
			Enqueue(selectionProviderIndexes.FirstOrDefault((PckResourceIndex candidate) => candidate.Archive == file2.Archive), file2);
		}
		while (true)
		{
			if (!queue.TryDequeue(out var pending))
			{
				break;
			}
			if (!MayContainResourceReferences(pending.File.Path))
			{
				continue;
			}
			byte[] bytes = pending.File.Read();
			string @string = Encoding.UTF8.GetString(bytes);
			foreach (Match item2 in EmbeddedResourcePathRegex().Matches(@string))
			{
				string value = item2.Value;
				if (sourceAliases.ContainsKey(value) || payloadAliases.ContainsKey(value))
				{
					continue;
				}
				PckResourceIndex pckResourceIndex = null;
				ResourceAsset resourceAsset = null;
				IEnumerable<PckResourceIndex> enumerable;
				if (pending.Index != null)
				{
					enumerable = selectionProviderIndexes.Where((PckResourceIndex index) => index.Mod.Id.Equals(pending.Index.Mod.Id, StringComparison.OrdinalIgnoreCase));
				}
				else
				{
					IEnumerable<PckResourceIndex> enumerable2 = selectionProviderIndexes;
					enumerable = enumerable2;
				}
				foreach (PckResourceIndex item3 in enumerable)
				{
					resourceAsset = item3.Assets.GetValueOrDefault(value) ?? item3.TryBuildAsset(value);
					if (resourceAsset != null)
					{
						pckResourceIndex = item3;
						break;
					}
				}
				if (resourceAsset == null || pckResourceIndex == null)
				{
					continue;
				}
				IncludeAsset(pckResourceIndex, resourceAsset);
				foreach (ResourceAsset siblingAtlasTextureAsset in GetSiblingAtlasTextureAssets(pckResourceIndex, value))
				{
					IncludeAsset(pckResourceIndex, siblingAtlasTextureAsset);
				}
			}
		}
		return result;
		void Enqueue(PckResourceIndex? index, ResourceFile file)
		{
			string item = (index?.Mod.Id ?? file.ArchivePath) + "\n" + file.Path;
			if (queued.Add(item))
			{
				queue.Enqueue((index, file));
			}
		}
		void IncludeAsset(PckResourceIndex index, ResourceAsset asset)
		{
			foreach (ResourceFile file3 in asset.Files)
			{
				byte[] array = file3.Read();
				if (IsRewritableTextResource(file3.Path))
				{
					array = RewriteTextResource(array, sourceAliases, payloadAliases);
				}
				result[file3.Path] = array;
				string text = NormalizeTakeoverPath(file3.Path);
				if (!text.Equals(file3.Path, StringComparison.OrdinalIgnoreCase))
				{
					result[text] = array;
				}
				Enqueue(index, file3);
			}
		}
	}

	private static bool IsRewritableTextResource(string path)
	{
		if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".import", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".gd", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool MayContainResourceReferences(string path)
	{
		if (!path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".scn", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".res", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".import", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".gd", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".gdc", StringComparison.OrdinalIgnoreCase))
		{
			return path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public RuntimeResourceOverlay BuildIsolatedCardResource(string groupId, string selectionId, string resourcePath, bool useSelectedProvider, string aliasToken)
	{
		RuntimeResourceOverlay runtimeResourceOverlay = BuildIsolatedCardResources(groupId, selectionId, new _003C_003Ez__ReadOnlySingleElementList<string>(resourcePath), useSelectedProvider, aliasToken);
		if (!runtimeResourceOverlay.ResourcePaths.ContainsKey(resourcePath))
		{
			throw new InvalidOperationException("找不到独立卡牌资源：" + resourcePath);
		}
		return runtimeResourceOverlay;
	}

	public bool TryReadCardImageBytes(string groupId, string selectionId, string resourcePath, bool useSelectedProvider, out byte[] bytes)
	{
		bytes = Array.Empty<byte>();
		ResourceAsset resourceAsset;
		if (useSelectedProvider)
		{
			CardSkinOption cardSkinOption = CardGroups.FirstOrDefault((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((CardSkinOption candidate) => candidate.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase));
			if ((object)cardSkinOption == null)
			{
				cardSkinOption = _pckCardOptions.FirstOrDefault((CardSkinOption candidate) => candidate.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase));
			}
			resourceAsset = ((cardSkinOption == null) ? null : ResolveCardProviderAsset(cardSkinOption, resourcePath));
		}
		else
		{
			resourceAsset = ResolveBaseline(resourcePath);
		}
		ResourceFile resourceFile = ((resourceAsset == null) ? null : FindDirectFile(resourceAsset, resourcePath));
		if (resourceFile == null || !IsRasterImagePath(resourceFile.Path))
		{
			return false;
		}
		bytes = resourceFile.Read();
		return bytes.Length != 0;
	}

	private static bool IsRasterImagePath(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".webp", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public RuntimeResourceOverlay BuildIsolatedCardResources(string groupId, string selectionId, IEnumerable<string> resourcePaths, bool useSelectedProvider, string aliasToken, IReadOnlyDictionary<string, string>? existingResourcePaths = null)
	{
		CardSkinOption cardSkinOption = null;
		if (useSelectedProvider)
		{
			cardSkinOption = CardGroups.FirstOrDefault((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))?.Options.FirstOrDefault((CardSkinOption candidate) => candidate.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase));
			if ((object)cardSkinOption == null)
			{
				cardSkinOption = _pckCardOptions.FirstOrDefault((CardSkinOption candidate) => candidate.Id.Equals(selectionId, StringComparison.OrdinalIgnoreCase));
			}
		}
		List<RuntimeResource> list = new List<RuntimeResource>();
		foreach (string item in resourcePaths.Where((string path) => !string.IsNullOrWhiteSpace(path)).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			ResourceAsset resourceAsset = ((!useSelectedProvider) ? ResolveBaseline(item) : ((cardSkinOption == null) ? null : ResolveCardProviderAsset(cardSkinOption, item)));
			if (resourceAsset != null)
			{
				list.Add(new RuntimeResource(item, FindDirectFile(resourceAsset, item), FindRemapFile(resourceAsset, item), GetImportedPayloadFiles(resourceAsset, item)));
			}
		}
		if (list.Count == 0)
		{
			throw new InvalidOperationException("找不到任何可隔离的卡牌资源。");
		}
		IncludeAliasedDependencyChain((useSelectedProvider && cardSkinOption != null) ? new SkinOption(cardSkinOption.Id, cardSkinOption.Name, cardSkinOption.Assets, IsRuntimeProvider: false, IsDirectCharacterRuntimeProvider: false, null, null, null, cardSkinOption.ProviderId) : null, list, reuseMountedPrivateDependencies: false, existingResourcePaths?.Keys.ToHashSet<string>(StringComparer.OrdinalIgnoreCase));
		return BuildAliasedResourceOverlay(list, (IReadOnlyCollection<string>)(object)list.Select((RuntimeResource resource) => resource.SourcePath).ToArray(), aliasToken, redirectDirectScenesAtCanonicalPath: false, existingResourcePaths);
	}

	private ResourceAsset? ResolveCardProviderAsset(CardSkinOption option, string resourcePath)
	{
		if (option.Assets.TryGetValue(resourcePath, out ResourceAsset value))
		{
			return value;
		}
		foreach (PckResourceIndex item in _cosmeticIndexes.Where((PckResourceIndex index) => string.Equals(index.Mod.RootPath, option.ProviderRootPath, StringComparison.OrdinalIgnoreCase)))
		{
			if (item.Assets.TryGetValue(resourcePath, out ResourceAsset value2))
			{
				return value2;
			}
			ResourceAsset resourceAsset = item.TryBuildAsset(resourcePath);
			if (resourceAsset != null)
			{
				return resourceAsset;
			}
		}
		return null;
	}

	private static RuntimeResourceOverlay BuildAliasedResourceOverlay(IReadOnlyCollection<RuntimeResource> resources, IReadOnlyCollection<string> resourcePaths, string aliasToken, bool redirectDirectScenesAtCanonicalPath = false, IReadOnlyDictionary<string, string>? existingSourceAliases = null)
	{
		Dictionary<string, string> dictionary = ((existingSourceAliases == null) ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) : new Dictionary<string, string>(existingSourceAliases, StringComparer.OrdinalIgnoreCase));
		foreach (RuntimeResource resource in resources)
		{
			dictionary[resource.SourcePath] = BuildRuntimeSourceAlias(resource, aliasToken);
		}
		Dictionary<string, string> dictionary2 = resources.SelectMany((RuntimeResource resource) => resource.PayloadFiles).DistinctBy<ResourceFile, string>((ResourceFile file) => file.Path, StringComparer.OrdinalIgnoreCase).ToDictionary<ResourceFile, string, string>((ResourceFile file) => file.Path, delegate(ResourceFile file)
		{
			string text = aliasToken;
			string path = file.Path;
			return "res://sts2_skin_runtime/" + text + "/_payload/" + path.Substring(6, path.Length - 6);
		}, StringComparer.OrdinalIgnoreCase);
		bool canReuseExternalDependencies = AliasedDependencyCachePolicy.CanReuseExternalDependencies(from file in resources.SelectMany((RuntimeResource resource) => new ResourceFile[2] { resource.DirectFile, resource.RemapFile }.Where((ResourceFile file) => file != null).Cast<ResourceFile>().Concat(resource.PayloadFiles)).DistinctBy<ResourceFile, string>((ResourceFile file) => file.ArchivePath + "\n" + file.Path, StringComparer.OrdinalIgnoreCase)
			select new AliasedDependencyReference(IsRewritableTextResource(file.Path), (IReadOnlyCollection<string>)(object)EnumerateDependencyPaths(file).ToArray()), dictionary.Keys.Concat(dictionary2.Keys));
		Dictionary<string, byte[]> dictionary3 = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (RuntimeResource resource2 in resources)
		{
			if (resource2.DirectFile != null)
			{
				byte[] bytes = resource2.DirectFile.Read();
				dictionary3[dictionary[resource2.SourcePath]] = RewriteAliasedResourceBytes(resource2.DirectFile.Path, bytes, dictionary, dictionary2);
				if (redirectDirectScenesAtCanonicalPath && resource2.SourcePath.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) && !FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(resource2.SourcePath))
				{
					string text2 = resource2.SourcePath + ".remap";
					dictionary3[text2] = Encoding.UTF8.GetBytes("[remap]\n\npath=\"" + dictionary[resource2.SourcePath] + "\"\n");
					hashSet.Add(text2);
				}
			}
			foreach (ResourceFile payloadFile in resource2.PayloadFiles)
			{
				byte[] bytes2 = payloadFile.Read();
				dictionary3[dictionary2[payloadFile.Path]] = RewriteAliasedResourceBytes(payloadFile.Path, bytes2, dictionary, dictionary2, rewriteOnlySpineAtlas: true, stripUids: false);
			}
			if (resource2.RemapFile == null)
			{
				continue;
			}
			string @string = Encoding.UTF8.GetString(resource2.RemapFile.Read());
			Dictionary<string, string> dictionary4 = new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase);
			foreach (Match item in ResourcePathRegex().Matches(@string))
			{
				string value = item.Groups[1].Value;
				if (!dictionary4.ContainsKey(value))
				{
					ResourceFile resourceFile = MatchImportedFile(value, resource2.PayloadFiles);
					if (resourceFile != null)
					{
						dictionary4[value] = dictionary2[resourceFile.Path];
					}
				}
			}
			string text3 = (resource2.RemapFile.Path.EndsWith(".import", StringComparison.OrdinalIgnoreCase) ? ".import" : ".remap");
			byte[] value2 = RewriteTextResource(Encoding.UTF8.GetBytes(@string), dictionary4, null);
			dictionary3[dictionary[resource2.SourcePath] + text3] = value2;
			string text4 = resource2.SourcePath + text3;
			if (!FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(resource2.SourcePath))
			{
				dictionary3[text4] = value2;
				hashSet.Add(text4);
			}
		}
		Dictionary<string, string> dictionary5 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item2 in resources.Select((RuntimeResource resource) => resource.SourcePath).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (!dictionary.TryGetValue(item2, out var value3) || (!dictionary3.ContainsKey(value3) && !dictionary3.ContainsKey(value3 + ".import") && !dictionary3.ContainsKey(value3 + ".remap")))
			{
				throw new InvalidOperationException("无法为 " + item2 + " 创建独立皮肤资源。");
			}
			dictionary5[item2] = value3;
		}
		return new RuntimeResourceOverlay(dictionary5, dictionary3, dictionary, dictionary2, hashSet, canReuseExternalDependencies);
	}

	internal static byte[] RewriteAliasedResourceBytes(string path, byte[] bytes, IReadOnlyDictionary<string, string> sourceAliases, IReadOnlyDictionary<string, string> payloadAliases, bool rewriteOnlySpineAtlas = false, bool stripUids = true)
	{
		if (!(rewriteOnlySpineAtlas ? path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase) : IsRewritableTextResource(path)))
		{
			return bytes;
		}
		return RewriteTextResource(bytes, sourceAliases, payloadAliases, stripUids);
	}

	private static string BuildRuntimeSourceAlias(RuntimeResource resource, string aliasToken)
	{
		string text = ((resource.RemapFile != null) ? StripResourceRedirectSuffix(NormalizeTakeoverPath(resource.RemapFile.Path)) : ((resource.DirectFile != null) ? NormalizeTakeoverPath(resource.DirectFile.Path) : resource.SourcePath));
		if (!text.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			text = resource.SourcePath;
		}
		string text2 = text;
		return "res://sts2_skin_runtime/" + aliasToken + "/" + text2.Substring(6, text2.Length - 6);
	}

	private static string StripResourceRedirectSuffix(string path)
	{
		string text;
		if (path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			text = path;
			return text.Substring(0, text.Length - 7);
		}
		if (!path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		text = path;
		return text.Substring(0, text.Length - 6);
	}

	private void IncludeAtlasTexturePages(SkinOption? selected, HashSet<string> sourcePaths)
	{
		string[] array = (from path in sourcePaths
			where path.EndsWith(".atlas", StringComparison.OrdinalIgnoreCase)
			select path.Substring(0, path.LastIndexOf('/') + 1)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		foreach (string item in _baselineIndexes.SelectMany((PckResourceIndex index) => index.Assets.Keys).Concat(selected?.Assets.Keys ?? Array.Empty<string>()).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			if (!item.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !item.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (item.StartsWith(text, StringComparison.OrdinalIgnoreCase))
				{
					string text2 = item;
					int length = text.Length;
					if (!text2.Substring(length, text2.Length - length).Contains('/'))
					{
						sourcePaths.Add(item);
						break;
					}
				}
			}
		}
	}

	public void Dispose()
	{
		foreach (PckResourceIndex item in _baselineIndexes.Skip(1).Concat(_cosmeticIndexes))
		{
			item.Dispose();
		}
		_gameArchive.Dispose();
	}

	private static IReadOnlyList<SkinGroup> BuildGroups(IEnumerable<PckResourceIndex> cosmeticIndexes, IEnumerable<PckResourceIndex>? baselineIndexes = null)
	{
		PckResourceIndex[] array = cosmeticIndexes.ToArray();
		PckResourceIndex[] array2 = (baselineIndexes ?? Array.Empty<PckResourceIndex>()).ToArray();
		IReadOnlyDictionary<string, GroupIdentity> managedMonsterAssetGroups = BuildManagedMonsterAssetGroups(array2);
		HashSet<string> knownGroupIds = (from GroupIdentity @group in from @group in array2.SelectMany((PckResourceIndex index) => index.Assets.Keys).Select(TryGetDefinedBaselineGroup)
				where @group != null
				select @group
			select @group.Id).Concat(managedMonsterAssetGroups.Values.Select((GroupIdentity group) => group.Id)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> knownCharacterGroupIds = (from GroupIdentity @group in from @group in array2.SelectMany((PckResourceIndex index) => index.Assets.Keys).Select(TryGetUnambiguousCharacterGroup)
				where @group != null
				select @group
			select @group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, SkinGroup> dictionary = new Dictionary<string, SkinGroup>(StringComparer.OrdinalIgnoreCase);
		PckResourceIndex[] array3 = array;
		foreach (PckResourceIndex pckResourceIndex in array3)
		{
			GroupIdentity[] source = (from path in pckResourceIndex.Assets.Keys
				select (Path: path, Group: TryGetPrimaryGroup(path) ?? managedMonsterAssetGroups.GetValueOrDefault(NormalizeTakeoverPath(path))) into candidate
				where candidate.Group != null && knownGroupIds.Contains(candidate.Group.Id)
				where IsGroupAnchorResource(candidate.Path, candidate.Group)
				select candidate.Group).DistinctBy((GroupIdentity group) => group.Id).ToArray();
			HashSet<string> hashSet = (from GroupIdentity @group in from path in pckResourceIndex.Assets.Keys
					select TryGetCharacterVisualAnchorGroup(path, knownCharacterGroupIds) into @group
					where @group != null
					select @group
				select @group.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			IReadOnlySet<string> eligibleCharacterGroupIds = CharacterGroupEvidencePolicy.ResolveEligibleGroups(from @group in source
				where knownCharacterGroupIds.Contains(@group.Id)
				select @group.Id, hashSet);
			GroupIdentity[] array4 = source.Where((GroupIdentity group) => !knownCharacterGroupIds.Contains(group.Id) || eligibleCharacterGroupIds.Contains(group.Id)).ToArray();
			if (array4.Length == 0)
			{
				AddBaselinePartOptions(pckResourceIndex, (IReadOnlyCollection<PckResourceIndex>)(object)array2, dictionary, knownGroupIds);
				AddLooseBaselineOptions(pckResourceIndex, (IReadOnlyCollection<PckResourceIndex>)(object)array2, dictionary);
				continue;
			}
			Dictionary<string, Dictionary<string, ResourceAsset>> dictionary2 = new Dictionary<string, Dictionary<string, ResourceAsset>>(StringComparer.OrdinalIgnoreCase);
			GroupIdentity[] array5 = array4;
			foreach (GroupIdentity groupIdentity in array5)
			{
				dictionary2[groupIdentity.Id] = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
			}
			foreach (ResourceAsset asset in pckResourceIndex.Assets.Values)
			{
				if (IsCardArtSourcePath(asset.SourcePath))
				{
					continue;
				}
				GroupIdentity groupIdentity2 = TryGetPrimaryGroup(asset.SourcePath) ?? managedMonsterAssetGroups.GetValueOrDefault(NormalizeTakeoverPath(asset.SourcePath));
				if (groupIdentity2 != null && knownGroupIds.Contains(groupIdentity2.Id) && dictionary2.TryGetValue(groupIdentity2.Id, out var value))
				{
					value[asset.SourcePath] = asset;
					continue;
				}
				GroupIdentity[] array6 = array4.Where((GroupIdentity group) => ContainsGroupToken(asset.SourcePath, group.Id)).ToArray();
				if (array6.Length == 1)
				{
					dictionary2[array6[0].Id][asset.SourcePath] = asset;
				}
			}
			array5 = array4;
			foreach (GroupIdentity groupIdentity3 in array5)
			{
				Dictionary<string, ResourceAsset> dictionary3 = dictionary2[groupIdentity3.Id];
				if (dictionary3.Count != 0)
				{
					if (!dictionary.TryGetValue(groupIdentity3.Id, out var value2))
					{
						value2 = new SkinGroup(groupIdentity3.Id, groupIdentity3.DisplayName);
						dictionary.Add(groupIdentity3.Id, value2);
					}
					value2.Options.Add(new SkinOption(pckResourceIndex.Mod.Id, pckResourceIndex.Mod.Name, dictionary3, IsRuntimeProvider: false, IsDirectCharacterRuntimeProvider: false, null, null, null, null, IsManagedMonsterRuntimeProfile: false, null, CharacterGroupEvidencePolicy.IsIconOnlySupplement(knownCharacterGroupIds.Contains(groupIdentity3.Id) && !hashSet.Contains(groupIdentity3.Id) && dictionary3.Keys.Any(IsCharacterIconSourcePath), ReplacesCharacterVisualsAtRuntime(pckResourceIndex.Mod))));
				}
			}
		}
		AddPckRuntimeProviderOptions((IReadOnlyCollection<PckResourceIndex>)(object)array, (IReadOnlyCollection<PckResourceIndex>)(object)array2, dictionary, knownGroupIds, knownCharacterGroupIds);
		AddDirectCharacterRuntimeProviderOptions((IReadOnlyCollection<PckResourceIndex>)(object)array, dictionary, knownCharacterGroupIds);
		AddManagedMonsterSceneOptions((IReadOnlyCollection<PckResourceIndex>)(object)array, dictionary, knownGroupIds);
		AddRuntimeMonsterVisualModeOptions((IReadOnlyCollection<PckResourceIndex>)(object)array, dictionary);
		ManualCharacterVariantScanner.Expand((IReadOnlyCollection<PckResourceIndex>)(object)array, dictionary, knownCharacterGroupIds);
		ProviderVariantOptions.Expand(dictionary);
		ProviderSettingsVariantOptions.Expand((IReadOnlyCollection<PckResourceIndex>)(object)array, dictionary);
		AddEventSkinGroups(array, array2, dictionary);
		MergeOrphanMonsterOptions(dictionary, knownGroupIds);
		foreach (SkinGroup value3 in dictionary.Values)
		{
			value3.Options.Sort((SkinOption left, SkinOption right) => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase));
		}
		return dictionary.Values.OrderBy((SkinGroup group) => GroupSortOrder(group.Id)).ThenBy<SkinGroup, string>((SkinGroup group) => group.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
	}

	private static void MergeOrphanMonsterOptions(IDictionary<string, SkinGroup> groups, IReadOnlySet<string> knownGroupIds)
	{
		SkinGroup[] array = groups.Values.Where((SkinGroup group) => group.Options.Count > 0).ToArray();
		SkinGroup[] array2 = array;
		foreach (SkinGroup orphan in array2)
		{
			SkinGroup[] source = array.Where((SkinGroup group) => !group.Id.Equals(orphan.Id, StringComparison.OrdinalIgnoreCase)).ToArray();
			string stem = orphan.Id.TrimEnd('s');
			if (stem.Length < 5)
			{
				continue;
			}
			SkinGroup[] array3 = source.Where((SkinGroup target) => target.Id.Contains(stem, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array3.Length == 0)
			{
				continue;
			}
			SkinGroup[] array4 = array3;
			foreach (SkinGroup skinGroup in array4)
			{
				foreach (SkinOption option in orphan.Options)
				{
					if (!skinGroup.Options.Any((SkinOption existing) => existing.EffectiveProviderId.Equals(option.EffectiveProviderId, StringComparison.OrdinalIgnoreCase) && existing.Name.Equals(option.Name, StringComparison.Ordinal)))
					{
						skinGroup.Options.Add(option);
					}
				}
			}
			ModLog.Info("共享资源分组 " + orphan.Id + " 的皮肤已并入：" + string.Join(", ", array3.Select((SkinGroup target) => target.Id)) + "。");
		}
	}

	private static void AddBaselinePartOptions(PckResourceIndex index, IReadOnlyCollection<PckResourceIndex> baselines, IDictionary<string, SkinGroup> groups, IReadOnlySet<string> knownGroupIds)
	{
		foreach (IGrouping<string, string> item in BaselinePartTakeoverPolicy.ResolveServablePartPaths(index.Assets.Keys, delegate(string path)
		{
			GroupIdentity groupIdentity = TryGetPrimaryGroup(path);
			return (object)groupIdentity != null && knownGroupIds.Contains(groupIdentity.Id);
		}, delegate(string path)
		{
			GroupIdentity groupIdentity2 = TryGetPrimaryGroup(path);
			return (object)groupIdentity2 == null || IsGroupAnchorResource(path, groupIdentity2);
		}, (string path) => baselines.Any((PckResourceIndex baseline) => ResourceExists(baseline, path))).GroupBy<string, string>((string path) => TryGetPrimaryGroup(path).Id, StringComparer.OrdinalIgnoreCase))
		{
			GroupIdentity groupIdentity3 = TryGetPrimaryGroup(item.First());
			if (!groups.TryGetValue(groupIdentity3.Id, out SkinGroup value))
			{
				value = new SkinGroup(groupIdentity3.Id, groupIdentity3.DisplayName);
				groups.Add(groupIdentity3.Id, value);
			}
			value.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, item.ToDictionary<string, string, ResourceAsset>((string path) => path, (string path) => index.Assets[path], StringComparer.OrdinalIgnoreCase), IsRuntimeProvider: false, IsDirectCharacterRuntimeProvider: false, null, null, null, null, IsManagedMonsterRuntimeProfile: false, null, IsCharacterIconOnly: false, IsBaselinePartOverride: true)
			{
				ProviderRootPath = index.Mod.RootPath
			});
		}
	}

	private static void AddLooseBaselineOptions(PckResourceIndex index, IReadOnlyCollection<PckResourceIndex> baselines, IDictionary<string, SkinGroup> groups)
	{
		foreach (IGrouping<string, (ResourceAsset, GroupIdentity)> item2 in (from asset in index.Assets.Values
			where baselines.Any((PckResourceIndex baseline) => ResourceExists(baseline, asset.SourcePath))
			select (Asset: asset, Group: TryGetPrimaryGroup(asset.SourcePath)) into pair
			where pair.Group != null
			select pair).GroupBy<(ResourceAsset, GroupIdentity), string>(((ResourceAsset Asset, GroupIdentity Group) pair) => pair.Group.Id, StringComparer.OrdinalIgnoreCase))
		{
			GroupIdentity item = item2.First().Item2;
			if (!groups.TryGetValue(item.Id, out SkinGroup value))
			{
				value = new SkinGroup(item.Id, item.DisplayName);
				groups.Add(item.Id, value);
			}
			if (!value.Options.Any((SkinOption option) => option.EffectiveProviderId.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase)))
			{
				value.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, item2.ToDictionary<(ResourceAsset, GroupIdentity), string, ResourceAsset>(((ResourceAsset Asset, GroupIdentity Group) pair) => pair.Asset.SourcePath, ((ResourceAsset Asset, GroupIdentity Group) pair) => pair.Asset, StringComparer.OrdinalIgnoreCase)));
				ModLog.Info($"皮肤提供者 {index.Mod.Name} 的基础路径组 {item.Id} " + "未出现在实体索引中，已按基线资源补入皮肤选项。");
			}
		}
	}

	internal static string? ResolvePartTakeoverDefault(IEnumerable<SkinOption> options)
	{
		List<string> list = new List<string>();
		list.AddRange(from option in options
			where option.IsBaselinePartOverride
			select option.Id);
		return BaselinePartTakeoverPolicy.ResolveAdoptedDefault(new _003C_003Ez__ReadOnlyList<string>(list));
	}

	private static void AddDirectCharacterRuntimeProviderOptions(IReadOnlyCollection<PckResourceIndex> indexes, IDictionary<string, SkinGroup> groups, IReadOnlySet<string> knownCharacterGroupIds)
	{
		foreach (PckResourceIndex index2 in indexes.Where((PckResourceIndex index) => index.Mod.HasDll && index.Mod.RootPath != null))
		{
			string text = SkinPackagePaths.Resolve(index2.Mod.RootPath, index2.Mod.ResourceNamespaceId, ".dll");
			if (!((!File.Exists(text)) ? ((IEnumerable<string>)Directory.EnumerateFiles(index2.Mod.RootPath, "*.dll", SearchOption.TopDirectoryOnly).ToArray()) : ((IEnumerable<string>)new string[1] { text })).Any(HasDirectVisualHarmonyPatch))
			{
				continue;
			}
			foreach (string item in DirectCharacterRuntimeTargetScanner.Scan(index2.Mod.RootPath, index2.Mod.ResourceNamespaceId, knownCharacterGroupIds))
			{
				if (!groups.TryGetValue(item, out SkinGroup value))
				{
					value = new SkinGroup(item, DisplayName(item));
					groups.Add(item, value);
				}
				int num = value.Options.FindIndex((SkinOption option) => option.Id.Equals(index2.Mod.Id, StringComparison.OrdinalIgnoreCase));
				if (num >= 0)
				{
					value.Options[num] = value.Options[num]with
					{
						IsRuntimeProvider = true,
						IsDirectCharacterRuntimeProvider = true,
						IsCharacterIconOnly = false
					};
				}
				else
				{
					value.Options.Add(new SkinOption(index2.Mod.Id, index2.Mod.Name, new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase), IsRuntimeProvider: true, IsDirectCharacterRuntimeProvider: true));
				}
			}
		}
	}

	private static IReadOnlyDictionary<string, GroupIdentity> BuildManagedMonsterAssetGroups(IEnumerable<PckResourceIndex> baselineIndexes)
	{
		Dictionary<string, List<GroupIdentity>> dictionary = new Dictionary<string, List<GroupIdentity>>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex item in baselineIndexes.Where((PckResourceIndex index) => index.Mod.AffectsGameplay && index.Mod.HasDll))
		{
			foreach (ManagedMonsterAssetDeclaration item2 in ManagedMonsterSceneScanner.ScanDeclaredAssets(item.Mod.RootPath, item.Mod.ResourceNamespaceId))
			{
				string id = item2.ModelTypeName.ToLowerInvariant();
				GroupIdentity identity = new GroupIdentity(id, DisplayName(id));
				foreach (string resourcePath in item2.ResourcePaths)
				{
					string key = NormalizeTakeoverPath(resourcePath);
					if (!dictionary.TryGetValue(key, out var value))
					{
						value = new List<GroupIdentity>();
						dictionary.Add(key, value);
					}
					if (value.All((GroupIdentity owner) => !owner.Id.Equals(identity.Id, StringComparison.OrdinalIgnoreCase)))
					{
						value.Add(identity);
					}
				}
			}
		}
		return dictionary.Where((KeyValuePair<string, List<GroupIdentity>> pair) => pair.Value.Count == 1).ToDictionary<KeyValuePair<string, List<GroupIdentity>>, string, GroupIdentity>((KeyValuePair<string, List<GroupIdentity>> pair) => pair.Key, (KeyValuePair<string, List<GroupIdentity>> pair) => pair.Value[0], StringComparer.OrdinalIgnoreCase);
	}

	private static void AddManagedMonsterSceneOptions(IReadOnlyCollection<PckResourceIndex> indexes, IDictionary<string, SkinGroup> groups, IReadOnlySet<string> knownGroupIds)
	{
		foreach (PckResourceIndex index2 in indexes.Where((PckResourceIndex index) => index.Mod.HasDll))
		{
			foreach (ManagedMonsterSceneReplacement item in ManagedMonsterSceneScanner.Scan(index2.Mod.RootPath, index2.Mod.ResourceNamespaceId))
			{
				ResourceAsset resourceAsset = index2.Assets.GetValueOrDefault(item.ScenePath) ?? index2.TryBuildAsset(item.ScenePath);
				if (resourceAsset == null)
				{
					continue;
				}
				string modelToken = NormalizeResourceToken(item.ModelTypeName);
				SkinGroup skinGroup = groups.Values.FirstOrDefault((SkinGroup candidate) => NormalizeResourceToken(candidate.Id).Equals(modelToken, StringComparison.OrdinalIgnoreCase));
				if (skinGroup == null)
				{
					string text = item.ModelTypeName.ToLowerInvariant();
					if (!knownGroupIds.Contains(text))
					{
						continue;
					}
					skinGroup = new SkinGroup(text, DisplayName(text));
					groups.Add(text, skinGroup);
				}
				int num = skinGroup.Options.FindIndex((SkinOption option) => option.Id.Equals(index2.Mod.Id, StringComparison.OrdinalIgnoreCase));
				if (num >= 0)
				{
					skinGroup.Options[num] = skinGroup.Options[num]with
					{
						IsRuntimeProvider = true,
						ManagedMonsterScene = resourceAsset
					};
				}
				else
				{
					skinGroup.Options.Add(new SkinOption(index2.Mod.Id, index2.Mod.Name, new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase), IsRuntimeProvider: true, IsDirectCharacterRuntimeProvider: false, null, resourceAsset));
				}
			}
			AddManagedMonsterRuntimeProfileOptions(index2, groups, knownGroupIds);
		}
	}

	private static void AddManagedMonsterRuntimeProfileOptions(PckResourceIndex index, IDictionary<string, SkinGroup> groups, IReadOnlySet<string> knownGroupIds)
	{
		IReadOnlyList<ManagedMonsterRuntimeProfile> readOnlyList = ManagedMonsterSceneScanner.ScanRuntimeProfiles(index.Mod.RootPath, index.Mod.ResourceNamespaceId);
		if (readOnlyList.Count == 0)
		{
			return;
		}
		(ManagedMonsterRuntimeProfile, GroupIdentity, ResourceAsset[])[] array = (from entry in readOnlyList.Select(delegate(ManagedMonsterRuntimeProfile profile)
			{
				GroupIdentity item = TryGetPrimaryGroup(profile.TargetScenePath);
				ResourceAsset[] item2 = (from asset in profile.ProviderResourcePaths.Select(index.TryBuildAsset)
					where asset != null
					select asset).Cast<ResourceAsset>().DistinctBy<ResourceAsset, string>((ResourceAsset asset) => asset.SourcePath, StringComparer.OrdinalIgnoreCase).ToArray();
				return (Profile: profile, Identity: item, Assets: item2);
			})
			where entry.Identity != null && knownGroupIds.Contains(entry.Identity.Id) && entry.Assets.Length != 0
			select entry).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		HashSet<string> targetGroupIds = array.Select(((ManagedMonsterRuntimeProfile Profile, GroupIdentity Identity, ResourceAsset[] Assets) entry) => entry.Identity.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> providerResourcePrefixes = (from prefix in array.SelectMany(((ManagedMonsterRuntimeProfile Profile, GroupIdentity Identity, ResourceAsset[] Assets) entry) => entry.Profile.ProviderResourcePaths).Select(GetProviderResourcePrefix)
			where prefix != null
			select prefix).Cast<string>().ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		SkinGroup[] array2 = groups.Values.Where((SkinGroup group) => !targetGroupIds.Contains(group.Id)).ToArray();
		foreach (SkinGroup skinGroup in array2)
		{
			skinGroup.Options.RemoveAll((SkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase) && option.Assets.Count > 0 && option.Assets.Values.All((ResourceAsset asset) => providerResourcePrefixes.Any((string prefix) => asset.SourcePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))));
			if (skinGroup.Options.Count == 0)
			{
				groups.Remove(skinGroup.Id);
			}
		}
		(ManagedMonsterRuntimeProfile, GroupIdentity, ResourceAsset[])[] array3 = array;
		for (int i = 0; i < array3.Length; i++)
		{
			(ManagedMonsterRuntimeProfile, GroupIdentity, ResourceAsset[]) tuple = array3[i];
			GroupIdentity item3 = tuple.Item2;
			if (!groups.TryGetValue(item3.Id, out SkinGroup value))
			{
				value = new SkinGroup(item3.Id, item3.DisplayName);
				groups.Add(item3.Id, value);
			}
			Dictionary<string, ResourceAsset> dictionary = tuple.Item3.ToDictionary<ResourceAsset, string>((ResourceAsset asset) => asset.SourcePath, StringComparer.OrdinalIgnoreCase);
			int num = value.Options.FindIndex((SkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase));
			if (num >= 0)
			{
				SkinOption skinOption = value.Options[num];
				Dictionary<string, ResourceAsset> dictionary2 = new Dictionary<string, ResourceAsset>(skinOption.Assets, StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, ResourceAsset> item4 in dictionary)
				{
					dictionary2[item4.Key] = item4.Value;
				}
				value.Options[num] = skinOption with
				{
					Assets = dictionary2,
					IsRuntimeProvider = true,
					IsManagedMonsterRuntimeProfile = true
				};
			}
			else
			{
				value.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, dictionary, IsRuntimeProvider: true, IsDirectCharacterRuntimeProvider: false, null, null, null, null, IsManagedMonsterRuntimeProfile: true));
			}
		}
		static string? GetProviderResourcePrefix(string resourcePath)
		{
			if (!resourcePath.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			int num2 = resourcePath.IndexOf('/', "res://".Length);
			if (num2 >= 0)
			{
				return resourcePath.Substring(0, num2 + 1);
			}
			return null;
		}
	}

	private static void AddRuntimeMonsterVisualModeOptions(IReadOnlyCollection<PckResourceIndex> indexes, IDictionary<string, SkinGroup> groups)
	{
		Dictionary<string, PckResourceIndex> dictionary = indexes.ToDictionary<PckResourceIndex, string>((PckResourceIndex index) => index.Mod.Id, StringComparer.OrdinalIgnoreCase);
		foreach (SkinGroup value2 in groups.Values)
		{
			for (int num = value2.Options.Count - 1; num >= 0; num--)
			{
				SkinOption skinOption = value2.Options[num];
				if (dictionary.TryGetValue(skinOption.Id, out var value))
				{
					IReadOnlyList<RuntimeMonsterVisualMode> readOnlyList = RuntimeMonsterVisualModeScanner.Scan(value, value.Mod.Id, skinOption);
					if (readOnlyList.Count >= 2)
					{
						RuntimeMonsterVisualMode runtimeMonsterVisualMode = readOnlyList.FirstOrDefault((RuntimeMonsterVisualMode mode) => mode.ModeName.Contains("Default", StringComparison.OrdinalIgnoreCase) || mode.ModeName.Contains("Performance", StringComparison.OrdinalIgnoreCase)) ?? readOnlyList[0];
						value2.Options.RemoveAt(num);
						for (int num2 = readOnlyList.Count - 1; num2 >= 0; num2--)
						{
							RuntimeMonsterVisualMode runtimeMonsterVisualMode2 = readOnlyList[num2];
							Dictionary<string, ResourceAsset> dictionary2 = new Dictionary<string, ResourceAsset>(skinOption.Assets, StringComparer.OrdinalIgnoreCase);
							foreach (string resourcePath in runtimeMonsterVisualMode2.ResourcePaths)
							{
								ResourceAsset resourceAsset = value.TryBuildAsset(resourcePath);
								if (resourceAsset != null)
								{
									dictionary2[resourcePath] = resourceAsset;
								}
							}
							string id = ((runtimeMonsterVisualMode2 == runtimeMonsterVisualMode) ? skinOption.Id : (skinOption.Id + "::visual-mode:" + runtimeMonsterVisualMode2.ModeName.ToLowerInvariant()));
							value2.Options.Insert(num, skinOption with
							{
								Id = id,
								Name = skinOption.Name + " · " + runtimeMonsterVisualMode2.DisplayName,
								Assets = dictionary2,
								RuntimeMonsterVisualMode = runtimeMonsterVisualMode2,
								ProviderId = runtimeMonsterVisualMode2.ProviderId
							});
						}
					}
				}
			}
		}
	}

	private static IReadOnlyList<CardSkinGroup> BuildCardGroups(IEnumerable<PckResourceIndex> cosmeticIndexes)
	{
		Dictionary<string, CardSkinGroup> dictionary = new Dictionary<string, CardSkinGroup>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex index in cosmeticIndexes)
		{
			foreach (string item in index.Archive.Paths.Where((string path) => path.EndsWith("/card_replacements.json", StringComparison.OrdinalIgnoreCase)))
			{
				try
				{
					CardReplacementConfig cardReplacementConfig = DeserializeCardJson<CardReplacementConfig>(index.Archive.ReadFile(item));
					if (cardReplacementConfig == null || cardReplacementConfig.AncientReplacements.Count > 0 || cardReplacementConfig.NormalReplacements.Any((NormalCardReplacement entry) => !string.IsNullOrWhiteSpace(entry.DifferentialPortrait)))
					{
						continue;
					}
					string[] array = (from id in cardReplacementConfig.NormalReplacements.Select((NormalCardReplacement entry) => TryGetCardPortraitGroup(entry.PortraitPath)).Concat<string>(cardReplacementConfig.AncientReplacements.SelectMany((AncientCardReplacement entry) => new string[2]
						{
							TryGetCardPortraitGroup(entry.NormalPortrait),
							TryGetCardPortraitGroup(entry.AncientPortrait)
						}))
						where id != null
						select id).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
					foreach (string groupId in array)
					{
						Dictionary<string, string> dictionary2 = (from entry in cardReplacementConfig.NormalReplacements
							where TryGetCardPortraitGroup(entry.PortraitPath)?.Equals(groupId, StringComparison.OrdinalIgnoreCase) ?? false
							where !string.IsNullOrWhiteSpace(entry.CardType) && !string.IsNullOrWhiteSpace(entry.PortraitPath)
							select entry).GroupBy<NormalCardReplacement, string>((NormalCardReplacement entry) => entry.CardType, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, NormalCardReplacement>, string, string>((IGrouping<string, NormalCardReplacement> entries) => entries.Key, (IGrouping<string, NormalCardReplacement> entries) => entries.Last().PortraitPath, StringComparer.OrdinalIgnoreCase);
						Dictionary<string, AncientCardPortrait> dictionary3 = (from entry in cardReplacementConfig.AncientReplacements
							where TryGetCardPortraitGroup(entry.PathForGrouping)?.Equals(groupId, StringComparison.OrdinalIgnoreCase) ?? false
							where !string.IsNullOrWhiteSpace(entry.CardType)
							select entry).GroupBy<AncientCardReplacement, string>((AncientCardReplacement entry) => entry.CardType, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, AncientCardReplacement>, string, AncientCardPortrait>((IGrouping<string, AncientCardReplacement> entries) => entries.Key, (IGrouping<string, AncientCardReplacement> entries) => new AncientCardPortrait(entries.Last().NormalPortrait, entries.Last().AncientPortrait), StringComparer.OrdinalIgnoreCase);
						Dictionary<string, CardPresentationDefinition> presentations = (from entry in cardReplacementConfig.AncientReplacements
							where TryGetCardPortraitGroup(entry.PathForGrouping)?.Equals(groupId, StringComparison.OrdinalIgnoreCase) ?? false
							where !string.IsNullOrWhiteSpace(entry.CardType)
							select entry).GroupBy<AncientCardReplacement, string>((AncientCardReplacement entry) => NormalizeCardPresentationType(entry.CardType), StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, AncientCardReplacement>, string, CardPresentationDefinition>((IGrouping<string, AncientCardReplacement> entries) => entries.Key, (IGrouping<string, AncientCardReplacement> _) => new CardPresentationDefinition(UseAncientLayout: true, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, UseFullFrameArt: false, UseExpandedPortraitLayout: false, null, null, null, null, null, null, null, null, null), StringComparer.OrdinalIgnoreCase);
						if (dictionary2.Count != 0 || dictionary3.Count != 0)
						{
							if (!dictionary.TryGetValue(groupId, out var value))
							{
								value = new CardSkinGroup(groupId, DisplayName(groupId));
								dictionary.Add(groupId, value);
							}
							int num = value.Options.FindIndex((CardSkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase));
							CardSkinOption cardSkinOption = new CardSkinOption(index.Mod.Id, index.Mod.Name, dictionary2, dictionary3, null, index.Mod.RootPath, index.Mod.Id, presentations);
							if (num >= 0)
							{
								value.Options[num] = value.Options[num].Merge(cardSkinOption);
							}
							else
							{
								value.Options.Add(cardSkinOption);
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
		}
		foreach (CardSkinGroup value2 in dictionary.Values)
		{
			value2.Options.Sort((CardSkinOption left, CardSkinOption right) => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase));
		}
		return dictionary.Values.OrderBy((CardSkinGroup group) => GroupSortOrder(group.Id)).ThenBy<CardSkinGroup, string>((CardSkinGroup group) => group.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToArray();
	}

	private static IReadOnlyList<CardSkinOption> BuildPckCardOptions(IEnumerable<PckResourceIndex> cosmeticIndexes, IReadOnlyList<PckResourceIndex>? baselineIndexes = null)
	{
		List<CardSkinOption> list = new List<CardSkinOption>();
		HashSet<string> knownCardGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex item in baselineIndexes ?? Array.Empty<PckResourceIndex>())
		{
			foreach (string item2 in item.Archive.Paths.Select(NormalizeIndexedSourcePath).Where(IsCardArtSourcePath))
			{
				string text = TryGetCardArtPathLayout(item2, knownCardGroups)?.Category;
				if (!string.IsNullOrWhiteSpace(text))
				{
					knownCardGroups.Add(text);
				}
			}
		}
		foreach (PckResourceIndex index in cosmeticIndexes)
		{
			StatefulCardArtContract statefulCardArtContract = StatefulCardArtContract.Scan(index.Mod);
			if ((object)statefulCardArtContract != null)
			{
				IReadOnlyDictionary<string, ResourceAsset> readOnlyDictionary = statefulCardArtContract.ReadAtlas(index);
				if (readOnlyDictionary.Count > 0)
				{
					list.Add(new CardSkinOption(index.Mod.Id, index.Mod.Name, new Dictionary<string, string>(), new Dictionary<string, AncientCardPortrait>(), readOnlyDictionary, index.Mod.RootPath, index.Mod.Id)
					{
						StatefulArt = statefulCardArtContract with
						{
							AvailablePortraits = readOnlyDictionary.Keys.ToHashSet<string>(StringComparer.Ordinal)
						}
					});
				}
				continue;
			}
			ProviderCardBehaviorHints providerCardBehaviorHints = ProviderCardBehaviorScanner.Scan(index.Mod.RootPath, index.Assets.Values);
			ExportedCardPortraits exportedCardPortraits = LoadExportedCardPortraits(index);
			CardSurfaceDefinition exportedSurface = ((exportedCardPortraits.Normal.Count + exportedCardPortraits.Ancient.Count + exportedCardPortraits.Modes.Count > 0) ? ManagedCardPresentationScanner.ScanExportedSurface(index.Mod.RootPath, index.Mod.ResourceNamespaceId) : null);
			Dictionary<string, string> dictionary = new Dictionary<string, string>(exportedCardPortraits.Normal, StringComparer.OrdinalIgnoreCase);
			foreach (KeyValuePair<string, string> item3 in ManagedCardPortraitReplacementScanner.Scan(index.Mod.RootPath, index.Mod.ResourceNamespaceId))
			{
				if (index.TryBuildAsset(item3.Value) != null)
				{
					dictionary[item3.Key] = item3.Value;
				}
			}
			IReadOnlyDictionary<string, CardPresentationDefinition> readOnlyDictionary2 = LoadCardPresentations(index, providerCardBehaviorHints.Presentations, dictionary.Keys.Concat(exportedCardPortraits.Ancient.Keys).Concat(exportedCardPortraits.Modes.SelectMany((ExportedCardPortraitMode mode) => mode.Portraits.Keys)).Distinct<string>(StringComparer.OrdinalIgnoreCase));
			HashSet<string> optionalAncientCards = exportedCardPortraits.Modes.Where((ExportedCardPortraitMode mode) => mode.UseAncientLayout).SelectMany((ExportedCardPortraitMode mode) => mode.Portraits.Keys).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, CardPresentationDefinition> dictionary2 = readOnlyDictionary2.ToDictionary<KeyValuePair<string, CardPresentationDefinition>, string, CardPresentationDefinition>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key, (KeyValuePair<string, CardPresentationDefinition> pair) => (!optionalAncientCards.Contains(pair.Key)) ? pair.Value : pair.Value with
			{
				UseAncientLayout = false
			}, StringComparer.OrdinalIgnoreCase);
			ResourceAsset[] first = index.Assets.Values.Where((ResourceAsset asset) => IsCardArtSourcePath(asset.SourcePath)).ToArray();
			ResourceAsset[] array = index.Assets.Values.Where((ResourceAsset asset) => IsLooseProviderCardArtPath(index.Mod.ResourceNamespaceId, asset.SourcePath)).ToArray();
			ResourceAsset[] second = (IsBulkLooseCardPack(index.Mod.ResourceNamespaceId, (IReadOnlyCollection<ResourceAsset>)(object)array) ? array : Array.Empty<ResourceAsset>());
			ResourceAsset[] array2 = first.Concat(second).DistinctBy<ResourceAsset, string>((ResourceAsset asset) => asset.SourcePath, StringComparer.OrdinalIgnoreCase).ToArray();
			ResourceAsset[] array3 = ((baselineIndexes == null) ? array2 : array2.Where((ResourceAsset asset) => AssetDiffersFromBaseline(asset, baselineIndexes)).ToArray());
			if (array3.Length == 0 && dictionary2.Count == 0 && dictionary.Count == 0 && exportedCardPortraits.Ancient.Count == 0 && exportedCardPortraits.Modes.Count == 0)
			{
				continue;
			}
			string[] array4 = array2.Select((ResourceAsset asset) => GetCardVariantKey(asset.SourcePath, knownCardGroups)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
			CardArtVariant[] array5 = (from @group in array3.GroupBy<ResourceAsset, string>((ResourceAsset asset) => GetCardVariantKey(asset.SourcePath, knownCardGroups), StringComparer.OrdinalIgnoreCase)
				select new CardArtVariant(@group.Key, @group.ToArray(), (from asset in @group
					select TryGetCardArtIdentity(asset.SourcePath, knownCardGroups)?.Stem into stem
					where !string.IsNullOrEmpty(stem)
					select stem).Cast<string>().ToHashSet<string>(StringComparer.OrdinalIgnoreCase)) into @group
				orderby (@group.Key.Length != 0) ? 1 : 0
				select @group).ThenBy<CardArtVariant, string>((CardArtVariant group) => group.Key, StringComparer.CurrentCultureIgnoreCase).ToArray();
			bool splitVariants = dictionary.Count == 0 && exportedCardPortraits.Ancient.Count == 0 && array5.Length > 1 && VariantsOverlap(array5);
			CardArtVariant[] array6 = (splitVariants ? array5 : new CardArtVariant[1]
			{
				new CardArtVariant((array5.Length == 1) ? array5[0].Key : string.Empty, array3, new HashSet<string>(StringComparer.OrdinalIgnoreCase))
			});
			bool flag = splitVariants || (array6[0].Key.Length > 0 && array4.Length > 1);
			int optionCount = array6.Length + exportedCardPortraits.Modes.Count;
			int num = 0;
			CardArtVariant[] array7 = array6;
			foreach (CardArtVariant variant in array7)
			{
				string id = ((array6.Length == 1 || variant.Key.Length == 0) ? index.Mod.Id : (index.Mod.Id + "::variant:" + variant.Key.ToLowerInvariant()));
				string text2 = ((flag && variant.Key.Length > 0) ? DisplayCardVariant(variant.Key) : null);
				string name = CardSkinOptionNamingPolicy.Build(index.Mod.Name, text2, (text2 == null) ? (++num) : 0, optionCount);
				CardSkinOption source = new CardSkinOption(id, name, new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase), (from pair in MergeAncientPortraits(providerCardBehaviorHints.AncientPortraits, exportedCardPortraits.Ancient)
					where !splitVariants || variant.Stems.Contains(NormalizeCardPresentationType(pair.Key))
					select pair).ToDictionary<KeyValuePair<string, AncientCardPortrait>, string, AncientCardPortrait>((KeyValuePair<string, AncientCardPortrait> pair) => pair.Key, (KeyValuePair<string, AncientCardPortrait> pair) => pair.Value, StringComparer.OrdinalIgnoreCase), variant.Assets.ToDictionary<ResourceAsset, string, ResourceAsset>((ResourceAsset asset) => asset.SourcePath, (ResourceAsset asset) => asset, StringComparer.OrdinalIgnoreCase), index.Mod.RootPath, index.Mod.Id, dictionary2.Where((KeyValuePair<string, CardPresentationDefinition> pair) => !splitVariants || variant.Stems.Contains(NormalizeCardPresentationType(pair.Key))).ToDictionary<KeyValuePair<string, CardPresentationDefinition>, string, CardPresentationDefinition>((KeyValuePair<string, CardPresentationDefinition> pair) => pair.Key, (KeyValuePair<string, CardPresentationDefinition> pair) => pair.Value, StringComparer.OrdinalIgnoreCase))
				{
					CardSurfaces = ((exportedSurface == null) ? new Dictionary<string, CardSurfaceDefinition>() : exportedCardPortraits.Normal.Keys.Concat(exportedCardPortraits.Ancient.Keys).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToDictionary<string, string, CardSurfaceDefinition>((string key) => key, (string _) => exportedSurface, StringComparer.OrdinalIgnoreCase))
				};
				list.AddRange(CardLayoutVariantPolicy.Expand(source, index.Mod.ResourceNamespaceId));
			}
			foreach (ExportedCardPortraitMode mode in exportedCardPortraits.Modes)
			{
				Dictionary<string, CardPresentationDefinition> dictionary3 = new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase);
				foreach (string key in mode.Portraits.Keys)
				{
					if (readOnlyDictionary2.TryGetValue(key, out var value))
					{
						dictionary3[key] = value with
						{
							UseAncientLayout = mode.UseAncientLayout
						};
					}
					else if (mode.UseAncientLayout)
					{
						dictionary3[key] = new CardPresentationDefinition(UseAncientLayout: true, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, UseFullFrameArt: false, UseExpandedPortraitLayout: false, null, null, null, null, null, null, null, null, null);
					}
				}
				Dictionary<string, ResourceAsset> dictionary4 = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
				foreach (string value2 in mode.Portraits.Values)
				{
					ResourceAsset resourceAsset = index.TryBuildAsset(value2);
					if (resourceAsset != null)
					{
						dictionary4[resourceAsset.SourcePath] = resourceAsset;
					}
				}
				list.Add(new CardSkinOption(index.Mod.Id + "::portrait-mode:" + mode.IdSuffix, CardSkinOptionNamingPolicy.Build(index.Mod.Name, null, ++num, optionCount), new Dictionary<string, string>(mode.Portraits, StringComparer.OrdinalIgnoreCase), new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase), dictionary4, index.Mod.RootPath, index.Mod.Id, dictionary3)
				{
					CardSurfaces = ((exportedSurface == null) ? new Dictionary<string, CardSurfaceDefinition>() : mode.Portraits.Keys.ToDictionary<string, string, CardSurfaceDefinition>((string key) => key, (string _) => exportedSurface, StringComparer.OrdinalIgnoreCase))
				});
			}
		}
		return list;
	}

	private static IReadOnlyDictionary<string, AncientCardPortrait> MergeAncientPortraits(IReadOnlyDictionary<string, AncientCardPortrait> inferred, IReadOnlyDictionary<string, AncientCardPortrait> exported)
	{
		if (inferred.Count == 0)
		{
			return exported;
		}
		if (exported.Count == 0)
		{
			return inferred;
		}
		Dictionary<string, AncientCardPortrait> dictionary = new Dictionary<string, AncientCardPortrait>(inferred, StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, AncientCardPortrait> item in exported)
		{
			if (dictionary.TryGetValue(item.Key, out var value))
			{
				dictionary[item.Key] = new AncientCardPortrait(item.Value.NormalPortrait ?? value.NormalPortrait, item.Value.AncientPortrait ?? value.AncientPortrait);
			}
			else
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	private static string NormalizeIndexedSourcePath(string path)
	{
		string text;
		if (path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
		{
			text = path;
			return text.Substring(0, text.Length - 7);
		}
		if (!path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		text = path;
		return text.Substring(0, text.Length - 6);
	}

	private static IReadOnlyDictionary<string, CardPresentationDefinition> LoadCardPresentations(PckResourceIndex index, IReadOnlyDictionary<string, CardPresentationDefinition>? providerBehavior = null, IEnumerable<string>? declaredCardTypes = null)
	{
		Dictionary<string, CardPresentationDefinition> dictionary = new Dictionary<string, CardPresentationDefinition>(StringComparer.OrdinalIgnoreCase);
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in index.Archive.Paths.Where((string path) => path.EndsWith("/frame_replacements.json", StringComparison.OrdinalIgnoreCase) || path.EndsWith("/framed_card_project.json", StringComparison.OrdinalIgnoreCase)))
		{
			try
			{
				CardFrameReplacementDocument cardFrameReplacementDocument = DeserializeCardJson<CardFrameReplacementDocument>(index.Archive.ReadFile(item));
				if (cardFrameReplacementDocument == null)
				{
					continue;
				}
				foreach (CardFrameReplacementEntry item2 in cardFrameReplacementDocument.Entries.Where((CardFrameReplacementEntry entry) => !string.IsNullOrWhiteSpace(entry.CardId)))
				{
					string text = NormalizeCardPresentationType(item2.CardId);
					dictionary[text] = new CardPresentationDefinition(item2.UiMode.Equals("Ancient", StringComparison.OrdinalIgnoreCase), EmptyToNull(item2.Frame), EmptyToNull(item2.FrameMaterial), EmptyToNull(item2.BannerTexture), EmptyToNull(item2.BannerMaterial), EmptyToNull(item2.PortraitBorder), EmptyToNull(item2.PortraitBorderMaterial), EmptyToNull(item2.AncientTextBg), EmptyToNull(item2.TextBackgroundMaterial), EmptyToNull(item2.EnergyIcon), EmptyToNull(item2.Highlight), EmptyToNull(item2.HighlightMaterial), item2.FrameVisible, item2.BannerVisible, item2.TextBackgroundVisible, item2.PortraitBorderVisible, item2.EnergyIconVisible, item2.HighlightVisible, item2.TypePlaqueVisible, item2.TypeLabelVisible, item2.DescriptionVisible, item2.InfectionOverlayVisible, UseFullFrameArt: false, UseExpandedPortraitLayout: false, null, null, null, null, null, null, null, null, null);
					if (!string.IsNullOrWhiteSpace(item2.UiMode))
					{
						hashSet.Add(text);
					}
				}
			}
			catch (Exception)
			{
			}
		}
		string[] knownCardStems = (from path in index.Assets.Keys
			select TryGetCardArtIdentity(path) into identity
			where identity != null
			select identity.Stem).Concat(declaredCardTypes ?? Array.Empty<string>()).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		foreach (KeyValuePair<string, CardPresentationDefinition> item3 in ManagedCardPresentationScanner.Scan(index.Mod.RootPath, (IReadOnlyCollection<string>)(object)knownCardStems, (IReadOnlyCollection<string>?)(object)declaredCardTypes?.ToArray()))
		{
			if (dictionary.TryGetValue(item3.Key, out var value))
			{
				value = value with
				{
					BuiltInOverlayVisible = (value.BuiltInOverlayVisible ?? item3.Value.BuiltInOverlayVisible)
				};
				dictionary[item3.Key] = value;
				if (!hashSet.Contains(item3.Key) && (item3.Value.UseAncientLayout || item3.Value.UseExpandedPortraitLayout))
				{
					dictionary[item3.Key] = value with
					{
						UseAncientLayout = item3.Value.UseAncientLayout,
						UseExpandedPortraitLayout = item3.Value.UseExpandedPortraitLayout
					};
				}
			}
			else
			{
				dictionary.Add(item3.Key, item3.Value);
			}
		}
		if (providerBehavior != null)
		{
			foreach (KeyValuePair<string, CardPresentationDefinition> item4 in providerBehavior)
			{
				dictionary.TryAdd(item4.Key, item4.Value);
			}
		}
		return dictionary;
	}

	private static T? DeserializeCardJson<T>(byte[] bytes)
	{
		return JsonSerializer.Deserialize<T>(StripUtf8Bom(bytes).Span, CardReplacementJsonOptions);
	}

	private static ReadOnlyMemory<byte> StripUtf8Bom(byte[] bytes)
	{
		return bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble) ? bytes.AsMemory(Encoding.UTF8.Preamble.Length) : ((Memory<byte>)bytes);
	}

	private static ExportedCardPortraits LoadExportedCardPortraits(PckResourceIndex index)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, AncientCardPortrait> dictionary2 = new Dictionary<string, AncientCardPortrait>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary3 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary4 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> dictionary5 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in index.Archive.Paths.Where((string path) => path.EndsWith("/card_replacements.json", StringComparison.OrdinalIgnoreCase)))
		{
			ReadExportedPortraitEntries(index, item, dictionary, dictionary2, new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "image", "portrait" }), new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "ancientImage", "ancientPortrait" }), requireStaticKind: true, overwrite: true);
			ReadCardPortraitsCoreEntries(index, item, dictionary, dictionary3, dictionary4, dictionary5);
		}
		foreach (string item2 in index.Archive.Paths.Where((string path) => path.EndsWith("/framed_card_project.json", StringComparison.OrdinalIgnoreCase)))
		{
			ReadExportedPortraitEntries(index, item2, dictionary, dictionary2, new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "portrait", "image" }), Array.Empty<string>(), requireStaticKind: false, overwrite: true);
		}
		foreach (string item3 in index.Archive.Paths.Where((string path) => path.EndsWith("/animations/card_animations.json", StringComparison.OrdinalIgnoreCase)))
		{
			ReadExportedPortraitEntries(index, item3, dictionary, dictionary2, new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "fallbackImage", "image", "portrait" }), Array.Empty<string>(), requireStaticKind: false, overwrite: false);
		}
		List<ExportedCardPortraitMode> list = new List<ExportedCardPortraitMode>();
		if (dictionary3.Count > 0)
		{
			list.Add(new ExportedCardPortraitMode("differential", "{skin-changer-differential}", dictionary3, UseAncientLayout: false));
		}
		if (dictionary4.Count > 0)
		{
			list.Add(new ExportedCardPortraitMode("ancient", "{skin-changer-ancient-style}", dictionary4, UseAncientLayout: true));
		}
		if (dictionary5.Count > 0)
		{
			list.Add(new ExportedCardPortraitMode("ancient-differential", "{skin-changer-ancient-differential}", dictionary5, UseAncientLayout: true));
		}
		return new ExportedCardPortraits(dictionary, dictionary2, list);
	}

	private static void ReadCardPortraitsCoreEntries(PckResourceIndex index, string configPath, IDictionary<string, string> normalPortraits, IDictionary<string, string> differentialPortraits, IDictionary<string, string> ancientStylePortraits, IDictionary<string, string> ancientDifferentialPortraits)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(StripUtf8Bom(index.Archive.ReadFile(configPath)));
			if (TryGetJsonProperty(jsonDocument.RootElement, "normalReplacements", out var value) && value.ValueKind == JsonValueKind.Array)
			{
				foreach (JsonElement item in value.EnumerateArray())
				{
					string text = TryGetJsonString(item, "cardType");
					if (!string.IsNullOrWhiteSpace(text))
					{
						string cardType2 = NormalizeCardPresentationType(text);
						AddOwnedPortrait(index, item, "portraitPath", cardType2, normalPortraits);
						AddOwnedPortrait(index, item, "differentialPortrait", cardType2, differentialPortraits);
					}
				}
			}
			if (!TryGetJsonProperty(jsonDocument.RootElement, "ancientReplacements", out var value2) || value2.ValueKind != JsonValueKind.Array)
			{
				return;
			}
			foreach (JsonElement item2 in value2.EnumerateArray())
			{
				string text2 = TryGetJsonString(item2, "cardType");
				if (!string.IsNullOrWhiteSpace(text2))
				{
					string cardType3 = NormalizeCardPresentationType(text2);
					AddOwnedPortrait(index, item2, "normalPortrait", cardType3, normalPortraits);
					if (AddOwnedPortrait(index, item2, "ancientPortrait", cardType3, ancientStylePortraits))
					{
						AddOwnedPortrait(index, item2, "differentialPortrait", cardType3, ancientDifferentialPortraits);
					}
				}
			}
		}
		catch (Exception)
		{
		}
		static bool AddOwnedPortrait(PckResourceIndex index, JsonElement entry, string property, string cardType, IDictionary<string, string> portraits)
		{
			string text3 = TryGetJsonString(entry, property);
			if (string.IsNullOrWhiteSpace(text3) || !text3.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || index.TryBuildAsset(text3) == null)
			{
				return false;
			}
			portraits[cardType] = text3;
			return true;
		}
	}

	private static void ReadExportedPortraitEntries(PckResourceIndex index, string configPath, IDictionary<string, string> normalPortraits, IDictionary<string, AncientCardPortrait> ancientPortraits, IReadOnlyList<string> portraitProperties, IReadOnlyList<string> ancientPortraitProperties, bool requireStaticKind, bool overwrite)
	{
		try
		{
			using JsonDocument jsonDocument = JsonDocument.Parse(StripUtf8Bom(index.Archive.ReadFile(configPath)));
			if (!TryGetJsonProperty(jsonDocument.RootElement, "entries", out var value) || value.ValueKind != JsonValueKind.Array)
			{
				return;
			}
			foreach (JsonElement entry in value.EnumerateArray())
			{
				string text = TryGetJsonString(entry, "cardId");
				if (string.IsNullOrWhiteSpace(text))
				{
					continue;
				}
				if (requireStaticKind)
				{
					string text2 = TryGetJsonString(entry, "kind");
					if (!string.IsNullOrWhiteSpace(text2) && !text2.Equals("image", StringComparison.OrdinalIgnoreCase) && !text2.Equals("static", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
				}
				string text3 = portraitProperties.Select((string property) => TryGetJsonString(entry, property)).FirstOrDefault((string path) => !string.IsNullOrWhiteSpace(path));
				string text4 = ancientPortraitProperties.Select((string property) => TryGetJsonString(entry, property)).FirstOrDefault((string path) => !string.IsNullOrWhiteSpace(path));
				if ((string.IsNullOrWhiteSpace(text3) && string.IsNullOrWhiteSpace(text4)) || !IsResourceReference(text3) || !IsResourceReference(text4))
				{
					continue;
				}
				string key = NormalizeCardPresentationType(text);
				if (!string.IsNullOrWhiteSpace(text3))
				{
					if (overwrite)
					{
						normalPortraits[key] = text3;
					}
					else
					{
						normalPortraits.TryAdd(key, text3);
					}
				}
				if (!string.IsNullOrWhiteSpace(text4))
				{
					if (overwrite || !ancientPortraits.ContainsKey(key))
					{
						ancientPortraits[key] = new AncientCardPortrait(text3, text4);
						continue;
					}
					AncientCardPortrait ancientCardPortrait = ancientPortraits[key];
					ancientPortraits[key] = new AncientCardPortrait(ancientCardPortrait.NormalPortrait ?? text3, ancientCardPortrait.AncientPortrait ?? text4);
				}
			}
		}
		catch (Exception)
		{
		}
		static bool IsResourceReference(string? path)
		{
			if (!string.IsNullOrWhiteSpace(path) && !path.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				return path.StartsWith("uid://", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}

	private static string? TryGetJsonString(JsonElement element, string propertyName)
	{
		if (!TryGetJsonProperty(element, propertyName, out var value) || value.ValueKind != JsonValueKind.String)
		{
			return null;
		}
		return value.GetString();
	}

	private static bool TryGetJsonProperty(JsonElement element, string propertyName, out JsonElement value)
	{
		if (element.ValueKind == JsonValueKind.Object)
		{
			foreach (JsonProperty item in element.EnumerateObject())
			{
				if (item.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
				{
					value = item.Value;
					return true;
				}
			}
		}
		value = default(JsonElement);
		return false;
	}

	private static string NormalizeCardPresentationType(string cardId)
	{
		int num = cardId.LastIndexOf('.');
		if (num < 0)
		{
			return cardId;
		}
		int num2 = num + 1;
		return cardId.Substring(num2, cardId.Length - num2);
	}

	private static string? EmptyToNull(string? value)
	{
		if (!string.IsNullOrWhiteSpace(value))
		{
			return value;
		}
		return null;
	}

	private static bool IsBulkLooseCardPack(string providerId, IReadOnlyCollection<ResourceAsset> candidates)
	{
		if (candidates.Count < 20)
		{
			return false;
		}
		int num = candidates.Count((ResourceAsset asset) => IsDirectProviderChild(providerId, asset.SourcePath));
		if (num >= 20)
		{
			return num * 100 >= candidates.Count * 80;
		}
		return false;
	}

	private static bool IsDirectProviderChild(string providerId, string path)
	{
		if (!IsProviderNamespacePath(path, NormalizeResourceToken(providerId)))
		{
			return false;
		}
		string text = path;
		string text2 = text.Substring(6, text.Length - 6);
		int num = text2.IndexOf('/');
		if (num >= 0)
		{
			text = text2;
			int num2 = num + 1;
			return !text.Substring(num2, text.Length - num2).Contains('/');
		}
		return false;
	}

	private static bool VariantsOverlap(IReadOnlyList<CardArtVariant> variants)
	{
		for (int i = 0; i < variants.Count; i++)
		{
			for (int j = i + 1; j < variants.Count; j++)
			{
				if (variants[i].Stems.Overlaps(variants[j].Stems))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool AssetDiffersFromBaseline(ResourceAsset asset, IReadOnlyList<PckResourceIndex> baselineIndexes)
	{
		ResourceAsset resourceAsset = null;
		int num = baselineIndexes.Count - 1;
		while (num >= 0 && resourceAsset == null)
		{
			resourceAsset = baselineIndexes[num].Assets.GetValueOrDefault(asset.SourcePath) ?? baselineIndexes[num].TryBuildAsset(asset.SourcePath);
			num--;
		}
		if (resourceAsset == null)
		{
			return true;
		}
		foreach (ResourceFile file in asset.Files)
		{
			string path = NormalizeTakeoverPath(file.Path);
			ResourceFile resourceFile = resourceAsset.Files.FirstOrDefault((ResourceFile candidate) => NormalizeTakeoverPath(candidate.Path).Equals(path, StringComparison.OrdinalIgnoreCase));
			if (resourceFile == null || file.IsInline != resourceFile.IsInline || !file.HasSameContentAs(resourceFile))
			{
				return true;
			}
		}
		return false;
	}

	private static string GetCardVariantKey(string path, IReadOnlySet<string>? knownCardGroups = null)
	{
		string text = TryGetCardArtPathLayout(path, knownCardGroups)?.Variant ?? string.Empty;
		if (!text.Equals("beta", StringComparison.OrdinalIgnoreCase))
		{
			return text;
		}
		return string.Empty;
	}

	private static string DisplayCardVariant(string variant)
	{
		if (variant.Length == 0)
		{
			return "{skin-changer-default}";
		}
		return variant.Replace('/', ' ').Replace('_', ' ').Replace('-', ' ')
			.CapitalizeWords();
	}

	private static string? TryGetCardPortraitGroup(string? path)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}
		Match match = CardPortraitGroupRegex().Match(path);
		if (!match.Success)
		{
			return null;
		}
		return match.Groups[1].Value.ToLowerInvariant();
	}

	private static bool IsCardArtSourcePath(string path)
	{
		if (!CardArtPathRegex().IsMatch(path) || !IsCardArtResourceExtension(path))
		{
			return false;
		}
		return TryGetCardArtPathLayout(path, null) != null;
	}

	private static bool IsCardArtResourceExtension(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".svg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".tres", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".res", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsLooseProviderCardArtPath(string providerId, string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".tres", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		return IsProviderNamespacePath(path, NormalizeResourceToken(providerId));
	}

	private static bool CardArtMatches(string assetPath, CardCatalogEntry card, IReadOnlySet<string> knownCardGroups)
	{
		CardArtIdentity cardArtIdentity = TryGetCardArtIdentity(assetPath, knownCardGroups);
		CardArtIdentity cardArtIdentity2 = TryGetCardArtIdentity(card.PortraitPath, knownCardGroups);
		if (cardArtIdentity == null || cardArtIdentity2 == null)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(cardArtIdentity.Category) && !cardArtIdentity.Category.Equals(card.PoolGroupId, StringComparison.OrdinalIgnoreCase) && !cardArtIdentity.Category.Equals(cardArtIdentity2.Category, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		string expected = NormalizeCardToken(card.TypeName);
		if (!CardStemsMatch(cardArtIdentity.Stem, cardArtIdentity2.Stem))
		{
			return CardStemsMatch(cardArtIdentity.Stem, expected);
		}
		return true;
	}

	private static CardArtIdentity? TryGetCardArtIdentity(string? path, IReadOnlySet<string>? knownCardGroups = null)
	{
		if (string.IsNullOrWhiteSpace(path))
		{
			return null;
		}
		string category = TryGetCardArtPathLayout(path, knownCardGroups)?.Category ?? string.Empty;
		string text = path;
		int num = path.LastIndexOf('/') + 1;
		string text2 = text.Substring(num, text.Length - num);
		int num2 = text2.LastIndexOf('.');
		string text3 = ((num2 >= 0) ? text2.Substring(0, num2) : text2);
		int num3 = text3.LastIndexOf('.');
		if (num3 >= 0)
		{
			text = text3;
			num = num3 + 1;
			text3 = text.Substring(num, text.Length - num);
		}
		string[] array = new string[5] { "_card_art", "-card-art", " card art", "card_art", "cardart" };
		foreach (string text4 in array)
		{
			if (text3.EndsWith(text4, StringComparison.OrdinalIgnoreCase))
			{
				text = text3;
				int length = text4.Length;
				text3 = text.Substring(0, text.Length - length);
				break;
			}
		}
		string stem = NormalizeCardToken(text3);
		return new CardArtIdentity(category, stem);
	}

	private static CardArtPathLayout? TryGetCardArtPathLayout(string path, IReadOnlySet<string>? knownCardGroups)
	{
		Match match = CardArtPathRegex().Match(path);
		int num = path.LastIndexOf('/');
		if (!match.Success || num < match.Index + match.Length)
		{
			return null;
		}
		int num2 = match.Index + match.Length;
		string[] array = path.Substring(num2, num - num2).Split('/', StringSplitOptions.RemoveEmptyEntries);
		if (array.Length == 0)
		{
			return null;
		}
		int groupIndex = ((knownCardGroups != null && knownCardGroups.Count != 0) ? Array.FindIndex(array, knownCardGroups.Contains) : 0);
		if (groupIndex < 0)
		{
			groupIndex = 0;
		}
		string category = array[groupIndex].ToLowerInvariant();
		string variant = string.Join('/', array.Where((string _, int index) => index != groupIndex));
		return new CardArtPathLayout(category, variant);
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

	private static bool IsDirectCharacterImagePath(string path)
	{
		string extension = Path.GetExtension(path);
		if (!extension.Equals(".png", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase))
		{
			return extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static void AddPckRuntimeProviderOptions(IReadOnlyCollection<PckResourceIndex> indexes, IReadOnlyCollection<PckResourceIndex> baselineIndexes, IDictionary<string, SkinGroup> groups, IReadOnlySet<string> knownGroupIds, IReadOnlySet<string> knownCharacterGroupIds)
	{
		foreach (PckResourceIndex index in indexes)
		{
			StatefulCardArtContract statefulCardArtContract = StatefulCardArtContract.Scan(index.Mod);
			if ((object)statefulCardArtContract != null)
			{
				foreach (var (text, assets) in statefulCardArtContract.CharacterAssets(index))
				{
					if (knownCharacterGroupIds.Contains(text))
					{
						if (!groups.TryGetValue(text, out SkinGroup value))
						{
							value = (groups[text] = new SkinGroup(text, DisplayName(text)));
						}
						value.Options.RemoveAll((SkinOption option) => option.EffectiveProviderId == index.Mod.Id);
						value.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, assets, IsRuntimeProvider: false, IsDirectCharacterRuntimeProvider: false, null, null, null, index.Mod.Id));
					}
				}
				continue;
			}
			HashSet<string> enabledGroupIds = ReadEnabledRuntimeGroupIds(index.Mod);
			FrameworkCharacterSkinContract[] array = (from contract in FrameworkSkinContractScanner.Scan(index.Mod.RootPath, index.Mod.ResourceNamespaceId)
				where FrameworkContractClosureOrLog(index, baselineIndexes, contract, index.Mod)
				select contract).ToArray();
			HashSet<string> frameworkTargetIds = array.Select((FrameworkCharacterSkinContract contract) => contract.TargetGroupId).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			FrameworkCharacterSkinContract[] array2 = array;
			foreach (FrameworkCharacterSkinContract frameworkCharacterSkinContract in array2)
			{
				if (!knownGroupIds.Contains(frameworkCharacterSkinContract.TargetGroupId))
				{
					continue;
				}
				if (!groups.TryGetValue(frameworkCharacterSkinContract.TargetGroupId, out SkinGroup value2))
				{
					value2 = new SkinGroup(frameworkCharacterSkinContract.TargetGroupId, DisplayName(frameworkCharacterSkinContract.TargetGroupId));
					groups.Add(frameworkCharacterSkinContract.TargetGroupId, value2);
				}
				Dictionary<string, ResourceAsset> dictionary = new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, string> characterResource in frameworkCharacterSkinContract.CharacterResources)
				{
					string frameworkCharacterCanonicalPath = GetFrameworkCharacterCanonicalPath(characterResource.Key, frameworkCharacterSkinContract.TargetGroupId);
					ResourceAsset resourceAsset = index.Assets.GetValueOrDefault(characterResource.Value) ?? index.TryBuildAsset(characterResource.Value);
					if (frameworkCharacterCanonicalPath != null && resourceAsset != null)
					{
						dictionary[frameworkCharacterCanonicalPath] = resourceAsset;
					}
				}
				value2.Options.RemoveAll((SkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase));
				string frameworkOptionId = ProviderInstanceIdentityPolicy.ScopeOptionId(index.Mod.ResourceNamespaceId, index.Mod.Id, frameworkCharacterSkinContract.OptionId);
				int num = value2.Options.FindIndex((SkinOption option) => option.Id.Equals(frameworkOptionId, StringComparison.OrdinalIgnoreCase));
				SkinOption skinOption = new SkinOption(frameworkOptionId, DistinguishDuplicateProviderOptionName(index.Mod, frameworkCharacterSkinContract.DisplayName), dictionary, IsRuntimeProvider: true, IsDirectCharacterRuntimeProvider: false, null, null, null, index.Mod.Id, IsManagedMonsterRuntimeProfile: false, frameworkCharacterSkinContract);
				if (num >= 0)
				{
					value2.Options[num] = skinOption;
				}
				else
				{
					value2.Options.Add(skinOption);
				}
			}
			IReadOnlyList<ManagedCharacterAssetReplacement> source = ManagedCharacterAssetReplacementScanner.Scan(index.Mod.RootPath, index.Mod.ResourceNamespaceId);
			Dictionary<string, RuntimeProviderAsset> managedRuntimeMappings = source.SelectMany((ManagedCharacterAssetReplacement replacement) => replacement.CanonicalPathsByProviderPath.Select<KeyValuePair<string, string>, KeyValuePair<string, RuntimeProviderAsset>>((KeyValuePair<string, string> pair) => new KeyValuePair<string, RuntimeProviderAsset>(pair.Key, new RuntimeProviderAsset(new GroupIdentity(replacement.TargetGroupId, DisplayName(replacement.TargetGroupId)), pair.Value)))).GroupBy<KeyValuePair<string, RuntimeProviderAsset>, string>((KeyValuePair<string, RuntimeProviderAsset> pair) => pair.Key, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, KeyValuePair<string, RuntimeProviderAsset>>, string, RuntimeProviderAsset>((IGrouping<string, KeyValuePair<string, RuntimeProviderAsset>> pairs) => pairs.Key, (IGrouping<string, KeyValuePair<string, RuntimeProviderAsset>> pairs) => pairs.Last().Value, StringComparer.OrdinalIgnoreCase);
			IReadOnlySet<string> first = CollectManagedCharacterDependencyPaths(index, managedRuntimeMappings.Keys);
			IReadOnlySet<string> second = CollectManagedCharacterDependencyPaths(index, array.SelectMany((FrameworkCharacterSkinContract contract) => contract.ResourcePaths));
			HashSet<string> declarativeDependencyPaths = first.Concat(second).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			ResourceAsset[] first2 = index.Assets.Values.ToArray();
			ResourceAsset[] second2 = (from asset in (from path in index.Archive.Paths
					where path.EndsWith(".tscn", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".tscn.remap", StringComparison.OrdinalIgnoreCase) || IsDirectCharacterImagePath(path)
					select (!path.EndsWith(".tscn.remap", StringComparison.OrdinalIgnoreCase)) ? path : path.Substring(0, path.Length - 6) into path
					where managedRuntimeMappings.ContainsKey(NormalizeTakeoverPath(path)) || TryGetRuntimeProviderAsset(index.Mod.ResourceNamespaceId, path) != null
					select path).Select(index.TryBuildAsset)
				where asset != null
				select asset).Cast<ResourceAsset>().ToArray();
			RuntimeProviderAsset value3;
			(ResourceAsset, RuntimeProviderAsset)[] source2 = (from asset in first2.Concat(second2).DistinctBy<ResourceAsset, string>((ResourceAsset asset) => asset.SourcePath, StringComparer.OrdinalIgnoreCase)
				select (Asset: asset, Mapping: managedRuntimeMappings.TryGetValue(asset.SourcePath, out value3) ? value3 : (declarativeDependencyPaths.Contains(asset.SourcePath) ? null : TryGetRuntimeProviderAsset(index.Mod.ResourceNamespaceId, asset.SourcePath))) into pair
				where pair.Mapping != null
				select (Asset: pair.Asset, Mapping: pair.Mapping)).ToArray();
			HashSet<string> managedTargetIds = source.Select((ManagedCharacterAssetReplacement replacement) => replacement.TargetGroupId).Concat(frameworkTargetIds).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> anchoredGroupIds = (from path in index.Assets.Keys.Concat(source2.Select(((ResourceAsset Asset, RuntimeProviderAsset Mapping) pair) => pair.Mapping.CanonicalPath))
				select TryGetCharacterVisualAnchorGroup(path, knownCharacterGroupIds) into identity
				where identity != null
				select identity.Id).Concat(managedTargetIds.Where(knownCharacterGroupIds.Contains)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			IReadOnlySet<string> eligibleCharacterGroupIds = CharacterGroupEvidencePolicy.ResolveEligibleGroups(source2.Select(((ResourceAsset Asset, RuntimeProviderAsset Mapping) pair) => pair.Mapping.Identity.Id).Concat(managedTargetIds).Concat(from @group in groups.Values
				where @group.Options.Any((SkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase))
				select @group.Id)
				.Where(knownCharacterGroupIds.Contains), anchoredGroupIds);
			SkinGroup[] array3 = groups.Values.Where((SkinGroup group) => !managedTargetIds.Contains(group.Id)).ToArray();
			foreach (SkinGroup sourceGroup in array3)
			{
				sourceGroup.Options.RemoveAll((SkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase) && option.Assets.Count > 0 && ((knownCharacterGroupIds.Contains(sourceGroup.Id) && !eligibleCharacterGroupIds.Contains(sourceGroup.Id)) || option.Assets.Values.Any((ResourceAsset asset) => declarativeDependencyPaths.Contains(asset.SourcePath))));
				if (sourceGroup.Options.Count == 0)
				{
					groups.Remove(sourceGroup.Id);
				}
			}
			GroupIdentity[] array4 = (from pair in source2
				select pair.Mapping.Identity into identity
				where knownGroupIds.Contains(identity.Id) && (!knownCharacterGroupIds.Contains(identity.Id) || eligibleCharacterGroupIds.Contains(identity.Id)) && !frameworkTargetIds.Contains(identity.Id) && (enabledGroupIds == null || enabledGroupIds.Contains(identity.Id) || managedTargetIds.Contains(identity.Id))
				select identity).DistinctBy((GroupIdentity identity) => identity.Id).ToArray();
			foreach (GroupIdentity identity2 in array4)
			{
				if (!groups.TryGetValue(identity2.Id, out SkinGroup value4))
				{
					value4 = new SkinGroup(identity2.Id, identity2.DisplayName);
					groups.Add(identity2.Id, value4);
				}
				Dictionary<string, ResourceAsset> dictionary2 = source2.Where(((ResourceAsset Asset, RuntimeProviderAsset Mapping) pair) => pair.Mapping.Identity.Id.Equals(identity2.Id, StringComparison.OrdinalIgnoreCase)).GroupBy<(ResourceAsset, RuntimeProviderAsset), string>(((ResourceAsset Asset, RuntimeProviderAsset Mapping) pair) => pair.Mapping.CanonicalPath, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, (ResourceAsset, RuntimeProviderAsset)>, string, ResourceAsset>((IGrouping<string, (ResourceAsset Asset, RuntimeProviderAsset Mapping)> pairs) => pairs.Key, (IGrouping<string, (ResourceAsset Asset, RuntimeProviderAsset Mapping)> pairs) => pairs.Last().Asset, StringComparer.OrdinalIgnoreCase);
				int num2 = value4.Options.FindIndex((SkinOption option) => option.Id.Equals(index.Mod.Id, StringComparison.OrdinalIgnoreCase));
				if (num2 >= 0)
				{
					SkinOption skinOption2 = value4.Options[num2];
					Dictionary<string, ResourceAsset> dictionary3 = new Dictionary<string, ResourceAsset>(skinOption2.Assets, StringComparer.OrdinalIgnoreCase);
					foreach (KeyValuePair<string, ResourceAsset> item in dictionary2)
					{
						dictionary3[item.Key] = item.Value;
					}
					value4.Options[num2] = skinOption2 with
					{
						Assets = dictionary3,
						IsRuntimeProvider = true,
						IsCharacterIconOnly = CharacterGroupEvidencePolicy.IsIconOnlySupplement(skinOption2.IsCharacterIconOnly && dictionary2.Keys.All(IsCharacterIconSourcePath), ReplacesCharacterVisualsAtRuntime(index.Mod))
					};
				}
				else
				{
					value4.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, dictionary2, IsRuntimeProvider: true, IsDirectCharacterRuntimeProvider: false, null, null, null, null, IsManagedMonsterRuntimeProfile: false, null, CharacterGroupEvidencePolicy.IsIconOnlySupplement(knownCharacterGroupIds.Contains(identity2.Id) && dictionary2.Keys.All(IsCharacterIconSourcePath), ReplacesCharacterVisualsAtRuntime(index.Mod))));
				}
			}
		}
	}

	private static bool FrameworkContractClosureOrLog(PckResourceIndex index, IReadOnlyCollection<PckResourceIndex> baselineIndexes, FrameworkCharacterSkinContract contract, SkinModDescriptor mod)
	{
		if (FrameworkContractResourceClosureComplete(index, baselineIndexes, contract))
		{
			return true;
		}
		try
		{
			string[] array = contract.ResourcePaths.Where((string path) => !ResourceExists(index, path) && !baselineIndexes.Any((PckResourceIndex baseline) => ResourceExists(baseline, path))).Take(4).ToArray();
			ModLog.Info($"未接受 {mod.Name ?? mod.Id} 的框架皮肤 {contract.SkinId}：" + ((contract.ResourcePaths.Count == 0) ? "契约没有声明任何资源路径。" : ("这些资源在包内与基线中都不存在：" + string.Join("、", array) + ((contract.ResourcePaths.Count > array.Length) ? " 等" : "。"))));
		}
		catch
		{
		}
		return false;
	}

	private static bool FrameworkContractResourceClosureComplete(PckResourceIndex index, IReadOnlyCollection<PckResourceIndex> baselineIndexes, FrameworkCharacterSkinContract contract)
	{
		if (contract.ResourcePaths.Count > 0)
		{
			return contract.ResourcePaths.All((string path) => ResourceExists(index, path) || baselineIndexes.Any((PckResourceIndex baseline) => ResourceExists(baseline, path)));
		}
		return false;
	}

	private static bool ResourceExists(PckResourceIndex index, string path)
	{
		if (!index.Archive.Paths.Contains<string>(path, StringComparer.OrdinalIgnoreCase) && !index.Archive.Paths.Contains<string>(path + ".remap", StringComparer.OrdinalIgnoreCase))
		{
			return index.Archive.Paths.Contains<string>(path + ".import", StringComparer.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool IsGameCharacterGroup(string groupId, IReadOnlyCollection<PckResourceIndex> baselineIndexes)
	{
		if (!string.IsNullOrWhiteSpace(groupId) && baselineIndexes.Count > 0)
		{
			return new string[3] { "CombatVisual", "CharacterSelectPortrait", "CharacterIcon" }.Select((string propertyName) => GetFrameworkCharacterCanonicalPath(propertyName, groupId)).OfType<string>().Any((string path) => baselineIndexes.Any((PckResourceIndex baseline) => ResourceExists(baseline, path)));
		}
		return false;
	}

	internal static string? GetFrameworkCharacterCanonicalPath(string propertyName, string targetGroupId)
	{
		return propertyName switch
		{
			"CombatVisual" => "res://scenes/creature_visuals/" + targetGroupId + ".tscn", 
			"MerchantVisual" => "res://scenes/merchant/characters/" + targetGroupId + "_merchant.tscn", 
			"RestVisual" => "res://scenes/rest_site/characters/" + targetGroupId + "_rest_site.tscn", 
			"CharacterSelectBg" => "res://scenes/screens/char_select/char_select_bg_" + targetGroupId + ".tscn", 
			"CharacterSelectPortrait" => "res://images/packed/character_select/char_select_" + targetGroupId + ".png", 
			"CharacterSelectTransition" => "res://materials/transitions/" + targetGroupId + "_transition_mat.tres", 
			"CharacterIcon" => "res://images/ui/top_panel/character_icon_" + targetGroupId + ".png", 
			"CharacterIconOutline" => "res://images/ui/top_panel/character_icon_" + targetGroupId + "_outline.png", 
			"CharacterIconScene" => "res://scenes/ui/character_icons/" + targetGroupId + "_icon.tscn", 
			"CharacterMapMarker" => "res://images/packed/map/icons/map_marker_" + targetGroupId + ".png", 
			"CardTrail" => "res://scenes/vfx/card_trail_" + targetGroupId + ".tscn", 
			"HandPoint" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_point.png", 
			"HandRock" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_rock.png", 
			"HandPaper" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_paper.png", 
			"HandScissors" => "res://images/ui/hands/multiplayer_hand_" + targetGroupId + "_scissors.png", 
			_ => null, 
		};
	}

	private static IReadOnlySet<string> CollectManagedCharacterDependencyPaths(PckResourceIndex index, IEnumerable<string> roots)
	{
		HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Queue<ResourceAsset> queue = new Queue<ResourceAsset>();
		foreach (string root in roots)
		{
			ResourceAsset resourceAsset = index.Assets.GetValueOrDefault(root) ?? index.TryBuildAsset(root);
			if (resourceAsset != null && result.Add(resourceAsset.SourcePath))
			{
				queue.Enqueue(resourceAsset);
			}
		}
		ResourceAsset result2;
		while (queue.TryDequeue(out result2))
		{
			foreach (ResourceFile item in result2.Files.Where((ResourceFile file) => MayContainResourceReferences(file.Path)))
			{
				string @string = Encoding.UTF8.GetString(item.Read());
				foreach (Match item2 in EmbeddedResourcePathRegex().Matches(@string))
				{
					Include(index.Assets.GetValueOrDefault(item2.Value) ?? index.TryBuildAsset(item2.Value));
					foreach (ResourceAsset siblingAtlasTextureAsset in GetSiblingAtlasTextureAssets(index, item2.Value))
					{
						Include(siblingAtlasTextureAsset);
					}
				}
			}
		}
		return result;
		void Include(ResourceAsset? dependency)
		{
			if (dependency != null && result.Add(dependency.SourcePath))
			{
				queue.Enqueue(dependency);
			}
		}
	}

	private static HashSet<string>? ReadEnabledRuntimeGroupIds(SkinModDescriptor mod)
	{
		if (mod.RootPath == null || !Directory.Exists(mod.RootPath))
		{
			return null;
		}
		foreach (string item in Directory.EnumerateFiles(mod.RootPath, "*_config.cfg", SearchOption.TopDirectoryOnly))
		{
			try
			{
				using JsonDocument jsonDocument = JsonDocument.Parse(File.ReadAllText(item));
				JsonProperty jsonProperty = jsonDocument.RootElement.EnumerateObject().FirstOrDefault((JsonProperty property) => property.Name.Equals("template_replacements", StringComparison.OrdinalIgnoreCase));
				if (jsonProperty.Value.ValueKind != JsonValueKind.Object)
				{
					continue;
				}
				return (from property in jsonProperty.Value.EnumerateObject()
					where property.Value.ValueKind == JsonValueKind.True
					select property.Name.ToLowerInvariant()).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
			}
		}
		return null;
	}

	private void AddImageRuntimeProviderOptions(IEnumerable<SkinModDescriptor> mods)
	{
		foreach (SkinModDescriptor item in mods.Where((SkinModDescriptor mod) => !mod.AffectsGameplay && mod.RootPath != null))
		{
			foreach (IGrouping<string, RuntimeAncientImage> item2 in DiscoverRuntimeAncientImages(item).GroupBy<RuntimeAncientImage, string>((RuntimeAncientImage image) => image.GroupId, StringComparer.OrdinalIgnoreCase))
			{
				RuntimeAncientImage[] array = item2.OrderByDescending((RuntimeAncientImage image) => image.Name.Equals("default", StringComparison.OrdinalIgnoreCase)).ThenBy<RuntimeAncientImage, string>((RuntimeAncientImage image) => image.Name, StringComparer.CurrentCultureIgnoreCase).ThenBy<RuntimeAncientImage, string>((RuntimeAncientImage image) => image.RelativePath, StringComparer.OrdinalIgnoreCase)
					.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					RuntimeAncientImage runtimeAncientImage = array[i];
					string optionId = ((i == 0) ? item.Id : (item.Id + ":image:" + runtimeAncientImage.RelativePath.ToLowerInvariant()));
					string optionName = ((array.Length == 1) ? item.Name : (item.Name + " · " + runtimeAncientImage.Name.Replace('_', ' ')));
					AddRuntimeProviderOption(runtimeAncientImage.GroupId, optionId, optionName, runtimeAncientImage.Path, item.Id);
				}
			}
		}
	}

	private static IReadOnlyList<RuntimeAncientImage> DiscoverRuntimeAncientImages(SkinModDescriptor mod)
	{
		if (mod.RootPath == null || !Directory.Exists(mod.RootPath))
		{
			return Array.Empty<RuntimeAncientImage>();
		}
		try
		{
			string fullPath = Path.GetFullPath(mod.RootPath);
			DateTime lastWriteTimeUtc = Directory.GetLastWriteTimeUtc(fullPath);
			lock (RuntimeAncientImageCacheSync)
			{
				if (RuntimeAncientImageCache.TryGetValue(fullPath, out RuntimeAncientImageCacheEntry value) && value.RootWriteTimeUtc == lastWriteTimeUtc)
				{
					return value.Images;
				}
			}
			List<RuntimeAncientImage> list = new List<RuntimeAncientImage>();
			foreach (string item in Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories))
			{
				string extension = Path.GetExtension(item);
				if ((extension.Equals(".png", StringComparison.OrdinalIgnoreCase) || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase) || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)) && !IsSpineAtlasTexture(item))
				{
					string text = Path.GetRelativePath(fullPath, item).Replace('\\', '/');
					string[] source = text.Split('/', StringSplitOptions.RemoveEmptyEntries);
					string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
					string text2 = source.Reverse().Skip(1).FirstOrDefault((string segment) => KnownAncientIds.Contains(segment));
					if (text2 == null && KnownAncientIds.Contains(fileNameWithoutExtension))
					{
						text2 = fileNameWithoutExtension;
					}
					if (text2 != null && !fileNameWithoutExtension.Equals("vanilla", StringComparison.OrdinalIgnoreCase))
					{
						list.Add(new RuntimeAncientImage(text2.ToLowerInvariant(), item, text, fileNameWithoutExtension));
					}
				}
			}
			RuntimeAncientImage[] array = list.DistinctBy<RuntimeAncientImage, string>((RuntimeAncientImage image) => image.Path, StringComparer.OrdinalIgnoreCase).ToArray();
			lock (RuntimeAncientImageCacheSync)
			{
				RuntimeAncientImageCache[fullPath] = new RuntimeAncientImageCacheEntry(lastWriteTimeUtc, array);
			}
			return array;
		}
		catch (Exception)
		{
			return Array.Empty<RuntimeAncientImage>();
		}
	}

	private static bool IsSpineAtlasTexture(string imagePath)
	{
		string stemPath = Path.Combine(Path.GetDirectoryName(imagePath) ?? string.Empty, Path.GetFileNameWithoutExtension(imagePath));
		if (!File.Exists(stemPath + ".atlas"))
		{
			return false;
		}
		return new string[3] { ".spjson", ".skel", ".json" }.Any((string extension) => File.Exists(stemPath + extension));
	}

	private static string DistinguishDuplicateProviderOptionName(SkinModDescriptor mod, string optionName)
	{
		if (mod.Id.Equals(mod.ResourceNamespaceId, StringComparison.OrdinalIgnoreCase))
		{
			return optionName;
		}
		int num = mod.Name.LastIndexOf(" · ", StringComparison.Ordinal);
		string text;
		if (num < 0)
		{
			text = " · " + mod.Name;
		}
		else
		{
			string name = mod.Name;
			int num2 = num;
			text = name.Substring(num2, name.Length - num2);
		}
		string text2 = text;
		if (!optionName.EndsWith(text2, StringComparison.OrdinalIgnoreCase))
		{
			return optionName + text2;
		}
		return optionName;
	}

	private void AddRuntimeProviderOption(string groupId, string optionId, string optionName, string runtimeImagePath, string providerId)
	{
		SkinGroup skinGroup = _groups.FirstOrDefault((SkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null)
		{
			skinGroup = new SkinGroup(groupId, DisplayName(groupId));
			_groups.Add(skinGroup);
		}
		int num = skinGroup.Options.FindIndex((SkinOption option) => option.Id.Equals(optionId, StringComparison.OrdinalIgnoreCase));
		if (num >= 0)
		{
			SkinOption skinOption = skinGroup.Options[num];
			skinGroup.Options[num] = skinOption with
			{
				IsRuntimeProvider = true,
				RuntimeImagePath = runtimeImagePath,
				ProviderId = providerId
			};
		}
		else
		{
			skinGroup.Options.Add(new SkinOption(optionId, optionName, new Dictionary<string, ResourceAsset>(StringComparer.OrdinalIgnoreCase), IsRuntimeProvider: true, IsDirectCharacterRuntimeProvider: false, runtimeImagePath, null, null, providerId));
		}
	}

	private static RuntimeProviderAsset? TryGetRuntimeProviderAsset(string providerId, string sourcePath)
	{
		string text = CanonicalizeRuntimeProviderPath(providerId, sourcePath);
		GroupIdentity groupIdentity = TryGetRuntimeProviderGroup(sourcePath) ?? TryGetPrimaryGroup(text) ?? TryGetCharacterSelectIconGroup(text) ?? TryGetCharacterUiTextureGroup(text) ?? TryGetCharacterMapMarkerGroup(text) ?? TryGetCharacterIconSceneGroup(text) ?? TryGetCharacterSupplementGroup(text);
		if (groupIdentity == null || !IsGroupAnchorResource(text, groupIdentity))
		{
			return null;
		}
		return new RuntimeProviderAsset(groupIdentity, text);
	}

	private static string CanonicalizeRuntimeProviderPath(string providerId, string sourcePath)
	{
		string text;
		if (sourcePath.StartsWith("res://custom/", StringComparison.OrdinalIgnoreCase))
		{
			text = sourcePath;
			return NormalizeRuntimeProviderCanonicalPath("res://" + text.Substring(13, text.Length - 13));
		}
		if (!IsProviderNamespacePath(sourcePath, NormalizeResourceToken(providerId)))
		{
			return NormalizeRuntimeProviderCanonicalPath(sourcePath);
		}
		text = sourcePath;
		string text2 = text.Substring(6, text.Length - 6);
		int num = text2.IndexOf('/');
		if (num >= 0)
		{
			text = text2;
			int num2 = num + 1;
			return NormalizeRuntimeProviderCanonicalPath("res://" + text.Substring(num2, text.Length - num2));
		}
		return sourcePath;
	}

	private static string NormalizeRuntimeProviderCanonicalPath(string path)
	{
		Match match = AnyCharacterSelectSceneRegex().Match(path);
		if (match.Success)
		{
			return "res://scenes/screens/char_select/char_select_bg_" + match.Groups[1].Value + ".tscn";
		}
		if (TryMapCharacterTemplatePath(path, "res://scenes/creature_visuals/templates/", "_template.tscn", "res://scenes/creature_visuals/", ".tscn", out string canonicalPath))
		{
			return canonicalPath;
		}
		if (TryMapCharacterTemplatePath(path, "res://scenes/merchant/characters/templates/", "_merchant_template.tscn", "res://scenes/merchant/characters/", "_merchant.tscn", out string canonicalPath2))
		{
			return canonicalPath2;
		}
		if (TryMapCharacterTemplatePath(path, "res://scenes/rest_site/characters/templates/", "_rest_site_template.tscn", "res://scenes/rest_site/characters/", "_rest_site.tscn", out string canonicalPath3))
		{
			return canonicalPath3;
		}
		string text;
		int length;
		if (path.StartsWith("res://scenes/character_select/", StringComparison.OrdinalIgnoreCase))
		{
			text = path;
			length = "res://scenes/character_select/".Length;
			return "res://scenes/screens/char_select/" + text.Substring(length, text.Length - length);
		}
		if (!path.StartsWith("res://scenes/char_select/", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}
		text = path;
		length = "res://scenes/char_select/".Length;
		return "res://scenes/screens/char_select/" + text.Substring(length, text.Length - length);
	}

	private static bool TryMapCharacterTemplatePath(string path, string templatePrefix, string templateSuffix, string canonicalPrefix, string canonicalSuffix, out string canonicalPath)
	{
		canonicalPath = string.Empty;
		if (!path.StartsWith(templatePrefix, StringComparison.OrdinalIgnoreCase) || !path.EndsWith(templateSuffix, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		int length = templatePrefix.Length;
		int length2 = templateSuffix.Length;
		string text = path.Substring(length, path.Length - length2 - length);
		if (string.IsNullOrWhiteSpace(text) || text.Contains('/'))
		{
			return false;
		}
		canonicalPath = canonicalPrefix + text + canonicalSuffix;
		return true;
	}

	private static GroupIdentity? TryGetCharacterSupplementGroup(string sourcePath)
	{
		Regex[] array = new Regex[2]
		{
			CharacterIconTemplateRegex(),
			MultiplayerHandRegex()
		};
		for (int i = 0; i < array.Length; i++)
		{
			Match match = array[i].Match(sourcePath);
			if (match.Success)
			{
				string id = match.Groups[1].Value.ToLowerInvariant();
				return new GroupIdentity(id, DisplayName(id));
			}
		}
		return null;
	}

	private static string MapAssetFilePath(string targetSourcePath, string assetSourcePath, string filePath)
	{
		if (targetSourcePath.Equals(assetSourcePath, StringComparison.OrdinalIgnoreCase))
		{
			return filePath;
		}
		if (filePath.Equals(assetSourcePath, StringComparison.OrdinalIgnoreCase))
		{
			return targetSourcePath;
		}
		string[] array = new string[2] { ".import", ".remap" };
		foreach (string text in array)
		{
			if (filePath.Equals(assetSourcePath + text, StringComparison.OrdinalIgnoreCase))
			{
				return targetSourcePath + text;
			}
		}
		return filePath;
	}

	private void SortGroupsAndOptions()
	{
		foreach (SkinGroup group in _groups)
		{
			group.Options.Sort((SkinOption left, SkinOption right) => string.Compare(left.Name, right.Name, StringComparison.CurrentCultureIgnoreCase));
		}
		_groups.Sort(delegate(SkinGroup left, SkinGroup right)
		{
			int num = GroupSortOrder(left.Id).CompareTo(GroupSortOrder(right.Id));
			return (num == 0) ? string.Compare(left.DisplayName, right.DisplayName, StringComparison.CurrentCultureIgnoreCase) : num;
		});
	}

	private static void MergeCharacterSelectIconPacks(IReadOnlyCollection<PckResourceIndex> indexes, IReadOnlyDictionary<string, SkinGroup> groups)
	{
		foreach (PckResourceIndex index in indexes)
		{
			if (index.Assets.Keys.Any((string path) => TryGetPrimaryGroup(path) != null))
			{
				continue;
			}
			foreach (IGrouping<string, (ResourceAsset, GroupIdentity)> item in (from asset in index.Assets.Values
				select (Asset: asset, Group: TryGetCharacterSelectIconGroup(asset.SourcePath)) into pair
				where pair.Group != null
				select pair).GroupBy<(ResourceAsset, GroupIdentity), string>(((ResourceAsset Asset, GroupIdentity Group) pair) => pair.Group.Id, StringComparer.OrdinalIgnoreCase))
			{
				if (!groups.TryGetValue(item.Key, out SkinGroup value))
				{
					continue;
				}
				Dictionary<string, ResourceAsset> dictionary = item.ToDictionary<(ResourceAsset, GroupIdentity), string, ResourceAsset>(((ResourceAsset Asset, GroupIdentity Group) pair) => pair.Asset.SourcePath, ((ResourceAsset Asset, GroupIdentity Group) pair) => pair.Asset, StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < value.Options.Count; i++)
				{
					SkinOption skinOption = value.Options[i];
					Dictionary<string, ResourceAsset> dictionary2 = new Dictionary<string, ResourceAsset>(skinOption.Assets, StringComparer.OrdinalIgnoreCase);
					foreach (KeyValuePair<string, ResourceAsset> item2 in dictionary)
					{
						dictionary2.TryAdd(item2.Key, item2.Value);
					}
					value.Options[i] = skinOption with
					{
						Assets = dictionary2
					};
				}
			}
		}
	}

	private static bool TryBuildCompositionOption(SkinGroup group, CharacterSkinComposition composition, bool session, out SkinOption option)
	{
		option = null;
		Dictionary<string, SkinOption> rawOptions = group.Options.Where((SkinOption candidate) => !candidate.IsComposition).ToDictionary<SkinOption, string>((SkinOption candidate) => candidate.Id, StringComparer.OrdinalIgnoreCase);
		Dictionary<string, CharacterSkinCompositionSource<ResourceAsset>> availableSources = rawOptions.ToDictionary<KeyValuePair<string, SkinOption>, string, CharacterSkinCompositionSource<ResourceAsset>>((KeyValuePair<string, SkinOption> pair) => pair.Key, (KeyValuePair<string, SkinOption> pair) => new CharacterSkinCompositionSource<ResourceAsset>(pair.Key, pair.Value.IsRuntimeProvider, pair.Value.Assets), StringComparer.OrdinalIgnoreCase);
		ResolvedCharacterSkinComposition<ResourceAsset> resolvedCharacterSkinComposition = CharacterSkinCompositionPolicy.ResolveAssets(composition.SourceOptionIds, availableSources, NormalizeTakeoverPath);
		if (resolvedCharacterSkinComposition.SourceOptionIds.Count == 0)
		{
			return false;
		}
		SkinOption skinOption = ((resolvedCharacterSkinComposition.DynamicSourceId == null) ? null : rawOptions[resolvedCharacterSkinComposition.DynamicSourceId]);
		option = new SkinOption(composition.Id, composition.Name, resolvedCharacterSkinComposition.Assets, skinOption?.IsRuntimeProvider ?? false, skinOption?.IsDirectCharacterRuntimeProvider ?? false, skinOption?.RuntimeImagePath, skinOption?.ManagedMonsterScene, skinOption?.RuntimeMonsterVisualMode, skinOption?.EffectiveProviderId, skinOption?.IsManagedMonsterRuntimeProfile ?? false, skinOption?.FrameworkContract)
		{
			CompositionSourceOptionIds = resolvedCharacterSkinComposition.SourceOptionIds,
			CompositionSourceProviderIds = resolvedCharacterSkinComposition.SourceOptionIds.Select((string sourceId) => rawOptions[sourceId].EffectiveProviderId).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray(),
			IsSessionComposition = session,
			ManualCharacterVariant = skinOption?.ManualCharacterVariant
		};
		return true;
	}

	private static GroupIdentity? TryGetDefinedBaselineGroup(string sourcePath)
	{
		if (CharacterPathRegex().Match(sourcePath).Success && !sourcePath.StartsWith("res://animations/characters/", StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}
		return TryGetPrimaryGroup(sourcePath);
	}

	private static GroupIdentity? TryGetPrimaryGroup(string sourcePath)
	{
		Regex[] array = new Regex[4]
		{
			CharacterSelectIconRegex(),
			CharacterUiTextureRegex(),
			CharacterIconSceneRegex(),
			CharacterMapMarkerRegex()
		};
		for (int i = 0; i < array.Length; i++)
		{
			Match match = array[i].Match(sourcePath);
			if (match.Success)
			{
				return new GroupIdentity(match.Groups[1].Value.ToLowerInvariant(), DisplayName(match.Groups[1].Value));
			}
		}
		Match match2 = CharacterPathRegex().Match(sourcePath);
		if (match2.Success)
		{
			string text = match2.Groups[1].Value.ToLowerInvariant();
			string[] array2 = new string[3] { "_rest_site", "_merchant", "_character_select" };
			foreach (string text2 in array2)
			{
				if (text.EndsWith(text2, StringComparison.OrdinalIgnoreCase))
				{
					string text3 = text;
					int length = text2.Length;
					text = text3.Substring(0, text3.Length - length);
					break;
				}
			}
			return new GroupIdentity(text, DisplayName(text));
		}
		Match match3 = MonsterPathRegex().Match(sourcePath);
		if (match3.Success)
		{
			string id = match3.Groups[1].Value.ToLowerInvariant();
			return new GroupIdentity(id, DisplayName(id));
		}
		if (FakeMerchantAppearancePathRegex().IsMatch(NormalizeTakeoverPath(sourcePath)))
		{
			return new GroupIdentity("fake_merchant_monster", DisplayName("fake_merchant_monster"));
		}
		if (MerchantAppearancePathRegex().IsMatch(NormalizeTakeoverPath(sourcePath)))
		{
			return new GroupIdentity("merchant", DisplayName("merchant"));
		}
		array = new Regex[4]
		{
			CreatureVisualSceneRegex(),
			CharacterSelectSceneRegex(),
			MerchantCharacterSceneRegex(),
			RestSiteCharacterSceneRegex()
		};
		for (int i = 0; i < array.Length; i++)
		{
			Match match4 = array[i].Match(sourcePath);
			if (match4.Success)
			{
				string id2 = match4.Groups[1].Value.ToLowerInvariant();
				return new GroupIdentity(id2, DisplayName(id2));
			}
		}
		Match match5 = AncientBackgroundSceneRegex().Match(sourcePath);
		if (match5.Success)
		{
			string id3 = match5.Groups[1].Value.ToLowerInvariant();
			return new GroupIdentity(id3, DisplayName(id3));
		}
		Match match6 = AncientBackgroundAnimationRegex().Match(sourcePath);
		if (match6.Success)
		{
			string text4 = match6.Groups[1].Value.ToLowerInvariant();
			if (text4.EndsWith("_room", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text4;
				text4 = text3.Substring(0, text3.Length - 5);
			}
			if (KnownAncientIds.Contains(text4))
			{
				return new GroupIdentity(text4, DisplayName(text4));
			}
		}
		Match match7 = AncientStaticImageRegex().Match(NormalizeTakeoverPath(sourcePath));
		if (match7.Success)
		{
			string text5 = match7.Groups["id"].Value.ToLowerInvariant();
			if (text5.EndsWith("_placeholder", StringComparison.OrdinalIgnoreCase))
			{
				string text3 = text5;
				text5 = text3.Substring(0, text3.Length - 12);
			}
			if (KnownAncientIds.Contains(text5))
			{
				return new GroupIdentity(text5, DisplayName(text5));
			}
		}
		Match match8 = AncientLayerImageRegex().Match(NormalizeTakeoverPath(sourcePath));
		if (match8.Success)
		{
			string text6 = match8.Groups["id"].Value.ToLowerInvariant();
			if (KnownAncientIds.Contains(text6))
			{
				return new GroupIdentity(text6, DisplayName(text6));
			}
		}
		return null;
	}

	private static GroupIdentity? TryGetUnambiguousCharacterGroup(string sourcePath)
	{
		Match match = CharacterPathRegex().Match(sourcePath);
		if (match.Success)
		{
			string text = match.Groups[1].Value.ToLowerInvariant();
			string[] array = new string[3] { "_rest_site", "_merchant", "_character_select" };
			foreach (string text2 in array)
			{
				if (text.EndsWith(text2, StringComparison.OrdinalIgnoreCase))
				{
					string text3 = text;
					int length = text2.Length;
					text = text3.Substring(0, text3.Length - length);
					break;
				}
			}
			return new GroupIdentity(text, DisplayName(text));
		}
		Regex[] array2 = new Regex[3]
		{
			CharacterSelectSceneRegex(),
			MerchantCharacterSceneRegex(),
			RestSiteCharacterSceneRegex()
		};
		for (int i = 0; i < array2.Length; i++)
		{
			Match match2 = array2[i].Match(sourcePath);
			if (match2.Success)
			{
				string id = match2.Groups[1].Value.ToLowerInvariant();
				return new GroupIdentity(id, DisplayName(id));
			}
		}
		return null;
	}

	private static GroupIdentity? TryGetCharacterVisualAnchorGroup(string sourcePath, IReadOnlySet<string> knownCharacterGroupIds)
	{
		GroupIdentity groupIdentity = TryGetUnambiguousCharacterGroup(sourcePath);
		if (groupIdentity != null && knownCharacterGroupIds.Contains(groupIdentity.Id))
		{
			return groupIdentity;
		}
		Match match = CreatureVisualSceneRegex().Match(sourcePath);
		if (!match.Success)
		{
			return null;
		}
		string text = match.Groups[1].Value.ToLowerInvariant();
		if (!knownCharacterGroupIds.Contains(text))
		{
			return null;
		}
		return new GroupIdentity(text, DisplayName(text));
	}

	private static GroupIdentity? TryGetCharacterSelectIconGroup(string sourcePath)
	{
		Match match = CharacterSelectIconRegex().Match(sourcePath);
		if (!match.Success)
		{
			return null;
		}
		string id = match.Groups[1].Value.ToLowerInvariant();
		return new GroupIdentity(id, DisplayName(id));
	}

	private static GroupIdentity? TryGetCharacterUiTextureGroup(string sourcePath)
	{
		Match match = CharacterUiTextureRegex().Match(sourcePath);
		if (!match.Success)
		{
			return null;
		}
		string id = match.Groups[1].Value.ToLowerInvariant();
		return new GroupIdentity(id, DisplayName(id));
	}

	private static GroupIdentity? TryGetCharacterIconSceneGroup(string sourcePath)
	{
		Match match = CharacterIconSceneRegex().Match(sourcePath);
		if (!match.Success)
		{
			return null;
		}
		string id = match.Groups[1].Value.ToLowerInvariant();
		return new GroupIdentity(id, DisplayName(id));
	}

	private static GroupIdentity? TryGetCharacterMapMarkerGroup(string sourcePath)
	{
		Match match = CharacterMapMarkerRegex().Match(sourcePath);
		if (!match.Success)
		{
			return null;
		}
		string id = match.Groups[1].Value.ToLowerInvariant();
		return new GroupIdentity(id, DisplayName(id));
	}

	private static GroupIdentity? TryGetAncientIconGroup(string sourcePath)
	{
		Regex[] array = new Regex[2]
		{
			AncientMapIconRegex(),
			AncientRunHistoryIconRegex()
		};
		for (int i = 0; i < array.Length; i++)
		{
			Match match = array[i].Match(sourcePath);
			if (match.Success)
			{
				string text = match.Groups[1].Value.ToLowerInvariant();
				if (KnownAncientIds.Contains(text))
				{
					return new GroupIdentity(text, DisplayName(text));
				}
			}
		}
		return null;
	}

	private static GroupIdentity? TryGetRuntimeProviderGroup(string sourcePath)
	{
		Regex[] array = new Regex[7]
		{
			AnyCharacterSelectSceneRegex(),
			RuntimeCharacterSelectSceneRegex(),
			RuntimeCharacterSelectIconRegex(),
			RuntimeCreatureTemplateRegex(),
			RuntimeMerchantTemplateRegex(),
			RuntimeRestSiteTemplateRegex(),
			RuntimeCharacterIconTemplateRegex()
		};
		for (int i = 0; i < array.Length; i++)
		{
			Match match = array[i].Match(sourcePath);
			if (match.Success)
			{
				string id = match.Groups[1].Value.ToLowerInvariant();
				return new GroupIdentity(id, DisplayName(id));
			}
		}
		return null;
	}

	private static bool ContainsGroupToken(string path, string groupId)
	{
		return Regex.IsMatch(path, "(?:^|[/_.-])" + Regex.Escape(groupId) + "(?:[/_.-]|$)", RegexOptions.IgnoreCase);
	}

	private static bool IsAnimationRemap(string path)
	{
		if (path.StartsWith("res://animations/", StringComparison.OrdinalIgnoreCase))
		{
			if (!path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
			{
				return path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	private static ResourceFile? FindDirectFile(ResourceAsset? asset, string sourcePath)
	{
		return asset?.Files.FirstOrDefault((ResourceFile file) => NormalizeTakeoverPath(file.Path).Equals(sourcePath, StringComparison.OrdinalIgnoreCase) || NormalizeTakeoverPath(file.Path).Equals(asset.SourcePath, StringComparison.OrdinalIgnoreCase));
	}

	private static ResourceFile? FindRemapFile(ResourceAsset? asset, string sourcePath)
	{
		return asset?.Files.FirstOrDefault((ResourceFile file) => NormalizeTakeoverPath(file.Path).Equals(sourcePath + ".import", StringComparison.OrdinalIgnoreCase) || NormalizeTakeoverPath(file.Path).Equals(sourcePath + ".remap", StringComparison.OrdinalIgnoreCase) || NormalizeTakeoverPath(file.Path).Equals(asset.SourcePath + ".import", StringComparison.OrdinalIgnoreCase) || NormalizeTakeoverPath(file.Path).Equals(asset.SourcePath + ".remap", StringComparison.OrdinalIgnoreCase));
	}

	private static ResourceFile[] GetImportedPayloadFiles(ResourceAsset asset, string sourcePath)
	{
		return asset.Files.Where((ResourceFile file) => !file.Path.Equals(sourcePath, StringComparison.OrdinalIgnoreCase) && !file.Path.Equals(sourcePath + ".import", StringComparison.OrdinalIgnoreCase) && !file.Path.Equals(sourcePath + ".remap", StringComparison.OrdinalIgnoreCase) && IsImportedPayloadPath(file.Path)).ToArray();
	}

	private static bool IsImportedPayloadPath(string path)
	{
		if (!path.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".spskel", StringComparison.OrdinalIgnoreCase) && !path.EndsWith(".ctex", StringComparison.OrdinalIgnoreCase))
		{
			if (path.StartsWith("res://.godot/exported/", StringComparison.OrdinalIgnoreCase))
			{
				if (!path.EndsWith(".scn", StringComparison.OrdinalIgnoreCase))
				{
					return path.EndsWith(".res", StringComparison.OrdinalIgnoreCase);
				}
				return true;
			}
			return false;
		}
		return true;
	}

	internal static string NormalizeTakeoverPath(string path)
	{
		Match match = WaifuAssetsPathRegex().Match(path);
		if (!match.Success)
		{
			return path;
		}
		return "res://" + match.Groups[1].Value;
	}

	private static ResourceFile? MatchImportedFile(string targetPath, IReadOnlyList<ResourceFile> files)
	{
		ResourceFile resourceFile = files.FirstOrDefault((ResourceFile file) => file.Path.Equals(targetPath, StringComparison.OrdinalIgnoreCase));
		if (resourceFile != null)
		{
			return resourceFile;
		}
		string suffix = (targetPath.EndsWith(".spatlas", StringComparison.OrdinalIgnoreCase) ? ".spatlas" : (targetPath.EndsWith(".spskel", StringComparison.OrdinalIgnoreCase) ? ".spskel" : (targetPath.EndsWith(".ctex", StringComparison.OrdinalIgnoreCase) ? ".ctex" : Path.GetExtension(targetPath))));
		ResourceFile[] array = files.Where((ResourceFile file) => file.Path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)).ToArray();
		if (array.Length != 1)
		{
			return null;
		}
		return array[0];
	}

	private static byte[] RewriteTextResource(byte[] bytes, IReadOnlyDictionary<string, string> replacements, IReadOnlyDictionary<string, string>? extraReplacements, bool stripUids = true)
	{
		string text = Encoding.UTF8.GetString(bytes);
		foreach (KeyValuePair<string, string> item in from pair in replacements.Concat(extraReplacements ?? new Dictionary<string, string>())
			orderby pair.Key.Length descending
			select pair)
		{
			text = Regex.Replace(text, Regex.Escape(item.Key) + "(?=[\\x00\\\"'\\s,\\]\\[]|$)", item.Value.Replace("$", "$$"), RegexOptions.IgnoreCase);
		}
		if (stripUids)
		{
			text = UidLineRegex().Replace(text, string.Empty);
			text = UidAttributeRegex().Replace(text, string.Empty);
		}
		return Encoding.UTF8.GetBytes(text);
	}

	private static int GroupSortOrder(string id)
	{
		return id switch
		{
			"ironclad" => 0, 
			"silent" => 1, 
			"regent" => 2, 
			"necrobinder" => 3, 
			"defect" => 4, 
			"watcher" => 5, 
			"colorless" => 6, 
			"ancients" => 7, 
			"misc" => 8, 
			"neow" => 9, 
			"merchant" => 10, 
			_ => 100, 
		};
	}

	private static string DisplayName(string id)
	{
		return id switch
		{
			"ironclad" => "铁甲战士", 
			"silent" => "静默猎手", 
			"regent" => "储君", 
			"necrobinder" => "亡灵契约师", 
			"defect" => "故障机器人", 
			"watcher" => "观者", 
			"colorless" => "无色", 
			"ancients" => "先古", 
			"misc" => "其他", 
			"neow" => "涅奥", 
			"merchant" => "商人", 
			"fake_merchant_monster" => "商人？？？", 
			"byrdpip" => "异鸟宝宝", 
			"cultists" => "邪教徒", 
			_ => id.Replace('_', ' ').Trim().CapitalizeWords(), 
		};
	}

	[GeneratedRegex("^res://animations/(?:characters|character_select|merchant|rest_site)/([^/]+)/", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterPathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterPathRegex_6.Instance;
	}

	[GeneratedRegex("(?:^|/)card_portraits/([^/]+)/", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CardPortraitGroupRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CardPortraitGroupRegex_7.Instance;
	}

	[GeneratedRegex("/(?:card_portraits|[^/]*card[^/]*\\.sprites|cards?|card_art|cardart)/", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CardArtPathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CardArtPathRegex_8.Instance;
	}

	[GeneratedRegex("^res://animations/monsters/([^/]+)/", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex MonsterPathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MonsterPathRegex_9.Instance;
	}

	[GeneratedRegex("^(?:res://animations/backgrounds/merchant_room/|res://animations/customs/merchant/|res://scenes/rooms/merchant_button\\.tscn$|res://scenes/merchant/(?!characters/)[^/]+\\.tscn$)", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex MerchantAppearancePathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantAppearancePathRegex_10.Instance;
	}

	[GeneratedRegex("^(?:res://animations/backgrounds/fake_merchant_room/(?:top|hand)/.*|res://scenes/backgrounds/fake_merchant_event_encounter/.*|res://scenes/events/custom/fake_merchant(?:_button|_inventory)?\\.tscn)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex FakeMerchantAppearancePathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__FakeMerchantAppearancePathRegex_11.Instance;
	}

	[GeneratedRegex("^res://animations/backgrounds/(?:fake_)?merchant_room/hand/", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex MerchantHandPartPathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantHandPartPathRegex_12.Instance;
	}

	[GeneratedRegex("^res://scenes/creature_visuals/([^/.]+)\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CreatureVisualSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CreatureVisualSceneRegex_13.Instance;
	}

	[GeneratedRegex("^res://scenes/screens/char_select/char_select_bg_([^/.]+)\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterSelectSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterSelectSceneRegex_14.Instance;
	}

	[GeneratedRegex("(?:^|/)char_select_bg_([^/.]+)\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AnyCharacterSelectSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AnyCharacterSelectSceneRegex_15.Instance;
	}

	[GeneratedRegex("^res://scenes/merchant/characters/([^/.]+)_merchant\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex MerchantCharacterSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantCharacterSceneRegex_16.Instance;
	}

	[GeneratedRegex("^res://scenes/rest_site/characters/([^/.]+)_rest_site\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RestSiteCharacterSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RestSiteCharacterSceneRegex_17.Instance;
	}

	[GeneratedRegex("^res://scenes/events/background_scenes/([^/.]+)\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AncientBackgroundSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientBackgroundSceneRegex_18.Instance;
	}

	[GeneratedRegex("^res://animations/backgrounds/([^/]+)/", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AncientBackgroundAnimationRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientBackgroundAnimationRegex_19.Instance;
	}

	[GeneratedRegex("^res://images/ancients/(?<id>.+?)_(?<kind>character|character_sleeping|character_mask|background_cover)\\.(?:png|webp|jpe?g|svg)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AncientLayerImageRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientLayerImageRegex_20.Instance;
	}

	[GeneratedRegex("^res://images/(?:ancients/)?(?<id>[^/.]+?)(?:_placeholder)?\\.(?:png|webp|jpe?g|svg)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AncientStaticImageRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientStaticImageRegex_21.Instance;
	}

	[GeneratedRegex("^res://images/packed/character_select/char_select_([^/.]+?)(?:_locked)?\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterSelectIconRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterSelectIconRegex_22.Instance;
	}

	[GeneratedRegex("^res://images/ui/top_panel/character_icon_([^/.]+?)(?:_outline)?\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterUiTextureRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterUiTextureRegex_23.Instance;
	}

	[GeneratedRegex("^res://images/atlases/relic(?:_outline)?_atlas\\.sprites/[^/]+\\.tres$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RelicAtlasSpriteRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RelicAtlasSpriteRegex_24.Instance;
	}

	[GeneratedRegex("^res://images/atlases/relic(?:_outline)?_atlas\\.png$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RelicAtlasTextureRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RelicAtlasTextureRegex_25.Instance;
	}

	[GeneratedRegex("(?m)^\\s*(?<property>region|margin)\\s*=\\s*Rect2\\(\\s*(?<x>-?[0-9.]+)\\s*,\\s*(?<y>-?[0-9.]+)\\s*,\\s*(?<width>-?[0-9.]+)\\s*,\\s*(?<height>-?[0-9.]+)\\s*\\)", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AtlasTextureRectRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AtlasTextureRectRegex_26.Instance;
	}

	[GeneratedRegex("(?m)^\\s*filter_clip\\s*=\\s*true\\s*$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AtlasTextureFilterClipRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AtlasTextureFilterClipRegex_27.Instance;
	}

	[GeneratedRegex("^res://scenes/ui/character_icons/([^/.]+?)_icon\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterIconSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterIconSceneRegex_28.Instance;
	}

	[GeneratedRegex("^res://images/packed/map/icons/map_marker_([^/.]+)\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterMapMarkerRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterMapMarkerRegex_29.Instance;
	}

	[GeneratedRegex("^res://images/packed/map/ancients/ancient_node_([^/.]+?)(?:_outline)?\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AncientMapIconRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientMapIconRegex_30.Instance;
	}

	[GeneratedRegex("^res://images/ui/run_history/([^/.]+?)(?:_outline)?\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex AncientRunHistoryIconRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientRunHistoryIconRegex_31.Instance;
	}

	[GeneratedRegex("^res://custom/scenes/screens/char_select/char_select_bg_([^/.]+)\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RuntimeCharacterSelectSceneRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RuntimeCharacterSelectSceneRegex_32.Instance;
	}

	[GeneratedRegex("^res://custom/images/packed/character_select/char_select_([^/.]+?)(?:_locked)?\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RuntimeCharacterSelectIconRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RuntimeCharacterSelectIconRegex_33.Instance;
	}

	[GeneratedRegex("^res://scenes/creature_visuals/templates/([^/.]+?)_template\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RuntimeCreatureTemplateRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RuntimeCreatureTemplateRegex_34.Instance;
	}

	[GeneratedRegex("^res://scenes/merchant/characters/templates/([^/.]+?)_merchant_template\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RuntimeMerchantTemplateRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RuntimeMerchantTemplateRegex_35.Instance;
	}

	[GeneratedRegex("^res://scenes/rest_site/characters/templates/([^/.]+?)_rest_site_template\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RuntimeRestSiteTemplateRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RuntimeRestSiteTemplateRegex_36.Instance;
	}

	[GeneratedRegex("^res://custom/scenes/ui/character_icons/([^/.]+?)_icon\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex RuntimeCharacterIconTemplateRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RuntimeCharacterIconTemplateRegex_37.Instance;
	}

	[GeneratedRegex("^res://scenes/ui/character_icons/templates/([^/.]+?)_icon\\.tscn$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex CharacterIconTemplateRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__CharacterIconTemplateRegex_38.Instance;
	}

	[GeneratedRegex("^res://images/ui/hands/multiplayer_hand_([^/.]+?)_(?:paper|point|rock|scissors)\\.(?:png|tres)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex MultiplayerHandRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MultiplayerHandRegex_39.Instance;
	}

	[GeneratedRegex("^res://waifu_assets/[^/]+/(.+)$", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex WaifuAssetsPathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__WaifuAssetsPathRegex_40.Instance;
	}

	[GeneratedRegex("\"(res://[^\"]+)\"", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex ResourcePathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__ResourcePathRegex_41.Instance;
	}

	[GeneratedRegex("res://[^\\x00\\\"'\\r\\n\\t \\]\\[(){}<>]+?\\.(?:spatlas|spskel|ctex|tscn|tres|cs|gdc|gd|gdshader|scn|res|png|webp|jpe?g|svg|skel|atlas|json|ogg|wav|mp3)(?=[\\x00\\\"'\\r\\n\\t \\]\\[(){}<>]|$)", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex EmbeddedResourcePathRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__EmbeddedResourcePathRegex_42.Instance;
	}

	[GeneratedRegex("^uid=\"uid://[^\"]+\"\\r?\\n", RegexOptions.IgnoreCase | RegexOptions.Multiline)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex UidLineRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__UidLineRegex_43.Instance;
	}

	[GeneratedRegex("\\s+uid=\"uid://[^\"]+\"", RegexOptions.IgnoreCase)]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex UidAttributeRegex()
	{
		return _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__UidAttributeRegex_44.Instance;
	}

	private static EventTables ReadEventTables(PckArchive archive)
	{
		return EventTableCache.GetValue(archive, delegate(PckArchive pack)
		{
			EventTables eventTables = new EventTables();
			foreach (string item in pack.Paths.Where((string path) => path.Contains("/localization/", StringComparison.OrdinalIgnoreCase) && path.EndsWith("/events.json", StringComparison.OrdinalIgnoreCase)).Order<string>(StringComparer.Ordinal))
			{
				string key = item.Split('/')[^2];
				try
				{
					Dictionary<string, string> dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(Encoding.UTF8.GetString(pack.ReadFile(item)).TrimStart('\ufeff'));
					if (dictionary != null)
					{
						if (!eventTables.Languages.TryGetValue(key, out Dictionary<string, string> value))
						{
							value = (eventTables.Languages[key] = new Dictionary<string, string>(StringComparer.Ordinal));
						}
						foreach (KeyValuePair<string, string> item2 in dictionary)
						{
							value[item2.Key] = item2.Value;
						}
					}
				}
				catch (JsonException ex)
				{
					ModLog.Error($"事件语言表读取失败 {pack.Path}/{item}: {ex.Message}");
				}
			}
			return eventTables;
		});
	}

	private static HashSet<string> KnownEventIds(IEnumerable<PckResourceIndex> baselines)
	{
		HashSet<string> keys = baselines.SelectMany((PckResourceIndex index) => ReadEventTables(index.Archive).Languages.Values).SelectMany((Dictionary<string, string> table) => table.Keys).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		return (from id in keys.Where((string key) => key.EndsWith(".title", StringComparison.Ordinal) && key.Count((char c) => c == '.') == 1).Select(EventSkinPolicy.EventIdFromKey).OfType<string>()
			where !KnownAncientIds.Contains(id) && !keys.Contains(id + ".epithet")
			select id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static void AddEventSkinGroups(IReadOnlyList<PckResourceIndex> indexes, IReadOnlyList<PckResourceIndex> baselines, IDictionary<string, SkinGroup> groups)
	{
		HashSet<string> knownIds = KnownEventIds(baselines);
		foreach (string id in knownIds)
		{
			if (!groups.TryGetValue(id, out SkinGroup value))
			{
				continue;
			}
			for (int num = value.Options.Count - 1; num >= 0; num--)
			{
				SkinOption skinOption = value.Options[num];
				Dictionary<string, ResourceAsset> dictionary = skinOption.Assets.Where<KeyValuePair<string, ResourceAsset>>((KeyValuePair<string, ResourceAsset> pair) => EventSkinPolicy.FindResourceOwner(pair.Key, knownIds) != id).ToDictionary<KeyValuePair<string, ResourceAsset>, string, ResourceAsset>((KeyValuePair<string, ResourceAsset> pair) => pair.Key, (KeyValuePair<string, ResourceAsset> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
				if (dictionary.Count == 0 && !skinOption.IsRuntimeProvider)
				{
					value.Options.RemoveAt(num);
				}
				else
				{
					value.Options[num] = skinOption with
					{
						Assets = dictionary
					};
				}
			}
			if (value.Options.Count == 0)
			{
				groups.Remove(id);
			}
		}
		foreach (PckResourceIndex index in indexes)
		{
			foreach (string item in index.Archive.Paths.Where(EventSkinPolicy.CouldOwnResource))
			{
				string text;
				string text2;
				if (!item.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
				{
					if (!item.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
					{
						text = item;
					}
					else
					{
						text2 = item;
						text = text2.Substring(0, text2.Length - 6);
					}
				}
				else
				{
					text2 = item;
					text = text2.Substring(0, text2.Length - 7);
				}
				string text3 = text;
				bool flag = EventSkinPolicy.FindResourceOwner(text3, knownIds) != null;
				if (flag)
				{
					text2 = Path.GetExtension(text3).ToLowerInvariant();
					if (text2 != null)
					{
						int length = text2.Length;
						if (length != 4)
						{
							if (length == 5)
							{
								switch (text2[2])
								{
								case 'p':
									break;
								case 'e':
									goto IL_0356;
								case 's':
									goto IL_0366;
								case 'r':
									goto IL_0376;
								default:
									goto IL_0389;
								}
								if (text2 == ".jpeg")
								{
									goto IL_0384;
								}
							}
						}
						else
						{
							switch (text2[1])
							{
							case 'p':
								break;
							case 'j':
								goto IL_0306;
							case 'w':
								goto IL_0316;
							case 'o':
								goto IL_0326;
							case 'm':
								goto IL_0336;
							default:
								goto IL_0389;
							}
							if (text2 == ".png")
							{
								goto IL_0384;
							}
						}
					}
					goto IL_0389;
				}
				goto IL_0390;
				IL_0366:
				if (text2 == ".tscn")
				{
					goto IL_0384;
				}
				goto IL_0389;
				IL_038c:
				bool flag2;
				flag = flag2;
				goto IL_0390;
				IL_0336:
				if (text2 == ".mp3")
				{
					goto IL_0384;
				}
				goto IL_0389;
				IL_0326:
				if (text2 == ".ogg")
				{
					goto IL_0384;
				}
				goto IL_0389;
				IL_0384:
				flag2 = true;
				goto IL_038c;
				IL_0356:
				if (text2 == ".webp")
				{
					goto IL_0384;
				}
				goto IL_0389;
				IL_0316:
				if (text2 == ".wav")
				{
					goto IL_0384;
				}
				goto IL_0389;
				IL_0306:
				if (text2 == ".jpg")
				{
					goto IL_0384;
				}
				goto IL_0389;
				IL_0390:
				if (flag)
				{
					index.TryBuildAsset(text3);
				}
				continue;
				IL_0389:
				flag2 = false;
				goto IL_038c;
				IL_0376:
				if (text2 == ".tres")
				{
					goto IL_0384;
				}
				goto IL_0389;
			}
			Dictionary<string, IReadOnlyDictionary<string, ResourceAsset>> dictionary2 = (from asset in index.Assets.Values
				select (Asset: asset, Id: EventSkinPolicy.FindResourceOwner(asset.SourcePath, knownIds)) into pair
				where pair.Id != null
				select pair).GroupBy<(ResourceAsset, string), string>(((ResourceAsset Asset, string Id) pair) => pair.Id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, (ResourceAsset, string)>, string, IReadOnlyDictionary<string, ResourceAsset>>((IGrouping<string, (ResourceAsset Asset, string Id)> group) => group.Key, (IGrouping<string, (ResourceAsset Asset, string Id)> group) => group.ToDictionary<(ResourceAsset, string), string, ResourceAsset>(((ResourceAsset Asset, string Id) pair) => pair.Asset.SourcePath, ((ResourceAsset Asset, string Id) pair) => pair.Asset, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
			string text4;
			IEnumerable<string> second = ReadEventTables(index.Archive).Languages.SelectMany<KeyValuePair<string, Dictionary<string, string>>, string>((KeyValuePair<string, Dictionary<string, string>> table) => from entry in table.Value
				where !TryReadBaselineEventText(baselines, entry.Key, table.Key, out text4) || entry.Value != text4
				select entry.Key).Select(EventSkinPolicy.EventIdFromKey).OfType<string>()
				.Where(knownIds.Contains);
			foreach (string item2 in dictionary2.Keys.Concat(second).Distinct<string>(StringComparer.OrdinalIgnoreCase))
			{
				string text5 = EventSkinPolicy.GroupId(item2);
				if (!groups.TryGetValue(text5, out SkinGroup value2))
				{
					value2 = (groups[text5] = new SkinGroup(text5, DisplayName(item2)));
				}
				IReadOnlyDictionary<string, ResourceAsset> readOnlyDictionary = dictionary2.GetValueOrDefault(item2) ?? new Dictionary<string, ResourceAsset>();
				int num2 = value2.Options.FindIndex((SkinOption option) => option.Id == index.Mod.Id);
				if (num2 < 0)
				{
					value2.Options.Add(new SkinOption(index.Mod.Id, index.Mod.Name, readOnlyDictionary));
					continue;
				}
				SkinOption skinOption2 = value2.Options[num2];
				Dictionary<string, ResourceAsset> dictionary3 = new Dictionary<string, ResourceAsset>(skinOption2.Assets, StringComparer.OrdinalIgnoreCase);
				foreach (KeyValuePair<string, ResourceAsset> item3 in readOnlyDictionary)
				{
					dictionary3[item3.Key] = item3.Value;
				}
				value2.Options[num2] = skinOption2 with
				{
					Assets = dictionary3
				};
			}
		}
	}

	public bool TryResolveEventText(string key, string language, IReadOnlyDictionary<string, string> selections, out string text)
	{
		text = string.Empty;
		string text2 = EventSkinPolicy.EventIdFromKey(key);
		if (text2 == null)
		{
			return false;
		}
		string groupId = EventSkinPolicy.GroupId(text2);
		SkinGroup group2 = Groups.FirstOrDefault((SkinGroup group) => group.Id == groupId);
		if (group2 == null)
		{
			return false;
		}
		PckResourceIndex[] array = _cosmeticIndexes.Where((PckResourceIndex index) => group2.Options.Any((SkinOption option) => option.Id == index.Mod.Id)).ToArray();
		if (!array.Any((PckResourceIndex index) => ReadEventTables(index.Archive).Languages.Values.Any((Dictionary<string, string> table) => table.ContainsKey(key))))
		{
			return false;
		}
		string selection = selections.GetValueOrDefault(groupId, "__base__");
		PckResourceIndex pckResourceIndex = array.FirstOrDefault((PckResourceIndex index) => index.Mod.Id == selection);
		if (pckResourceIndex != null && TryReadEventText(ReadEventTables(pckResourceIndex.Archive), key, language, out text))
		{
			return true;
		}
		PckResourceIndex[] array2 = array;
		foreach (PckResourceIndex pckResourceIndex2 in array2)
		{
			if (pckResourceIndex2 != pckResourceIndex && TryReadEventText(ReadEventTables(pckResourceIndex2.Archive), key, language, out text))
			{
				return true;
			}
		}
		if (TryReadBaselineEventText(_baselineIndexes, key, language, out text))
		{
			return true;
		}
		return false;
	}

	private static bool IsFakeMerchantEventPresentationPath(string path)
	{
		if (!path.StartsWith("res://animations/backgrounds/fake_merchant_room/bottom/", StringComparison.OrdinalIgnoreCase))
		{
			return path.StartsWith("res://images/events/fake_merchant", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public string? FindFakeMerchantEventArtProvider()
	{
		string text = "res://animations/backgrounds/fake_merchant_room/bottom/shop_fake_merchant_bottom.png";
		foreach (PckResourceIndex cosmeticIndex in _cosmeticIndexes)
		{
			if (cosmeticIndex.Archive.Contains(text) || cosmeticIndex.Archive.Contains(text + ".import"))
			{
				return cosmeticIndex.Mod.Id;
			}
		}
		return null;
	}

	public IReadOnlyDictionary<string, ResourceFile> FakeMerchantEventPresentation()
	{
		return _fakeMerchantEventPresentation ?? (_fakeMerchantEventPresentation = ScanFakeMerchantEventPresentation());
	}

	public void AddFakeMerchantEventPresentation(IDictionary<string, ResourceFile> files)
	{
		if (files.Count == 0)
		{
			return;
		}
		foreach (KeyValuePair<string, ResourceFile> item in _fakeMerchantEventPresentation ?? (_fakeMerchantEventPresentation = ScanFakeMerchantEventPresentation()))
		{
			files[item.Key] = item.Value;
		}
	}

	private Dictionary<string, ResourceFile> ScanFakeMerchantEventPresentation()
	{
		Dictionary<string, ResourceFile> dictionary = new Dictionary<string, ResourceFile>(StringComparer.OrdinalIgnoreCase);
		foreach (PckResourceIndex cosmeticIndex in _cosmeticIndexes)
		{
			foreach (string path in cosmeticIndex.Archive.Paths)
			{
				if (!IsFakeMerchantEventPresentationPath(path))
				{
					continue;
				}
				string text;
				if (!path.EndsWith(".import", StringComparison.OrdinalIgnoreCase))
				{
					if (!path.EndsWith(".remap", StringComparison.OrdinalIgnoreCase))
					{
						text = path;
					}
					else
					{
						string text2 = path;
						text = text2.Substring(0, text2.Length - 6);
					}
				}
				else
				{
					string text2 = path;
					text = text2.Substring(0, text2.Length - 7);
				}
				string sourcePath = text;
				ResourceAsset resourceAsset = cosmeticIndex.TryBuildAsset(sourcePath);
				if (resourceAsset == null)
				{
					continue;
				}
				foreach (ResourceFile file in resourceAsset.Files)
				{
					dictionary[NormalizeTakeoverPath(file.Path)] = file;
				}
			}
		}
		if (dictionary.Count > 0)
		{
			ModLog.Info($"已接入假商人事件背景/立绘：{dictionary.Count} 个文件来自其他事件外观包；" + "商人本体仍由商人皮肤提供。");
		}
		return dictionary;
	}

	private static bool TryReadBaselineEventText(IReadOnlyList<PckResourceIndex> baselines, string key, string language, out string text)
	{
		foreach (string item in new string[2] { language, "eng" }.Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			for (int num = baselines.Count - 1; num >= 0; num--)
			{
				if (ReadEventTables(baselines[num].Archive).Languages.TryGetValue(item, out Dictionary<string, string> value) && value.TryGetValue(key, out text))
				{
					return true;
				}
			}
		}
		text = string.Empty;
		return false;
	}

	public bool HasEventResource(string groupId, string selection, string path)
	{
		SkinGroup? skinGroup = Groups.FirstOrDefault((SkinGroup group) => group.Id == groupId);
		if (skinGroup == null || skinGroup.Options.FirstOrDefault((SkinOption option) => option.Id == selection)?.Assets.ContainsKey(path) != true)
		{
			return _baselineIndexes.Any((PckResourceIndex index) => index.Archive.Contains(path) || index.Archive.Contains(path + ".import") || index.Archive.Contains(path + ".remap"));
		}
		return true;
	}

	private static bool TryReadEventText(EventTables tables, string key, string language, out string text)
	{
		text = string.Empty;
		if (!tables.Languages.TryGetValue(language, out Dictionary<string, string> value) || !value.TryGetValue(key, out text))
		{
			if (tables.Languages.TryGetValue("eng", out Dictionary<string, string> value2))
			{
				return value2.TryGetValue(key, out text);
			}
			return false;
		}
		return true;
	}

	public IReadOnlyList<string> GetMultiplayerSourceIds(string groupId, string optionId)
	{
		Dictionary<string, SkinOption> options = GetRawCharacterOptions(groupId).ToDictionary<SkinOption, string>((SkinOption option) => option.Id, StringComparer.OrdinalIgnoreCase);
		return (from id in GetCompositionSourceOptionIds(groupId, optionId).Where(options.ContainsKey)
			select GetMultiplayerSourceId(options[id])).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
	}

	public IReadOnlyList<string> ResolveMultiplayerSourceIds(string groupId, IEnumerable<string> sourceIds)
	{
		SkinOption[] source = (from option in GetRawCharacterOptions(groupId)
			where option.Id != "__base__"
			select option).ToArray();
		List<string> list = new List<string>();
		foreach (string sourceId in (from id in sourceIds
			where !string.IsNullOrWhiteSpace(id)
			select id.Trim()).Distinct<string>(StringComparer.OrdinalIgnoreCase))
		{
			SkinOption[] array = source.Where((SkinOption option) => GetMultiplayerSourceId(option).Equals(sourceId, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (array.Length == 0)
			{
				array = source.Where((SkinOption option) => option.Id.Equals(sourceId, StringComparison.OrdinalIgnoreCase)).ToArray();
			}
			if (array.Length == 1 && !list.Contains<string>(array[0].Id, StringComparer.OrdinalIgnoreCase))
			{
				list.Add(array[0].Id);
			}
		}
		return list;
	}

	private string GetMultiplayerSourceId(SkinOption option)
	{
		string provider = option.EffectiveProviderId;
		if (_workshopSourceIds == null || !_workshopSourceIds.TryGetValue(provider, out var value) || value == 0L)
		{
			return option.Id;
		}
		string text = (_providerInstanceIdentities?.FirstOrDefault((ProviderInstanceIdentity candidate) => candidate.InstanceId.Equals(provider, StringComparison.OrdinalIgnoreCase)))?.ManifestId ?? provider;
		return ProviderInstanceIdentityPolicy.ScopeOptionId(provider, text + "::source:" + value.ToString(CultureInfo.InvariantCulture), option.Id);
	}

	internal static bool SupportsWorkshopResourcePath(string path)
	{
		string path2;
		if (!path.EndsWith(".remap"))
		{
			path2 = path;
		}
		else
		{
			path2 = path.Substring(0, path.Length - 6);
		}
		string text = Path.GetExtension(path2).ToLowerInvariant();
		if (text != null)
		{
			int length = text.Length;
			if (length != 3)
			{
				if (length != 4)
				{
					if (length == 12 && text == ".gdextension")
					{
						goto IL_00fd;
					}
				}
				else
				{
					char c = text[1];
					if ((uint)c <= 103u)
					{
						if (c != 'd')
						{
							if (c == 'g' && text == ".gdc")
							{
								goto IL_00fd;
							}
						}
						else if (text == ".dll")
						{
							goto IL_00fd;
						}
					}
					else if (c != 'r')
					{
						if (c == 's' && text == ".scn")
						{
							goto IL_00fd;
						}
					}
					else if (text == ".res")
					{
						goto IL_00fd;
					}
				}
			}
			else
			{
				char c = text[1];
				if (c != 'c')
				{
					if (c == 'g' && text == ".gd")
					{
						goto IL_00fd;
					}
				}
				else if (text == ".cs")
				{
					goto IL_00fd;
				}
			}
		}
		bool flag = false;
		goto IL_0103;
		IL_0103:
		return !flag;
		IL_00fd:
		flag = true;
		goto IL_0103;
	}

	internal bool HasCompleteWorkshopResourceCoverage(string providerId)
	{
		PckResourceIndex[] array = _cosmeticIndexes.Where((PckResourceIndex index) => index.Mod.Id == providerId).ToArray();
		if (array.Length != 1)
		{
			return false;
		}
		PckResourceIndex index2 = array[0];
		ResourceAsset[] source = (from option in Groups.SelectMany((SkinGroup @group) => @group.Options)
			where option.EffectiveProviderId == providerId
			select option).SelectMany((SkinOption option) => option.Assets.Values).Concat((from option in CardGroups.SelectMany((CardSkinGroup @group) => @group.Options)
			where (option.ProviderId ?? option.Id) == providerId
			select option).SelectMany((CardSkinOption option) => option.Assets.Values)).ToArray();
		HashSet<string> ownedFiles = (from file in source.SelectMany((ResourceAsset asset) => asset.Files)
			where file.Archive == index2.Archive
			select file.Path).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		string[] source2 = index2.Archive.Paths.ToArray();
		if (ownedFiles.Count > 0 && source2.All(delegate(string path)
		{
			bool flag = ownedFiles.Contains(path);
			if (!flag)
			{
				bool flag2;
				switch (path)
				{
				case "res://project.binary":
				case "res://.godot/uid_cache.bin":
				case "res://.godot/global_script_class_cache.cfg":
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = flag2;
			}
			return flag;
		}) && source2.All(SupportsWorkshopResourcePath))
		{
			return source2.SelectMany((string path) => EnumerateDependencyPaths(new ResourceFile(index2.Archive, path))).All(Available);
		}
		return false;
		bool Available(string dependency)
		{
			if (!index2.Archive.Contains(dependency) && !index2.Archive.Contains(dependency + ".import") && !index2.Archive.Contains(dependency + ".remap"))
			{
				return _baselineIndexes.Any((PckResourceIndex b) => b.Archive.Contains(dependency) || b.Archive.Contains(dependency + ".import") || b.Archive.Contains(dependency + ".remap"));
			}
			return true;
		}
	}

	internal string WorkshopKind(string groupId)
	{
		bool flag;
		switch (groupId)
		{
		case "osty":
		case "byrdpip":
		case "paels_legion":
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			return "companion";
		}
		string result;
		if (IsCharacterAppearanceGroup(groupId))
		{
			result = "character";
		}
		else
		{
			string text;
			if (KnownAncientIds.Contains(groupId))
			{
				text = "ancient";
			}
			else
			{
				string text2;
				if (EventSkinPolicy.IsEventGroup(groupId))
				{
					text2 = "event";
				}
				else
				{
					bool flag2 = ((groupId == "merchant" || groupId == "fake_merchant_monster") ? true : false);
					text2 = (flag2 ? "merchant" : "monster");
				}
				text = text2;
			}
			result = text;
		}
		return result;
	}

	internal WorkshopCatalogItem[] ExportWorkshopCatalog()
	{
		Dictionary<string, ulong> workshopSourceIds = _workshopSourceIds;
		List<(ulong, WorkshopTarget)> list = new List<(ulong, WorkshopTarget)>();
		foreach (SkinGroup group in Groups)
		{
			foreach (SkinOption option in group.Options)
			{
				if (workshopSourceIds.TryGetValue(option.EffectiveProviderId, out var value))
				{
					list.Add((value, new WorkshopTarget(WorkshopKind(group.Id), group.Id)));
				}
			}
		}
		foreach (CardSkinGroup cardGroup in CardGroups)
		{
			foreach (CardSkinOption option2 in cardGroup.Options)
			{
				if (workshopSourceIds.TryGetValue(option2.ProviderId ?? option2.Id, out var value2))
				{
					list.Add((value2, new WorkshopTarget("cards", cardGroup.Id)));
				}
			}
		}
		return (from t in list
			group t by t.Id into g
			orderby g.Key
			select new WorkshopCatalogItem(g.Key, g.Select(((ulong Id, WorkshopTarget Target) t) => t.Target).Distinct().ToArray())).ToArray();
	}

	internal static ulong WorkshopSourceId(string? path)
	{
		string[] array = (path ?? "").Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i + 1 < array.Length; i++)
		{
			if ((array[i] == "2868840" || array[i] == "_workshop_formal_cache" || array[i] == "github") && ulong.TryParse(array[i + 1], out var result))
			{
				return result;
			}
		}
		return 0uL;
	}

	internal string? ProviderForSourceId(ulong sourceId)
	{
		return (from pair in _workshopSourceIds
			where pair.Value == sourceId
			select pair.Key).FirstOrDefault((string providerId) => Groups.SelectMany((SkinGroup group) => group.Options).Any((SkinOption option) => option.EffectiveProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase)) || CardGroups.SelectMany((CardSkinGroup group) => group.Options).Any((CardSkinOption option) => (option.ProviderId ?? option.Id).Equals(providerId, StringComparison.OrdinalIgnoreCase)));
	}
}

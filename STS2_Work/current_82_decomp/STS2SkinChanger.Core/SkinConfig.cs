using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal sealed class SkinConfig
{
	private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		WriteIndented = true,
		PropertyNameCaseInsensitive = true
	};

	public Dictionary<string, string> Selections { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public List<string> RandomCharacterSkinGroups { get; set; } = new List<string>();

	public Dictionary<string, List<string>> RandomCharacterSkinExclusions { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

	public List<SlotVisibilitySelection> SlotVisibilitySelections { get; set; } = new List<SlotVisibilitySelection>();

	public Dictionary<string, string> CharacterIconSelections { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public List<CharacterSkinComposition> CharacterSkinCompositions { get; set; } = new List<CharacterSkinComposition>();

	public List<CharacterSkinBundle> CharacterSkinBundles { get; set; } = new List<CharacterSkinBundle>();

	public Dictionary<string, string> ActiveCharacterSkinBundles { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public List<string> VisualProviderPriority { get; set; } = new List<string>();

	public int VisualSelectionDefaultsVersion { get; set; }

	public int BaselinePartTakeoverVersion { get; set; }

	public Dictionary<string, List<CardSkinPriorityEntry>> CardSkinPriorities { get; set; } = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase);

	public int CardPriorityDefaultsVersion { get; set; }

	public List<CardSkinPreset> CardSkinPresets { get; set; } = new List<CardSkinPreset>();

	public int CardPresetMigrationRepairVersion { get; set; }

	public List<CardSkinPreset> ArchivedLegacyCardSkinPresets { get; set; } = new List<CardSkinPreset>();

	public Dictionary<string, string> ActiveCardSkinPresets { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public string? ActiveCardSkinPreset { get; set; }

	public Dictionary<string, List<MonsterSkinPriorityEntry>> MonsterSkinPriorities { get; set; } = new Dictionary<string, List<MonsterSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase);

	public List<MonsterSkinPreset> MonsterSkinPresets { get; set; } = new List<MonsterSkinPreset>();

	public Dictionary<string, string> ActiveMonsterSkinPresets { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, List<string>> MonsterSkinCategoryGroups { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

	public List<string> EnabledMonsterSkinPriorityCategories { get; set; } = new List<string>();

	public List<string> MonsterGroupsFollowingCategory { get; set; } = new List<string>();

	public List<string> MonsterGroupsWithManualSelection { get; set; } = new List<string>();

	public int MonsterPriorityDefaultsVersion { get; set; }

	public EventSkinPrioritySettings EventSkinPriorities { get; set; } = new EventSkinPrioritySettings();

	public Dictionary<string, string> BackgroundSelections { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, Dictionary<string, float>> MonsterScales { get; set; } = new Dictionary<string, Dictionary<string, float>>(StringComparer.OrdinalIgnoreCase);

	public Dictionary<string, Dictionary<string, CharacterCombatTransform>> CharacterCombatTransforms { get; set; } = new Dictionary<string, Dictionary<string, CharacterCombatTransform>>(StringComparer.OrdinalIgnoreCase);

	public bool SuppressLoadOrderWarning { get; set; }

	public bool? LastKnownFirstInLoadOrder { get; set; }

	public bool? LastKnownBeforeAllSkinMods { get; set; }

	public List<string>? LastKnownSkinProviderOrder { get; set; }

	public List<string> RespectedLoadOrderProviders { get; set; } = new List<string>();

	public bool MultiplayerSkinSyncEnabled { get; set; } = true;

	public int MultiplayerSkinMode { get; set; } = -1;

	public bool LoadOtherPlayersCustomSkins { get; set; } = true;

	public EventSkinPrioritySettings AncientSkinPriorities { get; set; } = new EventSkinPrioritySettings();

	public bool ShowInRunAppearanceEntry { get; set; } = true;

	public bool ShowInRunBackgroundEntry { get; set; } = true;

	public bool CharacterSelectorTopRight { get; set; }

	public float? CharacterSkinSelectorX { get; set; }

	public float? CharacterSkinSelectorY { get; set; }

	public float? CharacterSkinMergeX { get; set; }

	public float? CharacterSkinMergeY { get; set; }

	public float? CharacterSkinBundleX { get; set; }

	public float? CharacterSkinBundleY { get; set; }

	public float? CharacterModelPreviewX { get; set; }

	public float? CharacterModelPreviewY { get; set; }

	public bool CharacterModelPreviewDocked { get; set; }

	public float? IndividualCardSkinSelectorX { get; set; }

	public float? IndividualCardSkinSelectorY { get; set; }

	internal SkinConfig CloneForBundleTransaction()
	{
		SkinConfig obj = (SkinConfig)MemberwiseClone();
		obj.Selections = new Dictionary<string, string>(Selections, StringComparer.OrdinalIgnoreCase);
		obj.RandomCharacterSkinGroups = RandomCharacterSkinGroups.ToList();
		obj.CopyRandomCharacterSkinExclusionsFrom(this);
		obj.SlotVisibilitySelections = SlotVisibilitySelections.Select((SlotVisibilitySelection state) => state with
		{
			SourceSlots = state.SourceSlots.ToArray()
		}).ToList();
		obj.VisualProviderPriority = VisualProviderPriority.ToList();
		obj.CharacterSkinBundles = CharacterSkinBundles.Select(CharacterSkinBundlePolicy.Clone).ToList();
		obj.ActiveCharacterSkinBundles = new Dictionary<string, string>(ActiveCharacterSkinBundles, StringComparer.OrdinalIgnoreCase);
		obj.CardSkinPriorities = CardSkinPriorities.ToDictionary<KeyValuePair<string, List<CardSkinPriorityEntry>>, string, List<CardSkinPriorityEntry>>((KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
		obj.ActiveCardSkinPresets = new Dictionary<string, string>(ActiveCardSkinPresets, StringComparer.OrdinalIgnoreCase);
		obj.CardSkinPresets = CardSkinPresets.Select((CardSkinPreset preset) => preset.Clone()).ToList();
		obj.ArchivedLegacyCardSkinPresets = ArchivedLegacyCardSkinPresets.Select((CardSkinPreset preset) => preset.Clone()).ToList();
		obj.MonsterSkinPresets = MonsterSkinPresets.Select((MonsterSkinPreset preset) => preset.Clone()).ToList();
		obj.MonsterSkinPriorities = MonsterSkinPriorities.ToDictionary<KeyValuePair<string, List<MonsterSkinPriorityEntry>>, string, List<MonsterSkinPriorityEntry>>((KeyValuePair<string, List<MonsterSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<MonsterSkinPriorityEntry>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
		obj.ActiveMonsterSkinPresets = new Dictionary<string, string>(ActiveMonsterSkinPresets, StringComparer.OrdinalIgnoreCase);
		obj.MonsterSkinCategoryGroups = MonsterSkinCategoryGroups.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> pair) => pair.Key, (KeyValuePair<string, List<string>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
		obj.EnabledMonsterSkinPriorityCategories = EnabledMonsterSkinPriorityCategories.ToList();
		obj.MonsterGroupsFollowingCategory = MonsterGroupsFollowingCategory.ToList();
		obj.MonsterGroupsWithManualSelection = MonsterGroupsWithManualSelection.ToList();
		obj.EventSkinPriorities = EventSkinPriorities.Clone();
		obj.AncientSkinPriorities = AncientSkinPriorities.Clone();
		return obj;
	}

	internal void CopyRandomCharacterSkinExclusionsFrom(SkinConfig source)
	{
		RandomCharacterSkinExclusions = source.RandomCharacterSkinExclusions.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> pair) => pair.Key, (KeyValuePair<string, List<string>> pair) => pair.Value.ToList(), StringComparer.OrdinalIgnoreCase);
	}

	public static SkinConfig Load(string path)
	{
		string path2 = path + ".bak";
		try
		{
			if (File.Exists(path))
			{
				return Deserialize(File.ReadAllText(path));
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("读取皮肤配置失败，将尝试备份：" + ex.Message);
		}
		try
		{
			if (File.Exists(path2))
			{
				SkinConfig result = Deserialize(File.ReadAllText(path2));
				ModLog.Warn("已从 skin_changer.json.bak 恢复皮肤选择设置。");
				return result;
			}
		}
		catch (Exception ex2)
		{
			ModLog.Warn("读取皮肤配置备份失败，将使用默认配置：" + ex2.Message);
		}
		return new SkinConfig();
	}

	public void Save(string path)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path));
		string text = path + ".tmp";
		using (FileStream fileStream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.None))
		{
			JsonSerializer.Serialize(fileStream, this, JsonOptions);
			fileStream.Flush(flushToDisk: true);
		}
		File.Move(text, path, overwrite: true);
		try
		{
			File.Copy(path, path + ".bak", overwrite: true);
		}
		catch (Exception ex)
		{
			ModLog.Warn("更新皮肤配置备份失败：" + ex.Message);
		}
	}

	private static SkinConfig Deserialize(string json)
	{
		SkinConfig config = JsonSerializer.Deserialize<SkinConfig>(json, JsonOptions) ?? new SkinConfig();
		SkinConfig skinConfig = config;
		if (skinConfig.EventSkinPriorities == null)
		{
			EventSkinPrioritySettings eventSkinPrioritySettings2 = (skinConfig.EventSkinPriorities = new EventSkinPrioritySettings());
		}
		config.EventSkinPriorities.Normalize();
		skinConfig = config;
		if (skinConfig.AncientSkinPriorities == null)
		{
			EventSkinPrioritySettings eventSkinPrioritySettings2 = (skinConfig.AncientSkinPriorities = new EventSkinPrioritySettings());
		}
		config.AncientSkinPriorities.Normalize((string _) => true);
		config.RandomCharacterSkinGroups = (from id in config.RandomCharacterSkinGroups ?? new List<string>()
			where !string.IsNullOrWhiteSpace(id)
			select id.Trim()).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		config.RandomCharacterSkinExclusions = (config.RandomCharacterSkinExclusions ?? new Dictionary<string, List<string>>()).Where<KeyValuePair<string, List<string>>>((KeyValuePair<string, List<string>> pair) => !string.IsNullOrWhiteSpace(pair.Key)).GroupBy<KeyValuePair<string, List<string>>, string>((KeyValuePair<string, List<string>> pair) => pair.Key.Trim(), StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, KeyValuePair<string, List<string>>>, string, List<string>>((IGrouping<string, KeyValuePair<string, List<string>>> group) => group.Key, (IGrouping<string, KeyValuePair<string, List<string>>> group) => (from id in @group.SelectMany((KeyValuePair<string, List<string>> pair) => pair.Value ?? new List<string>())
			where !string.IsNullOrWhiteSpace(id)
			select id.Trim()).Where(RandomCharacterSkinPolicy.IsCandidate).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.Selections == null)
		{
			Dictionary<string, string> dictionary2 = (skinConfig.Selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		config.Selections = new Dictionary<string, string>(config.Selections, StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.BackgroundSelections == null)
		{
			Dictionary<string, string> dictionary2 = (skinConfig.BackgroundSelections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		config.BackgroundSelections = config.BackgroundSelections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key.Trim().ToLowerInvariant(), (KeyValuePair<string, string> pair) => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.CharacterIconSelections == null)
		{
			Dictionary<string, string> dictionary2 = (skinConfig.CharacterIconSelections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		config.CharacterIconSelections = config.CharacterIconSelections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
		config.CharacterSkinCompositions = CharacterSkinCompositionPolicy.Normalize(config.CharacterSkinCompositions);
		config.SlotVisibilitySelections = SlotVisibilityPolicy.Normalize(config.SlotVisibilitySelections);
		config.CharacterSkinBundles = CharacterSkinBundlePolicy.Normalize(config.CharacterSkinBundles);
		skinConfig = config;
		if (skinConfig.ActiveCharacterSkinBundles == null)
		{
			Dictionary<string, string> dictionary2 = (skinConfig.ActiveCharacterSkinBundles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		config.ActiveCharacterSkinBundles = config.ActiveCharacterSkinBundles.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value) && config.CharacterSkinBundles.Any((CharacterSkinBundle bundle) => bundle.CharacterGroupId.Equals(pair.Key, StringComparison.OrdinalIgnoreCase) && bundle.Name.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key.Trim().ToLowerInvariant(), (KeyValuePair<string, string> pair) => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.VisualProviderPriority == null)
		{
			List<string> list2 = (skinConfig.VisualProviderPriority = new List<string>());
		}
		config.VisualProviderPriority = config.VisualProviderPriority.Where((string providerId) => !string.IsNullOrWhiteSpace(providerId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		skinConfig = config;
		if (skinConfig.CardSkinPriorities == null)
		{
			Dictionary<string, List<CardSkinPriorityEntry>> dictionary7 = (skinConfig.CardSkinPriorities = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase));
		}
		config.CardSkinPriorities = config.CardSkinPriorities.ToDictionary<KeyValuePair<string, List<CardSkinPriorityEntry>>, string, List<CardSkinPriorityEntry>>((KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => (pair.Value ?? new List<CardSkinPriorityEntry>()).Where((CardSkinPriorityEntry entry) => entry != null && !string.IsNullOrWhiteSpace(entry.OptionId)).ToList(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.CardSkinPresets == null)
		{
			List<CardSkinPreset> list4 = (skinConfig.CardSkinPresets = new List<CardSkinPreset>());
		}
		skinConfig = config;
		if (skinConfig.ArchivedLegacyCardSkinPresets == null)
		{
			List<CardSkinPreset> list4 = (skinConfig.ArchivedLegacyCardSkinPresets = new List<CardSkinPreset>());
		}
		config.CardSkinPresets = config.CardSkinPresets.Where((CardSkinPreset preset) => preset != null && !string.IsNullOrWhiteSpace(preset.Name)).Select(delegate(CardSkinPreset preset)
		{
			preset.Name = preset.Name.Trim();
			preset.CategoryId = (string.IsNullOrWhiteSpace(preset.CategoryId) ? null : preset.CategoryId.Trim().ToLowerInvariant());
			CardSkinPreset cardSkinPreset = preset;
			if (cardSkinPreset.CardSkinPriorities == null)
			{
				Dictionary<string, List<CardSkinPriorityEntry>> dictionary9 = (cardSkinPreset.CardSkinPriorities = new Dictionary<string, List<CardSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase));
			}
			preset.CardSkinPriorities = preset.CardSkinPriorities.ToDictionary<KeyValuePair<string, List<CardSkinPriorityEntry>>, string, List<CardSkinPriorityEntry>>((KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<CardSkinPriorityEntry>> pair) => (pair.Value ?? new List<CardSkinPriorityEntry>()).Where((CardSkinPriorityEntry entry) => entry != null && !string.IsNullOrWhiteSpace(entry.OptionId)).ToList(), StringComparer.OrdinalIgnoreCase);
			cardSkinPreset = preset;
			if (cardSkinPreset.Selections == null)
			{
				Dictionary<string, string> dictionary11 = (cardSkinPreset.Selections = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
			}
			preset.Selections = preset.Selections.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => pair.Key.StartsWith("cards:", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(pair.Value)).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value, StringComparer.OrdinalIgnoreCase);
			return preset;
		}).DistinctBy<CardSkinPreset, string>((CardSkinPreset preset) => (preset.CategoryId ?? string.Empty) + "\n" + preset.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();
		skinConfig = config;
		if (skinConfig.ActiveCardSkinPresets == null)
		{
			Dictionary<string, string> dictionary2 = (skinConfig.ActiveCardSkinPresets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		config.ActiveCardSkinPresets = config.ActiveCardSkinPresets.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value) && config.CardSkinPresets.Any(delegate(CardSkinPreset preset)
		{
			string? categoryId2 = preset.CategoryId;
			return categoryId2 != null && categoryId2.Equals(pair.Key, StringComparison.OrdinalIgnoreCase) && preset.Name.Equals(pair.Value, StringComparison.OrdinalIgnoreCase);
		})).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key.Trim().ToLowerInvariant(), (KeyValuePair<string, string> pair) => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
		config.ActiveCardSkinPreset = (config.CardSkinPresets.Any((CardSkinPreset preset) => preset.Name.Equals(config.ActiveCardSkinPreset, StringComparison.OrdinalIgnoreCase)) ? config.CardSkinPresets.First((CardSkinPreset preset) => preset.Name.Equals(config.ActiveCardSkinPreset, StringComparison.OrdinalIgnoreCase)).Name : null);
		skinConfig = config;
		if (skinConfig.MonsterSkinPriorities == null)
		{
			Dictionary<string, List<MonsterSkinPriorityEntry>> dictionary14 = (skinConfig.MonsterSkinPriorities = new Dictionary<string, List<MonsterSkinPriorityEntry>>(StringComparer.OrdinalIgnoreCase));
		}
		config.MonsterSkinPriorities = config.MonsterSkinPriorities.ToDictionary<KeyValuePair<string, List<MonsterSkinPriorityEntry>>, string, List<MonsterSkinPriorityEntry>>((KeyValuePair<string, List<MonsterSkinPriorityEntry>> pair) => pair.Key, (KeyValuePair<string, List<MonsterSkinPriorityEntry>> pair) => (pair.Value ?? new List<MonsterSkinPriorityEntry>()).Where((MonsterSkinPriorityEntry entry) => entry != null && !string.IsNullOrWhiteSpace(entry.OptionId)).ToList(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.MonsterSkinPresets == null)
		{
			List<MonsterSkinPreset> list7 = (skinConfig.MonsterSkinPresets = new List<MonsterSkinPreset>());
		}
		config.MonsterSkinPresets = config.MonsterSkinPresets.Where((MonsterSkinPreset preset) => preset != null && !string.IsNullOrWhiteSpace(preset.Name) && !string.IsNullOrWhiteSpace(preset.CategoryId)).Select(delegate(MonsterSkinPreset preset)
		{
			preset.Name = preset.Name.Trim();
			preset.CategoryId = preset.CategoryId.Trim().ToLowerInvariant();
			preset.Priority = (preset.Priority ?? new List<MonsterSkinPriorityEntry>()).Where((MonsterSkinPriorityEntry entry) => entry != null && !string.IsNullOrWhiteSpace(entry.OptionId)).DistinctBy<MonsterSkinPriorityEntry, string>((MonsterSkinPriorityEntry entry) => entry.OptionId, StringComparer.OrdinalIgnoreCase).ToList();
			preset.Selections = new Dictionary<string, string>((preset.Selections ?? new Dictionary<string, string>()).Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value)).ToDictionary((KeyValuePair<string, string> pair) => pair.Key, (KeyValuePair<string, string> pair) => pair.Value), StringComparer.OrdinalIgnoreCase);
			preset.FollowingGroupIds = (preset.FollowingGroupIds ?? new List<string>()).Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
			return preset;
		}).DistinctBy<MonsterSkinPreset, string>((MonsterSkinPreset preset) => preset.CategoryId + "\n" + preset.Name, StringComparer.OrdinalIgnoreCase)
			.ToList();
		skinConfig = config;
		if (skinConfig.ActiveMonsterSkinPresets == null)
		{
			Dictionary<string, string> dictionary2 = (skinConfig.ActiveMonsterSkinPresets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
		}
		config.ActiveMonsterSkinPresets = config.ActiveMonsterSkinPresets.Where<KeyValuePair<string, string>>((KeyValuePair<string, string> pair) => !string.IsNullOrWhiteSpace(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value) && config.MonsterSkinPresets.Any((MonsterSkinPreset preset) => preset.CategoryId.Equals(pair.Key, StringComparison.OrdinalIgnoreCase) && preset.Name.Equals(pair.Value, StringComparison.OrdinalIgnoreCase))).ToDictionary<KeyValuePair<string, string>, string, string>((KeyValuePair<string, string> pair) => pair.Key.Trim().ToLowerInvariant(), (KeyValuePair<string, string> pair) => pair.Value.Trim(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.MonsterSkinCategoryGroups == null)
		{
			Dictionary<string, List<string>> dictionary17 = (skinConfig.MonsterSkinCategoryGroups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase));
		}
		config.MonsterSkinCategoryGroups = config.MonsterSkinCategoryGroups.ToDictionary<KeyValuePair<string, List<string>>, string, List<string>>((KeyValuePair<string, List<string>> pair) => pair.Key, (KeyValuePair<string, List<string>> pair) => (pair.Value ?? new List<string>()).Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList(), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.EnabledMonsterSkinPriorityCategories == null)
		{
			List<string> list2 = (skinConfig.EnabledMonsterSkinPriorityCategories = new List<string>());
		}
		config.EnabledMonsterSkinPriorityCategories = config.EnabledMonsterSkinPriorityCategories.Where((string categoryId) => !string.IsNullOrWhiteSpace(categoryId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		skinConfig = config;
		if (skinConfig.MonsterGroupsFollowingCategory == null)
		{
			List<string> list2 = (skinConfig.MonsterGroupsFollowingCategory = new List<string>());
		}
		config.MonsterGroupsFollowingCategory = config.MonsterGroupsFollowingCategory.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		skinConfig = config;
		if (skinConfig.MonsterGroupsWithManualSelection == null)
		{
			List<string> list2 = (skinConfig.MonsterGroupsWithManualSelection = new List<string>());
		}
		config.MonsterGroupsWithManualSelection = config.MonsterGroupsWithManualSelection.Where((string groupId) => !string.IsNullOrWhiteSpace(groupId)).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		skinConfig = config;
		if (skinConfig.MonsterScales == null)
		{
			Dictionary<string, Dictionary<string, float>> dictionary19 = (skinConfig.MonsterScales = new Dictionary<string, Dictionary<string, float>>(StringComparer.OrdinalIgnoreCase));
		}
		config.MonsterScales = config.MonsterScales.ToDictionary<KeyValuePair<string, Dictionary<string, float>>, string, Dictionary<string, float>>((KeyValuePair<string, Dictionary<string, float>> pair) => pair.Key, (KeyValuePair<string, Dictionary<string, float>> pair) => new Dictionary<string, float>(pair.Value ?? new Dictionary<string, float>(), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
		skinConfig = config;
		if (skinConfig.CharacterCombatTransforms == null)
		{
			Dictionary<string, Dictionary<string, CharacterCombatTransform>> dictionary21 = (skinConfig.CharacterCombatTransforms = new Dictionary<string, Dictionary<string, CharacterCombatTransform>>(StringComparer.OrdinalIgnoreCase));
		}
		config.CharacterCombatTransforms = config.CharacterCombatTransforms.ToDictionary<KeyValuePair<string, Dictionary<string, CharacterCombatTransform>>, string, Dictionary<string, CharacterCombatTransform>>((KeyValuePair<string, Dictionary<string, CharacterCombatTransform>> pair) => pair.Key, (KeyValuePair<string, Dictionary<string, CharacterCombatTransform>> pair) => new Dictionary<string, CharacterCombatTransform>((pair.Value ?? new Dictionary<string, CharacterCombatTransform>()).Where((KeyValuePair<string, CharacterCombatTransform> option) => option.Value != null).ToDictionary((KeyValuePair<string, CharacterCombatTransform> option) => option.Key, (KeyValuePair<string, CharacterCombatTransform> option) => option.Value), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
		return config;
	}

	public string GetSelection(string groupId)
	{
		return Selections.GetValueOrDefault(groupId, "__base__");
	}
}

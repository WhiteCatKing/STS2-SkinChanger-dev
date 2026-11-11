using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CznCore.Settings;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace CznCore.Presentation;

internal static class CharacterAppearanceCatalog
{
	private sealed record Entry(CharacterSkinProfile Base, Type Target, Func<string> Name, Dictionary<string, CharacterSkinProfile> Resolved, Dictionary<string, Func<string>> Labels);

	private sealed record PlayerBinding(CharacterAppearanceChoice Choice, string? RunKey);

	internal const string DefaultVariantId = "default";

	private static readonly SettingsLocalizationStore Text = new SettingsLocalizationStore("res://CznCore/localization/{0}/appearance.json");

	private static readonly object Sync = new object();

	private static readonly Dictionary<string, Entry> Entries = new Dictionary<string, Entry>(StringComparer.Ordinal);

	private static readonly ConditionalWeakTable<Player, PlayerBinding> Players = new ConditionalWeakTable<Player, PlayerBinding>();

	private static readonly Dictionary<Type, CharacterAppearanceChoice?> ChoiceCache = new Dictionary<Type, CharacterAppearanceChoice>();

	private static Dictionary<string, CharacterAppearanceChoice> Selected = new Dictionary<string, CharacterAppearanceChoice>(StringComparer.Ordinal);

	private static Dictionary<string, CharacterAppearanceChoice> RunChoices = new Dictionary<string, CharacterAppearanceChoice>(StringComparer.Ordinal);

	private static readonly List<string> RunOrder = new List<string>();

	private static readonly Dictionary<string, CharacterAppearanceChoice> ActiveRunChoices = new Dictionary<string, CharacterAppearanceChoice>(StringComparer.Ordinal);

	private static CharacterAppearanceStore? _store;

	private static string? _runKey;

	internal static void ConfigureStorage(string path, string? legacyPath = null)
	{
		lock (Sync)
		{
			if (_store == null)
			{
				if (legacyPath != null)
				{
					CharacterAppearanceStore.MigrateLegacy(legacyPath, path);
				}
				_store = new CharacterAppearanceStore(path);
				Selected = _store.LoadPreferences();
				RunChoices = _store.LoadRuns();
				RunOrder.Clear();
				RunOrder.AddRange(RunChoices.Keys.Select(CharacterAppearanceStore.RunOf).Distinct());
				ChoiceCache.Clear();
			}
		}
	}

	internal static void Register(CharacterSkinProfile profile)
	{
		lock (Sync)
		{
			Type type = profile.ResolveTargetCharacterType() ?? throw new InvalidOperationException("Appearance target unavailable");
			foreach (Entry value in Entries.Values)
			{
				if (value.Base.ProfileId == profile.ProfileId || value.Base.RuntimeNodeNamePrefix == profile.RuntimeNodeNamePrefix)
				{
					throw new InvalidOperationException("Duplicate appearance identity: " + profile.ProfileId);
				}
				if ((type.IsAssignableFrom(value.Target) || value.Target.IsAssignableFrom(type)) && (type != value.Target || profile.Appearance == null || value.Base.Appearance == null))
				{
					throw new InvalidOperationException("Overlapping legacy skin requires migration: " + profile.ProfileId);
				}
			}
			Dictionary<string, CharacterSkinProfile> dictionary = new Dictionary<string, CharacterSkinProfile>(StringComparer.Ordinal) { ["default"] = profile };
			Dictionary<string, Func<string>> dictionary2 = new Dictionary<string, Func<string>>(StringComparer.Ordinal) { ["default"] = () => Text.Get("default_outfit", "Default Outfit") };
			CharacterAppearanceDefinition appearance = profile.Appearance;
			if ((object)appearance != null)
			{
				if (string.IsNullOrWhiteSpace(appearance.ProviderId))
				{
					throw new ArgumentException("Provider ID required");
				}
				foreach (CharacterAppearanceVariant variant in appearance.Variants)
				{
					if (string.IsNullOrWhiteSpace(variant.Id) || variant.Id == "default")
					{
						throw new ArgumentException("Invalid variant ID");
					}
					CharacterSkinProfile characterSkinProfile = variant.Resolve(profile);
					if (characterSkinProfile.ProfileId != profile.ProfileId || characterSkinProfile.ResolveTargetCharacterType() != type || characterSkinProfile.RuntimeNodeNamePrefix != profile.RuntimeNodeNamePrefix)
					{
						throw new ArgumentException("A variant cannot change character identity");
					}
					if (!WithAppearanceAwareFieldsOf(characterSkinProfile, profile).Equals(profile))
					{
						throw new ArgumentException("Variant '" + variant.Id + "' of " + profile.ProfileId + " overrides a surface that is not appearance-aware; resolve it per player first");
					}
					dictionary.Add(variant.Id, characterSkinProfile);
					dictionary2.Add(variant.Id, () => Localized(variant.LocalizedDisplayName, variant.DisplayName));
				}
			}
			CharacterAppearanceDefinition definitionName = profile.Appearance;
			Entries.Add(profile.ProfileId, new Entry(profile, type, () => Localized(definitionName?.LocalizedDisplayName, definitionName?.DisplayName ?? profile.ProfileId), dictionary, dictionary2));
			ChoiceCache.Clear();
		}
	}

	private static CharacterSkinProfile WithAppearanceAwareFieldsOf(CharacterSkinProfile variant, CharacterSkinProfile source)
	{
		return variant with
		{
			BodyTexturePath = source.BodyTexturePath,
			BodySkeletonDataPath = source.BodySkeletonDataPath,
			CombatEffectOverrides = source.CombatEffectOverrides,
			RestSite = source.RestSite,
			CharacterSelectPresentation = source.CharacterSelectPresentation
		};
	}

	internal static void Unregister(string id)
	{
		lock (Sync)
		{
			Entries.Remove(id);
			ChoiceCache.Clear();
		}
	}

	internal static string UiText(string key, string fallback)
	{
		return Text.Get(key, fallback);
	}

	private static string Localized(ILocalizedText? text, string fallback)
	{
		string text2 = text?.Current;
		if (text2 == null || text2.Length <= 0 || string.IsNullOrWhiteSpace(text2))
		{
			return fallback;
		}
		return text2;
	}

	private static Entry[] Candidates(Type type)
	{
		return Entries.Values.Where((Entry e) => e.Target.IsAssignableFrom(type)).OrderBy<Entry, string>((Entry e) => e.Base.ProfileId, StringComparer.Ordinal).ToArray();
	}

	private static string Slot(Type type)
	{
		Type type2 = Candidates(type).FirstOrDefault()?.Target ?? type;
		return type2.FullName ?? type2.Name;
	}

	private static CharacterAppearanceChoice? Valid(CharacterAppearanceChoice? choice, Type type)
	{
		if (choice == null || !Entries.TryGetValue(choice.ProfileId, out Entry value) || !value.Target.IsAssignableFrom(type))
		{
			return null;
		}
		if (!value.Resolved.ContainsKey(choice.VariantId))
		{
			return choice with
			{
				VariantId = "default"
			};
		}
		return choice;
	}

	private static CharacterAppearanceChoice? Choice(Type type)
	{
		if (ChoiceCache.TryGetValue(type, out CharacterAppearanceChoice value))
		{
			return value;
		}
		return ChoiceCache[type] = ResolveChoice(type);
	}

	private static CharacterAppearanceChoice? ResolveChoice(Type type)
	{
		Entry[] array = Candidates(type);
		if (array.Length == 0)
		{
			return null;
		}
		string key = Slot(type);
		if (_runKey != null)
		{
			CharacterAppearanceChoice characterAppearanceChoice = Valid(ActiveRunChoices.GetValueOrDefault(key), type);
			if ((object)characterAppearanceChoice != null)
			{
				return characterAppearanceChoice;
			}
		}
		return Valid(Selected.GetValueOrDefault(key), type) ?? new CharacterAppearanceChoice(array[0].Base.ProfileId, "default");
	}

	internal static CharacterAppearanceChoice? Current(Type type)
	{
		lock (Sync)
		{
			return Choice(type);
		}
	}

	internal static bool IsSelected(CharacterSkinProfile profile, CharacterModel model)
	{
		lock (Sync)
		{
			return Entries.ContainsKey(profile.ProfileId) && Choice(((object)model).GetType())?.ProfileId == profile.ProfileId;
		}
	}

	internal static bool IsSelected(CharacterSkinProfile profile, Player player)
	{
		lock (Sync)
		{
			return Entries.ContainsKey(profile.ProfileId) && ForPlayer(player)?.ProfileId == profile.ProfileId;
		}
	}

	private static CharacterAppearanceChoice? ForPlayer(Player player)
	{
		Players.TryGetValue(player, out PlayerBinding value);
		if (value != null && value.RunKey == _runKey)
		{
			return value.Choice;
		}
		Type type = ((object)player.Character).GetType();
		string text = ((_runKey == null) ? null : CharacterAppearanceStore.RunKey(_runKey, player.NetId, Slot(type)));
		CharacterAppearanceChoice characterAppearanceChoice = ((text != null) ? Valid(RunChoices.GetValueOrDefault(text), type) : null) ?? Valid(value?.Choice, type) ?? Choice(type);
		if (characterAppearanceChoice == null)
		{
			return null;
		}
		Players.AddOrUpdate(player, new PlayerBinding(characterAppearanceChoice, _runKey));
		if (text != null && RunChoices.TryAdd(text, characterAppearanceChoice))
		{
			ActiveRunChoices[Slot(type)] = characterAppearanceChoice;
			ChoiceCache.Clear();
			if (!RunOrder.Contains(_runKey))
			{
				RunOrder.Add(_runKey);
			}
			CharacterAppearanceStore.Prune(RunChoices, RunOrder, _runKey);
			_store?.SaveRuns(RunChoices);
		}
		return characterAppearanceChoice;
	}

	internal static void BeginRun(string identity, bool fresh)
	{
		lock (Sync)
		{
			_runKey = identity;
			ActiveRunChoices.Clear();
			ChoiceCache.Clear();
			string prefix = identity + "/";
			if (fresh)
			{
				string[] array = RunChoices.Keys.Where((string k) => k.StartsWith(prefix, StringComparison.Ordinal)).ToArray();
				string[] array2 = array;
				foreach (string key in array2)
				{
					RunChoices.Remove(key);
				}
				if (array.Length != 0)
				{
					_store?.SaveRuns(RunChoices);
				}
				return;
			}
			foreach (KeyValuePair<string, CharacterAppearanceChoice> item in RunChoices.Where<KeyValuePair<string, CharacterAppearanceChoice>>((KeyValuePair<string, CharacterAppearanceChoice> p) => p.Key.StartsWith(prefix, StringComparison.Ordinal)))
			{
				if (Entries.TryGetValue(item.Value.ProfileId, out Entry value))
				{
					ActiveRunChoices[Slot(value.Target)] = item.Value;
				}
			}
		}
	}

	internal static void EndRun()
	{
		lock (Sync)
		{
			_runKey = null;
			ActiveRunChoices.Clear();
			ChoiceCache.Clear();
		}
	}

	internal static void EnterSelectionScreen()
	{
		EndRun();
	}

	internal static CharacterSkinProfile Resolve(CharacterSkinProfile profile, Player? player = null)
	{
		lock (Sync)
		{
			if (!Entries.TryGetValue(profile.ProfileId, out Entry value))
			{
				return profile;
			}
			CharacterAppearanceChoice characterAppearanceChoice = ((player == null) ? Choice(value.Target) : ForPlayer(player));
			CharacterSkinProfile value2;
			return (characterAppearanceChoice?.ProfileId == profile.ProfileId && value.Resolved.TryGetValue(characterAppearanceChoice.VariantId, out value2)) ? value2 : profile;
		}
	}

	internal static bool Select(Type type, string profileId, string variantId, bool persist = true)
	{
		lock (Sync)
		{
			if (!Entries.TryGetValue(profileId, out Entry value) || !((ReadOnlySpan<Entry>)Candidates(type)).Contains(value) || !value.Resolved.ContainsKey(variantId))
			{
				return false;
			}
			Selected[Slot(type)] = new CharacterAppearanceChoice(profileId, variantId);
			ChoiceCache.Clear();
			if (persist)
			{
				_store?.SavePreferences(Selected);
			}
			return true;
		}
	}

	internal static IReadOnlyList<string> CharacterIds(Type type)
	{
		lock (Sync)
		{
			return (from e in Candidates(type)
				select e.Base.ProfileId).ToArray();
		}
	}

	internal static IReadOnlyList<string> VariantIds(string id)
	{
		lock (Sync)
		{
			Entry value;
			return Entries.TryGetValue(id, out value) ? value.Labels.Keys.ToArray() : Array.Empty<string>();
		}
	}

	internal static bool IsActive(CharacterSkinProfile profile)
	{
		lock (Sync)
		{
			Entry value;
			return Entries.TryGetValue(profile.ProfileId, out value) && Choice(value.Target)?.ProfileId == profile.ProfileId;
		}
	}

	internal static string Label(Type type)
	{
		lock (Sync)
		{
			CharacterAppearanceChoice characterAppearanceChoice = Choice(type);
			return (characterAppearanceChoice == null) ? "" : Entries[characterAppearanceChoice.ProfileId].Labels[characterAppearanceChoice.VariantId]();
		}
	}

	internal static string CharacterName(Type type)
	{
		lock (Sync)
		{
			CharacterAppearanceChoice characterAppearanceChoice = Choice(type);
			return (characterAppearanceChoice == null) ? "" : Entries[characterAppearanceChoice.ProfileId].Name();
		}
	}
}

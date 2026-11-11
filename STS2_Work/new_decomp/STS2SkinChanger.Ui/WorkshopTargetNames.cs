using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal sealed class WorkshopTargetNames
{
	private readonly Dictionary<(string Kind, string Id), string> _names = new Dictionary<(string, string), string>();

	private readonly Dictionary<string, Dictionary<string, HashSet<string>>> _regions = new Dictionary<string, Dictionary<string, HashSet<string>>>();

	public Dictionary<string, string> RegionNames { get; } = new Dictionary<string, string>();

	internal static string Normalize(string id)
	{
		return new string(id.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}

	public string Name(string kind, string id)
	{
		if (_names.TryGetValue((kind, Normalize(id)), out string value))
		{
			return value;
		}
		string text;
		string text2;
		if (!id.StartsWith("event:"))
		{
			text = id;
		}
		else
		{
			text2 = id;
			int length = "event:".Length;
			text = text2.Substring(length, text2.Length - length);
		}
		string text3 = text;
		switch (kind)
		{
		case "monster":
		case "companion":
			text2 = "monsters";
			break;
		case "character":
		case "cards":
			text2 = "characters";
			break;
		default:
			text2 = "events";
			break;
		}
		string text4 = text2;
		return Localize(text4, text3.ToUpperInvariant() + ((text4 == "monsters") ? ".name" : ".title"), SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup g) => g.Id == id)?.DisplayName ?? id.Replace('_', ' '));
	}

	public IReadOnlyDictionary<string, HashSet<string>> Regions(string kind)
	{
		return _regions.GetValueOrDefault(kind) ?? new Dictionary<string, HashSet<string>>();
	}

	private void Add(string kind, string id, string title)
	{
		_names[(kind, Normalize(id))] = Plain(title);
	}

	private static string Plain(string text)
	{
		return Regex.Replace(text, "\\[[^\\]]*\\]", "").Replace('\n', ' ');
	}

	private static string Localize(string table, string key, string fallback)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			return LocManager.Instance.GetTable(table).HasEntry(key) ? Plain(new LocString(table, key).GetFormattedText()) : fallback;
		}
		catch
		{
			return fallback;
		}
	}

	private static string SafeTitle(Func<string> read, string fallback)
	{
		try
		{
			return read();
		}
		catch
		{
			return fallback;
		}
	}

	public static WorkshopTargetNames Build()
	{
		WorkshopTargetNames workshopTargetNames = new WorkshopTargetNames();
		foreach (CharacterModel character in ModelDb.AllCharacters)
		{
			string title = SafeTitle(() => character.Title.GetFormattedText(), ((AbstractModel)character).Id.Entry);
			workshopTargetNames.Add("character", ((AbstractModel)character).Id.Entry, title);
			workshopTargetNames.Add("cards", character.CardPool.Title, title);
		}
		(string, WorkshopBrowserTextKey)[] array = new(string, WorkshopBrowserTextKey)[8]
		{
			("ancients", WorkshopBrowserTextKey.AncientCards),
			("colorless", WorkshopBrowserTextKey.Colorless),
			("misc", WorkshopBrowserTextKey.MiscCards),
			("curse", WorkshopBrowserTextKey.Curse),
			("event", WorkshopBrowserTextKey.Event),
			("status", WorkshopBrowserTextKey.Status),
			("token", WorkshopBrowserTextKey.Token),
			("quest", WorkshopBrowserTextKey.Quest)
		};
		for (int j = 0; j < array.Length; j++)
		{
			var (id2, key) = array[j];
			workshopTargetNames.Add("cards", id2, WorkshopBrowserText.Get(key));
		}
		foreach (AncientEventModel allAncient in ModelDb.AllAncients)
		{
			workshopTargetNames.Add("ancient", ((AbstractModel)allAncient).Id.Entry, AncientCompendiumEntry.GetTitle(allAncient));
		}
		foreach (OtherCreatureDefinition item in OtherCreatureCatalog.All)
		{
			workshopTargetNames.Add("companion", item.Id, Localize(item.LocalizationTable, item.LocalizationKey, item.FallbackTitle));
		}
		workshopTargetNames.Add("merchant", "merchant", Localize("map", "LEGEND_MERCHANT.title", WorkshopText.Kind("merchant")));
		workshopTargetNames.Add("merchant", "fake_merchant_monster", Localize("events", "FAKE_MERCHANT.title", WorkshopText.Kind("merchant") + "???"));
		ActModel[] array2 = ModelDb.Acts.ToArray();
		WorkshopTarget[] source = SkinWorkshopService.Catalog.SelectMany((WorkshopCatalogItem i) => i.Targets).Where(WorkshopBrowserPolicy.VisibleTarget).Distinct()
			.ToArray();
		string[] source2 = (from t in source
			where t.Kind == "monster"
			select t.Target).ToArray();
		HashSet<string> assigned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
		ActModel[] array3 = array2;
		MonsterModel[] array5;
		foreach (ActModel act in array3)
		{
			workshopTargetNames.RegionNames[((AbstractModel)act).Id.Entry] = SafeTitle(() => act.Title.GetFormattedText(), ((AbstractModel)act).Id.Entry);
			MonsterModel[] array4 = act.AllMonsters.ToArray();
			array5 = array4;
			foreach (MonsterModel monster in array5)
			{
				workshopTargetNames.Add("monster", ((AbstractModel)monster).Id.Entry, SafeTitle(() => monster.Title.GetFormattedText(), ((AbstractModel)monster).Id.Entry));
			}
			HashSet<string> tokens = array4.Select((MonsterModel m) => Normalize(((AbstractModel)m).Id.Entry)).ToHashSet();
			HashSet<string> hashSet = source2.Where((string id) => tokens.Contains(Normalize(id))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			if (hashSet.Count > 0)
			{
				dictionary[((AbstractModel)act).Id.Entry] = hashSet;
			}
			assigned.UnionWith(hashSet);
		}
		HashSet<string> hashSet2 = source2.Where((string id) => !assigned.Contains(id)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		MonsterModel[] array6 = ((typeof(ModelDb).GetProperty("EventEncounters")?.GetValue(null) as IEnumerable<EncounterModel>) ?? Array.Empty<EncounterModel>()).SelectMany((EncounterModel e) => e.AllPossibleMonsters).DistinctBy((MonsterModel m) => ((AbstractModel)m).Id).ToArray();
		array5 = array6;
		foreach (MonsterModel monster2 in array5)
		{
			workshopTargetNames.Add("monster", ((AbstractModel)monster2).Id.Entry, SafeTitle(() => monster2.Title.GetFormattedText(), ((AbstractModel)monster2).Id.Entry));
		}
		HashSet<string> eventTokens = array6.Select((MonsterModel m) => Normalize(((AbstractModel)m).Id.Entry)).ToHashSet();
		HashSet<string> hashSet3 = source2.Where((string id) => eventTokens.Contains(Normalize(id))).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		if (hashSet3.Count > 0)
		{
			dictionary["events"] = hashSet3;
			workshopTargetNames.RegionNames["events"] = Localize("bestiary", "EVENTS.title", WorkshopText.Kind("event"));
			hashSet2.ExceptWith(hashSet3);
		}
		if (hashSet2.Count > 0)
		{
			dictionary["__other__"] = hashSet2;
		}
		workshopTargetNames._regions["monster"] = dictionary;
		EventModel[] array7 = EventCompendiumPreview.Events();
		foreach (EventModel model in array7)
		{
			workshopTargetNames.Add("event", EventSkinPolicy.GroupId(((AbstractModel)model).Id.Entry), SafeTitle(() => EventCompendiumPreview.Title(model), ((AbstractModel)model).Id.Entry));
		}
		IReadOnlyDictionary<string, string[]> source3 = EventRegionPolicy.Group((from t in source
			where t.Kind == "event"
			select t.Target).ToArray(), array2.ToDictionary((ActModel a) => ((AbstractModel)a).Id.Entry, (ActModel a) => a.AllEvents.Select((EventModel e) => EventSkinPolicy.GroupId(((AbstractModel)e).Id.Entry))), ModelDb.AllSharedEvents.Select((EventModel e) => EventSkinPolicy.GroupId(((AbstractModel)e).Id.Entry)));
		workshopTargetNames._regions["event"] = source3.ToDictionary((KeyValuePair<string, string[]> p) => p.Key, (KeyValuePair<string, string[]> p) => p.Value.ToHashSet<string>(StringComparer.OrdinalIgnoreCase));
		workshopTargetNames.RegionNames["__shared__"] = WorkshopBrowserText.Get(WorkshopBrowserTextKey.Shared);
		workshopTargetNames.RegionNames["__other__"] = WorkshopBrowserText.Get(WorkshopBrowserTextKey.Other);
		return workshopTargetNames;
	}
}

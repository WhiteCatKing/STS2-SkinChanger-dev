using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal sealed class LaunchPlan
{
	private readonly List<string> _baselineOrder;

	private readonly Dictionary<string, bool> _baselineEnabled;

	internal static bool KeepDisabledInPlace { get; set; }

	internal List<LaunchEntry> Entries { get; }

	internal bool HasChanges
	{
		get
		{
			bool value;
			if (Entries.Select((LaunchEntry entry) => entry.Key).SequenceEqual<string>(_baselineOrder, StringComparer.Ordinal))
			{
				return Entries.Any((LaunchEntry entry) => _baselineEnabled.TryGetValue(entry.Key, out value) && value != entry.Enabled);
			}
			return true;
		}
	}

	internal int EnabledCount => Entries.Count((LaunchEntry entry) => entry.Enabled);

	internal int DisabledCount => Entries.Count - EnabledCount;

	internal LaunchPlan(IEnumerable<Mod> mods)
		: this(mods.Select(delegate(Mod mod)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Invalid comparison between Unknown and I4
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			ModLoadState state = mod.state;
			return new LaunchPlanSeed(mod, (int)state != 3 && (int)state != 4);
		}))
	{
	}

	internal LaunchPlan(IEnumerable<LaunchPlanSeed> seeds)
	{
		Entries = seeds.Select((LaunchPlanSeed seed, int index) => new LaunchEntry(seed.Mod, index, seed.Enabled)).ToList();
		NormalizeDuplicateSelections();
		LaunchEntry launchEntry = Entries.FirstOrDefault((LaunchEntry entry) => entry.IsManager);
		if (launchEntry != null)
		{
			launchEntry.Enabled = true;
		}
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
		_baselineOrder = Entries.Select((LaunchEntry entry) => entry.Key).ToList();
		_baselineEnabled = new Dictionary<string, bool>(StringComparer.Ordinal);
		foreach (LaunchEntry entry in Entries)
		{
			_baselineEnabled.TryAdd(entry.Key, entry.Enabled);
		}
	}

	internal LaunchEntry? Find(string key)
	{
		return Entries.FirstOrDefault((LaunchEntry entry) => string.Equals(entry.Key, key, StringComparison.Ordinal));
	}

	internal IReadOnlyList<LaunchEntry> AddDiscoveredMods(IEnumerable<Mod> mods, Func<Mod, bool>? joinSwitchedOff = null)
	{
		List<LaunchEntry> list = new List<LaunchEntry>();
		foreach (Mod mod in mods)
		{
			string key = PhysicalModIdentity.KeyFor(mod);
			if (!Entries.Any((LaunchEntry entry) => string.Equals(entry.Key, key, StringComparison.Ordinal)))
			{
				LaunchEntry launchEntry = ((joinSwitchedOff != null && joinSwitchedOff(mod)) ? new LaunchEntry(mod, Entries.Count, enabled: false) : new LaunchEntry(mod, Entries.Count));
				Entries.Add(launchEntry);
				_baselineOrder.Add(launchEntry.Key);
				_baselineEnabled[launchEntry.Key] = launchEntry.Enabled;
				list.Add(launchEntry);
			}
		}
		NormalizeDuplicateSelections();
		HashSet<string> addedIds = (from entry in list
			select entry.Mod.manifest?.id into id
			where !string.IsNullOrWhiteSpace(id)
			select (id)).ToHashSet<string>(StringComparer.Ordinal);
		foreach (LaunchEntry item in Entries.Where(delegate(LaunchEntry candidate)
		{
			string text = candidate.Mod.manifest?.id;
			return text != null && addedIds.Contains(text);
		}))
		{
			_baselineEnabled[item.Key] = item.Enabled;
		}
		NormalizeEnabledSections();
		NormalizeBaselineEnabledSections();
		return list;
	}

	internal void Relocate(LaunchEntry entry, string path)
	{
		string previousKey = entry.Key;
		entry.Mod.path = path;
		entry.InvalidateManifestMetadata();
		string key = entry.Key;
		if (string.Equals(previousKey, key, StringComparison.Ordinal))
		{
			return;
		}
		int num = _baselineOrder.IndexOf(previousKey);
		if (num >= 0)
		{
			_baselineOrder[num] = key;
		}
		if (_baselineEnabled.TryGetValue(previousKey, out var value))
		{
			_baselineEnabled[key] = value;
			if (!Entries.Any((LaunchEntry other) => string.Equals(other.Key, previousKey, StringComparison.Ordinal)))
			{
				_baselineEnabled.Remove(previousKey);
			}
		}
	}

	internal LaunchEntry? MovedWorkshopCopy(Mod arrival, Func<string, bool> folderExists)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Invalid comparison between Unknown and I4
		ulong itemId = PhysicalModIdentity.WorkshopItemIdFor(arrival);
		string id = arrival.manifest?.id;
		if ((int)arrival.modSource != 2 || itemId == 0L || string.IsNullOrWhiteSpace(id))
		{
			return null;
		}
		List<LaunchEntry> list = Entries.Where((LaunchEntry entry) => entry.Mod != arrival && (int)entry.Mod.modSource == 2 && string.Equals(entry.Mod.manifest?.id, id, StringComparison.Ordinal) && PhysicalModIdentity.WorkshopItemIdFor(entry.Mod) == itemId && !string.IsNullOrWhiteSpace(entry.Mod.path) && !folderExists(entry.Mod.path)).Take(2).ToList();
		if (list.Count != 1)
		{
			return null;
		}
		return list[0];
	}

	internal IReadOnlyList<LaunchEntry> RemoveEntries(IEnumerable<LaunchEntry> entries)
	{
		List<LaunchEntry> list = new List<LaunchEntry>();
		foreach (LaunchEntry entry in entries)
		{
			if (!entry.IsManager && Entries.Remove(entry))
			{
				string key = entry.Key;
				_baselineOrder.Remove(key);
				if (!Entries.Any((LaunchEntry other) => string.Equals(other.Key, key, StringComparison.Ordinal)))
				{
					_baselineEnabled.Remove(key);
				}
				list.Add(entry);
			}
		}
		return list;
	}

	internal int IndexOf(LaunchEntry entry)
	{
		return Entries.IndexOf(entry);
	}

	internal int LoadPosition(LaunchEntry entry)
	{
		int num = 1;
		foreach (LaunchEntry entry2 in Entries)
		{
			if (entry2 == entry)
			{
				return num;
			}
			if (entry2.Enabled)
			{
				num++;
			}
		}
		return -1;
	}

	internal PlanResult MoveTo(LaunchEntry entry, LaunchEntry targetEntry, bool insertAfter)
	{
		if (!entry.CanMove)
		{
			return PlanResult.Error(MlmLocManager.Get("plan.move.managerMustBeFirst"));
		}
		int num = Entries.IndexOf(entry);
		int num2 = Entries.IndexOf(targetEntry);
		if (num < 0 || num2 < 0)
		{
			return PlanResult.Error(MlmLocManager.Get("plan.move.targetMissing"));
		}
		if (!KeepDisabledInPlace && entry.Enabled != targetEntry.Enabled)
		{
			return PlanResult.Error(MlmLocManager.Get("plan.move.crossSection"));
		}
		HashSet<string> before = ViolationKeys();
		int num3 = num2 + (insertAfter ? 1 : 0);
		Entries.RemoveAt(num);
		if (num < num3)
		{
			num3--;
		}
		num3 = Math.Clamp(num3, 1, Entries.Count);
		if (num3 == num)
		{
			Entries.Insert(num, entry);
			return PlanResult.Ok(MlmLocManager.Get("plan.move.alreadyThere", ("name", entry.Name)));
		}
		Entries.Insert(num3, entry);
		PlanResult? planResult = IntroducedViolation(before);
		if (planResult.HasValue)
		{
			PlanResult valueOrDefault = planResult.GetValueOrDefault();
			Entries.Remove(entry);
			Entries.Insert(num, entry);
			return valueOrDefault;
		}
		return PlanResult.Ok((!entry.Enabled && KeepDisabledInPlace) ? MlmLocManager.Get("plan.move.successDisabled", ("name", entry.Name), ("position", LoadPosition(entry))) : MlmLocManager.Get("plan.move.success", ("name", entry.Name), ("position", entry.Enabled ? LoadPosition(entry) : (num3 + 1))));
	}

	internal PlanResult SetEnabled(LaunchEntry entry, bool enabled)
	{
		if (!entry.CanToggle)
		{
			return PlanResult.Error(MlmLocManager.Get("plan.toggle.managerCannotDisable"));
		}
		HashSet<string> hashSet = ViolationKeys();
		HashSet<string> before = ConflictPairKeys();
		List<LaunchEntry> order = new List<LaunchEntry>(Entries);
		Dictionary<LaunchEntry, bool> previousEnabled = Entries.ToDictionary((LaunchEntry candidate) => candidate, (LaunchEntry candidate) => candidate.Enabled);
		int num = 0;
		bool flag = false;
		if (enabled)
		{
			(int AutoDisabled, bool InheritedPosition) tuple = EnableExclusively(entry);
			num = tuple.AutoDisabled;
			flag = tuple.InheritedPosition;
			PlanResult result = AutoSortDependencies(includeDisabledRelations: false, hashSet);
			if (!result.Success)
			{
				Restore(order, previousEnabled);
				return result;
			}
		}
		else
		{
			entry.Enabled = false;
		}
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
		PlanResult? planResult = IntroducedViolation(hashSet);
		if (planResult.HasValue)
		{
			PlanResult valueOrDefault = planResult.GetValueOrDefault();
			Restore(order, previousEnabled);
			return valueOrDefault;
		}
		List<string> list = new List<string>();
		if (num > 0)
		{
			list.Add(MlmLocManager.Get("plan.toggle.note.autoDisabledCopies", ("count", num)));
		}
		bool value;
		int num2 = Entries.Count((LaunchEntry candidate) => candidate != entry && previousEnabled.TryGetValue(candidate, out value) && !value && candidate.Enabled);
		if (num2 > 0)
		{
			list.Add(MlmLocManager.Get("plan.toggle.note.autoEnabledPrerequisites", ("count", num2)));
		}
		if (flag)
		{
			list.Add(MlmLocManager.Get("plan.toggle.note.inheritedPosition"));
		}
		int num3 = Entries.Where((LaunchEntry candidate) => candidate.Enabled == entry.Enabled).ToList().IndexOf(entry) + 1;
		list.Add(entry.Enabled ? MlmLocManager.Get("plan.toggle.note.loadPosition", ("position", num3)) : (KeepDisabledInPlace ? MlmLocManager.Get("plan.toggle.note.keptPosition", ("position", LoadPosition(entry))) : MlmLocManager.Get("plan.toggle.note.goesLast")));
		string item = MlmLocManager.Get(enabled ? "plan.toggle.summary.enabled" : "plan.toggle.summary.disabled", ("name", entry.Name));
		return WarnAboutNewConflicts(PlanResult.Ok((list.Count == 0) ? MlmLocManager.Get("plan.result.summary", ("summary", item)) : MlmLocManager.Get("plan.result.summaryWithNotes", ("summary", item), ("notes", string.Join(MlmLocManager.Get("common.listSeparator"), list)))), before);
	}

	internal PlanResult EnableRandom(out LaunchEntry? selected, Random? random = null)
	{
		selected = null;
		if (random == null)
		{
			random = Random.Shared;
		}
		Dictionary<string, LaunchEntry> providers = BuildDependencyProviderLookup();
		List<LaunchEntry> list = providers.Values.Where((LaunchEntry entry) => entry.CanToggle && !entry.Enabled && !string.IsNullOrWhiteSpace(entry.Mod.manifest?.id)).ToList();
		if (list.Count == 0)
		{
			return PlanResult.Error(MlmLocManager.Get("plan.random.noneDisabled"));
		}
		List<LaunchEntry> order = new List<LaunchEntry>(Entries);
		Dictionary<LaunchEntry, bool> previousEnabled = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => entry.Enabled);
		HashSet<string> conflictsBefore = ConflictPairKeys();
		while (list.Count > 0)
		{
			int index = random.Next(list.Count);
			LaunchEntry candidate = list[index];
			list.RemoveAt(index);
			if (!CanResolve(candidate, new HashSet<LaunchEntry>(), new HashSet<LaunchEntry>()))
			{
				continue;
			}
			bool flag = false;
			try
			{
				if (!SetEnabled(candidate, enabled: true).Success || Conflicts().Any((ModConflict conflict) => !conflictsBefore.Contains(conflict.PairKey)))
				{
					continue;
				}
				int num = Entries.Count((LaunchEntry entry) => entry.Enabled && !previousEnabled[entry] && entry != candidate);
				PlanResult result = PlanResult.Ok(MlmLocManager.Get((num == 0) ? "plan.random.enabled" : "plan.random.enabledWithDependencies", ("name", candidate.DisplayName), ("count", num)));
				flag = true;
				selected = candidate;
				return result;
			}
			finally
			{
				if (!flag)
				{
					Restore(order, previousEnabled);
				}
			}
		}
		return PlanResult.Error(MlmLocManager.Get("plan.random.noneSuitable"));
		bool CanResolve(LaunchEntry entry, HashSet<LaunchEntry> visiting, HashSet<LaunchEntry> visited)
		{
			if (visited.Contains(entry))
			{
				return true;
			}
			if (!visiting.Add(entry))
			{
				return false;
			}
			foreach (ModDependency item in entry.Mod.manifest?.dependencies ?? new List<ModDependency>())
			{
				if (string.IsNullOrWhiteSpace(item.id) || !providers.TryGetValue(item.id, out LaunchEntry value) || !CanResolve(value, visiting, visited))
				{
					return false;
				}
			}
			visiting.Remove(entry);
			visited.Add(entry);
			return true;
		}
	}

	internal PlanResult SetEnabledForAll(IReadOnlyList<LaunchEntry> targets, bool enabled)
	{
		HashSet<string> hashSet = ViolationKeys();
		HashSet<string> before = ConflictPairKeys();
		List<LaunchEntry> order = new List<LaunchEntry>(Entries);
		Dictionary<LaunchEntry, bool> previousEnabled = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => entry.Enabled);
		if (enabled)
		{
			EnableSelectionsInBulk(targets);
			if (!Entries.Any((LaunchEntry entry) => previousEnabled[entry] != entry.Enabled))
			{
				return PlanResult.Ok(MlmLocManager.Get("plan.bulk.allAlreadyEnabled"));
			}
			PlanResult result = AutoSortDependencies(includeDisabledRelations: false, hashSet);
			if (!result.Success)
			{
				Restore(order, previousEnabled);
				return result;
			}
		}
		else
		{
			foreach (LaunchEntry item in targets.Where((LaunchEntry entry) => entry.CanToggle))
			{
				item.Enabled = false;
			}
			NormalizeEnabledSections();
			NormalizeBootstrapOrder();
		}
		bool value;
		int num = Entries.Count((LaunchEntry entry) => previousEnabled.TryGetValue(entry, out value) && value != entry.Enabled);
		if (num == 0)
		{
			return PlanResult.Ok(enabled ? MlmLocManager.Get("plan.bulk.allAlreadyEnabled") : MlmLocManager.Get("plan.bulk.allAlreadyDisabled"));
		}
		if (!enabled)
		{
			PlanResult? planResult = IntroducedViolation(hashSet);
			if (planResult.HasValue)
			{
				PlanResult valueOrDefault = planResult.GetValueOrDefault();
				Restore(order, previousEnabled);
				return valueOrDefault;
			}
		}
		return WarnAboutNewConflicts(PlanResult.Ok(MlmLocManager.Get(enabled ? "plan.bulk.enabled" : "plan.bulk.disabled", ("count", num))), before);
	}

	internal PlanResult ApplyDeclaredPriorities(bool rebaseline, out int moved)
	{
		moved = 0;
		if (!Entries.Any((LaunchEntry entry) => entry.PriorityIsBinding))
		{
			return PlanResult.Ok(MlmLocManager.Get("priority.sort.none"));
		}
		if (!Validate().Success)
		{
			return PlanResult.Ok(MlmLocManager.Get("priority.sort.skipped"));
		}
		List<LaunchEntry> previousOrder = new List<LaunchEntry>(Entries);
		List<LaunchEntry> collection = (from item in Entries.Select((LaunchEntry entry, int index) => (entry: entry, index: index))
			orderby (int)item.entry.EffectivePriority, item.index
			select item.entry).ToList();
		Entries.Clear();
		Entries.AddRange(collection);
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
		PlanResult planResult = Validate();
		if (!planResult.Success)
		{
			Entries.Clear();
			Entries.AddRange(previousOrder);
			return PlanResult.Error(MlmLocManager.Get("priority.sort.conflict", ("error", planResult.Message)));
		}
		moved = Entries.Where((LaunchEntry entry, int index) => previousOrder[index] != entry).Count();
		if (rebaseline)
		{
			_baselineOrder.Clear();
			_baselineOrder.AddRange(Entries.Select((LaunchEntry entry) => entry.Key));
		}
		return PlanResult.Ok((moved == 0) ? MlmLocManager.Get("priority.sort.none") : MlmLocManager.Get("priority.sort.applied", ("count", moved)));
	}

	internal PlanResult AutoSortDependencies(bool includeDisabledRelations = true, IReadOnlyCollection<string>? preexisting = null)
	{
		if (preexisting == null)
		{
			preexisting = ViolationKeys();
		}
		HashSet<string> before = ConflictPairKeys();
		List<LaunchEntry> previousOrder = new List<LaunchEntry>(Entries);
		Dictionary<LaunchEntry, bool> enabled = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => entry.Enabled);
		HashSet<LaunchEntry> autoEnabled = new HashSet<LaunchEntry>();
		HashSet<LaunchEntry> visiting = new HashSet<LaunchEntry>();
		HashSet<LaunchEntry> visited = new HashSet<LaunchEntry>();
		Dictionary<string, LaunchEntry> dependencyProviders = BuildDependencyProviderLookup();
		foreach (LaunchEntry item in previousOrder.Where((LaunchEntry candidate) => candidate.Enabled))
		{
			EnsureDependencies(item);
		}
		Dictionary<LaunchEntry, int> indegree = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry _) => 0);
		Dictionary<LaunchEntry, HashSet<LaunchEntry>> consumers = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry _) => new HashSet<LaunchEntry>());
		IEnumerable<LaunchEntry> enumerable;
		if (!includeDisabledRelations)
		{
			enumerable = Entries.Where((LaunchEntry candidate) => candidate.Enabled);
		}
		else
		{
			IEnumerable<LaunchEntry> entries = Entries;
			enumerable = entries;
		}
		foreach (LaunchEntry item2 in enumerable)
		{
			List<ModDependency> list = item2.Mod.manifest?.dependencies;
			if (list == null)
			{
				continue;
			}
			foreach (ModDependency item3 in list)
			{
				if (item3.id != null && dependencyProviders.TryGetValue(item3.id, out LaunchEntry value) && consumers[value].Add(item2))
				{
					indegree[item2]++;
				}
			}
		}
		Dictionary<LaunchEntry, int> category = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => (consumers[entry].Count <= 0) ? ((indegree[entry] <= 0) ? 1 : 2) : 0);
		Dictionary<LaunchEntry, int> currentRank = new Dictionary<LaunchEntry, int>();
		for (int i = 0; i < previousOrder.Count; i++)
		{
			currentRank[previousOrder[i]] = i;
		}
		SortedSet<LaunchEntry> sortedSet = new SortedSet<LaunchEntry>(Comparer<LaunchEntry>.Create(CompareReady));
		foreach (LaunchEntry item4 in Entries.Where((LaunchEntry entry) => indegree[entry] == 0))
		{
			sortedSet.Add(item4);
		}
		List<LaunchEntry> list2 = new List<LaunchEntry>(Entries.Count);
		while (sortedSet.Count > 0)
		{
			LaunchEntry min = sortedSet.Min;
			sortedSet.Remove(min);
			list2.Add(min);
			foreach (LaunchEntry item5 in consumers[min])
			{
				indegree[item5]--;
				if (indegree[item5] == 0)
				{
					sortedSet.Add(item5);
				}
			}
		}
		bool flag = list2.Count == Entries.Count;
		if (flag)
		{
			Entries.Clear();
			Entries.AddRange(list2);
		}
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
		PlanResult? planResult = IntroducedViolation(preexisting);
		if (planResult.HasValue)
		{
			PlanResult valueOrDefault = planResult.GetValueOrDefault();
			Restore(previousOrder, enabled);
			return PlanResult.Error(MlmLocManager.Get("plan.dependencies.stillInvalid", ("error", valueOrDefault.Message)));
		}
		int num = Entries.Select((LaunchEntry entry, int index) => (entry: entry, index: index)).Count(((LaunchEntry entry, int index) item) => previousOrder[item.index] != item.entry);
		List<string> list3 = new List<string>();
		if (autoEnabled.Count > 0)
		{
			list3.Add(MlmLocManager.Get("plan.autoSort.note.autoEnabledPrerequisites", ("count", autoEnabled.Count)));
		}
		if (num > 0)
		{
			list3.Add(MlmLocManager.Get("plan.autoSort.note.positionsAdjusted", ("count", num)));
		}
		if (!flag)
		{
			list3.Add(MlmLocManager.Get("plan.dependencies.sortCycle"));
		}
		int count = Violations().Count;
		if (count > 0)
		{
			list3.Add(MlmLocManager.Get("plan.autoSort.note.unresolved", ("count", count)));
		}
		return WarnAboutNewConflicts(PlanResult.Ok((list3.Count == 0) ? MlmLocManager.Get("plan.autoSort.noChanges") : MlmLocManager.Get("plan.autoSort.complete", ("notes", string.Join(MlmLocManager.Get("common.listSeparator"), list3)))), before);
		int CompareReady(LaunchEntry left, LaunchEntry right)
		{
			int num2 = ((!KeepDisabledInPlace) ? ((!left.Enabled) ? 1 : 0).CompareTo((!right.Enabled) ? 1 : 0) : 0);
			if (num2 != 0)
			{
				return num2;
			}
			if (includeDisabledRelations)
			{
				num2 = ((int)left.EffectivePriority).CompareTo((int)right.EffectivePriority);
				if (num2 != 0)
				{
					return num2;
				}
				num2 = category[left].CompareTo(category[right]);
				if (num2 != 0)
				{
					return num2;
				}
				num2 = string.IsNullOrWhiteSpace(left.Group).CompareTo(string.IsNullOrWhiteSpace(right.Group));
				if (num2 != 0)
				{
					return num2;
				}
				num2 = StringComparer.OrdinalIgnoreCase.Compare(left.Group, right.Group);
				if (num2 != 0)
				{
					return num2;
				}
				num2 = StringComparer.OrdinalIgnoreCase.Compare(left.Name, right.Name);
				if (num2 != 0)
				{
					return num2;
				}
				num2 = StringComparer.OrdinalIgnoreCase.Compare(left.Id, right.Id);
				if (num2 != 0)
				{
					return num2;
				}
			}
			return currentRank[left].CompareTo(currentRank[right]);
		}
		void EnsureDependencies(LaunchEntry entry)
		{
			if (entry.Enabled && !visited.Contains(entry) && visiting.Add(entry))
			{
				List<ModDependency> list4 = entry.Mod.manifest?.dependencies;
				if (list4 != null)
				{
					foreach (ModDependency item6 in list4)
					{
						if (item6.id != null && dependencyProviders.TryGetValue(item6.id, out LaunchEntry value2))
						{
							if (!value2.Enabled && value2.CanToggle)
							{
								value2.Enabled = true;
								autoEnabled.Add(value2);
							}
							EnsureDependencies(value2);
						}
					}
				}
				visiting.Remove(entry);
				visited.Add(entry);
			}
		}
	}

	private Dictionary<string, LaunchEntry> BuildDependencyProviderLookup()
	{
		Dictionary<string, LaunchEntry> dictionary = new Dictionary<string, LaunchEntry>(StringComparer.Ordinal);
		foreach (LaunchEntry entry in Entries)
		{
			if (!dictionary.TryGetValue(entry.Id, out var value) || (!value.Enabled && entry.Enabled) || (!value.Enabled && !entry.Enabled && value.IsDuplicate && !entry.IsDuplicate))
			{
				dictionary[entry.Id] = entry;
			}
		}
		return dictionary;
	}

	internal bool HasDuplicateSiblings(LaunchEntry entry)
	{
		return Entries.Any((LaunchEntry candidate) => candidate != entry && SameModId(candidate, entry));
	}

	internal HashSet<string> SharedModIds()
	{
		return (from @group in (from id in Entries.Select((LaunchEntry entry) => entry.Mod.manifest?.id).OfType<string>()
				where !string.IsNullOrWhiteSpace(id)
				select id).GroupBy<string, string>((string id) => id, StringComparer.Ordinal)
			where @group.Count() > 1
			select @group.Key).ToHashSet<string>(StringComparer.Ordinal);
	}

	private (int AutoDisabled, bool InheritedPosition) EnableExclusively(LaunchEntry entry)
	{
		List<LaunchEntry> list = Entries.Where((LaunchEntry candidate) => candidate != entry && SameModId(candidate, entry)).ToList();
		LaunchEntry launchEntry = list.FirstOrDefault((LaunchEntry candidate) => candidate.Enabled);
		bool item = false;
		if (launchEntry != null)
		{
			int num = Entries.IndexOf(launchEntry);
			int num2 = Entries.IndexOf(entry);
			if (num >= 0 && num2 >= 0 && num != num2)
			{
				Entries[num] = entry;
				Entries[num2] = launchEntry;
				item = true;
			}
		}
		int num3 = 0;
		foreach (LaunchEntry item2 in list)
		{
			if (item2.Enabled)
			{
				item2.Enabled = false;
				num3++;
			}
		}
		entry.Enabled = true;
		return (AutoDisabled: num3, InheritedPosition: item);
	}

	private void EnableSelectionsInBulk(IReadOnlyList<LaunchEntry> targets)
	{
		Dictionary<string, List<LaunchEntry>> dictionary = Entries.GroupBy<LaunchEntry, string>(DuplicateGroupKey, StringComparer.Ordinal).ToDictionary<IGrouping<string, LaunchEntry>, string, List<LaunchEntry>>((IGrouping<string, LaunchEntry> group) => group.Key, (IGrouping<string, LaunchEntry> group) => group.ToList(), StringComparer.Ordinal);
		Dictionary<string, LaunchEntry> dictionary2 = targets.Where((LaunchEntry entry) => entry.CanToggle).GroupBy<LaunchEntry, string>(DuplicateGroupKey, StringComparer.Ordinal).ToDictionary<IGrouping<string, LaunchEntry>, string, LaunchEntry>((IGrouping<string, LaunchEntry> group) => group.Key, (IGrouping<string, LaunchEntry> group) => group.FirstOrDefault((LaunchEntry entry) => entry.Enabled) ?? group.FirstOrDefault((LaunchEntry entry) => !entry.IsDuplicate) ?? group.First(), StringComparer.Ordinal);
		Dictionary<LaunchEntry, int> dictionary3 = Entries.Select((LaunchEntry entry, int index) => (entry: entry, index: index)).ToDictionary(((LaunchEntry entry, int index) item) => item.entry, ((LaunchEntry entry, int index) item) => item.index);
		foreach (KeyValuePair<string, LaunchEntry> item in dictionary2)
		{
			item.Deconstruct(out var key, out var value);
			string key2 = key;
			LaunchEntry selected = value;
			List<LaunchEntry> list = dictionary[key2];
			LaunchEntry launchEntry = list.FirstOrDefault((LaunchEntry entry) => entry != selected && entry.Enabled);
			if (launchEntry != null)
			{
				int num = dictionary3[launchEntry];
				int num2 = dictionary3[selected];
				Entries[num] = selected;
				Entries[num2] = launchEntry;
				dictionary3[selected] = num;
				dictionary3[launchEntry] = num2;
			}
			foreach (LaunchEntry item2 in list)
			{
				item2.Enabled = item2 == selected;
			}
		}
	}

	private void NormalizeDuplicateSelections()
	{
		foreach (IGrouping<string, LaunchEntry> item in Entries.Where((LaunchEntry entry) => !string.IsNullOrWhiteSpace(entry.Mod.manifest?.id)).GroupBy<LaunchEntry, string>((LaunchEntry entry) => entry.Id, StringComparer.Ordinal))
		{
			List<LaunchEntry> list = item.Where((LaunchEntry entry) => entry.Enabled).ToList();
			if (list.Count <= 1)
			{
				continue;
			}
			LaunchEntry launchEntry = list[0];
			foreach (LaunchEntry item2 in list)
			{
				item2.Enabled = item2 == launchEntry;
			}
		}
	}

	private void NormalizeEnabledSections()
	{
		if (!KeepDisabledInPlace)
		{
			LaunchPlanOrdering.StablePartitionEnabledFirst(Entries, (LaunchEntry entry) => entry.Enabled);
		}
	}

	private void NormalizeBaselineEnabledSections()
	{
		if (!KeepDisabledInPlace)
		{
			LaunchPlanOrdering.StablePartitionEnabledFirst(_baselineOrder, (string key) => _baselineEnabled.GetValueOrDefault(key));
		}
	}

	internal void ApplyDisabledPlacement()
	{
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
	}

	internal void RestoreState(IReadOnlyList<LaunchEntry> order, IReadOnlyDictionary<LaunchEntry, bool> enabled)
	{
		HashSet<LaunchEntry> hashSet = Entries.ToHashSet();
		List<LaunchEntry> list = order.Where(hashSet.Contains).Distinct().ToList();
		HashSet<LaunchEntry> placed = list.ToHashSet();
		list.AddRange(Entries.Where((LaunchEntry entry) => !placed.Contains(entry)));
		Entries.Clear();
		Entries.AddRange(list);
		foreach (LaunchEntry entry in Entries)
		{
			if (enabled.TryGetValue(entry, out var value))
			{
				entry.Enabled = value;
			}
		}
		NormalizeDuplicateSelections();
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
	}

	private static bool SameModId(LaunchEntry left, LaunchEntry right)
	{
		string text = left.Mod.manifest?.id;
		string text2 = right.Mod.manifest?.id;
		if (!string.IsNullOrWhiteSpace(text) && !string.IsNullOrWhiteSpace(text2))
		{
			return string.Equals(text, text2, StringComparison.Ordinal);
		}
		return false;
	}

	private static string DuplicateGroupKey(LaunchEntry entry)
	{
		if (!string.IsNullOrWhiteSpace(entry.Mod.manifest?.id))
		{
			return "id:" + entry.Id;
		}
		return "entry:" + entry.Key;
	}

	private void Restore(IReadOnlyList<LaunchEntry> order, IReadOnlyDictionary<LaunchEntry, bool> enabled)
	{
		Entries.Clear();
		Entries.AddRange(order);
		foreach (KeyValuePair<LaunchEntry, bool> item in enabled)
		{
			item.Key.Enabled = item.Value;
		}
	}

	internal static string ProfileKey(string id, ModSource source)
	{
		return id;
	}

	internal LaunchProfile CaptureProfile(string name)
	{
		return new LaunchProfile
		{
			Name = name,
			Mods = Entries.Select((LaunchEntry entry) => new ProfileMod
			{
				Id = entry.Id,
				Source = ((object)(ModSource)(ref entry.Mod.modSource)/*cast due to .constrained prefix*/).ToString(),
				Enabled = entry.Enabled
			}).ToList()
		};
	}

	internal PlanResult ApplyProfile(LaunchProfile profile)
	{
		Dictionary<string, int> rank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, bool> dictionary = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> requestedCopies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, string> verifiedCopies = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < profile.Mods.Count; i++)
		{
			ProfileMod profileMod = profile.Mods[i];
			string id = profileMod.Id;
			if (!rank.ContainsKey(id))
			{
				rank[id] = i;
			}
			dictionary[id] = ((!dictionary.TryGetValue(id, out var value)) ? profileMod.Enabled : (value || profileMod.Enabled));
			if (profileMod.Enabled && (IsLocalCopySource(profileMod.Source) || TryParseWorkshopCopy(profileMod.Source, out var _)))
			{
				requestedCopies.TryAdd(id, profileMod.Source);
			}
			if (profileMod.Enabled)
			{
				string preferredPhysicalKey = profileMod.PreferredPhysicalKey;
				if (preferredPhysicalKey != null)
				{
					verifiedCopies.TryAdd(id, preferredPhysicalKey);
				}
			}
		}
		HashSet<string> preexisting = ViolationKeys();
		HashSet<string> before = ConflictPairKeys();
		List<LaunchEntry> collection = new List<LaunchEntry>(Entries);
		Dictionary<LaunchEntry, bool> dictionary2 = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => entry.Enabled);
		Dictionary<string, LaunchEntry> dictionary3 = Entries.Where((LaunchEntry entry) => entry.CanToggle).GroupBy<LaunchEntry, string>((LaunchEntry entry) => entry.Id, StringComparer.OrdinalIgnoreCase).ToDictionary<IGrouping<string, LaunchEntry>, string, LaunchEntry>((IGrouping<string, LaunchEntry> group) => group.Key, (IGrouping<string, LaunchEntry> group) => SelectProfileCopy(group, requestedCopies.GetValueOrDefault(group.Key), verifiedCopies.GetValueOrDefault(group.Key)), StringComparer.OrdinalIgnoreCase);
		int value2;
		List<LaunchEntry> collection2 = (from item in Entries.Select((LaunchEntry entry, int index) => (entry: entry, index: index))
			orderby (!rank.TryGetValue(KeyOf(item.entry), out value2)) ? int.MaxValue : value2, item.index
			select item.entry).ToList();
		Entries.Clear();
		Entries.AddRange(collection2);
		int num = 0;
		foreach (LaunchEntry entry in Entries)
		{
			if (entry.CanToggle)
			{
				bool value3;
				bool flag = dictionary.TryGetValue(KeyOf(entry), out value3);
				if (!flag && entry.Enabled)
				{
					num++;
				}
				entry.Enabled = flag && value3 && dictionary3.TryGetValue(KeyOf(entry), out var value4) && entry == value4;
			}
		}
		NormalizeBootstrapOrder();
		Dictionary<LaunchEntry, bool> profileEnabled = Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => entry.Enabled);
		PlanResult planResult = AutoSortDependencies(includeDisabledRelations: false, preexisting);
		if (!planResult.Success)
		{
			Entries.Clear();
			Entries.AddRange(collection);
			foreach (KeyValuePair<LaunchEntry, bool> item3 in dictionary2)
			{
				item3.Key.Enabled = item3.Value;
			}
			return PlanResult.Error(MlmLocManager.Get("profiles.apply.failed", ("name", profile.Name), ("error", planResult.Message)));
		}
		bool value5;
		int num2 = Entries.Count((LaunchEntry entry) => profileEnabled.TryGetValue(entry, out value5) && !value5 && entry.Enabled);
		int num3 = profile.Mods.Count((ProfileMod mod) => !Entries.Any((LaunchEntry entry) => string.Equals(KeyOf(entry), mod.Id, StringComparison.OrdinalIgnoreCase)));
		List<string> list = new List<string>();
		if (num > 0)
		{
			list.Add(MlmLocManager.Get("profiles.apply.note.disabledOutside", ("count", num)));
		}
		if (num3 > 0)
		{
			list.Add(MlmLocManager.Get("profiles.apply.note.missing", ("count", num3)));
		}
		if (num2 > 0)
		{
			list.Add(MlmLocManager.Get("plan.autoSort.note.autoEnabledPrerequisites", ("count", num2)));
		}
		string item2 = MlmLocManager.Get("profiles.apply.summary", ("name", profile.Name), ("enabled", EnabledCount));
		return WarnAboutNewConflicts(PlanResult.Ok((list.Count == 0) ? MlmLocManager.Get("plan.result.summary", ("summary", item2)) : MlmLocManager.Get("plan.result.summaryWithNotes", ("summary", item2), ("notes", string.Join(MlmLocManager.Get("common.listSeparator"), list)))), before);
	}

	private static string KeyOf(LaunchEntry entry)
	{
		return entry.Id;
	}

	private static LaunchEntry SelectProfileCopy(IEnumerable<LaunchEntry> group, string? source, string? verifiedKey)
	{
		List<LaunchEntry> list = group.ToList();
		if (verifiedKey != null)
		{
			LaunchEntry launchEntry = list.FirstOrDefault((LaunchEntry entry) => entry.Key == verifiedKey);
			if (launchEntry != null)
			{
				return launchEntry;
			}
		}
		if (TryParseWorkshopCopy(source, out var workshopId))
		{
			LaunchEntry launchEntry2 = list.FirstOrDefault((LaunchEntry entry) => PhysicalModIdentity.WorkshopItemIdFor(entry.Mod) == workshopId);
			if (launchEntry2 != null)
			{
				return launchEntry2;
			}
		}
		if (IsLocalCopySource(source))
		{
			List<LaunchEntry> list2 = list.Where((LaunchEntry entry) => (int)entry.Mod.modSource == 1).ToList();
			if (list2.Count > 0)
			{
				list = list2;
			}
		}
		return list.FirstOrDefault((LaunchEntry entry) => entry.Enabled) ?? list.FirstOrDefault((LaunchEntry entry) => !entry.IsDuplicate) ?? list[0];
	}

	private static bool IsLocalCopySource(string? source)
	{
		if (!string.Equals(source, "local", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(source, "l", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool TryParseWorkshopCopy(string? source, out ulong workshopId)
	{
		workshopId = 0uL;
		if (source != null && source.StartsWith("workshop:", StringComparison.OrdinalIgnoreCase) && ulong.TryParse(source.Substring("workshop:".Length), out workshopId))
		{
			return workshopId != 0;
		}
		return false;
	}

	internal void Reset()
	{
		Entries.Sort((LaunchEntry left, LaunchEntry right) => _baselineOrder.IndexOf(left.Key).CompareTo(_baselineOrder.IndexOf(right.Key)));
		foreach (LaunchEntry entry in Entries)
		{
			entry.Enabled = _baselineEnabled[entry.Key];
		}
		NormalizeEnabledSections();
		NormalizeBootstrapOrder();
	}

	internal List<PlanViolation> Violations()
	{
		List<PlanViolation> list = new List<PlanViolation>();
		if (Entries.Count == 0 || !Entries[0].IsManager)
		{
			list.Add(new PlanViolation("structure:managerFirst", MlmLocManager.Get("plan.validate.managerMustBeFirst")));
		}
		bool flag = false;
		foreach (LaunchEntry item in KeepDisabledInPlace ? new List<LaunchEntry>() : Entries)
		{
			if (!item.Enabled)
			{
				flag = true;
			}
			else if (flag)
			{
				list.Add(new PlanViolation("structure:sections", MlmLocManager.Get("plan.validate.sectionsInterleaved")));
				break;
			}
		}
		foreach (IGrouping<string, LaunchEntry> item2 in from @group in Entries.Where((LaunchEntry entry) => entry.Enabled && !string.IsNullOrWhiteSpace(entry.Mod.manifest?.id)).GroupBy<LaunchEntry, string>((LaunchEntry entry) => entry.Id, StringComparer.Ordinal)
			where @group.Count() > 1
			select @group)
		{
			list.Add(new PlanViolation("duplicateEnabled:" + item2.Key, MlmLocManager.Get("plan.validate.duplicateEnabled", ("id", item2.Key))));
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		for (int i = 0; i < Entries.Count; i++)
		{
			if (Entries[i].Enabled)
			{
				dictionary.TryAdd(Entries[i].Id, i);
			}
		}
		for (int j = 0; j < Entries.Count; j++)
		{
			LaunchEntry launchEntry = Entries[j];
			if (!launchEntry.Enabled || launchEntry.Mod.manifest?.dependencies == null)
			{
				continue;
			}
			foreach (ModDependency dependency in launchEntry.Mod.manifest.dependencies)
			{
				if (dependency.id == null || !dictionary.TryGetValue(dependency.id, out var value))
				{
					list.Add(new PlanViolation("dependencyDisabled:" + launchEntry.Key + ":" + dependency.id, MlmLocManager.Get("plan.validate.dependencyDisabled", ("mod", launchEntry.Name), ("dependency", dependency.id))));
				}
				else if (value > j)
				{
					list.Add(new PlanViolation("dependencyOrder:" + launchEntry.Key + ":" + dependency.id, MlmLocManager.Get("plan.validate.dependencyOrder", ("dependency", dependency.id), ("mod", launchEntry.Name))));
				}
			}
		}
		return list;
	}

	internal PlanResult Validate()
	{
		List<PlanViolation> list = Violations();
		if (list.Count != 0)
		{
			return PlanResult.Error(list[0].Message);
		}
		return PlanResult.Ok(MlmLocManager.Get("plan.validate.valid"));
	}

	private HashSet<string> ViolationKeys()
	{
		return (from violation in Violations()
			select violation.Key).ToHashSet<string>(StringComparer.Ordinal);
	}

	internal List<ModConflict> Conflicts()
	{
		Dictionary<string, LaunchEntry> dictionary = new Dictionary<string, LaunchEntry>(StringComparer.Ordinal);
		foreach (LaunchEntry item in Entries.Where((LaunchEntry entry) => entry.Enabled))
		{
			dictionary.TryAdd(item.Id, item);
		}
		List<ModConflict> list = new List<ModConflict>();
		foreach (LaunchEntry item2 in Entries.Where((LaunchEntry entry) => entry.Enabled))
		{
			foreach (ModIncompatibility declaredIncompatibility in item2.DeclaredIncompatibilities)
			{
				if (dictionary.TryGetValue(declaredIncompatibility.Id, out var value) && declaredIncompatibility.AppliesTo(value.Mod.manifest?.version))
				{
					list.Add(new ModConflict(item2, value, declaredIncompatibility));
				}
			}
		}
		return list;
	}

	internal List<ModConflict> ConflictsFor(LaunchEntry entry)
	{
		Dictionary<string, LaunchEntry> dictionary = BuildDependencyProviderLookup();
		List<ModConflict> list = new List<ModConflict>();
		foreach (ModIncompatibility declaredIncompatibility in entry.DeclaredIncompatibilities)
		{
			if (dictionary.TryGetValue(declaredIncompatibility.Id, out var value) && declaredIncompatibility.AppliesTo(value.Mod.manifest?.version))
			{
				list.Add(new ModConflict(entry, value, declaredIncompatibility));
			}
		}
		foreach (LaunchEntry item in dictionary.Values.Where((LaunchEntry candidate) => !SameModId(candidate, entry)))
		{
			foreach (ModIncompatibility declaredIncompatibility2 in item.DeclaredIncompatibilities)
			{
				if (string.Equals(declaredIncompatibility2.Id, entry.Id, StringComparison.Ordinal) && declaredIncompatibility2.AppliesTo(entry.Mod.manifest?.version))
				{
					list.Add(new ModConflict(item, entry, declaredIncompatibility2));
				}
			}
		}
		return list;
	}

	internal LaunchEntry? CopyInUse(string id)
	{
		return BuildDependencyProviderLookup().GetValueOrDefault(id);
	}

	internal HashSet<string> ConflictPairKeys()
	{
		return (from conflict in Conflicts()
			select conflict.PairKey).ToHashSet<string>(StringComparer.Ordinal);
	}

	internal PlanResult WarnAboutNewConflicts(PlanResult result, IReadOnlyCollection<string> before)
	{
		if (!result.Success)
		{
			return result;
		}
		List<ModConflict> list = (from conflict in Conflicts()
			where !before.Contains(conflict.PairKey)
			select conflict).DistinctBy((ModConflict conflict) => conflict.PairKey).ToList();
		if (list.Count == 0)
		{
			return result;
		}
		string item = ((list.Count == 1) ? MlmLocManager.Get("plan.conflicts.introducedOne", ("first", list[0].Declarer.Name), ("second", list[0].Target.Name)) : MlmLocManager.Get("plan.conflicts.introducedMany", ("count", list.Count)));
		return PlanResult.Warning(MlmLocManager.Get("plan.result.withWarning", ("message", result.Message), ("warning", item)));
	}

	private PlanResult? IntroducedViolation(IReadOnlyCollection<string> before)
	{
		PlanViolation planViolation = PlanViolationDiff.FirstIntroduced(Violations(), before);
		if ((object)planViolation == null)
		{
			return null;
		}
		return PlanResult.Error(planViolation.Message);
	}

	private void NormalizeBootstrapOrder()
	{
		int num = Entries.FindIndex((LaunchEntry entry) => entry.IsManager);
		if (num > 0)
		{
			LaunchEntry item = Entries[num];
			Entries.RemoveAt(num);
			Entries.Insert(0, item);
		}
	}
}

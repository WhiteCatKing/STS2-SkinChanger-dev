using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ModLaunchManager;

internal sealed class ModOrganizationDocument
{
	[JsonPropertyName("version")]
	public int Version { get; set; } = 1;

	[JsonPropertyName("mods")]
	public Dictionary<string, ModOrganizationEntry> Mods { get; set; } = new Dictionary<string, ModOrganizationEntry>(StringComparer.OrdinalIgnoreCase);

	[JsonPropertyName("groups")]
	public List<string> Groups { get; set; } = new List<string>();

	[JsonPropertyName("groupParents")]
	public Dictionary<string, string> Parents { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	internal ModOrganizationDocument Clone()
	{
		ModOrganizationDocument modOrganizationDocument = new ModOrganizationDocument
		{
			Version = Version
		};
		modOrganizationDocument.Groups.AddRange(Groups);
		string key2;
		foreach (KeyValuePair<string, string> parent in Parents)
		{
			parent.Deconstruct(out var key, out key2);
			string key3 = key;
			string value = key2;
			modOrganizationDocument.Parents[key3] = value;
		}
		foreach (KeyValuePair<string, ModOrganizationEntry> mod in Mods)
		{
			mod.Deconstruct(out key2, out var value2);
			string key4 = key2;
			ModOrganizationEntry modOrganizationEntry = value2;
			modOrganizationDocument.Mods[key4] = new ModOrganizationEntry
			{
				Alias = modOrganizationEntry.Alias,
				Group = modOrganizationEntry.Group,
				Description = modOrganizationEntry.Description,
				IgnorePriority = modOrganizationEntry.IgnorePriority
			};
		}
		return modOrganizationDocument;
	}

	internal List<string> KnownGroups()
	{
		List<string> list = new List<string>(Groups);
		foreach (ModOrganizationEntry value in Mods.Values)
		{
			string text = value.Group.Trim();
			if (text.Length > 0 && !list.Contains<string>(text, StringComparer.OrdinalIgnoreCase))
			{
				list.Add(text);
			}
		}
		return list;
	}

	internal string CanonicalGroup(string? name)
	{
		string value = name?.Trim() ?? string.Empty;
		string text;
		if (value.Length != 0)
		{
			text = KnownGroups().FirstOrDefault((string known) => string.Equals(known, value, StringComparison.OrdinalIgnoreCase));
			if (text == null)
			{
				return value;
			}
		}
		else
		{
			text = string.Empty;
		}
		return text;
	}

	private string RawParent(string group)
	{
		if (!Parents.TryGetValue(group.Trim(), out string value))
		{
			return string.Empty;
		}
		return value?.Trim() ?? string.Empty;
	}

	internal bool IsWithin(string group, string ancestor)
	{
		string b = ancestor.Trim();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string text = group.Trim();
		while (text.Length > 0 && hashSet.Add(text))
		{
			if (string.Equals(text, b, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			text = RawParent(text);
		}
		return false;
	}

	internal string ParentOf(string group)
	{
		string parent = RawParent(group);
		if (parent.Length == 0)
		{
			return string.Empty;
		}
		string text = KnownGroups().FirstOrDefault((string name) => string.Equals(name, parent, StringComparison.OrdinalIgnoreCase));
		if (text != null && !IsWithin(text, group))
		{
			return text;
		}
		return string.Empty;
	}

	internal List<(string Name, int Depth)> GroupTree()
	{
		List<string> order = KnownGroups();
		Dictionary<string, string> parents = ((IEnumerable<string>)order).ToDictionary((Func<string, string>)((string name) => name), (Func<string, string>)ParentOf, (IEqualityComparer<string>?)StringComparer.OrdinalIgnoreCase);
		List<(string Name, int Depth)> tree = new List<(string, int)>();
		Visit(string.Empty, 0);
		return tree;
		void Visit(string parent, int depth)
		{
			foreach (string item in order.Where((string name) => string.Equals(parents[name], parent, StringComparison.OrdinalIgnoreCase)))
			{
				tree.Add((item, depth));
				Visit(item, depth + 1);
			}
		}
	}

	internal bool SetParent(string group, string parent)
	{
		List<string> list = KnownGroups();
		string name = CanonicalGroup(group);
		string text = CanonicalGroup(parent);
		if (!list.Contains<string>(name, StringComparer.OrdinalIgnoreCase) || (text.Length > 0 && (!list.Contains<string>(text, StringComparer.OrdinalIgnoreCase) || IsWithin(text, name))) || string.Equals(ParentOf(name), text, StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		if (text.Length == 0)
		{
			Parents.Remove(name);
		}
		else
		{
			Parents[name] = text;
		}
		list.RemoveAll((string other) => string.Equals(other, name, StringComparison.OrdinalIgnoreCase));
		list.Add(name);
		Groups.Clear();
		Groups.AddRange(list);
		return true;
	}

	internal bool AddGroup(string name)
	{
		string text = name.Trim();
		if (text.Length == 0 || Groups.Contains<string>(text, StringComparer.OrdinalIgnoreCase))
		{
			return false;
		}
		Groups.Add(text);
		return true;
	}

	internal bool MoveGroup(string name, string? target, bool after)
	{
		List<string> list = KnownGroups();
		List<string> list2 = new List<string>(list);
		int num = list2.FindIndex((string group) => string.Equals(group, name.Trim(), StringComparison.OrdinalIgnoreCase));
		if (num < 0)
		{
			return false;
		}
		string text = list2[num];
		string text2 = string.Empty;
		if (target != null)
		{
			if (IsWithin(target, text))
			{
				return false;
			}
			text2 = ParentOf(target);
		}
		list2.RemoveAt(num);
		int index = list2.Count;
		if (target != null)
		{
			int num2 = list2.FindIndex((string group) => string.Equals(group, target.Trim(), StringComparison.OrdinalIgnoreCase));
			if (num2 < 0)
			{
				return false;
			}
			index = (after ? (num2 + 1) : num2);
		}
		list2.Insert(index, text);
		if (string.Equals(ParentOf(text), text2, StringComparison.OrdinalIgnoreCase) && list2.SequenceEqual<string>(list, StringComparer.Ordinal))
		{
			return false;
		}
		if (text2.Length == 0)
		{
			Parents.Remove(text);
		}
		else
		{
			Parents[text] = text2;
		}
		Groups.Clear();
		Groups.AddRange(list2);
		return true;
	}

	internal void SetGroup(string modId, string group)
	{
		Update(modId, delegate(ModOrganizationEntry metadata)
		{
			metadata.Group = group.Trim();
		});
	}

	internal void SetAlias(string modId, string alias)
	{
		Update(modId, delegate(ModOrganizationEntry metadata)
		{
			metadata.Alias = alias.Trim();
		});
	}

	internal void SetDescription(string modId, string description)
	{
		Update(modId, delegate(ModOrganizationEntry metadata)
		{
			metadata.Description = description.Trim();
		});
	}

	internal void SetIgnorePriority(string modId, bool ignore)
	{
		Update(modId, delegate(ModOrganizationEntry metadata)
		{
			metadata.IgnorePriority = ignore;
		});
	}

	internal bool IgnoresPriority(string modId)
	{
		if (Mods.TryGetValue(modId.Trim(), out ModOrganizationEntry value))
		{
			return value.IgnorePriority;
		}
		return false;
	}

	internal int RenameGroup(string from, string to)
	{
		string source = from.Trim();
		string text = to.Trim();
		if (source.Length == 0 || text.Length == 0)
		{
			return 0;
		}
		int num = Groups.FindIndex((string group) => string.Equals(group, source, StringComparison.OrdinalIgnoreCase));
		if (num >= 0)
		{
			Groups[num] = text;
		}
		else
		{
			AddGroup(text);
		}
		List<string> list = new List<string>();
		foreach (string group in Groups)
		{
			if (!list.Contains<string>(group, StringComparer.OrdinalIgnoreCase))
			{
				list.Add(group);
			}
		}
		Groups.Clear();
		Groups.AddRange(list);
		if (Parents.Remove(source, out string value) && !Parents.ContainsKey(text))
		{
			Parents[text] = value;
		}
		foreach (string item in (from link in Parents
			where string.Equals(link.Value, source, StringComparison.OrdinalIgnoreCase)
			select link.Key).ToList())
		{
			Parents[item] = text;
		}
		foreach (string item2 in (from link in Parents
			where string.Equals(link.Key, link.Value, StringComparison.OrdinalIgnoreCase)
			select link.Key).ToList())
		{
			Parents.Remove(item2);
		}
		int num2 = 0;
		foreach (ModOrganizationEntry value2 in Mods.Values)
		{
			string a = value2.Group.Trim();
			if (string.Equals(a, source, StringComparison.OrdinalIgnoreCase))
			{
				value2.Group = text;
				num2++;
			}
			else if (string.Equals(a, text, StringComparison.OrdinalIgnoreCase))
			{
				value2.Group = text;
			}
		}
		return num2;
	}

	internal int DeleteGroup(string name)
	{
		string value = name.Trim();
		if (value.Length == 0)
		{
			return 0;
		}
		string text = ParentOf(value);
		foreach (string item in (from link in Parents
			where string.Equals(link.Value, value, StringComparison.OrdinalIgnoreCase)
			select link.Key).ToList())
		{
			if (text.Length == 0)
			{
				Parents.Remove(item);
			}
			else
			{
				Parents[item] = text;
			}
		}
		Parents.Remove(value);
		Groups.RemoveAll((string group) => string.Equals(group, value, StringComparison.OrdinalIgnoreCase));
		int num = 0;
		foreach (string item2 in Mods.Keys.ToList())
		{
			ModOrganizationEntry modOrganizationEntry = Mods[item2];
			if (string.Equals(modOrganizationEntry.Group.Trim(), value, StringComparison.OrdinalIgnoreCase))
			{
				modOrganizationEntry.Group = text;
				num++;
				if (IsEmpty(modOrganizationEntry))
				{
					Mods.Remove(item2);
				}
			}
		}
		return num;
	}

	private static bool IsEmpty(ModOrganizationEntry metadata)
	{
		if (metadata.Alias.Trim().Length == 0 && metadata.Group.Trim().Length == 0 && metadata.Description.Trim().Length == 0)
		{
			return !metadata.IgnorePriority;
		}
		return false;
	}

	private void Update(string modId, Action<ModOrganizationEntry> change)
	{
		string text = modId.Trim();
		if (text.Length != 0)
		{
			if (!Mods.TryGetValue(text, out ModOrganizationEntry value))
			{
				value = new ModOrganizationEntry();
			}
			change(value);
			if (IsEmpty(value))
			{
				Mods.Remove(text);
			}
			else
			{
				Mods[text] = value;
			}
		}
	}
}

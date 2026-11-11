using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModLaunchManager;

internal static class AiTranslationReferences
{
	private sealed record Candidate(string Source, string Target, HashSet<string> Grams, HashSet<string> Words, int Size);

	internal const int MaxTerms = 100;

	internal const int ExampleBudget = 7000;

	internal const int StoryBudget = 1500;

	internal const int MaxStoryExamples = 4;

	internal const int EarlierBudget = 2000;

	internal const int MaxEarlier = 12;

	private static readonly string[] MechanicFiles = new string[8] { "card_keywords.json", "powers.json", "static_hover_tips.json", "enchantments.json", "orbs.json", "afflictions.json", "intents.json", "modifiers.json" };

	private static readonly string[] NameFiles = new string[9] { "cards.json", "relics.json", "potions.json", "characters.json", "monsters.json", "encounters.json", "events.json", "ancients.json", "acts.json" };

	private static readonly HashSet<string> ExampleFiles;

	private static readonly HashSet<string> StoryFiles;

	private static readonly HashSet<string> RulesTables;

	private static readonly Regex Highlight;

	private static readonly Regex Markup;

	private static readonly Regex Word;

	private static readonly char[] Clause;

	private static readonly HashSet<string> Stop;

	private static readonly HashSet<string> NonLatinLanguages;

	private static readonly Regex EnglishWord;

	private static readonly HashSet<string> UninflectedLanguages;

	private static readonly Regex KeptToken;

	private static readonly Regex GainedInPoints;

	private static readonly Regex MidSentenceCapital;

	internal const int MaxRecurringTerms = 40;

	private static readonly Regex CapitalPhrase;

	private static readonly Regex LowercaseWord;

	internal const int MaxCanonCandidates = 240;

	internal static bool LeavesChoiceUntranslated(string language, string source, string translated)
	{
		if (!NonLatinLanguages.Contains(language) || !source.Contains('|'))
		{
			return false;
		}
		HashSet<string> original = new ProtectedTranslationText(source).ChoiceTexts.ToHashSet<string>(StringComparer.Ordinal);
		return new ProtectedTranslationText(translated).ChoiceTexts.Any((string choice) => original.Contains(choice) && EnglishWord.IsMatch(Markup.Replace(choice, " ")));
	}

	internal static void CheckHighlights(string language, string source, string translated, IEnumerable<TranslationReference> terms)
	{
		if (!UninflectedLanguages.Contains(language) || !Highlight.IsMatch(source))
		{
			return;
		}
		HashSet<string> wanted = (from m in Highlight.Matches(source)
			select m.Groups[1].Value).ToHashSet<string>(StringComparer.Ordinal);
		HashSet<string> shown = (from m in Highlight.Matches(TidySpacing(language, translated))
			select m.Groups[1].Value).ToHashSet<string>(StringComparer.Ordinal);
		List<TranslationReference> list = (from g in terms.Where((TranslationReference t) => wanted.Contains(t.Source) && t.Target.Length > 0 && !Markup.IsMatch(t.Target)).GroupBy<TranslationReference, string>((TranslationReference t) => t.Source, StringComparer.Ordinal)
			where !g.Any((TranslationReference t) => shown.Contains(t.Target))
			select g.First()).ToList();
		if (list.Count <= 0)
		{
			return;
		}
		throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.Highlight)
		{
			Fallback = translated,
			Fix = string.Join("; ", list.Select((TranslationReference t) => t.Source + " → " + t.Target))
		};
	}

	internal static void CheckMeasureWords(string language, string source, string translated)
	{
		bool flag = ((language == "zhs" || language == "zht") ? true : false);
		if (!flag || !source.Contains("stack", StringComparison.OrdinalIgnoreCase) || !translated.Contains('层') || !GainedInPoints.IsMatch(translated))
		{
			return;
		}
		throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.MeasureWord)
		{
			Fallback = translated
		};
	}

	internal static void CheckForeignWords(string language, string source, string translated)
	{
		if (NonLatinLanguages.Contains(language))
		{
			HashSet<string> known = Words(source).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
			List<string> list = (from word in Words(translated)
				where !known.Contains(word)
				select word).Distinct<string>(StringComparer.Ordinal).ToList();
			if (list.Count > 0)
			{
				throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.ForeignWord)
				{
					Fallback = translated,
					Fix = string.Join(", ", list)
				};
			}
		}
		static IEnumerable<string> Words(string text)
		{
			return from m in EnglishWord.Matches(KeptToken.Replace(new ProtectedTranslationText(text).Masked, " "))
				select m.Value;
		}
	}

	internal static List<string> RecurringTerms(IEnumerable<string> texts, IEnumerable<string> names, IEnumerable<string> known)
	{
		List<string> list = (from n in (from n in names
				select n.Trim() into n
				where n.Length > 2 && n.Length <= 100
				select n).Distinct<string>(StringComparer.OrdinalIgnoreCase)
			orderby n.Length descending
			select n).ToList();
		HashSet<string> skip = list.Concat(known.Select((string k) => k.Trim())).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Regex regex = ((list.Count == 0) ? null : new Regex(string.Join("|", list.Select(Regex.Escape)), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L)));
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (string text in texts)
		{
			string input = regex?.Replace(text, " ") ?? text;
			foreach (string item in (from m in Highlight.Matches(input)
				select m.Groups[1].Value.Trim()).Concat(from m in MidSentenceCapital.Matches(Markup.Replace(input, " "))
				select m.Value).ToHashSet<string>(StringComparer.Ordinal))
			{
				dictionary[item] = dictionary.GetValueOrDefault(item) + 1;
			}
		}
		return (from p in (from p in dictionary.Where(delegate(KeyValuePair<string, int> p)
				{
					if (p.Value >= 2)
					{
						int length = p.Key.Length;
						if (length >= 3 && length <= 40 && p.Key.Count((char c) => c == ' ') < 4 && !p.Key.Any(char.IsAsciiDigit))
						{
							return !skip.Contains(p.Key);
						}
					}
					return false;
				})
				orderby p.Value descending
				select p).ThenBy<KeyValuePair<string, int>, string>((KeyValuePair<string, int> p) => p.Key, StringComparer.Ordinal).Take(40)
			select p.Key).ToList();
	}

	internal static List<string> CanonCandidates(IEnumerable<string> texts, IEnumerable<string> known)
	{
		List<string> list = texts.Select((string text) => Markup.Replace(text, " ")).ToList();
		HashSet<string> hashSet = list.SelectMany((string text) => from m in LowercaseWord.Matches(text)
			select m.Value).ToHashSet<string>(StringComparer.Ordinal);
		HashSet<string> hashSet2 = known.Select((string k) => k.Trim()).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Dictionary<string, (int, int)> dictionary = new Dictionary<string, (int, int)>(StringComparer.Ordinal);
		foreach (string item in list)
		{
			Dictionary<string, bool> dictionary2 = new Dictionary<string, bool>(StringComparer.Ordinal);
			bool value;
			foreach (Match item2 in CapitalPhrase.Matches(item))
			{
				List<string> list2 = item2.Value.Split(' ').ToList();
				int num = 0;
				while (list2.Count > 0 && hashSet.Contains(list2[0].ToLowerInvariant()))
				{
					list2.RemoveAt(0);
					num++;
				}
				string text2 = string.Join(' ', list2);
				int length = text2.Length;
				value = ((length < 2 || length > 40) ? true : false);
				if (!value && !text2.Any(char.IsAsciiDigit) && !hashSet2.Contains(text2) && !text2.Contains("'s", StringComparison.Ordinal) && !text2.Contains("’s", StringComparison.Ordinal) && !list2.All((string word) => word.Length > 1 && !word.Any(char.IsLower)))
				{
					bool flag = num > 0 || list2.Count > 1 || !StartsSentence(item, item2.Index) || item.Trim() == text2;
					dictionary2[text2] = dictionary2.GetValueOrDefault(text2) || flag;
				}
			}
			foreach (KeyValuePair<string, bool> item3 in dictionary2)
			{
				item3.Deconstruct(out var key, out value);
				string key2 = key;
				bool flag2 = value;
				(int, int) valueOrDefault = dictionary.GetValueOrDefault(key2);
				dictionary[key2] = (valueOrDefault.Item1 + (flag2 ? 1 : 0), valueOrDefault.Item2 + 1);
			}
		}
		return (from p in (from p in dictionary
				where p.Value.Named > 0
				orderby p.Value.Seen descending
				select p).ThenBy<KeyValuePair<string, (int, int)>, string>((KeyValuePair<string, (int Named, int Seen)> p) => p.Key, StringComparer.Ordinal).Take(240)
			select p.Key).ToList();
	}

	private static bool StartsSentence(string text, int index)
	{
		for (int num = index - 1; num >= 0; num--)
		{
			bool flag = char.IsWhiteSpace(text[num]);
			if (!flag)
			{
				bool flag2;
				switch (text[num])
				{
				case '"':
				case '\'':
				case '(':
				case '‘':
				case '“':
					flag2 = true;
					break;
				default:
					flag2 = false;
					break;
				}
				flag = flag2;
			}
			if (!flag)
			{
				switch (text[num])
				{
				case '!':
				case '.':
				case ':':
				case '?':
				case '…':
				case '。':
					return true;
				default:
					return false;
				}
			}
		}
		return true;
	}

	internal static List<(string Table, string Key, string Text)> ModTexts(IReadOnlyDictionary<string, byte[]> original)
	{
		List<(string, string, string)> list = new List<(string, string, string)>();
		foreach (IGrouping<string, KeyValuePair<string, byte[]>> item in (from p in original
			group p by p.Key.Substring(p.Key.IndexOf('/') + 1)).OrderBy<IGrouping<string, KeyValuePair<string, byte[]>>, string>((IGrouping<string, KeyValuePair<string, byte[]>> g) => g.Key, StringComparer.Ordinal))
		{
			KeyValuePair<string, byte[]> keyValuePair = item.FirstOrDefault((KeyValuePair<string, byte[]> p) => p.Key.StartsWith("eng/", StringComparison.Ordinal));
			if (keyValuePair.Key == null)
			{
				keyValuePair = item.OrderBy<KeyValuePair<string, byte[]>, string>((KeyValuePair<string, byte[]> p) => p.Key, StringComparer.Ordinal).First();
			}
			foreach (var (text3, text4) in ModTranslationFiles.Parse(keyValuePair.Value))
			{
				if (!text3.StartsWith("EXTENSION.", StringComparison.Ordinal) && !string.IsNullOrWhiteSpace(text4))
				{
					list.Add((item.Key, text3, text4));
				}
			}
		}
		return list;
	}

	internal static List<TranslationReference> Terms(IReadOnlyList<VanillaTable> tables, string modText)
	{
		string[] array = (from m in Word.Matches(Markup.Replace(modText, " "))
			select m.Value).ToArray();
		Dictionary<string, int> exact = Phrases(array);
		Dictionary<string, int> folded = Phrases(array.Select((string w) => w.ToLowerInvariant()).ToArray());
		Dictionary<string, (string Source, string Target, int Priority, int Count)> found = new Dictionary<string, (string, string, int, int)>(StringComparer.Ordinal);
		string value;
		string key;
		foreach (VanillaTable item in tables.Where((VanillaTable t) => ((ReadOnlySpan<string>)MechanicFiles).Contains(t.File)))
		{
			foreach (KeyValuePair<string, string> item2 in item.Source)
			{
				item2.Deconstruct(out value, out key);
				string key2 = value;
				string source2 = key;
				if (IsName(key2, ".title") && item.Target.TryGetValue(key2, out string value2))
				{
					Offer(source2, value2, 0);
				}
			}
		}
		foreach (var (source3, target2) in HighlightedTerms(tables))
		{
			Offer(source3, target2, 1);
		}
		foreach (VanillaTable item3 in tables.Where((VanillaTable t) => ((ReadOnlySpan<string>)NameFiles).Contains(t.File)))
		{
			foreach (KeyValuePair<string, string> item4 in item3.Source)
			{
				item4.Deconstruct(out key, out value);
				string key3 = key;
				string source4 = value;
				if ((IsName(key3, ".title") || IsName(key3, ".name")) && item3.Target.TryGetValue(key3, out string value3))
				{
					Offer(source4, value3, 2);
				}
			}
		}
		return (from t in (from t in found.Values
				orderby t.Priority, t.Count descending
				select t).ThenBy<(string, string, int, int), string>(((string Source, string Target, int Priority, int Count) t) => t.Source, StringComparer.Ordinal).Take(100)
			select new TranslationReference(t.Source, t.Target)).ToList();
		void Offer(string source, string target, int priority)
		{
			source = source.Trim();
			target = target.Trim();
			int length = source.Length;
			bool flag = ((length < 2 || length > 40) ? true : false);
			bool flag2 = flag;
			if (!flag2)
			{
				int length2 = target.Length;
				bool flag3 = ((length2 > 60 || length2 == 0) ? true : false);
				flag2 = flag3;
			}
			if (!flag2 && source.IndexOfAny(new char[4] { '[', '{', '<', '\n' }) < 0 && target.IndexOfAny(new char[4] { '[', '{', '<', '\n' }) < 0)
			{
				string text = string.Join(' ', from m in Word.Matches(source)
					select m.Value);
				if (text.Length != 0 && text.Count((char c) => c == ' ') < 4)
				{
					string key4 = text.ToLowerInvariant();
					int num = ((priority < 2 && text.Contains(' ')) ? folded.GetValueOrDefault(key4) : exact.GetValueOrDefault(text));
					if (num > 0 && (!found.TryGetValue(key4, out (string, string, int, int) value4) || priority < value4.Item3))
					{
						found[key4] = (source, target, priority, num);
					}
				}
			}
		}
	}

	private static bool IsName(string key, string suffix)
	{
		if (key.EndsWith(suffix, StringComparison.Ordinal))
		{
			return key.IndexOf('.') == key.Length - suffix.Length;
		}
		return false;
	}

	private static IEnumerable<(string Source, string Target)> HighlightedTerms(IReadOnlyList<VanillaTable> tables)
	{
		Dictionary<(string, string), int> pairs = new Dictionary<(string, string), int>();
		Dictionary<string, int> sources = new Dictionary<string, int>(StringComparer.Ordinal);
		Dictionary<string, int> targets = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (VanillaTable table in tables)
		{
			foreach (var (key, text3) in table.Source)
			{
				if (!text3.Contains("[gold]", StringComparison.Ordinal) || !table.Target.TryGetValue(key, out string value))
				{
					continue;
				}
				HashSet<string> hashSet = (from m in Highlight.Matches(text3)
					select m.Groups[1].Value).ToHashSet<string>(StringComparer.Ordinal);
				HashSet<string> hashSet2 = (from m in Highlight.Matches(value)
					select m.Groups[1].Value).ToHashSet<string>(StringComparer.Ordinal);
				if (hashSet.Count == 0 || hashSet2.Count == 0)
				{
					continue;
				}
				foreach (string item in hashSet)
				{
					sources[item] = sources.GetValueOrDefault(item) + 1;
				}
				foreach (string item2 in hashSet2)
				{
					targets[item2] = targets.GetValueOrDefault(item2) + 1;
				}
				foreach (string item3 in hashSet)
				{
					foreach (string item4 in hashSet2)
					{
						pairs[(item3, item4)] = pairs.GetValueOrDefault((item3, item4)) + 1;
					}
				}
			}
		}
		return (from g in pairs.Where<KeyValuePair<(string, string), int>>((KeyValuePair<(string, string), int> p) => p.Value >= 2).GroupBy<KeyValuePair<(string, string), int>, string>((KeyValuePair<(string, string), int> p) => p.Key.Item1, StringComparer.Ordinal)
			select g.OrderByDescending((KeyValuePair<(string, string), int> p) => p.Value).ThenBy<KeyValuePair<(string, string), int>, string>((KeyValuePair<(string, string), int> p) => p.Key.Item2, StringComparer.Ordinal).Take(2)
				.ToList() into top
			where top.Count == 1 || top[1].Value < top[0].Value
			select top[0].Key into p
			where 2.0 * (double)pairs[p] / (double)(sources[p.Item1] + targets[p.Item2]) >= 0.6
			select p).OrderBy<(string, string), string>(((string, string) p) => p.Item1, StringComparer.Ordinal);
	}

	private static Dictionary<string, int> Phrases(string[] words)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		for (int i = 0; i < words.Length; i++)
		{
			for (int j = 1; j <= 4 && i + j <= words.Length; j++)
			{
				string key = string.Join(' ', words, i, j);
				dictionary[key] = dictionary.GetValueOrDefault(key) + 1;
			}
		}
		return dictionary;
	}

	internal static List<TranslationReference> Examples(IReadOnlyList<VanillaTable> tables, IEnumerable<(string Table, string Key, string Text)> modTexts)
	{
		List<(string, string, string)> list = modTexts.Where<(string, string, string)>(((string Table, string Key, string Text) t) => !t.Key.EndsWith("title", StringComparison.OrdinalIgnoreCase) && !t.Key.EndsWith("name", StringComparison.OrdinalIgnoreCase)).ToList();
		Dictionary<string, int> wanted = new Dictionary<string, int>(StringComparer.Ordinal);
		foreach (var item in list)
		{
			foreach (string item2 in Grams(Readable(item.Item3)))
			{
				wanted[item2] = wanted.GetValueOrDefault(item2) + 1;
			}
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>(StringComparer.Ordinal);
		List<(string, string, string)> list2 = list.Where<(string, string, string)>(((string Table, string Key, string Text) t) => !RulesTables.Contains(t.Table) || t.Key.EndsWith("flavor", StringComparison.Ordinal)).ToList();
		if (list2.Sum<(string, string, string)>(((string Table, string Key, string Text) t) => t.Text.Length) >= 200)
		{
			foreach (var item3 in list2)
			{
				foreach (string item4 in ContentWords(Readable(item3.Item3)))
				{
					dictionary[item4] = dictionary.GetValueOrDefault(item4) + 1;
				}
			}
		}
		List<Candidate> list3 = new List<Candidate>();
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		foreach (VanillaTable item5 in tables.Where((VanillaTable t) => ExampleFiles.Contains(t.File)))
		{
			foreach (KeyValuePair<string, string> item6 in item5.Source)
			{
				item6.Deconstruct(out var key, out var value);
				string text = key;
				string text2 = value;
				int length = text2.Length;
				bool flag = ((length < 12 || length > 400) ? true : false);
				if (flag || !item5.Target.TryGetValue(text, out string value2) || string.IsNullOrWhiteSpace(value2) || value2 == text2)
				{
					continue;
				}
				string text3 = Readable(text2);
				string text4 = Readable(value2);
				if (hashSet.Add(text3))
				{
					HashSet<string> hashSet2 = Grams(text3).Where(wanted.ContainsKey).ToHashSet<string>(StringComparer.Ordinal);
					HashSet<string> hashSet3 = (((StoryFiles.Contains(item5.File) || text.EndsWith("flavor", StringComparison.Ordinal)) && text3.Count((char c) => c == '#') <= 1 && text3.Length >= 40 && dictionary.Count > 0) ? ContentWords(text3).Where(dictionary.ContainsKey).ToHashSet<string>(StringComparer.Ordinal) : new HashSet<string>());
					if ((hashSet2.Count > 0 && text2.Length <= 300) || hashSet3.Count > 0)
					{
						list3.Add(new Candidate(text3, text4, hashSet2, hashSet3, text3.Length + text4.Length));
					}
				}
			}
		}
		List<TranslationReference> list4 = new List<TranslationReference>();
		Pick(list3.Where((Candidate c) => c.Words.Count > 0 && c.Size <= 800).ToList(), (Candidate c) => c.Words, (string _) => 1.0, 2.0, 0.0, 1500, 4, list4);
		Pick(list3.Where((Candidate c) => c.Grams.Count > 0 && c.Size <= 600).ToList(), (Candidate c) => c.Grams, (string gram) => Math.Sqrt(wanted[gram]) * (double)gram.Count((char ch) => ch == ' '), 0.0, 0.5, 7000, int.MaxValue, list4);
		return list4;
	}

	internal static List<TranslationReference> Earlier(IEnumerable<(string Source, string Target)> done, IEnumerable<string> batch)
	{
		HashSet<string> hashSet = new HashSet<string>(batch.SelectMany((string text) => Grams(Readable(text))), StringComparer.Ordinal);
		HashSet<string> hashSet2 = new HashSet<string>(StringComparer.Ordinal);
		List<Candidate> list = new List<Candidate>();
		foreach (var item3 in done)
		{
			string item = item3.Source;
			string item2 = item3.Target;
			string text2 = Readable(item);
			string text3 = Readable(item2);
			HashSet<string> hashSet3 = Grams(text2).Where(hashSet.Contains).ToHashSet<string>(StringComparer.Ordinal);
			if (hashSet3.Count > 0 && text2.Length + text3.Length <= 500 && hashSet2.Add(text2))
			{
				list.Add(new Candidate(text2, text3, hashSet3, new HashSet<string>(), text2.Length + text3.Length));
			}
		}
		List<TranslationReference> list2 = new List<TranslationReference>();
		Pick(list, (Candidate c) => c.Grams, (string gram) => gram.Count((char ch) => ch == ' '), 0.0, 0.2, 2000, 12, list2);
		return list2.Select((TranslationReference c) => c with
		{
			Kind = "earlier"
		}).ToList();
	}

	private static void Pick(List<Candidate> pool, Func<Candidate, HashSet<string>> features, Func<string, double> weight, double minGain, double minScore, int budget, int limit, List<TranslationReference> chosen)
	{
		HashSet<string> covered = new HashSet<string>(StringComparer.Ordinal);
		HashSet<Candidate> hashSet = new HashSet<Candidate>();
		int num = 0;
		for (int i = 0; i < limit; i++)
		{
			Candidate candidate = null;
			double num2 = minScore;
			foreach (Candidate item in pool)
			{
				if (num + item.Size <= budget && !hashSet.Contains(item))
				{
					double num3 = (from f in features(item)
						where !covered.Contains(f)
						select f).Sum(weight);
					double num4 = num3 / Math.Sqrt(item.Size);
					if (num3 >= minGain && num4 > num2)
					{
						candidate = item;
						num2 = num4;
					}
				}
			}
			if (candidate == null)
			{
				break;
			}
			hashSet.Add(candidate);
			covered.UnionWith(features(candidate));
			num += candidate.Size;
			chosen.Add(new TranslationReference(candidate.Source, candidate.Target, "example"));
		}
	}

	private static HashSet<string> Grams(string readable)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.Ordinal);
		string[] array = readable.ToLowerInvariant().Split(Clause);
		foreach (string input in array)
		{
			string[] array2 = (from m in Word.Matches(input)
				select m.Value).ToArray();
			for (int j = 2; j <= 4; j++)
			{
				for (int k = 0; k + j <= array2.Length; k++)
				{
					if (array2.Skip(k).Take(j).Any((string w) => !Stop.Contains(w)))
					{
						hashSet.Add(string.Join(' ', array2, k, j));
					}
				}
			}
		}
		return hashSet;
	}

	private static IEnumerable<string> ContentWords(string readable)
	{
		return (from m in Word.Matches(readable.ToLowerInvariant())
			select m.Value into w
			where w.Length >= 4 && !Stop.Contains(w) && w.Any(char.IsLetter)
			select w).Distinct<string>(StringComparer.Ordinal);
	}

	internal static string TidySpacing(string language, string text)
	{
		bool flag = ((language == "zhs" || language == "jpn") ? true : false);
		if (!flag || !text.Contains(' '))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		int num = 0;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			switch (c)
			{
			case '{':
				num++;
				break;
			case '}':
				num--;
				break;
			case ' ':
				if (num == 0)
				{
					int j;
					for (j = i; j < text.Length && text[j] == ' '; j++)
					{
					}
					if (IsCjk(VisibleBefore(text, i)) || IsCjk(VisibleAfter(text, j)) || SeparatorTouchesCjk(text, i, j))
					{
						i = j - 1;
						continue;
					}
				}
				break;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private static bool SeparatorTouchesCjk(string text, int start, int end)
	{
		bool flag = start >= 2;
		if (flag)
		{
			char c = text[start - 1];
			bool flag2 = ((c == '-' || c == '–') ? true : false);
			flag = flag2;
		}
		if (flag && text[start - 2] == ' ')
		{
			int num = start - 2;
			while (num > 0 && text[num - 1] == ' ')
			{
				num--;
			}
			return IsCjk(VisibleBefore(text, num));
		}
		flag = end + 1 < text.Length;
		if (flag)
		{
			char c = text[end];
			bool flag2 = ((c == '-' || c == '–') ? true : false);
			flag = flag2;
		}
		if (flag && text[end + 1] == ' ')
		{
			int i;
			for (i = end + 1; i < text.Length && text[i] == ' '; i++)
			{
			}
			return IsCjk(VisibleAfter(text, i));
		}
		return false;
	}

	private static char VisibleBefore(string text, int index)
	{
		int num = index - 1;
		while (num >= 0)
		{
			if (text[num] == ']')
			{
				int num2 = text.LastIndexOf('[', num);
				if (num2 >= 0)
				{
					num = num2;
					num--;
					continue;
				}
			}
			return text[num];
		}
		return '\0';
	}

	private static char VisibleAfter(string text, int index)
	{
		int num = index;
		while (num < text.Length)
		{
			if (text[num] == '[')
			{
				int num2 = text.IndexOf(']', num);
				if (num2 >= 0)
				{
					num = num2;
					num++;
					continue;
				}
			}
			return text[num];
		}
		return '\0';
	}

	private static bool IsCjk(char c)
	{
		if (c >= '㐀')
		{
			if (c >= '\uff00')
			{
				if (c <= '\uffef')
				{
					goto IL_0034;
				}
			}
			else if (c <= '鿿')
			{
				goto IL_0034;
			}
		}
		else if (c >= '\u3000' && c <= 'ヿ')
		{
			goto IL_0034;
		}
		return false;
		IL_0034:
		return true;
	}

	internal static string Readable(string text)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		while (num < text.Length)
		{
			char c = text[num];
			bool flag;
			switch (c)
			{
			case '{':
			{
				int num2 = num;
				int num3 = 0;
				do
				{
					if (text[num] == '{')
					{
						num3++;
					}
					else if (text[num] == '}')
					{
						num3--;
					}
					num++;
				}
				while (num < text.Length && num3 > 0);
				int num4 = num2;
				string text2 = text.Substring(num4, num - num4);
				int num5 = ((num3 == 0) ? ProtectedTranslationText.ChoiceHeadLength(text2) : 0);
				object value;
				if (num5 <= 0)
				{
					value = "#";
				}
				else
				{
					string text3 = text2;
					num4 = num5;
					value = "⟨" + Readable(text3.Substring(num4, text3.Length - 1 - num4)) + "⟩";
				}
				stringBuilder.Append((string?)value);
				continue;
			}
			case '\n':
			case '\r':
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				num += ((c != '\r' || num + 1 >= text.Length || text[num + 1] != '\n') ? 1 : 2);
				stringBuilder.Append(" / ");
			}
			else if ((c == '<' || c == '[') ? true : false)
			{
				int num6 = text.IndexOf((c == '[') ? ']' : '>', num);
				int num7 = text.IndexOf('\n', num);
				if (num6 > num && (num7 < 0 || num6 < num7))
				{
					num = num6 + 1;
					continue;
				}
				stringBuilder.Append(c);
				num++;
			}
			else if (char.IsAsciiDigit(c))
			{
				while (true)
				{
					flag = num < text.Length;
					if (flag)
					{
						bool flag2 = char.IsAsciiDigit(text[num]);
						if (!flag2)
						{
							char c2 = text[num];
							bool flag3 = ((c2 == ',' || c2 == '.') ? true : false);
							flag2 = flag3 && num + 1 < text.Length && char.IsAsciiDigit(text[num + 1]);
						}
						flag = flag2;
					}
					if (!flag)
					{
						break;
					}
					num++;
				}
				stringBuilder.Append('#');
			}
			else
			{
				stringBuilder.Append(c);
				num++;
			}
		}
		return stringBuilder.ToString();
	}

	static AiTranslationReferences()
	{
		string[] mechanicFiles = MechanicFiles;
		string[] nameFiles = NameFiles;
		int num = 0;
		string[] array = new string[1 + (mechanicFiles.Length + nameFiles.Length)];
		ReadOnlySpan<string> readOnlySpan = new ReadOnlySpan<string>(mechanicFiles);
		readOnlySpan.CopyTo(new Span<string>(array).Slice(num, readOnlySpan.Length));
		num += readOnlySpan.Length;
		ReadOnlySpan<string> readOnlySpan2 = new ReadOnlySpan<string>(nameFiles);
		readOnlySpan2.CopyTo(new Span<string>(array).Slice(num, readOnlySpan2.Length));
		num += readOnlySpan2.Length;
		array[num] = "epochs.json";
		ExampleFiles = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(array), StringComparer.Ordinal);
		StoryFiles = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[4] { "events.json", "ancients.json", "characters.json", "epochs.json" }), StringComparer.Ordinal);
		array = MechanicFiles;
		num = 0;
		nameFiles = new string[3 + array.Length];
		readOnlySpan2 = new ReadOnlySpan<string>(array);
		readOnlySpan2.CopyTo(new Span<string>(nameFiles).Slice(num, readOnlySpan2.Length));
		num += readOnlySpan2.Length;
		nameFiles[num] = "cards.json";
		num++;
		nameFiles[num] = "relics.json";
		num++;
		nameFiles[num] = "potions.json";
		RulesTables = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(nameFiles), StringComparer.Ordinal);
		Highlight = new Regex("\\[(?:gold|color=#?\\w+)\\]([^\\[\\]{}<>\\r\\n]+?)\\[/(?:gold|color)\\]", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		Markup = new Regex("\\[[^\\]\\r\\n]*\\]|<[^>\\r\\n]*>|\\{[^{}]*\\}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		Word = new Regex("[\\p{L}\\p{N}#]+(?:['’][\\p{L}]+)*", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		Clause = new char[13]
		{
			'/', '.', ',', ';', ':', '(', ')', '?', '!', '⟨',
			'⟩', '|', '"'
		};
		Stop = new HashSet<string>("the a an of to and in your you is it its this that for with on at by from be are as or if their they them he she his her was were has have had not no but so all".Split(' '), StringComparer.Ordinal);
		NonLatinLanguages = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "zhs", "zht", "jpn", "kor", "tha", "rus" }), StringComparer.Ordinal);
		EnglishWord = new Regex("[A-Za-z]{3,}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		UninflectedLanguages = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "zhs", "zht", "jpn", "kor", "tha" }), StringComparer.Ordinal);
		KeptToken = new Regex("__MLM_KEEP_\\d+__", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		GainedInPoints = new Regex("获得\\{[^{}]*\\}点", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		MidSentenceCapital = new Regex("(?<=[a-z,;] )[A-Z][a-z]{2,}(?: [A-Z][a-z]{2,}){0,2}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		CapitalPhrase = new Regex("(?<![\\p{L}\\p{N}'’])[A-Z][\\p{L}'’]*(?:(?:-no-|-| of the | of | )[A-Z][\\p{L}'’]*)*", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		LowercaseWord = new Regex("(?<![\\p{L}'’])[a-z][\\p{L}'’]*", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
	}
}

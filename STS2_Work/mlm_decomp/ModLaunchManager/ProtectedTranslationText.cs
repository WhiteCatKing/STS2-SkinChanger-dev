using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ModLaunchManager;

internal sealed class ProtectedTranslationText
{
	private sealed record Choice(int End, int Bars, bool Optional);

	private const string CodePattern = "(?=[A-Za-z0-9_-]*\\d)[A-Za-z][A-Za-z0-9]*(?:[-_][A-Za-z0-9]+)+(?![\\w-])";

	private const string InlinePattern = "\\[[^\\]\\r\\n]+\\]|<[^>\\r\\n]+>|https?://[^\\s<>|{}]+|%(?:\\d+\\$)?[sdif]|(?=[A-Za-z0-9_-]*\\d)[A-Za-z][A-Za-z0-9]*(?:[-_][A-Za-z0-9]+)+(?![\\w-])|\\d+(?:[.,]\\d+)*|__MLM_KEEP_\\d+__";

	private static readonly Regex Code = new Regex("^(?=[A-Za-z0-9_-]*\\d)[A-Za-z][A-Za-z0-9]*(?:[-_][A-Za-z0-9]+)+(?![\\w-])$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex Inline = new Regex("\\[[^\\]\\r\\n]+\\]|<[^>\\r\\n]+>|https?://[^\\s<>|{}]+|%(?:\\d+\\$)?[sdif]|(?=[A-Za-z0-9_-]*\\d)[A-Za-z][A-Za-z0-9]*(?:[-_][A-Za-z0-9]+)+(?![\\w-])|\\d+(?:[.,]\\d+)*|__MLM_KEEP_\\d+__", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex InlineAt = new Regex("\\G(?:\\[[^\\]\\r\\n]+\\]|<[^>\\r\\n]+>|https?://[^\\s<>|{}]+|%(?:\\d+\\$)?[sdif]|(?=[A-Za-z0-9_-]*\\d)[A-Za-z][A-Za-z0-9]*(?:[-_][A-Za-z0-9]+)+(?![\\w-])|\\d+(?:[.,]\\d+)*|__MLM_KEEP_\\d+__)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex ChoiceHead = new Regex("\\G\\{(?<name>[A-Za-z_][\\w.]*):(?:(?<formatter>plural|cond|choose|show)(?:\\([^(){}|:]*\\))?:)?(?![A-Za-z]\\w*(?:\\([^(){}]*\\))?:)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex Token = new Regex("__MLM_KEEP_\\d+__", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex NumberOnly = new Regex("^\\d+(?:[.,]\\d+)*$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private readonly List<string> _kept = new List<string>();

	private readonly List<string> _kinds = new List<string>();

	private readonly List<(int Block, int Branch)> _places = new List<(int, int)>();

	private readonly Dictionary<int, Choice> _choices = new Dictionary<int, Choice>();

	private readonly List<string> _choiceTexts = new List<string>();

	private readonly int _lineBreaks;

	private static readonly Regex PluralHead = new Regex("\\{(?<name>[A-Za-z_][\\w.]*):plural:", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly Regex ChooseOneHead = new Regex("^\\{(?<name>[A-Za-z_][\\w.]*):choose\\(1\\):$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));

	private static readonly HashSet<string> SingleFormLanguages = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "zhs", "zht", "jpn", "kor", "tha", "ind" }), StringComparer.Ordinal);

	private static readonly HashSet<string> ThreeFormLanguages = new HashSet<string>(new global::_003C_003Ez__ReadOnlyArray<string>(new string[2] { "rus", "pol" }), StringComparer.Ordinal);

	internal string Masked { get; }

	internal bool HasTokens => _kept.Count > 0;

	internal IReadOnlyList<string> ChoiceTexts => _choiceTexts;

	internal Dictionary<string, string> Tokens => _kinds.Select((string kind, int i) => (kind: kind, i: i)).ToDictionary<(string, int), string, string>(((string kind, int i) p) => TokenName(p.i), ((string kind, int i) p) => p.kind, StringComparer.Ordinal);

	internal ProtectedTranslationText(string text)
	{
		_lineBreaks = text.Count((char c) => c == '\n');
		StringBuilder stringBuilder = new StringBuilder();
		Mask(text, 0, text.Length, -1, stringBuilder);
		Masked = stringBuilder.ToString();
		KeyValuePair<int, Choice>[] array = _choices.ToArray();
		foreach (KeyValuePair<int, Choice> keyValuePair in array)
		{
			keyValuePair.Deconstruct(out var key, out var value2);
			int num = key;
			Choice choice = value2;
			string name = ChoiceHead.Match(_kept[num]).Groups["name"].Value;
			if (!(ChoiceHead.Match(_kept[num]).Groups["formatter"].Value != "plural") && choice.End == num + 1 && _kept.Where((string value, int i) => _kinds[i] == "value").Any((string value) => value.StartsWith("{" + name + ":", StringComparison.Ordinal) || value == "{" + name + "}"))
			{
				_choices[num] = choice with
				{
					Optional = true
				};
				_kinds[num] = "plural start, optional";
				_kinds[choice.End] = "plural end, optional";
			}
		}
	}

	private static string TokenName(int index)
	{
		return $"__MLM_KEEP_{index}__";
	}

	internal static string ForGamePlurals(string language, string text)
	{
		bool flag = SingleFormLanguages.Contains(language);
		if (!flag && !ThreeFormLanguages.Contains(language))
		{
			return text;
		}
		foreach (Match item in PluralHead.Matches(text).Reverse())
		{
			List<string> list = new List<string>();
			int num = 0;
			int num2 = item.Index + item.Length;
			for (int i = num2; i < text.Length; i++)
			{
				if (num < 0)
				{
					break;
				}
				if (text[i] == '{')
				{
					num++;
				}
				else if (text[i] == '}')
				{
					num--;
				}
				if (num < 0 || (num == 0 && text[i] == '|'))
				{
					string text2 = text;
					int num3 = num2;
					list.Add(text2.Substring(num3, i - num3));
					num2 = i + 1;
				}
			}
			if (num < 0 && list.Count == 2 && (!flag || !(list[0] == list[1])))
			{
				text = text.Substring(0, item.Index) + "{" + item.Groups["name"].Value + ":choose(1):" + text.Substring(item.Index + item.Length);
			}
		}
		return text;
	}

	private static string Comparable(string kept)
	{
		Match match = ChooseOneHead.Match(kept);
		if (!match.Success)
		{
			return kept;
		}
		return "{" + match.Groups["name"].Value + ":plural:";
	}

	private int Keep(StringBuilder result, string value, string kind, int block, int branch)
	{
		result.Append(TokenName(_kept.Count));
		_kept.Add(value);
		_kinds.Add(kind);
		_places.Add((block, branch));
		return _kept.Count - 1;
	}

	private int Mask(string text, int from, int to, int block, StringBuilder result)
	{
		int num = 0;
		int num2 = from;
		while (num2 < to)
		{
			if (text[num2] == '{')
			{
				int num3 = num2;
				int num4 = 0;
				do
				{
					if (text[num2] == '{')
					{
						num4++;
					}
					if (text[num2] == '}')
					{
						num4--;
					}
					num2++;
				}
				while (num2 < to && num4 > 0);
				if (num4 != 0)
				{
					throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.SourceBraces);
				}
				Match match = ChoiceHead.Match(text, num3, num2 - num3);
				if (match.Success && HasVariants(text, num3 + match.Length, num2 - 1))
				{
					int num5 = Keep(result, match.Value, "choice start", block, num);
					List<string> choiceTexts = _choiceTexts;
					int num6 = num3 + match.Length;
					choiceTexts.Add(text.Substring(num6, num2 - 1 - num6));
					int bars = Mask(text, num3 + match.Length, num2 - 1, num5, result);
					int end = Keep(result, "}", "choice end", block, num);
					_choices[num5] = new Choice(end, bars, Optional: false);
				}
				else
				{
					int num6 = num3;
					string value = text.Substring(num6, num2 - num6);
					num6 = num3;
					Keep(result, value, text.Substring(num6, num2 - num6).Contains("Icon", StringComparison.Ordinal) ? "icon" : "value", block, num);
				}
				continue;
			}
			char c = text[num2];
			if ((c == '\n' || c == '\r') ? true : false)
			{
				int num7 = num2++;
				if (text[num7] == '\r' && num2 < to && text[num2] == '\n')
				{
					num2++;
				}
				int num6 = num7;
				Keep(result, text.Substring(num6, num2 - num6), "line break", block, num);
				continue;
			}
			if (text[num2] == '}')
			{
				throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.SourceBraces);
			}
			if (text[num2] == '|' && block >= 0)
			{
				num++;
				result.Append('|');
				num2++;
				continue;
			}
			Match match2 = InlineAt.Match(text, num2, to - num2);
			if (match2.Success)
			{
				Keep(result, match2.Value, Kind(match2.Value), block, num);
				num2 += match2.Length;
			}
			else
			{
				result.Append(text[num2]);
				num2++;
			}
		}
		return num;
	}

	internal static int ChoiceHeadLength(string block)
	{
		Match match = ChoiceHead.Match(block);
		if (!match.Success || !HasVariants(block, match.Length, block.Length - 1))
		{
			return 0;
		}
		return match.Length;
	}

	private static bool HasVariants(string text, int from, int to)
	{
		int num = 0;
		for (int i = from; i < to; i++)
		{
			if (text[i] == '{')
			{
				num++;
			}
			else if (text[i] == '}')
			{
				num--;
			}
			else if (text[i] == '|' && num == 0)
			{
				return true;
			}
		}
		return false;
	}

	private static string Kind(string value)
	{
		if (Code.IsMatch(value))
		{
			return "code";
		}
		string result;
		switch (value[0])
		{
		case '<':
		case '[':
			result = ((value.Length > 1 && value[1] == '/') ? "markup end" : "markup start");
			break;
		case '%':
			result = "value";
			break;
		case '_':
			result = "text";
			break;
		case 'h':
			result = "link";
			break;
		default:
			result = "number";
			break;
		}
		return result;
	}

	internal string Restore(string translated)
	{
		if (Masked.Length > 0 && string.IsNullOrWhiteSpace(translated))
		{
			throw new TranslationFileException("response")
			{
				ReplyIssue = "empty"
			};
		}
		translated = UnwrapSingleForms(translated);
		string[] seen = (from m in Token.Matches(translated)
			select m.Value).ToArray();
		HashSet<string> expected = Enumerable.Range(0, _kept.Count).Select(TokenName).ToHashSet<string>(StringComparer.Ordinal);
		if (seen.Any((string token) => !expected.Contains(token)))
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.UnknownToken);
		}
		if (seen.Distinct<string>(StringComparer.Ordinal).Count() != seen.Length)
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.DuplicateToken);
		}
		HashSet<string> dropped = _choices.Where<KeyValuePair<int, Choice>>((KeyValuePair<int, Choice> c) => c.Value.Optional && !((ReadOnlySpan<string>)seen).Contains(TokenName(c.Key)) && !((ReadOnlySpan<string>)seen).Contains(TokenName(c.Value.End))).SelectMany((KeyValuePair<int, Choice> c) => new string[2]
		{
			TokenName(c.Key),
			TokenName(c.Value.End)
		}).ToHashSet<string>(StringComparer.Ordinal);
		List<int> list = (from i in Enumerable.Range(0, _kept.Count)
			where !((ReadOnlySpan<string>)seen).Contains(TokenName(i)) && !dropped.Contains(TokenName(i))
			select i).ToList();
		if (list.Count > 0)
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.MissingToken)
			{
				Fix = string.Join(", ", list.Select((int i) => TokenName(i) + " (" + _kinds[i] + ")")),
				MissingKinds = list.Select((int i) => _kinds[i]).ToArray()
			};
		}
		string text = Token.Replace(translated, "");
		if (text.Contains('\r') || text.Contains('\n'))
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.LineBreaks);
		}
		if (text.Count((char c) => c == '#') > Token.Replace(Masked, "").Count((char c) => c == '#'))
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.AddedSyntax);
		}
		if (text.Contains('{') || text.Contains('}') || Inline.IsMatch(text))
		{
			bool flag = !text.Contains('{') && !text.Contains('}') && Inline.Matches(text).All((Match m) => NumberOnly.IsMatch(m.Value));
			throw new TranslationFileException("invalidFormat", null, flag ? TranslationFormatIssue.AddedNumber : TranslationFormatIssue.AddedSyntax);
		}
		VerifyChoices(translated);
		string text2 = Token.Replace(translated, delegate(Match m)
		{
			List<string> kept = _kept;
			string value = m.Value;
			return kept[int.Parse(value.Substring(11, value.Length - 2 - 11), CultureInfo.InvariantCulture)];
		});
		if (text2.Count((char c) => c == '\n') != _lineBreaks)
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.LineBreaks);
		}
		try
		{
			VerifyEquivalent(new ProtectedTranslationText(text2));
			return text2;
		}
		catch (TranslationFileException)
		{
			throw new TranslationFileException("invalidFormat", null, TranslationFormatIssue.ChoiceBlock);
		}
	}

	private string UnwrapSingleForms(string translated)
	{
		foreach (KeyValuePair<int, Choice> item in _choices.Where<KeyValuePair<int, Choice>>((KeyValuePair<int, Choice> c) => c.Value.Optional))
		{
			item.Deconstruct(out var key, out var value);
			int index = key;
			Choice choice = value;
			string text = TokenName(index);
			string text2 = TokenName(choice.End);
			int num = translated.IndexOf(text, StringComparison.Ordinal);
			int num2 = translated.IndexOf(text2, StringComparison.Ordinal);
			if (num >= 0 && num2 >= num && translated.IndexOf(text, num + 1, StringComparison.Ordinal) < 0 && translated.IndexOf(text2, num2 + 1, StringComparison.Ordinal) < 0)
			{
				string text3 = translated;
				key = num + text.Length;
				string text4 = text3.Substring(key, num2 - key);
				if (!text4.Contains('|') && !Token.IsMatch(text4))
				{
					translated = translated.Substring(0, num) + text4 + translated.Substring(num2 + text2.Length);
				}
			}
		}
		return translated;
	}

	private void VerifyChoices(string translated)
	{
		List<(int Head, int Bars)> open = new List<(int, int)>();
		HashSet<int> hashSet = _choices.Values.Select((Choice c) => c.End).ToHashSet();
		int position = 0;
		foreach (Match item2 in Token.Matches(translated))
		{
			CountBars(item2.Index);
			position = item2.Index + item2.Length;
			string value = item2.Value;
			int num = int.Parse(value.Substring(11, value.Length - 2 - 11), CultureInfo.InvariantCulture);
			(int Head, int Bars) obj;
			if (open.Count != 0)
			{
				List<(int Head, int Bars)> list = open;
				obj = list[list.Count - 1];
			}
			else
			{
				obj = (Head: -1, Bars: 0);
			}
			(int, int) tuple = obj;
			if (hashSet.Contains(num))
			{
				if (open.Count != 0)
				{
					Dictionary<int, Choice> choices = _choices;
					List<(int Head, int Bars)> list2 = open;
					if (choices[list2[list2.Count - 1].Head].End == num)
					{
						Dictionary<int, Choice> choices2 = _choices;
						List<(int Head, int Bars)> list3 = open;
						Choice choice = choices2[list3[list3.Count - 1].Head];
						bool num2;
						if (!choice.Optional)
						{
							List<(int Head, int Bars)> list4 = open;
							num2 = list4[list4.Count - 1].Bars != choice.Bars;
						}
						else
						{
							List<(int Head, int Bars)> list5 = open;
							num2 = list5[list5.Count - 1].Bars < 1;
						}
						if (num2)
						{
							throw ChoiceError();
						}
						open.RemoveAt(open.Count - 1);
						continue;
					}
				}
				throw ChoiceError();
			}
			(int, int) tuple2 = _places[num];
			(int, int) tuple3 = tuple;
			if (tuple2.Item1 != tuple3.Item1 || tuple2.Item2 != tuple3.Item2)
			{
				throw ChoiceError();
			}
			if (_choices.ContainsKey(num))
			{
				open.Add((num, 0));
			}
		}
		if (open.Count > 0)
		{
			throw ChoiceError();
		}
		void CountBars(int end)
		{
			for (; position < end; position++)
			{
				if (translated[position] == '|' && open.Count > 0)
				{
					List<(int Head, int Bars)> list6 = open;
					int index = list6.Count - 1;
					List<(int Head, int Bars)> list7 = open;
					int item = list7[list7.Count - 1].Head;
					List<(int Head, int Bars)> list8 = open;
					list6[index] = (item, list8[list8.Count - 1].Bars + 1);
				}
			}
		}
	}

	private static TranslationFileException ChoiceError()
	{
		return new TranslationFileException("invalidFormat", null, TranslationFormatIssue.ChoiceBlock);
	}

	internal void VerifyEquivalent(ProtectedTranslationText other)
	{
		if (_lineBreaks != other._lineBreaks)
		{
			throw new TranslationFileException("invalidFormat");
		}
		Dictionary<string, int> dictionary = other._kept.GroupBy<string, string>(Comparable, StringComparer.Ordinal).ToDictionary<IGrouping<string, string>, string, int>((IGrouping<string, string> g) => g.Key, (IGrouping<string, string> g) => g.Count(), StringComparer.Ordinal);
		List<int> list = new List<int>();
		for (int j = 0; j < _kept.Count; j++)
		{
			if (dictionary.TryGetValue(Comparable(_kept[j]), out var value) && value > 0)
			{
				dictionary[Comparable(_kept[j])] = value - 1;
			}
			else
			{
				list.Add(j);
			}
		}
		Choice value2;
		int num = list.Count((int i) => _choices.TryGetValue(i, out value2) && value2.Optional);
		if (dictionary.Values.Any((int count) => count > 0) || list.Any((int i) => (!_choices.TryGetValue(i, out Choice value3) || !value3.Optional) && _kept[i] != "}") || list.Count((int i) => _kept[i] == "}" && !_choices.ContainsKey(i)) != num)
		{
			throw new TranslationFileException("invalidFormat");
		}
	}
}

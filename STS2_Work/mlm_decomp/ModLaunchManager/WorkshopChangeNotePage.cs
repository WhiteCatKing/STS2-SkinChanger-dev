using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace ModLaunchManager;

internal sealed record WorkshopChangeNotePage(WorkshopChangeNotePageKind Kind, IReadOnlyList<WorkshopChangeNote> Notes, bool HasNextPage)
{
	private const string EntryMarker = "changeLogCtn";

	private static readonly Regex Paragraph = new Regex("<p id=\"(\\d{1,10})\"[^>]*>(.*?)</p>", RegexOptions.Singleline | RegexOptions.CultureInvariant);

	private static readonly Regex Versions = new Regex("author_snapshot_versions\"[^>]*>(.*?)</div>", RegexOptions.Singleline | RegexOptions.CultureInvariant);

	private static readonly Regex Span = new Regex("<span[^>]*>(.*?)</span>", RegexOptions.Singleline | RegexOptions.CultureInvariant);

	private const char SoftBreak = '\u0001';

	private const string Bullet = "• ";

	private static readonly Regex RawSpace = new Regex("[\\r\\n\\t]+", RegexOptions.CultureInvariant);

	private static readonly Regex ItemBreak = new Regex("<br\\s*/?>\\s*(?=</li>)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex LineBreak = new Regex("<br\\s*/?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex ListItem = new Regex("<li(?:\\s[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex Block = new Regex("</?(?:div|ul|ol|li|h\\d|blockquote|p)(?:\\s[^>]*)?>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

	private static readonly Regex Tag = new Regex("<[^>]*>", RegexOptions.Singleline | RegexOptions.CultureInvariant);

	private static readonly Regex SoftBreaks = new Regex("\\n?(?:[ ]*\\u0001[ ]*)+\\n?", RegexOptions.CultureInvariant);

	private static readonly Regex BlankLines = new Regex("\\n{3,}", RegexOptions.CultureInvariant);

	internal static WorkshopChangeNotePage Parse(string html, int page)
	{
		int num = html.IndexOf("changeLogCtn", StringComparison.Ordinal);
		if (num < 0)
		{
			if (html.Contains("class=\"error_ctn\"", StringComparison.Ordinal))
			{
				return new WorkshopChangeNotePage(WorkshopChangeNotePageKind.Unavailable, Array.Empty<WorkshopChangeNote>(), HasNextPage: false);
			}
			if (!html.Contains("workshopItemTitle", StringComparison.Ordinal))
			{
				return new WorkshopChangeNotePage(WorkshopChangeNotePageKind.Unrecognized, Array.Empty<WorkshopChangeNote>(), HasNextPage: false);
			}
			return new WorkshopChangeNotePage(WorkshopChangeNotePageKind.Notes, Array.Empty<WorkshopChangeNote>(), HasNextPage: false);
		}
		List<WorkshopChangeNote> list = new List<WorkshopChangeNote>();
		int num2 = num;
		while (num2 >= 0)
		{
			int num3 = html.IndexOf("changeLogCtn", num2 + "changeLogCtn".Length, StringComparison.Ordinal);
			string text;
			if (num3 >= 0)
			{
				int num4 = num2;
				text = html.Substring(num4, num3 - num4);
			}
			else
			{
				text = html.Substring(num2);
			}
			string input = text;
			Match match = Paragraph.Match(input);
			if (match.Success && uint.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var result) && result != 0)
			{
				Match match2 = Versions.Match(input, 0, match.Index);
				IReadOnlyList<string> branches = (match2.Success ? (from span in Span.Matches(match2.Groups[1].Value)
					select Decode(span.Groups[1].Value).Trim() into name
					where name.Length > 0
					select name).ToArray() : Array.Empty<string>());
				list.Add(new WorkshopChangeNote(result, ToPlainText(match.Groups[2].Value), branches));
			}
			num2 = num3;
		}
		if (list.Count == 0)
		{
			return new WorkshopChangeNotePage(WorkshopChangeNotePageKind.Unrecognized, Array.Empty<WorkshopChangeNote>(), HasNextPage: false);
		}
		bool hasNextPage = Regex.IsMatch(html, $"changelog/\\d+\\?(?:[^\"'<>]*&(?:amp;)?)?p={page + 1}(?![0-9])", RegexOptions.CultureInvariant);
		return new WorkshopChangeNotePage(WorkshopChangeNotePageKind.Notes, list.OrderByDescending((WorkshopChangeNote note) => note.Time).ToArray(), hasNextPage);
	}

	internal static string ToPlainText(string html)
	{
		string input = RawSpace.Replace(html, " ");
		input = ItemBreak.Replace(input, string.Empty);
		input = LineBreak.Replace(input, "\n");
		input = ListItem.Replace(input, "\u0001• ");
		input = Block.Replace(input, '\u0001'.ToString());
		input = Decode(Tag.Replace(input, string.Empty)).Replace('\u00a0', ' ');
		input = SoftBreaks.Replace(input, "\n");
		string[] value = (from line in input.Split('\n')
			select line.Trim() into line
			select (!line.StartsWith("• ", StringComparison.Ordinal)) ? line : ("• " + line.Substring("• ".Length).TrimStart())).ToArray();
		return BlankLines.Replace(string.Join("\n", value), "\n\n").Trim();
	}

	private static string Decode(string text)
	{
		return WebUtility.HtmlDecode(text);
	}

	[CompilerGenerated]
	private WorkshopChangeNotePage(WorkshopChangeNotePage original)
	{
		Kind = original.Kind;
		Notes = original.Notes;
		HasNextPage = original.HasNextPage;
	}
}

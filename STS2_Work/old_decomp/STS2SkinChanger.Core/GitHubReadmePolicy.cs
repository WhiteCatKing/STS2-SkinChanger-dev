using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal static class GitHubReadmePolicy
{
	public const int MaxExcerptChars = 800;

	private const int MaxInputChars = 65536;

	private static readonly Regex Fences = new Regex("```.*?```|~~~.*?~~~", RegexOptions.Singleline);

	private static readonly Regex Comments = new Regex("<!--.*?-->", RegexOptions.Singleline);

	private static readonly Regex Images = new Regex("!\\[[^\\]]*\\]\\([^)]*\\)", RegexOptions.Singleline);

	private static readonly Regex Links = new Regex("\\[([^\\]]*)\\]\\([^)]*\\)", RegexOptions.Singleline);

	private static readonly Regex Html = new Regex("<[^>]+>", RegexOptions.Singleline);

	private static readonly Regex Ordered = new Regex("^\\d+[.)]\\s+");

	private static readonly Regex Spaces = new Regex("\\s+");

	public static string ToExcerpt(string? markdown, int maxChars)
	{
		if (maxChars <= 0 || string.IsNullOrWhiteSpace(markdown))
		{
			return "";
		}
		string text = ((markdown.Length > 65536) ? markdown.Substring(0, 65536) : markdown);
		text = text.Replace("\r\n", "\n").Replace('\r', '\n');
		text = Fences.Replace(text, " ");
		text = Comments.Replace(text, " ");
		text = Images.Replace(text, " ");
		text = Links.Replace(text, "$1");
		text = Html.Replace(text, " ");
		List<string> list = new List<string>();
		string[] array = text.Split('\n');
		foreach (string text2 in array)
		{
			string text3 = Spaces.Replace(text2.Replace("|", " "), " ").Trim();
			if (text3.Length != 0 && !text3.All(delegate(char c)
			{
				switch (c)
				{
				case ' ':
				case '#':
				case '*':
				case '+':
				case '-':
				case ':':
				case '=':
				case '>':
				case '_':
				case '~':
					return true;
				default:
					return false;
				}
			}))
			{
				text3 = text3.TrimStart('#', '>', '-', '*', '+', '=');
				text3 = Ordered.Replace(text3, "");
				text3 = text3.Replace("**", "").Replace("__", "").Replace("`", "")
					.Replace("~~", "")
					.Trim();
				if (text3.Length > 0)
				{
					list.Add(text3);
				}
			}
		}
		string text4 = string.Join(" ", list);
		if (text4.Length <= maxChars)
		{
			return text4;
		}
		string text5 = text4.Substring(0, maxChars);
		int num = text5.LastIndexOf(' ');
		if (num >= maxChars / 2)
		{
			text5 = text5.Substring(0, num);
		}
		return text5.TrimEnd();
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace STS2SkinChanger.Core;

internal static class WorkshopCodeDiagnostics
{
	internal static ulong[] MetadataIds(IEnumerable<WorkshopCatalogItem> catalog, IEnumerable<WorkshopCodeIssue> issues)
	{
		return (from id in catalog.Select((WorkshopCatalogItem item) => item.Id).Concat(issues.Select((WorkshopCodeIssue issue) => issue.Id))
			where id != 0
			select id).Distinct().ToArray();
	}

	internal static WorkshopCodeProblem[] Group(IEnumerable<WorkshopCodeIssue> issues)
	{
		return (from i in issues
			group i by (i.Id == 0) ? ("code:" + i.Key) : ("id:" + i.Id) into g
			select new WorkshopCodeProblem(g.Key, g.First().Id, g.Select((WorkshopCodeIssue i) => i.ActualName).FirstOrDefault((string n) => !string.IsNullOrWhiteSpace(n)) ?? "", g.ToArray()) into g
			orderby (g.Sources.Length != 0) ? g.Sources.Min((WorkshopCodeSource s) => s.Order) : int.MaxValue
			select g).ToArray();
	}

	internal static bool SafeSource(string url)
	{
		if (Uri.TryCreate(url, UriKind.Absolute, out Uri result) && result.Scheme == "https" && result.Host == "steamcommunity.com" && result.Port == 443 && result.UserInfo.Length == 0 && result.AbsolutePath.TrimEnd('/') == new Uri("https://steamcommunity.com/workshop/filedetails/discussion/3787302680/592940620292752301/").AbsolutePath.TrimEnd('/') && Regex.IsMatch(result.Query, "^(?:\\?ctp=[0-9]{1,5})?$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L)))
		{
			return Regex.IsMatch(result.Fragment, "^(?:#c[0-9]+)?$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1L));
		}
		return false;
	}

	internal static string Report(WorkshopCodeProblem problem, string language)
	{
		StringBuilder stringBuilder = new StringBuilder().AppendLine((problem.Name.Length > 0) ? problem.Name : T(WorkshopCodeUi.Unknown, Array.Empty<object>())).AppendLine("Workshop ID: " + problem.Id);
		WorkshopCodeIssue[] issues = problem.Issues;
		foreach (WorkshopCodeIssue workshopCodeIssue in issues)
		{
			stringBuilder.AppendLine($"[SCM-{workshopCodeIssue.Error}] " + WorkshopCodeErrorText.ForLanguage(language, (int)(13 + workshopCodeIssue.Error)));
			if (workshopCodeIssue.ActualName.Length > 0)
			{
				stringBuilder.AppendLine(T(WorkshopCodeUi.Actual, new object[1] { workshopCodeIssue.ActualName }));
			}
			if (workshopCodeIssue.Group.Length > 0)
			{
				stringBuilder.AppendLine(T(WorkshopCodeUi.Group, new object[1] { workshopCodeIssue.Group }));
			}
			if (workshopCodeIssue.Total > 0)
			{
				stringBuilder.AppendLine(T(WorkshopCodeUi.Parts, new object[2]
				{
					(from s in workshopCodeIssue.Sources
						where s.Part > 0
						select s.Part).Distinct().Count(),
					workshopCodeIssue.Total
				}));
			}
			if (workshopCodeIssue.Missing.Length != 0)
			{
				stringBuilder.AppendLine(T(WorkshopCodeUi.Missing, new object[1] { string.Join(", ", workshopCodeIssue.Missing) }));
			}
		}
		WorkshopCodeSource[] sources = problem.Sources;
		foreach (WorkshopCodeSource workshopCodeSource in sources)
		{
			stringBuilder.AppendLine(T(WorkshopCodeUi.Code, new object[1] { WorkshopSubmissionCodec.Fingerprint(workshopCodeSource.Code) }));
			stringBuilder.AppendLine((workshopCodeSource.Reply == 0) ? T(WorkshopCodeUi.MainPost, Array.Empty<object>()) : T(WorkshopCodeUi.Source, new object[2] { workshopCodeSource.Page, workshopCodeSource.Reply })).AppendLine(workshopCodeSource.Url).AppendLine(workshopCodeSource.Code);
		}
		return stringBuilder.ToString();
		string T(WorkshopCodeUi key, object[] args)
		{
			return string.Format(WorkshopCodeErrorText.ForLanguage(language, (int)key), args);
		}
	}
}

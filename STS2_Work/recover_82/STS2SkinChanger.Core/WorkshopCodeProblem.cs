using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed record WorkshopCodeProblem(string Key, ulong Id, string Name, WorkshopCodeIssue[] Issues)
{
	public WorkshopCodeSource[] Sources => (from s in Issues.SelectMany((WorkshopCodeIssue i) => i.Sources).DistinctBy((WorkshopCodeSource s) => (Code: s.Code, Url: s.Url))
		orderby s.Order
		select s).ToArray();
}

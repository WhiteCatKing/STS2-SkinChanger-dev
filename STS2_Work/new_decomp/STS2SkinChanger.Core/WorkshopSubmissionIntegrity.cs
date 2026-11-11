using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal static class WorkshopSubmissionIntegrity
{
	internal static WorkshopCommunityState Verify(WorkshopCodeRead read, IReadOnlyDictionary<ulong, WorkshopIdentity> actual, WorkshopCommunityState previous)
	{
		List<WorkshopCodeIssue> list = read.Issues.Select((WorkshopCodeIssue i) => i with
		{
			ActualName = (actual.GetValueOrDefault(i.Id)?.Name ?? "")
		}).ToList();
		List<WorkshopVerifiedSubmission> list2 = new List<WorkshopVerifiedSubmission>();
		foreach (WorkshopCodeCandidate item in read.Candidates.OrderBy((WorkshopCodeCandidate c) => c.Sources.Max((WorkshopCodeSource s) => s.Order)))
		{
			WorkshopCodePayload payload = item.Payload;
			WorkshopIdentity valueOrDefault = actual.GetValueOrDefault(payload.Item.Id);
			WorkshopCodeError? workshopCodeError = ((valueOrDefault == null) ? new WorkshopCodeError?(WorkshopCodeError.Unavailable) : ((valueOrDefault.App != 2868840) ? new WorkshopCodeError?(WorkshopCodeError.WrongGame) : ((WorkshopCodeError?)null)));
			if (workshopCodeError.HasValue)
			{
				WorkshopCodeError valueOrDefault2 = workshopCodeError.GetValueOrDefault();
				list.Add(new WorkshopCodeIssue(item.Group, payload.Item.Id, "", valueOrDefault?.Name ?? "", valueOrDefault2, item.Group, item.Sources.Max((WorkshopCodeSource s) => s.Part), Array.Empty<int>(), item.Sources));
			}
			else
			{
				list2.Add(new WorkshopVerifiedSubmission(payload.Item, valueOrDefault.Name, item.Group)
				{
					Reply = item.Sources.Max((WorkshopCodeSource source) => source.Reply)
				});
			}
		}
		Dictionary<ulong, WorkshopVerifiedSubmission> dictionary = (from e in list2
			group e by e.Item.Id).ToDictionary((IGrouping<ulong, WorkshopVerifiedSubmission> g) => g.Key, (IGrouping<ulong, WorkshopVerifiedSubmission> g) => g.Last());
		return new WorkshopCommunityState(2, dictionary.Values.OrderBy((WorkshopVerifiedSubmission e) => e.Item.Id).ToArray(), list.ToArray());
	}
}

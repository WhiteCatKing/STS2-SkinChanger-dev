using System;

namespace STS2SkinChanger.Core;

internal sealed record WorkshopCommunityState(int Format, WorkshopVerifiedSubmission[] Entries, WorkshopCodeIssue[] Issues)
{
	internal static WorkshopCommunityState Empty => new WorkshopCommunityState(2, Array.Empty<WorkshopVerifiedSubmission>(), Array.Empty<WorkshopCodeIssue>());
}

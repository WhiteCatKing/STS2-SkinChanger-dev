using System;
using System.Collections.Generic;

namespace ModLaunchManager;

internal sealed class WorkshopChangeNoteItem
{
	internal required ulong ItemId { get; init; }

	internal required uint LocalTime { get; init; }

	internal required uint After { get; init; }

	internal required uint Through { get; init; }

	internal WorkshopChangeNoteState State { get; set; }

	internal IReadOnlyList<WorkshopChangeNote> Notes { get; set; } = Array.Empty<WorkshopChangeNote>();

	internal bool Truncated { get; set; }

	internal bool IsReading
	{
		get
		{
			WorkshopChangeNoteState state = State;
			if ((uint)state <= 1u)
			{
				return true;
			}
			return false;
		}
	}

	internal bool NothingNew
	{
		get
		{
			if (State == WorkshopChangeNoteState.Ready)
			{
				return Notes.Count == 0;
			}
			return false;
		}
	}
}

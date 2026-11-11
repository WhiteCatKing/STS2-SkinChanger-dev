using System.Collections.Generic;

namespace STS2SkinChanger.Ui;

internal static class WorkshopOptionSelectionPolicy
{
	public static int IndexOfId(IReadOnlyList<int> itemIds, int id)
	{
		for (int i = 0; i < itemIds.Count; i++)
		{
			if (itemIds[i] == id)
			{
				return i;
			}
		}
		return -1;
	}
}

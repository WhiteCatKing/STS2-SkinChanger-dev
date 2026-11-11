using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Core;

internal sealed class WorkshopPagePool<T>(int capacity, Func<WorkshopItemBinding, T> create)
{
	private readonly List<(WorkshopItemBinding Binding, T View)> _slots = new List<(WorkshopItemBinding, T)>();

	public IReadOnlyList<(WorkshopItemBinding Binding, T View)> Slots => _slots;

	public void Bind(IReadOnlyList<ulong> ids)
	{
		if (ids.Count > _003Ccapacity_003EP || ids.Any((ulong id) => id == 0))
		{
			throw new ArgumentOutOfRangeException("ids");
		}
		while (_slots.Count < ids.Count)
		{
			WorkshopItemBinding workshopItemBinding = new WorkshopItemBinding();
			_slots.Add((workshopItemBinding, _003Ccreate_003EP(workshopItemBinding)));
		}
		for (int i = 0; i < _slots.Count; i++)
		{
			_slots[i].Binding.Bind((i < ids.Count) ? ids[i] : 0);
		}
	}
}

namespace STS2SkinChanger.Core;

internal sealed class WorkshopHoverDwell
{
	private ulong _item;

	private double _since;

	public bool Observe(ulong item, double now)
	{
		if (item == 0L)
		{
			Clear();
			return false;
		}
		if (item != _item)
		{
			_item = item;
			_since = now;
			return false;
		}
		return now - _since >= 1.0;
	}

	public void Clear()
	{
		_item = 0uL;
		_since = 0.0;
	}
}

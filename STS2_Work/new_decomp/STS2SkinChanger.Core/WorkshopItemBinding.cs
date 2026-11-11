namespace STS2SkinChanger.Core;

internal sealed class WorkshopItemBinding
{
	private long _revision;

	public ulong Id { get; private set; }

	internal void Bind(ulong id)
	{
		Id = id;
		_revision++;
	}

	public WorkshopItemTicket Capture()
	{
		return new WorkshopItemTicket(Id, _revision);
	}

	public bool Matches(WorkshopItemTicket ticket)
	{
		if (Id != 0L)
		{
			return ticket == Capture();
		}
		return false;
	}
}

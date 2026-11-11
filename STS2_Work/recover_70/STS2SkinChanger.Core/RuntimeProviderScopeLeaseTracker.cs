namespace STS2SkinChanger.Core;

internal sealed class RuntimeProviderScopeLeaseTracker
{
	private long _current;

	public long Current => _current;

	public long Claim()
	{
		_current++;
		if (_current == 0L)
		{
			_current++;
		}
		return _current;
	}

	public bool IsCurrent(long lease)
	{
		if (lease != 0L)
		{
			return lease == _current;
		}
		return false;
	}

	public void Reset()
	{
		_current = 0L;
	}
}

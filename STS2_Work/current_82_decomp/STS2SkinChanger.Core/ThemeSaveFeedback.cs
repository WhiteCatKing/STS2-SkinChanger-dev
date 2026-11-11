namespace STS2SkinChanger.Core;

internal sealed class ThemeSaveFeedback
{
	public const double DurationSeconds = 1.0;

	private int _revision;

	public bool Active { get; private set; }

	public int Begin()
	{
		Active = true;
		return ++_revision;
	}

	public void Cancel()
	{
		Active = false;
		_revision++;
	}

	public bool Expire(int revision)
	{
		if (!Active || revision != _revision)
		{
			return false;
		}
		Active = false;
		return true;
	}
}

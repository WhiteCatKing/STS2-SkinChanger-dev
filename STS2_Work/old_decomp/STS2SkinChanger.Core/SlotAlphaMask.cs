namespace STS2SkinChanger.Core;

internal sealed class SlotAlphaMask
{
	private float? _restoreAlpha;

	public float Hide(float currentAlpha)
	{
		if (!_restoreAlpha.HasValue || currentAlpha != 0f)
		{
			_restoreAlpha = currentAlpha;
		}
		return 0f;
	}

	public float Restore(float currentAlpha)
	{
		float result = ((currentAlpha == 0f) ? (_restoreAlpha ?? currentAlpha) : currentAlpha);
		_restoreAlpha = null;
		return result;
	}
}

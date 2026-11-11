namespace STS2SkinChanger.Ui;

internal sealed class CardBuiltInOverlayVisibility
{
	private object? _model;

	private object? _overlay;

	private bool _original;

	private bool? _applied;

	public bool Resolve(object model, object overlay, bool current, bool? selected)
	{
		if (_model != model || _overlay != overlay)
		{
			_model = model;
			_overlay = overlay;
			_applied = null;
		}
		if (selected.HasValue)
		{
			bool valueOrDefault = selected == true;
			if (!_applied.HasValue)
			{
				_original = current;
			}
			_applied = valueOrDefault;
			return valueOrDefault;
		}
		bool result = ((_applied == current) ? _original : current);
		_applied = null;
		return result;
	}
}

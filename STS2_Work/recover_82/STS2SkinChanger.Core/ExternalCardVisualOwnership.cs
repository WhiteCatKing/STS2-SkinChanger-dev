namespace STS2SkinChanger.Core;

internal readonly record struct ExternalCardVisualOwnership(bool Portrait, bool Frame, bool Text)
{
	public ExternalCardVisualOwnership Merge(ExternalCardVisualOwnership other)
	{
		return new ExternalCardVisualOwnership(Portrait || other.Portrait, Frame || other.Frame, Text || other.Text);
	}
}

namespace STS2SkinChanger.Catalog;

internal sealed class AncientCardReplacement
{
	public string CardType { get; set; } = string.Empty;

	public string? NormalPortrait { get; set; }

	public string? AncientPortrait { get; set; }

	public string? DifferentialPortrait { get; set; }

	public string? ConfigKey { get; set; }

	public string? PathForGrouping
	{
		get
		{
			if (string.IsNullOrWhiteSpace(AncientPortrait))
			{
				return NormalPortrait;
			}
			return AncientPortrait;
		}
	}
}

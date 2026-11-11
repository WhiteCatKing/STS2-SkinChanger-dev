namespace STS2SkinChanger.Catalog;

internal sealed class CardFrameReplacementEntry
{
	public string CardId { get; set; } = string.Empty;

	public string UiMode { get; set; } = string.Empty;

	public string? Frame { get; set; }

	public string? FrameMaterial { get; set; }

	public string? BannerTexture { get; set; }

	public string? BannerMaterial { get; set; }

	public string? PortraitBorder { get; set; }

	public string? PortraitBorderMaterial { get; set; }

	public string? AncientTextBg { get; set; }

	public string? TextBackgroundMaterial { get; set; }

	public string? EnergyIcon { get; set; }

	public string? Highlight { get; set; }

	public string? HighlightMaterial { get; set; }

	public bool? FrameVisible { get; set; }

	public bool? BannerVisible { get; set; }

	public bool? TextBackgroundVisible { get; set; }

	public bool? PortraitBorderVisible { get; set; }

	public bool? EnergyIconVisible { get; set; }

	public bool? HighlightVisible { get; set; }

	public bool? TypePlaqueVisible { get; set; }

	public bool? TypeLabelVisible { get; set; }

	public bool? DescriptionVisible { get; set; }

	public bool? InfectionOverlayVisible { get; set; }
}

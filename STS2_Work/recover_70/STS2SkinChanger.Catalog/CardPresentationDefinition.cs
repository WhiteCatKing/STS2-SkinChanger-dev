using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Catalog;

internal sealed record CardPresentationDefinition(bool UseAncientLayout = false, string? Frame = null, string? FrameMaterial = null, string? BannerTexture = null, string? BannerMaterial = null, string? PortraitBorder = null, string? PortraitBorderMaterial = null, string? AncientTextBackground = null, string? TextBackgroundMaterial = null, string? EnergyIcon = null, string? Highlight = null, string? HighlightMaterial = null, bool? FrameVisible = null, bool? BannerVisible = null, bool? TextBackgroundVisible = null, bool? PortraitBorderVisible = null, bool? EnergyIconVisible = null, bool? HighlightVisible = null, bool? TypePlaqueVisible = null, bool? TypeLabelVisible = null, bool? DescriptionVisible = null, bool? InfectionOverlayVisible = null, bool UseFullFrameArt = false, bool UseExpandedPortraitLayout = false, string? FrameOverlay = null, bool? PortraitVisible = null, float? FrameOverlayOffsetTop = null, float? FrameOverlayOffsetBottom = null, float? FrameOverlayOffsetLeft = null, float? FrameOverlayOffsetRight = null, float? FrameOverlayScaleX = null, float? FrameOverlayScaleY = null, bool? BuiltInOverlayVisible = null)
{
	public IEnumerable<string> ResourcePaths => new string[12]
	{
		Frame, FrameMaterial, BannerTexture, BannerMaterial, PortraitBorder, PortraitBorderMaterial, AncientTextBackground, TextBackgroundMaterial, EnergyIcon, Highlight,
		HighlightMaterial, FrameOverlay
	}.Where((string path) => !string.IsNullOrWhiteSpace(path)).Cast<string>().Distinct<string>(StringComparer.OrdinalIgnoreCase);
}

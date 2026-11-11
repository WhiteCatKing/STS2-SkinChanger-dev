using System;

namespace STS2SkinChanger.Catalog;

internal sealed record CardArtPackCard(string SourcePath, string CardId, byte[] Bytes, string DisplayMode, string ArtType, bool UsesEditSource)
{
	public string PackId { get; init; } = string.Empty;

	public bool IsFullArt => DisplayMode.Equals("full_art", StringComparison.OrdinalIgnoreCase);

	public bool IsCardShaped
	{
		get
		{
			if (CardArtPackScanPolicy.TryReadPngSize(Bytes, out var width, out var height))
			{
				return height > width;
			}
			return false;
		}
	}
}

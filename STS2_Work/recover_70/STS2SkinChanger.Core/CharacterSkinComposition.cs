using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal sealed class CharacterSkinComposition
{
	public string Id { get; set; } = string.Empty;

	public string GroupId { get; set; } = string.Empty;

	public string Name { get; set; } = string.Empty;

	public List<string> SourceOptionIds { get; set; } = new List<string>();

	public bool HideSources { get; set; }
}

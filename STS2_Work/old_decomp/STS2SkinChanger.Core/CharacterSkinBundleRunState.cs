using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace STS2SkinChanger.Core;

internal sealed class CharacterSkinBundleRunState
{
	public int Version { get; set; } = 1;

	public string RunIdentity { get; set; } = string.Empty;

	public string CharacterGroupId { get; set; } = string.Empty;

	public string BundleName { get; set; } = string.Empty;

	public string? RandomCharacterOptionId { get; set; }

	public List<CardSkinPreset> Cards { get; set; } = new List<CardSkinPreset>();

	public List<MonsterSkinPreset> Monsters { get; set; } = new List<MonsterSkinPreset>();

	internal static string Identity(long startTime, string seed, ulong localPlayer, IEnumerable<ulong> players)
	{
		return JsonSerializer.Serialize(new
		{
			startTime = startTime,
			seed = seed,
			localPlayer = localPlayer,
			players = players.Order().ToArray()
		});
	}
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace STS2SkinChanger.Ui;

internal static class OtherCreatureCatalog
{
	internal const string ByrdpipAttackSfx = "event:/sfx/byrdpip/byrdpip_attack";

	private static readonly string[] IdleAliases = new string[4] { "idle_loop", "idle", "stand", "standing" };

	internal static IReadOnlyList<OtherCreatureDefinition> All { get; } = new global::_003C_003Ez__ReadOnlyArray<OtherCreatureDefinition>(new OtherCreatureDefinition[2]
	{
		new OtherCreatureDefinition("byrdpip", "monsters", "BYRDPIP.name", "异鸟宝宝", "res://scenes/creature_visuals/byrdpip.tscn", IdleAliases, new _003C_003Ez__ReadOnlySingleElementList<OtherCreatureActionDefinition>(new OtherCreatureActionDefinition(OtherCreatureActionKind.Attack, new global::_003C_003Ez__ReadOnlyArray<string>(new string[5] { "attack", "attack1", "attack_1", "atk", "bite" }), Loop: false, IdleAliases, FollowUpLoop: true, "event:/sfx/byrdpip/byrdpip_attack"))),
		new OtherCreatureDefinition("paels_legion", "relics", "PAELS_LEGION.title", "佩尔的士兵", "res://scenes/creature_visuals/paels_legion.tscn", IdleAliases, new global::_003C_003Ez__ReadOnlyArray<OtherCreatureActionDefinition>(new OtherCreatureActionDefinition[3]
		{
			new OtherCreatureActionDefinition(OtherCreatureActionKind.Block, new _003C_003Ez__ReadOnlySingleElementList<string>("block"), Loop: false, new _003C_003Ez__ReadOnlySingleElementList<string>("block_loop"), FollowUpLoop: true),
			new OtherCreatureActionDefinition(OtherCreatureActionKind.Sleep, new _003C_003Ez__ReadOnlySingleElementList<string>("sleep"), Loop: false, new _003C_003Ez__ReadOnlySingleElementList<string>("sleep_loop"), FollowUpLoop: true),
			new OtherCreatureActionDefinition(OtherCreatureActionKind.Wake, new global::_003C_003Ez__ReadOnlyArray<string>(new string[3] { "wake_up", "wake", "wakeup" }), Loop: false, IdleAliases, FollowUpLoop: true)
		}))
	});

	internal static OtherCreatureDefinition? Find(string? id)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return null;
		}
		return All.FirstOrDefault((OtherCreatureDefinition creature) => creature.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
	}
}

namespace STS2SkinChanger.Ui;

internal enum OtherCreatureActionKind
{
    Attack,
    Block,
    Sleep,
    Wake,
    Cast,
    Hurt,
    Death,
    Revive
}

internal sealed record OtherCreatureActionDefinition(
    OtherCreatureActionKind Kind,
    IReadOnlyList<string> AnimationAliases,
    bool Loop,
    IReadOnlyList<string> FollowUpAliases,
    bool FollowUpLoop,
    string? SfxPath = null);

internal sealed record OtherCreatureDefinition(
    string Id,
    string LocalizationTable,
    string LocalizationKey,
    string FallbackTitle,
    string ScenePath,
    IReadOnlyList<string> IdleAliases,
    IReadOnlyList<OtherCreatureActionDefinition> Actions,
    string? RequiredModelType = null);

internal static class OtherCreatureCatalog
{
    internal const string ByrdpipAttackSfx = "event:/sfx/byrdpip/byrdpip_attack";

    private static readonly string[] IdleAliases =
        ["idle_loop", "idle", "stand", "standing"];

    internal static IReadOnlyList<OtherCreatureDefinition> All { get; } =
    [
        new OtherCreatureDefinition(
            "byrdpip",
            "monsters",
            "BYRDPIP.name",
            "异鸟宝宝",
            "res://scenes/creature_visuals/byrdpip.tscn",
            IdleAliases,
            [
                new OtherCreatureActionDefinition(
                    OtherCreatureActionKind.Attack,
                    ["attack", "attack1", "attack_1", "atk", "bite"],
                    Loop: false,
                    FollowUpAliases: IdleAliases,
                    FollowUpLoop: true,
                    SfxPath: ByrdpipAttackSfx)
            ]),
        new OtherCreatureDefinition(
            "paels_legion",
            "relics",
            "PAELS_LEGION.title",
            "佩尔的士兵",
            "res://scenes/creature_visuals/paels_legion.tscn",
            IdleAliases,
            [
                new OtherCreatureActionDefinition(
                    OtherCreatureActionKind.Block,
                    ["block"],
                    Loop: false,
                    FollowUpAliases: ["block_loop"],
                    FollowUpLoop: true),
                new OtherCreatureActionDefinition(
                    OtherCreatureActionKind.Sleep,
                    ["sleep"],
                    Loop: false,
                    FollowUpAliases: ["sleep_loop"],
                    FollowUpLoop: true),
                new OtherCreatureActionDefinition(
                    OtherCreatureActionKind.Wake,
                    ["wake_up", "wake", "wakeup"],
                    Loop: false,
                    FollowUpAliases: IdleAliases,
                    FollowUpLoop: true)
            ]),
        new OtherCreatureDefinition(
            "osty", "monsters", "OSTY.name", "奥斯提",
            "res://scenes/creature_visuals/osty.tscn", IdleAliases,
            [
                Action(OtherCreatureActionKind.Attack, ["attack", "attack1", "atk"]),
                Action(OtherCreatureActionKind.Cast, ["cast", "skill"]),
                Action(OtherCreatureActionKind.Hurt, ["hurt", "hit"]),
                new(OtherCreatureActionKind.Death, ["die", "death"], false, ["dead_loop", "dead"], true),
                Action(OtherCreatureActionKind.Revive, ["revive", "resurrect"])
            ]),
        new OtherCreatureDefinition(
            "byrdine_monster", "relics", "BYRDINE.title", "异鸟成鸟",
            "res://scenes/creature_visuals/byrdine_monster.tscn", IdleAliases,
            [Action(OtherCreatureActionKind.Attack, ["attack", "attack1", "atk"])],
            "MegaCrit.Sts2.Core.Models.Monsters.ByrdineMonster"),
        new OtherCreatureDefinition(
            "wuppy", "monsters", "WUPPY.name", "瓦皮",
            "res://scenes/creature_visuals/wuppy.tscn", IdleAliases,
            [
                Action(OtherCreatureActionKind.Hurt, ["hurt", "hit"]),
                new(OtherCreatureActionKind.Death, ["die", "death"], false, [], false)
            ], "MegaCrit.Sts2.Core.Models.Monsters.Wuppy")
    ];

    private static OtherCreatureActionDefinition Action(OtherCreatureActionKind kind, string[] aliases) =>
        new(kind, aliases, false, IdleAliases, true);

    internal static IEnumerable<OtherCreatureDefinition> Available(Func<string, bool> hasModel) =>
        All.Where(c => c.RequiredModelType == null || hasModel(c.RequiredModelType));

    internal static OtherCreatureDefinition? Find(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        return All.FirstOrDefault(creature =>
            creature.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }
}

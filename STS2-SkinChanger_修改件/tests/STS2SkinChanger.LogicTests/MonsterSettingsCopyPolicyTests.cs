using STS2SkinChanger.Core;

// #12：把当前英雄的怪物区设置（内容模式 + Mod 优先级：启用状态与顺序）复制到其他英雄的皮肤包。
// 商人皮肤在切换器里属于怪物分组，所以这就是「给所有英雄一键设商人皮肤」的实际内容。
internal static class MonsterSettingsCopyPolicyTests
{
    internal static void Run()
    {
        var source = new CharacterSkinBundle
        {
            Name = "Source",
            CharacterGroupId = "fool",
            CharacterOptionId = "skin_a",
            MonsterMode = BundleContentMode.ModPriority,
            MonsterModPriority =
            [
                new BundleModPriorityEntry("Merchant2CuteII", true),
                new BundleModPriorityEntry("ATA_Merchant", false)
            ],
            CardMode = BundleContentMode.MultiplePresets,
            CardModPriority = [new BundleModPriorityEntry("SomeCardPack", true)],
            CardPresetNames = new Dictionary<string, string> { ["act:1"] = "cards-preset" },
            MonsterPresetNames = new Dictionary<string, string> { ["act:1"] = "monster-preset" }
        };

        var second = new CharacterSkinBundle
        {
            Name = "Second",
            CharacterGroupId = "ironclad",
            CharacterOptionId = "skin_b",
            MonsterMode = BundleContentMode.MultiplePresets,
            MonsterModPriority = [new BundleModPriorityEntry("OldMerchant", true)],
            CardMode = BundleContentMode.ModPriority,
            CardModPriority = [new BundleModPriorityEntry("KeepMe", false)],
            CardPresetNames = new Dictionary<string, string> { ["act:1"] = "keep-cards" },
            MonsterPresetNames = new Dictionary<string, string> { ["act:1"] = "keep-monster-preset" }
        };

        var third = new CharacterSkinBundle
        {
            Name = "Third",
            CharacterGroupId = "defect",
            CharacterOptionId = "skin_c",
            MonsterMode = BundleContentMode.ModPriority,
            MonsterModPriority = []
        };

        var settings = MonsterSettingsCopyPolicy.Capture(source);
        var changed = MonsterSettingsCopyPolicy.ApplyToAll([second, third], settings);

        Require(changed == 2, "两个英雄的皮肤包都应该被更新。");
        foreach (var target in new[] { second, third })
        {
            Require(target.MonsterMode == source.MonsterMode, "怪物区内容模式必须与源一致。");
            Require(target.MonsterModPriority.Count == source.MonsterModPriority.Count,
                "怪物 Mod 优先级条数必须与源一致。");
            for (var i = 0; i < source.MonsterModPriority.Count; i++)
            {
                Require(target.MonsterModPriority[i].OptionId == source.MonsterModPriority[i].OptionId,
                    "怪物 Mod 优先级顺序必须与源一致。");
                Require(target.MonsterModPriority[i].Enabled == source.MonsterModPriority[i].Enabled,
                    "怪物 Mod 启用状态必须与源一致。");
            }
        }

        // 卡牌区、角色皮肤、预设名一律不能被动过。
        Require(second.CardMode == BundleContentMode.ModPriority &&
                second.CardModPriority.Single().OptionId == "KeepMe" &&
                !second.CardModPriority.Single().Enabled,
            "复制怪物设置不能改动卡牌区。");
        Require(second.CharacterOptionId == "skin_b" && second.Name == "Second",
            "复制怪物设置不能改动角色皮肤或皮肤包名字。");
        Require(second.CardPresetNames["act:1"] == "keep-cards" &&
                second.MonsterPresetNames["act:1"] == "keep-monster-preset",
            "复制怪物设置不能改动预设名引用。");

        // 每个目标拿到独立列表：改其中一个不会影响源或另一个。
        second.MonsterModPriority[0] = second.MonsterModPriority[0] with { Enabled = false };
        Require(third.MonsterModPriority[0].Enabled == source.MonsterModPriority[0].Enabled &&
                source.MonsterModPriority[0].Enabled,
            "复制出来的优先级列表必须互相独立。");

        // 没有皮肤包的英雄不在序列里：服务层直接跳过，不会凭空造一个 [P] 皮肤包。
        Require(MonsterSettingsCopyPolicy.ApplyToAll([], settings) == 0,
            "没有皮肤包时不应产生任何改动。");

        Console.WriteLine("Monster settings copy policy passed: all heroes share the monster list, cards/character/presets untouched.");
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
    }
}

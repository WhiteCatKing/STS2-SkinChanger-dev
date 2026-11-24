using System.Collections;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

/// <summary>
/// 假商人预览要的是「六个遗物槽、没有卡牌与药水容器」的背包，所以不能直接用
/// <see cref="MerchantInventory.CreateForNormalMerchant"/>（那会把每一列都填满）。
/// 两个游戏版本装配入口不同：v0.111 先建空背包再逐个 AddRelicEntry；
/// v0.113 移除了 AddRelicEntry，改为把各条目列表交给构造函数，随后 Setup() 补 Id 与事件。
/// 这里按版本探测，两条路都只放六个遗物，预览外观因此与原版一致。
/// </summary>
internal static class FakeMerchantPreviewInventory
{
    internal static MerchantInventory Build(Player player)
    {
        // Preview the six items and prices from the reference shop. The actual run
        // continues to build its own inventory through FakeMerchant.BeforeEventStarted.
        var relics = new List<MerchantRelicEntry>
        {
            new(ModelDb.Relic<FakeLeesWaffle>().ToMutable(), player),
            new(ModelDb.Relic<FakeVenerableTeaSet>().ToMutable(), player),
            new(ModelDb.Relic<FakeStrikeDummy>().ToMutable(), player),
            new(ModelDb.Relic<FakeSneckoEye>().ToMutable(), player),
            new(ModelDb.Relic<FakeOrichalcum>().ToMutable(), player),
            new(ModelDb.Relic<FakeAnchor>().ToMutable(), player)
        };
        if (!TryBuildWithEntryLists(player, relics, out var model))
        {
            model = new MerchantInventory(player);
            var addRelic = AccessTools.Method(typeof(MerchantInventory), "AddRelicEntry") ??
                throw new MissingMethodException(nameof(MerchantInventory), "AddRelicEntry");
            foreach (var relic in relics) addRelic.Invoke(model, [relic]);
        }

        var cost = AccessTools.Field(typeof(MerchantEntry), "_cost");
        int[] prices = [47, 45, 47, 51, 45, 55];
        for (var index = 0; index < relics.Count; index++)
            cost.SetValue(relics[index], prices[index]);
        return model;
    }

    // v0.113+. AddRelicEntry 已移除，背包只能整块交给构造函数。
    private static bool TryBuildWithEntryLists(
        Player player, List<MerchantRelicEntry> relics, out MerchantInventory inventory)
    {
        inventory = null!;
        var constructor = typeof(MerchantInventory).GetConstructors()
            .FirstOrDefault(candidate => candidate.GetParameters().Length == 7);
        if (constructor == null) return false;

        try
        {
            var parameters = constructor.GetParameters();
            var arguments = new object?[parameters.Length];
            var relicIndex = -1;
            for (var index = 0; index < parameters.Length; index++)
            {
                var type = parameters[index].ParameterType;
                if (type == typeof(Player))
                {
                    arguments[index] = player;
                }
                else if (type == typeof(MerchantCardRemovalEntry))
                {
                    // This inventory has no card removal node. Initialize dereferences that node
                    // whenever the inventory supplies an entry.
                    arguments[index] = null;
                }
                else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    arguments[index] = Activator.CreateInstance(
                        typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]));
                    if (type == typeof(List<MerchantRelicEntry>)) relicIndex = index;
                }
                else
                {
                    return false;
                }
            }

            if (relicIndex < 0 || arguments[relicIndex] is not IList list) return false;
            foreach (var relic in relics) list.Add(relic);

            inventory = (MerchantInventory)constructor.Invoke(arguments);
            typeof(MerchantInventory).GetMethod("Setup", Type.EmptyTypes)?.Invoke(inventory, null);
            return true;
        }
        catch (Exception exception)
        {
            // 预览背包只是展示用：装配失败就退回原版工厂，别让图鉴里的假商人整块消失。
            ModLog.Warn("假商人预览背包按条目列表装配失败，改用原版工厂：" + exception.GetBaseException().Message);
            inventory = null!;
            return false;
        }
    }
}

using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

// Crusader 0.1.6 replaced the removed game SingletonModel with its own AbstractModel
// subscribers. Publishing its assembly after ModelDb.Init cannot construct those models.
internal static class CrusaderHookBridge
{
    private const string Root = "CrusaderSkin.Code.Singleton.CustomCrusaderSingletonModel";
    private static readonly HashSet<Assembly> Installed = [];
    private static readonly Harmony Harmony = new(Entry.ModId + ".crusader-hooks");

    internal static bool IsHookModel(Type type) => type.IsClass && !type.IsAbstract &&
        type.BaseType?.FullName == Root && typeof(AbstractModel).IsAssignableFrom(type) &&
        type.GetConstructor(Type.EmptyTypes) != null;

    internal static void Install(Assembly assembly)
    {
        var root = assembly.GetType(Root);
        var helper = assembly.GetType("CrusaderSkin.Code.Helper.CrusaderHelper");
        if (root == null || helper == null || Installed.Contains(assembly)) return;
        // Constructors subscribe permanently. Narrow the author's own predicate before any
        // subscription exists so late callbacks cannot animate another skin or another player.
        foreach (var argument in new[] { typeof(Player), typeof(Creature) })
            Harmony.Patch(AccessTools.Method(helper, "IsIronclad", [argument]),
                postfix: new HarmonyMethod(typeof(CrusaderHookBridge), nameof(FilterOwner)));
        Installed.Add(assembly);
    }

    private static void FilterOwner(MethodBase __originalMethod, object[] __args, ref bool __result)
    {
        if (!__result) return;
        var player = __args[0] switch { Player value => value, Creature value => value.Player, _ => null };
        if (player?.Character != null)
            __result = SkinService.IsCharacterBehaviorSelected(__originalMethod.Module.Assembly,
                player.Character, player) != false;
    }
}

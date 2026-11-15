using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using STS2SkinChanger.Core;

internal static class CrusaderHookTests
{
    private static bool _selected;
    private static Player _player = null!;
    private static string? _trigger;

    internal static void Run(string dll)
    {
        var patches = new Harmony("sc.tests.crusader");
        foreach (var name in new[] { "Info", "Warn" })
            patches.Patch(AccessTools.Method(typeof(ModLog), name), prefix: Patch(nameof(Skip)));
        patches.Patch(AccessTools.Method(typeof(SkinService), "IsCharacterBehaviorSelected"), prefix: Patch(nameof(Selection)));
        patches.Patch(AccessTools.PropertyGetter(typeof(CardModel), "Owner"), prefix: Patch(nameof(CardOwner)));
        patches.Patch(AccessTools.Method(typeof(CreatureCmd), "TriggerAnim"), prefix: Patch(nameof(Trigger)));
        try
        {
            var assembly = Assembly.LoadFrom(dll);
            var combatType = assembly.GetType("CrusaderSkin.Code.Singleton.CrusaderCombatSingleton", true)!;
            var runType = assembly.GetType("CrusaderSkin.Code.Singleton.CrusaderRunstateSingleton", true)!;
            Require(CrusaderHookBridge.IsHookModel(combatType) && CrusaderHookBridge.IsHookModel(runType), "Modern subscriber models not recognized.");
            Require(!CrusaderHookBridge.IsHookModel(typeof(Ironclad)), "Do not construct arbitrary character models.");
            ModelDb.Inject(typeof(Ironclad));
            var inject = AccessTools.Method(typeof(ProviderModelDiscovery), "InstantiateMissingSingletons");
            for (var i = 0; i < 3; i++) inject.Invoke(null, [assembly, "CrusaderSkin"]);
            Require(ModelDb.Contains(combatType) && ModelDb.Contains(runType), "Late models absent from database.");
            var combat = (CombatState)RuntimeHelpers.GetUninitializedObject(typeof(CombatState));
            var run = (RunState)RuntimeHelpers.GetUninitializedObject(typeof(RunState));
            var listeners = ModHelper.IterateAllCombatStateSubscribers(combat).Where(m => m.GetType() == combatType).ToArray();
            Require(listeners.Length == 1, "Combat hook must subscribe exactly once.");
            Require(ModHelper.IterateAllRunStateSubscribers(run).Count(m => m.GetType() == runType) == 1, "Room hook must subscribe exactly once.");
            _player = (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
            AccessTools.Field(typeof(Player), "<Character>k__BackingField").SetValue(_player,
                RuntimeHelpers.GetUninitializedObject(typeof(Ironclad)));
            var loader = assembly.GetType("CrusaderSkin.Code.Config.CrusaderConfigLoader", true)!;
            patches.Patch(AccessTools.Method(loader, "GetValueAsString"), prefix: Patch(nameof(CardAnimation)));
            var cardPlay = (CardPlay)RuntimeHelpers.GetUninitializedObject(typeof(CardPlay));
            var card = (CardModel)RuntimeHelpers.GetUninitializedObject(typeof(StrikeIronclad));
            AccessTools.Field(typeof(AbstractModel), "<Id>k__BackingField").SetValue(card, ModelDb.GetId(typeof(StrikeIronclad)));
            AccessTools.Field(typeof(CardPlay), "<Card>k__BackingField").SetValue(cardPlay, card);
            foreach (var expected in new[] { true, false, true, false })
            {
                _selected = expected;
                _trigger = null;
                listeners[0].BeforeCardPlayed(cardPlay).GetAwaiter().GetResult();
                Require(_trigger == (expected ? "CardPlay/Smite" : null), "Formal attack missing or leaked after leaving Crusader.");
            }
            _selected = true;
            var helper = assembly.GetType("CrusaderSkin.Code.Helper.CrusaderHelper", true)!;
            var applies = AccessTools.Method(helper, "IsIronclad", [typeof(Player)]);
            var other = (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
            AccessTools.Field(typeof(Player), "<Character>k__BackingField").SetValue(other, _player.Character);
            Require(!(bool)applies.Invoke(null, [other])!, "Same-character teammate inherited local Crusader skin.");
            AccessTools.Field(typeof(Player), "<Character>k__BackingField").SetValue(other,
                RuntimeHelpers.GetUninitializedObject(typeof(Silent)));
            Require(!(bool)applies.Invoke(null, [other])!, "Original character check must remain authoritative.");
            Console.WriteLine("Crusader hooks passed: real late-loaded combat/run subscribers, three registrations without duplication, formal CardPlay/Smite and deselection gating.");
        }
        finally { patches.UnpatchAll(patches.Id); }
    }

    private static HarmonyMethod Patch(string name) => new(typeof(CrusaderHookTests), name);
    private static bool Skip() => false;
    private static bool Selection(Player? player, ref bool? __result)
    {
        __result = _selected && ReferenceEquals(player, _player);
        return false;
    }
    private static bool CardOwner(ref Player __result) { __result = _player; return false; }
    private static bool CardAnimation(ref string __result) { __result = "Smite"; return false; }
    private static bool Trigger(string __1, ref Task __result)
    {
        _trigger = __1;
        __result = Task.CompletedTask;
        return false;
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}

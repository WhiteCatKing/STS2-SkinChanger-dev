using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class CustomMerchantBoundaryTests
{
    internal static void Run()
    {
        var vanillaType = typeof(CharacterModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Characters.Silent", true)!;
        var customType = typeof(AnimatorPatchDiscoveryTests).GetNestedType("AnimatorFixtureCharacter", BindingFlags.NonPublic)!;
        Player Make(Type type)
        {
            var player = (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
            var character = (CharacterModel)RuntimeHelpers.GetUninitializedObject(type);
            AccessTools.Field(typeof(Player), "<Character>k__BackingField").SetValue(player, character);
            return player;
        }
        var first = Make(vanillaType);
        var second = Make(vanillaType);
        var custom = Make(customType);
        Require(!MultiplayerMerchantPlayerVisualIsolationPatch.RequiresNativeCharacterCreation([first, second]),
            "Vanilla multiplayer must retain per-player appearance isolation.");
        var prefix = AccessTools.Method(typeof(MultiplayerMerchantPlayerVisualIsolationPatch), "Prefix");
        foreach (var players in new[] { new List<Player> { first }, new List<Player> { custom },
                     new List<Player> { first, custom }, new List<Player> { custom, first },
                     new List<Player> { first, second, custom } })
        {
            var order = players.ToArray();
            Require((bool)prefix.Invoke(null, [players, null, null])!, "Custom/single-player loop must defer to original.");
            Require(players.SequenceEqual(order), "Deferral must not mutate player order or begin scene construction.");
        }
        Require(!MerchantRuntimeAppearance.TryRefreshLocalPlayer(custom, "custom", out var visual, out var error) &&
                visual == null && error!.Contains("custom character"),
            "Hot swap must not repeat typed instantiation for custom scenes.");
        Console.WriteLine("Custom merchant boundary passed: mixed lobbies in both orders, vanilla isolation, no room mutation, custom hot-swap deferral.");
    }
    private static void Require(bool condition, string message) { if (!condition) throw new Exception(message); }
}

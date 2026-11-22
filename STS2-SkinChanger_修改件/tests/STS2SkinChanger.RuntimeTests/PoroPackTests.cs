using MegaCrit.Sts2.Core.Animation;
using System.Text.Json;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using System.Text;
using System.Text.RegularExpressions;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class PoroPackTests
{
    internal static void Run(string path)
    {
        using var archive = PckArchive.Open(path);
        const string root = "res://RedHoodNecrobinder/";
        var script = root + "scripts/spine_factory.gd";
        var variants = new[] { "redhood_bot01", "redhood_maid_bot01" };
        var files = PoroVariantBridge.CompanionFiles(archive, script, variants);
        if (!files.ContainsKey(script) || !Encoding.UTF8.GetString(archive.ReadFile(script)).Contains("func data_for(", StringComparison.Ordinal))
            throw new InvalidOperationException("The companion factory script is missing.");

        foreach (var variant in variants)
        {
            var prefix = root + "spine42/" + variant + "/";
            var import = files.Keys.Single(key => key.StartsWith(prefix) && key.EndsWith(".png.import"));
            var target = Regex.Match(Encoding.UTF8.GetString(archive.ReadFile(import)),
                "path(?:\\.[a-z0-9_]+)?=\"([^\"]+)\"").Groups[1].Value;
            if (!files.ContainsKey(prefix + variant + ".spine-json") ||
                !files.Keys.Any(key => key.StartsWith(prefix) && key.EndsWith(".atlas")) ||
                !files.ContainsKey(target))
                throw new InvalidOperationException("Companion texture or skeleton missing: " + variant);
        }
        var unexpected = files.Keys.Where(key => key != script && !key.StartsWith("res://.godot/imported/") &&
            !variants.Any(variant => key.StartsWith(root + "spine42/" + variant + "/")));
        if (unexpected.Any()) throw new InvalidOperationException("Companion projection contains unrelated files.");
        VerifyDeathAnimation(archive, root, variants);
        VerifyAuthorCallbacks(Path.ChangeExtension(path, ".dll"));
        Console.WriteLine("Poro pack passed: both variants, companion-only resources and real factory/actor detours.");
    }


    private static void VerifyDeathAnimation(PckArchive archive, string root, IEnumerable<string> variants)
    {
        foreach (var variant in variants)
        {
            using var doc = JsonDocument.Parse(archive.ReadFile(root + "spine42/" + variant + "/" + variant + ".spine-json"));
            var clips = doc.RootElement.GetProperty("animations");
            if (clips.GetProperty("die").GetRawText() != clips.GetProperty("dead_loop").GetRawText())
                throw new InvalidOperationException("Poro pack death clip contract has changed: " + variant);
        }

        var terminal = new AnimState("dead_loop", isLooping: true);
        var death = new AnimState("die") { NextState = terminal };
        var idle = new AnimState("idle_loop", isLooping: true);
        var revive = new AnimState("revive") { NextState = idle };
        var any = new AnimState("any");
        any.AddBranch(CreatureAnimator.deathTrigger, death);
        any.AddBranch(CreatureAnimator.reviveTrigger, revive);
        PoroDeathAnimationPatch.StopDeathLoop(any);
        if (death.IsLooping || death.GetNextState() != null || revive.GetNextState() != idle)
            throw new InvalidOperationException("Poro death must finish once and still allow revival.");
    }

    private static Player? _localPlayer;

    private static void VerifyAuthorCallbacks(string path)
    {
        var assembly = Assembly.LoadFrom(path);
        RedHoodCompanionCompat.Prepare(assembly);
        var harmony = new Harmony("sc.tests.redhood-ownership");
        var old = SkinService.Config.CompanionSelectionDecoupled;
        var oldVariant = SkinService.Config.CompanionProviderVariantId;
        var localOsty = (Creature)RuntimeHelpers.GetUninitializedObject(typeof(Creature));
        var otherOsty = (Creature)RuntimeHelpers.GetUninitializedObject(typeof(Creature));
        _localPlayer = (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
        localOsty.PetOwner = _localPlayer;
        otherOsty.PetOwner = (Player)RuntimeHelpers.GetUninitializedObject(typeof(Player));
        var monster = AccessTools.Field(typeof(Creature), "<Monster>k__BackingField");
        monster.SetValue(localOsty, RuntimeHelpers.GetUninitializedObject(typeof(Osty)));
        monster.SetValue(otherOsty, RuntimeHelpers.GetUninitializedObject(typeof(Osty)));
        harmony.Patch(AccessTools.DeclaredMethod(typeof(LocalContext), nameof(LocalContext.IsMe), [typeof(Player)]),
            prefix: new HarmonyMethod(typeof(PoroPackTests), nameof(IsLocal)));

        try
        {
            var factory = AccessTools.DeclaredMethod(assembly.GetType("RedHoodNecrobinder.NativeCreatureVisuals"), "Prefix");
            var attach = AccessTools.DeclaredMethod(assembly.GetType("RedHoodNecrobinder.Mod"), "AttachCombat");
            var node = (NCreature)RuntimeHelpers.GetUninitializedObject(typeof(NCreature));
            GC.SuppressFinalize(node);
            AccessTools.Field(typeof(NCreature), "<Entity>k__BackingField").SetValue(node, localOsty);
            SkinService.Config.CompanionSelectionDecoupled = false;
            if (RedHoodCompanionCompat.UsesIndependentOsty(localOsty))
                throw new InvalidOperationException("Automatic companion following was suppressed.");
            SkinService.Config.CompanionSelectionDecoupled = true;
            foreach (var variant in new[] { null, "redhood_bot01", "redhood_maid_bot01" })
            {
                SkinService.Config.CompanionProviderVariantId = variant;
                object?[] args = [localOsty, null];
                if (factory.Invoke(null, args) is not true || args[1] != null)
                    throw new InvalidOperationException("Author factory still owns the independent companion.");
                // No Godot tree exists here. Running the author's callback would access it.
                attach.Invoke(null, [node]);
            }
            var hero = (Creature)RuntimeHelpers.GetUninitializedObject(typeof(Creature));
            if (RedHoodCompanionCompat.UsesIndependentOsty(hero) ||
                RedHoodCompanionCompat.UsesIndependentOsty(otherOsty))
                throw new InvalidOperationException("Combat guard affected the hero or another player's companion.");
        }
        finally
        {
            SkinService.Config.CompanionSelectionDecoupled = old;
            SkinService.Config.CompanionProviderVariantId = oldVariant;
            harmony.UnpatchAll(harmony.Id);
            _localPlayer = null;
        }
    }

    private static bool IsLocal(Player __0, ref bool __result)
    {
        __result = ReferenceEquals(__0, _localPlayer);
        return false;
    }
}

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger;
using STS2SkinChanger.Core;

internal static class MlmOrderGuardTests
{
    internal static void Run(string managerPath, string selfRoot, string skinRoot, string gamePack)
    {
        var silence = AuditLogSilencer.Install();
        var registry = AccessTools.Field(typeof(ModManager), "_mods");
        var previous = registry.GetValue(null);
        var root = Path.Combine(Path.GetTempPath(), "sc-mlm-order-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            Mod Fake(string id)
            {
                var path = Path.Combine(root, id); Directory.CreateDirectory(path);
                File.WriteAllText(Path.Combine(path, id + ".json"), "{\"id\":\"" + id + "\"}");
                return new Mod { path = path, state = ModLoadState.Loaded,
                    manifest = new ModManifest { id = id, name = id, affectsGameplay = true } };
            }
            var manager = Fake("ModLaunchManager");
            var self = new Mod { path = selfRoot, state = ModLoadState.None,
                manifest = new ModManifest { id = Entry.ModId, name = "皮肤切换器", hasDll = true } };
            var skin = new Mod { path = skinRoot, state = ModLoadState.Loaded,
                manifest = new ModManifest { id = "CrusaderSkin", name = "十字军", hasDll = true, hasPck = true, affectsGameplay = false } };
            var content = Fake("UnrelatedContent");
            var bad = new List<Mod> { manager, content, skin, self };
            registry.SetValue(null, bad);
            Require(MlmEarlyLoadOrderGuard.FindViolations(bad, gamePack).SequenceEqual(["十字军"]), "Real skin before us was not identified, or content was falsely blocked.");
            Require(MlmEarlyLoadOrderGuard.FindViolations(bad).SequenceEqual(["十字军"]), "Baseline-free early startup probe missed the real package.");
            Exception? failure = null;
            try { Entry.Initialize(); }
            catch (InvalidOperationException error) { failure = error; }
            Require(failure?.Message.Contains("一键整理") == true && failure.Message.Contains("第 2 位") && failure.Message.Contains("重启"),
                "Bad order must stop at Entry before framework/skin initialization and explain native MLM repair.");
            var native = Assembly.LoadFrom(managerPath);
            var loc = native.GetType("ModLaunchManager.MlmLocManager", true)!;
            AccessTools.Field(loc, "_preferencesLoaded").SetValue(null, true);
            var preferences = AccessTools.Field(loc, "_preferences").GetValue(null)!;
            AccessTools.Property(preferences.GetType(), "Language").SetValue(preferences, "en");
            var entryType = native.GetType("ModLaunchManager.LaunchEntry", true)!;
            var priorities = native.GetType("ModLaunchManager.ModLoadPriorities", true)!;
            var declared = AccessTools.Method(priorities, "Declared").Invoke(null, [self]);
            Require(declared?.ToString() == "First", "MLM cannot read the priority declaration before loading Skin Changer DLL.");
            var stepType = native.GetType("ModLaunchManager.DeferredModLoader+StepResult", true)!;
            var step = Activator.CreateInstance(stepType, [self, 4, 10, TimeSpan.Zero, failure])!;
            Require((bool)AccessTools.Method(native.GetType("ModLaunchManager.ManagerOverlay", true)!, "HasLoadError").Invoke(null, [step])!,
                "The actual MLM load-error path must recognize our initializer guard.");
            var planType = native.GetType("ModLaunchManager.LaunchPlan", true)!;
            var plan = Activator.CreateInstance(planType, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, [bad], null)!;
            var originalEntries = ((IEnumerable)AccessTools.Property(planType, "Entries").GetValue(plan)!).Cast<object>().ToArray();
            var scEntry = originalEntries.Single(entry => (string)AccessTools.Property(entryType, "Id").GetValue(entry)! == Entry.ModId);
            var contentEntry = originalEntries.Single(entry => (string)AccessTools.Property(entryType, "Id").GetValue(entry)! == "UnrelatedContent");
            AccessTools.Property(entryType, "IgnorePriority").SetValue(scEntry, true);
            AccessTools.Property(entryType, "IgnorePriority").SetValue(contentEntry, true);
            var planField = AccessTools.Field(native.GetType("ModLaunchManager.ManagerOverlay", true)!, "_plan");
            var oldPlan = planField.GetValue(null);
            try
            {
                planField.SetValue(null, plan);
                MlmEarlyLoadOrderGuard.RestoreOwnDeclaredPriority();
                Require(!(bool)AccessTools.Property(entryType, "IgnorePriority").GetValue(scEntry)! &&
                         (bool)AccessTools.Property(entryType, "IgnorePriority").GetValue(contentEntry)!,
                    "Repair must re-enable only our declared priority, retaining unrelated user preferences.");
            }
            finally { planField.SetValue(null, oldPlan); }
            var result = AccessTools.Method(planType, "AutoSortDependencies").Invoke(plan, [true, null])!;
            Require((bool)AccessTools.Property(result.GetType(), "Success").GetValue(result)!, "Native one-click organize failed.");
            var entries = ((IEnumerable)AccessTools.Property(planType, "Entries").GetValue(plan)!).Cast<object>().ToArray();
            string Id(object entry) => (string)AccessTools.Property(entryType, "Id").GetValue(entry)!;
            Require(Id(entries[0]) == "ModLaunchManager" && Id(entries[1]) == Entry.ModId,
                "Native one-click organizer must keep MLM first and place Skin Changer second.");
            var ordered = entries.Select(entry => (Mod)AccessTools.Property(entryType, "Mod").GetValue(entry)!).ToList();
            Require(MlmEarlyLoadOrderGuard.FindViolations(ordered, gamePack).Count == 0, "Correctly sorted order was still blocked.");
            Require(MlmEarlyLoadOrderGuard.FindViolations([manager, content, self], gamePack).Count == 0, "Unrelated content before us must not trigger a skin order error.");
            Require(MlmEarlyLoadOrderGuard.FindViolations([skin, self], gamePack).Count == 0, "Without MLM retain the existing standard loader warning behavior.");
            var framework = Fake("STS2-RitsuLib");
            framework.assemblies.Add(AssemblyBuilder.DefineDynamicAssembly(new("MlmSharedFramework"), AssemblyBuilderAccess.RunAndCollect));
            Require(MlmEarlyLoadOrderGuard.FindViolations([manager, framework, self], gamePack).SequenceEqual(["STS2-RitsuLib"]),
                "A shared framework already in a collectible context must stop early even before skin discovery.");
            skin.state = ModLoadState.Disabled;
            Require(MlmEarlyLoadOrderGuard.FindViolations(bad, gamePack).Count == 0, "Disabled skin must not block launch.");
            Console.WriteLine("MLM order guard passed: real skin probe, Entry fail-before-initialization, actual MLM load-error recognition, manifest-first discovery and native one-click organizer places SC directly below MLM; content/disabled/no-MLM negatives preserved.");
        }
        finally { registry.SetValue(null, previous); silence.UnpatchAll(silence.Id); Directory.Delete(root, true); }
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger.Core;

internal static class MlmAssemblyContextTests
{
    internal static void Run(string managerDll)
    {
        var silence = AuditLogSilencer.Install();
        var modsField = AccessTools.Field(typeof(ModManager), "_mods");
        var previous = modsField.GetValue(null);
        var root = Path.Combine(Path.GetTempPath(), "sc-mlm-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            CheckClrBoundary();
            Mod Make(string id, params string[] dependencies)
            {
                var folder = Path.Combine(root, id); Directory.CreateDirectory(folder);
                File.WriteAllText(Path.Combine(folder, id + ".json"),
                    "{\"id\":\"" + id + "\",\"has_dll\":true,\"mod_launch_manager\":{\"load_context\":\"game\"}}");
                return new Mod { path = folder, manifest = new ModManifest { id = id, hasDll = true,
                    dependencies = dependencies.Select(value => new ModDependency(value)).ToList() } };
            }
            var framework = Make("STS2-RitsuLib", "SharedUtility");
            var dependency = Make("SharedUtility", "STS2-RitsuLib"); // cycle must terminate
            var unrelated = Make("UnrelatedContent");
            var skin = Make("JoanofArcIroncladHpwear");
            var all = new List<Mod> { framework, dependency, unrelated, skin };
            modsField.SetValue(null, all);
            Require(MlmAssemblyContextCompat.RequiresGameContext(framework, all) &&
                    MlmAssemblyContextCompat.RequiresGameContext(dependency, all) &&
                    !MlmAssemblyContextCompat.RequiresGameContext(unrelated, all) &&
                    !MlmAssemblyContextCompat.RequiresGameContext(skin, all), "Framework closure crossed into unrelated content or skin execution.");
            var manager = Assembly.LoadFrom(managerDll);
            Require(MlmAssemblyContextCompat.Install(manager), "Actual installed MLM policy was not patched.");
            Require(!MlmAssemblyContextCompat.Install(manager), "Compatibility patches installed twice.");
            var loader = manager.GetType("ModLaunchManager.CollectibleModAssemblyLoader", true)!;
            var field = AccessTools.Field(loader, "_loadDecisions");
            var oldDecisions = field.GetValue(null);
            var decisions = (IDictionary)Activator.CreateInstance(field.FieldType)!;
            var decisionType = field.FieldType.GetGenericArguments()[1];
            foreach (var mod in all) decisions[mod] = Activator.CreateInstance(decisionType, [false, "cached before SC"]);
            field.SetValue(null, decisions);
            try
            {
                var load = AccessTools.Method(loader, "LoadFromCollectibleContext");
                foreach (var mod in all)
                    AccessTools.Method(typeof(MlmAssemblyContextCompat), "BeforeLoad")
                        .Invoke(null, [load, Path.Combine(mod.path, mod.manifest!.id! + ".dll")]);
                foreach (var mod in all)
                    Require((bool)AccessTools.Property(decisionType, "UseGameContext").GetValue(decisions[mod])! ==
                            MlmAssemblyContextCompat.RequiresGameContext(mod, all),
                        "An already-cached MLM launch decision bypassed the compatibility policy.");
                Require(decisions.Count == all.Count, "Compatibility must not add/enable mods or alter launch order.");
                Require(Harmony.GetPatchInfo(load)!.Prefixes.Any(patch => patch.PatchMethod.DeclaringType == typeof(MlmAssemblyContextCompat)),
                    "Actual MLM loader entry was not patched.");
                field.SetValue(null, null);
                AccessTools.Method(typeof(MlmAssemblyContextCompat), "BeforeLoad")
                    .Invoke(null, [load, Path.Combine(framework.path, framework.manifest!.id! + ".dll")]);
                var cold = (IDictionary)field.GetValue(null)!;
                Require((bool)AccessTools.Property(decisionType, "UseGameContext").GetValue(cold[framework])!,
                    "A cold load plan must be prepared through MLM before correcting the framework.");
                Require(all.All(mod => !MlmEarlyLoadOrderGuard.LoadedAssemblies(mod).Any()), "Policy preparation must not initialize provider/framework modules.");
            }
            finally { field.SetValue(null, oldDecisions); }
            Console.WriteLine("MLM compatibility passed: CLR collectible negative/control, actual installed MLM decision struct and loader patch, cold/cached plan correction, cyclic framework dependencies and unrelated-mod isolation.");
        }
        finally { modsField.SetValue(null, previous); silence.UnpatchAll(silence.Id); Directory.Delete(root, true); }
    }
    private static void CheckClrBoundary()
    {
        Type Contract(AssemblyBuilderAccess access) => AssemblyBuilder.DefineDynamicAssembly(new("MlmContract" + Guid.NewGuid().ToString("N")), access)
            .DefineDynamicModule("main").DefineType("Contract", TypeAttributes.Public | TypeAttributes.Interface | TypeAttributes.Abstract).CreateType()!;
        void Consumer(Type contract) => AssemblyBuilder.DefineDynamicAssembly(new("MlmConsumer" + Guid.NewGuid().ToString("N")), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("main").DefineType("Consumer", TypeAttributes.Public, typeof(object), [contract]).CreateType();
        var failed = false;
        try { Consumer(Contract(AssemblyBuilderAccess.RunAndCollect)); }
        catch (NotSupportedException) { failed = true; }
        Require(failed, "Negative control did not reproduce the user's CLR load-context error.");
        Consumer(Contract(AssemblyBuilderAccess.Run));
    }
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

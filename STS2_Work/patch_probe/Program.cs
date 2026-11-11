using System.Reflection;
using HarmonyLib;

var asm = typeof(STS2SkinChanger.Entry).Assembly;
var type = asm.GetType("STS2SkinChanger.Core.BackgroundSkinCreatePatch", true);
var h = new Harmony("probe.bg");
try
{
    h.CreateClassProcessor(type).Patch();
    Console.WriteLine("patch ok");
}
catch (Exception ex)
{
    Console.WriteLine("patch failed: " + ex);
}
var target = typeof(MegaCrit.Sts2.Core.Models.EncounterModel).GetMethod("CreateBackground");
var info = Harmony.GetPatchInfo(target);
Console.WriteLine("target=" + target);
if (info == null) { Console.WriteLine("no patch info"); return; }
foreach (var p in info.Postfixes)
    Console.WriteLine($"postfix owner={p.owner} method={p.PatchMethod.DeclaringType?.Name}.{p.PatchMethod.Name} priority={p.priority}");

// Also check what Harmony would consider unmatched injected parameters
var method = type.GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
Console.WriteLine("postfix params: " + string.Join(", ", method.GetParameters().Select(p => $"{p.Name}:{p.ParameterType.Name}")));
Console.WriteLine("target params: " + string.Join(", ", target.GetParameters().Select(p => $"{p.Name}:{p.ParameterType.Name}")));

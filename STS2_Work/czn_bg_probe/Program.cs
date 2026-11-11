using System.Collections;
using System.Reflection;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Nodes.Rooms;

var path = @"D:\steam\steamapps\workshop\content\2868840\3783041069\CznEnemySkin.dll";
var dir = Path.GetDirectoryName(path)!;
AssemblyLoadContext.Default.Resolving += (ctx, name) =>
{
    var candidate = Path.Combine(dir, name.Name + ".dll");
    return File.Exists(candidate) ? ctx.LoadFromAssemblyPath(candidate) : null;
};
var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
Type[] types;
try { types = asm.GetTypes(); }
catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).Cast<Type>().ToArray(); }

string? ShellName(object battlefield)
{
    var bg = battlefield.GetType().GetProperty("Background", flags)?.GetValue(battlefield);
    var shell = bg?.GetType().GetProperty("ShellScenePath", flags)?.GetValue(bg) as string;
    if (string.IsNullOrWhiteSpace(shell)) return null;
    var name = Path.GetFileNameWithoutExtension(shell);
    return name.EndsWith("_background", StringComparison.OrdinalIgnoreCase) ? name[..^11] : name;
}

var picked = new Dictionary<string, (string Shell, bool Standard)>(StringComparer.OrdinalIgnoreCase);
foreach (var type in types.Where(t => t.Name.Contains("Profile", StringComparison.OrdinalIgnoreCase)))
{
    var prop = type.GetProperty("Profiles", flags);
    if (prop == null || prop.GetIndexParameters().Length != 0) continue;
    IEnumerable elements;
    try { if (prop.GetValue(null) is not IEnumerable v) continue; elements = v; } catch { continue; }
    foreach (var element in elements)
    {
        if (element == null) continue;
        var et = element.GetType();
        var target = et.GetProperty("Target", flags)?.GetValue(element);
        var actId = target?.GetType().GetProperty("ActId", flags)?.GetValue(target) as string;
        if (string.IsNullOrWhiteSpace(actId)) continue;
        var battlefield = et.GetProperty("Battlefield", flags)?.GetValue(element);
        if (battlefield == null) continue;
        var shell = ShellName(battlefield);
        if (string.IsNullOrWhiteSpace(shell)) continue;
        var scope = target?.GetType().GetProperty("RoomScope", flags)?.GetValue(target)?.ToString();
        var standard = string.Equals(scope, "StandardCombat", StringComparison.OrdinalIgnoreCase);
        if (picked.TryGetValue(actId, out var existing) && (existing.Standard || !standard)) continue;
        picked[actId] = (shell!, standard);
    }
}
foreach (var pair in picked) Console.WriteLine($"{pair.Key}: {pair.Value.Shell} standard={pair.Value.Standard}");

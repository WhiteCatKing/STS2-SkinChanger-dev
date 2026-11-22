using System.Collections;
using System.Reflection;
using HarmonyLib;

namespace STS2SkinChanger.Catalog;

internal sealed record ManualCharacterState(string TypeName, string FieldName, bool IsStatic,
    string ToggleTypeName, string ToggleMethod, string? SaveMethod, string[] SourceSlots,
    bool AppliesArgument = false)
{
    public string Id => TypeName + ":" + FieldName;
}

internal sealed record ManualCharacterVariant(ManualCharacterState State, bool Value);

/// <summary>Metadata only. Recognizes manual slot-alpha and saved shader-palette contracts;
/// never loads executable providers just to populate the skin list.</summary>
internal static class ManualCharacterVariantScanner
{
    private static readonly Dictionary<string, (long Length, DateTime Modified, ManualCharacterState[] States)> Cache =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<(Type, string), PropertyInfo?> Properties = [];

    internal static ManualCharacterState[] ScanAssembly(string path)
    {
        var file = new FileInfo(path);
        lock (Cache)
        {
            if (Cache.TryGetValue(path, out var cached) && cached.Length == file.Length && cached.Modified == file.LastWriteTimeUtc)
                return cached.States;
            using var input = File.OpenRead(path);
            var definitionType = typeof(Harmony).Assembly.GetType("Mono.Cecil.AssemblyDefinition", true)!;
            var definition = definitionType.GetMethod("ReadAssembly", [typeof(Stream)])!.Invoke(null, [input])!;
            try
            {
                var types = WalkTypes(Get(Get(definition, "MainModule"), "Types")).ToArray();
                var methods = types.SelectMany(t => Items(Get(t, "Methods"))).Where(m => (bool)Get(m, "HasBody")).ToArray();
                var bodies = methods.ToDictionary(m => m, Body);
                var togglesByField = methods.SelectMany(method => FindToggledFields(bodies[method])
                        .Select(field => (Field: field, Method: method)))
                    .GroupBy(pair => pair.Field, StringComparer.Ordinal)
                    .ToDictionary(group => group.Key, group => group.Select(pair => pair.Method).Distinct().ToArray(), StringComparer.Ordinal);
                var states = new List<ManualCharacterState>();
                foreach (var type in types)
                foreach (var field in Items(Get(type, "Fields")).Where(f => Text(f, "FieldType") == "System.Boolean"))
                {
                    var isStatic = (bool)Get(field, "IsStatic");
                    var fieldId = Text(field, "FullName");
                    if (!togglesByField.TryGetValue(fieldId, out var toggles))
                    {
                        // Second shape: the provider stores the variant in a static flag and applies it
                        // through its own argument-taking entry point (SetVariant(bool …)) instead of
                        // flipping the flag itself. Same meaning, so both become one switchable channel.
                        if (isStatic && FindArgumentApplier(methods, bodies, fieldId) is { } argumentApplier)
                            states.Add(new(Text(type, "FullName").Replace('/', '+'), Text(field, "Name"), true,
                                Text(argumentApplier, "DeclaringType").Replace('/', '+'), Text(argumentApplier, "Name"),
                                null, [], true));
                        continue;
                    }
                    foreach (var toggle in toggles)
                    {
                        string? save = null;
                        string[] slots = [];
                        if (!isStatic)
                        {
                            if (Text(toggle, "Name") != "_Input" || Text(toggle, "DeclaringType") != Text(type, "FullName")) continue;
                            var arrays = Items(Get(type, "Fields")).Where(f => Text(f, "FieldType") == "System.String[]").ToArray();
                            if (arrays.Length != 1) continue;
                            var apply = methods.FirstOrDefault(m => Text(m, "DeclaringType") == Text(type, "FullName") &&
                                Calls(bodies[toggle], m) && Reads(bodies[m], fieldId) &&
                                bodies[m].Any(i => Operand(i) is string s && s == "get_color") &&
                                bodies[m].Any(i => Operand(i) is string s && s == "set_color") &&
                                bodies[m].Any(i => Op(i) == "ldc.r4" && Equals(Operand(i), 0f)) &&
                                bodies[m].Any(i => Op(i) == "stfld" && Text(Operand(i), "Name") == "A" &&
                                    Text(Operand(i), "DeclaringType") == "Godot.Color"));
                            if (apply == null) continue;
                            slots = ReadArrayInitializer(methods, bodies, Text(arrays[0], "FullName"));
                            if (slots.Length is 0 or > 256) continue;
                        }
                        else
                        {
                            // A persisted flag alone may be gameplay/debug state. Require BOTH a
                            // button's Pressed delegate and a shader writer reading this exact field.
                            if (!methods.Any(m => Calls(bodies[m], "Godot.BaseButton", "add_Pressed") && References(bodies[m], toggle)) ||
                                !methods.Any(m => Reads(bodies[m], fieldId) && Calls(bodies[m], "Godot.ShaderMaterial", "SetShaderParameter"))) continue;
                            var saver = methods.FirstOrDefault(m => (bool)Get(m, "IsStatic") &&
                                !Items(Get(m, "Parameters")).Any() && Reads(bodies[m], fieldId) &&
                                Calls(bodies[m], "Godot.ConfigFile", "Save") && Calls(bodies[m], "Godot.ConfigFile", "SetValue") &&
                                Calls(bodies[toggle], m) && Text(m, "DeclaringType") == Text(type, "FullName"));
                            if (saver == null) continue;
                            save = Text(saver, "Name");
                        }
                        states.Add(new(Text(type, "FullName").Replace('/', '+'), Text(field, "Name"), isStatic,
                            Text(toggle, "DeclaringType").Replace('/', '+'), Text(toggle, "Name"), save, slots));
                        break;
                    }
                }
                var result = states.DistinctBy(s => s.Id).ToArray();
                Cache[path] = (file.Length, file.LastWriteTimeUtc, result);
                return result;
            }
            finally { ((IDisposable)definition).Dispose(); }
        }
    }

    internal static void Expand(IReadOnlyCollection<PckResourceIndex> indexes,
        IDictionary<string, SkinGroup> groups, IReadOnlySet<string> characterGroups)
    {
        foreach (var index in indexes)
        {
            if (!index.Mod.HasDll) continue;
            var options = groups.Values.Where(g => characterGroups.Contains(g.Id))
                .SelectMany(g => g.Options.Where(o => o.EffectiveProviderId == index.Mod.Id).Select(o => (Group: g, Option: o))).ToArray();
            if (options.Length == 0 || !Directory.Exists(index.Mod.RootPath)) continue;
            try
            {
                var primary = SkinPackagePaths.Resolve(index.Mod.RootPath, index.Mod.ResourceNamespaceId, ".dll");
                var paths = File.Exists(primary) ? [primary] : Directory.GetFiles(index.Mod.RootPath, "*.dll");
                if (!File.Exists(primary) && paths.Length != 1) continue;
                var states = paths.SelectMany(ScanAssembly).DistinctBy(s => s.Id).ToArray();
                // Ambiguous independent controls need a richer author contract. Never manufacture
                // combinations or choose an arbitrary global flag that could affect another role.
                if (states.Length != 1 || options.Select(p => p.Group.Id).Distinct().Count() != 1) continue;
                foreach (var (group, option) in options)
                {
                    var at = group.Options.IndexOf(option);
                    group.Options.RemoveAt(at);
                    foreach (var value in new[] { false, true })
                        group.Options.Insert(at++, option with
                        {
                            Id = value ? option.Id + "::manual:" + Uri.EscapeDataString(states[0].Id) + ":1" : option.Id,
                            Name = CardSkinOptionNamingPolicy.Build(option.Name, null, value ? 2 : 1, 2),
                            ProviderId = option.EffectiveProviderId,
                            ManualCharacterVariant = new(states[0], value)
                        });
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"手动角色差分扫描失败 {index.Mod.Id}：{exception.GetBaseException().Message}");
            }
        }
    }

    /// <summary>A provider-side applier: a static entry point whose first argument is the variant
    /// flag it stores (through that flag's own setter) and which the control that switches variants
    /// reaches, directly or through one local function. Also requires the flag to be read elsewhere
    /// so unrelated gameplay/debug booleans never become skin options.</summary>
    private static object? FindArgumentApplier(object[] methods, Dictionary<object, object[]> bodies, string field)
    {
        var setters = methods.Where(method => Text(method, "Name").StartsWith("set_", StringComparison.Ordinal) &&
                Items(Get(method, "Parameters")).Count() == 1 &&
                Text(Items(Get(method, "Parameters")).First(), "ParameterType") == "System.Boolean" &&
                Stores(bodies[method], field) && bodies[method].Any(i => Op(i) == "ldarg.0")).ToArray();
        if (setters.Length == 0) return null;
        var handlers = methods.Where(method => bodies[method].Any(instruction => Op(instruction) is "call" or "callvirt" &&
                Text(Operand(instruction), "Name") == "add_ItemSelected"))
            .SelectMany(method => bodies[method].Where(i => Op(i) == "ldftn").Select(Operand))
            .Where(handler => handler != null)
            .Select(handler => Text(handler, "FullName"))
            .ToHashSet(StringComparer.Ordinal);
        if (handlers.Count == 0) return null;

        bool Reaches(object applier)
        {
            foreach (var handler in methods.Where(method => handlers.Contains(Text(method, "FullName"))))
            {
                if (Calls(bodies[handler], applier)) return true;
                if (methods.Any(middle => Calls(bodies[handler], middle) && Calls(bodies[middle], applier))) return true;
            }
            return false;
        }

        foreach (var method in methods)
        {
            if (!(bool)Get(method, "IsStatic")) continue;
            var parameters = Items(Get(method, "Parameters")).ToArray();
            if (parameters.Length < 2 || Text(parameters[0], "ParameterType") != "System.Boolean") continue;
            var body = bodies[method];
            var storesFirstArgument = setters.Any(setter => Enumerable.Range(0, Math.Max(0, body.Length - 1)).Any(i =>
                Op(body[i]) == "ldarg.0" && Op(body[i + 1]) is "call" or "callvirt" &&
                Text(Operand(body[i + 1]), "FullName") == Text(setter, "FullName")));
            if (!storesFirstArgument) continue;
            if (!methods.Any(other => other != method && Reads(bodies[other], field))) continue;
            if (!Reaches(method)) continue;
            return method;
        }
        return null;
    }

    private static IEnumerable<string> FindToggledFields(object[] body)    {
        for (var i = 0; i + 3 < body.Length; i++)
            if (Op(body[i]) is "ldsfld" or "ldfld" && Text(Operand(body[i]), "FieldType") == "System.Boolean" &&
                Op(body[i + 1]) == "ldc.i4.0" && Op(body[i + 2]) == "ceq" &&
                Op(body[i + 3]) == (Op(body[i]) == "ldsfld" ? "stsfld" : "stfld") &&
                Text(Operand(body[i + 3]), "FullName") == Text(Operand(body[i]), "FullName"))
                yield return Text(Operand(body[i]), "FullName");
    }
    private static string[] ReadArrayInitializer(object[] methods, Dictionary<object, object[]> bodies, string field)
    {
        foreach (var method in methods.Where(m => Text(m, "Name") is ".ctor" or ".cctor"))
        {
            var body = bodies[method];
            var end = Array.FindIndex(body, i => Op(i) is "stfld" or "stsfld" && Text(Operand(i), "FullName") == field);
            if (end < 0) continue;
            var start = Array.FindLastIndex(body, end, i => Op(i) == "newarr" && Text(Operand(i), "FullName") == "System.String");
            if (start >= 0) return body[(start + 1)..end].Where(i => Op(i) == "ldstr").Select(i => (string)Operand(i)!).ToArray();
        }
        return [];
    }
    private static bool Reads(object[] body, string field) => body.Any(i => Op(i) is "ldfld" or "ldsfld" && Text(Operand(i), "FullName") == field);
    private static bool Stores(object[] body, string field) => body.Any(i => Op(i) is "stfld" or "stsfld" && Text(Operand(i), "FullName") == field);
    private static bool Calls(object[] body, object method) => body.Any(i => Op(i) is "call" or "callvirt" && Text(Operand(i), "FullName") == Text(method, "FullName"));
    private static bool Calls(object[] body, string type, string name) => body.Any(i => Op(i) is "call" or "callvirt" && Text(Operand(i), "DeclaringType") == type && Text(Operand(i), "Name") == name);
    private static bool References(object[] body, object method) => body.Any(i => Op(i) == "ldftn" && Text(Operand(i), "FullName") == Text(method, "FullName"));
    private static object[] Body(object method) => Items(Get(Get(method, "Body"), "Instructions")).Where(i => Op(i) != "nop").ToArray();
    private static IEnumerable<object> WalkTypes(object values) { foreach (var type in Items(values)) { yield return type; foreach (var nested in WalkTypes(Get(type, "NestedTypes"))) yield return nested; } }
    private static object Get(object value, string name) => Property(value, name)!;
    private static object? Operand(object value) => Get(value, "Operand");
    private static string Op(object value) => Text(value, "OpCode");
    private static string Text(object? value, string name) => Property(value, name)?.ToString() ?? string.Empty;
    private static object? Property(object? value, string name)
    {
        if (value == null) return null;
        var key = (value.GetType(), name);
        if (!Properties.TryGetValue(key, out var property))
        {
            property = key.Item1.GetProperties().Where(candidate => candidate.Name == name)
                .OrderByDescending(candidate => candidate.DeclaringType == key.Item1).FirstOrDefault();
            Properties[key] = property;
        }
        return property?.GetValue(value);
    }
    private static IEnumerable<object> Items(object value) => ((IEnumerable)value).Cast<object>();
}

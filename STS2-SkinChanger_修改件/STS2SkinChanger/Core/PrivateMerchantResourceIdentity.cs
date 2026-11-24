using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;

namespace STS2SkinChanger.Core;

// The author's asynchronous target iterator compares skeleton ResourcePath with the vanilla
// merchant paths. A resource displaced from the canonical cache keeps its isolated alias.
// Expose its recorded identity ONLY at those proven comparisons, never change ResourcePath,
// cache ownership, unrelated authors, or the private replacement skeleton's identity.
internal static class PrivateMerchantResourceIdentity
{
    internal const string Body = "res://animations/backgrounds/merchant_room/top/shop_merchant_top.tres";
    internal const string Hand = "res://animations/backgrounds/merchant_room/hand/merchant_hand_skel_data.tres";
    private const string FakeBody = "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres";
    private const string FakeHand = "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres";
    private static readonly string[] CanonicalPaths = [Body, Hand, FakeBody, FakeHand];
    private sealed record Identity(string Path);
    private static readonly ConditionalWeakTable<Resource, Identity> Identities = new();
    private static readonly HashSet<MethodInfo> Installed = [];
    private static readonly HashSet<Assembly> Inspected = [];
    private static readonly Harmony Patches = new(Entry.ModId + ".private_merchant_identity");

    internal static void Register(Resource resource, string canonical)
    {
        if (!CanonicalPaths.Contains(canonical)) return;
        Identities.Remove(resource);
        Identities.Add(resource, new(canonical));
    }

    internal static string ReadPath(Resource resource)
    {
        if (Identities.TryGetValue(resource, out var identity)) return identity.Path;
        var actual = resource.ResourcePath;
        // IgnoreDeep can give the instantiated scene its own dependency instance. Such a copy
        // is not in the ownership table, but its SC-authored alias retains the complete vanilla
        // source path. Match only the native merchant paths inside our reserved namespace.
        if (actual.StartsWith("res://sts2_skin_runtime/", StringComparison.Ordinal))
        {
            foreach (var path in CanonicalPaths)
                if (actual.EndsWith("/" + path[6..], StringComparison.Ordinal)) return path;
        }
        return actual;
    }

    internal static int Install(Assembly assembly)
    {
        if (Inspected.Contains(assembly)) return 0;
        var count = 0;
        Type[] types;
        try { types = assembly.GetTypes(); }
        catch (ReflectionTypeLoadException error) { types = error.Types.OfType<Type>().ToArray(); }
        foreach (var method in types.SelectMany(type => type.GetMethods(BindingFlags.Instance |
                     BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)))
        {
            if (Installed.Contains(method) || method.ContainsGenericParameters || method.GetMethodBody() == null) continue;
            var body = PatchProcessor.GetOriginalInstructions(method);
            if (!body.Any(instruction => instruction.opcode == OpCodes.Ldstr &&
                    instruction.operand is string path && CanonicalPaths.Contains(path)) ||
                !body.Any(IsPathGetter)) continue;
            Patches.Patch(method, transpiler: new HarmonyMethod(typeof(PrivateMerchantResourceIdentity), nameof(Rewrite)));
            Installed.Add(method);
            count++;
        }
        Inspected.Add(assembly);
        if (count > 0) ModLog.Info($"已接入私有商人的骨架身份比较：{assembly.GetName().Name}，{count} 个入口（保留资源隔离路径）。");
        return count;
    }

    private static bool IsPathGetter(CodeInstruction instruction) =>
        instruction.Calls(AccessTools.PropertyGetter(typeof(Resource), nameof(Resource.ResourcePath)));

    private static IEnumerable<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (IsPathGetter(instruction))
            {
                instruction.opcode = OpCodes.Call;
                instruction.operand = AccessTools.Method(typeof(PrivateMerchantResourceIdentity), nameof(ReadPath));
            }
            yield return instruction;
        }
    }
}

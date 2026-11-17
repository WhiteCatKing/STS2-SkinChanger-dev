using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Core;

// Portraits belong to each saved character selection, not the currently focused runtime.
// Read only icon getter/Init postfixes; never initialize a provider or replay SelectCharacter.
internal static class CharacterStripPortraits
{
    internal sealed record Callback(MethodInfo Method, MethodBase Target, int Priority);
    private static readonly Dictionary<Assembly, Callback[]> Contracts = new();

    internal static Callback[] ReadContracts(Assembly assembly)
    {
        if (Contracts.TryGetValue(assembly, out var cached)) return cached;
        var result = new List<Callback>();
        foreach (var type in assembly.GetTypes())
        foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (!method.IsDefined(typeof(HarmonyPostfix)) && method.Name != "Postfix") continue;
            var info = HarmonyMethodExtensions.GetMergedFromType(type)
                .Merge(HarmonyMethodExtensions.GetMergedFromMethod(method));
            var parameters = method.GetParameters();
            var getter = parameters.Any(p => p.Name == "__result" && p.ParameterType.IsByRef &&
                typeof(Texture2D).IsAssignableFrom(p.ParameterType.GetElementType()));
            var targets = new List<MethodBase>();
            if (info.declaringType == typeof(NCharacterSelectButton) && info.methodName == "Init")
                targets.Add(AccessTools.Method(typeof(NCharacterSelectButton), "Init"));
            if (getter && info.declaringType == typeof(CharacterModel))
            {
                var name = info.methodName;
                if (info.methodType == MethodType.Getter) name = "get_" + name;
                if (name is "get_CharacterSelectIcon" or "get_CharacterSelectLockedIcon")
                    targets.Add(AccessTools.Method(typeof(CharacterModel), name));
            }
            // Harmony's explicit TargetMethods contract is used by packed/embedded icon skins.
            // Only inspect it for a texture-result postfix, never unrelated gameplay patch classes.
            if (getter && info.declaringType == null &&
                AccessTools.DeclaredMethod(type, "TargetMethods", Type.EmptyTypes) is { } resolver &&
                resolver.IsStatic && typeof(IEnumerable<MethodBase>).IsAssignableFrom(resolver.ReturnType))
            {
                try
                {
                    if (resolver.Invoke(null, null) is IEnumerable<MethodBase> originals)
                        targets.AddRange(originals.Where(t => t.DeclaringType == typeof(CharacterModel) &&
                            t.Name is "get_CharacterSelectIcon" or "get_CharacterSelectLockedIcon"));
                }
                catch (Exception e) { ModLog.Warn("读取头像声明失败：" + e.GetBaseException().Message); }
            }
            result.AddRange(targets.Distinct().Select(t => new Callback(method, t,
                info.priority < 0 ? Priority.Normal : info.priority)));
        }
        return Contracts[assembly] = result.OrderByDescending(c => c.Priority).ToArray();
    }

    internal static Texture2D? ReplayGetter(Assembly assembly, CharacterModel character, bool locked,
        Texture2D? baseline)
    {
        var name = locked ? "get_CharacterSelectLockedIcon" : "get_CharacterSelectIcon";
        foreach (var callback in ReadContracts(assembly).Where(c => c.Target.Name == name))
        {
            var parameters = callback.Method.GetParameters();
            var values = new object?[parameters.Length];
            var valid = true;
            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                values[i] = p.Name switch
                {
                    "__instance" when p.ParameterType.IsInstanceOfType(character) => character,
                    "__originalMethod" when p.ParameterType == typeof(MethodBase) => callback.Target,
                    "__result" when p.ParameterType.IsByRef &&
                        (baseline == null || p.ParameterType.GetElementType()!.IsInstanceOfType(baseline)) => baseline,
                    _ => null
                };
                if (values[i] == null && p.Name != "__result") valid = false;
            }
            if (!valid) continue;
            callback.Method.Invoke(null, values);
            var index = Array.FindIndex(parameters, p => p.Name == "__result");
            if (index >= 0 && values[index] is Texture2D texture) baseline = texture;
        }
        return baseline;
    }

    internal static void Refresh(NCharacterSelectButton button, CharacterModel character, Action baseline)
    {
        var groupId = character.Id.Entry.ToLowerInvariant();
        var canonical = $"res://images/packed/character_select/char_select_{groupId}{(button.IsLocked ? "_locked" : "")}.png";
        if (!SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(groupId, canonical))
        {
            baseline();
            return;
        }
        var provider = SkinService.GetSelectedRuntimeProvider(groupId);
        var assembly = provider == null ? null : ManagedSkinModLoader.GetActiveProviderAssembly(provider);
        if (assembly == null && provider != null &&
            ManagedSkinModLoader.TryGetProviderAssemblyForInspection(provider, out var inspected)) assembly = inspected;
        if (assembly == null || ReadContracts(assembly).Length == 0) { baseline(); return; }
        SkinService.WithRuntimeResources(groupId, Array.Empty<string>(), _ =>
        {
            baseline();
            var icon = button.GetNodeOrNull<TextureRect>("%Icon");
            if (icon == null) return false;
            if (ManagedSkinModLoader.GetActiveProviderAssembly(provider!) == null)
                icon.Texture = ReplayGetter(assembly, character, button.IsLocked, icon.Texture);
            // Init-only packs usually have no locked portrait. Preserve the native lock state.
            if (!button.IsLocked)
                foreach (var callback in ReadContracts(assembly).Where(c => c.Target.Name == "Init"))
                    ManagedSkinModLoader.ReplayCharacterButtonIcon(callback.Method, callback.Target, button, character);
            if (icon.Texture is { } texture) Ui.ContextualSkinControls.SetCharacterButtonIcon(button, texture);
            return true;
        }, includeProviderDependencies: true);
    }
}

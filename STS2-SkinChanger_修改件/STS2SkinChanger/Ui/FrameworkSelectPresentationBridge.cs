using System.Reflection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace STS2SkinChanger.Ui;

/// <summary>
/// Re-applies a CZN-style skin framework's character-select presentation after Skin Changer
/// rebuilt the screen.
///
/// <para>
/// Packs built on the shared "CharacterSkinModBootstrap" runtime (for example
/// Chaos Zero Nightmare - Mei Lin) do not ship canonical character-select art: they construct the
/// background scene and the large portrait at runtime through
/// <c>CharacterSelectPresentationRuntime.ApplyScreenSelection</c>, which the pack calls from its
/// <c>CharacterSelectedEvent</c> handler. Skin Changer rebuilds the select screen afterwards
/// (clearing provider leftovers and applying asset-based skins), so the freshly built background
/// and portrait are removed and the screen falls back to vanilla even though the combat model
/// keeps working.
/// </para>
///
/// <para>
/// The bridge finds those runtime instances (each pack stores one in a static field) and asks them
/// to re-apply for the currently selected character. The call is idempotent by design — the
/// framework restores the native host before adding its own layers — and it is silently skipped
/// when no such framework is loaded.
/// </para>
/// </summary>
internal static class FrameworkSelectPresentationBridge
{
    private static readonly List<(object Instance, MethodInfo Apply)> Runtimes = [];
    private static int _scannedAssemblyCount = -1;

    public static void Reapply(
        NCharacterSelectScreen screen,
        CharacterModel character,
        NCharacterSelectButton? button)
    {
        try
        {
            Scan();
            if (Runtimes.Count == 0 || button == null)
            {
                return;
            }

            foreach (var (instance, apply) in Runtimes)
            {
                try
                {
                    apply.Invoke(instance, [screen, button, character]);
                }
                catch
                {
                    // One pack failing must not block the others or the vanilla screen.
                }
            }
        }
        catch
        {
            // Presentation repair only.
        }
    }

    private static void Scan()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        if (assemblies.Length == _scannedAssemblyCount)
        {
            return;
        }

        _scannedAssemblyCount = assemblies.Length;
        foreach (var assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException partial)
            {
                types = partial.Types.Where(type => type != null).Cast<Type>().ToArray();
            }
            catch
            {
                continue;
            }

            foreach (var type in types)
            {
                foreach (var field in type.GetFields(
                             BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!field.FieldType.Name.Equals(
                            "CharacterSelectPresentationRuntime",
                            StringComparison.Ordinal))
                    {
                        continue;
                    }

                    object? instance;
                    try
                    {
                        instance = field.GetValue(null);
                    }
                    catch
                    {
                        continue;
                    }

                    if (instance == null ||
                        Runtimes.Any(entry => ReferenceEquals(entry.Instance, instance)))
                    {
                        continue;
                    }

                    var apply = instance.GetType().GetMethod(
                        "ApplyScreenSelection",
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (apply != null)
                    {
                        Runtimes.Add((instance, apply));
                    }
                }
            }
        }
    }
}

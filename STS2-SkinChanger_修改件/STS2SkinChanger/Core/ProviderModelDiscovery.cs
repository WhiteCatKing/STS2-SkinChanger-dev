using System.Collections;
using System.Reflection;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Modding;

namespace STS2SkinChanger.Core;

/// <summary>
/// Skin providers are loaded by Skin Changer itself instead of the game's mod loader, so their
/// assemblies never enter <see cref="Mod"/>.<c>assemblies</c> and therefore never reach
/// <c>ReflectionHelper.ModTypes</c>/<c>ModelDb.Init</c>. Model types declared by a provider (most
/// importantly <see cref="SingletonModel"/> combat/run hook listeners) are then silently absent:
/// the skin still renders, but its per-card/per-room hooks never run.
///
/// This bridge publishes the provider assembly into the owning mod's assembly list before
/// <c>ModelDb.Init</c> and invalidates the cached type list. If the database is already
/// initialized (provider selected later at runtime), it instantiates the missing singleton models
/// so their constructors can subscribe their hooks. Modern providers can declare an explicit
/// custom AbstractModel subscription contract instead of the removed game SingletonModel.
/// </summary>
internal static class ProviderModelDiscovery
{
    private static readonly object Sync = new();
    private static readonly HashSet<Assembly> Published = [];
    private static readonly List<object> SingletonKeepAlive = [];
    private static readonly FieldInfo? ModTypesField = typeof(ReflectionHelper).GetField(
        "_modTypes",
        BindingFlags.Static | BindingFlags.NonPublic);

    // Never reference SingletonModel by type here: this code is reached from the Harmony patch on
    // OneTimeInitialization.ExecuteEssential, and v0.113.0 deleted that type. A compile-time token
    // would make the patched method throw TypeLoadException while the game is still starting up.
    private static readonly Type? SingletonModelType = typeof(ModelDb).Assembly.GetType(
        "MegaCrit.Sts2.Core.Models.SingletonModel",
        throwOnError: false,
        ignoreCase: false);

    private static bool _missingSingletonModelLogged;

    public static void Publish(Assembly? assembly, string providerId)    {
        if (assembly == null)
        {
            return;
        }

        lock (Sync)
        {
            if (!Published.Add(assembly))
            {
                return;
            }

            var mod = FindOwningMod(assembly, providerId);
            if (mod != null)
            {
                try
                {
                    PublishAssemblyToMod(mod, assembly);
                    ModTypesField?.SetValue(null, null);
                }
                catch (Exception exception)
                {
                    ModLog.Warn(
                        $"把皮肤提供者 {providerId} 的程序集并入模型类型发现失败：" +
                        exception.GetBaseException().Message);
                }
            }

            InstantiateMissingSingletons(assembly, providerId);
        }
    }

    /// <summary>
    /// Runs after <c>ModelDb.Init</c>. Even when a provider assembly was published before init,
    /// some branches only materialize models lazily; construct any still-missing singleton so its
    /// hook subscription exists before the first run starts.
    /// </summary>
    public static void InstantiatePublishedAfterInit()
    {
        lock (Sync)
        {
            foreach (var assembly in Published)
            {
                InstantiateMissingSingletons(assembly, assembly.GetName().Name ?? "provider");
            }
        }
    }

    private static void PublishAssemblyToMod(Mod mod, Assembly assembly)
    {
        var modType = mod.GetType();
        var listField = modType.GetField("assemblies",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (listField?.GetValue(mod) is IList list)
        {
            if (!list.Contains(assembly))
            {
                list.Add(assembly);
                ModLog.Info($"已把皮肤提供者 {mod.manifest?.id} 的程序集纳入模型类型发现，其单例/战斗钩子可按原版注册。");
            }
            return;
        }

        var singleField = modType.GetField("assembly",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (singleField == null)
        {
            return;
        }

        if (singleField.GetValue(mod) is Assembly existing)
        {
            if (existing != assembly)
            {
                ModLog.Warn(
                    $"皮肤提供者 {mod.manifest?.id} 已有程序集 {existing.GetName().Name}，" +
                    $"无法再挂入 {assembly.GetName().Name}；请检查是否重复加载。");
            }
            return;
        }

        singleField.SetValue(mod, assembly);
        ModLog.Info($"已把皮肤提供者 {mod.manifest?.id} 的程序集纳入模型类型发现，其单例/战斗钩子可按原版注册。");
    }

    private static Mod? FindOwningMod(Assembly assembly, string providerId)
    {
        var location = string.IsNullOrWhiteSpace(assembly.Location)
            ? null
            : Path.GetFullPath(assembly.Location);
        foreach (var mod in ModManager.GetLoadedMods())
        {
            if (location != null && !string.IsNullOrWhiteSpace(mod.path))
            {
                var root = Path.GetFullPath(mod.path)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                if (location.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    return mod;
                }
            }

            if (string.Equals(mod.manifest?.id, providerId, StringComparison.OrdinalIgnoreCase))
            {
                return mod;
            }
        }

        return null;
    }

    private static void InstantiateMissingSingletons(Assembly assembly, string providerId)
    {
        // Before ModelDb.Init the assembly publication above is enough: ModelDb constructs every
        // discovered model itself. Only a provider loaded after initialization needs this fallback.
        if (!ModelDb.Contains(typeof(Ironclad)))
        {
            return;
        }

        CrusaderHookBridge.Install(assembly);
        foreach (var type in SafeTypes(assembly).Where(type => type != null && CrusaderHookBridge.IsHookModel(type)).Cast<Type>())
        {
            if (ModelDb.Contains(type)) continue;
            try
            {
                // Inject registers the instance as well as running its subscribing constructor.
                // A keep-alive-only instance would not prevent repeated activation from subscribing twice.
                ModelDb.Inject(type);
                ModLog.Info($"已补注册皮肤提供者 {providerId} 的新版战斗/房间钩子模型 {type.Name}。");
            }
            catch (Exception exception)
            {
                ModLog.Warn($"补注册新版皮肤钩子 {type.Name} 失败：" + exception.GetBaseException().Message);
            }
        }

        if (SingletonModelType == null)
        {
            if (!_missingSingletonModelLogged)
            {
                _missingSingletonModelLogged = true;
                ModLog.Info(
                    "当前游戏版本没有 SingletonModel 类型，跳过旧型单例补注册；" +
                    "已识别的新版提供者钩子按其模型合同补注册。");
            }

            return;
        }

        foreach (var type in SafeTypes(assembly))
        {
            if (type == null || !type.IsClass || type.IsAbstract ||
                SingletonModelType == null ||
                !SingletonModelType.IsAssignableFrom(type) ||
                ModelDb.Contains(type))
            {
                continue;
            }

            try
            {
                var instance = Activator.CreateInstance(type);
                if (instance != null)
                {
                    SingletonKeepAlive.Add(instance);
                    ModLog.Info($"已补注册皮肤提供者 {providerId} 的单例模型 {type.Name}，其战斗/房间钩子已启用。");
                }
            }
            catch (Exception exception)
            {
                // The game may have registered the type but not constructed it yet; in that case
                // its own lifecycle will subscribe the hooks and a duplicate is harmless.
                ModLog.Warn(
                    $"补注册皮肤提供者 {providerId} 的单例模型 {type.Name} 失败：" +
                    exception.GetBaseException().Message);
            }
        }
    }

    private static IEnumerable<Type?> SafeTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types;
        }
        catch
        {
            return [];
        }
    }
}

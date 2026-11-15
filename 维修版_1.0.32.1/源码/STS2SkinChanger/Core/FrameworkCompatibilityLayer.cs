using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using System.Reflection;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

/// <summary>
/// Uses an enabled original framework cooperatively; otherwise supplies the behavior-free API
/// fallback. Only one assembly may own a framework identity in the game's load context.
/// </summary>
internal static class FrameworkCompatibilityLayer
{
    private const string BundledAdapterFileName = "thunninoiSkinManager.dll";
    private const string RegistryTypeName =
        "thunninoiSkinManager.thunninoiSkinManagerCode.SkinRegistry";
    private static readonly HashSet<string> AvailableAssemblies =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> KnownFrameworkAssemblies =
        new(StringComparer.OrdinalIgnoreCase);
    private static readonly DeferredRegistrationQueue<Assembly> ProviderRegistrations = new();
    private static Assembly? _adapterAssembly;
    private static MethodInfo? _setActiveSkin;
    private static MethodInfo? _skinDbSetup;
    private static Type? _registryType;

    public static IReadOnlyCollection<string> CompatibilityAssemblyNames => AvailableAssemblies;

    public static void Initialize()
    {
        if (_adapterAssembly != null)
        {
            return;
        }

        try
        {
            var root = Path.GetDirectoryName(typeof(Entry).Assembly.Location);
            var adapterPath = root == null
                ? null
                : Path.GetFullPath(Path.Combine(root, BundledAdapterFileName));
            if (adapterPath == null || !File.Exists(adapterPath))
            {
                ModLog.Warn("未找到内置皮肤框架兼容层；声明依赖该框架的皮肤将交回游戏加载器。");
                return;
            }

            var identity = AssemblyName.GetAssemblyName(adapterPath);
            var assemblyName = identity.Name;
            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                ModLog.Warn("内置皮肤框架兼容层没有有效的 CLR 程序集名称。");
                return;
            }

            KnownFrameworkAssemblies.Add(assemblyName);

            TryBindOriginalFramework();
            if (FrameworkRegistryCooperation.IsActive) return;

            if (!ManagedSkinModLoader.CanInstallFrameworkCompatibilityAssembly(assemblyName))
            {
                ModLog.Info(
                    $"未启用内置皮肤框架兼容层 {assemblyName}：" +
                    "原管理器已启用，或当前没有需要后备接口的完整皮肤契约。");
                return;
            }

            var loaded = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(candidate =>
                candidate.GetName().Name?.Equals(
                    assemblyName,
                    StringComparison.OrdinalIgnoreCase) == true);
            if (loaded != null)
            {
                var loadedPath = SafeAssemblyLocation(loaded);
                if (!loadedPath.Equals(adapterPath, StringComparison.OrdinalIgnoreCase))
                {
                    ModLog.Warn(
                        $"皮肤框架程序集 {assemblyName} 已由其它路径加载；" +
                        "本次会话保留原框架，不启用内置替代层。");
                    return;
                }

                _adapterAssembly = loaded;
            }
            else
            {
                _adapterAssembly = FrameworkAssemblyLoadContextPolicy.LoadFromAssemblyPath(
                    typeof(Entry).Assembly,
                    adapterPath);
            }

            var registry = _adapterAssembly.GetType(RegistryTypeName, throwOnError: true)!;
            _registryType = registry;
            _setActiveSkin = registry.GetMethod(
                "SetActiveSkin",
                BindingFlags.Static | BindingFlags.Public,
                binder: null,
                types: [typeof(ModelId), typeof(string)],
                modifiers: null);
            _skinDbSetup = registry.GetMethod(
                "SkinDbSetup",
                BindingFlags.Static | BindingFlags.Public,
                binder: null,
                types: Type.EmptyTypes,
                modifiers: null);
            if (_setActiveSkin == null || _skinDbSetup == null)
            {
                throw new MissingMethodException("皮肤框架兼容层缺少选择注册接口");
            }

            AvailableAssemblies.Add(assemblyName);
            ModLog.Info(
                $"已加载行为隔离的皮肤框架兼容层 {assemblyName}；" +
                "原管理器 UI、存档及全局资源补丁不会运行。");
        }
        catch (Exception exception)
        {
            _adapterAssembly = null;
            _setActiveSkin = null;
            _skinDbSetup = null;
            AvailableAssemblies.Clear();
            ModLog.Warn("加载内置皮肤框架兼容层失败：" + exception.GetBaseException().Message);
        }
    }

    public static bool IsBundledFrameworkHost(string? modId) =>
        modId != null && AvailableAssemblies.Contains(modId);

    public static bool IsKnownFrameworkHost(string? modId) =>
        modId != null && KnownFrameworkAssemblies.Contains(modId);

    public static void TryBindOriginalFramework()
    {
        if (AvailableAssemblies.Count != 0) return;
        try
        {
            var original = MegaCrit.Sts2.Core.Modding.ModManager.Mods.FirstOrDefault(mod =>
                mod.state == MegaCrit.Sts2.Core.Modding.ModLoadState.Loaded &&
                IsKnownFrameworkHost(mod.manifest?.id));
            if (original == null) return;
            var path = Path.GetFullPath(Path.Combine(original.path, BundledAdapterFileName));
            // Formal exposes the assembly on Mod; beta removed that field. Prefer it when
            // available (also tolerates symlinked paths), then use the exact loaded location.
            var assembly = original.GetType().GetField("assembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                               ?.GetValue(original) as Assembly ??
                           NativeFrameworkAssemblyLoader.Find(path) ??
                           AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(candidate =>
                SafeAssemblyLocation(candidate).Equals(path, StringComparison.OrdinalIgnoreCase));
            if (assembly == null) return;
            FrameworkRegistryCooperation.Bind(assembly);
            _adapterAssembly = assembly;
            _registryType = assembly.GetType(RegistryTypeName, true)!;
            _skinDbSetup = _registryType.GetMethod("SkinDbSetup");
        }
        catch (Exception exception)
        {
            ModLog.Warn("原皮肤管理器协作绑定失败，未加载同名后备 DLL：" + exception.GetBaseException().Message);
        }
    }

    public static void SynchronizeSelections(
        SkinCatalog catalog,
        IReadOnlyDictionary<string, string> selections)
    {
        if (_setActiveSkin == null && !FrameworkRegistryCooperation.IsActive)
        {
            return;
        }

        // ModelDb.AllCharacters dereferences the five built-in singleton entries and throws while
        // Essential initialization is still building the database. Contains(Type) is available in
        // both supported game versions and only checks the backing dictionary, so use it as the
        // readiness gate before touching those singleton getters.
        var modelDatabaseReady = ModelDb.Contains(typeof(Ironclad));
        if (modelDatabaseReady && _skinDbSetup != null)
        {
            var completed = ProviderRegistrations.RetryPending(
                isReady: true,
                RegisterProvider,
                (providerKey, exception) => ModLog.Warn(
                    $"补做框架皮肤注册 {providerKey} 失败，已保留后续重试资格：" +
                    exception.GetBaseException().Message));
            if (completed > 0)
            {
                ModLog.Info($"模型库就绪后已补做 {completed} 个框架皮肤提供者注册。");
            }
        }

        // Native rendering reads the current player scope. Persistence only receives local
        // committed choices; the session publication guard prevents a second selection request.
        if (FrameworkRegistryCooperation.IsActive)
        {
            FrameworkRegistryCooperation.SynchronizeLocalSelections(catalog);
            return;
        }

        var registeredCharacters = modelDatabaseReady
            ? ModelDb.AllCharacters
            : Enumerable.Empty<CharacterModel>();
        FrameworkSelectionSynchronizer.Synchronize(
            registeredCharacters,
            character => NormalizeToken(character.Id.Entry),
            groupId => catalog.TryGetSelectedFrameworkContract(
                groupId,
                selections.GetValueOrDefault(groupId),
                out var selected)
                ? selected.SkinId
                : null,
            (character, skinId) =>
                _setActiveSkin!.Invoke(null, [character.Id, skinId]));
    }

    public static void NotifyProviderActivated(Assembly providerAssembly)
    {
        if (_skinDbSetup == null)
        {
            return;
        }
        var cooperation = FrameworkRegistryCooperation.IsActive;
        if (cooperation && !FrameworkRegistryCooperation.HasRegistrationCallbacks(providerAssembly))
        {
            // The provider's [HarmonyPatch(typeof(SkinRegistry), "SkinDbSetup")] callback is not in
            // place yet, so the setup call below would register nothing and the skin would stay
            // invisible for this session. Retry after the activation has finished installing its
            // patches instead of dropping it.
            ScheduleLateRegistration(providerAssembly, cooperation);
            return;
        }

        var key = providerAssembly.FullName ?? providerAssembly.GetName().Name ?? string.Empty;
        try
        {
            var result = ProviderRegistrations.TryRegister(
                key,
                providerAssembly,
                ModelDb.Contains(typeof(Ironclad)),
                RegisterProvider);
            if (result != DeferredRegistrationResult.Deferred && FrameworkRegistryCooperation.IsActive && SkinService.Catalog is { } catalog)
                FrameworkRegistryCooperation.SynchronizeLocalSelections(catalog);
            if (result == DeferredRegistrationResult.Deferred)
            {
                ModLog.Info(
                    $"已延迟 {providerAssembly.GetName().Name} 的框架皮肤注册；" +
                    "游戏模型库就绪后将自动补做。");
            }
        }
        catch (Exception exception)
        {
            // Do not make a transient ModelDb timing failure tear down the provider's safe
            // animation/scene behavior. The queue deliberately retains this assembly and retries
            // before the next framework selection synchronization.
            ModLog.Warn(
                $"框架皮肤提供者 {providerAssembly.GetName().Name} 注册尚未完成，" +
                "已保留后续重试资格：" + exception.GetBaseException().Message);
        }

        ScheduleLateRegistration(providerAssembly, cooperation);
    }

    /// <summary>
    /// A framework skin only appears when its provider's postfix on <c>SkinRegistry.SkinDbSetup</c>
    /// runs. Managed providers install their patches during activation, and a provider activated
    /// from a different path (or one whose callback arrives a frame later) can miss that single
    /// setup call — the pack then looks "not recognized" in some sessions but fine in others.
    /// Re-running the (idempotent, de-duplicating) setup a moment later heals that race; the
    /// surviving log line reports what the framework actually holds, so the next report is
    /// diagnosable without a debugger.
    /// </summary>
    private static void ScheduleLateRegistration(Assembly providerAssembly, bool cooperation)
    {
        try
        {
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                return;
            }

            foreach (var delay in new[] { 0.5, 2.0 })
            {
                var at = delay;
                tree.CreateTimer(at).Timeout += () =>
                {
                    try
                    {
                        if (cooperation &&
                            !FrameworkRegistryCooperation.HasRegistrationCallbacks(providerAssembly))
                        {
                            return;
                        }

                        RegisterProvider(providerAssembly);
                        if (at >= 2.0)
                        {
                            LogFrameworkSkinInventory(providerAssembly);
                        }
                    }
                    catch (Exception exception)
                    {
                        ModLog.Warn(
                            $"补做框架皮肤注册 {providerAssembly.GetName().Name} 失败：" +
                            exception.GetBaseException().Message);
                    }
                };
            }
        }
        catch
        {
            // Timers are a repair path only; a missing SceneTree must never block activation.
        }
    }

    /// <summary>Debug line: which skins the (bundled or original) framework currently holds.</summary>
    private static void LogFrameworkSkinInventory(Assembly providerAssembly)
    {
        try
        {
            if (_registryType == null)
            {
                return;
            }

            var skinsField = _registryType.GetField(
                "Skins",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (skinsField?.GetValue(null) is not System.Collections.IDictionary skins)
            {
                return;
            }

            var ids = new List<string>();
            foreach (System.Collections.DictionaryEntry entry in skins)
            {
                if (entry.Value is not System.Collections.IEnumerable list)
                {
                    continue;
                }

                foreach (var item in list)
                {
                    var skinId = item?.GetType().GetProperty("SkinId")?.GetValue(item) as string;
                    if (!string.IsNullOrWhiteSpace(skinId))
                    {
                        ids.Add(skinId);
                    }
                }
            }

            ModLog.Info(
                $"框架皮肤注册统计（{providerAssembly.GetName().Name}）：角色={skins.Count}，" +
                $"皮肤={ids.Count}：[{string.Join("、", ids.Take(12))}]");
        }
        catch
        {
            // Diagnostics only.
        }
    }

    private static string SafeAssemblyLocation(Assembly assembly)
    {
        try
        {
            return Path.GetFullPath(assembly.Location);
        }
        catch
        {
            return string.Empty;
        }
    }

    private static void RegisterProvider(Assembly provider)
    {
        if (FrameworkRegistryCooperation.IsActive) FrameworkRegistryCooperation.RegisterProvider(provider);
        else _skinDbSetup!.Invoke(null, null);
    }

    private static string NormalizeToken(string value) =>
        new(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
}

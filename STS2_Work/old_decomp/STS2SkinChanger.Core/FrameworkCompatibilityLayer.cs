using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class FrameworkCompatibilityLayer
{
	private const string BundledAdapterFileName = "thunninoiSkinManager.dll";

	private const string RegistryTypeName = "thunninoiSkinManager.thunninoiSkinManagerCode.SkinRegistry";

	private static readonly HashSet<string> AvailableAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> KnownFrameworkAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly DeferredRegistrationQueue<Assembly> ProviderRegistrations = new DeferredRegistrationQueue<Assembly>();

	private static Assembly? _adapterAssembly;

	private static MethodInfo? _setActiveSkin;

	private static MethodInfo? _skinDbSetup;

	public static IReadOnlyCollection<string> CompatibilityAssemblyNames => AvailableAssemblies;

	public static void Initialize()
	{
		if (_adapterAssembly != null)
		{
			return;
		}
		try
		{
			string directoryName = Path.GetDirectoryName(typeof(Entry).Assembly.Location);
			string text = ((directoryName == null) ? null : Path.GetFullPath(Path.Combine(directoryName, "thunninoiSkinManager.dll")));
			if (text == null || !File.Exists(text))
			{
				ModLog.Warn("未找到内置皮肤框架兼容层；声明依赖该框架的皮肤将交回游戏加载器。");
				return;
			}
			AssemblyName assemblyName = AssemblyName.GetAssemblyName(text);
			string assemblyName2 = assemblyName.Name;
			if (string.IsNullOrWhiteSpace(assemblyName2))
			{
				ModLog.Warn("内置皮肤框架兼容层没有有效的 CLR 程序集名称。");
				return;
			}
			KnownFrameworkAssemblies.Add(assemblyName2);
			TryBindOriginalFramework();
			if (FrameworkRegistryCooperation.IsActive)
			{
				return;
			}
			if (!ManagedSkinModLoader.CanInstallFrameworkCompatibilityAssembly(assemblyName2))
			{
				ModLog.Info("未启用内置皮肤框架兼容层 " + assemblyName2 + "：原管理器已启用，或当前没有需要后备接口的完整皮肤契约。");
				return;
			}
			Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly candidate) => candidate.GetName().Name?.Equals(assemblyName2, StringComparison.OrdinalIgnoreCase) ?? false);
			if (assembly != null)
			{
				if (!SafeAssemblyLocation(assembly).Equals(text, StringComparison.OrdinalIgnoreCase))
				{
					ModLog.Warn("皮肤框架程序集 " + assemblyName2 + " 已由其它路径加载；本次会话保留原框架，不启用内置替代层。");
					return;
				}
				_adapterAssembly = assembly;
			}
			else
			{
				_adapterAssembly = FrameworkAssemblyLoadContextPolicy.LoadFromAssemblyPath(typeof(Entry).Assembly, text);
			}
			Type type = _adapterAssembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.SkinRegistry", throwOnError: true);
			_setActiveSkin = type.GetMethod("SetActiveSkin", BindingFlags.Static | BindingFlags.Public, null, new Type[2]
			{
				typeof(ModelId),
				typeof(string)
			}, null);
			_skinDbSetup = type.GetMethod("SkinDbSetup", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null);
			if (_setActiveSkin == null || _skinDbSetup == null)
			{
				throw new MissingMethodException("皮肤框架兼容层缺少选择注册接口");
			}
			AvailableAssemblies.Add(assemblyName2);
			ModLog.Info("已加载行为隔离的皮肤框架兼容层 " + assemblyName2 + "；原管理器 UI、存档及全局资源补丁不会运行。");
		}
		catch (Exception ex)
		{
			_adapterAssembly = null;
			_setActiveSkin = null;
			_skinDbSetup = null;
			AvailableAssemblies.Clear();
			ModLog.Warn("加载内置皮肤框架兼容层失败：" + ex.GetBaseException().Message);
		}
	}

	public static bool IsBundledFrameworkHost(string? modId)
	{
		if (modId != null)
		{
			return AvailableAssemblies.Contains(modId);
		}
		return false;
	}

	public static bool IsKnownFrameworkHost(string? modId)
	{
		if (modId != null)
		{
			return KnownFrameworkAssemblies.Contains(modId);
		}
		return false;
	}

	public static void TryBindOriginalFramework()
	{
		if (AvailableAssemblies.Count != 0)
		{
			return;
		}
		try
		{
			Mod val = ((IEnumerable<Mod>)ModManager.Mods).FirstOrDefault((Func<Mod, bool>)((Mod mod) => (int)mod.state == 1 && IsKnownFrameworkHost(mod.manifest?.id)));
			if (val != null)
			{
				string path = Path.GetFullPath(Path.Combine(val.path, "thunninoiSkinManager.dll"));
				Assembly assembly = (((object)val).GetType().GetField("assembly", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(val) as Assembly) ?? NativeFrameworkAssemblyLoader.Find(path) ?? AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault((Assembly candidate) => SafeAssemblyLocation(candidate).Equals(path, StringComparison.OrdinalIgnoreCase));
				if (!(assembly == null))
				{
					FrameworkRegistryCooperation.Bind(assembly);
					_adapterAssembly = assembly;
					_skinDbSetup = assembly.GetType("thunninoiSkinManager.thunninoiSkinManagerCode.SkinRegistry", throwOnError: true).GetMethod("SkinDbSetup");
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("原皮肤管理器协作绑定失败，未加载同名后备 DLL：" + ex.GetBaseException().Message);
		}
	}

	public static void SynchronizeSelections(SkinCatalog catalog, IReadOnlyDictionary<string, string> selections)
	{
		if (_setActiveSkin == null && !FrameworkRegistryCooperation.IsActive)
		{
			return;
		}
		bool flag = ModelDb.Contains(typeof(Ironclad));
		if (flag && _skinDbSetup != null)
		{
			int num = ProviderRegistrations.RetryPending(isReady: true, RegisterProvider, delegate(string providerKey, Exception exception)
			{
				ModLog.Warn("补做框架皮肤注册 " + providerKey + " 失败，已保留后续重试资格：" + exception.GetBaseException().Message);
			});
			if (num > 0)
			{
				ModLog.Info($"模型库就绪后已补做 {num} 个框架皮肤提供者注册。");
			}
		}
		if (FrameworkRegistryCooperation.IsActive)
		{
			FrameworkRegistryCooperation.SynchronizeLocalSelections(catalog);
			return;
		}
		FrameworkSelectionSynchronizer.Synchronize(flag ? ModelDb.AllCharacters : Enumerable.Empty<CharacterModel>(), (CharacterModel character) => NormalizeToken(((AbstractModel)character).Id.Entry), (string groupId) => (!catalog.TryGetSelectedFrameworkContract(groupId, selections.GetValueOrDefault(groupId), out FrameworkCharacterSkinContract contract)) ? null : contract.SkinId, delegate(CharacterModel character, string skinId)
		{
			_setActiveSkin.Invoke(null, new object[2]
			{
				((AbstractModel)character).Id,
				skinId
			});
		});
	}

	public static void NotifyProviderActivated(Assembly providerAssembly)
	{
		if (_skinDbSetup == null || (FrameworkRegistryCooperation.IsActive && !FrameworkRegistryCooperation.HasRegistrationCallbacks(providerAssembly)))
		{
			return;
		}
		string key = providerAssembly.FullName ?? providerAssembly.GetName().Name ?? string.Empty;
		try
		{
			DeferredRegistrationResult num = ProviderRegistrations.TryRegister(key, providerAssembly, ModelDb.Contains(typeof(Ironclad)), RegisterProvider);
			if (num != 0 && FrameworkRegistryCooperation.IsActive)
			{
				SkinCatalog catalog = SkinService.Catalog;
				if (catalog != null)
				{
					FrameworkRegistryCooperation.SynchronizeLocalSelections(catalog);
				}
			}
			if (num == DeferredRegistrationResult.Deferred)
			{
				ModLog.Info("已延迟 " + providerAssembly.GetName().Name + " 的框架皮肤注册；游戏模型库就绪后将自动补做。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("框架皮肤提供者 " + providerAssembly.GetName().Name + " 注册尚未完成，已保留后续重试资格：" + ex.GetBaseException().Message);
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
		if (FrameworkRegistryCooperation.IsActive)
		{
			FrameworkRegistryCooperation.RegisterProvider(provider);
		}
		else
		{
			_skinDbSetup.Invoke(null, null);
		}
	}

	private static string NormalizeToken(string value)
	{
		return new string(value.Where(char.IsLetterOrDigit).Select(char.ToLowerInvariant).ToArray());
	}
}

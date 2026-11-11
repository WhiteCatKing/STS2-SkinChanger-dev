using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class ManagedCharacterAssetRegistrationGuard
{
	private const string RegistryTypeName = "STS2RitsuLib.Content.ModContentRegistry";

	private const string FrameworkTypeName = "STS2RitsuLib.RitsuLibFramework";

	private static readonly object Sync = new object();

	private static readonly Harmony Harmony = new Harmony("Gurio.SkinChanger.CharacterAssetRegistryGuard");

	private static readonly HashSet<MethodBase> PatchedRegistrationMethods = new HashSet<MethodBase>();

	private static readonly HashSet<string> ManagedProviderIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<string, HashSet<string>> CharacterGroupsByProvider = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

	private static readonly HashSet<string> ReportedBlockedRegistrations = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<Type, PropertyInfo?> ModIdProperties = new Dictionary<Type, PropertyInfo>();

	public static void Configure(SkinCatalog catalog)
	{
		lock (Sync)
		{
			ManagedProviderIds.Clear();
			CharacterGroupsByProvider.Clear();
			foreach (SkinGroup item in catalog.Groups.Where((SkinGroup group) => catalog.IsCharacterAppearanceGroup(group.Id)))
			{
				foreach (SkinOption item2 in item.Options.Where((SkinOption option) => !option.Id.Equals("__base__", StringComparison.OrdinalIgnoreCase)))
				{
					string effectiveProviderId = item2.EffectiveProviderId;
					ManagedProviderIds.Add(effectiveProviderId);
					if (!CharacterGroupsByProvider.TryGetValue(effectiveProviderId, out HashSet<string> value))
					{
						value = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
						CharacterGroupsByProvider[effectiveProviderId] = value;
					}
					value.Add(item.Id);
				}
			}
			EnsureFrameworkPatchesInstalled();
			ClearExistingRegistrations();
		}
	}

	public static void SuppressCurrentRegistrations()
	{
		lock (Sync)
		{
			EnsureFrameworkPatchesInstalled();
			ClearExistingRegistrations();
		}
	}

	private static void EnsureFrameworkPatchesInstalled()
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		FrameworkCardVisualGuard.EnsureInstalled();
		MethodInfo methodInfo = AccessTools.DeclaredMethod(typeof(ManagedCharacterAssetRegistrationGuard), "RegistrationPrefix", (Type[])null, (Type[])null);
		if (methodInfo == null)
		{
			return;
		}
		foreach (Type item in (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			select assembly.GetType("STS2RitsuLib.Content.ModContentRegistry", throwOnError: false) into type
			where type != null
			select type).Cast<Type>().Distinct())
		{
			string[] array = new string[2] { "RegisterCharacterAssetReplacement", "RegisterGlobalCharacterAssetReplacement" };
			foreach (string methodName in array)
			{
				foreach (MethodInfo item2 in from method in item.GetMethods(BindingFlags.Instance | BindingFlags.Public)
					where method.Name.Equals(methodName, StringComparison.Ordinal)
					select method)
				{
					if (!PatchedRegistrationMethods.Contains(item2))
					{
						try
						{
							Harmony.Patch((MethodBase)item2, new HarmonyMethod(methodInfo), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
							PatchedRegistrationMethods.Add(item2);
							ModLog.Info($"已接管 {item.Assembly.GetName().Name} 的 {methodName} 注册入口。");
						}
						catch (Exception ex)
						{
							ModLog.Warn($"接管 {item.Assembly.GetName().Name} 的 {methodName} 注册入口失败：" + ex.GetBaseException().Message);
						}
					}
				}
			}
		}
	}

	private static bool RegistrationPrefix(object __instance)
	{
		lock (Sync)
		{
			string registryModId = GetRegistryModId(__instance);
			if (!ManagedCharacterAssetRegistrationPolicy.ShouldSuppress(registryModId, ManagedProviderIds))
			{
				return true;
			}
			if (ReportedBlockedRegistrations.Add(registryModId))
			{
				ModLog.Info("已阻止 " + registryModId + " 向外部框架写入持久角色外观；战斗、商店、营火、头像和多人素材统一由当前皮肤选择路由。");
			}
		}
		return false;
	}

	private static string? GetRegistryModId(object registry)
	{
		Type type = registry.GetType();
		if (!ModIdProperties.TryGetValue(type, out PropertyInfo value))
		{
			value = type.GetProperty("ModId", BindingFlags.Instance | BindingFlags.Public);
			ModIdProperties[type] = value;
		}
		return value?.GetValue(registry) as string;
	}

	private static void ClearExistingRegistrations()
	{
		if (ManagedProviderIds.Count == 0)
		{
			return;
		}
		foreach (Type item in (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			select assembly.GetType("STS2RitsuLib.RitsuLibFramework", throwOnError: false) into type
			where type != null
			select type).Cast<Type>().Distinct())
		{
			MethodInfo method = item.GetMethod("GetContentRegistry", BindingFlags.Static | BindingFlags.Public, null, new Type[1] { typeof(string) }, null);
			if (method == null)
			{
				continue;
			}
			foreach (string managedProviderId in ManagedProviderIds)
			{
				try
				{
					object obj = method.Invoke(null, new object[1] { managedProviderId });
					if (obj == null)
					{
						continue;
					}
					bool flag = InvokeBoolean(obj, "ClearGlobalCharacterAssetReplacement");
					if (CharacterGroupsByProvider.TryGetValue(managedProviderId, out HashSet<string> value))
					{
						foreach (string item2 in value)
						{
							flag |= InvokeBoolean(obj, "RemoveCharacterAssetReplacement", item2);
						}
					}
					if (flag)
					{
						ModLog.Info("已清除 " + managedProviderId + " 遗留在外部框架中的角色外观注册，避免切换后继续污染其它皮肤与场景。");
					}
				}
				catch (Exception ex)
				{
					ModLog.Warn("清理 " + managedProviderId + " 的外部角色外观注册失败：" + ex.GetBaseException().Message);
				}
			}
		}
	}

	private static bool InvokeBoolean(object target, string methodName, params object[] arguments)
	{
		Type[] types = arguments.Select((object argument) => argument.GetType()).ToArray();
		object obj = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public, null, types, null)?.Invoke(target, arguments);
		if (obj is bool)
		{
			return (bool)obj;
		}
		return false;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using STS2SkinChanger.Core;

namespace STS2SkinChanger;

[ModInitializer("Initialize")]
public static class Entry
{
	public const string ModId = "Gurio.SkinChanger";

	public const string LegacyModId = "STS2SkinChanger";

	public const string InternalTestVersion = "1.0.19.11";

	public static bool IsSelfModId(string? modId)
	{
		if (modId != null)
		{
			if (!modId.Equals("Gurio.SkinChanger", StringComparison.OrdinalIgnoreCase))
			{
				return modId.Equals("STS2SkinChanger", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	public static void Initialize()
	{
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		Assembly assembly = typeof(Entry).Assembly;
		ModLog.Info($"内测版本 {"1.0.19.11"}；程序集={assembly.GetName().Name} {assembly.GetName().Version}；路径={assembly.Location}");
		ModPrivateDependencyResolver.Install(ModLog.Info);
		SkinChangerPaths.Initialize();
		FrameworkCompatibilityLayer.Initialize();
		ManagedSkinModLoader.Initialize();
		List<string> list = InstallPatchClasses(new Harmony("Gurio.SkinChanger"), GetLoadableTypes(assembly));
		ProviderManifestCompatibility.AcknowledgeParsedManifests(ModManager.Mods);
		ModLog.Info((list.Count == 0) ? "代码补丁已加载。等待游戏资源初始化。" : $"代码补丁已加载（{list.Count} 个类失败被跳过：{string.Join("、", list)}）。");
	}

	internal static List<string> InstallPatchClasses(Harmony harmony, IEnumerable<Type> types)
	{
		List<string> list = new List<string>();
		foreach (Type type in types)
		{
			if (!type.IsClass || !type.IsDefined(typeof(HarmonyPatch), inherit: false))
			{
				continue;
			}
			try
			{
				harmony.CreateClassProcessor(type).Patch();
			}
			catch (Exception ex)
			{
				try
				{
					harmony.CreateClassProcessor(type).Unpatch();
				}
				catch
				{
				}
				list.Add(type.Name);
				ModLog.Error("补丁类 " + type.Name + " 安装失败，已跳过该类：" + ex.GetBaseException().Message);
			}
		}
		return list;
	}

	private static Type[] GetLoadableTypes(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			ModLog.Warn($"程序集类型枚举部分失败，仅对 {ex.Types.Count((Type type) => type != null)} 个可加载类型继续：{ex.LoaderExceptions.Length} 个加载异常。");
			return ex.Types.OfType<Type>().ToArray();
		}
	}
}

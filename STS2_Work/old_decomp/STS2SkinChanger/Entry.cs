using System;
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

	public const string InternalTestVersion = "1.0.19.0";

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
		//IL_00ce: Expected O, but got Unknown
		Assembly assembly = typeof(Entry).Assembly;
		ModLog.Info($"内测版本 {"1.0.19.0"}；程序集={assembly.GetName().Name} {assembly.GetName().Version}；路径={assembly.Location}");
		ModPrivateDependencyResolver.Install(ModLog.Info);
		SkinChangerPaths.Initialize();
		FrameworkCompatibilityLayer.Initialize();
		ManagedSkinModLoader.Initialize();
		Harmony val = new Harmony("Gurio.SkinChanger");
		try
		{
			val.PatchAll();
			ProviderManifestCompatibility.AcknowledgeParsedManifests(ModManager.Mods);
			ModLog.Info("代码补丁已加载。等待游戏资源初始化。");
		}
		catch (Exception ex2)
		{
			try
			{
				val.UnpatchAll("Gurio.SkinChanger");
			}
			catch (Exception ex)
			{
				ModLog.Error("回滚已安装补丁失败：" + ex.GetBaseException().Message);
			}
			ModLog.Error("安装代码补丁失败，本 Mod 已停用本次会话的代码接管：" + ex2);
		}
	}
}

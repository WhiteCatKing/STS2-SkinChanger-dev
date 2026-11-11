using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class CustomSkeletonLoaderCompat
{
	private const string LoaderTypeName = "CustomSkeletonLoader.SkeletonLoader";

	private const string LoaderMethodName = "TryBuildMegaSkeletonDataResource";

	private static readonly HashSet<string> ReportedTokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static Harmony? _harmony;

	private static bool _installed;

	private static bool _watching;

	public static void Install(Harmony harmony)
	{
		_harmony = harmony;
		TryInstall();
	}

	private static void TryInstall()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Expected O, but got Unknown
		if (_installed)
		{
			return;
		}
		Type type2 = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
			select assembly.GetType("CustomSkeletonLoader.SkeletonLoader", throwOnError: false)).FirstOrDefault((Type type) => type != null);
		if (type2 == null)
		{
			if (!_watching)
			{
				_watching = true;
				AppDomain.CurrentDomain.AssemblyLoad += delegate
				{
					TryInstall();
				};
			}
			return;
		}
		_installed = true;
		try
		{
			MethodInfo method = type2.GetMethod("TryBuildMegaSkeletonDataResource", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[1] { typeof(string) }, null);
			if (method == null)
			{
				ModLog.Warn("Custom Skeleton Loader 兼容：未找到 CustomSkeletonLoader.SkeletonLoader.TryBuildMegaSkeletonDataResource，跳过兼容处理（该 Mod 的版本可能不同）。");
				return;
			}
			_harmony.Patch((MethodBase)method, new HarmonyMethod(typeof(CustomSkeletonLoaderCompat), "DeferToSelectedSkin", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			ModLog.Info("Custom Skeleton Loader 兼容已启用：当前选中的皮肤拥有该角色时，它的骨骼替换会让位给皮肤（原皮外观下仍照常生效）。");
		}
		catch (Exception ex)
		{
			_installed = false;
			ModLog.Warn("Custom Skeleton Loader 兼容安装失败：" + ex.GetBaseException().Message);
		}
	}

	private static bool DeferToSelectedSkin(string name, ref MegaSkeletonDataResource __result)
	{
		try
		{
			if (!SkinOwnsSkeleton(name))
			{
				return true;
			}
			__result = null;
			return false;
		}
		catch
		{
			return true;
		}
	}

	private static bool SkinOwnsSkeleton(string name)
	{
		string token = NormalizeSkeletonName(name);
		if (token.Length == 0)
		{
			return false;
		}
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			return false;
		}
		SkinGroup skinGroup = catalog.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(token, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null)
		{
			skinGroup = (from candidate in catalog.Groups.Where(delegate(SkinGroup candidate)
				{
					bool flag = token.StartsWith(candidate.Id, StringComparison.OrdinalIgnoreCase);
					if (flag)
					{
						bool flag2 = token.Length == candidate.Id.Length;
						if (!flag2)
						{
							char c = token[candidate.Id.Length];
							bool flag3 = ((c == '-' || c == '_') ? true : false);
							flag2 = flag3;
						}
						flag = flag2;
					}
					return flag;
				})
				orderby candidate.Id.Length descending
				select candidate).FirstOrDefault();
		}
		if (skinGroup == null)
		{
			return false;
		}
		if (HasNonBaseSelection(skinGroup.Id))
		{
			if (ReportedTokens.Add(skinGroup.Id))
			{
				ModLog.Info("Custom Skeleton Loader 让位给皮肤：" + skinGroup.Id + " 当前使用皮肤选择，不再用 ReplaceResources 里的骨骼替换该角色。");
			}
			return true;
		}
		return false;
	}

	private static bool HasNonBaseSelection(string groupId)
	{
		if (!IsBase(SkinService.Config.GetSelection(groupId)))
		{
			return true;
		}
		foreach (IReadOnlyDictionary<string, string> availableSelectionMap in MultiplayerSkinSync.GetAvailableSelectionMaps())
		{
			if (availableSelectionMap.TryGetValue(groupId, out var value) && !IsBase(value))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsBase(string? selection)
	{
		if (!string.IsNullOrWhiteSpace(selection))
		{
			return selection.Equals("__base__", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static string NormalizeSkeletonName(string? name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return string.Empty;
		}
		string text = name.Trim().Replace('\\', '/');
		int num = text.LastIndexOf('/');
		if (num >= 0)
		{
			string text2 = text;
			int num2 = num + 1;
			text = text2.Substring(num2, text2.Length - num2);
		}
		string[] array = new string[4] { "_skel_data", "_skeleton_data", "_skel", "_data" };
		foreach (string text3 in array)
		{
			if (text.EndsWith(text3, StringComparison.OrdinalIgnoreCase))
			{
				string text2 = text;
				int length = text3.Length;
				text = text2.Substring(0, text2.Length - length);
			}
		}
		array = new string[3] { "characterselect_", "charselect_", "character_select_" };
		foreach (string text4 in array)
		{
			if (text.StartsWith(text4, StringComparison.OrdinalIgnoreCase))
			{
				string text2 = text;
				int length = text4.Length;
				text = text2.Substring(length, text2.Length - length);
			}
		}
		return text.Trim('_', '-');
	}
}

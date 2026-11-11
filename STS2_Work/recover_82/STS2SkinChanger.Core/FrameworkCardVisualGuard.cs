using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;

namespace STS2SkinChanger.Core;

internal static class FrameworkCardVisualGuard
{
	private const string HelperName = "STS2RitsuLib.Scaffolding.Content.Patches.ModCharacterOwnedVisualOverrideHelper";

	private static readonly object Sync;

	private static readonly Harmony Harmony;

	private static readonly HashSet<MethodBase> Patched;

	private static readonly HashSet<(Assembly Assembly, string Error)> ReportedCardFailures;

	private static volatile bool _discoveryDirty;

	private static int _reportedLookupFailure;

	[ThreadStatic]
	private static CardModel? _baselineCard;

	[ThreadStatic]
	private static bool _checkingOwnership;

	static FrameworkCardVisualGuard()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		Sync = new object();
		Harmony = new Harmony("Gurio.SkinChanger.FrameworkCardVisualGuard");
		Patched = new HashSet<MethodBase>();
		ReportedCardFailures = new HashSet<(Assembly, string)>();
		_discoveryDirty = true;
		AppDomain.CurrentDomain.AssemblyLoad += delegate
		{
			_discoveryDirty = true;
		};
	}

	internal static void EnsureInstalled()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Expected O, but got Unknown
		if (!_discoveryDirty)
		{
			return;
		}
		lock (Sync)
		{
			if (!_discoveryDirty)
			{
				return;
			}
			_discoveryDirty = false;
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in assemblies)
			{
				Type type = TryGetType(assembly, "STS2RitsuLib.Scaffolding.Content.Patches.ModCharacterOwnedVisualOverrideHelper");
				if (type == null)
				{
					continue;
				}
				MethodInfo methodInfo = AccessTools.DeclaredMethod(type, "TryGetOwningCharacterOverrides", new Type[1] { typeof(CardModel) }, (Type[])null);
				if (!(methodInfo == null) && methodInfo.IsStatic && !(methodInfo.ReturnType.FullName != "STS2RitsuLib.Scaffolding.Characters.IModCharacterAssetOverrides") && !Patched.Contains(methodInfo))
				{
					try
					{
						Harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(FrameworkCardVisualGuard), "CharacterOverridePrefix", (Type[])null)
						{
							priority = 800
						}, (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
						Patched.Add(methodInfo);
						ModLog.Info("已协调 " + assembly.GetName().Name + " 的角色卡面覆盖：已接管卡牌遵循单卡选择，未接管卡牌保留框架外观。");
					}
					catch (Exception ex)
					{
						ModLog.Warn("安装可选框架卡面协调失败，不影响其它接管功能：" + ex.GetBaseException().Message);
					}
				}
			}
		}
	}

	internal static Type? TryGetType(Assembly assembly, string name)
	{
		try
		{
			return assembly.GetType(name, throwOnError: false);
		}
		catch (TypeLoadException ex)
		{
			ModLog.Warn($"解析 {assembly.GetName().Name} 的 {name} 失败，本轮跳过该程序集：{ex.Message}");
			return null;
		}
	}

	internal static string GetBaselinePortraitPath(CardModel card)
	{
		EnsureInstalled();
		CardModel baselineCard = _baselineCard;
		_baselineCard = card;
		try
		{
			return card.PortraitPath;
		}
		finally
		{
			_baselineCard = baselineCard;
		}
	}

	internal static bool TryGetBaselinePortraitPath(CardModel card, out string path)
	{
		try
		{
			path = GetBaselinePortraitPath(card);
			return !string.IsNullOrWhiteSpace(path);
		}
		catch (Exception exception)
		{
			ReportCardReadFailure(card, exception);
			path = string.Empty;
			return false;
		}
	}

	internal static void ReportCardReadFailure(CardModel card, Exception exception)
	{
		Type type = ((object)card).GetType();
		Exception baseException = exception.GetBaseException();
		lock (Sync)
		{
			if (!ReportedCardFailures.Add((type.Assembly, baseException.GetType().FullName + ":" + baseException.Message)))
			{
				return;
			}
		}
		ModLog.Warn($"跳过无法读取外观定义的卡牌 {type.FullName}（{type.Assembly.GetName().Name}）；" + "继续处理其它卡牌，不修改该卡的原始资源定义。此保护不代表其依赖兼容问题已解决：" + baseException);
	}

	private static bool CharacterOverridePrefix(CardModel __0, ref object? __result)
	{
		if (__0 == null)
		{
			return true;
		}
		if (_baselineCard == __0)
		{
			if (((object)__0).GetType().Assembly != typeof(CardModel).Assembly)
			{
				return true;
			}
		}
		else
		{
			if (_checkingOwnership)
			{
				return true;
			}
			_checkingOwnership = true;
			try
			{
				if (!SkinService.HasCardSkin(__0))
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				if (Interlocked.Exchange(ref _reportedLookupFailure, 1) == 0)
				{
					ModLog.Warn("卡牌外观归属尚不可用，暂时保留框架行为：" + ex.GetBaseException().Message);
				}
				return true;
			}
			finally
			{
				_checkingOwnership = false;
			}
		}
		__result = null;
		return false;
	}
}

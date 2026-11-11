using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui.LoadOrderWarning;

internal static class LoadOrderWarningController
{
	private static bool _pending;

	private static bool _shownThisSession;

	private static bool _autoReorderedThisSession;

	private static Mod[] _actionableProviders = Array.Empty<Mod>();

	public static void Schedule()
	{
		if (_shownThisSession || !SkinService.ShouldShowLoadOrderWarning(ManagedSkinModLoader.IsBeforeAllSkinProviders))
		{
			return;
		}
		IReadOnlyList<Mod> skinProvidersBeforeSelf = ManagedSkinModLoader.SkinProvidersBeforeSelf;
		IReadOnlyList<string> respectedLoadOrderProviders = SkinService.GetRespectedLoadOrderProviders();
		var (values, readOnlyList) = LoadOrderRespectPolicy.Resolve(skinProvidersBeforeSelf.Select((Mod mod) => mod.manifest?.id ?? string.Empty), respectedLoadOrderProviders);
		if (readOnlyList.Count == 0)
		{
			ModLog.Info("已尊重玩家手动排序，保持以下皮肤 Mod 位于本 Mod 之前独立加载：" + string.Join("、", values));
			return;
		}
		HashSet<string> actionableIds = readOnlyList.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		_actionableProviders = skinProvidersBeforeSelf.Where((Mod mod) => actionableIds.Contains(mod.manifest?.id ?? string.Empty)).ToArray();
		if (!_autoReorderedThisSession)
		{
			try
			{
				MoveSelfBeforeSkinProviders(readOnlyList.ToArray());
				SkinService.SetRespectedLoadOrderProviders(LoadOrderRespectPolicy.Remember(respectedLoadOrderProviders, readOnlyList));
				_autoReorderedThisSession = true;
				ModLog.Info("已自动将皮肤切换器-Skin Changer 移到未手动上移的皮肤 Mod 之前；需要重启游戏后才能开始接管。");
			}
			catch (Exception ex)
			{
				ModLog.Error("自动调整皮肤 Mod 加载顺序失败：" + ex.GetBaseException().Message);
				return;
			}
		}
		_pending = true;
		ModLog.Info("已准备显示“需要重启才能生效”的加载顺序提示。");
		TryShow();
	}

	public static void TryShow()
	{
		NModalContainer instance = NModalContainer.Instance;
		if (_pending && !_shownThisSession && instance != null && instance.OpenModal == null)
		{
			_pending = false;
			TaskHelper.RunSafely(ShowWarning(instance));
		}
	}

	private static async Task ShowWarning(NModalContainer container)
	{
		try
		{
			NGenericPopup val = NGenericPopup.Create();
			if (val == null)
			{
				ModLog.Warn("游戏未能创建加载顺序提示框。");
				_pending = true;
				return;
			}
			container.Add((Node)(object)val, true);
			Task<bool> task = val.WaitForConfirmation(new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.description"), new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.title"), new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), new LocString("main_menu_ui", "GENERIC_POPUP.confirm"));
			NVerticalPopup verticalPopup = ((Node)val).GetNodeOrNull<NVerticalPopup>(NodePath.op_Implicit("VerticalPopup"));
			if (verticalPopup == null)
			{
				ModLog.Error("加载顺序提示框缺少 VerticalPopup 节点，无法显示。");
				((Node)val).QueueFree();
				_pending = true;
				return;
			}
			verticalPopup.SetText(ModLocalization.Get(ModText.LoadOrderTitle), BuildLoadOrderMessage());
			verticalPopup.YesButton.SetText(ModLocalization.Get(ModText.PrioritizeAndRestart));
			verticalPopup.NoButton.SetText(ModLocalization.Get(ModText.Acknowledge));
			_shownThisSession = true;
			ModLog.Info("已显示需要重启才能生效的加载顺序提示框。");
			if (await task)
			{
				RestartGame(verticalPopup);
			}
			else
			{
				ModLog.Info("玩家选择“知道了”，已自动调整加载顺序；等待玩家手动重启后生效。");
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("显示加载顺序提示框失败：" + ex.GetBaseException().Message);
			if (!_shownThisSession)
			{
				_pending = true;
			}
		}
	}

	private static void RestartGame(NVerticalPopup popup)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			OS.SetRestartOnExit(true, (string[])null);
			ModLog.Info("玩家选择“重启”，正在重启游戏以应用新的 Mod 加载顺序。");
			((Node)popup).QueueFree();
			Callable val = Callable.From((Action)delegate
			{
				if (NGame.Instance != null)
				{
					NGame.Instance.Quit();
				}
				else
				{
					MainLoop mainLoop = Engine.GetMainLoop();
					MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
					if (obj != null)
					{
						((SceneTree)obj).Quit(0);
					}
				}
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception ex)
		{
			ModLog.Error("按玩家请求重启游戏失败：" + ex.GetBaseException().Message);
			popup.SetText(ModLocalization.Get(ModText.LoadOrderTitle), ModLocalization.Get(ModText.LoadOrderFailure) + "\n\n" + ex.GetBaseException().Message);
		}
	}

	private static string BuildLoadOrderMessage()
	{
		string[] value = (from name in (from mod in _actionableProviders
				select mod.manifest?.name ?? mod.manifest?.id ?? mod.path into name
				where !string.IsNullOrWhiteSpace(name)
				select name).Distinct<string>(StringComparer.CurrentCultureIgnoreCase)
			select "• " + EscapeBbCode(name)).ToArray();
		return ModLocalization.Get(ModText.LoadOrderMessage).Replace("{0}", string.Join("\n", value), StringComparison.Ordinal);
	}

	private static string EscapeBbCode(string value)
	{
		return value.Replace('[', '(').Replace(']', ')').Replace('\r', ' ')
			.Replace('\n', ' ');
	}

	private static void MoveSelfBeforeSkinProviders(string[] actionableProviderIds)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		Mod self = ((IEnumerable<Mod>)ModManager.Mods).FirstOrDefault((Func<Mod, bool>)((Mod mod) => Entry.IsSelfModId(mod.manifest?.id))) ?? throw new InvalidOperationException("当前 Mod 列表中找不到皮肤切换器-Skin Changer。");
		HashSet<string> actionableIds = actionableProviderIds.ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		Mod[] skinProviders = ManagedSkinModLoader.SkinProvidersInLoadOrder.Where((Mod provider) => actionableIds.Contains(provider.manifest?.id ?? string.Empty)).ToArray();
		if (skinProviders.Length == 0)
		{
			throw new InvalidOperationException("当前 Mod 列表中找不到需要调整顺序的皮肤 Mod。");
		}
		SaveManager instance = SaveManager.Instance;
		SettingsSave settingsSave = instance.SettingsSave;
		ModSettings modSettings = settingsSave.ModSettings;
		ModSettings val = (ModSettings)(((object)modSettings) ?? ((object)new ModSettings()));
		List<SettingsSaveMod> modList = val.ModList;
		List<SettingsSaveMod> list = modList.ToList();
		SettingsSaveMod? obj = ((IEnumerable<SettingsSaveMod>)list).FirstOrDefault((Func<SettingsSaveMod, bool>)((SettingsSaveMod entry) => Entry.IsSelfModId(entry.Id) && entry.Source == self.modSource));
		bool isEnabled = obj == null || obj.IsEnabled;
		list.RemoveAll((SettingsSaveMod entry) => Entry.IsSelfModId(entry.Id));
		int num = list.FindIndex((SettingsSaveMod entry) => skinProviders.Any((Mod provider) => string.Equals(entry.Id, provider.manifest?.id, StringComparison.OrdinalIgnoreCase) && entry.Source == provider.modSource));
		if (num < 0)
		{
			num = list.FindIndex((SettingsSaveMod entry) => skinProviders.Any((Mod provider) => string.Equals(entry.Id, provider.manifest?.id, StringComparison.OrdinalIgnoreCase)));
		}
		if (num < 0)
		{
			throw new InvalidOperationException("无法在已保存的 Mod 顺序中定位皮肤 Mod。");
		}
		list.Insert(num, new SettingsSaveMod(self)
		{
			IsEnabled = isEnabled
		});
		object? obj2 = AccessTools.Field(typeof(SaveManager), "_settingsSaveManager")?.GetValue(instance);
		SettingsSaveManager val2 = (SettingsSaveManager)(((obj2 is SettingsSaveManager) ? obj2 : null) ?? throw new InvalidOperationException("无法取得游戏设置保存接口，保留原 Mod 加载顺序。"));
		settingsSave.ModSettings = val;
		val.ModList = list;
		try
		{
			val2.SaveSettings();
		}
		catch
		{
			val.ModList = modList;
			settingsSave.ModSettings = modSettings;
			throw;
		}
	}
}

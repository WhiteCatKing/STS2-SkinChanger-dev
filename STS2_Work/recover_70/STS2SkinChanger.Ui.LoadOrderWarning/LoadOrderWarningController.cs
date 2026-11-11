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

	private static bool _choiceDialog;

	private static Mod[] _actionableProviders = Array.Empty<Mod>();

	private static Mod[] _choiceProviders = Array.Empty<Mod>();

	public static void Schedule()
	{
		if (_shownThisSession)
		{
			return;
		}
		string[] array = BuildOrderSnapshot();
		IReadOnlyList<string> lastKnownSkinProviderOrder = SkinService.GetLastKnownSkinProviderOrder();
		if (lastKnownSkinProviderOrder == null)
		{
			if (!SkinService.HasLoadOrderHistory())
			{
				ScheduleFirstInstall();
				return;
			}
			SkinService.SetLastKnownSkinProviderOrder(array);
			ModLog.Info("已记录当前 Mod 顺序快照（没有上一次快照，不弹出加载顺序询问）。");
			return;
		}
		if (lastKnownSkinProviderOrder.Any(Entry.IsSelfModId))
		{
			SkinService.SetLastKnownSkinProviderOrder(array);
			ModLog.Info("已迁移旧版加载顺序快照为“本 Mod 之前的提供者”格式，不弹出询问。");
			return;
		}
		bool flag = !array.SequenceEqual<string>(lastKnownSkinProviderOrder, StringComparer.OrdinalIgnoreCase);
		if (!flag)
		{
			return;
		}
		IReadOnlyList<Mod> skinProvidersBeforeSelf = ManagedSkinModLoader.SkinProvidersBeforeSelf;
		if (!ShouldPromptForOrderChange(flag, skinProvidersBeforeSelf.Count))
		{
			if (skinProvidersBeforeSelf.Count == 0)
			{
				SkinService.SetLastKnownSkinProviderOrder(array);
			}
			return;
		}
		if (!SkinService.ShouldShowLoadOrderWarning(ManagedSkinModLoader.IsBeforeAllSkinProviders))
		{
			SkinService.SetLastKnownSkinProviderOrder(array);
			return;
		}
		if (skinProvidersBeforeSelf.Count == 0)
		{
			SkinService.SetLastKnownSkinProviderOrder(array);
			return;
		}
		_choiceProviders = skinProvidersBeforeSelf.ToArray();
		_choiceDialog = true;
		_pending = true;
		ModLog.Info("检测到皮肤 Mod 排序变更且位于本 Mod 之前，准备询问是否移动本 Mod：" + string.Join("、", skinProvidersBeforeSelf.Select((Mod mod) => mod.manifest?.name ?? mod.manifest?.id ?? mod.path)));
		TryShow();
	}

	private static void ScheduleFirstInstall()
	{
		if (!SkinService.ShouldShowLoadOrderWarning(ManagedSkinModLoader.IsBeforeAllSkinProviders))
		{
			SkinService.SetLastKnownSkinProviderOrder(Array.Empty<string>());
			return;
		}
		IReadOnlyList<Mod> skinProvidersBeforeSelf = ManagedSkinModLoader.SkinProvidersBeforeSelf;
		if (skinProvidersBeforeSelf.Count == 0)
		{
			SkinService.SetLastKnownSkinProviderOrder(Array.Empty<string>());
			return;
		}
		_actionableProviders = skinProvidersBeforeSelf.ToArray();
		if (!_autoReorderedThisSession)
		{
			try
			{
				MoveSelfBeforeSkinProviders((from mod in skinProvidersBeforeSelf
					select mod.manifest?.id ?? string.Empty into id
					where !string.IsNullOrWhiteSpace(id)
					select id).ToArray());
				_autoReorderedThisSession = true;
				ModLog.Info("已自动将皮肤切换器-Skin Changer 移到未手动上移的皮肤 Mod 之前；需要重启游戏后才能开始接管。");
			}
			catch (Exception ex)
			{
				ModLog.Error("自动调整皮肤 Mod 加载顺序失败：" + ex.GetBaseException().Message);
				return;
			}
		}
		SkinService.SetLastKnownSkinProviderOrder(Array.Empty<string>());
		_choiceDialog = false;
		_pending = true;
		ModLog.Info("已准备显示首次安装的加载顺序提示。");
		TryShow();
	}

	private static string[] BuildOrderSnapshot()
	{
		return (from mod in ManagedSkinModLoader.SkinProvidersBeforeSelf
			select mod.manifest?.id ?? string.Empty into id
			where !string.IsNullOrWhiteSpace(id)
			select id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
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
		bool choiceDialog = _choiceDialog;
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
			verticalPopup.SetText(ModLocalization.Get(ModText.LoadOrderTitle), choiceDialog ? BuildChoiceMessage() : BuildLoadOrderMessage());
			verticalPopup.YesButton.SetText(ModLocalization.Get(ModText.PrioritizeAndRestart));
			verticalPopup.NoButton.SetText(ModLocalization.Get(choiceDialog ? ModText.LoadOrderDecline : ModText.Acknowledge));
			_choiceDialog = false;
			_shownThisSession = true;
			ModLog.Info(choiceDialog ? "已显示“是否将本 Mod 移到前面”的加载顺序询问框。" : "已显示需要重启才能生效的加载顺序提示框。");
			bool flag = await task;
			if (choiceDialog)
			{
				if (!flag)
				{
					SkinService.SetLastKnownSkinProviderOrder(BuildOrderSnapshot());
					ModLog.Info("玩家选择“先不了”，保留当前 Mod 顺序。");
					return;
				}
				try
				{
					MoveSelfBeforeSkinProviders((from mod in _choiceProviders
						select mod.manifest?.id ?? string.Empty into id
						where !string.IsNullOrWhiteSpace(id)
						select id).ToArray());
					SkinService.SetLastKnownSkinProviderOrder(Array.Empty<string>());
				}
				catch (Exception ex)
				{
					ModLog.Error("按玩家请求调整皮肤 Mod 加载顺序失败：" + ex.GetBaseException().Message);
					verticalPopup.SetText(ModLocalization.Get(ModText.LoadOrderTitle), ModLocalization.Get(ModText.LoadOrderFailure) + "\n\n" + ex.GetBaseException().Message);
					return;
				}
				RestartGame(verticalPopup);
			}
			else if (flag)
			{
				RestartGame(verticalPopup);
			}
			else
			{
				ModLog.Info("玩家选择“知道了”，已自动调整加载顺序；等待玩家手动重启后生效。");
			}
		}
		catch (Exception ex2)
		{
			ModLog.Error("显示加载顺序提示框失败：" + ex2.GetBaseException().Message);
			if (!_shownThisSession)
			{
				_pending = true;
			}
		}
	}

	private static string BuildChoiceMessage()
	{
		string[] value = (from name in (from mod in _choiceProviders
				select mod.manifest?.name ?? mod.manifest?.id ?? mod.path into name
				where !string.IsNullOrWhiteSpace(name)
				select name).Distinct<string>(StringComparer.CurrentCultureIgnoreCase)
			select "• " + EscapeBbCode(name)).ToArray();
		return ModLocalization.Get(ModText.LoadOrderChoiceMessage).Replace("{0}", string.Join("\n", value), StringComparison.Ordinal);
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

	internal static bool ShouldPromptForOrderChange(bool orderChanged, int providersAboveCount)
	{
		if (orderChanged)
		{
			return providersAboveCount > 0;
		}
		return false;
	}

	private static void MoveSelfBeforeSkinProviders(string[] actionableProviderIds)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
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
		int num = list.FindIndex((SettingsSaveMod entry) => string.Equals(entry.Id, "ModLaunchManager", StringComparison.OrdinalIgnoreCase));
		int num2 = ((num >= 0) ? (num + 1) : (-1));
		if (num2 < 0)
		{
			num2 = list.FindIndex((SettingsSaveMod entry) => skinProviders.Any((Mod provider) => string.Equals(entry.Id, provider.manifest?.id, StringComparison.OrdinalIgnoreCase) && entry.Source == provider.modSource));
		}
		if (num2 < 0)
		{
			num2 = list.FindIndex((SettingsSaveMod entry) => skinProviders.Any((Mod provider) => string.Equals(entry.Id, provider.manifest?.id, StringComparison.OrdinalIgnoreCase)));
		}
		if (num2 < 0)
		{
			throw new InvalidOperationException("无法在已保存的 Mod 顺序中定位皮肤 Mod。");
		}
		list.Insert(num2, new SettingsSaveMod(self)
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
			MlmLaunchPlanSync.SyncFromSettings("重启前重排");
		}
		catch
		{
			val.ModList = modList;
			settingsSave.ModSettings = modSettings;
			throw;
		}
	}
}

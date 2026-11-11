using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class MultiplayerSkinFailureDialog
{
	private static bool _showing;

	internal static void TryShow()
	{
		if (!_showing && OnlineSkinCache.TryPeekBlockingFailure(out OnlineSkinCacheFailure failure))
		{
			NModalContainer instance = NModalContainer.Instance;
			if (instance != null && instance.OpenModal == null)
			{
				_showing = true;
				TaskHelper.RunSafely(Show(instance, failure));
			}
		}
	}

	private static async Task Show(NModalContainer container, OnlineSkinCacheFailure failure)
	{
		try
		{
			NGenericPopup val = NGenericPopup.Create();
			if (val == null)
			{
				ModLog.Warn("游戏未能创建联机皮肤失败提示框，将在选角界面重试。");
				return;
			}
			container.Add((Node)(object)val, true);
			Task<bool> task = val.WaitForConfirmation(new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.description"), new LocString("main_menu_ui", "MOD_NOT_LOADED_POPUP.title"), new LocString("main_menu_ui", "GENERIC_POPUP.cancel"), new LocString("main_menu_ui", "GENERIC_POPUP.confirm"));
			NVerticalPopup nodeOrNull = ((Node)val).GetNodeOrNull<NVerticalPopup>(NodePath.op_Implicit("VerticalPopup"));
			if (nodeOrNull == null)
			{
				ModLog.Warn("联机皮肤失败提示框缺少 VerticalPopup 节点，将在选角界面重试。");
				((Node)val).QueueFree();
				return;
			}
			nodeOrNull.SetText(ModLocalization.GetOnlineSkinFailureTitle(), ModLocalization.FormatOnlineSkinFailure(EscapeBbCode(failure.ProviderId), EscapeBbCode(failure.Detail)));
			nodeOrNull.YesButton.SetText(ModLocalization.Get(ModText.Acknowledge));
			((CanvasItem)nodeOrNull.NoButton).Hide();
			await task;
			OnlineSkinCache.AcknowledgeBlockingFailure(failure.Key);
			ModLog.Info("玩家已确认联机皮肤 " + failure.ProviderId + " 加载失败，本机将使用原皮继续。");
		}
		catch (Exception ex)
		{
			ModLog.Error("显示联机皮肤失败提示框失败：" + ex.GetBaseException().Message);
		}
		finally
		{
			_showing = false;
		}
	}

	private static string EscapeBbCode(string value)
	{
		return value.Replace('[', '(').Replace(']', ')').Replace('\r', ' ')
			.Replace('\n', ' ');
	}
}

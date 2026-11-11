using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.Core.Runs;

namespace PengoTarot.ConfigFW;

public static class ConfigFloatingWindow
{
	private const string EntryBtnScenePath = "res://configFW/Scenes/configfloatingwindow_entry_btn.tscn";

	private const string EntryBtnRootName = "ConfigFWEntryBtnRoot";

	private const string EntryImagePath = "res://configFW/Scenes/images/entry.webp";

	private static NCharacterSelectScreen? _boundScreen;

	private static NConfigFloatingWindow? _openPanel;

	private static INetGameService? _registeredNetService;

	private static Control? _currentEntryRoot;

	private static Control? _runEntryRoot;

	private static Control? _settingsEntryRoot;

	private static bool _entryVisibleBeforeOpen;

	private static bool _showingIntro;

	public static void OnCharacterSelectOpened(NCharacterSelectScreen screen)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Invalid comparison between Unknown and I4
		_boundScreen = screen;
		if (!ConfigFloatingWindowConfig.ShowInSettingsOnly)
		{
			AddEntryButton(screen);
		}
		RefreshEntryButton(screen);
		((Node)screen).GetNodeOrNull<NConfigFloatingWindowEntryButton>(NodePath.op_Implicit("ConfigFWEntryBtnRoot/Button"))?.ApplySavedState();
		RegisterSync(screen);
		StartRunLobby lobby = screen.Lobby;
		if (lobby != null)
		{
			INetGameService netService = lobby.NetService;
			if ((int)((netService != null) ? new NetGameType?(netService.Type) : ((NetGameType?)null)).GetValueOrDefault() == 3)
			{
				return;
			}
		}
		ConfigFloatingWindowRunData.SnapshotFromDefaults();
		BroadcastConfig();
	}

	public static void OnCharacterSelectClosed(NCharacterSelectScreen screen)
	{
		if (_boundScreen == screen)
		{
			_boundScreen = null;
		}
		ClosePanel(broadcast: false);
		UnregisterSync();
	}

	public static void OnRunReady(NRun run)
	{
		if (!ConfigFloatingWindowConfig.ShowInSettingsOnly || ConfigFloatingWindowRunData.GetTarFlag(20))
		{
			AddRunEntryButton(run);
		}
	}

	private static void AddEntryButton(NCharacterSelectScreen screen)
	{
		if (!((Node)screen).HasNode(NodePath.op_Implicit("ConfigFWEntryBtnRoot")))
		{
			Control val = CreateEntry();
			GodotTreeExtensions.AddChildSafely((Node)(object)screen, (Node)(object)val);
			_currentEntryRoot = val;
		}
	}

	private static void AddRunEntryButton(NRun run)
	{
		NCapstoneContainer capstoneContainer = run.GlobalUi.CapstoneContainer;
		if (!((Node)capstoneContainer).HasNode(NodePath.op_Implicit("ConfigFWEntryBtnRoot")))
		{
			Control val = CreateEntry();
			GodotTreeExtensions.AddChildSafely((Node)(object)capstoneContainer, (Node)(object)val);
			if (((Node)val).GetParent() == capstoneContainer)
			{
				((Node)capstoneContainer).MoveChild((Node)(object)val, ((Node)capstoneContainer).GetChildCount(false) - 1);
			}
			_runEntryRoot = val;
			_currentEntryRoot = val;
			((Node)val).GetNodeOrNull<NConfigFloatingWindowEntryButton>(NodePath.op_Implicit("Button"))?.SetRunEntryVisible(!ConfigFloatingWindowConfig.ShowInSettingsOnly);
		}
	}

	public static void RefreshRunEntryDisplayMode()
	{
		if (!RunManager.Instance.IsInProgress)
		{
			return;
		}
		bool flag = !ConfigFloatingWindowConfig.ShowInSettingsOnly;
		if (_runEntryRoot == null || !GodotObject.IsInstanceValid((GodotObject)(object)_runEntryRoot))
		{
			if (!flag && !ConfigFloatingWindowRunData.GetTarFlag(20))
			{
				return;
			}
			NRun instance = NRun.Instance;
			if (instance != null)
			{
				AddRunEntryButton(instance);
			}
		}
		Control? runEntryRoot = _runEntryRoot;
		if (runEntryRoot != null)
		{
			((Node)runEntryRoot).GetNodeOrNull<NConfigFloatingWindowEntryButton>(NodePath.op_Implicit("Button"))?.SetRunEntryVisible(flag);
		}
	}

	private static Control CreateEntry()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		Node obj = GD.Load<PackedScene>("res://configFW/Scenes/configfloatingwindow_entry_btn.tscn").Instantiate((GenEditState)0);
		obj.Name = StringName.op_Implicit("ConfigFWEntryBtnRoot");
		return (Control)obj;
	}

	private static void RefreshEntryButton(NCharacterSelectScreen screen)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Invalid comparison between Unknown and I4
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("ConfigFWEntryBtnRoot"));
		if (nodeOrNull == null)
		{
			return;
		}
		if (ResourceLoader.Exists("res://configFW/Scenes/images/entry.webp", ""))
		{
			Texture2D val = GD.Load<Texture2D>("res://configFW/Scenes/images/entry.webp");
			TextureRect nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Button/Icon"));
			if (val != null && nodeOrNull2 != null)
			{
				nodeOrNull2.Texture = val;
			}
		}
		StartRunLobby lobby = screen.Lobby;
		INetGameService val2 = ((lobby != null) ? lobby.NetService : null);
		bool flag = val2 != null && (int)val2.Type == 3;
		((CanvasItem)nodeOrNull).Visible = !flag;
	}

	public static void OnEntryClicked()
	{
		bool editable = IsInCharacterSelect() || IsInSettingsAwayFromRun();
		OpenPanel(remote: false, editable);
	}

	private static bool IsInCharacterSelect()
	{
		NGame instance = NGame.Instance;
		NMainMenu val = ((instance != null) ? instance.MainMenu : null);
		if (val != null && ((NSubmenuStack)val.SubmenuStack).SubmenusOpen)
		{
			return ((NSubmenuStack)val.SubmenuStack).Peek() is NCharacterSelectScreen;
		}
		return false;
	}

	private static bool IsInSettingsAwayFromRun()
	{
		RunManager instance = RunManager.Instance;
		if (instance == null || instance.IsInProgress)
		{
			return false;
		}
		NGame instance2 = NGame.Instance;
		NMainMenu val = ((instance2 != null) ? instance2.MainMenu : null);
		if (val != null && ((NSubmenuStack)val.SubmenuStack).SubmenusOpen)
		{
			return ((NSubmenuStack)val.SubmenuStack).Peek() is NSettingsScreen;
		}
		return false;
	}

	public static void OpenPanel(bool remote = false, bool editable = false)
	{
		if (_openPanel != null)
		{
			return;
		}
		NConfigFloatingWindow nConfigFloatingWindow = NConfigFloatingWindow.Create();
		if (nConfigFloatingWindow != null)
		{
			AttachPanelAboveCursor((Control)(object)nConfigFloatingWindow);
			nConfigFloatingWindow.Open(remote, editable);
			_openPanel = nConfigFloatingWindow;
			HideEntryRoot();
			if (!remote && IsMultiplayer())
			{
				SendState(isOpen: true);
				BroadcastConfig();
			}
			if (!ConfigFloatingWindowConfig.ConfigIntroDismissed && !_showingIntro)
			{
				ShowIntroAsync();
			}
		}
	}

	private static async Task ShowIntroAsync()
	{
		_showingIntro = true;
		try
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (tree == null || NModalContainer.Instance == null)
			{
				return;
			}
			for (int page = 1; page <= 3; page++)
			{
				if (!(await ShowIntroPageAsync($"CONFIG_INTRO_PAGE{page}", (page < 3) ? "CONFIG_INTRO_NEXT" : "CONFIG_INTRO_ACK")))
				{
					return;
				}
				if (page < 3)
				{
					await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				}
			}
			ConfigFloatingWindowConfig.SetConfigIntroDismissed(value: true);
		}
		catch (Exception ex)
		{
			GD.PrintErr("[PengoTarot] 配置界面首次说明弹窗失败：" + ex.Message);
		}
		finally
		{
			_showingIntro = false;
		}
	}

	private static async Task<bool> ShowIntroPageAsync(string bodyKey, string yesButtonKey)
	{
		PackedScene obj = GD.Load<PackedScene>("res://scenes/ui/generic_popup.tscn");
		NGenericPopup val = ((obj != null) ? obj.Instantiate<NGenericPopup>((GenEditState)0) : null);
		NModalContainer instance = NModalContainer.Instance;
		if (val == null || instance == null)
		{
			return false;
		}
		instance.Add((Node)(object)val, true);
		return await val.WaitForConfirmation(new LocString("main_menu_ui", bodyKey), new LocString("main_menu_ui", "CONFIG_INTRO_TITLE"), (LocString)null, new LocString("main_menu_ui", yesButtonKey));
	}

	private static void HideEntryRoot()
	{
		if (_currentEntryRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_currentEntryRoot))
		{
			_entryVisibleBeforeOpen = ((CanvasItem)_currentEntryRoot).Visible;
			((CanvasItem)_currentEntryRoot).Visible = false;
		}
	}

	private static void RestoreEntryRoot()
	{
		if (_currentEntryRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_currentEntryRoot))
		{
			((CanvasItem)_currentEntryRoot).Visible = _entryVisibleBeforeOpen;
		}
	}

	private static void AttachPanelAboveCursor(Control panel)
	{
		NGame instance = NGame.Instance;
		if (instance == null)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
			if (obj != null)
			{
				((Node)((SceneTree)obj).Root).AddChild((Node)(object)panel, false, (InternalMode)0);
			}
		}
		else
		{
			((Node)instance).AddChild((Node)(object)panel, false, (InternalMode)0);
			NRemoteMouseCursorContainer remoteCursorContainer = instance.RemoteCursorContainer;
			if (remoteCursorContainer != null && ((Node)remoteCursorContainer).GetParent() == instance)
			{
				((Node)instance).MoveChild((Node)(object)panel, ((Node)remoteCursorContainer).GetIndex(false));
			}
		}
	}

	public static void ClosePanel(bool broadcast = true)
	{
		if (_openPanel != null)
		{
			_openPanel.Close();
			_openPanel = null;
			RestoreEntryRoot();
			if (broadcast && IsMultiplayer())
			{
				SendState(isOpen: false);
			}
		}
	}

	private static bool IsMultiplayer()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		NCharacterSelectScreen? boundScreen = _boundScreen;
		if (boundScreen == null)
		{
			return false;
		}
		StartRunLobby lobby = boundScreen.Lobby;
		return ((lobby != null) ? new bool?(NetGameTypeExtensions.IsMultiplayer(lobby.NetService.Type)) : ((bool?)null)) == true;
	}

	public static void OnSettingsScreenOpened(Control settingsScreen)
	{
		if (RunManager.Instance.IsInProgress && _runEntryRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_runEntryRoot))
		{
			_currentEntryRoot = _runEntryRoot;
			return;
		}
		if (((Node)settingsScreen).HasNode(NodePath.op_Implicit("ConfigFWEntryBtnRoot")))
		{
			_settingsEntryRoot = ((Node)settingsScreen).GetNodeOrNull<Control>(NodePath.op_Implicit("ConfigFWEntryBtnRoot"));
			_currentEntryRoot = _settingsEntryRoot;
			return;
		}
		Control val = CreateEntry();
		GodotTreeExtensions.AddChildSafely((Node)(object)settingsScreen, (Node)(object)val);
		_settingsEntryRoot = val;
		_currentEntryRoot = val;
	}

	public static void OnSettingsScreenClosed()
	{
		if (_settingsEntryRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsEntryRoot))
		{
			((Node)_settingsEntryRoot).QueueFree();
		}
		_settingsEntryRoot = null;
		_currentEntryRoot = ((_runEntryRoot != null && GodotObject.IsInstanceValid((GodotObject)(object)_runEntryRoot)) ? _runEntryRoot : null);
	}

	private static void RegisterSync(NCharacterSelectScreen screen)
	{
		StartRunLobby lobby = screen.Lobby;
		RegisterSync((lobby != null) ? lobby.NetService : null);
	}

	private static void RegisterSync(INetGameService? net)
	{
		if (net != null && _registeredNetService != net)
		{
			INetGameService? registeredNetService = _registeredNetService;
			if (registeredNetService != null)
			{
				registeredNetService.UnregisterMessageHandler<ConfigFloatingWindowStateMessage>((MessageHandlerDelegate<ConfigFloatingWindowStateMessage>)OnStateMessage);
			}
			INetGameService? registeredNetService2 = _registeredNetService;
			if (registeredNetService2 != null)
			{
				registeredNetService2.UnregisterMessageHandler<ConfigFloatingWindowDataMessage>((MessageHandlerDelegate<ConfigFloatingWindowDataMessage>)OnDataMessage);
			}
			net.RegisterMessageHandler<ConfigFloatingWindowStateMessage>((MessageHandlerDelegate<ConfigFloatingWindowStateMessage>)OnStateMessage);
			net.RegisterMessageHandler<ConfigFloatingWindowDataMessage>((MessageHandlerDelegate<ConfigFloatingWindowDataMessage>)OnDataMessage);
			_registeredNetService = net;
		}
	}

	public static void RegisterSyncForLoadLobby(INetGameService net)
	{
		RegisterSync(net);
	}

	public static void OnLoadLobbyClosed(INetGameService net)
	{
		if (_registeredNetService == net)
		{
			UnregisterSync();
		}
	}

	private static void UnregisterSync()
	{
		INetGameService? registeredNetService = _registeredNetService;
		if (registeredNetService != null)
		{
			registeredNetService.UnregisterMessageHandler<ConfigFloatingWindowStateMessage>((MessageHandlerDelegate<ConfigFloatingWindowStateMessage>)OnStateMessage);
		}
		INetGameService? registeredNetService2 = _registeredNetService;
		if (registeredNetService2 != null)
		{
			registeredNetService2.UnregisterMessageHandler<ConfigFloatingWindowDataMessage>((MessageHandlerDelegate<ConfigFloatingWindowDataMessage>)OnDataMessage);
		}
		_registeredNetService = null;
	}

	private static void SendState(bool isOpen)
	{
		INetGameService? registeredNetService = _registeredNetService;
		if (registeredNetService != null)
		{
			registeredNetService.SendMessage<ConfigFloatingWindowStateMessage>(new ConfigFloatingWindowStateMessage
			{
				version = 2,
				isOpen = isOpen
			});
		}
	}

	public static void BroadcastConfig()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		if (_registeredNetService != null && (int)_registeredNetService.Type == 2)
		{
			_registeredNetService.SendMessage<ConfigFloatingWindowDataMessage>(ConfigFloatingWindowDataMessage.FromRunData());
		}
	}

	private static void OnStateMessage(ConfigFloatingWindowStateMessage message, ulong senderId)
	{
		if (_registeredNetService != null && senderId != _registeredNetService.NetId)
		{
			if (message.isOpen)
			{
				OpenPanel(remote: true);
			}
			else
			{
				ClosePanel(broadcast: false);
			}
		}
	}

	private static void OnDataMessage(ConfigFloatingWindowDataMessage message, ulong senderId)
	{
		if (_registeredNetService != null && senderId != _registeredNetService.NetId)
		{
			message.ApplyToRunData();
			if (_openPanel != null)
			{
				_openPanel.RefreshFromRunData();
			}
		}
	}
}

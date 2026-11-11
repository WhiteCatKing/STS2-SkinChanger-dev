using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CharacterAppearancePauseMenu
{
	private const string ButtonName = "SkinChangerCharacterAppearance";

	private const string ScreenName = "SkinChangerCharacterAppearanceScreen";

	private const string PauseButtonScenePath = "res://scenes/pause_menu/pause_menu_button.tscn";

	private static readonly FieldInfo? RunStateField = AccessTools.Field(typeof(NPauseMenu), "_runState");

	internal static void Attach(NPauseMenu pauseMenu)
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Control node = ((Node)pauseMenu).GetNode<Control>(NodePath.op_Implicit("%ButtonContainer"));
			NPauseMenuButton nodeOrNull = ((Node)node).GetNodeOrNull<NPauseMenuButton>(NodePath.op_Implicit("SkinChangerCharacterAppearance"));
			PauseMenuAppearanceEntryDecision pauseMenuAppearanceEntryDecision = PauseMenuAppearanceEntryPolicy.Resolve(SkinService.ShouldShowInRunAppearanceEntry(), nodeOrNull != null);
			if (nodeOrNull != null)
			{
				((CanvasItem)nodeOrNull).Visible = pauseMenuAppearanceEntryDecision.ShowButton;
				RebuildFocusNeighbors(node);
			}
			else if (pauseMenuAppearanceEntryDecision.CreateButton)
			{
				PackedScene val = ResourceLoader.Load<PackedScene>("res://scenes/pause_menu/pause_menu_button.tscn", (string)null, (CacheMode)1) ?? throw new InvalidOperationException("无法加载暂停菜单按钮场景。");
				NPauseMenuButton button = val.Instantiate<NPauseMenuButton>((GenEditState)0);
				((Node)button).Name = StringName.op_Implicit("SkinChangerCharacterAppearance");
				((Node)node).AddChild((Node)(object)button, false, (InternalMode)0);
				NPauseMenuButton nodeOrNull2 = ((Node)node).GetNodeOrNull<NPauseMenuButton>(NodePath.op_Implicit("Compendium"));
				if (nodeOrNull2 != null)
				{
					((Node)node).MoveChild((Node)(object)button, ((Node)nodeOrNull2).GetIndex(false) + 1);
				}
				((GodotObject)button).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
				{
					Open(pauseMenu);
				}), 0u);
				ModLocalization.Bind((Node)(object)button, delegate
				{
					((Node)button).GetNode<MegaLabel>(NodePath.op_Implicit("Label")).SetTextAutoSize(ModLocalization.Get(ModText.CharacterAppearance));
				});
				RebuildFocusNeighbors(node);
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("添加游戏内角色外观入口失败：" + ex);
		}
	}

	private static void Open(NPauseMenu pauseMenu)
	{
		try
		{
			object? obj = RunStateField?.GetValue(pauseMenu);
			Player player = LocalContext.GetMe((IPlayerCollection)(((obj is IRunState) ? obj : null) ?? throw new InvalidOperationException("暂停菜单尚未绑定当前游戏。"))) ?? throw new InvalidOperationException("找不到当前玩家。");
			Node parent = ((Node)pauseMenu).GetParent();
			NSubmenuStack val = (NSubmenuStack)(object)((parent is NSubmenuStack) ? parent : null);
			if (val == null)
			{
				throw new InvalidOperationException("找不到游戏内子菜单容器。");
			}
			CharacterAppearanceScreen characterAppearanceScreen = ((Node)val).GetNodeOrNull<CharacterAppearanceScreen>(NodePath.op_Implicit("SkinChangerCharacterAppearanceScreen"));
			if (characterAppearanceScreen == null)
			{
				CharacterAppearanceScreen characterAppearanceScreen2 = new CharacterAppearanceScreen();
				((Node)characterAppearanceScreen2).Name = StringName.op_Implicit("SkinChangerCharacterAppearanceScreen");
				((CanvasItem)characterAppearanceScreen2).Visible = false;
				((Control)characterAppearanceScreen2).MouseFilter = (MouseFilterEnum)2;
				characterAppearanceScreen = characterAppearanceScreen2;
				((Node)val).AddChild((Node)(object)characterAppearanceScreen, false, (InternalMode)0);
			}
			characterAppearanceScreen.Initialize(player);
			val.Push((NSubmenu)(object)characterAppearanceScreen);
		}
		catch (Exception ex)
		{
			ModLog.Error("打开游戏内角色外观界面失败：" + ex);
		}
	}

	private static void RebuildFocusNeighbors(Control container)
	{
		NPauseMenuButton[] array = (from button in ((IEnumerable)((Node)container).GetChildren(false)).OfType<NPauseMenuButton>()
			where ((CanvasItem)button).Visible
			select button).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			NPauseMenuButton obj = array[i];
			((Control)obj).FocusNeighborLeft = ((Node)obj).GetPath();
			((Control)obj).FocusNeighborRight = ((Node)obj).GetPath();
			((Control)obj).FocusNeighborTop = ((Node)array[(i - 1 + array.Length) % array.Length]).GetPath();
			((Control)obj).FocusNeighborBottom = ((Node)array[(i + 1) % array.Length]).GetPath();
		}
	}
}

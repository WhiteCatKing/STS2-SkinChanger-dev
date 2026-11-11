using System;
using System.Collections;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class BackgroundAppearancePauseMenu
{
	private const string ButtonName = "SkinChangerBackgroundAppearance";

	private const string ScreenName = "SkinChangerBackgroundAppearanceScreen";

	private const string PauseButtonScenePath = "res://scenes/pause_menu/pause_menu_button.tscn";

	internal static void Attach(NPauseMenu pauseMenu)
	{
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Control node = ((Node)pauseMenu).GetNode<Control>(NodePath.op_Implicit("%ButtonContainer"));
			NPauseMenuButton nodeOrNull = ((Node)node).GetNodeOrNull<NPauseMenuButton>(NodePath.op_Implicit("SkinChangerBackgroundAppearance"));
			PauseMenuAppearanceEntryDecision pauseMenuAppearanceEntryDecision = PauseMenuAppearanceEntryPolicy.Resolve(SkinService.ShouldShowInRunBackgroundEntry(), nodeOrNull != null);
			if (nodeOrNull != null)
			{
				((CanvasItem)nodeOrNull).Visible = pauseMenuAppearanceEntryDecision.ShowButton;
				RebuildFocusNeighbors(node);
			}
			else if (pauseMenuAppearanceEntryDecision.CreateButton)
			{
				PackedScene val = ResourceLoader.Load<PackedScene>("res://scenes/pause_menu/pause_menu_button.tscn", (string)null, (CacheMode)1) ?? throw new InvalidOperationException("无法加载暂停菜单按钮场景。");
				NPauseMenuButton button = val.Instantiate<NPauseMenuButton>((GenEditState)0);
				((Node)button).Name = StringName.op_Implicit("SkinChangerBackgroundAppearance");
				((Node)node).AddChild((Node)(object)button, false, (InternalMode)0);
				NPauseMenuButton val2 = ((Node)node).GetNodeOrNull<NPauseMenuButton>(NodePath.op_Implicit("SkinChangerCharacterAppearance")) ?? ((Node)node).GetNodeOrNull<NPauseMenuButton>(NodePath.op_Implicit("Compendium"));
				((Node)node).MoveChild((Node)(object)button, ((val2 != null) ? ((Node)val2).GetIndex(false) : (((Node)node).GetChildCount(false) - 2)) + 1);
				((GodotObject)button).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
				{
					Open(pauseMenu);
				}), 0u);
				ModLocalization.Bind((Node)(object)button, delegate
				{
					((Node)button).GetNode<MegaLabel>(NodePath.op_Implicit("Label")).SetTextAutoSize(ModLocalization.Get(ModText.Background));
				});
				RebuildFocusNeighbors(node);
			}
		}
		catch (Exception ex)
		{
			ModLog.Error("添加游戏内背景入口失败：" + ex);
		}
	}

	private static void Open(NPauseMenu pauseMenu)
	{
		try
		{
			Node parent = ((Node)pauseMenu).GetParent();
			NSubmenuStack val = (NSubmenuStack)(object)((parent is NSubmenuStack) ? parent : null);
			if (val == null)
			{
				throw new InvalidOperationException("找不到游戏内子菜单容器。");
			}
			BackgroundAppearanceScreen backgroundAppearanceScreen = ((Node)val).GetNodeOrNull<BackgroundAppearanceScreen>(NodePath.op_Implicit("SkinChangerBackgroundAppearanceScreen"));
			if (backgroundAppearanceScreen == null)
			{
				BackgroundAppearanceScreen backgroundAppearanceScreen2 = new BackgroundAppearanceScreen();
				((Node)backgroundAppearanceScreen2).Name = StringName.op_Implicit("SkinChangerBackgroundAppearanceScreen");
				((CanvasItem)backgroundAppearanceScreen2).Visible = false;
				((Control)backgroundAppearanceScreen2).MouseFilter = (MouseFilterEnum)2;
				backgroundAppearanceScreen = backgroundAppearanceScreen2;
				((Node)val).AddChild((Node)(object)backgroundAppearanceScreen, false, (InternalMode)0);
			}
			val.Push((NSubmenu)(object)backgroundAppearanceScreen);
		}
		catch (Exception ex)
		{
			ModLog.Error("打开游戏内背景界面失败：" + ex);
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

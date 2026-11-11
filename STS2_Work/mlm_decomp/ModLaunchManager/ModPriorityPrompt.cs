using System;
using Godot;

namespace ModLaunchManager;

internal static class ModPriorityPrompt
{
	private static CanvasLayer? _layer;

	private static Action? _onConfirm;

	internal static bool IsOpen
	{
		get
		{
			if (_layer != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)_layer);
			}
			return false;
		}
	}

	internal static void Show(LaunchEntry entry, ModLoadPriority priority, Action onConfirm)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Expected O, but got Unknown
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Expected O, but got Unknown
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Expected O, but got Unknown
		if (IsOpen)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			_onConfirm = onConfirm;
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("ModPriorityPrompt"),
				Layer = 1200
			};
			Control val2 = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("ModPriorityPromptRoot"),
				MouseFilter = (MouseFilterEnum)0,
				Theme = UiKit.CreateTheme()
			});
			ColorRect control = new ColorRect
			{
				Color = UiKit.Alpha(UiKit.Canvas, 0.7f),
				MouseFilter = (MouseFilterEnum)0
			};
			((Node)val2).AddChild((Node)(object)UiKit.FullRect<ColorRect>(control), false, (InternalMode)0);
			CenterContainer val3 = UiKit.FullRect<CenterContainer>(new CenterContainer());
			PanelContainer val4 = UiKit.Card("DialogPanel");
			((Control)val4).CustomMinimumSize = new Vector2(480f, 0f);
			((Control)val4).SizeFlagsVertical = (SizeFlags)4;
			MarginContainer val5 = UiKit.Margin((Control)new VBoxContainer(), 20, 18);
			VBoxContainer val6 = (VBoxContainer)((Node)val5).GetChild(0, false);
			((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			Label val7 = UiKit.Label(MlmLocManager.Get("priority.prompt.title"), "SectionLabel");
			val7.AutowrapMode = (AutowrapMode)3;
			Label val8 = UiKit.Label(MlmLocManager.Get("priority.prompt.body", ("name", entry.DisplayName), ("tier", ModLoadPriorities.Label(priority))), "MutedLabel");
			val8.AutowrapMode = (AutowrapMode)3;
			Label val9 = UiKit.Label(MlmLocManager.Get("priority.prompt.hint"), "DimLabel");
			val9.AutowrapMode = (AutowrapMode)3;
			((Node)val6).AddChild((Node)(object)val7, false, (InternalMode)0);
			((Node)val6).AddChild((Node)(object)val8, false, (InternalMode)0);
			((Node)val6).AddChild((Node)(object)val9, false, (InternalMode)0);
			HBoxContainer val10 = new HBoxContainer();
			((Control)val10).AddThemeConstantOverride(StringName.op_Implicit("separation"), 9);
			Button cancel = UiKit.Button(MlmLocManager.Get("priority.prompt.cancel"), "SecondaryButton", 140f, 34f);
			((BaseButton)cancel).Pressed += Close;
			Button val11 = UiKit.Button(MlmLocManager.Get("priority.prompt.confirm"), "PrimaryButton", 160f, 34f);
			((BaseButton)val11).Pressed += Confirm;
			((Node)val10).AddChild((Node)new Control
			{
				SizeFlagsHorizontal = (SizeFlags)3
			}, false, (InternalMode)0);
			((Node)val10).AddChild((Node)(object)cancel, false, (InternalMode)0);
			((Node)val10).AddChild((Node)(object)val11, false, (InternalMode)0);
			((Node)val6).AddChild((Node)(object)val10, false, (InternalMode)0);
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)_layer).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(val2, Close, () => (Control?)(object)cancel));
			((Control)val11).GrabFocus();
		}
	}

	internal static void Close()
	{
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
		_layer = null;
		_onConfirm = null;
	}

	private static void Confirm()
	{
		Action? onConfirm = _onConfirm;
		Close();
		onConfirm?.Invoke();
	}
}

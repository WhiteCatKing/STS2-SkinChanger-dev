using System;
using Godot;

namespace ModLaunchManager;

internal static class WorkshopRemovalPrompt
{
	private static CanvasLayer? _layer;

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

	internal static void Show(string name, string action, string affected, Action<Action<bool>> remove)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Expected O, but got Unknown
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Expected O, but got Unknown
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected O, but got Unknown
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Expected O, but got Unknown
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Expected O, but got Unknown
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Expected O, but got Unknown
		if (IsOpen)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		_layer = new CanvasLayer
		{
			Name = StringName.op_Implicit("WorkshopRemovalPrompt"),
			Layer = 1300
		};
		Control val2 = UiKit.FullRect<Control>(new Control
		{
			Theme = UiKit.CreateTheme(),
			MouseFilter = (MouseFilterEnum)0
		});
		((Node)val2).AddChild((Node)(object)UiKit.FullRect<ColorRect>(new ColorRect
		{
			Color = UiKit.Alpha(UiKit.Canvas, 0.8f),
			MouseFilter = (MouseFilterEnum)0
		}), false, (InternalMode)0);
		CenterContainer val3 = UiKit.FullRect<CenterContainer>(new CenterContainer());
		PanelContainer val4 = UiKit.Card("DialogPanel");
		((Control)val4).CustomMinimumSize = new Vector2(520f, 0f);
		VBoxContainer val5 = new VBoxContainer();
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		((Node)val5).AddChild((Node)(object)UiKit.Label(MlmLocManager.Get("workshop.remove.title", ("action", action)), "SectionLabel"), false, (InternalMode)0);
		Label val6 = UiKit.Label(MlmLocManager.Get("workshop.remove.body", ("name", name), ("affected", affected)), "MutedLabel");
		((Control)val6).CustomMinimumSize = new Vector2(480f, 0f);
		val6.AutowrapMode = (AutowrapMode)3;
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		Label status = UiKit.Label(string.Empty, "WarningLabel");
		status.AutowrapMode = (AutowrapMode)3;
		((Node)val5).AddChild((Node)(object)status, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		Button cancel = UiKit.Button(MlmLocManager.Get("workshop.remove.back"), "SecondaryButton", 120f, 34f);
		Button confirm = UiKit.Button(action, "PrimaryButton", 150f, 34f);
		bool busy = false;
		((BaseButton)cancel).Pressed += Close;
		((BaseButton)confirm).Pressed += delegate
		{
			if (!busy)
			{
				busy = true;
				Button obj = cancel;
				bool disabled = (((BaseButton)confirm).Disabled = true);
				((BaseButton)obj).Disabled = disabled;
				status.Text = MlmLocManager.Get("workshop.remove.waiting");
				remove(delegate(bool success)
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)status))
					{
						if (success)
						{
							Close();
						}
						else
						{
							busy = false;
							Button obj2 = cancel;
							bool disabled2 = (((BaseButton)confirm).Disabled = false);
							((BaseButton)obj2).Disabled = disabled2;
							status.Text = MlmLocManager.Get("workshop.remove.failed");
							((Control)cancel).GrabFocus();
						}
					}
				});
			}
		};
		((Node)val7).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)cancel, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)confirm, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)UiKit.Margin((Control)(object)val5, 20, 18), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)_layer).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
		LauncherController.Active?.AddScope(new LauncherController.Scope(val2, delegate
		{
			if (!busy)
			{
				Close();
			}
		}, () => (Control?)(object)cancel));
		((Control)cancel).GrabFocus();
		((Control)cancel).GuiInput += new GuiInputEventHandler(Escape);
		((Control)confirm).GuiInput += new GuiInputEventHandler(Escape);
		val2.GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			if (!busy && input.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
			{
				Close();
			}
		};
		void Escape(InputEvent input)
		{
			if (!busy && input.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
			{
				Close();
			}
		}
	}

	internal static void Close()
	{
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
		_layer = null;
	}
}

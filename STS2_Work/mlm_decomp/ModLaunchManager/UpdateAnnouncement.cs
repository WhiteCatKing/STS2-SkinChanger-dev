using System;
using System.IO;
using Godot;

namespace ModLaunchManager;

internal static class UpdateAnnouncement
{
	internal const int CurrentRevision = 12;

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

	internal static Func<int, int>? NewFeatureSteps { get; set; }

	internal static Action<int>? ShowNewFeatures { get; set; }

	internal static string PreviewRequestPath => Path.Combine(Path.GetDirectoryName(typeof(UpdateAnnouncement).Assembly.Location) ?? AppContext.BaseDirectory, "preview_update_announcement.once");

	internal static void ShowIfNeeded()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Expected O, but got Unknown
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Expected O, but got Unknown
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Expected O, but got Unknown
		bool preview = File.Exists(PreviewRequestPath);
		if (IsOpen || (!preview && MlmLocManager.AcknowledgedUpdateRevision >= 12))
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		try
		{
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("UpdateAnnouncement"),
				Layer = 1200
			};
			Control val2 = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("UpdateAnnouncementRoot"),
				MouseFilter = (MouseFilterEnum)0,
				Theme = UiKit.CreateTheme()
			});
			((Node)val2).AddChild((Node)(object)UiKit.FullRect<ColorRect>(new ColorRect
			{
				Color = UiKit.Alpha(UiKit.Canvas, 0.82f),
				MouseFilter = (MouseFilterEnum)0
			}), false, (InternalMode)0);
			PanelContainer val3 = UiKit.Card();
			((Control)val3).AnchorLeft = 0.25f;
			((Control)val3).AnchorRight = 0.75f;
			((Control)val3).AnchorTop = 0.23f;
			((Control)val3).AnchorBottom = 0.77f;
			MarginContainer val4 = UiKit.Margin((Control)new VBoxContainer(), 24, 20);
			VBoxContainer val5 = (VBoxContainer)((Node)val4).GetChild(0, false);
			((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 16);
			Label val6 = UiKit.Label(MlmLocManager.Get(preview ? "updateAnnouncement.preview.title" : "updateAnnouncement.title"), "SectionLabel");
			val6.AutowrapMode = (AutowrapMode)3;
			((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
			ScrollContainer scroll = new ScrollContainer
			{
				HorizontalScrollMode = (ScrollMode)0,
				SizeFlagsVertical = (SizeFlags)3
			};
			Label val7 = UiKit.Label(MlmLocManager.Get(preview ? "updateAnnouncement.preview.body" : "updateAnnouncement.body"), "MutedLabel");
			val7.AutowrapMode = (AutowrapMode)3;
			((Control)val7).SizeFlagsHorizontal = (SizeFlags)3;
			((Node)scroll).AddChild((Node)(object)val7, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)scroll, false, (InternalMode)0);
			HBoxContainer val8 = new HBoxContainer();
			((Control)val8).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
			((Node)val8).AddChild((Node)new Control
			{
				SizeFlagsHorizontal = (SizeFlags)3
			}, false, (InternalMode)0);
			int seen = (preview ? Math.Max(0, 11) : MlmLocManager.AcknowledgedUpdateRevision);
			int num = NewFeatureSteps?.Invoke(seen) ?? 0;
			if (num > 0 && ShowNewFeatures != null)
			{
				Button val9 = UiKit.Button(MlmLocManager.Get("updateAnnouncement.tour", ("count", num)));
				((Node)val9).Name = StringName.op_Implicit("UpdateAnnouncementTour");
				((Control)val9).TooltipText = MlmLocManager.Get("updateAnnouncement.tour.tooltip");
				((BaseButton)val9).Pressed += delegate
				{
					Dismiss();
					ShowNewFeatures?.Invoke(seen);
				};
				((Node)val8).AddChild((Node)(object)val9, false, (InternalMode)0);
			}
			Button confirm = UiKit.Button(MlmLocManager.Get("updateAnnouncement.confirm"), "PrimaryButton", 100f);
			((Node)confirm).Name = StringName.op_Implicit("ConfirmUpdateAnnouncement");
			((BaseButton)confirm).Pressed += Dismiss;
			((Node)val8).AddChild((Node)(object)confirm, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)val8, false, (InternalMode)0);
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
			UiKit.FitDialogHeight((Control)(object)val3, scroll, 0.8f);
			((Node)_layer).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(val2, delegate
			{
			}, () => (Control?)(object)confirm)
			{
				ScrollTarget = () => scroll
			});
			((Control)confirm).GrabFocus();
			if (preview)
			{
				try
				{
					File.Delete(PreviewRequestPath);
					return;
				}
				catch (Exception ex)
				{
					GD.PushWarning("[ModLaunchManager] could not clear the announcement preview request: " + ex.Message);
					return;
				}
			}
		}
		catch (Exception value)
		{
			Close();
			GD.PushWarning($"{"[ModLaunchManager]"} update announcement unavailable: {value}");
		}
		void Dismiss()
		{
			if (preview)
			{
				Close();
			}
			else
			{
				Confirm();
			}
		}
	}

	private static void Confirm()
	{
		MlmLocManager.AcknowledgeUpdate(12, out string _);
		Close();
	}

	internal static void Close()
	{
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			_layer.Hide();
			((Node)_layer).QueueFree();
		}
		_layer = null;
	}
}

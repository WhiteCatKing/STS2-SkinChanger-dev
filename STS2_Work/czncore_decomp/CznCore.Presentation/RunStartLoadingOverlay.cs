using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace CznCore.Presentation;

internal sealed class RunStartLoadingOverlay
{
	private readonly CanvasLayer _layer;

	private readonly ProgressBar _bar;

	private readonly Label _percent;

	private readonly Label _status;

	private RunStartLoadingOverlay(CanvasLayer layer, ProgressBar bar, Label percent, Label status)
	{
		_layer = layer;
		_bar = bar;
		_percent = percent;
		_status = status;
	}

	public static RunStartLoadingOverlay? TryCreate(Node host, RunStartPreloadProfile profile)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Expected O, but got Unknown
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Expected O, but got Unknown
		SceneTree treeOrNull = NodeUtil.GetTreeOrNull(host);
		if (((treeOrNull != null) ? treeOrNull.Root : null) == null)
		{
			return null;
		}
		CanvasLayer val = new CanvasLayer
		{
			Name = StringName.op_Implicit(profile.RuntimeNodeName("RunStartLoadingOverlay")),
			Layer = 4096
		};
		ColorRect val2 = new ColorRect
		{
			Color = new Color(0.02f, 0.018f, 0.024f, 0.92f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Control)val2).SetAnchorsPreset((LayoutPreset)15, false);
		CenterContainer val3 = new CenterContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val3).SetAnchorsPreset((LayoutPreset)15, false);
		VBoxContainer val4 = new VBoxContainer
		{
			CustomMinimumSize = new Vector2(620f, 160f),
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		Label val5 = new Label
		{
			Text = profile.LoadingTitle,
			HorizontalAlignment = (HorizontalAlignment)1
		};
		((Control)val5).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 30);
		ProgressBar val6 = new ProgressBar
		{
			MinValue = 0.0,
			MaxValue = 100.0,
			Value = 0.0,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(620f, 18f)
		};
		Label val7 = new Label
		{
			Text = "0%",
			HorizontalAlignment = (HorizontalAlignment)1
		};
		((Control)val7).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 24);
		Label val8 = new Label
		{
			Text = string.Empty,
			HorizontalAlignment = (HorizontalAlignment)1
		};
		((Control)val8).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)treeOrNull.Root).AddChild((Node)(object)val, false, (InternalMode)0);
		return new RunStartLoadingOverlay(val, val6, val7, val8);
	}

	public void SetProgress(float progress, string status)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			int num = Math.Clamp((int)MathF.Round(Math.Clamp(progress, 0f, 1f) * 100f), 0, 100);
			((Range)_bar).Value = num;
			_percent.Text = num + "%";
			_status.Text = status;
		}
	}

	public void Close()
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
	}
}

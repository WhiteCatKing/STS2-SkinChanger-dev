using System;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.PauseMenu;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class PauseMenuAppearanceRightClick
{
	internal static void Attach(NPauseMenuButton button, Action onRightClick, ModText tooltip)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (((GodotObject)button).HasMeta(StringName.op_Implicit("skin_changer_right_click")))
		{
			return;
		}
		((GodotObject)button).SetMeta(StringName.op_Implicit("skin_changer_right_click"), Variant.op_Implicit(true));
		((GodotObject)button).Connect(SignalName.MousePressed, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent input)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && PauseMenuAppearanceClickPolicy.ShouldToggleVisibility((long)val.ButtonIndex == 2, val.Pressed))
			{
				try
				{
					onRightClick();
				}
				catch (Exception ex)
				{
					ModLog.Error("切换外观入口可见性失败：" + ex);
				}
				((Control)button).AcceptEvent();
			}
		}), 0u);
		ModLocalization.Bind((Node)(object)button, delegate
		{
			((Control)button).TooltipText = ModLocalization.Get(tooltip);
		});
	}
}

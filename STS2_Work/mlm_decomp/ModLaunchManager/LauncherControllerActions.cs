using Godot;

namespace ModLaunchManager;

internal static class LauncherControllerActions
{
	internal static InputEvent? Translate(InputEventAction action)
	{
		//IL_0392: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Expected O, but got Unknown
		//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06dd: Expected O, but got Unknown
		JoyButton? val;
		switch (((object)action.Action).ToString())
		{
		case "controller_face_button_south":
			val = (JoyButton)0;
			break;
		case "controller_face_button_east":
			val = (JoyButton)1;
			break;
		case "controller_face_button_west":
			val = (JoyButton)2;
			break;
		case "controller_face_button_north":
			val = (JoyButton)3;
			break;
		case "controller_left_bumper":
			val = (JoyButton)9;
			break;
		case "controller_right_bumper":
			val = (JoyButton)10;
			break;
		case "controller_start_button":
			val = (JoyButton)6;
			break;
		case "controller_select_button":
			val = (JoyButton)4;
			break;
		case "controller_l_stick_press":
		case "controller_joystick_press":
			val = (JoyButton)7;
			break;
		case "controller_d_pad_north":
		case "controller_d_pad_up":
			val = (JoyButton)11;
			break;
		case "controller_d_pad_south":
		case "controller_d_pad_down":
			val = (JoyButton)12;
			break;
		case "controller_d_pad_left":
		case "controller_d_pad_west":
			val = (JoyButton)13;
			break;
		case "controller_d_pad_right":
		case "controller_d_pad_east":
			val = (JoyButton)14;
			break;
		default:
			val = null;
			break;
		}
		JoyButton? val2 = val;
		if (val2.HasValue)
		{
			JoyButton valueOrDefault = val2.GetValueOrDefault();
			return (InputEvent?)new InputEventJoypadButton
			{
				ButtonIndex = valueOrDefault,
				Pressed = action.Pressed,
				Device = ((InputEvent)action).Device
			};
		}
		(JoyAxis, float)? tuple;
		switch (((object)action.Action).ToString())
		{
		case "controller_left_trigger":
			tuple = ((JoyAxis)4, 1f);
			break;
		case "controller_right_trigger":
			tuple = ((JoyAxis)5, 1f);
			break;
		case "controller_l_stick_up":
		case "controller_joystick_up":
			tuple = ((JoyAxis)1, -1f);
			break;
		case "controller_l_stick_down":
		case "controller_joystick_down":
			tuple = ((JoyAxis)1, 1f);
			break;
		case "controller_l_stick_left":
		case "controller_joystick_left":
			tuple = ((JoyAxis)0, -1f);
			break;
		case "controller_l_stick_right":
		case "controller_joystick_right":
			tuple = ((JoyAxis)0, 1f);
			break;
		case "controller_r_stick_up":
			tuple = ((JoyAxis)3, -1f);
			break;
		case "controller_r_stick_down":
			tuple = ((JoyAxis)3, 1f);
			break;
		case "controller_r_stick_left":
			tuple = ((JoyAxis)2, -1f);
			break;
		case "controller_r_stick_right":
			tuple = ((JoyAxis)2, 1f);
			break;
		default:
			tuple = null;
			break;
		}
		(JoyAxis, float)? tuple2 = tuple;
		if (tuple2.HasValue)
		{
			(JoyAxis, float) valueOrDefault2 = tuple2.GetValueOrDefault();
			return (InputEvent?)new InputEventJoypadMotion
			{
				Axis = valueOrDefault2.Item1,
				AxisValue = (action.Pressed ? valueOrDefault2.Item2 : 0f),
				Device = ((InputEvent)action).Device
			};
		}
		return null;
	}
}

using System;
using System.Collections.Generic;
using Godot;

namespace JoanofArcIroncladHpwear;

internal static class AppearanceHotkey
{
	private static Node? _receiver;

	private static Key _safetyKey;

	private static readonly HashSet<Key> Down = new HashSet<Key>();

	internal static void Start(SceneTree tree)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		if (_receiver == null || !GodotObject.IsInstanceValid((GodotObject)(object)_receiver))
		{
			GDScript val = new GDScript
			{
				SourceCode = "extends Node\nsignal key_event(event)\nfunc _input(event):\n key_event.emit(event)\n"
			};
			if ((int)((Script)val).Reload(false) != 0)
			{
				throw new InvalidOperationException("appearance input script failed");
			}
			Node val2 = new Node
			{
				Name = StringName.op_Implicit("JoanAppearanceInput"),
				ProcessMode = (ProcessModeEnum)3
			};
			((GodotObject)val2).SetScript(Variant.op_Implicit((GodotObject)(object)val));
			((GodotObject)val2).Connect(StringName.op_Implicit("key_event"), Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent e)
			{
				Handle(tree, e);
			}), 0u);
			((Node)tree.Root).AddChild(val2, false, (InternalMode)0);
			val2.SetProcessInput(true);
			tree.Root.FocusExited += delegate
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				Down.Clear();
				_safetyKey = (Key)0;
				WearSettingsRuntime.UpdateHotkey(down: false);
			};
			_receiver = val2;
		}
	}

	private static void Handle(SceneTree tree, InputEvent input)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		InputEventKey val = (InputEventKey)(object)((input is InputEventKey) ? input : null);
		if (val == null)
		{
			return;
		}
		Key val2 = (((int)val.PhysicalKeycode != 0) ? val.PhysicalKeycode : val.Keycode);
		if (!val.Pressed)
		{
			Down.Remove(val2);
			if (_safetyKey == val2)
			{
				_safetyKey = (Key)0;
				WearSettingsRuntime.UpdateHotkey(down: false);
			}
		}
		else
		{
			if (val.Echo || !Down.Add(val2))
			{
				return;
			}
			try
			{
				if (SkinSelectionGate.SceneAllowed() && Matches(WearSettingsRuntime.Current.SafetyHotkey, val))
				{
					_safetyKey = val2;
					WearSettingsRuntime.UpdateHotkey(down: true);
					((Viewport)tree.Root).SetInputAsHandled();
				}
				else if (WearSettingsRuntime.PersistAppearance != null && !Editing(((Viewport)tree.Root).GuiGetFocusOwner()) && Matches(WearSettingsRuntime.Current.AppearanceHotkey, val) && SkinSelectionGate.SceneAllowed() && Palette.HasVisibleTarget())
				{
					int obj = Math.Clamp(WearSettingsRuntime.Current.Appearance, 1, Palette.Names.Length) % Palette.Names.Length + 1;
					WearSettingsRuntime.PersistAppearance(obj);
					Palette.RefreshAppearance(tree);
					((Viewport)tree.Root).SetInputAsHandled();
				}
			}
			catch (Exception ex)
			{
				ModEntry.TryInfo("appearance hotkey: " + ex.GetBaseException().Message);
			}
		}
	}

	private static bool Editing(Control? focus)
	{
		for (Node val = (Node)(object)focus; val != null; val = val.GetParent())
		{
			if ((val is LineEdit || val is TextEdit) ? true : false)
			{
				return true;
			}
			string text = ((object)val).GetType().FullName ?? "";
			if (text.Contains("KeyBinding", StringComparison.Ordinal) || text.Contains("InputBinding", StringComparison.Ordinal))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool Matches(string? binding, InputEventKey input)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(binding))
		{
			return false;
		}
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		Key result = (Key)0;
		string[] array = binding.Split('+', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		foreach (string text in array)
		{
			switch (text.ToLowerInvariant())
			{
			case "ctrl":
			case "control":
				flag = true;
				continue;
			case "alt":
				flag2 = true;
				continue;
			case "shift":
				flag3 = true;
				continue;
			case "meta":
			case "command":
			case "cmd":
			case "win":
			case "super":
				flag4 = true;
				continue;
			}
			if ((int)result != 0)
			{
				return false;
			}
			if (text == "=")
			{
				result = (Key)61;
			}
			else if (!Enum.TryParse<Key>(text, ignoreCase: true, out result) || (int)result == 0)
			{
				return false;
			}
		}
		if ((int)result != 0 && (input.Keycode == result || input.PhysicalKeycode == result) && ((InputEventWithModifiers)input).CtrlPressed == flag && ((InputEventWithModifiers)input).AltPressed == flag2 && ((InputEventWithModifiers)input).ShiftPressed == flag3)
		{
			return ((InputEventWithModifiers)input).MetaPressed == flag4;
		}
		return false;
	}
}

using System;
using Godot;

namespace ModLaunchManager;

internal static class LauncherControllerGlyphs
{
	private const string OriginPrefix = "k_EInputActionOrigin_";

	internal static PadFamily FamilyFromJoyName(string? name)
	{
		string text = (name ?? string.Empty).ToLowerInvariant();
		if (text.Contains("steam deck", StringComparison.Ordinal))
		{
			return PadFamily.SteamDeck;
		}
		if (text.Contains("nintendo", StringComparison.Ordinal) || text.Contains("switch", StringComparison.Ordinal) || text.Contains("joy-con", StringComparison.Ordinal) || text.Contains("joycon", StringComparison.Ordinal))
		{
			return PadFamily.Nintendo;
		}
		if (text.Contains("playstation", StringComparison.Ordinal) || text.Contains("dualsense", StringComparison.Ordinal) || text.Contains("dualshock", StringComparison.Ordinal) || text.Contains("ps3", StringComparison.Ordinal) || text.Contains("ps4", StringComparison.Ordinal) || text.Contains("ps5", StringComparison.Ordinal) || text.Contains("sony", StringComparison.Ordinal))
		{
			return PadFamily.PlayStation;
		}
		return PadFamily.Xbox;
	}

	internal static PadFamily FamilyFromSteamType(string? typeName)
	{
		switch (typeName)
		{
		case "k_ESteamInputType_PS3Controller":
		case "k_ESteamInputType_PS4Controller":
		case "k_ESteamInputType_PS5Controller":
			return PadFamily.PlayStation;
		case "k_ESteamInputType_SwitchProController":
		case "k_ESteamInputType_SwitchJoyConPair":
		case "k_ESteamInputType_SwitchJoyConSingle":
			return PadFamily.Nintendo;
		case "k_ESteamInputType_SteamDeckController":
			return PadFamily.SteamDeck;
		default:
			return PadFamily.Xbox;
		}
	}

	internal static PadGlyph Positional(JoyButton button, PadFamily family)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Invalid comparison between Unknown and I8
		JoyButton val = button;
		if ((long)val <= 10L)
		{
			switch ((uint)val)
			{
			case 0u:
				switch (family)
				{
				case PadFamily.PlayStation:
					return new PadGlyph(string.Empty, PadSymbol.Cross, Round: true);
				case PadFamily.Nintendo:
					return new PadGlyph("B", PadSymbol.None, Round: true);
				}
				goto IL_013a;
			case 1u:
				switch (family)
				{
				case PadFamily.PlayStation:
					return new PadGlyph(string.Empty, PadSymbol.Circle, Round: true);
				case PadFamily.Nintendo:
					return new PadGlyph("A", PadSymbol.None, Round: true);
				}
				goto IL_013a;
			case 2u:
				switch (family)
				{
				case PadFamily.PlayStation:
					return new PadGlyph(string.Empty, PadSymbol.Square, Round: true);
				case PadFamily.Nintendo:
					return new PadGlyph("Y", PadSymbol.None, Round: true);
				}
				goto IL_013a;
			case 3u:
				switch (family)
				{
				case PadFamily.PlayStation:
					return new PadGlyph(string.Empty, PadSymbol.Triangle, Round: true);
				case PadFamily.Nintendo:
					return new PadGlyph("X", PadSymbol.None, Round: true);
				}
				goto IL_013a;
			case 6u:
				return family switch
				{
					PadFamily.Nintendo => new PadGlyph("+"), 
					PadFamily.PlayStation => new PadGlyph("OPTIONS"), 
					_ => new PadGlyph(string.Empty, PadSymbol.Menu), 
				};
			case 4u:
				return family switch
				{
					PadFamily.Nintendo => new PadGlyph("−"), 
					PadFamily.PlayStation => new PadGlyph("CREATE"), 
					_ => new PadGlyph("VIEW"), 
				};
			case 9u:
			{
				string text;
				switch (family)
				{
				case PadFamily.PlayStation:
				case PadFamily.SteamDeck:
					text = "L1";
					break;
				case PadFamily.Nintendo:
					text = "L";
					break;
				default:
					text = "LB";
					break;
				}
				return new PadGlyph(text);
			}
			case 10u:
				{
					string text;
					switch (family)
					{
					case PadFamily.PlayStation:
					case PadFamily.SteamDeck:
						text = "R1";
						break;
					case PadFamily.Nintendo:
						text = "R";
						break;
					default:
						text = "RB";
						break;
					}
					return new PadGlyph(text);
				}
				IL_013a:
				return new PadGlyph(((object)(JoyButton)(ref button)/*cast due to .constrained prefix*/).ToString(), PadSymbol.None, Round: true);
			}
		}
		return new PadGlyph(((object)(JoyButton)(ref button)/*cast due to .constrained prefix*/).ToString());
	}

	internal static PadGlyph Trigger(int side, PadFamily family)
	{
		string text;
		if (side < 0)
		{
			switch (family)
			{
			case PadFamily.PlayStation:
			case PadFamily.SteamDeck:
				text = "L2";
				break;
			case PadFamily.Nintendo:
				text = "ZL";
				break;
			default:
				text = "LT";
				break;
			}
		}
		else
		{
			switch (family)
			{
			case PadFamily.PlayStation:
			case PadFamily.SteamDeck:
				text = "R2";
				break;
			case PadFamily.Nintendo:
				text = "ZR";
				break;
			default:
				text = "RT";
				break;
			}
		}
		return new PadGlyph(text);
	}

	internal static PadGlyph? FromSteamOrigin(string? originName)
	{
		if (originName == null || !originName.StartsWith("k_EInputActionOrigin_", StringComparison.Ordinal))
		{
			return null;
		}
		string text = originName.Substring("k_EInputActionOrigin_".Length);
		int num = text.IndexOf('_');
		if (num <= 0 || num == text.Length - 1)
		{
			return null;
		}
		PadFamily? padFamily;
		switch (text.Substring(0, num))
		{
		case "XBox360":
		case "XBoxOne":
		case "SteamController":
			padFamily = PadFamily.Xbox;
			break;
		case "PS4":
		case "PS5":
			padFamily = PadFamily.PlayStation;
			break;
		case "Switch":
			padFamily = PadFamily.Nintendo;
			break;
		case "SteamDeck":
			padFamily = PadFamily.SteamDeck;
			break;
		default:
			padFamily = null;
			break;
		}
		PadFamily? padFamily2 = padFamily;
		if (padFamily2.HasValue)
		{
			PadFamily valueOrDefault = padFamily2.GetValueOrDefault();
			string text2 = text.Substring(num + 1);
			if (valueOrDefault == PadFamily.PlayStation)
			{
				switch (text2)
				{
				case "X":
					return new PadGlyph(string.Empty, PadSymbol.Cross, Round: true);
				case "Circle":
					return new PadGlyph(string.Empty, PadSymbol.Circle, Round: true);
				case "Square":
					return new PadGlyph(string.Empty, PadSymbol.Square, Round: true);
				case "Triangle":
					return new PadGlyph(string.Empty, PadSymbol.Triangle, Round: true);
				case "Option":
				case "Options":
					return new PadGlyph("OPTIONS");
				case "Create":
				case "Share":
					return new PadGlyph("CREATE");
				case null:
					goto IL_089d;
				}
			}
			else if (text2 == null)
			{
				goto IL_089d;
			}
			switch (text2.Length)
			{
			case 1:
				switch (text2[0])
				{
				case 'A':
				case 'B':
				case 'X':
				case 'Y':
					return new PadGlyph(text2, PadSymbol.None, Round: true);
				}
				break;
			case 10:
				switch (text2[5])
				{
				case 'u':
					if (!(text2 == "LeftBumper"))
					{
						break;
					}
					return Positional((JoyButton)9, valueOrDefault);
				case 'N':
					if (!(text2 == "DPad_North"))
					{
						break;
					}
					return new PadGlyph("↑");
				case 'S':
					if (!(text2 == "DPad_South"))
					{
						break;
					}
					return new PadGlyph("↓");
				}
				break;
			case 16:
			{
				char c = text2[0];
				if (c != 'L')
				{
					if (c != 'R' || !(text2 == "RightStick_Click"))
					{
						break;
					}
					return new PadGlyph((valueOrDefault == PadFamily.PlayStation) ? "R3" : "RS");
				}
				if (!(text2 == "LeftTrigger_Pull"))
				{
					break;
				}
				goto IL_076b;
			}
			case 17:
			{
				char c = text2[0];
				if (c != 'L')
				{
					if (c != 'R' || !(text2 == "RightTrigger_Pull"))
					{
						break;
					}
					goto IL_077e;
				}
				if (!(text2 == "LeftTrigger_Click"))
				{
					break;
				}
				goto IL_076b;
			}
			case 2:
				switch (text2[1])
				{
				case '1':
					if (!(text2 == "L1") && !(text2 == "R1"))
					{
						break;
					}
					goto IL_0791;
				case '2':
					if (!(text2 == "L2") && !(text2 == "R2"))
					{
						break;
					}
					goto IL_0791;
				case '3':
					if (!(text2 == "L3") && !(text2 == "R3"))
					{
						break;
					}
					goto IL_0791;
				case '4':
					if (!(text2 == "L4") && !(text2 == "R4"))
					{
						break;
					}
					goto IL_0791;
				case '5':
					{
						if (!(text2 == "L5") && !(text2 == "R5"))
						{
							break;
						}
						goto IL_0791;
					}
					IL_0791:
					return new PadGlyph(text2);
				}
				break;
			case 4:
			{
				char c = text2[0];
				if ((uint)c <= 77u)
				{
					if (c != 'B')
					{
						if (c != 'M' || !(text2 == "Menu"))
						{
							break;
						}
						goto IL_07d6;
					}
					if (!(text2 == "Back"))
					{
						break;
					}
				}
				else
				{
					if (c == 'P')
					{
						if (!(text2 == "Plus"))
						{
							break;
						}
						return new PadGlyph("+");
					}
					if (c != 'V' || !(text2 == "View"))
					{
						break;
					}
				}
				return new PadGlyph("VIEW");
			}
			case 5:
			{
				char c = text2[0];
				if (c != 'M')
				{
					if (c != 'S' || !(text2 == "Start"))
					{
						break;
					}
					goto IL_07d6;
				}
				if (!(text2 == "Minus"))
				{
					break;
				}
				return new PadGlyph("−");
			}
			case 9:
				switch (text2[5])
				{
				case 'W':
					if (!(text2 == "DPad_West"))
					{
						break;
					}
					return new PadGlyph("←");
				case 'E':
					if (!(text2 == "DPad_East"))
					{
						break;
					}
					return new PadGlyph("→");
				}
				break;
			case 11:
				if (!(text2 == "RightBumper"))
				{
					break;
				}
				return Positional((JoyButton)10, valueOrDefault);
			case 18:
				if (!(text2 == "RightTrigger_Click"))
				{
					break;
				}
				goto IL_077e;
			case 15:
				{
					if (!(text2 == "LeftStick_Click"))
					{
						break;
					}
					return new PadGlyph((valueOrDefault == PadFamily.PlayStation) ? "L3" : "LS");
				}
				IL_077e:
				return Trigger(1, valueOrDefault);
				IL_07d6:
				return new PadGlyph(string.Empty, PadSymbol.Menu);
				IL_076b:
				return Trigger(-1, valueOrDefault);
			}
			goto IL_089d;
		}
		return null;
		IL_089d:
		return null;
	}
}

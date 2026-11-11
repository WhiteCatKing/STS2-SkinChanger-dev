using System;
using System.Buffers.Binary;
using System.Linq;
using System.Security.Cryptography;

namespace STS2SkinChanger.Core;

internal static class ThemePresetCode
{
	public const string Prefix = "SCT1.";

	public const int CodeLength = 159;

	public const int MaxInputLength = 2048;

	private const int PayloadLength = 111;

	private const int PacketLength = 115;

	public static bool LooksLikeCode(string? text)
	{
		string text2 = (text ?? "").TrimStart();
		if (!text2.StartsWith("SCT", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		int i;
		for (i = 3; i < text2.Length; i++)
		{
			char c = text2[i];
			if (c < '0' || c > '9')
			{
				break;
			}
		}
		if (i > 3 && i < text2.Length)
		{
			return text2[i] == '.';
		}
		return false;
	}

	public static string Encode(ModThemeSettings settings)
	{
		ModThemeSettings modThemeSettings = settings.Normalize();
		byte[] data = new byte[115];
		int offset = 0;
		C(modThemeSettings.PanelColor);
		C(modThemeSettings.SelectionColor);
		C(modThemeSettings.SelectionHoverColor);
		C(modThemeSettings.ButtonColor);
		C(modThemeSettings.HoverColor);
		C(modThemeSettings.DropdownColor);
		C(modThemeSettings.DropdownHoverColor);
		C(modThemeSettings.DropdownSelectionColor);
		C(modThemeSettings.DropdownSelectionHoverColor);
		C(modThemeSettings.DropdownBorderColor);
		C(modThemeSettings.TextColor);
		C(modThemeSettings.AccentColor);
		C(modThemeSettings.BorderColor);
		C(modThemeSettings.TextShadowColor);
		F(modThemeSettings.PanelOpacity);
		F(modThemeSettings.PanelBlur);
		F(modThemeSettings.SelectionOpacity);
		F(modThemeSettings.SelectionBlur);
		F(modThemeSettings.SelectionHoverOpacity);
		F(modThemeSettings.SelectionHoverBlur);
		F(modThemeSettings.ButtonOpacity);
		F(modThemeSettings.ButtonBlur);
		F(modThemeSettings.DropdownOpacity);
		F(modThemeSettings.DropdownBlur);
		F(modThemeSettings.DropdownHoverOpacity);
		F(modThemeSettings.DropdownSelectionOpacity);
		F(modThemeSettings.DropdownSelectionHoverOpacity);
		F(modThemeSettings.FontScale);
		F(modThemeSettings.TextShadowOpacity);
		I(modThemeSettings.DropdownBorderWidth);
		I(modThemeSettings.DropdownCornerRadius);
		I(modThemeSettings.BorderWidth);
		I(modThemeSettings.CornerRadius);
		I(modThemeSettings.TextOutline);
		I(modThemeSettings.TextShadowOffsetX);
		I(modThemeSettings.TextShadowOffsetY);
		I(modThemeSettings.TextShadowSize);
		data[offset] = (modThemeSettings.TextShadowEnabled ? ((byte)1) : ((byte)0));
		SHA256.HashData(data.AsSpan(0, 111)).AsSpan(0, 4).CopyTo(data.AsSpan(111));
		return "SCT1." + Base64(data);
		void C(string color)
		{
			Convert.FromHexString(color.Substring(1, color.Length - 1)).CopyTo(data, offset);
			offset += 3;
		}
		void F(float value)
		{
			BinaryPrimitives.WriteSingleLittleEndian(data.AsSpan(offset), value);
			offset += 4;
		}
		void I(int value)
		{
			data[offset++] = (byte)(sbyte)value;
		}
	}

	public static ModThemeSettings Decode(string? text)
	{
		if (text == null || text.Length > 2048)
		{
			throw InvalidCode();
		}
		string text2 = (text ?? "").Trim();
		if (text2.Length != 159 || !text2.StartsWith("SCT1.", StringComparison.OrdinalIgnoreCase))
		{
			throw InvalidCode();
		}
		string text3 = text2;
		int length = "SCT1.".Length;
		string text4 = text3.Substring(length, text3.Length - length);
		if (text4.Any((char c) => !char.IsAsciiLetterOrDigit(c) && c != '-' && c != '_'))
		{
			throw InvalidCode();
		}
		byte[] data;
		int offset;
		try
		{
			data = Convert.FromBase64String(text4.Replace('-', '+').Replace('_', '/') + "==");
			string C()
			{
				string result = "#" + Convert.ToHexString(data.AsSpan(offset, 3));
				offset += 3;
				return result;
			}
			float F()
			{
				float result2 = BinaryPrimitives.ReadSingleLittleEndian(data.AsSpan(offset));
				offset += 4;
				return result2;
			}
			int I()
			{
				return (sbyte)data[offset++];
			}
		}
		catch (FormatException)
		{
			throw InvalidCode();
		}
		if (data.Length != 115 || Base64(data) != text4 || !SHA256.HashData(data.AsSpan(0, 111)).AsSpan(0, 4).SequenceEqual(data.AsSpan(111)) || data[110] > 1)
		{
			throw InvalidCode();
		}
		offset = 0;
		ModThemeSettings modThemeSettings = new ModThemeSettings
		{
			PanelColor = C(),
			SelectionColor = C(),
			SelectionHoverColor = C(),
			ButtonColor = C(),
			HoverColor = C(),
			DropdownColor = C(),
			DropdownHoverColor = C(),
			DropdownSelectionColor = C(),
			DropdownSelectionHoverColor = C(),
			DropdownBorderColor = C(),
			TextColor = C(),
			AccentColor = C(),
			BorderColor = C(),
			TextShadowColor = C(),
			PanelOpacity = F(),
			PanelBlur = F(),
			SelectionOpacity = F(),
			SelectionBlur = F(),
			SelectionHoverOpacity = F(),
			SelectionHoverBlur = F(),
			ButtonOpacity = F(),
			ButtonBlur = F(),
			DropdownOpacity = F(),
			DropdownBlur = F(),
			DropdownHoverOpacity = F(),
			DropdownSelectionOpacity = F(),
			DropdownSelectionHoverOpacity = F(),
			FontScale = F(),
			TextShadowOpacity = F(),
			DropdownBorderWidth = I(),
			DropdownCornerRadius = I(),
			BorderWidth = I(),
			CornerRadius = I(),
			TextOutline = I(),
			TextShadowOffsetX = I(),
			TextShadowOffsetY = I(),
			TextShadowSize = I(),
			TextShadowEnabled = (data[offset] == 1)
		};
		if (modThemeSettings.Normalize() != modThemeSettings)
		{
			throw InvalidCode();
		}
		return modThemeSettings;
	}

	private static string Base64(byte[] data)
	{
		return Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-')
			.Replace('/', '_');
	}

	private static FormatException InvalidCode()
	{
		return new FormatException("Invalid, damaged or unsupported theme preset code.");
	}
}

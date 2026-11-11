using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModLaunchManager;

internal static class MlmLocFormatter
{
	internal static HashSet<string> GetPlaceholderNames(string template)
	{
		HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
		Parse(template, delegate(string name, string? _)
		{
			names.Add(name);
		});
		return names;
	}

	internal static string Format(string template, IReadOnlyDictionary<string, object?> arguments, IFormatProvider formatProvider)
	{
		StringBuilder stringBuilder = new StringBuilder(template.Length + 16);
		int num = 0;
		int num2 = 0;
		while (num2 < template.Length)
		{
			char c = template[num2];
			if (c == '{' && num2 + 1 < template.Length && template[num2 + 1] == '{')
			{
				stringBuilder.Append(template, num, num2 - num).Append('{');
				num2 += 2;
				num = num2;
				continue;
			}
			if (c == '}' && num2 + 1 < template.Length && template[num2 + 1] == '}')
			{
				stringBuilder.Append(template, num, num2 - num).Append('}');
				num2 += 2;
				num = num2;
				continue;
			}
			switch (c)
			{
			case '}':
				throw new FormatException($"Unexpected closing brace at position {num2}.");
			default:
				num2++;
				break;
			case '{':
			{
				stringBuilder.Append(template, num, num2 - num);
				int num3 = template.IndexOf('}', num2 + 1);
				if (num3 < 0)
				{
					throw new FormatException($"Unclosed placeholder at position {num2}.");
				}
				int num4 = num2 + 1;
				var (text, format) = ParseToken(template.Substring(num4, num3 - num4), num2);
				if (!arguments.TryGetValue(text, out object value))
				{
					throw new FormatException("Value for placeholder '" + text + "' was not supplied.");
				}
				stringBuilder.Append(FormatValue(value, format, formatProvider));
				num2 = num3 + 1;
				num = num2;
				break;
			}
			}
		}
		stringBuilder.Append(template, num, template.Length - num);
		return stringBuilder.ToString();
	}

	private static void Parse(string template, Action<string, string?> onPlaceholder)
	{
		int num = 0;
		while (num < template.Length)
		{
			if (template[num] == '{')
			{
				if (num + 1 < template.Length && template[num + 1] == '{')
				{
					num += 2;
					continue;
				}
				int num2 = template.IndexOf('}', num + 1);
				if (num2 < 0)
				{
					throw new FormatException($"Unclosed placeholder at position {num}.");
				}
				int num3 = num + 1;
				var (arg, arg2) = ParseToken(template.Substring(num3, num2 - num3), num);
				onPlaceholder(arg, arg2);
				num = num2 + 1;
			}
			else if (template[num] == '}')
			{
				if (num + 1 >= template.Length || template[num + 1] != '}')
				{
					throw new FormatException($"Unexpected closing brace at position {num}.");
				}
				num += 2;
			}
			else
			{
				num++;
			}
		}
	}

	private static (string Name, string? Format) ParseToken(string token, int position)
	{
		if (token.Contains('{'))
		{
			throw new FormatException($"Nested placeholder at position {position}.");
		}
		int num = token.IndexOf(':');
		string text = ((num < 0) ? token : token.Substring(0, num));
		string item = ((num < 0) ? null : token.Substring(num + 1));
		if (text.Length == 0 || !IsNameStart(text[0]) || text.Skip(1).Any((char character) => !IsNamePart(character)))
		{
			throw new FormatException($"Invalid placeholder '{token}' at position {position}.");
		}
		return (Name: text, Format: item);
	}

	private static string FormatValue(object? value, string? format, IFormatProvider provider)
	{
		if (value == null)
		{
			return string.Empty;
		}
		if (value is IFormattable formattable)
		{
			return formattable.ToString(format, provider) ?? string.Empty;
		}
		return Convert.ToString(value, provider) ?? string.Empty;
	}

	private static bool IsNameStart(char character)
	{
		if (!char.IsAsciiLetter(character))
		{
			return character == '_';
		}
		return true;
	}

	private static bool IsNamePart(char character)
	{
		if (!IsNameStart(character))
		{
			return char.IsAsciiDigit(character);
		}
		return true;
	}
}

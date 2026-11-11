using System;

namespace STS2SkinChanger.Core;

internal static class GodotScriptPathRegistrationPolicy
{
	private static readonly string[] DuplicateMarkers = new string[4] { "same key", "already been added", "相同的键", "已添加" };

	public static bool IsIdempotentRepeat(Type? existing, Type? incoming)
	{
		if ((object)existing == null || (object)incoming == null)
		{
			return false;
		}
		if (existing == incoming)
		{
			return true;
		}
		try
		{
			return existing.Module.ModuleVersionId == incoming.Module.ModuleVersionId;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool IsHarmlessDuplicate(Exception? exception)
	{
		for (Exception ex = exception; ex != null; ex = ex.InnerException)
		{
			if (ex is ArgumentException)
			{
				string[] duplicateMarkers = DuplicateMarkers;
				foreach (string value in duplicateMarkers)
				{
					if (ex.Message.Contains(value, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static string? DuplicatePath(Exception? exception)
	{
		for (Exception ex = exception; ex != null; ex = ex.InnerException)
		{
			if (ex is ArgumentException)
			{
				string message = ex.Message;
				int num = message.IndexOf("res://", StringComparison.Ordinal);
				if (num >= 0)
				{
					int i;
					for (i = num; i < message.Length && IsPathCharacter(message[i]); i++)
					{
					}
					if (i > num)
					{
						int num2 = num;
						return message.Substring(num2, i - num2);
					}
				}
			}
		}
		return null;
	}

	private static bool IsPathCharacter(char value)
	{
		bool flag = char.IsLetterOrDigit(value);
		if (!flag)
		{
			bool flag2;
			switch (value)
			{
			case '-':
			case '.':
			case '/':
			case ':':
			case '_':
				flag2 = true;
				break;
			default:
				flag2 = false;
				break;
			}
			flag = flag2;
		}
		return flag;
	}
}

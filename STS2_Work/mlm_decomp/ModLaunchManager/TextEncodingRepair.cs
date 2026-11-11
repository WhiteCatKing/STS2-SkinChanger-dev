using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace ModLaunchManager;

internal static class TextEncodingRepair
{
	private sealed record LineRepair(string Text, bool Clean, bool Usable);

	private const int MaxCacheEntries = 4096;

	private static readonly ConcurrentDictionary<string, string> Cache = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

	private static readonly Lazy<Encoding?> Gbk = new Lazy<Encoding>(CreateGbk);

	internal static string Repair(string? text)
	{
		if (string.IsNullOrEmpty(text) || !HasNonAscii(text) || Gbk.Value == null)
		{
			return text ?? string.Empty;
		}
		if (Cache.TryGetValue(text, out string value))
		{
			return value;
		}
		string[] array = text.Split('\n');
		LineRepair[] array2 = array.Select(AnalyzeLine).ToArray();
		string text2 = text;
		if (array2.Any((LineRepair repair) => repair?.Clean ?? false))
		{
			for (int i = 0; i < array.Length; i++)
			{
				LineRepair lineRepair = array2[i];
				if ((object)lineRepair != null && lineRepair.Usable)
				{
					array[i] = lineRepair.Text;
				}
			}
			text2 = string.Join('\n', array).Replace("\\n", "\n", StringComparison.Ordinal);
		}
		if (Cache.Count >= 4096)
		{
			Cache.Clear();
		}
		Cache[text] = text2;
		return text2;
	}

	private static LineRepair? AnalyzeLine(string line)
	{
		if (!HasNonAscii(line))
		{
			return null;
		}
		byte[] bytes;
		try
		{
			bytes = Gbk.Value.GetBytes(line);
		}
		catch (EncoderFallbackException)
		{
			return null;
		}
		bytes = RestoreLeadingBullet(bytes);
		string @string = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false).GetString(bytes);
		int num = @string.Count((char ch) => ch == '\ufffd');
		int num2 = @string.Count((char ch) => ch > '\u007f' && ch != '\ufffd');
		bool flag = num2 >= 2 && @string.Any(IsCjk) && ((num2 < 8) ? (num == 0) : (num * 10 <= num2));
		bool usable = flag || num <= Math.Max(1, num2 / 2);
		return new LineRepair(@string.Replace("\ufffd?", string.Empty).Replace("\ufffd", string.Empty), flag, usable);
	}

	private static byte[] RestoreLeadingBullet(byte[] bytes)
	{
		int num = 0;
		while (true)
		{
			bool flag = num < bytes.Length;
			if (flag)
			{
				byte b = bytes[num];
				bool flag2 = ((b == 9 || b == 13 || b == 32) ? true : false);
				flag = flag2;
			}
			if (!flag)
			{
				break;
			}
			num++;
		}
		if (bytes.Length - num < 3 || bytes[num] != 226 || bytes[num + 1] != 128 || bytes[num + 2] != 63)
		{
			return bytes;
		}
		byte[] obj = ((num + 3 >= bytes.Length || bytes[num + 3] != 32) ? new byte[4] { 226, 128, 162, 32 } : new byte[3] { 226, 128, 162 });
		byte[] subArray = bytes[..num];
		byte[] array = obj;
		byte[] subArray2 = bytes[(num + 3)..];
		int num2 = 0;
		byte[] array2 = new byte[subArray.Length + array.Length + subArray2.Length];
		ReadOnlySpan<byte> readOnlySpan = new ReadOnlySpan<byte>(subArray);
		readOnlySpan.CopyTo(new Span<byte>(array2).Slice(num2, readOnlySpan.Length));
		num2 += readOnlySpan.Length;
		ReadOnlySpan<byte> readOnlySpan2 = new ReadOnlySpan<byte>(array);
		readOnlySpan2.CopyTo(new Span<byte>(array2).Slice(num2, readOnlySpan2.Length));
		num2 += readOnlySpan2.Length;
		ReadOnlySpan<byte> readOnlySpan3 = new ReadOnlySpan<byte>(subArray2);
		readOnlySpan3.CopyTo(new Span<byte>(array2).Slice(num2, readOnlySpan3.Length));
		num2 += readOnlySpan3.Length;
		return array2;
	}

	private static bool HasNonAscii(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] > '\u007f')
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsCjk(char ch)
	{
		if (ch >= '一')
		{
			if (ch <= '鿿')
			{
				goto IL_0022;
			}
		}
		else if (ch >= '㐀' && ch <= '䶿')
		{
			goto IL_0022;
		}
		return false;
		IL_0022:
		return true;
	}

	private static Encoding? CreateGbk()
	{
		try
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding(936, EncoderFallback.ExceptionFallback, DecoderFallback.ReplacementFallback);
		}
		catch
		{
			return null;
		}
	}
}

using System.Buffers;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal static class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities
{
	internal static readonly TimeSpan s_defaultTimeout = ((AppContext.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") is TimeSpan timeSpan) ? timeSpan : Regex.InfiniteMatchTimeout);

	internal static readonly bool s_hasTimeout = s_defaultTimeout != Regex.InfiniteMatchTimeout;

	internal static readonly SearchValues<char> s_asciiHexDigits = SearchValues.Create("0123456789ABCDEFabcdef");

	internal static readonly SearchValues<char> s_asciiLettersAndDigits = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

	internal static readonly SearchValues<char> s_asciiWordChars = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz");

	internal static readonly SearchValues<char> s_ascii_1260000840300500000002800000028 = SearchValues.Create("\0\t\n\r\"'()<>[]{}");

	internal static readonly SearchValues<char> s_ascii_40FF030000000000000000 = SearchValues.Create(".0123456789");

	internal static readonly SearchValues<char> s_ascii_88FF23FEFFFF07FEFFFF07 = SearchValues.Create("+/0123456789=ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");

	internal static readonly SearchValues<char> s_ascii_FFC1FFFFFEFFFFFFBFFFFFFFBFFFFFFF = SearchValues.Create("\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdeghijklmnopqrstuvwxyz{|}~\u007f");

	internal static readonly SearchValues<char> s_ascii_FFC1FFFFFEFFFFFFFFDFFBFFFFDFFBFF = SearchValues.Create("\0\u0001\u0002\u0003\u0004\u0005\u0006\a\b\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLNOPQSTUVWXYZ[\\]^_`abcdefghijklnopqstuvwxyz{|}~\u007f");

	internal static readonly SearchValues<string> s_indexOfAnyStrings_OrdinalIgnoreCase_2CDD1846E7A9AA13DC45A8F092D4D08BD3ACE049796DAFCEAAA7C5411B326494;

	internal static readonly SearchValues<string> s_indexOfAnyStrings_OrdinalIgnoreCase_8CFD3E39BB63A31595D8A2B2F2BF99B948EA97CAE7DB2FC39B80B682682AFFA0;

	internal static readonly SearchValues<string> s_indexOfAnyStrings_OrdinalIgnoreCase_B6C7509F82BA11CF45AB3D08E001EEFC7955C5906A8E3BE7E147EAAAB6533794;

	internal static readonly SearchValues<string> s_indexOfString_2681E66F39A8B7964FC735A9BE3145D098828ECB3CE4F4C67C757ECDFCEE6833;

	internal static readonly SearchValues<string> s_indexOfString_2A878DB9BE1A71978FC34CB4C512D638EBAC04F270B41C1AA014091612B61527;

	internal static readonly SearchValues<string> s_indexOfString_36F4D3030D82929DC7EADF216BBEA1305C8AB6324FF55E37A7C72BD2DE843F68;

	internal static readonly SearchValues<string> s_indexOfString_3E8F6A9335F5942092785D27DE72DB3384C8C0449CA628FE5B2897594A07FE58;

	internal static readonly SearchValues<string> s_indexOfString_67C631CDB34E2886C3AE30BB53722CECED1CF2AD6A5B8816E142CFC17449008B;

	internal static readonly SearchValues<string> s_indexOfString_84A21C6523C7ECB212BDCD85C4F678722342B52A2DF6FA35939E13BB09255E7A;

	internal static readonly SearchValues<string> s_indexOfString_8B87CF6241D53984E594C711D2A93E47010C4048A8AB0A6E3645C62418E90839;

	internal static readonly SearchValues<string> s_indexOfString_B139E9D7E48E6962C345FE35EDCD52AC8EC2E8654DAAF4613535DB30693C7602;

	internal static readonly SearchValues<string> s_indexOfString_DBCF5569720FA761AC36428669700C2B4C4C01E45E3C9F5B81E6E63E80414563;

	internal static readonly SearchValues<string> s_indexOfString_dest_files_OrdinalIgnoreCase;

	internal static readonly SearchValues<string> s_indexOfString_path_OrdinalIgnoreCase;

	internal static readonly SearchValues<string> s_indexOfString_uid_OrdinalIgnoreCase;

	internal static readonly SearchValues<char> s_nonAscii_9FA52D3BAECB644578472387D5284CC6F36F408FEB88A04BA674CE14F24D2386;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int IndexOfNonAsciiOrAny_7091B6DA5C2749D379F79DDE6B55ABE13767EC535D963FA5881B8A7B76916E73(this ReadOnlySpan<char> span)
	{
		int num = span.IndexOfAnyExcept(s_ascii_FFC1FFFFFEFFFFFFBFFFFFFFBFFFFFFF);
		if ((uint)num < (uint)span.Length)
		{
			if (char.IsAscii(span[num]))
			{
				return num;
			}
			do
			{
				char c;
				if (((c = span[num]) < '\u0080') ? ((byte)("㸀\0\u0001\0@\0@\0"[(int)c >> 4] & (1 << (c & 0xF))) != 0) : RegexRunner.CharInClass(c, "\0\u0004\u0001FGfgd"))
				{
					return num;
				}
				num++;
			}
			while ((uint)num < (uint)span.Length);
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int IndexOfNonAsciiOrAny_AE593EF9A4E479A6268F396B40A3642E402C53B13FD4CFDA109DDD36AE686A06(this ReadOnlySpan<char> span)
	{
		int num = span.IndexOfAnyExcept(s_ascii_FFC1FFFFFEFFFFFFFFDFFBFFFFDFFBFF);
		if ((uint)num < (uint)span.Length)
		{
			if (char.IsAscii(span[num]))
			{
				return num;
			}
			do
			{
				char c;
				if (((c = span[num]) < '\u0080') ? ((byte)("㸀\0\u0001\0\u2000\u0004\u2000\u0004"[(int)c >> 4] & (1 << (c & 0xF))) != 0) : RegexRunner.CharInClass(c, "\0\b\u0001MNRSmnrsd"))
				{
					return num;
				}
				num++;
			}
			while ((uint)num < (uint)span.Length);
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void StackPush(ref int[] stack, ref int pos, int arg0)
	{
		int[] array = stack;
		int num = pos;
		if ((uint)num < (uint)array.Length)
		{
			array[num] = arg0;
			pos++;
		}
		else
		{
			WithResize(ref stack, ref pos, arg0);
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		static void WithResize(ref int[] stack, ref int pos, int arg0)
		{
			Array.Resize(ref stack, pos * 2);
			StackPush(ref stack, ref pos, arg0);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void StackPush(ref int[] stack, ref int pos, int arg0, int arg1)
	{
		int[] array = stack;
		int num = pos;
		if ((uint)(num + 1) < (uint)array.Length)
		{
			array[num] = arg0;
			array[num + 1] = arg1;
			pos += 2;
		}
		else
		{
			WithResize(ref stack, ref pos, arg0, arg1);
		}
		[MethodImpl(MethodImplOptions.NoInlining)]
		static void WithResize(ref int[] stack, ref int pos, int arg0, int arg1)
		{
			Array.Resize(ref stack, (pos + 1) * 2);
			StackPush(ref stack, ref pos, arg0, arg1);
		}
	}

	static _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities()
	{
		_003C_003Ey__InlineArray10<string> buffer = default(_003C_003Ey__InlineArray10<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 0) = "ext_reso";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 1) = "script";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 2) = "shader";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 3) = "gdextens";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 4) = "type";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 5) = ".dll";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 6) = ".cs";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 7) = ".gd";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 8) = ".gdc";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray10<string>, string>(ref buffer, 9) = ".gdshader";
		s_indexOfAnyStrings_OrdinalIgnoreCase_2CDD1846E7A9AA13DC45A8F092D4D08BD3ACE049796DAFCEAAA7C5411B326494 = SearchValues.Create(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray10<string>, string>(in buffer, 10), StringComparison.OrdinalIgnoreCase);
		_003C_003Ey__InlineArray5<string> buffer2 = default(_003C_003Ey__InlineArray5<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 0) = "gdextens";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 1) = "extensio";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 2) = "type";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 3) = "shader/c";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 4) = "code";
		s_indexOfAnyStrings_OrdinalIgnoreCase_8CFD3E39BB63A31595D8A2B2F2BF99B948EA97CAE7DB2FC39B80B682682AFFA0 = SearchValues.Create(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray5<string>, string>(in buffer2, 5), StringComparison.OrdinalIgnoreCase);
		buffer2 = default(_003C_003Ey__InlineArray5<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 0) = "gdscript";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 1) = "csharpsc";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 2) = "gdextens";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 3) = "extensio";
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray5<string>, string>(ref buffer2, 4) = "local://";
		s_indexOfAnyStrings_OrdinalIgnoreCase_B6C7509F82BA11CF45AB3D08E001EEFC7955C5906A8E3BE7E147EAAAB6533794 = SearchValues.Create(global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray5<string>, string>(in buffer2, 5), StringComparison.OrdinalIgnoreCase);
		string reference = "://";
		s_indexOfString_2681E66F39A8B7964FC735A9BE3145D098828ECB3CE4F4C67C757ECDFCEE6833 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		reference = "\"type\"";
		s_indexOfString_2A878DB9BE1A71978FC34CB4C512D638EBAC04F270B41C1AA014091612B61527 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		reference = "\"png_base64\"";
		s_indexOfString_36F4D3030D82929DC7EADF216BBEA1305C8AB6324FF55E37A7C72BD2DE843F68 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		reference = "\"display_mode\"";
		s_indexOfString_3E8F6A9335F5942092785D27DE72DB3384C8C0449CA628FE5B2897594A07FE58 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		reference = "\"source_path\"";
		s_indexOfString_67C631CDB34E2886C3AE30BB53722CECED1CF2AD6A5B8816E142CFC17449008B = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		reference = "/wor";
		s_indexOfString_84A21C6523C7ECB212BDCD85C4F678722342B52A2DF6FA35939E13BB09255E7A = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		reference = "[ext_resource";
		s_indexOfString_8B87CF6241D53984E594C711D2A93E47010C4048A8AB0A6E3645C62418E90839 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		reference = "\"edit_source_png_base64\"";
		s_indexOfString_B139E9D7E48E6962C345FE35EDCD52AC8EC2E8654DAAF4613535DB30693C7602 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.Ordinal);
		reference = "res://";
		s_indexOfString_DBCF5569720FA761AC36428669700C2B4C4C01E45E3C9F5B81E6E63E80414563 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		reference = "dest_files";
		s_indexOfString_dest_files_OrdinalIgnoreCase = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		reference = "path";
		s_indexOfString_path_OrdinalIgnoreCase = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		reference = "uid";
		s_indexOfString_uid_OrdinalIgnoreCase = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		s_nonAscii_9FA52D3BAECB644578472387D5284CC6F36F408FEB88A04BA674CE14F24D2386 = SearchValues.Create("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyzK");
	}
}

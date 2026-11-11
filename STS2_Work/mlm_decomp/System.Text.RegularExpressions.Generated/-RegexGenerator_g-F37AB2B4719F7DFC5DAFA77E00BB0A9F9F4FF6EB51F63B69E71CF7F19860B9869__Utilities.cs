using System.Buffers;
using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.26416")]
internal static class _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities
{
	internal static readonly TimeSpan s_defaultTimeout = ((AppContext.GetData("REGEX_DEFAULT_MATCH_TIMEOUT") is TimeSpan timeSpan) ? timeSpan : Regex.InfiniteMatchTimeout);

	internal static readonly bool s_hasTimeout = s_defaultTimeout != Regex.InfiniteMatchTimeout;

	internal static readonly SearchValues<string> s_indexOfString_237F0CBD8E0FEBD202F6C2730CD0C1005DDDB48603A88196DFC6AFE750775153;

	internal static readonly SearchValues<string> s_indexOfString_B6A7039197C22B7D955DBE3F0BCE05DCF21E641797D8C50CB9653648F162BDED;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static void StackPop(int[] stack, ref int pos, out int arg0, out int arg1)
	{
		arg0 = stack[--pos];
		arg1 = stack[--pos];
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

	static _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities()
	{
		string reference = "steam://url/communityfilepage/";
		s_indexOfString_237F0CBD8E0FEBD202F6C2730CD0C1005DDDB48603A88196DFC6AFE750775153 = SearchValues.Create(new ReadOnlySpan<string>(in reference), StringComparison.OrdinalIgnoreCase);
		string reference2 = "steamcommunity.com/";
		s_indexOfString_B6A7039197C22B7D955DBE3F0BCE05DCF21E641797D8C50CB9653648F162BDED = SearchValues.Create(new ReadOnlySpan<string>(in reference2), StringComparison.OrdinalIgnoreCase);
	}
}

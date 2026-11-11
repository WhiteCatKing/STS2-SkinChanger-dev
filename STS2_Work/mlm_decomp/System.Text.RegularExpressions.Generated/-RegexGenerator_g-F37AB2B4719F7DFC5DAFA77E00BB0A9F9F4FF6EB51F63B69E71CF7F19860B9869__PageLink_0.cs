using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.26416")]
internal sealed class _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__PageLink_0 : Regex
{
	private sealed class RunnerFactory : RegexRunnerFactory
	{
		private sealed class Runner : RegexRunner
		{
			protected override void Scan(ReadOnlySpan<char> inputSpan)
			{
				while (TryFindNextPossibleStartingPosition(inputSpan) && !TryMatchAtCurrentPosition(inputSpan) && runtextpos != inputSpan.Length)
				{
					runtextpos++;
					if (_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 44)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_indexOfString_B6A7039197C22B7D955DBE3F0BCE05DCF21E641797D8C50CB9653648F162BDED);
					if (num2 >= 0)
					{
						runtextpos = num + num2;
						return true;
					}
				}
				runtextpos = inputSpan.Length;
				return false;
			}

			private bool TryMatchAtCurrentPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				int start = num;
				int num2 = 0;
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if ((uint)span.Length < 19u || !span.StartsWith("steamcommunity.com/".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 20u)
				{
					UncaptureUntil(0);
					return false;
				}
				switch (span[19])
				{
				case 'S':
				case 's':
					if ((uint)span.Length < 30u || !span.Slice(20).StartsWith("haredfiles".AsSpan(), StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 30;
					span = inputSpan.Slice(num);
					break;
				case 'W':
				case 'w':
				{
					char c;
					if ((uint)span.Length < 27u || !span.Slice(20).StartsWith("or".AsSpan(), StringComparison.OrdinalIgnoreCase) || (((c = span[22]) | 0x20) != 107 && c != 'K') || !span.Slice(23).StartsWith("shop".AsSpan(), StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 27;
					span = inputSpan.Slice(num);
					break;
				}
				default:
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 12u || !span.StartsWith("/filedetails".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length > 12u && span[12] == '/')
				{
					span = span.Slice(1);
					num++;
				}
				if ((uint)span.Length < 13u || span[12] != '?')
				{
					UncaptureUntil(0);
					return false;
				}
				num += 13;
				span = inputSpan.Slice(num);
				num5 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
					num5++;
					num4 = num;
					while (true)
					{
						num3 = Crawlpos();
						_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.StackPush(ref runstack, ref pos, num4, num3);
						if (span.IsEmpty || span[0] != '&')
						{
							goto IL_0296;
						}
						num++;
						span = inputSpan.Slice(num);
						if (num5 == 0)
						{
							break;
						}
						goto IL_0338;
						IL_0296:
						_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.StackPop(runstack, ref pos, out num3, out num4);
						UncaptureUntil(num3);
						if (_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						num = num4;
						span = inputSpan.Slice(num);
						char c;
						if (!span.IsEmpty && !(((c = span[0]) < '\u0080') ? (("쇿\uffff\ufff6\uffff\uffff\uffff\uffff\uffff"[(int)c >> 4] & (1 << (c & 0xF))) == 0) : (!RegexRunner.CharInClass(c, "\u0001\u0002\u0001#$d"))))
						{
							num++;
							span = inputSpan.Slice(num);
							num4 = num;
							continue;
						}
						if (--num5 < 0)
						{
							UncaptureUntil(0);
							return false;
						}
						num = runstack[--pos];
						UncaptureUntil(runstack[--pos]);
						span = inputSpan.Slice(num);
						goto IL_0338;
						IL_0338:
						if ((uint)span.Length >= 3u && span.StartsWith("id".AsSpan(), StringComparison.OrdinalIgnoreCase) && span[2] == '=')
						{
							num += 3;
							span = inputSpan.Slice(num);
							num2 = num;
							int i;
							for (i = 0; i < 20 && (uint)i < (uint)span.Length && char.IsDigit(span[i]); i++)
							{
							}
							if (i != 0)
							{
								span = span.Slice(i);
								num += i;
								Capture(1, num2, num);
								runtextpos = num;
								Capture(0, start, num);
								return true;
							}
						}
						if (_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (num5 == 0)
						{
							UncaptureUntil(0);
							return false;
						}
						goto IL_0296;
					}
				}
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int capturePosition)
				{
					while (Crawlpos() > capturePosition)
					{
						Uncapture();
					}
				}
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__PageLink_0 Instance = new _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__PageLink_0();

	private _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__PageLink_0()
	{
		pattern = "steamcommunity\\.com/(?:sharedfiles|workshop)/filedetails/?\\?(?:[^#\\s]*?&)?id=(\\d{1,20})";
		roptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

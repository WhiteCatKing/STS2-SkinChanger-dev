using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientMapIconRegex_30 : Regex
{
	private sealed class RunnerFactory : RegexRunnerFactory
	{
		private sealed class Runner : RegexRunner
		{
			protected override void Scan(ReadOnlySpan<char> inputSpan)
			{
				if (TryFindNextPossibleStartingPosition(inputSpan) && !TryMatchAtCurrentPosition(inputSpan))
				{
					runtextpos = inputSpan.Length;
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 51 && num == 0)
				{
					return true;
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
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				char c;
				if ((uint)span.Length < 46u || !span.StartsWith("res://images/pac", StringComparison.OrdinalIgnoreCase) || (((c = span[16]) | 0x20) != 107 && c != 'K') || !span.Slice(17).StartsWith("ed/map/ancients/ancient_node_", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 46;
				span = inputSpan.Slice(num);
				num2 = num;
				if (span.IsEmpty || char.IsBetween(span[0], '.', '/'))
				{
					UncaptureUntil(0);
					return false;
				}
				num++;
				span = inputSpan.Slice(num);
				num4 = num;
				while (true)
				{
					num3 = Crawlpos();
					Capture(1, num2, num);
					num5 = 0;
					while (true)
					{
						_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
						num5++;
						if ((uint)span.Length < 8u || !span.StartsWith("_outline", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0197;
						}
						num += 8;
						span = inputSpan.Slice(num);
						if (num5 == 0)
						{
							continue;
						}
						goto IL_01d1;
						IL_01d1:
						if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
						{
							char c2 = span[1];
							if ((uint)c2 <= 84u)
							{
								if (c2 == 'P')
								{
									goto IL_0221;
								}
								if (c2 != 'T')
								{
									goto IL_0197;
								}
							}
							else
							{
								if (c2 == 'p')
								{
									goto IL_0221;
								}
								if (c2 != 't')
								{
									goto IL_0197;
								}
							}
							if ((uint)span.Length >= 5u && span.Slice(2).StartsWith("res", StringComparison.OrdinalIgnoreCase))
							{
								num += 5;
								span = inputSpan.Slice(num);
								goto IL_0293;
							}
						}
						goto IL_0197;
						IL_0221:
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0197;
						}
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_0293;
						IL_0197:
						if (--num5 < 0)
						{
							break;
						}
						num = runstack[--pos];
						UncaptureUntil(runstack[--pos]);
						span = inputSpan.Slice(num);
						goto IL_01d1;
						IL_0293:
						if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
						{
							runtextpos = num;
							Capture(0, start, num);
							return true;
						}
						goto IL_0197;
					}
					UncaptureUntil(num3);
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num4;
					span = inputSpan.Slice(num);
					if (span.IsEmpty || char.IsBetween(span[0], '.', '/'))
					{
						break;
					}
					num++;
					span = inputSpan.Slice(num);
					num4 = num;
				}
				UncaptureUntil(0);
				return false;
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

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientMapIconRegex_30 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientMapIconRegex_30();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientMapIconRegex_30()
	{
		pattern = "^res://images/packed/map/ancients/ancient_node_([^/.]+?)(?:_outline)?\\.(?:png|tres)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasPageRegex_50 : Regex
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
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 5)
				{
					if (num <= 0 || inputSpan[num - 1] == '\n')
					{
						goto IL_0058;
					}
					int num2 = inputSpan.Slice(num).IndexOf('\n');
					if ((uint)num2 <= inputSpan.Length - num - 1)
					{
						num += num2 + 1;
						if (num <= inputSpan.Length - 5)
						{
							goto IL_0058;
						}
					}
				}
				goto IL_0079;
				IL_0058:
				int num3 = inputSpan.Slice(num).IndexOfAnyExcept('\n', '\r');
				if (num3 >= 0)
				{
					runtextpos = num + num3;
					return true;
				}
				goto IL_0079;
				IL_0079:
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
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num > 0 && inputSpan[num - 1] != '\n')
				{
					UncaptureUntil(0);
					return false;
				}
				num2 = num;
				num5 = num;
				int num9 = span.IndexOfAny('\n', '\r');
				if (num9 < 0)
				{
					num9 = span.Length;
				}
				if (num9 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num9);
				num += num9;
				num6 = num;
				num5++;
				while (true)
				{
					num3 = Crawlpos();
					if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
					{
						char c = span[1];
						if ((uint)c <= 87u)
						{
							if (c != 'J')
							{
								if (c == 'P')
								{
									goto IL_0148;
								}
								if (c != 'W')
								{
									goto IL_008a;
								}
								goto IL_0185;
							}
						}
						else if (c != 'j')
						{
							if (c == 'p')
							{
								goto IL_0148;
							}
							if (c != 'w')
							{
								goto IL_008a;
							}
							goto IL_0185;
						}
						if ((uint)span.Length >= 3u && (span[2] | 0x20) == 112)
						{
							if ((uint)span.Length > 3u && (span[3] | 0x20) == 101)
							{
								span = span.Slice(1);
								num++;
							}
							if ((uint)span.Length >= 4u && (span[3] | 0x20) == 103)
							{
								num += 4;
								span = inputSpan.Slice(num);
								goto IL_0235;
							}
						}
					}
					goto IL_008a;
					IL_008a:
					UncaptureUntil(num3);
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num5 >= num6 || (num6 = inputSpan.Slice(num5, num6 - num5).LastIndexOf('.')) < 0)
					{
						break;
					}
					num6 += num5;
					num = num6;
					span = inputSpan.Slice(num);
					continue;
					IL_0148:
					if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_008a;
					}
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_0235;
					IL_0185:
					if ((uint)span.Length < 5u || !span.Slice(2).StartsWith("ebp", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_008a;
					}
					num += 5;
					span = inputSpan.Slice(num);
					goto IL_0235;
					IL_0235:
					Capture(1, num2, num);
					num7 = num;
					int i;
					for (i = 0; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
					{
					}
					span = span.Slice(i);
					num += i;
					num8 = num;
					while (true)
					{
						num4 = Crawlpos();
						if ((uint)num < (uint)inputSpan.Length && inputSpan[num] != '\n')
						{
							UncaptureUntil(num4);
							if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
							{
								CheckTimeout();
							}
							if (num7 >= num8)
							{
								break;
							}
							num = --num8;
							span = inputSpan.Slice(num);
							continue;
						}
						runtextpos = num;
						Capture(0, start, num);
						return true;
					}
					goto IL_008a;
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasPageRegex_50 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasPageRegex_50();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasPageRegex_50()
	{
		pattern = "(?im)^([^\\r\\n]+\\.(?:png|webp|jpe?g))\\s*$";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

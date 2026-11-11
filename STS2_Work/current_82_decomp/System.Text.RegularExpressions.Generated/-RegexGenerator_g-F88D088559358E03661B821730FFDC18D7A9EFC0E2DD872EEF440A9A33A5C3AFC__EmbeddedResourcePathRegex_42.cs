using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__EmbeddedResourcePathRegex_42 : Regex
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
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 10)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_indexOfString_DBCF5569720FA761AC36428669700C2B4C4C01E45E3C9F5B81E6E63E80414563);
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
				int num6 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if ((uint)span.Length < 6u || !span.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				char c;
				if ((uint)span.Length < 7u || ((c = span[6]) < '\u0080' && ("\ud9fe\uffffﱺ꿿\uffff\ud7ff\uffff\ud7ff"[(int)c >> 4] & (1 << (c & 0xF))) == 0))
				{
					return false;
				}
				num += 7;
				span = inputSpan.Slice(num);
				num6 = num;
				while (true)
				{
					if (span.IsEmpty || span[0] != '.')
					{
						goto IL_008b;
					}
					num4 = num;
					if ((uint)span.Length >= 3u && span.Slice(1).StartsWith("sp", StringComparison.OrdinalIgnoreCase) && (uint)span.Length >= 4u)
					{
						char c2 = span[3];
						if ((uint)c2 <= 83u)
						{
							if (c2 == 'A')
							{
								goto IL_016f;
							}
							if (c2 == 'S')
							{
								goto IL_01a6;
							}
						}
						else
						{
							if (c2 == 'a')
							{
								goto IL_016f;
							}
							if (c2 == 's')
							{
								goto IL_01a6;
							}
						}
					}
					goto IL_0202;
					IL_0543:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 5u || !span.Slice(1).StartsWith("webp", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0589;
					}
					num2 = 8;
					num += 5;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_03ca:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 3u || (span[2] | 0x20) != 100)
					{
						goto IL_0404;
					}
					num3 = 1;
					num += 3;
					span = inputSpan.Slice(num);
					goto IL_046a;
					IL_02e1:
					if ((uint)span.Length >= 5u && span.Slice(3).StartsWith("es", StringComparison.OrdinalIgnoreCase))
					{
						num += 5;
						span = inputSpan.Slice(num);
						goto IL_0313;
					}
					goto IL_031a;
					IL_072d:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 5u || !span.Slice(1).StartsWith("json", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0774;
					}
					num2 = 12;
					num += 5;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_0589:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 3u && span.Slice(1).StartsWith("jp", StringComparison.OrdinalIgnoreCase))
					{
						if ((uint)span.Length > 3u && (span[3] | 0x20) == 101)
						{
							span = span.Slice(1);
							num++;
						}
						if ((uint)span.Length >= 4u && (span[3] | 0x20) == 103)
						{
							num2 = 9;
							num += 4;
							span = inputSpan.Slice(num);
							goto IL_089f;
						}
					}
					goto IL_0612;
					IL_0313:
					num2 = 2;
					goto IL_089f;
					IL_046a:
					num2 = 4;
					goto IL_089f;
					IL_031a:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 3u || !span.Slice(1).StartsWith("cs", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0360;
					}
					num2 = 3;
					num += 3;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_0774:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("ogg", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_07bb;
					}
					num2 = 13;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_01a6:
					if ((uint)span.Length >= 7u && !(((c = span[4]) | 0x20) != 107 && c != 'K') && span.Slice(5).StartsWith("el", StringComparison.OrdinalIgnoreCase))
					{
						num += 7;
						span = inputSpan.Slice(num);
						goto IL_01fb;
					}
					goto IL_0202;
					IL_0360:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 2u && (span[1] | 0x20) == 103)
					{
						num5 = num;
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("dc", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_03ca;
						}
						num3 = 0;
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_046a;
					}
					goto IL_0471;
					IL_089f:
					while (true)
					{
						int num7 = num;
						if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						int num8 = num;
						if (!span.IsEmpty && (c = span[0]) < '~' && ("☁\0\u0385倀\0⠀\0⠀"[(int)c >> 4] & (1 << (c & 0xF))) != 0)
						{
							num++;
							span = inputSpan.Slice(num);
						}
						else
						{
							num = num8;
							span = inputSpan.Slice(num);
							if (num < inputSpan.Length - 1 || ((uint)num < (uint)inputSpan.Length && inputSpan[num] != '\n'))
							{
								if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
								{
									CheckTimeout();
								}
								switch (num2)
								{
								case 15:
									break;
								case 0:
									goto IL_0202;
								case 1:
									goto IL_0248;
								case 2:
									goto IL_031a;
								case 3:
									goto IL_0360;
								case 4:
									goto IL_044a;
								case 5:
									goto IL_04b7;
								case 6:
									goto IL_04fd;
								case 7:
									goto IL_0543;
								case 8:
									goto IL_0589;
								case 9:
									goto IL_0612;
								case 10:
									goto IL_06e6;
								case 11:
									goto IL_072d;
								case 12:
									goto IL_0774;
								case 13:
									goto IL_07bb;
								case 14:
									goto IL_0802;
								default:
									continue;
								}
								break;
							}
						}
						num = num7;
						span = inputSpan.Slice(num);
						runtextpos = num;
						Capture(0, start, num);
						return true;
					}
					goto IL_008b;
					IL_044a:
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					switch (num3)
					{
					case 0:
						break;
					case 1:
						goto IL_0404;
					default:
						goto IL_046a;
					case 2:
						goto IL_0471;
					}
					goto IL_03ca;
					IL_016f:
					if ((uint)span.Length >= 8u && span.Slice(4).StartsWith("tlas", StringComparison.OrdinalIgnoreCase))
					{
						num += 8;
						span = inputSpan.Slice(num);
						goto IL_01fb;
					}
					goto IL_0202;
					IL_0682:
					if ((uint)span.Length >= 4u && (span[3] | 0x20) == 103)
					{
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_06de;
					}
					goto IL_06e6;
					IL_008b:
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num6;
					span = inputSpan.Slice(num);
					if (span.IsEmpty || ((c = span[0]) < '\u0080' && ("\ud9fe\uffffﱺ꿿\uffff\ud7ff\uffff\ud7ff"[(int)c >> 4] & (1 << (c & 0xF))) == 0))
					{
						break;
					}
					num++;
					span = inputSpan.Slice(num);
					num6 = num;
					continue;
					IL_01fb:
					num2 = 0;
					goto IL_089f;
					IL_0202:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 5u || !span.Slice(1).StartsWith("ctex", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0248;
					}
					num2 = 1;
					num += 5;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_0471:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("scn", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_04b7;
					}
					num2 = 5;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_07bb:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("wav", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0802;
					}
					num2 = 14;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_0248:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 2u && (span[1] | 0x20) == 116 && (uint)span.Length >= 3u)
					{
						char c2 = span[2];
						if ((uint)c2 <= 83u)
						{
							if (c2 == 'R')
							{
								goto IL_02e1;
							}
							if (c2 == 'S')
							{
								goto IL_02ad;
							}
						}
						else
						{
							if (c2 == 'r')
							{
								goto IL_02e1;
							}
							if (c2 == 's')
							{
								goto IL_02ad;
							}
						}
					}
					goto IL_031a;
					IL_0612:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 2u && (span[1] | 0x20) == 115 && (uint)span.Length >= 3u)
					{
						char c2 = span[2];
						if ((uint)c2 <= 86u)
						{
							if (c2 != 'K')
							{
								if (c2 == 'V')
								{
									goto IL_0682;
								}
								goto IL_06e6;
							}
						}
						else if (c2 != 'k')
						{
							if (c2 == 'v')
							{
								goto IL_0682;
							}
							if (c2 != 'K')
							{
								goto IL_06e6;
							}
						}
						if ((uint)span.Length >= 5u && span.Slice(3).StartsWith("el", StringComparison.OrdinalIgnoreCase))
						{
							num += 5;
							span = inputSpan.Slice(num);
							goto IL_06de;
						}
					}
					goto IL_06e6;
					IL_04b7:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("res", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_04fd;
					}
					num2 = 6;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_0404:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 9u && span.Slice(2).StartsWith("dshader", StringComparison.OrdinalIgnoreCase))
					{
						num3 = 2;
						num += 9;
						span = inputSpan.Slice(num);
						goto IL_046a;
					}
					goto IL_0471;
					IL_06de:
					num2 = 10;
					goto IL_089f;
					IL_06e6:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 6u || !span.Slice(1).StartsWith("atlas", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_072d;
					}
					num2 = 11;
					num += 6;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_04fd:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("png", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0543;
					}
					num2 = 7;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_0802:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("mp3", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_008b;
					}
					num2 = 15;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_089f;
					IL_02ad:
					if ((uint)span.Length >= 5u && span.Slice(3).StartsWith("cn", StringComparison.OrdinalIgnoreCase))
					{
						num += 5;
						span = inputSpan.Slice(num);
						goto IL_0313;
					}
					goto IL_031a;
				}
				return false;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__EmbeddedResourcePathRegex_42 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__EmbeddedResourcePathRegex_42();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__EmbeddedResourcePathRegex_42()
	{
		pattern = "res://[^\\x00\\\"'\\r\\n\\t \\]\\[(){}<>]+?\\.(?:spatlas|spskel|ctex|tscn|tres|cs|gdc|gd|gdshader|scn|res|png|webp|jpe?g|svg|skel|atlas|json|ogg|wav|mp3)(?=[\\x00\\\"'\\r\\n\\t \\]\\[(){}<>]|$)";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__BinaryExecutableResourcePathRegex_53 : Regex
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
				if (num <= inputSpan.Length - 9)
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
				num += 6;
				span = inputSpan.Slice(num);
				num6 = num;
				while (true)
				{
					if (span.IsEmpty || span[0] != '.')
					{
						goto IL_0053;
					}
					num4 = num;
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("dll", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_011d;
					}
					num2 = 0;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_02de;
					IL_02bb:
					num2 = 2;
					goto IL_02de;
					IL_02de:
					while (true)
					{
						int num7 = num;
						if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						int num8 = num;
						if (!span.IsEmpty && span[0] == '\0')
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
								case 0:
									break;
								case 1:
									goto IL_0163;
								case 2:
									goto IL_0298;
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
					goto IL_011d;
					IL_024d:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 12u || !span.Slice(2).StartsWith("dextension", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0053;
					}
					num3 = 3;
					num += 12;
					span = inputSpan.Slice(num);
					goto IL_02bb;
					IL_01c2:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("dc", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0208;
					}
					num3 = 1;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_02bb;
					IL_011d:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 3u || !span.Slice(1).StartsWith("cs", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0163;
					}
					num2 = 1;
					num += 3;
					span = inputSpan.Slice(num);
					goto IL_02de;
					IL_0298:
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					switch (num3)
					{
					case 3:
						break;
					case 0:
						goto IL_01c2;
					case 1:
						goto IL_0208;
					case 2:
						goto IL_024d;
					default:
						goto IL_02bb;
					}
					goto IL_0053;
					IL_0053:
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num6;
					span = inputSpan.Slice(num);
					if (span.IsEmpty || span[0] == '\0')
					{
						return false;
					}
					num++;
					span = inputSpan.Slice(num);
					num6 = span.IndexOfAny('\0', '.');
					if ((uint)num6 >= (uint)span.Length || span[num6] == '\0')
					{
						break;
					}
					num += num6;
					span = inputSpan.Slice(num);
					num6 = num;
					continue;
					IL_0163:
					num = num4;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 2u || (span[1] | 0x20) != 103)
					{
						goto IL_0053;
					}
					num5 = num;
					if ((uint)span.Length < 3u || (span[2] | 0x20) != 100)
					{
						goto IL_01c2;
					}
					num3 = 0;
					num += 3;
					span = inputSpan.Slice(num);
					goto IL_02bb;
					IL_0208:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 9u || !span.Slice(2).StartsWith("dshader", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_024d;
					}
					num3 = 2;
					num += 9;
					span = inputSpan.Slice(num);
					goto IL_02bb;
				}
				return false;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__BinaryExecutableResourcePathRegex_53 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__BinaryExecutableResourcePathRegex_53();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__BinaryExecutableResourcePathRegex_53()
	{
		pattern = "(?i)res://[^\\x00]*?\\.(?:dll|cs|gd|gdc|gdshader|gdextension)(?=\\x00|$)";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

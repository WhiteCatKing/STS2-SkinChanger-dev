using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientLayerImageRegex_20 : Regex
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
				if (num <= inputSpan.Length - 37 && num == 0)
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
				int capturePosition2 = 0;
				int num5 = 0;
				int num6 = 0;
				int num7 = 0;
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 22u || !span.StartsWith("res://images/ancients/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 22;
				span = inputSpan.Slice(num);
				num7 = num;
				if (span.IsEmpty || span[0] == '\n')
				{
					UncaptureUntil(0);
					return false;
				}
				num++;
				span = inputSpan.Slice(num);
				num10 = num;
				while (true)
				{
					num9 = Crawlpos();
					Capture(1, num7, num);
					if (span.IsEmpty || span[0] != '_')
					{
						goto IL_00ab;
					}
					num++;
					span = inputSpan.Slice(num);
					num8 = num;
					num5 = num;
					num4 = Crawlpos();
					if ((uint)span.Length >= 8u && span.StartsWith("characte", StringComparison.OrdinalIgnoreCase))
					{
						num6 = num;
						capturePosition2 = Crawlpos();
						if ((uint)span.Length < 9u || (span[8] | 0x20) != 114)
						{
							goto IL_01e6;
						}
						num3 = 0;
						num += 9;
						span = inputSpan.Slice(num);
						goto IL_02c9;
					}
					goto IL_02d0;
					IL_02d0:
					num = num5;
					span = inputSpan.Slice(num);
					UncaptureUntil(num4);
					char c;
					if ((uint)span.Length < 16u || !span.StartsWith("bac", StringComparison.OrdinalIgnoreCase) || (((c = span[3]) | 0x20) != 107 && c != 'K') || !span.Slice(4).StartsWith("ground_cover", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_00ab;
					}
					num2 = 1;
					num += 16;
					span = inputSpan.Slice(num);
					goto IL_037a;
					IL_00ab:
					UncaptureUntil(num9);
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num10;
					span = inputSpan.Slice(num);
					if (span.IsEmpty || span[0] == '\n')
					{
						UncaptureUntil(0);
						return false;
					}
					num++;
					span = inputSpan.Slice(num);
					num10 = span.IndexOfAny('\n', '_');
					if ((uint)num10 >= (uint)span.Length || span[num10] == '\n')
					{
						break;
					}
					num += num10;
					span = inputSpan.Slice(num);
					num10 = num;
					continue;
					IL_0237:
					num = num6;
					span = inputSpan.Slice(num);
					UncaptureUntil(capturePosition2);
					if ((uint)span.Length >= 14u && span.Slice(8).StartsWith("r_mas", StringComparison.OrdinalIgnoreCase) && !(((c = span[13]) | 0x20) != 107 && c != 'K'))
					{
						num3 = 2;
						num += 14;
						span = inputSpan.Slice(num);
						goto IL_02c9;
					}
					goto IL_02d0;
					IL_02c9:
					num2 = 0;
					goto IL_037a;
					IL_01e6:
					num = num6;
					span = inputSpan.Slice(num);
					UncaptureUntil(capturePosition2);
					if ((uint)span.Length < 18u || !span.Slice(8).StartsWith("r_sleeping", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0237;
					}
					num3 = 1;
					num += 18;
					span = inputSpan.Slice(num);
					goto IL_02c9;
					IL_037a:
					while (true)
					{
						Capture(2, num8, num);
						if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
						{
							char c2 = span[1];
							if ((uint)c2 <= 87u)
							{
								if ((uint)c2 <= 80u)
								{
									if (c2 == 'J')
									{
										goto IL_0485;
									}
									if (c2 == 'P')
									{
										goto IL_040b;
									}
								}
								else
								{
									if (c2 == 'S')
									{
										goto IL_04fd;
									}
									if (c2 == 'W')
									{
										goto IL_0448;
									}
								}
							}
							else if ((uint)c2 <= 112u)
							{
								if (c2 == 'j')
								{
									goto IL_0485;
								}
								if (c2 == 'p')
								{
									goto IL_040b;
								}
							}
							else
							{
								if (c2 == 's')
								{
									goto IL_04fd;
								}
								if (c2 == 'w')
								{
									goto IL_0448;
								}
							}
						}
						goto IL_0360;
						IL_0535:
						if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
						{
							runtextpos = num;
							Capture(0, start, num);
							return true;
						}
						goto IL_0360;
						IL_0485:
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
								goto IL_0535;
							}
						}
						goto IL_0360;
						IL_0448:
						if ((uint)span.Length < 5u || !span.Slice(2).StartsWith("ebp", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0360;
						}
						num += 5;
						span = inputSpan.Slice(num);
						goto IL_0535;
						IL_04fd:
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("vg", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0360;
						}
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_0535;
						IL_040b:
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0360;
						}
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_0535;
						IL_0360:
						if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (num2 == 0)
						{
							break;
						}
						if (num2 != 1)
						{
							continue;
						}
						goto IL_00ab;
					}
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					switch (num3)
					{
					case 0:
						break;
					case 1:
						goto IL_0237;
					default:
						goto IL_02c9;
					case 2:
						goto IL_02d0;
					}
					goto IL_01e6;
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

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientLayerImageRegex_20 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientLayerImageRegex_20();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__AncientLayerImageRegex_20()
	{
		pattern = "^res://images/ancients/(?<id>.+?)_(?<kind>character|character_sleeping|character_mask|background_cover)\\.(?:png|webp|jpe?g|svg)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		base.CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "id", 1 },
			{ "kind", 2 }
		};
		capslist = new string[3] { "0", "id", "kind" };
		capsize = 3;
	}
}

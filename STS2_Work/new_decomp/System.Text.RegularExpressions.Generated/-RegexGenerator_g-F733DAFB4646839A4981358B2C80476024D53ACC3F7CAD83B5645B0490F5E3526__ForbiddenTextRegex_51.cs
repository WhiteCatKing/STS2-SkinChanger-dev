using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ForbiddenTextRegex_51 : Regex
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
				if (num <= inputSpan.Length - 3)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_indexOfAnyStrings_OrdinalIgnoreCase_2CDD1846E7A9AA13DC45A8F092D4D08BD3ACE049796DAFCEAAA7C5411B326494);
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
				int num7 = 0;
				int num8 = 0;
				int num9 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				int num10 = 0;
				if (span.IsEmpty)
				{
					return false;
				}
				char c;
				switch (span[0])
				{
				case 'E':
				case 'e':
				{
					if ((uint)span.Length < 12u || !span.Slice(1).StartsWith("xt_resource", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					num += 12;
					span = inputSpan.Slice(num);
					num8 = num;
					int num13 = span.IndexOfAny('\n', '\r');
					if (num13 < 0)
					{
						num13 = span.Length;
					}
					span = span.Slice(num13);
					num += num13;
					num9 = num;
					while (true)
					{
						if (!span.IsEmpty && (span[0] | 0x20) == 115 && (uint)span.Length >= 2u)
						{
							char c2 = span[1];
							if ((uint)c2 <= 72u)
							{
								if (c2 == 'C')
								{
									goto IL_01b9;
								}
								if (c2 != 'H')
								{
									goto IL_011d;
								}
							}
							else
							{
								if (c2 == 'c')
								{
									goto IL_01b9;
								}
								if (c2 != 'h')
								{
									goto IL_011d;
								}
							}
							if ((uint)span.Length >= 6u && span.Slice(2).StartsWith("ader", StringComparison.OrdinalIgnoreCase))
							{
								num += 6;
								span = inputSpan.Slice(num);
								break;
							}
						}
						goto IL_011d;
						IL_01b9:
						if ((uint)span.Length >= 6u && span.Slice(2).StartsWith("ript", StringComparison.OrdinalIgnoreCase))
						{
							num += 6;
							span = inputSpan.Slice(num);
							break;
						}
						goto IL_011d;
						IL_011d:
						if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (num8 >= num9 || (num9 = inputSpan.Slice(num8, num9 - num8).LastIndexOfAny('S', 's')) < 0)
						{
							return false;
						}
						num9 += num8;
						num = num9;
						span = inputSpan.Slice(num);
					}
					break;
				}
				case 'S':
				case 's':
					if ((uint)span.Length < 2u)
					{
						return false;
					}
					switch (span[1])
					{
					case 'C':
					case 'c':
					{
						if ((uint)span.Length < 6u || !span.Slice(2).StartsWith("ript", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						int j;
						for (j = 6; (uint)j < (uint)span.Length && char.IsWhiteSpace(span[j]); j++)
						{
						}
						span = span.Slice(j);
						num += j;
						if (span.IsEmpty || span[0] != '=')
						{
							return false;
						}
						num++;
						span = inputSpan.Slice(num);
						break;
					}
					case 'H':
					case 'h':
					{
						if ((uint)span.Length < 6u || !span.Slice(2).StartsWith("ader", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						int num12 = num;
						int i;
						for (i = 6; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
						{
						}
						span = span.Slice(i);
						num += i;
						if (!span.IsEmpty && span[0] == '=')
						{
							num++;
							span = inputSpan.Slice(num);
							break;
						}
						num = num12;
						span = inputSpan.Slice(num);
						if ((uint)span.Length < 14u || !span.Slice(6).StartsWith("material", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						num += 14;
						span = inputSpan.Slice(num);
						break;
					}
					default:
						return false;
					}
					break;
				case 'G':
				case 'g':
					if ((uint)span.Length < 11u || !span.Slice(1).StartsWith("dextension", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					num += 11;
					span = inputSpan.Slice(num);
					break;
				case 'T':
				case 't':
				{
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("ype", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					int k;
					for (k = 4; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
					{
					}
					span = span.Slice(k);
					num += k;
					if (span.IsEmpty || span[0] != '=')
					{
						return false;
					}
					int l;
					for (l = 1; (uint)l < (uint)span.Length && char.IsWhiteSpace(span[l]); l++)
					{
					}
					span = span.Slice(l);
					num += l;
					if (span.IsEmpty || ((c = span[0]) != '"' && c != '\''))
					{
						return false;
					}
					num5 = num;
					if ((uint)span.Length < 7u || !span.Slice(1).StartsWith("shader", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_051e;
					}
					num2 = 0;
					num += 7;
					span = inputSpan.Slice(num);
					goto IL_06a5;
				}
				case '.':
					num6 = num;
					if ((uint)span.Length < 4u || !span.Slice(1).StartsWith("dll", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_071b;
					}
					num3 = 0;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_0893;
				default:
					{
						return false;
					}
					IL_086e:
					num3 = 2;
					goto IL_0893;
					IL_0893:
					while (true)
					{
						int num11 = num;
						if (!span.IsEmpty && !(((c = span[0]) < '\u0080') ? (("㸀\0\u0085\0\0\0\0\0"[(int)c >> 4] & (1 << (c & 0xF))) == 0) : (!RegexRunner.CharInClass(c, "\0\u0004\u0001\"#'(d"))))
						{
							num++;
							span = inputSpan.Slice(num);
							break;
						}
						num = num11;
						span = inputSpan.Slice(num);
						if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
						{
							break;
						}
						if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						switch (num3)
						{
						case 0:
							break;
						case 1:
							goto IL_0762;
						case 2:
							goto IL_084a;
						default:
							continue;
						}
						goto IL_071b;
					}
					break;
					IL_071b:
					num = num6;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 3u || !span.Slice(1).StartsWith("cs", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0762;
					}
					num3 = 1;
					num += 3;
					span = inputSpan.Slice(num);
					goto IL_0893;
					IL_084a:
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					switch (num4)
					{
					case 0:
						break;
					case 1:
						goto IL_0802;
					case 2:
						return false;
					default:
						goto IL_086e;
					}
					goto IL_07be;
					IL_06a5:
					while (span.IsEmpty || ((c = span[0]) != '"' && c != '\''))
					{
						if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						switch (num2)
						{
						case 0:
							break;
						case 1:
							goto IL_0566;
						case 2:
							goto IL_05ae;
						case 3:
							goto IL_05f6;
						case 4:
							return false;
						default:
							continue;
						}
						goto IL_051e;
					}
					num++;
					span = inputSpan.Slice(num);
					break;
					IL_0762:
					num = num6;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 2u || (span[1] | 0x20) != 103)
					{
						return false;
					}
					num7 = num;
					if ((uint)span.Length < 3u || (span[2] | 0x20) != 100)
					{
						goto IL_07be;
					}
					num4 = 0;
					num += 3;
					span = inputSpan.Slice(num);
					goto IL_086e;
					IL_051e:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 9u || !span.Slice(1).StartsWith("gdscript", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0566;
					}
					num2 = 1;
					num += 9;
					span = inputSpan.Slice(num);
					goto IL_06a5;
					IL_05ae:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 12u || !span.Slice(1).StartsWith("gdextension", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_05f6;
					}
					num2 = 3;
					num += 12;
					span = inputSpan.Slice(num);
					goto IL_06a5;
					IL_07be:
					num = num7;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("dc", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0802;
					}
					num4 = 1;
					num += 4;
					span = inputSpan.Slice(num);
					goto IL_086e;
					IL_05f6:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 12u || !span.Slice(1).StartsWith("pac", StringComparison.OrdinalIgnoreCase) || (((c = span[4]) | 0x20) != 107 && c != 'K') || !span.Slice(5).StartsWith("edscene", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					num2 = 4;
					num += 12;
					span = inputSpan.Slice(num);
					goto IL_06a5;
					IL_0802:
					num = num7;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 9u || !span.Slice(2).StartsWith("dshader", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					num4 = 2;
					num += 9;
					span = inputSpan.Slice(num);
					goto IL_086e;
					IL_0566:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 13u || !span.Slice(1).StartsWith("csharpscript", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_05ae;
					}
					num2 = 2;
					num += 13;
					span = inputSpan.Slice(num);
					goto IL_06a5;
				}
				runtextpos = num;
				Capture(0, start, num);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ForbiddenTextRegex_51 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ForbiddenTextRegex_51();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__ForbiddenTextRegex_51()
	{
		pattern = "(?i)(?:ext_resource[^\\r\\n]*(?:Script|Shader)|script\\s*=|shader\\s*=|ShaderMaterial|GDExtension|type\\s*=\\s*[\\\"'](?:Shader|GDScript|CSharpScript|GDExtension|PackedScene)[\\\"']|\\.(?:dll|cs|gd|gdc|gdshader)(?:\\\"|'|\\s|$))";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

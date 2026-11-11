using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SceneExecutableResourceRegex_54 : Regex
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
					if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 41)
				{
					if (num <= 0 || inputSpan[num - 1] == '\n')
					{
						goto IL_005a;
					}
					int num2 = inputSpan.Slice(num).IndexOf('\n');
					if ((uint)num2 <= inputSpan.Length - num - 1)
					{
						num += num2 + 1;
						if (num <= inputSpan.Length - 41)
						{
							goto IL_005a;
						}
					}
				}
				goto IL_007c;
				IL_005a:
				int num3 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_indexOfString_8B87CF6241D53984E594C711D2A93E47010C4048A8AB0A6E3645C62418E90839);
				if (num3 >= 0)
				{
					runtextpos = num + num3;
					return true;
				}
				goto IL_007c;
				IL_007c:
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
				if (num > 0 && inputSpan[num - 1] != '\n')
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 13u || !span.StartsWith("[ext_resource", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 13;
				span = inputSpan.Slice(num);
				num6 = num;
				int num10 = span.IndexOf(']');
				if (num10 < 0)
				{
					num10 = span.Length;
				}
				span = span.Slice(num10);
				num += num10;
				num7 = num;
				while (true)
				{
					num4 = Crawlpos();
					if ((uint)span.Length >= 6u && span.StartsWith("type", StringComparison.OrdinalIgnoreCase) && span.Slice(4).StartsWith("=\""))
					{
						num += 6;
						span = inputSpan.Slice(num);
						num2 = num;
						if (!span.IsEmpty && (span[0] | 0x20) == 115 && (uint)span.Length >= 2u)
						{
							char c = span[1];
							if ((uint)c <= 72u)
							{
								if (c == 'C')
								{
									goto IL_01b3;
								}
								if (c != 'H')
								{
									goto IL_00ad;
								}
							}
							else
							{
								if (c == 'c')
								{
									goto IL_01b3;
								}
								if (c != 'h')
								{
									goto IL_00ad;
								}
							}
							if ((uint)span.Length >= 6u && span.Slice(2).StartsWith("ader", StringComparison.OrdinalIgnoreCase))
							{
								num += 6;
								span = inputSpan.Slice(num);
								goto IL_0225;
							}
						}
					}
					goto IL_00ad;
					IL_01b3:
					if ((uint)span.Length < 6u || !span.Slice(2).StartsWith("ript", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_00ad;
					}
					num += 6;
					span = inputSpan.Slice(num);
					goto IL_0225;
					IL_00ad:
					UncaptureUntil(num4);
					if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num6 >= num7 || (num7 = inputSpan.Slice(num6, num7 - num6).LastIndexOfAny('T', 't')) < 0)
					{
						break;
					}
					num7 += num6;
					num = num7;
					span = inputSpan.Slice(num);
					continue;
					IL_0225:
					Capture(1, num2, num);
					if (!span.IsEmpty && span[0] == '"')
					{
						num++;
						span = inputSpan.Slice(num);
						num8 = num;
						int num11 = span.IndexOf(']');
						if (num11 < 0)
						{
							num11 = span.Length;
						}
						span = span.Slice(num11);
						num += num11;
						num9 = num;
						while (true)
						{
							num5 = Crawlpos();
							if ((uint)span.Length >= 6u && span.StartsWith("path", StringComparison.OrdinalIgnoreCase) && span.Slice(4).StartsWith("=\""))
							{
								num += 6;
								span = inputSpan.Slice(num);
								num3 = num;
								if ((uint)span.Length >= 6u && span.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
								{
									int num12 = span.Slice(6).IndexOf('"');
									if (num12 < 0)
									{
										num12 = span.Length - 6;
									}
									if (num12 != 0)
									{
										span = span.Slice(num12);
										num += num12;
										num += 6;
										span = inputSpan.Slice(num);
										Capture(2, num3, num);
										if (!span.IsEmpty && span[0] == '"')
										{
											int num13 = span.Slice(1).IndexOf(']');
											if (num13 < 0)
											{
												num13 = span.Length - 1;
											}
											span = span.Slice(num13);
											num += num13;
											if ((uint)span.Length >= 2u && span[1] == ']' && (2 >= span.Length || span[2] == '\n'))
											{
												Capture(0, start, runtextpos = num + 2);
												return true;
											}
										}
									}
								}
							}
							UncaptureUntil(num5);
							if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
							{
								CheckTimeout();
							}
							if (num8 >= num9 || (num9 = inputSpan.Slice(num8, num9 - num8).LastIndexOfAny('P', 'p')) < 0)
							{
								break;
							}
							num9 += num8;
							num = num9;
							span = inputSpan.Slice(num);
						}
					}
					goto IL_00ad;
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

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SceneExecutableResourceRegex_54 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SceneExecutableResourceRegex_54();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SceneExecutableResourceRegex_54()
	{
		pattern = "(?im)^\\[ext_resource[^\\]]*type=\\\"(Script|Shader)\\\"[^\\]]*path=\\\"(res://[^\\\"]+)\\\"[^\\]]*\\]$";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 3;
	}
}

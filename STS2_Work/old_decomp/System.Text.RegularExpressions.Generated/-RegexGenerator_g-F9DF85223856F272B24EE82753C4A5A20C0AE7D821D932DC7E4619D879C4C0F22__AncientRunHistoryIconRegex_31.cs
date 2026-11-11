using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__AncientRunHistoryIconRegex_31 : Regex
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
				if (num <= inputSpan.Length - 33 && num == 0)
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
				if ((uint)span.Length < 28u || !span.StartsWith("res://images/ui/run_history/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 28;
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
						_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
						num5++;
						if ((uint)span.Length < 8u || !span.StartsWith("_outline", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0155;
						}
						num += 8;
						span = inputSpan.Slice(num);
						if (num5 == 0)
						{
							continue;
						}
						goto IL_018f;
						IL_018f:
						if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
						{
							char c = span[1];
							if ((uint)c <= 84u)
							{
								if (c == 'P')
								{
									goto IL_01df;
								}
								if (c != 'T')
								{
									goto IL_0155;
								}
							}
							else
							{
								if (c == 'p')
								{
									goto IL_01df;
								}
								if (c != 't')
								{
									goto IL_0155;
								}
							}
							if ((uint)span.Length >= 5u && span.Slice(2).StartsWith("res", StringComparison.OrdinalIgnoreCase))
							{
								num += 5;
								span = inputSpan.Slice(num);
								goto IL_0251;
							}
						}
						goto IL_0155;
						IL_01df:
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_0155;
						}
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_0251;
						IL_0155:
						if (--num5 < 0)
						{
							break;
						}
						num = runstack[--pos];
						UncaptureUntil(runstack[--pos]);
						span = inputSpan.Slice(num);
						goto IL_018f;
						IL_0251:
						if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
						{
							runtextpos = num;
							Capture(0, start, num);
							return true;
						}
						goto IL_0155;
					}
					UncaptureUntil(num3);
					if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
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

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__AncientRunHistoryIconRegex_31 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__AncientRunHistoryIconRegex_31();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__AncientRunHistoryIconRegex_31()
	{
		pattern = "^res://images/ui/run_history/([^/.]+?)(?:_outline)?\\.(?:png|tres)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

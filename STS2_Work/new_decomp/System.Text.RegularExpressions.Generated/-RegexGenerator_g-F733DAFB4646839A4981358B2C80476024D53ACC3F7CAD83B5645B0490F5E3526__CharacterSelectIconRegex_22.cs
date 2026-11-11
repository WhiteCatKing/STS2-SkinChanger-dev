using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__CharacterSelectIconRegex_22 : Regex
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
				if (num <= inputSpan.Length - 54 && num == 0)
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
				if ((uint)span.Length < 49u || !span.StartsWith("res://images/pac", StringComparison.OrdinalIgnoreCase) || (((c = span[16]) | 0x20) != 107 && c != 'K') || !span.Slice(17).StartsWith("ed/character_select/char_select_", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 49;
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
						_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
						num5++;
						if ((uint)span.Length < 7u || !span.StartsWith("_loc", StringComparison.OrdinalIgnoreCase) || (((c = span[4]) | 0x20) != 107 && c != 'K') || !span.Slice(5).StartsWith("ed", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_01d7;
						}
						num += 7;
						span = inputSpan.Slice(num);
						if (num5 == 0)
						{
							continue;
						}
						goto IL_0211;
						IL_01d7:
						if (--num5 < 0)
						{
							break;
						}
						num = runstack[--pos];
						UncaptureUntil(runstack[--pos]);
						span = inputSpan.Slice(num);
						goto IL_0211;
						IL_02d3:
						if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
						{
							runtextpos = num;
							Capture(0, start, num);
							return true;
						}
						goto IL_01d7;
						IL_0211:
						if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
						{
							char c2 = span[1];
							if ((uint)c2 <= 84u)
							{
								if (c2 == 'P')
								{
									goto IL_0261;
								}
								if (c2 != 'T')
								{
									goto IL_01d7;
								}
							}
							else
							{
								if (c2 == 'p')
								{
									goto IL_0261;
								}
								if (c2 != 't')
								{
									goto IL_01d7;
								}
							}
							if ((uint)span.Length >= 5u && span.Slice(2).StartsWith("res", StringComparison.OrdinalIgnoreCase))
							{
								num += 5;
								span = inputSpan.Slice(num);
								goto IL_02d3;
							}
						}
						goto IL_01d7;
						IL_0261:
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_01d7;
						}
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_02d3;
					}
					UncaptureUntil(num3);
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__CharacterSelectIconRegex_22 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__CharacterSelectIconRegex_22();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__CharacterSelectIconRegex_22()
	{
		pattern = "^res://images/packed/character_select/char_select_([^/.]+?)(?:_locked)?\\.(?:png|tres)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

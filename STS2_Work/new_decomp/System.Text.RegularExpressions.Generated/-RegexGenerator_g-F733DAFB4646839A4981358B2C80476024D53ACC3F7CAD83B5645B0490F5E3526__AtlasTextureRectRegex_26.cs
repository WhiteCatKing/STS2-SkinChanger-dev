using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureRectRegex_26 : Regex
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
				if (num <= inputSpan.Length - 21)
				{
					if (num <= 0 || inputSpan[num - 1] == '\n')
					{
						goto IL_005a;
					}
					int num2 = inputSpan.Slice(num).IndexOf('\n');
					if ((uint)num2 <= inputSpan.Length - num - 1)
					{
						num += num2 + 1;
						if (num <= inputSpan.Length - 21)
						{
							goto IL_005a;
						}
					}
				}
				goto IL_0077;
				IL_005a:
				int num3 = inputSpan.Slice(num).IndexOfNonAsciiOrAny_AE593EF9A4E479A6268F396B40A3642E402C53B13FD4CFDA109DDD36AE686A06();
				if (num3 >= 0)
				{
					runtextpos = num + num3;
					return true;
				}
				goto IL_0077;
				IL_0077:
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
				if (num > 0 && inputSpan[num - 1] != '\n')
				{
					UncaptureUntil(0);
					return false;
				}
				int i;
				for (i = 0; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
				{
				}
				span = span.Slice(i);
				num += i;
				num2 = num;
				if (span.IsEmpty)
				{
					UncaptureUntil(0);
					return false;
				}
				switch (span[0])
				{
				case 'R':
				case 'r':
					if ((uint)span.Length < 6u || !span.Slice(1).StartsWith("egion", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 6;
					span = inputSpan.Slice(num);
					break;
				case 'M':
				case 'm':
					if ((uint)span.Length < 6u || !span.Slice(1).StartsWith("argin", StringComparison.OrdinalIgnoreCase))
					{
						UncaptureUntil(0);
						return false;
					}
					num += 6;
					span = inputSpan.Slice(num);
					break;
				default:
					UncaptureUntil(0);
					return false;
				}
				Capture(1, num2, num);
				int j;
				for (j = 0; (uint)j < (uint)span.Length && char.IsWhiteSpace(span[j]); j++)
				{
				}
				span = span.Slice(j);
				num += j;
				if (span.IsEmpty || span[0] != '=')
				{
					UncaptureUntil(0);
					return false;
				}
				int k;
				for (k = 1; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
				{
				}
				span = span.Slice(k);
				num += k;
				if ((uint)span.Length < 6u || !span.StartsWith("rect2(", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				int l;
				for (l = 6; (uint)l < (uint)span.Length && char.IsWhiteSpace(span[l]); l++)
				{
				}
				span = span.Slice(l);
				num += l;
				num3 = num;
				if (!span.IsEmpty && span[0] == '-')
				{
					span = span.Slice(1);
					num++;
				}
				int num7 = span.IndexOfAnyExcept(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_ascii_40FF030000000000000000);
				if (num7 < 0)
				{
					num7 = span.Length;
				}
				if (num7 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num7);
				num += num7;
				Capture(2, num3, num);
				int m;
				for (m = 0; (uint)m < (uint)span.Length && char.IsWhiteSpace(span[m]); m++)
				{
				}
				span = span.Slice(m);
				num += m;
				if (span.IsEmpty || span[0] != ',')
				{
					UncaptureUntil(0);
					return false;
				}
				int n;
				for (n = 1; (uint)n < (uint)span.Length && char.IsWhiteSpace(span[n]); n++)
				{
				}
				span = span.Slice(n);
				num += n;
				num4 = num;
				if (!span.IsEmpty && span[0] == '-')
				{
					span = span.Slice(1);
					num++;
				}
				int num8 = span.IndexOfAnyExcept(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_ascii_40FF030000000000000000);
				if (num8 < 0)
				{
					num8 = span.Length;
				}
				if (num8 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num8);
				num += num8;
				Capture(3, num4, num);
				int num9;
				for (num9 = 0; (uint)num9 < (uint)span.Length && char.IsWhiteSpace(span[num9]); num9++)
				{
				}
				span = span.Slice(num9);
				num += num9;
				if (span.IsEmpty || span[0] != ',')
				{
					UncaptureUntil(0);
					return false;
				}
				int num10;
				for (num10 = 1; (uint)num10 < (uint)span.Length && char.IsWhiteSpace(span[num10]); num10++)
				{
				}
				span = span.Slice(num10);
				num += num10;
				num5 = num;
				if (!span.IsEmpty && span[0] == '-')
				{
					span = span.Slice(1);
					num++;
				}
				int num11 = span.IndexOfAnyExcept(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_ascii_40FF030000000000000000);
				if (num11 < 0)
				{
					num11 = span.Length;
				}
				if (num11 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num11);
				num += num11;
				Capture(4, num5, num);
				int num12;
				for (num12 = 0; (uint)num12 < (uint)span.Length && char.IsWhiteSpace(span[num12]); num12++)
				{
				}
				span = span.Slice(num12);
				num += num12;
				if (span.IsEmpty || span[0] != ',')
				{
					UncaptureUntil(0);
					return false;
				}
				int num13;
				for (num13 = 1; (uint)num13 < (uint)span.Length && char.IsWhiteSpace(span[num13]); num13++)
				{
				}
				span = span.Slice(num13);
				num += num13;
				num6 = num;
				if (!span.IsEmpty && span[0] == '-')
				{
					span = span.Slice(1);
					num++;
				}
				int num14 = span.IndexOfAnyExcept(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_ascii_40FF030000000000000000);
				if (num14 < 0)
				{
					num14 = span.Length;
				}
				if (num14 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num14);
				num += num14;
				Capture(5, num6, num);
				int num15;
				for (num15 = 0; (uint)num15 < (uint)span.Length && char.IsWhiteSpace(span[num15]); num15++)
				{
				}
				span = span.Slice(num15);
				num += num15;
				if (span.IsEmpty || span[0] != ')')
				{
					UncaptureUntil(0);
					return false;
				}
				Capture(0, start, runtextpos = num + 1);
				return true;
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureRectRegex_26 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureRectRegex_26();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureRectRegex_26()
	{
		pattern = "(?m)^\\s*(?<property>region|margin)\\s*=\\s*Rect2\\(\\s*(?<x>-?[0-9.]+)\\s*,\\s*(?<y>-?[0-9.]+)\\s*,\\s*(?<width>-?[0-9.]+)\\s*,\\s*(?<height>-?[0-9.]+)\\s*\\)";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		base.CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "height", 5 },
			{ "property", 1 },
			{ "width", 4 },
			{ "x", 2 },
			{ "y", 3 }
		};
		capslist = new string[6] { "0", "property", "x", "y", "width", "height" };
		capsize = 6;
	}
}

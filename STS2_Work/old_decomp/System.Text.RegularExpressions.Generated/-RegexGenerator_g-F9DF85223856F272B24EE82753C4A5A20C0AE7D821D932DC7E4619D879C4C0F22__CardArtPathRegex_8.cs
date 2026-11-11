using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CardArtPathRegex_8 : Regex
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
				if (num <= inputSpan.Length - 6)
				{
					int num2 = inputSpan.Slice(num).IndexOf('/');
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
				if (span.IsEmpty || span[0] != '/')
				{
					return false;
				}
				num4 = num;
				if ((uint)span.Length < 15u || !span.Slice(1).StartsWith("card_portraits", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_007f;
				}
				num2 = 0;
				num += 15;
				span = inputSpan.Slice(num);
				goto IL_0330;
				IL_007f:
				num = num4;
				span = inputSpan.Slice(num);
				num++;
				span = inputSpan.Slice(num);
				num6 = num;
				int num10 = span.IndexOf('/');
				if (num10 < 0)
				{
					num10 = span.Length;
				}
				span = span.Slice(num10);
				num += num10;
				num7 = num;
				goto IL_0115;
				IL_0115:
				if ((uint)span.Length < 4u || !span.StartsWith("card", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_00cb;
				}
				num += 4;
				span = inputSpan.Slice(num);
				num8 = num;
				int num11 = span.IndexOf('/');
				if (num11 < 0)
				{
					num11 = span.Length;
				}
				span = span.Slice(num11);
				num += num11;
				num9 = num;
				goto IL_01ba;
				IL_02ea:
				if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				switch (num3)
				{
				case 0:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 9u && span.Slice(5).StartsWith("_art", StringComparison.OrdinalIgnoreCase))
					{
						num3 = 1;
						num += 9;
						span = inputSpan.Slice(num);
						break;
					}
					goto case 1;
				case 1:
					num = num5;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 8u || !span.Slice(5).StartsWith("art", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					num3 = 2;
					num += 8;
					span = inputSpan.Slice(num);
					break;
				case 2:
					return false;
				}
				goto IL_030d;
				IL_0172:
				if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				if (num8 >= num9 || (num9 = inputSpan.Slice(num8, num9 - num8).LastIndexOf('.')) < 0)
				{
					goto IL_00cb;
				}
				num9 += num8;
				num = num9;
				span = inputSpan.Slice(num);
				goto IL_01ba;
				IL_0330:
				while (true)
				{
					if (span.IsEmpty || span[0] != '/')
					{
						if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						switch (num2)
						{
						case 0:
							break;
						case 1:
							goto IL_0172;
						case 2:
							goto IL_02ea;
						default:
							continue;
						}
						break;
					}
					Capture(0, start, runtextpos = num + 1);
					return true;
				}
				goto IL_007f;
				IL_01ba:
				if ((uint)span.Length < 8u || !span.StartsWith(".sprites", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_0172;
				}
				num2 = 1;
				num += 8;
				span = inputSpan.Slice(num);
				goto IL_0330;
				IL_030d:
				num2 = 2;
				goto IL_0330;
				IL_00cb:
				if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				if (num6 < num7 && (num7 = inputSpan.Slice(num6, num7 - num6).LastIndexOfAny('C', 'c')) >= 0)
				{
					num7 += num6;
					num = num7;
					span = inputSpan.Slice(num);
					goto IL_0115;
				}
				num = num4;
				span = inputSpan.Slice(num);
				if ((uint)span.Length < 5u || !span.Slice(1).StartsWith("card", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				num5 = num;
				if ((uint)span.Length > 5u && (span[5] | 0x20) == 115)
				{
					span = span.Slice(1);
					num++;
				}
				num3 = 0;
				num += 5;
				span = inputSpan.Slice(num);
				goto IL_030d;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CardArtPathRegex_8 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CardArtPathRegex_8();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CardArtPathRegex_8()
	{
		pattern = "/(?:card_portraits|[^/]*card[^/]*\\.sprites|cards?|card_art|cardart)/";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

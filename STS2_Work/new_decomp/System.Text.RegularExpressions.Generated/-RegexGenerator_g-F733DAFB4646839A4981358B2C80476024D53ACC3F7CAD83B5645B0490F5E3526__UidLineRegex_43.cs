using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidLineRegex_43 : Regex
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
				if (num <= inputSpan.Length - 14)
				{
					if (num <= 0 || inputSpan[num - 1] == '\n')
					{
						goto IL_005a;
					}
					int num2 = inputSpan.Slice(num).IndexOf('\n');
					if ((uint)num2 <= inputSpan.Length - num - 1)
					{
						num += num2 + 1;
						if (num <= inputSpan.Length - 14)
						{
							goto IL_005a;
						}
					}
				}
				goto IL_007c;
				IL_005a:
				int num3 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_indexOfString_uid_OrdinalIgnoreCase);
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num > 0 && inputSpan[num - 1] != '\n')
				{
					return false;
				}
				if ((uint)span.Length < 11u || !span.StartsWith("uid", StringComparison.OrdinalIgnoreCase) || !span.Slice(3).StartsWith("=\"") || !span.Slice(5).StartsWith("uid://", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				int num2 = span.Slice(11).IndexOf('"');
				if (num2 < 0)
				{
					num2 = span.Length - 11;
				}
				if (num2 == 0)
				{
					return false;
				}
				span = span.Slice(num2);
				num += num2;
				if ((uint)span.Length < 12u || span[11] != '"')
				{
					return false;
				}
				if ((uint)span.Length > 12u && span[12] == '\r')
				{
					span = span.Slice(1);
					num++;
				}
				if ((uint)span.Length < 13u || span[12] != '\n')
				{
					return false;
				}
				Capture(0, start, runtextpos = num + 13);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidLineRegex_43 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidLineRegex_43();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidLineRegex_43()
	{
		pattern = "^uid=\"uid://[^\"]+\"\\r?\\n";
		roptions = RegexOptions.IgnoreCase | RegexOptions.Multiline;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

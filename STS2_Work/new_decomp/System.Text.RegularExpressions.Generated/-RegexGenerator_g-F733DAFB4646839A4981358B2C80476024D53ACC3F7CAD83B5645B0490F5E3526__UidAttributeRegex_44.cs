using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidAttributeRegex_44 : Regex
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
					while (true)
					{
						ReadOnlySpan<char> span = inputSpan.Slice(num);
						int num2 = span.IndexOfAny('U', 'u');
						if (num2 < 0)
						{
							break;
						}
						int num3 = num2 - 1;
						while ((uint)num3 < (uint)span.Length && char.IsWhiteSpace(span[num3]))
						{
							num3--;
						}
						if (num2 - num3 - 1 < 1)
						{
							num += num2 + 1;
							continue;
						}
						runtextpos = num + num3 + 1;
						runtrackpos = num + num2;
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				num = runtrackpos;
				span = inputSpan.Slice(num);
				if (runtextpos < num)
				{
					runtextpos = num;
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
				Capture(0, start, runtextpos = num + 12);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidAttributeRegex_44 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidAttributeRegex_44();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__UidAttributeRegex_44()
	{
		pattern = "\\s+uid=\"uid://[^\"]+\"";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

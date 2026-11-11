using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureFilterClipRegex_27 : Regex
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
				if (num <= inputSpan.Length - 16)
				{
					if (num <= 0 || inputSpan[num - 1] == '\n')
					{
						goto IL_005a;
					}
					int num2 = inputSpan.Slice(num).IndexOf('\n');
					if ((uint)num2 <= inputSpan.Length - num - 1)
					{
						num += num2 + 1;
						if (num <= inputSpan.Length - 16)
						{
							goto IL_005a;
						}
					}
				}
				goto IL_0077;
				IL_005a:
				int num3 = inputSpan.Slice(num).IndexOfNonAsciiOrAny_7091B6DA5C2749D379F79DDE6B55ABE13767EC535D963FA5881B8A7B76916E73();
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num > 0 && inputSpan[num - 1] != '\n')
				{
					return false;
				}
				int i;
				for (i = 0; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
				{
				}
				span = span.Slice(i);
				num += i;
				if ((uint)span.Length < 11u || !span.StartsWith("filter_clip", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				int j;
				for (j = 11; (uint)j < (uint)span.Length && char.IsWhiteSpace(span[j]); j++)
				{
				}
				span = span.Slice(j);
				num += j;
				if (span.IsEmpty || span[0] != '=')
				{
					return false;
				}
				int k;
				for (k = 1; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
				{
				}
				span = span.Slice(k);
				num += k;
				if ((uint)span.Length < 4u || !span.StartsWith("true", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				num += 4;
				span = inputSpan.Slice(num);
				num2 = num;
				int l;
				for (l = 0; (uint)l < (uint)span.Length && char.IsWhiteSpace(span[l]); l++)
				{
				}
				span = span.Slice(l);
				num += l;
				num3 = num;
				while ((uint)num < (uint)inputSpan.Length && inputSpan[num] != '\n')
				{
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num2 >= num3)
					{
						return false;
					}
					num = --num3;
					span = inputSpan.Slice(num);
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureFilterClipRegex_27 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureFilterClipRegex_27();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AtlasTextureFilterClipRegex_27()
	{
		pattern = "(?m)^\\s*filter_clip\\s*=\\s*true\\s*$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

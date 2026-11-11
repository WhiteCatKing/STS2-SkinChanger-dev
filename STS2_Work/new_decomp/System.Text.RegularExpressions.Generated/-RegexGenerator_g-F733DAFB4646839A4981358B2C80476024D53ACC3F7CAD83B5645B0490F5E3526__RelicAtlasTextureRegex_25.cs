using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RelicAtlasTextureRegex_25 : Regex
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
				if (num <= inputSpan.Length - 36 && num == 0)
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
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					return false;
				}
				if ((uint)span.Length < 26u || !span.StartsWith("res://images/atlases/relic", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				num += 26;
				span = inputSpan.Slice(num);
				num2 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.StackPush(ref runstack, ref pos, num);
					num2++;
					if ((uint)span.Length < 8u || !span.StartsWith("_outline", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_0091;
					}
					num += 8;
					span = inputSpan.Slice(num);
					if (num2 == 0)
					{
						continue;
					}
					goto IL_00b2;
					IL_0091:
					if (--num2 < 0)
					{
						return false;
					}
					num = runstack[--pos];
					span = inputSpan.Slice(num);
					goto IL_00b2;
					IL_00b2:
					if ((uint)span.Length >= 10u && span.StartsWith("_atlas.png", StringComparison.OrdinalIgnoreCase) && 11 >= span.Length && (10 >= span.Length || span[10] == '\n'))
					{
						break;
					}
					goto IL_0091;
				}
				Capture(0, start, runtextpos = num + 10);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RelicAtlasTextureRegex_25 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RelicAtlasTextureRegex_25();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RelicAtlasTextureRegex_25()
	{
		pattern = "^res://images/atlases/relic(?:_outline)?_atlas\\.png$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

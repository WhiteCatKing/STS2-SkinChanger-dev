using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RuntimeCharacterIconTemplateRegex_37 : Regex
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
				if (num <= inputSpan.Length - 50 && num == 0)
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 39u || !span.StartsWith("res://custom/scenes/ui/character_icons/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 39;
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
					if ((uint)span.Length >= 10u && span.StartsWith("_icon.tscn", StringComparison.OrdinalIgnoreCase) && 11 >= span.Length && (10 >= span.Length || span[10] == '\n'))
					{
						break;
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
						UncaptureUntil(0);
						return false;
					}
					num++;
					span = inputSpan.Slice(num);
					num4 = num;
				}
				Capture(0, start, runtextpos = num + 10);
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RuntimeCharacterIconTemplateRegex_37 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RuntimeCharacterIconTemplateRegex_37();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RuntimeCharacterIconTemplateRegex_37()
	{
		pattern = "^res://custom/scenes/ui/character_icons/([^/.]+?)_icon\\.tscn$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RestSiteCharacterSceneRegex_17 : Regex
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
				int num5 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 34u || !span.StartsWith("res://scenes/rest_site/characters/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 34;
				span = inputSpan.Slice(num);
				num2 = num;
				num4 = num;
				int num6 = span.IndexOfAny('.', '/');
				if (num6 < 0)
				{
					num6 = span.Length;
				}
				if (num6 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num6);
				num += num6;
				num5 = num;
				num4++;
				while (true)
				{
					num3 = Crawlpos();
					Capture(1, num2, num);
					if ((uint)span.Length >= 15u && span.StartsWith("_rest_site.tscn", StringComparison.OrdinalIgnoreCase) && 16 >= span.Length && (15 >= span.Length || span[15] == '\n'))
					{
						break;
					}
					UncaptureUntil(num3);
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num4 >= num5 || (num5 = inputSpan.Slice(num4, num5 - num4).LastIndexOf('_')) < 0)
					{
						UncaptureUntil(0);
						return false;
					}
					num5 += num4;
					num = num5;
					span = inputSpan.Slice(num);
				}
				Capture(0, start, runtextpos = num + 15);
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RestSiteCharacterSceneRegex_17 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RestSiteCharacterSceneRegex_17();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__RestSiteCharacterSceneRegex_17()
	{
		pattern = "^res://scenes/rest_site/characters/([^/.]+)_rest_site\\.tscn$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

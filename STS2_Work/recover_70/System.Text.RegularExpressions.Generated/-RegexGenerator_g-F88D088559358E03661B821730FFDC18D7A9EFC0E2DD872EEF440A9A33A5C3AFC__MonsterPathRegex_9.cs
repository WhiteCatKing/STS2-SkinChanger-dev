using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MonsterPathRegex_9 : Regex
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
				if (num <= inputSpan.Length - 28 && num == 0)
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 26u || !span.StartsWith("res://animations/monsters/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 26;
				span = inputSpan.Slice(num);
				num2 = num;
				int num3 = span.IndexOf('/');
				if (num3 < 0)
				{
					num3 = span.Length;
				}
				if (num3 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num3);
				num += num3;
				Capture(1, num2, num);
				if (span.IsEmpty || span[0] != '/')
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

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MonsterPathRegex_9 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MonsterPathRegex_9();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MonsterPathRegex_9()
	{
		pattern = "^res://animations/monsters/([^/]+)/";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

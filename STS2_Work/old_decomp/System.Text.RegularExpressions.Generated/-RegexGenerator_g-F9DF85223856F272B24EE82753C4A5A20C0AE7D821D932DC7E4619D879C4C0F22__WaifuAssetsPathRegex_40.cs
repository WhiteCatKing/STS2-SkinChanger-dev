using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__WaifuAssetsPathRegex_40 : Regex
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
				if (num <= inputSpan.Length - 22 && num == 0)
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
				if ((uint)span.Length < 19u || !span.StartsWith("res://waifu_assets/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				int num6 = span.Slice(19).IndexOf('/');
				if (num6 < 0)
				{
					num6 = span.Length - 19;
				}
				if (num6 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num6);
				num += num6;
				if ((uint)span.Length < 20u || span[19] != '/')
				{
					UncaptureUntil(0);
					return false;
				}
				num += 20;
				span = inputSpan.Slice(num);
				num2 = num;
				num4 = num;
				int num7 = span.IndexOf('\n');
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
				num5 = num;
				num4++;
				while (true)
				{
					num3 = Crawlpos();
					Capture(1, num2, num);
					if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
					{
						break;
					}
					UncaptureUntil(num3);
					if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num4 >= num5)
					{
						UncaptureUntil(0);
						return false;
					}
					num = --num5;
					span = inputSpan.Slice(num);
				}
				runtextpos = num;
				Capture(0, start, num);
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

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__WaifuAssetsPathRegex_40 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__WaifuAssetsPathRegex_40();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__WaifuAssetsPathRegex_40()
	{
		pattern = "^res://waifu_assets/[^/]+/(.+)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

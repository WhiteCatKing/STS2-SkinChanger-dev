using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.26416")]
internal sealed class _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__ClientLink_1 : Regex
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
					if (_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 31)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_indexOfString_237F0CBD8E0FEBD202F6C2730CD0C1005DDDB48603A88196DFC6AFE750775153);
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if ((uint)span.Length < 30u || !span.StartsWith("steam://url/communityfilepage/".AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 30;
				span = inputSpan.Slice(num);
				num2 = num;
				int i;
				for (i = 0; i < 20 && (uint)i < (uint)span.Length && char.IsDigit(span[i]); i++)
				{
				}
				if (i == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(i);
				num += i;
				Capture(1, num2, num);
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

	internal static readonly _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__ClientLink_1 Instance = new _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__ClientLink_1();

	private _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__ClientLink_1()
	{
		pattern = "steam://url/CommunityFilePage/(\\d{1,20})";
		roptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF37AB2B4719F7DFC5DAFA77E00BB0A9F9F4FF6EB51F63B69E71CF7F19860B9869__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

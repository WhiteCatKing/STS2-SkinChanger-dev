using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SourcePathRegex_0 : Regex
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
				if (num <= inputSpan.Length - 17)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_indexOfString_67C631CDB34E2886C3AE30BB53722CECED1CF2AD6A5B8816E142CFC17449008B);
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
				if (!span.StartsWith("\"source_path\""))
				{
					UncaptureUntil(0);
					return false;
				}
				int i;
				for (i = 13; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
				{
				}
				span = span.Slice(i);
				num += i;
				if (span.IsEmpty || span[0] != ':')
				{
					UncaptureUntil(0);
					return false;
				}
				int j;
				for (j = 1; (uint)j < (uint)span.Length && char.IsWhiteSpace(span[j]); j++)
				{
				}
				span = span.Slice(j);
				num += j;
				if (span.IsEmpty || span[0] != '"')
				{
					UncaptureUntil(0);
					return false;
				}
				num++;
				span = inputSpan.Slice(num);
				num2 = num;
				int num3 = span.IndexOf('"');
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
				if (span.IsEmpty || span[0] != '"')
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

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SourcePathRegex_0 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SourcePathRegex_0();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__SourcePathRegex_0()
	{
		pattern = "\"source_path\"\\s*:\\s*\"(?<v>[^\"]+)\"";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		base.CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "v", 1 }
		};
		capslist = new string[2] { "0", "v" };
		capsize = 2;
	}
}

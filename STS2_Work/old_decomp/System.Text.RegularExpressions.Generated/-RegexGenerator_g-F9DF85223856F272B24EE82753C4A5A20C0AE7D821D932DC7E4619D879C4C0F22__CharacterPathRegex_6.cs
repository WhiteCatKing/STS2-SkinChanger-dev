using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CharacterPathRegex_6 : Regex
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
				int num3 = 0;
				int num4 = 0;
				int num5 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 17u || !span.StartsWith("res://animations/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num4 = num;
				num3 = Crawlpos();
				if ((uint)span.Length >= 26u && span.Slice(17).StartsWith("character", StringComparison.OrdinalIgnoreCase) && (uint)span.Length >= 27u)
				{
					char c = span[26];
					if (c == 'S')
					{
						goto IL_00ad;
					}
					if (c != '_')
					{
						if (c == 's')
						{
							goto IL_00ad;
						}
					}
					else if ((uint)span.Length >= 33u && span.Slice(27).StartsWith("select", StringComparison.OrdinalIgnoreCase))
					{
						num += 33;
						span = inputSpan.Slice(num);
						goto IL_00f3;
					}
				}
				goto IL_00fa;
				IL_01ca:
				while (true)
				{
					if (!span.IsEmpty && span[0] == '/')
					{
						num++;
						span = inputSpan.Slice(num);
						num5 = num;
						int num6 = span.IndexOf('/');
						if (num6 < 0)
						{
							num6 = span.Length;
						}
						if (num6 != 0)
						{
							span = span.Slice(num6);
							num += num6;
							Capture(1, num5, num);
							if (!span.IsEmpty && span[0] == '/')
							{
								Capture(0, start, runtextpos = num + 1);
								return true;
							}
						}
					}
					if (_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					switch (num2)
					{
					case 0:
						break;
					case 1:
						goto IL_014a;
					case 2:
						UncaptureUntil(0);
						return false;
					default:
						continue;
					}
					break;
				}
				goto IL_00fa;
				IL_00f3:
				num2 = 0;
				goto IL_01ca;
				IL_00fa:
				num = num4;
				span = inputSpan.Slice(num);
				UncaptureUntil(num3);
				if ((uint)span.Length < 25u || !span.Slice(17).StartsWith("merchant", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_014a;
				}
				num2 = 1;
				num += 25;
				span = inputSpan.Slice(num);
				goto IL_01ca;
				IL_014a:
				num = num4;
				span = inputSpan.Slice(num);
				UncaptureUntil(num3);
				if ((uint)span.Length < 26u || !span.Slice(17).StartsWith("rest_site", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num2 = 2;
				num += 26;
				span = inputSpan.Slice(num);
				goto IL_01ca;
				IL_00ad:
				num += 27;
				span = inputSpan.Slice(num);
				goto IL_00f3;
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

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CharacterPathRegex_6 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CharacterPathRegex_6();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__CharacterPathRegex_6()
	{
		pattern = "^res://animations/(?:characters|character_select|merchant|rest_site)/([^/]+)/";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RemapTargetRegex_45 : Regex
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
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
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
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_indexOfString_path_OrdinalIgnoreCase);
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
				int num3 = 0;
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if ((uint)span.Length < 4u || !span.StartsWith("path", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 4;
				span = inputSpan.Slice(num);
				num3 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
					num3++;
					if (!span.IsEmpty && span[0] == '.')
					{
						int num4 = span.Slice(1).IndexOfAnyExcept(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_nonAscii_9FA52D3BAECB644578472387D5284CC6F36F408FEB88A04BA674CE14F24D2386);
						if (num4 < 0)
						{
							num4 = span.Length - 1;
						}
						if (num4 != 0)
						{
							span = span.Slice(num4);
							num += num4;
							num++;
							span = inputSpan.Slice(num);
							if (num3 == 0)
							{
								continue;
							}
							goto IL_0108;
						}
					}
					goto IL_00ca;
					IL_00ca:
					if (--num3 < 0)
					{
						UncaptureUntil(0);
						return false;
					}
					num = runstack[--pos];
					UncaptureUntil(runstack[--pos]);
					span = inputSpan.Slice(num);
					goto IL_0108;
					IL_0108:
					int i;
					for (i = 0; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
					{
					}
					span = span.Slice(i);
					num += i;
					if (!span.IsEmpty && span[0] == '=')
					{
						int j;
						for (j = 1; (uint)j < (uint)span.Length && char.IsWhiteSpace(span[j]); j++)
						{
						}
						span = span.Slice(j);
						num += j;
						if (!span.IsEmpty && span[0] == '"')
						{
							num++;
							span = inputSpan.Slice(num);
							num2 = num;
							if ((uint)span.Length >= 6u && span.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
							{
								int num5 = span.Slice(6).IndexOf('"');
								if (num5 < 0)
								{
									num5 = span.Length - 6;
								}
								if (num5 != 0)
								{
									span = span.Slice(num5);
									num += num5;
									num += 6;
									span = inputSpan.Slice(num);
									Capture(1, num2, num);
									if (!span.IsEmpty && span[0] == '"')
									{
										break;
									}
								}
							}
						}
					}
					goto IL_00ca;
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

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RemapTargetRegex_45 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RemapTargetRegex_45();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RemapTargetRegex_45()
	{
		pattern = "path(?:\\.[a-z0-9_]+)?\\s*=\\s*\"(res://[^\"]+)\"";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

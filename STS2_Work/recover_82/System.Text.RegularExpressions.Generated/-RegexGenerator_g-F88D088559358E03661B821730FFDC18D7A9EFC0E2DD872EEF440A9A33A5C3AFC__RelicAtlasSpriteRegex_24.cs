using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RelicAtlasSpriteRegex_24 : Regex
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
				if (num <= inputSpan.Length - 47 && num == 0)
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
				num4 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.StackPush(ref runstack, ref pos, num);
					num4++;
					if ((uint)span.Length < 8u || !span.StartsWith("_outline", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_009b;
					}
					num += 8;
					span = inputSpan.Slice(num);
					if (num4 == 0)
					{
						continue;
					}
					goto IL_00c0;
					IL_009b:
					if (--num4 < 0)
					{
						break;
					}
					num = runstack[--pos];
					span = inputSpan.Slice(num);
					goto IL_00c0;
					IL_00c0:
					if ((uint)span.Length >= 15u && span.StartsWith("_atlas.sprites/", StringComparison.OrdinalIgnoreCase))
					{
						num += 15;
						span = inputSpan.Slice(num);
						num2 = num;
						int num5 = span.IndexOf('/');
						if (num5 < 0)
						{
							num5 = span.Length;
						}
						if (num5 != 0)
						{
							span = span.Slice(num5);
							num += num5;
							num3 = num;
							num2++;
							while (true)
							{
								if ((uint)span.Length < 5u || !span.StartsWith(".tres", StringComparison.OrdinalIgnoreCase) || 6 < span.Length || (5 < span.Length && span[5] != '\n'))
								{
									if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
									{
										CheckTimeout();
									}
									if (num2 >= num3 || (num3 = inputSpan.Slice(num2, num3 - num2).LastIndexOf('.')) < 0)
									{
										break;
									}
									num3 += num2;
									num = num3;
									span = inputSpan.Slice(num);
									continue;
								}
								Capture(0, start, runtextpos = num + 5);
								return true;
							}
						}
					}
					goto IL_009b;
				}
				return false;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RelicAtlasSpriteRegex_24 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RelicAtlasSpriteRegex_24();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__RelicAtlasSpriteRegex_24()
	{
		pattern = "^res://images/atlases/relic(?:_outline)?_atlas\\.sprites/[^/]+\\.tres$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

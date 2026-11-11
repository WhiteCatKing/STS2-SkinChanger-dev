using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantAppearancePathRegex_10 : Regex
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					return false;
				}
				if ((uint)span.Length < 6u || !span.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				if ((uint)span.Length < 7u)
				{
					return false;
				}
				switch (span[6])
				{
				case 'A':
				case 'a':
					if ((uint)span.Length < 17u || !span.Slice(7).StartsWith("nimations/", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					if ((uint)span.Length < 18u)
					{
						return false;
					}
					switch (span[17])
					{
					case 'B':
					case 'b':
					{
						char c;
						if ((uint)span.Length < 43u || !span.Slice(18).StartsWith("ac", StringComparison.OrdinalIgnoreCase) || (((c = span[20]) | 0x20) != 107 && c != 'K') || !span.Slice(21).StartsWith("grounds/merchant_room/", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						num += 43;
						span = inputSpan.Slice(num);
						break;
					}
					case 'C':
					case 'c':
						if ((uint)span.Length < 34u || !span.Slice(18).StartsWith("ustoms/merchant/", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						num += 34;
						span = inputSpan.Slice(num);
						break;
					default:
						return false;
					}
					break;
				case 'S':
				case 's':
				{
					if ((uint)span.Length < 13u || !span.Slice(7).StartsWith("cenes/", StringComparison.OrdinalIgnoreCase))
					{
						return false;
					}
					int num5 = num4;
					if ((uint)span.Length < 14u)
					{
						return false;
					}
					switch (span[13])
					{
					case 'R':
					case 'r':
						if ((uint)span.Length < 39u || !span.Slice(14).StartsWith("ooms/merchant_button.tscn", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						if (40 < span.Length || (39 < span.Length && span[39] != '\n'))
						{
							return false;
						}
						num += 39;
						span = inputSpan.Slice(num);
						break;
					case 'M':
					case 'm':
					{
						if ((uint)span.Length < 22u || !span.Slice(14).StartsWith("erchant/", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						int num6 = num;
						if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if ((uint)span.Length >= 33u && span.Slice(22).StartsWith("characters/", StringComparison.OrdinalIgnoreCase))
						{
							return false;
						}
						num = num6;
						span = inputSpan.Slice(num);
						num += 22;
						span = inputSpan.Slice(num);
						num2 = num;
						int num7 = span.IndexOf('/');
						if (num7 < 0)
						{
							num7 = span.Length;
						}
						if (num7 == 0)
						{
							return false;
						}
						span = span.Slice(num7);
						num += num7;
						num3 = num;
						num2++;
						while ((uint)span.Length < 5u || !span.StartsWith(".tscn", StringComparison.OrdinalIgnoreCase) || 6 < span.Length || (5 < span.Length && span[5] != '\n'))
						{
							if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
							{
								CheckTimeout();
							}
							if (num2 >= num3 || (num3 = inputSpan.Slice(num2, num3 - num2).LastIndexOf('.')) < 0)
							{
								return false;
							}
							num3 += num2;
							num = num3;
							span = inputSpan.Slice(num);
						}
						num += 5;
						span = inputSpan.Slice(num);
						break;
					}
					default:
						return false;
					}
					num4 = num5;
					break;
				}
				default:
					return false;
				}
				runtextpos = num;
				Capture(0, start, num);
				return true;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantAppearancePathRegex_10 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantAppearancePathRegex_10();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MerchantAppearancePathRegex_10()
	{
		pattern = "^(?:res://animations/backgrounds/merchant_room/|res://animations/customs/merchant/|res://scenes/rooms/merchant_button\\.tscn$|res://scenes/merchant/(?!characters/)[^/]+\\.tscn$)";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

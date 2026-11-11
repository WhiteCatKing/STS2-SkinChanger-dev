using System.CodeDom.Compiler;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MultiplayerHandRegex_39 : Regex
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
				if (num <= inputSpan.Length - 49 && num == 0)
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
				int num6 = 0;
				int num7 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 39u || !span.StartsWith("res://images/ui/hands/multiplayer_hand_", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 39;
				span = inputSpan.Slice(num);
				num5 = num;
				if (span.IsEmpty || char.IsBetween(span[0], '.', '/'))
				{
					UncaptureUntil(0);
					return false;
				}
				num++;
				span = inputSpan.Slice(num);
				num7 = num;
				while (true)
				{
					num6 = Crawlpos();
					Capture(1, num5, num);
					if (span.IsEmpty || span[0] != '_')
					{
						goto IL_00a3;
					}
					num4 = num;
					num3 = Crawlpos();
					if ((uint)span.Length >= 2u && (span[1] | 0x20) == 112 && (uint)span.Length >= 3u)
					{
						char c = span[2];
						if ((uint)c <= 79u)
						{
							if (c == 'A')
							{
								goto IL_018c;
							}
							if (c == 'O')
							{
								goto IL_01c0;
							}
						}
						else
						{
							if (c == 'a')
							{
								goto IL_018c;
							}
							if (c == 'o')
							{
								goto IL_01c0;
							}
						}
					}
					goto IL_01f9;
					IL_02d9:
					while (true)
					{
						if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
						{
							char c = span[1];
							if ((uint)c <= 84u)
							{
								if (c == 'P')
								{
									goto IL_0326;
								}
								if (c != 'T')
								{
									goto IL_02ba;
								}
							}
							else
							{
								if (c == 'p')
								{
									goto IL_0326;
								}
								if (c != 't')
								{
									goto IL_02ba;
								}
							}
							if ((uint)span.Length >= 5u && span.Slice(2).StartsWith("res", StringComparison.OrdinalIgnoreCase))
							{
								num += 5;
								span = inputSpan.Slice(num);
								goto IL_0395;
							}
						}
						goto IL_02ba;
						IL_0326:
						if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
						{
							goto IL_02ba;
						}
						num += 4;
						span = inputSpan.Slice(num);
						goto IL_0395;
						IL_02ba:
						if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						switch (num2)
						{
						case 2:
							break;
						case 0:
							goto IL_01f9;
						case 1:
							goto IL_0267;
						default:
							continue;
						}
						break;
						IL_0395:
						if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
						{
							runtextpos = num;
							Capture(0, start, num);
							return true;
						}
						goto IL_02ba;
					}
					goto IL_00a3;
					IL_01f2:
					num2 = 0;
					goto IL_02d9;
					IL_01c0:
					if ((uint)span.Length >= 6u && span.Slice(3).StartsWith("int", StringComparison.OrdinalIgnoreCase))
					{
						num += 6;
						span = inputSpan.Slice(num);
						goto IL_01f2;
					}
					goto IL_01f9;
					IL_01f9:
					num = num4;
					span = inputSpan.Slice(num);
					UncaptureUntil(num3);
					char c2;
					if ((uint)span.Length < 5u || !span.Slice(1).StartsWith("roc", StringComparison.OrdinalIgnoreCase) || (((c2 = span[4]) | 0x20) != 107 && c2 != 'K'))
					{
						goto IL_0267;
					}
					num2 = 1;
					num += 5;
					span = inputSpan.Slice(num);
					goto IL_02d9;
					IL_0267:
					num = num4;
					span = inputSpan.Slice(num);
					UncaptureUntil(num3);
					if ((uint)span.Length < 9u || !span.Slice(1).StartsWith("scissors", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_00a3;
					}
					num2 = 2;
					num += 9;
					span = inputSpan.Slice(num);
					goto IL_02d9;
					IL_018c:
					if ((uint)span.Length >= 6u && span.Slice(3).StartsWith("per", StringComparison.OrdinalIgnoreCase))
					{
						num += 6;
						span = inputSpan.Slice(num);
						goto IL_01f2;
					}
					goto IL_01f9;
					IL_00a3:
					UncaptureUntil(num6);
					if (_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					num = num7;
					span = inputSpan.Slice(num);
					if (span.IsEmpty || char.IsBetween(span[0], '.', '/'))
					{
						break;
					}
					num++;
					span = inputSpan.Slice(num);
					num7 = num;
				}
				UncaptureUntil(0);
				return false;
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

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MultiplayerHandRegex_39 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MultiplayerHandRegex_39();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__MultiplayerHandRegex_39()
	{
		pattern = "^res://images/ui/hands/multiplayer_hand_([^/.]+?)_(?:paper|point|rock|scissors)\\.(?:png|tres)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 2;
	}
}

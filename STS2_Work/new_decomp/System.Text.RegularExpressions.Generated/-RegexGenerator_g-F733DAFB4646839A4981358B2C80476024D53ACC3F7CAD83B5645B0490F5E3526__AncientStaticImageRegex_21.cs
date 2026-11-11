using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AncientStaticImageRegex_21 : Regex
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
				if (num <= inputSpan.Length - 18 && num == 0)
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
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					UncaptureUntil(0);
					return false;
				}
				if ((uint)span.Length < 13u || !span.StartsWith("res://images/", StringComparison.OrdinalIgnoreCase))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 13;
				span = inputSpan.Slice(num);
				num5 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
					num5++;
					if ((uint)span.Length < 9u || !span.StartsWith("ancients/", StringComparison.OrdinalIgnoreCase))
					{
						goto IL_00b7;
					}
					num += 9;
					span = inputSpan.Slice(num);
					if (num5 == 0)
					{
						continue;
					}
					goto IL_00f7;
					IL_00b7:
					if (--num5 < 0)
					{
						break;
					}
					num = runstack[--pos];
					UncaptureUntil(runstack[--pos]);
					span = inputSpan.Slice(num);
					goto IL_00f7;
					IL_00f7:
					num2 = num;
					if (!span.IsEmpty && !char.IsBetween(span[0], '.', '/'))
					{
						num++;
						span = inputSpan.Slice(num);
						num4 = num;
						while (true)
						{
							num3 = Crawlpos();
							Capture(1, num2, num);
							num6 = 0;
							while (true)
							{
								_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.StackPush(ref runstack, ref pos, Crawlpos(), num);
								num6++;
								if ((uint)span.Length < 12u || !span.StartsWith("_placeholder", StringComparison.OrdinalIgnoreCase))
								{
									goto IL_01df;
								}
								num += 12;
								span = inputSpan.Slice(num);
								if (num6 == 0)
								{
									continue;
								}
								goto IL_0219;
								IL_0398:
								if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("vg", StringComparison.OrdinalIgnoreCase))
								{
									goto IL_01df;
								}
								num += 4;
								span = inputSpan.Slice(num);
								goto IL_03d0;
								IL_0320:
								if ((uint)span.Length >= 3u && (span[2] | 0x20) == 112)
								{
									if ((uint)span.Length > 3u && (span[3] | 0x20) == 101)
									{
										span = span.Slice(1);
										num++;
									}
									if ((uint)span.Length >= 4u && (span[3] | 0x20) == 103)
									{
										num += 4;
										span = inputSpan.Slice(num);
										goto IL_03d0;
									}
								}
								goto IL_01df;
								IL_01df:
								if (--num6 < 0)
								{
									break;
								}
								num = runstack[--pos];
								UncaptureUntil(runstack[--pos]);
								span = inputSpan.Slice(num);
								goto IL_0219;
								IL_03d0:
								if (num >= inputSpan.Length - 1 && ((uint)num >= (uint)inputSpan.Length || inputSpan[num] == '\n'))
								{
									runtextpos = num;
									Capture(0, start, num);
									return true;
								}
								goto IL_01df;
								IL_0219:
								if (!span.IsEmpty && span[0] == '.' && (uint)span.Length >= 2u)
								{
									char c = span[1];
									if ((uint)c <= 87u)
									{
										if ((uint)c <= 80u)
										{
											if (c == 'J')
											{
												goto IL_0320;
											}
											if (c == 'P')
											{
												goto IL_02a6;
											}
										}
										else
										{
											if (c == 'S')
											{
												goto IL_0398;
											}
											if (c == 'W')
											{
												goto IL_02e3;
											}
										}
									}
									else if ((uint)c <= 112u)
									{
										if (c == 'j')
										{
											goto IL_0320;
										}
										if (c == 'p')
										{
											goto IL_02a6;
										}
									}
									else
									{
										if (c == 's')
										{
											goto IL_0398;
										}
										if (c == 'w')
										{
											goto IL_02e3;
										}
									}
								}
								goto IL_01df;
								IL_02e3:
								if ((uint)span.Length < 5u || !span.Slice(2).StartsWith("ebp", StringComparison.OrdinalIgnoreCase))
								{
									goto IL_01df;
								}
								num += 5;
								span = inputSpan.Slice(num);
								goto IL_03d0;
								IL_02a6:
								if ((uint)span.Length < 4u || !span.Slice(2).StartsWith("ng", StringComparison.OrdinalIgnoreCase))
								{
									goto IL_01df;
								}
								num += 4;
								span = inputSpan.Slice(num);
								goto IL_03d0;
							}
							UncaptureUntil(num3);
							if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
							{
								CheckTimeout();
							}
							num = num4;
							span = inputSpan.Slice(num);
							if (span.IsEmpty || char.IsBetween(span[0], '.', '/'))
							{
								break;
							}
							num++;
							span = inputSpan.Slice(num);
							num4 = num;
						}
					}
					goto IL_00b7;
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

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AncientStaticImageRegex_21 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AncientStaticImageRegex_21();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__AncientStaticImageRegex_21()
	{
		pattern = "^res://images/(?:ancients/)?(?<id>[^/.]+?)(?:_placeholder)?\\.(?:png|webp|jpe?g|svg)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		base.CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "id", 1 }
		};
		capslist = new string[2] { "0", "id" };
		capsize = 2;
	}
}

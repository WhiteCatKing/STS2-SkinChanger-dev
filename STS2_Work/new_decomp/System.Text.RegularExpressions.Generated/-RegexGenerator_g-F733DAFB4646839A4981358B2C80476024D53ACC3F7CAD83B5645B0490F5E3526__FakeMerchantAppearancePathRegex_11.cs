using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__FakeMerchantAppearancePathRegex_11 : Regex
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
				if (num <= inputSpan.Length - 45 && num == 0)
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
				int num8 = 0;
				int num9 = 0;
				int num10 = 0;
				int pos = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (num != 0)
				{
					return false;
				}
				if ((uint)span.Length < 6u || !span.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				num4 = num;
				char c;
				if ((uint)span.Length >= 48u && span.Slice(6).StartsWith("animations/bac", StringComparison.OrdinalIgnoreCase) && !(((c = span[20]) | 0x20) != 107 && c != 'K') && span.Slice(21).StartsWith("grounds/fa", StringComparison.OrdinalIgnoreCase) && !(((c = span[31]) | 0x20) != 107 && c != 'K') && span.Slice(32).StartsWith("e_merchant_room/", StringComparison.OrdinalIgnoreCase) && (uint)span.Length >= 49u)
				{
					char c2 = span[48];
					if ((uint)c2 <= 84u)
					{
						if (c2 == 'H')
						{
							goto IL_018a;
						}
						if (c2 == 'T')
						{
							goto IL_014d;
						}
					}
					else
					{
						if (c2 == 'h')
						{
							goto IL_018a;
						}
						if (c2 == 't')
						{
							goto IL_014d;
						}
					}
				}
				goto IL_0246;
				IL_018a:
				if ((uint)span.Length >= 52u && span.Slice(49).StartsWith("and", StringComparison.OrdinalIgnoreCase))
				{
					num += 52;
					span = inputSpan.Slice(num);
					goto IL_01c5;
				}
				goto IL_0246;
				IL_0554:
				if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				if (num3 == 0)
				{
					if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
					if (num8 < num9)
					{
						num = --num9;
						span = inputSpan.Slice(num);
						goto IL_0395;
					}
					goto IL_039d;
				}
				if (num3 == 1)
				{
					goto IL_04fe;
				}
				goto IL_056d;
				IL_056d:
				num2 = 1;
				goto IL_0588;
				IL_014d:
				if ((uint)span.Length >= 51u && span.Slice(49).StartsWith("op", StringComparison.OrdinalIgnoreCase))
				{
					num += 51;
					span = inputSpan.Slice(num);
					goto IL_01c5;
				}
				goto IL_0246;
				IL_039d:
				num = num5;
				span = inputSpan.Slice(num);
				if ((uint)span.Length < 40u || !span.Slice(13).StartsWith("events/custom/fa", StringComparison.OrdinalIgnoreCase) || (((c = span[29]) | 0x20) != 107 && c != 'K') || !span.Slice(30).StartsWith("e_merchant", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				num += 40;
				span = inputSpan.Slice(num);
				num10 = 0;
				while (true)
				{
					_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.StackPush(ref runstack, ref pos, num);
					num10++;
					if (span.IsEmpty || span[0] != '_' || (uint)span.Length < 2u)
					{
						break;
					}
					switch (span[1])
					{
					case 'B':
					case 'b':
						if ((uint)span.Length < 7u || !span.Slice(2).StartsWith("utton", StringComparison.OrdinalIgnoreCase))
						{
							break;
						}
						num += 7;
						span = inputSpan.Slice(num);
						goto IL_04f5;
					case 'I':
					case 'i':
						if ((uint)span.Length < 10u || !span.Slice(2).StartsWith("nventory", StringComparison.OrdinalIgnoreCase))
						{
							break;
						}
						num += 10;
						span = inputSpan.Slice(num);
						goto IL_04f5;
					}
					break;
					IL_04f5:
					if (num10 == 0)
					{
						continue;
					}
					goto IL_0523;
				}
				goto IL_04fe;
				IL_0395:
				num3 = 0;
				goto IL_056d;
				IL_01c5:
				if (!span.IsEmpty && span[0] == '/')
				{
					num++;
					span = inputSpan.Slice(num);
					num6 = num;
					int num11 = span.IndexOf('\n');
					if (num11 < 0)
					{
						num11 = span.Length;
					}
					span = span.Slice(num11);
					num += num11;
					num7 = num;
					goto IL_023f;
				}
				goto IL_0246;
				IL_0523:
				if ((uint)span.Length < 5u || !span.StartsWith(".tscn", StringComparison.OrdinalIgnoreCase))
				{
					goto IL_04fe;
				}
				num3 = 1;
				num += 5;
				span = inputSpan.Slice(num);
				goto IL_056d;
				IL_04fe:
				if (--num10 < 0)
				{
					return false;
				}
				num = runstack[--pos];
				span = inputSpan.Slice(num);
				goto IL_0523;
				IL_023f:
				num2 = 0;
				goto IL_0588;
				IL_0246:
				num = num4;
				span = inputSpan.Slice(num);
				if ((uint)span.Length < 13u || !span.Slice(6).StartsWith("scenes/", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				num5 = num;
				if ((uint)span.Length >= 55u && span.Slice(13).StartsWith("bac", StringComparison.OrdinalIgnoreCase) && !(((c = span[16]) | 0x20) != 107 && c != 'K') && span.Slice(17).StartsWith("grounds/fa", StringComparison.OrdinalIgnoreCase) && !(((c = span[27]) | 0x20) != 107 && c != 'K') && span.Slice(28).StartsWith("e_merchant_event_encounter/", StringComparison.OrdinalIgnoreCase))
				{
					num += 55;
					span = inputSpan.Slice(num);
					num8 = num;
					int num12 = span.IndexOf('\n');
					if (num12 < 0)
					{
						num12 = span.Length;
					}
					span = span.Slice(num12);
					num += num12;
					num9 = num;
					goto IL_0395;
				}
				goto IL_039d;
				IL_0588:
				while (true)
				{
					if (num < inputSpan.Length - 1 || ((uint)num < (uint)inputSpan.Length && inputSpan[num] != '\n'))
					{
						if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
						{
							CheckTimeout();
						}
						if (num2 == 0)
						{
							break;
						}
						if (num2 != 1)
						{
							continue;
						}
						goto IL_0554;
					}
					runtextpos = num;
					Capture(0, start, num);
					return true;
				}
				if (_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_hasTimeout)
				{
					CheckTimeout();
				}
				if (num6 < num7)
				{
					num = --num7;
					span = inputSpan.Slice(num);
					goto IL_023f;
				}
				goto IL_0246;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__FakeMerchantAppearancePathRegex_11 Instance = new _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__FakeMerchantAppearancePathRegex_11();

	private _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__FakeMerchantAppearancePathRegex_11()
	{
		pattern = "^(?:res://animations/backgrounds/fake_merchant_room/(?:top|hand)/.*|res://scenes/backgrounds/fake_merchant_event_encounter/.*|res://scenes/events/custom/fake_merchant(?:_button|_inventory)?\\.tscn)$";
		roptions = RegexOptions.IgnoreCase;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

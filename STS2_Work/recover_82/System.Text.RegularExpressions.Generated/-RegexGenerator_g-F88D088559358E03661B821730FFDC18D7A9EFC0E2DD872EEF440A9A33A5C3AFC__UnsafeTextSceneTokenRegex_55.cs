using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
internal sealed class _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__UnsafeTextSceneTokenRegex_55 : Regex
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
				if (num <= inputSpan.Length - 5)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_indexOfAnyStrings_OrdinalIgnoreCase_8CFD3E39BB63A31595D8A2B2F2BF99B948EA97CAE7DB2FC39B80B682682AFFA0);
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
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				int num2 = 0;
				int num3 = num;
				if ((uint)span.Length >= 11u && span.StartsWith("gdextension", StringComparison.OrdinalIgnoreCase))
				{
					num += 11;
					span = inputSpan.Slice(num);
				}
				else
				{
					num = num3;
					span = inputSpan.Slice(num);
					if ((uint)span.Length < 16u || !span.StartsWith("extensionlibrary", StringComparison.OrdinalIgnoreCase))
					{
						num = num3;
						span = inputSpan.Slice(num);
						if ((uint)span.Length >= 4u && span.StartsWith("type", StringComparison.OrdinalIgnoreCase))
						{
							int i;
							for (i = 4; (uint)i < (uint)span.Length && char.IsWhiteSpace(span[i]); i++)
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
								if (!span.IsEmpty && span[0] == '"' && (uint)span.Length >= 2u)
								{
									char c = span[1];
									if ((uint)c <= 83u)
									{
										if (c == 'C')
										{
											goto IL_01e2;
										}
										if (c == 'G')
										{
											goto IL_01a7;
										}
										if (c == 'S')
										{
											goto IL_021a;
										}
									}
									else
									{
										if (c == 'c')
										{
											goto IL_01e2;
										}
										if (c == 'g')
										{
											goto IL_01a7;
										}
										if (c == 's')
										{
											goto IL_021a;
										}
									}
								}
							}
						}
						goto IL_0273;
					}
					num += 16;
					span = inputSpan.Slice(num);
				}
				goto IL_0397;
				IL_0397:
				runtextpos = num;
				Capture(0, start, num);
				return true;
				IL_021a:
				if ((uint)span.Length >= 7u && span.Slice(2).StartsWith("hader", StringComparison.OrdinalIgnoreCase))
				{
					num += 7;
					span = inputSpan.Slice(num);
					goto IL_024b;
				}
				goto IL_0273;
				IL_01a7:
				if ((uint)span.Length >= 9u && span.Slice(2).StartsWith("dscript", StringComparison.OrdinalIgnoreCase))
				{
					num += 9;
					span = inputSpan.Slice(num);
					goto IL_024b;
				}
				goto IL_0273;
				IL_024b:
				if (span.IsEmpty || span[0] != '"')
				{
					goto IL_0273;
				}
				num++;
				span = inputSpan.Slice(num);
				goto IL_0397;
				IL_01e2:
				if ((uint)span.Length >= 13u && span.Slice(2).StartsWith("sharpscript", StringComparison.OrdinalIgnoreCase))
				{
					num += 13;
					span = inputSpan.Slice(num);
					goto IL_024b;
				}
				goto IL_0273;
				IL_0273:
				num = num3;
				span = inputSpan.Slice(num);
				if ((uint)span.Length >= 11u && span.StartsWith("shader/code", StringComparison.OrdinalIgnoreCase))
				{
					int k;
					for (k = 11; (uint)k < (uint)span.Length && char.IsWhiteSpace(span[k]); k++)
					{
					}
					span = span.Slice(k);
					num += k;
					if (!span.IsEmpty && span[0] == '=')
					{
						num++;
						span = inputSpan.Slice(num);
						goto IL_0397;
					}
				}
				num = num3;
				span = inputSpan.Slice(num);
				if (num > 0 && inputSpan[num - 1] != '\n')
				{
					return false;
				}
				if ((uint)span.Length < 4u || !span.StartsWith("code", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
				int l;
				for (l = 4; (uint)l < (uint)span.Length && char.IsWhiteSpace(span[l]); l++)
				{
				}
				span = span.Slice(l);
				num += l;
				if (span.IsEmpty || span[0] != '=')
				{
					return false;
				}
				num++;
				span = inputSpan.Slice(num);
				goto IL_0397;
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__UnsafeTextSceneTokenRegex_55 Instance = new _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__UnsafeTextSceneTokenRegex_55();

	private _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__UnsafeTextSceneTokenRegex_55()
	{
		pattern = "(?i)(?:GDExtension|ExtensionLibrary|type\\s*=\\s*\\\"(?:GDScript|CSharpScript|Shader)\\\"|shader/code\\s*=|(?m)^code\\s*=)";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF88D088559358E03661B821730FFDC18D7A9EFC0E2DD872EEF440A9A33A5C3AFC__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

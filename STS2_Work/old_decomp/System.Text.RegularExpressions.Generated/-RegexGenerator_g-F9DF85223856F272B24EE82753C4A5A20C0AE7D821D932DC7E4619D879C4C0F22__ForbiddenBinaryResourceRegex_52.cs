using System.CodeDom.Compiler;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.36724")]
internal sealed class _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__ForbiddenBinaryResourceRegex_52 : Regex
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
				if (num <= inputSpan.Length - 8)
				{
					int num2 = inputSpan.Slice(num).IndexOfAny(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_indexOfAnyStrings_OrdinalIgnoreCase_B6C7509F82BA11CF45AB3D08E001EEFC7955C5906A8E3BE7E147EAAAB6533794);
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
				int num2 = num;
				if ((uint)span.Length >= 8u && span.StartsWith("gdscript", StringComparison.OrdinalIgnoreCase))
				{
					num += 8;
					span = inputSpan.Slice(num);
				}
				else
				{
					num = num2;
					span = inputSpan.Slice(num);
					if ((uint)span.Length >= 12u && span.StartsWith("csharpscript", StringComparison.OrdinalIgnoreCase))
					{
						num += 12;
						span = inputSpan.Slice(num);
					}
					else
					{
						num = num2;
						span = inputSpan.Slice(num);
						if ((uint)span.Length >= 11u && span.StartsWith("gdextension", StringComparison.OrdinalIgnoreCase))
						{
							num += 11;
							span = inputSpan.Slice(num);
						}
						else
						{
							num = num2;
							span = inputSpan.Slice(num);
							if ((uint)span.Length >= 16u && span.StartsWith("extensionlibrary", StringComparison.OrdinalIgnoreCase))
							{
								num += 16;
								span = inputSpan.Slice(num);
							}
							else
							{
								num = num2;
								span = inputSpan.Slice(num);
								if ((uint)span.Length < 15u || !span.StartsWith("local://shader_", StringComparison.OrdinalIgnoreCase))
								{
									return false;
								}
								num += 15;
								span = inputSpan.Slice(num);
							}
						}
					}
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

	internal static readonly _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__ForbiddenBinaryResourceRegex_52 Instance = new _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__ForbiddenBinaryResourceRegex_52();

	private _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__ForbiddenBinaryResourceRegex_52()
	{
		pattern = "(?i)(?:GDScript|CSharpScript|GDExtension|ExtensionLibrary|local://Shader_)";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF9DF85223856F272B24EE82753C4A5A20C0AE7D821D932DC7E4619D879C4C0F22__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		capsize = 1;
	}
}

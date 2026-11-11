using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace CznCore.Presentation;

internal sealed record CharacterCurseCardCueProfile(IReadOnlyList<string> VoicePaths, string? SfxPath = null, string? CutInStateId = null, float SfxVolume = 1f)
{
	public static CharacterCurseCardCueProfile Empty { get; } = new CharacterCurseCardCueProfile(Array.Empty<string>());

	public bool HasAnyCue
	{
		get
		{
			if (VoicePaths.Count <= 0 && string.IsNullOrWhiteSpace(SfxPath))
			{
				return !string.IsNullOrWhiteSpace(CutInStateId);
			}
			return true;
		}
	}

	[CompilerGenerated]
	private CharacterCurseCardCueProfile(CharacterCurseCardCueProfile original)
	{
		VoicePaths = original.VoicePaths;
		SfxPath = original.SfxPath;
		CutInStateId = original.CutInStateId;
		SfxVolume = original.SfxVolume;
	}
}

using System;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace CznCore.Presentation;

internal sealed class NativeShieldBreakSfxRequest(Creature target)
{
	public Creature Target { get; } = target;

	public bool Replaced { get; private set; }

	public void TryReplace(Func<bool> play)
	{
		if (!Replaced)
		{
			Replaced = play();
		}
	}
}

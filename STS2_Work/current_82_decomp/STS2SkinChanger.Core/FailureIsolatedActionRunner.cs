using System;
using System.Collections.Generic;

namespace STS2SkinChanger.Core;

internal static class FailureIsolatedActionRunner
{
	internal static IReadOnlyList<ActionFailure> Run(IEnumerable<(string Stage, Action Action)> stages)
	{
		ArgumentNullException.ThrowIfNull(stages, "stages");
		List<ActionFailure> list = new List<ActionFailure>();
		foreach (var (stage, action) in stages)
		{
			try
			{
				action();
			}
			catch (Exception exception)
			{
				list.Add(new ActionFailure(stage, exception));
			}
		}
		return list;
	}
}

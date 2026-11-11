using System;

namespace ModLaunchManager;

internal sealed class LocalModImportException(LocalModImportProblem problem, string detail = "") : Exception((detail.Length == 0) ? problem.ToString() : $"{problem}: {detail}")
{
	internal LocalModImportProblem Problem { get; } = problem;

	internal string Detail { get; } = detail;
}

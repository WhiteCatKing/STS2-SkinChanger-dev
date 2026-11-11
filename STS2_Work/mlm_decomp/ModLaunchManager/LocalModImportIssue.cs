namespace ModLaunchManager;

internal sealed record LocalModImportIssue(string Label, LocalModImportProblem Problem, string Detail = "");

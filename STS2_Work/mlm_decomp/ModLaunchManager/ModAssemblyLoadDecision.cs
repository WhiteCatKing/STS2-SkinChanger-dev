namespace ModLaunchManager;

internal readonly record struct ModAssemblyLoadDecision(bool UseGameContext, string Reason);

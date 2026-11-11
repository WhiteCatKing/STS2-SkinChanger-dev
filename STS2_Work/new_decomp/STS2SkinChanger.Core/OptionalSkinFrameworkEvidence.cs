namespace STS2SkinChanger.Core;

internal sealed record OptionalSkinFrameworkEvidence(string DependentModId, string DependencyId, string ReferencedAssemblyName, bool HasDeclarativeSkinContract, bool ResourceClosureComplete);

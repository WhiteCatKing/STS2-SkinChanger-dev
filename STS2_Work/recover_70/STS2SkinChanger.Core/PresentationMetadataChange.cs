namespace STS2SkinChanger.Core;

internal sealed record PresentationMetadataChange<T>(string Key, bool HadOriginal, T? Original, bool HasApplied, T? Applied);

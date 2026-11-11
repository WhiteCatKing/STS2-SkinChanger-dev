namespace STS2SkinChanger.Core;

internal sealed record WorkshopCodeIssue(string Key, ulong Id, string Name, string ActualName, WorkshopCodeError Error, string Group, int Total, int[] Missing, WorkshopCodeSource[] Sources);

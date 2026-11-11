namespace STS2SkinChanger.Catalog;

internal sealed record ManualCharacterState(string TypeName, string FieldName, bool IsStatic, string ToggleTypeName, string ToggleMethod, string? SaveMethod, string[] SourceSlots)
{
	public string Id => TypeName + ":" + FieldName;
}

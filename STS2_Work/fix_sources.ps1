$ErrorActionPreference = 'Stop'
$dst = $args[1]
$src = $args[0]
$c = [IO.File]::ReadAllText($src)
$c = $c -replace 'internal static class ContextualSkinControls', 'internal static partial class ContextualSkinControls'
$c = [regex]::Replace($c, '\(ref ([A-Za-z_][A-Za-z0-9_]*)\)', '($1)')
$c = $c -replace 'StringName\.op_Implicit\(', 'new StringName('
$c = $c -replace 'NodePath\.op_Implicit\(', 'new NodePath('
$c = $c -replace 'Variant\.op_Implicit\(', 'Variant.From('
$c = [regex]::Replace($c, 'Vector2I\.op_Implicit\(([^)]+)\)', '(Vector2)($1)')
$c = $c -replace '\bEnvironment\.', 'System.Environment.'
$c = [regex]::Replace($c, '\bGrowDirection\b', 'Control.GrowDirection')
$c = $c -replace '\bInternalMode\b', 'Node.InternalMode'
$c = $c -replace '\bMouseFilterEnum\b', 'Control.MouseFilterEnum'
$c = $c -replace '\bSizeFlags\b', 'Control.SizeFlags'
$c = $c -replace '\bLayoutPresetMode\b', 'Control.LayoutPresetMode'
$c = $c -replace '\bLayoutPreset\b', 'Control.LayoutPreset'
$c = $c -replace '\bCursorShape\b', 'Control.CursorShape'
$c = $c -replace '\bFocusModeEnum\b', 'Control.FocusModeEnum'
$c = $c -replace '\bGenEditState\b', 'PackedScene.GenEditState'
$c = $c -replace '\bCacheMode\b', 'ResourceLoader.CacheMode'
$c = $c -replace '\bSelectModeEnum\b', 'ItemList.SelectModeEnum'
$c = $c -replace '\bOverrunBehavior\b', 'TextServer.OverrunBehavior'
$c = $c -replace '\(ToggledEventHandler\)', '(BaseButton.ToggledEventHandler)'
$c = $c -replace '\(ValueChangedEventHandler\)', '(Godot.Range.ValueChangedEventHandler)'
$c = $c -replace '\(ItemSelectedEventHandler\)', '(OptionButton.ItemSelectedEventHandler)'
$c = $c -replace '\(ItemClickedEventHandler\)', '(ItemList.ItemClickedEventHandler)'
$c = $c -replace '\(ItemActivatedEventHandler\)', '(ItemList.ItemActivatedEventHandler)'
$c = $c -replace '\(GuiInputEventHandler\)', '(Control.GuiInputEventHandler)'
$c = [regex]::Replace($c, '\(Range\)', '(Godot.Range)')
$c = [regex]::Replace($c, '\bnew Range\(', 'new Godot.Range(')
$c = [regex]::Replace($c, '\(AlignmentMode\)\d', '(HorizontalAlignment)1')
$c = [regex]::Replace($c, '(OffsetBottom = -92f,\s*\r?\n\s*Alignment = )\(HorizontalAlignment\)1', '${1}(BoxContainer.AlignmentMode)0')

$pattern = '(?s)\[GeneratedRegex\("[^"]*"\)\]\s*\[GeneratedCode\([^\]]*\)\]\s*private static Regex NonAlphanumericRegex\(\)\s*\{.*?\}'
$replacement = @'
private static Regex NonAlphanumericRegex()
	{
		return NonAlphanumericRegexInstance;
	}

	private static readonly Regex NonAlphanumericRegexInstance = new("[^a-zA-Z0-9]", RegexOptions.Compiled);
'@
$c = [regex]::Replace($c, $pattern, $replacement)
$helper = @'

internal sealed class _003C_003Ez__ReadOnlySingleElementList<T> : System.Collections.Generic.List<T>
{
	public _003C_003Ez__ReadOnlySingleElementList(T item)
		: base(1)
	{
		Add(item);
	}
}
'@
$c = $c.TrimEnd() + $helper + "`r`n"
[IO.File]::WriteAllText($dst, $c)
Write-Output ("written length=" + $c.Length)

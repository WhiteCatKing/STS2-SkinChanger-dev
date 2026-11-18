using Godot;

[assembly: AssemblyHasScripts([
    typeof(ScriptPathFixture.FirstScript),
    typeof(ScriptPathFixture.SecondScript)
])]

namespace ScriptPathFixture;

/// <summary>
/// Stands in for a complete skin pack that ships C# scenes: the manager registers these declared
/// paths before the pack's own initializer scans the same assembly again. Godot keys the paths by
/// resource path only, so the fixture is what proves a repeat cannot abort a visual session.
/// </summary>
[ScriptPath("res://sc-script-path-fixture/first.cs")]
public partial class FirstScript : Control;

[ScriptPath("res://sc-script-path-fixture/second.cs")]
public partial class SecondScript : Control;

// Loaded by the runtime tests after the guard is registered.
internal static class LateTakeoverFixture
{
    internal static int TakeoverCalls { get; private set; }
    internal static void Takeover() => TakeoverCalls++;
    internal static void Apply(string selection) { }
}

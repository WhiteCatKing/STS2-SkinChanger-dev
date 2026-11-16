using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;

internal static class FoxCharSelectTests
{
    private static Control? seen;
    private static object? profile;
    internal static void Run(string root)
    {
        var assembly = Assembly.LoadFrom(Path.Combine(root, "silentKaguyaHpWear.dll"));
        var contract = CharSelectPackRebindCompat.FoxAttachContract(assembly)
            ?? throw new Exception("Real fox Attach contract missing.");
        var harmony = new Harmony("sc.tests.fox-attach");
        harmony.Patch(contract.Attach, prefix: new HarmonyMethod(typeof(FoxCharSelectTests), nameof(Capture)));
        var background = (Control)RuntimeHelpers.GetUninitializedObject(typeof(Control));
        GC.SuppressFinalize(background);
        try
        {
            if (!CharSelectPackRebindCompat.ReattachFoxBackground(assembly, background) || seen != background ||
                !ReferenceEquals(profile, contract.Profile.GetValue(null))) throw new Exception("Native background/profile dispatch failed.");
            if ((int)profile!.GetType().GetField("MaxStage")!.GetValue(profile)! != 7)
                throw new Exception("The author's seven-stage profile must be retained.");
            if (CharSelectPackRebindCompat.FoxAttachContract(typeof(FoxCharSelectTests).Assembly) != null)
                throw new Exception("Unrelated providers must remain untouched.");
            Console.WriteLine("Real fox reattach passed: selected background, native profile, seven stages, unrelated provider exclusion.");
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }
    private static bool Capture(Control __0, object __1) { seen = __0; profile = __1; return false; }
}

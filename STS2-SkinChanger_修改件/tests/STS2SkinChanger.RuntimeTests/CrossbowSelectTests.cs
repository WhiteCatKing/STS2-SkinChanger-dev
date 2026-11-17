using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class CrossbowSelectTests
{
    private static Node2D _model = null!;
    private static Control _background = null!, _catcher = null!;
    private static int _creates, _fits;
    internal static void Run(string directory)
    {
        var assembly = Assembly.LoadFrom(Path.Combine(directory, "CrossbowFool.dll"));
        var contract = CrossbowFoolSelectCompat.ResolveContract(assembly)
            ?? throw new Exception("Actual CrossbowFool selection contract missing.");
        if (CrossbowFoolSelectCompat.ResolveContract(typeof(EntryPointFixture).Assembly) != null)
            throw new Exception("Unrelated provider accepted.");
        _model = Fake<Node2D>(); _background = Fake<Control>(); _catcher = Fake<Control>();
        var harmony = new Harmony("sc.test.crossbow-select");
        try
        {
            harmony.Patch(contract.Create, prefix: new HarmonyMethod(typeof(CrossbowSelectTests), nameof(Create)));
            harmony.Patch(contract.Fit, prefix: new HarmonyMethod(typeof(CrossbowSelectTests), nameof(Fit)));
            for (var i = 0; i < 200; i++)
            {
                if (!ReferenceEquals(CrossbowFoolSelectCompat.CreateCatcher(contract, _model, _background), _catcher))
                    throw new Exception("Author catcher not retained.");
                CrossbowFoolSelectCompat.FitCatcher(contract, _model, _background, _catcher);
            }
            if (_creates != 200 || _fits != 200) throw new Exception("Author dispatch missing.");
            foreach (var name in CharacterSelectScreenAccess.ModeTypes)
            {
                var type = typeof(MegaCrit.Sts2.Core.Runs.RunState).Assembly.GetType(name);
                if (type == null) continue;
                var screen = (Control)RuntimeHelpers.GetUninitializedObject(type); GC.SuppressFinalize(screen);
                if (!CrossbowFoolSelectCompat.SupportsScreen(screen)) throw new Exception("Mode screen excluded: " + name);
            }
            if (CrossbowFoolSelectCompat.SupportsScreen(_background) || CrossbowFoolSelectCompat.SupportsScreen(
                    Fake<MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectScreen>()))
                throw new Exception("Standard/unrelated screens must keep their existing owner.");
            using var stream = assembly.GetManifestResourceStream("CrossbowFool.SelectionParts")!;
            using var reader = new StreamReader(stream);
            var script = reader.ReadToEnd();
            if (!script.Contains("sync_enabled") || !script.Contains("outfit_changed") || script.Contains("NCharacterSelectScreen"))
                throw new Exception("Author click script no longer supports mode-independent binding.");
            Console.WriteLine("Crossbow selection passed: actual provider contract, 200 native factory/placement dispatches, Blitz/Dread support and standard/unrelated exclusion.");
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }
    private static T Fake<T>() where T : class
    { var item = (T)RuntimeHelpers.GetUninitializedObject(typeof(T)); GC.SuppressFinalize(item); return item; }
    private static bool Create(Node2D __0, Control __1, ref Control __result)
    { Check(ReferenceEquals(__0, _model) && ReferenceEquals(__1, _background)); _creates++; __result = _catcher; return false; }
    private static bool Fit(Control __0, Node2D __1, Control __2)
    { Check(ReferenceEquals(__0, _background) && ReferenceEquals(__1, _model) && ReferenceEquals(__2, _catcher)); _fits++; return false; }
    private static void Check(bool value) { if (!value) throw new Exception("Wrong author binding arguments."); }
    private sealed class EntryPointFixture { }
}

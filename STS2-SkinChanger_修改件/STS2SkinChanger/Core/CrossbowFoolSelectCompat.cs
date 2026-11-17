using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

// CrossbowFool.Selection.Bind accepts only the standard screen. Reuse the author's exact
// model detection, GDScript catcher, settings and placement on the other native screens.
internal static class CrossbowFoolSelectCompat
{
    private const string HostName = "SkinChangerCrossbowSelect";
    private const string CatcherName = "CrossbowFoolClick";
    private static readonly ConditionalWeakTable<Assembly, Contract> Contracts = new();
    internal sealed record Contract(MethodInfo Find, MethodInfo Get, MethodInfo Create,
        MethodInfo Fit, PropertyInfo Current, PropertyInfo Enabled);

    internal static Contract? ResolveContract(Assembly assembly)
    {
        if (assembly.GetName().Name != "CrossbowFool") return null;
        if (Contracts.TryGetValue(assembly, out var cached)) return cached;
        var stages = assembly.GetType("CrossbowFool.SkinStages");
        var selection = assembly.GetType("CrossbowFool.Selection");
        var parts = assembly.GetType("CrossbowFool.SelectionParts");
        var settings = assembly.GetType("CrossbowFool.WearSettings");
        var find = stages == null ? null : AccessTools.Method(stages, "Find", [typeof(Node)]);
        var get = stages == null ? null : AccessTools.Method(stages, "Get", [typeof(Node), typeof(string)]);
        var create = parts == null ? null : AccessTools.Method(parts, "Create", [typeof(Node2D), typeof(Control)]);
        var fit = selection == null ? null : AccessTools.Method(selection, "FitArt", [typeof(Control), typeof(Node2D), typeof(Control)]);
        var current = settings == null ? null : AccessTools.Property(settings, "Current");
        var enabled = settings == null ? null : AccessTools.Property(settings, "Enabled");
        if (get == null || current == null || find?.ReturnType != typeof(Node) || get.ReturnType != stages ||
            create?.ReturnType != typeof(Control) || fit?.ReturnType != typeof(void) ||
            current.PropertyType != settings || enabled?.PropertyType != typeof(bool)) return null;
        var contract = new Contract(find, get, create, fit, current, enabled);
        Contracts.Add(assembly, contract);
        return contract;
    }

    internal static bool SupportsScreen(Control screen) =>
        screen is not NCharacterSelectScreen && CharacterSelectScreenAccess.IsSupported(screen);

    internal static void AfterRebuild(Control screen, string groupId)
    {
        if (groupId != "fool" || !SupportsScreen(screen)) return;
        var timer = screen.GetNodeOrNull<Godot.Timer>(HostName);
        if (timer == null)
        {
            timer = new Godot.Timer { Name = HostName, WaitTime = 0.2,
                ProcessMode = Node.ProcessModeEnum.Always };
            var observer = timer;
            timer.Timeout += () => Tick(screen, observer);
            screen.AddChild(timer);
            timer.Start();
        }
        Tick(screen, timer);
    }

    internal static Control CreateCatcher(Contract contract, Node2D model, Control background) =>
        (Control)contract.Create.Invoke(null, [model, background])!;

    internal static void FitCatcher(Contract contract, Node2D model, Control background, Control catcher) =>
        contract.Fit.Invoke(null, [background, model, catcher]);

    private static void Tick(Control screen, Godot.Timer observer)
    {
        if (!GodotObject.IsInstanceValid(screen) || !screen.IsVisibleInTree()) return;
        try
        {
            if (CharacterSelectScreenAccess.Background(screen) is not { } container) return;
            var provider = SkinService.GetSelectedRuntimeProvider("fool");
            var assembly = provider == null ? null : ManagedSkinModLoader.GetActiveProviderAssembly(provider);
            var contract = assembly == null ? null : ResolveContract(assembly);
            foreach (var background in container.GetChildren().OfType<Control>())
            {
                var catcher = background.GetNodeOrNull<Control>(CatcherName);
                // Selection changes must not leave the previous provider consuming input.
                if (contract == null || !background.IsVisibleInTree())
                {
                    if (catcher != null) catcher.MouseFilter = Control.MouseFilterEnum.Ignore;
                    continue;
                }
                var model = contract.Find.Invoke(null, [background]) as Node2D;
                if (model == null || contract.Get.Invoke(null, [model, "select"]) == null)
                {
                    if (catcher != null) catcher.MouseFilter = Control.MouseFilterEnum.Ignore;
                    continue;
                }
                if (catcher == null)
                {
                    catcher = CreateCatcher(contract, model, background);
                    background.AddChild(catcher);
                }
                var enabled = (bool)contract.Enabled.GetValue(contract.Current.GetValue(null))!;
                catcher.Set("model", model);
                catcher.Call("sync_enabled", enabled);
                FitCatcher(contract, model, background, catcher);
                catcher.MouseFilter = enabled ? Control.MouseFilterEnum.Stop : Control.MouseFilterEnum.Ignore;
            }
        }
        catch (Exception exception)
        {
            // Fail once for an unsupported provider revision, without a 5 Hz exception loop.
            observer.Stop();
            ModLog.Warn("弩手选角点击接入失败：" + exception.GetBaseException().Message);
        }
    }
}

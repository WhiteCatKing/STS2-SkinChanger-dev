using System.Reflection;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Events;
using STS2SkinChanger;

internal static class AncientBackgroundContractTests
{
    internal static void Run()
    {
        // Inspect the installed game's real callback: no engine, event state or save writes.
        var configure = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(Vakuu), "ConfigureBackgroundScene"));
        Require(configure.Any(i => i.opcode == OpCodes.Ldstr && (string)i.operand == "TextureRect") &&
            configure.Any(i => i.operand is MethodInfo m && m.Name == "GetNode" && m.IsGenericMethod &&
                m.GetGenericArguments().SequenceEqual([typeof(TextureRect)])), "Vakuu's required child/type changed.");
        var choose = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(Vakuu), "ChooseBgPath"));
        Require(choose.Any(i => i.operand is MethodInfo m && m.Name == "get_AngryBgPath") &&
            choose.Any(i => i.operand is MethodInfo m && m.Name == "get_NormalBgPath"), "Both official art branches must remain native.");

        var screen = typeof(Entry).Assembly.GetType("STS2SkinChanger.Ui.AncientCompendiumScreen", true)!;
        var build = PatchProcessor.GetOriginalInstructions(AccessTools.Method(screen, "BuildSelectedRuntimeImageScene"));
        var rectangles = build.Select((instruction, index) => (instruction, index))
            .Where(pair => pair.instruction.operand is ConstructorInfo ctor && ctor.DeclaringType == typeof(TextureRect))
            .Select(pair => pair.index).ToArray();
        Require(rectangles.Length == 2, "Static provider art needs independent display and official-write slots.");
        var display = build.Skip(rectangles[0]).Take(rectangles[1] - rectangles[0]).ToArray();
        var slot = build.Skip(rectangles[1]).TakeWhile(i => i.operand is not ConstructorInfo c || c.DeclaringType != typeof(PackedScene)).ToArray();
        Require(display.Any(i => i.opcode == OpCodes.Ldstr && (string)i.operand == "Image") &&
            display.Any(i => i.operand is MethodInfo m && m.Name == "GetSelectedRuntimeImageTexture"), "Selected provider art must stay on Image.");
        Require(slot.Any(i => i.opcode == OpCodes.Ldstr && (string)i.operand == "TextureRect") &&
            !slot.Any(i => i.operand is MethodInfo m && m.Name == "set_Texture"), "Official slot must start empty and satisfy the actual node lookup.");
        Require(Assigned(slot, "set_Visible", OpCodes.Ldc_I4_0) &&
            Assigned(slot, "set_ExpandMode", OpCodes.Ldc_I4_1) &&
            Assigned(slot, "set_MouseFilter", OpCodes.Ldc_I4_2), "Official art must remain hidden and cannot capture event-option input.");
        foreach (var block in new[] { display, slot })
            Require(block.Any(i => i.operand is MethodInfo m && m.Name == "AddChild") &&
                block.Any(i => i.operand is MethodInfo m && m.Name == "set_Owner"), "Both children must survive PackedScene packing.");
        Console.WriteLine("Vakuu background contract passed: native normal/angry branches, separate provider Image and packed hidden TextureRect slot.");
    }
    private static bool Assigned(CodeInstruction[] code, string setter, OpCode value) =>
        code.Select((instruction, index) => (instruction, index)).Any(pair =>
        {
            if (pair.instruction.operand is not MethodInfo m || m.Name != setter) return false;
            var index = pair.index - 1;
            // Godot enums use long; the compiler inserts conv.i8 before their setters.
            if (index >= 0 && code[index].opcode == OpCodes.Conv_I8) index--;
            return index >= 0 && code[index].opcode == value;
        });
    private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
}

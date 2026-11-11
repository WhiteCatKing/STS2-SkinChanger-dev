using Mono.Cecil;

if (args.Length < 3)
{
    Console.Error.WriteLine("usage: asmrename <input.dll> <output.dll> <newAssemblyName>");
    return 2;
}

var input = Path.GetFullPath(args[0]);
var output = Path.GetFullPath(args[1]);
var newName = args[2];

// Only the assembly identity changes: every type, resource, attribute and IL body stays byte for
// byte identical so the pack keeps its own scripts, settings and Harmony callbacks.
using var assembly = AssemblyDefinition.ReadAssembly(
    input,
    new ReaderParameters { ReadSymbols = false });
var before = assembly.Name.FullName;
assembly.Name.Name = newName;
assembly.Write(output);
Console.WriteLine($"assembly identity: {before} -> {assembly.Name.FullName}");
Console.WriteLine($"module name kept: {assembly.MainModule.Name}");
Console.WriteLine($"written: {output} ({new FileInfo(output).Length} bytes)");
return 0;

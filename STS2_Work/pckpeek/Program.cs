using STS2SkinChanger.Pck;
var pck = args[0];
var mode = args.Length > 1 ? args[1] : "list";
var filter = args.Length > 2 ? args[2] : "";
using var a = PckArchive.Open(pck);
if (mode == "list")
{
    foreach (var p in a.Paths.Where(p => p.Contains(filter, StringComparison.OrdinalIgnoreCase)).OrderBy(p => p))
        Console.WriteLine(p);
}
else if (mode == "cat")
{
    var bytes = a.ReadFile(filter);
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(bytes));
}

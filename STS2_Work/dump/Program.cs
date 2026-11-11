using System.Collections.Immutable;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;

if (args.Length < 2)
{
    Console.Error.WriteLine("usage: dump <assembly> <outfile>");
    return 1;
}

using var fs = File.OpenRead(args[0]);
using var pe = new PEReader(fs);
var md = pe.GetMetadataReader();

var sb = new StringBuilder();
var asm = md.GetAssemblyDefinition();
sb.AppendLine($"ASSEMBLY {md.GetString(asm.Name)} {asm.Version}");

foreach (var handle in md.AssemblyReferences)
{
    var ar = md.GetAssemblyReference(handle);
    sb.AppendLine($"REF {md.GetString(ar.Name)} {ar.Version}");
}

foreach (var th in md.TypeDefinitions)
{
    var td = md.GetTypeDefinition(th);
    var ns = md.GetString(td.Namespace);
    var name = md.GetString(td.Name);
    var full = string.IsNullOrEmpty(ns) ? name : ns + "." + name;
    if (full.StartsWith("<")) continue;
    sb.AppendLine($"TYPE {full}");

    foreach (var mh in td.GetMethods())
    {
        var m = md.GetMethodDefinition(mh);
        var mn = md.GetString(m.Name);
        if (mn.StartsWith("get_") || mn.StartsWith("set_") || mn.StartsWith("add_") || mn.StartsWith("remove_")) continue;
        var sig = m.DecodeSignature(new SigProvider(), null);
        sb.AppendLine($"  M {mn}({string.Join(",", sig.ParameterTypes)})");
    }
}

File.WriteAllText(args[1], sb.ToString());
return 0;

sealed class SigProvider : ISignatureTypeProvider<string, object?>
{
    public string GetArrayType(string elementType, ArrayShape shape) => elementType + "[]";
    public string GetByReferenceType(string elementType) => "ref " + elementType;
    public string GetFunctionPointerType(MethodSignature<string> signature) => "fnptr";
    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments) => genericType + "<" + string.Join(",", typeArguments) + ">";
    public string GetGenericMethodParameter(object? genericContext, int index) => "!!" + index;
    public string GetGenericTypeParameter(object? genericContext, int index) => "!" + index;
    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired) => unmodifiedType;
    public string GetPinnedType(string elementType) => elementType;
    public string GetPointerType(string elementType) => elementType + "*";
    public string GetPrimitiveType(PrimitiveTypeCode typeCode) => typeCode.ToString();
    public string GetSZArrayType(string elementType) => elementType + "[]";
    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        var td = reader.GetTypeDefinition(handle);
        var ns = reader.GetString(td.Namespace);
        var n = reader.GetString(td.Name);
        return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
    }
    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        var tr = reader.GetTypeReference(handle);
        var ns = reader.GetString(tr.Namespace);
        var n = reader.GetString(tr.Name);
        return string.IsNullOrEmpty(ns) ? n : ns + "." + n;
    }
    public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        => "spec";
}

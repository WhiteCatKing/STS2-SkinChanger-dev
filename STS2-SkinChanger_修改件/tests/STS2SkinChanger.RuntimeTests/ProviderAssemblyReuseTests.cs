using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using STS2SkinChanger.Core;

internal static class ProviderAssemblyReuseTests
{
    internal static void Run()
    {
        var root = Directory.CreateTempSubdirectory("sc-provider-reuse-");
        var logging = AuditLogSilencer.Install();
        var type = typeof(ManagedSkinModLoader);
        const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
        var providers = (IDictionary)type.GetField("ProviderAssemblies", flags)!.GetValue(null)!;
        var aliases = (IDictionary)type.GetField("BehaviorProviderAliases", flags)!.GetValue(null)!;
        var registerLoaded = type.GetMethod("CacheLoadedProviderAssembly", flags)!;
        var providerType = type.GetNestedType("ProviderAssembly", BindingFlags.NonPublic)!;
        var family = "provider-fixture-" + Guid.NewGuid().ToString("N");
        var original = family + "::source:original";
        var reexport = family + "::source:reexport";
        var sameVersion = family + "::source:same-version";
        var unrelated = family + "-unrelated";
        try
        {
            string Write(string fileName, Version version)
            {
                var path = Path.Combine(root.FullName, fileName);
                File.WriteAllBytes(path, CreateAssembly(family, version));
                return path;
            }
            void Register(string id, string path, bool allowsBehavior = true) =>
                providers[id] = Activator.CreateInstance(providerType, path, id, false, false, allowsBehavior, id)!;

            // As with a compatibility rewrite, the already loaded assembly has no Location,
            // and neither package's DLL filename matches its actual assembly name.
            var firstPath = Write("first-merchant.dll", new Version(1, 2, 0, 0));
            var secondPath = Write("second-merchant.dll", new Version(1, 3, 7, 0));
            var sameVersionPath = Write("same-version.dll", new Version(1, 2, 0, 0));
            var unrelatedPath = Write("unrelated.dll", new Version(1, 4, 0, 0));
            Register(original, firstPath);
            Register(reexport, secondPath);
            Register(sameVersion, sameVersionPath, allowsBehavior: false);
            Register(unrelated, unrelatedPath);
            using var stream = File.OpenRead(firstPath);
            var context = AssemblyLoadContext.GetLoadContext(type.Assembly)!;
            var loaded = context.LoadFromStream(stream);
            registerLoaded.Invoke(null, [firstPath, "fixture-fingerprint", loaded.FullName, loaded]);
            Require(string.IsNullOrEmpty(loaded.Location), "Fixture must use an in-memory assembly.");

            Require(ManagedSkinModLoader.TryGetProviderAssemblyForInspection(reexport, out var resolved) &&
                    ReferenceEquals(resolved, loaded) && ManagedSkinModLoader.ResolveBehaviorProviderId(reexport) == original,
                "Same-ID versions must reuse the loaded assembly by metadata name in the actual load context.");
            Require(!ManagedSkinModLoader.TryGetLoadedProviderAssembly(secondPath, out _),
                "A behavior alias must not advertise the selected package's DLL as independently loaded.");
            Require(ManagedSkinModLoader.TryGetProviderAssemblyForInspection(sameVersion, out var identicalIdentity) &&
                    ReferenceEquals(identicalIdentity, loaded) && ManagedSkinModLoader.ResolveBehaviorProviderId(sameVersion) == original,
                "A registration-time binary conflict still permits delegation to the same-ID source.");
            Require(!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(unrelated, out _) &&
                    ManagedSkinModLoader.ResolveBehaviorProviderId(unrelated) == unrelated,
                "An unrelated same-name assembly must stay resource-only.");
            Require(context.Assemblies.Count(assembly => assembly.GetName().Name == family) == 1,
                "Only one behavior assembly may be loaded in the game context.");
        }
        finally
        {
            foreach (var id in new[] { original, reexport, sameVersion, unrelated })
            {
                providers.Remove(id);
                aliases.Remove(id);
            }
            logging.UnpatchAll(logging.Id);
            root.Delete(recursive: true);
        }
        Console.WriteLine("Provider assembly reuse passed: renamed DLLs, version collision, stream ownership and unrelated provider isolation.");
    }

    private static byte[] CreateAssembly(string name, Version version)
    {
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString(name + ".dll"), metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
        metadata.AddAssembly(metadata.GetOrAddString(name), version, default, default, (AssemblyFlags)0, AssemblyHashAlgorithm.None);
        metadata.AddTypeDefinition(TypeAttributes.NotPublic, default, metadata.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        var image = new BlobBuilder();
        new ManagedPEBuilder(new PEHeaderBuilder(imageCharacteristics: Characteristics.Dll | Characteristics.ExecutableImage),
            new MetadataRootBuilder(metadata), new BlobBuilder()).Serialize(image);
        return image.ToArray();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

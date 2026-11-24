using System.Collections;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using STS2SkinChanger.Core;

internal static class ProviderAssemblyReuseTests
{
    private static readonly (string Namespace, string Name)[] SlimmerFamilyTypes =
    [
        ("SharedSkin", "ModEntry"),
        ("SharedSkin", "Palette"),
        ("SharedSkin", "WearTimer"),
        ("SharedSkin", "Settings"),
        ("SharedSkin", "CharSelectBridge")
    ];

    private static readonly (string Namespace, string Name)[] FullerFamilyTypes =
    [
        .. SlimmerFamilyTypes,
        ("SharedSkin", "CombatStageWear")
    ];

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
        var unregisteredFamily = "provider-unregistered-" + Guid.NewGuid().ToString("N");
        var original = family + "::source:original";
        var reexport = family + "::source:reexport";
        var sameVersion = family + "::source:same-version";
        var slimmerTwin = family + "-slimmer-twin";
        var fullerTwin = family + "-fuller-twin";
        var unrelated = family + "-unrelated";
        var differentCulture = family + "-culture";
        var unregisteredCollision = unregisteredFamily + "-orphan";
        try
        {
            string Write(
                string fileName,
                string assemblyName,
                Version version,
                string? culture = null,
                (string Namespace, string Name)[]? definedTypes = null)
            {
                var path = Path.Combine(root.FullName, fileName);
                File.WriteAllBytes(path, CreateAssembly(assemblyName, version, culture, definedTypes));
                return path;
            }
            void Register(string id, string path, bool allowsBehavior = true) =>
                providers[id] = Activator.CreateInstance(providerType, path, id, false, false, allowsBehavior, id)!;

            // As with a compatibility rewrite, the already loaded assembly has no Location,
            // and neither package's DLL filename matches its actual assembly name.
            var firstPath = Write("first-merchant.dll", family, new Version(1, 2, 0, 0), definedTypes: FullerFamilyTypes);
            // Same-logical-ID copies intentionally omit FullerFamilyTypes to prove the original
            // same-ID reuse path does not require the different-ID type-overlap gate.
            var secondPath = Write("second-merchant.dll", family, new Version(1, 3, 7, 0));
            var sameVersionPath = Write("same-version.dll", family, new Version(1, 2, 0, 0));
            // Slimmer twin (5 types <= 6 types, Jaccard 5/6 = 0.833 >= 0.80): allowed to delegate to original.
            var slimmerTwinPath = Write("slimmer-twin.dll", family, new Version(1, 4, 0, 0),
                definedTypes: SlimmerFamilyTypes);
            // Fuller twin (7 types > 6 types, Jaccard 6/7 = 0.857 >= 0.80): must NOT reverse-delegate to slimmer loaded original.
            var fullerTwinPath = Write("fuller-twin.dll", family, new Version(1, 4, 5, 0),
                definedTypes: [.. FullerFamilyTypes, ("SharedSkin", "NewerCombatFeature")]);
            // Template-cloned unrelated mod shares the same simple name and only ModEntry (1/10 = 0.10 < 0.80).
            var unrelatedPath = Write("unrelated.dll", family, new Version(1, 4, 1, 0),
                definedTypes:
                [
                    ("SharedSkin", "ModEntry"),
                    ("UnrelatedMod", "Alpha"),
                    ("UnrelatedMod", "Beta"),
                    ("UnrelatedMod", "Gamma"),
                    ("UnrelatedMod", "Delta")
                ]);
            var differentCulturePath = Write("different-culture.dll", family, new Version(1, 5, 0, 0),
                culture: "en-US", definedTypes: SlimmerFamilyTypes);
            var unregisteredHostPath = Write("unregistered-host.dll", unregisteredFamily, new Version(1, 0, 0, 0),
                definedTypes: FullerFamilyTypes);
            var unregisteredCollisionPath = Write("unregistered-collision.dll", unregisteredFamily, new Version(2, 0, 0, 0),
                definedTypes: SlimmerFamilyTypes);

            Register(original, firstPath);
            Register(reexport, secondPath);
            Register(sameVersion, sameVersionPath, allowsBehavior: false);
            Register(slimmerTwin, slimmerTwinPath);
            Register(fullerTwin, fullerTwinPath);
            Register(unrelated, unrelatedPath);
            Register(differentCulture, differentCulturePath);
            Register(unregisteredCollision, unregisteredCollisionPath);

            var context = AssemblyLoadContext.GetLoadContext(type.Assembly)!;
            Assembly loaded;
            using (var stream = File.OpenRead(firstPath))
                loaded = context.LoadFromStream(stream);
            registerLoaded.Invoke(null, [firstPath, "fixture-fingerprint", loaded.FullName, loaded]);
            Require(string.IsNullOrEmpty(loaded.Location), "Fixture must use an in-memory assembly.");

            // Load an assembly with the same simple name as unregisteredCollision, without registering any provider for it.
            using (var hostStream = File.OpenRead(unregisteredHostPath))
                _ = context.LoadFromStream(hostStream);

            Require(ManagedSkinModLoader.TryGetProviderAssemblyForInspection(reexport, out var resolved) &&
                    ReferenceEquals(resolved, loaded) && ManagedSkinModLoader.ResolveBehaviorProviderId(reexport) == original,
                "Same-ID versions must reuse the loaded assembly by metadata name in the actual load context.");
            Require(!ManagedSkinModLoader.TryGetLoadedProviderAssembly(secondPath, out _),
                "A behavior alias must not advertise the selected package's DLL as independently loaded.");
            Require(ManagedSkinModLoader.TryGetProviderAssemblyForInspection(sameVersion, out var identicalIdentity) &&
                    ReferenceEquals(identicalIdentity, loaded) && ManagedSkinModLoader.ResolveBehaviorProviderId(sameVersion) == original,
                "A registration-time binary conflict still permits delegation to the same-ID source.");
            Require(ManagedSkinModLoader.TryGetProviderAssemblyForInspection(slimmerTwin, out var twinResolved) &&
                    ReferenceEquals(twinResolved, loaded) && ManagedSkinModLoader.ResolveBehaviorProviderId(slimmerTwin) == original &&
                    !ManagedSkinModLoader.TryGetLoadedProviderAssembly(slimmerTwinPath, out _),
                "A slimmer different-ID sibling sharing simple name, culture, public key token and >=0.80 type overlap must delegate to the fuller loaded sibling.");
            Require(!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(fullerTwin, out _) &&
                    ManagedSkinModLoader.ResolveBehaviorProviderId(fullerTwin) == fullerTwin,
                "A fuller sibling must NOT reverse-delegate to an already-loaded slimmer assembly.");
            Require(!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(unrelated, out _) &&
                    ManagedSkinModLoader.ResolveBehaviorProviderId(unrelated) == unrelated,
                "An unrelated same-name assembly with low type overlap must stay resource-only.");
            Require(!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(differentCulture, out _) &&
                    ManagedSkinModLoader.ResolveBehaviorProviderId(differentCulture) == differentCulture,
                "A same-name assembly with a different culture must stay resource-only.");
            Require(!ManagedSkinModLoader.TryGetProviderAssemblyForInspection(unregisteredCollision, out _) &&
                    ManagedSkinModLoader.ResolveBehaviorProviderId(unregisteredCollision) == unregisteredCollision,
                "A collision against an assembly not owned by any registered provider must stay resource-only.");
            Require(context.Assemblies.Count(assembly => assembly.GetName().Name == family) == 1,
                "Only one behavior assembly may be loaded in the game context.");
        }
        finally
        {
            foreach (var id in new[] { original, reexport, sameVersion, slimmerTwin, fullerTwin, unrelated, differentCulture, unregisteredCollision })
            {
                providers.Remove(id);
                aliases.Remove(id);
            }
            logging.UnpatchAll(logging.Id);
            root.Delete(recursive: true);
        }
        Console.WriteLine("Provider assembly reuse passed: renamed DLLs, version collision, slimmer->fuller delegation, reverse-delegation block, stream ownership and unrelated/culture/unregistered isolation.");
    }

    private static byte[] CreateAssembly(
        string name,
        Version version,
        string? culture = null,
        (string Namespace, string Name)[]? definedTypes = null)
    {
        var metadata = new MetadataBuilder();
        metadata.AddModule(0, metadata.GetOrAddString(name + ".dll"), metadata.GetOrAddGuid(Guid.NewGuid()), default, default);
        var cultureHandle = string.IsNullOrEmpty(culture) ? default : metadata.GetOrAddString(culture);
        metadata.AddAssembly(metadata.GetOrAddString(name), version, cultureHandle, default, (AssemblyFlags)0, AssemblyHashAlgorithm.None);
        metadata.AddTypeDefinition(TypeAttributes.NotPublic, default, metadata.GetOrAddString("<Module>"), default,
            MetadataTokens.FieldDefinitionHandle(1), MetadataTokens.MethodDefinitionHandle(1));
        if (definedTypes != null)
        {
            foreach (var (ns, typeName) in definedTypes)
            {
                metadata.AddTypeDefinition(
                    TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed,
                    string.IsNullOrEmpty(ns) ? default : metadata.GetOrAddString(ns),
                    metadata.GetOrAddString(typeName),
                    default,
                    MetadataTokens.FieldDefinitionHandle(1),
                    MetadataTokens.MethodDefinitionHandle(1));
            }
        }
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

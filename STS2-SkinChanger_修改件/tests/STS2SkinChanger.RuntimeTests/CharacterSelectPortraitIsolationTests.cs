using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using System.Runtime.Loader;
using System.Text;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

internal static class CharacterSelectPortraitIsolationTests
{
    private const string Normal = "res://images/packed/character_select/char_select_ironclad.png";
    private const string Locked = "res://images/packed/character_select/char_select_ironclad_locked.png";
    private static string _cache = null!;
    private static Dictionary<string, CompressedTexture2D> _testTextures = null!;
    private static Dictionary<string, Resource> _stageResources = null!;
    private static int _stageLoads;

    internal static void Run()
    {
        var log = AuditLogSilencer.Install();
        var root = Directory.CreateTempSubdirectory("sc-select-portrait-");
        var writer = PckArchive.WriteOutput;
        try
        {
            PckArchive.WriteOutput = null;
            var game = Path.Combine(root.FullName, "game.pck");
            var skin = Path.Combine(root.FullName, "portrait.pck");
            const string samePayload = "res://.godot/imported/ironclad.png-a.ctex";
            const string lockedGame = "res://.godot/imported/ironclad-locked.png-b.ctex";
            const string lockedSkin = "res://example/locked.ctex";
            const string privateAsset = "res://example/keep.txt";
            static byte[] Import(string path) => Encoding.UTF8.GetBytes($"[remap]\npath=\"{path}\"\n");

            PckArchive.Write(game, new Dictionary<string, byte[]>
            {
                [Normal + ".import"] = Import(samePayload), [samePayload] = Texture(1),
                [Locked + ".import"] = Import(lockedGame), [lockedGame] = Texture(2)
            });
            PckArchive.Write(skin, new Dictionary<string, byte[]>
            {
                [Normal + ".import"] = Import(samePayload), [samePayload] = Texture(3),
                [Locked + ".import"] = Import(lockedSkin), [lockedSkin] = Texture(4),
                [privateAsset] = [5]
            });

            using var catalog = SkinCatalog.Build(game, [new("fixture", "fixture", skin, false)]);
            CheckIsolation(catalog, "fixture", skin);
            var overlay = catalog.BuildOverlay(new Dictionary<string, string> { ["ironclad"] = "fixture" });
            Require(overlay[samePayload].Read().SequenceEqual(Texture(1)) &&
                    overlay[lockedGame].Read().SequenceEqual(Texture(2)),
                "The correction mount must keep both public icon payloads on the game source.");
            CheckProjection(catalog, skin, root.FullName, projected =>
            {
                Require(!projected.Contains(samePayload) && projected.Contains(privateAsset),
                    "A high-priority runtime pack must filter shared vanilla import payloads and retain private files.");
                GlobalWriteLedgerTests.AssertSafePack(catalog, GlobalWriteSite.A10, projected.Path, "select portrait projection");
            });
            Console.WriteLine("Character select portrait isolation passed: shared mounts, locked/normal aliases and runtime pack filtering.");
        }
        finally
        {
            PckArchive.WriteOutput = writer;
            log.UnpatchAll(log.Id);
        }
    }

    internal static void Audit(string game, string maidenRoot, string joanRoot, string ritsuRoot, string signatureRoot)
    {
        var log = AuditLogSilencer.Install();
        try
        {
            var joanPck = Path.Combine(joanRoot, "JoanofArcIroncladHpwear.pck");
            var maidenPck = Path.Combine(maidenRoot, "MaidenSuccubus.pck");
            using var catalog = SkinCatalog.Build(game,
            [
                new("JoanofArcIroncladHpwear", "Joan", joanPck, false, joanRoot, true),
                new("MaidenSuccubus", "Hibiki", maidenPck, true, maidenRoot, true)
            ]);
            var selection = catalog.Groups.Single(g => g.Id == "ironclad").Options.First(o =>
                o.EffectiveProviderId == "JoanofArcIroncladHpwear" && o.Assets.ContainsKey(Normal)).Id;
            CheckIsolation(catalog, selection, joanPck);
            using var original = PckArchive.Open(joanPck);
            var root = Directory.CreateTempSubdirectory("sc-hibiki-real-");
            CheckProjection(catalog, joanPck, root.FullName, filtered =>
            {
                Require(filtered.Paths.All(path => !catalog.ProtectCanonicalSelectPortraitFile(path)) &&
                        filtered.Paths.Count < original.Paths.Count,
                    "Real Joan runtime pack must omit base character select portraits.");
                GlobalWriteLedgerTests.AssertSafePack(catalog, GlobalWriteSite.A10, filtered.Path, "Joan select runtime projection");
            });

            // Resolve only the actual character's type and asset declarations. Do not run the
            // provider initializer or call into the engine from the offline package checks.
            var dependencies = Directory.GetFiles(ritsuRoot, "*.dll")
                .Concat(Directory.GetFiles(Path.Combine(ritsuRoot, "compat", "0.113.0"), "*.dll"))
                .Concat(Directory.GetFiles(Path.Combine(ritsuRoot, "shared"), "*.dll"))
                .Concat(Directory.GetFiles(signatureRoot, "*.dll")).Reverse().ToArray();
            Assembly? Resolve(AssemblyLoadContext loader, AssemblyName name)
            {
                var path = dependencies.FirstOrDefault(file =>
                    Path.GetFileNameWithoutExtension(file).Equals(name.Name, StringComparison.OrdinalIgnoreCase));
                return path == null ? null : loader.LoadFromAssemblyPath(Path.GetFullPath(path));
            }
            var loadContext = AssemblyLoadContext.Default;
            loadContext.Resolving += Resolve;
            try
            {
                var assembly = loadContext.LoadFromAssemblyPath(Path.GetFullPath(Path.Combine(maidenRoot, "MaidenSuccubus.dll")));
                var type = assembly.GetType("MaidenSuccubus.Characters.MaidenSuccubusCharacter", throwOnError: true)!;
                Require(typeof(CharacterModel).IsAssignableFrom(type), "Expected real Hibiki content class.");
                using var maiden = PckArchive.Open(maidenPck);
                foreach (var locked in new[] { false, true })
                {
                    var path = HibikiSelectPortrait.PrivatePath(type, locked);
                    Require(path != null && maiden.Contains(path + ".import"),
                        "Real Hibiki icon must resolve to its own imported resource.");
                    Require(HibikiSelectPortrait.PrivatePath(typeof(CharacterModel), locked) == null,
                        "The route must be scoped to the exact provider and character.");
                    var imported = Encoding.UTF8.GetString(maiden.ReadFile(path + ".import"));
                    var payload = imported.Split('\n').Single(line => line.StartsWith("path=\"", StringComparison.Ordinal))
                        .Split('"')[1];
                    Require(maiden.Contains(payload) && payload.Contains(locked ? "_locked_" : "_normal_"),
                        "Hibiki's normal and locked icons must have separate real payloads.");
                    Require(AccessTools.PropertyGetter(typeof(CharacterModel),
                        locked ? "CharacterSelectLockedIconPath" : "CharacterSelectIconPath") != null,
                        "The real game must expose the portrait path hook for both states.");
                }
                CheckRealGetterRouting(type, catalog, selection);
            }
            finally { loadContext.Resolving -= Resolve; }
            Console.WriteLine("Actual Hibiki and Joan portrait isolation passed.");
        }
        finally { log.UnpatchAll(log.Id); }
    }

    private static void CheckRealGetterRouting(Type type, SkinCatalog catalog, string selection)
    {
        var harmony = new Harmony("Gurio.SkinChanger.Tests.HibikiPortraitRouting");
        var normal = HibikiSelectPortrait.PrivatePath(type, false)!;
        var locked = HibikiSelectPortrait.PrivatePath(type, true)!;
        _testTextures = new(StringComparer.Ordinal)
        {
            [normal] = NewTexture(), [locked] = NewTexture(),
            [Normal] = NewTexture(), [Locked] = NewTexture()
        };
        try
        {
            harmony.Patch(AccessTools.Method(typeof(ResourceLoader), "Exists", [typeof(string), typeof(string)]),
                prefix: new HarmonyMethod(typeof(CharacterSelectPortraitIsolationTests), nameof(Exists)));
            var load = typeof(ResourceLoader).GetMethods().Single(method =>
                method.Name == "Load" && method.IsGenericMethodDefinition);
            harmony.Patch(load.MakeGenericMethod(typeof(CompressedTexture2D)),
                prefix: new HarmonyMethod(typeof(CharacterSelectPortraitIsolationTests), nameof(LoadTexture)));

            foreach (var path in new[] { "CharacterSelectIconPath", "CharacterSelectLockedIconPath" })
            {
                var getter = HibikiSelectPortrait.DeclaredGetter(type, path);
                harmony.Patch(getter, postfix: new HarmonyMethod(typeof(CharacterSelectPortraitIsolationTests), nameof(BorrowIronclad)) { priority = Priority.Normal });
            }
            HibikiSelectPortrait.InstallFor(type);

            var character = (CharacterModel)RuntimeHelpers.GetUninitializedObject(type);
            Require((string?)AccessTools.PropertyGetter(type, "CharacterSelectIconPath")
                        .Invoke(character, null) == normal &&
                    (string?)AccessTools.PropertyGetter(type, "CharacterSelectLockedIconPath")
                        .Invoke(character, null) == locked,
                "A borrowed third-party path must be replaced for normal and locked state.");
            Require(ReferenceEquals(character.CharacterSelectIcon, _testTextures[normal]) &&
                    ReferenceEquals(character.CharacterSelectLockedIcon, _testTextures[locked]),
                "The two real character getters must return their own texture when another patch supplies the borrowed path.");

            CheckStageLoadRouting(harmony, catalog, selection, character, normal, locked);
        }
        finally
        {
            new Harmony(STS2SkinChanger.Entry.ModId + ".HibikiSelectPortrait").UnpatchAll(
                STS2SkinChanger.Entry.ModId + ".HibikiSelectPortrait");
            harmony.UnpatchAll(harmony.Id);
            _testTextures.Clear();
        }
    }

    private static void CheckStageLoadRouting(
        Harmony harmony, SkinCatalog catalog, string selection,
        CharacterModel hibiki, string normal, string locked)
    {
        const string directory = "res://joanofarcironcladhpwear";
        var roots = Enumerable.Range(1, 3).Select(stage => $"{directory}/s{stage}_charselect_data.tres").ToArray();
        var graph = catalog.BuildRuntimeResourceOverlay("ironclad", selection, roots, "select-stage/001", true);
        Require(roots.All(path => graph.ResourcePaths.ContainsKey(path)) &&
                graph.Files.Keys.All(path => !catalog.ProtectCanonicalSelectPortraitFile(path)) &&
                graph.CanonicalDependencyPaths.All(path => !catalog.ProtectCanonicalSelectPortraitFile(path)),
            "Real Joan stage resources must remain available without publishing Ironclad's select portraits.");

        _stageResources = roots.ToDictionary(path => path, _ =>
        {
            var resource = (Resource)RuntimeHelpers.GetUninitializedObject(typeof(Resource));
            GC.SuppressFinalize(resource);
            return resource;
        });
        _stageLoads = 0;
        var cycleType = typeof(STS2SkinChanger.Ui.CharSelectStageCycle);
        var stageFileLoader = AccessTools.Method(cycleType, "LoadStageDataFile");
        harmony.Patch(stageFileLoader, transpiler: new HarmonyMethod(
            typeof(CharacterSelectPortraitIsolationTests), nameof(SubstituteStageResourceIo)));
        var stageLoader = AccessTools.Method(cycleType, "LoadStageData");
        try
        {
            var sharedNormal = _testTextures[Normal];
            var sharedLocked = _testTextures[Locked];
            foreach (var stage in new[] { 1, 2, 3, 1 })
            {
                var expected = _stageResources[roots[stage - 1]];
                Require(ReferenceEquals(stageLoader.Invoke(null, ["ironclad", directory, "charselect", stage]), expected),
                    "The live stage-resource entry must return the selected package's stage.");
                Require(ReferenceEquals(_testTextures[Normal], sharedNormal) &&
                        ReferenceEquals(_testTextures[Locked], sharedLocked) &&
                        ReferenceEquals(hibiki.CharacterSelectIcon, _testTextures[normal]) &&
                        ReferenceEquals(hibiki.CharacterSelectLockedIcon, _testTextures[locked]),
                    "Changing Joan's stage must leave the shared portraits and Hibiki's private icons intact.");
            }
            Require(_stageLoads == 4, "Every stage must enter the isolated graph loader exactly once.");
        }
        finally { _stageResources.Clear(); }
    }

    private static IEnumerable<CodeInstruction> SubstituteStageResourceIo(IEnumerable<CodeInstruction> instructions)
    {
        var replacement = AccessTools.Method(typeof(CharacterSelectPortraitIsolationTests), nameof(LoadStageGraph));
        foreach (var instruction in instructions)
        {
            if (instruction.operand is MethodInfo method && method.DeclaringType == typeof(SkinService) &&
                method.Name == nameof(SkinService.WithRuntimeResources))
                instruction.operand = replacement;
            yield return instruction;
        }
    }

    private static Resource? LoadStageGraph(
        string groupId, IReadOnlyCollection<string> resourcePaths,
        Func<IReadOnlyDictionary<string, Resource>, Resource?> callback,
        bool includeProviderDependencies, bool takeOverCanonicalPaths)
    {
        Require(groupId == "ironclad" && includeProviderDependencies && !takeOverCanonicalPaths &&
                resourcePaths.SequenceEqual(_stageResources.Keys),
            "The stage loader must request exactly the same private three-stage graph.");
        _stageLoads++;
        return callback(_stageResources);
    }

    private static CompressedTexture2D NewTexture()
    {
        var texture = (CompressedTexture2D)RuntimeHelpers.GetUninitializedObject(typeof(CompressedTexture2D));
        GC.SuppressFinalize(texture);
        return texture;
    }

    private static void BorrowIronclad(MethodBase __originalMethod, ref string __result)
    {
        __result = __originalMethod.Name.Contains("Locked", StringComparison.Ordinal) ? Locked : Normal;
    }

    private static bool Exists(string path, ref bool __result)
    {
        __result = _testTextures.ContainsKey(path);
        return false;
    }

    private static bool LoadTexture(string path, ref CompressedTexture2D? __result)
    {
        __result = _testTextures.GetValueOrDefault(path);
        return false;
    }

    private static void CheckIsolation(SkinCatalog catalog, string selection, string provider)
    {
        var overlay = catalog.BuildRuntimeResourceOverlay("ironclad", selection, [Normal, Locked], "select-isolation/001");
        Require(overlay.ResourcePaths.ContainsKey(Normal) && overlay.ResourcePaths.ContainsKey(Locked) &&
                overlay.ResourcePaths[Normal] != overlay.ResourcePaths[Locked],
            "Selected Ironclad portraits must remain independently addressable.");
        Require(overlay.ResourcePaths.Values.All(path => path.StartsWith("res://sts2_skin_runtime/")) &&
                !overlay.Files.Keys.Any(catalog.ProtectCanonicalSelectPortraitFile) &&
                !overlay.CanonicalDependencyPaths.Any(catalog.ProtectCanonicalSelectPortraitFile),
            "Private Ironclad textures must not replace globally visible character select paths.");
        var canonical = catalog.BuildOverlay(new Dictionary<string, string> { ["ironclad"] = selection });
        Require(!canonical.Any(file => catalog.ProtectCanonicalSelectPortraitFile(file.Key) &&
                    Path.GetFullPath(file.Value.ArchivePath) == Path.GetFullPath(provider)),
            "Global corrections must not publish the selected provider's direct icon or stock import path.");
    }

    private static void CheckProjection(SkinCatalog catalog, string source, string temp, Action<PckArchive> check)
    {
        var harmony = new Harmony("Gurio.SkinChanger.Tests.SelectPortraitProjection");
        var oldWriter = PckArchive.WriteOutput;
        try
        {
            _cache = temp;
            PckArchive.WriteOutput = null;
            harmony.Patch(AccessTools.PropertyGetter(typeof(SkinChangerPaths), "CacheDirectory"),
                prefix: new HarmonyMethod(typeof(CharacterSelectPortraitIsolationTests), nameof(Cache)));
            harmony.Patch(AccessTools.Method(typeof(SkinChangerPaths), "WriteCache"),
                prefix: new HarmonyMethod(typeof(CharacterSelectPortraitIsolationTests), nameof(Write)));
            var prepare = AccessTools.Method(typeof(SkinService), "PrepareSelectPortraitSafePack");
            var filtered = (string)prepare.Invoke(null, [catalog, source])!;
            Require(filtered != source && (string)prepare.Invoke(null, [catalog, source])! == filtered,
                "Full runtime filtering must generate one reusable PCK.");
            using var archive = PckArchive.Open(filtered);
            Require(!archive.Contains(Normal + ".import") && !archive.Contains(Locked + ".import"),
                "Full runtime filtering must omit both borrowed canonical icon paths.");
            check(archive);
        }
        finally
        {
            PckArchive.WriteOutput = oldWriter;
            harmony.UnpatchAll(harmony.Id);
        }
    }

    private static bool Cache(ref string __result) { __result = _cache; return false; }
    private static bool Write(Action write) { write(); return false; }

    private static byte[] Texture(byte marker)
    {
        var data = new byte[52];
        "GST2"u8.CopyTo(data);
        data[40] = data[42] = 8;
        data[51] = marker;
        return data;
    }

    private static void Require(bool value, string result)
    {
        if (!value) throw new InvalidOperationException(result);
    }
}


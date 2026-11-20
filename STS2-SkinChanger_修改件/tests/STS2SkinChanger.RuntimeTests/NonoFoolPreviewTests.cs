using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Core;
using STS2SkinChanger.Pck;

// NonoFoolSkin（工坊 3841262975）把愚者动画立绘挂在 NCreature._Ready（Rig.Create + AnimDirector.Play），
// 而 FrameworkModelPreview.TryRefresh 为避免战斗绑定型皮肤崩溃，跳过 FrameworkPreviewCreature 的 NCreature._Ready。
// 验证 IndependentFoolSkinCompat.ApplyPreview 能够按真实包合同在预览路径直接构建本地 Rig 播放待机动画、
// 不污染 AnimDirector._rig 与 Rig.Rigs，且切走至其他愚者皮肤或默认时无残留。
public static class NonoFoolPreviewTests
{
    private const string WorkshopNonoDir =
        @"C:\Program Files (x86)\Steam\steamapps\workshop\content\2868840\3841262975";

    private static string? _activeProvider;
    private static Assembly? _activeAssembly;
    private static CanvasItem? _vanillaBody;
    private static int _vanillaHideCount;

    // 供动态构造的 NonoFoolSkin 合同程序集回调记录执行轨迹
    public static FakeSkinConfig? HarnessConfig;
    public static readonly Dictionary<ulong, object> HarnessRigs = [];
    public static readonly Dictionary<string, Texture2D?> HarnessArt = new(StringComparer.OrdinalIgnoreCase);
    public static SpriteFrames? HarnessBuiltFrames;
    public static int HarnessInitCount;
    public static int HarnessRigCreateCount;
    public static int HarnessBuildFramesCount;
    public static int HarnessShowFramesCount;
    public static int HarnessShowStaticCount;
    public static FakeRig? HarnessLastRig;
    public static string? HarnessLastBuildName;
    public static float HarnessLastSpeed;
    public static bool HarnessLastLoop;
    public static float? HarnessLastTargetHeight;
    public static string? HarnessLastReturnTo;
    public static Texture2D? HarnessLastStaticOpen;
    public static bool HarnessLastStaticWithIdle;
    private static ulong _nextRigKey = 100;

    public class FakeLayoutConfig
    {
        public float TargetHeight { get; set; } = 340f;
        public float BaseX { get; set; } = 14f;
        public float BaseY { get; set; } = -180f;
    }

    public class FakeAnimConfig
    {
        public string Mode { get; set; } = "frames";
        public string? Prefix { get; set; } = "idle_";
        public string? File { get; set; }
        public string? EyesClosed { get; set; }
        public float Speed { get; set; } = 1.0f;
        public bool Loop { get; set; } = true;
        public float? TargetHeight { get; set; }
        public string? ReturnTo { get; set; }
        public string? Fallback { get; set; } = "idle_static";
    }

    public class FakeSkinConfig
    {
        public FakeLayoutConfig Layout { get; set; } = new();
        public Dictionary<string, FakeAnimConfig> Animations { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class FakeRig
    {
        public NCreatureVisuals? Visuals { get; set; }
        public Texture2D? Open { get; set; }
        public Texture2D? Closed { get; set; }
        public FakeLayoutConfig? Layout { get; set; }

        public void ShowFrames(SpriteFrames frames, float speed, bool loop, float? targetHeight, string? returnTo)
        {
            Require(ReferenceEquals(frames, HarnessBuiltFrames), "ShowFrames 必须收到 BuildFrames 返回的 SpriteFrames。");
            HarnessShowFramesCount++;
            HarnessLastSpeed = speed;
            HarnessLastLoop = loop;
            HarnessLastTargetHeight = targetHeight;
            HarnessLastReturnTo = returnTo;
        }

        public void ShowStatic(Texture2D? tex, bool withIdle)
        {
            HarnessShowStaticCount++;
            HarnessLastStaticOpen = tex;
            HarnessLastStaticWithIdle = withIdle;
        }
    }

    public static Texture2D? HarnessGetArt(string fileName) =>
        HarnessArt.TryGetValue(fileName, out var tex) ? tex : null;

    public static void HarnessInitialize()
    {
        HarnessInitCount++;
        HarnessConfig = BuildDefaultConfig();
    }

    public static SpriteFrames? HarnessBuildFrames(string name, FakeAnimConfig def)
    {
        HarnessBuildFramesCount++;
        HarnessLastBuildName = name;
        return HarnessBuiltFrames;
    }

    public static FakeRig HarnessCreateRig(
        NCreatureVisuals visuals,
        Texture2D open,
        Texture2D? closed,
        FakeLayoutConfig layout)
    {
        HarnessRigCreateCount++;
        var rig = new FakeRig
        {
            Visuals = visuals,
            Open = open,
            Closed = closed,
            Layout = layout
        };
        HarnessRigs[++_nextRigKey] = rig;
        HarnessLastRig = rig;
        return rig;
    }

    internal static void Run()
    {
        if (Directory.Exists(WorkshopNonoDir))
        {
            VerifyRealPackageContract(WorkshopNonoDir);
        }

        VerifyPreviewBehavior();
        Console.WriteLine("NonoFoolSkin preview contract & runtime isolation passed.");
    }

    internal static void VerifyRealPackageContract(string packageDir)
    {
        var dllPath = Path.Combine(packageDir, "NonoFoolSkin.dll");
        var pckPath = Path.Combine(packageDir, "NonoFoolSkin.pck");
        var animPath = Path.Combine(packageDir, "animations.json");
        Require(File.Exists(dllPath), "缺少真实 NonoFoolSkin.dll：" + dllPath);
        Require(File.Exists(pckPath), "缺少真实 NonoFoolSkin.pck：" + pckPath);

        // 纯静态 PE 元数据校验（不加载第三方程序集），确保 ApplyNonoFoolPreview 依赖的真实包合同完备。
        using (var stream = File.OpenRead(dllPath))
        using (var pe = new PEReader(stream))
        {
            var reader = pe.GetMetadataReader();
            Require(reader.GetString(reader.GetAssemblyDefinition().Name) == "NonoFoolSkin",
                "真实 NonoFoolSkin.dll 程序集名不匹配。");

            var membersByType = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            foreach (var handle in reader.TypeDefinitions)
            {
                var td = reader.GetTypeDefinition(handle);
                var fullName = reader.GetString(td.Namespace) + "." + reader.GetString(td.Name);
                var names = new HashSet<string>(StringComparer.Ordinal);
                foreach (var mh in td.GetMethods())
                {
                    names.Add(reader.GetString(reader.GetMethodDefinition(mh).Name));
                }
                foreach (var fh in td.GetFields())
                {
                    names.Add(reader.GetString(reader.GetFieldDefinition(fh).Name));
                }
                membersByType[fullName] = names;
            }

            void RequireMembers(string typeName, params string[] expected)
            {
                Require(membersByType.TryGetValue(typeName, out var found),
                    $"真实 NonoFoolSkin.dll 缺少类型：{typeName}");
                foreach (var member in expected)
                {
                    Require(found!.Contains(member),
                        $"真实 NonoFoolSkin.dll 类型 {typeName} 缺少成员：{member}");
                }
            }

            RequireMembers("NonoFoolSkin.AnimDirector", "get_Config", "Initialize", "BuildFrames", "_rig");
            RequireMembers("NonoFoolSkin.ArtStore", "Get");
            RequireMembers("NonoFoolSkin.Rig", "Create", "ShowFrames", "ShowStatic", "Rigs", "set_Closed");
            RequireMembers("NonoFoolSkin.SkinConfig", "get_Layout", "get_Animations");
            RequireMembers("NonoFoolSkin.AnimConfig",
                "get_Mode", "get_Prefix", "get_File", "get_EyesClosed",
                "get_Speed", "get_Loop", "get_TargetHeight", "get_ReturnTo", "get_Fallback");
        }

        using (var archive = PckArchive.Open(pckPath))
        {
            Require(archive.Paths.Any(p => p.EndsWith("/idle_00.png", StringComparison.OrdinalIgnoreCase)),
                "真实 NonoFoolSkin.pck 应包含待机首帧 idle_00.png。");
        }

        if (File.Exists(animPath))
        {
            var animText = File.ReadAllText(animPath);
            Require(animText.Contains("\"idle\"", StringComparison.Ordinal),
                "真实 NonoFoolSkin/animations.json 应定义 idle 动画。");
        }
    }

    private static void VerifyPreviewBehavior()
    {
        var nonoAssembly = BuildSyntheticNonoAssembly();
        var otherAssembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName("NailongFool"),
            AssemblyBuilderAccess.Run);

        var visuals = (NCreatureVisuals)RuntimeHelpers.GetUninitializedObject(typeof(NCreatureVisuals));
        var vanillaBody = (CanvasItem)RuntimeHelpers.GetUninitializedObject(typeof(Node2D));
        var idleFrame0 = (Texture2D)RuntimeHelpers.GetUninitializedObject(typeof(ImageTexture));
        var staticOpen = (Texture2D)RuntimeHelpers.GetUninitializedObject(typeof(ImageTexture));
        var staticClosed = (Texture2D)RuntimeHelpers.GetUninitializedObject(typeof(ImageTexture));
        var framesObj = (SpriteFrames)RuntimeHelpers.GetUninitializedObject(typeof(SpriteFrames));
        GC.SuppressFinalize(visuals);
        GC.SuppressFinalize(vanillaBody);
        GC.SuppressFinalize(idleFrame0);
        GC.SuppressFinalize(staticOpen);
        GC.SuppressFinalize(staticClosed);
        GC.SuppressFinalize(framesObj);

        var harmony = new Harmony("sc.tests.nono-fool-preview");
        try
        {
            harmony.Patch(
                AccessTools.Method(typeof(SkinService), "GetSelectedCreatureRuntimeProvider", [typeof(string)]),
                prefix: new HarmonyMethod(typeof(NonoFoolPreviewTests), nameof(PatchGetProvider)));
            harmony.Patch(
                AccessTools.Method(typeof(ManagedSkinModLoader), "GetActiveProviderAssembly", [typeof(string)]),
                prefix: new HarmonyMethod(typeof(NonoFoolPreviewTests), nameof(PatchGetAssembly)));
            harmony.Patch(
                AccessTools.Method(typeof(GameBranchApi), nameof(GameBranchApi.CurrentBody), [typeof(NCreatureVisuals)]),
                prefix: new HarmonyMethod(typeof(NonoFoolPreviewTests), nameof(PatchCurrentBody)));
            harmony.Patch(
                AccessTools.Method(typeof(CanvasItem), nameof(CanvasItem.Hide), Type.EmptyTypes),
                prefix: new HarmonyMethod(typeof(NonoFoolPreviewTests), nameof(PatchHide)));

            var directorType = nonoAssembly.GetType("NonoFoolSkin.AnimDirector", true)!;
            var rigField = directorType.GetField("_rig", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)!;
            var liveCombatRig = new FakeRig();
            rigField.SetValue(null, liveCombatRig);

            // 1. 帧动画模式（真实 Nono 默认配置）：走 BuildFrames + ShowFrames、隐藏原版 Body、不覆盖战斗 _rig、不残留 Rig.Rigs
            ResetCounters();
            HarnessConfig = BuildDefaultConfig();
            HarnessArt["idle_00.png"] = idleFrame0;
            HarnessBuiltFrames = framesObj;
            _activeProvider = "NonoFoolSkin";
            _activeAssembly = nonoAssembly;
            _vanillaBody = vanillaBody;

            IndependentFoolSkinCompat.ApplyPreview(visuals, "fool");

            Require(HarnessRigCreateCount == 1, "选中 NonoFoolSkin 时必须在预览上创建 Rig。");
            Require(HarnessBuildFramesCount == 1 && HarnessLastBuildName == "idle",
                "帧动画模式应调用 AnimDirector.BuildFrames(\"idle\", def)。");
            Require(HarnessShowFramesCount == 1 && HarnessShowStaticCount == 0,
                "成功构造 SpriteFrames 后必须调用 Rig.ShowFrames 播放待机帧。");
            Require(HarnessLastLoop && Math.Abs(HarnessLastSpeed - 1.0f) < 0.001f,
                "ShowFrames 参数应透传 idle 配置的 speed 与 loop。");
            Require(_vanillaHideCount == 1, "应用 Nono 预览后必须隐藏原版愚者 Body。");
            Require(ReferenceEquals(rigField.GetValue(null), liveCombatRig),
                "预览不得覆盖 AnimDirector._rig（避免干扰正在进行的战斗实体）。");
            Require(HarnessRigs.Count == 0,
                "临时预览 Rig 创建后应立即从 Rig.Rigs 摘除，切换皮肤不留陈旧引用。");

            // 2. 静态兜底模式（帧图缺失，BuildFrames 返回 null，按 Fallback=idle_static 播静态图）
            ResetCounters();
            HarnessConfig = BuildDefaultConfig();
            HarnessArt["idle_00.png"] = idleFrame0;
            HarnessArt["character.png"] = staticOpen;
            HarnessArt["character_eyes_closed.png"] = staticClosed;
            HarnessBuiltFrames = null;

            IndependentFoolSkinCompat.ApplyPreview(visuals, "fool");

            Require(HarnessRigCreateCount == 1 && HarnessShowFramesCount == 0 && HarnessShowStaticCount == 1,
                "帧图缺失时应回落 Rig.ShowStatic。");
            Require(ReferenceEquals(HarnessLastStaticOpen, staticOpen) && HarnessLastStaticWithIdle,
                "静态回落应使用 idle_static 的 character.png 并开启呼吸/眨眼。");
            Require(ReferenceEquals(HarnessLastRig?.Closed, staticClosed),
                "静态回落应同时透传 idle_static 的闭眼贴图。");
            Require(_vanillaHideCount == 1 && HarnessRigs.Count == 0 && ReferenceEquals(rigField.GetValue(null), liveCombatRig),
                "静态回落同样应隐藏原版 Body 且零状态残留。");

            // 3. 冷启动（Config == null）时自动触发 AnimDirector.Initialize()
            ResetCounters();
            HarnessConfig = null;
            HarnessArt["idle_00.png"] = idleFrame0;
            HarnessBuiltFrames = framesObj;

            IndependentFoolSkinCompat.ApplyPreview(visuals, "fool");

            Require(HarnessInitCount == 1 && HarnessShowFramesCount == 1 && _vanillaHideCount == 1,
                "AnimDirector.Config 尚未初始化时应自动调用 Initialize 再构建预览。");

            // 4. 贴图读取失败时（ArtStore.Get 返回 null）不得隐藏原版 Body，也不得建空 Rig
            ResetCounters();
            HarnessConfig = BuildDefaultConfig();
            HarnessArt.Clear();
            HarnessBuiltFrames = framesObj;

            IndependentFoolSkinCompat.ApplyPreview(visuals, "fool");

            Require(HarnessRigCreateCount == 0 && _vanillaHideCount == 0,
                "Nono 贴图缺失时应保留原版愚者模型，不得隐藏原版 Body。");

            // 5. 切回其他愚者皮肤或默认、或非愚者角色时，不得再触碰 Nono 或隐藏原版 Body
            ResetCounters();
            HarnessConfig = BuildDefaultConfig();
            HarnessArt["idle_00.png"] = idleFrame0;
            HarnessBuiltFrames = framesObj;

            _activeProvider = "NailongFool";
            _activeAssembly = otherAssembly;
            IndependentFoolSkinCompat.ApplyPreview(visuals, "fool");

            _activeProvider = null;
            _activeAssembly = null;
            IndependentFoolSkinCompat.ApplyPreview(visuals, "fool");

            _activeProvider = "NonoFoolSkin";
            _activeAssembly = nonoAssembly;
            IndependentFoolSkinCompat.ApplyPreview(visuals, "ironclad");

            Require(HarnessRigCreateCount == 0 && HarnessShowFramesCount == 0 &&
                    HarnessShowStaticCount == 0 && _vanillaHideCount == 0 &&
                    HarnessRigs.Count == 0 && ReferenceEquals(rigField.GetValue(null), liveCombatRig),
                "切走至其他愚者皮肤、默认皮肤或非愚者分组时不得残留或触发 Nono 预览逻辑。");
        }
        finally
        {
            harmony.UnpatchAll(harmony.Id);
            _activeProvider = null;
            _activeAssembly = null;
            _vanillaBody = null;
            HarnessRigs.Clear();
            HarnessArt.Clear();
        }
    }

    private static FakeSkinConfig BuildDefaultConfig() => new()
    {
        Layout = new FakeLayoutConfig(),
        Animations = new Dictionary<string, FakeAnimConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["idle"] = new FakeAnimConfig
            {
                Mode = "frames",
                Prefix = "idle_",
                Speed = 1.0f,
                Loop = true,
                Fallback = "idle_static"
            },
            ["idle_static"] = new FakeAnimConfig
            {
                Mode = "static",
                File = "character.png",
                EyesClosed = "character_eyes_closed.png"
            }
        }
    };

    private static void ResetCounters()
    {
        HarnessInitCount = 0;
        HarnessRigCreateCount = 0;
        HarnessBuildFramesCount = 0;
        HarnessShowFramesCount = 0;
        HarnessShowStaticCount = 0;
        HarnessLastRig = null;
        HarnessLastBuildName = null;
        HarnessLastSpeed = 0;
        HarnessLastLoop = false;
        HarnessLastTargetHeight = null;
        HarnessLastReturnTo = null;
        HarnessLastStaticOpen = null;
        HarnessLastStaticWithIdle = false;
        _vanillaHideCount = 0;
        HarnessRigs.Clear();
    }

    private static Assembly BuildSyntheticNonoAssembly()
    {
        var asm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("NonoFoolSkin"), AssemblyBuilderAccess.Run);
        var mod = asm.DefineDynamicModule("NonoFoolSkin");

        // NonoFoolSkin.ArtStore
        var artType = mod.DefineType("NonoFoolSkin.ArtStore", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        var artGet = artType.DefineMethod("Get", MethodAttributes.Public | MethodAttributes.Static, typeof(Texture2D), [typeof(string)]);
        var artIl = artGet.GetILGenerator();
        artIl.Emit(OpCodes.Ldarg_0);
        artIl.Emit(OpCodes.Call, typeof(NonoFoolPreviewTests).GetMethod(nameof(HarnessGetArt))!);
        artIl.Emit(OpCodes.Ret);
        artType.CreateType();

        // NonoFoolSkin.Rig (inherits FakeRig for ShowFrames / ShowStatic / Closed)
        var rigType = mod.DefineType("NonoFoolSkin.Rig", TypeAttributes.Public, typeof(FakeRig));
        var rigsField = rigType.DefineField("Rigs", typeof(Dictionary<ulong, object>), FieldAttributes.Private | FieldAttributes.Static);
        var rigCctor = rigType.DefineTypeInitializer();
        var rigCctorIl = rigCctor.GetILGenerator();
        rigCctorIl.Emit(OpCodes.Ldsfld, typeof(NonoFoolPreviewTests).GetField(nameof(HarnessRigs))!);
        rigCctorIl.Emit(OpCodes.Stsfld, rigsField);
        rigCctorIl.Emit(OpCodes.Ret);
        var createRig = rigType.DefineMethod(
            "Create",
            MethodAttributes.Public | MethodAttributes.Static,
            typeof(FakeRig),
            [typeof(NCreatureVisuals), typeof(Texture2D), typeof(Texture2D), typeof(FakeLayoutConfig)]);
        var createIl = createRig.GetILGenerator();
        createIl.Emit(OpCodes.Ldarg_0);
        createIl.Emit(OpCodes.Ldarg_1);
        createIl.Emit(OpCodes.Ldarg_2);
        createIl.Emit(OpCodes.Ldarg_3);
        createIl.Emit(OpCodes.Call, typeof(NonoFoolPreviewTests).GetMethod(nameof(HarnessCreateRig))!);
        createIl.Emit(OpCodes.Ret);
        rigType.CreateType();

        // NonoFoolSkin.AnimDirector
        var dirType = mod.DefineType("NonoFoolSkin.AnimDirector", TypeAttributes.Public | TypeAttributes.Abstract | TypeAttributes.Sealed);
        dirType.DefineField("_rig", typeof(FakeRig), FieldAttributes.Private | FieldAttributes.Static);

        var configProp = dirType.DefineProperty("Config", PropertyAttributes.None, typeof(FakeSkinConfig), null);
        var getConfig = dirType.DefineMethod("get_Config", MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.SpecialName, typeof(FakeSkinConfig), Type.EmptyTypes);
        var getCfgIl = getConfig.GetILGenerator();
        getCfgIl.Emit(OpCodes.Ldsfld, typeof(NonoFoolPreviewTests).GetField(nameof(HarnessConfig))!);
        getCfgIl.Emit(OpCodes.Ret);
        configProp.SetGetMethod(getConfig);

        var initMethod = dirType.DefineMethod("Initialize", MethodAttributes.Public | MethodAttributes.Static, typeof(void), Type.EmptyTypes);
        var initIl = initMethod.GetILGenerator();
        initIl.Emit(OpCodes.Call, typeof(NonoFoolPreviewTests).GetMethod(nameof(HarnessInitialize))!);
        initIl.Emit(OpCodes.Ret);

        var buildFrames = dirType.DefineMethod("BuildFrames", MethodAttributes.Private | MethodAttributes.Static, typeof(SpriteFrames), [typeof(string), typeof(FakeAnimConfig)]);
        var buildIl = buildFrames.GetILGenerator();
        buildIl.Emit(OpCodes.Ldarg_0);
        buildIl.Emit(OpCodes.Ldarg_1);
        buildIl.Emit(OpCodes.Call, typeof(NonoFoolPreviewTests).GetMethod(nameof(HarnessBuildFrames))!);
        buildIl.Emit(OpCodes.Ret);
        dirType.CreateType();

        return asm;
    }

    private static bool PatchGetProvider(string groupId, ref string? __result)
    {
        __result = groupId == "fool" ? _activeProvider : null;
        return false;
    }

    private static bool PatchGetAssembly(string providerId, ref Assembly? __result)
    {
        __result = providerId == _activeProvider ? _activeAssembly : null;
        return false;
    }

    private static bool PatchCurrentBody(ref CanvasItem? __result)
    {
        __result = _vanillaBody;
        return false;
    }

    private static bool PatchHide(CanvasItem __instance)
    {
        if (ReferenceEquals(__instance, _vanillaBody))
        {
            _vanillaHideCount++;
            return false;
        }
        return true;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}

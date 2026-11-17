using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Core;
using STS2SkinChanger.Ui;

internal static class CharacterStripPortraitTests
{
    private static CompressedTexture2D _open = null!, _locked = null!;
    private static CharacterModel _character = null!;
    private static NCharacterSelectButton _button = null!;
    private static int _init, _reads;
    internal static void Run(string workshop)
    {
        var waifu = Assembly.LoadFrom(Path.Combine(workshop, "3838977271/Fool_Waifu.dll"));
        var single = Assembly.LoadFrom(Path.Combine(workshop, "3838976517/FoolSkinStatic.dll"));
        var waifuContracts = CharacterStripPortraits.ReadContracts(waifu);
        var singleContracts = CharacterStripPortraits.ReadContracts(single);
        Require(waifuContracts.Count(c => c.Target.Name == "get_CharacterSelectIcon") == 1,
            "Dormant Waifu getter not discovered before selection.");
        Require(waifuContracts.Count(c => c.Target.Name == "get_CharacterSelectLockedIcon") == 1,
            "Locked getter missing.");
        Require(singleContracts.Count(c => c.Target.Name == "Init") == 1,
            "Static portrait Init postfix not discovered before selection.");
        Require(waifuContracts.Concat(singleContracts).All(c => c.Target.Name is
            "Init" or "get_CharacterSelectIcon" or "get_CharacterSelectLockedIcon"),
            "Icon read acquired gameplay/background callbacks.");
        Require(ReferenceEquals(waifuContracts, CharacterStripPortraits.ReadContracts(waifu)), "Uncached declarations.");
        if (typeof(CharacterModel).Assembly.GetType("MegaCrit.Sts2.Core.Models.Characters.Fool") == null)
        {
            Console.WriteLine("Character strip portrait metadata passed on stable; beta-only Fool callback execution deferred to beta runner.");
            return;
        }
        _open = Fake<CompressedTexture2D>(); _locked = Fake<CompressedTexture2D>();
        _character = Fake<Ironclad>(); _button = Fake<NCharacterSelectButton>();
        var harmony = new Harmony("sc.tests.character-strip-portraits");
        try
        {
            harmony.Patch(AccessTools.Method(waifu.GetType("FoolWaifu.SkinVisuals"), "IsFool"),
                prefix: new HarmonyMethod(typeof(CharacterStripPortraitTests), nameof(IsFool)));
            harmony.Patch(AccessTools.Method(waifu.GetType("FoolWaifu.SkinAssets"), "PackedUiTexture"),
                prefix: new HarmonyMethod(typeof(CharacterStripPortraitTests), nameof(Packed)));
            var init = singleContracts.Single(c => c.Target.Name == "Init");
            harmony.Patch(init.Method, prefix: new HarmonyMethod(typeof(CharacterStripPortraitTests), nameof(Init)));
            for (var i = 0; i < 200; i++)
            {
                Require(ReferenceEquals(CharacterStripPortraits.ReplayGetter(waifu, _character, false, _locked), _open),
                    "Unfocused character retained vanilla/stale portrait.");
                Require(ReferenceEquals(CharacterStripPortraits.ReplayGetter(waifu, _character, true, _open), _locked),
                    "Locked character acquired unlocked art.");
                ManagedSkinModLoader.ReplayCharacterButtonIcon(init.Method, init.Target, _button, _character);
            }
            Require(_reads == 400 && _init == 200, "Author contracts did not execute.");
            var refresh = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(ContextualSkinControls),
                "RefreshAllCharacterButtonIcons"));
            Require(refresh.Any(i => i.operand is MethodBase m && m.DeclaringType == typeof(CharacterStripPortraits) &&
                m.Name == "Refresh"), "Character strip bypasses selected provider portrait lifecycle.");
            var read = PatchProcessor.GetOriginalInstructions(AccessTools.Method(typeof(CharacterStripPortraits), "Refresh"));
            Require(!read.Any(i => i.operand is MethodBase m &&
                (m.Name.Contains("FocusRuntime") || m.Name == "SelectCharacter" || m.Name == "Initialize")),
                "Reading unfocused portraits must not change focus/lobby/provider activation.");
            Console.WriteLine("Character strip portraits passed: actual dormant Waifu getters, static Init binding, " +
                "400 locked/unlocked reads and 200 Init replays without selecting/initializing providers.");
        }
        finally { harmony.UnpatchAll(harmony.Id); }
    }
    private static bool IsFool(CharacterModel __0, ref bool __result)
    { __result = ReferenceEquals(__0, _character); return false; }
    private static bool Packed(string __0, ref CompressedTexture2D __result)
    { _reads++; __result = __0 == "select_icon_locked.png" ? _locked : _open; return false; }
    private static bool Init(NCharacterSelectButton __0, CharacterModel __1)
    {
        Require(ReferenceEquals(__0, _button) && ReferenceEquals(__1, _character), "Init binding changed character/button.");
        _init++; return false;
    }
    private static T Fake<T>() where T : class
    { var value = (T)RuntimeHelpers.GetUninitializedObject(typeof(T)); GC.SuppressFinalize(value); return value; }
    private static void Require(bool condition, string message)
    { if (!condition) throw new InvalidOperationException(message); }
}

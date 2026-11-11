using STS2SkinChanger.Core;

internal static class FakeMerchantEventArtPolicyTests
{
    public static void Run()
    {
        static void Require(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException(reason);
        }

        Require(
            FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                FakeMerchantEventArtPolicy.EncounterBackgroundScene),
            "假商人战斗背景场景必须保留游戏原路径，否则战斗会绑到我们副本里的原版贴图。");
        Require(
            FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                "res://scenes/backgrounds/fake_merchant_event_encounter/layers/fake_merchant_event_encounter_bg_00_a.tscn"),
            "背景层场景同样属于事件图，不能写原路径重定向。");
        Require(
            FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                "res://animations/backgrounds/fake_merchant_room/bottom/shop_fake_merchant_bottom.png"),
            "帐篷底层贴图必须由事件包决定，不能被运行包的原版副本顶掉。");

        Require(
            !FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                "res://animations/backgrounds/fake_merchant_room/top/fake_merchant_top.tres"),
            "商人本体（top）仍归商人皮肤，必须继续走私有重定向。");
        Require(
            !FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                "res://animations/backgrounds/fake_merchant_room/hand/fake_merchant_hand_skel_data.tres"),
            "商人手部仍归商人皮肤，必须继续走私有重定向。");
        Require(
            !FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                "res://scenes/backgrounds/fake_merchant_event/other.tscn") &&
            !FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(
                "res://scenes/backgrounds/hive/hive_background.tscn"),
            "其它背景场景不受假商人规则影响。");
        Require(
            !FakeMerchantEventArtPolicy.SuppressesCanonicalRedirect(null) &&
            !FakeMerchantEventArtPolicy.IsRedrawnArtPath(string.Empty),
            "空路径必须安全返回 false。");
        Require(
            FakeMerchantEventArtPolicy.IsRedrawnArtPath(
                @"res://scenes\backgrounds\fake_merchant_event_encounter\x.tscn"),
            "反斜杠路径也必须识别为事件图路径。");

        Console.WriteLine(
            "Fake merchant event art policy passed: canonical tent paths stay canonical, merchant body keeps its redirects.");
    }
}

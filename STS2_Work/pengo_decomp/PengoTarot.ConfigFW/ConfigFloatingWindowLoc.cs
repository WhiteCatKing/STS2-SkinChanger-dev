using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Divination;

namespace PengoTarot.ConfigFW;

public static class ConfigFloatingWindowLoc
{
	public const int ThanksUnlockOpens = 3;

	private static readonly string[] EmptyTips = Array.Empty<string>();

	private static readonly string[] ChineseMainTips = new string[14]
	{
		"Mod的配置入口总是会显示在游戏自己的设置界面中。", "你可以开启面板下方的按钮来在一局游戏中隐藏配置入口。", "塔罗牌卡包跳过时，不会扣除金币，你可以随时返回商店购买心仪的卡牌。", "火堆的观星选项跳过时，并不会消耗行动和观星套组的可用次数。", "如果塔罗牌和星球牌的总开关都被关闭，Mod会在下次你启动游戏时将自己标记为“不影响游戏玩法”，不过再次开启总开关也需要重启。（这个功能暂时还没有实现！）", "如果你想试试在一张牌上多次附魔，不妨试试创意工坊的MultiEnchantment。", "如果你想以优雅的方式浏览所有原版和Mod附魔，不妨试试创意工坊的Enchantment Compendium。", "你可以在图鉴左下角开启卡牌3D效果，这很小丑牌！", "你可以在图鉴点开一张牌的详情，然后点击右下角入口，开始设置卡牌专用的特效。", "如果你发现自己的卡牌出现部件错位甚至消失，请尝试关闭本Mod的所有特效内容！包括3D！",
		"推荐完全勾选塔罗牌前五个配置开关游玩本Mod！", "如果你觉得塔罗牌的效果太强了，不妨关掉女祭司的配置开关。", "希望你享受星球牌带来的混乱体验！", "如果喜欢这个Mod，在Steam创意工坊上点个赞吧！"
	};

	private static readonly string[] ChineseThanksTips = new string[13]
	{
		"感谢SeungjunKim_8224的韩语本地化支持。", "我的邮箱是3411737922@qq.com，你也可以直接添加我的QQ。", "感谢Balatri作者精美的游戏美术！", "感谢OLC开源的RItsulib项目！", "本Mod的多版本支持学习于Ritsulib。", "本Mod的多人联机配置同步学习于Ritsulib。", "感谢Ind_E开源的Balatoeffect项目！", "感谢我的朋友们！但是你们的名字太长了！", "死神-逆在最开始的设计是被附魔的牌本身也不能抽牌，后来我意识到这有多蠢。", "教皇-逆可以降级永世沙漏的凋亡卡牌，不过不会反过来升级它们。",
		"愚者-正附魔能力牌的实际实现是克隆一张牌，这可能存在某些小技巧……", "星星-逆和天际钻头的互动有非常多的额外代码！", "星星逆可以给原本没有星星消耗的牌附魔。"
	};

	private static readonly string[] EnglishMainTips = new string[14]
	{
		"The Mod's config entry always appears in the game's own Settings screen.", "You can hide the config entry for a run by enabling the button below the panel.", "Skipping a Tarot pack doesn't deduct gold; you can always return to the shop and buy the cards you want.", "Skipping the campfire Stargazer option doesn't consume an action or the Stargazer deck's uses.", "If both the Tarot and Planet master toggles are off, the Mod marks itself as 'does not affect gameplay' on next launch; re-enabling them also requires a restart. (Not implemented yet!)", "Want to enchant a card multiple times? Try MultiEnchantment on the Workshop.", "Want to browse all vanilla and Mod enchantments in style? Try Enchantment Compendium on the Workshop.", "You can enable 3D card effects in the bottom-left of the Card Library - very Balatro!", "In the Card Library, open a card's details, then click the entry in the bottom-right to set up per-card effects.", "If your cards glitch or vanish, try turning off all of this Mod's effects, including 3D!",
		"We recommend fully enabling the first five Tarot config toggles to play this Mod!", "If you find the Tarot effects too strong, try disabling the High Priestess toggle.", "Enjoy the chaos that Planet cards bring!", "If you like this Mod, give it a like on the Steam Workshop!"
	};

	private static readonly string[] EnglishThanksTips = new string[13]
	{
		"Thanks to SeungjunKim_8224 for the Korean localization support.", "My email is 3411737922@qq.com; you can also add my QQ directly.", "Thanks to the Balatri author for the beautiful game art!", "Thanks to OLC for the open-source RItsulib project!", "This Mod's multi-version support was learned from Ritsulib.", "This Mod's multiplayer config sync was learned from Ritsulib.", "Thanks to Ind_E for the open-source Balatoeffect project!", "Thanks to my friends! But your names are too long!", "Death-Reversed was originally designed so the enchanted card itself couldn't draw; later I realized how dumb that was.", "Hierophant-Reversed can downgrade the Decaying cards of the Everburning Hourglass, but won't upgrade them back.",
		"The Fool (upright) enchantment on Power cards is actually implemented by cloning a card; there may be some tricks to it...", "The Star-Reversed interaction with the Skybound Drill has a ton of extra code!", "Star-Reversed can enchant cards that don't naturally have a Star cost."
	};

	private static readonly string[] JapaneseMainTips = new string[14]
	{
		"Modの設定入口は常にゲーム本体の設定画面に表示されます。", "パネル下のボタンをオンにすると、そのラン中は設定入口を非表示にできます。", "タロットパックをスキップしてもゴールドは減りません。いつでもショップに戻って好きなカードを購入できます。", "焚き火の占星オプションをスキップしても、アクションや占星デッキの使用回数は消費されません。", "タロットとプラネットの両方のマスタースイッチをオフにすると、次回起動時にModが「ゲームプレイに影響なし」とマークします。再度オンにするには再起動が必要です。（まだ未実装！）", "1枚のカードに複数回エンチャントしてみたいなら、WorkshopのMultiEnchantmentをどうぞ。", "すべてのバニラおよびModエンチャントを優雅に閲覧したいなら、WorkshopのEnchantment Compendiumをどうぞ。", "図鑑の左下でカード3D効果を有効にできます。まるでBalatro！", "図鑑でカードの詳細を開き、右下の入口をクリックしてカード専用のエフェクトを設定できます。", "カードがずれたり消えたりしたら、3Dを含む本Modのすべてのエフェクトを無効にしてみてください！",
		"本Modをプレイするなら、タロットの最初の5つの設定スイッチをすべてオンにすることをおすすめします！", "タロットの効果が強すぎると感じたら、女教皇の設定スイッチをオフにしてみてください。", "プラネットカードがもたらすカオスをお楽しみください！", "このModが気に入ったら、Steam Workshopでいいねをお願いします！"
	};

	private static readonly string[] JapaneseThanksTips = new string[13]
	{
		"SeungjunKim_8224さんの韓国語ローカライズ支援に感謝します。", "メールアドレスは3411737922@qq.comです。QQに直接追加してもらっても構いません。", "Balatri作者の美しいゲームアートに感謝します！", "OLC氏のオープンソースRItsulibプロジェクトに感謝します！", "本Modのマルチバージョン対応はRitsulibから学びました。", "本Modのマルチプレイ設定同期はRitsulibから学びました。", "Ind_E氏のオープンソースBalatoeffectプロジェクトに感謝します！", "友達に感謝！でも名前が長すぎる！", "死神-逆は当初、エンチャントされたカード自体もドローできなくなる設計でしたが、それがどれだけ馬鹿げているかに後で気づきました。", "教皇-逆は永遠の砂時計の凋零カードを下位にできますが、逆に上位には戻しません。",
		"愚者-正のパワーカードへのエンチャントは、実際にはカードの複製として実装されており、小技があるかもしれません……", "星-逆とスカイボーンドリルの相互作用には非常に多くの追加コードがあります！", "星-逆はもともと星コストを持たないカードにもエンチャントできます。"
	};

	private static readonly string[] KoreanMainTips = new string[14]
	{
		"Mod 설정 입구는 항상 게임 자체 설정 화면에 표시됩니다.", "패널 아래 버튼을 켜면 이번 런 동안 설정 입구를 숨길 수 있습니다.", "타로 팩을 건너뛰어도 골드가 차감되지 않습니다. 언제든 상점으로 돌아와 원하는 카드를 구매할 수 있습니다.", "야영지의 점성술 옵션을 건너뛰어도 행동과 점성술 덱의 사용 횟수가 소모되지 않습니다.", "타로와 플래닛 마스터 스위치가 모두 꺼져 있으면 다음 시작 시 Mod가 '게임플레이에 영향 없음'으로 표시됩니다. 다시 켜려면 재시작이 필요합니다. (아직 미구현!)", "한 장의 카드에 여러 번 인챈트해 보고 싶다면 워크숍의 MultiEnchantment를 사용해 보세요.", "모든 기본 및 Mod 인챈트를 우아하게 둘러보고 싶다면 워크숍의 Enchantment Compendium을 사용해 보세요.", "도감 왼쪽 아래에서 카드 3D 효과를 켤 수 있습니다. 정말 Balatro스럽죠!", "도감에서 카드 상세를 열고 오른쪽 아래 입구를 클릭해 카드 전용 효과를 설정할 수 있습니다.", "카드가 어긋나거나 사라지면 3D를 포함한 이 Mod의 모든 효과를 꺼 보세요!",
		"이 Mod를 플레이할 때는 타로 설정 스위치 앞의 5개를 모두 켜는 것을 추천합니다!", "타로 효과가 너무 강하다고 느끼면 여사제 설정 스위치를 꺼 보세요.", "플래닛 카드가 가져오는 혼돈을 즐기세요!", "이 Mod가 마음에 든다면 Steam 워크숍에서 좋아요를 눌러 주세요!"
	};

	private static readonly string[] KoreanThanksTips = new string[13]
	{
		"한국어 로컬라이제이션을 지원해 주신 SeungjunKim_8224님께 감사드립니다.", "제 이메일은 3411737922@qq.com입니다. QQ를 직접 추가하셔도 됩니다.", "Balatri 작가님의 아름다운 게임 아트에 감사드립니다!", "OLC님의 오픈소스 RItsulib 프로젝트에 감사드립니다!", "이 Mod의 다중 버전 지원은 Ritsulib에서 배웠습니다.", "이 Mod의 멀티플레이 설정 동기화는 Ritsulib에서 배웠습니다.", "Ind_E님의 오픈소스 Balatoeffect 프로젝트에 감사드립니다!", "제 친구들에게 감사합니다! 하지만 여러분의 이름은 너무 깁니다!", "사신-역방향은 원래 인챈트된 카드 자체도 드로우할 수 없도록 설계되었지만, 나중에 그게 얼마나 어리석은지 깨달았습니다.", "교황-역방향은 영원한 모래시계의 쇠퇴 카드를 하위로 낮출 수 있지만, 반대로 상위로 올리지는 않습니다.",
		"광대-정방향의 파워 카드 인챈트는 실제로 카드를 복제하는 방식으로 구현되어 있으며, 약간의 꼼수가 있을 수 있습니다……", "별-역방향과 스카이본드 드릴의 상호작용에는 매우 많은 추가 코드가 있습니다!", "별-역방향은 원래 별 비용이 없는 카드에도 인챈트할 수 있습니다."
	};

	public static IReadOnlyList<string> GetMainTips(string language)
	{
		return language switch
		{
			"zhs" => ChineseMainTips, 
			"eng" => EnglishMainTips, 
			"jpn" => JapaneseMainTips, 
			"kor" => KoreanMainTips, 
			_ => EmptyTips, 
		};
	}

	public static IReadOnlyList<string> GetThanksTips(string language)
	{
		return language switch
		{
			"zhs" => ChineseThanksTips, 
			"eng" => EnglishThanksTips, 
			"jpn" => JapaneseThanksTips, 
			"kor" => KoreanThanksTips, 
			_ => EmptyTips, 
		};
	}

	public static bool IsMarkedDivination(int flagIndex)
	{
		if ((uint)(flagIndex - 7) <= 2u || (uint)(flagIndex - 11) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool HasDynamicSettingsDescription(int flagIndex)
	{
		if (flagIndex != 10)
		{
			return IsMarkedDivination(flagIndex);
		}
		return true;
	}

	public static bool IsEliteDivination(int flagIndex)
	{
		if ((uint)(flagIndex - 7) <= 2u)
		{
			return true;
		}
		return false;
	}

	private static string MarkedDivinationName(int flagIndex)
	{
		return flagIndex switch
		{
			7 => "CHARIOT", 
			8 => "STRENGTH", 
			9 => "HERMIT", 
			11 => "JUSTICE", 
			12 => "HANGEDMAN", 
			13 => "DEATH", 
			_ => string.Empty, 
		};
	}

	private static string Loc(string key)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return new LocString("gameplay_ui", key).GetFormattedText() ?? key;
	}

	public static string BuildSettingsDescription(int flagIndex)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		if (flagIndex == 10)
		{
			return BuildWheelOfFortuneSettingsDescription();
		}
		string text = MarkedDivinationName(flagIndex);
		if (string.IsNullOrEmpty(text))
		{
			return string.Empty;
		}
		string text2 = Loc("BAL_CFW_FLAG_" + text + "_DESC");
		if (!RunManager.Instance.IsInProgress)
		{
			return text2;
		}
		int completedCount = TarotMarkerSystem.GetCompletedCount(flagIndex);
		LocString val = new LocString("gameplay_ui", "BAL_CFW_PROGRESS_LINE");
		val.Add("Expired", IsEliteDivination(flagIndex) && completedCount >= 2);
		val.Add("Count", (decimal)completedCount);
		string text3 = val.GetFormattedText() ?? "BAL_CFW_PROGRESS_LINE";
		return text2 + "\n" + text3;
	}

	private static string BuildWheelOfFortuneSettingsDescription()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		string text = Loc("BAL_CFW_FLAG_WHEELOFFORTUNE_DESC");
		RunState val = RunManager.Instance.DebugOnlyGetState();
		if (!RunManager.Instance.IsInProgress || val == null)
		{
			return text;
		}
		Player me = LocalContext.GetMe((IPlayerCollection)(object)val);
		if (me == null)
		{
			return text;
		}
		int num = ConfigFloatingWindowRunData.GetWheelOfFortuneCardCount(me.NetId) % 5;
		int num2 = 5 - num;
		LocString val2 = new LocString("gameplay_ui", "BAL_CFW_WHEEL_PROGRESS");
		val2.Add("Count", (decimal)num2);
		return text + "\n" + (val2.GetFormattedText() ?? "BAL_CFW_WHEEL_PROGRESS");
	}

	public static LocString? BuildMapDescription(int flagIndex)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		string text = MarkedDivinationName(flagIndex);
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		LocString val = new LocString("gameplay_ui", "BAL_CFW_MAP_" + text);
		if (IsEliteDivination(flagIndex))
		{
			int completedCount = TarotMarkerSystem.GetCompletedCount(flagIndex);
			val.Add("Expired", completedCount >= 2);
			val.Add("Completed", completedCount >= 1);
		}
		else
		{
			val.Add("Completed", TarotMarkerSystem.GetProgressForDisplay(flagIndex) >= 1);
		}
		return val;
	}
}

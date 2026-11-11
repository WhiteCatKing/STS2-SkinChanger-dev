using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;

namespace PengoTarot.BalatroEffect;

public class InspectScreen
{
	[HarmonyPatch(typeof(NInspectCardScreen), "Open")]
	private static class OpenPatch
	{
		public static void Postfix(int index, List<CardModel> cards)
		{
			if (cards != null && index >= 0 && index < cards.Count)
			{
				_currentOriginIndex = index;
				_currentOriginCards = cards;
			}
		}
	}

	[HarmonyPatch(typeof(NInspectCardScreen), "SetCard")]
	private static class SetCardPatch
	{
		public static void Postfix(NInspectCardScreen __instance, int index, List<CardModel> ____cards)
		{
			if (____cards == null || index < 0 || index >= ____cards.Count)
			{
				return;
			}
			_currentOriginIndex = index;
			_currentOriginCards = ____cards;
			AddEntryButton((Control)(object)__instance, ____cards, index);
			NCard card = Traverse.Create((object)__instance).Field("_card").GetValue<NCard>();
			SceneTree tree = default(SceneTree);
			if (card != null && GodotObject.IsInstanceValid((GodotObject)(object)card) && card.Model != null)
			{
				ref SceneTree reference = ref tree;
				MainLoop mainLoop = Engine.GetMainLoop();
				reference = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
				if (tree != null)
				{
					tree.ProcessFrame += Reapply;
				}
			}
			void Reapply()
			{
				tree.ProcessFrame -= Reapply;
				if (GodotObject.IsInstanceValid((GodotObject)(object)card))
				{
					ShaderController.ApplyShader(card);
				}
			}
		}
	}

	private static int _currentOriginIndex;

	private static List<CardModel>? _currentOriginCards;

	private static List<EffectRegistry.EffectDef> _pgEffects = new List<EffectRegistry.EffectDef>();

	private static int _pgCurrentIndex;

	private static Label? _pgLabel;

	private static bool _pgInitialized;

	private static readonly string _entryBtnScene = "res://balatroeffect/Scenes/balatro_entry_btn.tscn";

	public static string CurrentCardId { get; set; } = "";

	public static int CurrentEffectIndex { get; set; }

	public static string SeparatelyHighlight { get; set; } = "";

	public static string Tr(string key)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return new LocString("gameplay_ui", "BAL_" + key).GetFormattedText() ?? key;
	}

	public static void InitPaginator(Label label)
	{
		_pgLabel = label;
		if (_pgInitialized)
		{
			return;
		}
		EffectRegistry.Initialize();
		_pgEffects.Clear();
		foreach (EffectRegistry.EffectDef allEffect in EffectRegistry.AllEffects)
		{
			_pgEffects.Add(allEffect);
		}
		_pgCurrentIndex = 0;
		_pgInitialized = true;
		UpdatePaginatorLabel();
		OnPaginatorIndexChanged();
	}

	public static void UpdatePaginatorTarget(string cardId)
	{
		CurrentCardId = cardId;
		int currentMode = Config.GetCardEffectMode(cardId);
		int num = _pgEffects.FindIndex((EffectRegistry.EffectDef d) => d.Mode == currentMode);
		_pgCurrentIndex = ((num >= 0) ? num : 0);
		UpdatePaginatorLabel();
		OnPaginatorIndexChanged();
	}

	public static void OnPaginatorNavigate(int delta)
	{
		int num = _pgCurrentIndex + delta;
		if (num < 0)
		{
			num = _pgEffects.Count - 1;
		}
		else if (num >= _pgEffects.Count)
		{
			num = 0;
		}
		_pgCurrentIndex = num;
		UpdatePaginatorLabel();
		OnPaginatorIndexChanged();
		Config.SetCardEffectMode(CurrentCardId, _pgEffects[_pgCurrentIndex].Mode);
		ApplyCurrentEffectToCheckedParts();
		RefreshCurrentScreen();
		ShaderController.RefreshAllCardsWithId(CurrentCardId);
	}

	private static void SetPaginatorToMode(int mode)
	{
		int num = _pgEffects.FindIndex((EffectRegistry.EffectDef d) => d.Mode == mode);
		if (num >= 0)
		{
			_pgCurrentIndex = num;
			UpdatePaginatorLabel();
			OnPaginatorIndexChanged();
		}
	}

	private static void UpdatePaginatorLabel()
	{
		if (_pgLabel != null && _pgInitialized)
		{
			string text = "BAL_" + _pgEffects[_pgCurrentIndex].LocKey;
			LocManager instance = LocManager.Instance;
			string text2 = ((instance != null) ? instance.GetTable("gameplay_ui").GetRawText(text) : null);
			_pgLabel.Text = ((!string.IsNullOrEmpty(text2)) ? text2 : _pgEffects[_pgCurrentIndex].LocKey);
		}
	}

	private static void OnPaginatorIndexChanged()
	{
		CurrentEffectIndex = _pgEffects[_pgCurrentIndex].Mode;
		RefreshScreenSlider();
	}

	public static void AddEntryButton(Control root, List<CardModel> cards, int index)
	{
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		if (((Node)root).HasNode(NodePath.op_Implicit("BalatroEntryBtnRoot")))
		{
			return;
		}
		Node val = GD.Load<PackedScene>(_entryBtnScene).Instantiate((GenEditState)0);
		((Node)root).AddChild(val, false, (InternalMode)0);
		NButton btn = val.GetNode<NButton>(NodePath.op_Implicit("Button"));
		Label node = ((Node)btn).GetNode<Label>(NodePath.op_Implicit("Label"));
		node.Text = Tr("BTN_ENTRY");
		FontControlUtils.ApplyLocaleFontSubstitution((Control)(object)node, (FontType)0, StringName.op_Implicit("font"));
		((CanvasItem)btn).Modulate = new Color(1f, 1f, 1f, 1f);
		((Control)btn).Scale = Vector2.One;
		((Control)btn).PivotOffset = ((Control)btn).Size / 2f;
		Tween fadeOutTween = ((Node)btn).CreateTween();
		fadeOutTween.TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 1.0).SetDelay(8.0);
		Tween scaleTween = null;
		((Control)btn).MouseEntered += delegate
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			Tween obj = fadeOutTween;
			if (obj != null && obj.IsValid())
			{
				fadeOutTween.Kill();
			}
			((Control)btn).PivotOffset = ((Control)btn).Size / 2f;
			((Node)btn).CreateTween().TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 0.10000000149011612);
			Tween obj2 = scaleTween;
			if (obj2 != null && obj2.IsValid())
			{
				scaleTween.Kill();
			}
			scaleTween = ((Node)btn).CreateTween();
			scaleTween.TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("scale"), Variant.op_Implicit(new Vector2(1.02f, 1.02f)), 0.10000000149011612);
		};
		((Control)btn).MouseExited += delegate
		{
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			Tween obj3 = fadeOutTween;
			if (obj3 != null && obj3.IsValid())
			{
				fadeOutTween.Kill();
			}
			fadeOutTween = ((Node)btn).CreateTween();
			fadeOutTween.TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(0f), 1.0).SetDelay(2.0);
			Tween obj4 = scaleTween;
			if (obj4 != null && obj4.IsValid())
			{
				scaleTween.Kill();
			}
			((Control)btn).PivotOffset = ((Control)btn).Size / 2f;
			scaleTween = ((Node)btn).CreateTween();
			scaleTween.TweenProperty((GodotObject)(object)btn, NodePath.op_Implicit("scale"), Variant.op_Implicit(Vector2.One), 0.20000000298023224);
		};
		WireEntryBtn(btn, root, cards);
	}

	private static void WireEntryBtn(NButton btn, Control root, List<CardModel> cards)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)btn).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			Node parent = ((Node)btn).GetParent();
			NInspectCardScreen val = null;
			for (Node val2 = parent; val2 != null; val2 = val2.GetParent())
			{
				NInspectCardScreen val3 = (NInspectCardScreen)(object)((val2 is NInspectCardScreen) ? val2 : null);
				if (val3 != null)
				{
					val = val3;
					break;
				}
			}
			string currentId = null;
			if (val != null)
			{
				Node obj = ((Node)val).FindChild("Card", true, false);
				NCard val4 = (NCard)(object)((obj is NCard) ? obj : null);
				object obj2;
				if (val4 == null)
				{
					obj2 = null;
				}
				else
				{
					CardModel model = val4.Model;
					obj2 = ((model != null) ? ((object)((AbstractModel)model).Id).ToString() : null);
				}
				currentId = (string)obj2;
			}
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val5 = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			Node obj3 = ((val5 != null) ? ((Node)val5.Root).FindChild("CardLibraryScreen", true, false) : null);
			Node obj4 = ((obj3 != null) ? obj3.FindChild("CardGrid", true, false) : null);
			NCardLibraryGrid val6 = (NCardLibraryGrid)(object)((obj4 is NCardLibraryGrid) ? obj4 : null);
			List<CardModel> list = (List<CardModel>)((((val6 != null) ? val6.VisibleCards : null) != null) ? ((IList)new List<CardModel>(val6.VisibleCards)) : ((IList)cards));
			if (currentId != null)
			{
				int num = list.FindIndex((CardModel c) => ((object)((AbstractModel)c).Id).ToString() == currentId);
				_ = 0;
			}
			if (val != null)
			{
				val.Close();
			}
			NBalatroInspectScreen nBalatroInspectScreen = NBalatroInspectScreen.Create();
			if (nBalatroInspectScreen != null)
			{
				if (val5 != null)
				{
					((Node)val5.Root).AddChild((Node)(object)nBalatroInspectScreen, false, (InternalMode)0);
				}
				nBalatroInspectScreen.Open(_currentOriginCards ?? cards, _currentOriginIndex, viewAllUpgraded: false, val);
			}
		}), 0u);
	}

	public static void OnEditModeSelected(string mode)
	{
		if (!string.IsNullOrEmpty(CurrentCardId))
		{
			Config.SetEditMode(CurrentCardId, mode);
			if (mode == "separately")
			{
				SeparatelyHighlight = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
				SetPaginatorToMode(Config.GetEffect(CurrentCardId, SeparatelyHighlight));
			}
			RefreshCurrentScreen();
			ShaderController.RefreshAllCardsWithId(CurrentCardId);
		}
	}

	public static void OnPartClicked(string partName)
	{
		if (string.IsNullOrEmpty(CurrentCardId))
		{
			return;
		}
		string editMode = Config.GetEditMode(CurrentCardId);
		if (editMode == "fully")
		{
			return;
		}
		if (editMode == "separately")
		{
			if (Config.GetEffect(CurrentCardId, partName) > 0 && SeparatelyHighlight == partName)
			{
				Config.SetEffect(CurrentCardId, partName, 0);
			}
			else
			{
				SeparatelyHighlight = partName;
			}
			SetPaginatorToMode(Config.GetEffect(CurrentCardId, SeparatelyHighlight));
			RefreshCurrentScreen();
			ShaderController.RefreshAllCardsWithId(CurrentCardId);
		}
		else
		{
			bool flag = Config.GetEffect(CurrentCardId, partName) > 0;
			Config.SetEffect(CurrentCardId, partName, (!flag) ? CurrentEffectIndex : 0);
			RefreshCurrentScreen();
			ShaderController.RefreshAllCardsWithId(CurrentCardId);
		}
	}

	public static void OnCopyCard()
	{
		if (!string.IsNullOrEmpty(CurrentCardId))
		{
			DisplayServer.ClipboardSet(Config.ExportCardPreset(CurrentCardId));
		}
	}

	public static void OnPasteCard()
	{
		if (!string.IsNullOrEmpty(CurrentCardId))
		{
			string text = DisplayServer.ClipboardGet();
			if (!string.IsNullOrEmpty(text) && Config.ImportCardPreset(CurrentCardId, text))
			{
				UpdatePaginatorTarget(CurrentCardId);
				RefreshCurrentScreen();
				ShaderController.RefreshAllCardsWithId(CurrentCardId);
			}
		}
	}

	public static void OnMenuAction(int id)
	{
		switch (id)
		{
		case 0:
			Config.ClearEffect(CurrentCardId);
			SeparatelyHighlight = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
			UpdatePaginatorTarget(CurrentCardId);
			RefreshCurrentScreen();
			ShaderController.RefreshAllCardsWithId(CurrentCardId);
			break;
		case 1:
		case 5:
			Config.ApplyAllAuthorPresets();
			RefreshCurrentScreen();
			break;
		case 2:
			ApplyToVisibleCards();
			break;
		case 3:
			DisplayServer.ClipboardSet(Config.ExportPreset());
			break;
		case 4:
		{
			string text = DisplayServer.ClipboardGet();
			if (!string.IsNullOrEmpty(text) && Config.ImportPreset(text))
			{
				RefreshCurrentScreen();
			}
			break;
		}
		}
	}

	private static void ApplyToVisibleCards()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
		List<CardModel> list = (((obj != null) ? ((Node)((SceneTree)obj).Root).FindChild("BalatroInspectScreen", true, false) : null) as NBalatroInspectScreen)?.CurrentCards;
		if (list == null || list.Count == 0)
		{
			GD.Print("[BalatroEffect] 应用给当前卡列表失败：检查界面卡列表为空");
			return;
		}
		Config.CardEffectEntry entry = Config.GetEntry(CurrentCardId);
		if (entry == null)
		{
			GD.Print("[BalatroEffect] 应用给当前卡列表失败：当前卡无有效配置");
			return;
		}
		foreach (CardModel item in list)
		{
			Config.ReplaceCardEffects(((object)((AbstractModel)item).Id).ToString(), entry);
		}
		RefreshCurrentScreen();
		foreach (CardModel item2 in list)
		{
			ShaderController.RefreshAllCardsWithId(((object)((AbstractModel)item2).Id).ToString());
		}
	}

	private static void RefreshCurrentScreen()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
		(((obj != null) ? ((Node)((SceneTree)obj).Root).FindChild("BalatroInspectScreen", true, false) : null) as NBalatroInspectScreen)?.RefreshPanelState();
	}

	private static void ApplyCurrentEffectToCheckedParts()
	{
		if (string.IsNullOrEmpty(CurrentCardId))
		{
			return;
		}
		string editMode = Config.GetEditMode(CurrentCardId);
		if (!(editMode == "fully"))
		{
			if (editMode == "separately")
			{
				string text = SeparatelyHighlight;
				if (string.IsNullOrEmpty(text))
				{
					text = ((Config.AllPartNames.Length != 0) ? Config.AllPartNames[0] : "");
				}
				if (!string.IsNullOrEmpty(text))
				{
					Config.SetEffect(CurrentCardId, text, CurrentEffectIndex);
				}
				return;
			}
			string[] allPartNames = Config.AllPartNames;
			foreach (string partName in allPartNames)
			{
				if (Config.GetEffect(CurrentCardId, partName) > 0)
				{
					Config.SetEffect(CurrentCardId, partName, CurrentEffectIndex);
				}
			}
		}
		else
		{
			Config.SetEffect(CurrentCardId, "FullCard", CurrentEffectIndex);
		}
	}

	private static void RefreshScreenSlider()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
		Node obj2 = ((obj != null) ? ((Node)((SceneTree)obj).Root).FindChild("BalatroEffectsSlider", true, false) : null);
		NSlider val = (NSlider)(object)((obj2 is NSlider) ? obj2 : null);
		if (val != null)
		{
			((GodotObject)val).SetBlockSignals(true);
			((Range)val).Value = Config.GetIntensity(CurrentCardId) * 100.0;
			((GodotObject)val).SetBlockSignals(false);
		}
	}
}

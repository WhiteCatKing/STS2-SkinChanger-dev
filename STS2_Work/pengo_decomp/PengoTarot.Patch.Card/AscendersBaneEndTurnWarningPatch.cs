using System;
using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace PengoTarot.Patch.Card;

[HarmonyPatch]
public static class AscendersBaneEndTurnWarningPatch
{
	[HarmonyPatch(typeof(NEndTurnButton), "OnFocus")]
	public static class EndTurnButton_OnFocus_WarningPatch
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			EnsureProcessHook();
		}
	}

	[HarmonyPatch(typeof(NEndTurnButton), "OnUnfocus")]
	public static class EndTurnButton_OnUnfocus_WarningPatch
	{
		[HarmonyPostfix]
		private static void Postfix()
		{
			StopWarning();
		}
	}

	private const float FlashPeriod = 0.5f;

	private const float FlashMinAlpha = 0.15f;

	private const float FlashMaxAlpha = 0.7f;

	private static bool _processHooked;

	private static bool _warningActive;

	private static readonly List<Control> _flashTargets = new List<Control>();

	private static void EnsureProcessHook()
	{
		if (!_processHooked)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame += OnProcessFrame;
				_processHooked = true;
			}
		}
	}

	private static void StopWarning()
	{
		if (_processHooked)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame -= OnProcessFrame;
			}
			_processHooked = false;
		}
		DeactivateVisuals();
	}

	private static void OnProcessFrame()
	{
		NCombatRoom instance = NCombatRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NCombatUi ui = instance.Ui;
			obj = ((ui != null) ? ui.Hand : null);
		}
		NPlayerHand val = (NPlayerHand)obj;
		if (val == null)
		{
			DeactivateVisuals();
			StopWarning();
			return;
		}
		if (!AscendersBaneTowerPatch.ShouldApply() || !HasEtherealAscendersBane(val))
		{
			DeactivateVisuals();
			return;
		}
		if (!_warningActive)
		{
			ActivateVisuals(val);
		}
		ApplyRedTint(val);
		ApplyFlash();
	}

	private static bool HasEtherealAscendersBane(NPlayerHand hand)
	{
		foreach (NHandCardHolder activeHolder in hand.ActiveHolders)
		{
			CardModel cardModel = ((NCardHolder)activeHolder).CardModel;
			if (cardModel is AscendersBane && cardModel.Keywords.Contains((CardKeyword)2))
			{
				return true;
			}
		}
		return false;
	}

	private static void ActivateVisuals(NPlayerHand hand)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		_warningActive = true;
		_flashTargets.Clear();
		foreach (NHandCardHolder activeHolder in hand.ActiveHolders)
		{
			NCard cardNode = ((NCardHolder)activeHolder).CardNode;
			NCardHighlight val = ((cardNode != null) ? cardNode.CardHighlight : null);
			if (val == null)
			{
				continue;
			}
			val.AnimShow();
			((CanvasItem)val).Modulate = NCardHighlight.red;
			if (((NCardHolder)activeHolder).CardModel is AscendersBane)
			{
				Node obj = ((Node)activeHolder).FindChild("Flash", false, false);
				Control val2 = (Control)(object)((obj is Control) ? obj : null);
				if (val2 != null)
				{
					_flashTargets.Add(val2);
				}
			}
		}
	}

	private static void ApplyRedTint(NPlayerHand hand)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		foreach (NHandCardHolder activeHolder in hand.ActiveHolders)
		{
			NCard cardNode = ((NCardHolder)activeHolder).CardNode;
			NCardHighlight val = ((cardNode != null) ? cardNode.CardHighlight : null);
			if (val != null)
			{
				if (((CanvasItem)val).Modulate != NCardHighlight.red)
				{
					val.AnimShow();
				}
				((CanvasItem)val).Modulate = NCardHighlight.red;
			}
		}
	}

	private static void ApplyFlash()
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (_flashTargets.Count == 0)
		{
			return;
		}
		float num = (Mathf.Sin((float)Time.GetTicksMsec() / 1000f * ((float)Math.PI * 2f) / 0.5f) + 1f) * 0.5f;
		float num2 = 0.15f + num * 0.54999995f;
		Color modulate = default(Color);
		((Color)(ref modulate))._002Ector(NCardHighlight.red.R, NCardHighlight.red.G, NCardHighlight.red.B, num2);
		for (int num3 = _flashTargets.Count - 1; num3 >= 0; num3--)
		{
			Control val = _flashTargets[num3];
			if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				_flashTargets.RemoveAt(num3);
			}
			else
			{
				((CanvasItem)val).Modulate = modulate;
			}
		}
	}

	private static void DeactivateVisuals()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!_warningActive && _flashTargets.Count == 0)
		{
			return;
		}
		_warningActive = false;
		_flashTargets.Clear();
		NCombatRoom instance = NCombatRoom.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NCombatUi ui = instance.Ui;
			obj = ((ui != null) ? ui.Hand : null);
		}
		NPlayerHand val = (NPlayerHand)obj;
		if (val == null)
		{
			return;
		}
		foreach (NHandCardHolder activeHolder in val.ActiveHolders)
		{
			if (((NCardHolder)activeHolder).CardModel is AscendersBane)
			{
				Node obj2 = ((Node)activeHolder).FindChild("Flash", false, false);
				Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
				if (val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val2))
				{
					Color modulate = ((CanvasItem)val2).Modulate;
					((CanvasItem)val2).Modulate = new Color(modulate.R, modulate.G, modulate.B, 0f);
				}
			}
			if (((NCardHolder)activeHolder).CardNode != null && GodotObject.IsInstanceValid((GodotObject)(object)((NCardHolder)activeHolder).CardNode))
			{
				activeHolder.UpdateCard();
			}
		}
	}
}

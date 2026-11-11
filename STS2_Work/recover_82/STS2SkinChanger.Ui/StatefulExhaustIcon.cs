using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class StatefulExhaustIcon : Node
{
	internal NExhaustPileButton Button;

	private Control? _custom;

	private Control? _original;

	private string? _provider;

	private bool _originalVisible;

	private bool _forcedButton;

	private bool _exiting;

	private double _elapsed;

	internal bool Active
	{
		get
		{
			if (_custom != null && GodotObject.IsInstanceValid((GodotObject)(object)_custom))
			{
				return ((CanvasItem)_custom).Visible;
			}
			return false;
		}
	}

	public override void _Process(double delta)
	{
		_elapsed += delta;
		if (_elapsed < 0.2 || !GodotObject.IsInstanceValid((GodotObject)(object)Button))
		{
			return;
		}
		_elapsed = 0.0;
		try
		{
			object? value = AccessTools.Field(typeof(NCombatCardPile), "_localPlayer").GetValue(Button);
			Player val = (Player)((value is Player) ? value : null);
			object obj2;
			if (val != null)
			{
				CardPile obj = CardPile.Get((PileType)2, val);
				obj2 = ((obj != null) ? ((IEnumerable<CardModel>)obj.Cards).FirstOrDefault((Func<CardModel, bool>)delegate(CardModel card)
				{
					if (((object)card).GetType().Name == "Defile")
					{
						StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(card);
						if (runtime != null && runtime.Enabled)
						{
							return !runtime.Simple;
						}
						return false;
					}
					return false;
				}) : null);
			}
			else
			{
				obj2 = null;
			}
			CardModel val2 = (CardModel)obj2;
			CardSkinOption cardSkinOption = ((val2 == null) ? null : SkinService.GetSelectedCardOption(val2));
			if ((object)cardSkinOption == null || cardSkinOption.ProviderId == null)
			{
				Restore();
				return;
			}
			if (_provider != cardSkinOption.ProviderId)
			{
				Restore();
				StatefulCardArtRuntime.Runtime runtime2 = StatefulCardArtRuntime.For(cardSkinOption);
				PackedScene val3 = SkinService.LoadCardPresentationResource<PackedScene>(val2, runtime2.Contract.ResourceRoot + "/images/defile_exhaust_icon.tscn");
				if (val3 == null)
				{
					return;
				}
				_custom = val3.Instantiate<Control>((GenEditState)0);
				AccessTools.Field(((object)_custom).GetType(), "Button").SetValue(_custom, Button);
				((Node)Button).AddChild((Node)(object)_custom, false, (InternalMode)0);
				((Node)_custom).SetProcess(false);
				_custom.MouseFilter = (MouseFilterEnum)2;
				_provider = cardSkinOption.ProviderId;
				_original = ((Node)Button).GetNodeOrNull<Control>(NodePath.op_Implicit("Icon"));
				Control? original = _original;
				_originalVisible = original != null && ((CanvasItem)original).Visible;
			}
			if (_original != null)
			{
				((CanvasItem)_original).Visible = false;
			}
			((CanvasItem)_custom).Visible = true;
			if (!((CanvasItem)Button).Visible)
			{
				_forcedButton = true;
				((NCombatCardPile)Button).AnimIn();
			}
		}
		catch (Exception exception)
		{
			Restore();
			StatefulCardArtRuntime.Warn("exhaust icon", exception);
		}
	}

	private void Restore()
	{
		if (_custom != null && GodotObject.IsInstanceValid((GodotObject)(object)_custom))
		{
			((CanvasItem)_custom).Visible = false;
			((Node)_custom).QueueFree();
		}
		if (_original != null && GodotObject.IsInstanceValid((GodotObject)(object)_original))
		{
			((CanvasItem)_original).Visible = _originalVisible;
		}
		_custom = null;
		_original = null;
		_provider = null;
		if (!_forcedButton || _exiting || !GodotObject.IsInstanceValid((GodotObject)(object)Button) || !((Node)Button).IsInsideTree())
		{
			return;
		}
		_forcedButton = false;
		object? value = AccessTools.Field(typeof(NCombatCardPile), "_localPlayer").GetValue(Button);
		Player val = (Player)((value is Player) ? value : null);
		if (val != null)
		{
			CardPile obj = CardPile.Get((PileType)4, val);
			if (obj != null && obj.Cards.Count == 0)
			{
				((NCombatCardPile)Button).AnimOut();
			}
		}
	}

	public override void _ExitTree()
	{
		_exiting = true;
		Restore();
	}
}

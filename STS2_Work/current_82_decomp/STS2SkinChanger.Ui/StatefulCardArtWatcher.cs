using System;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class StatefulCardArtWatcher : Node
{
	internal NCard Card;

	internal StatefulCardArtRuntime.Runtime Runtime;

	internal string Provider = "";

	internal CardPreviewMode Preview;

	private double _elapsed;

	private string? _signature;

	public override void _Process(double delta)
	{
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		_elapsed += delta;
		if (_elapsed < 0.2 || !GodotObject.IsInstanceValid((GodotObject)(object)Card) || !((CanvasItem)Card).IsVisibleInTree() || Card.Model == null)
		{
			return;
		}
		_elapsed = 0.0;
		try
		{
			CardSkinOption selectedCardOption = SkinService.GetSelectedCardOption(Card.Model);
			if (selectedCardOption?.ProviderId != Provider)
			{
				StatefulCardArtView.Release(Card);
				return;
			}
			string text = Runtime.Portrait(Card.Model, selectedCardOption.GetPortraitPath(((object)Card.Model).GetType().Name, useAncientStyle: false));
			string text2 = text + ":" + Runtime.Presentation(Card.Model)?.ToString() + ":" + Runtime.Simple + ":" + Runtime.Setting("HideTitle") + Runtime.Setting("HideStars");
			if (_signature != null && text2 != _signature)
			{
				_signature = text2;
				CardPreviewMode preview = Preview;
				AccessTools.Method(typeof(NCard), "Reload", (Type[])null, (Type[])null).Invoke(Card, null);
				Card.UpdateVisuals(Card.DisplayingPile, preview);
			}
			else
			{
				_signature = text2;
			}
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("visible refresh", exception);
			((Node)this).SetProcess(false);
		}
	}
}

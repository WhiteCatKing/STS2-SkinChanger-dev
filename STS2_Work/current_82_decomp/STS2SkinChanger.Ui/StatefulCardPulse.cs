using System;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class StatefulCardPulse : Node
{
	private NCard _card;

	private Control _body;

	private CardModel _model;

	private string _provider = "";

	private string _kind = "";

	private double _time;

	private Vector2 _scale = Vector2.One;

	private Vector2 _offset;

	private Vector2 _writtenScale;

	private Vector2 _writtenPosition;

	private bool _applied;

	internal static void Play(NCard card, StatefulCardArtRuntime.Runtime runtime, string kind)
	{
		StatefulCardPulse nodeOrNull = ((Node)card).GetNodeOrNull<StatefulCardPulse>(NodePath.op_Implicit("SCStatefulPulse"));
		if (nodeOrNull != null)
		{
			nodeOrNull.Undo();
			((Node)card).RemoveChild((Node)(object)nodeOrNull);
			((Node)nodeOrNull).QueueFree();
		}
		StatefulCardPulse statefulCardPulse = new StatefulCardPulse();
		((Node)statefulCardPulse).Name = StringName.op_Implicit("SCStatefulPulse");
		statefulCardPulse._card = card;
		statefulCardPulse._body = card.Body;
		statefulCardPulse._model = card.Model;
		statefulCardPulse._provider = SkinService.GetSelectedCardOption(card.Model)?.ProviderId ?? "";
		statefulCardPulse._kind = kind;
		StatefulCardPulse statefulCardPulse2 = statefulCardPulse;
		((Node)card).AddChild((Node)(object)statefulCardPulse2, false, (InternalMode)0);
	}

	private void Undo()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (_applied && GodotObject.IsInstanceValid((GodotObject)(object)_body))
		{
			Vector2 val = _body.Scale;
			if (((Vector2)(ref val)).IsEqualApprox(_writtenScale))
			{
				Control body = _body;
				body.Scale /= _scale;
			}
			val = _body.Position;
			if (((Vector2)(ref val)).IsEqualApprox(_writtenPosition))
			{
				Control body2 = _body;
				body2.Position -= _offset;
			}
			_applied = false;
		}
	}

	public override void _Process(double delta)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		Undo();
		_time += delta;
		if (!GodotObject.IsInstanceValid((GodotObject)(object)_card) || !GodotObject.IsInstanceValid((GodotObject)(object)_body) || _model != _card.Model || _time > ((_kind == "Rattle") ? 1.0 : 0.5) || SkinService.GetSelectedCardOption(_model)?.ProviderId != _provider)
		{
			((Node)this).QueueFree();
			return;
		}
		float num = MathF.Sin((float)_time * (float)Math.PI / ((_kind == "Rattle") ? 1f : 0.5f));
		Vector2 scale;
		Vector2 one;
		if (!(_kind == "Squeeze"))
		{
			if (!(_kind == "Flatten"))
			{
				scale = Vector2.One;
			}
			else
			{
				one = Vector2.One;
				scale = ((Vector2)(ref one)).Lerp(new Vector2(1.5f, 0.4f), num);
			}
		}
		else
		{
			one = Vector2.One;
			scale = ((Vector2)(ref one)).Lerp(new Vector2(0.65f, 1.35f), num);
		}
		_scale = scale;
		_offset = (Vector2)((_kind == "Rattle") ? new Vector2(MathF.Sin((float)_time * 95f) * num * 10f, 0f) : Vector2.Zero);
		Control body = _body;
		body.Scale *= _scale;
		Control body2 = _body;
		body2.Position += _offset;
		_writtenScale = _body.Scale;
		_writtenPosition = _body.Position;
		_applied = true;
	}

	public override void _ExitTree()
	{
		Undo();
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace PengoTarot.BalatroEffect;

public static class ShaderController
{
	private sealed class CardHoverState
	{
		public string Cid = "";

		public TiltWrapper.TiltContainer? Tilt;

		public ulong LeaveTimeMsec;

		public bool HasInlineEffects;
	}

	private static readonly HashSet<string> SkipNames;

	private static readonly HashSet<string> TextPartNames;

	private static readonly Dictionary<NCard, Action<CardModel?>> _handlers;

	private static readonly Dictionary<NCard, (CardModel model, Action handler)> _enchHandlers;

	private static readonly Dictionary<NCard, CardHoverState> _hoverCards;

	private static bool _hoverWatchActive;

	private const ulong TiltRemoveDelayMsec = 0uL;

	private static readonly Dictionary<NCard, (string Cid, Control? Portrait, Control? Ancient)> _portraitRefresh;

	private static bool _portraitRefreshActive;

	static ShaderController()
	{
		SkipNames = TiltWrapper.SkipNames;
		TextPartNames = PartWrapper.TextPartNames;
		_handlers = new Dictionary<NCard, Action<CardModel>>();
		_enchHandlers = new Dictionary<NCard, (CardModel, Action)>();
		_hoverCards = new Dictionary<NCard, CardHoverState>();
		_portraitRefresh = new Dictionary<NCard, (string, Control, Control)>();
		Config.PerformanceThrottled += OnPerformanceThrottled;
		EnchantmentConfig.Changed += delegate
		{
			RefreshAllCards();
		};
	}

	private static bool CardHasEffects(string cid)
	{
		return Config.HasEffect(cid);
	}

	public static void ApplyShader(NCard card)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Expected O, but got Unknown
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)card) || ((GodotObject)card).IsQueuedForDeletion())
		{
			return;
		}
		NCard obj = card;
		if (((obj != null) ? obj.Model : null) == null)
		{
			return;
		}
		if (card.Body == null)
		{
			ScheduleReapply(card);
			return;
		}
		string text = ((object)((AbstractModel)card.Model).Id).ToString();
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		EnchantmentModel enchantment = card.Model.Enchantment;
		Config.SetCardEnchantmentOverlay(text, (enchantment != null) ? ((object)((AbstractModel)enchantment).Id).ToString() : null, card.Model.IsEnchantmentPreview);
		Node obj2 = ((Node)card.Body).FindChild("Portrait", true, false);
		Control val = (Control)(object)((obj2 is Control) ? obj2 : null);
		if (val != null && val.Size == Vector2.Zero)
		{
			ScheduleReapply(card);
			return;
		}
		if (!Config.GlobalDynamicEffect && !CardHasEffects(text))
		{
			RemoveAllContainers(card.Body);
			CleanupInspect(card.Body);
			return;
		}
		if (_handlers.TryGetValue(card, out Action<CardModel> value))
		{
			card.ModelChanged -= value;
			_handlers.Remove(card);
		}
		Action<CardModel> action = delegate
		{
			ApplyShader(card);
		};
		card.ModelChanged += action;
		_handlers[card] = action;
		if (_enchHandlers.TryGetValue(card, out (CardModel, Action) value2))
		{
			value2.Item1.EnchantmentChanged -= value2.Item2;
			_enchHandlers.Remove(card);
		}
		if (card.Model != null)
		{
			Action action2 = delegate
			{
				ApplyShader(card);
			};
			card.Model.EnchantmentChanged += action2;
			_enchHandlers[card] = (card.Model, action2);
		}
		if (InInspect(card))
		{
			RemoveAllContainers(card.Body);
			if (Config.GetEffect(text, "FullCard") <= 0)
			{
				ApplyShaderInspect(card, text);
				return;
			}
		}
		else
		{
			RemoveAllContainers(card.Body);
			CleanupInspect(card.Body);
		}
		Control nodeOrNull = ((Node)card.Body).GetNodeOrNull<Control>(NodePath.op_Implicit("CardVfxContainer"));
		int effect = Config.GetEffect(text, "FullCard");
		if (effect > 0)
		{
			TiltWrapper.TiltContainer tiltContainer = TiltWrapper.CreateTilt(card, text, fullCard: true);
			if (tiltContainer == null)
			{
				if (nodeOrNull != null)
				{
					((Node)card.Body).MoveChild((Node)(object)nodeOrNull, 0);
				}
				return;
			}
			((Node)card.Body).AddChild((Node)(object)tiltContainer, false, (InternalMode)0);
			if (nodeOrNull != null)
			{
				((Node)card.Body).MoveChild((Node)(object)nodeOrNull, 0);
			}
			FixGlowLayer(card.Body);
			Material material = ((CanvasItem)tiltContainer).Material;
			ShaderMaterial val2 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val2 != null)
			{
				val2.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(0));
				val2.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(1.0));
			}
			FullCardWrapper.FullCardEffectContainer fullCardEffectContainer = FullCardWrapper.CreateFullCard(card, text, tiltContainer, effect);
			((Node)card.Body).AddChild((Node)(object)fullCardEffectContainer, false, (InternalMode)0);
			Control root = (Control)((Node)((Node)tiltContainer).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"))).GetChild(0, false);
			PartWrapper.WrapParts(text, root);
			UpdatePortraitMaterial(card, text, root);
		}
		else if (CardHasEffects(text))
		{
			WrapPartsInspect(text, card.Body);
			UpdatePortraitInspect(card, text, card.Body);
			_hoverCards[card] = new CardHoverState
			{
				Cid = text,
				HasInlineEffects = true
			};
			EnsureHoverWatcher();
			if (nodeOrNull != null)
			{
				((Node)card.Body).MoveChild((Node)(object)nodeOrNull, 0);
			}
		}
		else
		{
			_hoverCards[card] = new CardHoverState
			{
				Cid = text
			};
			EnsureHoverWatcher();
			if (nodeOrNull != null)
			{
				((Node)card.Body).MoveChild((Node)(object)nodeOrNull, 0);
			}
		}
	}

	internal static void ScheduleReapply(NCard card)
	{
		SceneTree tree = default(SceneTree);
		ref SceneTree reference = ref tree;
		MainLoop mainLoop = Engine.GetMainLoop();
		reference = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		int attempts;
		if (tree != null)
		{
			attempts = 0;
			tree.ProcessFrame += Poll;
		}
		void Poll()
		{
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			tree.ProcessFrame -= Poll;
			if (GodotObject.IsInstanceValid((GodotObject)(object)card) && !((GodotObject)card).IsQueuedForDeletion())
			{
				if (card.Model == null || card.Body == null)
				{
					if (++attempts < 120)
					{
						tree.ProcessFrame += Poll;
					}
				}
				else
				{
					Node obj = ((Node)card.Body).FindChild("Portrait", true, false);
					Control val = (Control)(object)((obj is Control) ? obj : null);
					if (val != null && val.Size == Vector2.Zero)
					{
						if (++attempts < 120)
						{
							tree.ProcessFrame += Poll;
						}
					}
					else
					{
						ApplyShader(card);
					}
				}
			}
		}
	}

	public static void CleanupCard(NCard card)
	{
		if (_handlers.TryGetValue(card, out Action<CardModel> value))
		{
			card.ModelChanged -= value;
			_handlers.Remove(card);
		}
		if (_enchHandlers.TryGetValue(card, out (CardModel, Action) value2))
		{
			value2.Item1.EnchantmentChanged -= value2.Item2;
			_enchHandlers.Remove(card);
		}
		_portraitRefresh.Remove(card);
		if (card.Body != null)
		{
			CleanupInspect(card.Body);
			RemoveAllContainers(card.Body);
		}
		_hoverCards.Remove(card);
		if (_hoverCards.Count == 0)
		{
			UnhookHoverWatcher();
		}
	}

	public static void RefreshAllCardsWithId(string cid)
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		foreach (Node child in ((Node)val.Root).GetChildren(false))
		{
			RNode(child, cid);
		}
	}

	public static void RefreshAllCards()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			RAll((Node)(object)val.Root);
		}
	}

	internal static void ApplyStarcloudBgAspect(Control bg)
	{
		Control obj = bg;
		Material val = ((obj != null) ? ((CanvasItem)obj).Material : null);
		ShaderMaterial mat = (ShaderMaterial)(object)((val is ShaderMaterial) ? val : null);
		SceneTree tree;
		if (mat != null)
		{
			tree = ((Node)bg).GetTree();
			if (tree != null)
			{
				tree.ProcessFrame += Poll;
			}
		}
		void Poll()
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			tree.ProcessFrame -= Poll;
			if (GodotObject.IsInstanceValid((GodotObject)(object)bg))
			{
				Vector2 size = bg.Size;
				float num = Mathf.Max(size.X, size.Y);
				if (num <= 0f)
				{
					tree.ProcessFrame += Poll;
				}
				else
				{
					mat.SetShaderParameter(StringName.op_Implicit("uv_scale"), Variant.op_Implicit(new Vector2(size.X / num, size.Y / num)));
				}
			}
		}
	}

	private static void RemoveAllContainers(Control body)
	{
		Control nodeOrNull = ((Node)body).GetNodeOrNull<Control>(NodePath.op_Implicit("CardVfxContainer"));
		foreach (FullCardWrapper.FullCardEffectContainer item in ((IEnumerable)((Node)body).GetChildren(false)).OfType<FullCardWrapper.FullCardEffectContainer>().ToList())
		{
			SubViewport nodeOrNull2 = ((Node)item).GetNodeOrNull<SubViewport>(NodePath.op_Implicit("SubViewport"));
			if (nodeOrNull2 != null)
			{
				Node[] array = (Node[])(object)new Node[((Node)nodeOrNull2).GetChildCount(false)];
				for (int i = 0; i < ((Node)nodeOrNull2).GetChildCount(false); i++)
				{
					array[i] = ((Node)nodeOrNull2).GetChild(i, false);
				}
				Node[] array2 = array;
				foreach (Node val in array2)
				{
					((Node)nodeOrNull2).RemoveChild(val);
					((Node)body).AddChild(val, false, (InternalMode)0);
				}
			}
			Node parent = ((Node)item).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)item);
			}
			((Node)item).QueueFree();
		}
		foreach (TiltWrapper.TiltContainer item2 in ((IEnumerable)((Node)body).GetChildren(false)).OfType<TiltWrapper.TiltContainer>().ToList())
		{
			TiltWrapper.DestroyTilt(item2, body);
		}
		if (nodeOrNull != null)
		{
			((Node)body).MoveChild((Node)(object)nodeOrNull, 0);
		}
	}

	private static void RegisterPortraitRefresh(NCard card, string cid, Control? portrait, Control? ancient)
	{
		if (card != null)
		{
			_portraitRefresh[card] = (cid, portrait, ancient);
			EnsurePortraitRefreshWatcher();
		}
	}

	private static void EnsurePortraitRefreshWatcher()
	{
		if (!_portraitRefreshActive)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame += OnPortraitRefreshProcessFrame;
				_portraitRefreshActive = true;
			}
		}
	}

	private static void OnPortraitRefreshProcessFrame()
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		if (_portraitRefresh.Count == 0)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame -= OnPortraitRefreshProcessFrame;
			}
			_portraitRefreshActive = false;
			return;
		}
		List<NCard> list = null;
		foreach (var (val3, tuple2) in _portraitRefresh)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)val3))
			{
				(list ?? (list = new List<NCard>())).Add(val3);
				continue;
			}
			double intensity = Config.GetIntensity(tuple2.Item1);
			Control item = tuple2.Item2;
			if (item != null && GodotObject.IsInstanceValid((GodotObject)(object)item))
			{
				Material material = ((CanvasItem)item).Material;
				ShaderMaterial val4 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
				if (val4 != null && val4.Shader != null && IsOurs((Material)(object)val4))
				{
					val4.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(intensity));
				}
			}
			Control item2 = tuple2.Item3;
			if (item2 != null && GodotObject.IsInstanceValid((GodotObject)(object)item2))
			{
				Material material2 = ((CanvasItem)item2).Material;
				ShaderMaterial val5 = (ShaderMaterial)(object)((material2 is ShaderMaterial) ? material2 : null);
				if (val5 != null && val5.Shader != null && IsOurs((Material)(object)val5))
				{
					val5.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(intensity));
				}
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (NCard item3 in list)
		{
			_portraitRefresh.Remove(item3);
		}
	}

	private static void UpdatePortraitMaterial(NCard card, string cid, Control root)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		Node obj = ((Node)root).FindChild("Portrait", true, false);
		Control val = (Control)(((object)((obj is Control) ? obj : null)) ?? ((object)/*isinst with value type is only supported in some contexts*/));
		if (val == null)
		{
			return;
		}
		int effect = Config.GetEffect(cid, "Portrait");
		Shader val2 = ((effect > 0) ? EffectRegistry.GetShader(effect) : null);
		if (val2 == null)
		{
			RestoreMaterial(val);
			return;
		}
		Vector2 val3 = val.Position + TiltWrapper.UvRefHalf;
		Material material = ((CanvasItem)val).Material;
		ShaderMaterial val4 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		ShaderMaterial val5;
		if (val4 != null && val4.Shader == val2)
		{
			val5 = val4;
		}
		else
		{
			val5 = new ShaderMaterial
			{
				Shader = val2
			};
			val5.SetShaderParameter(TiltWrapper.SeedKey, Variant.op_Implicit((float)(((object)val).GetHashCode() % 10000) / 10f));
			if (((CanvasItem)val).Material != null && !IsOurs(((CanvasItem)val).Material))
			{
				((Material)val5).NextPass = ((CanvasItem)val).Material;
			}
			((CanvasItem)val).Material = (Material)(object)val5;
		}
		val5.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(effect));
		val5.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(cid)));
		val5.SetShaderParameter(TiltWrapper.UvOffsetKey, Variant.op_Implicit(val3 / TiltWrapper.UvRefSize));
		val5.SetShaderParameter(TiltWrapper.UvScaleKey, Variant.op_Implicit(val.Size / TiltWrapper.UvRefSize));
		val5.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, Variant.op_Implicit((val3 - TiltWrapper.UvRefHalf) / TiltWrapper.UvRefSize));
		RegisterPortraitRefresh(card, cid, val, null);
	}

	private static void RestoreMaterial(Control? c)
	{
		if (c != null)
		{
			Material material = ((CanvasItem)c).Material;
			ShaderMaterial val = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val != null && IsOurs((Material)(object)val))
			{
				((CanvasItem)c).Material = ((Material)val).NextPass;
			}
		}
	}

	private static bool IsOurs(Material mat)
	{
		ShaderMaterial val = (ShaderMaterial)(object)((mat is ShaderMaterial) ? mat : null);
		if (val == null || val.Shader == null)
		{
			return false;
		}
		foreach (EffectRegistry.EffectDef allEffect in EffectRegistry.AllEffects)
		{
			if (allEffect.Shader == val.Shader)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsCardHovered(NCard card, out NCardHolder? holder)
	{
		holder = null;
		for (Node parent = ((Node)card).GetParent(); parent != null; parent = parent.GetParent())
		{
			NCardHolder val = (NCardHolder)(object)((parent is NCardHolder) ? parent : null);
			if (val != null)
			{
				holder = val;
				break;
			}
		}
		if (holder == null)
		{
			return false;
		}
		NCardHolder val2 = holder;
		if (!(val2 is NHandCardHolder) || ((CanvasItem)val2).ZIndex <= 0)
		{
			NClickableControl hitbox = holder.Hitbox;
			if (hitbox != null && hitbox.IsEnabled)
			{
				return Traverse.Create((object)hitbox).Field<bool>("_isHovered").Value;
			}
			return false;
		}
		return true;
	}

	private static void EnsureHoverWatcher()
	{
		if (!_hoverWatchActive)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame += OnHoverProcess;
				_hoverWatchActive = true;
			}
		}
	}

	private static void FixGlowLayer(Control body)
	{
		int num = 1;
		foreach (Node child in ((Node)body).GetChildren(false))
		{
			if (child.Name == StringName.op_Implicit("RareGlow") || child.Name == StringName.op_Implicit("UncommonGlow"))
			{
				((Node)body).MoveChild(child, num);
				num++;
			}
		}
	}

	private static void UnhookHoverWatcher()
	{
		if (_hoverWatchActive)
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val != null)
			{
				val.ProcessFrame -= OnHoverProcess;
			}
			_hoverWatchActive = false;
		}
	}

	private static void OnHoverProcess()
	{
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Expected O, but got Unknown
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Expected O, but got Unknown
		ulong ticksMsec = Time.GetTicksMsec();
		List<NCard> list = new List<NCard>();
		foreach (var (val2, cardHoverState2) in _hoverCards)
		{
			NCardHolder holder;
			if (!GodotObject.IsInstanceValid((GodotObject)(object)val2) || ((GodotObject)val2).IsQueuedForDeletion() || val2.Body == null)
			{
				if (cardHoverState2.Tilt != null && GodotObject.IsInstanceValid((GodotObject)(object)cardHoverState2.Tilt))
				{
					((Node)cardHoverState2.Tilt).QueueFree();
				}
				list.Add(val2);
			}
			else if (IsCardHovered(val2, out holder))
			{
				cardHoverState2.LeaveTimeMsec = 0uL;
				if (cardHoverState2.Tilt != null)
				{
					continue;
				}
				Control body = val2.Body;
				Control nodeOrNull = ((Node)body).GetNodeOrNull<Control>(NodePath.op_Implicit("CardVfxContainer"));
				if (!cardHoverState2.HasInlineEffects)
				{
					TiltWrapper.TiltContainer tiltContainer = TiltWrapper.CreateTilt(val2, cardHoverState2.Cid);
					if (tiltContainer != null)
					{
						((Node)body).AddChild((Node)(object)tiltContainer, false, (InternalMode)0);
						((Node)body).MoveChild((Node)(object)tiltContainer, 0);
						if (nodeOrNull != null)
						{
							((Node)body).MoveChild((Node)(object)nodeOrNull, 0);
						}
						FixGlowLayer(body);
						Control root = (Control)((Node)((Node)tiltContainer).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"))).GetChild(0, false);
						PartWrapper.WrapParts(cardHoverState2.Cid, root);
						UpdatePortraitMaterial(val2, cardHoverState2.Cid, root);
						cardHoverState2.Tilt = tiltContainer;
					}
					else if (nodeOrNull != null)
					{
						((Node)body).MoveChild((Node)(object)nodeOrNull, 0);
					}
					continue;
				}
				List<string> logicalOrder = new List<string>();
				foreach (Node child in ((Node)body).GetChildren(false))
				{
					logicalOrder.Add(Ln(StringName.op_Implicit(child.Name)));
				}
				TiltWrapper.TiltContainer tiltContainer2 = TiltWrapper.CreateTilt(val2, cardHoverState2.Cid);
				if (tiltContainer2 != null)
				{
					((Node)body).AddChild((Node)(object)tiltContainer2, false, (InternalMode)0);
					((Node)body).MoveChild((Node)(object)tiltContainer2, 0);
					if (nodeOrNull != null)
					{
						((Node)body).MoveChild((Node)(object)nodeOrNull, 0);
					}
					FixGlowLayer(body);
					Control val3 = (Control)((Node)((Node)tiltContainer2).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"))).GetChild(0, false);
					List<Node> list2 = new List<Node>();
					foreach (Node child2 in ((Node)body).GetChildren(false))
					{
						if (((object)child2.Name).ToString().StartsWith("BalatroShaderPart_"))
						{
							list2.Add(child2);
						}
					}
					foreach (Node item in list2)
					{
						((Node)body).RemoveChild(item);
						((Node)val3).AddChild(item, false, (InternalMode)0);
					}
					List<Node> list3 = new List<Node>();
					foreach (Node child3 in ((Node)val3).GetChildren(false))
					{
						list3.Add(child3);
					}
					list3.Sort((Node a, Node b) => logicalOrder.IndexOf(Ln(StringName.op_Implicit(a.Name))).CompareTo(logicalOrder.IndexOf(Ln(StringName.op_Implicit(b.Name)))));
					foreach (Node item2 in ((IEnumerable<Node>)((Node)val3).GetChildren(false)).ToList())
					{
						((Node)val3).RemoveChild(item2);
					}
					foreach (Node item3 in list3)
					{
						((Node)val3).AddChild(item3, false, (InternalMode)0);
					}
					cardHoverState2.Tilt = tiltContainer2;
				}
				else if (nodeOrNull != null)
				{
					((Node)body).MoveChild((Node)(object)nodeOrNull, 0);
				}
			}
			else
			{
				if (cardHoverState2.Tilt == null)
				{
					continue;
				}
				if (cardHoverState2.LeaveTimeMsec == 0L)
				{
					cardHoverState2.LeaveTimeMsec = ticksMsec;
				}
				if (ticksMsec - cardHoverState2.LeaveTimeMsec == 0)
				{
					continue;
				}
				if (cardHoverState2.HasInlineEffects)
				{
					Control body2 = val2.Body;
					Control nodeOrNull2 = ((Node)body2).GetNodeOrNull<Control>(NodePath.op_Implicit("CardVfxContainer"));
					Control val4 = (Control)((Node)((Node)cardHoverState2.Tilt).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"))).GetChild(0, false);
					Node[] array = (Node[])(object)new Node[((Node)val4).GetChildCount(false)];
					for (int i = 0; i < ((Node)val4).GetChildCount(false); i++)
					{
						array[i] = ((Node)val4).GetChild(i, false);
					}
					Node[] array2 = array;
					foreach (Node val5 in array2)
					{
						((Node)val4).RemoveChild(val5);
						((Node)body2).AddChild(val5, false, (InternalMode)0);
					}
					Node parent = ((Node)cardHoverState2.Tilt).GetParent();
					if (parent != null)
					{
						parent.RemoveChild((Node)(object)cardHoverState2.Tilt);
					}
					((Node)cardHoverState2.Tilt).QueueFree();
					if (nodeOrNull2 != null)
					{
						((Node)body2).MoveChild((Node)(object)nodeOrNull2, 0);
					}
				}
				else
				{
					TiltWrapper.DestroyTilt(cardHoverState2.Tilt, val2.Body);
				}
				cardHoverState2.Tilt = null;
				cardHoverState2.LeaveTimeMsec = 0uL;
			}
		}
		foreach (NCard item4 in list)
		{
			_hoverCards.Remove(item4);
		}
		if (_hoverCards.Count == 0)
		{
			UnhookHoverWatcher();
		}
		static string Ln(string nm)
		{
			if (!nm.StartsWith("BalatroShaderPart_"))
			{
				return nm;
			}
			return nm.Substring("BalatroShaderPart_".Length);
		}
	}

	private static void RAll(Node n)
	{
		NCard val = (NCard)(object)((n is NCard) ? n : null);
		if (val != null && val.Model != null)
		{
			ApplyShader(val);
		}
		foreach (Node child in n.GetChildren(false))
		{
			RAll(child);
		}
	}

	private static void RNode(Node n, string cid)
	{
		NCard val = (NCard)(object)((n is NCard) ? n : null);
		if (val != null)
		{
			CardModel model = val.Model;
			if (((model != null) ? ((object)((AbstractModel)model).Id).ToString() : null) == cid)
			{
				ApplyShader(val);
			}
		}
		foreach (Node child in n.GetChildren(false))
		{
			RNode(child, cid);
		}
	}

	private static bool InInspect(NCard card)
	{
		for (Node parent = ((Node)card).GetParent(); parent != null; parent = parent.GetParent())
		{
			bool flag;
			switch (((object)parent).GetType().Name)
			{
			case "NInspectCardScreen":
			case "NBalatroInspectScreen":
			case "NBalatroInspectEnchantScreen":
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private static void ApplyShaderInspect(NCard card, string cid)
	{
		Control body = card.Body;
		RemoveAllContainers(body);
		CleanupInspect(body);
		WrapPartsInspect(cid, body);
		UpdatePortraitInspect(card, cid, body);
	}

	private static void CleanupInspect(Control body)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		List<(PartWrapper.ShaderPartContainer, int)> list = new List<(PartWrapper.ShaderPartContainer, int)>();
		List<Node> list2 = new List<Node>();
		foreach (Node child in ((Node)body).GetChildren(false))
		{
			list2.Add(child);
		}
		for (int i = 0; i < list2.Count; i++)
		{
			if (list2[i] is PartWrapper.ShaderPartContainer shaderPartContainer && ((object)((Node)shaderPartContainer).Name).ToString().StartsWith("BalatroShaderPart_"))
			{
				list.Add((shaderPartContainer, i));
			}
		}
		list.Reverse();
		foreach (var item3 in list)
		{
			PartWrapper.ShaderPartContainer item = item3.Item1;
			int item2 = item3.Item2;
			SubViewport nodeOrNull = ((Node)item).GetNodeOrNull<SubViewport>(NodePath.op_Implicit("SubViewport"));
			if (nodeOrNull != null && ((Node)nodeOrNull).GetChildCount(false) > 0)
			{
				Node childOrNull = ((Node)nodeOrNull).GetChild(0, false).GetChildOrNull<Node>(0, false);
				if (childOrNull != null)
				{
					((Node)nodeOrNull).GetChild(0, false).RemoveChild(childOrNull);
					Control val = (Control)(object)((childOrNull is Control) ? childOrNull : null);
					if (val != null)
					{
						val.Position = item.OriginalPosition;
					}
					((Node)body).AddChild(childOrNull, false, (InternalMode)0);
					((Node)body).MoveChild(childOrNull, item2);
				}
			}
			Node parent = ((Node)item).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)item);
			}
			((Node)item).QueueFree();
		}
		Node obj = ((Node)body).FindChild("Portrait", true, false);
		RestoreMaterial((Control?)(object)((obj is Control) ? obj : null));
		Node obj2 = ((Node)body).FindChild("AncientPortrait", true, false);
		RestoreMaterial((Control?)(object)((obj2 is Control) ? obj2 : null));
	}

	private static void WrapPartsInspect(string cid, Control body)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		List<Node> list = new List<Node>();
		foreach (Node child in ((Node)body).GetChildren(false))
		{
			list.Add(child);
		}
		List<Node> list2 = new List<Node>();
		foreach (Node item in list)
		{
			Control val = (Control)(object)((item is Control) ? item : null);
			if (val == null)
			{
				list2.Add(item);
				continue;
			}
			string text = StringName.op_Implicit(((Node)val).Name);
			if (SkipNames.Contains(text) || text == "PortraitCanvasGroup" || TextPartNames.Contains(text))
			{
				list2.Add((Node)(object)val);
			}
			else if (Config.AllPartNames.Contains(text) && Config.GetEffect(cid, text) > 0)
			{
				Vector2 gp = val.Position + TiltWrapper.UvRefHalf;
				PartWrapper.ShaderPartContainer shaderPartContainer = PartWrapper.CreateShaderPart(cid, text, val, gp, val.Size);
				((Node)body).RemoveChild((Node)(object)val);
				val.Position = Vector2.Zero;
				((Node)((Node)shaderPartContainer).GetNode<SubViewport>(NodePath.op_Implicit("SubViewport"))).GetChild(0, false).AddChild((Node)(object)val, false, (InternalMode)0);
				list2.Add((Node)(object)shaderPartContainer);
			}
			else
			{
				list2.Add((Node)(object)val);
			}
		}
		foreach (Node item2 in list2)
		{
			if (item2.GetParent() == null)
			{
				((Node)body).AddChild(item2, false, (InternalMode)0);
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			((Node)body).MoveChild(list2[i], i);
		}
	}

	private static void UpdatePortraitInspect(NCard card, string cid, Control body)
	{
		Node obj = ((Node)body).FindChild("Portrait", true, false);
		Control val = (Control)(object)((obj is Control) ? obj : null);
		Node obj2 = ((Node)body).FindChild("AncientPortrait", true, false);
		Control val2 = (Control)(object)((obj2 is Control) ? obj2 : null);
		if (val == null && val2 == null)
		{
			return;
		}
		int effect = Config.GetEffect(cid, "Portrait");
		Shader val3 = ((effect > 0) ? EffectRegistry.GetShader(effect) : null);
		if (val3 == null)
		{
			if (val != null)
			{
				RestoreMaterial(val);
			}
			if (val2 != null)
			{
				RestoreMaterial(val2);
			}
			return;
		}
		if (val != null)
		{
			ApplyPortraitShader(val, cid, effect, val3);
		}
		if (val2 != null)
		{
			ApplyPortraitShader(val2, cid, effect, val3);
		}
		RegisterPortraitRefresh(card, cid, val, val2);
	}

	private static void ApplyPortraitShader(Control pn, string cid, int e, Shader sh)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = pn.Position + TiltWrapper.UvRefHalf;
		Material material = ((CanvasItem)pn).Material;
		ShaderMaterial val2 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
		ShaderMaterial val3;
		if (val2 != null && val2.Shader == sh)
		{
			val3 = val2;
		}
		else
		{
			val3 = new ShaderMaterial
			{
				Shader = sh
			};
			val3.SetShaderParameter(TiltWrapper.SeedKey, Variant.op_Implicit((float)(((object)pn).GetHashCode() % 10000) / 10f));
			if (((CanvasItem)pn).Material != null && !IsOurs(((CanvasItem)pn).Material))
			{
				((Material)val3).NextPass = ((CanvasItem)pn).Material;
			}
			((CanvasItem)pn).Material = (Material)(object)val3;
		}
		val3.SetShaderParameter(TiltWrapper.EffectModeKey, Variant.op_Implicit(e));
		val3.SetShaderParameter(TiltWrapper.IntensityKey, Variant.op_Implicit(Config.GetIntensity(cid)));
		val3.SetShaderParameter(TiltWrapper.UvOffsetKey, Variant.op_Implicit(val / TiltWrapper.UvRefSize));
		val3.SetShaderParameter(TiltWrapper.UvScaleKey, Variant.op_Implicit(pn.Size / TiltWrapper.UvRefSize));
		val3.SetShaderParameter(TiltWrapper.FoilAltUvOffsetKey, Variant.op_Implicit((val - TiltWrapper.UvRefHalf) / TiltWrapper.UvRefSize));
	}

	private static void OnPerformanceThrottled()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			val.ProcessFrame += PerformCleanupAfterThrottle;
		}
	}

	private static void PerformCleanupAfterThrottle()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			val.ProcessFrame -= PerformCleanupAfterThrottle;
		}
		_hoverCards.Clear();
		if (_hoverWatchActive)
		{
			UnhookHoverWatcher();
		}
		if (val == null)
		{
			return;
		}
		foreach (Node child in ((Node)val.Root).GetChildren(false))
		{
			CleanupAllCardContainersRecursive(child);
		}
	}

	private static void CleanupAllCardContainersRecursive(Node node)
	{
		NCard val = (NCard)(object)((node is NCard) ? node : null);
		if (val != null && val.Body != null)
		{
			RemoveAllContainers(val.Body);
			CleanupInspect(val.Body);
			if (_handlers.TryGetValue(val, out Action<CardModel> value))
			{
				val.ModelChanged -= value;
				_handlers.Remove(val);
			}
		}
		foreach (Node child in node.GetChildren(false))
		{
			CleanupAllCardContainersRecursive(child);
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.UI;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Cards;
using PengoTarot.Data;

namespace PengoTarot.Patches;

internal static class UnavailableEnchantmentPreview
{
	private sealed class PreviewController : Control
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Process = StringName.op_Implicit("_Process");

			public static readonly StringName _ExitTree = StringName.op_Implicit("_ExitTree");
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		public override void _Process(double delta)
		{
			Tick((float)delta);
		}

		public override void _ExitTree()
		{
			ControllerExited(this);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(2)
			{
				new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
				{
					new PropertyInfo((Type)3, StringName.op_Implicit("delta"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
				}, (List<Variant>)null),
				new MethodInfo(MethodName._ExitTree, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
			{
				((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName._ExitTree && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				((Node)this)._ExitTree();
				ret = default(godot_variant);
				return true;
			}
			return ((Control)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName._Process)
			{
				return true;
			}
			if ((ref method) == MethodName._ExitTree)
			{
				return true;
			}
			return ((Control)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).RestoreGodotObjectData(info);
		}
	}

	private sealed class InvalidCross : Control
	{
		public class MethodName : MethodName
		{
			public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

			public static readonly StringName _Process = StringName.op_Implicit("_Process");
		}

		public class PropertyName : PropertyName
		{
			public static readonly StringName _phase = StringName.op_Implicit("_phase");
		}

		public class SignalName : SignalName
		{
		}

		private const string TexturePath = "res://images/ui/unavailable_enchantment_x.png";

		private const float HorizontalOverflow = 72f;

		private const float VerticalOverflow = 34f;

		private float _phase;

		public override void _Ready()
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Expected O, but got Unknown
			((Control)this).MouseFilter = (MouseFilterEnum)2;
			((Control)this).Size = NCard.defaultSize + new Vector2(144f, 68f);
			((Control)this).Position = ((Control)this).Size * -0.5f;
			TextureRect val = new TextureRect
			{
				MouseFilter = (MouseFilterEnum)2,
				ExpandMode = (ExpandModeEnum)1,
				StretchMode = (StretchModeEnum)5,
				Texture = GD.Load<Texture2D>("res://images/ui/unavailable_enchantment_x.png")
			};
			((Node)this).AddChild((Node)(object)val, false, (InternalMode)0);
			((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		}

		public override void _Process(double delta)
		{
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			_phase = (_phase + (float)delta) % 3f;
			float num = 0.35f * (1f - Mathf.Cos(_phase * ((float)Math.PI * 2f) / 3f));
			((CanvasItem)this).Modulate = new Color(1f, 1f, 1f, num);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(2)
			{
				new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
				new MethodInfo(MethodName._Process, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
				{
					new PropertyInfo((Type)3, StringName.op_Implicit("delta"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
				}, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				((Node)this)._Ready();
				ret = default(godot_variant);
				return true;
			}
			if ((ref method) == MethodName._Process && ((NativeVariantPtrArgs)(ref args)).Count == 1)
			{
				((Node)this)._Process(VariantUtils.ConvertTo<double>(ref ((NativeVariantPtrArgs)(ref args))[0]));
				ret = default(godot_variant);
				return true;
			}
			return ((Control)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName._Ready)
			{
				return true;
			}
			if ((ref method) == MethodName._Process)
			{
				return true;
			}
			return ((Control)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
		{
			if ((ref name) == PropertyName._phase)
			{
				_phase = VariantUtils.ConvertTo<float>(ref value);
				return true;
			}
			return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if ((ref name) == PropertyName._phase)
			{
				value = VariantUtils.CreateFrom<float>(ref _phase);
				return true;
			}
			return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<PropertyInfo> GetGodotPropertyList()
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			return new List<PropertyInfo>
			{
				new PropertyInfo((Type)3, PropertyName._phase, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)this).SaveGodotObjectData(info);
			info.AddProperty(PropertyName._phase, Variant.From<float>(ref _phase));
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).RestoreGodotObjectData(info);
			Variant val = default(Variant);
			if (info.TryGetProperty(PropertyName._phase, ref val))
			{
				_phase = ((Variant)(ref val)).As<float>();
			}
		}
	}

	private sealed class PreviewEntry(NCard card)
	{
		public NCard Card { get; } = card;

		public Tween? Tween { get; set; }
	}

	private const float HoverDelay = 1f;

	private const float LeaveDelay = 5f;

	private const float CardScale = 0.3f;

	private const float CardGap = 5f;

	private const float TopMargin = 108f;

	private const float RightMargin = 18f;

	private const float BottomMargin = 18f;

	private const float ColumnGap = 8f;

	private const float AnimationDuration = 0.38f;

	private static readonly List<PreviewEntry> TypeEntries = new List<PreviewEntry>();

	private static readonly List<PreviewEntry> EnchantedEntries = new List<PreviewEntry>();

	private static PreviewController? _controller;

	private static NCardHolder? _hoveredHolder;

	private static float _hoverSeconds;

	private static float _unhoverSeconds;

	private static bool _shown;

	public static void NotifyHovered(NCardHolder holder)
	{
		CardModel cardModel = holder.CardModel;
		if (cardModel is TarCard || cardModel is PlanetCard)
		{
			if (_hoveredHolder != holder)
			{
				RetractImmediately();
				_hoveredHolder = holder;
				_hoverSeconds = 0f;
			}
			_unhoverSeconds = 0f;
			EnsureController();
		}
	}

	public static void NotifyUnhovered(NCardHolder holder)
	{
		if (_hoveredHolder == holder)
		{
			_unhoverSeconds = 0.0001f;
		}
	}

	public static void RetractImmediately()
	{
		_hoveredHolder = null;
		_hoverSeconds = 0f;
		_unhoverSeconds = 0f;
		RetractCards();
	}

	public static void ShowForEnchantment(EnchantmentModel enchantment)
	{
		_hoveredHolder = null;
		_hoverSeconds = 0f;
		_unhoverSeconds = 0f;
		ShowFor(enchantment);
	}

	private static void Tick(float delta)
	{
		if (_hoveredHolder == null || !GodotObject.IsInstanceValid((GodotObject)(object)_hoveredHolder))
		{
			if (_shown)
			{
				_unhoverSeconds += delta;
				if (_unhoverSeconds >= 5f)
				{
					RetractImmediately();
				}
			}
		}
		else if (_unhoverSeconds > 0f)
		{
			_unhoverSeconds += delta;
			if (_unhoverSeconds >= 5f)
			{
				RetractImmediately();
			}
		}
		else
		{
			if (_shown)
			{
				return;
			}
			_hoverSeconds += delta;
			if (_hoverSeconds >= 1f)
			{
				CardModel cardModel = _hoveredHolder.CardModel;
				if (cardModel != null)
				{
					ShowFor(cardModel);
				}
			}
		}
	}

	private static PreviewController? EnsureController()
	{
		NRun instance = NRun.Instance;
		NGlobalUi val = ((instance != null) ? instance.GlobalUi : null);
		NTopBar val2 = ((val != null) ? val.TopBar : null);
		if (val == null || val2 == null)
		{
			return null;
		}
		if (GodotObject.IsInstanceValid((GodotObject)(object)_controller) && ((Node)_controller).GetParent() == val)
		{
			return _controller;
		}
		PreviewController previewController = new PreviewController();
		((Node)previewController).Name = StringName.op_Implicit("PengoUnavailableEnchantmentPreview");
		((Control)previewController).MouseFilter = (MouseFilterEnum)2;
		_controller = previewController;
		((Control)_controller).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)val).AddChild((Node)(object)_controller, false, (InternalMode)0);
		((Node)val).MoveChild((Node)(object)_controller, ((Node)val2).GetIndex(false));
		return _controller;
	}

	private static void ShowFor(CardModel hoveredCard)
	{
		EnchantmentModel val = FindEnchantment(hoveredCard);
		if (val != null)
		{
			ShowFor(val);
		}
	}

	private static void ShowFor(EnchantmentModel enchantment)
	{
		Player val = FindLocalPlayer();
		PreviewController previewController = EnsureController();
		if (val == null || previewController == null)
		{
			return;
		}
		ClearEntries(animate: false);
		CardPile val2 = val.Piles.FirstOrDefault((Func<CardPile, bool>)((CardPile pile) => (int)pile.Type == 6));
		if (val2 != null)
		{
			List<CardModel> models = DeduplicateByTitle(val2.Cards.Where((CardModel card) => IsCardTypeRejected(enchantment, card)));
			List<CardModel> models2 = DeduplicateByTitle(val2.Cards.Where((CardModel card) => card.Enchantment != null));
			CreateColumn(models, TypeEntries);
			CreateColumn(models2, EnchantedEntries);
			LayoutColumn(TypeEntries, 0);
			LayoutColumn(EnchantedEntries, 1);
			_shown = true;
		}
	}

	private static EnchantmentModel? FindEnchantment(CardModel card)
	{
		Type type = ((object)card).GetType();
		if (card is TarCard)
		{
			return TarotDeck.All.FirstOrDefault((TarotDef def) => def.CardType == type)?.Enchantment;
		}
		if (card is PlanetCard)
		{
			return PlanetDeck.All.FirstOrDefault((PlanetDef def) => def.CardType == type)?.Enchantment;
		}
		return null;
	}

	private static Player? FindLocalPlayer()
	{
		RunState obj = RunManager.Instance.DebugOnlyGetState();
		if (obj == null)
		{
			return null;
		}
		return ((IEnumerable<Player>)obj.Players).FirstOrDefault((Func<Player, bool>)LocalContext.IsMe);
	}

	private static List<CardModel> DeduplicateByTitle(IEnumerable<CardModel> cards)
	{
		return (from @group in cards.GroupBy<CardModel, string>((CardModel card) => card.Title, StringComparer.CurrentCulture)
			select @group.First()).ToList();
	}

	private static bool IsCardTypeRejected(EnchantmentModel enchantment, CardModel card)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Invalid comparison between Unknown and I4
		bool flag = !enchantment.CanEnchantCardType(card.Type) || (card.Enchantment == null && !enchantment.CanEnchant(card));
		if (!flag)
		{
			CardType type = card.Type;
			bool flag2 = (((int)type == 0 || type - 4 <= 2) ? true : false);
			flag = flag2 && !enchantment.CanEnchant(card);
		}
		return flag;
	}

	private static void CreateColumn(IEnumerable<CardModel> models, List<PreviewEntry> entries)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		foreach (CardModel model in models)
		{
			NCard val = NCard.Create(model, (ModelVisibility)1);
			if (val != null)
			{
				((Control)val).MouseFilter = (MouseFilterEnum)2;
				((Control)val).Scale = Vector2.One * 0.3f;
				((Node)_controller).AddChild((Node)(object)val, false, (InternalMode)0);
				val.UpdateVisuals((PileType)0, (CardPreviewMode)1);
				((Node)val).AddChild((Node)(object)new InvalidCross(), false, (InternalMode)0);
				entries.Add(new PreviewEntry(val));
			}
		}
	}

	private static void LayoutColumn(List<PreviewEntry> entries, int column)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		if (_controller != null && entries.Count != 0)
		{
			Rect2 viewportRect = ((CanvasItem)_controller).GetViewportRect();
			Vector2 size = ((Rect2)(ref viewportRect)).Size;
			Vector2 val = NCard.defaultSize * 0.3f;
			float num = size.X - 18f - val.X * 0.5f - (float)(1 - column) * (val.X + 8f);
			float num2 = Mathf.Max(size.Y - 108f - 18f - val.Y, 0f);
			float num3 = ((entries.Count <= 1) ? 0f : Mathf.Min(val.Y + 5f, num2 / (float)(entries.Count - 1)));
			Vector2 val2 = default(Vector2);
			for (int i = 0; i < entries.Count; i++)
			{
				PreviewEntry previewEntry = entries[i];
				((Vector2)(ref val2))._002Ector(num, 108f + val.Y * 0.5f + num3 * (float)i);
				((Control)previewEntry.Card).Position = new Vector2(val2.X, (0f - val.Y) * 0.5f - (float)i * 5f);
				previewEntry.Tween = ((Node)previewEntry.Card).CreateTween();
				previewEntry.Tween.TweenProperty((GodotObject)(object)previewEntry.Card, NodePath.op_Implicit("position"), Variant.op_Implicit(val2), 0.3799999952316284).SetDelay((double)((float)i * 0.035f)).SetEase((EaseType)1)
					.SetTrans((TransitionType)10);
			}
		}
	}

	private static void RetractCards()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		if (!_shown && TypeEntries.Count == 0 && EnchantedEntries.Count == 0)
		{
			return;
		}
		_shown = false;
		PreviewController? controller = _controller;
		float num;
		if (controller == null)
		{
			num = 1920f;
		}
		else
		{
			Rect2 viewportRect = ((CanvasItem)controller).GetViewportRect();
			num = ((Rect2)(ref viewportRect)).Size.X;
		}
		float num2 = num;
		foreach (PreviewEntry item in TypeEntries.Concat(EnchantedEntries))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item.Card))
			{
				Tween? tween = item.Tween;
				if (tween != null)
				{
					tween.Kill();
				}
				item.Tween = ((Node)item.Card).CreateTween();
				item.Tween.TweenProperty((GodotObject)(object)item.Card, NodePath.op_Implicit("position:x"), Variant.op_Implicit(num2 + NCard.defaultSize.X), 0.3799999952316284).SetEase((EaseType)0).SetTrans((TransitionType)10);
				item.Tween.TweenCallback(Callable.From((Action)((Node)item.Card).QueueFree));
			}
		}
		TypeEntries.Clear();
		EnchantedEntries.Clear();
	}

	private static void ClearEntries(bool animate)
	{
		if (animate)
		{
			RetractCards();
			return;
		}
		foreach (PreviewEntry item in TypeEntries.Concat(EnchantedEntries))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item.Card))
			{
				((Node)item.Card).QueueFree();
			}
		}
		TypeEntries.Clear();
		EnchantedEntries.Clear();
		_shown = false;
	}

	private static void ControllerExited(PreviewController controller)
	{
		if (_controller == controller)
		{
			TypeEntries.Clear();
			EnchantedEntries.Clear();
			_controller = null;
			_hoveredHolder = null;
			_shown = false;
			_hoverSeconds = 0f;
			_unhoverSeconds = 0f;
		}
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.Fonts;
using MegaCrit.Sts2.Core.Runs;
using PengoTarot.Data.Judgement;

namespace PengoTarot.ConfigFW;

[ScriptPath("res://configFW/Scripts/NPortableTarotPopup.cs")]
public class NPortableTarotPopup : Control
{
	public class MethodName : MethodName
	{
		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName ApplyUseAvailability = StringName.op_Implicit("ApplyUseAvailability");

		public static readonly StringName OnUseButtonInput = StringName.op_Implicit("OnUseButtonInput");

		public static readonly StringName Use = StringName.op_Implicit("Use");

		public static readonly StringName OnDiscardButtonInput = StringName.op_Implicit("OnDiscardButtonInput");

		public static readonly StringName Discard = StringName.op_Implicit("Discard");

		public static readonly StringName _Input = StringName.op_Implicit("_Input");

		public static readonly StringName Remove = StringName.op_Implicit("Remove");

		public static readonly StringName _ExitTree = StringName.op_Implicit("_ExitTree");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName IsMarkedForRemoval = StringName.op_Implicit("IsMarkedForRemoval");

		public static readonly StringName _useButton = StringName.op_Implicit("_useButton");

		public static readonly StringName _discardButton = StringName.op_Implicit("_discardButton");

		public static readonly StringName _useBackground = StringName.op_Implicit("_useBackground");

		public static readonly StringName _discardBackground = StringName.op_Implicit("_discardBackground");

		public static readonly StringName _container = StringName.op_Implicit("_container");

		public static readonly StringName _tween = StringName.op_Implicit("_tween");

		public static readonly StringName _slotIndex = StringName.op_Implicit("_slotIndex");

		public static readonly StringName _tarotId = StringName.op_Implicit("_tarotId");

		public static readonly StringName _isReady = StringName.op_Implicit("_isReady");

		public static readonly StringName _isUsing = StringName.op_Implicit("_isUsing");
	}

	public class SignalName : SignalName
	{
	}

	private Control _useButton;

	private Control _discardButton;

	private TextureRect _useBackground;

	private TextureRect _discardBackground;

	private Control _container;

	private Tween? _tween;

	private Player _player;

	private int _slotIndex;

	private string _tarotId = "";

	private bool _isReady;

	private bool _isUsing;

	public bool IsMarkedForRemoval { get; private set; }

	public void Bind(Player player, int slotIndex, string tarotId, Vector2 cardGlobalPosition, Vector2 cardSize)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		_player = player;
		_slotIndex = slotIndex;
		_tarotId = tarotId;
		((Control)this).GlobalPosition = cardGlobalPosition + new Vector2(cardSize.X * 0.5f - ((Control)this).Size.X * 0.5f, cardSize.Y + 16f);
		if (_isReady)
		{
			ApplyUseAvailability();
		}
	}

	public override void _Ready()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		_container = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%Container"));
		_useButton = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%UseButton"));
		_discardButton = ((Node)this).GetNode<Control>(NodePath.op_Implicit("%DiscardButton"));
		_useBackground = ((Node)_useButton).GetNode<TextureRect>(NodePath.op_Implicit("Background"));
		_discardBackground = ((Node)_discardButton).GetNode<TextureRect>(NodePath.op_Implicit("Background"));
		Label node = ((Node)_useButton).GetNode<Label>(NodePath.op_Implicit("Label"));
		Label node2 = ((Node)_discardButton).GetNode<Label>(NodePath.op_Implicit("Label"));
		node.Text = new LocString("gameplay_ui", "BAL_PORTABLE_TAROT_USE").GetFormattedText();
		node2.Text = new LocString("gameplay_ui", "POTION_POPUP.discard").GetFormattedText();
		FontControlUtils.ApplyLocaleFontSubstitution((Control)(object)node, (FontType)0, StringName.op_Implicit("font"));
		FontControlUtils.ApplyLocaleFontSubstitution((Control)(object)node2, (FontType)0, StringName.op_Implicit("font"));
		((CanvasItem)_discardBackground).Modulate = Colors.Transparent;
		_useButton.MouseEntered += delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			if ((int)_useButton.MouseFilter == 0)
			{
				((CanvasItem)_useBackground).Modulate = new Color(1f, 1f, 1f, 0.25f);
				PortableTarotUseRouter.SetUseHoverGlow(_player, _tarotId, active: true);
			}
		};
		_useButton.MouseExited += delegate
		{
			PortableTarotUseRouter.ClearUseHoverGlow();
			ApplyUseAvailability();
		};
		_useButton.GuiInput += new GuiInputEventHandler(OnUseButtonInput);
		_discardButton.MouseEntered += delegate
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			((CanvasItem)_discardBackground).Modulate = new Color(1f, 1f, 1f, 0.25f);
		};
		_discardButton.MouseExited += delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			((CanvasItem)_discardBackground).Modulate = Colors.Transparent;
		};
		_discardButton.GuiInput += new GuiInputEventHandler(OnDiscardButtonInput);
		_isReady = true;
		ApplyUseAvailability();
		((CanvasItem)this).Modulate = Colors.Transparent;
		Control container = _container;
		container.Position += Vector2.Up * 25f;
		_tween = ((Node)this).CreateTween().SetParallel(true);
		_tween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.White), 0.10000000149011612).SetTrans((TransitionType)1);
		_tween.TweenProperty((GodotObject)(object)_container, NodePath.op_Implicit("position:y"), Variant.op_Implicit(_container.Position.Y + 25f), 0.15000000596046448).SetEase((EaseType)0).SetTrans((TransitionType)1);
	}

	private void ApplyUseAvailability()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		bool flag = !_isUsing && _player != null && PortableTarotUseRouter.CanUse(_player, _tarotId);
		((CanvasItem)_useBackground).Modulate = (Color)(flag ? Colors.Transparent : new Color(0.1f, 0.1f, 0.1f, 0.75f));
		_useButton.MouseFilter = (MouseFilterEnum)(flag ? 0 : 2);
	}

	private void OnUseButtonInput(InputEvent inputEvent)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null && (long)val.ButtonIndex == 1 && !val.Pressed)
		{
			Use();
		}
	}

	private void Use()
	{
		if (!IsMarkedForRemoval && !_isUsing)
		{
			_isUsing = true;
			_useButton.MouseFilter = (MouseFilterEnum)2;
			_discardButton.MouseFilter = (MouseFilterEnum)2;
			TaskHelper.RunSafely(UseAsync());
			Remove(immediate: true);
		}
	}

	private async Task UseAsync()
	{
		bool used = false;
		try
		{
			used = await PortableTarotUseRouter.TryUse(_player, _slotIndex, _tarotId);
		}
		finally
		{
			_isUsing = false;
			if (used)
			{
				Remove(immediate: true);
			}
			else if (!IsMarkedForRemoval)
			{
				ApplyUseAvailability();
				_discardButton.MouseFilter = (MouseFilterEnum)0;
			}
		}
	}

	private void OnDiscardButtonInput(InputEvent inputEvent)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null && (long)val.ButtonIndex == 1 && !val.Pressed)
		{
			Discard();
		}
	}

	private void Discard()
	{
		if (!IsMarkedForRemoval)
		{
			if (!JudgementTarotInventoryState.TryGetSlot(_player.NetId, _slotIndex, out string tarotId, out uint revision) || tarotId != _tarotId)
			{
				Remove(immediate: true);
				return;
			}
			_discardButton.MouseFilter = (MouseFilterEnum)2;
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)new DiscardPortableTarotAction(_player, _slotIndex, _tarotId, revision, CombatManager.Instance.IsInProgress));
			Remove(immediate: true);
		}
	}

	public override void _Input(InputEvent inputEvent)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I8
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Invalid comparison between Unknown and I8
		if (_isUsing)
		{
			return;
		}
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null && !val.Pressed && ((long)val.ButtonIndex == 1 || (long)val.ButtonIndex == 2))
		{
			Rect2 globalRect = ((Control)this).GetGlobalRect();
			if (!((Rect2)(ref globalRect)).HasPoint(((CanvasItem)this).GetGlobalMousePosition()))
			{
				Remove(immediate: true);
			}
		}
	}

	public void Remove(bool immediate = false)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		if (!IsMarkedForRemoval)
		{
			IsMarkedForRemoval = true;
			PortableTarotUseRouter.ClearUseHoverGlow();
			if (_useButton != null)
			{
				_useButton.MouseFilter = (MouseFilterEnum)2;
			}
			if (_discardButton != null)
			{
				_discardButton.MouseFilter = (MouseFilterEnum)2;
			}
			Tween? tween = _tween;
			if (tween != null)
			{
				tween.Kill();
			}
			if (immediate)
			{
				((Node)this).QueueFree();
				return;
			}
			_tween = ((Node)this).CreateTween().SetParallel(true);
			_tween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("modulate"), Variant.op_Implicit(Colors.Transparent), 0.10000000149011612);
			_tween.TweenProperty((GodotObject)(object)_container, NodePath.op_Implicit("position:y"), Variant.op_Implicit(-25f), 0.20000000298023224);
			_tween.Chain().TweenCallback(Callable.From((Action)((Node)this).QueueFree));
		}
	}

	public override void _ExitTree()
	{
		PortableTarotUseRouter.ClearUseHoverGlow();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Expected O, but got Unknown
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(9)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.ApplyUseAvailability, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnUseButtonInput, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Use, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnDiscardButtonInput, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Discard, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName._Input, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.Remove, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)1, StringName.op_Implicit("immediate"), (PropertyHint)0, "", (PropertyUsageFlags)6, false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.ApplyUseAvailability && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			ApplyUseAvailability();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnUseButtonInput && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnUseButtonInput(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Use && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Use();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnDiscardButtonInput && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnDiscardButtonInput(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Discard && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			Discard();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._Input && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			((Node)this)._Input(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Remove && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Remove(VariantUtils.ConvertTo<bool>(ref ((NativeVariantPtrArgs)(ref args))[0]));
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
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName.ApplyUseAvailability)
		{
			return true;
		}
		if ((ref method) == MethodName.OnUseButtonInput)
		{
			return true;
		}
		if ((ref method) == MethodName.Use)
		{
			return true;
		}
		if ((ref method) == MethodName.OnDiscardButtonInput)
		{
			return true;
		}
		if ((ref method) == MethodName.Discard)
		{
			return true;
		}
		if ((ref method) == MethodName._Input)
		{
			return true;
		}
		if ((ref method) == MethodName.Remove)
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
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if ((ref name) == PropertyName.IsMarkedForRemoval)
		{
			IsMarkedForRemoval = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._useButton)
		{
			_useButton = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._discardButton)
		{
			_discardButton = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._useBackground)
		{
			_useBackground = VariantUtils.ConvertTo<TextureRect>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._discardBackground)
		{
			_discardBackground = VariantUtils.ConvertTo<TextureRect>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._container)
		{
			_container = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._tween)
		{
			_tween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._slotIndex)
		{
			_slotIndex = VariantUtils.ConvertTo<int>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._tarotId)
		{
			_tarotId = VariantUtils.ConvertTo<string>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._isReady)
		{
			_isReady = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._isUsing)
		{
			_isUsing = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		return ((GodotObject)this).SetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		if ((ref name) == PropertyName.IsMarkedForRemoval)
		{
			bool isMarkedForRemoval = IsMarkedForRemoval;
			value = VariantUtils.CreateFrom<bool>(ref isMarkedForRemoval);
			return true;
		}
		if ((ref name) == PropertyName._useButton)
		{
			value = VariantUtils.CreateFrom<Control>(ref _useButton);
			return true;
		}
		if ((ref name) == PropertyName._discardButton)
		{
			value = VariantUtils.CreateFrom<Control>(ref _discardButton);
			return true;
		}
		if ((ref name) == PropertyName._useBackground)
		{
			value = VariantUtils.CreateFrom<TextureRect>(ref _useBackground);
			return true;
		}
		if ((ref name) == PropertyName._discardBackground)
		{
			value = VariantUtils.CreateFrom<TextureRect>(ref _discardBackground);
			return true;
		}
		if ((ref name) == PropertyName._container)
		{
			value = VariantUtils.CreateFrom<Control>(ref _container);
			return true;
		}
		if ((ref name) == PropertyName._tween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _tween);
			return true;
		}
		if ((ref name) == PropertyName._slotIndex)
		{
			value = VariantUtils.CreateFrom<int>(ref _slotIndex);
			return true;
		}
		if ((ref name) == PropertyName._tarotId)
		{
			value = VariantUtils.CreateFrom<string>(ref _tarotId);
			return true;
		}
		if ((ref name) == PropertyName._isReady)
		{
			value = VariantUtils.CreateFrom<bool>(ref _isReady);
			return true;
		}
		if ((ref name) == PropertyName._isUsing)
		{
			value = VariantUtils.CreateFrom<bool>(ref _isUsing);
			return true;
		}
		return ((GodotObject)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._useButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._discardButton, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._useBackground, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._discardBackground, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._container, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._tween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)2, PropertyName._slotIndex, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)4, PropertyName._tarotId, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._isReady, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._isUsing, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName.IsMarkedForRemoval, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)this).SaveGodotObjectData(info);
		StringName isMarkedForRemoval = PropertyName.IsMarkedForRemoval;
		bool isMarkedForRemoval2 = IsMarkedForRemoval;
		info.AddProperty(isMarkedForRemoval, Variant.From<bool>(ref isMarkedForRemoval2));
		info.AddProperty(PropertyName._useButton, Variant.From<Control>(ref _useButton));
		info.AddProperty(PropertyName._discardButton, Variant.From<Control>(ref _discardButton));
		info.AddProperty(PropertyName._useBackground, Variant.From<TextureRect>(ref _useBackground));
		info.AddProperty(PropertyName._discardBackground, Variant.From<TextureRect>(ref _discardBackground));
		info.AddProperty(PropertyName._container, Variant.From<Control>(ref _container));
		info.AddProperty(PropertyName._tween, Variant.From<Tween>(ref _tween));
		info.AddProperty(PropertyName._slotIndex, Variant.From<int>(ref _slotIndex));
		info.AddProperty(PropertyName._tarotId, Variant.From<string>(ref _tarotId));
		info.AddProperty(PropertyName._isReady, Variant.From<bool>(ref _isReady));
		info.AddProperty(PropertyName._isUsing, Variant.From<bool>(ref _isUsing));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((GodotObject)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName.IsMarkedForRemoval, ref val))
		{
			IsMarkedForRemoval = ((Variant)(ref val)).As<bool>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._useButton, ref val2))
		{
			_useButton = ((Variant)(ref val2)).As<Control>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._discardButton, ref val3))
		{
			_discardButton = ((Variant)(ref val3)).As<Control>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._useBackground, ref val4))
		{
			_useBackground = ((Variant)(ref val4)).As<TextureRect>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._discardBackground, ref val5))
		{
			_discardBackground = ((Variant)(ref val5)).As<TextureRect>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._container, ref val6))
		{
			_container = ((Variant)(ref val6)).As<Control>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._tween, ref val7))
		{
			_tween = ((Variant)(ref val7)).As<Tween>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._slotIndex, ref val8))
		{
			_slotIndex = ((Variant)(ref val8)).As<int>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._tarotId, ref val9))
		{
			_tarotId = ((Variant)(ref val9)).As<string>();
		}
		Variant val10 = default(Variant);
		if (info.TryGetProperty(PropertyName._isReady, ref val10))
		{
			_isReady = ((Variant)(ref val10)).As<bool>();
		}
		Variant val11 = default(Variant);
		if (info.TryGetProperty(PropertyName._isUsing, ref val11))
		{
			_isUsing = ((Variant)(ref val11)).As<bool>();
		}
	}
}

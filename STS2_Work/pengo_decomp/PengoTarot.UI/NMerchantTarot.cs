using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using MegaCrit.Sts2.addons.mega_text;
using PengoTarot.Data;

namespace PengoTarot.UI;

[ScriptPath("res://Data/merchantroom/NMerchantTarot.cs")]
public class NMerchantTarot : NMerchantSlot
{
	public class MethodName : MethodName
	{
		public static readonly StringName _Ready = StringName.op_Implicit("_Ready");

		public static readonly StringName _GuiInput = StringName.op_Implicit("_GuiInput");

		public static readonly StringName OnFocus = StringName.op_Implicit("OnFocus");

		public static readonly StringName OnUnfocus = StringName.op_Implicit("OnUnfocus");

		public static readonly StringName OnMousePressed = StringName.op_Implicit("OnMousePressed");

		public static readonly StringName OnMouseReleased = StringName.op_Implicit("OnMouseReleased");

		public static readonly StringName OnPreview = StringName.op_Implicit("OnPreview");

		public static readonly StringName CreateHoverTip = StringName.op_Implicit("CreateHoverTip");

		public static readonly StringName UpdateVisual = StringName.op_Implicit("UpdateVisual");

		public static readonly StringName Initialize = StringName.op_Implicit("Initialize");

		public static readonly StringName _ExitTree = StringName.op_Implicit("_ExitTree");
	}

	public class PropertyName : PropertyName
	{
		public static readonly StringName Visual = StringName.op_Implicit("Visual");

		public static readonly StringName _visual = StringName.op_Implicit("_visual");

		public static readonly StringName _costContainer = StringName.op_Implicit("_costContainer");

		public static readonly StringName _isHovered = StringName.op_Implicit("_isHovered");

		public static readonly StringName _hoverTween = StringName.op_Implicit("_hoverTween");

		public static readonly StringName _merchantRug = StringName.op_Implicit("_merchantRug");

		public static readonly StringName _ignoreMouseRelease = StringName.op_Implicit("_ignoreMouseRelease");

		public static readonly StringName _isAnimating = StringName.op_Implicit("_isAnimating");

		public static readonly StringName _hasPlayedFirstAnimation = StringName.op_Implicit("_hasPlayedFirstAnimation");

		public static readonly StringName _audioPlayer = StringName.op_Implicit("_audioPlayer");
	}

	public class SignalName : SignalName
	{
	}

	private Sprite2D _visual;

	private Control _costContainer;

	private MerchantTarotEntry _entry;

	private bool _isHovered;

	private Tween? _hoverTween;

	private NMerchantInventory? _merchantRug;

	private bool _ignoreMouseRelease;

	private bool _isAnimating;

	private bool _hasPlayedFirstAnimation;

	private AudioStreamPlayer? _audioPlayer;

	private static readonly Vector2 _hoverScale = Vector2.One * 0.8f;

	private static readonly Vector2 _smallScale = Vector2.One * 0.65f;

	public override MerchantEntry Entry => (MerchantEntry)(object)_entry;

	protected override CanvasItem Visual => (CanvasItem)(object)_visual;

	public override void _Ready()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Expected O, but got Unknown
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected O, but got Unknown
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Expected O, but got Unknown
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_035b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0413: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0465: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_047a: Expected O, but got Unknown
		int value = GD.RandRange(1, 4);
		Texture2D texture = ResourceLoader.Load<Texture2D>($"res://images/rooms/merchant_room/TarotEntry{value}.png", (string)null, (CacheMode)1);
		_visual = new Sprite2D();
		((Node)_visual).Name = StringName.op_Implicit("Visual");
		((Node2D)_visual).Scale = new Vector2(0.559943f, 0.559943f);
		((Node2D)_visual).Position = new Vector2(1.53845f, -1.53851f);
		_visual.Texture = texture;
		Sprite2D val = new Sprite2D();
		((CanvasItem)val).Modulate = new Color(0f, 0f, 0f, 0.25f);
		((CanvasItem)val).ShowBehindParent = true;
		val.Texture = texture;
		val.Offset = new Vector2(16f, 16f);
		((Node)_visual).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)this).AddChild((Node)(object)_visual, false, (InternalMode)0);
		HBoxContainer val2 = new HBoxContainer();
		((Node)val2).Name = StringName.op_Implicit("Cost");
		((Control)val2).LayoutMode = 0;
		((BoxContainer)val2).Alignment = (AlignmentMode)1;
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Control)val2).Position = new Vector2(-55f, 185f);
		_costContainer = (Control)(object)val2;
		((Node)this).AddChild((Node)(object)_costContainer, false, (InternalMode)0);
		TextureRect val3 = new TextureRect();
		((Control)val3).CustomMinimumSize = new Vector2(54f, 54f);
		val3.StretchMode = (StretchModeEnum)5;
		val3.ExpandMode = (ExpandModeEnum)1;
		val3.Texture = ResourceLoader.Load<Texture2D>("res://images/atlases/ui_atlas.sprites/top_bar/top_bar_gold.tres", (string)null, (CacheMode)1);
		((Node)_costContainer).AddChild((Node)(object)val3, false, (InternalMode)0);
		FontVariation val4 = ResourceLoader.Load<FontVariation>("res://themes/kreon_bold_glyph_space_two.tres", (string)null, (CacheMode)1);
		base._costLabel = new MegaLabel();
		((Node)base._costLabel).Name = StringName.op_Implicit("CostLabel");
		((Control)base._costLabel).AddThemeFontOverride(StringName.op_Implicit("font"), (Font)(object)val4);
		((Control)base._costLabel).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 39);
		((Label)base._costLabel).Text = "99";
		((Control)base._costLabel).AddThemeConstantOverride(StringName.op_Implicit("line_spacing"), 0);
		((Control)base._costLabel).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 15);
		((Control)base._costLabel).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color(0.0666667f, 0f, 0f, 0.431373f));
		((Control)base._costLabel).AddThemeConstantOverride(StringName.op_Implicit("shadow_outline_size"), 0);
		((Control)base._costLabel).AddThemeColorOverride(StringName.op_Implicit("font_shadow_color"), new Color(0.223529f, 0.223529f, 0.223529f, 0f));
		base._costLabel.MinFontSize = 40;
		((Node)_costContainer).AddChild((Node)(object)base._costLabel, false, (InternalMode)0);
		base._hitbox = new NClickableControl();
		((Control)base._hitbox).LayoutMode = 0;
		((Node)base._hitbox).Name = StringName.op_Implicit("Hitbox");
		((Control)base._hitbox).SetPosition(new Vector2(-157f, -126f), false);
		((Control)base._hitbox).SetSize(new Vector2(308f, 300f), false);
		((Control)base._hitbox).PivotOffset = new Vector2(56f, 56f);
		((Node)this).AddChild((Node)(object)base._hitbox, false, (InternalMode)0);
		((GodotObject)this).Connect(SignalName.FocusEntered, Callable.From((Action)OnFocus), 0u);
		((GodotObject)this).Connect(SignalName.FocusExited, Callable.From((Action)OnUnfocus), 0u);
		((GodotObject)base._hitbox).Connect(SignalName.MouseEntered, Callable.From((Action)OnFocus), 0u);
		((GodotObject)base._hitbox).Connect(SignalName.MouseExited, Callable.From((Action)OnUnfocus), 0u);
		((GodotObject)base._hitbox).Connect(SignalName.MousePressed, Callable.From<InputEvent>((Action<InputEvent>)OnMousePressed), 0u);
		((GodotObject)base._hitbox).Connect(SignalName.MouseReleased, Callable.From<InputEvent>((Action<InputEvent>)OnMouseReleased), 0u);
		((Control)this).Scale = _smallScale;
		((Control)base._hitbox).MouseFilter = (MouseFilterEnum)2;
		((GodotObject)base._hitbox).SetDeferred(StringName.op_Implicit("mouse_filter"), Variant.op_Implicit(0));
		_audioPlayer = new AudioStreamPlayer();
		_audioPlayer.Bus = StringName.op_Implicit("SFX");
		string text = "res://audio/tarot_open.ogg";
		if (ResourceLoader.Exists(text, ""))
		{
			_audioPlayer.Stream = ResourceLoader.Load<AudioStream>(text, (string)null, (CacheMode)1);
			_audioPlayer.VolumeDb = 15f;
		}
		else
		{
			GD.PushWarning("找不到音频: " + text);
		}
		((Node)this).AddChild((Node)(object)_audioPlayer, false, (InternalMode)0);
	}

	public override void _GuiInput(InputEvent inputEvent)
	{
		if (!_isAnimating)
		{
			if (inputEvent.IsActionPressed(MegaInput.select, false, false))
			{
				((Control)this).AcceptEvent();
				TaskHelper.RunSafely(OnSelected());
			}
			else if (inputEvent.IsActionPressed(MegaInput.confirm, false, false))
			{
				((Control)this).AcceptEvent();
				((NMerchantSlot)this).OnPreview();
			}
		}
	}

	private void OnFocus()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!_isAnimating)
		{
			_isHovered = true;
			Tween? hoverTween = _hoverTween;
			if (hoverTween != null)
			{
				hoverTween.Kill();
			}
			((Control)this).Scale = _hoverScale;
			((NMerchantSlot)this).CreateHoverTip();
			((GodotObject)this).EmitSignal(SignalName.Hovered, (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)this) });
		}
	}

	private void OnUnfocus()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!_isAnimating)
		{
			_isHovered = false;
			Tween? hoverTween = _hoverTween;
			if (hoverTween != null)
			{
				hoverTween.Kill();
			}
			_hoverTween = ((Node)this).CreateTween();
			_hoverTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("scale"), Variant.op_Implicit(_smallScale), 0.5).SetEase((EaseType)1).SetTrans((TransitionType)5);
			((NMerchantSlot)this).ClearHoverTip();
			((GodotObject)this).EmitSignal(SignalName.Unhovered, (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)this) });
		}
	}

	private void OnMousePressed(InputEvent inputEvent)
	{
		_ignoreMouseRelease = false;
	}

	private void OnMouseReleased(InputEvent inputEvent)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I8
		if (_isAnimating || !_isHovered || _ignoreMouseRelease)
		{
			return;
		}
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null)
		{
			if ((long)val.ButtonIndex == 1)
			{
				TaskHelper.RunSafely(OnSelected());
			}
			else
			{
				((NMerchantSlot)this).OnPreview();
			}
		}
	}

	private async Task OnSelected()
	{
		if (_isAnimating || !((NMerchantSlot)this).Entry.IsStocked)
		{
			return;
		}
		((NMerchantSlot)this).ClearHoverTip();
		if (!_hasPlayedFirstAnimation)
		{
			_hasPlayedFirstAnimation = true;
			await PlayOpenAnimation();
		}
		MerchantTarotEntry entry = _entry;
		if (entry == null || !entry.HasPurchased)
		{
			NMerchantTarot nMerchantTarot = this;
			NMerchantInventory? merchantRug = _merchantRug;
			await ((NMerchantSlot)nMerchantTarot).OnTryPurchase((merchantRug != null) ? merchantRug.Inventory : null);
			((NMerchantSlot)this).UpdateVisual();
			if (((NMerchantSlot)this).Entry.IsStocked && _isHovered)
			{
				((NMerchantSlot)this).CreateHoverTip();
			}
		}
	}

	private async Task PlayOpenAnimation()
	{
		_isAnimating = true;
		((Control)base._hitbox).MouseFilter = (MouseFilterEnum)2;
		_isHovered = false;
		Tween? hoverTween = _hoverTween;
		if (hoverTween != null)
		{
			hoverTween.Kill();
		}
		Rect2 rect = ((Control)this).GetRect();
		Vector2 size = ((Rect2)(ref rect)).Size;
		((Control)this).PivotOffset = size / 2f;
		Vector2 startScale = ((Control)this).Scale;
		AudioStreamPlayer? audioPlayer = _audioPlayer;
		if (audioPlayer != null)
		{
			audioPlayer.Play(0f);
		}
		Tween val = ((Node)this).CreateTween();
		val.SetParallel(false);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(1f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(-2f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(3f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(-5f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(8f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(-13f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(21f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(-34f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(19f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(-7f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(3f), 0.05000000074505806).SetEase((EaseType)2);
		val.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("rotation_degrees"), Variant.op_Implicit(0f), 0.05000000074505806).SetEase((EaseType)2);
		Tween scaleTween = ((Node)this).CreateTween();
		scaleTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("scale"), Variant.op_Implicit(startScale * 1.1f), 0.4000000059604645).SetEase((EaseType)1);
		scaleTween.TweenProperty((GodotObject)(object)this, NodePath.op_Implicit("scale"), Variant.op_Implicit(startScale), 0.10000000149011612).SetEase((EaseType)0);
		await ((GodotObject)this).ToSignal((GodotObject)(object)val, StringName.op_Implicit("finished"));
		if (scaleTween.IsRunning())
		{
			await ((GodotObject)this).ToSignal((GodotObject)(object)scaleTween, StringName.op_Implicit("finished"));
		}
		((Control)this).Scale = startScale;
		((Control)this).RotationDegrees = 0f;
		((Control)this).PivotOffset = Vector2.Zero;
		_isAnimating = false;
		((Control)base._hitbox).MouseFilter = (MouseFilterEnum)0;
	}

	protected override void OnPreview()
	{
	}

	protected override async Task OnTryPurchase(MerchantInventory? inventory)
	{
		await ((MerchantEntry)_entry).OnTryPurchaseWrapper(inventory, false);
	}

	protected override void CreateHoverTip()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		LocString val = new LocString("merchant_room", "TAROT_PILE_ENTRY.title");
		LocString val2 = new LocString("merchant_room", "TAROT_PILE_ENTRY.description");
		NHoverTipSet val3 = NHoverTipSet.CreateAndShow((Control)(object)this, (IHoverTip)(object)new HoverTip(val, val2, (Texture2D)null), (HoverTipAlignment)0);
		if (val3 != null)
		{
			((Control)val3).GlobalPosition = ((Control)this).GlobalPosition + new Vector2(-180f, -280f);
		}
	}

	protected override void UpdateVisual()
	{
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)this) && GodotObject.IsInstanceValid((GodotObject)(object)_costContainer))
		{
			if (((NMerchantSlot)this).Entry is MerchantTarotEntry { HasPurchased: not false } merchantTarotEntry)
			{
				((CanvasItem)_costContainer).Visible = false;
				((Control)base._hitbox).MouseFilter = (MouseFilterEnum)0;
				((CanvasItem)_visual).Modulate = (Color)(((MerchantEntry)merchantTarotEntry).IsStocked ? Colors.White : new Color(0.3f, 0.3f, 0.3f, 1f));
				((Control)this).FocusMode = (FocusModeEnum)0;
			}
			else if (((NMerchantSlot)this).Entry.IsStocked)
			{
				((CanvasItem)_costContainer).Visible = true;
				base._costLabel.SetTextAutoSize(((NMerchantSlot)this).Entry.Cost.ToString());
				((CanvasItem)base._costLabel).Modulate = (((NMerchantSlot)this).Entry.EnoughGold ? new Color("ffd9b3") : new Color(0.9f, 0.25f, 0.25f, 1f));
				((Control)base._hitbox).MouseFilter = (MouseFilterEnum)0;
				((CanvasItem)_visual).Modulate = Colors.White;
				((Control)this).FocusMode = (FocusModeEnum)2;
			}
			else
			{
				((CanvasItem)_costContainer).Visible = false;
				((Control)base._hitbox).MouseFilter = (MouseFilterEnum)2;
				((CanvasItem)_visual).Modulate = new Color(0.3f, 0.3f, 0.3f, 1f);
				((Control)this).FocusMode = (FocusModeEnum)0;
			}
		}
	}

	public void FillSlot(MerchantTarotEntry entry)
	{
		_entry = entry;
		((MerchantEntry)_entry).EntryUpdated += ((NMerchantSlot)this).UpdateVisual;
		((MerchantEntry)_entry).PurchaseFailed += base.OnPurchaseFailed;
		((MerchantEntry)_entry).PurchaseCompleted += OnSuccessfulPurchase;
		((NMerchantSlot)this).UpdateVisual();
	}

	private void OnSuccessfulPurchase(PurchaseStatus status, MerchantEntry entry)
	{
		((NMerchantSlot)this).UpdateVisual();
	}

	public void Initialize(NMerchantInventory rug)
	{
		_merchantRug = rug;
		((NMerchantSlot)this).Initialize(rug);
	}

	public override void _ExitTree()
	{
		if (_entry != null)
		{
			((MerchantEntry)_entry).EntryUpdated -= ((NMerchantSlot)this).UpdateVisual;
			((MerchantEntry)_entry).PurchaseFailed -= base.OnPurchaseFailed;
			((MerchantEntry)_entry).PurchaseCompleted -= OnSuccessfulPurchase;
		}
		NMerchantInventory? merchantRug = _merchantRug;
		object obj;
		if (merchantRug == null)
		{
			obj = null;
		}
		else
		{
			MerchantInventory inventory = merchantRug.Inventory;
			obj = ((inventory != null) ? inventory.Player : null);
		}
		if (obj != null)
		{
			_merchantRug.Inventory.Player.GoldChanged -= ((NMerchantSlot)this).UpdateVisual;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<MethodInfo> GetGodotMethodList()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Expected O, but got Unknown
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Expected O, but got Unknown
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		return new List<MethodInfo>(11)
		{
			new MethodInfo(MethodName._Ready, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName._GuiInput, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnFocus, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnUnfocus, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.OnMousePressed, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnMouseReleased, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("inputEvent"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("InputEvent"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName.OnPreview, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.CreateHoverTip, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.UpdateVisual, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null),
			new MethodInfo(MethodName.Initialize, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, new List<PropertyInfo>
			{
				new PropertyInfo((Type)24, StringName.op_Implicit("rug"), (PropertyHint)0, "", (PropertyUsageFlags)6, new StringName("Control"), false)
			}, (List<Variant>)null),
			new MethodInfo(MethodName._ExitTree, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)1, (List<PropertyInfo>)null, (List<Variant>)null)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		if ((ref method) == MethodName._Ready && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._Ready();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._GuiInput && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			((Control)this)._GuiInput(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnFocus && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnFocus();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnUnfocus && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			OnUnfocus();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnMousePressed && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnMousePressed(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnMouseReleased && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			OnMouseReleased(VariantUtils.ConvertTo<InputEvent>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.OnPreview && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((NMerchantSlot)this).OnPreview();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.CreateHoverTip && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((NMerchantSlot)this).CreateHoverTip();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.UpdateVisual && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((NMerchantSlot)this).UpdateVisual();
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 1)
		{
			Initialize(VariantUtils.ConvertTo<NMerchantInventory>(ref ((NativeVariantPtrArgs)(ref args))[0]));
			ret = default(godot_variant);
			return true;
		}
		if ((ref method) == MethodName._ExitTree && ((NativeVariantPtrArgs)(ref args)).Count == 0)
		{
			((Node)this)._ExitTree();
			ret = default(godot_variant);
			return true;
		}
		return ((NMerchantSlot)this).InvokeGodotClassMethod(ref method, args, ref ret);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool HasGodotClassMethod(in godot_string_name method)
	{
		if ((ref method) == MethodName._Ready)
		{
			return true;
		}
		if ((ref method) == MethodName._GuiInput)
		{
			return true;
		}
		if ((ref method) == MethodName.OnFocus)
		{
			return true;
		}
		if ((ref method) == MethodName.OnUnfocus)
		{
			return true;
		}
		if ((ref method) == MethodName.OnMousePressed)
		{
			return true;
		}
		if ((ref method) == MethodName.OnMouseReleased)
		{
			return true;
		}
		if ((ref method) == MethodName.OnPreview)
		{
			return true;
		}
		if ((ref method) == MethodName.CreateHoverTip)
		{
			return true;
		}
		if ((ref method) == MethodName.UpdateVisual)
		{
			return true;
		}
		if ((ref method) == MethodName.Initialize)
		{
			return true;
		}
		if ((ref method) == MethodName._ExitTree)
		{
			return true;
		}
		return ((NMerchantSlot)this).HasGodotClassMethod(ref method);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override bool SetGodotClassPropertyValue(in godot_string_name name, in godot_variant value)
	{
		if ((ref name) == PropertyName._visual)
		{
			_visual = VariantUtils.ConvertTo<Sprite2D>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._costContainer)
		{
			_costContainer = VariantUtils.ConvertTo<Control>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._isHovered)
		{
			_isHovered = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._hoverTween)
		{
			_hoverTween = VariantUtils.ConvertTo<Tween>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._merchantRug)
		{
			_merchantRug = VariantUtils.ConvertTo<NMerchantInventory>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._ignoreMouseRelease)
		{
			_ignoreMouseRelease = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._isAnimating)
		{
			_isAnimating = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._hasPlayedFirstAnimation)
		{
			_hasPlayedFirstAnimation = VariantUtils.ConvertTo<bool>(ref value);
			return true;
		}
		if ((ref name) == PropertyName._audioPlayer)
		{
			_audioPlayer = VariantUtils.ConvertTo<AudioStreamPlayer>(ref value);
			return true;
		}
		return ((NMerchantSlot)this).SetGodotClassPropertyValue(ref name, ref value);
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
		if ((ref name) == PropertyName.Visual)
		{
			CanvasItem visual = ((NMerchantSlot)this).Visual;
			value = VariantUtils.CreateFrom<CanvasItem>(ref visual);
			return true;
		}
		if ((ref name) == PropertyName._visual)
		{
			value = VariantUtils.CreateFrom<Sprite2D>(ref _visual);
			return true;
		}
		if ((ref name) == PropertyName._costContainer)
		{
			value = VariantUtils.CreateFrom<Control>(ref _costContainer);
			return true;
		}
		if ((ref name) == PropertyName._isHovered)
		{
			value = VariantUtils.CreateFrom<bool>(ref _isHovered);
			return true;
		}
		if ((ref name) == PropertyName._hoverTween)
		{
			value = VariantUtils.CreateFrom<Tween>(ref _hoverTween);
			return true;
		}
		if ((ref name) == PropertyName._merchantRug)
		{
			value = VariantUtils.CreateFrom<NMerchantInventory>(ref _merchantRug);
			return true;
		}
		if ((ref name) == PropertyName._ignoreMouseRelease)
		{
			value = VariantUtils.CreateFrom<bool>(ref _ignoreMouseRelease);
			return true;
		}
		if ((ref name) == PropertyName._isAnimating)
		{
			value = VariantUtils.CreateFrom<bool>(ref _isAnimating);
			return true;
		}
		if ((ref name) == PropertyName._hasPlayedFirstAnimation)
		{
			value = VariantUtils.CreateFrom<bool>(ref _hasPlayedFirstAnimation);
			return true;
		}
		if ((ref name) == PropertyName._audioPlayer)
		{
			value = VariantUtils.CreateFrom<AudioStreamPlayer>(ref _audioPlayer);
			return true;
		}
		return ((NMerchantSlot)this).GetGodotClassPropertyValue(ref name, ref value);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal static List<PropertyInfo> GetGodotPropertyList()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		return new List<PropertyInfo>
		{
			new PropertyInfo((Type)24, PropertyName._visual, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._costContainer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._isHovered, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._hoverTween, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._merchantRug, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._ignoreMouseRelease, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._isAnimating, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)1, PropertyName._hasPlayedFirstAnimation, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName._audioPlayer, (PropertyHint)0, "", (PropertyUsageFlags)4096, false),
			new PropertyInfo((Type)24, PropertyName.Visual, (PropertyHint)0, "", (PropertyUsageFlags)4096, false)
		};
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void SaveGodotObjectData(GodotSerializationInfo info)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		((NMerchantSlot)this).SaveGodotObjectData(info);
		info.AddProperty(PropertyName._visual, Variant.From<Sprite2D>(ref _visual));
		info.AddProperty(PropertyName._costContainer, Variant.From<Control>(ref _costContainer));
		info.AddProperty(PropertyName._isHovered, Variant.From<bool>(ref _isHovered));
		info.AddProperty(PropertyName._hoverTween, Variant.From<Tween>(ref _hoverTween));
		info.AddProperty(PropertyName._merchantRug, Variant.From<NMerchantInventory>(ref _merchantRug));
		info.AddProperty(PropertyName._ignoreMouseRelease, Variant.From<bool>(ref _ignoreMouseRelease));
		info.AddProperty(PropertyName._isAnimating, Variant.From<bool>(ref _isAnimating));
		info.AddProperty(PropertyName._hasPlayedFirstAnimation, Variant.From<bool>(ref _hasPlayedFirstAnimation));
		info.AddProperty(PropertyName._audioPlayer, Variant.From<AudioStreamPlayer>(ref _audioPlayer));
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected override void RestoreGodotObjectData(GodotSerializationInfo info)
	{
		((NMerchantSlot)this).RestoreGodotObjectData(info);
		Variant val = default(Variant);
		if (info.TryGetProperty(PropertyName._visual, ref val))
		{
			_visual = ((Variant)(ref val)).As<Sprite2D>();
		}
		Variant val2 = default(Variant);
		if (info.TryGetProperty(PropertyName._costContainer, ref val2))
		{
			_costContainer = ((Variant)(ref val2)).As<Control>();
		}
		Variant val3 = default(Variant);
		if (info.TryGetProperty(PropertyName._isHovered, ref val3))
		{
			_isHovered = ((Variant)(ref val3)).As<bool>();
		}
		Variant val4 = default(Variant);
		if (info.TryGetProperty(PropertyName._hoverTween, ref val4))
		{
			_hoverTween = ((Variant)(ref val4)).As<Tween>();
		}
		Variant val5 = default(Variant);
		if (info.TryGetProperty(PropertyName._merchantRug, ref val5))
		{
			_merchantRug = ((Variant)(ref val5)).As<NMerchantInventory>();
		}
		Variant val6 = default(Variant);
		if (info.TryGetProperty(PropertyName._ignoreMouseRelease, ref val6))
		{
			_ignoreMouseRelease = ((Variant)(ref val6)).As<bool>();
		}
		Variant val7 = default(Variant);
		if (info.TryGetProperty(PropertyName._isAnimating, ref val7))
		{
			_isAnimating = ((Variant)(ref val7)).As<bool>();
		}
		Variant val8 = default(Variant);
		if (info.TryGetProperty(PropertyName._hasPlayedFirstAnimation, ref val8))
		{
			_hasPlayedFirstAnimation = ((Variant)(ref val8)).As<bool>();
		}
		Variant val9 = default(Variant);
		if (info.TryGetProperty(PropertyName._audioPlayer, ref val9))
		{
			_audioPlayer = ((Variant)(ref val9)).As<AudioStreamPlayer>();
		}
	}
}

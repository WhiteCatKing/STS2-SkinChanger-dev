using System;
using System.Collections.Generic;
using CznCore.Godot;
using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;

namespace CznCore.Presentation;

internal sealed class CombatCutInToggleHudRuntime
{
	private readonly CombatCutInToggleHudProfile _profile;

	private readonly ICombatCutInToggleHudHost _host;

	private readonly Dictionary<string, Texture2D> _textureCache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);

	private readonly HashSet<string> _warnedMissingTexturePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private Button? _button;

	private TextureRect? _icon;

	private NCombatUi? _combatUi;

	private bool _isHovering;

	private bool _uiEnabled = true;

	public CombatCutInToggleHudRuntime(CombatCutInToggleHudProfile profile, ICombatCutInToggleHudHost host)
	{
		_profile = profile ?? throw new ArgumentNullException("profile");
		_host = host ?? throw new ArgumentNullException("host");
	}

	public void EnsureAttached(NCombatUi? ui)
	{
		try
		{
			if (!_host.IsAvailable() || !GodotNodeUtil.IsAlive((GodotObject?)(object)ui))
			{
				Cleanup();
				return;
			}
			_combatUi = ui;
			Button button = ResolveOrCreateButton(ui);
			Refresh(button);
		}
		catch (Exception ex)
		{
			_host.Warn("Combat cut-in toggle HUD failed: " + SkinLog.FormatException(ex), 2);
		}
	}

	public void SetCombatUiEnabled(bool enabled, NCombatUi? ui)
	{
		_uiEnabled = enabled;
		if (enabled)
		{
			EnsureAttached(ui);
		}
		else
		{
			RefreshExistingButton();
		}
	}

	public void Cleanup()
	{
		try
		{
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button))
			{
				((CanvasItem)_button).Visible = false;
				((Node)_button).QueueFree();
			}
		}
		catch (Exception ex)
		{
			_host.Warn("Combat cut-in toggle HUD cleanup failed: " + SkinLog.FormatException(ex), 2);
		}
		finally
		{
			HideHoverTip();
			_button = null;
			_icon = null;
			_combatUi = null;
			_isHovering = false;
			_uiEnabled = true;
		}
	}

	private Button ResolveOrCreateButton(NCombatUi ui)
	{
		Control val = ResolveButtonParent(ui);
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button) && ((Node)_button).GetParent() == val)
		{
			EnsureIcon(_button);
			return _button;
		}
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button))
		{
			((Node)_button).QueueFree();
		}
		_button = null;
		_icon = null;
		string text = _host.RuntimeNodeName(_profile.ButtonNodeNameSuffix);
		Node nodeOrNull = ((Node)val).GetNodeOrNull(NodePath.op_Implicit(text));
		Button val2 = (Button)(object)((nodeOrNull is Button) ? nodeOrNull : null);
		if (val2 != null)
		{
			_button = val2;
			EnsureIcon(val2);
			return val2;
		}
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)nodeOrNull))
		{
			CanvasItem val3 = (CanvasItem)(object)((nodeOrNull is CanvasItem) ? nodeOrNull : null);
			if (val3 != null)
			{
				val3.Visible = false;
			}
			nodeOrNull.QueueFree();
		}
		Button val4 = CreateButton(text);
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		_button = val4;
		EnsureIcon(val4);
		return val4;
	}

	private Control ResolveButtonParent(NCombatUi ui)
	{
		if (!_profile.UsePileRelativePlacement || !GodotNodeUtil.IsAlive((GodotObject?)(object)ui.DiscardPile))
		{
			return (Control)(object)ui;
		}
		return (Control)(object)ui.DiscardPile;
	}

	private Button CreateButton(string nodeName)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Expected O, but got Unknown
		//IL_010b: Expected O, but got Unknown
		float num = Math.Max(1f, _profile.Size);
		Button val = new Button
		{
			Name = StringName.op_Implicit(nodeName),
			Text = string.Empty,
			CustomMinimumSize = new Vector2(num, num),
			MouseFilter = (MouseFilterEnum)0,
			FocusMode = (FocusModeEnum)0,
			ZIndex = 1000,
			AnchorLeft = 1f,
			AnchorTop = 1f,
			AnchorRight = 1f,
			AnchorBottom = 1f,
			OffsetRight = _profile.OffsetRight,
			OffsetBottom = _profile.OffsetBottom,
			OffsetLeft = _profile.OffsetRight - num,
			OffsetTop = _profile.OffsetBottom - num
		};
		((BaseButton)val).Pressed += OnPressed;
		((Control)val).MouseEntered += OnMouseEntered;
		((Control)val).MouseExited += OnMouseExited;
		ApplyButtonStyle(val);
		return val;
	}

	private void EnsureIcon(Button button)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected O, but got Unknown
		if (!GodotNodeUtil.IsAlive((GodotObject?)(object)_icon) || ((Node)_icon).GetParent() != button)
		{
			string text = _host.RuntimeNodeName(_profile.IconNodeNameSuffix);
			_icon = ((Node)button).GetNodeOrNull<TextureRect>(NodePath.op_Implicit(text));
			if (!GodotNodeUtil.IsAlive((GodotObject?)(object)_icon))
			{
				_icon = new TextureRect
				{
					Name = StringName.op_Implicit(text),
					MouseFilter = (MouseFilterEnum)2,
					StretchMode = (StretchModeEnum)5
				};
				((Control)_icon).SetAnchorsPreset((LayoutPreset)15, false);
				((Control)_icon).OffsetLeft = 0f;
				((Control)_icon).OffsetTop = 0f;
				((Control)_icon).OffsetRight = 0f;
				((Control)_icon).OffsetBottom = 0f;
				((Node)button).AddChild((Node)(object)_icon, false, (InternalMode)0);
			}
		}
	}

	private void Refresh(Button button)
	{
		bool enabled = _host.IsCutInEnabled();
		Texture2D texture = GetTexture(enabled);
		NCombatUi val = (GodotNodeUtil.IsAlive((GodotObject?)(object)_combatUi) ? _combatUi : null);
		if (val != null && _profile.UsePileRelativePlacement)
		{
			PlaceRelativeToDiscardPile(button, val);
		}
		((CanvasItem)button).Visible = texture != null && _uiEnabled && val != null && _host.ShouldShow(val);
		((BaseButton)button).Disabled = !((CanvasItem)button).Visible;
		if (!((CanvasItem)button).Visible)
		{
			HideHoverTip(button);
		}
		else if (_isHovering)
		{
			ShowHoverTip(button);
		}
		((Control)button).TooltipText = string.Empty;
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_icon))
		{
			_icon.Texture = texture;
		}
	}

	private void RefreshExistingButton()
	{
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button))
		{
			Refresh(_button);
		}
	}

	private void OnPressed()
	{
		bool flag = !_host.IsCutInEnabled();
		_host.SetCutInEnabled(flag, persist: true);
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button))
		{
			Refresh(_button);
			if (_isHovering)
			{
				ShowHoverTip(_button);
			}
		}
		_host.Debug("Combat cut-in toggle set to " + (flag ? "on" : "off"), 2);
	}

	private void OnMouseEntered()
	{
		_isHovering = true;
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button) && ((CanvasItem)_button).Visible)
		{
			ShowHoverTip(_button);
		}
	}

	private void OnMouseExited()
	{
		_isHovering = false;
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)_button))
		{
			HideHoverTip(_button);
		}
	}

	private void ShowHoverTip(Button button)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)button))
		{
			HideHoverTip(button);
			HoverTip? val = CreateHoverTip(_host.IsCutInEnabled());
			if (val.HasValue)
			{
				NHoverTipSet hoverTipSet = NHoverTipSet.CreateAndShow((Control)(object)button, (IHoverTip)(object)val.Value, (HoverTipAlignment)0);
				PlaceHoverTip(hoverTipSet, button);
			}
		}
	}

	private void HideHoverTip(Button? button = null)
	{
		Button val = button ?? _button;
		if (GodotNodeUtil.IsAlive((GodotObject?)(object)val))
		{
			NHoverTipSet.Remove((Control)(object)val);
		}
	}

	private Texture2D? GetTexture(bool enabled)
	{
		string text = (enabled ? _profile.EnabledTexturePath : _profile.DisabledTexturePath);
		if (string.IsNullOrWhiteSpace(text))
		{
			return null;
		}
		if (_textureCache.TryGetValue(text, out Texture2D value) && GodotObject.IsInstanceValid((GodotObject)(object)value))
		{
			return value;
		}
		Texture2D val = LoadTexture2D(text);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			_textureCache[text] = val;
			return val;
		}
		if (_warnedMissingTexturePaths.Add(text))
		{
			_host.Warn("Combat cut-in toggle texture missing: " + text, 2);
		}
		return null;
	}

	private void PlaceRelativeToDiscardPile(Button button, NCombatUi ui)
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			if (GodotNodeUtil.IsAlive((GodotObject?)(object)ui.DiscardPile))
			{
				float num = Math.Max(1f, _profile.Size);
				if (((Node)button).GetParent() == ui.DiscardPile)
				{
					((Control)button).AnchorLeft = 0.5f;
					((Control)button).AnchorRight = 0.5f;
					((Control)button).AnchorTop = 0f;
					((Control)button).AnchorBottom = 0f;
					((Control)button).OffsetLeft = num * -0.5f;
					((Control)button).OffsetRight = num * 0.5f;
					((Control)button).OffsetTop = 0f - num - 8f;
					((Control)button).OffsetBottom = -8f;
				}
				else
				{
					Vector2 val = ToLocal((Control)(object)button, ((Control)ui.DiscardPile).GlobalPosition + ((Control)ui.DiscardPile).Size * 0.5f);
					((Control)button).AnchorLeft = 0f;
					((Control)button).AnchorTop = 0f;
					((Control)button).AnchorRight = 0f;
					((Control)button).AnchorBottom = 0f;
					((Control)button).OffsetLeft = val.X - num * 0.5f;
					((Control)button).OffsetTop = val.Y - ((Control)ui.DiscardPile).Size.Y * 0.5f - num - 8f;
					((Control)button).OffsetRight = ((Control)button).OffsetLeft + num;
					((Control)button).OffsetBottom = ((Control)button).OffsetTop + num;
				}
			}
		}
		catch (Exception ex)
		{
			_host.Debug("Combat cut-in toggle discard-relative placement failed: " + SkinLog.FormatException(ex), 3);
		}
	}

	private static Vector2 ToLocal(Control localSpace, Vector2 globalPosition)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		Node parent = ((Node)localSpace).GetParent();
		Control val = (Control)(object)((parent is Control) ? parent : null);
		if (val == null)
		{
			return globalPosition;
		}
		Transform2D globalTransform = ((CanvasItem)val).GetGlobalTransform();
		return ((Transform2D)(ref globalTransform)).AffineInverse() * globalPosition;
	}

	private HoverTip? CreateHoverTip(bool enabled)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		//IL_008f: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		string text = (enabled ? _profile.EnabledHoverTipTitleKey : _profile.DisabledHoverTipTitleKey);
		string text2 = (enabled ? _profile.EnabledHoverTipDescriptionKey : _profile.DisabledHoverTipDescriptionKey);
		if (string.IsNullOrWhiteSpace(_profile.HoverTipLocTable) || string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(text2))
		{
			return null;
		}
		try
		{
			LocString val = new LocString(_profile.HoverTipLocTable, text);
			LocString val2 = new LocString(_profile.HoverTipLocTable, text2);
			return new HoverTip(val, val2, (Texture2D)null);
		}
		catch (Exception ex)
		{
			_host.Warn("Combat cut-in toggle hover tip localization failed: " + SkinLog.FormatException(ex), 2);
			return null;
		}
	}

	private void PlaceHoverTip(NHoverTipSet? hoverTipSet, Button button)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (hoverTipSet == null)
		{
			return;
		}
		try
		{
			if (_profile.UseDiscardPileHoverTipPlacement && GodotNodeUtil.IsAlive((GodotObject?)(object)_combatUi))
			{
				NCombatUi combatUi = _combatUi;
				if (combatUi != null && GodotNodeUtil.IsAlive((GodotObject?)(object)combatUi.DiscardPile))
				{
					((Control)hoverTipSet).GlobalPosition = ((Control)combatUi.DiscardPile).GlobalPosition + new Vector2(_profile.HoverTipDiscardPileOffsetX, _profile.HoverTipDiscardPileOffsetY);
					return;
				}
			}
			hoverTipSet.SetAlignment((Control)(object)button, (HoverTipAlignment)1);
		}
		catch (Exception ex)
		{
			_host.Debug("Combat cut-in toggle hover tip placement failed: " + SkinLog.FormatException(ex), 3);
		}
	}

	private static Texture2D? LoadTexture2D(string texturePath)
	{
		if (IsRawImagePath(texturePath))
		{
			Texture2D val = LoadImageTexture2D(texturePath);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				return val;
			}
		}
		try
		{
			return ResourceLoader.Load<Texture2D>(texturePath, (string)null, (CacheMode)1);
		}
		catch
		{
		}
		if (!IsRawImagePath(texturePath))
		{
			return LoadImageTexture2D(texturePath);
		}
		return null;
	}

	private static bool IsRawImagePath(string texturePath)
	{
		if (!texturePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !texturePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !texturePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
		{
			return texturePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static Texture2D? LoadImageTexture2D(string texturePath)
	{
		try
		{
			if (!FileAccess.FileExists(texturePath))
			{
				return null;
			}
			Image val = Image.LoadFromFile(texturePath);
			if (val == null || val.IsEmpty())
			{
				return null;
			}
			return (Texture2D?)(object)ImageTexture.CreateFromImage(val);
		}
		catch
		{
			return null;
		}
	}

	private static void ApplyButtonStyle(Button button)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		StyleBoxEmpty val = new StyleBoxEmpty();
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)val);
		((Control)button).AddThemeStyleboxOverride(StringName.op_Implicit("disabled"), (StyleBox)(object)val);
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color(1f, 1f, 1f, 0f));
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), new Color(1f, 1f, 1f, 0f));
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), new Color(1f, 1f, 1f, 0f));
		((Control)button).AddThemeColorOverride(StringName.op_Implicit("font_disabled_color"), new Color(1f, 1f, 1f, 0f));
	}
}

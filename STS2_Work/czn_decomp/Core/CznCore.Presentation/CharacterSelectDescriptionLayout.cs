using System;
using Godot;

namespace CznCore.Presentation;

internal sealed class CharacterSelectDescriptionLayout
{
	private readonly Control _frame;

	private readonly VBoxContainer _column;

	private readonly RichTextLabel _description;

	private readonly Control _relic;

	private readonly Rect2 _columnOffsets;

	private readonly Vector2 _descriptionMinimum;

	private readonly Vector2 _relicMinimum;

	private readonly bool _fitContent;

	private readonly bool _scrollActive;

	private readonly bool _clipContents;

	private readonly int? _boldSize;

	private readonly string _nativeText;

	private readonly string _formattedText;

	private bool _restored;

	private CharacterSelectDescriptionLayout(Control frame, VBoxContainer column, RichTextLabel description, Control relic)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		_frame = frame;
		_column = column;
		_description = description;
		_relic = relic;
		_columnOffsets = new Rect2(((Control)column).OffsetLeft, ((Control)column).OffsetTop, ((Control)column).OffsetRight - ((Control)column).OffsetLeft, ((Control)column).OffsetBottom - ((Control)column).OffsetTop);
		_descriptionMinimum = ((Control)description).CustomMinimumSize;
		_relicMinimum = relic.CustomMinimumSize;
		_fitContent = description.FitContent;
		_scrollActive = description.ScrollActive;
		_clipContents = ((Control)description).ClipContents;
		_boldSize = (((Control)description).HasThemeFontSizeOverride(StringName.op_Implicit("bold_font_size")) ? new int?(((Control)description).GetThemeFontSize(StringName.op_Implicit("bold_font_size"), (StringName)null)) : ((int?)null));
		_nativeText = description.Text;
		int num = _nativeText.IndexOf('\n');
		string formattedText;
		if (num <= 0 || _nativeText.StartsWith("[b]", StringComparison.Ordinal))
		{
			formattedText = _nativeText;
		}
		else
		{
			string text = _nativeText.Substring(0, num);
			string nativeText = _nativeText;
			int num2 = num;
			formattedText = "[b]" + text + "[/b]" + nativeText.Substring(num2, nativeText.Length - num2);
		}
		_formattedText = formattedText;
	}

	public static CharacterSelectDescriptionLayout? Apply(Control screen)
	{
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("InfoPanel/NinePatchRect"));
		VBoxContainer nodeOrNull2 = ((Node)screen).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("InfoPanel/VBoxContainer"));
		if (nodeOrNull == null || nodeOrNull2 == null)
		{
			return null;
		}
		CharacterSelectDescriptionLayout characterSelectDescriptionLayout = new CharacterSelectDescriptionLayout(nodeOrNull, nodeOrNull2, ((Node)nodeOrNull2).GetNode<RichTextLabel>(NodePath.op_Implicit("DescriptionLabel")), ((Node)nodeOrNull2).GetNode<Control>(NodePath.op_Implicit("Relic")));
		characterSelectDescriptionLayout.Activate();
		return characterSelectDescriptionLayout;
	}

	public void Restore()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		_restored = true;
		_frame.Resized -= Layout;
		int? boldSize = _boldSize;
		if (boldSize.HasValue)
		{
			int valueOrDefault = boldSize.GetValueOrDefault();
			((Control)_description).AddThemeFontSizeOverride(StringName.op_Implicit("bold_font_size"), valueOrDefault);
		}
		else
		{
			((Control)_description).RemoveThemeFontSizeOverride(StringName.op_Implicit("bold_font_size"));
		}
		_description.FitContent = _fitContent;
		_description.ScrollActive = _scrollActive;
		((Control)_description).ClipContents = _clipContents;
		((Control)_description).CustomMinimumSize = _descriptionMinimum;
		_relic.CustomMinimumSize = _relicMinimum;
		if (_description.Text == _formattedText)
		{
			_description.Text = _nativeText;
		}
		((Control)_column).OffsetLeft = ((Rect2)(ref _columnOffsets)).Position.X;
		((Control)_column).OffsetTop = ((Rect2)(ref _columnOffsets)).Position.Y;
		((Control)_column).OffsetRight = ((Rect2)(ref _columnOffsets)).End.X;
		((Control)_column).OffsetBottom = ((Rect2)(ref _columnOffsets)).End.Y;
	}

	private void Activate()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		((Control)_description).AddThemeFontSizeOverride(StringName.op_Implicit("bold_font_size"), ((Control)_description).GetThemeFontSize(StringName.op_Implicit("normal_font_size"), (StringName)null));
		_description.Text = _formattedText;
		_description.FitContent = false;
		_description.ScrollActive = true;
		((Control)_description).ClipContents = true;
		_description.ScrollToLine(0);
		_frame.Resized += Layout;
		Layout();
		Callable val = Callable.From((Action)Layout);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void Layout()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		if (_restored)
		{
			return;
		}
		RichTextLabel node = ((Node)_relic).GetNode<RichTextLabel>(NodePath.op_Implicit("Description"));
		float num = Math.Max(128f, ((Control)node).Position.Y + (float)node.GetContentHeight() + 8f);
		_relic.CustomMinimumSize = new Vector2(_relicMinimum.X, num);
		float num2 = Math.Max(0, ((Node)_column).GetChildCount(false) - 1) * ((Control)_column).GetThemeConstant(StringName.op_Implicit("separation"), (StringName)null);
		foreach (Node child in ((Node)_column).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && val != _description && ((CanvasItem)val).Visible)
			{
				num2 += val.GetCombinedMinimumSize().Y;
			}
		}
		((Control)_description).CustomMinimumSize = new Vector2(_descriptionMinimum.X, Math.Max(48f, _frame.Size.Y - num2));
		((Control)_column).Position = new Vector2(((Rect2)(ref _columnOffsets)).Position.X, _frame.Position.Y);
		((Control)_column).Size = new Vector2(((Rect2)(ref _columnOffsets)).Size.X, _frame.Size.Y);
	}
}

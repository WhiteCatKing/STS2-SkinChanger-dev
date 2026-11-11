using Godot;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal sealed class CompendiumBackdrop
{
	public void AddPanel(Control parent)
	{
		ModThemeBackdrop background = ModThemeBackdrop.For(parent);
		ModThemeRuntime.Bind((Node)(object)parent, "backdrop", delegate(ModThemeSettings theme)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			background.Update(ModThemeRuntime.Tint(theme.PanelColor, theme.PanelOpacity), theme.PanelBlur, theme.CornerRadius);
		});
	}

	public void SetSelected(Button button, bool selected)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		bool num = !((GodotObject)button).HasMeta(StringName.op_Implicit("sc_theme_selected"));
		((GodotObject)button).SetMeta(StringName.op_Implicit("sc_theme_selected"), Variant.op_Implicit(selected));
		ModThemeBackdrop background = ModThemeBackdrop.For((Control)(object)button);
		if (num)
		{
			((CanvasItem)button).Draw += Refresh;
		}
		ModThemeRuntime.Bind((Node)(object)button, "selection", delegate
		{
			Refresh();
		});
		void Refresh()
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)button) && !((GodotObject)button).IsQueuedForDeletion())
			{
				ModThemeSettings current = ModThemeRuntime.Current;
				Button obj = button;
				StringName obj2 = StringName.op_Implicit("sc_theme_selected");
				Variant val = default(Variant);
				val = ((GodotObject)obj).GetMeta(obj2, val);
				var (tint, blur, visible) = Appearance(current, ((Variant)(ref val)).AsBool(), ((BaseButton)button).IsHovered(), ((BaseButton)button).Disabled);
				background.Update(tint, blur, current.CornerRadius, visible);
			}
		}
	}

	internal static (Color Tint, float Blur, bool Visible) Appearance(ModThemeSettings theme, bool selected, bool hovered, bool disabled)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		hovered = hovered && !disabled;
		return (Tint: ModThemeRuntime.ButtonTint(theme, hovered, selected, disabled), Blur: (!selected) ? theme.ButtonBlur : (hovered ? theme.SelectionHoverBlur : theme.SelectionBlur), Visible: selected || hovered);
	}
}

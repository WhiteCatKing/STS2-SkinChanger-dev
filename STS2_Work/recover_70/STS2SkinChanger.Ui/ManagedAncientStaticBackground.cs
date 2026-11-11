using Godot;
using MegaCrit.Sts2.Core.Nodes.Screens;

namespace STS2SkinChanger.Ui;

internal static class ManagedAncientStaticBackground
{
	private const string ManagedRootMeta = "sts2_skin_changer_static_ancient_background";

	internal static void Mark(Control root)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)root).SetMeta(StringName.op_Implicit("sts2_skin_changer_static_ancient_background"), Variant.op_Implicit(true));
	}

	internal static void Fit(Node sceneRoot)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		Control val = (Control)(object)((sceneRoot is Control) ? sceneRoot : null);
		if (val == null || !((GodotObject)val).HasMeta(StringName.op_Implicit("sts2_skin_changer_static_ancient_background")))
		{
			return;
		}
		StringName obj = StringName.op_Implicit("sts2_skin_changer_static_ancient_background");
		Variant val2 = default(Variant);
		val2 = ((GodotObject)val).GetMeta(obj, val2);
		if (!((Variant)(ref val2)).AsBool())
		{
			return;
		}
		val.SetAnchorsPreset((LayoutPreset)15, false);
		Node parent = ((Node)val).GetParent();
		NAncientBgContainer val3 = (NAncientBgContainer)(object)((parent is NAncientBgContainer) ? parent : null);
		if (val3 == null)
		{
			val.OffsetLeft = 0f;
			val.OffsetTop = 0f;
			val.OffsetRight = 0f;
			val.OffsetBottom = 0f;
			return;
		}
		Vector2 scale = ((Control)val3).Scale;
		if (!Mathf.IsZeroApprox(scale.X) && !Mathf.IsZeroApprox(scale.Y))
		{
			Vector2 pivotOffset = ((Control)val3).PivotOffset;
			Vector2 position = ((Control)val3).Position;
			Vector2 size = ((Control)val3).Size;
			Vector2 val4 = default(Vector2);
			((Vector2)(ref val4))._002Ector(pivotOffset.X + (0f - position.X - pivotOffset.X) / scale.X, pivotOffset.Y + (0f - position.Y - pivotOffset.Y) / scale.Y);
			Vector2 val5 = default(Vector2);
			((Vector2)(ref val5))._002Ector(pivotOffset.X + (size.X - position.X - pivotOffset.X) / scale.X, pivotOffset.Y + (size.Y - position.Y - pivotOffset.Y) / scale.Y);
			val.OffsetLeft = val4.X;
			val.OffsetTop = val4.Y;
			val.OffsetRight = val5.X - size.X;
			val.OffsetBottom = val5.Y - size.Y;
		}
	}

	internal static void FitChildren(NAncientBgContainer container)
	{
		foreach (Node child in ((Node)container).GetChildren(false))
		{
			Fit(child);
		}
	}
}

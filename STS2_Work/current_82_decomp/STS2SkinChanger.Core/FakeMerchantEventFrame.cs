using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;

namespace STS2SkinChanger.Core;

internal static class FakeMerchantEventFrame
{
	private const string SpritePath = "SceneContainer/BgContainer/FakeMerchantBackground";

	private const string ContainerPath = "SceneContainer/BgContainer";

	private const string CutterPath = "SceneContainer/SpineSprite2";

	private const float HorizontalShift = 41f;

	private const int MaxWaitFrames = 300;

	private const float MinArtSize = 8f;

	public static void Schedule(NFakeMerchant screen)
	{
		try
		{
			FitAsync(screen);
		}
		catch (Exception ex)
		{
			ModLog.Warn("假商人事件构图补丁未能启动，保持原构图：" + ex.GetBaseException().Message);
		}
	}

	private static async Task FitAsync(NFakeMerchant screen)
	{
		try
		{
			SceneTree tree = ((Node)screen).GetTree();
			if (tree == null)
			{
				return;
			}
			Node2D sprite = ((Node)screen).GetNodeOrNull<Node2D>(NodePath.op_Implicit("SceneContainer/BgContainer/FakeMerchantBackground"));
			Control container = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("SceneContainer/BgContainer"));
			Node2D cutter = ((Node)screen).GetNodeOrNull<Node2D>(NodePath.op_Implicit("SceneContainer/SpineSprite2"));
			if (sprite == null || container == null)
			{
				ModLog.Warn("假商人事件场景缺少 SceneContainer/BgContainer/FakeMerchantBackground，跳过构图调整。");
				return;
			}
			Rect2 bounds = default(Rect2);
			bool ready = false;
			for (int frame = 0; frame < 300; frame++)
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)screen) || !GodotObject.IsInstanceValid((GodotObject)(object)sprite))
				{
					return;
				}
				Rect2 val = ReadBounds(sprite);
				if (((Rect2)(ref val)).Size.X > 8f && ((Rect2)(ref val)).Size.Y > 8f)
				{
					bounds = val;
					ready = true;
					break;
				}
				await ((GodotObject)screen).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
			}
			if (!ready)
			{
				ModLog.Warn($"等不到假商人帐篷的骨架范围（{300} 帧），保持原构图。");
				return;
			}
			Transform2D globalTransform = ((CanvasItem)screen).GetGlobalTransform();
			Vector2 scale = ((Transform2D)(ref globalTransform)).Scale;
			Rect2 val2 = default(Rect2);
			((Rect2)(ref val2))._002Ector(((Control)screen).GetGlobalPosition(), ((Control)screen).Size * scale);
			if (((Rect2)(ref val2)).Size.X < 64f || ((Rect2)(ref val2)).Size.Y < 64f)
			{
				ModLog.Warn($"假商人事件画面尺寸异常（{((Rect2)(ref val2)).Size}），保持原构图。");
				return;
			}
			Vector2 val3 = sprite.Scale;
			if (val3.X <= 0.0001f || val3.Y <= 0.0001f)
			{
				val3 = Vector2.One;
			}
			Vector2 val4 = sprite.Position + ((Rect2)(ref bounds)).Position * val3;
			Vector2 val5 = ((Rect2)(ref bounds)).Size * val3;
			if (val5.X < 8f || val5.Y < 8f || val5.X > 100000f || val5.Y > 100000f)
			{
				ModLog.Warn($"假商人帐篷范围异常（{val5}），保持原构图。");
				return;
			}
			float num = Mathf.Max(((Rect2)(ref val2)).Size.X / val5.X, ((Rect2)(ref val2)).Size.Y / val5.Y);
			Vector2 val6 = val4 + val5 * 0.5f;
			Vector2 val7 = ((Rect2)(ref val2)).Position + ((Rect2)(ref val2)).Size * 0.5f - val6 * num + new Vector2(41f, 0f);
			ApplyGlobalTransform((CanvasItem)(object)container, new Transform2D(0f, new Vector2(num, num), 0f, val7));
			if (cutter != null && GodotObject.IsInstanceValid((GodotObject)(object)cutter))
			{
				Vector2 val8 = default(Vector2);
				((Vector2)(ref val8))._002Ector(num * val3.X, num * val3.Y);
				ApplyGlobalTransform((CanvasItem)(object)cutter, new Transform2D(0f, val8, 0f, val7));
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("假商人事件构图调整失败，保持游戏原构图：" + ex.GetBaseException().Message);
		}
	}

	private static Rect2 ReadBounds(Node2D sprite)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			MegaSkeleton skeleton = new MegaSprite(Variant.op_Implicit((GodotObject)(object)sprite)).GetSkeleton();
			return (Rect2)((skeleton != null) ? skeleton.GetBounds() : default(Rect2));
		}
		catch
		{
			return default(Rect2);
		}
	}

	private static void ApplyGlobalTransform(CanvasItem item, Transform2D global)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Node parent = ((Node)item).GetParent();
		Node obj = ((parent is CanvasItem) ? parent : null);
		Transform2D val = ((obj != null) ? ((CanvasItem)obj).GetGlobalTransform() : Transform2D.Identity);
		Transform2D val2 = ((Transform2D)(ref val)).AffineInverse() * global;
		Control val3 = (Control)(object)((item is Control) ? item : null);
		if (val3 == null)
		{
			Node2D val4 = (Node2D)(object)((item is Node2D) ? item : null);
			if (val4 != null)
			{
				val4.Transform = val2;
			}
		}
		else
		{
			val3.Scale = ((Transform2D)(ref val2)).Scale;
			val3.Position = val2.Origin;
		}
	}
}

using System;
using CznEnemySkin.Domain;
using Godot;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace CznEnemySkin.Infrastructure.Runtime;

internal static class EnemyIntentVisibilityGuard
{
	private static readonly StringName IntentClampLoggedMeta = new StringName("czn_enemy_skin_intent_top_clamp_logged");

	private static readonly StringName NativeIntentPositionMeta = new StringName("czn_enemy_skin_native_intent_position");

	private static readonly StringName RefreshQueuedMeta = new StringName("czn_enemy_skin_intent_refresh_queued");

	private const string TopBarBackgroundPath = "BgImage";

	private const string SkinnedTopBarBackgroundPath = "CznCombatTopBarBackground";

	public static void OnBoundsUpdated(NCreature creature)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)creature.IntentContainer))
		{
			((GodotObject)creature.IntentContainer).SetMeta(NativeIntentPositionMeta, Variant.op_Implicit(creature.IntentContainer.Position));
		}
		ClampToReadableTop(creature);
		QueueRefresh(creature);
	}

	public static void QueueRefresh(NCreature creature)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (!GodotObject.IsInstanceValid((GodotObject)(object)creature) || !((Node)creature).IsInsideTree() || !EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile _) || ((GodotObject)creature).HasMeta(RefreshQueuedMeta))
		{
			return;
		}
		((GodotObject)creature).SetMeta(RefreshQueuedMeta, Variant.op_Implicit(true));
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)creature))
			{
				((GodotObject)creature).RemoveMeta(RefreshQueuedMeta);
				if (((Node)creature).IsInsideTree())
				{
					ClampToReadableTop(creature);
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	internal static void ClampPosition(Control intentContainer, float minimumTop)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)intentContainer).HasMeta(NativeIntentPositionMeta))
		{
			((GodotObject)intentContainer).SetMeta(NativeIntentPositionMeta, Variant.op_Implicit(intentContainer.Position));
		}
		StringName nativeIntentPositionMeta = NativeIntentPositionMeta;
		Variant val = default(Variant);
		val = ((GodotObject)intentContainer).GetMeta(nativeIntentPositionMeta, val);
		intentContainer.Position = ((Variant)(ref val)).AsVector2();
		if (intentContainer.GlobalPosition.Y < minimumTop)
		{
			intentContainer.GlobalPosition = new Vector2(intentContainer.GlobalPosition.X, minimumTop);
		}
	}

	public static void ClampToReadableTop(NCreature creature)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (EnemyVisualHostAdapter.TryResolveAppliedProfile(creature, out EnemyReplacementProfile profile) && GodotObject.IsInstanceValid((GodotObject)(object)creature.IntentContainer) && ((Node)creature).IsInsideTree())
		{
			EnemyPresentationDepthPolicy.EnsureCreatureUiAbovePresentation(creature);
			Control intentContainer = creature.IntentContainer;
			float num = ResolveReadableTop(creature);
			float y = intentContainer.GlobalPosition.Y;
			ClampPosition(intentContainer, num);
			if (!(y >= num) && !((GodotObject)creature).HasMeta(IntentClampLoggedMeta))
			{
				StringName intentClampLoggedMeta = IntentClampLoggedMeta;
				bool flag = true;
				((GodotObject)creature).SetMeta(intentClampLoggedMeta, Variant.From<bool>(ref flag));
				Entry.Log.Info("Clamped replacement enemy intent below the readable top edge " + $"[profile={profile.Id}, fromY={y:F3}, " + $"toY={num:F3}].", 1);
			}
		}
	}

	private static float ResolveReadableTop(NCreature creature)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		NRun instance = NRun.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			NGlobalUi globalUi = instance.GlobalUi;
			obj = ((globalUi != null) ? globalUi.TopBar : null);
		}
		Control val = (Control)obj;
		Control val2 = ((val != null) ? ((Node)val).GetNodeOrNull<Control>(NodePath.op_Implicit("BgImage")) : null);
		Rect2 viewportRect;
		if (val != null && val2 != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && GodotObject.IsInstanceValid((GodotObject)(object)val2) && ((CanvasItem)val).IsVisibleInTree())
		{
			viewportRect = ((CanvasItem)creature).GetViewportRect();
			float y = ((Rect2)(ref viewportRect)).Position.Y;
			Rect2 globalRect = val2.GetGlobalRect();
			float num = Math.Max(((Rect2)(ref globalRect)).End.Y, y + ((Rect2)(ref globalRect)).Size.Y);
			Control nodeOrNull = ((Node)val).GetNodeOrNull<Control>(NodePath.op_Implicit("CznCombatTopBarBackground"));
			if (nodeOrNull != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull) && ((CanvasItem)nodeOrNull).IsVisibleInTree())
			{
				Rect2 globalRect2 = nodeOrNull.GetGlobalRect();
				num = Math.Max(num, Math.Max(((Rect2)(ref globalRect2)).End.Y, y + ((Rect2)(ref globalRect2)).Size.Y));
			}
			return num;
		}
		viewportRect = ((CanvasItem)creature).GetViewportRect();
		return ((Rect2)(ref viewportRect)).Position.Y;
	}
}

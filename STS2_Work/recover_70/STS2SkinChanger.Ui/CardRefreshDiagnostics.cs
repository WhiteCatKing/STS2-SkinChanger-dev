using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CardRefreshDiagnostics
{
	private sealed class TraceState(long sequence, CardModel model, string selection)
	{
		public long Sequence { get; } = sequence;

		public CardModel Model { get; } = model;

		public string Selection { get; } = selection;

		public bool Active { get; set; } = true;
	}

	private static readonly ConditionalWeakTable<NCard, TraceState> Traces = new ConditionalWeakTable<NCard, TraceState>();

	private static long _sequence;

	public static void Begin(NCard card, bool enabled)
	{
		if (!enabled || !GodotObject.IsInstanceValid((GodotObject)(object)card) || card.Model == null)
		{
			return;
		}
		try
		{
			TraceState value = new TraceState(++_sequence, card.Model, SkinService.GetEffectiveCardSelection(card.Model));
			Traces.Remove(card);
			Traces.Add(card, value);
			Record(card, "before");
		}
		catch (Exception ex)
		{
			ModLog.Warn("启动卡面切换诊断失败：" + ex.GetBaseException().Message);
		}
	}

	public static void Record(NCard card, string stage, MethodBase? source = null)
	{
		if (Traces.TryGetValue(card, out TraceState value) && value.Active && card.Model == value.Model)
		{
			WriteSnapshot(card, value, (source == null) ? stage : (source.Name + "/" + stage));
		}
	}

	public static void End(NCard card)
	{
		if (!Traces.TryGetValue(card, out TraceState state) || !state.Active)
		{
			return;
		}
		Record(card, "complete");
		state.Active = false;
		try
		{
			SceneTree tree = ((Node)card).GetTree();
			if (tree == null)
			{
				return;
			}
			WeakReference<NCard> weakCard = new WeakReference<NCard>(card);
			tree.CreateTimer(0.2, true, false, false).Timeout += delegate
			{
				if (weakCard.TryGetTarget(out NCard target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && !((GodotObject)target).IsQueuedForDeletion() && target.Model == state.Model && Traces.TryGetValue(target, out TraceState value) && value == state)
				{
					WriteSnapshot(target, state, "settled-200ms");
				}
			};
		}
		catch (Exception ex)
		{
			ModLog.Warn("安排卡面延迟诊断失败：" + ex.GetBaseException().Message);
		}
	}

	private static void WriteSnapshot(NCard card, TraceState state, string stage)
	{
		try
		{
			string[] source = new string[6] { "%PortraitCanvasGroup", "%Portrait", "%AncientPortrait", "%Frame", "%AncientBorder", "%AncientTextBg" };
			ModLog.Info($"[card-refresh:{state.Sequence}] card={((AbstractModel)state.Model).Id}, node={((GodotObject)card).GetInstanceId()}, selected={state.Selection}, stage={stage}; " + string.Join("; ", source.Select((string path) => NodeSnapshot(card, path))));
		}
		catch (Exception ex)
		{
			ModLog.Warn("记录卡面切换状态失败：" + ex.GetBaseException().Message);
		}
	}

	private static string NodeSnapshot(NCard card, string path)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		CanvasItem nodeOrNull = ((Node)card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit(path));
		if (nodeOrNull == null || !GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull))
		{
			return path + "=missing";
		}
		DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 6);
		defaultInterpolatedStringHandler.AppendFormatted(path);
		defaultInterpolatedStringHandler.AppendLiteral(":visible=");
		defaultInterpolatedStringHandler.AppendFormatted(nodeOrNull.Visible);
		defaultInterpolatedStringHandler.AppendLiteral("/");
		defaultInterpolatedStringHandler.AppendFormatted(nodeOrNull.IsVisibleInTree());
		defaultInterpolatedStringHandler.AppendLiteral(", ");
		defaultInterpolatedStringHandler.AppendLiteral("color=");
		defaultInterpolatedStringHandler.AppendFormatted<Color>(nodeOrNull.Modulate);
		defaultInterpolatedStringHandler.AppendLiteral("/");
		defaultInterpolatedStringHandler.AppendFormatted<Color>(nodeOrNull.SelfModulate);
		defaultInterpolatedStringHandler.AppendLiteral(", material=");
		Material material = nodeOrNull.Material;
		defaultInterpolatedStringHandler.AppendFormatted((material != null) ? ((Resource)material).ResourcePath : null);
		string text = defaultInterpolatedStringHandler.ToStringAndClear();
		Control val = (Control)(object)((nodeOrNull is Control) ? nodeOrNull : null);
		if (val != null)
		{
			text += $", pos={val.Position}, size={val.Size}, scale={val.Scale}";
		}
		TextureRect val2 = (TextureRect)(object)((nodeOrNull is TextureRect) ? nodeOrNull : null);
		if (val2 != null)
		{
			text = text + ", texture=" + TextureSnapshot(val2.Texture);
		}
		if (nodeOrNull is CanvasGroup)
		{
			Material material2 = nodeOrNull.Material;
			ShaderMaterial val3 = (ShaderMaterial)(object)((material2 is ShaderMaterial) ? material2 : null);
			if (val3 != null)
			{
				string[] obj = new string[5] { text, ", mask=", null, null, null };
				Variant shaderParameter = val3.GetShaderParameter(StringName.op_Implicit("mask_texture"));
				ref string reference = ref obj[2];
				GodotObject obj2 = ((Variant)(ref shaderParameter)).AsGodotObject();
				reference = TextureSnapshot((Texture2D?)(object)((obj2 is Texture2D) ? obj2 : null));
				obj[3] = ", mask_region=";
				shaderParameter = val3.GetShaderParameter(StringName.op_Implicit("mask_region"));
				obj[4] = ((object)(Variant)(ref shaderParameter)/*cast due to .constrained prefix*/).ToString();
				text = string.Concat(obj);
			}
		}
		return text;
	}

	private static string TextureSnapshot(Texture2D? texture)
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (texture == null)
		{
			return "null";
		}
		if (!GodotObject.IsInstanceValid((GodotObject)(object)texture))
		{
			return "freed";
		}
		string text = $"{((object)texture).GetType().Name}#{((GodotObject)texture).GetInstanceId()} {texture.GetWidth()}x{texture.GetHeight()} {((Resource)texture).ResourcePath}";
		AtlasTexture val = (AtlasTexture)(object)((texture is AtlasTexture) ? texture : null);
		if (val != null)
		{
			text += $" region={val.Region} margin={val.Margin}, atlas=";
			Texture2D atlas = val.Atlas;
			text += ((atlas != null && GodotObject.IsInstanceValid((GodotObject)(object)atlas)) ? $"{((object)atlas).GetType().Name}#{((GodotObject)atlas).GetInstanceId()} {atlas.GetWidth()}x{atlas.GetHeight()} {((Resource)atlas).ResourcePath}" : "null/freed");
		}
		return text;
	}
}

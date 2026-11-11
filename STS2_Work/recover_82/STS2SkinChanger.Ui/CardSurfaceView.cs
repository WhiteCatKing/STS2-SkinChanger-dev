using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CardSurfaceView
{
	private sealed record StretchLease(TextureRect Node, StretchModeEnum Original, StretchModeEnum Applied)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Node = ");
			builder.Append(Node);
			builder.Append(", Original = ");
			StretchModeEnum val = Original;
			builder.Append(((object)(StretchModeEnum)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Applied = ");
			val = Applied;
			builder.Append(((object)(StretchModeEnum)(ref val)/*cast due to .constrained prefix*/).ToString());
			return true;
		}

		[CompilerGenerated]
		public void Deconstruct(out TextureRect Node, out StretchModeEnum Original, out StretchModeEnum Applied)
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Expected I8, but got Unknown
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I8, but got Unknown
			Node = this.Node;
			Original = (StretchModeEnum)(long)this.Original;
			Applied = (StretchModeEnum)(long)this.Applied;
		}
	}

	private sealed record BannerLease(CardModel Card, Control Node, Color Original)
	{
		public Color? Applied { get; set; }

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Card = ");
			builder.Append(Card);
			builder.Append(", Node = ");
			builder.Append(Node);
			builder.Append(", Original = ");
			Color original = Original;
			builder.Append(((object)(Color)(ref original)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Applied = ");
			builder.Append(Applied.ToString());
			return true;
		}
	}

	private static readonly ConditionalWeakTable<NCard, StretchLease> Stretches = new ConditionalWeakTable<NCard, StretchLease>();

	private static readonly ConditionalWeakTable<NTinyCard, BannerLease> Banners = new ConditionalWeakTable<NTinyCard, BannerLease>();

	private static readonly FieldInfo? BannerField = AccessTools.Field(typeof(NTinyCard), "_cardBanner");

	internal static void ApplyPortrait(NCard card, TextureRect target)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		int? num = ((card.Model == null) ? ((int?)null) : SkinService.GetCardSurface(card.Model)?.PortraitStretchMode);
		if (Stretches.TryGetValue(card, out StretchLease value) && value.Node == target && num == (int)value.Applied)
		{
			target.StretchMode = value.Applied;
			return;
		}
		Release(card);
		if (num.HasValue)
		{
			int valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault >= 0 && valueOrDefault <= 6)
			{
				StretchModeEnum val = (StretchModeEnum)num.Value;
				Stretches.Add(card, new StretchLease(target, target.StretchMode, val));
				target.StretchMode = val;
			}
		}
	}

	internal static void Release(NCard card)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (Stretches.TryGetValue(card, out StretchLease value))
		{
			Stretches.Remove(card);
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Node) && value.Node.StretchMode == value.Applied)
			{
				value.Node.StretchMode = value.Original;
			}
		}
	}

	internal static void BindTinyCard(NTinyCard tiny, CardModel card)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Banners.Remove(tiny);
		object? obj = BannerField?.GetValue(tiny);
		Control val = (Control)((obj is Control) ? obj : null);
		if (val != null)
		{
			BannerLease bannerLease = new BannerLease(card, val, ((CanvasItem)val).Modulate);
			Banners.Add(tiny, bannerLease);
			ApplyBanner(bannerLease);
		}
	}

	internal static void ForgetTinyCard(NTinyCard tiny)
	{
		Banners.Remove(tiny);
	}

	private static void ApplyBanner(BannerLease lease)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected I4, but got Unknown
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		CardSurfaceDefinition cardSurface = SkinService.GetCardSurface(lease.Card);
		CardSurfaceColor cardSurfaceColor = cardSurface?.TinyBannerColor;
		if ((object)cardSurfaceColor != null && cardSurface.ExcludedBannerRarity != (int)lease.Card.Rarity)
		{
			lease.Applied = new Color(cardSurfaceColor.R, cardSurfaceColor.G, cardSurfaceColor.B, cardSurfaceColor.A);
			((CanvasItem)lease.Node).Modulate = lease.Applied.Value;
			return;
		}
		Color? applied = lease.Applied;
		if (applied.HasValue)
		{
			Color valueOrDefault = applied.GetValueOrDefault();
			if (((CanvasItem)lease.Node).Modulate == valueOrDefault)
			{
				((CanvasItem)lease.Node).Modulate = lease.Original;
			}
			lease.Applied = null;
		}
	}

	internal static void RefreshTinyCards()
	{
		foreach (KeyValuePair<NTinyCard, BannerLease> item in (IEnumerable<KeyValuePair<NTinyCard, BannerLease>>)Banners)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)item.Key) || !GodotObject.IsInstanceValid((GodotObject)(object)item.Value.Node))
			{
				Banners.Remove(item.Key);
			}
			else
			{
				ApplyBanner(item.Value);
			}
		}
	}
}

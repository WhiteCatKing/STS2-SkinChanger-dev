using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using CznCore.Standalone;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace CznCore.Presentation;

internal static class BundledSkinCardArtService
{
	private sealed record Applied(TextureRect Portrait, Texture2D? Previous, StretchModeEnum Stretch, Texture2D Texture)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Portrait = ");
			builder.Append(Portrait);
			builder.Append(", Previous = ");
			builder.Append(Previous);
			builder.Append(", Stretch = ");
			StretchModeEnum stretch = Stretch;
			builder.Append(((object)(StretchModeEnum)(ref stretch)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Texture = ");
			builder.Append(Texture);
			return true;
		}

		[CompilerGenerated]
		public void Deconstruct(out TextureRect Portrait, out Texture2D? Previous, out StretchModeEnum Stretch, out Texture2D Texture)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected I8, but got Unknown
			Portrait = this.Portrait;
			Previous = this.Previous;
			Stretch = (StretchModeEnum)(long)this.Stretch;
			Texture = this.Texture;
		}
	}

	[HarmonyPatch(typeof(NCard), "UpdatePortrait")]
	private static class PortraitPatch
	{
		[HarmonyPriority(0)]
		private static void Postfix(NCard __instance)
		{
			Apply(__instance);
		}
	}

	[HarmonyPatch(typeof(NCard), "_EnterTree")]
	private static class EnterPatch
	{
		[HarmonyPriority(0)]
		private static void Postfix(NCard __instance)
		{
			Apply(__instance);
		}
	}

	private static readonly object Sync = new object();

	private static readonly Dictionary<string, BundledSkinCardArt> Owners = new Dictionary<string, BundledSkinCardArt>(StringComparer.Ordinal);

	private static BundledSkinCardArt[] _snapshot = Array.Empty<BundledSkinCardArt>();

	private static readonly ConditionalWeakTable<NCard, Applied> States = new ConditionalWeakTable<NCard, Applied>();

	private static readonly FieldInfo? Portrait = AccessTools.Field(typeof(NCard), "_portrait");

	private static readonly FieldInfo? AncientPortrait = AccessTools.Field(typeof(NCard), "_ancientPortrait");

	internal static void Register(BundledSkinCardArt owner)
	{
		lock (Sync)
		{
			Owners.Add(owner.Owner, owner);
			_snapshot = Owners.Values.ToArray();
		}
		CharacterSkinModBootstrap.TrackInitializationRollback(delegate
		{
			Unregister(owner);
		});
		if (PresentationTransports.TryEnsureTypes(new _003C_003Ez__ReadOnlyArray<Type>(new Type[2]
		{
			typeof(PortraitPatch),
			typeof(EnterPatch)
		}), delegate(Type type, Exception ex)
		{
			owner.Warn("Card-art patch unavailable: " + type.Name + " / " + ex.Message);
		}).Count != 0)
		{
			Unregister(owner);
		}
	}

	internal static void Unregister(BundledSkinCardArt owner)
	{
		lock (Sync)
		{
			if (Owners.TryGetValue(owner.Owner, out BundledSkinCardArt value) && value == owner)
			{
				Owners.Remove(owner.Owner);
				_snapshot = Owners.Values.ToArray();
			}
		}
	}

	internal static BundledSkinCardArt? Resolve(IEnumerable<BundledSkinCardArt> owners, string cardId)
	{
		BundledSkinCardArt bundledSkinCardArt = null;
		foreach (BundledSkinCardArt owner in owners)
		{
			if (owner.Enabled && owner.Contains(cardId))
			{
				if (bundledSkinCardArt != null)
				{
					return null;
				}
				bundledSkinCardArt = owner;
			}
		}
		return bundledSkinCardArt;
	}

	internal static void Apply(NCard card)
	{
		SkinPatchGuard.Run("Bundled skin card art", delegate
		{
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Invalid comparison between Unknown and I4
			//IL_0156: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)card))
			{
				CardModel model = card.Model;
				if (model != null)
				{
					string cardId = ((object)model).GetType().FullName ?? ((object)model).GetType().Name;
					BundledSkinCardArt? bundledSkinCardArt = Resolve(Volatile.Read(in _snapshot), cardId);
					bool flag = (int)model.Rarity == 5;
					Texture2D val = bundledSkinCardArt?.Texture(cardId, flag);
					if (val == null)
					{
						Restore(card);
					}
					else
					{
						object? obj = (flag ? AncientPortrait : Portrait)?.GetValue(card);
						TextureRect val2 = (TextureRect)((obj is TextureRect) ? obj : null);
						if (val2 != null)
						{
							if (States.TryGetValue(card, out Applied value) && val2 != value.Portrait)
							{
								Restore(card);
							}
							if (States.TryGetValue(card, out Applied value2) && val2 == value2.Portrait && val2.Texture == value2.Texture)
							{
								val2.Texture = val;
								States.Remove(card);
								States.Add(card, value2 with
								{
									Texture = val
								});
							}
							else
							{
								States.Remove(card);
								States.Add(card, new Applied(val2, val2.Texture, val2.StretchMode, val));
								val2.Texture = val;
							}
							val2.StretchMode = (StretchModeEnum)6;
						}
					}
				}
			}
		});
	}

	private static void Restore(NCard card)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if (States.TryGetValue(card, out Applied value))
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)value.Portrait) && value.Portrait.Texture == value.Texture)
			{
				value.Portrait.Texture = value.Previous;
				value.Portrait.StretchMode = value.Stretch;
			}
			States.Remove(card);
		}
	}

	internal static void Refresh()
	{
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			Walk((Node)(object)val.Root);
		}
		static void Walk(Node node)
		{
			NCard val2 = (NCard)(object)((node is NCard) ? node : null);
			if (val2 != null)
			{
				Apply(val2);
			}
			foreach (Node child in node.GetChildren(false))
			{
				Walk(child);
			}
		}
	}
}

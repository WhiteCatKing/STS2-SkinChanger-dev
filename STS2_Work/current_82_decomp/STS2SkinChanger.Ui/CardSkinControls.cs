using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class CardSkinControls
{
	private sealed class CardLayoutState(CardModel? model, IReadOnlyList<CanvasItemState> items)
	{
		private static readonly string[] NodePaths = new string[17]
		{
			"%Portrait", "%PortraitShadow", "%PortraitBorder", "%Frame", "%TitleBanner", "%AncientPortrait", "%AncientBorderGlassOverlay", "%AncientBorder", "%AncientTextBg", "%AncientBanner",
			"%AncientHighlight", "%PortraitCanvasGroup", "%EnergyIcon", "%Highlight", "%TypePlaque", "%TypeLabel", "%DescriptionLabel"
		};

		public static CardLayoutState Capture(NCard card)
		{
			CanvasItemState[] items = (from item in NodePaths.Select((string path) => ((Node)card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit(path))).Concat(from item in Descendants((Node)(object)card).OfType<CanvasItem>()
					where ((object)((Node)item).Name).ToString().Contains("Infection", StringComparison.OrdinalIgnoreCase) || ((object)((Node)item).Name).ToString().Equals("Fire", StringComparison.OrdinalIgnoreCase)
					select item)
				where item != null
				select item).Cast<CanvasItem>().DistinctBy((CanvasItem item) => ((GodotObject)item).GetInstanceId()).Select(delegate(CanvasItem item)
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				bool visible = item.Visible;
				Material material = item.Material;
				Color modulate = item.Modulate;
				Color selfModulate = item.SelfModulate;
				int zIndex = item.ZIndex;
				CanvasItem obj = ((item is TextureRect) ? item : null);
				Texture2D texture = ((obj != null) ? ((TextureRect)obj).Texture : null);
				CanvasItem obj2 = ((item is TextureRect) ? item : null);
				ExpandModeEnum? expandMode = ((obj2 != null) ? new ExpandModeEnum?(((TextureRect)obj2).ExpandMode) : ((ExpandModeEnum?)null));
				CanvasItem obj3 = ((item is TextureRect) ? item : null);
				StretchModeEnum? stretchMode = ((obj3 != null) ? new StretchModeEnum?(((TextureRect)obj3).StretchMode) : ((StretchModeEnum?)null));
				CanvasItem obj4 = ((item is TextureRect) ? item : null);
				bool? flipH = ((obj4 != null) ? new bool?(((TextureRect)obj4).FlipH) : ((bool?)null));
				CanvasItem obj5 = ((item is TextureRect) ? item : null);
				bool? flipV = ((obj5 != null) ? new bool?(((TextureRect)obj5).FlipV) : ((bool?)null));
				CanvasItem obj6 = ((item is AnimatedSprite2D) ? item : null);
				return new CanvasItemState(item, visible, material, modulate, selfModulate, zIndex, texture, expandMode, stretchMode, flipH, flipV, (obj6 != null) ? new bool?(((AnimatedSprite2D)obj6).IsPlaying()) : ((bool?)null));
			})
				.ToArray();
			return new CardLayoutState(card.Model, items);
		}

		public bool BelongsTo(CardModel? currentModel)
		{
			return model == currentModel;
		}

		public void Restore()
		{
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			foreach (CanvasItemState item3 in items)
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)item3.Item))
				{
					continue;
				}
				item3.Item.Visible = item3.Visible;
				if (item3.Material == null || GodotObject.IsInstanceValid((GodotObject)(object)item3.Material))
				{
					item3.Item.Material = item3.Material;
				}
				item3.Item.Modulate = item3.Modulate;
				item3.Item.SelfModulate = item3.SelfModulate;
				item3.Item.ZIndex = item3.ZIndex;
				CanvasItem item = item3.Item;
				TextureRect val = (TextureRect)(object)((item is TextureRect) ? item : null);
				if (val != null)
				{
					if (item3.Texture == null || GodotObject.IsInstanceValid((GodotObject)(object)item3.Texture))
					{
						val.Texture = item3.Texture;
					}
					ExpandModeEnum? expandMode = item3.ExpandMode;
					if (expandMode.HasValue)
					{
						ExpandModeEnum valueOrDefault = expandMode.GetValueOrDefault();
						val.ExpandMode = valueOrDefault;
					}
					StretchModeEnum? stretchMode = item3.StretchMode;
					if (stretchMode.HasValue)
					{
						StretchModeEnum valueOrDefault2 = stretchMode.GetValueOrDefault();
						val.StretchMode = valueOrDefault2;
					}
					bool? flipH = item3.FlipH;
					if (flipH.HasValue)
					{
						bool valueOrDefault3 = flipH == true;
						val.FlipH = valueOrDefault3;
					}
					flipH = item3.FlipV;
					if (flipH.HasValue)
					{
						bool valueOrDefault4 = flipH == true;
						val.FlipV = valueOrDefault4;
					}
				}
				CanvasItem item2 = item3.Item;
				AnimatedSprite2D val2 = (AnimatedSprite2D)(object)((item2 is AnimatedSprite2D) ? item2 : null);
				if (val2 != null)
				{
					bool? flipH = item3.WasPlaying;
					if (flipH.HasValue)
					{
						if (flipH == true)
						{
							val2.Play((StringName)null, 1f, false);
						}
						else
						{
							val2.Stop();
						}
					}
				}
				SyncHighlightShaderMaterial(item3.Item, item3.Material);
			}
		}
	}

	private sealed record CanvasItemState(CanvasItem Item, bool Visible, Material? Material, Color Modulate, Color SelfModulate, int ZIndex, Texture2D? Texture, ExpandModeEnum? ExpandMode, StretchModeEnum? StretchMode, bool? FlipH, bool? FlipV, bool? WasPlaying)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Item = ");
			builder.Append(Item);
			builder.Append(", Visible = ");
			builder.Append(Visible.ToString());
			builder.Append(", Material = ");
			builder.Append(Material);
			builder.Append(", Modulate = ");
			Color val = Modulate;
			builder.Append(((object)(Color)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", SelfModulate = ");
			val = SelfModulate;
			builder.Append(((object)(Color)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", ZIndex = ");
			builder.Append(ZIndex.ToString());
			builder.Append(", Texture = ");
			builder.Append(Texture);
			builder.Append(", ExpandMode = ");
			builder.Append(ExpandMode.ToString());
			builder.Append(", StretchMode = ");
			builder.Append(StretchMode.ToString());
			builder.Append(", FlipH = ");
			builder.Append(FlipH.ToString());
			builder.Append(", FlipV = ");
			builder.Append(FlipV.ToString());
			builder.Append(", WasPlaying = ");
			builder.Append(WasPlaying.ToString());
			return true;
		}
	}

	private sealed record CardPresentationState(List<Node> AddedNodes);

	private sealed class CardPreviewState
	{
		public CardModel? Model;

		public CardPreviewMode Mode;

		public CardPreviewMode GetMode(CardModel? currentModel)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (Model != currentModel)
			{
				return (CardPreviewMode)1;
			}
			return Mode;
		}
	}

	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__89 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<Node> _003C_003E7__wrap3;

		Node IEnumerator<Node>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CDescendants_003Ed__89(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || (uint)(num - 1) <= 1u)
			{
				try
				{
					if (num == -4 || num == 2)
					{
						try
						{
						}
						finally
						{
							_003C_003Em__Finally2();
						}
					}
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003Cchild_003E5__3 = null;
			_003C_003E7__wrap3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					_003C_003E7__wrap3 = Descendants(_003Cchild_003E5__3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00c4;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00c4;
					}
					IL_00c4:
					if (_003C_003E7__wrap3.MoveNext())
					{
						Node current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Cchild_003E5__3 = null;
					break;
				}
				if (_003C_003E7__wrap1.MoveNext())
				{
					_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
					_003C_003E2__current = _003Cchild_003E5__3;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
		{
			_003C_003E1__state = -3;
			if (_003C_003E7__wrap3 != null)
			{
				_003C_003E7__wrap3.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Node> IEnumerable<Node>.GetEnumerator()
		{
			_003CDescendants_003Ed__89 _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__89(0);
			}
			_003CDescendants_003Ed__.root = _003C_003E3__root;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string SelectorName = "STS2CardSkinSelector";

	private const string PriorityButtonName = "CardSkinPriorityButton";

	private const string PresetButtonName = "CardSkinPresetButton";

	private const string PriorityOverlayName = "STS2CardSkinPriorityOverlay";

	private const string PriorityPanelName = "Panel";

	private const string PriorityPanelMarginName = "Margin";

	private const string PriorityContentName = "PriorityContent";

	private const string PresetOverlayName = "STS2CardSkinPresetOverlay";

	private const string PresetPanelName = "Panel";

	private const string PresetPanelMarginName = "Margin";

	private const string PresetContentName = "PresetContent";

	private const string AvailabilityFilterName = "STS2SkinnedCardsOnly";

	private const string AvailabilityFilterMeta = "sts2_skinned_cards_only";

	private const string GroupMeta = "sts2_card_skin_group";

	private const string SourceIndicatorName = "STS2CardSkinSources";

	private const string SourceSignatureMeta = "sts2_card_skin_source_signature";

	private const float CardVisualRightEdge = 150f;

	private const float CardVisualBottomEdge = 211f;

	private const string NormalTextBackgroundOverlayName = "STS2ManagedNormalTextBackgroundOverlay";

	private const string FullFrameOverlayName = "STS2ManagedFullFrameOverlay";

	private const string AncientBorderPath = "res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres";

	private const string AncientBannerPath = "res://images/atlases/ui_atlas.sprites/card/ancient_banner.tres";

	private const string AncientBannerMaterialPath = "res://materials/cards/banners/card_banner_ancient_mat.tres";

	private static readonly MethodInfo ReloadCardMethod = AccessTools.Method(typeof(NCard), "Reload", (Type[])null, (Type[])null);

	private static readonly MethodInfo UpdateLibraryFilterMethod = AccessTools.Method(typeof(NCardLibrary), "UpdateFilter", new Type[1] { typeof(bool) }, (Type[])null);

	private static readonly ConditionalWeakTable<NCard, CardLayoutState> BaselineLayouts = new ConditionalWeakTable<NCard, CardLayoutState>();

	private static readonly ConditionalWeakTable<NCard, CardPresentationState> PresentationLayouts = new ConditionalWeakTable<NCard, CardPresentationState>();

	private static readonly ConditionalWeakTable<NCard, CardPreviewState> PreviewModes = new ConditionalWeakTable<NCard, CardPreviewState>();

	private static readonly ConditionalWeakTable<NCard, CardBuiltInOverlayVisibility> BuiltInOverlays = new ConditionalWeakTable<NCard, CardBuiltInOverlayVisibility>();

	private static readonly FieldInfo? BuiltInOverlayField = AccessTools.Field(typeof(NCard), "_cardOverlay");

	private static readonly FieldInfo? HighlightShaderMaterialField = AccessTools.Field(typeof(NCardHighlight), "_shaderMaterial");

	private static Texture2D? _normalTextBackgroundCoverTexture;

	public static void Attach(NCardLibrary screen)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		SkinService.InitializeCardGroupsAfterModels();
		VBoxContainer nodeOrNull = ((Node)screen).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("Sidebar/MarginContainer/BottomVBox"));
		if (nodeOrNull == null || ((Node)nodeOrNull).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("STS2CardSkinSelector")) != null)
		{
			return;
		}
		HBoxContainer selector = new HBoxContainer
		{
			Name = StringName.op_Implicit("STS2CardSkinSelector"),
			CustomMinimumSize = new Vector2(0f, 40f),
			MouseFilter = (MouseFilterEnum)0,
			Visible = false
		};
		Control priorityOverlay = CreatePriorityOverlay();
		((Node)screen).AddChild((Node)(object)priorityOverlay, false, (InternalMode)0);
		priorityOverlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		Control presetOverlay = CreatePresetOverlay();
		((Node)screen).AddChild((Node)(object)presetOverlay, false, (InternalMode)0);
		presetOverlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		Button val = new Button
		{
			Name = StringName.op_Implicit("CardSkinPriorityButton"),
			CustomMinimumSize = new Vector2(154f, 40f),
			SizeFlagsHorizontal = (SizeFlags)3,
			Alignment = (HorizontalAlignment)1
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		((BaseButton)val).Pressed += delegate
		{
			OpenPriorityOverlay(screen, selector, priorityOverlay);
		};
		((Node)selector).AddChild((Node)(object)val, false, (InternalMode)0);
		Button val2 = new Button
		{
			Name = StringName.op_Implicit("CardSkinPresetButton"),
			CustomMinimumSize = new Vector2(94f, 40f),
			Alignment = (HorizontalAlignment)1
		};
		ContextualSkinControls.ApplyGameTheme(val2);
		((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		((BaseButton)val2).Pressed += delegate
		{
			OpenPresetOverlay(screen, selector, presetOverlay);
		};
		((Node)selector).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)nodeOrNull).AddChild((Node)(object)selector, false, (InternalMode)0);
		((Node)nodeOrNull).MoveChild((Node)(object)selector, 0);
		NLibraryStatTickbox availabilityFilter = null;
		PackedScene val3 = ResourceLoader.Load<PackedScene>("res://scenes/screens/card_library/card_library_tickbox.tscn", (string)null, (CacheMode)1);
		if (val3 != null)
		{
			availabilityFilter = val3.Instantiate<NLibraryStatTickbox>((GenEditState)0);
			((Node)availabilityFilter).Name = StringName.op_Implicit("STS2SkinnedCardsOnly");
			((Node)nodeOrNull).AddChild((Node)(object)availabilityFilter, false, (InternalMode)0);
			((Node)nodeOrNull).MoveChild((Node)(object)availabilityFilter, 1);
			availabilityFilter.SetLabel(ModLocalization.Get(ModText.SkinnedCardsOnly));
			((NTickbox)availabilityFilter).IsTicked = false;
			((GodotObject)availabilityFilter).Connect(SignalName.Toggled, Callable.From<NTickbox>((Action<NTickbox>)delegate(NTickbox tickbox)
			{
				SetAvailabilityFilter(screen, "sts2_skinned_cards_only", tickbox.IsTicked);
			}), 0u);
		}
		ShowFirstAvailableGroup(selector);
		ModLocalization.Bind((Node)(object)screen, delegate
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			NLibraryStatTickbox obj = availabilityFilter;
			if (obj != null)
			{
				obj.SetLabel(ModLocalization.Get(ModText.SkinnedCardsOnly));
			}
			Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_card_skin_group"), Variant.op_Implicit(string.Empty));
			string text = ((Variant)(ref meta)).AsString();
			if (string.IsNullOrWhiteSpace(text))
			{
				ShowFirstAvailableGroup(selector);
			}
			else
			{
				Populate(selector, text);
			}
			if (((CanvasItem)priorityOverlay).Visible)
			{
				BuildPriorityOverlay(screen, selector, priorityOverlay);
			}
			if (((CanvasItem)presetOverlay).Visible)
			{
				BuildPresetOverlay(screen, selector, presetOverlay);
			}
			RefreshSourceIndicators(screen);
		});
	}

	public static void ShowForFilter(NCardLibrary screen, NCardPoolFilter filter)
	{
		if (filter.IsSelected)
		{
			HBoxContainer nodeOrNull = ((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("Sidebar/MarginContainer/BottomVBox/STS2CardSkinSelector"));
			if (nodeOrNull != null)
			{
				Populate(nodeOrNull, GetGroupId(filter));
				RefreshSourceIndicators(screen);
			}
		}
	}

	public static void SyncToSelectedFilter(NCardLibrary screen)
	{
		HBoxContainer nodeOrNull = ((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("Sidebar/MarginContainer/BottomVBox/STS2CardSkinSelector"));
		if (nodeOrNull != null)
		{
			NCardPoolFilter val = Descendants((Node)(object)screen).OfType<NCardPoolFilter>().FirstOrDefault((Func<NCardPoolFilter, bool>)((NCardPoolFilter filter) => filter.IsSelected && FindGroup(GetGroupId(filter)) != null));
			if (val != null)
			{
				Populate(nodeOrNull, GetGroupId(val));
			}
			else
			{
				ShowFirstAvailableGroup(nodeOrNull);
			}
			RefreshSourceIndicators(screen);
		}
	}

	public static void ReplacePortrait(CardModel card, ref Texture2D result)
	{
		SkinService.ReplaceCardPortrait(card, ref result);
	}

	public static void UpdateLibrarySourceIndicators(NCard card)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Invalid comparison between Unknown and I4
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0251: Expected O, but got Unknown
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0308: Expected O, but got Unknown
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		if (card.Model == null || HasAncestor<NInspectCardScreen>((Node)(object)card) || !HasAncestor<NCardLibrary>((Node)(object)card))
		{
			RemoveSourceIndicators(card);
			return;
		}
		NCardLibrary val = null;
		for (Node parent = ((Node)card).GetParent(); parent != null; parent = parent.GetParent())
		{
			NCardLibrary val2 = (NCardLibrary)(object)((parent is NCardLibrary) ? parent : null);
			if (val2 != null)
			{
				val = val2;
				break;
			}
		}
		HBoxContainer obj = ((val != null) ? ((Node)val).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("Sidebar/MarginContainer/BottomVBox/STS2CardSkinSelector")) : null);
		object obj2;
		Variant meta;
		if (obj == null)
		{
			obj2 = null;
		}
		else
		{
			meta = ((GodotObject)obj).GetMeta(StringName.op_Implicit("sts2_card_skin_group"), Variant.op_Implicit(string.Empty));
			obj2 = ((Variant)(ref meta)).AsString();
		}
		string text = (string)obj2;
		IReadOnlyList<CardSkinSourceState> cardSkinSources = SkinService.GetCardSkinSources(card.Model, text);
		if (cardSkinSources.Count == 0)
		{
			RemoveSourceIndicators(card);
			return;
		}
		string text2 = (((int)card.Model.Rarity == 5 && !card.Model.Pool.IsColorless && SkinService.GetCardOverrideSelection(card.Model).Equals("__inherit__", StringComparison.OrdinalIgnoreCase)) ? InheritedSelectionText(card.Model) : null);
		string text3 = ModLocalization.CurrentLanguage + "\n" + text + "\n" + text2 + "\n" + string.Join("\n", cardSkinSources.Select((CardSkinSourceState source) => $"{source.OptionId}:{source.Enabled}:{source.ColorIndex}:{source.IsCurrent}"));
		Control val3 = (Control)(((object)((Node)card).GetNodeOrNull<Control>(NodePath.op_Implicit("%CardContainer"))) ?? ((object)card));
		Control nodeOrNull = ((Node)val3).GetNodeOrNull<Control>(NodePath.op_Implicit("STS2CardSkinSources"));
		if (nodeOrNull != null)
		{
			meta = ((GodotObject)nodeOrNull).GetMeta(StringName.op_Implicit("sts2_card_skin_source_signature"), Variant.op_Implicit(string.Empty));
			if (((Variant)(ref meta)).AsString() == text3)
			{
				return;
			}
		}
		RemoveSourceIndicators(card);
		string tooltipText = BuildSourceTooltip(cardSkinSources, text2);
		float num = (float)cardSkinSources.Count * 12f;
		float num2 = 175f;
		VBoxContainer val4 = new VBoxContainer
		{
			Name = StringName.op_Implicit("STS2CardSkinSources"),
			MouseFilter = (MouseFilterEnum)0,
			TooltipText = tooltipText,
			AnchorLeft = 0f,
			AnchorTop = 0f,
			AnchorRight = 0f,
			AnchorBottom = 0f,
			OffsetLeft = 146f,
			OffsetTop = num2 - num,
			OffsetRight = 186f,
			OffsetBottom = num2
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
		((GodotObject)val4).SetMeta(StringName.op_Implicit("sts2_card_skin_source_signature"), Variant.op_Implicit(text3));
		foreach (CardSkinSourceState item in cardSkinSources.Reverse())
		{
			Color color = SourceColor(item.ColorIndex);
			color.A = ((item.Enabled || item.IsCurrent) ? 1f : 0.45f);
			Panel val5 = new Panel
			{
				CustomMinimumSize = new Vector2((float)(item.IsCurrent ? 22 : 15), 12f),
				SizeFlagsHorizontal = (SizeFlags)0,
				MouseFilter = (MouseFilterEnum)0,
				TooltipText = tooltipText
			};
			((Control)val5).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateSourceTabStyle(color));
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		}
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		CanvasItem nodeOrNull2 = ((Node)val3).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("Shadow"));
		if (nodeOrNull2 != null)
		{
			((Node)val3).MoveChild((Node)(object)val4, ((Node)nodeOrNull2).GetIndex(false) + 1);
		}
	}

	private static string BuildSourceTooltip(IReadOnlyList<CardSkinSourceState> sources, string? inheritedText)
	{
		List<string> list = new List<string>();
		CardSkinSourceState cardSkinSourceState = sources.FirstOrDefault((CardSkinSourceState source) => source.IsCurrent);
		if (cardSkinSourceState != null)
		{
			list.Add(string.Format(ModLocalization.Get(ModText.CurrentCardSource), ModLocalization.DisplayOptionName(cardSkinSourceState.Name)));
			if (inheritedText != null)
			{
				list.Add(inheritedText);
			}
		}
		list.Add(string.Format(ModLocalization.Get(ModText.AvailableCardSources), string.Join(" · ", sources.Select((CardSkinSourceState source) => ModLocalization.DisplayOptionName(source.Name)))));
		return string.Join("\n", list);
	}

	internal static string InheritedSelectionText(CardModel card)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		string text = ModLocalization.Get(ModText.FollowCategory);
		if ((int)card.Rarity != 5 || card.Pool.IsColorless)
		{
			return text;
		}
		string groupId = SkinService.GetCardInheritedGroupId(card);
		if (groupId == null)
		{
			return text;
		}
		if (groupId.Equals("ancients", StringComparison.OrdinalIgnoreCase))
		{
			return text + " · " + ModLocalization.Get(ModText.OtherCategoryAncients);
		}
		try
		{
			CharacterModel val = ModelDb.AllCharacters.FirstOrDefault((Func<CharacterModel, bool>)((CharacterModel model) => model.CardPool.Title.Equals(groupId, StringComparison.OrdinalIgnoreCase)));
			if (val != null)
			{
				return text + " · " + val.Title.GetFormattedText();
			}
		}
		catch
		{
		}
		return text + " · " + (FindGroup(groupId)?.DisplayName ?? groupId);
	}

	private static Color SourceColor(int index)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return Color.FromHsv((0.12f + (float)index * 0.618034f) % 1f, 0.72f, 0.96f, 1f);
	}

	private static StyleBoxFlat CreateSourceTabStyle(Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = color,
			CornerRadiusTopLeft = 6,
			CornerRadiusTopRight = 6,
			CornerRadiusBottomRight = 6,
			CornerRadiusBottomLeft = 6
		};
	}

	private static void RemoveSourceIndicators(NCard card)
	{
		Control nodeOrNull = ((Node)(((object)((Node)card).GetNodeOrNull<Control>(NodePath.op_Implicit("%CardContainer"))) ?? ((object)card))).GetNodeOrNull<Control>(NodePath.op_Implicit("STS2CardSkinSources"));
		if (nodeOrNull != null)
		{
			Node parent = ((Node)nodeOrNull).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)nodeOrNull);
			}
			((Node)nodeOrNull).QueueFree();
		}
	}

	private static bool HasAncestor<T>(Node node) where T : Node
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent is T)
			{
				return true;
			}
		}
		return false;
	}

	public static void CaptureBaselineLayout(NCard card)
	{
		BaselineLayouts.Remove(card);
		BaselineLayouts.Add(card, CardLayoutState.Capture(card));
	}

	public static void RestoreBaselineLayout(NCard card)
	{
		CardSurfaceView.Release(card);
		StatefulCardArtView.BeforeRefresh(card, reload: true);
		if (PresentationLayouts.TryGetValue(card, out CardPresentationState value))
		{
			foreach (Node addedNode in value.AddedNodes)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)addedNode) && !((GodotObject)addedNode).IsQueuedForDeletion())
				{
					Node parent = addedNode.GetParent();
					if (parent != null)
					{
						parent.RemoveChild(addedNode);
					}
					addedNode.QueueFree();
				}
			}
			PresentationLayouts.Remove(card);
		}
		if (BaselineLayouts.TryGetValue(card, out CardLayoutState value2))
		{
			if (value2.BelongsTo(card.Model))
			{
				value2.Restore();
			}
			else
			{
				BaselineLayouts.Remove(card);
			}
		}
	}

	internal static void ReleasePresentationForReuse(NCard card)
	{
		CardSurfaceView.Release(card);
		StatefulCardArtView.Release(card);
		if (PresentationLayouts.TryGetValue(card, out CardPresentationState _))
		{
			RestoreBaselineLayout(card);
		}
		BaselineLayouts.Remove(card);
		PreviewModes.Remove(card);
	}

	private static void TrackAddedPresentationNode(NCard card, Node node)
	{
		PresentationLayouts.GetValue(card, (NCard _) => new CardPresentationState(new List<Node>())).AddedNodes.Add(node);
		SendPresentationBehindCardVfx(card, node);
	}

	private static void SendPresentationBehindCardVfx(NCard card, Node node)
	{
		if (node.GetParent() == null)
		{
			return;
		}
		Node nodeOrNull = ((Node)card).GetNodeOrNull<Node>(NodePath.op_Implicit("%CardVfxContainer"));
		if (nodeOrNull == null)
		{
			nodeOrNull = ((Node)card).GetNodeOrNull<Node>(NodePath.op_Implicit("CardVfxContainer"));
		}
		if (nodeOrNull != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull) && node != nodeOrNull)
		{
			Node val = nodeOrNull;
			while (val.GetParent() != null && val.GetParent() != node.GetParent())
			{
				val = val.GetParent();
			}
			if (val.GetParent() != null && val.GetParent() == node.GetParent() && node.GetIndex(false) > val.GetIndex(false))
			{
				node.GetParent().MoveChild(node, val.GetIndex(false));
			}
		}
	}

	private static void ReleaseSelectedPresentation(NCard card)
	{
		if (PresentationLayouts.TryGetValue(card, out CardPresentationState _))
		{
			RestoreBaselineLayout(card);
		}
	}

	public static void ReapplyQueuedCardPortraits(NCardPlayQueue queue)
	{
		foreach (NCard item in ((IEnumerable)((Node)queue).GetChildren(false)).OfType<NCard>())
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)item) && item.Model != null)
			{
				ApplySelectedPortraitToNode(item, ExternalCardVisualBridge.GetOwnership(item));
				ExternalCardVisualBridge.SynchronizeProvider(item);
			}
		}
	}

	internal static bool HasCurrentCardLayout(NCard card)
	{
		if (card.Model != null && BaselineLayouts.TryGetValue(card, out CardLayoutState value))
		{
			return value.BelongsTo(card.Model);
		}
		return false;
	}

	internal static void ReapplyCardAfterTreeEntry(NCard card)
	{
		if (HasCurrentCardLayout(card))
		{
			ExternalCardVisualOwnership ownership = ExternalCardVisualBridge.GetOwnership(card);
			ApplySelectedPresentation(card, ownership);
			ApplySelectedPortraitToNode(card, ownership);
			StatefulCardArtView.Apply(card, ownership, (CardPreviewMode)1);
			ExternalCardVisualBridge.SynchronizeProvider(card);
		}
	}

	public static void ApplySelectedPresentation(NCard card, ExternalCardVisualOwnership externalOwnership)
	{
		if (card.Model == null)
		{
			return;
		}
		if (externalOwnership.Frame)
		{
			ReleaseSelectedPresentation(card);
			ApplyBuiltInOverlay(card, null);
			return;
		}
		CardPresentationDefinition cardPresentation = SkinService.GetCardPresentation(card.Model);
		ApplyBuiltInOverlay(card, cardPresentation);
		if (cardPresentation == null)
		{
			ReleaseSelectedPresentation(card);
			return;
		}
		PresentationLayouts.GetValue(card, (NCard _) => new CardPresentationState(new List<Node>()));
		ApplyManagedCardPresentation(card, cardPresentation, externalOwnership.Text);
	}

	private static void ApplyManagedCardPresentation(NCard card, CardPresentationDefinition presentation, bool preserveExternalText)
	{
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Invalid comparison between Unknown and I4
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a6: Invalid comparison between Unknown and I4
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Invalid comparison between Unknown and I4
		TextureRect nodeOrNull = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Portrait"));
		TextureRect nodeOrNull2 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%PortraitBorder"));
		TextureRect nodeOrNull3 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Frame"));
		TextureRect nodeOrNull4 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%TitleBanner"));
		TextureRect nodeOrNull5 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientPortrait"));
		TextureRect nodeOrNull6 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientBorderGlassOverlay"));
		TextureRect nodeOrNull7 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientBorder"));
		TextureRect nodeOrNull8 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientTextBg"));
		TextureRect nodeOrNull9 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientBanner"));
		TextureRect target = (TextureRect)(((object)((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientHighlight"))) ?? ((object)/*isinst with value type is only supported in some contexts*/));
		CanvasGroup nodeOrNull10 = ((Node)card).GetNodeOrNull<CanvasGroup>(NodePath.op_Implicit("%PortraitCanvasGroup"));
		TextureRect nodeOrNull11 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%EnergyIcon"));
		TextureRect nodeOrNull12 = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%Highlight"));
		Node? obj = FindNodeByName((Node)(object)card, "Fire");
		AnimatedSprite2D val = (AnimatedSprite2D)(object)((obj is AnimatedSprite2D) ? obj : null);
		if (presentation.UseFullFrameArt)
		{
			ApplyManagedFullFrameArt(card, presentation, nodeOrNull, nodeOrNull2, nodeOrNull3, nodeOrNull4, nodeOrNull5, nodeOrNull6, nodeOrNull7, nodeOrNull8, nodeOrNull9);
			return;
		}
		CardPresentationLayout cardPresentationLayout = CardPresentationLayoutPolicy.Resolve((int)card.Model.Rarity == 5, presentation.UseAncientLayout, presentation.UseExpandedPortraitLayout);
		bool flag = cardPresentationLayout == CardPresentationLayout.Ancient;
		bool flag2 = cardPresentationLayout == CardPresentationLayout.ExpandedPortrait;
		if (flag)
		{
			SetVisible((CanvasItem?)(object)nodeOrNull, false);
			SetVisible((CanvasItem?)(object)nodeOrNull2, false);
			SetVisible((CanvasItem?)(object)nodeOrNull3, false);
			SetVisible((CanvasItem?)(object)nodeOrNull4, false);
			SetVisible((CanvasItem?)(object)nodeOrNull5, true);
			SetVisible((CanvasItem?)(object)nodeOrNull6, true);
			SetVisible((CanvasItem?)(object)nodeOrNull7, true);
			SetVisible((CanvasItem?)(object)nodeOrNull8, true);
			SetVisible((CanvasItem?)(object)nodeOrNull9, true);
			if (val != null)
			{
				((CanvasItem)val).Visible = true;
				val.Play((StringName)null, 1f, false);
			}
			if (nodeOrNull7 != null && nodeOrNull7.Texture == null)
			{
				TextureRect val2 = nodeOrNull7;
				val2.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, "res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres");
			}
			if (nodeOrNull9 != null && nodeOrNull9.Texture == null)
			{
				TextureRect val3 = nodeOrNull9;
				val3.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, "res://images/atlases/ui_atlas.sprites/card/ancient_banner.tres");
			}
			if (nodeOrNull9 != null && ((CanvasItem)nodeOrNull9).Material == null)
			{
				TextureRect val4 = nodeOrNull9;
				((CanvasItem)val4).Material = SkinService.LoadCardPresentationResource<Material>(card.Model, "res://materials/cards/banners/card_banner_ancient_mat.tres");
			}
			if (nodeOrNull8 != null && nodeOrNull8.Texture == null)
			{
				TextureRect val5 = nodeOrNull8;
				val5.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, DefaultAncientTextBackgroundPath(card.Model));
			}
			if (nodeOrNull10 != null)
			{
				string resourcePath = (((int)card.Visibility == 1) ? "res://scenes/cards/card_canvas_group_mask_material.tres" : "res://scenes/cards/card_canvas_group_mask_blur_material.tres");
				((CanvasItem)nodeOrNull10).Material = SkinService.LoadCardPresentationResource<Material>(card.Model, resourcePath);
			}
		}
		else if (flag2)
		{
			SetVisible((CanvasItem?)(object)nodeOrNull, false);
			SetVisible((CanvasItem?)(object)nodeOrNull2, false);
			SetVisible((CanvasItem?)(object)nodeOrNull3, false);
			SetVisible((CanvasItem?)(object)nodeOrNull5, true);
			if (nodeOrNull5 != null)
			{
				nodeOrNull5.StretchMode = (StretchModeEnum)6;
			}
			SetVisible((CanvasItem?)(object)nodeOrNull6, false);
			SetVisible((CanvasItem?)(object)nodeOrNull9, false);
			SetVisible((CanvasItem?)(object)nodeOrNull8, false);
			SetVisible((CanvasItem?)(object)target, false);
			if (val != null)
			{
				((CanvasItem)val).Visible = false;
				val.Stop();
			}
			if (nodeOrNull7 != null && nodeOrNull7.Texture == null)
			{
				TextureRect val6 = nodeOrNull7;
				val6.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, "res://images/atlases/compressed.sprites/card_template/ancient_card_border.tres");
			}
			SetVisible((CanvasItem?)(object)nodeOrNull7, presentation.FrameVisible ?? true);
			if (nodeOrNull10 != null)
			{
				string resourcePath2 = (((int)card.Visibility == 1) ? "res://scenes/cards/card_canvas_group_mask_material.tres" : "res://scenes/cards/card_canvas_group_mask_blur_material.tres");
				((CanvasItem)nodeOrNull10).Material = SkinService.LoadCardPresentationResource<Material>(card.Model, resourcePath2);
			}
		}
		else
		{
			SetVisible((CanvasItem?)(object)nodeOrNull5, false);
			SetVisible((CanvasItem?)(object)nodeOrNull6, false);
			SetVisible((CanvasItem?)(object)nodeOrNull7, false);
			SetVisible((CanvasItem?)(object)nodeOrNull8, false);
			SetVisible((CanvasItem?)(object)nodeOrNull9, false);
			SetVisible((CanvasItem?)(object)target, false);
			if (val != null)
			{
				((CanvasItem)val).Visible = false;
				val.Stop();
			}
		}
		ApplyTexture(card, nodeOrNull3, presentation.Frame);
		ApplyTexture(card, nodeOrNull7, presentation.Frame);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull3, presentation.FrameMaterial);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull7, presentation.FrameMaterial);
		ApplyTexture(card, nodeOrNull4, presentation.BannerTexture);
		ApplyTexture(card, nodeOrNull9, presentation.BannerTexture);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull4, presentation.BannerMaterial);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull9, presentation.BannerMaterial);
		ApplyTexture(card, nodeOrNull2, presentation.PortraitBorder);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull2, presentation.PortraitBorderMaterial);
		ApplyTexture(card, nodeOrNull8, presentation.AncientTextBackground);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull8, presentation.TextBackgroundMaterial);
		ApplyTexture(card, nodeOrNull11, presentation.EnergyIcon);
		ApplyTexture(card, nodeOrNull12, presentation.Highlight);
		ApplyTexture(card, target, presentation.Highlight);
		ApplyMaterial(card, (CanvasItem?)(object)nodeOrNull12, presentation.HighlightMaterial);
		ApplyMaterial(card, (CanvasItem?)(object)target, presentation.HighlightMaterial);
		bool? frameVisible = presentation.FrameVisible;
		if (frameVisible.HasValue)
		{
			bool valueOrDefault = frameVisible == true;
			SetVisible((CanvasItem?)(object)nodeOrNull3, valueOrDefault && cardPresentationLayout == CardPresentationLayout.Normal);
			SetVisible((CanvasItem?)(object)nodeOrNull7, valueOrDefault && cardPresentationLayout != CardPresentationLayout.Normal);
		}
		frameVisible = presentation.BannerVisible;
		if (frameVisible.HasValue)
		{
			bool valueOrDefault2 = frameVisible == true;
			SetVisible((CanvasItem?)(object)nodeOrNull4, valueOrDefault2 && !flag);
			SetVisible((CanvasItem?)(object)nodeOrNull9, valueOrDefault2 && flag);
		}
		if (flag)
		{
			SetVisible((CanvasItem?)(object)nodeOrNull8, presentation.TextBackgroundVisible);
		}
		else if (flag2)
		{
			bool valueOrDefault3 = presentation.TextBackgroundVisible == true;
			SetVisible((CanvasItem?)(object)nodeOrNull8, valueOrDefault3);
			if (valueOrDefault3 && nodeOrNull8 != null && nodeOrNull8.Texture == null)
			{
				TextureRect val7 = nodeOrNull8;
				val7.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, presentation.AncientTextBackground ?? DefaultAncientTextBackgroundPath(card.Model));
			}
			RemoveNormalTextBackground(card);
		}
		else
		{
			ApplyNormalTextBackground(card, presentation);
		}
		frameVisible = presentation.PortraitBorderVisible;
		if (frameVisible.HasValue)
		{
			bool valueOrDefault4 = frameVisible == true;
			SetVisible((CanvasItem?)(object)nodeOrNull2, valueOrDefault4 && cardPresentationLayout == CardPresentationLayout.Normal);
		}
		frameVisible = presentation.PortraitVisible;
		if (frameVisible.HasValue)
		{
			bool valueOrDefault5 = frameVisible == true;
			SetVisible((CanvasItem?)(object)nodeOrNull, valueOrDefault5 && cardPresentationLayout == CardPresentationLayout.Normal);
			SetVisible((CanvasItem?)(object)nodeOrNull5, valueOrDefault5 && cardPresentationLayout != CardPresentationLayout.Normal);
		}
		SetVisible((CanvasItem?)(object)nodeOrNull11, presentation.EnergyIconVisible);
		frameVisible = presentation.HighlightVisible;
		if (frameVisible.HasValue)
		{
			bool valueOrDefault6 = frameVisible == true;
			SetVisible((CanvasItem?)(object)nodeOrNull12, valueOrDefault6 && cardPresentationLayout == CardPresentationLayout.Normal);
			SetVisible((CanvasItem?)(object)target, valueOrDefault6 && flag);
		}
		if (!preserveExternalText)
		{
			ApplyTypePlaqueVisibility(card, presentation);
			SetVisible(((Node)card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("%DescriptionLabel")), presentation.DescriptionVisible);
		}
		Node? obj2 = FindNodeByName((Node)(object)card, "Infection");
		SetVisible((CanvasItem?)(object)((obj2 is CanvasItem) ? obj2 : null), presentation.InfectionOverlayVisible);
	}

	public static void ApplyBuiltInOverlay(NCard card, CardPresentationDefinition? presentation)
	{
		CardModel model = card.Model;
		if (model == null || !model.HasBuiltInOverlay)
		{
			return;
		}
		AfflictionModel affliction = model.Affliction;
		if (affliction == null || !affliction.HasOverlay)
		{
			object? obj = BuiltInOverlayField?.GetValue(card);
			CanvasItem val = (CanvasItem)((obj is CanvasItem) ? obj : null);
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val) && (((object)presentation != null && presentation.BuiltInOverlayVisible.HasValue) || BuiltInOverlays.TryGetValue(card, out CardBuiltInOverlayVisibility _)))
			{
				val.Visible = BuiltInOverlays.GetOrCreateValue(card).Resolve(model, val, val.Visible, presentation?.BuiltInOverlayVisible);
			}
		}
	}

	private static void ApplyManagedFullFrameArt(NCard card, CardPresentationDefinition presentation, TextureRect? portrait, TextureRect? portraitBorder, TextureRect? frame, TextureRect? banner, TextureRect? ancientPortrait, TextureRect? ancientGlass, TextureRect? ancientBorder, TextureRect? ancientTextBg, TextureRect? ancientBanner)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Invalid comparison between Unknown and I4
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		ApplyTexture(card, frame, presentation.Frame);
		if (frame != null)
		{
			((CanvasItem)frame).Material = null;
		}
		SetVisible((CanvasItem?)(object)frame, presentation.FrameVisible ?? true);
		SetVisible((CanvasItem?)(object)portrait, presentation.PortraitVisible == true);
		SetVisible((CanvasItem?)(object)portraitBorder, presentation.PortraitBorderVisible == true);
		SetVisible((CanvasItem?)(object)ancientPortrait, false);
		SetVisible((CanvasItem?)(object)ancientGlass, false);
		SetVisible((CanvasItem?)(object)ancientBanner, false);
		SetVisible((CanvasItem?)(object)banner, presentation.BannerVisible ?? true);
		bool flag = (int)card.Model.Rarity == 5;
		SetVisible((CanvasItem?)(object)ancientBorder, flag);
		SetVisible((CanvasItem?)(object)ancientTextBg, presentation.TextBackgroundVisible ?? true);
		if (ancientTextBg != null)
		{
			ancientTextBg.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, presentation.AncientTextBackground ?? DefaultAncientTextBackgroundPath(card.Model));
		}
		Node val = ((ancientBorder != null) ? ((Node)ancientBorder).GetParent() : null);
		TextureRect val2 = null;
		if (!flag && val != null && !string.IsNullOrWhiteSpace(presentation.FrameOverlay))
		{
			val2 = val.GetNodeOrNull<TextureRect>(NodePath.op_Implicit("STS2ManagedFullFrameOverlay"));
			if (val2 == null)
			{
				val2 = new TextureRect
				{
					Name = StringName.op_Implicit("STS2ManagedFullFrameOverlay"),
					MouseFilter = (MouseFilterEnum)2,
					AnchorLeft = 0.5f,
					AnchorTop = 0.5f,
					AnchorRight = 0.5f,
					AnchorBottom = 0.5f,
					GrowHorizontal = (GrowDirection)2,
					GrowVertical = (GrowDirection)2,
					PivotOffset = new Vector2(150f, 211f),
					ExpandMode = (ExpandModeEnum)1,
					StretchMode = (StretchModeEnum)6
				};
				val.AddChild((Node)(object)val2, false, (InternalMode)0);
				TrackAddedPresentationNode(card, (Node)(object)val2);
			}
			val2.Texture = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, presentation.FrameOverlay);
			((CanvasItem)val2).ZIndex = ((CanvasItem)ancientBorder).ZIndex;
			((Control)val2).OffsetTop = presentation.FrameOverlayOffsetTop ?? (-211f);
			((Control)val2).OffsetBottom = presentation.FrameOverlayOffsetBottom ?? 211f;
			((Control)val2).OffsetLeft = presentation.FrameOverlayOffsetLeft ?? (-150f);
			((Control)val2).OffsetRight = presentation.FrameOverlayOffsetRight ?? 150f;
			((Control)val2).Scale = new Vector2(presentation.FrameOverlayScaleX ?? 1f, presentation.FrameOverlayScaleY ?? 1f);
		}
		if (val == null)
		{
			return;
		}
		Node[] array = (Node[])(object)new Node[9]
		{
			(Node)ancientBorder,
			(Node)ancientTextBg,
			(Node)val2,
			((Node)card).GetNodeOrNull<Node>(NodePath.op_Implicit("%TypePlaque")),
			((Node)card).GetNodeOrNull<Node>(NodePath.op_Implicit("%DescriptionLabel")),
			(Node)banner,
			FindNodeByName((Node)(object)card, "TitleLabel"),
			((Node)card).GetNodeOrNull<Node>(NodePath.op_Implicit("%EnergyIcon")),
			FindNodeByName((Node)(object)card, "StarIcon")
		};
		foreach (Node val3 in array)
		{
			if (((val3 != null) ? val3.GetParent() : null) == val)
			{
				val.MoveChild(val3, val.GetChildCount(false) - 1);
			}
		}
	}

	private static string DefaultAncientTextBackgroundPath(CardModel card)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		CardType type = card.Type;
		string text = (((int)type == 1) ? "attack" : (((int)type != 3) ? "skill" : "power"));
		string text2 = text;
		return "res://images/atlases/compressed.sprites/card_template/ancient_card_text_bg_" + text2 + ".tres";
	}

	private static void ApplyTexture(NCard card, TextureRect? target, string? resourcePath)
	{
		if (target != null && !string.IsNullOrWhiteSpace(resourcePath))
		{
			Texture2D val = SkinService.LoadCardPresentationResource<Texture2D>(card.Model, resourcePath);
			if (val != null)
			{
				target.Texture = val;
			}
		}
	}

	private static void ApplyMaterial(NCard card, CanvasItem? target, string? resourcePath)
	{
		if (target != null && !string.IsNullOrWhiteSpace(resourcePath))
		{
			Material val = SkinService.LoadCardPresentationResource<Material>(card.Model, resourcePath);
			if (val != null)
			{
				target.Material = val;
				SyncHighlightShaderMaterial(target, val);
			}
		}
	}

	private static void ApplyNormalTextBackground(NCard card, CardPresentationDefinition presentation)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		if (presentation.AncientTextBackground == null && presentation.TextBackgroundMaterial == null && !presentation.TextBackgroundVisible.HasValue)
		{
			return;
		}
		Control body = card.Body;
		Control nodeOrNull = ((Node)card).GetNodeOrNull<Control>(NodePath.op_Implicit("%DescriptionLabel"));
		if (body != null && nodeOrNull != null)
		{
			TextureRect val = ((Node)body).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("STS2ManagedNormalTextBackgroundOverlay"));
			if (val == null)
			{
				val = new TextureRect
				{
					Name = StringName.op_Implicit("STS2ManagedNormalTextBackgroundOverlay"),
					MouseFilter = (MouseFilterEnum)2,
					ExpandMode = (ExpandModeEnum)1,
					StretchMode = (StretchModeEnum)0,
					TextureFilter = (TextureFilterEnum)2,
					Visible = false
				};
				((Node)body).AddChild((Node)(object)val, false, (InternalMode)0);
				TrackAddedPresentationNode(card, (Node)(object)val);
				((Node)body).MoveChild((Node)(object)val, Math.Max(0, ((Node)nodeOrNull).GetIndex(false)));
			}
			Control nodeOrNull2 = ((Node)card).GetNodeOrNull<Control>(NodePath.op_Implicit("%TypePlaque"));
			float num = nodeOrNull.Position.X - 14f;
			float num2 = nodeOrNull.Position.Y - 18f;
			float num3 = nodeOrNull.Position.X + nodeOrNull.Size.X + 14f;
			float num4 = nodeOrNull.Position.Y + nodeOrNull.Size.Y + 12f;
			if (nodeOrNull2 != null)
			{
				num = MathF.Min(num, nodeOrNull2.Position.X - 18f);
				num2 = MathF.Min(num2, nodeOrNull2.Position.Y - 18f);
				num3 = MathF.Max(num3, nodeOrNull2.Position.X + nodeOrNull2.Size.X + 18f);
			}
			((Control)val).Position = new Vector2(num, num2);
			((Control)val).Size = new Vector2(MathF.Max(1f, num3 - num), MathF.Max(1f, num4 - num2));
			bool flag = presentation.TextBackgroundVisible ?? true;
			val.Texture = (Texture2D)((flag && presentation.AncientTextBackground != null) ? ((object)SkinService.LoadCardPresentationResource<Texture2D>(card.Model, presentation.AncientTextBackground)) : ((object)(flag ? null : GetNormalTextBackgroundCoverTexture())));
			((CanvasItem)val).Material = ((flag && presentation.TextBackgroundMaterial != null) ? SkinService.LoadCardPresentationResource<Material>(card.Model, presentation.TextBackgroundMaterial) : null);
			((CanvasItem)val).Visible = val.Texture != null;
		}
	}

	private static void RemoveNormalTextBackground(NCard card)
	{
		Control body = card.Body;
		TextureRect val = ((body != null) ? ((Node)body).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("STS2ManagedNormalTextBackgroundOverlay")) : null);
		if (val != null)
		{
			Node parent = ((Node)val).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)val);
			}
			if (PresentationLayouts.TryGetValue(card, out CardPresentationState value))
			{
				value.AddedNodes.Remove((Node)(object)val);
			}
			((Node)val).QueueFree();
		}
	}

	private static Texture2D GetNormalTextBackgroundCoverTexture()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (_normalTextBackgroundCoverTexture != null && GodotObject.IsInstanceValid((GodotObject)(object)_normalTextBackgroundCoverTexture))
		{
			return _normalTextBackgroundCoverTexture;
		}
		Image val = Image.CreateEmpty(8, 8, false, (Format)5);
		try
		{
			val.Fill(new Color(0.11f, 0.1f, 0.08f, 0.92f));
			_normalTextBackgroundCoverTexture = (Texture2D?)(object)ImageTexture.CreateFromImage(val);
			return _normalTextBackgroundCoverTexture;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static void ApplyTypePlaqueVisibility(NCard card, CardPresentationDefinition presentation)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (presentation.TypePlaqueVisible.HasValue || presentation.TypeLabelVisible.HasValue)
		{
			Control nodeOrNull = ((Node)card).GetNodeOrNull<Control>(NodePath.op_Implicit("%TypePlaque"));
			CanvasItem nodeOrNull2 = ((Node)card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("%TypeLabel"));
			bool flag = presentation.TypePlaqueVisible ?? true;
			bool flag2 = presentation.TypeLabelVisible ?? true;
			if (nodeOrNull != null)
			{
				((CanvasItem)nodeOrNull).Visible = flag || flag2;
				((CanvasItem)nodeOrNull).SelfModulate = (Color)(flag ? Colors.White : new Color(1f, 1f, 1f, 0f));
			}
			SetVisible(nodeOrNull2, flag2);
		}
	}

	private static Node? FindNodeByName(Node root, string name)
	{
		foreach (Node child in root.GetChildren(false))
		{
			if (((object)child.Name).ToString().Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				return child;
			}
			Node val = FindNodeByName(child, name);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static void SyncHighlightShaderMaterial(CanvasItem target, Material? material)
	{
		NCardHighlight val = (NCardHighlight)(object)((target is NCardHighlight) ? target : null);
		if (val != null)
		{
			ShaderMaterial val2 = (ShaderMaterial)(object)((material is ShaderMaterial) ? material : null);
			if (val2 != null)
			{
				HighlightShaderMaterialField?.SetValue(val, val2);
			}
		}
	}

	private static void SetVisible(CanvasItem? target, bool? visible)
	{
		if (target != null && visible.HasValue)
		{
			bool valueOrDefault = visible == true;
			target.Visible = valueOrDefault;
		}
	}

	public static void ApplySelectedPortraitToNode(NCard card, ExternalCardVisualOwnership externalOwnership)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Invalid comparison between Unknown and I4
		if (card.Model == null)
		{
			return;
		}
		if (externalOwnership.Portrait)
		{
			CardSurfaceView.Release(card);
			return;
		}
		Texture2D portrait = card.Model.Portrait;
		CardPresentationDefinition cardPresentation = SkinService.GetCardPresentation(card.Model);
		string text = (((int)card.Model.Rarity == 5 || ((object)cardPresentation != null && cardPresentation.UseAncientLayout) || ((object)cardPresentation != null && cardPresentation.UseExpandedPortraitLayout)) ? "%AncientPortrait" : "%Portrait");
		TextureRect nodeOrNull = ((Node)card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit(text));
		if (nodeOrNull != null)
		{
			nodeOrNull.Texture = portrait;
			CardSurfaceView.ApplyPortrait(card, nodeOrNull);
		}
	}

	public static void ResetAvailabilityFilter(NCardLibrary screen)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)screen).SetMeta(StringName.op_Implicit("sts2_skinned_cards_only"), Variant.op_Implicit(false));
		NLibraryStatTickbox nodeOrNull = ((Node)screen).GetNodeOrNull<NLibraryStatTickbox>(NodePath.op_Implicit("Sidebar/MarginContainer/BottomVBox/STS2SkinnedCardsOnly"));
		if (nodeOrNull != null)
		{
			((NTickbox)nodeOrNull).IsTicked = false;
		}
	}

	public static void ApplyAvailabilityFilter(NCardLibraryGrid grid, ref Func<CardModel, bool> filter)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Node val = (Node)(object)grid;
		while (val != null && !(val is NCardLibrary))
		{
			val = val.GetParent();
		}
		NCardLibrary val2 = (NCardLibrary)(object)((val is NCardLibrary) ? val : null);
		if (val2 == null)
		{
			return;
		}
		Variant meta = ((GodotObject)val2).GetMeta(StringName.op_Implicit("sts2_skinned_cards_only"), Variant.op_Implicit(false));
		if (((Variant)(ref meta)).AsBool())
		{
			Func<CardModel, bool> original = filter;
			filter = (CardModel card) => original(card) && SkinService.HasCardSkin(card);
		}
	}

	private static void SetAvailabilityFilter(NCardLibrary screen, string meta, bool enabled)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)screen).SetMeta(StringName.op_Implicit(meta), Variant.op_Implicit(enabled));
		try
		{
			UpdateLibraryFilterMethod.Invoke(screen, new object[1] { false });
		}
		catch (Exception ex)
		{
			ModLog.Error("刷新有皮肤卡牌筛选失败：" + ex);
		}
	}

	private static void ShowFirstAvailableGroup(HBoxContainer selector)
	{
		Populate(selector, (FindGroup("ironclad") ?? SkinService.Catalog?.CardGroups.FirstOrDefault())?.Id);
	}

	private static void Populate(HBoxContainer selector, string? groupId)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Button node = ((Node)selector).GetNode<Button>(NodePath.op_Implicit("CardSkinPriorityButton"));
		Button node2 = ((Node)selector).GetNode<Button>(NodePath.op_Implicit("CardSkinPresetButton"));
		CardSkinGroup cardSkinGroup = ((groupId == null) ? null : FindGroup(groupId));
		if (cardSkinGroup == null || cardSkinGroup.Options.Count == 0)
		{
			((CanvasItem)selector).Visible = false;
			node.Text = string.Empty;
			return;
		}
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_card_skin_group"), Variant.op_Implicit(cardSkinGroup.Id));
		IReadOnlyList<CardPriorityOptionState> cardPriorityOptions = SkinService.GetCardPriorityOptions(cardSkinGroup.Id);
		node.Text = string.Format(ModLocalization.Get(ModText.CardSkinPriority), cardPriorityOptions.Count((CardPriorityOptionState option) => option.Enabled));
		((Control)node).TooltipText = string.Empty;
		node2.Text = ModLocalization.Get(ModText.CardPresets);
		((Control)node2).TooltipText = string.Empty;
		((CanvasItem)selector).Visible = true;
	}

	private static Control CreatePresetOverlay()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		Control overlay = new Control
		{
			Name = StringName.op_Implicit("STS2CardSkinPresetOverlay"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 2000
		};
		ColorRect mask = new ColorRect
		{
			Name = StringName.op_Implicit("Mask"),
			Color = new Color(0f, 0f, 0f, 0.68f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Control)mask).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && val.Pressed && (long)val.ButtonIndex == 1)
			{
				((CanvasItem)overlay).Visible = false;
				((Control)mask).AcceptEvent();
			}
		};
		((Node)overlay).AddChild((Node)(object)mask, false, (InternalMode)0);
		((Control)mask).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		PanelContainer val2 = new PanelContainer
		{
			Name = StringName.op_Implicit("Panel"),
			MouseFilter = (MouseFilterEnum)0,
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -470f,
			OffsetTop = -280f,
			OffsetRight = 470f,
			OffsetBottom = 280f
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("241a30"), new Color("79547e"), 2));
		ModThemeRuntime.Panel(val2);
		((Node)overlay).AddChild((Node)(object)val2, false, (InternalMode)0);
		MarginContainer val3 = new MarginContainer
		{
			Name = StringName.op_Implicit("Margin")
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 20);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 18);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 20);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 18);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		VBoxContainer val4 = new VBoxContainer
		{
			Name = StringName.op_Implicit("PresetContent")
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		return overlay;
	}

	private static void OpenPresetOverlay(NCardLibrary screen, HBoxContainer selector, Control overlay)
	{
		BuildPresetOverlay(screen, selector, overlay);
		((CanvasItem)overlay).Visible = true;
		((CanvasItem)overlay).MoveToFront();
	}

	private static void BuildPresetOverlay(NCardLibrary screen, HBoxContainer selector, Control overlay)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Expected O, but got Unknown
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Expected O, but got Unknown
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Expected O, but got Unknown
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Expected O, but got Unknown
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Expected O, but got Unknown
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Expected O, but got Unknown
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_card_skin_group"), Variant.op_Implicit(string.Empty));
		string groupId = ((Variant)(ref meta)).AsString();
		VBoxContainer node = ((Node)overlay).GetNode<VBoxContainer>(NodePath.op_Implicit("Panel/Margin/PresetContent"));
		ScrollContainer val = ScrollListRebuild.Begin(node, groupId);
		Label val2 = new Label
		{
			Text = ModLocalization.Get(ModText.CardPresets),
			HorizontalAlignment = (HorizontalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 25);
		ModThemeRuntime.AccentText((Control)(object)val2);
		((Node)node).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(890f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)node).AddChild((Node)(object)val3, false, (InternalMode)0);
		LineEdit newName = new LineEdit
		{
			PlaceholderText = ModLocalization.Get(ModText.CardPresetName),
			MaxLength = 40,
			CustomMinimumSize = new Vector2(600f, 40f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ModThemeRuntime.Input(newName, 18);
		((Node)val3).AddChild((Node)(object)newName, false, (InternalMode)0);
		Button val4 = new Button
		{
			Text = ModLocalization.Get(ModText.SaveCurrentPreset),
			CustomMinimumSize = new Vector2(220f, 40f)
		};
		ContextualSkinControls.ApplyGameTheme(val4);
		((Control)val4).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
		((BaseButton)val4).Pressed += delegate
		{
			QueuePresetChange(screen, selector, overlay, delegate
			{
				bool num = SkinService.CreateCardSkinPreset(groupId, newName.Text);
				if (num)
				{
					newName.Text = string.Empty;
				}
				return num;
			}, refreshCards: false);
		};
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Control)val).CustomMinimumSize = new Vector2(890f, 360f);
		((Control)val).SizeFlagsVertical = (SizeFlags)3;
		ScrollListRebuild.PlaceAfterHeader(val);
		VBoxContainer val5 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		IReadOnlyList<CardSkinPresetState> cardSkinPresets = SkinService.GetCardSkinPresets(groupId);
		if (cardSkinPresets.Count == 0)
		{
			Label val6 = new Label
			{
				Text = ModLocalization.Get(ModText.NoCardPresets),
				HorizontalAlignment = (HorizontalAlignment)1,
				VerticalAlignment = (VerticalAlignment)1,
				CustomMinimumSize = new Vector2(860f, 100f),
				MouseFilter = (MouseFilterEnum)2
			};
			((Control)val6).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
			((Control)val6).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("b9adbd"));
			((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		}
		foreach (CardSkinPresetState preset in cardSkinPresets)
		{
			HBoxContainer val7 = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(870f, 46f),
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)val5).AddChild((Node)(object)val7, false, (InternalMode)0);
			Label val8 = new Label
			{
				Text = (preset.Active ? "●" : string.Empty),
				TooltipText = (preset.Active ? ModLocalization.Get(ModText.ActiveCardPreset) : string.Empty),
				CustomMinimumSize = new Vector2(24f, 38f),
				HorizontalAlignment = (HorizontalAlignment)1,
				VerticalAlignment = (VerticalAlignment)1,
				MouseFilter = (MouseFilterEnum)1
			};
			((Control)val8).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			ModThemeRuntime.AccentText((Control)(object)val8);
			((Node)val7).AddChild((Node)(object)val8, false, (InternalMode)0);
			LineEdit name = new LineEdit
			{
				Text = preset.DisplayName,
				Editable = !preset.IsBundlePreset,
				MaxLength = ((!preset.IsBundlePreset) ? 40 : 0),
				CustomMinimumSize = new Vector2(310f, 38f),
				SizeFlagsHorizontal = (SizeFlags)3
			};
			ModThemeRuntime.Input(name, 18, preset.IsBundlePreset);
			((Node)val7).AddChild((Node)(object)name, false, (InternalMode)0);
			Button val9 = CreatePresetActionButton(preset.Active ? ModLocalization.Get(ModText.ActiveCardPreset) : ModLocalization.Get(ModText.ApplyCardPreset), 112f);
			((BaseButton)val9).Disabled = preset.Active;
			((BaseButton)val9).Pressed += delegate
			{
				QueuePresetChange(screen, selector, overlay, () => SkinService.ApplyCardSkinPreset(groupId, preset.Name), refreshCards: true);
			};
			((Node)val7).AddChild((Node)(object)val9, false, (InternalMode)0);
			Button val10 = CreatePresetActionButton(ModLocalization.Get(ModText.OverwriteCardPreset), 100f);
			((BaseButton)val10).Pressed += delegate
			{
				QueuePresetChange(screen, selector, overlay, () => SkinService.OverwriteCardSkinPreset(groupId, preset.Name), refreshCards: false);
			};
			((Node)val7).AddChild((Node)(object)val10, false, (InternalMode)0);
			Button val11 = CreatePresetActionButton(ModLocalization.Get(ModText.RenameCardPreset), 100f);
			((BaseButton)val11).Pressed += delegate
			{
				QueuePresetChange(screen, selector, overlay, () => SkinService.RenameCardSkinPreset(groupId, preset.Name, name.Text), refreshCards: false);
			};
			((Node)val7).AddChild((Node)(object)val11, false, (InternalMode)0);
			((BaseButton)val11).Disabled = preset.IsBundlePreset;
			Button delete = CreatePresetActionButton(ModLocalization.Get(ModText.DeleteCardPreset), 112f);
			bool deleteArmed = false;
			((BaseButton)delete).Pressed += delegate
			{
				if (!deleteArmed)
				{
					deleteArmed = true;
					delete.Text = ModLocalization.Get(ModText.ConfirmDeleteCardPreset);
				}
				else
				{
					QueuePresetChange(screen, selector, overlay, () => SkinService.DeleteCardSkinPreset(groupId, preset.Name), refreshCards: false);
				}
			};
			((Node)val7).AddChild((Node)(object)delete, false, (InternalMode)0);
			((BaseButton)delete).Disabled = preset.IsBundlePreset;
		}
		Button val12 = CreatePresetActionButton(ModLocalization.Get(ModText.Close), 180f);
		((Control)val12).SizeFlagsHorizontal = (SizeFlags)4;
		((BaseButton)val12).Pressed += delegate
		{
			((CanvasItem)overlay).Visible = false;
		};
		((Node)node).AddChild((Node)(object)val12, false, (InternalMode)0);
	}

	private static Button CreatePresetActionButton(string text, float width)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		Button val = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(width, 38f)
		};
		ContextualSkinControls.ApplyGameTheme(val);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
		return val;
	}

	private static void QueuePresetChange(NCardLibrary screen, HBoxContainer selector, Control overlay, Func<bool> change, bool refreshCards)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (!GodotObject.IsInstanceValid((GodotObject)(object)screen) || !change())
			{
				ModLog.Error("调整卡图预设失败：" + SkinService.LastError);
			}
			else
			{
				Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_card_skin_group"), Variant.op_Implicit(string.Empty));
				string text = ((Variant)(ref meta)).AsString();
				Populate(selector, text);
				BuildPresetOverlay(screen, selector, overlay);
				if (refreshCards && !string.IsNullOrWhiteSpace(text))
				{
					RefreshVisibleCards(screen, text);
					RefreshSourceIndicators(screen);
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static Control CreatePriorityOverlay()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected O, but got Unknown
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected O, but got Unknown
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Expected O, but got Unknown
		Control overlay = new Control
		{
			Name = StringName.op_Implicit("STS2CardSkinPriorityOverlay"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 2000
		};
		ColorRect mask = new ColorRect
		{
			Name = StringName.op_Implicit("Mask"),
			Color = new Color(0f, 0f, 0f, 0.68f),
			MouseFilter = (MouseFilterEnum)0
		};
		((Control)mask).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && val.Pressed && (long)val.ButtonIndex == 1)
			{
				((CanvasItem)overlay).Visible = false;
				((Control)mask).AcceptEvent();
			}
		};
		((Node)overlay).AddChild((Node)(object)mask, false, (InternalMode)0);
		((Control)mask).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		PanelContainer val2 = new PanelContainer
		{
			Name = StringName.op_Implicit("Panel"),
			MouseFilter = (MouseFilterEnum)0,
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -360f,
			OffsetTop = -240f,
			OffsetRight = 360f,
			OffsetBottom = 240f
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("241a30"), new Color("79547e"), 2));
		ModThemeRuntime.Panel(val2);
		((Node)overlay).AddChild((Node)(object)val2, false, (InternalMode)0);
		MarginContainer val3 = new MarginContainer
		{
			Name = StringName.op_Implicit("Margin")
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 20);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 18);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 20);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 18);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		VBoxContainer val4 = new VBoxContainer
		{
			Name = StringName.op_Implicit("PriorityContent")
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		return overlay;
	}

	private static void OpenPriorityOverlay(NCardLibrary screen, HBoxContainer selector, Control overlay)
	{
		BuildPriorityOverlay(screen, selector, overlay);
		((CanvasItem)overlay).Visible = true;
		((CanvasItem)overlay).MoveToFront();
	}

	private static void BuildPriorityOverlay(NCardLibrary screen, HBoxContainer selector, Control overlay)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Expected O, but got Unknown
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Expected O, but got Unknown
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Expected O, but got Unknown
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Expected O, but got Unknown
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Expected O, but got Unknown
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Expected O, but got Unknown
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_0417: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_043c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Expected O, but got Unknown
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d5: Expected O, but got Unknown
		//IL_0529: Unknown result type (might be due to invalid IL or missing references)
		//IL_052e: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		//IL_0550: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Expected O, but got Unknown
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Expected O, but got Unknown
		//IL_059c: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b1: Expected O, but got Unknown
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_card_skin_group"), Variant.op_Implicit(string.Empty));
		string groupId = ((Variant)(ref meta)).AsString();
		CardSkinGroup? cardSkinGroup = FindGroup(groupId);
		VBoxContainer node = ((Node)overlay).GetNode<VBoxContainer>(NodePath.op_Implicit("Panel/Margin/PriorityContent"));
		ScrollContainer val = ScrollListRebuild.Begin(node, groupId);
		if (cardSkinGroup == null)
		{
			return;
		}
		Label val2 = new Label
		{
			Text = string.Format(ModLocalization.Get(ModText.CardSkinPriority), SkinService.GetCardPriorityOptions(groupId).Count((CardPriorityOptionState option) => option.Enabled)),
			HorizontalAlignment = (HorizontalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 25);
		ModThemeRuntime.AccentText((Control)(object)val2);
		((Node)node).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Control)val).CustomMinimumSize = new Vector2(670f, 350f);
		((Control)val).SizeFlagsVertical = (SizeFlags)3;
		ScrollListRebuild.PlaceAfterHeader(val);
		VBoxContainer val3 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		IReadOnlyList<CardPriorityOptionState> cardPriorityOptions = SkinService.GetCardPriorityOptions(groupId);
		foreach (CardPriorityOptionState option2 in cardPriorityOptions)
		{
			HBoxContainer val4 = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(650f, 42f),
				SizeFlagsHorizontal = (SizeFlags)3
			};
			((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val4).AddChild((Node)new ColorRect
			{
				Color = SourceColor(option2.ColorIndex),
				CustomMinimumSize = new Vector2(13f, 32f),
				MouseFilter = (MouseFilterEnum)2
			}, false, (InternalMode)0);
			CheckBox val5 = new CheckBox
			{
				ButtonPressed = option2.Enabled,
				Text = ModLocalization.Get(ModText.EnabledForCategory),
				CustomMinimumSize = new Vector2(88f, 32f),
				TooltipText = ModLocalization.DisplayOptionName(option2.Name)
			};
			ContextualSkinControls.ApplyGameTheme((Button)(object)val5);
			((Control)val5).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
			((Control)val5).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
			((Control)val5).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), Colors.White);
			((BaseButton)val5).Toggled += (ToggledEventHandler)delegate(bool value)
			{
				QueuePriorityChange(screen, selector, overlay, groupId, () => SkinService.SetCardPriorityEnabled(groupId, option2.OptionId, value));
			};
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
			Label val6 = new Label
			{
				Text = ModLocalization.DisplayOptionName(option2.Name),
				ClipText = true,
				CustomMinimumSize = new Vector2(270f, 36f),
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1,
				TooltipText = ModLocalization.DisplayOptionName(option2.Name)
			};
			((Control)val6).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((Control)val6).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
			((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
			Label val7 = new Label
			{
				Text = $"{option2.Coverage}/{option2.TotalCards}",
				CustomMinimumSize = new Vector2(70f, 36f),
				HorizontalAlignment = (HorizontalAlignment)2,
				VerticalAlignment = (VerticalAlignment)1,
				MouseFilter = (MouseFilterEnum)2
			};
			((Control)val7).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 16);
			((Control)val7).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("afcdde"));
			((Node)val4).AddChild((Node)(object)val7, false, (InternalMode)0);
			Button val8 = new Button
			{
				Text = "↑",
				Disabled = (option2 == cardPriorityOptions[0]),
				CustomMinimumSize = new Vector2(42f, 34f)
			};
			ContextualSkinControls.ApplyGameTheme(val8);
			((Control)val8).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((BaseButton)val8).Pressed += delegate
			{
				QueuePriorityChange(screen, selector, overlay, groupId, () => SkinService.MoveCardPriority(groupId, option2.OptionId, -1));
			};
			((Node)val4).AddChild((Node)(object)val8, false, (InternalMode)0);
			Button val9 = new Button
			{
				Text = "↓"
			};
			((BaseButton)val9).Disabled = option2 == cardPriorityOptions[cardPriorityOptions.Count - 1];
			((Control)val9).CustomMinimumSize = new Vector2(42f, 34f);
			Button val10 = val9;
			ContextualSkinControls.ApplyGameTheme(val10);
			((Control)val10).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((BaseButton)val10).Pressed += delegate
			{
				QueuePriorityChange(screen, selector, overlay, groupId, () => SkinService.MoveCardPriority(groupId, option2.OptionId, 1));
			};
			((Node)val4).AddChild((Node)(object)val10, false, (InternalMode)0);
		}
		Button val11 = new Button
		{
			Text = ModLocalization.Get(ModText.Close),
			CustomMinimumSize = new Vector2(180f, 42f),
			SizeFlagsHorizontal = (SizeFlags)4
		};
		ContextualSkinControls.ApplyGameTheme(val11);
		((Control)val11).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
		((BaseButton)val11).Pressed += delegate
		{
			((CanvasItem)overlay).Visible = false;
		};
		HBoxContainer val12 = new HBoxContainer();
		((Node)val12).AddChild((Node)(object)val11, false, (InternalMode)0);
		((Node)val12).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val12).AddChild((Node)(object)SkinWorkshopEntry.CreatePriorityButton((Node)(object)screen, "cards", groupId, delegate
		{
			BuildPriorityOverlay(screen, selector, overlay);
		}), false, (InternalMode)0);
		((Node)node).AddChild((Node)(object)val12, false, (InternalMode)0);
	}

	private static void QueuePriorityChange(NCardLibrary screen, HBoxContainer selector, Control overlay, string groupId, Func<bool> change)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen) && GodotObject.IsInstanceValid((GodotObject)(object)selector) && GodotObject.IsInstanceValid((GodotObject)(object)overlay))
			{
				long timestamp = Stopwatch.GetTimestamp();
				Dictionary<NCard, (CardModel, string)> before = CaptureCardSelections(screen, groupId);
				long timestamp2 = Stopwatch.GetTimestamp();
				if (!change())
				{
					ModLog.Error("调整卡牌皮肤优先级失败：" + SkinService.LastError);
				}
				else
				{
					long timestamp3 = Stopwatch.GetTimestamp();
					Populate(selector, groupId);
					BuildPriorityOverlay(screen, selector, overlay);
					long timestamp4 = Stopwatch.GetTimestamp();
					RefreshVisibleCards(screen, groupId, before);
					ModLog.Info($"卡牌优先级耗时：{groupId}；选择快照={Stopwatch.GetElapsedTime(timestamp, timestamp2).TotalMilliseconds:F1} ms，资源与保存={Stopwatch.GetElapsedTime(timestamp2, timestamp3).TotalMilliseconds:F1} ms，列表={Stopwatch.GetElapsedTime(timestamp3, timestamp4).TotalMilliseconds:F1} ms，卡面={Stopwatch.GetElapsedTime(timestamp4).TotalMilliseconds:F1} ms。");
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static Dictionary<NCard, (CardModel Model, string Selection)>? CaptureCardSelections(NCardLibrary screen, string groupId)
	{
		try
		{
			return (from card in Descendants((Node)(object)screen).OfType<NCard>()
				where GodotObject.IsInstanceValid((GodotObject)(object)card) && card.Model != null && SkinService.CardBelongsToGroup(card.Model, groupId)
				select card).ToDictionary((NCard card) => card, (NCard card) => (card.Model, SkinService.GetEffectiveCardSelection(card.Model)));
		}
		catch (Exception ex)
		{
			ModLog.Warn("读取卡牌换肤前状态失败，本次使用完整刷新：" + ex.GetBaseException().Message);
			return null;
		}
	}

	public static void RememberPreviewMode(NCard card, CardPreviewMode previewMode)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		CardPreviewState value = PreviewModes.GetValue(card, (NCard _) => new CardPreviewState());
		value.Model = card.Model;
		value.Mode = previewMode;
	}

	private static CardPreviewMode GetRefreshPreviewMode(NCard card)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!PreviewModes.TryGetValue(card, out CardPreviewState value))
		{
			return (CardPreviewMode)1;
		}
		return value.GetMode(card.Model);
	}

	public static void RefreshCardSkin(NCard card)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if (GodotObject.IsInstanceValid((GodotObject)(object)card) && !((GodotObject)card).IsQueuedForDeletion() && ((Node)card).IsNodeReady() && card.Model != null)
		{
			PileType displayingPile = card.DisplayingPile;
			CardPreviewMode refreshPreviewMode = GetRefreshPreviewMode(card);
			ReloadCardMethod.Invoke(card, null);
			card.UpdateVisuals(displayingPile, refreshPreviewMode);
		}
	}

	private static void RefreshVisibleCards(NCardLibrary screen, string groupId, IReadOnlyDictionary<NCard, (CardModel Model, string Selection)>? before = null)
	{
		try
		{
			CardSurfaceView.RefreshTinyCards();
			int num = 0;
			int num2 = 0;
			foreach (NCard item in Descendants((Node)(object)screen).OfType<NCard>())
			{
				if (item.Model == null || !SkinService.CardBelongsToGroup(item.Model, groupId))
				{
					continue;
				}
				if (before != null && before.TryGetValue(item, out (CardModel, string) value) && value.Item1 == item.Model && value.Item2.Equals(SkinService.GetEffectiveCardSelection(item.Model), StringComparison.OrdinalIgnoreCase))
				{
					UpdateLibrarySourceIndicators(item);
					num2++;
					continue;
				}
				CardRefreshDiagnostics.Begin(item, num < 3 && (PresentationLayouts.TryGetValue(item, out CardPresentationState _) || SkinService.GetCardPresentation(item.Model) != null));
				try
				{
					RefreshCardSkin(item);
				}
				finally
				{
					CardRefreshDiagnostics.End(item);
				}
				num++;
			}
			ModLog.Info($"卡牌分类刷新：{groupId}；重载={num}，来源未变跳过={num2}。");
		}
		catch (Exception ex)
		{
			ModLog.Error("刷新卡牌总览皮肤失败：" + ex);
		}
	}

	private static void RefreshSourceIndicators(NCardLibrary screen)
	{
		foreach (NCard item in Descendants((Node)(object)screen).OfType<NCard>())
		{
			UpdateLibrarySourceIndicators(item);
		}
	}

	private static CardSkinGroup? FindGroup(string? groupId)
	{
		if (groupId != null)
		{
			return SkinService.Catalog?.CardGroups.FirstOrDefault((CardSkinGroup group) => group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		}
		return null;
	}

	private static string GetGroupId(NCardPoolFilter filter)
	{
		string text = ((object)((Node)filter).Name).ToString();
		if (!text.EndsWith("Pool", StringComparison.OrdinalIgnoreCase))
		{
			return text.ToLowerInvariant();
		}
		string text2 = text;
		return text2.Substring(0, text2.Length - 4).ToLowerInvariant();
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__89))]
	private static IEnumerable<Node> Descendants(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__89(-2)
		{
			_003C_003E3__root = root
		};
	}
}

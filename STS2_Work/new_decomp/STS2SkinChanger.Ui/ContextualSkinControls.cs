using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.RegularExpressions.Generated;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class ContextualSkinControls
{
	private sealed record CharacterBackgroundHostLayout(float AnchorLeft, float AnchorTop, float AnchorRight, float AnchorBottom, float OffsetLeft, float OffsetTop, float OffsetRight, float OffsetBottom, GrowDirection GrowHorizontal, GrowDirection GrowVertical, Vector2 PivotOffset, Vector2 Scale, float Rotation)
	{
		internal static CharacterBackgroundHostLayout Capture(Control control)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			return new CharacterBackgroundHostLayout(control.AnchorLeft, control.AnchorTop, control.AnchorRight, control.AnchorBottom, control.OffsetLeft, control.OffsetTop, control.OffsetRight, control.OffsetBottom, control.GrowHorizontal, control.GrowVertical, control.PivotOffset, control.Scale, control.Rotation);
		}

		internal bool Matches(Control control)
		{
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			if (Mathf.IsEqualApprox(control.AnchorLeft, AnchorLeft) && Mathf.IsEqualApprox(control.AnchorTop, AnchorTop) && Mathf.IsEqualApprox(control.AnchorRight, AnchorRight) && Mathf.IsEqualApprox(control.AnchorBottom, AnchorBottom) && Mathf.IsEqualApprox(control.OffsetLeft, OffsetLeft) && Mathf.IsEqualApprox(control.OffsetTop, OffsetTop) && Mathf.IsEqualApprox(control.OffsetRight, OffsetRight) && Mathf.IsEqualApprox(control.OffsetBottom, OffsetBottom) && control.GrowHorizontal == GrowHorizontal && control.GrowVertical == GrowVertical)
			{
				Vector2 val = control.PivotOffset;
				if (((Vector2)(ref val)).IsEqualApprox(PivotOffset))
				{
					val = control.Scale;
					if (((Vector2)(ref val)).IsEqualApprox(Scale))
					{
						return Mathf.IsEqualApprox(control.Rotation, Rotation);
					}
				}
			}
			return false;
		}

		internal void Apply(Control control)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			control.AnchorLeft = AnchorLeft;
			control.AnchorTop = AnchorTop;
			control.AnchorRight = AnchorRight;
			control.AnchorBottom = AnchorBottom;
			control.OffsetLeft = OffsetLeft;
			control.OffsetTop = OffsetTop;
			control.OffsetRight = OffsetRight;
			control.OffsetBottom = OffsetBottom;
			control.GrowHorizontal = GrowHorizontal;
			control.GrowVertical = GrowVertical;
			control.PivotOffset = PivotOffset;
			control.Scale = Scale;
			control.Rotation = Rotation;
		}

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_014b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Unknown result type (might be due to invalid IL or missing references)
			//IL_0177: Unknown result type (might be due to invalid IL or missing references)
			//IL_0199: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("AnchorLeft = ");
			builder.Append(AnchorLeft.ToString());
			builder.Append(", AnchorTop = ");
			builder.Append(AnchorTop.ToString());
			builder.Append(", AnchorRight = ");
			builder.Append(AnchorRight.ToString());
			builder.Append(", AnchorBottom = ");
			builder.Append(AnchorBottom.ToString());
			builder.Append(", OffsetLeft = ");
			builder.Append(OffsetLeft.ToString());
			builder.Append(", OffsetTop = ");
			builder.Append(OffsetTop.ToString());
			builder.Append(", OffsetRight = ");
			builder.Append(OffsetRight.ToString());
			builder.Append(", OffsetBottom = ");
			builder.Append(OffsetBottom.ToString());
			builder.Append(", GrowHorizontal = ");
			GrowDirection val = GrowHorizontal;
			builder.Append(((object)(GrowDirection)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", GrowVertical = ");
			val = GrowVertical;
			builder.Append(((object)(GrowDirection)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", PivotOffset = ");
			Vector2 val2 = PivotOffset;
			builder.Append(((object)(Vector2)(ref val2)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Scale = ");
			val2 = Scale;
			builder.Append(((object)(Vector2)(ref val2)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Rotation = ");
			builder.Append(Rotation.ToString());
			return true;
		}

		[CompilerGenerated]
		public void Deconstruct(out float AnchorLeft, out float AnchorTop, out float AnchorRight, out float AnchorBottom, out float OffsetLeft, out float OffsetTop, out float OffsetRight, out float OffsetBottom, out GrowDirection GrowHorizontal, out GrowDirection GrowVertical, out Vector2 PivotOffset, out Vector2 Scale, out float Rotation)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected I8, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected I8, but got Unknown
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			AnchorLeft = this.AnchorLeft;
			AnchorTop = this.AnchorTop;
			AnchorRight = this.AnchorRight;
			AnchorBottom = this.AnchorBottom;
			OffsetLeft = this.OffsetLeft;
			OffsetTop = this.OffsetTop;
			OffsetRight = this.OffsetRight;
			OffsetBottom = this.OffsetBottom;
			GrowHorizontal = (GrowDirection)(long)this.GrowHorizontal;
			GrowVertical = (GrowDirection)(long)this.GrowVertical;
			PivotOffset = this.PivotOffset;
			Scale = this.Scale;
			Rotation = this.Rotation;
		}
	}

	private sealed record MonsterSkinCategory(string Id, string DisplayName, IReadOnlyList<string> GroupIds);

	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static ToggledEventHandler _003C0_003E__SetReceiveMultiplayerSkinChanges;

		public static Func<string, string> _003C1_003E__DisplayOptionName;

		public static Func<string, bool> _003C2_003E__IsSkinChoice;
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateNodes_003Ed__103 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private IEnumerator<Node> _003C_003E7__wrap2;

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
		public _003CEnumerateNodes_003Ed__103(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if ((uint)(num - -4) <= 1u || num == 2)
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
			_003C_003E7__wrap2 = null;
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
					_003C_003E2__current = root;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00d0;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00b6;
					}
					IL_00d0:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Node current = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = EnumerateNodes(current).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00b6;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_00b6:
					if (_003C_003E7__wrap2.MoveNext())
					{
						Node current2 = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current2;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_00d0;
				}
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
			if (_003C_003E7__wrap2 != null)
			{
				_003C_003E7__wrap2.Dispose();
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
			_003CEnumerateNodes_003Ed__103 _003CEnumerateNodes_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateNodes_003Ed__ = this;
			}
			else
			{
				_003CEnumerateNodes_003Ed__ = new _003CEnumerateNodes_003Ed__103(0);
			}
			_003CEnumerateNodes_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateNodes_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string SelectorName = "STS2SkinSelector";

	private const string MultiplayerSkinSyncToggleName = "MultiplayerSkinSyncToggle";

	private const string MultiplayerSkinLoadingToggleName = "MultiplayerSkinLoadingToggle";

	private const string DropdownName = "SkinDropdown";

	private const string MonsterScaleSliderName = "MonsterScaleSlider";

	private const string MonsterScaleValueName = "MonsterScaleValue";

	private const string MonsterScaleLabelName = "MonsterScaleLabel";

	private const string MonsterScaleResetName = "MonsterScaleReset";

	private const string CharacterRefreshGenerationMeta = "sts2_skin_character_refresh_generation";

	private const string CharacterLoadingGenerationMeta = "sts2_skin_character_loading_generation";

	private const string CharacterLoadingOverlayName = "STS2CharacterSkinLoadingOverlay";

	private const string CharacterBundlePopupListName = "STS2CharacterBundlePopupList";

	private const string GroupMeta = "sts2_skin_group";

	private const string UpdatingMeta = "sts2_skin_updating";

	private const string MonsterScaleGroupMeta = "sts2_skin_monster_scale_group";

	private const string MonsterBaseScaleMeta = "sts2_skin_monster_base_scale";

	private const string MonsterBaseDefaultScaleMeta = "sts2_skin_monster_base_default_scale";

	private const string MonsterAppliedScaleMeta = "sts2_skin_monster_applied_scale";

	internal const string ManagedCombatSceneMeta = "sts2_skin_managed_combat_scene";

	private static readonly Dictionary<ulong, Action> RefreshActions = new Dictionary<ulong, Action>();

	private static readonly ConditionalWeakTable<NCharacterSelectScreen, CharacterBackgroundHostLayout> CharacterBackgroundHostLayouts = new ConditionalWeakTable<NCharacterSelectScreen, CharacterBackgroundHostLayout>();

	private static readonly HashSet<string> LoggedMissingMultiplayerIcons = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static bool _refreshingMonsterDisplay;

	private static readonly ReloadingReferenceCache<Font> GameFontCache = new ReloadingReferenceCache<Font>();

	private static readonly FieldInfo BestiarySelectedEntryField = AccessTools.Field(typeof(NBestiary), "_selectedEntry");

	private static readonly MethodInfo BestiarySelectMonsterMethod = AccessTools.Method(typeof(NBestiary), "SelectMonster", new Type[1] { typeof(NBestiaryEntry) }, (Type[])null);

	private static readonly MethodInfo MonsterVisualsPathGetter = AccessTools.PropertyGetter(typeof(MonsterModel), "VisualsPath");

	private static readonly FieldInfo CharacterNameField = AccessTools.Field(typeof(NCharacterSelectScreen), "_name");

	private static readonly FieldInfo CharacterDescriptionField = AccessTools.Field(typeof(NCharacterSelectScreen), "_description");

	private static readonly FieldInfo CharacterRelicTitleField = AccessTools.Field(typeof(NCharacterSelectScreen), "_relicTitle");

	private static readonly FieldInfo CharacterRelicDescriptionField = AccessTools.Field(typeof(NCharacterSelectScreen), "_relicDescription");

	private static readonly FieldInfo CharacterRelicIconField = AccessTools.Field(typeof(NCharacterSelectScreen), "_relicIcon");

	private static readonly FieldInfo CharacterRelicIconOutlineField = AccessTools.Field(typeof(NCharacterSelectScreen), "_relicIconOutline");

	private static readonly MethodInfo? CharacterBackgroundWindowChangeMethod = AccessTools.Method(typeof(NCharacterSelectScreenBg), "OnWindowChange", (Type[])null, (Type[])null);

	private const string MonsterPriorityHeaderName = "MonsterSkinPriorityHeader";

	private const string MonsterPriorityRegionLabelName = "MonsterSkinPriorityRegionLabel";

	private const string MonsterPriorityButtonName = "MonsterSkinPriorityButton";

	private const string MonsterPriorityOverlayName = "MonsterSkinPriorityOverlay";

	private const string MonsterPriorityPanelName = "PriorityPanel";

	private const string MonsterPriorityMarginName = "PriorityMargin";

	private const string MonsterPriorityContentName = "PriorityContent";

	private const string MonsterPresetButtonName = "MonsterPresetButton";

	private const string MonsterPresetOverlayName = "MonsterPresetOverlay";

	private const string MonsterPresetPanelName = "PresetPanel";

	private const string MonsterPresetMarginName = "PresetMargin";

	private const string MonsterPresetContentName = "PresetContent";

	private const string MonsterCategoryMeta = "sts2_skin_monster_category";

	private const string MonsterCategoryNameMeta = "sts2_skin_monster_category_name";

	private static readonly Color[] MonsterPriorityColors = (Color[])(object)new Color[8]
	{
		new Color("df6688"),
		new Color("58b9d1"),
		new Color("e1a95f"),
		new Color("78c891"),
		new Color("ad83d4"),
		new Color("e47d67"),
		new Color("71a3e3"),
		new Color("c4c85f")
	};

	private static readonly Dictionary<string, IReadOnlyList<string>> MonsterCategoryGroupCache = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

	internal static bool IsRefreshingMonsterDisplay => _refreshingMonsterDisplay;

	internal static Font? GameFont => GameFontCache.Get(() => ResourceLoader.Load<Font>("res://themes/kreon_bold_glyph_space_one.tres", (string)null, (CacheMode)1), (Font font) => GodotObject.IsInstanceValid((GodotObject)(object)font));

	internal static string CanonicalScenePath(string innerPath)
	{
		return "res://scenes/" + innerPath.TrimStart('/') + ".tscn";
	}

	internal static string CanonicalImagePath(string innerPath)
	{
		return "res://images/" + innerPath.TrimStart('/');
	}

	public static void ShowCharacter(NCharacterSelectScreen screen, CharacterModel character)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		CharacterModelPreviewControls.Hide(screen);
		CaptureCharacterBackgroundHostLayout(screen);
		CancelCharacterDropdownSelection(screen);
		ManagedSkinModLoader.RestoreCharacterPresentation(screen);
		RemoveStaleProviderCharacterSelectControls(screen);
		HBoxContainer selector = EnsureCharacterSelector(screen);
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sc_workshop_target"), Variant.op_Implicit(((AbstractModel)character).Id.Entry.ToLowerInvariant()));
		SkinGroup group = FindGroup(((AbstractModel)character).Id.Entry);
		if (group != null)
		{
			SkinService.FocusRuntimeProviderBehaviorsOnGroups(new _003C_003Ez__ReadOnlySingleElementList<string>(group.Id), (IReadOnlyCollection<string>)(object)Array.Empty<string>(), "选角预览");
		}
		RegisterRefresh(selector, delegate
		{
			RebuildCharacterDisplay(screen, character, FindGroup(((AbstractModel)character).Id.Entry)?.Id ?? ((AbstractModel)character).Id.Entry.ToLowerInvariant());
		});
		Populate(selector, group);
		Action refresh = delegate
		{
			Populate(selector, group);
			if (group != null)
			{
				ScheduleCharacterRefresh(screen, character, group.Id);
				if (IsMultiplayerCharacterSelect(screen))
				{
					MultiplayerSkinSync.OnLocalCharacterSelectionChanged(group.Id);
				}
			}
		};
		CharacterSkinCompositionControls.Show(screen, group, refresh);
		CharacterSkinBundleControls.ShowForCharacter(screen, group?.Id ?? ((AbstractModel)character).Id.Entry.ToLowerInvariant(), group?.DisplayName ?? character.Title.GetFormattedText(), delegate
		{
			Populate(selector, group);
		});
		RefreshMultiplayerSkinLoadingToggle(screen);
		if (group != null)
		{
			ScheduleCharacterRefresh(screen, character, group.Id);
			if (IsMultiplayerCharacterSelect(screen))
			{
				Callable val = Callable.From((Action)delegate
				{
					MultiplayerSkinSync.OnLocalCharacterSelectionChanged(group.Id);
				});
				((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
			}
		}
		else
		{
			CharacterModelPreviewControls.Refresh(screen, character);
		}
	}

	private static bool IsMultiplayerCharacterSelect(NCharacterSelectScreen screen)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			StartRunLobby lobby = screen.Lobby;
			return lobby != null && NetGameTypeExtensions.IsMultiplayer(lobby.NetService.Type);
		}
		catch
		{
			return false;
		}
	}

	private static void ScheduleCharacterRefresh(NCharacterSelectScreen screen, CharacterModel character, string groupId)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Variant meta = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_refresh_generation"), Variant.op_Implicit(0L));
		long generation = ((Variant)(ref meta)).AsInt64() + 1;
		((GodotObject)screen).SetMeta(StringName.op_Implicit("sts2_skin_character_refresh_generation"), Variant.op_Implicit(generation));
		Callable val = Callable.From((Action)delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen))
			{
				Variant meta2 = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_refresh_generation"), Variant.op_Implicit(0L));
				if (((Variant)(ref meta2)).AsInt64() == generation)
				{
					RunRefresh(delegate
					{
						RebuildCharacterDisplay(screen, character, groupId);
					});
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	public static void ShowMonster(NBestiary screen, NBestiaryEntry entry)
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer obj = EnsureMonsterSelector(screen);
		SetMonsterPriorityContext(obj, ResolveMonsterSkinCategory(entry));
		MonsterModel monster = (entry.IsDiscovered ? entry.Entry.monsterModel : null);
		StringName obj2 = StringName.op_Implicit("sc_workshop_target");
		MonsterModel obj3 = monster;
		((GodotObject)obj).SetMeta(obj2, Variant.op_Implicit(((obj3 != null) ? ((AbstractModel)obj3).Id.Entry.ToLowerInvariant() : null) ?? string.Empty));
		SkinGroup skinGroup = ((monster == null) ? null : FindGroup(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name));
		if (NRun.Instance != null)
		{
			IEnumerable<string> additionalGroupIds;
			if (skinGroup != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(skinGroup.Id);
				additionalGroupIds = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				additionalGroupIds = enumerable;
			}
			CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(additionalGroupIds, "局内怪物图鉴", null, null, refreshCurrentRoom: true);
		}
		else
		{
			IEnumerable<string> groupIds;
			if (skinGroup != null)
			{
				IEnumerable<string> enumerable = new _003C_003Ez__ReadOnlySingleElementList<string>(skinGroup.Id);
				groupIds = enumerable;
			}
			else
			{
				IEnumerable<string> enumerable = Array.Empty<string>();
				groupIds = enumerable;
			}
			SkinService.FocusRuntimeProviderBehaviorsOnGroups(groupIds, (IReadOnlyCollection<string>)(object)Array.Empty<string>(), "怪物图鉴");
		}
		RegisterRefresh(obj, (monster == null) ? null : ((Action)delegate
		{
			RebuildMonsterDisplay(screen, entry, monster, FindGroup(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name)?.Id ?? ((AbstractModel)monster).Id.Entry.ToLowerInvariant());
		}));
		Populate(obj, skinGroup);
	}

	private static HBoxContainer EnsureCharacterSelector(NCharacterSelectScreen screen)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("InfoPanel"));
		if (nodeOrNull == null)
		{
			ModLog.Error("选角界面缺少 InfoPanel 节点，无法挂载皮肤选择器。");
			return new HBoxContainer();
		}
		HBoxContainer val = FindCharacterSelector(screen);
		if (val != null)
		{
			AttachCharacterSelectorDragging(screen, nodeOrNull, val);
			EnsureMultiplayerSkinLoadingToggle(screen, nodeOrNull);
			return val;
		}
		HBoxContainer selector = BuildSelector();
		((Node)screen).AddChild((Node)(object)selector, false, (InternalMode)0);
		AttachCharacterSelectorDragging(screen, nodeOrNull, selector);
		EnsureMultiplayerSkinLoadingToggle(screen, nodeOrNull);
		ModLocalization.Bind((Node)(object)selector, delegate
		{
			RefreshLocalizedText(selector);
		});
		return selector;
	}

	private static HBoxContainer? FindCharacterSelector(NCharacterSelectScreen screen)
	{
		return ((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("STS2SkinSelector")) ?? ((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("InfoPanel/STS2SkinSelector"));
	}

	private static void AttachCharacterSelectorDragging(NCharacterSelectScreen screen, Control infoPanel, HBoxContainer selector)
	{
		DraggableSkinControl.Attach((Control)(object)screen, selector, mergeButton: false, delegate
		{
			ApplyCharacterSelectorPlacement(screen, infoPanel, selector);
		});
	}

	private static void ApplyCharacterSelectorPlacement(NCharacterSelectScreen screen, Control infoPanel, HBoxContainer selector)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		CharacterSelectorPlacement characterSelectorPlacement = CharacterSelectorPlacementPolicy.Resolve(SkinService.ShouldPlaceCharacterSelectorTopRight());
		if (characterSelectorPlacement.Host == CharacterSelectorHost.InfoPanel)
		{
			Transform2D globalTransformWithCanvas = ((CanvasItem)screen).GetGlobalTransformWithCanvas();
			Transform2D val = ((Transform2D)(ref globalTransformWithCanvas)).AffineInverse() * ((CanvasItem)infoPanel).GetGlobalTransformWithCanvas();
			Vector2 val2 = val * new Vector2(infoPanel.Size.X * characterSelectorPlacement.AnchorLeft + characterSelectorPlacement.OffsetLeft, infoPanel.Size.Y * characterSelectorPlacement.AnchorTop + characterSelectorPlacement.OffsetTop);
			Vector2 val3 = val * new Vector2(infoPanel.Size.X * characterSelectorPlacement.AnchorRight + characterSelectorPlacement.OffsetRight, infoPanel.Size.Y * characterSelectorPlacement.AnchorBottom + characterSelectorPlacement.OffsetBottom);
			float num2 = (((Control)selector).AnchorBottom = 0f);
			float num4 = (((Control)selector).AnchorRight = num2);
			float anchorLeft = (((Control)selector).AnchorTop = num4);
			((Control)selector).AnchorLeft = anchorLeft;
			((Control)selector).OffsetLeft = val2.X;
			((Control)selector).OffsetTop = val2.Y;
			((Control)selector).OffsetRight = val3.X;
			((Control)selector).OffsetBottom = val3.Y;
			((CanvasItem)selector).ZIndex = 20;
		}
		else
		{
			((Control)selector).AnchorLeft = characterSelectorPlacement.AnchorLeft;
			((Control)selector).AnchorTop = characterSelectorPlacement.AnchorTop;
			((Control)selector).AnchorRight = characterSelectorPlacement.AnchorRight;
			((Control)selector).AnchorBottom = characterSelectorPlacement.AnchorBottom;
			((Control)selector).OffsetLeft = characterSelectorPlacement.OffsetLeft;
			((Control)selector).OffsetTop = characterSelectorPlacement.OffsetTop;
			((Control)selector).OffsetRight = characterSelectorPlacement.OffsetRight;
			((Control)selector).OffsetBottom = characterSelectorPlacement.OffsetBottom;
			((CanvasItem)selector).ZIndex = ((characterSelectorPlacement.Host == CharacterSelectorHost.Screen) ? 20 : 0);
		}
	}

	private static void EnsureMultiplayerSkinLoadingToggle(NCharacterSelectScreen screen, Control infoPanel)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Expected O, but got Unknown
		EnsureMultiplayerSkinSyncToggle(screen, infoPanel);
		CheckButton toggle = ((Node)infoPanel).GetNodeOrNull<CheckButton>(NodePath.op_Implicit("MultiplayerSkinLoadingToggle"));
		if (!IsMultiplayerCharacterSelect(screen))
		{
			if (toggle != null)
			{
				((CanvasItem)toggle).Visible = false;
			}
			return;
		}
		if (toggle == null)
		{
			toggle = new CheckButton
			{
				Name = StringName.op_Implicit("MultiplayerSkinLoadingToggle"),
				AnchorLeft = 0.5f,
				AnchorTop = 0f,
				AnchorRight = 0.5f,
				AnchorBottom = 0f,
				OffsetLeft = -190f,
				OffsetTop = -128f,
				OffsetRight = 190f,
				OffsetBottom = -84f,
				Alignment = (HorizontalAlignment)1,
				MouseDefaultCursorShape = (CursorShape)2,
				TextOverrunBehavior = (OverrunBehavior)3
			};
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), Colors.White);
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), new Color("efc850"));
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
			((Control)toggle).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 3);
			((Control)toggle).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			if (GameFont != null)
			{
				((Control)toggle).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
			}
			((BaseButton)toggle).SetPressedNoSignal(SkinService.ShouldReceiveMultiplayerSkinChanges());
			CheckButton obj = toggle;
			object obj2 = _003C_003EO._003C0_003E__SetReceiveMultiplayerSkinChanges;
			if (obj2 == null)
			{
				ToggledEventHandler val = SkinService.SetReceiveMultiplayerSkinChanges;
				_003C_003EO._003C0_003E__SetReceiveMultiplayerSkinChanges = val;
				obj2 = (object)val;
			}
			((BaseButton)obj).Toggled += (ToggledEventHandler)obj2;
			((Node)infoPanel).AddChild((Node)(object)toggle, false, (InternalMode)0);
			ModLocalization.Bind((Node)(object)toggle, delegate
			{
				((Button)toggle).Text = ModLocalization.Get(ModText.LoadOtherPlayersCustomSkins);
				((BaseButton)toggle).SetPressedNoSignal(SkinService.ShouldReceiveMultiplayerSkinChanges());
			});
		}
		((CanvasItem)toggle).Visible = true;
		((BaseButton)toggle).Disabled = false;
		((BaseButton)toggle).SetPressedNoSignal(SkinService.ShouldReceiveMultiplayerSkinChanges());
	}

	private static void EnsureMultiplayerSkinSyncToggle(NCharacterSelectScreen screen, Control infoPanel)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Expected O, but got Unknown
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Expected O, but got Unknown
		CheckButton toggle = ((Node)infoPanel).GetNodeOrNull<CheckButton>(NodePath.op_Implicit("MultiplayerSkinSyncToggle"));
		if (!IsMultiplayerCharacterSelect(screen))
		{
			if (toggle != null)
			{
				((CanvasItem)toggle).Visible = false;
			}
			return;
		}
		if (toggle == null)
		{
			toggle = new CheckButton
			{
				Name = StringName.op_Implicit("MultiplayerSkinSyncToggle"),
				AnchorLeft = 0.5f,
				AnchorTop = 0f,
				AnchorRight = 0.5f,
				AnchorBottom = 0f,
				OffsetLeft = -190f,
				OffsetTop = -176f,
				OffsetRight = 190f,
				OffsetBottom = -132f,
				Alignment = (HorizontalAlignment)1,
				MouseDefaultCursorShape = (CursorShape)2,
				TextOverrunBehavior = (OverrunBehavior)3
			};
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), Colors.White);
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), new Color("efc850"));
			((Control)toggle).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
			((Control)toggle).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 3);
			((Control)toggle).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			if (GameFont != null)
			{
				((Control)toggle).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
			}
			((BaseButton)toggle).SetPressedNoSignal(SkinService.ShouldSendMultiplayerSkinChanges());
			((BaseButton)toggle).Toggled += (ToggledEventHandler)delegate(bool enabled)
			{
				SkinService.SetSendMultiplayerSkinChanges(enabled);
				RefreshMultiplayerSkinLoadingToggle(screen);
			};
			((Node)infoPanel).AddChild((Node)(object)toggle, false, (InternalMode)0);
			ModLocalization.Bind((Node)(object)toggle, delegate
			{
				((Button)toggle).Text = ModLocalization.Get(ModText.MultiplayerSkinSync);
				((BaseButton)toggle).SetPressedNoSignal(SkinService.ShouldSendMultiplayerSkinChanges());
			});
		}
		((CanvasItem)toggle).Visible = true;
		((BaseButton)toggle).SetPressedNoSignal(SkinService.ShouldSendMultiplayerSkinChanges());
	}

	private static void RefreshMultiplayerSkinLoadingToggle(NCharacterSelectScreen screen)
	{
		bool visible = IsMultiplayerCharacterSelect(screen);
		bool pressedNoSignal = SkinService.ShouldSendMultiplayerSkinChanges();
		CheckButton nodeOrNull = ((Node)screen).GetNodeOrNull<CheckButton>(NodePath.op_Implicit("InfoPanel/MultiplayerSkinSyncToggle"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = visible;
			((BaseButton)nodeOrNull).SetPressedNoSignal(pressedNoSignal);
		}
		CheckButton nodeOrNull2 = ((Node)screen).GetNodeOrNull<CheckButton>(NodePath.op_Implicit("InfoPanel/MultiplayerSkinLoadingToggle"));
		if (nodeOrNull2 != null)
		{
			((CanvasItem)nodeOrNull2).Visible = visible;
			((BaseButton)nodeOrNull2).Disabled = false;
			((BaseButton)nodeOrNull2).SetPressedNoSignal(SkinService.ShouldReceiveMultiplayerSkinChanges());
		}
	}

	private static HBoxContainer EnsureMonsterSelector(NBestiary screen)
	{
		HBoxContainer nodeOrNull = ((Node)screen).GetNodeOrNull<HBoxContainer>(NodePath.op_Implicit("STS2SkinSelector"));
		if (nodeOrNull != null)
		{
			return nodeOrNull;
		}
		HBoxContainer selector = BuildSelector();
		AttachMonsterPriorityControls(screen, selector);
		AddMonsterScaleControls(screen, selector);
		((Control)selector).AnchorLeft = 0.5f;
		((Control)selector).AnchorRight = 0.5f;
		((Control)selector).OffsetLeft = -350f;
		((Control)selector).OffsetTop = 168f;
		((Control)selector).OffsetRight = 350f;
		((Control)selector).OffsetBottom = 212f;
		((Node)screen).AddChild((Node)(object)selector, false, (InternalMode)0);
		ModLocalization.Bind((Node)(object)selector, delegate
		{
			RefreshLocalizedText(selector);
		});
		return selector;
	}

	private static void AddMonsterScaleControls(NBestiary screen, HBoxContainer selector)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Expected O, but got Unknown
		Label val = BuildCompactLabel(ModLocalization.Get(ModText.MonsterSize), 60f);
		((Node)val).Name = StringName.op_Implicit("MonsterScaleLabel");
		HSlider slider = new HSlider
		{
			Name = StringName.op_Implicit("MonsterScaleSlider"),
			MinValue = 0.20000000298023224,
			MaxValue = 5.0,
			Step = 0.05000000074505806,
			Value = 1.0,
			CustomMinimumSize = new Vector2(160f, 36f),
			SizeFlagsVertical = (SizeFlags)4
		};
		Label val2 = BuildCompactLabel("100%", 62f);
		((Node)val2).Name = StringName.op_Implicit("MonsterScaleValue");
		Button val3 = new Button
		{
			Name = StringName.op_Implicit("MonsterScaleReset"),
			Text = ModLocalization.Get(ModText.Reset),
			CustomMinimumSize = new Vector2(108f, 38f),
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2
		};
		ApplyCompactButtonTheme(val3);
		((Range)slider).ValueChanged += (ValueChangedEventHandler)delegate(double value)
		{
			OnMonsterScaleChanged(screen, selector, (float)value);
		};
		((BaseButton)val3).Pressed += delegate
		{
			((Range)slider).Value = 1.0;
		};
		((Node)selector).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)selector).AddChild((Node)(object)slider, false, (InternalMode)0);
		((Node)selector).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)selector).AddChild((Node)(object)val3, false, (InternalMode)0);
	}

	private static void RefreshLocalizedText(HBoxContainer selector)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
		string text = ((Variant)(ref meta)).AsString();
		SkinGroup skinGroup = (string.IsNullOrWhiteSpace(text) ? null : FindGroup(text));
		if (skinGroup != null)
		{
			Populate(selector, skinGroup);
		}
		Label nodeOrNull = ((Node)selector).GetNodeOrNull<Label>(NodePath.op_Implicit("MonsterScaleLabel"));
		if (nodeOrNull != null)
		{
			nodeOrNull.Text = ModLocalization.Get(ModText.MonsterSize);
		}
		Button nodeOrNull2 = ((Node)selector).GetNodeOrNull<Button>(NodePath.op_Implicit("MonsterScaleReset"));
		if (nodeOrNull2 != null)
		{
			nodeOrNull2.Text = ModLocalization.Get(ModText.Reset);
		}
		RefreshMonsterPriorityButton(selector);
	}

	private static Label BuildCompactLabel(string text, float width)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Label val = new Label
		{
			Text = text,
			CustomMinimumSize = new Vector2(width, 38f),
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		if (GameFont != null)
		{
			((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
		}
		return val;
	}

	private static void ApplyCompactButtonTheme(Button button)
	{
		if (GameFont != null)
		{
			((Control)button).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
		}
		ModThemeRuntime.Button(button, 19);
	}

	private static HBoxContainer BuildSelector()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		HBoxContainer selector = new HBoxContainer
		{
			Name = StringName.op_Implicit("STS2SkinSelector"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0
		};
		OptionButton dropdown = new OptionButton
		{
			Name = StringName.op_Implicit("SkinDropdown"),
			CustomMinimumSize = new Vector2(244f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1
		};
		ApplyGameTheme(dropdown);
		dropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			ApplyDropdownSelection(selector, dropdown, checked((int)index));
		};
		((Node)selector).AddChild((Node)(object)dropdown, false, (InternalMode)0);
		((Node)selector).TreeExited += delegate
		{
			ulong instanceId = ((GodotObject)selector).GetInstanceId();
			RefreshActions.Remove(instanceId);
		};
		return selector;
	}

	internal static void ApplyGameTheme(OptionButton dropdown)
	{
		Font gameFont = GameFont;
		if (gameFont != null)
		{
			((Control)dropdown).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
			((Window)dropdown.GetPopup()).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModThemeRuntime.Button((Button)(object)dropdown, 23);
		ModThemeRuntime.Popup(dropdown.GetPopup());
	}

	internal static void ApplyGameTheme(Button button)
	{
		Font gameFont = GameFont;
		if (gameFont != null)
		{
			((Control)button).AddThemeFontOverride(StringName.op_Implicit("font"), gameFont);
		}
		ModThemeRuntime.Button(button, 21);
	}

	internal static void HideCharacterSelector(NCharacterSelectScreen screen)
	{
		CharacterModelPreviewControls.Hide(screen);
		CharacterSkinCompositionControls.Hide(screen);
		CharacterSkinBundleControls.Hide(screen);
		HBoxContainer val = FindCharacterSelector(screen);
		if (val != null)
		{
			((CanvasItem)val).Visible = false;
		}
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("InfoPanel/MultiplayerSkinLoadingToggle"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = false;
		}
		Control nodeOrNull2 = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("InfoPanel/MultiplayerSkinSyncToggle"));
		if (nodeOrNull2 != null)
		{
			((CanvasItem)nodeOrNull2).Visible = false;
		}
	}

	internal static bool ShouldIgnoreBackgroundMute(NMuteInBackgroundHandler handler, int notification)
	{
		if (notification == 1005)
		{
			return ((Node)handler).GetWindow().HasFocus();
		}
		return false;
	}

	internal static StyleBoxFlat CreateStyleBox(Color background, Color border, int borderWidth = 1)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		return new StyleBoxFlat
		{
			BgColor = background,
			BorderColor = border,
			BorderWidthLeft = borderWidth,
			BorderWidthTop = borderWidth,
			BorderWidthRight = borderWidth,
			BorderWidthBottom = borderWidth,
			CornerRadiusTopLeft = 12,
			CornerRadiusTopRight = 12,
			CornerRadiusBottomRight = 12,
			CornerRadiusBottomLeft = 12,
			ContentMarginLeft = 12f,
			ContentMarginRight = 12f
		};
	}

	private static void Populate(HBoxContainer selector, SkinGroup? group)
	{
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0370: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_042c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0475: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		OptionButton dropdown = ((Node)selector).GetNode<OptionButton>(NodePath.op_Implicit("SkinDropdown"));
		if (group != null)
		{
			group = SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup g) => g.Id == group.Id) ?? group;
		}
		((Window)dropdown.GetPopup()).Hide();
		NCharacterSelectScreen val = FindAncestor<NCharacterSelectScreen>((Node)(object)selector);
		object obj;
		if (group != null)
		{
			SkinCatalog? catalog = SkinService.Catalog;
			obj = ((catalog != null && catalog.IsCharacterAppearanceGroup(group.Id)) ? ((object)SkinService.GetCharacterSkinOptions(group.Id).ToArray()) : ((object)group.Options.ToArray()));
		}
		else
		{
			obj = Array.Empty<SkinOption>();
		}
		SkinOption[] array = (SkinOption[])obj;
		IReadOnlyList<CharacterSkinBundle> readOnlyList2;
		if (group == null || val == null || HasMonsterPriorityContext(selector))
		{
			IReadOnlyList<CharacterSkinBundle> readOnlyList = Array.Empty<CharacterSkinBundle>();
			readOnlyList2 = readOnlyList;
		}
		else
		{
			readOnlyList2 = SkinService.GetCharacterSkinBundles(group.Id);
		}
		IReadOnlyList<CharacterSkinBundle> readOnlyList3 = readOnlyList2;
		bool flag = group != null && val != null && !HasMonsterPriorityContext(selector);
		SkinOption[] array3;
		if (group != null && val != null)
		{
			SkinOption currentVariant = group.Options.FirstOrDefault((SkinOption option) => option.ManualCharacterVariant != null && !option.IsComposition && option.Id.Equals(SkinService.GetCharacterSelectionOptionId(group.Id), StringComparison.OrdinalIgnoreCase));
			if ((object)currentVariant != null && array.All((SkinOption option) => option.Id != currentVariant.Id))
			{
				SkinOption[] array2 = array;
				int num = 0;
				array3 = new SkinOption[1 + array2.Length];
				ReadOnlySpan<SkinOption> readOnlySpan = new ReadOnlySpan<SkinOption>(array2);
				readOnlySpan.CopyTo(new Span<SkinOption>(array3).Slice(num, readOnlySpan.Length));
				num += readOnlySpan.Length;
				array3[num] = currentVariant;
				array = array3;
			}
		}
		IReadOnlyList<SkinPriorityState> states = null;
		if (group != null)
		{
			(IReadOnlyList<SkinOption> Options, IReadOnlyList<SkinPriorityState>? States) skinListForDisplay = SkinService.GetSkinListForDisplay(group.Id, array);
			array = skinListForDisplay.Options.ToArray();
			states = skinListForDisplay.States;
		}
		if (group == null)
		{
			dropdown.Clear();
			SkinChoiceDimming.Bind(dropdown, null);
			Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sc_workshop_target"), Variant.op_Implicit(string.Empty));
			string text = ((Variant)(ref meta)).AsString();
			((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(text));
			dropdown.AddItem(ModLocalization.Get(ModText.GameDefault), -1);
			dropdown.SetItemMetadata(0, Variant.op_Implicit("__base__"));
			SkinWorkshopEntry.Append(dropdown, characterPopup: true);
			dropdown.Select(0);
			((CanvasItem)selector).Visible = text.Length > 0;
			ConfigureCharacterBundlePopupList(selector, dropdown, 0);
			return;
		}
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(true));
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(group.Id));
		dropdown.Clear();
		SkinChoiceDimming.Bind(dropdown, states);
		bool flag2 = HasMonsterPriorityContext(selector);
		if (flag2)
		{
			dropdown.AddItem(ModLocalization.Get(ModText.FollowCategory), -1);
			dropdown.SetItemMetadata(0, Variant.op_Implicit("__monster_category__"));
		}
		foreach (CharacterSkinBundle item in readOnlyList3)
		{
			int itemCount = dropdown.ItemCount;
			dropdown.AddItem(CharacterSkinBundlePolicy.CreateSelectionDisplayName(item.Name), -1);
			dropdown.SetItemMetadata(itemCount, Variant.op_Implicit(CharacterSkinBundlePolicy.CreateSelectionOptionId(item.Name)));
		}
		int itemCount2 = dropdown.ItemCount;
		dropdown.AddItem(ModLocalization.Get(ModText.GameDefault), -1);
		dropdown.SetItemMetadata(itemCount2, Variant.op_Implicit("__base__"));
		array3 = array;
		foreach (SkinOption skinOption in array3)
		{
			int itemCount3 = dropdown.ItemCount;
			dropdown.AddItem(CharacterSkinCompositionPolicy.ResolveDisplayName(skinOption.Name, skinOption.IsComposition, ModLocalization.DisplayOptionName), -1);
			dropdown.SetItemMetadata(itemCount3, Variant.op_Implicit(skinOption.Id));
		}
		if (flag)
		{
			int itemCount4 = dropdown.ItemCount;
			dropdown.AddItem(ModLocalization.Get(ModText.RandomCharacterSkin), -1);
			dropdown.SetItemMetadata(itemCount4, Variant.op_Implicit("__random_character_skin__"));
		}
		SkinWorkshopEntry.Append(dropdown, characterPopup: true);
		ConfigureCharacterBundlePopupList(selector, dropdown, readOnlyList3.Count + (flag ? 1 : 0));
		string selected = (flag2 ? SkinService.GetMonsterOverrideSelection(group.Id) : ((val != null) ? SkinService.GetCharacterSelectionOptionId(group.Id) : SkinService.Config.GetSelection(group.Id)));
		if (dropdown.ItemCount > 0)
		{
			int num2 = Enumerable.Range(0, dropdown.ItemCount).FirstOrDefault(delegate(int index)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				Variant itemMetadata = dropdown.GetItemMetadata(index);
				return ((Variant)(ref itemMetadata)).AsString().Equals(selected, StringComparison.OrdinalIgnoreCase);
			});
			dropdown.Select(num2);
		}
		ApplyCharacterBundleSelectionTheme(dropdown, IsAccentedCharacterOption(selected));
		((CanvasItem)dropdown).Visible = true;
		PopulateMonsterScale(selector, group.Id);
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
		((CanvasItem)selector).Visible = true;
		RefreshMonsterPriorityButton(selector);
	}

	private static bool IsAccentedCharacterOption(string optionId)
	{
		return SkinOptionStylePolicy.IsAccented(optionId);
	}

	private static void ApplyCharacterBundleSelectionTheme(OptionButton dropdown, bool selectedBundle)
	{
		ApplyGameTheme(dropdown);
		if (selectedBundle)
		{
			ModThemeRuntime.AccentText((Control)(object)dropdown);
		}
	}

	private static void ConfigureCharacterBundlePopupList(HBoxContainer selector, OptionButton dropdown, int bundleCount)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Expected O, but got Unknown
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Expected O, but got Unknown
		PopupMenu popup = dropdown.GetPopup();
		ItemList val = ((Node)popup).GetNodeOrNull<ItemList>(NodePath.op_Implicit("STS2CharacterBundlePopupList"));
		if (bundleCount <= 0 && val == null && !Enumerable.Range(0, dropdown.ItemCount).Any(delegate(int index)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = dropdown.GetItemMetadata(index);
			return IsAccentedCharacterOption(((Variant)(ref itemMetadata)).AsString());
		}))
		{
			return;
		}
		if (val == null)
		{
			val = new ItemList
			{
				Name = StringName.op_Implicit("STS2CharacterBundlePopupList"),
				MouseFilter = (MouseFilterEnum)0,
				SelectMode = (SelectModeEnum)0,
				AllowReselect = true,
				SameColumnWidth = true,
				MaxColumns = 1,
				ZIndex = 100
			};
			((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
			((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_hovered_color"), Colors.White);
			((Control)val).AddThemeColorOverride(StringName.op_Implicit("font_selected_color"), Colors.White);
			((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 22);
			((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateStyleBox(new Color("45104e"), new Color("79547e"), 2));
			((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("hovered"), (StyleBox)(object)CreateStyleBox(new Color("2c586f"), new Color("afcdde")));
			((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("selected"), (StyleBox)(object)CreateStyleBox(new Color("58205f"), new Color("efc850"), 2));
			ModThemeRuntime.ItemList(val);
			ItemList themedList = val;
			ModThemeRuntime.Bind((Node)(object)val, "bundle_colors", delegate
			{
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				ApplyCharacterBundleRowColors(dropdown, themedList, ModThemeRuntime.Accent);
				CharacterSkinRandomExclusionUi.Refresh(dropdown);
			});
			if (GameFont != null)
			{
				((Control)val).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
			}
			((Node)popup).AddChild((Node)(object)val, false, (InternalMode)0);
			ItemList popupList = val;
			popupList.ItemClicked += (ItemClickedEventHandler)delegate(long clickedIndex, Vector2 _, long button)
			{
				if (button == 1 && clickedIndex >= 0 && clickedIndex < dropdown.ItemCount)
				{
					dropdown.Select((int)clickedIndex);
					((Window)popup).Hide();
					ApplyDropdownSelection(selector, dropdown, (int)clickedIndex);
				}
			};
			popupList.ItemActivated += (ItemActivatedEventHandler)delegate(long activatedIndex)
			{
				if (activatedIndex >= 0 && activatedIndex < dropdown.ItemCount)
				{
					dropdown.Select((int)activatedIndex);
					((Window)popup).Hide();
					ApplyDropdownSelection(selector, dropdown, (int)activatedIndex);
				}
			};
			((Window)popup).AboutToPopup += delegate
			{
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				//IL_003d: Unknown result type (might be due to invalid IL or missing references)
				if (RefreshCharacterBundlePopupList(dropdown, popupList))
				{
					Callable val2 = Callable.From((Action)delegate
					{
						//IL_0032: Unknown result type (might be due to invalid IL or missing references)
						//IL_004d: Unknown result type (might be due to invalid IL or missing references)
						//IL_0052: Unknown result type (might be due to invalid IL or missing references)
						if (GodotObject.IsInstanceValid((GodotObject)(object)popupList) && ((CanvasItem)popupList).Visible && ((Window)popup).Visible)
						{
							((Control)popupList).Position = Vector2.Zero;
							((Control)popupList).Size = Vector2I.op_Implicit(((Window)popup).Size);
							((Control)popupList).GrabFocus();
						}
					});
					((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
				}
			};
		}
		CharacterSkinRandomExclusionUi.Attach(dropdown, val, delegate
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
			string text = ((Variant)(ref meta)).AsString();
			if (FindAncestor<NCharacterSelectScreen>((Node)(object)selector) != null)
			{
				SkinCatalog? catalog = SkinService.Catalog;
				if (catalog != null && catalog.IsCharacterAppearanceGroup(text))
				{
					return text;
				}
			}
			return (string?)null;
		});
		RefreshCharacterBundlePopupList(dropdown, val);
	}

	private static bool RefreshCharacterBundlePopupList(OptionButton dropdown, ItemList list)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		list.Clear();
		((CanvasItem)list).Visible = false;
		if (!Enumerable.Range(0, dropdown.ItemCount).Any(delegate(int index)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = dropdown.GetItemMetadata(index);
			return IsAccentedCharacterOption(((Variant)(ref itemMetadata)).AsString());
		}))
		{
			return false;
		}
		for (int i = 0; i < dropdown.ItemCount; i++)
		{
			Variant itemMetadata2 = dropdown.GetItemMetadata(i);
			string optionId = ((Variant)(ref itemMetadata2)).AsString();
			list.AddItem(dropdown.GetItemText(i), (Texture2D)null, true);
			list.SetItemCustomFgColor(i, SkinChoiceDimming.Row(IsAccentedCharacterOption(optionId) ? ModThemeRuntime.Accent : ModThemeRuntime.Text, dropdown, optionId));
		}
		if (dropdown.Selected >= 0 && dropdown.Selected < list.ItemCount)
		{
			list.Select(dropdown.Selected, true);
		}
		CharacterSkinRandomExclusionUi.Refresh(dropdown);
		((CanvasItem)list).Visible = true;
		return true;
	}

	private static void ApplyCharacterBundleRowColors(OptionButton dropdown, ItemList list, Color accent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < Math.Min(list.ItemCount, dropdown.ItemCount); i++)
		{
			Variant itemMetadata = dropdown.GetItemMetadata(i);
			string optionId = ((Variant)(ref itemMetadata)).AsString();
			list.SetItemCustomFgColor(i, SkinChoiceDimming.Row(IsAccentedCharacterOption(optionId) ? accent : ModThemeRuntime.Text, dropdown, optionId));
		}
	}

	private static void PopulateMonsterScale(HBoxContainer selector, string groupId)
	{
		HSlider nodeOrNull = ((Node)selector).GetNodeOrNull<HSlider>(NodePath.op_Implicit("MonsterScaleSlider"));
		Label nodeOrNull2 = ((Node)selector).GetNodeOrNull<Label>(NodePath.op_Implicit("MonsterScaleValue"));
		if (nodeOrNull != null && nodeOrNull2 != null)
		{
			float selectedMonsterScale = SkinService.GetSelectedMonsterScale(groupId);
			((Range)nodeOrNull).Value = selectedMonsterScale;
			nodeOrNull2.Text = $"{Mathf.RoundToInt(selectedMonsterScale * 100f)}%";
		}
	}

	private static void OnMonsterScaleChanged(NBestiary screen, HBoxContainer selector, float scale)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		Label nodeOrNull = ((Node)selector).GetNodeOrNull<Label>(NodePath.op_Implicit("MonsterScaleValue"));
		if (nodeOrNull != null)
		{
			nodeOrNull.Text = $"{Mathf.RoundToInt(scale * 100f)}%";
		}
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
		if (((Variant)(ref meta)).AsBool())
		{
			return;
		}
		meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
		string text = ((Variant)(ref meta)).AsString();
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		try
		{
			SkinService.SetSelectedMonsterScale(text, scale);
			ApplyMonsterScaleToDescendants((Node)(object)screen, text);
		}
		catch (Exception ex)
		{
			ModLog.Error("保存怪物缩放失败：" + ex.Message);
		}
	}

	private static OptionButton? FindFrameworkDropdown(NCharacterSelectScreen screen, string groupId)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer val = FindCharacterSelector(screen);
		if (val != null)
		{
			Variant meta = ((GodotObject)val).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
			if (((Variant)(ref meta)).AsString().Equals(groupId, StringComparison.OrdinalIgnoreCase))
			{
				return ((Node)val).GetNodeOrNull<OptionButton>(NodePath.op_Implicit("SkinDropdown"));
			}
		}
		return null;
	}

	internal static string? GetFrameworkCycleOption(NCharacterSelectScreen screen, string groupId, string? pendingOptionId, int direction)
	{
		OptionButton dropdown = FindFrameworkDropdown(screen, groupId);
		if (dropdown == null)
		{
			return null;
		}
		string[] array = Enumerable.Range(0, dropdown.ItemCount).Select(delegate(int index)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = dropdown.GetItemMetadata(index);
			return ((Variant)(ref itemMetadata)).AsString();
		}).Where(WorkshopCatalogPolicy.IsSkinChoice)
			.ToArray();
		string current = ((pendingOptionId != null && array.Contains<string>(pendingOptionId, StringComparer.OrdinalIgnoreCase)) ? pendingOptionId : SkinService.GetCharacterSelectionOptionId(groupId));
		return SkinOptionCycle.NextOption(array, current, direction);
	}

	internal static string? GetFrameworkSelectionName(NCharacterSelectScreen screen, string groupId, string? pendingOptionId)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		OptionButton val = FindFrameworkDropdown(screen, groupId);
		if (val == null)
		{
			return null;
		}
		string value = pendingOptionId ?? SkinService.GetCharacterSelectionOptionId(groupId);
		for (int i = 0; i < val.ItemCount; i++)
		{
			Variant itemMetadata = val.GetItemMetadata(i);
			if (((Variant)(ref itemMetadata)).AsString().Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return val.GetItemText(i);
			}
		}
		return null;
	}

	internal static bool RequestFrameworkSelection(NCharacterSelectScreen screen, string groupId, string optionId)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		OptionButton val = FindFrameworkDropdown(screen, groupId);
		if (val == null)
		{
			return false;
		}
		HBoxContainer selector = (HBoxContainer)((Node)val).GetParent();
		for (int i = 0; i < val.ItemCount; i++)
		{
			Variant itemMetadata = val.GetItemMetadata(i);
			if (((Variant)(ref itemMetadata)).AsString().Equals(optionId, StringComparison.OrdinalIgnoreCase))
			{
				CancelCharacterDropdownSelection(screen);
				val.Select(i);
				ApplyDropdownSelection(selector, val, i);
				return true;
			}
		}
		return false;
	}

	internal static bool RequestManualVariantSelection(NCharacterSelectScreen screen, string groupId, string optionId)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (RequestFrameworkSelection(screen, groupId, optionId))
		{
			return true;
		}
		OptionButton val = FindFrameworkDropdown(screen, groupId);
		SkinOption skinOption = FindGroup(groupId)?.Options.FirstOrDefault((SkinOption option) => !option.IsComposition && option.Id == optionId && option.ManualCharacterVariant != null);
		if (val == null || skinOption == null)
		{
			return false;
		}
		int itemCount = val.ItemCount;
		val.AddItem(ModLocalization.DisplayOptionName(skinOption.Name), -1);
		val.SetItemMetadata(itemCount, Variant.op_Implicit(skinOption.Id));
		return RequestFrameworkSelection(screen, groupId, skinOption.Id);
	}

	internal static bool IsCharacterSelectionLoading(NCharacterSelectScreen screen)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		HBoxContainer? obj = FindCharacterSelector(screen);
		if (obj == null)
		{
			return false;
		}
		Variant meta = ((GodotObject)obj).GetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
		return ((Variant)(ref meta)).AsBool();
	}

	private static void ApplyDropdownSelection(HBoxContainer selector, OptionButton dropdown, int index)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
		if (((Variant)(ref val)).AsBool())
		{
			return;
		}
		val = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
		string groupId = ((Variant)(ref val)).AsString();
		val = dropdown.GetItemMetadata(index);
		string text = ((Variant)(ref val)).AsString();
		NCharacterSelectScreen val2 = FindAncestor<NCharacterSelectScreen>((Node)(object)selector);
		if (SkinWorkshopEntry.Open(text, (Node)(object)selector, groupId, delegate
		{
			Populate(selector, FindGroup(groupId));
		}, cards: false, (val2 != null) ? "character" : "monster"))
		{
			return;
		}
		string name;
		if (val2 != null && !HasMonsterPriorityContext(selector) && RandomCharacterSkinPolicy.IsRandom(text))
		{
			BeginCharacterDropdownSelection(val2, selector, dropdown, index, groupId, "__base__", preserveCharacterSkinBundle: false, selectRandomCharacterSkin: true);
		}
		else if (val2 != null && !HasMonsterPriorityContext(selector) && CharacterSkinBundlePolicy.TryGetSelectionBundleName(text, out name))
		{
			string characterSkinBundleCharacterOption = SkinService.GetCharacterSkinBundleCharacterOption(groupId, name);
			if (characterSkinBundleCharacterOption == null || !SkinService.SelectCharacterSkinBundle(groupId, name))
			{
				ModLog.Error("选择皮肤包失败：" + SkinService.LastError);
				Populate(selector, FindGroup(groupId));
			}
			else
			{
				ApplyCharacterBundleSelectionTheme(dropdown, selectedBundle: true);
				BeginCharacterDropdownSelection(val2, selector, dropdown, index, groupId, characterSkinBundleCharacterOption, preserveCharacterSkinBundle: true);
			}
		}
		else if (val2 != null && !HasMonsterPriorityContext(selector))
		{
			BeginCharacterDropdownSelection(val2, selector, dropdown, index, groupId, text);
		}
		else
		{
			ApplyDropdownSelectionNow(selector, dropdown, index, groupId, text);
		}
	}

	private static bool ApplyDropdownSelectionNow(HBoxContainer selector, OptionButton dropdown, int index, string groupId, string optionId, bool preserveCharacterSkinBundle = false, bool selectRandomCharacterSkin = false)
	{
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		Dictionary<string, string> previousSelections = new Dictionary<string, string>(SkinService.Config.Selections, StringComparer.OrdinalIgnoreCase);
		if (!(optionId.Equals("__monster_category__", StringComparison.OrdinalIgnoreCase) ? SkinService.FollowMonsterCategoryPriority(groupId) : SkinService.ApplySelection(groupId, optionId)))
		{
			ModLog.Error("界面切换失败：" + SkinService.LastError);
			string current = (HasMonsterPriorityContext(selector) ? SkinService.GetMonsterOverrideSelection(groupId) : SkinService.Config.GetSelection(groupId));
			int num = Enumerable.Range(0, dropdown.ItemCount).FirstOrDefault(delegate(int item)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0011: Unknown result type (might be due to invalid IL or missing references)
				Variant itemMetadata = dropdown.GetItemMetadata(item);
				return ((Variant)(ref itemMetadata)).AsString().Equals(current, StringComparison.OrdinalIgnoreCase);
			});
			dropdown.Select(num);
			return false;
		}
		if (FindAncestor<NCharacterSelectScreen>((Node)(object)selector) != null && !HasMonsterPriorityContext(selector) && !SkinService.SetRandomCharacterSkinEnabled(groupId, selectRandomCharacterSkin))
		{
			ModLog.Warn("保存选角随机皮肤状态失败：" + SkinService.LastError);
		}
		if (FindAncestor<NCharacterSelectScreen>((Node)(object)selector) != null && !HasMonsterPriorityContext(selector) && !preserveCharacterSkinBundle && !SkinService.ClearSelectedCharacterSkinBundle(groupId))
		{
			ModLog.Warn("清除选角皮肤包选择失败：" + SkinService.LastError);
		}
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(true));
		PopulateMonsterScale(selector, groupId);
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
		MultiplayerSkinSync.OnLocalCharacterSelectionChanged(groupId);
		HashSet<string> hashSet = (from key in previousSelections.Keys.Concat(SkinService.Config.Selections.Keys)
			where !string.Equals(previousSelections.GetValueOrDefault(key), SkinService.Config.Selections.GetValueOrDefault(key), StringComparison.OrdinalIgnoreCase)
			select key).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		hashSet.Add(groupId);
		CharacterAppearanceRuntime.RefreshRunMonsterSelection(hashSet, "局内怪物图鉴");
		Node parent = ((Node)selector).GetParent();
		NBestiary val = (NBestiary)(object)((parent is NBestiary) ? parent : null);
		if (val != null)
		{
			RefreshBestiaryMonsterNames(val);
		}
		if (RefreshActions.TryGetValue(((GodotObject)selector).GetInstanceId(), out Action refresh))
		{
			Callable val2 = Callable.From((Action)delegate
			{
				RunRefresh(refresh);
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
		return true;
	}

	private static void BeginCharacterDropdownSelection(NCharacterSelectScreen screen, HBoxContainer selector, OptionButton dropdown, int index, string groupId, string optionId, bool preserveCharacterSkinBundle = false, bool selectRandomCharacterSkin = false)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		string groupId2 = groupId;
		Variant val = dropdown.GetItemMetadata(index);
		FrameworkRegistryCooperation.SelectionStarting(groupId2, ((Variant)(ref val)).AsString());
		val = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_loading_generation"), Variant.op_Implicit(0L));
		long generation = ((Variant)(ref val)).AsInt64() + 1;
		((GodotObject)screen).SetMeta(StringName.op_Implicit("sts2_skin_character_loading_generation"), Variant.op_Implicit(generation));
		((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(true));
		((BaseButton)dropdown).Disabled = true;
		string optionName = dropdown.GetItemText(index);
		PanelContainer overlay = EnsureCharacterLoadingOverlay(screen, optionName);
		int progressValue = 0;
		IReadOnlyList<string> readOnlyList;
		if (!SkinService.IsSameManualVariantSource(groupId, optionId))
		{
			readOnlyList = SkinService.GetSelectionResourcePackPaths(groupId, optionId);
		}
		else
		{
			IReadOnlyList<string> readOnlyList2 = Array.Empty<string>();
			readOnlyList = readOnlyList2;
		}
		IReadOnlyList<string> resourcePackPaths = readOnlyList;
		Task warmTask = SkinService.WarmResourcePackFilesAsync(resourcePackPaths, delegate(double progress)
		{
			Interlocked.Exchange(ref progressValue, (int)Math.Round(progress * 78.0));
		});
		Action poll = null;
		poll = delegate
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen))
			{
				Variant meta = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_loading_generation"), Variant.op_Implicit(0L));
				if (((Variant)(ref meta)).AsInt64() == generation)
				{
					UpdateCharacterLoadingOverlay(overlay, optionName, Volatile.Read(in progressValue));
					if (!warmTask.IsCompleted)
					{
						ScheduleNextFrame((Node)(object)screen, poll);
					}
					else
					{
						if (warmTask.IsFaulted)
						{
							ModLog.Warn("后台预读 " + optionName + " 资源包失败，将直接继续挂载：" + warmTask.Exception?.GetBaseException().Message);
						}
						UpdateCharacterLoadingOverlay(overlay, optionName, 82);
						ScheduleNextFrame((Node)(object)screen, delegate
						{
							//IL_001f: Unknown result type (might be due to invalid IL or missing references)
							//IL_0024: Unknown result type (might be due to invalid IL or missing references)
							//IL_0029: Unknown result type (might be due to invalid IL or missing references)
							//IL_004b: Unknown result type (might be due to invalid IL or missing references)
							if (GodotObject.IsInstanceValid((GodotObject)(object)screen))
							{
								Variant meta2 = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_loading_generation"), Variant.op_Implicit(0L));
								if (((Variant)(ref meta2)).AsInt64() == generation)
								{
									((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
									bool flag = ApplyDropdownSelectionNow(selector, dropdown, index, groupId, optionId, preserveCharacterSkinBundle, selectRandomCharacterSkin);
									if (preserveCharacterSkinBundle && !flag)
									{
										SkinService.ClearSelectedCharacterSkinBundle(groupId);
									}
									Populate(selector, FindGroup(groupId));
									UpdateCharacterLoadingOverlay(overlay, optionName, 94);
									ScheduleNextFrame((Node)(object)screen, delegate
									{
										//IL_001f: Unknown result type (might be due to invalid IL or missing references)
										//IL_0024: Unknown result type (might be due to invalid IL or missing references)
										//IL_0029: Unknown result type (might be due to invalid IL or missing references)
										//IL_006a: Unknown result type (might be due to invalid IL or missing references)
										if (GodotObject.IsInstanceValid((GodotObject)(object)screen))
										{
											Variant meta3 = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_loading_generation"), Variant.op_Implicit(0L));
											if (((Variant)(ref meta3)).AsInt64() == generation)
											{
												UpdateCharacterLoadingOverlay(overlay, optionName, 100);
												((BaseButton)dropdown).Disabled = false;
												((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
												FrameworkRegistryCooperation.QueueRefreshControls();
												ScheduleNextFrame((Node)(object)screen, delegate
												{
													HideCharacterLoadingOverlay(screen, overlay);
												});
											}
										}
									});
								}
							}
						});
					}
				}
			}
		};
		ScheduleNextFrame((Node)(object)screen, poll);
	}

	private static void CancelCharacterDropdownSelection(NCharacterSelectScreen screen)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		PanelContainer nodeOrNull = ((Node)screen).GetNodeOrNull<PanelContainer>(NodePath.op_Implicit("STS2CharacterSkinLoadingOverlay"));
		if (nodeOrNull == null || !((CanvasItem)nodeOrNull).Visible)
		{
			return;
		}
		StringName obj = StringName.op_Implicit("sts2_skin_character_loading_generation");
		Variant meta = ((GodotObject)screen).GetMeta(StringName.op_Implicit("sts2_skin_character_loading_generation"), Variant.op_Implicit(0L));
		((GodotObject)screen).SetMeta(obj, Variant.op_Implicit(((Variant)(ref meta)).AsInt64() + 1));
		((CanvasItem)nodeOrNull).Visible = false;
		HBoxContainer val = FindCharacterSelector(screen);
		if (val != null)
		{
			((GodotObject)val).SetMeta(StringName.op_Implicit("sts2_skin_updating"), Variant.op_Implicit(false));
			OptionButton nodeOrNull2 = ((Node)val).GetNodeOrNull<OptionButton>(NodePath.op_Implicit("SkinDropdown"));
			if (nodeOrNull2 != null)
			{
				((BaseButton)nodeOrNull2).Disabled = false;
			}
		}
	}

	private static PanelContainer EnsureCharacterLoadingOverlay(NCharacterSelectScreen screen, string optionName)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Expected O, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Expected O, but got Unknown
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected O, but got Unknown
		PanelContainer nodeOrNull = ((Node)screen).GetNodeOrNull<PanelContainer>(NodePath.op_Implicit("STS2CharacterSkinLoadingOverlay"));
		if (nodeOrNull != null)
		{
			UpdateCharacterLoadingOverlay(nodeOrNull, optionName, 0);
			((CanvasItem)nodeOrNull).Visible = true;
			return nodeOrNull;
		}
		PanelContainer val = new PanelContainer
		{
			Name = StringName.op_Implicit("STS2CharacterSkinLoadingOverlay"),
			MouseFilter = (MouseFilterEnum)0,
			ZAsRelative = false,
			ZIndex = 500,
			AnchorLeft = 0.5f,
			AnchorTop = 0f,
			AnchorRight = 0.5f,
			AnchorBottom = 0f,
			OffsetLeft = -210f,
			OffsetTop = 34f,
			OffsetRight = 210f,
			OffsetBottom = 112f
		};
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateStyleBox(new Color("2a4058e8"), new Color("efc850"), 2));
		ModThemeRuntime.Panel(val);
		VBoxContainer val2 = new VBoxContainer
		{
			Name = StringName.op_Implicit("Content"),
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Label val3 = new Label
		{
			Name = StringName.op_Implicit("Label"),
			HorizontalAlignment = (HorizontalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val3).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		((Control)val3).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 20);
		if (GameFont != null)
		{
			((Control)val3).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
		}
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		ProgressBar val4 = new ProgressBar
		{
			Name = StringName.op_Implicit("Progress"),
			MinValue = 0.0,
			MaxValue = 100.0,
			ShowPercentage = false,
			CustomMinimumSize = new Vector2(380f, 16f),
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val4).AddThemeStyleboxOverride(StringName.op_Implicit("background"), (StyleBox)(object)CreateStyleBox(new Color("172433"), new Color("50606b")));
		((Control)val4).AddThemeStyleboxOverride(StringName.op_Implicit("fill"), (StyleBox)(object)CreateStyleBox(new Color("efc850"), new Color("fff1a8")));
		StyleBoxFlat progressFill = (StyleBoxFlat)((Control)val4).GetThemeStylebox(StringName.op_Implicit("fill"), (StringName)null);
		ModThemeRuntime.Bind((Node)(object)val4, "progress", delegate(ModThemeSettings theme)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			progressFill.BgColor = new Color(theme.AccentColor);
			progressFill.BorderColor = new Color(theme.AccentColor);
		});
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)screen).AddChild((Node)(object)val, false, (InternalMode)0);
		UpdateCharacterLoadingOverlay(val, optionName, 0);
		return val;
	}

	private static void UpdateCharacterLoadingOverlay(PanelContainer overlay, string optionName, int progress)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)overlay))
		{
			int num = Math.Clamp(progress, 0, 100);
			((Node)overlay).GetNode<Label>(NodePath.op_Implicit("Content/Label")).Text = $"{optionName}  {num}%";
			((Range)((Node)overlay).GetNode<ProgressBar>(NodePath.op_Implicit("Content/Progress"))).Value = num;
		}
	}

	private static void HideCharacterLoadingOverlay(NCharacterSelectScreen screen, PanelContainer overlay)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)screen) && GodotObject.IsInstanceValid((GodotObject)(object)overlay))
		{
			((CanvasItem)overlay).Visible = false;
		}
	}

	private static T? FindAncestor<T>(Node node) where T : Node
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			T val = (T)(object)((parent is T) ? parent : null);
			if (val != null)
			{
				return val;
			}
		}
		return default(T);
	}

	private static void ScheduleNextFrame(Node owner, Action action)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)owner) && owner.IsInsideTree())
		{
			owner.GetTree().CreateTimer(0.01, true, false, false).Timeout += action;
		}
	}

	private static void RunRefresh(Action refresh)
	{
		try
		{
			refresh();
		}
		catch (Exception ex)
		{
			ModLog.Error("重建皮肤展示失败：" + ex);
		}
	}

	private static void RegisterRefresh(HBoxContainer selector, Action? refresh)
	{
		ulong instanceId = ((GodotObject)selector).GetInstanceId();
		if (refresh == null)
		{
			RefreshActions.Remove(instanceId);
		}
		else
		{
			RefreshActions[instanceId] = refresh;
		}
	}

	private static void RebuildCharacterDisplay(NCharacterSelectScreen screen, CharacterModel character, string groupId)
	{
		ModLog.Info("开始重建选角预览：" + groupId + "。");
		CharacterModelPreviewControls.Hide(screen);
		ManagedSkinModLoader.RestoreCharacterPresentation(screen);
		RestoreCharacterBackgroundHostLayout(screen);
		RestoreCharacterInfoText(screen, character);
		if (ShouldSkipExternalRuntimeRedirect(groupId))
		{
			RebuildRuntimeProviderCharacterDisplay(screen, character);
			ReplaySelectedCharacterPresentation(screen, character, groupId);
			RefreshLocalLobbyAvatar(screen);
			CharacterModelPreviewControls.Refresh(screen, character);
			return;
		}
		string text = ((AbstractModel)character).Id.Entry.ToLowerInvariant();
		string characterSelectPath = CanonicalScenePath("screens/char_select/char_select_bg_" + text);
		string[] array = new string[2]
		{
			CanonicalImagePath("packed/character_select/char_select_" + text + ".png"),
			CanonicalImagePath("packed/character_select/char_select_" + text + "_locked.png")
		};
		string[] resourcePaths = new string[1] { characterSelectPath }.Concat(array).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		if (SkinService.GetSelectedFullRuntimeProvider(groupId) != null)
		{
			RebuildMountedFullRuntimeCharacterDisplay(screen, character, groupId, characterSelectPath, (IReadOnlyCollection<string>)(object)array);
			RefreshLocalLobbyAvatar(screen);
			CharacterModelPreviewControls.Refresh(screen, character);
			return;
		}
		SkinService.WithRuntimeResources(groupId, (IReadOnlyCollection<string>)(object)resourcePaths, delegate(IReadOnlyDictionary<string, Resource> resources)
		{
			Resource obj = resources[characterSelectPath];
			PackedScene scene = (PackedScene)(object)(((obj is PackedScene) ? obj : null) ?? throw new InvalidOperationException("角色选角资源不是场景：" + characterSelectPath));
			TakeOverCharacterSelectSceneCache(characterSelectPath, scene);
			ReplaceCharacterBackground(screen, character, scene, resources);
			RefreshCharacterButtonIcon(screen, character);
			ReplaySelectedCharacterPresentation(screen, character, groupId);
			return true;
		}, includeProviderDependencies: true);
		RefreshLocalLobbyAvatar(screen);
		CharacterModelPreviewControls.Refresh(screen, character);
		ModLog.Info("已完整重建 " + ((AbstractModel)character).Id.Entry + " 的选角展示。");
	}

	private static void RefreshLocalLobbyAvatar(NCharacterSelectScreen screen)
	{
		try
		{
			if (IsMultiplayerCharacterSelect(screen))
			{
				RefreshMultiplayerPlayerIcons(screen.Lobby.NetService.NetId);
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新本机选角头像失败：" + ex.GetBaseException().Message);
		}
	}

	internal static CharacterModel GetLocalLobbyCharacter(StartRunLobby lobby)
	{
		object obj = AccessTools.Property(typeof(StartRunLobby), "LocalPlayer")?.GetValue(lobby);
		if (obj != null)
		{
			object? obj2 = AccessTools.Field(obj.GetType(), "character")?.GetValue(obj);
			CharacterModel val = (CharacterModel)((obj2 is CharacterModel) ? obj2 : null);
			if (val != null)
			{
				return val;
			}
		}
		throw new InvalidOperationException("选角大厅中没有可读取的本机角色。");
	}

	private static void RebuildMountedFullRuntimeCharacterDisplay(NCharacterSelectScreen screen, CharacterModel character, string groupId, string scenePath, IReadOnlyCollection<string> texturePaths)
	{
		string[] resourcePaths = new string[1] { scenePath }.Concat(texturePaths).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		SkinService.WithRuntimeResources(groupId, (IReadOnlyCollection<string>)(object)resourcePaths, delegate(IReadOnlyDictionary<string, Resource> resources)
		{
			Resource obj = resources[scenePath];
			PackedScene scene = (PackedScene)(object)(((obj is PackedScene) ? obj : null) ?? throw new InvalidOperationException("完整运行时皮肤的选角资源不是场景：" + scenePath));
			TakeOverCharacterSelectSceneCache(scenePath, scene);
			ReplaceCharacterBackground(screen, character, scene, resources);
			RefreshCharacterButtonIcon(screen, character);
			ReplaySelectedCharacterPresentation(screen, character, groupId);
			return true;
		}, includeProviderDependencies: true);
		ModLog.Info("已从隔离依赖路径重建完整 DLL 皮肤 " + ((AbstractModel)character).Id.Entry + " 的选角展示。");
	}

	private static void RestoreCharacterInfoText(NCharacterSelectScreen screen, CharacterModel character)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			NCharacterSelectButton val = FindCharacterButton(screen, character);
			string text = ((val != null && val.IsLocked) ? new LocString("main_menu_ui", "CHARACTER_SELECT.locked.title").GetFormattedText() : new LocString("characters", character.CharacterSelectTitle).GetFormattedText());
			object value = CharacterNameField.GetValue(screen);
			AccessTools.Method(value?.GetType(), "SetTextAutoSize", new Type[1] { typeof(string) }, (Type[])null)?.Invoke(value, new object[1] { text });
			object? value2 = CharacterDescriptionField.GetValue(screen);
			RichTextLabel val2 = (RichTextLabel)((value2 is RichTextLabel) ? value2 : null);
			if (val2 != null)
			{
				val2.Text = ((val != null && val.IsLocked) ? character.GetUnlockText().GetFormattedText() : new LocString("characters", character.CharacterSelectDesc).GetFormattedText());
			}
			RelicModel val3 = character.StartingRelics.FirstOrDefault();
			if (val3 != null)
			{
				bool flag = val != null && val.IsLocked;
				object? value3 = CharacterRelicTitleField.GetValue(screen);
				RichTextLabel val4 = (RichTextLabel)((value3 is RichTextLabel) ? value3 : null);
				if (val4 != null)
				{
					val4.Text = (flag ? new LocString("main_menu_ui", "CHARACTER_SELECT.lockedRelic.title").GetFormattedText() : val3.Title.GetFormattedText());
				}
				object? value4 = CharacterRelicDescriptionField.GetValue(screen);
				RichTextLabel val5 = (RichTextLabel)((value4 is RichTextLabel) ? value4 : null);
				if (val5 != null)
				{
					val5.Text = (flag ? new LocString("main_menu_ui", "CHARACTER_SELECT.lockedRelic.description").GetFormattedText() : val3.DynamicDescription.GetFormattedText());
				}
				object? value5 = CharacterRelicIconField.GetValue(screen);
				TextureRect val6 = (TextureRect)((value5 is TextureRect) ? value5 : null);
				if (val6 != null)
				{
					val6.Texture = val3.Icon;
					((CanvasItem)val6).SelfModulate = (flag ? StsColors.ninetyPercentBlack : Colors.White);
				}
				object? value6 = CharacterRelicIconOutlineField.GetValue(screen);
				TextureRect val7 = (TextureRect)((value6 is TextureRect) ? value6 : null);
				if (val7 != null)
				{
					val7.Texture = val3.IconOutline;
					((CanvasItem)val7).SelfModulate = (flag ? StsColors.halfTransparentWhite : StsColors.halfTransparentBlack);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("恢复 " + ((AbstractModel)character).Id.Entry + " 的选角文字失败：" + ex.GetBaseException().Message);
		}
	}

	private static void ReplaySelectedCharacterPresentation(NCharacterSelectScreen screen, CharacterModel character, string groupId)
	{
		string providerId = SkinService.GetSelectedCreatureRuntimeProvider(groupId);
		if (providerId == null)
		{
			return;
		}
		HashSet<ulong> existingNodes = (from node in EnumerateNodes((Node)(object)screen)
			select ((GodotObject)node).GetInstanceId()).ToHashSet();
		NCharacterSelectButton val = FindCharacterButton(screen, character);
		if (val != null)
		{
			ManualCharacterVariantBridge.ApplyLocalChoice(groupId);
			ManagedSkinModLoader.ReplaySelectedCharacterPresentation(providerId, screen, val, character, delegate
			{
				StabilizeProviderCharacterSelectControls(screen, existingNodes, providerId);
			});
			ManualCharacterVariantBridge.BindScreen(screen, groupId, providerId);
		}
		Node nodeOrNull = ((Node)screen).GetNodeOrNull<Node>(NodePath.op_Implicit("AnimatedBg"));
		Node val2 = ((nodeOrNull != null && nodeOrNull.GetChildCount(false) > 0) ? nodeOrNull.GetChild(0, false) : null);
		if (val2 != null)
		{
			ManagedCharacterAnimationBridge.TryStartCharacterSelectLoops(val2, providerId);
			ManagedSlotVisibilityBridge.BindPreview(val2, groupId, providerId);
		}
	}

	private static void RemoveStaleProviderCharacterSelectControls(NCharacterSelectScreen screen)
	{
		Control[] array = (from control in EnumerateNodes((Node)(object)screen).OfType<Control>()
			where ((object)((Node)control).Name).ToString().EndsWith("CharacterSelectOptionsPanel", StringComparison.OrdinalIgnoreCase)
			select control).ToArray();
		Control[] array2 = array;
		foreach (Control val in array2)
		{
			Node parent = ((Node)val).GetParent();
			if (parent != null && GodotObject.IsInstanceValid((GodotObject)(object)parent))
			{
				GodotTreeExtensions.RemoveChildSafely(parent, (Node)(object)val);
			}
			GodotTreeExtensions.QueueFreeSafely((Node)(object)val);
		}
		if (array.Length != 0)
		{
			ModLog.Info($"已清理 {array.Length} 个上一角色遗留的选角交互面板。");
		}
	}

	private static void StabilizeProviderCharacterSelectControls(NCharacterSelectScreen screen, IReadOnlySet<ulong> existingNodes, string providerId)
	{
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		Control[] interactiveRoots = (from root in (from node in EnumerateNodes((Node)(object)screen)
				where !existingNodes.Contains(((GodotObject)node).GetInstanceId())
				select node).OfType<Control>()
			where ((object)((Node)root).Name).ToString().Contains("CharacterSelectOptionsPanel", StringComparison.OrdinalIgnoreCase) || ((Node)root).GetNodeOrNull<Node>(NodePath.op_Implicit("Options")) != null || ((Node)root).GetNodeOrNull<Node>(NodePath.op_Implicit("ButtonContainer")) != null
			select root).ToArray();
		if (interactiveRoots.Length == 0)
		{
			return;
		}
		Control[] array = interactiveRoots;
		foreach (Control control in array)
		{
			StabilizeProviderCharacterSelectControl(screen, control);
		}
		array = interactiveRoots;
		foreach (Control val in array)
		{
			Node parent = ((Node)val).GetParent();
			CanvasItem val2 = (CanvasItem)(object)((parent is CanvasItem) ? parent : null);
			bool value = val2 == null || val2.Visible;
			List<string> list = new List<string>();
			Node val3 = (Node)(object)val;
			while (val3 != null && val3 != screen)
			{
				Control val4 = (Control)(object)((val3 is Control) ? val3 : null);
				if (val4 != null)
				{
					list.Add($"{((Node)val4).Name}:v={((CanvasItem)val4).Visible},clip={val4.ClipContents},global={val4.GlobalPosition},size={val4.Size},a={((CanvasItem)val4).Modulate.A:0.###}");
				}
				val3 = val3.GetParent();
			}
			string[] value2 = (from child in ((IEnumerable)((Node)val).GetChildren(false)).OfType<CanvasItem>()
				select $"{((Node)child).Name}:v={child.Visible},a={child.Modulate.A:0.###}").ToArray();
			ModLog.Info($"选角交互面板诊断 provider={providerId} 节点={((Node)val).GetPath()} visible={((CanvasItem)val).Visible} parentVisible={value} position={val.Position} global={val.GlobalPosition} size={val.Size} z={((CanvasItem)val).ZIndex} alpha={((CanvasItem)val).Modulate.A:0.###} ancestors=[{string.Join(" | ", list)}] children=[{string.Join(" | ", value2)}]");
		}
		Callable val6 = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)screen))
			{
				Control[] array2 = interactiveRoots;
				foreach (Control val5 in array2)
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)val5) && ((Node)val5).IsInsideTree())
					{
						StabilizeProviderCharacterSelectControl(screen, val5);
					}
				}
			}
		});
		((Callable)(ref val6)).CallDeferred(Array.Empty<Variant>());
		ModLog.Info($"已保留 {providerId} 的 {interactiveRoots.Length} 个选角交互面板。");
	}

	private static void StabilizeProviderCharacterSelectControl(NCharacterSelectScreen screen, Control control)
	{
		((CanvasItem)control).Visible = true;
		((CanvasItem)control).ZAsRelative = false;
		((CanvasItem)control).ZIndex = Math.Max(((CanvasItem)control).ZIndex, 100);
		((CanvasItem)control).MoveToFront();
		control.ClipContents = false;
		ProviderToolbarCanvas.Bind(control);
	}

	private static void RebuildRuntimeProviderCharacterDisplay(NCharacterSelectScreen screen, CharacterModel character)
	{
		string characterSelectBg = character.CharacterSelectBg;
		PackedScene val = ResourceLoader.Load<PackedScene>(characterSelectBg, (string)null, (CacheMode)3);
		if (val != null)
		{
			TakeOverCharacterSelectSceneCache(characterSelectBg, val);
			PreloadManager.Cache.SetAsset(characterSelectBg, (Resource)(object)val);
		}
		else
		{
			val = PreloadManager.Cache.GetScene(characterSelectBg);
		}
		ReplaceCharacterBackground(screen, character, val);
		NCharacterSelectButton val2 = FindCharacterButton(screen, character);
		if (val2 != null)
		{
			((Node)val2).GetNode<TextureRect>(NodePath.op_Implicit("%Icon")).Texture = (Texture2D)(object)(val2.IsLocked ? character.CharacterSelectLockedIcon : character.CharacterSelectIcon);
		}
		ModLog.Info("已由 DLL 皮肤提供器重建 " + ((AbstractModel)character).Id.Entry + " 的选角展示。");
	}

	private static void TakeOverCharacterSelectSceneCache(string scenePath, PackedScene scene)
	{
		((Resource)scene).TakeOverPath(scenePath);
		PreloadManager.Cache.SetAsset(scenePath, (Resource)(object)scene);
	}

	private static void ReplaceCharacterBackground(NCharacterSelectScreen screen, CharacterModel character, PackedScene scene, IReadOnlyDictionary<string, Resource>? isolatedResources = null)
	{
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		Control container = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("AnimatedBg"));
		if (container == null)
		{
			ModLog.Error("选角界面缺少 AnimatedBg 节点，无法替换角色背景。");
			return;
		}
		((CanvasItem)container).Visible = true;
		foreach (Node child in ((Node)container).GetChildren(false))
		{
			GodotTreeExtensions.RemoveChildSafely((Node)(object)container, child);
			GodotTreeExtensions.QueueFreeSafely(child);
		}
		Control background = scene.Instantiate<Control>((GenEditState)0);
		if (isolatedResources != null)
		{
			RebindCharacterSceneResources((Node)(object)background, isolatedResources);
		}
		((Node)background).Name = StringName.op_Implicit(((AbstractModel)character).Id.Entry + "_bg");
		GodotTreeExtensions.AddChildSafely((Node)(object)container, (Node)(object)background);
		if (((Node)background).IsInsideTree())
		{
			RefreshCharacterBackgroundLayout(container, background);
		}
		Callable val = Callable.From((Action)delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)container) && GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				RefreshCharacterBackgroundLayout(container, background);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	internal static void CaptureCharacterBackgroundHostLayout(NCharacterSelectScreen screen)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)screen) && !CharacterBackgroundHostLayouts.TryGetValue(screen, out CharacterBackgroundHostLayout _))
		{
			Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("AnimatedBg"));
			if (nodeOrNull != null)
			{
				CharacterBackgroundHostLayouts.Add(screen, CharacterBackgroundHostLayout.Capture(nodeOrNull));
			}
		}
	}

	private static void RestoreCharacterBackgroundHostLayout(NCharacterSelectScreen screen)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Control nodeOrNull = ((Node)screen).GetNodeOrNull<Control>(NodePath.op_Implicit("AnimatedBg"));
		if (nodeOrNull == null)
		{
			return;
		}
		if (!CharacterBackgroundHostLayouts.TryGetValue(screen, out CharacterBackgroundHostLayout value))
		{
			CaptureCharacterBackgroundHostLayout(screen);
			return;
		}
		if (!value.Matches(nodeOrNull))
		{
			ModLog.Info($"已清除选角背景容器遗留变换：位置={nodeOrNull.Position}，缩放={nodeOrNull.Scale}，旋转={nodeOrNull.Rotation:F3}。");
		}
		value.Apply(nodeOrNull);
		((CanvasItem)nodeOrNull).Visible = true;
	}

	private static void RebindCharacterSceneResources(Node background, IReadOnlyDictionary<string, Resource> isolatedResources)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (Node item in EnumerateNodes(background))
		{
			if (((GodotObject)item).HasMethod(StringName.op_Implicit("set_skeleton_data_res")))
			{
				Variant val = ((GodotObject)item).Get(StringName.op_Implicit("skeleton_data_res"));
				GodotObject obj = ((Variant)(ref val)).AsGodotObject();
				Resource val2 = (Resource)(object)((obj is Resource) ? obj : null);
				if (val2 != null && !string.IsNullOrWhiteSpace(val2.ResourcePath) && isolatedResources.TryGetValue(val2.ResourcePath, out Resource value) && val2 != value)
				{
					((GodotObject)item).Call(StringName.op_Implicit("set_skeleton_data_res"), (Variant[])(object)new Variant[1] { Variant.op_Implicit((GodotObject)(object)value) });
					num++;
				}
			}
		}
		if (num > 0)
		{
			ModLog.Info($"选角场景已在进树前重新绑定 {num} 个隔离骨骼资源。");
		}
	}

	private static void RefreshCharacterBackgroundLayout(Control container, Control background)
	{
		if (CharacterBackgroundWindowChangeMethod == null)
		{
			return;
		}
		NCharacterSelectScreenBg val = (NCharacterSelectScreenBg)(object)((container is NCharacterSelectScreenBg) ? container : null);
		if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
		{
			try
			{
				CharacterBackgroundWindowChangeMethod.Invoke(val, null);
			}
			catch (Exception ex)
			{
				ModLog.Warn("刷新选角背景的宽高比布局失败：" + ex.GetBaseException().Message);
			}
			if (GodotObject.IsInstanceValid((GodotObject)(object)background))
			{
				((CanvasItem)background).QueueRedraw();
			}
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateNodes_003Ed__103))]
	private static IEnumerable<Node> EnumerateNodes(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateNodes_003Ed__103(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static void RefreshCharacterButtonIcon(NCharacterSelectScreen screen, CharacterModel character)
	{
		NCharacterSelectButton val = FindCharacterButton(screen, character);
		if (val == null)
		{
			return;
		}
		try
		{
			((Node)val).GetNode<TextureRect>(NodePath.op_Implicit("%Icon")).Texture = (Texture2D)(object)(val.IsLocked ? character.CharacterSelectLockedIcon : character.CharacterSelectIcon);
			ModLog.Info("已刷新 " + ((AbstractModel)character).Id.Entry + " 的角色列表头像。");
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新 " + ((AbstractModel)character).Id.Entry + " 的角色列表头像失败：" + ex.GetBaseException().Message);
		}
	}

	private static NCharacterSelectButton? FindCharacterButton(NCharacterSelectScreen screen, CharacterModel character)
	{
		return FindDescendant<NCharacterSelectButton>((Node)(object)screen, (Func<NCharacterSelectButton, bool>)delegate(NCharacterSelectButton candidate)
		{
			CharacterModel character2 = candidate.Character;
			return character2 != null && ((AbstractModel)character2).Id.Entry.Equals(((AbstractModel)character).Id.Entry, StringComparison.OrdinalIgnoreCase);
		});
	}

	private static T? FindDescendant<T>(Node root, Func<T, bool> predicate) where T : Node
	{
		foreach (Node child in root.GetChildren(false))
		{
			T val = (T)(object)((child is T) ? child : null);
			if (val != null && predicate(val))
			{
				return val;
			}
			T val2 = FindDescendant(child, predicate);
			if (val2 != null)
			{
				return val2;
			}
		}
		return default(T);
	}

	private static void RebuildMonsterDisplay(NBestiary screen, NBestiaryEntry entry, MonsterModel monster, string groupId)
	{
		string monsterVisualsPath = GetMonsterVisualsPath(monster);
		PackedScene val = SkinService.LoadRuntimeScene(groupId, monsterVisualsPath);
		PreloadManager.Cache.SetAsset(monsterVisualsPath, (Resource)(object)val);
		try
		{
			_refreshingMonsterDisplay = true;
			BestiarySelectedEntryField.SetValue(screen, null);
			BestiarySelectMonsterMethod.Invoke(screen, new object[1] { entry });
		}
		finally
		{
			_refreshingMonsterDisplay = false;
		}
		ModLog.Info("已完整重建 " + ((AbstractModel)monster).Id.Entry + " 的图鉴展示。");
	}

	internal static string GetMonsterVisualsPath(MonsterModel monster)
	{
		try
		{
			if (MonsterVisualsPathGetter.Invoke(monster, null) is string text && !string.IsNullOrWhiteSpace(text))
			{
				return text;
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("无法读取 " + ((AbstractModel)monster).Id.Entry + " 的实际 VisualsPath，将使用默认场景路径：" + ex.Message);
		}
		return CanonicalScenePath("creature_visuals/" + ((AbstractModel)monster).Id.Entry.ToLowerInvariant());
	}

	internal static SkinGroup? FindGroup(string modelId, string? modelTypeName = null)
	{
		HashSet<string> tokens = BuildModelIdentityTokens(modelId, modelTypeName);
		SkinGroup skinGroup = SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup group) => tokens.Contains(NormalizeToken(group.Id)));
		if (skinGroup != null)
		{
			return skinGroup;
		}
		return SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup group) => GroupMatchesSharedAnimationLayout(group.Id, modelId, modelTypeName));
	}

	internal static bool MatchesGroupIdentity(string groupId, string modelId, string? modelTypeName = null)
	{
		if (!BuildModelIdentityTokens(modelId, modelTypeName).Contains(NormalizeToken(groupId)))
		{
			return GroupMatchesSharedAnimationLayout(groupId, modelId, modelTypeName);
		}
		return true;
	}

	private static bool GroupMatchesSharedAnimationLayout(string groupId, string modelId, string? modelTypeName)
	{
		if (SkinCatalog.SharedAnimationModelIds.TryGetValue(groupId, out string[] value))
		{
			if (!value.Contains<string>(modelId, StringComparer.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrWhiteSpace(modelTypeName))
				{
					return value.Any((string id) => NormalizeToken(id).Equals(NormalizeToken(modelTypeName), StringComparison.OrdinalIgnoreCase));
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private static HashSet<string> BuildModelIdentityTokens(string modelId, string? modelTypeName)
	{
		return (from value in new string[2] { modelId, modelTypeName }
			where !string.IsNullOrWhiteSpace(value)
			select NormalizeToken(value)).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static string NormalizeToken(string value)
	{
		return NonAlphanumericRegex().Replace(value, string.Empty).ToLowerInvariant();
	}

	[GeneratedRegex("[^a-zA-Z0-9]")]
	[GeneratedCode("System.Text.RegularExpressions.Generator", "9.0.14.41315")]
	private static Regex NonAlphanumericRegex()
	{
		return _003CRegexGenerator_g_003EF733DAFB4646839A4981358B2C80476024D53ACC3F7CAD83B5645B0490F5E3526__NonAlphanumericRegex_56.Instance;
	}

	internal static void ReplaceCreatedVisuals(string modelId, string visualsPath, ref NCreatureVisuals result)
	{
		ReplaceCreatedVisuals(modelId, null, visualsPath, ref result);
	}

	internal static void ReplaceCreatedVisuals(string modelId, string? modelTypeName, string visualsPath, ref NCreatureVisuals result)
	{
		if (result != null && ((GodotObject)result).HasMeta(StringName.op_Implicit("sts2_skin_managed_combat_scene")))
		{
			return;
		}
		SkinGroup group = FindGroup(modelId, modelTypeName);
		if (group == null || ShouldSkipExternalRuntimeRedirect(group.Id) || SkinService.TryGetSelectedFrameworkContract(group.Id, out FrameworkCharacterSkinContract _))
		{
			return;
		}
		try
		{
			NCreatureVisuals val = SkinService.InstantiateRuntimeScene<NCreatureVisuals>(group.Id, visualsPath, (Action?)delegate
			{
				RuntimeMonsterVisualModeBridge.ApplySelected(group.Id);
			});
			int num = ManagedSceneCompatibility.CopyMissingUniqueNodes((Node?)(object)result, (Node?)(object)val);
			if (num > 0)
			{
				ModLog.Info($"已为 {modelId} 的替换场景补齐 {num} 个游戏必需节点。");
			}
			NCreatureVisuals obj = result;
			if (obj != null)
			{
				((Node)obj).QueueFree();
			}
			result = val;
		}
		catch (Exception value)
		{
			ModLog.Error($"最终应用 {modelId} 的场景皮肤失败：{value}");
		}
	}

	internal static void ReplaceCreatedCreatureVisuals(Creature creature, ref NCreatureVisuals? result)
	{
		if (result != null && MultiplayerSkinSync.GetScopedSelections() != null)
		{
			NCreatureVisuals result2 = result;
			if (creature.Player != null)
			{
				CharacterModel character = creature.Player.Character;
				ReplaceCreatedVisuals(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name, CanonicalScenePath("creature_visuals/" + ((AbstractModel)character).Id.Entry.ToLowerInvariant()), ref result2);
				ApplySelectedProviderVisualPostfix(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name, character, ref result2);
			}
			else if (creature.Monster != null)
			{
				MonsterModel monster = creature.Monster;
				ReplaceCreatedVisuals(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name, GetMonsterVisualsPath(monster), ref result2);
				ApplySelectedProviderVisualPostfix(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name, monster, ref result2);
				MarkAndApplyMonsterScale(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name, result2);
			}
			result = result2;
		}
	}

	internal static void ApplySelectedCreatureVisualPostfix(Creature creature, ref NCreatureVisuals? visuals)
	{
		if (visuals != null)
		{
			NCreatureVisuals visuals2 = visuals;
			string entry = creature.ModelId.Entry;
			Player player = creature.Player;
			ApplySelectedProviderVisualPostfix(entry, ((player != null) ? ((object)player.Character).GetType().Name : null) ?? ((object)creature.Monster)?.GetType().Name, creature, ref visuals2);
			visuals = visuals2;
		}
	}

	internal static void ApplySelectedProviderVisualPostfix(string modelId, string? modelTypeName, object model, ref NCreatureVisuals visuals)
	{
		SkinGroup skinGroup = FindGroup(modelId, modelTypeName);
		if (skinGroup == null)
		{
			return;
		}
		IDisposable disposable = null;
		if (MultiplayerSkinSync.GetScopedSelection(skinGroup.Id) != null)
		{
			object obj = ((model is MonsterModel) ? model : null);
			if (obj == null)
			{
				object obj2 = ((model is Creature) ? model : null);
				obj = ((obj2 != null) ? ((Creature)obj2).Monster : null);
			}
			MonsterModel val = (MonsterModel)obj;
			string scenePath = ((val != null) ? GetMonsterVisualsPath(val) : CanonicalScenePath("creature_visuals/" + modelId.ToLowerInvariant()));
			try
			{
				disposable = SkinService.BeginRuntimeResourceScope(skinGroup.Id, scenePath);
			}
			catch (Exception ex)
			{
				ModLog.Warn("为联机玩家挂载 " + skinGroup.Id + " 的视觉后处理资源失败：" + ex.Message);
			}
		}
		try
		{
			SkinService.ApplySelectedVisualPostfix(skinGroup.Id, model, ref visuals);
		}
		finally
		{
			disposable?.Dispose();
		}
	}

	internal static void MarkAndApplyMonsterScale(string modelId, string modelTypeName, NCreatureVisuals visuals)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		SkinGroup skinGroup = FindGroup(modelId, modelTypeName);
		if (skinGroup != null)
		{
			((GodotObject)visuals).SetMeta(StringName.op_Implicit("sts2_skin_monster_scale_group"), Variant.op_Implicit(skinGroup.Id));
			CaptureMonsterBaseScale(visuals);
			ApplyStoredMonsterScale(visuals, skinGroup.Id);
		}
	}

	internal static void ReapplyMonsterScaleAfterGameScale(NCreatureVisuals visuals)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (((GodotObject)visuals).HasMeta(StringName.op_Implicit("sts2_skin_monster_scale_group")))
		{
			StringName obj = StringName.op_Implicit("sts2_skin_monster_scale_group");
			Variant val = default(Variant);
			val = ((GodotObject)visuals).GetMeta(obj, val);
			string groupId = ((Variant)(ref val)).AsString();
			CaptureMonsterBaseScale(visuals);
			ApplyStoredMonsterScale(visuals, groupId);
		}
	}

	internal static float GetAppliedMonsterScaleFactor(NCreatureVisuals visuals)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!((GodotObject)visuals).HasMeta(StringName.op_Implicit("sts2_skin_monster_scale_group")))
		{
			return 1f;
		}
		Variant meta = ((GodotObject)visuals).GetMeta(StringName.op_Implicit("sts2_skin_monster_applied_scale"), Variant.op_Implicit(1f));
		return ((Variant)(ref meta)).AsSingle();
	}

	internal static bool IsMonsterScaleManaged(NCreatureVisuals visuals)
	{
		return ((GodotObject)visuals).HasMeta(StringName.op_Implicit("sts2_skin_monster_scale_group"));
	}

	internal static void ApplyMonsterScalePreview(NCreatureVisuals visuals, float factor)
	{
		if (((GodotObject)visuals).HasMeta(StringName.op_Implicit("sts2_skin_monster_scale_group")))
		{
			float factor2 = Mathf.Clamp(Mathf.Round(factor / 0.05f) * 0.05f, 0.2f, 5f);
			ApplyMonsterScaleFactor(visuals, factor2);
		}
	}

	private static void CaptureMonsterBaseScale(NCreatureVisuals visuals)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)visuals).SetMeta(StringName.op_Implicit("sts2_skin_monster_base_scale"), Variant.op_Implicit(((Node2D)visuals).Scale));
		((GodotObject)visuals).SetMeta(StringName.op_Implicit("sts2_skin_monster_base_default_scale"), Variant.op_Implicit(visuals.DefaultScale));
	}

	private static void ApplyStoredMonsterScale(NCreatureVisuals visuals, string groupId)
	{
		ApplyMonsterScaleFactor(visuals, SkinService.GetSelectedMonsterScale(groupId));
	}

	private static void ApplyMonsterScaleFactor(NCreatureVisuals visuals, float factor)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		Variant meta = ((GodotObject)visuals).GetMeta(StringName.op_Implicit("sts2_skin_monster_base_scale"), Variant.op_Implicit(((Node2D)visuals).Scale));
		Vector2 val = ((Variant)(ref meta)).AsVector2();
		meta = ((GodotObject)visuals).GetMeta(StringName.op_Implicit("sts2_skin_monster_base_default_scale"), Variant.op_Implicit(visuals.DefaultScale));
		float num = ((Variant)(ref meta)).AsSingle();
		((Node2D)visuals).Scale = val * factor;
		visuals.DefaultScale = num * factor;
		((GodotObject)visuals).SetMeta(StringName.op_Implicit("sts2_skin_monster_applied_scale"), Variant.op_Implicit(factor));
	}

	private static void ApplyMonsterScaleToDescendants(Node root, string groupId)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		NCreatureVisuals val = (NCreatureVisuals)(object)((root is NCreatureVisuals) ? root : null);
		if (val != null)
		{
			Variant meta = ((GodotObject)val).GetMeta(StringName.op_Implicit("sts2_skin_monster_scale_group"), Variant.op_Implicit(string.Empty));
			if (((Variant)(ref meta)).AsString().Equals(groupId, StringComparison.OrdinalIgnoreCase))
			{
				ApplyStoredMonsterScale(val, groupId);
			}
		}
		foreach (Node child in root.GetChildren(false))
		{
			ApplyMonsterScaleToDescendants(child, groupId);
		}
	}

	internal static void ReplaceCachedScene(string resourcePath, ref PackedScene result)
	{
		string text = SkinService.Catalog?.FindGroupIdForResourcePath(resourcePath);
		if (text == null || IsCreatureVisualScenePath(resourcePath) || IsCharacterSelectBackgroundPath(resourcePath) || ShouldSkipExternalRuntimeRedirect(text))
		{
			return;
		}
		try
		{
			result = SkinService.GetOrLoadRuntimeScene(text, resourcePath);
		}
		catch (Exception value)
		{
			ModLog.Error($"最终接管场景 {resourcePath} 失败：{value}");
		}
	}

	private static bool IsCharacterSelectBackgroundPath(string resourcePath)
	{
		return Path.GetFileNameWithoutExtension(resourcePath).StartsWith("char_select_bg_", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsCreatureVisualScenePath(string resourcePath)
	{
		return resourcePath.Contains("/creature_visuals/", StringComparison.OrdinalIgnoreCase);
	}

	internal static void ReplaceCachedTexture(string resourcePath, ref Texture2D result)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		string text = SkinService.Catalog?.FindGroupIdForResourcePath(resourcePath);
		if (text == null || ShouldSkipExternalRuntimeRedirect(text))
		{
			return;
		}
		try
		{
			Resource orLoadRuntimeResource = SkinService.GetOrLoadRuntimeResource(text, resourcePath);
			result = (Texture2D)(object)(((orLoadRuntimeResource is Texture2D) ? orLoadRuntimeResource : null) ?? throw new InvalidOperationException("独立皮肤资源不是贴图：" + resourcePath));
			if (EventSkinPolicy.IsEventGroup(text))
			{
				((GodotObject)result).SetMeta(StringName.op_Implicit("sc_event_texture_source"), Variant.op_Implicit(resourcePath));
			}
		}
		catch (Exception value)
		{
			ModLog.Error($"最终接管贴图 {resourcePath} 失败：{value}");
		}
	}

	internal static void ReplaceCharacterSelectTexture(CharacterModel character, bool locked, ref CompressedTexture2D result)
	{
		SkinGroup skinGroup = FindGroup(((AbstractModel)character).Id.Entry);
		if (skinGroup == null)
		{
			return;
		}
		string text = ((AbstractModel)character).Id.Entry.ToLowerInvariant();
		string text2 = CanonicalImagePath("packed/character_select/char_select_" + text + (locked ? "_locked.png" : ".png"));
		if (SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(skinGroup.Id, text2))
		{
			return;
		}
		try
		{
			Resource orLoadCharacterIconResource = SkinService.GetOrLoadCharacterIconResource(skinGroup.Id, text2);
			result = (CompressedTexture2D)(object)(((orLoadCharacterIconResource is CompressedTexture2D) ? orLoadCharacterIconResource : null) ?? throw new InvalidOperationException("角色头像资源不是压缩贴图：" + text2));
		}
		catch (Exception value)
		{
			ModLog.Error($"最终接管角色列表贴图 {text2} 失败：{value}");
		}
	}

	internal static void ReplaceCharacterIcon(CharacterModel character, ref Control result)
	{
		SkinGroup skinGroup = FindGroup(((AbstractModel)character).Id.Entry);
		if (skinGroup == null)
		{
			return;
		}
		string text = ((AbstractModel)character).Id.Entry.ToLowerInvariant();
		string resourcePath = CanonicalScenePath("ui/character_icons/" + text + "_icon");
		if (SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(skinGroup.Id, resourcePath))
		{
			return;
		}
		try
		{
			Control val = SkinService.WithCharacterIconResource(skinGroup.Id, resourcePath, (Resource resource) => ((PackedScene)(((resource is PackedScene) ? resource : null) ?? throw new InvalidOperationException("角色头像资源不是场景：" + resourcePath))).Instantiate<Control>((GenEditState)0), includeProviderDependencies: true);
			ManagedSceneCompatibility.CopyMissingUniqueNodes((Node?)(object)result, (Node?)(object)val);
			Control obj = result;
			if (obj != null)
			{
				((Node)obj).QueueFree();
			}
			result = val;
		}
		catch (Exception value)
		{
			ModLog.Error($"最终接管角色小头像 {resourcePath} 失败：{value}");
		}
	}

	internal static void ReplaceCharacterTexture(CharacterModel character, string resourcePath, ref Texture2D result)
	{
		SkinGroup skinGroup = FindGroup(((AbstractModel)character).Id.Entry);
		if (skinGroup == null || SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(skinGroup.Id, resourcePath))
		{
			return;
		}
		try
		{
			Resource orLoadCharacterIconResource = SkinService.GetOrLoadCharacterIconResource(skinGroup.Id, resourcePath);
			result = (Texture2D)(object)(((orLoadCharacterIconResource is Texture2D) ? orLoadCharacterIconResource : null) ?? throw new InvalidOperationException("角色头像资源不是贴图：" + resourcePath));
		}
		catch (Exception value)
		{
			ModLog.Error($"最终接管角色界面贴图 {resourcePath} 失败：{value}");
		}
	}

	private static bool ShouldSkipExternalRuntimeRedirect(string groupId)
	{
		if (MultiplayerSkinSync.GetScopedSelection(groupId) == null)
		{
			return SkinService.IsExternalRuntimeProviderSelected(groupId);
		}
		return false;
	}

	internal static bool RefreshMultiplayerPlayerIcons(ulong playerNetId)
	{
		try
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			MainLoop obj = ((mainLoop is SceneTree) ? mainLoop : null);
			object obj2 = ((obj != null) ? ((SceneTree)obj).Root : null);
			if (obj2 == null)
			{
				NGame instance = NGame.Instance;
				obj2 = ((instance != null) ? ((Node)instance).GetTree().Root : null);
			}
			Window val = (Window)obj2;
			if (val == null)
			{
				return false;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			Stack<Node> stack = new Stack<Node>();
			stack.Push((Node)(object)val);
			while (stack.Count > 0)
			{
				Node val2 = stack.Pop();
				NMultiplayerPlayerState val3 = (NMultiplayerPlayerState)(object)((val2 is NMultiplayerPlayerState) ? val2 : null);
				if (val3 != null)
				{
					Player player = val3.Player;
					if (player != null && player.NetId == playerNetId)
					{
						if (RefreshMultiplayerPlayerStateIcon(val3))
						{
							num++;
						}
						goto IL_00d6;
					}
				}
				NRemoteLobbyPlayer val4 = (NRemoteLobbyPlayer)(object)((val2 is NRemoteLobbyPlayer) ? val2 : null);
				if (val4 != null && val4.PlayerId == playerNetId)
				{
					if (RefreshRemoteLobbyPlayerIcon(val4))
					{
						num2++;
					}
				}
				else
				{
					NMultiplayerVoteContainer val5 = (NMultiplayerVoteContainer)(object)((val2 is NMultiplayerVoteContainer) ? val2 : null);
					if (val5 != null)
					{
						num3 += RefreshMultiplayerVoteIcons(val5, playerNetId);
					}
				}
				goto IL_00d6;
				IL_00d6:
				foreach (Node child in val2.GetChildren(false))
				{
					stack.Push(child);
				}
			}
			if (num == 0 && num2 == 0 && num3 == 0)
			{
				return false;
			}
			ModLog.Info($"已刷新联机玩家 {playerNetId} 的头像：战斗栏 {num}、选角栏 {num2}、投票栏 {num3}。 ");
			return true;
		}
		catch (Exception ex)
		{
			ModLog.Warn($"刷新联机玩家 {playerNetId} 的头像失败：{ex.GetBaseException().Message}");
			return false;
		}
	}

	private static bool RefreshMultiplayerPlayerStateIcon(NMultiplayerPlayerState state)
	{
		object? obj = AccessTools.Field(typeof(NMultiplayerPlayerState), "_characterIcon")?.GetValue(state);
		TextureRect val = (TextureRect)(((obj is TextureRect) ? obj : null) ?? ((Node)state).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%CharacterIcon")));
		Player player = state.Player;
		CharacterModel val2 = ((player != null) ? player.Character : null);
		if (val == null || !GodotObject.IsInstanceValid((GodotObject)(object)val) || player == null || val2 == null)
		{
			return false;
		}
		using (MultiplayerSkinSync.BeginPlayerSelectionScope(player.NetId))
		{
			val.Texture = TryLoadManagedCharacterIconTexture(val2) ?? val2.IconTexture;
			return true;
		}
	}

	private static Texture2D? TryLoadManagedCharacterIconTexture(CharacterModel character)
	{
		SkinGroup skinGroup = FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name) ?? FindGroup(((AbstractModel)character).Id.Entry);
		string text = CanonicalImagePath("ui/top_panel/character_icon_" + ((AbstractModel)character).Id.Entry.ToLowerInvariant() + ".png");
		if (skinGroup == null || SkinService.ShouldDeferCharacterIconResourceToExternalRuntime(skinGroup.Id, text))
		{
			return null;
		}
		try
		{
			Resource orLoadCharacterIconResource = SkinService.GetOrLoadCharacterIconResource(skinGroup.Id, text);
			Texture2D val = (Texture2D)(object)((orLoadCharacterIconResource is Texture2D) ? orLoadCharacterIconResource : null);
			if (val != null)
			{
				return val;
			}
			LogMissingMultiplayerIcon(skinGroup.Id, text, "资源类型=" + ((object)orLoadCharacterIconResource).GetType().Name);
			return null;
		}
		catch (Exception ex)
		{
			LogMissingMultiplayerIcon(skinGroup.Id, text, ex.GetBaseException().Message);
			return null;
		}
	}

	private static void LogMissingMultiplayerIcon(string groupId, string path, string detail)
	{
		string scopedSelection = MultiplayerSkinSync.GetScopedSelection(groupId);
		if (scopedSelection != null && scopedSelection.StartsWith("__online_", StringComparison.OrdinalIgnoreCase))
		{
			string item = groupId + "\n" + scopedSelection + "\n" + path;
			if (LoggedMissingMultiplayerIcons.Add(item))
			{
				ModLog.Warn($"联机皮肤头像未能从安全资源包加载：分组={groupId}，选项={scopedSelection}，路径={path}，原因={detail}。 ");
			}
		}
	}

	private static bool RefreshRemoteLobbyPlayerIcon(NRemoteLobbyPlayer node)
	{
		object? obj = AccessTools.Field(typeof(NRemoteLobbyPlayer), "_character")?.GetValue(node);
		CharacterModel val = (CharacterModel)((obj is CharacterModel) ? obj : null);
		if (val == null)
		{
			return false;
		}
		using (MultiplayerSkinSync.BeginPlayerSelectionScope(node.PlayerId))
		{
			object? obj2 = AccessTools.Field(typeof(NRemoteLobbyPlayer), "_characterIcon")?.GetValue(node);
			TextureRect val2 = (TextureRect)(((obj2 is TextureRect) ? obj2 : null) ?? ((Node)node).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%CharacterIcon")));
			if (val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2))
			{
				return false;
			}
			val2.Texture = TryLoadManagedCharacterIconTexture(val) ?? val.IconTexture;
			return true;
		}
	}

	internal static void RefreshMultiplayerVoteIcons(NMultiplayerVoteContainer container)
	{
		RefreshMultiplayerVoteIcons(container, null);
	}

	private static int RefreshMultiplayerVoteIcons(NMultiplayerVoteContainer container, ulong? playerNetId)
	{
		int num = 0;
		try
		{
			if (!(AccessTools.Field(typeof(NMultiplayerVoteContainer), "_votes")?.GetValue(container) is IEnumerable enumerable))
			{
				return 0;
			}
			foreach (object item in enumerable)
			{
				if (item == null)
				{
					continue;
				}
				Type type = item.GetType();
				object? obj = AccessTools.Field(type, "player")?.GetValue(item);
				Player val = (Player)((obj is Player) ? obj : null);
				object? obj2 = AccessTools.Field(type, "node")?.GetValue(item);
				TextureRect val2 = (TextureRect)((obj2 is TextureRect) ? obj2 : null);
				if (val == null || val2 == null || !GodotObject.IsInstanceValid((GodotObject)(object)val2) || (playerNetId.HasValue && val.NetId != playerNetId.Value))
				{
					continue;
				}
				using (MultiplayerSkinSync.BeginPlayerSelectionScope(val.NetId))
				{
					val2.Texture = TryLoadManagedCharacterIconTexture(val.Character) ?? val.Character.IconTexture;
					TextureRect nodeOrNull = ((Node)val2).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Outline"));
					if (nodeOrNull != null && GodotObject.IsInstanceValid((GodotObject)(object)nodeOrNull))
					{
						nodeOrNull.Texture = val.Character.IconOutlineTexture;
					}
					num++;
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新联机投票头像失败：" + ex.GetBaseException().Message);
		}
		return num;
	}

	internal static void ReplaceCharacterMapMarker(CharacterModel character, ref CompressedTexture2D result)
	{
		string text = ((AbstractModel)character).Id.Entry.ToLowerInvariant();
		string resourcePath = CanonicalImagePath("packed/map/icons/map_marker_" + text + ".png");
		Texture2D result2 = (Texture2D)(object)result;
		ReplaceCharacterTexture(character, resourcePath, ref result2);
		CompressedTexture2D val = (CompressedTexture2D)(object)((result2 is CompressedTexture2D) ? result2 : null);
		if (val != null)
		{
			result = val;
		}
	}

	private static void AttachMonsterPriorityControls(NBestiary screen, HBoxContainer selector)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Expected O, but got Unknown
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Expected O, but got Unknown
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Expected O, but got Unknown
		Control val = new Control
		{
			Name = StringName.op_Implicit("MonsterSkinPriorityHeader"),
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			OffsetLeft = -330f,
			OffsetTop = 72f,
			OffsetRight = 330f,
			OffsetBottom = 118f,
			MouseFilter = (MouseFilterEnum)2,
			Visible = false,
			ZIndex = 10
		};
		Label val2 = new Label
		{
			Name = StringName.op_Implicit("MonsterSkinPriorityRegionLabel"),
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			OffsetLeft = -295f,
			OffsetRight = -109f,
			OffsetBottom = 42f,
			HorizontalAlignment = (HorizontalAlignment)2,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 23);
		ModThemeRuntime.AccentText((Control)(object)val2);
		if (GameFont != null)
		{
			((Control)val2).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
		}
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Button val3 = new Button
		{
			Name = StringName.op_Implicit("MonsterSkinPriorityButton"),
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			OffsetLeft = -95f,
			OffsetRight = 95f,
			OffsetBottom = 42f,
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2
		};
		ApplyCompactButtonTheme(val3);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		Button presetButton = new Button
		{
			Name = StringName.op_Implicit("MonsterPresetButton"),
			AnchorLeft = 0.5f,
			AnchorRight = 0.5f,
			OffsetLeft = 110f,
			OffsetRight = 270f,
			OffsetBottom = 42f,
			FocusMode = (FocusModeEnum)0,
			MouseDefaultCursorShape = (CursorShape)2,
			Text = ModLocalization.Get(ModText.CardPresets)
		};
		ApplyCompactButtonTheme(presetButton);
		((Node)val).AddChild((Node)(object)presetButton, false, (InternalMode)0);
		((Node)screen).AddChild((Node)(object)val, false, (InternalMode)0);
		Control overlay = CreateMonsterPriorityOverlay();
		((Node)screen).AddChild((Node)(object)overlay, false, (InternalMode)0);
		overlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((BaseButton)val3).Pressed += delegate
		{
			OpenMonsterPriorityOverlay(screen, selector, overlay);
		};
		Control presetOverlay = CreateMonsterPresetOverlay();
		((Node)screen).AddChild((Node)(object)presetOverlay, false, (InternalMode)0);
		presetOverlay.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((BaseButton)presetButton).Pressed += delegate
		{
			OpenMonsterPresetOverlay(screen, selector, presetOverlay);
		};
		ModLocalization.Bind((Node)(object)overlay, delegate
		{
			if (((CanvasItem)overlay).Visible)
			{
				BuildMonsterPriorityOverlay(screen, selector, overlay);
			}
			presetButton.Text = ModLocalization.Get(ModText.CardPresets);
			if (((CanvasItem)presetOverlay).Visible)
			{
				BuildMonsterPresetOverlay(screen, selector, presetOverlay);
			}
		});
	}

	private static MonsterSkinCategory? ResolveMonsterSkinCategory(NBestiaryEntry entry)
	{
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		EncounterModel encounter = entry.Entry.encounterModel;
		if (encounter == null)
		{
			return null;
		}
		foreach (ActModel act in ModelDb.Acts)
		{
			if (act.AllEncounters.Any((EncounterModel candidate) => ((AbstractModel)candidate).Id.Equals(((AbstractModel)encounter).Id)))
			{
				return BuildMonsterSkinCategory("act:" + ((AbstractModel)act).Id.Entry.ToLowerInvariant(), act.Title.GetFormattedText(), act.AllMonsters);
			}
		}
		EncounterModel[] source = GetEventEncounters().ToArray();
		if (source.Any((EncounterModel candidate) => ((AbstractModel)candidate).Id.Equals(((AbstractModel)encounter).Id)))
		{
			return BuildMonsterSkinCategory("events", new LocString("bestiary", "EVENTS.title").GetFormattedText(), source.SelectMany((EncounterModel candidate) => candidate.AllPossibleMonsters));
		}
		return null;
	}

	private static IEnumerable<EncounterModel> GetEventEncounters()
	{
		return (typeof(ModelDb).GetProperty("EventEncounters")?.GetValue(null) as IEnumerable<EncounterModel>) ?? Array.Empty<EncounterModel>();
	}

	private static MonsterSkinCategory BuildMonsterSkinCategory(string id, string displayName, IEnumerable<MonsterModel> monsters)
	{
		if (MonsterCategoryGroupCache.TryGetValue(id, out IReadOnlyList<string> value))
		{
			return new MonsterSkinCategory(id, displayName, value);
		}
		string[] array = (from @group in monsters.Select((MonsterModel monster) => FindGroup(((AbstractModel)monster).Id.Entry, ((object)monster).GetType().Name)).Where(delegate(SkinGroup @group)
			{
				if (@group != null)
				{
					List<SkinOption> options = @group.Options;
					if (options != null)
					{
						return options.Count > 0;
					}
				}
				return false;
			})
			select @group.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		MonsterCategoryGroupCache[id] = array;
		return new MonsterSkinCategory(id, displayName, array);
	}

	private static void SetMonsterPriorityContext(HBoxContainer selector, MonsterSkinCategory? category)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		if (category == null || category.GroupIds.Count == 0 || !SkinService.RegisterMonsterSkinCategory(category.Id, category.GroupIds))
		{
			((GodotObject)selector).RemoveMeta(StringName.op_Implicit("sts2_skin_monster_category"));
			((GodotObject)selector).RemoveMeta(StringName.op_Implicit("sts2_skin_monster_category_name"));
			RefreshMonsterPriorityButton(selector);
		}
		else
		{
			((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_monster_category"), Variant.op_Implicit(category.Id));
			((GodotObject)selector).SetMeta(StringName.op_Implicit("sts2_skin_monster_category_name"), Variant.op_Implicit(category.DisplayName));
			RefreshMonsterPriorityButton(selector);
		}
	}

	private static bool HasMonsterPriorityContext(HBoxContainer selector)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category"), Variant.op_Implicit(string.Empty));
		return !string.IsNullOrWhiteSpace(((Variant)(ref meta)).AsString());
	}

	private static void RefreshMonsterPriorityButton(HBoxContainer selector)
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		Node parent = ((Node)selector).GetParent();
		Node obj = ((parent is NBestiary) ? parent : null);
		Control val = ((obj != null) ? obj.GetNodeOrNull<Control>(NodePath.op_Implicit("MonsterSkinPriorityHeader")) : null);
		Button val2 = ((val != null) ? ((Node)val).GetNodeOrNull<Button>(NodePath.op_Implicit("MonsterSkinPriorityButton")) : null);
		Label val3 = ((val != null) ? ((Node)val).GetNodeOrNull<Label>(NodePath.op_Implicit("MonsterSkinPriorityRegionLabel")) : null);
		Button val4 = ((val != null) ? ((Node)val).GetNodeOrNull<Button>(NodePath.op_Implicit("MonsterPresetButton")) : null);
		if (val != null && val2 != null && val4 != null && val3 != null)
		{
			Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category"), Variant.op_Implicit(string.Empty));
			string text = ((Variant)(ref meta)).AsString();
			IReadOnlyList<MonsterPriorityOptionState> readOnlyList;
			if (!string.IsNullOrWhiteSpace(text))
			{
				readOnlyList = SkinService.GetMonsterPriorityOptions(text);
			}
			else
			{
				IReadOnlyList<MonsterPriorityOptionState> readOnlyList2 = Array.Empty<MonsterPriorityOptionState>();
				readOnlyList = readOnlyList2;
			}
			IReadOnlyList<MonsterPriorityOptionState> readOnlyList3 = readOnlyList;
			((CanvasItem)val).Visible = readOnlyList3.Count > 0;
			meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category_name"), Variant.op_Implicit(string.Empty));
			val3.Text = ((Variant)(ref meta)).AsString();
			val2.Text = ModLocalization.Get(ModText.MonsterSkinPriority);
			((Control)val2).TooltipText = string.Empty;
			val4.Text = ModLocalization.Get(ModText.CardPresets);
		}
	}

	private static Control CreateMonsterPriorityOverlay()
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
			Name = StringName.op_Implicit("MonsterSkinPriorityOverlay"),
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
			Name = StringName.op_Implicit("PriorityPanel"),
			MouseFilter = (MouseFilterEnum)0,
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -360f,
			OffsetTop = -245f,
			OffsetRight = 360f,
			OffsetBottom = 245f
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateStyleBox(new Color("241a30"), new Color("79547e"), 2));
		ModThemeRuntime.Panel(val2);
		((Node)overlay).AddChild((Node)(object)val2, false, (InternalMode)0);
		MarginContainer val3 = new MarginContainer
		{
			Name = StringName.op_Implicit("PriorityMargin")
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

	private static void OpenMonsterPriorityOverlay(NBestiary screen, HBoxContainer selector, Control overlay)
	{
		BuildMonsterPriorityOverlay(screen, selector, overlay);
		((CanvasItem)overlay).Visible = true;
		((CanvasItem)overlay).MoveToFront();
	}

	private static void BuildMonsterPriorityOverlay(NBestiary screen, HBoxContainer selector, Control overlay)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected O, but got Unknown
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Expected O, but got Unknown
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Expected O, but got Unknown
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Expected O, but got Unknown
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Expected O, but got Unknown
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Expected O, but got Unknown
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_0454: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Expected O, but got Unknown
		//IL_049a: Unknown result type (might be due to invalid IL or missing references)
		//IL_049f: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e2: Expected O, but got Unknown
		//IL_0536: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_055d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Expected O, but got Unknown
		//IL_0594: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Expected O, but got Unknown
		//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Expected O, but got Unknown
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category"), Variant.op_Implicit(string.Empty));
		string categoryId = ((Variant)(ref meta)).AsString();
		meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category_name"), Variant.op_Implicit(string.Empty));
		string text = ((Variant)(ref meta)).AsString();
		VBoxContainer node = ((Node)overlay).GetNode<VBoxContainer>(NodePath.op_Implicit("PriorityPanel/PriorityMargin/PriorityContent"));
		ScrollContainer val = ScrollListRebuild.Begin(node, categoryId);
		IReadOnlyList<MonsterPriorityOptionState> monsterPriorityOptions = SkinService.GetMonsterPriorityOptions(categoryId);
		if (string.IsNullOrWhiteSpace(categoryId) || monsterPriorityOptions.Count == 0)
		{
			((CanvasItem)overlay).Visible = false;
			return;
		}
		Label val2 = new Label
		{
			Text = text + " · " + ModLocalization.Get(ModText.MonsterSkinPriority),
			HorizontalAlignment = (HorizontalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 25);
		ModThemeRuntime.AccentText((Control)(object)val2);
		if (GameFont != null)
		{
			((Control)val2).AddThemeFontOverride(StringName.op_Implicit("font"), GameFont);
		}
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
		foreach (MonsterPriorityOptionState option in monsterPriorityOptions)
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
				Color = MonsterPriorityColors[option.ColorIndex % MonsterPriorityColors.Length],
				CustomMinimumSize = new Vector2(13f, 32f),
				MouseFilter = (MouseFilterEnum)2
			}, false, (InternalMode)0);
			CheckBox val5 = new CheckBox
			{
				ButtonPressed = option.Enabled,
				Text = ModLocalization.Get(ModText.EnabledForCategory),
				CustomMinimumSize = new Vector2(88f, 32f),
				TooltipText = ModLocalization.DisplayOptionName(option.Name)
			};
			ApplyGameTheme((Button)(object)val5);
			((Control)val5).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 17);
			((BaseButton)val5).Toggled += (ToggledEventHandler)delegate(bool value)
			{
				QueueMonsterPriorityChange(screen, selector, overlay, categoryId, () => SkinService.SetMonsterPriorityOptionEnabled(categoryId, option.OptionId, value));
			};
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
			Label val6 = new Label
			{
				Text = ModLocalization.DisplayOptionName(option.Name),
				ClipText = true,
				CustomMinimumSize = new Vector2(270f, 36f),
				SizeFlagsHorizontal = (SizeFlags)3,
				VerticalAlignment = (VerticalAlignment)1,
				TooltipText = ModLocalization.DisplayOptionName(option.Name)
			};
			((Control)val6).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((Control)val6).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
			((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
			Label val7 = new Label
			{
				Text = $"{option.Coverage}/{option.TotalMonsters}",
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
				Disabled = (option == monsterPriorityOptions[0]),
				CustomMinimumSize = new Vector2(42f, 34f)
			};
			ApplyGameTheme(val8);
			((Control)val8).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((BaseButton)val8).Pressed += delegate
			{
				QueueMonsterPriorityChange(screen, selector, overlay, categoryId, () => SkinService.MoveMonsterPriority(categoryId, option.OptionId, -1));
			};
			((Node)val4).AddChild((Node)(object)val8, false, (InternalMode)0);
			Button val9 = new Button
			{
				Text = "↓"
			};
			((BaseButton)val9).Disabled = option == monsterPriorityOptions[monsterPriorityOptions.Count - 1];
			((Control)val9).CustomMinimumSize = new Vector2(42f, 34f);
			Button val10 = val9;
			ApplyGameTheme(val10);
			((Control)val10).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 18);
			((BaseButton)val10).Pressed += delegate
			{
				QueueMonsterPriorityChange(screen, selector, overlay, categoryId, () => SkinService.MoveMonsterPriority(categoryId, option.OptionId, 1));
			};
			((Node)val4).AddChild((Node)(object)val10, false, (InternalMode)0);
		}
		Button val11 = new Button
		{
			Text = ModLocalization.Get(ModText.Close),
			CustomMinimumSize = new Vector2(180f, 42f),
			SizeFlagsHorizontal = (SizeFlags)4
		};
		ApplyGameTheme(val11);
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
		((Node)val12).AddChild((Node)(object)SkinWorkshopEntry.CreatePriorityButton((Node)(object)screen, "monster", "", delegate
		{
			BuildMonsterPriorityOverlay(screen, selector, overlay);
		}, categoryId), false, (InternalMode)0);
		((Node)node).AddChild((Node)(object)val12, false, (InternalMode)0);
	}

	private static Control CreateMonsterPresetOverlay()
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
			Name = StringName.op_Implicit("MonsterPresetOverlay"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 2001
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
			Name = StringName.op_Implicit("PresetPanel"),
			MouseFilter = (MouseFilterEnum)0,
			AnchorLeft = 0.5f,
			AnchorTop = 0.5f,
			AnchorRight = 0.5f,
			AnchorBottom = 0.5f,
			OffsetLeft = -470f,
			OffsetTop = -275f,
			OffsetRight = 470f,
			OffsetBottom = 275f
		};
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)CreateStyleBox(new Color("241a30"), new Color("79547e"), 2));
		ModThemeRuntime.Panel(val2);
		((Node)overlay).AddChild((Node)(object)val2, false, (InternalMode)0);
		MarginContainer val3 = new MarginContainer
		{
			Name = StringName.op_Implicit("PresetMargin")
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

	private static void OpenMonsterPresetOverlay(NBestiary screen, HBoxContainer selector, Control overlay)
	{
		BuildMonsterPresetOverlay(screen, selector, overlay);
		((CanvasItem)overlay).Visible = true;
		((CanvasItem)overlay).MoveToFront();
	}

	private static void BuildMonsterPresetOverlay(NBestiary screen, HBoxContainer selector, Control overlay)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Expected O, but got Unknown
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Expected O, but got Unknown
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Expected O, but got Unknown
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Expected O, but got Unknown
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Expected O, but got Unknown
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_034f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Expected O, but got Unknown
		//IL_0380: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03df: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Expected O, but got Unknown
		Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category"), Variant.op_Implicit(string.Empty));
		string categoryId = ((Variant)(ref meta)).AsString();
		meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category_name"), Variant.op_Implicit(string.Empty));
		string text = ((Variant)(ref meta)).AsString();
		VBoxContainer node = ((Node)overlay).GetNode<VBoxContainer>(NodePath.op_Implicit("PresetPanel/PresetMargin/PresetContent"));
		ScrollContainer val = ScrollListRebuild.Begin(node, categoryId);
		if (string.IsNullOrWhiteSpace(categoryId))
		{
			((CanvasItem)overlay).Visible = false;
			return;
		}
		Label val2 = new Label
		{
			Text = text + " · " + ModLocalization.Get(ModText.CardPresets),
			HorizontalAlignment = (HorizontalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 25);
		ModThemeRuntime.AccentText((Control)(object)val2);
		((Node)node).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer val3 = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(890f, 44f)
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
		Button val4 = CreateMonsterPresetActionButton(ModLocalization.Get(ModText.SaveCurrentPreset), 220f);
		((BaseButton)val4).Pressed += delegate
		{
			QueueMonsterPresetChange(screen, selector, overlay, () => SkinService.CreateMonsterSkinPreset(categoryId, newName.Text), refreshMonsters: false);
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
		IReadOnlyList<MonsterSkinPresetState> monsterSkinPresets = SkinService.GetMonsterSkinPresets(categoryId);
		if (monsterSkinPresets.Count == 0)
		{
			Label val6 = new Label
			{
				Text = ModLocalization.Get(ModText.NoMonsterPresets),
				HorizontalAlignment = (HorizontalAlignment)1,
				VerticalAlignment = (VerticalAlignment)1,
				CustomMinimumSize = new Vector2(860f, 100f),
				MouseFilter = (MouseFilterEnum)2
			};
			((Control)val6).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 19);
			((Control)val6).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("b9adbd"));
			((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		}
		foreach (MonsterSkinPresetState preset in monsterSkinPresets)
		{
			HBoxContainer val7 = new HBoxContainer
			{
				CustomMinimumSize = new Vector2(870f, 46f)
			};
			((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			((Node)val5).AddChild((Node)(object)val7, false, (InternalMode)0);
			Label val8 = new Label
			{
				Text = (preset.Active ? "●" : string.Empty),
				CustomMinimumSize = new Vector2(24f, 38f),
				HorizontalAlignment = (HorizontalAlignment)1,
				VerticalAlignment = (VerticalAlignment)1
			};
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
			Button val9 = CreateMonsterPresetActionButton(preset.Active ? ModLocalization.Get(ModText.ActiveCardPreset) : ModLocalization.Get(ModText.ApplyCardPreset), 112f);
			((BaseButton)val9).Disabled = preset.Active;
			((BaseButton)val9).Pressed += delegate
			{
				QueueMonsterPresetChange(screen, selector, overlay, () => SkinService.ApplyMonsterSkinPreset(categoryId, preset.Name), refreshMonsters: true);
			};
			((Node)val7).AddChild((Node)(object)val9, false, (InternalMode)0);
			Button val10 = CreateMonsterPresetActionButton(ModLocalization.Get(ModText.OverwriteCardPreset), 100f);
			((BaseButton)val10).Pressed += delegate
			{
				QueueMonsterPresetChange(screen, selector, overlay, () => SkinService.OverwriteMonsterSkinPreset(categoryId, preset.Name), refreshMonsters: false);
			};
			((Node)val7).AddChild((Node)(object)val10, false, (InternalMode)0);
			Button val11 = CreateMonsterPresetActionButton(ModLocalization.Get(ModText.RenameCardPreset), 100f);
			((BaseButton)val11).Pressed += delegate
			{
				QueueMonsterPresetChange(screen, selector, overlay, () => SkinService.RenameMonsterSkinPreset(categoryId, preset.Name, name.Text), refreshMonsters: false);
			};
			((Node)val7).AddChild((Node)(object)val11, false, (InternalMode)0);
			((BaseButton)val11).Disabled = preset.IsBundlePreset;
			Button delete = CreateMonsterPresetActionButton(ModLocalization.Get(ModText.DeleteCardPreset), 112f);
			bool deleteArmed = false;
			((BaseButton)delete).Pressed += delegate
			{
				//IL_0036: Unknown result type (might be due to invalid IL or missing references)
				if (!deleteArmed)
				{
					deleteArmed = true;
					delete.Text = ModLocalization.Get(ModText.ConfirmDeleteCardPreset);
					((Control)delete).AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("ef6670"));
				}
				else
				{
					QueueMonsterPresetChange(screen, selector, overlay, () => SkinService.DeleteMonsterSkinPreset(categoryId, preset.Name), refreshMonsters: false);
				}
			};
			((Node)val7).AddChild((Node)(object)delete, false, (InternalMode)0);
			((BaseButton)delete).Disabled = preset.IsBundlePreset;
		}
		Button val12 = CreateMonsterPresetActionButton(ModLocalization.Get(ModText.Close), 180f);
		((Control)val12).SizeFlagsHorizontal = (SizeFlags)4;
		((BaseButton)val12).Pressed += delegate
		{
			((CanvasItem)overlay).Visible = false;
		};
		((Node)node).AddChild((Node)(object)val12, false, (InternalMode)0);
	}

	private static Button CreateMonsterPresetActionButton(string text, float width)
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

	private static void QueueMonsterPresetChange(NBestiary screen, HBoxContainer selector, Control overlay, Func<bool> change, bool refreshMonsters)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			if (!GodotObject.IsInstanceValid((GodotObject)(object)screen) || !change())
			{
				ModLog.Error("调整怪物皮肤预设失败：" + SkinService.LastError);
			}
			else
			{
				BuildMonsterPresetOverlay(screen, selector, overlay);
				RefreshMonsterPriorityButton(selector);
				if (refreshMonsters)
				{
					Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_monster_category"), Variant.op_Implicit(string.Empty));
					string key = ((Variant)(ref meta)).AsString();
					IReadOnlyList<string> affectedGroupIds = MonsterCategoryGroupCache.GetValueOrDefault(key) ?? Array.Empty<string>();
					if (NRun.Instance != null)
					{
						CharacterAppearanceRuntime.RefreshRunMonsterSelection(affectedGroupIds, "局内怪物预设");
					}
					RefreshBestiaryMonsterNames(screen);
					meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
					string text = ((Variant)(ref meta)).AsString();
					SkinGroup skinGroup = (string.IsNullOrWhiteSpace(text) ? null : FindGroup(text));
					if (skinGroup != null)
					{
						Populate(selector, skinGroup);
						if (RefreshActions.TryGetValue(((GodotObject)selector).GetInstanceId(), out Action value))
						{
							RunRefresh(value);
						}
					}
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void QueueMonsterPriorityChange(NBestiary screen, HBoxContainer selector, Control overlay, string categoryId, Func<bool> change)
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
				ModLog.Error("调整怪物皮肤优先级失败：" + SkinService.LastError);
			}
			else
			{
				Variant meta = ((GodotObject)selector).GetMeta(StringName.op_Implicit("sts2_skin_group"), Variant.op_Implicit(string.Empty));
				string text = ((Variant)(ref meta)).AsString();
				SkinGroup skinGroup = (string.IsNullOrWhiteSpace(text) ? null : FindGroup(text));
				if (skinGroup != null)
				{
					Populate(selector, skinGroup);
				}
				IReadOnlyList<string> readOnlyList = MonsterCategoryGroupCache.GetValueOrDefault(categoryId) ?? Array.Empty<string>();
				if (NRun.Instance != null)
				{
					CharacterAppearanceRuntime.RefreshRunMonsterSelection(readOnlyList, "局内怪物地区优先级");
				}
				else
				{
					SkinService.FocusRuntimeProviderBehaviorsOnGroups(readOnlyList, (IReadOnlyCollection<string>)(object)Array.Empty<string>(), "怪物图鉴名称刷新");
				}
				RefreshBestiaryMonsterNames(screen);
				BuildMonsterPriorityOverlay(screen, selector, overlay);
				RefreshMonsterPriorityButton(selector);
				if (RefreshActions.TryGetValue(((GodotObject)selector).GetInstanceId(), out Action value))
				{
					RunRefresh(value);
				}
				if (skinGroup != null)
				{
					if (NRun.Instance != null)
					{
						CharacterAppearanceRuntime.FocusRuntimeProviderBehaviorsOnRunContext(new _003C_003Ez__ReadOnlySingleElementList<string>(skinGroup.Id), "局内怪物图鉴", null, null, refreshCurrentRoom: true);
					}
					else
					{
						SkinService.FocusRuntimeProviderBehaviorsOnGroups(new _003C_003Ez__ReadOnlySingleElementList<string>(skinGroup.Id), (IReadOnlyCollection<string>)(object)Array.Empty<string>(), "怪物图鉴");
					}
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void RefreshBestiaryMonsterNames(NBestiary screen)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)screen))
		{
			return;
		}
		try
		{
			VBoxContainer nodeOrNull = ((Node)screen).GetNodeOrNull<VBoxContainer>(NodePath.op_Implicit("%BestiaryList"));
			if (nodeOrNull != null)
			{
				foreach (NBestiaryEntry item in from entry in ((IEnumerable)((Node)nodeOrNull).GetChildren(false)).OfType<NBestiaryEntry>()
					where entry.IsDiscovered
					select entry)
				{
					ManagedSkinModLoader.RestoreAllNodeReadyBehaviors((Node)(object)item);
					RichTextLabel nodeOrNull2 = ((Node)item).GetNodeOrNull<RichTextLabel>(NodePath.op_Implicit("%Label"));
					if (nodeOrNull2 != null)
					{
						nodeOrNull2.Text = item.Entry.GetEntryTitle();
					}
					MonsterModel monsterModel = item.Entry.monsterModel;
					SkinGroup skinGroup = ((monsterModel == null) ? null : FindGroup(((AbstractModel)monsterModel).Id.Entry, ((object)monsterModel).GetType().Name));
					string text = ((skinGroup == null) ? null : SkinService.GetSelectedRuntimeProvider(skinGroup.Id));
					if (text != null)
					{
						ManagedSkinModLoader.ReplaySelectedNodeReadyBehavior(text, (Node)(object)item);
					}
				}
			}
			object? obj = AccessTools.Field(typeof(NBestiary), "_selectedEntry")?.GetValue(screen);
			NBestiaryEntry val = (NBestiaryEntry)((obj is NBestiaryEntry) ? obj : null);
			if (val != null && val.IsDiscovered)
			{
				RichTextLabel nodeOrNull3 = ((Node)screen).GetNodeOrNull<RichTextLabel>(NodePath.op_Implicit("%MonsterName"));
				if (nodeOrNull3 != null)
				{
					nodeOrNull3.Text = val.Entry.GetEntryTitle();
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("刷新怪物图鉴名称失败：" + ex.GetBaseException().Message);
		}
	}

	internal static void ScheduleInitialBestiaryMonsterNameRefresh(NBestiary screen)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			RefreshBestiaryMonsterNames(screen);
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}
}

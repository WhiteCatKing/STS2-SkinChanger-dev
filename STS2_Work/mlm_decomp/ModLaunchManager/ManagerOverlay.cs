using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Debug;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Saves;
using Steamworks;

namespace ModLaunchManager;

internal static class ManagerOverlay
{
	private enum PromptKey
	{
		A,
		B,
		X,
		Y,
		LeftShoulder,
		RightShoulder,
		LeftTrigger,
		RightTrigger,
		Start,
		Back,
		UpDown
	}

	private readonly record struct Prompt(PromptKey[] Keys, string Text);

	private sealed record TreeBinding(TreeItem Item, LaunchEntry Entry);

	private enum ModFilter
	{
		All,
		Enabled,
		Disabled,
		Workshop,
		Local,
		Updates
	}

	private enum PendingCanonicalAction
	{
		None,
		Launch,
		Skip
	}

	private readonly record struct ModRowStatus(string Text, Color Color, string? Tooltip = null)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Text = ");
			builder.Append((object?)Text);
			builder.Append(", Color = ");
			Color color = Color;
			builder.Append(((object)(Color)(ref color)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Tooltip = ");
			builder.Append((object?)Tooltip);
			return true;
		}
	}

	private sealed class RowStatusLookup
	{
		private readonly Dictionary<LaunchEntry, List<ModConflict>> _conflicts = new Dictionary<LaunchEntry, List<ModConflict>>();

		private readonly HashSet<string> _sharedIds;

		internal RowStatusLookup(LaunchPlan plan)
		{
			foreach (ModConflict item in plan.Conflicts())
			{
				Add(item.Declarer, item);
				if (item.Target != item.Declarer)
				{
					Add(item.Target, item);
				}
			}
			_sharedIds = plan.SharedModIds();
		}

		private void Add(LaunchEntry entry, ModConflict conflict)
		{
			if (!_conflicts.TryGetValue(entry, out List<ModConflict> value))
			{
				value = (_conflicts[entry] = new List<ModConflict>());
			}
			value.Add(conflict);
		}

		internal List<IGrouping<LaunchEntry, ModConflict>> ActiveConflicts(LaunchEntry entry)
		{
			if (!entry.Enabled || !_conflicts.TryGetValue(entry, out List<ModConflict> value))
			{
				return new List<IGrouping<LaunchEntry, ModConflict>>();
			}
			return (from conflict in value
				group conflict by conflict.OtherThan(entry)).ToList();
		}

		internal bool HasDuplicateSiblings(LaunchEntry entry)
		{
			string text = entry.Mod.manifest?.id;
			if (text != null && !string.IsNullOrWhiteSpace(text))
			{
				return _sharedIds.Contains(text);
			}
			return false;
		}
	}

	private enum SettingsSection
	{
		Root,
		Customize,
		Saves,
		Mods,
		Ai
	}

	private enum BackgroundTrigger
	{
		Startup,
		Menu,
		Folder
	}

	private sealed record TourScene(string Id, Action Enter, Action Leave);

	private sealed record TourStep(string Id, string ChapterKey, string TitleKey, Func<bool, string> Body, Func<IEnumerable<Control?>> Targets, TourScene? Scene = null, int Since = 0, Func<bool>? Available = null, Action? Prepare = null);

	private sealed record TourButtonState(bool RandomVisible, bool VanillaVisible, bool RandomSwitch, bool VanillaSwitch);

	private sealed record UndoStep(List<LaunchEntry> Order, Dictionary<LaunchEntry, bool> Enabled, ModOrganizationDocument Organization, string Tag, ulong Frame, ulong At);

	[CompilerGenerated]
	private sealed class _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed : IEnumerable<TreeItem>, IEnumerable, IEnumerator<TreeItem>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private TreeItem _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private TreeItem first;

		public TreeItem _003C_003E3__first;

		private TreeItem _003Citem_003E5__2;

		private IEnumerator<TreeItem> _003C_003E7__wrap2;

		TreeItem IEnumerator<TreeItem>.Current
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
		public _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 2)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003Citem_003E5__2 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_003Citem_003E5__2 = first;
					goto IL_00ed;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap2 = _003CFolderHeaders_003Eg__Walk_007C565_0(_003Citem_003E5__2.GetFirstChild()).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_00c2;
				case 2:
					{
						_003C_003E1__state = -3;
						goto IL_00c2;
					}
					IL_00c2:
					if (_003C_003E7__wrap2.MoveNext())
					{
						TreeItem current = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap2 = null;
					goto IL_00dc;
					IL_00dc:
					_003Citem_003E5__2 = _003Citem_003E5__2.GetNext();
					goto IL_00ed;
					IL_00ed:
					if (_003Citem_003E5__2 != null)
					{
						Variant metadata = _003Citem_003E5__2.GetMetadata(0);
						if (IsFolderKey(((Variant)(ref metadata)).AsString()))
						{
							_003C_003E2__current = _003Citem_003E5__2;
							_003C_003E1__state = 1;
							return true;
						}
						goto IL_00dc;
					}
					_003Citem_003E5__2 = null;
					return false;
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
		IEnumerator<TreeItem> IEnumerable<TreeItem>.GetEnumerator()
		{
			_003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed = this;
			}
			else
			{
				_003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed = new _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed(0);
			}
			_003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed.first = _003C_003E3__first;
			return _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TreeItem>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private static class _003C_003EO
	{
		public static Func<LaunchProfile, PlanResult> _003C0_003E__ApplyTransferredList;

		public static Action _003C1_003E__RebuildProfileRow;

		public static Func<string, bool> _003C2_003E__Exists;

		public static Func<Mod, bool> _003C3_003E__KeepsOff;

		public static Func<IReadOnlyDictionary<string, LocalModInstalled>> _003C4_003E__InstalledModsById;

		public static Func<string> _003C5_003E__PrepareLocalModsFolder;

		public static Action<IReadOnlyList<string>> _003C6_003E__OnLocalModsReplaced;

		public static Action _003C7_003E__OpenLocalModsFolder;

		public static Func<LocalModScan, bool> _003C8_003E__OnLocalModsChanged;

		public static Action<string> _003C9_003E__OnLocalModWatcherError;

		public static ItemSelectedEventHandler _003C10_003E__OnAiProviderSelected;

		public static Action _003C11_003E__OpenAiKeyPage;

		public static Action _003C12_003E__SaveAiSettings;

		public static Action _003C13_003E__ForgetAiKey;

		public static Action _003C14_003E__ClearAiSecrets;

		public static Func<string, string> _003C15_003E__GetFileName;

		public static Action<Dictionary<string, string>> _003C16_003E__ValidateFormat;

		public static Action _003C17_003E__OpenChangeNotes;

		public static Action _003C18_003E__UpdateChangeNotesChrome;

		public static Action<IReadOnlyList<ulong>> _003C19_003E__TranslateChangeNotes;

		public static Action _003C20_003E__OnChangeNotesChanged;

		public static Func<ulong, string> _003C21_003E__WorkshopItemName;

		public static Action<ulong> _003C22_003E__OpenWorkshopChangeNotesPage;

		public static Action _003C23_003E__ControllerBack;

		public static Func<Control?> _003C24_003E__ControllerHome;

		public static Func<bool> _003C25_003E__RevealControllerFocus;

		public static Action<Control> _003C26_003E__ShowControllerKeyboard;

		public static Action _003C27_003E__OnControllerModeChanged;

		public static Action _003C28_003E__OnControllerFocusChanged;

		public static Func<Control, bool> _003C29_003E__ControllerAccept;

		public static Func<JoyButton, bool> _003C30_003E__ControllerShortcut;

		public static Func<Control, Vector2I, bool> _003C31_003E__ControllerDirection;

		public static Func<Control, Vector2I, Control?> _003C32_003E__ControllerRedirect;

		public static Func<Control, int, bool> _003C33_003E__ControllerPage;

		public static Action _003C34_003E__RebuildGroupRow;

		public static Action _003C35_003E__SyncControllerPromptTheme;

		public static Action<WorkshopBranchStatus?> _003C36_003E__OnWorkshopBranchChanged;

		public static Action _003C37_003E__OnBackgroundFolderChanged;

		public static Action<Mod> _003C38_003E__OnVanillaModDetected;

		public static Action<int> _003C39_003E__StartWhatsNewTour;

		public static ItemSelectedEventHandler _003C40_003E__OnLanguageSelected;

		public static ItemSelectedEventHandler _003C41_003E__OnThemeSelected;

		public static Action _003C42_003E__FlushPendingBackgroundChange;

		public static ItemSelectedEventHandler _003C43_003E__OnSaveProfileSelected;

		public static Action _003C44_003E__OpenSettings;

		public static Action _003C45_003E__DownloadAllWorkshopUpdates;

		public static Action _003C46_003E__RepeatSelectedMove;

		public static Action _003C47_003E__ShowGroupMenuForVisibleRows;

		public static Func<string, bool> _003C48_003E__CanStartTreeDrag;

		public static Func<string, string, bool> _003C49_003E__CanDropTreeEntry;

		public static Func<string, string, float, TreeDropPlacement> _003C50_003E__TreeDropPlacementFor;

		public static Action<string, string> _003C51_003E__DropTreeEntry;

		public static Action<string, string, TreeDropPlacement> _003C52_003E__DropTreeEntryAt;

		public static Action<string, string, TreeDropPlacement> _003C53_003E__OnTreeDropTargetChanged;

		public static Action<string> _003C54_003E__OnTreeHeadingDwell;

		public static Action<int> _003C55_003E__MoveSelected;

		public static Action _003C56_003E__ToggleFromKeyboard;

		public static Action<int> _003C57_003E__OnTreeSideways;

		public static Action<bool> _003C58_003E__OnTreeDragActiveChanged;

		public static Func<string, bool> _003C59_003E__IsFolderKey;

		public static Action<string> _003C60_003E__OnTreeHeaderActivated;

		public static Action _003C61_003E__OnTreeSelectionChanged;

		public static Action<string> _003C62_003E__ToggleEntry;

		public static Action<string, Vector2> _003C63_003E__ShowGroupMenuForRow;

		public static IdPressedEventHandler _003C64_003E__OnGroupMenuIdPressed;

		public static Action _003C65_003E__ClearFilters;

		public static Action _003C66_003E__RunQueuedRebuild;

		public static Action _003C67_003E__OnTreeScrolled;

		public static Action _003C68_003E__SyncListSpacer;

		public static Func<LocString, string> _003C69_003E__Describe;

		public static Func<LocString, bool> _003C70_003E__IsSteamBranchMismatch;

		public static GuiInputEventHandler _003C71_003E__OnDescriptionGuiInput;

		public static Action _003C72_003E__CancelDescriptionEdit;

		public static Action _003C73_003E__SaveDescriptionEdit;

		public static Action _003C74_003E__ToggleSelected;

		public static Action _003C75_003E__OpenSelectedModFolder;

		public static Action _003C76_003E__FillWorkshopAlias;

		public static ItemSelectedEventHandler _003C77_003E__OnDetailGroupSelected;

		public static Action _003C78_003E__UpdateDetails;

		public static Action _003C79_003E__ToggleFabMenu;

		public static Action _003C80_003E__PlaceFab;

		public static Action _003C81_003E__StyleFab;

		public static Action _003C82_003E__UpdateFab;

		public static Action _003C83_003E__OnDownloadsChanged;

		public static Action _003C84_003E__CloseFabMenu;

		public static Action _003C85_003E__OpenDownloads;

		public static Action _003C86_003E__OpenLocalModImport;

		public static Action _003C87_003E__CloseGameAndOpenLogFolder;

		public static Action _003C88_003E__StartTour;

		public static Action<ulong, string> _003C89_003E__OpenWorkshopItemPage;

		public static Action<ulong, string, string> _003C90_003E__RequestWorkshopRemoval;

		public static Func<ulong, bool> _003C91_003E__CanRemoveWorkshopItem;

		public static Action _003C92_003E__ToggleFilters;

		public static Action _003C93_003E__ApplyFilterCollapse;

		public static Action _003C94_003E__ClearFilterChoices;

		public static Action _003C95_003E__OnFriendModSyncChanged;

		public static Func<LaunchProfile, PlanResult> _003C96_003E__ApplyFriendProfile;

		public static Action _003C97_003E__ToggleGroupView;

		public static Action _003C98_003E__UpdateGroupViewToggle;

		public static Action _003C99_003E__UpdateFoldAllButton;

		public static Action _003C100_003E__UpdateStickyHeader;

		public static IdPressedEventHandler _003C101_003E__OnFolderMenuIdPressed;

		public static IdPressedEventHandler _003C102_003E__OnFolderMoveMenuIdPressed;

		public static GuiInputEventHandler _003C103_003E__OnStickyHeaderInput;

		public static Action _003C104_003E__CancelInlineRename;

		public static Action _003C105_003E__UpdateGroupRowHeight;

		public static Action _003C106_003E__CommitGroupName;

		public static Action _003C107_003E__UpdateLoadErrorActions;

		public static Action _003C108_003E__ContinueAfterLoadError;

		public static Action _003C109_003E__RestartAfterLoadError;

		public static Action _003C110_003E__EnableRandomMod;

		public static Action _003C111_003E__StartVanilla;

		public static Action _003C112_003E__ResetPlan;

		public static Action _003C113_003E__StartLoading;

		public static Action _003C114_003E__RestartGame;

		public static Action _003C115_003E__AutoSortPlan;

		public static Action _003C116_003E__PumpOneFrame;

		public static Action _003C117_003E__PumpWorkshopScan;

		public static Action _003C118_003E__UpdateCloudSyncStatus;

		public static Action _003C119_003E__OpenCollectionImport;

		public static Action _003C120_003E__DeleteActiveProfile;

		public static Action _003C121_003E__OpenFriendModSync;

		public static Action _003C122_003E__OpenModListTransfer;

		public static Func<LaunchProfile, bool, PlanResult> _003C123_003E__SaveImportedProfile;

		public static Action _003C124_003E__CloseSettings;

		public static Action _003C125_003E__UpdateSettingsFontValue;

		public static Action _003C126_003E__UpdateSettingsSectionChrome;

		public static Action<bool> _003C127_003E__OnRandomModVisibilityToggled;

		public static Action<bool> _003C128_003E__OnVanillaLaunchVisibilityToggled;

		public static ValueChangedEventHandler _003C129_003E__OnFontScaleChanged;

		public static Action _003C130_003E__UpdateAffectsToggleText;

		public static ToggledEventHandler _003C131_003E__OnAffectsColumnToggled;

		public static Action _003C132_003E__UpdateAutoUnlockToggleText;

		public static ToggledEventHandler _003C133_003E__OnAutoUnlockToggled;

		public static Action _003C134_003E__UpdateCloudSyncToggleText;

		public static ToggledEventHandler _003C135_003E__OnCloudSyncToggled;

		public static Action _003C136_003E__UpdateModIsolationToggleText;

		public static ToggledEventHandler _003C137_003E__OnModIsolationToggled;

		public static Action _003C138_003E__UpdateKeepDisabledToggleText;

		public static ToggledEventHandler _003C139_003E__OnKeepDisabledToggled;

		public static Action _003C140_003E__FinishBackgroundFade;

		public static Func<TourStep, bool> _003C141_003E__TourStepAvailable;

		public static Action _003C142_003E__RefreshTourSpot;

		public static Action _003C143_003E__EndTour;

		public static Action _003C144_003E__TourForward;

		public static Action<Rect2> _003C145_003E__SetTourSpot;

		public static Action _003C146_003E__ShowTourFilters;

		public static Action _003C147_003E__RestoreTourFilters;

		public static Action _003C148_003E__ShowTourButtons;

		public static Action _003C149_003E__RestoreTourButtons;

		public static Action _003C150_003E__ShowCollectionSample;

		public static Action _003C151_003E__Close;

		public static Action _003C152_003E__ShowFriendSample;

		public static Action _003C153_003E__ExportSelectedTranslations;

		public static Action _003C154_003E__TranslateSelectedMod;

		public static Action _003C155_003E__ToggleSelectedTranslation;

		public static Action _003C156_003E__ImportSelectedTranslation;

		public static Action _003C157_003E__OpenSelectedTranslations;

		public static Action _003C158_003E__UndoLastStep;

		public static Action _003C159_003E__OnWorkshopChanged;

		public static Action _003C160_003E__OpenSelectedWorkshopPage;

		public static Action _003C161_003E__DownloadSelectedWorkshopUpdate;

		public static Action _003C162_003E__SwitchSelectedWorkshopBranch;

		public static Func<WorkshopTag, string> _003C163_003E__TagLabel;
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static Func<LaunchEntry, Mod> _003C_003E9__3_1;

		public static Func<LaunchEntry, bool> _003C_003E9__8_0;

		public static Func<LaunchEntry, string> _003C_003E9__8_1;

		public static Func<LaunchEntry, bool> _003C_003E9__8_2;

		public static Func<LaunchEntry, bool> _003C_003E9__8_3;

		public static Predicate<AiProviderPreset> _003C_003E9__39_1;

		public static Func<string, bool> _003C_003E9__56_0;

		public static Func<KeyValuePair<string, byte[]>, string> _003C_003E9__57_0;

		public static Func<KeyValuePair<string, byte[]>, bool> _003C_003E9__57_1;

		public static Func<byte[], IEnumerable<string>> _003C_003E9__58_0;

		public static Action _003C_003E9__79_0;

		public static Func<LaunchEntry, string> _003C_003E9__85_1;

		public static Action _003C_003E9__87_0;

		public static Action _003C_003E9__87_1;

		public static Func<WorkshopChangeNote, uint> _003C_003E9__88_0;

		public static Func<(ulong ItemId, WorkshopChangeNote Note), ulong> _003C_003E9__89_0;

		public static Func<(ulong ItemId, WorkshopChangeNote Note), string> _003C_003E9__89_2;

		public static Action<Dictionary<string, TranslationFileException>> _003C_003E9__89_4;

		public static Action<string> _003C_003E9__109_0;

		public static Func<bool> _003C_003E9__109_1;

		public static Func<ScrollContainer?> _003C_003E9__109_2;

		public static Action _003C_003E9__109_3;

		public static Func<Control?> _003C_003E9__109_4;

		public static Action _003C_003E9__109_6;

		public static Func<Control?> _003C_003E9__109_7;

		public static Func<bool> _003C_003E9__109_8;

		public static Predicate<TreeBinding> _003C_003E9__115_0;

		public static Func<TreeBinding, bool> _003C_003E9__122_0;

		public static Func<Button, bool> _003C_003E9__124_0;

		public static Func<LaunchEntry, bool> _003C_003E9__124_1;

		public static Predicate<string> _003C_003E9__124_2;

		public static Predicate<Button> _003C_003E9__124_3;

		public static Func<PromptKey, string> _003C_003E9__138_1;

		public static Func<Prompt, string> _003C_003E9__138_0;

		public static Func<CanonicalLaunchEntry, CanonicalLaunchEntry> _003C_003E9__335_0;

		public static Func<LaunchEntry, bool> _003C_003E9__335_1;

		public static Func<LaunchProfile, bool> _003C_003E9__335_2;

		public static Func<int, int> _003C_003E9__335_3;

		public static Action _003C_003E9__338_3;

		public static Action _003C_003E9__338_4;

		public static Func<BadgeTone> _003C_003E9__338_6;

		public static Action _003C_003E9__338_7;

		public static TextChangedEventHandler _003C_003E9__340_0;

		public static Action _003C_003E9__340_1;

		public static Action _003C_003E9__340_2;

		public static Action _003C_003E9__340_3;

		public static Action _003C_003E9__340_4;

		public static Action _003C_003E9__340_5;

		public static Action _003C_003E9__340_6;

		public static Func<IGrouping<LaunchEntry, ModConflict>, string> _003C_003E9__358_0;

		public static Predicate<TreeBinding> _003C_003E9__360_0;

		public static Func<TreeBinding, LaunchEntry> _003C_003E9__371_0;

		public static Func<TreeBinding, bool> _003C_003E9__379_0;

		public static Func<TreeBinding, LaunchEntry> _003C_003E9__379_1;

		public static Func<Mod, bool> _003C_003E9__395_0;

		public static Func<LaunchEntry, string?> _003C_003E9__395_1;

		public static Func<Mod, string?> _003C_003E9__395_2;

		public static Func<(LaunchEntry Gone, Mod Arrival), LaunchEntry> _003C_003E9__395_3;

		public static Func<string, string, string> _003C_003E9__395_4;

		public static Func<LaunchEntry, string> _003C_003E9__399_0;

		public static Func<LaunchEntry, string> _003C_003E9__400_0;

		public static Func<LaunchEntry, string> _003C_003E9__401_0;

		public static Func<TreeBinding, bool> _003C_003E9__404_0;

		public static TextChangedEventHandler _003C_003E9__407_1;

		public static TextSubmittedEventHandler _003C_003E9__407_2;

		public static Action _003C_003E9__407_3;

		public static Func<ModOptionSpec, bool> _003C_003E9__419_0;

		public static Func<ModOptionSpec, bool> _003C_003E9__419_1;

		public static Func<string, string> _003C_003E9__419_3;

		public static Action _003C_003E9__419_4;

		public static Func<ModOptionSpec, bool> _003C_003E9__419_5;

		public static Func<(string Key, string Value), string> _003C_003E9__431_0;

		public static Func<LocString, (Color, string)> _003C_003E9__433_0;

		public static Func<ModConflict, ModIncompatibility> _003C_003E9__433_2;

		public static Func<(Color Color, string Text), string> _003C_003E9__433_1;

		public static Func<ModIncompatibility, string> _003C_003E9__436_0;

		public static Func<ModIncompatibility, string> _003C_003E9__437_0;

		public static Func<ModIncompatibility, string> _003C_003E9__437_4;

		public static Func<string, bool> _003C_003E9__437_5;

		public static Func<ModConflict, ModIncompatibility> _003C_003E9__437_6;

		public static Action _003C_003E9__456_0;

		public static GuiInputEventHandler _003C_003E9__456_1;

		public static Action _003C_003E9__456_2;

		public static Action _003C_003E9__456_3;

		public static GuiInputEventHandler _003C_003E9__461_0;

		public static GuiInputEventHandler _003C_003E9__461_1;

		public static ValueChangedEventHandler _003C_003E9__461_2;

		public static GuiInputEventHandler _003C_003E9__471_0;

		public static Func<Button, bool> _003C_003E9__471_2;

		public static GuiInputEventHandler _003C_003E9__485_0;

		public static Action _003C_003E9__492_0;

		public static Action _003C_003E9__533_0;

		public static Func<TreeItem, bool> _003C_003E9__534_0;

		public static Func<TreeItem, bool> _003C_003E9__535_0;

		public static Func<(string Name, int Depth), string> _003C_003E9__550_0;

		public static Comparison<(int Position, int Order, string? Folder, int Index)> _003C_003E9__552_7;

		public static Func<(string Name, int Depth), string> _003C_003E9__552_3;

		public static Func<LaunchEntry, bool> _003C_003E9__553_0;

		public static Func<LaunchEntry, bool> _003C_003E9__556_0;

		public static Func<LaunchEntry, bool> _003C_003E9__557_0;

		public static Func<LaunchEntry, bool> _003C_003E9__557_1;

		public static Func<TreeBinding, bool> _003C_003E9__569_0;

		public static Func<TreeBinding, bool> _003C_003E9__570_0;

		public static Func<TreeBinding, LaunchEntry> _003C_003E9__575_0;

		public static Func<LaunchEntry, bool> _003C_003E9__575_2;

		public static Func<LaunchEntry, bool> _003C_003E9__575_3;

		public static Func<LaunchEntry, bool> _003C_003E9__575_4;

		public static Func<LaunchEntry, string> _003C_003E9__575_7;

		public static Func<LaunchEntry, bool> _003C_003E9__575_8;

		public static Func<TreeItem, bool> _003C_003E9__583_0;

		public static Func<TreeItem, bool> _003C_003E9__583_1;

		public static Action _003C_003E9__587_0;

		public static Action _003C_003E9__587_1;

		public static Action _003C_003E9__587_2;

		public static ValueChangedEventHandler _003C_003E9__587_3;

		public static TextSubmittedEventHandler _003C_003E9__589_0;

		public static Action _003C_003E9__589_1;

		public static GuiInputEventHandler _003C_003E9__589_2;

		public static Action _003C_003E9__599_0;

		public static TextSubmittedEventHandler _003C_003E9__599_1;

		public static Action _003C_003E9__599_2;

		public static Func<LaunchEntry, bool> _003C_003E9__600_0;

		public static Action _003C_003E9__600_1;

		public static Func<(string Name, int Depth), string> _003C_003E9__604_0;

		public static Func<LaunchEntry, string> _003C_003E9__609_0;

		public static Func<string, bool> _003C_003E9__609_1;

		public static Func<LaunchEntry, bool> _003C_003E9__609_4;

		public static Func<LaunchEntry, string> _003C_003E9__614_0;

		public static Func<TreeBinding, LaunchEntry> _003C_003E9__617_0;

		public static Func<TreeBinding, bool> _003C_003E9__617_1;

		public static Func<LaunchEntry, string> _003C_003E9__619_0;

		public static Func<LaunchEntry, string> _003C_003E9__619_1;

		public static ToggledEventHandler _003C_003E9__627_1;

		public static Func<ModConflict, string> _003C_003E9__630_0;

		public static Func<ModConflict, bool> _003C_003E9__630_1;

		public static Func<ModConflict, string> _003C_003E9__630_2;

		public static Func<ModConflict, string> _003C_003E9__630_3;

		public static Func<LaunchEntry, bool> _003C_003E9__631_0;

		public static Func<CanonicalLaunchEntry, CanonicalLaunchEntry> _003C_003E9__633_0;

		public static Func<LocString, bool> _003C_003E9__650_0;

		public static TextSubmittedEventHandler _003C_003E9__672_0;

		public static TextChangedEventHandler _003C_003E9__672_1;

		public static GuiInputEventHandler _003C_003E9__672_2;

		public static Action _003C_003E9__672_3;

		public static Action _003C_003E9__672_4;

		public static Action _003C_003E9__673_0;

		public static Action _003C_003E9__673_1;

		public static Func<ProfileMod, bool> _003C_003E9__674_0;

		public static Func<LaunchProfile, bool> _003C_003E9__676_0;

		public static Func<LaunchEntry, string> _003C_003E9__677_0;

		public static Func<LaunchEntry, bool> _003C_003E9__678_0;

		public static Func<LaunchEntry, string> _003C_003E9__678_1;

		public static Func<LaunchEntry, bool> _003C_003E9__678_2;

		public static Func<LaunchEntry, string> _003C_003E9__678_3;

		public static Func<ProfileMod, string> _003C_003E9__678_5;

		public static Func<IReadOnlyList<LaunchProfile>> _003C_003E9__683_0;

		public static Func<ProfileMod, string> _003C_003E9__686_0;

		public static Action _003C_003E9__691_1;

		public static Action _003C_003E9__691_2;

		public static Action _003C_003E9__692_0;

		public static Action _003C_003E9__692_1;

		public static Action _003C_003E9__692_2;

		public static Action _003C_003E9__692_3;

		public static Action<LauncherBackground.Result?> _003C_003E9__737_0;

		public static Func<MlmLocaleInfo, string> _003C_003E9__760_0;

		public static Action _003C_003E9__798_0;

		public static Func<Control?> _003C_003E9__799_0;

		public static Func<JoyButton, bool> _003C_003E9__799_1;

		public static Action _003C_003E9__800_0;

		public static Action _003C_003E9__815_0;

		public static Action _003C_003E9__815_1;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_2;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_3;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_4;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_5;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_6;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_7;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_8;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_9;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_10;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_11;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_12;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_13;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_14;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_15;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_16;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_17;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_18;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_19;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_20;

		public static Func<(string Name, object? Value)[]> _003C_003E9__815_21;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_22;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_23;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_24;

		public static Func<(string Name, object? Value)[]> _003C_003E9__815_25;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_26;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_27;

		public static Action _003C_003E9__815_28;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_29;

		public static Action _003C_003E9__815_30;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_31;

		public static Action _003C_003E9__815_32;

		public static Func<IEnumerable<Control?>> _003C_003E9__815_33;

		public static Action _003C_003E9__815_34;

		public static Func<LaunchEntry, bool> _003C_003E9__826_0;

		public static Func<LaunchEntry, ulong> _003C_003E9__826_1;

		public static Func<IGrouping<ulong, LaunchEntry>, bool> _003C_003E9__826_2;

		public static Action _003C_003E9__828_0;

		public static Func<LaunchEntry, bool> _003C_003E9__830_0;

		public static Func<LaunchEntry, bool> _003C_003E9__830_1;

		public static Func<LaunchEntry, bool> _003C_003E9__830_4;

		public static Func<LaunchEntry, bool> _003C_003E9__830_3;

		public static Action _003C_003E9__841_2;

		public static Func<LaunchEntry, LaunchEntry> _003C_003E9__855_0;

		public static Func<LaunchEntry, bool> _003C_003E9__855_1;

		public static Func<LaunchEntry, Mod> _003C_003E9__888_0;

		public static Action _003C_003E9__888_1;

		public static Action _003C_003E9__899_0;

		public static ItemSelectedEventHandler _003C_003E9__899_1;

		public static Func<LaunchEntry, bool> _003C_003E9__910_0;

		internal Mod _003CAdoptDetectedMod_003Eb__3_1(LaunchEntry entry)
		{
			return entry.Mod;
		}

		internal bool _003CInstalledModsById_003Eb__8_0(LaunchEntry entry)
		{
			string text = entry.Mod.manifest?.id;
			if (text != null)
			{
				return text.Length > 0;
			}
			return false;
		}

		internal string _003CInstalledModsById_003Eb__8_1(LaunchEntry entry)
		{
			return entry.Id;
		}

		internal bool _003CInstalledModsById_003Eb__8_2(LaunchEntry copy)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			return (int)copy.Mod.modSource != 2;
		}

		internal bool _003CInstalledModsById_003Eb__8_3(LaunchEntry copy)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			return (int)copy.Mod.state == 1;
		}

		internal bool _003CBuildSettingsAiView_003Eb__39_1(AiProviderPreset p)
		{
			return !p.IsPreset;
		}

		internal bool _003CReadVanillaTables_003Eb__56_0(string f)
		{
			return f.EndsWith(".json", StringComparison.Ordinal);
		}

		internal string _003CAddManualTranslationReferences_003Eb__57_0(KeyValuePair<string, byte[]> p)
		{
			return p.Key.Substring(p.Key.IndexOf('/') + 1);
		}

		internal bool _003CAddManualTranslationReferences_003Eb__57_1(KeyValuePair<string, byte[]> p)
		{
			return p.Key.StartsWith("eng/", StringComparison.Ordinal);
		}

		internal IEnumerable<string> _003CTranslateSelectedMod_003Eb__58_0(byte[] b)
		{
			return ModTranslationFiles.Parse(b).Values;
		}

		internal void _003CBuildChangeNotesBanner_003Eb__79_0()
		{
			_changeNotesDismissed = true;
			UpdateChangeNotesChrome();
			Button? start = _start;
			if (start != null)
			{
				((Control)start).GrabFocus();
			}
		}

		internal string _003CWorkshopItemName_003Eb__85_1(LaunchEntry entry)
		{
			return entry.DisplayName;
		}

		internal void _003CBuildChangeNotesDetails_003Eb__87_0()
		{
			if (_selected != null)
			{
				ulong num = WorkshopUpdates.ItemIdFor(_selected.Mod);
				if (num != 0)
				{
					OpenWorkshopChangeNotesPage(num);
				}
			}
		}

		internal void _003CBuildChangeNotesDetails_003Eb__87_1()
		{
			if (_selected != null)
			{
				ulong num = WorkshopUpdates.ItemIdFor(_selected.Mod);
				if (num != 0)
				{
					WorkshopChangeNoteTranslations.Activate(num);
				}
			}
		}

		internal uint _003CUpdateChangeNotesDetails_003Eb__88_0(WorkshopChangeNote note)
		{
			return note.Time;
		}

		internal ulong _003CTranslateChangeNotes_003Eb__89_0((ulong ItemId, WorkshopChangeNote Note) entry)
		{
			return entry.ItemId;
		}

		internal string _003CTranslateChangeNotes_003Eb__89_2((ulong ItemId, WorkshopChangeNote Note) entry)
		{
			return entry.Note.Text;
		}

		internal void _003CTranslateChangeNotes_003Eb__89_4(Dictionary<string, TranslationFileException> _)
		{
		}

		internal void _003CStartControllerNavigation_003Eb__109_0(string source)
		{
			GD.Print("[ModLaunchManager] launcher controller source: " + source + ".");
		}

		internal bool _003CStartControllerNavigation_003Eb__109_1()
		{
			return SteamControllerCount() > 0;
		}

		internal ScrollContainer? _003CStartControllerNavigation_003Eb__109_2()
		{
			return _detailScroll;
		}

		internal void _003CStartControllerNavigation_003Eb__109_3()
		{
			if (_settingsSection == SettingsSection.Root)
			{
				CloseSettings();
			}
			else
			{
				ShowSettingsSection(SettingsSection.Root);
			}
		}

		internal Control? _003CStartControllerNavigation_003Eb__109_4()
		{
			Control? settingsRootView = _settingsRootView;
			if (settingsRootView == null)
			{
				return null;
			}
			return (Control?)(object)((IEnumerable)((Node)settingsRootView).GetChildren(false)).OfType<Button>().FirstOrDefault();
		}

		internal void _003CStartControllerNavigation_003Eb__109_6()
		{
		}

		internal Control? _003CStartControllerNavigation_003Eb__109_7()
		{
			return (Control?)(object)_restartAfterError;
		}

		internal bool _003CStartControllerNavigation_003Eb__109_8()
		{
			return _pumpActive;
		}

		internal bool _003CFocusModList_003Eb__115_0(TreeBinding binding)
		{
			return binding.Entry == _selected;
		}

		internal bool _003COnTreeSideways_003Eb__122_0(TreeBinding binding)
		{
			return binding.Entry == _selected;
		}

		internal bool _003CCycleFilter_003Eb__124_0(Button chip)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)chip) && ((CanvasItem)chip).IsVisibleInTree())
			{
				return !((BaseButton)chip).Disabled;
			}
			return false;
		}

		internal bool _003CCycleFilter_003Eb__124_1(LaunchEntry entry)
		{
			return string.IsNullOrWhiteSpace(entry.Group);
		}

		internal bool _003CCycleFilter_003Eb__124_2(string group)
		{
			return string.Equals(group, _groupFilter, StringComparison.OrdinalIgnoreCase);
		}

		internal bool _003CCycleFilter_003Eb__124_3(Button chip)
		{
			return ((BaseButton)chip).ButtonPressed;
		}

		internal string _003CUpdateControllerPrompts_003Eb__138_0(Prompt prompt)
		{
			return string.Join("+", prompt.Keys.Select((PromptKey key) => PromptGlyph(key).ToString())) + ":" + prompt.Text;
		}

		internal string _003CUpdateControllerPrompts_003Eb__138_1(PromptKey key)
		{
			return PromptGlyph(key).ToString();
		}

		internal CanonicalLaunchEntry _003CShow_003Eb__335_0(CanonicalLaunchEntry entry)
		{
			return entry.Clone();
		}

		internal bool _003CShow_003Eb__335_1(LaunchEntry entry)
		{
			return !entry.IsManager;
		}

		internal bool _003CShow_003Eb__335_2(LaunchProfile profile)
		{
			return string.Equals(profile.Name, _profiles.Active, StringComparison.Ordinal);
		}

		internal int _003CShow_003Eb__335_3(int revision)
		{
			return NewTourSteps(revision).Count;
		}

		internal void _003CBuildHeader_003Eb__338_3()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			((Control)_themeSelector).CustomMinimumSize = new Vector2((float)(UiKit.Palette.IsLiquidGlass ? 136 : 110), 32f);
		}

		internal void _003CBuildHeader_003Eb__338_4()
		{
			Defer(FlushPendingBackgroundChange);
		}

		internal BadgeTone _003CBuildHeader_003Eb__338_6()
		{
			return _workshopTone;
		}

		internal void _003CBuildHeader_003Eb__338_7()
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			_settingsButton.Icon = (Texture2D)(object)UiGlyphs.Gear(16, UiKit.Palette.IsLiquidGlass ? UiKit.Text : UiKit.Accent);
		}

		internal void _003CBuildModList_003Eb__340_0(string _)
		{
			QueueRebuild();
		}

		internal void _003CBuildModList_003Eb__340_1()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			_moveUp.Text = (UiKit.Palette.IsLiquidGlass ? string.Empty : "▲");
			_moveUp.Icon = (Texture2D)(object)(UiKit.Palette.IsLiquidGlass ? UiGlyphs.Chevron(14, UiKit.Text, 1.7f, up: true) : null);
			_moveUp.IconAlignment = (HorizontalAlignment)1;
		}

		internal void _003CBuildModList_003Eb__340_2()
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			_moveDown.Text = (UiKit.Palette.IsLiquidGlass ? string.Empty : "▼");
			_moveDown.Icon = (Texture2D)(object)(UiKit.Palette.IsLiquidGlass ? UiGlyphs.Chevron(14, UiKit.Text, 1.7f) : null);
			_moveDown.IconAlignment = (HorizontalAlignment)1;
		}

		internal void _003CBuildModList_003Eb__340_3()
		{
			SetAllEnabled(enabled: true);
		}

		internal void _003CBuildModList_003Eb__340_4()
		{
			SetAllEnabled(enabled: false);
		}

		internal void _003CBuildModList_003Eb__340_5()
		{
			_003C_003Ec__DisplayClass340_0 obj = new _003C_003Ec__DisplayClass340_0
			{
				rebuilding = _rebuildingTree,
				pointer = (Input.IsMouseButtonPressed((MouseButton)1) || Input.IsMouseButtonPressed((MouseButton)2))
			};
			if (!obj.rebuilding)
			{
				_listInputAt = Time.GetTicksMsec();
			}
			Defer(delegate
			{
				if (_modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
				{
					TreeItem selected = ((Tree)_modTree).GetSelected();
					if (selected != null)
					{
						RevealBelowPinnedTitle(selected);
						if (!obj.rebuilding && !obj.pointer)
						{
							RevealAboveFab(selected);
						}
					}
				}
			});
		}

		internal void _003CBuildModList_003Eb__340_6()
		{
			((Window)_groupMenu).AddThemeFontOverride(StringName.op_Implicit("font"), (Font)(object)UiKit.UiFont);
			((Window)_groupMenu).AddThemeFontOverride(StringName.op_Implicit("font_separator"), (Font)(object)UiKit.UiFont);
		}

		internal string _003CPlanStatus_003Eb__358_0(IGrouping<LaunchEntry, ModConflict> conflict)
		{
			return conflict.Key.DisplayName;
		}

		internal bool _003CUpdateListChrome_003Eb__360_0(TreeBinding binding)
		{
			return binding.Entry == _selected;
		}

		internal LaunchEntry _003CMoveSelected_003Eb__371_0(TreeBinding binding)
		{
			return binding.Entry;
		}

		internal bool _003CSetAllEnabled_003Eb__379_0(TreeBinding binding)
		{
			if (_groupListView)
			{
				return !InFoldedFolder(binding.Item);
			}
			return true;
		}

		internal LaunchEntry _003CSetAllEnabled_003Eb__379_1(TreeBinding binding)
		{
			return binding.Entry;
		}

		internal bool _003COnLocalModsChanged_003Eb__395_0(Mod mod)
		{
			_003C_003Ec__DisplayClass395_0 CS_0024_003C_003E8__locals1 = new _003C_003Ec__DisplayClass395_0
			{
				mod = mod
			};
			return !_plan.Entries.Any((LaunchEntry entry) => PhysicalModIdentity.Same(entry.Mod, CS_0024_003C_003E8__locals1.mod));
		}

		internal string? _003COnLocalModsChanged_003Eb__395_1(LaunchEntry entry)
		{
			return entry.Mod.manifest?.id;
		}

		internal string? _003COnLocalModsChanged_003Eb__395_2(Mod mod)
		{
			return mod.manifest?.id;
		}

		internal LaunchEntry _003COnLocalModsChanged_003Eb__395_3((LaunchEntry Gone, Mod Arrival) move)
		{
			return move.Gone;
		}

		internal string _003COnLocalModsChanged_003Eb__395_4(string first, string second)
		{
			return MlmLocManager.Get("common.twoSentences", ("first", first), ("second", second));
		}

		internal string _003CRemoveLocalEntries_003Eb__399_0(LaunchEntry entry)
		{
			return entry.Id + " (" + entry.Mod.path + ")";
		}

		internal string _003CAddDiscoveredLocalMods_003Eb__400_0(LaunchEntry entry)
		{
			return entry.Id;
		}

		internal string _003CNameList_003Eb__401_0(LaunchEntry entry)
		{
			return entry.DisplayName;
		}

		internal bool _003CScrollSelectionIntoView_003Eb__404_0(TreeBinding candidate)
		{
			if (_selected != null)
			{
				return candidate.Entry == _selected;
			}
			return false;
		}

		internal void _003CBuildDetails_003Eb__407_1(string _)
		{
			UpdateOrganizationSaveState();
		}

		internal void _003CBuildDetails_003Eb__407_2(string _)
		{
			SaveSelectedOrganization();
		}

		internal void _003CBuildDetails_003Eb__407_3()
		{
			SaveSelectedOrganization();
		}

		internal bool _003CRebuildOptionRows_003Eb__419_0(ModOptionSpec spec)
		{
			return !spec.Advanced;
		}

		internal bool _003CRebuildOptionRows_003Eb__419_1(ModOptionSpec spec)
		{
			return spec.Advanced;
		}

		internal string _003CRebuildOptionRows_003Eb__419_3(string key)
		{
			return key;
		}

		internal void _003CRebuildOptionRows_003Eb__419_4()
		{
			_optionsShowAdvanced = !_optionsShowAdvanced;
			RefreshOptionRows();
		}

		internal bool _003CRebuildOptionRows_003Eb__419_5(ModOptionSpec spec)
		{
			return !spec.Declared;
		}

		internal string _003CUpdateDetailMeta_003Eb__431_0((string Key, string Value) pair)
		{
			return pair.Key + "=" + pair.Value;
		}

		internal (Color, string) _003CUpdateWarningDetails_003Eb__433_0(LocString error)
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			return (ModErrorText.IsSteamBranchMismatch(error) ? UiKit.Danger : UiKit.Warning, ModErrorText.Describe(error));
		}

		internal ModIncompatibility _003CUpdateWarningDetails_003Eb__433_2(ModConflict rule)
		{
			return rule.Rule;
		}

		internal string _003CUpdateWarningDetails_003Eb__433_1((Color Color, string Text) warning)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return Colored(warning.Color, "•  " + Escape(warning.Text));
		}

		internal string _003CConflictReason_003Eb__436_0(ModIncompatibility rule)
		{
			return rule.Reason;
		}

		internal string _003CBuildIncompatibilityReport_003Eb__437_0(ModIncompatibility rule)
		{
			return rule.Id;
		}

		internal string _003CBuildIncompatibilityReport_003Eb__437_4(ModIncompatibility rule)
		{
			return rule.RangeText;
		}

		internal bool _003CBuildIncompatibilityReport_003Eb__437_5(string range)
		{
			return range.Length > 0;
		}

		internal ModIncompatibility _003CBuildIncompatibilityReport_003Eb__437_6(ModConflict conflict)
		{
			return conflict.Rule;
		}

		internal void _003CBuildFab_003Eb__456_0()
		{
			ReturnFab();
		}

		internal void _003CBuildFab_003Eb__456_1(InputEvent input)
		{
			double? num = WheelPages(input);
			if (!num.HasValue)
			{
				return;
			}
			double valueOrDefault = num.GetValueOrDefault();
			if (_modTree != null)
			{
				VScrollBar val = ListScroll((Tree)(object)_modTree);
				if (val != null && ((CanvasItem)val).Visible)
				{
					_listInputAt = Time.GetTicksMsec();
					((Range)val).Value = ((Range)val).Value + ((Range)val).Page * valueOrDefault;
					((Control)_fab).AcceptEvent();
				}
			}
		}

		internal void _003CBuildFab_003Eb__456_2()
		{
			ReturnFab();
		}

		internal void _003CBuildFab_003Eb__456_3()
		{
			Defer(PlaceFab);
		}

		internal void _003CWatchListScrolling_003Eb__461_0(InputEvent input)
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			_003C_003Ec__DisplayClass461_0 CS_0024_003C_003E8__locals5 = new _003C_003Ec__DisplayClass461_0
			{
				input = input
			};
			if (!WheelPages(CS_0024_003C_003E8__locals5.input).HasValue && !(CS_0024_003C_003E8__locals5.input is InputEventScreenDrag))
			{
				InputEvent obj = CS_0024_003C_003E8__locals5.input;
				InputEventMouseMotion val = (InputEventMouseMotion)(object)((obj is InputEventMouseMotion) ? obj : null);
				if (val == null || (int)((InputEventMouse)val).ButtonMask == 0)
				{
					InputEvent obj2 = CS_0024_003C_003E8__locals5.input;
					InputEventKey val2 = (InputEventKey)(object)((obj2 is InputEventKey) ? obj2 : null);
					if (val2 == null || !val2.Pressed || !new string[6] { "ui_up", "ui_down", "ui_page_up", "ui_page_down", "ui_home", "ui_end" }.Any((string action) => CS_0024_003C_003E8__locals5.input.IsAction(StringName.op_Implicit(action), false)))
					{
						return;
					}
				}
			}
			_listInputAt = Time.GetTicksMsec();
		}

		internal void _003CWatchListScrolling_003Eb__461_1(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (!(input is InputEventMouseButton))
			{
				InputEventMouseMotion val = (InputEventMouseMotion)(object)((input is InputEventMouseMotion) ? input : null);
				if (val == null || (int)((InputEventMouse)val).ButtonMask == 0)
				{
					return;
				}
			}
			_listInputAt = Time.GetTicksMsec();
		}

		internal void _003CWatchListScrolling_003Eb__461_2(double _)
		{
			if (!_rebuildingTree && Time.GetTicksMsec() - _listInputAt <= 400)
			{
				TuckFab();
			}
		}

		internal void _003COpenFabMenu_003Eb__471_0(InputEvent input)
		{
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && val.Pressed)
			{
				CloseFabMenu();
			}
		}

		internal bool _003COpenFabMenu_003Eb__471_2(Button button)
		{
			return !((BaseButton)button).Disabled;
		}

		internal void _003CBuildFilterSummary_003Eb__485_0(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
			if (val != null && val.Pressed && (long)val.ButtonIndex == 1)
			{
				ToggleFilters();
			}
		}

		internal void _003CStartFriendModSync_003Eb__492_0()
		{
			FriendModSync.Poll(0.5);
		}

		internal void _003CBuildFoldAllButton_003Eb__533_0()
		{
			SetAllFoldersCollapsed(AnyFolderOpen());
		}

		internal bool _003CAnyFolderOpen_003Eb__534_0(TreeItem header)
		{
			if (!header.Collapsed)
			{
				return header.GetFirstChild() != null;
			}
			return false;
		}

		internal bool _003CUpdateFoldAllButton_003Eb__535_0(TreeItem header)
		{
			return header.Collapsed;
		}

		internal string _003CSiblingGroups_003Eb__550_0((string Name, int Depth) node)
		{
			return node.Name;
		}

		internal int _003CAddGroupFolders_003Eb__552_7((int Position, int Order, string? Folder, int Index) left, (int Position, int Order, string? Folder, int Index) right)
		{
			if (left.Position == right.Position)
			{
				return left.Order.CompareTo(right.Order);
			}
			return left.Position.CompareTo(right.Position);
		}

		internal string _003CAddGroupFolders_003Eb__552_3((string Name, int Depth) node)
		{
			return node.Name;
		}

		internal bool _003CAddGroupFolderHeader_003Eb__553_0(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal bool _003CFolderStatus_003Eb__556_0(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal bool _003CFolderToggleIcon_003Eb__557_0(LaunchEntry entry)
		{
			return entry.CanToggle;
		}

		internal bool _003CFolderToggleIcon_003Eb__557_1(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal bool _003CReselectVisibleRow_003Eb__569_0(TreeBinding binding)
		{
			return binding.Entry == _selected;
		}

		internal bool _003CMarkFoldedSelection_003Eb__570_0(TreeBinding binding)
		{
			return binding.Entry == _selected;
		}

		internal LaunchEntry _003CToggleFolder_003Eb__575_0(TreeBinding binding)
		{
			return binding.Entry;
		}

		internal bool _003CToggleFolder_003Eb__575_2(LaunchEntry entry)
		{
			return entry.CanToggle;
		}

		internal bool _003CToggleFolder_003Eb__575_3(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal bool _003CToggleFolder_003Eb__575_4(LaunchEntry entry)
		{
			return !entry.Enabled;
		}

		internal string _003CToggleFolder_003Eb__575_7(LaunchEntry entry)
		{
			return entry.Key;
		}

		internal bool _003CToggleFolder_003Eb__575_8(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal bool _003CShowFolderMenu_003Eb__583_0(TreeItem header)
		{
			if (!header.Collapsed)
			{
				return header.GetFirstChild() != null;
			}
			return false;
		}

		internal bool _003CShowFolderMenu_003Eb__583_1(TreeItem header)
		{
			return header.Collapsed;
		}

		internal void _003CBuildStickyFolderHeader_003Eb__587_0()
		{
			((Control)_stickyHeader).TooltipText = MlmLocManager.Get("groups.folder.sticky.tooltip");
		}

		internal void _003CBuildStickyFolderHeader_003Eb__587_1()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			StyleBoxFlat val = UiKit.Box(UiKit.PinnedRowSurface(UiKit.Accent, 0.09f), UiKit.Palette.Border, 0, 0, 0, 0);
			val.BorderWidthBottom = 1;
			val.ShadowColor = UiKit.Palette.SurfaceShadow;
			val.ShadowSize = 4;
			val.ShadowOffset = new Vector2(0f, 2f);
			((Control)_stickyHeader).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val);
			((Control)_stickyName).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.Text);
			((Control)_stickyStatus).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.TextMuted);
		}

		internal void _003CBuildStickyFolderHeader_003Eb__587_2()
		{
			Defer(OnTreeScrolled);
		}

		internal void _003CBuildStickyFolderHeader_003Eb__587_3(double _)
		{
			OnTreeScrolled();
		}

		internal void _003CBeginInlineRename_003Eb__589_0(string _)
		{
			CommitInlineRename();
		}

		internal void _003CBeginInlineRename_003Eb__589_1()
		{
			Defer(CancelInlineRename);
		}

		internal void _003CBeginInlineRename_003Eb__589_2(InputEvent input)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Invalid comparison between Unknown and I8
			InputEventKey val = (InputEventKey)(object)((input is InputEventKey) ? input : null);
			if (val != null && val.Pressed && (long)val.Keycode == 4194305)
			{
				((Control)_folderRenameEdit).AcceptEvent();
				CancelInlineRename();
			}
		}

		internal void _003CBuildGroupBar_003Eb__599_0()
		{
			Defer(UpdateGroupRowHeight);
		}

		internal void _003CBuildGroupBar_003Eb__599_1(string _)
		{
			CommitGroupName();
		}

		internal void _003CBuildGroupBar_003Eb__599_2()
		{
			ShowGroupNameRow(visible: false);
		}

		internal bool _003CRebuildGroupRow_003Eb__600_0(LaunchEntry entry)
		{
			return string.IsNullOrWhiteSpace(entry.Group);
		}

		internal void _003CRebuildGroupRow_003Eb__600_1()
		{
			BeginCreateGroup(Array.Empty<LaunchEntry>());
		}

		internal string _003CKnownGroupNames_003Eb__604_0((string Name, int Depth) node)
		{
			return node.Name;
		}

		internal string _003CAssignGroup_003Eb__609_0(LaunchEntry entry)
		{
			return entry.Id;
		}

		internal bool _003CAssignGroup_003Eb__609_1(string id)
		{
			return !string.IsNullOrWhiteSpace(id);
		}

		internal bool _003CAssignGroup_003Eb__609_4(LaunchEntry entry)
		{
			return entry == _selected;
		}

		internal string _003CBeginCreateGroup_003Eb__614_0(LaunchEntry entry)
		{
			return entry.Key;
		}

		internal LaunchEntry _003CShowGroupMenuForVisibleRows_003Eb__617_0(TreeBinding binding)
		{
			return binding.Entry;
		}

		internal bool _003CShowGroupMenuForVisibleRows_003Eb__617_1(TreeBinding binding)
		{
			return binding.Entry == _selected;
		}

		internal string _003CShowGroupMenu_003Eb__619_0(LaunchEntry entry)
		{
			return entry.Key;
		}

		internal string _003CShowGroupMenu_003Eb__619_1(LaunchEntry entry)
		{
			return entry.Group.Trim();
		}

		internal void _003CBuildLoadingPanel_003Eb__627_1(bool _)
		{
			UpdateLoadErrorActions();
		}

		internal string _003CConfirmLaunchDespiteConflicts_003Eb__630_0(ModConflict conflict)
		{
			return conflict.PairKey;
		}

		internal bool _003CConfirmLaunchDespiteConflicts_003Eb__630_1(ModConflict conflict)
		{
			return LaunchConfirmedConflicts.Contains(conflict.PairKey);
		}

		internal string _003CConfirmLaunchDespiteConflicts_003Eb__630_2(ModConflict conflict)
		{
			return conflict.Declarer.Id + " (declared) / " + conflict.Target.Id;
		}

		internal string _003CConfirmLaunchDespiteConflicts_003Eb__630_3(ModConflict conflict)
		{
			return conflict.PairKey;
		}

		internal bool _003CStartLoading_003Eb__631_0(LaunchEntry entry)
		{
			if (entry.Enabled)
			{
				return WorkshopBranches.BlocksLaunch(entry.Mod);
			}
			return false;
		}

		internal CanonicalLaunchEntry _003CCommitCanonical_003Eb__633_0(CanonicalLaunchEntry entry)
		{
			return entry.Clone();
		}

		internal bool _003CBuildLoadErrorDetails_003Eb__650_0(LocString error)
		{
			return !ModErrorText.IsGenericFailure(error);
		}

		internal void _003CBuildProfileBar_003Eb__672_0(string _)
		{
			CommitProfile(null);
		}

		internal void _003CBuildProfileBar_003Eb__672_1(string _)
		{
			UpdateProfileNameRow();
		}

		internal void _003CBuildProfileBar_003Eb__672_2(InputEvent input)
		{
			if (!input.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
			{
				return;
			}
			HBoxContainer? profileNameRow = _profileNameRow;
			if (profileNameRow != null && ((CanvasItem)profileNameRow).Visible)
			{
				ShowProfileNameRow(visible: false);
				LineEdit? profileName = _profileName;
				if (profileName != null)
				{
					((Control)profileName).AcceptEvent();
				}
			}
		}

		internal void _003CBuildProfileBar_003Eb__672_3()
		{
			CommitProfile(null);
		}

		internal void _003CBuildProfileBar_003Eb__672_4()
		{
			ShowProfileNameRow(visible: false);
		}

		internal void _003CRebuildProfileRow_003Eb__673_0()
		{
			CommitProfile(_activeProfile);
		}

		internal void _003CRebuildProfileRow_003Eb__673_1()
		{
			ShowProfileNameRow(visible: true);
		}

		internal bool _003CProfileChipTooltip_003Eb__674_0(ProfileMod mod)
		{
			return mod.Enabled;
		}

		internal bool _003CActiveProfile_003Eb__676_0(LaunchProfile profile)
		{
			return string.Equals(profile.Name, _activeProfile, StringComparison.Ordinal);
		}

		internal string _003CPlanFingerprint_003Eb__677_0(LaunchEntry entry)
		{
			return entry.Key + (entry.Enabled ? "+" : "-");
		}

		internal bool _003CIsProfileModified_003Eb__678_0(LaunchEntry entry)
		{
			return entry.CanToggle;
		}

		internal string _003CIsProfileModified_003Eb__678_1(LaunchEntry entry)
		{
			return entry.Id;
		}

		internal bool _003CIsProfileModified_003Eb__678_2(LaunchEntry entry)
		{
			if (entry.CanToggle)
			{
				return entry.Enabled;
			}
			return false;
		}

		internal string _003CIsProfileModified_003Eb__678_3(LaunchEntry entry)
		{
			return entry.Id;
		}

		internal string _003CIsProfileModified_003Eb__678_5(ProfileMod mod)
		{
			return mod.Id;
		}

		internal IReadOnlyList<LaunchProfile> _003CCollectionImportHost_003Eb__683_0()
		{
			return _profiles.Profiles;
		}

		internal string _003CKeepUninstalledMods_003Eb__686_0(ProfileMod mod)
		{
			return mod.Id;
		}

		internal void _003CBuildSettingsPage_003Eb__691_1()
		{
			ShowSettingsSection(SettingsSection.Root);
		}

		internal void _003CBuildSettingsPage_003Eb__691_2()
		{
			if (_settingsFontSlider != null)
			{
				((Range)_settingsFontSlider).Value = 100.0;
			}
		}

		internal void _003CBuildSettingsRootView_003Eb__692_0()
		{
			ShowSettingsSection(SettingsSection.Customize);
		}

		internal void _003CBuildSettingsRootView_003Eb__692_1()
		{
			ShowSettingsSection(SettingsSection.Saves);
		}

		internal void _003CBuildSettingsRootView_003Eb__692_2()
		{
			ShowSettingsSection(SettingsSection.Mods);
		}

		internal void _003CBuildSettingsRootView_003Eb__692_3()
		{
			ShowSettingsSection(SettingsSection.Ai);
		}

		internal void _003CBeginBackground_003Eb__737_0(LauncherBackground.Result? result)
		{
			OnBackgroundLoaded(result, BackgroundTrigger.Startup, null, null);
		}

		internal string _003CPopulateLanguageSelector_003Eb__760_0(MlmLocaleInfo locale)
		{
			return locale.Id;
		}

		internal void _003CStartTour_003Eb__798_0()
		{
			GoToTourStep(0);
		}

		internal Control? _003CAddTourScope_003Eb__799_0()
		{
			return (Control?)(object)_tourNext;
		}

		internal bool _003CAddTourScope_003Eb__799_1(JoyButton button)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0004: Invalid comparison between Unknown and I8
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Invalid comparison between Unknown and I8
			if ((long)button == 9)
			{
				GoToTourStep(_tourIndex - 1);
				return true;
			}
			if ((long)button == 10)
			{
				TourForward();
				return true;
			}
			return false;
		}

		internal void _003CBuildTourCallout_003Eb__800_0()
		{
			GoToTourStep(_tourIndex - 1);
		}

		internal void _003CAllTourSteps_003Eb__815_0()
		{
			OpenSettings();
			ShowTourButtons();
		}

		internal void _003CAllTourSteps_003Eb__815_1()
		{
			RestoreTourButtons();
			CloseSettings();
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_2()
		{
			OptionButton? saveProfileSelector = _saveProfileSelector;
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((saveProfileSelector != null) ? ((Node)saveProfileSelector).GetParent<Control>() : null);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_3()
		{
			Control[] array = new Control[2];
			OptionButton? languageSelector = _languageSelector;
			array[0] = ((languageSelector != null) ? ((Node)languageSelector).GetParent<Control>() : null);
			OptionButton? themeSelector = _themeSelector;
			array[1] = ((themeSelector != null) ? ((Node)themeSelector).GetParent<Control>() : null);
			return new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)array);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_4()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_headerBadges);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_5()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_changeNotesBanner);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_6()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_settingsButton);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_7()
		{
			return ProfileChipsAndActions();
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_8()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_collectionImport);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_9()
		{
			return WorkshopCollectionImportDialog.TourSource;
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_10()
		{
			return WorkshopCollectionImportDialog.TourItems;
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_11()
		{
			return WorkshopCollectionImportDialog.TourActions;
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_12()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_friendSync);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_13()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(FriendModSyncDialog.TourFriends);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_14()
		{
			return FriendModSyncDialog.TourComparison;
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_15()
		{
			return FriendModSyncDialog.TourImport;
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_16()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_transfer);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_17()
		{
			return new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[2]
			{
				(Control)_search,
				(Control)_resultCount
			});
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_18()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_filterRow);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_19()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_groupLine);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_20()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_filterToggle);
		}

		internal (string Name, object? Value)[] _003CAllTourSteps_003Eb__815_21()
		{
			return new(string, object)[1] { ("hint", MlmLocManager.Get(_groupListView ? "mods.list.hint.folders" : (LaunchPlan.KeepDisabledInPlace ? "mods.list.hint.inPlace" : "mods.list.hint"))) };
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_22()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_modTree);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_23()
		{
			ScrollContainer? detailScroll = _detailScroll;
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((detailScroll != null) ? ((Node)detailScroll).GetParent<Control>() : null);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_24()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_fab);
		}

		internal (string Name, object? Value)[] _003CAllTourSteps_003Eb__815_25()
		{
			(string, object)[] array = new(string, object)[1];
			Button? start = _start;
			array[0] = ("launch", (((start != null) ? start.Text : null) ?? string.Empty).TrimEnd('→', ' '));
			return array;
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_26()
		{
			return new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[6]
			{
				(Control)_restartGame,
				(Control)_randomMod,
				(Control)_autoSort,
				(Control)_reset,
				(Control)_vanillaStart,
				(Control)_start
			});
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_27()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsCustomizeView);
		}

		internal void _003CAllTourSteps_003Eb__815_28()
		{
			ShowSettingsSection(SettingsSection.Customize);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_29()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsSavesView);
		}

		internal void _003CAllTourSteps_003Eb__815_30()
		{
			ShowSettingsSection(SettingsSection.Saves);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_31()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsModsView);
		}

		internal void _003CAllTourSteps_003Eb__815_32()
		{
			ShowSettingsSection(SettingsSection.Mods);
		}

		internal IEnumerable<Control?> _003CAllTourSteps_003Eb__815_33()
		{
			return new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsAiView);
		}

		internal void _003CAllTourSteps_003Eb__815_34()
		{
			ShowSettingsSection(SettingsSection.Ai);
		}

		internal bool _003CShowCollectionSample_003Eb__826_0(LaunchEntry entry)
		{
			return !entry.IsManager;
		}

		internal ulong _003CShowCollectionSample_003Eb__826_1(LaunchEntry entry)
		{
			return PhysicalModIdentity.WorkshopItemIdFor(entry.Mod);
		}

		internal bool _003CShowCollectionSample_003Eb__826_2(IGrouping<ulong, LaunchEntry> group)
		{
			return group.Key != 0;
		}

		internal void _003CFriendSampleScene_003Eb__828_0()
		{
			FriendModSyncDialog.Close();
			FriendModSync.EndSample();
		}

		internal bool _003CSampleFriends_003Eb__830_0(LaunchEntry entry)
		{
			if (!entry.IsManager)
			{
				string text = entry.Mod.manifest?.id;
				if (text != null)
				{
					return text.Length > 0;
				}
				return false;
			}
			return false;
		}

		internal bool _003CSampleFriends_003Eb__830_1(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal bool _003CSampleFriends_003Eb__830_4(LaunchEntry entry)
		{
			return entry.IsManager;
		}

		internal bool _003CSampleFriends_003Eb__830_3(LaunchEntry entry)
		{
			return !entry.Enabled;
		}

		internal void _003CBuildTranslations_003Eb__841_2()
		{
			_aiCancellation?.Cancel();
		}

		internal LaunchEntry _003CRecordUndo_003Eb__855_0(LaunchEntry entry)
		{
			return entry;
		}

		internal bool _003CRecordUndo_003Eb__855_1(LaunchEntry entry)
		{
			return entry.Enabled;
		}

		internal Mod _003CStartWorkshopUpdateCheck_003Eb__888_0(LaunchEntry entry)
		{
			return entry.Mod;
		}

		internal void _003CStartWorkshopUpdateCheck_003Eb__888_1()
		{
			WorkshopBranches.Poll(0.5);
			WorkshopUpdates.Poll(0.5);
			WorkshopDownloadCenter.Poll(0.5);
			WorkshopSubscription.Poll(0.5);
		}

		internal void _003CBuildWorkshopDetails_003Eb__899_0()
		{
			LaunchEntry selected = _selected;
			if (selected != null)
			{
				RequestWorkshopRemoval(PhysicalModIdentity.WorkshopItemIdFor(selected.Mod), selected.DisplayName, MlmLocManager.Get("workshop.remove.unsubscribe"));
			}
		}

		internal void _003CBuildWorkshopDetails_003Eb__899_1(long index)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (_selected != null)
			{
				_workshopBranchDraftItem = WorkshopUpdates.ItemIdFor(_selected.Mod);
				Variant itemMetadata = _detailWorkshopBranchSelector.GetItemMetadata((int)index);
				_workshopBranchDraft = ((Variant)(ref itemMetadata)).AsString();
				UpdateWorkshopBranchControls(_selected);
			}
		}

		internal bool _003CCanRemoveWorkshopItem_003Eb__910_0(LaunchEntry entry)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			if (!entry.IsManager && (int)entry.Mod.state != 1)
			{
				return CollectibleModAssemblyLoader.GetRecordedAssemblies(entry.Mod).Length == 0;
			}
			return false;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass340_0
	{
		public bool rebuilding;

		public bool pointer;

		internal void _003CBuildModList_003Eb__7()
		{
			if (_modTree == null || !GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
			{
				return;
			}
			TreeItem selected = ((Tree)_modTree).GetSelected();
			if (selected != null)
			{
				RevealBelowPinnedTitle(selected);
				if (!rebuilding && !pointer)
				{
					RevealAboveFab(selected);
				}
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass395_0
	{
		public Mod mod;

		internal bool _003COnLocalModsChanged_003Eb__5(LaunchEntry entry)
		{
			return PhysicalModIdentity.Same(entry.Mod, mod);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass461_0
	{
		public InputEvent input;

		internal bool _003CWatchListScrolling_003Eb__3(string action)
		{
			return input.IsAction(StringName.op_Implicit(action), false);
		}
	}

	[CompilerGenerated]
	private sealed class _003CFabMenuItems_003Ed__473 : IEnumerable<Button>, IEnumerable, IEnumerator<Button>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Button _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		Button IEnumerator<Button>.Current
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
		public _003CFabMenuItems_003Ed__473(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				int activeCount = WorkshopDownloadCenter.ActiveCount;
				_003C_003E2__current = FabMenuItem((activeCount > 0) ? MlmLocManager.Get("fab.downloads.active", ("count", activeCount)) : MlmLocManager.Get("fab.downloads"), MlmLocManager.Get("fab.downloads.tooltip"), enabled: true, OpenDownloads);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = FabMenuItem(MlmLocManager.Get("launcher.action.importLocalMods"), MlmLocManager.Get("launcher.action.importLocalMods.tooltip"), enabled: true, OpenLocalModImport);
				_003C_003E1__state = 2;
				return true;
			case 2:
				_003C_003E1__state = -1;
				_003C_003E2__current = FabMenuItem(MlmLocManager.Get("launcher.action.closeAndOpenLogFolder"), MlmLocManager.Get("launcher.action.closeAndOpenLogFolder.tooltip"), enabled: true, CloseGameAndOpenLogFolder);
				_003C_003E1__state = 3;
				return true;
			case 3:
				_003C_003E1__state = -1;
				_003C_003E2__current = FabMenuItem(MlmLocManager.Get("fab.tour"), MlmLocManager.Get("fab.tour.tooltip"), enabled: true, StartTour);
				_003C_003E1__state = 4;
				return true;
			case 4:
				_003C_003E1__state = -1;
				return false;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Button> IEnumerable<Button>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CFabMenuItems_003Ed__473(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Button>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CProfileChipsAndActions_003Ed__824 : IEnumerable<Control>, IEnumerable, IEnumerator<Control>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Control? _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerator<Control> _003C_003E7__wrap1;

		Control? IEnumerator<Control>.Current
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
		public _003CProfileChipsAndActions_003Ed__824(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
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
					if (_profileRow == null || !GodotObject.IsInstanceValid((GodotObject)(object)_profileRow))
					{
						return false;
					}
					_003C_003E7__wrap1 = ((IEnumerable)((Node)_profileRow).GetChildren(false)).OfType<Control>().GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					Control current = _003C_003E7__wrap1.Current;
					if (current != _collectionImport && current != _friendSync && current != _transfer)
					{
						_003C_003E2__current = current;
						_003C_003E1__state = 1;
						return true;
					}
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

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<Control> IEnumerable<Control>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CProfileChipsAndActions_003Ed__824(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Control>)this).GetEnumerator();
		}
	}

	private static Control? _settingsAiView;

	private static Button? _settingsAiEntry;

	private static PanelContainer? _settingsCard;

	private static OptionButton? _aiProvider;

	private static Control? _aiEndpointRow;

	private static LineEdit? _aiEndpoint;

	private static LineEdit? _aiModel;

	private static LineEdit? _aiKeyInput;

	private static Label? _aiModelLabel;

	private static Button? _aiGetKey;

	private static TextEdit? _aiNotes;

	private static Button? _translationAi;

	private static Button? _translationCancel;

	private static Button? _translationOriginal;

	private static Label? _aiCredentialStatus;

	private static Label? _aiProgress;

	private static CancellationTokenSource? _aiCancellation;

	private static AiTranslationSettings _aiSaved = new AiTranslationSettings();

	private const string AutoLaunchVariable = "MLM_AUTOLAUNCH";

	private const double AutoLaunchGiveUpSeconds = 120.0;

	private static bool _autoLaunchUsed;

	private const int DetailNoteCount = 3;

	private const int DetailNoteLength = 600;

	private const int NoteBatchCharacters = 6000;

	private static PanelContainer? _changeNotesBanner;

	private static Label? _changeNotesBannerText;

	private static Button? _changeNotesOpen;

	private static bool _changeNotesDismissed;

	private static VBoxContainer? _detailChangeNotes;

	private static VBoxContainer? _detailChangeNotesList;

	private static Button? _detailChangeNotesPage;

	private static Button? _detailChangeNotesTranslate;

	private static string _detailChangeNotesSignature = string.Empty;

	private static CancellationTokenSource? _changeNoteTranslation;

	private static readonly (PromptKey Key, string Action)[] SteamPromptActions = new(PromptKey, string)[10]
	{
		(PromptKey.A, "Select"),
		(PromptKey.B, "Cancel"),
		(PromptKey.X, "Top_Panel"),
		(PromptKey.Y, "Confirm"),
		(PromptKey.LeftShoulder, "Tab_Left"),
		(PromptKey.RightShoulder, "Tab_Right"),
		(PromptKey.LeftTrigger, "View_Draw_Pile"),
		(PromptKey.RightTrigger, "View_Discard_Pile"),
		(PromptKey.Start, "Settings"),
		(PromptKey.Back, "View_Map")
	};

	private const ulong ControllerDetectWindow = 5000uL;

	private const float TreeRowHeight = 39f;

	private static LauncherController? _controller;

	private static ScrollContainer? _detailScroll;

	private static CanvasLayer? _promptLayer;

	private static Control? _promptHost;

	private static HBoxContainer? _promptBar;

	private static Control? _promptSpacer;

	private static string _promptSignature = string.Empty;

	private static readonly Dictionary<PromptKey, PadGlyph> PromptGlyphs = new Dictionary<PromptKey, PadGlyph>();

	private static ulong _promptGlyphsAt;

	private static readonly InputHandle_t[] SteamControllers = (InputHandle_t[])(object)new InputHandle_t[16];

	private static string? _controllerMoveKey;

	private static bool _startFocusIsDefault;

	private static bool _selectFirstRowAfterRebuild;

	private const int ColumnOrder = 0;

	private const int ColumnToggle = 1;

	private const int ColumnName = 2;

	private const int ColumnGroup = 3;

	private const int ColumnVersion = 4;

	private const int ColumnSource = 5;

	private const int ColumnStatus = 6;

	private const int ColumnAffects = 7;

	private const int MaxColumnCount = 8;

	private const int MaxLogLines = 60;

	private const float LoadingSpinnerTurnsPerSecond = 0.4f;

	private const double MoveRepeatInitialDelay = 0.35;

	private const double MoveRepeatInterval = 0.075;

	private const int NewGroupMenuId = 10000;

	private const string GroupFilterUngrouped = "\u0001ungrouped";

	private const int BackgroundItemBase = 1000;

	private const int BackgroundFolderItemId = 999;

	private static readonly List<TreeBinding> TreeBindings = new List<TreeBinding>();

	private static readonly List<int> RowLoadPositions = new List<int>();

	private static readonly List<Button> FilterChips = new List<Button>();

	private static readonly List<string> DetailGroupValues = new List<string>();

	private static readonly List<string> LanguageSelectorValues = new List<string>();

	private static readonly List<string> BackgroundSelectorValues = new List<string>();

	private static readonly List<string> GroupMenuValues = new List<string>();

	private static readonly List<string> GroupMenuTargets = new List<string>();

	private static readonly Dictionary<string, bool> CollapsedTreeSections = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

	private static readonly List<Action> LocalizationBindings = new List<Action>();

	private static readonly List<Action> PaletteBindings = new List<Action>();

	private static readonly Queue<string> LogLines = new Queue<string>();

	private static readonly Stopwatch LoadClock = new Stopwatch();

	private static readonly List<(Control Control, bool Interactive)> OptionEditors = new List<(Control, bool)>();

	private static CanvasLayer? _layer;

	private static Control? _root;

	private static LauncherBackground.Result? _background;

	private static TextureRect? _backgroundNode;

	private static SceneTree? _sceneTree;

	private static LaunchPlan? _plan;

	private static LaunchEntry? _selected;

	private static ModReorderTree? _modTree;

	private static LineEdit? _search;

	private static Label? _resultCount;

	private static Label? _discoveredChip;

	private static Label? _stateChip;

	private static Control? _headerBadges;

	private static OptionButton? _languageSelector;

	private static OptionButton? _themeSelector;

	private static OptionButton? _saveProfileSelector;

	private static Button? _settingsButton;

	private static Control? _settingsPage;

	private static Control? _settingsRootView;

	private static Control? _settingsCustomizeView;

	private static Control? _settingsSavesView;

	private static Control? _settingsModsView;

	private static Label? _settingsTitle;

	private static Label? _settingsSubtitle;

	private static Button? _settingsBack;

	private static Button? _settingsFontReset;

	private static Button? _settingsAffectsToggle;

	private static Button? _settingsRandomModToggle;

	private static Button? _settingsVanillaLaunchToggle;

	private static ScrollContainer? _settingsScroll;

	private static Button? _settingsAutoUnlockToggle;

	private static Button? _settingsModIsolationToggle;

	private static Button? _settingsKeepDisabledToggle;

	private static Button? _settingsCloudSyncToggle;

	private static Label? _settingsCloudSyncStatus;

	private static SettingsSection _settingsSection;

	private static HSlider? _settingsFontSlider;

	private static Label? _settingsFontValue;

	private static Label? _settingsStatus;

	private static int _selectedSaveProfileId = 1;

	private static Button? _transfer;

	private static Label? _feedback;

	private static Label? _detailName;

	private static HFlowContainer? _detailMeta;

	private static string _detailMetaSignature = string.Empty;

	private static Label? _detailContent;

	private static Label? _detailGameVersion;

	private static Control? _detailInstallTimeFact;

	private static Label? _detailInstallTime;

	private static Control? _detailPriorityFact;

	private static Label? _detailPriority;

	private static RichTextLabel? _detailDescription;

	private static TextEdit? _detailDescriptionEditor;

	private static HBoxContainer? _detailDescriptionActions;

	private static string? _descriptionEditingKey;

	private static RichTextLabel? _detailDependencies;

	private static VBoxContainer? _detailIncompatibilities;

	private static RichTextLabel? _detailIncompatibilityText;

	private static HFlowContainer? _detailBadges;

	private static Button? _detailToggle;

	private static Button? _detailOpenFolder;

	private static Button? _restartGame;

	private static VBoxContainer? _detailWarnings;

	private static RichTextLabel? _detailWarningText;

	private static LineEdit? _detailAlias;

	private static OptionButton? _detailGroupSelector;

	private static Button? _detailOrganizationSave;

	private static VBoxContainer? _detailOptions;

	private static VBoxContainer? _detailOptionRows;

	private static Timer? _moveRepeatTimer;

	private static Button? _moveRepeatButton;

	private static Button? _movePressHandledButton;

	private static int _moveRepeatDirection;

	private static bool _framePumpSubscribed;

	private static bool _restartArmed;

	private static readonly HashSet<string> LaunchConfirmedConflicts = new HashSet<string>(StringComparer.Ordinal);

	private static Button? _moveUp;

	private static Button? _moveDown;

	private static Button? _enableAll;

	private static Button? _disableAll;

	private static Button? _autoSort;

	private static Button? _randomMod;

	private static Button? _reset;

	private static Button? _start;

	private static Button? _vanillaStart;

	private static VBoxContainer? _emptyState;

	private static Button? _clearFilters;

	private static ButtonGroup? _filterGroup;

	private static ButtonGroup? _groupChipGroup;

	private static HFlowContainer? _groupRow;

	private static HBoxContainer? _groupNameRow;

	private static LineEdit? _groupName;

	private static Label? _groupNamePrompt;

	private static Button? _assignGroup;

	private static PopupMenu? _groupMenu;

	private static ButtonGroup? _profileGroup;

	private static HFlowContainer? _profileRow;

	private static HBoxContainer? _profileNameRow;

	private static LineEdit? _profileName;

	private static ProfileDocument _profiles = new ProfileDocument();

	private static ModOrganizationDocument _organization = new ModOrganizationDocument();

	private static ModOptionDocument _options = new ModOptionDocument();

	private static CanonicalLaunchDocument _canonical = new CanonicalLaunchDocument();

	private static List<CanonicalLaunchEntry> _canonicalDormant = new List<CanonicalLaunchEntry>();

	private static CanonicalLaunchDocument? _pendingCanonical;

	private static PendingCanonicalAction _pendingCanonicalAction;

	private static Mod? _pendingSkipMod;

	private static string? _activeProfile;

	private static PlanResult? _priorityReport;

	private static bool _deleteArmed;

	private static PanelContainer? _loadingCard;

	private static VBoxContainer? _loadingPanel;

	private static Label? _loadingSpinner;

	private static Label? _loadingStatus;

	private static Label? _loadingCurrentMod;

	private static Label? _loadingElapsed;

	private static ProgressBar? _progress;

	private static RichTextLabel? _loadLog;

	private static PanelContainer? _loadErrorPanel;

	private static Label? _loadErrorTitle;

	private static RichTextLabel? _loadErrorDetails;

	private static Button? _disableAfterError;

	private static Button? _continueAfterError;

	private static string? _continueAfterErrorRetryKey;

	private static Button? _restartAfterError;

	private static Mod? _preparedMod;

	private static int _skippedRun;

	private static Mod? _loadErrorMod;

	private static ModFilter _filter = ModFilter.All;

	private static string? _groupFilter;

	private static string? _groupRenameSource;

	private static bool _groupDeleteArmed;

	private static string? _groupDeleteTarget;

	private static string? _groupCreateParent;

	private static string? _organizationEditingKey;

	private static string? _optionsEditingKey;

	private static bool _optionsShowAdvanced;

	private static bool _rebuildingTree;

	private static bool _rebuildQueued;

	private static bool _scrollAfterRebuild;

	private static bool _pumpActive;

	private static bool _awaitingLoadDecision;

	private static bool _releaseOnNextFrame;

	private static bool _deferLogText;

	private static bool _logTextStale;

	private static int _pumpBusy;

	private static int _startPressed;

	private static bool _updatingLanguageSelector;

	private static bool _updatingThemeSelector;

	private static bool _backgroundChangePending;

	private static bool _updatingSaveProfileSelector;

	private static bool _updatingOrganizationFields;

	private static bool _showAffectsColumn;

	private static bool _dragPreviewActive;

	private static RowStatusLookup? _rowStatusLookup;

	private const int FabSize = 48;

	private const float FabInset = 14f;

	private const double FabTuckSeconds = 0.12;

	private const double FabReturnSeconds = 0.18;

	private const double FabRestSeconds = 0.6;

	private const ulong ListInputWindowMs = 400uL;

	private static Button? _fab;

	private static PanelContainer? _fabBadge;

	private static Label? _fabBadgeText;

	private static CanvasLayer? _fabMenuLayer;

	private static Timer? _fabRest;

	private static Tween? _fabFade;

	private static bool _fabTucked;

	private static ulong _listInputAt;

	private static Button? _filterToggle;

	private static Control? _filterRow;

	private static Control? _groupLine;

	private static HBoxContainer? _filterSummary;

	private static Label? _filterSummaryText;

	private static Button? _filterClear;

	private static bool _filtersCollapsed;

	private static Button? _friendSync;

	private static Timer? _friendSyncTimer;

	private const string FolderKeyPrefix = "folder:";

	private const int FolderMenuNewChild = 1;

	private const int FolderMenuRename = 2;

	private const int FolderMenuMoveUp = 3;

	private const int FolderMenuMoveDown = 4;

	private const int FolderMenuDelete = 5;

	private const int FolderMenuCollapseAll = 6;

	private const int FolderMenuExpandAll = 7;

	private static readonly HashSet<string> CollapsedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	private static readonly Dictionary<(bool Collapsed, int Depth), Texture2D> FoldIcons = new Dictionary<(bool, int), Texture2D>();

	private static readonly Dictionary<int, Texture2D> IndentSpacers = new Dictionary<int, Texture2D>();

	private static readonly List<string> FolderMoveTargets = new List<string>();

	private static bool _groupListView;

	private static Button? _groupViewToggle;

	private static Control? _stickyClip;

	private static Panel? _stickyHeader;

	private static TextureRect? _stickyIcon;

	private static Label? _stickyName;

	private static Label? _stickyStatus;

	private static string? _stickyFolderKey;

	private static float _treeItemTop = -1f;

	private static (Color Color, int Size, int Step) _foldIconStyle;

	private static PopupMenu? _folderMenu;

	private static PopupMenu? _folderMoveMenu;

	private static string? _folderMenuGroup;

	private static bool _titlesPinned;

	private static UiPalette? _titlesPinnedPalette;

	private static Button? _foldAllButton;

	private static Vector2 _folderMenuPosition;

	private static LineEdit? _folderRenameEdit;

	private static string? _folderRenameGroup;

	private static readonly Dictionary<string, Dictionary<string, bool>> FolderToggleMemory = new Dictionary<string, Dictionary<string, bool>>(StringComparer.OrdinalIgnoreCase);

	private static (string Text, string Variation, string Tooltip)? _statusBeforeDrag;

	private static int _cursorRowAfterRebuild = -1;

	private static bool _revealSelectionAfterSuspend;

	private static Button? _activeProfileChip;

	private static Button? _profileSaveChanges;

	private static Button? _profileConfirmSave;

	private static Button? _collectionImport;

	private static string? _profileBaseline;

	private const double BackgroundFadeSeconds = 0.3;

	private static LauncherBackground.Stats? _backdropStats;

	private static ColorRect? _backdropVeil;

	private static Tween? _backgroundFade;

	private static TextureRect? _fadingBackgroundNode;

	private static LauncherBackground.Result? _fadingBackground;

	private const float TourPadding = 6f;

	private const float TourGap = 14f;

	private const float TourMargin = 16f;

	private const float TourCalloutWidth = 400f;

	private const double TourMoveSeconds = 0.22;

	private static readonly ColorRect?[] TourShade = (ColorRect?[])(object)new ColorRect[4];

	private static CanvasLayer? _tourLayer;

	private static Control? _tourRoot;

	private static Panel? _tourRing;

	private static PanelContainer? _tourCallout;

	private static Label? _tourProgress;

	private static PanelContainer? _tourNewBadge;

	private static Label? _tourTitle;

	private static RichTextLabel? _tourBody;

	private static Button? _tourSkip;

	private static Button? _tourBack;

	private static Button? _tourNext;

	private static List<TourStep> _tourSteps = new List<TourStep>();

	private static TourScene? _tourScene;

	private static bool _tourWhatsNew;

	private static int _tourIndex;

	private static int _tourToken;

	private static Rect2 _tourSpot;

	private static Tween? _tourTween;

	private const int ToolsAndCollectionsRevision = 11;

	private const int ToolsControllerRevision = 12;

	private static bool? _tourFiltersCollapsed;

	private static TourButtonState? _tourButtonState;

	private static VBoxContainer? _detailTranslations;

	private static OptionButton? _translationLanguage;

	private static Label? _translationHint;

	private static Button? _translationExport;

	private static Button? _translationImport;

	private static Button? _translationOpen;

	private static Label? _translationAiHint;

	private static Control? _translationFileActions;

	private const int MaxUndoSteps = 50;

	private const ulong UndoMergeMilliseconds = 1500uL;

	private static readonly List<UndoStep> UndoSteps = new List<UndoStep>();

	private static Button? _undoShortcut;

	private static PanelContainer? _workshopBadge;

	private static Label? _workshopChip;

	private static BadgeTone _workshopTone = BadgeTone.Caution;

	private static Button? _updateAll;

	private static Button? _updatesFilter;

	private static VBoxContainer? _detailWorkshop;

	private static Label? _detailWorkshopId;

	private static HFlowContainer? _detailWorkshopTags;

	private static string _detailWorkshopTagsSignature = string.Empty;

	private static Label? _detailWorkshopState;

	private static Label? _detailWorkshopBranch;

	private static Label? _detailWorkshopAvailableBranches;

	private static Label? _detailWorkshopBranchLabel;

	private static OptionButton? _detailWorkshopBranchSelector;

	private static Button? _detailWorkshopBranchSwitch;

	private static Label? _detailWorkshopBranchHint;

	private static ulong _workshopBranchDraftItem;

	private static string _workshopBranchDraft = string.Empty;

	private static Label? _detailWorkshopLocal;

	private static Label? _detailWorkshopServer;

	private static Button? _detailWorkshopUpdate;

	private static Button? _detailWorkshopOpen;

	private static Button? _detailWorkshopUnsubscribe;

	private static Button? _detailAliasFill;

	private static Timer? _workshopTimer;

	private static bool _workshopSummaryShown;

	private static bool _workshopRetryWasNeeded;

	private static bool _workshopScanSubscribed;

	private static bool _launchDuringDownloadArmed;

	private static AiProviderPreset SelectedAiPreset
	{
		get
		{
			AiProviderPreset[] all = AiProviderPreset.All;
			OptionButton? aiProvider = _aiProvider;
			return all[Math.Clamp((aiProvider != null) ? aiProvider.Selected : 0, 0, AiProviderPreset.All.Length - 1)];
		}
	}

	private static string GameTranslationLanguage
	{
		get
		{
			SaveManager instance = SaveManager.Instance;
			object obj;
			if (instance == null)
			{
				obj = null;
			}
			else
			{
				SettingsSave settingsSave = instance.SettingsSave;
				obj = ((settingsSave != null) ? settingsSave.Language : null);
			}
			if (obj == null)
			{
				obj = "eng";
			}
			return (string)obj;
		}
	}

	private static int ColumnCount
	{
		get
		{
			if (!_showAffectsColumn)
			{
				return 7;
			}
			return 8;
		}
	}

	internal static bool FabMenuOpen
	{
		get
		{
			if (_fabMenuLayer != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)_fabMenuLayer);
			}
			return false;
		}
	}

	internal static bool TourOpen
	{
		get
		{
			if (_tourLayer != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)_tourLayer);
			}
			return false;
		}
	}

	private static bool TourOnController => _controller?.ControllerMode ?? false;

	private static bool SelectedTranslatesTitleOnly
	{
		get
		{
			LaunchEntry? selected = _selected;
			if (selected == null)
			{
				return true;
			}
			return selected.Mod.manifest?.hasPck != true;
		}
	}

	private static string TranslationLanguage
	{
		get
		{
			(string Code, string Name)[] languages = ModTranslationFiles.Languages;
			OptionButton? translationLanguage = _translationLanguage;
			return languages[Math.Max(0, (translationLanguage != null) ? translationLanguage.Selected : 0)].Code;
		}
	}

	private static void OpenModListTransfer()
	{
		if (_plan != null && !_pumpActive)
		{
			ModListTransferDialog.Show(_plan, CurrentGameVersion(), ApplyTransferredList);
		}
	}

	private static PlanResult ApplyTransferredList(LaunchProfile profile)
	{
		if (_plan == null || _pumpActive)
		{
			return PlanResult.Error(MlmLocManager.Get("transfer.import.unavailable"));
		}
		RecordUndo();
		PlanResult result = _plan.ApplyProfile(profile);
		ShowFeedback(result);
		if (result.Success)
		{
			_activeProfile = null;
			_profiles.Active = null;
			PersistProfiles();
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
			Defer(RebuildProfileRow);
		}
		return result;
	}

	private static void OnVanillaModDetected(Mod mod)
	{
		if (_plan != null && !_pumpActive)
		{
			Defer(delegate
			{
				AdoptDetectedMod(mod);
			});
		}
	}

	private static void AdoptDetectedMod(Mod mod)
	{
		if (_plan == null || _pumpActive)
		{
			return;
		}
		try
		{
			if (WorkshopDownloadCenter.IsRemoved(PhysicalModIdentity.WorkshopItemIdFor(mod)))
			{
				DeferredModLoader.RemoveFromLiveList(mod);
				return;
			}
			LaunchEntry launchEntry = _plan.Entries.FirstOrDefault((LaunchEntry entry) => entry.Mod != mod && entry.Mod.modSource == mod.modSource && string.Equals(entry.Id, mod.manifest?.id, StringComparison.Ordinal) && SamePath(PhysicalModIdentity.SourcePath(entry.Mod), mod.path));
			bool flag = false;
			if (launchEntry == null)
			{
				LaunchEntry launchEntry2 = _plan.MovedWorkshopCopy(mod, Directory.Exists);
				if (launchEntry2 != null)
				{
					launchEntry = launchEntry2;
					flag = true;
				}
			}
			if (launchEntry == null)
			{
				HashSet<string> before = _plan.ConflictPairKeys();
				IReadOnlyList<LaunchEntry> readOnlyList = _plan.AddDiscoveredMods(new _003C_003Ez__ReadOnlySingleElementList<Mod>(mod), WorkshopCollectionArrivals.KeepsOff);
				if (readOnlyList.Count > 0)
				{
					string key = (WorkshopCollectionArrivals.KeepsOff(readOnlyList[0].Mod) ? "collection.discovered.addedOff" : "workshop.discovered.newModAdded");
					WorkshopBranches.AddDiscoveredMods(readOnlyList.Select((LaunchEntry entry) => entry.Mod));
					ApplyOrganization(readOnlyList);
					_selected = readOnlyList[0];
					_organizationEditingKey = null;
					ShowFeedback(_plan.WarnAboutNewConflicts(PlanResult.Ok(MlmLocManager.Get(key, ("name", readOnlyList[0].DisplayName))), before));
					UpdateDetails();
					UpdatePlanSummary();
					QueueRebuild(scrollToSelection: true);
				}
				return;
			}
			if (mod.manifest != null && WorkshopBranches.For(launchEntry.Mod)?.Active == null)
			{
				launchEntry.Mod.manifest = mod.manifest;
				launchEntry.Mod.errors = mod.errors;
				launchEntry.Mod.version = null;
			}
			if (flag)
			{
				string path = launchEntry.Mod.path;
				string key2 = launchEntry.Key;
				_plan.Relocate(launchEntry, mod.path);
				FollowEntryKey(key2, launchEntry);
				GD.Print($"{"[ModLaunchManager]"} workshop mod {launchEntry.Id} moved from {path} to {launchEntry.Mod.path} by its update; kept its entry.");
				WorkshopUpdates.CompleteMovedDownload(launchEntry.Mod);
			}
			DeferredModLoader.RemoveFromLiveList(mod);
			GD.Print($"{"[ModLaunchManager]"} adopted vanilla's re-read of {launchEntry.Id} and dropped the duplicate Mod entry.");
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not adopt a re-read mod: {value}");
		}
	}

	private static bool SamePath(string? left, string? right)
	{
		if (!string.IsNullOrWhiteSpace(left) && !string.IsNullOrWhiteSpace(right))
		{
			return string.Equals(PhysicalModIdentity.NormalizePath(left), PhysicalModIdentity.NormalizePath(right), OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
		}
		return false;
	}

	private static void RestartGame()
	{
		if (_pumpActive)
		{
			return;
		}
		LaunchPlan? plan = _plan;
		if (plan != null && plan.HasChanges && !_restartArmed)
		{
			_restartArmed = true;
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("restart.confirmDiscardChanges")));
			return;
		}
		try
		{
			int num = OS.CreateInstance(default(ReadOnlySpan<string>));
			if (num <= 0)
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("restart.unavailable")));
				return;
			}
			GD.Print($"{"[ModLaunchManager]"} restarting the game (pid {num}).");
			QuitGame("a restart");
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} restart failed: {ex}");
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("restart.failed", ("error", ex.Message))));
		}
	}

	private static void OpenSelectedModFolder()
	{
		if (_selected == null)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("mods.selection.required")));
		}
		else
		{
			OpenFolder(_selected.Mod.path, _selected.Name);
		}
	}

	private static void OpenLocalModImport()
	{
		if (_plan != null && !_pumpActive)
		{
			LocalModImportDialog.Show(new LocalModImportHost(InstalledModsById, PrepareLocalModsFolder, OnLocalModsReplaced, OpenLocalModsFolder));
		}
	}

	private static IReadOnlyDictionary<string, LocalModInstalled> InstalledModsById()
	{
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Invalid comparison between Unknown and I4
		Dictionary<string, LocalModInstalled> dictionary = new Dictionary<string, LocalModInstalled>(StringComparer.Ordinal);
		foreach (IGrouping<string, LaunchEntry> item in (_plan?.Entries ?? new List<LaunchEntry>()).Where(delegate(LaunchEntry entry)
		{
			string text = entry.Mod.manifest?.id;
			return text != null && text.Length > 0;
		}).GroupBy<LaunchEntry, string>((LaunchEntry entry) => entry.Id, StringComparer.Ordinal))
		{
			LaunchEntry launchEntry = item.FirstOrDefault((LaunchEntry copy) => (int)copy.Mod.modSource != 2) ?? item.First();
			bool flag = (int)launchEntry.Mod.modSource != 2 && !string.IsNullOrWhiteSpace(launchEntry.Mod.path);
			dictionary[launchEntry.Id] = new LocalModInstalled(launchEntry.Id, launchEntry.DisplayName, launchEntry.Mod.manifest?.version, flag ? Path.GetFullPath(ProjectSettings.GlobalizePath(launchEntry.Mod.path)) : null, item.Any((LaunchEntry copy) => (int)copy.Mod.state == 1));
		}
		return dictionary;
	}

	private static string PrepareLocalModsFolder()
	{
		string localModsPath = LocalModWatcher.GetLocalModsPath();
		if (string.IsNullOrWhiteSpace(localModsPath))
		{
			throw new IOException(MlmLocManager.Get("launcher.gameDirectory.unknown"));
		}
		if (!Directory.Exists(localModsPath))
		{
			Directory.CreateDirectory(localModsPath);
			GD.Print("[ModLaunchManager] created the local mods folder at " + localModsPath + ".");
		}
		if (!LocalModWatcher.IsRunning && _sceneTree != null && !_pumpActive)
		{
			LocalModWatcher.Start(_sceneTree, OnLocalModsChanged, OnLocalModWatcherError);
		}
		return localModsPath;
	}

	private static void OnLocalModsReplaced(IReadOnlyList<string> ids)
	{
		if (_plan == null || _pumpActive)
		{
			return;
		}
		foreach (string id in ids)
		{
			LaunchEntry launchEntry = _plan.Entries.FirstOrDefault((LaunchEntry candidate) => (int)candidate.Mod.modSource != 2 && string.Equals(candidate.Id, id, StringComparison.Ordinal));
			if (launchEntry != null)
			{
				if (ModManifestReader.RefreshInPlace(launchEntry.Mod, out string error))
				{
					GD.Print($"{"[ModLaunchManager]"} replaced local mod {id}; now {launchEntry.Mod.manifest?.version}.");
				}
				else
				{
					GD.PushWarning($"{"[ModLaunchManager]"} replaced local mod {id} but could not re-read it: {error}");
					ShowFeedback(PlanResult.Error(MlmLocManager.Get("localImport.replacedNeedsRestart", ("name", launchEntry.DisplayName))));
				}
			}
		}
		UpdateDetails();
		UpdatePlanSummary();
		QueueRebuild();
	}

	private static void OpenLocalModsFolder()
	{
		try
		{
			OpenFolder(PrepareLocalModsFolder(), MlmLocManager.Get("folder.localMods"));
		}
		catch (Exception ex)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("folder.openFailed", ("error", ex.Message))));
		}
	}

	private static void CloseGameAndOpenLogFolder()
	{
		if (OpenFolder(Path.GetDirectoryName(GameLogFile()), MlmLocManager.Get("folder.log")))
		{
			SetStatus(MlmLocManager.Get("folder.logOpenedAndClosing"), "WarningLabel", "→");
			QuitGame("opening the log folder");
		}
	}

	internal static string GameLogFile()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		string text = "user://logs/godot.log";
		try
		{
			if (ProjectSettings.HasSetting("debug/file_logging/log_path"))
			{
				Variant val = default(Variant);
				val = ProjectSettings.GetSetting("debug/file_logging/log_path", val);
				string text2 = ((Variant)(ref val)).AsString();
				if (!string.IsNullOrWhiteSpace(text2))
				{
					text = text2;
				}
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning("[ModLaunchManager] could not read the log path setting: " + ex.Message);
		}
		string text3 = ProjectSettings.GlobalizePath(text);
		if (!Path.HasExtension(text3))
		{
			return Path.Combine(text3, "godot.log");
		}
		return text3;
	}

	private static void QuitGame(string reason)
	{
		try
		{
			NGame instance = NGame.Instance;
			if (instance != null && GodotObject.IsInstanceValid((GodotObject)(object)instance))
			{
				GD.Print("[ModLaunchManager] quitting through the game's own exit for " + reason + ".");
				ManagerReturn.Quit(instance);
				return;
			}
			GD.PushWarning("[ModLaunchManager] no NGame instance to quit through for " + reason + "; ending the main loop directly, which skips the game's exit save.");
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} the game's own exit failed for {reason}; ending the main loop directly instead: {ex.GetType().Name} {ex.Message}");
		}
		SceneTree? sceneTree = _sceneTree;
		if (sceneTree != null)
		{
			sceneTree.Quit(0);
		}
	}

	private static bool OpenFolder(string rawPath, string displayName)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Invalid comparison between Unknown and I8
		try
		{
			string text = Path.GetFullPath(ProjectSettings.GlobalizePath(rawPath));
			if (File.Exists(text))
			{
				text = Path.GetDirectoryName(text) ?? text;
			}
			if (!Directory.Exists(text))
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("folder.missing", ("path", text))));
				return false;
			}
			Error val = OS.ShellShowInFileManager(text, true);
			ShowFeedback(((int)val == 0) ? PlanResult.Ok(MlmLocManager.Get("folder.opened", ("name", displayName))) : PlanResult.Error(MlmLocManager.Get("folder.openFailed", ("error", val))));
			return (long)val == 0;
		}
		catch (Exception ex)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("folder.openFailed", ("error", ex.Message))));
			return false;
		}
	}

	private static string CurrentGameVersion()
	{
		try
		{
			ReleaseInfo releaseInfo = ReleaseInfoManager.Instance.ReleaseInfo;
			string text = ((releaseInfo != null) ? releaseInfo.Version : null);
			if (string.IsNullOrWhiteSpace(text))
			{
				return MlmLocManager.Get("common.value.unknown");
			}
			return text.StartsWith('v') ? text : ("v" + text);
		}
		catch
		{
			return MlmLocManager.Get("common.value.unknown");
		}
	}

	private static string DisplayName(Mod mod)
	{
		LaunchEntry launchEntry = _plan?.Entries.FirstOrDefault((LaunchEntry candidate) => candidate.Mod == mod);
		if (launchEntry != null)
		{
			return launchEntry.DisplayName;
		}
		string result = mod.manifest?.id ?? "<unknown>";
		if (!string.IsNullOrWhiteSpace(mod.manifest?.name))
		{
			return TextEncodingRepair.Repair(mod.manifest.name);
		}
		return result;
	}

	private static string SourceName(ModSource source)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Invalid comparison between Unknown and I4
		if ((int)source != 2)
		{
			return MlmLocManager.Get("details.source.local");
		}
		return MlmLocManager.Get("details.source.workshop");
	}

	private static Control BuildSettingsAiView()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Expected O, but got Unknown
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Expected O, but got Unknown
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Expected O, but got Unknown
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Expected O, but got Unknown
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Expected O, but got Unknown
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Expected O, but got Unknown
		ScrollContainer val = (ScrollContainer)(object)(_settingsAiView = (Control?)new ScrollContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 240f),
			HorizontalScrollMode = (ScrollMode)0
		});
		VBoxContainer val2 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)LocalizedKeyLabel("ai.settings.provider", "DimLabel"), false, (InternalMode)0);
		OptionButton provider = UiKit.Dropdown();
		((Control)provider).SizeFlagsHorizontal = (SizeFlags)3;
		_aiProvider = provider;
		AiProviderPreset[] all = AiProviderPreset.All;
		foreach (AiProviderPreset aiProviderPreset in all)
		{
			provider.AddItem(aiProviderPreset.IsPreset ? aiProviderPreset.Provider.ToString() : string.Empty, -1);
		}
		int custom = Array.FindIndex(AiProviderPreset.All, (AiProviderPreset p) => !p.IsPreset);
		BindLocalization((Control)(object)provider, delegate
		{
			provider.SetItemText(custom, MlmLocManager.Get("ai.provider.custom"));
		});
		OptionButton obj = provider;
		object obj2 = _003C_003EO._003C10_003E__OnAiProviderSelected;
		if (obj2 == null)
		{
			ItemSelectedEventHandler val3 = OnAiProviderSelected;
			_003C_003EO._003C10_003E__OnAiProviderSelected = val3;
			obj2 = (object)val3;
		}
		obj.ItemSelected += (ItemSelectedEventHandler)obj2;
		((Node)val2).AddChild((Node)(object)provider, false, (InternalMode)0);
		VBoxContainer val4 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)val4).AddChild((Node)(object)LocalizedKeyLabel("ai.settings.endpoint", "DimLabel"), false, (InternalMode)0);
		_aiEndpoint = NewField();
		_aiEndpoint.PlaceholderText = "https://…/v1";
		((Node)val4).AddChild((Node)(object)_aiEndpoint, false, (InternalMode)0);
		_aiEndpointRow = (Control?)(object)val4;
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		Label modelLabel = UiKit.Label(string.Empty, "DimLabel");
		_aiModelLabel = modelLabel;
		BindLocalization((Control)(object)modelLabel, delegate
		{
			modelLabel.Text = MlmLocManager.Get(SelectedAiPreset.IsPreset ? "ai.settings.modelPreset" : "ai.settings.model");
		});
		((Node)val2).AddChild((Node)(object)modelLabel, false, (InternalMode)0);
		_aiModel = NewField();
		((Node)val2).AddChild((Node)(object)_aiModel, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)LocalizedKeyLabel("ai.settings.key", "DimLabel"), false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_aiKeyInput = NewField(secret: true);
		((Node)val5).AddChild((Node)(object)_aiKeyInput, false, (InternalMode)0);
		_aiGetKey = LocalizedKeyButton("ai.settings.getKey", "SecondaryButton", 0f, 34f);
		((BaseButton)_aiGetKey).Pressed += OpenAiKeyPage;
		((Node)val5).AddChild((Node)(object)_aiGetKey, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)LocalizedKeyLabel("ai.settings.notes", "DimLabel"), false, (InternalMode)0);
		_aiNotes = new TextEdit
		{
			CustomMinimumSize = new Vector2(0f, 80f),
			WrapMode = (LineWrappingMode)1
		};
		((Node)val2).AddChild((Node)(object)_aiNotes, false, (InternalMode)0);
		HFlowContainer val6 = new HFlowContainer();
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		Button val7 = LocalizedKeyButton("ai.settings.save", "SecondaryButton", 0f, 34f);
		((BaseButton)val7).Pressed += SaveAiSettings;
		Button val8 = LocalizedKeyButton("ai.settings.forget", "SecondaryButton", 0f, 34f);
		((BaseButton)val8).Pressed += ForgetAiKey;
		((Node)val6).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val6, false, (InternalMode)0);
		_aiCredentialStatus = UiKit.Label(string.Empty, "DimLabel");
		_aiCredentialStatus.AutowrapMode = (AutowrapMode)3;
		((Node)val2).AddChild((Node)(object)_aiCredentialStatus, false, (InternalMode)0);
		Label val9 = LocalizedKeyLabel("ai.settings.security", "DimLabel");
		val9.AutowrapMode = (AutowrapMode)3;
		((Node)val2).AddChild((Node)(object)val9, false, (InternalMode)0);
		ShowAiSettings(AiTranslationSettings.LoadForLocale(MlmLocManager.EffectiveLocale));
		((Node)val).TreeExiting += ClearAiSecrets;
		UiKit.FollowFocus(val);
		return (Control)(object)val;
		static LineEdit NewField(bool secret = false)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			return new LineEdit
			{
				Secret = secret,
				CustomMinimumSize = new Vector2(0f, 34f),
				SizeFlagsHorizontal = (SizeFlags)3
			};
		}
	}

	private static void ShowAiSettings(AiTranslationSettings settings)
	{
		_aiSaved = settings;
		if (_aiProvider != null && GodotObject.IsInstanceValid((GodotObject)(object)_aiProvider))
		{
			_aiProvider.Select(Array.IndexOf(AiProviderPreset.All, settings.Preset));
			_aiEndpoint.Text = (settings.Preset.IsPreset ? string.Empty : settings.Endpoint);
			_aiModel.Text = settings.Model;
			_aiNotes.Text = settings.Notes;
			ShowAiProvider();
		}
	}

	private static void OnAiProviderSelected(long index)
	{
		AiProviderPreset selectedAiPreset = SelectedAiPreset;
		bool flag = _aiSaved.Preset == selectedAiPreset;
		_aiEndpoint.Text = (selectedAiPreset.IsPreset ? string.Empty : (flag ? _aiSaved.Endpoint : string.Empty));
		_aiModel.Text = (flag ? _aiSaved.Model : selectedAiPreset.DefaultModel);
		ClearAiInputs();
		ShowAiProvider();
	}

	private static void ShowAiProvider()
	{
		AiProviderPreset selectedAiPreset = SelectedAiPreset;
		bool flag = _aiSaved.HasKey && _aiSaved.Preset == selectedAiPreset;
		((CanvasItem)_aiEndpointRow).Visible = !selectedAiPreset.IsPreset;
		((CanvasItem)_aiGetKey).Visible = selectedAiPreset.KeyPage != null;
		_aiModel.PlaceholderText = selectedAiPreset.DefaultModel;
		_aiModelLabel.Text = MlmLocManager.Get(selectedAiPreset.IsPreset ? "ai.settings.modelPreset" : "ai.settings.model");
		_aiKeyInput.PlaceholderText = (flag ? MlmLocManager.Get("ai.settings.keySaved") : string.Empty);
		SetAiSettingsStatus(flag ? "ai.settings.ready" : (_aiSaved.NeedsKeyAgain ? "ai.settings.reenterKey" : (selectedAiPreset.IsPreset ? "ai.settings.noKey" : "ai.settings.noKeyCustom")));
	}

	private static void OpenAiKeyPage()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		string keyPage = SelectedAiPreset.KeyPage;
		if (keyPage != null && (int)OS.ShellOpen(keyPage) != 0)
		{
			SetAiSettingsStatus("ai.settings.getKeyFailed", ("url", keyPage));
		}
	}

	private static void SetAiSettingsStatus(string key, params (string Name, object? Value)[] args)
	{
		if (_aiCredentialStatus != null && GodotObject.IsInstanceValid((GodotObject)(object)_aiCredentialStatus))
		{
			_aiCredentialStatus.Text = MlmLocManager.Get(key, args);
		}
		UpdateTranslationControls();
	}

	private static void SaveAiSettings()
	{
		if (_pumpActive || _aiCancellation != null)
		{
			return;
		}
		try
		{
			AiProviderPreset selectedAiPreset = SelectedAiPreset;
			string text = _aiModel.Text.Trim();
			AiTranslationSettings aiTranslationSettings = new AiTranslationSettings
			{
				Provider = selectedAiPreset.Id,
				Endpoint = (selectedAiPreset.IsPreset ? selectedAiPreset.Endpoint : _aiEndpoint.Text.Trim()),
				Model = ((text.Length == 0) ? selectedAiPreset.DefaultModel : text),
				Notes = _aiNotes.Text
			};
			aiTranslationSettings.Validate();
			string text2 = _aiKeyInput.Text;
			if (!string.IsNullOrWhiteSpace(text2))
			{
				aiTranslationSettings.StoreKey(text2);
			}
			else
			{
				AiTranslationSettings aiTranslationSettings2 = AiTranslationSettings.Load();
				if (!aiTranslationSettings2.HasKey || !SameAiService(aiTranslationSettings2, aiTranslationSettings))
				{
					throw new TranslationFileException("key");
				}
				byte[] array = aiTranslationSettings2.ReadKey();
				try
				{
					aiTranslationSettings.StoreKey(Encoding.UTF8.GetString(array));
				}
				finally
				{
					CryptographicOperations.ZeroMemory(array);
				}
			}
			aiTranslationSettings.Save();
			ShowAiSettings(aiTranslationSettings);
			SetAiSettingsStatus("ai.settings.saved");
		}
		catch (TranslationFileException ex)
		{
			SetAiSettingsStatus("translations.error." + ex.Reason);
		}
		catch
		{
			SetAiSettingsStatus("translations.error.io");
		}
		finally
		{
			ClearAiInputs();
		}
	}

	private static bool SameAiService(AiTranslationSettings saved, AiTranslationSettings edited)
	{
		try
		{
			return string.Equals(saved.CompletionEndpoint().Host, edited.CompletionEndpoint().Host, StringComparison.OrdinalIgnoreCase);
		}
		catch (TranslationFileException)
		{
			return false;
		}
	}

	private static void ForgetAiKey()
	{
		ClearAiSecrets();
		try
		{
			AiTranslationSettings aiTranslationSettings = AiTranslationSettings.LoadForLocale(MlmLocManager.EffectiveLocale);
			aiTranslationSettings.StoredKey = null;
			aiTranslationSettings.EncryptedKey = null;
			aiTranslationSettings.Save();
			ShowAiSettings(aiTranslationSettings);
			SetAiSettingsStatus("ai.settings.forgotten");
		}
		catch
		{
			SetAiSettingsStatus("translations.error.io");
		}
	}

	private static void ClearAiInputs()
	{
		if (_aiKeyInput != null && GodotObject.IsInstanceValid((GodotObject)(object)_aiKeyInput))
		{
			_aiKeyInput.Text = "";
		}
	}

	private static void ClearAiSecrets()
	{
		_aiCancellation?.Cancel();
		ClearAiInputs();
	}

	private static void RefreshTranslatedMetadata(LaunchEntry entry)
	{
		entry.TranslatedName = "";
		entry.TranslatedDescription = "";
		if (entry.IsManager || entry.Mod.manifest == null)
		{
			return;
		}
		try
		{
			ApplyTranslatedMetadata(entry, ModTranslations.Folder(entry.Mod), GameTranslationLanguage);
		}
		catch
		{
		}
	}

	private static void RefreshTranslatedMetadata(IReadOnlyList<LaunchEntry> entries)
	{
		string root;
		string gameTranslationLanguage;
		HashSet<string> hashSet;
		try
		{
			root = ModTranslations.Root;
			gameTranslationLanguage = GameTranslationLanguage;
			hashSet = (Directory.Exists(root) ? Directory.EnumerateDirectories(root).Select(Path.GetFileName).OfType<string>()
				.ToHashSet<string>(StringComparer.OrdinalIgnoreCase) : new HashSet<string>(StringComparer.OrdinalIgnoreCase));
		}
		catch
		{
			foreach (LaunchEntry entry in entries)
			{
				RefreshTranslatedMetadata(entry);
			}
			return;
		}
		foreach (LaunchEntry entry2 in entries)
		{
			entry2.TranslatedName = "";
			entry2.TranslatedDescription = "";
			if (entry2.IsManager || entry2.Mod.manifest == null)
			{
				continue;
			}
			try
			{
				string text = ModTranslationFiles.ModFolder(root, ModTranslations.Id(entry2.Mod));
				if (hashSet.Contains(Path.GetFileName(text)))
				{
					ApplyTranslatedMetadata(entry2, text, gameTranslationLanguage);
				}
			}
			catch
			{
			}
		}
	}

	private static void ApplyTranslatedMetadata(LaunchEntry entry, string folder, string language)
	{
		if (!AiTranslationDocument.Enabled(folder))
		{
			return;
		}
		AiTranslationDocument.Metadata metadata = AiTranslationDocument.ReadMetadata(folder, language);
		if (!(metadata == null))
		{
			if (metadata.SourceName == entry.Name)
			{
				entry.TranslatedName = metadata.Name;
			}
			if (metadata.SourceDescription == entry.ManifestDescription)
			{
				entry.TranslatedDescription = metadata.Description;
			}
		}
	}

	private static void ToggleSelectedTranslation()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Invalid comparison between Unknown and I4
		if (_pumpActive || _aiCancellation != null)
		{
			return;
		}
		LaunchEntry selected = _selected;
		if (selected == null)
		{
			return;
		}
		try
		{
			string folder = ModTranslations.Folder(selected.Mod);
			AiTranslationDocument.SetEnabled(folder, !AiTranslationDocument.Enabled(folder));
			RefreshTranslatedMetadata(selected);
			UpdateDetails();
			QueueRebuild();
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get(((int)selected.Mod.state == 1) ? "ai.original.restart" : "ai.original.changed")));
		}
		catch (Exception exception)
		{
			ReportTranslationError(exception);
		}
	}

	private static List<VanillaTable> ReadVanillaTables(string language)
	{
		List<VanillaTable> list = new List<VanillaTable>();
		if (language == "eng")
		{
			return list;
		}
		string text = "res://localization/eng";
		try
		{
			foreach (string item in (from f in DirAccess.GetFilesAt(text)
				where f.EndsWith(".json", StringComparison.Ordinal)
				select f).Order<string>(StringComparer.Ordinal))
			{
				string text2 = "res://localization/" + language + "/" + item;
				if (FileAccess.FileExists(text2))
				{
					try
					{
						list.Add(new VanillaTable(item, ModTranslationFiles.Parse(Encoding.UTF8.GetBytes(FileAccess.GetFileAsString(text + "/" + item))), ModTranslationFiles.Parse(Encoding.UTF8.GetBytes(FileAccess.GetFileAsString(text2)))));
					}
					catch
					{
					}
				}
			}
		}
		catch
		{
		}
		return list;
	}

	private static void AddManualTranslationReferences(string folder, string language, IReadOnlyDictionary<string, byte[]> original, List<TranslationReference> references)
	{
		foreach (IGrouping<string, KeyValuePair<string, byte[]>> item in from p in original
			group p by p.Key.Substring(p.Key.IndexOf('/') + 1))
		{
			string text = Path.Combine(ModTranslationFiles.LanguageFolder(folder, language), item.Key.Replace('/', Path.DirectorySeparatorChar));
			ModTranslationFiles.RejectLinks(folder, text);
			if (!File.Exists(text))
			{
				continue;
			}
			KeyValuePair<string, byte[]> keyValuePair = item.FirstOrDefault((KeyValuePair<string, byte[]> p) => p.Key.StartsWith("eng/", StringComparison.Ordinal));
			Dictionary<string, string> dictionary = ModTranslationFiles.Parse((keyValuePair.Key == null) ? item.First().Value : keyValuePair.Value);
			foreach (var (key, text4) in ModTranslationFiles.Parse(ModTranslationFiles.ReadFile(text)))
			{
				if (dictionary.TryGetValue(key, out var value) && value.Length < 300 && text4.Length < 500 && references.Count < 180)
				{
					references.Add(new TranslationReference(value, text4, "mod"));
				}
			}
		}
	}

	private static async void TranslateSelectedMod()
	{
		if (_aiCancellation != null || _pumpActive)
		{
			return;
		}
		LaunchEntry entry = _selected;
		if (entry == null)
		{
			return;
		}
		AiTranslationSettings settings = AiTranslationSettings.Load();
		byte[] key;
		try
		{
			if (!settings.HasKey)
			{
				throw new TranslationFileException(settings.NeedsKeyAgain ? "reenterKey" : "noKey");
			}
			settings.Validate();
			key = settings.ReadKey();
		}
		catch (TranslationFileException ex)
		{
			OpenSettings();
			ShowSettingsSection(SettingsSection.Ai);
			string reason = ex.Reason;
			if ((!(reason == "noKey") && !(reason == "reenterKey")) || 1 == 0)
			{
				SetAiSettingsStatus("translations.error." + ex.Reason);
			}
			return;
		}
		CancellationTokenSource cancellation = new CancellationTokenSource();
		_aiCancellation = cancellation;
		string language = TranslationLanguage;
		AiTranslationDiagnostics diagnostics = new AiTranslationDiagnostics();
		Timer waitTicker = null;
		GD.Print("[ModLaunchManager] AI translation started.");
		UpdateTranslationControls();
		int shownDone;
		int shownTotal;
		try
		{
			string folder = ModTranslations.Folder(entry.Mod);
			Dictionary<string, byte[]> original = new Dictionary<string, byte[]>();
			if (entry.Mod.manifest?.hasPck ?? false)
			{
				try
				{
					original = PckLocalizationArchive.Read(ModTranslations.PackPath(entry.Mod), entry.Id);
				}
				catch (TranslationFileException ex2) when (ex2.Reason == "noFiles")
				{
				}
			}
			string sourceText = entry.Name + "\n" + entry.ManifestDescription + "\n" + string.Join("\n", original.Values.SelectMany((byte[] b) => ModTranslationFiles.Parse(b).Values));
			diagnostics.Stage = AiTranslationStage.ReadReferences;
			List<VanillaTable> vanilla = ReadVanillaTables(language);
			_aiProgress.Text = MlmLocManager.Get("ai.progress", ("name", entry.Name), ("done", 0), ("total", "…"));
			AiFirstReplyWait wait = new AiFirstReplyWait(Time.GetTicksMsec());
			shownDone = 0;
			shownTotal = 0;
			Timer ticker = new Timer
			{
				WaitTime = 1.0,
				Autostart = true
			};
			ticker.Timeout += delegate
			{
				if (wait.Replied)
				{
					ticker.Stop();
				}
				else if (_aiCancellation == cancellation && _aiProgress != null && GodotObject.IsInstanceValid((GodotObject)(object)_aiProgress))
				{
					int? num = wait.Seconds(Time.GetTicksMsec());
					if (num.HasValue)
					{
						int valueOrDefault = num.GetValueOrDefault();
						_aiProgress.Text = Waiting(valueOrDefault);
					}
				}
			};
			((Node)_aiProgress).AddChild((Node)(object)ticker, false, (InternalMode)0);
			waitTicker = ticker;
			HttpClient http = AiTranslationClient.CreateHttpClient();
			try
			{
				AiTranslationCheckpoint checkpoint = new AiTranslationCheckpoint(folder, language);
				int concurrency = settings.Preset.Concurrency;
				AiTranslationDocument aiTranslationDocument = await Task.Run(delegate
				{
					List<TranslationReference> list = AiTranslationReferences.Terms(vanilla, sourceText);
					AddManualTranslationReferences(folder, language, original, list);
					List<TranslationReference> examples = AiTranslationReferences.Examples(vanilla, AiTranslationReferences.ModTexts(original).Prepend(("", "description", entry.ManifestDescription)));
					return AiModTranslator.GenerateAsync(new AiTranslationClient(http, diagnostics), settings, key, language, entry.Name, entry.ManifestDescription, original, list, delegate(int done, int total)
					{
						Defer(delegate
						{
							GD.Print($"{"[ModLaunchManager]"} AI translation progress: completedEntries={done}, totalEntries={total}.");
							wait.Progress(done);
							shownDone = done;
							shownTotal = total;
							if (_aiCancellation == cancellation && _aiProgress != null && GodotObject.IsInstanceValid((GodotObject)(object)_aiProgress))
							{
								Label? aiProgress = _aiProgress;
								int? num2 = wait.Seconds(Time.GetTicksMsec());
								string text;
								if (num2.HasValue)
								{
									int valueOrDefault2 = num2.GetValueOrDefault();
									text = Waiting(valueOrDefault2);
								}
								else
								{
									text = MlmLocManager.Get("ai.progress", ("name", entry.Name), ("done", done), ("total", total));
								}
								aiProgress.Text = text;
							}
						});
					}, cancellation.Token, ModTranslations.ValidateFormat, checkpoint, delegate(AiTranslationRetryNotice notice)
					{
						Defer(delegate
						{
							int num3 = (int)Math.Ceiling(notice.Delay.TotalSeconds);
							GD.Print($"{"[ModLaunchManager]"} AI translation retry: kind={notice.Kind}, attempt={notice.Attempt}, entries={notice.Entries}, waitSeconds={num3}" + ((notice.Issues.Length > 0) ? (", issues=" + notice.Issues + ".") : "."));
							wait.Retry(notice, Time.GetTicksMsec());
							if (_aiCancellation == cancellation && _aiProgress != null && GodotObject.IsInstanceValid((GodotObject)(object)_aiProgress))
							{
								_aiProgress.Text = ((notice.Kind == AiTranslationRetryKind.Format) ? MlmLocManager.Get("ai.retry.format", ("count", notice.Entries), ("attempt", notice.Attempt)) : MlmLocManager.Get("ai.retry.wait", ("seconds", num3), ("attempt", notice.Attempt)));
							}
						});
					}, null, concurrency, examples);
				});
				cancellation.Token.ThrowIfCancellationRequested();
				if (_pumpActive || _root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root))
				{
					return;
				}
				diagnostics.Stage = AiTranslationStage.CheckSource;
				if (entry.Name != aiTranslationDocument.SourceName || entry.ManifestDescription != aiTranslationDocument.SourceDescription)
				{
					throw new TranslationFileException("changed");
				}
				if (original.Count > 0)
				{
					Dictionary<string, byte[]> current = PckLocalizationArchive.Read(ModTranslations.PackPath(entry.Mod), entry.Id);
					if (current.Count != original.Count || original.Any<KeyValuePair<string, byte[]>>((KeyValuePair<string, byte[]> p) => !current.TryGetValue(p.Key, out byte[] value) || !((ReadOnlySpan<byte>)value).SequenceEqual((ReadOnlySpan<byte>)p.Value)))
					{
						throw new TranslationFileException("changed");
					}
				}
				diagnostics.Stage = AiTranslationStage.Save;
				aiTranslationDocument.Save(folder, language);
				if (aiTranslationDocument.SkippedEntries == 0)
				{
					try
					{
						checkpoint.Clear();
					}
					catch
					{
						GD.Print("[ModLaunchManager] AI translation draft cleanup deferred; final translation is saved.");
					}
				}
				diagnostics.Stage = AiTranslationStage.Complete;
				GD.Print("[ModLaunchManager] AI translation saved: " + diagnostics.Usage() + ((aiTranslationDocument.SkippedEntries > 0) ? $", keptOriginal={aiTranslationDocument.SkippedEntries} ({aiTranslationDocument.SkippedSummary})." : "."));
				RefreshTranslatedMetadata(entry);
				QueueRebuild();
				UpdateDetails();
				ShowFeedback(PlanResult.Ok((aiTranslationDocument.SkippedEntries == 0) ? MlmLocManager.Get((entry.Mod.manifest?.hasPck ?? false) ? "ai.completed" : "ai.completedTitleOnly", ("name", entry.Name), ("language", language)) : MlmLocManager.Get("ai.completedPartial", ("name", entry.Name), ("language", language), ("count", aiTranslationDocument.SkippedEntries))));
			}
			finally
			{
				if (http != null)
				{
					((IDisposable)http).Dispose();
				}
			}
		}
		catch (OperationCanceledException)
		{
			GD.Print("[ModLaunchManager] AI translation cancelled: " + diagnostics.Usage() + ".");
			if (_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				ShowFeedback(PlanResult.Ok(MlmLocManager.Get("ai.cancelled") + AiSavedProgress(diagnostics)));
			}
		}
		catch (Exception error)
		{
			GD.Print($"{"[ModLaunchManager]"} {diagnostics.Failure(error)} {diagnostics.Usage()}.");
			if (_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get(diagnostics.FeedbackKey(error)) + AiSavedProgress(diagnostics)));
			}
		}
		finally
		{
			CryptographicOperations.ZeroMemory(key);
			if (waitTicker != null && GodotObject.IsInstanceValid((GodotObject)(object)waitTicker))
			{
				((Node)waitTicker).QueueFree();
			}
			_aiCancellation = null;
			cancellation.Dispose();
			if (_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				if (_aiProgress != null)
				{
					_aiProgress.Text = "";
				}
				UpdateTranslationControls();
			}
		}
		string Waiting(int seconds)
		{
			return MlmLocManager.Get("ai.progress.waiting", ("name", entry.Name), ("done", shownDone), ("total", shownTotal), ("seconds", seconds));
		}
	}

	private static string AiSavedProgress(AiTranslationDiagnostics diagnostics)
	{
		if (diagnostics.CompletedEntries <= 0)
		{
			return "";
		}
		return "\n" + MlmLocManager.Get("ai.resume.saved", ("done", diagnostics.CompletedEntries), ("total", diagnostics.TotalEntries));
	}

	internal static bool AutoLaunchRequested(IEnumerable<string> engineArgs, IEnumerable<string> userArgs, string? variable)
	{
		if (!userArgs.Contains("--mlm-autolaunch") && !engineArgs.Contains("--mlm-autolaunch"))
		{
			return variable == "1";
		}
		return true;
	}

	private static void ScheduleAutoLaunch(Button start)
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		if (_autoLaunchUsed || !AutoLaunchRequested(OS.GetCmdlineArgs(), OS.GetCmdlineUserArgs(), Environment.GetEnvironmentVariable("MLM_AUTOLAUNCH")))
		{
			return;
		}
		_autoLaunchUsed = true;
		ulong deadline = Time.GetTicksMsec() + 120000;
		Timer poll = new Timer
		{
			WaitTime = 0.5,
			Autostart = true
		};
		poll.Timeout += delegate
		{
			if (Time.GetTicksMsec() > deadline)
			{
				GD.Print("[ModLaunchManager] automatic launch gave up: the launcher never became ready.");
				((Node)poll).QueueFree();
			}
			else if (AutoLaunchReady())
			{
				((Node)poll).QueueFree();
				GD.Print("[ModLaunchManager] launching automatically (--mlm-autolaunch).");
				StartLoading();
			}
		};
		((Node)start).AddChild((Node)(object)poll, false, (InternalMode)0);
	}

	private static bool AutoLaunchReady()
	{
		if (_plan == null || _start == null || ((BaseButton)_start).Disabled || !((CanvasItem)_start).IsVisibleInTree())
		{
			return false;
		}
		if (UpdateAnnouncement.IsOpen || VanillaLaunch.IsActive || VanillaLaunch.RequiresRestart)
		{
			return false;
		}
		if (_aiCancellation != null || _pendingCanonicalAction != 0)
		{
			return false;
		}
		if (FabMenuOpen || TourOpen)
		{
			return false;
		}
		if (WorkshopRemovalPrompt.IsOpen || WorkshopSubscription.Busy)
		{
			return false;
		}
		if (WorkshopBranches.IsBusy || WorkshopUpdates.DownloadingCount > 0)
		{
			return false;
		}
		if (WorkshopCollectionImportDialog.IsOpen || WorkshopDownloadsDialog.IsOpen)
		{
			return false;
		}
		return true;
	}

	private static Control BuildChangeNotesBanner()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		_changeNotesDismissed = false;
		_changeNotesBanner = new PanelContainer
		{
			Name = StringName.op_Implicit("WorkshopChangeNotesBanner"),
			ThemeTypeVariation = StringName.op_Implicit("BarPanel"),
			SizeFlagsHorizontal = (SizeFlags)3,
			Visible = false
		};
		HBoxContainer val = new HBoxContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_changeNotesBannerText = UiKit.Label(string.Empty, "MutedLabel");
		((Control)_changeNotesBannerText).SizeFlagsHorizontal = (SizeFlags)3;
		_changeNotesBannerText.VerticalAlignment = (VerticalAlignment)1;
		_changeNotesBannerText.AutowrapMode = (AutowrapMode)3;
		_changeNotesOpen = LocalizedKeyButton("changeNotes.banner.open", "SecondaryButton", 0f, 32f);
		BindKeyTooltip((Control)(object)_changeNotesOpen, "changeNotes.banner.open.tooltip");
		((Control)_changeNotesOpen).SizeFlagsVertical = (SizeFlags)4;
		((BaseButton)_changeNotesOpen).Pressed += OpenChangeNotes;
		Button val2 = LocalizedKeyButton("changeNotes.banner.dismiss", "MiniButton", 0f, 32f);
		BindKeyTooltip((Control)(object)val2, "changeNotes.banner.dismiss.tooltip");
		((Control)val2).SizeFlagsVertical = (SizeFlags)4;
		((BaseButton)val2).Pressed += delegate
		{
			_changeNotesDismissed = true;
			UpdateChangeNotesChrome();
			Button? start = _start;
			if (start != null)
			{
				((Control)start).GrabFocus();
			}
		};
		((Node)val).AddChild((Node)(object)_changeNotesBannerText, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_changeNotesOpen, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)_changeNotesBanner).AddChild((Node)(object)val, false, (InternalMode)0);
		BindLocalization((Control)(object)_changeNotesBanner, UpdateChangeNotesChrome);
		return (Control)(object)_changeNotesBanner;
	}

	private static void StartChangeNotes()
	{
		WorkshopChangeNoteTranslations.Reset();
		WorkshopChangeNoteTranslations.Runner = TranslateChangeNotes;
		WorkshopChangeNoteTranslations.Changed = OnChangeNotesChanged;
		WorkshopChangeNoteFeed.Start(OnChangeNotesChanged);
	}

	private static void StopChangeNotes()
	{
		_changeNoteTranslation?.Cancel();
		WorkshopChangeNoteFeed.Stop();
		WorkshopChangeNotesDialog.Close();
		WorkshopChangeNoteTranslations.Reset();
	}

	private static void OnChangeNotesChanged()
	{
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			UpdateChangeNotesChrome();
			UpdateDetails();
		}
	}

	private static void UpdateChangeNotesChrome()
	{
		IReadOnlyList<WorkshopChangeNoteItem> updates = WorkshopChangeNoteFeed.Updates;
		if (_changeNotesBanner != null && GodotObject.IsInstanceValid((GodotObject)(object)_changeNotesBanner))
		{
			((CanvasItem)_changeNotesBanner).Visible = updates.Count > 0 && !_changeNotesDismissed;
		}
		if (_changeNotesBannerText != null && GodotObject.IsInstanceValid((GodotObject)(object)_changeNotesBannerText))
		{
			_changeNotesBannerText.Text = WorkshopChangeNoteText.Summary(updates);
		}
		if (_changeNotesOpen != null && GodotObject.IsInstanceValid((GodotObject)(object)_changeNotesOpen))
		{
			((BaseButton)_changeNotesOpen).Disabled = _pumpActive;
		}
		WorkshopChangeNotesDialog.Refresh();
	}

	private static void OpenChangeNotes()
	{
		if (!_pumpActive)
		{
			WorkshopChangeNotesDialog.Show(WorkshopItemName, OpenWorkshopChangeNotesPage);
		}
	}

	private static string WorkshopItemName(ulong itemId)
	{
		List<string> list = (from entry in _plan?.Entries
			where WorkshopUpdates.ItemIdFor(entry.Mod) == itemId
			select entry.DisplayName).Distinct<string>(StringComparer.Ordinal).ToList() ?? new List<string>();
		if (list.Count == 1)
		{
			return list[0];
		}
		string text = WorkshopUpdates.AllStatuses.FirstOrDefault((WorkshopStatus status) => status.ItemId == itemId)?.Title;
		if (string.IsNullOrWhiteSpace(text))
		{
			if (list.Count <= 0)
			{
				return itemId.ToString();
			}
			return string.Join(" / ", list);
		}
		return text;
	}

	private static void OpenWorkshopChangeNotesPage(ulong itemId)
	{
		string item = WorkshopItemName(itemId);
		string text = WorkshopChangeNoteFeed.ChangeNotesUrl(itemId);
		try
		{
			ShowFeedback(SteamOverlay.OpenPage(text) switch
			{
				PageOpenResult.InGame => PlanResult.Ok(MlmLocManager.Get("changeNotes.feedback.pageOpened", ("name", item))), 
				PageOpenResult.InSteam => PlanResult.Ok(MlmLocManager.Get("changeNotes.feedback.pageOpenedInSteam", ("name", item))), 
				PageOpenResult.InBrowser => PlanResult.Ok(MlmLocManager.Get("changeNotes.feedback.pageOpenedInBrowser", ("name", item))), 
				_ => PlanResult.Error(MlmLocManager.Get("workshop.feedback.browserOpenFailed", ("url", text))), 
			});
		}
		catch (Exception ex)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("workshop.feedback.pageOpenFailed", ("error", ex.Message))));
		}
	}

	private static VBoxContainer BuildChangeNotesDetails()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Expected O, but got Unknown
		_detailChangeNotesSignature = string.Empty;
		_detailChangeNotes = new VBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_detailChangeNotes).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)_detailChangeNotes).AddChild((Node)(object)LocalizedKeyCaption("changeNotes.details.caption"), false, (InternalMode)0);
		_detailChangeNotesList = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_detailChangeNotesList).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)_detailChangeNotes).AddChild((Node)(object)_detailChangeNotesList, false, (InternalMode)0);
		HFlowContainer val = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		_detailChangeNotesPage = LocalizedKeyButton("changeNotes.action.viewAll", "SecondaryButton", 0f, 34f);
		BindKeyTooltip((Control)(object)_detailChangeNotesPage, "changeNotes.action.viewAll.tooltip");
		((BaseButton)_detailChangeNotesPage).Pressed += delegate
		{
			if (_selected != null)
			{
				ulong num = WorkshopUpdates.ItemIdFor(_selected.Mod);
				if (num != 0)
				{
					OpenWorkshopChangeNotesPage(num);
				}
			}
		};
		_detailChangeNotesTranslate = UiKit.Button(string.Empty, "SecondaryButton", 0f, 34f);
		((BaseButton)_detailChangeNotesTranslate).Pressed += delegate
		{
			if (_selected != null)
			{
				ulong num2 = WorkshopUpdates.ItemIdFor(_selected.Mod);
				if (num2 != 0)
				{
					WorkshopChangeNoteTranslations.Activate(num2);
				}
			}
		};
		((Node)val).AddChild((Node)(object)_detailChangeNotesPage, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_detailChangeNotesTranslate, false, (InternalMode)0);
		((Node)_detailChangeNotes).AddChild((Node)(object)val, false, (InternalMode)0);
		return _detailChangeNotes;
	}

	private static void UpdateChangeNotesDetails(ulong itemId)
	{
		if (_detailChangeNotes == null || _detailChangeNotesList == null || !GodotObject.IsInstanceValid((GodotObject)(object)_detailChangeNotes))
		{
			return;
		}
		WorkshopChangeNoteItem workshopChangeNoteItem = ((itemId != 0) ? WorkshopChangeNoteFeed.For(itemId) : null);
		IReadOnlyList<WorkshopChangeNote> readOnlyList2;
		if (workshopChangeNoteItem == null)
		{
			if (itemId == 0)
			{
				IReadOnlyList<WorkshopChangeNote> readOnlyList = Array.Empty<WorkshopChangeNote>();
				readOnlyList2 = readOnlyList;
			}
			else
			{
				readOnlyList2 = WorkshopChangeNoteFeed.RecentNotes(itemId);
			}
		}
		else
		{
			readOnlyList2 = workshopChangeNoteItem.Notes;
		}
		IReadOnlyList<WorkshopChangeNote> readOnlyList3 = readOnlyList2;
		string text = ((workshopChangeNoteItem == null) ? null : WorkshopChangeNoteText.StateMessage(workshopChangeNoteItem));
		((CanvasItem)_detailChangeNotes).Visible = text != null || readOnlyList3.Count > 0;
		if (_detailChangeNotesPage != null)
		{
			((BaseButton)_detailChangeNotesPage).Disabled = _pumpActive;
		}
		WorkshopChangeNoteTranslationView workshopChangeNoteTranslationView = ((itemId != 0) ? WorkshopChangeNoteTranslations.ViewFor(itemId) : WorkshopChangeNoteTranslationView.None);
		if (_detailChangeNotesTranslate != null)
		{
			WorkshopChangeNoteText.ConfigureTranslateButton(_detailChangeNotesTranslate, workshopChangeNoteTranslationView, _pumpActive);
		}
		string text2 = $"{MlmLocManager.EffectiveLocale}|{itemId}|{workshopChangeNoteItem?.State}|{text}|{workshopChangeNoteTranslationView}|" + WorkshopChangeNoteTranslations.Language + "|" + string.Join(",", readOnlyList3.Select((WorkshopChangeNote note) => note.Time));
		if (text2 == _detailChangeNotesSignature)
		{
			return;
		}
		_detailChangeNotesSignature = text2;
		foreach (Node child in ((Node)_detailChangeNotesList).GetChildren(false))
		{
			((Node)_detailChangeNotesList).RemoveChild(child);
			child.QueueFree();
		}
		if (text != null)
		{
			Label val = UiKit.Label(text, (workshopChangeNoteItem != null && workshopChangeNoteItem.State == WorkshopChangeNoteState.Failed) ? "WarningLabel" : "DimLabel");
			val.AutowrapMode = (AutowrapMode)3;
			((Node)_detailChangeNotesList).AddChild((Node)(object)val, false, (InternalMode)0);
		}
		foreach (WorkshopChangeNote item in readOnlyList3.Take(3))
		{
			((Node)_detailChangeNotesList).AddChild((Node)(object)WorkshopChangeNoteText.NoteView(itemId, item, 600), false, (InternalMode)0);
		}
		if (readOnlyList3.Count > 3 || (workshopChangeNoteItem != null && workshopChangeNoteItem.Truncated))
		{
			Label val2 = UiKit.Label(MlmLocManager.Get("changeNotes.note.more"), "DimLabel");
			val2.AutowrapMode = (AutowrapMode)3;
			((Node)_detailChangeNotesList).AddChild((Node)(object)val2, false, (InternalMode)0);
		}
	}

	private static async void TranslateChangeNotes(IReadOnlyList<ulong> itemIds)
	{
		if (_pumpActive || _changeNoteTranslation != null || WorkshopChangeNoteTranslations.Busy)
		{
			return;
		}
		string language = WorkshopChangeNoteTranslations.Language;
		Dictionary<string, (ulong ItemId, WorkshopChangeNote Note)> pending = new Dictionary<string, (ulong, WorkshopChangeNote)>(StringComparer.Ordinal);
		Dictionary<string, string> roles = new Dictionary<string, string>(StringComparer.Ordinal);
		int unusable = 0;
		foreach (ulong item in itemIds.Distinct())
		{
			string text = WorkshopItemName(item);
			foreach (WorkshopChangeNote item2 in WorkshopChangeNoteFeed.ShownNotes(item))
			{
				if (item2.Text.Length != 0 && WorkshopChangeNoteFeed.Translation(item, item2, language) == null)
				{
					try
					{
						new ProtectedTranslationText(item2.Text);
					}
					catch (TranslationFileException)
					{
						unusable++;
						continue;
					}
					string key = pending.Count.ToString(CultureInfo.InvariantCulture);
					pending[key] = (item, item2);
					roles[key] = "Steam Workshop update note of the mod \"" + text + "\", " + DisplayTime.Format(DateTimeOffset.FromUnixTimeSeconds(item2.Time));
				}
			}
		}
		if (pending.Count == 0)
		{
			if (unusable > 0)
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("ai.error.sourceFormat")));
			}
			return;
		}
		AiTranslationSettings settings = AiTranslationSettings.Load();
		byte[] key2;
		try
		{
			if (!settings.HasKey)
			{
				throw new TranslationFileException(settings.NeedsKeyAgain ? "reenterKey" : "noKey");
			}
			settings.Validate();
			key2 = settings.ReadKey();
		}
		catch (TranslationFileException ex2)
		{
			WorkshopChangeNotesDialog.Close();
			OpenSettings();
			ShowSettingsSection(SettingsSection.Ai);
			string reason = ex2.Reason;
			if ((!(reason == "noKey") && !(reason == "reenterKey")) || 1 == 0)
			{
				SetAiSettingsStatus("translations.error." + ex2.Reason);
			}
			return;
		}
		CancellationTokenSource cancellation = new CancellationTokenSource();
		_changeNoteTranslation = cancellation;
		WorkshopChangeNoteTranslations.Begin(pending.Values.Select<(ulong, WorkshopChangeNote), ulong>(((ulong ItemId, WorkshopChangeNote Note) entry) => entry.ItemId));
		AiTranslationDiagnostics diagnostics = new AiTranslationDiagnostics();
		GD.Print($"{"[ModLaunchManager]"} AI translation of {pending.Count} change note(s) started.");
		try
		{
			HttpClient http = AiTranslationClient.CreateHttpClient();
			try
			{
				List<VanillaTable> vanilla = ReadVanillaTables(language);
				string languageName = WorkshopChangeNoteTranslations.LanguageName(language);
				List<Dictionary<string, string>> batches = NoteBatches(pending);
				Dictionary<string, string> dictionary = await Task.Run(async delegate
				{
					List<TranslationReference> source = AiTranslationReferences.Terms(vanilla, string.Join("\n", pending.Values.Select<(ulong, WorkshopChangeNote), string>(((ulong ItemId, WorkshopChangeNote Note) entry) => entry.Note.Text)));
					AiTranslationBrief brief = new AiTranslationBrief(languageName, "Steam Workshop update notes", "Release notes that mod authors wrote for players about new versions of their mods. Keep mod names, version numbers and links; use the official game terms for cards, relics and mechanics.", Array.Empty<string>(), source.Take(180).ToList());
					AiTranslationClient client = new AiTranslationClient(http, diagnostics);
					Dictionary<string, string> results = new Dictionary<string, string>(StringComparer.Ordinal);
					foreach (Dictionary<string, string> item3 in batches)
					{
						AiTranslationSettings settings2 = settings;
						byte[] key3 = key2;
						Action<Dictionary<string, string>> accept = delegate(Dictionary<string, string> translated)
						{
							lock (results)
							{
								foreach (var (key4, value) in translated)
								{
									results[key4] = value;
								}
							}
						};
						CancellationToken token = cancellation.Token;
						IReadOnlyDictionary<string, string> roles2 = roles;
						await AiTranslationRetry.TranslateAsync(client, settings2, key3, brief, item3, null, accept, token, null, null, null, null, roles2, delegate
						{
						}).ConfigureAwait(continueOnCapturedContext: false);
					}
					return results;
				});
				foreach (var (key5, text6) in dictionary)
				{
					var (itemId, note) = pending[key5];
					WorkshopChangeNoteFeed.SaveTranslation(itemId, note, language, AiTranslationReferences.TidySpacing(language, text6));
				}
				int num = pending.Count - dictionary.Count + unusable;
				GD.Print($"{"[ModLaunchManager]"} AI translation of change notes finished: translated={dictionary.Count}, keptOriginal={num}, {diagnostics.Usage()}.");
				ShowFeedback((num == 0) ? PlanResult.Ok(MlmLocManager.Get("changeNotes.ai.completed", ("count", dictionary.Count), ("language", languageName))) : PlanResult.Warning(MlmLocManager.Get("changeNotes.ai.completedPartial", ("count", dictionary.Count), ("language", languageName), ("kept", num))));
			}
			finally
			{
				if (http != null)
				{
					((IDisposable)http).Dispose();
				}
			}
		}
		catch (OperationCanceledException)
		{
			GD.Print("[ModLaunchManager] AI translation of change notes cancelled: " + diagnostics.Usage() + ".");
		}
		catch (Exception error)
		{
			GD.Print($"{"[ModLaunchManager]"} change notes: {diagnostics.Failure(error)} {diagnostics.Usage()}.");
			if (_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get(diagnostics.FeedbackKey(error))));
			}
		}
		finally
		{
			CryptographicOperations.ZeroMemory(key2);
			if (_changeNoteTranslation == cancellation)
			{
				_changeNoteTranslation = null;
				WorkshopChangeNoteTranslations.End();
			}
			cancellation.Dispose();
		}
	}

	private static List<Dictionary<string, string>> NoteBatches(Dictionary<string, (ulong ItemId, WorkshopChangeNote Note)> pending)
	{
		List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
		int num = 0;
		foreach (KeyValuePair<string, (ulong, WorkshopChangeNote)> item2 in pending)
		{
			item2.Deconstruct(out var key, out var value);
			(ulong, WorkshopChangeNote) tuple = value;
			string key2 = key;
			WorkshopChangeNote item = tuple.Item2;
			if (dictionary.Count > 0 && num + item.Text.Length > 6000)
			{
				list.Add(dictionary);
				dictionary = new Dictionary<string, string>(StringComparer.Ordinal);
				num = 0;
			}
			dictionary[key2] = item.Text;
			num += item.Text.Length;
		}
		if (dictionary.Count > 0)
		{
			list.Add(dictionary);
		}
		return list;
	}

	private static void StartControllerNavigation()
	{
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Expected O, but got Unknown
		if (_root == null || _layer == null)
		{
			return;
		}
		_controllerMoveKey = null;
		_selectFirstRowAfterRebuild = false;
		_promptSignature = string.Empty;
		PromptGlyphs.Clear();
		_promptGlyphsAt = 0uL;
		BuildControllerPrompts(_layer);
		_controller = new LauncherController(_root, ControllerBack, ControllerHome);
		_controller.InputSourceDetected = delegate(string source)
		{
			GD.Print("[ModLaunchManager] launcher controller source: " + source + ".");
		};
		_controller.NativeControllerConnected = () => SteamControllerCount() > 0;
		_controller.RevealFocus = RevealControllerFocus;
		_controller.TextEntryRequested = ShowControllerKeyboard;
		_controller.ModeChanged = OnControllerModeChanged;
		_controller.FocusChanged = OnControllerFocusChanged;
		LauncherController.Scope mainScope = _controller.MainScope;
		mainScope.Accept = ControllerAccept;
		mainScope.Shortcut = ControllerShortcut;
		mainScope.Direction = ControllerDirection;
		mainScope.Redirect = ControllerRedirect;
		mainScope.Page = ControllerPage;
		mainScope.ScrollTarget = () => _detailScroll;
		if (_settingsPage != null)
		{
			_controller.AddScope(new LauncherController.Scope(_settingsPage, delegate
			{
				if (_settingsSection == SettingsSection.Root)
				{
					CloseSettings();
				}
				else
				{
					ShowSettingsSection(SettingsSection.Root);
				}
			}, delegate
			{
				Control? settingsRootView = _settingsRootView;
				return (Control?)(object)((settingsRootView == null) ? null : ((IEnumerable)((Node)settingsRootView).GetChildren(false)).OfType<Button>().FirstOrDefault());
			}));
		}
		if (_loadingCard != null)
		{
			_controller.AddScope(new LauncherController.Scope((Control)(object)_loadingCard, delegate
			{
			}, () => (Control?)(object)_restartAfterError)
			{
				Enabled = () => _pumpActive
			});
		}
		Button? start = _start;
		_startFocusIsDefault = start != null && ((Control)start).HasFocus();
		ulong detectUntil = Time.GetTicksMsec() + 5000;
		Timer detect = new Timer
		{
			WaitTime = 0.25,
			Autostart = true
		};
		detect.Timeout += delegate
		{
			if (_controller == null || _controller.ControllerMode || Time.GetTicksMsec() > detectUntil)
			{
				((Node)detect).QueueFree();
			}
			else if (SteamControllerCount() > 0)
			{
				_controller.ActivateForConnectedController();
				((Node)detect).QueueFree();
			}
		};
		((Node)_root).AddChild((Node)(object)detect, false, (InternalMode)0);
		UpdateControllerPrompts();
	}

	private static int SteamControllerCount()
	{
		try
		{
			return SteamInput.GetConnectedControllers(SteamControllers);
		}
		catch (InvalidOperationException)
		{
			return 0;
		}
	}

	private static Control? ControllerHome()
	{
		ModReorderTree modTree = _modTree;
		if (modTree != null && ((CanvasItem)modTree).Visible)
		{
			return (Control?)(object)_modTree;
		}
		Button clearFilters = _clearFilters;
		if (clearFilters != null && ((CanvasItem)clearFilters).IsVisibleInTree())
		{
			return (Control?)(object)clearFilters;
		}
		return (Control?)(object)_start;
	}

	private static void OnControllerModeChanged()
	{
		_promptGlyphsAt = 0uL;
		LauncherController? controller = _controller;
		if (controller != null && controller.ControllerMode && _modTree != null)
		{
			TreeItem root = ((Tree)_modTree).GetRoot();
			for (TreeItem val = ((root != null) ? root.GetFirstChild() : null); val != null; val = val.GetNext())
			{
				val.Collapsed = false;
			}
		}
		else
		{
			EndControllerMove();
		}
		if (_groupListView)
		{
			QueueRebuild();
		}
		RefreshTourText();
		UpdateControllerPrompts();
	}

	private static void OnControllerFocusChanged()
	{
		_startFocusIsDefault = false;
		if (_controllerMoveKey != null)
		{
			ModReorderTree? modTree = _modTree;
			if ((modTree == null || !((Control)modTree).HasFocus()) && _controller?.ActiveRoot == _root)
			{
				EndControllerMove();
			}
		}
		UpdateControllerPrompts();
	}

	private static bool RevealControllerFocus()
	{
		if (_startFocusIsDefault && !_pumpActive)
		{
			Button? start = _start;
			if (start != null && ((Control)start).HasFocus())
			{
				ModReorderTree modTree = _modTree;
				if (modTree != null && ((CanvasItem)modTree).Visible)
				{
					FocusModList();
					return true;
				}
			}
		}
		return false;
	}

	private static void FocusModList()
	{
		if (_modTree == null || !((CanvasItem)_modTree).IsVisibleInTree())
		{
			Control? obj = ControllerHome();
			if (obj != null)
			{
				obj.GrabFocus();
			}
			return;
		}
		((Control)_modTree).GrabFocus();
		if (EntryFor(((Tree)_modTree).GetSelected()) == null && TreeBindings.Count > 0)
		{
			int num = TreeBindings.FindIndex((TreeBinding binding) => binding.Entry == _selected);
			SelectTreeRow((num >= 0) ? TreeBindings[num] : TreeBindings[0]);
		}
		ScrollSelectionIntoView();
	}

	private static void SelectTreeRow(TreeBinding binding)
	{
		binding.Item.Select(2);
		ModReorderTree? modTree = _modTree;
		if (modTree != null)
		{
			((Tree)modTree).ScrollToItem(binding.Item, false);
		}
		RevealBelowPinnedTitle(binding.Item);
	}

	private static bool ControllerAccept(Control focused)
	{
		if (focused == _modTree)
		{
			if (_controllerMoveKey != null)
			{
				EndControllerMove();
			}
			else
			{
				ToggleFromController();
			}
			return true;
		}
		if (focused == _detailDescription && !_pumpActive)
		{
			BeginDescriptionEdit();
			return true;
		}
		return false;
	}

	private static void ToggleFromController()
	{
		if (_pumpActive || _plan == null || _modTree == null)
		{
			return;
		}
		LaunchEntry entry = EntryFor(((Tree)_modTree).GetSelected());
		if (entry == null)
		{
			FocusModList();
			return;
		}
		if (!entry.CanToggle)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("plan.toggle.managerCannotDisable")));
			return;
		}
		int index = TreeBindings.FindIndex((TreeBinding binding) => binding.Entry == entry);
		if (_groupListView)
		{
			ToggleKeepingCursor(entry, index);
			return;
		}
		LaunchEntry launchEntry = Neighbour(1) ?? Neighbour(-1);
		bool enabled = entry.Enabled;
		ToggleEntry(entry.Key);
		if (entry.Enabled != enabled && launchEntry != null && _plan.Entries.Contains(launchEntry))
		{
			_selected = launchEntry;
			UpdateDetails();
		}
		LaunchEntry? Neighbour(int step)
		{
			if (index + step < 0 || index + step >= TreeBindings.Count || TreeBindings[index + step].Entry.Enabled != entry.Enabled)
			{
				return null;
			}
			return TreeBindings[index + step].Entry;
		}
	}

	private static void ToggleKeepingCursor(LaunchEntry entry, int index)
	{
		bool enabled = entry.Enabled;
		ToggleEntry(entry.Key);
		if (entry.Enabled != enabled && index >= 0)
		{
			_cursorRowAfterRebuild = index;
		}
	}

	private static void ToggleFromKeyboard()
	{
		if (_pumpActive || _plan == null || _modTree == null)
		{
			return;
		}
		TreeItem selected = ((Tree)_modTree).GetSelected();
		LaunchEntry entry = EntryFor(selected);
		if (entry == null || InFoldedFolder(selected))
		{
			return;
		}
		if (!entry.CanToggle)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("plan.toggle.managerCannotDisable")));
			return;
		}
		ToggleKeepingCursor(entry, TreeBindings.FindIndex((TreeBinding binding) => binding.Entry == entry));
	}

	private static void KeepCursorRow()
	{
		if (_cursorRowAfterRebuild < 0)
		{
			return;
		}
		int num = Math.Min(_cursorRowAfterRebuild, TreeBindings.Count - 1);
		_cursorRowAfterRebuild = -1;
		if (num >= 0)
		{
			TreeBinding treeBinding = TreeBindings[num];
			if (!InFoldedFolder(treeBinding.Item))
			{
				_selected = treeBinding.Entry;
				treeBinding.Item.Select(2);
				UpdateDetails();
				UpdateListChrome();
				MarkFoldedSelection();
				UpdateControllerPrompts();
			}
		}
	}

	private static void OnTreeSideways(int direction)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		if (!_groupListView || _pumpActive || FolderFoldingSuspended() || _selected == null)
		{
			return;
		}
		TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding binding) => binding.Entry == _selected);
		if ((object)treeBinding == null)
		{
			return;
		}
		Variant metadata;
		if (direction < 0)
		{
			TreeItem parent = treeBinding.Item.GetParent();
			while (parent != null && parent.Collapsed)
			{
				parent = parent.GetParent();
			}
			if (parent != null)
			{
				metadata = parent.GetMetadata(0);
				string text = FolderGroup(((Variant)(ref metadata)).AsString());
				if (text != null)
				{
					SetFolderCollapsed(parent, text, collapsed: true);
				}
			}
			return;
		}
		List<TreeItem> list = new List<TreeItem>();
		for (TreeItem parent2 = treeBinding.Item.GetParent(); parent2 != null; parent2 = parent2.GetParent())
		{
			if (parent2.Collapsed)
			{
				metadata = parent2.GetMetadata(0);
				if (FolderGroup(((Variant)(ref metadata)).AsString()) != null)
				{
					list.Add(parent2);
				}
			}
		}
		for (int num = list.Count - 1; num >= 0; num--)
		{
			TreeItem header = list[num];
			metadata = list[num].GetMetadata(0);
			SetFolderCollapsed(header, FolderGroup(((Variant)(ref metadata)).AsString()), collapsed: false);
		}
		if (list.Count > 0)
		{
			ModReorderTree? modTree = _modTree;
			if (modTree != null)
			{
				((Tree)modTree).ScrollToItem(treeBinding.Item, false);
			}
		}
	}

	private static bool ControllerShortcut(JoyButton button)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Invalid comparison between Unknown and I8
		if (_pumpActive)
		{
			return true;
		}
		_003F val = button - 2;
		if ((long)val <= 8L)
		{
			switch ((uint)val)
			{
			case 0u:
			{
				ModReorderTree? modTree2 = _modTree;
				if (modTree2 != null && ((Control)modTree2).HasFocus())
				{
					if (_controllerMoveKey != null)
					{
						EndControllerMove();
					}
					else
					{
						BeginControllerMove();
					}
					return true;
				}
				break;
			}
			case 1u:
			{
				if (_controllerMoveKey != null)
				{
					return true;
				}
				ModReorderTree? modTree = _modTree;
				if (modTree != null && ((Control)modTree).HasFocus())
				{
					FocusControllerDetails();
				}
				else
				{
					FocusModList();
				}
				return true;
			}
			case 7u:
			case 8u:
				EndControllerMove();
				CycleFilter(((long)button != 9) ? 1 : (-1));
				return true;
			case 4u:
			{
				Button fab = _start;
				if (fab != null && !((BaseButton)fab).Disabled && ((CanvasItem)_start).IsVisibleInTree())
				{
					EndControllerMove();
					StartLoading();
				}
				return true;
			}
			case 2u:
			{
				Button fab = _fab;
				if (fab != null && !((BaseButton)fab).Disabled && ((CanvasItem)_fab).IsVisibleInTree())
				{
					EndControllerMove();
					OpenFabMenu();
				}
				return true;
			}
			}
		}
		return false;
	}

	private static void CycleFilter(int delta)
	{
		List<Button> list = FilterChips.Where((Button chip) => GodotObject.IsInstanceValid((GodotObject)(object)chip) && ((CanvasItem)chip).IsVisibleInTree() && !((BaseButton)chip).Disabled).ToList();
		if (list.Count == 0)
		{
			return;
		}
		HFlowContainer? groupRow = _groupRow;
		List<string> list2 = ((groupRow != null && ((CanvasItem)groupRow).IsVisibleInTree()) ? KnownGroupNames() : new List<string>());
		if (list2.Count > 0)
		{
			LaunchPlan? plan = _plan;
			if (plan != null && plan.Entries.Any((LaunchEntry entry) => string.IsNullOrWhiteSpace(entry.Group)))
			{
				list2.Insert(0, "\u0001ungrouped");
			}
		}
		int num = ((_groupFilter == null) ? (-1) : list2.FindIndex((string group) => string.Equals(group, _groupFilter, StringComparison.OrdinalIgnoreCase)));
		int num2 = ((num >= 0) ? (list.Count + num) : Math.Max(0, list.FindIndex((Button chip) => ((BaseButton)chip).ButtonPressed)));
		int num3 = list.Count + list2.Count;
		int num4 = (num2 + delta + num3) % num3;
		string text = ((num4 < list.Count) ? null : list2[num4 - list.Count]);
		if (!string.Equals(_groupFilter, text, StringComparison.OrdinalIgnoreCase))
		{
			_groupFilter = text;
			_groupDeleteArmed = false;
			Defer(RebuildGroupRow);
		}
		((BaseButton)((num4 < list.Count) ? ((object)list[num4]) : ((object)FilterChips[0]))).ButtonPressed = true;
		_selectFirstRowAfterRebuild = true;
		QueueRebuild(scrollToSelection: true);
	}

	private static void SelectFirstRowIfFilteredOut()
	{
		if (_selectFirstRowAfterRebuild && _modTree != null)
		{
			_selectFirstRowAfterRebuild = false;
			if (EntryFor(((Tree)_modTree).GetSelected()) == null && TreeBindings.Count > 0)
			{
				SelectTreeRow(TreeBindings[0]);
			}
			LauncherController? controller = _controller;
			if (controller != null && controller.ControllerMode && _controller.ActiveRoot == _root)
			{
				FocusModList();
			}
			UpdateControllerPrompts();
		}
	}

	private static void BeginControllerMove()
	{
		ModReorderTree? modTree = _modTree;
		LaunchEntry launchEntry = EntryFor((modTree != null) ? ((Tree)modTree).GetSelected() : null);
		if (launchEntry != null && !_pumpActive)
		{
			if (!launchEntry.CanMove)
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("plan.move.managerMustBeFirst")));
				return;
			}
			_selected = launchEntry;
			_controllerMoveKey = launchEntry.Key;
			SetStatus(MlmLocManager.Get("controller.move.started", ("name", launchEntry.DisplayName)), "MutedLabel", "↕");
			QueueRebuild(scrollToSelection: true);
			UpdateControllerPrompts();
		}
	}

	private static void EndControllerMove()
	{
		if (_controllerMoveKey != null)
		{
			_controllerMoveKey = null;
			QueueRebuild();
			UpdateControllerPrompts();
		}
	}

	private static bool ControllerDirection(Control focused, Vector2I direction)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (focused == _modTree && direction == Vector2I.Down && _controllerMoveKey == null && TreeBindings.Count > 0)
		{
			LaunchEntry? launchEntry = EntryFor(((Tree)_modTree).GetSelected());
			List<TreeBinding> treeBindings = TreeBindings;
			if (launchEntry == treeBindings[treeBindings.Count - 1].Entry)
			{
				return true;
			}
		}
		if (_controllerMoveKey == null)
		{
			return false;
		}
		if (focused != _modTree || !string.Equals(_selected?.Key, _controllerMoveKey, StringComparison.Ordinal))
		{
			EndControllerMove();
			return false;
		}
		if (direction.Y != 0)
		{
			MoveSelected(direction.Y);
		}
		return true;
	}

	private static Control? ControllerRedirect(Control focused, Vector2I direction)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (focused != _modTree || !(direction == Vector2I.Right))
		{
			return null;
		}
		return FirstDetailsControl();
	}

	private static Control? FirstDetailsControl()
	{
		if (_detailScroll != null)
		{
			Control val = LauncherController.FirstOnScreen((Control)(object)_detailScroll);
			if (val != null)
			{
				return val;
			}
		}
		Button detailToggle = _detailToggle;
		if (detailToggle != null && !((BaseButton)detailToggle).Disabled && ((CanvasItem)detailToggle).IsVisibleInTree())
		{
			return (Control?)(object)detailToggle;
		}
		Button detailOpenFolder = _detailOpenFolder;
		if (detailOpenFolder == null || ((BaseButton)detailOpenFolder).Disabled || !((CanvasItem)detailOpenFolder).IsVisibleInTree())
		{
			return null;
		}
		return (Control?)(object)detailOpenFolder;
	}

	private static void FocusControllerDetails()
	{
		Control val = FirstDetailsControl();
		if (val != null)
		{
			val.GrabFocus();
		}
	}

	private static bool ControllerPage(Control focused, int delta)
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (focused != _modTree || _modTree == null)
		{
			return false;
		}
		if (_controllerMoveKey != null || TreeBindings.Count == 0)
		{
			return true;
		}
		LaunchEntry current = EntryFor(((Tree)_modTree).GetSelected());
		int num = ((current != null) ? Math.Max(0, TreeBindings.FindIndex((TreeBinding binding) => binding.Entry == current)) : 0);
		int num2 = Math.Max(3, (int)(((Control)_modTree).Size.Y / 39f) - 2);
		SelectTreeRow(TreeBindings[Math.Clamp(num + delta * num2, 0, TreeBindings.Count - 1)]);
		return true;
	}

	private static void ControllerBack()
	{
		if (_pumpActive)
		{
			return;
		}
		if (_controllerMoveKey != null)
		{
			EndControllerMove();
			return;
		}
		if (_descriptionEditingKey != null)
		{
			CancelDescriptionEdit();
		}
		else
		{
			HBoxContainer? groupNameRow = _groupNameRow;
			if (groupNameRow != null && ((CanvasItem)groupNameRow).Visible)
			{
				ShowGroupNameRow(visible: false);
			}
			else
			{
				HBoxContainer? profileNameRow = _profileNameRow;
				if (profileNameRow != null && ((CanvasItem)profileNameRow).Visible)
				{
					ShowProfileNameRow(visible: false);
				}
			}
		}
		FocusModList();
	}

	private static void ShowControllerKeyboard(Control field)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			Rect2 val = ((Node)field).GetViewport().GetFinalTransform() * field.GetGlobalRect();
			SteamUtils.ShowFloatingGamepadTextInput((EFloatingGamepadTextInputMode)(field is TextEdit), (int)((Rect2)(ref val)).Position.X, (int)((Rect2)(ref val)).Position.Y, (int)((Rect2)(ref val)).Size.X, (int)((Rect2)(ref val)).Size.Y);
		}
		catch (InvalidOperationException)
		{
		}
	}

	private static void BuildControllerPrompts(CanvasLayer overlay)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		_promptLayer = new CanvasLayer
		{
			Name = StringName.op_Implicit("ControllerPrompts"),
			Layer = 1300
		};
		Control val = new Control
		{
			MouseFilter = (MouseFilterEnum)2
		};
		Control? root = _root;
		val.Theme = ((root != null) ? root.Theme : null);
		_promptHost = UiKit.FullRect<Control>(val);
		_promptBar = new HBoxContainer
		{
			Visible = false,
			Alignment = (AlignmentMode)2,
			MouseFilter = (MouseFilterEnum)2,
			AnchorLeft = 0f,
			AnchorRight = 1f,
			AnchorTop = 1f,
			AnchorBottom = 1f
		};
		((Node)_promptHost).AddChild((Node)(object)_promptBar, false, (InternalMode)0);
		((Node)_promptLayer).AddChild((Node)(object)_promptHost, false, (InternalMode)0);
		((Node)overlay).AddChild((Node)(object)_promptLayer, false, (InternalMode)0);
		BindPalette((GodotObject)(object)_promptHost, SyncControllerPromptTheme);
		BindLocalization(_promptHost, SyncControllerPromptTheme);
	}

	private static Control BuildControllerPromptSpacer()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		_promptSpacer = new Control
		{
			Visible = false,
			MouseFilter = (MouseFilterEnum)2
		};
		return _promptSpacer;
	}

	private static void SyncControllerPromptTheme()
	{
		if (_promptHost != null && GodotObject.IsInstanceValid((GodotObject)(object)_promptHost))
		{
			Control? promptHost = _promptHost;
			Control? root = _root;
			promptHost.Theme = ((root != null) ? root.Theme : null);
			_promptSignature = string.Empty;
			UpdateControllerPrompts();
		}
	}

	private static void UpdateControllerPrompts()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Expected O, but got Unknown
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Expected O, but got Unknown
		if (_promptBar == null || !GodotObject.IsInstanceValid((GodotObject)(object)_promptBar))
		{
			return;
		}
		LauncherController? controller = _controller;
		List<Prompt> list = ((controller != null && controller.ControllerMode) ? ControllerPromptItems() : new List<Prompt>());
		bool flag = list.Count > 0;
		int num = UiKit.ScaleFontSize(28);
		((Control)_promptBar).OffsetLeft = 26f;
		((Control)_promptBar).OffsetRight = -26f;
		((Control)_promptBar).OffsetTop = -20 - num;
		((Control)_promptBar).OffsetBottom = -20f;
		((CanvasItem)_promptBar).Visible = flag;
		if (_promptSpacer != null && GodotObject.IsInstanceValid((GodotObject)(object)_promptSpacer))
		{
			_promptSpacer.CustomMinimumSize = new Vector2(0f, (float)num);
			((CanvasItem)_promptSpacer).Visible = flag;
		}
		if (!flag)
		{
			_promptSignature = string.Empty;
			return;
		}
		RefreshPromptGlyphs();
		string text = string.Join("|", list.Select((Prompt prompt) => string.Join("+", prompt.Keys.Select((PromptKey key) => PromptGlyph(key).ToString())) + ":" + prompt.Text));
		if (text == _promptSignature)
		{
			return;
		}
		_promptSignature = text;
		foreach (Node child in ((Node)_promptBar).GetChildren(false))
		{
			((Node)_promptBar).RemoveChild(child);
			child.QueueFree();
		}
		((Control)_promptBar).AddThemeConstantOverride(StringName.op_Implicit("separation"), 20);
		foreach (Prompt item in list)
		{
			HBoxContainer val = new HBoxContainer
			{
				MouseFilter = (MouseFilterEnum)2
			};
			((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 3);
			PromptKey[] keys = item.Keys;
			foreach (PromptKey key2 in keys)
			{
				((Node)val).AddChild((Node)(object)PromptGlyphNode(PromptGlyph(key2)), false, (InternalMode)0);
			}
			((Node)val).AddChild((Node)new Control
			{
				CustomMinimumSize = new Vector2(3f, 0f),
				MouseFilter = (MouseFilterEnum)2
			}, false, (InternalMode)0);
			Label val2 = UiKit.Label(item.Text);
			val2.VerticalAlignment = (VerticalAlignment)1;
			((Control)val2).SizeFlagsVertical = (SizeFlags)4;
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)_promptBar).AddChild((Node)(object)val, false, (InternalMode)0);
		}
	}

	private static List<Prompt> ControllerPromptItems()
	{
		List<Prompt> list = new List<Prompt>();
		Control val = ((_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root)) ? ((Node)_root).GetViewport().GuiGetFocusOwner() : null);
		Control val2 = _controller?.ActiveRoot;
		string text = MlmLocManager.Get("controller.prompt.select");
		if (TourOpen)
		{
			list.Add(new Prompt(new PromptKey[1], MlmLocManager.Get((_tourIndex >= _tourSteps.Count - 1) ? "tour.done" : "tour.next")));
			list.Add(new Prompt(new PromptKey[2]
			{
				PromptKey.LeftShoulder,
				PromptKey.RightShoulder
			}, MlmLocManager.Get("tour.prompt.steps")));
			list.Add(new Prompt(new PromptKey[1] { PromptKey.B }, MlmLocManager.Get("tour.skip")));
			return list;
		}
		if (UpdateAnnouncement.IsOpen)
		{
			list.Add(new Prompt(new PromptKey[1], MlmLocManager.Get("updateAnnouncement.confirm")));
			return list;
		}
		if (_pumpActive)
		{
			PanelContainer? loadErrorPanel = _loadErrorPanel;
			if (loadErrorPanel != null && ((CanvasItem)loadErrorPanel).Visible && val is BaseButton)
			{
				list.Add(new Prompt(new PromptKey[1], text));
			}
			return list;
		}
		if (val2 != null && val2 != _root)
		{
			list.Add(new Prompt(new PromptKey[1], text));
			list.Add(new Prompt(new PromptKey[1] { PromptKey.B }, MlmLocManager.Get((val2 == _settingsPage) ? "controller.prompt.back" : "common.action.close")));
			return list;
		}
		if (val == _modTree && _modTree != null)
		{
			if (_controllerMoveKey != null)
			{
				list.Add(new Prompt(new PromptKey[1] { PromptKey.UpDown }, MlmLocManager.Get("controller.prompt.move")));
				list.Add(new Prompt(new PromptKey[1], MlmLocManager.Get("controller.prompt.done")));
				return list;
			}
			LaunchEntry launchEntry = EntryFor(((Tree)_modTree).GetSelected());
			if (launchEntry != null && launchEntry.CanToggle)
			{
				list.Add(new Prompt(new PromptKey[1], MlmLocManager.Get(launchEntry.Enabled ? "details.action.disableForLaunch" : "details.action.enableForLaunch")));
			}
			if (launchEntry != null && launchEntry.CanMove)
			{
				list.Add(new Prompt(new PromptKey[1] { PromptKey.X }, MlmLocManager.Get("controller.prompt.reorder")));
			}
			list.Add(new Prompt(new PromptKey[1] { PromptKey.Y }, MlmLocManager.Get("controller.prompt.details")));
			list.Add(new Prompt(new PromptKey[2]
			{
				PromptKey.LeftShoulder,
				PromptKey.RightShoulder
			}, MlmLocManager.Get("controller.prompt.filter")));
			list.Add(new Prompt(new PromptKey[2]
			{
				PromptKey.LeftTrigger,
				PromptKey.RightTrigger
			}, MlmLocManager.Get("controller.prompt.page")));
		}
		else if (val != null)
		{
			List<Prompt> list2 = list;
			PromptKey[] keys = new PromptKey[1];
			bool flag = ((val is LineEdit || val is TextEdit) ? true : false);
			list2.Add(new Prompt(keys, flag ? MlmLocManager.Get("controller.prompt.type") : text));
			list.Add(new Prompt(new PromptKey[1] { PromptKey.B }, MlmLocManager.Get("controller.prompt.backToList")));
			if (val == _clearFilters)
			{
				list.Add(new Prompt(new PromptKey[2]
				{
					PromptKey.LeftShoulder,
					PromptKey.RightShoulder
				}, MlmLocManager.Get("controller.prompt.filter")));
			}
			if (_detailScroll != null && ((Node)_detailScroll).IsAncestorOf((Node)(object)val))
			{
				list.Add(new Prompt(new PromptKey[2]
				{
					PromptKey.LeftTrigger,
					PromptKey.RightTrigger
				}, MlmLocManager.Get("controller.prompt.scroll")));
			}
		}
		Button fab = _fab;
		if (fab != null && !((BaseButton)fab).Disabled && ((CanvasItem)_fab).IsVisibleInTree())
		{
			list.Add(new Prompt(new PromptKey[1] { PromptKey.Back }, MlmLocManager.Get("controller.prompt.tools")));
		}
		fab = _start;
		if (fab != null && !((BaseButton)fab).Disabled && ((CanvasItem)_start).IsVisibleInTree())
		{
			list.Add(new Prompt(new PromptKey[1] { PromptKey.Start }, _start.Text.TrimEnd(' ', '\u00a0', '→')));
		}
		return list;
	}

	private static PadGlyph PromptGlyph(PromptKey key)
	{
		if (!PromptGlyphs.TryGetValue(key, out var value))
		{
			return PositionalGlyph(key, PadFamily.Xbox);
		}
		return value;
	}

	private static PadGlyph PositionalGlyph(PromptKey key, PadFamily family)
	{
		return key switch
		{
			PromptKey.A => LauncherControllerGlyphs.Positional((JoyButton)0, family), 
			PromptKey.B => LauncherControllerGlyphs.Positional((JoyButton)1, family), 
			PromptKey.X => LauncherControllerGlyphs.Positional((JoyButton)2, family), 
			PromptKey.Y => LauncherControllerGlyphs.Positional((JoyButton)3, family), 
			PromptKey.LeftShoulder => LauncherControllerGlyphs.Positional((JoyButton)9, family), 
			PromptKey.RightShoulder => LauncherControllerGlyphs.Positional((JoyButton)10, family), 
			PromptKey.LeftTrigger => LauncherControllerGlyphs.Trigger(-1, family), 
			PromptKey.RightTrigger => LauncherControllerGlyphs.Trigger(1, family), 
			PromptKey.Start => LauncherControllerGlyphs.Positional((JoyButton)6, family), 
			PromptKey.Back => LauncherControllerGlyphs.Positional((JoyButton)4, family), 
			_ => new PadGlyph("↑↓"), 
		};
	}

	private static void RefreshPromptGlyphs()
	{
		ulong ticksMsec = Time.GetTicksMsec();
		if (PromptGlyphs.Count <= 0 || ticksMsec >= _promptGlyphsAt)
		{
			_promptGlyphsAt = ticksMsec + 2000;
			PromptGlyphs.Clear();
			LauncherController controller = _controller;
			PadFamily family;
			if (controller != null && controller.LastSource == LauncherController.Source.Joypad && Input.GetConnectedJoypads().Contains(controller.LastDevice))
			{
				family = LauncherControllerGlyphs.FamilyFromJoyName(Input.GetJoyName(controller.LastDevice));
			}
			else if (!TryReadSteamPromptGlyphs(out family))
			{
				Array<int> connectedJoypads = Input.GetConnectedJoypads();
				family = ((connectedJoypads.Count > 0) ? LauncherControllerGlyphs.FamilyFromJoyName(Input.GetJoyName(connectedJoypads[0])) : PadFamily.Xbox);
			}
			PromptKey[] values = Enum.GetValues<PromptKey>();
			foreach (PromptKey key in values)
			{
				PromptGlyphs.TryAdd(key, PositionalGlyph(key, family));
			}
		}
	}

	private static bool TryReadSteamPromptGlyphs(out PadFamily family)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		family = PadFamily.Xbox;
		try
		{
			if (SteamInput.GetConnectedControllers(SteamControllers) <= 0)
			{
				return false;
			}
			InputHandle_t val = SteamControllers[0];
			ESteamInputType inputTypeForHandle = SteamInput.GetInputTypeForHandle(val);
			family = LauncherControllerGlyphs.FamilyFromSteamType(((object)(ESteamInputType)(ref inputTypeForHandle)/*cast due to .constrained prefix*/).ToString());
			InputActionSetHandle_t actionSetHandle = SteamInput.GetActionSetHandle("Controls");
			EInputActionOrigin[] array = (EInputActionOrigin[])(object)new EInputActionOrigin[8];
			(PromptKey, string)[] steamPromptActions = SteamPromptActions;
			for (int i = 0; i < steamPromptActions.Length; i++)
			{
				(PromptKey, string) tuple = steamPromptActions[i];
				PromptKey item = tuple.Item1;
				InputDigitalActionHandle_t digitalActionHandle = SteamInput.GetDigitalActionHandle(tuple.Item2);
				if (digitalActionHandle.m_InputDigitalActionHandle != 0L && SteamInput.GetDigitalActionOrigins(val, actionSetHandle, digitalActionHandle, array) > 0)
				{
					EInputActionOrigin val2 = SteamInput.TranslateActionOrigin(inputTypeForHandle, array[0]);
					if (!Enum.IsDefined<EInputActionOrigin>(val2))
					{
						val2 = SteamInput.TranslateActionOrigin((ESteamInputType)0, val2);
					}
					PadGlyph? padGlyph = LauncherControllerGlyphs.FromSteamOrigin(((object)(EInputActionOrigin)(ref val2)/*cast due to .constrained prefix*/).ToString());
					if (padGlyph.HasValue)
					{
						PadGlyph valueOrDefault = padGlyph.GetValueOrDefault();
						PromptGlyphs[item] = valueOrDefault;
					}
				}
			}
			return true;
		}
		catch (InvalidOperationException)
		{
			return false;
		}
	}

	private static Control PromptGlyphNode(PadGlyph glyph)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Expected O, but got Unknown
		int num = UiKit.ScaleFontSize(22);
		Color surfaceRaised = UiKit.Palette.SurfaceRaised;
		surfaceRaised.A = 1f;
		Color val = surfaceRaised;
		PanelContainer val2 = new PanelContainer
		{
			MouseFilter = (MouseFilterEnum)2,
			SizeFlagsVertical = (SizeFlags)4,
			CustomMinimumSize = new Vector2((float)num, (float)num)
		};
		StyleBoxFlat val3 = UiKit.Box(UiKit.Text, UiKit.Clear, glyph.Round ? (num / 2) : Math.Max(4, num / 4), 0, (!glyph.Round) ? Math.Max(5, num / 4) : 0, 0);
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val3);
		if (glyph.Symbol != 0)
		{
			((Node)val2).AddChild((Node)new TextureRect
			{
				Texture = (Texture2D)(object)UiGlyphs.PadSymbolIcon(num, val, glyph.Symbol),
				StretchMode = (StretchModeEnum)3,
				MouseFilter = (MouseFilterEnum)2
			}, false, (InternalMode)0);
			return (Control)(object)val2;
		}
		Label val4 = UiKit.Label(glyph.Text);
		val4.HorizontalAlignment = (HorizontalAlignment)1;
		val4.VerticalAlignment = (VerticalAlignment)1;
		((Control)val4).AddThemeColorOverride(StringName.op_Implicit("font_color"), val);
		((Control)val4).AddThemeColorOverride(StringName.op_Implicit("font_shadow_color"), UiKit.Clear);
		Label val5 = val4;
		StringName val6 = StringName.op_Implicit("font_size");
		int length = glyph.Text.Length;
		int size = ((length <= 1) ? 13 : ((length != 2) ? 9 : 11));
		((Control)val5).AddThemeFontSizeOverride(val6, UiKit.ScaleFontSize(size));
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		return (Control)(object)val2;
	}

	internal static void Show()
	{
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_036c: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Expected O, but got Unknown
		try
		{
			if (_layer != null)
			{
				return;
			}
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (val == null)
			{
				throw new InvalidOperationException("Godot SceneTree is unavailable");
			}
			_sceneTree = val;
			ConfigCloudSync.SyncAtStartup();
			LauncherDisplay.ApplySavedSettings();
			LauncherDisplay.SharpenFonts(val);
			LauncherDisplay.FitLauncher(val.Root);
			LauncherDisplay.MoveSteamNotices();
			LauncherDisplay.RedrawOnlyChanges();
			BeginBackground();
			IReadOnlyList<Mod> mods = DeferredModLoader.Snapshot();
			WorkshopBranches.Prepare(mods, OnWorkshopBranchChanged);
			LaunchPlan.KeepDisabledInPlace = MlmLocManager.KeepDisabledInPlace;
			CanonicalLaunchSession canonicalLaunchSession = null;
			string text = null;
			try
			{
				canonicalLaunchSession = CanonicalLaunchStore.LoadAndReconcile(mods);
				_canonical = canonicalLaunchSession.Document;
				_canonicalDormant = canonicalLaunchSession.Dormant.Select((CanonicalLaunchEntry entry) => entry.Clone()).ToList();
				_plan = new LaunchPlan(canonicalLaunchSession.Seeds);
			}
			catch (Exception ex)
			{
				canonicalLaunchSession = null;
				text = ex.Message;
				_canonical = new CanonicalLaunchDocument();
				_canonicalDormant = new List<CanonicalLaunchEntry>();
				_plan = new LaunchPlan(mods);
				GD.PushWarning($"{"[ModLaunchManager]"} the remembered launch plan could not be used; falling back to the live mod list: {ex}");
			}
			_pendingCanonical = null;
			_pendingCanonicalAction = PendingCanonicalAction.None;
			_pendingSkipMod = null;
			LaunchConfirmedConflicts.Clear();
			_organization = ModOrganizationStore.Load();
			_options = ModOptionStore.Load();
			ApplyOrganization(_plan.Entries);
			int moved;
			PlanResult value = _plan.ApplyDeclaredPriorities(rebaseline: true, out moved);
			_priorityReport = ((value.Success && moved == 0) ? ((PlanResult?)null) : new PlanResult?(value));
			_selected = _plan.Entries.FirstOrDefault((LaunchEntry entry) => !entry.IsManager) ?? _plan.Entries.FirstOrDefault();
			_filter = ModFilter.All;
			_groupFilter = null;
			_groupRenameSource = null;
			_groupDeleteArmed = false;
			_organizationEditingKey = null;
			_descriptionEditingKey = null;
			_optionsEditingKey = null;
			_optionsShowAdvanced = false;
			_dragPreviewActive = false;
			_selectedSaveProfileId = MlmLocManager.SaveProfileId;
			try
			{
				_profiles = ProfileStore.Load();
				_activeProfile = (_profiles.Profiles.Any((LaunchProfile profile) => string.Equals(profile.Name, _profiles.Active, StringComparison.Ordinal)) ? _profiles.Active : null);
				_profileBaseline = null;
			}
			catch (Exception ex2)
			{
				_profiles = new ProfileDocument();
				_activeProfile = null;
				GD.PushWarning("[ModLaunchManager] profiles unavailable: " + ex2.Message);
			}
			TreeBindings.Clear();
			UndoSteps.Clear();
			FolderToggleMemory.Clear();
			_cursorRowAfterRebuild = -1;
			FilterChips.Clear();
			OptionEditors.Clear();
			DetailGroupValues.Clear();
			GroupMenuValues.Clear();
			GroupMenuTargets.Clear();
			LocalizationBindings.Clear();
			PaletteBindings.Clear();
			LogLines.Clear();
			UiKit.UsePalette(UiPalette.For(MlmLocManager.EffectiveTheme));
			UiKit.UseFontScale(MlmLocManager.FontScale);
			_showAffectsColumn = MlmLocManager.ShowAffectsGameplayColumn;
			LoadGroupViewPreferences();
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("ModLaunchManagerOverlay"),
				Layer = 1000
			};
			Control val2 = BuildRoot();
			((Node)_layer).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherDisplay.FitLauncher(val.Root, val2);
			LocalModWatcher.Start(val, OnLocalModsChanged, OnLocalModWatcherError);
			LauncherBackground.Watch(val, OnBackgroundFolderChanged);
			ModManager.OnModDetected += OnVanillaModDetected;
			StartWorkshopUpdateCheck(val2);
			StartFriendModSync(val2);
			RebuildProfileRow();
			RebuildGroupRow();
			RebuildTree();
			UpdateDetails();
			UpdatePlanSummary();
			if (canonicalLaunchSession != null)
			{
				ReportCanonicalReconciliation(canonicalLaunchSession);
			}
			else
			{
				SetStatus(MlmLocManager.Get("launcher.canonical.unusable", ("error", text ?? string.Empty)), "WarningLabel", "!");
			}
			PlanResult? priorityReport = _priorityReport;
			if (priorityReport.HasValue)
			{
				PlanResult valueOrDefault = priorityReport.GetValueOrDefault();
				ShowFeedback(valueOrDefault);
				GD.Print("[ModLaunchManager] declared load priorities: " + valueOrDefault.Message);
			}
			UpdateWorkshopChrome();
			Button? start = _start;
			if (start != null)
			{
				((Control)start).GrabFocus();
			}
			StartControllerNavigation();
			UpdateAnnouncement.NewFeatureSteps = (int revision) => NewTourSteps(revision).Count;
			UpdateAnnouncement.ShowNewFeatures = StartWhatsNewTour;
			UpdateAnnouncement.ShowIfNeeded();
			GD.Print("[ModLaunchManager] launch manager is visible.");
			ManagerReturn.ManagerShown(val2);
		}
		catch (Exception value2)
		{
			LocalModWatcher.Stop();
			LauncherBackground.StopWatching();
			LauncherBackground.CancelLoad();
			GD.PushWarning($"{"[ModLaunchManager]"} boot UI failed: {value2}");
			ModLaunchManagerMod.ReportUnavailable(MlmLocManager.Get("notice.reason.bootUi"));
			ResumeSynchronouslyAndContinue();
		}
	}

	private static Control BuildRoot()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Expected O, but got Unknown
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		Control val = (_root = UiKit.FullRect<Control>(new Control
		{
			Name = StringName.op_Implicit("Root"),
			MouseFilter = (MouseFilterEnum)0,
			Theme = UiKit.CreateTheme()
		}));
		if (_backgroundNode != null)
		{
			((Node)val).AddChild((Node)(object)UiKit.FullRect<TextureRect>(_backgroundNode), false, (InternalMode)0);
		}
		ColorRect canvas = new ColorRect
		{
			Color = BackdropVeil(),
			MouseFilter = (MouseFilterEnum)0
		};
		_backdropVeil = canvas;
		BindPalette((GodotObject)(object)canvas, delegate
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			FinishBackgroundFade();
			canvas.Color = BackdropVeil();
		});
		((Node)val).AddChild((Node)(object)UiKit.FullRect<ColorRect>(canvas), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)LiquidGlass.Backdrop(), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)LiquidGlass.ScrollEdge(), false, (InternalMode)0);
		TextureRect wash = UiKit.GradientSheet(WashRamp(), new float[3] { 0f, 0.3f, 0.75f }, horizontal: false);
		BindPalette((GodotObject)(object)wash, delegate
		{
			UiKit.RestyleGradient(wash, WashRamp());
			((CanvasItem)wash).Visible = !UiKit.Palette.IsLiquidGlass;
		});
		((Node)val).AddChild((Node)(object)UiKit.FullRect<TextureRect>(wash), false, (InternalMode)0);
		TextureRect topRule = UiKit.GradientSheet(TopRuleRamp(), new float[3] { 0f, 0.4f, 1f }, horizontal: true);
		BindPalette((GodotObject)(object)topRule, delegate
		{
			UiKit.RestyleGradient(topRule, TopRuleRamp());
			((CanvasItem)topRule).Visible = !UiKit.Palette.IsLiquidGlass;
		});
		((Control)topRule).AnchorRight = 1f;
		((Control)topRule).OffsetBottom = 2f;
		((Node)val).AddChild((Node)(object)topRule, false, (InternalMode)0);
		PanelContainer val2 = UiKit.FullRect<PanelContainer>(new PanelContainer());
		((Control)val2).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.Box(UiKit.Clear, UiKit.Clear, 0, 0, 26, 20));
		VBoxContainer val3 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		((Node)val3).AddChild((Node)(object)BuildHeader(), false, (InternalMode)0);
		HSeparator headerDivider = UiKit.Divider();
		BindPalette((GodotObject)(object)headerDivider, delegate
		{
			((CanvasItem)headerDivider).Visible = !UiKit.Palette.IsLiquidGlass;
		});
		((Node)val3).AddChild((Node)(object)headerDivider, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildChangeNotesBanner(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildProfileBar(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildWorkspace(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildLoadingPanel(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildActionBar(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildControllerPromptSpacer(), false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)BuildSettingsPage(), false, (InternalMode)0);
		return val;
	}

	private static void ReportCanonicalReconciliation(CanonicalLaunchSession session)
	{
		CanonicalReconciliation reconciliation = session.Reconciliation;
		string text = $"workshopExact={reconciliation.WorkshopExact}, pathExact={reconciliation.PathExact}, moved={reconciliation.MovedFallback}, new={reconciliation.NewCount}, dormant={reconciliation.Dormant.Count}, ambiguous={reconciliation.AmbiguousGroups}";
		switch (session.LoadState)
		{
		case CanonicalLoadState.Missing:
			SetStatus(MlmLocManager.Get("launcher.canonical.missing"), "MutedLabel", "·");
			GD.Print("[ModLaunchManager] no canonical launch plan; bootstrapping live state (" + text + ").");
			break;
		case CanonicalLoadState.Invalid:
			SetStatus(MlmLocManager.Get("launcher.canonical.invalid", ("error", session.LoadError)), "WarningLabel", "!");
			GD.PushWarning("[ModLaunchManager] canonical fallback (" + text + ").");
			break;
		default:
			SetStatus(MlmLocManager.Get("launcher.canonical.restored", ("exact", reconciliation.WorkshopExact + reconciliation.PathExact), ("moved", reconciliation.MovedFallback), ("newCount", reconciliation.NewCount)), (reconciliation.AmbiguousGroups > 0) ? "WarningLabel" : "SuccessLabel", (reconciliation.AmbiguousGroups > 0) ? "!" : "✓");
			GD.Print("[ModLaunchManager] restored canonical launch plan (" + text + ").");
			break;
		}
	}

	private static Control BuildHeader()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Expected O, but got Unknown
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Expected O, but got Unknown
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Expected O, but got Unknown
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Expected O, but got Unknown
		//IL_0323: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Expected O, but got Unknown
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_054a: Expected O, but got Unknown
		HBoxContainer header = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)header).AddThemeConstantOverride(StringName.op_Implicit("separation"), 13);
		BindPalette((GodotObject)(object)header, delegate
		{
			((Control)header).AddThemeConstantOverride(StringName.op_Implicit("separation"), UiKit.Palette.IsLiquidGlass ? 10 : 13);
		});
		Panel mark = new Panel
		{
			CustomMinimumSize = new Vector2(3f, 30f),
			SizeFlagsVertical = (SizeFlags)4,
			MouseFilter = (MouseFilterEnum)2
		};
		BindPalette((GodotObject)(object)mark, delegate
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			((Control)mark).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.Box(UiKit.Palette.IsFluent ? UiKit.Accent : UiKit.BorderStrong, UiKit.Clear, 999, 0, 0, 0));
		});
		BindPalette((GodotObject)(object)mark, delegate
		{
			((CanvasItem)mark).Visible = !UiKit.Palette.IsLiquidGlass;
		});
		Label val = LocalizedKeyLabel("launcher.productName", "TitleLabel");
		((Control)val).SizeFlagsVertical = (SizeFlags)4;
		HBoxContainer val2 = new HBoxContainer
		{
			SizeFlagsVertical = (SizeFlags)4
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		Label val3 = LocalizedKeyLabel("settings.language.label", "CaptionLabel");
		((Control)val3).SizeFlagsVertical = (SizeFlags)4;
		_languageSelector = UiKit.Dropdown("GlassMenuButton", 150f, 32f, fitToLongestItem: true);
		((Control)_languageSelector).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_languageSelector, "settings.language.tooltip");
		OptionButton? languageSelector = _languageSelector;
		object obj = _003C_003EO._003C40_003E__OnLanguageSelected;
		if (obj == null)
		{
			ItemSelectedEventHandler val4 = OnLanguageSelected;
			_003C_003EO._003C40_003E__OnLanguageSelected = val4;
			obj = (object)val4;
		}
		languageSelector.ItemSelected += (ItemSelectedEventHandler)obj;
		PopulateLanguageSelector();
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_languageSelector, false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer
		{
			SizeFlagsVertical = (SizeFlags)4
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		Label val6 = LocalizedKeyLabel("settings.theme.label", "CaptionLabel");
		((Control)val6).SizeFlagsVertical = (SizeFlags)4;
		_themeSelector = UiKit.Dropdown("GlassMenuButton", 110f, 32f);
		((Control)_themeSelector).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_themeSelector, "settings.theme.tooltip");
		OptionButton? themeSelector = _themeSelector;
		object obj2 = _003C_003EO._003C41_003E__OnThemeSelected;
		if (obj2 == null)
		{
			ItemSelectedEventHandler val7 = OnThemeSelected;
			_003C_003EO._003C41_003E__OnThemeSelected = val7;
			obj2 = (object)val7;
		}
		themeSelector.ItemSelected += (ItemSelectedEventHandler)obj2;
		BindPalette((GodotObject)(object)_themeSelector, delegate
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			((Control)_themeSelector).CustomMinimumSize = new Vector2((float)(UiKit.Palette.IsLiquidGlass ? 136 : 110), 32f);
		});
		((Popup)_themeSelector.GetPopup()).PopupHide += delegate
		{
			Defer(FlushPendingBackgroundChange);
		};
		PopulateThemeSelector();
		((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)_themeSelector, false, (InternalMode)0);
		HBoxContainer val8 = new HBoxContainer
		{
			SizeFlagsVertical = (SizeFlags)4
		};
		((Control)val8).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		Label val9 = LocalizedKeyLabel("saveProfile.label", "CaptionLabel");
		((Control)val9).SizeFlagsVertical = (SizeFlags)4;
		_saveProfileSelector = UiKit.Dropdown("GlassMenuButton", 126f, 32f, fitToLongestItem: true);
		((Control)_saveProfileSelector).SizeFlagsVertical = (SizeFlags)4;
		((BaseButton)_saveProfileSelector).Disabled = !SaveProfileSwitcher.Available;
		BindKeyTooltip((Control)(object)_saveProfileSelector, "saveProfile.tooltip");
		OptionButton? saveProfileSelector = _saveProfileSelector;
		object obj3 = _003C_003EO._003C43_003E__OnSaveProfileSelected;
		if (obj3 == null)
		{
			ItemSelectedEventHandler val10 = OnSaveProfileSelected;
			_003C_003EO._003C43_003E__OnSaveProfileSelected = val10;
			obj3 = (object)val10;
		}
		saveProfileSelector.ItemSelected += (ItemSelectedEventHandler)obj3;
		PopulateSaveProfileSelector();
		((Node)val8).AddChild((Node)(object)val9, false, (InternalMode)0);
		((Node)val8).AddChild((Node)(object)_saveProfileSelector, false, (InternalMode)0);
		HBoxContainer val11 = new HBoxContainer
		{
			SizeFlagsVertical = (SizeFlags)4,
			SizeFlagsHorizontal = (SizeFlags)8
		};
		((Control)val11).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_headerBadges = (Control?)(object)val11;
		Label gameVersionChip;
		PanelContainer val12 = UiKit.Badge(string.Empty, BadgeTone.Neutral, out gameVersionChip);
		BindLocalization((Control)(object)gameVersionChip, delegate
		{
			gameVersionChip.Text = MlmLocManager.Get("launcher.gameVersion.badge", ("version", CurrentGameVersion()), ("branch", ModErrorText.CurrentBranchName()));
		});
		BindKeyTooltip((Control)(object)val12, "launcher.gameVersion.tooltip");
		((Node)val11).AddChild((Node)(object)PaletteBadge(val12, BadgeTone.Neutral), false, (InternalMode)0);
		((Node)val11).AddChild((Node)(object)PaletteBadge(UiKit.Badge(string.Empty, BadgeTone.Neutral, out _discoveredChip), BadgeTone.Neutral), false, (InternalMode)0);
		((Node)val11).AddChild((Node)(object)PaletteBadge(UiKit.Badge(string.Empty, BadgeTone.Neutral, out _stateChip), BadgeTone.Neutral), false, (InternalMode)0);
		_workshopBadge = UiKit.Badge(string.Empty, _workshopTone, out _workshopChip);
		((CanvasItem)_workshopBadge).Visible = false;
		((Node)val11).AddChild((Node)(object)PaletteBadge(_workshopBadge, () => _workshopTone), false, (InternalMode)0);
		_settingsButton = UiKit.Button(string.Empty, "GlassIconButton", 36f, 32f);
		((Control)_settingsButton).SizeFlagsVertical = (SizeFlags)4;
		((Control)_settingsButton).MouseDefaultCursorShape = (CursorShape)2;
		_settingsButton.ExpandIcon = false;
		_settingsButton.IconAlignment = (HorizontalAlignment)1;
		BindPalette((GodotObject)(object)_settingsButton, delegate
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			_settingsButton.Icon = (Texture2D)(object)UiGlyphs.Gear(16, UiKit.Palette.IsLiquidGlass ? UiKit.Text : UiKit.Accent);
		});
		BindKeyTooltip((Control)(object)_settingsButton, "settings.action.open.tooltip");
		((BaseButton)_settingsButton).Pressed += OpenSettings;
		((Node)header).AddChild((Node)(object)mark, false, (InternalMode)0);
		((Node)header).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)header).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)header).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)header).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)header).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)header).AddChild((Node)(object)val11, false, (InternalMode)0);
		((Node)header).AddChild((Node)(object)_settingsButton, false, (InternalMode)0);
		return (Control)(object)header;
	}

	private static Control BuildWorkspace()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		PanelContainer val2 = UiKit.Card();
		((Control)val2).CustomMinimumSize = new Vector2(640f, 340f);
		((Control)val2).SizeFlagsStretchRatio = 2.4f;
		((Node)val2).AddChild((Node)(object)BuildModList(), false, (InternalMode)0);
		PanelContainer val3 = UiKit.Card("GlassInspectorPanel");
		((Control)val3).CustomMinimumSize = new Vector2(340f, 340f);
		((Control)val3).SizeFlagsStretchRatio = 1f;
		((Node)val3).AddChild((Node)(object)BuildDetails(), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		return (Control)val;
	}

	private static Control BuildModList()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected O, but got Unknown
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Expected O, but got Unknown
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Expected O, but got Unknown
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Expected O, but got Unknown
		//IL_0781: Unknown result type (might be due to invalid IL or missing references)
		//IL_078b: Expected O, but got Unknown
		//IL_0835: Unknown result type (might be due to invalid IL or missing references)
		//IL_083a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0841: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_0851: Unknown result type (might be due to invalid IL or missing references)
		//IL_085e: Expected O, but got Unknown
		//IL_07ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0804: Unknown result type (might be due to invalid IL or missing references)
		//IL_080a: Expected O, but got Unknown
		//IL_08fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0900: Unknown result type (might be due to invalid IL or missing references)
		//IL_0910: Unknown result type (might be due to invalid IL or missing references)
		//IL_0918: Unknown result type (might be due to invalid IL or missing references)
		//IL_0920: Unknown result type (might be due to invalid IL or missing references)
		//IL_0929: Expected O, but got Unknown
		//IL_0964: Unknown result type (might be due to invalid IL or missing references)
		//IL_096e: Unknown result type (might be due to invalid IL or missing references)
		//IL_097c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0986: Unknown result type (might be due to invalid IL or missing references)
		//IL_0999: Unknown result type (might be due to invalid IL or missing references)
		//IL_09a4: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_search = new LineEdit
		{
			ClearButtonEnabled = true,
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(220f, 38f)
		};
		BindKeyPlaceholder(_search, "mods.search.placeholder");
		LineEdit? search = _search;
		object obj = _003C_003Ec._003C_003E9__340_0;
		if (obj == null)
		{
			TextChangedEventHandler val3 = delegate
			{
				QueueRebuild();
			};
			_003C_003Ec._003C_003E9__340_0 = val3;
			obj = (object)val3;
		}
		search.TextChanged += (TextChangedEventHandler)obj;
		_resultCount = UiKit.Label(string.Empty, "DimLabel");
		_resultCount.HorizontalAlignment = (HorizontalAlignment)2;
		_resultCount.VerticalAlignment = (VerticalAlignment)1;
		((Control)_resultCount).CustomMinimumSize = new Vector2(104f, 0f);
		((Node)val2).AddChild((Node)(object)_search, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_resultCount, false, (InternalMode)0);
		_updateAll = LocalizedKeyButton("workshop.action.downloadAll", "MiniButton", 0f, 30f);
		((Control)_updateAll).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_updateAll, "workshop.action.downloadAll.tooltip");
		((CanvasItem)_updateAll).Visible = false;
		((BaseButton)_updateAll).Pressed += DownloadAllWorkshopUpdates;
		((Node)val2).AddChild((Node)(object)_updateAll, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)BuildFilterToggle(), false, (InternalMode)0);
		HFlowContainer val4 = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 7);
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 7);
		_moveRepeatTimer = new Timer
		{
			OneShot = true,
			WaitTime = 0.35
		};
		_moveRepeatTimer.Timeout += RepeatSelectedMove;
		((Node)val4).AddChild((Node)(object)_moveRepeatTimer, false, (InternalMode)0);
		_filterGroup = new ButtonGroup();
		AddFilterChip((Container)(object)val4, "mods.filter.all", ModFilter.All);
		AddFilterChip((Container)(object)val4, "mods.filter.enabled", ModFilter.Enabled);
		AddFilterChip((Container)(object)val4, "mods.filter.disabled", ModFilter.Disabled);
		AddFilterChip((Container)(object)val4, "mods.filter.workshop", ModFilter.Workshop);
		AddFilterChip((Container)(object)val4, "mods.filter.local", ModFilter.Local);
		_updatesFilter = AddFilterChip((Container)(object)val4, "mods.filter.updates", ModFilter.Updates);
		((CanvasItem)_updatesFilter).Visible = false;
		((Node)val4).AddChild((Node)new VSeparator
		{
			CustomMinimumSize = new Vector2(1f, 22f)
		}, false, (InternalMode)0);
		_moveUp = UiKit.Button("▲", "IconButton", 34f, 30f);
		BindKeyTooltip((Control)(object)_moveUp, "mods.action.moveUp.tooltip");
		((BaseButton)_moveUp).Shortcut = UiKit.Keys((Key)4194320, ctrl: false, alt: true);
		BindRepeatingMoveButton(_moveUp, -1);
		_moveDown = UiKit.Button("▼", "IconButton", 34f, 30f);
		BindKeyTooltip((Control)(object)_moveDown, "mods.action.moveDown.tooltip");
		((BaseButton)_moveDown).Shortcut = UiKit.Keys((Key)4194322, ctrl: false, alt: true);
		BindRepeatingMoveButton(_moveDown, 1);
		BindPalette((GodotObject)(object)_moveUp, delegate
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			_moveUp.Text = (UiKit.Palette.IsLiquidGlass ? string.Empty : "▲");
			_moveUp.Icon = (Texture2D)(object)(UiKit.Palette.IsLiquidGlass ? UiGlyphs.Chevron(14, UiKit.Text, 1.7f, up: true) : null);
			_moveUp.IconAlignment = (HorizontalAlignment)1;
		});
		BindPalette((GodotObject)(object)_moveDown, delegate
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			_moveDown.Text = (UiKit.Palette.IsLiquidGlass ? string.Empty : "▼");
			_moveDown.Icon = (Texture2D)(object)(UiKit.Palette.IsLiquidGlass ? UiGlyphs.Chevron(14, UiKit.Text, 1.7f) : null);
			_moveDown.IconAlignment = (HorizontalAlignment)1;
		});
		_enableAll = LocalizedKeyButton("mods.action.enableAll", "MiniButton", 0f, 30f);
		BindKeyTooltip((Control)(object)_enableAll, "mods.action.enableAll.tooltip");
		((BaseButton)_enableAll).Pressed += delegate
		{
			SetAllEnabled(enabled: true);
		};
		_disableAll = LocalizedKeyButton("mods.action.disableAll", "MiniButton", 0f, 30f);
		BindKeyTooltip((Control)(object)_disableAll, "mods.action.disableAll.tooltip");
		((BaseButton)_disableAll).Pressed += delegate
		{
			SetAllEnabled(enabled: false);
		};
		_assignGroup = LocalizedKeyButton("mods.action.moveToGroup", "MiniButton", 0f, 30f);
		BindKeyTooltip((Control)(object)_assignGroup, "mods.action.moveToGroup.tooltip");
		((BaseButton)_assignGroup).Pressed += ShowGroupMenuForVisibleRows;
		((Node)val4).AddChild((Node)(object)_moveUp, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_moveDown, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_enableAll, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_disableAll, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_assignGroup, false, (InternalMode)0);
		ModReorderTree modReorderTree = new ModReorderTree();
		((Tree)modReorderTree).Columns = ColumnCount;
		((Tree)modReorderTree).HideRoot = true;
		((Tree)modReorderTree).ColumnTitlesVisible = true;
		((Tree)modReorderTree).SelectMode = (SelectModeEnum)1;
		((Tree)modReorderTree).AllowReselect = true;
		((Tree)modReorderTree).AllowSearch = false;
		((Control)modReorderTree).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)modReorderTree).SizeFlagsVertical = (SizeFlags)3;
		modReorderTree.CanStartDrag = CanStartTreeDrag;
		modReorderTree.CanDropEntry = CanDropTreeEntry;
		modReorderTree.PlacementFor = TreeDropPlacementFor;
		modReorderTree.DropEntry = DropTreeEntry;
		modReorderTree.DropEntryAt = DropTreeEntryAt;
		modReorderTree.DropTargetChanged = OnTreeDropTargetChanged;
		modReorderTree.HeadingDwell = OnTreeHeadingDwell;
		modReorderTree.MoveRequested = MoveSelected;
		modReorderTree.ActivateRequested = ToggleFromKeyboard;
		modReorderTree.SidewaysRequested = OnTreeSideways;
		modReorderTree.DragActiveChanged = OnTreeDragActiveChanged;
		modReorderTree.IsHeaderKey = IsFolderKey;
		modReorderTree.HeaderActivated = OnTreeHeaderActivated;
		_modTree = modReorderTree;
		((Control)_modTree).AddThemeConstantOverride(StringName.op_Implicit("item_margin"), 0);
		ConfigureTreeColumns();
		((Tree)_modTree).ItemSelected += OnTreeSelectionChanged;
		((Tree)_modTree).ItemSelected += delegate
		{
			bool rebuilding = _rebuildingTree;
			bool pointer = Input.IsMouseButtonPressed((MouseButton)1) || Input.IsMouseButtonPressed((MouseButton)2);
			if (!rebuilding)
			{
				_listInputAt = Time.GetTicksMsec();
			}
			Defer(delegate
			{
				if (_modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
				{
					TreeItem selected = ((Tree)_modTree).GetSelected();
					if (selected != null)
					{
						RevealBelowPinnedTitle(selected);
						if (!rebuilding && !pointer)
						{
							RevealAboveFab(selected);
						}
					}
				}
			});
		};
		_modTree.ToggleColumn = 1;
		_modTree.ToggleRequested = ToggleEntry;
		_modTree.ContextMenuRequested = ShowGroupMenuForRow;
		_groupMenu = new PopupMenu();
		BindPalette((GodotObject)(object)_groupMenu, delegate
		{
			((Window)_groupMenu).AddThemeFontOverride(StringName.op_Implicit("font"), (Font)(object)UiKit.UiFont);
			((Window)_groupMenu).AddThemeFontOverride(StringName.op_Implicit("font_separator"), (Font)(object)UiKit.UiFont);
		});
		((Window)_groupMenu).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(14));
		((Window)_groupMenu).AddThemeFontSizeOverride(StringName.op_Implicit("font_separator_size"), UiKit.ScaleFontSize(12));
		PopupMenu? groupMenu = _groupMenu;
		object obj2 = _003C_003EO._003C64_003E__OnGroupMenuIdPressed;
		if (obj2 == null)
		{
			IdPressedEventHandler val5 = OnGroupMenuIdPressed;
			_003C_003EO._003C64_003E__OnGroupMenuIdPressed = val5;
			obj2 = (object)val5;
		}
		groupMenu.IdPressed += (IdPressedEventHandler)obj2;
		((Node)_modTree).AddChild((Node)(object)_groupMenu, false, (InternalMode)0);
		BuildFolderMenu((Control)(object)_modTree);
		BuildStickyFolderHeader(_modTree);
		_emptyState = new VBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			Alignment = (AlignmentMode)1
		};
		((Control)_emptyState).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		Label val6 = LocalizedKeyLabel("mods.empty.message", "MutedLabel");
		val6.HorizontalAlignment = (HorizontalAlignment)1;
		_clearFilters = LocalizedKeyButton("mods.empty.clearFilters", "SecondaryButton", 180f, 36f);
		((Control)_clearFilters).SizeFlagsHorizontal = (SizeFlags)4;
		((BaseButton)_clearFilters).Pressed += ClearFilters;
		((Node)_emptyState).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)_emptyState).AddChild((Node)(object)_clearFilters, false, (InternalMode)0);
		Control val7 = new Control
		{
			Name = StringName.op_Implicit("ListArea"),
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val7).AddChild((Node)(object)UiKit.FullRect<ModReorderTree>(_modTree), false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)UiKit.FullRect<VBoxContainer>(_emptyState), false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)BuildFab(val7), false, (InternalMode)0);
		_filterRow = (Control?)(object)val4;
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)BuildFilterSummary(), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)BuildGroupBar(), false, (InternalMode)0);
		ApplyFilterCollapse();
		((Node)val).AddChild((Node)(object)val7, false, (InternalMode)0);
		return (Control)val;
	}

	private static Button AddFilterChip(Container parent, string key, ModFilter filter)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected O, but got Unknown
		Button val = UiKit.Chip(string.Empty, _filterGroup, filter == _filter);
		BindKeyText(val, key);
		((BaseButton)val).Toggled += (ToggledEventHandler)delegate(bool pressed)
		{
			if (pressed && _filter != filter)
			{
				_filter = filter;
				QueueRebuild();
			}
		};
		FilterChips.Add(val);
		((Node)parent).AddChild((Node)(object)val, false, (InternalMode)0);
		return val;
	}

	private static void ConfigureTreeColumns()
	{
		if (_modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
		{
			((Tree)_modTree).Columns = ColumnCount;
			UpdateTreeColumnTitles();
			((Tree)_modTree).SetColumnCustomMinimumWidth(0, 86);
			((Tree)_modTree).SetColumnCustomMinimumWidth(1, 34);
			((Tree)_modTree).SetColumnCustomMinimumWidth(2, 190);
			((Tree)_modTree).SetColumnCustomMinimumWidth(3, (!_groupListView) ? 96 : 0);
			((Tree)_modTree).SetColumnCustomMinimumWidth(4, 78);
			((Tree)_modTree).SetColumnCustomMinimumWidth(5, 96);
			((Tree)_modTree).SetColumnCustomMinimumWidth(6, Math.Max(88, UiKit.ScaleFontSize(100)));
			((Tree)_modTree).SetColumnExpand(0, false);
			((Tree)_modTree).SetColumnExpand(1, false);
			((Tree)_modTree).SetColumnExpand(2, true);
			((Tree)_modTree).SetColumnExpand(3, false);
			((Tree)_modTree).SetColumnExpand(4, false);
			((Tree)_modTree).SetColumnExpand(5, false);
			((Tree)_modTree).SetColumnExpand(6, false);
			((Tree)_modTree).SetColumnTitleAlignment(0, (HorizontalAlignment)1);
			((Tree)_modTree).SetColumnTitleAlignment(1, (HorizontalAlignment)1);
			((Tree)_modTree).SetColumnTitleAlignment(2, (HorizontalAlignment)0);
			((Tree)_modTree).SetColumnTitleAlignment(3, (HorizontalAlignment)0);
			((Tree)_modTree).SetColumnTitleAlignment(4, (HorizontalAlignment)1);
			((Tree)_modTree).SetColumnTitleAlignment(5, (HorizontalAlignment)1);
			((Tree)_modTree).SetColumnTitleAlignment(6, (HorizontalAlignment)1);
			((Tree)_modTree).SetColumnClipContent(2, true);
			((Tree)_modTree).SetColumnClipContent(3, true);
			if (_showAffectsColumn)
			{
				((Tree)_modTree).SetColumnCustomMinimumWidth(7, 104);
				((Tree)_modTree).SetColumnExpand(7, false);
				((Tree)_modTree).SetColumnTitleAlignment(7, (HorizontalAlignment)1);
			}
		}
	}

	private static void UpdateTreeColumnTitles()
	{
		if (_modTree != null)
		{
			((Tree)_modTree).SetColumnTitle(0, MlmLocManager.Get("mods.column.order"));
			((Tree)_modTree).SetColumnTitle(1, MlmLocManager.Get("mods.column.enabled"));
			((Tree)_modTree).SetColumnTitle(2, MlmLocManager.Get("mods.column.name"));
			((Tree)_modTree).SetColumnTitle(3, _groupListView ? string.Empty : MlmLocManager.Get("mods.column.group"));
			((Tree)_modTree).SetColumnTitle(4, MlmLocManager.Get("mods.column.version"));
			((Tree)_modTree).SetColumnTitle(5, MlmLocManager.Get("mods.column.source"));
			((Tree)_modTree).SetColumnTitle(6, MlmLocManager.Get("mods.column.status"));
			if (_showAffectsColumn)
			{
				((Tree)_modTree).SetColumnTitle(7, MlmLocManager.Get("mods.column.affectsGameplay"));
			}
		}
	}

	private static void Defer(Action action)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From(action);
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void QueueRebuild(bool scrollToSelection = false)
	{
		_scrollAfterRebuild |= scrollToSelection;
		if (!_rebuildQueued)
		{
			_rebuildQueued = true;
			Defer(RunQueuedRebuild);
		}
	}

	private static void RunQueuedRebuild()
	{
		_rebuildQueued = false;
		if (_modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
		{
			RebuildTree();
			SelectFirstRowIfFilteredOut();
			KeepCursorRow();
			if (_scrollAfterRebuild)
			{
				_scrollAfterRebuild = false;
				ScrollSelectionIntoView();
			}
		}
	}

	private static void RebuildTree()
	{
		if (_modTree == null || _plan == null)
		{
			return;
		}
		UpdateFilterSummary();
		ModReorderTree tree = _modTree;
		RefreshTranslatedMetadata(_plan.Entries);
		VScrollBar verticalScroll = ((IEnumerable)((Node)tree).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
		VScrollBar obj = verticalScroll;
		double? previousScroll = ((obj != null) ? new double?(((Range)obj).Value) : ((double?)null));
		_rebuildingTree = true;
		try
		{
			_rowStatusLookup = new RowStatusLookup(_plan);
			string selectedKey = _selected?.Key;
			LineEdit? search = _search;
			string query = ((search != null) ? search.Text.Trim() : null) ?? string.Empty;
			((Tree)_modTree).Clear();
			TreeItem val = ((Tree)_modTree).CreateItem((TreeItem)null, -1);
			if (val == null)
			{
				GD.PushWarning("[ModLaunchManager] tree rebuild was blocked; retrying next frame.");
				return;
			}
			TreeBindings.Clear();
			RowLoadPositions.Clear();
			int num = 0;
			foreach (LaunchEntry entry in _plan.Entries)
			{
				RowLoadPositions.Add(entry.Enabled ? (++num) : 0);
			}
			bool revealAll = _dragPreviewActive;
			int num2 = _plan.Entries.Count((LaunchEntry entry) => entry.Enabled && Shown(entry));
			int num3 = _plan.Entries.Count((LaunchEntry entry) => !entry.Enabled && Shown(entry));
			if (_groupListView)
			{
				RevealSelectionAfterSuspend();
				if (!AddGroupFolders(val, Shown, Matches, revealAll, selectedKey, query))
				{
					return;
				}
			}
			else
			{
				bool[] array = new bool[2] { true, false };
				foreach (bool flag in array)
				{
					TreeItem val2 = null;
					for (int j = 0; j < _plan.Entries.Count; j++)
					{
						LaunchEntry launchEntry = _plan.Entries[j];
						if (launchEntry.Enabled == flag && Shown(launchEntry))
						{
							if (val2 == null)
							{
								val2 = AddModSectionHeader(val, flag, flag ? num2 : num3);
							}
							if (val2 == null || !AddModRow(val2, launchEntry, j, revealAll && Matches(launchEntry), selectedKey))
							{
								return;
							}
						}
					}
				}
			}
			bool Matches(LaunchEntry entry)
			{
				if (MatchesSearch(entry, query))
				{
					return MatchesFilter(entry);
				}
				return false;
			}
			bool Shown(LaunchEntry entry)
			{
				if (!revealAll)
				{
					return Matches(entry);
				}
				return true;
			}
		}
		finally
		{
			_rowStatusLookup = null;
			_rebuildingTree = false;
		}
		UpdateListChrome();
		MarkFoldedSelection();
		UpdateFoldAllButton();
		Defer(OnTreeScrolled);
		if (_scrollAfterRebuild || !previousScroll.HasValue || verticalScroll == null)
		{
			return;
		}
		Defer(delegate
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)tree) && GodotObject.IsInstanceValid((GodotObject)(object)verticalScroll))
			{
				((Range)verticalScroll).Value = previousScroll.Value;
			}
		});
	}

	private static bool AddModRow(TreeItem parent, LaunchEntry entry, int index, bool highlightAsMatch, string? selectedKey, Texture2D? indent = null)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_0332: Unknown result type (might be due to invalid IL or missing references)
		ModReorderTree? modTree = _modTree;
		TreeItem val = ((modTree != null) ? ((Tree)modTree).CreateItem(parent, -1) : null);
		if (val == null)
		{
			return false;
		}
		if (indent != null)
		{
			val.SetIcon(2, indent);
		}
		val.SetCustomMinimumHeight(34);
		val.SetText(0, (entry.CanMove ? "≡" : "◆") + " " + OrderNumber(entry, index));
		val.SetTextAlignment(0, (HorizontalAlignment)1);
		val.SetMetadata(0, Variant.op_Implicit(entry.Key));
		val.SetCellMode(1, (TreeCellMode)3);
		val.SetIcon(1, ToggleIcon(entry));
		val.SetText(2, entry.DisplayName);
		val.SetTextOverrunBehavior(2, (OverrunBehavior)3);
		val.SetText(3, _groupListView ? string.Empty : (string.IsNullOrWhiteSpace(entry.Group) ? "—" : entry.Group));
		val.SetTextOverrunBehavior(3, (OverrunBehavior)3);
		val.SetText(4, entry.Mod.manifest?.version ?? "—");
		val.SetTextAlignment(4, (HorizontalAlignment)1);
		val.SetText(5, SourceName(entry.Mod.modSource));
		val.SetTextAlignment(5, (HorizontalAlignment)1);
		val.SetTextAlignment(6, (HorizontalAlignment)1);
		if (_showAffectsColumn)
		{
			val.SetText(7, AffectsGameplayText(entry));
			val.SetTextAlignment(7, (HorizontalAlignment)1);
		}
		string text = (string.IsNullOrWhiteSpace(entry.Alias) ? $"{entry.Name}\n{entry.Id}\n{entry.Mod.path}" : $"{entry.Alias}\n{MlmLocManager.Get("mods.tooltip.originalName")}: {entry.Name}\n{entry.Id}\n{entry.Mod.path}");
		bool flag = string.Equals(entry.Key, _controllerMoveKey, StringComparison.Ordinal);
		if (flag)
		{
			val.SetText(0, "↕ " + OrderNumber(entry, index));
		}
		for (int i = 0; i < ColumnCount; i++)
		{
			val.SetSelectable(i, true);
			val.SetTooltipText(i, text);
			if (flag)
			{
				val.SetCustomBgColor(i, UiKit.Alpha(UiKit.Accent, 0.2f), false);
			}
			else if (highlightAsMatch)
			{
				val.SetCustomBgColor(i, UiKit.RowTint(UiKit.Accent, 0.12f), false);
			}
		}
		val.SetCustomColor(4, entry.Enabled ? UiKit.TextMuted : UiKit.TextDim);
		val.SetCustomColor(5, entry.Enabled ? UiKit.TextMuted : UiKit.TextDim);
		val.SetCustomColor(3, entry.Enabled ? UiKit.TextMuted : UiKit.TextDim);
		if (_showAffectsColumn)
		{
			val.SetCustomColor(7, entry.Enabled ? UiKit.TextMuted : UiKit.TextDim);
		}
		TreeBindings.Add(new TreeBinding(val, entry));
		ApplyRowState(val, entry);
		if (string.Equals(entry.Key, selectedKey, StringComparison.Ordinal))
		{
			val.Select(2);
		}
		return true;
	}

	private static string OrderNumber(LaunchEntry entry, int index)
	{
		if (entry.Enabled)
		{
			if (index >= RowLoadPositions.Count)
			{
				return (index + 1).ToString();
			}
			return RowLoadPositions[index].ToString();
		}
		return "—";
	}

	private static TreeItem? AddModSectionHeader(TreeItem root, bool enabled, int count)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		ModReorderTree? modTree = _modTree;
		TreeItem val = ((modTree != null) ? ((Tree)modTree).CreateItem(root, -1) : null);
		if (val == null)
		{
			return null;
		}
		string text = "section:" + (enabled ? "enabled" : "disabled");
		val.SetMetadata(0, Variant.op_Implicit(text));
		bool value = default(bool);
		val.Collapsed = _filter != ModFilter.Updates && CollapsedTreeSections.TryGetValue(text, out value) && value;
		val.SetCustomMinimumHeight(28);
		val.SetText(2, MlmLocManager.Get(enabled ? "mods.section.enabled" : "mods.section.disabled", ("count", count)));
		val.SetText(6, enabled ? MlmLocManager.Get("mods.section.willLoad") : MlmLocManager.Get("mods.section.willNotLoad"));
		val.SetTextAlignment(6, (HorizontalAlignment)1);
		Color val2 = (enabled ? UiKit.Success : UiKit.TextDim);
		Color val3 = (enabled ? UiKit.RowTint(UiKit.Success, 0.1f) : UiKit.RowTint(UiKit.TextMuted, 0.07f));
		for (int i = 0; i < ColumnCount; i++)
		{
			val.SetSelectable(i, false);
			val.SetCustomBgColor(i, val3, false);
		}
		val.SetCustomColor(2, val2);
		val.SetCustomColor(6, val2);
		return val;
	}

	private static void OnTreeItemCollapsed(TreeItem item)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!_rebuildingTree)
		{
			Variant metadata = item.GetMetadata(0);
			string text = ((Variant)(ref metadata)).AsString();
			if (text.StartsWith("section:", StringComparison.Ordinal))
			{
				CollapsedTreeSections[text] = item.Collapsed;
			}
			Defer(SyncListSpacer);
		}
	}

	private static void ApplyRowState(TreeItem item, LaunchEntry entry)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		ModRowStatus modRowStatus = (_pumpActive ? LoadStatus(entry) : PlanStatus(entry));
		Color color = modRowStatus.Color;
		Color val = (_pumpActive ? color : (entry.Enabled ? UiKit.Text : UiKit.TextDim));
		item.SetText(6, modRowStatus.Text);
		item.SetTooltipText(6, modRowStatus.Tooltip ?? modRowStatus.Text);
		item.SetCustomColor(6, color);
		item.SetCustomColor(0, entry.CanMove ? val : UiKit.TextMuted);
		item.SetCustomColor(2, val);
		item.SetCustomColor(3, string.IsNullOrWhiteSpace(entry.Group) ? UiKit.TextDim : val);
	}

	private static string AffectsGameplayText(LaunchEntry entry)
	{
		ModManifest manifest = entry.Mod.manifest;
		if (manifest == null)
		{
			return "—";
		}
		return MlmLocManager.Get(manifest.affectsGameplay ? "mods.affects.yes" : "mods.affects.no");
	}

	private static bool HasDuplicateSiblings(LaunchEntry entry)
	{
		return _rowStatusLookup?.HasDuplicateSiblings(entry) ?? _plan?.HasDuplicateSiblings(entry) ?? false;
	}

	private static ModRowStatus PlanStatus(LaunchEntry entry)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		WorkshopStatus workshopStatus = WorkshopUpdates.For(entry.Mod);
		WorkshopBranchStatus workshopBranchStatus = WorkshopBranches.For(entry.Mod);
		if (workshopBranchStatus != null && workshopBranchStatus.Busy)
		{
			return new ModRowStatus(MlmLocManager.Get("workshop.switch.rowDownloading"), UiKit.Warning, MlmLocManager.Get("workshop.switch.downloading", ("branch", workshopBranchStatus.TargetBranch ?? string.Empty)));
		}
		if (workshopBranchStatus != null && workshopBranchStatus.ErrorKey != null)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.warning"), UiKit.Warning, MlmLocManager.Get(workshopBranchStatus.ErrorKey));
		}
		if (workshopStatus != null && workshopStatus.HasIncompatibleAvailableUpdate)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.updateBranchMismatch"), UiKit.Danger, MlmLocManager.Get("details.warning.latestBranchUnsupported", ("branch", ModErrorText.CurrentBranchName()), ("ranges", workshopStatus.LatestBranchRanges ?? MlmLocManager.Get("common.value.unknown"))));
		}
		ModRowStatus? modRowStatus;
		if (!entry.Enabled)
		{
			bool flag = HasDuplicateSiblings(entry);
			string text = (flag ? MlmLocManager.Get("mods.status.duplicateOff.tooltip") : MlmLocManager.Get("details.badge.disabled"));
			modRowStatus = WorkshopUpdateRowStatus(workshopStatus);
			if (modRowStatus.HasValue)
			{
				ModRowStatus valueOrDefault = modRowStatus.GetValueOrDefault();
				return valueOrDefault with
				{
					Tooltip = valueOrDefault.Text + "\n" + text
				};
			}
			return new ModRowStatus(MlmLocManager.Get(flag ? "mods.status.duplicateOff" : "mods.status.disabled"), UiKit.TextDim, text);
		}
		ModGameVersionCheck modGameVersionCheck = ModCompatibility.CheckGameVersion(entry.Mod);
		if (modGameVersionCheck.State == ModGameVersionCompatibility.Unsupported)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.gameVersionMismatch"), UiKit.Danger, MlmLocManager.Get("details.warning.gameVersionUnsupported", ("required", modGameVersionCheck.RequiredVersion), ("current", modGameVersionCheck.CurrentVersion)));
		}
		if (modGameVersionCheck.State == ModGameVersionCompatibility.InvalidMinimumVersion)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.invalidMinimumVersion"), UiKit.Danger, MlmLocManager.Get("details.warning.invalidMinimumVersion", ("version", modGameVersionCheck.RequiredVersion)));
		}
		List<IGrouping<LaunchEntry, ModConflict>> list = ActiveConflicts(entry);
		if (list != null && list.Count > 0)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.incompatible"), UiKit.Danger, MlmLocManager.Get("mods.status.incompatible.tooltip", ("names", string.Join(MlmLocManager.Get("common.listSeparator"), list.Select((IGrouping<LaunchEntry, ModConflict> conflict) => conflict.Key.DisplayName)))));
		}
		List<LocString> errors = entry.Mod.errors;
		if (errors != null && errors.Count > 0)
		{
			string tooltip = string.Join('\n', errors.Select(ModErrorText.Describe));
			if (!errors.Any(ModErrorText.IsSteamBranchMismatch))
			{
				return new ModRowStatus(MlmLocManager.Get("mods.status.warning"), UiKit.Warning, tooltip);
			}
			return new ModRowStatus(MlmLocManager.Get("mods.status.branchMismatch"), UiKit.Danger, tooltip);
		}
		if (workshopBranchStatus?.Active != null)
		{
			return new ModRowStatus(MlmLocManager.Get("workshop.switch.active", ("branch", workshopBranchStatus.Active.Branch)), UiKit.Success);
		}
		modRowStatus = WorkshopUpdateRowStatus(workshopStatus);
		if (modRowStatus.HasValue)
		{
			return modRowStatus.GetValueOrDefault();
		}
		if (entry.IsManager)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.fixed"), UiKit.TextMuted, MlmLocManager.Get("details.toggle.managerTooltip"));
		}
		if (entry.IsCompat)
		{
			return new ModRowStatus(MlmLocManager.Get("mods.status.repairBoundary"), UiKit.TextMuted, MlmLocManager.Get("details.badge.repairsLaterMods"));
		}
		return new ModRowStatus(MlmLocManager.Get("mods.status.pending"), UiKit.TextMuted);
	}

	private static ModRowStatus LoadStatus(LaunchEntry entry)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Invalid comparison between Unknown and I4
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Expected I4, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		Mod mod = entry.Mod;
		List<LocString> errors = mod.errors;
		string text = ((errors != null && errors.Count > 0) ? string.Join('\n', errors.Select(ModErrorText.Describe)) : null);
		if ((int)mod.state == 1)
		{
			List<LocString> errors2 = mod.errors;
			if (errors2 == null || errors2.Count <= 0)
			{
				return new ModRowStatus(MlmLocManager.Get("mods.status.loaded"), UiKit.Success);
			}
			return new ModRowStatus(MlmLocManager.Get("mods.status.warnings"), UiKit.Warning, text);
		}
		ModLoadState state = mod.state;
		return (state - 2) switch
		{
			0 => new ModRowStatus(MlmLocManager.Get("mods.status.failed"), UiKit.Danger, text), 
			2 => new ModRowStatus(MlmLocManager.Get("mods.status.duplicate"), UiKit.TextDim, MlmLocManager.Get("mods.status.duplicateOff.tooltip")), 
			1 => new ModRowStatus(entry.Enabled ? MlmLocManager.Get("mods.status.skipped") : MlmLocManager.Get("mods.status.disabled"), UiKit.TextDim, entry.Enabled ? text : MlmLocManager.Get("details.badge.disabled")), 
			3 => new ModRowStatus(MlmLocManager.Get("mods.status.runtime"), UiKit.Text), 
			_ => new ModRowStatus(MlmLocManager.Get("mods.status.waiting"), UiKit.TextMuted), 
		};
	}

	private static void UpdateListChrome()
	{
		int num = _plan?.Entries.Count ?? 0;
		bool flag = TreeBindings.Count > 0;
		if (_resultCount != null)
		{
			_resultCount.Text = ((flag && TreeBindings.Count == num) ? MlmLocManager.Get("mods.summary.all", ("total", num)) : MlmLocManager.Get("mods.summary.filtered", ("visible", TreeBindings.Count), ("total", num)));
		}
		if (_modTree != null)
		{
			((CanvasItem)_modTree).Visible = flag;
		}
		if (_emptyState != null)
		{
			((CanvasItem)_emptyState).Visible = !flag;
		}
		int num2 = ((_selected == null) ? (-1) : TreeBindings.FindIndex((TreeBinding binding) => binding.Entry == _selected));
		bool flag2 = !_pumpActive && num2 >= 0 && (_selected?.CanMove ?? false);
		if (_moveUp != null)
		{
			((BaseButton)_moveUp).Disabled = !flag2 || num2 <= 0 || !ShareMoveRange(TreeBindings[num2 - 1].Entry, _selected);
		}
		if (_moveDown != null)
		{
			((BaseButton)_moveDown).Disabled = !flag2 || num2 < 0 || num2 >= TreeBindings.Count - 1 || !ShareMoveRange(TreeBindings[num2 + 1].Entry, _selected);
		}
		if (_enableAll != null)
		{
			((BaseButton)_enableAll).Disabled = _pumpActive || !flag;
		}
		if (_disableAll != null)
		{
			((BaseButton)_disableAll).Disabled = _pumpActive || !flag;
		}
		if (_assignGroup != null)
		{
			((BaseButton)_assignGroup).Disabled = _pumpActive || !flag;
		}
	}

	private static bool MatchesSearch(LaunchEntry entry, string query)
	{
		if (string.IsNullOrWhiteSpace(query))
		{
			return true;
		}
		_003C_003Ey__InlineArray9<string> buffer = default(_003C_003Ey__InlineArray9<string>);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 0) = entry.Name;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 1) = entry.TranslatedName;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 2) = entry.DisplayDescription;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 3) = entry.Alias;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 4) = entry.Group;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 5) = entry.Id;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 6) = TextEncodingRepair.Repair(entry.Mod.manifest?.author);
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 7) = entry.ManifestDescription;
		global::_003CPrivateImplementationDetails_003E.InlineArrayElementRef<_003C_003Ey__InlineArray9<string>, string>(ref buffer, 8) = WorkshopUpdates.TagSearchText(entry.Mod);
		return string.Join('\n', global::_003CPrivateImplementationDetails_003E.InlineArrayAsReadOnlySpan<_003C_003Ey__InlineArray9<string>, string>(in buffer, 9)).Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private static bool MatchesFilter(LaunchEntry entry)
	{
		if (MatchesStateFilter(entry))
		{
			return MatchesGroupFilter(entry);
		}
		return false;
	}

	private static bool MatchesStateFilter(LaunchEntry entry)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Invalid comparison between Unknown and I4
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Invalid comparison between Unknown and I4
		return _filter switch
		{
			ModFilter.Enabled => entry.Enabled, 
			ModFilter.Disabled => !entry.Enabled, 
			ModFilter.Workshop => (int)entry.Mod.modSource == 2, 
			ModFilter.Local => (int)entry.Mod.modSource != 2, 
			ModFilter.Updates => WorkshopUpdates.For(entry.Mod)?.HasAvailableUpdate ?? false, 
			_ => true, 
		};
	}

	private static bool MatchesGroupFilter(LaunchEntry entry)
	{
		string groupFilter = _groupFilter;
		if (groupFilter != null)
		{
			if (groupFilter == "\u0001ungrouped")
			{
				return string.IsNullOrWhiteSpace(entry.Group);
			}
			return InGroup(entry.Group, _groupFilter);
		}
		return true;
	}

	private static void ClearFilters()
	{
		if (_search != null)
		{
			_search.Text = string.Empty;
		}
		_filter = ModFilter.All;
		if (FilterChips.Count > 0)
		{
			((BaseButton)FilterChips[0]).ButtonPressed = true;
		}
		_groupFilter = null;
		_groupDeleteArmed = false;
		QueueRebuild();
		Defer(RebuildGroupRow);
	}

	private static LaunchEntry? EntryFor(TreeItem? item)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (item == null || _plan == null)
		{
			return null;
		}
		Variant metadata = item.GetMetadata(0);
		string text = ((Variant)(ref metadata)).AsString();
		if (!string.IsNullOrEmpty(text))
		{
			return _plan.Find(text);
		}
		return null;
	}

	private static void OnTreeSelectionChanged()
	{
		if (_rebuildingTree || _modTree == null)
		{
			return;
		}
		LaunchEntry launchEntry = EntryFor(((Tree)_modTree).GetSelected());
		if (launchEntry != null && launchEntry != _selected)
		{
			if (!SaveSelectedOrganization(showFeedback: false))
			{
				QueueRebuild(scrollToSelection: true);
				return;
			}
			_selected = launchEntry;
			_revealSelectionAfterSuspend = _groupListView && FolderFoldingSuspended();
			UpdateDetails();
			UpdateListChrome();
		}
	}

	private static Texture2D? ToggleIcon(LaunchEntry entry)
	{
		if (_modTree == null)
		{
			return null;
		}
		bool flag = entry.CanToggle && !_pumpActive;
		ModReorderTree? modTree = _modTree;
		string text = (entry.Enabled ? ((!flag) ? "checked_disabled" : "checked") : ((!flag) ? "unchecked_disabled" : "unchecked"));
		return ((Control)modTree).GetThemeIcon(StringName.op_Implicit(text), StringName.op_Implicit("Tree"));
	}

	private static void ToggleEntry(string entryKey)
	{
		if (_rebuildingTree || _plan == null || _pumpActive)
		{
			return;
		}
		if (IsFolderKey(entryKey))
		{
			ToggleFolder(entryKey);
			return;
		}
		LaunchEntry launchEntry = _plan.Find(entryKey);
		if (launchEntry != null && launchEntry.CanToggle)
		{
			_selected = launchEntry;
			RecordUndo();
			FolderToggleMemory.Clear();
			ShowFeedback(_plan.SetEnabled(launchEntry, !launchEntry.Enabled));
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild();
		}
	}

	private static void ToggleSelected()
	{
		if (_plan != null && _selected != null && !_pumpActive)
		{
			RecordUndo();
			FolderToggleMemory.Clear();
			ShowFeedback(_plan.SetEnabled(_selected, !_selected.Enabled));
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild();
		}
	}

	private static void MoveSelected(int delta)
	{
		if (UpdateAnnouncement.IsOpen || _plan == null || _selected == null || _pumpActive)
		{
			return;
		}
		if (!_selected.CanMove)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("plan.move.managerMustBeFirst")));
			return;
		}
		HashSet<LaunchEntry> visibleEntries = TreeBindings.Select((TreeBinding binding) => binding.Entry).ToHashSet();
		LaunchEntry selected = _selected;
		List<LaunchEntry> list = _plan.Entries.Where((LaunchEntry entry) => visibleEntries.Contains(entry) && ((!_groupListView) ? (!LaunchPlan.KeepDisabledInPlace || entry.Enabled == selected.Enabled) : ShareMoveRange(entry, selected))).ToList();
		int num = list.IndexOf(_selected);
		if (num < 0)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("mods.move.selectionFilteredOut")));
			return;
		}
		if (_groupListView)
		{
			TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding binding) => binding.Entry == selected);
			if ((object)treeBinding != null && InFoldedFolder(treeBinding.Item) && RevealFolder(selected.Group))
			{
				QueueRebuild(scrollToSelection: true);
			}
		}
		int num2 = num + delta;
		if (num2 < 0 || num2 >= list.Count)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get(MoveLimitKey(selected, delta, visibleEntries))));
		}
		else if (ConfirmMoveAgainstDeclaredPriority(_selected, delegate
		{
			MoveSelected(delta);
		}))
		{
			RecordUndo("move:" + _selected.Key);
			ShowFeedback(_plan.MoveTo(_selected, list[num2], delta > 0));
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
		}
	}

	private static string MoveLimitKey(LaunchEntry selected, int delta, IReadOnlySet<LaunchEntry> visible)
	{
		if (!_groupListView)
		{
			if (delta >= 0)
			{
				return "mods.move.alreadyBottom";
			}
			return "mods.move.alreadyTop";
		}
		int at = _plan?.IndexOf(selected) ?? (-1);
		bool flag = !MixedStatesInterleave() && (_plan?.Entries ?? new List<LaunchEntry>()).Where((LaunchEntry entry, int index) => (delta >= 0) ? (index > at) : (index < at)).Any((LaunchEntry entry) => visible.Contains(entry) && SameGroup(entry.Group, selected.Group) && entry.Enabled != selected.Enabled);
		if (delta < 0)
		{
			if (flag)
			{
				return "mods.move.folderFirstOff";
			}
			return "mods.move.alreadyFolderTop";
		}
		if (flag)
		{
			return "mods.move.folderLastOn";
		}
		return "mods.move.alreadyFolderBottom";
	}

	private static void BindRepeatingMoveButton(Button button, int direction)
	{
		((BaseButton)button).ButtonDown += delegate
		{
			BeginMoveRepeat(button, direction);
		};
		((BaseButton)button).ButtonUp += delegate
		{
			EndMoveRepeat(button);
		};
		((Control)button).MouseExited += delegate
		{
			CancelMoveRepeat(button);
		};
		((BaseButton)button).Pressed += delegate
		{
			if (_movePressHandledButton == button)
			{
				_movePressHandledButton = null;
			}
			else
			{
				MoveSelected(direction);
			}
		};
	}

	private static void BeginMoveRepeat(Button button, int direction)
	{
		CancelMoveRepeat();
		_moveRepeatButton = button;
		_movePressHandledButton = button;
		_moveRepeatDirection = direction;
		MoveSelected(direction);
		if (!((BaseButton)button).Disabled && _moveRepeatTimer != null && GodotObject.IsInstanceValid((GodotObject)(object)_moveRepeatTimer))
		{
			_moveRepeatTimer.WaitTime = 0.35;
			_moveRepeatTimer.Start(-1.0);
		}
	}

	private static void RepeatSelectedMove()
	{
		Button moveRepeatButton = _moveRepeatButton;
		if (moveRepeatButton == null || !GodotObject.IsInstanceValid((GodotObject)(object)moveRepeatButton) || ((BaseButton)moveRepeatButton).Disabled || _pumpActive)
		{
			CancelMoveRepeat();
			return;
		}
		MoveSelected(_moveRepeatDirection);
		if (((BaseButton)moveRepeatButton).Disabled || _moveRepeatTimer == null || !GodotObject.IsInstanceValid((GodotObject)(object)_moveRepeatTimer))
		{
			CancelMoveRepeat();
			return;
		}
		_moveRepeatTimer.WaitTime = 0.075;
		_moveRepeatTimer.Start(-1.0);
	}

	private static void EndMoveRepeat(Button button)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (_moveRepeatButton != button)
		{
			return;
		}
		StopMoveRepeatTimer();
		_moveRepeatButton = null;
		_moveRepeatDirection = 0;
		Callable val = Callable.From((Action)delegate
		{
			if (_movePressHandledButton == button)
			{
				_movePressHandledButton = null;
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static void CancelMoveRepeat(Button? button = null)
	{
		if (button == null || _moveRepeatButton == button)
		{
			StopMoveRepeatTimer();
			_moveRepeatButton = null;
			_movePressHandledButton = null;
			_moveRepeatDirection = 0;
		}
	}

	private static void StopMoveRepeatTimer()
	{
		if (_moveRepeatTimer != null && GodotObject.IsInstanceValid((GodotObject)(object)_moveRepeatTimer))
		{
			_moveRepeatTimer.Stop();
		}
	}

	private static void SetAllEnabled(bool enabled)
	{
		if (_plan != null && !_pumpActive)
		{
			List<LaunchEntry> list = (from binding in TreeBindings
				where !_groupListView || !InFoldedFolder(binding.Item)
				select binding.Entry).ToList();
			if (list.Count != 0)
			{
				RecordUndo();
				FolderToggleMemory.Clear();
				ShowFeedback(_plan.SetEnabledForAll(list, enabled));
				UpdateDetails();
				UpdatePlanSummary();
				QueueRebuild();
			}
		}
	}

	private static bool ConfirmMoveAgainstDeclaredPriority(LaunchEntry entry, Action retry)
	{
		if (!entry.PriorityIsBinding || ModPriorityPrompt.IsOpen)
		{
			return true;
		}
		ModLoadPriority priority = entry.DeclaredPriority.Value;
		CancelMoveRepeat();
		ModPriorityPrompt.Show(entry, priority, delegate
		{
			if (MutateOrganization(delegate
			{
				_organization.SetIgnorePriority(entry.Id, ignore: true);
			}))
			{
				GD.Print("[ModLaunchManager] " + MlmLocManager.Get("priority.prompt.ignored", ("name", entry.Name), ("tier", ModLoadPriorities.Label(priority))));
				UpdateDetails();
				retry();
			}
		});
		return false;
	}

	private static bool HasActiveListFilter()
	{
		if (_filter == ModFilter.All && _groupFilter == null)
		{
			LineEdit? search = _search;
			return !string.IsNullOrWhiteSpace((search != null) ? search.Text : null);
		}
		return true;
	}

	private static void OnTreeDragActiveChanged(bool active)
	{
		if (_dragPreviewActive == active)
		{
			return;
		}
		_dragPreviewActive = active;
		UpdateStickyHeader();
		if (active)
		{
			TuckFab();
		}
		else
		{
			Timer? fabRest = _fabRest;
			if (fabRest != null)
			{
				fabRest.Start(-1.0);
			}
		}
		CancelInlineRename();
		if (active)
		{
			_statusBeforeDrag = ((_feedback == null) ? (((string, string, string)?)null) : new(string, string, string)?((_feedback.Text, StringName.op_Implicit(((Control)_feedback).ThemeTypeVariation), ((Control)_feedback).TooltipText)));
		}
		else
		{
			(string, string, string)? statusBeforeDrag = _statusBeforeDrag;
			if (statusBeforeDrag.HasValue)
			{
				(string, string, string) valueOrDefault = statusBeforeDrag.GetValueOrDefault();
				if (_feedback != null)
				{
					_feedback.Text = valueOrDefault.Item1;
					((Control)_feedback).ThemeTypeVariation = StringName.op_Implicit(valueOrDefault.Item2);
					((Control)_feedback).TooltipText = valueOrDefault.Item3;
					_statusBeforeDrag = null;
				}
			}
		}
		if (HasActiveListFilter())
		{
			if (active)
			{
				ShowFeedback(PlanResult.Ok(MlmLocManager.Get("mods.drag.showingAll")));
			}
			QueueRebuild(scrollToSelection: true);
		}
	}

	private static bool CanStartTreeDrag(string entryKey)
	{
		if (IsFolderKey(entryKey))
		{
			if (!_pumpActive && _groupListView)
			{
				string text = FolderGroup(entryKey);
				if (text != null)
				{
					return text.Length > 0;
				}
				return false;
			}
			return false;
		}
		LaunchEntry launchEntry = _plan?.Find(entryKey);
		if (!_pumpActive)
		{
			return launchEntry?.CanMove ?? false;
		}
		return false;
	}

	private static bool CanDropTreeEntry(string draggedKey, string targetKey)
	{
		if (_plan == null || _pumpActive)
		{
			return false;
		}
		if (IsFolderKey(draggedKey))
		{
			return CanDropFolder(draggedKey, targetKey);
		}
		LaunchEntry launchEntry = _plan.Find(draggedKey);
		string text = FolderGroup(targetKey);
		if (text != null)
		{
			if (_groupListView && launchEntry != null && launchEntry.CanMove)
			{
				return !SameGroup(launchEntry.Group, text);
			}
			return false;
		}
		LaunchEntry launchEntry2 = _plan.Find(targetKey);
		if (launchEntry != null && launchEntry.CanMove && launchEntry2 != null && launchEntry != launchEntry2)
		{
			if (!MixedStatesInterleave() && launchEntry.Enabled != launchEntry2.Enabled)
			{
				return JoinsFolderOnly(launchEntry, launchEntry2);
			}
			return true;
		}
		return false;
	}

	private static bool JoinsFolderOnly(LaunchEntry dragged, LaunchEntry target)
	{
		if (_groupListView && !MixedStatesInterleave() && dragged.Enabled != target.Enabled)
		{
			return !SameGroup(dragged.Group, target.Group);
		}
		return false;
	}

	private static TreeDropPlacement TreeDropPlacementFor(string draggedKey, string targetKey, float fraction)
	{
		if (_plan == null || !CanDropTreeEntry(draggedKey, targetKey))
		{
			return TreeDropPlacement.None;
		}
		if (IsFolderKey(draggedKey))
		{
			string text = FolderGroup(targetKey);
			bool flag = text != null && text.Length > 0;
			if (!flag || !(fraction < 0.3f))
			{
				if (!flag || !(fraction > 0.7f))
				{
					return TreeDropPlacement.Into;
				}
				return TreeDropPlacement.After;
			}
			return TreeDropPlacement.Before;
		}
		if (!IsFolderKey(targetKey))
		{
			LaunchEntry launchEntry = _plan.Find(draggedKey);
			if (launchEntry != null)
			{
				LaunchEntry launchEntry2 = _plan.Find(targetKey);
				if (launchEntry2 != null && JoinsFolderOnly(launchEntry, launchEntry2))
				{
					goto IL_007c;
				}
			}
			if (!(fraction < 0.5f))
			{
				return TreeDropPlacement.After;
			}
			return TreeDropPlacement.Before;
		}
		goto IL_007c;
		IL_007c:
		return TreeDropPlacement.Into;
	}

	private static TreeDropPlacement DefaultDropPlacement(string draggedKey, string targetKey)
	{
		if (IsFolderKey(draggedKey))
		{
			string group = FolderGroup(draggedKey);
			if (group != null)
			{
				string target = FolderGroup(targetKey);
				if (target != null && target.Length > 0)
				{
					List<string> list = KnownGroupNames();
					if (list.FindIndex((string name) => SameGroup(name, group)) >= list.FindIndex((string name) => SameGroup(name, target)))
					{
						return TreeDropPlacement.Before;
					}
					return TreeDropPlacement.After;
				}
			}
			return TreeDropPlacement.Into;
		}
		if (!IsFolderKey(targetKey))
		{
			LaunchEntry launchEntry = _plan?.Find(draggedKey);
			if (launchEntry != null)
			{
				LaunchEntry launchEntry2 = _plan.Find(targetKey);
				if (launchEntry2 != null && !JoinsFolderOnly(launchEntry, launchEntry2))
				{
					if (_plan.IndexOf(launchEntry) >= _plan.IndexOf(launchEntry2))
					{
						return TreeDropPlacement.Before;
					}
					return TreeDropPlacement.After;
				}
			}
		}
		return TreeDropPlacement.Into;
	}

	private static void DropTreeEntry(string draggedKey, string targetKey)
	{
		DropTreeEntryAt(draggedKey, targetKey, DefaultDropPlacement(draggedKey, targetKey));
	}

	private static void DropTreeEntryAt(string draggedKey, string targetKey, TreeDropPlacement placement)
	{
		if (_plan == null || _pumpActive || placement == TreeDropPlacement.None)
		{
			return;
		}
		if (IsFolderKey(draggedKey))
		{
			DropFolderAt(draggedKey, targetKey, placement);
			return;
		}
		LaunchEntry launchEntry = _plan.Find(draggedKey);
		if (launchEntry != null)
		{
			string text = FolderGroup(targetKey);
			if (text != null)
			{
				_selected = launchEntry;
				AssignGroup(new _003C_003Ez__ReadOnlySingleElementList<LaunchEntry>(launchEntry), text, reveal: false);
				QueueRebuild(scrollToSelection: true);
				return;
			}
		}
		LaunchEntry launchEntry2 = _plan.Find(targetKey);
		if (launchEntry == null || launchEntry2 == null)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("plan.move.targetMissing")));
			return;
		}
		if (JoinsFolderOnly(launchEntry, launchEntry2))
		{
			_selected = launchEntry;
			AssignGroup(new _003C_003Ez__ReadOnlySingleElementList<LaunchEntry>(launchEntry), launchEntry2.Group);
			QueueRebuild(scrollToSelection: true);
			return;
		}
		_selected = launchEntry;
		if (ConfirmMoveAgainstDeclaredPriority(launchEntry, delegate
		{
			DropTreeEntryAt(draggedKey, targetKey, placement);
		}))
		{
			bool insertAfter = placement == TreeDropPlacement.After || (placement == TreeDropPlacement.Into && _plan.IndexOf(launchEntry) < _plan.IndexOf(launchEntry2));
			RecordUndo();
			PlanResult result = _plan.MoveTo(launchEntry, launchEntry2, insertAfter);
			ShowFeedback(result);
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
			if (_groupListView && result.Success && !SameGroup(launchEntry.Group, launchEntry2.Group))
			{
				AssignGroup(new _003C_003Ez__ReadOnlySingleElementList<LaunchEntry>(launchEntry), launchEntry2.Group);
			}
		}
	}

	private static void OnTreeDropTargetChanged(string draggedKey, string targetKey, TreeDropPlacement placement)
	{
		if (!_dragPreviewActive || _plan == null)
		{
			return;
		}
		string text = null;
		if (targetKey.Length == 0 || placement == TreeDropPlacement.None)
		{
			text = MlmLocManager.Get(IsFolderKey(draggedKey) ? "groups.drag.hint.none" : "mods.drag.hint.none");
		}
		else
		{
			string text2 = FolderGroup(draggedKey);
			if (text2 != null)
			{
				string text3 = FolderOfRow(targetKey) ?? string.Empty;
				text = placement switch
				{
					TreeDropPlacement.Before => MlmLocManager.Get("groups.drag.hint.before", ("name", text2), ("target", GroupPath(text3))), 
					TreeDropPlacement.After => MlmLocManager.Get("groups.drag.hint.after", ("name", text2), ("target", GroupPath(text3))), 
					_ => (text3.Length != 0) ? MlmLocManager.Get("groups.drag.hint.nest", ("name", text2), ("parent", GroupPath(text3))) : MlmLocManager.Get("groups.drag.hint.topLevel", ("name", text2)), 
				};
			}
			else
			{
				LaunchEntry launchEntry = _plan.Find(draggedKey);
				if (launchEntry != null)
				{
					string text4 = FolderOfRow(targetKey) ?? string.Empty;
					bool flag = _groupListView && !SameGroup(launchEntry.Group, text4);
					string item = _plan.Find(targetKey)?.DisplayName ?? string.Empty;
					text = placement switch
					{
						TreeDropPlacement.Before => (!flag) ? MlmLocManager.Get("mods.drag.hint.before", ("name", item)) : MlmLocManager.Get("mods.drag.hint.beforeJoin", ("name", item), ("group", GroupLabel(text4))), 
						TreeDropPlacement.After => (!flag) ? MlmLocManager.Get("mods.drag.hint.after", ("name", item)) : MlmLocManager.Get("mods.drag.hint.afterJoin", ("name", item), ("group", GroupLabel(text4))), 
						_ => MlmLocManager.Get("mods.drag.hint.into", ("group", GroupLabel(text4))), 
					};
				}
			}
		}
		if (text != null)
		{
			SetStatus(text, "MutedLabel", "↕");
		}
		static string GroupLabel(string group)
		{
			if (group.Length != 0)
			{
				return GroupPath(group);
			}
			return MlmLocManager.Get("groups.option.ungrouped");
		}
	}

	private static void OnTreeHeadingDwell(string key)
	{
		if (!_groupListView)
		{
			return;
		}
		string text = FolderGroup(key);
		if (text != null)
		{
			TreeItem val = FindFolderHeader(key);
			if (val != null && val.Collapsed)
			{
				SetFolderCollapsed(val, text, collapsed: false);
			}
		}
	}

	private static void ResetPlan()
	{
		if (!UpdateAnnouncement.IsOpen && _plan != null && !_pumpActive)
		{
			RecordUndo();
			FolderToggleMemory.Clear();
			_plan.Reset();
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("plan.reset.success")));
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
		}
	}

	private static void EnableRandomMod()
	{
		if (_plan == null || _pumpActive || _pendingCanonicalAction != 0)
		{
			return;
		}
		RecordUndo();
		LaunchEntry selected;
		PlanResult result = _plan.EnableRandom(out selected);
		if (selected != null)
		{
			_controllerMoveKey = null;
			_selected = selected;
			_organizationEditingKey = null;
			ClearFilters();
			if (_groupListView)
			{
				RevealFolder(selected.Group);
			}
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
		}
		ShowFeedback(result);
	}

	private static void AutoSortPlan()
	{
		if (_plan != null && !_pumpActive)
		{
			RecordUndo();
			ShowFeedback(_plan.AutoSortDependencies());
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
		}
	}

	private static bool OnLocalModsChanged(LocalModScan scan)
	{
		if (_plan == null || _pumpActive)
		{
			return false;
		}
		try
		{
			List<Mod> arrivals = scan.Added.Where((Mod mod) => !_plan.Entries.Any((LaunchEntry entry) => PhysicalModIdentity.Same(entry.Mod, mod))).ToList();
			LocalModPairing<LaunchEntry, Mod> localModPairing = LocalModChanges.Pair(MissingLocalEntries(scan.VanishedFiles), (LaunchEntry entry) => entry.Mod.manifest?.id, arrivals, (Mod mod) => mod.manifest?.id);
			List<string> list = new List<string>();
			if (localModPairing.Moves.Count > 0)
			{
				foreach (var (entry2, arrival) in localModPairing.Moves)
				{
					RelocateLocalEntry(entry2, arrival);
				}
				list.Add(MlmLocManager.Get("watcher.modsMoved", ("count", localModPairing.Moves.Count), ("names", NameList(localModPairing.Moves.Select(((LaunchEntry Gone, Mod Arrival) move) => move.Gone)))));
			}
			IReadOnlyList<LaunchEntry> readOnlyList = RemoveLocalEntries(localModPairing.Removed);
			if (readOnlyList.Count > 0)
			{
				list.Add(MlmLocManager.Get("watcher.modsRemoved", ("count", readOnlyList.Count), ("names", NameList(readOnlyList))));
			}
			PlanResult? planResult = AddDiscoveredLocalMods(localModPairing.Added);
			if (list.Count == 0 && !planResult.HasValue)
			{
				return true;
			}
			if (readOnlyList.Count > 0 || planResult.HasValue)
			{
				Defer(RebuildGroupRow);
			}
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(planResult.HasValue);
			if (planResult.HasValue)
			{
				list.Add(planResult.GetValueOrDefault().Message);
			}
			string message = list.Aggregate((string first, string second) => MlmLocManager.Get("common.twoSentences", ("first", first), ("second", second)));
			ShowFeedback((planResult.HasValue && planResult.GetValueOrDefault().Caution) ? PlanResult.Warning(message) : PlanResult.Ok(message));
			return true;
		}
		catch (Exception ex)
		{
			UpdatePlanSummary();
			QueueRebuild();
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("watcher.listUpdateFailed", ("error", ex.Message))));
			GD.PushWarning($"{"[ModLaunchManager]"} following the local mods folder failed: {ex}");
			return false;
		}
	}

	private static List<LaunchEntry> MissingLocalEntries(IReadOnlyList<string> vanishedFiles)
	{
		if (_plan == null || vanishedFiles.Count == 0)
		{
			return new List<LaunchEntry>();
		}
		HashSet<string> folders = LocalModChanges.FoldersOf(vanishedFiles);
		return _plan.Entries.Where(delegate(LaunchEntry entry)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Invalid comparison between Unknown and I4
			if ((int)entry.Mod.modSource == 1 && !entry.IsManager && (int)entry.Mod.state != 1)
			{
				string text = entry.Mod.manifest?.id;
				if (text != null && text.Length > 0 && LocalModChanges.InFolders(entry.Mod.path, folders))
				{
					return !LocalModChanges.ManifestRemains(entry.Mod.path, text);
				}
			}
			return false;
		}).ToList();
	}

	private static void RelocateLocalEntry(LaunchEntry entry, Mod arrival)
	{
		string path = entry.Mod.path;
		string key = entry.Key;
		ModManifestReader.Adopt(entry.Mod, arrival);
		_plan.Relocate(entry, arrival.path);
		FollowEntryKey(key, entry);
		GD.Print($"{"[ModLaunchManager]"} local mod {entry.Id} moved from {path} to {entry.Mod.path}; kept its entry.");
	}

	private static void FollowEntryKey(string previousKey, LaunchEntry entry)
	{
		if (_organizationEditingKey == previousKey)
		{
			_organizationEditingKey = entry.Key;
		}
		if (_descriptionEditingKey == previousKey)
		{
			_descriptionEditingKey = entry.Key;
		}
		if (_controllerMoveKey == previousKey)
		{
			_controllerMoveKey = entry.Key;
		}
	}

	private static IReadOnlyList<LaunchEntry> RemoveLocalEntries(IReadOnlyList<LaunchEntry> gone)
	{
		if (_plan == null || gone.Count == 0)
		{
			return Array.Empty<LaunchEntry>();
		}
		int value = ((_selected == null) ? (-1) : _plan.IndexOf(_selected));
		IReadOnlyList<LaunchEntry> readOnlyList = _plan.RemoveEntries(gone);
		foreach (LaunchEntry item in readOnlyList)
		{
			DeferredModLoader.RemoveFromLiveList(item.Mod);
			if (_controllerMoveKey == item.Key)
			{
				EndControllerMove();
			}
		}
		_canonicalDormant = CanonicalLaunchStore.RememberRemoved(_canonicalDormant, readOnlyList);
		if (_selected != null && readOnlyList.Contains<LaunchEntry>(_selected) && _plan.Entries.Count > 0)
		{
			_selected = _plan.Entries[Math.Clamp(value, 0, _plan.Entries.Count - 1)];
		}
		if (readOnlyList.Count > 0)
		{
			GD.Print("[ModLaunchManager] local mod files are gone; removed " + string.Join(", ", readOnlyList.Select((LaunchEntry entry) => entry.Id + " (" + entry.Mod.path + ")")) + " from the list.");
		}
		return readOnlyList;
	}

	private static PlanResult? AddDiscoveredLocalMods(IReadOnlyList<Mod> parsedMods)
	{
		if (_plan == null || parsedMods.Count == 0)
		{
			return null;
		}
		IReadOnlyList<Mod> mods = DeferredModLoader.RegisterDiscoveredLocalMods(parsedMods);
		HashSet<string> before = _plan.ConflictPairKeys();
		IReadOnlyList<LaunchEntry> readOnlyList = _plan.AddDiscoveredMods(mods);
		if (readOnlyList.Count == 0)
		{
			return null;
		}
		ApplyOrganization(readOnlyList);
		_plan.ApplyDeclaredPriorities(rebaseline: true, out var _);
		_selected = readOnlyList[0];
		_organizationEditingKey = null;
		if (_search != null && !MatchesSearch(_selected, _search.Text.Trim()))
		{
			_search.Text = string.Empty;
		}
		if (!MatchesFilter(_selected))
		{
			_filter = ModFilter.All;
			if (FilterChips.Count > 0)
			{
				((BaseButton)FilterChips[0]).ButtonPressed = true;
			}
			_groupFilter = null;
		}
		GD.Print($"{"[ModLaunchManager]"} hot-discovered {readOnlyList.Count} local mod(s): " + string.Join(", ", readOnlyList.Select((LaunchEntry entry) => entry.Id)));
		return _plan.WarnAboutNewConflicts(PlanResult.Ok(MlmLocManager.Get("watcher.newModsAdded", ("count", readOnlyList.Count))), before);
	}

	private static string NameList(IEnumerable<LaunchEntry> entries)
	{
		return string.Join(MlmLocManager.Get("common.listSeparator"), entries.Select((LaunchEntry entry) => entry.DisplayName));
	}

	private static void OnLocalModWatcherError(string message)
	{
		ShowFeedback(PlanResult.Error(message));
	}

	private static void UpdatePlanSummary()
	{
		if (_plan != null)
		{
			if (_discoveredChip != null)
			{
				_discoveredChip.Text = MlmLocManager.Get("launcher.summary.discovered", ("count", _plan.Entries.Count));
			}
			if (_stateChip != null)
			{
				_stateChip.Text = MlmLocManager.Get("launcher.summary.states", ("enabled", _plan.EnabledCount), ("disabled", _plan.DisabledCount));
			}
			if (_reset != null)
			{
				((BaseButton)_reset).Disabled = !_plan.HasChanges || _pumpActive;
			}
			if (_start != null && !_pumpActive && _pendingCanonicalAction == PendingCanonicalAction.None)
			{
				_start.Text = (_plan.HasChanges ? MlmLocManager.Get("launcher.action.saveChangesAndLaunch") : MlmLocManager.Get("launcher.action.launchCurrentSetup"));
			}
			RefreshProfileState();
			FriendModSync.Publish(_plan, CurrentGameVersion());
			FriendModSyncDialog.Update();
			UpdateControllerPrompts();
		}
	}

	private static void ScrollSelectionIntoView()
	{
		TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding candidate) => _selected != null && candidate.Entry == _selected);
		if (treeBinding != null && !InFoldedFolder(treeBinding.Item))
		{
			ModReorderTree? modTree = _modTree;
			if (modTree != null)
			{
				((Tree)modTree).ScrollToItem(treeBinding.Item, true);
			}
		}
	}

	private static void ShowFeedback(PlanResult result)
	{
		SetStatus(result.Message, (!result.Success) ? "DangerLabel" : (result.Caution ? "WarningLabel" : "SuccessLabel"), (!result.Success) ? "✕" : (result.Caution ? "!" : "✓"));
	}

	private static void SetStatus(string message, string variation, string glyph)
	{
		if (_feedback != null)
		{
			_feedback.Text = glyph + "  " + message;
			((Control)_feedback).ThemeTypeVariation = StringName.op_Implicit(variation);
			((Control)_feedback).TooltipText = message;
		}
	}

	private static Control BuildDetails()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Expected O, but got Unknown
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Expected O, but got Unknown
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Expected O, but got Unknown
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Expected O, but got Unknown
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Expected O, but got Unknown
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_041e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0434: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Expected O, but got Unknown
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0479: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Expected O, but got Unknown
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Expected O, but got Unknown
		//IL_0591: Unknown result type (might be due to invalid IL or missing references)
		//IL_0596: Unknown result type (might be due to invalid IL or missing references)
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Expected O, but got Unknown
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0602: Expected O, but got Unknown
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_0625: Unknown result type (might be due to invalid IL or missing references)
		//IL_062f: Expected O, but got Unknown
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_070c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0712: Expected O, but got Unknown
		//IL_0730: Unknown result type (might be due to invalid IL or missing references)
		//IL_0735: Unknown result type (might be due to invalid IL or missing references)
		//IL_073b: Expected O, but got Unknown
		//IL_0759: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0764: Expected O, but got Unknown
		//IL_0860: Unknown result type (might be due to invalid IL or missing references)
		//IL_0865: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Expected O, but got Unknown
		//IL_08a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d7: Expected O, but got Unknown
		//IL_09e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f1: Expected O, but got Unknown
		//IL_0a19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a23: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a2f: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		ScrollContainer val2 = new ScrollContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)3,
			HorizontalScrollMode = (ScrollMode)0,
			VerticalScrollMode = (ScrollMode)1
		};
		UiKit.FollowFocus(val2);
		_detailScroll = val2;
		VBoxContainer val3 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 22);
		_detailName = LocalizedKeyLabel("details.noSelection", "SectionLabel");
		_detailName.AutowrapMode = (AutowrapMode)3;
		_detailMetaSignature = string.Empty;
		_detailMeta = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_detailMeta).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 16);
		((Control)_detailMeta).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 2);
		_detailBadges = new HFlowContainer();
		((Control)_detailBadges).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 6);
		((Control)_detailBadges).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 6);
		VBoxContainer val4 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		VBoxContainer val5 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
		((Node)val5).AddChild((Node)(object)_detailName, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)_detailMeta, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_detailBadges, false, (InternalMode)0);
		_detailDescription = new RichTextLabel
		{
			FocusMode = (FocusModeEnum)2,
			SizeFlagsHorizontal = (SizeFlags)3,
			FitContent = true,
			ScrollActive = false,
			SelectionEnabled = true,
			MouseFilter = (MouseFilterEnum)0
		};
		((Control)_detailDescription).AddThemeConstantOverride(StringName.op_Implicit("line_separation"), 3);
		BindKeyTooltip((Control)(object)_detailDescription, "details.description.editHint");
		((GodotObject)_detailDescription).SetMeta(StringName.op_Implicit("mlm_controller_target"), Variant.op_Implicit(true));
		RichTextLabel description = _detailDescription;
		BindPalette((GodotObject)(object)description, delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			StyleBoxFlat val6 = UiKit.Box(UiKit.Clear, UiKit.Palette.FocusRing, UiKit.Palette.IsLiquidGlass ? 9 : 5, 2, 0, 0);
			val6.SetExpandMarginAll(5f);
			((Control)description).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)val6);
		});
		RichTextLabel? detailDescription = _detailDescription;
		object obj = _003C_003EO._003C71_003E__OnDescriptionGuiInput;
		if (obj == null)
		{
			GuiInputEventHandler val7 = OnDescriptionGuiInput;
			_003C_003EO._003C71_003E__OnDescriptionGuiInput = val7;
			obj = (object)val7;
		}
		((Control)detailDescription).GuiInput += (GuiInputEventHandler)obj;
		_detailDescriptionEditor = new TextEdit
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 132f),
			WrapMode = (LineWrappingMode)1,
			ScrollFitContentHeight = true
		};
		BindKeyTooltip((Control)(object)_detailDescriptionEditor, "details.description.editorTooltip");
		_detailDescriptionActions = new HBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_detailDescriptionActions).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val8 = LocalizedKeyLabel("details.description.clearHint", "DimLabel");
		((Control)val8).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val8).SizeFlagsVertical = (SizeFlags)4;
		val8.AutowrapMode = (AutowrapMode)3;
		Button val9 = LocalizedKeyButton("common.action.cancel", "MiniButton", 64f, 30f);
		((BaseButton)val9).Pressed += CancelDescriptionEdit;
		Button val10 = LocalizedKeyButton("common.action.save", "MiniButton", 64f, 30f);
		((BaseButton)val10).Pressed += SaveDescriptionEdit;
		((Node)_detailDescriptionActions).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)_detailDescriptionActions).AddChild((Node)(object)val9, false, (InternalMode)0);
		((Node)_detailDescriptionActions).AddChild((Node)(object)val10, false, (InternalMode)0);
		GridContainer val11 = FactGrid();
		_detailContent = AddFact(val11, "details.fact.content");
		_detailGameVersion = AddFact(val11, "details.fact.minimumGameVersion");
		_detailInstallTime = AddFact(val11, "details.fact.installTime", out _detailInstallTimeFact);
		BindKeyTooltip(_detailInstallTimeFact, "details.fact.installTime.tooltip");
		((CanvasItem)_detailInstallTimeFact).Visible = false;
		_detailPriority = AddFact(val11, "details.fact.loadPriority", out _detailPriorityFact);
		((CanvasItem)_detailPriorityFact).Visible = false;
		_detailDependencies = new RichTextLabel
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			BbcodeEnabled = true,
			FitContent = true,
			ScrollActive = false,
			SelectionEnabled = true
		};
		_detailIncompatibilities = DetailSection("details.incompatibilities.caption", visible: false);
		_detailIncompatibilityText = new RichTextLabel
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			BbcodeEnabled = true,
			FitContent = true,
			ScrollActive = false,
			SelectionEnabled = true
		};
		((Node)_detailIncompatibilities).AddChild((Node)(object)_detailIncompatibilityText, false, (InternalMode)0);
		_detailToggle = LocalizedKeyButton("details.action.toggleDefault");
		LiquidGlass.Decorate((Control)(object)_detailToggle, primary: true, surface: false, null);
		((BaseButton)_detailToggle).Pressed += ToggleSelected;
		_detailOpenFolder = LocalizedKeyButton("details.action.openFolder", "SecondaryButton", 122f);
		((BaseButton)_detailOpenFolder).Pressed += OpenSelectedModFolder;
		HBoxContainer val12 = new HBoxContainer();
		((Control)val12).AddThemeConstantOverride(StringName.op_Implicit("separation"), 9);
		((Control)_detailOpenFolder).SizeFlagsHorizontal = (SizeFlags)0;
		((Control)_detailToggle).SizeFlagsHorizontal = (SizeFlags)3;
		((Node)val12).AddChild((Node)(object)_detailOpenFolder, false, (InternalMode)0);
		((Node)val12).AddChild((Node)(object)_detailToggle, false, (InternalMode)0);
		VBoxContainer val13 = DetailSection("organization.caption");
		GridContainer val14 = new GridContainer
		{
			Columns = 2,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val14).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 10);
		((Control)val14).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 7);
		_detailAlias = new LineEdit
		{
			ClearButtonEnabled = true,
			MaxLength = 80,
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 34f)
		};
		BindKeyPlaceholder(_detailAlias, "organization.alias.placeholder");
		BindKeyTooltip((Control)(object)_detailAlias, "organization.alias.tooltip");
		HBoxContainer val15 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val15).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		((Node)val15).AddChild((Node)(object)_detailAlias, false, (InternalMode)0);
		_detailAliasFill = LocalizedKeyButton("organization.alias.fillWorkshop", "SecondaryButton", 0f, 34f);
		BindKeyTooltip((Control)(object)_detailAliasFill, "organization.alias.workshopTooltip");
		((BaseButton)_detailAliasFill).Pressed += FillWorkshopAlias;
		((Node)val15).AddChild((Node)(object)_detailAliasFill, false, (InternalMode)0);
		_detailGroupSelector = UiKit.Dropdown();
		((Control)_detailGroupSelector).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)_detailGroupSelector).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_detailGroupSelector, "organization.group.tooltip");
		OptionButton? detailGroupSelector = _detailGroupSelector;
		object obj2 = _003C_003EO._003C77_003E__OnDetailGroupSelected;
		if (obj2 == null)
		{
			ItemSelectedEventHandler val16 = OnDetailGroupSelected;
			_003C_003EO._003C77_003E__OnDetailGroupSelected = val16;
			obj2 = (object)val16;
		}
		detailGroupSelector.ItemSelected += (ItemSelectedEventHandler)obj2;
		LineEdit? detailAlias = _detailAlias;
		object obj3 = _003C_003Ec._003C_003E9__407_1;
		if (obj3 == null)
		{
			TextChangedEventHandler val17 = delegate
			{
				UpdateOrganizationSaveState();
			};
			_003C_003Ec._003C_003E9__407_1 = val17;
			obj3 = (object)val17;
		}
		detailAlias.TextChanged += (TextChangedEventHandler)obj3;
		LineEdit? detailAlias2 = _detailAlias;
		object obj4 = _003C_003Ec._003C_003E9__407_2;
		if (obj4 == null)
		{
			TextSubmittedEventHandler val18 = delegate
			{
				SaveSelectedOrganization();
			};
			_003C_003Ec._003C_003E9__407_2 = val18;
			obj4 = (object)val18;
		}
		detailAlias2.TextSubmitted += (TextSubmittedEventHandler)obj4;
		_detailOrganizationSave = LocalizedKeyButton("organization.action.saveAlias", "SecondaryButton", 0f, 34f);
		BindKeyTooltip((Control)(object)_detailOrganizationSave, "organization.action.saveAlias.tooltip");
		((BaseButton)_detailOrganizationSave).Pressed += delegate
		{
			SaveSelectedOrganization();
		};
		((Node)val15).AddChild((Node)(object)_detailOrganizationSave, false, (InternalMode)0);
		((Node)val14).AddChild((Node)(object)LocalizedKeyLabel("organization.alias.label", "DimLabel"), false, (InternalMode)0);
		((Node)val14).AddChild((Node)(object)val15, false, (InternalMode)0);
		((Node)val14).AddChild((Node)(object)LocalizedKeyLabel("organization.group.label", "DimLabel"), false, (InternalMode)0);
		((Node)val14).AddChild((Node)(object)_detailGroupSelector, false, (InternalMode)0);
		((Node)val13).AddChild((Node)(object)val14, false, (InternalMode)0);
		_detailOptions = DetailSection("options.caption", visible: false);
		Label val19 = LocalizedKeyLabel("options.hint", "DimLabel");
		val19.AutowrapMode = (AutowrapMode)3;
		((Node)_detailOptions).AddChild((Node)(object)val19, false, (InternalMode)0);
		_detailOptionRows = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_detailOptionRows).AddThemeConstantOverride(StringName.op_Implicit("separation"), 6);
		((Node)_detailOptions).AddChild((Node)(object)_detailOptionRows, false, (InternalMode)0);
		_detailWarnings = DetailSection("details.warnings.caption", visible: false);
		_detailWarningText = new RichTextLabel
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			BbcodeEnabled = true,
			FitContent = true,
			ScrollActive = false,
			SelectionEnabled = true
		};
		((Node)_detailWarnings).AddChild((Node)(object)_detailWarningText, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)BuildWorkshopTags(), false, (InternalMode)0);
		VBoxContainer val20 = DetailSection("details.description.caption");
		((Node)val20).AddChild((Node)(object)_detailDescription, false, (InternalMode)0);
		((Node)val20).AddChild((Node)(object)_detailDescriptionEditor, false, (InternalMode)0);
		((Node)val20).AddChild((Node)(object)_detailDescriptionActions, false, (InternalMode)0);
		VBoxContainer val21 = DetailSection("details.facts.caption");
		((Node)val21).AddChild((Node)(object)val11, false, (InternalMode)0);
		VBoxContainer val22 = DetailSection("details.dependencies.caption");
		((Node)val22).AddChild((Node)(object)_detailDependencies, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_detailWarnings, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val20, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val21, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val22, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_detailIncompatibilities, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val13, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_detailOptions, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildWorkshopDetails(), false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildTranslations(), false, (InternalMode)0);
		MarginContainer val23 = new MarginContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val23).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 4);
		((Node)val23).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val23, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val12, false, (InternalMode)0);
		return (Control)val;
	}

	private static VBoxContainer DetailSection(string headingKey, bool visible = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			Visible = visible,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		((Node)val).AddChild((Node)(object)LocalizedKeyLabel(headingKey, "GroupLabel"), false, (InternalMode)0);
		return val;
	}

	private static GridContainer FactGrid()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		GridContainer val = new GridContainer
		{
			Columns = 2,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 18);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 10);
		return val;
	}

	private static Label AddFact(GridContainer grid, string key)
	{
		Control fact;
		return AddFact(grid, key, out fact);
	}

	private static Label AddFact(GridContainer grid, string key, out Control fact)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)1
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 1);
		Label val2 = LocalizedKeyLabel(key, "CaptionLabel");
		val2.TextOverrunBehavior = (OverrunBehavior)3;
		Label val3 = UiKit.Label("—");
		((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
		val3.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)grid).AddChild((Node)(object)val, false, (InternalMode)0);
		fact = (Control)(object)val;
		return val3;
	}

	private static void UpdateDescriptionEditor(LaunchEntry entry)
	{
		if (_detailDescription != null && _detailDescriptionEditor != null && _detailDescriptionActions != null)
		{
			bool flag = string.Equals(_descriptionEditingKey, entry.Key, StringComparison.Ordinal);
			if (_descriptionEditingKey != null && !flag)
			{
				_descriptionEditingKey = null;
			}
			((CanvasItem)_detailDescription).Visible = !flag;
			((CanvasItem)_detailDescriptionEditor).Visible = flag;
			((CanvasItem)_detailDescriptionActions).Visible = flag;
			if (!flag)
			{
				string displayDescription = entry.DisplayDescription;
				_detailDescription.Text = (string.IsNullOrWhiteSpace(displayDescription) ? MlmLocManager.Get("details.description.missing") : displayDescription);
			}
		}
	}

	private static void OnDescriptionGuiInput(InputEvent inputEvent)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Invalid comparison between Unknown and I8
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null && (long)val.ButtonIndex == 1 && val.Pressed && val.DoubleClick)
		{
			BeginDescriptionEdit();
		}
	}

	private static void BeginDescriptionEdit()
	{
		if (_selected != null && !_pumpActive && _detailDescriptionEditor != null)
		{
			LaunchEntry selected = _selected;
			_descriptionEditingKey = selected.Key;
			_detailDescriptionEditor.Text = selected.DisplayDescription;
			UpdateDescriptionEditor(selected);
			((Control)_detailDescriptionEditor).GrabFocus();
			_detailDescriptionEditor.SetCaretLine(_detailDescriptionEditor.GetLineCount() - 1, true, true, 0, 0);
			_detailDescriptionEditor.SetCaretColumn(_detailDescriptionEditor.GetLine(_detailDescriptionEditor.GetLineCount() - 1).Length, true, 0);
		}
	}

	private static void CancelDescriptionEdit()
	{
		_descriptionEditingKey = null;
		UpdateDetails();
	}

	private static void SaveDescriptionEdit()
	{
		if (_selected != null && _detailDescriptionEditor != null && _descriptionEditingKey != null)
		{
			LaunchEntry entry = _selected;
			string text = _detailDescriptionEditor.Text.Trim();
			string stored = (string.Equals(text, entry.DisplayDescription.Trim(), StringComparison.Ordinal) ? entry.DescriptionOverride : (string.Equals(text, entry.ManifestDescription.Trim(), StringComparison.Ordinal) ? string.Empty : text));
			if (MutateOrganization(delegate
			{
				_organization.SetDescription(entry.Id, stored);
			}))
			{
				_descriptionEditingKey = null;
				UpdateDetails();
				ShowFeedback(PlanResult.Ok(MlmLocManager.Get((stored.Length == 0) ? "details.description.restored" : "details.description.saved", ("name", entry.DisplayName))));
			}
		}
	}

	private static void UpdateOrganizationEditor(LaunchEntry entry)
	{
		if (_detailAlias == null || _detailGroupSelector == null)
		{
			return;
		}
		if (!string.Equals(_organizationEditingKey, entry.Key, StringComparison.Ordinal))
		{
			_updatingOrganizationFields = true;
			try
			{
				_detailAlias.Text = entry.Alias;
				PopulateDetailGroupSelector(entry.Group);
				_organizationEditingKey = entry.Key;
			}
			finally
			{
				_updatingOrganizationFields = false;
			}
		}
		if (_detailAliasFill != null)
		{
			((CanvasItem)_detailAliasFill).Visible = SuggestedWorkshopAlias(entry).Length > 0;
			((BaseButton)_detailAliasFill).Disabled = _pumpActive;
		}
		_detailAlias.Editable = !_pumpActive;
		((BaseButton)_detailGroupSelector).Disabled = _pumpActive;
		UpdateOrganizationSaveState();
	}

	private static void UpdateOptionsEditor(LaunchEntry entry)
	{
		if (_detailOptions != null && _detailOptionRows != null)
		{
			if (!string.Equals(_optionsEditingKey, entry.Key, StringComparison.Ordinal))
			{
				RebuildOptionRows(entry);
				_optionsEditingKey = entry.Key;
			}
			SetOptionEditorsEnabled(!_pumpActive);
		}
	}

	private static void RebuildOptionRows(LaunchEntry entry)
	{
		VBoxContainer detailOptionRows = _detailOptionRows;
		foreach (Node child in ((Node)detailOptionRows).GetChildren(false))
		{
			((Node)detailOptionRows).RemoveChild(child);
			child.QueueFree();
		}
		OptionEditors.Clear();
		ModOptionCatalog catalog = ModOptionCatalog.For(entry.Mod);
		ModOptionEntry valueOrDefault = _options.Mods.GetValueOrDefault(entry.Id);
		List<ModOptionSpec> list = catalog.Specs.Where((ModOptionSpec spec) => !spec.Advanced).ToList();
		List<ModOptionSpec> list2 = catalog.Specs.Where((ModOptionSpec spec) => spec.Advanced).ToList();
		List<string> list3 = valueOrDefault?.Values.Keys.Where((string key) => catalog.Find(key) == null).OrderBy<string, string>((string key) => key, StringComparer.Ordinal).ToList() ?? new List<string>();
		((CanvasItem)_detailOptions).Visible = list.Count > 0 || list2.Count > 0 || list3.Count > 0;
		if (!((CanvasItem)_detailOptions).Visible)
		{
			return;
		}
		string text = null;
		foreach (ModOptionSpec item in list)
		{
			if (!string.IsNullOrWhiteSpace(item.Section) && !string.Equals(text, item.Section, StringComparison.Ordinal))
			{
				text = item.Section;
				((Node)detailOptionRows).AddChild((Node)(object)UiKit.Label(text, "CaptionLabel"), false, (InternalMode)0);
			}
			((Node)detailOptionRows).AddChild((Node)(object)BuildOptionRow(entry, catalog, valueOrDefault, item), false, (InternalMode)0);
		}
		if (entry.IsManager)
		{
			Label val = UiKit.Label(MlmLocManager.Get("options.managerSelfHint"), "DimLabel");
			val.AutowrapMode = (AutowrapMode)3;
			((Node)detailOptionRows).AddChild((Node)(object)val, false, (InternalMode)0);
		}
		if (list2.Count > 0)
		{
			Button val2 = UiKit.Button(_optionsShowAdvanced ? MlmLocManager.Get("options.advanced.hide", ("count", list2.Count)) : MlmLocManager.Get("options.advanced.show", ("count", list2.Count)), "MiniButton", 0f, 30f);
			((Control)val2).SizeFlagsHorizontal = (SizeFlags)0;
			((BaseButton)val2).Pressed += delegate
			{
				_optionsShowAdvanced = !_optionsShowAdvanced;
				RefreshOptionRows();
			};
			((Node)detailOptionRows).AddChild((Node)(object)val2, false, (InternalMode)0);
			OptionEditors.Add(((Control)(object)val2, true));
			if (_optionsShowAdvanced)
			{
				if (list2.Any((ModOptionSpec spec) => !spec.Declared))
				{
					Label val3 = UiKit.Label(MlmLocManager.Get("options.advanced.discoveredHint"), "DimLabel");
					val3.AutowrapMode = (AutowrapMode)3;
					((Node)detailOptionRows).AddChild((Node)(object)val3, false, (InternalMode)0);
				}
				foreach (ModOptionSpec item2 in list2)
				{
					((Node)detailOptionRows).AddChild((Node)(object)BuildOptionRow(entry, catalog, valueOrDefault, item2), false, (InternalMode)0);
				}
			}
		}
		foreach (string item3 in list3)
		{
			((Node)detailOptionRows).AddChild((Node)(object)BuildStaleOptionRow(entry, valueOrDefault, item3), false, (InternalMode)0);
		}
	}

	private static Control BuildOptionRow(LaunchEntry entry, ModOptionCatalog catalog, ModOptionEntry? stored, ModOptionSpec spec)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		bool flag = ModOptionValues.IsOverridden(spec, stored);
		Label val2 = UiKit.Label((flag ? "●  " : string.Empty) + spec.DisplayName + (spec.Declared ? string.Empty : "  ?"), flag ? "MutedLabel" : "DimLabel");
		((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val2).SizeFlagsVertical = (SizeFlags)4;
		val2.TextOverrunBehavior = (OverrunBehavior)3;
		((Control)val2).TooltipText = BuildOptionTooltip(spec, catalog, stored);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)BuildOptionEditor(entry, spec, ModOptionValues.Effective(spec, catalog, stored)), false, (InternalMode)0);
		string text = ModOptionValues.Display(ModOptionValues.Fallback(spec, catalog, stored));
		Button val3 = UiKit.Button("↺", "IconButton", 30f, 30f);
		((Control)val3).SizeFlagsVertical = (SizeFlags)4;
		((Control)val3).TooltipText = ((text.Length > 0) ? MlmLocManager.Get("options.override.resetToFallback", ("fallback", text)) : MlmLocManager.Get("options.override.resetToDefault"));
		((BaseButton)val3).Pressed += delegate
		{
			ClearOptionOverride(entry, spec.Key, spec.DisplayName);
		};
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		OptionEditors.Add(((Control)(object)val3, flag));
		return (Control)(object)val;
	}

	private static Control BuildOptionEditor(LaunchEntry entry, ModOptionSpec spec, JsonNode? value)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Expected O, but got Unknown
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Expected O, but got Unknown
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Expected O, but got Unknown
		switch (spec.Type)
		{
		case ModOptionType.Bool:
		{
			bool flag = ModOptionValues.AsBool(value);
			Button val2 = UiKit.Button(flag ? MlmLocManager.Get("options.boolean.on") : MlmLocManager.Get("options.boolean.off"), "MiniButton", 62f, 30f);
			((BaseButton)val2).ToggleMode = true;
			((BaseButton)val2).ButtonPressed = flag;
			((Control)val2).SizeFlagsVertical = (SizeFlags)4;
			((BaseButton)val2).Toggled += (ToggledEventHandler)delegate(bool pressed)
			{
				SetOptionOverride(entry, spec, JsonValue.Create(pressed, (JsonNodeOptions?)null));
			};
			OptionEditors.Add(((Control)(object)val2, true));
			return (Control)(object)val2;
		}
		case ModOptionType.Enum:
		{
			string text2 = ModOptionValues.Display(value);
			List<string> choices = spec.Choices.ToList();
			if (text2.Length > 0 && !choices.Contains<string>(text2, StringComparer.Ordinal))
			{
				choices.Insert(0, text2);
			}
			OptionButton val = new OptionButton
			{
				CustomMinimumSize = new Vector2(150f, 30f),
				SizeFlagsVertical = (SizeFlags)4,
				FocusMode = (FocusModeEnum)2,
				ThemeTypeVariation = StringName.op_Implicit("MiniButton"),
				FitToLongestItem = false
			};
			for (int i = 0; i < choices.Count; i++)
			{
				val.AddItem(choices[i], i);
				if (string.Equals(choices[i], text2, StringComparison.Ordinal))
				{
					val.Selected = i;
				}
			}
			val.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
			{
				SetOptionOverride(entry, spec, JsonValue.Create(choices[checked((int)index)], (JsonNodeOptions?)null));
			};
			OptionEditors.Add(((Control)(object)val, true));
			return (Control)(object)val;
		}
		default:
		{
			LineEdit edit = new LineEdit
			{
				Text = ModOptionValues.Display(value),
				CustomMinimumSize = new Vector2(150f, 30f),
				SizeFlagsVertical = (SizeFlags)4
			};
			edit.TextSubmitted += (TextSubmittedEventHandler)delegate(string text)
			{
				CommitOptionText(entry, spec, text);
			};
			((Control)edit).FocusExited += delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)edit))
				{
					CommitOptionText(entry, spec, edit.Text);
				}
			};
			OptionEditors.Add(((Control)(object)edit, true));
			return (Control)(object)edit;
		}
		}
	}

	private static Control BuildStaleOptionRow(LaunchEntry entry, ModOptionEntry stored, string key)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val2 = UiKit.Label(key + " = " + ModOptionValues.Display(stored.Values[key]), "DimLabel");
		((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val2).SizeFlagsVertical = (SizeFlags)4;
		val2.TextOverrunBehavior = (OverrunBehavior)3;
		((Control)val2).TooltipText = MlmLocManager.Get("options.stale.tooltip");
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Button val3 = UiKit.Button("✕", "IconButton", 30f, 30f);
		((Control)val3).SizeFlagsVertical = (SizeFlags)4;
		((Control)val3).TooltipText = MlmLocManager.Get("options.stale.deleteTooltip");
		((BaseButton)val3).Pressed += delegate
		{
			ClearOptionOverride(entry, key, key);
		};
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		OptionEditors.Add(((Control)(object)val3, true));
		return (Control)val;
	}

	private static string BuildOptionTooltip(ModOptionSpec spec, ModOptionCatalog catalog, ModOptionEntry? stored)
	{
		List<string> list = new List<string>();
		if (!string.IsNullOrWhiteSpace(spec.Description))
		{
			list.Add(spec.Description);
		}
		list.Add(MlmLocManager.Get("options.tooltip.manifestKey", ("key", spec.Key)));
		if (spec.DeclaredDefault != null)
		{
			string item = ModOptionValues.Display(spec.DeclaredDefault);
			list.Add(MlmLocManager.Get("options.tooltip.declaredDefault", ("value", item)));
		}
		string text = ModOptionValues.RangeText(spec);
		if (text != null && text.Length > 0)
		{
			list.Add(MlmLocManager.Get("options.tooltip.range", ("range", text)));
		}
		if (!spec.Declared)
		{
			list.Add(MlmLocManager.Get("options.tooltip.discovered"));
		}
		if (ModOptionValues.IsOverridden(spec, stored))
		{
			string item2 = ModOptionValues.Display(ModOptionValues.Effective(spec, catalog, stored));
			list.Add(MlmLocManager.Get("options.tooltip.overridden", ("value", item2)));
		}
		return string.Join('\n', list);
	}

	private static void CommitOptionText(LaunchEntry entry, ModOptionSpec spec, string text)
	{
		if (_pumpActive)
		{
			return;
		}
		string error;
		JsonNode jsonNode = ModOptionValues.FromText(spec, text, out error);
		if (error != null)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("options.error.invalidNumber", ("value", error), ("label", spec.DisplayName))));
			RefreshOptionRows();
			return;
		}
		ModOptionCatalog catalog = ModOptionCatalog.For(entry.Mod);
		ModOptionEntry valueOrDefault = _options.Mods.GetValueOrDefault(entry.Id);
		if (!JsonNode.DeepEquals(ModOptionValues.Effective(spec, catalog, valueOrDefault), jsonNode))
		{
			SetOptionOverride(entry, spec, jsonNode);
		}
	}

	private static void SetOptionOverride(LaunchEntry entry, ModOptionSpec spec, JsonNode? value)
	{
		if (!_pumpActive)
		{
			if (!_options.Mods.TryGetValue(entry.Id, out ModOptionEntry value2))
			{
				value2 = new ModOptionEntry();
				_options.Mods[entry.Id] = value2;
			}
			value2.Values[spec.Key] = value;
			PersistOptions(MlmLocManager.Get("options.apply.currentLaunch", ("label", spec.DisplayName), ("value", ModOptionValues.Display(value))));
		}
	}

	private static void ClearOptionOverride(LaunchEntry entry, string key, string label)
	{
		if (!_pumpActive && _options.Mods.TryGetValue(entry.Id, out ModOptionEntry value) && value.Values.Remove(key))
		{
			if (value.IsEmpty)
			{
				_options.Mods.Remove(entry.Id);
			}
			PersistOptions(MlmLocManager.Get("options.value.restored", ("label", label)));
		}
	}

	private static void PersistOptions(string message)
	{
		PlanResult planResult = ModOptionStore.Save(_options);
		ShowFeedback(planResult.Success ? PlanResult.Ok(message) : planResult);
		RefreshOptionRows();
	}

	private static void RefreshOptionRows()
	{
		_optionsEditingKey = null;
		Defer(UpdateDetails);
	}

	private static void SetOptionEditorsEnabled(bool enabled)
	{
		foreach (var (val, flag) in OptionEditors)
		{
			if (!GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				continue;
			}
			bool flag2 = enabled && flag;
			LineEdit val2 = (LineEdit)(object)((val is LineEdit) ? val : null);
			if (val2 == null)
			{
				OptionButton val3 = (OptionButton)(object)((val is OptionButton) ? val : null);
				if (val3 == null)
				{
					Button val4 = (Button)(object)((val is Button) ? val : null);
					if (val4 != null)
					{
						((BaseButton)val4).Disabled = !flag2;
					}
				}
				else
				{
					((BaseButton)val3).Disabled = !flag2;
				}
			}
			else
			{
				val2.Editable = flag2;
			}
		}
	}

	private static void UpdateDetails()
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Invalid comparison between Unknown and I4
		if (_selected == null || _plan == null)
		{
			return;
		}
		LaunchEntry selected = _selected;
		ModManifest manifest = selected.Mod.manifest;
		RefreshTranslatedMetadata(selected);
		if (_detailName != null)
		{
			_detailName.Text = selected.DisplayName;
		}
		UpdateDetailMeta(selected, manifest);
		Label? detailContent;
		string text;
		if (_detailContent != null)
		{
			detailContent = _detailContent;
			bool? flag = manifest?.hasDll;
			bool? flag2 = manifest?.hasPck;
			if (!flag.HasValue)
			{
				goto IL_00fc;
			}
			if (flag == true)
			{
				if (!flag2.HasValue)
				{
					goto IL_00fc;
				}
				text = ((flag2 != true) ? MlmLocManager.Get("details.content.code") : MlmLocManager.Get("details.content.codeAndResources"));
			}
			else
			{
				if ((!flag2) ?? true)
				{
					goto IL_00fc;
				}
				text = MlmLocManager.Get("details.content.resources");
			}
			goto IL_010c;
		}
		goto IL_0112;
		IL_0112:
		if (_detailGameVersion != null)
		{
			_detailGameVersion.Text = manifest?.minGameVersion ?? MlmLocManager.Get("details.value.notSpecified");
		}
		bool flag3 = (int)selected.Mod.modSource != 2;
		if (_detailInstallTimeFact != null)
		{
			((CanvasItem)_detailInstallTimeFact).Visible = flag3;
		}
		if (_detailInstallTime != null)
		{
			_detailInstallTime.Text = ((flag3 && LocalModInstallTime.TryGet(selected.Mod, out var installTime)) ? DisplayTime.Format(installTime) : MlmLocManager.Get("details.value.notAvailable"));
		}
		if (_detailPriority != null)
		{
			if (_detailPriorityFact != null)
			{
				((CanvasItem)_detailPriorityFact).Visible = selected.DeclaredPriority.HasValue;
			}
			ModLoadPriority? declaredPriority = selected.DeclaredPriority;
			if (declaredPriority.HasValue)
			{
				ModLoadPriority valueOrDefault = declaredPriority.GetValueOrDefault();
				string text2 = ModLoadPriorities.Label(valueOrDefault);
				_detailPriority.Text = (selected.IgnorePriority ? MlmLocManager.Get("details.value.priorityIgnored", ("tier", text2)) : text2);
				if (_detailPriorityFact != null)
				{
					_detailPriorityFact.TooltipText = MlmLocManager.Get("details.fact.loadPriority.tooltip");
				}
			}
		}
		UpdateDescriptionEditor(selected);
		if (_detailDependencies != null)
		{
			_detailDependencies.Text = BuildDependencyReport(selected);
		}
		if (_detailIncompatibilities != null && _detailIncompatibilityText != null)
		{
			string text3 = BuildIncompatibilityReport(selected);
			((CanvasItem)_detailIncompatibilities).Visible = text3 != null;
			_detailIncompatibilityText.Text = text3 ?? string.Empty;
		}
		if (_detailBadges != null)
		{
			foreach (Node child in ((Node)_detailBadges).GetChildren(false))
			{
				((Node)_detailBadges).RemoveChild(child);
				child.QueueFree();
			}
			((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(selected.Enabled ? MlmLocManager.Get("details.badge.enabled") : MlmLocManager.Get("details.badge.disabled"), (!selected.Enabled) ? BadgeTone.Quiet : BadgeTone.Positive), false, (InternalMode)0);
			WorkshopStatus workshopStatus = WorkshopUpdates.For(selected.Mod);
			ModRowStatus? modRowStatus = WorkshopUpdateRowStatus(workshopStatus);
			if (modRowStatus.HasValue)
			{
				ModRowStatus valueOrDefault2 = modRowStatus.GetValueOrDefault();
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(valueOrDefault2.Text, (workshopStatus.State == WorkshopUpdateState.Updated) ? BadgeTone.Positive : BadgeTone.Caution), false, (InternalMode)0);
			}
			if (selected.IsManager)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.fixedOrder"), BadgeTone.Neutral), false, (InternalMode)0);
			}
			else if (selected.IsCompat)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.repairsLaterMods"), BadgeTone.Neutral), false, (InternalMode)0);
			}
			LaunchPlan? plan = _plan;
			if (plan != null && plan.HasDuplicateSiblings(selected))
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.sameIdCopy"), BadgeTone.Caution), false, (InternalMode)0);
			}
			if (!string.IsNullOrWhiteSpace(selected.DescriptionOverride))
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.customDescription"), BadgeTone.Neutral), false, (InternalMode)0);
			}
			int num = _options.Mods.GetValueOrDefault(selected.Id)?.Values.Count ?? 0;
			if (num > 0)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.optionCount", ("count", num)), BadgeTone.Neutral), false, (InternalMode)0);
			}
			List<LocString> errors = selected.Mod.errors;
			if (errors != null && errors.Any(ModErrorText.IsSteamBranchMismatch))
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.branchMismatch", ("branch", ModErrorText.CurrentBranchName())), BadgeTone.Critical), false, (InternalMode)0);
			}
			WorkshopStatus? workshopStatus2 = WorkshopUpdates.For(selected.Mod);
			if (workshopStatus2 != null && workshopStatus2.HasIncompatibleAvailableUpdate)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.updateBranchMismatch", ("branch", ModErrorText.CurrentBranchName())), BadgeTone.Critical), false, (InternalMode)0);
			}
			ModGameVersionCheck modGameVersionCheck = ModCompatibility.CheckGameVersion(selected.Mod);
			if (modGameVersionCheck.State == ModGameVersionCompatibility.Unsupported)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.gameVersionMismatch", ("required", modGameVersionCheck.RequiredVersion), ("current", modGameVersionCheck.CurrentVersion)), BadgeTone.Critical), false, (InternalMode)0);
			}
			else if (modGameVersionCheck.State == ModGameVersionCompatibility.InvalidMinimumVersion)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.invalidMinimumVersion", ("version", modGameVersionCheck.RequiredVersion)), BadgeTone.Critical), false, (InternalMode)0);
			}
			if (ActiveConflicts(selected).Count > 0)
			{
				((Node)_detailBadges).AddChild((Node)(object)UiKit.Badge(MlmLocManager.Get("details.badge.incompatible"), BadgeTone.Critical), false, (InternalMode)0);
			}
		}
		if (_detailToggle != null)
		{
			((BaseButton)_detailToggle).Disabled = !selected.CanToggle || _pumpActive;
			_detailToggle.Text = (selected.Enabled ? MlmLocManager.Get("details.action.disableForLaunch") : MlmLocManager.Get("details.action.enableForLaunch"));
			((Control)_detailToggle).ThemeTypeVariation = StringName.op_Implicit(selected.Enabled ? "SecondaryButton" : "PrimaryButton");
			Button? detailToggle = _detailToggle;
			object tooltipText;
			if (!selected.IsManager)
			{
				LaunchPlan? plan2 = _plan;
				tooltipText = ((plan2 != null && plan2.HasDuplicateSiblings(selected)) ? MlmLocManager.Get("details.toggle.duplicateTooltip") : ((!selected.Enabled) ? MlmLocManager.Get("details.toggle.enableTooltip") : string.Empty));
			}
			else
			{
				tooltipText = MlmLocManager.Get("details.toggle.managerTooltip");
			}
			((Control)detailToggle).TooltipText = (string)tooltipText;
		}
		if (_detailOpenFolder != null)
		{
			((BaseButton)_detailOpenFolder).Disabled = _pumpActive || string.IsNullOrWhiteSpace(selected.Mod.path);
			((Control)_detailOpenFolder).TooltipText = selected.Mod.path;
		}
		UpdateOrganizationEditor(selected);
		UpdateOptionsEditor(selected);
		UpdateTranslationControls();
		UpdateWarningDetails(selected);
		UpdateWorkshopTags(selected);
		UpdateWorkshopDetails(selected);
		UpdateControllerPrompts();
		return;
		IL_010c:
		detailContent.Text = text;
		goto IL_0112;
		IL_00fc:
		text = MlmLocManager.Get("details.value.notSpecified");
		goto IL_010c;
	}

	private static void UpdateDetailMeta(LaunchEntry entry, ModManifest? manifest)
	{
		if (_detailMeta == null)
		{
			return;
		}
		string text = MlmLocManager.Get("details.value.notSpecified");
		List<(string, string)> list = new List<(string, string)>();
		if (!string.IsNullOrWhiteSpace(entry.Alias))
		{
			list.Add(("details.meta.originalName", entry.Name));
		}
		list.Add(("details.fact.author", string.IsNullOrWhiteSpace(manifest?.author) ? text : TextEncodingRepair.Repair(manifest.author).Trim()));
		list.Add(("details.fact.version", string.IsNullOrWhiteSpace(manifest?.version) ? text : manifest.version.Trim()));
		list.Add(("details.meta.id", entry.Id));
		string text2 = MlmLocManager.EffectiveLocale + "\n" + string.Join('\n', list.Select<(string, string), string>(((string Key, string Value) pair) => pair.Key + "=" + pair.Value));
		if (text2 == _detailMetaSignature)
		{
			return;
		}
		_detailMetaSignature = text2;
		foreach (Node child in ((Node)_detailMeta).GetChildren(false))
		{
			((Node)_detailMeta).RemoveChild(child);
			child.QueueFree();
		}
		foreach (var (key, value) in list)
		{
			AddDetailMeta(key, value);
		}
	}

	private static void AddDetailMeta(string key, string value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			TooltipText = ((value.Length > 40) ? value : string.Empty),
			MouseFilter = (MouseFilterEnum)1
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 5);
		Label val2 = UiKit.Label(MlmLocManager.Get(key), "CaptionLabel");
		Label val3 = UiKit.Label((value.Length > 40) ? (value.Substring(0, 39).TrimEnd() + "…") : value, "MetaLabel");
		Label[] array = (Label[])(object)new Label[2] { val2, val3 };
		foreach (Label obj in array)
		{
			((Control)obj).SizeFlagsVertical = (SizeFlags)1;
			obj.VerticalAlignment = (VerticalAlignment)2;
		}
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)_detailMeta).AddChild((Node)(object)val, false, (InternalMode)0);
	}

	private static void UpdateWarningDetails(LaunchEntry entry)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		List<(Color, string)> list = new List<(Color, string)>();
		List<LocString> errors = entry.Mod.errors;
		if (errors != null && errors.Count > 0)
		{
			list.AddRange(errors.Select((LocString error) => (ModErrorText.IsSteamBranchMismatch(error) ? UiKit.Danger : UiKit.Warning, ModErrorText.Describe(error))));
		}
		ModGameVersionCheck modGameVersionCheck = ModCompatibility.CheckGameVersion(entry.Mod);
		if (modGameVersionCheck.State == ModGameVersionCompatibility.Unsupported)
		{
			list.Add((UiKit.Danger, MlmLocManager.Get("details.warning.gameVersionUnsupported", ("required", modGameVersionCheck.RequiredVersion), ("current", modGameVersionCheck.CurrentVersion))));
		}
		else if (modGameVersionCheck.State == ModGameVersionCompatibility.InvalidMinimumVersion)
		{
			list.Add((UiKit.Danger, MlmLocManager.Get("details.warning.invalidMinimumVersion", ("version", modGameVersionCheck.RequiredVersion))));
		}
		foreach (IGrouping<LaunchEntry, ModConflict> item in ActiveConflicts(entry))
		{
			Color danger = UiKit.Danger;
			string text = ConflictReason(item.Select((ModConflict rule) => rule.Rule));
			list.Add((danger, (text != null) ? MlmLocManager.Get("details.warning.incompatibleWithReason", ("name", item.Key.DisplayName), ("reason", text)) : MlmLocManager.Get("details.warning.incompatible", ("name", item.Key.DisplayName))));
		}
		WorkshopStatus workshopStatus = WorkshopUpdates.For(entry.Mod);
		if (workshopStatus != null && workshopStatus.HasIncompatibleAvailableUpdate)
		{
			list.Add((UiKit.Danger, MlmLocManager.Get("details.warning.latestBranchUnsupported", ("branch", ModErrorText.CurrentBranchName()), ("ranges", workshopStatus.LatestBranchRanges ?? MlmLocManager.Get("common.value.unknown")))));
		}
		bool flag = list.Count > 0;
		if (_detailWarnings != null)
		{
			((CanvasItem)_detailWarnings).Visible = flag;
		}
		if (_detailWarningText != null && flag)
		{
			_detailWarningText.Text = string.Join('\n', list.Select<(Color, string), string>(((Color Color, string Text) warning) => Colored(warning.Color, "•  " + Escape(warning.Text))));
		}
	}

	private static string BuildDependencyReport(LaunchEntry entry)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		if (_plan != null)
		{
			List<ModDependency> list = entry.Mod.manifest?.dependencies;
			if (list != null && list.Count > 0)
			{
				int num = _plan.IndexOf(entry);
				List<string> list2 = new List<string>();
				foreach (ModDependency dependency in list)
				{
					string item = Escape(dependency.id ?? "<unknown>");
					string item2 = (string.IsNullOrWhiteSpace(dependency.minVersion) ? string.Empty : ("  ≥ " + Escape(dependency.minVersion)));
					LaunchEntry launchEntry = _plan.Entries.FirstOrDefault((LaunchEntry candidate) => candidate.Enabled && string.Equals(candidate.Id, dependency.id, StringComparison.Ordinal)) ?? _plan.Entries.FirstOrDefault((LaunchEntry candidate) => string.Equals(candidate.Id, dependency.id, StringComparison.Ordinal));
					if (launchEntry == null)
					{
						list2.Add(Colored(UiKit.Danger, MlmLocManager.Get("details.dependencies.missing", ("id", item), ("bound", item2))));
						continue;
					}
					int num2 = _plan.IndexOf(launchEntry);
					if (!launchEntry.Enabled)
					{
						list2.Add(Colored(UiKit.Warning, MlmLocManager.Get("details.dependencies.disabled", ("id", item), ("bound", item2))));
					}
					else if (entry.Enabled && num2 > num)
					{
						list2.Add(Colored(UiKit.Warning, MlmLocManager.Get("details.dependencies.later", ("id", item), ("bound", item2), ("position", _plan.LoadPosition(launchEntry)))));
					}
					else
					{
						list2.Add(Colored(UiKit.Success, MlmLocManager.Get("details.dependencies.ready", ("id", item), ("bound", item2), ("position", _plan.LoadPosition(launchEntry)))));
					}
				}
				return string.Join('\n', list2);
			}
		}
		return Colored(UiKit.TextDim, Escape(MlmLocManager.Get("details.dependencies.none")));
	}

	private static List<IGrouping<LaunchEntry, ModConflict>> ActiveConflicts(LaunchEntry entry)
	{
		object obj = _rowStatusLookup?.ActiveConflicts(entry);
		if (obj == null)
		{
			if (!entry.Enabled || _plan == null)
			{
				return new List<IGrouping<LaunchEntry, ModConflict>>();
			}
			obj = (from conflict in _plan.Conflicts()
				where conflict.Involves(entry)
				group conflict by conflict.OtherThan(entry)).ToList();
		}
		return (List<IGrouping<LaunchEntry, ModConflict>>)obj;
	}

	private static string? ConflictReason(IEnumerable<ModIncompatibility> rules)
	{
		string text = string.Join(' ', rules.Select((ModIncompatibility rule) => rule.Reason).OfType<string>().Distinct<string>(StringComparer.Ordinal));
		if (text.Length != 0)
		{
			return text;
		}
		return null;
	}

	private static string? BuildIncompatibilityReport(LaunchEntry entry)
	{
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		if (_plan == null)
		{
			return null;
		}
		List<ModConflict> source = _plan.ConflictsFor(entry);
		List<string> list = entry.DeclaredIncompatibilities.Select((ModIncompatibility rule) => rule.Id).Concat(source.Select((ModConflict conflict) => conflict.OtherThan(entry).Id)).Distinct<string>(StringComparer.Ordinal)
			.ToList();
		if (list.Count == 0)
		{
			return null;
		}
		List<string> list2 = new List<string>();
		foreach (string id in list)
		{
			LaunchEntry other = _plan.CopyInUse(id);
			List<ModConflict> list3 = source.Where((ModConflict conflict) => conflict.OtherThan(entry) == other).ToList();
			List<ModIncompatibility> list4 = entry.DeclaredIncompatibilities.Where((ModIncompatibility rule) => string.Equals(rule.Id, id, StringComparison.Ordinal)).ToList();
			string text = string.Join(MlmLocManager.Get("common.listSeparator"), from rule in list4
				select rule.RangeText into range
				where range.Length > 0
				select range);
			Color color;
			string key;
			if (other != null)
			{
				if (list3.Count != 0)
				{
					if (other.Enabled)
					{
						if (!entry.Enabled)
						{
							Color warning = UiKit.Warning;
							color = warning;
							key = "details.incompatibilities.wouldConflict";
						}
						else
						{
							Color warning = UiKit.Danger;
							color = warning;
							key = "details.incompatibilities.conflict";
						}
					}
					else
					{
						Color warning = UiKit.Success;
						color = warning;
						key = "details.incompatibilities.off";
					}
				}
				else
				{
					Color warning = UiKit.Success;
					color = warning;
					key = "details.incompatibilities.otherVersion";
				}
			}
			else
			{
				Color warning = UiKit.Success;
				color = warning;
				key = "details.incompatibilities.notInstalled";
			}
			list2.Add(Colored(color, MlmLocManager.Get(key, ("name", Escape(other?.DisplayName ?? id)), ("range", (text.Length == 0) ? string.Empty : ("  " + Escape(text))), ("version", Escape(other?.Mod.manifest?.version ?? string.Empty)))));
			string text2 = ConflictReason(list4.Concat(list3.Select((ModConflict conflict) => conflict.Rule)));
			if (text2 != null)
			{
				list2.Add(Colored(UiKit.TextDim, "      " + MlmLocManager.Get("details.incompatibilities.reason", ("reason", Escape(text2)))));
			}
		}
		return string.Join('\n', list2);
	}

	private static string Colored(Color color, string text)
	{
		return $"[color=#{((Color)(ref color)).ToHtml(false)}]{text}[/color]";
	}

	private static string Escape(string text)
	{
		return text.Replace("[", "[lb]");
	}

	private static Control BuildFab(Control listArea)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Expected O, but got Unknown
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Expected O, but got Unknown
		_fabTucked = false;
		_listInputAt = 0uL;
		_fab = new Button
		{
			Name = StringName.op_Implicit("ToolsButton"),
			Text = "⋯",
			CustomMinimumSize = new Vector2(48f, 48f),
			FocusMode = (FocusModeEnum)2,
			MouseDefaultCursorShape = (CursorShape)2
		};
		((Control)_fab).SetAnchorsPreset((LayoutPreset)3, false);
		((Control)_fab).GrowHorizontal = (GrowDirection)0;
		((Control)_fab).GrowVertical = (GrowDirection)0;
		((BaseButton)_fab).Pressed += ToggleFabMenu;
		((Control)_fab).FocusEntered += delegate
		{
			ReturnFab();
		};
		Button? fab = _fab;
		object obj = _003C_003Ec._003C_003E9__456_1;
		if (obj == null)
		{
			GuiInputEventHandler val2 = delegate(InputEvent input)
			{
				double? num = WheelPages(input);
				if (num.HasValue)
				{
					double valueOrDefault = num.GetValueOrDefault();
					if (_modTree != null)
					{
						VScrollBar val = ListScroll((Tree)(object)_modTree);
						if (val != null && ((CanvasItem)val).Visible)
						{
							_listInputAt = Time.GetTicksMsec();
							((Range)val).Value = ((Range)val).Value + ((Range)val).Page * valueOrDefault;
							((Control)_fab).AcceptEvent();
						}
					}
				}
			};
			_003C_003Ec._003C_003E9__456_1 = val2;
			obj = (object)val2;
		}
		((Control)fab).GuiInput += (GuiInputEventHandler)obj;
		_fabRest = new Timer
		{
			OneShot = true,
			WaitTime = 0.6
		};
		_fabRest.Timeout += delegate
		{
			ReturnFab();
		};
		((Node)listArea).AddChild((Node)(object)_fabRest, false, (InternalMode)0);
		listArea.Resized += delegate
		{
			Defer(PlaceFab);
		};
		WatchListScrolling();
		_fabBadgeText = UiKit.Label(string.Empty);
		_fabBadgeText.HorizontalAlignment = (HorizontalAlignment)1;
		_fabBadgeText.VerticalAlignment = (VerticalAlignment)1;
		((Control)_fabBadgeText).MouseFilter = (MouseFilterEnum)2;
		_fabBadge = new PanelContainer
		{
			Visible = false,
			MouseFilter = (MouseFilterEnum)2,
			CustomMinimumSize = new Vector2(20f, 20f),
			Position = new Vector2(32f, -6f)
		};
		((Node)_fabBadge).AddChild((Node)(object)_fabBadgeText, false, (InternalMode)0);
		((Node)_fab).AddChild((Node)(object)_fabBadge, false, (InternalMode)0);
		BindPalette((GodotObject)(object)_fab, StyleFab);
		BindLocalization((Control)(object)_fab, UpdateFab);
		WorkshopDownloadCenter.Changed = OnDownloadsChanged;
		return (Control)(object)_fab;
	}

	private static void StyleFab()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Expected O, but got Unknown
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		if (_fab != null && GodotObject.IsInstanceValid((GodotObject)(object)_fab))
		{
			StyleBoxFlat val = new StyleBoxFlat
			{
				DrawCenter = false,
				BorderColor = UiKit.Text,
				AntiAliasing = true
			};
			val.SetBorderWidthAll(2);
			val.SetCornerRadiusAll(27);
			val.SetExpandMarginAll(3f);
			((Control)_fab).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)Round(UiKit.AccentStrong));
			((Control)_fab).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)Round(UiKit.Palette.PrimaryHover));
			((Control)_fab).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)Round(UiKit.Palette.PrimaryHover));
			((Control)_fab).AddThemeStyleboxOverride(StringName.op_Implicit("hover_pressed"), (StyleBox)(object)Round(UiKit.Palette.PrimaryHover));
			((Control)_fab).AddThemeStyleboxOverride(StringName.op_Implicit("disabled"), (StyleBox)(object)Round(UiKit.Alpha(UiKit.TextDim, 0.35f)));
			((Control)_fab).AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)val);
			string[] array = new string[5] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" };
			foreach (string text in array)
			{
				((Control)_fab).AddThemeColorOverride(StringName.op_Implicit(text), UiKit.OnAccent);
			}
			((Control)_fab).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(24));
			if (_fabBadge != null && _fabBadgeText != null)
			{
				((Control)_fabBadgeText).AddThemeColorOverride(StringName.op_Implicit("font_color"), Colors.White);
				((Control)_fabBadgeText).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(11));
			}
			UpdateFab();
			Defer(PlaceFab);
		}
		static StyleBoxFlat Round(Color fill)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Expected O, but got Unknown
			StyleBoxFlat val2 = new StyleBoxFlat
			{
				BgColor = fill,
				AntiAliasing = true,
				ShadowColor = UiKit.Alpha(Colors.Black, UiKit.Palette.IsDark ? 0.45f : 0.22f),
				ShadowSize = 6,
				ShadowOffset = new Vector2(0f, 2f)
			};
			val2.SetCornerRadiusAll(24);
			return val2;
		}
	}

	private static void PlaceFab()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		if (_fab != null && GodotObject.IsInstanceValid((GodotObject)(object)_fab) && _modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
		{
			StyleBox themeStylebox = ((Control)_modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null);
			VScrollBar? obj = ListScroll((Tree)(object)_modTree);
			float num = ((obj != null) ? ((Control)obj).GetCombinedMinimumSize().X : 0f);
			float num2 = themeStylebox.GetMargin((Side)2) + num + 14f;
			float num3 = themeStylebox.GetMargin((Side)3) + 14f;
			((Control)_fab).OffsetLeft = 0f - num2 - 48f;
			((Control)_fab).OffsetRight = 0f - num2;
			((Control)_fab).OffsetTop = 0f - num3 - 48f;
			((Control)_fab).OffsetBottom = 0f - num3;
			((Control)_fab).PivotOffset = new Vector2(48f, 48f) / 2f;
			SyncListSpacer();
		}
	}

	private static VScrollBar? ListScroll(Tree tree)
	{
		return ((IEnumerable)((Node)tree).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
	}

	private static double? WheelPages(InputEvent input)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Invalid comparison between Unknown and I8
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Invalid comparison between Unknown and I8
		InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
		if (val != null)
		{
			if (val.Pressed)
			{
				MouseButton buttonIndex = val.ButtonIndex;
				if ((long)buttonIndex == 4)
				{
					return (0f - Math.Max(val.Factor, 1f)) / 8f;
				}
				if ((long)buttonIndex == 5)
				{
					return Math.Max(val.Factor, 1f) / 8f;
				}
			}
		}
		else
		{
			InputEventPanGesture val2 = (InputEventPanGesture)(object)((input is InputEventPanGesture) ? input : null);
			if (val2 != null)
			{
				return val2.Delta.Y / 8f;
			}
		}
		return null;
	}

	private static void WatchListScrolling()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		ModReorderTree modTree = _modTree;
		if (modTree == null)
		{
			return;
		}
		object obj = _003C_003Ec._003C_003E9__461_0;
		if (obj == null)
		{
			GuiInputEventHandler val3 = delegate(InputEvent input)
			{
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				if (!WheelPages(input).HasValue && !(input is InputEventScreenDrag))
				{
					InputEvent obj2 = input;
					InputEventMouseMotion val = (InputEventMouseMotion)(object)((obj2 is InputEventMouseMotion) ? obj2 : null);
					if (val == null || (int)((InputEventMouse)val).ButtonMask == 0)
					{
						InputEvent obj3 = input;
						InputEventKey val2 = (InputEventKey)(object)((obj3 is InputEventKey) ? obj3 : null);
						if (val2 == null || !val2.Pressed || !new string[6] { "ui_up", "ui_down", "ui_page_up", "ui_page_down", "ui_home", "ui_end" }.Any((string action) => input.IsAction(StringName.op_Implicit(action), false)))
						{
							return;
						}
					}
				}
				_listInputAt = Time.GetTicksMsec();
			};
			_003C_003Ec._003C_003E9__461_0 = val3;
			obj = (object)val3;
		}
		((Control)modTree).GuiInput += (GuiInputEventHandler)obj;
		VScrollBar val4 = ListScroll((Tree)(object)modTree);
		if (val4 == null)
		{
			return;
		}
		object obj4 = _003C_003Ec._003C_003E9__461_1;
		if (obj4 == null)
		{
			GuiInputEventHandler val6 = delegate(InputEvent input)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				if (!(input is InputEventMouseButton))
				{
					InputEventMouseMotion val5 = (InputEventMouseMotion)(object)((input is InputEventMouseMotion) ? input : null);
					if (val5 == null || (int)((InputEventMouse)val5).ButtonMask == 0)
					{
						return;
					}
				}
				_listInputAt = Time.GetTicksMsec();
			};
			_003C_003Ec._003C_003E9__461_1 = val6;
			obj4 = (object)val6;
		}
		((Control)val4).GuiInput += (GuiInputEventHandler)obj4;
		object obj5 = _003C_003Ec._003C_003E9__461_2;
		if (obj5 == null)
		{
			ValueChangedEventHandler val7 = delegate
			{
				if (!_rebuildingTree && Time.GetTicksMsec() - _listInputAt <= 400)
				{
					TuckFab();
				}
			};
			_003C_003Ec._003C_003E9__461_2 = val7;
			obj5 = (object)val7;
		}
		((Range)val4).ValueChanged += (ValueChangedEventHandler)obj5;
	}

	private static void TuckFab()
	{
		if (_fab != null && GodotObject.IsInstanceValid((GodotObject)(object)_fab) && !((Control)_fab).HasFocus() && !FabMenuOpen && !TourOpen)
		{
			Timer? fabRest = _fabRest;
			if (fabRest != null)
			{
				fabRest.Start(-1.0);
			}
			if (!_fabTucked)
			{
				_fabTucked = true;
				((Control)_fab).MouseFilter = (MouseFilterEnum)2;
				FadeFab(shown: false, 0.12);
			}
		}
	}

	private static void ReturnFab(bool immediate = false)
	{
		if (_fab == null || !GodotObject.IsInstanceValid((GodotObject)(object)_fab) || !_fabTucked)
		{
			return;
		}
		if (_dragPreviewActive && !immediate && !((Control)_fab).HasFocus())
		{
			Timer? fabRest = _fabRest;
			if (fabRest != null)
			{
				fabRest.Start(-1.0);
			}
			return;
		}
		_fabTucked = false;
		Timer? fabRest2 = _fabRest;
		if (fabRest2 != null)
		{
			fabRest2.Stop();
		}
		((Control)_fab).MouseFilter = (MouseFilterEnum)0;
		FadeFab(shown: true, immediate ? 0.0 : 0.18);
	}

	private static void FadeFab(bool shown, double seconds)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (_fab != null)
		{
			Tween? fabFade = _fabFade;
			if (fabFade != null)
			{
				fabFade.Kill();
			}
			_fabFade = null;
			Color modulate = ((CanvasItem)_fab).Modulate;
			modulate.A = (shown ? 1f : 0f);
			Color val = modulate;
			Vector2 val2 = Vector2.One * (shown ? 1f : 0.8f);
			if (seconds <= 0.0)
			{
				((CanvasItem)_fab).Modulate = val;
				((Control)_fab).Scale = val2;
				return;
			}
			EaseType ease = (EaseType)(shown ? 1 : 0);
			_fabFade = ((Node)_fab).CreateTween().SetParallel(true);
			_fabFade.TweenProperty((GodotObject)(object)_fab, NodePath.op_Implicit("modulate"), Variant.op_Implicit(val), seconds).SetTrans((TransitionType)7).SetEase(ease);
			_fabFade.TweenProperty((GodotObject)(object)_fab, NodePath.op_Implicit("scale"), Variant.op_Implicit(val2), seconds).SetTrans((TransitionType)7).SetEase(ease);
		}
	}

	private static void SyncListSpacer()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		ModReorderTree modTree = _modTree;
		if (modTree == null || !GodotObject.IsInstanceValid((GodotObject)(object)modTree) || _rebuildingTree)
		{
			return;
		}
		TreeItem root = ((Tree)modTree).GetRoot();
		if (root == null || _fab == null || !GodotObject.IsInstanceValid((GodotObject)(object)_fab))
		{
			return;
		}
		TreeItem val = ListSpacer(root);
		float num = ((Control)_fab).Position.Y - 14f;
		bool flag = false;
		int num2 = root.GetChildCount() - ((val != null) ? 1 : 0);
		if (num2 > 0)
		{
			TreeItem child = root.GetChild(0);
			TreeItem child2 = root.GetChild(num2 - 1);
			while (!child2.Collapsed && child2.GetChildCount() > 0)
			{
				child2 = child2.GetChild(child2.GetChildCount() - 1);
			}
			float num3 = TreeItemTop((Tree)(object)modTree, ((Control)modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)0) + 4f);
			Rect2 itemAreaRect = ((Tree)modTree).GetItemAreaRect(child2, -1, -1);
			float y = ((Rect2)(ref itemAreaRect)).End.Y;
			itemAreaRect = ((Tree)modTree).GetItemAreaRect(child, -1, -1);
			float num4 = y - ((Rect2)(ref itemAreaRect)).Position.Y;
			flag = num3 >= 0f && num3 + num4 > num;
		}
		if (!flag)
		{
			if (val != null)
			{
				root.RemoveChild(val);
				((GodotObject)val).Free();
			}
			return;
		}
		if (val == null)
		{
			TreeItem val2 = ((Tree)modTree).CreateItem(root, -1);
			if (val2 == null)
			{
				return;
			}
			val = val2;
			val.SetMetadata(0, Variant.op_Implicit(string.Empty));
			for (int i = 0; i < ColumnCount; i++)
			{
				val.SetSelectable(i, false);
			}
		}
		int num5 = (int)Math.Ceiling(((Control)modTree).Size.Y - ((Control)modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)3) - num) - ((Control)modTree).GetThemeConstant(StringName.op_Implicit("v_separation"), (StringName)null);
		if (val.GetCustomMinimumHeight() != num5)
		{
			val.SetCustomMinimumHeight(Math.Max(1, num5));
		}
	}

	private static TreeItem? ListSpacer(TreeItem root)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Invalid comparison between Unknown and I8
		if (root.GetChildCount() == 0)
		{
			return null;
		}
		TreeItem child = root.GetChild(root.GetChildCount() - 1);
		Variant metadata = child.GetMetadata(0);
		if ((long)((Variant)(ref metadata)).VariantType != 4 || ((Variant)(ref metadata)).AsString().Length != 0)
		{
			return null;
		}
		return child;
	}

	private static void RevealAboveFab(TreeItem item)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		ModReorderTree modTree = _modTree;
		if (modTree == null || !GodotObject.IsInstanceValid((GodotObject)(object)modTree) || !GodotObject.IsInstanceValid((GodotObject)(object)item) || _fab == null || !GodotObject.IsInstanceValid((GodotObject)(object)_fab))
		{
			return;
		}
		VScrollBar val = ListScroll((Tree)(object)modTree);
		if (val == null || !((CanvasItem)val).Visible)
		{
			return;
		}
		Variant metadata = item.GetMetadata(0);
		string text = ((Variant)(ref metadata)).AsString();
		float num = ((Control)modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)0) + 4f;
		float num2 = ((Control)_fab).Position.Y - 14f;
		float num3 = -1f;
		for (float num4 = num2; num4 < ((Control)modTree).Size.Y; num4 += 1f)
		{
			TreeItem itemAtPosition = ((Tree)modTree).GetItemAtPosition(new Vector2(num, num4));
			object obj;
			if (itemAtPosition == null)
			{
				obj = null;
			}
			else
			{
				metadata = itemAtPosition.GetMetadata(0);
				obj = ((Variant)(ref metadata)).AsString();
			}
			if ((string?)obj == text)
			{
				num3 = num4 + 1f;
			}
			else if (num3 >= 0f)
			{
				break;
			}
		}
		if (num3 > num2)
		{
			((Range)val).Value = ((Range)val).Value + (double)(num3 - num2);
		}
	}

	private static void UpdateFab()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		if (_fab != null && GodotObject.IsInstanceValid((GodotObject)(object)_fab))
		{
			int activeCount = WorkshopDownloadCenter.ActiveCount;
			int failedCount = WorkshopDownloadCenter.FailedCount;
			((Control)_fab).TooltipText = ((activeCount > 0) ? MlmLocManager.Get("fab.tooltip.downloading", ("count", activeCount)) : MlmLocManager.Get("fab.tooltip"));
			if (_fabBadge != null && _fabBadgeText != null)
			{
				((CanvasItem)_fabBadge).Visible = activeCount > 0 || failedCount > 0;
				_fabBadgeText.Text = ((activeCount > 0) ? activeCount.ToString() : "!");
				StyleBoxFlat val = new StyleBoxFlat
				{
					BgColor = ((activeCount > 0) ? UiKit.Success : UiKit.Danger),
					AntiAliasing = true,
					ContentMarginLeft = 5f,
					ContentMarginRight = 5f
				};
				val.SetCornerRadiusAll(10);
				((Control)_fabBadge).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val);
			}
		}
	}

	private static void OnDownloadsChanged()
	{
		UpdateFab();
		WorkshopDownloadsDialog.Render();
	}

	private static void ToggleFabMenu()
	{
		if (FabMenuOpen)
		{
			CloseFabMenu();
		}
		else
		{
			OpenFabMenu();
		}
	}

	private static void OpenFabMenu()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		if (_fab == null || !GodotObject.IsInstanceValid((GodotObject)(object)_fab) || _pumpActive || _plan == null || FabMenuOpen || UpdateAnnouncement.IsOpen || TourOpen)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		DisarmDelete();
		ReturnFab(immediate: true);
		_fabMenuLayer = new CanvasLayer
		{
			Name = StringName.op_Implicit("ToolsMenu"),
			Layer = 1050
		};
		Control val2 = UiKit.FullRect<Control>(new Control
		{
			Name = StringName.op_Implicit("ToolsMenuRoot"),
			MouseFilter = (MouseFilterEnum)0,
			Theme = UiKit.CreateTheme()
		});
		object obj = _003C_003Ec._003C_003E9__471_0;
		if (obj == null)
		{
			GuiInputEventHandler val4 = delegate(InputEvent input)
			{
				InputEventMouseButton val3 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
				if (val3 != null && val3.Pressed)
				{
					CloseFabMenu();
				}
			};
			_003C_003Ec._003C_003E9__471_0 = val4;
			obj = (object)val4;
		}
		val2.GuiInput += (GuiInputEventHandler)obj;
		PanelContainer card = UiKit.Card("DialogPanel");
		((Node)card).Name = StringName.op_Implicit("ToolsMenuCard");
		((Control)card).MouseFilter = (MouseFilterEnum)0;
		VBoxContainer val5 = new VBoxContainer();
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		foreach (Button item in FabMenuItems())
		{
			((Node)val5).AddChild((Node)(object)item, false, (InternalMode)0);
		}
		((Node)card).AddChild((Node)(object)UiKit.Margin((Control)(object)val5, 8, 8), false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)card, false, (InternalMode)0);
		((Node)_fabMenuLayer).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val.Root).AddChild((Node)(object)_fabMenuLayer, false, (InternalMode)0);
		Button fab = _fab;
		((Control)card).MinimumSizeChanged += Place;
		Callable val6 = Callable.From((Action)Place);
		((Callable)(ref val6)).CallDeferred(Array.Empty<Variant>());
		Button first = ((IEnumerable)((Node)val5).GetChildren(false)).OfType<Button>().FirstOrDefault((Func<Button, bool>)((Button button) => !((BaseButton)button).Disabled));
		LauncherController.Active?.AddScope(new LauncherController.Scope(val2, CloseFabMenu, () => (Control?)(object)first));
		Button obj2 = first;
		if (obj2 != null)
		{
			((GodotObject)obj2).CallDeferred(MethodName.GrabFocus, Array.Empty<Variant>());
		}
		void Place()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)card) && GodotObject.IsInstanceValid((GodotObject)(object)fab))
			{
				Rect2 globalRect = ((Control)fab).GetGlobalRect();
				Vector2 combinedMinimumSize = ((Control)card).GetCombinedMinimumSize();
				((Control)card).Size = combinedMinimumSize;
				((Control)card).Position = new Vector2(Math.Max(8f, ((Rect2)(ref globalRect)).End.X - combinedMinimumSize.X), Math.Max(8f, ((Rect2)(ref globalRect)).Position.Y - combinedMinimumSize.Y - 10f));
			}
		}
	}

	internal static void CloseFabMenu()
	{
		if (_fabMenuLayer != null && GodotObject.IsInstanceValid((GodotObject)(object)_fabMenuLayer))
		{
			_fabMenuLayer.Hide();
			((Node)_fabMenuLayer).QueueFree();
		}
		_fabMenuLayer = null;
	}

	[IteratorStateMachine(typeof(_003CFabMenuItems_003Ed__473))]
	private static IEnumerable<Button> FabMenuItems()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFabMenuItems_003Ed__473(-2);
	}

	private static Button FabMenuItem(string text, string tooltip, bool enabled, Action action)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Expected O, but got Unknown
		Button item = UiKit.Button(text, "RowButton", 280f);
		item.Alignment = (HorizontalAlignment)0;
		((Control)item).TooltipText = tooltip;
		((BaseButton)item).Disabled = !enabled;
		((Control)item).MouseDefaultCursorShape = (CursorShape)2;
		((BaseButton)item).Pressed += delegate
		{
			CloseFabMenu();
			action();
		};
		((Control)item).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
		{
			if (input.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
			{
				CloseFabMenu();
				Button? fab = _fab;
				if (fab != null)
				{
					((Control)fab).GrabFocus();
				}
				((Control)item).AcceptEvent();
			}
		};
		return item;
	}

	private static void OpenDownloads()
	{
		if (_plan != null && !_pumpActive)
		{
			WorkshopDownloadsDialog.Show(OpenWorkshopItemPage, RequestWorkshopRemoval, CanRemoveWorkshopItem);
		}
	}

	private static void OpenWorkshopItemPage(ulong itemId, string name)
	{
		string text = $"https://steamcommunity.com/sharedfiles/filedetails/?id={itemId}";
		ShowFeedback(SteamOverlay.OpenPage(text) switch
		{
			PageOpenResult.InGame => PlanResult.Ok(MlmLocManager.Get("workshop.feedback.pageOpened", ("name", name))), 
			PageOpenResult.InSteam => PlanResult.Ok(MlmLocManager.Get("workshop.feedback.pageOpenedInSteam", ("name", name))), 
			PageOpenResult.InBrowser => PlanResult.Ok(MlmLocManager.Get("workshop.feedback.pageOpenedInBrowser", ("name", name))), 
			_ => PlanResult.Error(MlmLocManager.Get("workshop.feedback.browserOpenFailed", ("url", text))), 
		});
	}

	private static Button BuildFilterToggle()
	{
		_filtersCollapsed = MlmLocManager.FiltersCollapsed;
		_filterToggle = UiKit.Button(string.Empty, "MiniButton", 0f, 30f);
		((Control)_filterToggle).SizeFlagsVertical = (SizeFlags)4;
		((BaseButton)_filterToggle).Pressed += ToggleFilters;
		BindLocalization((Control)(object)_filterToggle, ApplyFilterCollapse);
		return _filterToggle;
	}

	private static Control BuildFilterSummary()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected O, but got Unknown
		_filterSummary = new HBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_filterSummary).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_filterSummaryText = UiKit.Label(string.Empty, "DimLabel");
		_filterSummaryText.VerticalAlignment = (VerticalAlignment)1;
		_filterSummaryText.TextOverrunBehavior = (OverrunBehavior)3;
		_filterSummaryText.ClipText = true;
		((Control)_filterSummaryText).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)_filterSummaryText).MouseFilter = (MouseFilterEnum)0;
		((Control)_filterSummaryText).MouseDefaultCursorShape = (CursorShape)2;
		Label? filterSummaryText = _filterSummaryText;
		object obj = _003C_003Ec._003C_003E9__485_0;
		if (obj == null)
		{
			GuiInputEventHandler val2 = delegate(InputEvent input)
			{
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Invalid comparison between Unknown and I8
				InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
				if (val != null && val.Pressed && (long)val.ButtonIndex == 1)
				{
					ToggleFilters();
				}
			};
			_003C_003Ec._003C_003E9__485_0 = val2;
			obj = (object)val2;
		}
		((Control)filterSummaryText).GuiInput += (GuiInputEventHandler)obj;
		_filterClear = UiKit.Button(string.Empty, "MiniButton", 0f, 30f);
		((BaseButton)_filterClear).Pressed += ClearFilterChoices;
		((Node)_filterSummary).AddChild((Node)(object)_filterSummaryText, false, (InternalMode)0);
		((Node)_filterSummary).AddChild((Node)(object)_filterClear, false, (InternalMode)0);
		return (Control)(object)_filterSummary;
	}

	private static void ToggleFilters()
	{
		if (!_pumpActive)
		{
			_filtersCollapsed = !_filtersCollapsed;
			if (!MlmLocManager.SetFiltersCollapsed(_filtersCollapsed, out string error))
			{
				GD.PushWarning("[ModLaunchManager] could not remember the folded filters: " + error);
			}
			ApplyFilterCollapse();
			Button? filterToggle = _filterToggle;
			if (filterToggle != null)
			{
				((Control)filterToggle).GrabFocus();
			}
		}
	}

	private static void ApplyFilterCollapse()
	{
		if (_filterRow != null && GodotObject.IsInstanceValid((GodotObject)(object)_filterRow))
		{
			((CanvasItem)_filterRow).Visible = !_filtersCollapsed;
		}
		if (_groupLine != null && GodotObject.IsInstanceValid((GodotObject)(object)_groupLine))
		{
			((CanvasItem)_groupLine).Visible = !_filtersCollapsed;
		}
		if (_filterSummary != null && GodotObject.IsInstanceValid((GodotObject)(object)_filterSummary))
		{
			((CanvasItem)_filterSummary).Visible = _filtersCollapsed;
		}
		if (_filterToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_filterToggle))
		{
			_filterToggle.Text = MlmLocManager.Get(_filtersCollapsed ? "filters.expand" : "filters.collapse");
			((Control)_filterToggle).TooltipText = MlmLocManager.Get(_filtersCollapsed ? "filters.expand.tooltip" : "filters.collapse.tooltip");
		}
		UpdateFilterSummary();
	}

	private static void UpdateFilterSummary()
	{
		if (_filterSummaryText != null && _filterClear != null && GodotObject.IsInstanceValid((GodotObject)(object)_filterSummaryText))
		{
			List<string> list = new List<string>();
			string text = _filter switch
			{
				ModFilter.Enabled => "mods.filter.enabled", 
				ModFilter.Disabled => "mods.filter.disabled", 
				ModFilter.Workshop => "mods.filter.workshop", 
				ModFilter.Local => "mods.filter.local", 
				ModFilter.Updates => "mods.filter.updates", 
				_ => null, 
			};
			if (text != null)
			{
				list.Add(MlmLocManager.Get(text));
			}
			if (_groupFilter != null)
			{
				list.Add(MlmLocManager.Get("filters.summary.group", ("name", (_groupFilter == "\u0001ungrouped") ? MlmLocManager.Get("filters.summary.ungrouped") : GroupPath(_groupFilter))));
			}
			_filterSummaryText.Text = MlmLocManager.Get("filters.summary", ("filters", (list.Count == 0) ? MlmLocManager.Get("filters.summary.none") : string.Join(" · ", list)));
			((Control)_filterSummaryText).TooltipText = MlmLocManager.Get("filters.summary.tooltip");
			((CanvasItem)_filterClear).Visible = list.Count > 0;
			_filterClear.Text = MlmLocManager.Get("filters.clear");
			((Control)_filterClear).TooltipText = MlmLocManager.Get("filters.clear.tooltip");
		}
	}

	private static void ClearFilterChoices()
	{
		_filter = ModFilter.All;
		if (FilterChips.Count > 0)
		{
			((BaseButton)FilterChips[0]).ButtonPressed = true;
		}
		_groupFilter = null;
		_groupDeleteArmed = false;
		QueueRebuild();
		Defer(RebuildGroupRow);
		UpdateFilterSummary();
	}

	private static void StartFriendModSync(Control root)
	{
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Expected O, but got Unknown
		if (_plan != null)
		{
			FriendModSync.Start(_plan, CurrentGameVersion(), OnFriendModSyncChanged);
			if (_friendSync != null && GodotObject.IsInstanceValid((GodotObject)(object)_friendSync))
			{
				((BaseButton)_friendSync).Disabled = _pumpActive || !FriendModSync.CanOpen;
				((Control)_friendSync).TooltipText = (FriendModSync.CanOpen ? MlmLocManager.Get("friendSync.action.open.tooltip") : MlmLocManager.Get("friendSync.unavailable", ("error", FriendModSync.AvailabilityError ?? MlmLocManager.Get("common.value.unknown"))));
			}
			_friendSyncTimer = new Timer
			{
				WaitTime = 0.5,
				Autostart = true
			};
			_friendSyncTimer.Timeout += delegate
			{
				FriendModSync.Poll(0.5);
			};
			((Node)root).AddChild((Node)(object)_friendSyncTimer, false, (InternalMode)0);
		}
	}

	private static void OnFriendModSyncChanged()
	{
		if (_friendSync != null && GodotObject.IsInstanceValid((GodotObject)(object)_friendSync))
		{
			((BaseButton)_friendSync).Disabled = _pumpActive || !FriendModSync.CanOpen;
			((Control)_friendSync).TooltipText = (FriendModSync.CanOpen ? MlmLocManager.Get("friendSync.action.open.tooltip") : MlmLocManager.Get("friendSync.unavailable", ("error", FriendModSync.AvailabilityError ?? MlmLocManager.Get("common.value.unknown"))));
		}
		FriendModSyncDialog.Update();
	}

	private static void OpenFriendModSync()
	{
		if (_plan == null || _pumpActive || !FriendModSync.CanOpen)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("friendSync.unavailable", ("error", FriendModSync.AvailabilityError ?? MlmLocManager.Get("common.value.unknown")))));
		}
		else
		{
			FriendModSyncDialog.Show(_plan, ApplyFriendProfile);
		}
	}

	private static PlanResult ApplyFriendProfile(LaunchProfile profile)
	{
		if (_plan == null || _pumpActive)
		{
			return PlanResult.Error(MlmLocManager.Get("friendSync.apply.unavailable"));
		}
		RecordUndo();
		PlanResult result = _plan.ApplyProfile(profile);
		ShowFeedback(result);
		if (result.Success)
		{
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
			Defer(RebuildProfileRow);
		}
		return result;
	}

	private static void LoadGroupViewPreferences()
	{
		_groupListView = MlmLocManager.GroupListView;
		CollapsedFolders.Clear();
		CollapsedFolders.UnionWith(MlmLocManager.CollapsedGroups);
		_revealSelectionAfterSuspend = false;
		_stickyFolderKey = null;
		_treeItemTop = -1f;
		_titlesPinned = false;
		_titlesPinnedPalette = null;
	}

	private static Button BuildGroupViewToggle()
	{
		_groupViewToggle = UiKit.Button(string.Empty, "MiniButton", 0f, 30f);
		((Control)_groupViewToggle).SizeFlagsVertical = (SizeFlags)0;
		((BaseButton)_groupViewToggle).Pressed += ToggleGroupView;
		BindLocalization((Control)(object)_groupViewToggle, UpdateGroupViewToggle);
		return _groupViewToggle;
	}

	private static Button BuildFoldAllButton()
	{
		_foldAllButton = UiKit.Button(string.Empty, "MiniButton", 0f, 30f);
		((Control)_foldAllButton).SizeFlagsVertical = (SizeFlags)0;
		((BaseButton)_foldAllButton).Pressed += delegate
		{
			SetAllFoldersCollapsed(AnyFolderOpen());
		};
		BindLocalization((Control)(object)_foldAllButton, UpdateFoldAllButton);
		return _foldAllButton;
	}

	private static bool AnyFolderOpen()
	{
		return FolderHeaders().Any((TreeItem header) => !header.Collapsed && header.GetFirstChild() != null);
	}

	private static void UpdateFoldAllButton()
	{
		if (_foldAllButton != null && GodotObject.IsInstanceValid((GodotObject)(object)_foldAllButton))
		{
			bool flag = AnyFolderOpen();
			((CanvasItem)_foldAllButton).Visible = _groupListView;
			_foldAllButton.Text = MlmLocManager.Get(flag ? "groups.menu.collapseAll" : "groups.menu.expandAll");
			((Control)_foldAllButton).TooltipText = MlmLocManager.Get("groups.action.foldAll.tooltip");
			((BaseButton)_foldAllButton).Disabled = _pumpActive || FolderFoldingSuspended() || (!flag && !FolderHeaders().Any((TreeItem header) => header.Collapsed));
		}
	}

	private static void UpdateGroupViewToggle()
	{
		if (_groupViewToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_groupViewToggle))
		{
			_groupViewToggle.Text = MlmLocManager.Get(_groupListView ? "groups.view.toPages" : "groups.view.toList");
			((Control)_groupViewToggle).TooltipText = MlmLocManager.Get(_groupListView ? "groups.view.toPages.tooltip" : "groups.view.toList.tooltip");
			((BaseButton)_groupViewToggle).Disabled = _pumpActive;
		}
		UpdateFoldAllButton();
	}

	private static void ToggleGroupView()
	{
		if (!_pumpActive)
		{
			_groupListView = !_groupListView;
			if (!MlmLocManager.SetGroupListView(_groupListView, out string error))
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("groups.view.saveFailed", ("error", error))));
			}
			CancelInlineRename();
			UpdateGroupViewToggle();
			ConfigureTreeColumns();
			QueueRebuild(scrollToSelection: true);
		}
	}

	private static bool IsFolderKey(string key)
	{
		return key.StartsWith("folder:", StringComparison.Ordinal);
	}

	private static string FolderKey(string group)
	{
		return "folder:" + ((group.Length == 0) ? "\u0001ungrouped" : group);
	}

	private static string? FolderGroup(string key)
	{
		if (!IsFolderKey(key))
		{
			return null;
		}
		string text = key.Substring("folder:".Length);
		if (!(text == "\u0001ungrouped"))
		{
			return text;
		}
		return string.Empty;
	}

	private static string? FolderOfRow(string key)
	{
		if (!IsFolderKey(key))
		{
			return _plan?.Find(key)?.Group.Trim();
		}
		return FolderGroup(key);
	}

	private static bool SameGroup(string? left, string? right)
	{
		return string.Equals(left?.Trim() ?? string.Empty, right?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase);
	}

	private static bool MixedStatesInterleave()
	{
		if (_groupListView)
		{
			return LaunchPlan.KeepDisabledInPlace;
		}
		return false;
	}

	private static bool ShareMoveRange(LaunchEntry left, LaunchEntry right)
	{
		if (MixedStatesInterleave() || left.Enabled == right.Enabled)
		{
			if (_groupListView)
			{
				return SameGroup(left.Group, right.Group);
			}
			return true;
		}
		return false;
	}

	private static string FolderStateName(string group)
	{
		if (group.Length != 0)
		{
			return group;
		}
		return "\u0001ungrouped";
	}

	private static List<(string Name, int Depth)> GroupTreeNames()
	{
		List<(string, int)> list = _organization.GroupTree();
		foreach (LaunchEntry item in _plan?.Entries ?? new List<LaunchEntry>())
		{
			string group = item.Group.Trim();
			if (group.Length > 0 && !list.Any<(string, int)>(((string Name, int Depth) node) => SameGroup(node.Name, group)))
			{
				list.Add((group, 0));
			}
		}
		return list;
	}

	private static string ParentGroupOf(string group)
	{
		return _organization.ParentOf(group);
	}

	private static string GroupPath(string group)
	{
		int num = 1;
		List<string> list = new List<string>(num);
		CollectionsMarshal.SetCount(list, num);
		CollectionsMarshal.AsSpan(list)[0] = group;
		List<string> list2 = list;
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { group };
		string text = ParentGroupOf(group);
		while (text.Length > 0 && hashSet.Add(text))
		{
			list2.Insert(0, text);
			text = ParentGroupOf(text);
		}
		return string.Join(" / ", list2);
	}

	private static bool InGroup(string entryGroup, string group)
	{
		if (group.Trim().Length != 0)
		{
			if (entryGroup.Trim().Length > 0)
			{
				return _organization.IsWithin(entryGroup, group);
			}
			return false;
		}
		return entryGroup.Trim().Length == 0;
	}

	private static List<string> SiblingGroups(string group)
	{
		string parent = ParentGroupOf(group);
		return (from node in GroupTreeNames()
			select node.Name into name
			where SameGroup(ParentGroupOf(name), parent)
			select name).ToList();
	}

	private static bool FolderFoldingSuspended()
	{
		LineEdit? search = _search;
		if (string.IsNullOrWhiteSpace((search != null) ? search.Text : null) && _filter != ModFilter.Updates && _groupFilter == null)
		{
			return _controller?.ControllerMode ?? false;
		}
		return true;
	}

	private static bool AddGroupFolders(TreeItem root, Func<LaunchEntry, bool> shown, Func<LaunchEntry, bool> matches, bool revealAll, string? selectedKey, string query)
	{
		List<LaunchEntry> entries = _plan.Entries;
		bool showEmpty = revealAll || (string.IsNullOrWhiteSpace(query) && _filter == ModFilter.All);
		List<(string, int)> list = GroupTreeNames();
		Dictionary<string, List<string>> children = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase) { [string.Empty] = new List<string>() };
		foreach (var item5 in list)
		{
			string item = item5.Item1;
			children[item] = new List<string>();
		}
		foreach (var item6 in list)
		{
			string item2 = item6.Item1;
			children.GetValueOrDefault(ParentGroupOf(item2), children[string.Empty]).Add(item2);
		}
		Dictionary<string, List<int>> direct = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
		for (int i = 0; i < entries.Count; i++)
		{
			if (shown(entries[i]))
			{
				string key = entries[i].Group.Trim();
				if (!direct.TryGetValue(key, out List<int> value))
				{
					value = (direct[key] = new List<int>());
				}
				value.Add(i);
			}
		}
		string chosen = (revealAll ? null : _groupFilter);
		if (chosen == "\u0001ungrouped")
		{
			return AddFolder(root, string.Empty, 0);
		}
		if (chosen != null)
		{
			return AddFolder(root, list.Select<(string, int), string>(((string Name, int Depth) node) => node.Name).FirstOrDefault((string name) => SameGroup(name, chosen)) ?? chosen, 0);
		}
		foreach (string item7 in children[string.Empty])
		{
			if (!AddFolder(root, item7, 0))
			{
				return false;
			}
		}
		return AddFolder(root, string.Empty, 0);
		bool AddFolder(TreeItem parent, string group, int depth)
		{
			List<LaunchEntry> list3 = SubtreeRows(group);
			List<string> list4 = ((group.Length == 0) ? new List<string>() : (children.GetValueOrDefault(group) ?? new List<string>()));
			if (list3.Count == 0 && (group.Length == 0 || !showEmpty))
			{
				return true;
			}
			bool hasChildren = list3.Count > 0 || (showEmpty && list4.Count > 0);
			TreeItem val = AddGroupFolderHeader(parent, group, list3, depth, hasChildren);
			if (val == null)
			{
				return false;
			}
			List<(int, int, string, int)> list5 = new List<(int, int, string, int)>();
			foreach (string item8 in list4)
			{
				List<int> list6 = SubtreeIndices(item8);
				list5.Add(((list6.Count > 0) ? list6[0] : int.MaxValue, list5.Count, item8, -1));
			}
			foreach (int item9 in direct.GetValueOrDefault(group) ?? new List<int>())
			{
				list5.Add((item9, list5.Count, null, item9));
			}
			list5.Sort(((int Position, int Order, string? Folder, int Index) left, (int Position, int Order, string? Folder, int Index) right) => (left.Position == right.Position) ? left.Order.CompareTo(right.Order) : left.Position.CompareTo(right.Position));
			foreach (var item10 in list5)
			{
				string item3 = item10.Item3;
				int item4 = item10.Item4;
				if (item3 != null)
				{
					if (!AddFolder(val, item3, depth + 1))
					{
						return false;
					}
				}
				else
				{
					LaunchEntry launchEntry = entries[item4];
					if (!AddModRow(val, launchEntry, item4, revealAll && matches(launchEntry), selectedKey, RowIndent(depth + 1)))
					{
						return false;
					}
				}
			}
			return true;
		}
		void Collect(string name)
		{
			P_1.indices.AddRange(direct.GetValueOrDefault(name) ?? new List<int>());
			if (name.Length > 0)
			{
				foreach (string item11 in children.GetValueOrDefault(name) ?? new List<string>())
				{
					Collect(item11);
				}
			}
		}
		List<int> SubtreeIndices(string group)
		{
			List<int> indices = new List<int>();
			Collect(group);
			indices.Sort();
			return indices;
		}
		List<LaunchEntry> SubtreeRows(string group)
		{
			return (from index in SubtreeIndices(@group)
				select entries[index]).ToList();
		}
	}

	private static TreeItem? AddGroupFolderHeader(TreeItem parent, string group, IReadOnlyList<LaunchEntry> rows, int depth, bool hasChildren)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		ModReorderTree? modTree = _modTree;
		TreeItem val = ((modTree != null) ? ((Tree)modTree).CreateItem(parent, -1) : null);
		if (val == null)
		{
			return null;
		}
		bool flag = group.Length > 0;
		bool collapsed = hasChildren && CollapsedFolders.Contains(FolderStateName(group)) && !FolderFoldingSuspended();
		val.SetMetadata(0, Variant.op_Implicit(FolderKey(group)));
		val.Collapsed = collapsed;
		val.SetCustomMinimumHeight(32);
		if (flag)
		{
			val.SetText(0, "≡");
			val.SetTextAlignment(0, (HorizontalAlignment)1);
			val.SetCustomColor(0, _pumpActive ? UiKit.TextDim : UiKit.TextMuted);
		}
		if (rows.Count > 0)
		{
			val.SetCellMode(1, (TreeCellMode)3);
			val.SetIcon(1, FolderToggleIcon(rows));
		}
		val.SetIcon(2, hasChildren ? FoldIcon(collapsed, depth) : FoldSpacer(depth));
		val.SetText(2, FolderTitle(group, rows.Count));
		val.SetExpandRight(2, true);
		val.SetTextOverrunBehavior(2, (OverrunBehavior)3);
		val.SetText(6, FolderStatus(rows));
		val.SetTextAlignment(6, (HorizontalAlignment)1);
		string text = MlmLocManager.Get(flag ? "groups.folder.tooltip" : "groups.folder.ungroupedTooltip");
		Color val2 = UiKit.RowTint(flag ? UiKit.Accent : UiKit.TextMuted, flag ? 0.09f : 0.07f);
		for (int i = 0; i < ColumnCount; i++)
		{
			val.SetSelectable(i, false);
			val.SetCustomBgColor(i, val2, false);
			val.SetTooltipText(i, text);
		}
		Color val3 = (rows.Any((LaunchEntry entry) => entry.Enabled) ? UiKit.Text : UiKit.TextMuted);
		val.SetCustomColor(2, val3);
		val.SetMetadata(2, Variant.op_Implicit(val3));
		val.SetCustomColor(6, UiKit.TextMuted);
		return val;
	}

	private static string FolderTitleName(string group)
	{
		if (group.Length != 0)
		{
			return GroupPath(group);
		}
		return MlmLocManager.Get("groups.option.ungrouped");
	}

	private static string FolderTitle(string group, int count)
	{
		if (group.Length != 0)
		{
			return $"{group} · {count}";
		}
		return MlmLocManager.Get("groups.filter.ungrouped", ("count", count));
	}

	private static string FolderStatus(IReadOnlyList<LaunchEntry> rows)
	{
		if (rows.Count != 0)
		{
			return MlmLocManager.Get("groups.folder.enabledCount", ("enabled", rows.Count((LaunchEntry entry) => entry.Enabled)), ("total", rows.Count));
		}
		return MlmLocManager.Get("groups.folder.empty");
	}

	private static Texture2D? FolderToggleIcon(IReadOnlyList<LaunchEntry> rows)
	{
		if (_modTree == null)
		{
			return null;
		}
		bool flag = !_pumpActive && rows.Any((LaunchEntry entry) => entry.CanToggle);
		int num = rows.Count((LaunchEntry entry) => entry.Enabled);
		string text = ((num == 0) ? "unchecked" : ((num == rows.Count) ? "checked" : "indeterminate"));
		return ((Control)_modTree).GetThemeIcon(StringName.op_Implicit(flag ? text : (text + "_disabled")), StringName.op_Implicit("Tree"));
	}

	private static Texture2D FoldIcon(bool collapsed, int depth)
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		RefreshFoldIconStyle();
		if (!FoldIcons.TryGetValue((collapsed, depth), out Texture2D value))
		{
			int indent = depth * _foldIconStyle.Step;
			value = (Texture2D)(object)(collapsed ? UiGlyphs.ChevronRight(_foldIconStyle.Size, _foldIconStyle.Color, 1.8f, indent) : UiGlyphs.Chevron(_foldIconStyle.Size, _foldIconStyle.Color, 1.8f, up: false, indent));
			FoldIcons[(collapsed, depth)] = value;
		}
		return value;
	}

	private static Texture2D FoldSpacer(int depth)
	{
		RefreshFoldIconStyle();
		return IndentSpacer(depth * _foldIconStyle.Step + _foldIconStyle.Size);
	}

	private static Texture2D? RowIndent(int depth)
	{
		if (depth <= 0)
		{
			return null;
		}
		RefreshFoldIconStyle();
		return IndentSpacer(depth * _foldIconStyle.Step - (_foldIconStyle.Step - _foldIconStyle.Size));
	}

	private static Texture2D IndentSpacer(int width)
	{
		if (!IndentSpacers.TryGetValue(width, out Texture2D value))
		{
			value = (Texture2D)(object)ImageTexture.CreateFromImage(Image.CreateEmpty(Math.Max(1, width), 1, false, (Format)5));
			IndentSpacers[width] = value;
		}
		return value;
	}

	private static void RefreshFoldIconStyle()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		int num = Math.Max(12, UiKit.ScaleFontSize(14));
		ModReorderTree? modTree = _modTree;
		int num2 = ((modTree != null) ? ((Control)modTree).GetThemeConstant(StringName.op_Implicit("h_separation"), (StringName)null) : 4);
		(Color, int, int) tuple = (UiKit.TextMuted, num, num + num2);
		(Color, int, int) foldIconStyle = _foldIconStyle;
		(Color, int, int) tuple2 = tuple;
		if (foldIconStyle.Item1 != tuple2.Item1 || foldIconStyle.Item2 != tuple2.Item2 || foldIconStyle.Item3 != tuple2.Item3)
		{
			FoldIcons.Clear();
			IndentSpacers.Clear();
			_foldIconStyle = tuple;
		}
	}

	private static int FolderDepth(TreeItem header)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (TreeItem parent = header.GetParent(); parent != null; parent = parent.GetParent())
		{
			Variant metadata = parent.GetMetadata(0);
			if (!IsFolderKey(((Variant)(ref metadata)).AsString()))
			{
				break;
			}
			num++;
		}
		return num;
	}

	private static bool InFoldedFolder(TreeItem item)
	{
		for (TreeItem parent = item.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent.Collapsed)
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<TreeItem> FolderHeaders()
	{
		ModReorderTree? modTree = _modTree;
		object first2;
		if (modTree == null)
		{
			first2 = null;
		}
		else
		{
			TreeItem root = ((Tree)modTree).GetRoot();
			first2 = ((root != null) ? root.GetFirstChild() : null);
		}
		return Walk((TreeItem?)first2);
		[IteratorStateMachine(typeof(_003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed))]
		static IEnumerable<TreeItem> Walk(TreeItem? first)
		{
			//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
			return new _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed(-2)
			{
				_003C_003E3__first = first
			};
		}
	}

	private static TreeItem? FindFolderHeader(string key)
	{
		return FolderHeaders().FirstOrDefault((Func<TreeItem, bool>)delegate(TreeItem item)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			Variant metadata = item.GetMetadata(0);
			return string.Equals(((Variant)(ref metadata)).AsString(), key, StringComparison.Ordinal);
		});
	}

	private static void OnTreeHeaderActivated(string key)
	{
		if (_rebuildingTree)
		{
			return;
		}
		string text = FolderGroup(key);
		if (text != null)
		{
			TreeItem val = FindFolderHeader(key);
			if (val != null && val.GetFirstChild() != null)
			{
				SetFolderCollapsed(val, text, !val.Collapsed);
			}
		}
	}

	private static void SetFolderCollapsed(TreeItem header, string group, bool collapsed)
	{
		if (!FolderFoldingSuspended() && (collapsed ? CollapsedFolders.Add(FolderStateName(group)) : CollapsedFolders.Remove(FolderStateName(group))) && !MlmLocManager.SetCollapsedGroups(CollapsedFolders, out string error))
		{
			GD.PushWarning("[ModLaunchManager] folded groups could not be saved: " + error);
		}
		header.Collapsed = collapsed;
		header.SetIcon(2, FoldIcon(collapsed, FolderDepth(header)));
		if (!collapsed)
		{
			ReselectVisibleRow();
		}
		MarkFoldedSelection();
		UpdateFoldAllButton();
		Defer(UpdateStickyHeader);
	}

	private static void ReselectVisibleRow()
	{
		if (_selected != null)
		{
			TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding binding) => binding.Entry == _selected);
			if ((object)treeBinding != null && !InFoldedFolder(treeBinding.Item) && !treeBinding.Item.IsSelected(2))
			{
				treeBinding.Item.Select(2);
			}
		}
	}

	private static void MarkFoldedSelection()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Invalid comparison between Unknown and I8
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		if (!_groupListView || _modTree == null || !GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
		{
			return;
		}
		HashSet<TreeItem> hashSet = new HashSet<TreeItem>();
		if (_selected != null)
		{
			TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding binding) => binding.Entry == _selected);
			if ((object)treeBinding != null)
			{
				for (TreeItem parent = treeBinding.Item.GetParent(); parent != null; parent = parent.GetParent())
				{
					hashSet.Add(parent);
				}
			}
		}
		foreach (TreeItem item in FolderHeaders())
		{
			Variant metadata = item.GetMetadata(2);
			Color val = (((long)((Variant)(ref metadata)).VariantType == 20) ? ((Variant)(ref metadata)).AsColor() : UiKit.Text);
			item.SetCustomColor(2, (item.Collapsed && hashSet.Contains(item)) ? UiKit.Accent : val);
		}
	}

	private static void RenameFolderState(string from, string? to)
	{
		if (CollapsedFolders.Remove(from.Trim()))
		{
			if (!string.IsNullOrWhiteSpace(to))
			{
				CollapsedFolders.Add(to.Trim());
			}
			MlmLocManager.SetCollapsedGroups(CollapsedFolders, out string _);
		}
	}

	private static bool RevealFolder(string group)
	{
		string text = group.Trim();
		bool flag = text.Length == 0 && CollapsedFolders.Remove("\u0001ungrouped");
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		string text2 = text;
		while (text2.Length > 0 && hashSet.Add(text2))
		{
			flag |= CollapsedFolders.Remove(text2);
			text2 = ParentGroupOf(text2);
		}
		if (flag && !MlmLocManager.SetCollapsedGroups(CollapsedFolders, out string error))
		{
			GD.PushWarning("[ModLaunchManager] folded groups could not be saved: " + error);
		}
		return flag;
	}

	private static void RevealSelectionAfterSuspend()
	{
		if (_revealSelectionAfterSuspend && !FolderFoldingSuspended())
		{
			_revealSelectionAfterSuspend = false;
			if (_selected != null)
			{
				RevealFolder(_selected.Group);
			}
		}
	}

	private static void SetAllFoldersCollapsed(bool collapsed)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (!_groupListView || FolderFoldingSuspended())
		{
			return;
		}
		if (collapsed)
		{
			foreach (TreeItem item in FolderHeaders())
			{
				if (item.GetFirstChild() != null)
				{
					Variant metadata = item.GetMetadata(0);
					string text = FolderGroup(((Variant)(ref metadata)).AsString());
					if (text != null)
					{
						CollapsedFolders.Add(FolderStateName(text));
					}
				}
			}
		}
		else
		{
			CollapsedFolders.Clear();
		}
		if (!MlmLocManager.SetCollapsedGroups(CollapsedFolders, out string error))
		{
			GD.PushWarning("[ModLaunchManager] folded groups could not be saved: " + error);
		}
		QueueRebuild();
	}

	private static void ToggleFolder(string key)
	{
		if (_plan == null || _pumpActive)
		{
			return;
		}
		string group = FolderGroup(key);
		if (group == null)
		{
			return;
		}
		List<LaunchEntry> list = (from binding in TreeBindings
			select binding.Entry into entry
			where InGroup(entry.Group, @group)
			select entry).ToList();
		List<LaunchEntry> list2 = list.Where((LaunchEntry entry) => entry.CanToggle).ToList();
		if (list2.Count == 0)
		{
			return;
		}
		string key2 = FolderStateName(group);
		bool flag = list2.All((LaunchEntry entry) => entry.Enabled);
		bool flag2 = list2.All((LaunchEntry entry) => !entry.Enabled);
		FolderToggleMemory.TryGetValue(key2, out Dictionary<string, bool> remembered);
		RecordUndo();
		PlanResult planResult;
		if (!flag && !flag2)
		{
			Dictionary<string, bool> value = list2.ToDictionary<LaunchEntry, string, bool>((LaunchEntry entry) => entry.Key, (LaunchEntry entry) => entry.Enabled, StringComparer.Ordinal);
			FolderToggleMemory.Clear();
			FolderToggleMemory[key2] = value;
			planResult = _plan.SetEnabledForAll(list, enabled: true);
		}
		else if (flag)
		{
			planResult = _plan.SetEnabledForAll(list, enabled: false);
			if (remembered != null && planResult.Success)
			{
				planResult = planResult with
				{
					Message = MlmLocManager.Get("common.twoSentences", ("first", planResult.Message), ("second", MlmLocManager.Get("groups.folder.toggle.restoreNext")))
				};
			}
		}
		else if (remembered != null && list2.Any((LaunchEntry entry) => remembered.GetValueOrDefault(entry.Key)))
		{
			FolderToggleMemory.Remove(key2);
			planResult = _plan.SetEnabledForAll(list2.Where((LaunchEntry entry) => remembered.GetValueOrDefault(entry.Key)).ToList(), enabled: true);
			if (planResult.Success)
			{
				planResult = planResult with
				{
					Message = MlmLocManager.Get("common.twoSentences", ("first", MlmLocManager.Get("groups.folder.toggle.restored", ("name", FolderTitleName(group)))), ("second", planResult.Message))
				};
			}
		}
		else
		{
			planResult = _plan.SetEnabledForAll(list, enabled: true);
		}
		ShowFeedback(planResult);
		UpdateDetails();
		UpdatePlanSummary();
		QueueRebuild();
	}

	private static bool CanDropFolder(string draggedKey, string targetKey)
	{
		if (_groupListView && !_pumpActive)
		{
			string text = FolderGroup(draggedKey);
			if (text != null && text.Length > 0)
			{
				string text2 = FolderOfRow(targetKey);
				if (text2 != null)
				{
					if (text2.Length != 0)
					{
						return !_organization.IsWithin(text2, text);
					}
					return true;
				}
			}
		}
		return false;
	}

	private static void DropFolder(string draggedKey, string targetKey)
	{
		if (!CanDropFolder(draggedKey, targetKey))
		{
			return;
		}
		string group = FolderGroup(draggedKey);
		if (group == null)
		{
			return;
		}
		string target = FolderOfRow(targetKey);
		if (target == null)
		{
			return;
		}
		if (!IsFolderKey(targetKey) && target.Length > 0)
		{
			NestGroup(group, target);
			return;
		}
		List<string> list = KnownGroupNames();
		int from = list.FindIndex((string name) => SameGroup(name, group));
		int to = ((target.Length == 0) ? list.Count : list.FindIndex((string name) => SameGroup(name, target)));
		bool moved = false;
		if (from >= 0 && to >= 0 && MutateOrganization(delegate
		{
			moved = _organization.MoveGroup(group, (target.Length == 0) ? null : target, from < to);
		}))
		{
			if (moved)
			{
				ReportGroupPosition(group);
			}
			RefreshAfterGroupChange();
		}
	}

	private static void DropFolderAt(string draggedKey, string targetKey, TreeDropPlacement placement)
	{
		if (!CanDropFolder(draggedKey, targetKey))
		{
			return;
		}
		string group = FolderGroup(draggedKey);
		if (group == null)
		{
			return;
		}
		string target = FolderOfRow(targetKey);
		if (target == null)
		{
			return;
		}
		if (target.Length == 0)
		{
			DropFolder(draggedKey, targetKey);
			return;
		}
		if (placement == TreeDropPlacement.Into || !IsFolderKey(targetKey))
		{
			NestGroup(group, target);
			return;
		}
		bool moved = false;
		if (MutateOrganization(delegate
		{
			moved = _organization.MoveGroup(group, target, placement == TreeDropPlacement.After);
		}))
		{
			if (moved)
			{
				ReportGroupPosition(group);
			}
			RefreshAfterGroupChange();
		}
	}

	private static void ReportGroupPosition(string group)
	{
		int num = SiblingGroups(group).FindIndex((string name) => SameGroup(name, group)) + 1;
		string text = ParentGroupOf(group);
		ShowFeedback(PlanResult.Ok((text.Length == 0) ? MlmLocManager.Get("groups.move.success", ("name", group), ("position", num)) : MlmLocManager.Get("groups.move.successNested", ("name", group), ("parent", GroupPath(text)), ("position", num))));
	}

	private static void NestGroup(string group, string parent)
	{
		if (_pumpActive)
		{
			return;
		}
		bool moved = false;
		if (!MutateOrganization(delegate
		{
			moved = _organization.SetParent(group, parent);
		}))
		{
			return;
		}
		if (moved)
		{
			if (parent.Length > 0)
			{
				RevealFolder(parent);
			}
			ShowFeedback(PlanResult.Ok((parent.Length == 0) ? MlmLocManager.Get("groups.nest.topLevel", ("name", group)) : MlmLocManager.Get("groups.nest.success", ("name", group), ("parent", GroupPath(parent)))));
		}
		RefreshAfterGroupChange();
	}

	private static void MoveGroupAmongSiblings(string group, int delta)
	{
		if (_pumpActive)
		{
			return;
		}
		List<string> siblings = SiblingGroups(group);
		int num = siblings.FindIndex((string name) => SameGroup(name, group));
		int target = num + delta;
		if (num < 0 || target < 0 || target >= siblings.Count)
		{
			return;
		}
		bool moved = false;
		if (MutateOrganization(delegate
		{
			moved = _organization.MoveGroup(group, siblings[target], delta > 0);
		}))
		{
			if (moved)
			{
				ReportGroupPosition(group);
			}
			RefreshAfterGroupChange();
		}
	}

	private static void BuildFolderMenu(Control owner)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		_folderMenu = Menu();
		_folderMoveMenu = Menu();
		PopupMenu? folderMenu = _folderMenu;
		object obj = _003C_003EO._003C101_003E__OnFolderMenuIdPressed;
		if (obj == null)
		{
			IdPressedEventHandler val = OnFolderMenuIdPressed;
			_003C_003EO._003C101_003E__OnFolderMenuIdPressed = val;
			obj = (object)val;
		}
		folderMenu.IdPressed += (IdPressedEventHandler)obj;
		PopupMenu? folderMoveMenu = _folderMoveMenu;
		object obj2 = _003C_003EO._003C102_003E__OnFolderMoveMenuIdPressed;
		if (obj2 == null)
		{
			IdPressedEventHandler val2 = OnFolderMoveMenuIdPressed;
			_003C_003EO._003C102_003E__OnFolderMoveMenuIdPressed = val2;
			obj2 = (object)val2;
		}
		folderMoveMenu.IdPressed += (IdPressedEventHandler)obj2;
		((Node)_folderMenu).AddChild((Node)(object)_folderMoveMenu, false, (InternalMode)0);
		((Node)owner).AddChild((Node)(object)_folderMenu, false, (InternalMode)0);
		static PopupMenu Menu()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			PopupMenu menu = new PopupMenu();
			BindPalette((GodotObject)(object)menu, delegate
			{
				((Window)menu).AddThemeFontOverride(StringName.op_Implicit("font"), (Font)(object)UiKit.UiFont);
				((Window)menu).AddThemeFontOverride(StringName.op_Implicit("font_separator"), (Font)(object)UiKit.UiFont);
			});
			((Window)menu).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(14));
			((Window)menu).AddThemeFontSizeOverride(StringName.op_Implicit("font_separator_size"), UiKit.ScaleFontSize(12));
			return menu;
		}
	}

	private static void ShowFolderMenu(string group, Vector2 screenPosition)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		if (_folderMenu == null || _folderMoveMenu == null || !GodotObject.IsInstanceValid((GodotObject)(object)_folderMenu) || _pumpActive || (group.Length == 0 && !_groupListView))
		{
			return;
		}
		_folderMenuGroup = group;
		_folderMenuPosition = screenPosition;
		PopupMenu folderMenu = _folderMenu;
		folderMenu.Clear();
		folderMenu.AddSeparator((group.Length == 0) ? MlmLocManager.Get("groups.option.ungrouped") : GroupPath(group), -1);
		if (group.Length > 0)
		{
			AddGroupMenuItems(folderMenu, _folderMoveMenu, group);
		}
		if (_groupListView)
		{
			if (group.Length > 0)
			{
				folderMenu.AddSeparator("", -1);
			}
			bool flag = FolderFoldingSuspended();
			List<TreeItem> source = FolderHeaders().ToList();
			folderMenu.AddItem(MlmLocManager.Get("groups.menu.collapseAll"), 6, (Key)0);
			folderMenu.SetItemDisabled(folderMenu.ItemCount - 1, flag || !source.Any((TreeItem header) => !header.Collapsed && header.GetFirstChild() != null));
			folderMenu.AddItem(MlmLocManager.Get("groups.menu.expandAll"), 7, (Key)0);
			folderMenu.SetItemDisabled(folderMenu.ItemCount - 1, flag || !source.Any((TreeItem header) => header.Collapsed));
		}
		if (group.Length > 0)
		{
			folderMenu.AddSeparator("", -1);
			folderMenu.AddItem(MlmLocManager.Get(IsGroupDeleteArmed(group) ? "common.action.confirmDelete" : "groups.menu.delete"), 5, (Key)0);
		}
		((Window)folderMenu).ResetSize();
		((Window)folderMenu).Position = new Vector2I((int)screenPosition.X, (int)screenPosition.Y);
		((Window)folderMenu).Popup((Rect2I?)null);
		if (group.Length > 0 && IsGroupDeleteArmed(group))
		{
			folderMenu.SetFocusedItem(folderMenu.ItemCount - 1);
		}
	}

	private static void AddGroupMenuItems(PopupMenu menu, PopupMenu moveMenu, string group)
	{
		List<string> list = SiblingGroups(group);
		int num = list.FindIndex((string name) => SameGroup(name, group));
		menu.AddItem(MlmLocManager.Get("groups.menu.newChild"), 1, (Key)0);
		menu.AddItem(MlmLocManager.Get("groups.menu.rename"), 2, (Key)0);
		menu.AddItem(MlmLocManager.Get("groups.menu.moveUp"), 3, (Key)0);
		menu.SetItemDisabled(menu.ItemCount - 1, num <= 0);
		menu.AddItem(MlmLocManager.Get("groups.menu.moveDown"), 4, (Key)0);
		menu.SetItemDisabled(menu.ItemCount - 1, num < 0 || num >= list.Count - 1);
		moveMenu.Clear();
		FolderMoveTargets.Clear();
		string parent = ParentGroupOf(group);
		AddTarget(MlmLocManager.Get("groups.menu.topLevel"), string.Empty);
		foreach (var item2 in GroupTreeNames())
		{
			string item = item2.Name;
			if (!_organization.IsWithin(item, group))
			{
				AddTarget(GroupPath(item), item);
			}
		}
		menu.AddSubmenuNodeItem(MlmLocManager.Get("groups.menu.moveInto"), moveMenu, -1);
		void AddTarget(string label, string target)
		{
			moveMenu.AddRadioCheckItem(label, FolderMoveTargets.Count, (Key)0);
			moveMenu.SetItemChecked(moveMenu.ItemCount - 1, SameGroup(parent, target));
			FolderMoveTargets.Add(target);
		}
	}

	private static void OnFolderMenuIdPressed(long id)
	{
		string group = _folderMenuGroup;
		if (group == null)
		{
			return;
		}
		Defer(delegate
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			long num = id;
			long num2 = num - 1;
			if ((ulong)num2 <= 6uL)
			{
				switch (num2)
				{
				case 0L:
					BeginCreateGroup(Array.Empty<LaunchEntry>(), group);
					break;
				case 1L:
					if (!BeginInlineRename(group))
					{
						BeginRenameGroup(group);
					}
					break;
				case 2L:
					MoveGroupAmongSiblings(group, -1);
					break;
				case 3L:
					MoveGroupAmongSiblings(group, 1);
					break;
				case 4L:
					DeleteGroup(group);
					if (IsGroupDeleteArmed(group))
					{
						ShowFolderMenu(group, _folderMenuPosition);
					}
					break;
				case 5L:
				case 6L:
					SetAllFoldersCollapsed(id == 6);
					break;
				}
			}
		});
	}

	private static void OnFolderMoveMenuIdPressed(long id)
	{
		int num = checked((int)id);
		string group = _folderMenuGroup;
		if (group != null && num >= 0 && num < FolderMoveTargets.Count)
		{
			string target = FolderMoveTargets[num];
			Defer(delegate
			{
				NestGroup(group, target);
			});
		}
	}

	private static void BuildStickyFolderHeader(ModReorderTree tree)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Expected O, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Expected O, but got Unknown
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Expected O, but got Unknown
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Expected O, but got Unknown
		_stickyFolderKey = null;
		_treeItemTop = -1f;
		_stickyClip = new Control
		{
			Visible = false,
			ClipContents = true,
			MouseFilter = (MouseFilterEnum)2
		};
		_stickyHeader = new Panel
		{
			MouseFilter = (MouseFilterEnum)1,
			MouseDefaultCursorShape = (CursorShape)2
		};
		_stickyIcon = new TextureRect
		{
			StretchMode = (StretchModeEnum)3,
			ExpandMode = (ExpandModeEnum)1,
			MouseFilter = (MouseFilterEnum)2
		};
		_stickyName = new Label
		{
			VerticalAlignment = (VerticalAlignment)1,
			ClipText = true,
			TextOverrunBehavior = (OverrunBehavior)3,
			MouseFilter = (MouseFilterEnum)2
		};
		_stickyStatus = new Label
		{
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1,
			ClipText = true,
			MouseFilter = (MouseFilterEnum)2
		};
		BindLocalization((Control)(object)_stickyHeader, delegate
		{
			((Control)_stickyHeader).TooltipText = MlmLocManager.Get("groups.folder.sticky.tooltip");
		});
		BindPalette((GodotObject)(object)_stickyHeader, delegate
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			StyleBoxFlat val = UiKit.Box(UiKit.PinnedRowSurface(UiKit.Accent, 0.09f), UiKit.Palette.Border, 0, 0, 0, 0);
			val.BorderWidthBottom = 1;
			val.ShadowColor = UiKit.Palette.SurfaceShadow;
			val.ShadowSize = 4;
			val.ShadowOffset = new Vector2(0f, 2f);
			((Control)_stickyHeader).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val);
			((Control)_stickyName).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.Text);
			((Control)_stickyStatus).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.TextMuted);
		});
		Panel? stickyHeader = _stickyHeader;
		object obj = _003C_003EO._003C103_003E__OnStickyHeaderInput;
		if (obj == null)
		{
			GuiInputEventHandler val2 = OnStickyHeaderInput;
			_003C_003EO._003C103_003E__OnStickyHeaderInput = val2;
			obj = (object)val2;
		}
		((Control)stickyHeader).GuiInput += (GuiInputEventHandler)obj;
		((Node)_stickyHeader).AddChild((Node)(object)_stickyIcon, false, (InternalMode)0);
		((Node)_stickyHeader).AddChild((Node)(object)_stickyName, false, (InternalMode)0);
		((Node)_stickyHeader).AddChild((Node)(object)_stickyStatus, false, (InternalMode)0);
		((Node)_stickyClip).AddChild((Node)(object)_stickyHeader, false, (InternalMode)0);
		((Node)tree).AddChild((Node)(object)_stickyClip, false, (InternalMode)0);
		((Control)tree).Resized += delegate
		{
			Defer(OnTreeScrolled);
		};
		VScrollBar val3 = ((IEnumerable)((Node)tree).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
		if (val3 == null)
		{
			return;
		}
		object obj2 = _003C_003Ec._003C_003E9__587_3;
		if (obj2 == null)
		{
			ValueChangedEventHandler val4 = delegate
			{
				OnTreeScrolled();
			};
			_003C_003Ec._003C_003E9__587_3 = val4;
			obj2 = (object)val4;
		}
		((Range)val3).ValueChanged += (ValueChangedEventHandler)obj2;
	}

	private static void OnTreeScrolled()
	{
		UpdatePinnedTitles();
		UpdateStickyHeader();
		PlaceInlineRename();
		Defer(SyncListSpacer);
	}

	private static bool BeginInlineRename(string group)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Expected O, but got Unknown
		if (_groupListView && !_pumpActive)
		{
			ModReorderTree modTree = _modTree;
			if (modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)modTree))
			{
				TreeItem val = FindFolderHeader(FolderKey(group));
				if (val != null)
				{
					CancelInlineRename();
					ShowGroupNameRow(visible: false);
					if (_folderRenameEdit == null || !GodotObject.IsInstanceValid((GodotObject)(object)_folderRenameEdit))
					{
						_folderRenameEdit = new LineEdit
						{
							Visible = false,
							MaxLength = 40,
							SelectAllOnFocus = true
						};
						LineEdit? folderRenameEdit = _folderRenameEdit;
						object obj = _003C_003Ec._003C_003E9__589_0;
						if (obj == null)
						{
							TextSubmittedEventHandler val2 = delegate
							{
								CommitInlineRename();
							};
							_003C_003Ec._003C_003E9__589_0 = val2;
							obj = (object)val2;
						}
						folderRenameEdit.TextSubmitted += (TextSubmittedEventHandler)obj;
						((Control)_folderRenameEdit).FocusExited += delegate
						{
							Defer(CancelInlineRename);
						};
						LineEdit? folderRenameEdit2 = _folderRenameEdit;
						object obj2 = _003C_003Ec._003C_003E9__589_2;
						if (obj2 == null)
						{
							GuiInputEventHandler val4 = delegate(InputEvent input)
							{
								//IL_0013: Unknown result type (might be due to invalid IL or missing references)
								//IL_001e: Invalid comparison between Unknown and I8
								InputEventKey val3 = (InputEventKey)(object)((input is InputEventKey) ? input : null);
								if (val3 != null && val3.Pressed && (long)val3.Keycode == 4194305)
								{
									((Control)_folderRenameEdit).AcceptEvent();
									CancelInlineRename();
								}
							};
							_003C_003Ec._003C_003E9__589_2 = val4;
							obj2 = (object)val4;
						}
						((Control)folderRenameEdit2).GuiInput += (GuiInputEventHandler)obj2;
						((Node)modTree).AddChild((Node)(object)_folderRenameEdit, false, (InternalMode)0);
					}
					_folderRenameGroup = group;
					_folderRenameEdit.Text = group;
					((Tree)modTree).ScrollToItem(val, false);
					((Node)modTree).GetTree().CreateTimer(0.0, true, false, false).Timeout += delegate
					{
						if (_folderRenameGroup == group && _folderRenameEdit != null && GodotObject.IsInstanceValid((GodotObject)(object)_folderRenameEdit))
						{
							((CanvasItem)_folderRenameEdit).Visible = true;
							PlaceInlineRename();
							((Control)_folderRenameEdit).GrabFocus();
							_folderRenameEdit.SelectAll();
						}
					};
					return true;
				}
			}
		}
		return false;
	}

	private static void PlaceInlineRename()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		string folderRenameGroup = _folderRenameGroup;
		if (folderRenameGroup == null)
		{
			return;
		}
		LineEdit folderRenameEdit = _folderRenameEdit;
		if (folderRenameEdit == null || !((CanvasItem)folderRenameEdit).Visible || !GodotObject.IsInstanceValid((GodotObject)(object)folderRenameEdit))
		{
			return;
		}
		ModReorderTree modTree = _modTree;
		if (modTree == null)
		{
			return;
		}
		TreeItem val = FindFolderHeader(FolderKey(folderRenameGroup));
		if (val == null || InFoldedFolder(val))
		{
			CancelInlineRename();
			return;
		}
		Rect2 itemAreaRect = ((Tree)modTree).GetItemAreaRect(val, 2, -1);
		Rect2 val2 = default(Rect2);
		((Rect2)(ref val2))._002Ector(Vector2.Zero, ((Control)modTree).Size);
		if (!((Rect2)(ref val2)).HasPoint(((Rect2)(ref itemAreaRect)).GetCenter()))
		{
			CancelInlineRename();
			return;
		}
		Texture2D icon = val.GetIcon(2);
		float num = ((icon != null) ? icon.GetWidth() : 0) + ((Control)modTree).GetThemeConstant(StringName.op_Implicit("h_separation"), (StringName)null);
		((Control)folderRenameEdit).Position = new Vector2(((Rect2)(ref itemAreaRect)).Position.X + num, ((Rect2)(ref itemAreaRect)).Position.Y + 1f);
		((Control)folderRenameEdit).Size = new Vector2(Math.Max(120f, ((Rect2)(ref itemAreaRect)).Size.X - num), Math.Max(24f, ((Rect2)(ref itemAreaRect)).Size.Y - 2f));
	}

	private static void CommitInlineRename()
	{
		string group = _folderRenameGroup;
		if (group == null || _folderRenameEdit == null || !GodotObject.IsInstanceValid((GodotObject)(object)_folderRenameEdit))
		{
			return;
		}
		string requested = _folderRenameEdit.Text.Trim();
		_folderRenameGroup = null;
		((CanvasItem)_folderRenameEdit).Visible = false;
		if (requested.Length == 0)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("groups.error.nameRequired")));
			return;
		}
		Defer(delegate
		{
			RenameGroup(group, requested);
		});
		ModReorderTree? modTree = _modTree;
		if (modTree != null)
		{
			((Control)modTree).GrabFocus();
		}
	}

	private static void CancelInlineRename()
	{
		if (_folderRenameGroup == null)
		{
			return;
		}
		_folderRenameGroup = null;
		if (_folderRenameEdit == null || !GodotObject.IsInstanceValid((GodotObject)(object)_folderRenameEdit))
		{
			return;
		}
		bool num = ((Control)_folderRenameEdit).HasFocus();
		((CanvasItem)_folderRenameEdit).Visible = false;
		if (num)
		{
			ModReorderTree? modTree = _modTree;
			if (modTree != null)
			{
				((Control)modTree).GrabFocus();
			}
		}
	}

	private static void RevealBelowPinnedTitle(TreeItem item)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		ModReorderTree modTree = _modTree;
		if (modTree == null || !GodotObject.IsInstanceValid((GodotObject)(object)modTree) || !GodotObject.IsInstanceValid((GodotObject)(object)item))
		{
			return;
		}
		Control stickyClip = _stickyClip;
		if (stickyClip == null || !((CanvasItem)stickyClip).Visible || _stickyHeader == null)
		{
			return;
		}
		VScrollBar val = ((IEnumerable)((Node)modTree).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
		if (val == null)
		{
			return;
		}
		float num = ((Control)modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)0) + 4f;
		float y = stickyClip.Position.Y;
		float num2 = y + ((Control)_stickyHeader).Position.Y + ((Control)_stickyHeader).Size.Y;
		for (float num3 = y; num3 < num2; num3 += 1f)
		{
			if (((Tree)modTree).GetItemAtPosition(new Vector2(num, num3)) == item)
			{
				((Range)val).Value = ((Range)val).Value - (double)(num2 - num3);
				break;
			}
		}
	}

	private static void UpdatePinnedTitles()
	{
		if (_modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
		{
			VScrollBar val = ((IEnumerable)((Node)_modTree).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
			bool flag = val != null && ((CanvasItem)val).Visible && ((Range)val).Value > 0.5;
			if (flag != _titlesPinned || (flag && _titlesPinnedPalette != UiKit.Palette))
			{
				_titlesPinned = flag;
				_titlesPinnedPalette = UiKit.Palette;
				UiKit.PinTreeTitles((Tree)(object)_modTree, flag);
			}
		}
	}

	private static void OnStickyHeaderInput(InputEvent inputEvent)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I8
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Invalid comparison between Unknown and I8
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		bool flag = val == null || !val.Pressed;
		if (!flag)
		{
			MouseButton buttonIndex = val.ButtonIndex;
			bool flag2 = (((long)(buttonIndex - 1) <= 1L) ? true : false);
			flag = !flag2;
		}
		if (flag || _stickyHeader == null)
		{
			return;
		}
		string stickyFolderKey = _stickyFolderKey;
		if (stickyFolderKey == null)
		{
			return;
		}
		((Control)_stickyHeader).AcceptEvent();
		if ((long)val.ButtonIndex == 2)
		{
			string text = FolderGroup(stickyFolderKey);
			if (text != null)
			{
				ShowFolderMenu(text, ((Control)_stickyHeader).GetScreenPosition() + ((InputEventMouse)val).Position);
			}
			return;
		}
		string text2 = FolderGroup(stickyFolderKey);
		if (text2 == null)
		{
			return;
		}
		TreeItem header = FindFolderHeader(stickyFolderKey);
		if (header == null)
		{
			return;
		}
		SetFolderCollapsed(header, text2, collapsed: true);
		Defer(delegate
		{
			if (_modTree != null && GodotObject.IsInstanceValid((GodotObject)(object)header))
			{
				((Tree)_modTree).ScrollToItem(header, false);
			}
		});
	}

	private static void UpdateStickyHeader()
	{
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (_stickyClip == null || !GodotObject.IsInstanceValid((GodotObject)(object)_stickyClip) || _stickyHeader == null || _modTree == null || !GodotObject.IsInstanceValid((GodotObject)(object)_modTree))
		{
			return;
		}
		ModReorderTree modTree = _modTree;
		TreeItem val = null;
		string text = null;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = ((Control)modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null).GetMargin((Side)0) + 4f;
		if (_groupListView && !_dragPreviewActive && !_rebuildingTree && ((CanvasItem)modTree).IsVisibleInTree())
		{
			num = TreeItemTop((Tree)(object)modTree, num4);
			TreeItem obj = ((num < 0f) ? null : ((Tree)modTree).GetItemAtPosition(new Vector2(num4, num)));
			TreeItem val2 = ((obj != null) ? obj.GetParent() : null);
			object obj2;
			if (val2 == null)
			{
				obj2 = null;
			}
			else
			{
				Variant metadata = val2.GetMetadata(0);
				obj2 = ((Variant)(ref metadata)).AsString();
			}
			if (obj2 == null)
			{
				obj2 = string.Empty;
			}
			string text2 = (string)obj2;
			if (obj != null && val2 != null && IsFolderKey(text2))
			{
				val = val2;
				text = text2;
				Rect2 itemAreaRect = ((Tree)modTree).GetItemAreaRect(val2, -1, -1);
				num3 = ((Rect2)(ref itemAreaRect)).Size.Y;
				num2 = PushedOffset((Tree)(object)modTree, num4, num, num3, val2);
			}
		}
		_stickyFolderKey = text;
		if (val == null || text == null || num3 <= 0f)
		{
			((CanvasItem)_stickyClip).Visible = false;
			return;
		}
		StyleBox themeStylebox = ((Control)modTree).GetThemeStylebox(StringName.op_Implicit("panel"), (StringName)null);
		float margin = themeStylebox.GetMargin((Side)0);
		VScrollBar val3 = ((IEnumerable)((Node)modTree).GetChildren(true)).OfType<VScrollBar>().FirstOrDefault();
		float num5 = ((Control)modTree).Size.X - margin - themeStylebox.GetMargin((Side)2) - ((val3 != null && ((CanvasItem)val3).Visible) ? ((Control)val3).Size.X : 0f);
		_stickyClip.Position = new Vector2(margin, num);
		_stickyClip.Size = new Vector2(num5, num3);
		((Control)_stickyHeader).Position = new Vector2(0f, num2);
		((Control)_stickyHeader).Size = new Vector2(num5, num3);
		Rect2 itemAreaRect2 = ((Tree)modTree).GetItemAreaRect(val, 2, -1);
		Rect2 itemAreaRect3 = ((Tree)modTree).GetItemAreaRect(val, 6, -1);
		int themeFontSize = ((Control)modTree).GetThemeFontSize(StringName.op_Implicit("font_size"), (StringName)null);
		float num6 = ((Control)modTree).GetThemeConstant(StringName.op_Implicit("inner_item_margin_left"), (StringName)null);
		Texture2D val4 = val.GetIcon(2) ?? FoldIcon(collapsed: false, 0);
		float num7 = ((Rect2)(ref itemAreaRect2)).Position.X - margin + num6;
		_stickyIcon.Texture = val4;
		((Control)_stickyIcon).Position = new Vector2(num7, 0f);
		((Control)_stickyIcon).Size = new Vector2((float)val4.GetWidth(), num3);
		float num8 = num7 + (float)val4.GetWidth() + (float)((Control)modTree).GetThemeConstant(StringName.op_Implicit("h_separation"), (StringName)null);
		float num9 = ((Rect2)(ref itemAreaRect3)).Position.X - margin;
		string text3 = val.GetText(2);
		string text4 = FolderGroup(text);
		if (text4 != null && text4.Length > 0)
		{
			string text5 = ParentGroupOf(text4);
			if (text5 != null && text5.Length > 0)
			{
				text3 = GroupPath(text5) + " / " + text3;
			}
		}
		((Control)_stickyName).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), themeFontSize);
		_stickyName.Text = text3;
		((Control)_stickyName).Position = new Vector2(num8, 0f);
		((Control)_stickyName).Size = new Vector2(Math.Max(0f, num9 - num8 - 8f), num3);
		((Control)_stickyStatus).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), themeFontSize);
		_stickyStatus.Text = val.GetText(6);
		((Control)_stickyStatus).Position = new Vector2(num9, 0f);
		((Control)_stickyStatus).Size = new Vector2(((Rect2)(ref itemAreaRect3)).Size.X, num3);
		((CanvasItem)_stickyClip).Visible = true;
	}

	private static float TreeItemTop(Tree tree, float probeX)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		if (_treeItemTop >= 1f && StartsAt(_treeItemTop))
		{
			return _treeItemTop;
		}
		_treeItemTop = -1f;
		float num = Math.Min(((Control)tree).Size.Y, 240f);
		for (float num2 = 0f; num2 < num; num2 += 1f)
		{
			if (tree.GetItemAtPosition(new Vector2(probeX, num2)) != null)
			{
				_treeItemTop = num2;
				break;
			}
		}
		return _treeItemTop;
		bool StartsAt(float y)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (tree.GetItemAtPosition(new Vector2(probeX, y - 1f)) == null)
			{
				return tree.GetItemAtPosition(new Vector2(probeX, y)) != null;
			}
			return false;
		}
	}

	private static float PushedOffset(Tree tree, float probeX, float top, float height, TreeItem pinned)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Variant metadata = pinned.GetMetadata(0);
		string pinnedKey = ((Variant)(ref metadata)).AsString();
		if (!NextTitleAt(top + height - 1f))
		{
			return 0f;
		}
		for (float num = top + 1f; num < top + height; num += 1f)
		{
			if (NextTitleAt(num))
			{
				return num - (top + height);
			}
		}
		return 0f;
		bool NextTitleAt(float y)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			TreeItem itemAtPosition = tree.GetItemAtPosition(new Vector2(probeX, y));
			if (itemAtPosition != null)
			{
				Variant metadata2 = itemAtPosition.GetMetadata(0);
				if (IsFolderKey(((Variant)(ref metadata2)).AsString()))
				{
					for (TreeItem val = itemAtPosition; val != null; val = val.GetParent())
					{
						metadata2 = val.GetMetadata(0);
						if (string.Equals(((Variant)(ref metadata2)).AsString(), pinnedKey, StringComparison.Ordinal))
						{
							return false;
						}
					}
					return true;
				}
			}
			return false;
		}
	}

	private static Control BuildGroupBar()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Expected O, but got Unknown
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Expected O, but got Unknown
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		_groupRow = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_groupRow).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 7);
		((Control)_groupRow).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 7);
		((Control)_groupRow).Resized += delegate
		{
			Defer(UpdateGroupRowHeight);
		};
		_groupNameRow = new HBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_groupNameRow).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		_groupNamePrompt = UiKit.Label(string.Empty, "DimLabel");
		_groupNamePrompt.VerticalAlignment = (VerticalAlignment)1;
		_groupName = new LineEdit
		{
			ClearButtonEnabled = true,
			MaxLength = 40,
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(0f, 32f)
		};
		BindKeyPlaceholder(_groupName, "groups.name.placeholder");
		LineEdit? groupName = _groupName;
		object obj = _003C_003Ec._003C_003E9__599_1;
		if (obj == null)
		{
			TextSubmittedEventHandler val2 = delegate
			{
				CommitGroupName();
			};
			_003C_003Ec._003C_003E9__599_1 = val2;
			obj = (object)val2;
		}
		groupName.TextSubmitted += (TextSubmittedEventHandler)obj;
		Button val3 = LocalizedKeyButton("common.action.confirm", "MiniButton", 64f, 32f);
		((BaseButton)val3).Pressed += CommitGroupName;
		Button val4 = LocalizedKeyButton("common.action.cancel", "MiniButton", 64f, 32f);
		((BaseButton)val4).Pressed += delegate
		{
			ShowGroupNameRow(visible: false);
		};
		((Node)_groupNameRow).AddChild((Node)(object)_groupNamePrompt, false, (InternalMode)0);
		((Node)_groupNameRow).AddChild((Node)(object)_groupName, false, (InternalMode)0);
		((Node)_groupNameRow).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)_groupNameRow).AddChild((Node)(object)val4, false, (InternalMode)0);
		HBoxContainer val5 = new HBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		((Node)val5).AddChild((Node)(object)_groupRow, false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)BuildFoldAllButton(), false, (InternalMode)0);
		((Node)val5).AddChild((Node)(object)BuildGroupViewToggle(), false, (InternalMode)0);
		_groupLine = (Control?)(object)val5;
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_groupNameRow, false, (InternalMode)0);
		return (Control)val;
	}

	private static void RebuildGroupRow()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Expected O, but got Unknown
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Expected O, but got Unknown
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		if (_groupRow == null || !GodotObject.IsInstanceValid((GodotObject)(object)_groupRow))
		{
			return;
		}
		UpdateGroupViewToggle();
		foreach (Node child in ((Node)_groupRow).GetChildren(false))
		{
			((Node)_groupRow).RemoveChild(child);
			child.QueueFree();
		}
		Label val = UiKit.Caption(MlmLocManager.Get("groups.caption"));
		((Control)val).SizeFlagsVertical = (SizeFlags)4;
		((Node)_groupRow).AddChild((Node)(object)val, false, (InternalMode)0);
		List<(string, int)> list = GroupTreeNames();
		_groupChipGroup = new ButtonGroup();
		int num = _plan?.Entries.Count ?? 0;
		int num2 = _plan?.Entries.Count((LaunchEntry entry) => string.IsNullOrWhiteSpace(entry.Group)) ?? 0;
		AddGroupChip(MlmLocManager.Get("groups.filter.all", ("count", num)), null, MlmLocManager.Get("groups.filter.all.tooltip"));
		if (num2 > 0 && list.Count > 0)
		{
			AddGroupChip(MlmLocManager.Get("groups.filter.ungrouped", ("count", num2)), "\u0001ungrouped", MlmLocManager.Get("groups.filter.ungrouped.tooltip", ("count", num2)));
		}
		foreach (var item3 in list)
		{
			string item = item3.Item1;
			int item2 = item3.Item2;
			string captured = item;
			int num3 = _plan?.Entries.Count((LaunchEntry entry) => InGroup(entry.Group, captured)) ?? 0;
			string text = ((item2 == 0) ? captured : GroupPath(captured));
			AddGroupChip($"{text} · {num3}", captured, MlmLocManager.Get("groups.filter.named.tooltip", ("name", text), ("count", num3)));
		}
		if (list.Count == 0)
		{
			Label val2 = UiKit.Label(MlmLocManager.Get("groups.emptyHint"), "DimLabel");
			((Control)val2).SizeFlagsVertical = (SizeFlags)4;
			((Node)_groupRow).AddChild((Node)(object)val2, false, (InternalMode)0);
		}
		((Node)_groupRow).AddChild((Node)new VSeparator
		{
			CustomMinimumSize = new Vector2(1f, 22f)
		}, false, (InternalMode)0);
		Button val3 = UiKit.Button(MlmLocManager.Get("groups.action.create"), "MiniButton", 0f, 30f);
		((Control)val3).TooltipText = MlmLocManager.Get("groups.action.create.tooltip");
		((BaseButton)val3).Disabled = _pumpActive;
		((BaseButton)val3).Pressed += delegate
		{
			BeginCreateGroup(Array.Empty<LaunchEntry>());
		};
		((Node)_groupRow).AddChild((Node)(object)val3, false, (InternalMode)0);
		Defer(UpdateGroupRowHeight);
		string active = ActiveGroupName();
		if (active == null)
		{
			return;
		}
		List<string> list2 = SiblingGroups(active);
		int num4 = list2.FindIndex((string name) => SameGroup(name, active));
		int[] array = new int[2] { -1, 1 };
		foreach (int num5 in array)
		{
			int captured2 = num5;
			Button val4 = UiKit.Button(string.Empty, "IconButton", 34f, 30f);
			val4.Icon = (Texture2D)(object)((captured2 < 0) ? UiGlyphs.ChevronLeft(14, UiKit.Text, 1.7f) : UiGlyphs.ChevronRight(14, UiKit.Text, 1.7f));
			val4.IconAlignment = (HorizontalAlignment)1;
			((Control)val4).TooltipText = MlmLocManager.Get((captured2 < 0) ? "groups.action.moveEarlier.tooltip" : "groups.action.moveLater.tooltip", ("name", active));
			((BaseButton)val4).Disabled = _pumpActive || num4 < 0 || ((captured2 < 0) ? (num4 == 0) : (num4 == list2.Count - 1));
			((BaseButton)val4).Pressed += delegate
			{
				MoveGroupAmongSiblings(active, captured2);
			};
			((Node)_groupRow).AddChild((Node)(object)val4, false, (InternalMode)0);
		}
		Button val5 = UiKit.Button(MlmLocManager.Get("groups.action.rename"), "MiniButton", 0f, 30f);
		((Control)val5).TooltipText = MlmLocManager.Get("groups.action.rename.tooltip", ("name", active));
		((BaseButton)val5).Disabled = _pumpActive;
		((BaseButton)val5).Pressed += delegate
		{
			BeginRenameGroup(active);
		};
		((Node)_groupRow).AddChild((Node)(object)val5, false, (InternalMode)0);
		string text2 = ParentGroupOf(active);
		Button val6 = UiKit.Button(IsGroupDeleteArmed(active) ? MlmLocManager.Get("common.action.confirmDelete") : MlmLocManager.Get("common.action.delete"), "MiniButton", 0f, 30f);
		((Control)val6).TooltipText = ((text2.Length == 0) ? MlmLocManager.Get("groups.action.delete.tooltip", ("name", active)) : MlmLocManager.Get("groups.action.delete.tooltipNested", ("name", active), ("parent", GroupPath(text2))));
		((BaseButton)val6).Disabled = _pumpActive;
		((BaseButton)val6).Pressed += delegate
		{
			DeleteGroup(active);
		};
		((Node)_groupRow).AddChild((Node)(object)val6, false, (InternalMode)0);
	}

	private static void UpdateGroupRowHeight()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (_groupRow == null || !GodotObject.IsInstanceValid((GodotObject)(object)_groupRow))
		{
			return;
		}
		float num = 0f;
		foreach (Node child in ((Node)_groupRow).GetChildren(false))
		{
			Control val = (Control)(object)((child is Control) ? child : null);
			if (val != null && ((CanvasItem)val).Visible)
			{
				num = Math.Max(num, val.Position.Y + val.Size.Y);
			}
		}
		((Control)_groupRow).CustomMinimumSize = new Vector2(0f, num);
	}

	private static void AddGroupChip(string text, string? value, string tooltip)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Expected O, but got Unknown
		if (_groupRow == null)
		{
			return;
		}
		Button chip = UiKit.Chip(text, _groupChipGroup, string.Equals(value, _groupFilter, StringComparison.OrdinalIgnoreCase));
		((Control)chip).TooltipText = tooltip;
		((BaseButton)chip).Disabled = _pumpActive;
		((BaseButton)chip).Toggled += (ToggledEventHandler)delegate(bool pressed)
		{
			if (pressed && !string.Equals(_groupFilter, value, StringComparison.OrdinalIgnoreCase))
			{
				_groupFilter = value;
				_groupDeleteArmed = false;
				QueueRebuild();
				Defer(RebuildGroupRow);
			}
		};
		if (value != null && value.Length > 0)
		{
			string group = value;
			if (value != "\u0001ungrouped")
			{
				((Control)chip).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
				{
					//IL_000b: Unknown result type (might be due to invalid IL or missing references)
					//IL_0012: Invalid comparison between Unknown and I8
					//IL_0033: Unknown result type (might be due to invalid IL or missing references)
					//IL_0039: Unknown result type (might be due to invalid IL or missing references)
					//IL_003e: Unknown result type (might be due to invalid IL or missing references)
					InputEventMouseButton val = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
					if (val != null && (long)val.ButtonIndex == 2 && val.Pressed)
					{
						((Control)chip).AcceptEvent();
						ShowFolderMenu(group, ((Control)chip).GetScreenPosition() + ((InputEventMouse)val).Position);
					}
				};
			}
		}
		((Node)_groupRow).AddChild((Node)(object)chip, false, (InternalMode)0);
	}

	private static string? ActiveGroupName()
	{
		if (!string.IsNullOrEmpty(_groupFilter) && !(_groupFilter == "\u0001ungrouped"))
		{
			return _groupFilter;
		}
		return null;
	}

	private static List<string> KnownGroupNames()
	{
		return (from node in GroupTreeNames()
			select node.Name).ToList();
	}

	private static bool IsGroupDeleteArmed(string group)
	{
		if (_groupDeleteArmed)
		{
			return SameGroup(_groupDeleteTarget, group);
		}
		return false;
	}

	private static void ApplyOrganization(IEnumerable<LaunchEntry> entries)
	{
		foreach (LaunchEntry entry in entries)
		{
			if (_organization.Mods.TryGetValue(entry.Id, out ModOrganizationEntry value))
			{
				entry.Alias = value.Alias.Trim();
				entry.Group = value.Group.Trim();
				entry.DescriptionOverride = value.Description.Trim();
				entry.IgnorePriority = value.IgnorePriority;
			}
			else
			{
				entry.Alias = string.Empty;
				entry.Group = string.Empty;
				entry.DescriptionOverride = string.Empty;
				entry.IgnorePriority = false;
			}
		}
	}

	private static bool MutateOrganization(Action change)
	{
		RecordUndo();
		ModOrganizationDocument organization = _organization.Clone();
		change();
		PlanResult result = ModOrganizationStore.Save(_organization);
		if (!result.Success)
		{
			_organization = organization;
			ShowFeedback(result);
			return false;
		}
		ApplyOrganization(_plan?.Entries ?? new List<LaunchEntry>());
		return true;
	}

	private static string StoredGroupOf(string modId)
	{
		if (!_organization.Mods.TryGetValue(modId, out ModOrganizationEntry value))
		{
			return string.Empty;
		}
		return value.Group.Trim();
	}

	private static void AssignGroup(IReadOnlyList<LaunchEntry> targets, string group, bool reveal = true)
	{
		if (_plan == null || _pumpActive || targets.Count == 0)
		{
			return;
		}
		string canonical = _organization.CanonicalGroup(group);
		List<string> ids = (from entry in targets
			select entry.Id into id
			where !string.IsNullOrWhiteSpace(id)
			select id).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToList();
		int num = ids.Count((string id) => !string.Equals(StoredGroupOf(id), canonical, StringComparison.OrdinalIgnoreCase));
		if (num == 0)
		{
			if (targets.Count > 1)
			{
				ShowFeedback(PlanResult.Ok((canonical.Length == 0) ? MlmLocManager.Get("groups.assign.alreadyUngrouped") : MlmLocManager.Get("groups.assign.alreadyIn", ("name", canonical))));
			}
		}
		else
		{
			if (!MutateOrganization(delegate
			{
				if (canonical.Length > 0)
				{
					_organization.AddGroup(canonical);
				}
				foreach (string item in ids)
				{
					_organization.SetGroup(item, canonical);
				}
			}))
			{
				return;
			}
			bool scrollToSelection = false;
			if (reveal && _groupListView)
			{
				RevealFolder(canonical);
				scrollToSelection = targets.Any((LaunchEntry entry) => entry == _selected);
			}
			bool flag = targets.Any(_plan.HasDuplicateSiblings);
			ShowFeedback(PlanResult.Ok((canonical.Length == 0) ? MlmLocManager.Get("groups.assign.removed", ("count", num)) : MlmLocManager.Get(flag ? "groups.assign.addedWithCopies" : "groups.assign.added", ("count", num), ("name", canonical))));
			RefreshAfterGroupChange(scrollToSelection);
		}
	}

	private static void RenameGroup(string from, string to)
	{
		string target = to.Trim();
		if (_pumpActive || target.Length == 0 || string.Equals(from, target, StringComparison.Ordinal))
		{
			return;
		}
		int affected = 0;
		if (MutateOrganization(delegate
		{
			affected = _organization.RenameGroup(from, target);
		}))
		{
			RenameFolderState(from, target);
			if (string.Equals(_groupFilter, from, StringComparison.OrdinalIgnoreCase))
			{
				_groupFilter = target;
			}
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("groups.rename.success", ("from", from), ("to", target), ("count", affected))));
			RefreshAfterGroupChange();
		}
	}

	private static void DeleteGroup(string group)
	{
		if (_pumpActive)
		{
			return;
		}
		string text = ParentGroupOf(group);
		if (!IsGroupDeleteArmed(group))
		{
			_groupDeleteArmed = true;
			_groupDeleteTarget = group;
			ShowFeedback(PlanResult.Warning((text.Length == 0) ? MlmLocManager.Get("groups.delete.confirm", ("name", group)) : MlmLocManager.Get("groups.delete.confirmNested", ("name", group), ("parent", GroupPath(text)))));
			Defer(RebuildGroupRow);
			return;
		}
		_groupDeleteArmed = false;
		_groupDeleteTarget = null;
		string text2 = ((text.Length == 0) ? string.Empty : GroupPath(text));
		int affected = 0;
		if (MutateOrganization(delegate
		{
			affected = _organization.DeleteGroup(group);
		}))
		{
			RenameFolderState(group, null);
			if (string.Equals(_groupFilter, group, StringComparison.OrdinalIgnoreCase))
			{
				_groupFilter = null;
			}
			ShowFeedback(PlanResult.Ok((text2.Length == 0) ? MlmLocManager.Get("groups.delete.success", ("name", group), ("count", affected)) : MlmLocManager.Get("groups.delete.successNested", ("name", group), ("count", affected), ("parent", text2))));
			RefreshAfterGroupChange();
		}
	}

	private static void RefreshAfterGroupChange(bool scrollToSelection = false)
	{
		string text = ActiveGroupName();
		if (text != null && !KnownGroupNames().Contains<string>(text, StringComparer.OrdinalIgnoreCase))
		{
			_groupFilter = null;
		}
		_groupDeleteArmed = false;
		UpdateDetails();
		if (_selected != null)
		{
			PopulateDetailGroupSelector(_selected.Group);
		}
		QueueRebuild(scrollToSelection);
		Defer(RebuildGroupRow);
	}

	private static void ShowGroupNameRow(bool visible)
	{
		if (_groupNameRow != null && _groupName != null)
		{
			((CanvasItem)_groupNameRow).Visible = visible;
			if (!visible)
			{
				GroupMenuTargets.Clear();
				_groupRenameSource = null;
				_groupCreateParent = null;
			}
			else
			{
				((Control)_groupName).GrabFocus();
			}
		}
	}

	private static void BeginCreateGroup(IReadOnlyList<LaunchEntry> targets, string? parent = null)
	{
		if (!_pumpActive && _groupName != null && _groupNamePrompt != null)
		{
			_groupRenameSource = null;
			GroupMenuTargets.Clear();
			GroupMenuTargets.AddRange(targets.Select((LaunchEntry entry) => entry.Key));
			_groupName.Text = string.Empty;
			_groupNamePrompt.Text = ((parent != null && parent.Length > 0) ? MlmLocManager.Get("groups.create.childPrompt", ("parent", GroupPath(parent))) : ((targets.Count == 0) ? MlmLocManager.Get("groups.create.prompt") : MlmLocManager.Get("groups.create.forMods", ("count", targets.Count))));
			ShowGroupNameRow(visible: true);
			_groupCreateParent = ((parent != null && parent.Length > 0) ? parent : null);
		}
	}

	private static void BeginRenameGroup(string group)
	{
		if (!_pumpActive && _groupName != null && _groupNamePrompt != null)
		{
			GroupMenuTargets.Clear();
			_groupRenameSource = group;
			_groupName.Text = group;
			_groupNamePrompt.Text = MlmLocManager.Get("groups.rename.prompt", ("name", group));
			ShowGroupNameRow(visible: true);
		}
	}

	private static void CommitGroupName()
	{
		if (_pumpActive || _groupName == null)
		{
			return;
		}
		string text = _groupName.Text.Trim();
		if (text.Length == 0)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("groups.error.nameRequired")));
			return;
		}
		string groupRenameSource = _groupRenameSource;
		string parent = _groupCreateParent;
		List<LaunchEntry> list = EntriesForKeys(GroupMenuTargets);
		ShowGroupNameRow(visible: false);
		if (groupRenameSource != null)
		{
			RenameGroup(groupRenameSource, text);
			return;
		}
		string canonical = _organization.CanonicalGroup(text);
		if (list.Count > 0)
		{
			AssignGroup(list, canonical);
		}
		else if (KnownGroupNames().Contains<string>(canonical, StringComparer.OrdinalIgnoreCase))
		{
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("groups.create.exists", ("name", canonical))));
		}
		else if (MutateOrganization(delegate
		{
			_organization.AddGroup(canonical);
			if (parent != null)
			{
				_organization.SetParent(canonical, parent);
			}
		}))
		{
			if (parent != null)
			{
				RevealFolder(parent);
			}
			ShowFeedback(PlanResult.Ok((parent == null) ? MlmLocManager.Get("groups.create.success", ("name", canonical)) : MlmLocManager.Get("groups.create.childSuccess", ("name", canonical), ("parent", GroupPath(parent)))));
			RefreshAfterGroupChange();
		}
	}

	private static void ShowGroupMenuForVisibleRows()
	{
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		if (_assignGroup == null || TreeBindings.Count == 0)
		{
			return;
		}
		List<LaunchEntry> list;
		if (!HasActiveListFilter())
		{
			if (_selected == null || !TreeBindings.Any((TreeBinding binding) => binding.Entry == _selected))
			{
				list = new List<LaunchEntry>();
			}
			else
			{
				int num = 1;
				list = new List<LaunchEntry>(num);
				CollectionsMarshal.SetCount(list, num);
				CollectionsMarshal.AsSpan(list)[0] = _selected;
			}
		}
		else
		{
			list = TreeBindings.Select((TreeBinding binding) => binding.Entry).ToList();
		}
		List<LaunchEntry> list2 = list;
		if (list2.Count != 0)
		{
			ShowGroupMenu(list2, Vector2.Zero, (Control?)(object)_assignGroup);
		}
	}

	private static void ShowGroupMenuForRow(string rowKey, Vector2 localPosition)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if (_modTree != null)
		{
			string text = FolderGroup(rowKey);
			if (text != null)
			{
				ShowFolderMenu(text, ((Control)_modTree).GetScreenPosition() + localPosition);
				return;
			}
		}
		if (_modTree == null)
		{
			return;
		}
		LaunchEntry launchEntry = _plan?.Find(rowKey);
		if (launchEntry == null)
		{
			return;
		}
		if (launchEntry != _selected)
		{
			if (!SaveSelectedOrganization(showFeedback: false))
			{
				return;
			}
			_selected = launchEntry;
			UpdateDetails();
			UpdateListChrome();
			QueueRebuild();
		}
		ShowGroupMenu(new _003C_003Ez__ReadOnlySingleElementList<LaunchEntry>(launchEntry), ((Control)_modTree).GetScreenPosition() + localPosition);
	}

	private static void ShowGroupMenu(IReadOnlyList<LaunchEntry> targets, Vector2 screenPosition, Control? anchor = null)
	{
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		if (_groupMenu == null || !GodotObject.IsInstanceValid((GodotObject)(object)_groupMenu) || _pumpActive || targets.Count == 0)
		{
			return;
		}
		PopupMenu menu = _groupMenu;
		ShowGroupNameRow(visible: false);
		GroupMenuTargets.Clear();
		GroupMenuTargets.AddRange(targets.Select((LaunchEntry entry) => entry.Key));
		GroupMenuValues.Clear();
		menu.Clear();
		menu.AddSeparator((targets.Count == 1) ? MlmLocManager.Get("groups.menu.moveOne", ("name", targets[0].DisplayName)) : MlmLocManager.Get("groups.menu.moveMany", ("count", targets.Count)), -1);
		string[] array = targets.Select((LaunchEntry entry) => entry.Group.Trim()).Distinct<string>(StringComparer.OrdinalIgnoreCase).ToArray();
		string shared = ((array.Length == 1) ? array[0] : null);
		AddChoice(MlmLocManager.Get("groups.option.ungrouped"), string.Empty);
		foreach (string item in KnownGroupNames())
		{
			AddChoice(GroupPath(item), item);
		}
		menu.AddSeparator("", -1);
		menu.AddItem(MlmLocManager.Get("groups.menu.create"), 10000, (Key)0);
		if (anchor != null)
		{
			UiKit.PopupUnder(anchor, menu);
			return;
		}
		((Window)menu).ResetSize();
		((Window)menu).Position = new Vector2I((int)screenPosition.X, (int)screenPosition.Y);
		((Window)menu).Popup((Rect2I?)null);
		void AddChoice(string label, string value)
		{
			menu.AddRadioCheckItem(label, GroupMenuValues.Count, (Key)0);
			menu.SetItemChecked(menu.ItemCount - 1, shared != null && string.Equals(shared, value, StringComparison.OrdinalIgnoreCase));
			GroupMenuValues.Add(value);
		}
	}

	private static void OnGroupMenuIdPressed(long id)
	{
		List<LaunchEntry> targets = EntriesForKeys(GroupMenuTargets);
		if (targets.Count == 0)
		{
			return;
		}
		if (id == 10000)
		{
			Defer(delegate
			{
				BeginCreateGroup(targets);
			});
			return;
		}
		int num = checked((int)id);
		if (num >= 0 && num < GroupMenuValues.Count)
		{
			string group = GroupMenuValues[num];
			Defer(delegate
			{
				AssignGroup(targets, group);
			});
		}
	}

	private static List<LaunchEntry> EntriesForKeys(IEnumerable<string> keys)
	{
		List<LaunchEntry> list = new List<LaunchEntry>();
		foreach (string key in keys)
		{
			LaunchEntry launchEntry = _plan?.Find(key);
			if (launchEntry != null)
			{
				list.Add(launchEntry);
			}
		}
		return list;
	}

	private static void PopulateDetailGroupSelector(string selectedGroup)
	{
		if (_detailGroupSelector == null || !GodotObject.IsInstanceValid((GodotObject)(object)_detailGroupSelector))
		{
			return;
		}
		_updatingOrganizationFields = true;
		try
		{
			_detailGroupSelector.Clear();
			DetailGroupValues.Clear();
			_detailGroupSelector.AddItem(MlmLocManager.Get("groups.option.ungrouped"), -1);
			DetailGroupValues.Add("\u0001ungrouped");
			foreach (string item in KnownGroupNames())
			{
				_detailGroupSelector.AddItem(GroupPath(item), -1);
				DetailGroupValues.Add(item);
			}
			int num = DetailGroupValues.FindIndex((string value) => string.Equals(value, selectedGroup, StringComparison.OrdinalIgnoreCase));
			_detailGroupSelector.Select((num >= 0) ? num : 0);
		}
		finally
		{
			_updatingOrganizationFields = false;
		}
	}

	private static void OnDetailGroupSelected(long selectedIndex)
	{
		if (!_updatingOrganizationFields && _selected != null && selectedIndex >= 0 && selectedIndex < DetailGroupValues.Count)
		{
			LaunchEntry target = _selected;
			string group = DetailGroupValues[checked((int)selectedIndex)];
			Defer(delegate
			{
				AssignGroup(new _003C_003Ez__ReadOnlySingleElementList<LaunchEntry>(target), (group == "\u0001ungrouped") ? string.Empty : group);
			});
		}
	}

	private static bool OrganizationFieldsDirty()
	{
		if (_selected == null || _detailAlias == null || !string.Equals(_organizationEditingKey, _selected.Key, StringComparison.Ordinal))
		{
			return false;
		}
		return !string.Equals(_detailAlias.Text.Trim(), _selected.Alias, StringComparison.Ordinal);
	}

	private static void UpdateOrganizationSaveState()
	{
		if (_detailOrganizationSave != null && !_updatingOrganizationFields)
		{
			((BaseButton)_detailOrganizationSave).Disabled = _pumpActive || !OrganizationFieldsDirty();
		}
	}

	private static bool SaveSelectedOrganization(bool showFeedback = true)
	{
		if (_selected == null || _detailAlias == null || _pumpActive || !OrganizationFieldsDirty())
		{
			return true;
		}
		LaunchEntry entry = _selected;
		string alias = _detailAlias.Text.Trim();
		if (!MutateOrganization(delegate
		{
			_organization.SetAlias(entry.Id, alias);
		}))
		{
			UpdateOrganizationSaveState();
			return false;
		}
		_organizationEditingKey = null;
		UpdateDetails();
		QueueRebuild();
		if (showFeedback)
		{
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("organization.alias.saved")));
		}
		return true;
	}

	private static Control BuildLoadingPanel()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Expected O, but got Unknown
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Expected O, but got Unknown
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Expected O, but got Unknown
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Expected O, but got Unknown
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Expected O, but got Unknown
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Expected O, but got Unknown
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Expected O, but got Unknown
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Expected O, but got Unknown
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_047d: Expected O, but got Unknown
		_loadingCard = UiKit.Card("InsetPanel");
		((CanvasItem)_loadingCard).Visible = false;
		((Control)_loadingCard).SizeFlagsVertical = (SizeFlags)8;
		_loadingPanel = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_loadingPanel).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		HBoxContainer val = new HBoxContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_loadingSpinner = UiKit.Label("☯\ufe0e", "MutedLabel");
		Label? loadingSpinner = _loadingSpinner;
		StringName obj = StringName.op_Implicit("font");
		SystemFont val2 = new SystemFont();
		val2.FontNames = new string[4] { "Segoe UI Symbol", "Apple Symbols", "Noto Sans Symbols 2", "DejaVu Sans" };
		val2.AllowSystemFallback = false;
		((Control)loadingSpinner).AddThemeFontOverride(obj, (Font)(object)val2);
		((Control)_loadingSpinner).CustomMinimumSize = new Vector2(28f, 28f);
		((Control)_loadingSpinner).PivotOffset = new Vector2(14f, 14f);
		_loadingSpinner.HorizontalAlignment = (HorizontalAlignment)1;
		_loadingSpinner.VerticalAlignment = (VerticalAlignment)1;
		((Control)_loadingSpinner).MouseFilter = (MouseFilterEnum)2;
		((Control)_loadingSpinner).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(22));
		_loadingStatus = LocalizedKeyLabel("loading.status.praying", "MutedLabel");
		((Control)_loadingStatus).SizeFlagsHorizontal = (SizeFlags)3;
		_loadingStatus.VerticalAlignment = (VerticalAlignment)1;
		_loadingStatus.TextOverrunBehavior = (OverrunBehavior)3;
		_loadingElapsed = UiKit.Label("0.0 s", "DimLabel");
		_loadingElapsed.HorizontalAlignment = (HorizontalAlignment)2;
		_loadingElapsed.VerticalAlignment = (VerticalAlignment)1;
		((Node)val).AddChild((Node)(object)_loadingSpinner, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_loadingStatus, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_loadingElapsed, false, (InternalMode)0);
		_loadingCurrentMod = LocalizedKeyLabel("loading.currentMod.preparing", "DimLabel");
		((Control)_loadingCurrentMod).CustomMinimumSize = new Vector2(0f, 20f);
		((Control)_loadingCurrentMod).SizeFlagsHorizontal = (SizeFlags)3;
		_loadingCurrentMod.TextOverrunBehavior = (OverrunBehavior)3;
		_progress = new ProgressBar
		{
			MinValue = 0.0,
			MaxValue = 1.0,
			Value = 0.0,
			ShowPercentage = true,
			CustomMinimumSize = new Vector2(0f, 26f)
		};
		_loadLog = new RichTextLabel
		{
			CustomMinimumSize = new Vector2(0f, 108f),
			BbcodeEnabled = true,
			FitContent = false,
			ScrollActive = true,
			ScrollFollowing = true,
			SelectionEnabled = true
		};
		((Control)_loadLog).AddThemeFontOverride(StringName.op_Implicit("normal_font"), (Font)(object)UiKit.MonoFont);
		((Control)_loadLog).AddThemeFontSizeOverride(StringName.op_Implicit("normal_font_size"), UiKit.ScaleFontSize(13));
		PanelContainer errorPanel = new PanelContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		_loadErrorPanel = errorPanel;
		BindPalette((GodotObject)(object)errorPanel, delegate
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			((Control)errorPanel).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.NoticeBox(UiKit.Danger));
		});
		VBoxContainer val3 = new VBoxContainer();
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		_loadErrorTitle = LocalizedKeyLabel("loading.error.defaultTitle", "DangerLabel");
		_loadErrorDetails = new RichTextLabel
		{
			CustomMinimumSize = new Vector2(0f, 84f),
			FitContent = false,
			ScrollActive = true,
			SelectionEnabled = true
		};
		HBoxContainer val4 = new HBoxContainer();
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 9);
		Button val5 = LocalizedKeyButton("launcher.action.closeAndOpenLogFolder", "ToolbarButton", 220f, 36f);
		BindKeyTooltip((Control)(object)val5, "launcher.action.closeAndOpenLogFolder.tooltip");
		((BaseButton)val5).Pressed += CloseGameAndOpenLogFolder;
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		Label val6 = LocalizedKeyLabel("loading.error.disableToggle", "MutedLabel");
		((Control)val6).SizeFlagsVertical = (SizeFlags)4;
		_disableAfterError = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)_disableAfterError).ToggleMode = true;
		((Control)_disableAfterError).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_disableAfterError, "loading.error.disableToggle.tooltip");
		Button? disableAfterError = _disableAfterError;
		object obj2 = _003C_003Ec._003C_003E9__627_1;
		if (obj2 == null)
		{
			ToggledEventHandler val7 = delegate
			{
				UpdateLoadErrorActions();
			};
			_003C_003Ec._003C_003E9__627_1 = val7;
			obj2 = (object)val7;
		}
		((BaseButton)disableAfterError).Toggled += (ToggledEventHandler)obj2;
		_continueAfterError = UiKit.Button(string.Empty, "SecondaryButton", 154f);
		BindLocalization((Control)(object)_continueAfterError, UpdateLoadErrorActions);
		_restartAfterError = LocalizedKeyButton("loading.error.restart", "PrimaryButton", 112f);
		BindKeyTooltip((Control)(object)_restartAfterError, "loading.error.restart.tooltip");
		((BaseButton)_continueAfterError).Pressed += ContinueAfterLoadError;
		((BaseButton)_restartAfterError).Pressed += RestartAfterLoadError;
		((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_disableAfterError, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_continueAfterError, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_restartAfterError, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_loadErrorTitle, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_loadErrorDetails, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)_loadErrorPanel).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)_loadingPanel).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)_loadingPanel).AddChild((Node)(object)_loadingCurrentMod, false, (InternalMode)0);
		((Node)_loadingPanel).AddChild((Node)(object)_progress, false, (InternalMode)0);
		((Node)_loadingPanel).AddChild((Node)(object)_loadLog, false, (InternalMode)0);
		((Node)_loadingPanel).AddChild((Node)(object)_loadErrorPanel, false, (InternalMode)0);
		((Node)_loadingCard).AddChild((Node)(object)_loadingPanel, false, (InternalMode)0);
		return (Control)(object)_loadingCard;
	}

	private static Control BuildActionBar()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0316: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Expected O, but got Unknown
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, 46f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		PanelContainer val2 = new PanelContainer
		{
			ThemeTypeVariation = StringName.op_Implicit("StatusPanel"),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		_feedback = LocalizedKeyLabel("launcher.status.unsavedHint", "MutedLabel");
		((Control)_feedback).SizeFlagsHorizontal = (SizeFlags)3;
		_feedback.VerticalAlignment = (VerticalAlignment)1;
		_feedback.AutowrapMode = (AutowrapMode)3;
		_feedback.MaxLinesVisible = 2;
		((Control)_feedback).CustomMinimumSize = new Vector2(0f, 20f);
		((Node)val2).AddChild((Node)(object)_feedback, false, (InternalMode)0);
		((Node)_feedback).AddChild((Node)(object)BuildUndoShortcut(), false, (InternalMode)0);
		_reset = LocalizedKeyButton("launcher.action.undoChanges", "GlassGroupButton", 142f);
		BindKeyTooltip((Control)(object)_reset, "launcher.action.undoChanges.tooltip");
		_autoSort = LocalizedKeyButton("launcher.action.organize", "GlassGroupButton", 132f);
		BindKeyTooltip((Control)(object)_autoSort, "launcher.action.organize.tooltip");
		_randomMod = LocalizedKeyButton("launcher.action.randomMod", "GlassGroupButton", 132f);
		BindKeyTooltip((Control)(object)_randomMod, "launcher.action.randomMod.tooltip");
		((BaseButton)_randomMod).Pressed += EnableRandomMod;
		((CanvasItem)_randomMod).Visible = MlmLocManager.ShowRandomModButton;
		_start = LocalizedKeyButton("launcher.action.saveAndLaunch", "PrimaryButton", 190f);
		BindKeyTooltip((Control)(object)_start, "launcher.action.launch.tooltip");
		((BaseButton)_start).Shortcut = UiKit.Keys((Key)4194336);
		_vanillaStart = LocalizedKeyButton("launcher.action.vanillaLaunch", "SecondaryButton", 132f);
		BindKeyTooltip((Control)(object)_vanillaStart, "launcher.action.vanillaLaunch.tooltip");
		((BaseButton)_vanillaStart).Pressed += StartVanilla;
		((CanvasItem)_vanillaStart).Visible = MlmLocManager.ShowVanillaLaunchButton;
		((BaseButton)_reset).Pressed += ResetPlan;
		((BaseButton)_start).Pressed += StartLoading;
		ScheduleAutoLaunch(_start);
		_restartGame = LocalizedKeyButton("launcher.action.restartGame", "GlassGroupButton", 142f);
		BindKeyTooltip((Control)(object)_restartGame, "launcher.action.restartGame.tooltip");
		((CanvasItem)_restartGame).Visible = false;
		((BaseButton)_restartGame).Pressed += RestartGame;
		((BaseButton)_autoSort).Pressed += AutoSortPlan;
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		HBoxContainer secondaryActions = new HBoxContainer();
		BindPalette((GodotObject)(object)secondaryActions, delegate
		{
			((Control)secondaryActions).AddThemeConstantOverride(StringName.op_Implicit("separation"), (!UiKit.Palette.IsLiquidGlass) ? 10 : 0);
		});
		((Node)secondaryActions).AddChild((Node)(object)_restartGame, false, (InternalMode)0);
		((Node)secondaryActions).AddChild((Node)(object)_randomMod, false, (InternalMode)0);
		((Node)secondaryActions).AddChild((Node)(object)_autoSort, false, (InternalMode)0);
		((Node)secondaryActions).AddChild((Node)(object)_reset, false, (InternalMode)0);
		Control val3 = (Control)(object)LiquidGlass.Group((Control)(object)secondaryActions);
		val3.SizeFlagsVertical = (SizeFlags)1;
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_vanillaStart, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_start, false, (InternalMode)0);
		return (Control)val;
	}

	private static void StartVanilla()
	{
		if (UpdateAnnouncement.IsOpen || TourOpen || WorkshopRemovalPrompt.IsOpen || WorkshopSubscription.Busy || _pumpActive || _plan == null || _pendingCanonicalAction != 0 || Interlocked.CompareExchange(ref _startPressed, 1, 0) != 0)
		{
			return;
		}
		string errorKey;
		if (WorkshopBranches.IsBusy)
		{
			Interlocked.Exchange(ref _startPressed, 0);
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("workshop.switch.launchWait")));
		}
		else if (!VanillaLaunch.TryEnter(out errorKey))
		{
			Interlocked.Exchange(ref _startPressed, 0);
			if (VanillaLaunch.RequiresRestart)
			{
				SetEditingEnabled(enabled: false);
				if (_restartGame != null)
				{
					((CanvasItem)_restartGame).Visible = true;
					((BaseButton)_restartGame).Disabled = false;
				}
			}
			ShowFeedback(PlanResult.Error(MlmLocManager.Get(errorKey)));
		}
		else
		{
			SetEditingEnabled(enabled: false);
			ReleaseStartup();
		}
	}

	private static bool ConfirmLaunchDespiteConflicts(LaunchPlan plan)
	{
		List<ModConflict> list = plan.Conflicts().DistinctBy((ModConflict conflict) => conflict.PairKey).ToList();
		if (list.All((ModConflict conflict) => LaunchConfirmedConflicts.Contains(conflict.PairKey)))
		{
			if (list.Count > 0)
			{
				GD.Print("[ModLaunchManager] launching with mods declared incompatible, confirmed by the player: " + string.Join(", ", list.Select((ModConflict conflict) => conflict.Declarer.Id + " (declared) / " + conflict.Target.Id)));
			}
			return true;
		}
		LaunchConfirmedConflicts.UnionWith(list.Select((ModConflict conflict) => conflict.PairKey));
		ShowFeedback(PlanResult.Warning((list.Count == 1) ? MlmLocManager.Get("launcher.conflictsConfirm", ("first", list[0].Declarer.DisplayName), ("second", list[0].Target.DisplayName)) : MlmLocManager.Get("launcher.conflictsConfirmMany", ("count", list.Count), ("first", list[0].Declarer.DisplayName), ("second", list[0].Target.DisplayName))));
		return false;
	}

	private static void StartLoading()
	{
		if (UpdateAnnouncement.IsOpen || TourOpen || WorkshopRemovalPrompt.IsOpen || WorkshopSubscription.Busy || VanillaLaunch.IsActive || VanillaLaunch.RequiresRestart)
		{
			return;
		}
		if (_aiCancellation != null)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("ai.launchWait")));
			return;
		}
		if (!WorkshopBranches.IsBusy)
		{
			LaunchPlan? plan = _plan;
			if (plan == null || !plan.Entries.Any((LaunchEntry entry) => entry.Enabled && WorkshopBranches.BlocksLaunch(entry.Mod)))
			{
				if (_pendingCanonicalAction == PendingCanonicalAction.Launch)
				{
					RetryPendingLaunchSave();
				}
				else
				{
					if (Interlocked.Exchange(ref _startPressed, 1) != 0 || _plan == null)
					{
						return;
					}
					if (!SaveSelectedOrganization(showFeedback: false))
					{
						Interlocked.Exchange(ref _startPressed, 0);
						return;
					}
					PlanResult planResult = _plan.Validate();
					if (!planResult.Success)
					{
						Interlocked.Exchange(ref _startPressed, 0);
						ShowFeedback(PlanResult.Error(MlmLocManager.Get("launcher.launchBlocked", ("error", planResult.Message))));
						return;
					}
					if (!ConfirmLaunchDespiteConflicts(_plan))
					{
						Interlocked.Exchange(ref _startPressed, 0);
						return;
					}
					int downloadingCount = WorkshopUpdates.DownloadingCount;
					if (downloadingCount > 0 && !_launchDuringDownloadArmed)
					{
						_launchDuringDownloadArmed = true;
						Interlocked.Exchange(ref _startPressed, 0);
						ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.downloadInProgressConfirm", ("count", downloadingCount))));
						return;
					}
					CanonicalLaunchDocument canonicalLaunchDocument = CanonicalLaunchStore.BuildCandidate(_plan, _canonicalDormant);
					DeferredModLoader.ProjectionSnapshot projectionSnapshot = null;
					try
					{
						projectionSnapshot = DeferredModLoader.ApplyPlanProjection(_plan);
						DeferredModLoader.SaveSettingsRawOrThrow();
					}
					catch (Exception ex)
					{
						if (projectionSnapshot != null)
						{
							try
							{
								DeferredModLoader.RestorePlanProjection(projectionSnapshot);
							}
							catch (Exception value)
							{
								GD.PushWarning($"{"[ModLaunchManager]"} launch-plan rollback failed: {value}");
							}
						}
						Interlocked.Exchange(ref _startPressed, 0);
						GD.PushWarning($"{"[ModLaunchManager]"} settings.save could not be verified: {ex}");
						ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.settingsSaveUnverified", ("error", ex.Message))));
						return;
					}
					try
					{
						CanonicalLaunchStore.Save(canonicalLaunchDocument);
						CommitCanonical(canonicalLaunchDocument);
						DeferredModLoader.LogSavedPlan(_plan);
					}
					catch (Exception ex2)
					{
						_pendingCanonical = canonicalLaunchDocument;
						_pendingCanonicalAction = PendingCanonicalAction.Launch;
						Interlocked.Exchange(ref _startPressed, 0);
						SetEditingEnabled(enabled: false);
						if (_start != null)
						{
							((BaseButton)_start).Disabled = false;
							_start.Text = MlmLocManager.Get("launcher.action.retryOrderSaveAndLaunch");
						}
						GD.PushWarning($"{"[ModLaunchManager]"} canonical launch-plan save failed: {ex2}");
						ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.canonicalSaveFailed", ("error", ex2.Message))));
						return;
					}
					ContinueLaunching();
				}
				return;
			}
		}
		ShowFeedback(PlanResult.Error(MlmLocManager.Get(WorkshopBranches.IsBusy ? "workshop.switch.launchWait" : "workshop.switch.restoreFailed")));
	}

	private static void RetryPendingLaunchSave()
	{
		if (_pendingCanonical == null || Interlocked.Exchange(ref _startPressed, 1) != 0)
		{
			return;
		}
		try
		{
			CanonicalLaunchStore.Save(_pendingCanonical);
			CommitCanonical(_pendingCanonical);
			if (_plan != null)
			{
				DeferredModLoader.LogSavedPlan(_plan);
			}
			ContinueLaunching();
		}
		catch (Exception ex)
		{
			Interlocked.Exchange(ref _startPressed, 0);
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.canonicalSaveStillFailed", ("error", ex.Message))));
		}
	}

	private static void CommitCanonical(CanonicalLaunchDocument candidate)
	{
		_canonical = CanonicalLaunchStore.Clone(candidate);
		_canonicalDormant = (from entry in CanonicalLaunchStore.DormantRows(candidate, _plan?.Entries.Count ?? 0)
			select entry.Clone()).ToList();
		_pendingCanonical = null;
		_pendingCanonicalAction = PendingCanonicalAction.None;
	}

	private static void ContinueLaunching()
	{
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		if (_aiCancellation != null)
		{
			Interlocked.Exchange(ref _startPressed, 0);
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("ai.launchWait")));
			return;
		}
		if (_plan == null)
		{
			throw new InvalidOperationException("the launch plan disappeared after it was saved");
		}
		SaveProfileSwitcher.Request(_selectedSaveProfileId);
		ModOptionApplyReport modOptionApplyReport = new ModOptionApplyReport();
		try
		{
			modOptionApplyReport = ModOptionApplier.ApplyAll(_plan, _options);
			if (modOptionApplyReport.StoreChanged)
			{
				ModOptionStore.Save(_options);
			}
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not apply launch options: {ex}");
			modOptionApplyReport.Failures.Add(MlmLocManager.Get("loading.optionsWriteFailed", ("error", ex.Message)));
		}
		try
		{
			LocalModWatcher.Stop();
			LauncherBackground.StopWatching();
			LauncherBackground.CancelLoad();
			FriendModSync.Stop();
			FriendModSyncDialog.Close();
			StopChangeNotes();
			UpdateChangeNotesChrome();
			ClearAiSecrets();
			LauncherDisplay.RestoreRedraw();
			_pumpActive = true;
			SetEditingEnabled(enabled: false);
			if (_loadingCard != null)
			{
				((CanvasItem)_loadingCard).Visible = true;
			}
			ShowPrayingState();
			SetStatus(MlmLocManager.Get("loading.planSaved"), "SuccessLabel", "✓");
			AppendLog(MlmLocManager.Get("loading.planSavedLog"), UiKit.TextMuted);
			LogOptionReport(modOptionApplyReport);
			DeferredModLoader.BeginResume();
			_skippedRun = 0;
			ShowLoadProgress(0, DeferredModLoader.LoadsTotal);
			if (_sceneTree == null)
			{
				throw new InvalidOperationException("Godot SceneTree disappeared");
			}
			LoadClock.Restart();
			_sceneTree.ProcessFrame += PumpOneFrame;
			_framePumpSubscribed = true;
			UpdatePlanSummary();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not start frame pump: {value}");
			ResumeSynchronouslyAndContinue();
		}
	}

	private static void LogOptionReport(ModOptionApplyReport report)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (report.ModsChanged > 0)
		{
			AppendLog(MlmLocManager.Get("loading.optionsApplied", ("keys", report.KeysWritten), ("mods", report.ModsChanged)), UiKit.TextMuted);
		}
		foreach (string failure in report.Failures)
		{
			AppendLog(failure, UiKit.Warning);
		}
	}

	private static void SetEditingEnabled(bool enabled)
	{
		if (!enabled)
		{
			_controller?.RestorePopupBindings();
			_controllerMoveKey = null;
		}
		if (_search != null)
		{
			_search.Editable = enabled;
		}
		if (_languageSelector != null)
		{
			((BaseButton)_languageSelector).Disabled = !enabled;
		}
		if (_themeSelector != null)
		{
			((BaseButton)_themeSelector).Disabled = !enabled;
		}
		if (_settingsButton != null)
		{
			((BaseButton)_settingsButton).Disabled = !enabled;
		}
		if (!enabled && _settingsPage != null)
		{
			((CanvasItem)_settingsPage).Visible = false;
		}
		if (_saveProfileSelector != null)
		{
			((BaseButton)_saveProfileSelector).Disabled = !enabled || !SaveProfileSwitcher.Available;
		}
		if (_friendSync != null)
		{
			((BaseButton)_friendSync).Disabled = !enabled || !FriendModSync.CanOpen;
		}
		if (_collectionImport != null)
		{
			((BaseButton)_collectionImport).Disabled = !enabled || !WorkshopUpdates.IsAvailable;
		}
		if (_transfer != null)
		{
			((BaseButton)_transfer).Disabled = !enabled;
		}
		if (_filterToggle != null)
		{
			((BaseButton)_filterToggle).Disabled = !enabled;
		}
		if (_fab != null)
		{
			((BaseButton)_fab).Disabled = !enabled;
		}
		if (!enabled)
		{
			CloseFabMenu();
			EndTour();
			WorkshopCollectionImportDialog.Close();
			WorkshopDownloadsDialog.Close();
			ModListTransferDialog.Close();
			ModPriorityPrompt.Close();
		}
		if (_modTree != null)
		{
			_modTree.DragEnabled = enabled;
		}
		if (!enabled)
		{
			CancelInlineRename();
		}
		if (_foldAllButton != null)
		{
			((BaseButton)_foldAllButton).Disabled = !enabled;
		}
		if (_undoShortcut != null)
		{
			((BaseButton)_undoShortcut).Disabled = !enabled;
		}
		if (_detailOpenFolder != null)
		{
			((BaseButton)_detailOpenFolder).Disabled = !enabled;
		}
		SetWorkshopTagsEnabled(enabled);
		if (_detailAlias != null)
		{
			_detailAlias.Editable = enabled;
		}
		if (!enabled && _descriptionEditingKey != null)
		{
			_descriptionEditingKey = null;
			UpdateDetails();
		}
		if (_detailGroupSelector != null)
		{
			((BaseButton)_detailGroupSelector).Disabled = !enabled;
		}
		if (_assignGroup != null)
		{
			((BaseButton)_assignGroup).Disabled = !enabled;
		}
		if (!enabled)
		{
			ShowGroupNameRow(visible: false);
		}
		if (_detailOrganizationSave != null)
		{
			((BaseButton)_detailOrganizationSave).Disabled = !enabled || !OrganizationFieldsDirty();
		}
		SetOptionEditorsEnabled(enabled);
		UpdateTranslationControls(enabled);
		if (_reset != null)
		{
			((BaseButton)_reset).Disabled = !enabled;
		}
		if (_autoSort != null)
		{
			((BaseButton)_autoSort).Disabled = !enabled;
		}
		if (_randomMod != null)
		{
			((BaseButton)_randomMod).Disabled = !enabled;
		}
		if (_vanillaStart != null)
		{
			((BaseButton)_vanillaStart).Disabled = !enabled;
		}
		if (_start != null)
		{
			((BaseButton)_start).Disabled = !enabled;
			if (!enabled)
			{
				_start.Text = MlmLocManager.Get("launcher.action.loading");
			}
		}
		if (_detailToggle != null)
		{
			((BaseButton)_detailToggle).Disabled = !enabled;
		}
		if (_restartGame != null)
		{
			((BaseButton)_restartGame).Disabled = !enabled;
		}
		if (_updateAll != null)
		{
			((BaseButton)_updateAll).Disabled = !enabled;
		}
		if (_detailWorkshopUpdate != null)
		{
			((BaseButton)_detailWorkshopUpdate).Disabled = !enabled;
		}
		if (_detailWorkshopOpen != null)
		{
			((BaseButton)_detailWorkshopOpen).Disabled = !enabled;
		}
		if (_detailWorkshopUnsubscribe != null)
		{
			((BaseButton)_detailWorkshopUnsubscribe).Disabled = !enabled || _selected == null || !CanRemoveWorkshopItem(PhysicalModIdentity.WorkshopItemIdFor(_selected.Mod));
		}
		if (_changeNotesOpen != null)
		{
			((BaseButton)_changeNotesOpen).Disabled = !enabled;
		}
		if (_detailChangeNotesPage != null)
		{
			((BaseButton)_detailChangeNotesPage).Disabled = !enabled;
		}
		if (_detailChangeNotesTranslate != null)
		{
			((BaseButton)_detailChangeNotesTranslate).Disabled = !enabled;
		}
		if (_detailWorkshopBranchSelector != null)
		{
			((BaseButton)_detailWorkshopBranchSelector).Disabled = !enabled;
		}
		if (_detailWorkshopBranchSwitch != null)
		{
			((BaseButton)_detailWorkshopBranchSwitch).Disabled = !enabled;
		}
		if (!enabled && _workshopTimer != null && GodotObject.IsInstanceValid((GodotObject)(object)_workshopTimer))
		{
			_workshopTimer.Stop();
		}
		if (!enabled && _friendSyncTimer != null && GodotObject.IsInstanceValid((GodotObject)(object)_friendSyncTimer))
		{
			_friendSyncTimer.Stop();
		}
		foreach (Button filterChip in FilterChips)
		{
			((BaseButton)filterChip).Disabled = !enabled;
		}
		if (_profileNameRow != null)
		{
			((CanvasItem)_profileNameRow).Visible = false;
		}
		Defer(RebuildProfileRow);
		Defer(RebuildGroupRow);
		QueueRebuild();
		UpdateControllerPrompts();
	}

	private static void PumpOneFrame()
	{
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (!_pumpActive || _awaitingLoadDecision || Interlocked.Exchange(ref _pumpBusy, 1) != 0)
		{
			return;
		}
		_deferLogText = true;
		try
		{
			UpdateElapsed();
			UpdateLoadingSpinner();
			if (_releaseOnNextFrame)
			{
				ReleaseStartup();
				return;
			}
			Mod nextMod = DeferredModLoader.NextMod;
			if (nextMod != null && _preparedMod == nextMod && !LoadPreparedMod())
			{
				return;
			}
			while (true)
			{
				Mod nextMod2 = DeferredModLoader.NextMod;
				if (nextMod2 == null || DeferredModLoader.WillLoad(nextMod2))
				{
					break;
				}
				if (!PassOverMod())
				{
					return;
				}
			}
			Mod nextMod3 = DeferredModLoader.NextMod;
			if (nextMod3 != null)
			{
				PrepareLoadingMod(nextMod3);
				return;
			}
			ShowCompletedState();
			_releaseOnNextFrame = true;
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} frame-pump loading failed: {ex}");
			AppendLog(MlmLocManager.Get("loading.framePumpFailed", ("errorType", ex.GetType().Name)), UiKit.Danger);
			ResumeSynchronouslyAndContinue();
		}
		finally
		{
			_deferLogText = false;
			if (_logTextStale)
			{
				ShowLogText();
			}
			Volatile.Write(ref _pumpBusy, 0);
		}
	}

	private static void PrepareLoadingMod(Mod mod)
	{
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		FlushSkippedRun();
		_preparedMod = mod;
		string text = DisplayName(mod);
		int num = DeferredModLoader.LoadsCompleted + 1;
		int loadsTotal = DeferredModLoader.LoadsTotal;
		ShowPrayingState();
		if (_loadingCurrentMod != null)
		{
			_loadingCurrentMod.Text = MlmLocManager.Get("loading.currentMod.active", ("position", num), ("total", loadsTotal), ("name", text));
		}
		AppendLog($"▶ [{num}/{loadsTotal}] {text}", UiKit.Text);
		HighlightLoadingMod(mod);
	}

	private static bool LoadPreparedMod()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		DeferredModLoader.StepResult stepResult = DeferredModLoader.ResumeNext();
		_preparedMod = null;
		if (stepResult == null)
		{
			ShowCompletedState();
			_releaseOnNextFrame = true;
			return false;
		}
		(string State, string Marker, Color Color) tuple = ResultText(stepResult);
		string item = tuple.State;
		string item2 = tuple.Marker;
		Color item3 = tuple.Color;
		int loadsCompleted = DeferredModLoader.LoadsCompleted;
		int loadsTotal = DeferredModLoader.LoadsTotal;
		ShowLoadProgress(loadsCompleted, loadsTotal);
		AppendLog($"{item2} [{loadsCompleted}/{loadsTotal}] {DisplayName(stepResult.Mod)}  {stepResult.Elapsed.TotalMilliseconds:N0} ms · {item}", item3);
		UpdateTreeLoadState(stepResult.Mod);
		return ContinueAfterStep(stepResult);
	}

	private static bool PassOverMod()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		DeferredModLoader.StepResult stepResult = DeferredModLoader.ResumeNext();
		if (stepResult == null)
		{
			ShowCompletedState();
			_releaseOnNextFrame = true;
			return false;
		}
		if ((int)stepResult.Mod.state == 3 && !HasLoadError(stepResult))
		{
			_skippedRun++;
		}
		else
		{
			FlushSkippedRun();
			var (value, value2, color) = ResultText(stepResult);
			AppendLog($"{value2} {DisplayName(stepResult.Mod)} · {value}", color);
		}
		if (HasLoadError(stepResult) || stepResult.Completed >= stepResult.Total)
		{
			HighlightLoadingMod(stepResult.Mod);
		}
		UpdateTreeLoadState(stepResult.Mod);
		return ContinueAfterStep(stepResult);
	}

	private static bool ContinueAfterStep(DeferredModLoader.StepResult step)
	{
		if (HasLoadError(step))
		{
			ShowLoadError(step);
			return false;
		}
		if (step.Completed >= step.Total)
		{
			ShowCompletedState();
			_releaseOnNextFrame = true;
			return false;
		}
		return true;
	}

	private static void FlushSkippedRun()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (_skippedRun != 0)
		{
			AppendLog("− " + MlmLocManager.Get("loading.log.skippedDisabled", ("count", _skippedRun)), UiKit.TextDim);
			_skippedRun = 0;
		}
	}

	private static void ShowLoadProgress(int loaded, int total)
	{
		if (_progress != null)
		{
			((Range)_progress).MaxValue = Math.Max(1, total);
			((Range)_progress).Value = loaded;
		}
	}

	private static void UpdateElapsed()
	{
		if (_loadingElapsed != null && LoadClock.IsRunning)
		{
			_loadingElapsed.Text = $"{LoadClock.Elapsed.TotalSeconds:N1} s";
		}
	}

	private static void UpdateLoadingSpinner()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (_loadingSpinner != null && LoadClock.IsRunning)
		{
			((Control)_loadingSpinner).PivotOffset = ((Control)_loadingSpinner).Size * 0.5f;
			((Control)_loadingSpinner).Rotation = (float)(LoadClock.Elapsed.TotalSeconds * (Math.PI * 2.0) * 0.4000000059604645);
		}
	}

	private static void ShowPrayingState()
	{
		if (_loadingStatus != null)
		{
			_loadingStatus.Text = MlmLocManager.Get("loading.status.praying");
			((Control)_loadingStatus).ThemeTypeVariation = StringName.op_Implicit("MutedLabel");
		}
	}

	private static bool HasLoadError(DeferredModLoader.StepResult step)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Invalid comparison between Unknown and I4
		if (step.InvocationError == null && (int)step.Mod.state != 2)
		{
			if ((int)step.Mod.state == 1)
			{
				List<LocString> errors = step.Mod.errors;
				if (errors != null)
				{
					return errors.Count > 0;
				}
				return false;
			}
			return false;
		}
		return true;
	}

	private static void ShowLoadError(DeferredModLoader.StepResult step)
	{
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		_awaitingLoadDecision = true;
		LauncherDisplay.RedrawOnlyChanges();
		_loadErrorMod = step.Mod;
		string item = DisplayName(step.Mod);
		if (_loadErrorTitle != null)
		{
			_loadErrorTitle.Text = MlmLocManager.Get("loading.error.title", ("name", item));
		}
		if (_loadErrorDetails != null)
		{
			_loadErrorDetails.Text = BuildLoadErrorDetails(step);
		}
		if (_loadErrorPanel != null)
		{
			((CanvasItem)_loadErrorPanel).Visible = true;
		}
		_continueAfterErrorRetryKey = null;
		if (_disableAfterError != null)
		{
			((BaseButton)_disableAfterError).SetPressedNoSignal(false);
			((BaseButton)_disableAfterError).Disabled = false;
		}
		if (_continueAfterError != null)
		{
			((BaseButton)_continueAfterError).Disabled = false;
		}
		if (_restartAfterError != null)
		{
			((BaseButton)_restartAfterError).Disabled = false;
		}
		UpdateLoadErrorActions();
		if (_loadingStatus != null)
		{
			_loadingStatus.Text = MlmLocManager.Get("loading.error.pausedAt", ("completed", DeferredModLoader.LoadsCompleted), ("total", DeferredModLoader.LoadsTotal), ("name", item));
			((Control)_loadingStatus).ThemeTypeVariation = StringName.op_Implicit("DangerLabel");
		}
		SetStatus(MlmLocManager.Get("loading.error.pausedPrompt", ("name", item)), "DangerLabel", "!");
		Button? restartAfterError = _restartAfterError;
		if (restartAfterError != null)
		{
			((Control)restartAfterError).GrabFocus();
		}
		ModLoadFailures.Failure failure = LoadFailure(step);
		GD.PushWarning($"{"[ModLaunchManager]"} paused after load error in {step.Mod.manifest?.id ?? "<unknown>"} (state {step.Mod.state}" + ((failure == null) ? ")." : $", {failure.TypeName}: {failure.Message}, cause {failure.Cause})."));
	}

	private static ModLoadFailures.Failure? LoadFailure(DeferredModLoader.StepResult step)
	{
		if (step.InvocationError != null)
		{
			return ModLoadFailures.Describe(step.InvocationError);
		}
		ModLoadFailures.Failure failure = ModLoadFailures.Find(CollectibleModAssemblyLoader.GetRecordedAssemblies(step.Mod));
		if ((object)failure != null)
		{
			return failure;
		}
		string text = ModErrorText.ExceptionType(step.Mod.errors);
		if (text != null)
		{
			return new ModLoadFailures.Failure(text.Substring(text.LastIndexOf('.') + 1), string.Empty, ModLoadFailures.ClassifyTypeName(text), null);
		}
		return null;
	}

	private static string BuildLoadErrorDetails(DeferredModLoader.StepResult step)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Invalid comparison between Unknown and I4
		string item = DisplayName(step.Mod);
		ModLoadFailures.Failure failure = LoadFailure(step);
		bool flag = step.InvocationError != null || (int)step.Mod.state == 2;
		bool flag2 = !flag && (failure != null || ModErrorText.HasInitializerFailure(step.Mod.errors));
		List<string> list = new List<string>();
		list.Add(MlmLocManager.Get(flag ? "loading.error.summary.failed" : (flag2 ? "loading.error.summary.startFailed" : "loading.error.summary.loadedWithErrors"), ("name", item)));
		List<string> list2 = list;
		switch (failure?.Cause)
		{
		case ModLoadFailures.Cause.VersionMismatch:
			list2.Add(MlmLocManager.Get("loading.error.cause.versionMismatch"));
			break;
		case ModLoadFailures.Cause.MissingDependency:
			list2.Add(MlmLocManager.Get("loading.error.cause.missingDependency", ("missing", failure.Missing)));
			break;
		case ModLoadFailures.Cause.VersionOrDependency:
			list2.Add(MlmLocManager.Get("loading.error.cause.versionOrDependency"));
			break;
		}
		List<LocString> errors = step.Mod.errors;
		if (errors != null && errors.Count > 0)
		{
			foreach (LocString item2 in errors.Where((LocString error) => !ModErrorText.IsGenericFailure(error)))
			{
				list2.Add("• " + ModErrorText.Describe(item2));
			}
		}
		list2.Add(MlmLocManager.Get("loading.error.guidance"));
		if (failure != null)
		{
			list2.Add((failure.Message.Length == 0) ? MlmLocManager.Get("loading.error.technicalType", ("type", failure.TypeName)) : MlmLocManager.Get("loading.error.technical", ("type", failure.TypeName), ("message", failure.Message)));
		}
		return string.Join('\n', list2);
	}

	private static void UpdateLoadErrorActions()
	{
		if (_continueAfterError != null && _disableAfterError != null)
		{
			bool buttonPressed = ((BaseButton)_disableAfterError).ButtonPressed;
			_disableAfterError.Text = MlmLocManager.Get(buttonPressed ? "options.boolean.on" : "options.boolean.off");
			_continueAfterError.Text = MlmLocManager.Get(_continueAfterErrorRetryKey ?? (buttonPressed ? "loading.error.disableContinue" : "loading.error.keepContinue"));
			((Control)_continueAfterError).ThemeTypeVariation = StringName.op_Implicit(buttonPressed ? "DangerButton" : "SecondaryButton");
			((Control)_continueAfterError).TooltipText = MlmLocManager.Get((!buttonPressed) ? "loading.error.keepContinue.tooltip" : ((_loadErrorMod != null && CollectibleModAssemblyLoader.RequiresRestartToUnload(_loadErrorMod)) ? "loading.error.disableForRestart.tooltip" : ((DeferredModLoader.NextMod == null) ? "loading.error.disableAndLaunch.tooltip" : "loading.error.disableContinue.tooltip")));
		}
	}

	private static void ContinueAfterLoadError()
	{
		Button? disableAfterError = _disableAfterError;
		if ((disableAfterError != null && ((BaseButton)disableAfterError).ButtonPressed) || _pendingCanonicalAction == PendingCanonicalAction.Skip)
		{
			SkipAfterLoadError();
		}
		else
		{
			KeepAfterLoadError();
		}
	}

	private static void RestartAfterLoadError()
	{
		if (_awaitingLoadDecision && _restartAfterError != null && !ManagerReturn.BeginDuringStartup((Control)(object)_restartAfterError))
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("restart.unavailable")));
		}
	}

	private static void KeepAfterLoadError()
	{
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (_awaitingLoadDecision && _loadErrorMod != null && _pendingCanonicalAction != PendingCanonicalAction.Skip)
		{
			string text = DisplayName(_loadErrorMod);
			_awaitingLoadDecision = false;
			LauncherDisplay.RestoreRedraw();
			_loadErrorMod = null;
			if (_loadErrorPanel != null)
			{
				((CanvasItem)_loadErrorPanel).Visible = false;
			}
			bool flag = DeferredModLoader.NextMod == null;
			if (_loadingStatus != null)
			{
				_loadingStatus.Text = MlmLocManager.Get(flag ? "loading.error.keptPreparing" : "loading.error.keptContinuing", ("name", text));
				((Control)_loadingStatus).ThemeTypeVariation = StringName.op_Implicit("WarningLabel");
			}
			SetStatus(MlmLocManager.Get("loading.error.kept", ("name", text)), "WarningLabel", "!");
			AppendLog(MlmLocManager.Get("loading.error.keptLog", ("name", text)), UiKit.Warning);
			GD.Print("[ModLaunchManager] kept " + text + " after its load error; loading goes on.");
			if (flag)
			{
				ShowCompletedState();
				_releaseOnNextFrame = true;
			}
		}
	}

	private static void SkipAfterLoadError()
	{
		if (_pendingCanonicalAction == PendingCanonicalAction.Skip)
		{
			RetryPendingSkipSave();
		}
		else
		{
			if (!_awaitingLoadDecision || _loadErrorMod == null)
			{
				return;
			}
			Mod loadErrorMod = _loadErrorMod;
			string text = DisplayName(loadErrorMod);
			CanonicalLaunchDocument canonicalLaunchDocument;
			try
			{
				canonicalLaunchDocument = CanonicalLaunchStore.Clone(_canonical);
				CanonicalLaunchStore.DisableEntry(canonicalLaunchDocument, loadErrorMod);
			}
			catch (Exception ex)
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.error.canonicalEntryMissing", ("name", text), ("error", ex.Message))));
				return;
			}
			DeferredModLoader.DisableResult disableResult = DeferredModLoader.DisableAndSkip(loadErrorMod);
			if (disableResult.Success)
			{
				try
				{
					CanonicalLaunchStore.Save(canonicalLaunchDocument);
					CommitCanonical(canonicalLaunchDocument);
					CompleteSkipAfterSave(loadErrorMod, text);
					return;
				}
				catch (Exception ex2)
				{
					_pendingCanonical = canonicalLaunchDocument;
					_pendingCanonicalAction = PendingCanonicalAction.Skip;
					_pendingSkipMod = loadErrorMod;
					_continueAfterErrorRetryKey = "loading.error.retrySaveAndSkip";
					if (_disableAfterError != null)
					{
						((BaseButton)_disableAfterError).Disabled = true;
					}
					UpdateLoadErrorActions();
					ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.error.settingsSavedCanonicalFailed", ("name", text), ("error", ex2.Message))));
					return;
				}
			}
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.error.disableSaveFailed", ("name", text), ("error", disableResult.Error))));
		}
	}

	private static void RetryPendingSkipSave()
	{
		if (_pendingCanonical == null || _pendingSkipMod == null)
		{
			return;
		}
		try
		{
			Mod? pendingSkipMod = _pendingSkipMod;
			string name = DisplayName(pendingSkipMod);
			CanonicalLaunchStore.Save(_pendingCanonical);
			CommitCanonical(_pendingCanonical);
			_pendingSkipMod = null;
			CompleteSkipAfterSave(pendingSkipMod, name);
		}
		catch (Exception ex)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("loading.error.canonicalSaveStillFailed", ("error", ex.Message))));
		}
	}

	private static void CompleteSkipAfterSave(Mod mod, string name)
	{
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		RuntimeUnloadResult runtimeUnloadResult = CollectibleModAssemblyLoader.TryRollback(mod);
		if (!runtimeUnloadResult.Success)
		{
			_continueAfterErrorRetryKey = (runtimeUnloadResult.RequiresRestart ? null : "loading.error.retryRuntimeUnload");
			if (_disableAfterError != null)
			{
				((BaseButton)_disableAfterError).Disabled = true;
			}
			UpdateLoadErrorActions();
			if (_continueAfterError != null)
			{
				((BaseButton)_continueAfterError).Disabled = runtimeUnloadResult.RequiresRestart;
				if (runtimeUnloadResult.RequiresRestart)
				{
					((Control)_continueAfterError).TooltipText = MlmLocManager.Get("loading.error.disableForRestart.tooltip");
				}
			}
			string text = ((runtimeUnloadResult.ErrorKey == null) ? MlmLocManager.Get("common.value.unknown") : runtimeUnloadResult.Describe());
			ShowFeedback(PlanResult.Error(MlmLocManager.Get(runtimeUnloadResult.RequiresRestart ? "loading.error.runtimeUnloadNeedsRestart" : "loading.error.runtimeUnloadFailed", ("name", name), ("patches", runtimeUnloadResult.RemovedHarmonyPatches), ("error", text))));
			if (runtimeUnloadResult.RequiresRestart)
			{
				Button? restartAfterError = _restartAfterError;
				if (restartAfterError != null)
				{
					((Control)restartAfterError).GrabFocus();
				}
			}
			AppendLog(MlmLocManager.Get("loading.error.runtimeUnloadFailedLog", ("name", name), ("error", text)), UiKit.Danger);
			GD.PushWarning($"{"[ModLaunchManager]"} runtime rollback for {mod.manifest?.id ?? "<unknown>"} is incomplete: {text}");
			return;
		}
		_awaitingLoadDecision = false;
		LauncherDisplay.RestoreRedraw();
		_loadErrorMod = null;
		_pendingSkipMod = null;
		if (_loadErrorPanel != null)
		{
			((CanvasItem)_loadErrorPanel).Visible = false;
		}
		bool flag = DeferredModLoader.NextMod == null;
		if (_loadingStatus != null)
		{
			_loadingStatus.Text = (flag ? MlmLocManager.Get("loading.error.disabledAndSkippedPreparing", ("name", name)) : MlmLocManager.Get("loading.error.disabledAndSkippedContinuing", ("name", name)));
			((Control)_loadingStatus).ThemeTypeVariation = StringName.op_Implicit("WarningLabel");
		}
		SetStatus(flag ? MlmLocManager.Get("loading.error.disabledAndSkippedLaunching", ("name", name)) : MlmLocManager.Get("loading.error.disabledAndSkippedContinuingDetails", ("name", name)), "WarningLabel", "−");
		AppendLog(flag ? MlmLocManager.Get("loading.error.disabledAndSkippedSavedAndLaunching", ("name", name)) : MlmLocManager.Get("loading.error.disabledAndSkippedSavedAndContinuing", ("name", name)), UiKit.Warning);
		UpdateTreeLoadState(mod);
		GD.Print($"{"[ModLaunchManager]"} disabled, unloaded and skipped {mod.manifest?.id}; removed {runtimeUnloadResult.RemovedHarmonyPatches} Harmony patch(es) and {runtimeUnloadResult.ClearedGameRegistrations} game registration(s).");
		if (flag)
		{
			ShowCompletedState();
			_releaseOnNextFrame = true;
		}
	}

	private static void HighlightLoadingMod(Mod mod)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding candidate) => candidate.Entry.Mod == mod);
		if (!(treeBinding == null))
		{
			treeBinding.Item.SetText(6, MlmLocManager.Get("loading.status.loading"));
			treeBinding.Item.SetCustomColor(6, UiKit.Warning);
			treeBinding.Item.SetCustomColor(0, UiKit.Warning);
			treeBinding.Item.SetCustomColor(2, UiKit.Warning);
			treeBinding.Item.Select(2);
			ModReorderTree? modTree = _modTree;
			if (modTree != null)
			{
				((Tree)modTree).ScrollToItem(treeBinding.Item, true);
			}
		}
	}

	private static void UpdateTreeLoadState(Mod mod)
	{
		TreeBinding treeBinding = TreeBindings.FirstOrDefault((TreeBinding candidate) => candidate.Entry.Mod == mod);
		if (treeBinding != null)
		{
			ApplyRowState(treeBinding.Item, treeBinding.Entry);
		}
	}

	private static void RefreshLoadStates()
	{
		foreach (TreeBinding treeBinding in TreeBindings)
		{
			ApplyRowState(treeBinding.Item, treeBinding.Entry);
		}
	}

	private static (string State, string Marker, Color Color) ResultText(DeferredModLoader.StepResult step)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Invalid comparison between Unknown and I4
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected I4, but got Unknown
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (step.InvocationError != null)
		{
			return (State: MlmLocManager.Get("loading.result.invocationError"), Marker: "✕", Color: UiKit.Danger);
		}
		if ((int)step.Mod.state == 1)
		{
			List<LocString> errors = step.Mod.errors;
			if (errors != null && errors.Count > 0)
			{
				return (State: MlmLocManager.Get("loading.result.loadedWithErrors"), Marker: "!", Color: UiKit.Warning);
			}
		}
		ModLoadState state = step.Mod.state;
		return (state - 1) switch
		{
			0 => (State: MlmLocManager.Get("loading.result.loaded"), Marker: "✓", Color: UiKit.Success), 
			1 => (State: MlmLocManager.Get("loading.result.failed"), Marker: "✕", Color: UiKit.Danger), 
			2 => (State: MlmLocManager.Get("loading.result.disabled"), Marker: "−", Color: UiKit.TextDim), 
			3 => (State: MlmLocManager.Get("loading.result.duplicate"), Marker: "−", Color: UiKit.TextDim), 
			4 => (State: MlmLocManager.Get("loading.result.addedAtRuntime"), Marker: "•", Color: UiKit.Text), 
			_ => (State: MlmLocManager.Get("loading.result.notProcessed"), Marker: "?", Color: UiKit.TextMuted), 
		};
	}

	private static void AppendLog(string line, Color color)
	{
		LogLines.Enqueue($"[color=#{((Color)(ref color)).ToHtml(false)}]{Escape(line)}[/color]");
		while (LogLines.Count > 60)
		{
			LogLines.Dequeue();
		}
		if (_deferLogText)
		{
			_logTextStale = true;
		}
		else
		{
			ShowLogText();
		}
	}

	private static void ShowLogText()
	{
		_logTextStale = false;
		if (_loadLog != null && GodotObject.IsInstanceValid((GodotObject)(object)_loadLog))
		{
			_loadLog.Text = string.Join('\n', LogLines);
		}
	}

	private static void ShowCompletedState()
	{
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		FlushSkippedRun();
		LoadClock.Stop();
		UpdateElapsed();
		ShowLoadProgress(DeferredModLoader.LoadsCompleted, DeferredModLoader.LoadsTotal);
		if (_progress != null)
		{
			((Range)_progress).Value = ((Range)_progress).MaxValue;
		}
		int loadedCount = DeferredModLoader.LoadedCount;
		if (_loadingStatus != null)
		{
			_loadingStatus.Text = MlmLocManager.Get("loading.status.modPhaseComplete", ("loaded", loadedCount));
			((Control)_loadingStatus).ThemeTypeVariation = StringName.op_Implicit("SuccessLabel");
		}
		if (_loadingCurrentMod != null)
		{
			_loadingCurrentMod.Text = MlmLocManager.Get("loading.currentMod.complete", ("loaded", loadedCount));
		}
		if (_loadingSpinner != null)
		{
			((Control)_loadingSpinner).Rotation = 0f;
		}
		SetStatus(MlmLocManager.Get("loading.status.complete"), "SuccessLabel", "✓");
		AppendLog(MlmLocManager.Get("loading.log.complete", ("loaded", loadedCount)), UiKit.Success);
		RefreshLoadStates();
		GD.Print($"{"[ModLaunchManager]"} UI frame pump completed; {loadedCount} mod(s) are loaded.");
	}

	private static void ResumeSynchronouslyAndContinue()
	{
		try
		{
			int value = DeferredModLoader.ResumeAll();
			GD.Print($"{"[ModLaunchManager]"} fail-open resume loaded {value} mod(s).");
		}
		catch (Exception value2)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} synchronous resume failed: {value2}");
		}
		finally
		{
			ReleaseStartup();
		}
	}

	private static void ReleaseStartup()
	{
		ClearAiSecrets();
		UpdateAnnouncement.Close();
		_controller?.Dispose();
		_controller = null;
		_promptLayer = null;
		_promptHost = null;
		_promptBar = null;
		_promptSpacer = null;
		_promptSignature = string.Empty;
		_controllerMoveKey = null;
		_detailScroll = null;
		_clearFilters = null;
		LocalModWatcher.Stop();
		LauncherBackground.StopWatching();
		LauncherBackground.CancelLoad();
		LauncherDisplay.RestoreFonts(_sceneTree);
		LauncherDisplay.RestoreFit();
		LauncherDisplay.RestoreSteamNotices();
		LauncherDisplay.RestoreRedraw();
		WorkshopUpdates.Stop();
		WorkshopBranches.Stop();
		StopChangeNotes();
		FriendModSync.Stop();
		FriendModSyncDialog.Close();
		WorkshopCollectionImportDialog.Close();
		WorkshopCollectionArrivals.Clear();
		WorkshopDownloadsDialog.Close();
		WorkshopRemovalPrompt.Close();
		WorkshopSubscription.Reset();
		WorkshopDownloadCenter.Reset();
		_detailWorkshopUnsubscribe = null;
		CloseFabMenu();
		EndTour();
		_fab = null;
		_fabBadge = null;
		_fabRest = null;
		_fabFade = null;
		_fabTucked = false;
		ModListTransferDialog.Close();
		ModPriorityPrompt.Close();
		CancelMoveRepeat();
		ModManager.OnModDetected -= OnVanillaModDetected;
		LoadClock.Stop();
		_workshopTimer = null;
		_friendSyncTimer = null;
		_changeNotesBanner = null;
		_changeNotesBannerText = null;
		_changeNotesOpen = null;
		_detailChangeNotes = null;
		_detailChangeNotesList = null;
		_detailChangeNotesPage = null;
		_detailChangeNotesTranslate = null;
		_moveRepeatTimer = null;
		_pumpActive = false;
		_awaitingLoadDecision = false;
		_loadErrorMod = null;
		if (_sceneTree != null && _framePumpSubscribed)
		{
			_sceneTree.ProcessFrame -= PumpOneFrame;
			_framePumpSubscribed = false;
		}
		if (_sceneTree != null && _workshopScanSubscribed)
		{
			_sceneTree.ProcessFrame -= PumpWorkshopScan;
			_workshopScanSubscribed = false;
		}
		if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			((Node)_layer).QueueFree();
		}
		_layer = null;
		_root = null;
		_languageSelector = null;
		_themeSelector = null;
		_saveProfileSelector = null;
		_settingsButton = null;
		_settingsPage = null;
		_settingsRootView = null;
		_settingsCustomizeView = null;
		_settingsSavesView = null;
		_settingsModsView = null;
		_settingsTitle = null;
		_settingsSubtitle = null;
		_settingsBack = null;
		_settingsFontReset = null;
		_settingsAffectsToggle = null;
		_settingsRandomModToggle = null;
		_settingsVanillaLaunchToggle = null;
		_settingsScroll = null;
		_settingsAutoUnlockToggle = null;
		_settingsModIsolationToggle = null;
		_settingsCloudSyncToggle = null;
		_settingsCloudSyncStatus = null;
		ConfigCloudSync.StatusChanged -= UpdateCloudSyncStatus;
		ConfigCloudSync.Stop();
		_settingsSection = SettingsSection.Root;
		_settingsFontSlider = null;
		_settingsFontValue = null;
		_settingsStatus = null;
		_friendSync = null;
		_transfer = null;
		_filterToggle = null;
		_filterRow = null;
		_groupLine = null;
		_filterSummary = null;
		_filterSummaryText = null;
		_filterClear = null;
		_priorityReport = null;
		_detailDescriptionEditor = null;
		_detailDescriptionActions = null;
		_detailTranslations = null;
		_translationLanguage = null;
		_translationHint = null;
		_translationExport = null;
		_translationImport = null;
		_translationOpen = null;
		_descriptionEditingKey = null;
		_dragPreviewActive = false;
		UiKit.UseCustomBackdrop(null);
		LocalizationBindings.Clear();
		PaletteBindings.Clear();
		OptionEditors.Clear();
		if (!VanillaLaunch.IsActive)
		{
			VanillaLaunch.CompleteModdedLaunch();
			if (_startPressed != 0)
			{
				AutoUnlockAll.RequestForLaunch(_selectedSaveProfileId);
			}
			WorkshopPlaytime.Start(ModManager.Mods);
		}
		else
		{
			MainMenuNotice.Clear();
		}
		StartupGate.Complete();
	}

	private static Control BuildProfileBar()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Expected O, but got Unknown
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Expected O, but got Unknown
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected O, but got Unknown
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected O, but got Unknown
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Expected O, but got Unknown
		PanelContainer val = new PanelContainer
		{
			ThemeTypeVariation = StringName.op_Implicit("BarPanel"),
			SizeFlagsHorizontal = (SizeFlags)3,
			SizeFlagsVertical = (SizeFlags)4
		};
		VBoxContainer val2 = new VBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_profileRow = new HFlowContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_profileRow).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 7);
		((Control)_profileRow).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 7);
		_profileNameRow = new HBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_profileNameRow).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_profileName = new LineEdit
		{
			CustomMinimumSize = new Vector2(240f, 32f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		BindKeyPlaceholder(_profileName, "profiles.name.placeholder");
		LineEdit? profileName = _profileName;
		object obj = _003C_003Ec._003C_003E9__672_0;
		if (obj == null)
		{
			TextSubmittedEventHandler val3 = delegate
			{
				CommitProfile(null);
			};
			_003C_003Ec._003C_003E9__672_0 = val3;
			obj = (object)val3;
		}
		profileName.TextSubmitted += (TextSubmittedEventHandler)obj;
		LineEdit? profileName2 = _profileName;
		object obj2 = _003C_003Ec._003C_003E9__672_1;
		if (obj2 == null)
		{
			TextChangedEventHandler val4 = delegate
			{
				UpdateProfileNameRow();
			};
			_003C_003Ec._003C_003E9__672_1 = val4;
			obj2 = (object)val4;
		}
		profileName2.TextChanged += (TextChangedEventHandler)obj2;
		LineEdit? profileName3 = _profileName;
		object obj3 = _003C_003Ec._003C_003E9__672_2;
		if (obj3 == null)
		{
			GuiInputEventHandler val5 = delegate(InputEvent input)
			{
				if (input.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
				{
					HBoxContainer? profileNameRow = _profileNameRow;
					if (profileNameRow != null && ((CanvasItem)profileNameRow).Visible)
					{
						ShowProfileNameRow(visible: false);
						LineEdit? profileName4 = _profileName;
						if (profileName4 != null)
						{
							((Control)profileName4).AcceptEvent();
						}
					}
				}
			};
			_003C_003Ec._003C_003E9__672_2 = val5;
			obj3 = (object)val5;
		}
		((Control)profileName3).GuiInput += (GuiInputEventHandler)obj3;
		_profileConfirmSave = LocalizedKeyButton("common.action.save", "MiniButton", 64f, 32f);
		((BaseButton)_profileConfirmSave).Pressed += delegate
		{
			CommitProfile(null);
		};
		Button val6 = LocalizedKeyButton("common.action.cancel", "MiniButton", 64f, 32f);
		((BaseButton)val6).Pressed += delegate
		{
			ShowProfileNameRow(visible: false);
		};
		((Node)_profileNameRow).AddChild((Node)(object)_profileName, false, (InternalMode)0);
		((Node)_profileNameRow).AddChild((Node)(object)_profileConfirmSave, false, (InternalMode)0);
		((Node)_profileNameRow).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_profileRow, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_profileNameRow, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		return (Control)val;
	}

	private static void RebuildProfileRow()
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Expected O, but got Unknown
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Expected O, but got Unknown
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Expected O, but got Unknown
		if (_profileRow == null || !GodotObject.IsInstanceValid((GodotObject)(object)_profileRow))
		{
			return;
		}
		_friendSync = null;
		_collectionImport = null;
		_transfer = null;
		_activeProfileChip = null;
		_profileSaveChanges = null;
		foreach (Node child in ((Node)_profileRow).GetChildren(false))
		{
			((Node)_profileRow).RemoveChild(child);
			child.QueueFree();
		}
		Label val = UiKit.Caption(MlmLocManager.Get("profiles.caption"));
		((Control)val).SizeFlagsVertical = (SizeFlags)4;
		((Node)_profileRow).AddChild((Node)(object)val, false, (InternalMode)0);
		_profileGroup = new ButtonGroup();
		if (_profiles.Profiles.Count == 0)
		{
			Label val2 = UiKit.Label(MlmLocManager.Get("profiles.emptyHint"), "DimLabel");
			((Control)val2).SizeFlagsVertical = (SizeFlags)4;
			((Node)_profileRow).AddChild((Node)(object)val2, false, (InternalMode)0);
		}
		else
		{
			foreach (LaunchProfile profile in _profiles.Profiles)
			{
				LaunchProfile captured = profile;
				bool flag = string.Equals(profile.Name, _activeProfile, StringComparison.Ordinal);
				Button val3 = UiKit.Chip(profile.Name, _profileGroup, flag);
				((BaseButton)val3).Disabled = _pumpActive;
				((Control)val3).TooltipText = ProfileChipTooltip(profile, modified: false);
				((BaseButton)val3).Pressed += delegate
				{
					ApplyProfile(captured);
				};
				if (flag)
				{
					_activeProfileChip = val3;
				}
				((Node)_profileRow).AddChild((Node)(object)val3, false, (InternalMode)0);
			}
		}
		((Node)_profileRow).AddChild((Node)new VSeparator
		{
			CustomMinimumSize = new Vector2(1f, 22f)
		}, false, (InternalMode)0);
		if (_activeProfile != null)
		{
			_profileSaveChanges = UiKit.Button(MlmLocManager.Get("profiles.action.overwrite"), "MiniButton", 0f, 30f);
			((BaseButton)_profileSaveChanges).Pressed += delegate
			{
				CommitProfile(_activeProfile);
			};
			((Node)_profileRow).AddChild((Node)(object)_profileSaveChanges, false, (InternalMode)0);
		}
		Button val4 = UiKit.Button(MlmLocManager.Get("profiles.action.saveAs"), "MiniButton", 0f, 30f);
		((Control)val4).TooltipText = MlmLocManager.Get("profiles.action.saveAs.tooltip");
		((BaseButton)val4).Disabled = _pumpActive;
		((BaseButton)val4).Pressed += delegate
		{
			ShowProfileNameRow(visible: true);
		};
		((Node)_profileRow).AddChild((Node)(object)val4, false, (InternalMode)0);
		_collectionImport = UiKit.Button(MlmLocManager.Get("collection.action.open"), "MiniButton", 0f, 30f);
		((BaseButton)_collectionImport).Disabled = _pumpActive || !WorkshopUpdates.IsAvailable;
		((Control)_collectionImport).TooltipText = (WorkshopUpdates.IsAvailable ? MlmLocManager.Get("collection.action.open.tooltip") : MlmLocManager.Get("collection.unavailable"));
		((BaseButton)_collectionImport).Pressed += OpenCollectionImport;
		((Node)_profileRow).AddChild((Node)(object)_collectionImport, false, (InternalMode)0);
		if (_activeProfile != null)
		{
			Button val5 = UiKit.Button(_deleteArmed ? MlmLocManager.Get("common.action.confirmDelete") : MlmLocManager.Get("common.action.delete"), "MiniButton", 0f, 30f);
			((Control)val5).TooltipText = MlmLocManager.Get("profiles.action.delete.tooltip", ("name", _activeProfile));
			((BaseButton)val5).Disabled = _pumpActive;
			((BaseButton)val5).Pressed += DeleteActiveProfile;
			((Node)_profileRow).AddChild((Node)(object)val5, false, (InternalMode)0);
		}
		((Node)_profileRow).AddChild((Node)new VSeparator
		{
			CustomMinimumSize = new Vector2(1f, 22f)
		}, false, (InternalMode)0);
		_friendSync = UiKit.Button(MlmLocManager.Get("friendSync.action.open"), "MiniButton", 0f, 30f);
		((BaseButton)_friendSync).Disabled = _pumpActive || !FriendModSync.CanOpen;
		((Control)_friendSync).TooltipText = (FriendModSync.CanOpen ? MlmLocManager.Get("friendSync.action.open.tooltip") : MlmLocManager.Get("friendSync.unavailable", ("error", FriendModSync.AvailabilityError ?? MlmLocManager.Get("common.value.unknown"))));
		((BaseButton)_friendSync).Pressed += OpenFriendModSync;
		((Node)_profileRow).AddChild((Node)(object)_friendSync, false, (InternalMode)0);
		_transfer = UiKit.Button(MlmLocManager.Get("transfer.action.open"), "MiniButton", 0f, 30f);
		((BaseButton)_transfer).Disabled = _pumpActive;
		((Control)_transfer).TooltipText = MlmLocManager.Get("transfer.action.open.tooltip");
		((BaseButton)_transfer).Pressed += OpenModListTransfer;
		((Node)_profileRow).AddChild((Node)(object)_transfer, false, (InternalMode)0);
		RefreshProfileState();
	}

	private static string ProfileChipTooltip(LaunchProfile profile, bool modified)
	{
		if (!modified)
		{
			return MlmLocManager.Get("profiles.item.tooltip", ("name", profile.Name), ("enabled", profile.Mods.Count((ProfileMod mod) => mod.Enabled)));
		}
		return MlmLocManager.Get("profiles.item.modified.tooltip", ("name", profile.Name));
	}

	private static void RefreshProfileState()
	{
		LaunchProfile launchProfile = ActiveProfile();
		bool flag = launchProfile != null && IsProfileModified(launchProfile);
		if (launchProfile != null && _activeProfileChip != null && GodotObject.IsInstanceValid((GodotObject)(object)_activeProfileChip))
		{
			_activeProfileChip.Text = (flag ? MlmLocManager.Get("profiles.item.modified", ("name", launchProfile.Name)) : launchProfile.Name);
			((Control)_activeProfileChip).TooltipText = ProfileChipTooltip(launchProfile, flag);
		}
		if (launchProfile != null && _profileSaveChanges != null && GodotObject.IsInstanceValid((GodotObject)(object)_profileSaveChanges))
		{
			((BaseButton)_profileSaveChanges).Disabled = !flag || _pumpActive;
			((Control)_profileSaveChanges).TooltipText = (flag ? MlmLocManager.Get("profiles.action.overwrite.tooltip", ("name", launchProfile.Name)) : MlmLocManager.Get("profiles.action.overwrite.unchanged", ("name", launchProfile.Name)));
		}
	}

	private static LaunchProfile? ActiveProfile()
	{
		if (_activeProfile != null)
		{
			return _profiles.Profiles.FirstOrDefault((LaunchProfile profile) => string.Equals(profile.Name, _activeProfile, StringComparison.Ordinal));
		}
		return null;
	}

	private static string PlanFingerprint()
	{
		if (_plan != null)
		{
			return string.Join("\n", _plan.Entries.Select((LaunchEntry entry) => entry.Key + (entry.Enabled ? "+" : "-")));
		}
		return string.Empty;
	}

	private static bool IsProfileModified(LaunchProfile profile)
	{
		if (_plan == null || (_profileBaseline != null && _profileBaseline == PlanFingerprint()))
		{
			return false;
		}
		HashSet<string> installed = (from entry in _plan.Entries
			where entry.CanToggle
			select entry.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		IEnumerable<string> first = (from entry in _plan.Entries
			where entry.CanToggle && entry.Enabled
			select entry.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase);
		IEnumerable<string> second = (from mod in profile.Mods
			where mod.Enabled && installed.Contains(mod.Id)
			select mod.Id).Distinct<string>(StringComparer.OrdinalIgnoreCase);
		return !first.SequenceEqual<string>(second, StringComparer.OrdinalIgnoreCase);
	}

	private static void ShowProfileNameRow(bool visible)
	{
		if (_profileNameRow != null && _profileName != null)
		{
			((CanvasItem)_profileNameRow).Visible = visible;
			if (visible)
			{
				_profileName.Text = string.Empty;
				UpdateProfileNameRow();
				((Control)_profileName).GrabFocus();
			}
			DisarmDelete();
		}
	}

	private static void UpdateProfileNameRow()
	{
		if (_profileName != null && _profileConfirmSave != null)
		{
			string name = _profileName.Text.Trim();
			LaunchProfile launchProfile = _profiles.Profiles.FirstOrDefault((LaunchProfile profile) => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase));
			((BaseButton)_profileConfirmSave).Disabled = name.Length == 0 || _pumpActive;
			_profileConfirmSave.Text = ((launchProfile != null) ? MlmLocManager.Get("profiles.action.replaceExisting") : MlmLocManager.Get("common.action.save"));
			((Control)_profileConfirmSave).TooltipText = ((launchProfile != null) ? MlmLocManager.Get("profiles.action.replaceExisting.tooltip", ("name", launchProfile.Name)) : string.Empty);
		}
	}

	private static PlanResult? ApplyProfile(LaunchProfile profile)
	{
		if (_plan == null || _pumpActive)
		{
			return null;
		}
		if (string.Equals(profile.Name, _activeProfile, StringComparison.Ordinal) && !IsProfileModified(profile))
		{
			return null;
		}
		DisarmDelete();
		string text = PlanFingerprint();
		RecordUndo();
		PlanResult planResult = _plan.ApplyProfile(profile);
		if (planResult.Success)
		{
			_activeProfile = profile.Name;
			_profiles.Active = profile.Name;
			_profileBaseline = PlanFingerprint();
			PersistProfiles();
			if (_profileBaseline != text)
			{
				planResult = planResult with
				{
					Message = MlmLocManager.Get("profiles.apply.undoHint", ("result", planResult.Message))
				};
			}
		}
		ShowFeedback(planResult);
		UpdateDetails();
		UpdatePlanSummary();
		QueueRebuild();
		Defer(RebuildProfileRow);
		return planResult;
	}

	private static void OpenCollectionImport()
	{
		if (_plan != null && !_pumpActive && WorkshopUpdates.IsAvailable)
		{
			DisarmDelete();
			WorkshopCollectionImportDialog.Show(_plan, CollectionImportHost());
		}
	}

	private static WorkshopCollectionHost CollectionImportHost()
	{
		return new WorkshopCollectionHost(() => _profiles.Profiles, SaveImportedProfile, OpenDownloads, RequestWorkshopRemoval, CanRemoveWorkshopItem);
	}

	private static PlanResult SaveImportedProfile(LaunchProfile profile, bool apply)
	{
		if (_plan == null || _pumpActive)
		{
			return PlanResult.Error(MlmLocManager.Get("collection.save.unavailable"));
		}
		profile.Name = profile.Name.Trim();
		if (profile.Name.Length == 0)
		{
			return PlanResult.Error(MlmLocManager.Get("profiles.error.nameRequired"));
		}
		List<LaunchProfile> list = new List<LaunchProfile>(_profiles.Profiles);
		int num = list.FindIndex((LaunchProfile candidate) => string.Equals(candidate.Name, profile.Name, StringComparison.OrdinalIgnoreCase));
		if (num >= 0)
		{
			profile.Name = list[num].Name;
			list[num] = profile;
		}
		else
		{
			list.Add(profile);
		}
		PlanResult result = ProfileStore.Save(new ProfileDocument
		{
			Version = _profiles.Version,
			Active = _profiles.Active,
			Profiles = list
		});
		if (!result.Success)
		{
			return result;
		}
		_profiles.Profiles = list;
		if (string.Equals(profile.Name, _activeProfile, StringComparison.Ordinal))
		{
			_profileBaseline = null;
		}
		DisarmDelete();
		Defer(RebuildProfileRow);
		PlanResult result2 = PlanResult.Ok(MlmLocManager.Get("collection.saved", ("name", profile.Name), ("count", profile.Mods.Count)));
		if (!apply)
		{
			ShowFeedback(result2);
			return result2;
		}
		PlanResult? planResult = ApplyProfile(profile);
		if (planResult.HasValue)
		{
			PlanResult valueOrDefault = planResult.GetValueOrDefault();
			if (valueOrDefault.Success)
			{
				PlanResult result3 = valueOrDefault with
				{
					Message = MlmLocManager.Get("collection.savedAndApplied", ("name", profile.Name), ("count", profile.Mods.Count), ("result", valueOrDefault.Message))
				};
				ShowFeedback(result3);
				return result3;
			}
			PlanResult result4 = PlanResult.Warning(MlmLocManager.Get("collection.savedNotApplied", ("name", profile.Name), ("error", valueOrDefault.Message)));
			ShowFeedback(result4);
			return result4;
		}
		ShowFeedback(result2);
		return result2;
	}

	private static void CommitProfile(string? name)
	{
		if (_plan == null || _pumpActive)
		{
			return;
		}
		object obj = name;
		if (obj == null)
		{
			LineEdit? profileName = _profileName;
			obj = ((profileName != null) ? profileName.Text : null) ?? string.Empty;
		}
		string target = ((string)obj).Trim();
		if (target.Length == 0)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("profiles.error.nameRequired")));
			return;
		}
		LaunchProfile launchProfile = _plan.CaptureProfile(target);
		int num = _profiles.Profiles.FindIndex((LaunchProfile profile) => string.Equals(profile.Name, target, StringComparison.OrdinalIgnoreCase));
		if (num >= 0)
		{
			KeepUninstalledMods(_profiles.Profiles[num], launchProfile);
			launchProfile.SourceCollection = _profiles.Profiles[num].SourceCollection;
			_profiles.Profiles[num] = launchProfile;
		}
		else
		{
			_profiles.Profiles.Add(launchProfile);
		}
		_activeProfile = target;
		_profiles.Active = target;
		_profileBaseline = PlanFingerprint();
		PlanResult planResult = ProfileStore.Save(_profiles);
		ShowFeedback(planResult.Success ? PlanResult.Ok(MlmLocManager.Get("profiles.saved", ("name", target), ("enabled", _plan.EnabledCount), ("total", _plan.Entries.Count))) : planResult);
		ShowProfileNameRow(visible: false);
		Defer(RebuildProfileRow);
	}

	private static void KeepUninstalledMods(LaunchProfile previous, LaunchProfile updated)
	{
		HashSet<string> hashSet = updated.Mods.Select((ProfileMod mod) => mod.Id).ToHashSet<string>(StringComparer.OrdinalIgnoreCase);
		string after = null;
		foreach (ProfileMod mod in previous.Mods)
		{
			if (!hashSet.Contains(mod.Id))
			{
				int index = ((after != null) ? (updated.Mods.FindLastIndex((ProfileMod candidate) => string.Equals(candidate.Id, after, StringComparison.OrdinalIgnoreCase)) + 1) : 0);
				updated.Mods.Insert(index, mod);
			}
			after = mod.Id;
		}
	}

	private static void DeleteActiveProfile()
	{
		if (_activeProfile == null || _pumpActive)
		{
			return;
		}
		if (!_deleteArmed)
		{
			_deleteArmed = true;
			ShowFeedback(PlanResult.Warning(MlmLocManager.Get("profiles.delete.confirm", ("name", _activeProfile))));
			Defer(RebuildProfileRow);
			return;
		}
		string removed = _activeProfile;
		_profiles.Profiles.RemoveAll((LaunchProfile profile) => string.Equals(profile.Name, removed, StringComparison.Ordinal));
		_activeProfile = null;
		_profiles.Active = null;
		_profileBaseline = null;
		_deleteArmed = false;
		PlanResult planResult = ProfileStore.Save(_profiles);
		ShowFeedback(planResult.Success ? PlanResult.Ok(MlmLocManager.Get("profiles.deleted", ("name", removed))) : planResult);
		Defer(RebuildProfileRow);
	}

	private static void DisarmDelete()
	{
		if (_deleteArmed)
		{
			_deleteArmed = false;
			Defer(RebuildProfileRow);
		}
	}

	private static void PersistProfiles()
	{
		PlanResult planResult = ProfileStore.Save(_profiles);
		if (!planResult.Success)
		{
			GD.PushWarning("[ModLaunchManager] " + planResult.Message);
		}
	}

	private static Control BuildSettingsPage()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected O, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Expected O, but got Unknown
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Expected O, but got Unknown
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Expected O, but got Unknown
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0432: Expected O, but got Unknown
		Control? obj = (_settingsPage = UiKit.FullRect<Control>(new Control
		{
			Name = StringName.op_Implicit("LauncherSettingsPage"),
			Visible = false,
			MouseFilter = (MouseFilterEnum)0
		}));
		ColorRect backdrop = new ColorRect
		{
			Color = UiKit.Alpha(UiKit.Palette.Canvas, 0.82f),
			MouseFilter = (MouseFilterEnum)0
		};
		BindPalette((GodotObject)(object)backdrop, delegate
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			backdrop.Color = UiKit.Alpha(UiKit.Palette.Canvas, 0.82f);
		});
		((Node)obj).AddChild((Node)(object)UiKit.FullRect<ColorRect>(backdrop), false, (InternalMode)0);
		PanelContainer val = (_settingsCard = UiKit.Card());
		((Control)val).AnchorLeft = 0.31f;
		((Control)val).AnchorTop = 0.18f;
		((Control)val).AnchorRight = 0.69f;
		((Control)val).AnchorBottom = 0.82f;
		((Control)val).OffsetLeft = 0f;
		((Control)val).OffsetTop = 0f;
		((Control)val).OffsetRight = 0f;
		((Control)val).OffsetBottom = 0f;
		MarginContainer val2 = UiKit.Margin((Control)new VBoxContainer(), 22, 20);
		VBoxContainer val3 = (VBoxContainer)((Node)val2).GetChild(0, false);
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		HBoxContainer val4 = new HBoxContainer();
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 9);
		_settingsBack = UiKit.Button("‹", "AccentIconButton", 34f, 32f);
		((Control)_settingsBack).SizeFlagsVertical = (SizeFlags)0;
		((Control)_settingsBack).MouseDefaultCursorShape = (CursorShape)2;
		((CanvasItem)_settingsBack).Visible = false;
		BindKeyTooltip((Control)(object)_settingsBack, "settings.action.back.tooltip");
		((BaseButton)_settingsBack).Pressed += delegate
		{
			ShowSettingsSection(SettingsSection.Root);
		};
		_settingsTitle = UiKit.Label(string.Empty, "SectionLabel");
		((Control)_settingsTitle).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)_settingsTitle).SizeFlagsVertical = (SizeFlags)4;
		Button val5 = UiKit.Button("✕", "AccentIconButton", 34f, 32f);
		((Control)val5).SizeFlagsVertical = (SizeFlags)0;
		((Control)val5).MouseDefaultCursorShape = (CursorShape)2;
		BindKeyTooltip((Control)(object)val5, "settings.action.close.tooltip");
		((BaseButton)val5).Pressed += CloseSettings;
		((Node)val4).AddChild((Node)(object)_settingsBack, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_settingsTitle, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		_settingsSubtitle = UiKit.Label(string.Empty, "DimLabel");
		_settingsSubtitle.AutowrapMode = (AutowrapMode)3;
		((Node)val3).AddChild((Node)(object)_settingsSubtitle, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		_settingsScroll = new ScrollContainer
		{
			HorizontalScrollMode = (ScrollMode)0,
			SizeFlagsVertical = (SizeFlags)3
		};
		UiKit.FollowFocus(_settingsScroll);
		VBoxContainer val6 = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Node)val6).AddChild((Node)(object)BuildSettingsRootView(), false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)BuildSettingsCustomizeView(), false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)BuildSettingsSavesView(), false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)BuildSettingsModsView(), false, (InternalMode)0);
		((Node)_settingsScroll).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_settingsScroll, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)BuildSettingsAiView(), false, (InternalMode)0);
		_settingsStatus = UiKit.Label(string.Empty, "DimLabel");
		_settingsStatus.AutowrapMode = (AutowrapMode)3;
		((Node)val3).AddChild((Node)(object)_settingsStatus, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		_settingsFontReset = LocalizedKeyButton("settings.fontSize.reset", "SecondaryButton", 0f, 34f);
		((CanvasItem)_settingsFontReset).Visible = false;
		((BaseButton)_settingsFontReset).Pressed += delegate
		{
			if (_settingsFontSlider != null)
			{
				((Range)_settingsFontSlider).Value = 100.0;
			}
		};
		Button val8 = LocalizedKeyButton("settings.action.done", "PrimaryButton", 90f, 34f);
		((BaseButton)val8).Pressed += CloseSettings;
		((Node)val7).AddChild((Node)(object)_settingsFontReset, false, (InternalMode)0);
		((Node)val7).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val7, false, (InternalMode)0);
		BindLocalization((Control)(object)_settingsFontValue, UpdateSettingsFontValue);
		BindLocalization((Control)(object)_settingsTitle, UpdateSettingsSectionChrome);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)obj).AddChild((Node)(object)val, false, (InternalMode)0);
		return obj;
	}

	private static Control BuildSettingsRootView()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_settingsRootView = (Control?)val;
		((Node)val).AddChild((Node)(object)LocalizedKeyLabel("settings.sections.caption", "CaptionLabel"), false, (InternalMode)0);
		Button val2 = LocalizedKeyButton("settings.customize.open");
		((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
		val2.Alignment = (HorizontalAlignment)0;
		BindKeyTooltip((Control)(object)val2, "settings.customize.open.tooltip");
		((BaseButton)val2).Pressed += delegate
		{
			ShowSettingsSection(SettingsSection.Customize);
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Label val3 = LocalizedKeyLabel("settings.customize.open.description", "DimLabel");
		val3.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		Button val4 = LocalizedKeyButton("settings.saves.title");
		((Control)val4).SizeFlagsHorizontal = (SizeFlags)3;
		val4.Alignment = (HorizontalAlignment)0;
		BindKeyTooltip((Control)(object)val4, "settings.saves.description");
		((BaseButton)val4).Pressed += delegate
		{
			ShowSettingsSection(SettingsSection.Saves);
		};
		((Node)val).AddChild((Node)(object)val4, false, (InternalMode)0);
		Label val5 = LocalizedKeyLabel("settings.saves.description", "DimLabel");
		val5.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		Button val6 = LocalizedKeyButton("settings.mods.title");
		((Control)val6).SizeFlagsHorizontal = (SizeFlags)3;
		val6.Alignment = (HorizontalAlignment)0;
		BindKeyTooltip((Control)(object)val6, "settings.mods.description");
		((BaseButton)val6).Pressed += delegate
		{
			ShowSettingsSection(SettingsSection.Mods);
		};
		((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		Label val7 = LocalizedKeyLabel("settings.mods.description", "DimLabel");
		val7.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val7, false, (InternalMode)0);
		Button val8 = (_settingsAiEntry = LocalizedKeyButton("ai.settings.title"));
		val8.Alignment = (HorizontalAlignment)0;
		((BaseButton)val8).Pressed += delegate
		{
			ShowSettingsSection(SettingsSection.Ai);
		};
		((Node)val).AddChild((Node)(object)val8, false, (InternalMode)0);
		return (Control)val;
	}

	private static Control BuildSettingsCustomizeView()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Expected O, but got Unknown
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Expected O, but got Unknown
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Expected O, but got Unknown
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_settingsCustomizeView = (Control?)val;
		((Node)val).AddChild((Node)(object)LocalizedKeyLabel("settings.buttons.caption", "CaptionLabel"), false, (InternalMode)0);
		_settingsRandomModToggle = BuildButtonVisibilityToggle(val, "settings.buttons.randomMod", MlmLocManager.ShowRandomModButton, OnRandomModVisibilityToggled);
		_settingsVanillaLaunchToggle = BuildButtonVisibilityToggle(val, "settings.buttons.vanillaLaunch", MlmLocManager.ShowVanillaLaunchButton, OnVanillaLaunchVisibilityToggled);
		((Node)val).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)LocalizedKeyLabel("settings.appearance.caption", "CaptionLabel"), false, (InternalMode)0);
		HBoxContainer val2 = new HBoxContainer();
		Label val3 = LocalizedKeyLabel("settings.fontSize.label", "MutedLabel");
		((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
		_settingsFontValue = UiKit.Label(string.Empty, "MutedLabel");
		_settingsFontValue.HorizontalAlignment = (HorizontalAlignment)2;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_settingsFontValue, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		_settingsFontSlider = new HSlider
		{
			MinValue = 85.0,
			MaxValue = 125.0,
			Step = 5.0,
			Value = MlmLocManager.FontScale * 100f,
			CustomMinimumSize = new Vector2(0f, 34f),
			SizeFlagsHorizontal = (SizeFlags)3,
			FocusMode = (FocusModeEnum)2
		};
		LiquidGlass.DecorateSlider(_settingsFontSlider);
		BindKeyTooltip((Control)(object)_settingsFontSlider, "settings.fontSize.tooltip");
		HSlider? settingsFontSlider = _settingsFontSlider;
		object obj = _003C_003EO._003C129_003E__OnFontScaleChanged;
		if (obj == null)
		{
			ValueChangedEventHandler val4 = OnFontScaleChanged;
			_003C_003EO._003C129_003E__OnFontScaleChanged = val4;
			obj = (object)val4;
		}
		((Range)settingsFontSlider).ValueChanged += (ValueChangedEventHandler)obj;
		((Node)val).AddChild((Node)(object)_settingsFontSlider, false, (InternalMode)0);
		Label val5 = LocalizedKeyLabel("settings.fontSize.hint", "DimLabel");
		val5.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)LocalizedKeyLabel("settings.columns.caption", "CaptionLabel"), false, (InternalMode)0);
		HBoxContainer val6 = new HBoxContainer();
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val7 = LocalizedKeyLabel("settings.columns.affectsGameplay.label", "MutedLabel");
		((Control)val7).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val7).SizeFlagsVertical = (SizeFlags)4;
		_settingsAffectsToggle = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)_settingsAffectsToggle).ToggleMode = true;
		((BaseButton)_settingsAffectsToggle).ButtonPressed = _showAffectsColumn;
		((Control)_settingsAffectsToggle).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_settingsAffectsToggle, "settings.columns.affectsGameplay.tooltip");
		BindLocalization((Control)(object)_settingsAffectsToggle, UpdateAffectsToggleText);
		Button? settingsAffectsToggle = _settingsAffectsToggle;
		object obj2 = _003C_003EO._003C131_003E__OnAffectsColumnToggled;
		if (obj2 == null)
		{
			ToggledEventHandler val8 = OnAffectsColumnToggled;
			_003C_003EO._003C131_003E__OnAffectsColumnToggled = val8;
			obj2 = (object)val8;
		}
		((BaseButton)settingsAffectsToggle).Toggled += (ToggledEventHandler)obj2;
		((Node)val6).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_settingsAffectsToggle, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		Label val9 = LocalizedKeyLabel("settings.columns.affectsGameplay.hint", "DimLabel");
		val9.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val9, false, (InternalMode)0);
		return (Control)val;
	}

	private static Button BuildButtonVisibilityToggle(VBoxContainer view, string labelKey, bool visible, Action<bool> toggled)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Expected O, but got Unknown
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val2 = LocalizedKeyLabel(labelKey, "MutedLabel");
		((Control)val2).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val2).SizeFlagsVertical = (SizeFlags)4;
		val2.AutowrapMode = (AutowrapMode)3;
		Button toggle = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)toggle).ToggleMode = true;
		((BaseButton)toggle).ButtonPressed = visible;
		((Control)toggle).SizeFlagsVertical = (SizeFlags)4;
		BindLocalization((Control)(object)toggle, delegate
		{
			UpdateVisibilityToggleText(toggle);
		});
		((BaseButton)toggle).Toggled += (ToggledEventHandler)delegate(bool value)
		{
			toggled(value);
		};
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)toggle, false, (InternalMode)0);
		((Node)view).AddChild((Node)(object)val, false, (InternalMode)0);
		return toggle;
	}

	private static void UpdateVisibilityToggleText(Button? toggle)
	{
		if (toggle != null && GodotObject.IsInstanceValid((GodotObject)(object)toggle))
		{
			toggle.Text = MlmLocManager.Get(((BaseButton)toggle).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		}
	}

	private static void OnRandomModVisibilityToggled(bool show)
	{
		string error;
		bool saved = MlmLocManager.SetShowRandomModButton(show, out error);
		if (_randomMod != null)
		{
			((CanvasItem)_randomMod).Visible = show;
		}
		UpdateVisibilityToggleText(_settingsRandomModToggle);
		ReportButtonVisibilitySaved(saved);
	}

	private static void OnVanillaLaunchVisibilityToggled(bool show)
	{
		string error;
		bool saved = MlmLocManager.SetShowVanillaLaunchButton(show, out error);
		if (_vanillaStart != null)
		{
			((CanvasItem)_vanillaStart).Visible = show;
		}
		UpdateVisibilityToggleText(_settingsVanillaLaunchToggle);
		ReportButtonVisibilitySaved(saved);
	}

	private static void ReportButtonVisibilitySaved(bool saved)
	{
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = MlmLocManager.Get(saved ? "settings.buttons.saved" : "settings.buttons.saveFailed");
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(saved ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static Control BuildSettingsSavesView()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Expected O, but got Unknown
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Expected O, but got Unknown
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Expected O, but got Unknown
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0392: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_settingsSavesView = (Control?)val;
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val3 = LocalizedKeyLabel("settings.saves.autoUnlock.label", "MutedLabel");
		((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val3).SizeFlagsVertical = (SizeFlags)4;
		_settingsAutoUnlockToggle = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)_settingsAutoUnlockToggle).ToggleMode = true;
		((BaseButton)_settingsAutoUnlockToggle).ButtonPressed = MlmLocManager.AutoUnlockAll;
		((BaseButton)_settingsAutoUnlockToggle).Disabled = !AutoUnlockAll.Available;
		((Control)_settingsAutoUnlockToggle).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_settingsAutoUnlockToggle, "settings.saves.autoUnlock.tooltip");
		BindLocalization((Control)(object)_settingsAutoUnlockToggle, UpdateAutoUnlockToggleText);
		Button? settingsAutoUnlockToggle = _settingsAutoUnlockToggle;
		object obj = _003C_003EO._003C133_003E__OnAutoUnlockToggled;
		if (obj == null)
		{
			ToggledEventHandler val4 = OnAutoUnlockToggled;
			_003C_003EO._003C133_003E__OnAutoUnlockToggled = val4;
			obj = (object)val4;
		}
		((BaseButton)settingsAutoUnlockToggle).Toggled += (ToggledEventHandler)obj;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_settingsAutoUnlockToggle, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Label val5 = LocalizedKeyLabel("settings.saves.autoUnlock.tooltip", "DimLabel");
		val5.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		Label val6 = LocalizedKeyLabel(AutoUnlockAll.Available ? "settings.saves.autoUnlock.hint" : "settings.saves.autoUnlock.unavailable", "DimLabel");
		val6.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)UiKit.Divider(), false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)LocalizedKeyLabel("settings.cloudSync.caption", "CaptionLabel"), false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val8 = LocalizedKeyLabel("settings.cloudSync.label", "MutedLabel");
		((Control)val8).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val8).SizeFlagsVertical = (SizeFlags)4;
		val8.AutowrapMode = (AutowrapMode)3;
		_settingsCloudSyncToggle = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)_settingsCloudSyncToggle).ToggleMode = true;
		((BaseButton)_settingsCloudSyncToggle).ButtonPressed = MlmLocManager.CloudSync;
		((Control)_settingsCloudSyncToggle).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_settingsCloudSyncToggle, "settings.cloudSync.tooltip");
		BindLocalization((Control)(object)_settingsCloudSyncToggle, UpdateCloudSyncToggleText);
		Button? settingsCloudSyncToggle = _settingsCloudSyncToggle;
		object obj2 = _003C_003EO._003C135_003E__OnCloudSyncToggled;
		if (obj2 == null)
		{
			ToggledEventHandler val9 = OnCloudSyncToggled;
			_003C_003EO._003C135_003E__OnCloudSyncToggled = val9;
			obj2 = (object)val9;
		}
		((BaseButton)settingsCloudSyncToggle).Toggled += (ToggledEventHandler)obj2;
		((Node)val7).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_settingsCloudSyncToggle, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val7, false, (InternalMode)0);
		Label val10 = LocalizedKeyLabel("settings.cloudSync.tooltip", "DimLabel");
		val10.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val10, false, (InternalMode)0);
		Label val11 = LocalizedKeyLabel("settings.cloudSync.hint", "DimLabel");
		val11.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val11, false, (InternalMode)0);
		_settingsCloudSyncStatus = UiKit.Label(string.Empty, "MutedLabel");
		_settingsCloudSyncStatus.AutowrapMode = (AutowrapMode)3;
		BindLocalization((Control)(object)_settingsCloudSyncStatus, UpdateCloudSyncStatus);
		ConfigCloudSync.StatusChanged -= UpdateCloudSyncStatus;
		ConfigCloudSync.StatusChanged += UpdateCloudSyncStatus;
		((Node)val).AddChild((Node)(object)_settingsCloudSyncStatus, false, (InternalMode)0);
		return (Control)val;
	}

	private static Control BuildSettingsModsView()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Expected O, but got Unknown
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Expected O, but got Unknown
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Expected O, but got Unknown
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Expected O, but got Unknown
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Expected O, but got Unknown
		VBoxContainer val = new VBoxContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_settingsModsView = (Control?)val;
		HBoxContainer val2 = new HBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val3 = LocalizedKeyLabel("settings.mods.isolation.label", "MutedLabel");
		((Control)val3).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val3).SizeFlagsVertical = (SizeFlags)4;
		val3.AutowrapMode = (AutowrapMode)3;
		_settingsModIsolationToggle = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)_settingsModIsolationToggle).ToggleMode = true;
		((BaseButton)_settingsModIsolationToggle).ButtonPressed = MlmLocManager.ModIsolation;
		((Control)_settingsModIsolationToggle).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_settingsModIsolationToggle, "settings.mods.isolation.description");
		BindLocalization((Control)(object)_settingsModIsolationToggle, UpdateModIsolationToggleText);
		Button? settingsModIsolationToggle = _settingsModIsolationToggle;
		object obj = _003C_003EO._003C137_003E__OnModIsolationToggled;
		if (obj == null)
		{
			ToggledEventHandler val4 = OnModIsolationToggled;
			_003C_003EO._003C137_003E__OnModIsolationToggled = val4;
			obj = (object)val4;
		}
		((BaseButton)settingsModIsolationToggle).Toggled += (ToggledEventHandler)obj;
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_settingsModIsolationToggle, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		Label val5 = LocalizedKeyLabel("settings.mods.isolation.description", "DimLabel");
		val5.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val5, false, (InternalMode)0);
		Label val6 = LocalizedKeyLabel("settings.mods.isolation.hint", "DimLabel");
		val6.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val6, false, (InternalMode)0);
		((Node)val).AddChild((Node)new Control
		{
			CustomMinimumSize = new Vector2(0f, 6f)
		}, false, (InternalMode)0);
		HBoxContainer val7 = new HBoxContainer();
		((Control)val7).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		Label val8 = LocalizedKeyLabel("settings.mods.keepDisabled.label", "MutedLabel");
		((Control)val8).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)val8).SizeFlagsVertical = (SizeFlags)4;
		val8.AutowrapMode = (AutowrapMode)3;
		_settingsKeepDisabledToggle = UiKit.Button(string.Empty, "SwitchButton", 62f, 30f);
		((BaseButton)_settingsKeepDisabledToggle).ToggleMode = true;
		((BaseButton)_settingsKeepDisabledToggle).ButtonPressed = MlmLocManager.KeepDisabledInPlace;
		((Control)_settingsKeepDisabledToggle).SizeFlagsVertical = (SizeFlags)4;
		BindKeyTooltip((Control)(object)_settingsKeepDisabledToggle, "settings.mods.keepDisabled.description");
		BindLocalization((Control)(object)_settingsKeepDisabledToggle, UpdateKeepDisabledToggleText);
		Button? settingsKeepDisabledToggle = _settingsKeepDisabledToggle;
		object obj2 = _003C_003EO._003C139_003E__OnKeepDisabledToggled;
		if (obj2 == null)
		{
			ToggledEventHandler val9 = OnKeepDisabledToggled;
			_003C_003EO._003C139_003E__OnKeepDisabledToggled = val9;
			obj2 = (object)val9;
		}
		((BaseButton)settingsKeepDisabledToggle).Toggled += (ToggledEventHandler)obj2;
		((Node)val7).AddChild((Node)(object)val8, false, (InternalMode)0);
		((Node)val7).AddChild((Node)(object)_settingsKeepDisabledToggle, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val7, false, (InternalMode)0);
		Label val10 = LocalizedKeyLabel("settings.mods.keepDisabled.description", "DimLabel");
		val10.AutowrapMode = (AutowrapMode)3;
		((Node)val).AddChild((Node)(object)val10, false, (InternalMode)0);
		return (Control)val;
	}

	private static void UpdateKeepDisabledToggleText()
	{
		if (_settingsKeepDisabledToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsKeepDisabledToggle))
		{
			_settingsKeepDisabledToggle.Text = MlmLocManager.Get(((BaseButton)_settingsKeepDisabledToggle).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		}
	}

	private static void OnKeepDisabledToggled(bool keep)
	{
		if (_pumpActive)
		{
			Button? settingsKeepDisabledToggle = _settingsKeepDisabledToggle;
			if (settingsKeepDisabledToggle != null)
			{
				((BaseButton)settingsKeepDisabledToggle).SetPressedNoSignal(!keep);
			}
			UpdateKeepDisabledToggleText();
			return;
		}
		string error;
		bool flag = MlmLocManager.SetKeepDisabledInPlace(keep, out error);
		int num;
		if (keep)
		{
			LaunchPlan? plan = _plan;
			num = ((plan != null && plan.DisabledCount > 0) ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag2 = (byte)num != 0;
		LaunchPlan.KeepDisabledInPlace = keep;
		_plan?.ApplyDisabledPlacement();
		UpdateKeepDisabledToggleText();
		UpdateGroupViewToggle();
		UpdateDetails();
		UpdatePlanSummary();
		QueueRebuild(scrollToSelection: true);
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = MlmLocManager.Get((!flag) ? "settings.mods.keepDisabled.saveFailed" : ((!keep) ? "settings.mods.keepDisabled.disabled" : (flag2 ? "settings.mods.keepDisabled.enabledAlreadyOff" : "settings.mods.keepDisabled.enabled")));
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(flag ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static void UpdateModIsolationToggleText()
	{
		if (_settingsModIsolationToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsModIsolationToggle))
		{
			_settingsModIsolationToggle.Text = MlmLocManager.Get(((BaseButton)_settingsModIsolationToggle).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		}
	}

	private static void OnModIsolationToggled(bool enabled)
	{
		string error;
		bool flag = MlmLocManager.SetModIsolation(enabled, out error);
		UpdateModIsolationToggleText();
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = MlmLocManager.Get((!flag) ? "settings.mods.isolation.saveFailed" : (enabled ? "settings.mods.isolation.enabled" : "settings.mods.isolation.disabled"));
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(flag ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static void UpdateCloudSyncToggleText()
	{
		if (_settingsCloudSyncToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsCloudSyncToggle))
		{
			_settingsCloudSyncToggle.Text = MlmLocManager.Get(((BaseButton)_settingsCloudSyncToggle).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		}
	}

	private static void UpdateCloudSyncStatus()
	{
		if (_settingsCloudSyncStatus != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsCloudSyncStatus))
		{
			Label settingsCloudSyncStatus = _settingsCloudSyncStatus;
			settingsCloudSyncStatus.Text = MlmLocManager.Get(ConfigCloudSync.Status switch
			{
				ConfigCloudStatus.Off => "settings.cloudSync.status.off", 
				ConfigCloudStatus.SteamUnavailable => "settings.cloudSync.status.steamUnavailable", 
				ConfigCloudStatus.CloudDisabled => "settings.cloudSync.status.cloudDisabled", 
				ConfigCloudStatus.WaitingForGameSaves => "settings.cloudSync.status.waiting", 
				ConfigCloudStatus.Synced => "settings.cloudSync.status.synced", 
				ConfigCloudStatus.Failed => "settings.cloudSync.status.failed", 
				_ => "settings.cloudSync.status.unknown", 
			});
			settingsCloudSyncStatus = _settingsCloudSyncStatus;
			((Control)settingsCloudSyncStatus).ThemeTypeVariation = StringName.op_Implicit(ConfigCloudSync.Status switch
			{
				ConfigCloudStatus.Synced => "SuccessLabel", 
				ConfigCloudStatus.Failed => "WarningLabel", 
				_ => "MutedLabel", 
			});
		}
	}

	private static void OnCloudSyncToggled(bool enabled)
	{
		string error;
		bool flag = MlmLocManager.SetCloudSync(enabled, out error);
		UpdateCloudSyncToggleText();
		if (flag)
		{
			ConfigCloudSync.Resume();
		}
		UpdateCloudSyncStatus();
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = MlmLocManager.Get((!flag) ? "settings.cloudSync.saveFailed" : (enabled ? "settings.cloudSync.enabled" : "settings.cloudSync.disabled"));
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(flag ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static Label LocalizedKeyLabel(string key, string variation = "")
	{
		Label obj = UiKit.Label(string.Empty, variation);
		BindKeyText(obj, key);
		return obj;
	}

	private static Label LocalizedKeyCaption(string key)
	{
		return LocalizedKeyLabel(key, "CaptionLabel");
	}

	private static Button LocalizedKeyButton(string key, string variation = "SecondaryButton", float minimumWidth = 0f, float height = 40f)
	{
		Button obj = UiKit.Button(string.Empty, variation, minimumWidth, height);
		BindKeyText(obj, key);
		return obj;
	}

	private static void BindKeyText(Label label, string key)
	{
		BindLocalization((Control)(object)label, delegate
		{
			label.Text = MlmLocManager.Get(key);
		});
	}

	private static void BindKeyText(Button button, string key)
	{
		BindLocalization((Control)(object)button, delegate
		{
			button.Text = MlmLocManager.Get(key);
		});
	}

	private static void BindKeyPlaceholder(LineEdit lineEdit, string key)
	{
		BindLocalization((Control)(object)lineEdit, delegate
		{
			lineEdit.PlaceholderText = MlmLocManager.Get(key);
		});
	}

	private static void BindKeyTooltip(Control control, string key)
	{
		BindLocalization(control, delegate
		{
			control.TooltipText = MlmLocManager.Get(key);
		});
	}

	private static void BindLocalization(Control owner, Action apply)
	{
		LocalizationBindings.Add(SafeApply);
		SafeApply();
		void SafeApply()
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)owner))
			{
				apply();
			}
		}
	}

	private static void BindPalette(GodotObject owner, Action apply)
	{
		PaletteBindings.Add(SafeApply);
		SafeApply();
		void SafeApply()
		{
			if (GodotObject.IsInstanceValid(owner))
			{
				apply();
			}
		}
	}

	private static void ApplyPaletteBindings()
	{
		foreach (Action paletteBinding in PaletteBindings)
		{
			paletteBinding();
		}
	}

	private static PanelContainer PaletteBadge(PanelContainer badge, BadgeTone tone)
	{
		return PaletteBadge(badge, () => tone);
	}

	private static PanelContainer PaletteBadge(PanelContainer badge, Func<BadgeTone> tone)
	{
		BindPalette((GodotObject)(object)badge, delegate
		{
			UiKit.RestyleBadge(badge, tone(), onReadingBand: true);
		});
		return badge;
	}

	private static Color[] WashRamp()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		return (Color[])(object)new Color[3]
		{
			UiKit.Alpha(UiKit.Accent, 0.045f),
			UiKit.Alpha(UiKit.Accent, 0.015f),
			UiKit.Clear
		};
	}

	private static Color[] TopRuleRamp()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		return (Color[])(object)new Color[3]
		{
			UiKit.Alpha(UiKit.Accent, 0.06f),
			UiKit.Alpha(UiKit.Accent, 0.32f),
			UiKit.Alpha(UiKit.Accent, 0.04f)
		};
	}

	private static void ApplyLocalizationBindings()
	{
		foreach (Action localizationBinding in LocalizationBindings)
		{
			localizationBinding();
		}
	}

	private static void PopulateThemeSelector()
	{
		if (_themeSelector == null || !GodotObject.IsInstanceValid((GodotObject)(object)_themeSelector))
		{
			return;
		}
		_updatingThemeSelector = true;
		try
		{
			_themeSelector.Clear();
			LauncherTheme[] array = new LauncherTheme[5]
			{
				LauncherTheme.System,
				LauncherTheme.Fluent,
				LauncherTheme.FluentDark,
				LauncherTheme.LiquidGlass,
				LauncherTheme.LiquidGlassDark
			};
			for (int i = 0; i < array.Length; i++)
			{
				LauncherTheme launcherTheme = array[i];
				_themeSelector.AddItem(MlmLocManager.ThemeLabel(launcherTheme), (int)launcherTheme);
				if (launcherTheme == MlmLocManager.ThemePreference)
				{
					_themeSelector.Selected = i;
				}
			}
			BackgroundSelectorValues.Clear();
			BackgroundSelectorValues.Add("auto");
			BackgroundSelectorValues.Add("none");
			BackgroundSelectorValues.AddRange(LauncherBackground.AvailableImages());
			string b = LauncherBackground.ActiveSelection();
			int num = -1;
			_themeSelector.AddSeparator(MlmLocManager.Get("settings.background.caption"));
			for (int j = 0; j < BackgroundSelectorValues.Count; j++)
			{
				string text = BackgroundSelectorValues[j];
				_themeSelector.AddItem(BackgroundLabel(text), 1000 + j);
				if (string.Equals(text, b, StringComparison.OrdinalIgnoreCase))
				{
					num = _themeSelector.ItemCount - 1;
				}
			}
			_themeSelector.AddItem(MlmLocManager.Get("settings.background.openFolder"), 999);
			if (num >= 0)
			{
				_themeSelector.GetPopup().SetItemChecked(num, true);
			}
		}
		finally
		{
			_updatingThemeSelector = false;
		}
	}

	private static string BackgroundLabel(string value)
	{
		if (!(value == "auto"))
		{
			if (value == "none")
			{
				return MlmLocManager.Get("settings.background.none");
			}
			return value;
		}
		return MlmLocManager.Get("settings.background.auto");
	}

	private static void OnThemeSelected(long selectedIndex)
	{
		if (!_updatingThemeSelector && _themeSelector != null)
		{
			int num = checked((int)selectedIndex);
			int itemId = _themeSelector.GetItemId(num);
			if (itemId == 999)
			{
				PopulateThemeSelector();
				OpenBackgroundFolder();
				return;
			}
			if (itemId >= 1000)
			{
				SelectBackground(itemId - 1000);
				return;
			}
			string error;
			bool num2 = MlmLocManager.SetThemePreference((LauncherTheme)itemId, out error);
			ApplyPalette();
			ShowFeedback(num2 ? PlanResult.Ok(MlmLocManager.Get("settings.theme.changed")) : PlanResult.Error(MlmLocManager.Get("settings.theme.saveFailed", ("error", error))));
		}
	}

	private static void SelectBackground(int valueIndex)
	{
		if (valueIndex < 0 || valueIndex >= BackgroundSelectorValues.Count)
		{
			PopulateThemeSelector();
			return;
		}
		string text = BackgroundSelectorValues[valueIndex];
		string label = BackgroundLabel(text);
		string error;
		bool num = MlmLocManager.SetBackgroundPreference(text, out error);
		PopulateThemeSelector();
		string text2 = (num ? null : (error ?? string.Empty));
		if (text2 != null)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("settings.background.saveFailed", ("error", text2))));
		}
		ApplyBackground(BackgroundTrigger.Menu, label, text2);
	}

	private static void OpenBackgroundFolder()
	{
		string error;
		string text = LauncherBackground.EnsureImageFolder(out error);
		if (text != null && text.Length > 0)
		{
			OpenFolder(text, MlmLocManager.Get("folder.background"));
			return;
		}
		ShowFeedback(PlanResult.Error((error != null && error.Length > 0) ? MlmLocManager.Get("settings.background.folderCreateFailed", ("error", error)) : MlmLocManager.Get("settings.background.folderUnknown")));
	}

	private static Color BackdropVeil()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return _background?.Stats.ReadabilityVeil(UiKit.Palette) ?? UiKit.Canvas;
	}

	private static void UseBackdrop(LauncherBackground.Stats? stats)
	{
		_backdropStats = stats;
		UiKit.UseCustomBackdrop(stats);
	}

	private static void BeginBackground()
	{
		_background = null;
		_backgroundNode = null;
		_backdropVeil = null;
		_backgroundFade = null;
		_fadingBackgroundNode = null;
		_fadingBackground = null;
		string text = LauncherBackground.SelectedPath();
		UseBackdrop((text == null) ? ((LauncherBackground.Stats?)null) : LauncherBackground.CachedStats(text));
		if (text != null)
		{
			LauncherBackground.Load(text, delegate(LauncherBackground.Result? result)
			{
				OnBackgroundLoaded(result, BackgroundTrigger.Startup, null, null);
			});
		}
		else
		{
			LauncherBackground.CancelLoad();
		}
	}

	private static void ApplyBackground(BackgroundTrigger trigger, string? label = null, string? saveError = null)
	{
		if (_root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			return;
		}
		string text = LauncherBackground.SelectedPath();
		if (text == null || LauncherBackground.Shows(_background, text))
		{
			LauncherBackground.CancelLoad();
			if (text == null)
			{
				PresentBackground(null);
			}
			ReportBackground(trigger, label, saveError, shown: true);
			return;
		}
		if (trigger == BackgroundTrigger.Menu && saveError == null)
		{
			SetStatus(MlmLocManager.Get("settings.background.loading", ("name", label)), "MutedLabel", "…");
		}
		LauncherBackground.Load(text, delegate(LauncherBackground.Result? result)
		{
			OnBackgroundLoaded(result, trigger, label, saveError);
		});
	}

	private static void OnBackgroundLoaded(LauncherBackground.Result? result, BackgroundTrigger trigger, string? label, string? saveError)
	{
		if (_pumpActive || _root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			result?.Release();
			return;
		}
		if (result == null && _background == null && _backdropStats.HasValue)
		{
			UseBackdrop(null);
			ApplyPalette();
		}
		else if (result != null)
		{
			PresentBackground(result);
		}
		ReportBackground(trigger, label, saveError, result != null);
	}

	private static void ReportBackground(BackgroundTrigger trigger, string? label, string? saveError, bool shown)
	{
		switch (trigger)
		{
		case BackgroundTrigger.Menu:
			if (saveError != null)
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("settings.background.saveFailed", ("error", saveError))));
			}
			else if (shown)
			{
				ShowFeedback(PlanResult.Ok(string.Equals(MlmLocManager.BackgroundPreference, "none", StringComparison.OrdinalIgnoreCase) ? MlmLocManager.Get("settings.background.cleared") : MlmLocManager.Get("settings.background.changed", ("name", label))));
			}
			else
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get("settings.background.loadFailed", ("name", label))));
				LauncherBackground.RetryLater();
			}
			break;
		case BackgroundTrigger.Folder:
			if (shown)
			{
				SetStatus(MlmLocManager.Get("settings.background.reloaded"), "MutedLabel", "↻");
			}
			else
			{
				LauncherBackground.RetryLater();
			}
			break;
		}
	}

	private static void PresentBackground(LauncherBackground.Result? next)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		if (_root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			next?.Release();
			return;
		}
		FinishBackgroundFade();
		TextureRect val = ((_backgroundNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_backgroundNode)) ? _backgroundNode : null);
		LauncherBackground.Result background = _background;
		bool flag = _backdropVeil != null && GodotObject.IsInstanceValid((GodotObject)(object)_backdropVeil);
		Color veilFrom = (flag ? _backdropVeil.Color : BackdropVeil());
		bool num = !Nullable.Equals(_backdropStats, next?.Stats);
		_background = next;
		_backgroundNode = next?.Node;
		if (val != null)
		{
			((Node)val).Name = StringName.op_Implicit("OutgoingLauncherBackground");
		}
		if (_backgroundNode != null)
		{
			((Node)_root).AddChild((Node)(object)UiKit.FullRect<TextureRect>(_backgroundNode), false, (InternalMode)0);
			((Node)_root).MoveChild((Node)(object)_backgroundNode, (val != null) ? (((Node)val).GetIndex(false) + 1) : 0);
		}
		UseBackdrop(next?.Stats);
		if (num)
		{
			ApplyPalette();
		}
		else if (flag)
		{
			_backdropVeil.Color = BackdropVeil();
		}
		FadeBackground(val, background, veilFrom);
	}

	private static void FadeBackground(TextureRect? previous, LauncherBackground.Result? replaced, Color veilFrom)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		bool num = _backdropVeil != null && GodotObject.IsInstanceValid((GodotObject)(object)_backdropVeil);
		Color val = BackdropVeil();
		bool flag = previous != null && _backgroundNode != null;
		bool flag2 = num && veilFrom != val;
		if (_root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root) || (!flag && !flag2))
		{
			RetireBackground(previous, replaced);
			return;
		}
		_fadingBackgroundNode = previous;
		_fadingBackground = replaced;
		Tween val2 = ((Node)_root).CreateTween().SetPauseMode((TweenPauseMode)2).SetParallel(true);
		if (flag2)
		{
			_backdropVeil.Color = veilFrom;
			val2.TweenProperty((GodotObject)(object)_backdropVeil, NodePath.op_Implicit("color"), Variant.op_Implicit(val), 0.3);
		}
		if (flag)
		{
			((CanvasItem)_backgroundNode).Modulate = new Color(1f, 1f, 1f, 0f);
			val2.TweenProperty((GodotObject)(object)_backgroundNode, NodePath.op_Implicit("modulate:a"), Variant.op_Implicit(1f), 0.3);
		}
		val2.Finished += FinishBackgroundFade;
		_backgroundFade = val2;
	}

	private static void FinishBackgroundFade()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		Tween backgroundFade = _backgroundFade;
		_backgroundFade = null;
		if (backgroundFade != null && GodotObject.IsInstanceValid((GodotObject)(object)backgroundFade))
		{
			backgroundFade.Finished -= FinishBackgroundFade;
			backgroundFade.Kill();
		}
		if (_backgroundNode != null && GodotObject.IsInstanceValid((GodotObject)(object)_backgroundNode))
		{
			((CanvasItem)_backgroundNode).Modulate = Colors.White;
		}
		if (_backdropVeil != null && GodotObject.IsInstanceValid((GodotObject)(object)_backdropVeil))
		{
			_backdropVeil.Color = BackdropVeil();
		}
		RetireBackground(_fadingBackgroundNode, _fadingBackground);
		_fadingBackgroundNode = null;
		_fadingBackground = null;
	}

	private static void RetireBackground(TextureRect? node, LauncherBackground.Result? result)
	{
		if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node))
		{
			Node parent = ((Node)node).GetParent();
			if (parent != null)
			{
				parent.RemoveChild((Node)(object)node);
			}
			((Node)node).QueueFree();
		}
		if (result != null && (object)result != _background)
		{
			result.Release();
		}
	}

	private static void OnBackgroundFolderChanged()
	{
		if (_pumpActive || _root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			return;
		}
		if (_themeSelector != null && GodotObject.IsInstanceValid((GodotObject)(object)_themeSelector) && ((Window)_themeSelector.GetPopup()).Visible)
		{
			_backgroundChangePending = true;
			return;
		}
		PopulateThemeSelector();
		if (LauncherBackground.NeedsReload(_background))
		{
			ApplyBackground(BackgroundTrigger.Folder);
		}
	}

	private static void FlushPendingBackgroundChange()
	{
		if (_backgroundChangePending)
		{
			_backgroundChangePending = false;
			OnBackgroundFolderChanged();
		}
	}

	private static void ApplyPalette()
	{
		UiKit.UsePalette(UiPalette.For(MlmLocManager.EffectiveTheme));
		_background?.ApplyAppearance(UiKit.Palette);
		if (_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			_root.Theme = UiKit.CreateTheme();
		}
		ApplyPaletteBindings();
		PopulateThemeSelector();
		_optionsEditingKey = null;
		RebuildProfileRow();
		RebuildGroupRow();
		UpdateDetails();
		UpdatePlanSummary();
		UpdateWorkshopChrome();
		FriendModSyncDialog.Refresh();
		WorkshopCollectionImportDialog.Refresh();
		WorkshopDownloadsDialog.Refresh();
		ModListTransferDialog.Refresh();
		QueueRebuild();
	}

	private static void OpenSettings()
	{
		if (_settingsPage != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsPage))
		{
			_settingsStatus.Text = string.Empty;
			if (_settingsFontSlider != null)
			{
				((Range)_settingsFontSlider).SetValueNoSignal((double)(MlmLocManager.FontScale * 100f));
			}
			if (_settingsAffectsToggle != null)
			{
				((BaseButton)_settingsAffectsToggle).SetPressedNoSignal(_showAffectsColumn);
				UpdateAffectsToggleText();
			}
			if (_settingsAutoUnlockToggle != null)
			{
				((BaseButton)_settingsAutoUnlockToggle).SetPressedNoSignal(MlmLocManager.AutoUnlockAll);
				UpdateAutoUnlockToggleText();
			}
			if (_settingsCloudSyncToggle != null)
			{
				((BaseButton)_settingsCloudSyncToggle).SetPressedNoSignal(MlmLocManager.CloudSync);
				UpdateCloudSyncToggleText();
			}
			UpdateCloudSyncStatus();
			UpdateSettingsFontValue();
			Button? settingsRandomModToggle = _settingsRandomModToggle;
			if (settingsRandomModToggle != null)
			{
				((BaseButton)settingsRandomModToggle).SetPressedNoSignal(MlmLocManager.ShowRandomModButton);
			}
			Button? settingsVanillaLaunchToggle = _settingsVanillaLaunchToggle;
			if (settingsVanillaLaunchToggle != null)
			{
				((BaseButton)settingsVanillaLaunchToggle).SetPressedNoSignal(MlmLocManager.ShowVanillaLaunchButton);
			}
			UpdateVisibilityToggleText(_settingsRandomModToggle);
			UpdateVisibilityToggleText(_settingsVanillaLaunchToggle);
			if (_settingsModIsolationToggle != null)
			{
				((BaseButton)_settingsModIsolationToggle).SetPressedNoSignal(MlmLocManager.ModIsolation);
				UpdateModIsolationToggleText();
			}
			if (_settingsKeepDisabledToggle != null)
			{
				((BaseButton)_settingsKeepDisabledToggle).SetPressedNoSignal(MlmLocManager.KeepDisabledInPlace);
				UpdateKeepDisabledToggleText();
			}
			ShowSettingsSection(SettingsSection.Root);
			((CanvasItem)_settingsPage).Visible = true;
		}
	}

	private static void CloseSettings()
	{
		ClearAiInputs();
		if (_settingsPage != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsPage))
		{
			((CanvasItem)_settingsPage).Visible = false;
		}
		Button? settingsButton = _settingsButton;
		if (settingsButton != null && !((BaseButton)settingsButton).Disabled)
		{
			((Control)_settingsButton).GrabFocus();
		}
	}

	private static void ShowSettingsSection(SettingsSection section)
	{
		if (section != SettingsSection.Ai || !_pumpActive)
		{
			ActivateSettingsSection(section);
		}
	}

	private static void ActivateSettingsSection(SettingsSection section)
	{
		if (section != SettingsSection.Ai)
		{
			ClearAiInputs();
		}
		SettingsSection settingsSection = _settingsSection;
		_settingsSection = section;
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = string.Empty;
		}
		UpdateSettingsSectionChrome();
		if (_settingsScroll != null)
		{
			_settingsScroll.ScrollVertical = 0;
		}
		switch (section)
		{
		case SettingsSection.Customize:
		{
			Button? settingsRandomModToggle = _settingsRandomModToggle;
			if (settingsRandomModToggle != null)
			{
				((Control)settingsRandomModToggle).GrabFocus();
			}
			return;
		}
		case SettingsSection.Saves:
		{
			Button? settingsAutoUnlockToggle = _settingsAutoUnlockToggle;
			if (settingsAutoUnlockToggle != null)
			{
				((Control)settingsAutoUnlockToggle).GrabFocus();
			}
			return;
		}
		case SettingsSection.Mods:
		{
			Button? settingsModIsolationToggle = _settingsModIsolationToggle;
			if (settingsModIsolationToggle != null)
			{
				((Control)settingsModIsolationToggle).GrabFocus();
			}
			return;
		}
		case SettingsSection.Ai:
		{
			ShowAiSettings(AiTranslationSettings.LoadForLocale(MlmLocManager.EffectiveLocale));
			OptionButton? aiProvider = _aiProvider;
			if (aiProvider != null)
			{
				((Control)aiProvider).GrabFocus();
			}
			return;
		}
		}
		if (settingsSection != SettingsSection.Ai)
		{
			return;
		}
		Control? settingsPage = _settingsPage;
		if (settingsPage != null && ((CanvasItem)settingsPage).IsVisibleInTree())
		{
			Button? settingsAiEntry = _settingsAiEntry;
			if (settingsAiEntry != null)
			{
				((Control)settingsAiEntry).GrabFocus();
			}
		}
	}

	private static void UpdateSettingsSectionChrome()
	{
		if (_settingsTitle != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsTitle))
		{
			Label settingsTitle = _settingsTitle;
			settingsTitle.Text = MlmLocManager.Get(_settingsSection switch
			{
				SettingsSection.Customize => "settings.customize.title", 
				SettingsSection.Saves => "settings.saves.title", 
				SettingsSection.Mods => "settings.mods.title", 
				SettingsSection.Ai => "ai.settings.title", 
				_ => "settings.title", 
			});
			if (_settingsSubtitle != null)
			{
				settingsTitle = _settingsSubtitle;
				settingsTitle.Text = MlmLocManager.Get(_settingsSection switch
				{
					SettingsSection.Customize => "settings.customize.subtitle", 
					SettingsSection.Saves => "settings.saves.subtitle", 
					SettingsSection.Mods => "settings.mods.subtitle", 
					SettingsSection.Ai => "ai.settings.subtitle", 
					_ => "settings.subtitle", 
				});
			}
			if (_settingsRootView != null)
			{
				((CanvasItem)_settingsRootView).Visible = _settingsSection == SettingsSection.Root;
			}
			if (_settingsCustomizeView != null)
			{
				((CanvasItem)_settingsCustomizeView).Visible = _settingsSection == SettingsSection.Customize;
			}
			if (_settingsSavesView != null)
			{
				((CanvasItem)_settingsSavesView).Visible = _settingsSection == SettingsSection.Saves;
			}
			if (_settingsModsView != null)
			{
				((CanvasItem)_settingsModsView).Visible = _settingsSection == SettingsSection.Mods;
			}
			if (_settingsAiView != null)
			{
				((CanvasItem)_settingsAiView).Visible = _settingsSection == SettingsSection.Ai;
			}
			if (_settingsScroll != null)
			{
				((CanvasItem)_settingsScroll).Visible = _settingsSection != SettingsSection.Ai;
			}
			if (_settingsStatus != null)
			{
				((CanvasItem)_settingsStatus).Visible = _settingsSection != SettingsSection.Ai;
			}
			if (_settingsCard != null)
			{
				bool flag = _settingsSection == SettingsSection.Ai;
				((Control)_settingsCard).AnchorLeft = (flag ? 0.24f : 0.31f);
				((Control)_settingsCard).AnchorRight = (flag ? 0.76f : 0.69f);
				((Control)_settingsCard).AnchorTop = (flag ? 0.08f : 0.18f);
				((Control)_settingsCard).AnchorBottom = (flag ? 0.92f : 0.82f);
			}
			if (_settingsBack != null)
			{
				((CanvasItem)_settingsBack).Visible = _settingsSection != SettingsSection.Root;
			}
			if (_settingsFontReset != null)
			{
				((CanvasItem)_settingsFontReset).Visible = _settingsSection == SettingsSection.Customize;
			}
		}
	}

	private static void UpdateAutoUnlockToggleText()
	{
		if (_settingsAutoUnlockToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsAutoUnlockToggle))
		{
			_settingsAutoUnlockToggle.Text = MlmLocManager.Get(((BaseButton)_settingsAutoUnlockToggle).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		}
	}

	private static void OnAutoUnlockToggled(bool enabled)
	{
		string error;
		bool flag = MlmLocManager.SetAutoUnlockAll(enabled, out error);
		UpdateAutoUnlockToggleText();
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = MlmLocManager.Get((!flag) ? "settings.saves.autoUnlock.saveFailed" : (enabled ? "settings.saves.autoUnlock.enabled" : "settings.saves.autoUnlock.disabled"));
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(flag ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static void UpdateAffectsToggleText()
	{
		if (_settingsAffectsToggle != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsAffectsToggle))
		{
			_settingsAffectsToggle.Text = MlmLocManager.Get(((BaseButton)_settingsAffectsToggle).ButtonPressed ? "options.boolean.on" : "options.boolean.off");
		}
	}

	private static void OnAffectsColumnToggled(bool show)
	{
		_showAffectsColumn = show;
		UpdateAffectsToggleText();
		string error;
		bool flag = MlmLocManager.SetShowAffectsGameplayColumn(show, out error);
		ConfigureTreeColumns();
		QueueRebuild();
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = (flag ? MlmLocManager.Get("settings.columns.changed") : MlmLocManager.Get("settings.columns.saveFailed", ("error", error)));
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(flag ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static void OnFontScaleChanged(double percent)
	{
		string error;
		bool flag = MlmLocManager.SetFontScale((float)(percent / 100.0), out error);
		ApplyFontScale();
		UpdateSettingsFontValue();
		if (_settingsStatus != null)
		{
			_settingsStatus.Text = (flag ? MlmLocManager.Get("settings.fontSize.changed") : MlmLocManager.Get("settings.fontSize.saveFailed", ("error", error)));
			((Control)_settingsStatus).ThemeTypeVariation = StringName.op_Implicit(flag ? "SuccessLabel" : "DangerLabel");
		}
	}

	private static void UpdateSettingsFontValue()
	{
		if (_settingsFontValue != null && GodotObject.IsInstanceValid((GodotObject)(object)_settingsFontValue))
		{
			HSlider? settingsFontSlider = _settingsFontSlider;
			int num = Mathf.RoundToInt((float)((settingsFontSlider != null) ? ((Range)settingsFontSlider).Value : ((double)(MlmLocManager.FontScale * 100f))));
			_settingsFontValue.Text = MlmLocManager.Get("settings.fontSize.value", ("percent", num));
		}
	}

	private static void ApplyFontScale()
	{
		UiKit.UseFontScale(MlmLocManager.FontScale);
		if (_root != null && GodotObject.IsInstanceValid((GodotObject)(object)_root))
		{
			_root.Theme = UiKit.CreateTheme();
		}
		SyncControllerPromptTheme();
		ConfigureTreeColumns();
		if (_groupMenu != null && GodotObject.IsInstanceValid((GodotObject)(object)_groupMenu))
		{
			((Window)_groupMenu).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(14));
			((Window)_groupMenu).AddThemeFontSizeOverride(StringName.op_Implicit("font_separator_size"), UiKit.ScaleFontSize(12));
		}
		if (_loadingSpinner != null && GodotObject.IsInstanceValid((GodotObject)(object)_loadingSpinner))
		{
			((Control)_loadingSpinner).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(22));
		}
		if (_loadLog != null && GodotObject.IsInstanceValid((GodotObject)(object)_loadLog))
		{
			((Control)_loadLog).AddThemeFontSizeOverride(StringName.op_Implicit("normal_font_size"), UiKit.ScaleFontSize(13));
		}
		FriendModSyncDialog.Refresh();
		WorkshopCollectionImportDialog.Refresh();
		WorkshopDownloadsDialog.Refresh();
		ModListTransferDialog.Refresh();
		QueueRebuild();
	}

	private static void PopulateLanguageSelector()
	{
		if (_languageSelector == null || !GodotObject.IsInstanceValid((GodotObject)(object)_languageSelector))
		{
			return;
		}
		_updatingLanguageSelector = true;
		try
		{
			_languageSelector.Clear();
			LanguageSelectorValues.Clear();
			LanguageSelectorValues.Add("system");
			LanguageSelectorValues.AddRange(MlmLocManager.AvailableLocales.Select((MlmLocaleInfo locale) => locale.Id));
			for (int i = 0; i < LanguageSelectorValues.Count; i++)
			{
				string text = LanguageSelectorValues[i];
				_languageSelector.AddItem(MlmLocManager.PreferenceLabel(text), i);
				if (string.Equals(text, MlmLocManager.Preference, StringComparison.OrdinalIgnoreCase))
				{
					_languageSelector.Select(i);
				}
			}
		}
		finally
		{
			_updatingLanguageSelector = false;
		}
	}

	private static void PopulateSaveProfileSelector()
	{
		if (_saveProfileSelector == null || !GodotObject.IsInstanceValid((GodotObject)(object)_saveProfileSelector))
		{
			return;
		}
		_updatingSaveProfileSelector = true;
		try
		{
			_saveProfileSelector.Clear();
			for (int i = 1; i <= 3; i++)
			{
				_saveProfileSelector.AddItem(MlmLocManager.Get("saveProfile.option", ("profile", i)), i);
			}
			_saveProfileSelector.Select(_selectedSaveProfileId - 1);
			((BaseButton)_saveProfileSelector).Disabled = !SaveProfileSwitcher.Available || _pumpActive;
		}
		finally
		{
			_updatingSaveProfileSelector = false;
		}
	}

	private static void OnSaveProfileSelected(long selectedIndex)
	{
		if (!_updatingSaveProfileSelector && _saveProfileSelector != null && SaveProfileSwitcher.Available)
		{
			int num = checked((int)selectedIndex);
			int num2 = (_selectedSaveProfileId = _saveProfileSelector.GetItemId(num));
			ShowFeedback(MlmLocManager.SetSaveProfileId(num2, out string error) ? PlanResult.Ok(MlmLocManager.Get("saveProfile.selected", ("profile", num2))) : PlanResult.Error(MlmLocManager.Get("saveProfile.saveFailed", ("profile", num2), ("error", error))));
		}
	}

	private static void OnLanguageSelected(long selectedIndex)
	{
		if (_updatingLanguageSelector || _languageSelector == null)
		{
			return;
		}
		int num = checked((int)selectedIndex);
		if (num >= 0 && num < LanguageSelectorValues.Count)
		{
			string error;
			bool num2 = MlmLocManager.SetPreference(LanguageSelectorValues[num], out error);
			WorkshopUpdates.RefreshLocalizedTitles();
			ApplyLocalizationBindings();
			PopulateLanguageSelector();
			PopulateThemeSelector();
			PopulateSaveProfileSelector();
			if (_selected != null)
			{
				PopulateDetailGroupSelector(_selected.Group);
			}
			_optionsEditingKey = null;
			UpdateTreeColumnTitles();
			RebuildProfileRow();
			ShowGroupNameRow(visible: false);
			RebuildGroupRow();
			UpdateDetails();
			UpdatePlanSummary();
			UpdateWorkshopChrome();
			FriendModSyncDialog.Refresh();
			WorkshopCollectionImportDialog.Refresh();
			WorkshopDownloadsDialog.Refresh();
			ModListTransferDialog.Refresh();
			QueueRebuild();
			ShowFeedback(num2 ? PlanResult.Ok(MlmLocManager.Get("settings.language.changed")) : PlanResult.Error(MlmLocManager.Get("settings.language.saveFailed", ("error", error))));
		}
	}

	private static void StartTour()
	{
		StartTour(AllTourSteps(), whatsNew: false);
	}

	private static void StartWhatsNewTour(int afterRevision)
	{
		StartTour(NewTourSteps(afterRevision), whatsNew: true);
	}

	private static List<TourStep> NewTourSteps(int afterRevision)
	{
		return (from step in AllTourSteps()
			where step.Since > afterRevision && TourStepAvailable(step)
			select step).ToList();
	}

	private static bool TourStepAvailable(TourStep step)
	{
		Func<bool>? available = step.Available;
		if (available == null)
		{
			if (!(step.Scene != null))
			{
				return TourRect(step).HasValue;
			}
			return true;
		}
		return available();
	}

	private static void StartTour(List<TourStep> candidates, bool whatsNew)
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected O, but got Unknown
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Expected O, but got Unknown
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Expected O, but got Unknown
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		if (_plan == null || _pumpActive || TourOpen)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val == null)
		{
			return;
		}
		CloseFabMenu();
		ReturnFab(immediate: true);
		_tourSteps = candidates.Where(TourStepAvailable).ToList();
		if (_tourSteps.Count != 0)
		{
			_tourWhatsNew = whatsNew;
			_tourLayer = new CanvasLayer
			{
				Name = StringName.op_Implicit("UsageTour"),
				Layer = 1250
			};
			_tourRoot = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("UsageTourRoot"),
				MouseFilter = (MouseFilterEnum)0,
				Theme = UiKit.CreateTheme()
			});
			Color color = UiKit.Alpha(Colors.Black, UiKit.Palette.IsDark ? 0.62f : 0.5f);
			for (int i = 0; i < TourShade.Length; i++)
			{
				TourShade[i] = new ColorRect
				{
					Color = color,
					MouseFilter = (MouseFilterEnum)2
				};
				((Node)_tourRoot).AddChild((Node)(object)TourShade[i], false, (InternalMode)0);
			}
			_tourRing = new Panel
			{
				MouseFilter = (MouseFilterEnum)2
			};
			((Node)_tourRoot).AddChild((Node)(object)_tourRing, false, (InternalMode)0);
			_tourCallout = BuildTourCallout();
			((Node)_tourRoot).AddChild((Node)(object)_tourCallout, false, (InternalMode)0);
			((Node)_tourLayer).AddChild((Node)(object)_tourRoot, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_tourLayer, false, (InternalMode)0);
			_tourRoot.Resized += RefreshTourSpot;
			AddTourScope();
			_tourSpot = ((CanvasItem)_tourRoot).GetViewportRect();
			_tourIndex = 0;
			Callable val2 = Callable.From((Action)delegate
			{
				GoToTourStep(0);
			});
			((Callable)(ref val2)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private static void AddTourScope()
	{
		if (_tourRoot == null)
		{
			return;
		}
		LauncherController.Active?.AddScope(new LauncherController.Scope(_tourRoot, EndTour, () => (Control?)(object)_tourNext)
		{
			Shortcut = delegate(JoyButton button)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_0004: Invalid comparison between Unknown and I8
				//IL_0014: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Invalid comparison between Unknown and I8
				if ((long)button == 9)
				{
					GoToTourStep(_tourIndex - 1);
					return true;
				}
				if ((long)button == 10)
				{
					TourForward();
					return true;
				}
				return false;
			}
		});
	}

	private static PanelContainer BuildTourCallout()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Expected O, but got Unknown
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Expected O, but got Unknown
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Expected O, but got Unknown
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Expected O, but got Unknown
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Expected O, but got Unknown
		PanelContainer val = UiKit.Card("DialogPanel");
		((Node)val).Name = StringName.op_Implicit("UsageTourCallout");
		((Control)val).MouseFilter = (MouseFilterEnum)0;
		((Control)val).CustomMinimumSize = new Vector2(400f, 0f);
		VBoxContainer val2 = new VBoxContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		HBoxContainer val3 = new HBoxContainer();
		((Control)val3).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_tourProgress = UiKit.Label(string.Empty, "CaptionLabel");
		((Control)_tourProgress).SizeFlagsHorizontal = (SizeFlags)3;
		_tourProgress.VerticalAlignment = (VerticalAlignment)1;
		_tourNewBadge = UiKit.Badge(MlmLocManager.Get("tour.new"), BadgeTone.Positive);
		((Node)val3).AddChild((Node)(object)_tourProgress, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)_tourNewBadge, false, (InternalMode)0);
		_tourTitle = UiKit.Label(string.Empty, "SectionLabel");
		_tourBody = new RichTextLabel
		{
			BbcodeEnabled = true,
			FitContent = true,
			ScrollActive = false,
			AutowrapMode = (AutowrapMode)3,
			CustomMinimumSize = new Vector2(356f, 0f),
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)_tourBody).AddThemeFontOverride(StringName.op_Implicit("normal_font"), (Font)(object)UiKit.UiFont);
		((Control)_tourBody).AddThemeFontOverride(StringName.op_Implicit("bold_font"), (Font)(object)UiKit.BoldUiFont);
		((Control)_tourBody).AddThemeFontSizeOverride(StringName.op_Implicit("bold_font_size"), UiKit.ScaleFontSize(14));
		HBoxContainer val4 = new HBoxContainer();
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
		_tourSkip = UiKit.Button(string.Empty, "MiniButton", 0f, 34f);
		((BaseButton)_tourSkip).Pressed += EndTour;
		_tourBack = UiKit.Button(string.Empty, "SecondaryButton", 84f, 34f);
		((BaseButton)_tourBack).Pressed += delegate
		{
			GoToTourStep(_tourIndex - 1);
		};
		_tourNext = UiKit.Button(string.Empty, "PrimaryButton", 96f, 34f);
		((BaseButton)_tourNext).Pressed += TourForward;
		Button[] array = (Button[])(object)new Button[3] { _tourSkip, _tourBack, _tourNext };
		foreach (Button val5 in array)
		{
			Button captured = val5;
			((Control)val5).MouseDefaultCursorShape = (CursorShape)2;
			((Control)val5).GuiInput += (GuiInputEventHandler)delegate(InputEvent input)
			{
				if (input.IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
				{
					((Control)captured).AcceptEvent();
					EndTour();
				}
			};
		}
		((Node)val4).AddChild((Node)(object)_tourSkip, false, (InternalMode)0);
		((Node)val4).AddChild((Node)new Control
		{
			SizeFlagsHorizontal = (SizeFlags)3
		}, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_tourBack, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)_tourNext, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_tourTitle, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_tourBody, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)UiKit.Margin((Control)(object)val2, 4, 2), false, (InternalMode)0);
		return val;
	}

	private static void TourForward()
	{
		if (_tourIndex >= _tourSteps.Count - 1)
		{
			EndTour();
		}
		else
		{
			GoToTourStep(_tourIndex + 1);
		}
	}

	private static async void GoToTourStep(int index)
	{
		if (!TourOpen || _tourRoot == null || index < 0 || index >= _tourSteps.Count)
		{
			return;
		}
		int token = ++_tourToken;
		_tourIndex = index;
		TourStep step = _tourSteps[index];
		bool flag = !string.Equals(step.Scene?.Id, _tourScene?.Id, StringComparison.Ordinal);
		try
		{
			if (flag)
			{
				LeaveTourScene();
				if (step.Scene != null)
				{
					_tourScene = step.Scene;
					step.Scene.Enter();
					AddTourScope();
				}
			}
			step.Prepare?.Invoke();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} usage tip {step.Id} could not set up its screen: {value}");
		}
		if (flag || step.Prepare != null)
		{
			SceneTree tree = ((Node)_tourRoot).GetTree();
			for (int frame = 0; frame < 3; frame++)
			{
				await ((GodotObject)_tourRoot).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
				if (token != _tourToken || !TourOpen)
				{
					return;
				}
			}
		}
		FillTourCallout(step);
		Rect2 target = (Rect2)(((_003F?)TourRect(step)) ?? new Rect2(_tourRoot.Size / 2f, Vector2.Zero));
		Tween? tourTween = _tourTween;
		if (tourTween != null)
		{
			tourTween.Kill();
		}
		_tourTween = ((Node)_tourRoot).CreateTween();
		_tourTween.TweenMethod(Callable.From<Rect2>((Action<Rect2>)SetTourSpot), Variant.op_Implicit(_tourSpot), Variant.op_Implicit(target), 0.22).SetTrans((TransitionType)7).SetEase((EaseType)1);
		Callable val = Callable.From((Action)delegate
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			PlaceTourCallout(target);
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		Button? tourNext = _tourNext;
		if (tourNext != null)
		{
			((GodotObject)tourNext).CallDeferred(MethodName.GrabFocus, Array.Empty<Variant>());
		}
		UpdateControllerPrompts();
	}

	private static void FillTourCallout(TourStep step)
	{
		if (_tourProgress != null && _tourNewBadge != null && _tourTitle != null && _tourBody != null && _tourSkip != null && _tourBack != null && _tourNext != null)
		{
			bool flag = _tourIndex == _tourSteps.Count - 1;
			_tourProgress.Text = MlmLocManager.Get("tour.progress.chapter", ("chapter", MlmLocManager.Get(step.ChapterKey)), ("step", _tourIndex + 1), ("total", _tourSteps.Count));
			((CanvasItem)_tourNewBadge).Visible = !_tourWhatsNew && step.Since > MlmLocManager.AcknowledgedUpdateRevision;
			_tourTitle.Text = MlmLocManager.Get(step.TitleKey);
			_tourBody.Text = step.Body(TourOnController);
			_tourSkip.Text = MlmLocManager.Get("tour.skip");
			((CanvasItem)_tourSkip).Visible = !flag;
			_tourBack.Text = MlmLocManager.Get("tour.back");
			((CanvasItem)_tourBack).Visible = _tourIndex > 0;
			_tourNext.Text = MlmLocManager.Get(flag ? "tour.done" : "tour.next");
		}
	}

	private static void RefreshTourText()
	{
		if (TourOpen && _tourIndex >= 0 && _tourIndex < _tourSteps.Count)
		{
			FillTourCallout(_tourSteps[_tourIndex]);
			RefreshTourSpot();
		}
	}

	private static void RefreshTourSpot()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (TourOpen && _tourIndex >= 0 && _tourIndex < _tourSteps.Count)
		{
			Rect2 target = (Rect2)(((_003F?)TourRect(_tourSteps[_tourIndex])) ?? new Rect2(_tourRoot.Size / 2f, Vector2.Zero));
			Tween? tourTween = _tourTween;
			if (tourTween != null)
			{
				tourTween.Kill();
			}
			SetTourSpot(target);
			Callable val = Callable.From((Action)delegate
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				PlaceTourCallout(target);
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private static Rect2? TourRect(TourStep step)
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Rect2? val = null;
		foreach (Control item in step.Targets())
		{
			if (item == null || !GodotObject.IsInstanceValid((GodotObject)(object)item) || !((CanvasItem)item).IsVisibleInTree())
			{
				continue;
			}
			Rect2 globalRect = item.GetGlobalRect();
			if (!(((Rect2)(ref globalRect)).Size.X < 1f) && !(((Rect2)(ref globalRect)).Size.Y < 1f))
			{
				Rect2 value;
				if (val.HasValue)
				{
					Rect2 valueOrDefault = val.GetValueOrDefault();
					value = ((Rect2)(ref valueOrDefault)).Merge(globalRect);
				}
				else
				{
					value = globalRect;
				}
				val = value;
			}
		}
		if (!val.HasValue)
		{
			return null;
		}
		Rect2 valueOrDefault2 = val.GetValueOrDefault();
		return ((Rect2)(ref valueOrDefault2)).Grow(6f);
	}

	private static void SetTourSpot(Rect2 spot)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Expected O, but got Unknown
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		if (!TourOpen || _tourRoot == null || _tourRing == null)
		{
			return;
		}
		_tourSpot = spot;
		Vector2 size = _tourRoot.Size;
		Rect2[] array = (Rect2[])(object)new Rect2[4]
		{
			new Rect2(0f, 0f, size.X, ((Rect2)(ref spot)).Position.Y),
			new Rect2(0f, ((Rect2)(ref spot)).End.Y, size.X, size.Y - ((Rect2)(ref spot)).End.Y),
			new Rect2(0f, ((Rect2)(ref spot)).Position.Y, ((Rect2)(ref spot)).Position.X, ((Rect2)(ref spot)).Size.Y),
			new Rect2(((Rect2)(ref spot)).End.X, ((Rect2)(ref spot)).Position.Y, size.X - ((Rect2)(ref spot)).End.X, ((Rect2)(ref spot)).Size.Y)
		};
		for (int i = 0; i < TourShade.Length; i++)
		{
			ColorRect val = TourShade[i];
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((Control)val).Position = ((Rect2)(ref array[i])).Position;
				((Control)val).Size = new Vector2(Math.Max(0f, ((Rect2)(ref array[i])).Size.X), Math.Max(0f, ((Rect2)(ref array[i])).Size.Y));
			}
		}
		bool flag = ((Rect2)(ref spot)).Size.X <= 80f && ((Rect2)(ref spot)).Size.Y <= 80f;
		StyleBoxFlat val2 = new StyleBoxFlat
		{
			DrawCenter = false,
			BorderColor = UiKit.Accent,
			AntiAliasing = true
		};
		val2.SetBorderWidthAll(2);
		val2.SetCornerRadiusAll(flag ? ((int)(Math.Min(((Rect2)(ref spot)).Size.X, ((Rect2)(ref spot)).Size.Y) / 2f)) : 10);
		((Control)_tourRing).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)val2);
		((Control)_tourRing).Position = ((Rect2)(ref spot)).Position;
		((Control)_tourRing).Size = ((Rect2)(ref spot)).Size;
	}

	private static void PlaceTourCallout(Rect2 target)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		if (TourOpen && _tourRoot != null && _tourCallout != null)
		{
			Vector2 combinedMinimumSize = ((Control)_tourCallout).GetCombinedMinimumSize();
			((Control)_tourCallout).Size = combinedMinimumSize;
			Rect2 val = default(Rect2);
			((Rect2)(ref val))._002Ector(Vector2.Zero, _tourRoot.Size);
			float num = ((((Rect2)(ref target)).GetCenter().X > ((Rect2)(ref val)).GetCenter().X) ? (((Rect2)(ref target)).End.X - combinedMinimumSize.X) : ((Rect2)(ref target)).Position.X);
			Vector2 val2 = default(Vector2);
			if (((Rect2)(ref target)).End.Y + 14f + combinedMinimumSize.Y <= ((Rect2)(ref val)).End.Y - 16f)
			{
				((Vector2)(ref val2))._002Ector(num, ((Rect2)(ref target)).End.Y + 14f);
			}
			else if (((Rect2)(ref target)).Position.Y - 14f - combinedMinimumSize.Y >= 16f)
			{
				((Vector2)(ref val2))._002Ector(num, ((Rect2)(ref target)).Position.Y - 14f - combinedMinimumSize.Y);
			}
			else if (((Rect2)(ref target)).End.X + 14f + combinedMinimumSize.X <= ((Rect2)(ref val)).End.X - 16f)
			{
				((Vector2)(ref val2))._002Ector(((Rect2)(ref target)).End.X + 14f, ((Rect2)(ref target)).Position.Y);
			}
			else if (((Rect2)(ref target)).Position.X - 14f - combinedMinimumSize.X >= 16f)
			{
				((Vector2)(ref val2))._002Ector(((Rect2)(ref target)).Position.X - 14f - combinedMinimumSize.X, ((Rect2)(ref target)).Position.Y);
			}
			else
			{
				((Vector2)(ref val2))._002Ector(((Rect2)(ref val)).End.X - 16f - combinedMinimumSize.X, ((Rect2)(ref val)).End.Y - 16f - combinedMinimumSize.Y);
			}
			((Control)_tourCallout).Position = new Vector2(Math.Clamp(val2.X, 16f, Math.Max(16f, ((Rect2)(ref val)).End.X - 16f - combinedMinimumSize.X)), Math.Clamp(val2.Y, 16f, Math.Max(16f, ((Rect2)(ref val)).End.Y - 16f - combinedMinimumSize.Y)));
		}
	}

	private static void LeaveTourScene()
	{
		TourScene tourScene = _tourScene;
		_tourScene = null;
		try
		{
			tourScene?.Leave();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} usage tips could not put away {tourScene?.Id}: {value}");
		}
	}

	internal static void EndTour()
	{
		bool tourOpen = TourOpen;
		_tourToken++;
		Tween? tourTween = _tourTween;
		if (tourTween != null)
		{
			tourTween.Kill();
		}
		_tourTween = null;
		LeaveTourScene();
		if (_tourLayer != null && GodotObject.IsInstanceValid((GodotObject)(object)_tourLayer))
		{
			_tourLayer.Hide();
			((Node)_tourLayer).QueueFree();
		}
		_tourLayer = null;
		_tourRoot = null;
		_tourRing = null;
		_tourCallout = null;
		_tourProgress = null;
		_tourNewBadge = null;
		_tourTitle = null;
		_tourBody = null;
		_tourSkip = null;
		_tourBack = null;
		_tourNext = null;
		Array.Clear(TourShade);
		_tourSteps = new List<TourStep>();
		if (!tourOpen)
		{
			return;
		}
		UpdateControllerPrompts();
		if (_fab != null && GodotObject.IsInstanceValid((GodotObject)(object)_fab))
		{
			if (_controller != null)
			{
				_controller.FocusInCurrentScope((Control)(object)_fab);
			}
			else
			{
				((Control)_fab).GrabFocus();
			}
		}
	}

	private static string TourGlyph(PromptKey key)
	{
		PadGlyph padGlyph = PromptGlyph(key);
		if (padGlyph.Text.Length > 0)
		{
			return padGlyph.Text;
		}
		return padGlyph.Symbol switch
		{
			PadSymbol.Cross => "✕", 
			PadSymbol.Circle => "○", 
			PadSymbol.Square => "□", 
			PadSymbol.Triangle => "△", 
			PadSymbol.Menu => "MENU", 
			_ => key.ToString(), 
		};
	}

	private static (string Name, object? Value)[] TourGlyphArgs()
	{
		return new(string, object)[10]
		{
			("a", TourGlyph(PromptKey.A)),
			("b", TourGlyph(PromptKey.B)),
			("x", TourGlyph(PromptKey.X)),
			("y", TourGlyph(PromptKey.Y)),
			("lb", TourGlyph(PromptKey.LeftShoulder)),
			("rb", TourGlyph(PromptKey.RightShoulder)),
			("lt", TourGlyph(PromptKey.LeftTrigger)),
			("rt", TourGlyph(PromptKey.RightTrigger)),
			("start", TourGlyph(PromptKey.Start)),
			("view", TourGlyph(PromptKey.Back))
		};
	}

	private static List<TourStep> AllTourSteps()
	{
		TourScene scene = new TourScene("settings", OpenSettings, CloseSettings);
		TourScene scene2 = new TourScene("filters-preview", ShowTourFilters, RestoreTourFilters);
		TourScene scene3 = new TourScene("launch-preview", ShowTourButtons, RestoreTourButtons);
		TourScene scene4 = new TourScene("customize-preview", delegate
		{
			OpenSettings();
			ShowTourButtons();
		}, delegate
		{
			RestoreTourButtons();
			CloseSettings();
		});
		int num = 27;
		List<TourStep> list = new List<TourStep>(num);
		CollectionsMarshal.SetCount(list, num);
		Span<TourStep> span = CollectionsMarshal.AsSpan(list);
		span[0] = new TourStep("header.save", "tour.chapter.header", "tour.header.save.title", TourText("tour.header.save.body"), delegate
		{
			OptionButton? saveProfileSelector = _saveProfileSelector;
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((saveProfileSelector != null) ? ((Node)saveProfileSelector).GetParent<Control>() : null);
		});
		span[1] = new TourStep("header.appearance", "tour.chapter.header", "tour.header.appearance.title", TourText("tour.header.appearance.body"), delegate
		{
			Control[] array = new Control[2];
			OptionButton? languageSelector = _languageSelector;
			array[0] = ((languageSelector != null) ? ((Node)languageSelector).GetParent<Control>() : null);
			OptionButton? themeSelector = _themeSelector;
			array[1] = ((themeSelector != null) ? ((Node)themeSelector).GetParent<Control>() : null);
			return new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)array);
		});
		span[2] = new TourStep("header.status", "tour.chapter.header", "tour.header.status.title", TourText("tour.header.status.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_headerBadges));
		span[3] = new TourStep("header.changeNotes", "tour.chapter.header", "tour.header.changeNotes.title", TourText("tour.header.changeNotes.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_changeNotesBanner));
		span[4] = new TourStep("header.settings", "tour.chapter.header", "tour.header.settings.title", TourText("tour.header.settings.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_settingsButton));
		span[5] = new TourStep("profiles", "tour.chapter.profiles", "tour.profiles.title", TourText("tour.profiles.body"), () => ProfileChipsAndActions());
		span[6] = new TourStep("collection", "tour.chapter.profiles", "tour.collection.title", TourText("tour.collection.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_collectionImport), null, 11);
		span[7] = new TourStep("collection.source", "tour.chapter.profiles", "tour.collection.source.title", TourText("tour.collection.source.body"), () => WorkshopCollectionImportDialog.TourSource, CollectionSampleScene(), 11);
		span[8] = new TourStep("collection.items", "tour.chapter.profiles", "tour.collection.items.title", TourText("tour.collection.items.body"), () => WorkshopCollectionImportDialog.TourItems, CollectionSampleScene(), 11);
		span[9] = new TourStep("collection.actions", "tour.chapter.profiles", "tour.collection.actions.title", TourText("tour.collection.actions.body"), () => WorkshopCollectionImportDialog.TourActions, CollectionSampleScene(), 11);
		span[10] = new TourStep("friends", "tour.chapter.profiles", "tour.friends.title", TourText("tour.friends.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_friendSync));
		span[11] = new TourStep("friends.list", "tour.chapter.profiles", "tour.friends.list.title", TourText("tour.friends.list.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(FriendModSyncDialog.TourFriends), FriendSampleScene());
		span[12] = new TourStep("friends.compare", "tour.chapter.profiles", "tour.friends.compare.title", TourText("tour.friends.compare.body"), () => FriendModSyncDialog.TourComparison, FriendSampleScene());
		span[13] = new TourStep("friends.import", "tour.chapter.profiles", "tour.friends.import.title", TourText("tour.friends.import.body"), () => FriendModSyncDialog.TourImport, FriendSampleScene());
		span[14] = new TourStep("transfer", "tour.chapter.profiles", "tour.transfer.title", TourText("tour.transfer.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_transfer));
		span[15] = new TourStep("list.search", "tour.chapter.list", "tour.search.title", TourText("tour.search.body"), () => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[2]
		{
			(Control)_search,
			(Control)_resultCount
		}));
		span[16] = new TourStep("list.filters", "tour.chapter.list", "tour.filters.title", TourText("tour.filters.body", "tour.filters.body.controller"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_filterRow), scene2);
		span[17] = new TourStep("list.groups", "tour.chapter.list", "tour.groups.title", TourText("tour.groups.body", "tour.groups.body.controller"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_groupLine), scene2);
		span[18] = new TourStep("list.collapse", "tour.chapter.list", "tour.collapse.title", TourText("tour.collapse.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_filterToggle), scene2, 11);
		span[19] = new TourStep("list.tree", "tour.chapter.list", "tour.list.title", TourText("tour.list.body", "tour.list.body.controller", () => new(string, object)[1] { ("hint", MlmLocManager.Get(_groupListView ? "mods.list.hint.folders" : (LaunchPlan.KeepDisabledInPlace ? "mods.list.hint.inPlace" : "mods.list.hint"))) }), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_modTree));
		span[20] = new TourStep("details", "tour.chapter.details", "tour.details.title", TourText("tour.details.body", "tour.details.body.controller"), delegate
		{
			ScrollContainer? detailScroll = _detailScroll;
			return new _003C_003Ez__ReadOnlySingleElementList<Control>((detailScroll != null) ? ((Node)detailScroll).GetParent<Control>() : null);
		});
		span[21] = new TourStep("tools", "tour.chapter.tools", "tour.tools.title", TourText("tour.tools.body", "tour.tools.body.controller"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>((Control)(object)_fab), null, 12);
		span[22] = new TourStep("launch", "tour.chapter.launch", "tour.launch.title", TourText("tour.launch.body", "tour.launch.body.controller", delegate
		{
			(string, object)[] array2 = new(string, object)[1];
			Button? start = _start;
			array2[0] = ("launch", (((start != null) ? start.Text : null) ?? string.Empty).TrimEnd('→', ' '));
			return array2;
		}), () => new global::_003C_003Ez__ReadOnlyArray<Control>((Control[])(object)new Control[6]
		{
			(Control)_restartGame,
			(Control)_randomMod,
			(Control)_autoSort,
			(Control)_reset,
			(Control)_vanillaStart,
			(Control)_start
		}), scene3);
		span[23] = new TourStep("settings.customize", "tour.chapter.settings", "tour.settings.customize.title", TourText("tour.settings.customize.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsCustomizeView), scene4, 0, null, delegate
		{
			ShowSettingsSection(SettingsSection.Customize);
		});
		span[24] = new TourStep("settings.saves", "tour.chapter.settings", "tour.settings.saves.title", TourText("tour.settings.saves.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsSavesView), scene, 0, null, delegate
		{
			ShowSettingsSection(SettingsSection.Saves);
		});
		span[25] = new TourStep("settings.mods", "tour.chapter.settings", "tour.settings.mods.title", TourText("tour.settings.mods.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsModsView), scene, 0, null, delegate
		{
			ShowSettingsSection(SettingsSection.Mods);
		});
		span[26] = new TourStep("settings.ai", "tour.chapter.settings", "tour.settings.ai.title", TourText("tour.settings.ai.body"), () => new _003C_003Ez__ReadOnlySingleElementList<Control>(_settingsAiView), scene, 0, null, delegate
		{
			ShowSettingsSection(SettingsSection.Ai);
		});
		return list;
	}

	private static void ShowTourFilters()
	{
		bool valueOrDefault = _tourFiltersCollapsed == true;
		if (!_tourFiltersCollapsed.HasValue)
		{
			valueOrDefault = _filtersCollapsed;
			_tourFiltersCollapsed = valueOrDefault;
		}
		_filtersCollapsed = false;
		ApplyFilterCollapse();
	}

	private static void RestoreTourFilters()
	{
		bool? tourFiltersCollapsed = _tourFiltersCollapsed;
		if (tourFiltersCollapsed.HasValue)
		{
			bool valueOrDefault = tourFiltersCollapsed == true;
			_tourFiltersCollapsed = null;
			_filtersCollapsed = valueOrDefault;
			ApplyFilterCollapse();
		}
	}

	private static void ShowTourButtons()
	{
		if ((object)_tourButtonState == null)
		{
			Button? randomMod = _randomMod;
			bool randomVisible = randomMod != null && ((CanvasItem)randomMod).Visible;
			Button? vanillaStart = _vanillaStart;
			bool vanillaVisible = vanillaStart != null && ((CanvasItem)vanillaStart).Visible;
			Button? settingsRandomModToggle = _settingsRandomModToggle;
			bool randomSwitch = settingsRandomModToggle != null && ((BaseButton)settingsRandomModToggle).ButtonPressed;
			Button? settingsVanillaLaunchToggle = _settingsVanillaLaunchToggle;
			_tourButtonState = new TourButtonState(randomVisible, vanillaVisible, randomSwitch, settingsVanillaLaunchToggle != null && ((BaseButton)settingsVanillaLaunchToggle).ButtonPressed);
		}
		if (_randomMod != null)
		{
			((CanvasItem)_randomMod).Visible = true;
		}
		if (_vanillaStart != null)
		{
			((CanvasItem)_vanillaStart).Visible = true;
		}
		Button? settingsRandomModToggle2 = _settingsRandomModToggle;
		if (settingsRandomModToggle2 != null)
		{
			((BaseButton)settingsRandomModToggle2).SetPressedNoSignal(true);
		}
		Button? settingsVanillaLaunchToggle2 = _settingsVanillaLaunchToggle;
		if (settingsVanillaLaunchToggle2 != null)
		{
			((BaseButton)settingsVanillaLaunchToggle2).SetPressedNoSignal(true);
		}
		UpdateVisibilityToggleText(_settingsRandomModToggle);
		UpdateVisibilityToggleText(_settingsVanillaLaunchToggle);
	}

	private static void RestoreTourButtons()
	{
		TourButtonState tourButtonState = _tourButtonState;
		if ((object)tourButtonState != null)
		{
			_tourButtonState = null;
			if (_randomMod != null)
			{
				((CanvasItem)_randomMod).Visible = tourButtonState.RandomVisible;
			}
			if (_vanillaStart != null)
			{
				((CanvasItem)_vanillaStart).Visible = tourButtonState.VanillaVisible;
			}
			Button? settingsRandomModToggle = _settingsRandomModToggle;
			if (settingsRandomModToggle != null)
			{
				((BaseButton)settingsRandomModToggle).SetPressedNoSignal(tourButtonState.RandomSwitch);
			}
			Button? settingsVanillaLaunchToggle = _settingsVanillaLaunchToggle;
			if (settingsVanillaLaunchToggle != null)
			{
				((BaseButton)settingsVanillaLaunchToggle).SetPressedNoSignal(tourButtonState.VanillaSwitch);
			}
			UpdateVisibilityToggleText(_settingsRandomModToggle);
			UpdateVisibilityToggleText(_settingsVanillaLaunchToggle);
		}
	}

	private static Func<bool, string> TourText(string key, string? controllerKey = null, Func<(string Name, object? Value)[]>? args = null)
	{
		return delegate(bool controller)
		{
			if (!controller || controllerKey == null)
			{
				return MlmLocManager.Get(key, args?.Invoke() ?? Array.Empty<(string, object)>());
			}
			string? key2 = controllerKey;
			(string, object)[] array = TourGlyphArgs();
			(string, object)[] array2 = args?.Invoke() ?? Array.Empty<(string, object)>();
			int num = 0;
			(string, object)[] array3 = new(string, object)[array.Length + array2.Length];
			ReadOnlySpan<(string, object)> readOnlySpan = new ReadOnlySpan<(string, object)>(array);
			readOnlySpan.CopyTo(new Span<(string, object)>(array3).Slice(num, readOnlySpan.Length));
			num += readOnlySpan.Length;
			ReadOnlySpan<(string, object)> readOnlySpan2 = new ReadOnlySpan<(string, object)>(array2);
			readOnlySpan2.CopyTo(new Span<(string, object)>(array3).Slice(num, readOnlySpan2.Length));
			num += readOnlySpan2.Length;
			return MlmLocManager.Get(key2, array3);
		};
	}

	[IteratorStateMachine(typeof(_003CProfileChipsAndActions_003Ed__824))]
	private static IEnumerable<Control?> ProfileChipsAndActions()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CProfileChipsAndActions_003Ed__824(-2);
	}

	private static TourScene CollectionSampleScene()
	{
		return new TourScene("collection", ShowCollectionSample, WorkshopCollectionImportDialog.Close);
	}

	private static void ShowCollectionSample()
	{
		if (_plan == null)
		{
			return;
		}
		WorkshopCollectionImportDialog.Show(_plan, CollectionImportHost());
		List<CollectionMember> list = new List<CollectionMember>();
		foreach (IGrouping<ulong, LaunchEntry> item in (from entry in _plan.Entries
			where !entry.IsManager
			group entry by PhysicalModIdentity.WorkshopItemIdFor(entry.Mod) into @group
			where @group.Key != 0
			select @group).Take(3))
		{
			list.Add(new CollectionMember(item.Key, item.First().DisplayName, WorkshopItemKind.Item, CollectionMemberRole.Listed, null, 0uL));
		}
		string via = list.FirstOrDefault()?.Title ?? MlmLocManager.Get("tour.sample.missingMod");
		ulong viaId = list.FirstOrDefault()?.ItemId ?? SampleItem(1);
		list.Add(new CollectionMember(SampleItem(1), MlmLocManager.Get("tour.sample.missingMod"), WorkshopItemKind.Item, CollectionMemberRole.Listed, MlmLocManager.Get("tour.sample.subCollection"), SampleItem(9)));
		list.Insert(0, new CollectionMember(SampleItem(2), MlmLocManager.Get("tour.sample.requiredMod"), WorkshopItemKind.Item, CollectionMemberRole.Required, via, viaId));
		list.Add(new CollectionMember(SampleItem(3), string.Empty, WorkshopItemKind.Unavailable, CollectionMemberRole.Listed, null, 0uL));
		WorkshopCollectionImportDialog.PreviewMine(new global::_003C_003Ez__ReadOnlyArray<CollectionChoice>(new CollectionChoice[2]
		{
			new CollectionChoice(SampleItem(0), MlmLocManager.Get("tour.sample.collection"), (uint)list.Count, Favorited: false),
			new CollectionChoice(SampleItem(10), MlmLocManager.Get("tour.sample.favoriteCollection"), 12u, Favorited: true)
		}));
		WorkshopCollectionImportDialog.Preview(new CollectionSnapshot(SampleItem(0), MlmLocManager.Get("tour.sample.collection"), list));
	}

	private static ulong SampleItem(int index)
	{
		return (ulong)(9000000000L + index);
	}

	private static TourScene FriendSampleScene()
	{
		return new TourScene("friends", ShowFriendSample, delegate
		{
			FriendModSyncDialog.Close();
			FriendModSync.EndSample();
		});
	}

	private static void ShowFriendSample()
	{
		if (_plan != null)
		{
			FriendModSync.BeginSample(SampleFriends(_plan));
			FriendModSyncDialog.Show(_plan, ApplyFriendProfile);
		}
	}

	private static List<OnlineFriendMods> SampleFriends(LaunchPlan plan)
	{
		List<LaunchEntry> source = plan.Entries.Where(delegate(LaunchEntry entry)
		{
			if (!entry.IsManager)
			{
				string text = entry.Mod.manifest?.id;
				if (text != null)
				{
					return text.Length > 0;
				}
				return false;
			}
			return false;
		}).ToList();
		List<LaunchEntry> list = source.Where((LaunchEntry entry) => entry.Enabled).ToList();
		FriendModList list2 = new FriendModList
		{
			GameVersion = CurrentGameVersion().TrimStart('v'),
			ManagerVersion = (plan.Entries.FirstOrDefault((LaunchEntry entry) => entry.IsManager)?.Mod.manifest?.version ?? string.Empty)
		};
		foreach (LaunchEntry item in list.Take(4))
		{
			Add(item);
		}
		if (list.Count > 4)
		{
			Add(list[4], "9.9.9");
		}
		LaunchEntry launchEntry = source.FirstOrDefault((LaunchEntry entry) => !entry.Enabled);
		if (launchEntry != null)
		{
			Add(launchEntry);
		}
		list2.Mods.Add(new FriendModEntry
		{
			Id = "SampleFriendMod",
			Name = MlmLocManager.Get("tour.sample.friendMod"),
			Version = "1.0.0",
			WorkshopId = SampleItem(5),
			Source = "w",
			ContentHash = "sample"
		});
		int num = 2;
		List<OnlineFriendMods> list3 = new List<OnlineFriendMods>(num);
		CollectionsMarshal.SetCount(list3, num);
		Span<OnlineFriendMods> span = CollectionsMarshal.AsSpan(list3);
		span[0] = new OnlineFriendMods(SampleItem(7), MlmLocManager.Get("tour.sample.friend"), FriendPresenceState.Ready, list2, string.Empty, null);
		span[1] = new OnlineFriendMods(SampleItem(8), MlmLocManager.Get("tour.sample.quietFriend"), FriendPresenceState.NotSharing, null, string.Empty, null);
		return list3;
		void Add(LaunchEntry entry, string? version = null)
		{
			list2.Mods.Add(new FriendModEntry
			{
				Id = entry.Id,
				Name = entry.DisplayName,
				Version = (version ?? entry.Mod.manifest?.version ?? string.Empty),
				WorkshopId = PhysicalModIdentity.WorkshopItemIdFor(entry.Mod),
				Source = ((PhysicalModIdentity.WorkshopItemIdFor(entry.Mod) != 0) ? "w" : "l"),
				ContentHash = "sample:same-files"
			});
		}
	}

	private static Control BuildTranslations()
	{
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Expected O, but got Unknown
		_detailTranslations = DetailSection("translations.caption", visible: false);
		_translationExport = LocalizedKeyButton("translations.export", "SecondaryButton", 0f, 34f);
		((BaseButton)_translationExport).Pressed += ExportSelectedTranslations;
		((Node)_detailTranslations).AddChild((Node)(object)_translationExport, false, (InternalMode)0);
		((Node)_detailTranslations).AddChild((Node)(object)LocalizedKeyLabel("translations.language", "DimLabel"), false, (InternalMode)0);
		_translationLanguage = UiKit.Dropdown();
		((Control)_translationLanguage).SizeFlagsHorizontal = (SizeFlags)3;
		(string, string)[] languages = ModTranslationFiles.Languages;
		for (int i = 0; i < languages.Length; i++)
		{
			var (text, text2) = languages[i];
			_translationLanguage.AddItem(text2 + " · " + text, -1);
		}
		SaveManager instance = SaveManager.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			SettingsSave settingsSave = instance.SettingsSave;
			obj = ((settingsSave != null) ? settingsSave.Language : null);
		}
		if (obj == null)
		{
			obj = "eng";
		}
		string language = (string)obj;
		_translationLanguage.Select(Math.Max(0, Array.FindIndex(ModTranslationFiles.Languages, ((string Code, string Name) l) => l.Code == language)));
		((Node)_detailTranslations).AddChild((Node)(object)_translationLanguage, false, (InternalMode)0);
		_translationAi = LocalizedKeyButton("ai.translate", "SecondaryButton", 0f, 34f);
		((BaseButton)_translationAi).Pressed += TranslateSelectedMod;
		((Node)_detailTranslations).AddChild((Node)(object)_translationAi, false, (InternalMode)0);
		Label aiHint = UiKit.Label(string.Empty, "DimLabel");
		aiHint.AutowrapMode = (AutowrapMode)3;
		BindLocalization((Control)(object)aiHint, delegate
		{
			aiHint.Text = MlmLocManager.Get(SelectedTranslatesTitleOnly ? "ai.translate.hintTitleOnly" : "ai.translate.hint");
		});
		_translationAiHint = aiHint;
		((Node)_detailTranslations).AddChild((Node)(object)aiHint, false, (InternalMode)0);
		_translationOriginal = UiKit.Button(string.Empty, "SecondaryButton", 0f, 34f);
		((BaseButton)_translationOriginal).Pressed += ToggleSelectedTranslation;
		((Node)_detailTranslations).AddChild((Node)(object)_translationOriginal, false, (InternalMode)0);
		_translationCancel = LocalizedKeyButton("ai.cancel", "SecondaryButton", 0f, 34f);
		((BaseButton)_translationCancel).Pressed += delegate
		{
			_aiCancellation?.Cancel();
		};
		((Node)_detailTranslations).AddChild((Node)(object)_translationCancel, false, (InternalMode)0);
		_aiProgress = UiKit.Label(string.Empty, "DimLabel");
		_aiProgress.AutowrapMode = (AutowrapMode)3;
		((Node)_detailTranslations).AddChild((Node)(object)_aiProgress, false, (InternalMode)0);
		HFlowContainer val = new HFlowContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		_translationImport = LocalizedKeyButton("translations.add", "SecondaryButton", 0f, 34f);
		((BaseButton)_translationImport).Pressed += ImportSelectedTranslation;
		_translationOpen = LocalizedKeyButton("translations.open", "SecondaryButton", 0f, 34f);
		((BaseButton)_translationOpen).Pressed += OpenSelectedTranslations;
		((Node)val).AddChild((Node)(object)_translationImport, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)_translationOpen, false, (InternalMode)0);
		_translationFileActions = (Control?)(object)val;
		((Node)_detailTranslations).AddChild((Node)(object)val, false, (InternalMode)0);
		_translationHint = UiKit.Label(string.Empty, "DimLabel");
		_translationHint.AutowrapMode = (AutowrapMode)3;
		((Node)_detailTranslations).AddChild((Node)(object)_translationHint, false, (InternalMode)0);
		return (Control)(object)_detailTranslations;
	}

	private static void UpdateTranslationControls(bool enabled = true)
	{
		if (_detailTranslations == null)
		{
			return;
		}
		Mod val = _selected?.Mod;
		VBoxContainer? detailTranslations = _detailTranslations;
		int visible;
		if (val?.manifest != null)
		{
			LaunchEntry? selected = _selected;
			visible = ((selected != null && !selected.IsManager) ? 1 : 0);
		}
		else
		{
			visible = 0;
		}
		((CanvasItem)detailTranslations).Visible = (byte)visible != 0;
		bool flag = _aiCancellation != null;
		bool flag2 = enabled && !_pumpActive && !flag && val != null && ModTranslations.CanEdit(val);
		if (_translationLanguage != null)
		{
			((BaseButton)_translationLanguage).Disabled = !enabled || _pumpActive || flag;
		}
		if (_translationExport != null)
		{
			((BaseButton)_translationExport).Disabled = !enabled || _pumpActive;
		}
		if (_translationExport != null)
		{
			((CanvasItem)_translationExport).Visible = val != null && val.manifest?.hasPck == true;
		}
		if (_translationImport != null)
		{
			((BaseButton)_translationImport).Disabled = !flag2;
		}
		if (_translationOpen != null)
		{
			((BaseButton)_translationOpen).Disabled = !flag2;
		}
		if (_translationAi != null)
		{
			((BaseButton)_translationAi).Disabled = !enabled || flag || _pumpActive;
		}
		if (_translationCancel != null)
		{
			((CanvasItem)_translationCancel).Visible = flag;
		}
		if (_aiProgress != null)
		{
			((CanvasItem)_aiProgress).Visible = flag;
		}
		if (_translationOriginal != null)
		{
			((BaseButton)_translationOriginal).Disabled = !enabled || _pumpActive || flag || val == null;
			bool flag3 = true;
			try
			{
				if (val != null)
				{
					flag3 = AiTranslationDocument.Enabled(ModTranslations.Folder(val));
				}
			}
			catch
			{
				((BaseButton)_translationOriginal).Disabled = true;
			}
			_translationOriginal.Text = MlmLocManager.Get(flag3 ? "ai.original.use" : "ai.original.translate");
		}
		bool selectedTranslatesTitleOnly = SelectedTranslatesTitleOnly;
		if (_translationAiHint != null)
		{
			_translationAiHint.Text = MlmLocManager.Get(selectedTranslatesTitleOnly ? "ai.translate.hintTitleOnly" : "ai.translate.hint");
		}
		if (_translationFileActions != null)
		{
			((CanvasItem)_translationFileActions).Visible = !selectedTranslatesTitleOnly;
		}
		if (_translationHint != null)
		{
			((CanvasItem)_translationHint).Visible = !selectedTranslatesTitleOnly;
		}
		if (_translationHint != null)
		{
			_translationHint.Text = MlmLocManager.Get((!ModTranslations.Available) ? "translations.unavailable" : ((val != null && !ModTranslations.CanEdit(val)) ? "translations.loaded" : "translations.hint"));
		}
	}

	private static void ExportSelectedTranslations()
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (_pumpActive)
		{
			return;
		}
		Mod val = _selected?.Mod;
		if (val == null)
		{
			return;
		}
		try
		{
			Dictionary<string, byte[]> dictionary = PckLocalizationArchive.Read(ModTranslations.PackPath(val), ModTranslations.Id(val));
			string text = ModTranslationFiles.Export(ModTranslations.Folder(val), dictionary);
			if ((int)OS.ShellShowInFileManager(text, true) != 0)
			{
				throw new IOException("Could not open the export folder.");
			}
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("translations.exported", ("count", dictionary.Count), ("path", text))));
		}
		catch (Exception exception)
		{
			ReportTranslationError(exception);
		}
	}

	private static void ImportSelectedTranslation()
	{
		if (_pumpActive)
		{
			return;
		}
		Mod mod = _selected?.Mod;
		if (mod != null && ModTranslations.CanEdit(mod))
		{
			string language = TranslationLanguage;
			if (!NativeFileDialogs.TryShow(save: false, MlmLocManager.Get("translations.add"), Environment.GetFolderPath(Environment.SpecialFolder.Personal), string.Empty, delegate(string path)
			{
				if (_pumpActive || _root == null || !GodotObject.IsInstanceValid((GodotObject)(object)_root) || !ModTranslations.CanEdit(mod))
				{
					return;
				}
				try
				{
					Dictionary<string, byte[]> original = PckLocalizationArchive.Read(ModTranslations.PackPath(mod), ModTranslations.Id(mod));
					ModTranslationFiles.Import(ModTranslations.Folder(mod), language, path, original, ModTranslations.ValidateFormat);
					ShowFeedback(PlanResult.Ok(MlmLocManager.Get("translations.added", ("file", Path.GetFileName(path)), ("language", language))));
				}
				catch (Exception exception)
				{
					ReportTranslationError(exception);
				}
			}, out string _))
			{
				OpenSelectedTranslations();
			}
		}
	}

	private static void OpenSelectedTranslations()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (_pumpActive)
		{
			return;
		}
		Mod val = _selected?.Mod;
		if (val == null || !ModTranslations.CanEdit(val))
		{
			return;
		}
		try
		{
			string text = ModTranslations.Folder(val);
			string text2 = ModTranslationFiles.LanguageFolder(text, TranslationLanguage);
			ModTranslationFiles.RejectLinks(text, text2);
			Directory.CreateDirectory(text2);
			if ((int)OS.ShellShowInFileManager(text2, true) != 0)
			{
				throw new IOException("Could not open the translation folder.");
			}
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("translations.opened", ("path", text2))));
		}
		catch (Exception exception)
		{
			ReportTranslationError(exception);
		}
	}

	private static void ReportTranslationError(Exception exception)
	{
		GD.PushWarning($"{"[ModLaunchManager]"} translation file operation failed: {exception}");
		string text;
		if (exception is TranslationFileException ex)
		{
			text = ex.Reason;
		}
		else
		{
			string text2;
			if (exception is DecoderFallbackException)
			{
				text2 = "invalidJson";
			}
			else
			{
				bool flag = ((exception is EndOfStreamException || exception is OverflowException) ? true : false);
				text2 = (flag ? "invalidPack" : "io");
			}
			text = text2;
		}
		string text3 = text;
		ShowFeedback(PlanResult.Error(MlmLocManager.Get("translations.error." + text3)));
	}

	private static Button BuildUndoShortcut()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Expected O, but got Unknown
		_undoShortcut = new Button
		{
			Flat = true,
			FocusMode = (FocusModeEnum)0,
			MouseFilter = (MouseFilterEnum)2,
			ShortcutFeedback = false,
			SelfModulate = new Color(1f, 1f, 1f, 0f),
			Shortcut = UiKit.Keys((Key)90, ctrl: true)
		};
		((BaseButton)_undoShortcut).Pressed += UndoLastStep;
		return _undoShortcut;
	}

	private static void RecordUndo(string tag = "")
	{
		if (_plan == null || _pumpActive)
		{
			return;
		}
		ulong processFrames = Engine.GetProcessFrames();
		ulong ticksMsec = Time.GetTicksMsec();
		if (UndoSteps.Count > 0)
		{
			List<UndoStep> undoSteps = UndoSteps;
			UndoStep undoStep = undoSteps[undoSteps.Count - 1];
			bool num = undoStep.Frame == processFrames;
			bool flag = tag.Length > 0 && undoStep.Tag == tag && ticksMsec - undoStep.At < 1500;
			if (num || flag)
			{
				List<UndoStep> undoSteps2 = UndoSteps;
				undoSteps2[undoSteps2.Count - 1] = undoStep with
				{
					At = ticksMsec,
					Frame = processFrames
				};
				return;
			}
		}
		UndoSteps.Add(new UndoStep(new List<LaunchEntry>(_plan.Entries), _plan.Entries.ToDictionary((LaunchEntry entry) => entry, (LaunchEntry entry) => entry.Enabled), _organization.Clone(), tag, processFrames, ticksMsec));
		if (UndoSteps.Count > 50)
		{
			UndoSteps.RemoveAt(0);
		}
	}

	private static bool OrganizationEquals(ModOrganizationDocument left, ModOrganizationDocument right)
	{
		return JsonSerializer.Serialize(left) == JsonSerializer.Serialize(right);
	}

	private static bool MatchesNow(UndoStep step)
	{
		if (_plan != null && step.Order.SequenceEqual(_plan.Entries) && _plan.Entries.All((LaunchEntry entry) => !step.Enabled.TryGetValue(entry, out var value) || value == entry.Enabled))
		{
			return OrganizationEquals(step.Organization, _organization);
		}
		return false;
	}

	private static void UndoLastStep()
	{
		if (UpdateAnnouncement.IsOpen || _plan == null || _pumpActive)
		{
			return;
		}
		while (UndoSteps.Count > 0)
		{
			List<UndoStep> undoSteps = UndoSteps;
			UndoStep undoStep = undoSteps[undoSteps.Count - 1];
			UndoSteps.RemoveAt(UndoSteps.Count - 1);
			if (MatchesNow(undoStep))
			{
				continue;
			}
			EndControllerMove();
			FolderToggleMemory.Clear();
			_plan.RestoreState(undoStep.Order, undoStep.Enabled);
			if (!OrganizationEquals(undoStep.Organization, _organization))
			{
				ModOrganizationDocument organization = _organization;
				_organization = undoStep.Organization.Clone();
				PlanResult result = ModOrganizationStore.Save(_organization);
				if (!result.Success)
				{
					_organization = organization;
					ShowFeedback(result);
				}
				ApplyOrganization(_plan.Entries);
				if (_selected != null)
				{
					PopulateDetailGroupSelector(_selected.Group);
				}
				string text = ActiveGroupName();
				if (text != null && !KnownGroupNames().Contains<string>(text, StringComparer.OrdinalIgnoreCase))
				{
					_groupFilter = null;
				}
				Defer(RebuildGroupRow);
			}
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("plan.undo.success")));
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild(scrollToSelection: true);
			return;
		}
		SetStatus(MlmLocManager.Get("plan.undo.nothing"), "MutedLabel", "·");
	}

	private static void StartWorkshopUpdateCheck(Control root)
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		if (_plan != null && WorkshopUpdates.IsAvailable)
		{
			_workshopSummaryShown = false;
			_workshopRetryWasNeeded = false;
			StartChangeNotes();
			WorkshopUpdates.Prepare(_plan.Entries.Select((LaunchEntry entry) => entry.Mod), OnWorkshopChanged);
			if (WorkshopUpdates.HasPendingScan && _sceneTree != null)
			{
				_sceneTree.ProcessFrame += PumpWorkshopScan;
				_workshopScanSubscribed = true;
			}
			_workshopTimer = new Timer
			{
				WaitTime = 0.5,
				Autostart = true
			};
			_workshopTimer.Timeout += delegate
			{
				WorkshopBranches.Poll(0.5);
				WorkshopUpdates.Poll(0.5);
				WorkshopDownloadCenter.Poll(0.5);
				WorkshopSubscription.Poll(0.5);
			};
			((Node)root).AddChild((Node)(object)_workshopTimer, false, (InternalMode)0);
		}
	}

	private static void PumpWorkshopScan()
	{
		if (WorkshopUpdates.ScanChunk(6) && _sceneTree != null && _workshopScanSubscribed)
		{
			_sceneTree.ProcessFrame -= PumpWorkshopScan;
			_workshopScanSubscribed = false;
		}
	}

	private static void OnWorkshopChanged()
	{
		if (_layer == null || !GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			return;
		}
		if (!WorkshopUpdates.MetadataRefreshing)
		{
			WorkshopBranches.RefreshCatalogs();
		}
		WorkshopChangeNoteFeed.Refresh();
		UpdateWorkshopChrome();
		UpdateDetails();
		QueueRebuild();
		if (WorkshopUpdates.MetadataRetryScheduled)
		{
			if (!_workshopRetryWasNeeded)
			{
				SetStatus(MlmLocManager.Get("workshop.feedback.metadataRetryScheduled"), "WarningLabel", "!");
			}
			_workshopRetryWasNeeded = true;
		}
		else if (_workshopRetryWasNeeded && WorkshopUpdates.MetadataAvailable)
		{
			_workshopRetryWasNeeded = false;
			SetStatus(MlmLocManager.Get("workshop.feedback.metadataRecovered"), "SuccessLabel", "✓");
		}
	}

	private static void UpdateWorkshopChrome()
	{
		int updateCount = WorkshopUpdates.UpdateCount;
		int num = WorkshopUpdates.DownloadingCount + WorkshopBranches.DownloadingCount;
		int updatedCount = WorkshopUpdates.UpdatedCount;
		int restartCount = WorkshopUpdates.RestartCount;
		int incompatibleUpdateCount = WorkshopUpdates.IncompatibleUpdateCount;
		bool flag = updateCount > 0;
		if (!flag)
		{
			ClearWorkshopUpdateFilter();
		}
		if (_updatesFilter != null)
		{
			((CanvasItem)_updatesFilter).Visible = flag;
		}
		if (_workshopBadge != null && _workshopChip != null)
		{
			((CanvasItem)_workshopBadge).Visible = updateCount > 0 || num > 0 || updatedCount > 0 || restartCount > 0;
			_workshopChip.Text = ((num > 0) ? MlmLocManager.Get("workshop.summary.downloading", ("count", num)) : ((updateCount > 0 && incompatibleUpdateCount > 0) ? MlmLocManager.Get("workshop.summary.updatesWithMismatch", ("updates", updateCount), ("incompatible", incompatibleUpdateCount)) : ((updateCount > 0) ? MlmLocManager.Get("workshop.summary.updates", ("count", updateCount)) : ((restartCount > 0) ? MlmLocManager.Get("workshop.summary.pendingRestart", ("count", restartCount)) : MlmLocManager.Get("workshop.summary.updated", ("count", updatedCount))))));
			_workshopTone = ((num > 0) ? BadgeTone.Caution : ((updateCount > 0 && incompatibleUpdateCount > 0) ? BadgeTone.Critical : ((updateCount > 0 || restartCount > 0) ? BadgeTone.Caution : BadgeTone.Positive)));
			UiKit.RestyleBadge(_workshopBadge, _workshopTone);
		}
		if (_updateAll != null)
		{
			((CanvasItem)_updateAll).Visible = flag;
			((BaseButton)_updateAll).Disabled = _pumpActive || WorkshopUpdates.Checking;
		}
		if (_restartGame != null)
		{
			((CanvasItem)_restartGame).Visible = WorkshopUpdates.RestartNeeded;
			((BaseButton)_restartGame).Disabled = _pumpActive;
		}
		AnnounceWorkshopSummary();
	}

	private static void ClearWorkshopUpdateFilter()
	{
		if (_filter == ModFilter.Updates)
		{
			_filter = ModFilter.All;
			if (FilterChips.Count > 0)
			{
				((BaseButton)FilterChips[0]).ButtonPressed = true;
			}
			QueueRebuild();
		}
	}

	private static void AnnounceWorkshopSummary()
	{
		if (!_workshopSummaryShown && !WorkshopUpdates.Checking && WorkshopUpdates.TrackedCount != 0)
		{
			_workshopSummaryShown = true;
			int updateCount = WorkshopUpdates.UpdateCount;
			int incompatibleUpdateCount = WorkshopUpdates.IncompatibleUpdateCount;
			if (updateCount > 0)
			{
				SetStatus((incompatibleUpdateCount > 0) ? MlmLocManager.Get("workshop.feedback.updatesWithBranchMismatch", ("updates", updateCount), ("incompatible", incompatibleUpdateCount), ("branch", ModErrorText.CurrentBranchName())) : MlmLocManager.Get("workshop.feedback.updatesAvailable", ("updates", updateCount)), "WarningLabel", "!");
			}
			else if (WorkshopUpdates.CheckFailed)
			{
				SetStatus(MlmLocManager.Get("workshop.feedback.checkUnavailable"), "WarningLabel", "!");
			}
			else
			{
				SetStatus(MlmLocManager.Get("workshop.feedback.upToDate", ("count", WorkshopUpdates.TrackedCount)), "SuccessLabel", "✓");
			}
		}
	}

	private static void DownloadAllWorkshopUpdates()
	{
		if (!_pumpActive)
		{
			int incompatibleUpdateCount = WorkshopUpdates.IncompatibleUpdateCount;
			int num = WorkshopUpdates.StartAllDownloads();
			if (num > 0)
			{
				ClearWorkshopUpdateFilter();
			}
			ShowFeedback((num > 0) ? PlanResult.Ok((incompatibleUpdateCount > 0) ? MlmLocManager.Get("workshop.feedback.downloadedAllWithBranchMismatch", ("started", num), ("incompatible", incompatibleUpdateCount), ("branch", ModErrorText.CurrentBranchName())) : MlmLocManager.Get("workshop.feedback.downloadedAll", ("started", num))) : ((incompatibleUpdateCount > 0) ? PlanResult.Ok(MlmLocManager.Get("workshop.feedback.noDownloadsWithBranchMismatch", ("incompatible", incompatibleUpdateCount), ("branch", ModErrorText.CurrentBranchName()))) : PlanResult.Error(MlmLocManager.Get("workshop.feedback.downloadRefused"))));
			UpdateWorkshopChrome();
			QueueRebuild();
		}
	}

	private static void DownloadSelectedWorkshopUpdate()
	{
		if (_selected == null || _pumpActive)
		{
			return;
		}
		WorkshopStatus workshopStatus = WorkshopUpdates.For(_selected.Mod);
		if (workshopStatus != null)
		{
			bool num = WorkshopUpdates.StartDownload(workshopStatus.ItemId);
			if (num)
			{
				ClearWorkshopUpdateFilter();
			}
			ShowFeedback(num ? PlanResult.Ok(MlmLocManager.Get("workshop.feedback.downloadedOne", ("name", _selected.DisplayName))) : PlanResult.Error(MlmLocManager.Get("workshop.feedback.downloadRefused")));
			UpdateDetails();
			UpdateWorkshopChrome();
			QueueRebuild();
		}
	}

	private static void SwitchSelectedWorkshopBranch()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (_selected != null && !_pumpActive && _detailWorkshopBranchSelector != null)
		{
			Variant itemMetadata = _detailWorkshopBranchSelector.GetItemMetadata(_detailWorkshopBranchSelector.Selected);
			string branch = ((Variant)(ref itemMetadata)).AsString();
			ShowFeedback(WorkshopBranches.Switch(_selected.Mod, branch));
			UpdateDetails();
		}
	}

	private static void OnWorkshopBranchChanged(WorkshopBranchStatus? changed)
	{
		if (_plan == null || _layer == null || !GodotObject.IsInstanceValid((GodotObject)(object)_layer))
		{
			return;
		}
		foreach (LaunchEntry entry in _plan.Entries)
		{
			entry.InvalidateManifestMetadata();
		}
		_optionsEditingKey = null;
		UpdateDetails();
		UpdateWorkshopChrome();
		UpdatePlanSummary();
		QueueRebuild();
		if (_selected == null)
		{
			return;
		}
		WorkshopBranchStatus workshopBranchStatus = WorkshopBranches.For(_selected.Mod);
		if (workshopBranchStatus != null && !workshopBranchStatus.Busy && workshopBranchStatus == changed)
		{
			if (workshopBranchStatus.ErrorKey != null)
			{
				ShowFeedback(PlanResult.Error(MlmLocManager.Get(workshopBranchStatus.ErrorKey)));
			}
			else if (workshopBranchStatus.Active != null)
			{
				ShowFeedback(PlanResult.Ok(MlmLocManager.Get("workshop.switch.ready", ("branch", workshopBranchStatus.Active.Branch))));
			}
		}
	}

	private static void OpenSelectedWorkshopPage()
	{
		if (_selected == null)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("mods.selection.required")));
			return;
		}
		ulong num = WorkshopUpdates.ItemIdFor(_selected.Mod);
		if (num == 0L)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("workshop.feedback.itemIdMissing")));
			return;
		}
		string text = $"https://steamcommunity.com/sharedfiles/filedetails/?id={num}";
		string displayName = _selected.DisplayName;
		try
		{
			ShowFeedback(SteamOverlay.OpenPage(text) switch
			{
				PageOpenResult.InGame => PlanResult.Ok(MlmLocManager.Get("workshop.feedback.pageOpened", ("name", displayName))), 
				PageOpenResult.InSteam => PlanResult.Ok(MlmLocManager.Get("workshop.feedback.pageOpenedInSteam", ("name", displayName))), 
				PageOpenResult.InBrowser => PlanResult.Ok(MlmLocManager.Get("workshop.feedback.pageOpenedInBrowser", ("name", displayName))), 
				_ => PlanResult.Error(MlmLocManager.Get("workshop.feedback.browserOpenFailed", ("url", text))), 
			});
		}
		catch (Exception ex)
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("workshop.feedback.pageOpenFailed", ("error", ex.Message))));
		}
	}

	private static VBoxContainer BuildWorkshopDetails()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Expected O, but got Unknown
		_detailWorkshop = DetailSection("details.workshop.caption", visible: false);
		GridContainer val = FactGrid();
		_detailWorkshopState = AddFact(val, "details.workshop.fact.updateStatus");
		_detailWorkshopAvailableBranches = AddFact(val, "details.workshop.fact.availableBranches", out Control fact);
		BindKeyTooltip(fact, "details.workshop.availableBranches.tooltip");
		_detailWorkshopLocal = AddFact(val, "details.workshop.fact.localFiles");
		_detailWorkshopServer = AddFact(val, "details.workshop.fact.listingEdited");
		_detailWorkshopBranch = AddFact(val, "details.workshop.fact.latestBranch");
		_detailWorkshopId = AddFact(val, "details.workshop.fact.itemId");
		((Node)_detailWorkshop).AddChild((Node)(object)val, false, (InternalMode)0);
		HFlowContainer val2 = new HFlowContainer();
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 8);
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 8);
		_detailWorkshopOpen = LocalizedKeyButton("details.workshop.action.openPage", "SecondaryButton", 154f, 34f);
		BindKeyTooltip((Control)(object)_detailWorkshopOpen, "details.workshop.openPage.tooltip");
		((BaseButton)_detailWorkshopOpen).Pressed += OpenSelectedWorkshopPage;
		_detailWorkshopUpdate = LocalizedKeyButton("details.workshop.action.downloadNow", "PrimaryButton", 0f, 34f);
		BindKeyTooltip((Control)(object)_detailWorkshopUpdate, "details.workshop.downloadNow.tooltip");
		((CanvasItem)_detailWorkshopUpdate).Visible = false;
		((BaseButton)_detailWorkshopUpdate).Pressed += DownloadSelectedWorkshopUpdate;
		_detailWorkshopUnsubscribe = LocalizedKeyButton("workshop.remove.unsubscribe", "SecondaryButton", 0f, 34f);
		((BaseButton)_detailWorkshopUnsubscribe).Pressed += delegate
		{
			LaunchEntry selected = _selected;
			if (selected != null)
			{
				RequestWorkshopRemoval(PhysicalModIdentity.WorkshopItemIdFor(selected.Mod), selected.DisplayName, MlmLocManager.Get("workshop.remove.unsubscribe"));
			}
		};
		((Node)val2).AddChild((Node)(object)_detailWorkshopOpen, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_detailWorkshopUnsubscribe, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)_detailWorkshopUpdate, false, (InternalMode)0);
		((Node)_detailWorkshop).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)_detailWorkshop).AddChild((Node)(object)BuildChangeNotesDetails(), false, (InternalMode)0);
		_detailWorkshopBranchLabel = LocalizedKeyLabel("workshop.switch.label", "DimLabel");
		((Node)_detailWorkshop).AddChild((Node)(object)_detailWorkshopBranchLabel, false, (InternalMode)0);
		_detailWorkshopBranchSelector = UiKit.Dropdown();
		((Control)_detailWorkshopBranchSelector).SizeFlagsHorizontal = (SizeFlags)3;
		OptionButton? detailWorkshopBranchSelector = _detailWorkshopBranchSelector;
		object obj = _003C_003Ec._003C_003E9__899_1;
		if (obj == null)
		{
			ItemSelectedEventHandler val3 = delegate(long index)
			{
				//IL_0023: Unknown result type (might be due to invalid IL or missing references)
				//IL_0028: Unknown result type (might be due to invalid IL or missing references)
				if (_selected != null)
				{
					_workshopBranchDraftItem = WorkshopUpdates.ItemIdFor(_selected.Mod);
					Variant itemMetadata = _detailWorkshopBranchSelector.GetItemMetadata((int)index);
					_workshopBranchDraft = ((Variant)(ref itemMetadata)).AsString();
					UpdateWorkshopBranchControls(_selected);
				}
			};
			_003C_003Ec._003C_003E9__899_1 = val3;
			obj = (object)val3;
		}
		detailWorkshopBranchSelector.ItemSelected += (ItemSelectedEventHandler)obj;
		((Node)_detailWorkshop).AddChild((Node)(object)_detailWorkshopBranchSelector, false, (InternalMode)0);
		_detailWorkshopBranchSwitch = LocalizedKeyButton("workshop.switch.action", "PrimaryButton", 0f, 34f);
		((BaseButton)_detailWorkshopBranchSwitch).Pressed += SwitchSelectedWorkshopBranch;
		((Node)_detailWorkshop).AddChild((Node)(object)_detailWorkshopBranchSwitch, false, (InternalMode)0);
		_detailWorkshopBranchHint = UiKit.Label(string.Empty, "DimLabel");
		_detailWorkshopBranchHint.AutowrapMode = (AutowrapMode)3;
		((Node)_detailWorkshop).AddChild((Node)(object)_detailWorkshopBranchHint, false, (InternalMode)0);
		return _detailWorkshop;
	}

	private static HFlowContainer BuildWorkshopTags()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected O, but got Unknown
		_detailWorkshopTagsSignature = string.Empty;
		_detailWorkshopTags = new HFlowContainer
		{
			Visible = false,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Control)_detailWorkshopTags).AddThemeConstantOverride(StringName.op_Implicit("h_separation"), 6);
		((Control)_detailWorkshopTags).AddThemeConstantOverride(StringName.op_Implicit("v_separation"), 6);
		return _detailWorkshopTags;
	}

	private static void UpdateWorkshopTags(LaunchEntry entry)
	{
		if (_detailWorkshopTags == null)
		{
			return;
		}
		string[] array = (WorkshopUpdates.For(entry.Mod)?.Tags ?? Array.Empty<WorkshopTag>()).Select(WorkshopUpdates.TagLabel).ToArray();
		((CanvasItem)_detailWorkshopTags).Visible = array.Length != 0;
		string text = MlmLocManager.EffectiveLocale + "\n" + string.Join('\n', array);
		if (text != _detailWorkshopTagsSignature)
		{
			_detailWorkshopTagsSignature = text;
			foreach (Node child in ((Node)_detailWorkshopTags).GetChildren(false))
			{
				((Node)_detailWorkshopTags).RemoveChild(child);
				child.QueueFree();
			}
			string[] array2 = array;
			foreach (string label in array2)
			{
				Button val = UiKit.Button(label, "TagButton", 0f, 26f);
				((Control)val).MouseDefaultCursorShape = (CursorShape)2;
				((Control)val).TooltipText = MlmLocManager.Get("details.workshop.tag.tooltip", ("tag", label));
				((BaseButton)val).Pressed += delegate
				{
					SearchForWorkshopTag(label);
				};
				((Node)_detailWorkshopTags).AddChild((Node)(object)val, false, (InternalMode)0);
			}
		}
		SetWorkshopTagsEnabled(!_pumpActive);
	}

	private static void SetWorkshopTagsEnabled(bool enabled)
	{
		if (_detailWorkshopTags == null)
		{
			return;
		}
		foreach (Button item in ((IEnumerable)((Node)_detailWorkshopTags).GetChildren(false)).OfType<Button>())
		{
			((BaseButton)item).Disabled = !enabled;
		}
	}

	private static void SearchForWorkshopTag(string tag)
	{
		if (_search != null && !_pumpActive)
		{
			_search.Text = tag;
			_search.CaretColumn = tag.Length;
			QueueRebuild();
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("details.workshop.tag.searched", ("tag", tag))));
		}
	}

	private static void UpdateWorkshopDetails(LaunchEntry entry)
	{
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Invalid comparison between Unknown and I4
		WorkshopStatus workshopStatus = WorkshopUpdates.For(entry.Mod);
		WorkshopBranchInfo workshopBranchInfo = WorkshopBranches.InfoFor(entry.Mod);
		ulong num = workshopStatus?.ItemId ?? WorkshopUpdates.ItemIdFor(entry.Mod);
		bool flag = num != 0;
		if (_detailWorkshop != null)
		{
			((CanvasItem)_detailWorkshop).Visible = flag;
		}
		if (_detailWorkshopOpen != null)
		{
			((CanvasItem)_detailWorkshopOpen).Visible = flag;
			((BaseButton)_detailWorkshopOpen).Disabled = _pumpActive || !flag;
			((Control)_detailWorkshopOpen).TooltipText = (flag ? $"https://steamcommunity.com/sharedfiles/filedetails/?id={num}" : string.Empty);
		}
		if (_detailWorkshopUnsubscribe != null)
		{
			((CanvasItem)_detailWorkshopUnsubscribe).Visible = (int)entry.Mod.modSource == 2 && flag;
			((BaseButton)_detailWorkshopUnsubscribe).Disabled = !CanRemoveWorkshopItem(num);
			((Control)_detailWorkshopUnsubscribe).TooltipText = MlmLocManager.Get(CanRemoveWorkshopItem(num) ? "workshop.remove.tooltip" : "workshop.remove.locked");
		}
		UpdateChangeNotesDetails(num);
		if (!flag)
		{
			if (_detailWorkshopUpdate != null)
			{
				((CanvasItem)_detailWorkshopUpdate).Visible = false;
			}
			return;
		}
		if (_detailWorkshopId != null)
		{
			_detailWorkshopId.Text = num.ToString();
		}
		if (_detailWorkshopState != null)
		{
			_detailWorkshopState.Text = ((workshopStatus == null) ? MlmLocManager.Get("details.workshop.notChecked") : WorkshopUpdates.StateText(workshopStatus));
		}
		if (_detailWorkshopBranch != null)
		{
			Label detailWorkshopBranch = _detailWorkshopBranch;
			WorkshopBranchKind kind = workshopBranchInfo.Kind;
			bool flag2 = (uint)(kind - 1) <= 1u;
			detailWorkshopBranch.Text = (flag2 ? WorkshopUpdates.AvailableBranchesText(workshopBranchInfo) : ((workshopStatus == null) ? MlmLocManager.Get("workshop.branch.unknown") : WorkshopUpdates.LatestBranchText(workshopStatus)));
		}
		if (_detailWorkshopAvailableBranches != null)
		{
			_detailWorkshopAvailableBranches.Text = WorkshopUpdates.AvailableBranchesText(workshopBranchInfo);
		}
		if (_detailWorkshopLocal != null)
		{
			_detailWorkshopLocal.Text = ((workshopStatus == null) ? "—" : WorkshopStatus.FormatTime(workshopStatus.LocalUpdated));
		}
		if (_detailWorkshopServer != null)
		{
			_detailWorkshopServer.Text = ((workshopStatus == null) ? "—" : ((workshopStatus.ServerUpdated == 0) ? WorkshopUpdates.MetadataFallbackText() : WorkshopStatus.FormatTime(workshopStatus.ServerUpdated)));
		}
		if (_detailWorkshopUpdate != null)
		{
			((CanvasItem)_detailWorkshopUpdate).Visible = workshopStatus != null && workshopStatus.State == WorkshopUpdateState.UpdateAvailable && WorkshopBranches.For(entry.Mod)?.Active == null;
			bool flag3 = workshopStatus?.HasIncompatibleAvailableUpdate ?? false;
			((BaseButton)_detailWorkshopUpdate).Disabled = _pumpActive || WorkshopUpdates.Checking;
			_detailWorkshopUpdate.Text = (flag3 ? MlmLocManager.Get("details.workshop.action.downloadAnyway") : MlmLocManager.Get("details.workshop.action.downloadNow"));
			((Control)_detailWorkshopUpdate).TooltipText = (flag3 ? MlmLocManager.Get("details.workshop.downloadAnyway.tooltip", ("branch", ModErrorText.CurrentBranchName())) : MlmLocManager.Get("details.workshop.downloadNow.tooltip"));
		}
		UpdateWorkshopBranchControls(entry);
	}

	private static void UpdateWorkshopBranchControls(LaunchEntry entry)
	{
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		if (_detailWorkshopBranchSelector == null || _detailWorkshopBranchSwitch == null || _detailWorkshopBranchHint == null)
		{
			return;
		}
		WorkshopBranchStatus workshopBranchStatus = WorkshopBranches.For(entry.Mod);
		WorkshopBranchInfo workshopBranchInfo = WorkshopBranches.InfoFor(entry.Mod);
		bool flag = workshopBranchStatus?.Active != null || (workshopBranchStatus?.RestoreFailed ?? false);
		bool flag2 = workshopBranchInfo.Kind == WorkshopBranchKind.Versioned && (workshopBranchStatus?.AvailableBranches.Any() ?? false);
		bool visible = flag2 || flag || (workshopBranchStatus?.Busy ?? false);
		OptionButton detailWorkshopBranchSelector = _detailWorkshopBranchSelector;
		if (_detailWorkshopBranchLabel != null)
		{
			((CanvasItem)_detailWorkshopBranchLabel).Visible = visible;
		}
		((CanvasItem)detailWorkshopBranchSelector).Visible = visible;
		((CanvasItem)_detailWorkshopBranchSwitch).Visible = visible;
		string text = ((_workshopBranchDraftItem == workshopBranchStatus?.ItemId) ? _workshopBranchDraft : (workshopBranchStatus?.Active?.Branch ?? string.Empty));
		detailWorkshopBranchSelector.Clear();
		detailWorkshopBranchSelector.AddItem(MlmLocManager.Get("workshop.switch.auto", ("branch", ModErrorText.CurrentBranchName())), -1);
		detailWorkshopBranchSelector.SetItemMetadata(0, Variant.op_Implicit(string.Empty));
		if (flag2 && workshopBranchStatus != null)
		{
			foreach (string availableBranch in workshopBranchStatus.AvailableBranches)
			{
				detailWorkshopBranchSelector.AddItem(availableBranch, -1);
				detailWorkshopBranchSelector.SetItemMetadata(detailWorkshopBranchSelector.ItemCount - 1, Variant.op_Implicit(availableBranch));
			}
		}
		detailWorkshopBranchSelector.Selected = 0;
		Variant itemMetadata;
		for (int i = 1; i < detailWorkshopBranchSelector.ItemCount; i++)
		{
			itemMetadata = detailWorkshopBranchSelector.GetItemMetadata(i);
			if (((Variant)(ref itemMetadata)).AsString() == text)
			{
				detailWorkshopBranchSelector.Selected = i;
			}
		}
		itemMetadata = detailWorkshopBranchSelector.GetItemMetadata(detailWorkshopBranchSelector.Selected);
		text = ((Variant)(ref itemMetadata)).AsString();
		((BaseButton)detailWorkshopBranchSelector).Disabled = _pumpActive || workshopBranchStatus == null || workshopBranchStatus.Busy || workshopBranchStatus.Locked;
		_detailWorkshopBranchSwitch.Text = MlmLocManager.Get((text.Length == 0) ? "workshop.switch.autoAction" : "workshop.switch.action");
		((BaseButton)_detailWorkshopBranchSwitch).Disabled = ((BaseButton)detailWorkshopBranchSelector).Disabled || ((text.Length != 0) ? (!WorkshopBranches.CanDownload || WorkshopBranches.DownloadPending) : (workshopBranchStatus?.Active == null && (workshopBranchStatus == null || !workshopBranchStatus.RestoreFailed)));
		string text2;
		if (workshopBranchStatus != null && workshopBranchStatus.Busy)
		{
			text2 = MlmLocManager.Get("workshop.switch.downloading", ("branch", workshopBranchStatus.TargetBranch ?? text));
		}
		else
		{
			string text3;
			if (text.Length > 0 && WorkshopBranches.DownloadPending)
			{
				text3 = MlmLocManager.Get("workshop.switch.nativeBusy");
			}
			else
			{
				string text4;
				if (workshopBranchStatus != null && workshopBranchStatus.ErrorKey != null)
				{
					text4 = MlmLocManager.Get(workshopBranchStatus.ErrorKey);
				}
				else
				{
					WorkshopBranchKind kind = workshopBranchInfo.Kind;
					bool flag3 = (uint)(kind - 1) <= 1u;
					text4 = (flag3 ? MlmLocManager.Get(flag ? "workshop.switch.sharedPinned" : ((workshopBranchInfo.Kind == WorkshopBranchKind.AllVersions) ? "workshop.switch.unrestricted" : "workshop.switch.sharedVersion")) : ((workshopBranchInfo.Kind == WorkshopBranchKind.Unknown) ? MlmLocManager.Get("workshop.switch.unknownBranches") : ((workshopBranchInfo.Kind == WorkshopBranchKind.NoPublicVersion) ? MlmLocManager.Get("workshop.switch.noPublicVersion") : ((workshopBranchStatus != null && workshopBranchStatus.Locked) ? MlmLocManager.Get("workshop.switch.loaded") : ((!flag2 && !flag) ? MlmLocManager.Get("workshop.switch.versionUnavailable") : ((text.Length > 0 && !WorkshopBranches.CanDownload) ? MlmLocManager.Get("workshop.switch.clientUnsupported") : ((text.Length > 0 && workshopBranchStatus != null && workshopBranchStatus.Catalog?.ForBranch(text)?.Supports(ModErrorText.CurrentBranchName()) == false) ? MlmLocManager.Get("workshop.switch.crossBranch", ("branch", ModErrorText.CurrentBranchName())) : MlmLocManager.Get("workshop.switch.hint"))))))));
				}
				text3 = text4;
			}
			text2 = text3;
		}
		string text5 = text2;
		if (workshopBranchStatus != null && workshopBranchStatus.Mods.Count > 1)
		{
			text5 = text5 + "\n" + MlmLocManager.Get("workshop.switch.bundle", ("count", workshopBranchStatus.Mods.Count));
		}
		_detailWorkshopBranchHint.Text = text5;
		((Control)_detailWorkshopBranchSwitch).TooltipText = text5;
		if (_detailWorkshopState != null && workshopBranchStatus != null && workshopBranchStatus.Busy)
		{
			_detailWorkshopState.Text = MlmLocManager.Get("workshop.switch.downloading", ("branch", workshopBranchStatus.TargetBranch ?? text));
		}
		else if (_detailWorkshopState != null && workshopBranchStatus?.Active != null)
		{
			_detailWorkshopState.Text = MlmLocManager.Get("workshop.switch.active", ("branch", workshopBranchStatus.Active.Branch));
		}
	}

	private static ModRowStatus? WorkshopUpdateRowStatus(WorkshopStatus? workshop)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		if (workshop == null || WorkshopBranches.IsPinned(workshop.ItemId))
		{
			return null;
		}
		return workshop.State switch
		{
			WorkshopUpdateState.UpdateAvailable => new ModRowStatus(MlmLocManager.Get("mods.status.update"), UiKit.Warning), 
			WorkshopUpdateState.Downloading => new ModRowStatus(MlmLocManager.Get("mods.status.downloading", ("percent", workshop.Progress * 100f)), UiKit.Warning), 
			WorkshopUpdateState.Updated => new ModRowStatus(MlmLocManager.Get("mods.status.updated"), UiKit.Success), 
			WorkshopUpdateState.RestartRequired => new ModRowStatus(MlmLocManager.Get("mods.status.restart"), UiKit.Warning), 
			_ => null, 
		};
	}

	private static void FillWorkshopAlias()
	{
		if (_selected != null && _detailAlias != null && !_pumpActive)
		{
			string text = SuggestedWorkshopAlias(_selected);
			if (text.Length != 0)
			{
				_detailAlias.Text = text;
				((Control)_detailAlias).GrabFocus();
				_detailAlias.CaretColumn = text.Length;
				UpdateOrganizationSaveState();
			}
		}
	}

	private static string SuggestedWorkshopAlias(LaunchEntry entry)
	{
		string text = WorkshopUpdates.For(entry.Mod)?.Title?.Trim() ?? string.Empty;
		if (text.Length == 0)
		{
			return string.Empty;
		}
		LineEdit? detailAlias = _detailAlias;
		int num = ((detailAlias != null) ? detailAlias.MaxLength : 80);
		if (text.Length > num)
		{
			return text.Substring(0, num).TrimEnd();
		}
		return text;
	}

	private static List<LaunchEntry> WorkshopItemEntries(ulong itemId)
	{
		return _plan?.Entries.Where((LaunchEntry entry) => (int)entry.Mod.modSource == 2 && PhysicalModIdentity.WorkshopItemIdFor(entry.Mod) == itemId).ToList() ?? new List<LaunchEntry>();
	}

	private static bool CanRemoveWorkshopItem(ulong itemId)
	{
		if (itemId != 0 && _plan != null && !_pumpActive && !TourOpen && WorkshopUpdates.IsAvailable && !WorkshopSubscription.Busy)
		{
			WorkshopBranchStatus? workshopBranchStatus = WorkshopBranches.ForItem(itemId);
			if (workshopBranchStatus == null || !workshopBranchStatus.Busy)
			{
				return WorkshopItemEntries(itemId).All((LaunchEntry entry) => !entry.IsManager && (int)entry.Mod.state != 1 && CollectibleModAssemblyLoader.GetRecordedAssemblies(entry.Mod).Length == 0);
			}
		}
		return false;
	}

	private static void RequestWorkshopRemoval(ulong itemId, string name, string action)
	{
		if (!CanRemoveWorkshopItem(itemId))
		{
			ShowFeedback(PlanResult.Error(MlmLocManager.Get("workshop.remove.locked")));
			return;
		}
		List<LaunchEntry> list = WorkshopItemEntries(itemId);
		string affected = ((list.Count > 1) ? MlmLocManager.Get("workshop.remove.bundle", ("names", NameList(list))) : string.Empty);
		WorkshopRemovalPrompt.Show(name, action, affected, delegate(Action<bool> done)
		{
			if (!CanRemoveWorkshopItem(itemId))
			{
				done(obj: false);
			}
			else
			{
				WorkshopSubscription.Remove(itemId, delegate(bool success)
				{
					Defer(delegate
					{
						if (success)
						{
							CompleteWorkshopRemoval(itemId, name);
						}
						done(success);
					});
				});
			}
		});
	}

	private static void CompleteWorkshopRemoval(ulong itemId, string name)
	{
		if (_plan != null)
		{
			RemoveLocalEntries(WorkshopItemEntries(itemId));
			WorkshopUpdates.Forget(itemId);
			WorkshopCollectionImportDialog.ItemRemoved(itemId);
			UpdateWorkshopChrome();
			UpdateDetails();
			UpdatePlanSummary();
			QueueRebuild();
			Defer(RebuildProfileRow);
			ShowFeedback(PlanResult.Ok(MlmLocManager.Get("workshop.remove.success", ("name", name))));
			WorkshopDownloadsDialog.Render();
		}
	}

	[IteratorStateMachine(typeof(_003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed))]
	[CompilerGenerated]
	internal static IEnumerable<TreeItem> _003CFolderHeaders_003Eg__Walk_007C565_0(TreeItem? first)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003C_003CFolderHeaders_003Eg__Walk_007C565_0_003Ed(-2)
		{
			_003C_003E3__first = first
		};
	}
}

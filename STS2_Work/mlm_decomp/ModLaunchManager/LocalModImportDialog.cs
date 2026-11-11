using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Modding;

namespace ModLaunchManager;

internal static class LocalModImportDialog
{
	private sealed record PendingImport(IReadOnlyList<LocalModImportUnit> Units, IReadOnlyList<LocalModImportIssue> Issues, string ModsFolder, string? Staging);

	private const int IconSize = 22;

	private static CanvasLayer? _layer;

	private static Button? _fromArchive;

	private static Button? _fromFolder;

	private static Button? _close;

	private static PanelContainer? _statusPanel;

	private static Label? _status;

	private static HBoxContainer? _confirmActions;

	private static Button? _confirmReplace;

	private static LocalModImportHost? _host;

	private static PendingImport? _pending;

	private static bool _busy;

	private static readonly StyleBoxEmpty NoFocusRing = new StyleBoxEmpty();

	internal static bool IsOpen
	{
		get
		{
			if (_layer != null)
			{
				return GodotObject.IsInstanceValid((GodotObject)(object)_layer);
			}
			return false;
		}
	}

	internal static void Show(LocalModImportHost host)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Expected O, but got Unknown
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Expected O, but got Unknown
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Expected O, but got Unknown
		//IL_0308: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Expected O, but got Unknown
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_033d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Expected O, but got Unknown
		//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Expected O, but got Unknown
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Expected O, but got Unknown
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Expected O, but got Unknown
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Expected O, but got Unknown
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0605: Unknown result type (might be due to invalid IL or missing references)
		if (IsOpen)
		{
			return;
		}
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree val = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (val != null)
		{
			_host = host;
			_layer = new CanvasLayer
			{
				Name = StringName.op_Implicit("LocalModImportDialog"),
				Layer = 1200
			};
			Control val2 = UiKit.FullRect<Control>(new Control
			{
				Name = StringName.op_Implicit("LocalModImportRoot"),
				MouseFilter = (MouseFilterEnum)0,
				Theme = UiKit.CreateTheme()
			});
			((Node)val2).AddChild((Node)(object)UiKit.FullRect<ColorRect>(new ColorRect
			{
				Color = UiKit.Alpha(UiKit.Canvas, 0.7f),
				MouseFilter = (MouseFilterEnum)0
			}), false, (InternalMode)0);
			CenterContainer val3 = UiKit.FullRect<CenterContainer>(new CenterContainer());
			PanelContainer val4 = UiKit.Card("DialogPanel");
			((Control)val4).CustomMinimumSize = new Vector2(500f, 0f);
			((Control)val4).SizeFlagsVertical = (SizeFlags)4;
			((Control)val4).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.DialogBox(24, 22));
			VBoxContainer val5 = new VBoxContainer();
			((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 0);
			HBoxContainer val6 = new HBoxContainer();
			((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			Label val7 = UiKit.Label(MlmLocManager.Get("localImport.title"), "SectionLabel");
			((Control)val7).SizeFlagsHorizontal = (SizeFlags)3;
			val7.VerticalAlignment = (VerticalAlignment)1;
			_close = UiKit.Button("✕", "IconButton", 30f, 30f);
			RingOnlyForKeys((Control)(object)_close);
			((Control)_close).TooltipText = MlmLocManager.Get("localImport.close");
			((BaseButton)_close).Pressed += Close;
			((Node)val6).AddChild((Node)(object)val7, false, (InternalMode)0);
			((Node)val6).AddChild((Node)(object)_close, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)val6, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)Gap(6), false, (InternalMode)0);
			Label val8 = UiKit.Label(MlmLocManager.Get("localImport.body"), "MutedLabel");
			val8.AutowrapMode = (AutowrapMode)3;
			((Node)val5).AddChild((Node)(object)val8, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)Gap(18), false, (InternalMode)0);
			PanelContainer val9 = new PanelContainer();
			((Control)val9).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.InsetBox(6, 6));
			VBoxContainer val10 = new VBoxContainer();
			((Control)val10).AddThemeConstantOverride(StringName.op_Implicit("separation"), 4);
			_fromArchive = SourceRow(UiGlyphs.Archive, "localImport.fromArchive", "localImport.fromArchive.caption");
			((BaseButton)_fromArchive).Pressed += PickArchive;
			_fromFolder = SourceRow(UiGlyphs.Folder, "localImport.fromFolder", "localImport.fromFolder.caption");
			((BaseButton)_fromFolder).Pressed += PickFolder;
			MarginContainer val11 = new MarginContainer();
			((Control)val11).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 66);
			((Control)val11).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 12);
			((Node)val11).AddChild((Node)new ColorRect
			{
				Color = UiKit.Border,
				CustomMinimumSize = new Vector2(0f, 1f)
			}, false, (InternalMode)0);
			((Node)val10).AddChild((Node)(object)_fromArchive, false, (InternalMode)0);
			((Node)val10).AddChild((Node)(object)val11, false, (InternalMode)0);
			((Node)val10).AddChild((Node)(object)_fromFolder, false, (InternalMode)0);
			((Node)val9).AddChild((Node)(object)val10, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)val9, false, (InternalMode)0);
			_statusPanel = new PanelContainer
			{
				Visible = false
			};
			((Control)_statusPanel).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.InsetBox(14, 12));
			VBoxContainer val12 = new VBoxContainer();
			((Control)val12).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
			_status = UiKit.Label(string.Empty, "MutedLabel");
			_status.AutowrapMode = (AutowrapMode)3;
			((Node)val12).AddChild((Node)(object)_status, false, (InternalMode)0);
			_confirmActions = new HBoxContainer
			{
				Visible = false
			};
			((Control)_confirmActions).AddThemeConstantOverride(StringName.op_Implicit("separation"), 8);
			Button val13 = UiKit.Button(MlmLocManager.Get("localImport.replace.keep"), "SecondaryButton", 110f, 34f);
			RingOnlyForKeys((Control)(object)val13);
			((BaseButton)val13).Pressed += delegate
			{
				ResolvePendingAsync(replace: false);
			};
			_confirmReplace = UiKit.Button(MlmLocManager.Get("localImport.replace.confirm"), "PrimaryButton", 110f, 34f);
			RingOnlyForKeys((Control)(object)_confirmReplace);
			((BaseButton)_confirmReplace).Pressed += delegate
			{
				ResolvePendingAsync(replace: true);
			};
			((Node)_confirmActions).AddChild((Node)new Control
			{
				SizeFlagsHorizontal = (SizeFlags)3
			}, false, (InternalMode)0);
			((Node)_confirmActions).AddChild((Node)(object)val13, false, (InternalMode)0);
			((Node)_confirmActions).AddChild((Node)(object)_confirmReplace, false, (InternalMode)0);
			((Node)val12).AddChild((Node)(object)_confirmActions, false, (InternalMode)0);
			((Node)_statusPanel).AddChild((Node)(object)val12, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)Gap(12), false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)_statusPanel, false, (InternalMode)0);
			((Node)val5).AddChild((Node)(object)Gap(10), false, (InternalMode)0);
			Button val14 = UiKit.Button(MlmLocManager.Get("localImport.openModsFolder"), "MiniButton", 0f, 28f);
			RingOnlyForKeys((Control)(object)val14);
			val14.Flat = true;
			((Control)val14).SizeFlagsHorizontal = (SizeFlags)0;
			((Control)val14).AddThemeColorOverride(StringName.op_Implicit("font_color"), UiKit.Accent);
			((Control)val14).AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), UiKit.Accent);
			((Control)val14).AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), UiKit.Accent);
			((Control)val14).AddThemeColorOverride(StringName.op_Implicit("font_focus_color"), UiKit.Accent);
			((Control)val14).TooltipText = MlmLocManager.Get("localImport.manualHint");
			((BaseButton)val14).Pressed += delegate
			{
				_host?.OpenModsFolder();
			};
			((Node)val5).AddChild((Node)(object)val14, false, (InternalMode)0);
			((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
			((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
			((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
			((Node)_layer).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val.Root).AddChild((Node)(object)_layer, false, (InternalMode)0);
			LauncherController.Active?.AddScope(new LauncherController.Scope(val2, Close, () => (Control?)(object)_fromArchive));
			((Control)_fromArchive).GrabFocus();
		}
	}

	internal static void Close()
	{
		if (!_busy)
		{
			if (_pending != null)
			{
				DeleteStagingLater(_pending.Staging);
				_pending = null;
			}
			if (_layer != null && GodotObject.IsInstanceValid((GodotObject)(object)_layer))
			{
				((Node)_layer).QueueFree();
			}
			_layer = null;
			_fromArchive = null;
			_fromFolder = null;
			_close = null;
			_statusPanel = null;
			_status = null;
			_confirmActions = null;
			_confirmReplace = null;
			_host = null;
		}
	}

	private static Button SourceRow(Func<int, Color, ImageTexture> icon, string titleKey, string captionKey)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Expected O, but got Unknown
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Expected O, but got Unknown
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Expected O, but got Unknown
		Button obj = UiKit.Button(string.Empty, "RowButton", 0f, 66f);
		((Control)obj).SizeFlagsHorizontal = (SizeFlags)3;
		((Control)obj).MouseDefaultCursorShape = (CursorShape)2;
		MarginContainer val = UiKit.FullRect<MarginContainer>(new MarginContainer
		{
			MouseFilter = (MouseFilterEnum)2
		});
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 12);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 12);
		HBoxContainer val2 = new HBoxContainer
		{
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 14);
		PanelContainer val3 = new PanelContainer
		{
			CustomMinimumSize = new Vector2(40f, 40f),
			SizeFlagsVertical = (SizeFlags)4,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val3).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)UiKit.Box(UiKit.AccentSoft, UiKit.Clear, UiKit.Palette.IsLiquidGlass ? 12 : 8, 0, 0, 0));
		((Node)val3).AddChild((Node)new TextureRect
		{
			Texture = (Texture2D)(object)icon(22, UiKit.Accent),
			StretchMode = (StretchModeEnum)3,
			MouseFilter = (MouseFilterEnum)2
		}, false, (InternalMode)0);
		VBoxContainer val4 = new VBoxContainer
		{
			Alignment = (AlignmentMode)1,
			SizeFlagsHorizontal = (SizeFlags)3,
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 2);
		Label val5 = UiKit.Label(MlmLocManager.Get(titleKey));
		((Control)val5).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), UiKit.ScaleFontSize(15));
		((Control)val5).MouseFilter = (MouseFilterEnum)2;
		Label val6 = UiKit.Label(MlmLocManager.Get(captionKey), "CaptionLabel");
		((Control)val6).MouseFilter = (MouseFilterEnum)2;
		((Node)val4).AddChild((Node)(object)val5, false, (InternalMode)0);
		((Node)val4).AddChild((Node)(object)val6, false, (InternalMode)0);
		TextureRect val7 = new TextureRect
		{
			Texture = (Texture2D)(object)UiGlyphs.ChevronRight(14, UiKit.TextDim, 1.6f),
			StretchMode = (StretchModeEnum)3,
			SizeFlagsVertical = (SizeFlags)4,
			MouseFilter = (MouseFilterEnum)2
		};
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val7, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		((Node)obj).AddChild((Node)(object)val, false, (InternalMode)0);
		RingOnlyForKeys((Control)(object)obj);
		return obj;
	}

	private static void RingOnlyForKeys(Control control)
	{
		control.FocusEntered += delegate
		{
			LauncherController? active = LauncherController.Active;
			if ((active != null && active.ControllerMode) || (Input.IsAnythingPressed() && !Input.IsMouseButtonPressed((MouseButton)1) && !Input.IsMouseButtonPressed((MouseButton)2)))
			{
				control.RemoveThemeStyleboxOverride(StringName.op_Implicit("focus"));
			}
			else
			{
				control.AddThemeStyleboxOverride(StringName.op_Implicit("focus"), (StyleBox)(object)NoFocusRing);
			}
		};
	}

	private static Control Gap(int height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		return new Control
		{
			CustomMinimumSize = new Vector2(0f, (float)height),
			MouseFilter = (MouseFilterEnum)2
		};
	}

	private static void PickArchive()
	{
		if (!_busy && !NativeFileDialogs.TryShow(MlmLocManager.Get("localImport.fromArchive"), StartFolder(), string.Empty, (FileDialogMode)0, new string[1] { "*.zip ; " + MlmLocManager.Get("localImport.archiveFilter") }, delegate(string path)
		{
			ImportAsync(path, archive: true);
		}, out string error))
		{
			ShowStatus(MlmLocManager.Get("localImport.pickerUnavailable", ("error", error)), "WarningLabel");
		}
	}

	private static void PickFolder()
	{
		if (!_busy && !NativeFileDialogs.TryShow(MlmLocManager.Get("localImport.fromFolder"), StartFolder(), string.Empty, (FileDialogMode)2, Array.Empty<string>(), delegate(string path)
		{
			ImportAsync(path, archive: false);
		}, out string error))
		{
			ShowStatus(MlmLocManager.Get("localImport.pickerUnavailable", ("error", error)), "WarningLabel");
		}
	}

	private static string StartFolder()
	{
		string text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
		if (!Directory.Exists(text))
		{
			return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		}
		return text;
	}

	private static async Task ImportAsync(string source, bool archive)
	{
		if (!IsOpen || _busy || _host == null)
		{
			return;
		}
		if (_pending != null)
		{
			DeleteStagingLater(_pending.Staging);
			_pending = null;
		}
		string modsFolder;
		try
		{
			modsFolder = _host.PrepareModsFolder();
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not prepare the local mods folder: {ex}");
			ShowStatus(MlmLocManager.Get("localImport.failed", ("error", ex.Message)), "DangerLabel");
			return;
		}
		if (!archive && LocalModImport.OverlapsModsFolder(source, modsFolder))
		{
			ShowStatus(Problem(LocalModImportProblem.InsideModsFolder, string.Empty), "WarningLabel");
			return;
		}
		IReadOnlyDictionary<string, LocalModInstalled> installed = _host.Installed();
		string sourceName = (archive ? Path.GetFileNameWithoutExtension(source) : Path.GetFileName(source.TrimEnd('/', '\\')));
		string staging = null;
		bool keepStaging = false;
		SetBusy(busy: true);
		ShowStatus(MlmLocManager.Get("localImport.checking", ("name", sourceName)), "MutedLabel");
		GD.Print($"{"[ModLaunchManager]"} importing local mods from {(archive ? "archive" : "folder")} {source}.");
		try
		{
			LocalModImportScan obj = await Task.Run(delegate
			{
				string root = source;
				if (archive)
				{
					staging = LocalModImport.CreateStagingFolder();
					LocalModImport.ExtractArchive(source, staging);
					root = staging;
				}
				return LocalModImport.Scan(root, sourceName, modsFolder, installed);
			});
			List<LocalModImportIssue> list = obj.Issues.ToList();
			List<LocalModImportUnit> list2 = new List<LocalModImportUnit>();
			foreach (LocalModImportUnit unit in obj.Units)
			{
				LocalModManifest localModManifest = unit.Mods.FirstOrDefault((LocalModManifest mod) => !GameCanRead(mod.ManifestFile));
				if (localModManifest != null)
				{
					list.Add(new LocalModImportIssue(localModManifest.DisplayName, LocalModImportProblem.ManifestBroken));
				}
				else
				{
					list2.Add(unit);
				}
			}
			if (list2.Any((LocalModImportUnit unit) => unit.Replaces != null))
			{
				_pending = new PendingImport(list2, list, modsFolder, staging);
				keepStaging = true;
				SetBusy(busy: false);
				AskToReplace(list2);
			}
			else
			{
				await InstallAsync(list2, list, Array.Empty<string>(), modsFolder);
			}
		}
		catch (LocalModImportException ex2)
		{
			GD.PushWarning("[ModLaunchManager] local mod import failed: " + ex2.Message);
			ShowStatus(Problem(ex2.Problem, ex2.Detail), "DangerLabel");
		}
		catch (Exception ex3)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} local mod import failed: {ex3}");
			ShowStatus(MlmLocManager.Get("localImport.failed", ("error", ex3.Message)), "DangerLabel");
		}
		finally
		{
			if (!keepStaging)
			{
				DeleteStagingLater(staging);
				SetBusy(busy: false);
			}
		}
	}

	private static void AskToReplace(IReadOnlyList<LocalModImportUnit> units)
	{
		List<string> list = new List<string>();
		foreach (LocalModImportUnit item in units.Where((LocalModImportUnit unit) => unit.Replaces != null))
		{
			LocalModInstalled replaces = item.Replaces;
			list.Add(MlmLocManager.Get("localImport.replace.item", ("name", replaces.Name), ("installed", Version(replaces.Version)), ("imported", Version(item.Mods[0].Version))));
		}
		int num = units.Count((LocalModImportUnit unit) => unit.Replaces == null);
		string text = MlmLocManager.Get("localImport.replace.question", ("count", list.Count));
		if (num > 0)
		{
			text = text + "\n" + MlmLocManager.Get("localImport.replace.alsoNew", ("count", num));
		}
		ShowStatus(text + "\n" + string.Join("\n", list), string.Empty);
		if (_confirmActions != null)
		{
			((CanvasItem)_confirmActions).Visible = true;
		}
		SetSourcesEnabled(enabled: false);
		Button? confirmReplace = _confirmReplace;
		if (confirmReplace != null)
		{
			((Control)confirmReplace).GrabFocus();
		}
	}

	private static async Task ResolvePendingAsync(bool replace)
	{
		PendingImport pending = _pending;
		if ((object)pending == null || _busy)
		{
			return;
		}
		_pending = null;
		if (_confirmActions != null)
		{
			((CanvasItem)_confirmActions).Visible = false;
		}
		List<LocalModImportUnit> units = (replace ? pending.Units.ToList() : pending.Units.Where((LocalModImportUnit unit) => unit.Replaces == null).ToList());
		List<string> kept = (replace ? new List<string>() : (from unit in pending.Units
			where unit.Replaces != null
			select unit.Replaces.Name).ToList());
		SetBusy(busy: true);
		ShowStatus(MlmLocManager.Get("localImport.copying"), "MutedLabel");
		try
		{
			await InstallAsync(units, pending.Issues.ToList(), kept, pending.ModsFolder);
		}
		catch (Exception ex)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} local mod import failed: {ex}");
			ShowStatus(MlmLocManager.Get("localImport.failed", ("error", ex.Message)), "DangerLabel");
		}
		finally
		{
			DeleteStagingLater(pending.Staging);
			SetBusy(busy: false);
		}
	}

	private static async Task InstallAsync(IReadOnlyList<LocalModImportUnit> units, List<LocalModImportIssue> issues, IReadOnlyList<string> kept, string modsFolder)
	{
		LocalModWatcher.MarkKnown(units.Where((LocalModImportUnit unit) => unit.Replaces != null).SelectMany((LocalModImportUnit unit) => LocalModImport.TargetManifests(unit, modsFolder)));
		string backups = LocalModImport.CreateStagingFolder();
		LocalModWatcher.HoldRemovals();
		List<LocalModImportUnit> list2;
		try
		{
			list2 = await Task.Run(delegate
			{
				List<LocalModImportUnit> list = new List<LocalModImportUnit>();
				foreach (LocalModImportUnit unit in units)
				{
					try
					{
						string value = LocalModImport.Install(unit, modsFolder, backups);
						GD.Print($"{"[ModLaunchManager]"} {((unit.Replaces == null) ? "imported" : "replaced")} {string.Join(", ", unit.Mods.Select((LocalModManifest mod) => mod.Id))} into {value}.");
						list.Add(unit);
					}
					catch (LocalModImportException ex)
					{
						GD.PushWarning($"{"[ModLaunchManager]"} could not import {unit.Folder}: {ex.Message}");
						issues.Add(new LocalModImportIssue(unit.DisplayName, ex.Problem, ex.Detail));
					}
				}
				return list;
			});
		}
		finally
		{
			LocalModWatcher.ReleaseRemovals();
		}
		DeleteStagingLater(backups);
		List<string> list3 = (from unit in list2
			where unit.Replaces != null
			select unit.Replaces.Id).ToList();
		if (list3.Count > 0)
		{
			_host?.Replaced(list3);
		}
		foreach (LocalModImportIssue issue in issues)
		{
			GD.Print($"{"[ModLaunchManager]"} skipped '{issue.Label}' while importing: {issue.Problem} {issue.Detail}");
		}
		ShowResult(list2, issues, kept);
	}

	private static bool GameCanRead(string manifestFile)
	{
		if (!ModManifestReader.IsAvailable)
		{
			return true;
		}
		try
		{
			Mod val = ModManifestReader.Read(manifestFile, (ModSource)1);
			int result;
			if (val != null)
			{
				ModManifest manifest = val.manifest;
				if (manifest != null)
				{
					result = ((manifest.id != null) ? 1 : 0);
					goto IL_002b;
				}
			}
			result = 0;
			goto IL_002b;
			IL_002b:
			return (byte)result != 0;
		}
		catch
		{
			return false;
		}
	}

	private static void ShowResult(IReadOnlyList<LocalModImportUnit> done, IReadOnlyList<LocalModImportIssue> issues, IReadOnlyList<string> kept)
	{
		if (done.Count == 0 && kept.Count == 0 && issues.Count == 1 && issues[0].Problem == LocalModImportProblem.NoMods)
		{
			ShowStatus(Problem(LocalModImportProblem.NoMods, string.Empty), "WarningLabel");
			return;
		}
		List<string> list = new List<string>();
		List<LocalModImportUnit> list2 = done.Where((LocalModImportUnit unit) => unit.Replaces == null).ToList();
		List<LocalModImportUnit> list3 = done.Where((LocalModImportUnit unit) => unit.Replaces != null).ToList();
		if (list2.Count > 0)
		{
			list.Add(MlmLocManager.Get("localImport.done", ("count", list2.Sum((LocalModImportUnit unit) => unit.Mods.Count)), ("names", string.Join("、", list2.Select((LocalModImportUnit unit) => unit.DisplayName)))));
		}
		if (list3.Count > 0)
		{
			list.Add(MlmLocManager.Get("localImport.replaced", ("count", list3.Count), ("names", string.Join("、", list3.Select((LocalModImportUnit unit) => unit.DisplayName)))));
		}
		if (kept.Count > 0)
		{
			list.Add(MlmLocManager.Get("localImport.kept", ("names", string.Join("、", kept))));
		}
		foreach (LocalModImportIssue issue in issues)
		{
			list.Add(MlmLocManager.Get("localImport.skipped", ("name", issue.Label), ("reason", Problem(issue.Problem, issue.Detail))));
		}
		ShowStatus(string.Join("\n", list), (done.Count == 0 && kept.Count == 0) ? "DangerLabel" : ((issues.Count > 0) ? "WarningLabel" : "SuccessLabel"));
	}

	private static string Version(string? version)
	{
		if (!string.IsNullOrWhiteSpace(version))
		{
			return version;
		}
		return MlmLocManager.Get("common.value.unknown");
	}

	private static string Problem(LocalModImportProblem problem, string detail)
	{
		return problem switch
		{
			LocalModImportProblem.NoMods => MlmLocManager.Get("localImport.problem.noMods"), 
			LocalModImportProblem.UnsupportedArchive => MlmLocManager.Get("localImport.problem.unsupportedArchive"), 
			LocalModImportProblem.ArchiveBroken => MlmLocManager.Get("localImport.problem.archiveBroken"), 
			LocalModImportProblem.TooLarge => MlmLocManager.Get("localImport.problem.tooLarge"), 
			LocalModImportProblem.InsideModsFolder => MlmLocManager.Get("localImport.problem.insideModsFolder"), 
			LocalModImportProblem.ManifestBroken => MlmLocManager.Get("localImport.problem.manifestBroken"), 
			LocalModImportProblem.MissingFile => MlmLocManager.Get("localImport.problem.missingFile", ("file", detail)), 
			LocalModImportProblem.NothingToLoad => MlmLocManager.Get("localImport.problem.nothingToLoad"), 
			LocalModImportProblem.DuplicateInSource => MlmLocManager.Get("localImport.problem.duplicateInSource"), 
			LocalModImportProblem.AlreadyInstalled => MlmLocManager.Get("localImport.problem.alreadyInstalled"), 
			LocalModImportProblem.InstalledFromWorkshop => MlmLocManager.Get("localImport.problem.installedFromWorkshop"), 
			LocalModImportProblem.InstalledInUse => MlmLocManager.Get("localImport.problem.installedInUse"), 
			_ => MlmLocManager.Get("localImport.problem.copyFailed", ("error", detail)), 
		};
	}

	private static void DeleteStagingLater(string? folder)
	{
		if (!string.IsNullOrWhiteSpace(folder))
		{
			Task.Run(delegate
			{
				LocalModImport.DeleteStagingFolder(folder);
			});
		}
	}

	private static void SetBusy(bool busy)
	{
		_busy = busy;
		SetSourcesEnabled(!busy && _pending == null);
		if (_close != null && GodotObject.IsInstanceValid((GodotObject)(object)_close))
		{
			((BaseButton)_close).Disabled = busy;
		}
	}

	private static void SetSourcesEnabled(bool enabled)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Button[] array = (Button[])(object)new Button[2] { _fromArchive, _fromFolder };
		foreach (Button val in array)
		{
			if (val != null && GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				((BaseButton)val).Disabled = !enabled;
				((CanvasItem)val).Modulate = (Color)(enabled ? Colors.White : new Color(1f, 1f, 1f, 0.5f));
			}
		}
	}

	private static void ShowStatus(string text, string variation)
	{
		if (_status != null && GodotObject.IsInstanceValid((GodotObject)(object)_status) && _statusPanel != null)
		{
			if (_confirmActions != null && _pending == null)
			{
				((CanvasItem)_confirmActions).Visible = false;
			}
			_status.Text = text;
			((Control)_status).ThemeTypeVariation = StringName.op_Implicit(variation);
			((CanvasItem)_statusPanel).Visible = text.Length > 0;
		}
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Saves;

namespace ModLaunchManager;

internal static class ManagerReturn
{
	[CompilerGenerated]
	private sealed class _003CHoldFinalQuit_003Ed__41 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CodeInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> _003C_003E3__instructions;

		private MethodInfo _003Cquit_003E5__2;

		private bool _003Cfound_003E5__3;

		private IEnumerator<CodeInstruction> _003C_003E7__wrap3;

		CodeInstruction IEnumerator<CodeInstruction>.Current
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
		public _003CHoldFinalQuit_003Ed__41(int _003C_003E1__state)
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
			_003Cquit_003E5__2 = null;
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
					_003Cquit_003E5__2 = AccessTools.Method(typeof(SceneTree), "Quit", new Type[1] { typeof(int) }, (Type[])null);
					_003Cfound_003E5__3 = false;
					_003C_003E7__wrap3 = instructions.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				if (_003C_003E7__wrap3.MoveNext())
				{
					CodeInstruction current = _003C_003E7__wrap3.Current;
					if (CodeInstructionExtensions.Calls(current, _003Cquit_003E5__2))
					{
						_003Cfound_003E5__3 = true;
						current.opcode = OpCodes.Call;
						current.operand = AccessTools.Method(typeof(ManagerReturn), "QuitUnlessReturning", (Type[])null, (Type[])null);
					}
					_003C_003E2__current = current;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap3 = null;
				if (!_003Cfound_003E5__3)
				{
					throw new MissingMethodException("NGame.Quit no longer ends with SceneTree.Quit");
				}
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
		IEnumerator<CodeInstruction> IEnumerable<CodeInstruction>.GetEnumerator()
		{
			_003CHoldFinalQuit_003Ed__41 _003CHoldFinalQuit_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CHoldFinalQuit_003Ed__ = this;
			}
			else
			{
				_003CHoldFinalQuit_003Ed__ = new _003CHoldFinalQuit_003Ed__41(0);
			}
			_003CHoldFinalQuit_003Ed__.instructions = _003C_003E3__instructions;
			return _003CHoldFinalQuit_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
		}
	}

	private static readonly TimeSpan HandOverTimeout = TimeSpan.FromMinutes(2L);

	private static readonly TimeSpan PreviousGameExitTimeout = TimeSpan.FromSeconds(60L);

	private static readonly TimeSpan KeepOnTopFor = TimeSpan.FromSeconds(20L);

	private static readonly TimeSpan LogCheckInterval = TimeSpan.FromMilliseconds(500L, 0L);

	private static readonly TimeSpan HeldExitTimeout = TimeSpan.FromSeconds(15L);

	private static readonly TimeSpan StartupHeldExitTimeout = TimeSpan.FromSeconds(2L);

	private static bool _holdsFinalQuit;

	private static bool _exitReached;

	private static MethodInfo? _shippedQuit;

	private static SceneTree? _tree;

	private static Control? _focus;

	private static CanvasLayer? _message;

	private static Label? _text;

	private static Button? _retry;

	private static Button? _close;

	private static Button? _resume;

	private static Process? _newGame;

	private static bool _busy;

	private static bool _saving;

	private static bool _saved;

	private static bool _asRestart;

	private static string? _readyFile;

	private static int _previousGame;

	internal static bool IsReturning { get; private set; }

	private static bool Smoke => Environment.GetEnvironmentVariable("MLM_RETURN_SMOKE") == "1";

	private static string ExitingText => MlmLocManager.Get(_asRestart ? "restart.inProgress" : "runtime.exiting");

	internal static void Install(Harmony harmony)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Expected O, but got Unknown
		_readyFile = Environment.GetEnvironmentVariable("MLM_RETURN_READY");
		int.TryParse(Environment.GetEnvironmentVariable("MLM_RETURN_PREVIOUS_GAME"), NumberStyles.Integer, CultureInfo.InvariantCulture, out _previousGame);
		Environment.SetEnvironmentVariable("MLM_RETURN_READY", null);
		Environment.SetEnvironmentVariable("MLM_RETURN_PREVIOUS_GAME", null);
		if (_readyFile != null)
		{
			GD.Print($"{"[ModLaunchManager]"} started by \"back to manager\" from process {_previousGame}.");
		}
		harmony.Patch((MethodBase)AccessTools.Method(typeof(NGame), "Quit", (Type[])null, (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(ManagerReturn), "HoldFinalQuit", (Type[])null), (HarmonyMethod)null);
		_holdsFinalQuit = true;
		if (_readyFile != null)
		{
			MethodInfo methodInfo = AccessTools.Method(typeof(NGame), "LaunchMainMenu", (Type[])null, (Type[])null);
			if ((object)methodInfo != null)
			{
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(ManagerReturn), "SkipIntro", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
		}
	}

	internal static void Begin(Control focus)
	{
		Begin(focus, duringStartup: false);
	}

	internal static bool BeginDuringStartup(Control focus)
	{
		if (IsReturning)
		{
			return false;
		}
		_asRestart = true;
		Begin(focus, duringStartup: true);
		if (!IsReturning)
		{
			_asRestart = false;
		}
		return IsReturning;
	}

	private static void Begin(Control focus, bool duringStartup)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if (!IsReturning && NGame.Instance != null && (!StartupGate.ShouldSuspend() || duringStartup))
		{
			_tree = ((Node)focus).GetTree();
			_focus = focus;
			IsReturning = true;
			ShowMessage();
			Callable val = Callable.From((Action)delegate
			{
				TaskHelper.RunSafely(ReturnAsync());
			});
			((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
		}
	}

	private static async Task ReturnAsync()
	{
		if (_busy || _tree == null)
		{
			return;
		}
		_busy = true;
		string ready = ManagerReturnProtocol.NewReadyFile();
		try
		{
			_text.Text = ExitingText;
			Button? retry = _retry;
			Button? close = _close;
			bool flag2 = (((CanvasItem)_resume).Visible = false);
			bool visible = (((CanvasItem)close).Visible = flag2);
			((CanvasItem)retry).Visible = visible;
			if (!_saved)
			{
				await SaveAndFreeze();
			}
			Stopwatch startup = Stopwatch.StartNew();
			_newGame = Process.Start(NewGameStart(ready, afterExit: false, out string log)) ?? throw new InvalidOperationException("The game could not be started again");
			GD.Print($"{"[ModLaunchManager]"} back to manager: started process {_newGame.Id}; its log is {log}");
			ManagerReturnProtocol.LogTail newLog = new ManagerReturnProtocol.LogTail(log);
			TimeSpan nextLogCheck = LogCheckInterval;
			while (!File.Exists(ready))
			{
				if (_newGame.HasExited)
				{
					throw new InvalidOperationException($"The new game exited ({_newGame.ExitCode}); see {log}");
				}
				if (startup.Elapsed > HandOverTimeout)
				{
					throw new TimeoutException("The new game did not show the manager; see " + log);
				}
				if (startup.Elapsed > KeepOnTopFor && _tree.Root.AlwaysOnTop)
				{
					_tree.Root.AlwaysOnTop = false;
				}
				if (startup.Elapsed > nextLogCheck)
				{
					nextLogCheck = startup.Elapsed + LogCheckInterval;
					if (ManagerReturnProtocol.IsRendererFailure(newLog.ReadNew()))
					{
						await ReopenAfterExit(log);
						return;
					}
				}
				await ((GodotObject)_tree).ToSignal((GodotObject)(object)_tree, SignalName.ProcessFrame);
			}
			GD.Print($"{"[ModLaunchManager]"} back to manager: shown after {startup.Elapsed.TotalSeconds:F1}s; closing this game.");
			CloseThisGame();
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} back to manager failed: {value}");
			await StopNewGame();
			_text.Text = MlmLocManager.Get((!_asRestart) ? (_saved ? "return.failed" : "return.failedBeforeSave") : (_saved ? "restart.returnFailed" : "restart.returnFailedBeforeSave"));
			((CanvasItem)_retry).Visible = true;
			((CanvasItem)_close).Visible = _saved;
			((CanvasItem)_resume).Visible = !_saved;
			((Control)_retry).GrabFocus();
		}
		finally
		{
			_busy = false;
			ManagerReturnProtocol.TryDelete(ready);
		}
	}

	private static async Task SaveAndFreeze()
	{
		if (AccessTools.Property(typeof(SaveManager), "CurrentRunSaveTask")?.GetValue(SaveManager.Instance) is Task task)
		{
			await task;
		}
		NGame game = NGame.Instance ?? throw new InvalidOperationException("No running game");
		bool starting = StartupGate.ShouldSuspend();
		_saving = true;
		try
		{
			game.Quit();
			Stopwatch held = Stopwatch.StartNew();
			while (!_saved && held.Elapsed < (starting ? StartupHeldExitTimeout : HeldExitTimeout))
			{
				await ((GodotObject)_tree).ToSignal((GodotObject)(object)_tree, SignalName.ProcessFrame);
			}
			if (!_saved && starting)
			{
				QuitAsShipped(game);
			}
		}
		finally
		{
			_saving = false;
		}
		if (!_saved)
		{
			throw new InvalidOperationException("The game did not reach its exit point");
		}
		foreach (Node child in ((Node)_tree.Root).GetChildren(false))
		{
			if (child != _message)
			{
				Freeze(child);
			}
		}
		_tree.Root.AlwaysOnTop = true;
	}

	private static async Task ReopenAfterExit(string failedLog)
	{
		GD.PushWarning($"{"[ModLaunchManager]"} back to manager: the new game could not start its renderer while this one is open (see {failedLog}); reopening it after this game has closed.");
		await StopNewGame();
		string log;
		ProcessStartInfo processStartInfo = NewGameStart(ManagerReturnProtocol.NewReadyFile(), afterExit: true, out log);
		ProcessStartInfo processStartInfo2 = ManagerReturnProtocol.StartAfterExit(Environment.ProcessId, processStartInfo.FileName, processStartInfo.ArgumentList, processStartInfo.WorkingDirectory);
		processStartInfo2.Environment["MLM_RETURN_READY"] = processStartInfo.Environment["MLM_RETURN_READY"];
		using (Process.Start(processStartInfo2) ?? throw new InvalidOperationException("The game could not be scheduled to start again"))
		{
			GD.Print("[ModLaunchManager] back to manager: the game starts again once this one has closed; its log is " + log);
			CloseThisGame();
		}
	}

	private static void CloseThisGame()
	{
		_tree.Root.AlwaysOnTop = false;
		Thread thread = new Thread((ThreadStart)delegate
		{
			Thread.Sleep(3000);
			Process.GetCurrentProcess().Kill();
		});
		thread.IsBackground = true;
		thread.Name = "ModLaunchManager.ExitWatchdog";
		thread.Start();
		_tree.Quit(0);
	}

	private static void Freeze(Node node)
	{
		node.ProcessMode = (ProcessModeEnum)4;
		foreach (Node child in node.GetChildren(false))
		{
			Freeze(child);
		}
	}

	private static ProcessStartInfo NewGameStart(string ready, bool afterExit, out string log)
	{
		DateTime localTime = DateTime.Now;
		log = ManagerReturnProtocol.SessionLogFile(ManagerOverlay.GameLogFile(), localTime);
		while (File.Exists(log))
		{
			log = ManagerReturnProtocol.SessionLogFile(ManagerOverlay.GameLogFile(), localTime = localTime.AddSeconds(1.0));
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo(OS.GetExecutablePath())
		{
			UseShellExecute = false,
			WorkingDirectory = Environment.CurrentDirectory
		};
		foreach (string item in ManagerReturnProtocol.Arguments(OS.GetCmdlineArgs(), OS.GetCmdlineUserArgs(), log))
		{
			processStartInfo.ArgumentList.Add(item);
		}
		processStartInfo.Environment["MLM_RETURN_READY"] = ready;
		if (!afterExit)
		{
			processStartInfo.Environment["MLM_RETURN_PREVIOUS_GAME"] = Environment.ProcessId.ToString(CultureInfo.InvariantCulture);
		}
		return processStartInfo;
	}

	[IteratorStateMachine(typeof(_003CHoldFinalQuit_003Ed__41))]
	private static IEnumerable<CodeInstruction> HoldFinalQuit(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CHoldFinalQuit_003Ed__41(-2)
		{
			_003C_003E3__instructions = instructions
		};
	}

	internal static void Quit(NGame game)
	{
		_exitReached = false;
		game.Quit();
		if (_holdsFinalQuit && !_exitReached && StartupGate.ShouldSuspend())
		{
			QuitAsShipped(game);
		}
	}

	private static void QuitAsShipped(NGame game)
	{
		GD.Print("[ModLaunchManager] a mod ignored the game's exit while the game is starting; exiting without other mods' exit patches.");
		PrepareShippedQuit();
		ShippedQuit(game);
	}

	private static void PrepareShippedQuit()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		if ((object)_shippedQuit == null)
		{
			_shippedQuit = Harmony.ReversePatch((MethodBase)AccessTools.Method(typeof(NGame), "Quit", (Type[])null, (Type[])null), new HarmonyMethod(typeof(ManagerReturn), "ShippedQuit", (Type[])null), AccessTools.Method(typeof(ManagerReturn), "HoldFinalQuit", (Type[])null, (Type[])null));
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static void ShippedQuit(NGame game)
	{
		throw new NotImplementedException("Replaced by NGame.Quit as the game ships it");
	}

	private static void QuitUnlessReturning(SceneTree tree, int exitCode)
	{
		_exitReached = true;
		if (_saving)
		{
			_saved = true;
			return;
		}
		if (IsReturning)
		{
			StopNewGameNow();
		}
		tree.Quit(exitCode);
	}

	private static async Task StopNewGame()
	{
		if (_newGame != null)
		{
			StopNewGameNow();
			try
			{
				await _newGame.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10L));
			}
			catch (TimeoutException)
			{
			}
			_newGame.Dispose();
			_newGame = null;
		}
	}

	private static void StopNewGameNow()
	{
		try
		{
			Process newGame = _newGame;
			if (newGame != null && !newGame.HasExited)
			{
				_newGame.Kill();
			}
		}
		catch (Exception ex) when (((ex is InvalidOperationException || ex is Win32Exception) ? 1 : 0) != 0)
		{
			GD.PushWarning("[ModLaunchManager] could not stop the new game: " + ex.Message);
		}
	}

	private static void ShowMessage()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Expected O, but got Unknown
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Expected O, but got Unknown
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected O, but got Unknown
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected O, but got Unknown
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Expected O, but got Unknown
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Expected O, but got Unknown
		if (_tree == null || _message != null)
		{
			return;
		}
		_message = new CanvasLayer
		{
			Layer = 1100,
			ProcessMode = (ProcessModeEnum)3
		};
		Control val = new Control
		{
			MouseFilter = (MouseFilterEnum)0
		};
		val.SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		ColorRect val2 = new ColorRect
		{
			Color = new Color(0f, 0f, 0f, 0.45f),
			MouseFilter = (MouseFilterEnum)2
		};
		((Control)val2).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
		CenterContainer val3 = new CenterContainer();
		((Control)val3).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		VBoxContainer val4 = new VBoxContainer
		{
			Alignment = (AlignmentMode)1
		};
		((Control)val4).AddThemeConstantOverride(StringName.op_Implicit("separation"), 22);
		_text = new Label
		{
			Text = ExitingText,
			HorizontalAlignment = (HorizontalAlignment)1
		};
		((Control)_text).AddThemeFontOverride(StringName.op_Implicit("font"), (Font)(object)UiKit.UiFont);
		((Control)_text).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 26);
		((Control)_text).AddThemeColorOverride(StringName.op_Implicit("font_color"), Colors.White);
		((Control)_text).AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), Colors.Black);
		((Control)_text).AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		((Node)val4).AddChild((Node)(object)_text, false, (InternalMode)0);
		_retry = new Button
		{
			Text = MlmLocManager.Get("return.retry"),
			Visible = false
		};
		((BaseButton)_retry).Pressed += delegate
		{
			if (!_busy)
			{
				TaskHelper.RunSafely(ReturnAsync());
			}
		};
		((Node)val4).AddChild((Node)(object)_retry, false, (InternalMode)0);
		_close = new Button
		{
			Text = MlmLocManager.Get("return.close"),
			Visible = false
		};
		((BaseButton)_close).Pressed += delegate
		{
			SceneTree? tree = _tree;
			if (tree != null)
			{
				tree.Quit(0);
			}
		};
		((Node)val4).AddChild((Node)(object)_close, false, (InternalMode)0);
		_resume = new Button
		{
			Text = MlmLocManager.Get(_asRestart ? "restart.back" : "return.resume"),
			Visible = false
		};
		((BaseButton)_resume).Pressed += KeepPlaying;
		((Node)val4).AddChild((Node)(object)_resume, false, (InternalMode)0);
		((Node)val3).AddChild((Node)(object)val4, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
		((Node)_message).AddChild((Node)(object)val, false, (InternalMode)0);
		((Node)_tree.Root).AddChild((Node)(object)_message, false, (InternalMode)0);
	}

	private static void KeepPlaying()
	{
		if (!_busy && !_saved)
		{
			CanvasLayer? message = _message;
			if (message != null)
			{
				((Node)message).QueueFree();
			}
			_message = null;
			_text = null;
			_retry = (_close = (_resume = null));
			IsReturning = false;
			_asRestart = false;
			if (_focus != null && GodotObject.IsInstanceValid((GodotObject)(object)_focus) && ((CanvasItem)_focus).IsVisibleInTree())
			{
				_focus.GrabFocus();
			}
			_focus = null;
		}
	}

	internal static void ManagerShown(Control manager)
	{
		if (_readyFile != null)
		{
			TaskHelper.RunSafely(TakeOver(manager));
		}
		else if (Smoke)
		{
			Begin(manager, duringStartup: true);
		}
	}

	internal static void ManagerUnavailable()
	{
		if (_readyFile != null)
		{
			TaskHelper.RunSafely(TakeOver(null));
		}
	}

	private static async Task TakeOver(Control? manager)
	{
		string ready = _readyFile;
		_readyFile = null;
		MainLoop mainLoop = Engine.GetMainLoop();
		SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
		if (tree == null)
		{
			return;
		}
		using Process previous = ManagerReturnProtocol.ObserveProcess(_previousGame);
		if (manager != null && previous != null)
		{
			((Node)manager).ProcessMode = (ProcessModeEnum)4;
		}
		await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
		if (manager != null && GodotObject.IsInstanceValid((GodotObject)(object)manager))
		{
			((CanvasItem)manager).QueueRedraw();
		}
		await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
		if (_previousGame > 0)
		{
			ManagerReturnProtocol.WriteReady(ready);
		}
		if (previous != null)
		{
			await WaitForExit(tree, previous);
		}
		if (manager != null && GodotObject.IsInstanceValid((GodotObject)(object)manager))
		{
			((Node)manager).ProcessMode = (ProcessModeEnum)0;
		}
		DisplayServer.WindowMoveToForeground(0);
		GD.Print("[ModLaunchManager] back to manager: the previous game has exited.");
		if (Smoke)
		{
			tree.Quit(0);
		}
	}

	private static async Task WaitForExit(SceneTree tree, Process previous)
	{
		Stopwatch waited = Stopwatch.StartNew();
		while (!previous.HasExited && waited.Elapsed < PreviousGameExitTimeout)
		{
			await ((GodotObject)tree).ToSignal((GodotObject)(object)tree, SignalName.ProcessFrame);
		}
		if (previous.HasExited)
		{
			return;
		}
		GD.PushWarning($"{"[ModLaunchManager]"} previous game {previous.Id} did not exit; ending it.");
		try
		{
			previous.Kill();
			await previous.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10L));
		}
		catch (Exception ex) when (((ex is InvalidOperationException || ex is Win32Exception || ex is TimeoutException) ? 1 : 0) != 0)
		{
			GD.PushWarning("[ModLaunchManager] could not end the previous game: " + ex.Message);
		}
	}

	private static void SkipIntro(ref bool skipLogo)
	{
		skipLogo = true;
	}
}

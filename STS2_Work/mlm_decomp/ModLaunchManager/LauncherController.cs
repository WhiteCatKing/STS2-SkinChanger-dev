using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;

namespace ModLaunchManager;

internal sealed class LauncherController : IDisposable
{
	internal enum Source
	{
		None,
		SteamInput,
		Joypad
	}

	internal sealed record Scope(Control Root, Action Back, Func<Control?> InitialFocus)
	{
		internal Func<bool>? Enabled { get; init; }

		internal Func<Control, bool>? Accept { get; set; }

		internal Func<JoyButton, bool>? Shortcut { get; set; }

		internal Func<Control, Vector2I, bool>? Direction { get; set; }

		internal Func<Control, Vector2I, Control?>? Redirect { get; set; }

		internal Func<Control, int, bool>? Page { get; set; }

		internal Func<ScrollContainer?>? ScrollTarget { get; set; }

		internal Control? ReturnFocus { get; set; }

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Root = ");
			builder.Append(Root);
			builder.Append(", Back = ");
			builder.Append(Back);
			builder.Append(", InitialFocus = ");
			builder.Append(InitialFocus);
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class _003CCandidates_003Ed__115 : IEnumerable<Control>, IEnumerable, IEnumerator<Control>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Control _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private IEnumerator<Control> _003C_003E7__wrap2;

		Control IEnumerator<Control>.Current
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
		public _003CCandidates_003Ed__115(int _003C_003E1__state)
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
				{
					_003C_003E1__state = -1;
					if (root is Window)
					{
						return false;
					}
					Node obj = root;
					Control val = (Control)(object)((obj is Control) ? obj : null);
					if (val != null)
					{
						if (!Live((Node?)(object)val) || !((CanvasItem)val).IsVisibleInTree())
						{
							return false;
						}
						if (Available(val))
						{
							_003C_003E2__current = val;
							_003C_003E1__state = 1;
							return true;
						}
					}
					goto IL_0085;
				}
				case 1:
					_003C_003E1__state = -1;
					goto IL_0085;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00f5;
					}
					IL_00f5:
					if (_003C_003E7__wrap2.MoveNext())
					{
						Control current = _003C_003E7__wrap2.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					goto IL_010f;
					IL_0085:
					_003C_003E7__wrap1 = root.GetChildren(false).GetEnumerator();
					_003C_003E1__state = -3;
					goto IL_010f;
					IL_010f:
					if (_003C_003E7__wrap1.MoveNext())
					{
						Node current2 = _003C_003E7__wrap1.Current;
						_003C_003E7__wrap2 = Candidates(current2).GetEnumerator();
						_003C_003E1__state = -4;
						goto IL_00f5;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
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
		IEnumerator<Control> IEnumerable<Control>.GetEnumerator()
		{
			_003CCandidates_003Ed__115 _003CCandidates_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CCandidates_003Ed__ = this;
			}
			else
			{
				_003CCandidates_003Ed__ = new _003CCandidates_003Ed__115(0);
			}
			_003CCandidates_003Ed__.root = _003C_003E3__root;
			return _003CCandidates_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Control>)this).GetEnumerator();
		}
	}

	internal const string TargetMeta = "mlm_controller_target";

	private const string TranslatedMeta = "mlm_steam_input";

	private const ulong RepeatDelay = 320uL;

	private const ulong ListRepeat = 70uL;

	private const ulong ListFastRepeat = 35uL;

	private const ulong ListFastAfter = 1500uL;

	private const ulong ControlRepeat = 110uL;

	private const ulong PageRepeatDelay = 450uL;

	private const ulong PageRepeat = 180uL;

	private const float WideControl = 240f;

	private const float LeadingAnchor = 48f;

	private const float SameRowTolerance = 12f;

	private readonly SceneTree _tree;

	private readonly Window _window;

	private readonly Control _owner;

	private readonly List<Scope> _scopes = new List<Scope>();

	private readonly Dictionary<Control, GuiInputEventHandler> _controls = new Dictionary<Control, GuiInputEventHandler>();

	private readonly Dictionary<Window, WindowInputEventHandler> _windows = new Dictionary<Window, WindowInputEventHandler>();

	private readonly Dictionary<ulong, bool> _forwardedActions = new Dictionary<ulong, bool>();

	private readonly Dictionary<int, Vector2> _rawSticks = new Dictionary<int, Vector2>();

	private readonly HashSet<(int Device, string Action)> _stickEchoes = new HashSet<(int, string)>();

	private readonly List<(string Action, InputEvent Event)> _removedBindings = new List<(string, InputEvent)>();

	private readonly List<(string Action, InputEvent Event)> _addedBindings = new List<(string, InputEvent)>();

	private readonly HashSet<string> _createdActions = new HashSet<string>();

	private readonly HashSet<JoyButton> _held = new HashSet<JoyButton>();

	private Scope? _scope;

	private int _device = -1;

	private Vector2 _stick;

	private float _scroll;

	private Vector2I _direction;

	private ulong _repeatAt;

	private ulong _directionSince;

	private int _page;

	private ulong _pageAt;

	private ulong _lastFrame;

	private bool _leftTrigger;

	private bool _rightTrigger;

	private bool _dispatching;

	private bool _disposed;

	private bool _nativeInputSeen;

	private bool _nativeConnectionActive;

	private bool _rawInputSeen;

	private bool _pointerUsed;

	private bool _revealPending;

	private bool _suppress;

	private ulong _connectionCheckAt;

	internal static LauncherController? Active { get; private set; }

	internal bool ControllerMode { get; private set; }

	internal Source LastSource { get; private set; }

	internal int LastDevice { get; private set; }

	internal Action? ModeChanged { get; set; }

	internal Action? FocusChanged { get; set; }

	internal Action<string>? InputSourceDetected { get; set; }

	internal Func<bool>? NativeControllerConnected { get; set; }

	internal Func<bool>? RevealFocus { get; set; }

	internal Action<Control>? TextEntryRequested { get; set; }

	internal Scope MainScope => _scopes[0];

	internal Control? ActiveRoot => CurrentScope()?.Root;

	internal LauncherController(Control root, Action back, Func<Control?> initialFocus)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Expected O, but got Unknown
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Expected O, but got Unknown
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Expected O, but got Unknown
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Expected O, but got Unknown
		Active?.Dispose();
		_owner = root;
		_tree = ((Node)root).GetTree();
		_window = ((Node)root).GetWindow();
		try
		{
			InstallPopupBindings();
			AddScope(new Scope(root, back, initialFocus));
			SubscribeWindow(_window);
			_tree.NodeAdded += new NodeAddedEventHandler(OnNodeAdded);
			_tree.NodeRemoved += new NodeRemovedEventHandler(OnNodeRemoved);
			_tree.ProcessFrame += OnFrame;
			((Viewport)_window).GuiFocusChanged += new GuiFocusChangedEventHandler(OnGuiFocusChanged);
			Input.Singleton.JoyConnectionChanged += new JoyConnectionChangedEventHandler(OnConnectionChanged);
			((Node)root).TreeExiting += Dispose;
			Active = this;
		}
		catch
		{
			Dispose();
			throw;
		}
	}

	internal void AddScope(Scope scope)
	{
		scope.ReturnFocus = ((Viewport)_window).GuiGetFocusOwner();
		_scopes.Add(scope);
		SubscribeBranch((Node)(object)scope.Root);
	}

	internal void FocusInCurrentScope(Control control)
	{
		if (!_disposed)
		{
			EnsureFocus();
			if (_scope != null && Within(control, _scope.Root) && Available(control))
			{
				Focus(control);
			}
		}
	}

	internal void ActivateForConnectedController()
	{
		if (!_disposed && !ControllerMode && !_pointerUsed)
		{
			ControllerMode = true;
			ModeChanged?.Invoke();
			RevealFocus?.Invoke();
			_revealPending = false;
			EnsureFocus();
		}
	}

	private static bool Live(Node? node)
	{
		if (node != null && GodotObject.IsInstanceValid((GodotObject)(object)node) && node.IsInsideTree())
		{
			return !((GodotObject)node).IsQueuedForDeletion();
		}
		return false;
	}

	private static bool Within(Control? control, Control root)
	{
		if (Live((Node?)(object)control))
		{
			if (control != root)
			{
				return ((Node)root).IsAncestorOf((Node)(object)control);
			}
			return true;
		}
		return false;
	}

	private static bool Available(Control control)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I8
		bool flag = Live((Node?)(object)control) && ((CanvasItem)control).IsVisibleInTree() && (long)control.FocusMode == 2;
		if (flag)
		{
			BaseButton val = (BaseButton)(object)((control is BaseButton) ? control : null);
			bool flag2;
			if (val == null)
			{
				LineEdit val2 = (LineEdit)(object)((control is LineEdit) ? control : null);
				if (val2 == null)
				{
					TextEdit val3 = (TextEdit)(object)((control is TextEdit) ? control : null);
					if (val3 == null)
					{
						Slider val4 = (Slider)(object)((control is Slider) ? control : null);
						flag2 = ((val4 != null) ? val4.Editable : (!(control is RichTextLabel) || ((GodotObject)control).HasMeta(StringName.op_Implicit("mlm_controller_target"))));
					}
					else
					{
						flag2 = val3.Editable;
					}
				}
				else
				{
					flag2 = val2.Editable;
				}
			}
			else
			{
				flag2 = !val.Disabled;
			}
			flag = flag2;
		}
		return flag;
	}

	private Scope? CurrentScope()
	{
		return _scopes.LastOrDefault((Scope scope) => Live((Node?)(object)scope.Root) && ((CanvasItem)scope.Root).IsVisibleInTree() && (scope.Enabled?.Invoke() ?? true));
	}

	private void SubscribeBranch(Node node)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		Window val = (Window)(object)((node is Window) ? node : null);
		if (val != null)
		{
			SubscribeWindow(val);
		}
		Control control = (Control)(object)((node is Control) ? node : null);
		if (control != null && !_controls.ContainsKey(control))
		{
			GuiInputEventHandler val2 = (GuiInputEventHandler)delegate(InputEvent input)
			{
				OnGuiInput(control, input);
			};
			_controls.Add(control, val2);
			control.GuiInput += val2;
		}
		foreach (Node child in node.GetChildren(true))
		{
			SubscribeBranch(child);
		}
	}

	private void SubscribeWindow(Window window)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		if (!_windows.ContainsKey(window))
		{
			WindowInputEventHandler val = (WindowInputEventHandler)delegate(InputEvent input)
			{
				OnWindowInput(window, input);
			};
			_windows.Add(window, val);
			window.WindowInput += val;
			window.FocusExited += OnWindowFocusExited;
		}
	}

	private void OnNodeAdded(Node node)
	{
		if (_scopes.Any((Scope scope) => Live((Node?)(object)scope.Root) && ((Node)scope.Root).IsAncestorOf(node)))
		{
			SubscribeBranch(node);
		}
	}

	private void OnNodeRemoved(Node node)
	{
		Node obj = node;
		Control val = (Control)(object)((obj is Control) ? obj : null);
		if (val != null && _controls.Remove(val, out GuiInputEventHandler value))
		{
			val.GuiInput -= value;
		}
		Node obj2 = node;
		Window val2 = (Window)(object)((obj2 is Window) ? obj2 : null);
		if (val2 != null && _windows.Remove(val2, out WindowInputEventHandler value2))
		{
			val2.WindowInput -= value2;
			val2.FocusExited -= OnWindowFocusExited;
		}
		_scopes.RemoveAll((Scope scope) => scope.Root == node);
	}

	private void OnGuiFocusChanged(Control control)
	{
		if (!_disposed)
		{
			FocusChanged?.Invoke();
		}
	}

	private bool PopupOpen()
	{
		return _windows.Keys.Any((Window window) => window != _window && Live((Node?)(object)window) && window.Visible && (!window.Unfocusable || !window.MousePassthrough));
	}

	private bool HasWindowFocus()
	{
		if (!_window.HasFocus())
		{
			return _windows.Keys.Any((Window window) => Live((Node?)(object)window) && window.Visible && window.HasFocus());
		}
		return true;
	}

	private void OnWindowInput(Window source, InputEvent input)
	{
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Invalid comparison between Unknown and I8
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (_disposed || _dispatching)
		{
			return;
		}
		InputEventAction val = (InputEventAction)(object)((input is InputEventAction) ? input : null);
		if (val != null)
		{
			if (!HasWindowFocus() || (source == _window && PopupOpen()))
			{
				return;
			}
			ulong instanceId = ((GodotObject)val).GetInstanceId();
			if (_forwardedActions.TryGetValue(instanceId, out var value) && value == val.Pressed)
			{
				return;
			}
			InputEvent val2 = LauncherControllerActions.Translate(val);
			try
			{
				if (val2 != null)
				{
					_forwardedActions[instanceId] = val.Pressed;
					_nativeConnectionActive = true;
					InputEventJoypadButton val3 = (InputEventJoypadButton)(object)((val2 is InputEventJoypadButton) ? val2 : null);
					if (val3 != null && val3.Pressed)
					{
						LastSource = Source.SteamInput;
					}
					if (!_nativeInputSeen)
					{
						_nativeInputSeen = true;
						InputSourceDetected?.Invoke("native Steam Input actions");
					}
					if (!IsStickEcho(val, val2))
					{
						((GodotObject)val2).SetMeta(StringName.op_Implicit("mlm_steam_input"), Variant.op_Implicit(true));
						Input.ParseInputEvent(val2);
					}
				}
				return;
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		bool flag = !((GodotObject)input).HasMeta(StringName.op_Implicit("mlm_steam_input"));
		InputEventJoypadMotion val4 = default(InputEventJoypadMotion);
		bool flag2;
		if (flag)
		{
			val4 = (InputEventJoypadMotion)(object)((input is InputEventJoypadMotion) ? input : null);
			if (val4 != null)
			{
				JoyAxis axis = val4.Axis;
				if ((long)axis <= 1L)
				{
					flag2 = true;
					goto IL_013d;
				}
			}
			flag2 = false;
			goto IL_013d;
		}
		goto IL_0141;
		IL_0141:
		if (flag && HasWindowFocus())
		{
			Vector2 valueOrDefault = _rawSticks.GetValueOrDefault(((InputEvent)val4).Device);
			if ((int)val4.Axis == 0)
			{
				valueOrDefault.X = val4.AxisValue;
			}
			else
			{
				valueOrDefault.Y = val4.AxisValue;
			}
			_rawSticks[((InputEvent)val4).Device] = valueOrDefault;
		}
		if (IsControllerIntent(input) && HasWindowFocus())
		{
			SetControllerMode(active: true);
			goto IL_0220;
		}
		InputEventMouseButton val5 = (InputEventMouseButton)(object)((input is InputEventMouseButton) ? input : null);
		if (val5 != null)
		{
			if (val5.Pressed)
			{
				goto IL_01dc;
			}
		}
		else
		{
			InputEventKey val6 = (InputEventKey)(object)((input is InputEventKey) ? input : null);
			if (val6 != null && val6.Pressed)
			{
				goto IL_01dc;
			}
		}
		flag = false;
		goto IL_01e4;
		IL_013d:
		flag = flag2;
		goto IL_0141;
		IL_01dc:
		flag = true;
		goto IL_01e4;
		IL_0220:
		if (!PopupOpen())
		{
			EnsureFocus();
		}
		return;
		IL_01e4:
		if (flag)
		{
			goto IL_0206;
		}
		InputEventMouseMotion val7 = (InputEventMouseMotion)(object)((input is InputEventMouseMotion) ? input : null);
		if (val7 != null)
		{
			Vector2 relative = val7.Relative;
			if (!((Vector2)(ref relative)).IsZeroApprox())
			{
				goto IL_0206;
			}
		}
		goto IL_0220;
		IL_0206:
		_pointerUsed = true;
		SetControllerMode(active: false);
		ResetHeld();
		ClearStickEchoes();
		goto IL_0220;
	}

	private bool IsStickEcho(InputEventAction action, InputEvent translated)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Invalid comparison between Unknown and I8
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Invalid comparison between Unknown and I8
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		(int, string) item = (((InputEvent)action).Device, ((object)action.Action).ToString());
		if (!action.Pressed)
		{
			return _stickEchoes.Remove(item);
		}
		if (_stickEchoes.Contains(item))
		{
			return true;
		}
		InputEventJoypadButton val = (InputEventJoypadButton)(object)((translated is InputEventJoypadButton) ? translated : null);
		Vector2I val3;
		if (val != null)
		{
			JoyButton buttonIndex = val.ButtonIndex;
			_003F val2 = buttonIndex - 11;
			if ((long)val2 > 3L)
			{
				goto IL_00eb;
			}
			switch ((uint)val2)
			{
			case 0u:
				break;
			case 1u:
				goto IL_00a2;
			case 2u:
				goto IL_00ab;
			case 3u:
				goto IL_00b4;
			default:
				goto IL_00eb;
			}
			val3 = Vector2I.Up;
		}
		else
		{
			InputEventJoypadMotion val4 = (InputEventJoypadMotion)(object)((translated is InputEventJoypadMotion) ? translated : null);
			if (val4 == null)
			{
				goto IL_00eb;
			}
			JoyAxis axis = val4.Axis;
			if ((int)axis != 0)
			{
				if ((long)axis != 1)
				{
					goto IL_00eb;
				}
				InputEventJoypadMotion val5 = val4;
				val3 = new Vector2I(0, Math.Sign(val5.AxisValue));
			}
			else
			{
				val3 = new Vector2I(Math.Sign(val4.AxisValue), 0);
			}
		}
		goto IL_00f2;
		IL_00b4:
		val3 = Vector2I.Right;
		goto IL_00f2;
		IL_00ab:
		val3 = Vector2I.Left;
		goto IL_00f2;
		IL_00a2:
		val3 = Vector2I.Down;
		goto IL_00f2;
		IL_00eb:
		val3 = Vector2I.Zero;
		goto IL_00f2;
		IL_00f2:
		Vector2I val6 = val3;
		if (val6 == Vector2I.Zero || !_rawSticks.TryGetValue(((InputEvent)action).Device, out var value) || Math.Max(Math.Abs(value.X), Math.Abs(value.Y)) < 0.6f)
		{
			return false;
		}
		if (((val6.X != 0) ? (value.X * (float)val6.X) : (value.Y * (float)val6.Y)) >= 0.5f)
		{
			return _stickEchoes.Add(item);
		}
		return false;
	}

	private void ClearStickEchoes()
	{
		_rawSticks.Clear();
		_stickEchoes.Clear();
	}

	private void OnWindowFocusExited()
	{
		ResetHeld();
		ClearStickEchoes();
	}

	private static bool IsControllerIntent(InputEvent input)
	{
		InputEventJoypadButton val = (InputEventJoypadButton)(object)((input is InputEventJoypadButton) ? input : null);
		if (val == null || !val.Pressed)
		{
			InputEventJoypadMotion val2 = (InputEventJoypadMotion)(object)((input is InputEventJoypadMotion) ? input : null);
			if (val2 != null)
			{
				return Math.Abs(val2.AxisValue) >= 0.6f;
			}
			return false;
		}
		return true;
	}

	private void SetControllerMode(bool active)
	{
		if (ControllerMode != active)
		{
			ControllerMode = active;
			_revealPending = active;
			ModeChanged?.Invoke();
		}
	}

	private void EnsureFocus()
	{
		Scope scope = CurrentScope();
		Control val = ((Viewport)_window).GuiGetFocusOwner();
		if (scope != _scope)
		{
			Scope scope2 = _scope;
			_scope = scope;
			ResetHeld();
			if (scope == null)
			{
				return;
			}
			if (Within(scope2?.ReturnFocus, scope.Root) && Available(scope2.ReturnFocus))
			{
				Focus(scope2.ReturnFocus);
			}
			else if (!Within(val, scope.Root))
			{
				if (Live((Node?)(object)val))
				{
					scope.ReturnFocus = val;
				}
				FocusInitial(scope);
			}
		}
		if (_scope != null && ControllerMode && (!Within(((Viewport)_window).GuiGetFocusOwner(), _scope.Root) || !Available(((Viewport)_window).GuiGetFocusOwner())))
		{
			FocusInitial(_scope);
		}
	}

	private void FocusInitial(Scope scope)
	{
		Control val = scope.InitialFocus();
		if (val != null && Within(val, scope.Root) && Available(val))
		{
			Focus(val);
			return;
		}
		Control val2 = Candidates((Node)(object)scope.Root).FirstOrDefault();
		if (val2 != null)
		{
			Focus(val2);
		}
		else
		{
			((Viewport)_window).GuiReleaseFocus();
		}
	}

	[IteratorStateMachine(typeof(_003CCandidates_003Ed__115))]
	private static IEnumerable<Control> Candidates(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCandidates_003Ed__115(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static void Focus(Control control)
	{
		control.GrabFocus();
		for (Node parent = ((Node)control).GetParent(); parent != null; parent = parent.GetParent())
		{
			ScrollContainer val = (ScrollContainer)(object)((parent is ScrollContainer) ? parent : null);
			if (val != null)
			{
				val.EnsureControlVisible(control);
			}
		}
	}

	private void OnGuiInput(Control control, InputEvent input)
	{
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Invalid comparison between Unknown and I8
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Invalid comparison between Unknown and I8
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Invalid comparison between Unknown and I8
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_038c: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Invalid comparison between Unknown and I8
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Invalid comparison between Unknown and I8
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0353: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Invalid comparison between Unknown and I8
		if (_disposed || _dispatching || PopupOpen())
		{
			return;
		}
		InputEventKey val2;
		int num;
		if ((!(input is InputEventJoypadButton) && !(input is InputEventJoypadMotion)) || 1 == 0)
		{
			InputEventKey val = (InputEventKey)(object)((input is InputEventKey) ? input : null);
			if (val != null && val.Pressed && (((InputEvent)val).IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false) || ((InputEvent)val).IsActionPressed(StringName.op_Implicit("ui_focus_next"), false, false) || ((InputEvent)val).IsActionPressed(StringName.op_Implicit("ui_focus_prev"), false, false)))
			{
				EnsureFocus();
				control.AcceptEvent();
				if (((InputEvent)val).IsActionPressed(StringName.op_Implicit("ui_cancel"), false, false))
				{
					_scope?.Back();
				}
				else
				{
					Cycle((!((InputEvent)val).IsActionPressed(StringName.op_Implicit("ui_focus_prev"), false, false)) ? 1 : (-1));
				}
				return;
			}
			val2 = (InputEventKey)(object)((input is InputEventKey) ? input : null);
			if (val2 != null && val2.Pressed && !((InputEventWithModifiers)val2).AltPressed && !((InputEventWithModifiers)val2).CtrlPressed && !((InputEventWithModifiers)val2).MetaPressed)
			{
				Scope scope = CurrentScope();
				if ((object)scope != null)
				{
					num = ((scope != MainScope) ? 1 : 0);
					goto IL_0121;
				}
			}
			num = 0;
			goto IL_0121;
		}
		control.AcceptEvent();
		if (!HasWindowFocus())
		{
			ResetHeld();
			return;
		}
		if (!((GodotObject)input).HasMeta(StringName.op_Implicit("mlm_steam_input")) && IsControllerIntent(input))
		{
			if (!_rawInputSeen)
			{
				_rawInputSeen = true;
				InputSourceDetected?.Invoke("standard joypad events");
			}
			if (input is InputEventJoypadButton)
			{
				LastSource = Source.Joypad;
				LastDevice = input.Device;
			}
		}
		if (IsControllerIntent(input))
		{
			SetControllerMode(active: true);
		}
		EnsureFocus();
		if (_scope == null)
		{
			return;
		}
		if (_device != input.Device)
		{
			if (!IsControllerIntent(input))
			{
				return;
			}
			ResetHeld();
			_device = input.Device;
		}
		bool flag = _revealPending && IsControllerIntent(input);
		bool flag2;
		if (flag)
		{
			InputEventJoypadButton val3 = (InputEventJoypadButton)(object)((input is InputEventJoypadButton) ? input : null);
			if (val3 != null)
			{
				JoyButton buttonIndex = val3.ButtonIndex;
				if ((long)buttonIndex == 4 || (long)buttonIndex == 6)
				{
					flag2 = true;
					goto IL_02c7;
				}
			}
			flag2 = false;
			goto IL_02c7;
		}
		goto IL_02cd;
		IL_02c7:
		flag = !flag2;
		goto IL_02cd;
		IL_018c:
		Vector2I val4 = Vector2I.Left;
		goto IL_01a5;
		IL_0195:
		val4 = Vector2I.Right;
		goto IL_01a5;
		IL_0121:
		flag = (byte)num != 0;
		if (flag)
		{
			flag2 = ((control is LineEdit || control is TextEdit) ? true : false);
			flag = !flag2;
		}
		if (flag)
		{
			Key keycode = val2.Keycode;
			_003F val5 = keycode - 4194319;
			if ((long)val5 > 3L)
			{
				goto IL_019e;
			}
			switch ((uint)val5)
			{
			case 1u:
				break;
			case 3u:
				goto IL_0183;
			case 0u:
				goto IL_018c;
			case 2u:
				goto IL_0195;
			default:
				goto IL_019e;
			}
			val4 = Vector2I.Up;
			goto IL_01a5;
		}
		return;
		IL_0183:
		val4 = Vector2I.Down;
		goto IL_01a5;
		IL_019e:
		val4 = Vector2I.Zero;
		goto IL_01a5;
		IL_02cd:
		_suppress = flag && ConsumeReveal();
		try
		{
			InputEventJoypadButton val6 = (InputEventJoypadButton)(object)((input is InputEventJoypadButton) ? input : null);
			if (val6 != null)
			{
				bool num2 = val6.Pressed && _held.Add(val6.ButtonIndex);
				if (!val6.Pressed)
				{
					_held.Remove(val6.ButtonIndex);
				}
				if (num2 && !_suppress)
				{
					Press(val6.ButtonIndex);
				}
				flag = num2;
				if (flag)
				{
					JoyButton buttonIndex = val6.ButtonIndex;
					flag2 = (((long)buttonIndex == 4 || (long)buttonIndex == 6) ? true : false);
					flag = flag2;
				}
				if (flag)
				{
					_revealPending = false;
				}
			}
			else
			{
				InputEventJoypadMotion val7 = (InputEventJoypadMotion)(object)((input is InputEventJoypadMotion) ? input : null);
				if (val7 != null)
				{
					JoyAxis axis = val7.Axis;
					if ((long)axis <= 5L)
					{
						switch ((uint)axis)
						{
						case 0u:
							_stick.X = val7.AxisValue;
							break;
						case 1u:
							_stick.Y = val7.AxisValue;
							break;
						case 3u:
							_scroll = ((Math.Abs(val7.AxisValue) > 0.25f) ? val7.AxisValue : 0f);
							break;
						case 4u:
							Trigger(val7.AxisValue, ref _leftTrigger, -1);
							break;
						case 5u:
							Trigger(val7.AxisValue, ref _rightTrigger, 1);
							break;
						}
					}
				}
			}
			UpdateDirection();
		}
		finally
		{
			_suppress = false;
		}
		if (Live((Node?)(object)control))
		{
			control.AcceptEvent();
		}
		return;
		IL_01a5:
		Vector2I val8 = val4;
		if (val8 != Vector2I.Zero)
		{
			EnsureFocus();
			Navigate(val8);
			control.AcceptEvent();
		}
	}

	private bool ConsumeReveal()
	{
		_revealPending = false;
		return RevealFocus?.Invoke() ?? false;
	}

	private void Press(JoyButton button)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I8
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Invalid comparison between Unknown and I8
		if (_scope == null)
		{
			return;
		}
		Control val = ((Viewport)_window).GuiGetFocusOwner();
		if ((long)button > 10L)
		{
			return;
		}
		switch ((uint)button)
		{
		case 0u:
		{
			if (val == null)
			{
				break;
			}
			Func<Control, bool>? accept = _scope.Accept;
			if (accept == null || !accept(val))
			{
				if ((val is LineEdit || val is TextEdit) ? true : false)
				{
					BeginTextEntry(val);
				}
				else
				{
					EmitUi("ui_accept");
				}
			}
			break;
		}
		case 1u:
			_scope.Back();
			break;
		case 9u:
		case 10u:
		{
			Func<JoyButton, bool>? shortcut = _scope.Shortcut;
			if (shortcut == null || !shortcut(button))
			{
				Cycle(((long)button != 9) ? 1 : (-1));
			}
			break;
		}
		case 2u:
		case 3u:
		case 4u:
		case 6u:
			_scope.Shortcut?.Invoke(button);
			break;
		case 5u:
		case 7u:
		case 8u:
			break;
		}
	}

	private void BeginTextEntry(Control field)
	{
		LineEdit val = (LineEdit)(object)((field is LineEdit) ? field : null);
		if (val != null)
		{
			if (!val.Editable)
			{
				return;
			}
			val.Edit();
		}
		else
		{
			TextEdit val2 = (TextEdit)(object)((field is TextEdit) ? field : null);
			if (val2 != null && !val2.Editable)
			{
				return;
			}
		}
		TextEntryRequested?.Invoke(field);
	}

	private void Trigger(float value, ref bool held, int delta)
	{
		if (value < 0.4f)
		{
			held = false;
			if (_page == delta)
			{
				_page = 0;
			}
		}
		else if (value >= 0.6f && !held)
		{
			held = true;
			if (!_suppress)
			{
				_page = delta;
				_pageAt = Time.GetTicksMsec() + 450;
				PageInput(delta);
			}
		}
	}

	private void UpdateDirection()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		Vector2I val = default(Vector2I);
		((Vector2I)(ref val))._002Ector((_held.Contains((JoyButton)14) ? 1 : 0) - (_held.Contains((JoyButton)13) ? 1 : 0), (_held.Contains((JoyButton)12) ? 1 : 0) - (_held.Contains((JoyButton)11) ? 1 : 0));
		if (val == Vector2I.Zero)
		{
			float num = ((_direction == Vector2I.Zero) ? 0.6f : 0.4f);
			if (Math.Max(Math.Abs(_stick.X), Math.Abs(_stick.Y)) >= num)
			{
				val = ((Math.Abs(_stick.Y) >= Math.Abs(_stick.X)) ? new Vector2I(0, Math.Sign(_stick.Y)) : new Vector2I(Math.Sign(_stick.X), 0));
			}
		}
		if (val.X != 0 && val.Y != 0)
		{
			val.X = 0;
		}
		if (!(val == _direction))
		{
			_direction = val;
			_directionSince = Time.GetTicksMsec();
			_repeatAt = _directionSince + 320;
			if (val != Vector2I.Zero && !_suppress)
			{
				Navigate(val);
			}
		}
	}

	private void Navigate(Vector2I direction, bool repeat = false)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (_scope == null)
		{
			return;
		}
		Control val = ((Viewport)_window).GuiGetFocusOwner();
		if (val == null || !Within(val, _scope.Root))
		{
			FocusInitial(_scope);
			return;
		}
		Func<Control, Vector2I, bool>? direction2 = _scope.Direction;
		if (direction2 != null && direction2(val, direction))
		{
			return;
		}
		Tree val2 = (Tree)(object)((val is Tree) ? val : null);
		if (val2 != null && direction.Y != 0)
		{
			if (TreeCanStep(val2, direction.Y))
			{
				EmitUi((direction.Y < 0) ? "ui_up" : "ui_down");
				return;
			}
			if (repeat)
			{
				return;
			}
		}
		if (val is Slider && direction.X != 0)
		{
			EmitUi((direction.X < 0) ? "ui_left" : "ui_right");
			return;
		}
		Control val3 = _scope.Redirect?.Invoke(val, direction);
		if (val3 != null && Within(val3, _scope.Root) && Available(val3))
		{
			Focus(val3);
			return;
		}
		Control val4 = FindNeighbor(_scope, val, direction, repeat);
		if (val4 != null)
		{
			Focus(val4);
		}
	}

	private static bool TreeCanStep(Tree tree, int step)
	{
		TreeItem selected = tree.GetSelected();
		if (selected == null)
		{
			return true;
		}
		int num = Math.Max(tree.GetSelectedColumn(), 0);
		for (TreeItem val = ((step > 0) ? selected.GetNextVisible(false) : selected.GetPrevVisible(false)); val != null; val = ((step > 0) ? val.GetNextVisible(false) : val.GetPrevVisible(false)))
		{
			if (val.IsSelectable(num))
			{
				return true;
			}
		}
		return false;
	}

	private static Control? FindNeighbor(Scope scope, Control focused, Vector2I direction, bool repeat)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		Rect2 origin = OriginRect(focused);
		ScrollContainer area = EnclosingScroll(focused, scope.Root);
		if (area != null)
		{
			Control val = Best(origin, direction, from candidate in Candidates((Node)(object)area)
				where candidate != focused
				select candidate);
			if (val != null)
			{
				return val;
			}
			if (ScrollStep(area, direction) || repeat)
			{
				return null;
			}
		}
		return Best(origin, direction, from candidate in Candidates((Node)(object)scope.Root)
			where candidate != focused && (area == null || !((Node)area).IsAncestorOf((Node)(object)candidate)) && OnScreen(candidate, scope.Root)
			select candidate);
	}

	private static Rect2 OriginRect(Control focused)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Rect2 globalRect = focused.GetGlobalRect();
		Tree val = (Tree)(object)((focused is Tree) ? focused : null);
		if (val != null)
		{
			TreeItem selected = val.GetSelected();
			if (selected != null)
			{
				Rect2 val2 = ((CanvasItem)val).GetGlobalTransform() * val.GetItemAreaRect(selected, -1, -1);
				Rect2 result = ((Rect2)(ref val2)).Intersection(globalRect);
				if (((Rect2)(ref result)).HasArea())
				{
					return result;
				}
			}
		}
		return globalRect;
	}

	private static Control? Best(Rect2 origin, Vector2I direction, IEnumerable<Control> candidates)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = direction.Y != 0;
		float num = (flag ? direction.Y : direction.X);
		float num2 = (flag ? ((Rect2)(ref origin)).Position.X : ((Rect2)(ref origin)).Position.Y);
		float num3 = (flag ? ((Rect2)(ref origin)).End.X : ((Rect2)(ref origin)).End.Y);
		float num4 = ((num3 - num2 <= 240f) ? ((num2 + num3) / 2f) : (num2 + 48f));
		float num5 = (flag ? 2f : 30f);
		List<(Control, float, float)> list = new List<(Control, float, float)>();
		foreach (Control candidate in candidates)
		{
			Rect2 globalRect = candidate.GetGlobalRect();
			float num6 = (flag ? (((Rect2)(ref globalRect)).GetCenter().Y - ((Rect2)(ref origin)).GetCenter().Y) : (((Rect2)(ref globalRect)).GetCenter().X - ((Rect2)(ref origin)).GetCenter().X)) * num;
			float num7 = ((!flag) ? ((num > 0f) ? (((Rect2)(ref globalRect)).End.X - ((Rect2)(ref origin)).End.X) : (((Rect2)(ref origin)).Position.X - ((Rect2)(ref globalRect)).Position.X)) : ((num > 0f) ? (((Rect2)(ref globalRect)).End.Y - ((Rect2)(ref origin)).End.Y) : (((Rect2)(ref origin)).Position.Y - ((Rect2)(ref globalRect)).Position.Y)));
			if (!(num6 <= 0.5f) && !(num7 <= 0.5f))
			{
				float val = ((!flag) ? ((num > 0f) ? (((Rect2)(ref globalRect)).Position.X - ((Rect2)(ref origin)).End.X) : (((Rect2)(ref origin)).Position.X - ((Rect2)(ref globalRect)).End.X)) : ((num > 0f) ? (((Rect2)(ref globalRect)).Position.Y - ((Rect2)(ref origin)).End.Y) : (((Rect2)(ref origin)).Position.Y - ((Rect2)(ref globalRect)).End.Y)));
				float num8 = (flag ? ((Rect2)(ref globalRect)).Position.X : ((Rect2)(ref globalRect)).Position.Y);
				float num9 = (flag ? ((Rect2)(ref globalRect)).End.X : ((Rect2)(ref globalRect)).End.Y);
				float num10 = Math.Max(0f, Math.Max(num8 - num3, num2 - num9));
				list.Add((candidate, Math.Max(0f, val) + num10 * num5, Math.Abs(Math.Clamp(num4, num8, num9) - num4)));
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		float best = list.Min<(Control, float, float)>(((Control Control, float Score, float Alignment) item) => item.Score);
		return (from item in list
			where item.Score <= best + 12f
			orderby item.Alignment, item.Score
			select item).First().Item1;
	}

	private static ScrollContainer? EnclosingScroll(Control control, Control root)
	{
		Node parent = ((Node)control).GetParent();
		while (parent != null && parent != root)
		{
			ScrollContainer val = (ScrollContainer)(object)((parent is ScrollContainer) ? parent : null);
			if (val != null)
			{
				return val;
			}
			parent = parent.GetParent();
		}
		return null;
	}

	private static bool OnScreen(Control control, Control root)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Rect2 globalRect = control.GetGlobalRect();
		if (!((Rect2)(ref globalRect)).HasArea())
		{
			return false;
		}
		for (Node parent = ((Node)control).GetParent(); parent != null; parent = parent.GetParent())
		{
			ScrollContainer val = (ScrollContainer)(object)((parent is ScrollContainer) ? parent : null);
			if (val != null)
			{
				Rect2 globalRect2 = ((Control)val).GetGlobalRect();
				if (!((Rect2)(ref globalRect2)).Intersects(globalRect, false))
				{
					return false;
				}
			}
			if (parent == root)
			{
				break;
			}
		}
		return true;
	}

	internal static Control? FirstOnScreen(Control area)
	{
		return (from candidate in Candidates((Node)(object)area)
			where OnScreen(candidate, area)
			select candidate).OrderBy(delegate(Control candidate)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Rect2 globalRect = candidate.GetGlobalRect();
			return Mathf.Round(((Rect2)(ref globalRect)).Position.Y);
		}).ThenBy(delegate(Control candidate)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			Rect2 globalRect2 = candidate.GetGlobalRect();
			return ((Rect2)(ref globalRect2)).Position.X;
		}).FirstOrDefault();
	}

	private static bool ScrollStep(ScrollContainer area, Vector2I direction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (direction.Y == 0)
		{
			return false;
		}
		VScrollBar vScrollBar = area.GetVScrollBar();
		double value = ((Range)vScrollBar).Value;
		((Range)vScrollBar).Value = ((Range)vScrollBar).Value + (double)((float)direction.Y * Math.Max(40f, ((Control)area).Size.Y * 0.4f));
		return !Mathf.IsEqualApprox(((Range)vScrollBar).Value, value);
	}

	private void PageInput(int delta)
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (_scope == null)
		{
			return;
		}
		Control focused = ((Viewport)_window).GuiGetFocusOwner();
		if (focused != null && Within(focused, _scope.Root))
		{
			Func<Control, int, bool>? page = _scope.Page;
			if (page != null && page(focused, delta))
			{
				return;
			}
		}
		ScrollContainer area = ((focused == null) ? null : EnclosingScroll(focused, _scope.Root));
		if (area == null)
		{
			area = _scope.ScrollTarget?.Invoke();
		}
		if (!Live((Node?)(object)area) || !((CanvasItem)area).IsVisibleInTree())
		{
			return;
		}
		VScrollBar vScrollBar = area.GetVScrollBar();
		((Range)vScrollBar).Value = ((Range)vScrollBar).Value + (double)((float)delta * Math.Max(80f, ((Control)area).Size.Y * 0.85f));
		if (focused == null || !((Node)area).IsAncestorOf((Node)(object)focused))
		{
			return;
		}
		Control root = _scope.Root;
		Callable val = Callable.From((Action)delegate
		{
			if (!_disposed && Live((Node?)(object)focused) && Live((Node?)(object)area) && focused.HasFocus() && !OnScreen(focused, root))
			{
				Control? obj = FirstOnScreen((Control)(object)area);
				if (obj != null)
				{
					obj.GrabFocus();
				}
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private void Cycle(int delta)
	{
		if (!(_scope == null))
		{
			List<Control> list = Candidates((Node)(object)_scope.Root).ToList();
			if (list.Count != 0)
			{
				int num = list.IndexOf(((Viewport)_window).GuiGetFocusOwner());
				Focus(list[(num + delta + list.Count) % list.Count]);
			}
		}
	}

	private void EmitUi(string action)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Expected O, but got Unknown
		_dispatching = true;
		try
		{
			((Viewport)_window).PushInput((InputEvent)new InputEventAction
			{
				Action = StringName.op_Implicit(action),
				Pressed = true
			}, false);
			((Viewport)_window).PushInput((InputEvent)new InputEventAction
			{
				Action = StringName.op_Implicit(action),
				Pressed = false
			}, false);
		}
		finally
		{
			_dispatching = false;
		}
	}

	private void OnFrame()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		_forwardedActions.Clear();
		ulong ticksMsec = Time.GetTicksMsec();
		float num = Math.Min((float)(ticksMsec - _lastFrame) / 1000f, 0.05f);
		_lastFrame = ticksMsec;
		if (_nativeConnectionActive && NativeControllerConnected != null && ticksMsec >= _connectionCheckAt)
		{
			_connectionCheckAt = ticksMsec + 1000;
			if (!NativeControllerConnected())
			{
				_nativeConnectionActive = false;
				ResetHeld();
				ClearStickEchoes();
			}
		}
		if (!HasWindowFocus())
		{
			ResetHeld();
			ClearStickEchoes();
			return;
		}
		if (PopupOpen())
		{
			ResetHeld();
			return;
		}
		EnsureFocus();
		if (_direction != Vector2I.Zero && ticksMsec >= _repeatAt)
		{
			bool flag = ((Viewport)_window).GuiGetFocusOwner() is Tree;
			_repeatAt = ticksMsec + (ulong)((!flag) ? 110 : ((ticksMsec - _directionSince >= 1500) ? 35 : 70));
			Navigate(_direction, repeat: true);
		}
		if (_page != 0 && ticksMsec >= _pageAt)
		{
			_pageAt = ticksMsec + 180;
			PageInput(_page);
		}
		if (_scroll == 0f || !(_scope != null))
		{
			return;
		}
		ScrollContainer val = _scope.ScrollTarget?.Invoke();
		if (val == null)
		{
			Node val2 = (Node)(object)((Viewport)_window).GuiGetFocusOwner();
			while (val2 != null && val2 != _scope.Root)
			{
				ScrollContainer val3 = (ScrollContainer)(object)((val2 is ScrollContainer) ? val2 : null);
				if (val3 != null)
				{
					val = val3;
					break;
				}
				val2 = val2.GetParent();
			}
		}
		if (Live((Node?)(object)val) && ((CanvasItem)val).IsVisibleInTree())
		{
			VScrollBar vScrollBar = val.GetVScrollBar();
			((Range)vScrollBar).Value = ((Range)vScrollBar).Value + (double)(_scroll * 650f * num);
		}
	}

	private void OnConnectionChanged(long device, bool connected)
	{
		if (!connected)
		{
			_rawSticks.Remove((int)device);
			_stickEchoes.RemoveWhere(((int Device, string Action) key) => key.Device == device);
			if (device == _device)
			{
				ResetHeld();
			}
		}
	}

	private void ResetHeld()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		_held.Clear();
		_stick = Vector2.Zero;
		_direction = Vector2I.Zero;
		_scroll = 0f;
		_page = 0;
		_leftTrigger = (_rightTrigger = false);
		_device = -1;
	}

	private void InstallPopupBindings()
	{
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected O, but got Unknown
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Expected O, but got Unknown
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Expected O, but got Unknown
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Expected O, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Expected O, but got Unknown
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Expected O, but got Unknown
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Expected O, but got Unknown
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Expected O, but got Unknown
		string[] array = new string[6] { "ui_accept", "ui_cancel", "ui_up", "ui_down", "ui_left", "ui_right" };
		foreach (string text in array)
		{
			if (!InputMap.HasAction(StringName.op_Implicit(text)))
			{
				InputMap.AddAction(StringName.op_Implicit(text), 0.2f);
				_createdActions.Add(text);
			}
			foreach (InputEvent item in InputMap.ActionGetEvents(StringName.op_Implicit(text)))
			{
				if ((item is InputEventJoypadButton || item is InputEventJoypadMotion) ? true : false)
				{
					_removedBindings.Add((text, item));
					InputMap.ActionEraseEvent(StringName.op_Implicit(text), item);
				}
			}
		}
		Bind("ui_accept", (InputEvent)new InputEventJoypadButton
		{
			ButtonIndex = (JoyButton)0,
			Device = -1
		});
		Bind("ui_cancel", (InputEvent)new InputEventJoypadButton
		{
			ButtonIndex = (JoyButton)1,
			Device = -1
		});
		Bind("ui_up", (InputEvent)new InputEventJoypadButton
		{
			ButtonIndex = (JoyButton)11,
			Device = -1
		});
		Bind("ui_down", (InputEvent)new InputEventJoypadButton
		{
			ButtonIndex = (JoyButton)12,
			Device = -1
		});
		Bind("ui_left", (InputEvent)new InputEventJoypadButton
		{
			ButtonIndex = (JoyButton)13,
			Device = -1
		});
		Bind("ui_right", (InputEvent)new InputEventJoypadButton
		{
			ButtonIndex = (JoyButton)14,
			Device = -1
		});
		Bind("ui_up", (InputEvent)new InputEventJoypadMotion
		{
			Axis = (JoyAxis)1,
			AxisValue = -1f,
			Device = -1
		});
		Bind("ui_down", (InputEvent)new InputEventJoypadMotion
		{
			Axis = (JoyAxis)1,
			AxisValue = 1f,
			Device = -1
		});
		Bind("ui_left", (InputEvent)new InputEventJoypadMotion
		{
			Axis = (JoyAxis)0,
			AxisValue = -1f,
			Device = -1
		});
		Bind("ui_right", (InputEvent)new InputEventJoypadMotion
		{
			Axis = (JoyAxis)0,
			AxisValue = 1f,
			Device = -1
		});
	}

	private void Bind(string action, InputEvent input)
	{
		InputMap.ActionAddEvent(StringName.op_Implicit(action), input);
		_addedBindings.Add((action, input));
	}

	internal void RestorePopupBindings()
	{
		foreach (string item in _addedBindings.Select<(string, InputEvent), string>(((string Action, InputEvent Event) binding) => binding.Action).Distinct())
		{
			if (InputMap.HasAction(StringName.op_Implicit(item)))
			{
				Input.ActionRelease(StringName.op_Implicit(item));
			}
		}
		foreach (var (text, val) in _addedBindings)
		{
			if (InputMap.HasAction(StringName.op_Implicit(text)))
			{
				InputMap.ActionEraseEvent(StringName.op_Implicit(text), val);
			}
		}
		foreach (var (text2, val2) in _removedBindings)
		{
			if (InputMap.HasAction(StringName.op_Implicit(text2)) && !InputMap.ActionHasEvent(StringName.op_Implicit(text2), val2))
			{
				InputMap.ActionAddEvent(StringName.op_Implicit(text2), val2);
			}
		}
		foreach (string createdAction in _createdActions)
		{
			InputMap.EraseAction(StringName.op_Implicit(createdAction));
		}
		_addedBindings.Clear();
		_removedBindings.Clear();
		_createdActions.Clear();
		ResetHeld();
		ClearStickEchoes();
	}

	public void Dispose()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Expected O, but got Unknown
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Expected O, but got Unknown
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		_tree.NodeAdded -= new NodeAddedEventHandler(OnNodeAdded);
		_tree.NodeRemoved -= new NodeRemovedEventHandler(OnNodeRemoved);
		_tree.ProcessFrame -= OnFrame;
		if (GodotObject.IsInstanceValid((GodotObject)(object)_window))
		{
			((Viewport)_window).GuiFocusChanged -= new GuiFocusChangedEventHandler(OnGuiFocusChanged);
		}
		Input.Singleton.JoyConnectionChanged -= new JoyConnectionChangedEventHandler(OnConnectionChanged);
		if (GodotObject.IsInstanceValid((GodotObject)(object)_owner))
		{
			((Node)_owner).TreeExiting -= Dispose;
		}
		foreach (var (val3, val4) in _controls)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val3))
			{
				val3.GuiInput -= val4;
			}
		}
		foreach (var (val7, val8) in _windows)
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)val7))
			{
				val7.WindowInput -= val8;
				val7.FocusExited -= OnWindowFocusExited;
			}
		}
		RestorePopupBindings();
		_controls.Clear();
		_windows.Clear();
		_scopes.Clear();
		ResetHeld();
		ModeChanged = null;
		FocusChanged = null;
		InputSourceDetected = null;
		NativeControllerConnected = null;
		RevealFocus = null;
		TextEntryRequested = null;
		if (Active == this)
		{
			Active = null;
		}
	}
}

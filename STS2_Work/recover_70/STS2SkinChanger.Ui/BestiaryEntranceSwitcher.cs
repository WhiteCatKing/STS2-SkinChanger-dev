using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Bestiary;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class BestiaryEntranceSwitcher
{
	private sealed class EntranceState
	{
		public bool UseAncient { get; set; }

		public List<LabelSnapshot> Labels { get; set; } = new List<LabelSnapshot>();

		public Control? ToggleHost { get; set; }

		public NCompendiumBottomButton? Toggle { get; set; }
	}

	private sealed record LabelSnapshot(Control Node, string OriginalText);

	[CompilerGenerated]
	private sealed class _003CEnumerateDescendants_003Ed__19 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
		public _003CEnumerateDescendants_003Ed__19(int _003C_003E1__state)
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
					_003C_003E7__wrap3 = EnumerateDescendants(_003Cchild_003E5__3).GetEnumerator();
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
			_003CEnumerateDescendants_003Ed__19 _003CEnumerateDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateDescendants_003Ed__ = this;
			}
			else
			{
				_003CEnumerateDescendants_003Ed__ = new _003CEnumerateDescendants_003Ed__19(0);
			}
			_003CEnumerateDescendants_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string ToggleName = "STS2AncientCompendiumToggle";

	private const string ToggleHostName = "STS2AncientCompendiumToggleHost";

	private static readonly ConditionalWeakTable<NClickableControl, EntranceState> States = new ConditionalWeakTable<NClickableControl, EntranceState>();

	private static NClickableControl? _lastReleased;

	private static int _scanAttempts;

	private static bool _scanScheduled;

	internal static void ScanCompendium(NCompendiumSubmenu compendium)
	{
		if (!GodotObject.IsInstanceValid((GodotObject)(object)compendium))
		{
			return;
		}
		try
		{
			foreach (NClickableControl item in EnumerateDescendants((Node)(object)compendium).OfType<NClickableControl>())
			{
				if (((object)((Node)item).Name).ToString().Equals("STS2AncientCompendiumToggle", StringComparison.Ordinal))
				{
					continue;
				}
				try
				{
					if (IsBestiaryEntrance(item))
					{
						EnsureAttached(item);
					}
				}
				catch (Exception ex)
				{
					ModLog.Warn($"跳过无法检查的图鉴入口 {((Node)item).Name}：" + ex.GetBaseException().Message);
				}
			}
		}
		catch (Exception ex2)
		{
			ModLog.Warn("检查怪物图鉴入口失败，已保留原界面：" + ex2.GetBaseException().Message);
		}
		ScheduleScan(compendium);
	}

	internal static void RememberReleased(NClickableControl control)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_lastReleased = control;
		Callable val = Callable.From((Action)delegate
		{
			if (_lastReleased == control)
			{
				_lastReleased = null;
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	internal static bool TryRedirectBestiaryOpen(NSubmenuStack stack, NSubmenu screen)
	{
		if (screen is NBestiary)
		{
			NClickableControl lastReleased = _lastReleased;
			if (lastReleased != null && GodotObject.IsInstanceValid((GodotObject)(object)lastReleased))
			{
				_lastReleased = null;
				if (!States.TryGetValue(lastReleased, out EntranceState value))
				{
					EnsureAttached(lastReleased);
					return false;
				}
				if (!value.UseAncient)
				{
					return false;
				}
				AncientCompendiumEntry.OpenFromStack(stack);
				return true;
			}
		}
		return false;
	}

	private static void ScheduleScan(NCompendiumSubmenu compendium)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (_scanScheduled || _scanAttempts >= 16)
		{
			return;
		}
		_scanScheduled = true;
		_scanAttempts++;
		Callable val = Callable.From((Action)delegate
		{
			_scanScheduled = false;
			if (GodotObject.IsInstanceValid((GodotObject)(object)compendium))
			{
				ScanCompendium(compendium);
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private static bool IsBestiaryEntrance(NClickableControl control)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (((object)((Node)control).Name).ToString().Contains("Bestiary", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		Array<Dictionary> signalConnectionList = ((GodotObject)control).GetSignalConnectionList(SignalName.Released);
		if (signalConnectionList == null)
		{
			return false;
		}
		Variant val = default(Variant);
		foreach (Dictionary item in signalConnectionList)
		{
			if (item != null && item.TryGetValue(Variant.op_Implicit("callable"), ref val))
			{
				Callable val2 = ((Variant)(ref val)).As<Callable>();
				string text = ((object)((Callable)(ref val2)).Method)?.ToString() ?? string.Empty;
				if (text.Contains("Bestiary", StringComparison.OrdinalIgnoreCase) || text.Equals("OpenBestiary", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				Delegate @delegate;
				try
				{
					@delegate = ((Callable)(ref val2)).Delegate;
				}
				catch
				{
					@delegate = null;
				}
				if ((object)@delegate != null && @delegate.Method.Name.Contains("Bestiary", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static void EnsureAttached(NClickableControl control)
	{
		if (GodotObject.IsInstanceValid((GodotObject)(object)control))
		{
			EntranceState orCreateValue = States.GetOrCreateValue(control);
			if (orCreateValue.Labels.Count == 0)
			{
				orCreateValue.Labels = CaptureLabels(control);
			}
			NCompendiumBottomButton val = (GodotObject.IsInstanceValid((GodotObject)(object)orCreateValue.Toggle) ? orCreateValue.Toggle : EnumerateDescendants((Node)(object)control).OfType<NCompendiumBottomButton>().FirstOrDefault((Func<NCompendiumBottomButton, bool>)((NCompendiumBottomButton candidate) => ((object)((Node)candidate).Name).ToString().Equals("STS2AncientCompendiumToggle", StringComparison.Ordinal))));
			if (!GodotObject.IsInstanceValid((GodotObject)(object)val))
			{
				val = CreateToggle(control, orCreateValue);
				Control val2 = CreateToggleHost();
				((Node)control).AddChild((Node)(object)val2, false, (InternalMode)0);
				((Node)val2).AddChild((Node)(object)val, false, (InternalMode)0);
				orCreateValue.ToggleHost = val2;
			}
			orCreateValue.Toggle = val;
			EntranceState entranceState = orCreateValue;
			if (entranceState.ToggleHost == null)
			{
				Control val4 = (entranceState.ToggleHost = (Control?)/*isinst with value type is only supported in some contexts*/);
			}
			PositionToggle(control, val, orCreateValue.ToggleHost);
			ApplyMode(control, orCreateValue);
		}
	}

	private static Control CreateToggleHost()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		return new Control
		{
			Name = StringName.op_Implicit("STS2AncientCompendiumToggleHost"),
			MouseFilter = (MouseFilterEnum)2,
			ZIndex = 40
		};
	}

	private static NCompendiumBottomButton CreateToggle(NClickableControl target, EntranceState state)
	{
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		string scenePath = SceneHelper.GetScenePath("screens/main_menu/compendium_bottom_button");
		NCompendiumBottomButton obj = PreloadManager.Cache.GetScene(scenePath).Instantiate<NCompendiumBottomButton>((GenEditState)0);
		((Node)obj).Name = StringName.op_Implicit("STS2AncientCompendiumToggle");
		((Control)obj).FocusMode = (FocusModeEnum)2;
		((Control)obj).MouseFilter = (MouseFilterEnum)0;
		((CanvasItem)obj).ZIndex = 1;
		TextureRect nodeOrNull = ((Node)obj).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("Icon"));
		if (nodeOrNull != null)
		{
			((CanvasItem)nodeOrNull).Visible = false;
		}
		((GodotObject)obj).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
		{
			state.UseAncient = !state.UseAncient;
			ApplyMode(target, state);
		}), 0u);
		return obj;
	}

	private static void PositionToggle(NClickableControl target, NCompendiumBottomButton toggle, Control? host)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (host != null)
		{
			((Control)toggle).SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
			((Control)toggle).Position = Vector2.Zero;
			if (((Control)toggle).Size.X <= 1f || ((Control)toggle).Size.Y <= 1f)
			{
				((Control)toggle).Size = new Vector2(250f, 100f);
			}
			Vector2 val = default(Vector2);
			((Vector2)(ref val))._002Ector(0.42f, 0.42f);
			host.SetAnchorsAndOffsetsPreset((LayoutPreset)0, (LayoutPresetMode)0, 0);
			host.Size = ((Control)toggle).Size;
			host.Scale = val;
			Vector2 val2 = host.Size * val;
			host.Position = new Vector2(Math.Max(((Control)target).Size.X - val2.X - 24f, 0f), 24f);
		}
	}

	private static void ApplyMode(NClickableControl target, EntranceState state)
	{
		foreach (LabelSnapshot label in state.Labels)
		{
			SetText(label.Node, state.UseAncient ? ModLocalization.Get(ModText.OtherCompendium) : label.OriginalText);
		}
		NCompendiumBottomButton? toggle = state.Toggle;
		MegaLabel val = ((toggle != null) ? ((Node)toggle).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("Label")) : null);
		if (val != null)
		{
			val.AutoSizeEnabled = false;
			((Control)val).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
			((Label)val).HorizontalAlignment = (HorizontalAlignment)1;
			((Label)val).VerticalAlignment = (VerticalAlignment)1;
			((Control)val).AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), 54);
			((Label)val).Text = "切换";
		}
	}

	private static List<LabelSnapshot> CaptureLabels(NClickableControl target)
	{
		List<Control> list = new List<Control>();
		NShortSubmenuButton val = (NShortSubmenuButton)(object)((target is NShortSubmenuButton) ? target : null);
		if (val != null)
		{
			MegaLabel nodeOrNull = ((Node)val).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("%Title"));
			if (nodeOrNull != null)
			{
				list.Add((Control)(object)nodeOrNull);
			}
		}
		NCompendiumBottomButton val2 = (NCompendiumBottomButton)(object)((target is NCompendiumBottomButton) ? target : null);
		if (val2 != null)
		{
			MegaLabel nodeOrNull2 = ((Node)val2).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("Label"));
			if (nodeOrNull2 != null)
			{
				list.Add((Control)(object)nodeOrNull2);
			}
		}
		if (list.Count == 0)
		{
			list.AddRange((from control in EnumerateDescendants((Node)(object)target).OfType<Control>()
				where (control is Label || control is MegaRichTextLabel) ? true : false
				orderby ((object)((Node)control).Name).ToString().Contains("title", StringComparison.OrdinalIgnoreCase) || ((object)((Node)control).Name).ToString().Equals("label", StringComparison.OrdinalIgnoreCase) descending
				select control).Take(1));
		}
		return (from control in list
			select new LabelSnapshot(control, ReadText(control)) into snapshot
			where !string.IsNullOrWhiteSpace(snapshot.OriginalText)
			select snapshot).ToList();
	}

	private static string ReadText(Control control)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((GodotObject)control).Get(StringName.op_Implicit("text"));
		return ((Variant)(ref val)).AsString();
	}

	private static void SetText(Control control, string text)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		MegaLabel val = (MegaLabel)(object)((control is MegaLabel) ? control : null);
		if (val != null)
		{
			val.SetTextAutoSize(text);
		}
		else
		{
			((GodotObject)control).Set(StringName.op_Implicit("text"), Variant.op_Implicit(text));
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateDescendants_003Ed__19))]
	private static IEnumerable<Node> EnumerateDescendants(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateDescendants_003Ed__19(-2)
		{
			_003C_003E3__root = root
		};
	}
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.Settings;
using MegaCrit.Sts2.addons.mega_text;

namespace ModLaunchManager;

internal static class InGameManager
{
	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__4 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private Node _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node node;

		public Node _003C_003E3__node;

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
		public _003CDescendants_003Ed__4(int _003C_003E1__state)
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
					_003C_003E2__current = node;
					_003C_003E1__state = 1;
					return true;
				case 1:
					_003C_003E1__state = -1;
					_003C_003E7__wrap1 = node.GetChildren(false).GetEnumerator();
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
						_003C_003E7__wrap2 = Descendants(current).GetEnumerator();
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
			_003CDescendants_003Ed__4 _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__4(0);
			}
			_003CDescendants_003Ed__.node = _003C_003E3__node;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private const string RowName = "ModLaunchManagerReturn";

	internal static bool Install(Harmony harmony)
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Expected O, but got Unknown
		try
		{
			ManagerReturn.Install(harmony);
			(string, string)[] array = new(string, string)[4]
			{
				("CommonUi.NControllerManager", "_Input"),
				("CommonUi.NHotkeyManager", "_UnhandledInput"),
				("CommonUi.NInputManager", "_UnhandledInput"),
				("CommonUi.NInputManager", "_UnhandledKeyInput")
			};
			for (int i = 0; i < array.Length; i++)
			{
				(string, string) tuple = array[i];
				string item = tuple.Item1;
				string item2 = tuple.Item2;
				Type type = typeof(NSettingsScreen).Assembly.GetType("MegaCrit.Sts2.Core.Nodes." + item);
				MethodInfo methodInfo = ((type == null) ? null : AccessTools.Method(type, item2, (Type[])null, (Type[])null));
				if (methodInfo == null)
				{
					throw new MissingMethodException(item, item2);
				}
				harmony.Patch((MethodBase)methodInfo, new HarmonyMethod(typeof(InGameManager), "AllowGameInput", (Type[])null), (HarmonyMethod)null, (HarmonyMethod)null, (HarmonyMethod)null);
			}
			harmony.Patch((MethodBase)AccessTools.Method(typeof(NSettingsScreen), "_Ready", (Type[])null, (Type[])null), (HarmonyMethod)null, new HarmonyMethod(typeof(InGameManager), "AddEntry", (Type[])null)
			{
				priority = 0
			}, (HarmonyMethod)null, (HarmonyMethod)null);
			return true;
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} in-game manager entry unavailable: {value}");
			return false;
		}
	}

	private static bool AllowGameInput()
	{
		return !ManagerReturn.IsReturning;
	}

	private static void AddEntry(NSettingsScreen __instance)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Expected O, but got Unknown
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Expected O, but got Unknown
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			NSettingsPanel panel = ((Node)__instance).GetNodeOrNull<NSettingsPanel>(NodePath.op_Implicit("%GeneralSettings"));
			NSettingsPanel obj = panel;
			VBoxContainer val = ((obj != null) ? obj.Content : null);
			if (val == null || ((Node)val).GetNodeOrNull(NodePath.op_Implicit("ModLaunchManagerReturn")) != null)
			{
				return;
			}
			Control nodeOrNull = ((Node)val).GetNodeOrNull<Control>(NodePath.op_Implicit("SendFeedback"));
			if (nodeOrNull == null)
			{
				return;
			}
			Control val2 = (Control)((Node)nodeOrNull).Duplicate(6);
			((Node)val2).Name = StringName.op_Implicit("ModLaunchManagerReturn");
			((CanvasItem)val2).Visible = true;
			foreach (Node item in Descendants((Node)(object)val2))
			{
				item.UniqueNameInOwner = false;
			}
			NOpenFeedbackScreenButton button = Descendants((Node)(object)val2).OfType<NOpenFeedbackScreenButton>().Single();
			((Node)button).Name = StringName.op_Implicit("ReturnToManager");
			((GodotObject)button).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)delegate
			{
				ManagerReturn.Begin((Control)(object)button);
			}), 0u);
			Control nodeOrNull2 = ((Node)val).GetNodeOrNull<Control>(NodePath.op_Implicit("SendFeedbackDivider"));
			if (nodeOrNull2 != null)
			{
				Control val3 = (Control)((Node)nodeOrNull2).Duplicate(0);
				((Node)val3).Name = StringName.op_Implicit("ModLaunchManagerReturnDivider");
				((Node)val3).UniqueNameInOwner = false;
				((CanvasItem)val3).Visible = true;
				((Node)val).AddChild((Node)(object)val3, false, (InternalMode)0);
			}
			((Node)val).AddChild((Node)(object)val2, false, (InternalMode)0);
			((Node)val2).GetNode<MegaRichTextLabel>(NodePath.op_Implicit("Label")).Text = MlmLocManager.Get("runtime.entry.label");
			((Node)button).GetNode<MegaLabel>(NodePath.op_Implicit("Label")).SetTextAutoSize(MlmLocManager.Get("runtime.entry.button"));
			((NClickableControl)button).Enable();
			MethodInfo updateNavigation = AccessTools.Method(typeof(NSettingsPanel), "UpdateNavigation", Type.EmptyTypes, (Type[])null);
			if ((object)updateNavigation != null)
			{
				((GodotObject)button).Connect(SignalName.VisibilityChanged, Callable.From((Action)delegate
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)panel))
					{
						updateNavigation.Invoke(panel, null);
					}
				}), 0u);
				updateNavigation.Invoke(panel, null);
			}
			else
			{
				List<Control> list = new List<Control>();
				AccessTools.Method(typeof(NSettingsPanel), "GetSettingsOptionsRecursive", new Type[2]
				{
					typeof(Control),
					typeof(List<Control>)
				}, (Type[])null).Invoke(panel, new object[2] { val, list });
				for (int i = 0; i < list.Count; i++)
				{
					Control obj2 = list[i];
					NodePath focusNeighborLeft = (obj2.FocusNeighborRight = ((Node)obj2).GetPath());
					obj2.FocusNeighborLeft = focusNeighborLeft;
					obj2.FocusNeighborTop = ((Node)list[Math.Max(0, i - 1)]).GetPath();
					obj2.FocusNeighborBottom = ((Node)list[Math.Min(list.Count - 1, i + 1)]).GetPath();
				}
			}
			Callable val4 = Callable.From((Action)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)panel))
				{
					AccessTools.Method(typeof(NSettingsPanel), "RefreshSize", (Type[])null, (Type[])null).Invoke(panel, null);
				}
			});
			((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
		}
		catch (Exception value)
		{
			GD.PushWarning($"{"[ModLaunchManager]"} could not add settings entry: {value}");
		}
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__4))]
	private static IEnumerable<Node> Descendants(Node node)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__4(-2)
		{
			_003C_003E3__node = node
		};
	}
}

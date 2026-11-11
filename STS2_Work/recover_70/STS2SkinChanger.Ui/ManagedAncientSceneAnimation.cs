using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Helpers;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class ManagedAncientSceneAnimation
{
	[CompilerGenerated]
	private sealed class _003CDescendantsAndSelf_003Ed__10 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
		public _003CDescendantsAndSelf_003Ed__10(int _003C_003E1__state)
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
						_003C_003E7__wrap2 = DescendantsAndSelf(current).GetEnumerator();
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
			_003CDescendantsAndSelf_003Ed__10 _003CDescendantsAndSelf_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendantsAndSelf_003Ed__ = this;
			}
			else
			{
				_003CDescendantsAndSelf_003Ed__ = new _003CDescendantsAndSelf_003Ed__10(0);
			}
			_003CDescendantsAndSelf_003Ed__.root = _003C_003E3__root;
			return _003CDescendantsAndSelf_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	public static void TryStart(string? groupId, Node sceneRoot)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		if (string.IsNullOrWhiteSpace(groupId) || (!SkinService.IsManagedResourceOptionSelected(groupId) && SkinService.GetSelectedFullRuntimeProvider(groupId) == null))
		{
			return;
		}
		Node val = FindSpineNode(sceneRoot);
		if (val == null)
		{
			return;
		}
		try
		{
			MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val));
			SpineNodeExtensions.RunWhenSpineReady(sceneRoot, sprite, (Action<MegaAnimationState>)delegate(MegaAnimationState animationState)
			{
				StartDefaultAnimation(groupId, sprite, animationState);
			});
		}
		catch (Exception ex)
		{
			ModLog.Warn("准备 " + groupId + " 的先古 Spine 动画失败：" + ex.Message);
		}
	}

	public static void TryPlay(Node sceneRoot, string? groupId, IReadOnlyList<string> aliases, bool loop, IReadOnlyList<string>? followUpAliases = null, bool followUpLoop = true, string? sfxPath = null)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		Node val = FindSpineNode(sceneRoot);
		if (val == null)
		{
			ModLog.Warn((groupId ?? "其它图鉴") + " 没有可播放的 Spine 节点。");
			return;
		}
		try
		{
			MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val));
			SpineNodeExtensions.RunWhenSpineReady(sceneRoot, sprite, (Action<MegaAnimationState>)delegate(MegaAnimationState animationState)
			{
				try
				{
					MegaSkeleton skeleton = sprite.GetSkeleton();
					object obj;
					if (skeleton == null)
					{
						obj = null;
					}
					else
					{
						MegaSkeletonDataResource data = skeleton.GetData();
						obj = ((data != null) ? data.GetAnimationNames() : null);
					}
					IReadOnlyList<string> readOnlyList = (IReadOnlyList<string>)obj;
					string text = ((readOnlyList == null) ? null : ResolveAnimation(readOnlyList, aliases));
					if (text == null)
					{
						ModLog.Warn((groupId ?? "其它图鉴") + " 没有匹配动作：" + string.Join(", ", aliases));
					}
					else
					{
						SetAnimationCompat(animationState, text, loop);
						if (!loop)
						{
							IReadOnlyList<string> readOnlyList2 = followUpAliases ?? new global::_003C_003Ez__ReadOnlyArray<string>(new string[6] { "idle_loop", "idle", "stand", "standing", "default", "animation" });
							string text2 = ((readOnlyList == null || readOnlyList2.Count == 0) ? null : ResolveAnimation(readOnlyList, readOnlyList2));
							if (text2 != null)
							{
								AddAnimationCompat(animationState, text2, 0f, followUpLoop);
							}
						}
						if (!string.IsNullOrWhiteSpace(sfxPath))
						{
							SfxCmd.Play(sfxPath, 1f);
						}
						ModLog.Info("已播放 " + (groupId ?? "其它图鉴") + " 动作：" + text);
					}
				}
				catch (Exception ex)
				{
					ModLog.Warn("播放 " + (groupId ?? "其它图鉴") + " 动作失败：" + ex.Message);
				}
			});
		}
		catch (Exception ex2)
		{
			ModLog.Warn("准备 " + (groupId ?? "其它图鉴") + " 动作失败：" + ex2.Message);
		}
	}

	public static void ConfigureActions(Node sceneRoot, string? groupId, IReadOnlyList<(Button Button, OtherCreatureActionDefinition Action)> actions)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		(Button Button, OtherCreatureActionDefinition Action)[] actionSnapshot = actions.ToArray();
		(Button, OtherCreatureActionDefinition)[] array = actionSnapshot;
		for (int i = 0; i < array.Length; i++)
		{
			((CanvasItem)array[i].Item1).Visible = false;
		}
		Node val = FindSpineNode(sceneRoot);
		if (val == null)
		{
			return;
		}
		try
		{
			MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val));
			SpineNodeExtensions.RunWhenSpineReady(sceneRoot, sprite, (Action<MegaAnimationState>)delegate
			{
				MegaSkeleton skeleton = sprite.GetSkeleton();
				object obj;
				if (skeleton == null)
				{
					obj = null;
				}
				else
				{
					MegaSkeletonDataResource data = skeleton.GetData();
					obj = ((data != null) ? data.GetAnimationNames() : null);
				}
				IReadOnlyList<string> names = (IReadOnlyList<string>)obj;
				if (names != null)
				{
					(Button, OtherCreatureActionDefinition)[] array2 = actionSnapshot;
					for (int j = 0; j < array2.Length; j++)
					{
						(Button, OtherCreatureActionDefinition) tuple = array2[j];
						if (GodotObject.IsInstanceValid((GodotObject)(object)tuple.Item1))
						{
							((CanvasItem)tuple.Item1).Visible = tuple.Item2.AnimationAliases.Any((string alias) => FindAnimation(names, alias) != null);
						}
					}
				}
			});
		}
		catch (Exception ex)
		{
			ModLog.Warn("检查 " + (groupId ?? "其它图鉴") + " 动作失败：" + ex.Message);
		}
	}

	private static Node? FindSpineNode(Node sceneRoot)
	{
		Node obj = sceneRoot.GetNodeOrNull<Node>(NodePath.op_Implicit("SpineSprite"));
		if (obj == null)
		{
			obj = sceneRoot.FindChild("SpineSprite", true, false);
			if (obj == null)
			{
				if (!((GodotObject)sceneRoot).GetClass().ToString().Equals("SpineSprite", StringComparison.Ordinal))
				{
					return ((IEnumerable)sceneRoot.GetChildren(false)).OfType<Node>().SelectMany(DescendantsAndSelf).FirstOrDefault((Func<Node, bool>)((Node node) => ((GodotObject)node).GetClass().ToString().Equals("SpineSprite", StringComparison.Ordinal)));
				}
				obj = sceneRoot;
			}
		}
		return obj;
	}

	private static void StartDefaultAnimation(string groupId, MegaSprite sprite, MegaAnimationState animationState)
	{
		try
		{
			MegaSkeleton skeleton = sprite.GetSkeleton();
			object obj;
			if (skeleton == null)
			{
				obj = null;
			}
			else
			{
				MegaSkeletonDataResource data = skeleton.GetData();
				obj = ((data != null) ? data.GetAnimationNames() : null);
			}
			IReadOnlyList<string> readOnlyList = (IReadOnlyList<string>)obj;
			if (readOnlyList == null || readOnlyList.Count == 0)
			{
				return;
			}
			string currentName = animationState.GetCurrentAnimationName(0);
			if (!string.IsNullOrWhiteSpace(currentName) && readOnlyList.Any((string name) => name.Equals(currentName, StringComparison.OrdinalIgnoreCase)) && !currentName.Equals("Dummy", StringComparison.OrdinalIgnoreCase) && !currentName.StartsWith("Touch_", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			string text = FindAnimation(readOnlyList, "Intro");
			string text2 = FindAnimation(readOnlyList, "Idle");
			if (text != null)
			{
				SetAnimationCompat(animationState, text, loop: false);
				if (text2 != null)
				{
					AddAnimationCompat(animationState, text2, 0f, loop: true);
				}
				ModLog.Info("已启动 " + groupId + " 的先古 Spine 动画：" + text + ((text2 == null) ? string.Empty : (" -> " + text2)));
			}
			else
			{
				string text3 = text2 ?? FindAnimation(readOnlyList, "animation") ?? FindAnimation(readOnlyList, "default") ?? readOnlyList.FirstOrDefault((string name) => !name.Equals("Dummy", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("Touch_", StringComparison.OrdinalIgnoreCase));
				if (text3 != null)
				{
					SetAnimationCompat(animationState, text3, loop: true);
					ModLog.Info("已启动 " + groupId + " 的先古 Spine 循环动画：" + text3);
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("启动 " + groupId + " 的先古 Spine 动画失败：" + ex.Message);
		}
	}

	private static void SetAnimationCompat(MegaAnimationState animationState, string animationName, bool loop)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
		{
			Variant.op_Implicit(animationName),
			Variant.op_Implicit(loop),
			Variant.op_Implicit(0)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static void AddAnimationCompat(MegaAnimationState animationState, string animationName, float delay, bool loop)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		Variant val = ((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("add_animation"), (Variant[])(object)new Variant[4]
		{
			Variant.op_Implicit(animationName),
			Variant.op_Implicit(delay),
			Variant.op_Implicit(loop),
			Variant.op_Implicit(0)
		});
		try
		{
		}
		finally
		{
			((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private static string? FindAnimation(IReadOnlyList<string> animationNames, string expectedName)
	{
		return animationNames.FirstOrDefault((string name) => name.Equals(expectedName, StringComparison.OrdinalIgnoreCase));
	}

	private static string? ResolveAnimation(IReadOnlyList<string> animationNames, IReadOnlyList<string> aliases)
	{
		string? text = aliases.Select((string alias) => FindAnimation(animationNames, alias)).FirstOrDefault((string candidate) => candidate != null);
		if (text == null)
		{
			if (!aliases.Any(IsIdleAlias))
			{
				return null;
			}
			text = animationNames.FirstOrDefault((string name) => !name.Equals("Dummy", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("Touch_", StringComparison.OrdinalIgnoreCase));
		}
		return text;
	}

	private static bool IsIdleAlias(string alias)
	{
		if (!alias.Equals("idle_loop", StringComparison.OrdinalIgnoreCase) && !alias.Equals("idle", StringComparison.OrdinalIgnoreCase) && !alias.Equals("stand", StringComparison.OrdinalIgnoreCase) && !alias.Equals("standing", StringComparison.OrdinalIgnoreCase) && !alias.Equals("default", StringComparison.OrdinalIgnoreCase))
		{
			return alias.Equals("animation", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	[IteratorStateMachine(typeof(_003CDescendantsAndSelf_003Ed__10))]
	private static IEnumerable<Node> DescendantsAndSelf(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendantsAndSelf_003Ed__10(-2)
		{
			_003C_003E3__root = root
		};
	}
}

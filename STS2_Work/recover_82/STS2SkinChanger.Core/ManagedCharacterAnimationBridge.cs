using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class ManagedCharacterAnimationBridge
{
	private sealed record AnimationRoute(string[] Candidates, bool QueueIdle);

	[CompilerGenerated]
	private sealed class _003CDescendantsAndSelf_003Ed__9 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
		public _003CDescendantsAndSelf_003Ed__9(int _003C_003E1__state)
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
			_003CDescendantsAndSelf_003Ed__9 _003CDescendantsAndSelf_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendantsAndSelf_003Ed__ = this;
			}
			else
			{
				_003CDescendantsAndSelf_003Ed__ = new _003CDescendantsAndSelf_003Ed__9(0);
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

	private static readonly MethodInfo? SetAnimationMethod = ResolveAnimationMethod("SetAnimation", 3);

	private static readonly MethodInfo? AddAnimationMethod = ResolveAnimationMethod("AddAnimation", 4);

	public static void TryStartCharacterSelectLoops(Node sceneRoot, string providerId)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		foreach (Node item in from node in DescendantsAndSelf(sceneRoot)
			where ((GodotObject)node).GetClass().ToString().Equals("SpineSprite", StringComparison.Ordinal)
			select node)
		{
			try
			{
				MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)item));
				SpineNodeExtensions.RunWhenSpineReady(sceneRoot, sprite, (Action<MegaAnimationState>)delegate(MegaAnimationState animationState)
				{
					TryStartCharacterSelectLoop(sprite, animationState, providerId);
				});
			}
			catch (Exception ex)
			{
				ModLog.Warn("准备 " + providerId + " 的选角 Spine 动画失败：" + ex.GetBaseException().Message);
			}
		}
	}

	public static void TryDrive(NCreature creature, string trigger)
	{
		try
		{
			Creature entity = creature.Entity;
			object obj;
			if (entity == null)
			{
				obj = null;
			}
			else
			{
				Player player = entity.Player;
				if (player == null)
				{
					obj = null;
				}
				else
				{
					CharacterModel character = player.Character;
					obj = ((character != null) ? ((AbstractModel)character).Id.Entry : null);
				}
			}
			string text = (string)obj;
			if (entity == null || string.IsNullOrWhiteSpace(text) || !SkinService.ShouldDriveManagedCharacterAnimations(text))
			{
				return;
			}
			NCreatureVisuals visuals = creature.Visuals;
			MegaSprite val = ((visuals != null) ? visuals.SpineBody : null);
			if (val == null)
			{
				return;
			}
			AnimationRoute route = GetRoute(trigger);
			if (route.Candidates.Length == 0)
			{
				return;
			}
			string text2 = ResolveAnimation(trigger, entity.CurrentHp, entity.MaxHp, (Func<string, bool>)val.HasAnimation);
			if (text2 == null)
			{
				return;
			}
			MegaAnimationState animationState = val.GetAnimationState();
			if (SetAnimationMethod == null)
			{
				return;
			}
			SetAnimationMethod.Invoke(animationState, new object[3]
			{
				text2,
				IsLooping(text2),
				0
			});
			if (route.QueueIdle)
			{
				string text3 = ResolveAnimation("Idle", entity.CurrentHp, entity.MaxHp, (Func<string, bool>)val.HasAnimation);
				if (text3 != null && AddAnimationMethod != null && !text3.Equals(text2, StringComparison.OrdinalIgnoreCase))
				{
					AddAnimationMethod.Invoke(animationState, new object[4] { text3, 0f, true, 0 });
				}
			}
		}
		catch
		{
		}
	}

	private static AnimationRoute GetRoute(string trigger)
	{
		switch (trigger)
		{
		case "Attack":
			return new AnimationRoute(new string[3] { "attack1", "attack", "attack_1" }, QueueIdle: true);
		case "Shiv":
			return new AnimationRoute(new string[5] { "shiv", "attack2", "attack_2", "attack1", "attack" }, QueueIdle: true);
		case "Cast":
		case "PowerUp":
			return new AnimationRoute(new string[4] { "cast", "power_up", "attack1", "attack" }, QueueIdle: true);
		case "Dead":
			return new AnimationRoute(new string[3] { "die", "death", "dead" }, QueueIdle: false);
		case "Hit":
			return new AnimationRoute(new string[2] { "hurt", "hit" }, QueueIdle: true);
		case "Revive":
		case "Idle":
			return new AnimationRoute(new string[2] { "idle_loop", "idle" }, QueueIdle: false);
		case "Relaxed":
			return new AnimationRoute(new string[3] { "relaxed_loop", "idle_loop", "idle" }, QueueIdle: false);
		default:
			return new AnimationRoute(Array.Empty<string>(), QueueIdle: false);
		}
	}

	internal static string? ResolveAnimation(string trigger, int currentHp, int maxHp, Func<string, bool> hasAnimation)
	{
		bool flag = ((trigger == "Idle" || trigger == "Revive") ? true : false);
		if (flag && maxHp > 0 && (long)currentHp * 4L <= maxHp && hasAnimation("low_health_loop"))
		{
			return "low_health_loop";
		}
		return GetRoute(trigger).Candidates.FirstOrDefault(hasAnimation);
	}

	private static bool IsLooping(string animation)
	{
		if (!animation.EndsWith("_loop", StringComparison.OrdinalIgnoreCase))
		{
			return animation.Equals("idle", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static void TryStartCharacterSelectLoop(MegaSprite sprite, MegaAnimationState animationState, string providerId)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
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
			if (!string.IsNullOrWhiteSpace(currentName) && readOnlyList.Any((string name) => name.Equals(currentName, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			string text = FindAnimation(readOnlyList, "idle_loop") ?? FindAnimation(readOnlyList, "idle") ?? FindAnimation(readOnlyList, "default") ?? FindAnimation(readOnlyList, "animation") ?? readOnlyList.FirstOrDefault((string name) => !name.Equals("Dummy", StringComparison.OrdinalIgnoreCase));
			if (text == null)
			{
				return;
			}
			Variant val = ((MegaSpineBinding)animationState).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
			{
				Variant.op_Implicit(text),
				Variant.op_Implicit(true),
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
		catch (Exception ex)
		{
			ModLog.Warn("启动 " + providerId + " 的选角 Spine 动画失败：" + ex.GetBaseException().Message);
		}
	}

	private static string? FindAnimation(IReadOnlyList<string> animationNames, string expectedName)
	{
		return animationNames.FirstOrDefault((string name) => name.Equals(expectedName, StringComparison.OrdinalIgnoreCase));
	}

	[IteratorStateMachine(typeof(_003CDescendantsAndSelf_003Ed__9))]
	private static IEnumerable<Node> DescendantsAndSelf(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendantsAndSelf_003Ed__9(-2)
		{
			_003C_003E3__root = root
		};
	}

	private static MethodInfo? ResolveAnimationMethod(string name, int parameterCount)
	{
		return typeof(MegaAnimationState).GetMethods(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault(delegate(MethodInfo method)
		{
			if (method.Name.Equals(name, StringComparison.Ordinal))
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == parameterCount)
				{
					Type[] second = ((!name.Equals("SetAnimation", StringComparison.Ordinal)) ? new Type[4]
					{
						typeof(string),
						typeof(float),
						typeof(bool),
						typeof(int)
					} : new Type[3]
					{
						typeof(string),
						typeof(bool),
						typeof(int)
					});
					return parameters.Select((ParameterInfo parameter) => parameter.ParameterType).SequenceEqual(second);
				}
			}
			return false;
		});
	}
}

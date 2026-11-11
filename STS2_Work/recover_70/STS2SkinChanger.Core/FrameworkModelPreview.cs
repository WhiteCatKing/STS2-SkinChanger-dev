using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Unlocks;
using STS2SkinChanger.Ui;

namespace STS2SkinChanger.Core;

internal static class FrameworkModelPreview
{
	[CompilerGenerated]
	private sealed class _003CDescendantsAndSelf_003Ed__13 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
		public _003CDescendantsAndSelf_003Ed__13(int _003C_003E1__state)
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
			_003CDescendantsAndSelf_003Ed__13 _003CDescendantsAndSelf_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendantsAndSelf_003Ed__ = this;
			}
			else
			{
				_003CDescendantsAndSelf_003Ed__ = new _003CDescendantsAndSelf_003Ed__13(0);
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

	private static readonly ConditionalWeakTable<Player, object> PreviewPlayers = new ConditionalWeakTable<Player, object>();

	internal static bool IsPreviewPlayer(Player player)
	{
		object value;
		return PreviewPlayers.TryGetValue(player, out value);
	}

	public static void Refresh(Node selector, CharacterModel character)
	{
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		Node2D nodeOrNull = selector.GetNodeOrNull<Node2D>(NodePath.op_Implicit("VisualContainer"));
		if (nodeOrNull == null)
		{
			return;
		}
		Control val = (Control)(object)((selector is Control) ? selector : null);
		if (val == null)
		{
			return;
		}
		string text = ContextualSkinControls.FindGroup(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name)?.Id;
		string text2 = ((text != null) ? ContextualSkinControls.CanonicalScenePath("creature_visuals/" + ((AbstractModel)character).Id.Entry.ToLowerInvariant()) : ((string)AccessTools.PropertyGetter(typeof(CharacterModel), "VisualsPath").Invoke(character, null)));
		FrameworkPreviewSurface frameworkPreviewSurface = null;
		NCreatureVisuals val2 = null;
		try
		{
			using ((text == null) ? null : SkinService.BeginRuntimeResourceScope(text, text2))
			{
				Player val3 = CreatePreviewPlayer(character, UnlockState.all);
				val2 = CreateVisuals(character, text, text2, val3.Creature);
				FrameworkPreviewSurface frameworkPreviewSurface2 = new FrameworkPreviewSurface();
				((Node)frameworkPreviewSurface2).Name = StringName.op_Implicit("SkinChangerPendingPreview");
				frameworkPreviewSurface = frameworkPreviewSurface2;
				FrameworkPreviewCreature frameworkPreviewCreature = new FrameworkPreviewCreature();
				frameworkPreviewCreature.Initialize(val3, val2);
				frameworkPreviewSurface.Initialize(val, nodeOrNull, frameworkPreviewCreature, text ?? ((AbstractModel)character).Id.Entry);
				((Node)nodeOrNull).AddChild((Node)(object)frameworkPreviewSurface, false, (InternalMode)0);
				ApplyRuntimeSpine(val2, character, text);
				bool flag = IntrinsicCharacterPreview.Replay((NCreature)(object)frameworkPreviewCreature, character, text);
				CharacterAppearanceRuntime.ReplaySelectedCreatureNodeReady((NCreature)(object)frameworkPreviewCreature);
				if (text != null)
				{
					ManagedSlotVisibilityBridge.BindModel((Node)(object)val2, text, SkinService.GetSelectedCreatureRuntimeProvider(text));
				}
				foreach (Control item in DescendantsAndSelf((Node)(object)frameworkPreviewSurface).OfType<Control>())
				{
					item.MouseFilter = (MouseFilterEnum)2;
				}
				Node nodeOrNull2 = ((Node)nodeOrNull).GetNodeOrNull<Node>(NodePath.op_Implicit("PreviewSprite"));
				if (nodeOrNull2 != null)
				{
					((Node)nodeOrNull).RemoveChild(nodeOrNull2);
					nodeOrNull2.QueueFree();
				}
				((Node)frameworkPreviewSurface).Name = StringName.op_Implicit("PreviewSprite");
				if (flag)
				{
					Callable val4 = Callable.From((Action)frameworkPreviewSurface.BeginCapture);
					((Callable)(ref val4)).CallDeferred(Array.Empty<Variant>());
				}
				else
				{
					frameworkPreviewSurface.BeginCapture();
				}
				ModLog.Info($"已刷新选角小模型：{((AbstractModel)character).Id.Entry}/{((text == null) ? "unmanaged" : SkinService.Config.GetSelection(text))}；模型类型={((object)val2).GetType().Name}；完整模型子节点={((Node)val2).GetChildCount(false)}。");
				frameworkPreviewSurface = null;
				val2 = null;
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn($"选角小模型加载失败 {((AbstractModel)character).Id.Entry}/{text2}：{ex.GetBaseException().Message}");
		}
		finally
		{
			if (frameworkPreviewSurface != null)
			{
				((Node)frameworkPreviewSurface).QueueFree();
			}
			else if (val2 != null && ((Node)val2).GetParent() == null)
			{
				((GodotObject)val2).Free();
			}
		}
	}

	internal static Player CreatePreviewPlayer(CharacterModel character, UnlockState unlockState)
	{
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		Player val = (Player)typeof(Player).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single(delegate(ConstructorInfo ctor)
		{
			ParameterInfo[] parameters = ctor.GetParameters();
			return parameters != null && parameters.Length == 15 && parameters[0].ParameterType == typeof(CharacterModel) && parameters[1].ParameterType == typeof(ulong);
		}).Invoke(new object[15]
		{
			character,
			0uL,
			character.StartingHp,
			character.StartingHp,
			character.MaxEnergy,
			0,
			0,
			character.BaseOrbSlotCount,
			(object)new RelicGrabBag(),
			unlockState,
			null,
			null,
			null,
			null,
			null
		});
		PreviewPlayers.Add(val, new object());
		return val;
	}

	private static void ApplyRuntimeSpine(NCreatureVisuals visuals, CharacterModel character, string? groupId)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		if (groupId == null)
		{
			return;
		}
		string text = ResolveCombatSpinePath(ManagedSkinModLoader.GetSelectedCharacterSkinPathMethods(groupId), ((object)character).GetType().Name);
		if (text != null)
		{
			MegaSprite obj = visuals.SpineBody ?? throw new InvalidOperationException("所选骨骼皮肤的预览缺少 Spine 主模型。");
			Resource orLoadRuntimeResource = SkinService.GetOrLoadRuntimeResource(groupId, text, includeProviderDependencies: true);
			if (((GodotObject)orLoadRuntimeResource).GetClass().ToString() != "SpineSkeletonDataResource")
			{
				throw new InvalidOperationException("预览路径不是完整 Spine 骨骼资源：" + text);
			}
			obj.SetSkeletonDataRes(new MegaSkeletonDataResource(Variant.op_Implicit((GodotObject)(object)orLoadRuntimeResource)));
			MegaSkeleton skeleton = obj.GetSkeleton();
			if (skeleton != null)
			{
				skeleton.SetSlotsToSetupPose();
			}
			ModLog.Info($"选角小模型已应用运行时骨骼：{groupId}/{text}。");
		}
	}

	internal static string? ResolveCombatSpinePath(IEnumerable<MethodInfo> methods, string characterName)
	{
		MethodInfo[] array = methods.Where(delegate(MethodInfo method)
		{
			if (method.IsStatic && method.ReturnType == typeof(string))
			{
				ParameterInfo[] parameters = method.GetParameters();
				if (parameters != null && parameters.Length == 1)
				{
					ParameterInfo parameterInfo = parameters[0];
					if (parameterInfo != null)
					{
						Type parameterType = parameterInfo.ParameterType;
						if (parameterType == typeof(string))
						{
							return method.Name.Contains("CombatSkinPath", StringComparison.OrdinalIgnoreCase);
						}
					}
				}
			}
			return false;
		}).ToArray();
		if (array.Length != 1)
		{
			return null;
		}
		if (!(array[0].Invoke(null, new object[1] { characterName }) is string text) || !text.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || (!text.EndsWith(".tres", StringComparison.OrdinalIgnoreCase) && !text.EndsWith(".res", StringComparison.OrdinalIgnoreCase)))
		{
			return null;
		}
		return text;
	}

	internal static (float Scale, Vector2 Position)? FitBounds(Rect2 bounds, Rect2 area)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = ((Rect2)(ref bounds)).Position;
		if (((Vector2)(ref val)).IsFinite())
		{
			val = ((Rect2)(ref bounds)).Size;
			if (((Vector2)(ref val)).IsFinite())
			{
				val = ((Rect2)(ref area)).Position;
				if (((Vector2)(ref val)).IsFinite())
				{
					val = ((Rect2)(ref area)).Size;
					if (((Vector2)(ref val)).IsFinite() && !(((Rect2)(ref bounds)).Size.X <= 0f) && !(((Rect2)(ref bounds)).Size.Y <= 0f) && !(((Rect2)(ref area)).Size.X <= 0f) && !(((Rect2)(ref area)).Size.Y <= 0f))
					{
						float num = Math.Min(((Rect2)(ref area)).Size.X / ((Rect2)(ref bounds)).Size.X, ((Rect2)(ref area)).Size.Y / ((Rect2)(ref bounds)).Size.Y);
						return (num, new Vector2(((Rect2)(ref area)).GetCenter().X - ((Rect2)(ref bounds)).GetCenter().X * num, ((Rect2)(ref area)).End.Y - ((Rect2)(ref bounds)).End.Y * num));
					}
				}
			}
		}
		return null;
	}

	internal static Rect2 PreviewArea(Rect2 panel, Rect2? footer)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Rect2 result = ((Rect2)(ref panel)).Grow(-12f);
		if (footer.HasValue)
		{
			Rect2 valueOrDefault = footer.GetValueOrDefault();
			if (((Rect2)(ref valueOrDefault)).Position.Y > ((Rect2)(ref result)).Position.Y && ((Rect2)(ref valueOrDefault)).Position.Y < ((Rect2)(ref panel)).End.Y)
			{
				((Rect2)(ref result)).Size = new Vector2(((Rect2)(ref result)).Size.X, Math.Min(((Rect2)(ref result)).End.Y, ((Rect2)(ref valueOrDefault)).Position.Y - 12f) - ((Rect2)(ref result)).Position.Y);
			}
		}
		return result;
	}

	internal static Rect2 MeasureCaptureSeed(NCreatureVisuals visuals)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		Rect2? val = null;
		foreach (Node2D item in DescendantsAndSelf((Node)(object)visuals).OfType<Node2D>())
		{
			if (!((CanvasItem)item).IsVisibleInTree() || ((CanvasItem)item).Modulate.A <= 0f || ((CanvasItem)item).SelfModulate.A <= 0f)
			{
				continue;
			}
			Node2D val2 = item;
			Sprite2D val3 = (Sprite2D)(object)((val2 is Sprite2D) ? val2 : null);
			Rect2? val6;
			if (val3 == null)
			{
				AnimatedSprite2D val4 = (AnimatedSprite2D)(object)((val2 is AnimatedSprite2D) ? val2 : null);
				if (val4 != null)
				{
					SpriteFrames spriteFrames = val4.SpriteFrames;
					Texture2D val5 = ((spriteFrames != null) ? spriteFrames.GetFrameTexture(val4.Animation, val4.Frame) : null);
					if (val5 != null)
					{
						val6 = new Rect2(val4.Offset - (val4.Centered ? (val5.GetSize() / 2f) : Vector2.Zero), val5.GetSize());
						goto IL_010a;
					}
				}
			}
			else if (val3.Texture != null)
			{
				val6 = val3.GetRect();
				goto IL_010a;
			}
			val6 = null;
			goto IL_010a;
			IL_010a:
			Rect2? val7 = val6;
			if (((GodotObject)item).GetClass().ToString() == "SpineSprite")
			{
				MegaSkeleton skeleton = new MegaSprite(Variant.op_Implicit((GodotObject)(object)item)).GetSkeleton();
				if (skeleton != null)
				{
					val7 = skeleton.GetBounds();
				}
			}
			if (!val7.HasValue)
			{
				continue;
			}
			Rect2 valueOrDefault = val7.GetValueOrDefault();
			Vector2 val8 = ((Rect2)(ref valueOrDefault)).Position;
			if (!((Vector2)(ref val8)).IsFinite())
			{
				continue;
			}
			val8 = ((Rect2)(ref valueOrDefault)).Size;
			if (((Vector2)(ref val8)).IsFinite() && !(((Rect2)(ref valueOrDefault)).Size.X <= 0f) && !(((Rect2)(ref valueOrDefault)).Size.Y <= 0f))
			{
				Rect2 val9 = item.GlobalTransform * valueOrDefault;
				Rect2 value;
				if (!val.HasValue)
				{
					value = val9;
				}
				else
				{
					Rect2 valueOrDefault2 = val.GetValueOrDefault();
					value = ((Rect2)(ref valueOrDefault2)).Merge(val9);
				}
				val = value;
			}
		}
		if (val.HasValue)
		{
			return val.Value;
		}
		Control bounds = visuals.Bounds;
		if (bounds != null && GodotObject.IsInstanceValid((GodotObject)(object)bounds))
		{
			return ((CanvasItem)bounds).GetGlobalTransform() * new Rect2(Vector2.Zero, bounds.Size);
		}
		return new Rect2(-200f, -400f, 400f, 400f);
	}

	internal static NCreatureVisuals CreateVisuals(CharacterModel character, string? groupId, string scenePath, Creature? creature = null)
	{
		if (groupId == null || !SkinService.TryInstantiateSelectedCharacterCreatureVisuals(groupId, scenePath, delegate(NCreatureVisuals visuals)
		{
			ContextualSkinControls.ApplySelectedProviderVisualPostfix(((AbstractModel)character).Id.Entry, ((object)character).GetType().Name, character, ref visuals);
			NCreatureVisuals visuals2 = visuals;
			if (creature != null)
			{
				ContextualSkinControls.ApplySelectedCreatureVisualPostfix(creature, ref visuals2);
			}
			return visuals2 ?? throw new InvalidOperationException("所选皮肤未返回预览模型。");
		}, out NCreatureVisuals visuals3))
		{
			return ((creature != null) ? creature.CreateVisuals() : character.CreateVisuals()) ?? throw new InvalidOperationException("角色未返回预览模型。");
		}
		return visuals3;
	}

	internal static (string? Entry, string? Idle) ResolveAnimations(IReadOnlyList<string> names)
	{
		return (Entry: Find(new string[1] { "entry" }), Idle: Find(new string[7] { "idle_loop", "idle", "stand", "standing", "default", "animation", "relaxed_loop" }));
		string? Find(string[] aliases)
		{
			return aliases.Select((string alias) => names.FirstOrDefault((string name) => name.Equals(alias, StringComparison.OrdinalIgnoreCase))).FirstOrDefault((string name) => name != null);
		}
	}

	public static void StartAnimations(Node root, string groupId, bool playEntry = true)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Expected O, but got Unknown
		Node[] array = DescendantsAndSelf(root).ToArray();
		foreach (Node node in array)
		{
			try
			{
				if (((GodotObject)node).GetClass().ToString() == "SpineSprite")
				{
					MegaSprite sprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)node));
					SpineNodeExtensions.RunWhenSpineReady(root, sprite, (Action<MegaAnimationState>)delegate(MegaAnimationState state)
					{
						//IL_017d: Unknown result type (might be due to invalid IL or missing references)
						//IL_0182: Unknown result type (might be due to invalid IL or missing references)
						//IL_018a: Unknown result type (might be due to invalid IL or missing references)
						//IL_018f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0197: Unknown result type (might be due to invalid IL or missing references)
						//IL_019c: Unknown result type (might be due to invalid IL or missing references)
						//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
						//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
						//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
						//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
						//IL_00be: Unknown result type (might be due to invalid IL or missing references)
						//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
						//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
						//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
						//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
						//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
						//IL_0101: Unknown result type (might be due to invalid IL or missing references)
						//IL_010d: Unknown result type (might be due to invalid IL or missing references)
						//IL_0112: Unknown result type (might be due to invalid IL or missing references)
						//IL_011a: Unknown result type (might be due to invalid IL or missing references)
						//IL_011f: Unknown result type (might be due to invalid IL or missing references)
						//IL_0127: Unknown result type (might be due to invalid IL or missing references)
						//IL_012c: Unknown result type (might be due to invalid IL or missing references)
						//IL_0131: Unknown result type (might be due to invalid IL or missing references)
						//IL_0136: Unknown result type (might be due to invalid IL or missing references)
						if (!GodotObject.IsInstanceValid((GodotObject)(object)root) || ((GodotObject)root).IsQueuedForDeletion() || !GodotObject.IsInstanceValid((GodotObject)(object)node) || ((GodotObject)node).IsQueuedForDeletion())
						{
							return;
						}
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
							if (readOnlyList != null)
							{
								(string, string) tuple = ResolveAnimations(readOnlyList);
								if (playEntry && tuple.Item1 != null)
								{
									Variant val = ((MegaSpineBinding)state).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
									{
										Variant.op_Implicit(tuple.Item1),
										Variant.op_Implicit(false),
										Variant.op_Implicit(0)
									});
									try
									{
										if (tuple.Item2 != null)
										{
											Variant val2 = ((MegaSpineBinding)state).BoundObject.Call(StringName.op_Implicit("add_animation"), (Variant[])(object)new Variant[4]
											{
												Variant.op_Implicit(tuple.Item2),
												Variant.op_Implicit(0f),
												Variant.op_Implicit(true),
												Variant.op_Implicit(0)
											});
											try
											{
												return;
											}
											finally
											{
												((IDisposable)(Variant)(ref val2)/*cast due to .constrained prefix*/).Dispose();
											}
										}
										return;
									}
									finally
									{
										((IDisposable)(Variant)(ref val)/*cast due to .constrained prefix*/).Dispose();
									}
								}
								if (tuple.Item2 != null)
								{
									Variant val3 = ((MegaSpineBinding)state).BoundObject.Call(StringName.op_Implicit("set_animation"), (Variant[])(object)new Variant[3]
									{
										Variant.op_Implicit(tuple.Item2),
										Variant.op_Implicit(true),
										Variant.op_Implicit(0)
									});
									try
									{
										return;
									}
									finally
									{
										((IDisposable)(Variant)(ref val3)/*cast due to .constrained prefix*/).Dispose();
									}
								}
							}
						}
						catch (Exception exception)
						{
							LogAnimationFailure(groupId, exception);
						}
					});
					continue;
				}
				Node obj2 = node;
				AnimatedSprite2D val4 = (AnimatedSprite2D)(object)((obj2 is AnimatedSprite2D) ? obj2 : null);
				if (val4 != null && !val4.IsPlaying() && val4.SpriteFrames != null)
				{
					(string, string) tuple2 = ResolveAnimations(val4.SpriteFrames.GetAnimationNames());
					if (tuple2.Item2 != null)
					{
						val4.Play(StringName.op_Implicit(tuple2.Item2), 1f, false);
					}
					continue;
				}
				Node obj3 = node;
				AnimationPlayer val5 = (AnimationPlayer)(object)((obj3 is AnimationPlayer) ? obj3 : null);
				if (val5 != null && !val5.IsPlaying())
				{
					(string, string) tuple3 = ResolveAnimations(((AnimationMixer)val5).GetAnimationList());
					if (tuple3.Item2 != null)
					{
						val5.Play(StringName.op_Implicit(tuple3.Item2), -1.0, 1f, false);
					}
				}
			}
			catch (Exception exception2)
			{
				LogAnimationFailure(groupId, exception2);
			}
		}
	}

	private static void LogAnimationFailure(string groupId, Exception exception)
	{
		ModLog.Warn("启动 " + groupId + " 的小模型预览动画失败：" + exception.GetBaseException().Message);
	}

	[IteratorStateMachine(typeof(_003CDescendantsAndSelf_003Ed__13))]
	private static IEnumerable<Node> DescendantsAndSelf(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendantsAndSelf_003Ed__13(-2)
		{
			_003C_003E3__root = root
		};
	}
}

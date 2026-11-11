using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;

namespace CznCore.Presentation;

internal static class CombatBodyRenderWarmup
{
	[CompilerGenerated]
	private sealed class _003CEnumerateWarmupAnimations_003Ed__6 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private CharacterSkinProfile profile;

		public CharacterSkinProfile _003C_003E3__profile;

		private string[] _003C_003E7__wrap1;

		private int _003C_003E7__wrap2;

		string IEnumerator<string>.Current
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
		public _003CEnumerateWarmupAnimations_003Ed__6(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003C_003E7__wrap1 = profile.CombatBody.EntrySequence;
				_003C_003E7__wrap2 = 0;
				goto IL_0074;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E7__wrap2++;
				goto IL_0074;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_0074:
				if (_003C_003E7__wrap2 < _003C_003E7__wrap1.Length)
				{
					string text = _003C_003E7__wrap1[_003C_003E7__wrap2];
					_003C_003E2__current = text;
					_003C_003E1__state = 1;
					return true;
				}
				_003C_003E7__wrap1 = null;
				_003C_003E2__current = profile.CombatBody.IdleAnimation;
				_003C_003E1__state = 2;
				return true;
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
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CEnumerateWarmupAnimations_003Ed__6 _003CEnumerateWarmupAnimations_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateWarmupAnimations_003Ed__ = this;
			}
			else
			{
				_003CEnumerateWarmupAnimations_003Ed__ = new _003CEnumerateWarmupAnimations_003Ed__6(0);
			}
			_003CEnumerateWarmupAnimations_003Ed__.profile = _003C_003E3__profile;
			return _003CEnumerateWarmupAnimations_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	private static readonly object WarmupSync = new object();

	private static readonly HashSet<string> CompletedProfiles = new HashSet<string>(StringComparer.Ordinal);

	private static readonly Dictionary<string, Task> ActiveWarmups = new Dictionary<string, Task>(StringComparer.Ordinal);

	public static Task WarmAsync(Node host, CharacterSkinProfile profile)
	{
		ArgumentNullException.ThrowIfNull(profile, "profile");
		profile = profile.For(null);
		lock (WarmupSync)
		{
			if (CompletedProfiles.Contains(profile.ProfileId + "|" + profile.BodySkeletonDataPath))
			{
				return Task.CompletedTask;
			}
			if (ActiveWarmups.TryGetValue(profile.ProfileId + "|" + profile.BodySkeletonDataPath, out Task value))
			{
				return value;
			}
			Task task = WarmAndRecordAsync(host, profile);
			ActiveWarmups[profile.ProfileId + "|" + profile.BodySkeletonDataPath] = task;
			return task;
		}
	}

	private static async Task WarmAndRecordAsync(Node host, CharacterSkinProfile profile)
	{
		bool completed = false;
		try
		{
			completed = await WarmCoreAsync(host, profile);
		}
		finally
		{
			lock (WarmupSync)
			{
				ActiveWarmups.Remove(profile.ProfileId + "|" + profile.BodySkeletonDataPath);
				if (completed)
				{
					CompletedProfiles.Add(profile.ProfileId + "|" + profile.BodySkeletonDataPath);
				}
			}
		}
	}

	private static async Task<bool> WarmCoreAsync(Node host, CharacterSkinProfile profile)
	{
		if (!string.IsNullOrWhiteSpace(profile.BodySkeletonDataPath))
		{
			MainLoop mainLoop = Engine.GetMainLoop();
			SceneTree tree = (SceneTree)(object)((mainLoop is SceneTree) ? mainLoop : null);
			if (tree != null && tree.Root != null && !(DisplayServer.GetName() == "headless"))
			{
				ulong startedMs = Time.GetTicksMsec();
				Node warmNode = null;
				try
				{
					Resource val = ResourceLoader.Load<Resource>(profile.BodySkeletonDataPath, (string)null, (CacheMode)1);
					if (val != null)
					{
						Variant val2 = ClassDB.Instantiate(StringName.op_Implicit("SpineSprite"));
						GodotObject obj = ((Variant)(ref val2)).AsGodotObject();
						Node val3 = (Node)(object)((obj is Node) ? obj : null);
						if (val3 != null)
						{
							warmNode = val3;
							val3.Name = StringName.op_Implicit(profile.RuntimeNodeName("CombatBodyRenderWarmup"));
							MegaSprite megaSprite = new MegaSprite(Variant.op_Implicit((GodotObject)(object)val3));
							GodotObject val4 = (GodotObject)(object)val;
							megaSprite.SetSkeletonDataRes(new MegaSkeletonDataResource(Variant.From<GodotObject>(ref val4)));
							Node2D val5 = (Node2D)(object)((val3 is Node2D) ? val3 : null);
							if (val5 != null)
							{
								val5.Position = Vector2.Zero;
								val5.Scale = CharacterSkinSurfaceDefaults.ResolveCznCombatBodySpineScale(Vector2.One, profile.BodySpineScaleMultiplier);
								val5.Rotation = 0f;
								((CanvasItem)val5).ZAsRelative = true;
								((CanvasItem)val5).ZIndex = 0;
								((CanvasItem)val5).Modulate = Colors.White;
								((CanvasItem)val5).Visible = true;
							}
							Node2D val6 = CombatRenderWarmupSurface.Resolve(profile.RuntimeNodeName("BodyWarmup"));
							if (val6 == null)
							{
								return false;
							}
							((Node)val6).AddChild(val3, false, (InternalMode)0);
							MegaAnimationState animationState = megaSprite.GetAnimationState();
							int warmed = 0;
							foreach (string item in EnumerateWarmupAnimations(profile))
							{
								if (!string.IsNullOrEmpty(item) && megaSprite.HasAnimation(item))
								{
									SpineAnimationCompat.SetAnimation(animationState, item, loop: false);
									await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
									await ((GodotObject)tree).ToSignal((GodotObject)(object)RenderingServer.Singleton, SignalName.FramePostDraw);
									warmed++;
								}
							}
							SkinLog.Info("Combat body render warmup completed animations=" + warmed + " elapsedMs=" + (Time.GetTicksMsec() - startedMs));
							return warmed > 0;
						}
					}
					return false;
				}
				catch (Exception ex)
				{
					SkinLog.Warn("Combat body render warmup failed elapsedMs=" + (Time.GetTicksMsec() - startedMs) + ": " + SkinLog.FormatException(ex));
					return false;
				}
				finally
				{
					if (warmNode != null && GodotObject.IsInstanceValid((GodotObject)(object)warmNode))
					{
						warmNode.QueueFree();
					}
				}
			}
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CEnumerateWarmupAnimations_003Ed__6))]
	private static IEnumerable<string> EnumerateWarmupAnimations(CharacterSkinProfile profile)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateWarmupAnimations_003Ed__6(-2)
		{
			_003C_003E3__profile = profile
		};
	}
}

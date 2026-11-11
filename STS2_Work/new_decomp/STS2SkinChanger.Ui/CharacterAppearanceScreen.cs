using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Events;
using MegaCrit.Sts2.Core.Nodes.Events.Custom;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal class CharacterAppearanceScreen : NSubmenu
{
	private sealed record SelectableAppearanceTarget(AppearanceTargetKind Kind, Rect2 Rect, int Priority, Func<Vector2, bool> Select)
	{
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			RuntimeHelpers.EnsureSufficientExecutionStack();
			builder.Append("Kind = ");
			builder.Append(Kind.ToString());
			builder.Append(", Rect = ");
			Rect2 rect = Rect;
			builder.Append(((object)(Rect2)(ref rect)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Priority = ");
			builder.Append(Priority.ToString());
			builder.Append(", Select = ");
			builder.Append(Select);
			return true;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass157_0
	{
		public CharacterAppearanceScreen _003C_003E4__this;

		public NMerchantCharacter shopPlayer;

		public Rect2 playerRect;

		public NMerchantButton merchant;

		public Rect2 merchantRect;

		public NMerchantButton fakeButton;

		public Rect2 fakeRect;

		public NFakeMerchant fakeMerchant;

		public AncientEventModel ancient;

		public SkinGroup ancientGroup;

		internal bool _003CEnumerateSelectableTargets_003Eb__0(Vector2 _)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			return _003C_003E4__this.SelectShopPlayerTarget(shopPlayer, ((Rect2)(ref playerRect)).GetCenter());
		}

		internal bool _003CEnumerateSelectableTargets_003Eb__2(Vector2 _)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			return _003C_003E4__this.SelectMerchantTarget(merchant, ((Rect2)(ref merchantRect)).GetCenter());
		}

		internal bool _003CEnumerateSelectableTargets_003Eb__4(Vector2 _)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			return _003C_003E4__this.SelectMerchantTarget(fakeButton, ((Rect2)(ref fakeRect)).GetCenter(), "fake_merchant_monster", fakeMerchant);
		}

		internal bool _003CEnumerateSelectableTargets_003Eb__7(Vector2 position)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			return _003C_003E4__this.SelectAncientTarget(ancient, ancientGroup, position);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass157_1
	{
		public NBossMapPoint point;

		public Rect2 rect;

		public _003C_003Ec__DisplayClass157_0 CS_0024_003C_003E8__locals1;

		internal bool _003CEnumerateSelectableTargets_003Eb__5(Vector2 _)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return CS_0024_003C_003E8__locals1._003C_003E4__this.SelectBossMapTarget(point, ((Rect2)(ref rect)).GetCenter());
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass157_2
	{
		public NCreature creature;

		public Rect2 rect;

		public _003C_003Ec__DisplayClass157_0 CS_0024_003C_003E8__locals2;

		internal bool _003CEnumerateSelectableTargets_003Eb__6(Vector2 _)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return CS_0024_003C_003E8__locals2._003C_003E4__this.SelectCreatureTarget(creature, ((Rect2)(ref rect)).GetCenter());
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateDescendants_003Ed__108<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable where T : notnull, Node
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node root;

		public Node _003C_003E3__root;

		private IEnumerator<Node> _003C_003E7__wrap1;

		private Node _003Cchild_003E5__3;

		private IEnumerator<T> _003C_003E7__wrap3;

		T IEnumerator<T>.Current
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
		public _003CEnumerateDescendants_003Ed__108(int _003C_003E1__state)
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
					goto IL_00f9;
				case 1:
					_003C_003E1__state = -3;
					goto IL_0092;
				case 2:
					{
						_003C_003E1__state = -4;
						goto IL_00d8;
					}
					IL_00f9:
					if (_003C_003E7__wrap1.MoveNext())
					{
						_003Cchild_003E5__3 = _003C_003E7__wrap1.Current;
						Node obj = _003Cchild_003E5__3;
						T val = (T)(object)((obj is T) ? obj : null);
						if (val != null)
						{
							_003C_003E2__current = val;
							_003C_003E1__state = 1;
							return true;
						}
						goto IL_0092;
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_0092:
					_003C_003E7__wrap3 = EnumerateDescendants<T>(_003Cchild_003E5__3).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_00d8;
					IL_00d8:
					if (_003C_003E7__wrap3.MoveNext())
					{
						T current = _003C_003E7__wrap3.Current;
						_003C_003E2__current = current;
						_003C_003E1__state = 2;
						return true;
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap3 = null;
					_003Cchild_003E5__3 = null;
					goto IL_00f9;
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
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			_003CEnumerateDescendants_003Ed__108<T> _003CEnumerateDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateDescendants_003Ed__ = this;
			}
			else
			{
				_003CEnumerateDescendants_003Ed__ = new _003CEnumerateDescendants_003Ed__108<T>(0);
			}
			_003CEnumerateDescendants_003Ed__.root = _003C_003E3__root;
			return _003CEnumerateDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<T>)this).GetEnumerator();
		}
	}

	[CompilerGenerated]
	private sealed class _003CEnumerateSelectableTargets_003Ed__157 : IEnumerable<SelectableAppearanceTarget>, IEnumerable, IEnumerator<SelectableAppearanceTarget>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private SelectableAppearanceTarget _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public CharacterAppearanceScreen _003C_003E4__this;

		private _003C_003Ec__DisplayClass157_0 _003C_003E8__1;

		private IEnumerator<NBossMapPoint> _003C_003E7__wrap1;

		private IEnumerator<NCreature> _003C_003E7__wrap2;

		SelectableAppearanceTarget IEnumerator<SelectableAppearanceTarget>.Current
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
		public _003CEnumerateSelectableTargets_003Ed__157(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			switch (_003C_003E1__state)
			{
			case -3:
			case 1:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
				break;
			case -4:
			case 2:
				try
				{
				}
				finally
				{
					_003C_003Em__Finally2();
				}
				break;
			}
			_003C_003E8__1 = null;
			_003C_003E7__wrap1 = null;
			_003C_003E7__wrap2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0609: Unknown result type (might be due to invalid IL or missing references)
			//IL_0351: Unknown result type (might be due to invalid IL or missing references)
			//IL_0370: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_0463: Unknown result type (might be due to invalid IL or missing references)
			//IL_057d: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				int num = _003C_003E1__state;
				CharacterAppearanceScreen characterAppearanceScreen = _003C_003E4__this;
				_003C_003Ec__DisplayClass157_0 _003C_003Ec__DisplayClass157_;
				NEventRoom instance3;
				ref NFakeMerchant reference;
				ICustomEventNode obj;
				_003C_003Ec__DisplayClass157_0 _003C_003Ec__DisplayClass157_2;
				NFakeMerchant obj2;
				NAncientEventLayout layout;
				Control backgroundTarget;
				Rect2 rect;
				_003C_003Ec__DisplayClass157_0 _003C_003Ec__DisplayClass157_3;
				NMerchantRoom instance4;
				switch (num)
				{
				default:
					return false;
				case 0:
				{
					_003C_003E1__state = -1;
					_003C_003E8__1 = new _003C_003Ec__DisplayClass157_0();
					_003C_003E8__1._003C_003E4__this = _003C_003E4__this;
					NMapScreen instance = NMapScreen.Instance;
					if (instance != null && GodotObject.IsInstanceValid((GodotObject)(object)instance) && ((CanvasItem)instance).IsVisibleInTree())
					{
						_003C_003E7__wrap1 = CharacterAppearanceScreen.EnumerateDescendants<NBossMapPoint>((Node)(object)instance).GetEnumerator();
						_003C_003E1__state = -3;
						goto IL_0145;
					}
					NCombatRoom instance2 = NCombatRoom.Instance;
					_003C_003E7__wrap2 = (((instance2 != null) ? instance2.CreatureNodes : null) ?? Array.Empty<NCreature>()).GetEnumerator();
					_003C_003E1__state = -4;
					goto IL_0289;
				}
				case 1:
					_003C_003E1__state = -3;
					goto IL_0145;
				case 2:
					_003C_003E1__state = -4;
					goto IL_0289;
				case 3:
					_003C_003E1__state = -1;
					goto IL_03a6;
				case 4:
					_003C_003E1__state = -1;
					goto IL_0499;
				case 5:
					_003C_003E1__state = -1;
					goto IL_05b3;
				case 6:
					{
						_003C_003E1__state = -1;
						break;
					}
					IL_0499:
					_003C_003Ec__DisplayClass157_ = _003C_003E8__1;
					instance3 = NEventRoom.Instance;
					reference = ref _003C_003Ec__DisplayClass157_.fakeMerchant;
					obj = ((instance3 != null) ? instance3.CustomEventNode : null);
					reference = (NFakeMerchant)(object)((obj is NFakeMerchant) ? obj : null);
					_003C_003Ec__DisplayClass157_2 = _003C_003E8__1;
					obj2 = _003C_003E8__1.fakeMerchant;
					_003C_003Ec__DisplayClass157_2.fakeButton = ((obj2 != null) ? obj2.MerchantButton : null);
					if (_003C_003E8__1.fakeButton != null && GodotObject.IsInstanceValid((GodotObject)(object)_003C_003E8__1.fakeButton) && ((CanvasItem)_003C_003E8__1.fakeButton).IsVisibleInTree())
					{
						SkinCatalog? catalog = SkinService.Catalog;
						if (catalog != null && catalog.Groups.Any((SkinGroup group) => group.Id.Equals("fake_merchant_monster", StringComparison.OrdinalIgnoreCase)) && characterAppearanceScreen._dragSurface.TryGetCanvasRect((Control)(object)_003C_003E8__1.fakeButton, 8f, out _003C_003E8__1.fakeRect))
						{
							_003C_003E2__current = new SelectableAppearanceTarget(AppearanceTargetKind.Merchant, _003C_003E8__1.fakeRect, 3, (Vector2 _) => _003C_003E8__1._003C_003E4__this.SelectMerchantTarget(_003C_003E8__1.fakeButton, ((Rect2)(ref _003C_003E8__1.fakeRect)).GetCenter(), "fake_merchant_monster", _003C_003E8__1.fakeMerchant));
							_003C_003E1__state = 5;
							return true;
						}
					}
					goto IL_05b3;
					IL_05b3:
					if (!AncientRuntimeAppearance.TryGetCurrent(out _003C_003E8__1.ancient, out layout, out _003C_003E8__1.ancientGroup))
					{
						break;
					}
					backgroundTarget = AncientRuntimeAppearance.GetBackgroundTarget(layout);
					if (backgroundTarget != null && GodotObject.IsInstanceValid((GodotObject)(object)backgroundTarget) && ((CanvasItem)backgroundTarget).IsVisibleInTree() && characterAppearanceScreen._dragSurface.TryGetCanvasRect(backgroundTarget, 0f, out rect))
					{
						_003C_003E2__current = new SelectableAppearanceTarget(AppearanceTargetKind.Ancient, rect, 5, (Vector2 position) => _003C_003E8__1._003C_003E4__this.SelectAncientTarget(_003C_003E8__1.ancient, _003C_003E8__1.ancientGroup, position));
						_003C_003E1__state = 6;
						return true;
					}
					break;
					IL_0145:
					while (_003C_003E7__wrap1.MoveNext())
					{
						_003C_003Ec__DisplayClass157_1 CS_0024_003C_003E8__locals9 = new _003C_003Ec__DisplayClass157_1
						{
							CS_0024_003C_003E8__locals1 = _003C_003E8__1,
							point = _003C_003E7__wrap1.Current
						};
						if (GodotObject.IsInstanceValid((GodotObject)(object)CS_0024_003C_003E8__locals9.point) && ((CanvasItem)CS_0024_003C_003E8__locals9.point).IsVisibleInTree() && CharacterAppearanceRuntime.TryGetBossMapAppearance(CS_0024_003C_003E8__locals9.point, out SkinGroup _, out string _) && characterAppearanceScreen._dragSurface.TryGetCanvasRect((Control)(object)CS_0024_003C_003E8__locals9.point, 8f, out CS_0024_003C_003E8__locals9.rect))
						{
							_003C_003E2__current = new SelectableAppearanceTarget(AppearanceTargetKind.MapBoss, CS_0024_003C_003E8__locals9.rect, 4, (Vector2 _) => CS_0024_003C_003E8__locals9.CS_0024_003C_003E8__locals1._003C_003E4__this.SelectBossMapTarget(CS_0024_003C_003E8__locals9.point, ((Rect2)(ref CS_0024_003C_003E8__locals9.rect)).GetCenter()));
							_003C_003E1__state = 1;
							return true;
						}
					}
					_003C_003Em__Finally1();
					_003C_003E7__wrap1 = null;
					return false;
					IL_03a6:
					_003C_003Ec__DisplayClass157_3 = _003C_003E8__1;
					instance4 = NMerchantRoom.Instance;
					_003C_003Ec__DisplayClass157_3.merchant = ((instance4 != null) ? instance4.MerchantButton : null);
					if (_003C_003E8__1.merchant != null && GodotObject.IsInstanceValid((GodotObject)(object)_003C_003E8__1.merchant) && ((CanvasItem)_003C_003E8__1.merchant).IsVisibleInTree())
					{
						SkinCatalog? catalog2 = SkinService.Catalog;
						if (catalog2 != null && catalog2.Groups.Any((SkinGroup group) => group.Id.Equals("merchant", StringComparison.OrdinalIgnoreCase)) && characterAppearanceScreen._dragSurface.TryGetCanvasRect((Control)(object)_003C_003E8__1.merchant, 8f, out _003C_003E8__1.merchantRect))
						{
							_003C_003E2__current = new SelectableAppearanceTarget(AppearanceTargetKind.Merchant, _003C_003E8__1.merchantRect, 2, (Vector2 _) => _003C_003E8__1._003C_003E4__this.SelectMerchantTarget(_003C_003E8__1.merchant, ((Rect2)(ref _003C_003E8__1.merchantRect)).GetCenter()));
							_003C_003E1__state = 4;
							return true;
						}
					}
					goto IL_0499;
					IL_0289:
					while (_003C_003E7__wrap2.MoveNext())
					{
						_003C_003Ec__DisplayClass157_2 CS_0024_003C_003E8__locals22 = new _003C_003Ec__DisplayClass157_2
						{
							CS_0024_003C_003E8__locals2 = _003C_003E8__1,
							creature = _003C_003E7__wrap2.Current
						};
						if (GodotObject.IsInstanceValid((GodotObject)(object)CS_0024_003C_003E8__locals22.creature) && ((CanvasItem)CS_0024_003C_003E8__locals22.creature).IsVisibleInTree() && !CharacterAppearanceRuntime.HasActiveDeathAnimation(CS_0024_003C_003E8__locals22.creature) && CharacterAppearanceRuntime.TryGetCreatureAppearance(CS_0024_003C_003E8__locals22.creature, out CreatureAppearanceBinding _) && characterAppearanceScreen._dragSurface.TryGetCreatureTargetRect(CS_0024_003C_003E8__locals22.creature, out CS_0024_003C_003E8__locals22.rect))
						{
							AppearanceTargetKind kind = ((!CS_0024_003C_003E8__locals22.creature.Entity.IsPlayer) ? ((CS_0024_003C_003E8__locals22.creature.Entity.PetOwner == null) ? AppearanceTargetKind.Monster : AppearanceTargetKind.Companion) : AppearanceTargetKind.Character);
							_003C_003E2__current = new SelectableAppearanceTarget(kind, CS_0024_003C_003E8__locals22.rect, 0, (Vector2 _) => CS_0024_003C_003E8__locals22.CS_0024_003C_003E8__locals2._003C_003E4__this.SelectCreatureTarget(CS_0024_003C_003E8__locals22.creature, ((Rect2)(ref CS_0024_003C_003E8__locals22.rect)).GetCenter()));
							_003C_003E1__state = 2;
							return true;
						}
					}
					_003C_003Em__Finally2();
					_003C_003E7__wrap2 = null;
					_003C_003E8__1.shopPlayer = MerchantRuntimeAppearance.GetLocalPlayerVisual();
					if (characterAppearanceScreen._player != null && _003C_003E8__1.shopPlayer != null && GodotObject.IsInstanceValid((GodotObject)(object)_003C_003E8__1.shopPlayer) && ((CanvasItem)_003C_003E8__1.shopPlayer).IsVisibleInTree() && ContextualSkinControls.FindGroup(((AbstractModel)characterAppearanceScreen._player.Character).Id.Entry, ((object)characterAppearanceScreen._player.Character).GetType().Name) != null && characterAppearanceScreen._dragSurface.TryGetNode2DTargetRect((Node2D)(object)_003C_003E8__1.shopPlayer, new Rect2(-190f, -450f, 380f, 520f), out _003C_003E8__1.playerRect))
					{
						_003C_003E2__current = new SelectableAppearanceTarget(AppearanceTargetKind.Character, _003C_003E8__1.playerRect, 1, (Vector2 _) => _003C_003E8__1._003C_003E4__this.SelectShopPlayerTarget(_003C_003E8__1.shopPlayer, ((Rect2)(ref _003C_003E8__1.playerRect)).GetCenter()));
						_003C_003E1__state = 3;
						return true;
					}
					goto IL_03a6;
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
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		private void _003C_003Em__Finally2()
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
		IEnumerator<SelectableAppearanceTarget> IEnumerable<SelectableAppearanceTarget>.GetEnumerator()
		{
			_003CEnumerateSelectableTargets_003Ed__157 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CEnumerateSelectableTargets_003Ed__157(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<SelectableAppearanceTarget>)this).GetEnumerator();
		}
	}

	private const string BackButtonScenePath = "res://scenes/ui/back_button.tscn";

	private OptionButton _skinDropdown;

	private HSlider _scaleSlider;

	private Label _scaleValue;

	private SpinBox _offsetX;

	private SpinBox _offsetY;

	private HSlider _healthBarScaleSlider;

	private Label _healthBarScaleValue;

	private SpinBox _healthBarOffsetX;

	private SpinBox _healthBarOffsetY;

	private CheckButton _healthBarFollowScale;

	private CheckButton _healthBarFollowMovement;

	private HSlider _intentScaleSlider;

	private Label _intentScaleValue;

	private SpinBox _intentOffsetX;

	private SpinBox _intentOffsetY;

	private CheckButton _intentFollowScale;

	private CheckButton _intentFollowMovement;

	private HSlider _selectionReticleScaleSlider;

	private Label _selectionReticleScaleValue;

	private SpinBox _selectionReticleOffsetX;

	private SpinBox _selectionReticleOffsetY;

	private CheckButton _selectionReticleFollowScale;

	private CheckButton _selectionReticleFollowMovement;

	private Button _compareButton;

	private Button _skinResetButton;

	private Button _modelResetButton;

	private Button _healthBarResetButton;

	private Button _intentResetButton;

	private Button _selectionReticleResetButton;

	private Button _restorePlayerButton;

	private Label _title;

	private Label _skinLabel;

	private Label _modelSectionLabel;

	private Label _scaleLabel;

	private Label _offsetXLabel;

	private Label _offsetYLabel;

	private Label _healthBarSectionLabel;

	private Label _healthBarScaleLabel;

	private Label _healthBarOffsetXLabel;

	private Label _healthBarOffsetYLabel;

	private Label _intentSectionLabel;

	private Label _intentScaleLabel;

	private Label _intentOffsetXLabel;

	private Label _intentOffsetYLabel;

	private Label _selectionReticleSectionLabel;

	private Label _selectionReticleScaleLabel;

	private Label _selectionReticleOffsetXLabel;

	private Label _selectionReticleOffsetYLabel;

	private Label _hint;

	private Label _selectionHint;

	private Label _status;

	private CharacterDragSurface _dragSurface;

	private CanvasLayer _hintLayer;

	private Tween? _hintPulseTween;

	private Tween? _selectionHintPulseTween;

	private PanelContainer _panel;

	private ScrollContainer _settingsScroll;

	private VBoxContainer _settingsContent;

	private NBackButton _backButton;

	private Control _modelSpacer;

	private readonly List<CanvasItem> _skinControls = new List<CanvasItem>();

	private readonly List<CanvasItem> _modelControls = new List<CanvasItem>();

	private readonly List<CanvasItem> _creatureOnlyControls = new List<CanvasItem>();

	private readonly List<CanvasItem> _intentOnlyControls = new List<CanvasItem>();

	private readonly List<CanvasItem> _selectionReticleOnlyControls = new List<CanvasItem>();

	private NCreature? _targetCreature;

	private AncientEventModel? _targetAncient;

	private NMerchantCharacter? _targetShopPlayerVisual;

	private NMerchantButton? _targetMerchantButton;

	private NFakeMerchant? _targetFakeMerchant;

	private NBossMapPoint? _targetBossMapPoint;

	private string? _targetBossTitle;

	private Player? _player;

	private SkinGroup? _group;

	private string? _transformKey;

	private bool _selectionMode = true;

	private bool _updating;

	private bool _comparing;

	private bool _canEditSkin = true;

	private bool _canEditTransform = true;

	private CharacterAppearanceDragTarget _dragTarget;

	private Vector2 _dragStartPosition;

	private Vector2 _dragStartOffset;

	private NSelectionReticle? _previewSelectionReticle;

	private bool _previewSelectionReticleWasSelected;

	private Color _previewSelectionReticleModulate;

	private Vector2 _previewSelectionReticleScale;

	private Timer _selectionHintTimer;

	protected override Control? InitialFocusedControl
	{
		get
		{
			if (!_selectionMode)
			{
				return GetTargetInitialFocus();
			}
			return (Control?)(object)_backButton;
		}
	}

	public override void _Ready()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		((Control)this).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)this).MouseFilter = (MouseFilterEnum)0;
		BuildInterface();
		_selectionHintTimer = new Timer
		{
			WaitTime = 0.35,
			ProcessMode = (ProcessModeEnum)3
		};
		_selectionHintTimer.Timeout += RefreshSelectionHint;
		((Node)this).AddChild((Node)(object)_selectionHintTimer, false, (InternalMode)0);
		((NSubmenu)this).ConnectSignals();
		ModLocalization.Bind((Node)(object)this, RefreshLocalizedText);
	}

	protected override void ConnectSignals()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((GodotObject)_backButton).Connect(SignalName.Released, Callable.From<NButton>((Action<NButton>)OnBackPressed), 0u);
		((NClickableControl)_backButton).Disable();
		((CanvasItem)this).VisibilityChanged += OnScreenVisibilityChanged;
	}

	private void OnBackPressed(NButton _)
	{
		if (!_selectionMode)
		{
			EnterSelectionMode();
		}
		else
		{
			base._stack.Pop();
		}
	}

	private void OnScreenVisibilityChanged()
	{
		if (((CanvasItem)this).Visible)
		{
			_backButton.MoveToHidePosition();
			((NClickableControl)_backButton).Enable();
			((NSubmenu)this).OnSubmenuShown();
			NodeUtil.TryGrabFocus((Control)(_selectionMode ? ((object)_backButton) : ((object)GetTargetInitialFocus())));
		}
		else
		{
			Viewport viewport = ((Node)this).GetViewport();
			base._lastFocusedControl = ((viewport != null) ? viewport.GuiGetFocusOwner() : null);
			((NClickableControl)_backButton).Disable();
			((NSubmenu)this).OnSubmenuHidden();
		}
	}

	internal void Initialize(Player player)
	{
		_player = player;
		EnterSelectionMode();
	}

	public override void OnSubmenuOpened()
	{
		CharacterAppearanceRuntime.QueuedSelectionFinished += OnQueuedSelectionFinished;
		NCapstoneContainer instance = NCapstoneContainer.Instance;
		if (instance != null)
		{
			instance.DisableBackstopInstantly();
		}
		_hintLayer.Visible = true;
		EnterSelectionMode();
	}

	public override void OnSubmenuClosed()
	{
		FinishDrag(save: true);
		EndComparison();
		EndSelectionReticlePreview();
		CharacterAppearanceRuntime.QueuedSelectionFinished -= OnQueuedSelectionFinished;
		StopHintPulseAnimations();
		_hintLayer.Visible = false;
		NCapstoneContainer instance = NCapstoneContainer.Instance;
		if (instance != null)
		{
			instance.EnableBackstopInstantly();
		}
		((NSubmenu)this).OnSubmenuClosed();
	}

	protected override void OnSubmenuShown()
	{
		_hintLayer.Visible = true;
		BeginSelectionReticlePreview(_targetCreature);
		UpdateDragSurfaceCreature();
		((NSubmenu)this).OnSubmenuShown();
	}

	protected override void OnSubmenuHidden()
	{
		FinishDrag(save: true);
		EndComparison();
		EndSelectionReticlePreview();
		StopHintPulseAnimations();
		_hintLayer.Visible = false;
		((NSubmenu)this).OnSubmenuHidden();
	}

	private void BuildInterface()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Expected O, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0387: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Expected O, but got Unknown
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Expected O, but got Unknown
		//IL_0449: Unknown result type (might be due to invalid IL or missing references)
		//IL_044e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0456: Unknown result type (might be due to invalid IL or missing references)
		//IL_045e: Unknown result type (might be due to invalid IL or missing references)
		//IL_046b: Expected O, but got Unknown
		//IL_047a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_049d: Expected O, but got Unknown
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0569: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0585: Expected O, but got Unknown
		//IL_059d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a7: Expected O, but got Unknown
		//IL_062b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0632: Expected O, but got Unknown
		//IL_0669: Unknown result type (might be due to invalid IL or missing references)
		//IL_0684: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06da: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e7: Expected O, but got Unknown
		//IL_06f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06fe: Expected O, but got Unknown
		//IL_0732: Unknown result type (might be due to invalid IL or missing references)
		//IL_0757: Unknown result type (might be due to invalid IL or missing references)
		//IL_075e: Expected O, but got Unknown
		//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fa: Expected O, but got Unknown
		//IL_0807: Unknown result type (might be due to invalid IL or missing references)
		//IL_0811: Expected O, but got Unknown
		//IL_08a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08af: Expected O, but got Unknown
		//IL_08e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0901: Unknown result type (might be due to invalid IL or missing references)
		//IL_0906: Unknown result type (might be due to invalid IL or missing references)
		//IL_0915: Unknown result type (might be due to invalid IL or missing references)
		//IL_0924: Unknown result type (might be due to invalid IL or missing references)
		//IL_0933: Unknown result type (might be due to invalid IL or missing references)
		//IL_0942: Unknown result type (might be due to invalid IL or missing references)
		//IL_094d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0957: Unknown result type (might be due to invalid IL or missing references)
		//IL_0964: Expected O, but got Unknown
		//IL_0971: Unknown result type (might be due to invalid IL or missing references)
		//IL_097b: Expected O, but got Unknown
		//IL_09af: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09db: Expected O, but got Unknown
		//IL_0a26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a40: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a77: Expected O, but got Unknown
		//IL_0a84: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8e: Expected O, but got Unknown
		//IL_0ad9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae0: Expected O, but got Unknown
		//IL_0b18: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b22: Expected O, but got Unknown
		//IL_0b57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b61: Expected O, but got Unknown
		//IL_0bc8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bcf: Expected O, but got Unknown
		//IL_0c06: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c53: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c62: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c84: Expected O, but got Unknown
		//IL_0c91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9b: Expected O, but got Unknown
		//IL_0ccf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cfb: Expected O, but got Unknown
		//IL_0d46: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d60: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d97: Expected O, but got Unknown
		//IL_0da4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dae: Expected O, but got Unknown
		//IL_0df9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e00: Expected O, but got Unknown
		//IL_0e38: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e42: Expected O, but got Unknown
		//IL_0e77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e81: Expected O, but got Unknown
		//IL_0ee8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0eef: Expected O, but got Unknown
		//IL_0f26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f41: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f46: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f55: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f64: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f82: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f97: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fa4: Expected O, but got Unknown
		//IL_0fb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbb: Expected O, but got Unknown
		//IL_0fef: Unknown result type (might be due to invalid IL or missing references)
		//IL_1014: Unknown result type (might be due to invalid IL or missing references)
		//IL_101b: Expected O, but got Unknown
		//IL_1066: Unknown result type (might be due to invalid IL or missing references)
		//IL_1080: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b7: Expected O, but got Unknown
		//IL_10c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_10ce: Expected O, but got Unknown
		//IL_1119: Unknown result type (might be due to invalid IL or missing references)
		//IL_1120: Expected O, but got Unknown
		//IL_1158: Unknown result type (might be due to invalid IL or missing references)
		//IL_1162: Expected O, but got Unknown
		//IL_1197: Unknown result type (might be due to invalid IL or missing references)
		//IL_11a1: Expected O, but got Unknown
		CharacterDragSurface characterDragSurface = new CharacterDragSurface();
		((Node)characterDragSurface).Name = StringName.op_Implicit("CharacterDragSurface");
		((Control)characterDragSurface).MouseFilter = (MouseFilterEnum)0;
		((Control)characterDragSurface).MouseDefaultCursorShape = (CursorShape)13;
		((Node)characterDragSurface).ProcessMode = (ProcessModeEnum)3;
		((CanvasItem)characterDragSurface).ZIndex = 5;
		_dragSurface = characterDragSurface;
		((Node)this).AddChild((Node)(object)_dragSurface, false, (InternalMode)0);
		((Control)_dragSurface).SetAnchorsAndOffsetsPreset((LayoutPreset)15, (LayoutPresetMode)0, 0);
		((Control)_dragSurface).GuiInput += new GuiInputEventHandler(OnDragSurfaceInput);
		_hintLayer = new CanvasLayer
		{
			Name = StringName.op_Implicit("CharacterAppearanceHintLayer"),
			Layer = 100,
			Visible = false
		};
		((Node)this).AddChild((Node)(object)_hintLayer, false, (InternalMode)0);
		_hint = new Label
		{
			Name = StringName.op_Implicit("DragHint"),
			Position = new Vector2(34f, 28f),
			Size = new Vector2(760f, 64f),
			HorizontalAlignment = (HorizontalAlignment)0,
			VerticalAlignment = (VerticalAlignment)1,
			AutowrapMode = (AutowrapMode)3,
			MouseFilter = (MouseFilterEnum)2,
			ProcessMode = (ProcessModeEnum)3
		};
		ApplyTextTheme((Control)(object)_hint, 22);
		ModThemeRuntime.AccentText((Control)(object)_hint);
		((Node)_hintLayer).AddChild((Node)(object)_hint, false, (InternalMode)0);
		_selectionHint = new Label
		{
			Name = StringName.op_Implicit("SelectionHint"),
			AnchorLeft = 0.15f,
			AnchorTop = 0f,
			AnchorRight = 0.85f,
			AnchorBottom = 0f,
			OffsetTop = 24f,
			OffsetBottom = 94f,
			HorizontalAlignment = (HorizontalAlignment)1,
			VerticalAlignment = (VerticalAlignment)1,
			AutowrapMode = (AutowrapMode)3,
			MouseFilter = (MouseFilterEnum)2,
			ProcessMode = (ProcessModeEnum)3
		};
		ApplyTextTheme((Control)(object)_selectionHint, 28);
		ModThemeRuntime.AccentText((Control)(object)_selectionHint);
		((Node)_hintLayer).AddChild((Node)(object)_selectionHint, false, (InternalMode)0);
		_restorePlayerButton = BuildButton(string.Empty);
		((Node)_restorePlayerButton).Name = StringName.op_Implicit("RestorePlayerPosition");
		((Control)_restorePlayerButton).AnchorLeft = 0.5f;
		((Control)_restorePlayerButton).AnchorTop = 0f;
		((Control)_restorePlayerButton).AnchorRight = 0.5f;
		((Control)_restorePlayerButton).AnchorBottom = 0f;
		((Control)_restorePlayerButton).OffsetLeft = -145f;
		((Control)_restorePlayerButton).OffsetTop = 102f;
		((Control)_restorePlayerButton).OffsetRight = 145f;
		((Control)_restorePlayerButton).OffsetBottom = 148f;
		((Control)_restorePlayerButton).MouseFilter = (MouseFilterEnum)0;
		((Node)_restorePlayerButton).ProcessMode = (ProcessModeEnum)3;
		((CanvasItem)_restorePlayerButton).ZIndex = 1;
		((CanvasItem)_restorePlayerButton).Visible = false;
		((BaseButton)_restorePlayerButton).Pressed += RestoreLocalPlayerModelTransform;
		((Node)_hintLayer).AddChild((Node)(object)_restorePlayerButton, false, (InternalMode)0);
		((CanvasItem)_hint).SelfModulate = Colors.White;
		((CanvasItem)_selectionHint).SelfModulate = Colors.White;
		_panel = new PanelContainer
		{
			Name = StringName.op_Implicit("AppearancePanel"),
			AnchorLeft = 0.61f,
			AnchorTop = 0.075f,
			AnchorRight = 0.98f,
			AnchorBottom = 0.075f,
			OffsetLeft = -10f,
			OffsetRight = 0f,
			GrowVertical = (GrowDirection)1,
			MouseFilter = (MouseFilterEnum)0,
			ZIndex = 10,
			Visible = false
		};
		((Control)_panel).AddThemeStyleboxOverride(StringName.op_Implicit("panel"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color(0.1f, 0.16f, 0.22f, 0.94f), new Color("7394ad"), 2));
		ModThemeRuntime.Panel(_panel);
		((Node)this).AddChild((Node)(object)_panel, false, (InternalMode)0);
		MarginContainer val = new MarginContainer();
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_left"), 28);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_top"), 24);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_right"), 28);
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("margin_bottom"), 24);
		((Node)_panel).AddChild((Node)(object)val, false, (InternalMode)0);
		_settingsScroll = new ScrollContainer
		{
			HorizontalScrollMode = (ScrollMode)0,
			VerticalScrollMode = (ScrollMode)0,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Node)val).AddChild((Node)(object)_settingsScroll, false, (InternalMode)0);
		VBoxContainer val2 = (_settingsContent = new VBoxContainer
		{
			SizeFlagsHorizontal = (SizeFlags)3,
			CustomMinimumSize = new Vector2(460f, 0f)
		});
		((Control)val2).AddThemeConstantOverride(StringName.op_Implicit("separation"), 7);
		((Node)_settingsScroll).AddChild((Node)(object)val2, false, (InternalMode)0);
		_title = BuildLabel(string.Empty, 31, (HorizontalAlignment)1);
		((Control)_title).CustomMinimumSize = new Vector2(0f, 46f);
		((Node)val2).AddChild((Node)(object)_title, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)BuildVerticalSpacer(12f), false, (InternalMode)0);
		HBoxContainer val3 = BuildSectionHeader(out _skinLabel, out _skinResetButton);
		((Node)val2).AddChild((Node)(object)val3, false, (InternalMode)0);
		((BaseButton)_skinResetButton).Pressed += ResetSkin;
		_skinDropdown = new OptionButton
		{
			CustomMinimumSize = new Vector2(0f, 50f),
			FitToLongestItem = false,
			ClipText = true,
			Alignment = (HorizontalAlignment)1,
			SizeFlagsHorizontal = (SizeFlags)3
		};
		ContextualSkinControls.ApplyGameTheme(_skinDropdown);
		_skinDropdown.ItemSelected += (ItemSelectedEventHandler)delegate(long index)
		{
			OnSkinSelected(checked((int)index));
		};
		((Node)val2).AddChild((Node)(object)_skinDropdown, false, (InternalMode)0);
		_skinControls.AddRange(new global::_003C_003Ez__ReadOnlyArray<CanvasItem>((CanvasItem[])(object)new CanvasItem[2]
		{
			(CanvasItem)val3,
			(CanvasItem)_skinDropdown
		}));
		_modelSpacer = BuildVerticalSpacer(15f);
		((Node)val2).AddChild((Node)(object)_modelSpacer, false, (InternalMode)0);
		HBoxContainer val4 = BuildSectionHeader(out _modelSectionLabel, out _modelResetButton);
		((Node)val2).AddChild((Node)(object)val4, false, (InternalMode)0);
		((BaseButton)_modelResetButton).Pressed += ResetModelTransform;
		HBoxContainer val5 = new HBoxContainer();
		((Control)val5).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_scaleLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_scaleLabel).CustomMinimumSize = new Vector2(74f, 44f);
		((Node)val5).AddChild((Node)(object)_scaleLabel, false, (InternalMode)0);
		_scaleSlider = new HSlider
		{
			MinValue = 0.20000000298023224,
			MaxValue = 5.0,
			Step = 0.05000000074505806,
			Value = 1.0,
			CustomMinimumSize = new Vector2(230f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Range)_scaleSlider).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val5).AddChild((Node)(object)_scaleSlider, false, (InternalMode)0);
		_scaleValue = BuildLabel("100%", 21, (HorizontalAlignment)2);
		((Control)_scaleValue).CustomMinimumSize = new Vector2(82f, 44f);
		((Node)val5).AddChild((Node)(object)_scaleValue, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val5, false, (InternalMode)0);
		HBoxContainer val6 = new HBoxContainer();
		((Control)val6).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_offsetXLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		_offsetYLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_offsetXLabel).CustomMinimumSize = new Vector2(24f, 44f);
		((Control)_offsetYLabel).CustomMinimumSize = new Vector2(24f, 44f);
		_offsetX = BuildOffsetSpinBox();
		_offsetY = BuildOffsetSpinBox();
		((Range)_offsetX).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Range)_offsetY).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val6).AddChild((Node)(object)_offsetXLabel, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_offsetX, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_offsetYLabel, false, (InternalMode)0);
		((Node)val6).AddChild((Node)(object)_offsetY, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val6, false, (InternalMode)0);
		Control val7 = BuildVerticalSpacer(15f);
		((Node)val2).AddChild((Node)(object)val7, false, (InternalMode)0);
		HBoxContainer val8 = BuildSectionHeader(out _healthBarSectionLabel, out _healthBarResetButton);
		((Node)val2).AddChild((Node)(object)val8, false, (InternalMode)0);
		((BaseButton)_healthBarResetButton).Pressed += ResetHealthBarTransform;
		HBoxContainer val9 = new HBoxContainer();
		((Control)val9).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_healthBarScaleLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_healthBarScaleLabel).CustomMinimumSize = new Vector2(74f, 44f);
		((Node)val9).AddChild((Node)(object)_healthBarScaleLabel, false, (InternalMode)0);
		_healthBarScaleSlider = new HSlider
		{
			MinValue = 0.20000000298023224,
			MaxValue = 5.0,
			Step = 0.05000000074505806,
			Value = 1.0,
			CustomMinimumSize = new Vector2(230f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Range)_healthBarScaleSlider).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val9).AddChild((Node)(object)_healthBarScaleSlider, false, (InternalMode)0);
		_healthBarScaleValue = BuildLabel("100%", 21, (HorizontalAlignment)2);
		((Control)_healthBarScaleValue).CustomMinimumSize = new Vector2(82f, 44f);
		((Node)val9).AddChild((Node)(object)_healthBarScaleValue, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val9, false, (InternalMode)0);
		HBoxContainer val10 = new HBoxContainer();
		((Control)val10).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_healthBarOffsetXLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		_healthBarOffsetYLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_healthBarOffsetXLabel).CustomMinimumSize = new Vector2(24f, 44f);
		((Control)_healthBarOffsetYLabel).CustomMinimumSize = new Vector2(24f, 44f);
		_healthBarOffsetX = BuildOffsetSpinBox();
		_healthBarOffsetY = BuildOffsetSpinBox();
		((Range)_healthBarOffsetX).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Range)_healthBarOffsetY).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val10).AddChild((Node)(object)_healthBarOffsetXLabel, false, (InternalMode)0);
		((Node)val10).AddChild((Node)(object)_healthBarOffsetX, false, (InternalMode)0);
		((Node)val10).AddChild((Node)(object)_healthBarOffsetYLabel, false, (InternalMode)0);
		((Node)val10).AddChild((Node)(object)_healthBarOffsetY, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val10, false, (InternalMode)0);
		HBoxContainer val11 = new HBoxContainer();
		((Control)val11).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_healthBarFollowScale = BuildCheckButton();
		((Control)_healthBarFollowScale).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_healthBarFollowScale).Toggled += (ToggledEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val11).AddChild((Node)(object)_healthBarFollowScale, false, (InternalMode)0);
		_healthBarFollowMovement = BuildCheckButton();
		((Control)_healthBarFollowMovement).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_healthBarFollowMovement).Toggled += (ToggledEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val11).AddChild((Node)(object)_healthBarFollowMovement, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val11, false, (InternalMode)0);
		Control val12 = BuildVerticalSpacer(15f);
		((Node)val2).AddChild((Node)(object)val12, false, (InternalMode)0);
		HBoxContainer val13 = BuildSectionHeader(out _intentSectionLabel, out _intentResetButton);
		((Node)val2).AddChild((Node)(object)val13, false, (InternalMode)0);
		((BaseButton)_intentResetButton).Pressed += ResetIntentTransform;
		HBoxContainer val14 = new HBoxContainer();
		((Control)val14).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_intentScaleLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_intentScaleLabel).CustomMinimumSize = new Vector2(74f, 44f);
		((Node)val14).AddChild((Node)(object)_intentScaleLabel, false, (InternalMode)0);
		_intentScaleSlider = new HSlider
		{
			MinValue = 0.20000000298023224,
			MaxValue = 5.0,
			Step = 0.05000000074505806,
			Value = 1.0,
			CustomMinimumSize = new Vector2(230f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Range)_intentScaleSlider).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val14).AddChild((Node)(object)_intentScaleSlider, false, (InternalMode)0);
		_intentScaleValue = BuildLabel("100%", 21, (HorizontalAlignment)2);
		((Control)_intentScaleValue).CustomMinimumSize = new Vector2(82f, 44f);
		((Node)val14).AddChild((Node)(object)_intentScaleValue, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val14, false, (InternalMode)0);
		HBoxContainer val15 = new HBoxContainer();
		((Control)val15).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_intentOffsetXLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		_intentOffsetYLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_intentOffsetXLabel).CustomMinimumSize = new Vector2(24f, 44f);
		((Control)_intentOffsetYLabel).CustomMinimumSize = new Vector2(24f, 44f);
		_intentOffsetX = BuildOffsetSpinBox();
		_intentOffsetY = BuildOffsetSpinBox();
		((Range)_intentOffsetX).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Range)_intentOffsetY).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val15).AddChild((Node)(object)_intentOffsetXLabel, false, (InternalMode)0);
		((Node)val15).AddChild((Node)(object)_intentOffsetX, false, (InternalMode)0);
		((Node)val15).AddChild((Node)(object)_intentOffsetYLabel, false, (InternalMode)0);
		((Node)val15).AddChild((Node)(object)_intentOffsetY, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val15, false, (InternalMode)0);
		HBoxContainer val16 = new HBoxContainer();
		((Control)val16).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_intentFollowScale = BuildCheckButton();
		((Control)_intentFollowScale).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_intentFollowScale).Toggled += (ToggledEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val16).AddChild((Node)(object)_intentFollowScale, false, (InternalMode)0);
		_intentFollowMovement = BuildCheckButton();
		((Control)_intentFollowMovement).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_intentFollowMovement).Toggled += (ToggledEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val16).AddChild((Node)(object)_intentFollowMovement, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val16, false, (InternalMode)0);
		Control val17 = BuildVerticalSpacer(15f);
		((Node)val2).AddChild((Node)(object)val17, false, (InternalMode)0);
		HBoxContainer val18 = BuildSectionHeader(out _selectionReticleSectionLabel, out _selectionReticleResetButton);
		((Node)val2).AddChild((Node)(object)val18, false, (InternalMode)0);
		((BaseButton)_selectionReticleResetButton).Pressed += ResetSelectionReticleTransform;
		HBoxContainer val19 = new HBoxContainer();
		((Control)val19).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_selectionReticleScaleLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_selectionReticleScaleLabel).CustomMinimumSize = new Vector2(74f, 44f);
		((Node)val19).AddChild((Node)(object)_selectionReticleScaleLabel, false, (InternalMode)0);
		_selectionReticleScaleSlider = new HSlider
		{
			MinValue = 0.20000000298023224,
			MaxValue = 5.0,
			Step = 0.05000000074505806,
			Value = 1.0,
			CustomMinimumSize = new Vector2(230f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3
		};
		((Range)_selectionReticleScaleSlider).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val19).AddChild((Node)(object)_selectionReticleScaleSlider, false, (InternalMode)0);
		_selectionReticleScaleValue = BuildLabel("100%", 21, (HorizontalAlignment)2);
		((Control)_selectionReticleScaleValue).CustomMinimumSize = new Vector2(82f, 44f);
		((Node)val19).AddChild((Node)(object)_selectionReticleScaleValue, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val19, false, (InternalMode)0);
		HBoxContainer val20 = new HBoxContainer();
		((Control)val20).AddThemeConstantOverride(StringName.op_Implicit("separation"), 10);
		_selectionReticleOffsetXLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		_selectionReticleOffsetYLabel = BuildLabel(string.Empty, 21, (HorizontalAlignment)0);
		((Control)_selectionReticleOffsetXLabel).CustomMinimumSize = new Vector2(24f, 44f);
		((Control)_selectionReticleOffsetYLabel).CustomMinimumSize = new Vector2(24f, 44f);
		_selectionReticleOffsetX = BuildOffsetSpinBox();
		_selectionReticleOffsetY = BuildOffsetSpinBox();
		((Range)_selectionReticleOffsetX).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Range)_selectionReticleOffsetY).ValueChanged += (ValueChangedEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val20).AddChild((Node)(object)_selectionReticleOffsetXLabel, false, (InternalMode)0);
		((Node)val20).AddChild((Node)(object)_selectionReticleOffsetX, false, (InternalMode)0);
		((Node)val20).AddChild((Node)(object)_selectionReticleOffsetYLabel, false, (InternalMode)0);
		((Node)val20).AddChild((Node)(object)_selectionReticleOffsetY, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val20, false, (InternalMode)0);
		HBoxContainer val21 = new HBoxContainer();
		((Control)val21).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		_selectionReticleFollowScale = BuildCheckButton();
		((Control)_selectionReticleFollowScale).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_selectionReticleFollowScale).Toggled += (ToggledEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val21).AddChild((Node)(object)_selectionReticleFollowScale, false, (InternalMode)0);
		_selectionReticleFollowMovement = BuildCheckButton();
		((Control)_selectionReticleFollowMovement).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_selectionReticleFollowMovement).Toggled += (ToggledEventHandler)delegate
		{
			OnTransformChanged();
		};
		((Node)val21).AddChild((Node)(object)_selectionReticleFollowMovement, false, (InternalMode)0);
		((Node)val2).AddChild((Node)(object)val21, false, (InternalMode)0);
		Control val22 = BuildVerticalSpacer(8f);
		((Node)val2).AddChild((Node)(object)val22, false, (InternalMode)0);
		_compareButton = BuildButton(string.Empty);
		((Control)_compareButton).SizeFlagsHorizontal = (SizeFlags)3;
		((BaseButton)_compareButton).ButtonDown += BeginComparison;
		((BaseButton)_compareButton).ButtonUp += EndComparison;
		((Node)val2).AddChild((Node)(object)_compareButton, false, (InternalMode)0);
		_status = BuildLabel(string.Empty, 19, (HorizontalAlignment)1);
		_status.AutowrapMode = (AutowrapMode)3;
		((CanvasItem)_status).Visible = false;
		((Node)val2).AddChild((Node)(object)_status, false, (InternalMode)0);
		_modelControls.AddRange(new global::_003C_003Ez__ReadOnlyArray<CanvasItem>((CanvasItem[])(object)new CanvasItem[3]
		{
			(CanvasItem)val4,
			(CanvasItem)val5,
			(CanvasItem)val6
		}));
		_creatureOnlyControls.AddRange(new global::_003C_003Ez__ReadOnlyArray<CanvasItem>((CanvasItem[])(object)new CanvasItem[7]
		{
			(CanvasItem)val7,
			(CanvasItem)val8,
			(CanvasItem)val9,
			(CanvasItem)val10,
			(CanvasItem)val11,
			(CanvasItem)val22,
			(CanvasItem)_compareButton
		}));
		_intentOnlyControls.AddRange(new global::_003C_003Ez__ReadOnlyArray<CanvasItem>((CanvasItem[])(object)new CanvasItem[5]
		{
			(CanvasItem)val12,
			(CanvasItem)val13,
			(CanvasItem)val14,
			(CanvasItem)val15,
			(CanvasItem)val16
		}));
		_selectionReticleOnlyControls.AddRange(new global::_003C_003Ez__ReadOnlyArray<CanvasItem>((CanvasItem[])(object)new CanvasItem[5]
		{
			(CanvasItem)val17,
			(CanvasItem)val18,
			(CanvasItem)val19,
			(CanvasItem)val20,
			(CanvasItem)val21
		}));
		PackedScene val23 = ResourceLoader.Load<PackedScene>("res://scenes/ui/back_button.tscn", (string)null, (CacheMode)1) ?? throw new InvalidOperationException("无法加载游戏返回按钮场景。");
		_backButton = val23.Instantiate<NBackButton>((GenEditState)0);
		((Node)_backButton).Name = StringName.op_Implicit("BackButton");
		((CanvasItem)_backButton).ZIndex = 20;
		((Node)this).AddChild((Node)(object)_backButton, false, (InternalMode)0);
	}

	private void RefreshLocalizedText()
	{
		RefreshTargetTitle();
		_skinLabel.Text = ModLocalization.Get(ModText.Skin);
		_modelSectionLabel.Text = ModLocalization.Get(ModText.ModelTransform);
		_scaleLabel.Text = ModLocalization.Get(ModText.CharacterScale);
		_healthBarSectionLabel.Text = ModLocalization.Get(ModText.HealthBarTransform);
		_healthBarScaleLabel.Text = ModLocalization.Get(ModText.CharacterScale);
		_intentSectionLabel.Text = ModLocalization.Get(ModText.IntentTransform);
		_intentScaleLabel.Text = ModLocalization.Get(ModText.CharacterScale);
		_selectionReticleSectionLabel.Text = ModLocalization.Get(ModText.SelectionReticleTransform);
		_selectionReticleScaleLabel.Text = ModLocalization.Get(ModText.CharacterScale);
		_offsetXLabel.Text = "X";
		_offsetYLabel.Text = "Y";
		_healthBarOffsetXLabel.Text = "X";
		_healthBarOffsetYLabel.Text = "Y";
		_intentOffsetXLabel.Text = "X";
		_intentOffsetYLabel.Text = "Y";
		_selectionReticleOffsetXLabel.Text = "X";
		_selectionReticleOffsetYLabel.Text = "Y";
		((Control)_offsetX).TooltipText = ModLocalization.Get(ModText.HorizontalOffset);
		((Control)_offsetY).TooltipText = ModLocalization.Get(ModText.VerticalOffset);
		((Control)_healthBarOffsetX).TooltipText = ModLocalization.Get(ModText.HorizontalOffset);
		((Control)_healthBarOffsetY).TooltipText = ModLocalization.Get(ModText.VerticalOffset);
		((Control)_intentOffsetX).TooltipText = ModLocalization.Get(ModText.HorizontalOffset);
		((Control)_intentOffsetY).TooltipText = ModLocalization.Get(ModText.VerticalOffset);
		((Control)_selectionReticleOffsetX).TooltipText = ModLocalization.Get(ModText.HorizontalOffset);
		((Control)_selectionReticleOffsetY).TooltipText = ModLocalization.Get(ModText.VerticalOffset);
		((Button)_healthBarFollowScale).Text = ModLocalization.Get(ModText.FollowModelScale);
		((Button)_healthBarFollowMovement).Text = ModLocalization.Get(ModText.FollowModelMovement);
		((Control)_healthBarFollowScale).TooltipText = ((Button)_healthBarFollowScale).Text;
		((Control)_healthBarFollowMovement).TooltipText = ((Button)_healthBarFollowMovement).Text;
		((Button)_intentFollowScale).Text = ModLocalization.Get(ModText.FollowModelScale);
		((Button)_intentFollowMovement).Text = ModLocalization.Get(ModText.FollowModelMovement);
		((Control)_intentFollowScale).TooltipText = ((Button)_intentFollowScale).Text;
		((Control)_intentFollowMovement).TooltipText = ((Button)_intentFollowMovement).Text;
		((Button)_selectionReticleFollowScale).Text = ModLocalization.Get(ModText.FollowModelScale);
		((Button)_selectionReticleFollowMovement).Text = ModLocalization.Get(ModText.FollowModelMovement);
		((Control)_selectionReticleFollowScale).TooltipText = ((Button)_selectionReticleFollowScale).Text;
		((Control)_selectionReticleFollowMovement).TooltipText = ((Button)_selectionReticleFollowMovement).Text;
		_compareButton.Text = ModLocalization.Get(ModText.HoldToCompare);
		_skinResetButton.Text = ModLocalization.Get(ModText.Reset);
		_modelResetButton.Text = ModLocalization.Get(ModText.Reset);
		_healthBarResetButton.Text = ModLocalization.Get(ModText.Reset);
		_intentResetButton.Text = ModLocalization.Get(ModText.Reset);
		_selectionReticleResetButton.Text = ModLocalization.Get(ModText.Reset);
		_restorePlayerButton.Text = ModLocalization.Get(ModText.RestorePlayerPosition);
		RefreshDragHint();
		RefreshSelectionHint();
		PopulateSkinDropdown();
		RefreshStatusForCurrentContext();
	}

	private void PopulateSkinDropdown()
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		if (_group != null)
		{
			_group = SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup g) => g.Id == _group.Id) ?? _group;
		}
		if (_skinDropdown == null)
		{
			return;
		}
		if (!((CanvasItem)_skinDropdown).Visible)
		{
			((BaseButton)_skinDropdown).Disabled = true;
			return;
		}
		_updating = true;
		_skinDropdown.Clear();
		if (_group == null)
		{
			((BaseButton)_skinDropdown).Disabled = true;
			_updating = false;
			return;
		}
		((BaseButton)_skinDropdown).Disabled = false;
		bool flag = SkinService.HasMonsterSkinCategory(_group.Id);
		if (flag)
		{
			_skinDropdown.AddItem(ModLocalization.Get(ModText.FollowCategory), -1);
			_skinDropdown.SetItemMetadata(0, Variant.op_Implicit("__monster_category__"));
		}
		int itemCount = _skinDropdown.ItemCount;
		_skinDropdown.AddItem(ModLocalization.Get(ModText.GameDefault), -1);
		_skinDropdown.SetItemMetadata(itemCount, Variant.op_Implicit("__base__"));
		SkinCatalog? catalog = SkinService.Catalog;
		IReadOnlyList<SkinOption> readOnlyList;
		if (catalog == null || !catalog.IsCharacterAppearanceGroup(_group.Id))
		{
			IReadOnlyList<SkinOption> options = _group.Options;
			readOnlyList = options;
		}
		else
		{
			readOnlyList = SkinService.GetCharacterSkinOptions(_group.Id);
		}
		IReadOnlyList<SkinOption> options2 = readOnlyList;
		var (readOnlyList2, states) = SkinService.GetSkinListForDisplay(_group.Id, options2);
		SkinChoiceDimming.Bind(_skinDropdown, states);
		foreach (SkinOption item in readOnlyList2)
		{
			int itemCount2 = _skinDropdown.ItemCount;
			_skinDropdown.AddItem(ModLocalization.DisplayOptionName(item.Name), -1);
			_skinDropdown.SetItemMetadata(itemCount2, Variant.op_Implicit(item.Id));
		}
		SkinWorkshopEntry.Append(_skinDropdown);
		PresetChoiceColoring.Attach(_skinDropdown, SkinOptionStylePolicy.IsAccented, colorSelection: false);
		ItemList nodeOrNull = ((Node)_skinDropdown.GetPopup()).GetNodeOrNull<ItemList>(NodePath.op_Implicit("SCColoredChoices"));
		if (nodeOrNull != null)
		{
			CharacterSkinRandomExclusionUi.Attach(_skinDropdown, nodeOrNull, delegate
			{
				if (_group != null)
				{
					SkinCatalog? catalog2 = SkinService.Catalog;
					if (catalog2 != null && catalog2.IsCharacterAppearanceGroup(_group.Id))
					{
						return _group.Id;
					}
				}
				return (string?)null;
			});
		}
		string selection = CharacterAppearanceRuntime.GetRequestedOption(_group.Id) ?? (flag ? SkinService.GetMonsterOverrideSelection(_group.Id) : ((_targetCreature != null) ? MultiplayerSkinSync.GetSelectionForCreature(_targetCreature.Entity, _group.Id) : SkinService.Config.GetSelection(_group.Id)));
		int num = Enumerable.Range(0, _skinDropdown.ItemCount).FirstOrDefault(delegate(int index)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Variant itemMetadata = _skinDropdown.GetItemMetadata(index);
			return ((Variant)(ref itemMetadata)).AsString().Equals(selection, StringComparison.OrdinalIgnoreCase);
		});
		_skinDropdown.Select(num);
		SkinWorkshopEntry.RefreshSelectionColor(_skinDropdown);
		_updating = false;
	}

	private void OnSkinSelected(int index)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (!_updating && _group != null && index >= 0 && index < _skinDropdown.ItemCount)
		{
			Variant itemMetadata = _skinDropdown.GetItemMetadata(index);
			string optionId = ((Variant)(ref itemMetadata)).AsString();
			if (!SkinWorkshopEntry.Open(optionId, (Node)(object)this, _group.Id, PopulateSkinDropdown))
			{
				RequestSkinSelection(optionId);
			}
		}
	}

	private void ResetSkin()
	{
		if (_group != null)
		{
			RequestSkinSelection("__base__");
		}
	}

	private void RequestSkinSelection(string optionId)
	{
		if (_group == null || !_canEditSkin)
		{
			return;
		}
		if (_targetCreature != null && MultiplayerSkinSync.UsesLocalFallbackControls(_targetCreature.Entity))
		{
			if (!MultiplayerSkinSync.TrySetLocalFallbackSkin(_targetCreature.Entity, _group.Id, optionId, out string error))
			{
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + error, warning: true);
				return;
			}
			PopulateSkinDropdown();
			SyncTransformControls();
			SetAppliedStatus(null);
			return;
		}
		EndComparison();
		SetStatus(string.Empty, warning: false);
		if (_targetMerchantButton != null)
		{
			if (_targetFakeMerchant != null)
			{
				RequestFakeMerchantSkinSelection(optionId);
				return;
			}
			string selection = SkinService.Config.GetSelection(_group.Id);
			MerchantRuntimeAppearance.PrepareMerchantSelectionChange();
			if (!SkinService.ApplySelection(_group.Id, optionId))
			{
				MerchantRuntimeAppearance.TryRefreshMerchant(out string _);
				PopulateSkinDropdown();
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + SkinService.LastError, warning: true);
				return;
			}
			PopulateSkinDropdown();
			if (!MerchantRuntimeAppearance.TryRefreshMerchant(out string error3))
			{
				string text = error3;
				MerchantRuntimeAppearance.PrepareMerchantSelectionChange();
				bool flag = SkinService.ApplySelection(_group.Id, selection);
				string error4 = null;
				bool num = flag && MerchantRuntimeAppearance.TryRefreshMerchant(out error4);
				PopulateSkinDropdown();
				NMerchantRoom instance = NMerchantRoom.Instance;
				_targetMerchantButton = ((instance != null) ? instance.MerchantButton : null);
				if (!num)
				{
					ModLog.Error("回滚商人皮肤选择失败：" + (flag ? error4 : SkinService.LastError));
				}
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + text, warning: true);
			}
			else
			{
				NMerchantRoom instance2 = NMerchantRoom.Instance;
				_targetMerchantButton = ((instance2 != null) ? instance2.MerchantButton : null);
				SetAppliedStatus(null);
			}
		}
		else if (_targetAncient != null)
		{
			if (!SkinService.ApplySelection(_group.Id, optionId))
			{
				PopulateSkinDropdown();
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + SkinService.LastError, warning: true);
				return;
			}
			PopulateSkinDropdown();
			if (!AncientRuntimeAppearance.TryRefresh(_group.Id, out string error5))
			{
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + error5, warning: true);
			}
			else
			{
				SetAppliedStatus(null);
			}
		}
		else
		{
			AppearanceSelectionRequestResult appearanceSelectionRequestResult = CharacterAppearanceRuntime.RequestSelection(_group.Id, optionId);
			switch (appearanceSelectionRequestResult.State)
			{
			case AppearanceSelectionRequestState.Applied:
			{
				string text2 = RefreshShopPlayerVisualAfterSelection();
				PopulateSkinDropdown();
				RefreshTargetTitle();
				SetTransformControlsEnabled(enabled: true);
				SyncTransformControls();
				UpdateDragSurfaceCreature();
				SetAppliedStatus(appearanceSelectionRequestResult.Error ?? text2);
				break;
			}
			case AppearanceSelectionRequestState.Queued:
				PopulateSkinDropdown();
				SetTransformControlsEnabled(enabled: false);
				SetStatus(ModLocalization.Get(ModText.AppearanceQueued), warning: true);
				break;
			case AppearanceSelectionRequestState.Failed:
				PopulateSkinDropdown();
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + (string.IsNullOrWhiteSpace(appearanceSelectionRequestResult.Error) ? string.Empty : (": " + appearanceSelectionRequestResult.Error)), warning: true);
				break;
			}
		}
	}

	private void OnQueuedSelectionFinished(string groupId, bool success, string? error)
	{
		if (_group != null && groupId.Equals(_group.Id, StringComparison.OrdinalIgnoreCase))
		{
			if (success)
			{
				string text = RefreshShopPlayerVisualAfterSelection();
				PopulateSkinDropdown();
				RefreshTargetTitle();
				SyncTransformControls();
				SetTransformControlsEnabled(enabled: true);
				UpdateDragSurfaceCreature();
				SetAppliedStatus(error ?? text);
			}
			else
			{
				PopulateSkinDropdown();
				SetTransformControlsEnabled(enabled: true);
				SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + (string.IsNullOrWhiteSpace(error) ? string.Empty : (": " + error)), warning: true);
			}
		}
	}

	private void SyncTransformControls()
	{
		if (_scaleSlider != null)
		{
			if (_targetCreature != null)
			{
				SetTransformControlValues(CharacterAppearanceRuntime.GetCreatureCombatTransform(_targetCreature));
			}
			else if (_targetShopPlayerVisual != null && _group != null)
			{
				SetTransformControlValues(MerchantRuntimeAppearance.GetLocalPlayerTransform(_group.Id));
			}
		}
	}

	private void RefreshTargetTitle()
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		if (_targetCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetCreature))
		{
			_title.Text = _targetCreature.Entity.Name;
		}
		else if (_targetAncient != null)
		{
			_title.Text = AncientCompendiumEntry.GetTitle(_targetAncient);
		}
		else if (_targetBossMapPoint != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetBossMapPoint))
		{
			_title.Text = _targetBossTitle ?? _group?.DisplayName ?? string.Empty;
		}
		else if (_targetMerchantButton != null)
		{
			_title.Text = new LocString("map", "LEGEND_MERCHANT.title").GetFormattedText();
		}
		else if (_targetShopPlayerVisual != null && _player != null)
		{
			_title.Text = _player.Character.Title.GetFormattedText();
		}
		else
		{
			_title.Text = ModLocalization.Get(ModText.CharacterAppearance);
		}
	}

	[IteratorStateMachine(typeof(_003CEnumerateDescendants_003Ed__108<>))]
	private static IEnumerable<T> EnumerateDescendants<T>(Node root) where T : Node
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateDescendants_003Ed__108<T>(-2)
		{
			_003C_003E3__root = root
		};
	}

	private void OnTransformChanged(bool save = true)
	{
		_scaleValue.Text = $"{Mathf.RoundToInt((float)((Range)_scaleSlider).Value * 100f)}%";
		_healthBarScaleValue.Text = $"{Mathf.RoundToInt((float)((Range)_healthBarScaleSlider).Value * 100f)}%";
		_intentScaleValue.Text = $"{Mathf.RoundToInt((float)((Range)_intentScaleSlider).Value * 100f)}%";
		_selectionReticleScaleValue.Text = $"{Mathf.RoundToInt((float)((Range)_selectionReticleScaleSlider).Value * 100f)}%";
		if (_updating || _group == null || CharacterAppearanceRuntime.GetRequestedOption(_group.Id) != null)
		{
			return;
		}
		try
		{
			if (_targetShopPlayerVisual != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetShopPlayerVisual))
			{
				CharacterCombatTransform characterCombatTransform = MerchantRuntimeAppearance.SetLocalPlayerTransform(_group.Id, ReadTransformControls(), save);
				SetTransformControlValues(characterCombatTransform);
				MerchantRuntimeAppearance.ApplyLocalPlayerTransform(_targetShopPlayerVisual, _group.Id, characterCombatTransform);
			}
			else if (_targetCreature != null)
			{
				CharacterCombatTransform transformControlValues = CharacterAppearanceRuntime.SetCreatureCombatTransform(_targetCreature, ReadTransformControls(), save);
				SetTransformControlValues(transformControlValues);
				if (_transformKey != null)
				{
					CharacterAppearanceRuntime.ApplyTransformToKey(_transformKey);
				}
			}
		}
		catch (Exception ex)
		{
			SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + ex.GetBaseException().Message, warning: true);
		}
	}

	private CharacterCombatTransform ReadTransformControls()
	{
		return new CharacterCombatTransform((float)((Range)_scaleSlider).Value, (float)((Range)_offsetX).Value, (float)((Range)_offsetY).Value)
		{
			HealthBarScale = (float)((Range)_healthBarScaleSlider).Value,
			HealthBarOffsetX = (float)((Range)_healthBarOffsetX).Value,
			HealthBarOffsetY = (float)((Range)_healthBarOffsetY).Value,
			HealthBarFollowsModelScale = ((BaseButton)_healthBarFollowScale).ButtonPressed,
			HealthBarFollowsModelMovement = ((BaseButton)_healthBarFollowMovement).ButtonPressed,
			IntentScale = (float)((Range)_intentScaleSlider).Value,
			IntentOffsetX = (float)((Range)_intentOffsetX).Value,
			IntentOffsetY = (float)((Range)_intentOffsetY).Value,
			IntentFollowsModelScale = ((BaseButton)_intentFollowScale).ButtonPressed,
			IntentFollowsModelMovement = ((BaseButton)_intentFollowMovement).ButtonPressed,
			SelectionReticleScale = (float)((Range)_selectionReticleScaleSlider).Value,
			SelectionReticleOffsetX = (float)((Range)_selectionReticleOffsetX).Value,
			SelectionReticleOffsetY = (float)((Range)_selectionReticleOffsetY).Value,
			SelectionReticleFollowsModelScale = ((BaseButton)_selectionReticleFollowScale).ButtonPressed,
			SelectionReticleFollowsModelMovement = ((BaseButton)_selectionReticleFollowMovement).ButtonPressed
		};
	}

	private void SetTransformControlValues(CharacterCombatTransform value)
	{
		bool updating = _updating;
		_updating = true;
		((Range)_scaleSlider).Value = value.Scale;
		((Range)_offsetX).Value = value.OffsetX;
		((Range)_offsetY).Value = value.OffsetY;
		((Range)_healthBarScaleSlider).Value = value.HealthBarScale;
		((Range)_healthBarOffsetX).Value = value.HealthBarOffsetX;
		((Range)_healthBarOffsetY).Value = value.HealthBarOffsetY;
		((BaseButton)_healthBarFollowScale).ButtonPressed = value.HealthBarFollowsModelScale;
		((BaseButton)_healthBarFollowMovement).ButtonPressed = value.HealthBarFollowsModelMovement;
		((Range)_intentScaleSlider).Value = value.IntentScale;
		((Range)_intentOffsetX).Value = value.IntentOffsetX;
		((Range)_intentOffsetY).Value = value.IntentOffsetY;
		((BaseButton)_intentFollowScale).ButtonPressed = value.IntentFollowsModelScale;
		((BaseButton)_intentFollowMovement).ButtonPressed = value.IntentFollowsModelMovement;
		((Range)_selectionReticleScaleSlider).Value = value.SelectionReticleScale;
		((Range)_selectionReticleOffsetX).Value = value.SelectionReticleOffsetX;
		((Range)_selectionReticleOffsetY).Value = value.SelectionReticleOffsetY;
		((BaseButton)_selectionReticleFollowScale).ButtonPressed = value.SelectionReticleFollowsModelScale;
		((BaseButton)_selectionReticleFollowMovement).ButtonPressed = value.SelectionReticleFollowsModelMovement;
		_scaleValue.Text = $"{Mathf.RoundToInt(value.Scale * 100f)}%";
		_healthBarScaleValue.Text = $"{Mathf.RoundToInt(value.HealthBarScale * 100f)}%";
		_intentScaleValue.Text = $"{Mathf.RoundToInt(value.IntentScale * 100f)}%";
		_selectionReticleScaleValue.Text = $"{Mathf.RoundToInt(value.SelectionReticleScale * 100f)}%";
		_updating = updating;
	}

	private void EnterSelectionMode()
	{
		FinishDrag(save: true);
		EndComparison();
		EndSelectionReticlePreview();
		_selectionMode = true;
		_targetCreature = null;
		_targetAncient = null;
		_targetShopPlayerVisual = null;
		_targetMerchantButton = null;
		_targetFakeMerchant = null;
		_targetBossMapPoint = null;
		_targetBossTitle = null;
		_group = null;
		_transformKey = null;
		_canEditSkin = true;
		_canEditTransform = true;
		((CanvasItem)_panel).Visible = false;
		SetStatus(string.Empty, warning: false);
		UpdateDragSurfaceCreature();
		NBackButton backButton = _backButton;
		if (backButton != null)
		{
			NodeUtil.TryGrabFocus((Control)(object)backButton);
		}
	}

	private bool TrySelectTarget(Vector2 localPosition)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		return (from candidate in GetSelectableTargets().Where(delegate(SelectableAppearanceTarget candidate)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				Rect2 rect = candidate.Rect;
				return ((Rect2)(ref rect)).HasPoint(localPosition);
			})
			orderby candidate.Priority
			select candidate).ThenBy(delegate(SelectableAppearanceTarget candidate)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			Rect2 rect2 = candidate.Rect;
			float x = ((Rect2)(ref rect2)).Size.X;
			rect2 = candidate.Rect;
			return x * ((Rect2)(ref rect2)).Size.Y;
		}).ThenBy(delegate(SelectableAppearanceTarget candidate)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			Rect2 rect3 = candidate.Rect;
			Vector2 center = ((Rect2)(ref rect3)).GetCenter();
			return ((Vector2)(ref center)).DistanceSquaredTo(localPosition);
		}).FirstOrDefault()?.Select(localPosition) ?? false;
	}

	private bool SelectAncientTarget(AncientEventModel ancient, SkinGroup ancientGroup, Vector2 localPosition)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		_selectionMode = false;
		EndSelectionReticlePreview();
		_targetAncient = ancient;
		_targetShopPlayerVisual = null;
		_targetMerchantButton = null;
		_targetFakeMerchant = null;
		_targetBossMapPoint = null;
		_targetBossTitle = null;
		_group = ancientGroup;
		_transformKey = null;
		_canEditSkin = true;
		_canEditTransform = false;
		SetTargetControlsVisible(supportsModel: false, isCreature: false, canSelectSkin: true, supportsIntent: false);
		PositionPanelAwayFrom(localPosition);
		((CanvasItem)_panel).Visible = true;
		RefreshTargetTitle();
		PopulateSkinDropdown();
		SetTransformControlsEnabled(enabled: true);
		UpdateDragSurfaceCreature();
		SetStatus(string.Empty, warning: false);
		NodeUtil.TryGrabFocus(GetTargetInitialFocus());
		return true;
	}

	private bool SelectBossMapTarget(NBossMapPoint bossPoint, Vector2 targetCenter)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (!CharacterAppearanceRuntime.TryGetBossMapAppearance(bossPoint, out SkinGroup group, out string title))
		{
			return false;
		}
		_selectionMode = false;
		EndSelectionReticlePreview();
		_targetCreature = null;
		_targetAncient = null;
		_targetShopPlayerVisual = null;
		_targetMerchantButton = null;
		_targetFakeMerchant = null;
		_targetBossMapPoint = bossPoint;
		_targetBossTitle = title;
		_group = group;
		_transformKey = null;
		_canEditSkin = true;
		_canEditTransform = false;
		SetTargetControlsVisible(supportsModel: false, isCreature: false, canSelectSkin: true, supportsIntent: false);
		PositionPanelAwayFrom(targetCenter);
		((CanvasItem)_panel).Visible = true;
		RefreshTargetTitle();
		PopulateSkinDropdown();
		SetTransformControlsEnabled(enabled: true);
		UpdateDragSurfaceCreature();
		SetStatus(string.Empty, warning: false);
		NodeUtil.TryGrabFocus(GetTargetInitialFocus());
		return true;
	}

	private bool SelectCreatureTarget(NCreature creature, Vector2 targetCenter)
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		if (!CharacterAppearanceRuntime.TryGetCreatureAppearance(creature, out CreatureAppearanceBinding binding))
		{
			return false;
		}
		_selectionMode = false;
		EndSelectionReticlePreview();
		_targetCreature = creature;
		_targetAncient = null;
		_targetShopPlayerVisual = null;
		_targetMerchantButton = null;
		_targetFakeMerchant = null;
		_targetBossMapPoint = null;
		_targetBossTitle = null;
		_group = binding.Group;
		_transformKey = binding.TransformKey;
		_canEditSkin = binding.CanSelectSkin && MultiplayerSkinSync.CanEditSkinForCreature(creature.Entity);
		_canEditTransform = MultiplayerSkinSync.CanEditTransformForCreature(creature.Entity);
		SetTargetControlsVisible(supportsModel: true, binding.SupportsCombatControls, binding.CanSelectSkin, binding.SupportsIntent);
		PositionPanelAwayFrom(targetCenter);
		((CanvasItem)_panel).Visible = true;
		RefreshTargetTitle();
		PopulateSkinDropdown();
		SyncTransformControls();
		SetTransformControlsEnabled(enabled: true);
		UpdateDragSurfaceCreature();
		BeginSelectionReticlePreview(creature);
		RefreshStatusForCurrentContext();
		NodeUtil.TryGrabFocus(GetTargetInitialFocus());
		return true;
	}

	private bool SelectShopPlayerTarget(NMerchantCharacter visual, Vector2 targetCenter)
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		if (_player == null)
		{
			return false;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)_player.Character).Id.Entry, ((object)_player.Character).GetType().Name);
		if (skinGroup == null)
		{
			return false;
		}
		MerchantRuntimeAppearance.ApplyLocalPlayerTransform(visual, skinGroup.Id);
		_selectionMode = false;
		EndSelectionReticlePreview();
		_targetCreature = null;
		_targetAncient = null;
		_targetShopPlayerVisual = visual;
		_targetMerchantButton = null;
		_targetFakeMerchant = null;
		_targetBossMapPoint = null;
		_targetBossTitle = null;
		_group = skinGroup;
		_transformKey = null;
		_canEditSkin = MultiplayerSkinSync.CanEditLocalPlayerSkinInRun();
		_canEditTransform = true;
		SetTargetControlsVisible(supportsModel: true, isCreature: false, canSelectSkin: true, supportsIntent: false);
		PositionPanelAwayFrom(targetCenter);
		((CanvasItem)_panel).Visible = true;
		RefreshTargetTitle();
		PopulateSkinDropdown();
		SyncTransformControls();
		SetTransformControlsEnabled(enabled: true);
		UpdateDragSurfaceCreature();
		SetStatus(string.Empty, warning: false);
		NodeUtil.TryGrabFocus(GetTargetInitialFocus());
		return true;
	}

	private bool SelectMerchantTarget(NMerchantButton merchantButton, Vector2 targetCenter, string groupId = "merchant", NFakeMerchant? fakeMerchant = null)
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		SkinGroup skinGroup = SkinService.Catalog?.Groups.FirstOrDefault((SkinGroup candidate) => candidate.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase));
		if (skinGroup == null)
		{
			return false;
		}
		_selectionMode = false;
		EndSelectionReticlePreview();
		_targetCreature = null;
		_targetAncient = null;
		_targetShopPlayerVisual = null;
		_targetMerchantButton = merchantButton;
		_targetFakeMerchant = fakeMerchant;
		_targetBossMapPoint = null;
		_targetBossTitle = null;
		_group = skinGroup;
		_transformKey = null;
		_canEditSkin = true;
		_canEditTransform = false;
		SetTargetControlsVisible(supportsModel: false, isCreature: false, canSelectSkin: true, supportsIntent: false);
		PositionPanelAwayFrom(targetCenter);
		((CanvasItem)_panel).Visible = true;
		RefreshTargetTitle();
		PopulateSkinDropdown();
		SetTransformControlsEnabled(enabled: true);
		UpdateDragSurfaceCreature();
		SetStatus(string.Empty, warning: false);
		NodeUtil.TryGrabFocus(GetTargetInitialFocus());
		return true;
	}

	private void RequestFakeMerchantSkinSelection(string optionId)
	{
		if (_group == null || _targetFakeMerchant == null)
		{
			return;
		}
		string selection = SkinService.Config.GetSelection(_group.Id);
		if (!SkinService.ApplySelection(_group.Id, optionId))
		{
			PopulateSkinDropdown();
			SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + SkinService.LastError, warning: true);
			return;
		}
		PopulateSkinDropdown();
		if (!MerchantRuntimeAppearance.TryRefreshFakeMerchant(_targetFakeMerchant, out string error))
		{
			string text = error;
			bool flag = SkinService.ApplySelection(_group.Id, selection);
			string error2 = null;
			bool num = flag && MerchantRuntimeAppearance.TryRefreshFakeMerchant(_targetFakeMerchant, out error2);
			PopulateSkinDropdown();
			_targetMerchantButton = _targetFakeMerchant.MerchantButton;
			if (!num)
			{
				ModLog.Error("回滚假商人皮肤选择失败：" + (flag ? error2 : SkinService.LastError));
			}
			SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + text, warning: true);
		}
		else
		{
			_targetMerchantButton = _targetFakeMerchant.MerchantButton;
			SetAppliedStatus(null);
		}
	}

	private string? RefreshShopPlayerVisualAfterSelection()
	{
		if (_targetShopPlayerVisual == null || _player == null || _group == null)
		{
			return null;
		}
		if (!MerchantRuntimeAppearance.TryRefreshLocalPlayer(_player, _group.Id, out NMerchantCharacter refreshedVisual, out string error))
		{
			return error;
		}
		_targetShopPlayerVisual = refreshedVisual;
		return null;
	}

	private void SetTargetControlsVisible(bool supportsModel, bool isCreature, bool canSelectSkin, bool supportsIntent)
	{
		foreach (CanvasItem skinControl in _skinControls)
		{
			skinControl.Visible = canSelectSkin;
		}
		foreach (CanvasItem modelControl in _modelControls)
		{
			modelControl.Visible = supportsModel;
		}
		foreach (CanvasItem creatureOnlyControl in _creatureOnlyControls)
		{
			creatureOnlyControl.Visible = isCreature;
		}
		foreach (CanvasItem intentOnlyControl in _intentOnlyControls)
		{
			intentOnlyControl.Visible = isCreature && supportsIntent;
		}
		foreach (CanvasItem selectionReticleOnlyControl in _selectionReticleOnlyControls)
		{
			selectionReticleOnlyControl.Visible = isCreature && supportsIntent;
		}
		((CanvasItem)_modelSpacer).Visible = supportsModel && canSelectSkin;
		ResizePanelToContent();
	}

	private void ResizePanelToContent()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		Callable val = Callable.From((Action)delegate
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			if (GodotObject.IsInstanceValid((GodotObject)(object)_settingsScroll) && GodotObject.IsInstanceValid((GodotObject)(object)_settingsContent))
			{
				Rect2 viewportRect = ((CanvasItem)this).GetViewportRect();
				float num = Math.Max(300f, ((Rect2)(ref viewportRect)).Size.Y * 0.9f - 48f);
				float y = ((Control)_settingsContent).GetCombinedMinimumSize().Y;
				float num2 = Math.Min(y, num);
				((Control)_settingsScroll).CustomMinimumSize = new Vector2(460f, num2);
				_settingsScroll.VerticalScrollMode = (ScrollMode)((y > num) ? 1 : 0);
				((Control)_panel).ResetSize();
			}
		});
		((Callable)(ref val)).CallDeferred(Array.Empty<Variant>());
	}

	private Control GetTargetInitialFocus()
	{
		if (!((CanvasItem)_skinDropdown).Visible)
		{
			return (Control)(object)_scaleSlider;
		}
		return (Control)(object)_skinDropdown;
	}

	private void PositionPanelAwayFrom(Vector2 targetCenter)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		bool flag = targetCenter.X <= Math.Max(1f, ((Control)_dragSurface).Size.X) * 0.5f;
		((Control)_panel).AnchorLeft = (flag ? 0.61f : 0.02f);
		((Control)_panel).AnchorRight = (flag ? 0.98f : 0.39f);
		((Control)_panel).OffsetLeft = (flag ? (-10f) : 0f);
		((Control)_panel).OffsetRight = (flag ? 0f : 10f);
		((Control)_panel).AnchorTop = 0.075f;
		((Control)_panel).AnchorBottom = 0.075f;
		((Control)_panel).GrowVertical = (GrowDirection)1;
	}

	private void ResetModelTransform()
	{
		if (_targetCreature != null || _targetShopPlayerVisual != null)
		{
			EndComparison();
			CharacterCombatTransform transformControlValues = CharacterTransformResetPolicy.ResetModel(ReadTransformControls());
			SetTransformControlValues(transformControlValues);
			OnTransformChanged();
		}
	}

	private void RestoreLocalPlayerModelTransform()
	{
		FinishDrag(save: true);
		NCreature currentCreature = CharacterAppearanceRuntime.GetCurrentCreature(_player);
		if (currentCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)currentCreature))
		{
			CharacterCombatTransform creatureCombatTransform = CharacterAppearanceRuntime.GetCreatureCombatTransform(currentCreature);
			CharacterAppearanceRuntime.SetCreatureCombatTransform(currentCreature, CharacterTransformResetPolicy.ResetModel(creatureCombatTransform));
			CharacterAppearanceRuntime.ApplyStoredTransform(currentCreature);
			UpdateDragSurfaceCreature();
			return;
		}
		NMerchantCharacter localPlayerVisual = MerchantRuntimeAppearance.GetLocalPlayerVisual();
		if (_player == null || localPlayerVisual == null || !GodotObject.IsInstanceValid((GodotObject)(object)localPlayerVisual))
		{
			UpdateDragSurfaceCreature();
			return;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)_player.Character).Id.Entry, ((object)_player.Character).GetType().Name);
		if (skinGroup == null)
		{
			UpdateDragSurfaceCreature();
			return;
		}
		CharacterCombatTransform value = MerchantRuntimeAppearance.SetLocalPlayerTransform(skinGroup.Id, CharacterTransformResetPolicy.ResetModel(MerchantRuntimeAppearance.GetLocalPlayerTransform(skinGroup.Id)), save: true);
		MerchantRuntimeAppearance.ApplyLocalPlayerTransform(localPlayerVisual, skinGroup.Id, value);
		UpdateDragSurfaceCreature();
	}

	private void ResetHealthBarTransform()
	{
		if (_targetCreature != null)
		{
			EndComparison();
			CharacterCombatTransform transformControlValues = ReadTransformControls()with
			{
				HealthBarScale = 1f,
				HealthBarOffsetX = 0f,
				HealthBarOffsetY = 0f,
				HealthBarFollowsModelScale = false,
				HealthBarFollowsModelMovement = true
			};
			SetTransformControlValues(transformControlValues);
			OnTransformChanged();
		}
	}

	private void ResetIntentTransform()
	{
		if (_targetCreature != null)
		{
			EndComparison();
			CharacterCombatTransform transformControlValues = ReadTransformControls()with
			{
				IntentScale = 1f,
				IntentOffsetX = 0f,
				IntentOffsetY = 0f,
				IntentFollowsModelScale = false,
				IntentFollowsModelMovement = true
			};
			SetTransformControlValues(transformControlValues);
			OnTransformChanged();
		}
	}

	private void ResetSelectionReticleTransform()
	{
		if (_targetCreature != null)
		{
			EndComparison();
			CharacterCombatTransform transformControlValues = ReadTransformControls()with
			{
				SelectionReticleScale = 1f,
				SelectionReticleOffsetX = 0f,
				SelectionReticleOffsetY = 0f,
				SelectionReticleFollowsModelScale = true,
				SelectionReticleFollowsModelMovement = true
			};
			SetTransformControlValues(transformControlValues);
			OnTransformChanged();
		}
	}

	private void BeginSelectionReticlePreview(NCreature? creature)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		if (_selectionMode || !CharacterAppearanceRuntime.SupportsIntentAppearance(creature))
		{
			return;
		}
		NSelectionReticle selectionReticle = CharacterAppearanceRuntime.GetSelectionReticle(creature);
		if (selectionReticle != null && GodotObject.IsInstanceValid((GodotObject)(object)selectionReticle) && selectionReticle != _previewSelectionReticle)
		{
			EndSelectionReticlePreview();
			_previewSelectionReticle = selectionReticle;
			_previewSelectionReticleWasSelected = selectionReticle.IsSelected;
			_previewSelectionReticleModulate = (selectionReticle.IsSelected ? ((CanvasItem)selectionReticle).Modulate : Colors.Transparent);
			_previewSelectionReticleScale = (selectionReticle.IsSelected ? ((Control)selectionReticle).Scale : Vector2.One);
			CharacterAppearanceRuntime.StopSelectionReticleAnimation(selectionReticle);
			if (!selectionReticle.IsSelected)
			{
				((CanvasItem)selectionReticle).Modulate = Colors.White;
				((Control)selectionReticle).Scale = Vector2.One;
			}
		}
	}

	private void EndSelectionReticlePreview()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (_previewSelectionReticle != null && GodotObject.IsInstanceValid((GodotObject)(object)_previewSelectionReticle) && !_previewSelectionReticleWasSelected && !_previewSelectionReticle.IsSelected)
		{
			((CanvasItem)_previewSelectionReticle).Modulate = _previewSelectionReticleModulate;
			((Control)_previewSelectionReticle).Scale = _previewSelectionReticleScale;
		}
		_previewSelectionReticle = null;
		_previewSelectionReticleWasSelected = false;
	}

	private void BeginComparison()
	{
		if (!_comparing && _targetCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetCreature))
		{
			_comparing = true;
			CharacterAppearanceRuntime.ApplyPreviewTransform(_targetCreature, new CharacterCombatTransform());
		}
	}

	private void EndComparison()
	{
		if (_comparing)
		{
			_comparing = false;
			if (_targetCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetCreature))
			{
				CharacterAppearanceRuntime.ApplyStoredTransform(_targetCreature);
			}
		}
	}

	private void UpdateDragSurfaceCreature()
	{
		NCreature val = ((!_selectionMode && _targetCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetCreature)) ? _targetCreature : null);
		_dragSurface.SetCreature(val);
		NMerchantCharacter val2 = ((!_selectionMode && _targetShopPlayerVisual != null && GodotObject.IsInstanceValid((GodotObject)(object)_targetShopPlayerVisual)) ? _targetShopPlayerVisual : null);
		_dragSurface.SetShopPlayerVisual(val2);
		_dragSurface.SetSelectionMode(_selectionMode);
		_dragSurface.SetDragEnabled(_canEditTransform && (val != null || val2 != null));
		bool selectionMode = _selectionMode;
		bool flag = _canEditTransform && (val != null || val2 != null);
		((CanvasItem)_selectionHint).Visible = selectionMode;
		((CanvasItem)_hint).Visible = flag;
		((CanvasItem)_restorePlayerButton).Visible = _selectionMode && CanRestoreLocalPlayerModelTransform();
		RefreshHintPulse(_selectionHint, selectionMode, ref _selectionHintPulseTween);
		RefreshHintPulse(_hint, flag, ref _hintPulseTween);
		((BaseButton)_compareButton).Disabled = val == null;
		RefreshDragHint();
		UpdateSelectionHintRefresh();
	}

	private bool CanRestoreLocalPlayerModelTransform()
	{
		NCreature currentCreature = CharacterAppearanceRuntime.GetCurrentCreature(_player);
		if (currentCreature != null && GodotObject.IsInstanceValid((GodotObject)(object)currentCreature))
		{
			return CharacterTransformResetPolicy.NeedsModelReset(CharacterAppearanceRuntime.GetCreatureCombatTransform(currentCreature));
		}
		NMerchantCharacter localPlayerVisual = MerchantRuntimeAppearance.GetLocalPlayerVisual();
		if (_player == null || localPlayerVisual == null || !GodotObject.IsInstanceValid((GodotObject)(object)localPlayerVisual))
		{
			return false;
		}
		SkinGroup skinGroup = ContextualSkinControls.FindGroup(((AbstractModel)_player.Character).Id.Entry, ((object)_player.Character).GetType().Name);
		if (skinGroup != null)
		{
			return CharacterTransformResetPolicy.NeedsModelReset(MerchantRuntimeAppearance.GetLocalPlayerTransform(skinGroup.Id));
		}
		return false;
	}

	private void StopHintPulseAnimations()
	{
		Timer selectionHintTimer = _selectionHintTimer;
		if (selectionHintTimer != null)
		{
			selectionHintTimer.Stop();
		}
		StopHintPulse(_selectionHint, ref _selectionHintPulseTween);
		StopHintPulse(_hint, ref _hintPulseTween);
	}

	private void RefreshHintPulse(Label label, bool shouldShow, ref Tween? tween)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		if (!shouldShow || !_hintLayer.Visible || !((CanvasItem)this).IsVisibleInTree())
		{
			StopHintPulse(label, ref tween);
		}
		else if (tween == null || !tween.IsValid() || !tween.IsRunning())
		{
			((Control)label).PivotOffset = ((Control)label).Size * 0.5f;
			((CanvasItem)label).SelfModulate = Colors.White;
			Tween val = ((Node)label).CreateTween().SetProcessMode((TweenProcessMode)1).SetPauseMode((TweenPauseMode)2)
				.SetIgnoreTimeScale(true);
			val.SetLoops(0);
			val.TweenProperty((GodotObject)(object)label, NodePath.op_Implicit("self_modulate:a"), Variant.op_Implicit(0.38f), 0.72).SetTrans((TransitionType)1).SetEase((EaseType)2);
			val.Chain().TweenProperty((GodotObject)(object)label, NodePath.op_Implicit("self_modulate:a"), Variant.op_Implicit(1f), 0.72).SetTrans((TransitionType)1)
				.SetEase((EaseType)2);
			tween = val;
		}
	}

	private static void StopHintPulse(Label label, ref Tween? tween)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (tween != null && tween.IsValid())
		{
			tween.Kill();
		}
		tween = null;
		if (GodotObject.IsInstanceValid((GodotObject)(object)label))
		{
			((CanvasItem)label).SelfModulate = Colors.White;
			((Control)label).Scale = Vector2.One;
		}
	}

	private void RefreshDragHint()
	{
		ModText text = (CharacterAppearanceRuntime.SupportsIntentAppearance(_targetCreature) ? ModText.DirectDragIntentHint : ModText.DirectDragHint);
		_hint.Text = ModLocalization.Get(text);
	}

	private void OnDragSurfaceInput(InputEvent inputEvent)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I8
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
		if (val != null && (long)val.ButtonIndex == 1)
		{
			if (val.Pressed)
			{
				if (_selectionMode)
				{
					if (TrySelectTarget(((InputEventMouse)val).Position))
					{
						((Control)_dragSurface).AcceptEvent();
					}
				}
				else if (_dragSurface.DragEnabled)
				{
					CharacterAppearanceDragTarget characterAppearanceDragTarget = _dragSurface.HitTest(((InputEventMouse)val).Position);
					if (characterAppearanceDragTarget != 0)
					{
						EndComparison();
						_dragTarget = characterAppearanceDragTarget;
						_dragStartPosition = ((InputEventMouse)val).Position;
						_dragStartOffset = (Vector2)(characterAppearanceDragTarget switch
						{
							CharacterAppearanceDragTarget.Model => new Vector2((float)((Range)_offsetX).Value, (float)((Range)_offsetY).Value), 
							CharacterAppearanceDragTarget.HealthBar => new Vector2((float)((Range)_healthBarOffsetX).Value, (float)((Range)_healthBarOffsetY).Value), 
							CharacterAppearanceDragTarget.Intent => new Vector2((float)((Range)_intentOffsetX).Value, (float)((Range)_intentOffsetY).Value), 
							CharacterAppearanceDragTarget.SelectionReticle => new Vector2((float)((Range)_selectionReticleOffsetX).Value, (float)((Range)_selectionReticleOffsetY).Value), 
							_ => Vector2.Zero, 
						});
						((Control)_dragSurface).GrabClickFocus();
						((Control)_dragSurface).AcceptEvent();
					}
				}
			}
			else if (_dragTarget != 0)
			{
				FinishDrag(save: true);
				((Control)_dragSurface).AcceptEvent();
			}
			return;
		}
		InputEventMouseMotion val2 = (InputEventMouseMotion)(object)((inputEvent is InputEventMouseMotion) ? inputEvent : null);
		if (val2 != null && _dragTarget != 0)
		{
			Vector2 targetLocalDelta = _dragSurface.GetTargetLocalDelta(_dragStartPosition, ((InputEventMouse)val2).Position);
			_updating = true;
			switch (_dragTarget)
			{
			case CharacterAppearanceDragTarget.Model:
				((Range)_offsetX).Value = _dragStartOffset.X + targetLocalDelta.X;
				((Range)_offsetY).Value = _dragStartOffset.Y + targetLocalDelta.Y;
				break;
			case CharacterAppearanceDragTarget.HealthBar:
				((Range)_healthBarOffsetX).Value = _dragStartOffset.X + targetLocalDelta.X;
				((Range)_healthBarOffsetY).Value = _dragStartOffset.Y + targetLocalDelta.Y;
				break;
			case CharacterAppearanceDragTarget.Intent:
				((Range)_intentOffsetX).Value = _dragStartOffset.X + targetLocalDelta.X;
				((Range)_intentOffsetY).Value = _dragStartOffset.Y + targetLocalDelta.Y;
				break;
			case CharacterAppearanceDragTarget.SelectionReticle:
				((Range)_selectionReticleOffsetX).Value = _dragStartOffset.X + targetLocalDelta.X;
				((Range)_selectionReticleOffsetY).Value = _dragStartOffset.Y + targetLocalDelta.Y;
				break;
			}
			_updating = false;
			OnTransformChanged(save: false);
			((Control)_dragSurface).AcceptEvent();
		}
	}

	private void FinishDrag(bool save)
	{
		if (_dragTarget != 0)
		{
			_dragTarget = CharacterAppearanceDragTarget.None;
			if (save)
			{
				OnTransformChanged();
			}
		}
	}

	private void RefreshStatusForCurrentContext()
	{
		if (_status != null)
		{
			if (!_selectionMode && _targetCreature != null && _group != null && CharacterAppearanceRuntime.GetRequestedOption(_group.Id) != null)
			{
				SetTransformControlsEnabled(enabled: false);
				SetStatus(ModLocalization.Get(ModText.AppearanceQueued), warning: true);
			}
			else
			{
				SetTransformControlsEnabled(!_selectionMode);
				SetStatus(string.Empty, warning: false);
			}
		}
	}

	private void SetTransformControlsEnabled(bool enabled)
	{
		bool flag = enabled && _canEditTransform;
		bool flag2 = enabled && _canEditSkin;
		((Slider)_scaleSlider).Editable = flag;
		_offsetX.Editable = flag;
		_offsetY.Editable = flag;
		((Slider)_healthBarScaleSlider).Editable = flag;
		_healthBarOffsetX.Editable = flag;
		_healthBarOffsetY.Editable = flag;
		((BaseButton)_healthBarFollowScale).Disabled = !flag;
		((BaseButton)_healthBarFollowMovement).Disabled = !flag;
		((Slider)_intentScaleSlider).Editable = flag;
		_intentOffsetX.Editable = flag;
		_intentOffsetY.Editable = flag;
		((BaseButton)_intentFollowScale).Disabled = !flag;
		((BaseButton)_intentFollowMovement).Disabled = !flag;
		((Slider)_selectionReticleScaleSlider).Editable = flag;
		_selectionReticleOffsetX.Editable = flag;
		_selectionReticleOffsetY.Editable = flag;
		((BaseButton)_selectionReticleFollowScale).Disabled = !flag;
		((BaseButton)_selectionReticleFollowMovement).Disabled = !flag;
		((BaseButton)_skinDropdown).Disabled = !flag2 || _group == null || !((CanvasItem)_skinDropdown).Visible;
		_dragSurface.SetDragEnabled(flag && (_targetCreature != null || _targetShopPlayerVisual != null));
		((BaseButton)_skinResetButton).Disabled = !flag2 || !((CanvasItem)_skinResetButton).Visible;
		((BaseButton)_modelResetButton).Disabled = !flag;
		((BaseButton)_healthBarResetButton).Disabled = !flag;
		((BaseButton)_intentResetButton).Disabled = !flag || !((CanvasItem)_intentResetButton).Visible;
		((BaseButton)_selectionReticleResetButton).Disabled = !flag || !((CanvasItem)_selectionReticleResetButton).Visible;
		((BaseButton)_compareButton).Disabled = !flag || _targetCreature == null;
	}

	private void SetStatus(string text, bool warning)
	{
		_status.Text = text;
		((CanvasItem)_status).Visible = !string.IsNullOrWhiteSpace(text);
		ModThemeRuntime.Bind((Node)(object)_status, "status", delegate(ModThemeSettings theme)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			((CanvasItem)_status).Modulate = Colors.White;
			((Control)_status).AddThemeColorOverride(StringName.op_Implicit("font_color"), warning ? new Color(theme.AccentColor) : new Color("b8e6c0"));
		});
	}

	private void SetAppliedStatus(string? liveRefreshError)
	{
		if (string.IsNullOrWhiteSpace(liveRefreshError))
		{
			SetStatus(string.Empty, warning: false);
		}
		else
		{
			SetStatus(ModLocalization.Get(ModText.AppearanceFailed) + ": " + liveRefreshError, warning: true);
		}
	}

	private static SpinBox BuildOffsetSpinBox()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected O, but got Unknown
		SpinBox val = new SpinBox
		{
			MinValue = -1000.0,
			MaxValue = 1000.0,
			Step = 1.0,
			CustomMinimumSize = new Vector2(125f, 44f),
			SizeFlagsHorizontal = (SizeFlags)3,
			AllowGreater = false,
			AllowLesser = false,
			UpdateOnTextChanged = true,
			Suffix = " px"
		};
		ModThemeRuntime.Input(val.GetLineEdit(), 20);
		return val;
	}

	private static Label BuildSectionLabel()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		Label obj = BuildLabel(string.Empty, 23, (HorizontalAlignment)0);
		((Control)obj).CustomMinimumSize = new Vector2(0f, 38f);
		((Control)obj).SizeFlagsHorizontal = (SizeFlags)3;
		ModThemeRuntime.AccentText((Control)(object)obj);
		return obj;
	}

	private static HBoxContainer BuildSectionHeader(out Label label, out Button resetButton)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Expected O, but got Unknown
		HBoxContainer val = new HBoxContainer
		{
			CustomMinimumSize = new Vector2(0f, 40f)
		};
		((Control)val).AddThemeConstantOverride(StringName.op_Implicit("separation"), 12);
		label = BuildSectionLabel();
		resetButton = BuildButton(string.Empty);
		((Control)resetButton).CustomMinimumSize = new Vector2(78f, 34f);
		((Control)resetButton).SizeFlagsHorizontal = (SizeFlags)8;
		((Control)resetButton).SizeFlagsVertical = (SizeFlags)4;
		ApplyTextTheme((Control)(object)resetButton, 16);
		((Node)val).AddChild((Node)(object)label, false, (InternalMode)0);
		((Node)val).AddChild((Node)(object)resetButton, false, (InternalMode)0);
		return val;
	}

	private static Control BuildVerticalSpacer(float height)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		return new Control
		{
			CustomMinimumSize = new Vector2(0f, height),
			MouseFilter = (MouseFilterEnum)2
		};
	}

	private static CheckButton BuildCheckButton()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_002b: Expected O, but got Unknown
		CheckButton val = new CheckButton
		{
			CustomMinimumSize = new Vector2(0f, 40f),
			MouseDefaultCursorShape = (CursorShape)2
		};
		ApplyTextTheme((Control)val, 18);
		return val;
	}

	private static Label BuildLabel(string text, int fontSize, HorizontalAlignment alignment = 0L)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_002b: Expected O, but got Unknown
		Label val = new Label
		{
			Text = text,
			HorizontalAlignment = alignment,
			VerticalAlignment = (VerticalAlignment)1,
			MouseFilter = (MouseFilterEnum)2
		};
		ApplyTextTheme((Control)val, fontSize);
		return val;
	}

	private static Button BuildButton(string text)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		//IL_00b8: Expected O, but got Unknown
		Button val = new Button
		{
			Text = text,
			CustomMinimumSize = new Vector2(0f, 48f),
			MouseDefaultCursorShape = (CursorShape)2
		};
		ApplyTextTheme((Control)val, 19);
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("normal"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("3c5f82"), new Color("7394ad")));
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("hover"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("4b7392"), new Color("afcdde")));
		((Control)val).AddThemeStyleboxOverride(StringName.op_Implicit("pressed"), (StyleBox)(object)ContextualSkinControls.CreateStyleBox(new Color("45104e"), new Color("efc850"), 2));
		ModThemeRuntime.Button(val, 19);
		return val;
	}

	private static void ApplyTextTheme(Control control, int fontSize)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		control.AddThemeColorOverride(StringName.op_Implicit("font_color"), new Color("fff6e2"));
		control.AddThemeColorOverride(StringName.op_Implicit("font_hover_color"), Colors.White);
		control.AddThemeColorOverride(StringName.op_Implicit("font_pressed_color"), new Color("efc850"));
		control.AddThemeColorOverride(StringName.op_Implicit("font_outline_color"), new Color("332f27"));
		control.AddThemeConstantOverride(StringName.op_Implicit("outline_size"), 4);
		control.AddThemeFontSizeOverride(StringName.op_Implicit("font_size"), fontSize);
		ModThemeRuntime.TextControl(control, fontSize);
		if (ContextualSkinControls.GameFont != null)
		{
			control.AddThemeFontOverride(StringName.op_Implicit("font"), ContextualSkinControls.GameFont);
		}
	}

	private IEnumerable<SelectableAppearanceTarget> GetSelectableTargets()
	{
		return EnumerateSelectableTargets().Where(delegate(SelectableAppearanceTarget target)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			Rect2 rect = target.Rect;
			return ((Rect2)(ref rect)).Intersects(new Rect2(Vector2.Zero, ((Control)_dragSurface).Size), false);
		});
	}

	[IteratorStateMachine(typeof(_003CEnumerateSelectableTargets_003Ed__157))]
	private IEnumerable<SelectableAppearanceTarget> EnumerateSelectableTargets()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateSelectableTargets_003Ed__157(-2)
		{
			_003C_003E4__this = this
		};
	}

	private void RefreshSelectionHint()
	{
		if (_selectionMode && _selectionHint != null && ((CanvasItem)this).IsVisibleInTree())
		{
			string text = ModLocalization.FormatAppearanceTargetHint(from target in GetSelectableTargets()
				select target.Kind);
			if (_selectionHint.Text != text)
			{
				_selectionHint.Text = text;
			}
		}
	}

	private void UpdateSelectionHintRefresh()
	{
		if (_selectionMode && ((CanvasItem)this).IsVisibleInTree())
		{
			RefreshSelectionHint();
			if (_selectionHintTimer.IsStopped())
			{
				_selectionHintTimer.Start(-1.0);
			}
		}
		else
		{
			_selectionHintTimer.Stop();
		}
	}
}

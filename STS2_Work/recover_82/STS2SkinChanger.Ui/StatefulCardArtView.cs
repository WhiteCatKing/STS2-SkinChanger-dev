using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.addons.mega_text;
using STS2SkinChanger.Catalog;
using STS2SkinChanger.Core;

namespace STS2SkinChanger.Ui;

internal static class StatefulCardArtView
{
	private sealed class Lease
	{
		private readonly NCard _card;

		private readonly List<Node> _added = new List<Node>();

		private readonly Dictionary<CanvasItem, (bool Visible, Color Modulate)> _items = new Dictionary<CanvasItem, (bool, Color)>();

		private readonly Dictionary<Control, float> _rotations = new Dictionary<Control, float>();

		private readonly Dictionary<MegaLabel, string> _labels = new Dictionary<MegaLabel, string>();

		private bool _animationsAttached;

		private bool _watcherAttached;

		private StatefulCardArtWatcher? _watcher;

		internal CardModel Model { get; }

		internal string Provider { get; }

		internal StatefulCardArtRuntime.Runtime Runtime { get; }

		internal bool Simple { get; }

		internal Lease(NCard card, StatefulCardArtRuntime.Runtime runtime, string provider)
		{
			_card = card;
			Model = card.Model;
			Runtime = runtime;
			Provider = provider;
			Simple = runtime.Simple;
		}

		private void Remember(CanvasItem? item)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (item != null && !_items.ContainsKey(item))
			{
				_items[item] = (item.Visible, item.Modulate);
			}
		}

		private void Show(string path, bool visible)
		{
			CanvasItem nodeOrNull = ((Node)_card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit(path));
			Remember(nodeOrNull);
			if (nodeOrNull != null)
			{
				nodeOrNull.Visible = visible;
			}
		}

		private void Tint(CanvasItem? item, Color color)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			Remember(item);
			if (item != null)
			{
				item.Modulate = color;
			}
		}

		internal void Restore()
		{
			foreach (KeyValuePair<CanvasItem, (bool, Color)> item4 in _items)
			{
				CanvasItem item;
				(bool Visible, Color Modulate) value;
				(item, value) = (KeyValuePair<CanvasItem, (bool, Color)>)(ref item4);
				TryRestore(delegate
				{
					//IL_002f: Unknown result type (might be due to invalid IL or missing references)
					if (GodotObject.IsInstanceValid((GodotObject)(object)item))
					{
						item.Visible = value.Visible;
						item.Modulate = value.Modulate;
					}
				});
			}
			foreach (KeyValuePair<MegaLabel, string> label in _labels)
			{
				var (item2, value2) = (KeyValuePair<MegaLabel, string>)(ref label);
				TryRestore(delegate
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)item2))
					{
						item2.SetTextAutoSize(value2);
					}
				});
			}
			foreach (KeyValuePair<Control, float> rotation in _rotations)
			{
				var (item3, value3) = (KeyValuePair<Control, float>)(ref rotation);
				TryRestore(delegate
				{
					if (GodotObject.IsInstanceValid((GodotObject)(object)item3))
					{
						item3.Rotation = value3;
					}
				});
			}
			_items.Clear();
			_labels.Clear();
			_rotations.Clear();
		}

		private static void TryRestore(Action restore)
		{
			try
			{
				restore();
			}
			catch (Exception exception)
			{
				StatefulCardArtRuntime.Warn("restore card node", exception);
			}
		}

		internal void Apply(ExternalCardVisualOwnership external, CardPreviewMode preview)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Invalid comparison between Unknown and I4
			//IL_017e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_0185: Unknown result type (might be due to invalid IL or missing references)
			//IL_0188: Invalid comparison between Unknown and I4
			//IL_018a: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Invalid comparison between Unknown and I4
			//IL_03f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			if (!_watcherAttached)
			{
				_watcherAttached = true;
				StatefulCardArtWatcher obj = new StatefulCardArtWatcher
				{
					Card = _card,
					Runtime = Runtime,
					Provider = Provider
				};
				StatefulCardArtWatcher statefulCardArtWatcher = obj;
				_watcher = obj;
				StatefulCardArtWatcher statefulCardArtWatcher2 = statefulCardArtWatcher;
				_added.Add((Node)(object)statefulCardArtWatcher2);
				((Node)_card).AddChild((Node)(object)statefulCardArtWatcher2, false, (InternalMode)0);
			}
			if (_watcher != null)
			{
				_watcher.Preview = preview;
			}
			if (!external.Text)
			{
				MegaLabel nodeOrNull = ((Node)_card).GetNodeOrNull<MegaLabel>(NodePath.op_Implicit("%TitleLabel"));
				if (Runtime.Setting("HideTitle", fallback: true) && nodeOrNull != null)
				{
					_labels.TryAdd(nodeOrNull, ((Label)nodeOrNull).Text);
					nodeOrNull.SetTextAutoSize((!Model.IsUpgraded) ? "" : ((Model.MaxUpgradeLevel <= 1) ? "+" : $"+{Model.CurrentUpgradeLevel}"));
				}
				if (Runtime.Setting("HideStars", fallback: true))
				{
					Show("%StarIcon", visible: false);
				}
			}
			if (!external.Frame)
			{
				CardModel val;
				if (Runtime.Setting("HideType", fallback: true))
				{
					val = (CardModel)(object)ModelDb.Card<Apotheosis>();
				}
				else
				{
					CardModel val2;
					if ((int)Model.Rarity == 5)
					{
						val2 = Model;
					}
					else
					{
						CardType type = Model.Type;
						CardModel val3 = (CardModel)(((int)type == 1) ? ModelDb.Card<NeowsFury>() : (((int)type != 3) ? ((object)ModelDb.Card<Apotheosis>()) : ((object)ModelDb.Card<Corruption>())));
						val2 = val3;
					}
					val = val2;
				}
				CardModel obj2 = val;
				TextureRect nodeOrNull2 = ((Node)_card).GetNodeOrNull<TextureRect>(NodePath.op_Implicit("%AncientTextBg"));
				string text = AccessTools.Property(typeof(CardModel), "AncientTextBgPath")?.GetValue(obj2) as string;
				if (nodeOrNull2 != null && text != null)
				{
					nodeOrNull2.Texture = SkinService.LoadCardPresentationResource<Texture2D>(Model, text);
				}
				string text2 = AccessTools.Property(typeof(CardModel), "BannerMaterialPath")?.GetValue(obj2) as string;
				CanvasItem nodeOrNull3 = ((Node)_card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("%AncientBanner"));
				if (nodeOrNull3 != null && text2 != null)
				{
					nodeOrNull3.Material = SkinService.LoadCardPresentationResource<Material>(Model, text2);
				}
				if (!external.Text)
				{
					Show("%AncientTextBg", !Runtime.Setting("HideDescription", fallback: true));
					Show("%AncientBorderGlassOverlay", !Runtime.Setting("HideDescription", fallback: true));
				}
				string[] array = new string[2] { "%TitleBanner", "%AncientBanner" };
				foreach (string text3 in array)
				{
					CanvasItem nodeOrNull4 = ((Node)_card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit(text3));
					if (nodeOrNull4 != null)
					{
						Tint(nodeOrNull4, new Color(nodeOrNull4.Modulate, 0.7f));
					}
				}
			}
			if (Simple || external.Frame || external.Portrait)
			{
				return;
			}
			if (!_animationsAttached)
			{
				_animationsAttached = true;
				AttachAnimations();
			}
			if (((object)Model).GetType().Name == "Alignment")
			{
				_rotations.TryAdd(_card.Body, _card.Body.Rotation);
				_card.Body.Rotation = _rotations[_card.Body] + Mathf.DegToRad(-15f);
				return;
			}
			string name = ((object)Model).GetType().Name;
			if ((name == "Glow" || name == "Luminesce") ? true : false)
			{
				ApplyGlow(preview);
			}
		}

		private void ApplyGlow(CardPreviewMode preview)
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01af: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ca: Invalid comparison between Unknown and I4
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015d: Unknown result type (might be due to invalid IL or missing references)
			//IL_015f: Invalid comparison between Unknown and I4
			Type type = Runtime.Assembly.GetType("NCardExtensions");
			if (type == null)
			{
				return;
			}
			HashSet<Node> before = Descendants((Node)(object)_card).ToHashSet();
			Color color = ((((object)Model).GetType().Name == "Glow") ? new Color(1f, 0.8f, 0.2f, 1f) : new Color(0.314f, 0.784f, 0.471f, 1f));
			string[] array = new string[2] { "AssertUncommonGlow", "AssertRareGlow" };
			foreach (string text in array)
			{
				object? obj = AccessTools.Field(typeof(NCard), (text == "AssertRareGlow") ? "_rareGlow" : "_uncommonGlow")?.GetValue(_card);
				Remember((CanvasItem?)((obj is CanvasItem) ? obj : null));
				object? obj2 = AccessTools.Method(type, text, (Type[])null, (Type[])null)?.Invoke(null, new object[1] { _card });
				CanvasItem val = (CanvasItem)((obj2 is CanvasItem) ? obj2 : null);
				if (val != null)
				{
					Tint(val, color);
					val.Visible = ((object)Model).GetType().Name == "Glow" || Model.IsUpgraded || (int)preview == 2;
				}
			}
			if (((object)Model).GetType().Name == "Luminesce")
			{
				CanvasItem nodeOrNull = ((Node)_card).GetNodeOrNull<CanvasItem>(NodePath.op_Implicit("CardContainer/CardSparkles"));
				Tint(nodeOrNull, color);
				if (nodeOrNull != null)
				{
					nodeOrNull.Visible = Model.IsUpgraded || (int)preview == 2;
				}
			}
			_added.AddRange(from node in Descendants((Node)(object)_card)
				where !before.Contains(node) && before.Contains(node.GetParent())
				select node);
		}

		private void AttachAnimations()
		{
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Expected O, but got Unknown
			string name = ((object)Model).GetType().Name;
			if ((name == "PullAggro" || name == "Putrefy") ? true : false)
			{
				string resourcePath = Runtime.Contract.ResourceRoot + "/scenes/cards/" + ((name == "PullAggro") ? "osty_dance" : "bad_apple") + ".tscn";
				PackedScene val = SkinService.LoadCardPresentationResource<PackedScene>(Model, resourcePath);
				if (val != null)
				{
					Node val2 = val.Instantiate((GenEditState)0);
					_added.Add(val2);
					if (name == "Putrefy")
					{
						AccessTools.Method(((object)val2).GetType(), "SetCard", (Type[])null, (Type[])null).Invoke(val2, new object[1] { _card });
						((Node)_card).AddChild(val2, false, (InternalMode)0);
					}
					else
					{
						AccessTools.Field(((object)val2).GetType(), "card").SetValue(val2, _card);
						((Node)((Node)_card).GetNode<CanvasGroup>(NodePath.op_Implicit("%PortraitCanvasGroup"))).AddChild(val2, false, (InternalMode)0);
					}
				}
			}
			else if (name == "InfiniteBlades")
			{
				Type type2 = Runtime.Assembly.GetTypes().Single((Type type) => type.Name == "InfiniteInfiniteBlades");
				Control val3 = (Control)Activator.CreateInstance(type2);
				_added.Add((Node)(object)val3);
				AccessTools.Property(type2, "CardNode").SetValue(val3, _card);
				((Node)_card.Body).AddChild((Node)(object)val3, false, (InternalMode)0);
				((Node)_card.Body).MoveChild((Node)(object)val3, ((Node)((Node)_card).GetNode<CanvasGroup>(NodePath.op_Implicit("%PortraitCanvasGroup"))).GetIndex(false) + 1);
			}
		}

		internal void Dispose()
		{
			Restore();
			foreach (Node item in _added.Distinct())
			{
				if (!GodotObject.IsInstanceValid((GodotObject)(object)item) || ((GodotObject)item).IsQueuedForDeletion())
				{
					continue;
				}
				string[] array = new string[2] { "_rareGlow", "_uncommonGlow" };
				foreach (string text in array)
				{
					FieldInfo fieldInfo = AccessTools.Field(typeof(NCard), text);
					if (fieldInfo?.GetValue(_card) == item)
					{
						fieldInfo.SetValue(_card, null);
					}
				}
				item.ProcessMode = (ProcessModeEnum)4;
				CanvasItem val = (CanvasItem)(object)((item is CanvasItem) ? item : null);
				if (val != null)
				{
					val.Visible = false;
				}
				item.QueueFree();
			}
			_added.Clear();
		}
	}

	[CompilerGenerated]
	private sealed class _003CDescendants_003Ed__5 : IEnumerable<Node>, IEnumerable, IEnumerator<Node>, IEnumerator, IDisposable
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
		public _003CDescendants_003Ed__5(int _003C_003E1__state)
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
			_003CDescendants_003Ed__5 _003CDescendants_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescendants_003Ed__ = this;
			}
			else
			{
				_003CDescendants_003Ed__ = new _003CDescendants_003Ed__5(0);
			}
			_003CDescendants_003Ed__.root = _003C_003E3__root;
			return _003CDescendants_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<Node>)this).GetEnumerator();
		}
	}

	private static readonly ConditionalWeakTable<NCard, Lease> Leases = new ConditionalWeakTable<NCard, Lease>();

	internal static void BeforeRefresh(NCard card, bool reload)
	{
		StatefulCardArtRuntime.Bind(card);
		if (Leases.TryGetValue(card, out Lease value))
		{
			value.Restore();
			CardSkinOption cardSkinOption = ((card.Model == null) ? null : SkinService.GetSelectedCardOption(card.Model));
			if (reload || card.Model != value.Model || cardSkinOption?.ProviderId != value.Provider || !value.Runtime.Enabled || value.Simple != value.Runtime.Simple)
			{
				Release(card);
			}
		}
	}

	internal static void Release(NCard card)
	{
		if (!Leases.TryGetValue(card, out Lease value))
		{
			return;
		}
		Leases.Remove(card);
		try
		{
			value.Dispose();
		}
		catch (Exception exception)
		{
			StatefulCardArtRuntime.Warn("release card node", exception);
		}
	}

	internal static void Apply(NCard card, ExternalCardVisualOwnership external, CardPreviewMode preview = 1)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		StatefulCardArtRuntime.Bind(card);
		if (card.Model == null || !((Node)card).IsInsideTree() || !((Node)card).IsNodeReady())
		{
			return;
		}
		CardSkinOption selectedCardOption = SkinService.GetSelectedCardOption(card.Model);
		StatefulCardArtRuntime.Runtime runtime = StatefulCardArtRuntime.For(selectedCardOption);
		if (runtime == null || !runtime.Enabled)
		{
			Release(card);
			return;
		}
		try
		{
			if (!Leases.TryGetValue(card, out Lease value))
			{
				value = new Lease(card, runtime, selectedCardOption.ProviderId);
				Leases.Add(card, value);
			}
			value.Apply(external, preview);
		}
		catch (Exception exception)
		{
			Release(card);
			StatefulCardArtRuntime.Warn("node " + ((object)card.Model).GetType().Name, exception);
		}
	}

	[IteratorStateMachine(typeof(_003CDescendants_003Ed__5))]
	private static IEnumerable<Node> Descendants(Node root)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescendants_003Ed__5(-2)
		{
			_003C_003E3__root = root
		};
	}
}

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Godot;
using STS2SkinChanger.Catalog;

namespace STS2SkinChanger.Core;

internal static class PartTakeoverVisualScope
{
	private sealed record OwnedPart(string OptionId, IReadOnlySet<string> Namespaces);

	private sealed class RoomVisualState
	{
		public string GroupId { get; set; } = string.Empty;

		public Dictionary<ulong, NodeVisualState> Nodes { get; } = new Dictionary<ulong, NodeVisualState>();

		public Dictionary<ulong, PackageNodeState> PackageNodes { get; } = new Dictionary<ulong, PackageNodeState>();

		public Dictionary<ulong, MerchantFigureState> Figures { get; } = new Dictionary<ulong, MerchantFigureState>();

		public HashSet<ulong> RoomVoiceNodes { get; } = new HashSet<ulong>();

		public HashSet<ulong> PackageVoiceNodes { get; } = new HashSet<ulong>();
	}

	private sealed class MerchantFigureState(WeakReference<Node> button)
	{
		public WeakReference<Node> Button { get; } = button;

		public WeakReference<Node>? Figure { get; set; }

		public bool FigureVisibleAtRest { get; set; } = true;

		public bool HasLayout { get; set; }

		public ControlLayout Layout { get; set; }
	}

	private readonly record struct ControlLayout(Vector2 Position, Vector2 Size, Vector2 PivotOffset)
	{
		public static ControlLayout Of(Control control)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			return new ControlLayout(control.Position, control.Size, control.PivotOffset);
		}

		public bool Matches(Control control)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (Position == control.Position && Size == control.Size)
			{
				return PivotOffset == control.PivotOffset;
			}
			return false;
		}

		public void ApplyTo(Control control)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			control.Position = Position;
			control.Size = Size;
			control.PivotOffset = PivotOffset;
		}

		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			builder.Append("Position = ");
			Vector2 val = Position;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", Size = ");
			val = Size;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			builder.Append(", PivotOffset = ");
			val = PivotOffset;
			builder.Append(((object)(Vector2)(ref val)/*cast due to .constrained prefix*/).ToString());
			return true;
		}
	}

	private sealed class NodeVisualState(WeakReference<Node> node)
	{
		public WeakReference<Node> Node { get; } = node;

		public Dictionary<string, Texture2D?> Baselines { get; } = new Dictionary<string, Texture2D>(StringComparer.Ordinal);

		public Dictionary<string, OwnedPart> AppliedOwners { get; } = new Dictionary<string, OwnedPart>(StringComparer.Ordinal);

		public Dictionary<string, Texture2D?> Applied { get; } = new Dictionary<string, Texture2D>(StringComparer.Ordinal);
	}

	private sealed class PackageNodeState(WeakReference<Node> node, OwnedPart owner, bool originalVisible)
	{
		public WeakReference<Node> Node { get; } = node;

		public OwnedPart Owner { get; } = owner;

		public bool OriginalVisible { get; } = originalVisible;
	}

	private sealed class ReconcileCounters
	{
		public int Restored { get; set; }

		public int Hidden { get; set; }

		public int Reapplied { get; set; }

		public int Reattached { get; set; }

		public int Silenced { get; set; }

		public int FiguresShown { get; set; }

		public int FiguresHidden { get; set; }

		public int LayoutRestored { get; set; }

		public bool Any
		{
			get
			{
				if (Restored <= 0 && Hidden <= 0 && Reapplied <= 0 && Reattached <= 0 && Silenced <= 0 && FiguresShown <= 0 && FiguresHidden <= 0)
				{
					return LayoutRestored > 0;
				}
				return true;
			}
		}
	}

	private readonly record struct TextureSlot(PropertyInfo Property)
	{
		public string Name => Property.Name;

		public object? GetValue(Node node)
		{
			return Property.GetValue(node);
		}

		public void SetValue(Node node, Texture2D? texture)
		{
			Property.SetValue(node, texture);
		}
	}

	[CompilerGenerated]
	private sealed class _003CDescribeAncestry_003Ed__22 : IEnumerable<(string TypeName, string NodeName)>, IEnumerable, IEnumerator<(string TypeName, string NodeName)>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private (string TypeName, string NodeName) _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Node node;

		public Node _003C_003E3__node;

		private Node _003Ccurrent_003E5__2;

		(string TypeName, string NodeName) IEnumerator<(string, string)>.Current
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
		public _003CDescribeAncestry_003Ed__22(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Ccurrent_003E5__2 = null;
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
				_003Ccurrent_003E5__2 = node;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ccurrent_003E5__2 = _003Ccurrent_003E5__2.GetParent();
				break;
			}
			if (_003Ccurrent_003E5__2 != null)
			{
				_003C_003E2__current = (TypeName: ((object)_003Ccurrent_003E5__2).GetType().Name, NodeName: ((object)_003Ccurrent_003E5__2.Name).ToString());
				_003C_003E1__state = 1;
				return true;
			}
			_003Ccurrent_003E5__2 = null;
			return false;
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
		IEnumerator<(string TypeName, string NodeName)> IEnumerable<(string, string)>.GetEnumerator()
		{
			_003CDescribeAncestry_003Ed__22 _003CDescribeAncestry_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CDescribeAncestry_003Ed__ = this;
			}
			else
			{
				_003CDescribeAncestry_003Ed__ = new _003CDescribeAncestry_003Ed__22(0);
			}
			_003CDescribeAncestry_003Ed__.node = _003C_003E3__node;
			return _003CDescribeAncestry_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<(string, string)>)this).GetEnumerator();
		}
	}

	private static readonly ConditionalWeakTable<Node, RoomVisualState> Rooms = new ConditionalWeakTable<Node, RoomVisualState>();

	private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyInfo>> TextureSlots = new ConcurrentDictionary<Type, IReadOnlyList<PropertyInfo>>();

	private static readonly string[] TexturePropertyNames = new string[2] { "Texture", "TextureNormal" };

	public static void Capture(Node root, string groupId)
	{
		try
		{
			RoomVisualState state = Rooms.GetOrCreateValue(root);
			state.GroupId = groupId;
			state.RoomVoiceNodes.UnionWith(AudioPlayerIds(root));
			Walk(root, delegate(Node node)
			{
				Remember(state, node);
			});
		}
		catch (Exception ex)
		{
			ModLog.Warn("记录商人外观的原始画面失败：" + ex.GetBaseException().Message);
		}
	}

	public static void Reconcile(Node root, string groupId)
	{
		try
		{
			IReadOnlyList<OwnedPart> parts = ResolveParts(groupId);
			if (parts.Count != 0)
			{
				RoomVisualState state = Rooms.GetOrCreateValue(root);
				state.GroupId = groupId;
				ReconcileCounters counters = new ReconcileCounters();
				Walk(root, delegate(Node node)
				{
					ReconcileNode(state, node, parts, counters);
				});
				ReconcileMerchantFigure(state, parts, counters);
				ReconcileMerchantFeedback(state, root, parts, counters);
				if (counters.Any)
				{
					ModLog.Info($"商人外观已按当前 {groupId} 选择对齐：还原 {counters.Restored} 处原版画面、隐藏 {counters.Hidden} 处部件自带画面、恢复 {counters.Reapplied} 处部件画面（其中重新挂载部件自己的商人呈现 {counters.Reattached} 次）、原版商人画面显示 {counters.FiguresShown} 处/隐藏 {counters.FiguresHidden} 处、还原作者位置 {counters.LayoutRestored} 处、停用部件语音 {counters.Silenced} 处；商店房间与商店界面保持作者原样。");
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("按选择对齐商人外观失败，保留作者画面：" + ex.GetBaseException().Message);
		}
	}

	private static void ReconcileMerchantFeedback(RoomVisualState state, Node root, IReadOnlyList<OwnedPart> parts, ReconcileCounters counters)
	{
		if (parts.Any((OwnedPart part) => IsPartSelected(state.GroupId, part)))
		{
			ManagedSkinModLoader.RestorePartTakeoverMerchantFeedback();
			counters.Reattached += ManagedSkinModLoader.ReplayPartTakeoverRoomPresentation(root);
		}
		else
		{
			ManagedSkinModLoader.SuspendPartTakeoverMerchantFeedback();
			counters.Silenced += SilencePackageVoice(state, root);
		}
	}

	public static void RememberMerchantFigure(Node merchantButton)
	{
		try
		{
			Node val = ResolveRoomRoot(merchantButton);
			if (val != null)
			{
				RoomVisualState orCreateValue = Rooms.GetOrCreateValue(val);
				ulong instanceId = ((GodotObject)merchantButton).GetInstanceId();
				if (!orCreateValue.Figures.TryGetValue(instanceId, out MerchantFigureState value) || (value.Button.TryGetTarget(out Node target) && !GodotObject.IsInstanceValid((GodotObject)(object)target)))
				{
					value = new MerchantFigureState(new WeakReference<Node>(merchantButton));
					orCreateValue.Figures[instanceId] = value;
				}
				Node nodeOrNull = merchantButton.GetNodeOrNull(NodePath.op_Implicit("%MerchantVisual"));
				CanvasItem val2 = (CanvasItem)(object)((nodeOrNull is CanvasItem) ? nodeOrNull : null);
				if (val2 != null)
				{
					value.Figure = new WeakReference<Node>((Node)(object)val2);
					value.FigureVisibleAtRest = val2.Visible;
				}
				Control val3 = (Control)(object)((merchantButton is Control) ? merchantButton : null);
				if (val3 != null)
				{
					value.Layout = ControlLayout.Of(val3);
					value.HasLayout = true;
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("记录商人本体画面失败：" + ex.GetBaseException().Message);
		}
	}

	private static void ReconcileMerchantFigure(RoomVisualState state, IReadOnlyList<OwnedPart> parts, ReconcileCounters counters)
	{
		if (state.Figures.Count == 0)
		{
			return;
		}
		bool partSelected = parts.Any((OwnedPart part) => IsPartSelected(state.GroupId, part));
		KeyValuePair<ulong, MerchantFigureState>[] array = state.Figures.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<ulong, MerchantFigureState> keyValuePair = array[i];
			var (key, merchantFigureState2) = (KeyValuePair<ulong, MerchantFigureState>)(ref keyValuePair);
			if (!merchantFigureState2.Button.TryGetTarget(out Node target) || !GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				state.Figures.Remove(key);
				continue;
			}
			WeakReference<Node>? figure = merchantFigureState2.Figure;
			Node target2;
			CanvasItem val = (CanvasItem)(object)((figure != null && figure.TryGetTarget(out target2) && GodotObject.IsInstanceValid((GodotObject)(object)target2)) ? ((target2 is CanvasItem) ? target2 : null) : null);
			switch (MerchantFigureScopePolicy.ResolveFigure(partSelected, IsPackageFigureVisible(state, target), merchantFigureState2.FigureVisibleAtRest, val != null && val.Visible))
			{
			case MerchantFigureScopePolicy.FigureAction.ShowGameFigure:
				val.Visible = true;
				counters.FiguresShown++;
				break;
			case MerchantFigureScopePolicy.FigureAction.HideGameFigure:
				val.Visible = false;
				counters.FiguresHidden++;
				break;
			}
			Control val2 = (Control)(object)((target is Control) ? target : null);
			if (val2 != null && merchantFigureState2.HasLayout && MerchantFigureScopePolicy.ShouldRestoreLayout(partSelected, !merchantFigureState2.Layout.Matches(val2)))
			{
				merchantFigureState2.Layout.ApplyTo(val2);
				counters.LayoutRestored++;
			}
		}
	}

	private static bool IsPackageFigureVisible(RoomVisualState state, Node button)
	{
		foreach (PackageNodeState value in state.PackageNodes.Values)
		{
			if (value.Node.TryGetTarget(out Node target) && GodotObject.IsInstanceValid((GodotObject)(object)target))
			{
				CanvasItem val = (CanvasItem)(object)((target is CanvasItem) ? target : null);
				if (val != null && val.Visible && IsPartSelected(state.GroupId, value.Owner) && IsDescendantOf(target, button))
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool SelectedPartDrawsFigure(Node root)
	{
		try
		{
			if (!Rooms.TryGetValue(root, out RoomVisualState value))
			{
				return false;
			}
			foreach (MerchantFigureState value2 in value.Figures.Values)
			{
				if (value2.Button.TryGetTarget(out Node target) && GodotObject.IsInstanceValid((GodotObject)(object)target) && IsPackageFigureVisible(value, target))
				{
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			ModLog.Warn("检查部件是否自己画了商人失败：" + ex.GetBaseException().Message);
		}
		return false;
	}

	private static bool IsDescendantOf(Node node, Node ancestor)
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			if (parent == ancestor)
			{
				return true;
			}
		}
		return false;
	}

	private static Node? ResolveRoomRoot(Node node)
	{
		for (Node parent = node.GetParent(); parent != null; parent = parent.GetParent())
		{
			string name = ((object)parent).GetType().Name;
			if (name.Contains("MerchantRoom", StringComparison.Ordinal) || name.Contains("FakeMerchant", StringComparison.Ordinal))
			{
				return parent;
			}
		}
		return null;
	}

	private static int SilencePackageVoice(RoomVisualState state, Node root)
	{
		int silenced = 0;
		Walk(root, delegate(Node node)
		{
			if (IsAudioPlayer(node))
			{
				ulong instanceId = ((GodotObject)node).GetInstanceId();
				if (!state.RoomVoiceNodes.Contains(instanceId) && state.PackageVoiceNodes.Add(instanceId))
				{
					try
					{
						AudioStreamPlayer val = (AudioStreamPlayer)(object)((node is AudioStreamPlayer) ? node : null);
						if (val != null)
						{
							val.Stop();
						}
						node.QueueFree();
						silenced++;
					}
					catch (Exception ex)
					{
						ModLog.Warn("停用部件自带商人语音失败：" + ex.GetBaseException().Message);
					}
				}
			}
		});
		return silenced;
	}

	private static IEnumerable<ulong> AudioPlayerIds(Node root)
	{
		List<ulong> ids = new List<ulong>();
		Walk(root, delegate(Node node)
		{
			if (IsAudioPlayer(node))
			{
				ids.Add(((GodotObject)node).GetInstanceId());
			}
		});
		return ids;
	}

	private static bool IsAudioPlayer(Node node)
	{
		return ((GodotObject)node).GetClass().ToString().StartsWith("AudioStreamPlayer", StringComparison.Ordinal);
	}

	private static IReadOnlyList<OwnedPart> ResolveParts(string groupId)
	{
		SkinCatalog catalog = SkinService.Catalog;
		if (catalog == null)
		{
			return Array.Empty<OwnedPart>();
		}
		List<OwnedPart> list = new List<OwnedPart>();
		foreach (SkinGroup group in catalog.Groups)
		{
			if (!group.Id.Equals(groupId, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			foreach (SkinOption item in group.Options.Where((SkinOption option) => option.IsBaselinePartOverride))
			{
				list.Add(new OwnedPart(item.Id, ResolveNamespaces(item)));
			}
		}
		return list;
	}

	private static IReadOnlySet<string> ResolveNamespaces(SkinOption option)
	{
		HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { option.Id };
		if (option.ProviderRootPath == null)
		{
			return hashSet;
		}
		try
		{
			foreach (string item in Directory.EnumerateFiles(option.ProviderRootPath, "*.dll", SearchOption.TopDirectoryOnly))
			{
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(item);
				if (!string.IsNullOrWhiteSpace(fileNameWithoutExtension))
				{
					hashSet.Add(fileNameWithoutExtension);
				}
			}
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException) ? 1 : 0) != 0)
		{
			ModLog.Warn("读取部件接管包 " + option.Id + " 的程序集名失败，仅使用清单 ID：" + ex.Message);
		}
		return hashSet;
	}

	private static void ReconcileNode(RoomVisualState state, Node node, IReadOnlyList<OwnedPart> parts, ReconcileCounters counters)
	{
		if (!IsInScope(node))
		{
			return;
		}
		if (!state.Nodes.TryGetValue(((GodotObject)node).GetInstanceId(), out NodeVisualState value))
		{
			OwnedPart ownedPart = ResolveNodeOwner(node, parts);
			if ((object)ownedPart != null)
			{
				ApplyAddedNode(state, node, ownedPart, counters);
			}
			else
			{
				Remember(state, node);
			}
			return;
		}
		if (value.Node.TryGetTarget(out Node target) && !GodotObject.IsInstanceValid((GodotObject)(object)target))
		{
			state.Nodes.Remove(((GodotObject)node).GetInstanceId());
			Remember(state, node);
			return;
		}
		foreach (TextureSlot item in ResolveTextureSlots(node))
		{
			object? value2 = item.GetValue(node);
			Texture2D val = (Texture2D)((value2 is Texture2D) ? value2 : null);
			Texture2D valueOrDefault = value.Baselines.GetValueOrDefault(item.Name);
			OwnedPart valueOrDefault2 = value.AppliedOwners.GetValueOrDefault(item.Name);
			Texture2D valueOrDefault3 = value.Applied.GetValueOrDefault(item.Name);
			if (val != valueOrDefault)
			{
				if (valueOrDefault != null)
				{
					OwnedPart ownedPart2 = ResolveOwner((val != null) ? ((Resource)val).ResourcePath : null, parts);
					if ((object)ownedPart2 != null)
					{
						value.AppliedOwners[item.Name] = ownedPart2;
						value.Applied[item.Name] = val;
						if (!IsPartSelected(state.GroupId, ownedPart2))
						{
							item.SetValue(node, valueOrDefault);
							counters.Restored++;
						}
						continue;
					}
				}
				if (valueOrDefault == null)
				{
					value.Baselines[item.Name] = val;
				}
			}
			else if (valueOrDefault2 != null && valueOrDefault3 != null && valueOrDefault3 != valueOrDefault && IsPartSelected(state.GroupId, valueOrDefault2))
			{
				item.SetValue(node, valueOrDefault3);
				counters.Reapplied++;
			}
		}
	}

	private static void ApplyAddedNode(RoomVisualState state, Node node, OwnedPart owner, ReconcileCounters counters)
	{
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		if (!state.PackageNodes.TryGetValue(instanceId, out PackageNodeState value) || (value.Node.TryGetTarget(out Node target) && !GodotObject.IsInstanceValid((GodotObject)(object)target)))
		{
			WeakReference<Node> node2 = new WeakReference<Node>(node);
			CanvasItem val = (CanvasItem)(object)((node is CanvasItem) ? node : null);
			value = new PackageNodeState(node2, owner, val == null || val.Visible);
			state.PackageNodes[instanceId] = value;
		}
		CanvasItem val2 = (CanvasItem)(object)((node is CanvasItem) ? node : null);
		if (val2 == null)
		{
			return;
		}
		bool flag = IsPartSelected(state.GroupId, value.Owner) && value.OriginalVisible;
		if (val2.Visible != flag)
		{
			val2.Visible = flag;
			if (flag)
			{
				counters.Reapplied++;
			}
			else
			{
				counters.Hidden++;
			}
		}
	}

	private static bool IsPartSelected(string groupId, OwnedPart part)
	{
		return SkinService.GetDisplayedSelection(groupId).Equals(part.OptionId, StringComparison.OrdinalIgnoreCase);
	}

	private static void Remember(RoomVisualState state, Node node)
	{
		if (!IsInScope(node))
		{
			return;
		}
		ulong instanceId = ((GodotObject)node).GetInstanceId();
		if (state.Nodes.ContainsKey(instanceId))
		{
			return;
		}
		NodeVisualState nodeVisualState = new NodeVisualState(new WeakReference<Node>(node));
		foreach (TextureSlot item in ResolveTextureSlots(node))
		{
			Dictionary<string, Texture2D?> baselines = nodeVisualState.Baselines;
			string name = item.Name;
			object? value = item.GetValue(node);
			baselines[name] = (Texture2D)((value is Texture2D) ? value : null);
		}
		state.Nodes[instanceId] = nodeVisualState;
	}

	private static bool IsInScope(Node node)
	{
		return MerchantAppearanceScopePolicy.IsMerchantAppearance(DescribeAncestry(node));
	}

	[IteratorStateMachine(typeof(_003CDescribeAncestry_003Ed__22))]
	private static IEnumerable<(string TypeName, string NodeName)> DescribeAncestry(Node node)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDescribeAncestry_003Ed__22(-2)
		{
			_003C_003E3__node = node
		};
	}

	private static OwnedPart? ResolveNodeOwner(Node node, IReadOnlyList<OwnedPart> parts)
	{
		foreach (TextureSlot item in ResolveTextureSlots(node))
		{
			object? value = item.GetValue(node);
			object? obj = ((value is Texture2D) ? value : null);
			OwnedPart ownedPart = ResolveOwner((obj != null) ? ((Resource)obj).ResourcePath : null, parts);
			if ((object)ownedPart != null)
			{
				return ownedPart;
			}
		}
		for (Node val = node; val != null; val = val.GetParent())
		{
			OwnedPart ownedPart2 = ResolveOwner(val.SceneFilePath, parts);
			if ((object)ownedPart2 != null)
			{
				return ownedPart2;
			}
		}
		return null;
	}

	private static OwnedPart? ResolveOwner(string? resourcePath, IReadOnlyList<OwnedPart> parts)
	{
		if (string.IsNullOrWhiteSpace(resourcePath))
		{
			return null;
		}
		string text;
		if (!resourcePath.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
		{
			text = resourcePath;
		}
		else
		{
			int length = "res://".Length;
			text = resourcePath.Substring(length, resourcePath.Length - length);
		}
		string text2 = text;
		int num = text2.IndexOf('/');
		string head = ((num < 0) ? text2 : text2.Substring(0, num));
		if (head.Length != 0)
		{
			return parts.FirstOrDefault((OwnedPart part) => part.Namespaces.Contains(head));
		}
		return null;
	}

	private static IReadOnlyList<TextureSlot> ResolveTextureSlots(Node node)
	{
		IReadOnlyList<PropertyInfo> orAdd = TextureSlots.GetOrAdd(((object)node).GetType(), (Type type) => (from name in TexturePropertyNames
			select type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public) into property
			where (object)property != null && property.CanRead && property.CanWrite && typeof(Texture2D).IsAssignableFrom(property.PropertyType)
			select property).Cast<PropertyInfo>().ToArray());
		if (orAdd.Count != 0)
		{
			List<TextureSlot> list = new List<TextureSlot>();
			list.AddRange(orAdd.Select((PropertyInfo property) => new TextureSlot(property)));
			return new _003C_003Ez__ReadOnlyList<TextureSlot>(list);
		}
		return Array.Empty<TextureSlot>();
	}

	private static void Walk(Node root, Action<Node> visit)
	{
		Stack<Node> stack = new Stack<Node>();
		stack.Push(root);
		while (stack.Count > 0)
		{
			Node val = stack.Pop();
			visit(val);
			foreach (Node child in val.GetChildren(false))
			{
				if (child != null)
				{
					Node item = child;
					stack.Push(item);
				}
			}
		}
	}
}

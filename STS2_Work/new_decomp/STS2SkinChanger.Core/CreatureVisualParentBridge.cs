using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace STS2SkinChanger.Core;

internal static class CreatureVisualParentBridge
{
	private sealed class Binding
	{
		public readonly WeakReference<Node> Wrapper = new WeakReference<Node>(wrapper);

		public readonly WeakReference<NCreature> Owner = new WeakReference<NCreature>(owner);

		public Binding(Node wrapper, NCreature owner)
		{
		}
	}

	[CompilerGenerated]
	private sealed class _003CRewrite_003Ed__6 : IEnumerable<CodeInstruction>, IEnumerable, IEnumerator<CodeInstruction>, IEnumerator, IDisposable
	{
		private int _003C_003E1__state;

		private CodeInstruction _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<CodeInstruction> instructions;

		public IEnumerable<CodeInstruction> _003C_003E3__instructions;

		private CodeInstruction[] _003Cbody_003E5__2;

		private int _003Cindex_003E5__3;

		CodeInstruction IEnumerator<CodeInstruction>.Current
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
		public _003CRewrite_003Ed__6(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Environment.CurrentManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cbody_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Expected O, but got Unknown
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Cbody_003E5__2 = instructions.ToArray();
				_003Cindex_003E5__3 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				goto IL_00d7;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_00d7;
				}
				IL_00d7:
				_003Cindex_003E5__3++;
				break;
			}
			if (_003Cindex_003E5__3 < _003Cbody_003E5__2.Length)
			{
				CodeInstruction val = _003Cbody_003E5__2[_003Cindex_003E5__3];
				if (!IsOwnerLookup(_003Cbody_003E5__2, _003Cindex_003E5__3))
				{
					_003C_003E2__current = val;
					_003C_003E1__state = 1;
					return true;
				}
				MethodInfo methodInfo = (MethodInfo)val.operand;
				_003C_003E2__current = new CodeInstruction(val)
				{
					opcode = OpCodes.Call,
					operand = AccessTools.Method(typeof(CreatureVisualParentBridge), methodInfo.IsGenericMethod ? "GetCreatureParent" : "GetParent", (Type[])null, (Type[])null)
				};
				_003C_003E1__state = 2;
				return true;
			}
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
		IEnumerator<CodeInstruction> IEnumerable<CodeInstruction>.GetEnumerator()
		{
			_003CRewrite_003Ed__6 _003CRewrite_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Environment.CurrentManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CRewrite_003Ed__ = this;
			}
			else
			{
				_003CRewrite_003Ed__ = new _003CRewrite_003Ed__6(0);
			}
			_003CRewrite_003Ed__.instructions = _003C_003E3__instructions;
			return _003CRewrite_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CodeInstruction>)this).GetEnumerator();
		}
	}

	private static readonly ConditionalWeakTable<NCreatureVisuals, Binding> Owners = new ConditionalWeakTable<NCreatureVisuals, Binding>();

	private static readonly HashSet<Assembly> Inspected = new HashSet<Assembly>();

	private static readonly Harmony Patches = new Harmony("Gurio.SkinChanger.visual_parent");

	internal static void Register(NCreature creature, Node2D wrapper)
	{
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Expected O, but got Unknown
		NCreatureVisuals visuals = creature.Visuals;
		Owners.Remove(visuals);
		Owners.Add(visuals, new Binding((Node)(object)wrapper, creature));
		Assembly assembly = ((object)visuals).GetType().Assembly;
		if (assembly == typeof(NCreatureVisuals).Assembly || assembly == typeof(Node).Assembly || assembly == typeof(CreatureVisualParentBridge).Assembly || !Inspected.Add(assembly))
		{
			return;
		}
		int num = 0;
		Type[] source;
		try
		{
			source = assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			source = ex.Types.OfType<Type>().ToArray();
		}
		foreach (MethodInfo item in source.SelectMany(GetMethods))
		{
			try
			{
				if (!item.ContainsGenericParameters && item.GetMethodBody() != null)
				{
					List<CodeInstruction> body = PatchProcessor.GetOriginalInstructions((MethodBase)item, (ILGenerator)null);
					if (Enumerable.Range(0, body.Count).Any((int index) => IsOwnerLookup(body, index)))
					{
						Patches.Patch((MethodBase)item, (HarmonyMethod)null, (HarmonyMethod)null, new HarmonyMethod(typeof(CreatureVisualParentBridge), "Rewrite", (Type[])null), (HarmonyMethod)null);
						num++;
					}
				}
			}
			catch (Exception ex2)
			{
				ModLog.Warn($"生物父节点兼容未应用 {item.DeclaringType?.FullName}.{item.Name}：{ex2.GetBaseException().Message}");
			}
		}
		if (num > 0)
		{
			ModLog.Info($"已保留自定义生物的模型父节点契约：{assembly.GetName().Name}，{num} 个查询入口。");
		}
	}

	private static IEnumerable<MethodInfo> GetMethods(Type type)
	{
		try
		{
			return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}
		catch
		{
			return Array.Empty<MethodInfo>();
		}
	}

	private static bool IsOwnerLookup(IReadOnlyList<CodeInstruction> body, int index)
	{
		CodeInstruction val = body[index];
		if ((val.opcode != OpCodes.Call && val.opcode != OpCodes.Callvirt) || !(val.operand is MethodInfo methodInfo) || methodInfo.DeclaringType != typeof(Node) || methodInfo.Name != "GetParent" || methodInfo.GetParameters().Length != 0)
		{
			return false;
		}
		if (methodInfo.IsGenericMethod)
		{
			Type[] genericArguments = methodInfo.GetGenericArguments();
			if (genericArguments != null && genericArguments.Length == 1)
			{
				Type type = genericArguments[0];
				return type == typeof(NCreature);
			}
			return false;
		}
		int i;
		for (i = index + 1; i < body.Count && body[i].opcode == OpCodes.Nop; i++)
		{
		}
		if (i < body.Count && (body[i].opcode == OpCodes.Isinst || body[i].opcode == OpCodes.Castclass))
		{
			return object.Equals(body[i].operand, typeof(NCreature));
		}
		return false;
	}

	[IteratorStateMachine(typeof(_003CRewrite_003Ed__6))]
	internal static IEnumerable<CodeInstruction> Rewrite(IEnumerable<CodeInstruction> instructions)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CRewrite_003Ed__6(-2)
		{
			_003C_003E3__instructions = instructions
		};
	}

	private static NCreature GetCreatureParent(Node node)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		return (NCreature)GetParent(node);
	}

	private static Node GetParent(Node node)
	{
		Node parent = node.GetParent();
		NCreatureVisuals val = (NCreatureVisuals)(object)((node is NCreatureVisuals) ? node : null);
		if (val == null || !Owners.TryGetValue(val, out Binding value) || !value.Owner.TryGetTarget(out NCreature target) || !GodotObject.IsInstanceValid((GodotObject)(object)target) || !value.Wrapper.TryGetTarget(out Node target2) || !GodotObject.IsInstanceValid((GodotObject)(object)target2) || target2.GetParent() != target)
		{
			return parent;
		}
		return Resolve<Node>((Node)(object)val, parent, target2, (Node)(object)target, (Node?)(object)target.Visuals);
	}

	internal static T? Resolve<T>(T visual, T? parent, T wrapper, T owner, T? ownerVisual) where T : class
	{
		if (parent != wrapper || visual != ownerVisual)
		{
			return parent;
		}
		return owner;
	}
}

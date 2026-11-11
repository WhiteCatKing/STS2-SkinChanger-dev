using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace ModLaunchManager;

internal static class ModAssemblyPreflight
{
	[Flags]
	private enum Value
	{
		Unknown = 1,
		OwnType = 2,
		ForeignType = 4,
		OwnAssembly = 8,
		ForeignAssembly = 0x10,
		OwnContext = 0x20,
		ForeignContext = 0x40,
		ContextEqual = 0x80
	}

	private readonly record struct Call(string Owner, string Name, int Arguments, bool Instance, bool ReturnsVoid, bool Runtime);

	private sealed record Instruction(int Offset, OpCode Code, int Operand, int[] Targets);

	private sealed class State
	{
		internal readonly List<Value> Stack = new List<Value>();

		internal readonly Dictionary<int, Value> Locals = new Dictionary<int, Value>();

		internal Value Pop()
		{
			if (Stack.Count == 0)
			{
				throw new BadImageFormatException("IL stack underflow");
			}
			List<Value> stack = Stack;
			Value result = stack[stack.Count - 1];
			Stack.RemoveAt(Stack.Count - 1);
			return result;
		}

		internal State Copy()
		{
			State state = new State();
			state.Stack.AddRange(Stack);
			foreach (KeyValuePair<int, Value> local in Locals)
			{
				state.Locals[local.Key] = local.Value;
			}
			return state;
		}

		internal bool Merge(State incoming)
		{
			if (Stack.Count != incoming.Stack.Count)
			{
				throw new BadImageFormatException("inconsistent IL stack at branch merge");
			}
			bool result = false;
			for (int i = 0; i < Stack.Count; i++)
			{
				Value value = Stack[i] | incoming.Stack[i];
				if (value != Stack[i])
				{
					Stack[i] = value;
					result = true;
				}
			}
			int[] array = Locals.Keys.Union(incoming.Locals.Keys).ToArray();
			foreach (int key in array)
			{
				Value valueOrDefault = Locals.GetValueOrDefault(key, Value.Unknown);
				Value value2 = valueOrDefault | incoming.Locals.GetValueOrDefault(key, Value.Unknown);
				if (valueOrDefault != value2)
				{
					Locals[key] = value2;
					result = true;
				}
			}
			return result;
		}
	}

	private static readonly Dictionary<short, OpCode> Codes = (from field in typeof(OpCodes).GetFields(BindingFlags.Static | BindingFlags.Public)
		where field.FieldType == typeof(OpCode)
		select (OpCode)field.GetValue(null)).ToDictionary((OpCode code) => code.Value);

	private const string ContextType = "System.Runtime.Loader.AssemblyLoadContext";

	internal static ModAssemblyLoadDecision Inspect(string path)
	{
		if (!File.Exists(path))
		{
			return new ModAssemblyLoadDecision(UseGameContext: false, "no runtime file to inspect");
		}
		try
		{
			using FileStream peStream = File.OpenRead(path);
			using PEReader pEReader = new PEReader(peStream);
			if (!pEReader.HasMetadata)
			{
				return new ModAssemblyLoadDecision(UseGameContext: true, "runtime file has no managed metadata");
			}
			MetadataReader metadataReader = pEReader.GetMetadataReader();
			foreach (MethodDefinitionHandle methodDefinition2 in metadataReader.MethodDefinitions)
			{
				MethodDefinition methodDefinition = metadataReader.GetMethodDefinition(methodDefinition2);
				if (methodDefinition.RelativeVirtualAddress == 0)
				{
					continue;
				}
				MethodBodyBlock methodBody = pEReader.GetMethodBody(methodDefinition.RelativeVirtualAddress);
				List<Instruction> list = Decode(methodBody.GetILReader());
				bool flag = false;
				foreach (Instruction item in list)
				{
					if (item.Code != OpCodes.Call && item.Code != OpCodes.Callvirt && item.Code != OpCodes.Jmp)
					{
						continue;
					}
					Call call = ReadCall(metadataReader, item.Operand);
					if (!call.Runtime)
					{
						continue;
					}
					string owner = call.Owner;
					string text;
					bool flag2;
					if (!(owner == "System.AppDomain"))
					{
						if (owner == "System.Reflection.Assembly")
						{
							switch (call.Name)
							{
							case "LoadFrom":
							case "LoadFile":
							case "UnsafeLoadFrom":
								flag2 = true;
								break;
							default:
								flag2 = false;
								break;
							}
							if (flag2)
							{
								text = "loads assemblies outside its owning context";
								goto IL_01b5;
							}
						}
					}
					else
					{
						string name = call.Name;
						if ((name == "add_AssemblyResolve" || name == "add_TypeResolve") ? true : false)
						{
							text = "registers a process-wide assembly resolver";
							goto IL_01b5;
						}
					}
					text = null;
					goto IL_01b5;
					IL_01b5:
					string text2 = text;
					if (text2 != null)
					{
						return Found(text2, methodDefinition, metadataReader, item.Offset);
					}
					flag2 = flag;
					bool flag3 = call.Owner == "System.Runtime.Loader.AssemblyLoadContext";
					if (flag3)
					{
						owner = call.Name;
						bool flag4 = ((owner == "GetLoadContext" || owner == "get_Default") ? true : false);
						flag3 = flag4;
					}
					flag = flag2 || flag3;
				}
				if (flag)
				{
					(string, int)? tuple = Analyze(list, methodBody, metadataReader);
					if (tuple.HasValue)
					{
						(string, int) valueOrDefault = tuple.GetValueOrDefault();
						return Found(valueOrDefault.Item1, methodDefinition, metadataReader, valueOrDefault.Item2);
					}
				}
			}
			return new ModAssemblyLoadDecision(UseGameContext: false, "preflight found no native-loading requirement");
		}
		catch (Exception ex) when (((ex is IOException || ex is UnauthorizedAccessException || ex is BadImageFormatException || ex is ArgumentException || ex is InvalidOperationException) ? 1 : 0) != 0)
		{
			return new ModAssemblyLoadDecision(UseGameContext: true, "loading preflight could not inspect the runtime (" + ex.GetType().Name + "); preserving native loading");
		}
	}

	private static ModAssemblyLoadDecision Found(string reason, MethodDefinition method, MetadataReader reader, int offset)
	{
		return new ModAssemblyLoadDecision(UseGameContext: true, $"preflight: {reason} at {reader.GetString(method.Name)} IL_{offset:x4}");
	}

	private static (string Reason, int Offset)? Analyze(List<Instruction> code, MethodBodyBlock body, MetadataReader reader)
	{
		if (code.Count == 0)
		{
			return null;
		}
		Dictionary<int, int> positions = code.Select((Instruction instruction, int index) => (Offset: instruction.Offset, index: index)).ToDictionary(((int Offset, int index) pair) => pair.Offset, ((int Offset, int index) pair) => pair.index);
		Dictionary<int, State> states = new Dictionary<int, State>();
		Queue<int> pending = new Queue<int>();
		Enqueue(0, new State());
		ImmutableArray<ExceptionRegion>.Enumerator enumerator = body.ExceptionRegions.GetEnumerator();
		while (enumerator.MoveNext())
		{
			ExceptionRegion current = enumerator.Current;
			State state2 = new State();
			ExceptionRegionKind kind = current.Kind;
			if (kind <= ExceptionRegionKind.Filter)
			{
				state2.Stack.Add(Value.Unknown);
			}
			Enqueue(Position(current.HandlerOffset), state2);
			if (current.Kind == ExceptionRegionKind.Filter)
			{
				Enqueue(Position(current.FilterOffset), state2);
			}
		}
		int num = Math.Max(1024, code.Count * 32);
		int result;
		while (pending.TryDequeue(out result))
		{
			if (--num == 0)
			{
				return ("context analysis exceeded its bounded work budget", code[result].Offset);
			}
			Instruction instruction2 = code[result];
			OpCode code2 = instruction2.Code;
			State state3 = states[result].Copy();
			bool flag;
			if (code2 == OpCodes.Call || code2 == OpCodes.Callvirt || code2 == OpCodes.Newobj)
			{
				Call call = ReadCall(reader, instruction2.Operand);
				Value[] array = new Value[call.Arguments];
				for (int num2 = array.Length - 1; num2 >= 0; num2--)
				{
					array[num2] = state3.Pop();
				}
				Value value = ((!call.Instance || !(code2 != OpCodes.Newobj)) ? Value.Unknown : state3.Pop());
				Value item = Value.Unknown;
				if (call.Runtime)
				{
					flag = call.Owner == "System.Runtime.Loader.AssemblyLoadContext" && (value & Value.ForeignContext) != 0;
					if (flag)
					{
						bool flag2;
						switch (call.Name)
						{
						case "LoadFromAssemblyPath":
						case "LoadFromStream":
						case "LoadFromNativeImage":
						case "add_Resolving":
							flag2 = true;
							break;
						default:
							flag2 = false;
							break;
						}
						flag = flag2;
					}
					if (flag)
					{
						return ("loads modules or installs a resolver in an external context", instruction2.Offset);
					}
					string owner = call.Owner;
					string name = call.Name;
					Value value2;
					switch (owner)
					{
					case "System.Type":
						if (!(name == "GetTypeFromHandle"))
						{
							if (name == "get_Assembly")
							{
								value2 = Map(value, Value.OwnType, Value.ForeignType, Value.OwnAssembly, Value.ForeignAssembly);
								break;
							}
						}
						else if (array.Length == 1)
						{
							value2 = array[0];
							break;
						}
						goto default;
					case "System.Reflection.Assembly":
						if (!(name == "GetExecutingAssembly"))
						{
							if (!(name == "GetEntryAssembly"))
							{
								goto default;
							}
							value2 = Value.ForeignAssembly;
							break;
						}
						value2 = Value.OwnAssembly;
						break;
					case "System.Runtime.Loader.AssemblyLoadContext":
						if (!(name == "get_Default"))
						{
							if (!(name == "GetLoadContext") || array.Length != 1)
							{
								goto default;
							}
							value2 = Map(array[0], Value.OwnAssembly, Value.ForeignAssembly, Value.OwnContext, Value.ForeignContext);
							break;
						}
						value2 = Value.ForeignContext;
						break;
					case "System.Object":
						if (!(name == "ReferenceEquals") || array.Length != 2 || !DifferentContexts(array[0], array[1]))
						{
							goto default;
						}
						value2 = Value.ContextEqual;
						break;
					default:
						value2 = Value.Unknown;
						break;
					}
					item = value2;
				}
				if (code2 == OpCodes.Newobj || !call.ReturnsVoid)
				{
					state3.Stack.Add(item);
				}
			}
			else
			{
				int? num3 = LocalIndex(instruction2, load: true);
				if (num3.HasValue)
				{
					int valueOrDefault = num3.GetValueOrDefault();
					state3.Stack.Add(state3.Locals.GetValueOrDefault(valueOrDefault, Value.Unknown));
				}
				else
				{
					num3 = LocalIndex(instruction2, load: false);
					if (num3.HasValue)
					{
						int valueOrDefault2 = num3.GetValueOrDefault();
						state3.Locals[valueOrDefault2] = state3.Pop();
					}
					else if (code2 == OpCodes.Dup)
					{
						Value item2 = state3.Pop();
						state3.Stack.Add(item2);
						state3.Stack.Add(item2);
					}
					else if (code2 == OpCodes.Ldtoken)
					{
						EntityHandle entityHandle = MetadataTokens.EntityHandle(instruction2.Operand);
						state3.Stack.Add((entityHandle.Kind == HandleKind.TypeDefinition) ? Value.OwnType : ((entityHandle.Kind != HandleKind.TypeReference) ? Value.Unknown : Value.ForeignType));
					}
					else if (code2 == OpCodes.Ceq)
					{
						Value right = state3.Pop();
						Value left = state3.Pop();
						state3.Stack.Add((!DifferentContexts(left, right)) ? Value.Unknown : Value.ContextEqual);
					}
					else if (code2.FlowControl == FlowControl.Cond_Branch)
					{
						int num4 = StackCount(code2.StackBehaviourPop);
						Value value3 = ((num4 <= 0) ? Value.Unknown : state3.Pop());
						Value left2 = ((num4 <= 1) ? Value.Unknown : state3.Pop());
						bool flag3 = code2 == OpCodes.Beq || code2 == OpCodes.Beq_S;
						bool flag4 = code2 == OpCodes.Bne_Un || code2 == OpCodes.Bne_Un_S;
						bool flag5 = code2 == OpCodes.Brtrue || code2 == OpCodes.Brtrue_S;
						bool flag6 = code2 == OpCodes.Brfalse || code2 == OpCodes.Brfalse_S;
						if ((flag5 || flag6) && value3 != 0 && (value3 & ~(Value.OwnContext | Value.ForeignContext)) == 0)
						{
							Enqueue(flag5 ? Position(instruction2.Targets[0]) : (result + 1), state3);
							continue;
						}
						if (((num4 == 2 && DifferentContexts(left2, value3)) || (num4 == 1 && (value3 & Value.ContextEqual) != 0)) && instruction2.Targets.Length == 1)
						{
							int index2 = ((flag3 || flag5) ? (result + 1) : Position(instruction2.Targets[0]));
							if ((flag3 || flag4 || flag5 || flag6) && LeadsToThrow(code, index2))
							{
								return ("rejects a context different from an external assembly or the default context", instruction2.Offset);
							}
						}
					}
					else
					{
						if (code2 == OpCodes.Calli)
						{
							return ("context-sensitive code uses an indirect call that preflight cannot prove isolated", instruction2.Offset);
						}
						for (int num5 = StackCount(code2.StackBehaviourPop); num5 > 0; num5--)
						{
							state3.Pop();
						}
						for (int num6 = StackCount(code2.StackBehaviourPush); num6 > 0; num6--)
						{
							state3.Stack.Add(Value.Unknown);
						}
					}
				}
			}
			FlowControl flowControl = code2.FlowControl;
			flag = (uint)(flowControl - 7) <= 1u;
			if (!flag && !(code2 == OpCodes.Jmp))
			{
				if (code2 == OpCodes.Leave || code2 == OpCodes.Leave_S)
				{
					state3.Stack.Clear();
				}
				int[] targets = instruction2.Targets;
				for (int i = 0; i < targets.Length; i++)
				{
					Enqueue(Position(targets[i]), state3);
				}
				if (code2.FlowControl != 0)
				{
					Enqueue(result + 1, state3);
				}
			}
		}
		return null;
		void Enqueue(int index, State state)
		{
			if (index < code.Count)
			{
				if (!states.TryGetValue(index, out State value4))
				{
					states[index] = state.Copy();
					pending.Enqueue(index);
				}
				else if (value4.Merge(state))
				{
					pending.Enqueue(index);
				}
			}
		}
		int Position(int offset)
		{
			if (!positions.TryGetValue(offset, out var value5))
			{
				throw new BadImageFormatException("invalid IL branch or handler target");
			}
			return value5;
		}
	}

	private static bool DifferentContexts(Value left, Value right)
	{
		if ((left & Value.OwnContext) == 0 || (right & Value.ForeignContext) == 0)
		{
			if ((left & Value.ForeignContext) != 0)
			{
				return (right & Value.OwnContext) != 0;
			}
			return false;
		}
		return true;
	}

	private static Value Map(Value value, Value ownSource, Value foreignSource, Value ownTarget, Value foreignTarget)
	{
		Value value2 = (Value)0;
		if ((value & ownSource) != 0)
		{
			value2 |= ownTarget;
		}
		if ((value & foreignSource) != 0)
		{
			value2 |= foreignTarget;
		}
		if (value2 == (Value)0 || (value & ~(ownSource | foreignSource)) != 0)
		{
			value2 |= Value.Unknown;
		}
		return value2;
	}

	private static bool LeadsToThrow(List<Instruction> code, int index)
	{
		int num = 16;
		while (index < code.Count && num-- > 0)
		{
			OpCode code2 = code[index].Code;
			if (code2 == OpCodes.Throw || code2 == OpCodes.Rethrow)
			{
				return true;
			}
			FlowControl flowControl = code2.FlowControl;
			if ((flowControl == FlowControl.Branch || flowControl == FlowControl.Cond_Branch || flowControl == FlowControl.Return) ? true : false)
			{
				return false;
			}
			index++;
		}
		return false;
	}

	private static int? LocalIndex(Instruction instruction, bool load)
	{
		OpCode code = instruction.Code;
		short num = (load ? OpCodes.Ldloc_0.Value : OpCodes.Stloc_0.Value);
		if (code.Value >= num && code.Value <= num + 3)
		{
			return code.Value - num;
		}
		bool num2;
		if (!load)
		{
			if (!(code == OpCodes.Stloc))
			{
				num2 = code == OpCodes.Stloc_S;
				goto IL_0081;
			}
		}
		else if (!(code == OpCodes.Ldloc))
		{
			num2 = code == OpCodes.Ldloc_S;
			goto IL_0081;
		}
		goto IL_0083;
		IL_0081:
		if (!num2)
		{
			return null;
		}
		goto IL_0083;
		IL_0083:
		return instruction.Operand;
	}

	private static int StackCount(StackBehaviour behavior)
	{
		if ((behavior != 0 && behavior != StackBehaviour.Push0 && (uint)(behavior - 26) > 1u) || 1 == 0)
		{
			return behavior.ToString().Count((char character) => character == '_') + 1;
		}
		return 0;
	}

	private static Call ReadCall(MetadataReader reader, int token)
	{
		EntityHandle entityHandle = MetadataTokens.EntityHandle(token);
		if (entityHandle.Kind == HandleKind.MethodSpecification)
		{
			entityHandle = reader.GetMethodSpecification((MethodSpecificationHandle)entityHandle).Method;
		}
		EntityHandle entityHandle2;
		StringHandle name;
		BlobHandle signature;
		if (entityHandle.Kind == HandleKind.MemberReference)
		{
			MemberReference memberReference = reader.GetMemberReference((MemberReferenceHandle)entityHandle);
			entityHandle2 = memberReference.Parent;
			name = memberReference.Name;
			signature = memberReference.Signature;
		}
		else
		{
			if (entityHandle.Kind != HandleKind.MethodDefinition)
			{
				throw new BadImageFormatException("unexpected method token");
			}
			MethodDefinition methodDefinition = reader.GetMethodDefinition((MethodDefinitionHandle)entityHandle);
			entityHandle2 = methodDefinition.GetDeclaringType();
			name = methodDefinition.Name;
			signature = methodDefinition.Signature;
		}
		BlobReader blobReader = reader.GetBlobReader(signature);
		SignatureHeader signatureHeader = blobReader.ReadSignatureHeader();
		if (signatureHeader.IsGeneric)
		{
			blobReader.ReadCompressedInteger();
		}
		int num = blobReader.ReadCompressedInteger();
		if (num < 0 || num > 65535)
		{
			throw new BadImageFormatException("invalid method argument count");
		}
		SignatureTypeCode signatureTypeCode = blobReader.ReadSignatureTypeCode();
		while (signatureTypeCode - 31 <= SignatureTypeCode.Void)
		{
			blobReader.ReadTypeHandle();
			signatureTypeCode = blobReader.ReadSignatureTypeCode();
		}
		string owner = string.Empty;
		bool runtime = false;
		if (entityHandle2.Kind == HandleKind.TypeReference)
		{
			TypeReference typeReference = reader.GetTypeReference((TypeReferenceHandle)entityHandle2);
			owner = reader.GetString(typeReference.Namespace) + "." + reader.GetString(typeReference.Name);
			if (typeReference.ResolutionScope.Kind == HandleKind.AssemblyReference)
			{
				bool flag;
				switch (reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)typeReference.ResolutionScope).Name))
				{
				case "System.Private.CoreLib":
				case "System.Runtime":
				case "System.Runtime.Loader":
				case "System.Reflection":
				case "mscorlib":
				case "netstandard":
					flag = true;
					break;
				default:
					flag = false;
					break;
				}
				runtime = flag;
			}
		}
		return new Call(owner, reader.GetString(name), num, signatureHeader.IsInstance, signatureTypeCode == SignatureTypeCode.Void, runtime);
	}

	private static List<Instruction> Decode(BlobReader reader)
	{
		List<Instruction> list = new List<Instruction>();
		while (reader.RemainingBytes > 0)
		{
			int offset = reader.Offset;
			int num = reader.ReadByte();
			short key = (short)((num == 254) ? (0xFE00 | reader.ReadByte()) : num);
			if (!Codes.TryGetValue(key, out var value))
			{
				throw new BadImageFormatException("unknown IL instruction");
			}
			int num2 = 0;
			int[] array = Array.Empty<int>();
			switch (value.OperandType)
			{
			case OperandType.ShortInlineBrTarget:
				num2 = reader.ReadSByte();
				array = new int[1] { reader.Offset + num2 };
				break;
			case OperandType.InlineBrTarget:
				num2 = reader.ReadInt32();
				array = new int[1] { reader.Offset + num2 };
				break;
			case OperandType.ShortInlineI:
				num2 = reader.ReadSByte();
				break;
			case OperandType.ShortInlineVar:
				num2 = reader.ReadByte();
				break;
			case OperandType.InlineVar:
				num2 = reader.ReadUInt16();
				break;
			case OperandType.InlineI8:
				reader.ReadInt64();
				break;
			case OperandType.ShortInlineR:
				reader.ReadSingle();
				break;
			case OperandType.InlineR:
				reader.ReadDouble();
				break;
			case OperandType.InlineSwitch:
			{
				int num3 = reader.ReadInt32();
				if (num3 < 0 || num3 > reader.RemainingBytes / 4)
				{
					throw new BadImageFormatException("invalid IL switch");
				}
				int num4 = reader.Offset + num3 * 4;
				array = new int[num3];
				for (int i = 0; i < num3; i++)
				{
					array[i] = num4 + reader.ReadInt32();
				}
				break;
			}
			default:
				num2 = reader.ReadInt32();
				break;
			case OperandType.InlineNone:
				break;
			}
			list.Add(new Instruction(offset, value, num2, array));
		}
		return list;
	}
}
